using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class Hearth17F03InspectionPanel : MonoBehaviour
{
    private enum PanelMode
    {
        Recall,
        DispositionChoice
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
    private PanelMode panelMode;
    private int choiceIndex;
    private bool choiceSubmitted;
    private bool choiceInputEnabled;

    public event Action RecallRequested;
    public event Action CloseRequested;
    public event Action<MinLoopDispositionChoice> ChoiceSubmitted;

    public bool IsOpen { get; private set; }
    public bool RecallQueued { get { return recallQueued; } }
    public bool ChoiceInputEnabled { get { return choiceInputEnabled; } }

    private void Awake()
    {
        ApplyContent();
        Close();
    }

    private void Update()
    {
        if (!IsOpen)
        {
            return;
        }

        if (Input.GetKeyDown(closeKey) && panelMode == PanelMode.Recall)
        {
            RequestClose();
            return;
        }

        if (panelMode == PanelMode.DispositionChoice)
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

    public void Open()
    {
        panelMode = PanelMode.Recall;
        ApplyContent();
        recallSubmitted = false;
        recallQueued = false;
        IsOpen = true;
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
        panelMode = PanelMode.DispositionChoice;
        choiceIndex = 0;
        choiceSubmitted = false;
        choiceInputEnabled = inputEnabled;
        IsOpen = true;

        if (titleText != null) titleText.text = "17F-03  DISPOSITION";
        if (choiceAText != null) choiceAText.text = "A  RESTART THE UNIT NOW";
        if (choiceBText != null) choiceBText.text = "B  HOLD REPAIR - 7 DAY HUMAN OBSERVATION";
        if (recommendedText != null) recommendedText.text = "RECOMMENDED";

        SetCanvasVisible(true);
        RefreshModeVisuals();
        RefreshChoiceInstruction();
        RefreshChoiceVisuals();
    }

    public void SetChoiceInputEnabled(bool value)
    {
        choiceInputEnabled = value && !choiceSubmitted;
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
        if (!IsOpen || panelMode != PanelMode.DispositionChoice || choiceSubmitted || !choiceInputEnabled)
        {
            return;
        }

        choiceIndex = (choiceIndex + (direction < 0 ? -1 : 1) + 2) % 2;
        RefreshChoiceVisuals();
    }

    public void SubmitChoice()
    {
        if (!IsOpen || panelMode != PanelMode.DispositionChoice || choiceSubmitted || !choiceInputEnabled)
        {
            return;
        }

        choiceSubmitted = true;
        choiceInputEnabled = false;
        RefreshChoiceInstruction();
        RefreshChoiceVisuals();
        if (ChoiceSubmitted != null)
        {
            ChoiceSubmitted.Invoke(choiceIndex == 0
                ? MinLoopDispositionChoice.SystemRecommendedA
                : MinLoopDispositionChoice.LowInterventionB);
        }
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
        bool recallMode = panelMode == PanelMode.Recall;
        if (recallHighlight != null)
        {
            recallHighlight.gameObject.SetActive(recallMode);
        }

        if (choiceRoot != null)
        {
            choiceRoot.SetActive(!recallMode);
        }
    }

    private void RefreshChoiceVisuals()
    {
        float selectedAlpha = choiceSubmitted ? 0.22f : choiceInputEnabled ? 0.62f : 0.32f;
        float idleAlpha = choiceSubmitted ? 0.12f : choiceInputEnabled ? 0.42f : 0.22f;
        Color selected = new Color(0.12f, 0.46f, 0.31f, selectedAlpha);
        Color idle = new Color(0.03f, 0.10f, 0.14f, idleAlpha);
        if (choiceABackground != null) choiceABackground.color = choiceIndex == 0 ? selected : idle;
        if (choiceBBackground != null) choiceBBackground.color = choiceIndex == 1 ? selected : idle;

        float alpha = choiceSubmitted ? 0.42f : choiceInputEnabled ? 1f : 0.58f;
        SetTextAlpha(choiceAText, alpha);
        SetTextAlpha(choiceBText, alpha);
        SetTextAlpha(recommendedText, choiceSubmitted ? 0.35f : choiceInputEnabled ? 1f : 0.58f);
    }

    private void RefreshChoiceInstruction()
    {
        if (panelMode != PanelMode.DispositionChoice)
        {
            return;
        }

        if (choiceSubmitted)
        {
            if (statusText != null) statusText.text = "STATUS: DISPOSITION SUBMITTED";
            if (detailText != null) detailText.text = "PLEASE WAIT";
            return;
        }

        if (choiceInputEnabled)
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
}
