// Play mode: report stage, active camera, Noah's real pose in room coordinates, and render through the active game camera.
var outDir = @"D:/UGit/GAME-CBY/Docs/VisualPipeline/prototype/_work/noah_views";
var tag = "TAG_PLACEHOLDER";
var sb = new System.Text.StringBuilder();
var t = System.Type.GetType("MinLoopFlowController, Assembly-CSharp");
var flow = UnityEngine.Object.FindObjectOfType(t);
var stage = flow != null ? t.GetField("currentStage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) : null;
sb.AppendLine("playing=" + UnityEngine.Application.isPlaying + " stage=" + (stage != null ? stage.GetValue(flow) : "?") + " ambSky=" + UnityEngine.RenderSettings.ambientSkyColor + " amb=" + UnityEngine.RenderSettings.ambientLight);
var rootGo = UnityEngine.GameObject.Find("Floor17/Apartment_17F01/VisualPass_NoahRoom");
var root = rootGo.transform;
UnityEngine.Camera game = null;
foreach (var c in UnityEngine.Camera.allCameras) { if (c.targetTexture == null && (game == null || c.depth > game.depth)) game = c; }
if (game != null) { var lp = root.InverseTransformPoint(game.transform.position); sb.AppendLine("camera=" + game.name + " parent=" + (game.transform.parent ? game.transform.parent.name : "-") + " room=(" + lp.x.ToString("0.00") + "," + lp.y.ToString("0.00") + "," + lp.z.ToString("0.00") + ") fov=" + game.fieldOfView.ToString("0")); }
var noah = UnityEngine.GameObject.Find("Actor_Noah_17F01");
if (noah != null)
{
    foreach (var s in noah.GetComponentsInChildren<UnityEngine.SkinnedMeshRenderer>(true))
    {
        if (s.bones == null || s.bones.Length == 0) continue;
        float minY = 99, maxY = -99, minU = 99, maxU = -99, minV = 99, maxV = -99;
        foreach (var b in s.bones) { if (b == null) continue; var p = root.InverseTransformPoint(b.position); minY = UnityEngine.Mathf.Min(minY, p.y); maxY = UnityEngine.Mathf.Max(maxY, p.y); minU = UnityEngine.Mathf.Min(minU, p.x); maxU = UnityEngine.Mathf.Max(maxU, p.x); minV = UnityEngine.Mathf.Min(minV, p.z); maxV = UnityEngine.Mathf.Max(maxV, p.z); }
        sb.AppendLine(s.name + " bones=" + s.bones.Length + " active=" + s.gameObject.activeInHierarchy + " y=" + minY.ToString("0.00") + ".." + maxY.ToString("0.00") + " u=" + minU.ToString("0.00") + ".." + maxU.ToString("0.00") + " v=" + minV.ToString("0.00") + ".." + maxV.ToString("0.00"));
        foreach (var b in s.bones) { if (b == null) continue; var n = b.name.ToLowerInvariant(); if (n.Contains("foot") || n.Contains("knee") || n.Contains("leg") || n.Contains("head") || n.Contains("hips") || n.Contains("pelvis") || n.Contains("hand")) { var p = root.InverseTransformPoint(b.position); sb.AppendLine("  " + b.name + " (" + p.x.ToString("0.00") + "," + p.y.ToString("0.00") + "," + p.z.ToString("0.00") + ")"); } }
    }
}
if (game != null)
{
    var rt = new UnityEngine.RenderTexture(1600, 900, 24, UnityEngine.RenderTextureFormat.ARGB32, UnityEngine.RenderTextureReadWrite.sRGB); rt.antiAliasing = 4;
    var tex = new UnityEngine.Texture2D(1600, 900, UnityEngine.TextureFormat.RGB24, false);
    var old = game.targetTexture; game.targetTexture = rt; game.Render(); game.targetTexture = old;
    var prev = UnityEngine.RenderTexture.active; UnityEngine.RenderTexture.active = rt; tex.ReadPixels(new UnityEngine.Rect(0, 0, 1600, 900), 0, 0); tex.Apply(); UnityEngine.RenderTexture.active = prev;
    System.IO.File.WriteAllBytes(outDir + "/" + tag + ".jpg", UnityEngine.ImageConversion.EncodeToJPG(tex, 90));
    rt.Release(); UnityEngine.Object.Destroy(rt); UnityEngine.Object.Destroy(tex);
}
return sb.ToString();
