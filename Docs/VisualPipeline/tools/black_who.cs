// Diagnostic, restores everything: from view points aimed at the black wall blocks, reports which renderer owns the pixel.
var sb = new System.Text.StringBuilder();
var a = 21.4f * UnityEngine.Mathf.Deg2Rad; var U = new UnityEngine.Vector3(UnityEngine.Mathf.Cos(a), 0, UnityEngine.Mathf.Sin(a)); var V = new UnityEngine.Vector3(-UnityEngine.Mathf.Sin(a), 0, UnityEngine.Mathf.Cos(a));
System.Func<float, float, float, UnityEngine.Vector3> W = (u, y, v) => new UnityEngine.Vector3(-2312f, 111.68f, 595.6f) + U * u + V * v + UnityEngine.Vector3.up * y;
var tests = new[] { new[] { -8.1f, 1.6f, -5f, -6.4f, 1.6f, -5f }, new[] { -10.6f, 1.6f, -3f, -12f, 1.6f, -6f }, new[] { -2.8f, 1.6f, -27.5f, -4.5f, 1.6f, -29f }, new[] { 1.8f, 1.6f, -29.8f, -2.2f, 1.6f, -30.5f } };
var camGo = new UnityEngine.GameObject("__T"); camGo.hideFlags = UnityEngine.HideFlags.HideAndDontSave;
var rt = new UnityEngine.RenderTexture(32, 32, 24, UnityEngine.RenderTextureFormat.ARGB32, UnityEngine.RenderTextureReadWrite.sRGB); var tex = new UnityEngine.Texture2D(32, 32, UnityEngine.TextureFormat.RGB24, false);
try {
    var cam = camGo.AddComponent<UnityEngine.Camera>(); var acd = camGo.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>(); acd.renderPostProcessing = false;
    cam.targetTexture = rt; cam.fieldOfView = 10; cam.nearClipPlane = 0.03f; cam.farClipPlane = 60f;
    System.Func<string> shot = () => { cam.Render(); var prev = UnityEngine.RenderTexture.active; UnityEngine.RenderTexture.active = rt; tex.ReadPixels(new UnityEngine.Rect(0, 0, 32, 32), 0, 0); tex.Apply(); UnityEngine.RenderTexture.active = prev; return UnityEngine.ColorUtility.ToHtmlStringRGB(tex.GetPixel(16, 16)); };
    var all = UnityEngine.Object.FindObjectsOfType<UnityEngine.MeshRenderer>(false);
    foreach (var t in tests) {
        var o = W(t[0], t[1], t[2]); var e = W(t[3], t[4], t[5]); var ray = new UnityEngine.Ray(o, (e - o).normalized);
        camGo.transform.position = o; camGo.transform.rotation = UnityEngine.Quaternion.LookRotation(ray.direction, UnityEngine.Vector3.up);
        var baseC = shot(); sb.AppendLine("view " + t[0] + "," + t[2] + " base " + baseC);
        foreach (var r in all) {
            if (!r.enabled) continue; float dist; var b = r.bounds; b.Expand(0.1f); if (!b.IntersectRay(ray, out dist) || dist > 30f) continue;
            r.enabled = false; var c = shot(); r.enabled = true;
            if (c != baseC) sb.AppendLine("  " + r.name + " -> " + c + " parent=" + r.transform.parent.name + " mat=" + r.sharedMaterial.name + " gi=" + UnityEditor.GameObjectUtility.GetStaticEditorFlags(r.gameObject) + " lm=" + r.lightmapIndex);
        }
    }
} finally { UnityEngine.Object.DestroyImmediate(camGo); rt.Release(); UnityEngine.Object.DestroyImmediate(rt); UnityEngine.Object.DestroyImmediate(tex); }
return sb.ToString();
