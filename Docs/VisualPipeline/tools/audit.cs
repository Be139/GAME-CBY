// Read-only placement audit. Reports what would be an absurd placement error: props in door swing zones, floating, interpenetrating, inside walls, outside the room.
var sb = new System.Text.StringBuilder();
var org = new UnityEngine.GameObject("__Frame"); org.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
org.transform.position = new UnityEngine.Vector3(-2312.0f, 111.68f, 595.6f); org.transform.rotation = UnityEngine.Quaternion.Euler(0, -21.4f, 0);
System.Func<UnityEngine.Transform, System.Nullable<UnityEngine.Bounds>> obb = t => {
    bool any = false; var b = new UnityEngine.Bounds();
    foreach (var mf in t.GetComponentsInChildren<UnityEngine.MeshFilter>(false)) {
        if (mf.sharedMesh == null) continue; var mb = mf.sharedMesh.bounds;
        for (int i = 0; i < 8; i++) { var c = new UnityEngine.Vector3(i % 2 == 0 ? mb.min.x : mb.max.x, i / 2 % 2 == 0 ? mb.min.y : mb.max.y, i / 4 == 0 ? mb.min.z : mb.max.z); var p = org.transform.InverseTransformPoint(mf.transform.TransformPoint(c)); if (!any) { b = new UnityEngine.Bounds(p, UnityEngine.Vector3.zero); any = true; } else b.Encapsulate(p); }
    }
    if (!any) return null; return b;
};
System.Func<UnityEngine.Bounds, string> fmt = b => "u" + b.min.x.ToString("0.0") + ".." + b.max.x.ToString("0.0") + " v" + b.min.z.ToString("0.0") + ".." + b.max.z.ToString("0.0") + " y" + b.min.y.ToString("0.00") + ".." + b.max.y.ToString("0.00");
var rooms = new[] { "Floor17/Apartment_17F01/VisualPass_NoahRoom", "Floor17/Apartment_17F01/VisualPass_17F01Living", "Floor17/Apartment_17F02/VisualPass_17F02", "Floor17/Apartment_17F03/VisualPass_17F03", "Floor17/Apartment_17F04/VisualPass_17F04" };
var names = new System.Collections.Generic.List<string>(); var boxes = new System.Collections.Generic.List<UnityEngine.Bounds>(); var kinds = new System.Collections.Generic.List<int>();
foreach (var rn in rooms) {
    var g = UnityEngine.GameObject.Find(rn); if (g == null) continue; var props = g.transform.Find("Props");
    foreach (UnityEngine.Transform t in props) { if (t.name.Contains("_Cord") || t.name.Contains("_Disc")) continue; var b = obb(t); if (b == null) continue; names.Add(t.name); boxes.Add(b.Value); kinds.Add(0); }
}
foreach (var n in new[] { "Floor17/Apartment_17F01", "Floor17/Apartment_17F02", "Floor17/Apartment_17F03", "Floor17/Apartment_17F04" }) foreach (UnityEngine.Transform t in UnityEngine.GameObject.Find(n).transform) {
    if (!t.gameObject.activeSelf || t.name.StartsWith("VisualPass") || t.name.StartsWith("Lighting_") || t.name.StartsWith("Carpet")) continue;
    var b = obb(t); if (b == null) continue; names.Add((t.name.StartsWith("Door_") ? "DOOR:" : "old:") + t.name); boxes.Add(b.Value); kinds.Add(t.name.StartsWith("Door_") ? 2 : 1);
}
foreach (var n in new[] { "HouseholdActors" }) foreach (UnityEngine.Transform t in UnityEngine.GameObject.Find(n).transform) { var b = obb(t); if (b == null) continue; names.Add("unit:" + t.name); boxes.Add(b.Value); kinds.Add(3); }
int issues = 0;
// doors: everything within 0.9 m of a door on either face, up to head height
for (int i = 0; i < names.Count; i++) if (kinds[i] == 2) {
    var d = boxes[i]; var clear = d; bool alongU = d.size.x > d.size.z; var e = clear.extents; if (alongU) e.z = 0.95f; else e.x = 0.95f; clear.extents = e;
    for (int j = 0; j < names.Count; j++) if (kinds[j] == 0 || kinds[j] == 1) { var b = boxes[j]; if (b.min.y > 1.9f || b.size.y < 0.12f) continue; if (b.Intersects(clear)) { sb.AppendLine("DOOR " + names[i].Substring(5) + " " + fmt(d) + " blocked by " + names[j] + " " + fmt(b)); issues++; } }
}
// floor props that overlap each other
for (int i = 0; i < names.Count; i++) { if (kinds[i] != 0) continue; var a = boxes[i]; for (int j = 0; j < names.Count; j++) { if (j == i || kinds[j] == 2 || (kinds[j] == 0 && j < i)) continue; var b = boxes[j]; float ox = UnityEngine.Mathf.Min(a.max.x, b.max.x) - UnityEngine.Mathf.Max(a.min.x, b.min.x), oy = UnityEngine.Mathf.Min(a.max.y, b.max.y) - UnityEngine.Mathf.Max(a.min.y, b.min.y), oz = UnityEngine.Mathf.Min(a.max.z, b.max.z) - UnityEngine.Mathf.Max(a.min.z, b.min.z); if (names[i].Contains("Pillows") || names[j].Contains("Pillows") || names[i].Contains("Throw") || names[j].Contains("Throw") || names[j] == "old:Kitchen" || names[j] == "old:Kitchen_3" || names[i].Contains("Plush") || names[j].Contains("Plush") || names[i].Contains("Bunny") || names[j].Contains("Bunny")) continue; if (ox > 0.06f && oy > 0.06f && oz > 0.06f) { sb.AppendLine("OVERLAP " + names[i] + " x " + names[j] + " " + ox.ToString("0.00") + "x" + oz.ToString("0.00") + "x" + oy.ToString("0.00")); issues++; } } }
// floating: raised props with nothing under them
for (int i = 0; i < names.Count; i++) { if (kinds[i] != 0) continue; var a = boxes[i]; if (a.min.y < 0.08f) continue; if (names[i].Contains("Pendant") || names[i].Contains("Mirror") || names[i].Contains("Panel") || names[i].Contains("Radiator") || names[i].Contains("Calendar") || names[i].Contains("Pillows") || names[i].Contains("Throw") || names[i].Contains("Plush") || names[i].Contains("Bunny") || names[i].Contains("Hanging") || names[i].Contains("Shelf") || names[i].Contains("Clock") || names[i].Contains("Schedule") || names[i].Contains("CoatHooks")) continue; bool held = false; for (int j = 0; j < names.Count; j++) { if (j == i) continue; var b = boxes[j]; if (b.max.y < a.min.y - 0.1f || b.max.y > a.min.y + 0.1f) continue; if (b.min.x < a.center.x && b.max.x > a.center.x && b.min.z < a.center.z && b.max.z > a.center.z) held = true; } if (!held) { sb.AppendLine("FLOAT " + names[i] + " " + fmt(a)); issues++; } }
UnityEngine.Object.DestroyImmediate(org);
return "issues " + issues + "\n" + sb.ToString();
