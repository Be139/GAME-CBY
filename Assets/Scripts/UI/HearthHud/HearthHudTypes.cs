using System;

public enum HearthHudPageId
{
    Slide01PersistentActive = 1,
    Slide02SyncTerminal = 2,
    Slide03DoorwaySummary = 3,
    Slide04DoorwayAcquisition = 4,
    Slide05DoorwayDisposition = 5,
    Slide06RobotReplay = 6,
    Slide07PersistentDormant = 7,
    Slide08FinalChoice = 8,
    Slide09WarningFinal = 9,
    Slide10ReturnToWork = 10,
    Slide11WorkspaceQuickMenu = 11,
    Slide12WorkspacePanel = 12,
    Slide13AlertDoorwaySummary = 13,
    Slide14AlertDoorwayAcquisition = 14,
    Slide15AlertFamilyLog = 15,
    Slide16AlertTrustTrend = 16,
    Slide17AlertInspectionHistory = 17,
    Slide18AlertDisposition = 18,
    Slide19IndoorSidePanel = 19,
    Slide20PhotoCard2023 = 20,
    Slide21PhotoCard2026 = 21,
    Slide22ShutdownConfirm = 22,
    Slide23Warning00 = 23,
    Slide24Warning02 = 24
}

public enum HearthHudState
{
    Active,
    Dormant,
    Alert,
    WarningOrange,
    WarningDeepOrange
}

public enum HearthDoorwayTab
{
    ResidentSummary,
    Acquisition,
    FamilyLog,
    TrustTrend,
    InspectionHistory
}

public enum HearthHudButtonActionType
{
    None,
    ShowPage,
    ShowNextPage,
    ShowPreviousPage,
    ShowRobotReplay,
    CompleteRobotReplay,
    SelectDoorwayTab,
    SetHudState,
    SetSubtitle,
    ShowTrustDelta,
    HideCurrentOverlay
}

[Serializable]
public struct HearthHudPageMetadata
{
    public HearthHudPageId pageId;
    public bool showPersistentHud;
    public HearthHudState hudState;
    public string clockText;
    public bool showTask;
    public string taskText;
    public string subtitleText;
}
