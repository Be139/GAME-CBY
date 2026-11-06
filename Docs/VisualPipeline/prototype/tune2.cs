// Second pass, based on what the real game camera showed in Play mode.
if (UnityEditor.EditorApplication.isPlaying) return "skip: play mode";
var sb = new System.Text.StringBuilder();
const string Dir = "Assets/Art/Generated/NoahRoom";
var root = UnityEngine.GameObject.Find("Floor17/Apartment_17F01/VisualPass_NoahRoom").transform;
var apt = root.parent;
var GI = UnityEditor.StaticEditorFlags.ContributeGI | UnityEditor.StaticEditorFlags.ReflectionProbeStatic;

// old curtains stayed unbaked and read as a black band under the windows; the blinds replace them
var curtain = apt.Find("CurtainSet_19"); if (curtain != null) curtain.gameObject.SetActive(false);

// lightmap UV margin: generated meshes occupy a small part of the atlas, so charts need generous spacing
foreach (var dir in UnityEditor.AssetDatabase.GetSubFolders(Dir))
{
    var id = System.IO.Path.GetFileName(dir); if (id == "shared") continue;
    var mi = UnityEditor.AssetImporter.GetAtPath(dir + "/" + id + ".fbx") as UnityEditor.ModelImporter; if (mi == null) continue;
    if (mi.secondaryUVPackMargin != 48f) { mi.secondaryUVPackMargin = 48f; mi.SaveAndReimport(); }
}
foreach (var p in new[] { "Prop_Bookshelf", "Prop_Dresser", "Prop_Desk", "Prop_Bed" }) { var t = root.Find("Props/" + p); if (t != null) t.GetComponentInChildren<UnityEngine.MeshRenderer>().scaleInLightmap = 1.5f; }
foreach (var p in new[] { "Prop_BedsideTable", "Prop_Chair", "Prop_ToyBox" }) { var t = root.Find("Props/" + p); if (t != null) t.GetComponentInChildren<UnityEngine.MeshRenderer>().scaleInLightmap = 1.1f; }
foreach (var r in root.Find("Trim").GetComponentsInChildren<UnityEngine.MeshRenderer>(true)) r.scaleInLightmap = 1.6f;

// blinds reach the sill
var wallTrim = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(Dir + "/shared/M_Noah_Trim.mat");
var blindM = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(Dir + "/shared/M_Noah_Blind.mat");
var blinds = root.Find("Blinds");
while (blinds.childCount > 0) UnityEngine.Object.DestroyImmediate(blinds.GetChild(0).gameObject);
System.Func<string, UnityEngine.Transform, UnityEngine.Material, UnityEngine.Vector3, UnityEngine.Vector3, float, UnityEngine.GameObject> box = (n, parent, m, a, b, lmScale) => {
    var g = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cube); g.name = n;
    UnityEngine.Object.DestroyImmediate(g.GetComponent<UnityEngine.Collider>());
    g.transform.SetParent(parent, false); g.transform.localPosition = (a + b) * 0.5f;
    g.transform.localScale = new UnityEngine.Vector3(UnityEngine.Mathf.Abs(b.x - a.x), UnityEngine.Mathf.Abs(b.y - a.y), UnityEngine.Mathf.Abs(b.z - a.z));
    var r = g.GetComponent<UnityEngine.MeshRenderer>(); r.sharedMaterial = m; r.scaleInLightmap = lmScale;
    UnityEditor.GameObjectUtility.SetStaticEditorFlags(g, GI);
    return g;
};
float ux1 = 2.0f, wy0 = 0.87f, wy1 = 2.47f;
System.Action<string, float, float> blind = (n, a, b) => {
    var g = new UnityEngine.GameObject(n).transform; g.SetParent(blinds, false);
    box(n + "_Rail", g, wallTrim, new UnityEngine.Vector3(ux1 - 0.075f, wy1 - 0.045f, a), new UnityEngine.Vector3(ux1 - 0.02f, wy1, b), 0.6f);
    for (float y = wy1 - 0.09f; y > wy0 + 0.02f; y -= 0.068f)
    {
        var s = box(n + "_Slat", g, blindM, new UnityEngine.Vector3(ux1 - 0.085f, y - 0.0035f, a + 0.01f), new UnityEngine.Vector3(ux1 - 0.015f, y + 0.0035f, b - 0.01f), 0.05f);
        s.transform.localRotation = UnityEngine.Quaternion.Euler(0, 0, 26f);
        UnityEditor.GameObjectUtility.SetStaticEditorFlags(s, UnityEditor.StaticEditorFlags.ContributeGI);
        s.GetComponent<UnityEngine.MeshRenderer>().receiveGI = UnityEngine.ReceiveGI.LightProbes;
    }
};
blind("Blind_W1", -1.67f, -0.87f); blind("Blind_W2", -0.17f, 1.73f);

// deeper night glass
var glass = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(Dir + "/shared/M_Noah_NightGlass.mat");
glass.SetColor("_BaseColor", new UnityEngine.Color(0.025f, 0.05f, 0.14f, 0.76f)); UnityEditor.EditorUtility.SetDirty(glass);

// wall art joins the bake
foreach (UnityEngine.Transform t in root.Find("WallArt"))
{
    if (!t.name.StartsWith("Art_") && !t.name.StartsWith("Poster_")) continue;
    var r = t.GetComponent<UnityEngine.MeshRenderer>(); r.receiveGI = UnityEngine.ReceiveGI.Lightmaps; r.scaleInLightmap = 0.8f;
    UnityEditor.GameObjectUtility.SetStaticEditorFlags(t.gameObject, GI);
}

// narrower floor planks
var floorM = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(Dir + "/shared/M_Noah_Floor.mat");
floorM.SetTextureScale("_BaseMap", new UnityEngine.Vector2(3.4f, 3.0f)); UnityEditor.EditorUtility.SetDirty(floorM);

// blanket rebuilt from Noah's measured runtime pose: pelvis u -0.09, y 0.55, v 0.87; feet at v 0.49; hands rest at v 1.0
var mesh = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(Dir + "/shared/Mesh_NoahBlanket.asset");
int nu = 30, nv = 36; float halfW = 0.585f, vFoot = -0.265f, vTop = 0.965f;
var verts = mesh.vertices;
System.Func<float, float, float, float> sm = (a, b, x) => { float t = UnityEngine.Mathf.Clamp01((x - a) / (b - a)); return t * t * (3 - 2 * t); };
for (int j = 0; j <= nv; j++) for (int i = 0; i <= nu; i++)
{
    float du = -halfW + 2 * halfW * i / nu; float v = vFoot + (vTop - vFoot) * j / nv;
    float y = 0.515f;
    float body = UnityEngine.Mathf.Exp(-(du * du) / (0.25f * 0.25f));
    y += 0.165f * body * sm(0.22f, 0.50f, v);
    y += 0.016f * sm(0.80f, 0.84f, v);
    float side = sm(0.495f, 0.572f, UnityEngine.Mathf.Abs(du)); float foot = 1f - sm(vFoot + 0.012f, vFoot + 0.09f, v); float d = UnityEngine.Mathf.Max(side, foot);
    y = UnityEngine.Mathf.Lerp(y, 0.235f, d);
    y -= 0.05f * sm(vTop - 0.022f, vTop, v);
    verts[j * (nu + 1) + i] = new UnityEngine.Vector3(du, y, v);
}
mesh.vertices = verts; mesh.RecalculateNormals(); mesh.RecalculateTangents(); mesh.RecalculateBounds(); UnityEditor.EditorUtility.SetDirty(mesh);
var blanketM = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(Dir + "/shared/M_Noah_Blanket.mat");
blanketM.SetColor("_BaseColor", new UnityEngine.Color(0.40f, 0.50f, 0.65f)); UnityEditor.EditorUtility.SetDirty(blanketM);
root.Find("Props/Prop_Blanket").GetComponent<UnityEngine.MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

// lights
var warm = new UnityEngine.Color(1f, 0.72f, 0.46f);
var lights = root.Find("Lights");
System.Action<UnityEngine.Light, UnityEngine.Vector3, float, float> aim = (l, target, outer, inner) => { l.spotAngle = outer; l.innerSpotAngle = inner; l.transform.LookAt(root.TransformPoint(target)); };
var moon = lights.Find("Light_Moon").GetComponent<UnityEngine.Light>(); moon.transform.localPosition = new UnityEngine.Vector3(4.4f, 2.9f, -0.4f); moon.intensity = 17f; moon.color = new UnityEngine.Color(0.50f, 0.63f, 1f); aim(moon, new UnityEngine.Vector3(0f, 0.7f, 1.2f), 62f, 42f);
var lamp = lights.Find("Light_TableLamp").GetComponent<UnityEngine.Light>(); lamp.transform.localPosition = new UnityEngine.Vector3(0.67f, 0.76f, 1.53f); lamp.intensity = 1.0f; aim(lamp, new UnityEngine.Vector3(-0.5f, 0.3f, 0.6f), 125f, 40f);
lights.Find("Bake_LampGlow").GetComponent<UnityEngine.Light>().intensity = 0.42f;
foreach (var n in new[] { "Bake_WindowSky_1", "Bake_WindowSky_2" }) { var l = lights.Find(n).GetComponent<UnityEngine.Light>(); l.intensity = 1.0f; l.color = new UnityEngine.Color(0.48f, 0.58f, 0.90f); }
lights.Find("Bake_NeonBounce").GetComponent<UnityEngine.Light>().intensity = 0.5f;
foreach (var n in new[] { "Bake_LampFill", "Light_BoyFill" }) { var o = lights.Find(n); if (o != null) UnityEngine.Object.DestroyImmediate(o.gameObject); }
System.Func<string, UnityEngine.LightType, UnityEngine.LightmapBakeType, UnityEngine.Vector3, UnityEngine.Color, float, float, UnityEngine.Light> add = (n, type, mode, pos, col, I, R) => {
    var g = new UnityEngine.GameObject(n); g.transform.SetParent(lights, false); g.transform.localPosition = pos;
    var l = g.AddComponent<UnityEngine.Light>(); l.type = type; l.lightmapBakeType = mode; l.color = col; l.intensity = I; l.range = R;
    g.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalLightData>();
    return l;
};
var fill = add("Bake_LampFill", UnityEngine.LightType.Point, UnityEngine.LightmapBakeType.Baked, new UnityEngine.Vector3(0.35f, 1.15f, 1.05f), warm, 0.75f, 5f); fill.shadows = UnityEngine.LightShadows.Soft; fill.shadowRadius = 0.25f;
var boy = add("Light_BoyFill", UnityEngine.LightType.Point, UnityEngine.LightmapBakeType.Realtime, new UnityEngine.Vector3(0.1f, 1.05f, 1.15f), new UnityEngine.Color(1f, 0.82f, 0.66f), 0.16f, 1.2f); boy.shadows = UnityEngine.LightShadows.None;

UnityEditor.AssetDatabase.SaveAssets();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
sb.AppendLine("blinds=" + blinds.childCount + " lights=" + lights.childCount);
return sb.ToString();
