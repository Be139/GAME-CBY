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
using UnityEngine;
using UnityEngine.UI;

public static class HearthHudPrefabBuilder
{
    private const float EmuPerInch = 914400f;
    private const float PixelsPerInch = 96f;
    private const float ReferenceWidth = 1920f;
    private const float ReferenceHeight = 1080f;
    private const string ComponentAssetFolder = "Assets/Resources/UI/HearthHudComponents";
    private const string RootPrefabPath = "Assets/Prefabs/UI/HearthHud/HearthHudRoot.prefab";
    private const string PagePrefabFolder = "Assets/Prefabs/UI/HearthHud/Pages";

    private static readonly XNamespace P = "http://schemas.openxmlformats.org/presentationml/2006/main";
    private static readonly XNamespace A = "http://schemas.openxmlformats.org/drawingml/2006/main";

    [MenuItem("Tools/Hearth HUD/Rebuild Formal HUD Prefabs")]
    public static void RebuildFormalHudPrefabs()
    {
        string pptxPath = FindPptxPath();
        if (string.IsNullOrEmpty(pptxPath))
        {
            Debug.LogError("[HearthHudPrefabBuilder] Could not find HEARTH-HUD.pptx under the project root.");
            return;
        }

        EnsureDirectory("Assets/Prefabs");
        EnsureDirectory("Assets/Prefabs/UI");
        EnsureDirectory("Assets/Prefabs/UI/HearthHud");
        EnsureDirectory(PagePrefabFolder);

        ImportComponentPngsAsSprites();

        List<SlideData> slides = ParsePptxSlides(pptxPath, 24);
        if (slides.Count == 0)
        {
            Debug.LogError("[HearthHudPrefabBuilder] No slides were parsed from " + pptxPath);
            return;
        }

        GameObject root = BuildRoot(slides);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, RootPrefabPath);
        UnityEngine.Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = prefab;

        Debug.Log("[HearthHudPrefabBuilder] Rebuilt formal Hearth HUD prefabs from " + pptxPath + ". Root prefab: " + RootPrefabPath);
    }

    private static GameObject BuildRoot(List<SlideData> slides)
    {
        GameObject root = new GameObject("HearthHudRoot", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(HearthHudController), typeof(HearthHudPreviewInput));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        StretchToParent(rootRect);

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 6000;

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform persistentLayer = CreateLayer(root.transform, "PersistentHudLayer");
        RectTransform panelLayer = CreateLayer(root.transform, "PanelLayer");
        RectTransform fullscreenLayer = CreateLayer(root.transform, "FullscreenTakeoverLayer");
        RectTransform subtitleLayer = CreateLayer(root.transform, "SubtitleLayer");
        RectTransform debugLayer = CreateLayer(root.transform, "DebugPreviewLayer");

        HearthHudPersistentView persistentView = BuildPersistentHud(persistentLayer);

        List<HearthHudPage> pageInstances = new List<HearthHudPage>();
        for (int i = 0; i < slides.Count; i++)
        {
            SlideData slide = slides[i];
            GameObject pageObject = BuildPage(slide);
            string pagePrefabPath = PagePrefabFolder + "/Slide" + slide.Number.ToString("00") + "_" + GetPageShortName(slide.Number) + ".prefab";
            GameObject savedPagePrefab = PrefabUtility.SaveAsPrefabAsset(pageObject, pagePrefabPath);

            RectTransform targetLayer = UsesFullscreenLayer(slide.Number) ? fullscreenLayer : panelLayer;
            GameObject pageInstance = (GameObject)PrefabUtility.InstantiatePrefab(savedPagePrefab, targetLayer);
            pageInstance.name = savedPagePrefab.name;
            HearthHudPage page = pageInstance.GetComponent<HearthHudPage>();
            if (page != null)
            {
                pageInstances.Add(page);
            }

            UnityEngine.Object.DestroyImmediate(pageObject);
        }

        HearthHudController controller = root.GetComponent<HearthHudController>();
        controller.Configure(canvas, scaler, persistentLayer, panelLayer, fullscreenLayer, subtitleLayer, debugLayer, persistentView, pageInstances.ToArray());

        HearthHudPreviewInput previewInput = root.GetComponent<HearthHudPreviewInput>();
        previewInput.SetPreviewInputEnabled(true);

        return root;
    }

    private static HearthHudPersistentView BuildPersistentHud(RectTransform parent)
    {
        GameObject root = new GameObject("PersistentHudView", typeof(RectTransform), typeof(HearthHudPersistentView));
        root.transform.SetParent(parent, false);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        StretchToParent(rootRect);

        Image badge = CreateImage(root.transform, "WorkerBadge_Backplate", new Rect(34f, 38f, 430f, 96f), new Color(0.02f, 0.05f, 0.05f, 0.72f));
        AddBorder(root.transform, new Rect(34f, 38f, 430f, 96f), new Color(0.16f, 0.94f, 0.56f, 0.45f), 2f);

        Image dot = CreateImage(root.transform, "StatusDot", new Rect(54f, 66f, 14f, 14f), new Color(0.08f, 0.94f, 0.54f, 1f));
        TMP_Text status = CreateText(root.transform, "StatusText", "COMPANION UNIT | ACTIVE", new Rect(78f, 56f, 330f, 24f), 18f, Color.white, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        TMP_Text worker = CreateText(root.transform, "WorkerNameText", "MIA · 7842", new Rect(78f, 88f, 260f, 28f), 23f, new Color(0.88f, 0.95f, 1f, 0.92f), FontStyles.Normal, TextAlignmentOptions.TopLeft);

        TMP_Text clock = CreateText(root.transform, "ClockText", "2026.09.15·MON·18:47", new Rect(1428f, 38f, 444f, 30f), 20f, new Color(0.85f, 0.94f, 0.92f, 0.9f), FontStyles.Normal, TextAlignmentOptions.TopRight);
        GameObject taskRoot = new GameObject("TaskRoot", typeof(RectTransform));
        taskRoot.transform.SetParent(root.transform, false);
        SetTopLeft(taskRoot.GetComponent<RectTransform>(), new Rect(1388f, 78f, 484f, 72f));
        AddBorder(taskRoot.transform, new Rect(0f, 0f, 484f, 72f), new Color(0.16f, 0.94f, 0.56f, 0.32f), 2f);
        CreateText(taskRoot.transform, "TaskKicker", "CURRENT TASK", new Rect(16f, 10f, 250f, 20f), 13f, new Color(0.48f, 0.68f, 0.68f, 0.86f), FontStyles.Bold, TextAlignmentOptions.TopLeft);
        TMP_Text taskText = CreateText(taskRoot.transform, "TaskText", "NIGHT ROUNDS · BLOCK A · 17F", new Rect(16f, 34f, 438f, 26f), 17f, new Color(0.88f, 1f, 0.94f, 0.94f), FontStyles.Normal, TextAlignmentOptions.TopLeft);

        GameObject subtitleRoot = new GameObject("SubtitleRoot", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        subtitleRoot.transform.SetParent(root.transform, false);
        SetTopLeft(subtitleRoot.GetComponent<RectTransform>(), new Rect(500f, 910f, 920f, 72f));
        Image subtitleBack = subtitleRoot.GetComponent<Image>();
        subtitleBack.color = new Color(0f, 0f, 0f, 0.56f);
        CanvasGroup subtitleGroup = subtitleRoot.GetComponent<CanvasGroup>();
        subtitleGroup.alpha = 0f;
        subtitleRoot.SetActive(false);
        TMP_Text subtitle = CreateText(subtitleRoot.transform, "SubtitleText", string.Empty, new Rect(24f, 16f, 872f, 42f), 21f, new Color(0.92f, 1f, 0.98f, 0.96f), FontStyles.Normal, TextAlignmentOptions.Center);

        GameObject trustRoot = new GameObject("TrustDeltaRoot", typeof(RectTransform), typeof(CanvasGroup));
        trustRoot.transform.SetParent(root.transform, false);
        SetTopLeft(trustRoot.GetComponent<RectTransform>(), new Rect(860f, 142f, 220f, 64f));
        CanvasGroup trustGroup = trustRoot.GetComponent<CanvasGroup>();
        trustGroup.alpha = 0f;
        TMP_Text trustText = CreateText(trustRoot.transform, "TrustDeltaText", "+1", new Rect(0f, 0f, 220f, 64f), 38f, new Color(0.3f, 1f, 0.62f, 1f), FontStyles.Bold, TextAlignmentOptions.Center);
        trustRoot.SetActive(false);

        HearthHudPersistentView view = root.GetComponent<HearthHudPersistentView>();
        view.ConfigureBindings(root, badge, dot, status, worker, clock, taskRoot, taskText, subtitleRoot, subtitle, subtitleGroup, trustRoot, trustText, trustGroup);
        return view;
    }

    private static GameObject BuildPage(SlideData slide)
    {
        GameObject pageRoot = new GameObject("Slide" + slide.Number.ToString("00") + "_" + GetPageShortName(slide.Number), typeof(RectTransform), typeof(CanvasGroup), typeof(HearthHudPage));
        RectTransform pageRect = pageRoot.GetComponent<RectTransform>();
        StretchToParent(pageRect);

        HearthHudPage page = pageRoot.GetComponent<HearthHudPage>();
        HearthHudPageMetadata metadata = BuildMetadata(slide);
        page.Configure(metadata.pageId, metadata.showPersistentHud, metadata.hudState, metadata.clockText, metadata.showTask, metadata.taskText, metadata.subtitleText);

        GameObject shapeRoot = new GameObject("PptShapes", typeof(RectTransform));
        shapeRoot.transform.SetParent(pageRoot.transform, false);
        StretchToParent(shapeRoot.GetComponent<RectTransform>());

        for (int i = 0; i < slide.Shapes.Count; i++)
        {
            ShapeData shape = slide.Shapes[i];
            if (ShouldSkipSourceShape(slide.Number, metadata, shape))
            {
                continue;
            }

            CreateShapeVisual(shapeRoot.transform, shape, i);
        }

        BuildPageInteractions(pageRoot.transform, slide);
        return pageRoot;
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

    private static void BuildPageInteractions(Transform pageRoot, SlideData slide)
    {
        int page = slide.Number;
        if (page == 6)
        {
            Rect holdRect = FindTextRect(slide, "HOLD E", new Rect(742f, 864f, 440f, 78f));
            GameObject hold = CreateButton(pageRoot, "HoldToActButton", holdRect, HearthHudButtonActionType.None, HearthHudPageId.Slide01PersistentActive, HearthHudPageId.Slide05DoorwayDisposition, HearthDoorwayTab.ResidentSummary, false, 0);
            Image fill = CreateImage(hold.transform, "HoldProgressFill", new Rect(16f, holdRect.height - 12f, holdRect.width - 32f, 6f), new Color(0.16f, 0.94f, 0.56f, 0.82f));
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = 0;
            fill.fillAmount = 0f;
            TMP_Text label = CreateText(hold.transform, "HoldProgressLabel", "HOLD E  00%", new Rect(0f, 0f, holdRect.width, holdRect.height), 18f, new Color(0.9f, 1f, 0.94f, 0.96f), FontStyles.Bold, TextAlignmentOptions.Center);
            hold.AddComponent<HearthHoldToActButton>().Configure(null, fill, label);
        }

        if (page == 2)
        {
            AddTextButton(pageRoot, slide, "CONFIRM", HearthHudButtonActionType.ShowPage, HearthHudPageId.Slide01PersistentActive);
            AddTextButton(pageRoot, slide, "BEGIN", HearthHudButtonActionType.ShowPage, HearthHudPageId.Slide01PersistentActive);
        }

        if (page == 3 || page == 4 || page == 5)
        {
            AddTerminalTabButtons(pageRoot, slide, false);
            AddTextButton(pageRoot, slide, "RECALL EVENT", HearthHudButtonActionType.ShowRobotReplay, HearthHudPageId.Slide06RobotReplay, HearthHudPageId.Slide05DoorwayDisposition);
        }

        if (page == 5)
        {
            AddTextButton(pageRoot, slide, "ACCEPT SYSTEM RECOMMENDATION", HearthHudButtonActionType.ShowPage, HearthHudPageId.Slide01PersistentActive, HearthHudPageId.Slide05DoorwayDisposition, true, -1);
            AddTextButton(pageRoot, slide, "PROMPT FAMILY RESPONSE", HearthHudButtonActionType.ShowPage, HearthHudPageId.Slide01PersistentActive, HearthHudPageId.Slide05DoorwayDisposition, true, 1);
        }

        if (page == 8)
        {
            AddTextButton(pageRoot, slide, "ANSWER LILY YOURSELF", HearthHudButtonActionType.ShowPage, HearthHudPageId.Slide22ShutdownConfirm);
            AddTextButton(pageRoot, slide, "LET SYSTEM HANDLE IT", HearthHudButtonActionType.ShowPage, HearthHudPageId.Slide01PersistentActive);
        }

        if (page == 9)
        {
            AddTextButton(pageRoot, slide, "YES", HearthHudButtonActionType.ShowPage, HearthHudPageId.Slide01PersistentActive);
            AddTextButton(pageRoot, slide, "CANCEL", HearthHudButtonActionType.ShowPage, HearthHudPageId.Slide08FinalChoice);
        }

        if (page == 11)
        {
            AddTextButton(pageRoot, slide, "TODAY'S ROUNDS", HearthHudButtonActionType.ShowPage, HearthHudPageId.Slide12WorkspacePanel);
            AddTextButton(pageRoot, slide, "TODAY’S ROUNDS", HearthHudButtonActionType.ShowPage, HearthHudPageId.Slide12WorkspacePanel);
            AddTextButton(pageRoot, slide, "DISPOSITION HISTORY", HearthHudButtonActionType.ShowPage, HearthHudPageId.Slide12WorkspacePanel);
            AddTextButton(pageRoot, slide, "SYSTEM SETTINGS", HearthHudButtonActionType.ShowPage, HearthHudPageId.Slide12WorkspacePanel);
        }

        if (page >= 13 && page <= 18)
        {
            AddTerminalTabButtons(pageRoot, slide, true);
            AddTextButton(pageRoot, slide, "ENTER UNIT", HearthHudButtonActionType.ShowPage, HearthHudPageId.Slide19IndoorSidePanel);
            AddTextButton(pageRoot, slide, "RECALL EVENT", HearthHudButtonActionType.ShowRobotReplay, HearthHudPageId.Slide06RobotReplay, HearthHudPageId.Slide18AlertDisposition);
        }

        if (page == 18)
        {
            AddTextButton(pageRoot, slide, "ACCEPT SYSTEM RECOMMENDATION", HearthHudButtonActionType.ShowPage, HearthHudPageId.Slide01PersistentActive, HearthHudPageId.Slide18AlertDisposition, true, -1);
            AddTextButton(pageRoot, slide, "PROMPT FAMILY RESPONSE", HearthHudButtonActionType.ShowPage, HearthHudPageId.Slide01PersistentActive, HearthHudPageId.Slide18AlertDisposition, true, 1);
        }

        if (page == 19)
        {
            AddTextButton(pageRoot, slide, "RECALL EVENT", HearthHudButtonActionType.ShowRobotReplay, HearthHudPageId.Slide06RobotReplay, HearthHudPageId.Slide18AlertDisposition);
            AddTextButton(pageRoot, slide, "CLOSE", HearthHudButtonActionType.ShowPage, HearthHudPageId.Slide13AlertDoorwaySummary);
        }

        if (page == 20 || page == 21)
        {
            AddInvisibleCornerCloseButton(pageRoot, HearthHudPageId.Slide07PersistentDormant);
        }

        if (page == 22)
        {
            AddTextButton(pageRoot, slide, "CONFIRM", HearthHudButtonActionType.ShowPage, HearthHudPageId.Slide01PersistentActive);
            AddTextButton(pageRoot, slide, "CANCEL", HearthHudButtonActionType.ShowPage, HearthHudPageId.Slide08FinalChoice);
        }

        if (page == 23)
        {
            AddTextButton(pageRoot, slide, "YES", HearthHudButtonActionType.ShowPage, HearthHudPageId.Slide24Warning02);
            AddTextButton(pageRoot, slide, "NO", HearthHudButtonActionType.ShowPage, HearthHudPageId.Slide08FinalChoice);
        }

        if (page == 24)
        {
            AddTextButton(pageRoot, slide, "YES", HearthHudButtonActionType.ShowPage, HearthHudPageId.Slide09WarningFinal);
            AddTextButton(pageRoot, slide, "NO", HearthHudButtonActionType.ShowPage, HearthHudPageId.Slide08FinalChoice);
        }
    }

    private static void AddTerminalTabButtons(Transform pageRoot, SlideData slide, bool alert)
    {
        AddTextButton(pageRoot, slide, "RESIDENT SUMMARY", HearthHudButtonActionType.ShowPage, alert ? HearthHudPageId.Slide13AlertDoorwaySummary : HearthHudPageId.Slide03DoorwaySummary);
        AddTextButton(pageRoot, slide, "ACQUISITION", HearthHudButtonActionType.ShowPage, alert ? HearthHudPageId.Slide14AlertDoorwayAcquisition : HearthHudPageId.Slide04DoorwayAcquisition);
        AddTextButton(pageRoot, slide, "FAMILY LOG", HearthHudButtonActionType.ShowPage, HearthHudPageId.Slide15AlertFamilyLog);
        AddTextButton(pageRoot, slide, "TRUST TREND", HearthHudButtonActionType.ShowPage, HearthHudPageId.Slide16AlertTrustTrend);
        AddTextButton(pageRoot, slide, "INSPECTION HISTORY", HearthHudButtonActionType.ShowPage, HearthHudPageId.Slide17AlertInspectionHistory);
    }

    private static void AddTextButton(Transform pageRoot, SlideData slide, string text, HearthHudButtonActionType action, HearthHudPageId targetPage)
    {
        AddTextButton(pageRoot, slide, text, action, targetPage, targetPage, false, 0);
    }

    private static void AddTextButton(Transform pageRoot, SlideData slide, string text, HearthHudButtonActionType action, HearthHudPageId targetPage, HearthHudPageId replayReturnPage)
    {
        AddTextButton(pageRoot, slide, text, action, targetPage, replayReturnPage, false, 0);
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

    private static void AddInvisibleCornerCloseButton(Transform pageRoot, HearthHudPageId targetPage)
    {
        CreateButton(pageRoot, "Button_CloseOverlay", new Rect(1660f, 80f, 160f, 120f), HearthHudButtonActionType.ShowPage, targetPage, targetPage, HearthDoorwayTab.ResidentSummary, false, 0);
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

    private static bool ShouldSkipSourceShape(int slideNumber, HearthHudPageMetadata metadata, ShapeData shape)
    {
        if (!metadata.showPersistentHud)
        {
            return false;
        }

        bool inPersistentBand = shape.Rect.y < 160f && (shape.Rect.x < 560f || shape.Rect.x > 1280f);
        if (inPersistentBand)
        {
            return true;
        }

        string text = NormalizeText(shape.Text);
        if (text.Contains("COMPANION UNIT") || text.Contains("ALERT PENDING REVIEW") || text.Contains("MIA 7842") || text.Contains("CURRENT TASK") || text.Contains("NIGHT ROUNDS BLOCK A"))
        {
            return true;
        }

        return false;
    }

    private static HearthHudPageMetadata BuildMetadata(SlideData slide)
    {
        HearthHudPageMetadata metadata = new HearthHudPageMetadata();
        metadata.pageId = (HearthHudPageId)slide.Number;
        metadata.showPersistentHud = slide.Number != 6;
        metadata.hudState = HearthHudState.Active;
        metadata.clockText = FindClockText(slide);
        metadata.showTask = HasText(slide, "CURRENT TASK");
        metadata.taskText = FindTaskText(slide);
        metadata.subtitleText = string.Empty;

        if (slide.Number == 6)
        {
            metadata.showPersistentHud = false;
        }
        else if (slide.Number == 7 || slide.Number == 8 || slide.Number == 10 || slide.Number == 20 || slide.Number == 21 || slide.Number == 22)
        {
            metadata.hudState = HearthHudState.Dormant;
        }
        else if (slide.Number == 9 || slide.Number == 23 || slide.Number == 24)
        {
            metadata.hudState = slide.Number == 24 ? HearthHudState.WarningDeepOrange : HearthHudState.WarningOrange;
        }
        else if (slide.Number >= 13 && slide.Number <= 19)
        {
            metadata.hudState = HearthHudState.Alert;
        }

        return metadata;
    }

    private static string FindClockText(SlideData slide)
    {
        for (int i = 0; i < slide.Shapes.Count; i++)
        {
            string text = slide.Shapes[i].Text;
            if (!string.IsNullOrEmpty(text) && text.Contains("2026."))
            {
                return text.Replace("\n", " ").Trim();
            }
        }

        return "2026.09.15·MON·18:47";
    }

    private static string FindTaskText(SlideData slide)
    {
        for (int i = 0; i < slide.Shapes.Count; i++)
        {
            string text = slide.Shapes[i].Text;
            if (string.IsNullOrEmpty(text))
            {
                continue;
            }

            string normalized = NormalizeText(text);
            if (normalized.Contains("NIGHT ROUNDS") || normalized.Contains("BLOCK A"))
            {
                return text.Replace("\n", " ").Trim();
            }
        }

        return string.Empty;
    }

    private static bool HasText(SlideData slide, string text)
    {
        string normalizedNeedle = NormalizeText(text);
        for (int i = 0; i < slide.Shapes.Count; i++)
        {
            if (NormalizeText(slide.Shapes[i].Text).Contains(normalizedNeedle))
            {
                return true;
            }
        }

        return false;
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

        if (!shape.HasFill && !shape.HasLine && string.IsNullOrWhiteSpace(shape.Text))
        {
            return false;
        }

        return true;
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

    private static RectTransform CreateLayer(Transform parent, string name)
    {
        GameObject layer = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
        layer.transform.SetParent(parent, false);
        RectTransform rect = layer.GetComponent<RectTransform>();
        StretchToParent(rect);
        return rect;
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

    private static Rect InflateRect(Rect rect, float horizontal, float vertical)
    {
        rect.x -= horizontal;
        rect.y -= vertical;
        rect.width += horizontal * 2f;
        rect.height += vertical * 2f;
        return rect;
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

    private static bool UsesFullscreenLayer(int slideNumber)
    {
        return slideNumber == 6 ||
               slideNumber == 8 ||
               slideNumber == 9 ||
               slideNumber == 20 ||
               slideNumber == 21 ||
               slideNumber == 22 ||
               slideNumber == 23 ||
               slideNumber == 24;
    }

    private static string GetPageShortName(int slideNumber)
    {
        switch (slideNumber)
        {
            case 1: return "PersistentActive";
            case 2: return "SyncTerminal";
            case 3: return "DoorwaySummary";
            case 4: return "DoorwayAcquisition";
            case 5: return "DoorwayDisposition";
            case 6: return "RobotReplay";
            case 7: return "PersistentDormant";
            case 8: return "FinalChoice";
            case 9: return "WarningFinal";
            case 10: return "ReturnToWork";
            case 11: return "WorkspaceQuickMenu";
            case 12: return "WorkspacePanel";
            case 13: return "AlertSummary";
            case 14: return "AlertAcquisition";
            case 15: return "AlertFamilyLog";
            case 16: return "AlertTrustTrend";
            case 17: return "AlertInspectionHistory";
            case 18: return "AlertDisposition";
            case 19: return "IndoorSidePanel";
            case 20: return "Photo2023";
            case 21: return "Photo2026";
            case 22: return "ShutdownConfirm";
            case 23: return "Warning01";
            case 24: return "Warning02";
            default: return "Page";
        }
    }

    private static void ImportComponentPngsAsSprites()
    {
        if (!Directory.Exists(ComponentAssetFolder))
        {
            return;
        }

        string[] pngs = Directory.GetFiles(ComponentAssetFolder, "*.png", SearchOption.TopDirectoryOnly);
        for (int i = 0; i < pngs.Length; i++)
        {
            string assetPath = pngs[i].Replace("\\", "/");
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                continue;
            }

            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }

            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                changed = true;
            }

            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }
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
        string[] uiFolders = Directory.GetDirectories(projectRoot, "UI*", SearchOption.TopDirectoryOnly);
        for (int i = 0; i < uiFolders.Length; i++)
        {
            string candidate = Path.Combine(uiFolders[i], "HEARTH-HUD.pptx");
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        string direct = Path.Combine(projectRoot, "HEARTH-HUD.pptx");
        return File.Exists(direct) ? direct : string.Empty;
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
