using System;
using UnityEngine;

public enum HearthTerminalNavigationTab
{
    BeforeAcquisition,
    AfterAcquisition
}

public enum HearthTerminalFocusTarget
{
    BeforeAcquisitionTab,
    AfterAcquisitionTab,
    PrimaryAction,
    PageContent
}

public enum HearthTerminalPrimaryActionType
{
    None,
    ReviewArchivedEvent,
    EnterUnit,
    EnterHome,
    Custom
}

[Serializable]
public sealed class HearthTerminalViewState : ISerializationCallbackReceiver
{
    public const string DefaultLockedMessage = "PLEASE WAIT";

    [SerializeField] private bool visible;
    [SerializeField] private string terminalId = "17F01";
    [SerializeField, Min(0)] private int pageIndex;
    [SerializeField, Min(1)] private int pageCount = 1;
    [SerializeField] private HearthTerminalNavigationTab selectedTab;
    [SerializeField] private HearthTerminalFocusTarget focusTarget;
    [SerializeField] private HearthTerminalPrimaryActionType primaryActionType;
    [SerializeField] private string customPrimaryActionLabel;
    [SerializeField] private bool primaryActionLocked;
    [SerializeField] private string lockedMessage = DefaultLockedMessage;
    [SerializeField] private bool canExit = true;

    public bool Visible { get { return visible; } }
    public string TerminalId { get { return terminalId ?? string.Empty; } }
    public int PageIndex { get { return pageIndex; } }
    public int PageCount { get { return pageCount; } }
    public HearthTerminalNavigationTab SelectedTab { get { return selectedTab; } }
    public HearthTerminalFocusTarget FocusTarget { get { return focusTarget; } }
    public HearthTerminalPrimaryActionType PrimaryActionType { get { return primaryActionType; } }
    public bool PrimaryActionVisible { get { return primaryActionType != HearthTerminalPrimaryActionType.None; } }
    public bool PrimaryActionLocked { get { return primaryActionLocked; } }
    public bool PrimaryActionExecutable { get { return Visible && PrimaryActionVisible && !primaryActionLocked; } }
    public bool CanExit { get { return canExit; } }

    public string PrimaryActionLabel
    {
        get
        {
            switch (primaryActionType)
            {
                case HearthTerminalPrimaryActionType.ReviewArchivedEvent:
                    return "REVIEW ARCHIVED EVENT";
                case HearthTerminalPrimaryActionType.EnterUnit:
                    return "ENTER UNIT";
                case HearthTerminalPrimaryActionType.EnterHome:
                    return "ENTER HOME";
                case HearthTerminalPrimaryActionType.Custom:
                    return customPrimaryActionLabel ?? string.Empty;
                default:
                    return string.Empty;
            }
        }
    }

    public string StatusMessage
    {
        get
        {
            if (!primaryActionLocked)
            {
                return string.Empty;
            }

            return string.IsNullOrWhiteSpace(lockedMessage)
                ? DefaultLockedMessage
                : lockedMessage;
        }
    }

    public void SetVisible(bool newVisible)
    {
        visible = newVisible;
    }

    public void SetTerminalId(string newTerminalId)
    {
        terminalId = newTerminalId ?? string.Empty;
    }

    public void SetPage(int newPageIndex, int newPageCount)
    {
        pageCount = Mathf.Max(1, newPageCount);
        pageIndex = Mathf.Clamp(newPageIndex, 0, pageCount - 1);
    }

    public void SetNavigation(
        HearthTerminalNavigationTab newSelectedTab,
        HearthTerminalFocusTarget newFocusTarget)
    {
        selectedTab = newSelectedTab;
        focusTarget = newFocusTarget;
    }

    public void SetPrimaryAction(
        HearthTerminalPrimaryActionType newActionType,
        bool locked,
        string newLockedMessage = DefaultLockedMessage,
        string newCustomActionLabel = "")
    {
        primaryActionType = newActionType;
        primaryActionLocked = locked;
        lockedMessage = newLockedMessage;
        customPrimaryActionLabel = newCustomActionLabel;
    }

    public void SetCanExit(bool value)
    {
        canExit = value;
    }

    public void OnBeforeSerialize()
    {
        NormalizePage();
    }

    public void OnAfterDeserialize()
    {
        NormalizePage();
    }

    private void NormalizePage()
    {
        if (pageCount < 1)
        {
            pageCount = 1;
        }

        if (pageIndex < 0)
        {
            pageIndex = 0;
        }
        else if (pageIndex >= pageCount)
        {
            pageIndex = pageCount - 1;
        }
    }
}
