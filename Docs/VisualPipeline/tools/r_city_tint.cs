// Old towers that stay (hand-placed ones with billboards, and slots beyond the new ring) move into the same indigo night as the new city. Materials are copied, never edited, so the originals stay untouched.
if (UnityEditor.EditorApplication.isPlaying) return "skip: play mode";
const string Dir = "Assets/Art/Generated/Rooms/city";
if (!UnityEditor.AssetDatabase.IsValidFolder(Dir)) UnityEditor.AssetDatabase.CreateFolder("Assets/Art/Generated/Rooms", "city");
var tint = new UnityEngine.Color(0.30f, 0.32f, 0.62f);
var clones = new System.Collections.Generic.Dictionary<UnityEngine.Material, UnityEngine.Material>(); int swapped = 0;
foreach (var n in new[] { "CityBuildings_Manual", "CityBuildings_Generated" }) foreach (var r in UnityEngine.GameObject.Find(n).GetComponentsInChildren<UnityEngine.MeshRenderer>(true)) {
    if (!r.enabled || r.transform.parent == null || r.gameObject.name == "Tower_New" || r.transform.GetComponentInParent<UnityEngine.Transform>().name.StartsWith("Tower_New")) continue;
    if (r.GetComponent<CityBillboardContentController>() != null) continue;
    var mats = r.sharedMaterials; bool changed = false;
    for (int i = 0; i < mats.Length; i++) {
        var m = mats[i]; if (m == null || m.name.StartsWith("C_") || !m.HasProperty("_BaseColor")) continue;
        UnityEngine.Material c; if (!clones.TryGetValue(m, out c)) { c = new UnityEngine.Material(m); c.name = "C_" + m.name; var bc = m.GetColor("_BaseColor"); c.SetColor("_BaseColor", new UnityEngine.Color(bc.r * tint.r, bc.g * tint.g, bc.b * tint.b, bc.a)); if (c.HasProperty("_Smoothness")) c.SetFloat("_Smoothness", 0.2f); var clean = System.Text.RegularExpressions.Regex.Replace(m.name, "[^A-Za-z0-9_]", "_"); UnityEditor.AssetDatabase.CreateAsset(c, UnityEditor.AssetDatabase.GenerateUniqueAssetPath(Dir + "/C_" + clean + ".mat")); clones[m] = c; }
        mats[i] = c; changed = true;
    }
    if (changed) { r.sharedMaterials = mats; swapped++; }
}
UnityEditor.AssetDatabase.SaveAssets();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
return "materials " + clones.Count + " renderers " + swapped;
