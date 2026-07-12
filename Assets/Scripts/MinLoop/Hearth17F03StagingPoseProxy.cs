using UnityEngine;

[DisallowMultipleComponent]
[ExecuteAlways]
public class Hearth17F03StagingPoseProxy : MonoBehaviour
{
    [SerializeField] private Transform targetAnchor;
    [SerializeField] private string stageLabel;
    [SerializeField] private string previewAnimatorState;
    [SerializeField, Range(0f, 1f)] private float previewNormalizedTime = 0.2f;

    private bool previewPosePending = true;

    public Transform TargetAnchor
    {
        get { return targetAnchor; }
    }

    public string StageLabel
    {
        get { return stageLabel; }
    }

    public string PreviewAnimatorState
    {
        get { return previewAnimatorState; }
    }

    public float PreviewNormalizedTime
    {
        get { return previewNormalizedTime; }
    }

    private void OnEnable()
    {
        previewPosePending = true;
    }

    private void OnValidate()
    {
        previewNormalizedTime = Mathf.Clamp01(previewNormalizedTime);
        previewPosePending = true;
    }

    private void Update()
    {
        if (Application.isPlaying || !previewPosePending)
        {
            return;
        }

        previewPosePending = !ApplyConfiguredPreviewPose();
    }

    public void Configure(Transform anchor, string label, string animatorState, float normalizedTime)
    {
        targetAnchor = anchor;
        stageLabel = label;
        previewAnimatorState = animatorState;
        previewNormalizedTime = Mathf.Clamp01(normalizedTime);
        previewPosePending = true;
    }

    public bool ApplyPreviewPoseToAnchor()
    {
        if (targetAnchor == null)
        {
            return false;
        }

        targetAnchor.SetPositionAndRotation(transform.position, transform.rotation);
        return true;
    }

    public bool ResetPreviewFromAnchor()
    {
        if (targetAnchor == null)
        {
            return false;
        }

        transform.SetPositionAndRotation(targetAnchor.position, targetAnchor.rotation);
        return true;
    }

    public bool ApplyConfiguredPreviewPose()
    {
        if (Application.isPlaying || string.IsNullOrEmpty(previewAnimatorState))
        {
            return false;
        }

        Animator[] animators = GetComponentsInChildren<Animator>(true);
        bool applied = false;
        for (int i = 0; i < animators.Length; i++)
        {
            Animator animator = animators[i];
            if (animator == null || animator.runtimeAnimatorController == null)
            {
                continue;
            }

            int stateHash = Animator.StringToHash("Base Layer." + previewAnimatorState);
            if (!animator.HasState(0, stateHash))
            {
                stateHash = Animator.StringToHash(previewAnimatorState);
                if (!animator.HasState(0, stateHash))
                {
                    continue;
                }
            }

            animator.enabled = true;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.speed = 1f;
            animator.Play(stateHash, 0, previewNormalizedTime);
            animator.Update(0f);
            animator.speed = 0f;
            applied = true;
        }

        previewPosePending = !applied;
        return applied;
    }
}
