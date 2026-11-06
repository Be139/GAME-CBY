// Read-only: how the copied walls cast shadows, which lights can leak through single-sided walls, URP shadow settings, reflection probes.
var sb = new System.Text.StringBuilder();
var copies = new[] { "Cor_Extra_Mesh59", "F02_Main_Walls", "F04_Lily_Walls", "Cor_WallsA", "F01_Walls", "F03_Main_Walls", "F02_Bed_Walls", "F03_Ava_Walls", "F04_Main_Walls", "F01_Floor", "F02_Skirting" };
foreach (var r in UnityEngine.Object.FindObjectsOfType<UnityEngine.MeshRenderer>()) {
    if (System.Array.IndexOf(copies, r.name) < 0) continue;
    var m = r.sharedMaterial; var f = UnityEditor.GameObjectUtility.GetStaticEditorFlags(r.gameObject);
    sb.AppendLine(r.name + " cast=" + r.shadowCastingMode + " recv=" + r.receiveShadows + " scale=" + r.scaleInLightmap + " gi=" + ((f & UnityEditor.StaticEditorFlags.ContributeGI) != 0) + " mat=" + (m == null ? "-" : m.name + " dsgi=" + m.doubleSidedGI + " cull=" + (m.HasProperty("_Cull") ? m.GetFloat("_Cull").ToString() : "?")) + " parent=" + r.transform.parent.name);
}
var urp = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
if (urp != null) sb.AppendLine("urp " + urp.name + " addShadows=" + urp.supportsAdditionalLightShadows + " addRes=" + urp.additionalLightsShadowmapResolution + " mainRes=" + urp.mainLightShadowmapResolution + " dist=" + urp.shadowDistance + " soft=" + urp.supportsSoftShadows + " perObj=" + urp.maxAdditionalLightsCount + " cascades=" + urp.shadowCascadeCount + " depthBias=" + urp.shadowDepthBias + " normalBias=" + urp.shadowNormalBias + " hdr=" + urp.supportsHDR + " msaa=" + urp.msaaSampleCount + " renderScale=" + urp.renderScale);
var byArea = new System.Collections.Generic.Dictionary<string, int[]>();
var leak = new System.Text.StringBuilder();
foreach (var l in UnityEngine.Object.FindObjectsOfType<UnityEngine.Light>()) {
    if (!l.enabled || !l.gameObject.activeInHierarchy) continue;
    var t = l.transform; var top = t; var parts = new System.Collections.Generic.List<string>(); while (t != null) { parts.Insert(0, t.name); t = t.parent; }
    var key = parts.Count > 2 ? parts[0] + "/" + parts[1] : parts[0];
    if (!byArea.ContainsKey(key)) byArea[key] = new int[4];
    var mode = l.lightmapBakeType; var c = byArea[key];
    if (mode == UnityEngine.LightmapBakeType.Baked) c[0]++; else if (mode == UnityEngine.LightmapBakeType.Mixed) c[1]++; else c[2]++;
    if (mode != UnityEngine.LightmapBakeType.Baked && l.type != UnityEngine.LightType.Directional && l.shadows == UnityEngine.LightShadows.None) { c[3]++; if (leak.Length < 1800) leak.Append(l.name + "(" + l.type + "," + mode + ",r" + l.range.ToString("0.0") + ") "); }
}
foreach (var kv in byArea) sb.AppendLine("lights " + kv.Key + " baked=" + kv.Value[0] + " mixed=" + kv.Value[1] + " realtime=" + kv.Value[2] + " noShadowNonBaked=" + kv.Value[3]);
sb.AppendLine("no-shadow realtime/mixed: " + leak);
foreach (var p in UnityEngine.Object.FindObjectsOfType<UnityEngine.ReflectionProbe>()) { var t = p.transform; var path = t.name; while (t.parent != null) { t = t.parent; path = t.name + "/" + path; } sb.AppendLine("probe " + path + " mode=" + p.mode + " size=" + p.size + " box=" + p.boxProjection + " res=" + p.resolution + " enabled=" + p.enabled); }
var sun = UnityEngine.RenderSettings.sun; sb.AppendLine("sun " + (sun == null ? "none" : sun.name + " " + sun.lightmapBakeType + " shadows=" + sun.shadows + " int=" + sun.intensity + " rot=" + sun.transform.eulerAngles));
return sb.ToString();
