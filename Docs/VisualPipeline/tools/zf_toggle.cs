// Test helper: switches a named set of renderers off (MODE=off) or back on (MODE=on) without saving, to see which overlapping surface makes the wall patchwork.
var sets = new System.Collections.Generic.Dictionary<string, string[]> {
    { "wainscot", new[] { "Cor_WallsB" } },
    { "skins", new[] { "Cor_SkinA", "Cor_SkinB" } },
    { "old", new[] { "Group1/Mesh61", "Group16/Mesh14", "Group15/Mesh13", "Group2/Mesh1" } },
};
var want = sets["SET"]; int n = 0;
foreach (var r in UnityEngine.Object.FindObjectsOfType<UnityEngine.MeshRenderer>(true)) {
    var key = (r.transform.parent != null ? r.transform.parent.name + "/" : "") + r.name;
    foreach (var w in want) if (w == r.name || w == key) { r.enabled = "MODE" == "on"; n++; }
}
return "SET MODE " + n;
