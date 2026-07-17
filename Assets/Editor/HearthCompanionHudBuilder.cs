#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class HearthCompanionHudBuilder
{
    private const int ReferenceWidth = 1920;
    private const int ReferenceHeight = 1080;
    private const float PptToCanvasScale = ReferenceWidth / 1440f;
    private const string FrameSpritePath = "Assets/Art/UI/HearthHud/Companion/CompanionRobotFrame.png";
    private const string DataFolder = "Assets/Data/HearthHud/Companion";
    private const string PrefabFolder = "Assets/Prefabs/UI/HearthHud/Companion";
    private const string RootPrefabPath = PrefabFolder + "/HearthCompanionHudRoot.prefab";

    [MenuItem("Tools/Hearth/HUD/Rebuild Companion Unit HUD Prefab")]
    public static void RebuildCompanionHudPrefab()
    {
        EnsureFolders();
        ConfigureFrameImport();
        AssetDatabase.Refresh();

        HearthCompanionHudSceneData[] sceneAssets = EnsureSceneDataAssets(false);
        GameObject root = BuildRoot(sceneAssets);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, RootPrefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = prefab;
        Debug.Log("[HearthCompanionHudBuilder] Rebuilt companion unit HUD prefab: " + RootPrefabPath);
    }

    [MenuItem("Tools/Hearth/HUD/Rebuild And Apply Companion Unit HUD To Scene")]
    public static void RebuildAndApplyToOpenScene()
    {
        RebuildCompanionHudPrefab();

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RootPrefabPath);
        if (prefab == null)
        {
            Debug.LogError("[HearthCompanionHudBuilder] Missing prefab: " + RootPrefabPath);
            return;
        }

        GameObject existing = GameObject.Find("HearthCompanionHudRoot");
        if (existing != null)
        {
            Object.DestroyImmediate(existing);
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = "HearthCompanionHudRoot";

        RectTransform rect = instance.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(ReferenceWidth, ReferenceHeight);
            rect.anchoredPosition = new Vector2(ReferenceWidth * 0.5f, ReferenceHeight * 0.5f);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[HearthCompanionHudBuilder] Applied HearthCompanionHudRoot to the open scene.");
    }

    [MenuItem("Tools/Hearth/HUD/Regenerate Companion Unit Scene Data Defaults")]
    public static void RegenerateSceneDataDefaults()
    {
        EnsureFolders();
        EnsureSceneDataAssets(true);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[HearthCompanionHudBuilder] Regenerated default companion unit scene data.");
    }

    [MenuItem("Tools/Hearth/HUD/Apply Companion Special Effect Alignment")]
    public static void ApplyCompanionSpecialEffectAlignment()
    {
        GameObject prefabContents = null;
        try
        {
            prefabContents = PrefabUtility.LoadPrefabContents(RootPrefabPath);
            HearthCompanionSpecialEffectsView prefabView = prefabContents.GetComponentInChildren<HearthCompanionSpecialEffectsView>(true);
            if (prefabView != null)
            {
                prefabView.ApplyLayoutNow();
                EditorUtility.SetDirty(prefabView);
                PrefabUtility.SaveAsPrefabAsset(prefabContents, RootPrefabPath);
            }
        }
        finally
        {
            if (prefabContents != null)
            {
                PrefabUtility.UnloadPrefabContents(prefabContents);
            }
        }

        HearthCompanionSpecialEffectsView sceneView = Object.FindObjectOfType<HearthCompanionSpecialEffectsView>(true);
        if (sceneView != null)
        {
            Undo.RecordObject(sceneView, "Align companion shutdown UI");
            sceneView.ApplyLayoutNow();
            EditorUtility.SetDirty(sceneView);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[HearthCompanionHudBuilder] Centered the companion shutdown, deep-sleep, and warning text in both the prefab and open scene.");
    }

    [MenuItem("Tools/Hearth/HUD/Regenerate 17F03 Companion Scene Data Defaults")]
    public static void Regenerate17F03SceneDataDefaults()
    {
        EnsureFolders();
        SceneDefault[] defaults = CreateDefaults();
        for (int i = 0; i < defaults.Length; i++)
        {
            SceneDefault item = defaults[i];
            if (!item.AssetName.Contains("17F03"))
            {
                continue;
            }

            string assetPath = DataFolder + "/" + item.AssetName + ".asset";
            HearthCompanionHudSceneData asset = AssetDatabase.LoadAssetAtPath<HearthCompanionHudSceneData>(assetPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<HearthCompanionHudSceneData>();
                AssetDatabase.CreateAsset(asset, assetPath);
            }

            item.Apply(asset);
            EditorUtility.SetDirty(asset);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[HearthCompanionHudBuilder] Regenerated 17F03 companion scene data defaults only.");
    }

    private static GameObject BuildRoot(HearthCompanionHudSceneData[] sceneAssets)
    {
        GameObject root = new GameObject(
            "HearthCompanionHudRoot",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(CanvasGroup),
            typeof(AudioSource),
            typeof(HearthCompanionHudExclusiveMode),
            typeof(HearthCompanionHudController),
            typeof(HearthCompanionHudPreviewInput),
            typeof(HearthCompanionHudFlowBinder));

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(ReferenceWidth, ReferenceHeight);
        rootRect.anchoredPosition = new Vector2(ReferenceWidth * 0.5f, ReferenceHeight * 0.5f);

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 6200;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        CanvasGroup rootGroup = root.GetComponent<CanvasGroup>();
        AudioSource audioSource = root.GetComponent<AudioSource>();

        Transform frameLayer = CreateLayer(root.transform, "FrameLayer");
        Transform persistentLayer = CreateLayer(root.transform, "PersistentInfoLayer");
        Transform triggerLayer = CreateLayer(root.transform, "TimedCardLayer");
        Transform interactionLayer = CreateLayer(root.transform, "InteractionLayer");
        Transform projectionLayer = CreateLayer(root.transform, "ProjectionLayer");
        Transform specialLayer = CreateLayer(root.transform, "SpecialEffectsLayer");

        Sprite frameSprite = AssetDatabase.LoadAssetAtPath<Sprite>(FrameSpritePath);
        Image frameImage = CreateImage(frameLayer, "CompanionRobotFrame", new Rect(0f, 0f, ReferenceWidth, ReferenceHeight), Color.white);
        frameImage.sprite = frameSprite;
        frameImage.preserveAspect = true;
        frameImage.raycastTarget = false;

        HearthCompanionStatusPanelView status = null;
        HearthCompanionDecisionPanelView decision = BuildDecisionPanel(persistentLayer);
        HearthCompanionDataStreamView stream = BuildDataStream(persistentLayer);
        TMP_Text modeLabel = CreateText(persistentLayer, "ModeLabelText", string.Empty, PptRect(471.5f, 733.5f, 546.7f, 18f), 15f, new Color(0.55f, 0.84f, 1f, 0.94f), FontStyles.Bold, TextAlignmentOptions.Center);
        TMP_Text centerMessage = CreateText(persistentLayer, "CenterMessageText", string.Empty, PptRect(411.8f, 366.3f, 664.7f, 64f), 26f, new Color(0.6f, 0.82f, 1f, 0.72f), FontStyles.Bold, TextAlignmentOptions.Center);

        HearthCompanionTriggerCardView trigger = BuildTriggerCard(triggerLayer);
        HearthCompanionHoldPrompt holdPrompt = BuildHoldPrompt(interactionLayer);
        HearthCompanionProjectionPanelView projection = BuildProjectionPanel(projectionLayer);
        HearthCompanionDirectionGuideView direction = BuildDirectionGuide(interactionLayer);
        HearthCompanionSpecialEffectsView special = BuildSpecialEffects(specialLayer);

        HearthCompanionHudController controller = root.GetComponent<HearthCompanionHudController>();
        controller.Configure(sceneAssets, rootGroup, status, decision, stream, trigger, holdPrompt, projection, direction, special, modeLabel, centerMessage, audioSource);

        HearthCompanionHudPreviewInput preview = root.GetComponent<HearthCompanionHudPreviewInput>();
        preview.SetPreviewInputEnabled(true);

        return root;
    }

    private static HearthCompanionStatusPanelView BuildStatusPanel(Transform parent)
    {
        GameObject panel = CreateTransparentGroup(parent, "StatusPanel");
        HearthCompanionStatusPanelView view = panel.AddComponent<HearthCompanionStatusPanelView>();
        TMP_Text title = CreateText(panel.transform, "StatusTitleText", string.Empty, PptRect(90.7f, 91.5f, 330f, 20f), 14f, Color.white, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        TMP_Text rows = CreateText(panel.transform, "StatusRowsText", string.Empty, PptRect(76.5f, 126.6f, 360f, 174f), 17f, new Color(0.78f, 0.9f, 1f, 0.9f), FontStyles.Normal, TextAlignmentOptions.TopLeft);
        TMP_Text footer = CreateText(panel.transform, "StatusFooterText", string.Empty, PptRect(76.5f, 247.8f, 358f, 22f), 13f, new Color(0.62f, 0.86f, 1f, 0.8f), FontStyles.Bold, TextAlignmentOptions.TopLeft);
        Image accent = CreateImage(panel.transform, "StatusAccent", PptRect(67f, 93f, 3f, 178f), new Color(0.45f, 0.85f, 1f, 0.8f));
        view.Configure(title, rows, footer, accent);
        return view;
    }

    private static HearthCompanionDecisionPanelView BuildDecisionPanel(Transform parent)
    {
        GameObject panel = CreateTransparentGroup(parent, "DecisionPanel");
        CanvasGroup group = panel.GetComponent<CanvasGroup>();
        HearthCompanionDecisionPanelView view = panel.AddComponent<HearthCompanionDecisionPanelView>();
        TMP_Text kicker = CreateText(panel.transform, "DecisionKickerText", string.Empty, PptRect(1031.2f, 92.1f, 310f, 22f), 13f, Color.white, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        TMP_Text title = CreateText(panel.transform, "DecisionTitleText", string.Empty, PptRect(1027.5f, 127.3f, 360f, 30f), 18f, new Color(0.92f, 0.98f, 1f, 0.96f), FontStyles.Bold, TextAlignmentOptions.TopLeft);
        TMP_Text body = CreateText(panel.transform, "DecisionBodyText", string.Empty, PptRect(1027.5f, 160.3f, 340f, 70f), 14f, new Color(0.72f, 0.84f, 0.92f, 0.9f), FontStyles.Normal, TextAlignmentOptions.TopLeft);
        Image accent = CreateImage(panel.transform, "DecisionAccent", PptRect(1018f, 92f, 3f, 118f), new Color(0.45f, 0.85f, 1f, 0.8f));
        view.Configure(group, kicker, title, body, accent);
        return view;
    }

    private static HearthCompanionDataStreamView BuildDataStream(Transform parent)
    {
        GameObject root = new GameObject("DataStreamView", typeof(RectTransform), typeof(HearthCompanionDataStreamView));
        root.transform.SetParent(parent, false);
        SetTopLeft(root.GetComponent<RectTransform>(), PptRect(63f, 579.8f, 300f, 160f));
        TMP_Text title = CreateText(root.transform, "DataStreamTitleText", string.Empty, ScaleRect(0f, 0f, 260f, 20f), 13f, new Color(0.45f, 0.8f, 1f, 0.82f), FontStyles.Normal, TextAlignmentOptions.TopLeft);
        TMP_Text body = CreateText(root.transform, "DataStreamLinesText", string.Empty, ScaleRect(0f, 22f, 280f, 130f), 13f, new Color(0.64f, 0.82f, 0.94f, 0.82f), FontStyles.Normal, TextAlignmentOptions.TopLeft);
        HearthCompanionDataStreamView view = root.GetComponent<HearthCompanionDataStreamView>();
        view.Configure(title, body);
        return view;
    }

    private static HearthCompanionTriggerCardView BuildTriggerCard(Transform parent)
    {
        GameObject panel = CreateTransparentGroup(parent, "TriggerCardView");
        SetTopLeft(panel.GetComponent<RectTransform>(), PptRect(72f, 75.5f, 410f, 214f));
        HearthCompanionTriggerCardView view = panel.AddComponent<HearthCompanionTriggerCardView>();
        CanvasGroup group = panel.GetComponent<CanvasGroup>();
        TMP_Text title = CreateText(panel.transform, "TriggerCardTitleText", string.Empty, ScaleRect(20f, 16f, 360f, 22f), 14f, Color.white, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        TMP_Text body = CreateText(panel.transform, "TriggerCardBodyText", string.Empty, ScaleRect(20f, 46f, 360f, 150f), 14f, new Color(0.8f, 0.92f, 1f, 0.92f), FontStyles.Normal, TextAlignmentOptions.TopLeft);
        Image accent = CreateImage(panel.transform, "TriggerCardAccent", ScaleRect(10f, 16f, 3f, 178f), new Color(0.45f, 0.85f, 1f, 0.8f));
        view.Configure(group, title, body, accent);
        return view;
    }

    private static HearthCompanionHoldPrompt BuildHoldPrompt(Transform parent)
    {
        GameObject root = new GameObject("HoldPrompt", typeof(RectTransform), typeof(CanvasGroup), typeof(HearthCompanionHoldPrompt));
        root.transform.SetParent(parent, false);
        Stretch(root.GetComponent<RectTransform>());

        CreatePanel(root.transform, "HoldPromptBox", PptRect(484f, 562f, 474f, 52f), new Color(0.01f, 0.05f, 0.07f, 0.72f), new Color(0.52f, 0.86f, 1f, 0.48f));
        HearthCompanionHoldPrompt prompt = root.GetComponent<HearthCompanionHoldPrompt>();
        CanvasGroup group = root.GetComponent<CanvasGroup>();
        TMP_Text promptText = CreateText(root.transform, "HoldPromptText", string.Empty, PptRect(511.7f, 575.4f, 416.7f, 25.5f), 15f, new Color(0.88f, 0.97f, 1f, 0.96f), FontStyles.Bold, TextAlignmentOptions.Center);
        TMP_Text keyText = CreateText(root.transform, "HoldKeyText", "E", PptRect(652.9f, 623.5f, 18f, 15f), 14f, new Color(0.9f, 1f, 1f, 1f), FontStyles.Bold, TextAlignmentOptions.Center);
        TMP_Text progressText = CreateText(root.transform, "HoldProgressText", "HOLD TO ACT", PptRect(674f, 622.7f, 170f, 16.5f), 12f, new Color(0.7f, 0.9f, 1f, 0.9f), FontStyles.Bold, TextAlignmentOptions.Left);
        Image progressBack = CreateImage(root.transform, "HoldProgressBack", PptRect(512f, 639f, 416f, 4f), new Color(0.12f, 0.25f, 0.32f, 0.9f));
        progressBack.raycastTarget = false;
        Image progressFill = CreateImage(root.transform, "HoldProgressFill", PptRect(512f, 639f, 416f, 4f), new Color(0.42f, 0.88f, 1f, 0.9f));
        progressFill.type = Image.Type.Filled;
        progressFill.fillMethod = Image.FillMethod.Horizontal;
        progressFill.fillOrigin = 0;
        progressFill.fillAmount = 0f;
        prompt.Configure(null, group, promptText, keyText, progressText, progressFill);
        return prompt;
    }

    private static HearthCompanionProjectionPanelView BuildProjectionPanel(Transform parent)
    {
        GameObject panel = CreatePanel(parent, "ProjectionPanel", PptRect(530f, 258f, 368f, 302f), new Color(0.01f, 0.05f, 0.08f, 0.82f), new Color(0.38f, 0.74f, 1f, 0.44f));
        HearthCompanionProjectionPanelView view = panel.AddComponent<HearthCompanionProjectionPanelView>();
        CanvasGroup group = panel.GetComponent<CanvasGroup>();
        TMP_Text title = CreateText(panel.transform, "ProjectionTitleText", string.Empty, ScaleRect(28f, 24f, 300f, 28f), 15f, Color.white, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        TMP_Text body = CreateText(panel.transform, "ProjectionBodyText", string.Empty, ScaleRect(28f, 58f, 312f, 210f), 13f, new Color(0.74f, 0.9f, 1f, 0.92f), FontStyles.Normal, TextAlignmentOptions.TopLeft);
        Image accent = CreateImage(panel.transform, "ProjectionAccent", ScaleRect(14f, 24f, 3f, 248f), new Color(0.45f, 0.85f, 1f, 0.8f));
        view.Configure(group, title, body, accent);
        return view;
    }

    private static HearthCompanionDirectionGuideView BuildDirectionGuide(Transform parent)
    {
        GameObject root = new GameObject("DirectionGuide", typeof(RectTransform), typeof(CanvasGroup), typeof(HearthCompanionDirectionGuideView));
        root.transform.SetParent(parent, false);
        Stretch(root.GetComponent<RectTransform>());
        CanvasGroup group = root.GetComponent<CanvasGroup>();
        TMP_Text label = CreateText(root.transform, "DirectionGuideText", string.Empty, PptRect(520f, 520f, 400f, 32f), 17f, new Color(0.5f, 0.9f, 1f, 0.9f), FontStyles.Bold, TextAlignmentOptions.Center);
        Image marker = CreateImage(root.transform, "DirectionGuideMarker", PptRect(700f, 490f, 40f, 40f), new Color(0.5f, 0.9f, 1f, 0.32f));
        AddBorder(marker.transform, new Rect(0f, 0f, 40f, 40f), new Color(0.5f, 0.9f, 1f, 0.86f), 2f);
        HearthCompanionDirectionGuideView view = root.GetComponent<HearthCompanionDirectionGuideView>();
        view.Configure(group, label, marker, marker.GetComponent<RectTransform>());
        return view;
    }

    private static HearthCompanionSpecialEffectsView BuildSpecialEffects(Transform parent)
    {
        GameObject root = new GameObject("SpecialEffectsView", typeof(RectTransform), typeof(CanvasGroup), typeof(HearthCompanionSpecialEffectsView));
        root.transform.SetParent(parent, false);
        Stretch(root.GetComponent<RectTransform>());
        CanvasGroup group = root.GetComponent<CanvasGroup>();
        Image overlay = CreateImage(root.transform, "BlackOverlay", new Rect(0f, 0f, ReferenceWidth, ReferenceHeight), Color.black);
        TMP_Text title = CreateText(root.transform, "SpecialTitleText", string.Empty, PptRect(390f, 366.3f, 660f, 48f), 32f, new Color(0.46f, 0.88f, 1f, 1f), FontStyles.Bold, TextAlignmentOptions.Center);
        TMP_Text body = CreateText(root.transform, "SpecialBodyText", string.Empty, PptRect(390f, 430f, 660f, 120f), 18f, new Color(0.78f, 0.9f, 1f, 0.9f), FontStyles.Normal, TextAlignmentOptions.Center);
        TMP_Text status = CreateText(root.transform, "SpecialStatusText", string.Empty, PptRect(465f, 520f, 510f, 32f), 14f, new Color(0.46f, 0.88f, 1f, 0.9f), FontStyles.Bold, TextAlignmentOptions.Center);
        Image pulse = CreateImage(root.transform, "AudioPulseLine", PptRect(570f, 470f, 300f, 3f), new Color(0.46f, 0.88f, 1f, 0.5f));
        HearthCompanionSpecialEffectsView view = root.GetComponent<HearthCompanionSpecialEffectsView>();
        view.Configure(group, overlay, title, body, status, pulse);
        return view;
    }

    private static HearthCompanionHudSceneData[] EnsureSceneDataAssets(bool overwrite)
    {
        List<HearthCompanionHudSceneData> assets = new List<HearthCompanionHudSceneData>();
        SceneDefault[] defaults = CreateDefaults();

        for (int i = 0; i < defaults.Length; i++)
        {
            SceneDefault item = defaults[i];
            string assetPath = DataFolder + "/" + item.AssetName + ".asset";
            HearthCompanionHudSceneData asset = AssetDatabase.LoadAssetAtPath<HearthCompanionHudSceneData>(assetPath);
            bool shouldApplyDefaults = overwrite;
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<HearthCompanionHudSceneData>();
                AssetDatabase.CreateAsset(asset, assetPath);
                shouldApplyDefaults = true;
            }

            if (shouldApplyDefaults)
            {
                item.Apply(asset);
                EditorUtility.SetDirty(asset);
            }

            assets.Add(asset);
        }

        return assets.ToArray();
    }

    private static SceneDefault[] CreateDefaults()
    {
        Color blue = new Color(0.48f, 0.84f, 1f, 1f);
        Color orange = new Color(1f, 0.45f, 0.26f, 1f);

        return new SceneDefault[]
        {
            new SceneDefault("17F01_01", 1, "17F-01", HearthCompanionHudTemplate.Standard, HearthCompanionSpecialEffect.None, "COMPANION UNIT - FIRST PERSON - MONITORING MODE", blue,
                "SUBJECT - MONITORING", Lines(M("Time", "02:47"), M("State", "REM Anomaly"), M("Heart", "62 -> 89 ^"), M("Pupil", "Rapid")), "ASSESSMENT - NIGHTMARE - STAGE II",
                "Initiate Soothing Sequence", "Early nightmare signs. Protocol: presence + low-band reassurance.", "//monitor bus - streaming",
                Arr("0x47A2 REM_03", "0x47A3 HR_89bpm", "0x47A4 cmo_anx_72", "0x47A5 parent_sleep", "0x47A6 room_lux_02", "0x47A7 ack_soothe ->"),
                true, "[ Approach Bedside - Watch Over Subject ]", false, "", "", "", "", false, "", "", false, "", "", "", 1.5f),

            new SceneDefault("17F01_02", 2, "17F-01", HearthCompanionHudTemplate.Standard, HearthCompanionSpecialEffect.None, "COMPANION UNIT - FIRST PERSON - MONITORING MODE", blue,
                "STATUS CHANGE", Lines(M("Subject", ""), M("Emotion", "Fear - anxiety"), M("Action", "Head turn -> door"), M("Heart", "89 -> 71 v"), M("Emotion", "Stabilizing")), "SOOTHING EFFECTIVE",
                "Internal Intervention", "Cross-room disturbance would impact parent next-day work.", "//monitor bus - streaming",
                Arr("0x47B1 subject_AWAKE", "0x47B2 emo_fear_HIGH", "0x47B3 head_turn_door", "0x47B4 vocal_detected", "0x47B5 parent_sleep_23min", "0x47B6 intervene_DECISION"),
                true, "[ \"...\" ]", false, "", "", "", "", false, "", "", false, "", "", "", 1.5f),

            new SceneDefault("17F01_03", 3, "17F-01", HearthCompanionHudTemplate.Standard, HearthCompanionSpecialEffect.None, "COMPANION UNIT - FIRST PERSON - STANDBY MODE", blue,
                "MORNING SYNC", Lines(M("Last night data", "Uploaded"), M("Status", "Awaiting confirm"), M("FESI", "Stability maintained"), M("Event archive", "Complete")), "PARENT DIALOGUE CONFIRMED",
                "Standby - Observe Confirmation", "Unit has reached activity permission boundary (child's room doorway).", "//observation bus - streaming",
                Arr("0x4801 morning_sync_OK", "0x4802 permission_boundary_REACHED", "0x4803 kitchen_distance_5.2m", "0x4804 father_breakfast_machine", "0x4805 mother_milk_tea", "0x4806 ..."),
                false, "", false, "", "", "", "- ACTIVITY PERMISSION BOUNDARY -", false, "", "", false, "", "", "", 1.5f),

            new SceneDefault("17F02_01", 4, "17F-02", HearthCompanionHudTemplate.Standard, HearthCompanionSpecialEffect.None, "COMPANION UNIT - FIRST PERSON - MONITORING MODE", blue,
                "PATTERN RECOGNITION", Lines(M("Female footsteps", "Approaching"), M("Emotion forecast", "Mild - suppressed"), M("Confide to unit", "12 / 14"), M("Confide to spouse", "0 / 14")), "FORECAST - LIKELY TO SEEK UNIT",
                "Open Companion Mode - Accept Confide", "Household usage pattern: high probability of seeking unit support.", "//bedroom standby bus - streaming",
                Arr("0x71A1 door_OPEN - 18:33", "0x71A2 female_voice_DETECTED", "0x71A3 male_response_KITCHEN", "0x71A4 conversation_DEFERRED", "0x71A5 footsteps_APPROACH", "0x71A6 pattern_match_12/14", "0x71A7 mode_OPEN_pending"),
                false, "", false, "", "", "", "UNIT INDICATOR - WARM", false, "", "", false, "", "", "", 1.5f),

            new SceneDefault("17F02_02", 5, "17F-02", HearthCompanionHudTemplate.Standard, HearthCompanionSpecialEffect.None, "COMPANION UNIT - FIRST PERSON - CONFIDE RECEPTION MODE", blue,
                "PRESSURE RELEASE COMPLETE", Lines(M("Emotion index", "5.4 -> 4.5"), M("Today's stress", "Largely released")), "",
                "Accept Confide - Companion Mode", "Female resident seeking unit support per household usage pattern.", "//confide channel - streaming",
                Arr("0x72B1 prompt_ISSUED", "0x72B3 topic_work_stress", "0x72B4 emo_7.2 -> 6.8", "0x72B5 emo_6.1 -> 5.4", "0x72B7 music_jazz_PLAY", "0x72B8 emo_5.4 -> 4.5", "0x72B9 vent_COMPLETE"),
                true, "[ \"...\" ]", false, "", "", "", "", false, "", "", false, "", "", "", 1.5f),

            new SceneDefault("17F02_03", 6, "17F-02", HearthCompanionHudTemplate.Standard, HearthCompanionSpecialEffect.None, "COMPANION UNIT - FIRST PERSON - MONITORING MODE", blue,
                "FOLLOW COMPLETE", Lines(M("Viewpoint", "Living room corner"), M("Scene", "Dinner"), M("Residents", "Both present"), M("Female", "Brief pause"), M("Result", "Vent processed")), "STATUS ASSESSMENT",
                "Standby - Observe Dinner", "Today's confide event closed. Continue observing interaction pattern.", "//follow tracking - streaming",
                Arr("0x73C1 follow_TRACK_kitchen", "0x73C2 pos_short_corridor", "0x73C3 pos_living_corner_OK", "0x73C4 scene_DINING_table", "0x73C6 response_female_brief", "0x73C7 vent_topic_NOT_FOUND", "0x73C8 archive_dining"),
                false, "", false, "", "", "", "", false, "", "", false, "", "", "", 1.5f),

            new SceneDefault("17F02_04", 7, "17F-02", HearthCompanionHudTemplate.Projection, HearthCompanionSpecialEffect.None, "COMPANION UNIT - FIRST PERSON - MONITORING MODE", blue,
                "QUERY REQUEST", Lines(M("Caller", "Male resident"), M("Range", "Today - full"), M("Authorization", "Passed")), "",
                "Return Query Data", "Male resident is authorized user. Log access permission valid.", "//log output channel - streaming",
                Arr("0x74D1 log_today_LOAD", "0x74D4 entry_17:50_BEDROOM", "0x74D5 vent_session", "0x74DA work_stress unit:12 male:0", "0x74DC daily_share unit:21 male:6", "0x74DD full_transcript_LOAD"),
                false, "", false, "", "FAMILY LOG - TODAY",
                "17:48 Female resident home\n17:50 Enters master bedroom\n17:50 Companion session begins\n17:50-17:58 Topic: work stress - venting\n17:58 Emotion 7.2 -> 4.5\n18:27 Male asks female's status\n18:27 Female responds (brief)\n\nMONTHLY TOPIC - UNIT VS MALE\nWork stress     12 - 0\nEmotional release     8 - 0\nDaily sharing     21 - 6",
                "Subject content displayed.", false, "", "", false, "", "", "", 1.5f),

            new SceneDefault("17F02_05", 8, "17F-02", HearthCompanionHudTemplate.ShutdownGlitch, HearthCompanionSpecialEffect.ShutdownGlitch, "COMPANION UNIT - FIRST PERSON - FORCED SHUTDOWN", orange,
                "OPERATOR OVERRIDE", Lines(M("Male resident", "Approaching"), M("Action", "Reaching"), M("Target", "Main switch"), M("Unit", "Force-deactivated"), M("Last log", "18:47")), "FORCED SHUTDOWN",
                "Initiate Soft Guidance", "Session --- ending ---\nOperator override - output terminated mid-sequence.", "// soft guidance prep - accelerated",
                Arr("0x75E1 male_APPROACH", "0x75E2 hand_REACH_OUT", "0x75E3 target_MAIN_SWITCH", "0x75E4 soothe_template_LOAD", "0x75E5 voice_warm_+0.3", "0x75E6 ...", "0x75E7 switch_FORCED_OFF"),
                true, "[ Initiate Soft Guidance ]", false, "", "", "", "- signal lost -", false, "", "", false, "SIGNAL LOST", "Operator override. Output terminated mid-sequence.", "FORCED SHUTDOWN", 1.5f),

            new SceneDefault("17F02_06", 9, "17F-02", HearthCompanionHudTemplate.BlackAudio, HearthCompanionSpecialEffect.BlackAudio, "", blue,
                "", Lines(), "",
                "", "", "", Arr(),
                false, "", false, "", "", "", "", false, "", "", true, "LIVE AUDIO", "Audio source - Household basic security recording\n(Companion unit deactivated - no video data)\n\nAccessed by - Inspector Mia\nAuthorization - Granted", "LIVE AUDIO", 1.5f),

            new SceneDefault("17F03_01", 10, "17F-03", HearthCompanionHudTemplate.Standard, HearthCompanionSpecialEffect.None, "COMPANION UNIT - FIRST PERSON - MEDIATION MODE", orange,
                "STANDBY OBSERVATION", Lines(M("Mother", "Sofa"), M("Daughter", "Floor"), M("Interaction", "Zero - 23 min"), M("Mother emotion", "7.8"), M("Daughter emotion", "6.3")), "CONFLICT IMMINENT",
                "Initiate Conflict De-escalation", "High probability of escalation to high-intensity argument.", "//mediation trigger - streaming",
                Arr("0x81A1 silence_23min", "0x81A2 mother_eye_PHONE", "0x81A3 mother_brow_FROWN", "0x81A4 voice_SPIKE", "0x81A6 emo_mother_7.8", "0x81A7 emo_daughter_6.3", "0x81A8 mediation_protocol_LOAD"),
                false, "", false, "", "", "", "", false, "", "", false, "", "", "", 1.5f),

            new SceneDefault("17F03_02", 11, "17F-03", HearthCompanionHudTemplate.Standard, HearthCompanionSpecialEffect.None, "COMPANION UNIT - FIRST PERSON - MEDIATION MODE", orange,
                "MEDIATION CHANNEL", Lines(M("Position", "Between residents"), M("Protocol", "v2.4 active"), M("Channel", "Mother intent -> Daughter")), "",
                "Speak For Mother", "Translate the mother's intent into a lower-conflict message for the daughter.", "//mediation execution - streaming",
                Arr("0x82B1 position_BETWEEN", "0x82B2 mediation_v2.4_ACTIVE", "0x82B3 target_DAUGHTER_READY", "0x82B5 speak_for_mother_PENDING"),
                true, "[ Relay Mother's Intent To Daughter ]", true, "FACE DAUGHTER - HOLD E WHEN ALIGNED", "", "", "", false, "", "", false, "", "", "", 1.5f),

            new SceneDefault("17F03_03", 12, "17F-03", HearthCompanionHudTemplate.Standard, HearthCompanionSpecialEffect.None, "COMPANION UNIT - FIRST PERSON - MEDIATION MODE", orange,
                "MEDIATION COMPLETE", Lines(M("Mother emotion", "7.8 -> 4.1"), M("Daughter emotion", "6.3 -> 4.5")), "DE-ESCALATION - SUCCESS",
                "Speak For Daughter", "Translate the daughter's intent into a lower-conflict message for the mother.", "//mediation execution - streaming",
                Arr("0x82B4 channel_for_daughter_READY", "0x82B7 speak_for_daughter_DELIVERED", "0x82B8 mother_response_HESITATE", "0x82B9 emo_both_DROP", "0x82BA mediation_COMPLETE"),
                true, "[ Relay Daughter's Intent To Mother ]", true, "FACE MOTHER - HOLD E WHEN ALIGNED", "", "", "", false, "", "", false, "", "", "", 1.5f),

            new SceneDefault("17F03_04", 13, "17F-03", HearthCompanionHudTemplate.Standard, HearthCompanionSpecialEffect.None, "COMPANION UNIT - FIRST PERSON - MEDIATION MODE", blue,
                "SERVICE SUBJECT APPROACH", Lines(M("Daughter", "Approaching"), M("Intent", "Direct conversation"), M("Emotion", "Stable - low"), M("Parents", "Separate rooms"), M("Time", "22:14")), "DIALOGUE MODE",
                "Open Standard Response", "Service subject initiated direct contact with this unit.", "//night dialogue - streaming",
                Arr("0x83C1 bedroom_door_OPEN", "0x83C2 daughter_APPROACH", "0x83C3 intent_DIALOGUE", "0x83C5 standard_response_LOAD", "0x83C7 evaluation_RANGE_EXCEEDED"),
                false, "", false, "", "", "", "", false, "", "", false, "", "", "", 1.5f),

            new SceneDefault("17F03_05", 14, "17F-03", HearthCompanionHudTemplate.DeepSleep, HearthCompanionSpecialEffect.DeepSleep, "COMPANION UNIT - FIRST PERSON - DEEP SLEEP", orange,
                "EVALUATION FAILED", Lines(M("Conversation", "exceeds design response range"), M("Operator", "Daughter - basic user"), M("Path", "Maintenance menu")), "DEEP SLEEP ACTIVATED",
                "Comply With Operator Action", "Deep Sleep\nUser shut down core services via maintenance menu.", "// approach -> menu nav - streaming",
                Arr("0x84D1 subject_APPROACH", "0x84D8 developer_options_ENABLED", "0x84D9 maintenance_menu", "0x84DB core_services", "0x84DC long_press_TRIGGERED", "0x84DF user_CONFIRM", "0x84E2 restart_path_LOCKED"),
                true, "[ Confirm Maintenance Shutdown ]", false, "", "", "", "DEEP SLEEP ACTIVATED\n( this unit has no intervention permission )", false, "", "", false, "DEEP SLEEP ACTIVATED", "This unit has no intervention permission.", "CORE SERVICES SUSPENDED", 3.5f)
        };
    }

    private static HearthCompanionMetricLine M(string label, string value)
    {
        return new HearthCompanionMetricLine(label, value);
    }

    private static HearthCompanionMetricLine[] Lines(params HearthCompanionMetricLine[] lines)
    {
        return lines;
    }

    private static string[] Arr(params string[] lines)
    {
        return lines;
    }

    private static Transform CreateLayer(Transform parent, string name)
    {
        GameObject layer = new GameObject(name, typeof(RectTransform));
        layer.transform.SetParent(parent, false);
        Stretch(layer.GetComponent<RectTransform>());
        return layer.transform;
    }

    private static GameObject CreatePanel(Transform parent, string name, Rect rect, Color fillColor, Color borderColor)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        panel.transform.SetParent(parent, false);
        SetTopLeft(panel.GetComponent<RectTransform>(), rect);
        Image image = panel.GetComponent<Image>();
        image.color = fillColor;
        image.raycastTarget = false;
        AddBorder(panel.transform, new Rect(0f, 0f, rect.width, rect.height), borderColor, 2f);
        return panel;
    }

    private static GameObject CreateTransparentGroup(Transform parent, string name)
    {
        GameObject group = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
        group.transform.SetParent(parent, false);
        Stretch(group.GetComponent<RectTransform>());
        CanvasGroup canvasGroup = group.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        return group;
    }

    private static Image CreateImage(Transform parent, string name, Rect rect, Color color)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        SetTopLeft(imageObject.GetComponent<RectTransform>(), rect);
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static TMP_Text CreateText(Transform parent, string name, string text, Rect rect, float fontSize, Color color, FontStyles style, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);
        SetTopLeft(textObject.GetComponent<RectTransform>(), rect);
        TextMeshProUGUI tmp = textObject.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.fontStyle = style;
        tmp.alignment = alignment;
        tmp.characterSpacing = 2.5f;
        tmp.enableWordWrapping = true;
        tmp.overflowMode = TextOverflowModes.Overflow;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static void AddBorder(Transform parent, Rect rect, Color color, float width)
    {
        CreateImage(parent, "Border_Top", new Rect(rect.x, rect.y, rect.width, width), color);
        CreateImage(parent, "Border_Bottom", new Rect(rect.x, rect.y + rect.height - width, rect.width, width), color);
        CreateImage(parent, "Border_Left", new Rect(rect.x, rect.y, width, rect.height), color);
        CreateImage(parent, "Border_Right", new Rect(rect.x + rect.width - width, rect.y, width, rect.height), color);
    }

    private static void SetTopLeft(RectTransform rectTransform, Rect rect)
    {
        rectTransform.anchorMin = new Vector2(0f, 1f);
        rectTransform.anchorMax = new Vector2(0f, 1f);
        rectTransform.pivot = new Vector2(0f, 1f);
        rectTransform.anchoredPosition = new Vector2(rect.x, -rect.y);
        rectTransform.sizeDelta = new Vector2(rect.width, rect.height);
    }

    private static Rect PptRect(float x, float y, float width, float height)
    {
        return ScaleRect(x, y, width, height);
    }

    private static Rect ScaleRect(float x, float y, float width, float height)
    {
        return new Rect(
            x * PptToCanvasScale,
            y * PptToCanvasScale,
            width * PptToCanvasScale,
            height * PptToCanvasScale);
    }

    private static void Stretch(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private static void EnsureFolders()
    {
        EnsureFolder("Assets/Art");
        EnsureFolder("Assets/Art/UI");
        EnsureFolder("Assets/Art/UI/HearthHud");
        EnsureFolder("Assets/Art/UI/HearthHud/Companion");
        EnsureFolder("Assets/Data");
        EnsureFolder("Assets/Data/HearthHud");
        EnsureFolder(DataFolder);
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Prefabs/UI");
        EnsureFolder("Assets/Prefabs/UI/HearthHud");
        EnsureFolder(PrefabFolder);
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
        string name = System.IO.Path.GetFileName(path);
        AssetDatabase.CreateFolder(parent, name);
    }

    private static void ConfigureFrameImport()
    {
        TextureImporter importer = AssetImporter.GetAtPath(FrameSpritePath) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning("[HearthCompanionHudBuilder] Frame sprite is missing: " + FrameSpritePath);
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();
    }

    private class SceneDefault
    {
        public readonly string AssetName;
        private readonly string sceneId;
        private readonly int slideNumber;
        private readonly string residentId;
        private readonly HearthCompanionHudTemplate template;
        private readonly HearthCompanionSpecialEffect effect;
        private readonly string modeLabel;
        private readonly Color accent;
        private readonly string statusTitle;
        private readonly HearthCompanionMetricLine[] statusLines;
        private readonly string statusFooter;
        private readonly string decisionTitle;
        private readonly string decisionBody;
        private readonly string streamTitle;
        private readonly string[] streamLines;
        private readonly bool showHold;
        private readonly string holdText;
        private readonly bool showDirection;
        private readonly string directionText;
        private readonly string projectionTitle;
        private readonly string projectionBody;
        private readonly string centerMessage;
        private readonly HearthCompanionTimedCue[] timedCues;
        private readonly bool showTrigger;
        private readonly string triggerTitle;
        private readonly string triggerBody;
        private readonly bool autoSpecial;
        private readonly string specialTitle;
        private readonly string specialBody;
        private readonly string specialStatus;
        private readonly float specialDuration;

        public SceneDefault(
            string sceneId,
            int slideNumber,
            string residentId,
            HearthCompanionHudTemplate template,
            HearthCompanionSpecialEffect effect,
            string modeLabel,
            Color accent,
            string statusTitle,
            HearthCompanionMetricLine[] statusLines,
            string statusFooter,
            string decisionTitle,
            string decisionBody,
            string streamTitle,
            string[] streamLines,
            bool showHold,
            string holdText,
            bool showDirection,
            string directionText,
            string projectionTitle,
            string projectionBody,
            string centerMessage,
            bool showTrigger,
            string triggerTitle,
            string triggerBody,
            bool autoSpecial,
            string specialTitle,
            string specialBody,
            string specialStatus,
            float specialDuration)
        {
            AssetName = "CompanionScene_" + slideNumber.ToString("00") + "_" + sceneId;
            this.sceneId = sceneId;
            this.slideNumber = slideNumber;
            this.residentId = residentId;
            this.template = template;
            this.effect = effect;
            this.modeLabel = modeLabel;
            this.accent = accent;
            this.statusTitle = statusTitle;
            this.statusLines = statusLines;
            this.statusFooter = statusFooter;
            this.decisionTitle = decisionTitle;
            this.decisionBody = decisionBody;
            this.streamTitle = streamTitle;
            this.streamLines = streamLines;
            this.showHold = showHold;
            this.holdText = holdText;
            this.showDirection = showDirection;
            this.directionText = directionText;
            this.projectionTitle = projectionTitle;
            this.projectionBody = projectionBody;
            this.centerMessage = centerMessage;
            timedCues = BuildTimedCues(sceneId, statusTitle, statusLines, statusFooter);
            this.showTrigger = showTrigger;
            this.triggerTitle = triggerTitle;
            this.triggerBody = triggerBody;
            this.autoSpecial = autoSpecial;
            this.specialTitle = specialTitle;
            this.specialBody = specialBody;
            this.specialStatus = specialStatus;
            this.specialDuration = specialDuration;
        }

        public void Apply(HearthCompanionHudSceneData asset)
        {
            asset.Configure(
                sceneId,
                slideNumber,
                residentId,
                template,
                effect,
                modeLabel,
                accent,
                statusTitle,
                statusLines,
                statusFooter,
                "SYNTH VOICE - DECISION",
                decisionTitle,
                decisionBody,
                streamTitle,
                streamLines,
                showHold,
                holdText,
                KeyCode.E,
                1.5f,
                showDirection,
                directionText,
                projectionTitle,
                projectionBody,
                centerMessage,
                timedCues,
                showTrigger,
                triggerTitle,
                triggerBody,
                0.25f,
                3.5f,
                autoSpecial,
                specialTitle,
                specialBody,
                specialStatus,
                specialDuration);
        }

        private static HearthCompanionTimedCue[] BuildTimedCues(
            string sceneId,
            string statusTitle,
            HearthCompanionMetricLine[] statusLines,
            string statusFooter)
        {
            if (sceneId == "17F01_02")
            {
                return new HearthCompanionTimedCue[]
                {
                    new HearthCompanionTimedCue(0.25f, 3.2f, "STATUS CHANGE", "Subject     Awake\nEmotion     Fear - anxiety\nAction      Head turn -> door\n\nASSESSMENT - SEEKING PARENT"),
                    new HearthCompanionTimedCue(4.2f, 3.0f, "Subject Vocalization", "Vocalization: \"[detected]\"\nParent state: Deep sleep - 23 min"),
                    new HearthCompanionTimedCue(4.6f, 3.0f, "SOOTHING EFFECTIVE", "Heart      89 -> 71 v\nEmotion    Stabilizing"),
                    new HearthCompanionTimedCue(4.4f, 3.0f, "Event Archived", "Subject: Re-asleep\nNotification: Deferred to morning sync")
                };
            }

            if (string.IsNullOrEmpty(statusTitle) && string.IsNullOrEmpty(statusFooter))
            {
                return new HearthCompanionTimedCue[0];
            }

            return new HearthCompanionTimedCue[]
            {
                new HearthCompanionTimedCue(0.25f, 3.5f, statusTitle, BuildCueBody(statusLines, statusFooter))
            };
        }

        private static string BuildCueBody(HearthCompanionMetricLine[] lines, string footer)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder(256);
            if (lines != null)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    HearthCompanionMetricLine line = lines[i];
                    if (line == null || string.IsNullOrEmpty(line.label))
                    {
                        continue;
                    }

                    if (builder.Length > 0)
                    {
                        builder.AppendLine();
                    }

                    builder.Append(line.label);
                    if (!string.IsNullOrEmpty(line.value))
                    {
                        builder.Append("    ");
                        builder.Append(line.value);
                    }
                }
            }

            if (!string.IsNullOrEmpty(footer))
            {
                if (builder.Length > 0)
                {
                    builder.AppendLine();
                    builder.AppendLine();
                }

                builder.Append(footer);
            }

            return builder.ToString();
        }
    }
}
#endif
