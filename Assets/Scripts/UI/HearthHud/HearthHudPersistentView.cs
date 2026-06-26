using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HearthHudPersistentView : MonoBehaviour
{
    [Header("Status")]
    [SerializeField] private GameObject root;
    [SerializeField] private Image workerBadgeImage;
    [SerializeField] private Image statusDotImage;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text workerNameText;
    [SerializeField] private TMP_Text clockText;
    [SerializeField] private GameObject taskRoot;
    [SerializeField] private TMP_Text taskText;

    [Header("Subtitle")]
    [SerializeField] private GameObject subtitleRoot;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private CanvasGroup subtitleCanvasGroup;

    [Header("Trust Delta")]
    [SerializeField] private GameObject trustRoot;
    [SerializeField] private TMP_Text trustDeltaText;
    [SerializeField] private CanvasGroup trustCanvasGroup;

    [Header("Palette")]
    [SerializeField] private Color activeColor = new Color(0.08f, 0.94f, 0.54f, 1f);
    [SerializeField] private Color dormantColor = new Color(0.58f, 0.62f, 0.62f, 1f);
    [SerializeField] private Color alertColor = new Color(1f, 0.18f, 0.13f, 1f);
    [SerializeField] private Color warningColor = new Color(1f, 0.52f, 0.14f, 1f);
    [SerializeField] private float pulseSpeed = 3.2f;
    [SerializeField] private float pulseAmount = 0.28f;
    [SerializeField] private float trustFlashSeconds = 1.6f;

    private HearthHudState currentState = HearthHudState.Active;
    private Color currentAccent;
    private Coroutine subtitleRoutine;
    private Coroutine trustRoutine;

    private void Awake()
    {
        if (root == null)
        {
            root = gameObject;
        }

        currentAccent = activeColor;
        ApplyState(currentState);
        SetTrustVisible(false, true);
    }

    private void Update()
    {
        if (statusDotImage == null || currentState == HearthHudState.Dormant)
        {
            return;
        }

        float pulse = 1f - pulseAmount + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseAmount;
        Color color = currentAccent;
        color.a = Mathf.Clamp01(0.72f + pulse * 0.28f);
        statusDotImage.color = color;
        statusDotImage.transform.localScale = Vector3.one * Mathf.Clamp(pulse, 0.72f, 1.2f);
    }

    public void ConfigureBindings(
        GameObject newRoot,
        Image newWorkerBadgeImage,
        Image newStatusDotImage,
        TMP_Text newStatusText,
        TMP_Text newWorkerNameText,
        TMP_Text newClockText,
        GameObject newTaskRoot,
        TMP_Text newTaskText,
        GameObject newSubtitleRoot,
        TMP_Text newSubtitleText,
        CanvasGroup newSubtitleCanvasGroup,
        GameObject newTrustRoot,
        TMP_Text newTrustDeltaText,
        CanvasGroup newTrustCanvasGroup)
    {
        root = newRoot;
        workerBadgeImage = newWorkerBadgeImage;
        statusDotImage = newStatusDotImage;
        statusText = newStatusText;
        workerNameText = newWorkerNameText;
        clockText = newClockText;
        taskRoot = newTaskRoot;
        taskText = newTaskText;
        subtitleRoot = newSubtitleRoot;
        subtitleText = newSubtitleText;
        subtitleCanvasGroup = newSubtitleCanvasGroup;
        trustRoot = newTrustRoot;
        trustDeltaText = newTrustDeltaText;
        trustCanvasGroup = newTrustCanvasGroup;
    }

    public void SetVisible(bool visible)
    {
        if (root != null)
        {
            root.SetActive(visible);
        }
        else
        {
            gameObject.SetActive(visible);
        }
    }

    public void SetHudState(HearthHudState state)
    {
        currentState = state;
        ApplyState(state);
    }

    public void SetClock(string value)
    {
        if (clockText != null)
        {
            clockText.text = string.IsNullOrEmpty(value) ? "--:--" : value;
        }
    }

    public void SetTask(bool visible, string value)
    {
        if (taskRoot != null)
        {
            taskRoot.SetActive(visible);
        }

        if (taskText != null)
        {
            taskText.text = value ?? string.Empty;
        }
    }

    public void SetSubtitle(string value)
    {
        bool visible = !string.IsNullOrWhiteSpace(value);

        if (subtitleText != null)
        {
            subtitleText.text = value ?? string.Empty;
        }

        if (subtitleRoutine != null)
        {
            StopCoroutine(subtitleRoutine);
            subtitleRoutine = null;
        }

        if (subtitleCanvasGroup == null)
        {
            if (subtitleRoot != null)
            {
                subtitleRoot.SetActive(visible);
            }

            return;
        }

        if (!isActiveAndEnabled)
        {
            subtitleCanvasGroup.alpha = visible ? 1f : 0f;
            if (subtitleRoot != null)
            {
                subtitleRoot.SetActive(visible);
            }

            return;
        }

        subtitleRoutine = StartCoroutine(FadeCanvasGroup(subtitleCanvasGroup, visible ? 1f : 0f, 0.18f, subtitleRoot));
    }

    public void ShowTrustDelta(int delta)
    {
        string prefix = delta >= 0 ? "+" : string.Empty;
        ShowTrustDelta(prefix + delta.ToString());
    }

    public void ShowTrustDelta(string label)
    {
        if (trustRoot == null || trustDeltaText == null)
        {
            return;
        }

        if (trustRoutine != null)
        {
            StopCoroutine(trustRoutine);
        }

        trustRoutine = StartCoroutine(TrustFlash(label));
    }

    private void ApplyState(HearthHudState state)
    {
        string label;

        switch (state)
        {
            case HearthHudState.Alert:
                currentAccent = alertColor;
                label = "COMPANION UNIT | ALERT";
                break;
            case HearthHudState.Dormant:
                currentAccent = dormantColor;
                label = "COMPANION UNIT | DORMANT";
                break;
            case HearthHudState.WarningOrange:
                currentAccent = warningColor;
                label = "SYSTEM WARNING";
                break;
            case HearthHudState.WarningDeepOrange:
                currentAccent = new Color(1f, 0.31f, 0.08f, 1f);
                label = "SYSTEM WARNING";
                break;
            default:
                currentAccent = activeColor;
                label = "COMPANION UNIT | ACTIVE";
                break;
        }

        if (statusDotImage != null)
        {
            statusDotImage.color = currentAccent;
            statusDotImage.transform.localScale = Vector3.one;
        }

        if (statusText != null)
        {
            statusText.text = label;
            statusText.color = Color.Lerp(Color.white, currentAccent, 0.45f);
        }

        if (workerNameText != null)
        {
            workerNameText.color = state == HearthHudState.Dormant
                ? new Color(0.72f, 0.75f, 0.75f, 0.94f)
                : new Color(0.9f, 1f, 0.96f, 0.96f);
        }

        if (workerBadgeImage != null)
        {
            Color badgeColor = Color.white;
            badgeColor.a = state == HearthHudState.Dormant ? 0.45f : 0.9f;
            workerBadgeImage.color = badgeColor;
        }
    }

    private IEnumerator TrustFlash(string label)
    {
        trustDeltaText.text = label;
        trustDeltaText.color = label.StartsWith("-", System.StringComparison.Ordinal)
            ? new Color(1f, 0.35f, 0.26f, 1f)
            : new Color(0.3f, 1f, 0.62f, 1f);

        SetTrustVisible(true, true);
        float elapsed = 0f;

        while (elapsed < trustFlashSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / trustFlashSeconds);
            if (trustCanvasGroup != null)
            {
                trustCanvasGroup.alpha = t < 0.72f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.72f) / 0.28f);
            }

            yield return null;
        }

        SetTrustVisible(false, true);
        trustRoutine = null;
    }

    private void SetTrustVisible(bool visible, bool immediate)
    {
        if (trustRoot != null)
        {
            trustRoot.SetActive(visible);
        }

        if (trustCanvasGroup != null && immediate)
        {
            trustCanvasGroup.alpha = visible ? 1f : 0f;
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float targetAlpha, float seconds, GameObject targetRoot)
    {
        if (group == null)
        {
            yield break;
        }

        if (targetRoot != null && targetAlpha > 0f)
        {
            targetRoot.SetActive(true);
        }

        float startAlpha = group.alpha;
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / seconds);
            group.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        group.alpha = targetAlpha;
        if (targetRoot != null && targetAlpha <= 0f)
        {
            targetRoot.SetActive(false);
        }

        subtitleRoutine = null;
    }
}
