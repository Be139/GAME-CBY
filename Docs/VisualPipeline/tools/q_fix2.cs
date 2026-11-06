// Second quality pass, after measuring what the lightmap really gives each surface. The copied shell meshes are long thin strips; their generated lightmap UVs fill only part of the square,
// so the real density was far below the nominal one (corridor wall 14 texels per metre, wainscot 4.8). Scale is now set from the real density, measured triangle by triangle.
if (UnityEditor.EditorApplication.isPlaying) return "skip: play mode";
var roots = new System.Collections.Generic.List<UnityEngine.Transform>();
foreach (var n in new[] { "Floor17/Apartment_17F01", "Floor17/Apartment_17F02", "Floor17/Apartment_17F03", "Floor17/Apartment_17F04", "Floor17/VisualPass_Corridor", "Lobby" }) roots.Add(UnityEngine.GameObject.Find(n).transform);
System.Func<UnityEngine.Transform, bool> inRooms = t => { foreach (var r in roots) if (t.IsChildOf(r)) return true; return false; };
var maps = UnityEngine.LightmapSettings.lightmaps; var log = new System.Text.StringBuilder(); var sb = new System.Text.StringBuilder();
int changed = 0; double total = 0; var worst = new System.Collections.Generic.List<string>();
foreach (var r in UnityEngine.Object.FindObjectsOfType<UnityEngine.MeshRenderer>()) {
    if (!r.enabled || !inRooms(r.transform) || r.receiveGI != UnityEngine.ReceiveGI.Lightmaps) continue;
    if ((UnityEditor.GameObjectUtility.GetStaticEditorFlags(r.gameObject) & UnityEditor.StaticEditorFlags.ContributeGI) == 0) continue;
    if (r.lightmapIndex < 0 || r.lightmapIndex >= maps.Length || maps[r.lightmapIndex].lightmapColor == null) continue;
    var mf = r.GetComponent<UnityEngine.MeshFilter>(); if (mf == null || mf.sharedMesh == null || !mf.sharedMesh.isReadable) continue;
    var m = mf.sharedMesh; var v = m.vertices; var t = m.triangles; var uv2 = m.uv2.Length > 0 ? m.uv2 : m.uv; var mx = r.transform.localToWorldMatrix;
    double wa = 0, ua = 0;
    for (int i = 0; i < t.Length; i += 3) {
        var a = mx.MultiplyPoint3x4(v[t[i]]); var b = mx.MultiplyPoint3x4(v[t[i + 1]]); var c = mx.MultiplyPoint3x4(v[t[i + 2]]); wa += UnityEngine.Vector3.Cross(b - a, c - a).magnitude * 0.5;
        var p = uv2[t[i]]; var q = uv2[t[i + 1]]; var s = uv2[t[i + 2]]; ua += System.Math.Abs((q.x - p.x) * (s.y - p.y) - (s.x - p.x) * (q.y - p.y)) * 0.5;
    }
    if (wa < 0.05) continue;
    var tex = maps[r.lightmapIndex].lightmapColor; var so = r.lightmapScaleOffset;
    double eff = System.Math.Sqrt(ua * so.x * tex.width * so.y * tex.height / wa);
    bool big = wa > 3.0; bool f17 = r.bounds.center.y > 100f;
    double target = big ? (f17 ? 32.0 : 20.0) : 18.0;
    float s0 = r.scaleInLightmap, s1 = s0;
    if (eff < target * 0.9) {
        s1 = (float)(s0 * target / eff);
        // allocated texels grow with scale squared over the whole UV square; keep any one object under 3 million
        double alloc = wa * System.Math.Pow(20.0 * s1, 2); if (alloc > 3e6) s1 = (float)(20.0 > 0 ? System.Math.Sqrt(3e6 / wa) / 20.0 : s1);
        s1 = UnityEngine.Mathf.Clamp(s1, s0, 12f);
        if (s1 > s0 * 1.05f) { log.AppendLine("L2 " + UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(r) + " " + s0.ToString(System.Globalization.CultureInfo.InvariantCulture)); r.scaleInLightmap = s1; changed++; }
    }
    total += wa * System.Math.Pow(20.0 * s1, 2);
    if (eff < 10) worst.Add(r.name + " " + eff.ToString("0.0") + "->" + (eff * s1 / s0).ToString("0") + " (s " + s0.ToString("0.00") + "->" + s1.ToString("0.00") + ")");
}
System.IO.File.AppendAllText(@"D:/UGit/GAME-CBY/Docs/VisualPipeline/tools/_work/q_fix_undo.txt", log.ToString());
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene(); UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene); UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
return "rescaled=" + changed + " allocatedTexelsM=" + (total / 1e6).ToString("0.0") + " | lowest before: " + string.Join("; ", worst.GetRange(0, System.Math.Min(25, worst.Count)).ToArray());
