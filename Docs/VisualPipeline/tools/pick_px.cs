// Read-only: which renderer is seen at given viewport points from a camera (floor coordinates, lobby floor 0.2), using temporary mesh colliders on nearby renderers.
var sb = new System.Text.StringBuilder();
float ca = UnityEngine.Mathf.Cos(21.4f * UnityEngine.Mathf.Deg2Rad), sa = UnityEngine.Mathf.Sin(21.4f * UnityEngine.Mathf.Deg2Rad);
System.Func<float, float, float, UnityEngine.Vector3> W = (u, y, v) => new UnityEngine.Vector3(-2312f + ca * u - sa * v, FLOORY + y, 595.6f + sa * u + ca * v);
var camGo = new UnityEngine.GameObject("__pick"); camGo.hideFlags = UnityEngine.HideFlags.HideAndDontSave; var cam = camGo.AddComponent<UnityEngine.Camera>(); cam.fieldOfView = FOV; cam.aspect = 16f / 9f;
camGo.transform.position = W(CAM); camGo.transform.LookAt(W(LOOK));
var temp = new System.Collections.Generic.List<UnityEngine.MeshCollider>();
foreach (var r in UnityEngine.Object.FindObjectsOfType<UnityEngine.MeshRenderer>()) {
    if (!r.enabled || r.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly) continue; if ((r.bounds.ClosestPoint(camGo.transform.position) - camGo.transform.position).magnitude > 40f) continue;
    var mf = r.GetComponent<UnityEngine.MeshFilter>(); if (mf == null || mf.sharedMesh == null) continue;
    var c = r.gameObject.AddComponent<UnityEngine.MeshCollider>(); c.sharedMesh = mf.sharedMesh; temp.Add(c);
}
UnityEngine.Physics.SyncTransforms();
foreach (var pt in new[] { PTS }) {
    var ray = cam.ViewportPointToRay(new UnityEngine.Vector3(pt[0], pt[1], 0)); float bd = 1e9f; UnityEngine.MeshCollider bc = null; UnityEngine.RaycastHit bh = default(UnityEngine.RaycastHit);
    foreach (var c in temp) { UnityEngine.RaycastHit h; if (c.Raycast(ray, out h, 80f) && h.distance < bd) { bd = h.distance; bc = c; bh = h; } }
    if (bc == null) { sb.Append("(" + pt[0] + "," + pt[1] + ") nothing | "); continue; }
    var r = bc.GetComponent<UnityEngine.MeshRenderer>(); var p = r.transform; var path = p.name; if (p.parent != null) path = p.parent.name + "/" + path; if (p.parent != null && p.parent.parent != null) path = p.parent.parent.name + "/" + path;
    sb.Append("(" + pt[0] + "," + pt[1] + ") " + path + " [" + (r.sharedMaterial ? r.sharedMaterial.name : "-") + "] lm=" + r.lightmapIndex + " gi=" + r.receiveGI + " scale=" + r.scaleInLightmap.ToString("0.00") + " d=" + bd.ToString("0.0") + " | ");
}
foreach (var c in temp) UnityEngine.Object.DestroyImmediate(c); UnityEngine.Object.DestroyImmediate(camGo);
return sb.ToString();
