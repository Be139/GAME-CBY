// Read-only: cut-away plan around the lobby, aligned to floor coordinates, for placing the street pass.
var outDir = @"D:/UGit/GAME-CBY/Docs/VisualPipeline/tools/_work/plans";
var camGo = new UnityEngine.GameObject("__Plan_Cam"); camGo.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
int W = 1600, H = 1600;
var rt = new UnityEngine.RenderTexture(W, H, 24, UnityEngine.RenderTextureFormat.ARGB32, UnityEngine.RenderTextureReadWrite.sRGB); rt.antiAliasing = 4;
var tex = new UnityEngine.Texture2D(W, H, UnityEngine.TextureFormat.RGB24, false);
try {
    var cam = camGo.AddComponent<UnityEngine.Camera>(); var acd = camGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
    acd.renderPostProcessing = false; cam.orthographic = true; cam.allowHDR = false; cam.targetTexture = rt; cam.clearFlags = UnityEngine.CameraClearFlags.SolidColor; cam.backgroundColor = new UnityEngine.Color(0.05f, 0.05f, 0.08f);
    var U = new UnityEngine.Vector3(0.931f, 0, 0.365f).normalized; var V = new UnityEngine.Vector3(-0.365f, 0, 0.931f).normalized;
    camGo.transform.position = new UnityEngine.Vector3(-2312f, 5f, 595.6f) + U * 12f + V * 12f; camGo.transform.rotation = UnityEngine.Quaternion.Euler(90, -21.4f, 0);
    cam.orthographicSize = 60f; cam.nearClipPlane = 0.01f; cam.farClipPlane = 8f;
    cam.Render();
    var prev = UnityEngine.RenderTexture.active; UnityEngine.RenderTexture.active = rt; tex.ReadPixels(new UnityEngine.Rect(0, 0, W, H), 0, 0); tex.Apply(); UnityEngine.RenderTexture.active = prev;
    System.IO.File.WriteAllBytes(outDir + "/uv.jpg", UnityEngine.ImageConversion.EncodeToJPG(tex, 85));
} finally { UnityEngine.Object.DestroyImmediate(camGo); rt.Release(); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(tex); }
return "plan done";
