using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HearthCompanion17F03ReplayController : MonoBehaviour
{
    public enum ReplayStep
    {
        Inactive,
        EnteringUnit,
        HumanParentDialogue,
        AwaitingUnitInspection,
        UnitInspection,
        SwitchingToMidday,
        MiddayConflict,
        AwaitingDaughter,
        AwaitingMother,
        SwitchingToNight,
        NightApproach,
        NightDialogue,
        NightShutdown,
        ReturningToHuman,
        PostReplayExplanation,
        AwaitingPostReplayDisposition,
        PostReplayDisposition,
        PostDispositionDialogue,
        ReturningToDoorTerminal,
        Complete
    }

    [Header("Flow")]
    [SerializeField] private MinLoopFlowController flowController;
    [SerializeField] private ViewSwitchController viewSwitchController;
    [SerializeField] private HearthCompanionHudController companionHud;
    [SerializeField] private MinLoopSubtitlePlayer subtitlePlayer;
    [SerializeField] private Hearth17F03InspectionPanel inspectionPanel;
    [SerializeField] private HearthTvTerminalController doorTerminal;
    [SerializeField] private CanvasGroup humanHudCanvasGroup;

    [Header("Human Rig")]
    [SerializeField] private Transform humanRoot;
    [SerializeField] private Camera humanCamera;
    [SerializeField] private FirstPersonMovement humanMovement;
    [SerializeField] private FirstPersonLook humanLook;
    [SerializeField] private PlayerInteraction humanInteraction;
    [SerializeField] private Rigidbody humanRigidbody;

    [Header("Companion Rig")]
    [SerializeField] private Transform robotRoot;
    [SerializeField] private Camera robotCamera;
    [SerializeField] private FirstPersonMovement robotMovement;
    [SerializeField] private FirstPersonLook robotLook;
    [SerializeField] private PlayerInteraction robotInteraction;
    [SerializeField] private Rigidbody robotRigidbody;
    [SerializeField] private float replayInteractionRange = 5f;

    [Header("Human / Inspection Anchors")]
    [SerializeField] private Transform humanEntryAnchor;
    [SerializeField] private Transform humanEntryCameraAnchor;
    [SerializeField] private Transform humanDoorReturnAnchor;
    [SerializeField] private Transform humanDoorReturnCameraAnchor;
    [SerializeField] private GameObject physicalUnitObject;
    [SerializeField] private Camera physicalUnitInspectionCamera;
    [SerializeField] private Hearth17F03UnitInteractable physicalUnitInteractable;

    [Header("Unit Inspection Camera Transition")]
    [SerializeField] private bool useSmoothInspectionCameraTransition = true;
    [SerializeField] private HearthTerminalCameraTransition inspectionCameraTransition;

    [Header("Replay Anchors")]
    [SerializeField] private Transform middayRobotAnchor;
    [SerializeField] private Transform middayRobotCameraAnchor;
    [SerializeField] private Transform nightRobotAnchor;
    [SerializeField] private Transform nightRobotCameraAnchor;

    [Header("Actors")]
    [SerializeField] private GameObject motherActor;
    [SerializeField] private Transform motherMoveRoot;
    [SerializeField] private HearthActorAnimatorDriver motherAnimation;
    [SerializeField] private Transform motherHumanAnchor;
    [SerializeField] private Transform motherReplayAnchor;
    [SerializeField] private GameObject fatherActor;
    [SerializeField] private Transform fatherMoveRoot;
    [SerializeField] private HearthActorAnimatorDriver fatherAnimation;
    [SerializeField] private Transform fatherHumanAnchor;
    [SerializeField] private GameObject middayFatherActor;
    [SerializeField] private Transform middayFatherMoveRoot;
    [SerializeField] private HearthActorAnimatorDriver middayFatherAnimation;
    [SerializeField] private Transform middayFatherAnchor;
    [SerializeField] private GameObject daughterActor;
    [SerializeField] private Transform daughterMoveRoot;
    [SerializeField] private HearthActorAnimatorDriver daughterAnimation;
    [SerializeField] private Transform daughterMiddayAnchor;
    [SerializeField] private Transform daughterNightStartAnchor;
    [SerializeField] private Transform[] daughterNightPathPoints;

    [Header("Actor Animation Ids")]
    [SerializeField] private string motherSitToStandId = "SitToStand";
    [SerializeField] private string motherTalkingId = "Talking";
    [SerializeField] private string motherArguingId = "StandingArguing";
    [SerializeField] private string fatherSittingId = "Sitting";
    [SerializeField] private string daughterSittingId = "SittingPose";
    [SerializeField] private string daughterSitupId = "SitupToIdle";
    [SerializeField] private string daughterTalkingId = "Talking";
    [SerializeField] private string daughterWalkId = "Walk";
    [SerializeField] private string daughterEnteringCodeId = "EnteringCode";

    [Header("Gaze Interactions")]
    [SerializeField] private Hearth17F03GazeInteractable daughterGazeInteractable;
    [SerializeField] private Hearth17F03GazeInteractable motherGazeInteractable;

    [Header("Night Door And Movement")]
    [SerializeField] private SmartDoorController daughterDoor;
    [SerializeField] private float daughterWalkSpeed = 1.1f;
    [SerializeField] private float daughterRotateSpeed = 360f;
    [SerializeField] private float actorSnapDistance = 0.03f;
    [SerializeField] private float actorNoProgressSeconds = 1.25f;

    [Header("Dialogue Assets")]
    [SerializeField] private HearthDialogueSequence terminalEntrySequence;
    [SerializeField] private HearthDialogueSequence humanParentSequence;
    [SerializeField] private HearthDialogueSequence inspectionRecallPromptSequence;
    [SerializeField] private HearthDialogueSequence middayConflictSequence;
    [SerializeField] private HearthDialogueSequence mediateToDaughterSequence;
    [SerializeField] private HearthDialogueSequence mediateToMotherSequence;
    [SerializeField] private HearthDialogueSequence nightDaughterSequence;
    [SerializeField] private HearthDialogueSequence nightShutdownLeadInSequence;
    [SerializeField] private HearthDialogueSequence nightShutdownSequence;
    [SerializeField] private HearthDialogueSequence postReplayQuestionSequence;
    [SerializeField] private HearthDialogueSequence postReplayExplanationSequence;
    [SerializeField] private HearthDialogueSequence postReplayOptionASequence;
    [SerializeField] private HearthDialogueSequence postReplayOptionBSequence;
    [SerializeField] private HearthDialogueSequence corridorEvaluationASequence;
    [SerializeField] private HearthDialogueSequence corridorEvaluationBSequence;
    [SerializeField] private HearthDialogueSequence postReplayPositiveTrustResultSequence;
    [SerializeField] private HearthDialogueSequence postReplayNegativeTrustWarningSequence;
    [SerializeField] private HearthDialogueSequence postReplayCompletionSequence;

    [Header("Companion HUD Scene Ids")]
    [SerializeField] private string conflictSceneId = "17F03_01";
    [SerializeField] private string daughterMediationSceneId = "17F03_02";
    [SerializeField] private string motherMediationSceneId = "17F03_03";
    [SerializeField] private string nightDialogueSceneId = "17F03_04";
    [SerializeField] private string deepSleepSceneId = "17F03_05";

    [Header("Timing")]
    [SerializeField] private float fadeOutSeconds = 0.5f;
    [SerializeField] private float blackHoldSeconds = 0.12f;
    [SerializeField] private float fadeInSeconds = 0.5f;
    [SerializeField] private float afterMediationSeconds = 0.8f;
    [SerializeField] private float afterDoorOpenSeconds = 0.35f;
    [SerializeField] private float deepSleepSeconds = 3.5f;
    [SerializeField] private float deepSleepPowerOffDelaySeconds = 0.65f;
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Blackout Overlay")]
    [SerializeField] private CanvasGroup blackoutCanvasGroup;
    [SerializeField] private Image blackoutImage;
    [SerializeField] private int blackoutSortingOrder = 8000;

    [Header("Events")]
    [SerializeField] private UnityEvent onHumanEntryStarted = new UnityEvent();
    [SerializeField] private UnityEvent onUnitInspectionReady = new UnityEvent();
    [SerializeField] private UnityEvent onMiddayReplayStarted = new UnityEvent();
    [SerializeField] private UnityEvent onNightReplayStarted = new UnityEvent();
    [SerializeField] private UnityEvent onPostReplayStarted = new UnityEvent();
    [SerializeField] private UnityEvent onDispositionA = new UnityEvent();
    [SerializeField] private UnityEvent onDispositionB = new UnityEvent();
    [SerializeField] private UnityEvent onHouseholdCompleted = new UnityEvent();

    [Header("Story SFX")]
    [SerializeField] private HearthSfxCuePlayer sfxCuePlayer;
    [SerializeField] private string motherStandCueId = "Mother.StandUp";
    [SerializeField] private string daughterStandCueId = "Daughter.StandUp";
    [SerializeField] private string daughterWalkCueId = "Daughter.Walk";
    [SerializeField] private string keypadCueId = "Daughter.Keypad";
    [SerializeField] private string glitchCueId = "System.Glitch";
    [SerializeField] private string powerOffCueId = "System.PowerOff";

    [Header("Runtime")]
    [SerializeField] private ReplayStep currentStep = ReplayStep.Inactive;

    private Coroutine activeRoutine;
    private bool daughterConfirmed;
    private bool motherConfirmed;
    private bool humanPoseSaved;
    private Vector3 savedHumanPosition;
    private Quaternion savedHumanRotation;
    private Vector3 savedHumanCameraLocalPosition;
    private Quaternion savedHumanCameraLocalRotation;
    private float previousRobotInteractionRange;
    private MinLoopFlowController subscribedFlow;
    private bool humanHudSuppressed;
    private float savedHumanHudAlpha = 1f;
    private bool savedHumanHudInteractable;
    private bool savedHumanHudBlocksRaycasts;
    private bool listeningHud;
    private MinLoopDispositionChoice pendingDispositionChoice;
    private int pendingDispositionTrust;
    private bool dispositionResultReceived;
    private bool householdCompletionInvoked;

    public ReplayStep CurrentStep { get { return currentStep; } }

    private void Awake()
    {
        ResolveHumanHudCanvasGroup();
        DisableCompetingActorAnimationBehaviours();
        EnsureBlackoutOverlay();
        SetBlackoutAlpha(0f);
        SetGazeTarget(null);
        if (physicalUnitInteractable != null)
        {
            physicalUnitInteractable.SetAvailable(false);
        }
    }

    private void LateUpdate()
    {
        if (currentStep == ReplayStep.AwaitingDaughter ||
            currentStep == ReplayStep.AwaitingMother)
        {
            RefreshGazeHoldPrompt();
        }
    }

    private void OnEnable()
    {
        SubscribeInspectionPanel();
        SubscribeFlow();
        SubscribeHud();
    }

    private void OnDisable()
    {
        UnsubscribeInspectionPanel();
        UnsubscribeFlow();
        UnsubscribeHud();
        StopAllStorySfx();
    }

    private void OnValidate()
    {
        replayInteractionRange = Mathf.Max(0.1f, replayInteractionRange);
        daughterWalkSpeed = Mathf.Max(0.01f, daughterWalkSpeed);
        daughterRotateSpeed = Mathf.Max(1f, daughterRotateSpeed);
        actorSnapDistance = Mathf.Max(0.001f, actorSnapDistance);
        actorNoProgressSeconds = Mathf.Max(0.1f, actorNoProgressSeconds);
        fadeOutSeconds = Mathf.Max(0f, fadeOutSeconds);
        blackHoldSeconds = Mathf.Max(0f, blackHoldSeconds);
        fadeInSeconds = Mathf.Max(0f, fadeInSeconds);
        afterMediationSeconds = Mathf.Max(0f, afterMediationSeconds);
        afterDoorOpenSeconds = Mathf.Max(0f, afterDoorOpenSeconds);
        deepSleepSeconds = Mathf.Max(0f, deepSleepSeconds);
        deepSleepPowerOffDelaySeconds = Mathf.Max(0f, deepSleepPowerOffDelaySeconds);
    }

    public void BeginHumanEntry()
    {
        BeginHumanEntry(flowController);
    }

    public void BeginHumanEntry(MinLoopFlowController owner)
    {
        if (owner != null)
        {
            SetFlowController(owner);
        }

        StartFlow(HumanEntryRoutine());
    }

    public void OpenUnitInspection()
    {
        if (currentStep == ReplayStep.AwaitingPostReplayDisposition)
        {
            StartFlow(OpenDispositionInspectionRoutine());
            return;
        }

        if (currentStep != ReplayStep.AwaitingUnitInspection)
        {
            return;
        }

        StartFlow(OpenInspectionRoutine());
    }

    public void BeginRecordedReplay()
    {
        BeginRecordedReplay(flowController);
    }

    public void BeginRecordedReplay(MinLoopFlowController owner)
    {
        if (owner != null)
        {
            SetFlowController(owner);
        }

        if (currentStep != ReplayStep.UnitInspection && currentStep != ReplayStep.AwaitingUnitInspection)
        {
            return;
        }

        StartFlow(RecordedReplayRoutine());
    }

    public void ConfirmCurrentGazeTarget(Hearth17F03GazeInteractable.Target target)
    {
        if (currentStep == ReplayStep.AwaitingDaughter && target == Hearth17F03GazeInteractable.Target.Daughter)
        {
            daughterConfirmed = true;
            SetGazeTarget(null);
        }
        else if (currentStep == ReplayStep.AwaitingMother && target == Hearth17F03GazeInteractable.Target.Mother)
        {
            motherConfirmed = true;
            SetGazeTarget(null);
        }
    }

    public void CancelFlow()
    {
        StopActiveRoutine();
        StopAllStorySfx();
        currentStep = ReplayStep.Inactive;
        daughterConfirmed = false;
        motherConfirmed = false;
        dispositionResultReceived = false;
        householdCompletionInvoked = false;
        SetGazeTarget(null);
        RestoreRobotInteractionRange();

        if (physicalUnitInteractable != null)
        {
            physicalUnitInteractable.SetAvailable(false);
        }

        if (inspectionPanel != null)
        {
            inspectionPanel.Close();
        }

        if (inspectionCameraTransition != null)
        {
            inspectionCameraTransition.CancelTransition();
        }

        SetInspectionCameraActive(false);
        RestoreHumanHud();
        SetHumanControl(true, true, true);
        SetRobotControl(false, false, false);
        SetPhysicalUnitVisible(true);
        if (companionHud != null)
        {
            companionHud.SetHoldPromptVisible(false);
            companionHud.SetVisible(false);
        }

        SetBlackoutAlpha(0f);
    }

    public void SetFlowController(MinLoopFlowController value)
    {
        if (flowController == value)
        {
            return;
        }

        UnsubscribeFlow();
        flowController = value;
        SubscribeFlow();
    }

    private IEnumerator HumanEntryRoutine()
    {
        RestoreHumanHud();
        currentStep = ReplayStep.EnteringUnit;
        onHumanEntryStarted.Invoke();
        SetHumanControl(false, false, false);
        SetRobotControl(false, false, false);
        SetGazeTarget(null);
        if (physicalUnitInteractable != null) physicalUnitInteractable.SetAvailable(false);

        yield return PlayDialogue(terminalEntrySequence);

        yield return FadeBlackTo(1f, fadeOutSeconds);
        yield return WaitSeconds(blackHoldSeconds);

        if (viewSwitchController != null && viewSwitchController.CurrentMode != ViewSwitchController.ViewMode.Human)
        {
            viewSwitchController.SwitchToHuman();
            while (viewSwitchController.IsSwitching) yield return null;
        }

        TeleportHuman(humanEntryAnchor, humanEntryCameraAnchor);
        PrepareHumanActors();
        SetPhysicalUnitVisible(true);
        SetInspectionCameraActive(false);
        SetHumanControl(true, true, true);

        yield return FadeBlackTo(0f, fadeInSeconds);

        currentStep = ReplayStep.HumanParentDialogue;
        if (flowController != null) flowController.NotifyResidentUnitDialogueStarted();
        yield return PlayEntryParentPerformance();

        currentStep = ReplayStep.AwaitingUnitInspection;
        if (physicalUnitInteractable != null) physicalUnitInteractable.SetAvailable(true);
        if (flowController != null) flowController.NotifyResidentUnitInspectionReady();
        onUnitInspectionReady.Invoke();
        activeRoutine = null;
    }

    private IEnumerator OpenInspectionRoutine()
    {
        SaveHumanPose();
        SetHumanControl(false, false, false);
        SuppressHumanHud();
        if (physicalUnitInteractable != null) physicalUnitInteractable.SetAvailable(false);

        bool usedSmoothTransition = CanUseSmoothInspectionEnterTransition();
        if (usedSmoothTransition)
        {
            SetBlackoutAlpha(0f);
            yield return inspectionCameraTransition.TransitionToTerminal(
                humanCamera,
                physicalUnitInspectionCamera,
                null);
        }
        else
        {
            yield return FadeBlackTo(1f, fadeOutSeconds);
        }

        SetInspectionCameraActive(true);
        if (inspectionPanel != null)
        {
            inspectionPanel.SetRecallAvailable(false);
            inspectionPanel.Open();
        }

        currentStep = ReplayStep.UnitInspection;
        if (!usedSmoothTransition)
        {
            yield return FadeBlackTo(0f, fadeInSeconds);
        }

        yield return PlayDialogue(inspectionRecallPromptSequence);
        if (inspectionPanel != null)
        {
            inspectionPanel.SetRecallAvailable(true);
        }

        activeRoutine = null;
    }

    private IEnumerator CloseInspectionRoutine()
    {
        if (inspectionPanel != null) inspectionPanel.Close();

        bool usedSmoothTransition = CanUseSmoothInspectionExitTransition();
        if (usedSmoothTransition)
        {
            SetBlackoutAlpha(0f);
            yield return inspectionCameraTransition.TransitionToPlayer(
                humanCamera,
                physicalUnitInspectionCamera,
                null,
                true,
                false,
                true,
                false);
        }
        else
        {
            yield return FadeBlackTo(1f, fadeOutSeconds);
        }

        SetInspectionCameraActive(false);
        RestoreSavedHumanPose();
        RestoreHumanHud();
        SetHumanControl(true, true, true);
        currentStep = ReplayStep.AwaitingUnitInspection;
        if (physicalUnitInteractable != null) physicalUnitInteractable.SetAvailable(true);
        if (!usedSmoothTransition)
        {
            yield return FadeBlackTo(0f, fadeInSeconds);
        }

        activeRoutine = null;
    }

    private IEnumerator OpenDispositionInspectionRoutine()
    {
        SaveHumanPose();
        SetHumanControl(false, false, false);
        SuppressHumanHud();
        if (physicalUnitInteractable != null) physicalUnitInteractable.SetAvailable(false);

        bool usedSmoothTransition = CanUseSmoothInspectionEnterTransition();
        if (usedSmoothTransition)
        {
            SetBlackoutAlpha(0f);
            yield return inspectionCameraTransition.TransitionToTerminal(
                humanCamera,
                physicalUnitInspectionCamera,
                null);
        }
        else
        {
            yield return FadeBlackTo(1f, fadeOutSeconds);
        }

        SetInspectionCameraActive(true);
        currentStep = ReplayStep.PostReplayExplanation;
        dispositionResultReceived = false;

        if (inspectionPanel != null)
        {
            inspectionPanel.OpenDispositionChoice(false);
        }

        if (!usedSmoothTransition)
        {
            yield return FadeBlackTo(0f, fadeInSeconds);
        }

        yield return PlayDialogue(postReplayExplanationSequence);

        if (flowController != null)
        {
            flowController.BeginExternalDispositionChoice("17F03");
        }

        currentStep = ReplayStep.PostReplayDisposition;
        if (inspectionPanel != null)
        {
            inspectionPanel.SetChoiceInputEnabled(true);
        }

        activeRoutine = null;
    }

    private IEnumerator RecordedReplayRoutine()
    {
        currentStep = ReplayStep.SwitchingToMidday;
        if (inspectionPanel != null)
        {
            inspectionPanel.SetRecallAvailable(false);
        }

        yield return FadeBlackTo(1f, fadeOutSeconds);
        if (inspectionPanel != null) inspectionPanel.Close();
        SetInspectionCameraActive(false);
        if (physicalUnitInteractable != null) physicalUnitInteractable.SetAvailable(false);

        SetPhysicalUnitVisible(false);
        PrepareMiddayActors();
        TeleportRobot(middayRobotAnchor, middayRobotCameraAnchor);
        ShowHudScene(conflictSceneId);
        CaptureRobotInteractionRange();

        if (viewSwitchController != null && viewSwitchController.CurrentMode != ViewSwitchController.ViewMode.Companion)
        {
            viewSwitchController.SwitchToCompanion();
            while (viewSwitchController.IsSwitching) yield return null;
        }

        SetRobotControl(false, true, false);
        yield return FadeBlackTo(0f, fadeInSeconds);

        currentStep = ReplayStep.MiddayConflict;
        onMiddayReplayStarted.Invoke();
        yield return PlayDialogue(middayConflictSequence);

        daughterConfirmed = false;
        currentStep = ReplayStep.AwaitingDaughter;
        ShowHudScene(daughterMediationSceneId);
        SetGazeTarget(Hearth17F03GazeInteractable.Target.Daughter);
        while (!daughterConfirmed) yield return null;

        PlayStorySfx(daughterStandCueId);
        yield return PlayActorOnceAndDialogueThenLoop(
            daughterAnimation,
            daughterSitupId,
            daughterTalkingId,
            mediateToDaughterSequence);

        motherConfirmed = false;
        currentStep = ReplayStep.AwaitingMother;
        ShowHudScene(motherMediationSceneId);
        SetGazeTarget(Hearth17F03GazeInteractable.Target.Mother);
        while (!motherConfirmed) yield return null;
        yield return PlayDialogue(mediateToMotherSequence);
        yield return WaitSeconds(afterMediationSeconds);

        currentStep = ReplayStep.SwitchingToNight;
        SetGazeTarget(null);
        SetRobotControl(false, false, false);
        yield return FadeBlackTo(1f, fadeOutSeconds);
        yield return WaitSeconds(blackHoldSeconds);

        PrepareNightActors();
        TeleportRobot(nightRobotAnchor, nightRobotCameraAnchor);
        ShowHudScene(nightDialogueSceneId);
        SetRobotControl(false, true, false);
        currentStep = ReplayStep.NightApproach;
        onNightReplayStarted.Invoke();
        yield return FadeBlackTo(0f, fadeInSeconds);

        if (daughterDoor != null)
        {
            daughterDoor.Open();
            while (daughterDoor.IsMoving) yield return null;
        }

        yield return WaitSeconds(afterDoorOpenSeconds);
        PlayActorLoop(daughterAnimation, daughterWalkId);
        StartStorySfxLoop(daughterWalkCueId);
        yield return MoveActorAlongPath(daughterMoveRoot, daughterNightPathPoints);
        StopStorySfx(daughterWalkCueId);

        currentStep = ReplayStep.NightDialogue;
        PlayActorLoop(daughterAnimation, daughterTalkingId);
        yield return PlayDialogue(nightDaughterSequence);
        yield return PlayDialogue(nightShutdownLeadInSequence);

        currentStep = ReplayStep.NightShutdown;
        ShowHudScene(deepSleepSceneId);
        PlayStorySfx(keypadCueId);
        yield return PlayActorOnceAndDialogueThenHold(daughterAnimation, daughterEnteringCodeId, nightShutdownSequence);
        PlayStorySfx(glitchCueId);
        if (companionHud != null) companionHud.PlayDeepSleep();

        float powerOffDelay = Mathf.Min(deepSleepSeconds, deepSleepPowerOffDelaySeconds);
        if (powerOffDelay > 0f)
        {
            yield return WaitSeconds(powerOffDelay);
        }

        PlayStorySfx(powerOffCueId);
        if (deepSleepSeconds > powerOffDelay)
        {
            yield return WaitSeconds(deepSleepSeconds - powerOffDelay);
        }

        yield return ReturnToHumanAndDispositionRoutine();
        activeRoutine = null;
    }

    private IEnumerator ReturnToHumanAndDispositionRoutine()
    {
        currentStep = ReplayStep.ReturningToHuman;
        StopStorySfx(daughterWalkCueId);
        SetRobotControl(false, false, false);
        SetGazeTarget(null);
        yield return FadeBlackTo(1f, fadeOutSeconds);

        SetPhysicalUnitVisible(true);
        PrepareHumanActors();
        RestoreSavedHumanPose();
        SetInspectionCameraActive(false);

        if (viewSwitchController != null && viewSwitchController.CurrentMode != ViewSwitchController.ViewMode.Human)
        {
            viewSwitchController.SwitchToHuman();
            while (viewSwitchController.IsSwitching) yield return null;
        }

        RestoreRobotInteractionRange();
        if (companionHud != null)
        {
            companionHud.SetHoldPromptVisible(false);
            companionHud.SetVisible(false);
        }

        RestoreHumanHud();
        SetHumanControl(true, true, true);
        yield return FadeBlackTo(0f, fadeInSeconds);

        currentStep = ReplayStep.PostReplayExplanation;
        if (flowController != null) flowController.NotifyResidentPostReplayStarted();
        onPostReplayStarted.Invoke();
        yield return PlayDialogue(postReplayQuestionSequence);

        currentStep = ReplayStep.AwaitingPostReplayDisposition;
        if (physicalUnitInteractable != null) physicalUnitInteractable.SetAvailable(true);
        SetHumanControl(true, true, true);
        activeRoutine = null;
    }

    private IEnumerator CompletePostReplayDispositionRoutine(MinLoopDispositionChoice choice)
    {
        currentStep = ReplayStep.PostDispositionDialogue;
        yield return WaitSeconds(0.18f);
        if (inspectionPanel != null) inspectionPanel.Close();

        bool usedSmoothTransition = CanUseSmoothInspectionExitTransition();
        if (usedSmoothTransition)
        {
            SetBlackoutAlpha(0f);
            yield return inspectionCameraTransition.TransitionToPlayer(
                humanCamera,
                physicalUnitInspectionCamera,
                null,
                true,
                false,
                true,
                false);
        }
        else
        {
            yield return FadeBlackTo(1f, fadeOutSeconds);
        }

        SetInspectionCameraActive(false);
        RestoreSavedHumanPose();
        RestoreHumanHud();
        SetHumanControl(false, true, false);
        if (!usedSmoothTransition)
        {
            yield return FadeBlackTo(0f, fadeInSeconds);
        }

        HearthDialogueSequence resultSequence = choice == MinLoopDispositionChoice.SystemRecommendedA
            ? postReplayOptionASequence
            : postReplayOptionBSequence;
        yield return PlayDialogue(resultSequence);

        SetHumanControl(false, false, false);
        yield return FadeBlackTo(1f, fadeOutSeconds);
        yield return WaitSeconds(blackHoldSeconds);
        TeleportHuman(humanDoorReturnAnchor, humanDoorReturnCameraAnchor);
        if (physicalUnitInteractable != null) physicalUnitInteractable.SetAvailable(false);
        yield return FadeBlackTo(0f, fadeInSeconds);
        SetHumanControl(false, true, false);

        HearthDialogueSequence corridorEvaluation = choice == MinLoopDispositionChoice.SystemRecommendedA
            ? corridorEvaluationASequence
            : corridorEvaluationBSequence;
        yield return PlayDialogue(corridorEvaluation);

        if (dispositionResultReceived && pendingDispositionTrust > 0)
        {
            yield return PlayDialogue(postReplayPositiveTrustResultSequence);
        }
        else if (dispositionResultReceived && pendingDispositionTrust < 0)
        {
            yield return PlayDialogue(postReplayNegativeTrustWarningSequence);
        }

        yield return PlayDialogue(postReplayCompletionSequence);
        SetHumanControl(true, true, true);

        if (!householdCompletionInvoked)
        {
            householdCompletionInvoked = true;
            if (choice == MinLoopDispositionChoice.SystemRecommendedA) onDispositionA.Invoke();
            else onDispositionB.Invoke();
            onHouseholdCompleted.Invoke();
        }

        if (flowController != null)
        {
            flowController.CompleteExternalDisposition();
        }

        currentStep = ReplayStep.Complete;
        activeRoutine = null;
    }

    private IEnumerator PlayEntryParentPerformance()
    {
        PlayStorySfx(motherStandCueId);
        float standSeconds = PlayActorOnce(motherAnimation, motherSitToStandId);
        Coroutine dialogueRoutine = StartCoroutine(PlayDialogue(humanParentSequence));
        if (standSeconds > 0f) yield return WaitSeconds(standSeconds);
        PlayActorLoop(motherAnimation, motherTalkingId);
        if (dialogueRoutine != null) yield return dialogueRoutine;
    }

    private IEnumerator PlayActorOnceAndDialogueThenLoop(
        HearthActorAnimatorDriver driver,
        string onceId,
        string loopId,
        HearthDialogueSequence sequence)
    {
        float animationSeconds = PlayActorOnce(driver, onceId);
        Coroutine dialogueRoutine = StartCoroutine(PlayDialogue(sequence));
        if (animationSeconds > 0f) yield return WaitSeconds(animationSeconds);
        PlayActorLoop(driver, loopId);
        if (dialogueRoutine != null) yield return dialogueRoutine;
    }

    private IEnumerator PlayActorOnceAndDialogueThenHold(
        HearthActorAnimatorDriver driver,
        string onceId,
        HearthDialogueSequence sequence)
    {
        float animationSeconds = PlayActorOnce(driver, onceId);
        Coroutine dialogueRoutine = StartCoroutine(PlayDialogue(sequence));
        if (animationSeconds > 0f) yield return WaitSeconds(animationSeconds);
        StopActorAndHold(driver);
        if (dialogueRoutine != null) yield return dialogueRoutine;
    }

    public void SetSfxCuePlayer(HearthSfxCuePlayer player)
    {
        sfxCuePlayer = player;
    }

    private void PlayStorySfx(string cueId)
    {
        if (sfxCuePlayer != null)
        {
            sfxCuePlayer.PlayCue(cueId);
        }
    }

    private void StartStorySfxLoop(string cueId)
    {
        if (sfxCuePlayer != null)
        {
            sfxCuePlayer.StartCueLoop(cueId);
        }
    }

    private void StopStorySfx(string cueId)
    {
        if (sfxCuePlayer != null)
        {
            sfxCuePlayer.StopCue(cueId);
        }
    }

    private void StopAllStorySfx()
    {
        if (sfxCuePlayer != null)
        {
            sfxCuePlayer.StopAllCues();
        }
    }

    private IEnumerator PlayDialogue(HearthDialogueSequence sequence)
    {
        if (subtitlePlayer == null || sequence == null || !sequence.HasLines)
        {
            yield break;
        }

        yield return subtitlePlayer.PlaySequenceAsset(sequence);
    }

    private void PrepareHumanActors()
    {
        DisableCompetingActorAnimationBehaviours();
        SetActorActive(motherActor, true);
        SetActorActive(fatherActor, true);
        SetActorActive(middayFatherActor, false);
        SetActorActive(daughterActor, false);
        SnapActor(motherMoveRoot, motherHumanAnchor);
        SnapActor(fatherMoveRoot, fatherHumanAnchor);
        if (!HoldActorAtStart(motherAnimation, motherSitToStandId))
        {
            PlayActorLoop(motherAnimation, motherTalkingId);
        }

        if (!HoldActorAtStart(fatherAnimation, fatherSittingId))
        {
            PlayActorLoop(fatherAnimation, fatherSittingId);
        }
    }

    private void PrepareMiddayActors()
    {
        DisableCompetingActorAnimationBehaviours();
        SetActorActive(motherActor, true);
        SetActorActive(fatherActor, false);
        SetActorActive(middayFatherActor, true);
        SetActorActive(daughterActor, true);
        SnapActor(motherMoveRoot, motherReplayAnchor);
        SnapActor(middayFatherMoveRoot, middayFatherAnchor);
        SnapActor(daughterMoveRoot, daughterMiddayAnchor);
        PlayActorOnce(motherAnimation, motherArguingId);
        if (!HoldActorAtStart(middayFatherAnimation, fatherSittingId))
        {
            PlayActorLoop(middayFatherAnimation, fatherSittingId);
        }
        if (!HoldActorAtStart(daughterAnimation, daughterSittingId))
        {
            PlayActorLoop(daughterAnimation, daughterSittingId);
        }
    }

    private void PrepareNightActors()
    {
        DisableCompetingActorAnimationBehaviours();
        SetActorActive(motherActor, false);
        SetActorActive(fatherActor, false);
        SetActorActive(middayFatherActor, false);
        SetActorActive(daughterActor, true);
        SnapActor(daughterMoveRoot, daughterNightStartAnchor);
        if (daughterDoor != null) daughterDoor.SnapClosed();
    }

    private void DisableCompetingActorAnimationBehaviours()
    {
        DisableCompetingActorAnimationBehaviours(motherActor);
        DisableCompetingActorAnimationBehaviours(fatherActor);
        DisableCompetingActorAnimationBehaviours(middayFatherActor);
        DisableCompetingActorAnimationBehaviours(daughterActor);
    }

    private static void DisableCompetingActorAnimationBehaviours(GameObject actor)
    {
        if (actor == null)
        {
            return;
        }

        MonoBehaviour[] behaviours = actor.GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour == null)
            {
                continue;
            }

            string typeName = behaviour.GetType().FullName;
            if (string.Equals(typeName, "CityPeople.CityPeople", System.StringComparison.Ordinal))
            {
                behaviour.enabled = false;
            }
        }
    }

    private void ShowHudScene(string sceneId)
    {
        if (companionHud == null || string.IsNullOrEmpty(sceneId))
        {
            return;
        }

        companionHud.SetAutoAdvanceOnHoldPrompt(false);
        companionHud.SetVisible(true);
        companionHud.ShowScene(sceneId);
        companionHud.SetHoldPromptVisible(false);
    }

    private void SetGazeTarget(Hearth17F03GazeInteractable.Target? target)
    {
        if (companionHud != null)
        {
            companionHud.SetHoldPromptVisible(false);
            companionHud.SetDirectionMarkerVisible(!target.HasValue);
            companionHud.SetDirectionGuideVisible(target.HasValue);
            Transform directionTarget = null;
            if (target.HasValue && target.Value == Hearth17F03GazeInteractable.Target.Daughter && daughterGazeInteractable != null)
            {
                directionTarget = daughterGazeInteractable.transform;
            }
            else if (target.HasValue && target.Value == Hearth17F03GazeInteractable.Target.Mother && motherGazeInteractable != null)
            {
                directionTarget = motherGazeInteractable.transform;
            }

            companionHud.SetDirectionTarget(directionTarget, robotCamera);
        }

        if (daughterGazeInteractable != null)
        {
            daughterGazeInteractable.SetAvailable(target.HasValue && target.Value == Hearth17F03GazeInteractable.Target.Daughter);
        }

        if (motherGazeInteractable != null)
        {
            motherGazeInteractable.SetAvailable(target.HasValue && target.Value == Hearth17F03GazeInteractable.Target.Mother);
        }
    }

    private void RefreshGazeHoldPrompt()
    {
        if (companionHud == null ||
            (currentStep != ReplayStep.AwaitingDaughter &&
             currentStep != ReplayStep.AwaitingMother))
        {
            return;
        }

        Hearth17F03GazeInteractable expectedTarget = null;
        if (currentStep == ReplayStep.AwaitingDaughter)
        {
            expectedTarget = daughterGazeInteractable;
        }
        else if (currentStep == ReplayStep.AwaitingMother)
        {
            expectedTarget = motherGazeInteractable;
        }

        bool canInteract = expectedTarget != null &&
            expectedTarget.CanInteract(robotCamera, replayInteractionRange);
        companionHud.SetHoldPromptVisible(canInteract);
    }

    private void SuppressHumanHud()
    {
        ResolveHumanHudCanvasGroup();
        if (humanHudCanvasGroup == null || humanHudSuppressed)
        {
            return;
        }

        humanHudSuppressed = true;
        savedHumanHudAlpha = humanHudCanvasGroup.alpha;
        savedHumanHudInteractable = humanHudCanvasGroup.interactable;
        savedHumanHudBlocksRaycasts = humanHudCanvasGroup.blocksRaycasts;
        humanHudCanvasGroup.alpha = 0f;
        humanHudCanvasGroup.interactable = false;
        humanHudCanvasGroup.blocksRaycasts = false;
    }

    private void RestoreHumanHud()
    {
        if (humanHudCanvasGroup == null || !humanHudSuppressed)
        {
            return;
        }

        humanHudCanvasGroup.alpha = savedHumanHudAlpha;
        humanHudCanvasGroup.interactable = savedHumanHudInteractable;
        humanHudCanvasGroup.blocksRaycasts = savedHumanHudBlocksRaycasts;
        humanHudSuppressed = false;
    }

    private void ResolveHumanHudCanvasGroup()
    {
        if (humanHudCanvasGroup != null && humanHudCanvasGroup.gameObject.activeInHierarchy)
        {
            return;
        }

        HearthFirstPersonHudController[] hudControllers =
            FindObjectsOfType<HearthFirstPersonHudController>(true);
        HearthFirstPersonHudController fallback = null;
        for (int i = 0; i < hudControllers.Length; i++)
        {
            HearthFirstPersonHudController candidate = hudControllers[i];
            if (candidate == null || candidate.gameObject.scene != gameObject.scene)
            {
                continue;
            }

            if (fallback == null)
            {
                fallback = candidate;
            }

            if (candidate.gameObject.activeInHierarchy)
            {
                fallback = candidate;
                break;
            }
        }

        if (fallback != null)
        {
            humanHudCanvasGroup = fallback.GetComponent<CanvasGroup>();
        }
    }

    private void SaveHumanPose()
    {
        if (humanRoot == null || humanCamera == null)
        {
            return;
        }

        humanPoseSaved = true;
        savedHumanPosition = humanRoot.position;
        savedHumanRotation = humanRoot.rotation;
        savedHumanCameraLocalPosition = humanCamera.transform.localPosition;
        savedHumanCameraLocalRotation = humanCamera.transform.localRotation;
    }

    private void RestoreSavedHumanPose()
    {
        if (!humanPoseSaved || humanRoot == null)
        {
            return;
        }

        humanRoot.SetPositionAndRotation(savedHumanPosition, savedHumanRotation);
        if (humanCamera != null)
        {
            humanCamera.transform.localPosition = savedHumanCameraLocalPosition;
            humanCamera.transform.localRotation = savedHumanCameraLocalRotation;
        }

        if (humanLook != null) humanLook.ForceLookFromCurrentTransforms();
        ClearVelocity(humanRigidbody);
    }

    private void TeleportHuman(Transform anchor, Transform cameraAnchor)
    {
        if (humanRoot == null || anchor == null)
        {
            return;
        }

        humanRoot.SetPositionAndRotation(anchor.position, anchor.rotation);
        if (humanCamera != null && cameraAnchor != null)
        {
            humanCamera.transform.SetPositionAndRotation(cameraAnchor.position, cameraAnchor.rotation);
        }

        if (humanLook != null) humanLook.ForceLookFromCurrentTransforms();
        ClearVelocity(humanRigidbody);
    }

    private void TeleportRobot(Transform anchor, Transform cameraAnchor)
    {
        if (robotRoot == null || anchor == null)
        {
            return;
        }

        robotRoot.SetPositionAndRotation(anchor.position, anchor.rotation);
        if (robotCamera != null && cameraAnchor != null)
        {
            robotCamera.transform.SetPositionAndRotation(cameraAnchor.position, cameraAnchor.rotation);
        }

        if (robotLook != null) robotLook.ForceLookFromCurrentTransforms();
        ClearVelocity(robotRigidbody);
    }

    private void SetHumanControl(bool move, bool look, bool interact)
    {
        if (humanMovement != null) humanMovement.enabled = move;
        if (humanLook != null) humanLook.enabled = look;
        if (humanInteraction != null) humanInteraction.SetInteractionEnabled(interact);
        if (!move) ClearVelocity(humanRigidbody);
    }

    private void SetRobotControl(bool move, bool look, bool interact)
    {
        if (robotMovement != null) robotMovement.enabled = move;
        if (robotLook != null) robotLook.enabled = look;
        if (robotInteraction != null) robotInteraction.SetInteractionEnabled(interact);
        if (!move) ClearVelocity(robotRigidbody);
    }

    private void SetInspectionCameraActive(bool inspectionActive)
    {
        SetCameraAndListener(humanCamera, !inspectionActive);
        SetCameraAndListener(physicalUnitInspectionCamera, inspectionActive);
        if (humanCamera != null) humanCamera.tag = inspectionActive ? "Untagged" : "MainCamera";
        if (physicalUnitInspectionCamera != null) physicalUnitInspectionCamera.tag = inspectionActive ? "MainCamera" : "Untagged";
    }

    private bool CanUseSmoothInspectionEnterTransition()
    {
        return useSmoothInspectionCameraTransition &&
               inspectionCameraTransition != null &&
               inspectionCameraTransition.CanRunEnterTransition(humanCamera, physicalUnitInspectionCamera);
    }

    private bool CanUseSmoothInspectionExitTransition()
    {
        return useSmoothInspectionCameraTransition &&
               inspectionCameraTransition != null &&
               inspectionCameraTransition.CanRunExitTransition(humanCamera, physicalUnitInspectionCamera);
    }

    private static void SetCameraAndListener(Camera camera, bool active)
    {
        if (camera == null) return;
        camera.enabled = active;
        AudioListener listener = camera.GetComponent<AudioListener>();
        if (listener != null) listener.enabled = active;
    }

    private void CaptureRobotInteractionRange()
    {
        if (robotInteraction == null) return;
        previousRobotInteractionRange = robotInteraction.interactionRange;
        robotInteraction.interactionRange = replayInteractionRange;
    }

    private void RestoreRobotInteractionRange()
    {
        if (robotInteraction != null && previousRobotInteractionRange > 0f)
        {
            robotInteraction.interactionRange = previousRobotInteractionRange;
        }
    }

    private IEnumerator MoveActorAlongPath(Transform actor, Transform[] path)
    {
        if (actor == null || path == null) yield break;
        for (int i = 0; i < path.Length; i++)
        {
            if (path[i] != null) yield return MoveActorToAnchor(actor, path[i]);
        }
    }

    private IEnumerator MoveActorToAnchor(Transform actor, Transform anchor)
    {
        float lastDistance = Vector3.Distance(actor.position, anchor.position);
        float noProgress = 0f;
        while ((actor.position - anchor.position).sqrMagnitude > actorSnapDistance * actorSnapDistance)
        {
            float delta = GetDeltaTime();
            actor.position = Vector3.MoveTowards(actor.position, anchor.position, daughterWalkSpeed * delta);
            actor.rotation = Quaternion.RotateTowards(actor.rotation, anchor.rotation, daughterRotateSpeed * delta);
            float distance = Vector3.Distance(actor.position, anchor.position);
            if (lastDistance - distance > 0.001f)
            {
                noProgress = 0f;
                lastDistance = distance;
            }
            else
            {
                noProgress += delta;
                if (noProgress >= actorNoProgressSeconds)
                {
                    Debug.LogWarning("[HearthCompanion17F03ReplayController] Actor path stalled at " + anchor.name + ". Snapping to keep the flow running.", this);
                    break;
                }
            }
            yield return null;
        }

        actor.SetPositionAndRotation(anchor.position, anchor.rotation);
    }

    private IEnumerator FadeBlackTo(float target, float duration)
    {
        EnsureBlackoutOverlay();
        if (blackoutCanvasGroup == null) yield break;
        float start = blackoutCanvasGroup.alpha;
        if (duration <= 0f)
        {
            SetBlackoutAlpha(target);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += GetDeltaTime();
            SetBlackoutAlpha(Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }
        SetBlackoutAlpha(target);
    }

    private IEnumerator WaitSeconds(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += GetDeltaTime();
            yield return null;
        }
    }

    private float GetDeltaTime()
    {
        return useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    }

    private void EnsureBlackoutOverlay()
    {
        if (blackoutCanvasGroup != null) return;
        GameObject root = new GameObject("Hearth17F03Blackout", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(CanvasGroup));
        root.transform.SetParent(transform, false);
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = blackoutSortingOrder;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        blackoutCanvasGroup = root.GetComponent<CanvasGroup>();
        GameObject imageObject = new GameObject("BlackOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(root.transform, false);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        blackoutImage = imageObject.GetComponent<Image>();
        blackoutImage.color = Color.black;
        blackoutImage.raycastTarget = false;
    }

    private void SetBlackoutAlpha(float value)
    {
        if (blackoutCanvasGroup != null)
        {
            blackoutCanvasGroup.alpha = Mathf.Clamp01(value);
            blackoutCanvasGroup.interactable = false;
            blackoutCanvasGroup.blocksRaycasts = value > 0.001f;
        }
    }

    private void StartFlow(IEnumerator routine)
    {
        StopActiveRoutine();
        activeRoutine = StartCoroutine(routine);
    }

    private void StopActiveRoutine()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }
    }

    private void SubscribeInspectionPanel()
    {
        if (inspectionPanel == null) return;
        inspectionPanel.RecallRequested -= HandleRecallRequested;
        inspectionPanel.CloseRequested -= HandleInspectionCloseRequested;
        inspectionPanel.RecallRequested += HandleRecallRequested;
        inspectionPanel.CloseRequested += HandleInspectionCloseRequested;
        inspectionPanel.ChoiceSubmitted -= HandleDispositionChoiceSubmitted;
        inspectionPanel.ChoiceSubmitted += HandleDispositionChoiceSubmitted;
    }

    private void UnsubscribeInspectionPanel()
    {
        if (inspectionPanel == null) return;
        inspectionPanel.RecallRequested -= HandleRecallRequested;
        inspectionPanel.CloseRequested -= HandleInspectionCloseRequested;
        inspectionPanel.ChoiceSubmitted -= HandleDispositionChoiceSubmitted;
    }

    private void HandleRecallRequested()
    {
        BeginRecordedReplay();
    }

    private void HandleInspectionCloseRequested()
    {
        if (currentStep == ReplayStep.UnitInspection)
        {
            StartFlow(CloseInspectionRoutine());
        }
    }

    private void HandleDispositionChoiceSubmitted(MinLoopDispositionChoice choice)
    {
        if (currentStep != ReplayStep.PostReplayDisposition)
        {
            return;
        }

        pendingDispositionChoice = choice;
        dispositionResultReceived = false;
        if (flowController == null || !flowController.SubmitDisposition(choice))
        {
            Debug.LogWarning("[HearthCompanion17F03ReplayController] The 17F03 disposition could not be submitted.", this);
            return;
        }

        StartFlow(CompletePostReplayDispositionRoutine(choice));
    }

    private void SubscribeFlow()
    {
        if (flowController == null || subscribedFlow == flowController) return;
        UnsubscribeFlow();
        subscribedFlow = flowController;
        subscribedFlow.DispositionApplied.AddListener(HandleDispositionApplied);
    }

    private void UnsubscribeFlow()
    {
        if (subscribedFlow != null)
        {
            subscribedFlow.DispositionApplied.RemoveListener(HandleDispositionApplied);
            subscribedFlow = null;
        }
    }

    private void SubscribeHud()
    {
        if (listeningHud || companionHud == null || companionHud.HoldPromptConfirmed == null)
        {
            return;
        }

        companionHud.HoldPromptConfirmed.AddListener(HandleHudHoldPromptConfirmed);
        listeningHud = true;
    }

    private void UnsubscribeHud()
    {
        if (!listeningHud || companionHud == null || companionHud.HoldPromptConfirmed == null)
        {
            listeningHud = false;
            return;
        }

        companionHud.HoldPromptConfirmed.RemoveListener(HandleHudHoldPromptConfirmed);
        listeningHud = false;
    }

    private void HandleHudHoldPromptConfirmed(string sceneId)
    {
        if (currentStep == ReplayStep.AwaitingDaughter &&
            string.Equals(sceneId, daughterMediationSceneId, System.StringComparison.OrdinalIgnoreCase) &&
            daughterGazeInteractable != null &&
            daughterGazeInteractable.CanInteract(robotCamera, replayInteractionRange))
        {
            ConfirmCurrentGazeTarget(Hearth17F03GazeInteractable.Target.Daughter);
            return;
        }

        if (currentStep == ReplayStep.AwaitingMother &&
            string.Equals(sceneId, motherMediationSceneId, System.StringComparison.OrdinalIgnoreCase) &&
            motherGazeInteractable != null &&
            motherGazeInteractable.CanInteract(robotCamera, replayInteractionRange))
        {
            ConfirmCurrentGazeTarget(Hearth17F03GazeInteractable.Target.Mother);
        }
    }

    private void HandleDispositionApplied(MinLoopDispositionChoice choice, int currentTrust, int delta)
    {
        if (flowController == null || !string.Equals(flowController.ActiveReplayResidentId, "17F03", System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        pendingDispositionChoice = choice;
        pendingDispositionTrust = currentTrust;
        dispositionResultReceived = true;
    }

    private void SetPhysicalUnitVisible(bool visible)
    {
        if (physicalUnitObject != null && physicalUnitObject.activeSelf != visible)
        {
            physicalUnitObject.SetActive(visible);
        }
    }

    private static void SetActorActive(GameObject actor, bool active)
    {
        if (actor != null && actor.activeSelf != active) actor.SetActive(active);
    }

    private static void SnapActor(Transform actor, Transform anchor)
    {
        if (actor != null && anchor != null) actor.SetPositionAndRotation(anchor.position, anchor.rotation);
    }

    private static bool HoldActorAtStart(HearthActorAnimatorDriver driver, string stateId)
    {
        return driver != null && driver.HasState(stateId) && driver.HoldStateAtStart(stateId) >= 0f;
    }

    private static float PlayActorOnce(HearthActorAnimatorDriver driver, string stateId)
    {
        if (driver == null || !driver.HasState(stateId)) return 0f;
        return driver.PlayOnce(stateId);
    }

    private static bool PlayActorLoop(HearthActorAnimatorDriver driver, string stateId)
    {
        if (driver == null || !driver.HasState(stateId)) return false;
        driver.PlayLoop(stateId);
        return true;
    }

    private static void StopActorAndHold(HearthActorAnimatorDriver driver)
    {
        if (driver == null) return;
        driver.StopAndHold();
        driver.SetRootMotion(false);
    }

    private static void ClearVelocity(Rigidbody body)
    {
        if (body == null || body.isKinematic) return;
        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
    }
}
