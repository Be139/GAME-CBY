using System;
using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class HearthActorAnimatorDriver : MonoBehaviour
{
    [Serializable]
    public class StateSlot
    {
        public string stateId;
        public string stateName;
        public AnimationClip clip;
        public bool loop = true;
        public bool applyRootMotion;
        public float fadeSeconds = 0.18f;
        public float playbackSpeed = 1f;
    }

    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("States")]
    [SerializeField] private StateSlot[] states = Array.Empty<StateSlot>();

    [Header("Startup")]
    [SerializeField] private bool playOnEnable;
    [SerializeField] private string playOnEnableStateId;

    private int activeStateHash;
    private string activeStateId;

    public Animator Animator
    {
        get
        {
            ResolveAnimator();
            return animator;
        }
    }

    public string ActiveStateId
    {
        get { return activeStateId; }
    }

    private void OnEnable()
    {
        if (playOnEnable && !string.IsNullOrEmpty(playOnEnableStateId))
        {
            Play(playOnEnableStateId);
        }
    }

    private void OnDisable()
    {
        if (animator != null)
        {
            animator.speed = 1f;
        }
    }

    public float Play(string stateId)
    {
        StateSlot slot = FindState(stateId);
        if (slot == null)
        {
            return 0f;
        }

        return PlayInternal(slot);
    }

    public float PlayLoop(string stateId)
    {
        StateSlot slot = FindState(stateId);
        if (slot == null)
        {
            return 0f;
        }

        slot.loop = true;
        return PlayInternal(slot);
    }

    public float PlayOnce(string stateId)
    {
        StateSlot slot = FindState(stateId);
        if (slot == null)
        {
            return 0f;
        }

        slot.loop = false;
        return PlayInternal(slot);
    }

    public float HoldStateAtStart(string stateId)
    {
        StateSlot slot = FindState(stateId);
        ResolveAnimator();
        if (slot == null || animator == null || string.IsNullOrEmpty(slot.stateName))
        {
            return 0f;
        }

        string statePath = ResolveStatePath(slot.stateName);
        if (string.IsNullOrEmpty(statePath))
        {
            Debug.LogWarning("[HearthActorAnimatorDriver] Animator state was not found: " + slot.stateName, this);
            return 0f;
        }

        animator.enabled = true;
        animator.applyRootMotion = slot.applyRootMotion;
        animator.speed = 1f;
        RestoreRootMotionChildBaseline();
        activeStateHash = Animator.StringToHash(statePath);
        activeStateId = slot.stateId;
        animator.Play(activeStateHash, 0, 0f);
        animator.Update(0f);
        RestoreRootMotionChildBaseline();
        animator.speed = 0f;
        return GetStateLength(stateId);
    }

    public void ResumePlayback()
    {
        ResolveAnimator();
        if (animator != null)
        {
            StateSlot slot = FindState(activeStateId);
            animator.speed = slot != null ? Mathf.Max(0.01f, slot.playbackSpeed) : 1f;
        }
    }

    public IEnumerator WaitForStateCompletion(string stateId, bool useUnscaledTime = false)
    {
        float duration = GetStateLength(stateId);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            yield return null;
        }
    }

    public void StopAndHold()
    {
        ResolveAnimator();
        if (animator == null)
        {
            return;
        }

        animator.speed = 0f;
    }

    public void StopPlayback()
    {
        ResolveAnimator();
        if (animator == null)
        {
            return;
        }

        animator.speed = 0f;
    }

    public void SetRootMotion(bool value)
    {
        ResolveAnimator();
        if (animator != null)
        {
            animator.applyRootMotion = value;
        }
    }

    public bool HasState(string stateId)
    {
        return FindState(stateId) != null;
    }

    public float GetStateLength(string stateId)
    {
        StateSlot slot = FindState(stateId);
        if (slot == null || slot.clip == null)
        {
            return 0f;
        }

        return slot.clip.length / Mathf.Max(0.01f, slot.playbackSpeed);
    }

    private float PlayInternal(StateSlot slot)
    {
        ResolveAnimator();
        if (animator == null || slot == null || string.IsNullOrEmpty(slot.stateName))
        {
            return 0f;
        }

        animator.enabled = true;
        animator.speed = Mathf.Max(0.01f, slot.playbackSpeed);
        animator.applyRootMotion = slot.applyRootMotion;
        RestoreRootMotionChildBaseline();

        string statePath = ResolveStatePath(slot.stateName);
        if (string.IsNullOrEmpty(statePath))
        {
            Debug.LogWarning(
                "[HearthActorAnimatorDriver] Animator state was not found: " + slot.stateName,
                this);
            return 0f;
        }

        int stateHash = Animator.StringToHash(statePath);
        float fade = Mathf.Max(0f, slot.fadeSeconds);
        if (fade > 0f && activeStateHash != 0)
        {
            animator.CrossFadeInFixedTime(stateHash, fade, 0, 0f);
        }
        else
        {
            animator.Play(stateHash, 0, 0f);
        }

        animator.Update(0f);
        RestoreRootMotionChildBaseline();
        activeStateHash = stateHash;
        activeStateId = slot.stateId;
        return slot.clip != null ? slot.clip.length / Mathf.Max(0.01f, slot.playbackSpeed) : 0f;
    }

    private string ResolveStatePath(string stateName)
    {
        if (animator == null || string.IsNullOrEmpty(stateName))
        {
            return null;
        }

        string fullPath = "Base Layer." + stateName;
        if (animator.HasState(0, Animator.StringToHash(fullPath)))
        {
            return fullPath;
        }

        if (animator.HasState(0, Animator.StringToHash(stateName)))
        {
            return stateName;
        }

        return null;
    }

    private void RestoreRootMotionChildBaseline()
    {
        if (animator == null)
        {
            return;
        }

        HearthActorRootMotionRelay relay = animator.GetComponent<HearthActorRootMotionRelay>();
        if (relay != null)
        {
            relay.RestoreAnimatorChildBaseline();
        }
    }

    private StateSlot FindState(string stateId)
    {
        if (string.IsNullOrEmpty(stateId) || states == null)
        {
            return null;
        }

        for (int i = 0; i < states.Length; i++)
        {
            StateSlot slot = states[i];
            if (slot != null && string.Equals(slot.stateId, stateId, StringComparison.OrdinalIgnoreCase))
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
