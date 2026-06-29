using System;
using UnityEngine;
using UnityEngine.Events;

public enum HearthFirstPersonHudPageId
{
    None = 0,
    Slide01PersistentHud = 1,
    Slide02TrustDelta = 2,
    Slide03MainMenu = 3,
    Slide04SyncTerminal = 4,
    Slide05TodayRounds = 5,
    Slide06HomeWelcome = 6,
    Slide07Photo2023 = 7,
    Slide08Photo2026 = 8,
    Slide09FinalChoice = 9,
    Slide10ShutdownConfirm = 10,
    Slide11Warning01 = 11,
    Slide12Warning02 = 12,
    Slide13Warning03 = 13,
    Slide14FinalChoiceReturn = 14,
    Slide15EndingGraceful = 15,
    Slide16EndingForced = 16,
    Slide17EndingCompanion = 17,
    Slide18HistoryEmpty = 18,
    Slide19HistoryOne = 19,
    Slide20HistoryTwo = 20,
    Slide21HistoryThree = 21,
    Slide22Settings = 22,
    Slide23SettingsFocus = 23,
    Slide24ExitConfirm = 24
}

public enum HearthFirstPersonHudState
{
    Active,
    Dormant,
    AlertPendingReview,
    AlertHighRisk
}

public enum HearthFirstPersonEndingPath
{
    GracefulA,
    ForcedA,
    CompanionB
}

public enum HearthFirstPersonHudActionType
{
    None,
    ShowPage,
    HideOverlay,
    ConfirmSync,
    OpenTodayRounds,
    OpenDispositionHistory,
    OpenSettings,
    ShowFinalChoice,
    ChooseFinalA,
    ChooseFinalB,
    ConfirmGracefulShutdown,
    CancelShutdown,
    ContinueWarning,
    CancelWarning,
    ConfirmExit,
    CancelExit
}

[Serializable]
public class HearthDispositionRecord
{
    public string timestamp = "2026.09.15 · 18:47";
    public string unitId = "17F-01";
    public string actionLabel = "Approve Upgrade · Deep Night Companion Pro";
    public string statusLabel = "RECOMMENDED";
    public int trustDelta = 1;
}

[Serializable]
public class HearthFirstPersonHudPageEvent : UnityEvent<HearthFirstPersonHudPageId>
{
}

[Serializable]
public class HearthFirstPersonEndingEvent : UnityEvent<HearthFirstPersonEndingPath>
{
}

[Serializable]
public class HearthTrustDeltaEvent : UnityEvent<int>
{
}

[Serializable]
public class HearthSettingsVolumeEvent : UnityEvent<string, int>
{
}
