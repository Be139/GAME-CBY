using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinLoopRobotHudPresenter : MonoBehaviour
{
    [Serializable]
    public class HudStageOverride
    {
        public MinLoopStage stage;
        public bool visible = true;
        public string timeLabel;
        public string heartRateLabel;
        public string statusLabel;

        [TextArea(2, 4)]
        public string instructionLabel;

        public Color accentColor = new Color(0.28f, 0.86f, 1f, 1f);
    }

    [Header("Flow")]
    [SerializeField] private MinLoopFlowController flowController;
    [SerializeField] private bool findFlowControllerOnAwake = true;
    [SerializeField] private bool refreshOnEnable = true;

    [Header("Bound UI Optional")]
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text heartRateText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private Image accentImage;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Fallback UI")]
    [SerializeField] private bool createFallbackUI = true;
    [SerializeField] private Canvas fallbackParentCanvas;
    [SerializeField] private Color fallbackPanelColor = new Color(0.02f, 0.07f, 0.09f, 0.72f);
    [SerializeField] private Color normalAccentColor = new Color(0.28f, 0.86f, 1f, 1f);
    [SerializeField] private Color alertAccentColor = new Color(1f, 0.58f, 0.28f, 1f);

    [Header("Display")]
    [SerializeField] private bool hideOutsideCompanionReplay = true;
    [SerializeField] private HudStageOverride[] stageOverrides;

    private bool isListening;

    public bool HasBoundUI
    {
        get { return hudRoot != null && timeText != null && statusText != null; }
    }

    public bool CanCreateFallbackUI
    {
        get { return createFallbackUI; }
    }

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
            RefreshCurrentHud();
        }
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void RefreshCurrentHud()
    {
        ResolveFlowController();

        if (flowController != null)
        {
            ShowStage(flowController.CurrentStage);
            return;
        }

        SetVisible(false);
    }

    public void ShowStage(MinLoopStage stage)
    {
        HudState state;
        if (!TryGetHudState(stage, out state) || !state.visible)
        {
            SetVisible(false);
            return;
        }

        EnsureReferences();
        SetVisible(true);

        if (timeText != null)
        {
            timeText.text = state.timeLabel;
        }

        if (heartRateText != null)
        {
            heartRateText.text = state.heartRateLabel;
        }

        if (statusText != null)
        {
            statusText.text = state.statusLabel;
        }

        if (instructionText != null)
        {
            instructionText.text = state.instructionLabel;
        }

        if (accentImage != null)
        {
            accentImage.color = state.accentColor;
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
        RefreshCurrentHud();
    }

    private bool TryGetHudState(MinLoopStage stage, out HudState state)
    {
        HudState defaultState;
        bool hasDefault = TryGetDefaultHudState(stage, out defaultState);

        if (TryGetOverride(stage, defaultState, out state))
        {
            return true;
        }

        if (hasDefault)
        {
            state = defaultState;
            return true;
        }

        state = new HudState(!hideOutsideCompanionReplay, "--:--", "心率 --", "待机", "等待巡检流程。", normalAccentColor);
        return !hideOutsideCompanionReplay;
    }

    private bool TryGetOverride(MinLoopStage stage, HudState fallback, out HudState state)
    {
        state = default(HudState);

        if (stageOverrides == null)
        {
            return false;
        }

        for (int i = 0; i < stageOverrides.Length; i++)
        {
            HudStageOverride item = stageOverrides[i];
            if (item == null || item.stage != stage)
            {
                continue;
            }

            state = new HudState(
                item.visible,
                UseOverrideOrFallback(item.timeLabel, fallback.timeLabel),
                UseOverrideOrFallback(item.heartRateLabel, fallback.heartRateLabel),
                UseOverrideOrFallback(item.statusLabel, fallback.statusLabel),
                UseOverrideOrFallback(item.instructionLabel, fallback.instructionLabel),
                item.accentColor);
            return true;
        }

        return false;
    }

    private bool TryGetDefaultHudState(MinLoopStage stage, out HudState state)
    {
        switch (stage)
        {
            case MinLoopStage.SwitchingToCompanion:
                state = new HudState(true, "02:47", "心率 --", "接入昨夜记录", "正在切换至陪伴单元第一人称视角。", normalAccentColor);
                return true;
            case MinLoopStage.CompanionReplay:
                state = new HudState(true, "02:47", "心率 124", "噩梦可能性高", "观察儿童醒来后的第一反应。", alertAccentColor);
                return true;
            case MinLoopStage.WaitingForComfort:
                state = new HudState(true, "02:48", "心率 132", "儿童呼唤家属", "执行唯一安抚操作：床边低刺激引导。", alertAccentColor);
                return true;
            case MinLoopStage.Comforting:
                state = new HudState(true, "02:49", "心率 104", "安抚执行中", "陪伴单元正在引导呼吸，等待心率回落。", normalAccentColor);
                return true;
            case MinLoopStage.MorningReview:
                state = new HudState(true, "07:36", "晨间记录", "父母反馈回放", "记录家属对昨夜自动响应的态度变化。", normalAccentColor);
                return true;
            case MinLoopStage.ReturningToTerminal:
                state = new HudState(true, "07:38", "回传完成", "复盘结束", "正在回到 Mia 终端处置界面。", normalAccentColor);
                return true;
            default:
                state = default(HudState);
                return false;
        }
    }

    private string UseOverrideOrFallback(string overrideValue, string fallbackValue)
    {
        return string.IsNullOrEmpty(overrideValue) ? fallbackValue : overrideValue;
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

        flowController.StageChanged.AddListener(ShowStage);
        isListening = true;
    }

    private void Unsubscribe()
    {
        if (!isListening || flowController == null || flowController.StageChanged == null)
        {
            isListening = false;
            return;
        }

        flowController.StageChanged.RemoveListener(ShowStage);
        isListening = false;
    }

    private void EnsureReferences()
    {
        if (HasBoundUI)
        {
            if (canvasGroup == null)
            {
                canvasGroup = hudRoot.GetComponent<CanvasGroup>();
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
        if (hudRoot != null)
        {
            hudRoot.SetActive(visible);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
        }
    }

    private void CreateFallbackUI()
    {
        Canvas canvas = fallbackParentCanvas != null ? fallbackParentCanvas : GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Min Loop Robot HUD Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
        }

        if (hudRoot == null)
        {
            GameObject panelObject = new GameObject("Min Loop Robot HUD Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            panelObject.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.03f, 0.035f);
            panelRect.anchorMax = new Vector2(0.43f, 0.215f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = fallbackPanelColor;

            canvasGroup = panelObject.GetComponent<CanvasGroup>();
            hudRoot = panelObject;
        }

        if (accentImage == null)
        {
            GameObject accentObject = new GameObject("Robot HUD Accent", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            accentObject.transform.SetParent(hudRoot.transform, false);

            RectTransform accentRect = accentObject.GetComponent<RectTransform>();
            accentRect.anchorMin = new Vector2(0f, 0f);
            accentRect.anchorMax = new Vector2(0.018f, 1f);
            accentRect.offsetMin = Vector2.zero;
            accentRect.offsetMax = Vector2.zero;

            accentImage = accentObject.GetComponent<Image>();
            accentImage.color = normalAccentColor;
        }

        if (timeText == null)
        {
            timeText = CreateText(hudRoot.transform, "Robot HUD Time", new Vector2(0.06f, 0.58f), new Vector2(0.28f, 0.88f), 26f, FontStyles.Bold);
        }

        if (heartRateText == null)
        {
            heartRateText = CreateText(hudRoot.transform, "Robot HUD Heart Rate", new Vector2(0.3f, 0.58f), new Vector2(0.58f, 0.88f), 22f, FontStyles.Bold);
        }

        if (statusText == null)
        {
            statusText = CreateText(hudRoot.transform, "Robot HUD Status", new Vector2(0.6f, 0.58f), new Vector2(0.94f, 0.88f), 22f, FontStyles.Bold);
        }

        if (instructionText == null)
        {
            instructionText = CreateText(hudRoot.transform, "Robot HUD Instruction", new Vector2(0.06f, 0.12f), new Vector2(0.94f, 0.54f), 20f, FontStyles.Normal);
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

    private struct HudState
    {
        public readonly bool visible;
        public readonly string timeLabel;
        public readonly string heartRateLabel;
        public readonly string statusLabel;
        public readonly string instructionLabel;
        public readonly Color accentColor;

        public HudState(bool visible, string timeLabel, string heartRateLabel, string statusLabel, string instructionLabel, Color accentColor)
        {
            this.visible = visible;
            this.timeLabel = timeLabel;
            this.heartRateLabel = heartRateLabel;
            this.statusLabel = statusLabel;
            this.instructionLabel = instructionLabel;
            this.accentColor = accentColor;
        }
    }
}
