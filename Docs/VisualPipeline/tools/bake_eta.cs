// Read-only: the lightmapper's own progress and timing figures.
var sb = new System.Text.StringBuilder();
foreach (var m in typeof(UnityEditor.Lightmapping).GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)) {
    if (m.GetParameters().Length != 0 || m.ReturnType == typeof(void)) continue;
    var n = m.Name; if (!(n.Contains("Time") || n.Contains("Progress") || n.Contains("Performance") || n.Contains("Stage") || n.Contains("Eta") || n.Contains("ETA") || n.Contains("Status"))) continue;
    try { sb.Append(n + "=" + m.Invoke(null, null) + "; "); } catch { }
}
foreach (var p in typeof(UnityEditor.Lightmapping).GetProperties(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)) {
    var n = p.Name; if (!(n.Contains("Time") || n.Contains("rogress") || n.Contains("Stage") || n.Contains("isRunning"))) continue;
    try { sb.Append(n + "=" + p.GetValue(null, null) + "; "); } catch { }
}
return sb.ToString();
