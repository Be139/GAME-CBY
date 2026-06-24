using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinLoopSubtitlePlayer : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject subtitlePanel;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private bool createFallbackUI = true;

    [Header("Timing")]
    [SerializeField] private float defaultHoldSeconds = 2.75f;
    [SerializeField] private bool useUnscaledTime;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    private Coroutine activeRoutine;

    public bool IsPlaying { get; private set; }

    private void Awake()
    {
        EnsureReferences();
        HideImmediate();
    }

    private void OnValidate()
    {
        defaultHoldSeconds = Mathf.Max(0f, defaultHoldSeconds);
    }

    public Coroutine PlaySequence(IList<MinLoopSubtitleLine> lines)
    {
        Stop();
        activeRoutine = StartCoroutine(PlaySequenceRoutine(lines));
        return activeRoutine;
    }

    public IEnumerator PlayLines(IList<MinLoopSubtitleLine> lines)
    {
        Stop();
        yield return PlaySequenceRoutine(lines);
    }

    public void ShowLine(string speaker, string text)
    {
        EnsureReferences();

        if (speakerText != null)
        {
            speakerText.text = speaker;
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

            ShowLine(line.speaker, line.text);
            PlayVoice(line.voiceClip);

            float holdSeconds = line.holdSeconds > 0f ? line.holdSeconds : defaultHoldSeconds;
            if (line.voiceClip != null)
            {
                holdSeconds = Mathf.Max(holdSeconds, line.voiceClip.length);
            }

            if (holdSeconds > 0f)
            {
                yield return Wait(holdSeconds);
            }
        }

        HideImmediate();
        IsPlaying = false;
        activeRoutine = null;
    }

    private void PlayVoice(AudioClip clip)
    {
        if (clip == null || audioSource == null)
        {
            return;
        }

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
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
        if (subtitlePanel != null && speakerText != null && bodyText != null)
        {
            if (canvasGroup == null)
            {
                canvasGroup = subtitlePanel.GetComponent<CanvasGroup>();
            }

            return;
        }

        if (createFallbackUI)
        {
            CreateFallbackUI();
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
            panelImage.color = new Color(0f, 0f, 0f, 0.72f);

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
        text.alignment = TextAlignmentOptions.Left;
        text.enableWordWrapping = true;
        return text;
    }
}
