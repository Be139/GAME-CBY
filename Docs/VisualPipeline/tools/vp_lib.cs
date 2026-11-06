// Shared helpers for a room pass. vp_run.sh puts this file in front of a room spec and runs the result through Unity MCP.
// All coordinates are floor coordinates: origin (-2312, floorY, 595.6), rotated -21.4 degrees, x = u, z = v, y up from the floor.
if (UnityEditor.EditorApplication.isPlaying) return "skip: play mode";
var sb = new System.Text.StringBuilder();
const string RoomsDir = "Assets/Art/Generated/Rooms";
const string NoahDir = "Assets/Art/Generated/NoahRoom";
const string SharedDir = RoomsDir + "/shared";
var GI = UnityEditor.StaticEditorFlags.ContributeGI | UnityEditor.StaticEditorFlags.ReflectionProbeStatic;
UnityEditor.AssetDatabase.Refresh();
foreach (var pair in new[] { new[] { "Assets/Art/Generated", "Rooms" }, new[] { RoomsDir, "shared" }, new[] { SharedDir, "meshes" } })
    if (!UnityEditor.AssetDatabase.IsValidFolder(pair[0] + "/" + pair[1])) UnityEditor.AssetDatabase.CreateFolder(pair[0], pair[1]);
UnityEngine.Transform apt = null, root = null, shellT = null, propsT = null, artT = null, lightsT = null;
UnityEngine.Transform oldShell = null;
var srcCache = new System.Collections.Generic.Dictionary<string, UnityEngine.Transform>();

System.Func<string, UnityEngine.Color> Hex = s => { UnityEngine.Color c; UnityEngine.ColorUtility.TryParseHtmlString("#" + s, out c); return c; };
System.Func<string, UnityEngine.Texture2D> Tx = p => UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(p);

// flat URP material; texture is optional and is tinted by the colour
System.Func<string, string, float, string, UnityEngine.Material> Mat = (name, hex, smooth, texPath) => {
    var path = SharedDir + "/M_" + name + ".mat";
    var m = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(path);
    if (m == null) { m = new UnityEngine.Material(UnityEngine.Shader.Find("Universal Render Pipeline/Lit")); UnityEditor.AssetDatabase.CreateAsset(m, path); }
    m.SetColor("_BaseColor", Hex(hex)); m.SetFloat("_Smoothness", smooth); m.SetFloat("_Metallic", 0f); m.SetFloat("_EnvironmentReflections", 1f);
    if (texPath != null) m.SetTexture("_BaseMap", Tx(texPath));
    UnityEditor.EditorUtility.SetDirty(m);
    return m;
};

// self-lit flat material for lamp shades, screens and bright windows
System.Func<string, string, float, UnityEngine.Material> Glow = (name, hex, power) => {
    var m = Mat(name, hex, 0.2f, null);
    m.EnableKeyword("_EMISSION"); m.SetColor("_EmissionColor", Hex(hex) * power); m.globalIlluminationFlags = UnityEngine.MaterialGlobalIlluminationFlags.BakedEmissive;
    UnityEditor.EditorUtility.SetDirty(m);
    return m;
};

// starts a room: one root per space, rebuilt from nothing on every run
System.Action<string, string, float> Room = (aptPath, name, floorY) => {
    apt = UnityEngine.GameObject.Find(aptPath).transform;
    var old = apt.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
    var g = new UnityEngine.GameObject(name); g.transform.SetParent(apt, true);
    g.transform.position = new UnityEngine.Vector3(-2312.0f, floorY, 595.6f); g.transform.rotation = UnityEngine.Quaternion.Euler(0, -21.4f, 0); g.transform.localScale = UnityEngine.Vector3.one;
    var ls = g.transform.lossyScale; g.transform.localScale = new UnityEngine.Vector3(1f / ls.x, 1f / ls.y, 1f / ls.z);
    root = g.transform;
    System.Func<string, UnityEngine.Transform> grp = n => { var c = new UnityEngine.GameObject(n); c.transform.SetParent(root, false); return c.transform; };
    shellT = grp("Shell"); propsT = grp("Props"); artT = grp("WallArt"); lightsT = grp("Lights");
};

System.Func<string, UnityEngine.Transform> OldPiece = n => {
    if (oldShell == null) { oldShell = UnityEngine.GameObject.Find("Floor17/unity3").transform; foreach (var t in oldShell.GetComponentsInChildren<UnityEngine.Transform>(true)) srcCache[t.name] = t; }
    UnityEngine.Transform r; return srcCache.TryGetValue(n, out r) ? r : null;
};

// copies an old shell piece into the room with fresh lightmap UVs. The old renderer stays only as a shadow caster, its collider keeps working.
// kind: 0 walls, 1 floor (uv by metres), 2 ceiling made from a floor piece at height h
System.Func<UnityEngine.Transform, string, UnityEngine.Material, int, float, float, float, UnityEngine.GameObject> CopyFrom = (src, name, m, kind, lm, tile, h) => {
    var mf = src.GetComponent<UnityEngine.MeshFilter>(); var o = mf.sharedMesh;
    var M = root.worldToLocalMatrix * src.localToWorldMatrix;
    var vs = o.vertices; var ns = o.normals; var uv = new UnityEngine.Vector2[vs.Length]; var tri = o.triangles;
    var N = M.inverse.transpose;
    for (int i = 0; i < vs.Length; i++) {
        var p = M.MultiplyPoint3x4(vs[i]); var n = ns.Length == vs.Length ? N.MultiplyVector(ns[i]).normalized : UnityEngine.Vector3.up;
        if (kind == 1) { p.y += 0.005f; uv[i] = new UnityEngine.Vector2(p.x / tile, p.z / tile); }
        else if (kind == 2) { p.y = h; n = UnityEngine.Vector3.down; uv[i] = new UnityEngine.Vector2(p.x / tile, p.z / tile); }
        else uv[i] = new UnityEngine.Vector2((p.x + p.z) / tile, p.y / tile);
        vs[i] = p; if (ns.Length == vs.Length) ns[i] = n;
    }
    if (M.determinant < 0f) for (int i = 0; i + 2 < tri.Length; i += 3) { var t = tri[i]; tri[i] = tri[i + 1]; tri[i + 1] = t; }
    // floors face up and ceilings face down whatever the source did
    if (kind != 0) for (int i = 0; i + 2 < tri.Length; i += 3) {
        var fn = UnityEngine.Vector3.Cross(vs[tri[i + 1]] - vs[tri[i]], vs[tri[i + 2]] - vs[tri[i]]);
        if ((kind == 1 && fn.y < 0f) || (kind == 2 && fn.y > 0f)) { var t = tri[i]; tri[i] = tri[i + 1]; tri[i + 1] = t; }
    }
    var mesh = new UnityEngine.Mesh(); mesh.name = name; mesh.vertices = vs; mesh.uv = uv; mesh.triangles = tri;
    // the source normals are wrong on many faces (those walls render black); the winding is right, so normals come from it
    mesh.RecalculateNormals();
    mesh.RecalculateBounds();
    UnityEditor.UnwrapParam up; UnityEditor.UnwrapParam.SetDefaults(out up); up.hardAngle = 88f; up.packMargin = 0.02f; UnityEditor.Unwrapping.GenerateSecondaryUVSet(mesh, up);
    UnityEditor.AssetDatabase.CreateAsset(mesh, SharedDir + "/meshes/" + name + ".asset");
    var g = new UnityEngine.GameObject(name); g.transform.SetParent(shellT, false);
    g.AddComponent<UnityEngine.MeshFilter>().sharedMesh = mesh;
    var r = g.AddComponent<UnityEngine.MeshRenderer>(); r.sharedMaterial = m; r.scaleInLightmap = lm; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
    UnityEditor.GameObjectUtility.SetStaticEditorFlags(g, GI);
    return g;
};
System.Action<string, string, UnityEngine.Material, int, float, float, float> Copy = (srcName, name, m, kind, lm, tile, h) => {
    var src = OldPiece(srcName); if (src == null) { sb.AppendLine("no piece " + srcName); return; }
    CopyFrom(src, name, m, kind, lm, tile, h);
    if (kind != 2) src.GetComponent<UnityEngine.MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
};

// room-space bounds of a transform, inactive children included
System.Func<UnityEngine.Transform, UnityEngine.Bounds> Obb = t => {
    bool any = false; var b = new UnityEngine.Bounds();
    foreach (var mf in t.GetComponentsInChildren<UnityEngine.MeshFilter>(true)) {
        if (mf.sharedMesh == null) continue; var mb = mf.sharedMesh.bounds;
        for (int i = 0; i < 8; i++) {
            var c = new UnityEngine.Vector3(i % 2 == 0 ? mb.min.x : mb.max.x, i / 2 % 2 == 0 ? mb.min.y : mb.max.y, i / 4 == 0 ? mb.min.z : mb.max.z);
            var p = root.InverseTransformPoint(mf.transform.TransformPoint(c));
            if (!any) { b = new UnityEngine.Bounds(p, UnityEngine.Vector3.zero); any = true; } else b.Encapsulate(p);
        }
    }
    return b;
};

System.Func<string, string> ModelPath = id => {
    var a = RoomsDir + "/" + id + "/" + id + ".fbx"; if (UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(a) != null) return a;
    return NoahDir + "/" + id + "/" + id + ".fbx";
};

// tinted copy of a model's material: multiplies the generated texture by the colour
System.Func<string, string, UnityEngine.Material> Tinted = (id, hex) => {
    var src = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(System.IO.Path.GetDirectoryName(ModelPath(id)).Replace("\\", "/") + "/M_" + id + ".mat");
    var path = SharedDir + "/M_" + id + "_" + hex + ".mat";
    var m = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(path);
    if (m == null) { m = new UnityEngine.Material(src); UnityEditor.AssetDatabase.CreateAsset(m, path); }
    m.SetColor("_BaseColor", Hex(hex)); UnityEditor.EditorUtility.SetDirty(m);
    return m;
};

// places a model. fit: L longest side, H height, F longest footprint side. x,z = footprint centre, y = base height, yaw 0 = model front toward +z.
System.Func<string, string, char, float, float, float, float, float, bool, float, string, UnityEngine.GameObject> Put = (id, name, fit, size, x, y, z, yaw, solid, lm, tint) => {
    var srcGo = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(ModelPath(id));
    if (srcGo == null) { sb.AppendLine("missing model " + id); return null; }
    var go = (UnityEngine.GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(srcGo, propsT); go.name = name;
    var mf = go.GetComponentInChildren<UnityEngine.MeshFilter>(); var b = mf.sharedMesh.bounds;
    // sizes are read in room space: Tripo exports keep a rotation on the mesh node, so the mesh's own axes are not the object's
    var baseRot = srcGo.transform.localRotation;
    go.transform.localPosition = UnityEngine.Vector3.zero; go.transform.localRotation = baseRot; go.transform.localScale = UnityEngine.Vector3.one;
    var unit = Obb(go.transform).size;
    float refSize = fit == 'H' ? unit.y : fit == 'F' ? UnityEngine.Mathf.Max(unit.x, unit.z) : UnityEngine.Mathf.Max(unit.x, UnityEngine.Mathf.Max(unit.y, unit.z));
    float s = size / refSize;
    go.transform.localPosition = UnityEngine.Vector3.zero; go.transform.localRotation = UnityEngine.Quaternion.Euler(0, yaw, 0) * baseRot; go.transform.localScale = new UnityEngine.Vector3(s, s, s);
    var r = go.GetComponentInChildren<UnityEngine.MeshRenderer>(); var wb = r.bounds;
    var target = root.TransformPoint(new UnityEngine.Vector3(x, y, z));
    go.transform.position += new UnityEngine.Vector3(target.x - wb.center.x, target.y - wb.min.y, target.z - wb.center.z);
    r.scaleInLightmap = lm; UnityEditor.GameObjectUtility.SetStaticEditorFlags(r.gameObject, GI);
    if (solid) { var c = r.gameObject.AddComponent<UnityEngine.BoxCollider>(); c.center = b.center; c.size = b.size; }
    if (tint != null) r.sharedMaterial = Tinted(id, tint);
    return go;
};

System.Action<string[]> Hide = names => { foreach (var n in names) { var t = apt.Find(n); if (t != null) t.gameObject.SetActive(false); else sb.AppendLine("no object " + n); } };

// nearest old wall from a room point, searched along the four room axes; returns direction index 0 +x, 1 -x, 2 +z, 3 -z, or -1
System.Func<UnityEngine.Vector3, float[]> NearWall = c => {
    var dirs = new[] { UnityEngine.Vector3.right, UnityEngine.Vector3.left, UnityEngine.Vector3.forward, UnityEngine.Vector3.back };
    float best = 99f; int idx = -1;
    OldPiece("Mesh1");
    for (int i = 0; i < 4; i++) {
        var wd = root.TransformDirection(dirs[i]).normalized;
        foreach (var h in UnityEngine.Physics.RaycastAll(root.TransformPoint(new UnityEngine.Vector3(c.x, 1.0f, c.z)), wd, 6f, ~0, UnityEngine.QueryTriggerInteraction.Ignore))
            if (h.collider.transform.IsChildOf(oldShell) && h.distance < best) { best = h.distance; idx = i; }
    }
    return new[] { (float)idx, best };
};

// replaces an old object by a generated one at the same spot. yaw NaN = back to the nearest wall and pushed against it.
System.Func<string, string, char, float, float, float, string, UnityEngine.GameObject> Swap = (oldName, id, fit, size, yaw, lift, tint) => {
    var t = apt.Find(oldName); if (t == null) { sb.AppendLine("no object " + oldName); return null; }
    var b = Obb(t); t.gameObject.SetActive(false); UnityEngine.Physics.SyncTransforms();
    float want = size > 0f ? size : UnityEngine.Mathf.Max(b.size.x, b.size.z);
    bool auto = float.IsNaN(yaw); float[] w = auto ? NearWall(b.center) : null; int dir = auto ? (int)w[0] : -1;
    float useYaw = auto ? (dir == 0 ? -90f : dir == 1 ? 90f : dir == 2 ? 180f : 0f) : yaw;
    bool solid = want > 0.55f && b.min.y < 0.3f;
    var go = Put(id, "Prop_" + id + "_" + oldName, fit, want, b.center.x, b.min.y + lift, b.center.z, useYaw, solid, want > 1.2f ? 1f : 0.6f, tint);
    if (go != null && auto && dir >= 0 && w[1] < 1.6f) {
        var nb = Obb(go.transform); var d = dir == 0 ? UnityEngine.Vector3.right : dir == 1 ? UnityEngine.Vector3.left : dir == 2 ? UnityEngine.Vector3.forward : UnityEngine.Vector3.back;
        float half = dir < 2 ? nb.size.x / 2f : nb.size.z / 2f; var wallAt = b.center + d * w[1];
        var p = root.InverseTransformPoint(go.transform.position); var centreOnAxis = dir < 2 ? nb.center.x : nb.center.z; var wallOnAxis = dir < 2 ? wallAt.x : wallAt.z;
        float sign = dir == 0 || dir == 2 ? 1f : -1f; float shift = wallOnAxis - sign * (half + 0.03f) - centreOnAxis;
        if (dir < 2) p.x += shift; else p.z += shift;
        go.transform.position = root.TransformPoint(p);
    }
    return go;
};

// flat box in room space with its own material
System.Func<string, UnityEngine.Transform, UnityEngine.Material, UnityEngine.Vector3, UnityEngine.Vector3, float, float, UnityEngine.GameObject> Box = (name, parent, m, c, size, yaw, lm) => {
    var g = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cube); g.name = name; UnityEngine.Object.DestroyImmediate(g.GetComponent<UnityEngine.Collider>());
    g.transform.SetParent(parent, false); g.transform.localPosition = c; g.transform.localRotation = UnityEngine.Quaternion.Euler(0, yaw, 0); g.transform.localScale = size;
    var r = g.GetComponent<UnityEngine.MeshRenderer>(); r.sharedMaterial = m; r.scaleInLightmap = lm;
    UnityEditor.GameObjectUtility.SetStaticEditorFlags(g, GI);
    return g;
};
// rug on the floor from a picture
System.Action<string, string, float, float, float, float, float> Rug = (name, tex, x, z, w, d, yaw) => { Box(name, propsT, Mat(name, "FFFFFF", 0.05f, tex), new UnityEngine.Vector3(x, 0.012f, z), new UnityEngine.Vector3(w, 0.012f, d), yaw, 1f); };
// framed picture: x,y,z = centre of the picture on the wall, yaw = direction the picture faces
System.Action<string, string, float, float, float, float, float, float> Art = (name, tex, x, y, z, w, h, yaw) => {
    var q = UnityEngine.Quaternion.Euler(0, yaw, 0); var c = new UnityEngine.Vector3(x, y, z);
    Box(name + "_Frame", artT, Mat("ArtFrame", "3B3530", 0.2f, null), c + q * new UnityEngine.Vector3(0, 0, 0.012f), new UnityEngine.Vector3(w + 0.05f, h + 0.05f, 0.024f), yaw, 0.5f);
    Box(name, artT, Mat(name, "FFFFFF", 0.1f, tex), c + q * new UnityEngine.Vector3(0, 0, 0.026f), new UnityEngine.Vector3(w, h, 0.004f), yaw, 0.6f);
};

System.Func<string, UnityEngine.LightType, UnityEngine.LightmapBakeType, UnityEngine.Vector3, string, float, float, UnityEngine.Light> Lt = (name, type, mode, p, hex, intensity, range) => {
    var g = new UnityEngine.GameObject(name); g.transform.SetParent(lightsT, false); g.transform.localPosition = p;
    var l = g.AddComponent<UnityEngine.Light>(); l.type = type; l.lightmapBakeType = mode; l.color = Hex(hex); l.intensity = intensity; l.range = range; l.shadows = UnityEngine.LightShadows.Soft;
    return l;
};
System.Action<string, UnityEngine.Vector3, string, float, float> Pt = (name, p, hex, i, r) => { Lt(name, UnityEngine.LightType.Point, UnityEngine.LightmapBakeType.Baked, p, hex, i, r); };
System.Func<string, bool, UnityEngine.Vector3, UnityEngine.Vector3, string, float, float, float, float, UnityEngine.Light> Sp = (name, mixed, p, target, hex, i, r, outer, inner) => {
    var l = Lt(name, UnityEngine.LightType.Spot, mixed ? UnityEngine.LightmapBakeType.Mixed : UnityEngine.LightmapBakeType.Baked, p, hex, i, r);
    l.transform.LookAt(root.TransformPoint(target)); l.spotAngle = outer; l.innerSpotAngle = inner; return l;
};
// rectangular window light: faces along yaw
System.Action<string, UnityEngine.Vector3, float, float, float, string, float, float> Rect = (name, p, w, h, yaw, hex, i, r) => {
    var l = Lt(name, UnityEngine.LightType.Rectangle, UnityEngine.LightmapBakeType.Baked, p, hex, i, r);
    l.areaSize = new UnityEngine.Vector2(w, h); l.transform.localRotation = UnityEngine.Quaternion.Euler(0, yaw, 0);
};
System.Action<string, UnityEngine.Vector3, UnityEngine.Vector3, int> Probe = (name, c, size, importance) => {
    var g = new UnityEngine.GameObject(name); g.transform.SetParent(lightsT, false); g.transform.localPosition = c;
    var p = g.AddComponent<UnityEngine.ReflectionProbe>(); p.mode = UnityEngine.Rendering.ReflectionProbeMode.Baked; p.size = size; p.boxProjection = true; p.importance = importance; p.resolution = 128; p.intensity = 0.8f;
    UnityEditor.GameObjectUtility.SetStaticEditorFlags(g, UnityEditor.StaticEditorFlags.ReflectionProbeStatic);
};
System.Action<string[]> LightsOff = paths => { foreach (var p in paths) { var g = UnityEngine.GameObject.Find(p); if (g != null) g.GetComponent<UnityEngine.Light>().enabled = false; else sb.AppendLine("no light " + p); } };
System.Func<float, float, float, UnityEngine.Vector3> V = (x, y, z) => new UnityEngine.Vector3(x, y, z);
const float Auto = float.NaN;
