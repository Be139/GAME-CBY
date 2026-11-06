// Read-only: how the city renderers are stored, so the tint and the new towers can be undone exactly.
var sb = new System.Text.StringBuilder();
int total = 0, prefabParts = 0, tinted = 0, towers = 0, disabled = 0, overridesMat = 0;
foreach (var n in new[] { "CityBuildings_Manual", "CityBuildings_Generated" }) {
    var root = UnityEngine.GameObject.Find(n); if (root == null) { sb.AppendLine("missing " + n); continue; }
    sb.AppendLine(n + " prefabInstanceRoot=" + UnityEditor.PrefabUtility.IsPartOfPrefabInstance(root) + " children=" + root.transform.childCount);
    foreach (var r in root.GetComponentsInChildren<UnityEngine.MeshRenderer>(true)) {
        bool isTower = false; var p = r.transform; while (p != null) { if (p.name == "Tower_New") { isTower = true; break; } p = p.parent; }
        if (isTower) { towers++; continue; }
        total++; if (!r.enabled) disabled++;
        if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(r)) { prefabParts++; foreach (var m in UnityEditor.PrefabUtility.GetPropertyModifications(r) ?? new UnityEditor.PropertyModification[0]) { } }
        foreach (var m in r.sharedMaterials) if (m != null && UnityEditor.AssetDatabase.GetAssetPath(m).Contains("/Rooms/city/")) { tinted++; break; }
    }
}
var gid = UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(UnityEngine.GameObject.Find("CityBuildings_Generated").GetComponentInChildren<UnityEngine.MeshRenderer>(true));
sb.AppendLine("renderers=" + total + " prefabParts=" + prefabParts + " tintedRenderers=" + tinted + " towerRenderers=" + towers + " disabled=" + disabled + " sampleId=" + gid);
var guids = UnityEditor.AssetDatabase.FindAssets("t:Material", new[] { "Assets/Art/Generated/Rooms/city" }); sb.AppendLine("cityMats=" + guids.Length);
for (int i = 0; i < guids.Length && i < 40; i++) sb.Append(System.IO.Path.GetFileNameWithoutExtension(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[i])) + " ");
return sb.ToString();
