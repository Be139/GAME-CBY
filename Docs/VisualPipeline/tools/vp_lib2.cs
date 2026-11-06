// Second helper block: restyling old props that stay, stacking and lamp glow.
// clones an object's materials with a tint and a matte finish; textures stay
System.Action<string, string, float> Grade = (name, hex, smooth) => {
    var t = apt.Find(name); if (t == null) { sb.AppendLine("no object " + name); return; }
    foreach (var r in t.GetComponentsInChildren<UnityEngine.MeshRenderer>(true)) {
        var mats = r.sharedMaterials;
        for (int i = 0; i < mats.Length; i++) {
            if (mats[i] == null) continue;
            var src = mats[i]; var clean = src.name.Replace("/", "_").Replace(" ", "_"); var path = SharedDir + "/G_" + clean + "_" + hex + ".mat";
            if (src.name.StartsWith("G_")) { clean = src.name.Substring(2); int cut = clean.LastIndexOf('_'); if (cut > 0) clean = clean.Substring(0, cut); path = SharedDir + "/G_" + clean + "_" + hex + ".mat"; }
            var m = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(path);
            if (m == null) { m = new UnityEngine.Material(src); UnityEditor.AssetDatabase.CreateAsset(m, path); }
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", Hex(hex));
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0f);
            if (m.HasProperty("_MetallicGlossMap")) m.SetTexture("_MetallicGlossMap", null);
            mats[i] = m; UnityEditor.EditorUtility.SetDirty(m);
        }
        r.sharedMaterials = mats;
    }
};
// flat colour for an old prop, every slot, texture dropped
System.Action<string, string, float> Matte = (name, hex, smooth) => {
    var t = apt.Find(name); if (t == null) { sb.AppendLine("no object " + name); return; }
    var m = Mat("Flat_" + hex, hex, smooth, null);
    foreach (var r in t.GetComponentsInChildren<UnityEngine.MeshRenderer>(true)) { var mats = r.sharedMaterials; for (int i = 0; i < mats.Length; i++) mats[i] = m; r.sharedMaterials = mats; }
};
// one material on every slot of an old prop
System.Action<string, UnityEngine.Material> Apply = (name, m) => {
    var t = apt.Find(name); if (t == null) { sb.AppendLine("no object " + name); return; }
    foreach (var r in t.GetComponentsInChildren<UnityEngine.MeshRenderer>(true)) { var mats = r.sharedMaterials; for (int i = 0; i < mats.Length; i++) mats[i] = m; r.sharedMaterials = mats; }
};
// swaps a placed model's texture for a recoloured variant
System.Action<UnityEngine.GameObject, string, string> Retex = (go, id, variant) => {
    if (go == null) return;
    var r = go.GetComponentInChildren<UnityEngine.MeshRenderer>(); var path = SharedDir + "/M_" + id + "_" + variant + ".mat";
    var m = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(path);
    if (m == null) { m = new UnityEngine.Material(r.sharedMaterial); UnityEditor.AssetDatabase.CreateAsset(m, path); }
    m.SetTexture("_BaseMap", Tx(RoomsDir + "/variants/" + variant + ".png")); UnityEditor.EditorUtility.SetDirty(m); r.sharedMaterial = m;
};
// copies a building piece with all its material slots. A null slot keeps the old material.
System.Collections.Generic.Dictionary<string, UnityEngine.Transform> siteCache = null;
System.Func<string, string, UnityEngine.Material[], float, UnityEngine.GameObject> CopySlots = (srcName, name, slots, lm) => {
    if (siteCache == null) { siteCache = new System.Collections.Generic.Dictionary<string, UnityEngine.Transform>(); foreach (var t in UnityEngine.GameObject.Find("BuildingSiteGeometry").GetComponentsInChildren<UnityEngine.Transform>(true)) siteCache[t.name] = t; }
    if (!siteCache.ContainsKey(srcName)) { sb.AppendLine("no piece " + srcName); return null; }
    var src = siteCache[srcName]; var o = src.GetComponent<UnityEngine.MeshFilter>().sharedMesh; var old = src.GetComponent<UnityEngine.MeshRenderer>();
    var M = root.worldToLocalMatrix * src.localToWorldMatrix; var N = M.inverse.transpose;
    var vs = o.vertices; var ns = o.normals; var uv = new UnityEngine.Vector2[vs.Length];
    for (int i = 0; i < vs.Length; i++) { var p = M.MultiplyPoint3x4(vs[i]); vs[i] = p; uv[i] = new UnityEngine.Vector2(p.x / 3f, p.z / 3f); if (ns.Length == vs.Length) ns[i] = N.MultiplyVector(ns[i]).normalized; }
    var mesh = new UnityEngine.Mesh(); mesh.name = name; if (vs.Length > 60000) mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    mesh.vertices = vs; mesh.uv = uv; if (ns.Length == vs.Length) mesh.normals = ns; mesh.subMeshCount = o.subMeshCount;
    bool flip = M.determinant < 0f;
    for (int s = 0; s < o.subMeshCount; s++) { var tri = o.GetTriangles(s); if (flip) for (int i = 0; i + 2 < tri.Length; i += 3) { var t = tri[i]; tri[i] = tri[i + 1]; tri[i + 1] = t; } mesh.SetTriangles(tri, s); }
    if (ns.Length != vs.Length) mesh.RecalculateNormals();
    mesh.RecalculateBounds();
    UnityEditor.UnwrapParam up; UnityEditor.UnwrapParam.SetDefaults(out up); up.hardAngle = 88f; up.packMargin = 0.01f; UnityEditor.Unwrapping.GenerateSecondaryUVSet(mesh, up);
    UnityEditor.AssetDatabase.CreateAsset(mesh, SharedDir + "/meshes/" + name + ".asset");
    var g = new UnityEngine.GameObject(name); g.transform.SetParent(shellT, false); g.AddComponent<UnityEngine.MeshFilter>().sharedMesh = mesh;
    var r = g.AddComponent<UnityEngine.MeshRenderer>(); var mats = new UnityEngine.Material[o.subMeshCount];
    for (int i = 0; i < mats.Length; i++) mats[i] = (i < slots.Length && slots[i] != null) ? slots[i] : old.sharedMaterials[System.Math.Min(i, old.sharedMaterials.Length - 1)];
    r.sharedMaterials = mats; r.scaleInLightmap = lm; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
    UnityEditor.GameObjectUtility.SetStaticEditorFlags(g, GI);
    old.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
    return g;
};
System.Func<UnityEngine.GameObject, float> TopOf = g => g == null ? 0.7f : Obb(g.transform).max.y;
// lamp body that glows: a copy of the model material with emission, lit part from a mask map when one exists
System.Action<UnityEngine.GameObject, string, string, float> Lit = (go, id, hex, power) => {
    if (go == null) return;
    var r = go.GetComponentInChildren<UnityEngine.MeshRenderer>();
    var path = SharedDir + "/M_" + id + "_lit.mat";
    var m = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(path);
    if (m == null) { m = new UnityEngine.Material(r.sharedMaterial); UnityEditor.AssetDatabase.CreateAsset(m, path); }
    var dir = System.IO.Path.GetDirectoryName(ModelPath(id)).Replace("\\", "/");
    var mask = Tx(dir + "/" + id + "_emit.png"); var body = Tx(dir + "/texture_0.png");
    m.EnableKeyword("_EMISSION"); m.SetTexture("_EmissionMap", mask != null ? mask : body); m.SetColor("_EmissionColor", Hex(hex) * power);
    m.globalIlluminationFlags = UnityEngine.MaterialGlobalIlluminationFlags.None;
    UnityEditor.EditorUtility.SetDirty(m); r.sharedMaterial = m; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    // a lamp sits next to its own baked light and would burn out in the lightmap
    UnityEditor.GameObjectUtility.SetStaticEditorFlags(r.gameObject, UnityEditor.StaticEditorFlags.ReflectionProbeStatic);
};
// hanging lamp: the generated model has no cord, so a thin box runs from the lamp to the ceiling
System.Action<string, float, float, float, float, float, string, float> Hang = (name, x, y, z, h, ceiling, hex, power) => {
    Lit(Put("pendant_lamp", name, 'H', h, x, y, z, 0f, false, 0.4f, null), "pendant_lamp", hex, power);
    var c = Box(name + "_Cord", propsT, Mat("Cord", "2B2622", 0.2f, null), V(x, (y + ceiling) * 0.5f, z), V(0.012f, ceiling - y, 0.012f), 0f, 0f);
    c.GetComponent<UnityEngine.MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    // the dome is open underneath: a thin lit disc closes it, so the lamp reads from below
    var go = propsT.Find(name); var b = Obb(go); float d = UnityEngine.Mathf.Max(b.size.x, b.size.z) * 0.9f;
    var disc = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cylinder); disc.name = name + "_Disc"; UnityEngine.Object.DestroyImmediate(disc.GetComponent<UnityEngine.Collider>());
    disc.transform.SetParent(propsT, false); disc.transform.localPosition = V(x, b.min.y + 0.02f, z); disc.transform.localScale = V(d, 0.004f, d);
    var dr = disc.GetComponent<UnityEngine.MeshRenderer>(); dr.sharedMaterial = Glow("LampDisc_" + hex, hex, 1.4f); dr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
};

// light probes so characters and robots pick up the room's colour: they get no lightmap
System.Action<string, float, float, float, float, float, float[]> ProbeGrid = (name, u0, u1, v0, v1, step, ys) => {
    var g = new UnityEngine.GameObject(name); g.transform.SetParent(lightsT, false); g.transform.localPosition = UnityEngine.Vector3.zero;
    var pts = new System.Collections.Generic.List<UnityEngine.Vector3>();
    for (float u = u0; u <= u1 + 0.01f; u += step) for (float v = v0; v <= v1 + 0.01f; v += step) foreach (var y in ys) pts.Add(V(u, y, v));
    g.AddComponent<UnityEngine.LightProbeGroup>().probePositions = pts.ToArray();
};
// horizontal blinds as one mesh. x = wall plane, side +1 for a wall on the +u side, -1 for -u; a..b along v, y0..y1 in height. Tilted slats let light through in stripes.
System.Action<string, float, float, float, float, float, float, string> Blinds = (name, x, a, b, y0, y1, side, hex) => {
    var cube = UnityEngine.Resources.GetBuiltinResource<UnityEngine.Mesh>("Cube.fbx"); var list = new System.Collections.Generic.List<UnityEngine.CombineInstance>();
    float cz = (a + b) / 2f, w = b - a - 0.02f, cx = x - side * 0.05f;
    for (float y = y1 - 0.09f; y > y0; y -= 0.068f) { var ci = new UnityEngine.CombineInstance(); ci.mesh = cube; ci.transform = UnityEngine.Matrix4x4.TRS(V(cx, y, cz), UnityEngine.Quaternion.Euler(0, 0, side * 26f), V(0.07f, 0.007f, w)); list.Add(ci); }
    var rail = new UnityEngine.CombineInstance(); rail.mesh = cube; rail.transform = UnityEngine.Matrix4x4.TRS(V(cx, y1 - 0.02f, cz), UnityEngine.Quaternion.identity, V(0.055f, 0.045f, b - a)); list.Add(rail);
    var mesh = new UnityEngine.Mesh(); mesh.name = name; mesh.CombineMeshes(list.ToArray(), true, true); mesh.RecalculateBounds();
    UnityEditor.AssetDatabase.CreateAsset(mesh, SharedDir + "/meshes/" + name + ".asset");
    var g = new UnityEngine.GameObject(name); g.transform.SetParent(shellT, false); g.AddComponent<UnityEngine.MeshFilter>().sharedMesh = mesh;
    var r = g.AddComponent<UnityEngine.MeshRenderer>(); r.sharedMaterial = Mat("Blind_" + hex, hex, 0.2f, null); r.receiveGI = UnityEngine.ReceiveGI.LightProbes;
    UnityEditor.GameObjectUtility.SetStaticEditorFlags(g, UnityEditor.StaticEditorFlags.ContributeGI);
};
// one curtain panel hanging at x,z. width and height in metres, yaw so the fabric faces the room, variant = recoloured texture or null
System.Action<string, float, float, float, float, float, string> Curtain = (name, x, z, width, height, yaw, variant) => {
    var go = Put("curtain", name, 'H', height, x, 0.04f, z, yaw, false, 0.5f, null); if (go == null) return;
    var mf = go.GetComponentInChildren<UnityEngine.MeshFilter>(); var ls = go.transform.localScale; float nowW = mf.sharedMesh.bounds.size.x * ls.x;
    go.transform.localScale = new UnityEngine.Vector3(ls.x * width / nowW, ls.y, ls.z);
    var r = go.GetComponentInChildren<UnityEngine.MeshRenderer>(); var c = r.bounds.center; var target = root.TransformPoint(V(x, 0.04f, z));
    go.transform.position += new UnityEngine.Vector3(target.x - c.x, target.y - r.bounds.min.y, target.z - c.z);
    if (variant != null) Retex(go, "curtain", variant);
};
