// 17F03 Mark, Laura and Ava. Time: 19:00, lights on. Order and tension: everything square to the walls, binders and a schedule board, hard pools of light. Ava's room is the only place with a mess. Laid out from the plan.
const string Planks = SharedDir + "/T_Planks.png";
Room("Floor17/Apartment_17F03", "VisualPass_17F03", 111.68f);
Copy("Mesh16", "F03_Main_Walls", Mat("F03_MainWall", "A08C6E", 0.08f, null), 0, 1f, 2f, 0f);
Copy("Mesh43", "F03_Main_Floor", Mat("F03_MainFloor", "8A6A4C", 0.34f, Planks), 1, 1f, 2f, 0f);
Copy("Mesh43", "F03_Main_Ceiling", Mat("F03_Ceiling", "C9BEA9", 0.05f, null), 2, 0.7f, 2f, 2.9f);
Copy("Mesh9", "F03_Ava_Walls", Mat("F03_AvaWall", "8C8A88", 0.08f, null), 0, 1f, 2f, 0f);
Copy("Mesh31", "F03_Ava_Floor", Mat("F03_AvaFloor", "8C7A64", 0.3f, Planks), 1, 1f, 2f, 0f);
Copy("Mesh31", "F03_Ava_Ceiling", Mat("F03_AvaCeiling", "BDB8BF", 0.05f, null), 2, 0.7f, 2f, 2.9f);
Copy("Mesh6", "F03_Skirting", Mat("F03_Trim", "3F3227", 0.2f, null), 0, 1.2f, 2f, 0f);

Hide(new[] { "SofaChairSet9_Sofa", "TvUnitSet_Table2", "Carpet_05_01", "Carpet_05_02", "Carpet_04", "Lamp_03", "Chair", "HomeLamp_01", "TeddyBear_02", "DeskLight", "BookPile7", "PenHolder", "CurtainSet_3", "TelevisionFurniture", "DiningTable", "Bed1_01", "Desk2", "Dresser_01", "Dresser_02", "WardRobe_01", "WardRobe_02", "Clock" });

// living and dining: everything square to the walls
var tv = Back("tv_console", "Prop_TvConsole", 'F', 2.4f, -16.9f, -2.6f, 2, "A89888", true);
Stand("sofa_modern", "Prop_Sofa", 'F', 2.6f, -16.6f, -5.75f, 0f, "B8AE9E", true);
Put("throw_pillows", "Prop_SofaPillows", 'F', 0.5f, -17.6f, 0.42f, -5.7f, -15f, false, 0.3f, "C8BEAE");
Stand("armchair", "Prop_Armchair_1", 'F', 0.85f, -17.6f, -4.15f, 90f, "B8AE9E", true);
Stand("armchair", "Prop_Armchair_2", 'F', 0.85f, -14.6f, -4.35f, -90f, "B8AE9E", true);
var table = Stand("coffee_table", "Prop_CoffeeTable", 'F', 1.2f, -16.4f, -4.25f, 0f, "8A7C6C", true);
On(table, "magazine_stack", "Prop_Mags", 'F', 0.26f, -0.25f, 0f, 0f, null);
On(table, "mug_pair", "Prop_Mugs", 'F', 0.2f, 0.3f, 0f, 90f, null);
Rug("F03_Rug", SharedDir + "/rug_navy.png", -16.4f, -4.25f, 3.1f, 2.3f, 0f);
OnWall("wall_clock", "Prop_WallClock", 'F', 0.42f, -16.9f, 1.95f, -2.9f, 2, null);
Stand("snake_plant", "Prop_SnakePlant", 'H', 1f, -14.85f, -5.75f, 0f, null, false);
OnWall("wall_radiator", "Prop_Radiator", 'F', 1.4f, -17.9f, 0.18f, -4.05f, 1, null);
var dining = Stand("dining_table", "Prop_DiningTable", 'F', 1.7f, -16.2f, -8.1f, 90f, "A08C78", true);
Stand("dining_chair", "Prop_DiningChair_1", 'H', 0.85f, -17.1f, -8.1f, 90f, "B8AE9E", true);
Stand("dining_chair", "Prop_DiningChair_2", 'H', 0.85f, -15.3f, -8.1f, -90f, "B8AE9E", true);
Stand("dining_chair", "Prop_DiningChair_3", 'H', 0.85f, -16.2f, -6.92f, 180f, "B8AE9E", true);
On(dining, "fruit_bowl", "Prop_FruitBowl", 'F', 0.28f, 0f, 0f, 0f, null);
ArtW("F03_Art_A", SharedDir + "/art_order.png", -17f, 1.65f, -8f, 3, 0.55f, 0.55f);
ArtW("F03_Art_B", SharedDir + "/art_order.png", -16.2f, 1.65f, -8f, 3, 0.55f, 0.55f);
ArtW("F03_Art_C", SharedDir + "/art_order.png", -15.4f, 1.65f, -8f, 3, 0.55f, 0.55f);
OnWall("schedule_board", "Prop_Schedule", 'F', 1.5f, -12.9f, 0.95f, -8f, 3, null);
OnWall("wall_calendar", "Prop_Calendar", 'H', 0.45f, -10.6f, 1.25f, -8f, 3, null);

// entry: coats in a row, binders on the cabinet
Stand("coat_rack", "Prop_CoatRack", 'H', 1.75f, -11.6f, -2.55f, 0f, null, false);
Stand("umbrella_stand", "Prop_Umbrellas", 'H', 0.7f, -11.6f, -3.1f, 0f, null, false);
foreach (var n in new[] { "WallCabinet", "Shelves1", "KitchenTabletop1_03_01", "KitchenTabletop1_03_02", "KitchenTabletop1_02", "Bar_01", "Oven" }) Grade(n, "BEB6A8", 0.2f);
Matte("Refrigerator", "B9B4A8", 0.25f);
Matte("RangeHood", "A8A296", 0.25f);
Matte("WaterDispenser", "B8BCC0", 0.25f);
On(Old("WallCabinet"), "binders_row", "Prop_Binders", 'F', 0.8f, -0.3f, 0f, 180f, null);
On(Old("WallCabinet"), "smart_speaker", "Prop_Speaker", 'H', 0.16f, 0.5f, 0f, 0f, null);
On(Old("KitchenTabletop1_03_02"), "kitchen_utensils", "Prop_Utensils", 'H', 0.34f, -0.4f, 0f, 0f, null);
On(Old("KitchenTabletop1_02"), "coffee_maker", "Prop_Coffee", 'H', 0.36f, 0.25f, 0f, 180f, null);
OnWall("home_panel", "Prop_HomePanel", 'H', 0.22f, -11.55f, 1.25f, -4f, 0, null);

// Ava's room: the one place with a mess
var bed = Back("bed", "Prop_Bed", 'F', 2.05f, -16.1f, -12f, 3, null, true); Retex(bed, "bed", "bed_ava");
var stand = Back("bedside_table", "Prop_BedsideTable", 'F', 0.48f, -15.2f, -12.6f, 3, "D0D4DA", true);
On(stand, "alarm_clock", "Prop_Alarm", 'H', 0.14f, -0.08f, 0f, 180f, null);
On(stand, "book_stack", "Prop_BedBooks", 'F', 0.2f, 0.06f, 0.05f, 25f, null);
var desk = Back("desk", "Prop_Desk", 'F', 1.35f, -17.25f, -9.5f, 2, "D0D4DA", true);
Lit(On(desk, "desk_lamp", "Prop_DeskLamp", 'H', 0.42f, -0.55f, 0.1f, 120f, null), "desk_lamp", "FFD9A8", 1.2f);
On(desk, "book_stack", "Prop_Books", 'F', 0.24f, 0.5f, 0.1f, 10f, null);
On(desk, "pencil_cup", "Prop_PencilCup", 'H', 0.14f, 0.52f, -0.28f, 0f, null);
On(desk, "headphones_stand", "Prop_Headphones", 'H', 0.24f, 0.55f, -0.18f, -30f, null);
Stand("chair", "Prop_DeskChair", 'H', 0.85f, -17.25f, -10.35f, 0f, "C4CAD2", true);
var shelf = Back("bookshelf", "Prop_Bookshelf", 'F', 1.2f, -15.7f, -9.4f, 2, "C6CBD0", true);
On(shelf, "books_row", "Prop_ShelfBooks", 'F', 0.5f, -0.25f, 0f, 180f, null);
On(shelf, "globe", "Prop_Globe", 'H', 0.3f, 0.35f, 0f, 0f, null);
Back("wardrobe", "Prop_Wardrobe_1", 'H', 2.1f, -14.2f, -12.7f, 3, "C4CAD2", true);
Back("wardrobe", "Prop_Wardrobe_2", 'H', 2.1f, -12.95f, -12.7f, 3, "C4CAD2", true);
var dresser = Back("dresser", "Prop_Dresser", 'F', 1.05f, -12.15f, -10.95f, 0, "D0D4DA", true);
On(dresser, "photo_frame", "Prop_DresserPhoto", 'H', 0.2f, 0f, -0.2f, -80f, null);
On(dresser, "small_plant", "Prop_DresserPlant", 'H', 0.28f, 0f, 0.3f, 0f, null);
Stand("laundry_basket", "Prop_Laundry", 'H', 0.5f, -12.3f, -11.95f, 0f, null, false);
Stand("guitar_stand", "Prop_Guitar", 'H', 1.05f, -14.9f, -11.7f, 150f, null, false);
Stand("backpack", "Prop_Backpack", 'H', 0.42f, -16.25f, -10.4f, 200f, "C4CAD2", false);
Stand("slippers", "Prop_Slippers", 'F', 0.28f, -15.05f, -11.1f, 40f, null, false);
ArtW("F03_Poster_1", SharedDir + "/poster_band.png", -15.7f, 1.62f, -10f, 2, 0.5f, 0.75f);
ArtW("F03_Poster_2", SharedDir + "/poster_space.png", -17.7f, 1.78f, -10f, 2, 0.45f, 0.68f);
ArtW("F03_Poster_3", SharedDir + "/poster_film.png", -12.6f, 1.62f, -11.4f, 0, 0.5f, 0.75f);
Rug("F03_Rug_Ava", SharedDir + "/rug_grey.png", -15.6f, -10.9f, 2.5f, 1.8f, 0f);
Blinds("F03_Blinds_Ava", -18.2f, -12.8f, -9.2f, 0.9f, 2.48f, -1f, "A8AEB4");
Blinds("F03_Blinds_Main", -18.3f, -5f, -3.1f, 0.9f, 2.48f, -1f, "B8AE9E");
Blinds("F03_Blinds_Door", -18.3f, -9f, -5.9f, 0.9f, 2.48f, -1f, "B8AE9E");

// order and tension: hard pools from a rigid row of downlights, cold street light cut into stripes by the blinds, blue in the gaps
Sp("Light_BlindStripes", true, V(-23f, 3.2f, -5.5f), V(-13f, 0.6f, -5f), "A8B8E8", 34f, 16f, 50f, 36f);
Sp("Light_Down_1", false, V(-16.4f, 2.85f, -5.5f), V(-16.4f, 0f, -5.5f), "FFD9B0", 16f, 6f, 76f, 42f);
Sp("Light_Down_2", true, V(-13.4f, 2.85f, -5.5f), V(-13.4f, 0f, -5.5f), "FFD9B0", 16f, 6f, 76f, 42f);
Sp("Light_Down_3", false, V(-10.9f, 2.85f, -5.5f), V(-10.9f, 0f, -5.5f), "FFD9B0", 16f, 6f, 76f, 42f);
Sp("Light_Down_4", false, V(-16.4f, 2.85f, -8.2f), V(-16.4f, 0f, -8.2f), "FFD9B0", 16f, 6f, 76f, 42f);
Sp("Light_Down_5", true, V(-16.4f, 2.85f, -3f), V(-16.4f, 0f, -3f), "FFD9B0", 14f, 6f, 76f, 42f);
Rect("Bake_MainWindow", V(-18.2f, 1.7f, -4.05f), 1.9f, 1.5f, 90f, "7F97D6", 0.9f, 9f);
Rect("Bake_MainDoor", V(-18.2f, 1.25f, -7.45f), 3f, 2.4f, 90f, "7F97D6", 0.9f, 9f);
Pt("Bake_MainFill_A", V(-14.5f, 1.7f, -5.5f), "8FA6D8", 0.55f, 10f);
Pt("Bake_EntryFill", V(-10.8f, 1.7f, -3.2f), "8FA6D8", 0.5f, 6f);
// Ava's room: one desk lamp, cold stripes, blue everywhere else
Sp("Light_AvaBlinds", true, V(-23f, 3f, -11f), V(-14f, 0.6f, -11f), "A8B8E8", 16f, 14f, 50f, 36f);
Sp("Light_AvaDesk", true, V(-17.8f, 1.2f, -9.4f), V(-17.2f, 0.75f, -9.8f), "FFD9A8", 0.9f, 4f, 120f, 50f);
Pt("Bake_AvaDeskGlow", V(-17.7f, 1.25f, -9.6f), "FFCF9C", 1.2f, 4f);
Pt("Bake_AvaFill", V(-15f, 1.7f, -11f), "A8B4C8", 0.6f, 7f);
Pt("Bake_AvaBedLamp", V(-15.2f, 0.9f, -12.5f), "FFC890", 0.5f, 3f);
Rect("Bake_AvaWindow", V(-18.1f, 1.25f, -11f), 3.5f, 2.4f, 90f, "8FA6D8", 0.5f, 9f);
Probe("Probe_F03Main", V(-14f, 1.4f, -5.5f), V(8.7f, 3f, 6.8f), 4);
Probe("Probe_F03Ava", V(-15f, 1.4f, -11f), V(6.3f, 3f, 3.8f), 4);
ProbeGrid("Probes_F03Main", -17.9f, -10.1f, -8.6f, -2.5f, 1.8f, new[] { 0.5f, 1.5f, 2.5f });
ProbeGrid("Probes_F03Ava", -17.8f, -12.2f, -12.6f, -9.5f, 1.6f, new[] { 0.5f, 1.5f, 2.5f });
LightsOff(new[] { "Lobby/Spot Light (9)/Point Light (2)", "Lobby/Spot Light (9)/Point Light (3)" });
