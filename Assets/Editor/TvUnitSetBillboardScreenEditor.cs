using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class TvUnitSetBillboardScreenEditor
{
    private const string TargetObjectName = "TvUnitSet5 (1)";
    private const string ScreenObjectName = "AdvertisingScreen_HDR";
    private const string ImageFolder = "Assets/Art/Environment/Billboards/AIImages";
    private const string ScreenMaterialPath = "Assets/materials/City/M_TvBillboard_HDR_Unlit.mat";
    private const string AutoSetupKeyPrefix = "GAME-CBY.TvUnitSet5Billboard.AutoSetup.";

    private static readonly Vector3 ScreenLocalPosition =
        new Vector3(0.0331f, 1.5996f, -0.00002f);

    private static readonly Vector3 ScreenLocalEulerAngles =
        new Vector3(0f, -90f, 0f);

    private static readonly Vector3 ScreenLocalScale =
        new Vector3(1.94f, 1.09f, 1f);

    static TvUnitSetBillboardScreenEditor()
    {
        EditorApplication.delayCall += TryAutomaticSetup;
    }

    [MenuItem("Tools/City/Billboards/Setup TvUnitSet5 (1) Image Screen")]
    public static void SetupTvUnitSet5ImageScreen()
    {
        if (!TrySetupScreen(true, true, out GameObject screen))
        {
            return;
        }

        Selection.activeGameObject = screen;
        EditorGUIUtility.PingObject(screen);
    }

    private static void TryAutomaticSetup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorPrefs.GetBool(AutoSetupKey, false))
        {
            return;
        }

        GameObject target = FindLoadedTarget();
        if (target == null)
        {
            return;
        }

        bool changed = TrySetupScreen(false, true, out GameObject screen);
        if (screen == null)
        {
            return;
        }

        EditorPrefs.SetBool(AutoSetupKey, true);
        if (changed)
        {
            Debug.Log(
                "TV billboard: added a glowing image screen to " + TargetObjectName +
                ". Use Tools > City > Billboards > Setup TvUnitSet5 (1) Image Screen to refresh it.",
                screen);
        }
    }

    private static bool TrySetupScreen(bool forceRefresh, bool saveScene, out GameObject screen)
    {
        screen = null;
        GameObject target = FindLoadedTarget();
        if (target == null)
        {
            Debug.LogError(
                "TV billboard: could not find an active scene object named '" + TargetObjectName + "'.");
            return false;
        }

        Transform existing = target.transform.Find(ScreenObjectName);
        bool created = existing == null;
        if (created)
        {
            screen = GameObject.CreatePrimitive(PrimitiveType.Quad);
            screen.name = ScreenObjectName;
            Undo.RegisterCreatedObjectUndo(screen, "Create TV advertising screen");
            Undo.SetTransformParent(screen.transform, target.transform, "Parent TV advertising screen");

            MeshCollider meshCollider = screen.GetComponent<MeshCollider>();
            if (meshCollider != null)
            {
                Undo.DestroyObjectImmediate(meshCollider);
            }
        }
        else
        {
            screen = existing.gameObject;
        }

        if (!created && !forceRefresh)
        {
            return false;
        }

        Undo.RecordObject(screen, "Configure TV advertising screen");
        Undo.RecordObject(screen.transform, "Align TV advertising screen");
        screen.layer = target.layer;
        screen.transform.localPosition = ScreenLocalPosition;
        screen.transform.localRotation = Quaternion.Euler(ScreenLocalEulerAngles);
        screen.transform.localScale = ScreenLocalScale;

        MeshRenderer renderer = screen.GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            renderer = Undo.AddComponent<MeshRenderer>(screen);
        }

        Material screenMaterial = AssetDatabase.LoadAssetAtPath<Material>(ScreenMaterialPath);
        if (screenMaterial == null)
        {
            Debug.LogError("TV billboard: missing HDR screen material at " + ScreenMaterialPath + ".", screen);
            return false;
        }

        Undo.RecordObject(renderer, "Configure TV advertising renderer");
        renderer.sharedMaterial = screenMaterial;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.lightProbeUsage = LightProbeUsage.Off;
        renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;

        Texture2D image = FindClosestLandscapeImage(ScreenLocalScale.x / ScreenLocalScale.y);
        if (image == null)
        {
            Debug.LogError("TV billboard: no usable image was found in " + ImageFolder + ".", screen);
            return false;
        }

        CityBillboardContentController controller =
            screen.GetComponent<CityBillboardContentController>();
        if (controller == null)
        {
            controller = Undo.AddComponent<CityBillboardContentController>(screen);
        }

        Undo.RecordObject(controller, "Assign TV billboard image");
        controller.Configure(renderer);
        controller.SetBrightness(2.2f, 2.6f);
        controller.SetImage(image);

        EditorUtility.SetDirty(screen);
        EditorUtility.SetDirty(renderer);
        EditorUtility.SetDirty(controller);

        Scene scene = target.scene;
        if (scene.IsValid() && scene.isLoaded)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            if (saveScene && !string.IsNullOrEmpty(scene.path))
            {
                EditorSceneManager.SaveScene(scene);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log(
            "TV billboard: " + TargetObjectName + " now uses image '" + image.name +
            "' with HDR brightness 2.2.",
            screen);
        return true;
    }

    private static GameObject FindLoadedTarget()
    {
        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform candidate = transforms[i];
            if (candidate == null || candidate.name != TargetObjectName)
            {
                continue;
            }

            Scene scene = candidate.gameObject.scene;
            if (scene.IsValid() && scene.isLoaded && !EditorUtility.IsPersistent(candidate))
            {
                return candidate.gameObject;
            }
        }

        return null;
    }

    private static Texture2D FindClosestLandscapeImage(float targetAspect)
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { ImageFolder });
        Texture2D bestImage = null;
        string bestPath = null;
        float bestError = float.MaxValue;

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null || texture.width <= 0 || texture.height <= 0)
            {
                continue;
            }

            float textureAspect = texture.width / (float)texture.height;
            float error = Mathf.Abs(Mathf.Log(textureAspect / targetAspect));
            bool isBetter = error < bestError - 0.0001f;
            bool isStableTie = Mathf.Abs(error - bestError) <= 0.0001f &&
                               string.Compare(path, bestPath, StringComparison.Ordinal) < 0;
            if (!isBetter && !isStableTie)
            {
                continue;
            }

            bestImage = texture;
            bestPath = path;
            bestError = error;
        }

        return bestImage;
    }

    private static string AutoSetupKey
    {
        get { return AutoSetupKeyPrefix + Application.dataPath; }
    }
}
