// 1F lobby. Time: about 18:00, warm amber. The system should feel everyday and trustworthy: walnut, warm plaster, terrazzo, pools of amber light, blue dusk beyond the glass.
const string Terrazzo = SharedDir + "/T_Terrazzo.png";
Room("Lobby", "VisualPass_Lobby", 0.2f);
var plaster = Mat("Lobby_Plaster", "A9A7A2", 0.1f, null);
var walnut = Mat("Lobby_Walnut", "6B5546", 0.25f, null);
var cream = Mat("Lobby_Cream", "BDB9B0", 0.12f, null);
var ceilingM = Mat("Lobby_Ceiling", "C8C6C0", 0.08f, null);
var floorM = Mat("Lobby_Terrazzo", "BEBAB2", 0.55f, Terrazzo);
CopySlots("Mesh2428", "Lobby_ShellA", new[] { plaster, ceilingM, walnut }, 0.35f);
CopySlots("Mesh2437", "Lobby_ShellB", new[] { walnut, walnut, cream }, 0.35f);
CopySlots("Mesh2491", "Lobby_Floor", new[] { floorM }, 0.5f);
CopySlots("Mesh2509", "Lobby_FloorInlay", new[] { walnut, floorM }, 0.5f);
CopySlots("Mesh2436", "Lobby_DeckA", new[] { ceilingM, null }, 0.4f);
CopySlots("Mesh2435", "Lobby_DeckB", new[] { ceilingM, null }, 0.4f);
CopySlots("Mesh2508", "Lobby_RingCeiling", new[] { ceilingM }, 0.3f);
CopySlots("Mesh2495", "Lobby_ReceptionBody", new[] { walnut }, 0.6f);
CopySlots("Mesh2498", "Lobby_ReceptionTopA", new[] { cream }, 0.6f);
CopySlots("Mesh2499", "Lobby_ReceptionTopB", new[] { cream }, 0.6f);
CopySlots("Mesh2494", "Lobby_ReceptionBase", new[] { walnut }, 0.6f);
CopySlots("Mesh2487", "Lobby_LiftFrameA", new[] { walnut, cream }, 0.5f);
CopySlots("Mesh2503", "Lobby_LiftFrameB", new[] { walnut, cream }, 0.5f);

// keeps the sky out of the bake from above; never seen from inside
var roof = Box("Lobby_RoofOccluder", shellT, ceilingM, V(1f, 18.6f, 12f), V(52f, 0.1f, 44f), 0f, 0f);
roof.GetComponent<UnityEngine.MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

// seating: the curved sofas stay, warmed and matted
foreach (var n in new[] { "LoungeRoomSofa_01", "LoungeRoomSofa_02", "LoungeRoomSofa_03", "SofaChairSet9_Sofa_01", "SofaChairSet9_Sofa_02" }) Grade(n, "D8D4CC", 0.15f);
foreach (var n in new[] { "KitchenTable_02_01", "KitchenTable_02_02", "KitchenTable_02_03" }) Grade(n, "9A7A5C", 0.2f);
Matte("Carpet_01", "8C5A3C", 0.05f);
Matte("Carpet_02", "8C5A3C", 0.05f);
Swap("ChristmasTree_01", "lobby_planter", 'H', 3.4f, 0f, 0f, null);

// pools of amber light over each seating group
var poolZones = new[] { new[] { 1.9f, 0.6f }, new[] { 17.5f, 20.9f }, new[] { 2.75f, 23.2f }, new[] { 16.3f, 1.2f } };
for (int i = 0; i < poolZones.Length; i++) {
    float u = poolZones[i][0], w = poolZones[i][1];
    Lit(Put("lobby_pendant", "Prop_Pendant_" + i, 'L', 2.6f, u, 4.2f, w, 0f, false, 0.4f, null), "lobby_pendant", "FFD2A0", 0.8f);
    Sp("Bake_Pool_" + i, false, V(u, 4.1f, w), V(u, 0f, w), "FFC48A", 22f, 10f, 100f, 55f);
}
for (int i = 0; i < 3; i++) {
    float w = 8.3f + i * 3.65f;
    Hang("Prop_CafeLamp_" + i, 19.7f, 3.4f, w, 0.6f, 11.3f, "FFC48A", 1f);
    Sp("Bake_Cafe_" + i, false, V(19.7f, 3.3f, w), V(19.7f, 0f, w), "FFC48A", 10f, 6f, 95f, 50f);
}
// the desk stays warm; the lift wall gets a soft cool wash

// plants and street furniture along the glass
Put("lobby_planter", "Prop_Planter_1", 'H', 3f, 22f, 0f, 6f, 0f, true, 0.5f, null);
Put("lobby_planter", "Prop_Planter_2", 'H', 3f, 22f, 0f, 19f, 0f, true, 0.5f, null);
Put("lobby_planter", "Prop_Planter_3", 'H', 3f, -4f, 0f, 27f, 0f, true, 0.5f, null);
Put("lobby_bench", "Prop_Bench_1", 'F', 2.4f, 22.4f, 0f, 12f, -90f, true, 0.6f, null);
Put("lobby_bench", "Prop_Bench_2", 'F', 2.4f, 11f, 0f, 26f, 180f, true, 0.6f, null);
Put("info_stand", "Prop_Info_1", 'H', 1.5f, 4f, 0f, 12f, 90f, false, 0.4f, null);
Put("trash_bin", "Prop_Bin_1", 'H', 0.9f, 5.8f, 0f, 20f, 0f, false, 0.4f, null);
Put("trash_bin", "Prop_Bin_2", 'H', 0.9f, 21.8f, 0f, 3.5f, 0f, false, 0.4f, null);

LightsOff(new[] { "Lobby/Spot Light (9)/Point Light (8)", "Lobby/Spot Light (9)/Point Light (9)", "Lobby/Spot Light (9)/Point Light (10)" });

// dusk-blue atrium with warm islands: the high lights turn near neutral and stay live so people are lit, warmth only at the pendants and the desk
foreach (var l in apt.GetComponentsInChildren<UnityEngine.Light>(true)) {
    if (!l.transform.parent.name.StartsWith("LobbyLighting")) continue;
    l.color = Hex("F0E4D4"); l.intensity = 34f; l.lightmapBakeType = UnityEngine.LightmapBakeType.Mixed; l.shadows = UnityEngine.LightShadows.None;
}
Pt("Bake_Reception_A", V(8.5f, 3.2f, 10f), "FFC890", 3.5f, 7f);
Pt("Bake_Reception_B", V(8.5f, 3.2f, 15f), "FFC890", 3.5f, 7f);
Sp("Bake_TerminalWash", false, V(-1f, 5f, 6f), V(-1f, 1.5f, 9.5f), "FFD2A0", 9f, 8f, 70f, 40f);
Pt("Bake_LiftWall_A", V(-9f, 4.5f, 8f), "C8C4C0", 1.4f, 10f);
Pt("Bake_LiftWall_B", V(-1f, 4.5f, 8.5f), "C8C4C0", 1.4f, 10f);
Rect("Bake_GlassEast", V(24f, 8f, 12f), 34f, 14f, -90f, "8FA6D8", 0.55f, 34f);
Rect("Bake_GlassNorth", V(3f, 8f, 31f), 34f, 14f, 180f, "8FA6D8", 0.55f, 34f);
Sp("Bake_ScreenGlow", false, V(11f, 6.5f, -5.5f), V(11f, 0f, 3f), "F24CBF", 9f, 15f, 90f, 50f);
Probe("Probe_Lobby", V(1f, 3f, 12f), V(48f, 7f, 40f), 3);
ProbeGrid("Probes_Lobby", -20f, 24f, -6f, 30f, 4f, new[] { 0.6f, 2f, 5f });
