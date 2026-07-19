using System;
using UnityEngine;

public enum HearthSubtitlePresentationMode
{
    StandardDialogue,
    CenteredEpilogue,
    TimeCard
}

[Serializable]
public class HearthSubtitleLayoutSettings
{
    [Range(0.35f, 0.95f)] public float widthFraction = 0.66f;
    [Range(0.05f, 0.9f)] public float speakerCenterY = 0.31f;
    [Range(0.03f, 0.18f)] public float speakerHeightFraction = 0.06f;
    [Range(0.05f, 0.9f)] public float bodyCenterY = 0.22f;
    [Range(0.06f, 0.4f)] public float bodyHeightFraction = 0.16f;
    public float speakerFontSize = 22f;
    public float speakerMinimumFontSize = 16f;
    public float bodyFontSize = 28f;
    public float bodyMinimumFontSize = 18f;
    [Range(1, 2)] public int bodyMaximumLines = 2;
    public float lineSpacing;
}

[CreateAssetMenu(menuName = "Hearth/UI/Subtitle Style Profile", fileName = "Hearth_SubtitleStyle")]
public class HearthSubtitleStyleProfile : ScriptableObject
{
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private HearthSubtitleLayoutSettings standardDialogue = new HearthSubtitleLayoutSettings();
    [SerializeField] private HearthSubtitleLayoutSettings centeredEpilogue = new HearthSubtitleLayoutSettings
    {
        speakerCenterY = 0.59f,
        bodyCenterY = 0.46f,
        bodyHeightFraction = 0.3f,
        speakerFontSize = 23f,
        bodyFontSize = 30f,
        bodyMinimumFontSize = 20f,
        bodyMaximumLines = 2
    };
    [SerializeField] private HearthSubtitleLayoutSettings timeCard = new HearthSubtitleLayoutSettings
    {
        widthFraction = 0.72f,
        speakerCenterY = 0.56f,
        speakerHeightFraction = 0.04f,
        bodyCenterY = 0.5f,
        bodyHeightFraction = 0.18f,
        speakerFontSize = 1f,
        speakerMinimumFontSize = 1f,
        bodyFontSize = 34f,
        bodyMinimumFontSize = 24f,
        bodyMaximumLines = 2
    };
    [SerializeField, Min(0f)] private float timeCardFadeSeconds = 0.35f;

    public Color TextColor
    {
        get { return textColor; }
    }

    public HearthSubtitleLayoutSettings GetLayout(HearthSubtitlePresentationMode mode)
    {
        switch (mode)
        {
            case HearthSubtitlePresentationMode.CenteredEpilogue:
                return centeredEpilogue;
            case HearthSubtitlePresentationMode.TimeCard:
                return timeCard;
            default:
                return standardDialogue;
        }
    }

    public float TimeCardFadeSeconds
    {
        get { return timeCardFadeSeconds; }
    }

    private void OnValidate()
    {
        Sanitize(standardDialogue);
        Sanitize(centeredEpilogue);
        Sanitize(timeCard);
        timeCardFadeSeconds = Mathf.Max(0f, timeCardFadeSeconds);
    }

    private static void Sanitize(HearthSubtitleLayoutSettings layout)
    {
        if (layout == null)
        {
            return;
        }

        layout.speakerFontSize = Mathf.Max(1f, layout.speakerFontSize);
        layout.speakerMinimumFontSize = Mathf.Clamp(layout.speakerMinimumFontSize, 1f, layout.speakerFontSize);
        layout.bodyFontSize = Mathf.Max(1f, layout.bodyFontSize);
        layout.bodyMinimumFontSize = Mathf.Clamp(layout.bodyMinimumFontSize, 1f, layout.bodyFontSize);
        layout.bodyMaximumLines = Mathf.Clamp(layout.bodyMaximumLines, 1, 2);
    }
}
