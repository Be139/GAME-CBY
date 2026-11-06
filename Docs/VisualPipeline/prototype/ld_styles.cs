// Render-only preview. Uses property blocks and temporary lights, restores everything, saves no assets.
var outDir = @"D:/UGit/GAME-CBY/Docs/VisualPipeline/lookdev_raw";
System.IO.Directory.CreateDirectory(outDir);
if (UnityEditor.EditorApplication.isPlaying) return "skip: play mode";
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
var log = new System.Text.StringBuilder();
log.AppendLine("dirtyBefore=" + scene.isDirty);

// ---- saved state ----
var savedProbe = UnityEngine.RenderSettings.ambientProbe;
var savedAmbMode = UnityEngine.RenderSettings.ambientMode; var savedAmbLight = UnityEngine.RenderSettings.ambientLight; var savedSky = UnityEngine.RenderSettings.ambientSkyColor; var savedEq = UnityEngine.RenderSettings.ambientEquatorColor; var savedGnd = UnityEngine.RenderSettings.ambientGroundColor;
var savedRefl = UnityEngine.RenderSettings.reflectionIntensity;
var savedFogCol = UnityEngine.RenderSettings.fogColor;
var savedFogDen = UnityEngine.RenderSettings.fogDensity;
var sunGo = UnityEngine.GameObject.Find("DuskLighting/Dusk_Content/Sun_Dusk");
var sun = sunGo.GetComponent<UnityEngine.Light>();
var savedSunI = sun.intensity; var savedSunC = sun.color;
var disabled = new System.Collections.Generic.List<UnityEngine.Light>();
var touched = new System.Collections.Generic.List<UnityEngine.Renderer>();
var temp = new System.Collections.Generic.List<UnityEngine.GameObject>();

var camGo = new UnityEngine.GameObject("__LD_Cam"); camGo.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
var volGo = new UnityEngine.GameObject("__LD_Vol"); volGo.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
var prof = UnityEngine.ScriptableObject.CreateInstance<UnityEngine.Rendering.VolumeProfile>(); prof.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
var rt = new UnityEngine.RenderTexture(1920, 1080, 24, UnityEngine.RenderTextureFormat.ARGB32, UnityEngine.RenderTextureReadWrite.sRGB); rt.antiAliasing = 4;
var tex = new UnityEngine.Texture2D(1920, 1080, UnityEngine.TextureFormat.RGB24, false);

System.Func<float, float, float, UnityEngine.Rendering.SphericalHarmonicsL2> flatSH = (r, g, b) => { var sh = new UnityEngine.Rendering.SphericalHarmonicsL2(); sh.AddAmbientLight(new UnityEngine.Color(r, g, b)); return sh; };

var avgCache = new System.Collections.Generic.Dictionary<UnityEngine.Material, UnityEngine.Color>();
System.Func<UnityEngine.Material, UnityEngine.Color> avgOf = m => {
    UnityEngine.Color c;
    if (avgCache.TryGetValue(m, out c)) return c;
    var baseCol = m.HasProperty("_BaseColor") ? m.GetColor("_BaseColor") : UnityEngine.Color.white;
    UnityEngine.Texture t = m.HasProperty("_BaseMap") ? m.GetTexture("_BaseMap") : null;
    c = baseCol;
    if (t != null) {
        var rtt = UnityEngine.RenderTexture.GetTemporary(32, 32, 0, UnityEngine.RenderTextureFormat.ARGB32, UnityEngine.RenderTextureReadWrite.Linear);
        UnityEngine.Graphics.Blit(t, rtt);
        var prev = UnityEngine.RenderTexture.active; UnityEngine.RenderTexture.active = rtt;
        var tt = new UnityEngine.Texture2D(32, 32, UnityEngine.TextureFormat.RGBA32, false, true);
        tt.ReadPixels(new UnityEngine.Rect(0, 0, 32, 32), 0, 0); tt.Apply();
        UnityEngine.RenderTexture.active = prev;
        var px = tt.GetPixels(); float sr = 0, sg = 0, sb = 0;
        for (int i = 0; i < px.Length; i++) { sr += px[i].r; sg += px[i].g; sb += px[i].b; }
        var av = new UnityEngine.Color(sr / px.Length, sg / px.Length, sb / px.Length, 1).gamma;
        c = new UnityEngine.Color(av.r * baseCol.r, av.g * baseCol.g, av.b * baseCol.b, 1);
        UnityEngine.Object.DestroyImmediate(tt); UnityEngine.RenderTexture.ReleaseTemporary(rtt);
    }
    avgCache[m] = c; return c;
};
System.Func<UnityEngine.Material, bool> skipMat = m => m == null || !m.HasProperty("_BaseColor") || m.renderQueue >= 2500 || (m.HasProperty("_Surface") && m.GetFloat("_Surface") > 0.5f) || m.IsKeywordEnabled("_EMISSION");

// mode: 0 keep tex and matte, 1 keep tex and gloss, 2 flat, 3 flat pastel, 4 flat pale
System.Action<int, float> applyMats = (mode, smooth) => {
    var whiteT = UnityEngine.Texture2D.whiteTexture; var blackT = UnityEngine.Texture2D.blackTexture; var normT = UnityEngine.Texture2D.normalTexture;
    var roots = new[] { "Floor17", "HouseholdActors" };
    foreach (var rn in roots) {
        var root = UnityEngine.GameObject.Find(rn); if (root == null) continue;
        foreach (var r in root.GetComponentsInChildren<UnityEngine.MeshRenderer>(true)) {
            var mats = r.sharedMaterials; bool any = false;
            for (int i = 0; i < mats.Length; i++) {
                var m = mats[i]; if (skipMat(m)) continue;
                var mpb = new UnityEngine.MaterialPropertyBlock();
                if (mode == 5) { mpb.SetColor("_BaseColor", UnityEngine.Color.white); mpb.SetTexture("_BaseMap", whiteT); mpb.SetTexture("_BumpMap", normT); mpb.SetFloat("_BumpScale", 0f); }
                else if (mode >= 2) {
                    var c = avgOf(m); float g = c.r * 0.3f + c.g * 0.59f + c.b * 0.11f;
                    if (mode == 3) { c = UnityEngine.Color.Lerp(c, new UnityEngine.Color(1f, 0.92f, 0.84f), 0.28f); }
                    if (mode == 4) { c = UnityEngine.Color.Lerp(new UnityEngine.Color(g, g, g), c, 0.45f); c = UnityEngine.Color.Lerp(c, new UnityEngine.Color(0.92f, 0.90f, 0.86f), 0.58f); }
                    c.a = 1;
                    mpb.SetColor("_BaseColor", c); mpb.SetTexture("_BaseMap", whiteT);
                    mpb.SetTexture("_BumpMap", normT); mpb.SetFloat("_BumpScale", 0f); mpb.SetFloat("_OcclusionStrength", 0f);
                }
                if (mode != 1) { mpb.SetFloat("_Smoothness", smooth); mpb.SetFloat("_Metallic", 0f); mpb.SetTexture("_MetallicGlossMap", blackT); }
                r.SetPropertyBlock(mpb, i); any = true;
            }
            if (any) touched.Add(r);
        }
    }
    if (mode == 5) foreach (var sk in UnityEngine.Object.FindObjectsOfType<UnityEngine.SkinnedMeshRenderer>(false)) { var ms = sk.sharedMaterials; for (int i = 0; i < ms.Length; i++) { if (ms[i] == null || !ms[i].HasProperty("_BaseColor")) continue; var mpb = new UnityEngine.MaterialPropertyBlock(); mpb.SetColor("_BaseColor", UnityEngine.Color.white); mpb.SetTexture("_BaseMap", whiteT); sk.SetPropertyBlock(mpb, i); } touched.Add(sk); }
};
System.Action clearMats = () => {
    foreach (var r in touched) { if (r == null) continue; var n = r.sharedMaterials.Length; for (int i = 0; i < n; i++) r.SetPropertyBlock(null, i); r.SetPropertyBlock(null); }
    touched.Clear();
};
System.Func<UnityEngine.LightType, UnityEngine.Vector3, UnityEngine.Color, float, float, UnityEngine.Light> addL = (type, p, col, I, R) => {
    var g = new UnityEngine.GameObject("__LD_L"); g.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
    var l = g.AddComponent<UnityEngine.Light>(); l.type = type; l.color = col; l.intensity = I; l.range = R; g.transform.position = p; temp.Add(g); return l;
};
System.Action<UnityEngine.Light, UnityEngine.Vector3, float> aim = (l, target, ang) => { l.spotAngle = ang; l.innerSpotAngle = ang * 0.5f; l.transform.LookAt(target); l.shadows = UnityEngine.LightShadows.Soft; };
System.Action clearL = () => { foreach (var g in temp) if (g != null) UnityEngine.Object.DestroyImmediate(g); temp.Clear(); };

try
{
    foreach (var l in UnityEngine.Object.FindObjectsOfType<UnityEngine.Light>(false)) { if (l == sun || !l.enabled) continue; if (l.transform.position.y > 100f || l.type == UnityEngine.LightType.Directional) { l.enabled = false; disabled.Add(l); } }

    var cam = camGo.AddComponent<UnityEngine.Camera>();
    var acd = camGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
    acd.renderPostProcessing = true; acd.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.SubpixelMorphologicalAntiAliasing; acd.requiresDepthTexture = true; acd.volumeLayerMask = ~0;
    cam.fieldOfView = 60; cam.nearClipPlane = 0.05f; cam.farClipPlane = 3000f; cam.allowHDR = true; cam.targetTexture = rt;

    var vol = volGo.AddComponent<UnityEngine.Rendering.Volume>(); vol.isGlobal = true; vol.priority = 100; vol.weight = 1;
    var ca = prof.Add<UnityEngine.Rendering.Universal.ColorAdjustments>(true);
    var tm = prof.Add<UnityEngine.Rendering.Universal.Tonemapping>(true);
    var bl = prof.Add<UnityEngine.Rendering.Universal.Bloom>(true);
    var vg = prof.Add<UnityEngine.Rendering.Universal.Vignette>(true);
    var st = prof.Add<UnityEngine.Rendering.Universal.SplitToning>(true);
    vol.sharedProfile = prof;

    var P0 = new UnityEngine.Vector3(-2323.19f, 114.35f, 600.34f); var f = new UnityEngine.Vector3(0.365f, 0, -0.931f); var rv = new UnityEngine.Vector3(-0.931f, 0, -0.365f);
    var T1 = new UnityEngine.Vector3(-2319.48f, 113.36f, 596.31f);
    var viewN = new[] { "V1corridor", "V2living", "V3noah" };
    var viewP = new[] { new UnityEngine.Vector3(-2323.19f, 113.16f, 600.34f), new UnityEngine.Vector3(-2315.9f, 113.2f, 596.2f), new UnityEngine.Vector3(-2313.05f, 112.71f, 593.82f) };
    var viewY = new[] { 158.6f, 345f, 24.1f };
    var styleN = new[] { "S1_stylizedReal", "S2_flatTheatre", "S3_softDiorama", "S5_neonNoir", "S6_cleanFuture", "CEL_albedo", "CEL_light" };
    var warm = new UnityEngine.Color(1f, 0.81f, 0.62f); var cool = new UnityEngine.Color(0.45f, 0.62f, 1f); var mag = new UnityEngine.Color(1f, 0.18f, 0.62f); var cyan = new UnityEngine.Color(0.2f, 0.95f, 0.95f); var neutral = new UnityEngine.Color(1f, 0.96f, 0.9f);

    for (int s = 0; s < styleN.Length; s++)
    {
        float exposure = 0.3f, contrast = 8f, sat = 0f, bloomI = 0.3f, bloomT = 1f, vig = 0.2f; float refl = 0.15f; float sunI = 0.9f; int mode = 0; float smooth = 0.05f;
        var tonemap = UnityEngine.Rendering.Universal.TonemappingMode.ACES; var amb = new UnityEngine.Color(0.12f, 0.13f, 0.18f);
        var fogC = new UnityEngine.Color(0.72f, 0.52f, 0.5f); float fogD = 0.0016f; var shTone = new UnityEngine.Color(0.5f, 0.5f, 0.5f); var hiTone = new UnityEngine.Color(0.5f, 0.5f, 0.5f);
        var keyC = warm; float keyMul = 1f; var fillC = cool; var accC = cyan;
        if (s == 0) { shTone = new UnityEngine.Color(0.32f, 0.40f, 0.56f); hiTone = new UnityEngine.Color(0.70f, 0.56f, 0.42f); contrast = 12; sat = 8; }
        if (s == 1) { mode = 2; smooth = 0f; refl = 0f; tonemap = UnityEngine.Rendering.Universal.TonemappingMode.Neutral; amb = new UnityEngine.Color(0.17f, 0.16f, 0.22f); contrast = 16; sat = 18; bloomI = 0.12f; vig = 0.08f; keyMul = 1.5f; exposure = 0.2f; }
        if (s == 2) { mode = 3; smooth = 0.22f; refl = 0.05f; tonemap = UnityEngine.Rendering.Universal.TonemappingMode.Neutral; amb = new UnityEngine.Color(0.30f, 0.27f, 0.27f); contrast = -2; sat = 6; bloomI = 0.18f; vig = 0.14f; keyMul = 0.62f; exposure = 0.05f; fogC = new UnityEngine.Color(0.86f, 0.70f, 0.62f); }
        if (s == 3) { mode = 1; refl = 0.4f; amb = new UnityEngine.Color(0.05f, 0.06f, 0.11f); contrast = 16; sat = 6; bloomI = 0.7f; bloomT = 0.9f; vig = 0.28f; keyC = new UnityEngine.Color(0.62f, 0.74f, 1f); fillC = mag; accC = cyan; sunI = 0.1f; exposure = 0.4f; fogC = new UnityEngine.Color(0.12f, 0.08f, 0.2f); fogD = 0.0022f; }
        if (s == 4) { mode = 4; smooth = 0.3f; refl = 0.1f; tonemap = UnityEngine.Rendering.Universal.TonemappingMode.Neutral; amb = new UnityEngine.Color(0.46f, 0.42f, 0.39f); contrast = 0; sat = 4; bloomI = 0.2f; bloomT = 0.95f; vig = 0.04f; keyC = new UnityEngine.Color(1f, 0.93f, 0.84f); fillC = new UnityEngine.Color(1f, 0.9f, 0.82f); keyMul = 0.42f; sunI = 0.6f; exposure = 0.1f; fogC = new UnityEngine.Color(0.9f, 0.82f, 0.76f); }
        bool celAlbedo = s == 5; bool celLight = s == 6;
        if (celAlbedo) { mode = 2; smooth = 0f; refl = 0f; tonemap = UnityEngine.Rendering.Universal.TonemappingMode.None; amb = new UnityEngine.Color(0.92f, 0.92f, 0.92f); contrast = 0; sat = 0; bloomI = 0f; vig = 0f; sunI = 0f; exposure = 0f; fogD = 0f; }
        if (celLight) { mode = 5; smooth = 0f; refl = 0f; tonemap = UnityEngine.Rendering.Universal.TonemappingMode.Neutral; amb = new UnityEngine.Color(0.2f, 0.2f, 0.24f); contrast = 0; sat = 0; bloomI = 0f; vig = 0f; keyMul = 1.3f; exposure = 0f; fogD = 0f; }

        applyMats(mode, smooth);
        ca.postExposure.value = exposure; ca.contrast.value = contrast; ca.saturation.value = sat; ca.colorFilter.value = UnityEngine.Color.white; ca.hueShift.value = 0;
        tm.mode.value = tonemap; bl.intensity.value = bloomI; bl.threshold.value = bloomT; bl.scatter.value = 0.6f; vg.intensity.value = vig; vg.smoothness.value = 0.45f; vg.color.value = UnityEngine.Color.black;
        st.shadows.value = shTone; st.highlights.value = hiTone; st.balance.value = -10f;
        UnityEngine.RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat; UnityEngine.RenderSettings.ambientLight = amb; UnityEngine.RenderSettings.reflectionIntensity = refl;
        UnityEngine.RenderSettings.fogColor = fogC; UnityEngine.RenderSettings.fogDensity = fogD;

        for (int v = 0; v < viewN.Length; v++)
        {
            sun.intensity = sunI; sun.color = savedSunC;
            if (v == 0) {
                if (!celAlbedo) { for (int k = 0; k < 5; k++) { var p = P0 + f * (3f + k * 6f) - rv * 0.6f; p.y = 114.62f; var c = keyC; if (s == 3 && k % 2 == 1) c = mag; var dl = addL(UnityEngine.LightType.Spot, p, c, 9f * keyMul, 8f); aim(dl, p + new UnityEngine.Vector3(0.01f, -3f, 0f), 150f); }
                addL(UnityEngine.LightType.Point, T1 + f * 0.1f + rv * 0.45f, accC, 0.4f, 2.2f); }
            } else if (v == 1) {
                if (!celAlbedo) { var l1 = addL(UnityEngine.LightType.Spot, new UnityEngine.Vector3(-2316.8f, 114.35f, 599.4f), keyC, 12f * keyMul, 8f); aim(l1, new UnityEngine.Vector3(-2316.8f, 112.4f, 599.4f), 120f);
                addL(UnityEngine.LightType.Point, new UnityEngine.Vector3(-2319.3f, 113.2f, 597.6f), keyC, 0.7f * keyMul, 3f);
                var l3 = addL(UnityEngine.LightType.Spot, new UnityEngine.Vector3(-2311.0f, 113.6f, 601.0f), fillC, s == 3 ? 12f : 5f, 12f); aim(l3, new UnityEngine.Vector3(-2316.8f, 112.4f, 599.4f), 80f); }
            } else {
                sun.intensity = sunI * 0.05f;
                if (!celAlbedo) { var l1 = addL(UnityEngine.LightType.Spot, new UnityEngine.Vector3(-2311.25f, 113.0f, 597.1f), keyC, 5f * keyMul, 6f); aim(l1, new UnityEngine.Vector3(-2312.4f, 112.2f, 595.6f), 125f);
                addL(UnityEngine.LightType.Point, new UnityEngine.Vector3(-2311.3f, 112.95f, 597.0f), keyC, 0.28f * keyMul, 3f);
                var l2 = addL(UnityEngine.LightType.Spot, new UnityEngine.Vector3(-2308.2f, 113.7f, 595.6f), s == 3 ? cool : fillC, 16f, 12f); aim(l2, new UnityEngine.Vector3(-2312.0f, 112.3f, 596.4f), 70f);
                var l3 = addL(UnityEngine.LightType.Spot, new UnityEngine.Vector3(-2308.2f, 114.0f, 597.4f), s == 4 ? fillC : mag, s == 4 ? 3f : (s == 3 ? 8f : 2.2f), 10f); aim(l3, new UnityEngine.Vector3(-2313.0f, 113.0f, 597.4f), 60f); }
            }
            camGo.transform.position = viewP[v]; camGo.transform.eulerAngles = new UnityEngine.Vector3(0, viewY[v], 0);
            cam.Render();
            var prev = UnityEngine.RenderTexture.active; UnityEngine.RenderTexture.active = rt;
            tex.ReadPixels(new UnityEngine.Rect(0, 0, 1920, 1080), 0, 0); tex.Apply();
            UnityEngine.RenderTexture.active = prev;
            System.IO.File.WriteAllBytes(outDir + "/" + styleN[s] + "_" + viewN[v] + ".png", UnityEngine.ImageConversion.EncodeToPNG(tex));
            clearL();
        }
        clearMats();
    }
}
finally
{
    clearL(); clearMats();
    foreach (var l in disabled) if (l != null) l.enabled = true;
    sun.intensity = savedSunI; sun.color = savedSunC;
    UnityEngine.RenderSettings.ambientMode = savedAmbMode; UnityEngine.RenderSettings.ambientSkyColor = savedSky; UnityEngine.RenderSettings.ambientEquatorColor = savedEq; UnityEngine.RenderSettings.ambientGroundColor = savedGnd; UnityEngine.RenderSettings.ambientLight = savedAmbLight; UnityEngine.RenderSettings.ambientProbe = savedProbe; UnityEngine.RenderSettings.reflectionIntensity = savedRefl;
    UnityEngine.RenderSettings.fogColor = savedFogCol; UnityEngine.RenderSettings.fogDensity = savedFogDen;
    UnityEngine.Object.DestroyImmediate(camGo); UnityEngine.Object.DestroyImmediate(volGo); UnityEngine.Object.DestroyImmediate(prof);
    rt.Release(); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(tex);
}
log.AppendLine("restoredLights=" + disabled.Count + " dirtyAfter=" + scene.isDirty + " ambMode=" + UnityEngine.RenderSettings.ambientMode + " sky=" + UnityEngine.RenderSettings.ambientSkyColor + " refl=" + UnityEngine.RenderSettings.reflectionIntensity + " fog=" + UnityEngine.RenderSettings.fogDensity + " sun=" + sun.intensity);
return log.ToString();
