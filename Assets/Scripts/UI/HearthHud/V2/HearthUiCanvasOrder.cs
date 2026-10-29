using UnityEngine;

// World-space orders apply inside their own camera; overlay orders share the screen.
public enum HearthUiCanvasBand
{
    WorldSurface = 0, TerminalPage = 10, TerminalNavigation = 20, TerminalChoice = 30,
    PhotoArchive = 80, HumanHud = 100, CompanionHud = 6200, Replay02Blackout = 7000,
    LobbyNarrative = 7200, Inspection = 7700, Replay03Blackout = 8000, Tutorial = 8200,
    Dialogue = 8500, Shutdown = 8800, FinaleBlackout = 9000, Epilogue = 9100,
    LobbyBlackout = 20000, ViewTransition = 32767
}

[RequireComponent(typeof(Canvas)), DisallowMultipleComponent]
public sealed class HearthUiCanvasOrder : MonoBehaviour
{
    [SerializeField] private HearthUiCanvasBand band;
    [SerializeField] private bool overrideParent = true;
    public HearthUiCanvasBand Band { get { return band; } }
    private void OnEnable() { Apply(); }
    public void Configure(HearthUiCanvasBand value, bool independent)
    {
        band = value;
        overrideParent = independent;
        Apply();
    }
    [ContextMenu("Apply Canvas Order")]
    public void Apply()
    {
        Canvas canvas = GetComponent<Canvas>();
        canvas.overrideSorting = overrideParent;
        canvas.sortingOrder = (int)band;
    }
}
