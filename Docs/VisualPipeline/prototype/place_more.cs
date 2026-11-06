// Add the second batch of props, fix lightmap sampling on generated meshes, and tidy lights. Rebuilds only objects named here.
if (UnityEditor.EditorApplication.isPlaying) return "skip: play mode";
var sb = new System.Text.StringBuilder();
const string Dir = "Assets/Art/Generated/NoahRoom";
var root = UnityEngine.GameObject.Find("Floor17/Apartment_17F01/VisualPass_NoahRoom").transform;
var props = root.Find("Props");
var GI = UnityEditor.StaticEditorFlags.ContributeGI | UnityEditor.StaticEditorFlags.ReflectionProbeStatic;
System.Func<float, float, float, UnityEngine.Vector3> Wp = (u, y, v) => root.TransformPoint(new UnityEngine.Vector3(u, y, v));

foreach (var n in new[] { "Prop_Plant", "Prop_Curtain_A", "Prop_Curtain_B", "Prop_Curtain_C", "Prop_CurtainRod", "Prop_AlarmClock", "Prop_PhotoFrame", "Prop_ToyBlocks", "Prop_FloorCushion", "Prop_LaundryBasket", "Prop_CoatHooks" })
{
    var o = props.Find(n); if (o != null) UnityEngine.Object.DestroyImmediate(o.gameObject);
}

System.Func<string, string, float, float, float, float, float, bool, float, UnityEngine.GameObject> place = (id, name, size, u, y, v, yaw, solid, lm) => {
    var src = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(Dir + "/" + id + "/" + id + ".fbx");
    if (src == null) { sb.AppendLine("missing model " + id); return null; }
    var go = (UnityEngine.GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(src, props);
    go.name = name;
    var mf = go.GetComponentInChildren<UnityEngine.MeshFilter>();
    var b = mf.sharedMesh.bounds;
    float s = size / UnityEngine.Mathf.Max(b.size.x, UnityEngine.Mathf.Max(b.size.y, b.size.z));
    go.transform.localRotation = UnityEngine.Quaternion.Euler(0, yaw, 0);
    go.transform.localScale = UnityEngine.Vector3.one * s;
    go.transform.localPosition = UnityEngine.Vector3.zero;
    var r = go.GetComponentInChildren<UnityEngine.MeshRenderer>();
    var wb = r.bounds; var target = Wp(u, y, v);
    go.transform.position += new UnityEngine.Vector3(target.x - wb.center.x, target.y - wb.min.y, target.z - wb.center.z);
    r.scaleInLightmap = lm;
    UnityEditor.GameObjectUtility.SetStaticEditorFlags(r.gameObject, GI);
    if (solid) { var c = r.gameObject.AddComponent<UnityEngine.BoxCollider>(); c.center = b.center; c.size = b.size; }
    sb.AppendLine(name + " size=" + (b.size.x * s).ToString("0.00") + "x" + (b.size.y * s).ToString("0.00") + "x" + (b.size.z * s).ToString("0.00"));
    return go;
};
System.Func<string, float> topOf = n => props.Find(n).GetComponentInChildren<UnityEngine.Renderer>().bounds.max.y - root.position.y;

place("plant", "Prop_Plant", 1.05f, 1.62f, 0f, -0.42f, 40f, false, 0.7f);
System.Action<string, float, float> curtainPanel = (name, v, width) => {
    var go = place("curtain", name, 2.40f, 1.86f, 0.06f, v, -90f, false, 0.9f);
    if (go == null) return;
    var r = go.GetComponentInChildren<UnityEngine.MeshRenderer>();
    var ls = go.transform.localScale; float nowW = go.GetComponentInChildren<UnityEngine.MeshFilter>().sharedMesh.bounds.size.x * ls.x;
    go.transform.localScale = new UnityEngine.Vector3(ls.x * width / nowW, ls.y, ls.z);
    var c = r.bounds.center; var target = Wp(1.86f, 0.06f, v);
    go.transform.position += new UnityEngine.Vector3(target.x - c.x, target.y - r.bounds.min.y, target.z - c.z);
};
curtainPanel("Prop_Curtain_A", -1.70f, 0.40f);
curtainPanel("Prop_Curtain_B", -0.52f, 0.52f);
curtainPanel("Prop_Curtain_C", 1.72f, 0.36f);
place("alarm_clock", "Prop_AlarmClock", 0.14f, 0.62f, topOf("Prop_BedsideTable"), 1.44f, 200f, false, 0.3f);
place("photo_frame", "Prop_PhotoFrame", 0.20f, 1.76f, topOf("Prop_Dresser"), 0.52f, -110f, false, 0.3f);
place("toy_blocks", "Prop_ToyBlocks", 0.34f, 0.15f, 0.018f, -0.86f, 30f, false, 0.4f);
place("floor_cushion", "Prop_FloorCushion", 0.62f, 1.05f, 0.0f, -1.28f, 0f, false, 0.6f);
place("laundry_basket", "Prop_LaundryBasket", 0.48f, -2.0f, 0f, -0.46f, 0f, false, 0.6f);
place("coat_hooks", "Prop_CoatHooks", 0.80f, -1.15f, 1.02f, -1.815f, 0f, false, 0.7f);

// curtain rod
var trim = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(Dir + "/shared/M_Noah_Trim.mat");
var rod = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cube); rod.name = "Prop_CurtainRod"; UnityEngine.Object.DestroyImmediate(rod.GetComponent<UnityEngine.Collider>());
rod.transform.SetParent(props, false); rod.transform.localPosition = new UnityEngine.Vector3(1.86f, 2.50f, -0.02f); rod.transform.localScale = new UnityEngine.Vector3(0.02f, 0.02f, 3.74f);
var rr = rod.GetComponent<UnityEngine.MeshRenderer>(); rr.sharedMaterial = trim; rr.scaleInLightmap = 0.5f; UnityEditor.GameObjectUtility.SetStaticEditorFlags(rod, GI);

// generated meshes have hidden inner faces; relax the validity test so visible texels are not thrown away
var lpPath = Dir + "/shared/LP_GeneratedProps.giparams";
var lp = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.LightmapParameters>(lpPath);
if (lp == null) { lp = new UnityEditor.LightmapParameters(); UnityEditor.AssetDatabase.CreateAsset(lp, lpPath); }
lp.backFaceTolerance = 0.05f; lp.pushoff = 0.012f; lp.antiAliasingSamples = 8; UnityEditor.EditorUtility.SetDirty(lp);
int assigned = 0;
foreach (var r in props.GetComponentsInChildren<UnityEngine.MeshRenderer>(true))
{
    var so = new UnityEditor.SerializedObject(r); var p = so.FindProperty("m_LightmapParameters");
    if (p != null) { p.objectReferenceValue = lp; so.ApplyModifiedPropertiesWithoutUndo(); assigned++; }
}

// only there to fit the current character; the scene does not need it
var boy = root.Find("Lights/Light_BoyFill"); if (boy != null) UnityEngine.Object.DestroyImmediate(boy.gameObject);

UnityEditor.AssetDatabase.SaveAssets();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
sb.AppendLine("params assigned=" + assigned + " props=" + props.childCount);
return sb.ToString();
