// Restores everything vp_bake_start.cs changed and saves the scene. Safe to run more than once.
if (UnityEditor.Lightmapping.isRunning) return "still running " + UnityEditor.Lightmapping.buildProgress.ToString("0.00");
var stateFile = @"D:/UGit/GAME-CBY/Docs/VisualPipeline/tools/_work/bake_state.txt";
int restored = 0, doors = 0, missing = 0;
if (System.IO.File.Exists(stateFile)) {
    foreach (var line in System.IO.File.ReadAllLines(stateFile)) {
        if (string.IsNullOrWhiteSpace(line)) continue;
        var p = line.Split(' ');
        UnityEditor.GlobalObjectId id;
        if (!UnityEditor.GlobalObjectId.TryParse(p[1], out id)) { missing++; continue; }
        var go = UnityEditor.GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as UnityEngine.GameObject;
        if (go == null) { missing++; continue; }
        var f = UnityEditor.GameObjectUtility.GetStaticEditorFlags(go);
        if (p[0] == "G") { UnityEditor.GameObjectUtility.SetStaticEditorFlags(go, f | UnityEditor.StaticEditorFlags.ContributeGI); restored++; }
        else { UnityEditor.GameObjectUtility.SetStaticEditorFlags(go, f & ~UnityEditor.StaticEditorFlags.ContributeGI); go.GetComponent<UnityEngine.MeshRenderer>().scaleInLightmap = float.Parse(p[2], System.Globalization.CultureInfo.InvariantCulture); doors++; }
    }
    System.IO.File.WriteAllText(stateFile, "");
}
foreach (var g in UnityEngine.Object.FindObjectsOfType<UnityEngine.LightProbeGroup>(true)) g.enabled = true;
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
int lit = 0; foreach (var r in UnityEngine.Object.FindObjectsOfType<UnityEngine.MeshRenderer>(true)) if (r.lightmapIndex >= 0 && r.lightmapIndex < 65534) lit++;
return "restored=" + restored + " doors=" + doors + " missing=" + missing + " lightmaps=" + UnityEngine.LightmapSettings.lightmaps.Length + " lit=" + lit + " probes=" + (UnityEngine.LightmapSettings.lightProbes != null ? UnityEngine.LightmapSettings.lightProbes.count : 0) + " dirty=" + scene.isDirty;
