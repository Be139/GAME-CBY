using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    private static DialogueSystem instance;

    [Header("Input")]
    [SerializeField] private KeyCode advanceKey = KeyCode.Space;
    [SerializeField] private KeyCode closeKey = KeyCode.Escape;

    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private string promptMessage = "Space: Continue    Esc: Close";

    private readonly List<DialogueLine> activeLines = new List<DialogueLine>();
    private string activeSpeakerOneName;
    private string activeSpeakerTwoName;
    private int activeLineIndex = -1;

    public static DialogueSystem Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }

            instance = FindObjectOfType<DialogueSystem>();
            if (instance != null)
            {
                return instance;
            }

            GameObject dialogueSystemObject = new GameObject("Dialogue System");
            instance = dialogueSystemObject.AddComponent<DialogueSystem>();
            return instance;
        }
    }

    public bool IsPlaying { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        EnsureUiReferences();
        SetPanelActive(false);
    }

    private void Update()
    {
        if (!IsPlaying)
        {
            return;
        }

        if (Input.GetKeyDown(closeKey))
        {
            Close();
            return;
        }

        if (Input.GetKeyDown(advanceKey))
        {
            Advance();
        }
    }

    public void StartDialogue(string speakerOneName, string speakerTwoName, IList<DialogueLine> lines)
    {
        if (lines == null || lines.Count == 0)
        {
            Debug.LogWarning("DialogueSystem could not start dialogue because no dialogue lines were provided.", this);
            return;
        }

        EnsureUiReferences();

        activeLines.Clear();
        for (int i = 0; i < lines.Count; i++)
        {
            if (lines[i] != null)
            {
                activeLines.Add(lines[i]);
            }
        }

        if (activeLines.Count == 0)
        {
            Debug.LogWarning("DialogueSystem could not start dialogue because all dialogue lines were empty.", this);
            return;
        }

        activeSpeakerOneName = speakerOneName;
        activeSpeakerTwoName = speakerTwoName;
        activeLineIndex = 0;
        IsPlaying = true;
        SetPanelActive(true);
        ShowActiveLine();
    }

    public void Advance()
    {
        if (!IsPlaying)
        {
            return;
        }

        activeLineIndex++;

        if (activeLineIndex >= activeLines.Count)
        {
            Close();
            return;
        }

        ShowActiveLine();
    }

    public void Close()
    {
        IsPlaying = false;
        activeLineIndex = -1;
        activeLines.Clear();
        SetPanelActive(false);
    }

    private void ShowActiveLine()
    {
        DialogueLine line = activeLines[activeLineIndex];

        if (speakerText != null)
        {
            speakerText.text = line.GetSpeakerName(activeSpeakerOneName, activeSpeakerTwoName);
        }

        if (dialogueText != null)
        {
            dialogueText.text = line.text;
        }

        if (promptText != null)
        {
            promptText.text = promptMessage;
        }
    }

    private void SetPanelActive(bool isActive)
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(isActive);
        }
    }

    private void EnsureUiReferences()
    {
        if (dialoguePanel != null && speakerText != null && dialogueText != null)
        {
            return;
        }

        CreateDefaultUi();
    }

    private void CreateDefaultUi()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Dialogue Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        GameObject panelObject = new GameObject("Dialogue Panel");
        panelObject.transform.SetParent(canvas.transform, false);
        dialoguePanel = panelObject;

        RectTransform panelRect = panelObject.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.08f, 0.03f);
        panelRect.anchorMax = new Vector2(0.92f, 0.25f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = panelObject.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.78f);

        speakerText = CreateText(panelObject.transform, "Speaker Text", new Vector2(0.04f, 0.68f), new Vector2(0.96f, 0.95f), 26, FontStyles.Bold);
        dialogueText = CreateText(panelObject.transform, "Dialogue Text", new Vector2(0.04f, 0.22f), new Vector2(0.96f, 0.68f), 22, FontStyles.Normal);
        promptText = CreateText(panelObject.transform, "Prompt Text", new Vector2(0.04f, 0.02f), new Vector2(0.96f, 0.2f), 16, FontStyles.Normal);

        SetPanelActive(false);
    }

    private TMP_Text CreateText(Transform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax, float fontSize, FontStyles fontStyle)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.color = Color.white;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = TextAlignmentOptions.Left;
        text.enableWordWrapping = true;

        return text;
    }
}
