using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class Hearth17F04FinaleController : MonoBehaviour
{
    public enum FinaleStage
    {
        Inactive,
        HomeTerminal,
        LivingRoom,
        Photo,
        DaughterRoom,
        Dialogue,
        FinalChoice,
        ApproachUnit,
        Shutdown,
        Epilogue,
        Complete
    }

    public enum TrustPreviewOverride
    {
        UseRuntimeTrust,
        ForceHigh,
        ForceLow
    }

    [Header("Progress")]
    [SerializeField] private HearthHouseholdProgressState householdProgress;
    [SerializeField] private bool requirePreviousHouseholds;
    [SerializeField] private TrustStateController trustState;
    [SerializeField] private TrustPreviewOverride trustPreviewOverride;

    [Header("Human Player")]
    [SerializeField] private Transform humanRoot;
    [SerializeField] private Camera humanCamera;
    [SerializeField] private FirstPersonMovement humanMovement;
    [SerializeField] private FirstPersonLook humanLook;
    [SerializeField] private PlayerInteraction humanInteraction;
    [SerializeField] private Rigidbody humanRigidbody;

    [Header("Anchors")]
    [SerializeField] private Transform livingRoomAnchor;
    [SerializeField] private Transform livingRoomCameraAnchor;
    [SerializeField] private Transform daughterRoomAnchor;
    [SerializeField] private Transform daughterRoomCameraAnchor;
    [SerializeField] private Transform corridorReturnAnchor;
    [SerializeField] private Transform corridorReturnCameraAnchor;

    [Header("Home Terminal Handoff")]
    [SerializeField] private HearthTvTerminalController homeTerminal;

    [Header("World Interactions")]
    [SerializeField] private HearthPhotoFrameInteractable photoFrame;
    [SerializeField] private Hearth17F04RoomDoorInteractable daughterRoomDoor;
    [SerializeField] private Hearth17F04HomeUnitInteractable homeUnit;
    [SerializeField] private Hearth17F04CatGuideController catGuide;

    [Header("HUD And Dialogue")]
    [SerializeField] private HearthFirstPersonHudController firstPersonHud;
    [SerializeField] private HearthFirstPersonHudInput firstPersonHudInput;
    [SerializeField] private MinLoopSubtitlePlayer sceneSubtitlePlayer;
    [SerializeField] private MinLoopSubtitlePlayer epilogueSubtitlePlayer;
    [SerializeField] private CanvasGroup blackoutCanvasGroup;
    [SerializeField] private Image blackoutImage;
    [SerializeField] private float fadeOutSeconds = 0.5f;
    [SerializeField] private float blackHoldSeconds = 0.1f;
    [SerializeField] private float fadeInSeconds = 0.5f;
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Sequences")]
    [SerializeField] private HearthDialogueSequence homeGreetingHighTrust;
    [SerializeField] private HearthDialogueSequence homeGreetingLowTrust;
    [SerializeField] private HearthDialogueSequence christmasPhotoSequence;
    [SerializeField] private HearthDialogueSequence hearingDaughterRoomSequence;
    [SerializeField] private HearthDialogueSequence daughterRoomHighTrustSequence;
    [SerializeField] private HearthDialogueSequence daughterRoomLowTrustSequence;
    [SerializeField] private HearthDialogueSequence answerSelfSequence;
    [SerializeField] private HearthDialogueSequence companionAnswerSequence;
    [SerializeField] private HearthDialogueSequence shutdownHighTrustSequence;
    [SerializeField] private HearthDialogueSequence shutdownLowTrustSequence;
    [SerializeField] private HearthDialogueSequence epilogueHighRetain;
    [SerializeField] private HearthDialogueSequence epilogueHighShutdown;
    [SerializeField] private HearthDialogueSequence epilogueLowRetain;
    [SerializeField] private HearthDialogueSequence epilogueLowShutdown;

    [Header("Shutdown Challenge")]
    [SerializeField] private HearthShutdownChallenge shutdownChallenge;

    [Header("Events")]
    [SerializeField] private UnityEvent onFinaleCompleted = new UnityEvent();

    [Header("Runtime State")]
    [SerializeField] private FinaleStage currentStage = FinaleStage.Inactive;

    private Coroutine flowRoutine;
    private Coroutine homeGreetingRoutine;
    private Coroutine photoDialogueRoutine;
    private bool photoUnlocked;
    private bool homeGreetingComplete;
    private bool daughterRoomUnlocked;
    private bool choiceSubmitted;
    private bool selectedAnswerSelf;
    private bool currentHighTrust;
    private bool savedHudRouting;
    private bool hasSavedFinalChoiceInputProfile;
    private HearthFinalChoiceInputProfile savedFinalChoiceInputProfile;
    private bool hasSavedCorridorPose;
    private Vector3 savedCorridorPosition;
    private Quaternion savedCorridorRotation;
    private Vector3 savedCorridorCameraPosition;
    private Quaternion savedCorridorCameraRotation;
    private Transform cachedHumanCameraParent;
    private Vector3 cachedHumanCameraLocalPosition;
    private bool hasCachedHumanCameraPivot;

    public FinaleStage CurrentStage
    {
        get { return currentStage; }
    }

    public bool CanInspectPhoto
    {
        get { return currentStage == FinaleStage.LivingRoom && photoUnlocked; }
    }

    public bool CanEnterDaughterRoom
    {
        get { return currentStage == FinaleStage.LivingRoom && daughterRoomUnlocked && flowRoutine == null; }
    }

    public bool CanBeginUnitShutdown
    {
        get { return currentStage == FinaleStage.ApproachUnit && selectedAnswerSelf && flowRoutine == null; }
    }

    public UnityEvent OnFinaleCompleted
    {
        get { return onFinaleCompleted; }
    }

    private void Awake()
    {
        ResolveReferences();
        SubscribeEvents();
        savedHudRouting = firstPersonHud == null || firstPersonHud.RouteFinalChoiceInternally;
        SetBlackoutAlpha(0f);
        SetHomeUnitAvailable(false);
    }

    private void OnDestroy()
    {
        RestoreFinalChoiceInputProfile();
        UnsubscribeEvents();
    }

    private void LateUpdate()
    {
        MaintainFreeControlStages();
    }

    private void OnValidate()
    {
        fadeOutSeconds = Mathf.Max(0f, fadeOutSeconds);
        blackHoldSeconds = Mathf.Max(0f, blackHoldSeconds);
        fadeInSeconds = Mathf.Max(0f, fadeInSeconds);
    }

    public void BeginFromHomeTerminal()
    {
        if (flowRoutine != null || (currentStage != FinaleStage.Inactive && currentStage != FinaleStage.Complete))
        {
            if (homeTerminal != null)
            {
                homeTerminal.CancelCustomActionHandoff();
            }
            return;
        }

        ResolveReferences();
        if (requirePreviousHouseholds && (householdProgress == null || !householdProgress.AreFirstThreeCompleted))
        {
            Debug.LogWarning("[Hearth17F04FinaleController] The first three households are not complete, so 17F04 entry is blocked.", this);
            if (homeTerminal != null)
            {
                homeTerminal.CancelCustomActionHandoff();
            }
            return;
        }

        SaveCorridorPose();
        currentHighTrust = EvaluateHighTrust();
        currentStage = FinaleStage.HomeTerminal;
        StopParallelNarrativeCoroutines();
        choiceSubmitted = false;
        selectedAnswerSelf = false;
        photoUnlocked = false;
        homeGreetingComplete = false;
        daughterRoomUnlocked = false;
        SetHomeUnitAvailable(false);
        StartFlow(EnterHomeRoutine());
    }

    public void BeginPhotoInspection()
    {
        if (currentStage != FinaleStage.LivingRoom || !photoUnlocked || photoDialogueRoutine != null)
        {
            return;
        }

        photoUnlocked = false;
        currentStage = FinaleStage.Photo;
        photoDialogueRoutine = StartCoroutine(PhotoSequenceAfterGreetingRoutine());
    }

    public void CompletePhotoInspection()
    {
        if (currentStage != FinaleStage.Photo || photoDialogueRoutine != null || flowRoutine != null)
        {
            return;
        }

        currentStage = FinaleStage.LivingRoom;
        SetHumanControls(true, true, true);
        StartFlow(UnlockDaughterRoomRoutine());
    }

    public void EnterDaughterRoom()
    {
        if (!CanEnterDaughterRoom)
        {
            return;
        }

        daughterRoomUnlocked = false;
        currentStage = FinaleStage.DaughterRoom;
        StartFlow(EnterDaughterRoomRoutine());
    }

    public void ChooseAnswerSelf()
    {
        if (currentStage != FinaleStage.FinalChoice || choiceSubmitted)
        {
            return;
        }

        choiceSubmitted = true;
        selectedAnswerSelf = true;
        RestoreFinalChoiceInputProfile();
        if (firstPersonHud != null)
        {
            firstPersonHud.HideOverlay();
        }

        StartFlow(AnswerSelfRoutine());
    }

    public void ChooseCompanionAnswer()
    {
        if (currentStage != FinaleStage.FinalChoice || choiceSubmitted)
        {
            return;
        }

        choiceSubmitted = true;
        selectedAnswerSelf = false;
        RestoreFinalChoiceInputProfile();
        if (firstPersonHud != null)
        {
            firstPersonHud.HideOverlay();
        }

        StartFlow(CompanionAnswerRoutine());
    }

    public void BeginUnitShutdown()
    {
        if (!CanBeginUnitShutdown)
        {
            return;
        }

        currentStage = FinaleStage.Shutdown;
        SetHomeUnitAvailable(false);
        SetHumanControls(false, false, false);
        if (shutdownChallenge != null)
        {
            shutdownChallenge.BeginChallenge(currentHighTrust);
        }
        else
        {
            Debug.LogWarning("[Hearth17F04FinaleController] Shutdown challenge is missing; continuing to the shutdown scene.", this);
            HandleShutdownChallengeCompleted();
        }
    }

    public void CompleteFinale()
    {
        if (currentStage == FinaleStage.Epilogue || currentStage == FinaleStage.Complete)
        {
            return;
        }

        StartFlow(EpilogueRoutine());
    }

    public void ResetForPreview()
    {
        if (flowRoutine != null)
        {
            StopCoroutine(flowRoutine);
            flowRoutine = null;
        }

        StopParallelNarrativeCoroutines();

        if (sceneSubtitlePlayer != null) sceneSubtitlePlayer.Hide();
        if (epilogueSubtitlePlayer != null) epilogueSubtitlePlayer.Hide();
        if (firstPersonHud != null)
        {
            firstPersonHud.SetRouteFinalChoiceInternally(savedHudRouting);
            firstPersonHud.HideOverlay();
        }
        RestoreFinalChoiceInputProfile();
        if (catGuide != null) catGuide.ResetSequence();

        currentStage = FinaleStage.Inactive;
        photoUnlocked = false;
        daughterRoomUnlocked = false;
        choiceSubmitted = false;
        selectedAnswerSelf = false;
        SetHomeUnitAvailable(false);
        SetBlackoutAlpha(0f);
        SetHumanControls(true, true, true);
    }

    private IEnumerator EnterHomeRoutine()
    {
        SetHumanControls(false, false, false);
        yield return FadeTo(1f, fadeOutSeconds);

        if (homeTerminal != null)
        {
            homeTerminal.CompleteCustomActionHandoff();
            SetHumanControls(false, false, false);
        }

        yield return Wait(blackHoldSeconds);
        TeleportHuman(livingRoomAnchor, livingRoomCameraAnchor);
        yield return FadeTo(0f, fadeInSeconds);

        currentStage = FinaleStage.LivingRoom;
        SetHumanControls(true, true, true);
        photoUnlocked = true;
        flowRoutine = null;
        if (catGuide != null)
        {
            catGuide.BeginSequence();
        }

        homeGreetingRoutine = StartCoroutine(HomeGreetingRoutine());
    }

    private IEnumerator HomeGreetingRoutine()
    {
        yield return PlaySceneSequence(currentHighTrust ? homeGreetingHighTrust : homeGreetingLowTrust);
        homeGreetingComplete = true;
        homeGreetingRoutine = null;
    }

    private IEnumerator PhotoSequenceAfterGreetingRoutine()
    {
        while (!homeGreetingComplete)
        {
            yield return null;
        }

        yield return PlaySceneSequence(christmasPhotoSequence);
        if (photoFrame != null)
        {
            photoFrame.NotifyDialogueComplete();
        }

        photoDialogueRoutine = null;
    }

    private IEnumerator UnlockDaughterRoomRoutine()
    {
        yield return PlaySceneSequence(hearingDaughterRoomSequence);
        daughterRoomUnlocked = true;
        flowRoutine = null;
    }

    private IEnumerator EnterDaughterRoomRoutine()
    {
        SetHumanControls(false, false, false);
        yield return FadeTo(1f, fadeOutSeconds);
        yield return Wait(blackHoldSeconds);
        TeleportHuman(daughterRoomAnchor, daughterRoomCameraAnchor);
        yield return FadeTo(0f, fadeInSeconds);

        currentStage = FinaleStage.Dialogue;
        SetHumanControls(true, true, true);
        yield return PlaySceneSequence(currentHighTrust ? daughterRoomHighTrustSequence : daughterRoomLowTrustSequence);

        currentStage = FinaleStage.FinalChoice;
        SetHumanControls(false, false, false);
        Apply17F04FinalChoiceInputProfile();
        if (firstPersonHud != null)
        {
            savedHudRouting = firstPersonHud.RouteFinalChoiceInternally;
            firstPersonHud.SetRouteFinalChoiceInternally(false);
            firstPersonHud.ShowFinalChoice(false);
        }
        else
        {
            Debug.LogWarning("[Hearth17F04FinaleController] First-person HUD is missing; call ChooseAnswerSelf or ChooseCompanionAnswer from a replacement UI.", this);
        }

        flowRoutine = null;
    }

    private IEnumerator AnswerSelfRoutine()
    {
        currentStage = FinaleStage.Dialogue;
        SetHumanControls(true, true, true);
        yield return PlaySceneSequence(answerSelfSequence);
        currentStage = FinaleStage.ApproachUnit;
        SetHomeUnitAvailable(true);
        flowRoutine = null;
    }

    private IEnumerator CompanionAnswerRoutine()
    {
        currentStage = FinaleStage.Dialogue;
        SetHumanControls(true, true, true);
        yield return PlaySceneSequence(companionAnswerSequence);
        flowRoutine = null;
        CompleteFinale();
    }

    private IEnumerator ShutdownResultRoutine()
    {
        if (firstPersonHud != null)
        {
            firstPersonHud.HideOverlay();
        }
        RestoreFinalChoiceInputProfile();

        yield return PlaySceneSequence(currentHighTrust ? shutdownHighTrustSequence : shutdownLowTrustSequence);
        flowRoutine = null;
        CompleteFinale();
    }

    private IEnumerator EpilogueRoutine()
    {
        currentStage = FinaleStage.Epilogue;
        SetHomeUnitAvailable(false);
        SetHumanControls(false, false, false);
        if (firstPersonHud != null)
        {
            firstPersonHud.HideOverlay();
        }

        yield return FadeTo(1f, fadeOutSeconds);
        HearthDialogueSequence epilogue = GetEpilogueSequence();
        if (epilogueSubtitlePlayer != null && epilogue != null)
        {
            yield return epilogueSubtitlePlayer.PlaySequenceAsset(epilogue);
        }

        yield return Wait(blackHoldSeconds);
        RestoreCorridorPose();
        yield return FadeTo(0f, fadeInSeconds);

        if (firstPersonHud != null)
        {
            firstPersonHud.SetRouteFinalChoiceInternally(savedHudRouting);
            firstPersonHud.HideOverlay();
        }

        SetHumanControls(true, true, true);
        currentStage = FinaleStage.Complete;
        if (householdProgress != null)
        {
            householdProgress.MarkHouseholdCompleted("17F04");
        }

        onFinaleCompleted.Invoke();
        flowRoutine = null;
    }

    private void HandleFinalChoiceA()
    {
        ChooseAnswerSelf();
    }

    private void HandleFinalChoiceB()
    {
        ChooseCompanionAnswer();
    }

    private void HandleShutdownChallengeCompleted()
    {
        if (currentStage != FinaleStage.Shutdown || flowRoutine != null)
        {
            return;
        }

        StartFlow(ShutdownResultRoutine());
    }

    private void HandleShutdownChallengeCancelled()
    {
        if (currentStage != FinaleStage.Shutdown)
        {
            return;
        }

        if (firstPersonHud != null)
        {
            firstPersonHud.HideOverlay();
        }

        currentStage = FinaleStage.ApproachUnit;
        SetHomeUnitAvailable(true);
        SetHumanControls(true, true, true);
    }

    private void SubscribeEvents()
    {
        if (firstPersonHud != null)
        {
            firstPersonHud.OnFinalChoiceA.RemoveListener(HandleFinalChoiceA);
            firstPersonHud.OnFinalChoiceB.RemoveListener(HandleFinalChoiceB);
            firstPersonHud.OnFinalChoiceA.AddListener(HandleFinalChoiceA);
            firstPersonHud.OnFinalChoiceB.AddListener(HandleFinalChoiceB);
        }

        if (shutdownChallenge != null)
        {
            shutdownChallenge.Completed.RemoveListener(HandleShutdownChallengeCompleted);
            shutdownChallenge.Cancelled.RemoveListener(HandleShutdownChallengeCancelled);
            shutdownChallenge.Completed.AddListener(HandleShutdownChallengeCompleted);
            shutdownChallenge.Cancelled.AddListener(HandleShutdownChallengeCancelled);
        }
    }

    private void UnsubscribeEvents()
    {
        if (firstPersonHud != null)
        {
            firstPersonHud.OnFinalChoiceA.RemoveListener(HandleFinalChoiceA);
            firstPersonHud.OnFinalChoiceB.RemoveListener(HandleFinalChoiceB);
        }

        if (shutdownChallenge != null)
        {
            shutdownChallenge.Completed.RemoveListener(HandleShutdownChallengeCompleted);
            shutdownChallenge.Cancelled.RemoveListener(HandleShutdownChallengeCancelled);
        }
    }

    private void ResolveReferences()
    {
        if (firstPersonHud == null) firstPersonHud = FindObjectOfType<HearthFirstPersonHudController>(true);
        if (firstPersonHudInput == null) firstPersonHudInput = FindObjectOfType<HearthFirstPersonHudInput>(true);
        if (catGuide == null) catGuide = FindObjectOfType<Hearth17F04CatGuideController>(true);
        if (trustState == null) trustState = FindObjectOfType<TrustStateController>(true);
        if (householdProgress == null) householdProgress = FindObjectOfType<HearthHouseholdProgressState>(true);
        if (homeTerminal == null)
        {
            HearthTvTerminalController[] terminals = FindObjectsOfType<HearthTvTerminalController>(true);
            for (int i = 0; i < terminals.Length; i++)
            {
                HearthTvTerminalController terminal = terminals[i];
                if (terminal != null &&
                    terminal.PrimaryAction == HearthTerminalPrimaryAction.Custom &&
                    terminal.GetReplayResidentId() == "17F04")
                {
                    homeTerminal = terminal;
                    break;
                }
            }
        }

        if (humanInteraction == null)
        {
            PlayerInteraction[] interactions = FindObjectsOfType<PlayerInteraction>(true);
            for (int i = 0; i < interactions.Length; i++)
            {
                if (interactions[i] != null && interactions[i].gameObject.name == "Person Controller")
                {
                    humanInteraction = interactions[i];
                    break;
                }
            }
        }

        if (humanInteraction != null)
        {
            if (humanRoot == null) humanRoot = humanInteraction.transform;
            if (humanCamera == null) humanCamera = humanInteraction.mainCamera;
            if (humanMovement == null) humanMovement = humanInteraction.GetComponent<FirstPersonMovement>();
            if (humanLook == null) humanLook = humanInteraction.GetComponentInChildren<FirstPersonLook>(true);
            if (humanRigidbody == null) humanRigidbody = humanInteraction.GetComponent<Rigidbody>();
        }

        CacheHumanCameraPivot();

        if (blackoutImage != null)
        {
            blackoutImage.color = Color.black;
            blackoutImage.raycastTarget = true;
        }
    }

    private void StartFlow(IEnumerator routine)
    {
        if (flowRoutine != null)
        {
            StopCoroutine(flowRoutine);
        }

        flowRoutine = StartCoroutine(routine);
    }

    private void StopParallelNarrativeCoroutines()
    {
        if (homeGreetingRoutine != null)
        {
            StopCoroutine(homeGreetingRoutine);
            homeGreetingRoutine = null;
        }

        if (photoDialogueRoutine != null)
        {
            StopCoroutine(photoDialogueRoutine);
            photoDialogueRoutine = null;
        }

        homeGreetingComplete = false;
    }

    private void Apply17F04FinalChoiceInputProfile()
    {
        if (firstPersonHudInput == null)
        {
            return;
        }

        if (!hasSavedFinalChoiceInputProfile)
        {
            savedFinalChoiceInputProfile = firstPersonHudInput.GetFinalChoiceInputProfile();
            hasSavedFinalChoiceInputProfile = true;
        }

        firstPersonHudInput.SetFinalChoiceInputProfile(
            new HearthFinalChoiceInputProfile(
                HearthFinalChoiceNavigationAxis.Vertical,
                false,
                false));
    }

    private void RestoreFinalChoiceInputProfile()
    {
        if (!hasSavedFinalChoiceInputProfile || firstPersonHudInput == null)
        {
            return;
        }

        firstPersonHudInput.SetFinalChoiceInputProfile(savedFinalChoiceInputProfile);
        hasSavedFinalChoiceInputProfile = false;
    }

    private IEnumerator PlaySceneSequence(HearthDialogueSequence sequence)
    {
        if (sceneSubtitlePlayer != null && sequence != null)
        {
            yield return sceneSubtitlePlayer.PlaySequenceAsset(sequence);
        }
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (blackoutCanvasGroup == null)
        {
            yield break;
        }

        float start = blackoutCanvasGroup.alpha;
        blackoutCanvasGroup.blocksRaycasts = true;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
            SetBlackoutAlpha(Mathf.Lerp(start, targetAlpha, t));
            yield return null;
        }

        SetBlackoutAlpha(targetAlpha);
    }

    private IEnumerator Wait(float seconds)
    {
        if (seconds <= 0f)
        {
            yield break;
        }

        if (useUnscaledTime)
        {
            yield return new WaitForSecondsRealtime(seconds);
        }
        else
        {
            yield return new WaitForSeconds(seconds);
        }
    }

    private void SetBlackoutAlpha(float alpha)
    {
        if (blackoutCanvasGroup == null)
        {
            return;
        }

        blackoutCanvasGroup.alpha = Mathf.Clamp01(alpha);
        blackoutCanvasGroup.blocksRaycasts = alpha > 0.001f;
        blackoutCanvasGroup.interactable = false;
    }

    private void SetHumanControls(bool movement, bool look, bool interaction)
    {
        if (humanMovement != null) humanMovement.enabled = movement;
        if (humanLook != null) humanLook.enabled = look;
        if (humanInteraction != null) humanInteraction.SetInteractionEnabled(interaction);
        if (!movement && humanRigidbody != null && !humanRigidbody.isKinematic)
        {
            humanRigidbody.velocity = Vector3.zero;
            humanRigidbody.angularVelocity = Vector3.zero;
        }
    }

    private void MaintainFreeControlStages()
    {
        bool freeControlStage = currentStage == FinaleStage.LivingRoom ||
                                currentStage == FinaleStage.Dialogue ||
                                currentStage == FinaleStage.ApproachUnit;
        if (!freeControlStage)
        {
            return;
        }

        if (photoFrame != null && (photoFrame.IsOpen || photoFrame.IsTransitioning))
        {
            return;
        }

        if (firstPersonHud != null &&
            firstPersonHud.CurrentPageId != HearthFirstPersonHudPageId.None &&
            firstPersonHud.CurrentPageId != HearthFirstPersonHudPageId.Slide01PersistentHud &&
            firstPersonHud.CurrentPageId != HearthFirstPersonHudPageId.Slide02TrustDelta)
        {
            return;
        }

        bool needsRepair = humanMovement != null && !humanMovement.enabled ||
                           humanLook != null && !humanLook.enabled ||
                           humanInteraction != null && !humanInteraction.InteractionEnabled;
        if (needsRepair)
        {
            SetHumanControls(true, true, true);
        }
    }

    public void CaptureCurrentHumanCameraPivot()
    {
        hasCachedHumanCameraPivot = false;
        CacheHumanCameraPivot();
    }

    private void CacheHumanCameraPivot()
    {
        if (humanCamera == null)
        {
            return;
        }

        Transform cameraTransform = humanCamera.transform;
        if (hasCachedHumanCameraPivot && cachedHumanCameraParent == cameraTransform.parent)
        {
            return;
        }

        cachedHumanCameraParent = cameraTransform.parent;
        cachedHumanCameraLocalPosition = cameraTransform.localPosition;
        hasCachedHumanCameraPivot = true;
    }

    private void TeleportHuman(Transform rootAnchor, Transform cameraAnchor)
    {
        if (humanRoot == null || rootAnchor == null)
        {
            Debug.LogWarning("[Hearth17F04FinaleController] A human destination anchor is missing.", this);
            return;
        }

        bool hasCameraPose = humanCamera != null && cameraAnchor != null;
        ApplyHumanPose(
            rootAnchor.position,
            rootAnchor.rotation,
            hasCameraPose,
            hasCameraPose ? cameraAnchor.position : Vector3.zero,
            hasCameraPose ? cameraAnchor.rotation : Quaternion.identity);
    }

    private void ApplyHumanPose(
        Vector3 rootPosition,
        Quaternion rootRotation,
        bool hasCameraPose,
        Vector3 cameraPosition,
        Quaternion cameraRotation)
    {
        CacheHumanCameraPivot();

        if (hasCameraPose)
        {
            Vector3 flatForward = Vector3.ProjectOnPlane(cameraRotation * Vector3.forward, Vector3.up);
            if (flatForward.sqrMagnitude > 0.0001f)
            {
                rootRotation = Quaternion.LookRotation(flatForward.normalized, Vector3.up);
            }
        }

        if (humanRigidbody != null && !humanRigidbody.isKinematic)
        {
            humanRigidbody.velocity = Vector3.zero;
            humanRigidbody.angularVelocity = Vector3.zero;
        }

        // Set the Transform first. Rigidbody.position is not guaranteed to update child transforms
        // until the next physics step, which can otherwise create a large camera local offset.
        humanRoot.SetPositionAndRotation(rootPosition, rootRotation);

        if (humanCamera != null)
        {
            Transform cameraTransform = humanCamera.transform;
            if (hasCachedHumanCameraPivot && cameraTransform.parent == cachedHumanCameraParent)
            {
                cameraTransform.localPosition = cachedHumanCameraLocalPosition;
            }

            if (hasCameraPose)
            {
                cameraTransform.rotation = cameraRotation;

                if (hasCachedHumanCameraPivot && cameraTransform.parent == cachedHumanCameraParent)
                {
                    cameraTransform.localPosition = cachedHumanCameraLocalPosition;
                    humanRoot.position += cameraPosition - cameraTransform.position;
                }
                else
                {
                    cameraTransform.position = cameraPosition;
                }
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

    private void SaveCorridorPose()
    {
        if (humanRoot == null)
        {
            return;
        }

        savedCorridorPosition = humanRoot.position;
        savedCorridorRotation = humanRoot.rotation;
        if (humanCamera != null)
        {
            savedCorridorCameraPosition = humanCamera.transform.position;
            savedCorridorCameraRotation = humanCamera.transform.rotation;
        }

        hasSavedCorridorPose = true;
    }

    private void RestoreCorridorPose()
    {
        if (corridorReturnAnchor != null)
        {
            TeleportHuman(corridorReturnAnchor, corridorReturnCameraAnchor);
            return;
        }

        if (!hasSavedCorridorPose || humanRoot == null)
        {
            return;
        }

        ApplyHumanPose(
            savedCorridorPosition,
            savedCorridorRotation,
            humanCamera != null,
            savedCorridorCameraPosition,
            savedCorridorCameraRotation);
    }

    private bool EvaluateHighTrust()
    {
        if (trustPreviewOverride == TrustPreviewOverride.ForceHigh) return true;
        if (trustPreviewOverride == TrustPreviewOverride.ForceLow) return false;

        int score = trustState != null
            ? trustState.CurrentTrust
            : firstPersonHud != null ? firstPersonHud.TrustScore : 0;

        if (score == 0)
        {
            Debug.LogWarning("[Hearth17F04FinaleController] Trust is 0; using the high-trust preview branch. Use Trust Preview Override to force either branch.", this);
            return true;
        }

        return score > 0;
    }

    private HearthDialogueSequence GetEpilogueSequence()
    {
        if (currentHighTrust)
        {
            return selectedAnswerSelf ? epilogueHighShutdown : epilogueHighRetain;
        }

        return selectedAnswerSelf ? epilogueLowShutdown : epilogueLowRetain;
    }

    private void SetHomeUnitAvailable(bool value)
    {
        if (homeUnit != null)
        {
            homeUnit.SetAvailable(value);
        }
    }
}
