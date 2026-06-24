using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MinLoopStageCueController : MonoBehaviour
{
    public enum DoorCueAction
    {
        Open,
        Close,
        Toggle,
        SnapOpen,
        SnapClosed,
        Lock,
        Unlock
    }

    [Serializable]
    public class DoorCue
    {
        public SmartDoorController door;
        public DoorCueAction action = DoorCueAction.Open;
    }

    [Serializable]
    public class AnimatorCue
    {
        public Animator animator;
        public string triggerName;
        public bool resetTriggerBeforeSet;
    }

    [Serializable]
    public class AudioCue
    {
        public AudioSource audioSource;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
        public bool playOneShot = true;
        public bool stopBeforePlay;
    }

    [Serializable]
    public class StageCueRule
    {
        public string label = "Stage Cue";
        public MinLoopStage[] activeStages;
        public bool triggerOnce = true;
        [Min(0f)]
        public float delaySeconds;

        [Header("Built-in Cues")]
        public InteractionFeedbackController[] feedbackObjects;
        public DoorCue[] doorCues;
        public AnimatorCue[] animatorCues;
        public AudioCue[] audioCues;

        [Header("Object Toggles")]
        public GameObject[] enableObjects;
        public GameObject[] disableObjects;
        public Behaviour[] enableBehaviours;
        public Behaviour[] disableBehaviours;

        [Header("Events")]
        public UnityEvent cueEvent = new UnityEvent();

        public bool Matches(MinLoopStage stage)
        {
            if (activeStages == null || activeStages.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < activeStages.Length; i++)
            {
                if (activeStages[i] == stage)
                {
                    return true;
                }
            }

            return false;
        }
    }

    [Header("Flow")]
    [SerializeField] private MinLoopFlowController flowController;
    [SerializeField] private bool findFlowControllerOnAwake = true;
    [SerializeField] private bool applyCurrentStageOnEnable;

    [Header("Rules")]
    [SerializeField] private StageCueRule[] rules;
    [SerializeField] private bool resetTriggeredRulesOnCorridor = true;

    private readonly List<Coroutine> activeDelayRoutines = new List<Coroutine>();
    private bool[] triggeredRules;
    private bool isListening;

    public bool HasRules
    {
        get { return rules != null && rules.Length > 0; }
    }

    public int RuleCount
    {
        get { return rules != null ? rules.Length : 0; }
    }

    private void Awake()
    {
        ResolveFlowController();
        EnsureTriggeredState();
    }

    private void OnEnable()
    {
        ResolveFlowController();
        Subscribe();

        if (applyCurrentStageOnEnable)
        {
            ApplyCurrentStage();
        }
    }

    private void OnDisable()
    {
        Unsubscribe();
        StopDelayRoutines();
    }

    private void OnValidate()
    {
        SanitizeRules();
    }

    public void ApplyCurrentStage()
    {
        ResolveFlowController();

        if (flowController != null)
        {
            ApplyStage(flowController.CurrentStage);
        }
    }

    public void ApplyStage(MinLoopStage stage)
    {
        if (rules == null)
        {
            return;
        }

        EnsureTriggeredState();

        if (resetTriggeredRulesOnCorridor && stage == MinLoopStage.Corridor)
        {
            StopDelayRoutines();
            ResetTriggeredRules();
        }

        for (int i = 0; i < rules.Length; i++)
        {
            StageCueRule rule = rules[i];
            if (rule == null || !rule.Matches(stage))
            {
                continue;
            }

            if (rule.triggerOnce && triggeredRules[i])
            {
                continue;
            }

            triggeredRules[i] = true;
            if (rule.delaySeconds > 0f && Application.isPlaying)
            {
                activeDelayRoutines.Add(StartCoroutine(TriggerAfterDelay(rule, rule.delaySeconds)));
            }
            else
            {
                TriggerRule(rule);
            }
        }
    }

    public void SetFlowController(MinLoopFlowController controller)
    {
        if (flowController == controller)
        {
            return;
        }

        Unsubscribe();
        flowController = controller;
        Subscribe();
        ApplyCurrentStage();
    }

    public void ExecuteRuleNow(int ruleIndex)
    {
        if (rules == null || ruleIndex < 0 || ruleIndex >= rules.Length)
        {
            return;
        }

        TriggerRule(rules[ruleIndex]);
    }

    public void ResetTriggeredRules()
    {
        EnsureTriggeredState();

        for (int i = 0; i < triggeredRules.Length; i++)
        {
            triggeredRules[i] = false;
        }
    }

    private IEnumerator TriggerAfterDelay(StageCueRule rule, float delaySeconds)
    {
        yield return new WaitForSeconds(delaySeconds);
        TriggerRule(rule);
    }

    private void TriggerRule(StageCueRule rule)
    {
        if (rule == null)
        {
            return;
        }

        TriggerFeedbacks(rule.feedbackObjects);
        TriggerDoors(rule.doorCues);
        TriggerAnimators(rule.animatorCues);
        TriggerAudio(rule.audioCues);
        ToggleObjects(rule.enableObjects, true);
        ToggleObjects(rule.disableObjects, false);
        ToggleBehaviours(rule.enableBehaviours, true);
        ToggleBehaviours(rule.disableBehaviours, false);

        if (rule.cueEvent != null)
        {
            rule.cueEvent.Invoke();
        }
    }

    private void TriggerFeedbacks(InteractionFeedbackController[] feedbackObjects)
    {
        if (feedbackObjects == null)
        {
            return;
        }

        for (int i = 0; i < feedbackObjects.Length; i++)
        {
            if (feedbackObjects[i] != null)
            {
                feedbackObjects[i].PlayFeedback();
            }
        }
    }

    private void TriggerDoors(DoorCue[] doorCues)
    {
        if (doorCues == null)
        {
            return;
        }

        for (int i = 0; i < doorCues.Length; i++)
        {
            DoorCue cue = doorCues[i];
            if (cue == null || cue.door == null)
            {
                continue;
            }

            switch (cue.action)
            {
                case DoorCueAction.Open:
                    cue.door.Open();
                    break;
                case DoorCueAction.Close:
                    cue.door.Close();
                    break;
                case DoorCueAction.Toggle:
                    cue.door.Toggle();
                    break;
                case DoorCueAction.SnapOpen:
                    cue.door.SnapOpen();
                    break;
                case DoorCueAction.SnapClosed:
                    cue.door.SnapClosed();
                    break;
                case DoorCueAction.Lock:
                    cue.door.SetLocked(true);
                    break;
                case DoorCueAction.Unlock:
                    cue.door.SetLocked(false);
                    break;
            }
        }
    }

    private void TriggerAnimators(AnimatorCue[] animatorCues)
    {
        if (animatorCues == null)
        {
            return;
        }

        for (int i = 0; i < animatorCues.Length; i++)
        {
            AnimatorCue cue = animatorCues[i];
            if (cue == null || cue.animator == null || string.IsNullOrEmpty(cue.triggerName))
            {
                continue;
            }

            if (!HasAnimatorTrigger(cue.animator, cue.triggerName))
            {
                continue;
            }

            if (cue.resetTriggerBeforeSet)
            {
                cue.animator.ResetTrigger(cue.triggerName);
            }

            cue.animator.SetTrigger(cue.triggerName);
        }
    }

    private void TriggerAudio(AudioCue[] audioCues)
    {
        if (audioCues == null)
        {
            return;
        }

        for (int i = 0; i < audioCues.Length; i++)
        {
            AudioCue cue = audioCues[i];
            if (cue == null || cue.audioSource == null)
            {
                continue;
            }

            if (cue.stopBeforePlay)
            {
                cue.audioSource.Stop();
            }

            if (cue.clip != null)
            {
                if (cue.playOneShot)
                {
                    cue.audioSource.PlayOneShot(cue.clip, cue.volume);
                }
                else
                {
                    cue.audioSource.clip = cue.clip;
                    cue.audioSource.volume = cue.volume;
                    cue.audioSource.Play();
                }
            }
            else
            {
                cue.audioSource.Play();
            }
        }
    }

    private void ToggleObjects(GameObject[] targets, bool active)
    {
        if (targets == null)
        {
            return;
        }

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] != null)
            {
                targets[i].SetActive(active);
            }
        }
    }

    private void ToggleBehaviours(Behaviour[] targets, bool enabled)
    {
        if (targets == null)
        {
            return;
        }

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] != null)
            {
                targets[i].enabled = enabled;
            }
        }
    }

    private bool HasAnimatorTrigger(Animator animator, string triggerName)
    {
        if (animator.runtimeAnimatorController == null)
        {
            return false;
        }

        AnimatorControllerParameter[] parameters = animator.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].type == AnimatorControllerParameterType.Trigger &&
                parameters[i].name == triggerName)
            {
                return true;
            }
        }

        return false;
    }

    private void ResolveFlowController()
    {
        if (flowController == null && findFlowControllerOnAwake)
        {
            flowController = FindObjectOfType<MinLoopFlowController>();
        }
    }

    private void Subscribe()
    {
        if (isListening || flowController == null || flowController.StageChanged == null)
        {
            return;
        }

        flowController.StageChanged.AddListener(ApplyStage);
        isListening = true;
    }

    private void Unsubscribe()
    {
        if (!isListening || flowController == null || flowController.StageChanged == null)
        {
            isListening = false;
            return;
        }

        flowController.StageChanged.RemoveListener(ApplyStage);
        isListening = false;
    }

    private void EnsureTriggeredState()
    {
        int length = rules != null ? rules.Length : 0;
        if (triggeredRules != null && triggeredRules.Length == length)
        {
            return;
        }

        triggeredRules = new bool[length];
    }

    private void StopDelayRoutines()
    {
        for (int i = 0; i < activeDelayRoutines.Count; i++)
        {
            if (activeDelayRoutines[i] != null)
            {
                StopCoroutine(activeDelayRoutines[i]);
            }
        }

        activeDelayRoutines.Clear();
    }

    private void SanitizeRules()
    {
        if (rules == null)
        {
            return;
        }

        for (int i = 0; i < rules.Length; i++)
        {
            StageCueRule rule = rules[i];
            if (rule == null)
            {
                continue;
            }

            rule.delaySeconds = Mathf.Max(0f, rule.delaySeconds);
            SanitizeAudio(rule.audioCues);
        }
    }

    private void SanitizeAudio(AudioCue[] audioCues)
    {
        if (audioCues == null)
        {
            return;
        }

        for (int i = 0; i < audioCues.Length; i++)
        {
            if (audioCues[i] != null)
            {
                audioCues[i].volume = Mathf.Clamp01(audioCues[i].volume);
            }
        }
    }
}
