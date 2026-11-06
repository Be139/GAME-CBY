// Read-only: ground height at street points, floor coordinates with the lobby floor as zero.
var sb = new System.Text.StringBuilder();
var a = 21.4f * UnityEngine.Mathf.Deg2Rad; var U = new UnityEngine.Vector3(UnityEngine.Mathf.Cos(a), 0, UnityEngine.Mathf.Sin(a)); var V = new UnityEngine.Vector3(-UnityEngine.Mathf.Sin(a), 0, UnityEngine.Mathf.Cos(a));
var pts = new[] { new[] { 24f, 10f }, new[] { 30f, 10f }, new[] { 2f, -6f }, new[] { 0f, -11f }, new[] { 40f, 10f } };
var all = UnityEngine.Object.FindObjectsOfType<UnityEngine.MeshRenderer>(false);
foreach (var q in pts) {
    var pt = new UnityEngine.Vector3(-2312f, 0f, 595.6f) + U * q[0] + V * q[1];
    var found = new System.Collections.Generic.List<string>();
    foreach (var r in all) {
        if (!r.enabled) continue; var b = r.bounds; if (pt.x < b.min.x || pt.x > b.max.x || pt.z < b.min.z || pt.z > b.max.z || b.min.y > 2.5f || b.max.y < -4f) continue;
        var mf = r.GetComponent<UnityEngine.MeshFilter>(); if (mf == null || mf.sharedMesh == null) continue; var m = mf.sharedMesh; if (m.vertexCount > 40000) continue;
        var vs = m.vertices; var tri = m.triangles; var L = r.transform.localToWorldMatrix;
        for (int i = 0; i + 2 < tri.Length; i += 3) {
            var p0 = L.MultiplyPoint3x4(vs[tri[i]]); var p1 = L.MultiplyPoint3x4(vs[tri[i + 1]]); var p2 = L.MultiplyPoint3x4(vs[tri[i + 2]]);
            var n = UnityEngine.Vector3.Cross(p1 - p0, p2 - p0); if (n.sqrMagnitude < 1e-8f || UnityEngine.Mathf.Abs(n.normalized.y) < 0.8f) continue;
            float s1 = (p1.x - p0.x) * (pt.z - p0.z) - (p1.z - p0.z) * (pt.x - p0.x), s2 = (p2.x - p1.x) * (pt.z - p1.z) - (p2.z - p1.z) * (pt.x - p1.x), s3 = (p0.x - p2.x) * (pt.z - p2.z) - (p0.z - p2.z) * (pt.x - p2.x);
            if (!((s1 >= 0 && s2 >= 0 && s3 >= 0) || (s1 <= 0 && s2 <= 0 && s3 <= 0))) continue;
            float y = (p0.y + p1.y + p2.y) / 3f; if (y < -4f || y > 2.5f) continue; found.Add((y - 0.2f).ToString("0.00") + ":" + r.name + "/" + r.sharedMaterial.name);
        }
    }
    found.Sort(); sb.AppendLine(q[0] + "," + q[1] + " -> " + string.Join(" ", found.ToArray()));
}
return sb.ToString();
