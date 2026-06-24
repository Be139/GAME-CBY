using UnityEngine;

public class MinLoopDebugHotkeys : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MinLoopFlowController flowController;
    [SerializeField] private ReplaySequenceController replaySequenceController;
    [SerializeField] private bool findReferencesOnAwake = true;

    [Header("Safety")]
    [SerializeField] private bool enableHotkeys = true;
    [SerializeField] private bool editorOrDevelopmentOnly = true;

    [Header("Keys")]
    [SerializeField] private KeyCode resetKey = KeyCode.F1;
    [SerializeField] private KeyCode openTerminalKey = KeyCode.F2;
    [SerializeField] private KeyCode confirmAccessCardKey = KeyCode.F3;
    [SerializeField] private KeyCode requestReplayKey = KeyCode.F4;
    [SerializeField] private KeyCode performComfortKey = KeyCode.F5;
    [SerializeField] private KeyCode chooseAKey = KeyCode.F6;
    [SerializeField] private KeyCode chooseBKey = KeyCode.F7;
    [SerializeField] private KeyCode nextResidentKey = KeyCode.F8;

    private void Awake()
    {
        ResolveReferences();
    }

    private void Update()
    {
        if (!enableHotkeys || !CanUseHotkeys())
        {
            return;
        }

        ResolveReferences();

        if (Input.GetKeyDown(resetKey))
        {
            ResetFlow();
        }

        if (Input.GetKeyDown(openTerminalKey))
        {
            BeginTerminalInspection();
        }

        if (Input.GetKeyDown(confirmAccessCardKey))
        {
            ConfirmAccessCard();
        }

        if (Input.GetKeyDown(requestReplayKey))
        {
            RequestReplay();
        }

        if (Input.GetKeyDown(performComfortKey))
        {
            PerformComfort();
        }

        if (Input.GetKeyDown(chooseAKey))
        {
            ChooseDispositionA();
        }

        if (Input.GetKeyDown(chooseBKey))
        {
            ChooseDispositionB();
        }

        if (Input.GetKeyDown(nextResidentKey))
        {
            ContinueToNextResident();
        }
    }

    public void ResetFlow()
    {
        if (flowController != null)
        {
            flowController.ResetFlow();
        }
    }

    public void BeginTerminalInspection()
    {
        if (flowController != null)
        {
            flowController.BeginTerminalInspection();
        }
    }

    public void ConfirmAccessCard()
    {
        if (flowController != null)
        {
            flowController.ConfirmAccessCard();
        }
    }

    public void RequestReplay()
    {
        if (flowController != null)
        {
            flowController.RequestReplayFromTerminal();
        }
    }

    public void PerformComfort()
    {
        if (replaySequenceController != null)
        {
            replaySequenceController.PerformComfortAction();
        }
    }

    public void ChooseDispositionA()
    {
        if (flowController != null)
        {
            flowController.ChooseDispositionA();
        }
    }

    public void ChooseDispositionB()
    {
        if (flowController != null)
        {
            flowController.ChooseDispositionB();
        }
    }

    public void ContinueToNextResident()
    {
        if (flowController != null)
        {
            flowController.ContinueToNextResident();
        }
    }

    public void SetHotkeysEnabled(bool value)
    {
        enableHotkeys = value;
    }

    private void ResolveReferences()
    {
        if (!findReferencesOnAwake)
        {
            return;
        }

        if (flowController == null)
        {
            flowController = FindObjectOfType<MinLoopFlowController>();
        }

        if (replaySequenceController == null)
        {
            replaySequenceController = FindObjectOfType<ReplaySequenceController>();
        }
    }

    private bool CanUseHotkeys()
    {
        if (!editorOrDevelopmentOnly)
        {
            return true;
        }

        return Application.isEditor || Debug.isDebugBuild;
    }
}
