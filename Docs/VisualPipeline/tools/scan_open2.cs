// Read-only: finds where the public 17F halls open to the outside by casting level rays at eye height from points inside; prints the openings in floor coordinates.
UnityEngine.Physics.SyncTransforms();
var sb = new System.Text.StringBuilder();
float ca = UnityEngine.Mathf.Cos(21.4f * UnityEngine.Mathf.Deg2Rad), sa = UnityEngine.Mathf.Sin(21.4f * UnityEngine.Mathf.Deg2Rad);
System.Func<float, float, float, UnityEngine.Vector3> W = (u, y, v) => new UnityEngine.Vector3(-2312f + ca * u - sa * v, 111.68f + y, 595.6f + sa * u + ca * v);
var pts = new[] { new[] { -14f, -20f }, new[] { -6f, -20f }, new[] { 0f, -20f }, new[] { -9.8f, 6f }, new[] { -16f, 2.5f }, new[] { -9.8f, 10f } };
foreach (var p in pts) {
    var hits = new System.Collections.Generic.List<string>(); string run = null; float runA = 0, last = 0;
    for (int i = 0; i <= 720; i++) {
        float ang = i * 0.5f; var dir = new UnityEngine.Vector3(UnityEngine.Mathf.Cos(ang * UnityEngine.Mathf.Deg2Rad), 0, UnityEngine.Mathf.Sin(ang * UnityEngine.Mathf.Deg2Rad));
        var du = dir.x; var dv = dir.z; var wd = new UnityEngine.Vector3(ca * du - sa * dv, 0, sa * du + ca * dv);
        UnityEngine.RaycastHit h; bool hit = UnityEngine.Physics.Raycast(W(p[0], 1.5f, p[1]), wd, out h, 40f, ~0, UnityEngine.QueryTriggerInteraction.Ignore);
        bool open = !hit || h.distance > 14f;
        float d = hit ? h.distance : 40f;
        if (open && run == null) { run = "o"; runA = ang; }
        if ((!open || i == 720) && run != null) { hits.Add(runA.ToString("0") + "-" + ang.ToString("0") + "deg"); run = null; }
        if (!open && i % 30 == 0) hits.Add("[" + ang.ToString("0") + ":" + d.ToString("0.0") + " " + h.collider.name + "]");
    }
    sb.AppendLine("from " + p[0] + "," + p[1] + ": " + string.Join(" ", hits.ToArray()));
}
return sb.ToString();
