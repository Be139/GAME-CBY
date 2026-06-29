using UnityEngine;

[DisallowMultipleComponent]
public class HearthLocationSurface : MonoBehaviour
{
    [SerializeField] private string locationId = "UNKNOWN";
    [SerializeField] private string displayLabel = "UNKNOWN";
    [SerializeField] private int priority;
    [SerializeField] private bool canTriggerHomeWelcome;

    public string LocationId
    {
        get { return locationId; }
    }

    public string DisplayLabel
    {
        get { return displayLabel; }
    }

    public int Priority
    {
        get { return priority; }
    }

    public bool CanTriggerHomeWelcome
    {
        get { return canTriggerHomeWelcome; }
    }

    public void Configure(string newLocationId, string newDisplayLabel, int newPriority, bool newCanTriggerHomeWelcome)
    {
        locationId = newLocationId;
        displayLabel = newDisplayLabel;
        priority = newPriority;
        canTriggerHomeWelcome = newCanTriggerHomeWelcome;
    }
}
