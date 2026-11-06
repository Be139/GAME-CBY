using UnityEngine;

[DisallowMultipleComponent]
public class HearthSequentialShutdownChallenge : HearthShutdownChallenge
{
    [SerializeField] private HearthFirstPersonHudController hudController;
    private bool savedRouting;
    private bool hasSavedRouting;

    private void Awake()
    {
        ResolveHud();
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        AbortChallenge();
        Unsubscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    public void SetHudController(HearthFirstPersonHudController value)
    {
        AbortChallenge();
        Unsubscribe();
        hudController = value;
        if (isActiveAndEnabled) Subscribe();
    }

    public override void BeginChallenge(bool highTrust)
    {
        if (!isActiveAndEnabled) return;
        AbortChallenge();
        ResolveHud();
        Subscribe();
        if (hudController == null)
        {
            Debug.LogWarning("[HearthSequentialShutdownChallenge] HUD controller is missing; completing the placeholder challenge immediately.", this);
            completed.Invoke();
            return;
        }

        IsRunning = true;
        savedRouting = hudController.RouteFinalChoiceInternally;
        hasSavedRouting = true;
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

        AbortChallenge();
        cancelled.Invoke();
    }

    private void ResolveHud()
    {
        if (hudController == null)
        {
            hudController = FindObjectOfType<HearthFirstPersonHudController>(true);
        }
    }

    public override void AbortChallenge()
    {
        bool wasRunning = IsRunning;
        IsRunning = false;
        RestoreRouting();
        if (wasRunning && hudController != null)
        {
            var page = hudController.CurrentPageId;
            if (page == HearthFirstPersonHudPageId.Slide10ShutdownConfirm ||
                page == HearthFirstPersonHudPageId.Slide11Warning01 ||
                page == HearthFirstPersonHudPageId.Slide12Warning02 ||
                page == HearthFirstPersonHudPageId.Slide13Warning03)
                hudController.HideOverlay();
        }
    }

    private void RestoreRouting()
    {
        if (!hasSavedRouting) return;
        if (hudController != null) hudController.SetRouteFinalChoiceInternally(savedRouting);
        hasSavedRouting = false;
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
        RestoreRouting();
        completed.Invoke();
    }

    private void HandleCancelled()
    {
        if (!IsRunning)
        {
            return;
        }

        IsRunning = false;
        RestoreRouting();
        cancelled.Invoke();
    }
}
