using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HearthTerminalBootSequence : MonoBehaviour
{
    [Header("Groups")]
    [SerializeField] private CanvasGroup contentGroup;
    [SerializeField] private CanvasGroup offOverlayGroup;
    [SerializeField] private CanvasGroup bootOverlayGroup;
    [SerializeField] private RectTransform contentRoot;

    [Header("Overlay Images")]
    [SerializeField] private Image offImage;
    [SerializeField] private Image bootFlashImage;
    [SerializeField] private Image scanlineImage;

    [Header("Timing")]
    [SerializeField] private float bootDuration = 0.6f;
    [SerializeField] private float closeFadeDuration = 0.18f;
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Look")]
    [SerializeField] private Color offColor = new Color(0.005f, 0.008f, 0.012f, 0.96f);
    [SerializeField] private Color bootFlashColor = new Color(0.55f, 0.95f, 0.85f, 0.3f);
    [SerializeField] private float maxFlickerAlpha = 0.38f;
    [SerializeField] private float scanlineMaxAlpha = 0.26f;
    [SerializeField] private Vector2 bootScaleRange = new Vector2(1.035f, 1f);
    [SerializeField] private AnimationCurve revealCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Vector3 initialContentScale = Vector3.one;

    public float BootDuration
    {
        get { return bootDuration; }
    }

    public float CloseFadeDuration
    {
        get { return closeFadeDuration; }
    }

    private void Awake()
    {
        ResolveReferences();
        CacheInitialScale();
        ApplyClosedInstant();
    }

    private void OnValidate()
    {
        bootDuration = Mathf.Max(0f, bootDuration);
        closeFadeDuration = Mathf.Max(0f, closeFadeDuration);
        maxFlickerAlpha = Mathf.Max(0f, maxFlickerAlpha);
        scanlineMaxAlpha = Mathf.Max(0f, scanlineMaxAlpha);

        if (bootScaleRange.x <= 0f)
        {
            bootScaleRange.x = 1f;
        }

        if (bootScaleRange.y <= 0f)
        {
            bootScaleRange.y = 1f;
        }

        ApplyColors();
    }

    public void Configure(CanvasGroup newContentGroup, CanvasGroup newOffOverlayGroup, CanvasGroup newBootOverlayGroup, RectTransform newContentRoot)
    {
        contentGroup = newContentGroup;
        offOverlayGroup = newOffOverlayGroup;
        bootOverlayGroup = newBootOverlayGroup;
        contentRoot = newContentRoot;
        ResolveReferences();
        CacheInitialScale();
        ApplyClosedInstant();
    }

    public IEnumerator PlayOpenSequence()
    {
        ResolveReferences();
        CacheInitialScale();
        ApplyColors();

        SetGroup(contentGroup, 0f, false);
        SetGroup(offOverlayGroup, 1f, false);
        SetGroup(bootOverlayGroup, 1f, false);
        SetContentScale(bootScaleRange.x);

        if (bootDuration <= 0f)
        {
            ApplyOpenInstant();
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < bootDuration)
        {
            elapsed += GetDeltaTime();
            float normalized = Mathf.Clamp01(elapsed / bootDuration);
            float reveal = revealCurve != null ? Mathf.Clamp01(revealCurve.Evaluate(normalized)) : normalized;
            float flicker = GetFlicker(normalized, elapsed);

            SetGroup(contentGroup, reveal, false);
            SetGroup(offOverlayGroup, Mathf.Lerp(1f, 0f, reveal), false);
            SetGroup(bootOverlayGroup, Mathf.Clamp01((1f - reveal) * 0.45f + flicker), false);

            if (bootFlashImage != null)
            {
                Color color = bootFlashColor;
                color.a = Mathf.Clamp01(maxFlickerAlpha * flicker);
                bootFlashImage.color = color;
            }

            if (scanlineImage != null)
            {
                Color color = scanlineImage.color;
                color.a = Mathf.Clamp01(scanlineMaxAlpha * (1f - reveal) + flicker * 0.08f);
                scanlineImage.color = color;
            }

            SetContentScale(Mathf.Lerp(bootScaleRange.x, bootScaleRange.y, reveal));
            yield return null;
        }

        ApplyOpenInstant();
    }

    public IEnumerator PlayCloseSequence()
    {
        ResolveReferences();
        ApplyColors();

        float startContentAlpha = contentGroup != null ? contentGroup.alpha : 1f;
        float startOffAlpha = offOverlayGroup != null ? offOverlayGroup.alpha : 0f;
        SetGroup(bootOverlayGroup, 0f, false);

        if (closeFadeDuration <= 0f)
        {
            ApplyClosedInstant();
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < closeFadeDuration)
        {
            elapsed += GetDeltaTime();
            float t = Mathf.Clamp01(elapsed / closeFadeDuration);
            SetGroup(contentGroup, Mathf.Lerp(startContentAlpha, 0f, t), false);
            SetGroup(offOverlayGroup, Mathf.Lerp(startOffAlpha, 1f, t), false);
            yield return null;
        }

        ApplyClosedInstant();
    }

    public void ApplyClosedInstant()
    {
        ResolveReferences();
        ApplyColors();
        SetGroup(contentGroup, 0f, false);
        SetGroup(bootOverlayGroup, 0f, false);
        SetGroup(offOverlayGroup, 1f, false);
        SetContentScale(bootScaleRange.y);
    }

    public void ApplyOpenInstant()
    {
        ResolveReferences();
        ApplyColors();
        SetGroup(contentGroup, 1f, false);
        SetGroup(bootOverlayGroup, 0f, false);
        SetGroup(offOverlayGroup, 0f, false);
        SetContentScale(bootScaleRange.y);
    }

    private void ResolveReferences()
    {
        if (contentRoot == null)
        {
            Transform found = transform.Find("TerminalContentRoot");
            contentRoot = found as RectTransform;
        }

        if (contentGroup == null && contentRoot != null)
        {
            contentGroup = contentRoot.GetComponent<CanvasGroup>();
        }

        if (offOverlayGroup == null)
        {
            Transform found = transform.Find("TerminalOffOverlay");
            offOverlayGroup = found != null ? found.GetComponent<CanvasGroup>() : null;
        }

        if (bootOverlayGroup == null)
        {
            Transform found = transform.Find("TerminalBootOverlay");
            bootOverlayGroup = found != null ? found.GetComponent<CanvasGroup>() : null;
        }

        if (offImage == null && offOverlayGroup != null)
        {
            offImage = offOverlayGroup.GetComponentInChildren<Image>(true);
        }

        if (bootFlashImage == null && bootOverlayGroup != null)
        {
            Transform foundFlash = bootOverlayGroup.transform.Find("BootFlash");
            bootFlashImage = foundFlash != null ? foundFlash.GetComponent<Image>() : bootOverlayGroup.GetComponentInChildren<Image>(true);
        }

        if (scanlineImage == null && bootOverlayGroup != null)
        {
            Transform foundScanlines = bootOverlayGroup.transform.Find("BootScanlines");
            scanlineImage = foundScanlines != null ? foundScanlines.GetComponent<Image>() : null;
        }
    }

    private void CacheInitialScale()
    {
        if (contentRoot != null)
        {
            initialContentScale = contentRoot.localScale;
        }
    }

    private void ApplyColors()
    {
        if (offImage != null)
        {
            offImage.color = offColor;
        }

        if (bootFlashImage != null)
        {
            Color color = bootFlashColor;
            color.a = Mathf.Min(color.a, maxFlickerAlpha);
            bootFlashImage.color = color;
        }
    }

    private float GetFlicker(float normalized, float elapsed)
    {
        float envelope = 1f - normalized;
        float pulse = Mathf.Abs(Mathf.Sin(elapsed * 87f));
        float noise = Mathf.PerlinNoise(elapsed * 37f, 0.37f);
        return Mathf.Clamp01((pulse * 0.55f + noise * 0.45f) * envelope);
    }

    private float GetDeltaTime()
    {
        return useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    }

    private void SetContentScale(float multiplier)
    {
        if (contentRoot == null)
        {
            return;
        }

        contentRoot.localScale = initialContentScale * Mathf.Max(0.01f, multiplier);
    }

    private static void SetGroup(CanvasGroup group, float alpha, bool blocksRaycasts)
    {
        if (group == null)
        {
            return;
        }

        group.alpha = Mathf.Clamp01(alpha);
        group.interactable = blocksRaycasts;
        group.blocksRaycasts = blocksRaycasts;
    }
}
