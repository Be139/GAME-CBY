// Render the room from fixed cameras with the scene's own lighting and volumes. Read-only.
var outDir = @"D:/UGit/GAME-CBY/Docs/VisualPipeline/prototype/_work/noah_views";
var tag = "TAG_PLACEHOLDER";
var U = new UnityEngine.Vector3(0.931f, 0, 0.365f).normalized; var V = new UnityEngine.Vector3(-0.365f, 0, 0.931f).normalized;
System.Func<float, float, float, UnityEngine.Vector3> W = (u, y, v) => new UnityEngine.Vector3(-2312.0f, 111.68f, 595.6f) + U * u + V * v + UnityEngine.Vector3.up * y;
var camGo = new UnityEngine.GameObject("__LD_Cam"); camGo.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
var rt = new UnityEngine.RenderTexture(1280, 720, 24, UnityEngine.RenderTextureFormat.ARGB32, UnityEngine.RenderTextureReadWrite.sRGB); rt.antiAliasing = 4;
var tex = new UnityEngine.Texture2D(1280, 720, UnityEngine.TextureFormat.RGB24, false);
var sb = new System.Text.StringBuilder();
try
{
    var cam = camGo.AddComponent<UnityEngine.Camera>();
    var acd = camGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
    acd.renderPostProcessing = true; acd.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.SubpixelMorphologicalAntiAliasing; acd.volumeLayerMask = 1; cam.nearClipPlane = 0.02f; cam.farClipPlane = 3000f; cam.allowHDR = true; cam.targetTexture = rt;
    var names = new[] { "game_start", "to_window", "to_head", "to_south", "to_desk", "top" };
    var pos = new[] { W(-1.63f, 1.03f, -1.27f), W(-1.7f, 1.25f, -0.2f), W(0.4f, 1.3f, -1.5f), W(0.6f, 1.3f, 1.6f), W(1.5f, 1.2f, 0.2f), W(-0.1f, 2.85f, 0f) };
    var look = new[] { W(0f, 0.6f, 1.2f), W(2f, 1.1f, 0.4f), W(-0.1f, 0.7f, 1.8f), W(0.4f, 0.8f, -1.9f), W(-2.2f, 0.9f, 0.6f), W(-0.1f, 0f, 0f) };
    var fov = new[] { 60f, 70f, 70f, 70f, 70f, 110f };
    for (int i = 0; i < names.Length; i++)
    {
        camGo.transform.position = pos[i];
        if (i == 5) camGo.transform.rotation = UnityEngine.Quaternion.Euler(90, -21.4f, 0); else camGo.transform.LookAt(look[i]);
        cam.fieldOfView = fov[i];
        cam.Render();
        var prev = UnityEngine.RenderTexture.active; UnityEngine.RenderTexture.active = rt; tex.ReadPixels(new UnityEngine.Rect(0, 0, 1280, 720), 0, 0); tex.Apply(); UnityEngine.RenderTexture.active = prev;
        System.IO.File.WriteAllBytes(outDir + "/" + tag + "_" + names[i] + ".jpg", UnityEngine.ImageConversion.EncodeToJPG(tex, 88));
    }
}
finally { UnityEngine.Object.DestroyImmediate(camGo); rt.Release(); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(tex); }
return "rendered " + tag;
