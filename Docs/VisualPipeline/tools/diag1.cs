// Read-only diagnosis of the blotchy walls: bake settings, lightmap sizes, texel density per area, and coplanar shadow-only shells.
var sb = new System.Text.StringBuilder();
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
sb.AppendLine("dirty=" + scene.isDirty + " playing=" + UnityEditor.EditorApplication.isPlaying);
var ls = UnityEditor.Lightmapping.lightingSettings;
foreach (var p in typeof(UnityEngine.LightingSettings).GetProperties()) { try { sb.Append(p.Name + "=" + p.GetValue(ls, null) + "; "); } catch { } }
sb.AppendLine();
var maps = UnityEngine.LightmapSettings.lightmaps;
for (int i = 0; i < maps.Length; i++) { var t = maps[i].lightmapColor; sb.AppendLine("lightmap" + i + " " + (t == null ? "null" : t.width + "x" + t.height + " " + t.format) + " dir=" + (maps[i].lightmapDir != null)); }
sb.AppendLine("ambient " + UnityEngine.RenderSettings.ambientMode + " sky=" + UnityEngine.RenderSettings.ambientSkyColor + " eq=" + UnityEngine.RenderSettings.ambientEquatorColor + " gr=" + UnityEngine.RenderSettings.ambientGroundColor + " int=" + UnityEngine.RenderSettings.ambientIntensity + " skybox=" + (UnityEngine.RenderSettings.skybox == null ? "none" : UnityEngine.RenderSettings.skybox.name) + " refl=" + UnityEngine.RenderSettings.reflectionIntensity);
sb.AppendLine("fog " + UnityEngine.RenderSettings.fog + " " + UnityEngine.RenderSettings.fogMode + " " + UnityEngine.RenderSettings.fogColor + " d=" + UnityEngine.RenderSettings.fogDensity + " s=" + UnityEngine.RenderSettings.fogStartDistance + " e=" + UnityEngine.RenderSettings.fogEndDistance);
foreach (var m in typeof(UnityEditor.PlayerSettings).GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
    if (m.Name.Contains("LightmapEncoding") && m.Name.StartsWith("Get") && m.GetParameters().Length == 1) { try { sb.AppendLine(m.Name + "=" + m.Invoke(null, new object[] { UnityEditor.BuildTargetGroup.Standalone })); } catch (System.Exception e) { sb.AppendLine(m.Name + " err " + e.GetType().Name); } }
var prof = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Rendering.VolumeProfile>("Assets/Settings/SampleSceneProfile_Dusk.asset");
if (prof != null) foreach (var c in prof.components) { sb.Append("vol " + c.GetType().Name + " active=" + c.active + ": "); foreach (var f in c.GetType().GetFields()) { var v = f.GetValue(c) as UnityEngine.Rendering.VolumeParameter; if (v != null && v.overrideState) sb.Append(f.Name + "=" + v.GetType().GetProperty("value").GetValue(v, null) + " "); } sb.AppendLine(); }
// texel density per area: sqrt(allocated texels / world surface area)
var groups = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<float>>();
var worst = new System.Collections.Generic.List<string>();
foreach (var r in UnityEngine.Object.FindObjectsOfType<UnityEngine.MeshRenderer>()) {
    if (!r.enabled || r.lightmapIndex < 0 || r.lightmapIndex >= maps.Length || maps[r.lightmapIndex].lightmapColor == null) continue;
    var mf = r.GetComponent<UnityEngine.MeshFilter>(); if (mf == null || mf.sharedMesh == null || !mf.sharedMesh.isReadable) continue;
    var mesh = mf.sharedMesh; var v = mesh.vertices; var tri = mesh.triangles; var mx = r.transform.localToWorldMatrix; double area = 0;
    for (int i = 0; i < tri.Length; i += 3) { var a = mx.MultiplyPoint3x4(v[tri[i]]); var b = mx.MultiplyPoint3x4(v[tri[i + 1]]); var c = mx.MultiplyPoint3x4(v[tri[i + 2]]); area += UnityEngine.Vector3.Cross(b - a, c - a).magnitude * 0.5; }
    if (area < 0.5) continue;
    var tex = maps[r.lightmapIndex].lightmapColor; var so = r.lightmapScaleOffset; double texels = so.x * tex.width * so.y * tex.height;
    float dens = (float)System.Math.Sqrt(texels / area);
    var path = r.transform.name; var q = r.transform.parent; var parts = new System.Collections.Generic.List<string>(); var t2 = r.transform; while (t2 != null) { parts.Insert(0, t2.name); t2 = t2.parent; }
    var key = parts.Count > 1 ? parts[0] + "/" + parts[1] : parts[0]; if (parts.Count > 2 && parts[1].StartsWith("Apartment")) key += "/" + parts[2];
    if (!groups.ContainsKey(key)) groups[key] = new System.Collections.Generic.List<float>(); groups[key].Add(dens);
    if (area > 8 && dens < 6) worst.Add(string.Join("/", parts.ToArray()) + " area=" + area.ToString("0") + " dens=" + dens.ToString("0.0") + " scale=" + r.scaleInLightmap);
}
foreach (var kv in groups) { kv.Value.Sort(); sb.AppendLine("dens " + kv.Key + " n=" + kv.Value.Count + " min=" + kv.Value[0].ToString("0.0") + " med=" + kv.Value[kv.Value.Count / 2].ToString("0.0") + " max=" + kv.Value[kv.Value.Count - 1].ToString("0.0")); }
sb.AppendLine("large low-density surfaces " + worst.Count); for (int i = 0; i < worst.Count && i < 40; i++) sb.AppendLine("  " + worst[i]);
// shadow-only renderers that sit on top of a visible lightmapped surface
var shadowOnly = new System.Collections.Generic.List<UnityEngine.MeshRenderer>(); var visible = new System.Collections.Generic.List<UnityEngine.MeshRenderer>();
foreach (var r in UnityEngine.Object.FindObjectsOfType<UnityEngine.MeshRenderer>()) { if (!r.enabled) continue; if (r.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly) shadowOnly.Add(r); else if (r.lightmapIndex >= 0) visible.Add(r); }
int dup = 0; var dupNames = new System.Text.StringBuilder();
foreach (var s in shadowOnly) { var bs = s.bounds; bool gi = (UnityEditor.GameObjectUtility.GetStaticEditorFlags(s.gameObject) & UnityEditor.StaticEditorFlags.ContributeGI) != 0;
    foreach (var r in visible) { var br = r.bounds; if ((br.center - bs.center).magnitude < 0.05f && (br.size - bs.size).magnitude < 0.05f) { dup++; if (dupNames.Length < 1500) dupNames.Append(s.name + "(gi=" + gi + ")~" + r.name + " "); break; } } }
sb.AppendLine("shadowOnly=" + shadowOnly.Count + " coincidentWithVisible=" + dup); sb.AppendLine(dupNames.ToString());
return sb.ToString();
