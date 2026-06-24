using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour, IInteractable, IDialogueSource
{
    [Header("Interaction")]
    [SerializeField] private string interactionDescription = "Talk";

    [Header("Speakers")]
    [SerializeField] private string speakerOneName = "Player";
    [SerializeField] private string speakerTwoName = "NPC";

    [Header("Dialogue")]
    [SerializeField] private List<DialogueLine> dialogueLines = new List<DialogueLine>();

    public void Interact()
    {
        TriggerDialogue();
        // shijian
    }

    public string GetDescription()
    {
        return interactionDescription;
    }

    public void TriggerDialogue()
    {
        DialogueSystem.Instance.StartDialogue(speakerOneName, speakerTwoName, dialogueLines);
    }
}
