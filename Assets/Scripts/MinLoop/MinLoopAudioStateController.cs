using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinLoopAudioStateController : MonoBehaviour
{
    [Serializable]
    public class AudioStageRule
    {
        public string label = "Audio Rule";
        public MinLoopStage[] activeStages;

        [Header("Audio Targets")]
        public AudioSource[] audioSources;
        public AudioClip fallbackClip;
        public bool assignFallbackClipIfMissing = true;
        public bool loop = true;
        [Range(0f, 1f)]
        public float targetVolume = 0.25f;
        public bool setSpatialBlend = true;
        [Range(0f, 1f)]
        public float spatialBlend;
        public bool setPitch;
        [Min(0.01f)]
        public float pitch = 1f;
        public bool restartOnEnter;
        public bool stopWhenUnmatched = true;

        [Header("Transition")]
        [Min(0f)]
        public float fadeInSeconds = 0.45f;
        [Min(0f)]
        public float fadeOutSeconds = 0.35f;

        [Header("Object Targets")]
        public GameObject[] activeObjects;
        public Behaviour[] behaviours;

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
    [SerializeField] private bool applyCurrentStageOnEnable = true;

    [Header("Rules")]
    [SerializeField] private AudioStageRule[] rules;

    [Header("Disable")]
    [SerializeField] private bool stopAudioOnDisable = true;

    private readonly Dictionary<AudioSource, Coroutine> activeFades = new Dictionary<AudioSource, Coroutine>();
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
        StopActiveFades();

        if (stopAudioOnDisable)
        {
            StopAllAudioImmediate();
        }
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

        SanitizeRules();

        for (int i = 0; i < rules.Length; i++)
        {
            AudioStageRule rule = rules[i];
            if (rule == null)
            {
                continue;
            }

            bool matches = rule.Matches(stage);
            ApplyObjectTargets(rule, matches);

            if (matches)
            {
                PlayRule(rule);
            }
            else if (rule.stopWhenUnmatched)
            {
                StopRule(rule, stage);
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

    public void StopAllAudio()
    {
        if (rules == null)
        {
            return;
        }

        for (int i = 0; i < rules.Length; i++)
        {
            AudioStageRule rule = rules[i];
            if (rule == null || rule.audioSources == null)
            {
                continue;
            }

            for (int sourceIndex = 0; sourceIndex < rule.audioSources.Length; sourceIndex++)
            {
                FadeSource(rule.audioSources[sourceIndex], 0f, rule.fadeOutSeconds, true);
            }
        }
    }

    private void PlayRule(AudioStageRule rule)
    {
        if (rule.audioSources == null)
        {
            return;
        }

        for (int i = 0; i < rule.audioSources.Length; i++)
        {
            AudioSource source = rule.audioSources[i];
            if (source == null)
            {
                continue;
            }

            PrepareSource(rule, source);

            if (source.clip == null)
            {
                continue;
            }

            if (rule.restartOnEnter && source.isPlaying)
            {
                source.Stop();
            }

            if (!source.isPlaying)
            {
                if (rule.fadeInSeconds > 0f)
                {
                    source.volume = 0f;
                }

                source.Play();
            }

            FadeSource(source, rule.targetVolume, rule.fadeInSeconds, false);
        }
    }

    private void StopRule(AudioStageRule rule, MinLoopStage currentStage)
    {
        if (rule.audioSources == null)
        {
            return;
        }

        for (int i = 0; i < rule.audioSources.Length; i++)
        {
            AudioSource source = rule.audioSources[i];
            if (source == null || IsSourceUsedByMatchedRule(source, currentStage))
            {
                continue;
            }

            FadeSource(source, 0f, rule.fadeOutSeconds, true);
        }
    }

    private void PrepareSource(AudioStageRule rule, AudioSource source)
    {
        if (!source.enabled)
        {
            source.enabled = true;
        }

        if (rule.assignFallbackClipIfMissing && source.clip == null && rule.fallbackClip != null)
        {
            source.clip = rule.fallbackClip;
        }

        source.loop = rule.loop;

        if (rule.setSpatialBlend)
        {
            source.spatialBlend = rule.spatialBlend;
        }

        if (rule.setPitch)
        {
            source.pitch = rule.pitch;
        }
    }

    private void FadeSource(AudioSource source, float targetVolume, float fadeSeconds, bool stopWhenFinished)
    {
        if (source == null)
        {
            return;
        }

        if (activeFades.TryGetValue(source, out Coroutine existingFade) && existingFade != null)
        {
            StopCoroutine(existingFade);
        }

        targetVolume = Mathf.Clamp01(targetVolume);
        fadeSeconds = Mathf.Max(0f, fadeSeconds);

        if (!Application.isPlaying || fadeSeconds <= 0f)
        {
            source.volume = targetVolume;
            if (stopWhenFinished && Mathf.Approximately(targetVolume, 0f))
            {
                source.Stop();
            }

            activeFades.Remove(source);
            return;
        }

        Coroutine fade = StartCoroutine(FadeRoutine(source, targetVolume, fadeSeconds, stopWhenFinished));
        activeFades[source] = fade;
    }

    private IEnumerator FadeRoutine(AudioSource source, float targetVolume, float fadeSeconds, bool stopWhenFinished)
    {
        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < fadeSeconds && source != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeSeconds);
            source.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        if (source != null)
        {
            source.volume = targetVolume;

            if (stopWhenFinished && Mathf.Approximately(targetVolume, 0f))
            {
                source.Stop();
            }

            activeFades.Remove(source);
        }
    }

    private void ApplyObjectTargets(AudioStageRule rule, bool active)
    {
        if (rule.activeObjects != null)
        {
            for (int i = 0; i < rule.activeObjects.Length; i++)
            {
                if (rule.activeObjects[i] != null)
                {
                    rule.activeObjects[i].SetActive(active);
                }
            }
        }

        if (rule.behaviours != null)
        {
            for (int i = 0; i < rule.behaviours.Length; i++)
            {
                if (rule.behaviours[i] != null)
                {
                    rule.behaviours[i].enabled = active;
                }
            }
        }
    }

    private bool IsSourceUsedByMatchedRule(AudioSource source, MinLoopStage currentStage)
    {
        if (source == null || rules == null)
        {
            return false;
        }

        for (int i = 0; i < rules.Length; i++)
        {
            AudioStageRule rule = rules[i];
            if (rule == null || !rule.Matches(currentStage) || rule.audioSources == null)
            {
                continue;
            }

            for (int sourceIndex = 0; sourceIndex < rule.audioSources.Length; sourceIndex++)
            {
                if (rule.audioSources[sourceIndex] == source)
                {
                    return true;
                }
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

    private void StopActiveFades()
    {
        foreach (KeyValuePair<AudioSource, Coroutine> pair in activeFades)
        {
            if (pair.Value != null)
            {
                StopCoroutine(pair.Value);
            }
        }

        activeFades.Clear();
    }

    private void StopAllAudioImmediate()
    {
        if (rules == null)
        {
            return;
        }

        for (int i = 0; i < rules.Length; i++)
        {
            AudioStageRule rule = rules[i];
            if (rule == null || rule.audioSources == null)
            {
                continue;
            }

            for (int sourceIndex = 0; sourceIndex < rule.audioSources.Length; sourceIndex++)
            {
                AudioSource source = rule.audioSources[sourceIndex];
                if (source == null)
                {
                    continue;
                }

                source.volume = 0f;
                source.Stop();
            }
        }
    }

    private void SanitizeRules()
    {
        if (rules == null)
        {
            return;
        }

        for (int i = 0; i < rules.Length; i++)
        {
            AudioStageRule rule = rules[i];
            if (rule == null)
            {
                continue;
            }

            rule.targetVolume = Mathf.Clamp01(rule.targetVolume);
            rule.spatialBlend = Mathf.Clamp01(rule.spatialBlend);
            rule.pitch = Mathf.Max(0.01f, rule.pitch);
            rule.fadeInSeconds = Mathf.Max(0f, rule.fadeInSeconds);
            rule.fadeOutSeconds = Mathf.Max(0f, rule.fadeOutSeconds);
        }
    }
}
