using System.Collections;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class HearthLocationHudView : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text locationText;
    [SerializeField] private TMP_Text glowText;

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float holdWhenLost;
    [SerializeField] private float blurStartScale = 1.08f;
    [SerializeField] private float glowAlpha = 0.32f;
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private bool keepVisibleWhenHidden = true;
    [SerializeField] private string placeholderLabel = string.Empty;

    [Header("Style")]
    [SerializeField] private Color titleColor = new Color(0.52f, 0.82f, 0.95f, 0.86f);
    [SerializeField] private Color textColor = new Color(0.78f, 0.93f, 1f, 0.96f);
    [SerializeField] private Color glowColor = new Color(0.25f, 0.75f, 1f, 0.32f);

    private Coroutine transitionRoutine;
    private Coroutine delayedHideRoutine;
    private string currentLabel;
    private bool visible;

    public float FadeDuration
    {
        get { return fadeDuration; }
    }

    public void Configure(CanvasGroup newCanvasGroup, TMP_Text newTitleText, TMP_Text newLocationText, TMP_Text newGlowText)
    {
        canvasGroup = newCanvasGroup;
        titleText = newTitleText;
        locationText = newLocationText;
        glowText = newGlowText;
        ApplyStyle();
    }

    private void Awake()
    {
        ResolveReferences();
        ApplyStyle();
        HideImmediate();
    }

    private void OnValidate()
    {
        fadeDuration = Mathf.Max(0f, fadeDuration);
        holdWhenLost = Mathf.Max(0f, holdWhenLost);
        blurStartScale = Mathf.Max(1f, blurStartScale);
        glowAlpha = Mathf.Clamp01(glowAlpha);
        // RectTransform writes from OnValidate can dispatch TMP dimension
        // callbacks during Unity's consistency pass. Runtime Awake and the
        // explicit editor Configure path both apply the same style safely.
        if (Application.isPlaying)
        {
            ApplyStyle();
        }
    }

    public void ShowLocation(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            HideLocation();
            return;
        }

        if (delayedHideRoutine != null)
        {
            StopCoroutine(delayedHideRoutine);
            delayedHideRoutine = null;
        }

        bool labelChanged = currentLabel != label;
        currentLabel = label;
        SetLabel(label);
        if (labelChanged && keepVisibleWhenHidden)
        {
            EnsureVisible();
            SetLocationValueAlpha(0f);
        }
        else if (labelChanged && canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        if (!labelChanged && visible && canvasGroup != null && canvasGroup.alpha >= 0.99f)
        {
            return;
        }

        StartTransition(true);
    }

    public void HideLocation()
    {
        if (keepVisibleWhenHidden)
        {
            EnsureVisible();
            return;
        }

        if (holdWhenLost > 0f)
        {
            if (delayedHideRoutine != null)
            {
                StopCoroutine(delayedHideRoutine);
            }

            delayedHideRoutine = StartCoroutine(DelayedHideRoutine());
            return;
        }

        StartTransition(false);
    }

    public void HideImmediate()
    {
        StopRoutines();
        visible = keepVisibleWhenHidden;
        currentLabel = string.Empty;
        SetLabel(placeholderLabel);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = keepVisibleWhenHidden ? 1f : 0f;
            canvasGroup.gameObject.SetActive(keepVisibleWhenHidden);
        }

        transform.localScale = Vector3.one;
        SetLocationValueAlpha(1f);
        SetGlowAlpha(0f);
    }

    private void ResolveReferences()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private IEnumerator DelayedHideRoutine()
    {
        yield return WaitForDuration(holdWhenLost);
        delayedHideRoutine = null;
        StartTransition(false);
    }

    private void StartTransition(bool show)
    {
        ResolveReferences();
        if (show && canvasGroup != null)
        {
            canvasGroup.gameObject.SetActive(true);
        }

        // The V2 preview and the central UI coordinator can suppress the
        // complete Human HUD while a different base view is active. A child
        // under that inactive hierarchy cannot own a coroutine, so commit the
        // requested state immediately and let the next visible presentation
        // animate normally.
        if (!isActiveAndEnabled || !gameObject.activeInHierarchy)
        {
            StopRoutines();
            visible = show || keepVisibleWhenHidden;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.gameObject.SetActive(visible);
            }

            transform.localScale = Vector3.one;
            SetLocationValueAlpha(1f);
            SetGlowAlpha(0f);
            return;
        }

        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
        }

        transitionRoutine = StartCoroutine(TransitionRoutine(show));
    }

    private IEnumerator TransitionRoutine(bool show)
    {
        ResolveReferences();
        visible = show;

        if (canvasGroup == null)
        {
            yield break;
        }

        canvasGroup.gameObject.SetActive(true);

        if (keepVisibleWhenHidden)
        {
            yield return RunPersistentTransition(show);
            transitionRoutine = null;
            yield break;
        }

        float startAlpha = canvasGroup.alpha;
        float endAlpha = show ? 1f : 0f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.one;

        if (show)
        {
            startScale = Vector3.one * blurStartScale;
            transform.localScale = startScale;
        }

        if (fadeDuration <= 0f)
        {
            canvasGroup.alpha = endAlpha;
            transform.localScale = Vector3.one;
            canvasGroup.gameObject.SetActive(show || keepVisibleWhenHidden);
            transitionRoutine = null;
            yield break;
        }

        while (elapsed < fadeDuration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float eased = 1f - Mathf.Pow(1f - t, 2f);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, eased);
            transform.localScale = Vector3.Lerp(startScale, endScale, eased);
            SetGlowAlpha(show ? (1f - t) * glowAlpha : t * glowAlpha);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
        transform.localScale = Vector3.one;
        SetGlowAlpha(show ? 0f : glowAlpha);
        canvasGroup.gameObject.SetActive(show || keepVisibleWhenHidden);
        transitionRoutine = null;
    }

    private void EnsureVisible()
    {
        ResolveReferences();
        StopRoutines();
        visible = true;

        if (canvasGroup != null)
        {
            canvasGroup.gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
        }

        transform.localScale = Vector3.one;
        SetLocationValueAlpha(1f);
        SetGlowAlpha(0f);
    }

    private IEnumerator RunPersistentTransition(bool show)
    {
        visible = true;

        if (canvasGroup != null)
        {
            canvasGroup.gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
        }

        if (!show)
        {
            transform.localScale = Vector3.one;
            SetLocationValueAlpha(1f);
            SetGlowAlpha(0f);
            yield break;
        }

        float elapsed = 0f;
        Vector3 startScale = Vector3.one * blurStartScale;
        transform.localScale = startScale;

        if (fadeDuration <= 0f)
        {
            transform.localScale = Vector3.one;
            SetLocationValueAlpha(1f);
            SetGlowAlpha(0f);
            yield break;
        }

        while (elapsed < fadeDuration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float eased = 1f - Mathf.Pow(1f - t, 2f);
            transform.localScale = Vector3.Lerp(startScale, Vector3.one, eased);
            SetLocationValueAlpha(eased);
            SetGlowAlpha((1f - t) * glowAlpha);
            yield return null;
        }

        transform.localScale = Vector3.one;
        SetLocationValueAlpha(1f);
        SetGlowAlpha(0f);
    }

    private void SetLabel(string label)
    {
        if (titleText != null)
        {
            titleText.text = "LOCATION";
        }

        if (locationText != null)
        {
            locationText.text = label;
        }

        if (glowText != null)
        {
            glowText.text = label;
        }
    }

    private void ApplyStyle()
    {
        if (titleText != null)
        {
            ApplyLeftAlignedRect(titleText, 0f, 28f);
            titleText.fontSize = 16f;
            titleText.fontSizeMin = 16f;
            titleText.fontSizeMax = 16f;
            titleText.enableAutoSizing = false;
            titleText.alignment = TextAlignmentOptions.BottomLeft;
            titleText.color = titleColor;
        }

        if (locationText != null)
        {
            ApplyLeftAlignedRect(locationText, 30f, 38f);
            locationText.fontSize = 24f;
            locationText.fontSizeMin = 24f;
            locationText.fontSizeMax = 24f;
            locationText.enableAutoSizing = false;
            locationText.alignment = TextAlignmentOptions.BottomLeft;
            locationText.color = textColor;
        }

        if (glowText != null)
        {
            ApplyLeftAlignedRect(glowText, 30f, 38f);
            glowText.fontSize = 24f;
            glowText.fontSizeMin = 24f;
            glowText.fontSizeMax = 24f;
            glowText.enableAutoSizing = false;
            glowText.alignment = TextAlignmentOptions.BottomLeft;
            glowColor.a = glowAlpha;
            glowText.color = glowColor;
        }
    }

    private static void ApplyLeftAlignedRect(
        TMP_Text text,
        float top,
        float height)
    {
        if (text == null)
        {
            return;
        }

        RectTransform rect = text.rectTransform;
        rect.anchorMin = Vector2.up;
        rect.anchorMax = Vector2.up;
        rect.pivot = Vector2.up;
        rect.anchoredPosition = new Vector2(0f, -top);
        rect.sizeDelta = new Vector2(340f, height);
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
    }

    private void SetGlowAlpha(float alpha)
    {
        if (glowText == null)
        {
            return;
        }

        Color color = glowColor;
        color.a = Mathf.Clamp01(alpha);
        glowText.color = color;
    }

    private void SetLocationValueAlpha(float alpha)
    {
        if (locationText == null)
        {
            return;
        }

        Color color = textColor;
        color.a *= Mathf.Clamp01(alpha);
        locationText.color = color;
    }

    private void StopRoutines()
    {
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        if (delayedHideRoutine != null)
        {
            StopCoroutine(delayedHideRoutine);
            delayedHideRoutine = null;
        }
    }

    private IEnumerator WaitForDuration(float seconds)
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
}
