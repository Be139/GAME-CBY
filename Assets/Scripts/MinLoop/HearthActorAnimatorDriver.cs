using System;
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

    public Animator Animator
    {
        get
        {
            ResolveAnimator();
            return animator;
        }
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
        activeStateHash = stateHash;
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
