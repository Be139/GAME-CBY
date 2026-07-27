using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public enum HearthLobbyFlowStage
{
    Inactive,
    Opening,
    FreeExploration,
    OptionalConversation,
    AssignmentTerminal,
    ElevatorTransition,
    ElevatorRide,
    Floor17Transition,
    ArrivedFloor17
}

[DisallowMultipleComponent]
public class HearthLobbyFlowController : MonoBehaviour
{
    [Header("Startup")]
    [SerializeField] private bool autoStart = true;
    [SerializeField] private bool resetOptionalConversationsOnStart = true;

    [Header("Formal Player Only")]
    [SerializeField] private Transform humanRoot;
    [SerializeField] private Camera humanCamera;
    [SerializeField] private FirstPersonMovement humanMovement;
    [SerializeField] private FirstPersonLook humanLook;
    [SerializeField] private PlayerInteraction humanInteraction;
    [SerializeField] private Rigidbody humanRigidbody;
    [SerializeField] private Behaviour[] auxiliaryInputBehaviours;

    [Header("Pose References")]
    [SerializeField] private Transform lobbyStartAnchor;
    [SerializeField] private Transform lobbyStartCameraAnchor;
    [SerializeField] private Transform elevatorAnchor;
    [SerializeField] private Transform elevatorCameraAnchor;
    [SerializeField] private Transform floor17ArrivalAnchor;
    [SerializeField] private Transform floor17ArrivalCameraAnchor;

    [Header("Presentation")]
    [SerializeField] private MinLoopSubtitlePlayer subtitlePlayer;
    [SerializeField] private HearthScreenFader screenFader;
    [SerializeField] private HearthLobbyHudOverlay hudOverlay;
    [SerializeField] private HearthLocationProbe locationProbe;
    [SerializeField] private HearthTvTerminalController assignmentTerminal;
    [SerializeField] private HearthLobbyConversationZone[] optionalConversationZones;
    [Tooltip("Screen-space HUD canvases hidden only while the lobby assignment terminal owns the camera.")]
    [SerializeField] private Canvas[] hudCanvasesHiddenDuringTerminal;

    [Header("Dialogue")]
    [SerializeField] private HearthDialogueSequence openingBriefingDialogue;
    [SerializeField] private HearthDialogueSequence lilyVoiceMessageDialogue;
    [SerializeField] private HearthDialogueSequence openingCloseoutDialogue;
    [SerializeField] private HearthDialogueSequence assignmentLoadedDialogue;
    [SerializeField] private HearthDialogueSequence elevatorDialogue;
    [SerializeField] private HearthDialogueSequence floor17ArrivalDialogue;

    [Header("Sound Effects")]
    [SerializeField] private HearthSfxCuePlayer sfxCuePlayer;

    [Header("Timing")]
    [SerializeField] private float startupFadeSeconds = 0.35f;
    [SerializeField] private float transitionFadeOutSeconds = 0.5f;
    [SerializeField] private float transitionFadeInSeconds = 0.5f;
    [SerializeField] private float activationLeadSeconds = 0.45f;
    [SerializeField] private float assignmentTerminalMinimumViewSeconds = 5f;

    [Header("Events")]
    [SerializeField] private UnityEvent onOpeningCompleted = new UnityEvent();
    [SerializeField] private UnityEvent onAssignmentLoaded = new UnityEvent();
    [SerializeField] private UnityEvent onElevatorEntered = new UnityEvent();
    [SerializeField] private UnityEvent onFloor17Arrived = new UnityEvent();

    [Header("Runtime State")]
    [SerializeField] private HearthLobbyFlowStage currentStage = HearthLobbyFlowStage.Inactive;
    [SerializeField] private bool assignmentLoaded;

    private Coroutine activeRoutine;
    private bool busy;
    private bool desiredMove;
    private bool desiredLook;
    private bool desiredInteract;
    private bool desiredControlStateActive;
    private bool[] auxiliaryEnabledBeforeLock;
    private bool auxiliaryLocked;
    private Vector3 cachedCameraLocalPosition;
    private Transform cachedCameraParent;
    private bool hasCachedCameraPivot;
    private bool terminalHudSuppressed;
    private bool[] terminalHudCanvasWasEnabled;
    private bool assignmentTerminalCloseAvailable;
    private bool assignmentTerminalCloseRequested;

    public HearthLobbyFlowStage CurrentStage
    {
        get { return currentStage; }
    }

    public bool AssignmentLoaded
    {
        get { return assignmentLoaded; }
    }

    public bool CanOpenAssignmentTerminal
    {
        get
        {
            return !assignmentLoaded &&
                   !busy &&
                   (currentStage == HearthLobbyFlowStage.FreeExploration ||
                    currentStage == HearthLobbyFlowStage.AssignmentTerminal) &&
                   (assignmentTerminal == null || !assignmentTerminal.IsOpen);
        }
    }

    public bool CanUseElevator
    {
        get
        {
            return assignmentLoaded &&
                   !busy &&
                   currentStage == HearthLobbyFlowStage.FreeExploration &&
                   (assignmentTerminal == null || !assignmentTerminal.IsOpen);
        }
    }

    private void Awake()
    {
        ResolveReferences();
        CacheHumanCameraPivot();
        if (!autoStart)
        {
            return;
        }

        busy = true;
        currentStage = HearthLobbyFlowStage.Opening;
        if (screenFader != null)
        {
            screenFader.SetImmediate(1f);
        }

        if (hudOverlay != null)
        {
            hudOverlay.HideAllImmediate();
        }

        TeleportHuman(lobbyStartAnchor, lobbyStartCameraAnchor);
        SetHumanControl(false, true, false, true);
    }

    private void Start()
    {
        if (autoStart)
        {
            BeginOpening();
        }
    }

    private void LateUpdate()
    {
        RefreshTerminalHudVisibility();

        if (desiredControlStateActive)
        {
            ApplyDesiredControlState();
        }
    }

    private void OnDisable()
    {
        if (sfxCuePlayer != null)
        {
            sfxCuePlayer.StopAllCues();
        }

        RestoreTerminalHudVisibility();
        desiredControlStateActive = false;
        RestoreAuxiliaryInputs();
    }

    private void RefreshTerminalHudVisibility()
    {
        bool shouldSuppress = assignmentTerminal != null && assignmentTerminal.IsOpen;
        if (shouldSuppress == terminalHudSuppressed)
        {
            return;
        }

        if (shouldSuppress)
        {
            terminalHudSuppressed = true;
            terminalHudCanvasWasEnabled = hudCanvasesHiddenDuringTerminal != null
                ? new bool[hudCanvasesHiddenDuringTerminal.Length]
                : new bool[0];

            for (int i = 0; i < terminalHudCanvasWasEnabled.Length; i++)
            {
                Canvas canvas = hudCanvasesHiddenDuringTerminal[i];
                terminalHudCanvasWasEnabled[i] = canvas != null && canvas.enabled;
                if (canvas != null)
                {
                    canvas.enabled = false;
                }
            }

            return;
        }

        RestoreTerminalHudVisibility();
    }

    private void RestoreTerminalHudVisibility()
    {
        if (!terminalHudSuppressed)
        {
            return;
        }

        if (hudCanvasesHiddenDuringTerminal != null && terminalHudCanvasWasEnabled != null)
        {
            int count = Mathf.Min(hudCanvasesHiddenDuringTerminal.Length, terminalHudCanvasWasEnabled.Length);
            for (int i = 0; i < count; i++)
            {
                if (hudCanvasesHiddenDuringTerminal[i] != null)
                {
                    hudCanvasesHiddenDuringTerminal[i].enabled = terminalHudCanvasWasEnabled[i];
                }
            }
        }

        terminalHudSuppressed = false;
        terminalHudCanvasWasEnabled = null;
    }

    private void OnValidate()
    {
        startupFadeSeconds = Mathf.Max(0f, startupFadeSeconds);
        transitionFadeOutSeconds = Mathf.Max(0f, transitionFadeOutSeconds);
        transitionFadeInSeconds = Mathf.Max(0f, transitionFadeInSeconds);
        activationLeadSeconds = Mathf.Max(0f, activationLeadSeconds);
        assignmentTerminalMinimumViewSeconds = Mathf.Max(0f, assignmentTerminalMinimumViewSeconds);
    }

    public void BeginOpening()
    {
        StartFlowRoutine(OpeningRoutine());
    }

    public void SetSfxCuePlayer(HearthSfxCuePlayer player)
    {
        sfxCuePlayer = player;
    }

    public bool TryPlayOptionalConversation(
        HearthLobbyConversationZone zone,
        HearthDialogueSequence sequence)
    {
        if (zone == null || sequence == null || busy || currentStage != HearthLobbyFlowStage.FreeExploration)
        {
            return false;
        }

        if (assignmentTerminal != null && assignmentTerminal.IsOpen)
        {
            return false;
        }

        StartFlowRoutine(OptionalConversationRoutine(zone, sequence));
        return true;
    }

    public bool TryPlayExitCommentary(
        HearthLobbyConversationZone zone,
        HearthDialogueSequence sequence)
    {
        if (zone == null || sequence == null || busy || currentStage != HearthLobbyFlowStage.FreeExploration)
        {
            return false;
        }

        if (assignmentTerminal != null && assignmentTerminal.IsOpen)
        {
            return false;
        }

        StartFlowRoutine(ExitCommentaryRoutine(zone, sequence));
        return true;
    }

    public void AcquireAssignmentFromTerminal()
    {
        if (currentStage == HearthLobbyFlowStage.AssignmentTerminal)
        {
            ConfirmAssignmentTerminalClose();
            return;
        }

        BeginAssignmentBriefingFromTerminal();
    }

    public void BeginAssignmentBriefingFromTerminal()
    {
        if (assignmentLoaded || busy || assignmentTerminal == null || !assignmentTerminal.IsOpen)
        {
            return;
        }

        StartFlowRoutine(AssignmentLoadedRoutine());
    }

    public void ConfirmAssignmentTerminalClose()
    {
        if (currentStage != HearthLobbyFlowStage.AssignmentTerminal ||
            !assignmentTerminalCloseAvailable ||
            assignmentTerminalCloseRequested)
        {
            return;
        }

        assignmentTerminalCloseRequested = true;
        assignmentTerminalCloseAvailable = false;
        if (assignmentTerminal != null)
        {
            assignmentTerminal.SetPrimaryActionInputEnabled(false);
            assignmentTerminal.SetRuntimePrompt("PLEASE WAIT");
            assignmentTerminal.CloseTerminal();
        }
    }

    public void BeginElevatorRide()
    {
        if (!CanUseElevator)
        {
            return;
        }

        StartFlowRoutine(ElevatorRideRoutine());
    }

    public void ResetLobbyFlowForPreview()
    {
        StopActiveRoutine();
        assignmentLoaded = false;
        assignmentTerminalCloseAvailable = false;
        assignmentTerminalCloseRequested = false;
        busy = false;
        currentStage = HearthLobbyFlowStage.Inactive;

        if (optionalConversationZones != null)
        {
            for (int i = 0; i < optionalConversationZones.Length; i++)
            {
                if (optionalConversationZones[i] != null)
                {
                    optionalConversationZones[i].ResetConversation();
                }
            }
        }

        if (hudOverlay != null)
        {
            hudOverlay.HideAllImmediate();
        }

        if (assignmentTerminal != null)
        {
            assignmentTerminal.SetPrimaryActionInputEnabled(true);
            assignmentTerminal.SetCloseInputEnabled(true);
            assignmentTerminal.ClearRuntimePrompt();
        }

        BeginOpening();
    }

    private IEnumerator OpeningRoutine()
    {
        busy = true;
        currentStage = HearthLobbyFlowStage.Opening;
        assignmentLoaded = false;
        StartSfxLoop("Lobby.RoomTone");
        SetHumanControl(false, true, false, true);
        TeleportHuman(lobbyStartAnchor, lobbyStartCameraAnchor);

        if (resetOptionalConversationsOnStart && optionalConversationZones != null)
        {
            for (int i = 0; i < optionalConversationZones.Length; i++)
            {
                if (optionalConversationZones[i] != null)
                {
                    optionalConversationZones[i].ResetConversation();
                }
            }
        }

        if (hudOverlay != null)
        {
            hudOverlay.HideAllImmediate();
            hudOverlay.ShowActivation();
        }

        if (screenFader != null)
        {
            yield return screenFader.FadeIn(startupFadeSeconds);
        }

        if (activationLeadSeconds > 0f)
        {
            yield return new WaitForSecondsRealtime(activationLeadSeconds);
        }

        yield return PlayDialogue(openingBriefingDialogue);

        if (hudOverlay != null)
        {
            hudOverlay.HideActivation();
            hudOverlay.ShowExpandedVoiceMessage();
        }

        yield return PlayDialogue(lilyVoiceMessageDialogue);

        if (hudOverlay != null)
        {
            hudOverlay.CollapseVoiceMessage();
        }

        yield return PlayDialogue(openingCloseoutDialogue);

        if (hudOverlay != null)
        {
            hudOverlay.DismissVoiceMessage();
        }

        busy = false;
        currentStage = HearthLobbyFlowStage.FreeExploration;
        SetHumanControl(true, true, true, false);
        if (onOpeningCompleted != null)
        {
            onOpeningCompleted.Invoke();
        }

        activeRoutine = null;
    }

    private IEnumerator OptionalConversationRoutine(
        HearthLobbyConversationZone zone,
        HearthDialogueSequence sequence)
    {
        busy = true;
        currentStage = HearthLobbyFlowStage.OptionalConversation;
        SetHumanControl(false, true, false, true);

        yield return PlayDialogue(sequence);

        if (zone != null)
        {
            zone.MarkExchangeCompleted();
        }

        busy = false;
        currentStage = HearthLobbyFlowStage.FreeExploration;
        SetHumanControl(true, true, true, false);
        activeRoutine = null;
    }

    private IEnumerator ExitCommentaryRoutine(
        HearthLobbyConversationZone zone,
        HearthDialogueSequence sequence)
    {
        busy = true;
        currentStage = HearthLobbyFlowStage.OptionalConversation;
        SetHumanControl(true, true, true, false);

        yield return PlayDialogue(sequence);

        if (zone != null)
        {
            zone.MarkExitCommentaryCompleted();
        }

        busy = false;
        currentStage = HearthLobbyFlowStage.FreeExploration;
        SetHumanControl(true, true, true, false);
        activeRoutine = null;
    }

    private IEnumerator AssignmentLoadedRoutine()
    {
        busy = true;
        currentStage = HearthLobbyFlowStage.AssignmentTerminal;
        assignmentLoaded = true;
        assignmentTerminalCloseAvailable = false;
        assignmentTerminalCloseRequested = false;
        PlaySfxCue("AssignmentTerminal.Confirm");

        if (assignmentTerminal != null)
        {
            assignmentTerminal.SetPrimaryActionInputEnabled(false);
            assignmentTerminal.SetCloseInputEnabled(false);
            assignmentTerminal.SetRuntimePrompt("PLEASE WAIT");
        }

        if (hudOverlay != null)
        {
            hudOverlay.SetAssignmentLoaded(true);
        }

        if (onAssignmentLoaded != null)
        {
            onAssignmentLoaded.Invoke();
        }

        float elapsed = 0f;
        while (!assignmentTerminalCloseRequested)
        {
            elapsed += Time.unscaledDeltaTime;
            if (!assignmentTerminalCloseAvailable && elapsed >= assignmentTerminalMinimumViewSeconds)
            {
                assignmentTerminalCloseAvailable = true;
                if (assignmentTerminal != null && assignmentTerminal.IsOpen)
                {
                    assignmentTerminal.SetPrimaryActionInputEnabled(true);
                    assignmentTerminal.SetRuntimePrompt("SPACE  CLOSE TERMINAL");
                }
            }

            yield return null;
        }

        while (assignmentTerminal != null && assignmentTerminal.IsOpen)
        {
            yield return null;
        }

        // The route briefing continues in-world. Mia can move and look, while
        // all interaction remains locked until the final instruction ends.
        SetHumanControl(true, true, false, true);
        if (subtitlePlayer != null &&
            assignmentLoadedDialogue != null &&
            assignmentLoadedDialogue.HasLines)
        {
            subtitlePlayer.PlaySequence(assignmentLoadedDialogue);
        }

        while (subtitlePlayer != null && subtitlePlayer.IsPlaying)
        {
            yield return null;
        }

        if (assignmentTerminal != null)
        {
            assignmentTerminal.SetCloseInputEnabled(true);
            assignmentTerminal.ClearRuntimePrompt();
        }

        busy = false;
        currentStage = HearthLobbyFlowStage.FreeExploration;
        SetHumanControl(true, true, true, false);
        activeRoutine = null;
    }

    private IEnumerator ElevatorRideRoutine()
    {
        busy = true;
        currentStage = HearthLobbyFlowStage.ElevatorTransition;
        SetHumanControl(false, false, false, true);
        PlaySfxCue("Elevator.Button");
        PlaySfxCue("Elevator.DoorsClose");
        StopSfxCue("Lobby.RoomTone");

        if (screenFader != null)
        {
            yield return screenFader.FadeOut(transitionFadeOutSeconds);
        }

        TeleportHuman(elevatorAnchor, elevatorCameraAnchor);
        currentStage = HearthLobbyFlowStage.ElevatorRide;
        StartSfxLoop("Elevator.Motor");
        SetHumanControl(false, true, false, true);

        if (onElevatorEntered != null)
        {
            onElevatorEntered.Invoke();
        }

        if (screenFader != null)
        {
            yield return screenFader.FadeIn(transitionFadeInSeconds);
        }

        yield return PlayDialogue(elevatorDialogue);

        StopSfxCue("Elevator.Motor");
        PlaySfxCue("Elevator.Arrival");
        PlaySfxCue("Elevator.DoorsOpen");

        currentStage = HearthLobbyFlowStage.Floor17Transition;
        SetHumanControl(false, false, false, true);
        if (screenFader != null)
        {
            yield return screenFader.FadeOut(transitionFadeOutSeconds);
        }

        TeleportHuman(floor17ArrivalAnchor, floor17ArrivalCameraAnchor);
        StartSfxLoop("Corridor.RoomTone");
        if (locationProbe != null)
        {
            locationProbe.RefreshCurrentLocation();
        }

        if (screenFader != null)
        {
            yield return screenFader.FadeIn(transitionFadeInSeconds);
        }

        SetHumanControl(false, true, false, true);
        yield return PlayDialogue(floor17ArrivalDialogue);

        busy = false;
        currentStage = HearthLobbyFlowStage.ArrivedFloor17;
        SetHumanControl(true, true, true, false);
        if (onFloor17Arrived != null)
        {
            onFloor17Arrived.Invoke();
        }

        activeRoutine = null;
    }

    private void PlaySfxCue(string cueId)
    {
        if (sfxCuePlayer != null)
        {
            sfxCuePlayer.PlayCue(cueId);
        }
    }

    private void StartSfxLoop(string cueId)
    {
        if (sfxCuePlayer != null)
        {
            sfxCuePlayer.StartCueLoop(cueId);
        }
    }

    private void StopSfxCue(string cueId)
    {
        if (sfxCuePlayer != null)
        {
            sfxCuePlayer.StopCue(cueId);
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

    private void StartFlowRoutine(IEnumerator routine)
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

        if (subtitlePlayer != null && subtitlePlayer.IsPlaying)
        {
            subtitlePlayer.Stop();
        }
    }

    private void ResolveReferences()
    {
        if (humanRoot == null)
        {
            GameObject player = GameObject.Find("Player/Person Controller");
            humanRoot = player != null ? player.transform : null;
        }

        if (humanRoot != null)
        {
            if (humanMovement == null) humanMovement = humanRoot.GetComponent<FirstPersonMovement>();
            if (humanInteraction == null) humanInteraction = humanRoot.GetComponent<PlayerInteraction>();
            if (humanRigidbody == null) humanRigidbody = humanRoot.GetComponent<Rigidbody>();
            if (humanCamera == null) humanCamera = humanRoot.GetComponentInChildren<Camera>(true);
        }

        if (humanCamera != null && humanLook == null)
        {
            humanLook = humanCamera.GetComponent<FirstPersonLook>();
        }

        if (subtitlePlayer == null)
        {
            subtitlePlayer = FindObjectOfType<MinLoopSubtitlePlayer>(true);
        }

        if (locationProbe == null)
        {
            locationProbe = FindObjectOfType<HearthLocationProbe>(true);
        }
    }

    private void SetHumanControl(bool move, bool look, bool interact, bool lockAuxiliaryInputs)
    {
        desiredMove = move;
        desiredLook = look;
        desiredInteract = interact;
        desiredControlStateActive = lockAuxiliaryInputs || !move || !look || !interact;
        ApplyDesiredControlState();

        if (lockAuxiliaryInputs)
        {
            LockAuxiliaryInputs();
        }
        else
        {
            RestoreAuxiliaryInputs();
        }
    }

    private void ApplyDesiredControlState()
    {
        if (humanMovement != null) humanMovement.enabled = desiredMove;
        if (humanLook != null) humanLook.enabled = desiredLook;
        if (humanInteraction != null) humanInteraction.SetInteractionEnabled(desiredInteract);
        if (!desiredMove) ClearHumanVelocity();
    }

    private void LockAuxiliaryInputs()
    {
        if (auxiliaryLocked || auxiliaryInputBehaviours == null)
        {
            return;
        }

        auxiliaryEnabledBeforeLock = new bool[auxiliaryInputBehaviours.Length];
        for (int i = 0; i < auxiliaryInputBehaviours.Length; i++)
        {
            Behaviour behaviour = auxiliaryInputBehaviours[i];
            auxiliaryEnabledBeforeLock[i] = behaviour != null && behaviour.enabled;
            if (behaviour != null)
            {
                behaviour.enabled = false;
            }
        }

        auxiliaryLocked = true;
    }

    private void RestoreAuxiliaryInputs()
    {
        if (!auxiliaryLocked || auxiliaryInputBehaviours == null || auxiliaryEnabledBeforeLock == null)
        {
            auxiliaryLocked = false;
            return;
        }

        int count = Mathf.Min(auxiliaryInputBehaviours.Length, auxiliaryEnabledBeforeLock.Length);
        for (int i = 0; i < count; i++)
        {
            if (auxiliaryInputBehaviours[i] != null)
            {
                auxiliaryInputBehaviours[i].enabled = auxiliaryEnabledBeforeLock[i];
            }
        }

        auxiliaryLocked = false;
        auxiliaryEnabledBeforeLock = null;
    }

    private void CacheHumanCameraPivot()
    {
        if (humanCamera == null)
        {
            return;
        }

        cachedCameraParent = humanCamera.transform.parent;
        cachedCameraLocalPosition = humanCamera.transform.localPosition;
        hasCachedCameraPivot = true;
    }

    private void TeleportHuman(Transform rootAnchor, Transform cameraAnchor)
    {
        if (humanRoot == null || rootAnchor == null)
        {
            Debug.LogWarning("[HearthLobbyFlowController] A player destination anchor is missing.", this);
            return;
        }

        CacheHumanCameraPivot();
        Quaternion rootRotation = rootAnchor.rotation;
        bool hasCameraPose = humanCamera != null && cameraAnchor != null;
        if (hasCameraPose)
        {
            Vector3 flatForward = Vector3.ProjectOnPlane(cameraAnchor.forward, Vector3.up);
            if (flatForward.sqrMagnitude > 0.0001f)
            {
                rootRotation = Quaternion.LookRotation(flatForward.normalized, Vector3.up);
            }
        }

        ClearHumanVelocity();
        humanRoot.SetPositionAndRotation(rootAnchor.position, rootRotation);

        if (hasCameraPose)
        {
            Transform cameraTransform = humanCamera.transform;
            if (hasCachedCameraPivot && cameraTransform.parent == cachedCameraParent)
            {
                cameraTransform.localPosition = cachedCameraLocalPosition;
            }

            cameraTransform.rotation = cameraAnchor.rotation;
            if (hasCachedCameraPivot && cameraTransform.parent == cachedCameraParent)
            {
                humanRoot.position += cameraAnchor.position - cameraTransform.position;
                cameraTransform.localPosition = cachedCameraLocalPosition;
            }
            else
            {
                cameraTransform.position = cameraAnchor.position;
            }
        }

        if (humanLook != null)
        {
            humanLook.ForceLookFromCurrentTransforms();
        }

        if (humanRigidbody != null)
        {
            humanRigidbody.position = humanRoot.position;
            humanRigidbody.rotation = humanRoot.rotation;
        }

        Physics.SyncTransforms();
    }

    private void ClearHumanVelocity()
    {
        if (humanRigidbody == null || humanRigidbody.isKinematic)
        {
            return;
        }

        humanRigidbody.velocity = Vector3.zero;
        humanRigidbody.angularVelocity = Vector3.zero;
    }
}
