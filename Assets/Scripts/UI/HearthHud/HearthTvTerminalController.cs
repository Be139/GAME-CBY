using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HearthTvTerminalController : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private bool createEventSystemIfMissing = true;

    [Header("Pages")]
    [SerializeField] private HearthHudPage[] pages;
    [SerializeField] private int firstSlideNumber = 1;
    [SerializeField] private HearthHudPageId startingPage = HearthHudPageId.Slide01PersistentActive;
    [SerializeField] private bool showStartingPageOnStart = true;
    [SerializeField] private bool refreshPagesFromChildrenOnAwake = true;

    [Header("Focus Camera")]
    [SerializeField] private bool switchCameraWhileOpen;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera terminalCamera;

    [Header("Player Lock")]
    [SerializeField] private bool lockGameplayWhileOpen = true;
    [SerializeField] private Behaviour[] gameplayBehavioursToDisable;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private bool unlockCursorWhileOpen = true;

    [Header("Scale")]
    [SerializeField] private float zoom = 1f;

    [Header("Keyboard Navigation")]
    [SerializeField] private bool keyboardNavigationEnabled = true;
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;
    [SerializeField] private KeyCode cyclePageKey = KeyCode.Tab;
    [SerializeField] private KeyCode previousSelectionKey = KeyCode.LeftArrow;
    [SerializeField] private KeyCode nextSelectionKey = KeyCode.RightArrow;
    [SerializeField] private KeyCode submitKey = KeyCode.Space;
    [SerializeField] private int keyboardCyclePageCount = 5;
    [SerializeField] private TMP_Text keyboardHintText;
    [SerializeField] private TMP_Text keyboardFocusText;
    [SerializeField] private string pageFocusFormat = "PAGE {0}/{1}";
    [SerializeField] private string replayFocusLabel = "RECALL EVENT | SPACE";
    [SerializeField] private string keyboardHintLabel = "TAB NEXT PAGE     LEFT/RIGHT SELECT     SPACE CONFIRM     ESC EXIT";

    [Header("Robot Replay")]
    [SerializeField] private MinLoopFlowController minLoopFlowController;
    [SerializeField] private ViewSwitchController viewSwitchController;
    [SerializeField] private bool closeTerminalWhenReplayStarts = true;
    [SerializeField] private bool showFinalChoiceWhenReplayUnavailable = true;
    [SerializeField] private UnityEvent onRobotReplayRequested;
    [SerializeField] private UnityEvent onPostReplayChoiceShown;

    [Header("Events")]
    [SerializeField] private UnityEvent onOpened;
    [SerializeField] private UnityEvent onClosed;
    [SerializeField] private UnityEvent onPageChanged;

    private readonly Dictionary<HearthHudPageId, HearthHudPage> pageMap = new Dictionary<HearthHudPageId, HearthHudPage>();
    private HearthHudPage currentPage;
    private HearthHudPageId currentPageId;
    private int currentPageIndex = -1;
    private bool[] gameplayWasEnabled;
    private bool playerInteractionWasEnabled;
    private CursorLockMode previousCursorLockState;
    private bool previousCursorVisible;
    private bool playerCameraWasEnabled;
    private bool terminalCameraWasEnabled;
    private bool playerAudioWasEnabled;
    private bool terminalAudioWasEnabled;
    private int keyboardFocusIndex;

    public bool IsOpen { get; private set; }

    public HearthHudPage CurrentPage
    {
        get { return currentPage; }
    }

    public HearthHudPageId CurrentPageId
    {
        get { return currentPageId; }
    }

    public RectTransform ContentRoot
    {
        get { return contentRoot; }
    }

    public float Zoom
    {
        get { return zoom; }
    }

    public Camera TerminalCamera
    {
        get { return terminalCamera; }
    }

    private void Reset()
    {
        EnsureReferences();
        ApplyZoom();
    }

    private void Awake()
    {
        EnsureReferences();

        if (refreshPagesFromChildrenOnAwake || pages == null || pages.Length == 0)
        {
            RefreshPageListFromChildren();
        }
        else
        {
            RebuildPageMap();
        }

        ApplyZoom();
        HideAllPages();
        SetTerminalInputEnabled(false);
    }

    private void Start()
    {
        if (showStartingPageOnStart)
        {
            ShowPage(startingPage);
        }
    }

    private void OnValidate()
    {
        zoom = Mathf.Max(0.1f, zoom);
        keyboardCyclePageCount = Mathf.Max(1, keyboardCyclePageCount);
        ApplyZoom();
    }

    private void Update()
    {
        if (!IsOpen)
        {
            return;
        }

        if (Input.GetKeyDown(closeKey))
        {
            CloseTerminal();
            return;
        }

        if (!keyboardNavigationEnabled)
        {
            return;
        }

        if (Input.GetKeyDown(cyclePageKey))
        {
            CycleNormalPage(1);
        }

        if (Input.GetKeyDown(previousSelectionKey))
        {
            MoveKeyboardFocus(-1);
        }

        if (Input.GetKeyDown(nextSelectionKey))
        {
            MoveKeyboardFocus(1);
        }

        if (Input.GetKeyDown(submitKey))
        {
            SubmitKeyboardFocus();
        }
    }

    public void Configure(
        Canvas newCanvas,
        CanvasScaler newCanvasScaler,
        RectTransform newContentRoot,
        CanvasGroup newCanvasGroup,
        HearthHudPage[] newPages,
        int newFirstSlideNumber,
        HearthHudPageId newStartingPage,
        float newZoom)
    {
        canvas = newCanvas;
        canvasScaler = newCanvasScaler;
        contentRoot = newContentRoot;
        canvasGroup = newCanvasGroup;
        pages = newPages;
        firstSlideNumber = Mathf.Max(1, newFirstSlideNumber);
        startingPage = newStartingPage;
        zoom = Mathf.Max(0.1f, newZoom);
        SortPagesByPageId();
        RebuildPageMap();
        ApplyZoom();
    }

    public void RefreshPageListFromChildren()
    {
        RectTransform searchRoot = contentRoot != null ? contentRoot : transform as RectTransform;
        pages = searchRoot != null
            ? searchRoot.GetComponentsInChildren<HearthHudPage>(true)
            : GetComponentsInChildren<HearthHudPage>(true);
        SortPagesByPageId();
        RebuildPageMap();
    }

    public void OpenTerminal()
    {
        EnsureReferences();
        EnsureEventSystem();

        if (IsOpen)
        {
            return;
        }

        IsOpen = true;
        previousCursorLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        SetTerminalInputEnabled(true);
        ApplyCameraFocus(true);
        SetGameplayLocked(true);

        if (unlockCursorWhileOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (currentPage == null && showStartingPageOnStart)
        {
            ShowPage(startingPage);
        }
        else
        {
            SyncKeyboardFocusToCurrentPage();
            RefreshKeyboardHint();
        }

        if (onOpened != null)
        {
            onOpened.Invoke();
        }
    }

    public void CloseTerminal()
    {
        if (!IsOpen)
        {
            return;
        }

        IsOpen = false;
        SetTerminalInputEnabled(false);
        SetGameplayLocked(false);
        ApplyCameraFocus(false);

        if (unlockCursorWhileOpen)
        {
            Cursor.lockState = previousCursorLockState;
            Cursor.visible = previousCursorVisible;
        }

        if (onClosed != null)
        {
            onClosed.Invoke();
        }
    }

    public void ShowPage(HearthHudPageId pageId)
    {
        RebuildPageMap();

        HearthHudPage page;
        if (!pageMap.TryGetValue(pageId, out page) || page == null)
        {
            Debug.LogWarning("[HearthTvTerminalController] Page not found on this TV terminal: " + pageId, this);
            return;
        }

        ShowPage(page);
    }

    public void ShowPage(int slideNumber)
    {
        if (slideNumber < 1)
        {
            return;
        }

        ShowPage((HearthHudPageId)slideNumber);
    }

    public void ShowNextPage()
    {
        if (pages == null || pages.Length == 0)
        {
            return;
        }

        int next = currentPageIndex + 1;
        if (next < 0 || next >= pages.Length)
        {
            next = 0;
        }

        ShowPage(pages[next]);
    }

    public void ShowPreviousPage()
    {
        if (pages == null || pages.Length == 0)
        {
            return;
        }

        int previous = currentPageIndex - 1;
        if (previous < 0)
        {
            previous = pages.Length - 1;
        }

        ShowPage(pages[previous]);
    }

    public void SelectDoorwayTab(HearthDoorwayTab tab)
    {
        ShowPage(firstSlideNumber + GetTabOffset(tab));
    }

    public void SetZoom(float newZoom)
    {
        zoom = Mathf.Max(0.1f, newZoom);
        ApplyZoom();
    }

    public void SetWorldCamera(Camera camera)
    {
        worldCamera = camera;
        if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
        {
            canvas.worldCamera = worldCamera;
        }
    }

    public void SetTerminalCamera(Camera camera)
    {
        terminalCamera = camera;
        SetWorldCamera(camera);
    }

    public void SetPlayerCamera(Camera camera)
    {
        playerCamera = camera;
    }

    public void SetSwitchCameraWhileOpen(bool shouldSwitch)
    {
        switchCameraWhileOpen = shouldSwitch;
    }

    public void SetMinLoopFlowController(MinLoopFlowController controller)
    {
        minLoopFlowController = controller;
    }

    public void SetViewSwitchController(ViewSwitchController controller)
    {
        viewSwitchController = controller;
    }

    public void RequestRobotReplay()
    {
        if (onRobotReplayRequested != null)
        {
            onRobotReplayRequested.Invoke();
        }

        if (minLoopFlowController != null)
        {
            if (closeTerminalWhenReplayStarts)
            {
                CloseTerminal();
            }

            minLoopFlowController.RequestReplayFromTerminal();
            return;
        }

        if (viewSwitchController != null)
        {
            if (closeTerminalWhenReplayStarts)
            {
                CloseTerminal();
            }

            viewSwitchController.SwitchToCompanion();
            return;
        }

        if (showFinalChoiceWhenReplayUnavailable)
        {
            ShowPostReplayChoicePage();
            return;
        }

        Debug.LogWarning("[HearthTvTerminalController] Robot replay requested, but no MinLoopFlowController or ViewSwitchController is assigned.", this);
    }

    public void ShowPostReplayChoicePage()
    {
        if (!IsOpen)
        {
            OpenTerminal();
        }

        int finalPageIndex = GetPostReplayChoicePageIndex();
        if (finalPageIndex >= 0 && pages != null && finalPageIndex < pages.Length)
        {
            ShowPage(pages[finalPageIndex]);
        }

        if (onPostReplayChoiceShown != null)
        {
            onPostReplayChoiceShown.Invoke();
        }
    }

    private void ShowPage(HearthHudPage page)
    {
        if (page == null)
        {
            return;
        }

        if (currentPage != null && currentPage != page)
        {
            currentPage.Hide();
        }

        currentPage = page;
        currentPageId = page.PageId;
        currentPageIndex = FindPageIndex(page);
        currentPage.Show();
        SyncKeyboardFocusToCurrentPage();
        RefreshKeyboardHint();

        if (onPageChanged != null)
        {
            onPageChanged.Invoke();
        }
    }

    private void EnsureReferences()
    {
        if (canvas == null)
        {
            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = GetComponentInParent<Canvas>();
            }
        }

        if (canvas != null)
        {
            if (canvasScaler == null)
            {
                canvasScaler = canvas.GetComponent<CanvasScaler>();
            }

            if (worldCamera == null)
            {
                worldCamera = canvas.worldCamera;
            }
        }

        if (contentRoot == null)
        {
            Transform found = transform.Find("TerminalContentRoot");
            if (found == null)
            {
                found = transform.Find("TerminalScreenRoot/TerminalContentRoot");
            }

            contentRoot = found as RectTransform;
        }

        if (keyboardHintText == null)
        {
            Transform foundHint = transform.Find("KeyboardNavigationRoot/KeyboardHintText");
            keyboardHintText = foundHint != null ? foundHint.GetComponent<TMP_Text>() : null;
        }

        if (keyboardFocusText == null)
        {
            Transform foundFocus = transform.Find("KeyboardNavigationRoot/KeyboardFocusText");
            keyboardFocusText = foundFocus != null ? foundFocus.GetComponent<TMP_Text>() : null;
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
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

    private void SortPagesByPageId()
    {
        if (pages == null)
        {
            return;
        }

        Array.Sort(pages, ComparePages);
    }

    private static int ComparePages(HearthHudPage left, HearthHudPage right)
    {
        if (ReferenceEquals(left, right))
        {
            return 0;
        }

        if (left == null)
        {
            return 1;
        }

        if (right == null)
        {
            return -1;
        }

        return ((int)left.PageId).CompareTo((int)right.PageId);
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

    private void SetTerminalInputEnabled(bool enabled)
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.interactable = enabled;
        canvasGroup.blocksRaycasts = enabled;
    }

    private void ApplyZoom()
    {
        if (contentRoot != null)
        {
            contentRoot.localScale = Vector3.one * Mathf.Max(0.1f, zoom);
        }
    }

    private void ApplyCameraFocus(bool focused)
    {
        if (!switchCameraWhileOpen)
        {
            if (focused && terminalCamera != null)
            {
                SetWorldCamera(terminalCamera);
            }

            return;
        }

        if (focused)
        {
            playerCameraWasEnabled = playerCamera != null && playerCamera.enabled;
            terminalCameraWasEnabled = terminalCamera != null && terminalCamera.enabled;
            playerAudioWasEnabled = GetAudioListenerEnabled(playerCamera);
            terminalAudioWasEnabled = GetAudioListenerEnabled(terminalCamera);

            if (playerCamera != null)
            {
                SetCameraAndAudioEnabled(playerCamera, false);
            }

            if (terminalCamera != null)
            {
                SetCameraAndAudioEnabled(terminalCamera, true);
                SetWorldCamera(terminalCamera);
            }
        }
        else
        {
            if (playerCamera != null)
            {
                SetCameraAndAudioEnabled(playerCamera, playerCameraWasEnabled, playerAudioWasEnabled);
            }

            if (terminalCamera != null)
            {
                SetCameraAndAudioEnabled(terminalCamera, terminalCameraWasEnabled, terminalAudioWasEnabled);
            }
        }
    }

    private static bool GetAudioListenerEnabled(Camera camera)
    {
        if (camera == null)
        {
            return false;
        }

        AudioListener listener = camera.GetComponent<AudioListener>();
        return listener != null && listener.enabled;
    }

    private static void SetCameraAndAudioEnabled(Camera camera, bool cameraEnabled)
    {
        SetCameraAndAudioEnabled(camera, cameraEnabled, cameraEnabled);
    }

    private static void SetCameraAndAudioEnabled(Camera camera, bool cameraEnabled, bool audioEnabled)
    {
        if (camera == null)
        {
            return;
        }

        camera.enabled = cameraEnabled;

        AudioListener listener = camera.GetComponent<AudioListener>();
        if (listener != null)
        {
            listener.enabled = audioEnabled;
        }
    }

    private void SetGameplayLocked(bool locked)
    {
        if (!lockGameplayWhileOpen)
        {
            return;
        }

        if (locked)
        {
            if (gameplayBehavioursToDisable != null)
            {
                gameplayWasEnabled = new bool[gameplayBehavioursToDisable.Length];
                for (int i = 0; i < gameplayBehavioursToDisable.Length; i++)
                {
                    Behaviour behaviour = gameplayBehavioursToDisable[i];
                    gameplayWasEnabled[i] = behaviour != null && behaviour.enabled;
                    if (behaviour != null)
                    {
                        behaviour.enabled = false;
                    }
                }
            }

            playerInteractionWasEnabled = playerInteraction != null && playerInteraction.InteractionEnabled;
            if (playerInteraction != null)
            {
                playerInteraction.SetInteractionEnabled(false);
            }

            if (playerRigidbody != null)
            {
                playerRigidbody.velocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
            }
        }
        else
        {
            if (gameplayBehavioursToDisable != null && gameplayWasEnabled != null)
            {
                int count = Mathf.Min(gameplayBehavioursToDisable.Length, gameplayWasEnabled.Length);
                for (int i = 0; i < count; i++)
                {
                    if (gameplayBehavioursToDisable[i] != null)
                    {
                        gameplayBehavioursToDisable[i].enabled = gameplayWasEnabled[i];
                    }
                }
            }

            if (playerInteraction != null)
            {
                playerInteraction.SetInteractionEnabled(playerInteractionWasEnabled);
            }
        }
    }

    private int FindPageIndex(HearthHudPage page)
    {
        if (pages == null)
        {
            return -1;
        }

        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] == page)
            {
                return i;
            }
        }

        return -1;
    }

    private int GetTabOffset(HearthDoorwayTab tab)
    {
        switch (tab)
        {
            case HearthDoorwayTab.Acquisition:
                return 1;
            case HearthDoorwayTab.FamilyLog:
                return 2;
            case HearthDoorwayTab.TrustTrend:
                return 3;
            case HearthDoorwayTab.InspectionHistory:
                return 4;
            default:
                return 0;
        }
    }

    private void CycleNormalPage(int direction)
    {
        int normalCount = GetNormalCyclePageCount();
        if (normalCount <= 0)
        {
            return;
        }

        int localIndex = currentPageIndex;
        if (localIndex < 0 || localIndex >= normalCount)
        {
            localIndex = 0;
        }

        int next = Wrap(localIndex + direction, normalCount);
        keyboardFocusIndex = next;
        ShowPage(pages[next]);
    }

    private void MoveKeyboardFocus(int direction)
    {
        int focusCount = GetKeyboardFocusCount();
        if (focusCount <= 0)
        {
            return;
        }

        keyboardFocusIndex = Wrap(keyboardFocusIndex + direction, focusCount);
        int normalCount = GetNormalCyclePageCount();
        if (keyboardFocusIndex < normalCount)
        {
            ShowPage(pages[keyboardFocusIndex]);
            return;
        }

        RefreshKeyboardHint();
    }

    private void SubmitKeyboardFocus()
    {
        int normalCount = GetNormalCyclePageCount();
        if (keyboardFocusIndex >= normalCount)
        {
            RequestRobotReplay();
        }
    }

    private void SyncKeyboardFocusToCurrentPage()
    {
        int normalCount = GetNormalCyclePageCount();
        if (currentPageIndex >= 0 && currentPageIndex < normalCount)
        {
            keyboardFocusIndex = currentPageIndex;
        }
    }

    private void RefreshKeyboardHint()
    {
        if (keyboardHintText != null)
        {
            keyboardHintText.text = keyboardHintLabel;
        }

        if (keyboardFocusText == null)
        {
            return;
        }

        int normalCount = GetNormalCyclePageCount();
        if (keyboardFocusIndex >= normalCount)
        {
            keyboardFocusText.text = replayFocusLabel;
            return;
        }

        keyboardFocusText.text = string.Format(pageFocusFormat, keyboardFocusIndex + 1, normalCount);
    }

    private int GetNormalCyclePageCount()
    {
        if (pages == null || pages.Length == 0)
        {
            return 0;
        }

        int maxWithoutFinalChoice = Mathf.Max(1, pages.Length - 1);
        return Mathf.Clamp(keyboardCyclePageCount, 1, maxWithoutFinalChoice);
    }

    private int GetKeyboardFocusCount()
    {
        int normalCount = GetNormalCyclePageCount();
        return normalCount > 0 ? normalCount + 1 : 0;
    }

    private int GetPostReplayChoicePageIndex()
    {
        if (pages == null || pages.Length == 0)
        {
            return -1;
        }

        return pages.Length - 1;
    }

    private int Wrap(int value, int count)
    {
        if (count <= 0)
        {
            return 0;
        }

        while (value < 0)
        {
            value += count;
        }

        return value % count;
    }
}
