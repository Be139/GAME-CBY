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
        SetGroup(pinnedMessageGroup, true);
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
