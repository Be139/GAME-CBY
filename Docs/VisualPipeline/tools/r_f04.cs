// 17F04 Mia and Lily. Time: night. Private and familiar: low lamps, cat things, a child's drawings, a school board half finished on the desk. The evening sun is kept out: this is hours after dark. Laid out from the plan.
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

Hide(new[] { "Carpet_02", "Puzzle", "HomeLamp_03", "Houseplant_01", "BookPile4", "TeddyBear_03", "Sofa3", "KitchenChair_01_01", "KitchenChair_01_02", "KitchenChair_01_03", "HomeLamp_01", "Houseplant_03", "Cabinet_03", "Bed", "Desk", "Shelve_13", "Chest", "WardRobe_01", "WardRobe_02", "Houseplant_04", "KidsCube_U_02" });
SunBlock("F04_SunBlockS", V(-3.5f, 1.5f, -37.2f), V(15f, 3.2f, 0.1f));
SunBlock("F04_SunBlockE", V(2.9f, 1.5f, -30f), V(0.1f, 3.2f, 13f));

// living room
var sofa = Back("sofa_modern", "Prop_Sofa", 'F', 2.8f, -2.9f, -34.2f, 0, "D8C4AE", true);
Put("throw_pillows", "Prop_SofaPillows", 'F', 0.5f, -2.95f, 0.42f, -33.2f, -80f, false, 0.3f, null);
Put("folded_throw", "Prop_SofaThrow", 'F', 0.5f, -2.9f, 0.42f, -35.1f, 90f, false, 0.3f, null);
Grade("Table_11", "B8A088", 0.2f);
Grade("TV", "FFFFFF", 0.2f);
var coffee = Stand("coffee_table", "Prop_CoffeeTable", 'F', 1.1f, -4.05f, -34.2f, 90f, "B8A088", true);
On(coffee, "book_stack", "Prop_Books", 'F', 0.26f, 0f, 0.28f, 15f, null);
On(coffee, "mug_pair", "Prop_Mugs", 'F', 0.2f, 0f, -0.25f, 60f, null);
Rug("F04_Rug", SharedDir + "/rug_rust.png", -4.8f, -33.6f, 3.6f, 2.5f, 0f);
var sofaSide = Stand("side_table_round", "Prop_SofaTable", 'H', 0.55f, -2.85f, -32.35f, 0f, null, true);
Lit(On(sofaSide, "table_lamp", "Prop_SofaLamp", 'H', 0.42f, 0f, 0f, 0f, null), "table_lamp", "FFC48A", 0.55f);
ArtW("F04_Art", SharedDir + "/art_home.png", -3.3f, 1.72f, -34.2f, 0, 1.5f, 1f);
Grade("RoomTable_01", "9A7A5C", 0.2f);
Stand("dining_chair", "Prop_DiningChair_1", 'H', 0.95f, -9.15f, -33.6f, 90f, "E0CDB8", true);
Stand("dining_chair", "Prop_DiningChair_2", 'H', 0.95f, -7.3f, -34f, -90f, "E0CDB8", true);
Stand("dining_chair", "Prop_DiningChair_3", 'H', 0.95f, -7.3f, -33.2f, -90f, "E0CDB8", true);
On(Old("RoomTable_01"), "fruit_bowl", "Prop_FruitBowl", 'F', 0.3f, 0f, 0.45f, 0f, null);
Hang("Prop_Pendant", -8.2f, 2.44f, -33.6f, 0.45f, 2.9f, "FFC48A", 1.3f);
Lit(Stand("floor_lamp", "Prop_FloorLamp", 'H', 1.6f, -9.05f, -31.6f, 0f, null, false), "floor_lamp", "FFC48A", 0.6f);
Stand("plant", "Prop_Plant", 'H', 1.25f, -9f, -35.3f, 0f, null, false);
Put("hanging_plant", "Prop_HangingPlant", 'H', 0.7f, -5.1f, 2.2f, -35.3f, 0f, false, 0.3f, null);
foreach (var n in new[] { "Kitchen_3" }) Grade(n, "E0D4C4", 0.22f);

// the cat lives here
Stand("cat_tree", "Prop_CatTree", 'H', 1f, -8f, -31.45f, 0f, null, false);
Stand("cat_bed", "Prop_CatBed", 'F', 0.55f, -3.6f, -31.75f, 0f, null, false);
Stand("cat_bowls", "Prop_CatBowls", 'F', 0.42f, -4.3f, -30.2f, 90f, null, false);
Stand("litter_box", "Prop_LitterBox", 'F', 0.55f, -4.3f, -28.6f, 90f, null, false);

// hall and entry
var board = Back("sideboard", "Prop_Sideboard", 'F', 1.7f, -4.2f, -26f, 1, null, true);
Lit(On(board, "table_lamp", "Prop_EntryLamp", 'H', 0.42f, 0f, 0.5f, 0f, null), "table_lamp", "FFC48A", 0.55f);
On(board, "photo_frame", "Prop_EntryPhoto", 'H', 0.2f, 0f, -0.15f, 100f, null);
On(board, "small_plant", "Prop_EntryPlant", 'H', 0.28f, 0f, -0.6f, 0f, null);
OnWall("wall_shelf", "Prop_WallShelf", 'F', 1.3f, -4.3f, 1.62f, -26f, 1, null);
OnWall("wall_mirror", "Prop_Mirror", 'H', 1.2f, -2.7f, 0.85f, -27.85f, 0, null);
Back("entry_bench", "Prop_EntryBench", 'F', 1f, -2.75f, -26.4f, 0, null, true);
OnWall("coat_hooks", "Prop_CoatHooks", 'F', 1.1f, -2.75f, 1.2f, -26.4f, 0, null);
Stand("slippers", "Prop_Slippers", 'F', 0.3f, -2.85f, -25.5f, 20f, null, false);
Stand("umbrella_stand", "Prop_Umbrellas", 'H', 0.7f, -2.7f, -27.25f, 0f, null, false);
OnWall("home_panel", "Prop_HomePanel", 'H', 0.22f, -4.3f, 1.25f, -25.3f, 2, null);

// Lily's room
var bed = Back("bed", "Prop_Bed", 'F', 2f, -1.72f, -34.7f, 3, null, true); Retex(bed, "bed", "bed_lily");
Put("plush_bunny", "Prop_Bunny", 'H', 0.28f, -1.95f, 0.52f, -35.3f, 60f, false, 0.3f, null);
Put("plush_dino", "Prop_Plush", 'H', 0.26f, -1.5f, 0.52f, -35.25f, 150f, false, 0.3f, null);
var stand = Back("bedside_table", "Prop_BedsideTable", 'F', 0.48f, -0.85f, -35.5f, 3, "F0E8E0", true);
Lit(On(stand, "night_light", "Prop_NightLight", 'H', 0.16f, -0.08f, 0f, 0f, null), "night_light", "FFD9A8", 0.8f);
On(stand, "alarm_clock", "Prop_Alarm", 'H', 0.13f, 0.1f, 0.05f, 180f, null);
var box = Back("toybox", "Prop_ToyBox", 'F', 0.9f, 0.1f, -35.4f, 3, null, true);
Stand("toy_blocks", "Prop_Blocks", 'F', 0.36f, 0.7f, -34.3f, 20f, null, false);
Back("wardrobe", "Prop_Wardrobe", 'H', 2.05f, 1.5f, -35.4f, 3, "E8E4D8", true);
var desk = Back("desk", "Prop_Desk", 'F', 1.35f, 1.25f, -30.4f, 2, "F4E4D4", true);
Lit(On(desk, "table_lamp", "Prop_DeskLamp", 'H', 0.4f, 0.5f, 0.08f, 0f, null), "table_lamp", "FFC48A", 0.55f);
On(desk, "school_board", "Prop_SchoolBoard", 'H', 0.42f, -0.2f, 0.12f, 180f, null);
On(desk, "book_stack", "Prop_DeskBooks", 'F', 0.22f, -0.05f, -0.2f, 10f, null);
On(desk, "pencil_cup", "Prop_PencilCup", 'H', 0.14f, 0.2f, -0.18f, 0f, null);
Stand("chair", "Prop_DeskChair", 'H', 0.85f, 1.25f, -31.2f, 0f, "F4E4D4", true);
var shelf = Back("bookshelf", "Prop_Bookshelf", 'F', 1.3f, -2f, -32.4f, 1, "F4E4D4", true);
On(shelf, "toy_blocks", "Prop_ShelfBlocks", 'F', 0.3f, 0f, -0.3f, 30f, null);
On(shelf, "books_row", "Prop_ShelfBooks", 'F', 0.42f, 0f, 0.32f, 90f, null);
Stand("floor_cushion", "Prop_Cushion", 'F', 0.55f, -0.75f, -30.75f, 0f, null, false);
Stand("backpack", "Prop_Backpack", 'H', 0.4f, 0.2f, -30.3f, 180f, null, false);
Stand("storage_basket", "Prop_Basket", 'F', 0.5f, 1.75f, -34.3f, 0f, null, false);
Rug("F04_Rug_Lily", NoahDir + "/shared/rug.png", 0.2f, -32.7f, 2.4f, 2f, 0f);
Grade("TeaParty", "FFF4E8", 0.2f);
Move("TeaParty", -0.75f, -33.4f);
Move("KidsCube_C", 0.95f, -33.95f);
ArtW("F04_Kid_1", SharedDir + "/kid_family.png", -1f, 1.62f, -32.9f, 1, 0.42f, 0.42f);
ArtW("F04_Kid_2", SharedDir + "/kid_robot.png", -1f, 1.62f, -32.4f, 1, 0.42f, 0.42f);
ArtW("F04_Kid_3", SharedDir + "/kid_school.png", -1f, 1.62f, -31.9f, 1, 0.42f, 0.42f);
ArtW("F04_Art_Lily", SharedDir + "/art_lily.png", 0.5f, 1.55f, -32f, 2, 0.75f, 0.5f);
Blinds("F04_Blinds_Side", -9.4f, -34.6f, -31.3f, 0.9f, 2.48f, -1f, "B8AE9E");
Blinds("F04_Blinds_Lily", 2.2f, -34.5f, -31.2f, 0.9f, 2.48f, 1f, "E8E4D8");

// spare room
Back("wardrobe", "Prop_SpareWardrobe", 'H', 2f, 0.7f, -29.7f, 3, "E8E4D8", true);
Stand("storage_boxes", "Prop_SpareBoxes", 'H', 1f, 1.5f, -27f, 20f, null, false);
Stand("laundry_basket", "Prop_SpareLaundry", 'H', 0.5f, 0.7f, -26.2f, 0f, null, false);

// home at night: small warm pockets from lamps she actually uses, blue from the windows everywhere else
Sp("Light_FloorLamp", true, V(-9.05f, 1.5f, -31.6f), V(-7.8f, 0f, -32.6f), "FFC48A", 1.5f, 5f, 130f, 50f);
Pt("Bake_LampGlow", V(-8.95f, 1.45f, -31.6f), "FFC48A", 1f, 4f);
Sp("Light_Pendant", true, V(-8.2f, 2.4f, -33.6f), V(-8.2f, 0f, -33.6f), "FFC48A", 3.5f, 5f, 100f, 55f);
Pt("Bake_PendantGlow", V(-8.2f, 1.7f, -33.6f), "FFC48A", 0.7f, 3.5f);
Pt("Bake_SofaLamp", V(-3.1f, 1f, -32.3f), "FFBE82", 0.9f, 3.5f);
Pt("Bake_KitchenFill", V(-6.5f, 1.5f, -29.6f), "FFD8B0", 0.9f, 3.5f);
Pt("Bake_EntryLamp", V(-3.9f, 1.3f, -25.5f), "FFC48A", 1f, 4f);
Pt("Bake_Fill_Living", V(-6f, 1.7f, -32.5f), "8FA6D8", 0.9f, 9f);
Pt("Bake_Fill_Hall", V(-3.5f, 1.7f, -27.5f), "8FA6D8", 0.6f, 6f);
Rect("Bake_BalconyDoor", V(-6.65f, 1.25f, -35.6f), 4.9f, 2.4f, 0f, "6F88CC", 1f, 10f);
Rect("Bake_SideWindow", V(-9.3f, 1.7f, -32.8f), 3.3f, 1.5f, 90f, "6F88CC", 0.8f, 8f);
// Lily's room: one lamp, teal walls, blue windows
Sp("Light_LilyDesk", true, V(1.75f, 1.2f, -30.3f), V(0.6f, 0.3f, -32f), "FFC890", 1.2f, 5f, 130f, 45f);
Pt("Bake_LilyGlow", V(1.75f, 1.2f, -30.45f), "FFC48A", 1f, 3.5f);
Pt("Bake_LilyFill", V(0f, 1.7f, -32.5f), "8FA6D8", 0.7f, 6f);
Pt("Bake_SpareFill", V(0f, 1.7f, -26.8f), "8FA6D8", 0.6f, 5f);
Rect("Bake_LilyWindow_E", V(2.1f, 1.7f, -32.8f), 3.3f, 1.5f, -90f, "6F88CC", 0.75f, 8f);
Rect("Bake_LilyWindow_S", V(-0.05f, 1.7f, -35.8f), 1.9f, 1.5f, 0f, "6F88CC", 0.75f, 8f);
Probe("Probe_F04Main", V(-5.9f, 1.4f, -31f), V(7f, 3f, 9.8f), 4);
Probe("Probe_F04Lily", V(0f, 1.4f, -32.8f), V(4.4f, 3f, 6.2f), 4);
ProbeGrid("Probes_F04Main", -9f, -2.8f, -35.4f, -25.4f, 1.8f, new[] { 0.5f, 1.5f, 2.5f });
ProbeGrid("Probes_F04Lily", -1.9f, 1.9f, -35.4f, -30.3f, 1.6f, new[] { 0.5f, 1.5f, 2.5f });
ProbeGrid("Probes_F04Spare", -1.9f, 1.9f, -29.4f, -25.3f, 1.6f, new[] { 0.5f, 1.5f, 2.5f });
LightsOff(new[] { "Lobby/Spot Light (9)/Point Light (6)", "Lobby/Spot Light (9)/Point Light (7)" });
