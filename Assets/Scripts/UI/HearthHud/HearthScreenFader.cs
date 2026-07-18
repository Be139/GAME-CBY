using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class HearthScreenFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private bool blockRaycastsWhileVisible = true;

    public float Alpha
    {
        get { return canvasGroup != null ? canvasGroup.alpha : 0f; }
    }

    public bool IsVisible
    {
        get { return Alpha > 0.001f; }
    }

    private void Awake()
    {
        ResolveReferences();
        ApplyInteractionState();
    }

    public void Configure(CanvasGroup group, bool unscaledTime)
    {
        canvasGroup = group;
        useUnscaledTime = unscaledTime;
        ApplyInteractionState();
    }

    public void SetImmediate(float alpha)
    {
        ResolveReferences();
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = Mathf.Clamp01(alpha);
        ApplyInteractionState();
    }

    public IEnumerator FadeTo(float targetAlpha, float duration)
    {
        ResolveReferences();
        if (canvasGroup == null)
        {
            yield break;
        }

        targetAlpha = Mathf.Clamp01(targetAlpha);
        if (duration <= 0f)
        {
            SetImmediate(targetAlpha);
            yield break;
        }

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            ApplyInteractionState();
            yield return null;
        }

        SetImmediate(targetAlpha);
    }

    public IEnumerator FadeOut(float duration)
    {
        yield return FadeTo(1f, duration);
    }

    public IEnumerator FadeIn(float duration)
    {
        yield return FadeTo(0f, duration);
    }

    private void ResolveReferences()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }

    private void ApplyInteractionState()
    {
        if (canvasGroup == null)
        {
            return;
        }

        bool blocks = blockRaycastsWhileVisible && canvasGroup.alpha > 0.001f;
        canvasGroup.blocksRaycasts = blocks;
        canvasGroup.interactable = false;
    }
}
