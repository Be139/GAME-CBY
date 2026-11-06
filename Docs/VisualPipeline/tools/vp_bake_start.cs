// Bakes every finished space together: a Unity bake replaces all lightmaps, so a room left out would lose its light.
// Everything outside those spaces keeps its state: its GI flags are recorded, cleared for the bake and restored by vp_bake_finish.cs.
if (UnityEditor.EditorApplication.isPlaying) return "skip: play mode";
if (UnityEditor.Lightmapping.isRunning) return "already running";
var stateFile = @"D:/UGit/GAME-CBY/Docs/VisualPipeline/tools/_work/bake_state.txt";
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
var roots = new System.Collections.Generic.List<UnityEngine.Transform>();
foreach (var n in new[] { "Floor17/Apartment_17F01", "Floor17/Apartment_17F02", "Floor17/Apartment_17F03", "Floor17/Apartment_17F04", "Floor17/VisualPass_Corridor", "Lobby" }) { var g = UnityEngine.GameObject.Find(n); if (g != null) roots.Add(g.transform); }
System.Func<UnityEngine.Transform, bool> inRooms = t => { foreach (var r in roots) if (t.IsChildOf(r)) return true; return false; };
var state = new System.Text.StringBuilder(); int cleared = 0, doors = 0;
foreach (var r in UnityEngine.Object.FindObjectsOfType<UnityEngine.MeshRenderer>(true)) {
    if (inRooms(r.transform)) continue;
    var f = UnityEditor.GameObjectUtility.GetStaticEditorFlags(r.gameObject);
    if ((f & UnityEditor.StaticEditorFlags.ContributeGI) == 0) continue;
    state.AppendLine("G " + UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(r.gameObject));
    UnityEditor.GameObjectUtility.SetStaticEditorFlags(r.gameObject, f & ~UnityEditor.StaticEditorFlags.ContributeGI);
    cleared++;
}
// closed doors block light during the bake without taking lightmap space
foreach (var r in roots) foreach (UnityEngine.Transform t in r) {
    if (!t.name.StartsWith("Door_")) continue;
    foreach (var m in t.GetComponentsInChildren<UnityEngine.MeshRenderer>(true)) {
        var f = UnityEditor.GameObjectUtility.GetStaticEditorFlags(m.gameObject);
        if ((f & UnityEditor.StaticEditorFlags.ContributeGI) != 0) continue;
        state.AppendLine("D " + UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(m.gameObject) + " " + m.scaleInLightmap.ToString(System.Globalization.CultureInfo.InvariantCulture));
        UnityEditor.GameObjectUtility.SetStaticEditorFlags(m.gameObject, f | UnityEditor.StaticEditorFlags.ContributeGI); m.scaleInLightmap = 0f; doors++;
    }
}
System.IO.File.WriteAllText(stateFile, state.ToString());

var ls = UnityEditor.Lightmapping.lightingSettings;
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
bool ok = UnityEditor.Lightmapping.BakeAsync();
return "cleared=" + cleared + " doors=" + doors + " rooms=" + roots.Count + " bakeStarted=" + ok;
