using UnityEngine;

[DisallowMultipleComponent]
public class Hearth17F03UnitInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private HearthCompanion17F03ReplayController controller;
    [SerializeField] private Collider interactionCollider;
    [SerializeField] private string description = "E INSPECT COMPANION UNIT";
    [SerializeField] private bool available;

    private void Awake()
    {
        if (interactionCollider == null)
        {
            interactionCollider = GetComponent<Collider>();
        }

        ApplyAvailability();
    }

    public void Interact()
    {
        if (available && controller != null)
        {
            controller.OpenUnitInspection();
        }
    }

    public string GetDescription()
    {
        return description;
    }

    public void SetController(HearthCompanion17F03ReplayController value)
    {
        controller = value;
    }

    public void SetAvailable(bool value)
    {
        available = value;
        ApplyAvailability();
    }

    private void ApplyAvailability()
    {
        if (interactionCollider != null)
        {
            interactionCollider.enabled = available;
        }
    }
}
