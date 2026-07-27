using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HearthCompanion17F02ReplayController : MonoBehaviour
{
    public enum ReplayStep
    {
        Inactive,
        BedroomWake,
        BedroomConfide,
        WifeExitLocked,
        DiningObservation,
        LivingRoomTerminal,
        ForcedShutdown,
        BlackAudio,
        ReturningToTerminal
    }

    [Header("Flow")]
    [SerializeField] private MinLoopFlowController flowController;
    [SerializeField] private ViewSwitchController viewSwitchController;
    [SerializeField] private HearthCompanionHudController companionHud;
    [SerializeField] private MinLoopSubtitlePlayer subtitlePlayer;
    [SerializeField] private bool autoFindReferences = true;

    [Header("Robot")]
    [SerializeField] private Transform robotRoot;
    [SerializeField] private Camera robotCamera;
    [SerializeField] private FirstPersonMovement robotMovement;
    [SerializeField] private FirstPersonLook robotLook;
    [SerializeField] private PlayerInteraction robotInteraction;
    [SerializeField] private Rigidbody robotRigidbody;

    [Header("Anchors")]
    [SerializeField] private Transform bedroomStartAnchor;
    [SerializeField] private Transform bedroomStartCameraAnchor;
    [SerializeField] private Transform livingRoomTerminalAnchor;
    [SerializeField] private Transform livingRoomTerminalCameraAnchor;

    [Header("HUD Scene Ids")]
    [SerializeField] private string bedroomWakeSceneId = "17F02_01";
    [SerializeField] private string bedroomConfideSceneId = "17F02_02";
    [SerializeField] private string diningObservationSceneId = "17F02_03";
    [SerializeField] private string logAccessSceneId = "17F02_04";
    [SerializeField] private string forcedShutdownSceneId = "17F02_05";
    [SerializeField] private string blackAudioSceneId = "17F02_06";

    [Header("Dialogue Assets")]
    [SerializeField] private bool preferDialogueSequenceAssets = true;
    [SerializeField] private HearthDialogueSequence bedroomWakeSequence;
    [SerializeField] private HearthDialogueSequence bedroomConfideSequence;
    [SerializeField] private HearthDialogueSequence bedroomComfortSequence;
    [SerializeField] private HearthDialogueSequence wifeExitSequence;
    [SerializeField] private HearthDialogueSequence diningObservationSequence;
    [SerializeField] private HearthDialogueSequence logAccessSequence;
    [SerializeField] private HearthDialogueSequence forcedShutdownSequence;
    [SerializeField] private HearthDialogueSequence blackAudioSequence;

    [Header("Timing")]
    [SerializeField] private float initialBlackSeconds = 0.6f;
    [SerializeField] private float wakeFadeSeconds = 0.45f;
    [SerializeField] private float bedroomPromptDelayAfterConfideSeconds = 1.5f;
    [SerializeField] private float waitAfterConfideSeconds = 0.5f;
    [SerializeField] private float wifeExitLockedSeconds = 3f;
    [SerializeField] private float waitBeforeDiningDialogueSeconds = 0.6f;
    [SerializeField] private float postDiningSilenceSeconds = 2f;
    [SerializeField] private float livingRoomFadeOutSeconds = 0.45f;
    [SerializeField] private float livingRoomBlackHoldSeconds = 0.25f;
    [SerializeField] private float livingRoomFadeInSeconds = 0.45f;
    [SerializeField] private float waitBeforeForcedShutdownSeconds = 0.8f;
    [SerializeField] private float shutdownEffectSeconds = 1.5f;
    [SerializeField] private float waitBeforeReturnSeconds = 0.8f;
    [SerializeField] private bool useUnscaledReplayTime = true;

    [Header("Interaction Gates")]
    [SerializeField] private bool showBedroomHoldPromptDuringConfide;
    [SerializeField] private bool waitForBedroomAcknowledgement = true;
    [SerializeField] private bool waitForShutdownConfirmation;

    [Header("Actor Visibility")]
    [SerializeField] private bool manageActorVisibility;
    [SerializeField] private GameObject bedroomWifeActor;
    [SerializeField] private GameObject diningWifeActor;
    [SerializeField] private GameObject diningHusbandActor;
    [SerializeField] private GameObject terminalHusbandActor;

    [Header("Wife Exit Blocking")]
    [SerializeField] private bool moveBedroomWifeToDoor = true;
    [SerializeField] private Transform bedroomWifeMoveRoot;
    [SerializeField] private bool useSimpleWifeExitRoute = true;
    [SerializeField] private Transform[] wifeBeforeDoorPathPoints;
    [SerializeField] private Transform[] wifeAfterDoorPathPoints;
    [SerializeField] private bool moveToDoorPauseBeforeOpening = true;
    [SerializeField] private Transform[] wifeExitPathPoints;
    [SerializeField] private Transform wifeDoorPauseAnchor;
    [SerializeField] private Transform wifeExitOutsideAnchor;
    [SerializeField] private float wifeWalkSpeed = 1.15f;
    [SerializeField] private float wifeRotateSpeed = 360f;
    [SerializeField] private float wifeAnchorSnapDistance = 0.03f;
    [SerializeField] private float wifeMoveNoProgressSeconds = 1.25f;
    [SerializeField] private float wifeMoveProgressEpsilon = 0.002f;
    [SerializeField] private float wifeDoorPauseSeconds = 0.45f;
    [SerializeField] private float waitAfterDoorOpenSeconds = 0.55f;
    [SerializeField] private bool hideBedroomWifeAfterExit;
    [SerializeField] private SmartDoorController wifeExitDoor;
    [SerializeField] private bool openDoorDuringWifeExit = true;
    [Tooltip("Legacy split-route setting. Used only when Use Simple Wife Exit Route is off.")]
    [SerializeField] private int openDoorAfterPathPointCount = -1;
    [SerializeField] private bool keepDoorOpenAfterWifeExit = true;

    [Header("Actor Poses")]
    [SerializeField] private HearthActorPosePreset bedroomWifePose;
    [SerializeField] private HearthActorPosePreset diningWifePose;
    [SerializeField] private HearthActorPosePreset diningHusbandPose;
    [SerializeField] private HearthActorPosePreset terminalHusbandPose;
    [SerializeField] private string bedroomWifePoseId = "BedroomSit";
    [SerializeField] private string diningSittingPoseId = "DiningSit";
    [SerializeField] private string terminalHusbandPoseId = "LeanToUnit";

    [Header("Actor Animations")]
    [SerializeField] private HearthActorAnimatorDriver bedroomWifeAnimation;
    [SerializeField] private string bedroomWifeIdleAnimationId = "SittingDisbelief";
    [SerializeField] private string bedroomWifeTalkingAnimationId = "SittingTalking";
    [Tooltip("Maximum seconds to keep the one-shot bedroom talking animation/dialogue before continuing. Set to 0 or below to wait for the full clip/dialogue.")]
    [SerializeField] private float bedroomTalkingMaxSeconds = 10f;
    [SerializeField] private string bedroomWifeSitToStandAnimationId = "SitToStand";
    [SerializeField] private string bedroomWifeWalkLoopAnimationId = "WalkLoop";
    [SerializeField] private string bedroomWifeOpenDoorAnimationId = "OpenDoorOutwards";
    [SerializeField] private float doorOpenDelayAfterAnimationStartSeconds = 0.5f;
    [SerializeField] private HearthActorAnimatorDriver diningWifeAnimation;
    [SerializeField] private string diningWifeAnimationId = "Sitting";
    [SerializeField] private HearthActorAnimatorDriver diningHusbandAnimation;
    [SerializeField] private string diningHusbandAnimationId = "SittingIdle";
    [SerializeField] private HearthActorAnimatorDriver terminalHusbandAnimation;
    [SerializeField] private string terminalHusbandAnimationId = "ButtonPushing";

    [Header("Blackout Overlay")]
    [SerializeField] private bool createBlackoutOverlay = true;
    [SerializeField] private CanvasGroup blackoutCanvasGroup;
    [SerializeField] private Image blackoutImage;
    [SerializeField] private Color blackoutColor = Color.black;
    [SerializeField] private int blackoutSortingOrder = 7000;
    [SerializeField] private bool blackoutUsesUnscaledTime = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onBedroomWakeStarted = new UnityEvent();
    [SerializeField] private UnityEvent onBedroomConfideStarted = new UnityEvent();
    [SerializeField] private UnityEvent onWifeExitStarted = new UnityEvent();
    [SerializeField] private UnityEvent onWifeExitFinished = new UnityEvent();
    [SerializeField] private UnityEvent onDiningObservationStarted = new UnityEvent();
    [SerializeField] private UnityEvent onLivingRoomTerminalStarted = new UnityEvent();
    [SerializeField] private UnityEvent onForcedShutdownStarted = new UnityEvent();
    [SerializeField] private UnityEvent onReplayFinished = new UnityEvent();

    [Header("Story SFX")]
    [SerializeField] private HearthSfxCuePlayer sfxCuePlayer;
    [SerializeField] private string wifeStandCueId = "Wife.StandUp";
    [SerializeField] private string wifeWalkCueId = "Wife.Walk";
    [SerializeField] private string diningFoleyCueId = "Dining.TableFoley";
    [SerializeField] private string dataScanCueId = "System.DataScan";
    [SerializeField] private string glitchCueId = "System.Glitch";
    [SerializeField] private string powerOffCueId = "System.PowerOff";

    [Header("Runtime")]
    [SerializeField] private ReplayStep currentStep = ReplayStep.Inactive;

    private Coroutine activeRoutine;
    private bool bedroomAcknowledged;
    private bool shutdownConfirmed;
    private bool listeningHud;

    public ReplayStep CurrentStep
    {
        get { return currentStep; }
    }

    private void Awake()
    {
        ResolveReferences();
        ConfigureBedroomWifeAnimationPolish();
        EnsureBlackoutOverlay();
    }

    private void OnEnable()
    {
        SubscribeHud();
    }

    private void OnDisable()
    {
        UnsubscribeHud();
        StopAllStorySfx();
    }

    private void OnValidate()
    {
        initialBlackSeconds = Mathf.Max(0f, initialBlackSeconds);
        wakeFadeSeconds = Mathf.Max(0f, wakeFadeSeconds);
        bedroomPromptDelayAfterConfideSeconds = Mathf.Max(0f, bedroomPromptDelayAfterConfideSeconds);
        waitAfterConfideSeconds = Mathf.Max(0f, waitAfterConfideSeconds);
        wifeExitLockedSeconds = Mathf.Max(0f, wifeExitLockedSeconds);
        waitBeforeDiningDialogueSeconds = Mathf.Max(0f, waitBeforeDiningDialogueSeconds);
        postDiningSilenceSeconds = Mathf.Max(0f, postDiningSilenceSeconds);
        livingRoomFadeOutSeconds = Mathf.Max(0f, livingRoomFadeOutSeconds);
        livingRoomBlackHoldSeconds = Mathf.Max(0f, livingRoomBlackHoldSeconds);
        livingRoomFadeInSeconds = Mathf.Max(0f, livingRoomFadeInSeconds);
        waitBeforeForcedShutdownSeconds = Mathf.Max(0f, waitBeforeForcedShutdownSeconds);
        shutdownEffectSeconds = Mathf.Max(0f, shutdownEffectSeconds);
        waitBeforeReturnSeconds = Mathf.Max(0f, waitBeforeReturnSeconds);
        wifeWalkSpeed = Mathf.Max(0.01f, wifeWalkSpeed);
        wifeRotateSpeed = Mathf.Max(1f, wifeRotateSpeed);
        wifeAnchorSnapDistance = Mathf.Max(0.001f, wifeAnchorSnapDistance);
        wifeMoveNoProgressSeconds = Mathf.Max(0.1f, wifeMoveNoProgressSeconds);
        wifeMoveProgressEpsilon = Mathf.Max(0.0001f, wifeMoveProgressEpsilon);
        wifeDoorPauseSeconds = Mathf.Max(0f, wifeDoorPauseSeconds);
        waitAfterDoorOpenSeconds = Mathf.Max(0f, waitAfterDoorOpenSeconds);
        doorOpenDelayAfterAnimationStartSeconds = Mathf.Max(0f, doorOpenDelayAfterAnimationStartSeconds);
    }

    public void BeginReplay()
    {
        BeginReplay(flowController);
    }

    public void PrepareReplayStart()
    {
        ResolveReferences();
        ConfigureBedroomWifeAnimationPolish();
        EnsureBlackoutOverlay();
        StopActiveRoutine();
        StopAllStorySfx();
        currentStep = ReplayStep.Inactive;
        bedroomAcknowledged = false;
        shutdownConfirmed = false;

        if (subtitlePlayer != null)
        {
            subtitlePlayer.Hide();
        }

        if (companionHud != null)
        {
            companionHud.SetHoldPromptVisible(false);
        }

        SetStageActors(true, false, false);
        PlayActorLoopOrPose(bedroomWifeAnimation, bedroomWifeIdleAnimationId, bedroomWifePose, bedroomWifePoseId);
        TeleportRobot(bedroomStartAnchor, bedroomStartCameraAnchor);
        SetRobotControl(false, false, false);
        SetBlackoutAlpha(1f);
    }

    public void BeginReplay(MinLoopFlowController owner)
    {
        ResolveReferences();
        EnsureBlackoutOverlay();

        if (owner != null)
        {
            flowController = owner;
        }

        StopActiveRoutine();
        SubscribeHud();
        activeRoutine = StartCoroutine(ReplayRoutine());
    }

    public void CancelReplay()
    {
        StopActiveRoutine();
        StopAllStorySfx();
        currentStep = ReplayStep.Inactive;
        bedroomAcknowledged = false;
        shutdownConfirmed = false;

        if (subtitlePlayer != null)
        {
            subtitlePlayer.Hide();
        }

        if (companionHud != null)
        {
            companionHud.SetHoldPromptVisible(false);
        }

        SetBlackoutAlpha(0f);
        SetRobotControl(false, false, false);
    }

    public void SetReferences(
        MinLoopFlowController newFlowController,
        ViewSwitchController newViewSwitchController,
        HearthCompanionHudController newCompanionHud,
        MinLoopSubtitlePlayer newSubtitlePlayer)
    {
        flowController = newFlowController;
        viewSwitchController = newViewSwitchController;
        companionHud = newCompanionHud;
        subtitlePlayer = newSubtitlePlayer;
        SubscribeHud();
    }

    public void SetRobotRig(
        Transform newRobotRoot,
        Camera newRobotCamera,
        FirstPersonMovement newRobotMovement,
        FirstPersonLook newRobotLook,
        PlayerInteraction newRobotInteraction,
        Rigidbody newRobotRigidbody)
    {
        robotRoot = newRobotRoot;
        robotCamera = newRobotCamera;
        robotMovement = newRobotMovement;
        robotLook = newRobotLook;
        robotInteraction = newRobotInteraction;
        robotRigidbody = newRobotRigidbody;
    }

    public void SetAnchors(Transform bedroomAnchor, Transform livingTerminalAnchor)
    {
        SetAnchors(bedroomAnchor, bedroomStartCameraAnchor, livingTerminalAnchor, livingRoomTerminalCameraAnchor);
    }

    public void SetAnchors(Transform bedroomAnchor, Transform livingTerminalAnchor, Transform livingTerminalCameraAnchor)
    {
        SetAnchors(bedroomAnchor, bedroomStartCameraAnchor, livingTerminalAnchor, livingTerminalCameraAnchor);
    }

    public void SetAnchors(Transform bedroomAnchor, Transform bedroomCameraAnchor, Transform livingTerminalAnchor, Transform livingTerminalCameraAnchor)
    {
        bedroomStartAnchor = bedroomAnchor;
        bedroomStartCameraAnchor = bedroomCameraAnchor;
        livingRoomTerminalAnchor = livingTerminalAnchor;
        livingRoomTerminalCameraAnchor = livingTerminalCameraAnchor;
    }

    private IEnumerator ReplayRoutine()
    {
        if (companionHud != null)
        {
            companionHud.SetAutoAdvanceOnHoldPrompt(false);
            companionHud.SetVisible(true);
            companionHud.SetHoldPromptVisible(false);
        }

        SetStageActors(true, false, false);
        PlayActorLoopOrPose(bedroomWifeAnimation, bedroomWifeIdleAnimationId, bedroomWifePose, bedroomWifePoseId);
        TeleportRobot(bedroomStartAnchor, bedroomStartCameraAnchor);
        SetRobotControl(false, false, false);

        currentStep = ReplayStep.BedroomWake;
        onBedroomWakeStarted.Invoke();
        SetBlackoutAlpha(1f);

        if (initialBlackSeconds > 0f)
        {
            yield return WaitForReplaySeconds(initialBlackSeconds);
        }

        yield return PlayDialogue(bedroomWakeSequence);
        PlayActorLoop(bedroomWifeAnimation, bedroomWifeIdleAnimationId);
        yield return FadeBlackTo(0f, wakeFadeSeconds);

        ShowHudScene(bedroomWakeSceneId, false);
        SetRobotControl(true, true, false);

        currentStep = ReplayStep.BedroomConfide;
        onBedroomConfideStarted.Invoke();
        ShowHudScene(bedroomConfideSceneId, showBedroomHoldPromptDuringConfide);
        bedroomAcknowledged = false;
        yield return PlayDialogue(bedroomConfideSequence);

        if (waitForBedroomAcknowledgement)
        {
            if (bedroomPromptDelayAfterConfideSeconds > 0f)
            {
                yield return WaitForReplaySeconds(bedroomPromptDelayAfterConfideSeconds);
            }

            if (companionHud != null)
            {
                companionHud.ShowCurrentHoldPrompt();
            }

            while (!bedroomAcknowledged)
            {
                yield return null;
            }

            yield return PlayBedroomTalkingAndComfort();
        }

        if (companionHud != null)
        {
            companionHud.SetHoldPromptVisible(false);
        }

        if (waitAfterConfideSeconds > 0f)
        {
            yield return WaitForReplaySeconds(waitAfterConfideSeconds);
        }

        currentStep = ReplayStep.WifeExitLocked;
        onWifeExitStarted.Invoke();
        SetRobotControl(false, true, false);
        yield return PlayDialogue(wifeExitSequence);

        if (moveBedroomWifeToDoor)
        {
            yield return MoveBedroomWifeExitRoutine();
        }

        if (wifeExitLockedSeconds > 0f)
        {
            yield return WaitForReplaySeconds(wifeExitLockedSeconds);
        }

        onWifeExitFinished.Invoke();

        currentStep = ReplayStep.DiningObservation;
        onDiningObservationStarted.Invoke();
        PlayStorySfx(diningFoleyCueId);
        SetStageActors(false, true, false);
        PlayActorLoopOrPose(diningWifeAnimation, diningWifeAnimationId, diningWifePose, diningSittingPoseId);
        PlayActorLoopOrPose(diningHusbandAnimation, diningHusbandAnimationId, diningHusbandPose, diningSittingPoseId);
        ShowHudScene(diningObservationSceneId, false);
        SetRobotControl(true, true, false);

        if (flowController != null)
        {
            flowController.NotifyMorningReviewStarted();
        }

        if (waitBeforeDiningDialogueSeconds > 0f)
        {
            yield return WaitForReplaySeconds(waitBeforeDiningDialogueSeconds);
        }

        yield return PlayDialogue(diningObservationSequence);

        if (postDiningSilenceSeconds > 0f)
        {
            yield return WaitForReplaySeconds(postDiningSilenceSeconds);
        }

        yield return FadeBlackTo(1f, livingRoomFadeOutSeconds);

        if (livingRoomBlackHoldSeconds > 0f)
        {
            yield return WaitForReplaySeconds(livingRoomBlackHoldSeconds);
        }

        currentStep = ReplayStep.LivingRoomTerminal;
        onLivingRoomTerminalStarted.Invoke();
        PlayStorySfx(dataScanCueId);
        SetStageActors(false, false, true);
        PlayActorLoopOrPose(terminalHusbandAnimation, terminalHusbandAnimationId, terminalHusbandPose, terminalHusbandPoseId);
        TeleportRobot(livingRoomTerminalAnchor, livingRoomTerminalCameraAnchor);
        ShowHudScene(logAccessSceneId, false);
        SetRobotControl(false, false, false);
        yield return FadeBlackTo(0f, livingRoomFadeInSeconds);
        yield return PlayDialogue(logAccessSequence);

        if (waitBeforeForcedShutdownSeconds > 0f)
        {
            yield return WaitForReplaySeconds(waitBeforeForcedShutdownSeconds);
        }

        currentStep = ReplayStep.ForcedShutdown;
        onForcedShutdownStarted.Invoke();
        shutdownConfirmed = false;
        ShowHudScene(forcedShutdownSceneId, waitForShutdownConfirmation);

        if (waitForShutdownConfirmation)
        {
            while (!shutdownConfirmed)
            {
                yield return null;
            }
        }

        yield return PlayDialogue(forcedShutdownSequence);

        if (companionHud != null)
        {
            companionHud.SetHoldPromptVisible(false);
            PlayStorySfx(glitchCueId);
            companionHud.PlayShutdownGlitch();
        }

        if (shutdownEffectSeconds > 0f)
        {
            yield return WaitForReplaySeconds(shutdownEffectSeconds);
        }

        PlayStorySfx(powerOffCueId);

        currentStep = ReplayStep.BlackAudio;
        ShowHudScene(blackAudioSceneId, false);
        yield return PlayDialogue(blackAudioSequence);

        if (waitBeforeReturnSeconds > 0f)
        {
            yield return WaitForReplaySeconds(waitBeforeReturnSeconds);
        }

        ReturnToTerminalForDisposition(false);
        activeRoutine = null;
    }

    private void ReturnToTerminalForDisposition(bool stopRoutine)
    {
        if (stopRoutine)
        {
            StopActiveRoutine();
        }

        currentStep = ReplayStep.ReturningToTerminal;
        StopAllStorySfx();
        SetRobotControl(false, false, false);

        if (companionHud != null)
        {
            companionHud.SetHoldPromptVisible(false);
        }

        onReplayFinished.Invoke();

        if (flowController != null)
        {
            flowController.NotifyReplayCompleted();
        }
    }

    private IEnumerator PlayDialogue(HearthDialogueSequence sequence)
    {
        if (subtitlePlayer == null || !preferDialogueSequenceAssets || sequence == null || !sequence.HasLines)
        {
            yield break;
        }

        yield return subtitlePlayer.PlaySequenceAsset(sequence);
    }

    private IEnumerator WaitForReplaySeconds(float seconds)
    {
        if (seconds <= 0f)
        {
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += GetReplayDeltaTime();
            yield return null;
        }
    }

    private float GetReplayDeltaTime()
    {
        return useUnscaledReplayTime ? Time.unscaledDeltaTime : Time.deltaTime;
    }

    private void ShowHudScene(string sceneId, bool showHoldPrompt)
    {
        if (companionHud == null || string.IsNullOrEmpty(sceneId))
        {
            return;
        }

        companionHud.SetVisible(true);
        companionHud.ShowScene(sceneId);
        companionHud.SetHoldPromptVisible(showHoldPrompt);
        companionHud.ResetHoldPrompt();
    }

    private void TeleportRobot(Transform anchor)
    {
        TeleportRobot(anchor, null);
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

        if (robotLook != null)
        {
            robotLook.ForceLookFromCurrentTransforms();
        }

        ClearRobotVelocity();
    }

    private void SetRobotControl(bool allowMove, bool allowLook, bool allowInteraction)
    {
        if (robotMovement != null)
        {
            robotMovement.enabled = allowMove;
        }

        if (robotLook != null)
        {
            robotLook.enabled = allowLook;
        }

        if (robotInteraction != null)
        {
            robotInteraction.SetInteractionEnabled(allowInteraction);
        }

        if (!allowMove)
        {
            ClearRobotVelocity();
        }
    }

    private void ClearRobotVelocity()
    {
        if (robotRigidbody == null)
        {
            return;
        }

        if (robotRigidbody.isKinematic)
        {
            return;
        }

        robotRigidbody.velocity = Vector3.zero;
        robotRigidbody.angularVelocity = Vector3.zero;
    }

    private void SetStageActors(bool bedroom, bool dining, bool terminal)
    {
        if (!manageActorVisibility)
        {
            return;
        }

        SetActiveIfDistinct(bedroomWifeActor, bedroom, diningWifeActor, diningHusbandActor, terminalHusbandActor);
        SetActiveIfDistinct(diningWifeActor, dining, bedroomWifeActor, terminalHusbandActor);
        SetActiveIfDistinct(diningHusbandActor, dining, bedroomWifeActor, terminalHusbandActor);
        SetActiveIfDistinct(terminalHusbandActor, terminal, bedroomWifeActor, diningWifeActor, diningHusbandActor);
    }

    private IEnumerator MoveBedroomWifeExitRoutine()
    {
        Transform moveRoot = ResolveBedroomWifeMoveRoot();
        if (moveRoot == null)
        {
            yield break;
        }

        if (useSimpleWifeExitRoute)
        {
            yield return MoveSimpleWifeExitRoute(moveRoot);
        }
        else
        {
            yield return MoveLegacyWifeExitRoute(moveRoot);
        }

        if (!keepDoorOpenAfterWifeExit && wifeExitDoor != null)
        {
            wifeExitDoor.Close();
        }

        if (hideBedroomWifeAfterExit && bedroomWifeActor != null)
        {
            bedroomWifeActor.SetActive(false);
        }
    }

    private IEnumerator MoveSimpleWifeExitRoute(Transform moveRoot)
    {
        PlayStorySfx(wifeStandCueId);
        yield return PlayActorOnceAndWait(bedroomWifeAnimation, bedroomWifeSitToStandAnimationId);
        PlayActorLoop(bedroomWifeAnimation, bedroomWifeWalkLoopAnimationId);
        StartStorySfxLoop(wifeWalkCueId);

        bool hasExplicitSimpleRoute = HasPathPoints(wifeBeforeDoorPathPoints) || HasPathPoints(wifeAfterDoorPathPoints);
        if (hasExplicitSimpleRoute)
        {
            yield return MoveActorAlongPath(moveRoot, wifeBeforeDoorPathPoints);
        }
        else
        {
            int fallbackSplitIndex = ResolveDefaultSimpleRouteSplitIndex();
            yield return MoveActorAlongPath(moveRoot, wifeExitPathPoints, 0, fallbackSplitIndex);
        }

        if (moveToDoorPauseBeforeOpening)
        {
            yield return MoveActorToAnchor(moveRoot, wifeDoorPauseAnchor);
        }

        StopActorAndHold(bedroomWifeAnimation);
        StopStorySfx(wifeWalkCueId);

        if (wifeDoorPauseSeconds > 0f)
        {
            yield return WaitForReplaySeconds(wifeDoorPauseSeconds);
        }

        yield return PlayOpenDoorAnimationAndTriggerDoor();

        if (waitAfterDoorOpenSeconds > 0f)
        {
            yield return WaitForReplaySeconds(waitAfterDoorOpenSeconds);
        }

        PlayActorLoop(bedroomWifeAnimation, bedroomWifeWalkLoopAnimationId);
        StartStorySfxLoop(wifeWalkCueId);

        if (hasExplicitSimpleRoute)
        {
            yield return MoveActorAlongPath(moveRoot, wifeAfterDoorPathPoints);
        }
        else
        {
            int fallbackSplitIndex = ResolveDefaultSimpleRouteSplitIndex();
            yield return MoveActorAlongPath(moveRoot, wifeExitPathPoints, fallbackSplitIndex, GetPathLength(wifeExitPathPoints));
        }

        yield return MoveActorToAnchor(moveRoot, wifeExitOutsideAnchor);
        StopStorySfx(wifeWalkCueId);
        FinalizeActorAtAnchor(moveRoot, wifeExitOutsideAnchor, bedroomWifeAnimation);
    }

    private IEnumerator MoveLegacyWifeExitRoute(Transform moveRoot)
    {
        PlayStorySfx(wifeStandCueId);
        yield return PlayActorOnceAndWait(bedroomWifeAnimation, bedroomWifeSitToStandAnimationId);
        PlayActorLoop(bedroomWifeAnimation, bedroomWifeWalkLoopAnimationId);
        StartStorySfxLoop(wifeWalkCueId);

        int splitIndex = ResolveDoorOpenPathSplitIndex();
        yield return MoveActorAlongPath(moveRoot, wifeExitPathPoints, 0, splitIndex);
        yield return MoveActorToAnchor(moveRoot, wifeDoorPauseAnchor);
        StopActorAndHold(bedroomWifeAnimation);
        StopStorySfx(wifeWalkCueId);

        if (wifeDoorPauseSeconds > 0f)
        {
            yield return WaitForReplaySeconds(wifeDoorPauseSeconds);
        }

        yield return PlayOpenDoorAnimationAndTriggerDoor();

        if (waitAfterDoorOpenSeconds > 0f)
        {
            yield return WaitForReplaySeconds(waitAfterDoorOpenSeconds);
        }

        PlayActorLoop(bedroomWifeAnimation, bedroomWifeWalkLoopAnimationId);
        StartStorySfxLoop(wifeWalkCueId);
        yield return MoveActorAlongPath(moveRoot, wifeExitPathPoints, splitIndex, GetPathLength(wifeExitPathPoints));
        yield return MoveActorToAnchor(moveRoot, wifeExitOutsideAnchor);
        StopStorySfx(wifeWalkCueId);
        FinalizeActorAtAnchor(moveRoot, wifeExitOutsideAnchor, bedroomWifeAnimation);
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

    private IEnumerator PlayOpenDoorAnimationAndTriggerDoor()
    {
        float animationSeconds = PlayActorOnce(bedroomWifeAnimation, bedroomWifeOpenDoorAnimationId);
        float doorDelay = animationSeconds > 0f ? doorOpenDelayAfterAnimationStartSeconds : 0f;
        Coroutine doorRoutine = null;
        if (openDoorDuringWifeExit && wifeExitDoor != null)
        {
            doorRoutine = StartCoroutine(OpenDoorAfterDelay(doorDelay));
        }
        else if (openDoorDuringWifeExit && wifeExitDoor == null)
        {
            Debug.LogWarning("[HearthCompanion17F02ReplayController] Wife exit door is not assigned. Skipping door open and continuing the replay.", this);
        }

        if (animationSeconds > 0f)
        {
            yield return WaitForReplaySeconds(animationSeconds);
        }

        if (doorRoutine != null)
        {
            yield return doorRoutine;
        }

        if (wifeExitDoor != null)
        {
            while (wifeExitDoor.IsMoving)
            {
                yield return null;
            }
        }

        if (bedroomWifeAnimation != null)
        {
            bedroomWifeAnimation.SetRootMotion(false);
        }
    }

    private IEnumerator PlayBedroomTalkingAndComfort()
    {
        float animationSeconds = PlayActorOnce(bedroomWifeAnimation, bedroomWifeTalkingAnimationId);
        float maxSeconds = ResolveBedroomTalkingMaxSeconds(animationSeconds);
        bool waitFullLength = bedroomTalkingMaxSeconds <= 0f;
        bool hasDialogue = subtitlePlayer != null && preferDialogueSequenceAssets && bedroomComfortSequence != null && bedroomComfortSequence.HasLines;
        bool dialogueFinished = false;
        Coroutine dialogueRoutine = null;
        if (hasDialogue)
        {
            dialogueRoutine = StartCoroutine(PlayDialogueAndMarkDone(bedroomComfortSequence, () => dialogueFinished = true));
        }
        else
        {
            dialogueFinished = true;
        }

        if (maxSeconds > 0f)
        {
            float elapsed = 0f;
            while (!BedroomTalkingWaitIsDone(waitFullLength, hasDialogue, dialogueFinished, elapsed, maxSeconds))
            {
                elapsed += GetReplayDeltaTime();
                yield return null;
            }
        }
        else if (dialogueRoutine != null)
        {
            yield return dialogueRoutine;
            dialogueRoutine = null;
        }

        if (dialogueRoutine != null && !dialogueFinished)
        {
            StopCoroutine(dialogueRoutine);
            if (subtitlePlayer != null)
            {
                subtitlePlayer.Hide();
            }
        }

        StopActorAndHold(bedroomWifeAnimation);
    }

    private static bool BedroomTalkingWaitIsDone(
        bool waitFullLength,
        bool hasDialogue,
        bool dialogueFinished,
        float elapsed,
        float targetSeconds)
    {
        bool timeFinished = targetSeconds <= 0f || elapsed >= targetSeconds;
        if (waitFullLength)
        {
            return timeFinished && (!hasDialogue || dialogueFinished);
        }

        if (hasDialogue)
        {
            return timeFinished || dialogueFinished;
        }

        return timeFinished;
    }

    private IEnumerator PlayDialogueAndMarkDone(HearthDialogueSequence sequence, System.Action onDone)
    {
        yield return PlayDialogue(sequence);
        onDone?.Invoke();
    }

    private float ResolveBedroomTalkingMaxSeconds(float animationSeconds)
    {
        if (bedroomTalkingMaxSeconds <= 0f)
        {
            return Mathf.Max(0f, animationSeconds);
        }

        if (animationSeconds <= 0f)
        {
            return bedroomTalkingMaxSeconds;
        }

        return Mathf.Min(animationSeconds, bedroomTalkingMaxSeconds);
    }

    private IEnumerator OpenDoorAfterDelay(float delaySeconds)
    {
        if (delaySeconds > 0f)
        {
            yield return WaitForReplaySeconds(delaySeconds);
        }

        if (wifeExitDoor != null)
        {
            wifeExitDoor.Open();
        }
    }

    private Transform ResolveBedroomWifeMoveRoot()
    {
        if (bedroomWifeMoveRoot != null)
        {
            return bedroomWifeMoveRoot;
        }

        if (bedroomWifeActor != null)
        {
            bedroomWifeMoveRoot = bedroomWifeActor.transform;
        }

        return bedroomWifeMoveRoot;
    }

    private IEnumerator MoveActorAlongPath(Transform actor, Transform[] path)
    {
        yield return MoveActorAlongPath(actor, path, 0, GetPathLength(path));
    }

    private IEnumerator MoveActorAlongPath(Transform actor, Transform[] path, int startIndex, int endIndex)
    {
        if (actor == null || path == null)
        {
            yield break;
        }

        int clampedStart = Mathf.Clamp(startIndex, 0, path.Length);
        int clampedEnd = Mathf.Clamp(endIndex, clampedStart, path.Length);
        for (int i = clampedStart; i < clampedEnd; i++)
        {
            if (path[i] != null)
            {
                yield return MoveActorToAnchor(actor, path[i]);
            }
        }
    }

    private int ResolveDoorOpenPathSplitIndex()
    {
        int pathLength = GetPathLength(wifeExitPathPoints);
        if (pathLength == 0)
        {
            return 0;
        }

        if (openDoorAfterPathPointCount < 0)
        {
            return pathLength;
        }

        return Mathf.Clamp(openDoorAfterPathPointCount, 0, pathLength);
    }

    private int ResolveDefaultSimpleRouteSplitIndex()
    {
        int pathLength = GetPathLength(wifeExitPathPoints);
        if (pathLength <= 0)
        {
            return 0;
        }

        if (pathLength >= 6)
        {
            return 4;
        }

        return Mathf.Max(0, pathLength - 1);
    }

    private static int GetPathLength(Transform[] path)
    {
        return path != null ? path.Length : 0;
    }

    private static bool HasPathPoints(Transform[] path)
    {
        if (path == null)
        {
            return false;
        }

        for (int i = 0; i < path.Length; i++)
        {
            if (path[i] != null)
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerator MoveActorToAnchor(Transform actor, Transform anchor)
    {
        if (actor == null || anchor == null)
        {
            yield break;
        }

        float lastDistance = Vector3.Distance(actor.position, anchor.position);
        float noProgressSeconds = 0f;
        while ((actor.position - anchor.position).sqrMagnitude > wifeAnchorSnapDistance * wifeAnchorSnapDistance)
        {
            float step = wifeWalkSpeed * GetReplayDeltaTime();
            actor.position = Vector3.MoveTowards(actor.position, anchor.position, step);
            RotateActorToward(actor, anchor.rotation);

            float currentDistance = Vector3.Distance(actor.position, anchor.position);
            if (lastDistance - currentDistance > wifeMoveProgressEpsilon)
            {
                noProgressSeconds = 0f;
                lastDistance = currentDistance;
            }
            else
            {
                noProgressSeconds += GetReplayDeltaTime();
                if (noProgressSeconds >= wifeMoveNoProgressSeconds)
                {
                    Debug.LogWarning(
                        "[HearthCompanion17F02ReplayController] Wife move made no progress toward " +
                        anchor.name + ". Snapping to the anchor so the 17F02 replay can continue.",
                        this);
                    actor.SetPositionAndRotation(anchor.position, anchor.rotation);
                    yield break;
                }
            }

            yield return null;
        }

        actor.SetPositionAndRotation(anchor.position, anchor.rotation);
    }

    private void RotateActorToward(Transform actor, Quaternion targetRotation)
    {
        if (actor == null)
        {
            return;
        }

        actor.rotation = Quaternion.RotateTowards(actor.rotation, targetRotation, wifeRotateSpeed * GetReplayDeltaTime());
    }

    private static void SetActiveIfDistinct(GameObject target, bool active, params GameObject[] possiblyShared)
    {
        if (target == null)
        {
            return;
        }

        if (!active && possiblyShared != null)
        {
            for (int i = 0; i < possiblyShared.Length; i++)
            {
                if (possiblyShared[i] == target)
                {
                    return;
                }
            }
        }

        target.SetActive(active);
    }

    private static void ApplyPose(HearthActorPosePreset preset, string poseId)
    {
        if (preset != null && !string.IsNullOrEmpty(poseId))
        {
            preset.ApplyPose(poseId);
        }
    }

    private static bool PlayActorLoop(HearthActorAnimatorDriver player, string clipId)
    {
        if (player == null || string.IsNullOrEmpty(clipId) || !player.HasState(clipId))
        {
            return false;
        }

        player.PlayLoop(clipId);
        return true;
    }

    private static bool PlayActorLoopOrPose(
        HearthActorAnimatorDriver player,
        string clipId,
        HearthActorPosePreset fallbackPose,
        string fallbackPoseId)
    {
        if (PlayActorLoop(player, clipId))
        {
            return true;
        }

        ApplyPose(fallbackPose, fallbackPoseId);
        return false;
    }

    private static float PlayActorOnce(HearthActorAnimatorDriver player, string clipId)
    {
        if (player == null || string.IsNullOrEmpty(clipId) || !player.HasState(clipId))
        {
            return 0f;
        }

        return player.PlayOnce(clipId);
    }

    private IEnumerator PlayActorOnceAndWait(HearthActorAnimatorDriver player, string clipId)
    {
        float seconds = PlayActorOnce(player, clipId);
        if (seconds > 0f)
        {
            yield return WaitForReplaySeconds(seconds);
        }
    }

    private static void StopActorAndHold(HearthActorAnimatorDriver player)
    {
        if (player != null)
        {
            player.StopAndHold();
            player.SetRootMotion(false);
        }
    }

    private static void FinalizeActorAtAnchor(
        Transform actor,
        Transform anchor,
        HearthActorAnimatorDriver player)
    {
        StopActorAndHold(player);
        if (player != null)
        {
            player.RestoreAnimatorTransformNow();
        }

        if (actor != null && anchor != null)
        {
            actor.SetPositionAndRotation(anchor.position, anchor.rotation);
        }
    }

    private void ConfigureBedroomWifeAnimationPolish()
    {
        if (bedroomWifeAnimation == null)
        {
            return;
        }

        bedroomWifeAnimation.SetMinimumTransitionSeconds(0.32f);
        bedroomWifeAnimation.SetAllStateStabilization(true);
        bedroomWifeAnimation.CaptureAnimatorTransformBaseline();
    }

    private void HandleHudHoldPromptConfirmed(string sceneId)
    {
        if (currentStep == ReplayStep.BedroomConfide &&
            string.Equals(sceneId, bedroomConfideSceneId, System.StringComparison.OrdinalIgnoreCase))
        {
            bedroomAcknowledged = true;
            if (companionHud != null)
            {
                companionHud.SetHoldPromptVisible(false);
            }
        }

        if (currentStep == ReplayStep.ForcedShutdown &&
            string.Equals(sceneId, forcedShutdownSceneId, System.StringComparison.OrdinalIgnoreCase))
        {
            shutdownConfirmed = true;
            if (companionHud != null)
            {
                companionHud.SetHoldPromptVisible(false);
            }
        }
    }

    private void StopActiveRoutine()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }
    }

    private void ResolveReferences()
    {
        if (!autoFindReferences)
        {
            return;
        }

        if (flowController == null)
        {
            flowController = FindObjectOfType<MinLoopFlowController>();
        }

        ViewSwitchController preferredViewSwitch =
            ViewSwitchController.FindPreferredController(gameObject.scene);
        if (preferredViewSwitch != null && viewSwitchController != preferredViewSwitch)
        {
            viewSwitchController = preferredViewSwitch;
        }

        if (companionHud == null)
        {
            companionHud = FindObjectOfType<HearthCompanionHudController>();
        }

        if (subtitlePlayer == null)
        {
            subtitlePlayer = FindObjectOfType<MinLoopSubtitlePlayer>();
        }

        if (robotRoot == null)
        {
            GameObject robotObject = GameObject.Find("Player/Robot Controller");
            if (robotObject == null)
            {
                robotObject = GameObject.Find("Robot Controller");
            }

            if (robotObject != null)
            {
                robotRoot = robotObject.transform;
            }
        }

        if (robotRoot != null)
        {
            if (robotCamera == null)
            {
                robotCamera = robotRoot.GetComponentInChildren<Camera>(true);
            }

            if (robotMovement == null)
            {
                robotMovement = robotRoot.GetComponent<FirstPersonMovement>();
            }

            if (robotLook == null)
            {
                robotLook = robotRoot.GetComponentInChildren<FirstPersonLook>(true);
            }

            if (robotInteraction == null)
            {
                robotInteraction = robotRoot.GetComponent<PlayerInteraction>();
            }

            if (robotRigidbody == null)
            {
                robotRigidbody = robotRoot.GetComponent<Rigidbody>();
            }
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

    private void EnsureBlackoutOverlay()
    {
        if (blackoutCanvasGroup != null)
        {
            if (blackoutImage != null)
            {
                blackoutImage.color = blackoutColor;
            }

            return;
        }

        if (!createBlackoutOverlay)
        {
            return;
        }

        GameObject overlay = new GameObject(
            "Hearth17F02ReplayBlackout",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(CanvasGroup),
            typeof(Image));
        overlay.transform.SetParent(transform, false);

        RectTransform rect = overlay.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Canvas canvas = overlay.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = blackoutSortingOrder;

        CanvasScaler scaler = overlay.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        blackoutCanvasGroup = overlay.GetComponent<CanvasGroup>();
        blackoutImage = overlay.GetComponent<Image>();
        blackoutImage.color = blackoutColor;
        blackoutImage.raycastTarget = false;
        SetBlackoutAlpha(0f);
    }

    private IEnumerator FadeBlackTo(float targetAlpha, float seconds)
    {
        EnsureBlackoutOverlay();
        if (blackoutCanvasGroup == null)
        {
            yield break;
        }

        float start = blackoutCanvasGroup.alpha;
        if (seconds <= 0f)
        {
            SetBlackoutAlpha(targetAlpha);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += blackoutUsesUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            SetBlackoutAlpha(Mathf.Lerp(start, targetAlpha, Mathf.Clamp01(elapsed / seconds)));
            yield return null;
        }

        SetBlackoutAlpha(targetAlpha);
    }

    private void SetBlackoutAlpha(float alpha)
    {
        if (blackoutCanvasGroup == null)
        {
            return;
        }

        blackoutCanvasGroup.alpha = Mathf.Clamp01(alpha);
        blackoutCanvasGroup.blocksRaycasts = blackoutCanvasGroup.alpha > 0.01f;
        blackoutCanvasGroup.interactable = false;
    }
}
