using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HearthCompanionDecisionPanelView : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text kickerText;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Image accentImage;

    [Header("Motion")]
    [SerializeField] private float fadeSeconds = 0.18f;
    [SerializeField] private bool useUnscaledTime = true;

    private Coroutine fadeRoutine;

    public bool IsVisible
    {
        get { return canvasGroup != null && canvasGroup.alpha > 0.001f; }
    }

    public void Configure(CanvasGroup newCanvasGroup, TMP_Text newKickerText, TMP_Text newTitleText, TMP_Text newBodyText, Image newAccentImage)
    {
        canvasGroup = newCanvasGroup;
        kickerText = newKickerText;
        titleText = newTitleText;
        bodyText = newBodyText;
        accentImage = newAccentImage;
    }

    public void Apply(HearthCompanionHudSceneData scene)
    {
        if (scene == null)
        {
            HideImmediate();
            return;
        }

        if (kickerText != null)
        {
            kickerText.text = scene.DecisionKicker;
            kickerText.color = scene.AccentColor;
        }

        if (titleText != null)
        {
            titleText.text = scene.DecisionTitle;
        }

        if (bodyText != null)
        {
            bodyText.text = scene.DecisionBody;
        }

        if (accentImage != null)
        {
            accentImage.color = scene.AccentColor;
        }

        FadeTo(1f);
    }

    public void HideImmediate()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void FadeTo(float alpha)
    {
        if (canvasGroup == null)
        {
            return;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeRoutine(alpha));
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        float start = canvasGroup.alpha;
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, fadeSeconds);

        while (elapsed < duration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = Mathf.Lerp(start, targetAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.interactable = targetAlpha > 0.01f;
        canvasGroup.blocksRaycasts = false;
        fadeRoutine = null;
    }
}
