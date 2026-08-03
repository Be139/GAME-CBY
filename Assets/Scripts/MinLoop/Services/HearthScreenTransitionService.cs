using System.Collections;
using UnityEngine;

/// <summary>
/// Shared facade for authored screen fades. It never creates a Canvas at
/// runtime; the fade visual must be supplied by the production prefab/scene.
/// </summary>
[DisallowMultipleComponent]
public sealed class HearthScreenTransitionService : MonoBehaviour
{
    [SerializeField] private HearthScreenFader screenFader;
    [SerializeField] private CanvasGroup fallbackCanvasGroup;
    [SerializeField] private bool useUnscaledTime = true;

    public bool IsConfigured
    {
        get { return screenFader != null || fallbackCanvasGroup != null; }
    }

    public void Configure(
        HearthScreenFader fader,
        CanvasGroup group,
        bool unscaledTime)
    {
        screenFader = fader;
        fallbackCanvasGroup = group;
        useUnscaledTime = unscaledTime;
    }

    public void SetImmediate(float alpha)
    {
        if (screenFader != null)
        {
            screenFader.SetImmediate(alpha);
            return;
        }

        ApplyFallbackAlpha(alpha);
    }

    public IEnumerator FadeOut(float seconds)
    {
        return FadeTo(1f, seconds);
    }

    public IEnumerator FadeIn(float seconds)
    {
        return FadeTo(0f, seconds);
    }

    public IEnumerator FadeTo(float targetAlpha, float seconds)
    {
        if (screenFader != null)
        {
            yield return screenFader.FadeTo(targetAlpha, seconds);
            yield break;
        }

        if (fallbackCanvasGroup == null)
        {
            yield break;
        }

        float start = fallbackCanvasGroup.alpha;
        if (seconds <= 0f)
        {
            ApplyFallbackAlpha(targetAlpha);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += useUnscaledTime
                ? Time.unscaledDeltaTime
                : Time.deltaTime;
            ApplyFallbackAlpha(Mathf.Lerp(
                start,
                targetAlpha,
                Mathf.Clamp01(elapsed / seconds)));
            yield return null;
        }

        ApplyFallbackAlpha(targetAlpha);
    }

    private void ApplyFallbackAlpha(float alpha)
    {
        if (fallbackCanvasGroup == null)
        {
            return;
        }

        fallbackCanvasGroup.alpha = Mathf.Clamp01(alpha);
        fallbackCanvasGroup.interactable = false;
        fallbackCanvasGroup.blocksRaycasts = alpha > 0.001f;
    }
}
