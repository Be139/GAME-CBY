using UnityEngine;

[DisallowMultipleComponent]
public class Hearth17F03GazeInteractable : MonoBehaviour
{
    public enum Target
    {
        Daughter,
        Mother
    }

    [SerializeField] private HearthCompanion17F03ReplayController controller;
    [SerializeField] private Target target;
    [SerializeField] private Collider interactionCollider;
    [SerializeField] private string daughterDescription = "E SPEAK FOR MOTHER";
    [SerializeField] private string motherDescription = "E SPEAK FOR DAUGHTER";
    [SerializeField] private bool available;

    public Target TargetType { get { return target; } }
    public bool IsAvailable { get { return available; } }

    private void Awake()
    {
        if (interactionCollider == null)
        {
            interactionCollider = GetComponent<Collider>();
        }

        ApplyAvailability();
    }

    public string GetDescription()
    {
        return target == Target.Daughter ? daughterDescription : motherDescription;
    }

    public void Configure(HearthCompanion17F03ReplayController newController, Target newTarget)
    {
        controller = newController;
        target = newTarget;
    }

    public void SetAvailable(bool value)
    {
        available = value;
        ApplyAvailability();
    }

    public bool CanInteract(Camera viewCamera, float maxDistance)
    {
        if (!available || viewCamera == null || interactionCollider == null || !interactionCollider.enabled)
        {
            return false;
        }

        Ray centerRay = viewCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        return interactionCollider.Raycast(centerRay, out hit, Mathf.Max(0.1f, maxDistance));
    }

    private void ApplyAvailability()
    {
        if (interactionCollider != null)
        {
            interactionCollider.enabled = available;
        }
    }
}
