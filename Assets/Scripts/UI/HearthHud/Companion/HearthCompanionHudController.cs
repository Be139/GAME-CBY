using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class HearthCompanionHudController : MonoBehaviour
{
    [Header("Authored V2 Bindings")]
    [SerializeField] private HearthCompanionHudBindings authoredBindings;

    [Header("Scenes")]
    [SerializeField] private HearthCompanionHudSceneData[] scenes;
    [SerializeField] private string startingSceneId = "17F01_01";
    [SerializeField] private bool showStartingSceneOnStart = true;

    [Header("Visibility")]
    [SerializeField] private CanvasGroup rootCanvasGroup;
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
    [SerializeField] private TMP_Text identityHeadingText;
    [SerializeField] private TMP_Text identityValueText;
    [SerializeField] private TMP_Text currentTaskHeadingText;
    [SerializeField] private TMP_Text currentTaskBodyText;
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
    private bool coordinatorPresentationVisible;
    private bool missingHoldPromptWarningLogged;
    private HearthCompanionTriggerCardView subscribedTriggerCardView;
    private Coroutine decisionVisibilityRoutine;
    private Coroutine centerMessageRoutine;
    private PlayerInteraction companionInteraction;
    private bool shortPressInteractionSuspendedByHold;
    private bool shortPressInteractionEnabledBeforeHold;
    private bool transientDialogueExclusive;
    private bool decisionWasVisibleBeforeTransientDialogue;

    public HearthCompanionHudSceneData CurrentScene { get { return currentScene; } }
    public string CurrentSceneId { get { return currentScene != null ? currentScene.SceneId : string.Empty; } }
    public HearthCompanionHudSceneEvent SceneShown { get { return sceneShown; } }
    public HearthCompanionHudSceneEvent HoldPromptConfirmed { get { return holdPromptConfirmed; } }
    public UnityEvent ReplayCompleted { get { return replayCompleted; } }
    public bool IsPresented
    {
        get
        {
            return explicitVisibility && coordinatorPresentationVisible &&
                (rootCanvasGroup == null || rootCanvasGroup.alpha > 0.001f);
        }
    }

    private void Awake()
    {
        ResolveReferences();
        BindTriggerCardVisibility();
        BuildSceneMap();
        ApplyRootVisibility();
    }

    private void OnDestroy()
    {
        RestoreShortPressInteractionAfterHold();
        StopTransientPresentationRoutines();
        UnbindTriggerCardVisibility();
    }

    private void OnDisable()
    {
        RestoreShortPressInteractionAfterHold();
    }

    private void Start()
    {
        if (showStartingSceneOnStart && !string.IsNullOrEmpty(startingSceneId))
        {
            ShowScene(startingSceneId);
        }

        ApplyRootVisibility();
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
    }

    public void SetAuthoredBindings(HearthCompanionHudBindings bindings)
    {
        authoredBindings = bindings;
        ResolveReferences();
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

            if (!visible)
            {
                RestoreShortPressInteractionAfterHold();
            }

            return;
        }

        missingHoldPromptWarningLogged = false;
        if (visible)
        {
            if (currentScene == null || !currentScene.ShowHoldPrompt)
            {
                holdPrompt.SetVisible(false);
                RestoreShortPressInteractionAfterHold();
                return;
            }

            SuspendShortPressInteractionForHold();

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

        RestoreShortPressInteractionAfterHold();
    }

    public void ShowCurrentHoldPrompt()
    {
        ResolveReferences();
        if (holdPrompt == null || currentScene == null || !currentScene.ShowHoldPrompt)
        {
            SetHoldPromptVisible(false);
            return;
        }

        SetHoldPromptVisible(true);
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

    public void SetCurrentTask(string task)
    {
        if (currentTaskText == null && currentTaskBodyText == null)
        {
            ResolveReferences();
        }

        string normalized = HearthCurrentTaskRouter.NormalizeTaskText(task);
        if (currentTaskHeadingText != null)
        {
            currentTaskHeadingText.text = "CURRENT TASK";
        }
        if (currentTaskBodyText != null)
        {
            currentTaskBodyText.text = normalized;
        }
        else if (currentTaskText != null)
        {
            currentTaskText.text = string.IsNullOrWhiteSpace(normalized)
                ? "CURRENT TASK"
                : "CURRENT TASK\n" + normalized;
        }
    }

    /// <summary>
    /// Temporarily gives a formal Field Unit / Synth Voice subtitle exclusive
    /// ownership of the right side of the companion HUD. The authored status
    /// data is kept and the DecisionPanel is restored only after the dialogue
    /// line has finished.
    /// </summary>
    public void SetTransientDialogueExclusive(bool exclusive)
    {
        ResolveReferences();
        if (transientDialogueExclusive == exclusive)
        {
            return;
        }

        transientDialogueExclusive = exclusive;
        if (exclusive)
        {
            decisionWasVisibleBeforeTransientDialogue =
                decisionPanelView != null && decisionPanelView.IsVisible;
            if (decisionVisibilityRoutine != null)
            {
                StopCoroutine(decisionVisibilityRoutine);
                decisionVisibilityRoutine = null;
            }
            if (decisionPanelView != null)
            {
                decisionPanelView.HideImmediate();
            }
            return;
        }

        if (decisionWasVisibleBeforeTransientDialogue &&
            decisionPanelView != null && currentScene != null)
        {
            decisionPanelView.Apply(currentScene);
            StartDecisionVisibilityTimer(currentScene);
        }
        decisionWasVisibleBeforeTransientDialogue = false;
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
        if (!visible)
        {
            SetHoldPromptVisible(false);
        }
        ApplyRootVisibility();
    }

    /// <summary>
    /// Called only by HearthUiStateCoordinator. The Companion controller owns
    /// the data inside its HUD, while the coordinator owns whether the HUD is
    /// presented at all.
    /// </summary>
    public void SetCoordinatorPresentationVisible(bool visible)
    {
        coordinatorPresentationVisible = visible;
        ApplyRootVisibility();
    }

    public bool CoordinatorPresentationVisible
    {
        get { return coordinatorPresentationVisible; }
    }

    public void ResetTransientPresentation()
    {
        StopTransientPresentationRoutines();

        if (decisionPanelView != null)
        {
            decisionPanelView.HideImmediate();
        }

        if (triggerCardView != null)
        {
            // Keep the view object active and hide it through its CanvasGroup.
            // ApplyScene may immediately start a timed cue on this component;
            // a disabled GameObject would make that coroutine fail to start.
            triggerCardView.gameObject.SetActive(true);
            triggerCardView.HideImmediate();
        }

        SetHoldPromptVisible(false);

        if (centerMessageText != null)
        {
            centerMessageText.gameObject.SetActive(false);
        }

        if (specialEffectsView != null)
        {
            specialEffectsView.HideImmediate();
        }
    }

    private void ApplyScene(HearthCompanionHudSceneData scene)
    {
        if (scene == null)
        {
            return;
        }

        currentScene = scene;

        if (statusPanelView != null)
        {
            bool hideStandbyObservation = IsStandbyObservation(scene.StatusTitle);
            if (!hideStandbyObservation)
            {
                statusPanelView.Apply(scene);
            }
            SetStatusPanelVisible(
                !hideStandbyObservation && HasStatusContent(scene));
        }

        if (decisionPanelView != null)
        {
            bool hideStandbyDecision =
                IsStandbyObservation(scene.DecisionTitle) ||
                IsStandbyObservation(scene.DecisionKicker);
            if (hideStandbyDecision)
            {
                decisionPanelView.HideImmediate();
            }
            else
            {
                decisionPanelView.Apply(scene);
                StartDecisionVisibilityTimer(scene);
            }
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
            SetHoldPromptVisible(scene.ShowHoldPrompt);
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

        string residentLabel = "UNIT " + NormalizeResidentLabel(scene.ResidentId);
        if (identityHeadingText != null)
        {
            identityHeadingText.text = "COMPANION UNIT · ACTIVE";
            identityHeadingText.color = scene.AccentColor;
        }
        if (identityValueText != null)
        {
            identityValueText.text = residentLabel;
            identityValueText.color = scene.AccentColor;
        }
        else if (identityText != null)
        {
            identityText.text =
                "COMPANION UNIT · ACTIVE\n" + residentLabel;
            identityText.color = scene.AccentColor;
        }

        string resolvedTask = HearthCurrentTaskRouter.ResolveCompanionSceneTask(
            scene.SceneId,
            scene.CurrentTask);
        if (currentTaskHeadingText != null)
        {
            currentTaskHeadingText.text = "CURRENT TASK";
            currentTaskHeadingText.color = scene.AccentColor;
        }
        if (currentTaskBodyText != null)
        {
            currentTaskBodyText.text = resolvedTask;
            currentTaskBodyText.color = scene.AccentColor;
        }
        else if (currentTaskText != null)
        {
            currentTaskText.text = "CURRENT TASK\n" + resolvedTask;
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

        ApplyRootVisibility();
        PlayOneShot(sceneChangedClip);

        if (sceneShown != null)
        {
            sceneShown.Invoke(scene.SceneId);
        }
    }

    private static bool IsStandbyObservation(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string normalized = value.Trim().ToUpperInvariant();
        return normalized.Contains("STANDBY OBSERVATION") ||
               normalized.Contains("STANDBY - OBSERVE");
    }

    private void ResolveReferences()
    {
        if (authoredBindings == null)
        {
            authoredBindings = GetComponent<HearthCompanionHudBindings>();
        }

        if (authoredBindings != null)
        {
            identityHeadingText = authoredBindings.IdentityHeadingText;
            identityValueText = authoredBindings.IdentityValueText;
            currentTaskHeadingText = authoredBindings.CurrentTaskHeadingText;
            currentTaskBodyText = authoredBindings.CurrentTaskBodyText;
            if (authoredBindings.HoldPrompt != null)
            {
                holdPrompt = authoredBindings.HoldPrompt;
            }
        }

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

        if (identityHeadingText == null)
        {
            identityHeadingText = FindTextByName("V2_IdentityHeading");
        }

        if (identityValueText == null)
        {
            identityValueText = FindTextByName("V2_IdentityValue");
        }

        if (currentTaskText == null)
        {
            currentTaskText = FindTextByName("V2_CurrentTask");
        }

        if (currentTaskHeadingText == null)
        {
            currentTaskHeadingText = FindTextByName("V2_TaskHeading");
        }

        if (currentTaskBodyText == null)
        {
            currentTaskBodyText = FindTextByName("V2_TaskBody");
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

        if (companionInteraction == null)
        {
            PlayerInteraction[] interactions =
                FindObjectsOfType<PlayerInteraction>(true);
            for (int i = 0; i < interactions.Length; i++)
            {
                if (interactions[i] != null &&
                    interactions[i].gameObject.scene == gameObject.scene &&
                    interactions[i].name == "Robot Controller")
                {
                    companionInteraction = interactions[i];
                    break;
                }
            }
        }
    }

    private void SuspendShortPressInteractionForHold()
    {
        ResolveReferences();
        if (shortPressInteractionSuspendedByHold || companionInteraction == null)
        {
            return;
        }

        shortPressInteractionEnabledBeforeHold =
            companionInteraction.InteractionEnabled;
        shortPressInteractionSuspendedByHold = true;
        companionInteraction.SetInteractionEnabled(false);
    }

    private void RestoreShortPressInteractionAfterHold()
    {
        if (!shortPressInteractionSuspendedByHold)
        {
            return;
        }

        shortPressInteractionSuspendedByHold = false;
        if (companionInteraction != null)
        {
            companionInteraction.SetInteractionEnabled(
                shortPressInteractionEnabledBeforeHold);
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

    private void ApplyRootVisibility()
    {
        bool visible = explicitVisibility && coordinatorPresentationVisible;

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

        if (transientDialogueExclusive ||
            decisionPanelView == null ||
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
