using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class Hearth17F03InspectionPanel : MonoBehaviour
{
    private enum PresentationPhase
    {
        Recall,
        FieldUnitExplanation,
        AwaitSpaceRelease,
        DispositionChoice,
        Submitted
    }

    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text detailText;
    [SerializeField] private TMP_Text recallActionText;
    [SerializeField] private Image recallHighlight;
    [SerializeField] private GameObject choiceRoot;
    [SerializeField] private Image choiceABackground;
    [SerializeField] private Image choiceBBackground;
    [SerializeField] private TMP_Text choiceAText;
    [SerializeField] private TMP_Text choiceBText;
    [SerializeField] private TMP_Text recommendedText;
    [SerializeField] private Image fullscreenSelectionDimmer;
    [SerializeField] private HearthDialogueSurface fieldUnitDialogueSurface;

    [Header("Second UI Visual")]
    [SerializeField] private bool useSecondUiVisual = true;
    [Tooltip("Migration-only compatibility. Disable after the current panel appearance has been adopted into the canonical V2 Prefab.")]
    [SerializeField] private bool applyRuntimeVisualCompatibility;
    [SerializeField] private HearthUiThemeProfile secondUiTheme;
    [SerializeField] private HearthUiStateCoordinator uiStateCoordinator;

    [Header("Content")]
    [SerializeField] private string title = "COMPANION UNIT - LOCAL INSPECTION";
    [SerializeField] private string status = "STATUS: DEEP SLEEP / REMOTE LINK UNAVAILABLE";
    [TextArea(3, 8)]
    [SerializeField] private string detail = "Core services are offline. Local inspection access is available to an authorized inspector.";
    [SerializeField] private string recallLabel = "RECALL TODAY'S EVENT   [SPACE]";
    [SerializeField] private string recallQueuedLabel = "RECALL REQUESTED   PLEASE WAIT";

    [Header("Input")]
    [SerializeField] private KeyCode confirmKey = KeyCode.Space;
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;
    [SerializeField] private KeyCode upKey = KeyCode.UpArrow;
    [SerializeField] private KeyCode downKey = KeyCode.DownArrow;

    [Header("Events")]
    [SerializeField] private UnityEvent onRecallRequested = new UnityEvent();
    [SerializeField] private UnityEvent onCloseRequested = new UnityEvent();

    private bool recallAvailable = true;
    private bool recallSubmitted;
    private bool recallQueued;
    private PresentationPhase presentationPhase;
    private int choiceIndex;
    private bool choiceSubmitted;
    private bool choiceInputEnabled;
    private bool choiceInputRequested;
    private int choiceSpaceReleasedFrame = -1;

    public event Action RecallRequested;
    public event Action CloseRequested;
    public event Action<MinLoopDispositionChoice> ChoiceSubmitted;

    public bool IsOpen { get; private set; }
    public bool RecallQueued { get { return recallQueued; } }
    public bool ChoiceInputEnabled { get { return choiceInputEnabled; } }
    public bool UsesAuthoredVisualLayout { get { return !applyRuntimeVisualCompatibility; } }
    public bool IsDispositionChoiceOpen
    {
        get
        {
            return IsOpen && presentationPhase != PresentationPhase.Recall;
        }
    }

    private void Awake()
    {
        if (applyRuntimeVisualCompatibility)
        {
            ApplySecondUiVisual();
        }
        DisableDeprecatedAuthoredVisuals();
        ApplyContent();
        Close();
    }

    private void OnDisable()
    {
        if (!IsOpen)
        {
            return;
        }

        IsOpen = false;
        SetCoordinatorModalRequest(false);
    }

    private void Update()
    {
        if (!IsOpen)
        {
            return;
        }

        if (Input.GetKeyDown(closeKey) &&
            presentationPhase == PresentationPhase.Recall)
        {
            RequestClose();
            return;
        }

        if (presentationPhase == PresentationPhase.AwaitSpaceRelease)
        {
            if (!choiceInputRequested)
            {
                return;
            }

            if (Input.GetKey(confirmKey))
            {
                choiceSpaceReleasedFrame = -1;
                return;
            }

            if (choiceSpaceReleasedFrame < 0)
            {
                // Record the first frame on which the final dialogue Space is
                // fully released. Choice input is deliberately not exposed on
                // this frame.
                choiceSpaceReleasedFrame = Time.frameCount;
                return;
            }

            if (Time.frameCount > choiceSpaceReleasedFrame)
            {
                ActivateDispositionChoice();
            }

            return;
        }

        if (presentationPhase == PresentationPhase.DispositionChoice)
        {
            if (choiceSubmitted || !choiceInputEnabled)
            {
                return;
            }

            if (Input.GetKeyDown(upKey))
            {
                MoveChoice(-1);
            }
            else if (Input.GetKeyDown(downKey))
            {
                MoveChoice(1);
            }
            else if (Input.GetKeyDown(confirmKey))
            {
                SubmitChoice();
            }

            return;
        }

        if (presentationPhase != PresentationPhase.Recall)
        {
            // Field Unit dialogue input is owned by MinLoopSubtitlePlayer.
            // Never allow the same Space press to leak into the recall action.
            return;
        }

        if (recallAvailable && recallQueued && !recallSubmitted)
        {
            ConfirmRecall();
            return;
        }

        if (!recallSubmitted && Input.GetKeyDown(confirmKey))
        {
            QueueRecallRequest();
        }
    }

    public void Configure(
        CanvasGroup group,
        TMP_Text newTitleText,
        TMP_Text newStatusText,
        TMP_Text newDetailText,
        TMP_Text newRecallActionText,
        Image newRecallHighlight)
    {
        canvasGroup = group;
        titleText = newTitleText;
        statusText = newStatusText;
        detailText = newDetailText;
        recallActionText = newRecallActionText;
        recallHighlight = newRecallHighlight;
        ApplyContent();
    }

    public void ConfigureChoiceUi(
        GameObject newChoiceRoot,
        Image newChoiceABackground,
        Image newChoiceBBackground,
        TMP_Text newChoiceAText,
        TMP_Text newChoiceBText,
        TMP_Text newRecommendedText)
    {
        choiceRoot = newChoiceRoot;
        choiceABackground = newChoiceABackground;
        choiceBBackground = newChoiceBBackground;
        choiceAText = newChoiceAText;
        choiceBText = newChoiceBText;
        recommendedText = newRecommendedText;
        RefreshModeVisuals();
    }

    public void ConfigureDispositionPresentation(
        Image dimmer,
        HearthDialogueSurface dialogueSurface)
    {
        fullscreenSelectionDimmer = dimmer;
        fieldUnitDialogueSurface = dialogueSurface;
        RefreshModeVisuals();
    }

    public HearthDialogueSurface ResolveDialogueSurface()
    {
        return fieldUnitDialogueSurface;
    }

    public void ConfigureSecondUiVisual(HearthUiThemeProfile themeProfile, bool enabled)
    {
        secondUiTheme = themeProfile;
        useSecondUiVisual = enabled;
        ApplySecondUiVisual();
        ApplyContent();
        RefreshModeVisuals();
        RefreshRecallVisual();
    }

    public void UseAuthoredVisualLayout(bool authored)
    {
        applyRuntimeVisualCompatibility = !authored;
        if (authored)
        {
            DisableDeprecatedAuthoredVisuals();
        }
    }

    public void ConfigureAuthoredHeader(string newTitle, string newStatus)
    {
        title = string.IsNullOrWhiteSpace(newTitle) ? title : newTitle;
        status = string.IsNullOrWhiteSpace(newStatus) ? status : newStatus;
        ApplyContent();
    }

    public void ApplySecondUiVisual()
    {
        if (!useSecondUiVisual || !applyRuntimeVisualCompatibility)
        {
            return;
        }

        Transform panel = transform.Find("InspectionPanel");
        if (panel == null)
        {
            return;
        }

        title = "ENTITY INSPECTION";
        status = "COMPANION UNIT 17F-03  ·  PHYSICAL UNIT FEED";

        RectTransform panelRect = panel as RectTransform;
        ApplyTopLeft(panelRect, new Rect(300f, 96f, 1320f, 840f));
        Image panelImage = panel.GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.sprite = null;
            panelImage.type = Image.Type.Simple;
            panelImage.color = WithAlpha(ThemePanelBackground, 0.72f);
            panelImage.raycastTarget = false;
        }
        DisableLegacyPanelBorders(panel);

        ConfigureText(titleText, new Rect(0f, 24f, 1320f, 48f), 30f, TextAlignmentOptions.Center, ThemePrimary, FontStyles.Bold);
        ConfigureText(statusText, new Rect(0f, 78f, 1320f, 40f), 20f, TextAlignmentOptions.Center, ThemeInformation, FontStyles.Bold);
        ConfigureText(detailText, new Rect(816f, 562f, 430f, 120f), 21f, TextAlignmentOptions.TopLeft, ThemeSecondary, FontStyles.Normal);

        if (recallHighlight != null)
        {
            ApplyTopLeft(recallHighlight.rectTransform, new Rect(420f, 700f, 480f, 72f));
            recallHighlight.sprite = null;
            recallHighlight.type = Image.Type.Simple;
            recallHighlight.color = WithAlpha(ThemeInformation, 0.28f);
            recallHighlight.raycastTarget = false;
        }
        ConfigureText(recallActionText, new Rect(0f, 0f, 480f, 72f), 22f, TextAlignmentOptions.Center, ThemePrimary, FontStyles.Bold);

        RectTransform choiceRect = choiceRoot != null ? choiceRoot.GetComponent<RectTransform>() : null;
        ApplyTopLeft(choiceRect, new Rect(180f, 584f, 960f, 176f));
        ConfigureChoiceRow(choiceABackground, new Rect(0f, 0f, 960f, 72f));
        ConfigureChoiceRow(choiceBBackground, new Rect(0f, 92f, 960f, 72f));
        ConfigureText(choiceAText, new Rect(24f, 0f, 690f, 72f), 23f, TextAlignmentOptions.MidlineLeft, ThemePrimary, FontStyles.Bold);
        ConfigureText(choiceBText, new Rect(24f, 0f, 910f, 72f), 23f, TextAlignmentOptions.MidlineLeft, ThemePrimary, FontStyles.Bold);
        ConfigureText(recommendedText, new Rect(728f, 0f, 206f, 72f), 17f, TextAlignmentOptions.Center, ThemeSuccess, FontStyles.Bold);

        EnsureRule(panel, "V2_TopRule", new Rect(0f, 0f, 1320f, 2f), ThemeInformation);
        EnsureRule(panel, "V2_LeftRule", new Rect(0f, 0f, 2f, 840f), ThemeInformation);
        EnsureLabel(
            panel,
            "V2_PhysicalFeedLabel",
            "PHYSICAL UNIT FEED",
            new Rect(42f, 142f, 700f, 34f),
            19f,
            ThemePrimary,
            TextAlignmentOptions.TopLeft);
        EnsureRule(panel, "V2_PhysicalFeedRule", new Rect(42f, 184f, 700f, 2f), ThemeInformation);
        EnsureRule(panel, "V2_CrosshairHorizontal", new Rect(330f, 390f, 120f, 2f), WithAlpha(ThemeInformation, 0.72f));
        EnsureRule(panel, "V2_CrosshairVertical", new Rect(389f, 331f, 2f, 120f), WithAlpha(ThemeInformation, 0.72f));
        EnsureStatusRow(panel, "V2_PowerState", 142f, "POWER STATE", "DEEP SLEEP");
        EnsureStatusRow(panel, "V2_MemoryArchive", 226f, "MEMORY ARCHIVE", "AVAILABLE");
        EnsureStatusRow(panel, "V2_MotorResponse", 310f, "MOTOR RESPONSE", "LOCKED");
        EnsureStatusRow(panel, "V2_LastEvent", 394f, "LAST EVENT", "22:41");
        EnsureLabel(
            panel,
            "V2_FieldUnitLabel",
            "FIELD UNIT",
            new Rect(816f, 500f, 430f, 36f),
            22f,
            ThemePrimary,
            TextAlignmentOptions.TopLeft);
        EnsureRule(panel, "V2_FieldUnitRule", new Rect(816f, 544f, 430f, 2f), ThemeInformation);
        EnsureLabel(
            panel,
            "V2_InspectionFooter",
            "LOCAL INSPECTION CHANNEL  ·  VERIFIED",
            new Rect(760f, 798f, 492f, 30f),
            17f,
            ThemeSecondary,
            TextAlignmentOptions.TopRight);
    }

    public void Open()
    {
        presentationPhase = PresentationPhase.Recall;
        ApplyContent();
        recallSubmitted = false;
        recallQueued = false;
        if (fieldUnitDialogueSurface != null)
        {
            fieldUnitDialogueSurface.HideImmediate();
        }
        IsOpen = true;
        SetCoordinatorModalRequest(true);
        SetCanvasVisible(true);
        RefreshModeVisuals();
        RefreshRecallVisual();
    }

    public void OpenDispositionChoice()
    {
        OpenDispositionChoice(true);
    }

    public void OpenDispositionChoice(bool inputEnabled)
    {
        presentationPhase = PresentationPhase.FieldUnitExplanation;
        choiceIndex = 0;
        choiceSubmitted = false;
        choiceInputEnabled = false;
        choiceInputRequested = false;
        choiceSpaceReleasedFrame = -1;
        IsOpen = true;
        SetCoordinatorModalRequest(true);

        if (titleText != null) titleText.text = "17F-03  DISPOSITION";
        if (choiceAText != null) choiceAText.text = "A  RESTART THE UNIT NOW";
        if (choiceBText != null) choiceBText.text = "B  HOLD REPAIR - 7 DAY HUMAN OBSERVATION";
        if (recommendedText != null) recommendedText.text = "RECOMMENDED";

        SetCanvasVisible(true);
        RefreshModeVisuals();
        RefreshChoiceInstruction();
        RefreshChoiceVisuals();

        if (inputEnabled)
        {
            SetChoiceInputEnabled(true);
        }
    }

    public void SetChoiceInputEnabled(bool value)
    {
        if (!IsOpen || choiceSubmitted)
        {
            choiceInputEnabled = false;
            choiceInputRequested = false;
            RefreshModeVisuals();
            return;
        }

        if (!value)
        {
            presentationPhase = PresentationPhase.FieldUnitExplanation;
            choiceInputEnabled = false;
            choiceInputRequested = false;
            choiceSpaceReleasedFrame = -1;
            RefreshModeVisuals();
            RefreshChoiceInstruction();
            RefreshChoiceVisuals();
            return;
        }

        // The last manual dialogue advance can still be held on this frame.
        // Keep the choices and their input disabled until Space has been fully
        // released, then wait one additional frame before exposing the layer.
        if (fieldUnitDialogueSurface != null)
        {
            fieldUnitDialogueSurface.HideImmediate();
        }

        presentationPhase = PresentationPhase.AwaitSpaceRelease;
        choiceInputEnabled = false;
        choiceInputRequested = true;
        choiceSpaceReleasedFrame = -1;
        RefreshModeVisuals();
        RefreshChoiceInstruction();
        RefreshChoiceVisuals();
    }

    public void Close()
    {
        IsOpen = false;
        recallSubmitted = false;
        recallQueued = false;
        choiceSubmitted = false;
        choiceInputEnabled = false;
        choiceInputRequested = false;
        choiceSpaceReleasedFrame = -1;
        presentationPhase = PresentationPhase.Recall;
        SetCoordinatorModalRequest(false);
        SetCanvasVisible(false);
    }

    public void ConfirmRecall()
    {
        if (!IsOpen || !recallAvailable || recallSubmitted)
        {
            return;
        }

        recallSubmitted = true;
        recallQueued = false;
        RefreshRecallVisual();
        onRecallRequested.Invoke();
        if (RecallRequested != null)
        {
            RecallRequested.Invoke();
        }
    }

    public void RequestClose()
    {
        if (!IsOpen)
        {
            return;
        }

        recallQueued = false;
        RefreshRecallVisual();

        onCloseRequested.Invoke();
        if (CloseRequested != null)
        {
            CloseRequested.Invoke();
        }
    }

    public void SetRecallAvailable(bool value)
    {
        recallAvailable = value;
        if (!value)
        {
            recallSubmitted = false;
        }

        RefreshRecallVisual();
    }

    public void QueueRecallRequest()
    {
        if (!IsOpen || recallSubmitted)
        {
            return;
        }

        if (recallAvailable)
        {
            ConfirmRecall();
            return;
        }

        recallQueued = true;
        RefreshRecallVisual();
    }

    public void MoveChoice(int direction)
    {
        if (!IsOpen ||
            presentationPhase != PresentationPhase.DispositionChoice ||
            choiceSubmitted ||
            !choiceInputEnabled)
        {
            return;
        }

        choiceIndex = (choiceIndex + (direction < 0 ? -1 : 1) + 2) % 2;
        RefreshChoiceVisuals();
    }

    public void SubmitChoice()
    {
        if (!IsOpen ||
            presentationPhase != PresentationPhase.DispositionChoice ||
            choiceSubmitted ||
            !choiceInputEnabled)
        {
            return;
        }

        choiceSubmitted = true;
        choiceInputEnabled = false;
        choiceInputRequested = false;
        choiceSpaceReleasedFrame = -1;
        presentationPhase = PresentationPhase.Submitted;
        RefreshChoiceInstruction();
        RefreshChoiceVisuals();
        RefreshModeVisuals();
        if (ChoiceSubmitted != null)
        {
            ChoiceSubmitted.Invoke(choiceIndex == 0
                ? MinLoopDispositionChoice.SystemRecommendedA
                : MinLoopDispositionChoice.LowInterventionB);
        }
    }

    /// <summary>
    /// Re-enables the authored choice UI when the owning story controller
    /// rejects a submission. This prevents the panel from remaining frozen
    /// after a recoverable flow/state mismatch.
    /// </summary>
    public void RestoreChoiceInputAfterRejectedSubmission()
    {
        if (!IsOpen || presentationPhase == PresentationPhase.Recall)
        {
            return;
        }

        choiceSubmitted = false;
        choiceInputEnabled = true;
        choiceInputRequested = false;
        choiceSpaceReleasedFrame = -1;
        presentationPhase = PresentationPhase.DispositionChoice;
        RefreshModeVisuals();
        RefreshChoiceInstruction();
        RefreshChoiceVisuals();
    }

    private void ApplyContent()
    {
        if (titleText != null) titleText.text = title;
        if (statusText != null) statusText.text = status;
        if (detailText != null) detailText.text = detail;
        if (recallActionText != null) recallActionText.text = recallQueued ? recallQueuedLabel : recallLabel;
    }

    private void RefreshRecallVisual()
    {
        float alpha = recallAvailable && !recallSubmitted ? 1f : recallQueued ? 0.58f : 0.35f;
        if (recallActionText != null)
        {
            recallActionText.text = recallQueued ? recallQueuedLabel : recallLabel;
            Color color = recallActionText.color;
            color.a = alpha;
            recallActionText.color = color;
        }

        if (recallHighlight != null)
        {
            Color color = recallHighlight.color;
            color.a = recallAvailable && !recallSubmitted ? 0.28f : recallQueued ? 0.18f : 0.08f;
            recallHighlight.color = color;
        }
    }

    private void RefreshModeVisuals()
    {
        bool recallMode = presentationPhase == PresentationPhase.Recall;
        bool explanationMode =
            presentationPhase == PresentationPhase.FieldUnitExplanation;
        bool choiceMode =
            presentationPhase == PresentationPhase.DispositionChoice;
        if (fullscreenSelectionDimmer != null)
        {
            fullscreenSelectionDimmer.gameObject.SetActive(
                IsOpen && choiceMode);
        }
        if (recallHighlight != null)
        {
            recallHighlight.gameObject.SetActive(recallMode);
        }

        if (choiceRoot != null)
        {
            // Do not expose the choices while the Field Unit is still
            // explaining the disposition. The choice layer appears only
            // when input is genuinely enabled, above the disposition dimmer.
            choiceRoot.SetActive(
                IsOpen && choiceMode && !choiceSubmitted && choiceInputEnabled);
        }

        if (detailText != null)
        {
            // The authored choice rows already contain their own instruction
            // treatment. Retire the old PLEASE WAIT / UP-DOWN overlay that
            // used to intersect the data rows and dialogue surface.
            detailText.gameObject.SetActive(false);
        }

        if (fieldUnitDialogueSurface != null)
        {
            if (!explanationMode && fieldUnitDialogueSurface.IsVisible)
            {
                fieldUnitDialogueSurface.HideImmediate();
            }

            if (explanationMode)
            {
                fieldUnitDialogueSurface.transform.SetAsLastSibling();
            }
        }

        // Authored hierarchy contract: base inspection content, dialogue,
        // selection dimmer, then AB choices. Reassert only the active phase so
        // a hidden layer can never cover the visible one.
        if (choiceMode)
        {
            if (fullscreenSelectionDimmer != null)
            {
                fullscreenSelectionDimmer.transform.SetAsLastSibling();
            }
            if (choiceRoot != null)
            {
                choiceRoot.transform.SetAsLastSibling();
            }
        }
    }

    private void RefreshChoiceVisuals()
    {
        float selectedAlpha = choiceSubmitted ? 0.22f : choiceInputEnabled ? 0.62f : 0.32f;
        float idleAlpha = choiceSubmitted ? 0.12f : choiceInputEnabled ? 0.42f : 0.22f;
        Color selected = WithAlpha(ThemeSuccess, selectedAlpha);
        Color idle = WithAlpha(ThemePanelBackground, idleAlpha);
        if (choiceABackground != null) choiceABackground.color = choiceIndex == 0 ? selected : idle;
        if (choiceBBackground != null) choiceBBackground.color = choiceIndex == 1 ? selected : idle;

        float alpha = choiceSubmitted ? 0.42f : choiceInputEnabled ? 1f : 0.58f;
        SetTextAlpha(choiceAText, alpha);
        SetTextAlpha(choiceBText, alpha);
        SetTextAlpha(recommendedText, choiceSubmitted ? 0.35f : choiceInputEnabled ? 1f : 0.58f);
    }

    private void RefreshChoiceInstruction()
    {
        if (presentationPhase == PresentationPhase.Recall)
        {
            return;
        }

        if (choiceSubmitted)
        {
            if (statusText != null) statusText.text = "STATUS: DISPOSITION SUBMITTED";
            if (detailText != null)
            {
                detailText.text = string.Empty;
                detailText.gameObject.SetActive(false);
            }
            return;
        }

        if (presentationPhase == PresentationPhase.DispositionChoice &&
            choiceInputEnabled)
        {
            if (statusText != null) statusText.text = "STATUS: INPUT ENABLED";
            if (detailText != null) detailText.text = "UP / DOWN  SELECT     SPACE  CONFIRM";
        }
        else
        {
            if (statusText != null) statusText.text = "STATUS: INPUT LOCKED - FIELD REVIEW IN PROGRESS";
            if (detailText != null) detailText.text = "PLEASE WAIT";
        }
    }

    private void ActivateDispositionChoice()
    {
        if (!IsOpen || choiceSubmitted || !choiceInputRequested)
        {
            return;
        }

        presentationPhase = PresentationPhase.DispositionChoice;
        choiceInputRequested = false;
        choiceInputEnabled = true;
        choiceSpaceReleasedFrame = -1;
        RefreshModeVisuals();
        RefreshChoiceInstruction();
        RefreshChoiceVisuals();
    }

    private void DisableDeprecatedAuthoredVisuals()
    {
        string[] retiredNames =
        {
            "V2_PhysicalFeedLabel",
            "V2_PhysicalFeedRule",
            "V2_CrosshairHorizontal",
            "V2_CrosshairVertical",
            "V2_FieldUnitLabel",
            "V2_FieldUnitRule",
            "V2_InspectionFooter",
            "V2_LeftRule"
        };

        Transform[] descendants = GetComponentsInChildren<Transform>(true);
        for (int nameIndex = 0; nameIndex < retiredNames.Length; nameIndex++)
        {
            for (int transformIndex = 0; transformIndex < descendants.Length; transformIndex++)
            {
                Transform candidate = descendants[transformIndex];
                if (candidate != null && candidate.name == retiredNames[nameIndex])
                {
                    candidate.gameObject.SetActive(false);
                }
            }
        }
    }

    private static void SetTextAlpha(TMP_Text text, float alpha)
    {
        if (text == null)
        {
            return;
        }

        Color color = text.color;
        color.a = alpha;
        text.color = color;
    }

    private void SetCanvasVisible(bool visible)
    {
        if (canvasGroup == null)
        {
            gameObject.SetActive(visible);
            return;
        }

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private Color ThemePanelBackground
    {
        get
        {
            return secondUiTheme != null
                ? secondUiTheme.TerminalPanelBackground
                : (Color)new Color32(9, 16, 28, 255);
        }
    }

    private Color ThemeSecondary
    {
        get
        {
            return secondUiTheme != null
                ? secondUiTheme.Secondary
                : (Color)new Color32(95, 120, 149, 255);
        }
    }

    private Color ThemePrimary
    {
        get
        {
            return secondUiTheme != null
                ? secondUiTheme.Primary
                : (Color)new Color32(215, 230, 246, 255);
        }
    }

    private Color ThemeInformation
    {
        get
        {
            return secondUiTheme != null
                ? secondUiTheme.Information
                : (Color)new Color32(120, 170, 220, 255);
        }
    }

    private Color ThemeSuccess
    {
        get
        {
            return secondUiTheme != null
                ? secondUiTheme.Success
                : (Color)new Color32(87, 184, 138, 255);
        }
    }

    private void ConfigureChoiceRow(Image background, Rect rect)
    {
        if (background == null)
        {
            return;
        }

        ApplyTopLeft(background.rectTransform, rect);
        background.sprite = null;
        background.type = Image.Type.Simple;
        background.color = WithAlpha(ThemePanelBackground, 0.54f);
        background.raycastTarget = false;
    }

    private void ConfigureText(
        TMP_Text text,
        Rect rect,
        float fontSize,
        TextAlignmentOptions alignment,
        Color color,
        FontStyles fontStyle)
    {
        if (text == null)
        {
            return;
        }

        ApplyTopLeft(text.rectTransform, rect);
        if (secondUiTheme != null && secondUiTheme.PrimaryFontAsset != null)
        {
            text.font = secondUiTheme.PrimaryFontAsset;
        }
        text.enableAutoSizing = false;
        text.fontSize = fontSize;
        text.fontSizeMin = fontSize;
        text.fontSizeMax = fontSize;
        text.enableWordWrapping = true;
        text.maxVisibleLines = int.MaxValue;
        text.overflowMode = TextOverflowModes.Overflow;
        text.alignment = alignment;
        text.fontStyle = fontStyle;
        text.color = color;
        text.raycastTarget = false;
    }

    private void EnsureRule(Transform parent, string objectName, Rect rect, Color color)
    {
        Transform child = parent.Find(objectName);
        if (child == null)
        {
            GameObject target = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            target.transform.SetParent(parent, false);
            child = target.transform;
        }

        Image image = child.GetComponent<Image>();
        if (image == null)
        {
            image = child.gameObject.AddComponent<Image>();
        }

        ApplyTopLeft(image.rectTransform, rect);
        image.sprite = null;
        image.type = Image.Type.Simple;
        image.color = color;
        image.raycastTarget = false;
    }

    private void EnsureLabel(
        Transform parent,
        string objectName,
        string value,
        Rect rect,
        float fontSize,
        Color color,
        TextAlignmentOptions alignment)
    {
        Transform child = parent.Find(objectName);
        if (child == null)
        {
            GameObject target = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            target.transform.SetParent(parent, false);
            child = target.transform;
        }

        TMP_Text text = child.GetComponent<TMP_Text>();
        if (text == null)
        {
            text = child.gameObject.AddComponent<TextMeshProUGUI>();
        }

        text.text = value;
        ConfigureText(text, rect, fontSize, alignment, color, FontStyles.Bold);
    }

    private void EnsureStatusRow(
        Transform parent,
        string objectName,
        float y,
        string label,
        string value)
    {
        Transform row = parent.Find(objectName);
        if (row == null)
        {
            GameObject target = new GameObject(
                objectName,
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            target.transform.SetParent(parent, false);
            row = target.transform;
        }

        Image background = row.GetComponent<Image>();
        if (background == null)
        {
            background = row.gameObject.AddComponent<Image>();
        }

        ApplyTopLeft(background.rectTransform, new Rect(790f, y, 462f, 64f));
        background.sprite = null;
        background.type = Image.Type.Simple;
        background.color = WithAlpha(ThemePanelBackground, 0.68f);
        background.raycastTarget = false;
        EnsureRule(row, "Rule", new Rect(0f, 0f, 2f, 64f), ThemeInformation);
        EnsureLabel(
            row,
            "Label",
            label,
            new Rect(24f, 18f, 250f, 32f),
            18f,
            ThemePrimary,
            TextAlignmentOptions.MidlineLeft);
        EnsureLabel(
            row,
            "Value",
            value,
            new Rect(280f, 18f, 154f, 32f),
            18f,
            ThemeSecondary,
            TextAlignmentOptions.MidlineRight);
    }

    private void SetCoordinatorModalRequest(bool visible)
    {
        if (uiStateCoordinator == null)
        {
            uiStateCoordinator =
                FindObjectOfType<HearthUiStateCoordinator>(true);
        }

        if (uiStateCoordinator != null)
        {
            uiStateCoordinator.SetExternalModalRequest(this, visible);
        }
    }

    private static void DisableLegacyPanelBorders(Transform panel)
    {
        if (panel == null)
        {
            return;
        }

        for (int i = 0; i < panel.childCount; i++)
        {
            Transform child = panel.GetChild(i);
            if (child != null && child.name.StartsWith("Border", StringComparison.Ordinal))
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    private static void ApplyTopLeft(RectTransform target, Rect rect)
    {
        if (target == null)
        {
            return;
        }

        target.anchorMin = new Vector2(0f, 1f);
        target.anchorMax = new Vector2(0f, 1f);
        target.pivot = new Vector2(0f, 1f);
        target.anchoredPosition = new Vector2(rect.x, -rect.y);
        target.sizeDelta = new Vector2(rect.width, rect.height);
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }
}
