using System;
using UnityEngine;

public class MinLoopStageObjectActivator : MonoBehaviour
{
    [Serializable]
    public class StageActivationRule
    {
        public string label = "Stage Rule";
        public MinLoopStage[] activeStages;
        public bool invertMatch;
        public GameObject targetObject;
        public Renderer[] renderers;
        public Collider[] colliders;
        public Behaviour[] behaviours;

        public void Apply(MinLoopStage stage)
        {
            bool isActive = Matches(stage);
            if (invertMatch)
            {
                isActive = !isActive;
            }

            if (targetObject != null)
            {
                targetObject.SetActive(isActive);
            }

            SetRenderers(isActive);
            SetColliders(isActive);
            SetBehaviours(isActive);
        }

        private bool Matches(MinLoopStage stage)
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

        private void SetRenderers(bool isActive)
        {
            if (renderers == null)
            {
                return;
            }

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].enabled = isActive;
                }
            }
        }

        private void SetColliders(bool isActive)
        {
            if (colliders == null)
            {
                return;
            }

            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i] != null)
                {
                    colliders[i].enabled = isActive;
                }
            }
        }

        private void SetBehaviours(bool isActive)
        {
            if (behaviours == null)
            {
                return;
            }

            for (int i = 0; i < behaviours.Length; i++)
            {
                if (behaviours[i] != null)
                {
                    behaviours[i].enabled = isActive;
                }
            }
        }
    }

    [Header("Flow")]
    [SerializeField] private MinLoopFlowController flowController;
    [SerializeField] private bool findFlowControllerOnAwake = true;
    [SerializeField] private bool applyCurrentStageOnEnable = true;

    [Header("Rules")]
    [SerializeField] private StageActivationRule[] rules;

    private bool isListening;

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

        for (int i = 0; i < rules.Length; i++)
        {
            if (rules[i] != null)
            {
                rules[i].Apply(stage);
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
}
