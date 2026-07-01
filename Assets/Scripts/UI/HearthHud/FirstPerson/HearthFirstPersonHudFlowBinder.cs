using UnityEngine;

[DisallowMultipleComponent]
public class HearthFirstPersonHudFlowBinder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HearthFirstPersonHudController hudController;
    [SerializeField] private MinLoopFlowController flowController;
    [SerializeField] private TrustStateController trustStateController;
    [SerializeField] private bool autoFindReferences = true;

    [Header("Trust Rules")]
    [SerializeField] private bool configureTrustControllerOnAwake = true;
    [SerializeField] private int startingTrustScore;
    [SerializeField] private int minTrustScore = -3;
    [SerializeField] private int maxTrustScore = 3;
    [SerializeField] private int optionADelta = 1;
    [SerializeField] private int optionBDelta = -1;

    [Header("Stage Mapping")]
    [SerializeField] private bool showSyncTerminalOnAccessCard;
    [SerializeField] private bool updateRoundsFromDisposition = true;

    private bool listening;

    private void Awake()
    {
        ResolveReferences();

        if (configureTrustControllerOnAwake && trustStateController != null)
        {
            trustStateController.ConfigureRules(
                startingTrustScore,
                minTrustScore,
                maxTrustScore,
                optionADelta,
                optionBDelta,
                true);
        }

        if (hudController != null)
        {
            hudController.SetTrustScore(startingTrustScore);
            hudController.SetRoundsProgress(0, 3);
        }
    }

    private void OnEnable()
    {
        ResolveReferences();
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void SetReferences(
        HearthFirstPersonHudController hud,
        MinLoopFlowController flow,
        TrustStateController trust)
    {
        Unsubscribe();
        hudController = hud;
        flowController = flow;
        trustStateController = trust;
        Subscribe();
    }

    private void OnStageChanged(MinLoopStage stage)
    {
        if (hudController == null)
        {
            return;
        }

        switch (stage)
        {
            case MinLoopStage.Corridor:
                hudController.SetHudState(HearthFirstPersonHudState.Active);
                hudController.ShowPage(HearthFirstPersonHudPageId.Slide01PersistentHud);
                break;
            case MinLoopStage.AccessCard:
                if (showSyncTerminalOnAccessCard)
                {
                    hudController.ShowPage(HearthFirstPersonHudPageId.Slide04SyncTerminal);
                }
                break;
            case MinLoopStage.Complete:
                hudController.ShowPage(HearthFirstPersonHudPageId.Slide01PersistentHud);
                break;
        }
    }

    private void OnDispositionApplied(MinLoopDispositionChoice choice, int trustValue, int delta)
    {
        if (hudController == null || !updateRoundsFromDisposition)
        {
            return;
        }

        hudController.RecordDisposition(choice, trustValue, delta);
    }

    private void OnTrustChanged(int value)
    {
        if (hudController != null)
        {
            hudController.SetTrustScore(value);
        }
    }

    private void Subscribe()
    {
        if (listening || flowController == null)
        {
            return;
        }

        if (flowController.StageChanged != null)
        {
            flowController.StageChanged.AddListener(OnStageChanged);
        }

        if (flowController.DispositionApplied != null)
        {
            flowController.DispositionApplied.AddListener(OnDispositionApplied);
        }

        if (trustStateController != null && trustStateController.TrustChanged != null)
        {
            trustStateController.TrustChanged.AddListener(OnTrustChanged);
        }

        listening = true;
    }

    private void Unsubscribe()
    {
        if (!listening)
        {
            return;
        }

        if (flowController != null && flowController.StageChanged != null)
        {
            flowController.StageChanged.RemoveListener(OnStageChanged);
        }

        if (flowController != null && flowController.DispositionApplied != null)
        {
            flowController.DispositionApplied.RemoveListener(OnDispositionApplied);
        }

        if (trustStateController != null && trustStateController.TrustChanged != null)
        {
            trustStateController.TrustChanged.RemoveListener(OnTrustChanged);
        }

        listening = false;
    }

    private void ResolveReferences()
    {
        if (!autoFindReferences)
        {
            return;
        }

        if (hudController == null)
        {
            hudController = GetComponent<HearthFirstPersonHudController>();
        }

        if (hudController == null)
        {
            hudController = FindObjectOfType<HearthFirstPersonHudController>();
        }

        if (flowController == null)
        {
            flowController = FindObjectOfType<MinLoopFlowController>();
        }

        if (trustStateController == null)
        {
            trustStateController = FindObjectOfType<TrustStateController>();
        }
    }
}
