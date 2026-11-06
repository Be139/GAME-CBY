// Noah's room pass, stage 1: independent inner shell, trims, blinds; hide replaced props. Safe to re-run: rebuilds only its own root.
if (UnityEditor.EditorApplication.isPlaying) return "skip: play mode";
var sb = new System.Text.StringBuilder();
const string Dir = "Assets/Art/Generated/NoahRoom/shared";
UnityEditor.AssetDatabase.Refresh();

System.Func<string, UnityEngine.Color, float, UnityEngine.Material> mat = (name, col, smooth) => {
    var path = Dir + "/" + name + ".mat";
    var m = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(path);
    if (m == null) { m = new UnityEngine.Material(UnityEngine.Shader.Find("Universal Render Pipeline/Lit")); UnityEditor.AssetDatabase.CreateAsset(m, path); }
    m.SetColor("_BaseColor", col); m.SetFloat("_Smoothness", smooth); m.SetFloat("_Metallic", 0f); m.SetFloat("_EnvironmentReflections", 1f);
    UnityEditor.EditorUtility.SetDirty(m);
    return m;
};
var wallM = mat("M_Noah_Wall", new UnityEngine.Color(0.47f, 0.52f, 0.62f), 0.1f);
var ceilM = mat("M_Noah_Ceiling", new UnityEngine.Color(0.60f, 0.62f, 0.67f), 0.05f);
var trimM = mat("M_Noah_Trim", new UnityEngine.Color(0.78f, 0.77f, 0.74f), 0.2f);
var blindM = mat("M_Noah_Blind", new UnityEngine.Color(0.70f, 0.70f, 0.70f), 0.2f);
var floorM = mat("M_Noah_Floor", UnityEngine.Color.white, 0.38f);
var planks = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(Dir + "/T_FloorPlanks.png");
floorM.SetTexture("_BaseMap", planks); floorM.SetTextureScale("_BaseMap", new UnityEngine.Vector2(2.2f, 1.9f));

var apt = UnityEngine.GameObject.Find("Floor17/Apartment_17F01");
var oldRoot = apt.transform.Find("VisualPass_NoahRoom");
if (oldRoot != null) UnityEngine.Object.DestroyImmediate(oldRoot.gameObject);
var root = new UnityEngine.GameObject("VisualPass_NoahRoom");
root.transform.SetParent(apt.transform, true);
root.transform.position = new UnityEngine.Vector3(-2312.0f, 111.68f, 595.6f);
root.transform.rotation = UnityEngine.Quaternion.Euler(0, -21.4f, 0);
root.transform.localScale = UnityEngine.Vector3.one;
// parent scale may not be one; keep world scale true
var ls = root.transform.lossyScale; root.transform.localScale = new UnityEngine.Vector3(1f / ls.x, 1f / ls.y, 1f / ls.z);

System.Func<string, UnityEngine.Transform> group = n => { var g = new UnityEngine.GameObject(n); g.transform.SetParent(root.transform, false); return g.transform; };
var shell = group("Shell"); var trim = group("Trim"); var blinds = group("Blinds"); group("Props"); group("Lights"); group("WallArt");

var GI = UnityEditor.StaticEditorFlags.ContributeGI | UnityEditor.StaticEditorFlags.ReflectionProbeStatic;
// box from min/max in room coordinates (x along windows, z along headboard, y up from the floor)
System.Func<string, UnityEngine.Transform, UnityEngine.Material, UnityEngine.Vector3, UnityEngine.Vector3, float, UnityEngine.GameObject> box = (n, parent, m, a, b, lmScale) => {
    var g = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cube); g.name = n;
    UnityEngine.Object.DestroyImmediate(g.GetComponent<UnityEngine.Collider>());
    g.transform.SetParent(parent, false);
    g.transform.localPosition = (a + b) * 0.5f;
    g.transform.localScale = new UnityEngine.Vector3(UnityEngine.Mathf.Abs(b.x - a.x), UnityEngine.Mathf.Abs(b.y - a.y), UnityEngine.Mathf.Abs(b.z - a.z));
    var r = g.GetComponent<UnityEngine.MeshRenderer>(); r.sharedMaterial = m; r.scaleInLightmap = lmScale;
    UnityEditor.GameObjectUtility.SetStaticEditorFlags(g, GI);
    return g;
};
System.Func<float, float, float, UnityEngine.Vector3> P = (x, y, z) => new UnityEngine.Vector3(x, y, z);

// inner faces sit 1.5 cm inside the old walls; thickness runs back into them
float ux0 = -2.275f, ux1 = 2.0f, vz0 = -1.905f, vz1 = 1.855f, h = 2.955f, th = 0.14f;
box("Floor", shell, floorM, P(ux0 - th, -0.06f, vz0 - th), P(ux1 + th, 0.006f, vz1 + th), 1.2f);
box("Ceiling", shell, ceilM, P(ux0 - th, h, vz0 - th), P(ux1 + th, h + 0.06f, vz1 + th), 0.8f);
box("Wall_Head", shell, wallM, P(ux0 - th, 0, vz1), P(ux1 + th, h, vz1 + th), 1.2f);
box("Wall_South", shell, wallM, P(ux0 - th, 0, vz0 - th), P(ux1 + th, h, vz0), 1f);
// west wall with door opening
float dv0 = -1.77f, dv1 = -0.87f, dTop = 2.27f;
box("Wall_West_A", shell, wallM, P(ux0 - th, 0, vz0), P(ux0, h, dv0), 0.6f);
box("Wall_West_B", shell, wallM, P(ux0 - th, 0, dv1), P(ux0, h, vz1), 1f);
box("Wall_West_Top", shell, wallM, P(ux0 - th, dTop, dv0), P(ux0, h, dv1), 0.6f);
// east wall with two windows
float wy0 = 0.87f, wy1 = 2.47f, w1a = -1.67f, w1b = -0.87f, w2a = -0.17f, w2b = 1.73f;
box("Wall_East_Low", shell, wallM, P(ux1, 0, vz0), P(ux1 + th, wy0, vz1), 1f);
box("Wall_East_Top", shell, wallM, P(ux1, wy1, vz0), P(ux1 + th, h, vz1), 0.8f);
box("Wall_East_P0", shell, wallM, P(ux1, wy0, vz0), P(ux1 + th, wy1, w1a), 0.6f);
box("Wall_East_P1", shell, wallM, P(ux1, wy0, w1b), P(ux1 + th, wy1, w2a), 0.8f);
box("Wall_East_P2", shell, wallM, P(ux1, wy0, w2b), P(ux1 + th, wy1, vz1), 0.5f);

// skirting and window sills
float sk = 0.095f, skd = 0.018f;
box("Skirt_Head", trim, trimM, P(ux0, 0, vz1 - skd), P(ux1, sk, vz1), 0.4f);
box("Skirt_South", trim, trimM, P(ux0, 0, vz0), P(ux1, sk, vz0 + skd), 0.4f);
box("Skirt_East", trim, trimM, P(ux1 - skd, 0, vz0), P(ux1, sk, vz1), 0.4f);
box("Skirt_West_B", trim, trimM, P(ux0, 0, dv1), P(ux0 + skd, sk, vz1), 0.4f);
box("Skirt_West_A", trim, trimM, P(ux0, 0, vz0), P(ux0 + skd, sk, dv0), 0.2f);
box("Sill_W1", trim, trimM, P(ux1 - 0.05f, wy0 - 0.025f, w1a - 0.04f), P(ux1 + 0.05f, wy0, w1b + 0.04f), 0.3f);
box("Sill_W2", trim, trimM, P(ux1 - 0.05f, wy0 - 0.025f, w2a - 0.04f), P(ux1 + 0.05f, wy0, w2b + 0.04f), 0.4f);

// horizontal blinds cover the upper part of both windows, slats tilted so moonlight falls in stripes
System.Action<string, float, float> blind = (n, a, b) => {
    var g = new UnityEngine.GameObject(n).transform; g.SetParent(blinds, false);
    box(n + "_Rail", g, trimM, P(ux1 - 0.075f, wy1 - 0.045f, a), P(ux1 - 0.02f, wy1, b), 0.2f);
    for (float y = wy1 - 0.09f; y > wy0 + 0.46f; y -= 0.068f)
    {
        var s = box(n + "_Slat", g, blindM, P(ux1 - 0.085f, y - 0.0035f, a + 0.01f), P(ux1 - 0.015f, y + 0.0035f, b - 0.01f), 0.05f);
        s.transform.localRotation = UnityEngine.Quaternion.Euler(0, 0, 26f);
        UnityEditor.GameObjectUtility.SetStaticEditorFlags(s, UnityEditor.StaticEditorFlags.ContributeGI);
        s.GetComponent<UnityEngine.MeshRenderer>().receiveGI = UnityEngine.ReceiveGI.LightProbes;
    }
};
blind("Blind_W1", w1a, w1b); blind("Blind_W2", w2a, w2b);

// hide what the new props replace; objects stay in the scene so this can be undone
var hide = new[] { "Bed1_02", "KidsChair", "Desk", "Book", "PuzzleCube", "DeskLight", "EarthMapBall", "RoomFurniture_01", "Nightstand_01", "Ball_04", "HomeLamp_04", "AcousticGuitar", "Carpet" };
foreach (var n in hide) { var t = apt.transform.Find(n); if (t == null) { sb.AppendLine("missing " + n); continue; } t.gameObject.SetActive(false); }

// the old sample lights and the white fill inside this room stop being used here
var sampleRoot = apt.transform.Find("Lighting_17F_ROOM1_DuskSample");
var coolFill = sampleRoot.Find("WindowFill_Cool"); if (coolFill != null) coolFill.GetComponent<UnityEngine.Light>().enabled = false;
var neonSpill = sampleRoot.Find("BillboardSpill_Magenta"); if (neonSpill != null) neonSpill.GetComponent<UnityEngine.Light>().enabled = true;
var bedroomLeak = UnityEngine.GameObject.Find("Lobby/Spot Light (9)/Point Light (5)"); if (bedroomLeak != null) { var bl = bedroomLeak.GetComponent<UnityEngine.Light>(); bl.shadows = UnityEngine.LightShadows.Soft; UnityEditor.EditorUtility.SetDirty(bl); }
var fill = UnityEngine.GameObject.Find("Lobby/Spot Light (9)/Point Light (1)");
if (fill != null) fill.GetComponent<UnityEngine.Light>().enabled = false; else sb.AppendLine("fill light not found");

UnityEditor.AssetDatabase.SaveAssets();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
sb.AppendLine("shell children=" + shell.childCount + " trim=" + trim.childCount + " blinds=" + blinds.childCount + " rootScale=" + root.transform.lossyScale.ToString("0.000"));
return sb.ToString();
