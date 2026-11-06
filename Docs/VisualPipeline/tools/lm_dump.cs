// Read-only: cuts one object's rectangle out of its lightmap and draws its lightmap-UV triangles on top, saved as PNG, to see whether a pattern follows the UV layout.
var name = "OBJ"; UnityEngine.MeshRenderer r = null;
foreach (var x in UnityEngine.Object.FindObjectsOfType<UnityEngine.MeshRenderer>()) if (x.name == name && x.enabled) r = x;
if (r == null) return "no " + name;
var lm = UnityEngine.LightmapSettings.lightmaps[r.lightmapIndex].lightmapColor; var so = r.lightmapScaleOffset;
var rt = UnityEngine.RenderTexture.GetTemporary(lm.width, lm.height, 0, UnityEngine.RenderTextureFormat.ARGBHalf, UnityEngine.RenderTextureReadWrite.Linear);
UnityEngine.Graphics.Blit(lm, rt);
int x0 = (int)(so.z * lm.width), y0 = (int)(so.w * lm.height), w = (int)(so.x * lm.width), h = (int)(so.y * lm.height);
var prev = UnityEngine.RenderTexture.active; UnityEngine.RenderTexture.active = rt;
var tex = new UnityEngine.Texture2D(w, h, UnityEngine.TextureFormat.RGBAHalf, false, true); tex.ReadPixels(new UnityEngine.Rect(x0, y0, w, h), 0, 0); tex.Apply();
UnityEngine.RenderTexture.active = prev; UnityEngine.RenderTexture.ReleaseTemporary(rt);
var outT = new UnityEngine.Texture2D(w, h, UnityEngine.TextureFormat.RGB24, false);
var px = tex.GetPixels(); for (int i = 0; i < px.Length; i++) { var c = px[i]; float l = UnityEngine.Mathf.Max(c.r, UnityEngine.Mathf.Max(c.g, c.b)); var t = c * (0.9f / UnityEngine.Mathf.Max(0.0001f, l)) * UnityEngine.Mathf.Pow(UnityEngine.Mathf.Clamp01(l / 0.8f), 0.45f); t.a = 1; px[i] = t; }
outT.SetPixels(px);
var m = r.GetComponent<UnityEngine.MeshFilter>().sharedMesh; var uv = m.uv2.Length > 0 ? m.uv2 : m.uv; var tris = m.triangles;
System.Action<UnityEngine.Vector2, UnityEngine.Vector2> Line = (a, b) => { int n = (int)(UnityEngine.Vector2.Distance(a, b) * UnityEngine.Mathf.Max(w, h)) + 2; for (int i = 0; i <= n; i++) { var p = UnityEngine.Vector2.Lerp(a, b, i / (float)n); int xx = (int)(p.x * (w - 1)), yy = (int)(p.y * (h - 1)); if (xx >= 0 && yy >= 0 && xx < w && yy < h) outT.SetPixel(xx, yy, UnityEngine.Color.red); } };
for (int i = 0; i < tris.Length; i += 3) { Line(uv[tris[i]], uv[tris[i + 1]]); Line(uv[tris[i + 1]], uv[tris[i + 2]]); Line(uv[tris[i + 2]], uv[tris[i]]); }
outT.Apply();
System.IO.File.WriteAllBytes(@"D:/UGit/GAME-CBY/Docs/VisualPipeline/tools/_work/views/lm_" + name + ".png", UnityEngine.ImageConversion.EncodeToPNG(outT));
UnityEngine.Object.DestroyImmediate(tex); UnityEngine.Object.DestroyImmediate(outT);
return name + " rect " + w + "x" + h + " tris " + tris.Length / 3;
