using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MinLoopTerminalPresenter : MonoBehaviour
{
    [Header("Terminal Shell")]
    [SerializeField] private TerminalUIController terminalUI;
    [SerializeField] private bool findTerminalUIOnAwake = true;
    [SerializeField] private bool createFallbackUI = true;

    [Header("Input Focus")]
    [SerializeField] private bool createEventSystemIfMissing = true;
    [SerializeField] private bool disableGameplayBehavioursWhenOpen = true;
    [SerializeField] private Behaviour[] gameplayBehavioursToDisable;

    [Header("Default Copy")]
    [SerializeField] private string accessTitle = "17F-01 门口终端";
    [SerializeField] private string accessBody = "请刷工牌以读取本户陪伴单元昨夜事件摘要。";
    [SerializeField] private string summaryTitle = "住户摘要 / 17F-01";
    [TextArea(4, 8)]
    [SerializeField] private string summaryBody =
        "儿童：男孩，夜间噩梦醒来。\n" +
        "设备：陪伴单元已在 02:47 自动介入。\n" +
        "记录：孩子醒来后呼唤“妈妈”，随后接受系统安抚。\n" +
        "检查原因：系统判定家庭回应模式存在长期替代风险。";
    [SerializeField] private string dispositionTitle = "处置建议";
    [TextArea(2, 4)]
    [SerializeField] private string dispositionBody = "昨夜事件复盘结束。请选择本户后续处置。";
    [SerializeField] private string optionALabel = "A 系统推荐：继续自动响应";
    [SerializeField] private string optionBLabel = "B 低介入观察：提示家属亲自回应";
    [TextArea(2, 4)]
    [SerializeField] private string nextResidentBody = "本户记录已提交。\n下一户检查目标：17F-02。";

    [Header("Fallback UI")]
    [SerializeField] private Color fallbackPanelColor = new Color(0.04f, 0.05f, 0.06f, 0.96f);

    [Header("Bound UI Optional")]
    [SerializeField] private bool useBoundUIWhenAssigned = true;
    [SerializeField] private GameObject boundUIRoot;
    [SerializeField] private TMP_Text boundTitleText;
    [SerializeField] private TMP_Text boundBodyText;
    [SerializeField] private Button boundPrimaryButton;
    [SerializeField] private TMP_Text boundPrimaryButtonText;
    [SerializeField] private Button boundSecondaryButton;
    [SerializeField] private TMP_Text boundSecondaryButtonText;
    [SerializeField] private Button boundCloseButton;
    [SerializeField] private TMP_Text boundCloseButtonText;

    private Transform contentRoot;
    private GameObject generatedRoot;
    private GameObject fallbackPanel;
    private Transform fallbackContentRoot;
    private Button fallbackCloseButton;
    private bool usingFallbackContentRoot;
    private UnityAction currentPrimaryAction;
    private UnityAction currentSecondaryAction;
    private UnityAction currentCloseAction;
    private CursorLockMode previousCursorLockState;
    private bool previousCursorVisible;
    private bool presenterOwnsCursor;
    private bool presenterOwnsGameplayDisable;
    private bool[] previousGameplayBehaviourStates;

    public bool HasTerminalUIController
    {
        get { return terminalUI != null; }
    }

    public bool HasSingleButtonBoundUI
    {
        get { return CanUseBoundUI(false); }
    }

    public bool HasChoiceBoundUI
    {
        get { return CanUseBoundUI(true); }
    }

    public bool HasAnyBoundUIAssigned
    {
        get
        {
            return boundUIRoot != null ||
                   boundTitleText != null ||
                   boundBodyText != null ||
                   boundPrimaryButton != null ||
                   boundSecondaryButton != null ||
                   boundCloseButton != null;
        }
    }

    public bool CanCreateFallbackUI
    {
        get { return createFallbackUI; }
    }

    public bool CanCreateEventSystemIfMissing
    {
        get { return createEventSystemIfMissing; }
    }

    public bool HasGameplayBehavioursToDisable
    {
        get { return gameplayBehavioursToDisable != null && gameplayBehavioursToDisable.Length > 0; }
    }

    private void Awake()
    {
        if (terminalUI == null && findTerminalUIOnAwake)
        {
            terminalUI = FindObjectOfType<TerminalUIController>();
        }
    }

    private void OnDestroy()
    {
        RemoveBoundButtonListeners();

        if (fallbackCloseButton != null)
        {
            fallbackCloseButton.onClick.RemoveListener(Close);
        }
    }

    public void ShowAccessCard(UnityAction confirmAction)
    {
        if (TryShowBoundPage(accessTitle, accessBody, "刷工牌", confirmAction, null, null))
        {
            return;
        }

        Transform root = PrepareContentRoot();
        if (root == null)
        {
            return;
        }

        CreateTitle(root, accessTitle);
        CreateBody(root, accessBody);
        CreateButton(root, "刷工牌", confirmAction);
    }

    public void ShowResidentSummary(UnityAction replayAction)
    {
        if (TryShowBoundPage(summaryTitle, summaryBody, "调出昨夜事件", replayAction, null, null))
        {
            return;
        }

        Transform root = PrepareContentRoot();
        if (root == null)
        {
            return;
        }

        CreateTitle(root, summaryTitle);
        CreateBody(root, summaryBody);
        CreateButton(root, "调出昨夜事件", replayAction);
    }

    public void ShowDispositionChoices(UnityAction chooseAAction, UnityAction chooseBAction)
    {
        if (TryShowBoundPage(dispositionTitle, dispositionBody, optionALabel, chooseAAction, optionBLabel, chooseBAction))
        {
            return;
        }

        Transform root = PrepareContentRoot();
        if (root == null)
        {
            return;
        }

        CreateTitle(root, dispositionTitle);
        CreateBody(root, dispositionBody);
        CreateButton(root, optionALabel, chooseAAction);
        CreateButton(root, optionBLabel, chooseBAction);
    }

    public void ShowDispositionResult(MinLoopDispositionChoice choice, int trustValue, int delta, UnityAction continueAction)
    {
        string choiceText = choice == MinLoopDispositionChoice.SystemRecommendedA ? optionALabel : optionBLabel;
        string deltaPrefix = delta >= 0 ? "+" : string.Empty;
        string body = choiceText + "\n信任度变化：" + deltaPrefix + delta + "\n当前信任度：" + trustValue;

        if (TryShowBoundPage("处置已提交", body, "查看下一户指引", continueAction, null, null))
        {
            return;
        }

        Transform root = PrepareContentRoot();
        if (root == null)
        {
            return;
        }

        CreateTitle(root, "处置已提交");
        CreateBody(root, body);
        CreateButton(root, "查看下一户指引", continueAction);
    }

    public void ShowNextResident(UnityAction closeAction)
    {
        if (TryShowBoundPage("下一户指引", nextResidentBody, "关闭终端", closeAction, null, null))
        {
            return;
        }

        Transform root = PrepareContentRoot();
        if (root == null)
        {
            return;
        }

        CreateTitle(root, "下一户指引");
        CreateBody(root, nextResidentBody);
        CreateButton(root, "关闭终端", closeAction);
    }

    public void Close()
    {
        RemoveBoundButtonListeners();
        ClearGeneratedContent();

        if (boundUIRoot != null)
        {
            boundUIRoot.SetActive(false);
        }

        if (fallbackPanel != null)
        {
            fallbackPanel.SetActive(false);
        }

        if (terminalUI != null)
        {
            terminalUI.Close();
        }

        RestoreGameplayBehavioursIfOwned();
        RestoreCursorIfOwned();
    }

    private bool TryShowBoundPage(string title, string body, string primaryLabel, UnityAction primaryAction, string secondaryLabel, UnityAction secondaryAction)
    {
        bool needsSecondary = !string.IsNullOrEmpty(secondaryLabel) || secondaryAction != null;
        if (!CanUseBoundUI(needsSecondary))
        {
            return false;
        }

        EnterTerminalFocus();

        if (boundUIRoot != null)
        {
            boundUIRoot.SetActive(true);
        }

        if (fallbackPanel != null)
        {
            fallbackPanel.SetActive(false);
        }

        ClearGeneratedContent();

        boundTitleText.text = title;
        boundBodyText.text = body;

        ConfigureButton(boundPrimaryButton, boundPrimaryButtonText, primaryLabel, primaryAction, ref currentPrimaryAction, true);
        ConfigureButton(boundSecondaryButton, boundSecondaryButtonText, secondaryLabel, secondaryAction, ref currentSecondaryAction, needsSecondary);
        ConfigureButton(boundCloseButton, boundCloseButtonText, null, Close, ref currentCloseAction, boundCloseButton != null);

        return true;
    }

    private bool CanUseBoundUI(bool needsSecondary)
    {
        if (!useBoundUIWhenAssigned)
        {
            return false;
        }

        if (boundTitleText == null || boundBodyText == null || boundPrimaryButton == null)
        {
            return false;
        }

        if (needsSecondary && boundSecondaryButton == null)
        {
            return false;
        }

        return true;
    }

    private void ConfigureButton(Button button, TMP_Text explicitLabel, string label, UnityAction action, ref UnityAction storedAction, bool isVisible)
    {
        if (button == null)
        {
            storedAction = null;
            return;
        }

        if (storedAction != null)
        {
            button.onClick.RemoveListener(storedAction);
            storedAction = null;
        }

        button.gameObject.SetActive(isVisible);

        if (!isVisible)
        {
            return;
        }

        TMP_Text labelText = explicitLabel != null ? explicitLabel : button.GetComponentInChildren<TMP_Text>(true);
        if (labelText != null && label != null)
        {
            labelText.text = label;
        }

        if (action != null)
        {
            button.onClick.AddListener(action);
            storedAction = action;
        }
    }

    private void RemoveBoundButtonListeners()
    {
        RemoveButtonListener(boundPrimaryButton, ref currentPrimaryAction);
        RemoveButtonListener(boundSecondaryButton, ref currentSecondaryAction);
        RemoveButtonListener(boundCloseButton, ref currentCloseAction);
    }

    private void RemoveButtonListener(Button button, ref UnityAction storedAction)
    {
        if (button != null && storedAction != null)
        {
            button.onClick.RemoveListener(storedAction);
        }

        storedAction = null;
    }

    private Transform PrepareContentRoot()
    {
        RemoveBoundButtonListeners();

        if (boundUIRoot != null)
        {
            boundUIRoot.SetActive(false);
        }

        EnsureContentRoot();
        if (contentRoot == null)
        {
            return null;
        }

        ClearGeneratedContent();

        generatedRoot = new GameObject("Min Loop Terminal Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
        generatedRoot.transform.SetParent(contentRoot, false);

        RectTransform rootRect = generatedRoot.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup layout = generatedRoot.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(12, 12, 12, 12);
        layout.spacing = 14f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        return generatedRoot.transform;
    }

    private void EnsureContentRoot()
    {
        usingFallbackContentRoot = false;

        if (terminalUI != null)
        {
            contentRoot = terminalUI.ContentRoot;
        }

        EnterTerminalFocus();

        if (contentRoot == null && createFallbackUI)
        {
            CreateFallbackUI();
            contentRoot = fallbackContentRoot;
            usingFallbackContentRoot = true;
        }

        if (fallbackPanel != null && (terminalUI == null || usingFallbackContentRoot))
        {
            fallbackPanel.SetActive(true);
        }

        if (contentRoot == null)
        {
            Debug.LogWarning("MinLoopTerminalPresenter has no content root. Assign TerminalUIController.ContentRoot or enable Create Fallback UI.", this);
        }
    }

    private void UnlockCursorForPresenter()
    {
        if (presenterOwnsCursor)
        {
            return;
        }

        previousCursorLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        presenterOwnsCursor = true;
    }

    private void EnterTerminalFocus()
    {
        EnsureEventSystem();

        if (terminalUI != null)
        {
            terminalUI.Open();
        }
        else
        {
            UnlockCursorForPresenter();
            DisableGameplayBehavioursIfNeeded();
        }
    }

    private void EnsureEventSystem()
    {
        if (!createEventSystemIfMissing || EventSystem.current != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        eventSystemObject.transform.SetParent(transform, false);
    }

    private void DisableGameplayBehavioursIfNeeded()
    {
        if (!disableGameplayBehavioursWhenOpen || presenterOwnsGameplayDisable || gameplayBehavioursToDisable == null)
        {
            return;
        }

        previousGameplayBehaviourStates = new bool[gameplayBehavioursToDisable.Length];
        for (int i = 0; i < gameplayBehavioursToDisable.Length; i++)
        {
            Behaviour behaviour = gameplayBehavioursToDisable[i];
            if (behaviour == null || behaviour == this)
            {
                continue;
            }

            previousGameplayBehaviourStates[i] = behaviour.enabled;
            behaviour.enabled = false;
        }

        presenterOwnsGameplayDisable = true;
    }

    private void RestoreGameplayBehavioursIfOwned()
    {
        if (!presenterOwnsGameplayDisable)
        {
            return;
        }

        if (gameplayBehavioursToDisable != null && previousGameplayBehaviourStates != null)
        {
            int count = Mathf.Min(gameplayBehavioursToDisable.Length, previousGameplayBehaviourStates.Length);
            for (int i = 0; i < count; i++)
            {
                Behaviour behaviour = gameplayBehavioursToDisable[i];
                if (behaviour != null && behaviour != this)
                {
                    behaviour.enabled = previousGameplayBehaviourStates[i];
                }
            }
        }

        previousGameplayBehaviourStates = null;
        presenterOwnsGameplayDisable = false;
    }

    private void RestoreCursorIfOwned()
    {
        if (!presenterOwnsCursor)
        {
            return;
        }

        Cursor.lockState = previousCursorLockState;
        Cursor.visible = previousCursorVisible;
        presenterOwnsCursor = false;
    }

    private void ClearGeneratedContent()
    {
        if (generatedRoot == null)
        {
            return;
        }

        Destroy(generatedRoot);
        generatedRoot = null;
    }

    private TMP_Text CreateTitle(Transform parent, string text)
    {
        TMP_Text title = CreateText(parent, "Title", 30f, FontStyles.Bold);
        title.text = text;
        AddLayout(title.gameObject, 58f);
        return title;
    }

    private TMP_Text CreateBody(Transform parent, string text)
    {
        TMP_Text body = CreateText(parent, "Body", 22f, FontStyles.Normal);
        body.text = text;
        AddLayout(body.gameObject, 230f);
        return body;
    }

    private Button CreateButton(Transform parent, string label, UnityAction action)
    {
        GameObject buttonObject = new GameObject("Button - " + label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(0f, 54f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.12f, 0.34f, 0.42f, 0.96f);

        Button button = buttonObject.GetComponent<Button>();
        if (action != null)
        {
            button.onClick.AddListener(action);
        }

        LayoutElement layout = buttonObject.GetComponent<LayoutElement>();
        layout.minHeight = 54f;
        layout.preferredHeight = 54f;

        TMP_Text buttonText = CreateText(buttonObject.transform, "Label", 20f, FontStyles.Bold);
        buttonText.text = label;
        buttonText.alignment = TextAlignmentOptions.Center;

        RectTransform textRect = buttonText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(12f, 4f);
        textRect.offsetMax = new Vector2(-12f, -4f);

        return button;
    }

    private TMP_Text CreateText(Transform parent, string objectName, float fontSize, FontStyles style)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
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

    private void AddLayout(GameObject target, float preferredHeight)
    {
        LayoutElement layout = target.GetComponent<LayoutElement>();
        if (layout == null)
        {
            layout = target.AddComponent<LayoutElement>();
        }

        layout.minHeight = preferredHeight;
        layout.preferredHeight = preferredHeight;
    }

    private void CreateFallbackUI()
    {
        if (fallbackPanel != null && fallbackContentRoot != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("Min Loop Terminal Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        fallbackPanel = new GameObject("Min Loop Terminal Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        fallbackPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = fallbackPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(920f, 620f);
        panelRect.anchoredPosition = Vector2.zero;

        Image image = fallbackPanel.GetComponent<Image>();
        image.color = fallbackPanelColor;

        GameObject contentObject = new GameObject("Content Root", typeof(RectTransform));
        contentObject.transform.SetParent(fallbackPanel.transform, false);

        RectTransform contentRect = contentObject.GetComponent<RectTransform>();
        contentRect.anchorMin = Vector2.zero;
        contentRect.anchorMax = Vector2.one;
        contentRect.offsetMin = new Vector2(36f, 36f);
        contentRect.offsetMax = new Vector2(-36f, -80f);

        fallbackContentRoot = contentObject.transform;
        CreateFallbackCloseButton();
        fallbackPanel.SetActive(false);
    }

    private void CreateFallbackCloseButton()
    {
        if (fallbackPanel == null || fallbackCloseButton != null)
        {
            return;
        }

        GameObject buttonObject = new GameObject("Close Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(fallbackPanel.transform, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(1f, 1f);
        buttonRect.anchorMax = new Vector2(1f, 1f);
        buttonRect.pivot = new Vector2(1f, 1f);
        buttonRect.sizeDelta = new Vector2(44f, 44f);
        buttonRect.anchoredPosition = new Vector2(-16f, -16f);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(0f, 0f, 0f, 0.35f);

        fallbackCloseButton = buttonObject.GetComponent<Button>();
        fallbackCloseButton.onClick.AddListener(Close);
        CreateFallbackCloseButtonLine(buttonRect, 45f);
        CreateFallbackCloseButtonLine(buttonRect, -45f);
    }

    private void CreateFallbackCloseButtonLine(RectTransform parent, float angle)
    {
        GameObject lineObject = new GameObject("Close Line", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        lineObject.transform.SetParent(parent, false);

        RectTransform lineRect = lineObject.GetComponent<RectTransform>();
        lineRect.anchorMin = new Vector2(0.5f, 0.5f);
        lineRect.anchorMax = new Vector2(0.5f, 0.5f);
        lineRect.pivot = new Vector2(0.5f, 0.5f);
        lineRect.sizeDelta = new Vector2(22f, 2f);
        lineRect.anchoredPosition = Vector2.zero;
        lineRect.localRotation = Quaternion.Euler(0f, 0f, angle);

        Image lineImage = lineObject.GetComponent<Image>();
        lineImage.color = Color.white;
    }
}
