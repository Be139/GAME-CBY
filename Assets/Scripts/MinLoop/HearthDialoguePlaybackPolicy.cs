using System;
using System.Collections.Generic;

/// <summary>
/// Versioned, line-ID based playback rules for the small set of dialogue that
/// intentionally does not use the default framed, Space-to-continue flow.
/// Keeping this policy independent from speaker names prevents a Mia line in a
/// real conversation from being mistaken for an automatic inner caption.
/// </summary>
public static class HearthDialoguePlaybackPolicy
{
    public const string Version = "HEARTH_V2_DIALOGUE_POLICY_2026_08_01";

    private static readonly HashSet<string> NaturalAutomaticLineIds =
        new HashSet<string>(StringComparer.Ordinal)
        {
            "Lobby_OpeningBriefing_Mia_001",
            "Lobby_OpeningCloseout_Mia_001",
            "Lobby_OpeningCloseout_Mia_002",
            "Lobby_Group01_MiaExit_Mia_001",
            "Lobby_Group02_MiaExit_Mia_001",
            "Lobby_Group03_MiaExit_Mia_001",
            "Lobby_AssignmentLoaded_Mia_001",
            "Lobby_ElevatorRide_Mia_001",
            "Lobby_ElevatorRide_Mia_002",
            "Lobby_ElevatorRide_Mia_003",
            "17F04_HomeGreeting_High_17F04_HomeGreeting_Low_Mia_001"
        };

    private static readonly HashSet<string> AutomaticEpilogueSequenceIds =
        new HashSet<string>(StringComparer.Ordinal)
        {
            "17F04_Epilogue_High_Shutdown",
            "17F04_Epilogue_Low_Shutdown",
            "17F04_Epilogue_High_Retain",
            "17F04_Epilogue_Low_Retain"
        };

    private static readonly HashSet<string> DedicatedMessageLineIds =
        new HashSet<string>(StringComparer.Ordinal)
        {
            "17F04_HomeGreeting_High_17F04_HomeGreeting_Low_Lily_001"
        };

    public static IEnumerable<string> NaturalAutomaticLines
    {
        get { return NaturalAutomaticLineIds; }
    }

    public static IEnumerable<string> AutomaticEpilogueSequences
    {
        get { return AutomaticEpilogueSequenceIds; }
    }

    public static IEnumerable<string> DedicatedMessageLines
    {
        get { return DedicatedMessageLineIds; }
    }

    public static bool IsNaturalAutomaticLine(string lineId)
    {
        return !string.IsNullOrWhiteSpace(lineId) &&
               NaturalAutomaticLineIds.Contains(lineId.Trim());
    }

    public static bool IsAutomaticEpilogueSequence(string sequenceId)
    {
        return !string.IsNullOrWhiteSpace(sequenceId) &&
               AutomaticEpilogueSequenceIds.Contains(sequenceId.Trim());
    }

    public static bool IsAutomatic(string sequenceId, string lineId)
    {
        return IsAutomaticEpilogueSequence(sequenceId) ||
               IsNaturalAutomaticLine(lineId);
    }

    public static bool IsDedicatedMessageLine(string lineId)
    {
        return !string.IsNullOrWhiteSpace(lineId) &&
               DedicatedMessageLineIds.Contains(lineId.Trim());
    }

    public static void Apply(
        string sequenceId,
        string lineId,
        ref HearthSubtitleLinePresentationKind presentationKind,
        ref HearthDialogueLineAdvancePolicy advancePolicy)
    {
        if (presentationKind == HearthSubtitleLinePresentationKind.TimeCard)
        {
            advancePolicy = HearthDialogueLineAdvancePolicy.AudioComplete;
            return;
        }

        if (!IsAutomatic(sequenceId, lineId))
        {
            return;
        }

        presentationKind = HearthSubtitleLinePresentationKind.NaturalCaption;
        advancePolicy = HearthDialogueLineAdvancePolicy.AudioComplete;
    }

    public static void Apply(
        string sequenceId,
        string lineId,
        ref HearthSubtitleLinePresentationKind presentationKind,
        ref HearthDialogueLineAdvancePolicy advancePolicy,
        ref HearthDialogueLineMode dialogueMode)
    {
        Apply(
            sequenceId,
            lineId,
            ref presentationKind,
            ref advancePolicy);

        if (IsDedicatedMessageLine(lineId))
        {
            dialogueMode = HearthDialogueLineMode.DedicatedMessage;
            advancePolicy = HearthDialogueLineAdvancePolicy.ManualSpace;
        }
    }
}
