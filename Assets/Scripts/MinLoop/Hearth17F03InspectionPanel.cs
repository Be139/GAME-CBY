using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class Hearth17F03InspectionPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text detailText;
    [SerializeField] private TMP_Text recallActionText;
    [SerializeField] private Image recallHighlight;

    [Header("Content")]
    [SerializeField] private string title = "COMPANION UNIT - LOCAL INSPECTION";
    [SerializeField] private string status = "STATUS: DEEP SLEEP / REMOTE LINK UNAVAILABLE";
    [TextArea(3, 8)]
    [SerializeField] private string detail = "Core services are offline. Local inspection access is available to an authorized inspector.";
    [SerializeField] private string recallLabel = "RECALL TODAY'S EVENT   [SPACE]";

    [Header("Input")]
    [SerializeField] private KeyCode confirmKey = KeyCode.Space;
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;

    [Header("Events")]
    [SerializeField] private UnityEvent onRecallRequested = new UnityEvent();
    [SerializeField] private UnityEvent onCloseRequested = new UnityEvent();

    private bool recallAvailable = true;
    private bool recallSubmitted;

    public event Action RecallRequested;
    public event Action CloseRequested;

    public bool IsOpen { get; private set; }

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

        if (Input.GetKeyDown(closeKey))
        {
            RequestClose();
            return;
        }

        if (recallAvailable && !recallSubmitted && Input.GetKeyDown(confirmKey))
        {
            ConfirmRecall();
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

    public void Open()
    {
        ApplyContent();
        recallSubmitted = false;
        IsOpen = true;
        SetCanvasVisible(true);
        RefreshRecallVisual();
    }

    public void Close()
    {
        IsOpen = false;
        recallSubmitted = false;
        SetCanvasVisible(false);
    }

    public void ConfirmRecall()
    {
        if (!IsOpen || !recallAvailable || recallSubmitted)
        {
            return;
        }

        recallSubmitted = true;
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

    private void ApplyContent()
    {
        if (titleText != null) titleText.text = title;
        if (statusText != null) statusText.text = status;
        if (detailText != null) detailText.text = detail;
        if (recallActionText != null) recallActionText.text = recallLabel;
    }

    private void RefreshRecallVisual()
    {
        float alpha = recallAvailable && !recallSubmitted ? 1f : 0.35f;
        if (recallActionText != null)
        {
            Color color = recallActionText.color;
            color.a = alpha;
            recallActionText.color = color;
        }

        if (recallHighlight != null)
        {
            Color color = recallHighlight.color;
            color.a = recallAvailable && !recallSubmitted ? 0.28f : 0.08f;
            recallHighlight.color = color;
        }
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
