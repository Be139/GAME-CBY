// Play mode: render the fixed room views with the live game camera's settings, so the review matches what players get.
var outDir = @"D:/UGit/GAME-CBY/Docs/VisualPipeline/prototype/_work/noah_views";
var tag = "TAG_PLACEHOLDER";
var sb = new System.Text.StringBuilder();
foreach (var v in UnityEngine.Object.FindObjectsOfType<UnityEngine.Rendering.Volume>(true))
{
    sb.Append("volume " + v.name + " active=" + v.isActiveAndEnabled + " global=" + v.isGlobal + " w=" + v.weight.ToString("0.00") + " prio=" + v.priority + " :");
    if (v.sharedProfile != null) foreach (var c in v.sharedProfile.components) { if (!c.active) continue; var ca = c as UnityEngine.Rendering.Universal.ColorAdjustments; if (ca != null) sb.Append(" exposure=" + ca.postExposure.value.ToString("0.00") + " sat=" + ca.saturation.value.ToString("0") + " contrast=" + ca.contrast.value.ToString("0")); var tm = c as UnityEngine.Rendering.Universal.Tonemapping; if (tm != null) sb.Append(" tone=" + tm.mode.value); }
    sb.AppendLine();
}
var lighting = UnityEngine.GameObject.Find("DuskLighting");
sb.AppendLine("sun=" + (UnityEngine.GameObject.Find("DuskLighting/Dusk_Content/Sun_Dusk") != null ? UnityEngine.GameObject.Find("DuskLighting/Dusk_Content/Sun_Dusk").GetComponent<UnityEngine.Light>().intensity.ToString("0.00") : "?") + " reflection=" + UnityEngine.RenderSettings.reflectionIntensity + " fog=" + UnityEngine.RenderSettings.fogDensity);
UnityEngine.Camera game = null;
foreach (var c in UnityEngine.Camera.allCameras) { if (c.targetTexture == null && (game == null || c.depth > game.depth)) game = c; }
if (game == null) return sb.ToString() + "no camera";
var gameData = game.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
sb.AppendLine("game camera=" + game.name + " post=" + gameData.renderPostProcessing + " volMask=" + gameData.volumeLayerMask.value + " aa=" + gameData.antialiasing + " hdr=" + game.allowHDR);
var U = new UnityEngine.Vector3(0.931f, 0, 0.365f).normalized; var V = new UnityEngine.Vector3(-0.365f, 0, 0.931f).normalized;
System.Func<float, float, float, UnityEngine.Vector3> W = (u, y, v) => new UnityEngine.Vector3(-2312.0f, 111.68f, 595.6f) + U * u + V * v + UnityEngine.Vector3.up * y;
var camGo = new UnityEngine.GameObject("__LD_Cam"); var rt = new UnityEngine.RenderTexture(1600, 900, 24, UnityEngine.RenderTextureFormat.ARGB32, UnityEngine.RenderTextureReadWrite.sRGB); rt.antiAliasing = 4;
var tex = new UnityEngine.Texture2D(1600, 900, UnityEngine.TextureFormat.RGB24, false);
var cam = camGo.AddComponent<UnityEngine.Camera>(); cam.CopyFrom(game); cam.depth = -100; cam.enabled = false;
var d = camGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>(); d.renderPostProcessing = gameData.renderPostProcessing; d.volumeLayerMask = gameData.volumeLayerMask; d.antialiasing = gameData.antialiasing; d.volumeTrigger = camGo.transform;
cam.nearClipPlane = 0.02f; cam.targetTexture = rt;
var names = new[] { "game_start", "to_window", "to_head", "to_south", "to_desk", "door_side" };
var pos = new[] { W(-1.63f, 1.03f, -1.27f), W(-1.7f, 1.25f, -0.2f), W(0.4f, 1.3f, -1.5f), W(0.6f, 1.3f, 1.6f), W(1.5f, 1.2f, 0.2f), W(1.45f, 1.1f, -0.9f) };
var look = new[] { W(0f, 0.6f, 1.2f), W(2f, 1.1f, 0.4f), W(-0.1f, 0.7f, 1.8f), W(0.4f, 0.8f, -1.9f), W(-2.2f, 0.9f, 0.6f), W(-2.2f, 0.9f, -1.3f) };
var fov = new[] { 60f, 70f, 70f, 70f, 70f, 70f };
for (int i = 0; i < names.Length; i++)
{
    camGo.transform.position = pos[i]; camGo.transform.LookAt(look[i]); cam.fieldOfView = fov[i];
    cam.Render();
    var prev = UnityEngine.RenderTexture.active; UnityEngine.RenderTexture.active = rt; tex.ReadPixels(new UnityEngine.Rect(0, 0, 1600, 900), 0, 0); tex.Apply(); UnityEngine.RenderTexture.active = prev;
    System.IO.File.WriteAllBytes(outDir + "/" + tag + "_" + names[i] + ".jpg", UnityEngine.ImageConversion.EncodeToJPG(tex, 90));
}
UnityEngine.Object.Destroy(camGo); rt.Release(); UnityEngine.Object.Destroy(rt); UnityEngine.Object.Destroy(tex);
return sb.ToString();
