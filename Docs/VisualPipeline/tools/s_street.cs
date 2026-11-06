// Street around the lobby, the part seen through the glass. Blue-hour pavement, warm lamp pools, lit trees and everyday street furniture. The rest of the city comes later.
const string Tile = SharedDir + "/T_Tile.png";
Room("OutdoorProps", "VisualPass_Street", 0f);
// short slabs of paving: an object takes 8 lights at most, and one long strip would lose the street lamps to the lobby's lights
var paving = Mat("Street_Paving", "8C8F98", 0.45f, Tile); paving.SetTextureScale("_BaseMap", new UnityEngine.Vector2(4f, 5f));
var pavingS = Mat("Street_PavingS", "8C8F98", 0.45f, Tile); pavingS.SetTextureScale("_BaseMap", new UnityEngine.Vector2(5f, 4f));
var kerb = Mat("Street_Kerb", "B8B8B4", 0.2f, null);
for (int i = 0; i < 7; i++) UnityEditor.GameObjectUtility.SetStaticEditorFlags(Box("Street_PavingE_" + i, shellT, paving, V(23.75f, 0.015f, -5.5f + i * 6f), V(4.5f, 0.03f, 6f), 0f, 0f), 0);
for (int i = 0; i < 8; i++) UnityEditor.GameObjectUtility.SetStaticEditorFlags(Box("Street_PavingS_" + i, shellT, pavingS, V(-19f + i * 6f, 0.015f, -6f), V(6f, 0.03f, 4.6f), 0f, 0f), 0);
var kE = Box("Street_KerbEast", shellT, kerb, V(26.08f, 0.04f, 12f), V(0.16f, 0.08f, 41f), 0f, 0f); UnityEditor.GameObjectUtility.SetStaticEditorFlags(kE, 0);
var kS = Box("Street_KerbSouth", shellT, kerb, V(2f, 0.04f, -8.38f), V(47f, 0.08f, 0.16f), 0f, 0f); UnityEditor.GameObjectUtility.SetStaticEditorFlags(kS, 0);

// no generated prop outside joins the bake: this ground has no lightmap
System.Func<string, string, char, float, float, float, float, float, string, UnityEngine.GameObject> Out = (id, name, fit, size, x, y, z, yaw, tint) => {
    var go = Put(id, name, fit, size, x, y, z, yaw, false, 0f, tint);
    if (go != null) foreach (var r in go.GetComponentsInChildren<UnityEngine.MeshRenderer>(true)) UnityEditor.GameObjectUtility.SetStaticEditorFlags(r.gameObject, 0);
    return go;
};
System.Action<string, float, float, float> Lamp = (name, u, w, yaw) => {
    Out("street_lamp", "Prop_Lamp_" + name, 'H', 5.6f, u, 0f, w, yaw, null);
    var l = Sp("Light_Street_" + name, true, V(u, 5.3f, w), V(u, 0f, w), "FFC48A", 26f, 12f, 125f, 70f); l.shadows = UnityEngine.LightShadows.None;
};
Lamp("E1", 25.7f, -3f, -90f); Lamp("E2", 25.7f, 9f, -90f); Lamp("E3", 25.7f, 21f, -90f); Lamp("E4", 25.7f, 32f, -90f);
Lamp("S1", -18f, -8f, 180f); Lamp("S2", -6f, -8f, 180f); Lamp("S3", 6f, -8f, 180f); Lamp("S4", 18f, -8f, 180f);

// warm light from the lobby falls onto the pavement
System.Action<string, float, float, float, float> Spill = (name, u, w, tu, tw) => {
    var l = Sp("Light_Spill_" + name, true, V(u, 3.2f, w), V(tu, 0f, tw), "FFC890", 9f, 9f, 130f, 70f); l.shadows = UnityEngine.LightShadows.None;
};
Spill("E1", 20.6f, 3f, 25f, 3f); Spill("E2", 20.6f, 15f, 25f, 15f); Spill("S1", 12f, -2.6f, 12f, -7f);

// the trees stop being black shapes
System.Action<string, float, float> TreeLight = (name, u, w) => {
    var l = Sp("Light_Tree_" + name, true, V(u + 0.6f, 0.2f, w), V(u, 7f, w), "FFE0B8", 34f, 12f, 110f, 60f); l.shadows = UnityEngine.LightShadows.None;
};
TreeLight("A", 27.8f, 30f); TreeLight("B", 37.9f, 30f); TreeLight("C", 38.3f, 19f); TreeLight("D", 37.9f, 8.2f); TreeLight("E", 39.4f, -9.4f);

// street furniture
Out("outdoor_bench", "Prop_Bench_1", 'F', 2.2f, 22.7f, 0f, 5f, 90f, null);
Out("outdoor_bench", "Prop_Bench_2", 'F', 2.2f, 22.7f, 0f, 19f, 90f, null);
Out("outdoor_bench", "Prop_Bench_3", 'F', 2.2f, -2f, 0f, -4.4f, 180f, null);
Out("planter_box", "Prop_Planter_1", 'F', 2.8f, 25.2f, 0f, 3.2f, 90f, null);
Out("planter_box", "Prop_Planter_2", 'F', 2.8f, 25.2f, 0f, 15f, 90f, null);
Out("planter_box", "Prop_Planter_3", 'F', 2.8f, 25.2f, 0f, 27f, 90f, null);
Out("planter_box", "Prop_Planter_4", 'F', 2.8f, -12f, 0f, -7.5f, 0f, null);
Out("planter_box", "Prop_Planter_5", 'F', 2.8f, 0f, 0f, -7.5f, 0f, null);
for (int i = 0; i < 6; i++) Out("bollard", "Prop_Bollard_" + i, 'H', 0.85f, 25.75f, 0f, -7.5f + i * 0.9f, 0f, null);
for (int i = 0; i < 5; i++) Out("bollard", "Prop_BollardS_" + i, 'H', 0.85f, 21f + i * 0.9f, 0f, -8.05f, 0f, null);
Out("bus_shelter", "Prop_BusShelter", 'F', 5f, 12f, 0f, -7f, 180f, null);
Out("bike_rack", "Prop_BikeRack", 'F', 2.2f, -8f, 0f, -5.2f, 0f, null);
Lit(Out("public_kiosk", "Prop_Kiosk", 'H', 1.9f, 23.2f, 0f, 12f, 90f, null), "public_kiosk", "BFD8FF", 0.5f);
Out("trash_bin", "Prop_StreetBin_1", 'H', 0.9f, 25.3f, 0f, 9.6f, 0f, null);
Out("trash_bin", "Prop_StreetBin_2", 'H', 0.9f, 7.5f, 0f, -7.6f, 0f, null);
Out("parked_car", "Prop_Car_1", 'F', 4f, 27.7f, 0f, 3f, 86f, null);
Out("parked_car", "Prop_Car_2", 'F', 4f, 28.3f, 0f, 16f, 86f, "E0D4C0");
Out("parked_car", "Prop_Car_3", 'F', 4f, -3f, 0f, -10.2f, 3f, "A8B8A8");
