// 17F01 living, kitchen and dining. Time: the next morning. Warm cream, one sun beam, a family home with traces of a child. Laid out from the plan: every piece stands against a real wall or on a real surface.
const string Planks = SharedDir + "/T_Planks.png";
Room("Floor17/Apartment_17F01", "VisualPass_17F01Living", 111.68f);
Copy("Mesh18", "F01_Walls", Mat("F01_Wall", "D9CDB8", 0.08f, null), 0, 1f, 2f, 0f);
Copy("Mesh27", "F01_Floor", Mat("F01_Floor", "C4A27A", 0.3f, Planks), 1, 1f, 2f, 0f);
Copy("Mesh27", "F01_Ceiling", Mat("F01_Ceiling", "E6E0D4", 0.05f, null), 2, 0.7f, 2f, 2.9f);
Copy("Mesh2", "F01_Skirting", Mat("F01_Trim", "EDE6D8", 0.2f, null), 0, 1.2f, 2f, 0f);

var a1 = apt; apt = UnityEngine.GameObject.Find("Floor17/Apartment_17F02").transform; Hide(new[] { "Sofa_06_D" }); apt = a1;
Hide(new[] { "Cabinet_06", "TvUnitSet11", "Console_01", "Flower_02_02", "Donut_02_02", "Donut_01_02", "Donut_01_04", "Cupcake_02", "Chair_01", "Chair_02", "Painting_2", "Carpet_02", "Table_01", "Flower_05" });

// lounge
var sofa = Back("sofa_modern", "Prop_Sofa", 'F', 2.7f, 0.05f, 2.8f, 3, "F0E2CC", true);
Put("throw_pillows", "Prop_SofaPillows", 'F', 0.5f, -0.95f, 0.42f, 2.75f, 15f, false, 0.3f, null);
Put("folded_throw", "Prop_SofaThrow", 'F', 0.5f, 1.15f, 0.42f, 2.8f, 0f, false, 0.3f, null);
var tv = Back("tv_console", "Prop_TvConsole", 'F', 2.6f, 0f, 5.95f, 2, null, true);
Rug("F01_Rug", SharedDir + "/rug_rust.png", 0.1f, 4.35f, 3f, 2.1f, 0f);
var table = Stand("coffee_table", "Prop_CoffeeTable", 'F', 1.2f, 0.1f, 4.3f, 0f, null, true);
On(table, "book_stack", "Prop_Books", 'F', 0.26f, -0.3f, 0.05f, 15f, null);
On(table, "mug_pair", "Prop_Mugs", 'F', 0.2f, 0.28f, -0.05f, 0f, null);
Stand("armchair", "Prop_Armchair", 'F', 0.85f, 1.4f, 4.9f, -125f, "F0E6D8", true);
Lit(Stand("floor_lamp", "Prop_FloorLamp", 'H', 1.6f, -1.6f, 2.5f, 0f, null, false), "floor_lamp", "FFD9A8", 1.4f);
Stand("plant", "Prop_Plant", 'H', 1.25f, 1.65f, 5.95f, 0f, null, false);
var sideT = Stand("side_table_round", "Prop_SideTable", 'H', 0.55f, 1.72f, 2.55f, 0f, null, false);
On(sideT, "photo_frame", "Prop_SidePhoto", 'H', 0.2f, 0f, 0f, 200f, null);
Stand("floor_cushion", "Prop_Cushion", 'F', 0.55f, -0.9f, 4.95f, 0f, null, false);
Stand("toy_blocks", "Prop_Blocks", 'F', 0.4f, 1f, 3.65f, 30f, null, false);
ArtW("F01_Art", SharedDir + "/art_landscape.png", 0.2f, 1.72f, 3f, 3, 1.5f, 1f);
On(tv, "smart_speaker", "Prop_Speaker", 'H', 0.16f, -1f, 0f, 0f, null);

// dining
var dining = Stand("dining_table", "Prop_DiningTable", 'F', 1.9f, -3.1f, 5.2f, 90f, null, true);
Stand("dining_chair", "Prop_DiningChair_1", 'H', 0.85f, -3.95f, 4.85f, 90f, null, true);
Stand("dining_chair", "Prop_DiningChair_2", 'H', 0.85f, -3.95f, 5.65f, 90f, null, true);
Stand("dining_chair", "Prop_DiningChair_3", 'H', 0.85f, -2.25f, 4.85f, -90f, null, true);
Stand("dining_chair", "Prop_DiningChair_4", 'H', 0.85f, -2.25f, 5.65f, -90f, null, true);
On(dining, "fruit_bowl", "Prop_FruitBowl", 'F', 0.3f, 0f, 0.25f, 0f, null);
On(dining, "mug_pair", "Prop_TableMugs", 'F', 0.2f, -0.1f, -0.45f, 40f, null);
On(dining, "magazine_stack", "Prop_Paper", 'F', 0.26f, 0.2f, 0.7f, 8f, null);
Hang("Prop_Pendant", -3.1f, 2.44f, 5.2f, 0.45f, 2.9f, "FFD9A8", 1f);
var board = Back("sideboard", "Prop_Sideboard", 'F', 1.3f, -4.95f, 5.9f, 2, null, true);
On(board, "vase_stems", "Prop_Vase", 'H', 0.5f, -0.35f, 0f, 0f, null);
On(board, "photo_frame", "Prop_BoardPhoto", 'H', 0.2f, 0.3f, 0f, 170f, null);
ArtW("F01_Art_Family", SharedDir + "/photo_family.png", -4.95f, 1.7f, 5.5f, 2, 0.75f, 0.5f);
OnWall("wall_clock", "Prop_Clock", 'F', 0.36f, -1.75f, 1.9f, 5.5f, 2, null);

// kitchen: the old counters stay, tools go on top of them
foreach (var n in new[] { "KitchenTabletop2_01_01", "KitchenTabletop2_01_02", "KitchenTabletop2_02", "KitchenTabletop1_06_01", "KitchenTabletop1_06_02", "KitchenTabletop1_06_03", "KitchenShelf_01" }) Grade(n, "D6DCD2", 0.2f);
Matte("RangeHood", "E3DFD4", 0.2f);
On(Old("KitchenTabletop2_02"), "coffee_maker", "Prop_Coffee", 'H', 0.36f, 0f, 0.3f, 90f, null);
On(Old("KitchenTabletop2_01_02"), "dish_rack", "Prop_DishRack", 'F', 0.4f, 0.02f, 0.25f, 90f, null);
Stand("pedal_bin", "Prop_Bin", 'H', 0.55f, -5.5f, 3.05f, 0f, null, false);

// entry and hall: everything a small boy leaves behind, none of it in a doorway
On(Old("KitchenShelf_01"), "small_plant", "Prop_ShelfPlant", 'H', 0.28f, -0.7f, 0f, 0f, null);
On(Old("KitchenShelf_01"), "photo_frame", "Prop_ShelfPhoto", 'H', 0.2f, 0.5f, 0f, 170f, null);
Back("shoe_rack", "Prop_ShoeRack", 'F', 0.85f, -2.75f, 1.2f, 0, null, false);
OnWall("coat_hooks", "Prop_CoatHooks", 'F', 1f, -2.75f, 1.25f, 1.2f, 0, null);
Stand("backpack", "Prop_Backpack", 'H', 0.42f, -2.8f, 0.25f, -60f, null, false);
Stand("slippers", "Prop_Slippers", 'F', 0.3f, -5.4f, 2.5f, 20f, null, false);
ArtW("F01_Kid_1", NoahDir + "/shared/art_house.png", -3.2f, 1.45f, -0.7f, 1, 0.4f, 0.4f);
ArtW("F01_Kid_2", NoahDir + "/shared/art_rocket.png", -3.2f, 1.45f, -0.15f, 1, 0.4f, 0.4f);
OnWall("home_panel", "Prop_HomePanel", 'H', 0.22f, -3f, 1.25f, 0.4f, 0, null);

Apply("CurtainSet_10", Glow("F01_Sheer", "F3E6D2", 0.75f));
Box("F01_MorningWindow", shellT, Glow("F01_MorningGlass", "FFF1DC", 1.3f), V(2.22f, 1.25f, 4.2f), V(0.02f, 2.5f, 4.2f), 0f, 0f);
var mw = root.Find("Shell/F01_MorningWindow"); mw.GetComponent<UnityEngine.MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; UnityEditor.GameObjectUtility.SetStaticEditorFlags(mw.gameObject, 0);
foreach (var r in apt.Find("CurtainSet_10").GetComponentsInChildren<UnityEngine.MeshRenderer>(true)) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

// morning: a narrow low sun beam across the room, soft cool daylight in the shade, no warm fill
Sp("Light_MorningSun", true, V(7.5f, 4.2f, 4.6f), V(-3f, 0.2f, 4.4f), "FFDDB0", 150f, 18f, 40f, 30f);
Rect("Bake_MorningSky", V(1.9f, 1.25f, 4.2f), 4.1f, 2.4f, -90f, "EAF0F8", 1.2f, 12f);
Pt("Bake_Fill_Hall", V(-3.3f, 1.7f, 0.3f), "C8D4E8", 0.9f, 6f);
Pt("Bake_Fill_Living", V(-1f, 1.7f, 4.2f), "DCE4F0", 0.9f, 8f);
Pt("Bake_Fill_Kitchen", V(-5f, 1.7f, 4.5f), "DCE4F0", 0.8f, 6f);
Probe("Probe_F01", V(-2.2f, 1.4f, 4.2f), V(8.8f, 3f, 4.4f), 4);
ProbeGrid("Probes_F01", -6.1f, 1.7f, 2.4f, 5.9f, 1.6f, new[] { 0.5f, 1.5f, 2.5f });
ProbeGrid("Probes_F01Hall", -6.1f, -2.8f, -1.5f, 1.7f, 1.6f, new[] { 0.5f, 1.5f, 2.5f });
LightsOff(new[] { "Lobby/Spot Light (9)/Point Light", "Lobby/Spot Light (9)", "Floor17/Apartment_17F01/Lighting_17F_ROOM1_DuskSample/BillboardSpill_Magenta" });
