// 2026-11-06 atmosphere pass. Window light strengths were raised after the first bake: at 1.3 the hall openings did not read.
// Light from outside comes in where rooms only had their own lamps: dusk sky and sign colour through the 17F hall openings, a stronger sky at apartment windows, the billboard's magenta in 17F04.
// The lobby keeps a neutral base and gets amber from its own fixtures. Nothing placed earlier is moved: new lights go into an Atmos_1106 group that is rebuilt on every run.
System.Action<string, string> UseRoot = (aptPath, name) => {
    apt = UnityEngine.GameObject.Find(aptPath).transform; root = apt.Find(name);
    propsT = root.Find("Props"); shellT = root.Find("Shell"); artT = root.Find("WallArt");
    var old = root.Find("Atmos_1106"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
    var g = new UnityEngine.GameObject("Atmos_1106"); g.transform.SetParent(root, false); lightsT = g.transform;
};
System.Action<UnityEngine.Transform, string, float, string> SetL = (scope, name, i, hex) => {
    int n = 0; foreach (var l in scope.GetComponentsInChildren<UnityEngine.Light>(true)) if (l.name == name) { l.intensity = i; if (hex != null) l.color = Hex(hex); n++; }
    if (n == 0) sb.AppendLine("no light " + name);
};
// a second window light on top of an existing one, for a sign's colour mixed into the sky
System.Action<string, string, string, float, float> Twin = (srcName, name, hex, i, k) => {
    UnityEngine.Light src = null; foreach (var l in root.GetComponentsInChildren<UnityEngine.Light>(true)) if (l.name == srcName) src = l;
    if (src == null) { sb.AppendLine("no light " + srcName); return; }
    var g = new UnityEngine.GameObject(name); g.transform.SetParent(lightsT, false);
    g.transform.position = src.transform.position + src.transform.forward * 0.03f; g.transform.rotation = src.transform.rotation;
    var l2 = g.AddComponent<UnityEngine.Light>(); l2.type = src.type; l2.lightmapBakeType = UnityEngine.LightmapBakeType.Baked; l2.color = Hex(hex); l2.intensity = i; l2.range = src.range; l2.areaSize = src.areaSize * k; l2.shadows = UnityEngine.LightShadows.Soft;
};

// 17F public halls: open to the city on the south and west of the south hall, and on the west of the arrival hall
UseRoot("Floor17", "VisualPass_Corridor");
Rect("Bake_HallWin_S", V(-12.5f, 1.45f, -24.6f), 9f, 2.2f, 0f, "7A8BD0", 3.2f, 12f);
Rect("Bake_HallNeon_S", V(-14.5f, 1.6f, -24.55f), 3f, 1.6f, 0f, "C858B8", 1.4f, 10f);
Rect("Bake_HallWin_W", V(-18.75f, 1.45f, -19.5f), 8.5f, 2.2f, 90f, "7A8BD0", 3.2f, 12f);
Rect("Bake_ArrivalWin_W", V(-12.75f, 1.45f, 6.1f), 6f, 2.2f, 90f, "7A8BD0", 2.8f, 12f);
Rect("Bake_ArrivalNeon_W", V(-12.7f, 1.6f, 6.5f), 3f, 1.6f, 90f, "C858B8", 1.3f, 10f);
Probe("Probe_SouthHall", V(-8.2f, 1.5f, -19f), V(21.5f, 3f, 12f), 1);
Probe("Probe_Arrival", V(-9.75f, 1.5f, 6f), V(6.6f, 3f, 11.2f), 1);
Probe("Probe_Corridor", V(-8.1f, 1.5f, -3f), V(2.8f, 3f, 28f), 1);

// the arrival kiosk was a black slab: give it a building-service screen that follows its tilted face
// the screen itself is placed by kiosk_fix.cs, which finds the kiosk's real face wherever it stands; run it after this file
UnityEngine.Transform kiosk = null;
if (kiosk == null) sb.AppendLine("kiosk screen: run kiosk_fix.cs"); else {
    var mr = kiosk.GetComponentInChildren<UnityEngine.MeshRenderer>(); var mfk = mr.GetComponent<UnityEngine.MeshFilter>();
    var col = mr.gameObject.AddComponent<UnityEngine.MeshCollider>(); col.sharedMesh = mfk.sharedMesh; UnityEngine.Physics.SyncTransforms();
    var kb = root.InverseTransformPoint(mr.bounds.center);
    // walk up the front face and keep the longest run of hits that share one plane
    var hitsK = new System.Collections.Generic.List<UnityEngine.RaycastHit>();
    for (float y = 0.4f; y <= 1.8f; y += 0.04f) { UnityEngine.RaycastHit hk; if (col.Raycast(new UnityEngine.Ray(root.TransformPoint(V(kb.x, y, kb.z - 2f)), root.TransformDirection(UnityEngine.Vector3.forward)), out hk, 4f)) hitsK.Add(hk); }
    int bestA = 0, bestB = -1, a0 = 0;
    for (int i = 1; i <= hitsK.Count; i++) { bool br = i == hitsK.Count || UnityEngine.Vector3.Angle(hitsK[i].normal, hitsK[a0].normal) > 8f; if (br) { if (i - 1 - a0 > bestB - bestA) { bestA = a0; bestB = i - 1; } a0 = i; } }
    UnityEngine.RaycastHit h1 = default(UnityEngine.RaycastHit), h2 = default(UnityEngine.RaycastHit); bool ok1 = bestB - bestA >= 3, ok2 = ok1;
    if (ok1) { int span = bestB - bestA; h1 = hitsK[bestA + span / 6]; h2 = hitsK[bestB - span / 6]; }
    sb.AppendLine("kiosk face hits " + hitsK.Count + " run " + (bestB - bestA + 1));
    if (ok1 && ok2) {
        var up = (h2.point - h1.point).normalized; var n = ((h1.normal + h2.normal) * 0.5f).normalized; var c = (h1.point + h2.point) * 0.5f;
        var q = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Quad); UnityEngine.Object.DestroyImmediate(q.GetComponent<UnityEngine.Collider>());
        q.name = "Kiosk_Screen"; q.transform.SetParent(lightsT, true); q.transform.position = c + n * 0.006f; q.transform.rotation = UnityEngine.Quaternion.LookRotation(-n, up);
        float len = (h2.point - h1.point).magnitude; q.transform.localScale = new UnityEngine.Vector3(len * 0.62f, len * 0.95f, 1f);
        var sm = Mat("Kiosk_Screen", "FFFFFF", 0.6f, RoomsDir + "/city/accents/T_KioskScreen.png"); sm.EnableKeyword("_EMISSION"); sm.SetTexture("_EmissionMap", sm.GetTexture("_BaseMap")); sm.SetColor("_EmissionColor", UnityEngine.Color.white * 1.1f); sm.globalIlluminationFlags = UnityEngine.MaterialGlobalIlluminationFlags.None;
        var qr = q.GetComponent<UnityEngine.MeshRenderer>(); qr.sharedMaterial = sm; qr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; UnityEditor.GameObjectUtility.SetStaticEditorFlags(q, 0);
        sb.AppendLine("kiosk screen " + len.ToString("0.00") + " tilt " + UnityEngine.Vector3.Angle(up, UnityEngine.Vector3.up).ToString("0"));
    } else sb.AppendLine("kiosk raycast missed " + ok1 + " " + ok2);
    UnityEngine.Object.DestroyImmediate(col);
}

// lobby: neutral ceiling spots a little lower, amber pools, reception and pendants warmer, more dusk through the glass
UseRoot("Lobby", "VisualPass_Lobby");
var lobbyAll = UnityEngine.GameObject.Find("Lobby").transform;
foreach (var l in lobbyAll.GetComponentsInChildren<UnityEngine.Light>(true)) if (l.name.StartsWith("Spot Light (") && l.transform.parent != null && l.transform.parent.name.StartsWith("LobbyLighting_")) { l.intensity = 27f; l.color = Hex("EDE7DF"); }
foreach (var n in new[] { "Bake_Pool_0", "Bake_Pool_1", "Bake_Pool_2", "Bake_Pool_3" }) SetL(root, n, 30f, "FFB870");
SetL(root, "Bake_Reception_A", 4.5f, null); SetL(root, "Bake_Reception_B", 4.5f, null);
SetL(root, "Bake_GlassEast", 0.8f, null); SetL(root, "Bake_GlassNorth", 0.8f, null);
var warmed = new System.Collections.Generic.HashSet<UnityEngine.Material>();
foreach (var r in root.GetComponentsInChildren<UnityEngine.MeshRenderer>(true)) {
    var low = r.name.ToLower(); if (!(low.Contains("pendant") || low.Contains("disc"))) continue;
    foreach (var m in r.sharedMaterials) if (m != null && m.IsKeywordEnabled("_EMISSION") && !warmed.Contains(m)) { m.SetColor("_EmissionColor", Hex("FFB466") * 2.2f); UnityEditor.EditorUtility.SetDirty(m); warmed.Add(m); }
}
sb.AppendLine("lobby pendant materials warmed " + warmed.Count);

// apartments: the sky outside reads stronger, plus the colour of the signs facing them
UseRoot("Floor17/Apartment_17F02", "VisualPass_17F02");
SetL(root, "Bake_BedWindow", 1.25f, null); SetL(root, "Bake_LivingWindow", 1.3f, null); SetL(root, "Bake_LivingDoor", 1.3f, null);
Twin("Bake_BedWindow", "Bake_BedNeon", "C858B8", 0.8f, 0.8f);
UseRoot("Floor17/Apartment_17F03", "VisualPass_17F03");
SetL(root, "Bake_MainWindow", 1.2f, null); SetL(root, "Bake_MainDoor", 1.2f, null); SetL(root, "Bake_AvaWindow", 0.8f, null);
UseRoot("Floor17/Apartment_17F04", "VisualPass_17F04");
SetL(root, "Bake_BalconyDoor", 1.3f, null); SetL(root, "Bake_SideWindow", 1.1f, null); SetL(root, "Bake_LilyWindow_E", 1.0f, null); SetL(root, "Bake_LilyWindow_S", 1.0f, null);
Twin("Bake_BalconyDoor", "Bake_BillboardSpill", "D050B0", 1.6f, 0.9f);
// a plate kept for Mia at the table: the home goes on without her
var oldPlate = propsT.Find("Prop_PlateForMia"); if (oldPlate != null) UnityEngine.Object.DestroyImmediate(oldPlate.gameObject);
On(Old("RoomTable_01"), "dinner_plate", "Prop_PlateForMia", 'F', 0.28f, -0.45f, -0.3f, 0f, null);
