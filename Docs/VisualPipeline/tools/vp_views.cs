// Read-only view sheet: VIEW_LIST holds name|x,y,z|lookX,lookY,lookZ|fov|post(0/1) entries separated by ';'.
var outDir = @"D:/UGit/GAME-CBY/Docs/VisualPipeline/tools/_work/views";
System.IO.Directory.CreateDirectory(outDir);
var list = "VIEW_LIST".Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries);
int W = 960, H = 540;
var camGo = new UnityEngine.GameObject("__VP_Cam"); camGo.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
var hidden = new System.Collections.Generic.List<UnityEngine.Renderer>();
if (!"VIEW_LIST".Contains("keepactors")) foreach (var r in UnityEngine.Object.FindObjectsOfType<UnityEngine.SkinnedMeshRenderer>()) if (r.enabled) { r.enabled = false; hidden.Add(r); }
var rt = new UnityEngine.RenderTexture(W, H, 24, UnityEngine.RenderTextureFormat.ARGB32, UnityEngine.RenderTextureReadWrite.sRGB); rt.antiAliasing = 4;
var tex = new UnityEngine.Texture2D(W, H, UnityEngine.TextureFormat.RGB24, false);
try {
    var cam = camGo.AddComponent<UnityEngine.Camera>();
    var acd = camGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
    acd.antialiasing = UnityEngine.Rendering.Universal.AntialiasingMode.SubpixelMorphologicalAntiAliasing; acd.volumeLayerMask = 1;
    cam.nearClipPlane = 0.03f; cam.farClipPlane = 3000f; cam.allowHDR = true; cam.targetTexture = rt;
    foreach (var e in list) {
        var f = e.Split('|'); var a = f[1].Split(','); var b = f[2].Split(',');
        System.Func<string[], UnityEngine.Vector3> v3 = s => new UnityEngine.Vector3(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2]));
        camGo.transform.position = v3(a); camGo.transform.LookAt(v3(b)); cam.fieldOfView = float.Parse(f[3]); acd.renderPostProcessing = f[4] == "1";
        cam.Render();
        var prev = UnityEngine.RenderTexture.active; UnityEngine.RenderTexture.active = rt; tex.ReadPixels(new UnityEngine.Rect(0, 0, W, H), 0, 0); tex.Apply(); UnityEngine.RenderTexture.active = prev;
        System.IO.File.WriteAllBytes(outDir + "/" + f[0] + ".jpg", UnityEngine.ImageConversion.EncodeToJPG(tex, 85));
    }
} finally { foreach (var r in hidden) r.enabled = true; UnityEngine.Object.DestroyImmediate(camGo); rt.Release(); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(tex); }
return "rendered " + list.Length;
