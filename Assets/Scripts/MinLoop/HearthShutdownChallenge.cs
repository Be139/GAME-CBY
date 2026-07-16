using UnityEngine;
using UnityEngine.Events;

public abstract class HearthShutdownChallenge : MonoBehaviour
{
    [SerializeField] protected UnityEvent completed = new UnityEvent();
    [SerializeField] protected UnityEvent cancelled = new UnityEvent();

    public bool IsRunning { get; protected set; }

    public UnityEvent Completed
    {
        get { return completed; }
    }

    public UnityEvent Cancelled
    {
        get { return cancelled; }
    }

    public abstract void BeginChallenge(bool highTrust);
    public abstract void Submit();
    public abstract void Cancel();
}
