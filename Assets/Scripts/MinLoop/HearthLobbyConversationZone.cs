using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class HearthLobbyConversationZone :
    MonoBehaviour,
    IInteractable,
    IInteractionAvailability
{
    [SerializeField] private HearthLobbyFlowController flowController;
    [SerializeField] private HearthDialogueSequence exchangeSequence;
    [SerializeField] private HearthDialogueSequence exitCommentarySequence;
    [SerializeField] private Transform formalPlayerRoot;
    [SerializeField] private bool playOnce = true;
    [SerializeField] private bool requireInteraction = true;
    [SerializeField] private string interactionDescription = "TALK";
    [SerializeField] private bool exchangeCompleted;
    [SerializeField] private bool exitCommentaryCompleted;

    private bool playerInside;

    public bool Completed
    {
        get { return exchangeCompleted && (exitCommentarySequence == null || exitCommentaryCompleted); }
    }

    public HearthDialogueSequence ExchangeSequence
    {
        get { return exchangeSequence; }
    }

    public HearthDialogueSequence ExitCommentarySequence
    {
        get { return exitCommentarySequence; }
    }

    public bool IsInteractionAvailable
    {
        get
        {
            return enabled &&
                flowController != null &&
                exchangeSequence != null &&
                (!playOnce || !exchangeCompleted);
        }
    }

    private void Reset()
    {
        Collider trigger = GetComponent<Collider>();
        if (trigger != null)
        {
            trigger.isTrigger = true;
        }
    }

    private void OnValidate()
    {
        Collider trigger = GetComponent<Collider>();
        if (trigger != null)
        {
            trigger.isTrigger = true;
        }
    }

    private void Update()
    {
        if (!playerInside && exchangeCompleted && !exitCommentaryCompleted)
        {
            TryStartExitCommentary();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsFormalPlayer(other))
        {
            return;
        }

        playerInside = true;
        if (!requireInteraction)
        {
            TryStartConversation();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsFormalPlayer(other))
        {
            return;
        }

        playerInside = true;
        if (!requireInteraction)
        {
            TryStartConversation();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsFormalPlayer(other))
        {
            playerInside = false;
            TryStartExitCommentary();
        }
    }

    public void Configure(
        HearthLobbyFlowController flow,
        HearthDialogueSequence exchange,
        HearthDialogueSequence exitCommentary,
        Transform playerRoot,
        bool once)
    {
        flowController = flow;
        exchangeSequence = exchange;
        exitCommentarySequence = exitCommentary;
        formalPlayerRoot = playerRoot;
        playOnce = once;
    }

    public void MarkExchangeCompleted()
    {
        exchangeCompleted = true;
    }

    public void Interact()
    {
        if (!IsInteractionAvailable)
        {
            return;
        }

        TryStartConversation(true);
    }

    public string GetDescription()
    {
        return string.IsNullOrWhiteSpace(interactionDescription)
            ? "TALK"
            : interactionDescription.Trim();
    }

    public void MarkExitCommentaryCompleted()
    {
        exitCommentaryCompleted = true;
    }

    public void ResetConversation()
    {
        exchangeCompleted = false;
        exitCommentaryCompleted = false;
        playerInside = false;
    }

    private void TryStartConversation(bool requestedByInteraction = false)
    {
        if ((!playerInside && !requestedByInteraction) ||
            flowController == null ||
            exchangeSequence == null)
        {
            return;
        }

        if (requireInteraction && !requestedByInteraction)
        {
            return;
        }

        if (playOnce && exchangeCompleted)
        {
            return;
        }

        flowController.TryPlayOptionalConversation(this, exchangeSequence);
    }

    private void TryStartExitCommentary()
    {
        if (playerInside || flowController == null || exitCommentarySequence == null || !exchangeCompleted)
        {
            return;
        }

        if (playOnce && exitCommentaryCompleted)
        {
            return;
        }

        flowController.TryPlayExitCommentary(this, exitCommentarySequence);
    }

    private bool IsFormalPlayer(Collider other)
    {
        if (other == null || formalPlayerRoot == null)
        {
            return false;
        }

        Transform candidate = other.transform;
        return candidate == formalPlayerRoot || candidate.IsChildOf(formalPlayerRoot);
    }
}
