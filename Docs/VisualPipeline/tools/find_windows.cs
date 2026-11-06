// Read-only: glass and window renderers on the 17F floor and in the lobby, in floor coordinates (u along the building, v across), so window light can be placed at real openings.
var sb = new System.Text.StringBuilder();
var a = -21.4f * UnityEngine.Mathf.Deg2Rad; var U = new UnityEngine.Vector3(UnityEngine.Mathf.Cos(a), 0, UnityEngine.Mathf.Sin(-a) * -1f);
System.Func<UnityEngine.Vector3, string> F = w => { var d = w - new UnityEngine.Vector3(-2312f, 0, 595.6f); float ca = UnityEngine.Mathf.Cos(21.4f * UnityEngine.Mathf.Deg2Rad), sa = UnityEngine.Mathf.Sin(21.4f * UnityEngine.Mathf.Deg2Rad); float u = d.x * ca + d.z * sa, v = -d.x * sa + d.z * ca; return u.ToString("0.0") + "," + v.ToString("0.0"); };
foreach (var r in UnityEngine.Object.FindObjectsOfType<UnityEngine.MeshRenderer>()) {
    if (!r.enabled) continue; var b = r.bounds;
    bool f17 = b.center.y > 111.5f && b.center.y < 115.5f, lob = b.center.y > 0f && b.center.y < 9f;
    if (!f17 && !lob) continue;
    string mn = r.sharedMaterial != null ? r.sharedMaterial.name : "";
    var low = (r.name + " " + mn).ToLower();
    if (!(low.Contains("glass") || low.Contains("window") || low.Contains("glazing"))) continue;
    var dc = b.center - new UnityEngine.Vector3(-2312f, 0, 595.6f); if (new UnityEngine.Vector2(dc.x, dc.z).magnitude > 60f) continue;
    var p = r.transform; var path = p.name; if (p.parent != null) path = p.parent.name + "/" + path; if (p.parent != null && p.parent.parent != null) path = p.parent.parent.name + "/" + path;
    sb.AppendLine((f17 ? "17F " : "LB ") + path + " [" + mn + "] c=" + F(b.center) + " y=" + (b.center.y - (f17 ? 111.68f : 0.2f)).ToString("0.0") + " minUV=" + F(b.min) + " maxUV=" + F(b.max) + " size=" + b.size.ToString("0.0"));
}
return sb.ToString();
