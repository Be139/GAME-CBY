using UnityEngine;

[DisallowMultipleComponent]
public class HearthTvTerminalInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private HearthTvTerminalController terminalController;
    [SerializeField] private bool findControllerInChildren = true;
    [SerializeField] private bool toggleWhenAlreadyOpen = true;
    [SerializeField] private string openDescription = "E  ACCESS TERMINAL";
    [SerializeField] private string closeDescription = "E  LEAVE TERMINAL";

    private void Awake()
    {
        ResolveController();
    }

    public void Interact()
    {
        ResolveController();

        if (terminalController == null)
        {
            Debug.LogWarning("[HearthTvTerminalInteractable] No HearthTvTerminalController is assigned.", this);
            return;
        }

        if (terminalController.IsOpen && toggleWhenAlreadyOpen)
        {
            terminalController.CloseTerminal();
            return;
        }

        terminalController.OpenTerminal();
    }

    public string GetDescription()
    {
        ResolveController();

        if (terminalController != null && terminalController.IsOpen && toggleWhenAlreadyOpen)
        {
            return closeDescription;
        }

        return openDescription;
    }

    public void SetTerminalController(HearthTvTerminalController controller)
    {
        terminalController = controller;
    }

    private void ResolveController()
    {
        if (terminalController != null || !findControllerInChildren)
        {
            return;
        }

        terminalController = GetComponentInChildren<HearthTvTerminalController>(true);
        if (terminalController == null)
        {
            terminalController = GetComponentInParent<HearthTvTerminalController>();
        }
    }
}
