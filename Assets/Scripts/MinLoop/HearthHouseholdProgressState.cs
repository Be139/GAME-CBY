using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class HearthHouseholdCompletedEvent : UnityEvent<string>
{
}

[DisallowMultipleComponent]
public class HearthHouseholdProgressState : MonoBehaviour
{
    [SerializeField] private List<string> completedHouseholds = new List<string>();
    [SerializeField] private HearthHouseholdCompletedEvent householdCompleted = new HearthHouseholdCompletedEvent();

    public bool AreFirstThreeCompleted
    {
        get
        {
            return HasHouseholdCompleted("17F01") &&
                   HasHouseholdCompleted("17F02") &&
                   HasHouseholdCompleted("17F03");
        }
    }

    public IReadOnlyList<string> CompletedHouseholds
    {
        get { return completedHouseholds; }
    }

    public HearthHouseholdCompletedEvent HouseholdCompleted
    {
        get { return householdCompleted; }
    }

    public bool HasHouseholdCompleted(string residentId)
    {
        string normalized = NormalizeResidentId(residentId);
        for (int i = 0; i < completedHouseholds.Count; i++)
        {
            if (NormalizeResidentId(completedHouseholds[i]) == normalized)
            {
                return true;
            }
        }

        return false;
    }

    public void MarkHouseholdCompleted(string residentId)
    {
        string normalized = NormalizeResidentId(residentId);
        if (string.IsNullOrEmpty(normalized) || HasHouseholdCompleted(normalized))
        {
            return;
        }

        completedHouseholds.Add(normalized);
        householdCompleted.Invoke(normalized);
    }

    public void ResetProgress()
    {
        completedHouseholds.Clear();
    }

    private static string NormalizeResidentId(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.Trim().ToUpperInvariant().Replace("-", string.Empty).Replace("_", string.Empty).Replace(" ", string.Empty);
    }
}
