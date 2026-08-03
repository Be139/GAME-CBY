using TMPro;
using UnityEngine;

/// <summary>
/// Explicit authored view for the TV4 photo archive. The runtime controller
/// updates page text, visibility and camera pose only; layout stays in Prefab.
/// </summary>
[DisallowMultipleComponent]
public sealed class HearthPhotoArchiveViewBindings : MonoBehaviour
{
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private CanvasGroup rootGroup;
    [SerializeField] private TMP_Text pageText;
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private HearthDialogueSurface dialogueSurface;

    public Canvas WorldCanvas { get { return worldCanvas; } }
    public CanvasGroup RootGroup { get { return rootGroup; } }
    public TMP_Text PageText { get { return pageText; } }
    public TMP_Text HintText { get { return hintText; } }
    public HearthDialogueSurface DialogueSurface { get { return dialogueSurface; } }

    public bool IsComplete
    {
        get
        {
            return worldCanvas != null && rootGroup != null &&
                pageText != null && hintText != null && dialogueSurface != null;
        }
    }

    public void Configure(
        Canvas newWorldCanvas,
        CanvasGroup newRootGroup,
        TMP_Text newPageText,
        TMP_Text newHintText,
        HearthDialogueSurface newDialogueSurface)
    {
        worldCanvas = newWorldCanvas;
        rootGroup = newRootGroup;
        pageText = newPageText;
        hintText = newHintText;
        dialogueSurface = newDialogueSurface;
    }
}
