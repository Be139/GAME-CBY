// Read-only: opening corners of each old wall piece in floor coordinates. Rectangles come straight from the wall meshes.
var sb = new System.Text.StringBuilder();
var org = new UnityEngine.GameObject("__Frame"); org.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
org.transform.position = new UnityEngine.Vector3(-2312.0f, 111.68f, 595.6f); org.transform.rotation = UnityEngine.Quaternion.Euler(0, -21.4f, 0);
var shell = UnityEngine.GameObject.Find("Floor17/unity3").transform;
foreach (var name in new[] { "Mesh18", "Mesh10", "Mesh58", "Mesh16", "Mesh9", "Mesh4", "Mesh54", "Mesh19", "Mesh21", "Mesh60", "Mesh53" }) {
    UnityEngine.Transform src = null; foreach (var t in shell.GetComponentsInChildren<UnityEngine.Transform>(true)) if (t.name == name) src = t;
    var mesh = src.GetComponent<UnityEngine.MeshFilter>().sharedMesh; var M = org.transform.worldToLocalMatrix * src.localToWorldMatrix;
    var groups = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.SortedSet<string>>();
    float minU = 99, maxU = -99, minV = 99, maxV = -99, maxY = -9;
    foreach (var v0 in mesh.vertices) {
        var p = M.MultiplyPoint3x4(v0); minU = UnityEngine.Mathf.Min(minU, p.x); maxU = UnityEngine.Mathf.Max(maxU, p.x); minV = UnityEngine.Mathf.Min(minV, p.z); maxV = UnityEngine.Mathf.Max(maxV, p.z); maxY = UnityEngine.Mathf.Max(maxY, p.y);
        if (p.y < 0.2f || p.y > 2.8f) continue;
        var key = "y" + p.y.ToString("0.0"); if (!groups.ContainsKey(key)) groups[key] = new System.Collections.Generic.SortedSet<string>();
        groups[key].Add(p.x.ToString("0.0") + "," + p.z.ToString("0.0"));
    }
    sb.AppendLine(name + " box u " + minU.ToString("0.0") + ".." + maxU.ToString("0.0") + " v " + minV.ToString("0.0") + ".." + maxV.ToString("0.0") + " top " + maxY.ToString("0.00"));
    foreach (var g in groups) sb.AppendLine("  " + g.Key + ": " + string.Join(" ", new System.Collections.Generic.List<string>(g.Value).ToArray()));
}
UnityEngine.Object.DestroyImmediate(org);
return sb.ToString();
