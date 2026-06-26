using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HearthHudDemoController : MonoBehaviour
{
    private enum DemoPage
    {
        PersistentActive,
        SyncTerminal,
        DoorwaySummary,
        DoorwayAcquisition,
        RobotReplay,
        DoorwayDisposition,
        WorkspaceQuickMenu,
        WorkspacePanel,
        AlertTerminal,
        AlertDisposition,
        SideTerminal,
        HomeDormant,
        Photo2023,
        Photo2026,
        FinalChoice,
        ShutdownConfirm,
        Warning01,
        Warning02,
        Warning03,
        ComponentGallery
    }

    private enum HudState
    {
        Active,
        Dormant,
        Alert,
        Warning
    }

    [Header("Resources")]
    [SerializeField] private string componentResourceFolder = "UI/HearthHudComponents";
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920f, 1080f);

    [Header("Input")]
    [SerializeField] private bool enableKeyboardDemoControls = true;
    [SerializeField] private KeyCode previousPageKey = KeyCode.LeftBracket;
    [SerializeField] private KeyCode nextPageKey = KeyCode.RightBracket;
    [SerializeField] private KeyCode activateKey = KeyCode.E;
    [SerializeField] private KeyCode tabKey = KeyCode.Tab;

    private readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
    private readonly List<GameObject> temporaryObjects = new List<GameObject>();
    private readonly DemoPage[] pageOrder =
    {
        DemoPage.PersistentActive,
        DemoPage.SyncTerminal,
        DemoPage.DoorwaySummary,
        DemoPage.DoorwayAcquisition,
        DemoPage.RobotReplay,
        DemoPage.DoorwayDisposition,
        DemoPage.WorkspaceQuickMenu,
        DemoPage.WorkspacePanel,
        DemoPage.AlertTerminal,
        DemoPage.AlertDisposition,
        DemoPage.SideTerminal,
        DemoPage.HomeDormant,
        DemoPage.Photo2023,
        DemoPage.Photo2026,
        DemoPage.FinalChoice,
        DemoPage.ShutdownConfirm,
        DemoPage.Warning01,
        DemoPage.Warning02,
        DemoPage.Warning03,
        DemoPage.ComponentGallery
    };

    private Canvas canvas;
    private RectTransform canvasRect;
    private RectTransform persistentRoot;
    private RectTransform pageRoot;
    private TMP_FontAsset runtimeFont;
    private Image workerBadgeImage;
    private Image statusDot;
    private TMP_Text workerStatusText;
    private TMP_Text clockText;
    private TMP_Text taskText;
    private TMP_Text subtitleText;
    private GameObject subtitleBackdrop;
    private TMP_Text trustText;
    private DemoPage currentPage;
    private HudState currentHudState;
    private Image holdFillImage;
    private bool holdingAction;
    private float holdSeconds;
    private Coroutine pageFadeRoutine;
    private Coroutine trustRoutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreateInEditorPlayMode()
    {
        if (!Application.isEditor)
        {
            return;
        }

        if (GameObject.Find("HEARTH_HUD_DEMO_LEGACY_ENABLE") == null)
        {
            return;
        }

        if (FindObjectOfType<HearthHudDemoController>() != null)
        {
            return;
        }

        GameObject controllerObject = new GameObject("HEARTH_HUD_DEMO_RUNTIME");
        controllerObject.AddComponent<HearthHudDemoController>();
    }

    private void Awake()
    {
        LoadSprites();
        EnsureEventSystem();
        CreateCanvas();
        CreatePersistentHud();
        ShowPage(DemoPage.PersistentActive);
    }

    private void Update()
    {
        AnimateStatusDot();

        if (!enableKeyboardDemoControls)
        {
            return;
        }

        HandlePageShortcuts();
        HandleContextInput();
    }

    private void LoadSprites()
    {
        sprites.Clear();

        Texture2D[] textures = Resources.LoadAll<Texture2D>(componentResourceFolder);
        for (int i = 0; i < textures.Length; i++)
        {
            Texture2D texture = textures[i];
            if (texture == null || sprites.ContainsKey(texture.name))
            {
                continue;
            }

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f);
            sprites.Add(texture.name, sprite);
        }
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    private void CreateCanvas()
    {
        GameObject canvasObject = new GameObject("HEARTH_HUD_DEMO_CANVAS", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = referenceResolution;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasRect = canvasObject.GetComponent<RectTransform>();
        persistentRoot = CreateLayer("Persistent HUD", canvasRect);
        pageRoot = CreateLayer("Page Layer", canvasRect);
        CreateLayer("Overlay Layer", canvasRect);
    }

    private RectTransform CreateLayer(string objectName, Transform parent)
    {
        GameObject layerObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasGroup));
        layerObject.transform.SetParent(parent, false);

        RectTransform rect = layerObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return rect;
    }

    private void CreatePersistentHud()
    {
        CreateCornerLine(persistentRoot, new Vector2(34f, -32f), TextAnchor.UpperLeft);
        CreateCornerLine(persistentRoot, new Vector2(-34f, -32f), TextAnchor.UpperRight);
        CreateCornerLine(persistentRoot, new Vector2(34f, 32f), TextAnchor.LowerLeft);
        CreateCornerLine(persistentRoot, new Vector2(-34f, 32f), TextAnchor.LowerRight);

        workerBadgeImage = CreateAssetImage("Slide28_WorkerBadgeActive", persistentRoot, "Worker Badge Base", new Vector2(34f, -44f), new Vector2(424f, 112f), new Vector2(0f, 1f), new Vector2(0f, 1f), 0.78f);

        statusDot = CreateSolidImage(persistentRoot, "Status Dot", new Color(0.09f, 0.96f, 0.56f, 1f), new Vector2(53f, -87f), new Vector2(13f, 13f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        workerStatusText = CreateText(persistentRoot, "Worker Status", "COMPANION UNIT | ACTIVE", 20f, FontStyles.Bold, new Vector2(76f, -78f), new Vector2(310f, 26f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.84f, 1f, 0.93f, 0.95f));
        CreateText(persistentRoot, "Worker Name", "MIA | 7842", 16f, FontStyles.Normal, new Vector2(76f, -105f), new Vector2(180f, 23f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Color(0.78f, 0.9f, 0.92f, 0.86f));

        Button badgeButton = CreateInvisibleButton(persistentRoot, "Worker Badge Hitbox", new Vector2(34f, -44f), new Vector2(424f, 112f), new Vector2(0f, 1f), new Vector2(0f, 1f));
        badgeButton.onClick.AddListener(delegate { ShowPage(DemoPage.WorkspaceQuickMenu); });

        clockText = CreateText(persistentRoot, "Clock", "2026.09.15 | MON | 18:47", 22f, FontStyles.Normal, new Vector2(0f, -38f), new Vector2(460f, 35f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Color(0.78f, 0.92f, 0.94f, 0.88f));
        clockText.alignment = TextAlignmentOptions.Center;

        trustText = CreateText(persistentRoot, "Trust Float", string.Empty, 22f, FontStyles.Bold, new Vector2(-70f, -76f), new Vector2(260f, 45f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Color(0.4f, 1f, 0.78f, 0f));
        trustText.alignment = TextAlignmentOptions.Right;

        taskText = CreateText(persistentRoot, "Task", "CURRENT TASK\nNIGHT ROUNDS | BLOCK A | 17F\n0/3", 20f, FontStyles.Normal, new Vector2(42f, 54f), new Vector2(470f, 105f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Color(0.84f, 0.96f, 1f, 0.86f));
        subtitleText = CreateText(persistentRoot, "Subtitle", "\"Inspector - route assigned. Proceed to Block A, 17F.\"", 24f, FontStyles.Normal, new Vector2(0f, 54f), new Vector2(980f, 56f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Color.white);
        subtitleText.alignment = TextAlignmentOptions.Center;
        subtitleBackdrop = CreatePanelBehind(subtitleText.rectTransform, new Color(0f, 0f, 0f, 0.58f), 18f);
    }

    private void HandlePageShortcuts()
    {
        if (Input.GetKeyDown(previousPageKey))
        {
            StepPage(-1);
        }

        if (Input.GetKeyDown(nextPageKey))
        {
            StepPage(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ShowPage(DemoPage.PersistentActive);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ShowPage(DemoPage.SyncTerminal);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ShowPage(DemoPage.DoorwaySummary);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            ShowPage(DemoPage.RobotReplay);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            ShowPage(DemoPage.DoorwayDisposition);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            ShowPage(DemoPage.WorkspacePanel);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            ShowPage(DemoPage.AlertTerminal);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            ShowPage(DemoPage.FinalChoice);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            ShowPage(DemoPage.ComponentGallery);
        }
    }

    private void HandleContextInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowPage(DemoPage.PersistentActive);
        }

        if (Input.GetKeyDown(tabKey))
        {
            CycleContextTab();
        }

        if (currentPage == DemoPage.RobotReplay)
        {
            if (Input.GetKey(activateKey))
            {
                holdingAction = true;
                holdSeconds += Time.unscaledDeltaTime;
                if (holdFillImage != null)
                {
                    holdFillImage.fillAmount = Mathf.Clamp01(holdSeconds / 1.5f);
                }

                if (holdSeconds >= 1.5f)
                {
                    holdingAction = false;
                    holdSeconds = 0f;
                    ShowPage(DemoPage.DoorwayDisposition);
                }
            }
            else if (holdingAction)
            {
                holdingAction = false;
                holdSeconds = 0f;
                if (holdFillImage != null)
                {
                    holdFillImage.fillAmount = 0f;
                }
            }
        }
        else if (Input.GetKeyDown(activateKey))
        {
            ActivateCurrentPage();
        }
    }

    private void StepPage(int direction)
    {
        int index = 0;
        for (int i = 0; i < pageOrder.Length; i++)
        {
            if (pageOrder[i] == currentPage)
            {
                index = i;
                break;
            }
        }

        index = (index + direction + pageOrder.Length) % pageOrder.Length;
        ShowPage(pageOrder[index]);
    }

    private void CycleContextTab()
    {
        if (currentPage == DemoPage.DoorwaySummary)
        {
            ShowPage(DemoPage.DoorwayAcquisition);
        }
        else if (currentPage == DemoPage.DoorwayAcquisition)
        {
            ShowPage(DemoPage.DoorwaySummary);
        }
        else if (currentPage == DemoPage.AlertTerminal)
        {
            ShowPage(DemoPage.SideTerminal);
        }
        else if (currentPage == DemoPage.SideTerminal)
        {
            ShowPage(DemoPage.AlertTerminal);
        }
    }

    private void ActivateCurrentPage()
    {
        switch (currentPage)
        {
            case DemoPage.PersistentActive:
                ShowPage(DemoPage.WorkspaceQuickMenu);
                break;
            case DemoPage.SyncTerminal:
                FlashTrust("+0 TRUST");
                ShowPage(DemoPage.PersistentActive);
                break;
            case DemoPage.DoorwaySummary:
            case DemoPage.DoorwayAcquisition:
                ShowPage(DemoPage.RobotReplay);
                break;
            case DemoPage.DoorwayDisposition:
                FlashTrust("+1 TRUST");
                ShowPage(DemoPage.PersistentActive);
                break;
            case DemoPage.WorkspaceQuickMenu:
                ShowPage(DemoPage.WorkspacePanel);
                break;
            case DemoPage.AlertTerminal:
                ShowPage(DemoPage.SideTerminal);
                break;
            case DemoPage.SideTerminal:
                ShowPage(DemoPage.RobotReplay);
                break;
            case DemoPage.FinalChoice:
                ShowPage(DemoPage.ShutdownConfirm);
                break;
            case DemoPage.ShutdownConfirm:
                ShowPage(DemoPage.Warning01);
                break;
            case DemoPage.Warning01:
                ShowPage(DemoPage.Warning02);
                break;
            case DemoPage.Warning02:
                ShowPage(DemoPage.Warning03);
                break;
            case DemoPage.Warning03:
                ShowPage(DemoPage.FinalChoice);
                break;
        }
    }

    private void ShowPage(DemoPage page)
    {
        currentPage = page;
        holdingAction = false;
        holdSeconds = 0f;
        holdFillImage = null;
        ClearTemporaryObjects();

        switch (page)
        {
            case DemoPage.PersistentActive:
                SetPersistentHud(HudState.Active, true, "CURRENT TASK\nNIGHT ROUNDS | BLOCK A | 17F\n0/3", "\"Inspector - route assigned. Proceed to Block A, 17F.\"", "2026.09.15 | MON | 18:47");
                break;
            case DemoPage.SyncTerminal:
                SetPersistentHud(HudState.Active, true, "CURRENT TASK\nNIGHT ROUNDS | TASK RECEIVED", "\"Confirm tonight's route before entering the elevator.\"", "2026.09.15 | MON | 18:32");
                BuildSyncTerminal();
                break;
            case DemoPage.DoorwaySummary:
                SetPersistentHud(HudState.Active, true, "CURRENT TASK\nNIGHT ROUNDS | BLOCK A | 17F\n17F-01", "\"Doorway terminal connected. Review resident summary.\"", "2026.09.15 | MON | 18:42");
                BuildDoorwayTerminal(false, false);
                break;
            case DemoPage.DoorwayAcquisition:
                SetPersistentHud(HudState.Active, true, "CURRENT TASK\nNIGHT ROUNDS | BLOCK A | 17F\n17F-01", "\"Acquisition context loaded.\"", "2026.09.15 | MON | 18:43");
                BuildAcquisitionTerminal(false);
                break;
            case DemoPage.RobotReplay:
                SetPersistentHud(HudState.Active, false, string.Empty, string.Empty, "02:47");
                persistentRoot.gameObject.SetActive(false);
                BuildRobotReplay();
                break;
            case DemoPage.DoorwayDisposition:
                SetPersistentHud(HudState.Active, true, "CURRENT TASK\nNIGHT ROUNDS | BLOCK A | 17F\n1/3", "\"Replay complete. Select disposition.\"", "2026.09.15 | MON | 18:47");
                BuildDoorwayTerminal(true, false);
                break;
            case DemoPage.WorkspaceQuickMenu:
                SetPersistentHud(HudState.Active, true, "CURRENT TASK\nNIGHT ROUNDS | BLOCK A | 17F\n1/3", "\"Workspace focus confirmed.\"", "2026.09.15 | MON | 18:47");
                BuildWorkspaceQuickMenu();
                break;
            case DemoPage.WorkspacePanel:
                SetPersistentHud(HudState.Active, true, "CURRENT TASK\nNIGHT ROUNDS | BLOCK A | 17F\n2/3", "\"Today's rounds are available.\"", "2026.09.15 | MON | 18:47");
                BuildWorkspacePanel();
                break;
            case DemoPage.AlertTerminal:
                SetPersistentHud(HudState.Alert, true, "CURRENT TASK\nNIGHT ROUNDS | BLOCK A | 17F\n2/3", "\"Warning: 17F-03 companion unit is pending review.\"", "2026.09.15 | MON | 22:16");
                BuildDoorwayTerminal(false, true);
                break;
            case DemoPage.AlertDisposition:
                SetPersistentHud(HudState.Alert, true, "CURRENT TASK\nNIGHT ROUNDS | BLOCK A | 17F\n3/3", "\"Alert replay complete. Select final disposition.\"", "2026.09.15 | MON | 22:38");
                BuildDoorwayTerminal(true, true);
                break;
            case DemoPage.SideTerminal:
                SetPersistentHud(HudState.Alert, true, "CURRENT TASK\nNIGHT ROUNDS | BLOCK A | 17F\n2/3", "(faint | parents arguing in the bedroom, muffled)", "2026.09.15 | MON | 22:24");
                BuildSideTerminal();
                break;
            case DemoPage.HomeDormant:
                SetPersistentHud(HudState.Dormant, false, string.Empty, "-- companion unit dormant --", "2026.09.15 | MON | 23:42");
                BuildHomeWelcome();
                break;
            case DemoPage.Photo2023:
                SetPersistentHud(HudState.Dormant, false, string.Empty, "(an old photograph | Lily, age 6, classroom open day | front row)", "2026.09.15 | MON | 23:48");
                BuildPhotoCard("Slide38_PhotoCard2023");
                break;
            case DemoPage.Photo2026:
                SetPersistentHud(HudState.Dormant, false, string.Empty, "(another photograph | Lily, age 9, today's date | no parent in frame)", "2026.09.15 | MON | 23:48");
                BuildPhotoCard("Slide39_PhotoCard2026");
                break;
            case DemoPage.FinalChoice:
                SetPersistentHud(HudState.Dormant, false, string.Empty, "Lily: \"Mom, will you come tomorrow?\"", "2026.09.15 | MON | 23:58");
                BuildFinalChoice();
                break;
            case DemoPage.ShutdownConfirm:
                SetPersistentHud(HudState.Dormant, false, string.Empty, "\"Inspector - standard shutdown acknowledged. Goodnight, Inspector.\"", "2026.09.15 | MON | 23:59");
                BuildConfirmDialog();
                break;
            case DemoPage.Warning01:
                SetPersistentHud(HudState.Warning, false, string.Empty, "\"Inspector - this is an unusual choice. Consider standard shutdown.\"", "2026.09.15 | MON | 23:59");
                BuildWarning("WARNING", "01 / 03", "Companion units serve a household ecosystem.\nBypassing the farewell protocol is not advised.", false);
                break;
            case DemoPage.Warning02:
                SetPersistentHud(HudState.Warning, false, string.Empty, "\"Continuity profile risk increasing.\"", "2026.09.16 | TUE | 00:00");
                BuildWarning("WARNING", "02 / 03", "This unit holds 6 years of Lily's developmental records.\nForce shutdown will fragment her continuity profile.", true);
                break;
            case DemoPage.Warning03:
                SetPersistentHud(HudState.Alert, false, string.Empty, "Final confirmation.", "2026.09.16 | TUE | 00:02");
                BuildWarning("WARNING | HIGH-RISK", "03 / 03", "This shutdown will bypass the family unit's farewell protocol.\nYour night-rounds clearance will be suspended for seven days.", true);
                break;
            case DemoPage.ComponentGallery:
                SetPersistentHud(HudState.Active, false, string.Empty, string.Empty, "COMPONENT LIBRARY");
                BuildComponentGallery();
                break;
        }

        persistentRoot.gameObject.SetActive(page != DemoPage.RobotReplay);
        FadePageIn();
    }

    private void SetPersistentHud(HudState hudState, bool showTask, string task, string subtitle, string clock)
    {
        currentHudState = hudState;
        persistentRoot.gameObject.SetActive(true);

        if (clockText != null)
        {
            clockText.text = clock;
        }

        if (taskText != null)
        {
            taskText.text = task;
            taskText.gameObject.SetActive(showTask);
        }

        if (subtitleText != null)
        {
            subtitleText.text = subtitle;
            subtitleText.gameObject.SetActive(!string.IsNullOrEmpty(subtitle));
        }

        if (subtitleBackdrop != null)
        {
            subtitleBackdrop.SetActive(!string.IsNullOrEmpty(subtitle));
        }

        if (workerStatusText != null)
        {
            workerStatusText.text = GetWorkerStatusLabel(hudState);
            workerStatusText.color = hudState == HudState.Alert || hudState == HudState.Warning
                ? new Color(1f, 0.67f, 0.58f, 0.95f)
                : new Color(0.84f, 1f, 0.93f, 0.95f);
        }

        if (workerBadgeImage != null)
        {
            Sprite badgeSprite;
            if ((hudState == HudState.Alert || hudState == HudState.Warning) && sprites.TryGetValue("Slide36_WorkerBadgeAlert", out badgeSprite))
            {
                workerBadgeImage.sprite = badgeSprite;
                workerBadgeImage.color = new Color(1f, 1f, 1f, 0.86f);
            }
            else if (sprites.TryGetValue("Slide28_WorkerBadgeActive", out badgeSprite))
            {
                workerBadgeImage.sprite = badgeSprite;
                workerBadgeImage.color = hudState == HudState.Dormant ? new Color(0.72f, 0.78f, 0.78f, 0.55f) : new Color(1f, 1f, 1f, 0.78f);
            }
        }

        if (statusDot != null)
        {
            statusDot.color = GetStatusColor(hudState);
        }
    }

    private void BuildSyncTerminal()
    {
        AddDimmer(0.36f);
        CreateGlassPanel(pageRoot, "Sync Terminal Panel", new Vector2(0f, 0f), new Vector2(980f, 600f), new Color(0.03f, 0.09f, 0.1f, 0.78f), new Color(0.18f, 0.95f, 0.82f, 0.42f));
        TMP_Text title = CreateText(pageRoot, "Sync Title", "SYNC TERMINAL | BLOCK A LOBBY", 28f, FontStyles.Bold, new Vector2(0f, 242f), new Vector2(820f, 48f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0.83f, 1f, 0.96f, 0.96f));
        title.alignment = TextAlignmentOptions.Center;
        TMP_Text body = CreateText(pageRoot, "Sync Body", "Tonight's Rounds\n17F | BLOCK A | 3 UNITS\n\n17F-01    ROUTINE CHECK\n17F-02    ROUTINE CHECK\n17F-03    PENDING ALERT", 26f, FontStyles.Normal, new Vector2(0f, 45f), new Vector2(720f, 260f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0.82f, 0.96f, 0.98f, 0.92f));
        body.alignment = TextAlignmentOptions.Center;
        CreateHudButton(pageRoot, "CONFIRM", new Vector2(0f, -225f), new Vector2(250f, 58f), delegate { ShowPage(DemoPage.PersistentActive); });
    }

    private void BuildDoorwayTerminal(bool disposition, bool alert)
    {
        AddDimmer(0.54f);
        string assetName = disposition ? "Slide30_DoorwayTerminalDisposition" : "Slide29_DoorwayTerminalSummary";
        Image shell = CreateAssetImage(assetName, pageRoot, "Doorway Terminal", Vector2.zero, new Vector2(1420f, 760f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 0.94f);
        if (alert && shell != null)
        {
            shell.color = new Color(1f, 0.46f, 0.38f, 0.96f);
        }

        TMP_Text header = CreateText(pageRoot, "Terminal Header", alert ? "DOORWAY TERMINAL | ALERT     17F-03" : "DOORWAY TERMINAL     17F-01", 28f, FontStyles.Bold, new Vector2(-520f, 315f), new Vector2(720f, 42f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), alert ? new Color(1f, 0.62f, 0.5f, 0.96f) : new Color(0.85f, 1f, 0.96f, 0.96f));
        header.alignment = TextAlignmentOptions.Left;

        string cta = alert ? "ENTER UNIT >" : "RECALL EVENT >";
        if (disposition)
        {
            cta = "VIEWED OK";
        }

        TMP_Text tabs = CreateText(pageRoot, "Terminal Tabs", "RESIDENT SUMMARY    ACQUISITION    FAMILY LOG    TRUST TREND    INSPECTION HISTORY    " + cta, 19f, FontStyles.Normal, new Vector2(10f, 254f), new Vector2(1240f, 35f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), alert ? new Color(1f, 0.74f, 0.64f, 0.9f) : new Color(0.78f, 0.92f, 0.94f, 0.9f));
        tabs.alignment = TextAlignmentOptions.Center;

        if (disposition)
        {
            CreateHudButton(pageRoot, alert ? "A | Restart and Restore Service" : "A | Approve Upgrade", new Vector2(-270f, -296f), new Vector2(420f, 58f), delegate { FlashTrust("-1 TRUST"); ShowPage(alert ? DemoPage.HomeDormant : DemoPage.PersistentActive); });
            CreateHudButton(pageRoot, alert ? "B | Honor User Shutdown" : "B | Enable Observation", new Vector2(270f, -296f), new Vector2(420f, 58f), delegate { FlashTrust("+1 TRUST"); ShowPage(alert ? DemoPage.HomeDormant : DemoPage.PersistentActive); });
            return;
        }

        CreateHudButton(pageRoot, alert ? "ENTER UNIT >" : "RECALL EVENT >", new Vector2(510f, -304f), new Vector2(300f, 58f), delegate { ShowPage(alert ? DemoPage.SideTerminal : DemoPage.RobotReplay); });
        CreateInvisibleButton(pageRoot, "Acquisition Hitbox", new Vector2(-90f, 250f), new Vector2(160f, 50f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).onClick.AddListener(delegate { ShowPage(alert ? DemoPage.AlertTerminal : DemoPage.DoorwayAcquisition); });
    }

    private void BuildAcquisitionTerminal(bool alert)
    {
        AddDimmer(0.54f);
        Image acquisition = CreateAssetImage("Slide31_AcquisitionTab", pageRoot, "Acquisition Panel", Vector2.zero, new Vector2(1420f, 760f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 0.94f);
        if (alert && acquisition != null)
        {
            acquisition.color = new Color(1f, 0.5f, 0.42f, 0.96f);
        }

        TMP_Text header = CreateText(pageRoot, "Acquisition Header", "DOORWAY TERMINAL     17F-01     ACQUISITION", 28f, FontStyles.Bold, new Vector2(-360f, 315f), new Vector2(840f, 42f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0.85f, 1f, 0.96f, 0.96f));
        header.alignment = TextAlignmentOptions.Left;
        CreateHudButton(pageRoot, "RECALL EVENT >", new Vector2(510f, -304f), new Vector2(300f, 58f), delegate { ShowPage(DemoPage.RobotReplay); });
        CreateHudButton(pageRoot, "SUMMARY", new Vector2(-530f, -304f), new Vector2(220f, 50f), delegate { ShowPage(DemoPage.DoorwaySummary); });
    }

    private void BuildRobotReplay()
    {
        CreateSolidImage(pageRoot, "Robot Backdrop", new Color(0f, 0.018f, 0.022f, 0.98f), Vector2.zero, referenceResolution, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        CreateAssetImage("Slide32_RobotViewFrame", pageRoot, "Robot Frame", Vector2.zero, new Vector2(1900f, 1040f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 0.95f);
        CreateAssetImage("Slide25_SubjectMonitoring", pageRoot, "Subject Monitoring", new Vector2(52f, -70f), new Vector2(560f, 150f), new Vector2(0f, 1f), new Vector2(0f, 1f), 0.95f);

        TMP_Text decision = CreateText(pageRoot, "Synth Voice", "SYNTH VOICE | DECISION\nInitiate Soothing Sequence", 24f, FontStyles.Bold, new Vector2(-74f, 190f), new Vector2(620f, 86f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Color(0.84f, 1f, 0.96f, 0.94f));
        decision.alignment = TextAlignmentOptions.Right;

        Image holdButton = CreateAssetImage("Slide33_HoldToActButton", pageRoot, "Hold Button", new Vector2(0f, -226f), new Vector2(560f, 176f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 0.96f);
        if (holdButton != null)
        {
            Button button = holdButton.gameObject.AddComponent<Button>();
            button.onClick.AddListener(delegate { ShowPage(DemoPage.DoorwayDisposition); });
        }

        holdFillImage = CreateSolidImage(pageRoot, "Hold Fill", new Color(0.18f, 1f, 0.74f, 0.26f), new Vector2(0f, -226f), new Vector2(440f, 76f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        holdFillImage.type = Image.Type.Filled;
        holdFillImage.fillMethod = Image.FillMethod.Horizontal;
        holdFillImage.fillOrigin = 0;
        holdFillImage.fillAmount = 0f;

        TMP_Text footer = CreateText(pageRoot, "Robot Footer", "COMPANION UNIT | FIRST PERSON | MONITORING MODE", 18f, FontStyles.Bold, new Vector2(0f, 34f), new Vector2(720f, 32f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Color(0.72f, 0.95f, 1f, 0.82f));
        footer.alignment = TextAlignmentOptions.Center;
    }

    private void BuildWorkspaceQuickMenu()
    {
        CreateAssetImage("Slide35_WorkspaceQuickMenu", pageRoot, "Workspace Quick Menu", new Vector2(322f, -152f), new Vector2(520f, 200f), new Vector2(0f, 1f), new Vector2(0f, 1f), 0.96f);
        CreateHudButton(pageRoot, "TODAY'S ROUNDS", new Vector2(586f, -206f), new Vector2(360f, 46f), delegate { ShowPage(DemoPage.WorkspacePanel); });
    }

    private void BuildWorkspacePanel()
    {
        AddDimmer(0.48f);
        CreateAssetImage("Slide27_WorkspacePanel", pageRoot, "Workspace Panel", Vector2.zero, new Vector2(1120f, 620f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 0.96f);
        CreateHudButton(pageRoot, "CLOSE", new Vector2(0f, -350f), new Vector2(180f, 48f), delegate { ShowPage(DemoPage.PersistentActive); });
    }

    private void BuildSideTerminal()
    {
        CreateAssetImage("Slide37_IndoorSideTerminal", pageRoot, "Indoor Side Terminal", new Vector2(520f, -20f), new Vector2(520f, 760f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 0.96f);
        CreateHudButton(pageRoot, "RECALL EVENT >", new Vector2(520f, -376f), new Vector2(280f, 52f), delegate { ShowPage(DemoPage.RobotReplay); });
    }

    private void BuildHomeWelcome()
    {
        TMP_Text welcome = CreateText(pageRoot, "Home Welcome", "You're home. Welcome.", 36f, FontStyles.Normal, Vector2.zero, new Vector2(650f, 80f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0.88f, 0.94f, 0.96f, 0.9f));
        welcome.alignment = TextAlignmentOptions.Center;
    }

    private void BuildPhotoCard(string assetName)
    {
        AddDimmer(0.28f);
        CreateAssetImage(assetName, pageRoot, "Photo Card", Vector2.zero, new Vector2(740f, 360f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 0.98f);
        CreateHudButton(pageRoot, "CLOSE", new Vector2(0f, -255f), new Vector2(180f, 48f), delegate { ShowPage(DemoPage.HomeDormant); });
    }

    private void BuildFinalChoice()
    {
        CreateAssetImage("Slide26_FinalChoiceOptions", pageRoot, "Final Choice", Vector2.zero, new Vector2(680f, 520f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 0.98f);
        CreateInvisibleButton(pageRoot, "Choice A", new Vector2(-210f, -12f), new Vector2(330f, 260f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).onClick.AddListener(delegate { ShowPage(DemoPage.ShutdownConfirm); });
        CreateInvisibleButton(pageRoot, "Choice B", new Vector2(210f, -12f), new Vector2(330f, 260f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).onClick.AddListener(delegate { FlashTrust("-"); ShowPage(DemoPage.HomeDormant); });
    }

    private void BuildConfirmDialog()
    {
        AddDimmer(0.32f);
        CreateAssetImage("Slide40_ShutdownConfirmDialog", pageRoot, "Confirm Dialog", Vector2.zero, new Vector2(620f, 290f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 0.98f);
        CreateInvisibleButton(pageRoot, "Confirm", new Vector2(-118f, -83f), new Vector2(180f, 54f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).onClick.AddListener(delegate { ShowPage(DemoPage.HomeDormant); });
        CreateInvisibleButton(pageRoot, "Cancel", new Vector2(118f, -83f), new Vector2(180f, 54f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f)).onClick.AddListener(delegate { ShowPage(DemoPage.FinalChoice); });
    }

    private void BuildWarning(string title, string step, string body, bool stronger)
    {
        AddDimmer(stronger ? 0.5f : 0.38f);
        Color border = stronger ? new Color(1f, 0.19f, 0.13f, 0.82f) : new Color(1f, 0.55f, 0.2f, 0.7f);
        CreateGlassPanel(pageRoot, "Warning Panel", Vector2.zero, new Vector2(900f, 470f), new Color(0.1f, 0.015f, 0.014f, 0.84f), border);
        TMP_Text titleText = CreateText(pageRoot, "Warning Title", title, stronger ? 34f : 30f, FontStyles.Bold, new Vector2(-310f, 150f), new Vector2(430f, 50f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), border);
        titleText.alignment = TextAlignmentOptions.Left;
        TMP_Text stepText = CreateText(pageRoot, "Warning Step", step, 24f, FontStyles.Bold, new Vector2(292f, 150f), new Vector2(240f, 50f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), border);
        stepText.alignment = TextAlignmentOptions.Right;
        TMP_Text bodyText = CreateText(pageRoot, "Warning Body", body, stronger ? 25f : 23f, FontStyles.Normal, new Vector2(0f, 30f), new Vector2(700f, 150f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(1f, 0.88f, 0.82f, 0.95f));
        bodyText.alignment = TextAlignmentOptions.Center;
        CreateHudButton(pageRoot, stronger ? "YES | CONTINUE" : "YES | CONTINUE", new Vector2(-160f, -160f), new Vector2(260f, 56f), delegate { ActivateCurrentPage(); });
        CreateHudButton(pageRoot, "NO | KEEP UNIT ACTIVE", new Vector2(180f, -160f), new Vector2(320f, 56f), delegate { ShowPage(DemoPage.FinalChoice); });
    }

    private void BuildComponentGallery()
    {
        AddDimmer(0.62f);
        string[] names =
        {
            "Slide25_SubjectMonitoring",
            "Slide26_FinalChoiceOptions",
            "Slide27_WorkspacePanel",
            "Slide28_WorkerBadgeActive",
            "Slide29_DoorwayTerminalSummary",
            "Slide30_DoorwayTerminalDisposition",
            "Slide31_AcquisitionTab",
            "Slide32_RobotViewFrame",
            "Slide33_HoldToActButton",
            "Slide34_AlertElements",
            "Slide35_WorkspaceQuickMenu",
            "Slide36_WorkerBadgeAlert",
            "Slide37_IndoorSideTerminal",
            "Slide38_PhotoCard2023",
            "Slide39_PhotoCard2026",
            "Slide40_ShutdownConfirmDialog"
        };

        float startX = -700f;
        float startY = 280f;
        for (int i = 0; i < names.Length; i++)
        {
            int col = i % 4;
            int row = i / 4;
            Vector2 position = new Vector2(startX + col * 470f, startY - row * 230f);
            CreateGlassPanel(pageRoot, "Gallery Cell " + i, position, new Vector2(400f, 180f), new Color(0.015f, 0.04f, 0.045f, 0.68f), new Color(0.3f, 0.95f, 0.88f, 0.24f));
            CreateAssetImage(names[i], pageRoot, names[i], position + new Vector2(0f, 16f), new Vector2(340f, 112f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), 0.98f);
            TMP_Text label = CreateText(pageRoot, "Gallery Label " + i, "Slide " + (25 + i), 16f, FontStyles.Bold, position + new Vector2(0f, -70f), new Vector2(260f, 26f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0.74f, 0.95f, 0.96f, 0.82f));
            label.alignment = TextAlignmentOptions.Center;
        }
    }

    private void AddDimmer(float alpha)
    {
        Image dimmer = CreateSolidImage(pageRoot, "Dimmer", new Color(0f, 0f, 0f, alpha), Vector2.zero, referenceResolution, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        dimmer.transform.SetAsFirstSibling();
    }

    private Image CreateAssetImage(string spriteName, Transform parent, string objectName, Vector2 anchoredPosition, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, float alpha)
    {
        Sprite sprite;
        if (!sprites.TryGetValue(spriteName, out sprite) || sprite == null)
        {
            return CreateMissingAsset(parent, objectName + " Missing", anchoredPosition, size, anchorMin, anchorMax, spriteName);
        }

        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        imageObject.transform.SetParent(parent, false);
        temporaryObjects.Add(imageObject);

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
        image.color = new Color(1f, 1f, 1f, alpha);
        return image;
    }

    private Image CreateMissingAsset(Transform parent, string objectName, Vector2 anchoredPosition, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, string missingName)
    {
        CreateGlassPanel(parent, objectName, anchoredPosition, size, new Color(0.08f, 0.02f, 0.02f, 0.8f), new Color(1f, 0.2f, 0.16f, 0.8f));
        TMP_Text text = CreateText(parent, objectName + " Text", "Missing\n" + missingName, 18f, FontStyles.Bold, anchoredPosition, size - new Vector2(30f, 30f), anchorMin, anchorMax, new Color(1f, 0.7f, 0.62f, 0.96f));
        text.alignment = TextAlignmentOptions.Center;
        return null;
    }

    private Image CreateSolidImage(Transform parent, string objectName, Color color, Vector2 anchoredPosition, Vector2 size, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        temporaryObjects.Add(imageObject);

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private void CreateGlassPanel(Transform parent, string objectName, Vector2 anchoredPosition, Vector2 size, Color fill, Color border)
    {
        Image panel = CreateSolidImage(parent, objectName, fill, anchoredPosition, size, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        panel.raycastTarget = false;
        Outline outline = panel.gameObject.AddComponent<Outline>();
        outline.effectColor = border;
        outline.effectDistance = new Vector2(2f, -2f);
    }

    private Button CreateHudButton(Transform parent, string label, Vector2 anchoredPosition, Vector2 size, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject("Button - " + label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        temporaryObjects.Add(buttonObject);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.04f, 0.18f, 0.18f, 0.76f);
        Outline outline = buttonObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.35f, 1f, 0.86f, 0.54f);
        outline.effectDistance = new Vector2(1f, -1f);

        Button button = buttonObject.GetComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.74f, 1f, 0.92f, 1f);
        colors.pressedColor = new Color(0.38f, 1f, 0.8f, 1f);
        button.colors = colors;
        button.onClick.AddListener(action);

        TMP_Text text = CreateText(buttonObject.transform, "Label", label, 18f, FontStyles.Bold, Vector2.zero, size - new Vector2(18f, 10f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Color(0.86f, 1f, 0.96f, 0.96f));
        text.alignment = TextAlignmentOptions.Center;
        return button;
    }

    private Button CreateInvisibleButton(Transform parent, string objectName, Vector2 anchoredPosition, Vector2 size, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        temporaryObjects.Add(buttonObject);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.001f);
        image.raycastTarget = true;
        return buttonObject.GetComponent<Button>();
    }

    private TMP_Text CreateText(Transform parent, string objectName, string text, float fontSize, FontStyles fontStyle, Vector2 anchoredPosition, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        temporaryObjects.Add(textObject);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        TextMeshProUGUI tmp = textObject.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = fontStyle;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.enableWordWrapping = true;
        tmp.overflowMode = TextOverflowModes.Ellipsis;
        tmp.raycastTarget = false;
        tmp.font = GetRuntimeFont();
        return tmp;
    }

    private TMP_FontAsset GetRuntimeFont()
    {
        if (runtimeFont != null)
        {
            return runtimeFont;
        }

        if (TMP_Settings.defaultFontAsset != null)
        {
            runtimeFont = TMP_Settings.defaultFontAsset;
            return runtimeFont;
        }

        Font arial = Resources.GetBuiltinResource<Font>("Arial.ttf");
        if (arial != null)
        {
            runtimeFont = TMP_FontAsset.CreateFontAsset(arial);
        }

        return runtimeFont;
    }

    private GameObject CreatePanelBehind(RectTransform target, Color color, float padding)
    {
        GameObject panelObject = new GameObject(target.name + " Backdrop", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panelObject.transform.SetParent(target.parent, false);
        panelObject.transform.SetSiblingIndex(target.GetSiblingIndex());

        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = target.anchorMin;
        rect.anchorMax = target.anchorMax;
        rect.pivot = target.pivot;
        rect.sizeDelta = target.sizeDelta + new Vector2(padding * 2f, padding);
        rect.anchoredPosition = target.anchoredPosition;

        Image image = panelObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return panelObject;
    }

    private void CreateCornerLine(Transform parent, Vector2 anchoredPosition, TextAnchor anchor)
    {
        Vector2 anchorValue = new Vector2(anchor == TextAnchor.UpperRight || anchor == TextAnchor.LowerRight ? 1f : 0f, anchor == TextAnchor.UpperLeft || anchor == TextAnchor.UpperRight ? 1f : 0f);
        Image horizontal = CreateSolidImage(parent, "HUD Corner H", new Color(0.34f, 1f, 0.86f, 0.34f), anchoredPosition, new Vector2(92f, 2f), anchorValue, anchorValue);
        horizontal.rectTransform.pivot = anchorValue;

        Image vertical = CreateSolidImage(parent, "HUD Corner V", new Color(0.34f, 1f, 0.86f, 0.34f), anchoredPosition, new Vector2(2f, 58f), anchorValue, anchorValue);
        vertical.rectTransform.pivot = anchorValue;
    }

    private Color GetStatusColor(HudState state)
    {
        switch (state)
        {
            case HudState.Active:
                return new Color(0.09f, 0.96f, 0.56f, 1f);
            case HudState.Dormant:
                return new Color(0.48f, 0.54f, 0.55f, 1f);
            case HudState.Warning:
                return new Color(1f, 0.55f, 0.18f, 1f);
            case HudState.Alert:
                return new Color(1f, 0.16f, 0.12f, 1f);
            default:
                return Color.white;
        }
    }

    private string GetWorkerStatusLabel(HudState state)
    {
        switch (state)
        {
            case HudState.Active:
                return "COMPANION UNIT | ACTIVE";
            case HudState.Dormant:
                return "COMPANION UNIT | DORMANT";
            case HudState.Warning:
            case HudState.Alert:
                return "ALERT | PENDING REVIEW";
            default:
                return "COMPANION UNIT";
        }
    }

    private void AnimateStatusDot()
    {
        if (statusDot == null)
        {
            return;
        }

        Color color = GetStatusColor(currentHudState);
        float pulseSpeed = currentHudState == HudState.Alert ? 7.2f : 3.2f;
        float pulse = currentHudState == HudState.Dormant ? 0.72f : Mathf.Lerp(0.58f, 1f, (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f);
        color.a = pulse;
        statusDot.color = color;
    }

    private void FlashTrust(string label)
    {
        if (trustRoutine != null)
        {
            StopCoroutine(trustRoutine);
        }

        trustRoutine = StartCoroutine(FlashTrustRoutine(label));
    }

    private IEnumerator FlashTrustRoutine(string label)
    {
        if (trustText == null)
        {
            yield break;
        }

        trustText.text = label;
        Color color = label.StartsWith("-") ? new Color(1f, 0.36f, 0.3f, 1f) : new Color(0.35f, 1f, 0.72f, 1f);
        float duration = 2.8f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            color.a = Mathf.Clamp01(1f - elapsed / duration);
            trustText.color = color;
            yield return null;
        }

        trustText.text = string.Empty;
        trustRoutine = null;
    }

    private void FadePageIn()
    {
        CanvasGroup group = pageRoot.GetComponent<CanvasGroup>();
        if (pageFadeRoutine != null)
        {
            StopCoroutine(pageFadeRoutine);
        }

        pageFadeRoutine = StartCoroutine(FadeCanvasGroup(group, 0f, 1f, 0.18f));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float from, float to, float duration)
    {
        if (group == null)
        {
            yield break;
        }

        float elapsed = 0f;
        group.alpha = from;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        group.alpha = to;
    }

    private void ClearTemporaryObjects()
    {
        for (int i = temporaryObjects.Count - 1; i >= 0; i--)
        {
            if (temporaryObjects[i] != null && temporaryObjects[i].transform.IsChildOf(pageRoot))
            {
                Destroy(temporaryObjects[i]);
            }
        }

        temporaryObjects.RemoveAll(item => item == null || item.transform.IsChildOf(pageRoot));
    }
}
