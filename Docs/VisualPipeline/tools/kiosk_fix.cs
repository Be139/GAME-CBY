// Puts the service screen on the kiosk's largest flat face, wherever the kiosk now stands and faces.
var root = UnityEngine.GameObject.Find("Floor17/VisualPass_Corridor").transform;
var atmos = root.Find("Atmos_1106"); var kiosk = root.Find("Props/Prop_Kiosk_Arrival");
var oldQ = atmos.Find("Kiosk_Screen"); if (oldQ != null) UnityEngine.Object.DestroyImmediate(oldQ.gameObject);
var mr = kiosk.GetComponentInChildren<UnityEngine.MeshRenderer>(); var col = mr.gameObject.AddComponent<UnityEngine.MeshCollider>(); col.sharedMesh = mr.GetComponent<UnityEngine.MeshFilter>().sharedMesh; UnityEngine.Physics.SyncTransforms();
var c0 = mr.bounds.center; var toHall = root.TransformPoint(new UnityEngine.Vector3(-9.75f, 0f, 6f)) - c0; toHall.y = 0; toHall.Normalize(); float yMin = mr.bounds.min.y, yMax = mr.bounds.max.y;
float bestArea = 0; UnityEngine.Vector3 bestC = UnityEngine.Vector3.zero, bestN = UnityEngine.Vector3.zero, bestUp = UnityEngine.Vector3.up; float bestW = 0, bestH = 0;
for (int k = 0; k < 16; k++) {
    float ang = k * 22.5f * UnityEngine.Mathf.Deg2Rad; var dir = new UnityEngine.Vector3(UnityEngine.Mathf.Cos(ang), 0, UnityEngine.Mathf.Sin(ang)); var side = new UnityEngine.Vector3(-dir.z, 0, dir.x);
    var hits = new System.Collections.Generic.List<UnityEngine.RaycastHit>();
    for (float y = yMin + 0.05f; y < yMax; y += 0.03f) { UnityEngine.RaycastHit h; if (col.Raycast(new UnityEngine.Ray(new UnityEngine.Vector3(c0.x, y, c0.z) - dir * 2f, dir), out h, 4f)) hits.Add(h); }
    int a = 0, ba = 0, bb = -1;
    for (int i = 1; i <= hits.Count; i++) { if (i == hits.Count || UnityEngine.Vector3.Angle(hits[i].normal, hits[a].normal) > 6f) { if (i - 1 - a > bb - ba) { ba = a; bb = i - 1; } a = i; } }
    if (bb - ba < 4) continue;
    var lo = hits[ba]; var hi = hits[bb]; var mid = hits[(ba + bb) / 2];
    var nh = mid.normal; nh.y = 0; if (UnityEngine.Vector3.Dot(nh.normalized, toHall) < 0.2f) continue; float hgt = (hi.point - lo.point).magnitude;
    int run = 0, best = 0;
    for (float s = -0.6f; s <= 0.6f; s += 0.02f) { UnityEngine.RaycastHit h; bool ok = col.Raycast(new UnityEngine.Ray(mid.point - dir * 1f + side * s, dir), out h, 2f) && UnityEngine.Vector3.Angle(h.normal, mid.normal) < 6f; run = ok ? run + 1 : 0; if (run > best) best = run; }
    float wid = best * 0.02f;
    if (hgt * wid > bestArea) { bestArea = hgt * wid; bestC = (lo.point + hi.point) * 0.5f; bestN = mid.normal; bestUp = (hi.point - lo.point).normalized; bestW = wid; bestH = hgt; }
}
UnityEngine.Object.DestroyImmediate(col);
if (bestArea <= 0) return "no face";
var q = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Quad); UnityEngine.Object.DestroyImmediate(q.GetComponent<UnityEngine.Collider>());
q.name = "Kiosk_Screen"; q.transform.SetParent(atmos, true); q.transform.position = bestC + bestN * 0.006f; q.transform.rotation = UnityEngine.Quaternion.LookRotation(-bestN, bestUp);
float sh = UnityEngine.Mathf.Min(bestH * 0.8f, bestW * 0.8f * 1.5f); q.transform.localScale = new UnityEngine.Vector3(sh / 1.5f, sh, 1f);
var qr = q.GetComponent<UnityEngine.MeshRenderer>(); qr.sharedMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>("Assets/Art/Generated/Rooms/shared/M_Kiosk_Screen.mat"); qr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; UnityEditor.GameObjectUtility.SetStaticEditorFlags(q, 0);
var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene(); UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene); UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
var lp = root.InverseTransformPoint(bestC); var ln = root.InverseTransformDirection(bestN);
return "face w=" + bestW.ToString("0.00") + " h=" + bestH.ToString("0.00") + " at u=" + lp.x.ToString("0.0") + " v=" + lp.z.ToString("0.0") + " y=" + lp.y.ToString("0.0") + " normal=" + ln.ToString("0.0") + " tilt=" + UnityEngine.Vector3.Angle(bestUp, UnityEngine.Vector3.up).ToString("0");
