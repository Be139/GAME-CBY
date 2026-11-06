// The rest of the tower (floors above, roof) is not part of the bake, so dusk sky light fell onto the ceiling copies from above and leaked down through the gaps where ceilings meet walls,
// drawing skyline-like teeth along the top of the walls (blurred into blotches before). A thin invisible roof over each baked space closes that. It only casts shadows and takes no lightmap space.
if (UnityEditor.EditorApplication.isPlaying) return "skip: play mode";
var sb = new System.Text.StringBuilder();
var cube = UnityEngine.Resources.GetBuiltinResource<UnityEngine.Mesh>("Cube.fbx");
var mat = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>("Assets/Art/Generated/Rooms/shared/M_Cor_Wall.mat");
System.Action<string, string, string[], float> Roof = (rootPath, name, shellRoots, yTop) => {
    var root = UnityEngine.GameObject.Find(rootPath).transform;
    var old = root.Find(name); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
    float u0 = 1e9f, u1 = -1e9f, v0 = 1e9f, v1 = -1e9f, yMax = -1e9f;
    foreach (var sp in shellRoots) { var s = UnityEngine.GameObject.Find(sp); if (s == null) { sb.Append("no " + sp + "; "); continue; }
        foreach (var r in s.GetComponentsInChildren<UnityEngine.MeshRenderer>()) { var b = r.bounds; for (int k = 0; k < 8; k++) { var p = root.InverseTransformPoint(b.center + UnityEngine.Vector3.Scale(b.extents, new UnityEngine.Vector3((k & 1) == 0 ? -1 : 1, (k & 2) == 0 ? -1 : 1, (k & 4) == 0 ? -1 : 1))); u0 = UnityEngine.Mathf.Min(u0, p.x); u1 = UnityEngine.Mathf.Max(u1, p.x); v0 = UnityEngine.Mathf.Min(v0, p.z); v1 = UnityEngine.Mathf.Max(v1, p.z); yMax = UnityEngine.Mathf.Max(yMax, p.y); } } }
    float y = yTop > 0 ? yTop : yMax + 0.05f;
    var g = new UnityEngine.GameObject(name); g.transform.SetParent(root, false);
    g.transform.localPosition = new UnityEngine.Vector3((u0 + u1) / 2f, y, (v0 + v1) / 2f); g.transform.localRotation = UnityEngine.Quaternion.identity; g.transform.localScale = new UnityEngine.Vector3(u1 - u0 + 3f, 0.01f, v1 - v0 + 3f);
    g.AddComponent<UnityEngine.MeshFilter>().sharedMesh = cube; var mr = g.AddComponent<UnityEngine.MeshRenderer>(); mr.sharedMaterial = mat;
    mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly; mr.receiveShadows = false; mr.scaleInLightmap = 0f;
    UnityEditor.GameObjectUtility.SetStaticEditorFlags(g, UnityEditor.StaticEditorFlags.ContributeGI);
    sb.Append(name + " u " + u0.ToString("0.0") + ".." + u1.ToString("0.0") + " v " + v0.ToString("0.0") + ".." + v1.ToString("0.0") + " y " + y.ToString("0.000") + " (shell top " + yMax.ToString("0.00") + "); ");
};
// 17F: ceilings sit at 2.895-2.90 and the tower's own ceiling mesh at 2.919, so the roof goes between them
Roof("Floor17/VisualPass_Corridor", "BakeRoof_17F", new[] { "Floor17/VisualPass_Corridor/Shell", "Floor17/Apartment_17F01/VisualPass_NoahRoom", "Floor17/Apartment_17F01/VisualPass_17F01Living/Shell", "Floor17/Apartment_17F02/VisualPass_17F02/Shell", "Floor17/Apartment_17F03/VisualPass_17F03/Shell", "Floor17/Apartment_17F04/VisualPass_17F04/Shell" }, 2.911f);
Roof("Lobby/VisualPass_Lobby", "BakeRoof_Lobby", new[] { "Lobby/VisualPass_Lobby/Shell" }, -1f);
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene(); UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene); UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
return sb.ToString();
