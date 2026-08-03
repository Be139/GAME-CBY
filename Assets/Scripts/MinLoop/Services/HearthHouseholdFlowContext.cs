using UnityEngine;

/// <summary>
/// Explicit service/reference bundle shared by household controllers. It owns
/// no story order and no coroutines; each household keeps its current flow.
/// </summary>
[DisallowMultipleComponent]
public sealed class HearthHouseholdFlowContext : MonoBehaviour
{
    [SerializeField] private HearthFirstPersonHudController humanHud;
    [SerializeField] private HearthCompanionHudController companionHud;
    [SerializeField] private MinLoopSubtitlePlayer subtitlePlayer;
    [SerializeField] private HearthPlayerControlLock playerControlLock;
    [SerializeField] private ViewSwitchController viewSwitchController;
    [SerializeField] private HearthPlayerRigService playerRigService;
    [SerializeField] private HearthScreenTransitionService transitionService;
    [SerializeField] private HearthStoryCueService storyCueService;

    public HearthFirstPersonHudController HumanHud { get { return humanHud; } }
    public HearthCompanionHudController CompanionHud { get { return companionHud; } }
    public MinLoopSubtitlePlayer SubtitlePlayer { get { return subtitlePlayer; } }
    public HearthPlayerControlLock PlayerControlLock { get { return playerControlLock; } }
    public ViewSwitchController ViewSwitchController { get { return viewSwitchController; } }
    public HearthPlayerRigService PlayerRigService { get { return playerRigService; } }
    public HearthScreenTransitionService TransitionService { get { return transitionService; } }
    public HearthStoryCueService StoryCueService { get { return storyCueService; } }

    public void SetHumanTask(HearthCurrentTaskId taskId, string residentId = null)
    {
        HearthCurrentTaskRouter.ApplyHuman(humanHud, taskId, residentId);
    }

    public void SetCompanionTask(HearthCurrentTaskId taskId, string residentId = null)
    {
        HearthCurrentTaskRouter.ApplyCompanion(companionHud, taskId, residentId);
    }
}
