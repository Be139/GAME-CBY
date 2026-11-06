// Noah's room pass, stage 2: place generated props, wall art and the rug. Rebuilds only Props and WallArt under VisualPass_NoahRoom.
if (UnityEditor.EditorApplication.isPlaying) return "skip: play mode";
var sb = new System.Text.StringBuilder();
const string Dir = "Assets/Art/Generated/NoahRoom";
var root = UnityEngine.GameObject.Find("Floor17/Apartment_17F01/VisualPass_NoahRoom").transform;
var props = root.Find("Props"); var art = root.Find("WallArt");
while (props.childCount > 0) UnityEngine.Object.DestroyImmediate(props.GetChild(0).gameObject);
while (art.childCount > 0) UnityEngine.Object.DestroyImmediate(art.GetChild(0).gameObject);

var GI = UnityEditor.StaticEditorFlags.ContributeGI | UnityEditor.StaticEditorFlags.ReflectionProbeStatic;
System.Func<float, float, float, UnityEngine.Vector3> Wp = (u, y, v) => root.TransformPoint(new UnityEngine.Vector3(u, y, v));

// id, instance name, longest side in metres, u, base height, v, yaw in room space, collider, lightmap scale, depth scale
System.Func<string, string, float, float, float, float, float, bool, float, float, UnityEngine.GameObject> placeZ = (id, name, size, u, y, v, yaw, solid, lm, zs) => {
    var src = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(Dir + "/" + id + "/" + id + ".fbx");
    if (src == null) { sb.AppendLine("missing model " + id); return null; }
    var go = (UnityEngine.GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(src, props);
    go.name = name;
    var mf = go.GetComponentInChildren<UnityEngine.MeshFilter>();
    var b = mf.sharedMesh.bounds;
    float s = size / UnityEngine.Mathf.Max(b.size.x, UnityEngine.Mathf.Max(b.size.y, b.size.z));
    go.transform.localRotation = UnityEngine.Quaternion.Euler(0, yaw, 0);
    go.transform.localScale = new UnityEngine.Vector3(s, s, s * zs);
    go.transform.localPosition = UnityEngine.Vector3.zero;
    var r = go.GetComponentInChildren<UnityEngine.MeshRenderer>();
    var wb = r.bounds; var target = Wp(u, y, v);
    go.transform.position += new UnityEngine.Vector3(target.x - wb.center.x, target.y - wb.min.y, target.z - wb.center.z);
    r.scaleInLightmap = lm;
    UnityEditor.GameObjectUtility.SetStaticEditorFlags(r.gameObject, GI);
    if (solid) { var c = r.gameObject.AddComponent<UnityEngine.BoxCollider>(); c.center = b.center; c.size = b.size; }
    sb.AppendLine(name + " size=" + (b.size.x * s).ToString("0.00") + "x" + (b.size.y * s).ToString("0.00") + "x" + (b.size.z * s * zs).ToString("0.00") + " top=" + (r.bounds.max.y - root.position.y).ToString("0.00"));
    return go;
};
System.Func<string, string, float, float, float, float, float, bool, float, UnityEngine.GameObject> place = (id, name, size, u, y, v, yaw, solid, lm) => placeZ(id, name, size, u, y, v, yaw, solid, lm, 1f);
System.Func<UnityEngine.GameObject, float> topOf = g => g == null ? 0.7f : g.GetComponentInChildren<UnityEngine.Renderer>().bounds.max.y - root.position.y;

var bed = place("bed", "Prop_Bed", 2.05f, -0.10f, 0f, 0.80f, 180f, true, 1f);
if (bed != null) { var c = bed.GetComponent<UnityEngine.BoxCollider>(); var bb = bed.GetComponent<UnityEngine.MeshFilter>().sharedMesh.bounds; float wantH = 0.50f / bed.transform.localScale.y; c.center = new UnityEngine.Vector3(bb.center.x, bb.min.y + wantH * 0.5f, bb.center.z); c.size = new UnityEngine.Vector3(bb.size.x, wantH, bb.size.z); }
var nightstand = place("bedside_table", "Prop_BedsideTable", 0.52f, 0.72f, 0f, 1.60f, 180f, true, 0.6f);
var lampGo = place("table_lamp", "Prop_TableLamp", 0.42f, 0.72f, topOf(nightstand), 1.60f, 0f, false, 0.4f);
if (lampGo != null) lampGo.GetComponentInChildren<UnityEngine.MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
placeZ("dresser", "Prop_Dresser", 0.95f, 1.74f, 0f, 0.78f, -90f, true, 0.7f, 0.72f);
place("toybox", "Prop_ToyBox", 0.62f, 1.60f, 0f, -1.40f, 205f, true, 0.5f);
var shelf = place("bookshelf", "Prop_Bookshelf", 1.55f, 0.35f, 0f, -1.67f, 0f, true, 0.8f);
var desk = place("desk", "Prop_Desk", 1.25f, -1.92f, 0f, 0.86f, 90f, true, 0.8f);
place("chair", "Prop_Chair", 0.80f, -1.33f, 0f, 0.92f, -100f, true, 0.5f);
place("backpack", "Prop_Backpack", 0.42f, -2.10f, 0f, 0.0f, 90f, false, 0.4f);
place("plush_dino", "Prop_PlushDino", 0.30f, -0.84f, 0f, 0.28f, 150f, false, 0.3f);
place("slippers", "Prop_Slippers", 0.27f, -0.86f, 0f, 0.82f, 75f, false, 0.3f);
float shelfTop = topOf(shelf); float deskTop = topOf(desk);
place("toy_rocket", "Prop_ToyRocket", 0.30f, -0.18f, shelfTop, -1.70f, 20f, false, 0.3f);
place("globe", "Prop_Globe", 0.28f, 0.88f, shelfTop, -1.70f, 150f, false, 0.3f);
place("book_stack", "Prop_Books_Rug", 0.27f, 0.52f, 0.018f, -0.42f, 35f, false, 0.3f);
place("book_stack", "Prop_Books_Desk", 0.27f, -2.0f, deskTop, 1.22f, 100f, false, 0.3f);
place("pencil_cup", "Prop_PencilCup", 0.15f, -2.08f, deskTop, 0.46f, 0f, false, 0.2f);
place("desk_lamp", "Prop_DeskLamp", 0.40f, -2.08f, deskTop, 0.80f, 90f, false, 0.3f);
var lamp = place("ceiling_lamp", "Prop_CeilingLamp", 0.42f, -0.13f, 2.87f, -0.02f, 0f, false, 0.3f);
if (lamp != null) { var r = lamp.GetComponentInChildren<UnityEngine.Renderer>(); lamp.transform.position += UnityEngine.Vector3.up * (root.position.y + 2.955f - r.bounds.max.y); }

// lamp shade glow
var lampMat = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(Dir + "/table_lamp/M_table_lamp.mat");
lampMat.EnableKeyword("_EMISSION"); lampMat.SetTexture("_EmissionMap", UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(Dir + "/table_lamp/table_lamp_emit.png"));
lampMat.SetColor("_EmissionColor", new UnityEngine.Color(1.9f, 1.45f, 1.0f)); lampMat.globalIlluminationFlags = UnityEngine.MaterialGlobalIlluminationFlags.EmissiveIsBlack;
UnityEditor.EditorUtility.SetDirty(lampMat);

// the blanket that covers Noah: a simple draped sheet sized to the bed, with a hump for his body
int nu = 30, nv = 36; float halfW = 0.585f, vFoot = -0.265f, vTop = 1.04f;
var verts = new UnityEngine.Vector3[(nu + 1) * (nv + 1)]; var uvs = new UnityEngine.Vector2[verts.Length]; var tris = new int[nu * nv * 6];
System.Func<float, float, float, float> sm = (a, b, x) => { float t = UnityEngine.Mathf.Clamp01((x - a) / (b - a)); return t * t * (3 - 2 * t); };
for (int j = 0; j <= nv; j++) for (int i = 0; i <= nu; i++)
{
    float du = -halfW + 2 * halfW * i / nu; float v = vFoot + (vTop - vFoot) * j / nv;
    float y = 0.515f;
    y += 0.125f * UnityEngine.Mathf.Exp(-(du * du) / (0.26f * 0.26f)) * sm(0.16f, 0.46f, v) * (1f - 0.35f * sm(0.82f, vTop, v));
    y += 0.016f * sm(0.86f, 0.90f, v);
    float side = sm(0.495f, 0.572f, UnityEngine.Mathf.Abs(du)); float foot = 1f - sm(vFoot + 0.012f, vFoot + 0.09f, v); float d = UnityEngine.Mathf.Max(side, foot);
    y = UnityEngine.Mathf.Lerp(y, 0.235f, d);
    y -= 0.05f * sm(vTop - 0.022f, vTop, v);
    verts[j * (nu + 1) + i] = new UnityEngine.Vector3(du, y, v); uvs[j * (nu + 1) + i] = new UnityEngine.Vector2((float)i / nu, (float)j / nv);
}
int ti = 0; for (int j = 0; j < nv; j++) for (int i = 0; i < nu; i++) { int a = j * (nu + 1) + i, b = a + 1, c = a + nu + 1, d = c + 1; tris[ti++] = a; tris[ti++] = c; tris[ti++] = b; tris[ti++] = b; tris[ti++] = c; tris[ti++] = d; }
var meshPath = Dir + "/shared/Mesh_NoahBlanket.asset";
var mesh = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(meshPath);
if (mesh == null) { mesh = new UnityEngine.Mesh(); UnityEditor.AssetDatabase.CreateAsset(mesh, meshPath); }
mesh.Clear(); mesh.name = "Mesh_NoahBlanket"; mesh.vertices = verts; mesh.uv = uvs; mesh.uv2 = uvs; mesh.triangles = tris; mesh.RecalculateNormals(); mesh.RecalculateTangents(); mesh.RecalculateBounds();
UnityEditor.EditorUtility.SetDirty(mesh);
var blanketM = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(Dir + "/shared/M_Noah_Blanket.mat");
if (blanketM == null) { blanketM = new UnityEngine.Material(UnityEngine.Shader.Find("Universal Render Pipeline/Lit")); UnityEditor.AssetDatabase.CreateAsset(blanketM, Dir + "/shared/M_Noah_Blanket.mat"); }
blanketM.SetColor("_BaseColor", new UnityEngine.Color(0.70f, 0.79f, 0.90f)); blanketM.SetFloat("_Smoothness", 0.1f); blanketM.SetFloat("_Metallic", 0f); UnityEditor.EditorUtility.SetDirty(blanketM);
var bl = new UnityEngine.GameObject("Prop_Blanket"); bl.transform.SetParent(props, false); bl.transform.localPosition = new UnityEngine.Vector3(-0.10f, 0f, 0f);
bl.AddComponent<UnityEngine.MeshFilter>().sharedMesh = mesh; var blr = bl.AddComponent<UnityEngine.MeshRenderer>(); blr.sharedMaterial = blanketM; blr.scaleInLightmap = 0.7f; UnityEditor.GameObjectUtility.SetStaticEditorFlags(bl, GI);

// flat art and rug
System.Func<string, string, float, UnityEngine.Material> texMat = (id, file, smooth) => {
    var folder = Dir + "/shared"; var path = folder + "/M_" + id + ".mat";
    var m = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(path);
    if (m == null) { m = new UnityEngine.Material(UnityEngine.Shader.Find("Universal Render Pipeline/Lit")); UnityEditor.AssetDatabase.CreateAsset(m, path); }
    m.SetTexture("_BaseMap", UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(folder + "/" + file)); m.SetFloat("_Smoothness", smooth); m.SetFloat("_Metallic", 0f);
    UnityEditor.EditorUtility.SetDirty(m); return m;
};
System.Action<string, UnityEngine.Material, UnityEngine.Vector3, float, float, float> quad = (name, m, local, w, h, yaw) => {
    var g = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Quad); g.name = name;
    UnityEngine.Object.DestroyImmediate(g.GetComponent<UnityEngine.Collider>());
    g.transform.SetParent(art, false); g.transform.localPosition = local; g.transform.localRotation = UnityEngine.Quaternion.Euler(0, yaw, 0); g.transform.localScale = new UnityEngine.Vector3(w, h, 1f);
    var r = g.GetComponent<UnityEngine.MeshRenderer>(); r.sharedMaterial = m; r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; r.receiveGI = UnityEngine.ReceiveGI.LightProbes;
    UnityEditor.GameObjectUtility.SetStaticEditorFlags(g, UnityEditor.StaticEditorFlags.ReflectionProbeStatic);
};
float wallA = 1.849f, wallC = -1.899f;
quad("Art_Rocket", texMat("art_rocket", "art_rocket.png", 0.1f), new UnityEngine.Vector3(-1.05f, 1.46f, wallA), 0.40f, 0.53f, 0f);
quad("Art_Robot", texMat("art_robot", "art_robot.png", 0.1f), new UnityEngine.Vector3(-0.48f, 1.53f, wallA), 0.40f, 0.53f, 0f);
quad("Art_House", texMat("art_house", "art_house.png", 0.1f), new UnityEngine.Vector3(0.12f, 1.42f, wallA), 0.50f, 0.375f, 0f);
quad("Poster_Space", texMat("poster_space", "poster_space.png", 0.15f), new UnityEngine.Vector3(0.35f, 1.62f, wallC), 0.62f, 0.93f, 180f);
var rugM = texMat("rug", "rug.png", 0.05f);
var rug = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cube); rug.name = "Prop_Rug";
UnityEngine.Object.DestroyImmediate(rug.GetComponent<UnityEngine.Collider>());
rug.transform.SetParent(props, false); rug.transform.localPosition = new UnityEngine.Vector3(-0.1f, 0.012f, -0.72f); rug.transform.localScale = new UnityEngine.Vector3(2.1f, 0.012f, 1.4f);
rug.GetComponent<UnityEngine.MeshRenderer>().sharedMaterial = rugM; UnityEditor.GameObjectUtility.SetStaticEditorFlags(rug, GI);

UnityEditor.AssetDatabase.SaveAssets();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
sb.AppendLine("props=" + props.childCount + " art=" + art.childCount);
return sb.ToString();
