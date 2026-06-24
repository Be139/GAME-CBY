using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinLoopObjectivePresenter : MonoBehaviour
{
    [Serializable]
    public class ObjectiveOverride
    {
        public MinLoopStage stage;
        public bool visible = true;
        public string title;

        [TextArea(2, 4)]
        public string body;
    }

    [Header("Flow")]
    [SerializeField] private MinLoopFlowController flowController;
    [SerializeField] private bool findFlowControllerOnAwake = true;
    [SerializeField] private bool refreshOnEnable = true;

    [Header("Bound UI Optional")]
    [SerializeField] private GameObject objectiveRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Fallback UI")]
    [SerializeField] private bool createFallbackUI = true;
    [SerializeField] private Color fallbackPanelColor = new Color(0f, 0f, 0f, 0.58f);

    [Header("Copy Overrides")]
    [SerializeField] private ObjectiveOverride[] objectiveOverrides;

    private bool isListening;

    private void Awake()
    {
        ResolveFlowController();
        EnsureReferences();
    }

    private void OnEnable()
    {
        ResolveFlowController();
        Subscribe();

        if (refreshOnEnable)
        {
            RefreshCurrentObjective();
        }
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void RefreshCurrentObjective()
    {
        ResolveFlowController();

        if (flowController != null)
        {
            ShowStage(flowController.CurrentStage);
        }
    }

    public void ShowStage(MinLoopStage stage)
    {
        ObjectiveText objective;
        if (!TryGetObjective(stage, out objective) || !objective.visible)
        {
            SetVisible(false);
            return;
        }

        EnsureReferences();
        SetVisible(true);

        if (titleText != null)
        {
            titleText.text = objective.title;
        }

        if (bodyText != null)
        {
            bodyText.text = objective.body;
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
        RefreshCurrentObjective();
    }

    private void OnDispositionApplied(MinLoopDispositionChoice choice, int trustValue, int delta)
    {
        string choiceLabel = choice == MinLoopDispositionChoice.SystemRecommendedA ? "A 系统推荐" : "B 低介入观察";
        string deltaPrefix = delta >= 0 ? "+" : string.Empty;
        ShowCustomObjective("处置已提交", choiceLabel + "；信任度 " + deltaPrefix + delta + "，当前 " + trustValue + "。");
    }

    private void ShowCustomObjective(string title, string body)
    {
        EnsureReferences();
        SetVisible(true);

        if (titleText != null)
        {
            titleText.text = title;
        }

        if (bodyText != null)
        {
            bodyText.text = body;
        }
    }

    private bool TryGetObjective(MinLoopStage stage, out ObjectiveText objective)
    {
        if (TryGetOverride(stage, out objective))
        {
            return true;
        }

        return TryGetDefaultObjective(stage, out objective);
    }

    private bool TryGetOverride(MinLoopStage stage, out ObjectiveText objective)
    {
        objective = default(ObjectiveText);

        if (objectiveOverrides == null)
        {
            return false;
        }

        for (int i = 0; i < objectiveOverrides.Length; i++)
        {
            ObjectiveOverride item = objectiveOverrides[i];
            if (item == null || item.stage != stage)
            {
                continue;
            }

            objective = new ObjectiveText(item.visible, item.title, item.body);
            return true;
        }

        return false;
    }

    private bool TryGetDefaultObjective(MinLoopStage stage, out ObjectiveText objective)
    {
        switch (stage)
        {
            case MinLoopStage.Corridor:
                objective = new ObjectiveText(true, "当前目标", "前往 17F-01 门口终端，刷工牌读取住户摘要。");
                return true;
            case MinLoopStage.AccessCard:
                objective = new ObjectiveText(true, "刷工牌", "在终端页面点击“刷工牌”。");
                return true;
            case MinLoopStage.ResidentSummary:
                objective = new ObjectiveText(true, "读取住户摘要", "阅读摘要后，调出昨夜事件。");
                return true;
            case MinLoopStage.SwitchingToCompanion:
                objective = new ObjectiveText(true, "接入陪伴单元", "正在切换到昨夜陪伴单元视角。");
                return true;
            case MinLoopStage.CompanionReplay:
                objective = new ObjectiveText(true, "复盘昨夜事件", "观察孩子醒来和陪伴单元回应。");
                return true;
            case MinLoopStage.WaitingForComfort:
                objective = new ObjectiveText(true, "执行唯一安抚操作", "看向床边安抚点，按 E。");
                return true;
            case MinLoopStage.Comforting:
                objective = new ObjectiveText(true, "安抚中", "等待孩子重新睡下。");
                return true;
            case MinLoopStage.MorningReview:
                objective = new ObjectiveText(true, "查看第二天反馈", "听取父母早晨对话。");
                return true;
            case MinLoopStage.ReturningToTerminal:
                objective = new ObjectiveText(true, "返回终端", "正在切回 Mia 视角。");
                return true;
            case MinLoopStage.DispositionChoice:
                objective = new ObjectiveText(true, "提交处置意见", "在终端选择 A 或 B。");
                return true;
            case MinLoopStage.Complete:
                objective = new ObjectiveText(true, "前往下一户", "查看 17F-02 指引。");
                return true;
            default:
                objective = default(ObjectiveText);
                return false;
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
        if (isListening || flowController == null)
        {
            return;
        }

        if (flowController.StageChanged != null)
        {
            flowController.StageChanged.AddListener(ShowStage);
        }

        if (flowController.DispositionApplied != null)
        {
            flowController.DispositionApplied.AddListener(OnDispositionApplied);
        }

        isListening = true;
    }

    private void Unsubscribe()
    {
        if (!isListening || flowController == null)
        {
            isListening = false;
            return;
        }

        if (flowController.StageChanged != null)
        {
            flowController.StageChanged.RemoveListener(ShowStage);
        }

        if (flowController.DispositionApplied != null)
        {
            flowController.DispositionApplied.RemoveListener(OnDispositionApplied);
        }

        isListening = false;
    }

    private void EnsureReferences()
    {
        if (objectiveRoot != null && titleText != null && bodyText != null)
        {
            if (canvasGroup == null)
            {
                canvasGroup = objectiveRoot.GetComponent<CanvasGroup>();
            }

            return;
        }

        if (createFallbackUI)
        {
            CreateFallbackUI();
        }
    }

    private void SetVisible(bool visible)
    {
        if (objectiveRoot != null)
        {
            objectiveRoot.SetActive(visible);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
        }
    }

    private void CreateFallbackUI()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Min Loop Objective Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
        }

        if (objectiveRoot == null)
        {
            GameObject panelObject = new GameObject("Min Loop Objective Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            panelObject.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.025f, 0.78f);
            panelRect.anchorMax = new Vector2(0.36f, 0.96f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = fallbackPanelColor;

            canvasGroup = panelObject.GetComponent<CanvasGroup>();
            objectiveRoot = panelObject;
        }

        if (titleText == null)
        {
            titleText = CreateText(objectiveRoot.transform, "Objective Title", new Vector2(0.055f, 0.56f), new Vector2(0.945f, 0.9f), 26f, FontStyles.Bold);
        }

        if (bodyText == null)
        {
            bodyText = CreateText(objectiveRoot.transform, "Objective Body", new Vector2(0.055f, 0.12f), new Vector2(0.945f, 0.56f), 20f, FontStyles.Normal);
        }
    }

    private TMP_Text CreateText(Transform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax, float fontSize, FontStyles style)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.color = Color.white;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = TextAlignmentOptions.Left;
        text.enableWordWrapping = true;
        return text;
    }

    private struct ObjectiveText
    {
        public readonly bool visible;
        public readonly string title;
        public readonly string body;

        public ObjectiveText(bool visible, string title, string body)
        {
            this.visible = visible;
            this.title = title;
            this.body = body;
        }
    }
}
