using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HearthCompanionTriggerCardView : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Image accentImage;

    [Header("Motion")]
    [SerializeField] private float fadeSeconds = 0.2f;
    [SerializeField] private bool useUnscaledTime = true;

    private Coroutine routine;

    public void Configure(CanvasGroup newCanvasGroup, TMP_Text newTitleText, TMP_Text newBodyText, Image newAccentImage)
    {
        canvasGroup = newCanvasGroup;
        titleText = newTitleText;
        bodyText = newBodyText;
        accentImage = newAccentImage;
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
        if (routine != null)
        {
            StopCoroutine(routine);
        }

        routine = StartCoroutine(CueSequenceRoutine(cues, accentColor));
    }

    public void ShowCard(string title, string body, Color accentColor, float delay, float seconds)
    {
        if (routine != null)
        {
            StopCoroutine(routine);
        }

        routine = StartCoroutine(CardRoutine(title, body, accentColor, delay, seconds));
    }

    public void HideImmediate()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
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
            titleText.color = accentColor;
        }

        if (bodyText != null)
        {
            bodyText.text = body;
        }

        if (accentImage != null)
        {
            accentImage.color = accentColor;
        }

        if (delay > 0f)
        {
            yield return Wait(delay);
        }

        yield return Fade(1f);

        if (seconds > 0f)
        {
            yield return Wait(seconds);
        }

        yield return Fade(0f);
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
                titleText.color = accentColor;
            }

            if (bodyText != null)
            {
                bodyText.text = cue.body;
            }

            if (accentImage != null)
            {
                accentImage.color = accentColor;
            }

            yield return Fade(1f);

            if (cue.visibleSeconds > 0f)
            {
                yield return Wait(cue.visibleSeconds);
            }

            yield return Fade(0f);
        }

        routine = null;
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
