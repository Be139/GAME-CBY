using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HearthCompanionTriggerCardView : MonoBehaviour
{
    private static readonly Color V2TitleColor =
        new Color32(122, 209, 235, 255);
    private static readonly Color V2BodyColor =
        new Color32(215, 230, 246, 245);

    [Header("Bindings")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Image accentImage;

    [Header("Motion")]
    [SerializeField] private float fadeSeconds = 0.2f;
    [SerializeField] private bool useUnscaledTime = true;

    private Coroutine routine;
    private bool isVisible;

    public event System.Action<bool> VisibilityChanged;

    public bool IsVisible { get { return isVisible; } }

    public void Configure(CanvasGroup newCanvasGroup, TMP_Text newTitleText, TMP_Text newBodyText, Image newAccentImage)
    {
        canvasGroup = newCanvasGroup;
        titleText = newTitleText;
        bodyText = newBodyText;
        accentImage = newAccentImage;
        ApplyV2VisualPolicy();
        HideImmediate();
    }

    private void OnDisable()
    {
        HideImmediate();
    }

    public void Apply(HearthCompanionHudSceneData scene)
    {
        if (scene == null)
        {
            HideImmediate();
            return;
        }

        HearthCompanionTimedCue[] cues = scene.TimedCues;
        if (cues != null && cues.Length > 0)
        {
            ShowCueSequence(cues, scene.AccentColor);
            return;
        }

        if (!scene.ShowTriggerCard)
        {
            HideImmediate();
            return;
        }

        ShowCard(scene.TriggerCardTitle, scene.TriggerCardBody, scene.AccentColor, scene.TriggerCardDelay, scene.TriggerCardSeconds);
    }

    public void ShowCueSequence(HearthCompanionTimedCue[] cues, Color accentColor)
    {
        if (canvasGroup == null || cues == null || cues.Length == 0)
        {
            HideImmediate();
            return;
        }

        StopActiveRoutine();
        ApplyV2VisualPolicy();

        routine = StartCoroutine(CueSequenceRoutine(cues, accentColor));
    }

    public void ShowCard(string title, string body, Color accentColor, float delay, float seconds)
    {
        if (canvasGroup == null)
        {
            HideImmediate();
            return;
        }

        StopActiveRoutine();
        ApplyV2VisualPolicy();

        routine = StartCoroutine(CardRoutine(title, body, accentColor, delay, seconds));
    }

    public void HideImmediate()
    {
        StopActiveRoutine();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        SetVisibleState(false);
    }

    private IEnumerator CardRoutine(string title, string body, Color accentColor, float delay, float seconds)
    {
        if (canvasGroup == null)
        {
            yield break;
        }

        canvasGroup.alpha = 0f;

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (bodyText != null)
        {
            bodyText.text = body;
        }

        if (delay > 0f)
        {
            yield return Wait(delay);
        }

        SetVisibleState(true);
        yield return Fade(1f);

        if (seconds > 0f)
        {
            yield return Wait(seconds);
        }

        yield return Fade(0f);
        SetVisibleState(false);
        routine = null;
    }

    private IEnumerator CueSequenceRoutine(HearthCompanionTimedCue[] cues, Color accentColor)
    {
        if (canvasGroup == null)
        {
            yield break;
        }

        canvasGroup.alpha = 0f;

        for (int i = 0; i < cues.Length; i++)
        {
            HearthCompanionTimedCue cue = cues[i];
            if (cue == null)
            {
                continue;
            }

            if (cue.delaySeconds > 0f)
            {
                yield return Wait(cue.delaySeconds);
            }

            if (titleText != null)
            {
                titleText.text = cue.title;
            }

            if (bodyText != null)
            {
                bodyText.text = cue.body;
            }

            SetVisibleState(true);
            yield return Fade(1f);

            if (cue.visibleSeconds > 0f)
            {
                yield return Wait(cue.visibleSeconds);
            }

            yield return Fade(0f);
            SetVisibleState(false);
        }

        routine = null;
    }

    private void ApplyV2VisualPolicy()
    {
        if (titleText != null)
        {
            titleText.color = V2TitleColor;
            titleText.fontSize = 20f;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.TopLeft;
            titleText.enableAutoSizing = false;
            titleText.enableWordWrapping = true;
            titleText.overflowMode = TextOverflowModes.Overflow;
        }

        if (bodyText != null)
        {
            bodyText.color = V2BodyColor;
            bodyText.fontSize = 18f;
            bodyText.fontStyle = FontStyles.Normal;
            bodyText.alignment = TextAlignmentOptions.TopLeft;
            bodyText.enableAutoSizing = false;
            bodyText.enableWordWrapping = true;
            bodyText.overflowMode = TextOverflowModes.Overflow;
        }

        // The old TriggerCardAccent is the full-height V1 vertical rule.  It
        // remains serialized for backwards-compatible prefab bindings, but
        // V2 owns its border through the fixed vector panel frame instead.
        if (accentImage != null)
        {
            accentImage.raycastTarget = false;
            accentImage.gameObject.SetActive(false);
        }
    }

    private void StopActiveRoutine()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        SetVisibleState(false);
    }

    private void SetVisibleState(bool visible)
    {
        if (isVisible == visible)
        {
            return;
        }

        isVisible = visible;
        if (VisibilityChanged != null)
        {
            VisibilityChanged.Invoke(visible);
        }
    }

    private IEnumerator Fade(float targetAlpha)
    {
        float start = canvasGroup.alpha;
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, fadeSeconds);

        while (elapsed < duration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, targetAlpha, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
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
}
