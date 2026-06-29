using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HearthCompanionStatusPanelView : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text rowsText;
    [SerializeField] private TMP_Text footerText;
    [SerializeField] private Image accentImage;

    private readonly StringBuilder builder = new StringBuilder(256);

    public void Configure(TMP_Text newTitleText, TMP_Text newRowsText, TMP_Text newFooterText, Image newAccentImage)
    {
        titleText = newTitleText;
        rowsText = newRowsText;
        footerText = newFooterText;
        accentImage = newAccentImage;
    }

    public void Apply(HearthCompanionHudSceneData scene)
    {
        if (scene == null)
        {
            Clear();
            return;
        }

        if (titleText != null)
        {
            titleText.text = scene.StatusTitle;
            titleText.color = scene.AccentColor;
        }

        builder.Length = 0;
        HearthCompanionMetricLine[] lines = scene.StatusLines;
        if (lines != null)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                HearthCompanionMetricLine line = lines[i];
                if (line == null)
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                builder.Append(line.label);
                if (!string.IsNullOrEmpty(line.value))
                {
                    builder.Append("    ");
                    builder.Append(line.value);
                }
            }
        }

        if (rowsText != null)
        {
            rowsText.text = builder.ToString();
        }

        if (footerText != null)
        {
            footerText.text = scene.StatusFooter;
        }

        if (accentImage != null)
        {
            accentImage.color = scene.AccentColor;
        }
    }

    public void Clear()
    {
        if (titleText != null)
        {
            titleText.text = string.Empty;
        }

        if (rowsText != null)
        {
            rowsText.text = string.Empty;
        }

        if (footerText != null)
        {
            footerText.text = string.Empty;
        }
    }
}
