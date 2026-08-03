using TMPro;
using UnityEngine;

/// <summary>
/// Explicit, prefab-authored bindings for the Human HUD. Runtime controllers
/// may change content and visibility, but must not rewrite the authored layout.
/// </summary>
[DisallowMultipleComponent]
public sealed class HearthHumanHudBindings : MonoBehaviour
{
    [Header("Persistent HUD")]
    [SerializeField] private GameObject persistentRoot;
    [SerializeField] private CanvasGroup persistentCanvasGroup;
    [SerializeField] private TMP_Text identityText;
    [SerializeField] private TMP_Text currentTaskHeadingText;
    [SerializeField] private TMP_Text currentTaskBodyText;

    [Header("Interaction")]
    [SerializeField] private HearthInteractionPromptPresenter interactionPrompt;

    [Header("Authored Selection Fills")]
    [SerializeField] private RectTransform[] selectionTargets =
        new RectTransform[0];
    [SerializeField] private RectTransform[] selectionFills =
        new RectTransform[0];

    public GameObject PersistentRoot { get { return persistentRoot; } }
    public CanvasGroup PersistentCanvasGroup { get { return persistentCanvasGroup; } }
    public TMP_Text IdentityText { get { return identityText; } }
    public TMP_Text CurrentTaskHeadingText { get { return currentTaskHeadingText; } }
    public TMP_Text CurrentTaskBodyText { get { return currentTaskBodyText; } }
    public HearthInteractionPromptPresenter InteractionPrompt { get { return interactionPrompt; } }
    public bool HasSelectionBindings
    {
        get
        {
            return selectionTargets != null && selectionFills != null &&
                selectionTargets.Length > 0 &&
                selectionTargets.Length == selectionFills.Length;
        }
    }

    public bool HasCurrentTaskBinding
    {
        get { return currentTaskHeadingText != null && currentTaskBodyText != null; }
    }

    public bool IsComplete
    {
        get
        {
            return persistentRoot != null && persistentCanvasGroup != null &&
                identityText != null && HasCurrentTaskBinding &&
                interactionPrompt != null && HasSelectionBindings;
        }
    }

    public bool TryGetSelectionFill(
        RectTransform target,
        out RectTransform fill)
    {
        if (target != null && HasSelectionBindings)
        {
            for (int i = 0; i < selectionTargets.Length; i++)
            {
                if (selectionTargets[i] == target)
                {
                    fill = selectionFills[i];
                    return fill != null;
                }
            }
        }

        fill = null;
        return false;
    }

    public void Configure(
        GameObject newPersistentRoot,
        CanvasGroup newPersistentCanvasGroup,
        TMP_Text newIdentityText,
        TMP_Text newCurrentTaskHeadingText,
        TMP_Text newCurrentTaskBodyText,
        HearthInteractionPromptPresenter newInteractionPrompt,
        RectTransform[] newSelectionTargets,
        RectTransform[] newSelectionFills)
    {
        persistentRoot = newPersistentRoot;
        persistentCanvasGroup = newPersistentCanvasGroup;
        identityText = newIdentityText;
        currentTaskHeadingText = newCurrentTaskHeadingText;
        currentTaskBodyText = newCurrentTaskBodyText;
        interactionPrompt = newInteractionPrompt;
        selectionTargets = newSelectionTargets ?? new RectTransform[0];
        selectionFills = newSelectionFills ?? new RectTransform[0];
    }
}
