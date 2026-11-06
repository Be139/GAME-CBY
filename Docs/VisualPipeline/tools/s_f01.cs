// 17F01 living, kitchen and dining. Time: the next morning. Warm cream, soft sun, a family home with traces of a child.
const string Planks = SharedDir + "/T_Planks.png";
Room("Floor17/Apartment_17F01", "VisualPass_17F01Living", 111.68f);
Copy("Mesh18", "F01_Walls", Mat("F01_Wall", "D9CDB8", 0.08f, null), 0, 1f, 2f, 0f);
Copy("Mesh27", "F01_Floor", Mat("F01_Floor", "C4A27A", 0.3f, Planks), 1, 1f, 2f, 0f);
Copy("Mesh27", "F01_Ceiling", Mat("F01_Ceiling", "E6E0D4", 0.05f, null), 2, 0.7f, 2f, 2.9f);
Copy("Mesh2", "F01_Skirting", Mat("F01_Trim", "EDE6D8", 0.2f, null), 0, 1.2f, 2f, 0f);

var a1 = apt; apt = UnityEngine.GameObject.Find("Floor17/Apartment_17F02").transform;
Swap("Sofa_06_D", "sofa_modern", 'F', 2.9f, Auto, 0f, "F0E2CC"); apt = a1;
var tv = Swap("Cabinet_06", "tv_console", 'F', 2.6f, Auto, 0f, null);
Hide(new[] { "TvUnitSet11", "Console_01", "Flower_02_02", "Donut_02_02", "Donut_01_02", "Donut_01_04", "Cupcake_02", "Chair_01", "Chair_02", "Painting_2", "Carpet_02" });
var table = Swap("Table_01", "dining_table", 'F', 1.9f, 90f, 0f, null);
float top = TopOf(table);
Put("dining_chair", "Prop_DiningChair_1", 'H', 0.85f, -3.95f, 0f, 4.9f, 90f, true, 0.5f, null);
Put("dining_chair", "Prop_DiningChair_2", 'H', 0.85f, -3.95f, 0f, 5.7f, 90f, true, 0.5f, null);
Put("dining_chair", "Prop_DiningChair_3", 'H', 0.85f, -2.25f, 0f, 4.9f, -90f, true, 0.5f, null);
Put("dining_chair", "Prop_DiningChair_4", 'H', 0.85f, -2.25f, 0f, 5.7f, -90f, true, 0.5f, null);
Put("fruit_bowl", "Prop_FruitBowl", 'F', 0.3f, -3.1f, top, 5.4f, 0f, false, 0.3f, null);
Rug("F01_Rug", SharedDir + "/rug_rust.png", 0.1f, 4.4f, 3.0f, 2.2f, 0f);
Art("F01_Art", SharedDir + "/art_landscape.png", 0.2f, 1.62f, 2.08f, 1.5f, 1f, 0f);
Put("coffee_table", "Prop_CoffeeTable", 'F', 1.2f, 0.1f, 0f, 4.3f, 0f, true, 0.6f, null);
Put("armchair", "Prop_Armchair", 'F', 0.85f, -1.75f, 0f, 4.4f, 90f, true, 0.6f, "F0E6D8");
Lit(Put("floor_lamp", "Prop_FloorLamp", 'H', 1.6f, -1.85f, 0f, 2.55f, 0f, false, 0.4f, null), "floor_lamp", "FFD9A8", 1.4f);
Swap("Flower_05", "plant", 'H', 1.25f, 0f, 0f, null);
Put("toy_blocks", "Prop_Blocks", 'F', 0.4f, 1.35f, 0f, 3.3f, 30f, false, 0.4f, null);
Put("floor_cushion", "Prop_Cushion", 'F', 0.55f, 1.25f, 0f, 4.9f, 0f, false, 0.4f, null);
Hang("Prop_Pendant", -3.1f, 2.44f, 5.3f, 0.45f, 2.9f, "FFD9A8", 1f);
Put("photo_frame", "Prop_Photo", 'H', 0.2f, 1.05f, 0.55f, 5.9f, 180f, false, 0.3f, null);
Put("wall_shelf", "Prop_WallShelf", 'F', 1.3f, -4.2f, 1.5f, 2.1f, 0f, false, 0.4f, null);
Put("backpack", "Prop_Backpack", 'H', 0.42f, -5.9f, 0f, 2.3f, 60f, false, 0.4f, null);
Put("slippers", "Prop_Slippers", 'F', 0.3f, -5.5f, 0f, 1.3f, 20f, false, 0.3f, null);
Put("book_stack", "Prop_Books", 'F', 0.26f, 0.35f, 0.45f, 4.3f, 15f, false, 0.3f, null);
Put("small_plant", "Prop_SmallPlant", 'H', 0.32f, -5.95f, 0.92f, 3.3f, 0f, false, 0.3f, null);
foreach (var n in new[] { "KitchenTabletop2_01_01", "KitchenTabletop2_01_02", "KitchenTabletop2_02", "KitchenTabletop1_06_01", "KitchenTabletop1_06_02", "KitchenTabletop1_06_03", "KitchenShelf_01" }) Grade(n, "D6DCD2", 0.2f);
Matte("RangeHood", "E3DFD4", 0.2f);
Apply("CurtainSet_10", Glow("F01_Sheer", "F3E6D2", 0.75f));
Box("F01_MorningWindow", shellT, Glow("F01_MorningGlass", "FFF1DC", 1.3f), V(2.22f, 1.25f, 4.2f), V(0.02f, 2.5f, 4.2f), 0f, 0f);
var mw = root.Find("Shell/F01_MorningWindow"); mw.GetComponent<UnityEngine.MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; UnityEditor.GameObjectUtility.SetStaticEditorFlags(mw.gameObject, 0);
foreach (var r in apt.Find("CurtainSet_10").GetComponentsInChildren<UnityEngine.MeshRenderer>(true)) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

LightsOff(new[] { "Lobby/Spot Light (9)/Point Light", "Lobby/Spot Light (9)", "Floor17/Apartment_17F01/Lighting_17F_ROOM1_DuskSample/BillboardSpill_Magenta" });

// morning: a narrow low sun beam across the room, soft cool daylight in the shade, no warm fill
Sp("Light_MorningSun", true, V(7.5f, 4.2f, 4.6f), V(-3f, 0.2f, 4.4f), "FFDDB0", 150f, 18f, 40f, 30f);
Rect("Bake_MorningSky", V(1.9f, 1.25f, 4.2f), 4.1f, 2.4f, -90f, "EAF0F8", 1.2f, 12f);
Pt("Bake_Fill_Hall", V(-4.5f, 1.7f, 0.3f), "C8D4E8", 0.9f, 6f);
Pt("Bake_Fill_Living", V(-1f, 1.7f, 4.2f), "DCE4F0", 0.9f, 8f);
Pt("Bake_Fill_Kitchen", V(-5f, 1.7f, 4.5f), "DCE4F0", 0.8f, 6f);
Probe("Probe_F01", V(-2.2f, 1.4f, 4.2f), V(8.8f, 3f, 4.4f), 4);
ProbeGrid("Probes_F01", -6.1f, 1.7f, 2.4f, 5.9f, 1.6f, new[] { 0.5f, 1.5f, 2.5f });
ProbeGrid("Probes_F01Hall", -6.1f, -2.8f, -1.5f, 1.7f, 1.6f, new[] { 0.5f, 1.5f, 2.5f });
