using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinLoopLightingStateController : MonoBehaviour
{
    [Serializable]
    public class LightingStageRule
    {
        public string label = "Lighting Rule";
        public MinLoopStage[] activeStages;

        [Header("Light Targets")]
        public Light[] lights;
        public bool setLightEnabled = true;
        public bool lightEnabled = true;
        public bool setLightColor = true;
        public Color lightColor = Color.white;
        public bool setLightIntensity = true;
        public float lightIntensity = 1f;
        public bool setLightRange;
        public float lightRange = 10f;

        [Header("Object Targets")]
        public GameObject[] activeObjects;
        public Behaviour[] behaviours;

        [Header("Ambient Optional")]
        public bool applyAmbientColor;
        public Color ambientColor = new Color(0.18f, 0.18f, 0.18f, 1f);
        public bool applyFog;
        public bool fogEnabled;
        public Color fogColor = Color.black;
        public float fogDensity = 0.01f;

        [Header("Transition")]
        [Min(0f)]
        public float transitionSeconds = 0.35f;

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
    [SerializeField] private LightingStageRule[] rules;
    [SerializeField] private bool disableUnmatchedRuleLights = true;

    [Header("Restore")]
    [SerializeField] private bool captureSceneAmbientOnAwake = true;
    [SerializeField] private bool restoreSceneAmbientOnDisable;

    private Color originalAmbientLight;
    private bool originalFogEnabled;
    private Color originalFogColor;
    private float originalFogDensity;
    private bool isListening;
    private readonly List<Coroutine> activeTransitions = new List<Coroutine>();

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
        CaptureSceneAmbient();
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
        StopActiveTransitions();

        if (restoreSceneAmbientOnDisable)
        {
            RestoreSceneAmbient();
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

        StopActiveTransitions();
        SanitizeRules();

        for (int i = 0; i < rules.Length; i++)
        {
            LightingStageRule rule = rules[i];
            if (rule == null)
            {
                continue;
            }

            bool matches = rule.Matches(stage);
            ApplyObjectTargets(rule, matches);

            if (matches)
            {
                ApplyMatchedRule(rule);
            }
            else if (disableUnmatchedRuleLights)
            {
                SetRuleLightsEnabled(rule, false, stage);
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

    public void RestoreSceneAmbient()
    {
        RenderSettings.ambientLight = originalAmbientLight;
        RenderSettings.fog = originalFogEnabled;
        RenderSettings.fogColor = originalFogColor;
        RenderSettings.fogDensity = originalFogDensity;
    }

    private void ApplyMatchedRule(LightingStageRule rule)
    {
        if (Application.isPlaying && rule.transitionSeconds > 0f)
        {
            Coroutine transition = StartCoroutine(TransitionMatchedRule(rule));
            activeTransitions.Add(transition);
            return;
        }

        ApplyLightTargets(rule);

        if (rule.applyAmbientColor)
        {
            RenderSettings.ambientLight = rule.ambientColor;
        }

        if (rule.applyFog)
        {
            RenderSettings.fog = rule.fogEnabled;
            RenderSettings.fogColor = rule.fogColor;
            RenderSettings.fogDensity = rule.fogDensity;
        }
    }

    private IEnumerator TransitionMatchedRule(LightingStageRule rule)
    {
        LightSnapshot[] snapshots = CaptureLightSnapshots(rule);
        Color ambientStart = RenderSettings.ambientLight;
        Color fogColorStart = RenderSettings.fogColor;
        float fogDensityStart = RenderSettings.fogDensity;

        if (rule.applyFog)
        {
            RenderSettings.fog = rule.fogEnabled;
        }

        if (rule.setLightEnabled && rule.lightEnabled && rule.lights != null)
        {
            for (int i = 0; i < rule.lights.Length; i++)
            {
                if (rule.lights[i] != null)
                {
                    rule.lights[i].enabled = true;
                }
            }
        }

        float elapsed = 0f;
        while (elapsed < rule.transitionSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / rule.transitionSeconds);

            ApplyTransitionFrame(rule, snapshots, ambientStart, fogColorStart, fogDensityStart, t);
            yield return null;
        }

        ApplyTransitionFrame(rule, snapshots, ambientStart, fogColorStart, fogDensityStart, 1f);

        if (rule.setLightEnabled && !rule.lightEnabled)
        {
            SetRuleLightsEnabledDirect(rule, false);
        }
    }

    private LightSnapshot[] CaptureLightSnapshots(LightingStageRule rule)
    {
        if (rule.lights == null || rule.lights.Length == 0)
        {
            return new LightSnapshot[0];
        }

        List<LightSnapshot> snapshots = new List<LightSnapshot>(rule.lights.Length);
        for (int i = 0; i < rule.lights.Length; i++)
        {
            Light targetLight = rule.lights[i];
            if (targetLight == null)
            {
                continue;
            }

            snapshots.Add(new LightSnapshot(targetLight));
        }

        return snapshots.ToArray();
    }

    private void ApplyTransitionFrame(
        LightingStageRule rule,
        LightSnapshot[] snapshots,
        Color ambientStart,
        Color fogColorStart,
        float fogDensityStart,
        float t)
    {
        for (int i = 0; i < snapshots.Length; i++)
        {
            Light targetLight = snapshots[i].light;
            if (targetLight == null)
            {
                continue;
            }

            if (rule.setLightColor)
            {
                targetLight.color = Color.Lerp(snapshots[i].color, rule.lightColor, t);
            }

            if (rule.setLightIntensity)
            {
                targetLight.intensity = Mathf.Lerp(snapshots[i].intensity, rule.lightIntensity, t);
            }

            if (rule.setLightRange)
            {
                targetLight.range = Mathf.Lerp(snapshots[i].range, rule.lightRange, t);
            }
        }

        if (rule.applyAmbientColor)
        {
            RenderSettings.ambientLight = Color.Lerp(ambientStart, rule.ambientColor, t);
        }

        if (rule.applyFog)
        {
            RenderSettings.fogColor = Color.Lerp(fogColorStart, rule.fogColor, t);
            RenderSettings.fogDensity = Mathf.Lerp(fogDensityStart, rule.fogDensity, t);
        }
    }

    private void ApplyLightTargets(LightingStageRule rule)
    {
        if (rule.lights == null)
        {
            return;
        }

        for (int i = 0; i < rule.lights.Length; i++)
        {
            Light targetLight = rule.lights[i];
            if (targetLight == null)
            {
                continue;
            }

            if (rule.setLightEnabled)
            {
                targetLight.enabled = rule.lightEnabled;
            }

            if (rule.setLightColor)
            {
                targetLight.color = rule.lightColor;
            }

            if (rule.setLightIntensity)
            {
                targetLight.intensity = rule.lightIntensity;
            }

            if (rule.setLightRange)
            {
                targetLight.range = rule.lightRange;
            }
        }
    }

    private void SetRuleLightsEnabled(LightingStageRule rule, bool enabled, MinLoopStage currentStage)
    {
        if (rule.lights == null)
        {
            return;
        }

        for (int i = 0; i < rule.lights.Length; i++)
        {
            if (rule.lights[i] != null && (enabled || !IsLightUsedByMatchedRule(rule.lights[i], currentStage)))
            {
                rule.lights[i].enabled = enabled;
            }
        }
    }

    private void SetRuleLightsEnabledDirect(LightingStageRule rule, bool enabled)
    {
        if (rule.lights == null)
        {
            return;
        }

        for (int i = 0; i < rule.lights.Length; i++)
        {
            if (rule.lights[i] != null)
            {
                rule.lights[i].enabled = enabled;
            }
        }
    }

    private void ApplyObjectTargets(LightingStageRule rule, bool active)
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

    private void CaptureSceneAmbient()
    {
        if (!captureSceneAmbientOnAwake)
        {
            return;
        }

        originalAmbientLight = RenderSettings.ambientLight;
        originalFogEnabled = RenderSettings.fog;
        originalFogColor = RenderSettings.fogColor;
        originalFogDensity = RenderSettings.fogDensity;
    }

    private bool IsLightUsedByMatchedRule(Light targetLight, MinLoopStage currentStage)
    {
        if (targetLight == null || rules == null)
        {
            return false;
        }

        for (int i = 0; i < rules.Length; i++)
        {
            LightingStageRule rule = rules[i];
            if (rule == null || !rule.Matches(currentStage) || rule.lights == null)
            {
                continue;
            }

            for (int lightIndex = 0; lightIndex < rule.lights.Length; lightIndex++)
            {
                if (rule.lights[lightIndex] == targetLight)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void StopActiveTransitions()
    {
        if (activeTransitions.Count == 0)
        {
            return;
        }

        for (int i = 0; i < activeTransitions.Count; i++)
        {
            if (activeTransitions[i] != null)
            {
                StopCoroutine(activeTransitions[i]);
            }
        }

        activeTransitions.Clear();
    }

    private void SanitizeRules()
    {
        if (rules == null)
        {
            return;
        }

        for (int i = 0; i < rules.Length; i++)
        {
            LightingStageRule rule = rules[i];
            if (rule == null)
            {
                continue;
            }

            rule.lightIntensity = Mathf.Max(0f, rule.lightIntensity);
            rule.lightRange = Mathf.Max(0f, rule.lightRange);
            rule.fogDensity = Mathf.Max(0f, rule.fogDensity);
            rule.transitionSeconds = Mathf.Max(0f, rule.transitionSeconds);
        }
    }

    private struct LightSnapshot
    {
        public readonly Light light;
        public readonly Color color;
        public readonly float intensity;
        public readonly float range;

        public LightSnapshot(Light light)
        {
            this.light = light;
            color = light.color;
            intensity = light.intensity;
            range = light.range;
        }
    }
}
