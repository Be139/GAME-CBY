using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinLoopTrustPresenter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TrustStateController trustStateController;
    [SerializeField] private MinLoopFlowController flowController;
    [SerializeField] private bool findReferencesOnAwake = true;
    [SerializeField] private bool refreshOnEnable = true;

    [Header("Bound UI Optional")]
    [SerializeField] private GameObject trustRoot;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private TMP_Text deltaText;
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private Slider trustSlider;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Fallback UI")]
    [SerializeField] private bool createFallbackUI = true;
    [SerializeField] private Color fallbackPanelColor = new Color(0f, 0f, 0f, 0.58f);

    [Header("Display")]
    [SerializeField] private string label = "住户信任度";
    [SerializeField] private string valueFormat = "{0}";
    [SerializeField] private string deltaFormat = "{0}{1}";
    [SerializeField] private bool showOnlyAfterDisposition;

    private bool listeningTrust;
    private bool listeningDisposition;
    private int currentTrust;
    private int currentDelta;

    private void Awake()
    {
        ResolveReferences();
        EnsureReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();
        Subscribe();

        if (refreshOnEnable)
        {
            Refresh();
        }
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void Refresh()
    {
        ResolveReferences();

        if (trustStateController != null)
        {
            currentTrust = trustStateController.CurrentTrust;
            currentDelta = trustStateController.LastDelta;
        }

        RefreshUI();
    }

    public void SetTrustValue(int trustValue)
    {
        currentTrust = trustValue;
        currentDelta = trustStateController != null ? trustStateController.LastDelta : 0;
        RefreshUI();
    }

    public void SetReferences(TrustStateController trustController, MinLoopFlowController flow)
    {
        Unsubscribe();
        trustStateController = trustController;
        flowController = flow;
        Subscribe();
        Refresh();
    }

    private void OnDispositionApplied(MinLoopDispositionChoice choice, int trustValue, int delta)
    {
        currentTrust = trustValue;
        currentDelta = delta;
        RefreshUI();
    }

    private void RefreshUI()
    {
        EnsureReferences();
        SetVisible(!showOnlyAfterDisposition || currentDelta != 0);

        if (labelText != null)
        {
            labelText.text = label;
        }

        if (valueText != null)
        {
            valueText.text = string.Format(valueFormat, currentTrust);
        }

        if (deltaText != null)
        {
            string prefix = currentDelta > 0 ? "+" : string.Empty;
            deltaText.text = currentDelta == 0 ? string.Empty : string.Format(deltaFormat, prefix, currentDelta);
        }

        if (trustSlider != null)
        {
            trustSlider.minValue = 0f;
            trustSlider.maxValue = 100f;
            trustSlider.SetValueWithoutNotify(Mathf.Clamp(currentTrust, 0, 100));
        }
    }

    private void ResolveReferences()
    {
        if (!findReferencesOnAwake)
        {
            return;
        }

        if (trustStateController == null)
        {
            trustStateController = FindObjectOfType<TrustStateController>();
        }

        if (flowController == null)
        {
            flowController = FindObjectOfType<MinLoopFlowController>();
        }
    }

    private void Subscribe()
    {
        if (!listeningTrust && trustStateController != null && trustStateController.TrustChanged != null)
        {
            trustStateController.TrustChanged.AddListener(SetTrustValue);
            listeningTrust = true;
        }

        if (!listeningDisposition && flowController != null && flowController.DispositionApplied != null)
        {
            flowController.DispositionApplied.AddListener(OnDispositionApplied);
            listeningDisposition = true;
        }
    }

    private void Unsubscribe()
    {
        if (listeningTrust && trustStateController != null && trustStateController.TrustChanged != null)
        {
            trustStateController.TrustChanged.RemoveListener(SetTrustValue);
        }

        if (listeningDisposition && flowController != null && flowController.DispositionApplied != null)
        {
            flowController.DispositionApplied.RemoveListener(OnDispositionApplied);
        }

        listeningTrust = false;
        listeningDisposition = false;
    }

    private void EnsureReferences()
    {
        if (trustRoot != null && valueText != null)
        {
            if (canvasGroup == null)
            {
                canvasGroup = trustRoot.GetComponent<CanvasGroup>();
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
        if (trustRoot != null)
        {
            trustRoot.SetActive(visible);
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
            GameObject canvasObject = new GameObject("Min Loop Trust Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
        }

        if (trustRoot == null)
        {
            GameObject panelObject = new GameObject("Min Loop Trust Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
            panelObject.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.76f, 0.84f);
            panelRect.anchorMax = new Vector2(0.975f, 0.96f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = fallbackPanelColor;

            canvasGroup = panelObject.GetComponent<CanvasGroup>();
            trustRoot = panelObject;
        }

        if (labelText == null)
        {
            labelText = CreateText(trustRoot.transform, "Trust Label", new Vector2(0.06f, 0.58f), new Vector2(0.94f, 0.9f), 20f, FontStyles.Bold);
        }

        if (valueText == null)
        {
            valueText = CreateText(trustRoot.transform, "Trust Value", new Vector2(0.06f, 0.12f), new Vector2(0.58f, 0.58f), 28f, FontStyles.Bold);
        }

        if (deltaText == null)
        {
            deltaText = CreateText(trustRoot.transform, "Trust Delta", new Vector2(0.58f, 0.16f), new Vector2(0.94f, 0.58f), 22f, FontStyles.Bold);
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
}
