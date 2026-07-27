using System;
using UnityEngine;

public enum HearthSubtitlePresentationMode
{
    StandardDialogue,
    CenteredEpilogue,
    TimeCard
}

public enum HearthSubtitleContext
{
    Human,
    FieldUnit,
    Terminal
}

[Serializable]
public class HearthSubtitleLayoutSettings
{
    [Range(0.35f, 0.95f)] public float widthFraction = 0.66f;
    [Range(0.05f, 0.9f)] public float speakerCenterY = 0.31f;
    [Range(0.03f, 0.18f)] public float speakerHeightFraction = 0.06f;
    [Range(0.05f, 0.9f)] public float bodyCenterY = 0.22f;
    [Range(0.06f, 0.4f)] public float bodyHeightFraction = 0.09f;
    public float speakerFontSize = 22f;
    public float speakerMinimumFontSize = 16f;
    public float bodyFontSize = 28f;
    public float bodyMinimumFontSize = 18f;
    [Min(1), Tooltip("QA budget only. Runtime never truncates authored dialogue at this line count.")]
    public int bodyMaximumLines = 8;
    public float lineSpacing;
    [Min(28f)] public float speakerHeightPixels = 34f;
    [Min(48f)] public float minimumBodyHeightPixels = 96f;
    [Min(0f), Tooltip("QA budget only. Runtime may grow higher when required to preserve the full authored line.")]
    public float maximumBodyHeightPixels = 360f;
}

[Serializable]
public class HearthSubtitleContextSettings
{
    [Range(0.6f, 1.15f)] public float widthMultiplier = 1f;
    [Min(0f)] public float horizontalPaddingPixels = 28f;
    [Min(0f)] public float verticalPaddingPixels = 16f;
    [Min(0f)] public float speakerGapPixels = 8f;
    public Color backgroundColor = new Color(0.035f, 0.055f, 0.08f, 0.62f);
    public Color accentColor = new Color(0.47f, 0.67f, 0.86f, 1f);
    public Color speakerColor = new Color(0.84f, 0.9f, 0.96f, 1f);
}

[CreateAssetMenu(menuName = "Hearth/UI/Subtitle Style Profile", fileName = "Hearth_SubtitleStyle")]
public class HearthSubtitleStyleProfile : ScriptableObject
{
    [SerializeField] private Color textColor = new Color(0.84f, 0.9f, 0.96f, 1f);
    [SerializeField] private HearthSubtitleLayoutSettings standardDialogue = new HearthSubtitleLayoutSettings();
    [SerializeField] private HearthSubtitleLayoutSettings centeredEpilogue = new HearthSubtitleLayoutSettings
    {
        speakerCenterY = 0.59f,
        bodyCenterY = 0.46f,
        bodyHeightFraction = 0.3f,
        speakerFontSize = 22f,
        speakerMinimumFontSize = 22f,
        bodyFontSize = 30f,
        bodyMinimumFontSize = 30f,
        bodyMaximumLines = 10,
        minimumBodyHeightPixels = 92f,
        maximumBodyHeightPixels = 440f
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
        bodyMinimumFontSize = 34f,
        bodyMaximumLines = 8,
        minimumBodyHeightPixels = 72f,
        maximumBodyHeightPixels = 360f
    };
    [SerializeField] private HearthSubtitleContextSettings human = new HearthSubtitleContextSettings();
    [SerializeField] private HearthSubtitleContextSettings fieldUnit = new HearthSubtitleContextSettings
    {
        widthMultiplier = 0.84f,
        backgroundColor = new Color(0.035f, 0.055f, 0.08f, 0.76f),
        accentColor = new Color(0.4f, 0.82f, 0.9f, 1f),
        speakerColor = new Color(0.4f, 0.82f, 0.9f, 1f)
    };
    [SerializeField] private HearthSubtitleContextSettings terminal = new HearthSubtitleContextSettings
    {
        widthMultiplier = 0.76f,
        backgroundColor = new Color(0.035f, 0.063f, 0.11f, 0.86f),
        accentColor = new Color(0.88f, 0.59f, 0.25f, 1f),
        speakerColor = new Color(0.95f, 0.72f, 0.4f, 1f)
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

    public HearthSubtitleContextSettings GetContext(HearthSubtitleContext context)
    {
        switch (context)
        {
            case HearthSubtitleContext.FieldUnit:
                return fieldUnit ?? human;
            case HearthSubtitleContext.Terminal:
                return terminal ?? human;
            default:
                return human;
        }
    }

    public float GetSpeakerFontSize(HearthSubtitlePresentationMode mode)
    {
        return mode == HearthSubtitlePresentationMode.TimeCard ? 1f : 22f;
    }

    public float GetBodyFontSize(HearthSubtitlePresentationMode mode)
    {
        switch (mode)
        {
            case HearthSubtitlePresentationMode.CenteredEpilogue:
                return 30f;
            case HearthSubtitlePresentationMode.TimeCard:
                return 34f;
            default:
                return 28f;
        }
    }

    public float TimeCardFadeSeconds
    {
        get { return timeCardFadeSeconds; }
    }

    public void ApplyProductionDefaults()
    {
        textColor = HtmlColor("#D7E6F6", Color.white);

        EnsureInstances();
        ConfigureLayout(standardDialogue, 0.66f, 0.31f, 0.06f, 0.223f, 0.09f, 22f, 28f, 8, 34f, 96f, 360f);
        ConfigureLayout(centeredEpilogue, 0.66f, 0.59f, 0.06f, 0.46f, 0.3f, 22f, 30f, 10, 34f, 92f, 440f);
        ConfigureLayout(timeCard, 0.72f, 0.56f, 0.04f, 0.5f, 0.18f, 1f, 34f, 8, 28f, 72f, 360f);

        ConfigureContext(
            human,
            1f,
            28f,
            16f,
            8f,
            new Color(0.035f, 0.055f, 0.08f, 0.62f),
            HtmlColor("#78AADC", new Color(0.47f, 0.67f, 0.86f, 1f)),
            HtmlColor("#D7E6F6", Color.white));
        ConfigureContext(
            fieldUnit,
            0.84f,
            28f,
            16f,
            8f,
            new Color(0.035f, 0.055f, 0.08f, 0.76f),
            new Color(0.4f, 0.82f, 0.9f, 1f),
            new Color(0.4f, 0.82f, 0.9f, 1f));
        ConfigureContext(
            terminal,
            0.76f,
            28f,
            16f,
            8f,
            new Color(0.035f, 0.063f, 0.11f, 0.86f),
            HtmlColor("#E0973F", new Color(0.88f, 0.59f, 0.25f, 1f)),
            HtmlColor("#F4B766", new Color(0.95f, 0.72f, 0.4f, 1f)));

        timeCardFadeSeconds = 0.35f;
        OnValidate();
    }

    private void OnValidate()
    {
        EnsureInstances();
        Sanitize(standardDialogue);
        Sanitize(centeredEpilogue);
        Sanitize(timeCard);
        Sanitize(human);
        Sanitize(fieldUnit);
        Sanitize(terminal);
        timeCardFadeSeconds = Mathf.Max(0f, timeCardFadeSeconds);
    }

    private void EnsureInstances()
    {
        if (standardDialogue == null) standardDialogue = new HearthSubtitleLayoutSettings();
        if (centeredEpilogue == null) centeredEpilogue = new HearthSubtitleLayoutSettings();
        if (timeCard == null) timeCard = new HearthSubtitleLayoutSettings();
        if (human == null) human = new HearthSubtitleContextSettings();
        if (fieldUnit == null) fieldUnit = new HearthSubtitleContextSettings();
        if (terminal == null) terminal = new HearthSubtitleContextSettings();
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
        layout.bodyMaximumLines = Mathf.Max(1, layout.bodyMaximumLines);
        layout.speakerHeightPixels = Mathf.Max(28f, layout.speakerHeightPixels);
        layout.minimumBodyHeightPixels = Mathf.Max(48f, layout.minimumBodyHeightPixels);
        layout.maximumBodyHeightPixels = Mathf.Max(layout.minimumBodyHeightPixels, layout.maximumBodyHeightPixels);
    }

    private static void Sanitize(HearthSubtitleContextSettings context)
    {
        if (context == null)
        {
            return;
        }

        context.widthMultiplier = Mathf.Clamp(context.widthMultiplier, 0.6f, 1.15f);
        context.horizontalPaddingPixels = Mathf.Max(0f, context.horizontalPaddingPixels);
        context.verticalPaddingPixels = Mathf.Max(0f, context.verticalPaddingPixels);
        context.speakerGapPixels = Mathf.Max(0f, context.speakerGapPixels);
    }

    private static void ConfigureLayout(
        HearthSubtitleLayoutSettings layout,
        float widthFraction,
        float speakerCenterY,
        float speakerHeightFraction,
        float bodyCenterY,
        float bodyHeightFraction,
        float speakerFontSize,
        float bodyFontSize,
        int bodyMaximumLines,
        float speakerHeightPixels,
        float minimumBodyHeightPixels,
        float maximumBodyHeightPixels)
    {
        layout.widthFraction = widthFraction;
        layout.speakerCenterY = speakerCenterY;
        layout.speakerHeightFraction = speakerHeightFraction;
        layout.bodyCenterY = bodyCenterY;
        layout.bodyHeightFraction = bodyHeightFraction;
        layout.speakerFontSize = speakerFontSize;
        layout.speakerMinimumFontSize = speakerFontSize;
        layout.bodyFontSize = bodyFontSize;
        layout.bodyMinimumFontSize = bodyFontSize;
        layout.bodyMaximumLines = bodyMaximumLines;
        layout.lineSpacing = 0f;
        layout.speakerHeightPixels = speakerHeightPixels;
        layout.minimumBodyHeightPixels = minimumBodyHeightPixels;
        layout.maximumBodyHeightPixels = maximumBodyHeightPixels;
    }

    private static void ConfigureContext(
        HearthSubtitleContextSettings context,
        float widthMultiplier,
        float horizontalPaddingPixels,
        float verticalPaddingPixels,
        float speakerGapPixels,
        Color backgroundColor,
        Color accentColor,
        Color speakerColor)
    {
        context.widthMultiplier = widthMultiplier;
        context.horizontalPaddingPixels = horizontalPaddingPixels;
        context.verticalPaddingPixels = verticalPaddingPixels;
        context.speakerGapPixels = speakerGapPixels;
        context.backgroundColor = backgroundColor;
        context.accentColor = accentColor;
        context.speakerColor = speakerColor;
    }

    private static Color HtmlColor(string html, Color fallback)
    {
        Color color;
        return ColorUtility.TryParseHtmlString(html, out color) ? color : fallback;
    }
}
