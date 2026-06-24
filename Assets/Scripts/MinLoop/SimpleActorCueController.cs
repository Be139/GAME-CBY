using System;
using System.Collections;
using UnityEngine;

public class SimpleActorCueController : MonoBehaviour
{
    [Serializable]
    public class LocalPoseCue
    {
        public bool enabled = true;
        public Vector3 localPosition;
        public Vector3 localEulerAngles;
        public Vector3 localScale = Vector3.one;
        public float duration = 0.35f;
        public AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    }

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform lookRoot;
    [SerializeField] private GameObject visibleRoot;

    [Header("Animator Triggers")]
    [SerializeField] private string sleepTrigger = "Sleep";
    [SerializeField] private string nightmareTrigger = "Nightmare";
    [SerializeField] private string comfortedTrigger = "Comforted";
    [SerializeField] private string morningTrigger = "Morning";

    [Header("Fallback Poses")]
    [SerializeField] private bool useFallbackPosesWhenNoAnimator = true;
    [SerializeField] private bool applyFallbackPosesEvenWithAnimator;
    [SerializeField] private Transform poseRoot;
    [SerializeField] private bool captureInitialPoseOnAwake = true;
    [SerializeField] private bool resetPoseWhenHidden = true;
    [SerializeField] private LocalPoseCue sleepPose = CreatePose(new Vector3(0f, 0f, 0f), new Vector3(75f, 0f, 0f), Vector3.one, 0.25f);
    [SerializeField] private LocalPoseCue nightmareWakePose = CreatePose(new Vector3(0f, 0.05f, 0f), new Vector3(12f, 0f, 0f), Vector3.one, 0.32f);
    [SerializeField] private LocalPoseCue comfortedPose = CreatePose(new Vector3(0f, 0f, 0f), new Vector3(75f, 0f, 0f), Vector3.one, 0.45f);
    [SerializeField] private LocalPoseCue morningPose = CreatePose(Vector3.zero, Vector3.zero, Vector3.one, 0.25f);

    [Header("Look")]
    [SerializeField] private bool lookHorizontalOnly = true;

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private Vector3 initialLocalScale = Vector3.one;
    private Coroutine poseRoutine;

    private void Awake()
    {
        ResolveReferences();
        CaptureInitialPose();
    }

    private void OnValidate()
    {
        SanitizePose(sleepPose);
        SanitizePose(nightmareWakePose);
        SanitizePose(comfortedPose);
        SanitizePose(morningPose);
    }

    public void SetVisible(bool isVisible)
    {
        if (!isVisible && resetPoseWhenHidden)
        {
            ResetToInitialPose();
        }

        if (visibleRoot != null)
        {
            visibleRoot.SetActive(isVisible);
        }
        else
        {
            gameObject.SetActive(isVisible);
        }
    }

    public void PlaySleep()
    {
        PlayCue(sleepTrigger, sleepPose);
    }

    public void PlayNightmareWake()
    {
        PlayCue(nightmareTrigger, nightmareWakePose);
    }

    public void PlayComforted()
    {
        PlayCue(comfortedTrigger, comfortedPose);
    }

    public void PlayMorning()
    {
        PlayCue(morningTrigger, morningPose);
    }

    public void PlayTrigger(string triggerName)
    {
        TryPlayTrigger(triggerName);
    }

    public void ResetToInitialPose()
    {
        ResolveReferences();
        StopPoseRoutine();

        if (poseRoot == null)
        {
            return;
        }

        poseRoot.localPosition = initialLocalPosition;
        poseRoot.localRotation = initialLocalRotation;
        poseRoot.localScale = initialLocalScale;
    }

    public void StopPose()
    {
        StopPoseRoutine();
    }

    public void SetAnimatorBool(string parameterName, bool value)
    {
        if (animator == null || string.IsNullOrEmpty(parameterName))
        {
            return;
        }

        animator.SetBool(parameterName, value);
    }

    public void LookAt(Transform target)
    {
        if (target == null)
        {
            return;
        }

        LookAtPosition(target.position);
    }

    public void LookAtPosition(Vector3 worldPosition)
    {
        ResolveReferences();

        if (lookRoot == null)
        {
            return;
        }

        Vector3 direction = worldPosition - lookRoot.position;
        if (lookHorizontalOnly)
        {
            direction.y = 0f;
        }

        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        lookRoot.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }

    [ContextMenu("Capture Sleep Pose From Current")]
    public void CaptureSleepPoseFromCurrent()
    {
        CapturePoseFromCurrent(sleepPose);
    }

    [ContextMenu("Capture Nightmare Wake Pose From Current")]
    public void CaptureNightmareWakePoseFromCurrent()
    {
        CapturePoseFromCurrent(nightmareWakePose);
    }

    [ContextMenu("Capture Comforted Pose From Current")]
    public void CaptureComfortedPoseFromCurrent()
    {
        CapturePoseFromCurrent(comfortedPose);
    }

    [ContextMenu("Capture Morning Pose From Current")]
    public void CaptureMorningPoseFromCurrent()
    {
        CapturePoseFromCurrent(morningPose);
    }

    private void ResolveReferences()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>(true);
        }

        if (lookRoot == null)
        {
            lookRoot = transform;
        }

        if (visibleRoot == null)
        {
            visibleRoot = gameObject;
        }

        if (poseRoot == null)
        {
            poseRoot = transform;
        }
    }

    private void PlayCue(string triggerName, LocalPoseCue poseCue)
    {
        bool playedAnimatorTrigger = TryPlayTrigger(triggerName);

        if (!ShouldPlayFallbackPose(playedAnimatorTrigger))
        {
            return;
        }

        PlayPose(poseCue);
    }

    private bool TryPlayTrigger(string triggerName)
    {
        if (animator == null || string.IsNullOrEmpty(triggerName) || !HasAnimatorTrigger(triggerName))
        {
            return false;
        }

        animator.ResetTrigger(triggerName);
        animator.SetTrigger(triggerName);
        return true;
    }

    private bool HasAnimatorTrigger(string triggerName)
    {
        AnimatorControllerParameter[] parameters = animator.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            AnimatorControllerParameter parameter = parameters[i];
            if (parameter.type == AnimatorControllerParameterType.Trigger && parameter.name == triggerName)
            {
                return true;
            }
        }

        return false;
    }

    private bool ShouldPlayFallbackPose(bool playedAnimatorTrigger)
    {
        if (applyFallbackPosesEvenWithAnimator)
        {
            return true;
        }

        return useFallbackPosesWhenNoAnimator && !playedAnimatorTrigger;
    }

    private void PlayPose(LocalPoseCue poseCue)
    {
        ResolveReferences();

        if (poseRoot == null || poseCue == null || !poseCue.enabled)
        {
            return;
        }

        StopPoseRoutine();

        if (poseCue.duration <= 0f)
        {
            ApplyPose(poseCue);
            return;
        }

        poseRoutine = StartCoroutine(PoseRoutine(poseCue));
    }

    private IEnumerator PoseRoutine(LocalPoseCue poseCue)
    {
        Vector3 startPosition = poseRoot.localPosition;
        Quaternion startRotation = poseRoot.localRotation;
        Vector3 startScale = poseRoot.localScale;
        Quaternion targetRotation = Quaternion.Euler(poseCue.localEulerAngles);
        float duration = Mathf.Max(0.0001f, poseCue.duration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easedT = poseCue.curve != null ? Mathf.Clamp01(poseCue.curve.Evaluate(t)) : t;

            poseRoot.localPosition = Vector3.LerpUnclamped(startPosition, poseCue.localPosition, easedT);
            poseRoot.localRotation = Quaternion.SlerpUnclamped(startRotation, targetRotation, easedT);
            poseRoot.localScale = Vector3.LerpUnclamped(startScale, poseCue.localScale, easedT);

            yield return null;
        }

        ApplyPose(poseCue);
        poseRoutine = null;
    }

    private void ApplyPose(LocalPoseCue poseCue)
    {
        poseRoot.localPosition = poseCue.localPosition;
        poseRoot.localRotation = Quaternion.Euler(poseCue.localEulerAngles);
        poseRoot.localScale = poseCue.localScale;
    }

    private void StopPoseRoutine()
    {
        if (poseRoutine != null)
        {
            StopCoroutine(poseRoutine);
            poseRoutine = null;
        }
    }

    private void CaptureInitialPose()
    {
        if (!captureInitialPoseOnAwake)
        {
            return;
        }

        ResolveReferences();
        if (poseRoot == null)
        {
            return;
        }

        initialLocalPosition = poseRoot.localPosition;
        initialLocalRotation = poseRoot.localRotation;
        initialLocalScale = poseRoot.localScale;
    }

    private void CapturePoseFromCurrent(LocalPoseCue targetPose)
    {
        ResolveReferences();

        if (poseRoot == null || targetPose == null)
        {
            return;
        }

        targetPose.localPosition = poseRoot.localPosition;
        targetPose.localEulerAngles = poseRoot.localEulerAngles;
        targetPose.localScale = poseRoot.localScale;
        targetPose.enabled = true;
    }

    private static LocalPoseCue CreatePose(Vector3 localPosition, Vector3 localEulerAngles, Vector3 localScale, float duration)
    {
        LocalPoseCue poseCue = new LocalPoseCue();
        poseCue.localPosition = localPosition;
        poseCue.localEulerAngles = localEulerAngles;
        poseCue.localScale = localScale;
        poseCue.duration = duration;
        return poseCue;
    }

    private static void SanitizePose(LocalPoseCue poseCue)
    {
        if (poseCue == null)
        {
            return;
        }

        poseCue.duration = Mathf.Max(0f, poseCue.duration);
        if (poseCue.localScale == Vector3.zero)
        {
            poseCue.localScale = Vector3.one;
        }
    }
}
