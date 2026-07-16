using UnityEngine;

[DisallowMultipleComponent]
public class Hearth17F04HomeUnitInteractable : MonoBehaviour, IInteractable, IInteractionAvailability
{
    [SerializeField] private Hearth17F04FinaleController finaleController;
    [SerializeField] private string description = "E  SHUT DOWN COMPANION UNIT";
    [SerializeField] private bool locallyEnabled = true;

    public bool IsInteractionAvailable
    {
        get { return locallyEnabled && finaleController != null && finaleController.CanBeginUnitShutdown; }
    }

    public void Interact()
    {
        if (IsInteractionAvailable)
        {
            finaleController.BeginUnitShutdown();
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

    public void SetAvailable(bool value)
    {
        locallyEnabled = value;
    }
}
