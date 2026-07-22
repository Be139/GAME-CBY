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

    public HearthCompanionHudSceneData CurrentScene { get { return currentScene; } }
    public string CurrentSceneId { get { return currentScene != null ? currentScene.SceneId : string.Empty; } }
    public HearthCompanionHudSceneEvent SceneShown { get { return sceneShown; } }
    public HearthCompanionHudSceneEvent HoldPromptConfirmed { get { return holdPromptConfirmed; } }
    public UnityEvent ReplayCompleted { get { return replayCompleted; } }

    private void Awake()
    {
        ResolveReferences();
        BuildSceneMap();
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
            statusPanelView.Clear();
        }

        if (decisionPanelView != null)
        {
            decisionPanelView.Apply(scene);
        }

        if (dataStreamView != null)
        {
            dataStreamView.Apply(scene);
        }

        if (triggerCardView != null)
        {
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

        if (centerMessageText != null)
        {
            centerMessageText.text = scene.CenterMessage;
            centerMessageText.gameObject.SetActive(!string.IsNullOrEmpty(scene.CenterMessage));
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

        if (autoFindViewSwitchController &&
            (viewSwitchController == null ||
             !viewSwitchController.enabled ||
             !viewSwitchController.gameObject.activeInHierarchy))
        {
            viewSwitchController = ViewSwitchController.FindPreferredController();
        }

        if (holdPrompt != null)
        {
            holdPrompt.SetController(this);
        }
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
}
