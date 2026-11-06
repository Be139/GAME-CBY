// Puts the city back to the committed state: removes the generated towers, then resets every city renderer to its prefab and re-applies the overrides the committed scene had (night window materials, enabled flags).
if (UnityEditor.EditorApplication.isPlaying) return "skip: play mode";
var tsv = @"D:/UGit/GAME-CBY/Docs/VisualPipeline/tools/_work/head_mods.tsv";
var mods = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string[]>>();
foreach (var line in System.IO.File.ReadAllLines(tsv)) { var f = line.Split('\t'); if (f.Length < 8) continue; var k = f[0] + "|" + f[1]; System.Collections.Generic.List<string[]> l; if (!mods.TryGetValue(k, out l)) { l = new System.Collections.Generic.List<string[]>(); mods[k] = l; } l.Add(f); }
var assetCache = new System.Collections.Generic.Dictionary<string, UnityEngine.Object>();
System.Func<string, string, UnityEngine.Object> load = (guid, fid) => {
    var key = guid + ":" + fid; UnityEngine.Object o; if (assetCache.TryGetValue(key, out o)) return o;
    var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid); o = null;
    if (!string.IsNullOrEmpty(path)) foreach (var a in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path)) { string g; long id; if (a != null && UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(a, out g, out id) && id.ToString() == fid) { o = a; break; } }
    assetCache[key] = o; return o;
};
int towers = 0, reset = 0, applied = 0, missingRef = 0, unknownPath = 0;
foreach (var n in new[] { "CityBuildings_Manual", "CityBuildings_Generated" }) {
    var root = UnityEngine.GameObject.Find(n).transform;
    var dead = new System.Collections.Generic.List<UnityEngine.GameObject>();
    foreach (var t in root.GetComponentsInChildren<UnityEngine.Transform>(true)) if (t.name == "Tower_New") dead.Add(t.gameObject);
    foreach (var g in dead) { UnityEngine.Object.DestroyImmediate(g); towers++; }
    foreach (var r in root.GetComponentsInChildren<UnityEngine.MeshRenderer>(true)) {
        if (!UnityEditor.PrefabUtility.IsPartOfPrefabInstance(r)) continue;
        var id = UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(r);
        UnityEditor.PrefabUtility.RevertObjectOverride(r, UnityEditor.InteractionMode.AutomatedAction); reset++;
        System.Collections.Generic.List<string[]> l; if (!mods.TryGetValue(id.targetPrefabId + "|" + id.targetObjectId, out l)) continue;
        var so = new UnityEditor.SerializedObject(r);
        foreach (var f in l) {
            var p = so.FindProperty(f[3]); if (p == null) { unknownPath++; continue; }
            if (p.propertyType == UnityEditor.SerializedPropertyType.ObjectReference) { if (f[5] == "0") p.objectReferenceValue = null; else { var o = load(f[6], f[5]); if (o == null) { missingRef++; continue; } p.objectReferenceValue = o; } }
            else if (p.propertyType == UnityEditor.SerializedPropertyType.Boolean) p.boolValue = f[4] == "1";
            else if (p.propertyType == UnityEditor.SerializedPropertyType.Integer || p.propertyType == UnityEditor.SerializedPropertyType.Enum) p.intValue = int.Parse(f[4]);
            else if (p.propertyType == UnityEditor.SerializedPropertyType.Float) p.floatValue = float.Parse(f[4], System.Globalization.CultureInfo.InvariantCulture);
            else if (p.propertyType == UnityEditor.SerializedPropertyType.ArraySize) p.intValue = int.Parse(f[4]);
            else { unknownPath++; continue; }
            applied++;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
int tinted = 0, off = 0;
foreach (var n in new[] { "CityBuildings_Manual", "CityBuildings_Generated" }) foreach (var r in UnityEngine.GameObject.Find(n).GetComponentsInChildren<UnityEngine.MeshRenderer>(true)) { if (!r.enabled) off++; foreach (var m in r.sharedMaterials) if (m != null && UnityEditor.AssetDatabase.GetAssetPath(m).Contains("/Rooms/city/")) { tinted++; break; } }
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene); UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
return "towersRemoved=" + towers + " renderersReset=" + reset + " overridesApplied=" + applied + " missingRef=" + missingRef + " unknownPath=" + unknownPath + " stillTinted=" + tinted + " disabledNow=" + off;
