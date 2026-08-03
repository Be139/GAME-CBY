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
    EnterUnit,
    Custom
}

public enum HearthTerminalMode
{
    Auto,
    LobbySync,
    Doorway,
    Home
}

[DisallowMultipleComponent]
public class HearthTvTerminalController : MonoBehaviour
{
    private static readonly HashSet<HearthTvTerminalController> OpenTerminals =
        new HashSet<HearthTvTerminalController>();

    [Header("Canvas")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private RectTransform contentRoot;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private bool createEventSystemIfMissing = true;
    [Tooltip("When enabled, the World Space Canvas is not rendered at all while the terminal is closed. Intended for the lobby task terminal whose UI must sit flush with the physical screen.")]
    [SerializeField] private bool hideCanvasWhenClosed;
    [Tooltip("Keeps this World Space Canvas visible while editing so its position and scale can be adjusted. Runtime visibility still follows Hide Canvas When Closed.")]
    [SerializeField] private bool showCanvasInEditMode = true;

    [Header("Pages")]
    [SerializeField] private HearthHudPage[] pages;
    [SerializeField] private int firstSlideNumber = 1;
    [SerializeField] private HearthHudPageId startingPage = HearthHudPageId.Slide01PersistentActive;
    [SerializeField] private bool showStartingPageOnStart = true;
    [SerializeField] private bool refreshPagesFromChildrenOnAwake = true;

    [Header("Terminal Strategy")]
    [SerializeField] private HearthTerminalMode terminalMode =
        HearthTerminalMode.Auto;

    [Header("Focus Camera")]
    [SerializeField] private bool switchCameraWhileOpen;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera terminalCamera;
    [Tooltip("Physical TV or terminal root that owns the fixed terminal camera. Automatically resolved from the hierarchy when left empty.")]
    [SerializeField] private Transform terminalHardwareRoot;
    [SerializeField] private HearthTerminalCameraTransition cameraTransition;

    [Header("Player Lock")]
    [SerializeField] private bool lockGameplayWhileOpen = true;
    [SerializeField] private HearthPlayerControlLock playerControlLock;
    [SerializeField] private Behaviour[] gameplayBehavioursToDisable;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private Rigidbody playerRigidbody;
    [SerializeField] private bool unlockCursorWhileOpen = true;

    [Header("Scale")]
    [SerializeField] private float zoom = 1f;

    [Header("Presentation")]
    [SerializeField] private HearthTerminalBootSequence bootSequence;
    [SerializeField] private HearthTerminalSelectionHighlighter selectionHighlighter;
    [SerializeField] private HearthUiPressFeedback submitFeedback;
    [SerializeField] private HearthDialogueSurface dialogueSurface;
    [SerializeField] private HearthDialogueSurface messageSurface;
    [SerializeField] private HearthUiThemeProfile uiThemeProfile;
    [SerializeField] private HearthUiLayoutProfile uiLayoutProfile;

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
    [SerializeField] private TMP_Text runtimePromptText;
    [SerializeField] private Color runtimePromptReadyColor = new Color(0.78f, 0.96f, 1f, 0.96f);
    [SerializeField] private Color runtimePromptWaitingColor = new Color(0.46f, 0.52f, 0.56f, 0.84f);
    [SerializeField] private string pageFocusFormat = "PAGE {0}/{1}";
    [SerializeField] private string replayFocusLabel =
        "REVIEW ARCHIVED EVENT | SPACE";
    [SerializeField] private string keyboardHintLabel = "TAB NEXT PAGE     LEFT/RIGHT SELECT     SPACE CONFIRM     ESC EXIT";
    [SerializeField] private bool submitPrimaryActionFromCurrentPage;

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
    [Tooltip("Optional continuous electrical hum/static while the terminal is open. Uses a separate AudioSource so short UI cues keep their own volume and pitch.")]
    [SerializeField] private AudioSource activeLoopSource;
    [SerializeField] private AudioClip activeLoopClip;
    [SerializeField] private HearthSfxCuePlayer activeLoopCuePlayer;
    [SerializeField] private string activeLoopCueId = string.Empty;
    [Range(0f, 1f)]
    [SerializeField] private float activeLoopVolume = 0.35f;
    [Range(0f, 1f)]
    [SerializeField] private float audioVolume = 1f;

    [Header("Robot Replay")]
    [SerializeField] private HearthTerminalPrimaryAction primaryAction = HearthTerminalPrimaryAction.RequestReplay;
    [SerializeField] private MinLoopFlowController minLoopFlowController;
    [SerializeField] private ViewSwitchController viewSwitchController;
    [SerializeField] private string replayResidentId = "";
    [SerializeField] private bool closeTerminalWhenReplayStarts = true;
    [Tooltip("Keep the terminal camera active after a Custom action until the receiving flow finishes its blackout handoff.")]
    [SerializeField] private bool deferCustomActionCloseUntilExternalFade;
    [SerializeField] private bool showFinalChoiceWhenReplayUnavailable = true;
    [SerializeField] private bool closeTerminalWhenChoiceSubmitted = true;
    [SerializeField] private bool preventRepeatedChoiceSubmission = true;
    [SerializeField] private bool routeChoicesToMinLoop = true;
    [SerializeField] private UnityEvent onRobotReplayRequested;
    [SerializeField] private UnityEvent onEnterUnitRequested;
    [SerializeField] private UnityEvent onCustomPrimaryAction = new UnityEvent();
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
    private bool terminalPresentationReady;
    private bool choiceInputEnabled = true;
    private bool primaryActionInputEnabled = true;
    private bool closeInputEnabled = true;
    private bool runtimePromptOverrideActive;
    private string runtimePromptOverride = string.Empty;
    private bool postReplayChoiceMode;
    private bool postReplayChoicesAvailable;
    private bool choiceSubmitted;
    private int pageDrivenChoiceLocalIndex;
    private bool customActionHandoffPending;
    private bool terminalSessionCleanupInProgress;
    private bool gameplayLockHeld;
    private int compactFocusIndex;
    private bool postReplayAnalysisMode;
    private readonly HearthTerminalViewState terminalViewState = new HearthTerminalViewState();
#if UNITY_EDITOR
    [NonSerialized] private bool editorCanvasRefreshQueued;
#endif

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

    public UnityEvent OnCustomPrimaryAction
    {
        get { return onCustomPrimaryAction; }
    }

    public UnityEvent OnOpened
    {
        get { return onOpened; }
    }

    public UnityEvent OnClosed
    {
        get { return onClosed; }
    }

    public HearthTerminalViewState TerminalViewState
    {
        get { return terminalViewState; }
    }

    public event Action<HearthTerminalViewState> TerminalViewStateChanged;

    /// <summary>
    /// Returns the terminal-owned world-space dialogue lane. Existing V2
    /// FieldUnitPanel objects are reused; older terminal prefabs receive the
    /// same lane at runtime without changing their physical transforms.
    /// </summary>
    public HearthDialogueSurface ResolveDialogueSurface()
    {
        EnsureReferences();
        if (dialogueSurface != null)
        {
            HearthHudPage ownerPage =
                dialogueSurface.GetComponentInParent<HearthHudPage>(true);
            if (ownerPage == null || ownerPage == currentPage)
            {
                return dialogueSurface;
            }

            dialogueSurface.HideImmediate();
            dialogueSurface = null;
        }

        // Page prefabs can contain several FieldUnitPanel objects under
        // mutually exclusive V2_PageVisual roots. Only bind the panel owned by
        // the currently visible page; otherwise use a terminal-wide surface.
        Transform existingPanel = currentPage != null
            ? FindDescendantByName(currentPage.transform, "FieldUnitPanel")
            : null;
        if (existingPanel == null && contentRoot != null)
        {
            existingPanel = contentRoot.Find("TerminalDialogueSurface_V2");
        }
        if (existingPanel != null)
        {
            dialogueSurface = existingPanel.GetComponent<HearthDialogueSurface>();
            CanvasGroup group = existingPanel.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = existingPanel.gameObject.AddComponent<CanvasGroup>();
            }

            TMP_Text speaker = FindTextInChildren(existingPanel, "Title", "Speaker", "SpeakerName");
            TMP_Text body = FindTextInChildren(existingPanel, "Body", "DialogueText", "MessageText");
            TMP_Text hint = FindTextInChildren(existingPanel, "DialogueAdvanceHint", "AdvanceHint");
            if (hint == null)
            {
                hint = CreateDialogueText(
                    existingPanel as RectTransform,
                    "DialogueAdvanceHint",
                    new Vector2(0.62f, 0.02f),
                    new Vector2(0.97f, 0.24f),
                    18f,
                    TextAlignmentOptions.BottomRight);
            }

            if (dialogueSurface == null)
            {
                dialogueSurface = existingPanel.gameObject.AddComponent<HearthDialogueSurface>();
            }

            dialogueSurface.Configure(group, speaker, body, hint);
            ApplyDialogueSurfaceProfile(dialogueSurface, existingPanel as RectTransform, false);
            LinkExclusiveDialogueSurfaces();
            dialogueSurface.HideImmediate();
            return dialogueSurface;
        }

        dialogueSurface = CreateRuntimeDialogueSurface();
        LinkExclusiveDialogueSurfaces();
        return dialogueSurface;
    }

    /// <summary>
    /// Returns the terminal-owned voice-message card. It is intentionally
    /// separate from FieldUnitPanel so a recorded Lily message never stacks
    /// with, or inherits the geometry of, ordinary terminal narration.
    /// </summary>
    public HearthDialogueSurface ResolveMessageSurface()
    {
        EnsureReferences();
        if (messageSurface != null)
        {
            HearthHudPage ownerPage =
                messageSurface.GetComponentInParent<HearthHudPage>(true);
            if (ownerPage == null || ownerPage == currentPage)
            {
                return messageSurface;
            }

            messageSurface.HideImmediate();
            messageSurface = null;
        }

        Transform existingPanel = currentPage != null
            ? FindDescendantByName(currentPage.transform, "LilyMessagePanel")
            : null;
        if (existingPanel == null && contentRoot != null)
        {
            existingPanel = contentRoot.Find("TerminalMessageSurface_V2");
        }
        if (existingPanel != null)
        {
            messageSurface = existingPanel.GetComponent<HearthDialogueSurface>();
            CanvasGroup group = existingPanel.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = existingPanel.gameObject.AddComponent<CanvasGroup>();
            }

            TMP_Text speaker = FindTextInChildren(existingPanel, "Speaker", "Title", "SpeakerName");
            TMP_Text body = FindTextInChildren(existingPanel, "Body", "MessageText", "DialogueText");
            TMP_Text hint = FindTextInChildren(existingPanel, "AdvanceHint", "DialogueAdvanceHint");
            if (messageSurface == null)
            {
                messageSurface = existingPanel.gameObject.AddComponent<HearthDialogueSurface>();
            }

            messageSurface.Configure(group, speaker, body, hint);
            ApplyDialogueSurfaceProfile(messageSurface, existingPanel as RectTransform, true);
            LinkExclusiveDialogueSurfaces();
            messageSurface.HideImmediate();
            return messageSurface;
        }

        messageSurface = CreateRuntimeMessageSurface();
        LinkExclusiveDialogueSurfaces();
        return messageSurface;
    }

    public void SetUiProfiles(
        HearthUiThemeProfile themeProfile,
        HearthUiLayoutProfile layoutProfile)
    {
        uiThemeProfile = themeProfile;
        uiLayoutProfile = layoutProfile;
        ApplyDialogueSurfaceProfile(
            dialogueSurface,
            dialogueSurface != null ? dialogueSurface.transform as RectTransform : null,
            false);
        ApplyDialogueSurfaceProfile(
            messageSurface,
            messageSurface != null ? messageSurface.transform as RectTransform : null,
            true);
        LinkExclusiveDialogueSurfaces();
    }

    public bool IsCustomActionHandoffPending
    {
        get { return customActionHandoffPending; }
    }

    public bool IsPresentationReady
    {
        get { return terminalPresentationReady; }
    }

    public HearthTerminalMode TerminalMode
    {
        get { return ResolveTerminalMode(); }
    }

    public bool PreservesHumanHud
    {
        get { return ResolveTerminalMode() == HearthTerminalMode.LobbySync; }
    }

    public bool IsPostReplayAnalysisMode
    {
        get { return postReplayAnalysisMode; }
    }

    public static HearthTvTerminalController ActiveTerminal
    {
        get
        {
            OpenTerminals.RemoveWhere(
                terminal => terminal == null || !terminal.IsOpen);
            foreach (HearthTvTerminalController terminal in OpenTerminals)
            {
                return terminal;
            }

            return null;
        }
    }

    public static bool AnyTerminalOpen
    {
        get
        {
            OpenTerminals.RemoveWhere(terminal => terminal == null || !terminal.IsOpen);
            return OpenTerminals.Count > 0;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOpenTerminalRegistry()
    {
        OpenTerminals.Clear();
    }

    private static HearthTvTerminalController FindRegisteredOpenTerminal(
        HearthTvTerminalController excluded,
        UnityEngine.SceneManagement.Scene scene)
    {
        OpenTerminals.RemoveWhere(terminal => terminal == null || !terminal.IsOpen);
        foreach (HearthTvTerminalController terminal in OpenTerminals)
        {
            if (terminal != excluded && terminal.gameObject.scene == scene)
            {
                return terminal;
            }
        }

        return null;
    }

    private void Reset()
    {
        EnsureReferences();
        ApplyZoom();
    }

    private void Awake()
    {
        EnsureReferences();
        ApplyTerminalModeDefaults();

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

        RefreshCanvasPresentationVisibility();

    }

    private void Start()
    {
        RefreshCanvasPresentationVisibility();
        if (showStartingPageOnStart)
        {
            ShowPage(startingPage);
        }
    }

    private void OnDisable()
    {
        if (Application.isPlaying && IsOpen && !terminalSessionCleanupInProgress)
        {
            CloseTerminalInstant(false);
            return;
        }

        OpenTerminals.Remove(this);
        StopActiveAudioLoop();
        SetGameplayLocked(false);
    }

    private void OnValidate()
    {
        zoom = Mathf.Max(0.1f, zoom);
        keyboardCyclePageCount = Mathf.Max(1, keyboardCyclePageCount);
        preChoiceSelectionPageCount = Mathf.Max(1, preChoiceSelectionPageCount);
        postReplayNavigationPageCount = Mathf.Max(1, postReplayNavigationPageCount);
        postReplayChoicePageCount = Mathf.Max(0, postReplayChoicePageCount);
        audioVolume = Mathf.Clamp01(audioVolume);
        activeLoopVolume = Mathf.Clamp01(activeLoopVolume);
        ApplyTerminalModeDefaults();
        ApplyZoom();
        QueueEditorCanvasRefresh();
    }

    private void Update()
    {
        if (!IsOpen || !terminalInputReady)
        {
            return;
        }

        HearthTerminalMode resolvedMode = ResolveTerminalMode();
        if (closeInputEnabled &&
            !postReplayAnalysisMode &&
            Input.GetKeyDown(closeKey))
        {
            CloseTerminal();
            return;
        }

        if (!keyboardNavigationEnabled)
        {
            return;
        }

        if (postReplayAnalysisMode)
        {
            return;
        }

        if (resolvedMode == HearthTerminalMode.LobbySync)
        {
            if (Input.GetKeyDown(submitKey))
            {
                // Lobby dialogue and the terminal share Space.  The owning
                // flow disables the primary action while a line is active so
                // the same key press cannot both advance dialogue and close
                // the terminal.
                if (!primaryActionInputEnabled)
                {
                    return;
                }

                if (primaryAction == HearthTerminalPrimaryAction.Custom)
                {
                    RequestCustomPrimaryAction();
                }
                else if (closeInputEnabled)
                {
                    CloseTerminal();
                }
            }

            return;
        }

        if (resolvedMode == HearthTerminalMode.Doorway ||
            resolvedMode == HearthTerminalMode.Home)
        {
            if (resolvedMode == HearthTerminalMode.Home)
            {
                compactFocusIndex = 2;
                if (Input.GetKeyDown(submitKey))
                {
                    SubmitCompactFocus();
                }
                return;
            }

            if (Input.GetKeyDown(previousSelectionKey))
            {
                MoveCompactFocus(-1);
            }

            if (Input.GetKeyDown(nextSelectionKey))
            {
                MoveCompactFocus(1);
            }

            if (Input.GetKeyDown(submitKey))
            {
                SubmitCompactFocus();
            }

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

        HearthTvTerminalController otherOpenTerminal = FindOtherOpenTerminal();
        if (otherOpenTerminal != null)
        {
            Debug.LogWarning(
                "[HearthTvTerminalController] Open request ignored because another terminal is already active: " +
                GetHierarchyPath(otherOpenTerminal.transform),
                this);
            return;
        }

        if (switchCameraWhileOpen && !ResolveOwnedTerminalCamera())
        {
            Debug.LogError(
                "[HearthTvTerminalController] Terminal camera must belong to this terminal's physical TV hierarchy and must not be a player camera: " +
                GetHierarchyPath(transform),
                this);
            return;
        }

        SetCanvasPresentationVisible(true);
        compactFocusIndex = 0;
        postReplayAnalysisMode = false;

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

#if UNITY_EDITOR
    public void CloseTerminalImmediateForPreview()
    {
        if (IsOpen)
        {
            CloseTerminalInstant(false);
        }
    }
#endif

    private IEnumerator OpenTerminalRoutine()
    {
        IsOpen = true;
        OpenTerminals.Add(this);
        customActionHandoffPending = false;
        terminalInputReady = false;
        terminalPresentationReady = false;
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
        StartActiveAudioLoop();
        Coroutine bootRoutine = bootSequence != null ? StartCoroutine(bootSequence.PlayOpenSequence()) : null;
        StartCameraFocusRoutine(true);
        yield return WaitForRealtime(Mathf.Max(GetExpectedCameraDuration(true), GetExpectedBootDuration(true)));

        if (bootRoutine != null)
        {
            StopCoroutine(bootRoutine);
            bootSequence.ApplyOpenInstant();
        }

        CompleteCameraFocusIfNeeded(true);

        terminalPresentationReady = true;
        terminalInputReady = choiceInputEnabled;
        SetTerminalInputEnabled(terminalInputReady);
        RefreshKeyboardHint();

        if (onOpened != null)
        {
            onOpened.Invoke();
        }

        terminalRoutine = null;
    }

    private IEnumerator CloseTerminalRoutine(bool smoothCamera)
    {
        customActionHandoffPending = false;
        terminalInputReady = false;
        terminalPresentationReady = false;
        SetTerminalInputEnabled(false);
        if (selectionHighlighter != null)
        {
            selectionHighlighter.SetVisible(false);
        }

        StopActiveAudioLoop();
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
        OpenTerminals.Remove(this);
        RefreshTerminalViewState();
        RefreshRuntimePrompt();
        RefreshCanvasPresentationVisibility();

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
    }

    public void SetTerminalHardwareRoot(Transform hardwareRoot)
    {
        terminalHardwareRoot = hardwareRoot;
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

    public void SetTerminalMode(HearthTerminalMode mode)
    {
        terminalMode = mode;
        ApplyTerminalModeDefaults();
        RefreshKeyboardHint();
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
        if (action == HearthTerminalPrimaryAction.EnterUnit)
        {
            replayFocusLabel = "ENTER UNIT | SPACE";
        }
        else if (action == HearthTerminalPrimaryAction.Custom)
        {
            string terminalName = name ?? string.Empty;
            if (terminalName.IndexOf(
                    "17F04",
                    System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                terminalName.IndexOf(
                    "HOME",
                    System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                replayFocusLabel = "ENTER HOME | SPACE";
            }
            else if (terminalName.IndexOf(
                         "LOBBY",
                         System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                     terminalName.IndexOf(
                         "ASSIGNMENT",
                         System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                replayFocusLabel = "CLOSE TERMINAL | SPACE";
            }
            else
            {
                replayFocusLabel = "CONFIRM | SPACE";
            }
        }
        else
        {
            replayFocusLabel = "REVIEW ARCHIVED EVENT | SPACE";
        }
    }

    public void SetHideCanvasWhenClosed(bool value)
    {
        hideCanvasWhenClosed = value;
        EnsureReferences();
        RefreshCanvasPresentationVisibility();
    }

    public void SetHideFirstPersonUiWhileOpen(bool value)
    {
        // Compatibility entry point retained for older installers and scene
        // bindings. V2 visibility is resolved centrally by
        // HearthUiStateCoordinator, so terminals no longer persist or mutate
        // a private list of HUD roots.
    }

    public void SetShowCanvasInEditMode(bool value)
    {
        showCanvasInEditMode = value;
        EnsureReferences();
        RefreshCanvasPresentationVisibility();
    }

    public void SetActiveLoopAudio(AudioSource source, AudioClip clip)
    {
        activeLoopSource = source;
        activeLoopClip = clip;
        ConfigureActiveLoopSource();
    }

    public void SetActiveLoopCue(HearthSfxCuePlayer cuePlayer, string cueId)
    {
        activeLoopCuePlayer = cuePlayer;
        activeLoopCueId = cueId ?? string.Empty;
    }

    public void SetChoiceInputEnabled(bool value)
    {
        choiceInputEnabled = value;
        terminalInputReady = IsOpen && terminalPresentationReady && choiceInputEnabled;
        SetTerminalInputEnabled(terminalInputReady);
        RefreshKeyboardHint();
    }

    public void SetPrimaryActionInputEnabled(bool value)
    {
        primaryActionInputEnabled = value;
        RefreshKeyboardHint();
    }

    public void SetCloseInputEnabled(bool value)
    {
        closeInputEnabled = value;
        RefreshKeyboardHint();
    }

    public void SetRuntimePrompt(string prompt)
    {
        EnsureReferences();
        runtimePromptOverride = prompt ?? string.Empty;
        runtimePromptOverrideActive = !string.IsNullOrWhiteSpace(runtimePromptOverride);
        RefreshKeyboardHint();
    }

    public void ClearRuntimePrompt()
    {
        runtimePromptOverride = string.Empty;
        runtimePromptOverrideActive = false;
        RefreshKeyboardHint();
    }

    public void SetCloseTerminalWhenChoiceSubmitted(bool value)
    {
        closeTerminalWhenChoiceSubmitted = value;
    }

    public void SetSubmitPrimaryActionFromCurrentPage(bool value)
    {
        submitPrimaryActionFromCurrentPage = value;
    }

    public void SetDeferCustomActionCloseUntilExternalFade(bool value)
    {
        deferCustomActionCloseUntilExternalFade = value;
    }

    public void SetSubmitFeedback(HearthUiPressFeedback feedback)
    {
        submitFeedback = feedback;
    }

    public string GetReplayResidentId()
    {
        if (IsLobbyAssignmentTerminal())
        {
            return string.Empty;
        }

        string explicitId = NormalizeReplayResidentId(replayResidentId);
        if (!string.IsNullOrEmpty(explicitId))
        {
            return explicitId;
        }

        return InferReplayResidentId();
    }

    public void RequestRobotReplay()
    {
        if (!primaryActionInputEnabled)
        {
            return;
        }

        if (primaryAction == HearthTerminalPrimaryAction.Custom)
        {
            RequestCustomPrimaryAction();
            return;
        }

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

    public void RequestCustomPrimaryAction()
    {
        if (!primaryActionInputEnabled || customActionHandoffPending)
        {
            return;
        }

        PlayClip(submitClip);

        bool deferClose = closeTerminalWhenReplayStarts &&
                          deferCustomActionCloseUntilExternalFade &&
                          IsOpen;
        if (deferClose)
        {
            customActionHandoffPending = true;
            terminalInputReady = false;
            SetTerminalInputEnabled(false);
            if (selectionHighlighter != null)
            {
                selectionHighlighter.SetVisible(false);
            }
        }
        else if (closeTerminalWhenReplayStarts)
        {
            CloseTerminalInstant();
        }

        if (onCustomPrimaryAction != null)
        {
            onCustomPrimaryAction.Invoke();
        }
    }

    public void CompleteCustomActionHandoff()
    {
        customActionHandoffPending = false;
        if (IsOpen)
        {
            CloseTerminalInstant();
        }
    }

    public void CancelCustomActionHandoff()
    {
        if (!customActionHandoffPending)
        {
            return;
        }

        customActionHandoffPending = false;
        terminalInputReady = choiceInputEnabled;
        SetTerminalInputEnabled(terminalInputReady);
        SyncKeyboardFocusToCurrentPage();
        RefreshKeyboardHint();
    }

    public void RequestEnterUnit()
    {
        if (!primaryActionInputEnabled)
        {
            return;
        }

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
        OpenPostReplayAnalysis();
    }

    public void OpenPostReplayAnalysis()
    {
        postReplayAnalysisMode = true;
        postReplayChoicesAvailable = false;
        postReplayChoiceMode = false;
        choiceSubmitted = false;
        closeInputEnabled = false;
        primaryActionInputEnabled = false;
        if (!IsOpen)
        {
            OpenTerminal();
            postReplayAnalysisMode = true;
        }
        SetRuntimePrompt("ANALYSIS COMPLETE / PLEASE WAIT");
        RefreshKeyboardHint();
        if (onPostReplayChoiceShown != null)
        {
            onPostReplayChoiceShown.Invoke();
        }
    }

    public void ShowDispositionRecorded()
    {
        postReplayAnalysisMode = true;
        closeInputEnabled = false;
        primaryActionInputEnabled = false;
        SetRuntimePrompt("DISPOSITION RECORDED");
    }

    public void CompletePostReplayAnalysis(bool closeTerminal)
    {
        postReplayAnalysisMode = false;
        closeInputEnabled = true;
        primaryActionInputEnabled = true;
        ClearRuntimePrompt();
        if (closeTerminal && IsOpen)
        {
            CloseTerminal();
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

        if (runtimePromptText == null)
        {
            Transform foundPrompt = transform.Find("KeyboardNavigationRoot/RuntimePromptText");
            runtimePromptText = foundPrompt != null ? foundPrompt.GetComponent<TMP_Text>() : null;
        }

        EnsureKeyboardNavigationCanvasSorting();

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

        if (activeLoopSource == null)
        {
            Transform loopTransform = transform.Find("TerminalActiveLoop");
            activeLoopSource = loopTransform != null ? loopTransform.GetComponent<AudioSource>() : null;
        }

        ConfigureActiveLoopSource();
    }

    private HearthDialogueSurface CreateRuntimeDialogueSurface()
    {
        RectTransform parent = contentRoot != null
            ? contentRoot
            : transform as RectTransform;
        if (parent == null)
        {
            return null;
        }

        GameObject panelObject = new GameObject(
            "TerminalDialogueSurface_V2",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Outline),
            typeof(CanvasGroup));
        RectTransform panel = panelObject.GetComponent<RectTransform>();
        panel.SetParent(parent, false);
        ApplyTerminalDialogueRect(panel, false);
        panel.SetAsLastSibling();

        Image background = panelObject.GetComponent<Image>();
        background.color = new Color(0.025f, 0.035f, 0.07f, 0.94f);
        background.raycastTarget = false;
        Outline outline = panelObject.GetComponent<Outline>();
        outline.effectColor = new Color(0.28f, 0.68f, 0.94f, 0.9f);
        outline.effectDistance = new Vector2(2f, -2f);
        outline.useGraphicAlpha = false;

        TMP_Text speaker = CreateDialogueText(
            panel,
            "Speaker",
            new Vector2(0.035f, 0.72f),
            new Vector2(0.965f, 0.96f),
            ResolveTerminalSpeakerSize(),
            TextAlignmentOptions.TopLeft);
        TMP_Text body = CreateDialogueText(
            panel,
            "Body",
            new Vector2(0.035f, 0.22f),
            new Vector2(0.965f, 0.72f),
            ResolveTerminalBodySize(),
            TextAlignmentOptions.TopLeft);
        TMP_Text hint = CreateDialogueText(
            panel,
            "DialogueAdvanceHint",
            new Vector2(0.62f, 0.03f),
            new Vector2(0.965f, 0.2f),
            ResolveTerminalAdvanceSize(),
            TextAlignmentOptions.BottomRight);

        speaker.color = new Color(0.3f, 0.9f, 1f, 1f);
        body.color = new Color(0.9f, 0.95f, 1f, 1f);
        hint.color = new Color(0.3f, 0.9f, 1f, 0.92f);

        CanvasGroup group = panelObject.GetComponent<CanvasGroup>();
        HearthDialogueSurface surface = panelObject.AddComponent<HearthDialogueSurface>();
        surface.Configure(group, speaker, body, hint);
        surface.ApplyTypography(
            ResolveTerminalSpeakerSize(),
            ResolveTerminalBodySize(),
            ResolveTerminalAdvanceSize());
        surface.ApplyTerminalInternalLayout();
        surface.HideImmediate();
        return surface;
    }

    private HearthDialogueSurface CreateRuntimeMessageSurface()
    {
        RectTransform parent = contentRoot != null
            ? contentRoot
            : transform as RectTransform;
        if (parent == null)
        {
            return null;
        }

        GameObject panelObject = new GameObject(
            "TerminalMessageSurface_V2",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Outline),
            typeof(CanvasGroup));
        RectTransform panel = panelObject.GetComponent<RectTransform>();
        panel.SetParent(parent, false);
        ApplyTerminalDialogueRect(panel, true);
        panel.SetAsLastSibling();

        Image background = panelObject.GetComponent<Image>();
        background.color = new Color(0.025f, 0.035f, 0.07f, 0.965f);
        background.raycastTarget = false;
        Outline outline = panelObject.GetComponent<Outline>();
        outline.effectColor = new Color(0.28f, 0.68f, 0.94f, 0.94f);
        outline.effectDistance = new Vector2(2f, -2f);
        outline.useGraphicAlpha = false;

        TMP_Text speaker = CreateDialogueText(
            panel,
            "Speaker",
            new Vector2(0.05f, 0.76f),
            new Vector2(0.95f, 0.94f),
            ResolveTerminalSpeakerSize(),
            TextAlignmentOptions.TopLeft);
        TMP_Text body = CreateDialogueText(
            panel,
            "Body",
            new Vector2(0.05f, 0.20f),
            new Vector2(0.95f, 0.75f),
            ResolveTerminalBodySize(),
            TextAlignmentOptions.TopLeft);
        TMP_Text hint = CreateDialogueText(
            panel,
            "AdvanceHint",
            new Vector2(0.55f, 0.035f),
            new Vector2(0.95f, 0.19f),
            ResolveTerminalAdvanceSize(),
            TextAlignmentOptions.BottomRight);

        speaker.color = new Color(0.9f, 0.95f, 1f, 1f);
        body.color = new Color(0.9f, 0.95f, 1f, 1f);
        hint.color = new Color(0.3f, 0.9f, 1f, 0.92f);

        CanvasGroup group = panelObject.GetComponent<CanvasGroup>();
        HearthDialogueSurface surface = panelObject.AddComponent<HearthDialogueSurface>();
        surface.Configure(group, speaker, body, hint);
        surface.ApplyTypography(
            ResolveTerminalSpeakerSize(),
            ResolveTerminalBodySize(),
            ResolveTerminalAdvanceSize());
        surface.ApplyTerminalInternalLayout();
        surface.HideImmediate();
        return surface;
    }

    private void ApplyDialogueSurfaceProfile(
        HearthDialogueSurface surface,
        RectTransform panel,
        bool message)
    {
        if (surface == null)
        {
            return;
        }

        if (panel != null)
        {
            ApplyTerminalDialogueRect(panel, message);
        }

        surface.ApplyTypography(
            ResolveTerminalSpeakerSize(),
            ResolveTerminalBodySize(),
            ResolveTerminalAdvanceSize());
        surface.ApplyTerminalInternalLayout();
    }

    private void ApplyTerminalDialogueRect(RectTransform panel, bool message)
    {
        if (panel == null)
        {
            return;
        }

        if (uiLayoutProfile != null)
        {
            if (!message && ResolveTerminalMode() == HearthTerminalMode.LobbySync)
            {
                return;
            }
            HearthUiReferenceRect reference = message
                ? uiLayoutProfile.GetRegion(HearthUiLayoutRegion.TerminalContent)
                : uiLayoutProfile.GetRegion(HearthUiLayoutRegion.TerminalMessageLane);
            if (message)
            {
                float horizontalInset = reference.Width * 0.18f;
                float verticalInset = reference.Height * 0.18f;
                reference = new HearthUiReferenceRect(
                    reference.Left + horizontalInset,
                    reference.Top + verticalInset,
                    reference.Width - horizontalInset * 2f,
                    reference.Height - verticalInset * 2f);
            }
            reference.ApplyTopLeftAnchors(panel);
            return;
        }

        panel.anchorMin = message
            ? new Vector2(0.22f, 0.34f)
            : new Vector2(0.12f, 0.08f);
        panel.anchorMax = message
            ? new Vector2(0.78f, 0.68f)
            : new Vector2(0.88f, 0.31f);
        panel.offsetMin = Vector2.zero;
        panel.offsetMax = Vector2.zero;
    }

    private float ResolveTerminalSpeakerSize()
    {
        return uiThemeProfile != null
            ? uiThemeProfile.TerminalDialogueSpeakerFontSize
            : 52f;
    }

    private float ResolveTerminalBodySize()
    {
        return uiThemeProfile != null
            ? uiThemeProfile.TerminalDialogueBodyFontSize
            : 26f;
    }

    private float ResolveTerminalAdvanceSize()
    {
        return uiThemeProfile != null
            ? uiThemeProfile.TerminalDialogueAdvanceFontSize
            : 26f;
    }

    private void LinkExclusiveDialogueSurfaces()
    {
        if (dialogueSurface != null)
        {
            dialogueSurface.SetExclusivePeer(messageSurface);
        }
        if (messageSurface != null)
        {
            messageSurface.SetExclusivePeer(dialogueSurface);
        }
    }

    private TMP_Text CreateDialogueText(
        RectTransform parent,
        string objectName,
        Vector2 anchorMin,
        Vector2 anchorMax,
        float fontSize,
        TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        TMP_Text template = contentRoot != null
            ? contentRoot.GetComponentInChildren<TMP_Text>(true)
            : null;
        if (template != null && template.font != null)
        {
            text.font = template.font;
            text.fontSharedMaterial = template.fontSharedMaterial;
        }

        text.fontSize = fontSize;
        text.alignment = alignment;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Ellipsis;
        text.raycastTarget = false;
        return text;
    }

    private static Transform FindDescendantByName(Transform root, string objectName)
    {
        if (root == null)
        {
            return null;
        }

        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] != null && children[i].name == objectName)
            {
                return children[i];
            }
        }

        return null;
    }

    private static TMP_Text FindTextInChildren(Transform root, params string[] names)
    {
        if (root == null)
        {
            return null;
        }

        TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
        for (int nameIndex = 0; nameIndex < names.Length; nameIndex++)
        {
            for (int textIndex = 0; textIndex < texts.Length; textIndex++)
            {
                if (texts[textIndex] != null && texts[textIndex].name == names[nameIndex])
                {
                    return texts[textIndex];
                }
            }
        }

        return null;
    }

    private void EnsureKeyboardNavigationCanvasSorting()
    {
        Transform navigationRoot = transform.Find("KeyboardNavigationRoot");
        if (navigationRoot == null)
        {
            return;
        }

        Canvas navigationCanvas = navigationRoot.GetComponent<Canvas>();
        if (navigationCanvas == null)
        {
            navigationCanvas = navigationRoot.gameObject.AddComponent<Canvas>();
        }

        // The page reconstruction uses nested canvases at sorting order 10.
        // Keep the shared keyboard footer above every page surface, including
        // scene instances that still carry an old prefab override.
        navigationCanvas.overrideSorting = true;
        navigationCanvas.sortingOrder = 20;
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

    private void SetCanvasPresentationVisible(bool visible)
    {
        if (canvas != null)
        {
            canvas.enabled = visible;
        }
    }

    private void RefreshCanvasPresentationVisibility()
    {
        bool visible = Application.isPlaying
            ? IsOpen || !hideCanvasWhenClosed
            : showCanvasInEditMode;
        SetCanvasPresentationVisible(visible);
    }

    private void QueueEditorCanvasRefresh()
    {
#if UNITY_EDITOR
        if (Application.isPlaying || editorCanvasRefreshQueued)
        {
            return;
        }

        editorCanvasRefreshQueued = true;
        UnityEditor.EditorApplication.delayCall += ApplyQueuedEditorCanvasRefresh;
#endif
    }

#if UNITY_EDITOR
    private void ApplyQueuedEditorCanvasRefresh()
    {
        editorCanvasRefreshQueued = false;
        if (this == null || Application.isPlaying)
        {
            return;
        }

        EnsureReferences();
        RefreshCanvasPresentationVisibility();
    }
#endif

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
        ViewSwitchController preferredViewSwitch =
            ViewSwitchController.FindPreferredController(gameObject.scene);

        if (preferredViewSwitch != null)
        {
            viewSwitchController = preferredViewSwitch;

            PlayerInteraction currentInteraction = preferredViewSwitch.CurrentInteraction;
            Camera currentCamera = preferredViewSwitch.CurrentViewCamera;
            if (currentInteraction != null &&
                currentInteraction.gameObject.activeInHierarchy)
            {
                playerInteraction = currentInteraction;
            }

            if (IsUsablePlayerCamera(currentCamera) &&
                currentCamera.gameObject.activeInHierarchy)
            {
                playerCamera = currentCamera;
                return;
            }
        }

        ResolvePlayerControlLock();

        PlayerInteraction resolvedInteraction = FindBestPlayerInteraction();
        if (resolvedInteraction != null)
        {
            playerInteraction = resolvedInteraction;
        }

        Camera resolvedCamera = FindBestRuntimePlayerCamera();
        if (resolvedCamera != null)
        {
            playerCamera = resolvedCamera;
        }
    }

    private void ResolvePlayerControlLock()
    {
        if (playerControlLock != null &&
            playerControlLock.gameObject.scene.IsValid() &&
            playerControlLock.gameObject.scene == gameObject.scene)
        {
            return;
        }

        HearthPlayerControlLock[] locks =
            UnityEngine.Object.FindObjectsOfType<HearthPlayerControlLock>(true);
        HearthPlayerControlLock fallback = null;
        for (int i = 0; i < locks.Length; i++)
        {
            HearthPlayerControlLock candidate = locks[i];
            if (candidate == null ||
                !candidate.gameObject.scene.IsValid() ||
                candidate.gameObject.scene != gameObject.scene)
            {
                continue;
            }

            if (fallback == null)
            {
                fallback = candidate;
            }

            if (candidate.enabled && candidate.gameObject.activeInHierarchy)
            {
                playerControlLock = candidate;
                return;
            }
        }

        playerControlLock = fallback;
    }

    private HearthTvTerminalController FindOtherOpenTerminal()
    {
        return FindRegisteredOpenTerminal(this, gameObject.scene);
    }

    private bool ResolveOwnedTerminalCamera()
    {
        Transform resolvedHardwareRoot = ResolveTerminalHardwareRoot();
        if (resolvedHardwareRoot == null)
        {
            return false;
        }

        if (!IsValidOwnedTerminalCamera(terminalCamera, resolvedHardwareRoot))
        {
            terminalCamera = FindBestOwnedTerminalCamera(resolvedHardwareRoot);
        }

        bool resolved =
            IsValidOwnedTerminalCamera(terminalCamera, resolvedHardwareRoot);
        if (resolved)
        {
            SetWorldCamera(terminalCamera);
        }

        return resolved;
    }

    private Transform ResolveTerminalHardwareRoot()
    {
        if (terminalHardwareRoot != null &&
            transform.IsChildOf(terminalHardwareRoot) &&
            FindBestOwnedTerminalCamera(terminalHardwareRoot) != null)
        {
            return terminalHardwareRoot;
        }

        Transform cursor = transform.parent;
        Transform fallback = null;
        for (int depth = 0; cursor != null && depth < 8; depth++)
        {
            Camera ownedCamera = FindBestOwnedTerminalCamera(cursor);
            if (ownedCamera != null && fallback == null)
            {
                fallback = cursor;
            }

            string candidateName = cursor.name;
            if (ownedCamera != null &&
                (candidateName.StartsWith("TV", StringComparison.OrdinalIgnoreCase) ||
                 candidateName.IndexOf("terminal", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 candidateName.IndexOf("monitor", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                terminalHardwareRoot = cursor;
                return terminalHardwareRoot;
            }

            cursor = cursor.parent;
        }

        terminalHardwareRoot = fallback;
        return terminalHardwareRoot;
    }

    private Camera FindBestOwnedTerminalCamera(Transform hardwareRoot)
    {
        if (hardwareRoot == null)
        {
            return null;
        }

        Camera[] cameras = hardwareRoot.GetComponentsInChildren<Camera>(true);
        Camera fallback = null;
        for (int i = 0; i < cameras.Length; i++)
        {
            Camera candidate = cameras[i];
            if (!IsValidOwnedTerminalCamera(candidate, hardwareRoot))
            {
                continue;
            }

            if (fallback == null)
            {
                fallback = candidate;
            }

            string candidateName = candidate.name;
            if (string.Equals(candidateName, "Camera", StringComparison.OrdinalIgnoreCase) ||
                candidateName.IndexOf("terminal", StringComparison.OrdinalIgnoreCase) >= 0 ||
                candidateName.IndexOf("monitor", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return candidate;
            }
        }

        return fallback;
    }

    private bool IsValidOwnedTerminalCamera(Camera candidate, Transform hardwareRoot)
    {
        if (candidate == null ||
            hardwareRoot == null ||
            !candidate.transform.IsChildOf(hardwareRoot) ||
            candidate.transform.IsChildOf(transform) ||
            candidate.name.IndexOf("Transition", StringComparison.OrdinalIgnoreCase) >= 0 ||
            candidate.GetComponentInParent<FirstPersonMovement>() != null ||
            candidate.GetComponentInParent<PlayerInteraction>() != null ||
            candidate == playerCamera)
        {
            return false;
        }

        string candidatePath = GetHierarchyPath(candidate.transform);
        return candidatePath.IndexOf("First Person Camera", StringComparison.OrdinalIgnoreCase) < 0 &&
               candidatePath.IndexOf("Robot First Person Camera", StringComparison.OrdinalIgnoreCase) < 0;
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

            if (interaction.enabled &&
                interaction.InteractionEnabled &&
                interaction.gameObject.activeInHierarchy &&
                IsUsablePlayerCamera(interaction.mainCamera) &&
                (interaction.mainCamera.enabled || GetAudioListenerEnabled(interaction.mainCamera)))
            {
                return interaction;
            }
        }

        for (int i = 0; i < interactions.Length; i++)
        {
            PlayerInteraction interaction = interactions[i];
            if (interaction != null &&
                interaction.enabled &&
                interaction.gameObject.activeInHierarchy &&
                IsUsablePlayerCamera(interaction.mainCamera) &&
                (interaction.mainCamera.enabled || GetAudioListenerEnabled(interaction.mainCamera)))
            {
                return interaction;
            }
        }

        return fallback;
    }

    private Camera FindBestRuntimePlayerCamera()
    {
        if (playerInteraction != null &&
            playerInteraction.InteractionEnabled &&
            playerInteraction.gameObject.activeInHierarchy &&
            IsUsablePlayerCamera(playerInteraction.mainCamera) &&
            (playerInteraction.mainCamera.enabled || GetAudioListenerEnabled(playerInteraction.mainCamera)))
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

        if (playerInteraction != null && IsUsablePlayerCamera(playerInteraction.mainCamera))
        {
            return playerInteraction.mainCamera;
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
        if (locked == gameplayLockHeld)
        {
            return;
        }

        gameplayLockHeld = locked;

        if (viewSwitchController == null ||
            !viewSwitchController.gameObject.scene.IsValid() ||
            viewSwitchController.gameObject.scene != gameObject.scene)
        {
            viewSwitchController = ViewSwitchController.FindPreferredController(gameObject.scene);
        }

        if (viewSwitchController != null)
        {
            viewSwitchController.SetManualSwitchBlocked(this, locked);
        }

        if (!lockGameplayWhileOpen)
        {
            return;
        }

        ResolvePlayerControlLock();
        if (playerControlLock != null)
        {
            playerControlLock.SetControlsLocked(this, locked);
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

            if (playerRigidbody != null && !playerRigidbody.isKinematic)
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

    private HearthTerminalMode ResolveTerminalMode()
    {
        if (terminalMode != HearthTerminalMode.Auto)
        {
            return terminalMode;
        }

        Transform cursor = transform;
        while (cursor != null)
        {
            string objectName = cursor.name ?? string.Empty;
            if (objectName.IndexOf(
                    "Lobby",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                objectName.IndexOf(
                    "Assignment",
                    StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return HearthTerminalMode.LobbySync;
            }

            if (objectName.IndexOf(
                    "17F04",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                objectName.IndexOf(
                    "Home",
                    StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return HearthTerminalMode.Home;
            }

            cursor = cursor.parent;
        }

        return HearthTerminalMode.Doorway;
    }

    private void ApplyTerminalModeDefaults()
    {
        HearthTerminalMode resolvedMode = ResolveTerminalMode();
        if (resolvedMode == HearthTerminalMode.LobbySync)
        {
            keyboardHintLabel = "SPACE CLOSE TERMINAL";
            return;
        }

        keyboardHintLabel =
            "LEFT/RIGHT SELECT     SPACE CONFIRM     ESC EXIT";
    }

    private void MoveCompactFocus(int direction)
    {
        compactFocusIndex = Wrap(compactFocusIndex + direction, 3);
        keyboardFocusIndex = compactFocusIndex;
        PlayClip(focusMoveClip);

        if (compactFocusIndex <= 1 &&
            pages != null &&
            pages.Length > compactFocusIndex &&
            pages[compactFocusIndex] != null)
        {
            ShowPage(pages[compactFocusIndex]);
            return;
        }

        RefreshKeyboardHint();
    }

    private void SubmitCompactFocus()
    {
        if (compactFocusIndex != 2 || !primaryActionInputEnabled)
        {
            return;
        }

        if (submitFeedback != null)
        {
            submitFeedback.PlayFeedback();
        }

        PlayClip(submitClip);
        RequestRobotReplay();
    }

    private void RefreshCompactKeyboardHint()
    {
        RefreshTerminalViewState();

        if (keyboardHintText != null)
        {
            keyboardHintText.text = ResolveTerminalMode() ==
                HearthTerminalMode.LobbySync
                ? "SPACE CLOSE TERMINAL"
                : "LEFT/RIGHT SELECT     SPACE CONFIRM     ESC EXIT";
        }

        if (keyboardFocusText != null)
        {
            if (ResolveTerminalMode() == HearthTerminalMode.LobbySync)
            {
                keyboardFocusText.text = string.Empty;
            }
            else if (compactFocusIndex == 0)
            {
                keyboardFocusText.text = "BEFORE ACQUISITION";
            }
            else if (compactFocusIndex == 1)
            {
                keyboardFocusText.text = "AFTER ACQUISITION";
            }
            else
            {
                keyboardFocusText.text = replayFocusLabel;
            }
        }

        RefreshSelectionHighlighter();
        RefreshRuntimePrompt();
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
        if (!primaryActionInputEnabled)
        {
            return;
        }

        if (submitFeedback != null)
        {
            submitFeedback.PlayFeedback();
        }

        if (pageDrivenSelectionStates)
        {
            SubmitPageDrivenSelection();
            return;
        }

        if (submitPrimaryActionFromCurrentPage)
        {
            RequestRobotReplay();
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
        HearthTerminalMode resolvedMode = ResolveTerminalMode();
        if (resolvedMode == HearthTerminalMode.LobbySync)
        {
            compactFocusIndex = 0;
            keyboardFocusIndex = 0;
            return;
        }

        if (resolvedMode == HearthTerminalMode.Doorway ||
            resolvedMode == HearthTerminalMode.Home)
        {
            if (currentPageIndex == 0 || currentPageIndex == 1)
            {
                compactFocusIndex = currentPageIndex;
                keyboardFocusIndex = compactFocusIndex;
            }
            return;
        }

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
        HearthTerminalMode resolvedMode = ResolveTerminalMode();
        if (resolvedMode == HearthTerminalMode.LobbySync ||
            resolvedMode == HearthTerminalMode.Doorway ||
            resolvedMode == HearthTerminalMode.Home)
        {
            RefreshCompactKeyboardHint();
            return;
        }

        RefreshTerminalViewState();

        if (pageDrivenSelectionStates)
        {
            RefreshPageDrivenKeyboardHint();
            RefreshRuntimePrompt();
            return;
        }

        if (keyboardHintText != null)
        {
            keyboardHintText.text = keyboardHintLabel;
        }

        if (keyboardFocusText == null)
        {
            RefreshSelectionHighlighter();
            RefreshRuntimePrompt();
            return;
        }

        int normalCount = GetNormalCyclePageCount();
        if (keyboardFocusIndex >= normalCount)
        {
            keyboardFocusText.text = replayFocusLabel;
            RefreshSelectionHighlighter();
            RefreshRuntimePrompt();
            return;
        }

        keyboardFocusText.text = string.Format(pageFocusFormat, keyboardFocusIndex + 1, normalCount);
        RefreshSelectionHighlighter();
        RefreshRuntimePrompt();
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
        if (!primaryActionInputEnabled || (preventRepeatedChoiceSubmission && choiceSubmitted))
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

        if (normalized.Contains("17F04") || normalized.Contains("ROOM4"))
        {
            return "17F04";
        }

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

    private void RefreshRuntimePrompt()
    {
        if (runtimePromptText == null)
        {
            return;
        }

        string prompt = string.Empty;
        if (runtimePromptOverrideActive)
        {
            prompt = runtimePromptOverride;
        }
        else if (ResolveTerminalMode() == HearthTerminalMode.LobbySync)
        {
            prompt = primaryActionInputEnabled
                ? "SPACE  CLOSE TERMINAL"
                : HearthTerminalViewState.DefaultLockedMessage;
        }
        else if ((ResolveTerminalMode() == HearthTerminalMode.Doorway ||
                  ResolveTerminalMode() == HearthTerminalMode.Home) &&
                 compactFocusIndex == 2)
        {
            prompt = primaryActionInputEnabled
                ? "SPACE  " + terminalViewState.PrimaryActionLabel
                : HearthTerminalViewState.DefaultLockedMessage;
        }
        else if (pageDrivenSelectionStates && !primaryActionInputEnabled)
        {
            prompt = HearthTerminalViewState.DefaultLockedMessage;
        }
        else if (pageDrivenSelectionStates &&
                 postReplayChoicesAvailable &&
                 (postReplayChoiceMode || IsCurrentPageChoicePage()))
        {
            if (!choiceInputEnabled || !primaryActionInputEnabled)
            {
                prompt = "PLEASE WAIT";
            }
            else
            {
                string residentId = GetReplayResidentId();
                prompt = residentId == "17F03" || residentId == "17F04"
                    ? "UP / DOWN  SELECT     SPACE  CONFIRM"
                    : "LEFT / RIGHT  SELECT     SPACE  CONFIRM";
            }
        }
        else if (pageDrivenSelectionStates &&
                 keyboardFocusIndex == GetPreChoiceSelectionPageCount() - 1 &&
                 terminalViewState.PrimaryActionVisible)
        {
            prompt = "SPACE  " + terminalViewState.PrimaryActionLabel;
        }

        runtimePromptText.text = prompt;
        runtimePromptText.color = string.Equals(prompt, "PLEASE WAIT", System.StringComparison.Ordinal)
            ? runtimePromptWaitingColor
            : runtimePromptReadyColor;
        runtimePromptText.gameObject.SetActive(
            IsOpen && terminalPresentationReady && !string.IsNullOrEmpty(prompt));
    }

    private void RefreshTerminalViewState()
    {
        terminalViewState.SetVisible(IsOpen);
        terminalViewState.SetTerminalId(GetReplayResidentId());

        HearthTerminalMode resolvedMode = ResolveTerminalMode();
        if (resolvedMode == HearthTerminalMode.LobbySync)
        {
            terminalViewState.SetPage(0, 1);
            terminalViewState.SetNavigation(
                HearthTerminalNavigationTab.BeforeAcquisition,
                HearthTerminalFocusTarget.PrimaryAction);
            terminalViewState.SetPrimaryAction(
                HearthTerminalPrimaryActionType.Custom,
                !primaryActionInputEnabled,
                HearthTerminalViewState.DefaultLockedMessage,
                "CLOSE TERMINAL");
            terminalViewState.SetCanExit(closeInputEnabled);
            NotifyTerminalViewStateChanged();
            return;
        }

        if (resolvedMode == HearthTerminalMode.Doorway ||
            resolvedMode == HearthTerminalMode.Home)
        {
            if (resolvedMode == HearthTerminalMode.Home)
            {
                compactFocusIndex = 2;
            }

            int compactPageIndex = Mathf.Clamp(currentPageIndex, 0, 1);
            terminalViewState.SetPage(compactPageIndex, 2);
            HearthTerminalNavigationTab compactSelectedTab =
                compactPageIndex <= 0
                ? HearthTerminalNavigationTab.BeforeAcquisition
                : HearthTerminalNavigationTab.AfterAcquisition;
            HearthTerminalFocusTarget compactFocusTarget =
                compactFocusIndex == 2
                ? HearthTerminalFocusTarget.PrimaryAction
                : compactFocusIndex == 0
                    ? HearthTerminalFocusTarget.BeforeAcquisitionTab
                    : HearthTerminalFocusTarget.AfterAcquisitionTab;
            terminalViewState.SetNavigation(
                compactSelectedTab,
                compactFocusTarget);
            HearthTerminalPrimaryActionType compactAction =
                ResolveTerminalViewActionType();
            terminalViewState.SetPrimaryAction(
                compactAction,
                !primaryActionInputEnabled,
                HearthTerminalViewState.DefaultLockedMessage,
                compactAction == HearthTerminalPrimaryActionType.Custom
                    ? ResolveTerminalViewCustomActionLabel()
                    : string.Empty);
            terminalViewState.SetCanExit(
                closeInputEnabled && !postReplayAnalysisMode);
            NotifyTerminalViewStateChanged();
            return;
        }

        int visiblePageCount = pageDrivenSelectionStates
            ? (postReplayChoicesAvailable
                ? Mathf.Max(GetPostReplayNavigationPageCount(), GetChoicePageCount())
                : GetPreChoiceSelectionPageCount())
            : Mathf.Max(1, GetNormalCyclePageCount());
        int visiblePageIndex = Mathf.Clamp(currentPageIndex, 0, Mathf.Max(0, visiblePageCount - 1));
        terminalViewState.SetPage(visiblePageIndex, Mathf.Max(1, visiblePageCount));

        int actionIndex = Mathf.Max(0, GetPreChoiceSelectionPageCount() - 1);
        HearthTerminalNavigationTab selectedTab =
            keyboardFocusIndex <= 0
                ? HearthTerminalNavigationTab.BeforeAcquisition
                : HearthTerminalNavigationTab.AfterAcquisition;
        HearthTerminalFocusTarget focusTarget =
            keyboardFocusIndex == actionIndex
                ? HearthTerminalFocusTarget.PrimaryAction
                : keyboardFocusIndex <= 0
                    ? HearthTerminalFocusTarget.BeforeAcquisitionTab
                    : HearthTerminalFocusTarget.AfterAcquisitionTab;
        terminalViewState.SetNavigation(selectedTab, focusTarget);

        HearthTerminalPrimaryActionType actionType = ResolveTerminalViewActionType();
        terminalViewState.SetPrimaryAction(
            actionType,
            !primaryActionInputEnabled,
            HearthTerminalViewState.DefaultLockedMessage,
            actionType == HearthTerminalPrimaryActionType.Custom
                ? ResolveTerminalViewCustomActionLabel()
                : string.Empty);
        terminalViewState.SetCanExit(closeInputEnabled);

        NotifyTerminalViewStateChanged();
    }

    private void NotifyTerminalViewStateChanged()
    {
        if (TerminalViewStateChanged != null)
        {
            TerminalViewStateChanged(terminalViewState);
        }
    }

    private HearthTerminalPrimaryActionType ResolveTerminalViewActionType()
    {
        string residentId = GetReplayResidentId();
        if (string.Equals(residentId, "17F04", StringComparison.OrdinalIgnoreCase))
        {
            return HearthTerminalPrimaryActionType.EnterHome;
        }

        switch (primaryAction)
        {
            case HearthTerminalPrimaryAction.RequestReplay:
                return HearthTerminalPrimaryActionType.ReviewArchivedEvent;
            case HearthTerminalPrimaryAction.EnterUnit:
                return HearthTerminalPrimaryActionType.EnterUnit;
            case HearthTerminalPrimaryAction.Custom:
                return HearthTerminalPrimaryActionType.Custom;
            default:
                return HearthTerminalPrimaryActionType.None;
        }
    }

    private string ResolveTerminalViewCustomActionLabel()
    {
        if (IsLobbyAssignmentTerminal())
        {
            return "CLOSE TERMINAL";
        }

        string label = replayFocusLabel ?? string.Empty;
        const string spaceSuffix = " | SPACE";
        if (label.EndsWith(spaceSuffix, StringComparison.OrdinalIgnoreCase))
        {
            label = label.Substring(0, label.Length - spaceSuffix.Length);
        }

        return string.IsNullOrWhiteSpace(label)
            ? "CONFIRM"
            : label.Trim();
    }

    private bool IsLobbyAssignmentTerminal()
    {
        Transform cursor = transform;
        while (cursor != null)
        {
            if (cursor.name.IndexOf("Lobby", StringComparison.OrdinalIgnoreCase) >= 0 ||
                cursor.name.IndexOf("Assignment", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            cursor = cursor.parent;
        }

        return false;
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

    private void CloseTerminalInstant(bool invokeClosedEvent = true)
    {
        if (!IsOpen || terminalSessionCleanupInProgress)
        {
            return;
        }

        terminalSessionCleanupInProgress = true;

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

        customActionHandoffPending = false;
        StopActiveAudioLoop();
        terminalInputReady = false;
        terminalPresentationReady = false;
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
        OpenTerminals.Remove(this);
        RefreshTerminalViewState();
        RefreshRuntimePrompt();
        RefreshCanvasPresentationVisibility();

        if (invokeClosedEvent && onClosed != null)
        {
            onClosed.Invoke();
        }

        terminalSessionCleanupInProgress = false;
    }

    private void PlayClip(AudioClip clip)
    {
        if (audioSource == null || clip == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip, audioVolume);
    }

    private void ConfigureActiveLoopSource()
    {
        if (activeLoopSource == null)
        {
            return;
        }

        activeLoopSource.playOnAwake = false;
        activeLoopSource.loop = true;
        activeLoopSource.volume = activeLoopVolume;
        activeLoopSource.spatialBlend = 0f;
    }

    private void StartActiveAudioLoop()
    {
        if (activeLoopCuePlayer != null && !string.IsNullOrWhiteSpace(activeLoopCueId))
        {
            if (activeLoopCuePlayer.StartCueLoop(activeLoopCueId))
            {
                return;
            }
        }

        AudioClip loopClip = activeLoopClip != null
            ? activeLoopClip
            : activeLoopSource != null ? activeLoopSource.clip : null;
        if (activeLoopSource == null || loopClip == null)
        {
            return;
        }

        ConfigureActiveLoopSource();
        if (activeLoopSource.isPlaying && activeLoopSource.clip == loopClip)
        {
            return;
        }

        activeLoopSource.Stop();
        activeLoopSource.clip = loopClip;
        activeLoopSource.Play();
    }

    private void StopActiveAudioLoop()
    {
        if (activeLoopCuePlayer != null && !string.IsNullOrWhiteSpace(activeLoopCueId))
        {
            activeLoopCuePlayer.StopCue(activeLoopCueId);
        }

        if (activeLoopSource != null)
        {
            activeLoopSource.Stop();
        }
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
