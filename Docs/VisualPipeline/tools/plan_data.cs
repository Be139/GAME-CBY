// Read-only: exports walls, doors, props and actors in floor coordinates for plan_draw.py. Also lists the footprint of every generated model at unit scale.
var outFile = @"D:/UGit/GAME-CBY/Docs/VisualPipeline/tools/_work/plan_data.txt";
var sb = new System.Text.StringBuilder();
var org = new UnityEngine.GameObject("__Frame"); org.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
org.transform.position = new UnityEngine.Vector3(-2312.0f, 111.68f, 595.6f); org.transform.rotation = UnityEngine.Quaternion.Euler(0, -21.4f, 0);
var inv = org.transform.worldToLocalMatrix;
var ci = System.Globalization.CultureInfo.InvariantCulture;
System.Func<float, string> F = v => v.ToString("0.00", ci);
System.Func<UnityEngine.Transform, System.Nullable<UnityEngine.Bounds>> obb = t => {
    bool any = false; var b = new UnityEngine.Bounds();
    foreach (var mf in t.GetComponentsInChildren<UnityEngine.MeshFilter>(false)) {
        if (mf.sharedMesh == null) continue; var mb = mf.sharedMesh.bounds;
        for (int i = 0; i < 8; i++) { var c = new UnityEngine.Vector3(i % 2 == 0 ? mb.min.x : mb.max.x, i / 2 % 2 == 0 ? mb.min.y : mb.max.y, i / 4 == 0 ? mb.min.z : mb.max.z); var p = org.transform.InverseTransformPoint(mf.transform.TransformPoint(c)); if (!any) { b = new UnityEngine.Bounds(p, UnityEngine.Vector3.zero); any = true; } else b.Encapsulate(p); }
    }
    if (!any) return null; return b;
};
// walls: where each old wall mesh crosses a horizontal plane 1.2 m above the floor
var shell = UnityEngine.GameObject.Find("Floor17/unity3").transform;
foreach (var name in new[] { "Mesh18", "Mesh20", "Mesh10", "Mesh58", "Mesh16", "Mesh9", "Mesh4", "Mesh54", "Mesh19", "Mesh53", "Mesh21", "Mesh60" }) {
    UnityEngine.Transform src = null; foreach (var t in shell.GetComponentsInChildren<UnityEngine.Transform>(true)) if (t.name == name) src = t;
    var m = src.GetComponent<UnityEngine.MeshFilter>().sharedMesh; var M = inv * src.localToWorldMatrix; var vs = m.vertices; var tri = m.triangles;
    for (int i = 0; i + 2 < tri.Length; i += 3) {
        var p = new[] { M.MultiplyPoint3x4(vs[tri[i]]), M.MultiplyPoint3x4(vs[tri[i + 1]]), M.MultiplyPoint3x4(vs[tri[i + 2]]) };
        var hit = new System.Collections.Generic.List<UnityEngine.Vector3>();
        for (int k = 0; k < 3; k++) { var a = p[k]; var b = p[(k + 1) % 3]; if ((a.y - 1.2f) * (b.y - 1.2f) < 0f) { float s = (1.2f - a.y) / (b.y - a.y); hit.Add(UnityEngine.Vector3.Lerp(a, b, s)); } }
        if (hit.Count == 2) sb.AppendLine("W " + name + " " + F(hit[0].x) + " " + F(hit[0].z) + " " + F(hit[1].x) + " " + F(hit[1].z));
    }
}
System.Action<string, UnityEngine.Transform> emit = (kind, t) => { var b = obb(t); if (b == null) return; var bb = b.Value; float yaw = t.eulerAngles.y + 21.4f; sb.AppendLine("P " + kind + " " + t.name.Replace(" ", "_") + " " + F(bb.min.x) + " " + F(bb.max.x) + " " + F(bb.min.z) + " " + F(bb.max.z) + " " + F(bb.min.y) + " " + F(bb.max.y) + " " + F(yaw)); };
foreach (var n in new[] { "Floor17/Apartment_17F01", "Floor17/Apartment_17F02", "Floor17/Apartment_17F03", "Floor17/Apartment_17F04" }) foreach (UnityEngine.Transform t in UnityEngine.GameObject.Find(n).transform) {
    if (t.name.StartsWith("Lighting_")) continue;
    if (t.name.StartsWith("VisualPass")) { var ps = t.Find("Props"); if (ps != null) foreach (UnityEngine.Transform c in ps) if (!c.name.Contains("_Cord") && !c.name.Contains("_Disc")) emit("new", c); continue; }
    if (t.gameObject.activeSelf) emit(t.name.StartsWith("Door_") ? "door" : "old", t);
}
foreach (UnityEngine.Transform t in UnityEngine.GameObject.Find("HouseholdActors").transform) emit("unit", t);
foreach (var g in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects()) if (g.name.StartsWith("Actor_")) { var p = org.transform.InverseTransformPoint(g.transform.position); sb.AppendLine("A " + g.name + " " + F(p.x) + " " + F(p.z) + " " + F(p.y)); }
foreach (var a in UnityEngine.Object.FindObjectsOfType<UnityEngine.Transform>(true)) { var path = a.name; if (a.parent != null && a.parent.name == "Anchors" && a.parent.parent != null && a.parent.parent.name.StartsWith("ReplayRoom")) { var p = org.transform.InverseTransformPoint(a.position); if (p.y > -5f && p.y < 5f) sb.AppendLine("N " + a.name.Replace(" ", "_") + " " + F(p.x) + " " + F(p.z)); } }
UnityEngine.Object.DestroyImmediate(org);
System.IO.File.WriteAllText(outFile, sb.ToString());
// model proportions: width (x), height (y), depth (z) when the longest side is 1
var sz = new System.Text.StringBuilder();
foreach (var dir in new[] { "Assets/Art/Generated/Rooms", "Assets/Art/Generated/NoahRoom" }) foreach (var d in UnityEditor.AssetDatabase.GetSubFolders(dir)) { var id = System.IO.Path.GetFileName(d); var go = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(d + "/" + id + ".fbx"); if (go == null) continue; var s = go.GetComponentInChildren<UnityEngine.MeshFilter>().sharedMesh.bounds.size; float k = UnityEngine.Mathf.Max(s.x, s.y, s.z); sz.Append(id + " " + (s.x / k).ToString("0.00", ci) + "x" + (s.y / k).ToString("0.00", ci) + "x" + (s.z / k).ToString("0.00", ci) + "; "); }
return sz.ToString();
