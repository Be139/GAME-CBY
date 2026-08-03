using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Formal dialogue and authored time-card presenter.
/// Tutorial and action-hint UI must use their own presenters so they cannot stop
/// or replace an active dialogue sequence.
/// </summary>
public class MinLoopSubtitlePlayer : MonoBehaviour
{
    private static readonly HashSet<MinLoopSubtitlePlayer> ActiveDialoguePlayers =
        new HashSet<MinLoopSubtitlePlayer>();

    [Header("Shared Presentation")]
    [SerializeField] private HearthSubtitleStyleProfile styleProfile;
    [SerializeField] private HearthUiThemeProfile uiThemeProfile;
    [SerializeField] private HearthUiLayoutProfile uiLayoutProfile;
    [SerializeField] private HearthSubtitlePresentationMode presentationMode = HearthSubtitlePresentationMode.StandardDialogue;
    [SerializeField] private HearthSubtitleContext defaultContext = HearthSubtitleContext.Human;
    [SerializeField] private bool inferContextFromSpeaker = true;

    [Header("Explicit Dialogue VisualRoot")]
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private RectTransform layoutRoot;
    [SerializeField] private Image backdropImage;
    [SerializeField] private Image accentRuleImage;
    [SerializeField] private Image speakerTabImage;
    [SerializeField] private Image formalFrameImage;
    [SerializeField] private Image auxiliaryFrameImage;
    [SerializeField] private Image leftSpeakerTabImage;
    [SerializeField] private Image rightSpeakerTabImage;

    [Header("UI (legacy-compatible bindings)")]
    [SerializeField] private GameObject subtitlePanel;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text advanceHintText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text persistentSceneHeaderText;
    [SerializeField] private CanvasGroup persistentSceneHeaderGroup;
    [SerializeField] private bool createFallbackUI = true;

    [Header("Canvas Sorting")]
    [SerializeField] private bool forceSubtitleCanvasSorting = true;
    [SerializeField] private int subtitleSortingOrder = 7600;

    [Header("Clean Centered Style")]
    [SerializeField] private bool useCleanCenteredStyle = true;
    [SerializeField, Range(0.35f, 0.95f)] private float subtitleWidthFraction = 0.66f;
    [SerializeField, Range(0.03f, 0.16f)] private float speakerHeightFraction = 0.06f;
    [SerializeField, Range(0.08f, 0.8f)] private float bodyCenterY = 0.22f;
    [SerializeField, Range(0.06f, 0.28f)] private float bodyHeightFraction = 0.12f;
    [SerializeField] private Color cleanTextColor = Color.white;
    [SerializeField] private float cleanSpeakerFontSize = 28f;
    [SerializeField] private float cleanBodyFontSize = 26f;

    [Header("1920x1080 Dialogue Layout (Inspector Adjustable)")]
    [SerializeField] private Rect formalFrameRect =
        new Rect(480f, 670f, 960f, 256f);
    [SerializeField] private Rect formalBackdropRect =
        new Rect(486f, 676f, 948f, 244f);
    [SerializeField] private Rect formalLeftSpeakerRect =
        new Rect(480f, 622f, 340f, 48f);
    [SerializeField] private Rect formalRightSpeakerRect =
        new Rect(1100f, 622f, 340f, 48f);
    [SerializeField] private Rect formalBodyRect =
        new Rect(520f, 714f, 880f, 150f);
    [SerializeField] private Rect formalAdvanceHintRect =
        new Rect(1176f, 884f, 224f, 24f);
    [SerializeField] private Rect auxiliaryFrameRect =
        new Rect(1216f, 214f, 640f, 400f);
    [SerializeField] private Rect auxiliaryBackdropRect =
        new Rect(1222f, 220f, 628f, 388f);
    [SerializeField] private Rect auxiliarySpeakerRect =
        new Rect(1248f, 242f, 560f, 40f);
    [SerializeField] private Rect auxiliaryBodyRect =
        new Rect(1248f, 300f, 560f, 236f);
    [SerializeField] private Rect auxiliaryAdvanceHintRect =
        new Rect(1584f, 570f, 224f, 24f);

    [Header("Timing")]
    [SerializeField] private float defaultHoldSeconds = 2.75f;
    [SerializeField] private bool useUnscaledTime;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private HearthAudioChannelSource dialogueChannelSource;

    [Header("Dialogue Control")]
    [SerializeField] private HearthPlayerControlLock playerControlLock;
    [SerializeField] private KeyCode manualAdvanceKey = KeyCode.Space;

    private Coroutine activeRoutine;
    private int playbackGeneration;
    private HearthSubtitleContext activeContext;
    private HearthSubtitlePresentationMode activePresentationMode;
    private DialogueChannel activeChannel = DialogueChannel.Formal;
    private SpeakerSide activeSpeakerSide = SpeakerSide.None;
    private AdvancePolicy activeAdvancePolicy = AdvancePolicy.ManualSpace;
    private bool ownsControlLock;
    private bool adaptiveLayoutDirty;
    private Vector2 lastLayoutRootSize = new Vector2(-1f, -1f);
    private bool externalPresentationSuppressed;
    private float desiredCanvasAlpha;
    private bool manualAdvanceRequested;
    private HearthDialogueSurface activeExternalSurface;
    private readonly List<HearthCompanionHudController> suppressedCompanionHuds =
        new List<HearthCompanionHudController>();

    public bool IsPlaying { get; private set; }

    public static bool AnyDialoguePlaying
    {
        get { return ActiveDialoguePlayers.Count > 0; }
    }

    public event Action<string, MinLoopSubtitleLine, int> LineStarted;
    public event Action<string> SequenceCompleted;

    public HearthSubtitleStyleProfile StyleProfile
    {
        get { return styleProfile; }
    }

    public HearthSubtitlePresentationMode PresentationMode
    {
        get { return presentationMode; }
    }

    public GameObject VisualRoot
    {
        get { return GetVisualRoot(); }
    }

    public HearthSubtitleContext ActiveContext
    {
        get { return activeContext; }
    }

    public DialogueChannel ActiveChannel
    {
        get { return activeChannel; }
    }

    public SpeakerSide ActiveSpeakerSide
    {
        get { return activeSpeakerSide; }
    }

    public AdvancePolicy ActiveAdvancePolicy
    {
        get { return activeAdvancePolicy; }
    }

    public bool ExternalPresentationSuppressed
    {
        get { return externalPresentationSuppressed; }
    }

    private void Awake()
    {
        activeContext = defaultContext;
        activePresentationMode = presentationMode;
        EnsureReferences();
        HideImmediate();
    }

    private void LateUpdate()
    {
        GameObject root = GetVisualRoot();
        if (root == null || !root.activeInHierarchy)
        {
            return;
        }

        RectTransform rootRect = GetLayoutRoot();
        Vector2 rootSize = ResolveRootSize(rootRect);
        if (rootSize != lastLayoutRootSize)
        {
            adaptiveLayoutDirty = true;
        }

        if (adaptiveLayoutDirty)
        {
            ApplyAdaptiveLayout(activePresentationMode, activeContext);
        }
    }

    private void OnValidate()
    {
        defaultHoldSeconds = Mathf.Max(0f, defaultHoldSeconds);
        cleanSpeakerFontSize = Mathf.Max(1f, cleanSpeakerFontSize);
        cleanBodyFontSize = Mathf.Max(1f, cleanBodyFontSize);
    }

    private void OnDisable()
    {
        ActiveDialoguePlayers.Remove(this);
        ClearPersistentSceneHeader();
        RestoreCompanionDialogueLayers();
        ReleaseDialogueControlLock();
        HideActiveExternalSurface();
    }

    public Coroutine PlaySequence(IList<MinLoopSubtitleLine> lines)
    {
        Stop();
        activeRoutine = StartCoroutine(PlaySequenceRoutine(lines));
        return activeRoutine;
    }

    public Coroutine PlaySequence(
        IList<MinLoopSubtitleLine> lines,
        HearthSubtitleContext context)
    {
        Stop();
        activeRoutine = StartCoroutine(PlaySequenceRoutine(lines, context));
        return activeRoutine;
    }

    public Coroutine PlaySequence(HearthDialogueSequence sequence)
    {
        Stop();
        activeRoutine = StartCoroutine(PlaySequenceRoutine(sequence));
        return activeRoutine;
    }

    public Coroutine PlaySequence(
        HearthDialogueSequence sequence,
        HearthSubtitleContext context)
    {
        Stop();
        activeRoutine = StartCoroutine(PlaySequenceRoutine(sequence, context));
        return activeRoutine;
    }

    public Coroutine PlaySequence(
        HearthDialogueSequence sequence,
        HearthDialoguePlaybackContext playbackContext)
    {
        Stop();
        HearthSubtitleContext? context =
            playbackContext != null && playbackContext.HasSubtitleContextOverride
                ? playbackContext.SubtitleContext
                : (HearthSubtitleContext?)null;
        HearthDialogueSurface surface = playbackContext != null
            ? playbackContext.FramedSurface
            : null;
        HearthDialogueSurface messageSurface = playbackContext != null
            ? playbackContext.MessageSurface
            : null;
        activeRoutine = StartCoroutine(
            PlaySequenceRoutine(
                sequence,
                context,
                null,
                null,
                surface,
                messageSurface));
        return activeRoutine;
    }

    public IEnumerator PlayLines(IList<MinLoopSubtitleLine> lines)
    {
        Stop();
        yield return PlaySequenceRoutine(lines);
    }

    public IEnumerator PlayLines(
        IList<MinLoopSubtitleLine> lines,
        HearthSubtitleContext context)
    {
        Stop();
        yield return PlaySequenceRoutine(lines, context);
    }

    public IEnumerator PlaySequenceAsset(HearthDialogueSequence sequence)
    {
        Stop();
        yield return PlaySequenceRoutine(sequence);
    }

    /// <summary>
    /// Plays one sequence with an explicit visual context without changing the
    /// player's configured automatic/fixed context mode.
    /// </summary>
    public IEnumerator PlaySequenceAsset(
        HearthDialogueSequence sequence,
        HearthSubtitleContext context)
    {
        Stop();
        yield return PlaySequenceRoutine(sequence, context);
    }

    /// <summary>
    /// Plays a sequence with an optional world-space target for framed lines.
    /// Natural captions and time cards always remain on the global HUD.
    /// </summary>
    public IEnumerator PlaySequenceAsset(
        HearthDialogueSequence sequence,
        HearthDialoguePlaybackContext playbackContext)
    {
        Stop();
        HearthSubtitleContext? context =
            playbackContext != null && playbackContext.HasSubtitleContextOverride
                ? playbackContext.SubtitleContext
                : (HearthSubtitleContext?)null;
        HearthDialogueSurface surface = playbackContext != null
            ? playbackContext.FramedSurface
            : null;
        HearthDialogueSurface messageSurface = playbackContext != null
            ? playbackContext.MessageSurface
            : null;
        yield return PlaySequenceRoutine(
            sequence,
            context,
            null,
            null,
            surface,
            messageSurface);
    }

    /// <summary>
    /// Plays an authored sequence while explicitly choosing its visual lane
    /// and/or progression policy. This keeps terminal narration, automatic
    /// scene dialogue and black-screen captions on the shared player without
    /// forcing every line into the same framed Space-to-continue treatment.
    /// </summary>
    public IEnumerator PlaySequenceAsset(
        HearthDialogueSequence sequence,
        HearthSubtitleContext context,
        HearthSubtitlePresentationMode mode,
        AdvancePolicy advancePolicy)
    {
        Stop();
        yield return PlaySequenceRoutine(
            sequence,
            context,
            mode,
            advancePolicy);
    }

    public void ShowLine(string speaker, string text)
    {
        ShowLine(speaker, text, presentationMode);
    }

    public void ShowLine(string speaker, string text, HearthSubtitleContext context)
    {
        ShowLine(speaker, text, presentationMode, context);
    }

    private void ShowLine(string speaker, string text, HearthSubtitlePresentationMode mode)
    {
        ShowLine(speaker, text, mode, ResolveContext(speaker));
    }

    private void ShowLine(
        string speaker,
        string text,
        HearthSubtitlePresentationMode mode,
        HearthSubtitleContext context)
    {
        DialogueChannel channel = ResolveChannel(null, context, speaker);
        ShowLine(
            speaker,
            text,
            mode,
            context,
            channel,
            ResolveSpeakerSide(SpeakerSide.Auto, speaker),
            AdvancePolicy.ManualSpace,
            true);
    }

    private void ShowLine(
        string speaker,
        string text,
        HearthSubtitlePresentationMode mode,
        HearthSubtitleContext context,
        DialogueChannel channel,
        SpeakerSide side,
        AdvancePolicy advancePolicy,
        bool showVisual,
        HearthDialogueSurface externalSurface = null)
    {
        EnsureReferences();
        activePresentationMode = mode;
        activeContext = context;
        activeChannel = channel;
        activeSpeakerSide = side;
        activeAdvancePolicy = advancePolicy;

        bool useExternalSurface =
            showVisual &&
            externalSurface != null &&
            (mode == HearthSubtitlePresentationMode.StandardDialogue ||
             mode == HearthSubtitlePresentationMode.TerminalLowerThird);
        if (useExternalSurface)
        {
            if (activeExternalSurface != null &&
                activeExternalSurface != externalSurface)
            {
                activeExternalSurface.HideImmediate();
            }

            activeExternalSurface = externalSurface;
            HideGlobalVisualImmediate();
            activeExternalSurface.Show(
                speaker,
                text,
                side != SpeakerSide.None,
                advancePolicy == AdvancePolicy.ManualSpace);
            return;
        }

        HideActiveExternalSurface();

        if (speakerText != null)
        {
            speakerText.text = speaker;
            speakerText.gameObject.SetActive(
                showVisual &&
                mode != HearthSubtitlePresentationMode.TimeCard &&
                mode != HearthSubtitlePresentationMode.NaturalCaption &&
                mode != HearthSubtitlePresentationMode.CenteredEpilogue &&
                side != SpeakerSide.None &&
                !string.IsNullOrWhiteSpace(speaker));
        }

        if (bodyText != null)
        {
            bodyText.text = text;
            bodyText.gameObject.SetActive(showVisual);
        }

        GameObject root = GetVisualRoot();
        if (root != null)
        {
            root.SetActive(showVisual);
        }

        if (canvasGroup != null)
        {
            desiredCanvasAlpha = showVisual ? 1f : 0f;
            ApplyDesiredCanvasAlpha();
        }

        if (showVisual)
        {
            ApplyConfiguredStyle(mode);
        }
    }

    public void SetPresentation(HearthSubtitleStyleProfile profile, HearthSubtitlePresentationMode mode)
    {
        styleProfile = profile;
        presentationMode = mode;
        activePresentationMode = mode;
        ApplyConfiguredStyle();
    }

    public void SetUiProfiles(
        HearthUiThemeProfile themeProfile,
        HearthUiLayoutProfile layoutProfile)
    {
        uiThemeProfile = themeProfile;
        uiLayoutProfile = layoutProfile;
        EnsurePersistentSceneHeader();
        ApplyPersistentSceneHeaderStyle();
    }

    public void SetSubtitleContext(HearthSubtitleContext context)
    {
        defaultContext = context;
        activeContext = context;
        inferContextFromSpeaker = false;
        ApplyConfiguredStyle(activePresentationMode);
    }

    public void UseAutomaticSubtitleContext()
    {
        inferContextFromSpeaker = true;
    }

    public void SetExternalPresentationSuppressed(bool suppressed)
    {
        if (externalPresentationSuppressed == suppressed)
        {
            return;
        }

        externalPresentationSuppressed = suppressed;
        ApplyDesiredCanvasAlpha();
    }

    public void Hide()
    {
        Stop();
    }

    /// <summary>
    /// Advances the same manual dialogue gate as the configured Space key.
    /// This is intended for UI buttons and deterministic Play Mode tests; it
    /// does not bypass audio-complete lines or non-dialogue flow gates.
    /// </summary>
    public void RequestManualAdvance()
    {
        if (IsPlaying && activeAdvancePolicy == AdvancePolicy.ManualSpace)
        {
            manualAdvanceRequested = true;
        }
    }

    public void Stop()
    {
        playbackGeneration++;
        ActiveDialoguePlayers.Remove(this);
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        IsPlaying = false;
        manualAdvanceRequested = false;
        ClearPersistentSceneHeader();
        RestoreCompanionDialogueLayers();
        ReleaseDialogueControlLock();
        HideImmediate();
    }

    private IEnumerator PlaySequenceRoutine(
        IList<MinLoopSubtitleLine> lines,
        HearthSubtitleContext? explicitContext = null,
        DialogueChannel? sequenceChannel = null,
        SpeakerSide sequenceSpeakerSide = SpeakerSide.Auto,
        AdvancePolicy sequenceAdvancePolicy = AdvancePolicy.ManualSpace,
        HearthSubtitlePresentationMode? explicitPresentation = null,
        string sequenceId = null,
        HearthDialogueSurface externalSurface = null,
        HearthDialogueSurface externalMessageSurface = null)
    {
        int playbackToken = playbackGeneration;
        IsPlaying = true;
        ActiveDialoguePlayers.Add(this);
        ClearPersistentSceneHeader();

        if (lines == null || lines.Count == 0)
        {
            ActiveDialoguePlayers.Remove(this);
            HideImmediate();
            ClearPersistentSceneHeader();
            IsPlaying = false;
            activeRoutine = null;
            yield break;
        }

        for (int i = 0; i < lines.Count; i++)
        {
            MinLoopSubtitleLine line = lines[i];
            if (line == null)
            {
                continue;
            }

            if (line.startDelay > 0f)
            {
                yield return Wait(line.startDelay);
                if (playbackToken != playbackGeneration)
                {
                    yield break;
                }
            }

            HearthSubtitleContext lineContext = explicitContext.HasValue
                ? explicitContext.Value
                : ResolveContext(line.speaker);
            HearthSubtitleLinePresentationKind authoredPresentation =
                line.presentationKind;
            HearthDialogueLineAdvancePolicy authoredAdvancePolicy =
                line.advancePolicy;
            HearthDialogueLineMode authoredDialogueMode = line.dialogueMode;
            HearthDialoguePlaybackPolicy.Apply(
                sequenceId,
                line.lineId,
                ref authoredPresentation,
                ref authoredAdvancePolicy,
                ref authoredDialogueMode);
            HearthSubtitlePresentationMode linePresentation =
                ResolveLinePresentation(
                    authoredPresentation,
                    explicitContext,
                    explicitPresentation);
            DialogueChannel lineChannel = ResolveLineChannel(
                line,
                sequenceChannel,
                lineContext);
            if (authoredDialogueMode == HearthDialogueLineMode.DedicatedMessage)
            {
                lineChannel = DialogueChannel.Auxiliary;
            }
            SpeakerSide lineSpeakerSide = ResolveLineSpeakerSide(
                line,
                sequenceSpeakerSide);
            AdvancePolicy lineAdvancePolicy = ResolveLineAdvancePolicy(
                authoredAdvancePolicy,
                sequenceAdvancePolicy);
            bool audioOnly =
                authoredDialogueMode == HearthDialogueLineMode.AudioOnly;
            HearthDialogueSurface lineSurface =
                authoredDialogueMode == HearthDialogueLineMode.DedicatedMessage &&
                externalMessageSurface != null
                    ? externalMessageSurface
                    : externalSurface;
            // Audio-only lines still respect their authored advance policy.
            // This keeps dedicated HUD messages skippable with Space without
            // bringing back the ordinary bottom dialogue box.
            AdvancePolicy effectiveAdvancePolicy =
                linePresentation == HearthSubtitlePresentationMode.NaturalCaption
                    ? AdvancePolicy.AudioComplete
                    : lineAdvancePolicy;
            manualAdvanceRequested = false;

            if (LineStarted != null)
            {
                LineStarted(sequenceId, line, i);
            }

            ApplyDialogueControlLock(lineChannel);
            ShowLine(
                line.speaker,
                line.text,
                linePresentation,
                lineContext,
                lineChannel,
                audioOnly ? SpeakerSide.None : lineSpeakerSide,
                effectiveAdvancePolicy,
                !audioOnly,
                lineSurface);
            SetCompanionDialogueExclusive(
                ShouldSuppressCompanionDecisionLayer(
                    line.speaker,
                    linePresentation,
                    lineChannel));
            PlayVoice(line.voiceClip);

            float holdSeconds = ResolveLineDuration(line);
            if (linePresentation == HearthSubtitlePresentationMode.TimeCard)
            {
                ClearPersistentSceneHeader();
                float fadeSeconds = styleProfile != null ? styleProfile.TimeCardFadeSeconds : 0.35f;
                yield return FadeCanvas(0f, 1f, fadeSeconds);
                if (playbackToken != playbackGeneration)
                {
                    yield break;
                }
                if (holdSeconds > 0f)
                {
                    yield return Wait(holdSeconds);
                    if (playbackToken != playbackGeneration)
                    {
                        yield break;
                    }
                }
                yield return TransitionTimeCardToPersistentHeader(
                    line.text,
                    fadeSeconds);
                if (playbackToken != playbackGeneration)
                {
                    yield break;
                }
            }
            else if (effectiveAdvancePolicy == AdvancePolicy.ManualSpace)
            {
                yield return WaitForManualAdvance(playbackToken);
                if (playbackToken != playbackGeneration)
                {
                    yield break;
                }
            }
            else if (holdSeconds > 0f)
            {
                yield return Wait(holdSeconds);
                if (playbackToken != playbackGeneration)
                {
                    yield break;
                }
            }

            RestoreCompanionDialogueLayers();
        }

        if (playbackToken != playbackGeneration)
        {
            yield break;
        }

        HideImmediate();
        ClearPersistentSceneHeader();
        RestoreCompanionDialogueLayers();
        IsPlaying = false;
        ActiveDialoguePlayers.Remove(this);
        activeRoutine = null;
        ReleaseDialogueControlLock();
    }

    private IEnumerator PlaySequenceRoutine(
        HearthDialogueSequence sequence,
        HearthSubtitleContext? explicitContext = null,
        HearthSubtitlePresentationMode? explicitPresentation = null,
        AdvancePolicy? explicitAdvancePolicy = null,
        HearthDialogueSurface externalSurface = null,
        HearthDialogueSurface externalMessageSurface = null)
    {
        int playbackToken = playbackGeneration;
        if (sequence == null || !sequence.HasLines)
        {
            yield return PlaySequenceRoutine(
                (IList<MinLoopSubtitleLine>)null,
                explicitContext);
            yield break;
        }

        IList<MinLoopSubtitleLine> sequenceLines = sequence.Lines as IList<MinLoopSubtitleLine>;
        if (sequenceLines == null)
        {
            sequenceLines = new List<MinLoopSubtitleLine>(sequence.Lines);
        }

        DialogueChannel resolvedChannel = ResolveChannel(
            sequence,
            explicitContext,
            null);
        yield return PlaySequenceRoutine(
            sequenceLines,
            explicitContext,
            resolvedChannel,
            sequence.DefaultSpeakerSide,
            explicitAdvancePolicy ?? sequence.AdvancePolicy,
            explicitPresentation,
            sequence.SequenceId,
            externalSurface,
            externalMessageSurface);

        if (playbackToken != playbackGeneration)
        {
            yield break;
        }

        if (sequence.PostSequenceDelay > 0f)
        {
            yield return Wait(sequence.PostSequenceDelay);
        }

        if (playbackToken == playbackGeneration && SequenceCompleted != null)
        {
            SequenceCompleted(sequence.SequenceId);
        }
    }

    private void PlayVoice(AudioClip clip)
    {
        if (clip == null || audioSource == null)
        {
            return;
        }

        audioSource.Stop();
        audioSource.clip = clip;
        EnsureDialogueChannelSource();
        if (dialogueChannelSource != null)
        {
            dialogueChannelSource.ApplyVolume();
        }
        audioSource.Play();
    }

    private float ResolveLineDuration(MinLoopSubtitleLine line)
    {
        float manualDuration = line.holdSeconds > 0f ? line.holdSeconds : defaultHoldSeconds;
        if (line.voiceClip == null)
        {
            return manualDuration;
        }

        float voiceDuration = line.voiceClip.length + Mathf.Max(0f, line.voiceTailSeconds);
        switch (line.durationMode)
        {
            case HearthSubtitleDurationMode.ManualHold:
                return manualDuration;
            case HearthSubtitleDurationMode.LongerOfVoiceAndManual:
                return Mathf.Max(manualDuration, voiceDuration);
            default:
                return voiceDuration;
        }
    }

    private IEnumerator WaitForManualAdvance(int playbackToken)
    {
        while (playbackToken == playbackGeneration &&
               !manualAdvanceRequested &&
               Input.GetKey(manualAdvanceKey))
        {
            yield return null;
        }

        while (playbackToken == playbackGeneration &&
               !manualAdvanceRequested &&
               !Input.GetKeyDown(manualAdvanceKey))
        {
            yield return null;
        }

        if (playbackToken != playbackGeneration)
        {
            yield break;
        }

        manualAdvanceRequested = false;

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }

    private DialogueChannel ResolveChannel(
        HearthDialogueSequence sequence,
        HearthSubtitleContext? explicitContext,
        string speaker)
    {
        if (explicitContext.HasValue &&
            (explicitContext.Value == HearthSubtitleContext.FieldUnit ||
             explicitContext.Value == HearthSubtitleContext.Terminal))
        {
            return DialogueChannel.Auxiliary;
        }

        if (sequence != null)
        {
            return sequence.DialogueChannel;
        }

        HearthSubtitleContext inferredContext = explicitContext.HasValue
            ? explicitContext.Value
            : ResolveContext(speaker);
        return inferredContext == HearthSubtitleContext.FieldUnit ||
               inferredContext == HearthSubtitleContext.Terminal
            ? DialogueChannel.Auxiliary
            : DialogueChannel.Formal;
    }

    private DialogueChannel ResolveLineChannel(
        MinLoopSubtitleLine line,
        DialogueChannel? sequenceChannel,
        HearthSubtitleContext context)
    {
        if (line.dialogueMode == HearthDialogueLineMode.Formal)
        {
            return DialogueChannel.Formal;
        }

        if (line.dialogueMode == HearthDialogueLineMode.Auxiliary ||
            line.dialogueMode == HearthDialogueLineMode.AudioOnly)
        {
            return DialogueChannel.Auxiliary;
        }

        if (context == HearthSubtitleContext.FieldUnit ||
            context == HearthSubtitleContext.Terminal)
        {
            return DialogueChannel.Auxiliary;
        }

        if (sequenceChannel == DialogueChannel.Auxiliary)
        {
            return DialogueChannel.Auxiliary;
        }

        return DialogueChannel.Formal;
    }

    private static SpeakerSide ResolveLineSpeakerSide(
        MinLoopSubtitleLine line,
        SpeakerSide sequenceSpeakerSide)
    {
        SpeakerSide requestedSide = line.speakerSide != SpeakerSide.Auto
            ? line.speakerSide
            : sequenceSpeakerSide;
        return ResolveSpeakerSide(requestedSide, line.speaker);
    }

    private static SpeakerSide ResolveSpeakerSide(
        SpeakerSide requestedSide,
        string speaker)
    {
        if (requestedSide != SpeakerSide.Auto)
        {
            return requestedSide;
        }

        string normalized = NormalizeSpeaker(speaker);
        if (normalized.Length == 0)
        {
            return SpeakerSide.None;
        }

        if (IsMiaSpeaker(normalized) ||
            normalized.Contains("COMPANION") ||
            normalized.Contains("HOME UNIT") ||
            normalized.Contains("FIELD UNIT") ||
            normalized.Contains("SYNTH VOICE"))
        {
            return SpeakerSide.Right;
        }

        return SpeakerSide.Left;
    }

    private static AdvancePolicy ResolveLineAdvancePolicy(
        HearthDialogueLineAdvancePolicy authoredAdvancePolicy,
        AdvancePolicy sequenceAdvancePolicy)
    {
        switch (authoredAdvancePolicy)
        {
            case HearthDialogueLineAdvancePolicy.ManualSpace:
                return AdvancePolicy.ManualSpace;
            case HearthDialogueLineAdvancePolicy.AudioComplete:
                return AdvancePolicy.AudioComplete;
            default:
                return sequenceAdvancePolicy;
        }
    }

    private HearthSubtitlePresentationMode ResolveLinePresentation(
        HearthSubtitleLinePresentationKind authoredPresentation,
        HearthSubtitleContext? explicitContext,
        HearthSubtitlePresentationMode? explicitPresentation)
    {
        switch (authoredPresentation)
        {
            case HearthSubtitleLinePresentationKind.TimeCard:
                return HearthSubtitlePresentationMode.TimeCard;
        }

        // The black-screen finale explicitly owns the ordinary dialogue lane.
        // Time cards remain authored above, while every other finale line stays
        // centered even if it was previously tagged as a natural caption.
        if (explicitPresentation == HearthSubtitlePresentationMode.CenteredEpilogue)
        {
            return HearthSubtitlePresentationMode.CenteredEpilogue;
        }

        switch (authoredPresentation)
        {
            case HearthSubtitleLinePresentationKind.NaturalCaption:
                return HearthSubtitlePresentationMode.NaturalCaption;
            case HearthSubtitleLinePresentationKind.TerminalLowerThird:
                return HearthSubtitlePresentationMode.TerminalLowerThird;
        }

        if (explicitPresentation.HasValue)
        {
            return explicitPresentation.Value;
        }

        if (explicitContext == HearthSubtitleContext.Terminal)
        {
            return HearthSubtitlePresentationMode.TerminalLowerThird;
        }

        return presentationMode;
    }

    private void ApplyDialogueControlLock(DialogueChannel channel)
    {
        HearthPlayerControlMask requestedMask = channel == DialogueChannel.Formal
            ? HearthPlayerControlMask.All
            : HearthPlayerControlMask.Interaction | HearthPlayerControlMask.Menu;

        EnsurePlayerControlLock();
        if (playerControlLock == null)
        {
            return;
        }

        if (ownsControlLock &&
            playerControlLock.IsLocked(requestedMask) &&
            activeChannel == channel)
        {
            return;
        }

        ReleaseDialogueControlLock();
        playerControlLock.SetControlLock(this, requestedMask, true);
        ownsControlLock = true;
    }

    private void ReleaseDialogueControlLock()
    {
        if (!ownsControlLock || playerControlLock == null)
        {
            ownsControlLock = false;
            return;
        }

        playerControlLock.ReleaseOwner(this);
        ownsControlLock = false;
    }

    private void EnsurePlayerControlLock()
    {
        if (playerControlLock == null && Application.isPlaying)
        {
            playerControlLock =
                UnityEngine.Object.FindObjectOfType<HearthPlayerControlLock>();
        }
    }

    private static string NormalizeSpeaker(string speaker)
    {
        return (speaker ?? string.Empty).Trim().ToUpperInvariant();
    }

    private static bool IsMiaSpeaker(string speaker)
    {
        string normalized = NormalizeSpeaker(speaker);
        return normalized == "MIA" || normalized.StartsWith("MIA ");
    }

    private static bool ShouldSuppressCompanionDecisionLayer(
        string speaker,
        HearthSubtitlePresentationMode presentation,
        DialogueChannel channel)
    {
        if (presentation == HearthSubtitlePresentationMode.TimeCard ||
            presentation == HearthSubtitlePresentationMode.NaturalCaption ||
            presentation == HearthSubtitlePresentationMode.CenteredEpilogue)
        {
            return false;
        }

        // Synth/Field lines can be routed through the auxiliary audio channel
        // while still being the only formal visual message on Companion HUD.
        // Do not use the audio channel as the visibility gate.
        string normalized = NormalizeSpeaker(speaker);
        return normalized.Contains("FIELD UNIT") ||
               normalized.Contains("SYNTH VOICE") ||
               normalized.Contains("HOME UNIT");
    }

    private void SetCompanionDialogueExclusive(bool exclusive)
    {
        RestoreCompanionDialogueLayers();
        if (!exclusive || !Application.isPlaying)
        {
            return;
        }

        HearthCompanionHudController[] controllers =
            FindObjectsOfType<HearthCompanionHudController>(true);
        for (int i = 0; i < controllers.Length; i++)
        {
            HearthCompanionHudController controller = controllers[i];
            if (controller == null || !controller.IsPresented)
            {
                continue;
            }

            controller.SetTransientDialogueExclusive(true);
            suppressedCompanionHuds.Add(controller);
        }
    }

    private void RestoreCompanionDialogueLayers()
    {
        for (int i = 0; i < suppressedCompanionHuds.Count; i++)
        {
            HearthCompanionHudController controller = suppressedCompanionHuds[i];
            if (controller != null)
            {
                controller.SetTransientDialogueExclusive(false);
            }
        }
        suppressedCompanionHuds.Clear();
    }

    private IEnumerator Wait(float seconds)
    {
        if (useUnscaledTime)
        {
            yield return new WaitForSecondsRealtime(seconds);
        }
        else
        {
            yield return new WaitForSeconds(seconds);
        }
    }

    private IEnumerator FadeCanvas(float from, float to, float seconds)
    {
        if (canvasGroup == null || seconds <= 0f)
        {
            desiredCanvasAlpha = to;
            ApplyDesiredCanvasAlpha();
            yield break;
        }

        desiredCanvasAlpha = from;
        ApplyDesiredCanvasAlpha();
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            desiredCanvasAlpha =
                Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / seconds));
            ApplyDesiredCanvasAlpha();
            yield return null;
        }

        desiredCanvasAlpha = to;
        ApplyDesiredCanvasAlpha();
    }

    private IEnumerator TransitionTimeCardToPersistentHeader(
        string text,
        float seconds)
    {
        EnsurePersistentSceneHeader();
        if (persistentSceneHeaderText != null)
        {
            persistentSceneHeaderText.text = text ?? string.Empty;
        }

        if (persistentSceneHeaderGroup == null || seconds <= 0f)
        {
            desiredCanvasAlpha = 0f;
            ApplyDesiredCanvasAlpha();
            SetPersistentSceneHeaderAlpha(1f);
            yield break;
        }

        SetPersistentSceneHeaderAlpha(0f);
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / seconds);
            desiredCanvasAlpha = 1f - progress;
            ApplyDesiredCanvasAlpha();
            SetPersistentSceneHeaderAlpha(progress);
            yield return null;
        }

        desiredCanvasAlpha = 0f;
        ApplyDesiredCanvasAlpha();
        SetPersistentSceneHeaderAlpha(1f);
    }

    private void HideImmediate()
    {
        HideGlobalVisualImmediate();
        HideActiveExternalSurface();
    }

    private void HideGlobalVisualImmediate()
    {
        GameObject root = GetVisualRoot();
        if (root != null)
        {
            root.SetActive(false);
        }

        if (canvasGroup != null)
        {
            desiredCanvasAlpha = 0f;
            ApplyDesiredCanvasAlpha();
        }
    }

    private void HideActiveExternalSurface()
    {
        if (activeExternalSurface != null)
        {
            activeExternalSurface.HideImmediate();
            activeExternalSurface = null;
        }
    }

    private void ApplyDesiredCanvasAlpha()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha =
                externalPresentationSuppressed ? 0f : desiredCanvasAlpha;
        }
    }

    private void EnsureReferences()
    {
        EnsureDialogueChannelSource();
        AdoptLegacyVisualBindings();

        if (GetVisualRoot() != null && speakerText != null && bodyText != null)
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetVisualRoot().GetComponent<CanvasGroup>();
            }

            FindOptionalVisualReferences();
            ApplyConfiguredStyle();
            EnsureSubtitleCanvasSorting();
            EnsurePersistentSceneHeader();
            return;
        }

        if (createFallbackUI)
        {
            CreateFallbackUI();
        }

        ApplyConfiguredStyle();
        EnsureSubtitleCanvasSorting();
        EnsurePersistentSceneHeader();
    }

    private void AdoptLegacyVisualBindings()
    {
        if (visualRoot == null && subtitlePanel != null)
        {
            visualRoot = subtitlePanel;
        }
        else if (subtitlePanel == null && visualRoot != null)
        {
            subtitlePanel = visualRoot;
        }

        if (layoutRoot == null && GetVisualRoot() != null)
        {
            layoutRoot = GetVisualRoot().GetComponent<RectTransform>();
        }
    }

    private void FindOptionalVisualReferences()
    {
        if (GetVisualRoot() == null)
        {
            return;
        }

        if (backdropImage == null)
        {
            Transform backdrop = GetVisualRoot().transform.Find("Backdrop");
            if (backdrop != null)
            {
                backdropImage = backdrop.GetComponent<Image>();
            }
        }

        if (accentRuleImage == null)
        {
            Transform accent = GetVisualRoot().transform.Find("AccentRule");
            if (accent != null)
            {
                accentRuleImage = accent.GetComponent<Image>();
            }
        }

        if (speakerTabImage == null)
        {
            Transform speakerTab = GetVisualRoot().transform.Find("SpeakerTab");
            if (speakerTab != null)
            {
                speakerTabImage = speakerTab.GetComponent<Image>();
            }
        }

        if (formalFrameImage == null)
        {
            formalFrameImage =
                FindImage(GetVisualRoot().transform, "FormalFrame");
        }

        if (auxiliaryFrameImage == null)
        {
            auxiliaryFrameImage =
                FindImage(GetVisualRoot().transform, "AuxiliaryFrame");
        }

        if (leftSpeakerTabImage == null)
        {
            leftSpeakerTabImage =
                FindImage(GetVisualRoot().transform, "SpeakerTabLeft");
        }

        if (rightSpeakerTabImage == null)
        {
            rightSpeakerTabImage =
                FindImage(GetVisualRoot().transform, "SpeakerTabRight");
        }

        if (advanceHintText == null)
        {
            Transform hint =
                GetVisualRoot().transform.Find("AdvanceHint");
            if (hint != null)
            {
                advanceHintText = hint.GetComponent<TMP_Text>();
            }
        }
    }

    private void EnsureDialogueChannelSource()
    {
        if (audioSource == null)
        {
            return;
        }

        if (dialogueChannelSource == null)
        {
            dialogueChannelSource = audioSource.GetComponent<HearthAudioChannelSource>();
        }
    }

    private void CreateFallbackUI()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Min Loop Subtitle Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (GetVisualRoot() == null)
        {
            GameObject panelObject = new GameObject("HearthSubtitleVisualRoot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            panelObject.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = Color.clear;
            panelImage.raycastTarget = false;

            canvasGroup = panelObject.GetComponent<CanvasGroup>();
            visualRoot = panelObject;
            subtitlePanel = panelObject;
            layoutRoot = panelRect;
        }

        Transform visualParent = GetVisualRoot().transform;
        if (backdropImage == null)
        {
            backdropImage = CreateImage(visualParent, "Backdrop");
        }

        if (accentRuleImage == null)
        {
            accentRuleImage = CreateImage(visualParent, "AccentRule");
        }

        if (speakerTabImage == null)
        {
            speakerTabImage = CreateImage(visualParent, "SpeakerTab");
        }

        if (speakerText == null)
        {
            speakerText = CreateText(visualParent, "Speaker", 22f, FontStyles.Bold);
        }

        if (bodyText == null)
        {
            bodyText = CreateText(visualParent, "Body", 28f, FontStyles.Normal);
        }
    }

    private Image CreateImage(Transform parent, string objectName)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.GetComponent<Image>();
        image.color = Color.clear;
        image.raycastTarget = false;
        return image;
    }

    private TMP_Text CreateText(Transform parent, string objectName, float fontSize, FontStyles style)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.17f, 0.14f);
        rect.anchorMax = new Vector2(0.83f, 0.14f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.color = new Color(0.84f, 0.9f, 0.96f, 1f);
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.enableAutoSizing = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private void ApplyConfiguredStyle()
    {
        ApplyConfiguredStyle(presentationMode);
    }

    private void ApplyConfiguredStyle(HearthSubtitlePresentationMode mode)
    {
        GameObject root = GetVisualRoot();
        if (!useCleanCenteredStyle || root == null)
        {
            return;
        }

        RectTransform panelRect = GetLayoutRoot();
        if (panelRect != null)
        {
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
        }

        Image panelImage = root.GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.color = Color.clear;
            panelImage.raycastTarget = false;
        }

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        EnsureSubtitleCanvasSorting();

        HearthSubtitleLayoutSettings sharedLayout = styleProfile != null
            ? styleProfile.GetLayout(mode)
            : null;
        float speakerSize = styleProfile != null
            ? styleProfile.GetSpeakerFontSize(mode)
            : (mode == HearthSubtitlePresentationMode.TimeCard
                ? 1f
                : (mode == HearthSubtitlePresentationMode.CenteredEpilogue
                    ? 30f
                    : cleanSpeakerFontSize));
        float bodySize = styleProfile != null
            ? styleProfile.GetBodyFontSize(mode)
            : (mode == HearthSubtitlePresentationMode.TimeCard
                ? 34f
                : (mode == HearthSubtitlePresentationMode.CenteredEpilogue
                    ? 28f
                    : cleanBodyFontSize));
        if (mode == HearthSubtitlePresentationMode.NaturalCaption)
        {
            bodySize = 27f;
        }
        else if (mode == HearthSubtitlePresentationMode.TerminalLowerThird)
        {
            speakerSize = 22f;
            bodySize = 24f;
        }
        float lineSpacing = sharedLayout != null ? sharedLayout.lineSpacing : 0f;
        Color textColor = styleProfile != null ? styleProfile.TextColor : cleanTextColor;
        HearthSubtitleContextSettings contextStyle = styleProfile != null
            ? styleProfile.GetContext(activeContext)
            : null;
        Color speakerColor = contextStyle != null ? contextStyle.speakerColor : textColor;

        ApplyTextStyle(
            speakerText,
            speakerSize,
            0f,
            speakerColor,
            FontStyles.Bold);
        ApplyTextStyle(
            bodyText,
            bodySize,
            lineSpacing,
            textColor,
            FontStyles.Normal);

        if (backdropImage != null)
        {
            if (mode == HearthSubtitlePresentationMode.TimeCard ||
                mode == HearthSubtitlePresentationMode.CenteredEpilogue)
            {
                backdropImage.color = Color.clear;
            }
            else if (mode == HearthSubtitlePresentationMode.NaturalCaption)
            {
                backdropImage.color = new Color(0f, 0f, 0f, 0.78f);
            }
            else if (mode == HearthSubtitlePresentationMode.TerminalLowerThird)
            {
                backdropImage.color = new Color(0.035f, 0.055f, 0.08f, 0.92f);
            }
            else
            {
                backdropImage.color = contextStyle != null
                    ? contextStyle.backgroundColor
                    : new Color(0.035f, 0.055f, 0.08f, 0.62f);
            }
            backdropImage.raycastTarget = false;
        }

        if (accentRuleImage != null)
        {
            accentRuleImage.color =
                mode == HearthSubtitlePresentationMode.TimeCard ||
                mode == HearthSubtitlePresentationMode.NaturalCaption ||
                mode == HearthSubtitlePresentationMode.CenteredEpilogue
                ? Color.clear
                : (contextStyle != null ? contextStyle.accentColor : new Color(0.47f, 0.67f, 0.86f, 1f));
            accentRuleImage.raycastTarget = false;
        }

        if (speakerTabImage != null)
        {
            speakerTabImage.color = Color.clear;
            speakerTabImage.raycastTarget = false;
            speakerTabImage.gameObject.SetActive(false);
        }

        bool standardDialogue =
            mode == HearthSubtitlePresentationMode.StandardDialogue;
        bool terminalLowerThird =
            mode == HearthSubtitlePresentationMode.TerminalLowerThird;
        bool auxiliary =
            standardDialogue &&
            activeChannel == DialogueChannel.Auxiliary;
        bool formal = standardDialogue && !auxiliary;
        Color frameColor =
            contextStyle != null
                ? contextStyle.accentColor
                : new Color(0.47f, 0.67f, 0.86f, 0.92f);

        ConfigureFrameVisibility(formalFrameImage, formal, frameColor);
        ConfigureFrameVisibility(
            auxiliaryFrameImage,
            auxiliary || terminalLowerThird,
            frameColor);
        ConfigureFrameVisibility(
            leftSpeakerTabImage,
            formal && activeSpeakerSide == SpeakerSide.Left,
            frameColor);
        ConfigureFrameVisibility(
            rightSpeakerTabImage,
            formal && activeSpeakerSide == SpeakerSide.Right,
            frameColor);

        if (advanceHintText != null)
        {
            bool showHint =
                (standardDialogue || terminalLowerThird) &&
                activeAdvancePolicy == AdvancePolicy.ManualSpace;
            advanceHintText.text = "SPACE  CONTINUE";
            advanceHintText.gameObject.SetActive(showHint);
            ApplyTextStyle(
                advanceHintText,
                auxiliary || terminalLowerThird ? 14f : 15f,
                0f,
                frameColor,
                FontStyles.Bold);
            advanceHintText.alignment = TextAlignmentOptions.Right;
        }

        activePresentationMode = mode;
        adaptiveLayoutDirty = true;
        ApplyAdaptiveLayout(mode, activeContext);
    }

    private void EnsurePersistentSceneHeader()
    {
        if (persistentSceneHeaderText != null &&
            persistentSceneHeaderGroup != null)
        {
            ApplyPersistentSceneHeaderStyle();
            return;
        }

        GameObject root = GetVisualRoot();
        Canvas canvas = root != null ? root.GetComponentInParent<Canvas>() : null;
        if (canvas == null)
        {
            return;
        }

        Transform existing = canvas.transform.Find("PersistentSceneHeader");
        GameObject headerObject = existing != null
            ? existing.gameObject
            : new GameObject(
                "PersistentSceneHeader",
                typeof(RectTransform),
                typeof(CanvasGroup),
                typeof(TextMeshProUGUI));
        if (existing == null)
        {
            headerObject.transform.SetParent(canvas.transform, false);
        }

        persistentSceneHeaderText =
            headerObject.GetComponent<TextMeshProUGUI>();
        persistentSceneHeaderGroup =
            headerObject.GetComponent<CanvasGroup>();
        ApplyPersistentSceneHeaderStyle();
        SetPersistentSceneHeaderAlpha(0f);
    }

    private void ApplyPersistentSceneHeaderStyle()
    {
        if (persistentSceneHeaderText == null)
        {
            return;
        }

        float fontSize = uiThemeProfile != null
            ? uiThemeProfile.PersistentSceneHeaderFontSize
            : 24f;
        ApplyTextStyle(
            persistentSceneHeaderText,
            fontSize,
            0f,
            Color.white,
            FontStyles.Bold);
        persistentSceneHeaderText.alignment = TextAlignmentOptions.Center;
        persistentSceneHeaderText.maxVisibleLines = 2;

        RectTransform rect = persistentSceneHeaderText.rectTransform;
        if (uiLayoutProfile != null)
        {
            uiLayoutProfile
                .GetRegion(HearthUiLayoutRegion.EpilogueSceneHeader)
                .ApplyTopLeftAnchors(rect);
        }
        else
        {
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -292f);
            rect.sizeDelta = new Vector2(1280f, 56f);
        }

        persistentSceneHeaderText.raycastTarget = false;
        if (persistentSceneHeaderGroup != null)
        {
            persistentSceneHeaderGroup.blocksRaycasts = false;
            persistentSceneHeaderGroup.interactable = false;
        }
    }

    private void ClearPersistentSceneHeader()
    {
        if (persistentSceneHeaderText != null)
        {
            persistentSceneHeaderText.text = string.Empty;
        }
        SetPersistentSceneHeaderAlpha(0f);
    }

    private void SetPersistentSceneHeaderAlpha(float alpha)
    {
        if (persistentSceneHeaderGroup != null)
        {
            persistentSceneHeaderGroup.alpha = alpha;
        }
    }

    private void ApplyTextStyle(
        TMP_Text text,
        float fontSize,
        float lineSpacing,
        Color color,
        FontStyles style)
    {
        if (text == null)
        {
            return;
        }

        text.color = color;
        text.fontSize = fontSize;
        text.fontSizeMax = fontSize;
        text.fontSizeMin = fontSize;
        text.enableAutoSizing = false;
        text.fontStyle = style;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.maxVisibleLines = 999;
        text.lineSpacing = lineSpacing;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
    }

    private void ApplyAdaptiveLayout(
        HearthSubtitlePresentationMode mode,
        HearthSubtitleContext context)
    {
        RectTransform rootRect = GetLayoutRoot();
        if (rootRect == null || bodyText == null)
        {
            adaptiveLayoutDirty = false;
            return;
        }

        Vector2 rootSize = ResolveRootSize(rootRect);
        if (mode == HearthSubtitlePresentationMode.StandardDialogue)
        {
            ApplyFixedDialogueLayout(rootSize);
            lastLayoutRootSize = rootSize;
            adaptiveLayoutDirty = false;
            return;
        }

        if (mode == HearthSubtitlePresentationMode.NaturalCaption ||
            mode == HearthSubtitlePresentationMode.TerminalLowerThird)
        {
            ApplyNarrativeLowerThirdLayout(
                rootSize,
                mode == HearthSubtitlePresentationMode.TerminalLowerThird);
            lastLayoutRootSize = rootSize;
            adaptiveLayoutDirty = false;
            return;
        }

        float rootWidth = Mathf.Max(1f, rootSize.x);
        float rootHeight = Mathf.Max(1f, rootSize.y);
        HearthSubtitleLayoutSettings layout = styleProfile != null
            ? styleProfile.GetLayout(mode)
            : null;
        HearthSubtitleContextSettings contextStyle = styleProfile != null
            ? styleProfile.GetContext(context)
            : null;

        float widthFraction = layout != null ? layout.widthFraction : subtitleWidthFraction;
        float widthMultiplier = contextStyle != null ? contextStyle.widthMultiplier : 1f;
        widthFraction = Mathf.Clamp(widthFraction * widthMultiplier, 0.35f, 0.92f);
        float xMin = 0.5f - widthFraction * 0.5f;
        float xMax = 0.5f + widthFraction * 0.5f;
        float availableTextWidth = Mathf.Max(1f, rootWidth * widthFraction);

        float bodyCenter = layout != null ? layout.bodyCenterY : bodyCenterY;
        float originalBodyHeight = layout != null ? layout.bodyHeightFraction : bodyHeightFraction;
        float bodyBottom = Mathf.Clamp(
            bodyCenter - originalBodyHeight * 0.5f,
            0.03f,
            0.86f);
        float minimumBodyHeight = layout != null
            ? Mathf.Max(48f, layout.minimumBodyHeightPixels)
            : Mathf.Max(72f, rootHeight * originalBodyHeight * 0.5f);

        Vector2 preferredBody = bodyText.GetPreferredValues(
            bodyText.text ?? string.Empty,
            availableTextWidth,
            0f);
        float bodyHeight = Mathf.Max(minimumBodyHeight, preferredBody.y + 4f);
        SetBottomAnchoredRect(
            bodyText.rectTransform,
            xMin,
            xMax,
            bodyBottom,
            bodyHeight,
            Vector2.zero);

        bool hasSpeaker =
            mode != HearthSubtitlePresentationMode.TimeCard &&
            speakerText != null &&
            speakerText.gameObject.activeSelf;
        float speakerHeight = layout != null
            ? Mathf.Max(28f, layout.speakerHeightPixels)
            : Mathf.Max(30f, rootHeight * speakerHeightFraction);
        float speakerGap = contextStyle != null ? contextStyle.speakerGapPixels : 8f;
        float contentHeight = bodyHeight;
        if (hasSpeaker)
        {
            float speakerBottom = bodyBottom + (bodyHeight + speakerGap) / rootHeight;
            SetBottomAnchoredRect(
                speakerText.rectTransform,
                xMin,
                xMax,
                speakerBottom,
                speakerHeight,
                Vector2.zero);
            contentHeight += speakerGap + speakerHeight;
        }

        float horizontalPadding = contextStyle != null ? contextStyle.horizontalPaddingPixels : 28f;
        float verticalPadding = contextStyle != null ? contextStyle.verticalPaddingPixels : 16f;
        if (backdropImage != null)
        {
            SetBottomAnchoredRect(
                backdropImage.rectTransform,
                xMin,
                xMax,
                bodyBottom,
                contentHeight + verticalPadding * 2f,
                new Vector2(0f, -verticalPadding),
                horizontalPadding * 2f);
        }

        if (accentRuleImage != null)
        {
            RectTransform accentRect = accentRuleImage.rectTransform;
            accentRect.anchorMin = new Vector2(xMin, bodyBottom);
            accentRect.anchorMax = new Vector2(xMin, bodyBottom);
            accentRect.pivot = new Vector2(0f, 0f);
            accentRect.anchoredPosition = new Vector2(-horizontalPadding, -verticalPadding);
            accentRect.sizeDelta = new Vector2(2f, contentHeight + verticalPadding * 2f);
        }

        lastLayoutRootSize = rootSize;
        adaptiveLayoutDirty = false;
    }

    private void ApplyFixedDialogueLayout(Vector2 rootSize)
    {
        bool auxiliary = activeChannel == DialogueChannel.Auxiliary;
        Rect backdrop = auxiliary
            ? auxiliaryBackdropRect
            : formalBackdropRect;
        Rect body = auxiliary
            ? auxiliaryBodyRect
            : formalBodyRect;
        Rect speaker = auxiliary
            ? auxiliarySpeakerRect
            : (activeSpeakerSide == SpeakerSide.Right
                ? formalRightSpeakerRect
                : formalLeftSpeakerRect);
        Rect advanceHint = auxiliary
            ? auxiliaryAdvanceHintRect
            : formalAdvanceHintRect;

        ApplyTopLeftRect(
            backdropImage != null ? backdropImage.rectTransform : null,
            backdrop,
            rootSize);
        if (accentRuleImage != null)
        {
            accentRuleImage.gameObject.SetActive(false);
        }
        ApplyTopLeftRect(
            speakerTabImage != null ? speakerTabImage.rectTransform : null,
            speaker,
            rootSize);
        ApplyTopLeftRect(
            formalFrameImage != null ? formalFrameImage.rectTransform : null,
            formalFrameRect,
            rootSize);
        ApplyTopLeftRect(
            auxiliaryFrameImage != null
                ? auxiliaryFrameImage.rectTransform
                : null,
            auxiliaryFrameRect,
            rootSize);
        ApplyTopLeftRect(
            leftSpeakerTabImage != null
                ? leftSpeakerTabImage.rectTransform
                : null,
            formalLeftSpeakerRect,
            rootSize);
        ApplyTopLeftRect(
            rightSpeakerTabImage != null
                ? rightSpeakerTabImage.rectTransform
                : null,
            formalRightSpeakerRect,
            rootSize);
        ApplyTopLeftRect(
            speakerText != null ? speakerText.rectTransform : null,
            speaker,
            rootSize,
            auxiliary
                ? Vector4.zero
                : new Vector4(16f, 4f, 16f, 4f));
        ApplyTopLeftRect(
            bodyText != null ? bodyText.rectTransform : null,
            body,
            rootSize);
        ApplyTopLeftRect(
            advanceHintText != null
                ? advanceHintText.rectTransform
                : null,
            advanceHint,
            rootSize);

        if (speakerText != null)
        {
            speakerText.alignment = auxiliary
                ? TextAlignmentOptions.Left
                : (activeSpeakerSide == SpeakerSide.Right
                    ? TextAlignmentOptions.Right
                    : TextAlignmentOptions.Left);
            speakerText.maxVisibleLines = 1;
            speakerText.overflowMode = TextOverflowModes.Overflow;
        }

        if (bodyText != null)
        {
            bodyText.alignment = TextAlignmentOptions.TopLeft;
            bodyText.maxVisibleLines = auxiliary ? 6 : 3;
            bodyText.overflowMode = TextOverflowModes.Overflow;
        }
    }

    private void ApplyNarrativeLowerThirdLayout(
        Vector2 rootSize,
        bool terminal)
    {
        Rect frame = terminal
            ? new Rect(300f, 768f, 1320f, 212f)
            : new Rect(350f, 814f, 1220f, 132f);
        Rect backdrop = terminal
            ? new Rect(306f, 774f, 1308f, 200f)
            : new Rect(350f, 814f, 1220f, 132f);
        Rect speaker = new Rect(344f, 794f, 1232f, 34f);
        Rect body = terminal
            ? new Rect(344f, 838f, 1232f, 94f)
            : new Rect(390f, 836f, 1140f, 86f);
        Rect hint = new Rect(1320f, 936f, 256f, 24f);

        ApplyTopLeftRect(
            backdropImage != null ? backdropImage.rectTransform : null,
            backdrop,
            rootSize);
        ApplyTopLeftRect(
            auxiliaryFrameImage != null
                ? auxiliaryFrameImage.rectTransform
                : null,
            frame,
            rootSize);
        ApplyTopLeftRect(
            speakerText != null ? speakerText.rectTransform : null,
            speaker,
            rootSize);
        ApplyTopLeftRect(
            bodyText != null ? bodyText.rectTransform : null,
            body,
            rootSize);
        ApplyTopLeftRect(
            advanceHintText != null
                ? advanceHintText.rectTransform
                : null,
            hint,
            rootSize);

        if (accentRuleImage != null)
        {
            accentRuleImage.gameObject.SetActive(false);
        }

        if (speakerText != null)
        {
            speakerText.alignment = TextAlignmentOptions.Left;
            speakerText.maxVisibleLines = 1;
        }

        if (bodyText != null)
        {
            bodyText.alignment = terminal
                ? TextAlignmentOptions.TopLeft
                : TextAlignmentOptions.Center;
            bodyText.maxVisibleLines = terminal ? 4 : 3;
            bodyText.overflowMode = TextOverflowModes.Overflow;
        }
    }

    private static Image FindImage(Transform root, string objectName)
    {
        if (root == null)
        {
            return null;
        }

        Transform found = root.Find(objectName);
        return found != null ? found.GetComponent<Image>() : null;
    }

    private static void ConfigureFrameVisibility(
        Image image,
        bool visible,
        Color color)
    {
        if (image == null)
        {
            return;
        }

        image.color = color;
        image.raycastTarget = false;
        image.gameObject.SetActive(visible);
    }

    private static void ApplyTopLeftRect(
        RectTransform rect,
        Rect referenceRect,
        Vector2 rootSize)
    {
        ApplyTopLeftRect(
            rect,
            referenceRect,
            rootSize,
            Vector4.zero);
    }

    private static void ApplyTopLeftRect(
        RectTransform rect,
        Rect referenceRect,
        Vector2 rootSize,
        Vector4 padding)
    {
        if (rect == null)
        {
            return;
        }

        float scaleX = Mathf.Max(1f, rootSize.x) / 1920f;
        float scaleY = Mathf.Max(1f, rootSize.y) / 1080f;
        float x = (referenceRect.x + padding.x) * scaleX;
        float y = (referenceRect.y + padding.y) * scaleY;
        float width = Mathf.Max(
            1f,
            (referenceRect.width - padding.x - padding.z) * scaleX);
        float height = Mathf.Max(
            1f,
            (referenceRect.height - padding.y - padding.w) * scaleY);

        rect.anchorMin = Vector2.up;
        rect.anchorMax = Vector2.up;
        rect.pivot = Vector2.up;
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void SetBottomAnchoredRect(
        RectTransform rect,
        float xMin,
        float xMax,
        float bottom,
        float height,
        Vector2 anchoredOffset,
        float extraWidth = 0f)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(xMin, bottom);
        rect.anchorMax = new Vector2(xMax, bottom);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = anchoredOffset;
        rect.sizeDelta = new Vector2(extraWidth, Mathf.Max(1f, height));
    }

    private GameObject GetVisualRoot()
    {
        return visualRoot != null ? visualRoot : subtitlePanel;
    }

    private RectTransform GetLayoutRoot()
    {
        if (layoutRoot == null && GetVisualRoot() != null)
        {
            layoutRoot = GetVisualRoot().GetComponent<RectTransform>();
        }

        return layoutRoot;
    }

    private static Vector2 ResolveRootSize(RectTransform rootRect)
    {
        if (rootRect != null && rootRect.rect.width > 1f && rootRect.rect.height > 1f)
        {
            return rootRect.rect.size;
        }

        return new Vector2(
            Mathf.Max(1f, Screen.width > 0 ? Screen.width : 1920),
            Mathf.Max(1f, Screen.height > 0 ? Screen.height : 1080));
    }

    private HearthSubtitleContext ResolveContext(string speaker)
    {
        if (!inferContextFromSpeaker)
        {
            return defaultContext;
        }

        string normalized = (speaker ?? string.Empty).Trim().ToUpperInvariant();
        if (normalized.Contains("TERMINAL") ||
            normalized.Contains("ARCHIVE") ||
            normalized.Contains("DOORWAY") ||
            normalized.Contains("SYNTH VOICE"))
        {
            return HearthSubtitleContext.Terminal;
        }

        if (normalized.Contains("FIELD UNIT") ||
            normalized == "FIELD COMPANION UNIT")
        {
            return HearthSubtitleContext.FieldUnit;
        }

        return defaultContext;
    }

    private void EnsureSubtitleCanvasSorting()
    {
        GameObject root = GetVisualRoot();
        if (!forceSubtitleCanvasSorting || root == null)
        {
            return;
        }

        Canvas subtitleCanvas = null;
        Canvas[] parentCanvases = root.GetComponentsInParent<Canvas>(true);
        for (int i = 0; i < parentCanvases.Length; i++)
        {
            if (parentCanvases[i] != null && parentCanvases[i].isRootCanvas)
            {
                subtitleCanvas = parentCanvases[i];
                break;
            }
        }

        if (subtitleCanvas == null)
        {
            Canvas[] childCanvases = GetComponentsInChildren<Canvas>(true);
            for (int i = 0; i < childCanvases.Length; i++)
            {
                if (childCanvases[i] != null && childCanvases[i].isRootCanvas)
                {
                    subtitleCanvas = childCanvases[i];
                    break;
                }
            }
        }

        if (subtitleCanvas == null)
        {
            subtitleCanvas = root.GetComponent<Canvas>();
            if (subtitleCanvas == null)
            {
                subtitleCanvas = root.AddComponent<Canvas>();
            }
        }

        Canvas nestedCanvas = root.GetComponent<Canvas>();
        if (nestedCanvas != null && nestedCanvas != subtitleCanvas)
        {
            nestedCanvas.overrideSorting = false;
        }

        subtitleCanvas.sortingOrder = subtitleSortingOrder;
    }
}
