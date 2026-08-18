using TMPro;
using UnityEngine;

[CreateAssetMenu(
    menuName = "Hearth/UI/V2/Theme Profile",
    fileName = "Hearth_UiV2Theme")]
public sealed class HearthUiThemeProfile : ScriptableObject
{
    [Header("Typography")]
    [Tooltip("Primary UI font: headings, labels, buttons, speaker names and operation hints.")]
    [SerializeField] private TMP_FontAsset primaryFontAsset;
    [Tooltip("Dialogue body font: spoken subtitle sentences and terminal dialogue bodies.")]
    [SerializeField] private TMP_FontAsset dialogueFontAsset;

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

    [Header("Terminal Dialogue Typography")]
    [SerializeField, Min(1f)] private float terminalDialogueSpeakerFontSize = 52f;
    [SerializeField, Min(1f)] private float terminalDialogueBodyFontSize = 26f;
    [SerializeField, Min(1f)] private float terminalDialogueAdvanceFontSize = 26f;

    [Header("Companion Header Typography")]
    [SerializeField, Min(1f)] private float companionIdentityHeadingFontSize = 21f;
    [SerializeField, Min(1f)] private float companionIdentityValueFontSize = 32f;
    [SerializeField, Min(1f)] private float companionTaskHeadingFontSize = 19f;
    [SerializeField, Min(1f)] private float companionTaskBodyFontSize = 22f;

    [Header("Interaction Typography")]
    [SerializeField, Min(1f)] private float interactionPromptFontSize = 22f;

    [Header("Finale Typography")]
    [SerializeField, Min(1f)] private float sceneCardFontSize = 44f;
    [SerializeField, Min(1f)] private float epilogueCaptionFontSize = 36f;
    [SerializeField, Min(1f)] private float persistentSceneHeaderFontSize = 24f;

    [Header("Overlay")]
    [SerializeField, Range(0f, 1f)] private float fullscreenDecisionDimmerAlpha = 0.82f;

    public TMP_FontAsset PrimaryFontAsset { get { return primaryFontAsset; } }
    public TMP_FontAsset UiFontAsset { get { return primaryFontAsset; } }
    public TMP_FontAsset DialogueFontAsset
    {
        get { return dialogueFontAsset != null ? dialogueFontAsset : primaryFontAsset; }
    }
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
    public float TerminalDialogueSpeakerFontSize { get { return terminalDialogueSpeakerFontSize; } }
    public float TerminalDialogueBodyFontSize { get { return terminalDialogueBodyFontSize; } }
    public float TerminalDialogueAdvanceFontSize { get { return terminalDialogueAdvanceFontSize; } }
    public float CompanionIdentityHeadingFontSize { get { return companionIdentityHeadingFontSize; } }
    public float CompanionIdentityValueFontSize { get { return companionIdentityValueFontSize; } }
    public float CompanionTaskHeadingFontSize { get { return companionTaskHeadingFontSize; } }
    public float CompanionTaskBodyFontSize { get { return companionTaskBodyFontSize; } }
    public float InteractionPromptFontSize { get { return interactionPromptFontSize; } }
    public float SceneCardFontSize { get { return sceneCardFontSize; } }
    public float EpilogueCaptionFontSize { get { return epilogueCaptionFontSize; } }
    public float PersistentSceneHeaderFontSize { get { return persistentSceneHeaderFontSize; } }
    public float FullscreenDecisionDimmerAlpha { get { return fullscreenDecisionDimmerAlpha; } }

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
        terminalDialogueSpeakerFontSize = Mathf.Max(1f, terminalDialogueSpeakerFontSize);
        terminalDialogueBodyFontSize = Mathf.Max(1f, terminalDialogueBodyFontSize);
        terminalDialogueAdvanceFontSize = Mathf.Max(1f, terminalDialogueAdvanceFontSize);
        companionIdentityHeadingFontSize = Mathf.Max(1f, companionIdentityHeadingFontSize);
        companionIdentityValueFontSize = Mathf.Max(1f, companionIdentityValueFontSize);
        companionTaskHeadingFontSize = Mathf.Max(1f, companionTaskHeadingFontSize);
        companionTaskBodyFontSize = Mathf.Max(1f, companionTaskBodyFontSize);
        interactionPromptFontSize = Mathf.Max(1f, interactionPromptFontSize);
        sceneCardFontSize = Mathf.Max(1f, sceneCardFontSize);
        epilogueCaptionFontSize = Mathf.Max(1f, epilogueCaptionFontSize);
        persistentSceneHeaderFontSize = Mathf.Max(1f, persistentSceneHeaderFontSize);
        fullscreenDecisionDimmerAlpha = Mathf.Clamp01(fullscreenDecisionDimmerAlpha);
    }
}
