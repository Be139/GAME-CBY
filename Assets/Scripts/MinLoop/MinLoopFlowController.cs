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
    [SerializeField] private TrustStateController trustStateController;
    [SerializeField] private bool autoFindMissingReferences = true;
    [SerializeField] private bool useCompanion17F01ReplayController = true;
    [SerializeField] private bool useResidentSpecificReplayControllers = true;

    [Header("Active Resident")]
    [SerializeField] private string activeReplayResidentId = "17F01";

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

        if (viewSwitchController != null && viewSwitchController.CurrentMode != ViewSwitchController.ViewMode.Human)
        {
            viewSwitchController.SwitchToHuman();
        }

        if (terminalPresenter != null)
        {
            terminalPresenter.Close();
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
        StartFlowRoutine(ReturnToTerminalForDisposition());
    }

    public void ChooseDispositionA()
    {
        ApplyDispositionChoice(MinLoopDispositionChoice.SystemRecommendedA);
    }

    public void ChooseDispositionB()
    {
        ApplyDispositionChoice(MinLoopDispositionChoice.LowInterventionB);
    }

    public void ContinueToNextResident()
    {
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
                return "E 提交处置意见";
            case MinLoopStage.Complete:
                return "E 查看下一户指引";
            case MinLoopStage.AccessCard:
            case MinLoopStage.ResidentSummary:
                return "E 查看终端";
            default:
                return "E 刷工牌";
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

        if (useResidentSpecificReplayControllers && string.Equals(activeReplayResidentId, "17F02", System.StringComparison.OrdinalIgnoreCase))
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

        SetStage(MinLoopStage.DispositionChoice);
        dispositionSubmitted = false;

        if (tvTerminalController != null)
        {
            tvTerminalController.ShowPostReplayChoicePage();
        }
        else if (terminalPresenter != null)
        {
            terminalPresenter.ShowDispositionChoices(ChooseDispositionA, ChooseDispositionB);
        }

        activeFlowRoutine = null;
    }

    private void ApplyDispositionChoice(MinLoopDispositionChoice choice)
    {
        ResolveReferences();

        if (dispositionSubmitted || currentStage != MinLoopStage.DispositionChoice)
        {
            return;
        }

        dispositionSubmitted = true;
        PlayFeedback(dispositionSubmitFeedback);

        int currentTrust = 0;
        int delta = 0;

        if (trustStateController != null)
        {
            currentTrust = trustStateController.ApplyChoice(choice);
            delta = trustStateController.LastDelta;
        }

        if (tvTerminalController != null)
        {
            SetStage(MinLoopStage.Complete);

            if (terminalPresenter != null)
            {
                terminalPresenter.Close();
            }
        }
        else if (terminalPresenter != null)
        {
            terminalPresenter.ShowDispositionResult(choice, currentTrust, delta, ContinueToNextResident);
        }

        if (dispositionApplied != null)
        {
            dispositionApplied.Invoke(choice, currentTrust, delta);
        }
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

        if (trustStateController == null)
        {
            trustStateController = FindObjectOfType<TrustStateController>();
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
