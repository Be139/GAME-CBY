using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HearthTerminalSelectionHighlighter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform highlightRect;
    [SerializeField] private Image highlightFill;
    [SerializeField] private RectTransform navigationBounds;
    [SerializeField] private RectTransform[] targets;

    [Header("Style")]
    [SerializeField] private Vector2 padding = new Vector2(8f, 5f);
    [SerializeField] private Color fillColor = new Color(0.1f, 0.95f, 0.58f, 0.12f);
    [SerializeField] private Color borderColor = new Color(0.38f, 1f, 0.72f, 0.42f);
    [SerializeField] private Image[] borderImages;

    [Header("Motion")]
    [SerializeField] private bool animateMovement = true;
    [SerializeField] private float followSpeed = 18f;
    [SerializeField] private bool clampToNavigationBounds = true;

    private int currentIndex = -1;
    private Vector2 targetAnchoredPosition;
    private Vector2 targetSizeDelta;

    private void Awake()
    {
        ResolveReferences();
        ApplyStyle();
        SetVisible(false);
    }

    private void OnValidate()
    {
        followSpeed = Mathf.Max(0f, followSpeed);
        ApplyStyle();
    }

    private void Update()
    {
        if (!animateMovement || highlightRect == null || currentIndex < 0)
        {
            return;
        }

        float t = 1f - Mathf.Exp(-followSpeed * Time.unscaledDeltaTime);
        highlightRect.anchoredPosition = Vector2.Lerp(highlightRect.anchoredPosition, targetAnchoredPosition, t);
        highlightRect.sizeDelta = Vector2.Lerp(highlightRect.sizeDelta, targetSizeDelta, t);
    }

    public void Configure(RectTransform newHighlightRect, Image newHighlightFill, RectTransform newNavigationBounds, RectTransform[] newTargets)
    {
        highlightRect = newHighlightRect;
        highlightFill = newHighlightFill;
        navigationBounds = newNavigationBounds;
        targets = newTargets;
        ResolveReferences();
        ApplyStyle();
        SetFocus(0, true);
        SetVisible(false);
    }

    public void SetTargets(RectTransform[] newTargets)
    {
        targets = newTargets;
        SetFocus(currentIndex, true);
    }

    public void SetFocus(int index, bool instant = false)
    {
        ResolveReferences();

        if (targets == null || targets.Length == 0 || highlightRect == null)
        {
            currentIndex = -1;
            SetVisible(false);
            return;
        }

        currentIndex = Wrap(index, targets.Length);
        RectTransform target = targets[currentIndex];
        if (target == null)
        {
            SetVisible(false);
            return;
        }

        CalculateTargetRect(target, out targetAnchoredPosition, out targetSizeDelta);
        SetVisible(true);

        if (instant || !animateMovement)
        {
            highlightRect.anchoredPosition = targetAnchoredPosition;
            highlightRect.sizeDelta = targetSizeDelta;
        }
    }

    public void SetVisible(bool visible)
    {
        if (highlightRect != null)
        {
            highlightRect.gameObject.SetActive(visible);
        }
    }

    public int TargetCount
    {
        get { return targets != null ? targets.Length : 0; }
    }

    private void ResolveReferences()
    {
        if (highlightRect == null)
        {
            Transform found = transform.Find("KeyboardSelectionHighlight");
            highlightRect = found as RectTransform;
        }

        if (highlightFill == null && highlightRect != null)
        {
            highlightFill = highlightRect.GetComponent<Image>();
        }

        if (navigationBounds == null)
        {
            Transform foundBounds = transform.Find("KeyboardSelectionBounds");
            navigationBounds = foundBounds as RectTransform;
        }

        if ((targets == null || targets.Length == 0) && transform.Find("SelectionTargets") != null)
        {
            Transform targetRoot = transform.Find("SelectionTargets");
            RectTransform[] foundTargets = targetRoot.GetComponentsInChildren<RectTransform>(true);
            int count = Mathf.Max(0, foundTargets.Length - 1);
            targets = new RectTransform[count];
            int writeIndex = 0;
            for (int i = 0; i < foundTargets.Length; i++)
            {
                if (foundTargets[i] != targetRoot)
                {
                    targets[writeIndex] = foundTargets[i];
                    writeIndex++;
                }
            }
        }
    }

    private void ApplyStyle()
    {
        if (highlightFill != null)
        {
            highlightFill.color = fillColor;
            highlightFill.raycastTarget = false;
        }

        if (borderImages == null || borderImages.Length == 0)
        {
            return;
        }

        for (int i = 0; i < borderImages.Length; i++)
        {
            if (borderImages[i] != null)
            {
                borderImages[i].color = borderColor;
                borderImages[i].raycastTarget = false;
            }
        }
    }

    private void CalculateTargetRect(RectTransform target, out Vector2 anchoredPosition, out Vector2 sizeDelta)
    {
        anchoredPosition = target.anchoredPosition + new Vector2(-padding.x, padding.y);
        sizeDelta = target.rect.size + padding * 2f;

        if (!clampToNavigationBounds || navigationBounds == null)
        {
            return;
        }

        float minX = navigationBounds.anchoredPosition.x;
        float maxX = navigationBounds.anchoredPosition.x + navigationBounds.rect.width - sizeDelta.x;
        float topY = navigationBounds.anchoredPosition.y;
        float bottomY = navigationBounds.anchoredPosition.y - navigationBounds.rect.height + sizeDelta.y;

        if (maxX >= minX)
        {
            anchoredPosition.x = Mathf.Clamp(anchoredPosition.x, minX, maxX);
        }

        if (topY >= bottomY)
        {
            anchoredPosition.y = Mathf.Clamp(anchoredPosition.y, bottomY, topY);
        }
    }

    private static int Wrap(int value, int count)
    {
        if (count <= 0)
        {
            return 0;
        }

        while (value < 0)
        {
            value += count;
        }

        return value % count;
    }
}
