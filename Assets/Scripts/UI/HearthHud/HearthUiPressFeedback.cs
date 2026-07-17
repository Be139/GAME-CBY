using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HearthUiPressFeedback : MonoBehaviour
{
    [SerializeField] private Graphic[] targetGraphics;
    [SerializeField] private Color pressedColor = new Color(0.42f, 0.46f, 0.48f, 1f);
    [SerializeField] private float feedbackSeconds = 0.14f;
    [SerializeField] private bool useUnscaledTime = true;

    private Color[] restingColors;
    private Coroutine feedbackRoutine;

    public void Configure(Graphic[] targets)
    {
        RestoreColors();
        targetGraphics = targets;
        CacheColors();
    }

    public void PlayFeedback()
    {
        if (targetGraphics == null || targetGraphics.Length == 0)
        {
            return;
        }

        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
            RestoreColors();
        }

        CacheColors();
        feedbackRoutine = StartCoroutine(FeedbackRoutine());
    }

    private void OnDisable()
    {
        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
            feedbackRoutine = null;
        }

        RestoreColors();
    }

    private void OnValidate()
    {
        feedbackSeconds = Mathf.Max(0.01f, feedbackSeconds);
    }

    private IEnumerator FeedbackRoutine()
    {
        for (int i = 0; i < targetGraphics.Length; i++)
        {
            Graphic graphic = targetGraphics[i];
            if (graphic == null)
            {
                continue;
            }

            Color color = pressedColor;
            color.a = restingColors != null && i < restingColors.Length
                ? restingColors[i].a
                : graphic.color.a;
            graphic.color = color;
        }

        if (useUnscaledTime)
        {
            yield return new WaitForSecondsRealtime(feedbackSeconds);
        }
        else
        {
            yield return new WaitForSeconds(feedbackSeconds);
        }

        RestoreColors();
        feedbackRoutine = null;
    }

    private void CacheColors()
    {
        if (targetGraphics == null)
        {
            restingColors = null;
            return;
        }

        restingColors = new Color[targetGraphics.Length];
        for (int i = 0; i < targetGraphics.Length; i++)
        {
            restingColors[i] = targetGraphics[i] != null ? targetGraphics[i].color : Color.white;
        }
    }

    private void RestoreColors()
    {
        if (targetGraphics == null || restingColors == null)
        {
            return;
        }

        int count = Mathf.Min(targetGraphics.Length, restingColors.Length);
        for (int i = 0; i < count; i++)
        {
            if (targetGraphics[i] != null)
            {
                targetGraphics[i].color = restingColors[i];
            }
        }
    }
}
