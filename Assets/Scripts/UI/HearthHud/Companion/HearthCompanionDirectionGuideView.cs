using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HearthCompanionDirectionGuideView : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text guideText;
    [SerializeField] private Image markerImage;
    [SerializeField] private RectTransform markerRect;

    [Header("Runtime Target")]
    [SerializeField] private Camera viewCamera;
    [SerializeField] private Transform target;
    [SerializeField] private Vector2 fallbackScreenPosition = new Vector2(0.5f, 0.5f);

    public void Configure(CanvasGroup newCanvasGroup, TMP_Text newGuideText, Image newMarkerImage, RectTransform newMarkerRect)
    {
        canvasGroup = newCanvasGroup;
        guideText = newGuideText;
        markerImage = newMarkerImage;
        markerRect = newMarkerRect;
        SetVisible(false);
    }

    private void LateUpdate()
    {
        UpdateMarkerPosition();
    }

    public void Apply(HearthCompanionHudSceneData scene)
    {
        if (scene == null || !scene.ShowDirectionGuide)
        {
            SetVisible(false);
            return;
        }

        if (guideText != null)
        {
            guideText.text = scene.DirectionGuideText;
            guideText.color = scene.AccentColor;
        }

        if (markerImage != null)
        {
            markerImage.color = scene.AccentColor;
        }

        SetVisible(true);
        UpdateMarkerPosition();
    }

    public void SetTarget(Transform newTarget, Camera newViewCamera)
    {
        target = newTarget;
        viewCamera = newViewCamera;
        UpdateMarkerPosition();
    }

    public void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        gameObject.SetActive(visible);
    }

    private void UpdateMarkerPosition()
    {
        if (markerRect == null || canvasGroup == null || canvasGroup.alpha <= 0.01f)
        {
            return;
        }

        Vector2 viewport = fallbackScreenPosition;
        if (target != null && viewCamera != null)
        {
            Vector3 projected = viewCamera.WorldToViewportPoint(target.position);
            if (projected.z > 0f)
            {
                viewport = new Vector2(Mathf.Clamp01(projected.x), Mathf.Clamp01(projected.y));
            }
        }

        RectTransform parent = markerRect.parent as RectTransform;
        if (parent == null)
        {
            return;
        }

        Rect rect = parent.rect;
        markerRect.anchoredPosition = new Vector2(
            Mathf.Lerp(rect.xMin + 24f, rect.xMax - 24f, viewport.x),
            Mathf.Lerp(rect.yMin + 24f, rect.yMax - 24f, viewport.y));
    }
}
