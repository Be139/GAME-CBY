using UnityEngine;

[DisallowMultipleComponent]
public class Hearth17F04RoomDoorInteractable : MonoBehaviour, IInteractable, IInteractionAvailability
{
    [SerializeField] private Hearth17F04FinaleController finaleController;
    [SerializeField] private string description = "E  ENTER LILY'S ROOM";

    public bool IsInteractionAvailable
    {
        get { return finaleController != null && finaleController.CanEnterDaughterRoom; }
    }

    public void Interact()
    {
        if (IsInteractionAvailable)
        {
            finaleController.EnterDaughterRoom();
        }
    }

    public string GetDescription()
    {
        return description;
    }

    public void SetController(Hearth17F04FinaleController value)
    {
        finaleController = value;
    }
}
