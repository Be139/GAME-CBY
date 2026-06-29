using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
public class HearthFirstPersonHudButtonAction : MonoBehaviour
{
    [Header("Action")]
    [SerializeField] private HearthFirstPersonHudController controller;
    [SerializeField] private HearthFirstPersonHudActionType actionType = HearthFirstPersonHudActionType.None;
    [SerializeField] private HearthFirstPersonHudPageId targetPage = HearthFirstPersonHudPageId.None;

    private Button button;

    private void Awake()
    {
        EnsureButton();
        FindControllerIfMissing();
        button.onClick.AddListener(InvokeAction);
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(InvokeAction);
        }
    }

    public void Configure(
        HearthFirstPersonHudController newController,
        HearthFirstPersonHudActionType newActionType,
        HearthFirstPersonHudPageId newTargetPage)
    {
        controller = newController;
        actionType = newActionType;
        targetPage = newTargetPage;
    }

    public void InvokeAction()
    {
        FindControllerIfMissing();
        if (controller == null)
        {
            return;
        }

        switch (actionType)
        {
            case HearthFirstPersonHudActionType.ShowPage:
                controller.ShowPage(targetPage);
                break;
            case HearthFirstPersonHudActionType.HideOverlay:
                controller.HideOverlay();
                break;
            case HearthFirstPersonHudActionType.ConfirmSync:
                controller.ConfirmSyncTerminal();
                break;
            case HearthFirstPersonHudActionType.OpenTodayRounds:
                controller.OpenTodayRounds();
                break;
            case HearthFirstPersonHudActionType.OpenDispositionHistory:
                controller.OpenDispositionHistory();
                break;
            case HearthFirstPersonHudActionType.OpenSettings:
                controller.OpenSettings();
                break;
            case HearthFirstPersonHudActionType.ShowFinalChoice:
                controller.ShowFinalChoice(false);
                break;
            case HearthFirstPersonHudActionType.ChooseFinalA:
                controller.ChooseFinalA();
                break;
            case HearthFirstPersonHudActionType.ChooseFinalB:
                controller.ChooseFinalB();
                break;
            case HearthFirstPersonHudActionType.ConfirmGracefulShutdown:
                controller.ConfirmGracefulShutdown();
                break;
            case HearthFirstPersonHudActionType.CancelShutdown:
                controller.CancelShutdownDecision();
                break;
            case HearthFirstPersonHudActionType.ContinueWarning:
                controller.ContinueWarning();
                break;
            case HearthFirstPersonHudActionType.CancelWarning:
                controller.CancelWarning();
                break;
            case HearthFirstPersonHudActionType.ConfirmExit:
                controller.ConfirmExitGame();
                break;
            case HearthFirstPersonHudActionType.CancelExit:
                controller.CancelExitGame();
                break;
        }
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
        if (controller == null)
        {
            controller = GetComponentInParent<HearthFirstPersonHudController>();
        }

        if (controller == null)
        {
            controller = FindObjectOfType<HearthFirstPersonHudController>();
        }
    }
}
