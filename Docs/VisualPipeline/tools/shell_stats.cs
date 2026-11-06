// Read-only: vertex and triangle counts of the shell meshes, to see which are unwelded (about three vertices per triangle) and so get one lightmap island per triangle.
var sb = new System.Text.StringBuilder(); var seen = new System.Collections.Generic.HashSet<UnityEngine.Mesh>();
foreach (var r in UnityEngine.Object.FindObjectsOfType<UnityEngine.MeshRenderer>()) {
    if (!r.enabled || r.transform.parent == null || r.transform.parent.name != "Shell") continue;
    var mf = r.GetComponent<UnityEngine.MeshFilter>(); if (mf == null || mf.sharedMesh == null || !seen.Add(mf.sharedMesh)) continue;
    var m = mf.sharedMesh; int tris = m.triangles.Length / 3; var path = UnityEditor.AssetDatabase.GetAssetPath(m);
    sb.Append(r.transform.parent.parent.name + "/" + r.name + " v=" + m.vertexCount + " t=" + tris + " ratio=" + (m.vertexCount / (float)UnityEngine.Mathf.Max(1, tris)).ToString("0.00") + " sub=" + m.subMeshCount + " uv0=" + (m.uv.Length > 0) + " " + (path.Contains("Generated") ? "gen" : path) + " | ");
}
return sb.ToString();
