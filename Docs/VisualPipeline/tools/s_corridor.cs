// 17F corridor. Time: 18:10 to 19:00. Quiet hotel feel: dark sage walls, deep carpet, warm sconces with dark gaps between them, long perspective toward each door.
Room("Floor17", "VisualPass_Corridor", 111.68f);
Copy("Mesh19", "Cor_WallsA", Mat("Cor_Wall", "6C7F80", 0.1f, null), 0, 0.8f, 2f, 0f);
Copy("Mesh53", "Cor_WallsB", Mat("Cor_Wainscot", "566866", 0.14f, null), 0, 0.8f, 2f, 0f);
Copy("Mesh40", "Cor_Floor", Mat("Cor_Carpet", "3A3F4A", 0.06f, null), 1, 0.7f, 2f, 0f);
Copy("Mesh40", "Cor_Ceiling", Mat("Cor_Ceiling", "A9A99E", 0.05f, null), 2, 0.6f, 2f, 2.895f);

// one sconce beside every apartment door
System.Action<string, float, float, float> Sconce = (name, u, w, yaw) => {
    Lit(Put("wall_sconce", "Prop_Sconce_" + name, 'H', 0.3f, u, 1.65f, w, yaw, false, 0.3f, null), "wall_sconce", "FFC48A", 1.5f);
    var d = UnityEngine.Quaternion.Euler(0, yaw, 0) * UnityEngine.Vector3.forward;
    Pt("Bake_Sconce_" + name, V(u, 1.85f, w) + d * 0.25f, "FFC48A", 1.6f, 5f);
};
Sconce("F01", -6.72f, 0.9f, -90f);
Sconce("F02", -6.62f, -17f, -90f);
Sconce("F03", -9.52f, -1.6f, 90f);
Sconce("F04", -2f, -24.72f, 0f);
Put("door_mat", "Prop_Mat_F01", 'F', 1f, -7.05f, 0f, 2.1f, 90f, false, 0.3f, null);
Put("door_mat", "Prop_Mat_F02", 'F', 1f, -6.95f, 0f, -15.7f, 90f, false, 0.3f, null);
Put("door_mat", "Prop_Mat_F03", 'F', 1f, -9.25f, 0f, -2.9f, 90f, false, 0.3f, null);
Put("door_mat", "Prop_Mat_F04", 'F', 1f, -3.2f, 0f, -24.3f, 0f, false, 0.3f, null);

// ceiling lights down the corridor, spaced so dark gaps stay between them
var panel = Glow("Cor_Panel", "FFE2BE", 1.3f);
for (int i = 0; i < 6; i++) {
    float w = 8f - i * 6f;
    var b = Box("Cor_Panel_" + i, shellT, panel, V(-8.1f, 2.885f, w), V(0.5f, 0.02f, 0.5f), 0f, 0f);
    b.GetComponent<UnityEngine.MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    Sp("Bake_Corridor_" + i, false, V(-8.1f, 2.84f, w), V(-8.1f, 0f, w), "FFD9B0", 6f, 5f, 100f, 50f);
    Pt("Bake_CorridorFill_" + i, V(-8.1f, 1.7f, w), "A8B4C8", 0.7f, 7f);
}
Put("snake_plant", "Prop_Plant_Lift", 'H', 1f, -7.1f, 0f, 9f, 0f, false, 0.4f, null);
Put("fire_cabinet", "Prop_FireCabinet", 'H', 0.6f, -9.56f, 1.1f, -9f, 90f, false, 0.3f, null);
Put("umbrella_stand", "Prop_Umbrella_F03", 'H', 0.7f, -9.3f, 0f, -1.1f, 0f, false, 0.3f, null);
LightsOff(new[] { "DuskLighting/Spot Light" });
ProbeGrid("Probes_Corridor", -9.2f, -7f, -24f, 10f, 2.2f, new[] { 0.5f, 1.5f, 2.5f });
