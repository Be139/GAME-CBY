// 17F public floor: arrival hall, the corridor and the wide south hall. Time 18:10 to 19:00. A quiet residential floor: teal walls with a wainscot railM, deep carpet, warm lights in sequence, a small lounge in the south hall. The low sun stays out.
Room("Floor17", "VisualPass_Corridor", 111.68f);
var wallM = Mat("Cor_Wall", "6C7F80", 0.1f, null);
Copy("Mesh19", "Cor_WallsA", wallM, 0, 0.8f, 2f, 0f);
Copy("Mesh53", "Cor_WallsB", Mat("Cor_Wainscot", "566866", 0.14f, null), 0, 0.8f, 2f, 0f);
foreach (var n in new[] { "Mesh3", "Mesh8", "Mesh49", "Mesh50", "Mesh56", "Mesh57", "Mesh59" }) Copy(n, "Cor_Extra_" + n, wallM, 0, 0.7f, 2f, 0f);
CopySplit("Mesh60", "Cor_SkinA", wallM, new[] { new[] { -9.7f, -6.5f, -24f, 1f }, new[] { -19f, 2.6f, -24.9f, -12.9f }, new[] { -13f, -6.5f, 0.5f, 11.6f }, new[] { -19f, -12.7f, -2f, 7f } }, 0.7f);
CopySplit("Mesh21", "Cor_SkinB", wallM, new[] { new[] { -9.7f, -6.5f, -24f, 1f }, new[] { -19f, 2.6f, -24.9f, -12.9f }, new[] { -13f, -6.5f, 0.5f, 11.6f }, new[] { -19f, -12.7f, -2f, 7f } }, 0.7f);
Copy("Mesh40", "Cor_Floor", Mat("Cor_Carpet", "3A3F4A", 0.06f, null), 1, 0.7f, 2f, 0f);
Copy("Mesh40", "Cor_Ceiling", Mat("Cor_Ceiling", "A9A99E", 0.05f, null), 2, 0.6f, 2f, 2.895f);
SunBlock("Cor_SunBlock", V(-14f, 1.5f, -26.2f), V(14f, 3.2f, 0.1f));

var railM = Mat("Cor_Rail", "C8BCA8", 0.2f, null); var skirtM = Mat("Cor_Skirt", "3C3530", 0.2f, null);
// wainscot railM and skirting down both corridor walls, broken at the doors
System.Action<string, float, float, float, float> WallBand = (name, x, a, b, side) => {
    float d = WallAt(x, 1.2f, (a + b) / 2f, side > 0f ? 0 : 1); float at = x + side * (d - 0.012f); float c = (a + b) / 2f, len = b - a;
    Box(name + "_Rail", propsT, railM, V(at, 0.95f, c), V(0.024f, 0.05f, len), 0f, 0.5f);
    Box(name + "_Skirt", propsT, skirtM, V(at, 0.07f, c), V(0.024f, 0.14f, len), 0f, 0.5f);
};
WallBand("Cor_BandE1", -8.1f, -14.9f, 1.3f, 1f);
WallBand("Cor_BandW1", -8.1f, -12.8f, -3.6f, -1f);
WallBand("Cor_BandW2", -8.1f, -2.1f, 0.6f, -1f);

// one sconce beside every apartment door
System.Action<string, float, float, float> Sconce = (name, u, w, yaw) => {
    Lit(Put("wall_sconce", "Prop_Sconce_" + name, 'H', 0.3f, u, 1.65f, w, yaw, false, 0.3f, null), "wall_sconce", "FFC48A", 1.5f);
    var d = UnityEngine.Quaternion.Euler(0, yaw, 0) * UnityEngine.Vector3.forward;
    Pt("Bake_Sconce_" + name, V(u, 1.85f, w) + d * 0.25f, "FFC48A", 1.6f, 5f);
};
Sconce("F01", -6.72f, 0.9f, -90f);
Sconce("F02", -6.62f, -17f, -90f);
Sconce("F03", -9.52f, -1.6f, 90f);
Sconce("F04", -2f, -24.72f, 0f);
Put("door_mat", "Prop_Mat_F01", 'F', 1f, -7.1f, 0f, 2.1f, 90f, false, 0.3f, null);
Put("door_mat", "Prop_Mat_F02", 'F', 1f, -7f, 0f, -15.7f, 90f, false, 0.3f, null);
Put("door_mat", "Prop_Mat_F03", 'F', 1f, -9.2f, 0f, -2.9f, 90f, false, 0.3f, null);
Put("door_mat", "Prop_Mat_F04", 'F', 1f, -3.2f, 0f, -24.25f, 0f, false, 0.3f, null);

// ceiling lights in sequence: corridor, then a grid across the south hall
var panelM = Glow("Cor_Panel", "FFE2BE", 1.2f);
System.Action<string, float, float> Ceil = (name, u, w) => {
    var b = Box("Cor_Panel_" + name, shellT, panelM, V(u, 2.885f, w), V(0.5f, 0.02f, 0.5f), 0f, 0f); b.GetComponent<UnityEngine.MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; UnityEditor.GameObjectUtility.SetStaticEditorFlags(b, 0);
    Sp("Bake_Ceil_" + name, false, V(u, 2.84f, w), V(u, 0f, w), "FFD9B0", 6f, 5f, 100f, 50f);
};
foreach (var w in new[] { 8f, 4.5f, 0f, -5f, -10f, -14.5f }) Ceil("C" + w, -8.1f, w);
foreach (var u in new[] { -16f, -12f, -8f, -4f, 0f }) { Ceil("H" + u, u, -19f); Ceil("I" + u, u, -22.5f); }
foreach (var w in new[] { 8f, 4f, -5f, -14f }) Pt("Bake_CorFill_" + w, V(-8.1f, 1.7f, w), "A8B4C8", 0.7f, 7f);
foreach (var u in new[] { -14f, -6f, 0f }) Pt("Bake_HallFill_" + u, V(u, 1.7f, -20.5f), "A8B4C8", 0.8f, 9f);

// arrival hall
LiftDoor("Cor_Lift", 3f, -12.72f, true, 1f, 2.1f);
Back("outdoor_bench", "Prop_Bench_Arrival", 'F', 1.8f, -7.15f, 6.5f, 0, "8A7C6C", true);
Stand("lobby_planter", "Prop_Planter_Arrival", 'H', 1.9f, -7.5f, 9.6f, 0f, null, true);
Stand("public_kiosk", "Prop_Kiosk_Arrival", 'H', 1.75f, -10.6f, 9.8f, 180f, null, false);
Stand("trash_bin", "Prop_Bin_Arrival", 'H', 0.8f, -7.1f, 4.9f, 0f, null, false);

// corridor walls
ArtW("Cor_Art_1", SharedDir + "/art_landscape.png", -8.1f, 1.6f, -7.5f, 1, 1.2f, 0.8f);
ArtW("Cor_Art_2", SharedDir + "/art_triptych_a.png", -8.1f, 1.6f, -9.8f, 0, 0.6f, 0.9f);
ArtW("Cor_Art_3", SharedDir + "/art_triptych_b.png", -8.1f, 1.6f, -4.8f, 0, 0.6f, 0.9f);
OnWall("fire_cabinet", "Prop_FireCabinet", 'H', 0.6f, -8.1f, 1.05f, -12f, 0, null);
Stand("umbrella_stand", "Prop_Umbrella_F03", 'H', 0.7f, -9.3f, -1.05f, 0f, null, false);

// south hall: a small residents' lounge
Rug("Cor_Rug_Lounge", SharedDir + "/rug_navy.png", -12.5f, -21.2f, 3.6f, 2.6f, 0f);
Stand("sofa_modern", "Prop_Sofa_Lounge", 'F', 2.6f, -12.5f, -22.6f, 0f, "A8B0B0", true);
Stand("armchair", "Prop_Armchair_Lounge_1", 'F', 0.85f, -14.8f, -21.2f, 90f, "B8AE9E", true);
Stand("armchair", "Prop_Armchair_Lounge_2", 'F', 0.85f, -10.2f, -21.2f, -90f, "B8AE9E", true);
var ct = Stand("coffee_table", "Prop_Table_Lounge", 'F', 1.2f, -12.5f, -21.1f, 0f, "8A7C6C", true);
On(ct, "magazine_stack", "Prop_Mags_Lounge", 'F', 0.28f, 0f, 0f, 10f, null);
Lit(Stand("floor_lamp", "Prop_Lamp_Lounge", 'H', 1.6f, -14.5f, -23f, 0f, null, false), "floor_lamp", "FFC48A", 0.7f);
Pt("Bake_LoungeLamp", V(-14.5f, 1.45f, -23f), "FFC48A", 1.4f, 5f);
Stand("lobby_planter", "Prop_Planter_Hall_1", 'H', 2.2f, -17.6f, -23.7f, 0f, null, true);
Stand("lobby_planter", "Prop_Planter_Hall_2", 'H', 2.2f, 1.4f, -23.6f, 0f, null, true);
Stand("snake_plant", "Prop_SnakePlant_Hall", 'H', 1f, -9.9f, -23.6f, 0f, null, false);
Stand("info_stand", "Prop_Info_Hall", 'H', 1.5f, -6.5f, -18.5f, 90f, null, false);
ProbeGrid("Probes_Corridor", -9.2f, -7f, -13f, 10f, 2.2f, new[] { 0.5f, 1.5f, 2.5f });
ProbeGrid("Probes_Hall", -18f, 2f, -24f, -17.5f, 3f, new[] { 0.5f, 1.5f, 2.5f });
LightsOff(new[] { "DuskLighting/Spot Light" });
