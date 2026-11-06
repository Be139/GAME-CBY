// Read-only: every old prop in floor coordinates (u along the windows of Noah's room, v along its headboard), plus a cut-away plan aligned to those axes.
var sb = new System.Text.StringBuilder();
var org = new UnityEngine.GameObject("__Frame"); org.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
org.transform.position = new UnityEngine.Vector3(-2312.0f, 111.68f, 595.6f); org.transform.rotation = UnityEngine.Quaternion.Euler(0, -21.4f, 0);
foreach (var apt in new[] { "Apartment_17F01", "Apartment_17F02", "Apartment_17F03", "Apartment_17F04" }) {
    var a = UnityEngine.GameObject.Find("Floor17/" + apt).transform; sb.AppendLine("# " + apt);
    foreach (UnityEngine.Transform t in a) {
        if (!t.gameObject.activeSelf || t.name.StartsWith("VisualPass")) continue;
        bool any = false; var b = new UnityEngine.Bounds();
        foreach (var mf in t.GetComponentsInChildren<UnityEngine.MeshFilter>(true)) {
            if (mf.sharedMesh == null) continue; var mb = mf.sharedMesh.bounds;
            for (int i = 0; i < 8; i++) {
                var c = new UnityEngine.Vector3(i % 2 == 0 ? mb.min.x : mb.max.x, i / 2 % 2 == 0 ? mb.min.y : mb.max.y, i / 4 == 0 ? mb.min.z : mb.max.z);
                var p = org.transform.InverseTransformPoint(mf.transform.TransformPoint(c));
                if (!any) { b = new UnityEngine.Bounds(p, UnityEngine.Vector3.zero); any = true; } else b.Encapsulate(p);
            }
        }
        if (!any) continue;
        sb.AppendLine(t.name + " " + b.center.x.ToString("0.0") + "," + b.center.z.ToString("0.0") + " " + b.size.x.ToString("0.0") + "x" + b.size.z.ToString("0.0") + " h" + b.max.y.ToString("0.0"));
    }
}
UnityEngine.Object.DestroyImmediate(org);
var outDir = @"D:/UGit/GAME-CBY/Docs/VisualPipeline/tools/_work/plans";
var camGo = new UnityEngine.GameObject("__Plan_Cam"); camGo.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
int W = 1600, H = 1600;
var rt = new UnityEngine.RenderTexture(W, H, 24, UnityEngine.RenderTextureFormat.ARGB32, UnityEngine.RenderTextureReadWrite.sRGB); rt.antiAliasing = 4;
var tex = new UnityEngine.Texture2D(W, H, UnityEngine.TextureFormat.RGB24, false);
try {
    var cam = camGo.AddComponent<UnityEngine.Camera>();
    var acd = camGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
    acd.renderPostProcessing = false; cam.orthographic = true; cam.allowHDR = false; cam.targetTexture = rt; cam.clearFlags = UnityEngine.CameraClearFlags.SolidColor; cam.backgroundColor = new UnityEngine.Color(0.05f, 0.05f, 0.08f);
    var U = new UnityEngine.Vector3(0.931f, 0, 0.365f).normalized; var V = new UnityEngine.Vector3(-0.365f, 0, 0.931f).normalized;
    var c0 = new UnityEngine.Vector3(-2312f, 113.2f, 595.6f) + U * (-6f) + V * (-18f);
    camGo.transform.position = c0; camGo.transform.rotation = UnityEngine.Quaternion.Euler(90, -21.4f, 0);
    cam.orthographicSize = 24f; cam.nearClipPlane = 0.01f; cam.farClipPlane = 5f;
    cam.Render();
    var prev = UnityEngine.RenderTexture.active; UnityEngine.RenderTexture.active = rt; tex.ReadPixels(new UnityEngine.Rect(0, 0, W, H), 0, 0); tex.Apply(); UnityEngine.RenderTexture.active = prev;
    System.IO.File.WriteAllBytes(outDir + "/uv.jpg", UnityEngine.ImageConversion.EncodeToJPG(tex, 85));
} finally { UnityEngine.Object.DestroyImmediate(camGo); rt.Release(); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(tex); }
return sb.ToString();
