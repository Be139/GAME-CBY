using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    void Interact();

    string GetDescription();
}

public interface IInteractionAvailability
{
    bool IsInteractionAvailable { get; }
}

/// <summary>
/// Optional target-specific short-press key. Interactables that do not
/// implement this continue to use PlayerInteraction's default E key.
/// </summary>
public interface IInteractionKeyProvider
{
    KeyCode InteractionKey { get; }
    string InteractionKeyLabel { get; }
}
