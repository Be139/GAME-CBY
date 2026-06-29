using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HearthCompanionProjectionPanelView : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Image accentImage;

    public void Configure(CanvasGroup newCanvasGroup, TMP_Text newTitleText, TMP_Text newBodyText, Image newAccentImage)
    {
        canvasGroup = newCanvasGroup;
        titleText = newTitleText;
        bodyText = newBodyText;
        accentImage = newAccentImage;
        SetVisible(false);
    }

    public void Apply(HearthCompanionHudSceneData scene)
    {
        if (scene == null || scene.Template != HearthCompanionHudTemplate.Projection)
        {
            SetVisible(false);
            return;
        }

        if (titleText != null)
        {
            titleText.text = scene.ProjectionTitle;
            titleText.color = scene.AccentColor;
        }

        if (bodyText != null)
        {
            bodyText.text = scene.ProjectionBody;
        }

        if (accentImage != null)
        {
            accentImage.color = scene.AccentColor;
        }

        SetVisible(true);
    }

    public void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = false;
        }

        gameObject.SetActive(visible);
    }
}
