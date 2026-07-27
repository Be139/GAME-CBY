using TMPro;
using UnityEngine;

[CreateAssetMenu(
    menuName = "Hearth/UI/V2/Theme Profile",
    fileName = "Hearth_UiV2Theme")]
public sealed class HearthUiThemeProfile : ScriptableObject
{
    [Header("Typography")]
    [Tooltip("Assign the project's Liberation Sans TMP font asset here.")]
    [SerializeField] private TMP_FontAsset primaryFontAsset;

    [Header("Terminal Palette")]
    [SerializeField] private Color terminalBackground = new Color32(11, 16, 24, 255);
    [SerializeField] private Color terminalPanelBackground = new Color32(9, 16, 28, 255);
    [SerializeField] private Color secondary = new Color32(95, 120, 149, 255);
    [SerializeField] private Color primary = new Color32(215, 230, 246, 255);
    [SerializeField] private Color information = new Color32(120, 170, 220, 255);

    [Header("Semantic Palette")]
    [SerializeField] private Color warning = new Color32(224, 151, 63, 255);
    [SerializeField] private Color warningHighlight = new Color32(244, 183, 102, 255);
    [SerializeField] private Color success = new Color32(87, 184, 138, 255);
    [SerializeField] private Color danger = new Color32(224, 82, 77, 255);

    [Header("UI Metrics")]
    [SerializeField, Min(1f)] private float ruleLineThickness = 2f;
    [SerializeField] private Vector2 regularKeycapSize = new Vector2(64f, 40f);
    [SerializeField] private Vector2 wideKeycapSize = new Vector2(96f, 40f);
    [SerializeField, Min(1f)] private float speakerFontSize = 22f;
    [SerializeField, Min(1f)] private float subtitleFontSize = 28f;
    [SerializeField, Min(1f)] private float endingSubtitleFontSize = 30f;

    public TMP_FontAsset PrimaryFontAsset { get { return primaryFontAsset; } }
    public Color TerminalBackground { get { return terminalBackground; } }
    public Color TerminalPanelBackground { get { return terminalPanelBackground; } }
    public Color Secondary { get { return secondary; } }
    public Color Primary { get { return primary; } }
    public Color Information { get { return information; } }
    public Color Warning { get { return warning; } }
    public Color WarningHighlight { get { return warningHighlight; } }
    public Color Success { get { return success; } }
    public Color Danger { get { return danger; } }
    public float RuleLineThickness { get { return ruleLineThickness; } }
    public Vector2 RegularKeycapSize { get { return regularKeycapSize; } }
    public Vector2 WideKeycapSize { get { return wideKeycapSize; } }
    public float SpeakerFontSize { get { return speakerFontSize; } }
    public float SubtitleFontSize { get { return subtitleFontSize; } }
    public float EndingSubtitleFontSize { get { return endingSubtitleFontSize; } }

    private void OnValidate()
    {
        ruleLineThickness = Mathf.Max(1f, ruleLineThickness);
        regularKeycapSize.x = Mathf.Max(1f, regularKeycapSize.x);
        regularKeycapSize.y = Mathf.Max(1f, regularKeycapSize.y);
        wideKeycapSize.x = Mathf.Max(regularKeycapSize.x, wideKeycapSize.x);
        wideKeycapSize.y = Mathf.Max(1f, wideKeycapSize.y);
        speakerFontSize = Mathf.Max(1f, speakerFontSize);
        subtitleFontSize = Mathf.Max(1f, subtitleFontSize);
        endingSubtitleFontSize = Mathf.Max(1f, endingSubtitleFontSize);
    }
}
