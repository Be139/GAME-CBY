using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class HearthCompanion17F01ReplayController : MonoBehaviour
{
    public enum ReplayStep
    {
        Inactive,
        BedroomMonitor,
        LookAtBoyPrompt,
        BedsideSoothing,
        LivingRoomObservation,
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
    [SerializeField] private Transform childRoomStartAnchor;
    [SerializeField] private Transform bedsideInteractAnchor;
    [SerializeField] private Transform livingRoomStartAnchor;
    [SerializeField] private Transform[] bedsidePathPoints;

    [Header("Interaction")]
    [SerializeField] private HearthCompanionReplayInteractable approachBoyInteractable;
    [SerializeField] private float promptRefreshSeconds = 0.05f;
    [SerializeField] private float promptDelayAfterBedroomPrelude = 1.5f;

    [Header("Movement")]
    [SerializeField] private float autoMoveSpeed = 1.15f;
    [SerializeField] private float autoRotateSpeed = 360f;
    [SerializeField] private float anchorSnapDistance = 0.03f;

    [Header("HUD Scene Ids")]
    [SerializeField] private string bedroomSceneId = "17F01_01";
    [SerializeField] private string bedsideSceneId = "17F01_02";
    [SerializeField] private string livingRoomSceneId = "17F01_03";

    [Header("Actors")]
    [SerializeField] private HearthActorPosePreset boyPosePreset;
    [SerializeField] private HearthActorPosePreset motherPosePreset;
    [SerializeField] private HearthActorPosePreset fatherPosePreset;
    [SerializeField] private string boySleepPoseId = "Sleep";
    [SerializeField] private string boyAwakePoseId = "Awake";
    [SerializeField] private string boyComfortedPoseId = "Comforted";
    [SerializeField] private string parentSittingPoseId = "Sitting";

    [Header("Dialogue Assets")]
    [SerializeField] private bool preferDialogueSequenceAssets = true;
    [SerializeField] private HearthDialogueSequence bedroomPreludeSequence;
    [SerializeField] private HearthDialogueSequence soothingSequence;
    [SerializeField] private HearthDialogueSequence livingRoomSequence;

    [Header("Fallback Subtitle Lines")]
    [SerializeField] private bool seedDefaultLinesIfEmpty = true;
    [SerializeField] private List<MinLoopSubtitleLine> bedroomPreludeLines = new List<MinLoopSubtitleLine>();
    [SerializeField] private List<MinLoopSubtitleLine> soothingLines = new List<MinLoopSubtitleLine>();
    [SerializeField] private List<MinLoopSubtitleLine> livingRoomLines = new List<MinLoopSubtitleLine>();

    [Header("Timing")]
    [SerializeField] private float bedroomPreludeDelay = 0.8f;
    [SerializeField] private float waitAfterSoothingLines = 0.5f;
    [SerializeField] private float waitBeforeLivingRoomLines = 0.6f;

    [Header("Runtime")]
    [SerializeField] private ReplayStep currentStep = ReplayStep.Inactive;

    private Coroutine activeRoutine;
    private Coroutine bedroomPreludeRoutine;
    private float nextPromptRefreshTime;
    private bool listeningHud;

    public ReplayStep CurrentStep
    {
        get { return currentStep; }
    }

    private void Awake()
    {
        ResolveReferences();
        if (seedDefaultLinesIfEmpty)
        {
            SeedDefaultLinesIfNeeded();
        }
    }

    private void OnEnable()
    {
        SubscribeHud();
    }

    private void OnDisable()
    {
        UnsubscribeHud();
    }

    private void Update()
    {
        if (currentStep != ReplayStep.LookAtBoyPrompt)
        {
            return;
        }

        if (Time.unscaledTime < nextPromptRefreshTime)
        {
            return;
        }

        nextPromptRefreshTime = Time.unscaledTime + Mathf.Max(0.01f, promptRefreshSeconds);
        RefreshApproachPrompt();
    }

    private void OnValidate()
    {
        promptRefreshSeconds = Mathf.Max(0.01f, promptRefreshSeconds);
        promptDelayAfterBedroomPrelude = Mathf.Max(0f, promptDelayAfterBedroomPrelude);
        autoMoveSpeed = Mathf.Max(0.05f, autoMoveSpeed);
        autoRotateSpeed = Mathf.Max(1f, autoRotateSpeed);
        anchorSnapDistance = Mathf.Max(0.001f, anchorSnapDistance);
        bedroomPreludeDelay = Mathf.Max(0f, bedroomPreludeDelay);
        waitAfterSoothingLines = Mathf.Max(0f, waitAfterSoothingLines);
        waitBeforeLivingRoomLines = Mathf.Max(0f, waitBeforeLivingRoomLines);
    }

    public void BeginReplay()
    {
        BeginReplay(flowController);
    }

    public void BeginReplay(MinLoopFlowController owner)
    {
        ResolveReferences();
        if (owner != null)
        {
            flowController = owner;
        }

        StopActiveRoutine();
        StopBedroomPrelude();
        SubscribeHud();

        SetStep(ReplayStep.BedroomMonitor);
        TeleportRobot(childRoomStartAnchor);
        SetRobotControl(true, true, false);
        ApplyPose(boyPosePreset, boySleepPoseId);
        ApplyPose(motherPosePreset, parentSittingPoseId);
        ApplyPose(fatherPosePreset, parentSittingPoseId);

        if (approachBoyInteractable != null)
        {
            approachBoyInteractable.SetAvailable(false);
        }

        if (companionHud != null)
        {
            companionHud.SetAutoAdvanceOnHoldPrompt(false);
            companionHud.SetVisible(true);
            companionHud.ShowScene(bedroomSceneId);
            companionHud.SetHoldPromptVisible(false);
        }

        bedroomPreludeRoutine = StartCoroutine(BedroomPreludeRoutine());
    }

    public void CompleteCurrentStep()
    {
        switch (currentStep)
        {
            case ReplayStep.LookAtBoyPrompt:
                StopBedroomPrelude();
                activeRoutine = StartCoroutine(BeginBedsideSoothingInPlaceRoutine());
                break;
            case ReplayStep.BedsideSoothing:
                activeRoutine = StartCoroutine(SoothingRoutine());
                break;
            case ReplayStep.LivingRoomObservation:
                ReturnToTerminalForDisposition();
                break;
        }
    }

    public void ReturnToTerminalForDisposition()
    {
        ReturnToTerminalForDisposition(true);
    }

    public void CancelReplay()
    {
        StopActiveRoutine();
        StopBedroomPrelude();
        SetStep(ReplayStep.Inactive);

        if (approachBoyInteractable != null)
        {
            approachBoyInteractable.SetAvailable(false);
        }

        if (companionHud != null)
        {
            companionHud.SetHoldPromptVisible(false);
        }

        if (subtitlePlayer != null)
        {
            subtitlePlayer.Hide();
        }
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

    public void SetAnchors(Transform childStart, Transform bedside, Transform livingRoom)
    {
        childRoomStartAnchor = childStart;
        bedsideInteractAnchor = bedside;
        livingRoomStartAnchor = livingRoom;
    }

    public void SetApproachInteractable(HearthCompanionReplayInteractable interactable)
    {
        approachBoyInteractable = interactable;
    }

    private IEnumerator BedroomPreludeRoutine()
    {
        if (bedroomPreludeDelay > 0f)
        {
            yield return new WaitForSeconds(bedroomPreludeDelay);
        }

        if (subtitlePlayer != null)
        {
            yield return PlayDialogue(bedroomPreludeSequence, bedroomPreludeLines);
        }

        ApplyPose(boyPosePreset, boyAwakePoseId);

        if (promptDelayAfterBedroomPrelude > 0f)
        {
            yield return new WaitForSeconds(promptDelayAfterBedroomPrelude);
        }

        if (approachBoyInteractable != null)
        {
            approachBoyInteractable.SetAvailable(true);
        }

        if (flowController != null)
        {
            flowController.NotifyReplayComfortReady();
        }

        SetStep(ReplayStep.LookAtBoyPrompt);
        RefreshApproachPrompt();
        bedroomPreludeRoutine = null;
    }

    private IEnumerator BeginBedsideSoothingInPlaceRoutine()
    {
        SetStep(ReplayStep.BedsideSoothing);
        SetRobotControl(false, true, false);

        if (approachBoyInteractable != null)
        {
            approachBoyInteractable.SetAvailable(false);
        }

        if (companionHud != null)
        {
            companionHud.SetHoldPromptVisible(false);
        }

        if (flowController != null)
        {
            flowController.NotifyComfortActionPerformed();
        }

        if (companionHud != null)
        {
            companionHud.ShowScene(bedsideSceneId);
            companionHud.ResetHoldPrompt();
            companionHud.SetHoldPromptVisible(true);
        }

        activeRoutine = null;
        yield break;
    }

    private IEnumerator SoothingRoutine()
    {
        if (companionHud != null)
        {
            companionHud.SetHoldPromptVisible(false);
        }

        if (subtitlePlayer != null)
        {
            yield return PlayDialogue(soothingSequence, soothingLines);
        }

        ApplyPose(boyPosePreset, boyComfortedPoseId);

        if (waitAfterSoothingLines > 0f)
        {
            yield return new WaitForSeconds(waitAfterSoothingLines);
        }

        yield return LivingRoomObservationRoutine();
        activeRoutine = null;
    }

    private IEnumerator LivingRoomObservationRoutine()
    {
        SetStep(ReplayStep.LivingRoomObservation);
        if (flowController != null)
        {
            flowController.NotifyMorningReviewStarted();
        }

        TeleportRobot(livingRoomStartAnchor);
        SetRobotControl(true, true, false);

        if (companionHud != null)
        {
            companionHud.ShowScene(livingRoomSceneId);
            companionHud.SetHoldPromptVisible(false);
        }

        if (waitBeforeLivingRoomLines > 0f)
        {
            yield return new WaitForSeconds(waitBeforeLivingRoomLines);
        }

        if (subtitlePlayer != null)
        {
            yield return PlayDialogue(livingRoomSequence, livingRoomLines);
        }

        ReturnToTerminalForDisposition(false);
        activeRoutine = null;
    }

    private IEnumerator MoveRobotTo(Transform target)
    {
        if (robotRoot == null || target == null)
        {
            yield break;
        }

        while ((robotRoot.position - target.position).sqrMagnitude > anchorSnapDistance * anchorSnapDistance)
        {
            float step = autoMoveSpeed * Time.deltaTime;
            robotRoot.position = Vector3.MoveTowards(robotRoot.position, target.position, step);
            RotateRobotToward(target.rotation);
            ClearRobotVelocity();
            yield return null;
        }
    }

    private void RefreshApproachPrompt()
    {
        bool canInteract = approachBoyInteractable != null &&
                           approachBoyInteractable.CanInteract(robotRoot, robotCamera);

        if (companionHud != null)
        {
            companionHud.SetHoldPromptVisible(canInteract);
        }
    }

    private IEnumerator PlayDialogue(HearthDialogueSequence sequence, IList<MinLoopSubtitleLine> fallbackLines)
    {
        if (subtitlePlayer == null)
        {
            yield break;
        }

        if (preferDialogueSequenceAssets && sequence != null && sequence.HasLines)
        {
            yield return subtitlePlayer.PlaySequenceAsset(sequence);
            yield break;
        }

        if (fallbackLines != null && fallbackLines.Count > 0)
        {
            yield return subtitlePlayer.PlayLines(fallbackLines);
        }
    }

    private void ReturnToTerminalForDisposition(bool stopRoutines)
    {
        if (stopRoutines)
        {
            StopActiveRoutine();
            StopBedroomPrelude();
        }

        SetStep(ReplayStep.ReturningToTerminal);
        SetRobotControl(false, false, false);

        if (approachBoyInteractable != null)
        {
            approachBoyInteractable.SetAvailable(false);
        }

        if (companionHud != null)
        {
            companionHud.SetHoldPromptVisible(false);
        }

        if (flowController != null)
        {
            flowController.NotifyReplayCompleted();
        }
    }

    private void HandleHudHoldPromptConfirmed(string sceneId)
    {
        if (currentStep == ReplayStep.LookAtBoyPrompt && string.Equals(sceneId, bedroomSceneId, System.StringComparison.OrdinalIgnoreCase))
        {
            CompleteCurrentStep();
            return;
        }

        if (currentStep == ReplayStep.BedsideSoothing && string.Equals(sceneId, bedsideSceneId, System.StringComparison.OrdinalIgnoreCase))
        {
            CompleteCurrentStep();
        }
    }

    private void SetStep(ReplayStep step)
    {
        currentStep = step;
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

    private void RotateRobotToward(Quaternion targetRotation)
    {
        if (robotRoot == null)
        {
            return;
        }

        robotRoot.rotation = Quaternion.RotateTowards(robotRoot.rotation, targetRotation, autoRotateSpeed * Time.deltaTime);
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

        robotRigidbody.velocity = Vector3.zero;
        robotRigidbody.angularVelocity = Vector3.zero;
    }

    private static void ApplyPose(HearthActorPosePreset preset, string poseId)
    {
        if (preset != null && !string.IsNullOrEmpty(poseId))
        {
            preset.ApplyPose(poseId);
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

    private void StopBedroomPrelude()
    {
        if (bedroomPreludeRoutine != null)
        {
            StopCoroutine(bedroomPreludeRoutine);
            bedroomPreludeRoutine = null;
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

        if (viewSwitchController != null && robotRoot == null)
        {
            GameObject robotObject = GameObject.Find("Robot Controller");
            if (robotObject == null)
            {
                robotObject = GameObject.Find("Player/Robot Controller");
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

    private void SeedDefaultLinesIfNeeded()
    {
        if (bedroomPreludeLines.Count == 0)
        {
            AddLine(bedroomPreludeLines, "Son", "... No...", 1.8f, 1.2f);
            AddLine(bedroomPreludeLines, "Son", "... Mom...", 2.2f, 1.5f);
            AddLine(bedroomPreludeLines, "Synth Voice", "Decision: initiate soothing protocol. Reason: service subject showing signs of nightmare.", 0.4f, 3.0f);
        }

        if (soothingLines.Count == 0)
        {
            AddLine(soothingLines, "Companion Unit", "Was it a nightmare? Come on, with me, slowly. Deep breath. One, two...", 0f, 3.5f);
            AddLine(soothingLines, "Companion Unit", "Mom and Dad should be asleep. If you go knock at this hour, she'll be very tired tomorrow.", 0.2f, 4.0f);
            AddLine(soothingLines, "Companion Unit", "Let's calm down like this first. When you feel a little better, if you still want to go, you can go then. Okay?", 0.2f, 4.4f);
            AddLine(soothingLines, "Companion Unit", "Two more deep breaths. That's it. Good. Let's lie back down. I'll stay with you until you fall asleep.", 0.2f, 4.2f);
            AddLine(soothingLines, "Synth Voice", "Event archived.", 0.6f, 1.8f);
        }

        if (livingRoomLines.Count == 0)
        {
            AddLine(livingRoomLines, "Father", "He had a nightmare last night?", 0.2f, 2.2f);
            AddLine(livingRoomLines, "Mother", "... He didn't come out.", 0.4f, 2.2f);
            AddLine(livingRoomLines, "Father", "Mm.", 0.2f, 1.2f);
            AddLine(livingRoomLines, "Mother", "Then it handled it well.", 1.2f, 2.2f);
            AddLine(livingRoomLines, "Father", "Mm.", 0.2f, 1.2f);
            AddLine(livingRoomLines, "Mother", "... But it feels strange.", 0.6f, 2.4f);
            AddLine(livingRoomLines, "Mother", "Recently, I haven't heard him knock at all. I'm actually a little unused to it.", 0.2f, 4.0f);
            AddLine(livingRoomLines, "Father", "Isn't that a good thing? He's grown up. And it saves us from getting up in the middle of the night.", 0.4f, 4.2f);
            AddLine(livingRoomLines, "Mother", "But the child hasn't told us about his nightmares for a long time.", 0.3f, 3.6f);
            AddLine(livingRoomLines, "Father", "At least we don't have to worry about him at night anymore, right?", 0.6f, 3.2f);
            AddLine(livingRoomLines, "Mother", "... Mm.", 0.8f, 1.6f);
        }
    }

    private static void AddLine(List<MinLoopSubtitleLine> target, string speaker, string text, float startDelay, float holdSeconds)
    {
        MinLoopSubtitleLine line = new MinLoopSubtitleLine();
        line.speaker = speaker;
        line.text = text;
        line.startDelay = startDelay;
        line.holdSeconds = holdSeconds;
        target.Add(line);
    }
}
