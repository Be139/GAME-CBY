#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class HearthLobbyOpeningBinder
{
    private const string RootPath = "MIN_LOOP_ROOT/LobbyOpening";
    private const string HumanPath = "Player/Person Controller";
    private const string LobbyReferencePath = "Player/Person Controller (4)";
    private const string ElevatorReferencePath = "Player/Person Controller (5)";
    private const string TaskTerminalPath = "1F (1)/TvUnitSet5";
    private const string ElevatorButtonPath = "DIKUAIunity/Group1/Group144/Rectangle2106772232";
    private const string GirlZonePath = "1F (1)/Girl_A_Rigged (2)/space";
    private const string YoungManZonePath = "1F (1)/casual_Male_G@Sitting (1)/space1";
    private const string GrandmotherZonePath = "1F (1)/casual_Male_G@Sitting (1)/Sitting_Idle (2)/space2";
    private const string DialogueFolder = "Assets/Data/MinLoop/Dialogues/Lobby";
    private const string TerminalPrefabPath = "Assets/Prefabs/UI/HearthHud/Terminals/Terminal_Lobby_Assignment.prefab";
    private const string FinalScriptFileName = "HEARTH_Full_Game_Script_No_Audio_Tags_Native_English.md";

    [MenuItem("Tools/Hearth/Lobby/Apply Ground Floor Opening Setup")]
    public static void ApplySetup()
    {
        if (Application.isPlaying)
        {
            Debug.LogError("[HearthLobbyOpeningBinder] Exit Play Mode before applying the lobby setup.");
            return;
        }

        Transform human = FindTransform(HumanPath);
        Transform lobbyReference = FindTransform(LobbyReferencePath);
        Transform elevatorReference = FindTransform(ElevatorReferencePath);
        Transform taskTerminalRoot = FindTransform(TaskTerminalPath);
        Transform elevatorButton = FindTransform(ElevatorButtonPath);
        Transform girlZoneTransform = FindTransform(GirlZonePath);
        Transform youngManZoneTransform = FindTransform(YoungManZonePath);
        Transform grandmotherZoneTransform = FindTransform(GrandmotherZonePath);

        if (human == null || lobbyReference == null || elevatorReference == null ||
            taskTerminalRoot == null || elevatorButton == null || girlZoneTransform == null ||
            youngManZoneTransform == null || grandmotherZoneTransform == null)
        {
            Debug.LogError("[HearthLobbyOpeningBinder] A required object is missing. Run the validation menu for exact paths.");
            ValidateSetup();
            return;
        }

        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Prefabs/UI");
        EnsureFolder("Assets/Prefabs/UI/HearthHud");
        EnsureFolder("Assets/Prefabs/UI/HearthHud/Terminals");
        EnsureFolder("Assets/Data");
        EnsureFolder("Assets/Data/MinLoop");
        EnsureFolder("Assets/Data/MinLoop/Dialogues");
        EnsureFolder(DialogueFolder);

        DialogueLibrary dialogues = EnsureDialogueAssets();
        if (!HearthFinalDialogueSync.SyncAllFromFinalScript(false))
        {
            Debug.LogError("[HearthLobbyOpeningBinder] Lobby setup stopped because the final dialogue source could not be synchronized.");
            return;
        }
        dialogues.Floor17Arrival = AssetDatabase.LoadAssetAtPath<HearthDialogueSequence>(
            "Assets/Data/MinLoop/Dialogues/FinalScriptSupplemental/17F01_CorridorArrival.asset");
        BuildAssignmentTerminalPrefab();

        Transform root = EnsureHierarchy(RootPath);
        Transform anchors = EnsureChild(root, "Anchors");
        Transform interactions = EnsureChild(root, "Interactions");
        Transform ui = EnsureChild(root, "UI");
        Transform controllerHost = EnsureChild(root, "HearthLobbyFlowController");

        Camera humanCamera = human.GetComponentInChildren<Camera>(true);
        Camera lobbyReferenceCamera = lobbyReference.GetComponentInChildren<Camera>(true);
        Camera elevatorReferenceCamera = elevatorReference.GetComponentInChildren<Camera>(true);
        Transform floor17Anchor = EnsurePoseAnchor(anchors, "Anchor_Mia_17F_Arrival", human, false);
        Transform floor17CameraAnchor = EnsurePoseAnchor(anchors, "Anchor_Mia_17F_Arrival_Camera", humanCamera != null ? humanCamera.transform : human, false);

        PrepareReferenceController(lobbyReference.gameObject);
        PrepareReferenceController(elevatorReference.gameObject);

        HearthLobbyFlowController flow = GetOrAdd<HearthLobbyFlowController>(controllerHost.gameObject);
        LobbyUiReferences lobbyUi = EnsureLobbyUi(ui);
        HearthTvTerminalController terminal = ConfigureAssignmentTerminal(taskTerminalRoot, human, flow);
        HearthLobbyElevatorInteractable elevatorInteraction = ConfigureElevatorInteraction(interactions, elevatorButton, flow);

        HearthLobbyConversationZone girlZone = ConfigureConversationZone(girlZoneTransform, flow, human, dialogues.Girl, dialogues.GirlExit);
        HearthLobbyConversationZone youngManZone = ConfigureConversationZone(youngManZoneTransform, flow, human, dialogues.YoungMan, dialogues.YoungManExit);
        HearthLobbyConversationZone grandmotherZone = ConfigureConversationZone(grandmotherZoneTransform, flow, human, dialogues.Grandmother, dialogues.GrandmotherExit);

        MinLoopSubtitlePlayer subtitlePlayer = UnityEngine.Object.FindObjectOfType<MinLoopSubtitlePlayer>(true);
        if (subtitlePlayer == null)
        {
            GameObject subtitleHost = new GameObject("LobbySubtitlePlayer");
            subtitleHost.transform.SetParent(ui, false);
            subtitlePlayer = Undo.AddComponent<MinLoopSubtitlePlayer>(subtitleHost);
        }

        HearthFirstPersonHudInput hudInput = UnityEngine.Object.FindObjectOfType<HearthFirstPersonHudInput>(true);
        HearthLocationProbe locationProbe = UnityEngine.Object.FindObjectOfType<HearthLocationProbe>(true);
        ConfigureFlow(
            flow,
            human,
            humanCamera,
            lobbyReference,
            lobbyReferenceCamera,
            elevatorReference,
            elevatorReferenceCamera,
            floor17Anchor,
            floor17CameraAnchor,
            subtitlePlayer,
            lobbyUi,
            locationProbe,
            terminal,
            new[] { girlZone, youngManZone, grandmotherZone },
            hudInput,
            dialogues);

        if (terminal != null)
        {
            ClearPersistentListeners(terminal.OnOpened);
            ClearPersistentListeners(terminal.OnCustomPrimaryAction);
            UnityEventTools.AddPersistentListener(terminal.OnOpened, flow.BeginAssignmentBriefingFromTerminal);
            UnityEventTools.AddPersistentListener(terminal.OnCustomPrimaryAction, flow.ConfirmAssignmentTerminalClose);
            EditorUtility.SetDirty(terminal);
        }

        elevatorInteraction.Configure(flow);
        EditorUtility.SetDirty(elevatorInteraction);
        EditorUtility.SetDirty(flow);
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());

        Debug.Log("[HearthLobbyOpeningBinder] Ground-floor opening applied. Existing NPC, trigger, terminal camera, and reference-controller transforms were preserved.");
        ValidateSetup();
    }

    [MenuItem("Tools/Hearth/Lobby/Capture Current Player Pose As 17F Arrival")]
    public static void CaptureCurrentPlayerAsFloor17Arrival()
    {
        if (Application.isPlaying)
        {
            Debug.LogError("[HearthLobbyOpeningBinder] Exit Play Mode before capturing an arrival pose.");
            return;
        }

        Transform human = FindTransform(HumanPath);
        Transform root = FindTransform(RootPath);
        if (human == null || root == null)
        {
            Debug.LogError("[HearthLobbyOpeningBinder] Apply the lobby setup before capturing the 17F arrival pose.");
            return;
        }

        Transform anchors = EnsureChild(root, "Anchors");
        Transform humanCamera = human.GetComponentInChildren<Camera>(true) != null
            ? human.GetComponentInChildren<Camera>(true).transform
            : human;
        EnsurePoseAnchor(anchors, "Anchor_Mia_17F_Arrival", human, true);
        EnsurePoseAnchor(anchors, "Anchor_Mia_17F_Arrival_Camera", humanCamera, true);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Debug.Log("[HearthLobbyOpeningBinder] Captured the current formal player pose as the 17F arrival destination.");
    }

    [MenuItem("Tools/Hearth/Lobby/Show And Align Task Terminal Canvas For Editing")]
    public static void ShowAndAlignTaskTerminalCanvasForEditing()
    {
        if (Application.isPlaying)
        {
            Debug.LogError("[HearthLobbyOpeningBinder] Exit Play Mode before editing the task terminal Canvas.");
            return;
        }

        Transform terminalRoot = FindTransform(TaskTerminalPath);
        if (terminalRoot == null)
        {
            Debug.LogError("[HearthLobbyOpeningBinder] Task terminal was not found at " + TaskTerminalPath + ".");
            return;
        }

        HearthTvTerminalController terminal = terminalRoot.GetComponentInChildren<HearthTvTerminalController>(true);
        Transform monitorCanvas = terminalRoot.Find("MonitorCanvas");
        Camera terminalCamera = terminalRoot.GetComponentsInChildren<Camera>(true).FirstOrDefault(item => item.name == "Camera")
            ?? terminalRoot.GetComponentInChildren<Camera>(true);
        if (terminal == null || monitorCanvas == null)
        {
            Debug.LogError("[HearthLobbyOpeningBinder] Task terminal controller or MonitorCanvas is missing. Run Apply Ground Floor Opening Setup first.");
            return;
        }

        Undo.RecordObject(terminal, "Show task terminal Canvas in Edit Mode");
        terminal.SetHideCanvasWhenClosed(true);
        terminal.SetShowCanvasInEditMode(true);
        AlignMonitorCanvasToScreen(terminalRoot, terminalCamera);

        Canvas canvas = monitorCanvas.GetComponent<Canvas>();
        if (canvas != null)
        {
            Undo.RecordObject(canvas, "Enable task terminal Canvas in Edit Mode");
            canvas.enabled = true;
            EditorUtility.SetDirty(canvas);
        }

        EditorUtility.SetDirty(terminal);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Selection.activeGameObject = monitorCanvas.gameObject;
        Debug.Log("[HearthLobbyOpeningBinder] Task terminal Canvas is visible in Edit Mode and aligned to TaskTerminalScreenAnchor. The terminal Camera was not changed.");
    }

    [MenuItem("Tools/Hearth/Lobby/Capture Task Terminal Canvas Placement")]
    public static void CaptureTaskTerminalCanvasPlacement()
    {
        if (Application.isPlaying)
        {
            Debug.LogError("[HearthLobbyOpeningBinder] Exit Play Mode before capturing the task terminal Canvas placement.");
            return;
        }

        Transform terminalRoot = FindTransform(TaskTerminalPath);
        Transform monitorCanvas = terminalRoot != null ? terminalRoot.Find("MonitorCanvas") : null;
        Transform screenAnchor = terminalRoot != null ? terminalRoot.Find("TaskTerminalScreenAnchor") : null;
        if (monitorCanvas == null || screenAnchor == null)
        {
            Debug.LogError("[HearthLobbyOpeningBinder] MonitorCanvas or TaskTerminalScreenAnchor is missing.");
            return;
        }

        Undo.RecordObject(screenAnchor, "Capture task terminal Canvas placement");
        screenAnchor.SetPositionAndRotation(monitorCanvas.position, monitorCanvas.rotation);
        screenAnchor.localScale = monitorCanvas.localScale;
        EditorUtility.SetDirty(screenAnchor);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Debug.Log("[HearthLobbyOpeningBinder] Current MonitorCanvas position, rotation, and scale were saved to TaskTerminalScreenAnchor.");
    }

    [MenuItem("Tools/Hearth/Lobby/Validate Ground Floor Opening Setup")]
    public static void ValidateSetup()
    {
        List<string> issues = new List<string>();
        RequirePath(issues, HumanPath);
        RequirePath(issues, LobbyReferencePath);
        RequirePath(issues, ElevatorReferencePath);
        RequirePath(issues, TaskTerminalPath);
        RequirePath(issues, ElevatorButtonPath);
        RequirePath(issues, GirlZonePath);
        RequirePath(issues, YoungManZonePath);
        RequirePath(issues, GrandmotherZonePath);
        RequirePath(issues, RootPath + "/HearthLobbyFlowController");
        RequirePath(issues, RootPath + "/Anchors/Anchor_Mia_17F_Arrival");

        Transform terminalRoot = FindTransform(TaskTerminalPath);
        if (terminalRoot != null)
        {
            HearthTvTerminalController terminal = terminalRoot.GetComponentInChildren<HearthTvTerminalController>(true);
            if (terminal == null)
            {
                issues.Add("The lobby assignment terminal has no HearthTvTerminalController.");
            }

            if (terminalRoot.GetComponent<HearthLobbyTaskTerminalInteractable>() == null)
            {
                issues.Add("The lobby assignment terminal has no lobby-specific E interaction.");
            }

            Transform screenAnchor = terminalRoot.Find("TaskTerminalScreenAnchor");
            Transform monitorCanvas = terminalRoot.Find("MonitorCanvas");
            if (screenAnchor == null)
            {
                issues.Add("The lobby assignment terminal has no editable TaskTerminalScreenAnchor.");
            }
            else if (monitorCanvas == null)
            {
                issues.Add("The lobby assignment terminal has no MonitorCanvas.");
            }
            else
            {
                float positionError = Vector3.Distance(screenAnchor.position, monitorCanvas.position);
                float rotationError = Quaternion.Angle(screenAnchor.rotation, monitorCanvas.rotation);
                if (positionError > 0.003f || rotationError > 0.5f)
                {
                    issues.Add("MonitorCanvas is not aligned to TaskTerminalScreenAnchor (position " + positionError.ToString("F4") + "m, rotation " + rotationError.ToString("F2") + " deg).");
                }
            }

            Camera terminalCamera = terminalRoot.GetComponentsInChildren<Camera>(true).FirstOrDefault(item => item.name == "Camera");
            if (terminalCamera == null)
            {
                issues.Add("The lobby assignment terminal camera is missing.");
            }
            else if (terminalCamera.enabled)
            {
                issues.Add("The lobby assignment terminal camera must be disabled outside terminal view.");
            }
        }

        ValidateConversationZone(issues, GirlZonePath);
        ValidateConversationZone(issues, YoungManZonePath);
        ValidateConversationZone(issues, GrandmotherZonePath);

        HearthLobbyElevatorInteractable elevator = UnityEngine.Object.FindObjectOfType<HearthLobbyElevatorInteractable>(true);
        if (elevator == null || elevator.GetComponent<Collider>() == null)
        {
            issues.Add("The elevator button interaction volume is missing.");
        }

        if (!System.IO.File.Exists(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), FinalScriptFileName)))
        {
            issues.Add("The finalized narrative source is missing from the project root: " + FinalScriptFileName);
        }

        Camera[] enabledCameras = UnityEngine.Object.FindObjectsOfType<Camera>(true)
            .Where(item => item.enabled && item.gameObject.activeInHierarchy)
            .ToArray();
        AudioListener[] enabledListeners = UnityEngine.Object.FindObjectsOfType<AudioListener>(true)
            .Where(item => item.enabled && item.gameObject.activeInHierarchy)
            .ToArray();
        if (enabledListeners.Length > 1)
        {
            issues.Add("More than one AudioListener is enabled in edit state: " + enabledListeners.Length + ".");
        }

        if (issues.Count == 0)
        {
            Debug.Log("[HearthLobbyOpeningBinder] Validation passed. Enabled cameras: " + enabledCameras.Length + ", enabled AudioListeners: " + enabledListeners.Length + ".");
            return;
        }

        Debug.LogError("[HearthLobbyOpeningBinder] Validation found " + issues.Count + " issue(s):\n- " + string.Join("\n- ", issues.ToArray()));
    }

    private static void BuildAssignmentTerminalPrefab()
    {
        GameObject root = new GameObject(
            "Terminal_Lobby_Assignment",
            typeof(RectTransform),
            typeof(CanvasGroup),
            typeof(AudioSource),
            typeof(HearthTerminalCameraTransition),
            typeof(HearthTerminalBootSequence),
            typeof(HearthUiPressFeedback),
            typeof(HearthTvTerminalController));
        Stretch(root.GetComponent<RectTransform>());

        CanvasGroup rootGroup = root.GetComponent<CanvasGroup>();
        rootGroup.alpha = 1f;
        rootGroup.interactable = false;
        rootGroup.blocksRaycasts = false;
        AudioSource audio = root.GetComponent<AudioSource>();
        audio.playOnAwake = false;
        audio.spatialBlend = 0f;

        GameObject activeLoopObject = new GameObject("TerminalActiveLoop", typeof(AudioSource), typeof(HearthAudioChannelSource));
        activeLoopObject.transform.SetParent(root.transform, false);
        AudioSource activeLoopSource = activeLoopObject.GetComponent<AudioSource>();
        activeLoopSource.playOnAwake = false;
        activeLoopSource.loop = true;
        activeLoopSource.spatialBlend = 0f;
        activeLoopObject.GetComponent<HearthAudioChannelSource>().Configure(activeLoopSource, HearthAudioChannel.SFX, 0.35f);

        CreateImage(root.transform, "TerminalScreenGlass", new Rect(0f, 0f, 1920f, 1080f), new Color(0.008f, 0.025f, 0.038f, 1f));
        GameObject content = new GameObject("TerminalContentRoot", typeof(RectTransform), typeof(CanvasGroup));
        content.transform.SetParent(root.transform, false);
        Stretch(content.GetComponent<RectTransform>());
        CanvasGroup contentGroup = content.GetComponent<CanvasGroup>();

        GameObject pageObject = new GameObject("TerminalSlide01_LobbyAssignment", typeof(RectTransform), typeof(CanvasGroup), typeof(HearthHudPage));
        pageObject.transform.SetParent(content.transform, false);
        Stretch(pageObject.GetComponent<RectTransform>());
        HearthHudPage page = pageObject.GetComponent<HearthHudPage>();
        page.Configure(HearthHudPageId.Slide01PersistentActive, false, HearthHudState.Active, string.Empty, false, string.Empty, string.Empty);
        BuildAssignmentPage(pageObject.transform);

        GameObject keyboard = new GameObject("KeyboardNavigationRoot", typeof(RectTransform));
        keyboard.transform.SetParent(root.transform, false);
        Stretch(keyboard.GetComponent<RectTransform>());
        CreateText(keyboard.transform, "KeyboardHintText", "SPACE LOAD ASSIGNMENT     ESC EXIT", new Rect(94f, 1005f, 1000f, 34f), 18f, new Color(0.65f, 0.86f, 0.92f, 0.9f), TextAlignmentOptions.TopLeft);
        CreateText(keyboard.transform, "KeyboardFocusText", "LOAD ROUTE | SPACE", new Rect(1120f, 1005f, 700f, 34f), 19f, new Color(0.35f, 0.95f, 0.78f, 0.98f), TextAlignmentOptions.TopRight);
        TMP_Text runtimePrompt = CreateText(keyboard.transform, "RuntimePromptText", string.Empty, new Rect(560f, 92f, 800f, 38f), 19f, new Color(0.78f, 0.96f, 1f, 0.96f), TextAlignmentOptions.Center);
        runtimePrompt.gameObject.SetActive(false);

        GameObject boot = new GameObject("TerminalBootOverlay", typeof(RectTransform), typeof(CanvasGroup));
        boot.transform.SetParent(root.transform, false);
        Stretch(boot.GetComponent<RectTransform>());
        CreateImage(boot.transform, "BootFlash", new Rect(0f, 0f, 1920f, 1080f), new Color(0.48f, 0.94f, 0.88f, 0.24f));
        CreateImage(boot.transform, "BootScanlines", new Rect(0f, 0f, 1920f, 1080f), new Color(0.28f, 0.8f, 0.82f, 0.1f));

        GameObject off = new GameObject("TerminalOffOverlay", typeof(RectTransform), typeof(CanvasGroup));
        off.transform.SetParent(root.transform, false);
        Stretch(off.GetComponent<RectTransform>());
        CreateImage(off.transform, "OffScreen", new Rect(0f, 0f, 1920f, 1080f), new Color(0.003f, 0.006f, 0.01f, 0.98f));

        HearthTerminalBootSequence bootSequence = root.GetComponent<HearthTerminalBootSequence>();
        bootSequence.Configure(contentGroup, off.GetComponent<CanvasGroup>(), boot.GetComponent<CanvasGroup>(), content.GetComponent<RectTransform>());

        HearthTerminalCameraTransition transition = root.GetComponent<HearthTerminalCameraTransition>();
        SerializedObject transitionSo = new SerializedObject(transition);
        SetBool(transitionSo, "smoothTransitionEnabled", true);
        SetFloat(transitionSo, "enterDuration", 0.5f);
        SetFloat(transitionSo, "exitDuration", 0.5f);
        SetBool(transitionSo, "smoothExit", true);
        SetBool(transitionSo, "useUnscaledTime", true);
        SetBool(transitionSo, "copyAudioListenerIfMissing", true);
        transitionSo.ApplyModifiedPropertiesWithoutUndo();

        HearthTvTerminalController controller = root.GetComponent<HearthTvTerminalController>();
        controller.Configure(null, null, content.GetComponent<RectTransform>(), rootGroup, new[] { page }, 1, HearthHudPageId.Slide01PersistentActive, 1f);
        controller.SetActiveLoopAudio(activeLoopSource, null);
        controller.SetPrimaryAction(HearthTerminalPrimaryAction.Custom);
        controller.SetSubmitPrimaryActionFromCurrentPage(true);
        SerializedObject controllerSo = new SerializedObject(controller);
        SetInt(controllerSo, "keyboardCyclePageCount", 1);
        SetBool(controllerSo, "pageDrivenSelectionStates", false);
        SetBool(controllerSo, "showFinalChoiceWhenReplayUnavailable", false);
        SetBool(controllerSo, "closeTerminalWhenReplayStarts", false);
        SetBool(controllerSo, "deferCustomActionCloseUntilExternalFade", false);
        SetBool(controllerSo, "unlockCursorWhileOpen", false);
        SetString(controllerSo, "replayFocusLabel", "LOAD ROUTE | SPACE");
        SetString(controllerSo, "keyboardHintLabel", "SPACE LOAD ASSIGNMENT     ESC EXIT");
        SetString(controllerSo, "replayResidentId", "LOBBY");
        controllerSo.ApplyModifiedPropertiesWithoutUndo();

        HearthUiPressFeedback feedback = root.GetComponent<HearthUiPressFeedback>();
        feedback.Configure(new Graphic[]
        {
            pageObject.transform.Find("ConfirmBack").GetComponent<Graphic>(),
            pageObject.transform.Find("ConfirmText").GetComponent<Graphic>()
        });
        controller.SetSubmitFeedback(feedback);

        PrefabUtility.SaveAsPrefabAsset(root, TerminalPrefabPath);
        UnityEngine.Object.DestroyImmediate(root);
    }

    private static void BuildAssignmentPage(Transform parent)
    {
        Color cyan = new Color(0.34f, 0.88f, 0.96f, 0.94f);
        Color soft = new Color(0.68f, 0.83f, 0.88f, 0.86f);
        Color green = new Color(0.35f, 0.95f, 0.76f, 0.95f);

        CreateImage(parent, "TopRule", new Rect(100f, 90f, 1720f, 2f), cyan);
        CreateImage(parent, "LeftRule", new Rect(100f, 90f, 2f, 830f), cyan);
        CreateText(parent, "SystemLabel", "HEARTH SYNCHRONIZED ASSIGNMENT TERMINAL", new Rect(138f, 122f, 1100f, 46f), 22f, cyan, TextAlignmentOptions.TopLeft);
        CreateText(parent, "Inspector", "INSPECTOR ID 7842  /  PARTNER MIA", new Rect(138f, 168f, 1000f, 38f), 18f, soft, TextAlignmentOptions.TopLeft);
        CreateText(parent, "AssignmentTitle", "TONIGHT'S ASSIGNMENT", new Rect(138f, 256f, 1120f, 72f), 42f, Color.white, TextAlignmentOptions.TopLeft);
        CreateText(parent, "Floor", "FLOOR 17", new Rect(138f, 325f, 760f, 70f), 34f, green, TextAlignmentOptions.TopLeft);

        CreateAssignmentRow(parent, 430f, "17F-01", "ROUTINE REVIEW", "UPGRADE REQUEST PENDING", cyan, soft);
        CreateAssignmentRow(parent, 550f, "17F-02", "ROUTINE REVIEW", "FORCED-SHUTDOWN INCIDENT", cyan, soft);
        CreateAssignmentRow(parent, 670f, "17F-03", "FLAGGED", "FOLLOW-UP REQUIRED", new Color(1f, 0.48f, 0.3f, 0.95f), soft);

        CreateText(parent, "ResidenceNote", "ONE GUARDIAN CONFIRMATION IS PENDING AT YOUR OWN RESIDENCE.  HANDLE OFF-SHIFT.", new Rect(138f, 814f, 1500f, 54f), 20f, soft, TextAlignmentOptions.TopLeft);
        CreateImage(parent, "ConfirmBack", new Rect(1260f, 882f, 500f, 74f), new Color(0.12f, 0.56f, 0.45f, 0.16f));
        AddBorder(parent, new Rect(1260f, 882f, 500f, 74f), new Color(0.35f, 0.95f, 0.76f, 0.72f), 2f);
        CreateText(parent, "ConfirmText", "SPACE  LOAD ROUTE", new Rect(1260f, 902f, 500f, 40f), 22f, green, TextAlignmentOptions.Center);
    }

    private static void CreateAssignmentRow(Transform parent, float y, string room, string type, string detail, Color accent, Color soft)
    {
        CreateImage(parent, room + "Rule", new Rect(138f, y, 1540f, 1f), new Color(accent.r, accent.g, accent.b, 0.35f));
        CreateText(parent, room + "Room", room, new Rect(138f, y + 20f, 260f, 60f), 30f, accent, TextAlignmentOptions.TopLeft);
        CreateText(parent, room + "Type", type, new Rect(430f, y + 18f, 450f, 38f), 20f, Color.white, TextAlignmentOptions.TopLeft);
        CreateText(parent, room + "Detail", detail, new Rect(430f, y + 57f, 900f, 34f), 17f, soft, TextAlignmentOptions.TopLeft);
    }

    private static HearthTvTerminalController ConfigureAssignmentTerminal(
        Transform terminalRoot,
        Transform human,
        HearthLobbyFlowController flow)
    {
        HearthTvTerminalController terminal = terminalRoot.GetComponentInChildren<HearthTvTerminalController>(true);
        bool createdTerminal = terminal == null;
        if (createdTerminal && !HearthTvTerminalPrefabBuilder.StandardizeTvTerminal(terminalRoot, TerminalPrefabPath))
        {
            return null;
        }

        terminal = terminalRoot.GetComponentInChildren<HearthTvTerminalController>(true);
        Camera terminalCamera = terminalRoot.GetComponentsInChildren<Camera>(true).FirstOrDefault(item => item.name == "Camera")
            ?? terminalRoot.GetComponentInChildren<Camera>(true);
        Camera humanCamera = human.GetComponentInChildren<Camera>(true);
        PlayerInteraction humanInteraction = human.GetComponent<PlayerInteraction>();
        FirstPersonMovement movement = human.GetComponent<FirstPersonMovement>();
        FirstPersonLook look = humanCamera != null ? humanCamera.GetComponent<FirstPersonLook>() : null;
        HearthFirstPersonHudInput hudInput = UnityEngine.Object.FindObjectOfType<HearthFirstPersonHudInput>(true);

        terminal.SetPrimaryAction(HearthTerminalPrimaryAction.Custom);
        terminal.SetSubmitPrimaryActionFromCurrentPage(true);
        terminal.SetDeferCustomActionCloseUntilExternalFade(false);
        terminal.SetReplayResidentId("LOBBY");
        terminal.SetPlayerInteraction(humanInteraction);
        terminal.SetPlayerCamera(humanCamera);
        terminal.SetTerminalCamera(terminalCamera);
        terminal.SetSwitchCameraWhileOpen(terminalCamera != null);
        terminal.SetHideCanvasWhenClosed(true);
        terminal.SetShowCanvasInEditMode(true);
        terminal.SetChoiceInputEnabled(true);
        EnsureRuntimePromptText(terminal);

        SerializedObject terminalSo = new SerializedObject(terminal);
        SetBool(terminalSo, "closeTerminalWhenReplayStarts", false);
        SetBool(terminalSo, "unlockCursorWhileOpen", false);
        SetObject(terminalSo, "playerRigidbody", human.GetComponent<Rigidbody>());
        SetObjectArray(terminalSo, "gameplayBehavioursToDisable", new Behaviour[] { movement, look, hudInput }.Where(item => item != null).ToArray());
        terminalSo.ApplyModifiedPropertiesWithoutUndo();

        if (terminalCamera != null)
        {
            terminalCamera.enabled = false;
            AudioListener listener = terminalCamera.GetComponent<AudioListener>();
            if (listener != null) listener.enabled = false;
        }

        if (createdTerminal)
        {
            AlignMonitorCanvasToScreen(terminalRoot, terminalCamera);
            FitTerminalColliderToScreen(terminalRoot);
        }

        foreach (HearthTvTerminalInteractable oldInteractable in terminalRoot.GetComponents<HearthTvTerminalInteractable>())
        {
            Undo.DestroyObjectImmediate(oldInteractable);
        }

        HearthLobbyTaskTerminalInteractable interactable = GetOrAdd<HearthLobbyTaskTerminalInteractable>(terminalRoot.gameObject);
        interactable.Configure(flow, terminal);
        EditorUtility.SetDirty(interactable);
        EditorUtility.SetDirty(terminalRoot.gameObject);
        return terminal;
    }

    private static void AlignMonitorCanvasToScreen(Transform terminalRoot, Camera terminalCamera)
    {
        Transform canvasTransform = terminalRoot.Find("MonitorCanvas");
        Renderer screenRenderer = terminalRoot.Find("TV") != null
            ? terminalRoot.Find("TV").GetComponent<Renderer>()
            : terminalRoot.GetComponentInChildren<Renderer>(true);
        if (canvasTransform == null || screenRenderer == null)
        {
            return;
        }

        Transform screenAnchor = terminalRoot.Find("TaskTerminalScreenAnchor");
        if (screenAnchor == null)
        {
            GameObject anchorObject = new GameObject("TaskTerminalScreenAnchor");
            Undo.RegisterCreatedObjectUndo(anchorObject, "Create task terminal screen anchor");
            screenAnchor = anchorObject.transform;
            screenAnchor.SetParent(terminalRoot, false);

            Vector3 screenCenter = screenRenderer.bounds.center;
            Vector3 towardCamera = terminalCamera != null
                ? terminalCamera.transform.position - screenCenter
                : terminalRoot.forward;
            if (towardCamera.sqrMagnitude < 0.0001f)
            {
                towardCamera = terminalRoot.forward;
            }
            towardCamera.Normalize();

            Vector3 extents = screenRenderer.bounds.extents;
            float surfaceDistance =
                Mathf.Abs(towardCamera.x) * extents.x +
                Mathf.Abs(towardCamera.y) * extents.y +
                Mathf.Abs(towardCamera.z) * extents.z;
            screenAnchor.position = screenCenter + towardCamera * (surfaceDistance + 0.008f);
            // A World Space Canvas renders its readable face opposite its Transform.forward.
            screenAnchor.rotation = Quaternion.LookRotation(-towardCamera, Vector3.up);
            float horizontalWorldSize = Mathf.Max(screenRenderer.bounds.size.x, screenRenderer.bounds.size.z);
            float scale = Mathf.Min(horizontalWorldSize / 1920f, screenRenderer.bounds.size.y / 1080f) * 0.92f;
            screenAnchor.localScale = Vector3.one * Mathf.Max(0.0001f, scale);
        }

        canvasTransform.SetPositionAndRotation(screenAnchor.position, screenAnchor.rotation);
        canvasTransform.localScale = screenAnchor.localScale;

        Canvas canvas = canvasTransform.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.worldCamera = terminalCamera;
        }

    }

    private static void FitTerminalColliderToScreen(Transform terminalRoot)
    {
        Renderer screenRenderer = terminalRoot.Find("TV") != null
            ? terminalRoot.Find("TV").GetComponent<Renderer>()
            : terminalRoot.GetComponentInChildren<Renderer>(true);
        if (screenRenderer == null)
        {
            return;
        }

        BoxCollider collider = terminalRoot.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = Undo.AddComponent<BoxCollider>(terminalRoot.gameObject);
        }

        Bounds localBounds = TransformWorldBoundsToLocal(terminalRoot, screenRenderer.bounds);
        collider.center = localBounds.center;
        collider.size = new Vector3(
            Mathf.Max(0.35f, localBounds.size.x + 0.18f),
            Mathf.Max(0.5f, localBounds.size.y + 0.18f),
            Mathf.Max(0.35f, localBounds.size.z + 0.18f));
        collider.isTrigger = false;
        collider.enabled = true;
    }

    private static HearthLobbyElevatorInteractable ConfigureElevatorInteraction(
        Transform interactionsRoot,
        Transform elevatorButton,
        HearthLobbyFlowController flow)
    {
        Renderer renderer = elevatorButton.GetComponentInChildren<Renderer>(true);
        Transform volume = EnsureChild(interactionsRoot, "InteractionVolume_LobbyElevatorButton");
        if (renderer != null)
        {
            volume.position = renderer.bounds.center;
            volume.rotation = Quaternion.identity;
        }

        BoxCollider collider = GetOrAdd<BoxCollider>(volume.gameObject);
        Vector3 visibleSize = renderer != null ? renderer.bounds.size : new Vector3(0.2f, 0.3f, 0.1f);
        collider.center = Vector3.zero;
        collider.size = new Vector3(
            Mathf.Max(0.42f, visibleSize.x + 0.24f),
            Mathf.Max(0.52f, visibleSize.y + 0.24f),
            Mathf.Max(0.34f, visibleSize.z + 0.24f));
        collider.isTrigger = false;
        collider.enabled = true;

        HearthLobbyElevatorInteractable interactable = GetOrAdd<HearthLobbyElevatorInteractable>(volume.gameObject);
        interactable.Configure(flow);
        return interactable;
    }

    private static HearthLobbyConversationZone ConfigureConversationZone(
        Transform zoneTransform,
        HearthLobbyFlowController flow,
        Transform human,
        HearthDialogueSequence exchange,
        HearthDialogueSequence exitCommentary)
    {
        Collider trigger = zoneTransform.GetComponent<Collider>();
        if (trigger == null)
        {
            trigger = Undo.AddComponent<BoxCollider>(zoneTransform.gameObject);
        }
        trigger.isTrigger = true;
        trigger.enabled = true;

        HearthLobbyConversationZone zone = GetOrAdd<HearthLobbyConversationZone>(zoneTransform.gameObject);
        zone.Configure(flow, exchange, exitCommentary, human, true);
        EditorUtility.SetDirty(zone);
        return zone;
    }

    private static LobbyUiReferences EnsureLobbyUi(Transform uiRoot)
    {
        Transform existingNarrative = FindDirectChild(uiRoot, "LobbyNarrativeCanvas");
        Transform existingBlackout = FindDirectChild(uiRoot, "LobbyBlackoutCanvas");
        HearthLobbyHudOverlay existingOverlay = existingNarrative != null
            ? existingNarrative.GetComponentInChildren<HearthLobbyHudOverlay>(true)
            : null;
        HearthScreenFader existingFader = existingBlackout != null
            ? existingBlackout.GetComponentInChildren<HearthScreenFader>(true)
            : null;
        if (existingOverlay != null && existingFader != null)
        {
            Transform existingActivation = existingOverlay.transform.Find("ActivationPanel");
            RectTransform activationRect = existingActivation != null
                ? existingActivation.GetComponent<RectTransform>()
                : null;
            if (activationRect != null && Mathf.Abs(activationRect.anchoredPosition.y + 72f) < 0.5f)
            {
                activationRect.anchoredPosition = new Vector2(activationRect.anchoredPosition.x, -190f);
            }

            ReplaceText(existingOverlay.transform, "ExpandedLilyMessage/MessageMeta", "FROM  LILY\nTIME  4:42 PM");
            ReplaceText(existingOverlay.transform, "PinnedLilyMessage/PinnedMessage", "LILY VOICE MESSAGE  /  READ  /  4:42 PM");
            EditorUtility.SetDirty(existingOverlay);
            return new LobbyUiReferences { Overlay = existingOverlay, Fader = existingFader };
        }

        DestroyDirectChild(uiRoot, "LobbyNarrativeCanvas");
        DestroyDirectChild(uiRoot, "LobbyBlackoutCanvas");

        GameObject narrativeCanvasObject = new GameObject("LobbyNarrativeCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        narrativeCanvasObject.transform.SetParent(uiRoot, false);
        Canvas narrativeCanvas = narrativeCanvasObject.GetComponent<Canvas>();
        narrativeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        narrativeCanvas.overrideSorting = true;
        narrativeCanvas.sortingOrder = 7200;
        CanvasScaler scaler = narrativeCanvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        GameObject overlayObject = new GameObject("HearthLobbyHudOverlay", typeof(RectTransform), typeof(HearthLobbyHudOverlay));
        overlayObject.transform.SetParent(narrativeCanvasObject.transform, false);
        Stretch(overlayObject.GetComponent<RectTransform>());

        CanvasGroup activation = CreateGroup(overlayObject.transform, "ActivationPanel", new Rect(88f, 190f, 690f, 148f));
        CreateImage(activation.transform, "ActivationBack", new Rect(0f, 0f, 690f, 148f), new Color(0.015f, 0.05f, 0.075f, 0.32f));
        CreateImage(activation.transform, "ActivationRule", new Rect(0f, 0f, 4f, 148f), new Color(0.35f, 0.91f, 0.98f, 0.92f));
        CreateText(activation.transform, "ActivationTitle", "FIELD COMPANION UNIT  /  ACTIVATED", new Rect(24f, 20f, 620f, 34f), 19f, new Color(0.45f, 0.9f, 0.98f, 0.98f), TextAlignmentOptions.TopLeft);
        CreateText(activation.transform, "ActivationId", "INSPECTOR ID 7842\nASSIGNED PARTNER  MIA", new Rect(24f, 62f, 620f, 70f), 21f, Color.white, TextAlignmentOptions.TopLeft);

        CanvasGroup expanded = CreateGroup(overlayObject.transform, "ExpandedLilyMessage", new Rect(1280f, 72f, 550f, 292f));
        CreateImage(expanded.transform, "MessageBack", new Rect(0f, 0f, 550f, 292f), new Color(0.015f, 0.04f, 0.062f, 0.58f));
        AddBorder(expanded.transform, new Rect(0f, 0f, 550f, 292f), new Color(0.32f, 0.82f, 0.92f, 0.55f), 1.5f);
        CreateText(expanded.transform, "MessageHeader", "INCOMING VOICE MESSAGE", new Rect(24f, 20f, 500f, 34f), 18f, new Color(0.42f, 0.9f, 0.98f, 0.98f), TextAlignmentOptions.TopLeft);
        CreateText(expanded.transform, "MessageMeta", "FROM  LILY\nTIME  4:42 PM", new Rect(24f, 60f, 500f, 54f), 17f, Color.white, TextAlignmentOptions.TopLeft);
        CreateText(expanded.transform, "MessageTranscript", "Mom, are you getting home late tonight?\nI wanted to tell you something.\nWe can talk when you get home. I'll wait for you.\n...Don't forget, okay?", new Rect(24f, 128f, 500f, 140f), 18f, new Color(0.86f, 0.94f, 0.98f, 0.96f), TextAlignmentOptions.TopLeft);

        CanvasGroup pinned = CreateGroup(overlayObject.transform, "PinnedLilyMessage", new Rect(1370f, 72f, 460f, 112f));
        CreateImage(pinned.transform, "PinnedBack", new Rect(0f, 0f, 460f, 112f), new Color(0.012f, 0.035f, 0.052f, 0.34f));
        CreateImage(pinned.transform, "PinnedRule", new Rect(0f, 0f, 3f, 112f), new Color(0.32f, 0.82f, 0.92f, 0.75f));
        CreateText(pinned.transform, "PinnedMessage", "LILY VOICE MESSAGE  /  READ  /  4:42 PM", new Rect(18f, 16f, 420f, 30f), 16f, new Color(0.65f, 0.88f, 0.94f, 0.94f), TextAlignmentOptions.TopLeft);
        TMP_Text status = CreateText(pinned.transform, "AssignmentStatus", "ASSIGNMENT NOT LOADED", new Rect(18f, 62f, 420f, 30f), 16f, new Color(0.45f, 0.95f, 0.78f, 0.94f), TextAlignmentOptions.TopLeft);

        HearthLobbyHudOverlay overlay = overlayObject.GetComponent<HearthLobbyHudOverlay>();
        overlay.Configure(activation, expanded, pinned, status);

        GameObject blackoutCanvasObject = new GameObject("LobbyBlackoutCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup), typeof(GraphicRaycaster), typeof(HearthScreenFader));
        blackoutCanvasObject.transform.SetParent(uiRoot, false);
        Canvas blackoutCanvas = blackoutCanvasObject.GetComponent<Canvas>();
        blackoutCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        blackoutCanvas.overrideSorting = true;
        blackoutCanvas.sortingOrder = 20000;
        CanvasScaler blackoutScaler = blackoutCanvasObject.GetComponent<CanvasScaler>();
        blackoutScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        blackoutScaler.referenceResolution = new Vector2(1920f, 1080f);
        CreateImage(blackoutCanvasObject.transform, "Blackout", new Rect(0f, 0f, 1920f, 1080f), Color.black);
        CanvasGroup blackoutGroup = blackoutCanvasObject.GetComponent<CanvasGroup>();
        blackoutGroup.alpha = 0f;
        HearthScreenFader fader = blackoutCanvasObject.GetComponent<HearthScreenFader>();
        fader.Configure(blackoutGroup, true);

        return new LobbyUiReferences
        {
            Overlay = overlay,
            Fader = fader
        };
    }

    private static void ConfigureFlow(
        HearthLobbyFlowController flow,
        Transform human,
        Camera humanCamera,
        Transform lobbyReference,
        Camera lobbyReferenceCamera,
        Transform elevatorReference,
        Camera elevatorReferenceCamera,
        Transform floor17Anchor,
        Transform floor17CameraAnchor,
        MinLoopSubtitlePlayer subtitlePlayer,
        LobbyUiReferences ui,
        HearthLocationProbe locationProbe,
        HearthTvTerminalController terminal,
        HearthLobbyConversationZone[] zones,
        HearthFirstPersonHudInput hudInput,
        DialogueLibrary dialogues)
    {
        SerializedObject so = new SerializedObject(flow);
        SetBool(so, "autoStart", true);
        SetObject(so, "humanRoot", human);
        SetObject(so, "humanCamera", humanCamera);
        SetObject(so, "humanMovement", human.GetComponent<FirstPersonMovement>());
        SetObject(so, "humanLook", humanCamera != null ? humanCamera.GetComponent<FirstPersonLook>() : null);
        SetObject(so, "humanInteraction", human.GetComponent<PlayerInteraction>());
        SetObject(so, "humanRigidbody", human.GetComponent<Rigidbody>());
        SetObjectArray(so, "auxiliaryInputBehaviours", hudInput != null ? new Behaviour[] { hudInput } : new Behaviour[0]);
        SetObject(so, "lobbyStartAnchor", lobbyReference);
        SetObject(so, "lobbyStartCameraAnchor", lobbyReferenceCamera != null ? lobbyReferenceCamera.transform : lobbyReference);
        SetObject(so, "elevatorAnchor", elevatorReference);
        SetObject(so, "elevatorCameraAnchor", elevatorReferenceCamera != null ? elevatorReferenceCamera.transform : elevatorReference);
        SetObject(so, "floor17ArrivalAnchor", floor17Anchor);
        SetObject(so, "floor17ArrivalCameraAnchor", floor17CameraAnchor);
        SetObject(so, "subtitlePlayer", subtitlePlayer);
        SetObject(so, "screenFader", ui.Fader);
        SetObject(so, "hudOverlay", ui.Overlay);
        SetObject(so, "locationProbe", locationProbe);
        SetObject(so, "assignmentTerminal", terminal);
        SetObjectArray(so, "optionalConversationZones", zones);
        SetObjectArray(so, "hudCanvasesHiddenDuringTerminal", FindLobbyTerminalHudCanvases(ui));
        SetObject(so, "openingBriefingDialogue", dialogues.OpeningBriefing);
        SetObject(so, "lilyVoiceMessageDialogue", dialogues.LilyMessage);
        SetObject(so, "openingCloseoutDialogue", dialogues.OpeningCloseout);
        SetObject(so, "assignmentLoadedDialogue", dialogues.AssignmentLoaded);
        SetObject(so, "elevatorDialogue", dialogues.Elevator);
        SetObject(so, "floor17ArrivalDialogue", dialogues.Floor17Arrival);
        SetFloat(so, "startupFadeSeconds", 0.35f);
        SetFloat(so, "transitionFadeOutSeconds", 0.5f);
        SetFloat(so, "transitionFadeInSeconds", 0.5f);
        SetFloat(so, "assignmentTerminalMinimumViewSeconds", 5f);
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Canvas[] FindLobbyTerminalHudCanvases(LobbyUiReferences ui)
    {
        List<Canvas> canvases = new List<Canvas>();

        HearthFirstPersonHudController firstPersonHud = UnityEngine.Object.FindObjectOfType<HearthFirstPersonHudController>(true);
        if (firstPersonHud != null)
        {
            canvases.AddRange(firstPersonHud.GetComponentsInChildren<Canvas>(true));
        }

        if (ui.Overlay != null)
        {
            Canvas lobbyOverlayCanvas = ui.Overlay.GetComponentInParent<Canvas>();
            if (lobbyOverlayCanvas != null)
            {
                canvases.Add(lobbyOverlayCanvas);
            }
        }

        return canvases
            .Where(canvas => canvas != null && canvas.renderMode != RenderMode.WorldSpace)
            .Distinct()
            .ToArray();
    }

    private static DialogueLibrary EnsureDialogueAssets()
    {
        DialogueLibrary library = new DialogueLibrary();
        library.OpeningBriefing = EnsureDialogue(
            "Lobby_OpeningBriefing",
            "Scene 1.1 opening. Player movement and view are locked.",
            new[]
            {
                L("Field Unit", "Good evening, Inspector. Field Companion Unit online. I'll be your partner tonight."),
                L("Mia", "All right."),
                L("Field Unit", "Tonight's assignment is on the seventeenth floor: three household companion units scheduled for inspection."),
                L("Field Unit", "This is a routine service review. You'll check how each unit has been operating in the home, identify any issues in its recent use, and decide whether its role in the household should be adjusted going forward."),
                L("Field Unit", "As one of the most highly regarded inspectors at the world's largest companion-unit company, I'm confident you'll complete tonight's route successfully."),
                L("Field Unit", "First, use the assignment terminal in this lobby to load the files for all three households. One additional detail: tonight's inspections are on the same floor as your own residence."),
                L("Field Unit", "Before you begin, take a moment to observe the lobby. You'll see companion units working throughout the community: our company's defining product, and the most successful companion technology in the world.")
            });
        library.LilyMessage = EnsureDialogue(
            "Lobby_LilyVoiceMessage",
            "Recorded voice message shown in the upper-right HUD card.",
            new[]
            {
                L("Lily", "Mom, are you getting home late tonight? I wanted to tell you something. We can talk when you get home. I'll wait for you... Don't forget, okay?")
            });
        library.OpeningCloseout = EnsureDialogue(
            "Lobby_OpeningCloseout",
            "Mia and the Field Unit respond to Lily's message before free exploration.",
            new[]
            {
                L("Mia", "Did she say what it was about?"),
                L("Field Unit", "No. That was the whole message. She wants to tell you in person. I recommend finishing the three inspections first, then handling the home item when you return."),
                L("Mia", "Okay.")
            });
        library.Girl = EnsureDialogue(
            "Lobby_Group01_Girl",
            "Optional proximity dialogue: lobby girl and public companion unit.",
            new[]
            {
                L("Lobby Girl", "Hi, everyone. I'm-"),
                L("Public Unit", "You know it. You just rushed the first part. Start with your name and try it again."),
                L("Lobby Girl", "Okay.")
            });
        library.GirlExit = EnsureDialogue(
            "Lobby_Group01_MiaExit",
            "Mia's commentary after leaving the lobby-girl trigger.",
            new[] { L("Mia", "Huh. Guess these things really do help with kids.") });
        library.YoungMan = EnsureDialogue(
            "Lobby_Group02_YoungMan",
            "Optional proximity dialogue: young man and work-assist unit.",
            new[]
            {
                L("Work Unit", "This section's solid. One small thing: in the second paragraph, 'in summary' sounds more formal than 'anyway.'"),
                L("Young Man", "Mm-hm."),
                L("Work Unit", "Want me to bring in last week's chart?"),
                L("Young Man", "Yeah, thanks."),
                L("Work Unit", "You got it. Also, you've been sitting since three. How about two minutes on your feet?"),
                L("Young Man", "In a minute.")
            });
        library.YoungManExit = EnsureDialogue(
            "Lobby_Group02_MiaExit",
            "Mia's commentary after leaving the young-man trigger.",
            new[] { L("Mia", "It's not just work. It handles all the little day-to-day stuff, too. I've been using mine that way for years.") });
        library.Grandmother = EnsureDialogue(
            "Lobby_Group03_Grandmother",
            "Optional proximity dialogue: Mrs. Ellis and care unit.",
            new[]
            {
                L("Mrs. Ellis", "How old is she now?"),
                L("Care Unit", "She's nine, Mrs. Ellis. She sent you a drawing yesterday."),
                L("Mrs. Ellis", "What did she draw?"),
                L("Care Unit", "The two of you, holding hands."),
                L("Mrs. Ellis", "Oh, that's sweet. Why didn't she show me?"),
                L("Care Unit", "She did, Mrs. Ellis. This is the third time you've asked. Would you like to see it again?"),
                L("Mrs. Ellis", "Yes, put it up.")
            });
        library.GrandmotherExit = EnsureDialogue(
            "Lobby_Group03_MiaExit",
            "Mia's commentary after leaving the Mrs. Ellis trigger.",
            new[] { L("Mia", "Maybe I should get one of these for my parents.") });
        library.AssignmentLoaded = EnsureDialogue(
            "Lobby_AssignmentLoaded",
            "Plays after the assignment terminal closes and unlocks the elevator button.",
            new[]
            {
                L("Field Unit", "Inspector, companion-request volume is currently high across Building A's public level. The children's area, work pods, and park-side assistance points are all connected to the synchronized network."),
                L("Mia", "I can see that."),
                L("Field Unit", "Public companion deployment in this community is one unit for every four residents, above the residential average. Building A is one of the company's priority demonstration sites."),
                L("Field Unit", "The benefit is fewer short gaps in care. Parents can continue what they're doing, and child users are less likely to be left waiting without a response."),
                L("Field Unit", "Route loaded. Proceed to the elevator and call it when you're ready. Destination: Floor Seventeen.")
            });
        library.Elevator = EnsureDialogue(
            "Lobby_ElevatorRide",
            "Scene 1.2 elevator briefing. Movement is locked while view rotation remains available.",
            new[]
            {
                L("Field Unit", "Inspector, a quick briefing before we reach seventeen. Procedure first, then tonight's route."),
                L("Mia", "Go ahead."),
                L("Field Unit", "Each household is reviewed through its companion unit's inspection terminal. Once you badge in, you'll see why the household purchased the unit, how it's being used, and its current Household Emotional Stability Index."),
                L("Field Unit", "From there, you can enter the unit's point of view and replay recent significant events. Your review is based on that playback."),
                L("Field Unit", "At the end, you'll choose a disposition. That determines how the unit is used in the household going forward and may affect the household's stability score."),
                L("Mia", "And you'll tell me which one you prefer."),
                L("Field Unit", "I'll recommend an option at every terminal. The recommendation comes directly from the inspection manual. Standard answers, Inspector. That's all I operate on."),
                L("Field Unit", "For context, companion-unit adoption in this district is ninety-four point seven percent. According to this year's white paper, households with a unit average eight point four out of ten on the stability index. Households without one average five point nine."),
                L("Field Unit", "The index is public data. Employers, insurers, schools, and community boards have authorized access. A low score may be interpreted as a household spending too much time and energy on conflict, and it can affect hiring, premiums, and school placement."),
                L("Field Unit", "That does not determine your decisions tonight. It is context only."),
                L("Mia", "Noted."),
                L("Field Unit", "Tonight's route: 17F-01, routine review. Daniel and Emily requested an upgrade to Night Companion Pro this morning. Review last night's event before signing off. 17F-02, Ben and Claire. Ben force-shut their unit at 6:47 this evening. Full playback required. 17F-03 is flagged. I'll brief you when we get there."),
                L("Mia", "A forced shutdown? That's unusual."),
                L("Field Unit", "Seven cases company-wide this month."),
                L("Field Unit", "I'll guide you at each apartment. Have a good shift, Inspector.")
            });
        library.Floor17Arrival = AssetDatabase.LoadAssetAtPath<HearthDialogueSequence>(
            "Assets/Data/MinLoop/Dialogues/FinalScriptSupplemental/17F01_CorridorArrival.asset");
        return library;
    }

    private static DialogueLine L(string speaker, string text)
    {
        return new DialogueLine(speaker, text);
    }

    private static HearthDialogueSequence EnsureDialogue(string id, string notes, DialogueLine[] lines)
    {
        string path = DialogueFolder + "/" + id + ".asset";
        HearthDialogueSequence asset = AssetDatabase.LoadAssetAtPath<HearthDialogueSequence>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<HearthDialogueSequence>();
            AssetDatabase.CreateAsset(asset, path);
        }

        SerializedObject so = new SerializedObject(asset);
        SetString(so, "sequenceId", id);
        SetString(so, "notes", notes + " Source: " + FinalScriptFileName + ".");
        SerializedProperty list = so.FindProperty("lines");
        list.arraySize = lines.Length;
        for (int i = 0; i < lines.Length; i++)
        {
            SerializedProperty entry = list.GetArrayElementAtIndex(i);
            SetRelativeFloat(entry, "startDelay", i == 0 ? 0.15f : 0.18f);
            SetRelativeString(entry, "speaker", lines[i].Speaker);
            SetRelativeString(entry, "text", lines[i].Text);
            SetRelativeFloat(entry, "holdSeconds", EstimateHoldSeconds(lines[i].Text));
            SetRelativeInt(entry, "durationMode", (int)HearthSubtitleDurationMode.VoiceClipWhenAssigned);
            SetRelativeFloat(entry, "voiceTailSeconds", 0.12f);
        }
        SetFloat(so, "postSequenceDelay", 0.18f);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static float EstimateHoldSeconds(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return 2.4f;
        int words = text.Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
        return Mathf.Clamp(1.15f + words / 2.65f, 2.2f, 10f);
    }

    private static void PrepareReferenceController(GameObject reference)
    {
        HearthEditorOnlyReferenceModel marker = GetOrAdd<HearthEditorOnlyReferenceModel>(reference);
        marker.ApplyReferenceState();
        foreach (Camera camera in reference.GetComponentsInChildren<Camera>(true)) camera.enabled = false;
        foreach (AudioListener listener in reference.GetComponentsInChildren<AudioListener>(true)) listener.enabled = false;
        EditorUtility.SetDirty(reference);
    }

    private static Transform EnsurePoseAnchor(Transform parent, string name, Transform source, bool overwrite)
    {
        Transform anchor = FindDirectChild(parent, name);
        if (anchor == null)
        {
            anchor = new GameObject(name).transform;
            anchor.SetParent(parent, true);
            overwrite = true;
        }

        if (overwrite && source != null)
        {
            anchor.SetPositionAndRotation(source.position, source.rotation);
        }

        return anchor;
    }

    private static Bounds TransformWorldBoundsToLocal(Transform target, Bounds worldBounds)
    {
        Vector3 min = worldBounds.min;
        Vector3 max = worldBounds.max;
        Vector3[] corners =
        {
            new Vector3(min.x, min.y, min.z), new Vector3(min.x, min.y, max.z),
            new Vector3(min.x, max.y, min.z), new Vector3(min.x, max.y, max.z),
            new Vector3(max.x, min.y, min.z), new Vector3(max.x, min.y, max.z),
            new Vector3(max.x, max.y, min.z), new Vector3(max.x, max.y, max.z)
        };
        Bounds local = new Bounds(target.InverseTransformPoint(corners[0]), Vector3.zero);
        for (int i = 1; i < corners.Length; i++) local.Encapsulate(target.InverseTransformPoint(corners[i]));
        return local;
    }

    private static void ValidateConversationZone(List<string> issues, string path)
    {
        Transform transform = FindTransform(path);
        if (transform == null) return;
        Collider collider = transform.GetComponent<Collider>();
        if (collider == null || !collider.enabled || !collider.isTrigger)
        {
            issues.Add(path + " must retain an enabled Trigger collider.");
        }
        if (transform.GetComponent<HearthLobbyConversationZone>() == null)
        {
            issues.Add(path + " has no HearthLobbyConversationZone.");
        }
    }

    private static void ClearPersistentListeners(UnityEvent unityEvent)
    {
        if (unityEvent == null) return;
        unityEvent.RemoveAllListeners();
        for (int i = unityEvent.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            UnityEventTools.RemovePersistentListener(unityEvent, i);
        }
    }

    private static CanvasGroup CreateGroup(Transform parent, string name, Rect rect)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasGroup));
        obj.transform.SetParent(parent, false);
        SetTopLeft(obj.GetComponent<RectTransform>(), rect);
        CanvasGroup group = obj.GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
        return group;
    }

    private static Image CreateImage(Transform parent, string name, Rect rect, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        obj.transform.SetParent(parent, false);
        SetTopLeft(obj.GetComponent<RectTransform>(), rect);
        Image image = obj.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static TMP_Text CreateText(Transform parent, string name, string value, Rect rect, float fontSize, Color color, TextAlignmentOptions alignment)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        SetTopLeft(obj.GetComponent<RectTransform>(), rect);
        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Truncate;
        text.raycastTarget = false;
        return text;
    }

    private static void EnsureRuntimePromptText(HearthTvTerminalController terminal)
    {
        if (terminal == null)
        {
            return;
        }

        Transform keyboard = terminal.transform.Find("KeyboardNavigationRoot");
        if (keyboard == null || keyboard.Find("RuntimePromptText") != null)
        {
            return;
        }

        TMP_Text prompt = CreateText(
            keyboard,
            "RuntimePromptText",
            string.Empty,
            new Rect(560f, 92f, 800f, 38f),
            19f,
            new Color(0.78f, 0.96f, 1f, 0.96f),
            TextAlignmentOptions.Center);
        prompt.fontStyle = FontStyles.Bold;
        prompt.gameObject.SetActive(false);
    }

    private static void ReplaceText(Transform root, string path, string value)
    {
        Transform target = root != null ? root.Find(path) : null;
        TMP_Text text = target != null ? target.GetComponent<TMP_Text>() : null;
        if (text == null)
        {
            return;
        }

        text.text = value;
        EditorUtility.SetDirty(text);
    }

    private static void AddBorder(Transform parent, Rect rect, Color color, float thickness)
    {
        CreateImage(parent, "BorderTop", new Rect(rect.x, rect.y, rect.width, thickness), color);
        CreateImage(parent, "BorderBottom", new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
        CreateImage(parent, "BorderLeft", new Rect(rect.x, rect.y, thickness, rect.height), color);
        CreateImage(parent, "BorderRight", new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
    }

    private static void SetTopLeft(RectTransform rect, Rect source)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(source.x, -source.y);
        rect.sizeDelta = new Vector2(source.width, source.height);
        rect.localScale = Vector3.one;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private static Transform EnsureHierarchy(string path)
    {
        string[] names = path.Split('/');
        Transform current = null;
        for (int i = 0; i < names.Length; i++)
        {
            Transform next = current == null ? FindRoot(names[i]) : FindDirectChild(current, names[i]);
            if (next == null)
            {
                GameObject obj = new GameObject(names[i]);
                if (current != null) obj.transform.SetParent(current, false);
                Undo.RegisterCreatedObjectUndo(obj, "Create " + names[i]);
                next = obj.transform;
            }
            current = next;
        }
        return current;
    }

    private static Transform EnsureChild(Transform parent, string name)
    {
        Transform child = FindDirectChild(parent, name);
        if (child != null) return child;
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        Undo.RegisterCreatedObjectUndo(obj, "Create " + name);
        return obj.transform;
    }

    private static Transform FindRoot(string name)
    {
        return SceneManager.GetActiveScene().GetRootGameObjects()
            .Select(item => item.transform)
            .FirstOrDefault(item => item.name == name);
    }

    private static Transform FindDirectChild(Transform parent, string name)
    {
        if (parent == null) return null;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == name) return child;
        }
        return null;
    }

    private static Transform FindTransform(string path)
    {
        string[] parts = path.Split('/');
        Transform current = FindRoot(parts[0]);
        for (int i = 1; i < parts.Length && current != null; i++) current = FindDirectChild(current, parts[i]);
        return current;
    }

    private static void DestroyDirectChild(Transform parent, string name)
    {
        Transform child = FindDirectChild(parent, name);
        if (child != null) Undo.DestroyObjectImmediate(child.gameObject);
    }

    private static void RequirePath(List<string> issues, string path)
    {
        if (FindTransform(path) == null) issues.Add("Missing scene object: " + path);
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int slash = path.LastIndexOf('/');
        string parent = path.Substring(0, slash);
        string name = path.Substring(slash + 1);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        return component != null ? component : Undo.AddComponent<T>(target);
    }

    private static void SetObject(SerializedObject so, string name, UnityEngine.Object value)
    {
        SerializedProperty property = so.FindProperty(name);
        if (property != null) property.objectReferenceValue = value;
    }

    private static void SetObjectArray(SerializedObject so, string name, UnityEngine.Object[] values)
    {
        SerializedProperty property = so.FindProperty(name);
        if (property == null) return;
        property.arraySize = values != null ? values.Length : 0;
        for (int i = 0; values != null && i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }

    private static void SetBool(SerializedObject so, string name, bool value)
    {
        SerializedProperty property = so.FindProperty(name);
        if (property != null) property.boolValue = value;
    }

    private static void SetInt(SerializedObject so, string name, int value)
    {
        SerializedProperty property = so.FindProperty(name);
        if (property != null) property.intValue = value;
    }

    private static void SetFloat(SerializedObject so, string name, float value)
    {
        SerializedProperty property = so.FindProperty(name);
        if (property != null) property.floatValue = value;
    }

    private static void SetString(SerializedObject so, string name, string value)
    {
        SerializedProperty property = so.FindProperty(name);
        if (property != null) property.stringValue = value;
    }

    private static void SetRelativeString(SerializedProperty parent, string name, string value)
    {
        SerializedProperty property = parent.FindPropertyRelative(name);
        if (property != null) property.stringValue = value;
    }

    private static void SetRelativeFloat(SerializedProperty parent, string name, float value)
    {
        SerializedProperty property = parent.FindPropertyRelative(name);
        if (property != null) property.floatValue = value;
    }

    private static void SetRelativeInt(SerializedProperty parent, string name, int value)
    {
        SerializedProperty property = parent.FindPropertyRelative(name);
        if (property != null) property.enumValueIndex = value;
    }

    private struct DialogueLine
    {
        public readonly string Speaker;
        public readonly string Text;

        public DialogueLine(string speaker, string text)
        {
            Speaker = speaker;
            Text = text;
        }
    }

    private class DialogueLibrary
    {
        public HearthDialogueSequence OpeningBriefing;
        public HearthDialogueSequence LilyMessage;
        public HearthDialogueSequence OpeningCloseout;
        public HearthDialogueSequence Girl;
        public HearthDialogueSequence GirlExit;
        public HearthDialogueSequence YoungMan;
        public HearthDialogueSequence YoungManExit;
        public HearthDialogueSequence Grandmother;
        public HearthDialogueSequence GrandmotherExit;
        public HearthDialogueSequence AssignmentLoaded;
        public HearthDialogueSequence Elevator;
        public HearthDialogueSequence Floor17Arrival;
    }

    private struct LobbyUiReferences
    {
        public HearthLobbyHudOverlay Overlay;
        public HearthScreenFader Fader;
    }
}
#endif
