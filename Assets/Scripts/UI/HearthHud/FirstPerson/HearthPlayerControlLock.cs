using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class HearthPlayerControlLock : MonoBehaviour
{
    private static readonly HashSet<HearthPlayerControlLock> ActiveLocks =
        new HashSet<HearthPlayerControlLock>();

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
    [SerializeField] private bool disableCrouchAlways = true;
    [SerializeField] private bool clearRigidbodyVelocityOnLock = true;
    [SerializeField] private Rigidbody[] rigidbodiesToClear;

    private readonly Dictionary<Behaviour, bool> enabledBeforeLock = new Dictionary<Behaviour, bool>();
    private readonly HashSet<Object> lockOwners = new HashSet<Object>();
    private bool controlsLocked;

    public static bool AnyControlsLocked
    {
        get
        {
            RemoveInvalidActiveLocks();
            return ActiveLocks.Count > 0;
        }
    }

    public bool ControlsLocked
    {
        get { return controlsLocked; }
    }

    public int ActiveLockCount
    {
        get { return lockOwners.Count; }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        ActiveLocks.Clear();
    }

    private void Awake()
    {
        ResolveReferences();

        if (disableJumpAlways)
        {
            SetJumpComponentsEnabled(false);
        }

        if (disableCrouchAlways)
        {
            SetCrouchComponentsEnabled(false);
        }
    }

    private void OnDisable()
    {
        ReleaseAllLocks();
    }

    private void LateUpdate()
    {
        ReleaseDestroyedOwners();
    }

    public void SetControlsLocked(bool locked)
    {
        SetControlsLocked(this, locked);
    }

    public void SetControlsLocked(Object owner, bool locked)
    {
        Object effectiveOwner = owner != null ? owner : this;

        if (locked)
        {
            if (!lockOwners.Add(effectiveOwner))
            {
                return;
            }

            if (lockOwners.Count > 1)
            {
                controlsLocked = true;
                ActiveLocks.Add(this);
                return;
            }

            ResolveReferences();
            controlsLocked = true;
            ActiveLocks.Add(this);
            CaptureEnabledStates();
            SetCoreControlsEnabled(false);
            ClearRigidbodies();
            return;
        }

        if (!lockOwners.Remove(effectiveOwner))
        {
            return;
        }

        if (lockOwners.Count > 0)
        {
            controlsLocked = true;
            return;
        }

        controlsLocked = false;
        ActiveLocks.Remove(this);
        RestoreEnabledStates();
        if (disableJumpAlways)
        {
            SetJumpComponentsEnabled(false);
        }

        if (disableCrouchAlways)
        {
            SetCrouchComponentsEnabled(false);
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

    public void ReleaseOwner(Object owner)
    {
        SetControlsLocked(owner, false);
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

    public void SetDisableCrouchAlways(bool value)
    {
        disableCrouchAlways = value;
        ResolveReferences();

        if (disableCrouchAlways)
        {
            SetCrouchComponentsEnabled(false);
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
        SetEnabled(interactionComponents, enabled);

        if (!disableCrouchAlways)
        {
            SetCrouchComponentsEnabled(enabled);
        }
        else
        {
            SetCrouchComponentsEnabled(false);
        }

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

    private void SetCrouchComponentsEnabled(bool enabled)
    {
        if (crouchComponents == null)
        {
            return;
        }

        for (int i = 0; i < crouchComponents.Length; i++)
        {
            if (crouchComponents[i] != null)
            {
                crouchComponents[i].SetCrouchEnabled(enabled);
                crouchComponents[i].enabled = enabled;
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
            if (body != null && body.gameObject.activeInHierarchy && !body.isKinematic)
            {
                body.velocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
        }
    }

    private void ReleaseDestroyedOwners()
    {
        if (lockOwners.Count == 0)
        {
            return;
        }

        List<Object> destroyedOwners = null;
        foreach (Object owner in lockOwners)
        {
            if (owner != null)
            {
                continue;
            }

            if (destroyedOwners == null)
            {
                destroyedOwners = new List<Object>();
            }

            destroyedOwners.Add(owner);
        }

        if (destroyedOwners == null)
        {
            return;
        }

        for (int i = 0; i < destroyedOwners.Count; i++)
        {
            lockOwners.Remove(destroyedOwners[i]);
        }

        if (lockOwners.Count == 0)
        {
            controlsLocked = false;
            ActiveLocks.Remove(this);
            RestoreEnabledStates();
            if (disableJumpAlways)
            {
                SetJumpComponentsEnabled(false);
            }

            if (disableCrouchAlways)
            {
                SetCrouchComponentsEnabled(false);
            }
        }
    }

    private void ReleaseAllLocks()
    {
        lockOwners.Clear();
        ActiveLocks.Remove(this);

        if (!controlsLocked)
        {
            enabledBeforeLock.Clear();
            return;
        }

        controlsLocked = false;
        RestoreEnabledStates();
        if (disableJumpAlways)
        {
            SetJumpComponentsEnabled(false);
        }

        if (disableCrouchAlways)
        {
            SetCrouchComponentsEnabled(false);
        }
    }

    private static void RemoveInvalidActiveLocks()
    {
        if (ActiveLocks.Count == 0)
        {
            return;
        }

        List<HearthPlayerControlLock> invalid = null;
        foreach (HearthPlayerControlLock activeLock in ActiveLocks)
        {
            if (activeLock != null && activeLock.controlsLocked)
            {
                continue;
            }

            if (invalid == null)
            {
                invalid = new List<HearthPlayerControlLock>();
            }

            invalid.Add(activeLock);
        }

        if (invalid == null)
        {
            return;
        }

        for (int i = 0; i < invalid.Count; i++)
        {
            ActiveLocks.Remove(invalid[i]);
        }
    }
}
