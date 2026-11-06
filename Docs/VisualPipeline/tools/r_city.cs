// City pass. New towers stand in the existing building slots near the site; slots further out keep their old block but go dark indigo so the skyline reads as one city. Nothing is deleted: old renderers are switched off and the slots, billboards and window controller stay as they are.
if (UnityEditor.EditorApplication.isPlaying) return "skip: play mode";
var sb = new System.Text.StringBuilder();
const string Rooms = "Assets/Art/Generated/Rooms";
const float Radius = NEAR_RADIUS;
var centre = new UnityEngine.Vector3(-2306f, 0f, 590f);
var ids = new[] { "tower_a", "tower_b", "tower_c", "tower_d", "tower_e", "tower_f", "tower_g", "tower_h", "tower_i", "tower_j" };
var prefabs = new System.Collections.Generic.Dictionary<string, UnityEngine.GameObject>(); var mats = new System.Collections.Generic.Dictionary<string, UnityEngine.Material>();
foreach (var id in ids) {
    var go = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(Rooms + "/" + id + "/" + id + ".fbx"); if (go == null) { sb.AppendLine("missing " + id); continue; } prefabs[id] = go;
    var ti = UnityEditor.AssetImporter.GetAtPath(Rooms + "/" + id + "/texture_0.png") as UnityEditor.TextureImporter; if (ti != null && ti.maxTextureSize != 2048) { ti.maxTextureSize = 2048; ti.SaveAndReimport(); }
    var m = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(Rooms + "/" + id + "/M_" + id + ".mat"); var tex = m.GetTexture("_BaseMap");
    m.SetColor("_BaseColor", new UnityEngine.Color(0.32f, 0.32f, 0.36f)); m.EnableKeyword("_EMISSION"); m.SetTexture("_EmissionMap", tex); m.SetColor("_EmissionColor", UnityEngine.Color.white * 0.95f); m.globalIlluminationFlags = UnityEngine.MaterialGlobalIlluminationFlags.None; m.SetFloat("_Smoothness", 0.25f); UnityEditor.EditorUtility.SetDirty(m); mats[id] = m;
}
var byBand = new System.Collections.Generic.Dictionary<string, string[]> { { "Buildings_High_180mPlus", new[] { "tower_f", "tower_d", "tower_a", "tower_e", "tower_i" } }, { "Buildings_Mid_120_179m", new[] { "tower_a", "tower_b", "tower_c", "tower_e", "tower_g", "tower_h", "tower_i" } }, { "Buildings_Low_Under120m", new[] { "tower_b", "tower_c", "tower_g", "tower_h", "tower_j" } } };
var gen = UnityEngine.GameObject.Find("CityBuildings_Generated").transform; int placed = 0, kept = 0; long tris = 0;
foreach (UnityEngine.Transform band in gen) {
    var pool = byBand.ContainsKey(band.name) ? byBand[band.name] : ids;
    foreach (UnityEngine.Transform slot in band) {
        var old = slot.Find("Tower_New"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
        var rs = slot.GetComponentsInChildren<UnityEngine.Renderer>(true); if (rs.Length == 0) continue;
        foreach (var r in rs) r.enabled = true;
        var b = rs[0].bounds; foreach (var r in rs) b.Encapsulate(r.bounds);
        float dist = new UnityEngine.Vector2(b.center.x - centre.x, b.center.z - centre.z).magnitude;
        if (dist > Radius) { kept++; continue; }
        int hash = 0; foreach (var ch in slot.name) hash = hash * 31 + ch; hash = System.Math.Abs(hash);
        var id = pool[hash % pool.Length]; if (!prefabs.ContainsKey(id)) { kept++; continue; }
        foreach (var r in rs) r.enabled = false;
        var src = prefabs[id];
        var go = (UnityEngine.GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(src); go.name = "Tower_New"; go.transform.position = UnityEngine.Vector3.zero;
        go.transform.rotation = UnityEngine.Quaternion.Euler(0, -21.4f + (hash % 4) * 90f, 0) * src.transform.localRotation; go.transform.localScale = UnityEngine.Vector3.one;
        var nr = go.GetComponentInChildren<UnityEngine.MeshRenderer>(); var ub = nr.bounds;
        float k = b.size.y / ub.size.y; float kFoot = UnityEngine.Mathf.Min(b.size.x / ub.size.x, b.size.z / ub.size.z);
        // keep proportions close to the design: footprint may differ from height scale by 45% at most
        float foot = UnityEngine.Mathf.Clamp(kFoot, k * 0.55f, k * 1.45f);
        var s = src.transform.localScale; var rot = go.transform.rotation;
        go.transform.rotation = src.transform.localRotation; go.transform.localScale = new UnityEngine.Vector3(foot, foot, k); go.transform.rotation = rot;
        var nb = nr.bounds; go.transform.position += new UnityEngine.Vector3(b.center.x - nb.center.x, b.min.y - nb.min.y, b.center.z - nb.center.z);
        go.transform.SetParent(slot, true); nr.sharedMaterial = mats[id]; nr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; nr.receiveShadows = false;
        placed++; tris += nr.GetComponent<UnityEngine.MeshFilter>().sharedMesh.triangles.Length / 3;
    }
}
UnityEditor.AssetDatabase.SaveAssets();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
return "placed " + placed + " kept " + kept + " tris " + tris + "\n" + sb.ToString();
