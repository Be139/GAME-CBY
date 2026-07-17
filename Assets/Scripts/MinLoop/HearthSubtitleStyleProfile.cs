using System;
using UnityEngine;

public enum HearthSubtitlePresentationMode
{
    StandardDialogue,
    CenteredEpilogue
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
    [Range(1, 8)] public int bodyMaximumLines = 5;
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
        bodyMaximumLines = 6
    };

    public Color TextColor
    {
        get { return textColor; }
    }

    public HearthSubtitleLayoutSettings GetLayout(HearthSubtitlePresentationMode mode)
    {
        return mode == HearthSubtitlePresentationMode.CenteredEpilogue
            ? centeredEpilogue
            : standardDialogue;
    }

    private void OnValidate()
    {
        Sanitize(standardDialogue);
        Sanitize(centeredEpilogue);
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
        layout.bodyMaximumLines = Mathf.Clamp(layout.bodyMaximumLines, 1, 8);
    }
}
