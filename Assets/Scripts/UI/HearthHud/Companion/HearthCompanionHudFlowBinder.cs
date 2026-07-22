using UnityEngine;

[DisallowMultipleComponent]
public class HearthCompanionHudFlowBinder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HearthCompanionHudController companionHud;
    [SerializeField] private MinLoopFlowController minLoopFlowController;
    [SerializeField] private ViewSwitchController viewSwitchController;
    [SerializeField] private bool autoFindReferences = true;

    [Header("Flow Defaults")]
    [SerializeField] private bool showHudWhenCompanionReplayStarts = true;
    [SerializeField] private string firstReplaySceneId = "17F01_01";
    [SerializeField] private bool hideHudWhenReturningToHuman = true;

    private bool isListening;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();
        Subscribe();
        RefreshFromViewMode();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void LateUpdate()
    {
        RefreshFromViewMode();
    }

    public void BindReplayScene(string sceneId)
    {
        firstReplaySceneId = sceneId;
    }

    public void ShowScene(string sceneId)
    {
        ResolveReferences();
        if (companionHud != null)
        {
            companionHud.ShowScene(sceneId);
        }
    }

    public void NotifyReplayCompleted()
    {
        ResolveReferences();
        if (minLoopFlowController != null)
        {
            minLoopFlowController.NotifyReplayCompleted();
        }

        if (companionHud != null)
        {
            companionHud.NotifyReplayCompleted();
        }
    }

    private void HandleStageChanged(MinLoopStage stage)
    {
        ResolveReferences();
        if (companionHud == null)
        {
            return;
        }

        if (stage == MinLoopStage.CompanionReplay && showHudWhenCompanionReplayStarts)
        {
            companionHud.SetVisible(true);
            companionHud.ShowScene(firstReplaySceneId);
        }
        else if (stage == MinLoopStage.ReturningToTerminal && hideHudWhenReturningToHuman)
        {
            companionHud.SetVisible(false);
        }
    }

    private void RefreshFromViewMode()
    {
        if (companionHud == null)
        {
            ResolveReferences();
        }

        if (companionHud == null || viewSwitchController == null)
        {
            return;
        }

        companionHud.SetViewSwitchController(viewSwitchController);
    }

    private void ResolveReferences()
    {
        if (companionHud == null)
        {
            companionHud = GetComponent<HearthCompanionHudController>();
        }

        if (!autoFindReferences)
        {
            return;
        }

        if (minLoopFlowController == null)
        {
            minLoopFlowController = FindObjectOfType<MinLoopFlowController>();
        }

        if (viewSwitchController == null ||
            !viewSwitchController.enabled ||
            !viewSwitchController.gameObject.activeInHierarchy)
        {
            viewSwitchController = ViewSwitchController.FindPreferredController();
        }
    }

    private void Subscribe()
    {
        if (isListening || minLoopFlowController == null || minLoopFlowController.StageChanged == null)
        {
            return;
        }

        minLoopFlowController.StageChanged.AddListener(HandleStageChanged);
        isListening = true;
    }

    private void Unsubscribe()
    {
        if (!isListening || minLoopFlowController == null || minLoopFlowController.StageChanged == null)
        {
            isListening = false;
            return;
        }

        minLoopFlowController.StageChanged.RemoveListener(HandleStageChanged);
        isListening = false;
    }
}
