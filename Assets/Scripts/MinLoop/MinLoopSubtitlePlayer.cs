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
    [Header("Shared Presentation")]
    [SerializeField] private HearthSubtitleStyleProfile styleProfile;
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
    [SerializeField] private bool createFallbackUI = true;

    [Header("Canvas Sorting")]
    [SerializeField] private bool forceSubtitleCanvasSorting = true;
    [SerializeField] private int subtitleSortingOrder = 7600;

    [Header("Clean Centered Style")]
    [SerializeField] private bool useCleanCenteredStyle = true;
    [SerializeField, Range(0.35f, 0.95f)] private float subtitleWidthFraction = 0.66f;
    [SerializeField, Range(0.12f, 0.85f)] private float speakerCenterY = 0.31f;
    [SerializeField, Range(0.03f, 0.16f)] private float speakerHeightFraction = 0.06f;
    [SerializeField, Range(0.08f, 0.8f)] private float bodyCenterY = 0.22f;
    [SerializeField, Range(0.06f, 0.28f)] private float bodyHeightFraction = 0.12f;
    [SerializeField] private Color cleanTextColor = Color.white;
    [SerializeField] private float cleanSpeakerFontSize = 22f;
    [SerializeField] private float cleanBodyFontSize = 28f;

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

    public bool IsPlaying { get; private set; }

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
        ReleaseDialogueControlLock();
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
        bool showVisual)
    {
        EnsureReferences();
        activePresentationMode = mode;
        activeContext = context;
        activeChannel = channel;
        activeSpeakerSide = side;
        activeAdvancePolicy = advancePolicy;

        if (speakerText != null)
        {
            speakerText.text = speaker;
            speakerText.gameObject.SetActive(
                showVisual &&
                mode != HearthSubtitlePresentationMode.TimeCard &&
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

    public void Stop()
    {
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
        ReleaseDialogueControlLock();
        HideImmediate();
    }

    private IEnumerator PlaySequenceRoutine(
        IList<MinLoopSubtitleLine> lines,
        HearthSubtitleContext? explicitContext = null,
        DialogueChannel? sequenceChannel = null,
        SpeakerSide sequenceSpeakerSide = SpeakerSide.Auto,
        AdvancePolicy sequenceAdvancePolicy = AdvancePolicy.ManualSpace)
    {
        IsPlaying = true;

        if (lines == null || lines.Count == 0)
        {
            HideImmediate();
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
            }

            HearthSubtitlePresentationMode linePresentation = line.presentationKind == HearthSubtitleLinePresentationKind.TimeCard
                ? HearthSubtitlePresentationMode.TimeCard
                : presentationMode;
            HearthSubtitleContext lineContext = explicitContext.HasValue
                ? explicitContext.Value
                : ResolveContext(line.speaker);
            DialogueChannel lineChannel = ResolveLineChannel(
                line,
                sequenceChannel,
                lineContext);
            SpeakerSide lineSpeakerSide = ResolveLineSpeakerSide(
                line,
                sequenceSpeakerSide);
            AdvancePolicy lineAdvancePolicy = ResolveLineAdvancePolicy(
                line,
                sequenceAdvancePolicy);
            bool audioOnly =
                line.dialogueMode == HearthDialogueLineMode.AudioOnly ||
                (lineChannel == DialogueChannel.Auxiliary &&
                 IsMiaSpeaker(line.speaker));

            ApplyDialogueControlLock(lineChannel);
            ShowLine(
                line.speaker,
                line.text,
                linePresentation,
                lineContext,
                lineChannel,
                audioOnly ? SpeakerSide.None : lineSpeakerSide,
                audioOnly ? AdvancePolicy.AudioComplete : lineAdvancePolicy,
                !audioOnly);
            PlayVoice(line.voiceClip);

            float holdSeconds = ResolveLineDuration(line);
            if (linePresentation == HearthSubtitlePresentationMode.TimeCard)
            {
                float fadeSeconds = styleProfile != null ? styleProfile.TimeCardFadeSeconds : 0.35f;
                yield return FadeCanvas(0f, 1f, fadeSeconds);
                if (holdSeconds > 0f)
                {
                    yield return Wait(holdSeconds);
                }
                yield return FadeCanvas(1f, 0f, fadeSeconds);
            }
            else if (!audioOnly &&
                     lineAdvancePolicy == AdvancePolicy.ManualSpace)
            {
                yield return WaitForManualAdvance();
            }
            else if (holdSeconds > 0f)
            {
                yield return Wait(holdSeconds);
            }
        }

        HideImmediate();
        IsPlaying = false;
        activeRoutine = null;
        ReleaseDialogueControlLock();
    }

    private IEnumerator PlaySequenceRoutine(
        HearthDialogueSequence sequence,
        HearthSubtitleContext? explicitContext = null)
    {
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
            sequence.AdvancePolicy);

        if (sequence.PostSequenceDelay > 0f)
        {
            yield return Wait(sequence.PostSequenceDelay);
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

    private IEnumerator WaitForManualAdvance()
    {
        while (Input.GetKey(manualAdvanceKey))
        {
            yield return null;
        }

        while (!Input.GetKeyDown(manualAdvanceKey))
        {
            yield return null;
        }

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
            if (sequence.DialogueChannel == DialogueChannel.Auxiliary)
            {
                return DialogueChannel.Auxiliary;
            }

            IReadOnlyList<MinLoopSubtitleLine> sequenceLines = sequence.Lines;
            for (int i = 0; i < sequenceLines.Count; i++)
            {
                MinLoopSubtitleLine candidate = sequenceLines[i];
                if (candidate != null &&
                    (candidate.dialogueMode == HearthDialogueLineMode.Auxiliary ||
                     IsFieldUnitSpeaker(candidate.speaker)))
                {
                    return DialogueChannel.Auxiliary;
                }
            }
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

        if (sequenceChannel.HasValue)
        {
            return sequenceChannel.Value;
        }

        return ResolveChannel(null, context, line.speaker);
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
        MinLoopSubtitleLine line,
        AdvancePolicy sequenceAdvancePolicy)
    {
        switch (line.advancePolicy)
        {
            case HearthDialogueLineAdvancePolicy.ManualSpace:
                return AdvancePolicy.ManualSpace;
            case HearthDialogueLineAdvancePolicy.AudioComplete:
                return AdvancePolicy.AudioComplete;
            default:
                return sequenceAdvancePolicy;
        }
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

    private static bool IsFieldUnitSpeaker(string speaker)
    {
        return NormalizeSpeaker(speaker).Contains("FIELD UNIT");
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

    private void HideImmediate()
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
            return;
        }

        if (createFallbackUI)
        {
            CreateFallbackUI();
        }

        ApplyConfiguredStyle();
        EnsureSubtitleCanvasSorting();
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
            : (mode == HearthSubtitlePresentationMode.TimeCard ? 1f : 22f);
        float bodySize = styleProfile != null
            ? styleProfile.GetBodyFontSize(mode)
            : (mode == HearthSubtitlePresentationMode.CenteredEpilogue ? 30f : cleanBodyFontSize);
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
            backdropImage.color = mode == HearthSubtitlePresentationMode.TimeCard
                ? Color.clear
                : (contextStyle != null ? contextStyle.backgroundColor : new Color(0.035f, 0.055f, 0.08f, 0.62f));
            backdropImage.raycastTarget = false;
        }

        if (accentRuleImage != null)
        {
            accentRuleImage.color = mode == HearthSubtitlePresentationMode.TimeCard
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
            auxiliary,
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
                standardDialogue &&
                activeAdvancePolicy == AdvancePolicy.ManualSpace;
            advanceHintText.text = "SPACE  CONTINUE";
            advanceHintText.gameObject.SetActive(showHint);
            ApplyTextStyle(
                advanceHintText,
                auxiliary ? 14f : 15f,
                0f,
                frameColor,
                FontStyles.Bold);
            advanceHintText.alignment = TextAlignmentOptions.Right;
        }

        activePresentationMode = mode;
        adaptiveLayoutDirty = true;
        ApplyAdaptiveLayout(mode, activeContext);
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
            ? new Rect(1216f, 214f, 640f, 180f)
            : new Rect(432f, 670f, 960f, 256f);
        Rect body = auxiliary
            ? new Rect(1244f, 278f, 560f, 76f)
            : new Rect(472f, 714f, 880f, 150f);
        const float formalSpeakerWidth = 340f;
        Rect speaker = auxiliary
            ? new Rect(1244f, 238f, 560f, 30f)
            : new Rect(
                activeSpeakerSide == SpeakerSide.Right
                    ? backdrop.xMax - formalSpeakerWidth
                    : backdrop.x,
                622f,
                formalSpeakerWidth,
                48f);
        Rect advanceHint = auxiliary
            ? new Rect(1608f, 358f, 196f, 22f)
            : new Rect(1128f, 884f, 224f, 24f);
        float accentX = !auxiliary &&
                        activeSpeakerSide == SpeakerSide.Right
            ? backdrop.xMax - 2f
            : backdrop.x;
        Rect accent = new Rect(accentX, backdrop.y, 2f, backdrop.height);

        ApplyTopLeftRect(
            backdropImage != null ? backdropImage.rectTransform : null,
            backdrop,
            rootSize);
        ApplyTopLeftRect(
            accentRuleImage != null ? accentRuleImage.rectTransform : null,
            accent,
            rootSize);
        ApplyTopLeftRect(
            speakerTabImage != null ? speakerTabImage.rectTransform : null,
            speaker,
            rootSize);
        ApplyTopLeftRect(
            formalFrameImage != null ? formalFrameImage.rectTransform : null,
            new Rect(432f, 670f, 960f, 256f),
            rootSize);
        ApplyTopLeftRect(
            auxiliaryFrameImage != null
                ? auxiliaryFrameImage.rectTransform
                : null,
            new Rect(1216f, 214f, 640f, 180f),
            rootSize);
        ApplyTopLeftRect(
            leftSpeakerTabImage != null
                ? leftSpeakerTabImage.rectTransform
                : null,
            new Rect(432f, 622f, formalSpeakerWidth, 48f),
            rootSize);
        ApplyTopLeftRect(
            rightSpeakerTabImage != null
                ? rightSpeakerTabImage.rectTransform
                : null,
            new Rect(
                1392f - formalSpeakerWidth,
                622f,
                formalSpeakerWidth,
                48f),
            rootSize);
        ApplyTopLeftRect(
            speakerText != null ? speakerText.rectTransform : null,
            speaker,
            rootSize,
            new Vector4(16f, 4f, 16f, 4f));
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
