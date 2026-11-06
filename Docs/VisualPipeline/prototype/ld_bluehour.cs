// Render-only preview of the chosen blue-hour look. Property blocks and temporary objects only; everything is restored and nothing is saved.
var outDir = @"D:/UGit/GAME-CBY/Docs/VisualPipeline/lookdev_raw";
if (UnityEditor.EditorApplication.isPlaying) return "skip: play mode";
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
var log = new System.Text.StringBuilder();
log.AppendLine("dirtyBefore=" + scene.isDirty);

var savedProbe = UnityEngine.RenderSettings.ambientProbe;
var savedAmbMode = UnityEngine.RenderSettings.ambientMode; var savedAmbLight = UnityEngine.RenderSettings.ambientLight; var savedSky = UnityEngine.RenderSettings.ambientSkyColor; var savedEq = UnityEngine.RenderSettings.ambientEquatorColor; var savedGnd = UnityEngine.RenderSettings.ambientGroundColor;
var savedRefl = UnityEngine.RenderSettings.reflectionIntensity; var savedFogCol = UnityEngine.RenderSettings.fogColor; var savedFogDen = UnityEngine.RenderSettings.fogDensity;
var sun = UnityEngine.GameObject.Find("DuskLighting/Dusk_Content/Sun_Dusk").GetComponent<UnityEngine.Light>();
var savedSunI = sun.intensity; var savedSunC = sun.color;
var disabled = new System.Collections.Generic.List<UnityEngine.Light>();
var touched = new System.Collections.Generic.List<UnityEngine.Renderer>();
var temp = new System.Collections.Generic.List<UnityEngine.GameObject>();
var tempObjs = new System.Collections.Generic.List<UnityEngine.Object>();

var camGo = new UnityEngine.GameObject("__LD_Cam"); camGo.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
var volGo = new UnityEngine.GameObject("__LD_Vol"); volGo.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
var prof = UnityEngine.ScriptableObject.CreateInstance<UnityEngine.Rendering.VolumeProfile>(); prof.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
var rt = new UnityEngine.RenderTexture(1920, 1080, 24, UnityEngine.RenderTextureFormat.ARGB32, UnityEngine.RenderTextureReadWrite.sRGB); rt.antiAliasing = 4;
var tex = new UnityEngine.Texture2D(1920, 1080, UnityEngine.TextureFormat.RGB24, false);

var cookie = new UnityEngine.Texture2D(256, 256, UnityEngine.TextureFormat.RGBA32, false); cookie.wrapMode = UnityEngine.TextureWrapMode.Clamp;
for (int y = 0; y < 256; y++) for (int x = 0; x < 256; x++) { bool slat = (x / 14) % 2 == 0; float a = slat ? 1f : 0.04f; cookie.SetPixel(x, y, new UnityEngine.Color(a, a, a, a)); }
cookie.Apply(); tempObjs.Add(cookie);

var unlit = new UnityEngine.Material(UnityEngine.Shader.Find("Universal Render Pipeline/Unlit")); unlit.hideFlags = UnityEngine.HideFlags.HideAndDontSave; unlit.SetColor("_BaseColor", new UnityEngine.Color(1.7f, 1.12f, 0.62f)); tempObjs.Add(unlit);

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

var wallC = new UnityEngine.Color(0.47f, 0.52f, 0.62f);
var ceilC = new UnityEngine.Color(0.50f, 0.53f, 0.60f);
var whiteT = UnityEngine.Texture2D.whiteTexture; var blackT = UnityEngine.Texture2D.blackTexture; var normT = UnityEngine.Texture2D.normalTexture;

System.Action<UnityEngine.Renderer, int, UnityEngine.Color, float> paint = (r, i, c, smooth) => {
    var mpb = new UnityEngine.MaterialPropertyBlock();
    c.a = 1;
    mpb.SetColor("_BaseColor", c); mpb.SetTexture("_BaseMap", whiteT);
    mpb.SetTexture("_BumpMap", normT); mpb.SetFloat("_BumpScale", 0f); mpb.SetFloat("_OcclusionStrength", 0f);
    mpb.SetFloat("_Smoothness", smooth); mpb.SetFloat("_Metallic", 0f); mpb.SetTexture("_MetallicGlossMap", blackT);
    r.SetPropertyBlock(mpb, i);
};

try
{
    foreach (var l in UnityEngine.Object.FindObjectsOfType<UnityEngine.Light>(false)) { if (l == sun || !l.enabled) continue; if (l.transform.position.y > 100f || l.type == UnityEngine.LightType.Directional) { l.enabled = false; disabled.Add(l); } }

    // shell: walls, floors and ceilings by shape; furniture keeps its own colour, softened
    var f17 = UnityEngine.GameObject.Find("Floor17");
    foreach (var r in f17.GetComponentsInChildren<UnityEngine.MeshRenderer>(true))
    {
        var mats = r.sharedMaterials; bool any = false;
        bool shell = r.transform.root.name == "Floor17" && (r.transform.parent != null) && (r.transform.GetComponentInParent<UnityEngine.Transform>() != null) && r.name.StartsWith("Mesh");
        for (int i = 0; i < mats.Length; i++)
        {
            var m = mats[i]; if (skipMat(m)) continue;
            var c = avgOf(m); float smooth = 0.1f;
            if (shell)
            {
                if (r.bounds.size.y > 1f) { c = wallC; smooth = 0.12f; }
                else if (r.bounds.center.y > 113.6f) { c = ceilC; smooth = 0.05f; }
                else { c = UnityEngine.Color.Lerp(c, new UnityEngine.Color(0.42f, 0.30f, 0.23f), 0.55f); smooth = 0.42f; }
            }
            else
            {
                float g = c.r * 0.3f + c.g * 0.59f + c.b * 0.11f;
                c = UnityEngine.Color.Lerp(new UnityEngine.Color(g, g, g), c, 0.72f);
                c = UnityEngine.Color.Lerp(c, new UnityEngine.Color(0.82f, 0.78f, 0.70f), 0.22f);
            }
            paint(r, i, c, smooth); any = true;
        }
        if (any) touched.Add(r);
    }
    var hu = UnityEngine.GameObject.Find("HouseholdActors");
    if (hu != null) foreach (var r in hu.GetComponentsInChildren<UnityEngine.MeshRenderer>(true)) { var mats = r.sharedMaterials; for (int i = 0; i < mats.Length; i++) { if (skipMat(mats[i])) continue; paint(r, i, new UnityEngine.Color(0.86f, 0.84f, 0.8f), 0.3f); } touched.Add(r); }

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
    ca.postExposure.value = 0.5f; ca.contrast.value = 12f; ca.saturation.value = 10f; ca.colorFilter.value = UnityEngine.Color.white; ca.hueShift.value = 0;
    tm.mode.value = UnityEngine.Rendering.Universal.TonemappingMode.ACES;
    bl.intensity.value = 0.6f; bl.threshold.value = 0.95f; bl.scatter.value = 0.65f;
    vg.intensity.value = 0.24f; vg.smoothness.value = 0.45f; vg.color.value = UnityEngine.Color.black;
    st.shadows.value = new UnityEngine.Color(0.30f, 0.38f, 0.60f); st.highlights.value = new UnityEngine.Color(0.72f, 0.56f, 0.40f); st.balance.value = -15f;

    UnityEngine.RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
    UnityEngine.RenderSettings.reflectionIntensity = 0.22f;
    UnityEngine.RenderSettings.fogColor = new UnityEngine.Color(0.30f, 0.33f, 0.52f); UnityEngine.RenderSettings.fogDensity = 0.0018f;

    System.Func<UnityEngine.LightType, UnityEngine.Vector3, UnityEngine.Color, float, float, UnityEngine.Light> addL = (type, p, col, I, R) => {
        var g = new UnityEngine.GameObject("__LD_L"); g.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
        var l = g.AddComponent<UnityEngine.Light>(); l.type = type; l.color = col; l.intensity = I; l.range = R; g.transform.position = p; temp.Add(g); return l;
    };
    System.Action<UnityEngine.Light, UnityEngine.Vector3, float> aim = (l, target, ang) => { l.spotAngle = ang; l.innerSpotAngle = ang * 0.45f; l.transform.LookAt(target); l.shadows = UnityEngine.LightShadows.Soft; };
    System.Action<UnityEngine.Vector3, UnityEngine.Vector3> glow = (p, s) => {
        var g = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Sphere); g.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
        UnityEngine.Object.DestroyImmediate(g.GetComponent<UnityEngine.Collider>());
        g.transform.position = p; g.transform.localScale = s; g.GetComponent<UnityEngine.Renderer>().sharedMaterial = unlit;
        g.GetComponent<UnityEngine.Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; temp.Add(g);
    };

    var warm = new UnityEngine.Color(1f, 0.72f, 0.46f); var moon = new UnityEngine.Color(0.40f, 0.56f, 1f); var mag = new UnityEngine.Color(1f, 0.24f, 0.66f);
    var P0 = new UnityEngine.Vector3(-2323.19f, 114.35f, 600.34f); var f = new UnityEngine.Vector3(0.365f, 0, -0.931f); var rv = new UnityEngine.Vector3(-0.931f, 0, -0.365f);
    var viewN = new[] { "BH_V3noah", "BH_V2living", "BH_V1corridor" };
    var viewP = new[] { new UnityEngine.Vector3(-2313.05f, 112.71f, 593.82f), new UnityEngine.Vector3(-2315.9f, 113.2f, 596.2f), new UnityEngine.Vector3(-2323.19f, 113.16f, 600.34f) };
    var viewY = new[] { 24.1f, 345f, 158.6f };

    for (int v = 0; v < viewN.Length; v++)
    {
        if (v == 0)
        {
            UnityEngine.RenderSettings.ambientLight = new UnityEngine.Color(0.17f, 0.21f, 0.36f); sun.intensity = 0f;
            var l1 = addL(UnityEngine.LightType.Spot, new UnityEngine.Vector3(-2308.2f, 113.75f, 595.7f), moon, 24f, 12f); aim(l1, new UnityEngine.Vector3(-2312.4f, 112.2f, 596.6f), 72f); l1.cookie = cookie;
            var l2 = addL(UnityEngine.LightType.Spot, new UnityEngine.Vector3(-2311.15f, 113.0f, 597.2f), warm, 5.5f, 6f); aim(l2, new UnityEngine.Vector3(-2312.3f, 112.25f, 595.9f), 125f);
            addL(UnityEngine.LightType.Point, new UnityEngine.Vector3(-2311.25f, 112.95f, 597.1f), warm, 0.5f, 3.2f);
            var l3 = addL(UnityEngine.LightType.Spot, new UnityEngine.Vector3(-2308.2f, 114.0f, 597.4f), mag, 1.6f, 10f); aim(l3, new UnityEngine.Vector3(-2313.0f, 113.0f, 597.4f), 60f);
            glow(new UnityEngine.Vector3(-2311.1f, 113.03f, 597.6f), new UnityEngine.Vector3(0.30f, 0.26f, 0.30f));
        }
        else if (v == 1)
        {
            UnityEngine.RenderSettings.ambientLight = new UnityEngine.Color(0.22f, 0.26f, 0.37f); sun.intensity = 0.25f;
            var l1 = addL(UnityEngine.LightType.Spot, new UnityEngine.Vector3(-2316.8f, 113.95f, 599.4f), warm, 9f, 8f); aim(l1, new UnityEngine.Vector3(-2316.8f, 112.4f, 599.4f), 135f);
            addL(UnityEngine.LightType.Point, new UnityEngine.Vector3(-2316.8f, 114.2f, 599.4f), warm, 0.6f, 3f);
            var l3 = addL(UnityEngine.LightType.Spot, new UnityEngine.Vector3(-2311.0f, 113.6f, 601.0f), moon, 7f, 12f); aim(l3, new UnityEngine.Vector3(-2316.8f, 112.4f, 599.4f), 80f);
            glow(new UnityEngine.Vector3(-2316.8f, 114.02f, 599.4f), new UnityEngine.Vector3(0.52f, 0.17f, 0.52f));
        }
        else
        {
            UnityEngine.RenderSettings.ambientLight = new UnityEngine.Color(0.18f, 0.22f, 0.36f); sun.intensity = 0f;
            for (int k = 0; k < 6; k++)
            {
                float d = 4f + k * 4.2f; float side = (k % 2 == 0) ? -1.95f : 0.8f; float inward = (k % 2 == 0) ? 0.3f : -0.3f;
                var p = P0 + f * d + rv * side; p.y = 113.45f;
                addL(UnityEngine.LightType.Point, p + rv * inward, warm, 1.1f, 4.6f);
                glow(p, new UnityEngine.Vector3(0.14f, 0.26f, 0.14f));
            }
        }
        camGo.transform.position = viewP[v]; camGo.transform.eulerAngles = new UnityEngine.Vector3(0, viewY[v], 0);
        cam.Render();
        var prev = UnityEngine.RenderTexture.active; UnityEngine.RenderTexture.active = rt;
        tex.ReadPixels(new UnityEngine.Rect(0, 0, 1920, 1080), 0, 0); tex.Apply();
        UnityEngine.RenderTexture.active = prev;
        System.IO.File.WriteAllBytes(outDir + "/" + viewN[v] + ".png", UnityEngine.ImageConversion.EncodeToPNG(tex));
        foreach (var g in temp) if (g != null) UnityEngine.Object.DestroyImmediate(g);
        temp.Clear();
    }
}
finally
{
    foreach (var g in temp) if (g != null) UnityEngine.Object.DestroyImmediate(g);
    foreach (var r in touched) { if (r == null) continue; var n = r.sharedMaterials.Length; for (int i = 0; i < n; i++) r.SetPropertyBlock(null, i); r.SetPropertyBlock(null); }
    foreach (var l in disabled) if (l != null) l.enabled = true;
    sun.intensity = savedSunI; sun.color = savedSunC;
    UnityEngine.RenderSettings.ambientMode = savedAmbMode; UnityEngine.RenderSettings.ambientSkyColor = savedSky; UnityEngine.RenderSettings.ambientEquatorColor = savedEq; UnityEngine.RenderSettings.ambientGroundColor = savedGnd; UnityEngine.RenderSettings.ambientLight = savedAmbLight; UnityEngine.RenderSettings.ambientProbe = savedProbe; UnityEngine.RenderSettings.reflectionIntensity = savedRefl;
    UnityEngine.RenderSettings.fogColor = savedFogCol; UnityEngine.RenderSettings.fogDensity = savedFogDen;
    UnityEngine.Object.DestroyImmediate(camGo); UnityEngine.Object.DestroyImmediate(volGo); UnityEngine.Object.DestroyImmediate(prof);
    foreach (var o in tempObjs) if (o != null) UnityEngine.Object.DestroyImmediate(o);
    rt.Release(); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(tex);
}
log.AppendLine("restoredLights=" + disabled.Count + " dirtyAfter=" + scene.isDirty + " ambMode=" + UnityEngine.RenderSettings.ambientMode + " sky=" + UnityEngine.RenderSettings.ambientSkyColor + " refl=" + UnityEngine.RenderSettings.reflectionIntensity + " fog=" + UnityEngine.RenderSettings.fogDensity + " sun=" + sun.intensity);
return log.ToString();
