// Noah's room pass, stage 3: night lighting. Rebuilds only the Lights group.
if (UnityEditor.EditorApplication.isPlaying) return "skip: play mode";
var sb = new System.Text.StringBuilder();
var root = UnityEngine.GameObject.Find("Floor17/Apartment_17F01/VisualPass_NoahRoom").transform;
var group = root.Find("Lights");
while (group.childCount > 0) UnityEngine.Object.DestroyImmediate(group.GetChild(0).gameObject);

var warm = new UnityEngine.Color(1f, 0.72f, 0.46f); var moon = new UnityEngine.Color(0.42f, 0.58f, 1f); var sky = new UnityEngine.Color(0.36f, 0.50f, 1f); var neon = new UnityEngine.Color(0.95f, 0.30f, 0.75f);
System.Func<string, UnityEngine.LightType, UnityEngine.LightmapBakeType, UnityEngine.Vector3, UnityEngine.Color, float, float, UnityEngine.Light> add = (n, type, mode, pos, col, I, R) => {
    var g = new UnityEngine.GameObject(n); g.transform.SetParent(group, false); g.transform.localPosition = pos;
    var l = g.AddComponent<UnityEngine.Light>(); l.type = type; l.lightmapBakeType = mode; l.color = col; l.intensity = I; l.range = R; l.bounceIntensity = 1f;
    g.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalLightData>();
    return l;
};
System.Action<UnityEngine.Light, int> tier = (l, t) => {
    var so = new UnityEditor.SerializedObject(l.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalLightData>());
    var p = so.FindProperty("m_AdditionalLightsShadowResolutionTier"); if (p != null) { p.intValue = t; so.ApplyModifiedPropertiesWithoutUndo(); }
};
System.Action<UnityEngine.Light, UnityEngine.Vector3, float, float> aim = (l, target, outer, inner) => { l.spotAngle = outer; l.innerSpotAngle = inner; l.transform.LookAt(root.TransformPoint(target)); };

var m = add("Light_Moon", UnityEngine.LightType.Spot, UnityEngine.LightmapBakeType.Mixed, new UnityEngine.Vector3(4.4f, 2.9f, -1.0f), moon, MOON_I, 13f);
aim(m, new UnityEngine.Vector3(0f, 0.9f, 1.5f), 58f, 40f); m.shadows = UnityEngine.LightShadows.Soft; m.shadowStrength = 1f; m.shadowNearPlane = 0.6f; m.shadowBias = 0.02f; m.shadowNormalBias = 0.2f; tier(m, 2);

var k = add("Light_TableLamp", UnityEngine.LightType.Spot, UnityEngine.LightmapBakeType.Mixed, new UnityEngine.Vector3(0.70f, 0.76f, 1.56f), warm, LAMP_I, 6.5f);
aim(k, new UnityEngine.Vector3(-0.3f, 0.35f, 1.0f), 150f, 60f); k.shadows = UnityEngine.LightShadows.Soft; k.shadowNearPlane = 0.1f; k.shadowBias = 0.03f; k.shadowNormalBias = 0.3f; tier(k, 1);

var g2 = add("Bake_LampGlow", UnityEngine.LightType.Point, UnityEngine.LightmapBakeType.Baked, new UnityEngine.Vector3(0.72f, 0.82f, 1.60f), warm, GLOW_I, 5f); g2.shadows = UnityEngine.LightShadows.Soft; g2.shadowRadius = 0.12f;

System.Action<string, float, float> window = (n, a, b) => {
    var w = add(n, UnityEngine.LightType.Rectangle, UnityEngine.LightmapBakeType.Baked, new UnityEngine.Vector3(1.93f, 1.67f, (a + b) * 0.5f), sky, SKY_I, 9f);
    w.transform.localRotation = UnityEngine.Quaternion.Euler(0, -90f, 0); w.areaSize = new UnityEngine.Vector2(b - a, 1.55f); w.shadows = UnityEngine.LightShadows.Soft;
};
window("Bake_WindowSky_1", -1.67f, -0.87f); window("Bake_WindowSky_2", -0.17f, 1.73f);

var n2 = add("Bake_NeonBounce", UnityEngine.LightType.Spot, UnityEngine.LightmapBakeType.Baked, new UnityEngine.Vector3(2.5f, 1.3f, 0.6f), neon, NEON_I, 7f);
aim(n2, new UnityEngine.Vector3(-1.5f, 2.3f, 1.2f), 70f, 40f); n2.shadows = UnityEngine.LightShadows.Soft;

var old = group.Find("Probe_NoahRoom"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
var pg = new UnityEngine.GameObject("Probe_NoahRoom"); pg.transform.SetParent(group, false); pg.transform.position = root.TransformPoint(new UnityEngine.Vector3(-0.14f, 1.5f, -0.02f));
var rp = pg.AddComponent<UnityEngine.ReflectionProbe>(); rp.mode = UnityEngine.Rendering.ReflectionProbeMode.Baked; rp.size = new UnityEngine.Vector3(5.4f, 3.0f, 5.2f); rp.boxProjection = true; rp.importance = 5; rp.blendDistance = 0.3f; rp.resolution = 128; rp.hdr = true;

UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
UnityEngine.RaycastHit h; var moonFrom = root.TransformPoint(new UnityEngine.Vector3(4.4f, 2.9f, -1.0f)); var moonTo = root.TransformPoint(new UnityEngine.Vector3(2.05f, 1.75f, 0.35f));
sb.AppendLine("moon to window blocked=" + UnityEngine.Physics.Linecast(moonFrom, moonTo, out h, ~0, UnityEngine.QueryTriggerInteraction.Ignore) + (h.collider != null ? " by " + h.collider.name : ""));
sb.AppendLine("lights=" + group.childCount);
return sb.ToString();
