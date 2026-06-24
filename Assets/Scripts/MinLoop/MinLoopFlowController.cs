using System.Collections;
using UnityEngine;

public class MinLoopFlowController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MinLoopTerminalPresenter terminalPresenter;
    [SerializeField] private ViewSwitchController viewSwitchController;
    [SerializeField] private ReplaySequenceController replaySequenceController;
    [SerializeField] private TrustStateController trustStateController;
    [SerializeField] private bool autoFindMissingReferences = true;

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
        SetStage(MinLoopStage.Corridor, true);

        if (trustStateController != null)
        {
            trustStateController.ResetTrust();
        }

        if (replaySequenceController != null)
        {
            replaySequenceController.CancelReplay();
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

        if (replaySequenceController != null)
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

        if (terminalPresenter != null)
        {
            terminalPresenter.ShowDispositionChoices(ChooseDispositionA, ChooseDispositionB);
        }

        activeFlowRoutine = null;
    }

    private void ApplyDispositionChoice(MinLoopDispositionChoice choice)
    {
        ResolveReferences();
        PlayFeedback(dispositionSubmitFeedback);

        int currentTrust = 0;
        int delta = 0;

        if (trustStateController != null)
        {
            currentTrust = trustStateController.ApplyChoice(choice);
            delta = trustStateController.LastDelta;
        }

        if (terminalPresenter != null)
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

        if (viewSwitchController == null)
        {
            viewSwitchController = FindObjectOfType<ViewSwitchController>();
        }

        if (replaySequenceController == null)
        {
            replaySequenceController = FindObjectOfType<ReplaySequenceController>();
        }

        if (trustStateController == null)
        {
            trustStateController = FindObjectOfType<TrustStateController>();
        }
    }
}
