// City accents, 2026-11-06. In cyberpunk and near-future city design the buildings keep ordinary concrete, glass and steel; the colour comes from small light sources on them.
// So the original buildings stay as they are and get vertical shop signs on the near mid-rise blocks and a red aviation light on the tall ones.
// Crown light lines were tried and dropped: many towers taper, and a line at the footprint edge hangs in the air.
// Fog hides towers beyond about 600 m, so lights stop there or they would float in the sky.
// Everything sits under one root, CityAccents_1106, which can be switched off or deleted without touching the city.
if (UnityEditor.EditorApplication.isPlaying) return "skip: play mode";
const string AccDir = "Assets/Art/Generated/Rooms/city/accents";
UnityEditor.AssetDatabase.Refresh();
var old = UnityEngine.GameObject.Find("CityAccents_1106"); if (old != null) UnityEngine.Object.DestroyImmediate(old);
var R = new UnityEngine.GameObject("CityAccents_1106").transform;
var site = new UnityEngine.Vector3(-2306f, 0f, 590f);
System.Func<string, UnityEngine.Color> HexC = s => { UnityEngine.Color c; UnityEngine.ColorUtility.TryParseHtmlString("#" + s, out c); return c; };
System.Func<string, UnityEngine.Color, float, string, UnityEngine.Material> EMat = (name, col, power, texPath) => {
    var path = AccDir + "/M_" + name + ".mat"; var m = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(path);
    if (m == null) { m = new UnityEngine.Material(UnityEngine.Shader.Find("Universal Render Pipeline/Lit")); UnityEditor.AssetDatabase.CreateAsset(m, path); }
    m.SetFloat("_Smoothness", 0.3f); m.EnableKeyword("_EMISSION"); m.globalIlluminationFlags = UnityEngine.MaterialGlobalIlluminationFlags.None;
    if (texPath != null) { var t = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(texPath); m.SetTexture("_BaseMap", t); m.SetTexture("_EmissionMap", t); m.SetColor("_BaseColor", new UnityEngine.Color(0.3f, 0.3f, 0.3f)); m.SetColor("_EmissionColor", UnityEngine.Color.white * power); }
    else { m.SetColor("_BaseColor", col * 0.3f); m.SetColor("_EmissionColor", col * power); }
    UnityEditor.EditorUtility.SetDirty(m); return m;
};
var red = EMat("Aviation", HexC("FF2A1A"), 4f, null);
var crowns = new[] { EMat("Crown_Cool", HexC("DDEBFF"), 2.2f, null), EMat("Crown_Cyan", HexC("7FE6FF"), 2.0f, null), EMat("Crown_Warm", HexC("FFE2B8"), 2.0f, null) };
var words = new[] { "HOTEL", "CAFE", "NOODLES", "CLINIC", "BOOKS", "24H", "RAMEN", "PHARMA" };
var signMats = new UnityEngine.Material[words.Length]; for (int i = 0; i < words.Length; i++) signMats[i] = EMat("Sign_" + words[i], UnityEngine.Color.white, 1.6f, AccDir + "/T_Sign_" + words[i] + ".png");
var cube = UnityEngine.Resources.GetBuiltinResource<UnityEngine.Mesh>("Cube.fbx");
System.Func<string, UnityEngine.Vector3, UnityEngine.Quaternion, UnityEngine.Vector3, UnityEngine.Material, UnityEngine.GameObject> Block = (name, pos, rot, size, mat) => {
    var g = new UnityEngine.GameObject(name); g.transform.SetParent(R, false); g.transform.position = pos; g.transform.rotation = rot; g.transform.localScale = size;
    g.AddComponent<UnityEngine.MeshFilter>().sharedMesh = cube; var r = g.AddComponent<UnityEngine.MeshRenderer>(); r.sharedMaterial = mat;
    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveShadows = false; r.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off; r.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
    return g;
};
var slots = new System.Collections.Generic.List<UnityEngine.Transform>();
foreach (UnityEngine.Transform c in UnityEngine.GameObject.Find("CityBuildings_Manual").transform) slots.Add(c);
foreach (UnityEngine.Transform band in UnityEngine.GameObject.Find("CityBuildings_Generated").transform) foreach (UnityEngine.Transform s in band) slots.Add(s);
var signCand = new System.Collections.Generic.List<object[]>(); int towers = 0, crownN = 0, signs = 0;
foreach (var slot in slots) {
    if (slot.GetComponentInChildren<CityBillboardContentController>(true) != null) continue;
    var rs = slot.GetComponentsInChildren<UnityEngine.MeshRenderer>(false); if (rs.Length == 0) continue;
    // building prefabs carry an import rotation, so measure in a level frame turned to the building's horizontal axes
    var axes = new[] { slot.right, slot.up, slot.forward }; int vi = 0; for (int k = 1; k < 3; k++) if (UnityEngine.Mathf.Abs(axes[k].y) > UnityEngine.Mathf.Abs(axes[vi].y)) vi = k;
    var hA = axes[(vi + 1) % 3]; hA.y = 0; hA.Normalize(); var rotF = UnityEngine.Quaternion.LookRotation(hA, UnityEngine.Vector3.up);
    var M = UnityEngine.Matrix4x4.TRS(slot.position, rotF, UnityEngine.Vector3.one).inverse;
    bool init = false; var lb = new UnityEngine.Bounds();
    foreach (var r in rs) { if (!r.enabled) continue; var mf = r.GetComponent<UnityEngine.MeshFilter>(); if (mf == null || mf.sharedMesh == null) continue; var mb = mf.sharedMesh.bounds; var m4 = M * r.transform.localToWorldMatrix;
        for (int k = 0; k < 8; k++) { var p = m4.MultiplyPoint3x4(mb.center + UnityEngine.Vector3.Scale(mb.extents, new UnityEngine.Vector3((k & 1) == 0 ? -1 : 1, (k & 2) == 0 ? -1 : 1, (k & 4) == 0 ? -1 : 1))); if (!init) { lb = new UnityEngine.Bounds(p, UnityEngine.Vector3.zero); init = true; } else lb.Encapsulate(p); } }
    if (!init) continue;
    var ex = lb.extents; var cw = slot.position + rotF * lb.center; float H = ex.y * 2f, baseY = cw.y - ex.y, top = cw.y + ex.y;
    var right = rotF * UnityEngine.Vector3.right; var fwd = rotF * UnityEngine.Vector3.forward;
    float dist = new UnityEngine.Vector2(cw.x - site.x, cw.z - site.z).magnitude;
    int hash = 17; foreach (var ch in slot.parent.name + "/" + slot.name) hash = hash * 31 + ch; hash = System.Math.Abs(hash);
    var rot = UnityEngine.Quaternion.LookRotation(fwd, UnityEngine.Vector3.up);
    if (H >= 110f && dist < 600f) {
        towers++;
        // one light on the highest point: towers taper, so corners of the footprint can be out in the air
        var pa = cw; pa.y = top + 0.7f; Block("Aviation", pa, rot, UnityEngine.Vector3.one * 1.6f, red);
        if (false) {
            crownN++; var cm = crowns[hash % 3]; float y = top - 1.2f;
            Block("Crown_N", cw + fwd * (ex.z + 0.35f) + UnityEngine.Vector3.up * (y - cw.y), rot, new UnityEngine.Vector3(ex.x * 2f + 0.7f, 0.6f, 0.5f), cm);
            Block("Crown_S", cw - fwd * (ex.z + 0.35f) + UnityEngine.Vector3.up * (y - cw.y), rot, new UnityEngine.Vector3(ex.x * 2f + 0.7f, 0.6f, 0.5f), cm);
            Block("Crown_E", cw + right * (ex.x + 0.35f) + UnityEngine.Vector3.up * (y - cw.y), rot, new UnityEngine.Vector3(0.5f, 0.6f, ex.z * 2f + 0.7f), cm);
            Block("Crown_W", cw - right * (ex.x + 0.35f) + UnityEngine.Vector3.up * (y - cw.y), rot, new UnityEngine.Vector3(0.5f, 0.6f, ex.z * 2f + 0.7f), cm);
        }
    } else if (H > 15f && dist < 420f) signCand.Add(new object[] { dist, cw, ex, right, fwd, baseY, H, hash });
}
var signLog = new System.Text.StringBuilder();
signCand.Sort((a, b) => ((float)a[0]).CompareTo((float)b[0]));
for (int i = 0; i < signCand.Count && signs < 14; i++) {
    var c = signCand[i]; var cw = (UnityEngine.Vector3)c[1]; var ex = (UnityEngine.Vector3)c[2]; var right = (UnityEngine.Vector3)c[3]; var fwd = (UnityEngine.Vector3)c[4]; float baseY = (float)c[5], H = (float)c[6]; int hash = (int)c[7];
    var toSite = site - cw; toSite.y = 0; toSite.Normalize();
    var dirs = new[] { right, -right, fwd, -fwd }; var half = new[] { ex.x, ex.x, ex.z, ex.z }; var lat = new[] { ex.z, ex.z, ex.x, ex.x };
    int best = 0; for (int k = 1; k < 4; k++) if (UnityEngine.Vector3.Dot(dirs[k], toSite) > UnityEngine.Vector3.Dot(dirs[best], toSite)) best = k;
    var n = dirs[best]; var t = UnityEngine.Vector3.Cross(UnityEngine.Vector3.up, n);
    float h = UnityEngine.Mathf.Clamp(H * 0.28f, 7f, 15f); float side = (hash % 2 == 0 ? 1f : -1f) * lat[best] * 0.7f;
    var p = cw + n * (half[best] + 0.3f) + t * side; p.y = baseY + 5.5f + h * 0.5f;
    Block("Sign_" + words[hash % words.Length], p, UnityEngine.Quaternion.LookRotation(n, UnityEngine.Vector3.up), new UnityEngine.Vector3(1.8f, h, 0.25f), signMats[hash % words.Length]);
    var fl = UnityEngine.Quaternion.Euler(0, 21.4f, 0) * (p - new UnityEngine.Vector3(-2312f, 0f, 595.6f)); signLog.Append(words[hash % words.Length] + "@" + fl.x.ToString("0") + "," + fl.z.ToString("0") + "," + p.y.ToString("0") + " ");
    signs++;
}
UnityEditor.AssetDatabase.SaveAssets();
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene(); UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene); UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
return "towers=" + towers + " signs=" + signs + " objects=" + R.childCount + " | " + signLog;
