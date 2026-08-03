using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds the 17F04 photo archive chrome as a world-space child of TV4.
/// The physical photo renderer remains the image source; this component only
/// owns the title, page state, return hint and embedded dialogue surface.
/// </summary>
[DisallowMultipleComponent]
public sealed class HearthPhotoArchiveWorldView : MonoBehaviour
{
    private static readonly Color Cyan = new Color32(116, 190, 232, 255);
    private static readonly Color Warm = new Color32(235, 176, 79, 255);
    private static readonly Color White = new Color32(223, 235, 242, 255);
    private static readonly Color DarkPanel = new Color32(8, 12, 24, 226);

    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private CanvasGroup rootGroup;
    [SerializeField] private TMP_Text pageText;
    [SerializeField] private TMP_Text hintText;
    [SerializeField] private HearthDialogueSurface dialogueSurface;
    [SerializeField] private HearthUiThemeProfile uiThemeProfile;
    [SerializeField] private HearthUiLayoutProfile uiLayoutProfile;

    private Camera activeCamera;
    private Renderer activePhotoRenderer;
    private RectTransform canvasRect;

    public bool IsVisible
    {
        get { return rootGroup != null && rootGroup.alpha > 0.001f; }
    }

    public HearthDialogueSurface ResolveDialogueSurface()
    {
        EnsureBuilt();
        ApplyProfileToBuiltUi();
        return dialogueSurface;
    }

    public void SetUiProfiles(
        HearthUiThemeProfile themeProfile,
        HearthUiLayoutProfile layoutProfile)
    {
        uiThemeProfile = themeProfile;
        uiLayoutProfile = layoutProfile;
        ApplyProfileToBuiltUi();
    }

    public void Show(
        Camera photoCamera,
        Renderer photoRenderer,
        int pageIndex,
        int pageCount)
    {
        activeCamera = photoCamera;
        activePhotoRenderer = photoRenderer;
        EnsureBuilt();
        UpdatePose();
        SetPage(pageIndex, pageCount);
        SetHint(string.Empty, false);

        if (worldCanvas != null)
        {
            worldCanvas.worldCamera = activeCamera;
            worldCanvas.gameObject.SetActive(true);
        }

        if (rootGroup != null)
        {
            rootGroup.alpha = 1f;
            rootGroup.interactable = false;
            rootGroup.blocksRaycasts = false;
        }
    }

    public void Hide()
    {
        if (dialogueSurface != null)
        {
            dialogueSurface.HideImmediate();
        }

        if (rootGroup != null)
        {
            rootGroup.alpha = 0f;
            rootGroup.interactable = false;
            rootGroup.blocksRaycasts = false;
        }

        if (worldCanvas != null)
        {
            worldCanvas.gameObject.SetActive(false);
        }
    }

    public void SetPage(int pageIndex, int pageCount)
    {
        EnsureBuilt();
        if (pageText != null)
        {
            pageText.text = string.Format(
                "PAGE {0:00} / {1:00}",
                Mathf.Clamp(pageIndex + 1, 1, Mathf.Max(1, pageCount)),
                Mathf.Max(1, pageCount));
        }
    }

    public void SetHint(string label, bool visible)
    {
        EnsureBuilt();
        if (hintText == null)
        {
            return;
        }

        hintText.text = visible ? label : string.Empty;
        hintText.gameObject.SetActive(visible);
    }

    private void Awake()
    {
        EnsureBuilt();
        Hide();
    }

    private void EnsureBuilt()
    {
        if (worldCanvas != null && dialogueSurface != null)
        {
            ApplyProfileToBuiltUi();
            return;
        }

        Transform existing = transform.Find("PhotoArchiveCanvas_V2");
        GameObject canvasObject = existing != null
            ? existing.gameObject
            : new GameObject(
                "PhotoArchiveCanvas_V2",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster),
                typeof(CanvasGroup));
        if (existing == null)
        {
            canvasObject.transform.SetParent(transform, false);
        }

        worldCanvas = canvasObject.GetComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        worldCanvas.overrideSorting = true;
        worldCanvas.sortingOrder = 80;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.dynamicPixelsPerUnit = 12f;

        GraphicRaycaster raycaster = canvasObject.GetComponent<GraphicRaycaster>();
        if (raycaster != null)
        {
            raycaster.enabled = false;
        }

        rootGroup = canvasObject.GetComponent<CanvasGroup>();
        canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = ResolveReferenceResolution();

        TMP_Text title = CreateText(
            canvasRect,
            "ArchiveTitle",
            new Vector2(0.07f, 0.88f),
            new Vector2(0.42f, 0.97f),
            "PHOTO ARCHIVE",
            42f,
            White,
            TextAlignmentOptions.MidlineLeft,
            FontStyles.Bold);
        title.characterSpacing = 1.5f;

        TMP_Text unit = CreateText(
            canvasRect,
            "ArchiveUnit",
            new Vector2(0.07f, 0.835f),
            new Vector2(0.44f, 0.885f),
            "HOME UNIT 17F-04",
            20f,
            Cyan,
            TextAlignmentOptions.MidlineLeft,
            FontStyles.Normal);
        unit.characterSpacing = 1.2f;
        unit.gameObject.SetActive(false);

        pageText = CreateText(
            canvasRect,
            "ArchivePage",
            new Vector2(0.07f, 0.06f),
            new Vector2(0.30f, 0.12f),
            "PAGE 01 / 01",
            22f,
            White,
            TextAlignmentOptions.MidlineLeft,
            FontStyles.Bold);

        hintText = CreateText(
            canvasRect,
            "ArchiveReturnHint",
            new Vector2(0.68f, 0.055f),
            new Vector2(0.93f, 0.125f),
            "SPACE  RETURN",
            22f,
            White,
            TextAlignmentOptions.MidlineRight,
            FontStyles.Bold);

        CreateRule(canvasRect, "ArchiveTopRule", 0.07f, 0.93f, 0.825f, Cyan);
        CreateRule(canvasRect, "ArchiveBottomRule", 0.07f, 0.93f, 0.145f, Cyan);

        GameObject panel = CreatePanel(
            canvasRect,
            "FieldUnitPanel",
            new Vector2(0.12f, 0.16f),
            new Vector2(0.88f, 0.37f));
        CanvasGroup panelGroup = panel.GetComponent<CanvasGroup>();
        RectTransform panelRect = panel.GetComponent<RectTransform>();

        TMP_Text speaker = CreateText(
            panelRect,
            "Speaker",
            new Vector2(0.035f, 0.66f),
            new Vector2(0.55f, 0.93f),
            "Field Unit",
            28f,
            Warm,
            TextAlignmentOptions.MidlineLeft,
            FontStyles.Bold);
        TMP_Text body = CreateText(
            panelRect,
            "Body",
            new Vector2(0.035f, 0.18f),
            new Vector2(0.965f, 0.68f),
            string.Empty,
            25f,
            White,
            TextAlignmentOptions.TopLeft,
            FontStyles.Normal);
        body.enableWordWrapping = true;
        body.overflowMode = TextOverflowModes.Truncate;

        TMP_Text advance = CreateText(
            panelRect,
            "AdvanceHint",
            new Vector2(0.68f, 0.025f),
            new Vector2(0.965f, 0.22f),
            "SPACE  CONTINUE",
            17f,
            Cyan,
            TextAlignmentOptions.MidlineRight,
            FontStyles.Bold);

        dialogueSurface = panel.GetComponent<HearthDialogueSurface>();
        if (dialogueSurface == null)
        {
            dialogueSurface = panel.AddComponent<HearthDialogueSurface>();
        }
        dialogueSurface.Configure(panelGroup, speaker, body, advance);
        ApplyProfileToBuiltUi();
    }

    private void UpdatePose()
    {
        if (canvasRect == null || activeCamera == null)
        {
            return;
        }

        Vector3 focus = activePhotoRenderer != null
            ? activePhotoRenderer.bounds.center
            : transform.position;
        float distance = Mathf.Max(
            0.35f,
            Vector3.Distance(activeCamera.transform.position, focus));
        float verticalWorldSize =
            2f * distance * Mathf.Tan(activeCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float desiredHeight = verticalWorldSize * 0.94f;
        Vector2 referenceResolution = ResolveReferenceResolution();
        canvasRect.sizeDelta = referenceResolution;
        float desiredWorldScale = desiredHeight / referenceResolution.y;
        Vector3 parentScale = canvasRect.parent != null
            ? canvasRect.parent.lossyScale
            : Vector3.one;
        canvasRect.localScale = new Vector3(
            DivideByScale(desiredWorldScale, parentScale.x),
            DivideByScale(desiredWorldScale, parentScale.y),
            DivideByScale(desiredWorldScale, parentScale.z));
        canvasRect.position = focus - activeCamera.transform.forward * 0.012f;
        canvasRect.rotation = activeCamera.transform.rotation;
    }

    private void ApplyProfileToBuiltUi()
    {
        if (canvasRect == null && worldCanvas != null)
        {
            canvasRect = worldCanvas.GetComponent<RectTransform>();
        }
        if (canvasRect == null)
        {
            return;
        }

        Transform unit = canvasRect.Find("ArchiveUnit");
        if (unit != null)
        {
            unit.gameObject.SetActive(false);
        }
        Transform topRule = canvasRect.Find("ArchiveTopRule");
        if (topRule != null)
        {
            topRule.gameObject.SetActive(false);
        }

        if (pageText == null)
        {
            Transform page = canvasRect.Find("ArchivePage");
            pageText = page != null ? page.GetComponent<TMP_Text>() : null;
        }
        if (uiLayoutProfile != null)
        {
            HearthUiReferenceRect pageRect =
                uiLayoutProfile.GetRegion(HearthUiLayoutRegion.PhotoArchivePage);
            pageRect.ApplyTopLeftAnchors(
                pageText != null ? pageText.rectTransform : null);

            HearthUiReferenceRect dialogueRect =
                uiLayoutProfile.GetRegion(HearthUiLayoutRegion.PhotoArchiveFieldUnit);
            dialogueRect.ApplyTopLeftAnchors(
                dialogueSurface != null
                    ? dialogueSurface.transform as RectTransform
                    : null);
        }

        if (dialogueSurface != null)
        {
            dialogueSurface.ApplyTypography(
                uiThemeProfile != null
                    ? uiThemeProfile.TerminalDialogueSpeakerFontSize
                    : 52f,
                uiThemeProfile != null
                    ? uiThemeProfile.TerminalDialogueBodyFontSize
                    : 26f,
                uiThemeProfile != null
                    ? uiThemeProfile.TerminalDialogueAdvanceFontSize
                    : 26f);
            dialogueSurface.ApplyTerminalInternalLayout();
        }
    }

    private Vector2 ResolveReferenceResolution()
    {
        return uiLayoutProfile != null
            ? uiLayoutProfile.ReferenceResolution
            : new Vector2(1920f, 1080f);
    }

    private static float DivideByScale(float value, float scale)
    {
        float magnitude = Mathf.Abs(scale);
        return magnitude > 0.0001f ? value / magnitude : value;
    }

    private static GameObject CreatePanel(
        RectTransform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax)
    {
        GameObject panel = new GameObject(
            name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Outline),
            typeof(CanvasGroup));
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = panel.GetComponent<Image>();
        image.color = DarkPanel;
        image.raycastTarget = false;

        Outline outline = panel.GetComponent<Outline>();
        outline.effectColor = Cyan;
        outline.effectDistance = new Vector2(2f, -2f);
        outline.useGraphicAlpha = true;
        return panel;
    }

    private static TMP_Text CreateText(
        RectTransform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        string value,
        float fontSize,
        Color color,
        TextAlignmentOptions alignment,
        FontStyles style)
    {
        GameObject target = new GameObject(
            name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        target.transform.SetParent(parent, false);
        RectTransform rect = target.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        TextMeshProUGUI text = target.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.fontStyle = style;
        text.raycastTarget = false;
        return text;
    }

    private static void CreateRule(
        RectTransform parent,
        string name,
        float anchorMinX,
        float anchorMaxX,
        float anchorY,
        Color color)
    {
        GameObject line = new GameObject(
            name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        line.transform.SetParent(parent, false);
        RectTransform rect = line.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(anchorMinX, anchorY);
        rect.anchorMax = new Vector2(anchorMaxX, anchorY);
        rect.sizeDelta = new Vector2(0f, 2f);

        Image image = line.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
    }
}
