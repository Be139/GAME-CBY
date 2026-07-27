using System;
using UnityEngine;

public enum HearthActionHintPriority
{
    None = 0,
    InitialTutorial = 10,
    DynamicInteraction = 20,
    Menu = 30,
    ChoiceOrHold = 40,
    Fullscreen = 50,
    Takeover = 60
}

[Serializable]
public struct HearthActionHintItem
{
    [SerializeField] private string keyLabel;
    [SerializeField] private string actionLabel;
    [SerializeField] private bool available;
    [SerializeField] private bool wideKeycap;

    public HearthActionHintItem(
        string keyLabel,
        string actionLabel,
        bool available,
        bool wideKeycap)
    {
        this.keyLabel = keyLabel;
        this.actionLabel = actionLabel;
        this.available = available;
        this.wideKeycap = wideKeycap;
    }

    public HearthActionHintItem(string keyLabel, string actionLabel)
        : this(keyLabel, actionLabel, true, false)
    {
    }

    public string KeyLabel { get { return keyLabel ?? string.Empty; } }
    public string ActionLabel { get { return actionLabel ?? string.Empty; } }
    public bool Available { get { return available; } }
    public bool WideKeycap { get { return wideKeycap; } }
}

[Serializable]
public sealed class HearthActionHintState
{
    [SerializeField] private bool visible;
    [SerializeField] private HearthActionHintPriority priority;
    [SerializeField] private string contextId;
    [SerializeField] private string statusMessage;
    [SerializeField] private HearthActionHintItem[] items = new HearthActionHintItem[0];

    public HearthActionHintState()
    {
    }

    public HearthActionHintState(
        bool visible,
        HearthActionHintPriority priority,
        string contextId,
        string statusMessage,
        params HearthActionHintItem[] items)
    {
        this.visible = visible;
        this.priority = priority;
        this.contextId = contextId;
        this.statusMessage = statusMessage;
        this.items = items ?? new HearthActionHintItem[0];
    }

    public static HearthActionHintState Hidden
    {
        get
        {
            return new HearthActionHintState(
                false,
                HearthActionHintPriority.None,
                string.Empty,
                string.Empty);
        }
    }

    public bool Visible { get { return visible; } }
    public HearthActionHintPriority Priority { get { return priority; } }
    public string ContextId { get { return contextId ?? string.Empty; } }
    public string StatusMessage { get { return statusMessage ?? string.Empty; } }
    public int ItemCount { get { return items != null ? items.Length : 0; } }

    public HearthActionHintItem GetItem(int index)
    {
        if (items == null || index < 0 || index >= items.Length)
        {
            throw new ArgumentOutOfRangeException("index");
        }

        return items[index];
    }
}
