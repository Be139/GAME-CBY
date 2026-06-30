using UnityEngine;

[DisallowMultipleComponent]
public class HearthCompanionReplayInteractable : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform focusTarget;
    [SerializeField] private string interactionLabel = "[ Approach bedside - Guard service subject ]";
    [SerializeField] private bool availableOnStart;

    [Header("View Gate")]
    [SerializeField] private float maxDistance = 3.25f;
    [SerializeField] private float maxViewAngle = 12f;
    [SerializeField] private LayerMask lineOfSightMask = ~0;
    [SerializeField] private bool requireLineOfSight;
    [SerializeField] private bool requireCenterRayHit = true;
    [SerializeField] private Transform raycastTargetRoot;
    [SerializeField] private QueryTriggerInteraction raycastTriggerInteraction = QueryTriggerInteraction.Collide;

    [Header("Allowed Side Gate")]
    [SerializeField] private bool useAllowedSideGate = true;
    [SerializeField] private Transform allowedSideReference;
    [SerializeField] private Vector3 allowedSideLocalNormal = Vector3.forward;
    [SerializeField] private float minAllowedSideDot = 0f;

    [Header("Events")]
    [SerializeField] private HearthCompanion17F01ReplayController replayController;

    private bool available;

    public string InteractionLabel
    {
        get { return interactionLabel; }
    }

    public bool IsAvailable
    {
        get { return available; }
    }

    private void Awake()
    {
        if (focusTarget == null)
        {
            focusTarget = transform;
        }

        available = availableOnStart;
    }

    private void OnValidate()
    {
        maxDistance = Mathf.Max(0.1f, maxDistance);
        maxViewAngle = Mathf.Clamp(maxViewAngle, 1f, 90f);
        if (allowedSideLocalNormal.sqrMagnitude < 0.0001f)
        {
            allowedSideLocalNormal = Vector3.forward;
        }
    }

    public void Configure(
        Transform newFocusTarget,
        Transform newAllowedSideReference,
        HearthCompanion17F01ReplayController newReplayController)
    {
        focusTarget = newFocusTarget != null ? newFocusTarget : transform;
        raycastTargetRoot = focusTarget;
        allowedSideReference = newAllowedSideReference;
        replayController = newReplayController;
    }

    public void SetAvailable(bool value)
    {
        available = value;
    }

    public bool CanInteract(Transform actor, Camera camera)
    {
        if (!available || actor == null || camera == null || focusTarget == null)
        {
            return false;
        }

        Transform targetRoot = raycastTargetRoot != null ? raycastTargetRoot : focusTarget;
        if (requireCenterRayHit)
        {
            if (!CenterRayHitsTarget(camera, targetRoot))
            {
                return false;
            }
        }
        else if (!AngleGatePasses(camera))
        {
            return false;
        }

        if (useAllowedSideGate && !IsActorOnAllowedSide(actor.position))
        {
            return false;
        }

        return true;
    }

    public void NotifyConfirmed()
    {
        if (replayController != null)
        {
            replayController.CompleteCurrentStep();
        }
    }

    private bool IsActorOnAllowedSide(Vector3 actorPosition)
    {
        if (allowedSideReference == null)
        {
            return true;
        }

        Vector3 normal = allowedSideReference.TransformDirection(allowedSideLocalNormal.normalized);
        Vector3 fromReference = actorPosition - allowedSideReference.position;
        fromReference.y = 0f;
        normal.y = 0f;

        if (fromReference.sqrMagnitude < 0.0001f || normal.sqrMagnitude < 0.0001f)
        {
            return true;
        }

        return Vector3.Dot(fromReference.normalized, normal.normalized) >= minAllowedSideDot;
    }

    private bool CenterRayHitsTarget(Camera camera, Transform targetRoot)
    {
        if (camera == null || targetRoot == null)
        {
            return false;
        }

        Ray ray = new Ray(camera.transform.position, camera.transform.forward);
        if (!Physics.Raycast(ray, out RaycastHit hit, maxDistance, lineOfSightMask, raycastTriggerInteraction))
        {
            return false;
        }

        return IsTargetTransform(hit.transform, targetRoot);
    }

    private bool AngleGatePasses(Camera camera)
    {
        Vector3 cameraPosition = camera.transform.position;
        Vector3 toTarget = focusTarget.position - cameraPosition;
        float distance = toTarget.magnitude;
        if (distance > maxDistance || distance <= 0.001f)
        {
            return false;
        }

        float angle = Vector3.Angle(camera.transform.forward, toTarget / distance);
        if (angle > maxViewAngle)
        {
            return false;
        }

        if (requireLineOfSight && Physics.Raycast(cameraPosition, toTarget / distance, out RaycastHit hit, distance, lineOfSightMask, QueryTriggerInteraction.Ignore))
        {
            if (!IsTargetTransform(hit.transform, focusTarget))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsTargetTransform(Transform hitTransform, Transform targetRoot)
    {
        return hitTransform == targetRoot ||
               hitTransform.IsChildOf(targetRoot) ||
               targetRoot.IsChildOf(hitTransform);
    }
}
