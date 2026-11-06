// Read-only: GPU devices the lightmapper can use, the graphics card, and this process's temp path.
var sb = new System.Text.StringBuilder();
sb.Append("gfx=" + UnityEngine.SystemInfo.graphicsDeviceName + " vram=" + UnityEngine.SystemInfo.graphicsMemorySize + " cpu=" + UnityEngine.SystemInfo.processorType + " cores=" + UnityEngine.SystemInfo.processorCount + "; ");
var mi = typeof(UnityEditor.Lightmapping).GetMethod("GetLightmappingGpuDevices", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
if (mi != null) { try { var arr = mi.Invoke(null, null) as System.Array; foreach (var d in arr) { var t = d.GetType(); foreach (var f in t.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)) sb.Append(f.Name + "=" + f.GetValue(d) + " "); sb.Append("; "); } } catch (System.Exception e) { sb.Append("devErr " + e.Message + "; "); } }
sb.Append("TEMP=" + System.Environment.GetEnvironmentVariable("TEMP") + " TMP=" + System.Environment.GetEnvironmentVariable("TMP") + " tempPath=" + System.IO.Path.GetTempPath());
return sb.ToString();
