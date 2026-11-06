// Human camera grading: the old profile pushed exposure 1.5 stops, tinted highlights orange and crushed shadows with ACES. This keeps the blue in the shadows and leaves highlights neutral.
// Noah's probes join here too, since his room is not rebuilt by a Room() spec.
var vol = (UnityEngine.Rendering.Volume)null; foreach (var v in UnityEngine.Object.FindObjectsOfType<UnityEngine.Rendering.Volume>(true)) if (v.name == "Global Volume_Dusk") vol = v;
var profile = vol.sharedProfile;
UnityEngine.Rendering.Universal.Tonemapping tm; UnityEngine.Rendering.Universal.ColorAdjustments ca; UnityEngine.Rendering.Universal.SplitToning st;
profile.TryGet(out tm); profile.TryGet(out ca); profile.TryGet(out st);
tm.mode.value = UnityEngine.Rendering.Universal.TonemappingMode.Neutral; ca.postExposure.value = 0.6f; ca.contrast.value = 0f; ca.saturation.value = 5f;
st.highlights.value = new UnityEngine.Color(0.5f, 0.5f, 0.5f); st.balance.value = -20f;
UnityEditor.EditorUtility.SetDirty(profile);
var noah = UnityEngine.GameObject.Find("Floor17/Apartment_17F01/VisualPass_NoahRoom/Lights").transform;
var old = noah.Find("Probes_Noah"); if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
var g = new UnityEngine.GameObject("Probes_Noah"); g.transform.SetParent(noah, false); g.transform.localPosition = UnityEngine.Vector3.zero;
var pts = new System.Collections.Generic.List<UnityEngine.Vector3>();
for (float u = -1.9f; u <= 1.7f; u += 1.2f) for (float w = -1.5f; w <= 1.5f; w += 1.5f) foreach (var y in new[] { 0.5f, 1.5f, 2.5f }) pts.Add(new UnityEngine.Vector3(u, y, w));
g.AddComponent<UnityEngine.LightProbeGroup>().probePositions = pts.ToArray();
UnityEditor.AssetDatabase.SaveAssets();
UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
return "post set, noah probes " + pts.Count;
