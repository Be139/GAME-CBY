using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class HearthActorRootMotionRelay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform actorRoot;

    [Header("Transfer")]
    [SerializeField] private bool applyHorizontalPosition = true;
    [SerializeField] private bool applyRotation;
    [SerializeField] private bool restoreAnimatorChildLocalTransform = true;

    private Vector3 baselineLocalPosition;
    private Quaternion baselineLocalRotation;
    private Vector3 baselineLocalScale;
    private bool hasBaseline;

    public Transform ActorRoot
    {
        get { return actorRoot; }
    }

    public void Configure(Transform targetActorRoot)
    {
        animator = GetComponent<Animator>();
        actorRoot = targetActorRoot;
        CaptureBaseline();
    }

    public void CaptureBaseline()
    {
        if (actorRoot == null || actorRoot == transform || !transform.IsChildOf(actorRoot))
        {
            hasBaseline = false;
            return;
        }

        baselineLocalPosition = transform.localPosition;
        baselineLocalRotation = transform.localRotation;
        baselineLocalScale = transform.localScale;
        hasBaseline = true;
    }

    public void RestoreAnimatorChildBaseline()
    {
        if (!hasBaseline)
        {
            return;
        }

        transform.localPosition = baselineLocalPosition;
        transform.localRotation = baselineLocalRotation;
        transform.localScale = baselineLocalScale;
    }

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void OnEnable()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (!hasBaseline)
        {
            CaptureBaseline();
        }
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying || animator == null || actorRoot == null || !animator.applyRootMotion || !hasBaseline)
        {
            return;
        }

        bool hasRootMotionDelta = animator.deltaPosition.sqrMagnitude > 0.0000001f ||
            Quaternion.Angle(animator.deltaRotation, Quaternion.identity) > 0.0001f;
        if (applyHorizontalPosition && hasRootMotionDelta)
        {
            Vector3 horizontalDelta = actorRoot.TransformVector(transform.localPosition - baselineLocalPosition);
            horizontalDelta.y = 0f;
            actorRoot.position += horizontalDelta;
        }

        if (applyRotation && hasRootMotionDelta)
        {
            Quaternion localRotationDelta = transform.localRotation * Quaternion.Inverse(baselineLocalRotation);
            actorRoot.rotation *= localRotationDelta;
        }

        if (restoreAnimatorChildLocalTransform)
        {
            RestoreAnimatorChildBaseline();
        }
    }
}
