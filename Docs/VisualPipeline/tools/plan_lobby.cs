// Read-only: lobby and site structure cut at 1.4 m, in floor coordinates, for planning the elevator interior and the surroundings.
var outFile = @"D:/UGit/GAME-CBY/Docs/VisualPipeline/tools/_work/plan_lobby.txt";
var sb = new System.Text.StringBuilder(); var ci = System.Globalization.CultureInfo.InvariantCulture;
System.Func<float, string> F = v => v.ToString("0.00", ci);
var org = new UnityEngine.GameObject("__Frame"); org.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
org.transform.position = new UnityEngine.Vector3(-2312.0f, 0.2f, 595.6f); org.transform.rotation = UnityEngine.Quaternion.Euler(0, -21.4f, 0);
var inv = org.transform.worldToLocalMatrix; var box = new UnityEngine.Bounds(new UnityEngine.Vector3(-2306, 2, 612), new UnityEngine.Vector3(95, 8, 95));
int n = 0, cut = 0;
foreach (var r in UnityEngine.Object.FindObjectsOfType<UnityEngine.MeshRenderer>(false)) {
    if (!r.bounds.Intersects(box)) continue; var rn = r.transform.root.name; if (rn.StartsWith("City") || rn == "OutdoorProps" || rn == "HearthHudRoot") continue;
    var mf = r.GetComponent<UnityEngine.MeshFilter>(); if (mf == null || mf.sharedMesh == null || mf.sharedMesh.vertexCount > 40000) continue;
    var m = mf.sharedMesh; var M = inv * r.transform.localToWorldMatrix; var vs = m.vertices; var tri = m.triangles; n++;
    string tag = rn == "Lobby" ? (r.transform.parent != null && r.transform.parent.name == "Shell" ? "S" : "L") : "B";
    if (tag == "L") { var b = r.bounds; var c = org.transform.InverseTransformPoint(b.center); sb.AppendLine("O " + r.name.Replace(" ", "_") + " " + F(c.x) + " " + F(c.z) + " " + F(b.size.x) + " " + F(b.size.z)); continue; }
    for (int i = 0; i + 2 < tri.Length; i += 3) {
        var p = new[] { M.MultiplyPoint3x4(vs[tri[i]]), M.MultiplyPoint3x4(vs[tri[i + 1]]), M.MultiplyPoint3x4(vs[tri[i + 2]]) };
        var hit = new System.Collections.Generic.List<UnityEngine.Vector3>();
        for (int k = 0; k < 3; k++) { var a = p[k]; var b = p[(k + 1) % 3]; if ((a.y - 1.4f) * (b.y - 1.4f) < 0f) { float s = (1.4f - a.y) / (b.y - a.y); hit.Add(UnityEngine.Vector3.Lerp(a, b, s)); } }
        if (hit.Count == 2) { cut++; sb.AppendLine("W " + tag + " " + F(hit[0].x) + " " + F(hit[0].z) + " " + F(hit[1].x) + " " + F(hit[1].z)); }
    }
}
UnityEngine.Object.DestroyImmediate(org);
System.IO.File.WriteAllText(outFile, sb.ToString());
return "renderers " + n + " segments " + cut;
