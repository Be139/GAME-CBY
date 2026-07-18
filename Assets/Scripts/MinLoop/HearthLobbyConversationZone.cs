using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class HearthLobbyConversationZone : MonoBehaviour
{
    [SerializeField] private HearthLobbyFlowController flowController;
    [SerializeField] private HearthDialogueSequence dialogueSequence;
    [SerializeField] private Transform formalPlayerRoot;
    [SerializeField] private bool playOnce = true;
    [SerializeField] private bool completed;

    private bool playerInside;

    public bool Completed
    {
        get { return completed; }
    }

    public HearthDialogueSequence DialogueSequence
    {
        get { return dialogueSequence; }
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

    private void OnTriggerEnter(Collider other)
    {
        if (!IsFormalPlayer(other))
        {
            return;
        }

        playerInside = true;
        TryStartConversation();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsFormalPlayer(other))
        {
            return;
        }

        playerInside = true;
        TryStartConversation();
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsFormalPlayer(other))
        {
            playerInside = false;
        }
    }

    public void Configure(
        HearthLobbyFlowController flow,
        HearthDialogueSequence sequence,
        Transform playerRoot,
        bool once)
    {
        flowController = flow;
        dialogueSequence = sequence;
        formalPlayerRoot = playerRoot;
        playOnce = once;
    }

    public void MarkCompleted()
    {
        completed = true;
    }

    public void ResetConversation()
    {
        completed = false;
        playerInside = false;
    }

    private void TryStartConversation()
    {
        if (!playerInside || flowController == null || dialogueSequence == null)
        {
            return;
        }

        if (playOnce && completed)
        {
            return;
        }

        flowController.TryPlayOptionalConversation(this, dialogueSequence);
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
