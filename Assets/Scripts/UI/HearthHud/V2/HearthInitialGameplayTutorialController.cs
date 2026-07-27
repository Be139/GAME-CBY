using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class HearthInitialGameplayTutorialController : MonoBehaviour
{
    private const string DefaultCompletionKey =
        "HEARTH.UI.V2.InitialHumanTutorial.Completed";

    private static bool completedThisRun;

    [Header("Presentation")]
    [SerializeField] private HearthActionHintPresenter presenter;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Runtime State Sources")]
    [SerializeField] private HearthFirstPersonHudController humanHud;
    [SerializeField] private HearthUiStateCoordinator uiStateCoordinator;
    [SerializeField] private ViewSwitchController viewSwitchController;
    [SerializeField] private PlayerInteraction humanInteraction;
    [SerializeField] private MinLoopSubtitlePlayer[] subtitlePlayers =
        new MinLoopSubtitlePlayer[0];

    [Header("Timing")]
    [SerializeField, Min(0.1f)] private float effectiveGameplaySeconds = 10f;
    [SerializeField, Min(0f)] private float fadeOutSeconds = 0.35f;
    [SerializeField] private bool persistCompletion = true;
    [SerializeField] private string completionKey = DefaultCompletionKey;

    private float elapsedGameplaySeconds;
    private float fadeElapsedSeconds;
    private bool fading;
    private bool finished;

    public float ElapsedGameplaySeconds
    {
        get { return elapsedGameplaySeconds; }
    }

    public bool IsRunning
    {
        get { return !finished && !fading; }
    }

    public HearthUiStateCoordinator UiStateCoordinator
    {
        get { return uiStateCoordinator; }
    }

    public void Configure(
        HearthActionHintPresenter newPresenter,
        CanvasGroup newCanvasGroup,
        HearthFirstPersonHudController newHumanHud)
    {
        presenter = newPresenter;
        canvasGroup = newCanvasGroup;
        humanHud = newHumanHud;
    }

    public void SetUiStateCoordinator(
        HearthUiStateCoordinator newUiStateCoordinator)
    {
        uiStateCoordinator = newUiStateCoordinator;
    }

    private void Awake()
    {
        ResolveRuntimeSources();

        if (completedThisRun ||
            (persistCompletion &&
             PlayerPrefs.GetInt(ResolveCompletionKey(), 0) != 0))
        {
            FinishImmediate();
            return;
        }

        ShowTutorial();
    }

    private void Update()
    {
        if (finished)
        {
            return;
        }

        if (fading)
        {
            UpdateFade();
            return;
        }

        bool isEffectiveGameplayTime = IsEffectiveGameplayTime();
        SetPresentationVisible(isEffectiveGameplayTime);
        if (!isEffectiveGameplayTime)
        {
            return;
        }

        elapsedGameplaySeconds += Time.unscaledDeltaTime;
        if (elapsedGameplaySeconds >= effectiveGameplaySeconds)
        {
            BeginFade();
        }
    }

    private void OnValidate()
    {
        effectiveGameplaySeconds = Mathf.Max(0.1f, effectiveGameplaySeconds);
        fadeOutSeconds = Mathf.Max(0f, fadeOutSeconds);
    }

    public void RestartForPreview()
    {
        completedThisRun = false;
        PlayerPrefs.DeleteKey(ResolveCompletionKey());
        elapsedGameplaySeconds = 0f;
        fadeElapsedSeconds = 0f;
        fading = false;
        finished = false;
        ResolveRuntimeSources();
        ShowTutorial();
    }

    public static void ResetCompletionForCurrentRun()
    {
        completedThisRun = false;
        PlayerPrefs.DeleteKey(DefaultCompletionKey);
    }

    private bool IsEffectiveGameplayTime()
    {
        if (uiStateCoordinator != null &&
            uiStateCoordinator.enabled &&
            uiStateCoordinator.HasResolvedState)
        {
            HearthUiVisibilityState state =
                uiStateCoordinator.ResolvedState;
            return state.Persistent &&
                state.Interaction &&
                !state.Dialogue &&
                !state.Terminal &&
                !state.Modal &&
                !state.Takeover &&
                !IsDynamicInteractionPromptVisible();
        }

        if (Time.timeScale <= 0f ||
            HearthTvTerminalController.AnyTerminalOpen ||
            HearthPlayerControlLock.AnyControlsLocked)
        {
            return false;
        }

        if (viewSwitchController != null &&
            (viewSwitchController.IsSwitching ||
             viewSwitchController.CurrentMode != ViewSwitchController.ViewMode.Human))
        {
            return false;
        }

        if (humanHud != null)
        {
            HearthFirstPersonHudPageId page = humanHud.CurrentPageId;
            if (page != HearthFirstPersonHudPageId.None &&
                page != HearthFirstPersonHudPageId.Slide01PersistentHud &&
                page != HearthFirstPersonHudPageId.Slide02TrustDelta)
            {
                return false;
            }
        }

        if (IsDynamicInteractionPromptVisible())
        {
            return false;
        }

        if (subtitlePlayers == null || subtitlePlayers.Length == 0)
        {
            subtitlePlayers =
                Object.FindObjectsOfType<MinLoopSubtitlePlayer>(true);
        }

        for (int i = 0; i < subtitlePlayers.Length; i++)
        {
            if (subtitlePlayers[i] != null && subtitlePlayers[i].IsPlaying)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsDynamicInteractionPromptVisible()
    {
        return humanInteraction != null &&
            humanInteraction.uiInteraction != null &&
            humanInteraction.uiInteraction.activeInHierarchy;
    }

    private void ResolveRuntimeSources()
    {
        if (humanHud == null)
        {
            humanHud = GetComponentInParent<HearthFirstPersonHudController>(true);
        }

        if (uiStateCoordinator == null)
        {
            uiStateCoordinator =
                GetComponentInParent<HearthUiStateCoordinator>(true);
        }

        if (viewSwitchController == null)
        {
            viewSwitchController = ViewSwitchController.FindPreferredController(
                gameObject.scene);
        }

        if (humanInteraction == null)
        {
            PlayerInteraction[] interactions =
                Object.FindObjectsOfType<PlayerInteraction>(true);
            for (int i = 0; i < interactions.Length; i++)
            {
                PlayerInteraction candidate = interactions[i];
                if (candidate != null &&
                    candidate.name != "Robot Controller" &&
                    candidate.gameObject.scene == gameObject.scene)
                {
                    humanInteraction = candidate;
                    break;
                }
            }
        }

        if (subtitlePlayers == null || subtitlePlayers.Length == 0)
        {
            subtitlePlayers =
                Object.FindObjectsOfType<MinLoopSubtitlePlayer>(true);
        }
    }

    private void ShowTutorial()
    {
        SetPresentationVisible(true);

        if (presenter != null)
        {
            presenter.Apply(
                new HearthActionHintState(
                    true,
                    HearthActionHintPriority.InitialTutorial,
                    "human_initial_gameplay",
                    string.Empty,
                    new HearthActionHintItem("WASD", "MOVE", true, true),
                    new HearthActionHintItem("MOUSE", "LOOK", true, true),
                    new HearthActionHintItem("E", "INTERACT"),
                    new HearthActionHintItem("TAB", "MENU", true, true)));
        }
    }

    private void SetPresentationVisible(bool visible)
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void BeginFade()
    {
        fading = true;
        fadeElapsedSeconds = 0f;
        MarkCompleted();

        if (fadeOutSeconds <= 0f)
        {
            FinishImmediate();
        }
    }

    private void UpdateFade()
    {
        fadeElapsedSeconds += Time.unscaledDeltaTime;
        float progress = fadeOutSeconds > 0f
            ? Mathf.Clamp01(fadeElapsedSeconds / fadeOutSeconds)
            : 1f;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f - progress;
        }

        if (progress >= 1f)
        {
            FinishImmediate();
        }
    }

    private void MarkCompleted()
    {
        completedThisRun = true;
        if (!persistCompletion)
        {
            return;
        }

        PlayerPrefs.SetInt(ResolveCompletionKey(), 1);
        PlayerPrefs.Save();
    }

    private void FinishImmediate()
    {
        finished = true;
        fading = false;

        if (presenter != null)
        {
            presenter.Apply(HearthActionHintState.Hidden);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private string ResolveCompletionKey()
    {
        return string.IsNullOrWhiteSpace(completionKey)
            ? DefaultCompletionKey
            : completionKey;
    }
}
