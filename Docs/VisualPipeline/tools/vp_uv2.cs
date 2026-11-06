// Old props that stay in a room get lightmap UVs from their importers, then join the bake. Doors join too: a swinging door keeps its baked shading, which reads far better than an unlit black door.
if (UnityEditor.EditorApplication.isPlaying) return "skip: play mode";
var sb = new System.Text.StringBuilder();
var GI = UnityEditor.StaticEditorFlags.ContributeGI | UnityEditor.StaticEditorFlags.ReflectionProbeStatic;
var imps = new System.Collections.Generic.HashSet<string>();
var keep = new System.Collections.Generic.List<UnityEngine.MeshRenderer>();
foreach (var n in new[] { "Floor17/Apartment_17F01", "Floor17/Apartment_17F02", "Floor17/Apartment_17F03", "Floor17/Apartment_17F04" }) {
    var a = UnityEngine.GameObject.Find(n).transform;
    foreach (UnityEngine.Transform t in a) {
        if (t.name.StartsWith("VisualPass") || t.name.StartsWith("Lighting_")) continue;
        if (!t.gameObject.activeSelf) continue;
        foreach (var r in t.GetComponentsInChildren<UnityEngine.MeshRenderer>(false)) {
            var mf = r.GetComponent<UnityEngine.MeshFilter>(); if (mf == null || mf.sharedMesh == null) continue;
            keep.Add(r);
            var uv2 = mf.sharedMesh.uv2; if (uv2 != null && uv2.Length > 0) continue;
            var p = UnityEditor.AssetDatabase.GetAssetPath(mf.sharedMesh);
            var mi = UnityEditor.AssetImporter.GetAtPath(p) as UnityEditor.ModelImporter;
            if (mi != null) imps.Add(p); else sb.AppendLine("no importer: " + r.name + " " + p);
        }
    }
}
foreach (var n in new[] { "LoungeRoomSofa_01", "LoungeRoomSofa_02", "LoungeRoomSofa_03", "SofaChairSet9_Sofa_01", "SofaChairSet9_Sofa_02", "KitchenTable_02_01", "KitchenTable_02_02", "KitchenTable_02_03", "KitchenChair_01_01", "KitchenChair_01_02", "KitchenChair_01_03", "KitchenChair_01_04", "KitchenChair_01_05", "KitchenChair_01_06", "KitchenChair_01_07", "KitchenChair_01_08", "KitchenChair_01_09", "KitchenChair_01_10", "KitchenChair_01_11", "KitchenChair_01_12", "Carpet_01", "Carpet_02", "Houseplant_05", "Houseplant_07", "RMTreebedcompact" }) {
    var t = UnityEngine.GameObject.Find("Lobby").transform.Find(n); if (t == null) { sb.AppendLine("no lobby object " + n); continue; }
    foreach (var r in t.GetComponentsInChildren<UnityEngine.MeshRenderer>(false)) {
        var mf = r.GetComponent<UnityEngine.MeshFilter>(); if (mf == null || mf.sharedMesh == null) continue;
        keep.Add(r);
        var uv2 = mf.sharedMesh.uv2; if (uv2 != null && uv2.Length > 0) continue;
        var p = UnityEditor.AssetDatabase.GetAssetPath(mf.sharedMesh);
        var mi = UnityEditor.AssetImporter.GetAtPath(p) as UnityEditor.ModelImporter;
        if (mi != null) imps.Add(p);
    }
}
sb.AppendLine("kept renderers " + keep.Count + " model files needing uv2 " + imps.Count);
UnityEditor.AssetDatabase.StartAssetEditing();
try { foreach (var p in imps) { var mi = (UnityEditor.ModelImporter)UnityEditor.AssetImporter.GetAtPath(p); mi.generateSecondaryUV = true; mi.secondaryUVPackMargin = 16f; mi.SaveAndReimport(); } }
finally { UnityEditor.AssetDatabase.StopAssetEditing(); }
int flagged = 0;
foreach (var r in keep) {
    var f = UnityEditor.GameObjectUtility.GetStaticEditorFlags(r.gameObject);
    UnityEditor.GameObjectUtility.SetStaticEditorFlags(r.gameObject, f | GI);
    var s = r.bounds.size; r.scaleInLightmap = UnityEngine.Mathf.Max(s.x, s.y, s.z) > 1.2f ? 0.8f : 0.4f; flagged++;
}
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
sb.AppendLine("flagged " + flagged);
return sb.ToString();
