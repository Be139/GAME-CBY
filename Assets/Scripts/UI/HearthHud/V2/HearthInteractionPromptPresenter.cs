using TMPro;
using UnityEngine;

public interface IHearthInteractionPromptPresenter
{
    GameObject Root { get; }
    TMP_Text Label { get; }
    bool IsVisible { get; }
    void Show(string message);
    void Hide();
}

/// <summary>
/// Shared V2 presentation component for short-press interaction prompts.
/// PlayerInteraction owns target detection; this component owns only visuals.
/// </summary>
[DisallowMultipleComponent]
public sealed class HearthInteractionPromptPresenter : MonoBehaviour,
    IHearthInteractionPromptPresenter
{
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private CanvasGroup canvasGroup;

    public GameObject Root
    {
        get { return promptRoot != null ? promptRoot : gameObject; }
    }

    public TMP_Text Label
    {
        get { return promptText; }
    }

    public bool IsVisible
    {
        get
        {
            GameObject root = Root;
            return root != null && root.activeInHierarchy &&
                (canvasGroup == null || canvasGroup.alpha > 0.001f);
        }
    }

    public bool IsComplete
    {
        get { return Root != null && promptText != null && canvasGroup != null; }
    }

    private void Awake()
    {
        ResolveLocalReferences();
    }

    public void Configure(
        GameObject newPromptRoot,
        TMP_Text newPromptText,
        CanvasGroup newCanvasGroup)
    {
        promptRoot = newPromptRoot;
        promptText = newPromptText;
        canvasGroup = newCanvasGroup;
    }

    public void Show(string message)
    {
        ResolveLocalReferences();
        if (promptText != null && message != null)
        {
            promptText.text = message;
        }

        GameObject root = Root;
        if (root != null && !root.activeSelf)
        {
            root.SetActive(true);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void Hide()
    {
        ResolveLocalReferences();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        GameObject root = Root;
        if (root != null && root.activeSelf)
        {
            root.SetActive(false);
        }
    }

    private void ResolveLocalReferences()
    {
        if (promptRoot == null)
        {
            promptRoot = gameObject;
        }

        if (promptText == null)
        {
            promptText = GetComponentInChildren<TMP_Text>(true);
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }
}
