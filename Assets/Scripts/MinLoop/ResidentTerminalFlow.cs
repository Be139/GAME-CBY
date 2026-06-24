using UnityEngine;

public class ResidentTerminalFlow : MonoBehaviour, IInteractable
{
    [SerializeField] private string fallbackDescription = "E 刷工牌";
    [SerializeField] private MinLoopFlowController flowController;
    [SerializeField] private bool findControllerOnAwake = true;

    private void Awake()
    {
        ResolveFlowController();
    }

    public void Interact()
    {
        ResolveFlowController();

        if (flowController == null)
        {
            Debug.LogWarning("ResidentTerminalFlow cannot start because no MinLoopFlowController is assigned.", this);
            return;
        }

        flowController.BeginTerminalInspection();
    }

    public string GetDescription()
    {
        ResolveFlowController();

        if (flowController != null)
        {
            return flowController.GetTerminalInteractionDescription();
        }

        return fallbackDescription;
    }

    public void SetFlowController(MinLoopFlowController controller)
    {
        flowController = controller;
    }

    private void ResolveFlowController()
    {
        if (flowController == null && findControllerOnAwake)
        {
            flowController = FindObjectOfType<MinLoopFlowController>();
        }
    }
}
