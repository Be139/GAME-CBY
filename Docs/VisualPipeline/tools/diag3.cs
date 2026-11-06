// Read-only: interior lights (for soft baked shadows and new window light), denoiser support, small lightmapped props.
var sb = new System.Text.StringBuilder();
foreach (var m in typeof(UnityEditor.Lightmapping).GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
    if (m.Name.Contains("Denoiser") && m.Name.StartsWith("Is") && m.GetParameters().Length == 0) { try { sb.Append(m.Name + "=" + m.Invoke(null, null) + " "); } catch (System.Exception e) { sb.Append(m.Name + " err "); } }
sb.AppendLine();
foreach (var root in new[] { "Floor17/Apartment_17F01", "Floor17/Apartment_17F02", "Floor17/Apartment_17F03", "Floor17/Apartment_17F04", "Floor17/VisualPass_Corridor", "Lobby" }) {
    var g = UnityEngine.GameObject.Find(root); sb.Append("[" + root + "] ");
    foreach (var l in g.GetComponentsInChildren<UnityEngine.Light>()) {
        if (!l.enabled) continue;
        var c = UnityEngine.ColorUtility.ToHtmlStringRGB(l.color);
        sb.Append(l.name + ":" + l.type.ToString().Substring(0, 2) + "/" + l.lightmapBakeType.ToString().Substring(0, 2) + "/i" + l.intensity.ToString("0.##") + "/r" + l.range.ToString("0.#") + "/sr" + l.shadowRadius.ToString("0.##") + "/" + c + (l.shadows == UnityEngine.LightShadows.None ? "/ns" : "") + "  ");
    }
    sb.AppendLine();
}
int small = 0, smallTex = 0; float smallArea = 0;
foreach (var r in UnityEngine.Object.FindObjectsOfType<UnityEngine.MeshRenderer>()) {
    if (!r.enabled || r.lightmapIndex < 0 || r.lightmapIndex > 65000) continue;
    var e = r.bounds.size; float mx = UnityEngine.Mathf.Max(e.x, UnityEngine.Mathf.Max(e.y, e.z));
    if (mx < 0.6f) { small++; }
}
sb.AppendLine("lightmapped objects under 0.6 m: " + small);
return sb.ToString();
