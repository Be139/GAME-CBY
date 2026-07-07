using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[DisallowMultipleComponent]
public class HearthActorAnimationPlayer : MonoBehaviour
{
    [Serializable]
    public class ClipSlot
    {
        public string clipId;
        public AnimationClip clip;
        public bool loop = true;
        public bool applyRootMotion;
        public bool applyFootIk = true;
        public bool stabilizeAnimatorTransform;
        public float fadeSeconds = 0.18f;
        public float playbackSpeed = 1f;
    }

    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Clips")]
    [SerializeField] private ClipSlot[] clips = Array.Empty<ClipSlot>();

    [Header("Startup")]
    [SerializeField] private bool playOnEnable;
    [SerializeField] private string playOnEnableClipId;

    [Header("Timing")]
    [SerializeField] private bool useUnscaledTime;

    private readonly Playable[] inputPlayables = new Playable[2];
    private PlayableGraph graph;
    private AnimationMixerPlayable mixer;
    private AnimationPlayableOutput output;
    private AnimationClipPlayable currentPlayable;
    private AnimationClip currentClip;
    private int currentInput = -1;
    private bool currentLoop;
    private bool currentStabilizeAnimatorTransform;
    private bool hasStableAnimatorTransform;
    private Vector3 stableAnimatorLocalPosition;
    private Quaternion stableAnimatorLocalRotation;
    private Vector3 stableAnimatorLocalScale;
    private float currentPlaybackSpeed = 1f;
    private Coroutine fadeRoutine;

    public bool HasAnimator
    {
        get
        {
            ResolveAnimator();
            return animator != null;
        }
    }

    private void OnEnable()
    {
        if (playOnEnable && !string.IsNullOrEmpty(playOnEnableClipId))
        {
            Play(playOnEnableClipId);
        }
    }

    private void Update()
    {
        if (!currentPlayable.IsValid() || currentClip == null)
        {
            return;
        }

        double length = Mathf.Max(0.0001f, currentClip.length);
        double time = currentPlayable.GetTime();
        if (currentLoop)
        {
            if (time >= length)
            {
                currentPlayable.SetTime(time % length);
                currentPlayable.SetDone(false);
            }

            return;
        }

        if (time >= length)
        {
            currentPlayable.SetTime(length);
            currentPlayable.SetSpeed(0d);
        }
    }

    private void LateUpdate()
    {
        RestoreStableAnimatorTransform();
    }

    private void OnDisable()
    {
        DestroyGraph();
    }

    private void OnDestroy()
    {
        DestroyGraph();
    }

    public float Play(string clipId)
    {
        ClipSlot slot = FindClip(clipId);
        if (slot == null)
        {
            return 0f;
        }

        return PlayInternal(slot, slot.loop);
    }

    public float PlayLoop(string clipId)
    {
        ClipSlot slot = FindClip(clipId);
        if (slot == null)
        {
            return 0f;
        }

        return PlayInternal(slot, true);
    }

    public float PlayOnce(string clipId)
    {
        ClipSlot slot = FindClip(clipId);
        if (slot == null)
        {
            return 0f;
        }

        return PlayInternal(slot, false);
    }

    public void StopAndHold()
    {
        if (!currentPlayable.IsValid())
        {
            return;
        }

        currentPlayable.SetSpeed(0d);
        if (currentClip != null)
        {
            double clampedTime = Math.Min(currentPlayable.GetTime(), currentClip.length);
            currentPlayable.SetTime(clampedTime);
        }
    }

    public void StopPlayback()
    {
        DestroyGraph();
    }

    public void SetRootMotion(bool value)
    {
        ResolveAnimator();
        if (animator != null)
        {
            animator.applyRootMotion = value;
        }
    }

    public bool HasClip(string clipId)
    {
        return FindClip(clipId) != null;
    }

    public float GetClipLength(string clipId)
    {
        ClipSlot slot = FindClip(clipId);
        if (slot == null || slot.clip == null)
        {
            return 0f;
        }

        float speed = Mathf.Max(0.01f, slot.playbackSpeed);
        return slot.clip.length / speed;
    }

    private float PlayInternal(ClipSlot slot, bool loop)
    {
        ResolveAnimator();
        if (animator == null || slot == null || slot.clip == null)
        {
            return 0f;
        }

        EnsureGraph();
        animator.enabled = true;
        animator.applyRootMotion = slot.applyRootMotion;
        currentStabilizeAnimatorTransform = slot.stabilizeAnimatorTransform;
        CaptureStableAnimatorTransform();

        int nextInput = currentInput == 0 ? 1 : 0;
        DestroyInput(nextInput);

        AnimationClipPlayable nextPlayable = AnimationClipPlayable.Create(graph, slot.clip);
        nextPlayable.SetApplyFootIK(slot.applyFootIk);
        currentPlaybackSpeed = Mathf.Max(0.01f, slot.playbackSpeed);
        nextPlayable.SetSpeed(currentPlaybackSpeed);
        nextPlayable.SetTime(0d);
        nextPlayable.SetDuration(slot.clip.length);

        graph.Connect(nextPlayable, 0, mixer, nextInput);
        mixer.SetInputWeight(nextInput, 0f);
        inputPlayables[nextInput] = (Playable)nextPlayable;

        int oldInput = currentInput;
        currentInput = nextInput;
        currentPlayable = nextPlayable;
        currentClip = slot.clip;
        currentLoop = loop;

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        float fade = Mathf.Max(0f, slot.fadeSeconds);
        if (oldInput < 0 || fade <= 0f || !inputPlayables[oldInput].IsValid())
        {
            mixer.SetInputWeight(nextInput, 1f);
            if (oldInput >= 0)
            {
                mixer.SetInputWeight(oldInput, 0f);
                DestroyInput(oldInput);
            }
        }
        else
        {
            fadeRoutine = StartCoroutine(FadeInputs(oldInput, nextInput, fade));
        }

        graph.Play();
        return slot.clip.length / currentPlaybackSpeed;
    }

    private void CaptureStableAnimatorTransform()
    {
        hasStableAnimatorTransform = false;
        if (!currentStabilizeAnimatorTransform || animator == null)
        {
            return;
        }

        Transform animatorTransform = animator.transform;
        stableAnimatorLocalPosition = animatorTransform.localPosition;
        stableAnimatorLocalRotation = animatorTransform.localRotation;
        stableAnimatorLocalScale = animatorTransform.localScale;
        hasStableAnimatorTransform = true;
    }

    private void RestoreStableAnimatorTransform()
    {
        if (!currentStabilizeAnimatorTransform || !hasStableAnimatorTransform || animator == null)
        {
            return;
        }

        Transform animatorTransform = animator.transform;
        animatorTransform.localPosition = stableAnimatorLocalPosition;
        animatorTransform.localRotation = stableAnimatorLocalRotation;
        animatorTransform.localScale = stableAnimatorLocalScale;
    }

    private System.Collections.IEnumerator FadeInputs(int oldInput, int newInput, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration && mixer.IsValid())
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            mixer.SetInputWeight(oldInput, 1f - t);
            mixer.SetInputWeight(newInput, t);
            yield return null;
        }

        if (mixer.IsValid())
        {
            mixer.SetInputWeight(newInput, 1f);
            mixer.SetInputWeight(oldInput, 0f);
        }

        DestroyInput(oldInput);
        fadeRoutine = null;
    }

    private void EnsureGraph()
    {
        if (graph.IsValid())
        {
            return;
        }

        graph = PlayableGraph.Create(name + "_ActorAnimationGraph");
        graph.SetTimeUpdateMode(useUnscaledTime ? DirectorUpdateMode.UnscaledGameTime : DirectorUpdateMode.GameTime);
        mixer = AnimationMixerPlayable.Create(graph, 2);
        output = AnimationPlayableOutput.Create(graph, name + "_ActorAnimationOutput", animator);
        output.SetSourcePlayable(mixer);
    }

    private void DestroyInput(int input)
    {
        if (input < 0 || input >= inputPlayables.Length)
        {
            return;
        }

        if (!inputPlayables[input].IsValid())
        {
            return;
        }

        if (mixer.IsValid())
        {
            mixer.DisconnectInput(input);
        }

        if (graph.IsValid())
        {
            graph.DestroySubgraph(inputPlayables[input]);
        }

        inputPlayables[input] = Playable.Null;
    }

    private void DestroyGraph()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        if (graph.IsValid())
        {
            graph.Destroy();
        }

        inputPlayables[0] = Playable.Null;
        inputPlayables[1] = Playable.Null;
        currentPlayable = default(AnimationClipPlayable);
        currentClip = null;
        currentInput = -1;
        currentStabilizeAnimatorTransform = false;
        hasStableAnimatorTransform = false;
    }

    private ClipSlot FindClip(string clipId)
    {
        if (string.IsNullOrEmpty(clipId) || clips == null)
        {
            return null;
        }

        for (int i = 0; i < clips.Length; i++)
        {
            ClipSlot slot = clips[i];
            if (slot != null && string.Equals(slot.clipId, clipId, StringComparison.OrdinalIgnoreCase))
            {
                return slot;
            }
        }

        return null;
    }

    private void ResolveAnimator()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>(true);
        }
    }
}
