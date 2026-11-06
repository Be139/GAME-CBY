// 17F02 Claire and Ben. Time: evening into night. Tidy, distant, cold neutral; the only warmth is her lamp and the dinner table. Two of everything, kept apart. Laid out from the plan.
const string Planks = SharedDir + "/T_Planks.png";
Room("Floor17/Apartment_17F02", "VisualPass_17F02", 111.68f);
Copy("Mesh10", "F02_Bed_Walls", Mat("F02_BedWall", "727E88", 0.08f, null), 0, 1f, 2f, 0f);
Copy("Mesh29", "F02_Bed_Floor", Mat("F02_BedFloor", "9A9690", 0.3f, Planks), 1, 1f, 2f, 0f);
Copy("Mesh29", "F02_Bed_Ceiling", Mat("F02_Ceiling", "B8BABC", 0.05f, null), 2, 0.7f, 2f, 2.9f);
Copy("Mesh58", "F02_Main_Walls", Mat("F02_MainWall", "8C8F90", 0.08f, null), 0, 1f, 2f, 0f);
Copy("Mesh30", "F02_Main_Floor", Mat("F02_MainFloor", "A89C8C", 0.3f, Planks), 1, 1f, 2f, 0f);
Copy("Mesh30", "F02_Main_Ceiling", Mat("F02_Ceiling", "B8BABC", 0.05f, null), 2, 0.7f, 2f, 2.9f);
Copy("Mesh11", "F02_Skirting", Mat("F02_Trim", "C9CACA", 0.2f, null), 0, 1.2f, 2f, 0f);

Hide(new[] { "ToyTrain_04", "Table_15", "TeddyBear_03", "ShapeSortingBlock_SixCornerStar", "MathToy", "BuildingBlockToy_04", "BuildingBlockToy_06", "Toy_01", "ToyXylophone", "Ball_01", "TeddyBear_02", "TeddyBear_01", "WallDecor_01", "Carpet_04", "Carpet_01", "Carpet_02", "Painting_13", "TelevisionFurniture", "Houseplant_04", "WoodenShelf", "CurtainSet_20", "Bed_09", "Nightstand_01", "Lamp", "Shelve_13", "Library", "Desk_02", "Chair_03", "RoomFurniture_08_01", "RoomFurniture_08_02", "KitchenChair_01_01", "KitchenChair_01_02", "KitchenChair_01_03", "KitchenChair_01_04", "Sofa_02_G", "Sofa_02_A", "Table_17", "SofaChairSet6_Table", "HomeLamp_02", "Houseplant_03", "Flower_04" });

// bedroom: wide bed, her side lit, his side dark
var bed = Back("double_bed", "Prop_Bed", 'F', 2.2f, -0.5f, -6.6f, 2, "D8DCE0", true);
Stand("bed_bench", "Prop_BedBench", 'F', 1.3f, -0.5f, -8.05f, 0f, "B8C8C8", true);
var herStand = Back("bedside_table", "Prop_HerTable", 'F', 0.5f, 0.75f, -5.85f, 2, "C8D0D8", true);
Lit(On(herStand, "table_lamp", "Prop_HerLamp", 'H', 0.42f, 0f, 0f, 0f, null), "table_lamp", "FFD2A0", 1.6f);
var hisStand = Back("bedside_table", "Prop_HisTable", 'F', 0.5f, -1.75f, -5.85f, 2, "C8D0D8", true);
On(hisStand, "book_stack", "Prop_HisBooks", 'F', 0.22f, 0f, 0f, 15f, null);
var dresser = Back("dresser", "Prop_Dresser", 'F', 1.05f, -3f, -5.9f, 2, "C8D0D8", true);
On(dresser, "photo_frame", "Prop_DresserPhoto", 'H', 0.2f, -0.3f, 0f, 165f, null);
On(dresser, "vase_stems", "Prop_DresserVase", 'H', 0.42f, 0.32f, 0f, 0f, "C8D0D8");
OnWall("wall_mirror", "Prop_Mirror", 'H', 1f, -3f, 1.2f, -6.2f, 2, null);
Back("wardrobe", "Prop_Wardrobe_1", 'H', 2f, -4f, -6.1f, 1, "D0D4D6", true);
Back("wardrobe", "Prop_Wardrobe_2", 'H', 2f, -4f, -7.32f, 1, "D0D4D6", true);
var desk = Back("desk", "Prop_Desk", 'F', 1.45f, 0.95f, -8.9f, 3, "C8D0D8", true);
Stand("chair", "Prop_DeskChair", 'H', 0.85f, 0.95f, -8.1f, 180f, "C8D0D8", true);
On(desk, "laptop_notebook", "Prop_DeskLaptop", 'F', 0.38f, -0.25f, 0f, 170f, null);
On(desk, "small_plant", "Prop_DeskPlant", 'H', 0.3f, 0.5f, 0.1f, 0f, null);
Back("bookshelf", "Prop_Bookshelf", 'F', 1.1f, -0.5f, -9f, 3, "C6CBD0", true);
Back("laundry_basket", "Prop_Laundry", 'H', 0.48f, 1.65f, -6.55f, 0, null, false);
Rug("F02_Rug_Bed", SharedDir + "/rug_grey.png", -0.5f, -7.3f, 3f, 2.2f, 0f);
ArtW("F02_Art", SharedDir + "/art_cold.png", -0.5f, 1.72f, -6.5f, 2, 0.7f, 1.05f);
ArtW("F02_Art_B", SharedDir + "/art_triptych_a.png", -1.75f, 1.55f, -6.5f, 2, 0.4f, 0.6f);
ArtW("F02_Art_C", SharedDir + "/art_triptych_b.png", 0.75f, 1.55f, -6.5f, 2, 0.4f, 0.6f);
OnWall("home_panel", "Prop_HomePanel", 'H', 0.22f, -3.1f, 1.25f, -7.8f, 3, null);
Blinds("F02_Blinds_Bed", 2f, -9.3f, -5.7f, 0.95f, 2.48f, 1f, "A8AEB4");
Curtain("F02_Curtain_A", 1.86f, -9.05f, 0.5f, 2.5f, -90f, "curtain_slate");
Curtain("F02_Curtain_B", 1.86f, -5.95f, 0.5f, 2.5f, -90f, "curtain_slate");

// dining and kitchen
Matte("Bar_02", "5C4636", 0.25f);
Stand("dining_chair", "Prop_DiningChair_1", 'H', 0.95f, -2.05f, -11.2f, -90f, "D8D8D4", true);
Stand("dining_chair", "Prop_DiningChair_2", 'H', 0.95f, -2.05f, -12f, -90f, "D8D8D4", true);
Stand("dining_chair", "Prop_DiningChair_3", 'H', 0.95f, -3.8f, -11.6f, 90f, "D8D8D4", true);
var bar = Old("Bar_02");
On(bar, "dinner_plate", "Prop_Plate_1", 'F', 0.28f, 0.35f, 0.3f, 0f, null);
On(bar, "dinner_plate", "Prop_Plate_2", 'F', 0.28f, -0.35f, -0.3f, 0f, null);
On(bar, "wine_set", "Prop_Wine", 'F', 0.3f, 0f, 0.65f, 0f, null);
Hang("Prop_Pendant", -2.9f, 2.44f, -11.5f, 0.45f, 2.9f, "FFCB94", 1.2f);
foreach (var n in new[] { "Kitchen", "KitchenDesk", "Cooker", "Microvawe", "Cabinet", "Cabinet (1)", "Desk1" }) Grade(n, "C4C6C4", 0.2f);
On(Old("KitchenDesk"), "coffee_maker", "Prop_Coffee", 'H', 0.36f, 0f, 0f, 90f, null);
On(Old("Cabinet"), "cook_pots", "Prop_Pots", 'F', 0.3f, -0.2f, 0f, 0f, null);
On(Old("Cabinet"), "kitchen_utensils", "Prop_Utensils", 'H', 0.34f, 0.3f, 0f, 0f, null);
On(Old("Cabinet (1)"), "spice_rack", "Prop_Spices", 'F', 0.34f, 0f, 0f, 0f, null);

// desk where Ben reads the log
Stand("chair", "Prop_LogChair", 'H', 0.85f, 0.5f, -9.75f, 180f, "C8D0D8", true);
ArtW("F02_Art_Main", SharedDir + "/art_triptych_a.png", 0.45f, 1.45f, -10.5f, 2, 0.5f, 0.75f);
On(Old("Desk1"), "headphones_stand", "Prop_Headphones", 'H', 0.24f, -0.55f, 0.1f, 20f, null);

// living room
var sofa = Stand("sofa_modern", "Prop_Sofa", 'F', 2.8f, 0.45f, -13.2f, 180f, "D8D8D4", true);
Put("throw_pillows", "Prop_SofaPillows", 'F', 0.5f, 1.45f, 0.42f, -13.15f, 195f, false, 0.3f, "C8D4D8");
Stand("armchair", "Prop_Armchair", 'F', 0.85f, -1.6f, -14.6f, 90f, "C8D4D8", true);
var tv = Back("tv_console", "Prop_TvConsole", 'F', 2.5f, 0.55f, -16f, 3, "D0D0D0", true);
var table = Stand("coffee_table", "Prop_CoffeeTable", 'F', 1.2f, 0.45f, -14.65f, 0f, "C8C8C8", true);
On(table, "magazine_stack", "Prop_Mags", 'F', 0.26f, -0.3f, 0f, 12f, null);
On(table, "mug_pair", "Prop_LivingMugs", 'F', 0.2f, 0.3f, 0f, 0f, null);
Lit(Stand("floor_lamp", "Prop_FloorLamp", 'H', 1.55f, -1.25f, -16f, 0f, null, false), "floor_lamp", "FFC88F", 1.4f);
Stand("snake_plant", "Prop_SnakePlant", 'H', 1f, 2f, -13.1f, 0f, null, false);
Stand("plant", "Prop_Plant", 'H', 1.3f, -2.8f, -16.05f, 0f, null, false);
Rug("F02_Rug_Living", SharedDir + "/rug_grey.png", 0.3f, -14.55f, 3.5f, 2.2f, 0f);
ArtW("F02_Art_Living", SharedDir + "/art_cold.png", -5.2f, 1.6f, -15.2f, 3, 0.7f, 1.05f);
OnWall("home_panel", "Prop_EntryPanel", 'H', 0.22f, -6.1f, 1.25f, -14.5f, 2, null);
Back("shoe_rack", "Prop_ShoeRack", 'F', 0.85f, -4.95f, -15.95f, 3, null, false);
Blinds("F02_Blinds_Main", 2.1f, -15.5f, -13.6f, 0.95f, 2.48f, 1f, "A8AEB4");
SunBlock("F02_SunBlock", V(2.9f, 1.5f, -11f), V(0.1f, 3.2f, 13f));

// bedroom: cold dusk through the blinds in stripes, one small warm lamp on her side only
Sp("Light_BedDusk", true, V(6f, 3f, -7.5f), V(-0.5f, 0.5f, -7f), "8FA6D8", 24f, 14f, 62f, 42f);
Sp("Light_BedLamp", true, V(0.75f, 0.95f, -5.85f), V(-0.4f, 0.2f, -6.8f), "FFD2A0", 1f, 5f, 125f, 40f);
Pt("Bake_BedLampGlow", V(0.75f, 1.05f, -6f), "FFC890", 0.5f, 3.5f);
Rect("Bake_BedWindow", V(1.9f, 1.25f, -7.5f), 3.6f, 2.4f, -90f, "7F97D6", 0.9f, 9f);
Pt("Bake_BedFill", V(-1f, 1.7f, -7.4f), "8FA6D8", 0.7f, 6f);
// main room: the dinner table is the only warm island
Sp("Light_Pendant", true, V(-2.9f, 2.4f, -11.5f), V(-2.9f, 0f, -11.5f), "FFCB94", 5f, 5f, 100f, 55f);
Pt("Bake_PendantGlow", V(-2.9f, 1.6f, -11.5f), "FFC48A", 0.5f, 3.5f);
Pt("Bake_KitchenFill", V(-5f, 1.5f, -11f), "FFE2C0", 0.3f, 3f);
Pt("Bake_LivingLamp", V(-1.25f, 1.45f, -16f), "FFC88F", 0.6f, 3.5f);
Rect("Bake_LivingDoor", V(2f, 1.25f, -11.05f), 3f, 2.4f, -90f, "7F97D6", 1f, 9f);
Rect("Bake_LivingWindow", V(2f, 1.7f, -14.55f), 1.9f, 1.5f, -90f, "7F97D6", 1f, 9f);
Pt("Bake_MainFill", V(-2f, 1.7f, -13f), "8FA6D8", 0.7f, 9f);
Sp("Light_Living", true, V(-0.5f, 2.7f, -13.5f), V(0f, 0f, -13.5f), "A8B8D8", 1.2f, 7f, 130f, 60f);
Probe("Probe_F02Bed", V(-1f, 1.4f, -7.4f), V(6.4f, 3f, 3.9f), 4);
Probe("Probe_F02Main", V(-2f, 1.4f, -13f), V(8.6f, 3f, 7f), 4);
ProbeGrid("Probes_F02Bed", -4f, 1.7f, -9.1f, -5.8f, 1.6f, new[] { 0.5f, 1.5f, 2.5f });
ProbeGrid("Probes_F02Main", -6f, 1.8f, -16.1f, -9.9f, 1.6f, new[] { 0.5f, 1.5f, 2.5f });
LightsOff(new[] { "Lobby/Spot Light (9)/Point Light (4)", "Lobby/Spot Light (9)/Point Light (5)" });
