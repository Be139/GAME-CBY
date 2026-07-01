using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class HearthFirstPersonHudBuilder
{
    private const int ReferenceWidth = 1920;
    private const int ReferenceHeight = 1080;
    private const string LayoutJsonPath = "Assets/Art/UI/HearthHud/FirstPersonLayout/HearthFirstPersonHudLayout.json";
    private const string PagePrefabDir = "Assets/Prefabs/UI/HearthHud/FirstPersonPages";
    private const string RootPrefabPath = "Assets/Prefabs/UI/HearthHud/HearthHudRoot.prefab";

    [MenuItem("Tools/Hearth/HUD/Rebuild First Person HUD Prefab")]
    public static void BuildFirstPersonHudPrefab()
    {
        EnsureAssetFolders();
        AssetDatabase.Refresh();

        HearthHudLayout layout = LoadLayout();
        if (layout == null || layout.slides == null || layout.slides.Length == 0)
        {
            Debug.LogError("[HearthFirstPersonHudBuilder] Missing or invalid layout JSON: " + LayoutJsonPath);
            return;
        }

        GameObject root = CreateRoot();
        HearthFirstPersonHudController controller = root.GetComponent<HearthFirstPersonHudController>();
        HearthFirstPersonHudInput input = root.GetComponent<HearthFirstPersonHudInput>();
        HearthDispositionHistoryView historyView = root.GetComponent<HearthDispositionHistoryView>();
        HearthSettingsView settingsView = root.GetComponent<HearthSettingsView>();
        HearthPlayerControlLock playerControlLock = root.GetComponent<HearthPlayerControlLock>();

        RectTransform rootRect = root.GetComponent<RectTransform>();
        Transform persistentLayer = CreateLayer(rootRect, "PersistentHudLayer");
        Transform overlayLayer = CreateLayer(rootRect, "PanelLayer");
        Transform fullscreenLayer = CreateLayer(rootRect, "FullscreenTakeoverLayer");
        Transform focusLayer = CreateLayer(rootRect, "FocusLayer");
        Transform trustLayer = CreateLayer(rootRect, "TrustDeltaLayer");
        CreateLayer(rootRect, "DebugPreviewLayer");

        SlideLayout persistentSlide = FindSlide(layout, 1);
        GameObject persistent = CreateGroup(persistentLayer, "PersistentHud");
        BuildSlideContent(persistent.transform, persistentSlide, false, true, out _, out _);
        CreateLocationHudView(persistent.transform);
        CanvasGroup persistentGroup = persistent.AddComponent<CanvasGroup>();

        CanvasGroup trustGroup = CreateTrustDeltaView(trustLayer, FindSlide(layout, 2), out TMP_Text trustDeltaText);

        List<HearthFirstPersonHudPage> pages = new List<HearthFirstPersonHudPage>();
        List<HearthDispositionHistoryView.RowBinding> historyRows = new List<HearthDispositionHistoryView.RowBinding>();
        List<TMP_Text> historyShiftTexts = new List<TMP_Text>();
        List<TMP_Text> historyTrustTexts = new List<TMP_Text>();
        for (int i = 3; i <= 24; i++)
        {
            bool fullscreen = i >= 15 && i <= 17;
            Transform parent = fullscreen ? fullscreenLayer : overlayLayer;
            HearthFirstPersonHudPage page = CreatePage(parent, FindSlide(layout, i), fullscreen, controller, historyRows, historyShiftTexts, historyTrustTexts);
            pages.Add(page);

            string pagePath = PagePrefabDir + "/" + GetPagePrefabName(i) + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(page.gameObject, pagePath);
        }

        RectTransform menuFocus = CreateFocusRect(focusLayer, "MenuFocus", new Color(0.18f, 0.6f, 0.85f, 0.16f));
        RectTransform[] menuTargets = CreateMenuTargets(focusLayer, FindSlide(layout, 3));
        RectTransform finalFocus = CreateFocusRect(focusLayer, "FinalChoiceFocus", new Color(0.18f, 0.6f, 0.85f, 0.14f));
        RectTransform[] finalTargets = CreateFinalChoiceTargets(focusLayer, FindSlide(layout, 9));
        RectTransform settingsFocus = CreateFocusRect(focusLayer, "SettingsFocus", new Color(0.18f, 0.6f, 0.85f, 0.14f));
        RectTransform[] settingsTargets = CreateSettingsTargets(focusLayer, FindSlide(layout, 22));

        BindController(
            controller,
            pages.ToArray(),
            persistent,
            persistentGroup,
            trustGroup,
            trustDeltaText,
            menuFocus,
            menuTargets,
            finalFocus,
            finalTargets,
            historyView,
            settingsView,
            playerControlLock);
        BindHistoryView(historyView, historyRows, historyShiftTexts, historyTrustTexts);

        BindInput(input, controller, settingsView);
        BindSettingsView(settingsView, settingsFocus, settingsTargets);
        UnityEventTools.AddPersistentListener(settingsView.OnExitRequested, controller.OpenExitConfirm);

        PrefabUtility.SaveAsPrefabAsset(root, RootPrefabPath);
        UnityEngine.Object.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[HearthFirstPersonHudBuilder] Rebuilt vector first-person HUD prefab at " + RootPrefabPath);
    }

    [MenuItem("Tools/Hearth/HUD/Rebuild And Apply First Person HUD To Scene")]
    public static void BuildAndApplyToOpenScene()
    {
        BuildFirstPersonHudPrefab();

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RootPrefabPath);
        if (prefab == null)
        {
            Debug.LogError("[HearthFirstPersonHudBuilder] Missing prefab: " + RootPrefabPath);
            return;
        }

        GameObject existing = GameObject.Find("HearthHudRoot");
        if (existing != null)
        {
            UnityEngine.Object.DestroyImmediate(existing);
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = "HearthHudRoot";
        RectTransform rect = instance.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = new Vector2(ReferenceWidth * 0.5f, ReferenceHeight * 0.5f);
            rect.sizeDelta = new Vector2(ReferenceWidth, ReferenceHeight);
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[HearthFirstPersonHudBuilder] Applied vector HearthHudRoot to the open scene.");
    }

    private static GameObject CreateRoot()
    {
        GameObject root = new GameObject(
            "HearthHudRoot",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(AudioSource),
            typeof(HearthPlayerControlLock),
            typeof(HearthFirstPersonHudController),
            typeof(HearthFirstPersonHudInput),
            typeof(HearthDispositionHistoryView),
            typeof(HearthSettingsView),
            typeof(HearthFirstPersonHudFlowBinder));

        RectTransform rect = root.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(ReferenceWidth, ReferenceHeight);
        rect.anchoredPosition = new Vector2(ReferenceWidth * 0.5f, ReferenceHeight * 0.5f);

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        return root;
    }

    private static Transform CreateLayer(Transform parent, string name)
    {
        GameObject layer = CreateGroup(parent, name);
        return layer.transform;
    }

    private static GameObject CreateGroup(Transform parent, string name)
    {
        GameObject group = new GameObject(name, typeof(RectTransform));
        group.transform.SetParent(parent, false);
        SetFullStretch(group.GetComponent<RectTransform>());
        return group;
    }

    private static HearthFirstPersonHudPage CreatePage(
        Transform parent,
        SlideLayout slide,
        bool fullscreen,
        HearthFirstPersonHudController controller,
        List<HearthDispositionHistoryView.RowBinding> historyRows,
        List<TMP_Text> historyShiftTexts,
        List<TMP_Text> historyTrustTexts)
    {
        int slideNumber = slide != null ? slide.number : 0;
        GameObject pageObject = CreateGroup(parent, GetPagePrefabName(slideNumber));

        if (fullscreen)
        {
            CreateImage(pageObject.transform, "FullscreenBlack", new RectData(0f, 0f, ReferenceWidth, ReferenceHeight), Color.black);
        }

        BuildSlideContent(pageObject.transform, slide, fullscreen, false, out _, out _);
        if (IsHistorySlide(slideNumber))
        {
            CreateHistoryRuntimeBindings(pageObject.transform, slideNumber, historyRows, historyShiftTexts, historyTrustTexts);
        }
        CanvasGroup group = pageObject.AddComponent<CanvasGroup>();
        HearthFirstPersonHudPage page = pageObject.AddComponent<HearthFirstPersonHudPage>();
        page.Configure((HearthFirstPersonHudPageId)slideNumber, fullscreen, !fullscreen);
        AddPageInteractions(pageObject, controller, slide);
        page.SetVisible(false);
        group.alpha = 0f;
        return page;
    }

    private static void BuildSlideContent(
        Transform parent,
        SlideLayout slide,
        bool fullscreen,
        bool persistentSlide,
        out List<TMP_Text> createdTexts,
        out List<RectTransform> createdShapes)
    {
        createdTexts = new List<TMP_Text>();
        createdShapes = new List<RectTransform>();
        if (slide == null || slide.shapes == null)
        {
            return;
        }

        for (int i = 0; i < slide.shapes.Length; i++)
        {
            ShapeLayout shape = slide.shapes[i];
            if (ShouldSkipShape(slide.number, shape, fullscreen, persistentSlide))
            {
                continue;
            }

            bool hasText = !string.IsNullOrWhiteSpace(shape.text);
            bool isLine = IsLineShape(shape);

            if (shape.fillVisible && shape.fillAlpha > 0.01f && !isLine && !ShouldSkipFill(slide.number, shape, fullscreen, persistentSlide))
            {
                RectTransform rect = CreateImage(parent, "ShapeFill_" + shape.index.ToString("000"), shape.rect, ToFillColor(slide.number, shape));
                createdShapes.Add(rect);
            }

            if (shape.lineVisible && shape.lineAlpha > 0.01f)
            {
                if (isLine)
                {
                    RectTransform rect = CreateLine(parent, "Line_" + shape.index.ToString("000"), slide.number, shape);
                    createdShapes.Add(rect);
                }
                else
                {
                    AddBorder(parent, shape.rect, ToLineColor(slide.number, shape), Mathf.Max(1f, shape.lineWeight));
                }
            }

            if (hasText)
            {
                RectData textRect = shape.textRect != null && shape.textRect.w > 0.1f && shape.textRect.h > 0.1f
                    ? Inflate(shape.textRect, 3f, 2f)
                    : shape.rect;

                TMP_Text text = CreateText(
                    parent,
                    "Text_" + shape.index.ToString("000") + "_" + CleanObjectName(shape.text),
                    shape.text,
                    textRect,
                    Mathf.Clamp(shape.fontSize, 7f, 96f),
                    ToTextColor(slide.number, shape),
                    shape.bold ? FontStyles.Bold : FontStyles.Normal,
                    ToAlignment(shape.align));
                createdTexts.Add(text);
            }
        }
    }

    private static bool ShouldSkipShape(int slideNumber, ShapeLayout shape, bool fullscreen, bool persistentSlide)
    {
        if (shape == null || shape.rect == null)
        {
            return true;
        }

        bool nearFullScreen = shape.rect.w >= ReferenceWidth - 4f && shape.rect.h >= ReferenceHeight - 4f;
        bool darkFill = shape.fillVisible && IsVeryDark(shape.fillColor) && shape.fillAlpha > 0.65f;
        bool textEmpty = string.IsNullOrWhiteSpace(shape.text);

        if (!textEmpty && IsHistoryGeneratedDynamicText(slideNumber, shape.text))
        {
            return true;
        }

        if (!fullscreen && nearFullScreen && textEmpty && darkFill)
        {
            return true;
        }

        if (!fullscreen && textEmpty && darkFill && shape.lineAlpha <= 0.01f && shape.rect.w > 900f && shape.rect.h > 500f)
        {
            return true;
        }

        if (persistentSlide && textEmpty && darkFill && shape.fillAlpha < 0.9f)
        {
            return true;
        }

        return false;
    }

    private static bool ShouldSkipFill(int slideNumber, ShapeLayout shape, bool fullscreen, bool persistentSlide)
    {
        if (shape == null || shape.rect == null)
        {
            return true;
        }

        if (fullscreen || !persistentSlide || !string.IsNullOrWhiteSpace(shape.text))
        {
            return false;
        }

        if (shape.rect.w * shape.rect.h > 2500f)
        {
            return true;
        }

        Color fillColor = ToColor(shape.fillColor, shape.fillAlpha);
        return fillColor.a > 0.85f && Luminance(fillColor) > 0.42f;
    }

    private static CanvasGroup CreateTrustDeltaView(Transform parent, SlideLayout slide, out TMP_Text trustDeltaText)
    {
        GameObject root = CreateGroup(parent, "TrustDelta");
        SetTopLeft(root.GetComponent<RectTransform>(), new RectData(0f, 0f, ReferenceWidth, ReferenceHeight));

        BuildSlideContent(root.transform, slide, false, false, out List<TMP_Text> texts, out _);
        trustDeltaText = null;
        for (int i = 0; i < texts.Count; i++)
        {
            if (texts[i] != null && texts[i].text.Contains("+"))
            {
                trustDeltaText = texts[i];
                break;
            }
        }

        if (trustDeltaText == null)
        {
            trustDeltaText = CreateText(root.transform, "TrustDeltaText", "+1 TRUST", new RectData(1700f, 35f, 180f, 44f), 18f, new Color(0.82f, 0.95f, 1f, 0.95f), FontStyles.Bold, TextAlignmentOptions.Right);
        }

        CanvasGroup group = root.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        root.SetActive(false);
        return group;
    }

    private static HearthLocationHudView CreateLocationHudView(Transform parent)
    {
        GameObject root = CreateGroup(parent, "LocationHud");
        CanvasGroup group = root.AddComponent<CanvasGroup>();

        TMP_Text titleText = CreateText(
            root.transform,
            "LocationTitleText",
            "LOCATION",
            new RectData(1570f, 956f, 290f, 18f),
            14f,
            new Color(0.52f, 0.82f, 0.95f, 0.86f),
            FontStyles.Bold,
            TextAlignmentOptions.Right);

        TMP_Text glowText = CreateText(
            root.transform,
            "LocationGlowText",
            "17F-04",
            new RectData(1490f, 980f, 370f, 46f),
            19f,
            new Color(0.25f, 0.75f, 1f, 0.28f),
            FontStyles.Bold,
            TextAlignmentOptions.Right);

        TMP_Text locationText = CreateText(
            root.transform,
            "LocationValueText",
            "17F-04",
            new RectData(1490f, 982f, 370f, 46f),
            19f,
            new Color(0.78f, 0.93f, 1f, 0.96f),
            FontStyles.Bold,
            TextAlignmentOptions.Right);

        HearthLocationHudView view = root.AddComponent<HearthLocationHudView>();
        view.Configure(group, titleText, locationText, glowText);
        view.HideImmediate();
        return view;
    }

    private static void CreateHistoryRuntimeBindings(
        Transform parent,
        int slideNumber,
        List<HearthDispositionHistoryView.RowBinding> rows,
        List<TMP_Text> shiftTrustTexts,
        List<TMP_Text> currentTrustTexts)
    {
        if (rows == null || shiftTrustTexts == null || currentTrustTexts == null)
        {
            return;
        }

        float[] rowY = GetHistoryRowYPositions(slideNumber);
        for (int i = 0; i < rowY.Length; i++)
        {
            GameObject rowRoot = CreateGroup(parent, "RuntimeHistoryRow_" + (i + 1).ToString("00"));
            TMP_Text timestamp = CreateText(rowRoot.transform, "Timestamp", string.Empty, new RectData(622.6f, rowY[i], 230f, 22f), 16f, HistoryPrimaryColor(), FontStyles.Bold, TextAlignmentOptions.Left);
            TMP_Text unit = CreateText(rowRoot.transform, "Unit", string.Empty, new RectData(1160f, rowY[i], 140f, 22f), 17f, HistoryPrimaryColor(), FontStyles.Bold, TextAlignmentOptions.Right);
            TMP_Text action = CreateText(rowRoot.transform, "Action", string.Empty, new RectData(622.6f, rowY[i] + 26.2f, 520f, 24f), 18f, HistoryPrimaryColor(), FontStyles.Bold, TextAlignmentOptions.Left);
            TMP_Text status = CreateText(rowRoot.transform, "Status", string.Empty, new RectData(622.6f, rowY[i] + 53.9f, 220f, 18f), 13f, HistoryMutedColor(), FontStyles.Bold, TextAlignmentOptions.Left);
            TMP_Text trust = CreateText(rowRoot.transform, "TrustDelta", string.Empty, new RectData(1165f, rowY[i] + 53.2f, 136f, 22f), 16f, HistoryPrimaryColor(), FontStyles.Bold, TextAlignmentOptions.Right);

            rows.Add(new HearthDispositionHistoryView.RowBinding
            {
                recordIndex = i,
                rowRoot = rowRoot,
                timestampText = timestamp,
                unitText = unit,
                actionText = action,
                statusText = status,
                trustDeltaText = trust
            });
        }

        float footerShiftY = slideNumber == 21 ? 711.7f : 729f;
        float footerTrustY = slideNumber == 21 ? 743.7f : 761f;
        shiftTrustTexts.Add(CreateText(parent, "RuntimeShiftTrustDelta", string.Empty, new RectData(1216f, footerShiftY, 96f, 24f), 18f, HistoryPrimaryColor(), FontStyles.Bold, TextAlignmentOptions.Right));
        currentTrustTexts.Add(CreateText(parent, "RuntimeCurrentTrust", string.Empty, new RectData(1216f, footerTrustY, 96f, 24f), 18f, HistoryPrimaryColor(), FontStyles.Bold, TextAlignmentOptions.Right));
    }

    private static float[] GetHistoryRowYPositions(int slideNumber)
    {
        switch (slideNumber)
        {
            case 19:
                return new[] { 412.1f };
            case 20:
                return new[] { 412.1f, 524.1f };
            case 21:
                return new[] { 379.7f, 491.7f, 603.7f };
            default:
                return Array.Empty<float>();
        }
    }

    private static Color HistoryPrimaryColor()
    {
        return new Color(0.78f, 0.93f, 1f, 0.95f);
    }

    private static Color HistoryMutedColor()
    {
        return new Color(0.52f, 0.82f, 0.95f, 0.86f);
    }

    private static void AddPageInteractions(GameObject pageRoot, HearthFirstPersonHudController controller, SlideLayout slide)
    {
        if (slide == null)
        {
            return;
        }

        switch (slide.number)
        {
            case 3:
                CreateButtonFromText(pageRoot.transform, slide, "TODAY", controller, HearthFirstPersonHudActionType.OpenTodayRounds, 22f, 12f);
                CreateButtonFromText(pageRoot.transform, slide, "DISPOSITION HISTORY", controller, HearthFirstPersonHudActionType.OpenDispositionHistory, 22f, 12f);
                CreateButtonFromText(pageRoot.transform, slide, "SYSTEM SETTINGS", controller, HearthFirstPersonHudActionType.OpenSettings, 22f, 12f);
                break;
            case 4:
                CreateButtonFromText(pageRoot.transform, slide, "CONFIRM", controller, HearthFirstPersonHudActionType.ConfirmSync, 28f, 18f);
                break;
            case 6:
            case 7:
            case 8:
                CreateButton(pageRoot.transform, "Button_CloseStory", new RectData(0f, 0f, ReferenceWidth, ReferenceHeight), controller, HearthFirstPersonHudActionType.HideOverlay);
                break;
            case 9:
            case 14:
                CreateButtonFromText(pageRoot.transform, slide, "ANSWER LILY", controller, HearthFirstPersonHudActionType.ChooseFinalA, 32f, 18f);
                CreateButtonFromText(pageRoot.transform, slide, "COMPANION ANSWER", controller, HearthFirstPersonHudActionType.ChooseFinalB, 32f, 18f);
                break;
            case 10:
                CreateButtonFromText(pageRoot.transform, slide, "CONFIRM", controller, HearthFirstPersonHudActionType.ConfirmGracefulShutdown, 24f, 16f);
                CreateButtonFromText(pageRoot.transform, slide, "CANCEL", controller, HearthFirstPersonHudActionType.CancelShutdown, 24f, 16f);
                break;
            case 11:
            case 12:
                CreateButtonFromText(pageRoot.transform, slide, "YES", controller, HearthFirstPersonHudActionType.ContinueWarning, 24f, 14f);
                CreateButtonFromText(pageRoot.transform, slide, "NO", controller, HearthFirstPersonHudActionType.CancelWarning, 24f, 14f);
                break;
            case 13:
                CreateButtonFromText(pageRoot.transform, slide, "FORCE EXECUTE", controller, HearthFirstPersonHudActionType.ContinueWarning, 24f, 14f);
                CreateButtonFromText(pageRoot.transform, slide, "CANCEL", controller, HearthFirstPersonHudActionType.CancelWarning, 24f, 14f);
                break;
            case 22:
            case 23:
                CreateButtonFromText(pageRoot.transform, slide, "EXIT GAME", controller, HearthFirstPersonHudActionType.ShowPage, 26f, 16f, HearthFirstPersonHudPageId.Slide24ExitConfirm);
                break;
            case 24:
                CreateButtonFromText(pageRoot.transform, slide, "CONFIRM", controller, HearthFirstPersonHudActionType.ConfirmExit, 24f, 16f);
                CreateButtonFromText(pageRoot.transform, slide, "CANCEL", controller, HearthFirstPersonHudActionType.CancelExit, 24f, 16f);
                break;
        }
    }

    private static RectTransform[] CreateMenuTargets(Transform parent, SlideLayout slide)
    {
        return new[]
        {
            CreateFocusTarget(parent, "MenuTarget_TodaysRounds", FindTextRect(slide, "TODAY", new RectData(970f, 42f, 150f, 24f)), 4f, 2f),
            CreateFocusTarget(parent, "MenuTarget_History", FindTextRect(slide, "DISPOSITION HISTORY", new RectData(962f, 70f, 170f, 24f)), 4f, 2f),
            CreateFocusTarget(parent, "MenuTarget_Settings", FindTextRect(slide, "SYSTEM SETTINGS", new RectData(970f, 98f, 150f, 24f)), 4f, 2f)
        };
    }

    private static RectTransform[] CreateFinalChoiceTargets(Transform parent, SlideLayout slide)
    {
        return new[]
        {
            CreateFocusTarget(parent, "FinalChoiceTarget_A", FindTextRect(slide, "ANSWER LILY", new RectData(210f, 455f, 390f, 32f)), 8f, 5f),
            CreateFocusTarget(parent, "FinalChoiceTarget_B", FindTextRect(slide, "COMPANION ANSWER", new RectData(210f, 552f, 560f, 32f)), 8f, 5f)
        };
    }

    private static RectTransform[] CreateSettingsTargets(Transform parent, SlideLayout slide)
    {
        return new[]
        {
            CreateFocusTarget(parent, "SettingsTarget_Master", FindTextRect(slide, "Master Volume", new RectData(700f, 444f, 430f, 24f)), 4f, 2f),
            CreateFocusTarget(parent, "SettingsTarget_Dialogue", FindTextRect(slide, "Dialogue Volume", new RectData(700f, 476f, 430f, 24f)), 4f, 2f),
            CreateFocusTarget(parent, "SettingsTarget_Ambient", FindTextRect(slide, "Ambient Volume", new RectData(700f, 508f, 430f, 24f)), 4f, 2f),
            CreateFocusTarget(parent, "SettingsTarget_SFX", FindTextRect(slide, "SFX Volume", new RectData(700f, 540f, 430f, 24f)), 4f, 2f),
            CreateFocusTarget(parent, "SettingsTarget_Exit", FindTextRect(slide, "EXIT GAME", new RectData(835f, 600f, 160f, 36f)), 6f, 4f)
        };
    }

    private static void CreateButtonFromText(
        Transform parent,
        SlideLayout slide,
        string contains,
        HearthFirstPersonHudController controller,
        HearthFirstPersonHudActionType action,
        float horizontalPadding,
        float verticalPadding,
        HearthFirstPersonHudPageId targetPage = HearthFirstPersonHudPageId.None)
    {
        RectData rect = FindTextRect(slide, contains, null);
        if (rect == null)
        {
            return;
        }

        CreateButton(parent, "Button_" + CleanObjectName(contains), Inflate(rect, horizontalPadding, verticalPadding), controller, action, targetPage);
    }

    private static GameObject CreateButton(
        Transform parent,
        string name,
        RectData rect,
        HearthFirstPersonHudController controller,
        HearthFirstPersonHudActionType action,
        HearthFirstPersonHudPageId targetPage = HearthFirstPersonHudPageId.None)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(HearthFirstPersonHudButtonAction));
        go.transform.SetParent(parent, false);
        SetTopLeft(go.GetComponent<RectTransform>(), rect);
        Image image = go.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0f);
        Button button = go.GetComponent<Button>();
        button.transition = Selectable.Transition.None;
        go.GetComponent<HearthFirstPersonHudButtonAction>().Configure(controller, action, targetPage);
        return go;
    }

    private static RectTransform CreateFocusRect(Transform parent, string name, Color color)
    {
        RectTransform rect = CreateImage(parent, name, new RectData(0f, 0f, 10f, 10f), color);
        rect.gameObject.SetActive(false);
        return rect;
    }

    private static RectTransform CreateFocusTarget(Transform parent, string name, RectData rect, float horizontalPadding, float verticalPadding)
    {
        GameObject target = new GameObject(name, typeof(RectTransform));
        target.transform.SetParent(parent, false);
        SetTopLeft(target.GetComponent<RectTransform>(), Inflate(rect, horizontalPadding, verticalPadding));
        target.hideFlags = HideFlags.HideInHierarchy;
        return target.GetComponent<RectTransform>();
    }

    private static RectData FindTextRect(SlideLayout slide, string contains, RectData fallback)
    {
        if (slide == null || slide.shapes == null)
        {
            return fallback;
        }

        string needle = Normalize(contains);
        ShapeLayout best = null;
        for (int i = 0; i < slide.shapes.Length; i++)
        {
            ShapeLayout shape = slide.shapes[i];
            if (shape == null || string.IsNullOrWhiteSpace(shape.text))
            {
                continue;
            }

            if (Normalize(shape.text).Contains(needle))
            {
                if (best == null || Area(TextOrShapeRect(shape)) > Area(TextOrShapeRect(best)))
                {
                    best = shape;
                }
            }
        }

        return best != null ? TextOrShapeRect(best) : fallback;
    }

    private static RectData TextOrShapeRect(ShapeLayout shape)
    {
        return shape.textRect != null && shape.textRect.w > 0.1f && shape.textRect.h > 0.1f ? shape.textRect : shape.rect;
    }

    private static RectTransform CreateImage(Transform parent, string name, RectData rect, Color color)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        SetTopLeft(imageObject.GetComponent<RectTransform>(), rect);
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return imageObject.GetComponent<RectTransform>();
    }

    private static RectTransform CreateLine(Transform parent, string name, int slideNumber, ShapeLayout shape)
    {
        RectData rect = shape.rect;
        Color color = ToLineColor(slideNumber, shape);
        GameObject lineObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        lineObject.transform.SetParent(parent, false);
        Image image = lineObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;

        RectTransform transform = lineObject.GetComponent<RectTransform>();
        float thickness = Mathf.Max(1f, shape.lineWeight);

        if (rect.w >= rect.h)
        {
            SetTopLeft(transform, new RectData(rect.x, rect.y + rect.h * 0.5f - thickness * 0.5f, Mathf.Max(rect.w, thickness), thickness));
        }
        else
        {
            SetTopLeft(transform, new RectData(rect.x + rect.w * 0.5f - thickness * 0.5f, rect.y, thickness, Mathf.Max(rect.h, thickness)));
        }

        transform.localRotation = Quaternion.Euler(0f, 0f, -shape.rotation);
        return transform;
    }

    private static TMP_Text CreateText(Transform parent, string name, string value, RectData rect, float fontSize, Color color, FontStyles style, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        SetTopLeft(textObject.GetComponent<RectTransform>(), rect);

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.fontStyle = style;
        text.alignment = alignment;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        text.margin = Vector4.zero;
        if (TMP_Settings.defaultFontAsset != null)
        {
            text.font = TMP_Settings.defaultFontAsset;
        }

        return text;
    }

    private static void AddBorder(Transform parent, RectData rect, Color color, float thickness)
    {
        thickness = Mathf.Max(1f, thickness);
        CreateImage(parent, "Border_Top", new RectData(rect.x, rect.y, rect.w, thickness), color);
        CreateImage(parent, "Border_Bottom", new RectData(rect.x, rect.y + rect.h - thickness, rect.w, thickness), color);
        CreateImage(parent, "Border_Left", new RectData(rect.x, rect.y, thickness, rect.h), color);
        CreateImage(parent, "Border_Right", new RectData(rect.x + rect.w - thickness, rect.y, thickness, rect.h), color);
    }

    private static void BindController(
        HearthFirstPersonHudController controller,
        HearthFirstPersonHudPage[] pages,
        GameObject persistentRoot,
        CanvasGroup persistentGroup,
        CanvasGroup trustGroup,
        TMP_Text trustDeltaText,
        RectTransform menuFocus,
        RectTransform[] menuTargets,
        RectTransform finalFocus,
        RectTransform[] finalTargets,
        HearthDispositionHistoryView historyView,
        HearthSettingsView settingsView,
        HearthPlayerControlLock playerControlLock)
    {
        SerializedObject serialized = new SerializedObject(controller);
        SetObject(serialized, "persistentHudRoot", persistentRoot);
        SetObject(serialized, "persistentHudCanvasGroup", persistentGroup);
        SetObject(serialized, "trustDeltaCanvasGroup", trustGroup);
        SetObject(serialized, "trustDeltaText", trustDeltaText);
        SetObject(serialized, "menuFocusRect", menuFocus);
        SetObjectArray(serialized, "menuFocusTargets", menuTargets);
        SetObject(serialized, "finalChoiceFocusRect", finalFocus);
        SetObjectArray(serialized, "finalChoiceFocusTargets", finalTargets);
        SetObject(serialized, "dispositionHistoryView", historyView);
        SetObject(serialized, "settingsView", settingsView);
        SetObject(serialized, "playerControlLock", playerControlLock);
        SetVector2(serialized, "menuFocusPadding", new Vector2(8f, 4f));
        SetVector2(serialized, "finalChoiceFocusPadding", new Vector2(10f, 6f));
        SetInt(serialized, "finalChoiceTrustThreshold", 3);
        SetInt(serialized, "totalRounds", 3);
        SetPageArray(serialized, pages);
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void BindInput(HearthFirstPersonHudInput input, HearthFirstPersonHudController controller, HearthSettingsView settingsView)
    {
        SerializedObject serialized = new SerializedObject(input);
        SetObject(serialized, "controller", controller);
        SetObject(serialized, "settingsView", settingsView);
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void BindSettingsView(HearthSettingsView settingsView, RectTransform focusRect, RectTransform[] focusTargets)
    {
        SerializedObject serialized = new SerializedObject(settingsView);
        SetObject(serialized, "focusRect", focusRect);
        SetObjectArray(serialized, "focusTargets", focusTargets);
        SetVector2(serialized, "focusPadding", new Vector2(8f, 4f));
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void BindHistoryView(
        HearthDispositionHistoryView historyView,
        List<HearthDispositionHistoryView.RowBinding> rowBindings,
        List<TMP_Text> shiftTrustTexts,
        List<TMP_Text> currentTrustTexts)
    {
        if (historyView == null)
        {
            return;
        }

        SerializedObject serialized = new SerializedObject(historyView);
        SerializedProperty rows = serialized.FindProperty("rowBindings");
        if (rows != null)
        {
            rows.arraySize = rowBindings != null ? rowBindings.Count : 0;
            for (int i = 0; rowBindings != null && i < rowBindings.Count; i++)
            {
                HearthDispositionHistoryView.RowBinding source = rowBindings[i];
                SerializedProperty row = rows.GetArrayElementAtIndex(i);
                row.FindPropertyRelative("recordIndex").intValue = source.recordIndex;
                row.FindPropertyRelative("rowRoot").objectReferenceValue = source.rowRoot;
                row.FindPropertyRelative("timestampText").objectReferenceValue = source.timestampText;
                row.FindPropertyRelative("unitText").objectReferenceValue = source.unitText;
                row.FindPropertyRelative("actionText").objectReferenceValue = source.actionText;
                row.FindPropertyRelative("statusText").objectReferenceValue = source.statusText;
                row.FindPropertyRelative("trustDeltaText").objectReferenceValue = source.trustDeltaText;
            }
        }

        SetObjectArray(serialized, "shiftTrustDeltaTexts", shiftTrustTexts != null ? shiftTrustTexts.ToArray() : Array.Empty<TMP_Text>());
        SetObjectArray(serialized, "currentTrustTexts", currentTrustTexts != null ? currentTrustTexts.ToArray() : Array.Empty<TMP_Text>());
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static HearthHudLayout LoadLayout()
    {
        TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(LayoutJsonPath);
        return asset != null ? JsonUtility.FromJson<HearthHudLayout>(asset.text) : null;
    }

    private static SlideLayout FindSlide(HearthHudLayout layout, int number)
    {
        if (layout == null || layout.slides == null)
        {
            return null;
        }

        for (int i = 0; i < layout.slides.Length; i++)
        {
            if (layout.slides[i] != null && layout.slides[i].number == number)
            {
                return layout.slides[i];
            }
        }

        return null;
    }

    private static bool IsLineShape(ShapeLayout shape)
    {
        return shape.type == 9 || shape.rect.w <= 2.5f || shape.rect.h <= 2.5f;
    }

    private static bool IsHistorySlide(int slideNumber)
    {
        return slideNumber >= 18 && slideNumber <= 21;
    }

    private static bool IsHistoryGeneratedDynamicText(int slideNumber, string value)
    {
        if (!IsHistorySlide(slideNumber) || string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string text = NormalizeHistoryToken(value);
        if (text == "VIEW ARCHIVE" || text == "EARLIER SHIFTS")
        {
            return true;
        }

        if (text == "0" || text == "+1" || text == "-1" || text == "50 / 100" || text == "51 / 100")
        {
            return true;
        }

        if (text == "RECOMMENDED" || text == "·" || text == ".")
        {
            return true;
        }

        if (text.StartsWith("17F-", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (text.StartsWith("2026.") && text.Contains(" · "))
        {
            return true;
        }

        if ((text.StartsWith("+", StringComparison.Ordinal) || text.StartsWith("-", StringComparison.Ordinal)) &&
            text.Contains("TRUST"))
        {
            return true;
        }

        return text.Contains("APPROVE UPGRADE") ||
               text.Contains("RECOMMEND FAMILY COUNSELING") ||
               text.Contains("HONOR USER SHUTDOWN");
    }

    private static string NormalizeHistoryToken(string value)
    {
        return value.Replace("\r", " ")
            .Replace("\n", " ")
            .Trim()
            .ToUpperInvariant();
    }

    private static bool IsVeryDark(string hex)
    {
        Color color = ToColor(hex, 1f);
        return color.r < 0.04f && color.g < 0.04f && color.b < 0.04f;
    }

    private static Color ToFillColor(int slideNumber, ShapeLayout shape)
    {
        Color source = ToColor(shape.fillColor, shape.fillAlpha);
        float alpha = Mathf.Clamp(source.a, 0.08f, 0.72f);

        if (slideNumber >= 11 && slideNumber <= 13)
        {
            if (slideNumber == 13)
            {
                return new Color(0.22f, 0.025f, 0.018f, Mathf.Min(alpha, 0.58f));
            }

            return new Color(0.24f, 0.13f, 0.035f, Mathf.Min(alpha, 0.56f));
        }

        if (slideNumber == 24)
        {
            return new Color(0.035f, 0.075f, 0.105f, Mathf.Min(alpha, 0.62f));
        }

        if (slideNumber >= 18 && slideNumber <= 23)
        {
            return new Color(0.035f, 0.08f, 0.105f, Mathf.Min(alpha, 0.5f));
        }

        if (Luminance(source) > 0.42f || source.a > 0.85f)
        {
            return new Color(0.025f, 0.07f, 0.095f, Mathf.Min(alpha, 0.5f));
        }

        return source;
    }

    private static Color ToLineColor(int slideNumber, ShapeLayout shape)
    {
        Color source = ToColor(shape.lineColor, shape.lineAlpha);
        if (source.a <= 0.01f)
        {
            return source;
        }

        if (slideNumber >= 11 && slideNumber <= 13)
        {
            return slideNumber == 13
                ? new Color(1f, 0.24f, 0.16f, Mathf.Max(source.a, 0.55f))
                : new Color(1f, 0.58f, 0.16f, Mathf.Max(source.a, 0.55f));
        }

        if (Luminance(source) < 0.16f)
        {
            return new Color(0.35f, 0.78f, 0.95f, 0.42f);
        }

        source.a = Mathf.Clamp(source.a, 0.18f, 0.78f);
        return source;
    }

    private static Color ToTextColor(int slideNumber, ShapeLayout shape)
    {
        Color source = ToColor(shape.textColor, shape.textAlpha);
        if (source.a <= 0.01f)
        {
            return source;
        }

        if (slideNumber >= 11 && slideNumber <= 13)
        {
            if (Luminance(source) < 0.28f)
            {
                return slideNumber == 13
                    ? new Color(1f, 0.62f, 0.54f, 0.96f)
                    : new Color(1f, 0.78f, 0.42f, 0.96f);
            }

            source.a = Mathf.Clamp(source.a, 0.72f, 1f);
            return source;
        }

        if (Luminance(source) < 0.28f)
        {
            return new Color(0.78f, 0.93f, 1f, 0.95f);
        }

        source.a = Mathf.Clamp(source.a, 0.72f, 1f);
        return source;
    }

    private static Color ToColor(string hex, float alpha)
    {
        Color color;
        if (!ColorUtility.TryParseHtmlString(hex, out color))
        {
            color = Color.white;
        }

        color.a = Mathf.Clamp01(alpha);
        return color;
    }

    private static float Luminance(Color color)
    {
        return color.r * 0.2126f + color.g * 0.7152f + color.b * 0.0722f;
    }

    private static TextAlignmentOptions ToAlignment(string align)
    {
        if (align == "center")
        {
            return TextAlignmentOptions.Center;
        }

        if (align == "right")
        {
            return TextAlignmentOptions.Right;
        }

        return TextAlignmentOptions.Left;
    }

    private static float Area(RectData rect)
    {
        return rect != null ? rect.w * rect.h : 0f;
    }

    private static RectData Inflate(RectData rect, float horizontal, float vertical)
    {
        if (rect == null)
        {
            return null;
        }

        return new RectData(rect.x - horizontal, rect.y - vertical, rect.w + horizontal * 2f, rect.h + vertical * 2f);
    }

    private static string Normalize(string value)
    {
        return string.IsNullOrEmpty(value)
            ? string.Empty
            : value.Replace("’", "'").Replace("\n", " ").Replace("·", " ").ToUpperInvariant();
    }

    private static string CleanObjectName(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "Item";
        }

        string clean = Normalize(value).Replace(" ", "_").Replace("/", "_");
        char[] invalid = System.IO.Path.GetInvalidFileNameChars();
        for (int i = 0; i < invalid.Length; i++)
        {
            clean = clean.Replace(invalid[i], '_');
        }

        return clean.Length > 42 ? clean.Substring(0, 42) : clean;
    }

    private static string GetPagePrefabName(int slideNumber)
    {
        switch (slideNumber)
        {
            case 3: return "Slide03_MainMenu";
            case 4: return "Slide04_SyncTerminal";
            case 5: return "Slide05_TodayRounds";
            case 6: return "Slide06_HomeWelcome";
            case 7: return "Slide07_Photo2023";
            case 8: return "Slide08_Photo2026";
            case 9: return "Slide09_FinalChoice";
            case 10: return "Slide10_ShutdownConfirm";
            case 11: return "Slide11_Warning01";
            case 12: return "Slide12_Warning02";
            case 13: return "Slide13_Warning03";
            case 14: return "Slide14_FinalChoiceReturn";
            case 15: return "Slide15_EndingGraceful";
            case 16: return "Slide16_EndingForced";
            case 17: return "Slide17_EndingCompanion";
            case 18: return "Slide18_HistoryEmpty";
            case 19: return "Slide19_HistoryOne";
            case 20: return "Slide20_HistoryTwo";
            case 21: return "Slide21_HistoryThree";
            case 22: return "Slide22_Settings";
            case 23: return "Slide23_SettingsFocus";
            case 24: return "Slide24_ExitConfirm";
            default: return "Slide" + slideNumber.ToString("00");
        }
    }

    private static void EnsureAssetFolders()
    {
        EnsureFolder("Assets/Art");
        EnsureFolder("Assets/Art/UI");
        EnsureFolder("Assets/Art/UI/HearthHud");
        EnsureFolder("Assets/Art/UI/HearthHud/FirstPersonLayout");
        EnsureFolder("Assets/Prefabs/UI");
        EnsureFolder("Assets/Prefabs/UI/HearthHud");
        EnsureFolder(PagePrefabDir);
    }

    private static void EnsureFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder))
        {
            return;
        }

        string parent = System.IO.Path.GetDirectoryName(folder).Replace("\\", "/");
        string name = System.IO.Path.GetFileName(folder);
        if (!AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }

        AssetDatabase.CreateFolder(parent, name);
    }

    private static void SetObject(SerializedObject serialized, string propertyName, UnityEngine.Object value)
    {
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
        }
    }

    private static void SetInt(SerializedObject serialized, string propertyName, int value)
    {
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null)
        {
            property.intValue = value;
        }
    }

    private static void SetVector2(SerializedObject serialized, string propertyName, Vector2 value)
    {
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null)
        {
            property.vector2Value = value;
        }
    }

    private static void SetPageArray(SerializedObject serialized, HearthFirstPersonHudPage[] pages)
    {
        SerializedProperty property = serialized.FindProperty("pages");
        property.arraySize = pages.Length;
        for (int i = 0; i < pages.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = pages[i];
        }
    }

    private static void SetObjectArray(SerializedObject serialized, string propertyName, UnityEngine.Object[] values)
    {
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null)
        {
            return;
        }

        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }

    private static void SetFullStretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private static void SetTopLeft(RectTransform rect, RectData pptRect)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(pptRect.x, -pptRect.y);
        rect.sizeDelta = new Vector2(pptRect.w, pptRect.h);
        rect.localScale = Vector3.one;
    }

    [Serializable]
    private sealed class HearthHudLayout
    {
        public int slideCount;
        public int referenceWidth;
        public int referenceHeight;
        public SlideLayout[] slides;
    }

    [Serializable]
    private sealed class SlideLayout
    {
        public int number;
        public ShapeLayout[] shapes;
    }

    [Serializable]
    private sealed class ShapeLayout
    {
        public int index;
        public string name;
        public int type;
        public int autoShapeType;
        public float rotation;
        public RectData rect;
        public bool fillVisible;
        public string fillColor;
        public float fillAlpha;
        public bool lineVisible;
        public string lineColor;
        public float lineAlpha;
        public float lineWeight;
        public string text;
        public RectData textRect;
        public float fontSize;
        public string textColor;
        public float textAlpha;
        public bool bold;
        public string align;
    }

    [Serializable]
    private sealed class RectData
    {
        public float x;
        public float y;
        public float w;
        public float h;

        public RectData()
        {
        }

        public RectData(float newX, float newY, float newW, float newH)
        {
            x = newX;
            y = newY;
            w = newW;
            h = newH;
        }
    }
}
