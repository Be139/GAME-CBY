#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Hearth17F02MinimalLoopBinder
{
    private const string MenuPath = "Tools/Hearth/Replay/Apply 17F02 Minimal Loop Setup";
    private const string MigrateWifeExitRouteMenuPath = "Tools/Hearth/Replay/Migrate 17F02 Wife Exit Route To Simple Segments";
    private const string DialogueFolder = "Assets/Data/MinLoop/Dialogues";
    private const string BedroomWakeDialoguePath = DialogueFolder + "/17F02_BedroomWake.asset";
    private const string BedroomConfideDialoguePath = DialogueFolder + "/17F02_BedroomConfide.asset";
    private const string BedroomComfortDialoguePath = DialogueFolder + "/17F02_BedroomComfort.asset";
    private const string WifeExitDialoguePath = DialogueFolder + "/17F02_WifeExit.asset";
    private const string DiningDialoguePath = DialogueFolder + "/17F02_DiningObservation.asset";
    private const string LogAccessDialoguePath = DialogueFolder + "/17F02_LogAccess.asset";
    private const string ShutdownDialoguePath = DialogueFolder + "/17F02_ForcedShutdown.asset";
    private const string BlackAudioDialoguePath = DialogueFolder + "/17F02_BlackAudioArgument.asset";

    [MenuItem(MenuPath)]
    public static void Apply()
    {
        GameObject minLoopRoot = FindOrCreate("MIN_LOOP_ROOT");
        Transform anchorsRoot = FindOrCreateChild(minLoopRoot.transform, "Anchors");
        Transform replayRoot = FindOrCreateChild(minLoopRoot.transform, "ReplayRoom_17F02");

        GameObject person = Find("Player/Person Controller", "Person Controller", "Player_Mia_Controller");
        GameObject robot = Find("Player/Robot Controller", "Robot Controller", "Companion_Controller");
        GameObject robotBedroomReference = Find(
            "Player/Robot Controller (2)",
            "Player/Robot Controller2",
            "Player/Robot Controller 2",
            "Robot Controller (2)",
            "Robot Controller2",
            "Robot Controller 2",
            "robot Controller2",
            "Robot controller2");
        GameObject robotLivingReference = Find(
            "Player/Robot Controller (3)",
            "Player/Robot Controller3",
            "Player/Robot Controller 3",
            "Robot Controller (3)",
            "Robot Controller3",
            "Robot Controller 3",
            "robot Controller3",
            "Robot controller3");
        GameObject bedroomWife = Find("casual_Female_K (2)", "casual_Female_K2", "casual_Female_K 2");
        GameObject diningWife = Find("casual_Female_K", "casual_Female_K (2)", "casual_Female_K2");
        GameObject diningHusband = Find("casual_Male_K", "casual_Male_K (1)");
        GameObject terminalHusband = Find("casual_Male_K (1)", "casual_Male_K2", "casual_Male_K 2");
        GameObject bedroomDoorObject = Find("Door_2_Brown (4)", "Door_2_Brown");
        GameObject bedroomBed = Find("17F/ROOM3/Prop_Bed_09", "Prop_Bed_09");

        Transform bedroomAnchor = CreateAnchor(anchorsRoot, "Anchor_Robot_17F02_BedroomStart", robotBedroomReference != null ? robotBedroomReference.transform : robot != null ? robot.transform : null);
        Transform livingTerminalAnchor = CreateAnchor(anchorsRoot, "Anchor_Robot_17F02_LivingRoomTerminal", robotLivingReference != null ? robotLivingReference.transform : robot != null ? robot.transform : null);
        Transform wifePathAnchorA = CreateWifeExitPathAnchor(
            anchorsRoot,
            "Anchor_Wife_17F02_Path01",
            bedroomWife != null ? bedroomWife.transform : null,
            bedroomDoorObject != null ? bedroomDoorObject.transform : null,
            bedroomBed != null ? bedroomBed.transform : null,
            robotBedroomReference != null ? robotBedroomReference.transform : null,
            0.28f,
            1.15f);
        Transform wifePathAnchorB = CreateWifeExitPathAnchor(
            anchorsRoot,
            "Anchor_Wife_17F02_Path02",
            bedroomWife != null ? bedroomWife.transform : null,
            bedroomDoorObject != null ? bedroomDoorObject.transform : null,
            bedroomBed != null ? bedroomBed.transform : null,
            robotBedroomReference != null ? robotBedroomReference.transform : null,
            0.68f,
            0.75f);
        Transform[] wifeExitPathAnchors = CollectWifeExitPathAnchors(anchorsRoot, wifePathAnchorA, wifePathAnchorB);
        Transform wifeDoorPauseAnchor = CreateDoorSideAnchor(anchorsRoot, "Anchor_Wife_17F02_DoorPause", bedroomDoorObject != null ? bedroomDoorObject.transform : null, -0.65f);
        Transform wifeExitOutsideAnchor = CreateDoorSideAnchor(anchorsRoot, "Anchor_Wife_17F02_ExitOutside", bedroomDoorObject != null ? bedroomDoorObject.transform : null, 1.15f);
        SmartDoorController bedroomDoor = ConfigureDoorController(bedroomDoorObject);
        SplitWifeExitPathAnchors(wifeExitPathAnchors, out Transform[] wifeBeforeDoorPathAnchors, out Transform[] wifeAfterDoorPathAnchors);
        ConfigureWifeExitAnchorGizmos(wifeBeforeDoorPathAnchors, wifeDoorPauseAnchor, wifeAfterDoorPathAnchors, wifeExitOutsideAnchor);

        DisableReferenceController(robotBedroomReference);
        DisableReferenceController(robotLivingReference);

        ViewSwitchController viewSwitch = GetOrAdd<ViewSwitchController>(FindOrCreate("MIN_LOOP_ROOT/FlowManagers/ViewSwitchController"));
        ConfigureViewSwitch(viewSwitch, person, robot);

        MinLoopFlowController flow = GetOrAdd<MinLoopFlowController>(FindOrCreate("MIN_LOOP_ROOT/FlowManagers/MinLoopFlowController"));
        TrustStateController trust = GetOrAdd<TrustStateController>(FindOrCreate("MIN_LOOP_ROOT/FlowManagers/TrustStateController"));
        MinLoopSubtitlePlayer subtitlePlayer = Object.FindObjectOfType<MinLoopSubtitlePlayer>(true);
        HearthCompanionHudController hud = Object.FindObjectOfType<HearthCompanionHudController>(true);
        HearthCompanionHudPreviewInput previewInput = hud != null ? hud.GetComponent<HearthCompanionHudPreviewInput>() : null;
        HearthCompanionHudExclusiveMode exclusiveMode = hud != null ? hud.GetComponent<HearthCompanionHudExclusiveMode>() : null;
        HearthCompanionHudFlowBinder flowBinder = hud != null ? hud.GetComponent<HearthCompanionHudFlowBinder>() : null;
        HearthTvTerminalController terminal17F02 = FindTerminal17F02();

        HearthDialogueSequence bedroomWake = Ensure17F02BedroomWakeDialogue();
        HearthDialogueSequence bedroomConfide = Ensure17F02BedroomConfideDialogue();
        HearthDialogueSequence bedroomComfort = Ensure17F02BedroomComfortDialogue();
        HearthDialogueSequence wifeExit = Ensure17F02WifeExitDialogue();
        HearthDialogueSequence dining = Ensure17F02DiningDialogue();
        HearthDialogueSequence logAccess = Ensure17F02LogAccessDialogue();
        HearthDialogueSequence shutdown = Ensure17F02ShutdownDialogue();
        HearthDialogueSequence blackAudio = Ensure17F02BlackAudioDialogue();

        GameObject replayControllerObject = FindOrCreateChild(replayRoot, "HearthCompanion17F02ReplayController").gameObject;
        HearthCompanion17F02ReplayController replayController = GetOrAdd<HearthCompanion17F02ReplayController>(replayControllerObject);

        ConfigureReplayController(
            replayController,
            flow,
            viewSwitch,
            hud,
            subtitlePlayer,
            robot,
            bedroomAnchor,
            livingTerminalAnchor,
            bedroomWife,
            diningWife,
            diningHusband,
            terminalHusband,
            wifeExitPathAnchors,
            wifeBeforeDoorPathAnchors,
            wifeAfterDoorPathAnchors,
            wifeDoorPauseAnchor,
            wifeExitOutsideAnchor,
            bedroomDoor,
            bedroomWake,
            bedroomConfide,
            bedroomComfort,
            wifeExit,
            dining,
            logAccess,
            shutdown,
            blackAudio);

        ConfigureFlow(flow, terminal17F02, viewSwitch, replayController, trust);
        ConfigureTerminal(terminal17F02, flow, viewSwitch, person);
        ConfigureHud(hud, flowBinder, previewInput, exclusiveMode, flow, viewSwitch);

        EditorUtility.SetDirty(minLoopRoot);
        if (terminal17F02 != null)
        {
            EditorUtility.SetDirty(terminal17F02);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[Hearth17F02MinimalLoopBinder] 17F02 minimal loop setup applied.");
    }

    [MenuItem(MigrateWifeExitRouteMenuPath)]
    public static void MigrateWifeExitRouteToSimpleSegments()
    {
        HearthCompanion17F02ReplayController replayController = Object.FindObjectOfType<HearthCompanion17F02ReplayController>(true);
        if (replayController == null)
        {
            Debug.LogWarning("[Hearth17F02MinimalLoopBinder] 17F02 replay controller was not found. Run the full 17F02 setup first.");
            return;
        }

        Transform[] wifeExitPathAnchors = CollectExistingWifeExitPathAnchors();
        SplitWifeExitPathAnchors(wifeExitPathAnchors, out Transform[] wifeBeforeDoorPathAnchors, out Transform[] wifeAfterDoorPathAnchors);
        Transform wifeDoorPauseAnchor = FindTransform("Anchor_Wife_17F02_DoorPause");
        Transform wifeExitOutsideAnchor = FindTransform("Anchor_Wife_17F02_ExitOutside");

        SerializedObject so = new SerializedObject(replayController);
        SetBool(so, "useSimpleWifeExitRoute", true);
        SetArray(so, "wifeBeforeDoorPathPoints", CollectObjects(wifeBeforeDoorPathAnchors));
        SetBool(so, "moveToDoorPauseBeforeOpening", true);
        SetArray(so, "wifeAfterDoorPathPoints", CollectObjects(wifeAfterDoorPathAnchors));
        SetArray(so, "wifeExitPathPoints", CollectObjects(wifeExitPathAnchors));
        SetInt(so, "openDoorAfterPathPointCount", -1);
        if (wifeDoorPauseAnchor != null)
        {
            SetObject(so, "wifeDoorPauseAnchor", wifeDoorPauseAnchor);
        }

        if (wifeExitOutsideAnchor != null)
        {
            SetObject(so, "wifeExitOutsideAnchor", wifeExitOutsideAnchor);
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(replayController);

        ConfigureWifeExitAnchorGizmos(wifeBeforeDoorPathAnchors, wifeDoorPauseAnchor, wifeAfterDoorPathAnchors, wifeExitOutsideAnchor);

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log("[Hearth17F02MinimalLoopBinder] 17F02 wife exit route migrated to simple segments. BeforeDoor="
            + wifeBeforeDoorPathAnchors.Length + ", AfterDoor=" + wifeAfterDoorPathAnchors.Length + ".");
    }

    private static void ConfigureViewSwitch(ViewSwitchController viewSwitch, GameObject person, GameObject robot)
    {
        if (viewSwitch == null)
        {
            return;
        }

        SerializedObject so = new SerializedObject(viewSwitch);
        SetObject(so, "human.rootObject", person);
        SetObject(so, "human.viewCamera", person != null ? person.GetComponentInChildren<Camera>(true) : null);
        SetObject(so, "human.movement", person != null ? person.GetComponent<FirstPersonMovement>() : null);
        SetObject(so, "human.look", person != null ? person.GetComponentInChildren<FirstPersonLook>(true) : null);
        SetObject(so, "human.interaction", person != null ? person.GetComponent<PlayerInteraction>() : null);
        SetObject(so, "human.rigidbody", person != null ? person.GetComponent<Rigidbody>() : null);
        SetObject(so, "companion.rootObject", robot);
        SetObject(so, "companion.viewCamera", robot != null ? robot.GetComponentInChildren<Camera>(true) : null);
        SetObject(so, "companion.movement", robot != null ? robot.GetComponent<FirstPersonMovement>() : null);
        SetObject(so, "companion.look", robot != null ? robot.GetComponentInChildren<FirstPersonLook>(true) : null);
        SetObject(so, "companion.interaction", robot != null ? robot.GetComponent<PlayerInteraction>() : null);
        SetObject(so, "companion.rigidbody", robot != null ? robot.GetComponent<Rigidbody>() : null);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(viewSwitch);
    }

    private static void ConfigureReplayController(
        HearthCompanion17F02ReplayController replayController,
        MinLoopFlowController flow,
        ViewSwitchController viewSwitch,
        HearthCompanionHudController hud,
        MinLoopSubtitlePlayer subtitlePlayer,
        GameObject robot,
        Transform bedroomAnchor,
        Transform livingTerminalAnchor,
        GameObject bedroomWife,
        GameObject diningWife,
        GameObject diningHusband,
        GameObject terminalHusband,
        Transform[] wifeExitPathAnchors,
        Transform[] wifeBeforeDoorPathAnchors,
        Transform[] wifeAfterDoorPathAnchors,
        Transform wifeDoorPauseAnchor,
        Transform wifeExitOutsideAnchor,
        SmartDoorController bedroomDoor,
        HearthDialogueSequence bedroomWake,
        HearthDialogueSequence bedroomConfide,
        HearthDialogueSequence bedroomComfort,
        HearthDialogueSequence wifeExit,
        HearthDialogueSequence dining,
        HearthDialogueSequence logAccess,
        HearthDialogueSequence shutdown,
        HearthDialogueSequence blackAudio)
    {
        if (replayController == null)
        {
            return;
        }

        SerializedObject so = new SerializedObject(replayController);
        SetObject(so, "flowController", flow);
        SetObject(so, "viewSwitchController", viewSwitch);
        SetObject(so, "companionHud", hud);
        SetObject(so, "subtitlePlayer", subtitlePlayer);
        SetObject(so, "robotRoot", robot != null ? robot.transform : null);
        SetObject(so, "robotCamera", robot != null ? robot.GetComponentInChildren<Camera>(true) : null);
        SetObject(so, "robotMovement", robot != null ? robot.GetComponent<FirstPersonMovement>() : null);
        SetObject(so, "robotLook", robot != null ? robot.GetComponentInChildren<FirstPersonLook>(true) : null);
        SetObject(so, "robotInteraction", robot != null ? robot.GetComponent<PlayerInteraction>() : null);
        SetObject(so, "robotRigidbody", robot != null ? robot.GetComponent<Rigidbody>() : null);
        SetObject(so, "bedroomStartAnchor", bedroomAnchor);
        SetObject(so, "livingRoomTerminalAnchor", livingTerminalAnchor);
        SetObject(so, "bedroomWakeSequence", bedroomWake);
        SetObject(so, "bedroomConfideSequence", bedroomConfide);
        SetObject(so, "bedroomComfortSequence", bedroomComfort);
        SetObject(so, "wifeExitSequence", wifeExit);
        SetObject(so, "diningObservationSequence", dining);
        SetObject(so, "logAccessSequence", logAccess);
        SetObject(so, "forcedShutdownSequence", shutdown);
        SetObject(so, "blackAudioSequence", blackAudio);
        SetObject(so, "bedroomWifeActor", bedroomWife);
        SetObject(so, "diningWifeActor", diningWife);
        SetObject(so, "diningHusbandActor", diningHusband);
        SetObject(so, "terminalHusbandActor", terminalHusband);
        SetObject(so, "bedroomWifeMoveRoot", bedroomWife != null ? bedroomWife.transform : null);
        SetBool(so, "useSimpleWifeExitRoute", true);
        SetArray(so, "wifeBeforeDoorPathPoints", CollectObjects(wifeBeforeDoorPathAnchors));
        SetArray(so, "wifeAfterDoorPathPoints", CollectObjects(wifeAfterDoorPathAnchors));
        SetBool(so, "moveToDoorPauseBeforeOpening", true);
        SetArray(so, "wifeExitPathPoints", CollectObjects(wifeExitPathAnchors));
        SetObject(so, "wifeDoorPauseAnchor", wifeDoorPauseAnchor);
        SetObject(so, "wifeExitOutsideAnchor", wifeExitOutsideAnchor);
        SetObject(so, "wifeExitDoor", bedroomDoor);
        SetBool(so, "moveBedroomWifeToDoor", true);
        SetBool(so, "openDoorDuringWifeExit", true);
        SetInt(so, "openDoorAfterPathPointCount", -1);
        SetBool(so, "keepDoorOpenAfterWifeExit", true);
        SetBool(so, "hideBedroomWifeAfterExit", false);
        SetFloat(so, "wifeWalkSpeed", 1.15f);
        SetFloat(so, "wifeDoorPauseSeconds", 0.45f);
        SetFloat(so, "waitAfterDoorOpenSeconds", 0.55f);
        SetBool(so, "manageActorVisibility", true);
        SetBool(so, "showBedroomHoldPromptDuringConfide", false);
        SetBool(so, "waitForBedroomAcknowledgement", true);
        SetFloat(so, "bedroomPromptDelayAfterConfideSeconds", 1.5f);
        SetBool(so, "waitForShutdownConfirmation", false);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(replayController);
    }

    private static void ConfigureFlow(
        MinLoopFlowController flow,
        HearthTvTerminalController terminal17F02,
        ViewSwitchController viewSwitch,
        HearthCompanion17F02ReplayController replayController,
        TrustStateController trust)
    {
        if (flow == null)
        {
            return;
        }

        SerializedObject so = new SerializedObject(flow);
        SetObject(so, "tvTerminalController", terminal17F02);
        SetObject(so, "viewSwitchController", viewSwitch);
        SetObject(so, "companion17F02ReplayController", replayController);
        SetObject(so, "trustStateController", trust);
        SetBool(so, "useResidentSpecificReplayControllers", true);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(flow);
    }

    private static void ConfigureTerminal(
        HearthTvTerminalController terminal,
        MinLoopFlowController flow,
        ViewSwitchController viewSwitch,
        GameObject person)
    {
        if (terminal == null)
        {
            Debug.LogWarning("[Hearth17F02MinimalLoopBinder] No 17F02 HearthTvTerminalController was found. Bind it manually if the terminal has a custom name.");
            return;
        }

        SerializedObject so = new SerializedObject(terminal);
        SetString(so, "replayResidentId", "17F02");
        SetObject(so, "minLoopFlowController", flow);
        SetObject(so, "viewSwitchController", viewSwitch);
        SetObject(so, "playerCamera", person != null ? person.GetComponentInChildren<Camera>(true) : null);
        SetBool(so, "routeChoicesToMinLoop", true);
        SetBool(so, "closeTerminalWhenChoiceSubmitted", true);
        SetBool(so, "preventRepeatedChoiceSubmission", true);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(terminal);
    }

    private static void ConfigureHud(
        HearthCompanionHudController hud,
        HearthCompanionHudFlowBinder flowBinder,
        HearthCompanionHudPreviewInput previewInput,
        HearthCompanionHudExclusiveMode exclusiveMode,
        MinLoopFlowController flow,
        ViewSwitchController viewSwitch)
    {
        if (hud != null)
        {
            SerializedObject so = new SerializedObject(hud);
            SetBool(so, "showStartingSceneOnStart", false);
            SetBool(so, "autoAdvanceOnHoldPrompt", false);
            SetObject(so, "viewSwitchController", viewSwitch);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(hud);
        }

        if (flowBinder != null)
        {
            SerializedObject so = new SerializedObject(flowBinder);
            SetObject(so, "companionHud", hud);
            SetObject(so, "minLoopFlowController", flow);
            SetObject(so, "viewSwitchController", viewSwitch);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(flowBinder);
        }

        if (previewInput != null)
        {
            SerializedObject so = new SerializedObject(previewInput);
            SetBool(so, "previewInputEnabled", false);
            SetObject(so, "controller", hud);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(previewInput);
        }

        if (exclusiveMode != null)
        {
            SerializedObject so = new SerializedObject(exclusiveMode);
            SetObject(so, "viewSwitchController", viewSwitch);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(exclusiveMode);
        }
    }

    private static HearthTvTerminalController FindTerminal17F02()
    {
        HearthTvTerminalController[] terminals = Object.FindObjectsOfType<HearthTvTerminalController>(true);
        for (int i = 0; i < terminals.Length; i++)
        {
            HearthTvTerminalController terminal = terminals[i];
            if (terminal != null && terminal.GetReplayResidentId() == "17F02")
            {
                return terminal;
            }
        }

        return null;
    }

    private static Transform CreateWifeExitPathAnchor(Transform parent, string name, Transform wife, Transform door, Transform bed, Transform sideGuide, float pathFraction, float sideOffset)
    {
        Transform anchor = FindOrCreateChild(parent, name);
        if (wife != null && door != null)
        {
            Vector3 position = Vector3.Lerp(wife.position, door.position, Mathf.Clamp01(pathFraction));
            position.y = wife.position.y;

            if (bed != null && sideGuide != null)
            {
                Vector3 sideDirection = ProjectFlat(sideGuide.position - bed.position, wife.forward);
                position += sideDirection * sideOffset;
            }

            Quaternion rotation = Quaternion.LookRotation(ProjectFlat(door.position - position, wife.forward), Vector3.up);
            anchor.SetPositionAndRotation(position, rotation);
        }
        else if (wife != null)
        {
            anchor.SetPositionAndRotation(wife.position, wife.rotation);
        }

        anchor.localScale = Vector3.one;
        EditorUtility.SetDirty(anchor);
        return anchor;
    }

    private static Transform[] CollectWifeExitPathAnchors(Transform anchorsRoot, Transform path01, Transform fallbackPath02)
    {
        List<Transform> anchors = new List<Transform>();
        AddIfNotNull(anchors, path01);

        bool foundFineTuneAnchors = false;
        for (int i = 1; i <= 5; i++)
        {
            Transform fineTuneAnchor = anchorsRoot != null ? anchorsRoot.Find("Anchor_Wife_17F02_Path01 (" + i + ")") : null;
            if (fineTuneAnchor == null)
            {
                continue;
            }

            anchors.Add(fineTuneAnchor);
            foundFineTuneAnchors = true;
        }

        if (!foundFineTuneAnchors)
        {
            AddIfNotNull(anchors, fallbackPath02);
        }

        return anchors.ToArray();
    }

    private static Transform[] CollectExistingWifeExitPathAnchors()
    {
        List<Transform> anchors = new List<Transform>();
        AddIfNotNull(anchors, FindTransform("Anchor_Wife_17F02_Path01"));

        bool foundFineTuneAnchors = false;
        for (int i = 1; i <= 5; i++)
        {
            Transform fineTuneAnchor = FindTransform("Anchor_Wife_17F02_Path01 (" + i + ")");
            if (fineTuneAnchor == null)
            {
                continue;
            }

            anchors.Add(fineTuneAnchor);
            foundFineTuneAnchors = true;
        }

        if (!foundFineTuneAnchors)
        {
            AddIfNotNull(anchors, FindTransform("Anchor_Wife_17F02_Path02"));
        }

        return anchors.ToArray();
    }

    private static void SplitWifeExitPathAnchors(Transform[] source, out Transform[] beforeDoor, out Transform[] afterDoor)
    {
        List<Transform> before = new List<Transform>();
        List<Transform> after = new List<Transform>();

        if (source != null)
        {
            int beforeCount = source.Length >= 6 ? 4 : Mathf.Max(0, source.Length - 1);
            for (int i = 0; i < source.Length; i++)
            {
                if (source[i] == null)
                {
                    continue;
                }

                if (i < beforeCount)
                {
                    before.Add(source[i]);
                }
                else
                {
                    after.Add(source[i]);
                }
            }
        }

        beforeDoor = before.ToArray();
        afterDoor = after.ToArray();
    }

    private static void ConfigureWifeExitAnchorGizmos(Transform[] beforeDoor, Transform doorPause, Transform[] afterDoor, Transform exitOutside)
    {
        Color beforeColor = new Color(0.2f, 0.85f, 1f, 0.85f);
        Color doorColor = new Color(1f, 0.75f, 0.2f, 0.9f);
        Color afterColor = new Color(0.35f, 1f, 0.45f, 0.85f);
        Color forwardColor = new Color(1f, 0.92f, 0.25f, 0.95f);

        if (beforeDoor != null)
        {
            for (int i = 0; i < beforeDoor.Length; i++)
            {
                ConfigureRouteAnchorGizmo(beforeDoor[i], "Before Door " + (i + 1), beforeColor, forwardColor);
            }
        }

        ConfigureRouteAnchorGizmo(doorPause, "Door Pause - open", doorColor, forwardColor);

        if (afterDoor != null)
        {
            for (int i = 0; i < afterDoor.Length; i++)
            {
                ConfigureRouteAnchorGizmo(afterDoor[i], "After Door " + (i + 1), afterColor, forwardColor);
            }
        }

        ConfigureRouteAnchorGizmo(exitOutside, "Exit Outside", afterColor, forwardColor);
    }

    private static void ConfigureRouteAnchorGizmo(Transform anchor, string label, Color bodyColor, Color forwardColor)
    {
        if (anchor == null)
        {
            return;
        }

        HearthRouteAnchorGizmo gizmo = GetOrAdd<HearthRouteAnchorGizmo>(anchor.gameObject);
        gizmo.Configure(label, bodyColor, forwardColor);
        EditorUtility.SetDirty(gizmo);
    }

    private static void AddIfNotNull(List<Transform> anchors, Transform anchor)
    {
        if (anchor != null && !anchors.Contains(anchor))
        {
            anchors.Add(anchor);
        }
    }

    private static Transform CreateDoorSideAnchor(Transform parent, string name, Transform door, float forwardOffset)
    {
        Transform anchor = FindOrCreateChild(parent, name);
        if (door != null)
        {
            Vector3 position = door.position + door.forward * forwardOffset;
            position.y = door.position.y;
            Vector3 lookDirection = ProjectFlat(door.position - position, door.forward);
            Quaternion rotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            anchor.SetPositionAndRotation(position, rotation);
        }

        anchor.localScale = Vector3.one;
        EditorUtility.SetDirty(anchor);
        return anchor;
    }

    private static SmartDoorController ConfigureDoorController(GameObject doorObject)
    {
        if (doorObject == null)
        {
            Debug.LogWarning("[Hearth17F02MinimalLoopBinder] Door_2_Brown (4) was not found. Assign Wife Exit Door manually on HearthCompanion17F02ReplayController.");
            return null;
        }

        SmartDoorController door = GetOrAdd<SmartDoorController>(doorObject);
        Transform movingRoot = FindChildRecursive(doorObject.transform, "Door");
        if (movingRoot == null)
        {
            movingRoot = doorObject.transform;
        }

        AudioSource source = movingRoot.GetComponent<AudioSource>();
        if (source == null)
        {
            source = doorObject.GetComponentInChildren<AudioSource>(true);
        }

        SerializedObject so = new SerializedObject(door);
        SetObject(so, "movingRoot", movingRoot);
        SetObject(so, "audioSource", source);
        SetBool(so, "captureClosedStateOnAwake", true);
        SetBool(so, "canToggle", true);
        SetBool(so, "locked", false);
        SetBool(so, "autoClose", false);
        SetFloat(so, "moveDuration", 0.55f);
        SetEnum(so, "motionMode", (int)SmartDoorController.DoorMotionMode.Rotate);
        SetVector3(so, "openLocalEulerOffset", new Vector3(0f, 90f, 0f));
        SetVector3(so, "openLocalPositionOffset", Vector3.zero);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(door);
        return door;
    }

    private static Transform FindChildRecursive(Transform root, string childName)
    {
        if (root == null || string.IsNullOrEmpty(childName))
        {
            return null;
        }

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == childName)
            {
                return child;
            }
        }

        return null;
    }

    private static Vector3 ProjectFlat(Vector3 vector, Vector3 fallback)
    {
        vector.y = 0f;
        if (vector.sqrMagnitude < 0.0001f)
        {
            vector = fallback;
            vector.y = 0f;
        }

        if (vector.sqrMagnitude < 0.0001f)
        {
            vector = Vector3.forward;
        }

        return vector.normalized;
    }

    private static HearthDialogueSequence Ensure17F02BedroomWakeDialogue()
    {
        return EnsureDialogueSequence(
            BedroomWakeDialoguePath,
            "17F02_BedroomWake",
            "Black-screen/offline opening. The couple speaks outside, the wife enters the bedroom, then wakes the companion unit.",
            new DefaultSubtitleLine("Husband", "You are back late.", 0.2f, 2.0f),
            new DefaultSubtitleLine("Wife", "The train stalled again. I just need a minute.", 0.2f, 3.0f),
            new DefaultSubtitleLine("Husband", "I still have calls. Dinner is almost ready.", 0.3f, 3.0f),
            new DefaultSubtitleLine("Wife", "I know. I am going to the room first.", 0.2f, 2.6f),
            new DefaultSubtitleLine("SFX", "[bedroom door opens]", 0.5f, 1.1f),
            new DefaultSubtitleLine("SFX", "[door closes]", 0.2f, 1.1f),
            new DefaultSubtitleLine("SFX", "[fabric shifts as she sits on the bed]", 0.3f, 1.7f),
            new DefaultSubtitleLine("Wife", "Hello? Are you there?", 0.3f, 2.0f),
            new DefaultSubtitleLine("Companion Unit", "Companion unit online.", 0.4f, 1.8f));
    }

    private static HearthDialogueSequence Ensure17F02BedroomConfideDialogue()
    {
        return EnsureDialogueSequence(
            BedroomConfideDialoguePath,
            "17F02_BedroomConfide",
            "Bedroom confide sequence. The robot can move in a limited room area while this plays.",
            new DefaultSubtitleLine("Wife", "I do not know why I keep telling you first.", 0.4f, 3.0f),
            new DefaultSubtitleLine("Wife", "It is easier than saying it at the table.", 0.2f, 3.0f),
            new DefaultSubtitleLine("Companion Unit", "I am listening.", 0.4f, 1.8f),
            new DefaultSubtitleLine("Wife", "Today was exhausting. I just need a minute before I go out there.", 0.2f, 4.0f));
    }

    private static HearthDialogueSequence Ensure17F02BedroomComfortDialogue()
    {
        return EnsureDialogueSequence(
            BedroomComfortDialoguePath,
            "17F02_BedroomComfort",
            "Companion response after the player confirms the bedroom comfort interaction.",
            new DefaultSubtitleLine("Companion Unit", "I am here. You are not alone in this room.", 0.1f, 2.8f),
            new DefaultSubtitleLine("Companion Unit", "You do not have to solve the whole night right now. Start with one breath.", 0.2f, 4.2f),
            new DefaultSubtitleLine("Wife", "...That helps.", 0.4f, 1.8f),
            new DefaultSubtitleLine("Wife", "I can go back out there.", 0.3f, 2.2f));
    }

    private static HearthDialogueSequence Ensure17F02WifeExitDialogue()
    {
        return EnsureDialogueSequence(
            WifeExitDialoguePath,
            "17F02_WifeExit",
            "Husband calls from the dining area. Wife answers him and leaves without addressing the companion unit.",
            new DefaultSubtitleLine("Husband", "Dinner is ready. Are you coming?", 0.4f, 2.6f),
            new DefaultSubtitleLine("Wife", "Coming.", 0.2f, 1.5f),
            new DefaultSubtitleLine("SFX", "[she stands and walks to the door]", 0.3f, 1.8f));
    }

    private static HearthDialogueSequence Ensure17F02DiningDialogue()
    {
        return EnsureDialogueSequence(
            DiningDialoguePath,
            "17F02_DiningObservation",
            "Dining observation after the wife leaves the bedroom. The robot can move and listen.",
            new DefaultSubtitleLine("Husband", "You are quiet tonight.", 0.4f, 2.4f),
            new DefaultSubtitleLine("Wife", "Just tired.", 0.3f, 1.8f),
            new DefaultSubtitleLine("Husband", "Work again?", 0.5f, 1.8f),
            new DefaultSubtitleLine("Wife", "It is fine. Let us eat.", 0.3f, 2.4f),
            new DefaultSubtitleLine("Husband", "You always say that after talking to it.", 0.8f, 3.0f));
    }

    private static HearthDialogueSequence Ensure17F02LogAccessDialogue()
    {
        return EnsureDialogueSequence(
            LogAccessDialoguePath,
            "17F02_LogAccess",
            "Living-room terminal/log access scene. The robot viewpoint is fixed.",
            new DefaultSubtitleLine("Wife", "I am going to shower.", 0.2f, 2.2f),
            new DefaultSubtitleLine("Husband", "Show me today's companion log.", 1.0f, 2.6f),
            new DefaultSubtitleLine("Companion Unit", "Authorized resident. Log access granted.", 0.3f, 2.8f));
    }

    private static HearthDialogueSequence Ensure17F02ShutdownDialogue()
    {
        return EnsureDialogueSequence(
            ShutdownDialoguePath,
            "17F02_ForcedShutdown",
            "Husband reacts and forces the companion unit offline.",
            new DefaultSubtitleLine("Husband", "So this is what she tells you.", 0.2f, 3.0f),
            new DefaultSubtitleLine("Companion Unit", "Soft guidance protocol prepared.", 0.4f, 2.6f),
            new DefaultSubtitleLine("Husband", "Enough.", 0.2f, 1.5f));
    }

    private static HearthDialogueSequence Ensure17F02BlackAudioDialogue()
    {
        return EnsureDialogueSequence(
            BlackAudioDialoguePath,
            "17F02_BlackAudioArgument",
            "Black-screen audio-only argument after forced shutdown.",
            new DefaultSubtitleLine("Wife", "Why did you turn it off?", 0.8f, 2.4f),
            new DefaultSubtitleLine("Husband", "Because apparently it knows more about you than I do.", 0.3f, 3.4f),
            new DefaultSubtitleLine("Wife", "That is not what this is.", 0.4f, 2.4f));
    }

    private static HearthDialogueSequence EnsureDialogueSequence(string path, string sequenceId, string notes, params DefaultSubtitleLine[] defaults)
    {
        EnsureAssetFolder(DialogueFolder);

        HearthDialogueSequence sequence = AssetDatabase.LoadAssetAtPath<HearthDialogueSequence>(path);
        if (sequence == null)
        {
            sequence = ScriptableObject.CreateInstance<HearthDialogueSequence>();
            AssetDatabase.CreateAsset(sequence, path);
        }

        SerializedObject so = new SerializedObject(sequence);
        SetString(so, "sequenceId", sequenceId);
        SetString(so, "notes", notes);

        SerializedProperty lines = so.FindProperty("lines");
        if (lines != null && lines.arraySize == 0 && defaults != null)
        {
            lines.arraySize = defaults.Length;
            for (int i = 0; i < defaults.Length; i++)
            {
                SerializedProperty line = lines.GetArrayElementAtIndex(i);
                line.FindPropertyRelative("speaker").stringValue = defaults[i].speaker;
                line.FindPropertyRelative("text").stringValue = defaults[i].text;
                line.FindPropertyRelative("startDelay").floatValue = defaults[i].startDelay;
                line.FindPropertyRelative("holdSeconds").floatValue = defaults[i].holdSeconds;
                line.FindPropertyRelative("voiceClip").objectReferenceValue = null;
            }
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(sequence);
        return sequence;
    }

    private static void EnsureAssetFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder))
        {
            return;
        }

        string parent = System.IO.Path.GetDirectoryName(folder).Replace("\\", "/");
        if (!AssetDatabase.IsValidFolder(parent))
        {
            EnsureAssetFolder(parent);
        }

        AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(folder));
    }

    private static Transform CreateAnchor(Transform parent, string name, Transform source)
    {
        Transform anchor = FindOrCreateChild(parent, name);
        if (source != null)
        {
            anchor.SetPositionAndRotation(source.position, source.rotation);
            anchor.localScale = Vector3.one;
        }

        EditorUtility.SetDirty(anchor);
        return anchor;
    }

    private static void DisableReferenceController(GameObject reference)
    {
        if (reference == null)
        {
            return;
        }

        foreach (Camera camera in reference.GetComponentsInChildren<Camera>(true))
        {
            camera.enabled = false;
            EditorUtility.SetDirty(camera);
        }

        foreach (AudioListener listener in reference.GetComponentsInChildren<AudioListener>(true))
        {
            listener.enabled = false;
            EditorUtility.SetDirty(listener);
        }

        foreach (FirstPersonMovement movement in reference.GetComponentsInChildren<FirstPersonMovement>(true))
        {
            movement.enabled = false;
            EditorUtility.SetDirty(movement);
        }

        foreach (FirstPersonLook look in reference.GetComponentsInChildren<FirstPersonLook>(true))
        {
            look.enabled = false;
            EditorUtility.SetDirty(look);
        }

        foreach (PlayerInteraction interaction in reference.GetComponentsInChildren<PlayerInteraction>(true))
        {
            interaction.enabled = false;
            EditorUtility.SetDirty(interaction);
        }

        Rigidbody body = reference.GetComponent<Rigidbody>();
        if (body != null)
        {
            if (!body.isKinematic)
            {
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }

            body.isKinematic = true;
            EditorUtility.SetDirty(body);
        }

        reference.SetActive(false);
        EditorUtility.SetDirty(reference);
    }

    private static GameObject FindOrCreate(string path)
    {
        GameObject existing = GameObject.Find(path);
        if (existing != null)
        {
            return existing;
        }

        string[] parts = path.Split('/');
        Transform parent = null;
        GameObject current = null;
        for (int i = 0; i < parts.Length; i++)
        {
            string currentPath = string.Join("/", parts, 0, i + 1);
            current = GameObject.Find(currentPath);
            if (current == null)
            {
                current = new GameObject(parts[i]);
                if (parent != null)
                {
                    current.transform.SetParent(parent, false);
                }
            }

            parent = current.transform;
        }

        return current;
    }

    private static Transform FindOrCreateChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
        {
            return child;
        }

        GameObject childObject = new GameObject(name);
        childObject.transform.SetParent(parent, false);
        return childObject.transform;
    }

    private static GameObject Find(params string[] names)
    {
        if (names == null)
        {
            return null;
        }

        for (int i = 0; i < names.Length; i++)
        {
            if (string.IsNullOrEmpty(names[i]))
            {
                continue;
            }

            GameObject exact = GameObject.Find(names[i]);
            if (exact != null)
            {
                return exact;
            }
        }

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < names.Length; i++)
        {
            string wanted = names[i];
            if (string.IsNullOrEmpty(wanted))
            {
                continue;
            }

            for (int j = 0; j < allObjects.Length; j++)
            {
                GameObject candidate = allObjects[j];
                if (candidate != null && candidate.scene.IsValid() && candidate.name == wanted)
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private static Transform FindTransform(params string[] names)
    {
        GameObject found = Find(names);
        return found != null ? found.transform : null;
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        if (component == null)
        {
            component = target.AddComponent<T>();
        }

        return component;
    }

    private static void SetObject(SerializedObject so, string propertyPath, Object value)
    {
        SerializedProperty property = so.FindProperty(propertyPath);
        if (property != null)
        {
            property.objectReferenceValue = value;
        }
    }

    private static void SetBool(SerializedObject so, string propertyPath, bool value)
    {
        SerializedProperty property = so.FindProperty(propertyPath);
        if (property != null)
        {
            property.boolValue = value;
        }
    }

    private static void SetString(SerializedObject so, string propertyPath, string value)
    {
        SerializedProperty property = so.FindProperty(propertyPath);
        if (property != null)
        {
            property.stringValue = value;
        }
    }

    private static void SetFloat(SerializedObject so, string propertyPath, float value)
    {
        SerializedProperty property = so.FindProperty(propertyPath);
        if (property != null)
        {
            property.floatValue = value;
        }
    }

    private static void SetInt(SerializedObject so, string propertyPath, int value)
    {
        SerializedProperty property = so.FindProperty(propertyPath);
        if (property != null)
        {
            property.intValue = value;
        }
    }

    private static void SetVector3(SerializedObject so, string propertyPath, Vector3 value)
    {
        SerializedProperty property = so.FindProperty(propertyPath);
        if (property != null)
        {
            property.vector3Value = value;
        }
    }

    private static void SetEnum(SerializedObject so, string propertyPath, int value)
    {
        SerializedProperty property = so.FindProperty(propertyPath);
        if (property != null)
        {
            property.enumValueIndex = value;
        }
    }

    private static void SetArray(SerializedObject so, string propertyPath, Object[] values)
    {
        SerializedProperty property = so.FindProperty(propertyPath);
        if (property == null)
        {
            return;
        }

        property.arraySize = values != null ? values.Length : 0;
        for (int i = 0; i < property.arraySize; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }

    private static Object[] CollectObjects(params Object[] values)
    {
        if (values == null)
        {
            return new Object[0];
        }

        int count = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] != null)
            {
                count++;
            }
        }

        Object[] result = new Object[count];
        int writeIndex = 0;
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] != null)
            {
                result[writeIndex] = values[i];
                writeIndex++;
            }
        }

        return result;
    }

    private readonly struct DefaultSubtitleLine
    {
        public readonly string speaker;
        public readonly string text;
        public readonly float startDelay;
        public readonly float holdSeconds;

        public DefaultSubtitleLine(string newSpeaker, string newText, float newStartDelay, float newHoldSeconds)
        {
            speaker = newSpeaker;
            text = newText;
            startDelay = newStartDelay;
            holdSeconds = newHoldSeconds;
        }
    }
}
#endif
