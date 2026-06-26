using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
public class HearthHudButtonAction : MonoBehaviour
{
    [Header("Action")]
    [SerializeField] private HearthHudController controller;
    [SerializeField] private HearthHudButtonActionType actionType = HearthHudButtonActionType.None;
    [SerializeField] private HearthHudPageId targetPage = HearthHudPageId.Slide01PersistentActive;
    [SerializeField] private HearthHudPageId robotReplayReturnPage = HearthHudPageId.Slide05DoorwayDisposition;
    [SerializeField] private HearthDoorwayTab targetTab = HearthDoorwayTab.ResidentSummary;
    [SerializeField] private HearthHudState targetHudState = HearthHudState.Active;
    [SerializeField] private string subtitleText = string.Empty;

    [Header("Trust Feedback")]
    [SerializeField] private bool showTrustDeltaBeforeAction;
    [SerializeField] private int trustDelta;

    [Header("Extra Event")]
    [SerializeField] private UnityEvent onClickExtra;

    private Button button;
    private UnityAction clickAction;

    private void Awake()
    {
        EnsureButton();
        FindControllerIfMissing();
        Bind();
    }

    private void OnDestroy()
    {
        if (button != null && clickAction != null)
        {
            button.onClick.RemoveListener(clickAction);
        }
    }

    public void Configure(
        HearthHudController newController,
        HearthHudButtonActionType newActionType,
        HearthHudPageId newTargetPage,
        HearthHudPageId newRobotReplayReturnPage,
        HearthDoorwayTab newTargetTab,
        HearthHudState newTargetHudState,
        string newSubtitleText,
        bool newShowTrustDeltaBeforeAction,
        int newTrustDelta)
    {
        controller = newController;
        actionType = newActionType;
        targetPage = newTargetPage;
        robotReplayReturnPage = newRobotReplayReturnPage;
        targetTab = newTargetTab;
        targetHudState = newTargetHudState;
        subtitleText = newSubtitleText;
        showTrustDeltaBeforeAction = newShowTrustDeltaBeforeAction;
        trustDelta = newTrustDelta;
        Bind();
    }

    public void InvokeAction()
    {
        FindControllerIfMissing();

        if (showTrustDeltaBeforeAction && controller != null)
        {
            controller.ShowTrustDelta(trustDelta);
        }

        if (controller != null)
        {
            switch (actionType)
            {
                case HearthHudButtonActionType.ShowPage:
                    controller.ShowPage(targetPage);
                    break;
                case HearthHudButtonActionType.ShowNextPage:
                    controller.ShowNextPage();
                    break;
                case HearthHudButtonActionType.ShowPreviousPage:
                    controller.ShowPreviousPage();
                    break;
                case HearthHudButtonActionType.ShowRobotReplay:
                    controller.ShowRobotReplay(robotReplayReturnPage);
                    break;
                case HearthHudButtonActionType.CompleteRobotReplay:
                    controller.CompleteRobotReplay();
                    break;
                case HearthHudButtonActionType.SelectDoorwayTab:
                    controller.SelectDoorwayTab(targetTab);
                    break;
                case HearthHudButtonActionType.SetHudState:
                    controller.SetHudState(targetHudState);
                    break;
                case HearthHudButtonActionType.SetSubtitle:
                    controller.SetSubtitle(subtitleText);
                    break;
                case HearthHudButtonActionType.ShowTrustDelta:
                    controller.ShowTrustDelta(trustDelta);
                    break;
                case HearthHudButtonActionType.HideCurrentOverlay:
                    controller.HideCurrentOverlay();
                    break;
            }
        }

        if (onClickExtra != null)
        {
            onClickExtra.Invoke();
        }
    }

    private void Bind()
    {
        EnsureButton();

        if (button == null)
        {
            return;
        }

        if (clickAction != null)
        {
            button.onClick.RemoveListener(clickAction);
        }

        clickAction = InvokeAction;
        button.onClick.AddListener(clickAction);
    }

    private void EnsureButton()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }
    }

    private void FindControllerIfMissing()
    {
        if (controller != null)
        {
            return;
        }

        controller = GetComponentInParent<HearthHudController>();
        if (controller == null)
        {
            controller = FindObjectOfType<HearthHudController>();
        }
    }
}
