int towers = 0, tinted = 0, off = 0, total = 0, night = 0;
foreach (var n in new[] { "CityBuildings_Manual", "CityBuildings_Generated" }) {
    foreach (var t in UnityEngine.GameObject.Find(n).GetComponentsInChildren<UnityEngine.Transform>(true)) if (t.name == "Tower_New") towers++;
    foreach (var r in UnityEngine.GameObject.Find(n).GetComponentsInChildren<UnityEngine.MeshRenderer>(true)) { total++; if (!r.enabled) off++; bool t2 = false; foreach (var m in r.sharedMaterials) { if (m == null) continue; if (UnityEditor.AssetDatabase.GetAssetPath(m).Contains("/Rooms/city/")) t2 = true; if (m.name.StartsWith("M_WindowNight")) night++; } if (t2) tinted++; }
}
return "towers=" + towers + " total=" + total + " tinted=" + tinted + " disabled=" + off + " nightSlots=" + night + " dirty=" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().isDirty + " busy=" + UnityEditor.EditorApplication.isCompiling;
