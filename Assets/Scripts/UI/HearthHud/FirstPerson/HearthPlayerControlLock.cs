using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class HearthPlayerControlLock : MonoBehaviour
{
    [Header("Auto Bind")]
    [SerializeField] private bool autoFindSceneControllers = true;

    [Header("Controls To Lock")]
    [SerializeField] private FirstPersonMovement[] movementComponents;
    [SerializeField] private FirstPersonLook[] lookComponents;
    [SerializeField] private Crouch[] crouchComponents;
    [SerializeField] private Jump[] jumpComponents;
    [SerializeField] private PlayerInteraction[] interactionComponents;
    [SerializeField] private Behaviour[] extraBehavioursToDisable;

    [Header("Lock Behaviour")]
    [SerializeField] private bool disableJumpAlways = true;
    [SerializeField] private bool clearRigidbodyVelocityOnLock = true;
    [SerializeField] private Rigidbody[] rigidbodiesToClear;

    private readonly Dictionary<Behaviour, bool> enabledBeforeLock = new Dictionary<Behaviour, bool>();
    private bool controlsLocked;

    public bool ControlsLocked
    {
        get { return controlsLocked; }
    }

    private void Awake()
    {
        ResolveReferences();

        if (disableJumpAlways)
        {
            SetJumpComponentsEnabled(false);
        }
    }

    private void OnDisable()
    {
        SetControlsLocked(false);
    }

    public void SetControlsLocked(bool locked)
    {
        ResolveReferences();

        if (locked == controlsLocked)
        {
            return;
        }

        controlsLocked = locked;

        if (locked)
        {
            CaptureEnabledStates();
            SetCoreControlsEnabled(false);
            ClearRigidbodies();
        }
        else
        {
            RestoreEnabledStates();
            if (disableJumpAlways)
            {
                SetJumpComponentsEnabled(false);
            }
        }
    }

    public void LockControls()
    {
        SetControlsLocked(true);
    }

    public void UnlockControls()
    {
        SetControlsLocked(false);
    }

    public void SetDisableJumpAlways(bool value)
    {
        disableJumpAlways = value;
        ResolveReferences();

        if (disableJumpAlways)
        {
            SetJumpComponentsEnabled(false);
        }
    }

    public void ResolveReferences()
    {
        if (!autoFindSceneControllers)
        {
            return;
        }

        if (movementComponents == null || movementComponents.Length == 0)
        {
            movementComponents = FindObjectsOfType<FirstPersonMovement>(true);
        }

        if (lookComponents == null || lookComponents.Length == 0)
        {
            lookComponents = FindObjectsOfType<FirstPersonLook>(true);
        }

        if (crouchComponents == null || crouchComponents.Length == 0)
        {
            crouchComponents = FindObjectsOfType<Crouch>(true);
        }

        if (jumpComponents == null || jumpComponents.Length == 0)
        {
            jumpComponents = FindObjectsOfType<Jump>(true);
        }

        if (interactionComponents == null || interactionComponents.Length == 0)
        {
            interactionComponents = FindObjectsOfType<PlayerInteraction>(true);
        }

        if (rigidbodiesToClear == null || rigidbodiesToClear.Length == 0)
        {
            List<Rigidbody> bodies = new List<Rigidbody>();
            if (movementComponents != null)
            {
                for (int i = 0; i < movementComponents.Length; i++)
                {
                    if (movementComponents[i] == null)
                    {
                        continue;
                    }

                    Rigidbody body = movementComponents[i].GetComponent<Rigidbody>();
                    if (body != null && !bodies.Contains(body))
                    {
                        bodies.Add(body);
                    }
                }
            }

            rigidbodiesToClear = bodies.ToArray();
        }
    }

    private void CaptureEnabledStates()
    {
        enabledBeforeLock.Clear();
        Capture(movementComponents);
        Capture(lookComponents);
        Capture(crouchComponents);
        Capture(jumpComponents);
        Capture(interactionComponents);
        Capture(extraBehavioursToDisable);
    }

    private void Capture(Behaviour[] behaviours)
    {
        if (behaviours == null)
        {
            return;
        }

        for (int i = 0; i < behaviours.Length; i++)
        {
            Behaviour behaviour = behaviours[i];
            if (behaviour != null && !enabledBeforeLock.ContainsKey(behaviour))
            {
                enabledBeforeLock.Add(behaviour, behaviour.enabled);
            }
        }
    }

    private void SetCoreControlsEnabled(bool enabled)
    {
        SetEnabled(movementComponents, enabled);
        SetEnabled(lookComponents, enabled);
        SetEnabled(crouchComponents, enabled);
        SetEnabled(interactionComponents, enabled);

        if (!disableJumpAlways)
        {
            SetEnabled(jumpComponents, enabled);
        }
        else
        {
            SetJumpComponentsEnabled(false);
        }

        SetEnabled(extraBehavioursToDisable, enabled);
    }

    private void RestoreEnabledStates()
    {
        foreach (KeyValuePair<Behaviour, bool> pair in enabledBeforeLock)
        {
            if (pair.Key != null)
            {
                pair.Key.enabled = pair.Value;
            }
        }

        enabledBeforeLock.Clear();
    }

    private void SetJumpComponentsEnabled(bool enabled)
    {
        if (jumpComponents == null)
        {
            return;
        }

        for (int i = 0; i < jumpComponents.Length; i++)
        {
            if (jumpComponents[i] != null)
            {
                jumpComponents[i].enabled = enabled;
                jumpComponents[i].SetJumpEnabled(enabled);
            }
        }
    }

    private void SetEnabled(Behaviour[] behaviours, bool enabled)
    {
        if (behaviours == null)
        {
            return;
        }

        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] != null)
            {
                behaviours[i].enabled = enabled;
            }
        }
    }

    private void ClearRigidbodies()
    {
        if (!clearRigidbodyVelocityOnLock || rigidbodiesToClear == null)
        {
            return;
        }

        for (int i = 0; i < rigidbodiesToClear.Length; i++)
        {
            Rigidbody body = rigidbodiesToClear[i];
            if (body != null)
            {
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
        }
    }
}
