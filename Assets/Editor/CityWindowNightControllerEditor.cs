using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(CityWindowNightController))]
public class CityWindowNightControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CityWindowNightController controller =
            (CityWindowNightController)target;

        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("Window Night Actions", EditorStyles.boldLabel);

        if (GUILayout.Button("Build / Refresh And Apply"))
        {
            CityWindowNightSetupEditor.BuildOrRefreshAndApply();
        }

        if (GUILayout.Button("Apply Night Windows"))
        {
            CityWindowNightSetupEditor.ApplyNightWindows();
        }

        if (GUILayout.Button("Restore Original Windows"))
        {
            CityWindowNightSetupEditor.RestoreOriginalWindows();
        }

        if (GUILayout.Button("Redistribute Night Windows"))
        {
            CityWindowNightSetupEditor.RedistributeNightWindows();
        }

        if (GUILayout.Button("Validate Setup"))
        {
            controller.ValidateSetup();
        }
    }
}

public static class CityWindowNightSetupEditor
{
    private const string MenuRoot = "Tools/City/Lighting/Window Night/";
    private const string SystemObjectName = "CityWindowNightSystem";
    private const string SelectedBuildingRootName = "BUILDING";
    private const string HeightBuildingRootName = "CityBuildings_ByHeight";
    private const string MaterialFolder = "Assets/materials/City/WindowNight";
    private const string MaskFolder = "Assets/Textures/Lighting/CityWindows";
    private const int MaskResolution = 512;

    private static readonly string[] SourceMaterialPaths =
    {
        "Assets/hazelwoodloft/CITY_DATA_NEW/new_city_materials/city_day_reflective_windows.mat",
        "Assets/JapaneseCity/Materials/jcBldWinA.mat",
        "Assets/JapaneseCity/Materials/jcBldWin2divA.mat",
        "Assets/JapaneseCity/Materials/jcBldWin4divA.mat",
        "Assets/JapaneseCity/Materials/jcBldWin5divA.mat"
    };

    [MenuItem(MenuRoot + "Build Or Refresh And Apply")]
    public static void BuildOrRefreshAndApply()
    {
        BuildOrRefresh(true);
    }

    [MenuItem(MenuRoot + "Apply Night Windows")]
    public static void ApplyNightWindows()
    {
        CityWindowNightController controller = FindController();
        if (controller == null)
        {
            Debug.LogWarning(
                "City window night: build the window night system first.");
            return;
        }

        controller.ApplyNightWindows();
        SaveControllerScene(controller);
    }

    [MenuItem(MenuRoot + "Restore Original Windows")]
    public static void RestoreOriginalWindows()
    {
        CityWindowNightController controller = FindController();
        if (controller == null)
        {
            Debug.LogWarning(
                "City window night: no window night system was found.");
            return;
        }

        controller.RestoreOriginalWindows();
        SaveControllerScene(controller);
    }

    [MenuItem(MenuRoot + "Redistribute Night Windows")]
    public static void RedistributeNightWindows()
    {
        CityWindowNightController controller = FindController();
        if (controller == null)
        {
            Debug.LogWarning(
                "City window night: build the window night system first.");
            return;
        }

        Undo.RecordObject(controller, "Redistribute city window night");
        controller.RedistributeNightWindows();
        EditorUtility.SetDirty(controller);
        SaveControllerScene(controller);
    }

    [MenuItem(MenuRoot + "Validate Setup")]
    public static void ValidateSetup()
    {
        CityWindowNightController controller = FindController();
        if (controller == null)
        {
            Debug.LogError(
                "City window night: no window night system was found.");
            return;
        }

        controller.ValidateSetup();
    }

    private static void BuildOrRefresh(bool applyAfterBuild)
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError(
                "City window night: no loaded scene is available.");
            return;
        }

        Transform selectedRoot = FindSceneTransform(
            scene,
            SelectedBuildingRootName);
        Transform heightRoot = FindSceneTransform(
            scene,
            HeightBuildingRootName);

        if (selectedRoot == null || heightRoot == null)
        {
            Debug.LogError(
                "City window night: BUILDING or CityBuildings_ByHeight was not found.");
            return;
        }

        CityWindowNightController controller = GetOrCreateController(scene);
        if (controller.MaterialSets.Count > 0)
        {
            controller.RestoreOriginalWindows();
        }

        List<Material> sourceMaterials = LoadSourceMaterials();
        if (sourceMaterials.Count != SourceMaterialPaths.Length)
        {
            Debug.LogError(
                "City window night: one or more source window materials are missing.");
            return;
        }

        EnsureFolder(MaterialFolder);
        EnsureFolder(MaskFolder);

        Dictionary<Material, List<string>> maskPaths =
            GenerateMaskTextures(controller, sourceMaterials);
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        ConfigureMaskImporters(maskPaths);

        List<CityWindowNightController.WindowMaterialSet> materialSets =
            CreateOrUpdateNightMaterials(controller, sourceMaterials, maskPaths);

        Undo.RecordObject(controller, "Configure city window night");
        controller.Configure(selectedRoot, heightRoot, materialSets);
        EditorUtility.SetDirty(controller);

        if (applyAfterBuild)
        {
            controller.ApplyNightWindows();
        }

        AssetDatabase.SaveAssets();
        SaveControllerScene(controller);
        controller.ValidateSetup();

        Debug.Log(
            "City window night: generated " + materialSets.Count +
            " source material sets with " + controller.PatternVariantCount +
            " fixed patterns each.");
    }

    private static CityWindowNightController GetOrCreateController(Scene scene)
    {
        CityWindowNightController controller = FindController();
        if (controller != null)
        {
            return controller;
        }

        GameObject systemObject = new GameObject(SystemObjectName);
        Undo.RegisterCreatedObjectUndo(systemObject, "Create city window night system");
        SceneManager.MoveGameObjectToScene(systemObject, scene);
        controller = Undo.AddComponent<CityWindowNightController>(systemObject);
        return controller;
    }

    private static CityWindowNightController FindController()
    {
        return Object.FindObjectOfType<CityWindowNightController>(true);
    }

    private static Transform FindSceneTransform(Scene scene, string objectName)
    {
        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform candidate = transforms[i];
            if (candidate.name == objectName &&
                candidate.gameObject.scene == scene)
            {
                return candidate;
            }
        }

        return null;
    }

    private static List<Material> LoadSourceMaterials()
    {
        List<Material> materials = new List<Material>();
        for (int i = 0; i < SourceMaterialPaths.Length; i++)
        {
            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(SourceMaterialPaths[i]);
            if (material == null)
            {
                Debug.LogError(
                    "City window night: source material not found at " +
                    SourceMaterialPaths[i] + ".");
                continue;
            }

            materials.Add(material);
        }

        return materials;
    }

    private static Dictionary<Material, List<string>> GenerateMaskTextures(
        CityWindowNightController controller,
        IList<Material> sourceMaterials)
    {
        Dictionary<Material, List<string>> paths =
            new Dictionary<Material, List<string>>();

        for (int materialIndex = 0; materialIndex < sourceMaterials.Count; materialIndex++)
        {
            Material sourceMaterial = sourceMaterials[materialIndex];
            Texture sourceTexture = GetBaseTexture(sourceMaterial);
            if (sourceTexture == null)
            {
                Debug.LogError(
                    "City window night: " + sourceMaterial.name +
                    " has no base texture.");
                paths[sourceMaterial] = new List<string>();
                continue;
            }

            Color[] sourcePixels = ReadTexturePixels(sourceTexture, MaskResolution);
            List<string> materialMaskPaths = new List<string>();

            for (int patternIndex = 0;
                 patternIndex < controller.PatternVariantCount;
                 patternIndex++)
            {
                Color[] maskPixels = BuildMaskPixels(
                    sourcePixels,
                    controller,
                    sourceMaterial.name,
                    patternIndex);
                string maskPath =
                    MaskFolder + "/T_WindowNight_" +
                    SanitizeFileName(sourceMaterial.name) +
                    "_P" + (patternIndex + 1) + ".png";

                WritePng(maskPath, maskPixels, MaskResolution);
                materialMaskPaths.Add(maskPath);
            }

            paths[sourceMaterial] = materialMaskPaths;
        }

        return paths;
    }

    private static Color[] BuildMaskPixels(
        Color[] sourcePixels,
        CityWindowNightController controller,
        string materialName,
        int patternIndex)
    {
        int gridSize = controller.PatternGridSize;
        Color[] cellColors = new Color[gridSize * gridSize];
        int baseSeed =
            StableHash(materialName) ^
            controller.RandomSeed ^
            ((patternIndex + 1) * 7919);

        for (int cellY = 0; cellY < gridSize; cellY++)
        {
            for (int cellX = 0; cellX < gridSize; cellX++)
            {
                int cellSeed =
                    baseSeed ^
                    (cellX * 73856093) ^
                    (cellY * 19349663);
                float litRoll = Hash01(cellSeed);

                if (litRoll > controller.LitWindowRatio)
                {
                    cellColors[(cellY * gridSize) + cellX] = Color.black;
                    continue;
                }

                float colorRoll = Hash01(cellSeed ^ 83492791);
                cellColors[(cellY * gridSize) + cellX] =
                    PickWindowColor(controller, colorRoll);
            }
        }

        Color[] output = new Color[sourcePixels.Length];
        for (int y = 0; y < MaskResolution; y++)
        {
            int cellY = Mathf.Min(
                gridSize - 1,
                (y * gridSize) / MaskResolution);

            for (int x = 0; x < MaskResolution; x++)
            {
                int index = (y * MaskResolution) + x;
                int cellX = Mathf.Min(
                    gridSize - 1,
                    (x * gridSize) / MaskResolution);
                Color source = sourcePixels[index];
                float luminance =
                    (source.r * 0.2126f) +
                    (source.g * 0.7152f) +
                    (source.b * 0.0722f);
                float frameGate = Mathf.SmoothStep(
                    0f,
                    1f,
                    Mathf.InverseLerp(0.08f, 0.28f, luminance));
                float detail = Mathf.Lerp(0.65f, 1f, Mathf.Clamp01(luminance));
                Color tint = cellColors[(cellY * gridSize) + cellX];

                output[index] = new Color(
                    tint.r * frameGate * detail,
                    tint.g * frameGate * detail,
                    tint.b * frameGate * detail,
                    1f);
            }
        }

        return output;
    }

    private static Color PickWindowColor(
        CityWindowNightController controller,
        float colorRoll)
    {
        if (colorRoll < controller.WarmWindowShare)
        {
            return controller.DeepWarmColor;
        }

        float coolRoll = Mathf.InverseLerp(
            controller.WarmWindowShare,
            1f,
            colorRoll);

        if (coolRoll < 1f / 3f)
        {
            return controller.BlueColor;
        }

        if (coolRoll < 2f / 3f)
        {
            return controller.CyanColor;
        }

        return controller.MagentaColor;
    }

    private static Color[] ReadTexturePixels(Texture source, int resolution)
    {
        RenderTexture previous = RenderTexture.active;
        RenderTexture temporary = RenderTexture.GetTemporary(
            resolution,
            resolution,
            0,
            RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.Default);
        Texture2D readable = new Texture2D(
            resolution,
            resolution,
            TextureFormat.RGBA32,
            false,
            false);

        Graphics.Blit(source, temporary);
        RenderTexture.active = temporary;
        readable.ReadPixels(new Rect(0f, 0f, resolution, resolution), 0, 0);
        readable.Apply(false, false);

        Color[] pixels = readable.GetPixels();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(temporary);
        Object.DestroyImmediate(readable);
        return pixels;
    }

    private static void WritePng(
        string assetPath,
        Color[] pixels,
        int resolution)
    {
        Texture2D texture = new Texture2D(
            resolution,
            resolution,
            TextureFormat.RGBA32,
            false,
            false);
        texture.SetPixels(pixels);
        texture.Apply(false, false);

        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string absolutePath = Path.Combine(
            projectRoot,
            assetPath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        File.WriteAllBytes(absolutePath, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
    }

    private static void ConfigureMaskImporters(
        Dictionary<Material, List<string>> maskPaths)
    {
        foreach (KeyValuePair<Material, List<string>> entry in maskPaths)
        {
            for (int i = 0; i < entry.Value.Count; i++)
            {
                string path = entry.Value[i];
                TextureImporter importer =
                    AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    continue;
                }

                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
                importer.mipmapEnabled = true;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.filterMode = FilterMode.Bilinear;
                importer.maxTextureSize = MaskResolution;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;
                importer.alphaSource = TextureImporterAlphaSource.None;
                importer.SaveAndReimport();
            }
        }
    }

    private static List<CityWindowNightController.WindowMaterialSet>
        CreateOrUpdateNightMaterials(
            CityWindowNightController controller,
            IList<Material> sourceMaterials,
            Dictionary<Material, List<string>> maskPaths)
    {
        List<CityWindowNightController.WindowMaterialSet> sets =
            new List<CityWindowNightController.WindowMaterialSet>();

        for (int materialIndex = 0; materialIndex < sourceMaterials.Count; materialIndex++)
        {
            Material sourceMaterial = sourceMaterials[materialIndex];
            List<Material> variants = new List<Material>();
            List<string> paths = maskPaths[sourceMaterial];

            for (int patternIndex = 0; patternIndex < paths.Count; patternIndex++)
            {
                Texture2D mask = AssetDatabase.LoadAssetAtPath<Texture2D>(
                    paths[patternIndex]);
                if (mask == null)
                {
                    Debug.LogError(
                        "City window night: failed to import mask " +
                        paths[patternIndex] + ".");
                    continue;
                }

                string materialPath =
                    MaterialFolder + "/M_WindowNight_" +
                    SanitizeFileName(sourceMaterial.name) +
                    "_P" + (patternIndex + 1) + ".mat";
                Material nightMaterial = CreateOrUpdateNightMaterial(
                    sourceMaterial,
                    mask,
                    materialPath,
                    controller.EmissionIntensity);
                variants.Add(nightMaterial);
            }

            sets.Add(
                new CityWindowNightController.WindowMaterialSet(
                    sourceMaterial,
                    variants));
        }

        return sets;
    }

    private static Material CreateOrUpdateNightMaterial(
        Material source,
        Texture emissionMask,
        string materialPath,
        float emissionIntensity)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            material = new Material(source);
            AssetDatabase.CreateAsset(material, materialPath);
        }
        else
        {
            EditorUtility.CopySerialized(source, material);
        }

        material.name = Path.GetFileNameWithoutExtension(materialPath);
        material.EnableKeyword("_EMISSION");

        if (material.HasProperty("_EmissionMap"))
        {
            material.SetTexture("_EmissionMap", emissionMask);
        }

        if (material.HasProperty("_EmissionColor"))
        {
            material.SetColor(
                "_EmissionColor",
                Color.white * Mathf.Max(0f, emissionIntensity));
        }

        material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
        material.enableInstancing = true;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Texture GetBaseTexture(Material material)
    {
        if (material.HasProperty("_BaseMap"))
        {
            Texture texture = material.GetTexture("_BaseMap");
            if (texture != null)
            {
                return texture;
            }
        }

        if (material.HasProperty("_MainTex"))
        {
            return material.GetTexture("_MainTex");
        }

        return null;
    }

    private static void SaveControllerScene(CityWindowNightController controller)
    {
        if (controller == null || !controller.gameObject.scene.IsValid())
        {
            return;
        }

        EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
        EditorSceneManager.SaveScene(controller.gameObject.scene);
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = Path.GetDirectoryName(path).Replace("\\", "/");
        string name = Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }

        AssetDatabase.CreateFolder(parent, name);
    }

    private static string SanitizeFileName(string value)
    {
        foreach (char invalid in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalid, '_');
        }

        return value.Replace(' ', '_');
    }

    private static int StableHash(string value)
    {
        unchecked
        {
            const int offsetBasis = (int)2166136261;
            const int prime = 16777619;
            int hash = offsetBasis;

            for (int i = 0; i < value.Length; i++)
            {
                hash ^= value[i];
                hash *= prime;
            }

            return hash;
        }
    }

    private static float Hash01(int value)
    {
        unchecked
        {
            uint hash = (uint)value;
            hash ^= hash >> 16;
            hash *= 0x7feb352d;
            hash ^= hash >> 15;
            hash *= 0x846ca68b;
            hash ^= hash >> 16;
            return (hash & 0x00ffffff) / 16777215f;
        }
    }

}
