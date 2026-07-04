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
    [SerializeField] private Transform livingRoomTerminalAnchor;

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
        EnsureBlackoutOverlay();
    }

    private void OnEnable()
    {
        SubscribeHud();
    }

    private void OnDisable()
    {
        UnsubscribeHud();
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
        wifeDoorPauseSeconds = Mathf.Max(0f, wifeDoorPauseSeconds);
        waitAfterDoorOpenSeconds = Mathf.Max(0f, waitAfterDoorOpenSeconds);
    }

    public void BeginReplay()
    {
        BeginReplay(flowController);
    }

    public void PrepareReplayStart()
    {
        ResolveReferences();
        EnsureBlackoutOverlay();
        StopActiveRoutine();
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
        ApplyPose(bedroomWifePose, bedroomWifePoseId);
        TeleportRobot(bedroomStartAnchor);
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
        bedroomStartAnchor = bedroomAnchor;
        livingRoomTerminalAnchor = livingTerminalAnchor;
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
        ApplyPose(bedroomWifePose, bedroomWifePoseId);
        TeleportRobot(bedroomStartAnchor);
        SetRobotControl(false, false, false);

        currentStep = ReplayStep.BedroomWake;
        onBedroomWakeStarted.Invoke();
        SetBlackoutAlpha(1f);

        if (initialBlackSeconds > 0f)
        {
            yield return new WaitForSeconds(initialBlackSeconds);
        }

        yield return PlayDialogue(bedroomWakeSequence);
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
                yield return new WaitForSeconds(bedroomPromptDelayAfterConfideSeconds);
            }

            if (companionHud != null)
            {
                companionHud.ResetHoldPrompt();
                companionHud.SetHoldPromptVisible(true);
            }

            while (!bedroomAcknowledged)
            {
                yield return null;
            }

            yield return PlayDialogue(bedroomComfortSequence);
        }

        if (companionHud != null)
        {
            companionHud.SetHoldPromptVisible(false);
        }

        if (waitAfterConfideSeconds > 0f)
        {
            yield return new WaitForSeconds(waitAfterConfideSeconds);
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
            yield return new WaitForSeconds(wifeExitLockedSeconds);
        }

        onWifeExitFinished.Invoke();

        currentStep = ReplayStep.DiningObservation;
        onDiningObservationStarted.Invoke();
        SetStageActors(false, true, false);
        ApplyPose(diningWifePose, diningSittingPoseId);
        ApplyPose(diningHusbandPose, diningSittingPoseId);
        ShowHudScene(diningObservationSceneId, false);
        SetRobotControl(true, true, false);

        if (flowController != null)
        {
            flowController.NotifyMorningReviewStarted();
        }

        if (waitBeforeDiningDialogueSeconds > 0f)
        {
            yield return new WaitForSeconds(waitBeforeDiningDialogueSeconds);
        }

        yield return PlayDialogue(diningObservationSequence);

        if (postDiningSilenceSeconds > 0f)
        {
            yield return new WaitForSeconds(postDiningSilenceSeconds);
        }

        yield return FadeBlackTo(1f, livingRoomFadeOutSeconds);

        if (livingRoomBlackHoldSeconds > 0f)
        {
            yield return new WaitForSeconds(livingRoomBlackHoldSeconds);
        }

        currentStep = ReplayStep.LivingRoomTerminal;
        onLivingRoomTerminalStarted.Invoke();
        SetStageActors(false, false, true);
        ApplyPose(terminalHusbandPose, terminalHusbandPoseId);
        TeleportRobot(livingRoomTerminalAnchor);
        ShowHudScene(logAccessSceneId, false);
        SetRobotControl(false, false, false);
        yield return FadeBlackTo(0f, livingRoomFadeInSeconds);
        yield return PlayDialogue(logAccessSequence);

        if (waitBeforeForcedShutdownSeconds > 0f)
        {
            yield return new WaitForSeconds(waitBeforeForcedShutdownSeconds);
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
            companionHud.PlayShutdownGlitch();
        }

        if (shutdownEffectSeconds > 0f)
        {
            yield return new WaitForSeconds(shutdownEffectSeconds);
        }

        currentStep = ReplayStep.BlackAudio;
        ShowHudScene(blackAudioSceneId, false);
        yield return PlayDialogue(blackAudioSequence);

        if (waitBeforeReturnSeconds > 0f)
        {
            yield return new WaitForSeconds(waitBeforeReturnSeconds);
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
        if (robotRoot == null || anchor == null)
        {
            return;
        }

        robotRoot.SetPositionAndRotation(anchor.position, anchor.rotation);

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

        if (wifeDoorPauseSeconds > 0f)
        {
            yield return new WaitForSeconds(wifeDoorPauseSeconds);
        }

        if (openDoorDuringWifeExit && wifeExitDoor != null)
        {
            wifeExitDoor.Open();
            while (wifeExitDoor.IsMoving)
            {
                yield return null;
            }
        }

        if (waitAfterDoorOpenSeconds > 0f)
        {
            yield return new WaitForSeconds(waitAfterDoorOpenSeconds);
        }

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
    }

    private IEnumerator MoveLegacyWifeExitRoute(Transform moveRoot)
    {
        int splitIndex = ResolveDoorOpenPathSplitIndex();
        yield return MoveActorAlongPath(moveRoot, wifeExitPathPoints, 0, splitIndex);
        yield return MoveActorToAnchor(moveRoot, wifeDoorPauseAnchor);

        if (wifeDoorPauseSeconds > 0f)
        {
            yield return new WaitForSeconds(wifeDoorPauseSeconds);
        }

        if (openDoorDuringWifeExit && wifeExitDoor != null)
        {
            wifeExitDoor.Open();
            while (wifeExitDoor.IsMoving)
            {
                yield return null;
            }
        }

        if (waitAfterDoorOpenSeconds > 0f)
        {
            yield return new WaitForSeconds(waitAfterDoorOpenSeconds);
        }

        yield return MoveActorAlongPath(moveRoot, wifeExitPathPoints, splitIndex, GetPathLength(wifeExitPathPoints));
        yield return MoveActorToAnchor(moveRoot, wifeExitOutsideAnchor);
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

        while ((actor.position - anchor.position).sqrMagnitude > wifeAnchorSnapDistance * wifeAnchorSnapDistance)
        {
            float step = wifeWalkSpeed * Time.deltaTime;
            actor.position = Vector3.MoveTowards(actor.position, anchor.position, step);
            RotateActorToward(actor, anchor.rotation);
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

        actor.rotation = Quaternion.RotateTowards(actor.rotation, targetRotation, wifeRotateSpeed * Time.deltaTime);
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

        if (viewSwitchController == null)
        {
            viewSwitchController = FindObjectOfType<ViewSwitchController>();
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
