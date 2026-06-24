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
    DispositionChoice,
    Complete
}

public enum MinLoopDispositionChoice
{
    SystemRecommendedA,
    LowInterventionB
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
    [Min(0f)]
    public float startDelay;

    public string speaker;

    [TextArea(2, 5)]
    public string text;

    [Min(0f)]
    public float holdSeconds = 2.75f;

    public AudioClip voiceClip;
}
