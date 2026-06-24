using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SmartDoorTriggerZone : MonoBehaviour
{
    [Header("Door")]
    [SerializeField] private SmartDoorController targetDoor;
    [SerializeField] private bool openOnEnter = true;
    [SerializeField] private bool closeOnExit = true;
    [SerializeField] private float closeDelay = 0.25f;

    [Header("Trigger Filter")]
    [SerializeField] private LayerMask allowedLayers = ~0;
    [SerializeField] private string requiredTag;
    [SerializeField] private Transform requiredRoot;
    [SerializeField] private bool ignoreTriggerColliders = true;

    [Header("Fallback Collider")]
    [SerializeField] private bool createBoxColliderIfMissing = true;
    [SerializeField] private bool autoMarkColliderAsTrigger = true;
    [SerializeField] private Vector3 fallbackBoxSize = new Vector3(2f, 2f, 2f);

    [Header("Feedback")]
    [SerializeField] private InteractionFeedbackController enterFeedback;
    [SerializeField] private InteractionFeedbackController exitFeedback;

    [Header("Events")]
    [SerializeField] private UnityEvent zoneEntered = new UnityEvent();
    [SerializeField] private UnityEvent zoneEmptied = new UnityEvent();

    private readonly HashSet<Collider> occupants = new HashSet<Collider>();
    private Coroutine closeRoutine;

    public bool IsOccupied
    {
        get { return occupants.Count > 0; }
    }

    public int OccupantCount
    {
        get { return occupants.Count; }
    }

    public SmartDoorController TargetDoor
    {
        get { return targetDoor; }
    }

    public bool HasConfiguredDoor
    {
        get { return targetDoor != null || GetComponentInParent<SmartDoorController>() != null; }
    }

    public bool HasUsableTriggerCollider
    {
        get
        {
            Collider triggerCollider = GetComponent<Collider>();
            return triggerCollider != null && triggerCollider.isTrigger;
        }
    }

    public bool CanCreateBoxColliderIfMissing
    {
        get { return createBoxColliderIfMissing; }
    }

    private void Reset()
    {
        ResolveReferences();
        EnsureTriggerCollider();
    }

    private void Awake()
    {
        ResolveReferences();
        EnsureTriggerCollider();
    }

    private void OnValidate()
    {
        closeDelay = Mathf.Max(0f, closeDelay);
        fallbackBoxSize.x = Mathf.Max(0.01f, fallbackBoxSize.x);
        fallbackBoxSize.y = Mathf.Max(0.01f, fallbackBoxSize.y);
        fallbackBoxSize.z = Mathf.Max(0.01f, fallbackBoxSize.z);

        if (autoMarkColliderAsTrigger)
        {
            Collider triggerCollider = GetComponent<Collider>();
            if (triggerCollider != null)
            {
                triggerCollider.isTrigger = true;
            }
        }
    }

    private void OnDisable()
    {
        StopCloseRoutine();
        occupants.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!ShouldTrack(other))
        {
            return;
        }

        bool wasOccupied = occupants.Count > 0;
        occupants.Add(other);

        if (!wasOccupied && occupants.Count > 0)
        {
            HandleZoneEntered();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!occupants.Remove(other) || occupants.Count > 0)
        {
            return;
        }

        HandleZoneEmptied();
    }

    public void SetTargetDoor(SmartDoorController door)
    {
        targetDoor = door;
    }

    public void ForceOpen()
    {
        StopCloseRoutine();
        OpenDoor();
    }

    public void ForceClose()
    {
        StopCloseRoutine();
        CloseDoor();
    }

    public void ClearOccupants()
    {
        occupants.Clear();
        StopCloseRoutine();
    }

    public void RefreshReferences()
    {
        ResolveReferences();
        EnsureTriggerCollider();
    }

    private void HandleZoneEntered()
    {
        StopCloseRoutine();

        if (openOnEnter)
        {
            OpenDoor();
        }

        PlayFeedback(enterFeedback);
        zoneEntered.Invoke();
    }

    private void HandleZoneEmptied()
    {
        PlayFeedback(exitFeedback);
        zoneEmptied.Invoke();

        if (!closeOnExit)
        {
            return;
        }

        if (closeDelay > 0f && Application.isPlaying)
        {
            closeRoutine = StartCoroutine(CloseAfterDelay());
        }
        else
        {
            CloseDoor();
        }
    }

    private IEnumerator CloseAfterDelay()
    {
        yield return new WaitForSeconds(closeDelay);
        closeRoutine = null;

        if (occupants.Count == 0)
        {
            CloseDoor();
        }
    }

    private void OpenDoor()
    {
        ResolveReferences();

        if (targetDoor == null)
        {
            return;
        }

        if (!targetDoor.IsOpen || targetDoor.IsMoving)
        {
            targetDoor.Open();
        }
    }

    private void CloseDoor()
    {
        ResolveReferences();

        if (targetDoor == null)
        {
            return;
        }

        if (targetDoor.IsOpen || targetDoor.IsMoving)
        {
            targetDoor.Close();
        }
    }

    private bool ShouldTrack(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        if (ignoreTriggerColliders && other.isTrigger)
        {
            return false;
        }

        int otherLayerMask = 1 << other.gameObject.layer;
        if ((allowedLayers.value & otherLayerMask) == 0)
        {
            return false;
        }

        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag))
        {
            return false;
        }

        if (requiredRoot != null && !IsSameOrChildOf(other.transform, requiredRoot))
        {
            return false;
        }

        return true;
    }

    private bool IsSameOrChildOf(Transform candidate, Transform root)
    {
        Transform current = candidate;
        while (current != null)
        {
            if (current == root)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private void ResolveReferences()
    {
        if (targetDoor == null)
        {
            targetDoor = GetComponentInParent<SmartDoorController>();
        }
    }

    private void EnsureTriggerCollider()
    {
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null && createBoxColliderIfMissing)
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.size = fallbackBoxSize;
            triggerCollider = boxCollider;
        }

        if (triggerCollider != null && autoMarkColliderAsTrigger)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void StopCloseRoutine()
    {
        if (closeRoutine != null)
        {
            StopCoroutine(closeRoutine);
            closeRoutine = null;
        }
    }

    private void PlayFeedback(InteractionFeedbackController feedback)
    {
        if (feedback != null)
        {
            feedback.PlayFeedback();
        }
    }
}
