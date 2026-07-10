#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class HearthTvTerminalPrefabBuilder
{
    private const float EmuPerInch = 914400f;
    private const float PixelsPerInch = 96f;
    private const float ReferenceWidth = 1920f;
    private const float ReferenceHeight = 1080f;
    private const float DefaultTerminalZoom = 1.08f;
    private const string SourcePptxName = "HEARTH-Night-Rounds-Master.pptx";
    private const string PagePrefabFolder = "Assets/Prefabs/UI/HearthHud/TerminalPages";
    private const string TerminalPrefabFolder = "Assets/Prefabs/UI/HearthHud/Terminals";
    private const string Terminal17F01PrefabPath = TerminalPrefabFolder + "/Terminal_17F01.prefab";
    private const string Terminal17F02PrefabPath = TerminalPrefabFolder + "/Terminal_17F02.prefab";
    private const string Terminal17F03PrefabPath = TerminalPrefabFolder + "/Terminal_17F03_Alert.prefab";

    private static readonly XNamespace P = "http://schemas.openxmlformats.org/presentationml/2006/main";
    private static readonly XNamespace A = "http://schemas.openxmlformats.org/drawingml/2006/main";

    [MenuItem("Tools/Hearth HUD/Rebuild TV Terminal UI From Master PPT")]
    public static void RebuildTvTerminalPrefabs()
    {
        string pptxPath = FindPptxPath();
        if (string.IsNullOrEmpty(pptxPath))
        {
            Debug.LogError("[HearthTvTerminalPrefabBuilder] Could not find " + SourcePptxName + ". Put it under UI参考资料 or project root.");
            return;
        }

        EnsureDirectory("Assets/Prefabs");
        EnsureDirectory("Assets/Prefabs/UI");
        EnsureDirectory("Assets/Prefabs/UI/HearthHud");
        EnsureDirectory(PagePrefabFolder);
        EnsureDirectory(TerminalPrefabFolder);

        List<SlideData> slides = ParsePptxSlides(pptxPath, 18);
        if (slides.Count != 18)
        {
            Debug.LogWarning("[HearthTvTerminalPrefabBuilder] Expected 18 terminal slides, parsed " + slides.Count + ".");
        }

        Dictionary<int, GameObject> pagePrefabs = new Dictionary<int, GameObject>();
        Dictionary<int, SlideData> slidesByNumber = new Dictionary<int, SlideData>();
        for (int i = 0; i < slides.Count; i++)
        {
            SlideData slide = slides[i];
            slidesByNumber[slide.Number] = slide;
            int firstSlideNumber = GetGroupFirstSlide(slide.Number);
            GameObject pageObject = BuildPage(slide, firstSlideNumber);
            string pagePath = PagePrefabFolder + "/TerminalSlide" + slide.Number.ToString("00") + "_" + GetTerminalSlideShortName(slide.Number) + ".prefab";
            GameObject savedPagePrefab = PrefabUtility.SaveAsPrefabAsset(pageObject, pagePath);
            pagePrefabs[slide.Number] = savedPagePrefab;
            UnityEngine.Object.DestroyImmediate(pageObject);
        }

        GameObject terminal17F01 = BuildTerminalGroup("Terminal_17F01", 1, HearthHudPageId.Slide01PersistentActive, pagePrefabs, slidesByNumber);
        GameObject terminal17F02 = BuildTerminalGroup("Terminal_17F02", 7, HearthHudPageId.Slide07PersistentDormant, pagePrefabs, slidesByNumber);
        GameObject terminal17F03 = BuildTerminalGroup("Terminal_17F03_Alert", 13, HearthHudPageId.Slide13AlertDoorwaySummary, pagePrefabs, slidesByNumber);

        GameObject saved17F01 = PrefabUtility.SaveAsPrefabAsset(terminal17F01, Terminal17F01PrefabPath);
        PrefabUtility.SaveAsPrefabAsset(terminal17F02, Terminal17F02PrefabPath);
        PrefabUtility.SaveAsPrefabAsset(terminal17F03, Terminal17F03PrefabPath);

        UnityEngine.Object.DestroyImmediate(terminal17F01);
        UnityEngine.Object.DestroyImmediate(terminal17F02);
        UnityEngine.Object.DestroyImmediate(terminal17F03);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = saved17F01;

        Debug.Log("[HearthTvTerminalPrefabBuilder] Rebuilt TV terminal prefabs from " + pptxPath + ".");
    }

    private static GameObject BuildPage(SlideData slide, int firstSlideNumber)
    {
        GameObject pageRoot = new GameObject("TerminalSlide" + slide.Number.ToString("00") + "_" + GetTerminalSlideShortName(slide.Number), typeof(RectTransform), typeof(CanvasGroup), typeof(HearthHudPage));
        StretchToParent(pageRoot.GetComponent<RectTransform>());

        HearthHudPage page = pageRoot.GetComponent<HearthHudPage>();
        page.Configure(
            (HearthHudPageId)slide.Number,
            false,
            slide.Number >= 13 ? HearthHudState.Alert : HearthHudState.Active,
            string.Empty,
            false,
            string.Empty,
            string.Empty);

        GameObject shapeRoot = new GameObject("PptShapes", typeof(RectTransform));
        shapeRoot.transform.SetParent(pageRoot.transform, false);
        StretchToParent(shapeRoot.GetComponent<RectTransform>());

        for (int i = 0; i < slide.Shapes.Count; i++)
        {
            ShapeData shape = slide.Shapes[i];
            if (ShouldSkipTerminalSourceShape(shape))
            {
                continue;
            }

            CreateShapeVisual(shapeRoot.transform, shape, i);
        }

        BuildTerminalInteractions(pageRoot.transform, slide, firstSlideNumber);
        return pageRoot;
    }

    private static GameObject BuildTerminalGroup(string prefabName, int firstSlideNumber, HearthHudPageId startingPage, Dictionary<int, GameObject> pagePrefabs, Dictionary<int, SlideData> slidesByNumber)
    {
        GameObject root = new GameObject(
            prefabName,
            typeof(RectTransform),
            typeof(CanvasGroup),
            typeof(AudioSource),
            typeof(HearthTerminalCameraTransition),
            typeof(HearthTerminalBootSequence),
            typeof(HearthTvTerminalController));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        StretchToParent(rootRect);

        CanvasGroup canvasGroup = root.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        AudioSource audioSource = root.GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        Image screenGlass = CreateImage(root.transform, "TerminalScreenGlass", new Rect(0f, 0f, ReferenceWidth, ReferenceHeight), new Color(0.043f, 0.063f, 0.094f, 0.58f));
        screenGlass.raycastTarget = false;

        GameObject contentObject = new GameObject("TerminalContentRoot", typeof(RectTransform), typeof(CanvasGroup));
        contentObject.transform.SetParent(root.transform, false);
        RectTransform contentRect = contentObject.GetComponent<RectTransform>();
        StretchToParent(contentRect);
        contentRect.localScale = Vector3.one * DefaultTerminalZoom;
        CanvasGroup contentGroup = contentObject.GetComponent<CanvasGroup>();
        contentGroup.alpha = 0f;
        contentGroup.interactable = false;
        contentGroup.blocksRaycasts = false;

        List<HearthHudPage> pages = new List<HearthHudPage>();
        for (int slideNumber = firstSlideNumber; slideNumber < firstSlideNumber + 6; slideNumber++)
        {
            GameObject pagePrefab;
            if (!pagePrefabs.TryGetValue(slideNumber, out pagePrefab) || pagePrefab == null)
            {
                continue;
            }

            GameObject pageInstance = (GameObject)PrefabUtility.InstantiatePrefab(pagePrefab, contentObject.transform);
            pageInstance.name = pagePrefab.name;
            RectTransform pageRect = pageInstance.GetComponent<RectTransform>();
            if (pageRect != null)
            {
                StretchToParent(pageRect);
            }

            HearthHudPage page = pageInstance.GetComponent<HearthHudPage>();
            if (page != null)
            {
                pages.Add(page);
                page.SetVisible((int)page.PageId == (int)startingPage);
            }
        }

        BuildSelectionHighlighter(contentObject.transform, firstSlideNumber, slidesByNumber);
        BuildKeyboardNavigation(root.transform);
        BuildBootOverlays(root.transform, contentGroup, contentRect);

        HearthTvTerminalController controller = root.GetComponent<HearthTvTerminalController>();
        controller.Configure(null, null, contentRect, canvasGroup, pages.ToArray(), firstSlideNumber, startingPage, DefaultTerminalZoom);
        return root;
    }

    private static void BuildKeyboardNavigation(Transform parent)
    {
        GameObject root = new GameObject("KeyboardNavigationRoot", typeof(RectTransform));
        root.transform.SetParent(parent, false);
        StretchToParent(root.GetComponent<RectTransform>());

        CreateImage(root.transform, "KeyboardHintBackplate", new Rect(52f, 990f, 1816f, 46f), new Color(0f, 0f, 0f, 0.28f));
        CreateText(root.transform, "KeyboardHintText", "TAB NEXT PAGE     LEFT/RIGHT SELECT     SPACE CONFIRM     ESC EXIT", new Rect(72f, 1002f, 1160f, 26f), 17f, new Color(0.76f, 0.94f, 0.94f, 0.86f), FontStyles.Normal, TextAlignmentOptions.TopLeft);
        CreateText(root.transform, "KeyboardFocusText", "PAGE 1/5", new Rect(1300f, 1002f, 548f, 26f), 18f, new Color(0.16f, 0.94f, 0.56f, 0.94f), FontStyles.Bold, TextAlignmentOptions.TopRight);
    }

    private static void BuildSelectionHighlighter(Transform contentParent, int firstSlideNumber, Dictionary<int, SlideData> slidesByNumber)
    {
        GameObject root = new GameObject("TerminalSelectionRoot", typeof(RectTransform), typeof(HearthTerminalSelectionHighlighter));
        root.transform.SetParent(contentParent, false);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        StretchToParent(rootRect);

        string[] labels =
        {
            "RESIDENT SUMMARY",
            "ACQUISITION",
            "FAMILY LOG",
            "TRUST TREND",
            "INSPECTION HISTORY"
        };

        List<Rect> targetRects = new List<Rect>();
        for (int i = 0; i < labels.Length; i++)
        {
            Rect fallback = GetFallbackNavigationRect(i);
            targetRects.Add(FindTextRectInGroup(slidesByNumber, firstSlideNumber, labels[i], fallback));
        }

        Rect navigationBounds = BuildNavigationBounds(targetRects);
        Rect replayRect = FindTextRectInGroup(slidesByNumber, firstSlideNumber, "RECALL EVENT", Rect.zero);
        if (!IsRectUsable(replayRect) || !IsRectNearNavigation(replayRect, navigationBounds))
        {
            replayRect = new Rect(navigationBounds.x + navigationBounds.width - 235f, navigationBounds.y + (navigationBounds.height - 26f) * 0.5f, 220f, 26f);
        }

        targetRects.Add(replayRect);

        RectTransform bounds = CreateRectTransform(root.transform, "KeyboardSelectionBounds", navigationBounds);
        Image highlight = CreateImage(root.transform, "KeyboardSelectionHighlight", targetRects[0], new Color(0.1f, 0.95f, 0.58f, 0.12f));
        Outline outline = highlight.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.38f, 1f, 0.72f, 0.42f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        GameObject targetsRoot = new GameObject("SelectionTargets", typeof(RectTransform));
        targetsRoot.transform.SetParent(root.transform, false);
        StretchToParent(targetsRoot.GetComponent<RectTransform>());

        RectTransform[] targets = new RectTransform[targetRects.Count];
        for (int i = 0; i < targetRects.Count; i++)
        {
            targets[i] = CreateRectTransform(targetsRoot.transform, "Target_" + i.ToString("00"), targetRects[i]);
        }

        HearthTerminalSelectionHighlighter highlighter = root.GetComponent<HearthTerminalSelectionHighlighter>();
        highlighter.Configure(highlight.rectTransform, highlight, bounds, targets);
    }

    private static void BuildBootOverlays(Transform parent, CanvasGroup contentGroup, RectTransform contentRect)
    {
        GameObject bootOverlay = new GameObject("TerminalBootOverlay", typeof(RectTransform), typeof(CanvasGroup));
        bootOverlay.transform.SetParent(parent, false);
        StretchToParent(bootOverlay.GetComponent<RectTransform>());
        CanvasGroup bootGroup = bootOverlay.GetComponent<CanvasGroup>();
        bootGroup.alpha = 0f;
        bootGroup.interactable = false;
        bootGroup.blocksRaycasts = false;

        CreateImage(bootOverlay.transform, "BootFlash", new Rect(0f, 0f, ReferenceWidth, ReferenceHeight), new Color(0.55f, 0.95f, 0.85f, 0.18f));

        GameObject scanlineRoot = new GameObject("BootScanlines", typeof(RectTransform));
        scanlineRoot.transform.SetParent(bootOverlay.transform, false);
        StretchToParent(scanlineRoot.GetComponent<RectTransform>());
        for (int y = 0; y < ReferenceHeight; y += 28)
        {
            CreateImage(scanlineRoot.transform, "Scanline_" + y.ToString("0000"), new Rect(0f, y, ReferenceWidth, 2f), new Color(0.68f, 1f, 0.9f, 0.12f));
        }

        GameObject offOverlay = new GameObject("TerminalOffOverlay", typeof(RectTransform), typeof(CanvasGroup));
        offOverlay.transform.SetParent(parent, false);
        StretchToParent(offOverlay.GetComponent<RectTransform>());
        CanvasGroup offGroup = offOverlay.GetComponent<CanvasGroup>();
        offGroup.alpha = 1f;
        offGroup.interactable = false;
        offGroup.blocksRaycasts = false;
        CreateImage(offOverlay.transform, "OffDarkScreen", new Rect(0f, 0f, ReferenceWidth, ReferenceHeight), new Color(0.005f, 0.008f, 0.012f, 0.96f));

        HearthTerminalBootSequence bootSequence = parent.GetComponent<HearthTerminalBootSequence>();
        if (bootSequence != null)
        {
            bootSequence.Configure(contentGroup, offGroup, bootGroup, contentRect);
        }
    }

    private static void BuildTerminalInteractions(Transform pageRoot, SlideData slide, int firstSlideNumber)
    {
        AddTerminalTabButton(pageRoot, slide, "RESIDENT SUMMARY", HearthDoorwayTab.ResidentSummary);
        AddTerminalTabButton(pageRoot, slide, "ACQUISITION", HearthDoorwayTab.Acquisition);
        AddTerminalTabButton(pageRoot, slide, "FAMILY LOG", HearthDoorwayTab.FamilyLog);
        AddTerminalTabButton(pageRoot, slide, "TRUST TREND", HearthDoorwayTab.TrustTrend);
        AddTerminalTabButton(pageRoot, slide, "INSPECTION HISTORY", HearthDoorwayTab.InspectionHistory);

        HearthHudPageId detailPage = ToPageId(firstSlideNumber + 5);
        AddTextButton(pageRoot, slide, "RECALL EVENT", HearthHudButtonActionType.ShowRobotReplay, detailPage);
        AddTextButton(pageRoot, slide, "ENTER UNIT", HearthHudButtonActionType.ShowPage, detailPage);
        AddTextButton(pageRoot, slide, "BACK", HearthHudButtonActionType.ShowPreviousPage, ToPageId(firstSlideNumber));
        AddTextButton(pageRoot, slide, "CLOSE", HearthHudButtonActionType.CloseTerminal, ToPageId(firstSlideNumber));
        AddTextButton(pageRoot, slide, "EXIT", HearthHudButtonActionType.CloseTerminal, ToPageId(firstSlideNumber));
        AddTextButton(pageRoot, slide, "ACCEPT SYSTEM RECOMMENDATION", HearthHudButtonActionType.CloseTerminal, ToPageId(firstSlideNumber), ToPageId(firstSlideNumber), true, -1);
        AddTextButton(pageRoot, slide, "PROMPT FAMILY RESPONSE", HearthHudButtonActionType.CloseTerminal, ToPageId(firstSlideNumber), ToPageId(firstSlideNumber), true, 1);
    }

    private static void AddTerminalTabButton(Transform pageRoot, SlideData slide, string text, HearthDoorwayTab tab)
    {
        Rect rect = FindTextRect(slide, text, Rect.zero);
        if (rect.width <= 0f || rect.height <= 0f)
        {
            return;
        }

        rect = InflateRect(rect, 20f, 12f);
        CreateButton(pageRoot, "Button_Tab_" + CleanObjectName(text), rect, HearthHudButtonActionType.SelectDoorwayTab, HearthHudPageId.Slide01PersistentActive, HearthHudPageId.Slide01PersistentActive, tab, false, 0);
    }

    private static void AddTextButton(Transform pageRoot, SlideData slide, string text, HearthHudButtonActionType action, HearthHudPageId targetPage)
    {
        AddTextButton(pageRoot, slide, text, action, targetPage, targetPage, false, 0);
    }

    private static void AddTextButton(Transform pageRoot, SlideData slide, string text, HearthHudButtonActionType action, HearthHudPageId targetPage, HearthHudPageId replayReturnPage, bool showTrustDelta, int trustDelta)
    {
        Rect rect = FindTextRect(slide, text, Rect.zero);
        if (rect.width <= 0f || rect.height <= 0f)
        {
            return;
        }

        rect = InflateRect(rect, 28f, 16f);
        CreateButton(pageRoot, "Button_" + CleanObjectName(text), rect, action, targetPage, replayReturnPage, HearthDoorwayTab.ResidentSummary, showTrustDelta, trustDelta);
    }

    private static GameObject CreateButton(Transform parent, string name, Rect rect, HearthHudButtonActionType action, HearthHudPageId targetPage, HearthHudPageId replayReturnPage, HearthDoorwayTab tab, bool showTrustDelta, int trustDelta)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(HearthHudButtonAction));
        buttonObject.transform.SetParent(parent, false);
        SetTopLeft(buttonObject.GetComponent<RectTransform>(), rect);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.001f);

        Button button = buttonObject.GetComponent<Button>();
        button.transition = Selectable.Transition.None;

        HearthHudButtonAction actionComponent = buttonObject.GetComponent<HearthHudButtonAction>();
        actionComponent.Configure(null, action, targetPage, replayReturnPage, tab, HearthHudState.Active, string.Empty, showTrustDelta, trustDelta);
        return buttonObject;
    }

    [MenuItem("Tools/Hearth HUD/TV Terminal/Apply 17F-01 To Selected TV")]
    public static void Apply17F01ToSelectedTv()
    {
        StandardizeSelectedTv(Terminal17F01PrefabPath);
    }

    [MenuItem("Tools/Hearth HUD/TV Terminal/Apply 17F-02 To Selected TV")]
    public static void Apply17F02ToSelectedTv()
    {
        StandardizeSelectedTv(Terminal17F02PrefabPath);
    }

    [MenuItem("Tools/Hearth HUD/TV Terminal/Apply 17F-03 Alert To Selected TV")]
    public static void Apply17F03AlertToSelectedTv()
    {
        StandardizeSelectedTv(Terminal17F03PrefabPath);
    }

    public static bool StandardizeTvByHierarchyPath(string hierarchyPath, string terminalPrefabPath)
    {
        Transform target = FindTransformByHierarchyPath(hierarchyPath);
        if (target == null)
        {
            Debug.LogError("[HearthTvTerminalPrefabBuilder] Could not find TV object by hierarchy path: " + hierarchyPath);
            return false;
        }

        return StandardizeTvTerminal(target, terminalPrefabPath);
    }

    public static bool StandardizeTvTerminal(Transform tvTransform, string terminalPrefabPath)
    {
        if (tvTransform == null)
        {
            Debug.LogError("[HearthTvTerminalPrefabBuilder] No TV transform provided.");
            return false;
        }

        GameObject terminalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(terminalPrefabPath);
        if (terminalPrefab == null)
        {
            Debug.LogError("[HearthTvTerminalPrefabBuilder] Missing terminal prefab at " + terminalPrefabPath + ". Rebuild TV terminal prefabs first.");
            return false;
        }

        GameObject canvasObject = FindDirectChild(tvTransform, "MonitorCanvas");
        bool createdCanvas = false;
        if (canvasObject == null)
        {
            canvasObject = new GameObject("MonitorCanvas", typeof(RectTransform));
            canvasObject.transform.SetParent(tvTransform, false);
            createdCanvas = true;
        }

        RectTransform canvasRect = EnsureComponent<RectTransform>(canvasObject);
        canvasRect.sizeDelta = new Vector2(ReferenceWidth, ReferenceHeight);
        if (createdCanvas)
        {
            canvasRect.localPosition = Vector3.zero;
            canvasRect.localRotation = Quaternion.identity;
            canvasRect.localScale = Vector3.one * 0.001f;
        }
        else if (canvasRect.localScale == Vector3.zero)
        {
            canvasRect.localScale = Vector3.one * 0.001f;
        }

        Canvas canvas = EnsureComponent<Canvas>(canvasObject);
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.pixelPerfect = false;

        CanvasScaler scaler = EnsureComponent<CanvasScaler>(canvasObject);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        EnsureComponent<GraphicRaycaster>(canvasObject);
        RemoveGeneratedTerminalChildren(canvasObject.transform);

        GameObject terminalInstance = (GameObject)PrefabUtility.InstantiatePrefab(terminalPrefab, canvasObject.transform);
        terminalInstance.name = terminalPrefab.name;
        RectTransform terminalRect = terminalInstance.GetComponent<RectTransform>();
        if (terminalRect != null)
        {
            StretchToParent(terminalRect);
        }

        HearthTvTerminalController controller = terminalInstance.GetComponent<HearthTvTerminalController>();
        if (controller != null)
        {
            Camera terminalCamera = FindBestTerminalCamera(tvTransform);
            PlayerInteraction playerInteraction = FindBestPlayerInteraction();
            Camera playerCamera = FindBestPlayerCamera(playerInteraction, terminalCamera);
            controller.SetPlayerInteraction(playerInteraction);
            controller.SetPlayerCamera(playerCamera);
            controller.SetTerminalCamera(terminalCamera != null ? terminalCamera : canvas.worldCamera);
            controller.SetSwitchCameraWhileOpen(terminalCamera != null);
            controller.SetMinLoopFlowController(UnityEngine.Object.FindObjectOfType<MinLoopFlowController>());
            controller.SetViewSwitchController(UnityEngine.Object.FindObjectOfType<ViewSwitchController>());
            controller.RefreshPageListFromChildren();

            if (terminalCamera != null)
            {
                terminalCamera.enabled = false;
                AudioListener listener = terminalCamera.GetComponent<AudioListener>();
                if (listener != null)
                {
                    listener.enabled = false;
                }
            }
        }

        HearthTvTerminalInteractable interactable = tvTransform.GetComponent<HearthTvTerminalInteractable>();
        if (interactable == null)
        {
            interactable = tvTransform.gameObject.AddComponent<HearthTvTerminalInteractable>();
        }

        interactable.SetTerminalController(controller);
        EnsureCollider(tvTransform.gameObject);

        EditorUtility.SetDirty(tvTransform.gameObject);
        EditorUtility.SetDirty(canvasObject);
        EditorUtility.SetDirty(terminalInstance);
        EditorSceneManager.MarkSceneDirty(tvTransform.gameObject.scene);

        Debug.Log("[HearthTvTerminalPrefabBuilder] Standardized TV terminal: " + GetHierarchyPath(tvTransform) + " using " + terminalPrefabPath);
        return true;
    }

    private static void StandardizeSelectedTv(string terminalPrefabPath)
    {
        if (Selection.activeTransform == null)
        {
            Debug.LogError("[HearthTvTerminalPrefabBuilder] Select a TV GameObject first.");
            return;
        }

        StandardizeTvTerminal(Selection.activeTransform, terminalPrefabPath);
    }

    private static Camera FindBestTerminalCamera(Transform tvTransform)
    {
        if (tvTransform == null)
        {
            return null;
        }

        Camera[] childCameras = tvTransform.GetComponentsInChildren<Camera>(true);
        Camera namedCamera = FindNamedTerminalCamera(childCameras);
        if (namedCamera != null)
        {
            return namedCamera;
        }

        if (childCameras.Length > 0)
        {
            return childCameras[0];
        }

        Transform parent = tvTransform.parent;
        if (parent != null)
        {
            Camera[] siblingCameras = parent.GetComponentsInChildren<Camera>(true);
            namedCamera = FindNamedTerminalCamera(siblingCameras);
            if (namedCamera != null)
            {
                return namedCamera;
            }
        }

        return null;
    }

    private static PlayerInteraction FindBestPlayerInteraction()
    {
        PlayerInteraction[] interactions = UnityEngine.Object.FindObjectsOfType<PlayerInteraction>(true);
        PlayerInteraction fallback = null;
        PlayerInteraction bestInteraction = null;
        int bestScore = int.MinValue;

        for (int i = 0; i < interactions.Length; i++)
        {
            PlayerInteraction interaction = interactions[i];
            if (interaction == null)
            {
                continue;
            }

            if (fallback == null)
            {
                fallback = interaction;
            }

            if (!IsUsablePlayerCamera(interaction.mainCamera, null))
            {
                continue;
            }

            int score = ScorePlayerInteractionCandidate(interaction);
            if (bestInteraction == null || score > bestScore)
            {
                bestInteraction = interaction;
                bestScore = score;
            }
        }

        return bestInteraction != null ? bestInteraction : fallback;
    }

    private static int ScorePlayerInteractionCandidate(PlayerInteraction interaction)
    {
        if (interaction == null)
        {
            return int.MinValue;
        }

        int score = 0;
        if (interaction.gameObject.activeInHierarchy)
        {
            score += 100;
        }

        if (interaction.enabled)
        {
            score += 100;
        }

        Camera camera = interaction.mainCamera;
        if (camera != null && camera.enabled)
        {
            score += 40;
        }

        string path = GetHierarchyPath(interaction.transform).ToUpperInvariant();
        if (path.Contains("PERSON CONTROLLER"))
        {
            score += 1000;
        }
        else if (path.Contains("PERSON"))
        {
            score += 250;
        }

        if (path.Contains("ROBOT CONTROLLER"))
        {
            score -= 500;
        }

        return score;
    }

    private static Camera FindBestPlayerCamera(PlayerInteraction playerInteraction, Camera terminalCamera)
    {
        if (playerInteraction != null && IsUsablePlayerCamera(playerInteraction.mainCamera, terminalCamera))
        {
            return playerInteraction.mainCamera;
        }

        Camera[] cameras = UnityEngine.Object.FindObjectsOfType<Camera>(true);
        Camera bestCamera = null;
        int bestScore = int.MinValue;

        for (int i = 0; i < cameras.Length; i++)
        {
            Camera camera = cameras[i];
            if (!IsUsablePlayerCamera(camera, terminalCamera))
            {
                continue;
            }

            int score = ScorePlayerCameraCandidate(camera);
            if (bestCamera == null || score > bestScore)
            {
                bestCamera = camera;
                bestScore = score;
            }
        }

        if (bestCamera != null)
        {
            return bestCamera;
        }

        return IsUsablePlayerCamera(Camera.main, terminalCamera) ? Camera.main : null;
    }

    private static int ScorePlayerCameraCandidate(Camera camera)
    {
        int score = 0;
        if (camera.gameObject.activeInHierarchy)
        {
            score += 20;
        }

        if (camera.enabled)
        {
            score += 40;
        }

        AudioListener listener = camera.GetComponent<AudioListener>();
        if (listener != null && listener.enabled)
        {
            score += 30;
        }

        string path = GetHierarchyPath(camera.transform).ToUpperInvariant();
        if (path.Contains("FIRST PERSON"))
        {
            score += 120;
        }

        if (path.Contains("PLAYER") || path.Contains("PERSON"))
        {
            score += 80;
        }

        if (camera.CompareTag("MainCamera"))
        {
            score += 5;
        }

        if (camera.name == "Main Camera")
        {
            score -= 30;
        }

        return score;
    }

    private static bool IsUsablePlayerCamera(Camera camera, Camera terminalCamera)
    {
        if (camera == null || camera == terminalCamera)
        {
            return false;
        }

        if (camera.name == "Terminal Transition Camera")
        {
            return false;
        }

        return !IsLikelyTerminalCamera(camera);
    }

    private static bool IsLikelyTerminalCamera(Camera camera)
    {
        Transform cursor = camera != null ? camera.transform : null;
        while (cursor != null)
        {
            string name = cursor.name.ToUpperInvariant();
            if (name.Contains("TV") || name.Contains("MONITORCANVAS") || name.Contains("TERMINAL_"))
            {
                return true;
            }

            cursor = cursor.parent;
        }

        return false;
    }

    private static Camera FindNamedTerminalCamera(Camera[] cameras)
    {
        if (cameras == null)
        {
            return null;
        }

        for (int i = 0; i < cameras.Length; i++)
        {
            Camera camera = cameras[i];
            if (camera == null)
            {
                continue;
            }

            string name = camera.name.ToUpperInvariant();
            if (name.Contains("TERMINAL") || name.Contains("TV") || name.Contains("MONITOR"))
            {
                return camera;
            }
        }

        return null;
    }

    private static void EnsureCollider(GameObject tvObject)
    {
        if (tvObject.GetComponentInChildren<Collider>() != null)
        {
            return;
        }

        BoxCollider collider = tvObject.AddComponent<BoxCollider>();
        collider.isTrigger = false;
        collider.size = Vector3.one;
    }

    private static void RemoveGeneratedTerminalChildren(Transform canvasTransform)
    {
        for (int i = canvasTransform.childCount - 1; i >= 0; i--)
        {
            Transform child = canvasTransform.GetChild(i);
            bool generatedTerminal =
                child.name.StartsWith("Terminal_", StringComparison.Ordinal) ||
                child.name.StartsWith("TerminalSlide", StringComparison.Ordinal) ||
                child.GetComponent<HearthTvTerminalController>() != null ||
                child.GetComponent<HearthHudPage>() != null;

            if (generatedTerminal)
            {
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }
    }

    private static GameObject FindDirectChild(Transform parent, string childName)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == childName)
            {
                return child.gameObject;
            }
        }

        return null;
    }

    private static Transform FindTransformByHierarchyPath(string hierarchyPath)
    {
        GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Transform result = FindTransformByHierarchyPath(roots[i].transform, hierarchyPath);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static Transform FindTransformByHierarchyPath(Transform root, string hierarchyPath)
    {
        if (GetHierarchyPath(root) == hierarchyPath || root.name == hierarchyPath)
        {
            return root;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform result = FindTransformByHierarchyPath(root.GetChild(i), hierarchyPath);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static string GetHierarchyPath(Transform transform)
    {
        if (transform == null)
        {
            return string.Empty;
        }

        Stack<string> names = new Stack<string>();
        Transform current = transform;
        while (current != null)
        {
            names.Push(current.name);
            current = current.parent;
        }

        return string.Join("/", names.ToArray());
    }

    private static T EnsureComponent<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        if (component == null)
        {
            component = target.AddComponent<T>();
        }

        return component;
    }

    private static void CreateShapeVisual(Transform parent, ShapeData shape, int index)
    {
        bool hasFill = shape.HasFill && shape.FillColor.a > 0.01f && shape.Rect.width > 0.5f && shape.Rect.height > 0.5f;
        bool hasLine = shape.HasLine && shape.LineColor.a > 0.01f;

        if (hasFill)
        {
            CreateImage(parent, "ShapeFill_" + index.ToString("000"), shape.Rect, shape.FillColor);
        }

        if (hasLine && shape.Rect.width > 0.5f && shape.Rect.height > 0.5f)
        {
            AddBorder(parent, shape.Rect, shape.LineColor, Mathf.Max(1f, shape.LineWidth));
        }

        if (!string.IsNullOrWhiteSpace(shape.Text))
        {
            TMP_Text text = CreateText(parent, "Text_" + index.ToString("000") + "_" + CleanObjectName(shape.Text), shape.Text, shape.Rect, shape.FontSize, shape.TextColor, shape.Bold ? FontStyles.Bold : FontStyles.Normal, shape.Alignment);
            text.enableWordWrapping = true;
            text.overflowMode = TextOverflowModes.Overflow;
            text.raycastTarget = false;
        }
    }

    private static bool ShouldSkipTerminalSourceShape(ShapeData shape)
    {
        bool coversSlide = shape.Rect.x <= 2f &&
            shape.Rect.y <= 2f &&
            shape.Rect.width >= ReferenceWidth - 4f &&
            shape.Rect.height >= ReferenceHeight - 4f;

        return coversSlide && shape.HasFill && IsDarkPptBackground(shape.FillColor);
    }

    private static bool IsDarkPptBackground(Color color)
    {
        Color background = new Color(0.043f, 0.063f, 0.094f, 1f);
        return Mathf.Abs(color.r - background.r) < 0.02f &&
            Mathf.Abs(color.g - background.g) < 0.02f &&
            Mathf.Abs(color.b - background.b) < 0.02f;
    }

    private static List<SlideData> ParsePptxSlides(string pptxPath, int count)
    {
        List<SlideData> slides = new List<SlideData>();

        using (ZipArchive archive = ZipFile.OpenRead(pptxPath))
        {
            for (int i = 1; i <= count; i++)
            {
                ZipArchiveEntry entry = archive.GetEntry("ppt/slides/slide" + i.ToString(CultureInfo.InvariantCulture) + ".xml");
                if (entry == null)
                {
                    continue;
                }

                using (Stream stream = entry.Open())
                {
                    XDocument document = XDocument.Load(stream);
                    SlideData slide = new SlideData();
                    slide.Number = i;

                    IEnumerable<XElement> shapes = document.Descendants(P + "sp");
                    foreach (XElement element in shapes)
                    {
                        ShapeData shape;
                        if (TryParseShape(element, out shape))
                        {
                            slide.Shapes.Add(shape);
                        }
                    }

                    slides.Add(slide);
                }
            }
        }

        return slides;
    }

    private static bool TryParseShape(XElement element, out ShapeData shape)
    {
        shape = new ShapeData();
        Rect rect;
        if (!TryParseRect(element, out rect))
        {
            return false;
        }

        shape.Rect = rect;
        shape.Name = GetShapeName(element);
        shape.Text = GetShapeText(element);
        shape.FontSize = GetFontSize(element);
        shape.TextColor = GetTextColor(element);
        shape.Bold = GetBold(element);
        shape.Alignment = GetAlignment(element);

        XElement spPr = element.Element(P + "spPr");
        if (spPr != null)
        {
            Color fill;
            if (TryGetSolidFillColor(spPr, out fill) && spPr.Element(A + "noFill") == null)
            {
                shape.HasFill = true;
                shape.FillColor = fill;
            }

            XElement line = spPr.Element(A + "ln");
            if (line != null)
            {
                Color lineColor;
                if (TryGetSolidFillColor(line, out lineColor))
                {
                    shape.HasLine = true;
                    shape.LineColor = lineColor;
                    XAttribute widthAttribute = line.Attribute("w");
                    if (widthAttribute != null)
                    {
                        float emu;
                        if (float.TryParse(widthAttribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out emu))
                        {
                            shape.LineWidth = Mathf.Max(1f, EmuToPixels(emu));
                        }
                    }
                    else
                    {
                        shape.LineWidth = 1f;
                    }
                }
            }
        }

        return shape.HasFill || shape.HasLine || !string.IsNullOrWhiteSpace(shape.Text);
    }

    private static bool TryParseRect(XElement element, out Rect rect)
    {
        rect = Rect.zero;
        XElement transform = element.Descendants(A + "xfrm").FirstOrDefault();
        if (transform == null)
        {
            return false;
        }

        XElement off = transform.Element(A + "off");
        XElement ext = transform.Element(A + "ext");
        if (off == null || ext == null)
        {
            return false;
        }

        float x = GetFloatAttribute(off, "x");
        float y = GetFloatAttribute(off, "y");
        float w = GetFloatAttribute(ext, "cx");
        float h = GetFloatAttribute(ext, "cy");
        rect = new Rect(EmuToPixels(x), EmuToPixels(y), EmuToPixels(w), EmuToPixels(h));
        return rect.width > 0.25f && rect.height > 0.25f;
    }

    private static string GetShapeName(XElement element)
    {
        XElement nonVisual = element.Descendants(P + "cNvPr").FirstOrDefault();
        XAttribute name = nonVisual != null ? nonVisual.Attribute("name") : null;
        return name != null ? name.Value : "Shape";
    }

    private static string GetShapeText(XElement element)
    {
        List<string> paragraphs = new List<string>();
        foreach (XElement paragraph in element.Descendants(A + "p"))
        {
            string text = string.Concat(paragraph.Descendants(A + "t").Select(t => t.Value));
            if (!string.IsNullOrWhiteSpace(text))
            {
                paragraphs.Add(text.Trim());
            }
        }

        return string.Join("\n", paragraphs.ToArray()).Trim();
    }

    private static float GetFontSize(XElement element)
    {
        XElement runProperties = element.Descendants(A + "rPr").FirstOrDefault(r => r.Attribute("sz") != null);
        if (runProperties == null)
        {
            runProperties = element.Descendants(A + "defRPr").FirstOrDefault(r => r.Attribute("sz") != null);
        }

        if (runProperties != null)
        {
            XAttribute sizeAttribute = runProperties.Attribute("sz");
            float size;
            if (sizeAttribute != null && float.TryParse(sizeAttribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out size))
            {
                return Mathf.Clamp(size / 100f * (PixelsPerInch / 72f), 8f, 78f);
            }
        }

        return 18f;
    }

    private static Color GetTextColor(XElement element)
    {
        XElement runProperties = element.Descendants(A + "rPr").FirstOrDefault();
        if (runProperties != null)
        {
            Color color;
            if (TryGetSolidFillColor(runProperties, out color))
            {
                return color;
            }
        }

        XElement defaultRunProperties = element.Descendants(A + "defRPr").FirstOrDefault();
        if (defaultRunProperties != null)
        {
            Color color;
            if (TryGetSolidFillColor(defaultRunProperties, out color))
            {
                return color;
            }
        }

        return new Color(0.86f, 0.96f, 0.95f, 0.96f);
    }

    private static bool GetBold(XElement element)
    {
        XElement runProperties = element.Descendants(A + "rPr").FirstOrDefault();
        XAttribute bold = runProperties != null ? runProperties.Attribute("b") : null;
        return bold != null && (bold.Value == "1" || string.Equals(bold.Value, "true", StringComparison.OrdinalIgnoreCase));
    }

    private static TextAlignmentOptions GetAlignment(XElement element)
    {
        XElement paragraphProperties = element.Descendants(A + "pPr").FirstOrDefault();
        string align = paragraphProperties != null && paragraphProperties.Attribute("algn") != null
            ? paragraphProperties.Attribute("algn").Value
            : string.Empty;

        if (align == "ctr")
        {
            return TextAlignmentOptions.Top;
        }

        if (align == "r")
        {
            return TextAlignmentOptions.TopRight;
        }

        return TextAlignmentOptions.TopLeft;
    }

    private static bool TryGetSolidFillColor(XElement parent, out Color color)
    {
        color = Color.clear;
        XElement solidFill = parent.Element(A + "solidFill");
        if (solidFill == null)
        {
            return false;
        }

        XElement srgb = solidFill.Element(A + "srgbClr");
        if (srgb != null)
        {
            XAttribute value = srgb.Attribute("val");
            if (value != null && TryParseHexColor(value.Value, out color))
            {
                color.a = GetAlpha(srgb, color.a);
                return true;
            }
        }

        XElement scheme = solidFill.Element(A + "schemeClr");
        if (scheme != null)
        {
            XAttribute value = scheme.Attribute("val");
            color = SchemeColor(value != null ? value.Value : string.Empty);
            color.a = GetAlpha(scheme, color.a);
            return true;
        }

        return false;
    }

    private static float GetAlpha(XElement colorElement, float fallback)
    {
        XElement alpha = colorElement.Element(A + "alpha");
        if (alpha == null)
        {
            return fallback;
        }

        XAttribute value = alpha.Attribute("val");
        float raw;
        if (value != null && float.TryParse(value.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out raw))
        {
            return Mathf.Clamp01(raw / 100000f);
        }

        return fallback;
    }

    private static bool TryParseHexColor(string hex, out Color color)
    {
        color = Color.white;
        if (string.IsNullOrEmpty(hex) || hex.Length < 6)
        {
            return false;
        }

        int r = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        int g = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        int b = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        color = new Color(r / 255f, g / 255f, b / 255f, 1f);
        return true;
    }

    private static Color SchemeColor(string key)
    {
        switch (key)
        {
            case "bg1":
            case "tx2":
                return new Color(0.02f, 0.03f, 0.035f, 1f);
            case "tx1":
            case "bg2":
                return new Color(0.92f, 0.97f, 0.96f, 1f);
            case "accent1":
                return new Color(0.11f, 0.92f, 0.55f, 1f);
            case "accent2":
                return new Color(1f, 0.22f, 0.18f, 1f);
            case "accent3":
                return new Color(0.38f, 0.62f, 0.78f, 1f);
            case "accent4":
                return new Color(1f, 0.55f, 0.16f, 1f);
            default:
                return new Color(0.86f, 0.96f, 0.95f, 1f);
        }
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

    private static TMP_Text CreateText(Transform parent, string name, string value, Rect rect, float fontSize, Color color, FontStyles style, TextAlignmentOptions alignment)
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
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        if (TMP_Settings.defaultFontAsset != null)
        {
            text.font = TMP_Settings.defaultFontAsset;
        }

        return text;
    }

    private static RectTransform CreateRectTransform(Transform parent, string name, Rect rect)
    {
        GameObject rectObject = new GameObject(name, typeof(RectTransform));
        rectObject.transform.SetParent(parent, false);
        RectTransform rectTransform = rectObject.GetComponent<RectTransform>();
        SetTopLeft(rectTransform, rect);
        return rectTransform;
    }

    private static void AddBorder(Transform parent, Rect rect, Color color, float thickness)
    {
        thickness = Mathf.Max(1f, thickness);
        CreateImage(parent, "Border_Top", new Rect(rect.x, rect.y, rect.width, thickness), color);
        CreateImage(parent, "Border_Bottom", new Rect(rect.x, rect.y + rect.height - thickness, rect.width, thickness), color);
        CreateImage(parent, "Border_Left", new Rect(rect.x, rect.y, thickness, rect.height), color);
        CreateImage(parent, "Border_Right", new Rect(rect.x + rect.width - thickness, rect.y, thickness, rect.height), color);
    }

    private static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.anchoredPosition = Vector2.zero;
    }

    private static void SetTopLeft(RectTransform rect, Rect pptRect)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(pptRect.x, -pptRect.y);
        rect.sizeDelta = new Vector2(pptRect.width, pptRect.height);
        rect.localScale = Vector3.one;
    }

    private static Rect FindTextRect(SlideData slide, string contains, Rect fallback)
    {
        ShapeData best = null;
        string normalizedNeedle = NormalizeText(contains);
        for (int i = 0; i < slide.Shapes.Count; i++)
        {
            ShapeData shape = slide.Shapes[i];
            if (string.IsNullOrEmpty(shape.Text))
            {
                continue;
            }

            if (NormalizeText(shape.Text).Contains(normalizedNeedle))
            {
                if (best == null || shape.Rect.width * shape.Rect.height > best.Rect.width * best.Rect.height)
                {
                    best = shape;
                }
            }
        }

        return best != null ? best.Rect : fallback;
    }

    private static Rect FindTextRectInGroup(Dictionary<int, SlideData> slidesByNumber, int firstSlideNumber, string contains, Rect fallback)
    {
        Rect bestRect = Rect.zero;
        float bestArea = -1f;

        for (int slideNumber = firstSlideNumber; slideNumber < firstSlideNumber + 6; slideNumber++)
        {
            SlideData slide;
            if (slidesByNumber == null || !slidesByNumber.TryGetValue(slideNumber, out slide) || slide == null)
            {
                continue;
            }

            Rect rect = FindTextRect(slide, contains, Rect.zero);
            if (!IsRectUsable(rect))
            {
                continue;
            }

            float area = rect.width * rect.height;
            if (area > bestArea)
            {
                bestArea = area;
                bestRect = rect;
            }
        }

        return IsRectUsable(bestRect) ? bestRect : fallback;
    }

    private static Rect BuildNavigationBounds(List<Rect> targetRects)
    {
        Rect bounds = Rect.zero;
        bool hasBounds = false;

        for (int i = 0; i < targetRects.Count; i++)
        {
            Rect rect = targetRects[i];
            if (!IsRectUsable(rect))
            {
                continue;
            }

            if (!hasBounds)
            {
                bounds = rect;
                hasBounds = true;
                continue;
            }

            bounds = Union(bounds, rect);
        }

        if (!hasBounds)
        {
            bounds = new Rect(140f, 112f, 1540f, 44f);
        }

        bounds = InflateRect(bounds, 18f, 10f);
        bounds.x = Mathf.Clamp(bounds.x, 0f, ReferenceWidth - 1f);
        bounds.y = Mathf.Clamp(bounds.y, 0f, ReferenceHeight - 1f);
        bounds.width = Mathf.Min(bounds.width, ReferenceWidth - bounds.x);
        bounds.height = Mathf.Min(Mathf.Max(36f, bounds.height), ReferenceHeight - bounds.y);
        return bounds;
    }

    private static Rect Union(Rect a, Rect b)
    {
        float xMin = Mathf.Min(a.xMin, b.xMin);
        float yMin = Mathf.Min(a.yMin, b.yMin);
        float xMax = Mathf.Max(a.xMax, b.xMax);
        float yMax = Mathf.Max(a.yMax, b.yMax);
        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    private static Rect GetFallbackNavigationRect(int index)
    {
        switch (index)
        {
            case 0:
                return new Rect(180f, 118f, 220f, 28f);
            case 1:
                return new Rect(455f, 118f, 160f, 28f);
            case 2:
                return new Rect(690f, 118f, 150f, 28f);
            case 3:
                return new Rect(910f, 118f, 160f, 28f);
            case 4:
                return new Rect(1140f, 118f, 250f, 28f);
            default:
                return new Rect(1500f, 118f, 220f, 28f);
        }
    }

    private static bool IsRectNearNavigation(Rect rect, Rect navigationBounds)
    {
        if (!IsRectUsable(rect))
        {
            return false;
        }

        float rectCenterY = rect.y + rect.height * 0.5f;
        float boundsCenterY = navigationBounds.y + navigationBounds.height * 0.5f;
        return Mathf.Abs(rectCenterY - boundsCenterY) <= 96f;
    }

    private static bool IsRectUsable(Rect rect)
    {
        return rect.width > 0.5f && rect.height > 0.5f;
    }

    private static Rect InflateRect(Rect rect, float horizontal, float vertical)
    {
        rect.x -= horizontal;
        rect.y -= vertical;
        rect.width += horizontal * 2f;
        rect.height += vertical * 2f;
        return rect;
    }

    private static HearthHudPageId ToPageId(int slideNumber)
    {
        return (HearthHudPageId)slideNumber;
    }

    private static int GetGroupFirstSlide(int slideNumber)
    {
        if (slideNumber >= 13)
        {
            return 13;
        }

        if (slideNumber >= 7)
        {
            return 7;
        }

        return 1;
    }

    private static string GetTerminalSlideShortName(int slideNumber)
    {
        int group = GetGroupFirstSlide(slideNumber);
        int local = slideNumber - group + 1;
        string room = group == 13 ? "17F03Alert" : group == 7 ? "17F02" : "17F01";
        switch (local)
        {
            case 1: return room + "_ResidentSummary";
            case 2: return room + "_Acquisition";
            case 3: return room + "_FamilyLog";
            case 4: return room + "_TrustTrend";
            case 5: return room + "_InspectionHistory";
            case 6: return room + "_Action";
            default: return room + "_Page";
        }
    }

    private static float EmuToPixels(float emu)
    {
        return emu / EmuPerInch * PixelsPerInch;
    }

    private static float GetFloatAttribute(XElement element, string attributeName)
    {
        XAttribute attribute = element.Attribute(attributeName);
        if (attribute == null)
        {
            return 0f;
        }

        float value;
        return float.TryParse(attribute.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out value) ? value : 0f;
    }

    private static string NormalizeText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        string value = text.Replace("·", " ").Replace("’", "'").Replace("\n", " ");
        while (value.Contains("  "))
        {
            value = value.Replace("  ", " ");
        }

        return value.ToUpperInvariant();
    }

    private static string CleanObjectName(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "Item";
        }

        char[] invalid = Path.GetInvalidFileNameChars();
        string shortValue = value.Replace("\n", "_").Replace(" ", "_").Replace("·", "_");
        for (int i = 0; i < invalid.Length; i++)
        {
            shortValue = shortValue.Replace(invalid[i], '_');
        }

        if (shortValue.Length > 42)
        {
            shortValue = shortValue.Substring(0, 42);
        }

        return shortValue;
    }

    private static void EnsureDirectory(string assetDirectory)
    {
        if (AssetDatabase.IsValidFolder(assetDirectory))
        {
            return;
        }

        string parent = Path.GetDirectoryName(assetDirectory).Replace("\\", "/");
        string name = Path.GetFileName(assetDirectory);
        if (!AssetDatabase.IsValidFolder(parent))
        {
            EnsureDirectory(parent);
        }

        AssetDatabase.CreateFolder(parent, name);
    }

    private static string FindPptxPath()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string directReference = Path.Combine(projectRoot, "UI参考资料", SourcePptxName);
        if (File.Exists(directReference))
        {
            return directReference;
        }

        string[] uiFolders = Directory.GetDirectories(projectRoot, "UI*", SearchOption.TopDirectoryOnly);
        for (int i = 0; i < uiFolders.Length; i++)
        {
            string candidate = Path.Combine(uiFolders[i], SourcePptxName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        string direct = Path.Combine(projectRoot, SourcePptxName);
        if (File.Exists(direct))
        {
            return direct;
        }

        string desktop = Path.Combine("E:\\桌面", SourcePptxName);
        return File.Exists(desktop) ? desktop : string.Empty;
    }

    private sealed class SlideData
    {
        public int Number;
        public readonly List<ShapeData> Shapes = new List<ShapeData>();
    }

    private sealed class ShapeData
    {
        public string Name;
        public Rect Rect;
        public string Text;
        public float FontSize = 18f;
        public Color TextColor = Color.white;
        public bool Bold;
        public TextAlignmentOptions Alignment = TextAlignmentOptions.TopLeft;
        public bool HasFill;
        public Color FillColor = Color.clear;
        public bool HasLine;
        public Color LineColor = Color.clear;
        public float LineWidth = 1f;
    }
}
#endif
