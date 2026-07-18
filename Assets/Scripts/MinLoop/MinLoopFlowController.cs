using System.Collections;
using UnityEngine;

public class MinLoopFlowController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MinLoopTerminalPresenter terminalPresenter;
    [SerializeField] private HearthTvTerminalController tvTerminalController;
    [SerializeField] private ViewSwitchController viewSwitchController;
    [SerializeField] private ReplaySequenceController replaySequenceController;
    [SerializeField] private HearthCompanion17F01ReplayController companion17F01ReplayController;
    [SerializeField] private HearthCompanion17F02ReplayController companion17F02ReplayController;
    [SerializeField] private HearthCompanion17F03ReplayController companion17F03ReplayController;
    [SerializeField] private TrustStateController trustStateController;
    [SerializeField] private HearthHouseholdProgressState householdProgress;
    [SerializeField] private MinLoopSubtitlePlayer subtitlePlayer;
    [SerializeField] private bool autoFindMissingReferences = true;
    [SerializeField] private bool useCompanion17F01ReplayController = true;
    [SerializeField] private bool useResidentSpecificReplayControllers = true;

    [Header("Active Resident")]
    [SerializeField] private string activeReplayResidentId = "17F01";

    [Header("Disposition Dialogue")]
    [SerializeField] private HearthResidentDispositionDialogueSet[] dispositionDialogueSets;

    [Header("Start")]
    [SerializeField] private bool resetFlowOnStart = true;

    [Header("Optional Feedback")]
    [SerializeField] private InteractionFeedbackController terminalOpenFeedback;
    [SerializeField] private InteractionFeedbackController accessCardFeedback;
    [SerializeField] private InteractionFeedbackController replayRequestFeedback;
    [SerializeField] private InteractionFeedbackController dispositionSubmitFeedback;

    [Header("Events")]
    [SerializeField] private MinLoopStageEvent stageChanged = new MinLoopStageEvent();
    [SerializeField] private MinLoopDispositionEvent dispositionApplied = new MinLoopDispositionEvent();

    [Header("Runtime State")]
    [SerializeField] private MinLoopStage currentStage = MinLoopStage.Corridor;
    [SerializeField] private bool dispositionSubmitted;

    private Coroutine activeFlowRoutine;
    private bool externalDispositionPresenter;

    public MinLoopStage CurrentStage
    {
        get { return currentStage; }
    }

    public MinLoopStageEvent StageChanged
    {
        get { return stageChanged; }
    }

    public MinLoopDispositionEvent DispositionApplied
    {
        get { return dispositionApplied; }
    }

    public bool DispositionSubmitted
    {
        get { return dispositionSubmitted; }
    }

    public string ActiveReplayResidentId
    {
        get { return activeReplayResidentId; }
    }

    private void Awake()
    {
        ResolveReferences();
    }

    private void Start()
    {
        if (resetFlowOnStart)
        {
            ResetFlow();
        }
    }

    public void ResetFlow()
    {
        StopActiveFlowRoutine();
        dispositionSubmitted = false;
        externalDispositionPresenter = false;
        SetStage(MinLoopStage.Corridor, true);

        if (trustStateController != null)
        {
            trustStateController.ResetTrust();
        }

        if (replaySequenceController != null)
        {
            replaySequenceController.CancelReplay();
        }

        if (companion17F01ReplayController != null)
        {
            companion17F01ReplayController.CancelReplay();
        }

        if (companion17F02ReplayController != null)
        {
            companion17F02ReplayController.CancelReplay();
        }

        if (companion17F03ReplayController != null)
        {
            companion17F03ReplayController.CancelFlow();
        }

        if (viewSwitchController != null && viewSwitchController.CurrentMode != ViewSwitchController.ViewMode.Human)
        {
            viewSwitchController.SwitchToHuman();
        }

        if (terminalPresenter != null)
        {
            terminalPresenter.Close();
        }

        if (tvTerminalController != null)
        {
            tvTerminalController.SetChoiceInputEnabled(true);
        }
    }

    public void SetTvTerminalController(HearthTvTerminalController controller)
    {
        tvTerminalController = controller;
    }

    public void SetViewSwitchController(ViewSwitchController controller)
    {
        viewSwitchController = controller;
    }

    public void SetCompanion17F01ReplayController(HearthCompanion17F01ReplayController controller)
    {
        companion17F01ReplayController = controller;
    }

    public void SetCompanion17F02ReplayController(HearthCompanion17F02ReplayController controller)
    {
        companion17F02ReplayController = controller;
    }

    public void SetHouseholdProgressState(HearthHouseholdProgressState progressState)
    {
        householdProgress = progressState;
    }

    public void SetCompanion17F03ReplayController(HearthCompanion17F03ReplayController controller)
    {
        companion17F03ReplayController = controller;
        if (companion17F03ReplayController != null)
        {
            companion17F03ReplayController.SetFlowController(this);
        }
    }

    public void SetActiveReplayResident(string residentId)
    {
        activeReplayResidentId = NormalizeResidentId(residentId);
    }

    public void SetActiveReplayResident(string residentId, HearthTvTerminalController terminal)
    {
        SetActiveReplayResident(residentId);
        if (terminal != null)
        {
            tvTerminalController = terminal;
        }
    }

    public void BeginTerminalInspection()
    {
        ResolveReferences();

        if (currentStage == MinLoopStage.SwitchingToCompanion ||
            currentStage == MinLoopStage.CompanionReplay ||
            currentStage == MinLoopStage.WaitingForComfort ||
            currentStage == MinLoopStage.Comforting ||
            currentStage == MinLoopStage.MorningReview ||
            currentStage == MinLoopStage.ReturningToTerminal)
        {
            return;
        }

        if (terminalPresenter == null)
        {
            Debug.LogWarning("MinLoopFlowController cannot open the terminal because no MinLoopTerminalPresenter is assigned.", this);
            return;
        }

        if (currentStage == MinLoopStage.AccessCard)
        {
            PlayFeedback(terminalOpenFeedback);
            terminalPresenter.ShowAccessCard(ConfirmAccessCard);
            return;
        }

        if (currentStage == MinLoopStage.ResidentSummary)
        {
            PlayFeedback(terminalOpenFeedback);
            terminalPresenter.ShowResidentSummary(RequestReplayFromTerminal);
            return;
        }

        if (currentStage == MinLoopStage.DispositionChoice)
        {
            terminalPresenter.ShowDispositionChoices(ChooseDispositionA, ChooseDispositionB);
            return;
        }

        if (currentStage == MinLoopStage.Complete)
        {
            terminalPresenter.ShowNextResident(CloseTerminal);
            return;
        }

        SetStage(MinLoopStage.AccessCard);
        PlayFeedback(terminalOpenFeedback);
        terminalPresenter.ShowAccessCard(ConfirmAccessCard);
    }

    public void ConfirmAccessCard()
    {
        if (terminalPresenter == null)
        {
            return;
        }

        SetStage(MinLoopStage.ResidentSummary);
        PlayFeedback(accessCardFeedback);
        terminalPresenter.ShowResidentSummary(RequestReplayFromTerminal);
    }

    public void RequestReplayFromTerminal()
    {
        ResolveReferences();

        if (currentStage == MinLoopStage.SwitchingToCompanion ||
            currentStage == MinLoopStage.CompanionReplay ||
            currentStage == MinLoopStage.WaitingForComfort ||
            currentStage == MinLoopStage.Comforting ||
            currentStage == MinLoopStage.MorningReview)
        {
            return;
        }

        PlayFeedback(replayRequestFeedback);

        if (terminalPresenter != null)
        {
            terminalPresenter.Close();
        }

        StartFlowRoutine(SwitchToCompanionAndBeginReplay());
    }

    public void RequestEnterUnitFromTerminal()
    {
        ResolveReferences();

        if (!string.Equals(activeReplayResidentId, "17F03", System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogWarning("MinLoopFlowController received Enter Unit for a resident other than 17F03.", this);
            return;
        }

        if (companion17F03ReplayController == null)
        {
            Debug.LogWarning("MinLoopFlowController cannot enter 17F03 because no HearthCompanion17F03ReplayController is assigned.", this);
            return;
        }

        StopActiveFlowRoutine();
        if (terminalPresenter != null)
        {
            terminalPresenter.Close();
        }

        SetStage(MinLoopStage.EnteringResidentUnit);
        companion17F03ReplayController.BeginHumanEntry(this);
    }

    public void NotifyResidentUnitDialogueStarted()
    {
        SetStage(MinLoopStage.ResidentUnitDialogue);
    }

    public void NotifyResidentUnitInspectionReady()
    {
        SetStage(MinLoopStage.ResidentUnitInspection);
    }

    public void NotifyResidentPostReplayStarted()
    {
        SetStage(MinLoopStage.ResidentPostReplay);
    }

    public void NotifyReplayComfortReady()
    {
        SetStage(MinLoopStage.WaitingForComfort);
    }

    public void NotifyComfortActionPerformed()
    {
        SetStage(MinLoopStage.Comforting);
    }

    public void NotifyMorningReviewStarted()
    {
        SetStage(MinLoopStage.MorningReview);
    }

    public void NotifyReplayCompleted()
    {
        BeginDispositionBriefing(activeReplayResidentId);
    }

    public void ChooseDispositionA()
    {
        SubmitDisposition(MinLoopDispositionChoice.SystemRecommendedA);
    }

    public void ChooseDispositionB()
    {
        SubmitDisposition(MinLoopDispositionChoice.LowInterventionB);
    }

    public void BeginDispositionBriefing(string residentId)
    {
        SetActiveReplayResident(residentId);
        externalDispositionPresenter = false;
        StartFlowRoutine(ReturnToTerminalForDisposition());
    }

    public void BeginExternalDispositionChoice(string residentId)
    {
        StopActiveFlowRoutine();
        SetActiveReplayResident(residentId);
        dispositionSubmitted = false;
        externalDispositionPresenter = true;
        SetStage(MinLoopStage.DispositionChoice);
    }

    public bool SubmitDisposition(MinLoopDispositionChoice choice)
    {
        return ApplyDispositionChoice(choice);
    }

    public void CompleteExternalDisposition()
    {
        if (!externalDispositionPresenter)
        {
            return;
        }

        MarkActiveHouseholdCompleted();
        externalDispositionPresenter = false;
        SetStage(MinLoopStage.Complete);
    }

    public void ContinueToNextResident()
    {
        MarkActiveHouseholdCompleted();
        SetStage(MinLoopStage.Complete);

        if (terminalPresenter != null)
        {
            terminalPresenter.ShowNextResident(CloseTerminal);
        }
    }

    public void CloseTerminal()
    {
        if (terminalPresenter != null)
        {
            terminalPresenter.Close();
        }
    }

    public string GetTerminalInteractionDescription()
    {
        switch (currentStage)
        {
            case MinLoopStage.DispositionChoice:
                return "E  SUBMIT DISPOSITION";
            case MinLoopStage.Complete:
                return "E  VIEW NEXT HOUSEHOLD";
            case MinLoopStage.AccessCard:
            case MinLoopStage.ResidentSummary:
                return "E  ACCESS TERMINAL";
            default:
                return "E  SCAN ID";
        }
    }

    private IEnumerator SwitchToCompanionAndBeginReplay()
    {
        SetStage(MinLoopStage.SwitchingToCompanion);
        PrepareActiveResidentReplayStart();

        if (viewSwitchController != null)
        {
            viewSwitchController.SwitchToCompanion();
            while (viewSwitchController.IsSwitching)
            {
                yield return null;
            }
        }
        else
        {
            Debug.LogWarning("MinLoopFlowController has no ViewSwitchController. Replay will start without switching view.", this);
        }

        SetStage(MinLoopStage.CompanionReplay);

        if (useResidentSpecificReplayControllers && string.Equals(activeReplayResidentId, "17F03", System.StringComparison.OrdinalIgnoreCase))
        {
            if (companion17F03ReplayController != null)
            {
                companion17F03ReplayController.BeginRecordedReplay(this);
            }
            else
            {
                Debug.LogWarning("MinLoopFlowController is set to 17F03, but no HearthCompanion17F03ReplayController is assigned. Returning directly to disposition choice.", this);
                yield return ReturnToTerminalForDisposition();
            }
        }
        else if (useResidentSpecificReplayControllers && string.Equals(activeReplayResidentId, "17F02", System.StringComparison.OrdinalIgnoreCase))
        {
            if (companion17F02ReplayController != null)
            {
                companion17F02ReplayController.BeginReplay(this);
            }
            else
            {
                Debug.LogWarning("MinLoopFlowController is set to 17F02, but no HearthCompanion17F02ReplayController is assigned. Returning directly to disposition choice.", this);
                yield return ReturnToTerminalForDisposition();
            }
        }
        else if (useCompanion17F01ReplayController && companion17F01ReplayController != null)
        {
            companion17F01ReplayController.BeginReplay(this);
        }
        else if (replaySequenceController != null)
        {
            replaySequenceController.BeginReplay(this);
        }
        else
        {
            Debug.LogWarning("MinLoopFlowController has no ReplaySequenceController. Returning directly to disposition choice.", this);
            yield return ReturnToTerminalForDisposition();
        }

        activeFlowRoutine = null;
    }

    private void PrepareActiveResidentReplayStart()
    {
        if (useResidentSpecificReplayControllers && string.Equals(activeReplayResidentId, "17F02", System.StringComparison.OrdinalIgnoreCase))
        {
            if (companion17F02ReplayController != null)
            {
                companion17F02ReplayController.PrepareReplayStart();
            }

            return;
        }

        if (useCompanion17F01ReplayController && companion17F01ReplayController != null)
        {
            companion17F01ReplayController.PrepareReplayStart();
        }
    }

    private IEnumerator ReturnToTerminalForDisposition()
    {
        SetStage(MinLoopStage.ReturningToTerminal);

        if (viewSwitchController != null)
        {
            viewSwitchController.SwitchToHuman();
            while (viewSwitchController.IsSwitching)
            {
                yield return null;
            }
        }

        SetStage(MinLoopStage.DispositionBriefing);
        dispositionSubmitted = false;
        externalDispositionPresenter = false;

        HearthResidentDispositionDialogueSet dialogueSet = FindDispositionDialogueSet(activeReplayResidentId);

        if (tvTerminalController != null)
        {
            tvTerminalController.SetCloseTerminalWhenChoiceSubmitted(false);
            tvTerminalController.SetChoiceInputEnabled(false);
            tvTerminalController.ShowPostReplayChoicePage();
            while (tvTerminalController.IsOpen && !tvTerminalController.IsPresentationReady)
            {
                yield return null;
            }
        }
        else if (terminalPresenter != null)
        {
            terminalPresenter.ShowDispositionChoices(ChooseDispositionA, ChooseDispositionB);
        }

        if (dialogueSet != null)
        {
            yield return PlayDialogue(dialogueSet.PreChoiceBriefing);
        }

        SetStage(MinLoopStage.DispositionChoice);
        if (tvTerminalController != null)
        {
            tvTerminalController.SetChoiceInputEnabled(true);
        }

        activeFlowRoutine = null;
    }

    private bool ApplyDispositionChoice(MinLoopDispositionChoice choice)
    {
        ResolveReferences();

        if (dispositionSubmitted || currentStage != MinLoopStage.DispositionChoice)
        {
            return false;
        }

        dispositionSubmitted = true;
        if (tvTerminalController != null && !externalDispositionPresenter)
        {
            tvTerminalController.SetChoiceInputEnabled(false);
        }
        PlayFeedback(dispositionSubmitFeedback);

        int currentTrust = 0;
        int delta = 0;

        if (trustStateController != null)
        {
            currentTrust = trustStateController.ApplyChoice(choice);
            delta = trustStateController.LastDelta;
        }

        if (dispositionApplied != null)
        {
            dispositionApplied.Invoke(choice, currentTrust, delta);
        }

        if (externalDispositionPresenter)
        {
            SetStage(MinLoopStage.DispositionResult);
            return true;
        }

        if (tvTerminalController != null)
        {
            StartFlowRoutine(DispositionResultRoutine(choice));

            if (terminalPresenter != null)
            {
                terminalPresenter.Close();
            }
        }
        else if (terminalPresenter != null)
        {
            terminalPresenter.ShowDispositionResult(choice, currentTrust, delta, ContinueToNextResident);
        }

        return true;
    }

    private IEnumerator DispositionResultRoutine(MinLoopDispositionChoice choice)
    {
        SetStage(MinLoopStage.DispositionResult);
        HearthResidentDispositionDialogueSet dialogueSet = FindDispositionDialogueSet(activeReplayResidentId);
        if (dialogueSet != null)
        {
            HearthDialogueSequence result = choice == MinLoopDispositionChoice.SystemRecommendedA
                ? dialogueSet.OptionAResult
                : dialogueSet.OptionBResult;
            yield return PlayDialogue(result);
            yield return PlayDialogue(dialogueSet.PostChoiceCommon);
        }

        if (tvTerminalController != null && tvTerminalController.IsOpen)
        {
            tvTerminalController.CloseTerminal();
            while (tvTerminalController.IsOpen)
            {
                yield return null;
            }
        }

        MarkActiveHouseholdCompleted();
        SetStage(MinLoopStage.Complete);
        activeFlowRoutine = null;
    }

    private void MarkActiveHouseholdCompleted()
    {
        ResolveReferences();
        if (householdProgress != null)
        {
            householdProgress.MarkHouseholdCompleted(activeReplayResidentId);
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

    private HearthResidentDispositionDialogueSet FindDispositionDialogueSet(string residentId)
    {
        if (dispositionDialogueSets == null)
        {
            return null;
        }

        for (int i = 0; i < dispositionDialogueSets.Length; i++)
        {
            HearthResidentDispositionDialogueSet candidate = dispositionDialogueSets[i];
            if (candidate != null && candidate.Matches(residentId))
            {
                return candidate;
            }
        }

        return null;
    }

    private void PlayFeedback(InteractionFeedbackController feedback)
    {
        if (feedback != null)
        {
            feedback.PlayFeedback();
        }
    }

    private void SetStage(MinLoopStage nextStage, bool forceNotify = false)
    {
        if (!forceNotify && currentStage == nextStage)
        {
            return;
        }

        currentStage = nextStage;
        if (stageChanged != null)
        {
            stageChanged.Invoke(currentStage);
        }
    }

    private void StartFlowRoutine(IEnumerator routine)
    {
        StopActiveFlowRoutine();
        activeFlowRoutine = StartCoroutine(routine);
    }

    private void StopActiveFlowRoutine()
    {
        if (activeFlowRoutine != null)
        {
            StopCoroutine(activeFlowRoutine);
            activeFlowRoutine = null;
        }
    }

    private void ResolveReferences()
    {
        if (!autoFindMissingReferences)
        {
            return;
        }

        if (terminalPresenter == null)
        {
            terminalPresenter = FindObjectOfType<MinLoopTerminalPresenter>();
        }

        if (tvTerminalController == null)
        {
            tvTerminalController = FindObjectOfType<HearthTvTerminalController>();
        }

        if (viewSwitchController == null)
        {
            viewSwitchController = FindObjectOfType<ViewSwitchController>();
        }

        if (replaySequenceController == null)
        {
            replaySequenceController = FindObjectOfType<ReplaySequenceController>();
        }

        if (companion17F01ReplayController == null)
        {
            companion17F01ReplayController = FindObjectOfType<HearthCompanion17F01ReplayController>();
        }

        if (companion17F02ReplayController == null)
        {
            companion17F02ReplayController = FindObjectOfType<HearthCompanion17F02ReplayController>();
        }

        if (companion17F03ReplayController == null)
        {
            companion17F03ReplayController = FindObjectOfType<HearthCompanion17F03ReplayController>();
            if (companion17F03ReplayController != null)
            {
                companion17F03ReplayController.SetFlowController(this);
            }
        }

        if (trustStateController == null)
        {
            trustStateController = FindObjectOfType<TrustStateController>();
        }

        if (householdProgress == null)
        {
            householdProgress = FindObjectOfType<HearthHouseholdProgressState>();
        }

        if (subtitlePlayer == null)
        {
            subtitlePlayer = FindObjectOfType<MinLoopSubtitlePlayer>(true);
        }
    }

    private static string NormalizeResidentId(string residentId)
    {
        if (string.IsNullOrEmpty(residentId))
        {
            return "17F01";
        }

        string normalized = residentId.Trim().ToUpperInvariant();
        normalized = normalized.Replace("-", string.Empty);
        normalized = normalized.Replace("_", string.Empty);
        normalized = normalized.Replace(" ", string.Empty);

        if (normalized.Contains("17F03") || normalized.Contains("ROOM3"))
        {
            return "17F03";
        }

        if (normalized.Contains("17F02") || normalized.Contains("ROOM2"))
        {
            return "17F02";
        }

        return "17F01";
    }
}
