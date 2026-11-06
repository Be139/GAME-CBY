// Read-only: from a camera position, find which lightmapped surface is actually seen in a set of directions and how many lightmap texels per metre it gets at that exact triangle.
var sb = new System.Text.StringBuilder();
float ca = UnityEngine.Mathf.Cos(21.4f * UnityEngine.Mathf.Deg2Rad), sa = UnityEngine.Mathf.Sin(21.4f * UnityEngine.Mathf.Deg2Rad);
System.Func<float, float, float, UnityEngine.Vector3> W = (u, y, v) => new UnityEngine.Vector3(-2312f + ca * u - sa * v, 111.68f + y, 595.6f + sa * u + ca * v);
var cam = W(CAM); var look = W(LOOK); var fwd0 = (look - cam).normalized;
var maps = UnityEngine.LightmapSettings.lightmaps;
var temp = new System.Collections.Generic.List<UnityEngine.MeshCollider>();
foreach (var r in UnityEngine.Object.FindObjectsOfType<UnityEngine.MeshRenderer>()) {
    if (!r.enabled || r.lightmapIndex < 0 || r.lightmapIndex >= maps.Length) continue; if ((r.bounds.ClosestPoint(cam) - cam).magnitude > 12f) continue;
    var mf = r.GetComponent<UnityEngine.MeshFilter>(); if (mf == null || mf.sharedMesh == null || !mf.sharedMesh.isReadable) continue;
    var c = r.gameObject.AddComponent<UnityEngine.MeshCollider>(); c.sharedMesh = mf.sharedMesh; temp.Add(c);
}
UnityEngine.Physics.SyncTransforms();
var counts = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<float>>();
for (int yi = -2; yi <= 2; yi++) for (int xi = -4; xi <= 4; xi++) {
    var dir = UnityEngine.Quaternion.AngleAxis(xi * 9f, UnityEngine.Vector3.up) * fwd0; dir = UnityEngine.Quaternion.AngleAxis(yi * 6f, UnityEngine.Vector3.Cross(UnityEngine.Vector3.up, dir)) * dir;
    UnityEngine.RaycastHit best = default(UnityEngine.RaycastHit); float bd = 1e9f; UnityEngine.MeshCollider bc = null;
    foreach (var c in temp) { UnityEngine.RaycastHit h; if (c.Raycast(new UnityEngine.Ray(cam, dir), out h, 30f) && h.distance < bd) { bd = h.distance; best = h; bc = c; } }
    if (bc == null) continue;
    var r = bc.GetComponent<UnityEngine.MeshRenderer>(); var m = bc.sharedMesh; var t = m.triangles; var v = m.vertices; var uv2 = m.uv2.Length > 0 ? m.uv2 : m.uv; int ti = best.triangleIndex * 3; var mx = r.transform.localToWorldMatrix;
    var a = mx.MultiplyPoint3x4(v[t[ti]]); var b = mx.MultiplyPoint3x4(v[t[ti + 1]]); var cc = mx.MultiplyPoint3x4(v[t[ti + 2]]); float wa = UnityEngine.Vector3.Cross(b - a, cc - a).magnitude * 0.5f;
    var ua = uv2[t[ti]]; var ub = uv2[t[ti + 1]]; var uc = uv2[t[ti + 2]]; float uvA = UnityEngine.Mathf.Abs((ub.x - ua.x) * (uc.y - ua.y) - (uc.x - ua.x) * (ub.y - ua.y)) * 0.5f;
    var tex = maps[r.lightmapIndex].lightmapColor; var so = r.lightmapScaleOffset; float d = UnityEngine.Mathf.Sqrt(uvA * so.x * tex.width * so.y * tex.height / UnityEngine.Mathf.Max(wa, 1e-5f));
    var key = r.name + (m.uv2.Length > 0 ? "" : "(no uv2)"); System.Collections.Generic.List<float> l; if (!counts.TryGetValue(key, out l)) { l = new System.Collections.Generic.List<float>(); counts[key] = l; } l.Add(d);
}
foreach (var c in temp) UnityEngine.Object.DestroyImmediate(c);
foreach (var kv in counts) { kv.Value.Sort(); sb.Append(kv.Key + " hits=" + kv.Value.Count + " t/m min=" + kv.Value[0].ToString("0.0") + " med=" + kv.Value[kv.Value.Count / 2].ToString("0.0") + " max=" + kv.Value[kv.Value.Count - 1].ToString("0.0") + " | "); }
return sb.ToString();
