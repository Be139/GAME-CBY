// Bake quality pass for the blotchy walls. Nothing is moved or rebuilt:
// 1. invisible old shell pieces that sit exactly on their visible copies stop rendering,
// 2. baked lamp shadows get a soft radius so their edges stop showing lightmap texels,
// 3. big surfaces get more lightmap texels, small props take light probes instead,
// 4. the bake settings switch to more samples with the Intel denoiser.
if (UnityEditor.EditorApplication.isPlaying) return "skip: play mode";
var sb = new System.Text.StringBuilder();
var log = new System.Text.StringBuilder();
var roots = new System.Collections.Generic.List<UnityEngine.Transform>();
foreach (var n in new[] { "Floor17/Apartment_17F01", "Floor17/Apartment_17F02", "Floor17/Apartment_17F03", "Floor17/Apartment_17F04", "Floor17/VisualPass_Corridor", "Lobby" }) roots.Add(UnityEngine.GameObject.Find(n).transform);
System.Func<UnityEngine.Transform, bool> inRooms = t => { foreach (var r in roots) if (t.IsChildOf(r)) return true; return false; };
var maps = UnityEngine.LightmapSettings.lightmaps;

// 1. coplanar shadow-only shells
var visible = new System.Collections.Generic.List<UnityEngine.MeshRenderer>(); var shadowOnly = new System.Collections.Generic.List<UnityEngine.MeshRenderer>();
foreach (var r in UnityEngine.Object.FindObjectsOfType<UnityEngine.MeshRenderer>()) { if (!r.enabled) continue; if (r.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly) shadowOnly.Add(r); else if (r.lightmapIndex >= 0 && r.lightmapIndex < 65000) visible.Add(r); }
int shells = 0;
foreach (var s in shadowOnly) { var bs = s.bounds; foreach (var r in visible) { var br = r.bounds; if ((br.center - bs.center).magnitude < 0.05f && (br.size - bs.size).magnitude < 0.05f) { s.enabled = false; shells++; log.AppendLine("S " + UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(s)); break; } } }

// 2. soft baked shadows
int soft = 0;
foreach (var root in roots) foreach (var l in root.GetComponentsInChildren<UnityEngine.Light>(true)) {
    if (l.lightmapBakeType != UnityEngine.LightmapBakeType.Baked || l.shadowRadius > 0.01f) continue;
    if (l.type == UnityEngine.LightType.Point) l.shadowRadius = l.name.Contains("Fill") ? 0.35f : 0.1f;
    else if (l.type == UnityEngine.LightType.Spot) l.shadowRadius = 0.05f;
    else continue;
    soft++;
}

// 3. lightmap texels where they show
int scaled = 0, probes = 0; double predicted = 0;
foreach (var r in UnityEngine.Object.FindObjectsOfType<UnityEngine.MeshRenderer>()) {
    if (!r.enabled || !inRooms(r.transform)) continue;
    var flags = UnityEditor.GameObjectUtility.GetStaticEditorFlags(r.gameObject);
    if ((flags & UnityEditor.StaticEditorFlags.ContributeGI) == 0 || r.receiveGI != UnityEngine.ReceiveGI.Lightmaps) continue;
    var e = r.bounds.size; float mx = UnityEngine.Mathf.Max(e.x, UnityEngine.Mathf.Max(e.y, e.z));
    if (mx < 0.6f) { r.receiveGI = UnityEngine.ReceiveGI.LightProbes; probes++; log.AppendLine("P " + UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(r)); continue; }
    var mf = r.GetComponent<UnityEngine.MeshFilter>(); if (mf == null || mf.sharedMesh == null || !mf.sharedMesh.isReadable) continue;
    if (r.lightmapIndex < 0 || r.lightmapIndex >= maps.Length || maps[r.lightmapIndex].lightmapColor == null) continue;
    var mesh = mf.sharedMesh; var v = mesh.vertices; var tri = mesh.triangles; var m4 = r.transform.localToWorldMatrix; double area = 0;
    for (int i = 0; i < tri.Length; i += 3) { var a = m4.MultiplyPoint3x4(v[tri[i]]); var b = m4.MultiplyPoint3x4(v[tri[i + 1]]); var c = m4.MultiplyPoint3x4(v[tri[i + 2]]); area += UnityEngine.Vector3.Cross(b - a, c - a).magnitude * 0.5; }
    var tex = maps[r.lightmapIndex].lightmapColor; var so = r.lightmapScaleOffset; double texels = so.x * tex.width * so.y * tex.height;
    float d0 = (float)System.Math.Sqrt(texels / System.Math.Max(area, 0.01)); float target = r.bounds.center.y > 100f ? 30f : 18f;
    float s0 = r.scaleInLightmap, s1 = s0;
    if (area > 3 && d0 < target * 0.9f) { s1 = UnityEngine.Mathf.Clamp(s0 * target / d0, s0, 4f); log.AppendLine("L " + UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(r) + " " + s0.ToString(System.Globalization.CultureInfo.InvariantCulture)); r.scaleInLightmap = s1; scaled++; }
    predicted += texels * (s1 / s0) * (s1 / s0);
}
System.IO.File.AppendAllText(@"D:/UGit/GAME-CBY/Docs/VisualPipeline/tools/_work/q_fix_undo.txt", log.ToString());

// 4. bake settings (also re-applied by the bake script)
var ls = UnityEditor.Lightmapping.lightingSettings;
System.Action<string, string> setEnum = (prop, val) => { var p = typeof(UnityEngine.LightingSettings).GetProperty(prop); p.SetValue(ls, System.Enum.Parse(p.PropertyType, val), null); };
ls.lightmapMaxSize = 4096; ls.lightmapPadding = 4; ls.directSampleCount = 128; ls.indirectSampleCount = 512; ls.environmentSampleCount = 512;
ls.ao = true; ls.aoMaxDistance = 0.6f; ls.aoExponentIndirect = 1.0f; ls.aoExponentDirect = 0f;
setEnum("lightmapCompression", "None"); setEnum("filteringMode", "Advanced");
setEnum("denoiserTypeDirect", "OpenImage"); setEnum("denoiserTypeIndirect", "OpenImage"); setEnum("denoiserTypeAO", "OpenImage");
setEnum("filterTypeDirect", "None"); setEnum("filterTypeIndirect", "Gaussian"); setEnum("filterTypeAO", "Gaussian");
ls.filteringGaussianRadiusIndirect = 1f; ls.filteringGaussianRadiusAO = 1f;
UnityEditor.EditorUtility.SetDirty(ls);
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene); UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
return "shellsOff=" + shells + " softShadows=" + soft + " scaledSurfaces=" + scaled + " propsToProbes=" + probes + " predictedTexelsM=" + (predicted / 1e6).ToString("0.0");
