using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinLoopSubtitlePlayer : MonoBehaviour
{
    [Header("Shared Presentation")]
    [SerializeField] private HearthSubtitleStyleProfile styleProfile;
    [SerializeField] private HearthSubtitlePresentationMode presentationMode = HearthSubtitlePresentationMode.StandardDialogue;

    [Header("UI")]
    [SerializeField] private GameObject subtitlePanel;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text bodyText;
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

    private Coroutine activeRoutine;

    public bool IsPlaying { get; private set; }

    public HearthSubtitleStyleProfile StyleProfile
    {
        get { return styleProfile; }
    }

    public HearthSubtitlePresentationMode PresentationMode
    {
        get { return presentationMode; }
    }

    private void Awake()
    {
        EnsureReferences();
        HideImmediate();
    }

    private void OnValidate()
    {
        defaultHoldSeconds = Mathf.Max(0f, defaultHoldSeconds);
        cleanSpeakerFontSize = Mathf.Max(1f, cleanSpeakerFontSize);
        cleanBodyFontSize = Mathf.Max(1f, cleanBodyFontSize);
    }

    public Coroutine PlaySequence(IList<MinLoopSubtitleLine> lines)
    {
        Stop();
        activeRoutine = StartCoroutine(PlaySequenceRoutine(lines));
        return activeRoutine;
    }

    public Coroutine PlaySequence(HearthDialogueSequence sequence)
    {
        Stop();
        activeRoutine = StartCoroutine(PlaySequenceRoutine(sequence));
        return activeRoutine;
    }

    public IEnumerator PlayLines(IList<MinLoopSubtitleLine> lines)
    {
        Stop();
        yield return PlaySequenceRoutine(lines);
    }

    public IEnumerator PlaySequenceAsset(HearthDialogueSequence sequence)
    {
        Stop();
        yield return PlaySequenceRoutine(sequence);
    }

    public void ShowLine(string speaker, string text)
    {
        ShowLine(speaker, text, presentationMode);
    }

    private void ShowLine(string speaker, string text, HearthSubtitlePresentationMode mode)
    {
        EnsureReferences();

        if (speakerText != null)
        {
            speakerText.text = speaker;
            speakerText.gameObject.SetActive(mode != HearthSubtitlePresentationMode.TimeCard);
        }

        if (bodyText != null)
        {
            bodyText.text = text;
        }

        if (subtitlePanel != null)
        {
            subtitlePanel.SetActive(true);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }

        ApplyConfiguredStyle(mode);
    }

    public void SetPresentation(HearthSubtitleStyleProfile profile, HearthSubtitlePresentationMode mode)
    {
        styleProfile = profile;
        presentationMode = mode;
        ApplyConfiguredStyle();
    }

    public void Hide()
    {
        Stop();
        HideImmediate();
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
    }

    private IEnumerator PlaySequenceRoutine(IList<MinLoopSubtitleLine> lines)
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
            ShowLine(line.speaker, line.text, linePresentation);
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
            else if (holdSeconds > 0f)
            {
                yield return Wait(holdSeconds);
            }
        }

        HideImmediate();
        IsPlaying = false;
        activeRoutine = null;
    }

    private IEnumerator PlaySequenceRoutine(HearthDialogueSequence sequence)
    {
        if (sequence == null || !sequence.HasLines)
        {
            yield return PlaySequenceRoutine((IList<MinLoopSubtitleLine>)null);
            yield break;
        }

        IList<MinLoopSubtitleLine> sequenceLines = sequence.Lines as IList<MinLoopSubtitleLine>;
        if (sequenceLines == null)
        {
            sequenceLines = new List<MinLoopSubtitleLine>(sequence.Lines);
        }

        yield return PlaySequenceRoutine(sequenceLines);

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
            if (canvasGroup != null) canvasGroup.alpha = to;
            yield break;
        }

        canvasGroup.alpha = from;
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / seconds));
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    private void HideImmediate()
    {
        if (subtitlePanel != null)
        {
            subtitlePanel.SetActive(false);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }

    private void EnsureReferences()
    {
        EnsureDialogueChannelSource();

        if (subtitlePanel != null && speakerText != null && bodyText != null)
        {
            if (canvasGroup == null)
            {
                canvasGroup = subtitlePanel.GetComponent<CanvasGroup>();
            }

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
        }

        if (subtitlePanel == null)
        {
            GameObject panelObject = new GameObject("Min Loop Subtitle Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            panelObject.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.08f, 0.04f);
            panelRect.anchorMax = new Vector2(0.92f, 0.24f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = useCleanCenteredStyle ? Color.clear : new Color(0f, 0f, 0f, 0.72f);
            panelImage.raycastTarget = false;

            canvasGroup = panelObject.GetComponent<CanvasGroup>();
            subtitlePanel = panelObject;
        }

        if (speakerText == null)
        {
            speakerText = CreateText(subtitlePanel.transform, "Speaker", new Vector2(0.04f, 0.64f), new Vector2(0.96f, 0.92f), 26f, FontStyles.Bold);
        }

        if (bodyText == null)
        {
            bodyText = CreateText(subtitlePanel.transform, "Line", new Vector2(0.04f, 0.16f), new Vector2(0.96f, 0.64f), 24f, FontStyles.Normal);
        }
    }

    private TMP_Text CreateText(Transform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax, float fontSize, FontStyles style)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.color = Color.white;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = useCleanCenteredStyle ? TextAlignmentOptions.Center : TextAlignmentOptions.Left;
        text.enableWordWrapping = true;
        return text;
    }

    private void ApplyConfiguredStyle()
    {
        ApplyConfiguredStyle(presentationMode);
    }

    private void ApplyConfiguredStyle(HearthSubtitlePresentationMode mode)
    {
        if (!useCleanCenteredStyle || subtitlePanel == null)
        {
            return;
        }

        RectTransform panelRect = subtitlePanel.GetComponent<RectTransform>();
        if (panelRect != null)
        {
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
        }

        Image panelImage = subtitlePanel.GetComponent<Image>();
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
        float width = sharedLayout != null ? sharedLayout.widthFraction : subtitleWidthFraction;
        float speakerY = sharedLayout != null ? sharedLayout.speakerCenterY : speakerCenterY;
        float speakerHeight = sharedLayout != null ? sharedLayout.speakerHeightFraction : speakerHeightFraction;
        float bodyY = sharedLayout != null ? sharedLayout.bodyCenterY : bodyCenterY;
        float bodyHeight = sharedLayout != null ? sharedLayout.bodyHeightFraction : bodyHeightFraction;
        float speakerSize = sharedLayout != null ? sharedLayout.speakerFontSize : cleanSpeakerFontSize;
        float bodySize = sharedLayout != null ? sharedLayout.bodyFontSize : cleanBodyFontSize;
        float speakerMinSize = sharedLayout != null ? sharedLayout.speakerMinimumFontSize : Mathf.Max(12f, speakerSize * 0.72f);
        float bodyMinSize = sharedLayout != null ? sharedLayout.bodyMinimumFontSize : Mathf.Max(14f, bodySize * 0.64f);
        int bodyMaxLines = sharedLayout != null ? sharedLayout.bodyMaximumLines : 5;
        float lineSpacing = sharedLayout != null ? sharedLayout.lineSpacing : 0f;
        Color textColor = styleProfile != null ? styleProfile.TextColor : cleanTextColor;

        float halfWidth = Mathf.Clamp01(width) * 0.5f;
        ApplyTextStyle(
            speakerText,
            MakeCenteredAnchor(speakerY, speakerHeight, halfWidth),
            speakerSize,
            speakerMinSize,
            1,
            0f,
            textColor,
            FontStyles.Bold);
        ApplyTextStyle(
            bodyText,
            MakeCenteredAnchor(bodyY, bodyHeight, halfWidth),
            bodySize,
            bodyMinSize,
            bodyMaxLines,
            lineSpacing,
            textColor,
            FontStyles.Normal);
    }

    private void ApplyTextStyle(
        TMP_Text text,
        Rect anchorRect,
        float fontSize,
        float minimumFontSize,
        int maximumLines,
        float lineSpacing,
        Color color,
        FontStyles style)
    {
        if (text == null)
        {
            return;
        }

        RectTransform rect = text.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(anchorRect.xMin, anchorRect.yMin);
            rect.anchorMax = new Vector2(anchorRect.xMax, anchorRect.yMax);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        text.color = color;
        text.fontSize = fontSize;
        text.fontSizeMax = fontSize;
        text.fontSizeMin = Mathf.Min(minimumFontSize, fontSize);
        text.enableAutoSizing = true;
        text.fontStyle = style;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.maxVisibleLines = Mathf.Max(1, maximumLines);
        text.lineSpacing = lineSpacing;
        text.overflowMode = TextOverflowModes.Truncate;
    }

    private Rect MakeCenteredAnchor(float centerY, float height, float halfWidth)
    {
        centerY = Mathf.Clamp01(centerY);
        height = Mathf.Clamp01(height);
        halfWidth = Mathf.Clamp01(halfWidth);
        return Rect.MinMaxRect(
            0.5f - halfWidth,
            Mathf.Clamp01(centerY - height * 0.5f),
            0.5f + halfWidth,
            Mathf.Clamp01(centerY + height * 0.5f));
    }

    private void EnsureSubtitleCanvasSorting()
    {
        if (!forceSubtitleCanvasSorting || subtitlePanel == null)
        {
            return;
        }

        Canvas subtitleCanvas = null;
        Canvas[] parentCanvases = subtitlePanel.GetComponentsInParent<Canvas>(true);
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
            subtitleCanvas = subtitlePanel.GetComponent<Canvas>();
            if (subtitleCanvas == null)
            {
                subtitleCanvas = subtitlePanel.AddComponent<Canvas>();
            }
        }

        Canvas nestedCanvas = subtitlePanel.GetComponent<Canvas>();
        if (nestedCanvas != null && nestedCanvas != subtitleCanvas)
        {
            nestedCanvas.overrideSorting = false;
        }

        subtitleCanvas.sortingOrder = subtitleSortingOrder;
    }
}
