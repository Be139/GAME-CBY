using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(CityDuskLightingPresetController))]
public class CityDuskLightingPresetControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CityDuskLightingPresetController controller = (CityDuskLightingPresetController)target;
        EditorGUILayout.Space(10f);

        if (GUILayout.Button("Apply Fixed Dusk"))
        {
            Undo.RecordObject(controller, "Apply fixed dusk");
            controller.ApplyDusk();
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
        }

        if (GUILayout.Button("Restore Original Lighting"))
        {
            Undo.RecordObject(controller, "Restore original lighting");
            controller.RestoreOriginal();
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
        }
    }
}

public static class CityDuskLightingSetupEditor
{
    private const string OriginalProfilePath = "Assets/Settings/SampleSceneProfile.asset";
    private const string DuskProfilePath = "Assets/Settings/SampleSceneProfile_Dusk.asset";
    private const string BillboardMaterialPath = "Assets/Materials/City/M_CityBillboard_HDR_Unlit.mat";
    private const string DuskSkyboxPath = "Assets/Materials/Environment/M_Skybox_Dusk.mat";
    private const string WindowCookiePath = "Assets/Textures/Lighting/T_WindowFrameCookie.asset";

    [MenuItem("Tools/City/Lighting/Apply Fixed Dusk")]
    public static void ApplyFixedDusk()
    {
        CityDuskLightingPresetController controller =
            Object.FindObjectOfType<CityDuskLightingPresetController>(true);
        if (controller == null)
        {
            Debug.LogWarning("City dusk setup: build the fixed dusk preset first.");
            return;
        }

        Undo.RecordObject(controller, "Apply fixed dusk");
        controller.gameObject.SetActive(true);
        controller.ApplyDusk();
        EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
    }

    [MenuItem("Tools/City/Lighting/Restore Original Lighting")]
    public static void RestoreOriginalLighting()
    {
        CityDuskLightingPresetController controller =
            Object.FindObjectOfType<CityDuskLightingPresetController>(true);
        if (controller == null)
        {
            Debug.LogWarning("City dusk setup: no fixed dusk preset was found.");
            return;
        }

        Undo.RecordObject(controller, "Restore original lighting");
        controller.RestoreOriginal();
        EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
    }

    [MenuItem("Tools/City/Lighting/Apply Brighter Dusk Baseline")]
    public static void ApplyBrighterDuskBaseline()
    {
        CityDuskLightingPresetController controller =
            Object.FindObjectOfType<CityDuskLightingPresetController>(true);
        Light duskSun = FindLight("Sun_Dusk");
        VolumeProfile duskProfile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(DuskProfilePath);
        Material duskSkybox = AssetDatabase.LoadAssetAtPath<Material>(DuskSkyboxPath);

        if (controller == null || duskSun == null || duskProfile == null || duskSkybox == null)
        {
            Debug.LogError(
                "City dusk setup: fixed dusk preset is incomplete. Build the fixed dusk preset first.");
            return;
        }

        ApplyBrighterDuskSettings(controller, duskSun, duskProfile, duskSkybox);
        controller.ApplyDusk();
        EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(controller.gameObject.scene);
        Debug.Log("City dusk setup: brighter dusk baseline applied without changing the sun angle.");
    }

    [MenuItem("Tools/City/Lighting/Build Fixed Dusk Preset")]
    public static void BuildFixedDuskPreset()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("City dusk setup: no loaded scene is available.");
            return;
        }

        Light originalSun = FindLight("Directional Light");
        Volume originalVolume = FindComponentByName<Volume>("Global Volume");
        Transform room = FindTransformByPath("17F/ROOM1");
        CityBillboardContentDistributor distributor =
            FindComponentByName<CityBillboardContentDistributor>("CityBillboardPlacer");

        if (originalSun == null || originalVolume == null || room == null)
        {
            Debug.LogError("City dusk setup: Directional Light, Global Volume, or 17F/ROOM1 could not be found.");
            return;
        }

        EnsureAssetFolder("Assets/Materials");
        EnsureAssetFolder("Assets/Materials/City");
        EnsureAssetFolder("Assets/Materials/Environment");
        EnsureAssetFolder("Assets/Textures");
        EnsureAssetFolder("Assets/Textures/Lighting");

        Material billboardMaterial = CreateOrUpdateBillboardMaterial();
        Material duskSkybox = CreateOrUpdateDuskSkybox();
        Texture2D windowCookie = CreateOrUpdateWindowCookie();
        VolumeProfile duskProfile = CreateOrUpdateDuskProfile();

        GameObject presetRoot = GameObject.Find("Lighting_Dusk_Fixed");
        bool createdPresetRoot = presetRoot == null;
        if (createdPresetRoot)
        {
            presetRoot = new GameObject("Lighting_Dusk_Fixed");
            presetRoot.SetActive(false);
            Undo.RegisterCreatedObjectUndo(presetRoot, "Create fixed dusk lighting");
        }

        CityDuskLightingPresetController presetController =
            presetRoot.GetComponent<CityDuskLightingPresetController>();
        if (presetController == null)
        {
            presetController = Undo.AddComponent<CityDuskLightingPresetController>(presetRoot);
        }
        else if (presetController.IsDuskApplied)
        {
            presetController.RestoreOriginal();
        }

        presetRoot.SetActive(false);
        GameObject duskContent = FindOrCreateChild(presetRoot.transform, "Dusk_Content");
        duskContent.SetActive(false);

        Light duskSun = FindOrCreateLight(duskContent.transform, "Sun_Dusk", LightType.Directional);
        duskSun.transform.rotation = Quaternion.Euler(12f, 330f, 0f);
        duskSun.color = new Color(1f, 0.58f, 0.38f, 1f);
        duskSun.useColorTemperature = false;
        duskSun.intensity = 1f;
        duskSun.shadows = LightShadows.Soft;
        duskSun.shadowStrength = 0.8f;

        GameObject duskVolumeObject = FindOrCreateChild(duskContent.transform, "Global Volume_Dusk");
        Volume duskVolume = GetOrAddComponent<Volume>(duskVolumeObject);
        duskVolume.isGlobal = true;
        duskVolume.priority = 10f;
        duskVolume.weight = 1f;
        duskVolume.sharedProfile = duskProfile;

        GameObject sampleRoot = FindOrCreateChild(room, "Lighting_17F_ROOM1_DuskSample");
        sampleRoot.SetActive(false);
        ClearChildren(sampleRoot.transform);
        BuildRoomSample(room, sampleRoot.transform, windowCookie);

        Undo.RecordObject(presetController, "Configure fixed dusk lighting");
        presetController.Configure(originalSun, originalVolume, duskContent, sampleRoot, duskSkybox);
        ApplyBrighterDuskSettings(presetController, duskSun, duskProfile, duskSkybox);

        ApplyBillboardMaterial(distributor, billboardMaterial);
        LockMinLoopEnvironmentOverrides();

        presetRoot.SetActive(true);
        presetController.ApplyDusk();

        EditorUtility.SetDirty(presetController);
        EditorUtility.SetDirty(originalSun);
        EditorUtility.SetDirty(originalVolume);
        EditorSceneManager.MarkSceneDirty(scene);
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(scene);
        Debug.Log("City dusk setup: fixed dusk, HDR billboards, and the 17F/ROOM1 lighting sample are ready.");
    }

    private static VolumeProfile CreateOrUpdateDuskProfile()
    {
        if (AssetDatabase.LoadAssetAtPath<VolumeProfile>(DuskProfilePath) == null)
        {
            if (!AssetDatabase.CopyAsset(OriginalProfilePath, DuskProfilePath))
            {
                VolumeProfile fallback = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(fallback, DuskProfilePath);
            }
        }

        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(DuskProfilePath);

        Tonemapping tonemapping = GetOrAddVolumeOverride<Tonemapping>(profile);
        tonemapping.active = true;
        tonemapping.mode.Override(TonemappingMode.ACES);

        Bloom bloom = GetOrAddVolumeOverride<Bloom>(profile);
        bloom.active = true;
        bloom.threshold.Override(1.05f);
        bloom.intensity.Override(0.32f);
        bloom.scatter.Override(0.58f);

        Vignette vignette = GetOrAddVolumeOverride<Vignette>(profile);
        vignette.active = true;
        vignette.intensity.Override(0.18f);
        vignette.smoothness.Override(0.45f);

        ColorAdjustments colorAdjustments = GetOrAddVolumeOverride<ColorAdjustments>(profile);
        colorAdjustments.active = true;
        colorAdjustments.postExposure.Override(0.1f);
        colorAdjustments.contrast.Override(8f);
        colorAdjustments.saturation.Override(-5f);

        SplitToning splitToning = GetOrAddVolumeOverride<SplitToning>(profile);
        splitToning.active = true;
        splitToning.shadows.Override(new Color(0.28f, 0.38f, 0.56f, 1f));
        splitToning.highlights.Override(new Color(0.92f, 0.6f, 0.34f, 1f));
        splitToning.balance.Override(-15f);

        EditorUtility.SetDirty(profile);
        return profile;
    }

    private static T GetOrAddVolumeOverride<T>(VolumeProfile profile) where T : VolumeComponent
    {
        profile.components.RemoveAll(component => component == null);

        T component;
        bool hasPersistentComponent =
            profile.TryGet(out component) &&
            component != null &&
            AssetDatabase.IsSubAsset(component);
        if (!hasPersistentComponent)
        {
            if (component != null)
            {
                profile.components.Remove(component);
            }

            component = ScriptableObject.CreateInstance<T>();
            component.name = typeof(T).Name;
            component.hideFlags = HideFlags.HideInInspector | HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(component, profile);
            profile.components.Add(component);
        }

        EditorUtility.SetDirty(component);
        EditorUtility.SetDirty(profile);
        return component;
    }

    private static Material CreateOrUpdateBillboardMaterial()
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(BillboardMaterialPath);
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (material == null)
        {
            material = new Material(shader);
            material.name = "M_CityBillboard_HDR_Unlit";
            AssetDatabase.CreateAsset(material, BillboardMaterialPath);
        }
        else if (shader != null && material.shader != shader)
        {
            material.shader = shader;
        }

        Color blankColor = new Color(0.02f, 0.06f, 0.09f, 1f);
        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", blankColor);
        }
        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", blankColor);
        }

        material.enableInstancing = true;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material CreateOrUpdateDuskSkybox()
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(DuskSkyboxPath);
        Shader shader = Shader.Find("Skybox/Procedural");
        if (material == null)
        {
            material = new Material(shader);
            material.name = "M_Skybox_Dusk";
            AssetDatabase.CreateAsset(material, DuskSkyboxPath);
        }
        else if (shader != null && material.shader != shader)
        {
            material.shader = shader;
        }

        SetFloatIfSupported(material, "_SunDisk", 2f);
        SetFloatIfSupported(material, "_SunSize", 0.025f);
        SetFloatIfSupported(material, "_SunSizeConvergence", 5f);
        SetFloatIfSupported(material, "_AtmosphereThickness", 0.8f);
        SetFloatIfSupported(material, "_Exposure", 0.72f);
        SetColorIfSupported(material, "_SkyTint", new Color(0.25f, 0.32f, 0.55f, 1f));
        SetColorIfSupported(material, "_GroundColor", new Color(0.12f, 0.08f, 0.12f, 1f));
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Texture2D CreateOrUpdateWindowCookie()
    {
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(WindowCookiePath);
        if (texture == null)
        {
            texture = new Texture2D(256, 256, TextureFormat.RGBA32, false, true);
            texture.name = "T_WindowFrameCookie";
            AssetDatabase.CreateAsset(texture, WindowCookiePath);
        }

        const int size = 256;
        if (texture.width != size || texture.height != size)
        {
            texture.Reinitialize(size, size, TextureFormat.RGBA32, false);
        }

        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            float v = (y + 0.5f) / size;
            for (int x = 0; x < size; x++)
            {
                float u = (x + 0.5f) / size;
                float dx = Mathf.Abs(u - 0.5f) * 2f;
                float dy = Mathf.Abs(v - 0.5f) * 2f;
                float edge = Mathf.Clamp01(1f - Mathf.Max(dx, dy));
                edge = Mathf.SmoothStep(0f, 1f, edge * 2.5f);
                float mullion = Mathf.Abs(u - 0.5f) < 0.025f ? 0.12f : 1f;
                float crossbar = Mathf.Abs(v - 0.5f) < 0.018f ? 0.18f : 1f;
                float value = edge * mullion * crossbar;
                pixels[y * size + x] = new Color(value, value, value, value);
            }
        }

        texture.SetPixels(pixels);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        texture.Apply(false, false);
        EditorUtility.SetDirty(texture);
        return texture;
    }

    private static void ApplyBrighterDuskSettings(
        CityDuskLightingPresetController controller,
        Light duskSun,
        VolumeProfile duskProfile,
        Material duskSkybox)
    {
        Undo.RecordObject(controller, "Brighten dusk ambient");
        SerializedObject serializedController = new SerializedObject(controller);
        SerializedProperty ambientIntensity = serializedController.FindProperty("ambientIntensity");
        if (ambientIntensity != null)
        {
            ambientIntensity.floatValue = 0.9f;
        }
        serializedController.ApplyModifiedProperties();

        Undo.RecordObject(duskSun, "Brighten dusk sun");
        duskSun.intensity = 1f;

        ColorAdjustments colorAdjustments = GetOrAddVolumeOverride<ColorAdjustments>(duskProfile);
        colorAdjustments.active = true;
        colorAdjustments.postExposure.Override(0.1f);
        colorAdjustments.contrast.Override(8f);
        colorAdjustments.saturation.Override(-5f);

        SplitToning splitToning = GetOrAddVolumeOverride<SplitToning>(duskProfile);
        splitToning.active = true;
        splitToning.shadows.Override(new Color(0.28f, 0.38f, 0.56f, 1f));
        splitToning.highlights.Override(new Color(0.92f, 0.6f, 0.34f, 1f));
        splitToning.balance.Override(-15f);

        SetFloatIfSupported(duskSkybox, "_Exposure", 0.72f);
        EditorUtility.SetDirty(controller);
        EditorUtility.SetDirty(duskSun);
        EditorUtility.SetDirty(duskProfile);
        EditorUtility.SetDirty(duskSkybox);
    }

    private static void BuildRoomSample(Transform room, Transform sampleRoot, Texture cookie)
    {
        Bounds roomBounds = CalculateBounds(room.gameObject);
        Transform coolCurtain = room.Find("CurtainSet_19");
        Transform magentaCurtain = room.Find("CurtainSet_10 (1)");
        Transform deskLightAnchor = room.Find("(Prb)DeskLight");

        CreateWindowSpot(
            sampleRoot,
            "WindowFill_Cool",
            coolCurtain,
            roomBounds,
            new Color(0.36f, 0.5f, 1f, 1f),
            0.55f,
            9f,
            72f,
            48f,
            LightShadows.Soft,
            cookie);

        CreateWindowSpot(
            sampleRoot,
            "BillboardSpill_Magenta",
            magentaCurtain,
            roomBounds,
            new Color(1f, 0.18f, 0.55f, 1f),
            0.18f,
            6.5f,
            50f,
            34f,
            LightShadows.None,
            null);

        Light practical = FindOrCreateLight(sampleRoot, "Practical_Warm", LightType.Point);
        practical.color = new Color(1f, 0.6f, 0.3f, 1f);
        practical.intensity = 0.65f;
        practical.range = 4f;
        practical.shadows = LightShadows.None;
        practical.transform.position = deskLightAnchor != null
            ? CalculateBounds(deskLightAnchor.gameObject).center + Vector3.up * 0.35f
            : roomBounds.center + Vector3.up * 0.4f;

        GameObject probeObject = FindOrCreateChild(sampleRoot, "ReflectionProbe_ROOM1");
        probeObject.transform.position = roomBounds.center;
        ReflectionProbe probe = GetOrAddComponent<ReflectionProbe>(probeObject);
        probe.mode = ReflectionProbeMode.Baked;
        probe.boxProjection = true;
        probe.intensity = 1f;
        probe.blendDistance = 1f;
        probe.resolution = 256;
        probe.center = Vector3.zero;
        probe.size = roomBounds.size + new Vector3(0.5f, 0.5f, 0.5f);
    }

    private static void CreateWindowSpot(
        Transform parent,
        string name,
        Transform anchor,
        Bounds roomBounds,
        Color color,
        float intensity,
        float range,
        float outerAngle,
        float innerAngle,
        LightShadows shadows,
        Texture cookie)
    {
        Light light = FindOrCreateLight(parent, name, LightType.Spot);
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.spotAngle = outerAngle;
        light.innerSpotAngle = innerAngle;
        light.shadows = shadows;
        light.shadowStrength = shadows == LightShadows.None ? 0f : 0.65f;
        light.cookie = cookie;

        Bounds anchorBounds = anchor != null ? CalculateBounds(anchor.gameObject) : roomBounds;
        Vector3 anchorCenter = anchorBounds.center;
        Vector3 outward = anchorCenter - roomBounds.center;
        outward.y = 0f;
        if (outward.sqrMagnitude < 0.01f)
        {
            outward = anchor != null ? anchor.forward : Vector3.forward;
            outward.y = 0f;
        }
        outward.Normalize();

        Vector3 position = anchorCenter + outward * 0.75f + Vector3.up * 0.15f;
        Vector3 target = anchorCenter - outward * Mathf.Min(4f, range * 0.55f);
        target.y = Mathf.Lerp(anchorCenter.y, roomBounds.center.y, 0.35f);
        light.transform.position = position;
        light.transform.rotation = Quaternion.LookRotation((target - position).normalized, Vector3.up);
    }

    private static void ApplyBillboardMaterial(
        CityBillboardContentDistributor distributor,
        Material billboardMaterial)
    {
        if (distributor == null)
        {
            Debug.LogWarning("City dusk setup: CityBillboardContentDistributor was not found.");
            return;
        }

        Undo.RecordObject(distributor, "Configure billboard HDR material");
        distributor.MediaSurfaceMaterial = billboardMaterial;
        distributor.ApplySurfaceMaterialToAll();

        CityBillboardContentController[] controllers =
            Object.FindObjectsOfType<CityBillboardContentController>(true);
        for (int i = 0; i < controllers.Length; i++)
        {
            Undo.RecordObject(controllers[i], "Set billboard HDR brightness");
            controllers[i].SetBrightness(2.2f, 2.6f);
            EditorUtility.SetDirty(controllers[i]);
        }

        CityFacadeBillboardPlacer placer =
            distributor.GetComponent<CityFacadeBillboardPlacer>();
        if (placer != null)
        {
            SerializedObject serializedPlacer = new SerializedObject(placer);
            SerializedProperty emissionColor = serializedPlacer.FindProperty("placeholderEmissionColor");
            SerializedProperty emissionIntensity = serializedPlacer.FindProperty("placeholderEmissionIntensity");
            if (emissionColor != null)
            {
                emissionColor.colorValue = Color.white;
            }
            if (emissionIntensity != null)
            {
                emissionIntensity.floatValue = 1.4f;
            }
            serializedPlacer.ApplyModifiedProperties();
        }
    }

    private static void LockMinLoopEnvironmentOverrides()
    {
        MinLoopLightingStateController controller =
            Object.FindObjectOfType<MinLoopLightingStateController>(true);
        if (controller == null)
        {
            return;
        }

        SerializedObject serializedController = new SerializedObject(controller);
        SerializedProperty property =
            serializedController.FindProperty("allowRulesToOverrideSceneEnvironment");
        if (property != null)
        {
            property.boolValue = false;
            serializedController.ApplyModifiedProperties();
            EditorUtility.SetDirty(controller);
        }
    }

    private static Light FindLight(string gameObjectName)
    {
        GameObject found = GameObject.Find(gameObjectName);
        return found != null ? found.GetComponent<Light>() : null;
    }

    private static T FindComponentByName<T>(string gameObjectName) where T : Component
    {
        GameObject found = GameObject.Find(gameObjectName);
        return found != null ? found.GetComponent<T>() : null;
    }

    private static Transform FindTransformByPath(string path)
    {
        GameObject found = GameObject.Find(path);
        return found != null ? found.transform : null;
    }

    private static GameObject FindOrCreateChild(Transform parent, string name)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject child = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(child, "Create " + name);
        child.transform.SetParent(parent, false);
        return child;
    }

    private static Light FindOrCreateLight(Transform parent, string name, LightType type)
    {
        GameObject lightObject = FindOrCreateChild(parent, name);
        Light light = GetOrAddComponent<Light>(lightObject);
        light.type = type;
        return light;
    }

    private static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        return component != null ? component : Undo.AddComponent<T>(gameObject);
    }

    private static Bounds CalculateBounds(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return new Bounds(root.transform.position, Vector3.one);
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }
        return bounds;
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Undo.DestroyObjectImmediate(parent.GetChild(i).gameObject);
        }
    }

    private static void EnsureAssetFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = Path.GetDirectoryName(path).Replace("\\", "/");
        string name = Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, name);
    }

    private static void SetFloatIfSupported(Material material, string propertyName, float value)
    {
        if (material != null && material.HasProperty(propertyName))
        {
            material.SetFloat(propertyName, value);
        }
    }

    private static void SetColorIfSupported(Material material, string propertyName, Color value)
    {
        if (material != null && material.HasProperty(propertyName))
        {
            material.SetColor(propertyName, value);
        }
    }
}
