// Wall shells: texels that see back faces are thrown away by the lightmapper and refilled from neighbours, which leaves flat blocks with stepped edges.
// A Lightmap Parameters asset with backface tolerance 0 and a 1 mm push-off keeps every wall texel and starts its rays clear of the surface.
if (UnityEditor.EditorApplication.isPlaying) return "skip: play mode";
const string path = "Assets/Art/Generated/Rooms/shared/LP_Walls_KeepTexels.giparams";
var lp = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.LightmapParameters>(path);
if (lp == null) { lp = new UnityEditor.LightmapParameters(); UnityEditor.AssetDatabase.CreateAsset(lp, path); }
lp.backFaceTolerance = 0f; lp.pushoff = 0.001f; lp.antiAliasingSamples = 8; lp.blurRadius = 2;
UnityEditor.EditorUtility.SetDirty(lp); UnityEditor.AssetDatabase.SaveAssets();
int n = 0;
foreach (var r in UnityEngine.Object.FindObjectsOfType<UnityEngine.MeshRenderer>()) {
    if (!r.enabled || r.transform.parent == null || r.transform.parent.name != "Shell" || r.lightmapIndex < 0) continue;
    var so = new UnityEditor.SerializedObject(r); var prop = so.FindProperty("m_LightmapParameters"); if (prop == null) continue;
    prop.objectReferenceValue = lp; so.ApplyModifiedPropertiesWithoutUndo(); n++;
}
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene(); UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene); UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
return "shells with keep-texels parameters: " + n;
