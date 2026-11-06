// Third helper block: placement tied to real surfaces. Nothing here guesses where a wall is: rays find it from the old colliders, and stacked props read the height of what they stand on.
// dir: 0 +u, 1 -u, 2 +v, 3 -v
System.Func<int, UnityEngine.Vector3> DirV = d => d == 0 ? UnityEngine.Vector3.right : d == 1 ? UnityEngine.Vector3.left : d == 2 ? UnityEngine.Vector3.forward : UnityEngine.Vector3.back;
System.Func<float, float, float, int, float> WallAt = (u, y, v, dir) => {
    OldPiece("Mesh1"); float best = 99f;
    var wd = root.TransformDirection(DirV(dir)).normalized;
    foreach (var h in UnityEngine.Physics.RaycastAll(root.TransformPoint(V(u, y, v)), wd, 14f, ~0, UnityEngine.QueryTriggerInteraction.Ignore))
        if (h.collider.transform.IsChildOf(oldShell) && h.distance < best) best = h.distance;
    if (best > 90f) sb.AppendLine("no wall from " + u + "," + v + " dir " + dir);
    return best;
};
System.Func<int, float> YawFrom = d => d == 0 ? -90f : d == 1 ? 90f : d == 2 ? 180f : 0f;
// slides a placed object so its back face touches the wall in direction dir
System.Action<UnityEngine.GameObject, float, float, float, int, float> Snap = (go, u, y, v, dir, gap) => {
    float d = WallAt(u, y, v, dir); if (d > 90f) return;
    var nb = Obb(go.transform); var p = root.InverseTransformPoint(go.transform.position);
    if (dir == 0) p.x += (u + d - gap) - nb.max.x; else if (dir == 1) p.x += (u - d + gap) - nb.min.x; else if (dir == 2) p.z += (v + d - gap) - nb.max.z; else p.z += (v - d + gap) - nb.min.z;
    go.transform.position = root.TransformPoint(p);
};
// floor standing, back to the wall found in direction dir
System.Func<string, string, char, float, float, float, int, string, bool, UnityEngine.GameObject> Back = (id, name, fit, size, u, v, dir, tint, solid) => {
    var go = Put(id, name, fit, size, u, 0f, v, YawFrom(dir), solid, size > 1.2f ? 1f : 0.6f, tint); if (go == null) return null;
    Snap(go, u, 1f, v, dir, 0.03f); return go;
};
// free standing on the floor
System.Func<string, string, char, float, float, float, float, string, bool, UnityEngine.GameObject> Stand = (id, name, fit, size, u, v, yaw, tint, solid) => Put(id, name, fit, size, u, 0f, v, yaw, solid, size > 1.2f ? 1f : 0.6f, tint);
// flat against a wall at height y (bottom edge)
System.Func<string, string, char, float, float, float, float, int, string, UnityEngine.GameObject> OnWall = (id, name, fit, size, u, y, v, dir, tint) => {
    var go = Put(id, name, fit, size, u, y, v, YawFrom(dir), false, 0.4f, tint); if (go == null) return null;
    Snap(go, u, y + 0.1f, v, dir, 0.005f); return go;
};
// stands on top of another object, offset from its centre
System.Func<UnityEngine.GameObject, string, string, char, float, float, float, float, string, UnityEngine.GameObject> On = (host, id, name, fit, size, du, dv, yaw, tint) => {
    if (host == null) { sb.AppendLine("no host for " + name); return null; }
    var b = Obb(host.transform); var go = Put(id, name, fit, size, b.center.x + du, b.max.y - 0.002f, b.center.z + dv, yaw, false, 0.3f, tint); return go;
};
System.Func<string, UnityEngine.GameObject> Old = n => { var t = apt.Find(n); if (t == null) { sb.AppendLine("no object " + n); return null; } return t.gameObject; };
// framed picture on the real wall surface
System.Action<string, string, float, float, float, int, float, float> ArtW = (name, tex, u, y, v, dir, w, h) => {
    float d = WallAt(u, y, v, dir); if (d > 90f) return; var at = V(u, y, v) + DirV(dir) * (d - 0.002f); Art(name, tex, at.x, at.y, at.z, w, h, YawFrom(dir));
};
// moves a kept old prop without touching anything else
System.Action<string, float, float> Move = (n, u, v) => { var t = apt.Find(n); if (t == null) { sb.AppendLine("no object " + n); return; } var b = Obb(t); var d = root.TransformVector(V(u - b.center.x, 0f, v - b.center.z)); t.position += d; };
// invisible caster outside a window: keeps the low evening sun out of rooms set at another hour
System.Action<string, UnityEngine.Vector3, UnityEngine.Vector3> SunBlock = (name, c, size) => {
    var g = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cube); g.name = name; UnityEngine.Object.DestroyImmediate(g.GetComponent<UnityEngine.Collider>()); g.transform.SetParent(shellT, false); g.transform.localPosition = c; g.transform.localScale = size;
    g.GetComponent<UnityEngine.MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
};
