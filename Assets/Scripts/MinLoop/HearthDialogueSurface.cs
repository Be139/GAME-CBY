using TMPro;
using UnityEngine;

/// <summary>
/// A visual-only dialogue target for a world-space terminal or TV. Audio,
/// timing and Space input remain owned by MinLoopSubtitlePlayer.
/// </summary>
[DisallowMultipleComponent]
public class HearthDialogueSurface : MonoBehaviour
{
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text advanceHintText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private string manualAdvanceLabel = "SPACE  CONTINUE";
    [SerializeField] private HearthDialogueSurface exclusivePeer;

    public bool IsVisible { get; private set; }

    private void Awake()
    {
        ResolveReferences();
        HideImmediate();
    }

    private void OnDisable()
    {
        IsVisible = false;
    }

    public void ConfigureBindings(
        GameObject root,
        TMP_Text speaker,
        TMP_Text body,
        TMP_Text advanceHint,
        CanvasGroup group = null)
    {
        visualRoot = root;
        speakerText = speaker;
        bodyText = body;
        advanceHintText = advanceHint;
        canvasGroup = group;
        ResolveReferences();
    }

    public void Configure(
        CanvasGroup root,
        TMP_Text speaker,
        TMP_Text body,
        TMP_Text advanceHint)
    {
        ConfigureBindings(
            root != null ? root.gameObject : null,
            speaker,
            body,
            advanceHint,
            root);
    }

    public void SetExclusivePeer(HearthDialogueSurface peer)
    {
        exclusivePeer = peer != this ? peer : null;
    }

    public void ApplyTypography(
        float speakerFontSize,
        float bodyFontSize,
        float advanceFontSize)
    {
        ResolveReferences();
        ApplyFixedFontSize(speakerText, speakerFontSize);
        ApplyFixedFontSize(bodyText, bodyFontSize);
        ApplyFixedFontSize(advanceHintText, advanceFontSize);
    }

    public void ApplyTerminalInternalLayout()
    {
        ResolveReferences();
        SetStretchRect(speakerText, 0.035f, 0.69f, 0.965f, 0.96f);
        SetStretchRect(bodyText, 0.035f, 0.22f, 0.965f, 0.69f);
        SetStretchRect(advanceHintText, 0.62f, 0.025f, 0.965f, 0.21f);
        if (speakerText != null)
        {
            speakerText.alignment = TextAlignmentOptions.TopLeft;
        }
        if (bodyText != null)
        {
            bodyText.alignment = TextAlignmentOptions.TopLeft;
        }
        if (advanceHintText != null)
        {
            advanceHintText.alignment = TextAlignmentOptions.BottomRight;
        }
    }

    public void Show(
        string speaker,
        string text,
        bool showSpeaker,
        bool showAdvanceHint)
    {
        ResolveReferences();

        if (exclusivePeer != null && exclusivePeer.IsVisible)
        {
            exclusivePeer.HideImmediate();
        }

        GameObject root = GetVisualRoot();
        if (root != null)
        {
            root.SetActive(true);
        }

        if (speakerText != null)
        {
            speakerText.text = speaker ?? string.Empty;
            speakerText.gameObject.SetActive(
                showSpeaker && !string.IsNullOrWhiteSpace(speaker));
        }

        if (bodyText != null)
        {
            bodyText.text = text ?? string.Empty;
            bodyText.gameObject.SetActive(true);
        }

        if (advanceHintText != null)
        {
            advanceHintText.text = manualAdvanceLabel;
            advanceHintText.gameObject.SetActive(showAdvanceHint);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        IsVisible = true;
    }

    public void HideImmediate()
    {
        GameObject root = GetVisualRoot();
        if (root != null)
        {
            root.SetActive(false);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        IsVisible = false;
    }

    private GameObject GetVisualRoot()
    {
        return visualRoot != null ? visualRoot : gameObject;
    }

    private void ResolveReferences()
    {
        GameObject root = GetVisualRoot();
        if (root == null)
        {
            return;
        }

        if (canvasGroup == null)
        {
            canvasGroup = root.GetComponent<CanvasGroup>();
        }

        TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
        if (speakerText == null)
        {
            speakerText = FindText(texts, "Speaker", "SpeakerName", "Name");
        }

        if (bodyText == null)
        {
            bodyText = FindText(texts, "Body", "DialogueText", "MessageText");
        }

        if (advanceHintText == null)
        {
            advanceHintText = FindText(texts, "AdvanceHint", "ActionHint");
        }
    }

    private static TMP_Text FindText(TMP_Text[] texts, params string[] names)
    {
        if (texts == null)
        {
            return null;
        }

        for (int nameIndex = 0; nameIndex < names.Length; nameIndex++)
        {
            for (int textIndex = 0; textIndex < texts.Length; textIndex++)
            {
                TMP_Text candidate = texts[textIndex];
                if (candidate != null && candidate.name == names[nameIndex])
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private static void ApplyFixedFontSize(TMP_Text text, float size)
    {
        if (text == null)
        {
            return;
        }

        float safeSize = Mathf.Max(1f, size);
        text.enableAutoSizing = false;
        text.fontSize = safeSize;
        text.fontSizeMin = safeSize;
        text.fontSizeMax = safeSize;
    }


    private static void SetStretchRect(
        TMP_Text text,
        float minX,
        float minY,
        float maxX,
        float maxY)
    {
        if (text == null)
        {
            return;
        }

        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(minX, minY);
        rect.anchorMax = new Vector2(maxX, maxY);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
