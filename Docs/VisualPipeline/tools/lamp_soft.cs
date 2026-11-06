// Small baked lamps sit a hand's width from the wall, so their light grazes along it and throws long, sharp shadows of door and picture frames (the stepped shapes beside sconces and lamps).
// Their baked shadows become very soft, and the corridor sconce lights move from 0.25 m to 0.4 m off the wall.
if (UnityEditor.EditorApplication.isPlaying) return "skip: play mode";
int soft = 0, moved = 0; var sb = new System.Text.StringBuilder();
foreach (var n in new[] { "Floor17/Apartment_17F01", "Floor17/Apartment_17F02", "Floor17/Apartment_17F03", "Floor17/Apartment_17F04", "Floor17/VisualPass_Corridor", "Lobby" })
    foreach (var l in UnityEngine.GameObject.Find(n).GetComponentsInChildren<UnityEngine.Light>(true)) {
        if (l.lightmapBakeType != UnityEngine.LightmapBakeType.Baked || l.type != UnityEngine.LightType.Point) continue;
        if (l.name.Contains("Sconce") || l.name.Contains("Glow") || l.name.Contains("Lamp")) { l.shadowRadius = 0.5f; soft++; }
    }
var cor = UnityEngine.GameObject.Find("Floor17/VisualPass_Corridor").transform;
foreach (var k in new[] { "F01", "F02", "F03", "F04" }) {
    var lamp = cor.Find("Props/Prop_Sconce_" + k); UnityEngine.Light lt = null; foreach (var l in cor.GetComponentsInChildren<UnityEngine.Light>(true)) if (l.name == "Bake_Sconce_" + k) lt = l;
    if (lamp == null || lt == null) { sb.Append("missing " + k + "; "); continue; }
    var d = lt.transform.position - lamp.position; float y = lt.transform.position.y; d.y = 0; if (d.magnitude < 0.01f) continue;
    var p = lamp.position + d.normalized * 0.4f; p.y = y; lt.transform.position = p; moved++;
}
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene(); UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene); UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
return "softened=" + soft + " sconcesMoved=" + moved + " " + sb;
