using System;
using UnityEngine;

public class MinLoopStageAnchorController : MonoBehaviour
{
    [Serializable]
    public class StageAnchorRule
    {
        public string label = "Stage Anchor";
        public MinLoopStage[] activeStages;

        [Header("Target")]
        public Transform targetRoot;
        public Transform anchor;
        public Rigidbody targetRigidbody;
        public CharacterController targetCharacterController;
        public FirstPersonLook firstPersonLook;
        public Transform lookTransform;

        [Header("Apply")]
        public bool applyPosition = true;
        public bool applyRotation = true;
        public bool yawOnly = true;
        public bool resetLookLocalRotation = true;
        public bool syncFirstPersonLook = true;
        public bool clearRigidbodyVelocity = true;

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
    [SerializeField] private StageAnchorRule[] rules;
    [SerializeField] private bool onlyFirstMatchingRulePerStage;

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
        ResolveRuleReferences();
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

        ResolveRuleReferences();

        for (int i = 0; i < rules.Length; i++)
        {
            StageAnchorRule rule = rules[i];
            if (rule == null || !rule.Matches(stage))
            {
                continue;
            }

            ApplyRule(rule);

            if (onlyFirstMatchingRulePerStage)
            {
                return;
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

    public void ApplyRuleNow(int ruleIndex)
    {
        if (rules == null || ruleIndex < 0 || ruleIndex >= rules.Length)
        {
            return;
        }

        ApplyRule(rules[ruleIndex]);
    }

    private void ApplyRule(StageAnchorRule rule)
    {
        if (rule == null || rule.targetRoot == null || rule.anchor == null)
        {
            return;
        }

        bool controllerWasEnabled = rule.targetCharacterController != null && rule.targetCharacterController.enabled;
        if (controllerWasEnabled)
        {
            rule.targetCharacterController.enabled = false;
        }

        ClearVelocity(rule);

        if (rule.applyPosition)
        {
            SetPosition(rule);
        }

        if (rule.applyRotation)
        {
            SetRotation(rule);
        }

        if (rule.resetLookLocalRotation && rule.lookTransform != null)
        {
            rule.lookTransform.localRotation = Quaternion.identity;
        }

        if (rule.syncFirstPersonLook && rule.firstPersonLook != null)
        {
            rule.firstPersonLook.ForceLookFromCurrentTransforms();
        }

        if (controllerWasEnabled)
        {
            rule.targetCharacterController.enabled = true;
        }

        ClearVelocity(rule);
    }

    private void SetPosition(StageAnchorRule rule)
    {
        if (rule.targetRigidbody != null)
        {
            rule.targetRigidbody.position = rule.anchor.position;
        }
        else
        {
            rule.targetRoot.position = rule.anchor.position;
        }
    }

    private void SetRotation(StageAnchorRule rule)
    {
        Quaternion targetRotation = rule.anchor.rotation;
        if (rule.yawOnly)
        {
            Vector3 euler = rule.targetRoot.rotation.eulerAngles;
            euler.y = rule.anchor.rotation.eulerAngles.y;
            targetRotation = Quaternion.Euler(euler);
        }

        if (rule.targetRigidbody != null)
        {
            rule.targetRigidbody.rotation = targetRotation;
        }
        else
        {
            rule.targetRoot.rotation = targetRotation;
        }
    }

    private void ClearVelocity(StageAnchorRule rule)
    {
        if (!rule.clearRigidbodyVelocity || rule.targetRigidbody == null)
        {
            return;
        }

        rule.targetRigidbody.velocity = Vector3.zero;
        rule.targetRigidbody.angularVelocity = Vector3.zero;
    }

    private void ResolveFlowController()
    {
        if (flowController == null && findFlowControllerOnAwake)
        {
            flowController = FindObjectOfType<MinLoopFlowController>();
        }
    }

    private void ResolveRuleReferences()
    {
        if (rules == null)
        {
            return;
        }

        for (int i = 0; i < rules.Length; i++)
        {
            StageAnchorRule rule = rules[i];
            if (rule == null || rule.targetRoot == null)
            {
                continue;
            }

            if (rule.targetRigidbody == null)
            {
                rule.targetRigidbody = rule.targetRoot.GetComponent<Rigidbody>();
                if (rule.targetRigidbody == null)
                {
                    rule.targetRigidbody = rule.targetRoot.GetComponentInChildren<Rigidbody>(true);
                }
            }

            if (rule.targetCharacterController == null)
            {
                rule.targetCharacterController = rule.targetRoot.GetComponent<CharacterController>();
                if (rule.targetCharacterController == null)
                {
                    rule.targetCharacterController = rule.targetRoot.GetComponentInChildren<CharacterController>(true);
                }
            }

            if (rule.firstPersonLook == null)
            {
                rule.firstPersonLook = rule.targetRoot.GetComponentInChildren<FirstPersonLook>(true);
            }
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
}
