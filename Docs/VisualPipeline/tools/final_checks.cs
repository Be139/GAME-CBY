// Read-only: shadow budget and the objects hidden by this pass, listed so they can be checked against the dynamic lookups.
var sb = new System.Text.StringBuilder();
var asset = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
sb.AppendLine("addlShadowAtlas=" + asset.additionalLightsShadowmapResolution + " tiers=" + asset.additionalLightsShadowResolutionTierLow + "/" + asset.additionalLightsShadowResolutionTierMedium + "/" + asset.additionalLightsShadowResolutionTierHigh);
int shadowed = 0, mixed = 0, realtime = 0; var names = new System.Collections.Generic.List<string>();
foreach (var l in UnityEngine.Object.FindObjectsOfType<UnityEngine.Light>(false)) {
    if (!l.enabled || l.transform.root.name.StartsWith("City")) continue;
    if (l.lightmapBakeType == UnityEngine.LightmapBakeType.Baked) continue;
    if (l.lightmapBakeType == UnityEngine.LightmapBakeType.Mixed) mixed++; else realtime++;
    if (l.shadows != UnityEngine.LightShadows.None && l.type != UnityEngine.LightType.Directional) { shadowed++; names.Add(l.name); }
}
sb.AppendLine("mixed=" + mixed + " realtime=" + realtime + " shadowedPunctual=" + shadowed + " : " + string.Join(",", names.ToArray()));
var hidden = new System.Collections.Generic.List<string>();
foreach (var n in new[] { "Floor17/Apartment_17F01", "Floor17/Apartment_17F02", "Floor17/Apartment_17F03", "Floor17/Apartment_17F04", "Lobby" }) foreach (UnityEngine.Transform t in UnityEngine.GameObject.Find(n).transform) if (!t.gameObject.activeSelf) hidden.Add(t.name);
sb.AppendLine("HIDDEN " + string.Join("|", hidden.ToArray()));
return sb.ToString();
