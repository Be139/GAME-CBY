using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HearthCompanionSpecialEffectsView : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private CanvasGroup overlayGroup;
    [SerializeField] private Image overlayImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Image pulseImage;

    [Header("Timing")]
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private float fadeSeconds = 0.2f;

    [Header("Centered Layout")]
    [SerializeField] private bool enforceHorizontalCenter = true;
    [SerializeField] private float referenceCanvasWidth = 1920f;

    private Coroutine activeRoutine;

    private void Awake()
    {
        ApplyCenteredLayout();
    }

    private void OnEnable()
    {
        ApplyCenteredLayout();
    }

    private void OnValidate()
    {
        fadeSeconds = Mathf.Max(0.01f, fadeSeconds);
        referenceCanvasWidth = Mathf.Max(1f, referenceCanvasWidth);
        ApplyCenteredLayout();
    }

    public void Configure(
        CanvasGroup newOverlayGroup,
        Image newOverlayImage,
        TMP_Text newTitleText,
        TMP_Text newBodyText,
        TMP_Text newStatusText,
        Image newPulseImage)
    {
        overlayGroup = newOverlayGroup;
        overlayImage = newOverlayImage;
        titleText = newTitleText;
        bodyText = newBodyText;
        statusText = newStatusText;
        pulseImage = newPulseImage;
        ApplyCenteredLayout();
        HideImmediate();
    }

    public void ApplyLayoutNow()
    {
        ApplyCenteredLayout();
    }

    public void Apply(HearthCompanionHudSceneData scene)
    {
        if (scene == null)
        {
            HideImmediate();
            return;
        }

        switch (scene.SpecialEffect)
        {
            case HearthCompanionSpecialEffect.ShutdownGlitch:
                PlayShutdownGlitch(scene.SpecialTitle, scene.SpecialBody, scene.SpecialStatusLabel, scene.SpecialDuration);
                break;
            case HearthCompanionSpecialEffect.BlackAudio:
                ShowBlackAudio(scene.SpecialTitle, scene.SpecialBody, scene.SpecialStatusLabel);
                break;
            case HearthCompanionSpecialEffect.DeepSleep:
                PlayDeepSleep(scene.SpecialTitle, scene.SpecialBody, scene.SpecialStatusLabel, scene.SpecialDuration);
                break;
            default:
                HideImmediate();
                break;
        }
    }

    public void HideImmediate()
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }

        if (overlayGroup != null)
        {
            overlayGroup.alpha = 0f;
            overlayGroup.blocksRaycasts = false;
            overlayGroup.interactable = false;
        }

        gameObject.SetActive(false);
    }

    public void ShowBlackAudio(string title, string body, string status)
    {
        StartEffectRoutine(BlackAudioRoutine(title, body, status));
    }

    public void PlayShutdownGlitch(string title, string body, string status, float duration)
    {
        StartEffectRoutine(GlitchRoutine(title, body, status, Mathf.Max(0.1f, duration), new Color(0.02f, 0.04f, 0.06f, 0.98f)));
    }

    public void PlayDeepSleep(string title, string body, string status, float duration)
    {
        StartEffectRoutine(GlitchRoutine(title, body, status, Mathf.Max(0.1f, duration), Color.black));
    }

    private void StartEffectRoutine(IEnumerator routine)
    {
        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
        }

        gameObject.SetActive(true);
        activeRoutine = StartCoroutine(routine);
    }

    private IEnumerator BlackAudioRoutine(string title, string body, string status)
    {
        PrepareText(title, body, status, new Color(0.36f, 0.82f, 1f, 1f));
        SetOverlayColor(Color.black);
        yield return FadeOverlay(1f);
        activeRoutine = null;
    }

    private IEnumerator GlitchRoutine(string title, string body, string status, float duration, Color finalColor)
    {
        PrepareText(title, body, status, new Color(1f, 0.38f, 0.25f, 1f));
        float elapsed = 0f;
        SetOverlayColor(finalColor);

        while (elapsed < duration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float flicker = Mathf.PingPong(elapsed * 18f, 1f);
            SetOverlayAlpha(Mathf.Lerp(0.08f, 0.92f, t) + flicker * 0.08f);

            if (pulseImage != null)
            {
                Color color = pulseImage.color;
                color.a = Mathf.Lerp(0.05f, 0.65f, flicker);
                pulseImage.color = color;
            }

            yield return null;
        }

        SetOverlayAlpha(1f);
        activeRoutine = null;
    }

    private void PrepareText(string title, string body, string status, Color accent)
    {
        ApplyCenteredLayout();
        if (titleText != null)
        {
            titleText.text = title;
            titleText.color = accent;
        }

        if (bodyText != null)
        {
            bodyText.text = body;
        }

        if (statusText != null)
        {
            statusText.text = status;
            statusText.color = accent;
        }

        if (pulseImage != null)
        {
            pulseImage.color = accent;
        }
    }

    private void ApplyCenteredLayout()
    {
        if (!enforceHorizontalCenter)
        {
            return;
        }

        CenterRect(titleText != null ? titleText.rectTransform : null);
        CenterRect(bodyText != null ? bodyText.rectTransform : null);
        CenterRect(statusText != null ? statusText.rectTransform : null);
        CenterRect(pulseImage != null ? pulseImage.rectTransform : null);
        ConfigureContainedText(titleText);
        ConfigureContainedText(bodyText);
        ConfigureContainedText(statusText);
    }

    private void CenterRect(RectTransform rect)
    {
        if (rect == null)
        {
            return;
        }

        Vector2 position = rect.anchoredPosition;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        position.x = (referenceCanvasWidth - rect.sizeDelta.x) * 0.5f;
        rect.anchoredPosition = position;
    }

    private static void ConfigureContainedText(TMP_Text text)
    {
        if (text == null)
        {
            return;
        }

        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.enableAutoSizing = false;
        text.maxVisibleCharacters = int.MaxValue;
        text.maxVisibleWords = int.MaxValue;
        text.maxVisibleLines = int.MaxValue;
        text.overflowMode = TextOverflowModes.Overflow;
    }

    private void SetOverlayColor(Color color)
    {
        if (overlayImage != null)
        {
            overlayImage.color = color;
        }
    }

    private IEnumerator FadeOverlay(float targetAlpha)
    {
        if (overlayGroup == null)
        {
            yield break;
        }

        float start = overlayGroup.alpha;
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, fadeSeconds);

        while (elapsed < duration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            SetOverlayAlpha(Mathf.Lerp(start, targetAlpha, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }

        SetOverlayAlpha(targetAlpha);
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (overlayGroup != null)
        {
            overlayGroup.alpha = Mathf.Clamp01(alpha);
            overlayGroup.blocksRaycasts = overlayGroup.alpha > 0.01f;
            overlayGroup.interactable = false;
        }
    }
}
