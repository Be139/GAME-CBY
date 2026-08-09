using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class HearthLobbyHudOverlay : MonoBehaviour
{
    [SerializeField] private CanvasGroup externalPresentationGroup;
    [SerializeField] private CanvasGroup activationGroup;
    [SerializeField] private CanvasGroup expandedMessageGroup;
    [SerializeField] private CanvasGroup pinnedMessageGroup;
    [SerializeField] private TMP_Text assignmentStatusText;

    private bool voiceMessageDismissed;

    private void Awake()
    {
        ResolveExternalPresentationGroup();
        ApplyApprovedLayout();
        HideAllImmediate();
    }

    public void Configure(
        CanvasGroup activation,
        CanvasGroup expandedMessage,
        CanvasGroup pinnedMessage,
        TMP_Text assignmentStatus)
    {
        activationGroup = activation;
        expandedMessageGroup = expandedMessage;
        pinnedMessageGroup = pinnedMessage;
        assignmentStatusText = assignmentStatus;
        ApplyApprovedLayout();
        HideAllImmediate();
    }

    public void HideAllImmediate()
    {
        voiceMessageDismissed = false;
        SetGroup(activationGroup, false);
        SetGroup(expandedMessageGroup, false);
        SetGroup(pinnedMessageGroup, false);
        SetAssignmentLoaded(false);
    }

    public void ShowActivation()
    {
        SetGroup(activationGroup, true);
    }

    public void HideActivation()
    {
        SetGroup(activationGroup, false);
    }

    public void ShowExpandedVoiceMessage()
    {
        if (voiceMessageDismissed)
        {
            return;
        }

        SetGroup(expandedMessageGroup, true);
        SetGroup(pinnedMessageGroup, false);
    }

    public void CollapseVoiceMessage()
    {
        if (voiceMessageDismissed)
        {
            return;
        }

        SetGroup(expandedMessageGroup, false);
        // The expanded incoming-message card is the only persistent visual
        // required by the approved lobby flow. After it closes, dialogue may
        // continue but the old compact READ / assignment status card must not
        // remain in the top-right corner.
        SetGroup(pinnedMessageGroup, false);
    }

    public void DismissVoiceMessage()
    {
        voiceMessageDismissed = true;
        SetGroup(expandedMessageGroup, false);
        SetGroup(pinnedMessageGroup, false);
    }

    public void SetAssignmentLoaded(bool loaded)
    {
        if (assignmentStatusText != null)
        {
            assignmentStatusText.text = loaded
                ? "ASSIGNMENT LOADED  /  FLOOR 17"
                : "ASSIGNMENT NOT LOADED";
        }
    }

    public void SetExternalPresentationSuppressed(bool suppressed)
    {
        ResolveExternalPresentationGroup();
        if (externalPresentationGroup == null)
        {
            return;
        }

        externalPresentationGroup.alpha = suppressed ? 0f : 1f;
        externalPresentationGroup.interactable = false;
        externalPresentationGroup.blocksRaycasts = false;
    }

    private void ResolveExternalPresentationGroup()
    {
        if (externalPresentationGroup == null)
        {
            externalPresentationGroup = GetComponent<CanvasGroup>();
        }

        if (externalPresentationGroup == null)
        {
            externalPresentationGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    private void ApplyApprovedLayout()
    {
        SetTopLeft(
            activationGroup != null
                ? activationGroup.transform as RectTransform
                : null,
            64f,
            178f,
            620f,
            140f);
        SetTopRight(
            expandedMessageGroup != null
                ? expandedMessageGroup.transform as RectTransform
                : null,
            64f,
            150f,
            540f,
            300f);
        SetTopRight(
            pinnedMessageGroup != null
                ? pinnedMessageGroup.transform as RectTransform
                : null,
            64f,
            150f,
            540f,
            84f);

        if (assignmentStatusText != null)
        {
            assignmentStatusText.alignment =
                TextAlignmentOptions.TopRight;
            assignmentStatusText.enableAutoSizing = false;
            assignmentStatusText.overflowMode =
                TextOverflowModes.Overflow;
        }
    }

    private static void SetTopLeft(
        RectTransform rect,
        float x,
        float y,
        float width,
        float height)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = Vector2.up;
        rect.anchorMax = Vector2.up;
        rect.pivot = Vector2.up;
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void SetTopRight(
        RectTransform rect,
        float right,
        float y,
        float width,
        float height)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = Vector2.one;
        rect.anchorMax = Vector2.one;
        rect.pivot = Vector2.one;
        rect.anchoredPosition = new Vector2(-right, -y);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void SetGroup(CanvasGroup group, bool visible)
    {
        if (group == null)
        {
            return;
        }

        group.alpha = visible ? 1f : 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
        group.gameObject.SetActive(true);
    }
}
