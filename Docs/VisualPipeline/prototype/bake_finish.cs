// Restore everything bake_start.cs changed. Safe to run more than once.
if (UnityEditor.Lightmapping.isRunning) return "still running " + UnityEditor.Lightmapping.buildProgress.ToString("0.00");
var stateFile = @"D:/UGit/GAME-CBY/Docs/VisualPipeline/prototype/_work/bake_state.txt";
var sb = new System.Text.StringBuilder();
var root = UnityEngine.GameObject.Find("Floor17/Apartment_17F01/VisualPass_NoahRoom").transform;
int restored = 0, missing = 0;
if (System.IO.File.Exists(stateFile))
{
    foreach (var line in System.IO.File.ReadAllLines(stateFile))
    {
        if (string.IsNullOrWhiteSpace(line)) continue;
        UnityEditor.GlobalObjectId id;
        if (!UnityEditor.GlobalObjectId.TryParse(line, out id)) { missing++; continue; }
        var go = UnityEditor.GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as UnityEngine.GameObject;
        if (go == null) { missing++; continue; }
        var f = UnityEditor.GameObjectUtility.GetStaticEditorFlags(go);
        UnityEditor.GameObjectUtility.SetStaticEditorFlags(go, f | UnityEditor.StaticEditorFlags.ContributeGI);
        restored++;
    }
    System.IO.File.WriteAllText(stateFile, "");
}
foreach (var g in UnityEngine.Object.FindObjectsOfType<UnityEngine.LightProbeGroup>(true)) g.enabled = true;
var blocker = root.Find("__BakeDoorBlocker"); if (blocker != null) UnityEngine.Object.DestroyImmediate(blocker.gameObject);
var apt = root.parent;
var sample = apt.Find("Lighting_17F_ROOM1_DuskSample"); var room1 = sample != null ? sample.Find("ReflectionProbe_ROOM1") : null;
if (room1 != null) room1.gameObject.SetActive(true);
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
int lit = 0, unlit = 0;
foreach (var r in root.GetComponentsInChildren<UnityEngine.MeshRenderer>(true)) { if (r.lightmapIndex >= 0 && r.lightmapIndex < 65534) lit++; else unlit++; }
int outside = 0;
foreach (var r in UnityEngine.Object.FindObjectsOfType<UnityEngine.MeshRenderer>(true)) if (!r.transform.IsChildOf(root) && r.lightmapIndex >= 0 && r.lightmapIndex < 65534) outside++;
sb.AppendLine("restored=" + restored + " missing=" + missing + " lightmaps=" + UnityEngine.LightmapSettings.lightmaps.Length + " roomLit=" + lit + " roomUnlit=" + unlit + " outsideLit=" + outside + " probes=" + (UnityEngine.LightmapSettings.lightProbes != null ? UnityEngine.LightmapSettings.lightProbes.count : 0));
sb.AppendLine("lightingData=" + (UnityEditor.Lightmapping.lightingDataAsset != null ? UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.Lightmapping.lightingDataAsset) : "none") + " dirty=" + scene.isDirty);
return sb.ToString();
