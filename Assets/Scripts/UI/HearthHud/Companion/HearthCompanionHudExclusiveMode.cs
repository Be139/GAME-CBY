using UnityEngine;

/// <summary>
/// Legacy compatibility shell. Base HUD exclusivity is now owned by
/// HearthUiStateCoordinator, so this component must never suppress or restore
/// another CanvasGroup on its own.
/// </summary>
[DisallowMultipleComponent]
public class HearthCompanionHudExclusiveMode : MonoBehaviour
{
    private void Awake()
    {
        enabled = false;
    }

    private void OnEnable()
    {
        enabled = false;
    }

    public void SetViewSwitchController(ViewSwitchController controller)
    {
        // Kept so legacy UnityEvent and editor bindings remain valid.
    }

    public void RefreshTargets()
    {
        // Intentionally empty. The coordinator is the single visibility owner.
    }
}
