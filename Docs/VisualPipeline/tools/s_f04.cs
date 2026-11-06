// 17F04 Mia and Lily. Time: night. Private and familiar: warm clay walls, low lamps, cat things, a child's drawing. Lily's room is soft teal with one lamp.
const string Planks = SharedDir + "/T_Planks.png";
Room("Floor17/Apartment_17F04", "VisualPass_17F04", 111.68f);
Copy("Mesh4", "F04_Main_Walls", Mat("F04_MainWall", "B3A08A", 0.08f, null), 0, 1f, 2f, 0f);
Copy("Mesh33", "F04_Main_Floor", Mat("F04_MainFloor", "8A6A4C", 0.3f, Planks), 1, 1f, 2f, 0f);
Copy("Mesh33", "F04_Main_Ceiling", Mat("F04_Ceiling", "D8C9B4", 0.05f, null), 2, 0.7f, 2f, 2.9f);
Copy("Mesh54", "F04_Lily_Walls", Mat("F04_LilyWall", "6E9694", 0.08f, null), 0, 1f, 2f, 0f);
Copy("Mesh35", "F04_Lily_Floor", Mat("F04_LilyFloor", "B8966E", 0.3f, Planks), 1, 1f, 2f, 0f);
Copy("Mesh35", "F04_Lily_Ceiling", Mat("F04_LilyCeiling", "D6D7CC", 0.05f, null), 2, 0.7f, 2f, 2.9f);
Copy("Mesh36", "F04_Spare_Floor", Mat("F04_LilyFloor", "B8966E", 0.3f, Planks), 1, 1f, 2f, 0f);
Copy("Mesh36", "F04_Spare_Ceiling", Mat("F04_LilyCeiling", "D6D7CC", 0.05f, null), 2, 0.7f, 2f, 2.9f);
Copy("Mesh17", "F04_Skirting", Mat("F04_Trim", "E4D9C8", 0.2f, null), 0, 1.2f, 2f, 0f);

Hide(new[] { "Carpet_02", "Puzzle", "HomeLamp_03", "Houseplant_01", "BookPile4", "TeddyBear_03" });

// living room and dining
Swap("Sofa3", "sofa_modern", 'F', 2.8f, Auto, 0f, "D8C4AE");
Grade("Table_11", "B8A088", 0.2f);
Grade("TV", "FFFFFF", 0.2f);
Rug("F04_Rug", SharedDir + "/rug_rust.png", -4.8f, -33.6f, 3.6f, 2.5f, 0f);
Grade("RoomTable_01", "9A7A5C", 0.2f);
Swap("KitchenChair_01_01", "dining_chair", 'H', 0.95f, 90f, 0f, "E0CDB8");
Swap("KitchenChair_01_02", "dining_chair", 'H', 0.95f, -90f, 0f, "E0CDB8");
Swap("KitchenChair_01_03", "dining_chair", 'H', 0.95f, -90f, 0f, "E0CDB8");
Hang("Prop_Pendant", -8.2f, 2.44f, -33.6f, 0.45f, 2.9f, "FFC48A", 1.3f);
Lit(Swap("HomeLamp_01", "floor_lamp", 'H', 1.6f, 0f, 0f, null), "floor_lamp", "FFC48A", 0.6f);
Swap("Houseplant_03", "plant", 'H', 1.25f, 0f, 0f, null);
var board = Swap("Cabinet_03", "sideboard", 'F', 1.8f, Auto, 0f, null);
Lit(Put("table_lamp", "Prop_EntryLamp", 'H', 0.42f, -4.2f, TopOf(board), -25.5f, 0f, false, 0.4f, null), "table_lamp", "FFC48A", 0.55f);
Put("photo_frame", "Prop_Photo", 'H', 0.2f, -4.2f, TopOf(board), -26.2f, -90f, false, 0.3f, null);
Art("F04_Art", SharedDir + "/art_city.png", -2.45f, 1.6f, -34.1f, 1.5f, 1f, -90f);
Put("wall_shelf", "Prop_WallShelf", 'F', 1.3f, -7f, 1.45f, -30.55f, 180f, false, 0.4f, null);
var sofaSide = Put("bedside_table", "Prop_SofaTable", 'F', 0.5f, -2.9f, 0f, -32.15f, -90f, true, 0.5f, "D8C4AE");
Lit(Put("table_lamp", "Prop_SofaLamp", 'H', 0.42f, -2.9f, TopOf(sofaSide), -32.15f, 0f, false, 0.4f, null), "table_lamp", "FFC48A", 0.55f);
Put("cat_bed", "Prop_CatBed", 'F', 0.55f, -3.5f, 0f, -31.6f, 0f, false, 0.4f, null);
Put("cat_tree", "Prop_CatTree", 'H', 1f, -2.95f, 0f, -30.6f, 0f, false, 0.4f, null);
Put("cat_bowls", "Prop_CatBowls", 'F', 0.42f, -4.9f, 0f, -29.5f, 0f, false, 0.3f, null);
Put("coat_hooks", "Prop_CoatHooks", 'F', 1.1f, -2.47f, 1.15f, -26.6f, -90f, false, 0.4f, null);
Put("entry_bench", "Prop_EntryBench", 'F', 1f, -2.72f, 0f, -26.6f, -90f, false, 0.5f, null);
Put("fruit_bowl", "Prop_FruitBowl", 'F', 0.3f, -8.2f, 0.91f, -33.1f, 0f, false, 0.3f, null);
Put("book_stack", "Prop_LivingBooks", 'F', 0.26f, -5.3f, 0.72f, -34.1f, 15f, false, 0.3f, null);
Put("floor_cushion", "Prop_LivingCushion", 'F', 0.55f, -4f, 0f, -32.2f, 0f, false, 0.4f, null);
Put("storage_boxes", "Prop_SpareBoxes", 'H', 1f, 1.5f, 0f, -27f, 20f, false, 0.4f, null);
Put("laundry_basket", "Prop_SpareLaundry", 'H', 0.5f, 0.7f, 0f, -26.2f, 0f, false, 0.4f, null);
Put("slippers", "Prop_Slippers", 'F', 0.3f, -3.2f, 0f, -25.6f, 20f, false, 0.3f, null);
foreach (var n in new[] { "Kitchen_3" }) Grade(n, "E0D4C4", 0.22f);

// Lily's room
var bed = Swap("Bed", "bed", 'F', 2f, 0f, 0f, null); Retex(bed, "bed", "bed_lily");
Put("plush_dino", "Prop_Plush", 'H', 0.3f, -1.5f, 0.5f, -34.6f, 150f, false, 0.3f, null);
var bedSide = Put("bedside_table", "Prop_BedsideTable", 'F', 0.5f, -0.15f, 0f, -35.45f, 0f, true, 0.5f, "F0E8E0");
var desk = Swap("Desk", "desk", 'F', 1.5f, Auto, 0f, "F4E4D4");
float dt = TopOf(desk);
Lit(Put("table_lamp", "Prop_DeskLamp", 'H', 0.42f, 1.75f, dt, -31.5f, 0f, false, 0.4f, null), "table_lamp", "FFC48A", 0.55f);
Put("book_stack", "Prop_Books", 'F', 0.24f, 1.7f, dt, -30.5f, 10f, false, 0.3f, null);
Put("pencil_cup", "Prop_PencilCup", 'H', 0.14f, 1.5f, dt, -30.1f, 0f, false, 0.3f, null);
Put("chair", "Prop_DeskChair", 'H', 0.85f, 0.75f, 0f, -30.9f, 90f, true, 0.5f, "F4E4D4");
Swap("Shelve_13", "bookshelf", 'H', 1.5f, Auto, 0f, "F4E4D4");
Swap("Chest", "toybox", 'F', 0.95f, Auto, 0f, null);
Swap("WardRobe_01", "wardrobe", 'F', 1.3f, Auto, 0f, "E8E4D8");
Swap("WardRobe_02", "wardrobe", 'F', 1.8f, Auto, 0f, "E8E4D8");
Put("backpack", "Prop_Backpack", 'H', 0.4f, 0.6f, 0f, -29.6f, 180f, false, 0.4f, null);
Put("floor_cushion", "Prop_Cushion", 'F', 0.55f, 0.9f, 0f, -33f, 0f, false, 0.4f, null);
Rug("F04_Rug_Lily", NoahDir + "/shared/rug.png", 0.6f, -32.9f, 2.4f, 2f, 0f);
Art("F04_Art_Lily", SharedDir + "/art_lily.png", 0.5f, 1.5f, -29.58f, 0.75f, 0.5f, 180f);
Grade("TeaParty", "FFF4E8", 0.2f);
Blinds("F04_Blinds_Side", -9.4f, -34.6f, -31.3f, 0.9f, 2.48f, -1f, "B8AE9E");
Blinds("F04_Blinds_Lily", 2.2f, -34.5f, -31.2f, 0.9f, 2.48f, 1f, "E8E4D8");

LightsOff(new[] { "Lobby/Spot Light (9)/Point Light (6)", "Lobby/Spot Light (9)/Point Light (7)" });

// home at night: small warm pockets from lamps she actually uses, blue from the windows everywhere else
Sp("Light_FloorLamp", true, V(-9.1f, 1.5f, -31.6f), V(-7.8f, 0f, -32.6f), "FFC48A", 1.5f, 5f, 130f, 50f);
Pt("Bake_LampGlow", V(-9f, 1.45f, -31.6f), "FFC48A", 1f, 4f);
Sp("Light_Pendant", true, V(-8.2f, 2.4f, -33.6f), V(-8.2f, 0f, -33.6f), "FFC48A", 3.5f, 5f, 100f, 55f);
Pt("Bake_PendantGlow", V(-8.2f, 1.7f, -33.6f), "FFC48A", 0.7f, 3.5f);
Pt("Bake_SofaLamp", V(-2.9f, 1.05f, -32.15f), "FFBE82", 0.9f, 3.5f);
Pt("Bake_KitchenFill", V(-6.5f, 1.5f, -29.6f), "FFD8B0", 0.9f, 3.5f);
Pt("Bake_EntryLamp", V(-4.2f, 1.3f, -25.6f), "FFC48A", 1f, 4f);
Pt("Bake_Fill_Living", V(-6f, 1.7f, -32.5f), "8FA6D8", 0.9f, 9f);
Pt("Bake_Fill_Hall", V(-3.5f, 1.7f, -27.5f), "8FA6D8", 0.6f, 6f);
Rect("Bake_BalconyDoor", V(-6.65f, 1.25f, -35.6f), 4.9f, 2.4f, 0f, "6F88CC", 1f, 10f);
Rect("Bake_SideWindow", V(-9.3f, 1.7f, -32.8f), 3.3f, 1.5f, 90f, "6F88CC", 0.8f, 8f);
// Lily's room: one lamp, teal walls, blue windows
Sp("Light_LilyDesk", true, V(1.75f, 1.2f, -31.5f), V(0.6f, 0.3f, -32.4f), "FFC890", 1.2f, 5f, 130f, 45f);
Pt("Bake_LilyGlow", V(1.7f, 1.3f, -31.6f), "FFC48A", 1f, 3.5f);
Pt("Bake_LilyFill", V(0f, 1.7f, -32.5f), "8FA6D8", 0.7f, 6f);
Pt("Bake_SpareFill", V(0f, 1.7f, -26.8f), "8FA6D8", 0.6f, 5f);
Rect("Bake_LilyWindow_E", V(2.1f, 1.7f, -32.8f), 3.3f, 1.5f, -90f, "6F88CC", 0.75f, 8f);
Rect("Bake_LilyWindow_S", V(-0.05f, 1.7f, -35.8f), 1.9f, 1.5f, 0f, "6F88CC", 0.75f, 8f);
Probe("Probe_F04Main", V(-5.9f, 1.4f, -31f), V(7f, 3f, 9.8f), 4);
Probe("Probe_F04Lily", V(0f, 1.4f, -32.8f), V(4.4f, 3f, 6.2f), 4);
ProbeGrid("Probes_F04Main", -9f, -2.8f, -35.4f, -25.4f, 1.8f, new[] { 0.5f, 1.5f, 2.5f });
ProbeGrid("Probes_F04Lily", -1.9f, 1.9f, -35.4f, -29.1f, 1.6f, new[] { 0.5f, 1.5f, 2.5f });
ProbeGrid("Probes_F04Spare", -1.9f, 1.9f, -28.2f, -25.3f, 1.6f, new[] { 0.5f, 1.5f, 2.5f });
