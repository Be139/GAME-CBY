using UnityEngine;

[DisallowMultipleComponent]
public class HearthSequentialShutdownChallenge : HearthShutdownChallenge
{
    [SerializeField] private HearthFirstPersonHudController hudController;

    private void Awake()
    {
        ResolveHud();
        Subscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    public void SetHudController(HearthFirstPersonHudController value)
    {
        Unsubscribe();
        hudController = value;
        Subscribe();
    }

    public override void BeginChallenge(bool highTrust)
    {
        ResolveHud();
        if (hudController == null)
        {
            Debug.LogWarning("[HearthSequentialShutdownChallenge] HUD controller is missing; completing the placeholder challenge immediately.", this);
            completed.Invoke();
            return;
        }

        IsRunning = true;
        hudController.SetRouteFinalChoiceInternally(false);
        hudController.ShowShutdownConfirmation(highTrust);
    }

    public override void Submit()
    {
        if (IsRunning && hudController != null)
        {
            hudController.HandleSubmit();
        }
    }

    public override void Cancel()
    {
        if (!IsRunning)
        {
            return;
        }

        IsRunning = false;
        if (hudController != null)
        {
            hudController.HideOverlay();
        }

        cancelled.Invoke();
    }

    private void ResolveHud()
    {
        if (hudController == null)
        {
            hudController = FindObjectOfType<HearthFirstPersonHudController>(true);
        }
    }

    private void Subscribe()
    {
        if (hudController == null)
        {
            return;
        }

        hudController.OnGracefulShutdownConfirmed.RemoveListener(HandleCompleted);
        hudController.OnForcedShutdownConfirmed.RemoveListener(HandleCompleted);
        hudController.OnShutdownCancelled.RemoveListener(HandleCancelled);
        hudController.OnGracefulShutdownConfirmed.AddListener(HandleCompleted);
        hudController.OnForcedShutdownConfirmed.AddListener(HandleCompleted);
        hudController.OnShutdownCancelled.AddListener(HandleCancelled);
    }

    private void Unsubscribe()
    {
        if (hudController == null)
        {
            return;
        }

        hudController.OnGracefulShutdownConfirmed.RemoveListener(HandleCompleted);
        hudController.OnForcedShutdownConfirmed.RemoveListener(HandleCompleted);
        hudController.OnShutdownCancelled.RemoveListener(HandleCancelled);
    }

    private void HandleCompleted()
    {
        if (!IsRunning)
        {
            return;
        }

        IsRunning = false;
        completed.Invoke();
    }

    private void HandleCancelled()
    {
        if (!IsRunning)
        {
            return;
        }

        IsRunning = false;
        cancelled.Invoke();
    }
}
