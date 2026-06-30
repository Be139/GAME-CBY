using System;
using UnityEngine;

[DisallowMultipleComponent]
public class HearthActorPosePreset : MonoBehaviour
{
    [Serializable]
    public class Pose
    {
        public string id = "Sleep";
        public Transform root;
        public Vector3 localPosition;
        public Vector3 localEulerAngles;
        public Vector3 localScale = Vector3.one;
    }

    [Header("Poses")]
    [SerializeField] private Transform defaultPoseRoot;
    [SerializeField] private Pose[] poses = Array.Empty<Pose>();

    [Header("Motion")]
    [SerializeField] private bool smoothApply = true;
    [SerializeField] private float defaultDuration = 0.25f;
    [SerializeField] private AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Coroutine activeRoutine;

    private void Awake()
    {
        if (defaultPoseRoot == null)
        {
            defaultPoseRoot = transform;
        }
    }

    private void OnValidate()
    {
        defaultDuration = Mathf.Max(0f, defaultDuration);
        if (defaultPoseRoot == null)
        {
            defaultPoseRoot = transform;
        }
    }

    public void ApplyPose(string poseId)
    {
        ApplyPose(poseId, defaultDuration);
    }

    public void ApplyPose(string poseId, float duration)
    {
        Pose pose = FindPose(poseId);
        if (pose == null)
        {
            Debug.LogWarning("[HearthActorPosePreset] Pose not found: " + poseId, this);
            return;
        }

        Transform targetRoot = pose.root != null ? pose.root : defaultPoseRoot;
        if (targetRoot == null)
        {
            return;
        }

        if (activeRoutine != null)
        {
            StopCoroutine(activeRoutine);
            activeRoutine = null;
        }

        if (!smoothApply || duration <= 0f)
        {
            ApplyPoseImmediate(targetRoot, pose);
            return;
        }

        activeRoutine = StartCoroutine(ApplyPoseRoutine(targetRoot, pose, duration));
    }

    [ContextMenu("Capture Current Pose As First Pose")]
    public void CaptureFirstPoseFromCurrent()
    {
        if (poses == null || poses.Length == 0)
        {
            poses = new[] { new Pose { id = "Captured", root = defaultPoseRoot } };
        }

        CaptureCurrentPose(poses[0].id);
    }

    public void CaptureCurrentPose(string poseId)
    {
        if (string.IsNullOrEmpty(poseId))
        {
            return;
        }

        Pose pose = FindPose(poseId);
        if (pose == null)
        {
            Array.Resize(ref poses, poses.Length + 1);
            pose = new Pose { id = poseId, root = defaultPoseRoot };
            poses[poses.Length - 1] = pose;
        }

        Transform targetRoot = pose.root != null ? pose.root : defaultPoseRoot;
        if (targetRoot == null)
        {
            return;
        }

        pose.localPosition = targetRoot.localPosition;
        pose.localEulerAngles = targetRoot.localEulerAngles;
        pose.localScale = targetRoot.localScale;
    }

    public void SetDefaultPoseRoot(Transform root)
    {
        defaultPoseRoot = root != null ? root : transform;
    }

    private Pose FindPose(string poseId)
    {
        if (poses == null)
        {
            return null;
        }

        for (int i = 0; i < poses.Length; i++)
        {
            Pose pose = poses[i];
            if (pose != null && string.Equals(pose.id, poseId, StringComparison.OrdinalIgnoreCase))
            {
                return pose;
            }
        }

        return null;
    }

    private System.Collections.IEnumerator ApplyPoseRoutine(Transform targetRoot, Pose pose, float duration)
    {
        Vector3 startPosition = targetRoot.localPosition;
        Quaternion startRotation = targetRoot.localRotation;
        Vector3 startScale = targetRoot.localScale;
        Quaternion targetRotation = Quaternion.Euler(pose.localEulerAngles);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = curve != null ? curve.Evaluate(t) : t;
            targetRoot.localPosition = Vector3.Lerp(startPosition, pose.localPosition, eased);
            targetRoot.localRotation = Quaternion.Slerp(startRotation, targetRotation, eased);
            targetRoot.localScale = Vector3.Lerp(startScale, pose.localScale, eased);
            yield return null;
        }

        ApplyPoseImmediate(targetRoot, pose);
        activeRoutine = null;
    }

    private static void ApplyPoseImmediate(Transform targetRoot, Pose pose)
    {
        targetRoot.localPosition = pose.localPosition;
        targetRoot.localRotation = Quaternion.Euler(pose.localEulerAngles);
        targetRoot.localScale = pose.localScale;
    }
}
