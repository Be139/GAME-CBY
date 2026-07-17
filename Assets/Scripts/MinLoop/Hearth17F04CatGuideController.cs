using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class Hearth17F04CatGuideController : MonoBehaviour
{
    public enum CatMotion
    {
        Walk,
        RunJump
    }

    [Serializable]
    public class RouteStep
    {
        public Transform target;
        [Min(0f)] public float duration = 3f;
        public CatMotion motion = CatMotion.Walk;
    }

    [Header("Actor")]
    [SerializeField] private Transform actorRoot;
    [SerializeField] private HearthActorAnimationPlayer animationPlayer;

    [Header("Route")]
    [SerializeField] private RouteStep[] routeSteps = Array.Empty<RouteStep>();
    [SerializeField, Min(0f)] private float jumpArcHeight = 0.25f;
    [SerializeField, Range(0f, 1f)] private float pathSmoothing = 0.75f;
    [Tooltip("Only shortens Walk route segments. RunJump and lie animation timing are not changed.")]
    [SerializeField, Min(0.01f)] private float walkRouteSpeedMultiplier = 1.5f;

    [Header("Animation IDs")]
    [SerializeField] private string walkClipId = "Walk_F";
    [SerializeField] private string runClipId = "Run_F";
    [SerializeField] private string lieTransitionClipId = "Lie_to";
    [SerializeField] private string lieIdleClipId = "Lie_idle";

    [Header("Animation Playback")]
    [Tooltip("Only affects Walk_F cadence. Route speed changes do not multiply the leg animation speed.")]
    [SerializeField, Min(0.01f)] private float walkPlaybackSpeed = 2f;

    [Header("Start Pose")]
    [SerializeField] private bool hasStartPose;
    [SerializeField] private Vector3 startWorldPosition;
    [SerializeField] private Quaternion startWorldRotation = Quaternion.identity;

    [Header("Events")]
    [SerializeField] private UnityEvent onReachedPhoto = new UnityEvent();
    [SerializeField] private UnityEvent onSequenceCompleted = new UnityEvent();

    private Coroutine sequenceRoutine;
    private bool isRunning;
    private bool hasReachedPhoto;
    private CatMotion? currentMotion;

    public bool IsRunning
    {
        get { return isRunning; }
    }

    public bool HasReachedPhoto
    {
        get { return hasReachedPhoto; }
    }

    public int RoutePointCount
    {
        get { return routeSteps == null ? 0 : routeSteps.Length; }
    }

    public UnityEvent OnReachedPhoto
    {
        get { return onReachedPhoto; }
    }

    public UnityEvent OnSequenceCompleted
    {
        get { return onSequenceCompleted; }
    }

    private void Awake()
    {
        ResolveReferences();
        if (!hasStartPose)
        {
            CaptureCurrentAsStartPose();
        }
    }

    private void OnDisable()
    {
        StopSequence();
    }

    private void OnValidate()
    {
        jumpArcHeight = Mathf.Max(0f, jumpArcHeight);
        pathSmoothing = Mathf.Clamp01(pathSmoothing);
        walkRouteSpeedMultiplier = Mathf.Max(0.01f, walkRouteSpeedMultiplier);
        walkPlaybackSpeed = Mathf.Max(0.01f, walkPlaybackSpeed);
        if (routeSteps == null)
        {
            routeSteps = Array.Empty<RouteStep>();
        }

        for (int i = 0; i < routeSteps.Length; i++)
        {
            if (routeSteps[i] != null)
            {
                routeSteps[i].duration = Mathf.Max(0f, routeSteps[i].duration);
            }
        }
    }

    public void BeginSequence()
    {
        ResolveReferences();
        if (actorRoot == null)
        {
            Debug.LogWarning("[Hearth17F04CatGuideController] Cat actor root is missing.", this);
            return;
        }

        if (!hasStartPose)
        {
            CaptureCurrentAsStartPose();
        }

        StopSequence();
        ResetActorPose();
        hasReachedPhoto = false;
        isRunning = true;
        sequenceRoutine = StartCoroutine(PlayRoute());
    }

    public void ResetSequence()
    {
        StopSequence();
        ResolveReferences();
        ResetActorPose();
        hasReachedPhoto = false;
        currentMotion = null;
        if (animationPlayer != null)
        {
            animationPlayer.StopPlayback();
        }
    }

    public void StopSequence()
    {
        if (sequenceRoutine != null)
        {
            StopCoroutine(sequenceRoutine);
            sequenceRoutine = null;
        }

        isRunning = false;
        currentMotion = null;
        if (animationPlayer != null)
        {
            animationPlayer.StopPlayback();
        }
    }

    public void CaptureCurrentAsStartPose()
    {
        ResolveReferences();
        if (actorRoot == null)
        {
            return;
        }

        startWorldPosition = actorRoot.position;
        startWorldRotation = actorRoot.rotation;
        hasStartPose = true;
    }

    public void SnapToRoutePoint(int index)
    {
        ResolveReferences();
        if (actorRoot == null || routeSteps == null || index < 0 || index >= routeSteps.Length)
        {
            return;
        }

        Transform target = routeSteps[index] != null ? routeSteps[index].target : null;
        if (target != null)
        {
            actorRoot.SetPositionAndRotation(target.position, target.rotation);
        }
    }

    private IEnumerator PlayRoute()
    {
        if (routeSteps == null || routeSteps.Length == 0)
        {
            Debug.LogWarning("[Hearth17F04CatGuideController] No cat route points are configured.", this);
            FinishSequence();
            yield break;
        }

        Vector3[] routePositions = BuildRoutePositions();
        float[] routeDurations = BuildRouteDurations();
        Vector3[] nodeVelocities = BuildNodeVelocities(routePositions, routeDurations);

        for (int i = 0; i < routeSteps.Length; i++)
        {
            RouteStep step = routeSteps[i];
            if (step == null || step.target == null)
            {
                Debug.LogWarning("[Hearth17F04CatGuideController] Cat route point " + (i + 1) + " is missing; skipping it.", this);
                continue;
            }

            float effectiveDuration = GetEffectiveDuration(step);
            PlayMotion(step.motion, effectiveDuration);
            yield return MoveTo(
                step.target,
                effectiveDuration,
                step.motion == CatMotion.RunJump,
                nodeVelocities[i],
                nodeVelocities[i + 1]);
        }

        hasReachedPhoto = true;
        onReachedPhoto.Invoke();

        float lieTransitionDuration = PlayOnceOrWarn(lieTransitionClipId);
        if (lieTransitionDuration > 0f)
        {
            yield return WaitForGameSeconds(lieTransitionDuration);
        }

        PlayLoopOrWarn(lieIdleClipId);

        FinishSequence();
    }

    private IEnumerator MoveTo(
        Transform target,
        float duration,
        bool useJumpArc,
        Vector3 startVelocity,
        Vector3 endVelocity)
    {
        Vector3 fromPosition = actorRoot.position;
        Quaternion fromRotation = actorRoot.rotation;
        Vector3 toPosition = target.position;
        Quaternion toRotation = target.rotation;

        if (duration <= 0f)
        {
            actorRoot.SetPositionAndRotation(toPosition, toRotation);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Vector3 position;
            if (pathSmoothing <= 0f)
            {
                position = Vector3.Lerp(fromPosition, toPosition, t);
            }
            else
            {
                position = EvaluateHermite(
                    fromPosition,
                    toPosition,
                    startVelocity * pathSmoothing * duration,
                    endVelocity * pathSmoothing * duration,
                    t);
            }

            if (useJumpArc)
            {
                position += Vector3.up * (Mathf.Sin(t * Mathf.PI) * jumpArcHeight);
            }

            float rotationT = t * t * (3f - 2f * t);
            actorRoot.SetPositionAndRotation(position, Quaternion.Slerp(fromRotation, toRotation, rotationT));
            yield return null;
        }

        actorRoot.SetPositionAndRotation(toPosition, toRotation);
    }

    private Vector3[] BuildRoutePositions()
    {
        Vector3[] positions = new Vector3[routeSteps.Length + 1];
        positions[0] = actorRoot.position;
        for (int i = 0; i < routeSteps.Length; i++)
        {
            Transform target = routeSteps[i] != null ? routeSteps[i].target : null;
            positions[i + 1] = target != null ? target.position : positions[i];
        }

        return positions;
    }

    private float[] BuildRouteDurations()
    {
        float[] durations = new float[routeSteps.Length];
        for (int i = 0; i < routeSteps.Length; i++)
        {
            durations[i] = routeSteps[i] != null ? GetEffectiveDuration(routeSteps[i]) : 0.01f;
        }

        return durations;
    }

    private float GetEffectiveDuration(RouteStep step)
    {
        if (step == null)
        {
            return 0.01f;
        }

        float duration = Mathf.Max(0.01f, step.duration);
        return step.motion == CatMotion.Walk
            ? duration / Mathf.Max(0.01f, walkRouteSpeedMultiplier)
            : duration;
    }

    private static Vector3[] BuildNodeVelocities(Vector3[] positions, float[] durations)
    {
        Vector3[] velocities = new Vector3[positions.Length];
        if (positions.Length < 2 || durations.Length == 0)
        {
            return velocities;
        }

        velocities[0] = (positions[1] - positions[0]) / durations[0];
        for (int i = 1; i < positions.Length - 1; i++)
        {
            float timeSpan = Mathf.Max(0.01f, durations[i - 1] + durations[i]);
            velocities[i] = (positions[i + 1] - positions[i - 1]) / timeSpan;
        }

        int last = positions.Length - 1;
        velocities[last] = (positions[last] - positions[last - 1]) / durations[durations.Length - 1];
        return velocities;
    }

    private static Vector3 EvaluateHermite(
        Vector3 start,
        Vector3 end,
        Vector3 startTangent,
        Vector3 endTangent,
        float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;
        float h00 = 2f * t3 - 3f * t2 + 1f;
        float h10 = t3 - 2f * t2 + t;
        float h01 = -2f * t3 + 3f * t2;
        float h11 = t3 - t2;
        return h00 * start + h10 * startTangent + h01 * end + h11 * endTangent;
    }

    private void PlayMotion(CatMotion motion, float duration)
    {
        if (animationPlayer == null)
        {
            return;
        }

        if (motion == CatMotion.RunJump)
        {
            if (animationPlayer.GetClipLength(runClipId) > 0f)
            {
                animationPlayer.PlayOnceForDuration(runClipId, duration);
            }
            else
            {
                WarnMissingClip(runClipId);
            }
            currentMotion = motion;
            return;
        }

        if (currentMotion != CatMotion.Walk)
        {
            PlayLoopOrWarn(walkClipId, walkPlaybackSpeed);
            currentMotion = CatMotion.Walk;
        }
    }

    private float PlayOnceOrWarn(string clipId)
    {
        if (animationPlayer == null || animationPlayer.GetClipLength(clipId) <= 0f)
        {
            WarnMissingClip(clipId);
            return 0f;
        }

        return animationPlayer.PlayOnce(clipId);
    }

    private void PlayLoopOrWarn(string clipId)
    {
        PlayLoopOrWarn(clipId, -1f);
    }

    private void PlayLoopOrWarn(string clipId, float playbackSpeedOverride)
    {
        if (animationPlayer == null || animationPlayer.GetClipLength(clipId) <= 0f)
        {
            WarnMissingClip(clipId);
            return;
        }

        if (playbackSpeedOverride > 0f)
        {
            animationPlayer.PlayLoopAtSpeed(clipId, playbackSpeedOverride);
        }
        else
        {
            animationPlayer.PlayLoop(clipId);
        }
    }

    private void WarnMissingClip(string clipId)
    {
        Debug.LogWarning(
            "[Hearth17F04CatGuideController] Cat clip '" + clipId + "' is missing; route movement will continue without it.",
            this);
    }

    private IEnumerator WaitForGameSeconds(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void FinishSequence()
    {
        sequenceRoutine = null;
        isRunning = false;
        currentMotion = null;
        onSequenceCompleted.Invoke();
    }

    private void ResetActorPose()
    {
        if (actorRoot != null && hasStartPose)
        {
            actorRoot.SetPositionAndRotation(startWorldPosition, startWorldRotation);
        }
    }

    private void ResolveReferences()
    {
        if (actorRoot == null)
        {
            actorRoot = transform;
        }

        if (animationPlayer == null)
        {
            animationPlayer = GetComponent<HearthActorAnimationPlayer>();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Transform previous = actorRoot != null ? actorRoot : transform;
        if (routeSteps == null)
        {
            return;
        }

        for (int i = 0; i < routeSteps.Length; i++)
        {
            RouteStep step = routeSteps[i];
            if (step == null || step.target == null)
            {
                continue;
            }

            Gizmos.color = step.motion == CatMotion.RunJump
                ? new Color(1f, 0.65f, 0.18f, 0.95f)
                : new Color(0.2f, 0.85f, 1f, 0.9f);
            Gizmos.DrawLine(previous.position, step.target.position);
            Gizmos.DrawWireSphere(step.target.position, 0.08f);

#if UNITY_EDITOR
            Handles.Label(
                step.target.position + Vector3.up * 0.12f,
                (i + 1) + "  " + step.motion + "  " + GetEffectiveDuration(step).ToString("0.##") + "s");
#endif
            previous = step.target;
        }
    }
}
