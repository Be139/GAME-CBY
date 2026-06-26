using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HearthHudController : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private RectTransform persistentHudLayer;
    [SerializeField] private RectTransform panelLayer;
    [SerializeField] private RectTransform fullscreenTakeoverLayer;
    [SerializeField] private RectTransform subtitleLayer;
    [SerializeField] private RectTransform debugPreviewLayer;
    [SerializeField] private HearthHudPersistentView persistentView;

    [Header("Pages")]
    [SerializeField] private HearthHudPage[] pages;
    [SerializeField] private HearthHudPageId startingPage = HearthHudPageId.Slide01PersistentActive;
    [SerializeField] private bool showStartingPageOnStart = true;
    [SerializeField] private bool createEventSystemIfMissing = true;

    private readonly Dictionary<HearthHudPageId, HearthHudPage> pageMap = new Dictionary<HearthHudPageId, HearthHudPage>();
    private HearthHudPage currentPage;
    private HearthHudPageId currentPageId;
    private HearthHudPageId previousPageId = HearthHudPageId.Slide01PersistentActive;
    private HearthHudPageId robotReplayReturnPage = HearthHudPageId.Slide05DoorwayDisposition;

    public HearthHudPageId CurrentPageId
    {
        get { return currentPageId; }
    }

    public HearthHudPage CurrentPage
    {
        get { return currentPage; }
    }

    private void Awake()
    {
        EnsureReferences();
        EnsureEventSystem();
        RebuildPageMap();
        HideAllPages();
    }

    private void Start()
    {
        if (showStartingPageOnStart)
        {
            ShowPage(startingPage);
        }
    }

    public void Configure(
        Canvas newCanvas,
        CanvasScaler newCanvasScaler,
        RectTransform newPersistentHudLayer,
        RectTransform newPanelLayer,
        RectTransform newFullscreenTakeoverLayer,
        RectTransform newSubtitleLayer,
        RectTransform newDebugPreviewLayer,
        HearthHudPersistentView newPersistentView,
        HearthHudPage[] newPages)
    {
        canvas = newCanvas;
        canvasScaler = newCanvasScaler;
        persistentHudLayer = newPersistentHudLayer;
        panelLayer = newPanelLayer;
        fullscreenTakeoverLayer = newFullscreenTakeoverLayer;
        subtitleLayer = newSubtitleLayer;
        debugPreviewLayer = newDebugPreviewLayer;
        persistentView = newPersistentView;
        pages = newPages;
        RebuildPageMap();
    }

    public void RefreshPageListFromChildren()
    {
        pages = GetComponentsInChildren<HearthHudPage>(true);
        RebuildPageMap();
    }

    public void ShowPage(HearthHudPageId pageId)
    {
        RebuildPageMap();

        HearthHudPage page;
        if (!pageMap.TryGetValue(pageId, out page) || page == null)
        {
            Debug.LogWarning("[HearthHudController] Page not found: " + pageId);
            return;
        }

        if (currentPage != null && currentPage != page)
        {
            previousPageId = currentPageId;
            currentPage.Hide();
        }

        currentPage = page;
        currentPageId = pageId;
        currentPage.Show();
        RefreshPersistentHudForPage(currentPage);

        if (pageId == HearthHudPageId.Slide06RobotReplay)
        {
            ResetHoldButtons(currentPage);
        }
    }

    public void ShowPage(int pageNumber)
    {
        pageNumber = Mathf.Clamp(pageNumber, 1, 24);
        ShowPage((HearthHudPageId)pageNumber);
    }

    public void ShowNextPage()
    {
        int next = (int)currentPageId + 1;
        if (next > 24)
        {
            next = 1;
        }

        ShowPage(next);
    }

    public void ShowPreviousPage()
    {
        int previous = (int)currentPageId - 1;
        if (previous < 1)
        {
            previous = 24;
        }

        ShowPage(previous);
    }

    public void ShowRobotReplay(HearthHudPageId returnPage)
    {
        robotReplayReturnPage = returnPage;
        ShowPage(HearthHudPageId.Slide06RobotReplay);
    }

    public void ShowRobotReplayToCurrentPage()
    {
        robotReplayReturnPage = currentPageId == HearthHudPageId.Slide06RobotReplay
            ? HearthHudPageId.Slide05DoorwayDisposition
            : currentPageId;
        ShowPage(HearthHudPageId.Slide06RobotReplay);
    }

    public void CompleteRobotReplay()
    {
        ShowPage(robotReplayReturnPage);
    }

    public void HideCurrentOverlay()
    {
        ShowPage(previousPageId);
    }

    public void SetHudState(HearthHudState state)
    {
        if (persistentView != null)
        {
            persistentView.SetHudState(state);
        }
    }

    public void SetSubtitle(string text)
    {
        if (persistentView != null)
        {
            persistentView.SetSubtitle(text);
        }
    }

    public void ShowTrustDelta(int delta)
    {
        if (persistentView != null)
        {
            persistentView.ShowTrustDelta(delta);
        }
    }

    public void ShowTrustDelta(string label)
    {
        if (persistentView != null)
        {
            persistentView.ShowTrustDelta(label);
        }
    }

    public void SelectDoorwayTab(HearthDoorwayTab tab)
    {
        HearthDoorwayTerminalPanel panel = null;

        if (currentPage != null)
        {
            panel = currentPage.DoorwayTerminalPanel;
            if (panel == null)
            {
                panel = currentPage.GetComponentInChildren<HearthDoorwayTerminalPanel>(true);
            }
        }

        if (panel != null)
        {
            panel.SelectTab(tab);
        }
    }

    public void SetStartingPage(HearthHudPageId pageId)
    {
        startingPage = pageId;
    }

    private void EnsureReferences()
    {
        if (canvas == null)
        {
            canvas = GetComponentInChildren<Canvas>(true);
        }

        if (canvas != null && canvasScaler == null)
        {
            canvasScaler = canvas.GetComponent<CanvasScaler>();
        }

        if (persistentView == null)
        {
            persistentView = GetComponentInChildren<HearthHudPersistentView>(true);
        }

        if (pages == null || pages.Length == 0)
        {
            pages = GetComponentsInChildren<HearthHudPage>(true);
        }
    }

    private void EnsureEventSystem()
    {
        if (!createEventSystemIfMissing || EventSystem.current != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        eventSystemObject.transform.SetParent(transform, false);
    }

    private void RebuildPageMap()
    {
        pageMap.Clear();

        if (pages == null)
        {
            return;
        }

        for (int i = 0; i < pages.Length; i++)
        {
            HearthHudPage page = pages[i];
            if (page == null)
            {
                continue;
            }

            pageMap[page.PageId] = page;
        }
    }

    private void HideAllPages()
    {
        if (pages == null)
        {
            return;
        }

        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
            {
                pages[i].Hide();
            }
        }
    }

    private void RefreshPersistentHudForPage(HearthHudPage page)
    {
        if (persistentView == null || page == null)
        {
            return;
        }

        persistentView.SetVisible(page.ShowPersistentHud);
        persistentView.SetHudState(page.HudState);
        persistentView.SetClock(page.ClockText);
        persistentView.SetTask(page.ShowTask, page.TaskText);
        persistentView.SetSubtitle(page.SubtitleText);
    }

    private void ResetHoldButtons(HearthHudPage page)
    {
        if (page == null)
        {
            return;
        }

        HearthHoldToActButton[] holdButtons = page.GetComponentsInChildren<HearthHoldToActButton>(true);
        for (int i = 0; i < holdButtons.Length; i++)
        {
            holdButtons[i].ResetHold();
        }
    }
}
