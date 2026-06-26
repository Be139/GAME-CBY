using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class HearthHudPage : MonoBehaviour
{
    [Header("Page")]
    [SerializeField] private HearthHudPageId pageId = HearthHudPageId.Slide01PersistentActive;
    [SerializeField] private bool showPersistentHud = true;
    [SerializeField] private HearthHudState hudState = HearthHudState.Active;
    [SerializeField] private string clockText = "07:36";
    [SerializeField] private bool showTask;
    [SerializeField] private string taskText = string.Empty;
    [TextArea(1, 3)]
    [SerializeField] private string subtitleText = string.Empty;

    [Header("Bindings")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private HearthDoorwayTerminalPanel doorwayTerminalPanel;

    public HearthHudPageId PageId
    {
        get { return pageId; }
    }

    public bool ShowPersistentHud
    {
        get { return showPersistentHud; }
    }

    public HearthHudState HudState
    {
        get { return hudState; }
    }

    public string ClockText
    {
        get { return clockText; }
    }

    public bool ShowTask
    {
        get { return showTask; }
    }

    public string TaskText
    {
        get { return taskText; }
    }

    public string SubtitleText
    {
        get { return subtitleText; }
    }

    public HearthDoorwayTerminalPanel DoorwayTerminalPanel
    {
        get { return doorwayTerminalPanel; }
    }

    private void Reset()
    {
        EnsureCanvasGroup();
    }

    private void Awake()
    {
        EnsureCanvasGroup();
    }

    public void Configure(
        HearthHudPageId newPageId,
        bool newShowPersistentHud,
        HearthHudState newHudState,
        string newClockText,
        bool newShowTask,
        string newTaskText,
        string newSubtitleText)
    {
        pageId = newPageId;
        showPersistentHud = newShowPersistentHud;
        hudState = newHudState;
        clockText = newClockText;
        showTask = newShowTask;
        taskText = newTaskText;
        subtitleText = newSubtitleText;
        EnsureCanvasGroup();
    }

    public void SetDoorwayTerminalPanel(HearthDoorwayTerminalPanel panel)
    {
        doorwayTerminalPanel = panel;
    }

    public void Show()
    {
        SetVisible(true);
    }

    public void Hide()
    {
        SetVisible(false);
    }

    public void SetVisible(bool visible)
    {
        EnsureCanvasGroup();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        gameObject.SetActive(visible);
    }

    private void EnsureCanvasGroup()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }
}
