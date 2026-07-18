using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class HearthLobbyElevatorInteractable : MonoBehaviour, IInteractable, IInteractionAvailability
{
    [SerializeField] private HearthLobbyFlowController flowController;
    [SerializeField] private string description = "CALL ELEVATOR";

    public bool IsInteractionAvailable
    {
        get
        {
            ResolveFlow();
            return flowController != null && flowController.CanUseElevator;
        }
    }

    private void Reset()
    {
        Collider interactionCollider = GetComponent<Collider>();
        if (interactionCollider != null)
        {
            interactionCollider.isTrigger = false;
        }
    }

    public void Configure(HearthLobbyFlowController flow)
    {
        flowController = flow;
    }

    public void Interact()
    {
        ResolveFlow();
        if (IsInteractionAvailable)
        {
            flowController.BeginElevatorRide();
        }
    }

    public string GetDescription()
    {
        return description;
    }

    private void ResolveFlow()
    {
        if (flowController == null)
        {
            flowController = FindObjectOfType<HearthLobbyFlowController>(true);
        }
    }
}
