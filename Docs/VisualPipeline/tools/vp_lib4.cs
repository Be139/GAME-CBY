// Fourth helper block: textured glowing parts, skin faces painted by zone, and lift doors.
// material with a colour map and a separate lit map
System.Func<string, string, string, float, UnityEngine.Material> GlowTex = (name, tex, glowTex, power) => {
    var m = Mat(name, "FFFFFF", 0.35f, tex); m.EnableKeyword("_EMISSION"); m.SetTexture("_EmissionMap", Tx(glowTex)); m.SetColor("_EmissionColor", UnityEngine.Color.white * power);
    m.globalIlluminationFlags = UnityEngine.MaterialGlobalIlluminationFlags.None; UnityEditor.EditorUtility.SetDirty(m); return m;
};
// paints only the faces of an old skin piece that look into the given zones (u0,u1,v0,v1). The new faces sit 4 mm proud of the old ones.
System.Action<string, string, UnityEngine.Material, float[][], float> CopySplit = (srcName, name, m, zones, lm) => {
    var src = OldPiece(srcName); if (src == null) { sb.AppendLine("no piece " + srcName); return; }
    var o = src.GetComponent<UnityEngine.MeshFilter>().sharedMesh; var M = root.worldToLocalMatrix * src.localToWorldMatrix; var vs = o.vertices; var tri = o.triangles;
    var outV = new System.Collections.Generic.List<UnityEngine.Vector3>(); var outT = new System.Collections.Generic.List<int>(); var outU = new System.Collections.Generic.List<UnityEngine.Vector2>();
    for (int i = 0; i + 2 < tri.Length; i += 3) {
        var a = M.MultiplyPoint3x4(vs[tri[i]]); var b = M.MultiplyPoint3x4(vs[tri[i + 1]]); var c = M.MultiplyPoint3x4(vs[tri[i + 2]]);
        var n = UnityEngine.Vector3.Cross(b - a, c - a); if (M.determinant < 0f) n = -n; if (n.sqrMagnitude < 1e-8f) continue; n.Normalize(); if (UnityEngine.Mathf.Abs(n.y) > 0.5f) continue;
        var p = (a + b + c) / 3f + n * 0.3f; bool hit = false; foreach (var z in zones) if (p.x > z[0] && p.x < z[1] && p.z > z[2] && p.z < z[3] && p.y > -0.2f && p.y < 3.2f) hit = true; if (!hit) continue;
        int k = outV.Count; var ord = M.determinant < 0f ? new[] { a, c, b } : new[] { a, b, c };
        foreach (var q in ord) { outV.Add(q + n * 0.004f); outU.Add(new UnityEngine.Vector2((q.x + q.z) / 2f, q.y / 2f)); }
        outT.Add(k); outT.Add(k + 1); outT.Add(k + 2);
    }
    var mesh = new UnityEngine.Mesh(); mesh.name = name; mesh.vertices = outV.ToArray(); mesh.uv = outU.ToArray(); mesh.triangles = outT.ToArray(); mesh.RecalculateNormals(); mesh.RecalculateBounds();
    UnityEditor.UnwrapParam up; UnityEditor.UnwrapParam.SetDefaults(out up); up.hardAngle = 88f; up.packMargin = 0.02f; UnityEditor.Unwrapping.GenerateSecondaryUVSet(mesh, up);
    UnityEditor.AssetDatabase.CreateAsset(mesh, SharedDir + "/meshes/" + name + ".asset");
    var g = new UnityEngine.GameObject(name); g.transform.SetParent(shellT, false); g.AddComponent<UnityEngine.MeshFilter>().sharedMesh = mesh;
    var r = g.AddComponent<UnityEngine.MeshRenderer>(); r.sharedMaterial = m; r.scaleInLightmap = lm; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;
    UnityEditor.GameObjectUtility.SetStaticEditorFlags(g, GI); sb.AppendLine(name + " faces " + outT.Count / 3);
};
// double lift doors in a wall. c = centre along the wall, at = wall position, alongV true when the wall runs along v, face +1/-1 = side the doors face, width of the opening
System.Action<string, float, float, bool, float, float> LiftDoor = (name, c, at, alongV, face, width) => {
    var steel = Mat("Lift_Steel", "9A9A96", 0.45f, null); var frame = Mat("Lift_Frame", "4A3E36", 0.3f, null); var dark = Mat("Lift_Seam", "1C1C1E", 0.2f, null);
    System.Func<float, float, float, float, float, float, UnityEngine.Vector3[]> PS = (along, y, out0, sa, sy, so) => alongV ? new[] { V(at + out0 * face, y, along), V(so, sy, sa) } : new[] { V(along, y, at + out0 * face), V(sa, sy, so) };
    System.Action<string, UnityEngine.Material, float, float, float, float, float, float, float> B = (n, m, along, y, out0, sa, sy, so, lm) => { var ps = PS(along, y, out0, sa, sy, so); Box(name + n, propsT, m, ps[0], ps[1], 0f, lm); };
    B("_LeafA", steel, c - width / 4f, 1.15f, 0.03f, width / 2f - 0.01f, 2.3f, 0.05f, 0.5f);
    B("_LeafB", steel, c + width / 4f, 1.15f, 0.03f, width / 2f - 0.01f, 2.3f, 0.05f, 0.5f);
    B("_Seam", dark, c, 1.15f, 0.058f, 0.012f, 2.3f, 0.004f, 0f);
    B("_FrameL", frame, c - width / 2f - 0.07f, 1.2f, 0.05f, 0.14f, 2.4f, 0.1f, 0.5f);
    B("_FrameR", frame, c + width / 2f + 0.07f, 1.2f, 0.05f, 0.14f, 2.4f, 0.1f, 0.5f);
    B("_FrameT", frame, c, 2.44f, 0.05f, width + 0.28f, 0.14f, 0.1f, 0.5f);
    var disp = Mat(name + "_Disp", "FFFFFF", 0.3f, SharedDir + "/T_LiftDisplay.png"); disp.EnableKeyword("_EMISSION"); disp.SetTexture("_EmissionMap", Tx(SharedDir + "/T_LiftDisplay.png")); disp.SetColor("_EmissionColor", UnityEngine.Color.white * 1.4f); disp.globalIlluminationFlags = UnityEngine.MaterialGlobalIlluminationFlags.None;
    B("_Display", disp, c, 2.66f, 0.03f, 0.62f, 0.2f, 0.04f, 0f);
    var call = GlowTex("Lift_Call", SharedDir + "/T_LiftCall.png", SharedDir + "/T_LiftCallGlow.png", 1.2f);
    B("_Call", call, c + width / 2f + 0.42f, 1.25f, 0.015f, 0.14f, 0.24f, 0.03f, 0f);
};
