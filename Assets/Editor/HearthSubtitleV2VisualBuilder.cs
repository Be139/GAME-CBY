#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class HearthSubtitleV2VisualBuilder
{
    public const string PrefabPath =
        "Assets/Prefabs/UI/HearthSubtitle/V2/HearthSubtitleVisualCanvas_V2.prefab";

    private const string CanvasName = "HearthSubtitleVisualCanvas_V2";
    private const string VisualRootName = "VisualRoot";
    private const string DialogueFrameSpritePath =
        "Assets/UI/HEARTH/GeneratedParts/Common/HUD_Common_DialogueFrame.png";
    private const string SpeakerTabSpritePath =
        "Assets/UI/HEARTH/GeneratedParts/Common/HUD_Common_SpeakerTabFrame_9Slice.png";

    [MenuItem(HearthLegacyToolGuard.MenuRoot + "Subtitle Builder/Apply Production Profile Defaults")]
    public static void ApplyProductionProfileDefaults()
    {
        if (!HearthLegacyToolGuard.Confirm(
                "Apply Subtitle Production Profile Defaults",
                "the shared subtitle style asset"))
        {
            return;
        }

        HearthSubtitleStyleProfile profile =
            HearthDialoguePresentationBinder.EnsureSharedProfile();
        Undo.RecordObject(profile, "Apply Hearth subtitle production defaults");
        profile.ApplyProductionDefaults();
        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();
        Debug.Log(
            "[HearthSubtitleV2VisualBuilder] Applied the approved production values " +
            "to the shared subtitle profile. Rebuilding the prefab does not repeat " +
            "or overwrite this profile operation.");
    }

    [MenuItem(HearthLegacyToolGuard.MenuRoot + "Subtitle Builder/Build Visual Prefab")]
    public static void BuildVisualPrefab()
    {
        if (!HearthLegacyToolGuard.Confirm(
                "Build Subtitle Visual Prefab",
                "the canonical V2 subtitle Prefab"))
        {
            return;
        }

        BuildVisualPrefabAsset();
        Debug.Log(
            "[HearthSubtitleV2VisualBuilder] Built explicit subtitle VisualRoot prefab at " +
            PrefabPath +
            ".");
    }

    [MenuItem(HearthLegacyToolGuard.MenuRoot + "Subtitle Builder/Bind Open Scene Players")]
    public static void BindOpenScenePlayers()
    {
        if (!HearthLegacyToolGuard.Confirm(
                "Bind Open Scene Subtitle Players",
                "all subtitle players in the active scene"))
        {
            return;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            prefab = BuildVisualPrefabAsset();
        }

        int boundCount = BindPlayers(prefab);
        Debug.Log(
            "[HearthSubtitleV2VisualBuilder] Bound explicit subtitle VisualRoot to " +
            boundCount +
            " player(s). The open scene is dirty but was not auto-saved.");
    }

    [MenuItem(HearthLegacyToolGuard.MenuRoot + "Subtitle Builder/Build And Bind Open Scene")]
    public static void BuildAndBindOpenScene()
    {
        if (!HearthLegacyToolGuard.Confirm(
                "Build And Bind Subtitle Visuals",
                "the canonical subtitle Prefab and all subtitle players in the active scene"))
        {
            return;
        }

        GameObject prefab = BuildVisualPrefabAsset();
        int boundCount = BindPlayers(prefab);
        ValidateOpenScene();
        Debug.Log(
            "[HearthSubtitleV2VisualBuilder] Build and bind complete for " +
            boundCount +
            " player(s). Review the Game view, then save the scene manually.");
    }

    [MenuItem("Tools/Hearth/Production UI/Validate Subtitle View")]
    public static void ValidateOpenScene()
    {
        List<string> issues = new List<string>();
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab == null)
        {
            issues.Add("The explicit subtitle VisualRoot prefab is missing.");
        }

        MinLoopSubtitlePlayer[] players =
            Object.FindObjectsOfType<MinLoopSubtitlePlayer>(true);
        if (players.Length == 0)
        {
            issues.Add("The open scene has no MinLoopSubtitlePlayer.");
        }

        for (int i = 0; i < players.Length; i++)
        {
            ValidatePlayer(players[i], issues);
        }

        if (issues.Count == 0)
        {
            Debug.Log(
                "[HearthSubtitleV2VisualBuilder] Validation passed: every subtitle player " +
                "has an explicit VisualRoot, fixed-size TMP text, overflow-safe layout, " +
                "and fallback creation disabled.");
            return;
        }

        Debug.LogError(
            "[HearthSubtitleV2VisualBuilder] Validation found " +
            issues.Count +
            " issue(s):\n- " +
            string.Join("\n- ", issues));
    }

    private static GameObject BuildVisualPrefabAsset()
    {
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Prefabs/UI");
        EnsureFolder("Assets/Prefabs/UI/HearthSubtitle");
        EnsureFolder("Assets/Prefabs/UI/HearthSubtitle/V2");

        GameObject canvasObject = new GameObject(
            CanvasName,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler));
        try
        {
            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            Stretch(canvasRect);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.pixelPerfect = false;
            canvas.sortingOrder = 8500;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100f;

            GameObject visualObject = new GameObject(
                VisualRootName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image),
                typeof(CanvasGroup));
            visualObject.transform.SetParent(canvasObject.transform, false);
            RectTransform visualRect = visualObject.GetComponent<RectTransform>();
            Stretch(visualRect);

            Image rootImage = visualObject.GetComponent<Image>();
            rootImage.color = Color.clear;
            rootImage.raycastTarget = false;

            CanvasGroup group = visualObject.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
            group.ignoreParentGroups = false;

            Image backdrop = CreateImage(
                visualObject.transform,
                "Backdrop",
                new Color(0.035f, 0.063f, 0.11f, 0.9f));
            backdrop.sprite =
                AssetDatabase.LoadAssetAtPath<Sprite>(DialogueFrameSpritePath);
            backdrop.type = Image.Type.Simple;
            backdrop.preserveAspect = false;

            CreateImage(visualObject.transform, "AccentRule", Color.clear);
            Image speakerTab = CreateImage(
                visualObject.transform,
                "SpeakerTab",
                new Color(0.37f, 0.47f, 0.58f, 0.95f));
            speakerTab.sprite =
                AssetDatabase.LoadAssetAtPath<Sprite>(SpeakerTabSpritePath);
            speakerTab.type = Image.Type.Sliced;
            speakerTab.pixelsPerUnitMultiplier = 1f;

            CreateText(visualObject.transform, "Speaker", 22f, FontStyles.Bold);
            CreateText(visualObject.transform, "Body", 28f, FontStyles.Normal);

            GameObject saved =
                PrefabUtility.SaveAsPrefabAsset(canvasObject, PrefabPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return saved;
        }
        finally
        {
            Object.DestroyImmediate(canvasObject);
        }
    }

    private static int BindPlayers(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError(
                "[HearthSubtitleV2VisualBuilder] Cannot bind because the prefab is missing.");
            return 0;
        }

        MinLoopSubtitlePlayer[] players =
            Object.FindObjectsOfType<MinLoopSubtitlePlayer>(true);
        int boundCount = 0;
        for (int i = 0; i < players.Length; i++)
        {
            MinLoopSubtitlePlayer player = players[i];
            if (player == null)
            {
                continue;
            }

            SerializedObject playerSo = new SerializedObject(player);
            SerializedProperty previousRootProperty =
                playerSo.FindProperty("visualRoot") ??
                playerSo.FindProperty("subtitlePanel");
            GameObject previousRoot =
                previousRootProperty != null
                    ? previousRootProperty.objectReferenceValue as GameObject
                    : null;

            Transform oldGenerated = player.transform.Find(CanvasName);
            if (oldGenerated != null)
            {
                Undo.DestroyObjectImmediate(oldGenerated.gameObject);
            }

            GameObject instance =
                PrefabUtility.InstantiatePrefab(prefab, player.transform) as GameObject;
            if (instance == null)
            {
                Debug.LogError(
                    "[HearthSubtitleV2VisualBuilder] Failed to instantiate VisualRoot for " +
                    player.name +
                    ".");
                continue;
            }

            instance.name = CanvasName;
            Undo.RegisterCreatedObjectUndo(instance, "Bind Hearth subtitle VisualRoot");
            Transform visual = instance.transform.Find(VisualRootName);
            if (visual == null)
            {
                Undo.DestroyObjectImmediate(instance);
                Debug.LogError(
                    "[HearthSubtitleV2VisualBuilder] Prefab has no " +
                    VisualRootName +
                    " child.");
                continue;
            }

            TMP_Text speaker = visual.Find("Speaker")?.GetComponent<TMP_Text>();
            TMP_Text body = visual.Find("Body")?.GetComponent<TMP_Text>();
            Image backdrop = visual.Find("Backdrop")?.GetComponent<Image>();
            Image accent = visual.Find("AccentRule")?.GetComponent<Image>();
            Image speakerTab = visual.Find("SpeakerTab")?.GetComponent<Image>();
            CanvasGroup canvasGroup = visual.GetComponent<CanvasGroup>();

            playerSo.Update();
            SetObject(playerSo, "visualRoot", visual.gameObject);
            SetObject(playerSo, "layoutRoot", visual.GetComponent<RectTransform>());
            SetObject(playerSo, "backdropImage", backdrop);
            SetObject(playerSo, "accentRuleImage", accent);
            SetObject(playerSo, "speakerTabImage", speakerTab);
            SetObject(playerSo, "subtitlePanel", visual.gameObject);
            SetObject(playerSo, "speakerText", speaker);
            SetObject(playerSo, "bodyText", body);
            SetObject(playerSo, "canvasGroup", canvasGroup);
            SetBool(playerSo, "createFallbackUI", false);
            playerSo.ApplyModifiedProperties();
            EditorUtility.SetDirty(player);

            if (previousRoot != null &&
                previousRoot != visual.gameObject &&
                previousRoot.scene.IsValid())
            {
                Undo.RecordObject(previousRoot, "Disable legacy subtitle visual");
                previousRoot.SetActive(false);
                EditorUtility.SetDirty(previousRoot);
            }

            visual.gameObject.SetActive(false);
            boundCount++;
        }

        if (boundCount > 0)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        return boundCount;
    }

    private static void ValidatePlayer(
        MinLoopSubtitlePlayer player,
        List<string> issues)
    {
        SerializedObject so = new SerializedObject(player);
        GameObject visual =
            GetObject<GameObject>(so, "visualRoot") ??
            GetObject<GameObject>(so, "subtitlePanel");
        TMP_Text speaker = GetObject<TMP_Text>(so, "speakerText");
        TMP_Text body = GetObject<TMP_Text>(so, "bodyText");
        CanvasGroup group = GetObject<CanvasGroup>(so, "canvasGroup");
        Image speakerTab = GetObject<Image>(so, "speakerTabImage");
        bool fallback = GetBool(so, "createFallbackUI", true);

        string label = player.name;
        if (visual == null) issues.Add(label + " has no explicit VisualRoot.");
        if (speaker == null) issues.Add(label + " has no speaker TMP binding.");
        if (body == null) issues.Add(label + " has no body TMP binding.");
        if (group == null) issues.Add(label + " has no CanvasGroup binding.");
        if (speakerTab == null) issues.Add(label + " has no speaker-tab Image binding.");
        if (fallback) issues.Add(label + " still permits runtime fallback UI creation.");

        if (speaker != null)
        {
            if (speaker.enableAutoSizing)
                issues.Add(label + " speaker TMP still enables auto sizing.");
            if (!Mathf.Approximately(speaker.fontSize, 22f))
                issues.Add(label + " speaker TMP is not 22 px.");
            if (speaker.overflowMode == TextOverflowModes.Truncate)
                issues.Add(label + " speaker TMP still truncates.");
        }

        if (body != null)
        {
            if (body.enableAutoSizing)
                issues.Add(label + " body TMP still enables auto sizing.");
            if (body.overflowMode == TextOverflowModes.Truncate)
                issues.Add(label + " body TMP still truncates.");
            if (body.maxVisibleLines < 8)
                issues.Add(label + " body TMP does not permit enough lines for long dialogue.");
        }
    }

    private static Image CreateImage(
        Transform parent,
        string objectName,
        Color color)
    {
        GameObject imageObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static TMP_Text CreateText(
        Transform parent,
        string objectName,
        float fontSize,
        FontStyles style)
    {
        GameObject textObject =
            new GameObject(objectName, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null)
        {
            text.font = TMP_Settings.defaultFontAsset;
        }

        text.text = string.Empty;
        text.fontSize = fontSize;
        text.fontSizeMin = fontSize;
        text.fontSizeMax = fontSize;
        text.enableAutoSizing = false;
        text.fontStyle = style;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.maxVisibleLines = 999;
        text.overflowMode = TextOverflowModes.Overflow;
        text.color = new Color(0.84f, 0.9f, 0.96f, 1f);
        text.raycastTarget = false;
        return text;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        int split = path.LastIndexOf('/');
        string parent = path.Substring(0, split);
        string name = path.Substring(split + 1);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }

    private static void SetObject(
        SerializedObject so,
        string field,
        Object value)
    {
        SerializedProperty property = so.FindProperty(field);
        if (property != null)
        {
            property.objectReferenceValue = value;
        }
    }

    private static void SetBool(
        SerializedObject so,
        string field,
        bool value)
    {
        SerializedProperty property = so.FindProperty(field);
        if (property != null)
        {
            property.boolValue = value;
        }
    }

    private static T GetObject<T>(
        SerializedObject so,
        string field)
        where T : Object
    {
        SerializedProperty property = so.FindProperty(field);
        return property != null ? property.objectReferenceValue as T : null;
    }

    private static bool GetBool(
        SerializedObject so,
        string field,
        bool fallback)
    {
        SerializedProperty property = so.FindProperty(field);
        return property != null ? property.boolValue : fallback;
    }
}
#endif
