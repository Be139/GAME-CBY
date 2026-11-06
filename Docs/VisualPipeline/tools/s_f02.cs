// 17F02 Claire and Ben. Time: evening into night. Tidy, distant, cold neutral; the only warmth is the lamp and the dinner table.
const string Planks = SharedDir + "/T_Planks.png";
Room("Floor17/Apartment_17F02", "VisualPass_17F02", 111.68f);
Copy("Mesh10", "F02_Bed_Walls", Mat("F02_BedWall", "727E88", 0.08f, null), 0, 1f, 2f, 0f);
Copy("Mesh29", "F02_Bed_Floor", Mat("F02_BedFloor", "9A9690", 0.3f, Planks), 1, 1f, 2f, 0f);
Copy("Mesh29", "F02_Bed_Ceiling", Mat("F02_Ceiling", "B8BABC", 0.05f, null), 2, 0.7f, 2f, 2.9f);
Copy("Mesh58", "F02_Main_Walls", Mat("F02_MainWall", "8C8F90", 0.08f, null), 0, 1f, 2f, 0f);
Copy("Mesh30", "F02_Main_Floor", Mat("F02_MainFloor", "A89C8C", 0.3f, Planks), 1, 1f, 2f, 0f);
Copy("Mesh30", "F02_Main_Ceiling", Mat("F02_Ceiling", "B8BABC", 0.05f, null), 2, 0.7f, 2f, 2.9f);
Copy("Mesh11", "F02_Skirting", Mat("F02_Trim", "C9CACA", 0.2f, null), 0, 1.2f, 2f, 0f);

// no child lives here: the toy clutter from the old furnishing goes
Hide(new[] { "ToyTrain_04", "Table_15", "TeddyBear_03", "ShapeSortingBlock_SixCornerStar", "MathToy", "BuildingBlockToy_04", "BuildingBlockToy_06", "Toy_01", "ToyXylophone", "Ball_01", "TeddyBear_02", "TeddyBear_01", "WallDecor_01", "Carpet_04", "Carpet_01", "Carpet_02", "Painting_13", "TelevisionFurniture", "Houseplant_04", "WoodenShelf" });

// bedroom
Swap("Bed_09", "double_bed", 'F', 2.2f, Auto, 0f, "D8DCE0");
var stand = Swap("Nightstand_01", "bedside_table", 'F', 0.55f, Auto, 0f, "C8D0D8");
var lamp = Put("table_lamp", "Prop_TableLamp", 'H', 0.42f, 0.7f, TopOf(stand), -5.85f, 0f, false, 0.4f, null);
Lit(lamp, "table_lamp", "FFD2A0", 1.6f);
Put("book_stack", "Prop_Books", 'F', 0.22f, 0.86f, TopOf(stand), -6.05f, 15f, false, 0.3f, null);
Lit(Swap("Lamp", "floor_lamp", 'H', 1.6f, 0f, 0f, null), "floor_lamp", "F2D5B5", 0.5f);
Swap("Shelve_13", "dresser", 'F', 1f, Auto, 0f, "C8D0D8");
Swap("Library", "bookshelf", 'H', 2f, Auto, 0f, "C6CBD0");
var desk = Swap("Desk_02", "desk", 'F', 1.5f, Auto, 0f, "C8D0D8");
Swap("Chair_03", "chair", 'H', 0.85f, 180f, 0f, "C8D0D8");
Put("small_plant", "Prop_SmallPlant", 'H', 0.3f, 1.65f, TopOf(desk), -9.15f, 0f, false, 0.3f, null);
Swap("RoomFurniture_08_01", "wardrobe", 'F', 1.2f, Auto, 0f, "D0D4D6");
Swap("RoomFurniture_08_02", "wardrobe", 'F', 1.2f, Auto, 0f, "D0D4D6");
Rug("F02_Rug_Bed", SharedDir + "/rug_grey.png", -0.6f, -7.5f, 2.6f, 2f, 0f);
Art("F02_Art", SharedDir + "/art_cold.png", -0.6f, 1.55f, -5.58f, 0.8f, 1.2f, 180f);
Art("F02_Art_2", SharedDir + "/art_cold.png", -2.65f, 1.6f, -5.58f, 0.5f, 0.75f, 180f);
Put("laundry_basket", "Prop_Laundry", 'H', 0.5f, -3.9f, 0f, -8.7f, 0f, false, 0.4f, null);
Hide(new[] { "CurtainSet_20" });
Blinds("F02_Blinds_Bed", 2f, -9.3f, -5.7f, 0.95f, 2.48f, 1f, "A8AEB4");
Curtain("F02_Curtain_A", 1.86f, -9.05f, 0.5f, 2.5f, -90f, "curtain_slate");
Curtain("F02_Curtain_B", 1.86f, -5.95f, 0.5f, 2.5f, -90f, "curtain_slate");

// dining and kitchen
Matte("Bar_02", "5C4636", 0.25f);
Swap("KitchenChair_01_01", "dining_chair", 'H', 0.95f, -90f, 0f, "D8D8D4");
Swap("KitchenChair_01_02", "dining_chair", 'H', 0.95f, -90f, 0f, "D8D8D4");
Swap("KitchenChair_01_04", "dining_chair", 'H', 0.95f, 90f, 0f, "D8D8D4");
Swap("KitchenChair_01_03", "chair", 'H', 0.85f, 180f, 0f, "C8D0D8");
Hang("Prop_Pendant", -2.9f, 2.44f, -11.5f, 0.45f, 2.9f, "FFCB94", 1.2f);
Put("dinner_plate", "Prop_Plate_1", 'F', 0.28f, -2.55f, 0.91f, -11.2f, 0f, false, 0.3f, null);
Put("dinner_plate", "Prop_Plate_2", 'F', 0.28f, -3.25f, 0.91f, -11.8f, 0f, false, 0.3f, null);
Put("wall_shelf", "Prop_WallShelf", 'F', 1.3f, -0.3f, 1.55f, -9.62f, 180f, false, 0.4f, null);
foreach (var n in new[] { "Kitchen", "KitchenDesk", "Cooker", "Microvawe", "Cabinet", "Cabinet (1)", "Desk1" }) Grade(n, "C4C6C4", 0.2f);

// living room
Swap("Sofa_02_G", "sofa_modern", 'F', 2.8f, 180f, 0f, "D8D8D4");
Swap("Sofa_02_A", "armchair", 'F', 0.85f, 90f, 0f, "C8D4D8");
Swap("Table_17", "tv_console", 'F', 2.6f, Auto, 0f, "D0D0D0");
Swap("SofaChairSet6_Table", "coffee_table", 'F', 1.2f, 0f, 0f, "C8C8C8");
Lit(Swap("HomeLamp_02", "floor_lamp", 'H', 1.55f, 0f, 0f, null), "floor_lamp", "FFC88F", 1.4f);
Hide(new[] { "Houseplant_03" });
Put("snake_plant", "Prop_SnakePlant", 'H', 1f, 1.9f, 0f, -16f, 0f, false, 0.4f, null);
Swap("Flower_04", "plant", 'H', 1.3f, 0f, 0f, null);
Rug("F02_Rug_Living", SharedDir + "/rug_grey.png", -0.2f, -14.5f, 3.6f, 2.2f, 0f);

LightsOff(new[] { "Lobby/Spot Light (9)/Point Light (4)", "Lobby/Spot Light (9)/Point Light (5)" });

// bedroom: cold dusk through the blinds in stripes, one small warm lamp on her side only
Sp("Light_BedDusk", true, V(6f, 3f, -7.5f), V(-0.5f, 0.5f, -7f), "8FA6D8", 24f, 14f, 62f, 42f);
Sp("Light_BedLamp", true, V(0.7f, 0.9f, -5.85f), V(-0.4f, 0.2f, -6.8f), "FFD2A0", 1f, 5f, 125f, 40f);
Pt("Bake_BedLampGlow", V(0.7f, 1.05f, -6f), "FFC890", 0.45f, 3.5f);
Rect("Bake_BedWindow", V(1.9f, 1.25f, -7.5f), 3.6f, 2.4f, -90f, "7F97D6", 0.9f, 9f);
Pt("Bake_BedFill", V(-1f, 1.7f, -7.4f), "8FA6D8", 0.7f, 6f);
// main room: the dinner table is the only warm island
Sp("Light_Pendant", true, V(-2.9f, 2.4f, -11.5f), V(-2.9f, 0f, -11.5f), "FFCB94", 5f, 5f, 100f, 55f);
Pt("Bake_PendantGlow", V(-2.9f, 1.6f, -11.5f), "FFC48A", 0.5f, 3.5f);
Pt("Bake_KitchenFill", V(-5f, 1.5f, -11f), "FFE2C0", 0.3f, 3f);
Pt("Bake_LivingLamp", V(-1f, 1.45f, -16f), "FFC88F", 0.6f, 3.5f);
Rect("Bake_LivingDoor", V(2f, 1.25f, -11.05f), 3f, 2.4f, -90f, "7F97D6", 1f, 9f);
Rect("Bake_LivingWindow", V(2f, 1.7f, -14.55f), 1.9f, 1.5f, -90f, "7F97D6", 1f, 9f);
Pt("Bake_MainFill", V(-2f, 1.7f, -13f), "8FA6D8", 0.7f, 9f);
Sp("Light_Living", true, V(-0.5f, 2.7f, -13.5f), V(0f, 0f, -13.5f), "A8B8D8", 1.2f, 7f, 130f, 60f);
Probe("Probe_F02Bed", V(-1f, 1.4f, -7.4f), V(6.4f, 3f, 3.9f), 4);
Probe("Probe_F02Main", V(-2f, 1.4f, -13f), V(8.6f, 3f, 7f), 4);
ProbeGrid("Probes_F02Bed", -4f, 1.7f, -9.1f, -5.8f, 1.6f, new[] { 0.5f, 1.5f, 2.5f });
ProbeGrid("Probes_F02Main", -6f, 1.8f, -16.1f, -9.9f, 1.6f, new[] { 0.5f, 1.5f, 2.5f });
