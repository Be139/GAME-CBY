// Test only: gives the lights near the 17F01 door and the 17F04 entry pure colours (MODE=tag) so one bake shows which light draws the stepped shapes; MODE=restore puts the saved colours back.
var file = @"D:/UGit/GAME-CBY/Docs/VisualPipeline/tools/_work/light_tag_saved.txt";
var plan = new[] {
    new object[] { "Floor17/VisualPass_Corridor", "Bake_Sconce_F01", UnityEngine.Color.green }, new object[] { "Floor17/VisualPass_Corridor", "Bake_Ceil_C0", UnityEngine.Color.red },
    new object[] { "Floor17/VisualPass_Corridor", "Bake_Ceil_C4.5", UnityEngine.Color.red }, new object[] { "Floor17/VisualPass_Corridor", "Bake_Ceil_C8", UnityEngine.Color.red },
    new object[] { "Floor17/VisualPass_Corridor", "Bake_CorFill_4", UnityEngine.Color.blue }, new object[] { "Floor17/VisualPass_Corridor", "Bake_CorFill_8", UnityEngine.Color.blue },
    new object[] { "Floor17/VisualPass_Corridor", "Bake_ArrivalWin_W", new UnityEngine.Color(1f, 1f, 0f) },
    new object[] { "Floor17/Apartment_17F01", "Bake_Fill_Hall", UnityEngine.Color.magenta }, new object[] { "Floor17/Apartment_17F01", "Light_MorningSun", UnityEngine.Color.cyan }, new object[] { "Floor17/Apartment_17F01", "Bake_MorningSky", UnityEngine.Color.cyan },
    new object[] { "Floor17/Apartment_17F04", "Bake_EntryLamp", UnityEngine.Color.green }, new object[] { "Floor17/Apartment_17F04", "Bake_Fill_Hall", UnityEngine.Color.cyan },
    new object[] { "Floor17/Apartment_17F04", "Bake_Fill_Living", UnityEngine.Color.blue }, new object[] { "Floor17/Apartment_17F04", "Bake_SofaLamp", UnityEngine.Color.red }, new object[] { "Floor17/Apartment_17F04", "Bake_LampGlow", UnityEngine.Color.magenta },
};
var sb = new System.Text.StringBuilder();
if ("MODE" == "tag") {
    var saved = new System.Text.StringBuilder();
    foreach (var p in plan) foreach (var l in UnityEngine.GameObject.Find((string)p[0]).GetComponentsInChildren<UnityEngine.Light>(true)) if (l.name == (string)p[1]) {
        saved.AppendLine(UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(l) + " " + UnityEngine.ColorUtility.ToHtmlStringRGBA(l.color)); l.color = (UnityEngine.Color)p[2]; sb.Append(l.name + " ");
    }
    System.IO.File.WriteAllText(file, saved.ToString());
    return "tagged " + sb;
}
foreach (var line in System.IO.File.ReadAllLines(file)) { var q = line.Split(' '); if (q.Length < 2) continue; UnityEditor.GlobalObjectId id; if (!UnityEditor.GlobalObjectId.TryParse(q[0], out id)) continue; var l = UnityEditor.GlobalObjectId.GlobalObjectIdentifierToObjectSlow(id) as UnityEngine.Light; UnityEngine.Color c; if (l != null && UnityEngine.ColorUtility.TryParseHtmlString("#" + q[1], out c)) { l.color = c; sb.Append(l.name + " "); } }
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene(); UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene); UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
return "restored " + sb;
