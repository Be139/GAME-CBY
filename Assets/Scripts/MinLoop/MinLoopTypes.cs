using System;
using UnityEngine;
using UnityEngine.Events;

public enum MinLoopStage
{
    Corridor,
    AccessCard,
    ResidentSummary,
    SwitchingToCompanion,
    CompanionReplay,
    WaitingForComfort,
    Comforting,
    MorningReview,
    ReturningToTerminal,
    DispositionBriefing,
    DispositionChoice,
    DispositionResult,
    Complete,
    EnteringResidentUnit,
    ResidentUnitDialogue,
    ResidentUnitInspection,
    ResidentPostReplay
}

public enum MinLoopDispositionChoice
{
    SystemRecommendedA,
    LowInterventionB
}

public enum HearthSubtitleDurationMode
{
    VoiceClipWhenAssigned,
    ManualHold,
    LongerOfVoiceAndManual
}

public enum HearthSubtitleLinePresentationKind
{
    Dialogue,
    TimeCard
}

public enum DialogueChannel
{
    Formal,
    Auxiliary
}

public enum SpeakerSide
{
    Auto,
    Left,
    Right,
    None
}

public enum AdvancePolicy
{
    ManualSpace,
    AudioComplete
}

public enum HearthDialogueLineMode
{
    SequenceDefault,
    Formal,
    Auxiliary,
    AudioOnly
}

public enum HearthDialogueLineAdvancePolicy
{
    SequenceDefault,
    ManualSpace,
    AudioComplete
}

[Serializable]
public class MinLoopStageEvent : UnityEvent<MinLoopStage>
{
}

[Serializable]
public class MinLoopDispositionEvent : UnityEvent<MinLoopDispositionChoice, int, int>
{
}

[Serializable]
public class MinLoopSubtitleLine
{
    public HearthSubtitleLinePresentationKind presentationKind = HearthSubtitleLinePresentationKind.Dialogue;

    [Tooltip("Sequence Default inherits the owning HearthDialogueSequence channel.")]
    public HearthDialogueLineMode dialogueMode = HearthDialogueLineMode.SequenceDefault;

    [Tooltip("Auto places NPC/resident names on the left and Mia/Companion names on the right.")]
    public SpeakerSide speakerSide = SpeakerSide.Auto;

    [Tooltip("Sequence Default inherits the owning HearthDialogueSequence advance policy.")]
    public HearthDialogueLineAdvancePolicy advancePolicy =
        HearthDialogueLineAdvancePolicy.SequenceDefault;

    [Min(0f)]
    public float startDelay;

    public string speaker;

    [TextArea(2, 5)]
    public string text;

    [Min(0f)]
    public float holdSeconds = 2.75f;

    public AudioClip voiceClip;

    [Tooltip("Voice Clip When Assigned follows the clip length and falls back to Hold Seconds when no clip is assigned.")]
    public HearthSubtitleDurationMode durationMode = HearthSubtitleDurationMode.VoiceClipWhenAssigned;

    [Min(0f)]
    [Tooltip("Extra silence after the voice clip before the next subtitle line starts.")]
    public float voiceTailSeconds;
}
