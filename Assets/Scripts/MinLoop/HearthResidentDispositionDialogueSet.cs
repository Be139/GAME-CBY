using System;
using UnityEngine;

[Serializable]
public class HearthResidentDispositionDialogueSet
{
    [SerializeField] private string residentId = "17F01";
    [SerializeField] private HearthDialogueSequence preChoiceBriefing;
    [SerializeField] private HearthDialogueSequence optionAResult;
    [SerializeField] private HearthDialogueSequence optionBResult;
    [SerializeField] private HearthDialogueSequence postChoiceCommon;

    public string ResidentId { get { return residentId; } }
    public HearthDialogueSequence PreChoiceBriefing { get { return preChoiceBriefing; } }
    public HearthDialogueSequence OptionAResult { get { return optionAResult; } }
    public HearthDialogueSequence OptionBResult { get { return optionBResult; } }
    public HearthDialogueSequence PostChoiceCommon { get { return postChoiceCommon; } }

    public bool Matches(string value)
    {
        return string.Equals(
            NormalizeResidentId(residentId),
            NormalizeResidentId(value),
            StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeResidentId(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().Replace("-", string.Empty).Replace("_", string.Empty).Replace(" ", string.Empty).ToUpperInvariant();
    }
}
