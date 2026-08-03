using TMPro;
using UnityEngine;

/// <summary>
/// Explicit, prefab-authored bindings for the Companion HUD header and prompts.
/// It replaces runtime name searches without changing story scene data.
/// </summary>
[DisallowMultipleComponent]
public sealed class HearthCompanionHudBindings : MonoBehaviour
{
    [SerializeField] private TMP_Text identityHeadingText;
    [SerializeField] private TMP_Text identityValueText;
    [SerializeField] private TMP_Text currentTaskHeadingText;
    [SerializeField] private TMP_Text currentTaskBodyText;
    [SerializeField] private HearthInteractionPromptPresenter interactionPrompt;
    [SerializeField] private HearthCompanionHoldPrompt holdPrompt;

    public TMP_Text IdentityHeadingText { get { return identityHeadingText; } }
    public TMP_Text IdentityValueText { get { return identityValueText; } }
    public TMP_Text CurrentTaskHeadingText { get { return currentTaskHeadingText; } }
    public TMP_Text CurrentTaskBodyText { get { return currentTaskBodyText; } }
    public HearthInteractionPromptPresenter InteractionPrompt { get { return interactionPrompt; } }
    public HearthCompanionHoldPrompt HoldPrompt { get { return holdPrompt; } }

    public bool IsComplete
    {
        get
        {
            return identityHeadingText != null &&
                identityValueText != null &&
                currentTaskHeadingText != null &&
                currentTaskBodyText != null &&
                interactionPrompt != null &&
                holdPrompt != null;
        }
    }

    public void Configure(
        TMP_Text newIdentityHeadingText,
        TMP_Text newIdentityValueText,
        TMP_Text newCurrentTaskHeadingText,
        TMP_Text newCurrentTaskBodyText,
        HearthInteractionPromptPresenter newInteractionPrompt,
        HearthCompanionHoldPrompt newHoldPrompt)
    {
        identityHeadingText = newIdentityHeadingText;
        identityValueText = newIdentityValueText;
        currentTaskHeadingText = newCurrentTaskHeadingText;
        currentTaskBodyText = newCurrentTaskBodyText;
        interactionPrompt = newInteractionPrompt;
        holdPrompt = newHoldPrompt;
    }
}
