// Read-only: along view rays from a camera, every surface (visible or shadow-only, GI or not) lying within 6 cm of the first one hit. Finds thin layers stacked on a wall.
var sb = new System.Text.StringBuilder();
float ca = UnityEngine.Mathf.Cos(21.4f * UnityEngine.Mathf.Deg2Rad), sa = UnityEngine.Mathf.Sin(21.4f * UnityEngine.Mathf.Deg2Rad);
System.Func<float, float, float, UnityEngine.Vector3> W = (u, y, v) => new UnityEngine.Vector3(-2312f + ca * u - sa * v, 111.68f + y, 595.6f + sa * u + ca * v);
var camGo = new UnityEngine.GameObject("__pick"); camGo.hideFlags = UnityEngine.HideFlags.HideAndDontSave; var cam = camGo.AddComponent<UnityEngine.Camera>(); cam.fieldOfView = 70f; cam.aspect = 16f / 9f;
camGo.transform.position = W(CAM); camGo.transform.LookAt(W(LOOK));
var temp = new System.Collections.Generic.List<UnityEngine.MeshCollider>();
foreach (var r in UnityEngine.Object.FindObjectsOfType<UnityEngine.MeshRenderer>(true)) {
    if ((r.bounds.ClosestPoint(camGo.transform.position) - camGo.transform.position).magnitude > 15f) continue;
    var mf = r.GetComponent<UnityEngine.MeshFilter>(); if (mf == null || mf.sharedMesh == null) continue;
    var c = r.gameObject.AddComponent<UnityEngine.MeshCollider>(); c.sharedMesh = mf.sharedMesh; temp.Add(c);
}
UnityEngine.Physics.SyncTransforms();
var found = new System.Collections.Generic.Dictionary<string, int>();
for (int yi = 0; yi < 6; yi++) for (int xi = 0; xi < 6; xi++) {
    var ray = cam.ViewportPointToRay(new UnityEngine.Vector3(0.05f + xi * 0.08f, 0.35f + yi * 0.1f, 0));
    var hits = new System.Collections.Generic.List<UnityEngine.RaycastHit>();
    foreach (var c in temp) { UnityEngine.RaycastHit h; if (c.Raycast(ray, out h, 30f)) hits.Add(h); }
    if (hits.Count == 0) continue; hits.Sort((a, b) => a.distance.CompareTo(b.distance));
    // the surfaces within 6 cm along the wall normal of the first hit
    var first = hits[0];
    foreach (var h in hits) {
        float gap = UnityEngine.Mathf.Abs(UnityEngine.Vector3.Dot(h.point - first.point, first.normal)); if (gap > 0.06f) continue;
        var r = h.collider.GetComponent<UnityEngine.MeshRenderer>(); var f = UnityEditor.GameObjectUtility.GetStaticEditorFlags(r.gameObject);
        var p = r.transform; var path = p.name; if (p.parent != null) path = p.parent.name + "/" + path; if (p.parent != null && p.parent.parent != null) path = p.parent.parent.name + "/" + path;
        var key = path + " en=" + r.enabled + " cast=" + r.shadowCastingMode + " gi=" + ((f & UnityEditor.StaticEditorFlags.ContributeGI) != 0) + " gap=" + (gap * 1000f).ToString("0") + "mm";
        int n; found.TryGetValue(key, out n); found[key] = n + 1;
    }
}
foreach (var c in temp) UnityEngine.Object.DestroyImmediate(c); UnityEngine.Object.DestroyImmediate(camGo);
foreach (var kv in found) sb.Append(kv.Key + " x" + kv.Value + " | ");
return sb.ToString();
