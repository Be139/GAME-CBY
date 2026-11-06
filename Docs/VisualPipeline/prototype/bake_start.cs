// Bake only Noah's room. Everything else stays exactly as it was: its GI flags are recorded, cleared for the bake, and restored by bake_finish.cs.
if (UnityEditor.EditorApplication.isPlaying) return "skip: play mode";
if (UnityEditor.Lightmapping.isRunning) return "already running";
var stateFile = @"D:/UGit/GAME-CBY/Docs/VisualPipeline/prototype/_work/bake_state.txt";
var sb = new System.Text.StringBuilder();
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
var root = UnityEngine.GameObject.Find("Floor17/Apartment_17F01/VisualPass_NoahRoom").transform;

// keep a saved copy of the real state before flags are touched
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);

var state = new System.Text.StringBuilder();
int cleared = 0;
foreach (var r in UnityEngine.Object.FindObjectsOfType<UnityEngine.MeshRenderer>(true))
{
    if (r.transform.IsChildOf(root)) continue;
    var f = UnityEditor.GameObjectUtility.GetStaticEditorFlags(r.gameObject);
    if ((f & UnityEditor.StaticEditorFlags.ContributeGI) == 0) continue;
    state.AppendLine(UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(r.gameObject).ToString());
    UnityEditor.GameObjectUtility.SetStaticEditorFlags(r.gameObject, f & ~UnityEditor.StaticEditorFlags.ContributeGI);
    cleared++;
}
System.IO.File.WriteAllText(stateFile, state.ToString());

// the probe groups that came with the tree models would make every unbaked object in the scene switch to them
foreach (var g in UnityEngine.Object.FindObjectsOfType<UnityEngine.LightProbeGroup>(true)) g.enabled = false;

// the living room probe stays out of this bake
var room1 = UnityEngine.GameObject.Find("Floor17/Apartment_17F01/Lighting_17F_ROOM1_DuskSample/ReflectionProbe_ROOM1");
if (room1 != null) room1.SetActive(false);

// door opening blocker: occludes during the bake, needs no lightmap, removed afterwards
var old = root.Find("__BakeDoorBlocker"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
var door = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cube); door.name = "__BakeDoorBlocker"; UnityEngine.Object.DestroyImmediate(door.GetComponent<UnityEngine.Collider>());
door.transform.SetParent(root, false); door.transform.localPosition = new UnityEngine.Vector3(-2.345f, 1.14f, -1.32f); door.transform.localScale = new UnityEngine.Vector3(0.14f, 2.28f, 0.92f);
var dr = door.GetComponent<UnityEngine.MeshRenderer>(); dr.sharedMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>("Assets/Art/Generated/NoahRoom/shared/M_Noah_Wall.mat"); dr.scaleInLightmap = 0f;
UnityEditor.GameObjectUtility.SetStaticEditorFlags(door, UnityEditor.StaticEditorFlags.ContributeGI);

var path = "Assets/Art/Generated/NoahRoom/shared/NoahRoom_Lighting.lighting";
var ls = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.LightingSettings>(path);
if (ls == null) { ls = new UnityEngine.LightingSettings(); ls.name = "NoahRoom_Lighting"; UnityEditor.AssetDatabase.CreateAsset(ls, path); }
ls.lightmapper = UnityEngine.LightingSettings.Lightmapper.ProgressiveCPU;
ls.mixedBakeMode = UnityEngine.MixedLightingMode.IndirectOnly;
ls.bakedGI = true; ls.realtimeGI = false; ls.autoGenerate = false;
ls.lightmapResolution = LM_RES; ls.lightmapPadding = 4; ls.lightmapMaxSize = 2048;
ls.directionalityMode = UnityEngine.LightmapsMode.NonDirectional;
ls.directSampleCount = 64; ls.indirectSampleCount = LM_SAMPLES; ls.environmentSampleCount = 256; ls.maxBounces = 3;
ls.ao = true; ls.aoMaxDistance = 0.9f; ls.aoExponentDirect = 0f; ls.aoExponentIndirect = 1.2f;
ls.albedoBoost = 1.1f; ls.indirectScale = 1.0f;
ls.filteringMode = UnityEngine.LightingSettings.FilterMode.Auto;
UnityEditor.EditorUtility.SetDirty(ls);
UnityEditor.Lightmapping.lightingSettings = ls;

bool ok = UnityEditor.Lightmapping.BakeAsync();
return "cleared=" + cleared + " bakeStarted=" + ok + " running=" + UnityEditor.Lightmapping.isRunning;
