// 17F03 Mark, Laura and Ava. Time: 19:00, lights on. Order and tension: khaki walls, dark floor, hard pools of light, a rigid row of downlights. Ava's room is cooler and apart.
const string Planks = SharedDir + "/T_Planks.png";
Room("Floor17/Apartment_17F03", "VisualPass_17F03", 111.68f);
Copy("Mesh16", "F03_Main_Walls", Mat("F03_MainWall", "A08C6E", 0.08f, null), 0, 1f, 2f, 0f);
Copy("Mesh43", "F03_Main_Floor", Mat("F03_MainFloor", "8A6A4C", 0.34f, Planks), 1, 1f, 2f, 0f);
Copy("Mesh43", "F03_Main_Ceiling", Mat("F03_Ceiling", "C9BEA9", 0.05f, null), 2, 0.7f, 2f, 2.9f);
Copy("Mesh9", "F03_Ava_Walls", Mat("F03_AvaWall", "77808C", 0.08f, null), 0, 1f, 2f, 0f);
Copy("Mesh31", "F03_Ava_Floor", Mat("F03_AvaFloor", "8C7A64", 0.3f, Planks), 1, 1f, 2f, 0f);
Copy("Mesh31", "F03_Ava_Ceiling", Mat("F03_AvaCeiling", "BDB8BF", 0.05f, null), 2, 0.7f, 2f, 2.9f);
Copy("Mesh6", "F03_Skirting", Mat("F03_Trim", "3F3227", 0.2f, null), 0, 1.2f, 2f, 0f);

Hide(new[] { "SofaChairSet9_Sofa", "TvUnitSet_Table2", "Carpet_05_01", "Carpet_05_02", "Carpet_04", "Lamp_03", "Chair", "HomeLamp_01", "TeddyBear_02", "DeskLight", "BookPile7", "PenHolder" });

// living and dining: everything square to the walls
Put("sofa_modern", "Prop_Sofa", 'F', 2.6f, -16.2f, 0f, -5.7f, 0f, true, 1f, "B8AE9E");
Put("armchair", "Prop_Armchair_1", 'F', 0.85f, -17.9f, 0f, -4f, 90f, true, 0.6f, "B8AE9E");
Put("armchair", "Prop_Armchair_2", 'F', 0.85f, -14.3f, 0f, -4f, -90f, true, 0.6f, "B8AE9E");
var tv = Swap("TelevisionFurniture", "tv_console", 'F', 2.8f, Auto, 0f, "A89888");
Put("coffee_table", "Prop_CoffeeTable", 'F', 1.2f, -16.2f, 0f, -4.2f, 0f, true, 0.6f, "8A7C6C");
Rug("F03_Rug", SharedDir + "/rug_navy.png", -16.2f, -4.2f, 3.1f, 2.3f, 0f);
Swap("DiningTable", "dining_table", 'F', 1.7f, 90f, 0f, "A08C78");
Put("dining_chair", "Prop_DiningChair_1", 'H', 0.85f, -17.1f, 0f, -8.2f, 90f, true, 0.5f, "B8AE9E");
Put("dining_chair", "Prop_DiningChair_2", 'H', 0.85f, -15.3f, 0f, -8.2f, -90f, true, 0.5f, "B8AE9E");
Put("wall_clock", "Prop_WallClock", 'F', 0.42f, -16.3f, 1.95f, -2.14f, 180f, false, 0.3f, null);
Put("schedule_board", "Prop_Schedule", 'F', 1.5f, -12.4f, 0.95f, -8.93f, 0f, false, 0.5f, null);
Art("F03_Art", SharedDir + "/art_grid.png", -11f, 1.6f, -8.97f, 0.8f, 0.8f, 0f);
Art("F03_Art_B", SharedDir + "/art_grid.png", -17f, 1.55f, -8.97f, 0.5f, 0.5f, 0f);
Art("F03_Art_C", SharedDir + "/art_grid.png", -16.2f, 1.55f, -8.97f, 0.5f, 0.5f, 0f);
Art("F03_Art_D", SharedDir + "/art_grid.png", -15.4f, 1.55f, -8.97f, 0.5f, 0.5f, 0f);
Put("storage_boxes", "Prop_Boxes", 'H', 1f, -13.6f, 0f, -2.55f, 0f, false, 0.4f, null);
Put("coat_rack", "Prop_CoatRack", 'H', 1.75f, -10.2f, 0f, -4f, 0f, false, 0.5f, null);
Put("umbrella_stand", "Prop_Umbrellas", 'H', 0.7f, -10.2f, 0f, -3.45f, 0f, false, 0.3f, null);
foreach (var n in new[] { "WallCabinet", "Shelves1", "KitchenTabletop1_03_01", "KitchenTabletop1_03_02", "KitchenTabletop1_02", "Bar_01", "Oven" }) Grade(n, "BEB6A8", 0.2f);
Matte("Refrigerator", "B9B4A8", 0.25f);
Matte("RangeHood", "A8A296", 0.25f);
Matte("WaterDispenser", "B8BCC0", 0.25f);

// Ava's room
var bed = Swap("Bed1_01", "bed", 'F', 2.1f, Auto, 0f, null); Retex(bed, "bed", "bed_ava");
var desk = Swap("Desk2", "desk", 'F', 1.45f, Auto, 0f, "D0D4DA");
float dt = TopOf(desk);
Lit(Put("desk_lamp", "Prop_DeskLamp", 'H', 0.42f, -17.85f, dt, -9.45f, 120f, false, 0.3f, null), "desk_lamp", "FFD9A8", 1.2f);
Put("book_stack", "Prop_Books", 'F', 0.24f, -16.75f, dt, -9.45f, 10f, false, 0.3f, null);
Put("pencil_cup", "Prop_PencilCup", 'H', 0.14f, -16.95f, dt, -9.75f, 0f, false, 0.3f, null);
Put("chair", "Prop_DeskChair", 'H', 0.85f, -17.3f, 0f, -10.3f, 0f, true, 0.5f, "C4CAD2");
Swap("Dresser_01", "dresser", 'F', 1.15f, Auto, 0f, "D0D4DA");
Swap("Dresser_02", "bookshelf", 'H', 1.6f, Auto, 0f, "C6CBD0");
Swap("WardRobe_01", "wardrobe", 'F', 1.55f, Auto, 0f, "C4CAD2");
Swap("WardRobe_02", "wardrobe", 'F', 1.55f, Auto, 0f, "C4CAD2");
Swap("Clock", "wall_clock", 'F', 0.36f, 180f, 0f, null);
Put("backpack", "Prop_Backpack", 'H', 0.42f, -14.9f, 0f, -11f, 90f, false, 0.4f, "C4CAD2");
Put("laundry_basket", "Prop_Laundry", 'H', 0.5f, -12.3f, 0f, -11.7f, 0f, false, 0.4f, null);
Rug("F03_Rug_Ava", SharedDir + "/rug_grey.png", -16.2f, -11.1f, 2.5f, 1.8f, 0f);
Hide(new[] { "CurtainSet_3" });
Blinds("F03_Blinds_Ava", -18.2f, -12.8f, -9.2f, 0.9f, 2.48f, -1f, "A8AEB4");
Blinds("F03_Blinds_Main", -18.3f, -5f, -3.1f, 0.9f, 2.48f, -1f, "B8AE9E");
Blinds("F03_Blinds_Door", -18.3f, -9f, -5.9f, 0.9f, 2.48f, -1f, "B8AE9E");

// a rigid row of downlights: hard pools, dark gaps between them
LightsOff(new[] { "Lobby/Spot Light (9)/Point Light (2)", "Lobby/Spot Light (9)/Point Light (3)" });

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
Sp("Light_AvaBlinds", true, V(-23f, 3f, -11f), V(-14f, 0.6f, -11f), "8FA6D8", 20f, 14f, 50f, 36f);
Sp("Light_AvaDesk", true, V(-17.7f, 1.2f, -9.5f), V(-17.2f, 0.75f, -9.8f), "FFD9A8", 0.9f, 4f, 120f, 50f);
Pt("Bake_AvaDeskGlow", V(-17.6f, 1.25f, -9.6f), "FFCF9C", 0.8f, 3f);
Pt("Bake_AvaFill", V(-15f, 1.7f, -11f), "8FA6D8", 0.7f, 7f);
Rect("Bake_AvaWindow", V(-18.1f, 1.25f, -11f), 3.5f, 2.4f, 90f, "7F97D6", 0.7f, 9f);
Probe("Probe_F03Main", V(-14f, 1.4f, -5.5f), V(8.7f, 3f, 6.8f), 4);
Probe("Probe_F03Ava", V(-15f, 1.4f, -11f), V(6.3f, 3f, 3.8f), 4);
ProbeGrid("Probes_F03Main", -17.9f, -10.1f, -8.6f, -2.5f, 1.8f, new[] { 0.5f, 1.5f, 2.5f });
ProbeGrid("Probes_F03Ava", -17.8f, -12.2f, -12.6f, -9.5f, 1.6f, new[] { 0.5f, 1.5f, 2.5f });
