using UnityEngine;

[CreateAssetMenu(
    fileName = "CompanionHudScene",
    menuName = "Hearth/HUD/Companion Scene Data")]
public class HearthCompanionHudSceneData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string sceneId = "17F01_01";
    [SerializeField] private int slideNumber = 1;
    [SerializeField] private string residentId = "17F-01";
    [SerializeField] private HearthCompanionHudTemplate template = HearthCompanionHudTemplate.Standard;
    [SerializeField] private HearthCompanionSpecialEffect specialEffect = HearthCompanionSpecialEffect.None;

    [Header("Mode")]
    [SerializeField] private string modeLabel = "COMPANION UNIT - FIRST PERSON - MONITORING MODE";
    [SerializeField] private Color accentColor = new Color(0.48f, 0.86f, 1f, 1f);

    [Header("Left Status Panel")]
    [SerializeField] private string statusTitle = "SUBJECT - MONITORING";
    [SerializeField] private HearthCompanionMetricLine[] statusLines;
    [SerializeField] private string statusFooter;

    [Header("Right Decision Panel")]
    [SerializeField] private string decisionKicker = "SYNTH VOICE - DECISION";
    [SerializeField] private string decisionTitle = "Initiate Soothing Sequence";
    [TextArea(2, 5)]
    [SerializeField] private string decisionBody;

    [Header("Data Stream")]
    [TextArea(1, 2)]
    [SerializeField] private string dataStreamTitle = "//monitor bus - streaming";
    [SerializeField] private string[] dataStreamLines;

    [Header("Interaction")]
    [SerializeField] private bool showHoldPrompt;
    [SerializeField] private string holdPromptText = "[ Approach Bedside - Watch Over Subject ]";
    [SerializeField] private KeyCode holdKey = KeyCode.E;
    [SerializeField] private float holdSeconds = 1.5f;
    [SerializeField] private bool showDirectionGuide;
    [SerializeField] private string directionGuideText = "FACE TARGET";

    [Header("Projection / Center")]
    [SerializeField] private string projectionTitle;
    [TextArea(4, 12)]
    [SerializeField] private string projectionBody;
    [SerializeField] private string centerMessage;

    [Header("Timed Cards")]
    [SerializeField] private bool showTriggerCard;
    [SerializeField] private string triggerCardTitle;
    [TextArea(2, 6)]
    [SerializeField] private string triggerCardBody;
    [SerializeField] private float triggerCardDelay = 0.25f;
    [SerializeField] private float triggerCardSeconds = 3.5f;

    [Header("Black Audio / Special")]
    [SerializeField] private bool autoPlaySpecialEffect;
    [SerializeField] private string specialTitle;
    [TextArea(2, 8)]
    [SerializeField] private string specialBody;
    [SerializeField] private string specialStatusLabel;
    [SerializeField] private float specialDuration = 1.5f;

    public string SceneId { get { return sceneId; } }
    public int SlideNumber { get { return slideNumber; } }
    public string ResidentId { get { return residentId; } }
    public HearthCompanionHudTemplate Template { get { return template; } }
    public HearthCompanionSpecialEffect SpecialEffect { get { return specialEffect; } }
    public string ModeLabel { get { return modeLabel; } }
    public Color AccentColor { get { return accentColor; } }
    public string StatusTitle { get { return statusTitle; } }
    public HearthCompanionMetricLine[] StatusLines { get { return statusLines; } }
    public string StatusFooter { get { return statusFooter; } }
    public string DecisionKicker { get { return decisionKicker; } }
    public string DecisionTitle { get { return decisionTitle; } }
    public string DecisionBody { get { return decisionBody; } }
    public string DataStreamTitle { get { return dataStreamTitle; } }
    public string[] DataStreamLines { get { return dataStreamLines; } }
    public bool ShowHoldPrompt { get { return showHoldPrompt; } }
    public string HoldPromptText { get { return holdPromptText; } }
    public KeyCode HoldKey { get { return holdKey; } }
    public float HoldSeconds { get { return holdSeconds; } }
    public bool ShowDirectionGuide { get { return showDirectionGuide; } }
    public string DirectionGuideText { get { return directionGuideText; } }
    public string ProjectionTitle { get { return projectionTitle; } }
    public string ProjectionBody { get { return projectionBody; } }
    public string CenterMessage { get { return centerMessage; } }
    public bool ShowTriggerCard { get { return showTriggerCard; } }
    public string TriggerCardTitle { get { return triggerCardTitle; } }
    public string TriggerCardBody { get { return triggerCardBody; } }
    public float TriggerCardDelay { get { return triggerCardDelay; } }
    public float TriggerCardSeconds { get { return triggerCardSeconds; } }
    public string SpecialTitle { get { return specialTitle; } }
    public bool AutoPlaySpecialEffect { get { return autoPlaySpecialEffect; } }
    public string SpecialBody { get { return specialBody; } }
    public string SpecialStatusLabel { get { return specialStatusLabel; } }
    public float SpecialDuration { get { return specialDuration; } }

    public void Configure(
        string newSceneId,
        int newSlideNumber,
        string newResidentId,
        HearthCompanionHudTemplate newTemplate,
        HearthCompanionSpecialEffect newSpecialEffect,
        string newModeLabel,
        Color newAccentColor,
        string newStatusTitle,
        HearthCompanionMetricLine[] newStatusLines,
        string newStatusFooter,
        string newDecisionKicker,
        string newDecisionTitle,
        string newDecisionBody,
        string newDataStreamTitle,
        string[] newDataStreamLines,
        bool newShowHoldPrompt,
        string newHoldPromptText,
        KeyCode newHoldKey,
        float newHoldSeconds,
        bool newShowDirectionGuide,
        string newDirectionGuideText,
        string newProjectionTitle,
        string newProjectionBody,
        string newCenterMessage,
        bool newShowTriggerCard,
        string newTriggerCardTitle,
        string newTriggerCardBody,
        float newTriggerCardDelay,
        float newTriggerCardSeconds,
        bool newAutoPlaySpecialEffect,
        string newSpecialTitle,
        string newSpecialBody,
        string newSpecialStatusLabel,
        float newSpecialDuration)
    {
        sceneId = newSceneId;
        slideNumber = newSlideNumber;
        residentId = newResidentId;
        template = newTemplate;
        specialEffect = newSpecialEffect;
        modeLabel = newModeLabel;
        accentColor = newAccentColor;
        statusTitle = newStatusTitle;
        statusLines = newStatusLines;
        statusFooter = newStatusFooter;
        decisionKicker = newDecisionKicker;
        decisionTitle = newDecisionTitle;
        decisionBody = newDecisionBody;
        dataStreamTitle = newDataStreamTitle;
        dataStreamLines = newDataStreamLines;
        showHoldPrompt = newShowHoldPrompt;
        holdPromptText = newHoldPromptText;
        holdKey = newHoldKey;
        holdSeconds = Mathf.Max(0.1f, newHoldSeconds);
        showDirectionGuide = newShowDirectionGuide;
        directionGuideText = newDirectionGuideText;
        projectionTitle = newProjectionTitle;
        projectionBody = newProjectionBody;
        centerMessage = newCenterMessage;
        showTriggerCard = newShowTriggerCard;
        triggerCardTitle = newTriggerCardTitle;
        triggerCardBody = newTriggerCardBody;
        triggerCardDelay = Mathf.Max(0f, newTriggerCardDelay);
        triggerCardSeconds = Mathf.Max(0f, newTriggerCardSeconds);
        autoPlaySpecialEffect = newAutoPlaySpecialEffect;
        specialTitle = newSpecialTitle;
        specialBody = newSpecialBody;
        specialStatusLabel = newSpecialStatusLabel;
        specialDuration = Mathf.Max(0f, newSpecialDuration);
    }
}
