// New robot body on every unit in the scene: wheels, a flat black chest screen, no ears. The old mesh renderers are switched off and stay in place; interaction volumes, cameras and colliders are untouched, and the new body is a child so it follows any movement.
if (UnityEditor.EditorApplication.isPlaying) return "skip: play mode";
var sb = new System.Text.StringBuilder();
const float FrontYaw = FRONT_YAW;
const float Height = 1.42f;
var src = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/Art/Generated/Rooms/robot_a/robot_a.fbx");
var units = new System.Collections.Generic.List<UnityEngine.Transform>();
foreach (var t in UnityEngine.Object.FindObjectsOfType<UnityEngine.Transform>(true)) if (t.name == "Retopo_立方体.003" && t.parent != null && !units.Contains(t.parent)) units.Add(t.parent);
foreach (var unit in units) {
    var old = unit.Find("Body_Robot"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
    bool any = false; var b = new UnityEngine.Bounds(); var rs = new System.Collections.Generic.List<UnityEngine.Renderer>();
    foreach (var r in unit.GetComponentsInChildren<UnityEngine.Renderer>(true)) { rs.Add(r); r.enabled = true; if (!any) { b = r.bounds; any = true; } else b.Encapsulate(r.bounds); }
    foreach (var r in rs) r.enabled = false;
    var go = (UnityEngine.GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(src, unit); go.name = "Body_Robot";
    go.transform.localPosition = UnityEngine.Vector3.zero; go.transform.rotation = UnityEngine.Quaternion.Euler(0, unit.eulerAngles.y + FrontYaw, 0) * src.transform.localRotation;
    go.transform.SetParent(null, true); go.transform.localScale = UnityEngine.Vector3.one;
    var nr = go.GetComponentInChildren<UnityEngine.MeshRenderer>(); float h = nr.bounds.size.y; float k = Height / h; go.transform.localScale *= k;
    var nb = nr.bounds; go.transform.position += new UnityEngine.Vector3(b.center.x - nb.center.x, b.min.y - nb.min.y, b.center.z - nb.center.z);
    go.transform.SetParent(unit, true);
    sb.AppendLine(unit.name + " old " + b.size.ToString("0.00") + " new " + nr.bounds.size.ToString("0.00"));
}
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
return "units " + units.Count + "\n" + sb.ToString();
