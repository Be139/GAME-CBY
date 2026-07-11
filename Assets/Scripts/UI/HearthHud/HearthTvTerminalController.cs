using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum HearthTerminalPrimaryAction
{
    RequestReplay,
    EnterUnit
}

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
    [SerializeField] private HearthTerminalCameraTransition cameraTransition;

    [Header("Player Lock")]
    [SerializeField] private bool lockGameplayWhileOpen = true;
    [SerializeField] private Behaviour[] gameplayBehavioursToDisable;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private bool unlockCursorWhileOpen = true;

    [Header("Scale")]
    [SerializeField] private float zoom = 1f;

    [Header("Presentation")]
    [SerializeField] private HearthTerminalBootSequence bootSequence;
    [SerializeField] private HearthTerminalSelectionHighlighter selectionHighlighter;

    [Header("Keyboard Navigation")]
    [SerializeField] private bool keyboardNavigationEnabled = true;
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;
    [SerializeField] private KeyCode cyclePageKey = KeyCode.Tab;
    [SerializeField] private KeyCode previousSelectionKey = KeyCode.LeftArrow;
    [SerializeField] private KeyCode nextSelectionKey = KeyCode.RightArrow;
    [SerializeField] private KeyCode upSelectionKey = KeyCode.UpArrow;
    [SerializeField] private KeyCode downSelectionKey = KeyCode.DownArrow;
    [SerializeField] private KeyCode submitKey = KeyCode.Space;
    [SerializeField] private int keyboardCyclePageCount = 5;
    [SerializeField] private TMP_Text keyboardHintText;
    [SerializeField] private TMP_Text keyboardFocusText;
    [SerializeField] private string pageFocusFormat = "PAGE {0}/{1}";
    [SerializeField] private string replayFocusLabel = "RECALL EVENT | SPACE";
    [SerializeField] private string keyboardHintLabel = "TAB NEXT PAGE     LEFT/RIGHT SELECT     SPACE CONFIRM     ESC EXIT";

    [Header("PPT Image State Navigation")]
    [SerializeField] private bool pageDrivenSelectionStates;
    [SerializeField] private int preChoiceSelectionPageCount = 6;
    [SerializeField] private int postReplayNavigationPageCount = 5;
    [SerializeField] private int postReplayChoicePageCount = 2;
    [SerializeField] private bool hideGeneratedHighlighterWhenPageDriven = true;
    [SerializeField] private string choiceAFocusLabel = "A SELECTED | SPACE";
    [SerializeField] private string choiceBFocusLabel = "B SELECTED | SPACE";

    [Header("Audio Hooks")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openClip;
    [SerializeField] private AudioClip closeClip;
    [SerializeField] private AudioClip bootClip;
    [SerializeField] private AudioClip pageSwitchClip;
    [SerializeField] private AudioClip focusMoveClip;
    [SerializeField] private AudioClip submitClip;
    [SerializeField] private AudioClip replayRequestClip;
    [SerializeField] private AudioClip viewSwitchClip;
    [Range(0f, 1f)]
    [SerializeField] private float audioVolume = 1f;

    [Header("Robot Replay")]
    [SerializeField] private HearthTerminalPrimaryAction primaryAction = HearthTerminalPrimaryAction.RequestReplay;
    [SerializeField] private MinLoopFlowController minLoopFlowController;
    [SerializeField] private ViewSwitchController viewSwitchController;
    [SerializeField] private string replayResidentId = "";
    [SerializeField] private bool closeTerminalWhenReplayStarts = true;
    [SerializeField] private bool showFinalChoiceWhenReplayUnavailable = true;
    [SerializeField] private bool closeTerminalWhenChoiceSubmitted = true;
    [SerializeField] private bool preventRepeatedChoiceSubmission = true;
    [SerializeField] private bool routeChoicesToMinLoop = true;
    [SerializeField] private UnityEvent onRobotReplayRequested;
    [SerializeField] private UnityEvent onEnterUnitRequested;
    [SerializeField] private UnityEvent onPostReplayChoiceShown;
    [SerializeField] private UnityEvent onChoiceASelected;
    [SerializeField] private UnityEvent onChoiceBSelected;

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
    private Coroutine terminalRoutine;
    private Coroutine cameraFocusRoutine;
    private bool terminalInputReady;
    private bool postReplayChoiceMode;
    private bool postReplayChoicesAvailable;
    private bool choiceSubmitted;
    private int pageDrivenChoiceLocalIndex;

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

    public HearthTerminalPrimaryAction PrimaryAction
    {
        get { return primaryAction; }
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
        terminalInputReady = false;

        if (bootSequence != null)
        {
            bootSequence.ApplyClosedInstant();
        }
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
        preChoiceSelectionPageCount = Mathf.Max(1, preChoiceSelectionPageCount);
        postReplayNavigationPageCount = Mathf.Max(1, postReplayNavigationPageCount);
        postReplayChoicePageCount = Mathf.Max(0, postReplayChoicePageCount);
        audioVolume = Mathf.Clamp01(audioVolume);
        ApplyZoom();
    }

    private void Update()
    {
        if (!IsOpen || !terminalInputReady)
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

        if (Input.GetKeyDown(upSelectionKey))
        {
            MoveKeyboardFocusVertical(-1);
        }

        if (Input.GetKeyDown(downSelectionKey))
        {
            MoveKeyboardFocusVertical(1);
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
        ResolveRuntimePlayerReferences();

        if (IsOpen)
        {
            return;
        }

        if (pageDrivenSelectionStates && showStartingPageOnStart)
        {
            postReplayChoiceMode = false;
            postReplayChoicesAvailable = false;
            choiceSubmitted = false;
            pageDrivenChoiceLocalIndex = 0;
            ShowPage(startingPage);
        }

        StartTerminalRoutine(OpenTerminalRoutine());
    }

    public void CloseTerminal()
    {
        if (!IsOpen)
        {
            return;
        }

        StartTerminalRoutine(CloseTerminalRoutine(true));
    }

    private IEnumerator OpenTerminalRoutine()
    {
        IsOpen = true;
        terminalInputReady = false;
        previousCursorLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        SetTerminalInputEnabled(false);
        SetGameplayLocked(true);
        PlayClip(openClip);

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

        PlayClip(bootClip);
        Coroutine bootRoutine = bootSequence != null ? StartCoroutine(bootSequence.PlayOpenSequence()) : null;
        StartCameraFocusRoutine(true);
        yield return WaitForRealtime(Mathf.Max(GetExpectedCameraDuration(true), GetExpectedBootDuration(true)));

        if (bootRoutine != null)
        {
            StopCoroutine(bootRoutine);
            bootSequence.ApplyOpenInstant();
        }

        CompleteCameraFocusIfNeeded(true);

        terminalInputReady = true;
        SetTerminalInputEnabled(true);
        RefreshKeyboardHint();

        if (onOpened != null)
        {
            onOpened.Invoke();
        }

        terminalRoutine = null;
    }

    private IEnumerator CloseTerminalRoutine(bool smoothCamera)
    {
        terminalInputReady = false;
        SetTerminalInputEnabled(false);
        if (selectionHighlighter != null)
        {
            selectionHighlighter.SetVisible(false);
        }

        PlayClip(closeClip);

        Coroutine closeBootRoutine = bootSequence != null ? StartCoroutine(bootSequence.PlayCloseSequence()) : null;

        if (smoothCamera)
        {
            StartCameraFocusRoutine(false);
            yield return WaitForRealtime(Mathf.Max(GetExpectedCameraDuration(false), GetExpectedBootDuration(false)));

            if (closeBootRoutine != null)
            {
                StopCoroutine(closeBootRoutine);
                bootSequence.ApplyClosedInstant();
            }

            CompleteCameraFocusIfNeeded(false);
        }
        else
        {
            if (closeBootRoutine != null)
            {
                StopCoroutine(closeBootRoutine);
                bootSequence.ApplyClosedInstant();
            }

            ApplyCameraFocusImmediate(false);
        }

        if (unlockCursorWhileOpen)
        {
            Cursor.lockState = previousCursorLockState;
            Cursor.visible = previousCursorVisible;
        }

        SetGameplayLocked(false);
        IsOpen = false;

        if (onClosed != null)
        {
            onClosed.Invoke();
        }

        terminalRoutine = null;
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

    public void SetPlayerInteraction(PlayerInteraction interaction)
    {
        playerInteraction = interaction;
        if (playerInteraction != null && IsUsablePlayerCamera(playerInteraction.mainCamera))
        {
            playerCamera = playerInteraction.mainCamera;
        }
    }

    public void SetPageDrivenSelectionStates(bool enabled, int preChoicePageCount, int choicePageCount)
    {
        pageDrivenSelectionStates = enabled;
        preChoiceSelectionPageCount = Mathf.Max(1, preChoicePageCount);
        postReplayNavigationPageCount = Mathf.Max(1, preChoiceSelectionPageCount - 1);
        postReplayChoicePageCount = Mathf.Max(0, choicePageCount);
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

    public void SetReplayResidentId(string residentId)
    {
        replayResidentId = residentId;
    }

    public void SetPrimaryAction(HearthTerminalPrimaryAction action)
    {
        primaryAction = action;
        replayFocusLabel = action == HearthTerminalPrimaryAction.EnterUnit
            ? "ENTER UNIT | SPACE"
            : "RECALL EVENT | SPACE";
    }

    public string GetReplayResidentId()
    {
        string explicitId = NormalizeReplayResidentId(replayResidentId);
        if (!string.IsNullOrEmpty(explicitId))
        {
            return explicitId;
        }

        return InferReplayResidentId();
    }

    public void RequestRobotReplay()
    {
        if (primaryAction == HearthTerminalPrimaryAction.EnterUnit)
        {
            RequestEnterUnit();
            return;
        }

        PlayClip(replayRequestClip);
        string residentId = GetReplayResidentId();

        if (minLoopFlowController != null)
        {
            if (closeTerminalWhenReplayStarts)
            {
                CloseTerminalInstant();
            }

            PlayClip(viewSwitchClip);
            minLoopFlowController.SetActiveReplayResident(residentId, this);

            if (onRobotReplayRequested != null)
            {
                onRobotReplayRequested.Invoke();
            }

            minLoopFlowController.RequestReplayFromTerminal();
            return;
        }

        if (onRobotReplayRequested != null)
        {
            onRobotReplayRequested.Invoke();
        }

        if (viewSwitchController != null)
        {
            if (closeTerminalWhenReplayStarts)
            {
                CloseTerminalInstant();
            }

            PlayClip(viewSwitchClip);
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

    public void RequestEnterUnit()
    {
        PlayClip(replayRequestClip);
        string residentId = GetReplayResidentId();

        if (closeTerminalWhenReplayStarts)
        {
            CloseTerminalInstant();
        }

        if (minLoopFlowController != null)
        {
            minLoopFlowController.SetActiveReplayResident(residentId, this);
        }

        if (onEnterUnitRequested != null)
        {
            onEnterUnitRequested.Invoke();
        }

        if (minLoopFlowController != null)
        {
            minLoopFlowController.RequestEnterUnitFromTerminal();
            return;
        }

        Debug.LogWarning("[HearthTvTerminalController] Enter Unit requested, but no MinLoopFlowController is assigned.", this);
    }

    public void ShowPostReplayChoicePage()
    {
        if (!IsOpen)
        {
            OpenTerminal();
        }

        choiceSubmitted = false;
        int finalPageIndex = GetPostReplayChoicePageIndex();
        if (finalPageIndex >= 0 && pages != null && finalPageIndex < pages.Length)
        {
            if (pageDrivenSelectionStates)
            {
                postReplayChoicesAvailable = true;
                postReplayChoiceMode = true;
                choiceSubmitted = false;
                keyboardFocusIndex = finalPageIndex;
                pageDrivenChoiceLocalIndex = 0;
            }

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

        bool changedPage = currentPage != page;
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

        if (changedPage && IsOpen && terminalInputReady)
        {
            PlayClip(pageSwitchClip);
        }

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

        if (cameraTransition == null)
        {
            cameraTransition = GetComponent<HearthTerminalCameraTransition>();
        }

        if (bootSequence == null)
        {
            bootSequence = GetComponent<HearthTerminalBootSequence>();
        }

        if (selectionHighlighter == null)
        {
            selectionHighlighter = GetComponentInChildren<HearthTerminalSelectionHighlighter>(true);
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
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

    private IEnumerator ApplyCameraFocusRoutine(bool focused)
    {
        if (!switchCameraWhileOpen)
        {
            if (focused && terminalCamera != null)
            {
                SetWorldCamera(terminalCamera);
            }

            yield break;
        }

        if (focused)
        {
            ResolveRuntimePlayerReferences();
            CaptureCameraState();

            if (cameraTransition != null && cameraTransition.CanRunEnterTransition(playerCamera, terminalCamera))
            {
                yield return StartCoroutine(RunCameraTransitionWithTimeout(
                    cameraTransition.TransitionToTerminal(playerCamera, terminalCamera, SetWorldCamera),
                    cameraTransition.EnterDuration,
                    true));
                yield break;
            }

            ApplyCameraFocusImmediate(true);
            yield break;
        }

        if (cameraTransition != null && cameraTransition.CanRunExitTransition(playerCamera, terminalCamera))
        {
            yield return StartCoroutine(RunCameraTransitionWithTimeout(
                cameraTransition.TransitionToPlayer(
                    playerCamera,
                    terminalCamera,
                    SetWorldCamera,
                    playerCameraWasEnabled,
                    terminalCameraWasEnabled,
                    playerAudioWasEnabled,
                    terminalAudioWasEnabled),
                cameraTransition.ExitDuration,
                false));
            yield break;
        }

        ApplyCameraFocusImmediate(false);
    }

    private IEnumerator RunCameraTransitionWithTimeout(IEnumerator transition, float expectedDuration, bool focused)
    {
        Coroutine routine = StartCoroutine(transition);
        float timeoutAt = Time.realtimeSinceStartup + Mathf.Max(0.1f, expectedDuration) + 1f;

        while (cameraTransition != null && cameraTransition.IsTransitioning && Time.realtimeSinceStartup < timeoutAt)
        {
            yield return null;
        }

        if (cameraTransition != null && cameraTransition.IsTransitioning)
        {
            StopCoroutine(routine);
            cameraTransition.CancelTransition();
            ApplyCameraFocusImmediate(focused, false);
        }
    }

    private void ApplyCameraFocusImmediate(bool focused, bool captureState = true)
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
            ResolveRuntimePlayerReferences();

            if (captureState)
            {
                CaptureCameraState();
            }

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

    private void CaptureCameraState()
    {
        playerCameraWasEnabled = playerCamera != null && playerCamera.enabled;
        terminalCameraWasEnabled = terminalCamera != null && terminalCamera.enabled;
        playerAudioWasEnabled = GetAudioListenerEnabled(playerCamera);
        terminalAudioWasEnabled = GetAudioListenerEnabled(terminalCamera);
    }

    private void ResolveRuntimePlayerReferences()
    {
        if (playerInteraction == null || !playerInteraction.gameObject.activeInHierarchy)
        {
            playerInteraction = FindBestPlayerInteraction();
        }

        Camera resolvedCamera = FindBestRuntimePlayerCamera();
        if (resolvedCamera != null)
        {
            playerCamera = resolvedCamera;
        }
    }

    private PlayerInteraction FindBestPlayerInteraction()
    {
        PlayerInteraction[] interactions = UnityEngine.Object.FindObjectsOfType<PlayerInteraction>(true);
        PlayerInteraction fallback = null;

        for (int i = 0; i < interactions.Length; i++)
        {
            PlayerInteraction interaction = interactions[i];
            if (interaction == null)
            {
                continue;
            }

            if (fallback == null)
            {
                fallback = interaction;
            }

            if (interaction.enabled && interaction.gameObject.activeInHierarchy && IsUsablePlayerCamera(interaction.mainCamera))
            {
                return interaction;
            }
        }

        for (int i = 0; i < interactions.Length; i++)
        {
            PlayerInteraction interaction = interactions[i];
            if (interaction != null && IsUsablePlayerCamera(interaction.mainCamera))
            {
                return interaction;
            }
        }

        return fallback;
    }

    private Camera FindBestRuntimePlayerCamera()
    {
        if (playerInteraction != null && IsUsablePlayerCamera(playerInteraction.mainCamera))
        {
            return playerInteraction.mainCamera;
        }

        PlayerInteraction bestInteraction = FindBestPlayerInteraction();
        if (bestInteraction != null)
        {
            playerInteraction = bestInteraction;
            if (IsUsablePlayerCamera(playerInteraction.mainCamera))
            {
                return playerInteraction.mainCamera;
            }
        }

        if (IsUsablePlayerCamera(playerCamera) && (playerCamera.enabled || GetAudioListenerEnabled(playerCamera)))
        {
            return playerCamera;
        }

        Camera sceneCamera = FindBestScenePlayerCamera();
        if (sceneCamera != null)
        {
            return sceneCamera;
        }

        if (Camera.main != null && IsUsablePlayerCamera(Camera.main))
        {
            return Camera.main;
        }

        return IsUsablePlayerCamera(playerCamera) ? playerCamera : null;
    }

    private Camera FindBestScenePlayerCamera()
    {
        Camera[] cameras = UnityEngine.Object.FindObjectsOfType<Camera>(true);
        Camera bestCamera = null;
        int bestScore = int.MinValue;

        for (int i = 0; i < cameras.Length; i++)
        {
            Camera camera = cameras[i];
            if (!IsUsablePlayerCamera(camera))
            {
                continue;
            }

            int score = ScorePlayerCameraCandidate(camera);
            if (bestCamera == null || score > bestScore)
            {
                bestCamera = camera;
                bestScore = score;
            }
        }

        return bestCamera;
    }

    private int ScorePlayerCameraCandidate(Camera camera)
    {
        int score = 0;
        if (camera.gameObject.activeInHierarchy)
        {
            score += 20;
        }

        if (camera.enabled)
        {
            score += 40;
        }

        if (GetAudioListenerEnabled(camera))
        {
            score += 30;
        }

        string path = GetHierarchyPath(camera.transform).ToUpperInvariant();
        if (path.Contains("FIRST PERSON"))
        {
            score += 120;
        }

        if (path.Contains("PLAYER") || path.Contains("PERSON"))
        {
            score += 80;
        }

        if (camera.CompareTag("MainCamera"))
        {
            score += 5;
        }

        if (camera.name == "Main Camera")
        {
            score -= 30;
        }

        return score;
    }

    private bool IsUsablePlayerCamera(Camera camera)
    {
        if (camera == null || camera == terminalCamera)
        {
            return false;
        }

        if (camera.name == "Terminal Transition Camera")
        {
            return false;
        }

        if (camera.transform.IsChildOf(transform))
        {
            return false;
        }

        if (IsLikelyTerminalCamera(camera))
        {
            return false;
        }

        return true;
    }

    private static bool IsLikelyTerminalCamera(Camera camera)
    {
        Transform cursor = camera != null ? camera.transform : null;
        while (cursor != null)
        {
            string name = cursor.name.ToUpperInvariant();
            if (name.Contains("TV") || name.Contains("MONITORCANVAS") || name.Contains("TERMINAL_"))
            {
                return true;
            }

            cursor = cursor.parent;
        }

        return false;
    }

    private static string GetHierarchyPath(Transform transformToRead)
    {
        if (transformToRead == null)
        {
            return string.Empty;
        }

        string path = transformToRead.name;
        Transform cursor = transformToRead.parent;
        while (cursor != null)
        {
            path = cursor.name + "/" + path;
            cursor = cursor.parent;
        }

        return path;
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
        if (pageDrivenSelectionStates)
        {
            CyclePageDrivenSelection(direction);
            return;
        }

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
        if (pageDrivenSelectionStates)
        {
            MovePageDrivenSelection(direction, true);
            return;
        }

        int focusCount = GetKeyboardFocusCount();
        if (focusCount <= 0)
        {
            return;
        }

        keyboardFocusIndex = Wrap(keyboardFocusIndex + direction, focusCount);
        PlayClip(focusMoveClip);
        int normalCount = GetNormalCyclePageCount();
        if (keyboardFocusIndex < normalCount)
        {
            ShowPage(pages[keyboardFocusIndex]);
            return;
        }

        RefreshKeyboardHint();
    }

    private void MoveKeyboardFocusVertical(int direction)
    {
        if (pageDrivenSelectionStates)
        {
            MovePageDrivenSelectionVertical(direction);
        }
    }

    private void SubmitKeyboardFocus()
    {
        if (pageDrivenSelectionStates)
        {
            SubmitPageDrivenSelection();
            return;
        }

        int normalCount = GetNormalCyclePageCount();
        if (keyboardFocusIndex >= normalCount)
        {
            PlayClip(submitClip);
            RequestRobotReplay();
        }
    }

    private void SyncKeyboardFocusToCurrentPage()
    {
        if (pageDrivenSelectionStates)
        {
            SyncPageDrivenFocusToCurrentPage();
            return;
        }

        int normalCount = GetNormalCyclePageCount();
        if (currentPageIndex >= 0 && currentPageIndex < normalCount)
        {
            keyboardFocusIndex = currentPageIndex;
        }
    }

    private void RefreshKeyboardHint()
    {
        if (pageDrivenSelectionStates)
        {
            RefreshPageDrivenKeyboardHint();
            return;
        }

        if (keyboardHintText != null)
        {
            keyboardHintText.text = keyboardHintLabel;
        }

        if (keyboardFocusText == null)
        {
            RefreshSelectionHighlighter();
            return;
        }

        int normalCount = GetNormalCyclePageCount();
        if (keyboardFocusIndex >= normalCount)
        {
            keyboardFocusText.text = replayFocusLabel;
            RefreshSelectionHighlighter();
            return;
        }

        keyboardFocusText.text = string.Format(pageFocusFormat, keyboardFocusIndex + 1, normalCount);
        RefreshSelectionHighlighter();
    }

    private void RefreshSelectionHighlighter()
    {
        if (selectionHighlighter == null)
        {
            return;
        }

        if (pageDrivenSelectionStates && hideGeneratedHighlighterWhenPageDriven)
        {
            selectionHighlighter.SetVisible(false);
            return;
        }

        if (!IsOpen || !terminalInputReady)
        {
            selectionHighlighter.SetVisible(false);
            return;
        }

        selectionHighlighter.SetFocus(keyboardFocusIndex);
    }

    private void CyclePageDrivenSelection(int direction)
    {
        MovePageDrivenSelection(direction, false);
    }

    private void MovePageDrivenSelection(int direction, bool playFocusMoveSound)
    {
        if (postReplayChoicesAvailable && postReplayChoiceMode)
        {
            MovePageDrivenChoice(direction, playFocusMoveSound);
            return;
        }

        int count = postReplayChoicesAvailable ? GetPostReplayNavigationPageCount() : GetPreChoiceSelectionPageCount();
        if (count <= 0)
        {
            return;
        }

        int localIndex = keyboardFocusIndex;
        if (localIndex < 0 || localIndex >= count)
        {
            localIndex = currentPageIndex >= 0 && currentPageIndex < count ? currentPageIndex : 0;
        }

        int next = Wrap(localIndex + direction, count);
        keyboardFocusIndex = next;
        postReplayChoiceMode = false;

        if (playFocusMoveSound)
        {
            PlayClip(focusMoveClip);
        }

        ShowPage(pages[next]);
    }

    private void MovePageDrivenSelectionVertical(int direction)
    {
        if (!postReplayChoicesAvailable)
        {
            return;
        }

        if (direction < 0 && postReplayChoiceMode)
        {
            postReplayChoiceMode = false;
            keyboardFocusIndex = 0;
            PlayClip(focusMoveClip);
            ShowPage(pages[keyboardFocusIndex]);
            return;
        }

        if (direction > 0 && !postReplayChoiceMode)
        {
            int choiceStart = GetChoiceStartIndex();
            int choiceCount = GetChoicePageCount();
            if (choiceStart < 0 || choiceCount <= 0)
            {
                return;
            }

            pageDrivenChoiceLocalIndex = Mathf.Clamp(pageDrivenChoiceLocalIndex, 0, choiceCount - 1);
            keyboardFocusIndex = choiceStart + pageDrivenChoiceLocalIndex;
            postReplayChoiceMode = true;
            PlayClip(focusMoveClip);
            ShowPage(pages[keyboardFocusIndex]);
        }
    }

    private void MovePageDrivenChoice(int direction, bool playFocusMoveSound)
    {
        int choiceStart = GetChoiceStartIndex();
        int choiceCount = GetChoicePageCount();
        if (choiceStart < 0 || choiceCount <= 0)
        {
            return;
        }

        int localIndex = keyboardFocusIndex - choiceStart;
        if (localIndex < 0 || localIndex >= choiceCount)
        {
            localIndex = 0;
        }

        int nextLocal = Wrap(localIndex + direction, choiceCount);
        pageDrivenChoiceLocalIndex = nextLocal;
        keyboardFocusIndex = choiceStart + nextLocal;
        postReplayChoiceMode = true;

        if (playFocusMoveSound)
        {
            PlayClip(focusMoveClip);
        }

        ShowPage(pages[keyboardFocusIndex]);
    }

    private void SubmitPageDrivenSelection()
    {
        if (postReplayChoicesAvailable && (postReplayChoiceMode || IsCurrentPageChoicePage()))
        {
            SubmitPageDrivenChoice();
            return;
        }

        if (postReplayChoicesAvailable)
        {
            return;
        }

        int actionIndex = GetPreChoiceSelectionPageCount() - 1;
        if (keyboardFocusIndex == actionIndex || currentPageIndex == actionIndex)
        {
            PlayClip(submitClip);
            RequestRobotReplay();
        }
    }

    private void SubmitPageDrivenChoice()
    {
        if (preventRepeatedChoiceSubmission && choiceSubmitted)
        {
            return;
        }

        int choiceStart = GetChoiceStartIndex();
        if (choiceStart < 0)
        {
            return;
        }

        int localIndex = Mathf.Clamp(keyboardFocusIndex - choiceStart, 0, Mathf.Max(0, GetChoicePageCount() - 1));
        pageDrivenChoiceLocalIndex = localIndex;
        choiceSubmitted = true;
        terminalInputReady = false;
        SetTerminalInputEnabled(false);
        PlayClip(submitClip);

        if (localIndex == 0)
        {
            if (routeChoicesToMinLoop && minLoopFlowController != null)
            {
                minLoopFlowController.ChooseDispositionA();
            }

            if (onChoiceASelected != null)
            {
                onChoiceASelected.Invoke();
            }
        }
        else
        {
            if (routeChoicesToMinLoop && minLoopFlowController != null)
            {
                minLoopFlowController.ChooseDispositionB();
            }

            if (onChoiceBSelected != null)
            {
                onChoiceBSelected.Invoke();
            }
        }

        if (closeTerminalWhenChoiceSubmitted)
        {
            CloseTerminal();
        }
    }

    private string InferReplayResidentId()
    {
        Transform cursor = transform;
        while (cursor != null)
        {
            string fromName = NormalizeReplayResidentId(cursor.name);
            if (!string.IsNullOrEmpty(fromName))
            {
                return fromName;
            }

            cursor = cursor.parent;
        }

        if (firstSlideNumber >= 17)
        {
            return "17F03";
        }

        if (firstSlideNumber >= 9)
        {
            return "17F02";
        }

        return "17F01";
    }

    private static string NormalizeReplayResidentId(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        string normalized = value.Trim().ToUpperInvariant();
        normalized = normalized.Replace("-", string.Empty);
        normalized = normalized.Replace("_", string.Empty);
        normalized = normalized.Replace(" ", string.Empty);

        if (normalized.Contains("17F03") || normalized.Contains("ROOM3"))
        {
            return "17F03";
        }

        if (normalized.Contains("17F02") || normalized.Contains("ROOM2"))
        {
            return "17F02";
        }

        if (normalized.Contains("17F01") || normalized.Contains("ROOM1"))
        {
            return "17F01";
        }

        return string.Empty;
    }

    private void SyncPageDrivenFocusToCurrentPage()
    {
        if (currentPageIndex < 0)
        {
            return;
        }

        if (IsPageIndexChoicePage(currentPageIndex))
        {
            postReplayChoicesAvailable = true;
            postReplayChoiceMode = true;
            keyboardFocusIndex = currentPageIndex;
            pageDrivenChoiceLocalIndex = Mathf.Clamp(currentPageIndex - GetChoiceStartIndex(), 0, Mathf.Max(0, GetChoicePageCount() - 1));
            return;
        }

        int preChoiceCount = postReplayChoicesAvailable ? GetPostReplayNavigationPageCount() : GetPreChoiceSelectionPageCount();
        if (currentPageIndex < preChoiceCount)
        {
            postReplayChoiceMode = false;
            keyboardFocusIndex = currentPageIndex;
        }
    }

    private void RefreshPageDrivenKeyboardHint()
    {
        if (keyboardHintText != null)
        {
            keyboardHintText.text = keyboardHintLabel;
        }

        if (keyboardFocusText != null)
        {
            if (postReplayChoiceMode || IsCurrentPageChoicePage())
            {
                int choiceStart = GetChoiceStartIndex();
                int localIndex = choiceStart >= 0 ? keyboardFocusIndex - choiceStart : 0;
                keyboardFocusText.text = localIndex <= 0 ? choiceAFocusLabel : choiceBFocusLabel;
            }
            else
            {
                int actionIndex = GetPreChoiceSelectionPageCount() - 1;
                keyboardFocusText.text = keyboardFocusIndex == actionIndex
                    ? replayFocusLabel
                    : string.Format(pageFocusFormat, keyboardFocusIndex + 1, postReplayChoicesAvailable ? GetPostReplayNavigationPageCount() : GetPreChoiceSelectionPageCount());
            }
        }

        RefreshSelectionHighlighter();
    }

    private bool IsCurrentPageChoicePage()
    {
        return IsPageIndexChoicePage(currentPageIndex);
    }

    private bool IsPageIndexChoicePage(int pageIndex)
    {
        int choiceStart = GetChoiceStartIndex();
        return choiceStart >= 0 && pageIndex >= choiceStart && pageIndex < pages.Length;
    }

    private void StartTerminalRoutine(IEnumerator routine)
    {
        if (terminalRoutine != null)
        {
            StopCoroutine(terminalRoutine);
        }

        if (cameraTransition != null)
        {
            cameraTransition.CancelTransition();
        }

        if (cameraFocusRoutine != null)
        {
            StopCoroutine(cameraFocusRoutine);
            cameraFocusRoutine = null;
        }

        terminalRoutine = StartCoroutine(routine);
    }

    private void StartCameraFocusRoutine(bool focused)
    {
        if (cameraFocusRoutine != null)
        {
            StopCoroutine(cameraFocusRoutine);
            cameraFocusRoutine = null;
        }

        cameraFocusRoutine = StartCoroutine(ApplyCameraFocusRoutine(focused));
    }

    private void CompleteCameraFocusIfNeeded(bool focused)
    {
        if (cameraTransition != null && cameraTransition.IsTransitioning)
        {
            if (cameraFocusRoutine != null)
            {
                StopCoroutine(cameraFocusRoutine);
            }

            cameraTransition.CancelTransition();
            ApplyCameraFocusImmediate(focused, false);
        }

        cameraFocusRoutine = null;
    }

    private IEnumerator WaitForRealtime(float seconds)
    {
        if (seconds <= 0f)
        {
            yield break;
        }

        float endTime = Time.realtimeSinceStartup + seconds;
        while (Time.realtimeSinceStartup < endTime)
        {
            yield return null;
        }
    }

    private float GetExpectedCameraDuration(bool focused)
    {
        if (cameraTransition == null)
        {
            return 0f;
        }

        float duration = focused ? cameraTransition.EnterDuration : cameraTransition.ExitDuration;
        return Mathf.Max(0f, duration) + 0.15f;
    }

    private float GetExpectedBootDuration(bool opening)
    {
        if (bootSequence == null)
        {
            return 0f;
        }

        float duration = opening ? bootSequence.BootDuration : bootSequence.CloseFadeDuration;
        return Mathf.Max(0f, duration) + 0.15f;
    }

    private void CloseTerminalInstant()
    {
        if (!IsOpen)
        {
            return;
        }

        if (terminalRoutine != null)
        {
            StopCoroutine(terminalRoutine);
            terminalRoutine = null;
        }

        if (cameraTransition != null)
        {
            cameraTransition.CancelTransition();
        }

        if (cameraFocusRoutine != null)
        {
            StopCoroutine(cameraFocusRoutine);
            cameraFocusRoutine = null;
        }

        terminalInputReady = false;
        SetTerminalInputEnabled(false);
        if (selectionHighlighter != null)
        {
            selectionHighlighter.SetVisible(false);
        }

        if (bootSequence != null)
        {
            bootSequence.ApplyClosedInstant();
        }

        ApplyCameraFocusImmediate(false);

        if (unlockCursorWhileOpen)
        {
            Cursor.lockState = previousCursorLockState;
            Cursor.visible = previousCursorVisible;
        }

        SetGameplayLocked(false);
        IsOpen = false;

        if (onClosed != null)
        {
            onClosed.Invoke();
        }
    }

    private void PlayClip(AudioClip clip)
    {
        if (audioSource == null || clip == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip, audioVolume);
    }

    private int GetNormalCyclePageCount()
    {
        if (pages == null || pages.Length == 0)
        {
            return 0;
        }

        if (pageDrivenSelectionStates)
        {
            return GetPreChoiceSelectionPageCount();
        }

        int maxWithoutFinalChoice = Mathf.Max(1, pages.Length - 1);
        return Mathf.Clamp(keyboardCyclePageCount, 1, maxWithoutFinalChoice);
    }

    private int GetKeyboardFocusCount()
    {
        if (pageDrivenSelectionStates)
        {
            if (postReplayChoicesAvailable)
            {
                return postReplayChoiceMode ? GetChoicePageCount() : GetPostReplayNavigationPageCount();
            }

            return GetPreChoiceSelectionPageCount();
        }

        int normalCount = GetNormalCyclePageCount();
        return normalCount > 0 ? normalCount + 1 : 0;
    }

    private int GetPostReplayChoicePageIndex()
    {
        if (pages == null || pages.Length == 0)
        {
            return -1;
        }

        if (pageDrivenSelectionStates)
        {
            return GetChoiceStartIndex();
        }

        return pages.Length - 1;
    }

    private int GetPreChoiceSelectionPageCount()
    {
        if (pages == null || pages.Length == 0)
        {
            return 0;
        }

        int maxBeforeChoices = pages.Length - GetChoicePageCount();
        if (maxBeforeChoices <= 0)
        {
            maxBeforeChoices = pages.Length;
        }

        return Mathf.Clamp(preChoiceSelectionPageCount, 1, maxBeforeChoices);
    }

    private int GetPostReplayNavigationPageCount()
    {
        if (pages == null || pages.Length == 0)
        {
            return 0;
        }

        int maxBeforeChoices = pages.Length - GetChoicePageCount();
        if (maxBeforeChoices <= 0)
        {
            maxBeforeChoices = pages.Length;
        }

        return Mathf.Clamp(postReplayNavigationPageCount, 1, maxBeforeChoices);
    }

    private int GetChoicePageCount()
    {
        if (pages == null || pages.Length == 0 || postReplayChoicePageCount <= 0)
        {
            return 0;
        }

        return Mathf.Clamp(postReplayChoicePageCount, 0, pages.Length);
    }

    private int GetChoiceStartIndex()
    {
        int choiceCount = GetChoicePageCount();
        if (pages == null || pages.Length == 0 || choiceCount <= 0)
        {
            return -1;
        }

        return Mathf.Max(0, pages.Length - choiceCount);
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
