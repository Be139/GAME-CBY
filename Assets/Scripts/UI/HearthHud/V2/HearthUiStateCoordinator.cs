using System;
using System.Collections.Generic;
using UnityEngine;

public enum HearthUiLayer
{
    Persistent,
    Dialogue,
    Interaction,
    Terminal,
    Modal,
    Takeover
}

public enum HearthUiRuntimeState
{
    Exploration,
    FormalDialogue,
    AuxiliaryCommunication,
    HumanMenu,
    CompanionMenu,
    SyncTerminal,
    HouseholdTerminal,
    ForcedReplay,
    DecisionModal,
    PhotoArchive,
    Shutdown,
    LowTrustTakeover
}

[Serializable]
public struct HearthUiVisibilityState : IEquatable<HearthUiVisibilityState>
{
    [SerializeField] private bool persistent;
    [SerializeField] private bool dialogue;
    [SerializeField] private bool interaction;
    [SerializeField] private bool terminal;
    [SerializeField] private bool modal;
    [SerializeField] private bool takeover;

    public HearthUiVisibilityState(
        bool persistent,
        bool dialogue,
        bool interaction,
        bool terminal,
        bool modal,
        bool takeover)
    {
        this.persistent = persistent;
        this.dialogue = dialogue;
        this.interaction = interaction;
        this.terminal = terminal;
        this.modal = modal;
        this.takeover = takeover;
    }

    public static HearthUiVisibilityState Gameplay
    {
        get
        {
            return new HearthUiVisibilityState(
                true,
                false,
                true,
                false,
                false,
                false);
        }
    }

    public bool Persistent { get { return persistent; } }
    public bool Dialogue { get { return dialogue; } }
    public bool Interaction { get { return interaction; } }
    public bool Terminal { get { return terminal; } }
    public bool Modal { get { return modal; } }
    public bool Takeover { get { return takeover; } }

    public bool GetLayer(HearthUiLayer layer)
    {
        switch (layer)
        {
            case HearthUiLayer.Persistent:
                return persistent;
            case HearthUiLayer.Dialogue:
                return dialogue;
            case HearthUiLayer.Interaction:
                return interaction;
            case HearthUiLayer.Terminal:
                return terminal;
            case HearthUiLayer.Modal:
                return modal;
            case HearthUiLayer.Takeover:
                return takeover;
            default:
                throw new ArgumentOutOfRangeException("layer", layer, "Unknown HEARTH UI layer.");
        }
    }

    public HearthUiVisibilityState WithLayer(HearthUiLayer layer, bool visible)
    {
        HearthUiVisibilityState copy = this;
        switch (layer)
        {
            case HearthUiLayer.Persistent:
                copy.persistent = visible;
                break;
            case HearthUiLayer.Dialogue:
                copy.dialogue = visible;
                break;
            case HearthUiLayer.Interaction:
                copy.interaction = visible;
                break;
            case HearthUiLayer.Terminal:
                copy.terminal = visible;
                break;
            case HearthUiLayer.Modal:
                copy.modal = visible;
                break;
            case HearthUiLayer.Takeover:
                copy.takeover = visible;
                break;
            default:
                throw new ArgumentOutOfRangeException("layer", layer, "Unknown HEARTH UI layer.");
        }

        return copy;
    }

    public bool Equals(HearthUiVisibilityState other)
    {
        return persistent == other.persistent &&
            dialogue == other.dialogue &&
            interaction == other.interaction &&
            terminal == other.terminal &&
            modal == other.modal &&
            takeover == other.takeover;
    }

    public override bool Equals(object obj)
    {
        return obj is HearthUiVisibilityState && Equals((HearthUiVisibilityState)obj);
    }

    public override int GetHashCode()
    {
        int hash = 17;
        hash = (hash * 31) + (persistent ? 1 : 0);
        hash = (hash * 31) + (dialogue ? 1 : 0);
        hash = (hash * 31) + (interaction ? 1 : 0);
        hash = (hash * 31) + (terminal ? 1 : 0);
        hash = (hash * 31) + (modal ? 1 : 0);
        hash = (hash * 31) + (takeover ? 1 : 0);
        return hash;
    }
}

[DefaultExecutionOrder(-500)]
[DisallowMultipleComponent]
public sealed class HearthUiStateCoordinator : MonoBehaviour
{
    [Header("Runtime Resolution")]
    [SerializeField] private bool automaticallyResolveRuntimeState = true;
    [SerializeField] private bool applyResolvedStateToCanvasGroups;
    [SerializeField] private HearthFirstPersonHudController humanHud;
    [SerializeField] private ViewSwitchController viewSwitchController;
    [SerializeField] private HearthPlayerControlLock playerControlLock;
    [SerializeField] private MinLoopSubtitlePlayer[] subtitlePlayers =
        new MinLoopSubtitlePlayer[0];
    [SerializeField] private HearthShutdownChallenge[] takeoverChallenges =
        new HearthShutdownChallenge[0];
    [SerializeField] private HearthLobbyHudOverlay[] lobbyHudOverlays =
        new HearthLobbyHudOverlay[0];
    [SerializeField] private HearthFirstPersonHudPage[] humanPages =
        new HearthFirstPersonHudPage[0];

    [Header("Layer Bindings")]
    [SerializeField] private CanvasGroup persistentLayer;
    [SerializeField] private CanvasGroup dialogueLayer;
    [SerializeField] private CanvasGroup interactionLayer;
    [SerializeField] private CanvasGroup terminalLayer;
    [SerializeField] private CanvasGroup modalLayer;
    [SerializeField] private CanvasGroup takeoverLayer;

    [Header("Requested State")]
    [SerializeField] private HearthUiVisibilityState requestedState =
        HearthUiVisibilityState.Gameplay;

    private HearthUiVisibilityState resolvedState;
    private HearthUiRuntimeState resolvedRuntimeState =
        HearthUiRuntimeState.Exploration;
    private bool hasAppliedState;
    private readonly HashSet<int> externalModalOwnerIds =
        new HashSet<int>();

    public event Action<HearthUiVisibilityState> ResolvedStateChanged;

    public HearthUiVisibilityState RequestedState { get { return requestedState; } }
    public HearthUiVisibilityState ResolvedState { get { return resolvedState; } }
    public HearthUiRuntimeState ResolvedRuntimeState
    {
        get { return resolvedRuntimeState; }
    }
    public bool HasResolvedState { get { return hasAppliedState; } }
    public bool AutomaticallyResolveRuntimeState
    {
        get { return automaticallyResolveRuntimeState; }
    }

    public bool AppliesResolvedStateToCanvasGroups
    {
        get { return applyResolvedStateToCanvasGroups; }
    }

    public HearthFirstPersonHudController HumanHud
    {
        get { return humanHud; }
    }

    public bool HasHumanHudBinding
    {
        get { return humanHud != null; }
    }

    public bool CanOpenHumanMenu
    {
        get
        {
            return hasAppliedState &&
                requestedState.Persistent &&
                requestedState.Interaction &&
                !requestedState.Dialogue &&
                !requestedState.Terminal &&
                !requestedState.Modal &&
                !requestedState.Takeover;
        }
    }

    public bool HasDuplicateLayerBindings
    {
        get
        {
            return IsDuplicateLayerBinding(dialogueLayer, persistentLayer) ||
                IsDuplicateLayerBinding(
                    interactionLayer,
                    persistentLayer,
                    dialogueLayer) ||
                IsDuplicateLayerBinding(
                    terminalLayer,
                    persistentLayer,
                    dialogueLayer,
                    interactionLayer) ||
                IsDuplicateLayerBinding(
                    modalLayer,
                    persistentLayer,
                    dialogueLayer,
                    interactionLayer,
                    terminalLayer) ||
                IsDuplicateLayerBinding(
                    takeoverLayer,
                    persistentLayer,
                    dialogueLayer,
                    interactionLayer,
                    terminalLayer,
                    modalLayer);
        }
    }

    private void Awake()
    {
        ResolveRuntimeSources();
        Refresh();
    }

    private void OnEnable()
    {
        ResolveRuntimeSources();
        Refresh();
    }

    private void OnDisable()
    {
        if (humanHud != null)
        {
            humanHud.SetExternalPersistentPresentationSuppressed(false);
        }

        if (subtitlePlayers != null)
        {
            for (int i = 0; i < subtitlePlayers.Length; i++)
            {
                if (subtitlePlayers[i] != null)
                {
                    subtitlePlayers[i].SetExternalPresentationSuppressed(false);
                }
            }
        }

        if (lobbyHudOverlays != null)
        {
            for (int i = 0; i < lobbyHudOverlays.Length; i++)
            {
                if (lobbyHudOverlays[i] != null)
                {
                    lobbyHudOverlays[i].SetExternalPresentationSuppressed(false);
                }
            }
        }
    }

    private void Update()
    {
        if (automaticallyResolveRuntimeState)
        {
            RefreshRuntimeState();
        }
    }

    public void ConfigureRuntimeSources(
        HearthFirstPersonHudController newHumanHud,
        ViewSwitchController newViewSwitchController,
        MinLoopSubtitlePlayer[] newSubtitlePlayers,
        HearthShutdownChallenge[] newTakeoverChallenges)
    {
        humanHud = newHumanHud;
        viewSwitchController = newViewSwitchController;
        subtitlePlayers = newSubtitlePlayers ?? new MinLoopSubtitlePlayer[0];
        takeoverChallenges =
            newTakeoverChallenges ?? new HearthShutdownChallenge[0];
        humanPages = humanHud != null
            ? humanHud.GetComponentsInChildren<HearthFirstPersonHudPage>(true)
            : new HearthFirstPersonHudPage[0];
        ResolveRuntimeSources();
        Refresh();
    }

    public void SetRuntimeIntegration(
        bool resolveAutomatically,
        bool applyCanvasGroups)
    {
        automaticallyResolveRuntimeState = resolveAutomatically;
        applyResolvedStateToCanvasGroups = applyCanvasGroups;
        Refresh();
    }

    public void Configure(
        CanvasGroup newPersistentLayer,
        CanvasGroup newDialogueLayer,
        CanvasGroup newInteractionLayer,
        CanvasGroup newTerminalLayer,
        CanvasGroup newModalLayer,
        CanvasGroup newTakeoverLayer)
    {
        persistentLayer = newPersistentLayer;
        dialogueLayer = newDialogueLayer;
        interactionLayer = newInteractionLayer;
        terminalLayer = newTerminalLayer;
        modalLayer = newModalLayer;
        takeoverLayer = newTakeoverLayer;
        Refresh();
    }

    public void ApplyRequestedState(HearthUiVisibilityState state)
    {
        requestedState = state;
        RefreshResolvedState();
    }

    public void SetLayerRequested(HearthUiLayer layer, bool visible)
    {
        requestedState = requestedState.WithLayer(layer, visible);
        RefreshResolvedState();
    }

    public void SetExternalModalRequest(
        UnityEngine.Object owner,
        bool visible)
    {
        if (owner == null)
        {
            return;
        }

        int ownerId = owner.GetInstanceID();
        if (visible)
        {
            externalModalOwnerIds.Add(ownerId);
        }
        else
        {
            externalModalOwnerIds.Remove(ownerId);
        }

        if (automaticallyResolveRuntimeState)
        {
            RefreshRuntimeState();
        }
    }

    public void Refresh()
    {
        if (automaticallyResolveRuntimeState)
        {
            RefreshRuntimeState();
            return;
        }

        RefreshResolvedState();
    }

    public void RefreshRuntimeState()
    {
        ResolveRuntimeSources();
        requestedState = BuildRuntimeRequestedState();
        RefreshResolvedState();
    }

    public void ResolveRuntimeSources()
    {
        if (humanHud == null)
        {
            humanHud =
                GetComponent<HearthFirstPersonHudController>() ??
                GetComponentInParent<HearthFirstPersonHudController>(true);
        }

        if (humanHud != null &&
            (humanPages == null || humanPages.Length == 0))
        {
            humanPages =
                humanHud.GetComponentsInChildren<HearthFirstPersonHudPage>(true);
        }

        if (!Application.isPlaying)
        {
            return;
        }

        if (viewSwitchController == null &&
            gameObject.scene.IsValid() &&
            gameObject.scene.isLoaded)
        {
            viewSwitchController =
                ViewSwitchController.FindPreferredController(gameObject.scene);
        }

        if (playerControlLock == null)
        {
            playerControlLock =
                UnityEngine.Object.FindObjectOfType<HearthPlayerControlLock>();
        }

        if (subtitlePlayers == null || subtitlePlayers.Length == 0)
        {
            subtitlePlayers =
                UnityEngine.Object.FindObjectsOfType<MinLoopSubtitlePlayer>(true);
        }

        if (takeoverChallenges == null || takeoverChallenges.Length == 0)
        {
            takeoverChallenges =
                UnityEngine.Object.FindObjectsOfType<HearthShutdownChallenge>(true);
        }

        if (lobbyHudOverlays == null || lobbyHudOverlays.Length == 0)
        {
            lobbyHudOverlays =
                UnityEngine.Object.FindObjectsOfType<HearthLobbyHudOverlay>(true);
        }
    }

    private HearthUiVisibilityState BuildRuntimeRequestedState()
    {
        bool humanView =
            viewSwitchController == null ||
            (!viewSwitchController.IsSwitching &&
             viewSwitchController.CurrentMode ==
                ViewSwitchController.ViewMode.Human);
        bool terminal = HearthTvTerminalController.AnyTerminalOpen;
        HearthTvTerminalController activeTerminal =
            HearthTvTerminalController.ActiveTerminal;
        bool preserveHumanHudDuringTerminal =
            activeTerminal != null &&
            (activeTerminal.PreservesHumanHud ||
             activeTerminal.IsPostReplayAnalysisMode);
        bool dialogue = IsAnySubtitlePlaying();
        bool interactionLocked =
            Time.timeScale <= 0f ||
            (playerControlLock != null &&
             playerControlLock.IsLocked(HearthPlayerControlMask.Interaction));

        HearthFirstPersonHudPageId pageId = humanHud != null
            ? humanHud.CurrentPageId
            : HearthFirstPersonHudPageId.None;
        bool takeover =
            IsFullscreenTakeoverPage(pageId) ||
            IsAnyTakeoverChallengeRunning();
        bool modal =
            !takeover &&
            (IsModalPage(pageId) || externalModalOwnerIds.Count > 0);
        bool interaction =
            humanView &&
            !interactionLocked &&
            !dialogue &&
            !terminal &&
            !modal &&
            !takeover;

        return new HearthUiVisibilityState(
            humanView && (!terminal || preserveHumanHudDuringTerminal),
            dialogue,
            interaction,
            terminal,
            modal,
            takeover);
    }

    private void RefreshResolvedState()
    {
        HearthUiVisibilityState nextResolvedState = Resolve(requestedState);
        resolvedRuntimeState = ResolveRuntimeState(
            requestedState,
            humanHud != null ? humanHud.CurrentPageId : HearthFirstPersonHudPageId.None);

        if (applyResolvedStateToCanvasGroups)
        {
            ApplyResolvedLayerBindings(nextResolvedState);
        }
        ApplyExternalPresentationSuppression(nextResolvedState);

        bool changed = !hasAppliedState || !resolvedState.Equals(nextResolvedState);
        resolvedState = nextResolvedState;
        hasAppliedState = true;

        if (changed && ResolvedStateChanged != null)
        {
            ResolvedStateChanged(resolvedState);
        }
    }

    public static HearthUiVisibilityState Resolve(HearthUiVisibilityState requested)
    {
        if (requested.Takeover)
        {
            return new HearthUiVisibilityState(false, false, false, false, false, true);
        }

        if (requested.Modal)
        {
            return new HearthUiVisibilityState(
                requested.Terminal && requested.Persistent,
                false,
                false,
                requested.Terminal,
                true,
                false);
        }

        if (requested.Terminal)
        {
            return new HearthUiVisibilityState(
                requested.Persistent,
                requested.Dialogue,
                false,
                true,
                false,
                false);
        }

        return new HearthUiVisibilityState(
            requested.Persistent,
            requested.Dialogue,
            requested.Interaction && !requested.Dialogue,
            false,
            false,
            false);
    }

    private HearthUiRuntimeState ResolveRuntimeState(
        HearthUiVisibilityState state,
        HearthFirstPersonHudPageId pageId)
    {
        if (state.Takeover)
        {
            return IsAnyTakeoverChallengeRunning()
                ? HearthUiRuntimeState.LowTrustTakeover
                : HearthUiRuntimeState.Shutdown;
        }

        if (state.Modal)
        {
            return pageId == HearthFirstPersonHudPageId.Slide07Photo2023
                ? HearthUiRuntimeState.PhotoArchive
                : HearthUiRuntimeState.DecisionModal;
        }

        if (state.Terminal)
        {
            return state.Persistent
                ? HearthUiRuntimeState.SyncTerminal
                : HearthUiRuntimeState.HouseholdTerminal;
        }

        DialogueChannel? dialogueChannel = GetActiveDialogueChannel();
        if (dialogueChannel.HasValue)
        {
            return dialogueChannel.Value == DialogueChannel.Formal
                ? HearthUiRuntimeState.FormalDialogue
                : HearthUiRuntimeState.AuxiliaryCommunication;
        }

        if (pageId != HearthFirstPersonHudPageId.None &&
            pageId != HearthFirstPersonHudPageId.Slide01PersistentHud)
        {
            return HearthUiRuntimeState.HumanMenu;
        }

        if (viewSwitchController != null &&
            viewSwitchController.CurrentMode ==
                ViewSwitchController.ViewMode.Companion)
        {
            return HearthUiRuntimeState.CompanionMenu;
        }

        return HearthUiRuntimeState.Exploration;
    }

    private DialogueChannel? GetActiveDialogueChannel()
    {
        if (subtitlePlayers == null)
        {
            return null;
        }

        for (int i = 0; i < subtitlePlayers.Length; i++)
        {
            MinLoopSubtitlePlayer player = subtitlePlayers[i];
            if (player != null && player.IsPlaying)
            {
                return player.ActiveChannel;
            }
        }

        return null;
    }

    private bool IsAnySubtitlePlaying()
    {
        if (subtitlePlayers == null)
        {
            return false;
        }

        for (int i = 0; i < subtitlePlayers.Length; i++)
        {
            if (subtitlePlayers[i] != null && subtitlePlayers[i].IsPlaying)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsAnyTerminalSubtitlePlaying()
    {
        if (subtitlePlayers == null)
        {
            return false;
        }

        for (int i = 0; i < subtitlePlayers.Length; i++)
        {
            MinLoopSubtitlePlayer player = subtitlePlayers[i];
            if (player != null &&
                player.IsPlaying &&
                player.ActiveContext == HearthSubtitleContext.Terminal)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsAnyTakeoverChallengeRunning()
    {
        if (takeoverChallenges == null)
        {
            return false;
        }

        for (int i = 0; i < takeoverChallenges.Length; i++)
        {
            if (takeoverChallenges[i] != null &&
                takeoverChallenges[i].IsRunning)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsFullscreenTakeoverPage(
        HearthFirstPersonHudPageId pageId)
    {
        if (humanPages != null)
        {
            for (int i = 0; i < humanPages.Length; i++)
            {
                HearthFirstPersonHudPage page = humanPages[i];
                if (page != null &&
                    page.PageId == pageId &&
                    page.FullscreenTakeover)
                {
                    return true;
                }
            }
        }

        return pageId == HearthFirstPersonHudPageId.Slide15EndingGraceful ||
            pageId == HearthFirstPersonHudPageId.Slide16EndingForced ||
            pageId == HearthFirstPersonHudPageId.Slide17EndingCompanion;
    }

    private static bool IsModalPage(HearthFirstPersonHudPageId pageId)
    {
        return pageId != HearthFirstPersonHudPageId.None &&
            pageId != HearthFirstPersonHudPageId.Slide01PersistentHud &&
            pageId != HearthFirstPersonHudPageId.Slide02TrustDelta &&
            pageId != HearthFirstPersonHudPageId.Slide15EndingGraceful &&
            pageId != HearthFirstPersonHudPageId.Slide16EndingForced &&
            pageId != HearthFirstPersonHudPageId.Slide17EndingCompanion;
    }

    private void ApplyResolvedLayerBindings(HearthUiVisibilityState state)
    {
        ApplyUniqueLayerBinding(persistentLayer, 0, state);
        ApplyUniqueLayerBinding(dialogueLayer, 1, state);
        ApplyUniqueLayerBinding(interactionLayer, 2, state);
        ApplyUniqueLayerBinding(terminalLayer, 3, state);
        ApplyUniqueLayerBinding(modalLayer, 4, state);
        ApplyUniqueLayerBinding(takeoverLayer, 5, state);
    }

    private void ApplyExternalPresentationSuppression(
        HearthUiVisibilityState state)
    {
        if (humanHud != null)
        {
            humanHud.SetExternalPersistentPresentationSuppressed(
                !state.Persistent);
        }

        if (subtitlePlayers != null)
        {
            for (int i = 0; i < subtitlePlayers.Length; i++)
            {
                MinLoopSubtitlePlayer player = subtitlePlayers[i];
                if (player != null)
                {
                    bool suppressDialogue =
                        player.IsPlaying && !state.Dialogue;
                    player.SetExternalPresentationSuppressed(
                        suppressDialogue);
                }
            }
        }

        bool suppressAuxiliaryHumanHud = !state.Persistent;
        if (lobbyHudOverlays != null)
        {
            for (int i = 0; i < lobbyHudOverlays.Length; i++)
            {
                HearthLobbyHudOverlay overlay = lobbyHudOverlays[i];
                if (overlay != null)
                {
                    overlay.SetExternalPresentationSuppressed(
                        suppressAuxiliaryHumanHud);
                }
            }
        }
    }

    private void ApplyUniqueLayerBinding(
        CanvasGroup group,
        int bindingIndex,
        HearthUiVisibilityState state)
    {
        if (group == null || HasEarlierBinding(group, bindingIndex))
        {
            return;
        }

        bool visible =
            (group == persistentLayer && state.Persistent) ||
            (group == dialogueLayer && state.Dialogue) ||
            (group == interactionLayer && state.Interaction) ||
            (group == terminalLayer && state.Terminal) ||
            (group == modalLayer && state.Modal) ||
            (group == takeoverLayer && state.Takeover);
        bool acceptsInput =
            (group == terminalLayer && state.Terminal) ||
            (group == modalLayer && state.Modal) ||
            (group == takeoverLayer && state.Takeover);

        group.alpha = visible ? 1f : 0f;
        group.interactable = visible && acceptsInput;
        group.blocksRaycasts = visible && acceptsInput;
    }

    private bool HasEarlierBinding(CanvasGroup group, int bindingIndex)
    {
        if (bindingIndex > 0 && group == persistentLayer)
        {
            return true;
        }

        if (bindingIndex > 1 && group == dialogueLayer)
        {
            return true;
        }

        if (bindingIndex > 2 && group == interactionLayer)
        {
            return true;
        }

        if (bindingIndex > 3 && group == terminalLayer)
        {
            return true;
        }

        return bindingIndex > 4 && group == modalLayer;
    }

    private static bool IsDuplicateLayerBinding(
        CanvasGroup candidate,
        params CanvasGroup[] earlierBindings)
    {
        if (candidate == null || earlierBindings == null)
        {
            return false;
        }

        for (int i = 0; i < earlierBindings.Length; i++)
        {
            if (candidate == earlierBindings[i])
            {
                return true;
            }
        }

        return false;
    }
}
