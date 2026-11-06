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
    [SerializeField] private GameObject inspectionContentRoot;
    [SerializeField] private Image choiceABackground;
    [SerializeField] private Image choiceBBackground;
    [SerializeField] private TMP_Text choiceAText;
    [SerializeField] private TMP_Text choiceBText;
    [SerializeField] private TMP_Text recommendedText;
    [SerializeField] private Image fullscreenSelectionDimmer;
    [SerializeField] private HearthDialogueSurface fieldUnitDialogueSurface;

    [Header("UI Theme")]
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
    public bool IsDispositionChoiceOpen
    {
        get
        {
            return IsOpen && presentationPhase != PresentationPhase.Recall;
        }
    }

    private void Awake()
    {
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
        if (fieldUnitDialogueSurface != null)
        {
            fieldUnitDialogueSurface.ApplyFonts(secondUiTheme);
        }
        RefreshModeVisuals();
    }

    public HearthDialogueSurface ResolveDialogueSurface()
    {
        return fieldUnitDialogueSurface;
    }

    public void ConfigureAuthoredHeader(string newTitle, string newStatus)
    {
        title = string.IsNullOrWhiteSpace(newTitle) ? title : newTitle;
        status = string.IsNullOrWhiteSpace(newStatus) ? status : newStatus;
        ApplyContent();
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
            color.a = 1f;
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
        if (inspectionContentRoot != null)
            inspectionContentRoot.SetActive(IsOpen && recallMode);
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
        float selectedAlpha = 1f;
        float idleAlpha = 1f;
        Color selected = WithAlpha(ThemeSuccess, selectedAlpha);
        Color idle = WithAlpha(ThemePanelBackground, idleAlpha);
        if (choiceABackground != null) choiceABackground.color = choiceIndex == 0 ? selected : idle;
        if (choiceBBackground != null) choiceBBackground.color = choiceIndex == 1 ? selected : idle;
        if (choiceABackground != null) HearthReachControl.Focus(choiceABackground.transform, choiceIndex == 0);
        if (choiceBBackground != null) HearthReachControl.Focus(choiceBBackground.transform, choiceIndex == 1);

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

    private Color ThemeSuccess
    {
        get
        {
            return secondUiTheme != null
                ? secondUiTheme.Success
                : (Color)new Color32(87, 184, 138, 255);
        }
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

    private static Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }
}
