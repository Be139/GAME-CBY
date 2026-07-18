using UnityEngine;

[DisallowMultipleComponent]
public class HearthLobbyTaskTerminalInteractable : MonoBehaviour, IInteractable, IInteractionAvailability
{
    [SerializeField] private HearthLobbyFlowController flowController;
    [SerializeField] private HearthTvTerminalController terminalController;
    [SerializeField] private string firstAccessDescription = "ACCESS ASSIGNMENT TERMINAL";
    [SerializeField] private string reviewDescription = "REVIEW ASSIGNMENT";

    public bool IsInteractionAvailable
    {
        get
        {
            ResolveReferences();
            return flowController != null && terminalController != null && flowController.CanOpenAssignmentTerminal;
        }
    }

    public void Configure(HearthLobbyFlowController flow, HearthTvTerminalController terminal)
    {
        flowController = flow;
        terminalController = terminal;
    }

    public void Interact()
    {
        ResolveReferences();
        if (!IsInteractionAvailable)
        {
            return;
        }

        terminalController.OpenTerminal();
    }

    public string GetDescription()
    {
        ResolveReferences();
        return flowController != null && flowController.AssignmentLoaded
            ? reviewDescription
            : firstAccessDescription;
    }

    private void ResolveReferences()
    {
        if (terminalController == null)
        {
            terminalController = GetComponentInChildren<HearthTvTerminalController>(true);
        }

        if (flowController == null)
        {
            flowController = FindObjectOfType<HearthLobbyFlowController>(true);
        }
    }
}
