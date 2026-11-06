// The lift cabin the player rides in (Scene 1.2, about 18:08), and the lift doors in the lobby. It was an unlit black box. Warm panelled cabin in the same language as the lobby: walnut, brushed metalM, a mirror, a lit button panelM.
UnityEditor.AssetDatabase.Refresh();
Room("Lobby", "VisualPass_Elevator", 0.2f);
float cu0 = -8.37f, cu1 = -5.97f, cv0 = 9.42f, cv1 = 11.67f, ch = 2.55f, uc = (cu0 + cu1) / 2f, vc = (cv0 + cv1) / 2f;
var walnut = Mat("Lift_Walnut", "6B4E3A", 0.3f, null); var metalM = Mat("Lift_Metal", "A8A49C", 0.45f, null); var brass = Mat("Lift_Brass", "B08E5C", 0.4f, null); var floorM = Mat("Lift_Floor", "4A4640", 0.42f, SharedDir + "/T_Terrazzo.png");
var mirror = Mat("Lift_Mirror", "AEBBC4", 0.92f, null); var ceil = Mat("Lift_Ceiling", "3A3632", 0.2f, null); var lightM = Glow("Lift_Light", "FFE6C8", 1.5f);
Box("Lift_FloorPlate", shellT, floorM, V(uc, 0.01f, vc), V(cu1 - cu0, 0.02f, cv1 - cv0), 0f, 1f);
Box("Lift_CeilingPlate", shellT, ceil, V(uc, ch + 0.02f, vc), V(cu1 - cu0, 0.04f, cv1 - cv0), 0f, 1f);
var lampBox = Box("Lift_CeilingLight", shellT, lightM, V(uc, ch - 0.01f, vc), V(1.5f, 0.02f, 1.3f), 0f, 0f); lampBox.GetComponent<UnityEngine.MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
for (int i = 0; i < 3; i++) { var b = Box("Lift_CeilingBar_" + i, shellT, ceil, V(uc - 0.5f + i * 0.5f, ch - 0.03f, vc), V(0.03f, 0.03f, 1.3f), 0f, 0f); b.GetComponent<UnityEngine.MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; }
// side walls: walnut below, brushed metalM above, a brass line between
foreach (var s in new[] { -1f, 1f }) {
    float x = s < 0f ? cu0 + 0.015f : cu1 - 0.015f; var tag = s < 0f ? "L" : "R";
    Box("Lift_Wood" + tag, shellT, walnut, V(x, 0.5f, vc), V(0.03f, 1f, cv1 - cv0), 0f, 1f);
    Box("Lift_Metal" + tag, shellT, metalM, V(x, 1.78f, vc), V(0.03f, 1.54f, cv1 - cv0), 0f, 1f);
    Box("Lift_Line" + tag, shellT, brass, V(x - s * 0.006f, 1.01f, vc), V(0.02f, 0.03f, cv1 - cv0), 0f, 0.4f);
    for (int i = 1; i < 3; i++) Box("Lift_Seam" + tag + i, shellT, ceil, V(x - s * 0.017f, 1.78f, cv0 + i * 0.75f), V(0.004f, 1.54f, 0.006f), 0f, 0f);
    Box("Lift_Rail" + tag, propsT, brass, V(x - s * 0.06f, 0.92f, vc), V(0.035f, 0.035f, cv1 - cv0 - 0.25f), 0f, 0.3f);
    foreach (var b in new[] { 0.4f, 1.85f }) Box("Lift_RailArm" + tag + b, propsT, brass, V(x - s * 0.035f, 0.92f, cv0 + b), V(0.05f, 0.02f, 0.02f), 0f, 0f);
}
// back wall: mirror above walnut, the rail continues
Box("Lift_BackWood", shellT, walnut, V(uc, 0.5f, cv1 - 0.015f), V(cu1 - cu0, 1f, 0.03f), 0f, 1f);
Box("Lift_BackFrame", shellT, brass, V(uc, 1.78f, cv1 - 0.016f), V(cu1 - cu0 - 0.1f, 1.5f, 0.03f), 0f, 0.5f);
Box("Lift_Mirror", shellT, mirror, V(uc, 1.78f, cv1 - 0.034f), V(cu1 - cu0 - 0.2f, 1.42f, 0.004f), 0f, 0.5f);
Box("Lift_BackLine", shellT, brass, V(uc, 1.01f, cv1 - 0.036f), V(cu1 - cu0, 0.03f, 0.02f), 0f, 0.4f);
Box("Lift_BackRail", propsT, brass, V(uc, 0.92f, cv1 - 0.09f), V(cu1 - cu0 - 0.25f, 0.035f, 0.035f), 0f, 0.3f);
// door wall: closed leaves, panels either side, the display above
Box("Lift_DoorPanelL", shellT, metalM, V(cu0 + 0.21f, 1.275f, cv0 + 0.015f), V(0.42f, 2.55f, 0.03f), 0f, 0.8f);
Box("Lift_DoorPanelR", shellT, metalM, V(cu1 - 0.21f, 1.275f, cv0 + 0.015f), V(0.42f, 2.55f, 0.03f), 0f, 0.8f);
Box("Lift_DoorHead", shellT, metalM, V(uc, 2.42f, cv0 + 0.015f), V(1.56f, 0.26f, 0.03f), 0f, 0.8f);
Box("Lift_DoorLeafL", propsT, Mat("Lift_Leaf", "BDBAB2", 0.5f, null), V(uc - 0.39f, 1.145f, cv0 + 0.01f), V(0.77f, 2.29f, 0.02f), 0f, 0.6f);
Box("Lift_DoorLeafR", propsT, Mat("Lift_Leaf", "BDBAB2", 0.5f, null), V(uc + 0.39f, 1.145f, cv0 + 0.01f), V(0.77f, 2.29f, 0.02f), 0f, 0.6f);
Box("Lift_DoorSeam", propsT, Mat("Lift_Seam", "1C1C1E", 0.2f, null), V(uc, 1.145f, cv0 + 0.021f), V(0.01f, 2.29f, 0.003f), 0f, 0f);
var dispFace = Mat("Lift_DisplayFace", "FFFFFF", 0.3f, SharedDir + "/T_LiftDisplay.png"); dispFace.EnableKeyword("_EMISSION"); dispFace.SetTexture("_EmissionMap", Tx(SharedDir + "/T_LiftDisplay.png")); dispFace.SetColor("_EmissionColor", UnityEngine.Color.white * 1.5f); dispFace.globalIlluminationFlags = UnityEngine.MaterialGlobalIlluminationFlags.None;
Box("Lift_Display", propsT, dispFace, V(uc, 2.42f, cv0 + 0.034f), V(0.5f, 0.16f, 0.004f), 180f, 0f);
var panelM = GlowTex("Lift_Panel", SharedDir + "/T_LiftPanel.png", SharedDir + "/T_LiftPanelGlow.png", 1.3f);
Box("Lift_ButtonPanel", propsT, panelM, V(cu1 - 0.21f, 1.28f, cv0 + 0.036f), V(0.27f, 0.78f, 0.012f), 180f, 0f);


Box("Lift_Mat", propsT, Mat("Lift_MatFace", "2E2C2A", 0.1f, null), V(uc, 0.025f, vc), V(1.5f, 0.01f, 1.2f), 0f, 0.8f);

// the two lift doors in the lobby
LiftDoor("Lobby_LiftA", -7.16f, 8.82f, false, -1f, 1.6f);
LiftDoor("Lobby_LiftB", -10.9f, 8.82f, false, -1f, 1.6f);

Pt("Bake_LiftCeiling", V(uc, 2.3f, vc), "FFE6C8", 1.5f, 4.5f);
Pt("Bake_LiftLow", V(uc, 0.8f, vc), "FFD8B0", 0.35f, 3f);
var pr = new UnityEngine.GameObject("Probe_Lift"); pr.transform.SetParent(lightsT, false); pr.transform.localPosition = V(uc, 1.3f, vc);
var rp = pr.AddComponent<UnityEngine.ReflectionProbe>(); rp.mode = UnityEngine.Rendering.ReflectionProbeMode.Baked; rp.size = V(2.4f, 2.5f, 2.25f); rp.boxProjection = true; rp.importance = 6; rp.resolution = 256; UnityEditor.GameObjectUtility.SetStaticEditorFlags(pr, UnityEditor.StaticEditorFlags.ReflectionProbeStatic);
ProbeGrid("Probes_Lift", -8f, -6.2f, 9.8f, 11.4f, 0.8f, new[] { 0.5f, 1.5f, 2.3f });
// the passage to the lifts: warm downlights with blue between them
for (int i = 0; i < 5; i++) { float u = -17f + i * 3.8f; Sp("Bake_LiftPassage_" + i, false, V(u, 3.5f, 6.4f), V(u, 0f, 6.4f), "FFD2A0", 12f, 7f, 80f, 45f); }
ProbeGrid("Probes_LiftPassage", -19f, -2f, 4.6f, 8.2f, 3.4f, new[] { 0.6f, 2f });
