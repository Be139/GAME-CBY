using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class HearthCompanionHudController : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private HearthCompanionHudSceneData[] scenes;
    [SerializeField] private string startingSceneId = "17F01_01";
    [SerializeField] private bool showStartingSceneOnStart = true;

    [Header("Visibility")]
    [SerializeField] private CanvasGroup rootCanvasGroup;
    [SerializeField] private bool visibleOnlyInCompanionView = true;
    [SerializeField] private ViewSwitchController viewSwitchController;
    [SerializeField] private bool autoFindViewSwitchController = true;

    [Header("Views")]
    [SerializeField] private HearthCompanionStatusPanelView statusPanelView;
    [SerializeField] private HearthCompanionDecisionPanelView decisionPanelView;
    [SerializeField] private HearthCompanionDataStreamView dataStreamView;
    [SerializeField] private HearthCompanionTriggerCardView triggerCardView;
    [SerializeField] private HearthCompanionHoldPrompt holdPrompt;
    [SerializeField] private HearthCompanionProjectionPanelView projectionPanelView;
    [SerializeField] private HearthCompanionDirectionGuideView directionGuideView;
    [SerializeField] private HearthCompanionSpecialEffectsView specialEffectsView;
    [SerializeField] private TMP_Text identityText;
    [SerializeField] private TMP_Text currentTaskText;
    [SerializeField] private TMP_Text recText;
    [SerializeField] private TMP_Text modeLabelText;
    [SerializeField] private TMP_Text centerMessageText;

    [Header("Audio Hooks")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sceneChangedClip;
    [SerializeField] private AudioClip holdCompletedClip;
    [SerializeField] private AudioClip specialEffectClip;

    [Header("Interaction Behavior")]
    [SerializeField] private bool autoAdvanceOnHoldPrompt = true;

    [Header("Events")]
    [SerializeField] private HearthCompanionHudSceneEvent sceneShown = new HearthCompanionHudSceneEvent();
    [SerializeField] private HearthCompanionHudSceneEvent holdPromptConfirmed = new HearthCompanionHudSceneEvent();
    [SerializeField] private UnityEvent replayCompleted = new UnityEvent();

    private readonly Dictionary<string, HearthCompanionHudSceneData> sceneMap =
        new Dictionary<string, HearthCompanionHudSceneData>();
    private readonly Dictionary<int, HearthCompanionHudSceneData> slideMap =
        new Dictionary<int, HearthCompanionHudSceneData>();

    private HearthCompanionHudSceneData currentScene;
    private bool explicitVisibility = true;
    private bool missingHoldPromptWarningLogged;
    private HearthCompanionTriggerCardView subscribedTriggerCardView;
    private Coroutine decisionVisibilityRoutine;
    private Coroutine centerMessageRoutine;

    public HearthCompanionHudSceneData CurrentScene { get { return currentScene; } }
    public string CurrentSceneId { get { return currentScene != null ? currentScene.SceneId : string.Empty; } }
    public HearthCompanionHudSceneEvent SceneShown { get { return sceneShown; } }
    public HearthCompanionHudSceneEvent HoldPromptConfirmed { get { return holdPromptConfirmed; } }
    public UnityEvent ReplayCompleted { get { return replayCompleted; } }

    private void Awake()
    {
        ResolveReferences();
        BindTriggerCardVisibility();
        BuildSceneMap();
    }

    private void OnDestroy()
    {
        StopTransientPresentationRoutines();
        UnbindTriggerCardVisibility();
    }

    private void Start()
    {
        if (showStartingSceneOnStart && !string.IsNullOrEmpty(startingSceneId))
        {
            ShowScene(startingSceneId);
        }

        RefreshVisibilityFromViewMode();
    }

    private void LateUpdate()
    {
        RefreshVisibilityFromViewMode();
    }

    public void Configure(
        HearthCompanionHudSceneData[] newScenes,
        CanvasGroup newRootCanvasGroup,
        HearthCompanionStatusPanelView newStatusPanelView,
        HearthCompanionDecisionPanelView newDecisionPanelView,
        HearthCompanionDataStreamView newDataStreamView,
        HearthCompanionTriggerCardView newTriggerCardView,
        HearthCompanionHoldPrompt newHoldPrompt,
        HearthCompanionProjectionPanelView newProjectionPanelView,
        HearthCompanionDirectionGuideView newDirectionGuideView,
        HearthCompanionSpecialEffectsView newSpecialEffectsView,
        TMP_Text newModeLabelText,
        TMP_Text newCenterMessageText,
        AudioSource newAudioSource)
    {
        UnbindTriggerCardVisibility();

        scenes = newScenes;
        rootCanvasGroup = newRootCanvasGroup;
        statusPanelView = newStatusPanelView;
        decisionPanelView = newDecisionPanelView;
        dataStreamView = newDataStreamView;
        triggerCardView = newTriggerCardView;
        holdPrompt = newHoldPrompt;
        projectionPanelView = newProjectionPanelView;
        directionGuideView = newDirectionGuideView;
        specialEffectsView = newSpecialEffectsView;
        modeLabelText = newModeLabelText;
        centerMessageText = newCenterMessageText;
        audioSource = newAudioSource;

        if (holdPrompt != null)
        {
            holdPrompt.SetController(this);
        }

        BindTriggerCardVisibility();
        BuildSceneMap();
    }

    public void SetViewSwitchController(ViewSwitchController controller)
    {
        viewSwitchController = controller;
        RefreshVisibilityFromViewMode();
    }

    public void ShowScene(string sceneId)
    {
        BuildSceneMap();

        if (string.IsNullOrEmpty(sceneId))
        {
            return;
        }

        HearthCompanionHudSceneData scene;
        if (!sceneMap.TryGetValue(sceneId, out scene))
        {
            Debug.LogWarning("[HearthCompanionHudController] Scene not found: " + sceneId, this);
            return;
        }

        ApplyScene(scene);
    }

    public void ShowScene(int slideNumber)
    {
        BuildSceneMap();

        HearthCompanionHudSceneData scene;
        if (!slideMap.TryGetValue(slideNumber, out scene))
        {
            Debug.LogWarning("[HearthCompanionHudController] Slide scene not found: " + slideNumber, this);
            return;
        }

        ApplyScene(scene);
    }

    public void AdvanceScene()
    {
        if (scenes == null || scenes.Length == 0)
        {
            return;
        }

        int currentIndex = 0;
        for (int i = 0; i < scenes.Length; i++)
        {
            if (scenes[i] == currentScene)
            {
                currentIndex = i;
                break;
            }
        }

        int nextIndex = (currentIndex + 1) % scenes.Length;
        ApplyScene(scenes[nextIndex]);
    }

    public void ShowPreviousScene()
    {
        if (scenes == null || scenes.Length == 0)
        {
            return;
        }

        int currentIndex = 0;
        for (int i = 0; i < scenes.Length; i++)
        {
            if (scenes[i] == currentScene)
            {
                currentIndex = i;
                break;
            }
        }

        int nextIndex = currentIndex - 1;
        if (nextIndex < 0)
        {
            nextIndex = scenes.Length - 1;
        }

        ApplyScene(scenes[nextIndex]);
    }

    public void ConfirmCurrentPrompt()
    {
        if (currentScene == null)
        {
            return;
        }

        PlayOneShot(holdCompletedClip);

        if (holdPromptConfirmed != null)
        {
            holdPromptConfirmed.Invoke(currentScene.SceneId);
        }

        if (currentScene.SpecialEffect == HearthCompanionSpecialEffect.ShutdownGlitch)
        {
            PlayShutdownGlitch();
            return;
        }

        if (currentScene.SpecialEffect == HearthCompanionSpecialEffect.DeepSleep)
        {
            PlayDeepSleep();
            return;
        }

        if (!autoAdvanceOnHoldPrompt)
        {
            return;
        }

        AdvanceScene();
    }

    public void SetAutoAdvanceOnHoldPrompt(bool value)
    {
        autoAdvanceOnHoldPrompt = value;
    }

    public void SetHoldPromptVisible(bool visible)
    {
        ResolveReferences();
        if (holdPrompt == null)
        {
            if (visible && !missingHoldPromptWarningLogged)
            {
                Debug.LogWarning("[HearthCompanionHudController] HoldPrompt is not bound. Reapply the companion HUD binding tool.", this);
                missingHoldPromptWarningLogged = true;
            }

            return;
        }

        missingHoldPromptWarningLogged = false;
        if (visible)
        {
            if (currentScene == null || !currentScene.ShowHoldPrompt)
            {
                holdPrompt.SetVisible(false);
                return;
            }

            if (!holdPrompt.IsVisible)
            {
                holdPrompt.Apply(currentScene);
            }
            else
            {
                holdPrompt.SetVisible(true);
            }

            return;
        }

        if (holdPrompt.IsVisible)
        {
            holdPrompt.SetVisible(false);
            holdPrompt.ResetHold();
        }
    }

    public void ShowCurrentHoldPrompt()
    {
        ResolveReferences();
        if (holdPrompt == null || currentScene == null || !currentScene.ShowHoldPrompt)
        {
            SetHoldPromptVisible(false);
            return;
        }

        holdPrompt.Apply(currentScene);
    }

    public void SetDirectionGuideVisible(bool visible)
    {
        if (directionGuideView != null)
        {
            directionGuideView.SetVisible(visible);
        }
    }

    public void SetDirectionMarkerVisible(bool visible)
    {
        if (directionGuideView != null)
        {
            directionGuideView.SetMarkerVisible(visible);
        }
    }

    public void ResetHoldPrompt()
    {
        if (holdPrompt != null)
        {
            holdPrompt.ResetHold();
        }
    }

    public void NotifyReplayCompleted()
    {
        if (replayCompleted != null)
        {
            replayCompleted.Invoke();
        }
    }

    public void ShowBlackAudio()
    {
        if (currentScene != null && specialEffectsView != null)
        {
            specialEffectsView.ShowBlackAudio(currentScene.SpecialTitle, currentScene.SpecialBody, currentScene.SpecialStatusLabel);
            PlayOneShot(specialEffectClip);
        }
    }

    public void PlayShutdownGlitch()
    {
        if (currentScene != null && specialEffectsView != null)
        {
            specialEffectsView.PlayShutdownGlitch(currentScene.SpecialTitle, currentScene.SpecialBody, currentScene.SpecialStatusLabel, currentScene.SpecialDuration);
            PlayOneShot(specialEffectClip);
        }
    }

    public void PlayDeepSleep()
    {
        if (currentScene != null && specialEffectsView != null)
        {
            specialEffectsView.PlayDeepSleep(currentScene.SpecialTitle, currentScene.SpecialBody, currentScene.SpecialStatusLabel, currentScene.SpecialDuration);
            PlayOneShot(specialEffectClip);
        }
    }

    public void SetDirectionTarget(Transform target, Camera viewCamera)
    {
        if (directionGuideView != null)
        {
            directionGuideView.SetTarget(target, viewCamera);
        }
    }

    public void SetVisible(bool visible)
    {
        explicitVisibility = visible;
        RefreshVisibilityFromViewMode();
    }

    private void ApplyScene(HearthCompanionHudSceneData scene)
    {
        if (scene == null)
        {
            return;
        }

        currentScene = scene;
        explicitVisibility = true;

        if (statusPanelView != null)
        {
            statusPanelView.Apply(scene);
            SetStatusPanelVisible(HasStatusContent(scene));
        }

        if (decisionPanelView != null)
        {
            decisionPanelView.Apply(scene);
            StartDecisionVisibilityTimer(scene);
        }

        if (dataStreamView != null)
        {
            dataStreamView.Clear();
            dataStreamView.gameObject.SetActive(false);
        }

        if (triggerCardView != null)
        {
            BindTriggerCardVisibility();
            triggerCardView.Apply(scene);
        }

        if (holdPrompt != null)
        {
            holdPrompt.Apply(scene);
        }

        if (projectionPanelView != null)
        {
            projectionPanelView.Apply(scene);
        }

        if (directionGuideView != null)
        {
            directionGuideView.Apply(scene);
        }

        if (specialEffectsView != null)
        {
            if (scene.AutoPlaySpecialEffect)
            {
                specialEffectsView.Apply(scene);
            }
            else
            {
                specialEffectsView.HideImmediate();
            }
        }

        if (modeLabelText != null)
        {
            modeLabelText.text = scene.ModeLabel;
            modeLabelText.color = scene.AccentColor;
        }

        if (identityText != null)
        {
            identityText.text =
                "COMPANION UNIT · ACTIVE\nUNIT " +
                NormalizeResidentLabel(scene.ResidentId);
            identityText.color = scene.AccentColor;
        }

        if (currentTaskText != null)
        {
            string task = string.IsNullOrWhiteSpace(scene.CurrentTask)
                ? "REVIEW RECORDED HOUSEHOLD EVENT"
                : scene.CurrentTask.Trim();
            currentTaskText.text = "CURRENT TASK\n" + task;
            currentTaskText.color = scene.AccentColor;
        }

        if (recText != null)
        {
            recText.text = "●  REC";
            recText.color = new Color(0.92f, 0.22f, 0.18f, 1f);
            recText.gameObject.SetActive(true);
        }

        if (centerMessageText != null)
        {
            centerMessageText.text = scene.CenterMessage;
            centerMessageText.gameObject.SetActive(!string.IsNullOrEmpty(scene.CenterMessage));
            StartCenterMessageTimer(scene);
        }

        RefreshVisibilityFromViewMode();
        PlayOneShot(sceneChangedClip);

        if (sceneShown != null)
        {
            sceneShown.Invoke(scene.SceneId);
        }
    }

    private void ResolveReferences()
    {
        if (rootCanvasGroup == null)
        {
            rootCanvasGroup = GetComponent<CanvasGroup>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (holdPrompt == null)
        {
            holdPrompt = GetComponentInChildren<HearthCompanionHoldPrompt>(true);
        }

        if (statusPanelView == null)
        {
            statusPanelView = GetComponentInChildren<HearthCompanionStatusPanelView>(true);
        }

        if (identityText == null)
        {
            identityText = FindTextByName("V2_Identity");
        }

        if (currentTaskText == null)
        {
            currentTaskText = FindTextByName("V2_CurrentTask");
        }

        if (recText == null)
        {
            recText = FindTextByName("V2_REC");
        }

        if (dataStreamView != null)
        {
            dataStreamView.gameObject.SetActive(false);
        }

        if (triggerCardView == null)
        {
            triggerCardView = GetComponentInChildren<HearthCompanionTriggerCardView>(true);
        }

        if (autoFindViewSwitchController)
        {
            ViewSwitchController preferredViewSwitch =
                ViewSwitchController.FindPreferredController(gameObject.scene);
            if (preferredViewSwitch != null &&
                viewSwitchController != preferredViewSwitch)
            {
                viewSwitchController = preferredViewSwitch;
            }
        }

        if (holdPrompt != null)
        {
            holdPrompt.SetController(this);
        }
    }

    private void BindTriggerCardVisibility()
    {
        if (subscribedTriggerCardView == triggerCardView)
        {
            return;
        }

        UnbindTriggerCardVisibility();
        subscribedTriggerCardView = triggerCardView;
        if (subscribedTriggerCardView != null)
        {
            subscribedTriggerCardView.VisibilityChanged += HandleTriggerCardVisibilityChanged;
            HandleTriggerCardVisibilityChanged(subscribedTriggerCardView.IsVisible);
        }
    }

    private void UnbindTriggerCardVisibility()
    {
        if (subscribedTriggerCardView != null)
        {
            subscribedTriggerCardView.VisibilityChanged -= HandleTriggerCardVisibilityChanged;
            subscribedTriggerCardView = null;
        }
    }

    private void HandleTriggerCardVisibilityChanged(bool triggerVisible)
    {
        SetStatusPanelVisible(!triggerVisible && HasStatusContent(currentScene));
    }

    private void SetStatusPanelVisible(bool visible)
    {
        if (statusPanelView != null &&
            statusPanelView.gameObject.activeSelf != visible)
        {
            statusPanelView.gameObject.SetActive(visible);
        }
    }

    private static bool HasStatusContent(HearthCompanionHudSceneData scene)
    {
        if (scene == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(scene.StatusTitle) ||
            !string.IsNullOrWhiteSpace(scene.StatusFooter))
        {
            return true;
        }

        HearthCompanionMetricLine[] lines = scene.StatusLines;
        if (lines == null)
        {
            return false;
        }

        for (int i = 0; i < lines.Length; i++)
        {
            HearthCompanionMetricLine line = lines[i];
            if (line != null &&
                (!string.IsNullOrWhiteSpace(line.label) ||
                 !string.IsNullOrWhiteSpace(line.value)))
            {
                return true;
            }
        }

        return false;
    }

    private void BuildSceneMap()
    {
        sceneMap.Clear();
        slideMap.Clear();

        if (scenes == null)
        {
            return;
        }

        for (int i = 0; i < scenes.Length; i++)
        {
            HearthCompanionHudSceneData scene = scenes[i];
            if (scene == null)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(scene.SceneId) && !sceneMap.ContainsKey(scene.SceneId))
            {
                sceneMap.Add(scene.SceneId, scene);
            }

            if (!slideMap.ContainsKey(scene.SlideNumber))
            {
                slideMap.Add(scene.SlideNumber, scene);
            }
        }
    }

    private void RefreshVisibilityFromViewMode()
    {
        bool allowedByMode = true;
        if (visibleOnlyInCompanionView)
        {
            ResolveReferences();
            if (viewSwitchController != null)
            {
                allowedByMode = viewSwitchController.CurrentMode == ViewSwitchController.ViewMode.Companion;
            }
        }

        bool visible = explicitVisibility && allowedByMode;

        if (rootCanvasGroup != null)
        {
            rootCanvasGroup.alpha = visible ? 1f : 0f;
            rootCanvasGroup.interactable = visible;
            rootCanvasGroup.blocksRaycasts = visible;
        }
        else
        {
            gameObject.SetActive(visible);
        }
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void StartDecisionVisibilityTimer(
        HearthCompanionHudSceneData scene)
    {
        if (decisionVisibilityRoutine != null)
        {
            StopCoroutine(decisionVisibilityRoutine);
            decisionVisibilityRoutine = null;
        }

        if (decisionPanelView == null ||
            scene == null ||
            (string.IsNullOrWhiteSpace(scene.DecisionTitle) &&
             string.IsNullOrWhiteSpace(scene.DecisionBody)))
        {
            if (decisionPanelView != null)
            {
                decisionPanelView.HideImmediate();
            }
            return;
        }

        float duration = scene.DecisionDisplaySeconds > 0f
            ? scene.DecisionDisplaySeconds
            : 4f;
        decisionVisibilityRoutine =
            StartCoroutine(HideDecisionAfter(duration));
    }

    private System.Collections.IEnumerator HideDecisionAfter(float seconds)
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, seconds));
        if (decisionPanelView != null)
        {
            decisionPanelView.HideImmediate();
        }
        decisionVisibilityRoutine = null;
    }

    private void StartCenterMessageTimer(HearthCompanionHudSceneData scene)
    {
        if (centerMessageRoutine != null)
        {
            StopCoroutine(centerMessageRoutine);
            centerMessageRoutine = null;
        }

        if (centerMessageText == null ||
            scene == null ||
            string.IsNullOrWhiteSpace(scene.CenterMessage))
        {
            return;
        }

        float duration = scene.CenterMessageSeconds > 0f
            ? scene.CenterMessageSeconds
            : 2.5f;
        centerMessageRoutine =
            StartCoroutine(HideCenterMessageAfter(duration));
    }

    private System.Collections.IEnumerator HideCenterMessageAfter(
        float seconds)
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, seconds));
        if (centerMessageText != null)
        {
            centerMessageText.gameObject.SetActive(false);
        }
        centerMessageRoutine = null;
    }

    private void StopTransientPresentationRoutines()
    {
        if (decisionVisibilityRoutine != null)
        {
            StopCoroutine(decisionVisibilityRoutine);
            decisionVisibilityRoutine = null;
        }

        if (centerMessageRoutine != null)
        {
            StopCoroutine(centerMessageRoutine);
            centerMessageRoutine = null;
        }
    }

    private TMP_Text FindTextByName(string objectName)
    {
        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i] != null && texts[i].name == objectName)
            {
                return texts[i];
            }
        }

        return null;
    }

    private static string NormalizeResidentLabel(string residentId)
    {
        string normalized = (residentId ?? string.Empty)
            .Trim()
            .ToUpperInvariant()
            .Replace("-", string.Empty)
            .Replace("_", string.Empty)
            .Replace(" ", string.Empty);
        if (normalized.Contains("17F01"))
        {
            return "17F-01";
        }

        if (normalized.Contains("17F02"))
        {
            return "17F-02";
        }

        if (normalized.Contains("17F03"))
        {
            return "17F-03";
        }

        if (normalized.Contains("17F04"))
        {
            return "17F-04";
        }

        return string.IsNullOrEmpty(normalized) ? "UNASSIGNED" : normalized;
    }
}
