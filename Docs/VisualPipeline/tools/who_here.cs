// Read-only: every visible renderer whose bounds contain a point on the wall, plus that renderer's local lightmap density at the point (UV2 area of the nearest triangle against its world area).
var sb = new System.Text.StringBuilder();
float ca = UnityEngine.Mathf.Cos(21.4f * UnityEngine.Mathf.Deg2Rad), sa = UnityEngine.Mathf.Sin(21.4f * UnityEngine.Mathf.Deg2Rad);
System.Func<float, float, float, float, UnityEngine.Vector3> W = (u, y, v, fy) => new UnityEngine.Vector3(-2312f + ca * u - sa * v, fy + y, 595.6f + sa * u + ca * v);
var maps = UnityEngine.LightmapSettings.lightmaps;
foreach (var probe in new[] { new[] { -6.75f, 1.5f, 0f, 111.68f }, new[] { -6.75f, 2.2f, -1.5f, 111.68f }, new[] { -9.45f, 1.5f, -4f, 111.68f } }) {
    var P = W(probe[0], probe[1], probe[2], probe[3]); sb.AppendLine("point u=" + probe[0] + " y=" + probe[1] + " v=" + probe[2]);
    foreach (var r in UnityEngine.Object.FindObjectsOfType<UnityEngine.MeshRenderer>()) {
        if (!r.enabled || !r.gameObject.activeInHierarchy) continue; var b = r.bounds; b.Expand(0.3f); if (!b.Contains(P)) continue;
        var mf = r.GetComponent<UnityEngine.MeshFilter>(); string dens = "?"; float best = 99f;
        if (mf != null && mf.sharedMesh != null && mf.sharedMesh.isReadable && r.lightmapIndex >= 0 && r.lightmapIndex < maps.Length) {
            var m = mf.sharedMesh; var v = m.vertices; var uv2 = m.uv2.Length > 0 ? m.uv2 : m.uv; var t = m.triangles; var mx = r.transform.localToWorldMatrix; var tex = maps[r.lightmapIndex].lightmapColor; var so = r.lightmapScaleOffset;
            for (int i = 0; i < t.Length; i += 3) {
                var a = mx.MultiplyPoint3x4(v[t[i]]); var bb = mx.MultiplyPoint3x4(v[t[i + 1]]); var c = mx.MultiplyPoint3x4(v[t[i + 2]]);
                var n = UnityEngine.Vector3.Cross(bb - a, c - a); float wa = n.magnitude * 0.5f; if (wa < 1e-4f) continue; n.Normalize();
                float dist = UnityEngine.Mathf.Abs(UnityEngine.Vector3.Dot(P - a, n)); if (dist > 0.05f) continue;
                // barycentric inside test
                var v0 = bb - a; var v1 = c - a; var v2 = P - a; float d00 = UnityEngine.Vector3.Dot(v0, v0), d01 = UnityEngine.Vector3.Dot(v0, v1), d11 = UnityEngine.Vector3.Dot(v1, v1), d20 = UnityEngine.Vector3.Dot(v2, v0), d21 = UnityEngine.Vector3.Dot(v2, v1); float den = d00 * d11 - d01 * d01; if (UnityEngine.Mathf.Abs(den) < 1e-9f) continue;
                float bv = (d11 * d20 - d01 * d21) / den, bw = (d00 * d21 - d01 * d20) / den; if (bv < -0.01f || bw < -0.01f || bv + bw > 1.01f) continue;
                var ua = uv2[t[i]]; var ub = uv2[t[i + 1]]; var uc = uv2[t[i + 2]]; float uvA = UnityEngine.Mathf.Abs((ub.x - ua.x) * (uc.y - ua.y) - (uc.x - ua.x) * (ub.y - ua.y)) * 0.5f;
                float tx = uvA * so.x * tex.width * so.y * tex.height; float d = UnityEngine.Mathf.Sqrt(tx / wa); if (dist < best) { best = dist; dens = d.ToString("0.0") + "t/m off" + (dist * 1000f).ToString("0") + "mm"; }
            }
        }
        var p2 = r.transform; var path = p2.name; if (p2.parent != null) path = p2.parent.name + "/" + path; if (p2.parent != null && p2.parent.parent != null) path = p2.parent.parent.name + "/" + path;
        if (dens != "?" || b.size.magnitude < 60f) sb.AppendLine("  " + path + " [" + (r.sharedMaterial ? r.sharedMaterial.name : "-") + "] cast=" + r.shadowCastingMode + " lm=" + r.lightmapIndex + " scale=" + r.scaleInLightmap.ToString("0.00") + " plane=" + dens);
    }
}
return sb.ToString();
