// Noah's ceiling sat behind the building body (2.919 above the floor): it moves down to 2.90 like the other rooms.
if (UnityEditor.EditorApplication.isPlaying) return "skip: play mode";
var t = UnityEngine.GameObject.Find("Floor17/Apartment_17F01/VisualPass_NoahRoom/Shell/Ceiling").transform;
var before = t.localPosition.y; var h = t.localScale.y; var p = t.localPosition; p.y = 2.90f + h / 2f; t.localPosition = p;
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
return "noah ceiling centre " + before.ToString("0.000") + " -> " + p.y.ToString("0.000");
