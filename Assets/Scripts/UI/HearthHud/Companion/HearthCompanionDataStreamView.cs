using System.Text;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class HearthCompanionDataStreamView : MonoBehaviour
{
    [Header("Bindings")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text streamText;

    [Header("Display")]
    [SerializeField] private string linePrefix = "";

    private readonly StringBuilder builder = new StringBuilder(512);

    public void Configure(TMP_Text newTitleText, TMP_Text newStreamText)
    {
        titleText = newTitleText;
        streamText = newStreamText;
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
            titleText.text = scene.DataStreamTitle;
            titleText.color = scene.AccentColor;
        }

        SetLines(scene.DataStreamLines);
    }

    public void SetLines(string[] lines)
    {
        builder.Length = 0;

        if (lines != null)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrEmpty(lines[i]))
                {
                    continue;
                }

                if (builder.Length > 0)
                {
                    builder.AppendLine();
                }

                builder.Append(linePrefix);
                builder.Append(lines[i]);
            }
        }

        if (streamText != null)
        {
            streamText.text = builder.ToString();
        }
    }

    public void AppendLine(string line)
    {
        if (string.IsNullOrEmpty(line))
        {
            return;
        }

        if (streamText == null)
        {
            return;
        }

        string current = streamText.text;
        streamText.text = string.IsNullOrEmpty(current) ? linePrefix + line : current + "\n" + linePrefix + line;
    }

    public void Clear()
    {
        if (titleText != null)
        {
            titleText.text = string.Empty;
        }

        if (streamText != null)
        {
            streamText.text = string.Empty;
        }
    }
}
