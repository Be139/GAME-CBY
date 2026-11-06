// Read-only: how far each shell mesh's vertex normals bend away from the flat faces they belong to. Bent normals on big flat walls give triangle-shaped light.
var sb = new System.Text.StringBuilder(); var seen = new System.Collections.Generic.HashSet<UnityEngine.Mesh>();
foreach (var r in UnityEngine.Object.FindObjectsOfType<UnityEngine.MeshRenderer>()) {
    if (!r.enabled || r.transform.parent == null || r.transform.parent.name != "Shell") continue;
    var mf = r.GetComponent<UnityEngine.MeshFilter>(); if (mf == null || mf.sharedMesh == null || !mf.sharedMesh.isReadable || !seen.Add(mf.sharedMesh)) continue;
    var m = mf.sharedMesh; var v = m.vertices; var n = m.normals; var t = m.triangles; if (n.Length == 0) continue;
    int bent = 0, corners = 0; float worst = 0;
    for (int i = 0; i < t.Length; i += 3) {
        var fn = UnityEngine.Vector3.Cross(v[t[i + 1]] - v[t[i]], v[t[i + 2]] - v[t[i]]); if (fn.sqrMagnitude < 1e-10f) continue; fn.Normalize();
        for (int k = 0; k < 3; k++) { corners++; float a = UnityEngine.Vector3.Angle(fn, n[t[i + k]]); if (a > 5f) bent++; if (a > worst) worst = a; }
    }
    if (bent > 0) sb.Append(r.name + " bent=" + bent + "/" + corners + " worst=" + worst.ToString("0") + " | ");
}
return sb.Length == 0 ? "no bent normals" : sb.ToString();
