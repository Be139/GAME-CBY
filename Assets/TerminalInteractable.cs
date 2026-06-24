using UnityEngine;

public class TerminalInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionDescription = "Open Terminal";
    [SerializeField] private TerminalUIController terminalUI;
    [SerializeField] private bool findControllerOnAwake = true;

    private void Awake()
    {
        if (terminalUI == null && findControllerOnAwake)
        {
            terminalUI = FindObjectOfType<TerminalUIController>();
        }
    }

    public void Interact()
    {
        if (terminalUI == null)
        {
            Debug.LogWarning("TerminalInteractable could not open because no TerminalUIController is assigned.", this);
            return;
        }

        terminalUI.Open();
    }

    public string GetDescription()
    {
        return interactionDescription;
    }

    public void SetTerminalUI(TerminalUIController controller)
    {
        terminalUI = controller;
    }
}
