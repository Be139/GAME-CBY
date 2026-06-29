using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class HearthFirstPersonHudPage : MonoBehaviour
{
    [Header("Page")]
    [SerializeField] private HearthFirstPersonHudPageId pageId = HearthFirstPersonHudPageId.None;
    [SerializeField] private bool fullscreenTakeover;
    [SerializeField] private bool keepPersistentHudVisible = true;

    [Header("Bindings")]
    [SerializeField] private CanvasGroup canvasGroup;

    public HearthFirstPersonHudPageId PageId
    {
        get { return pageId; }
    }

    public bool FullscreenTakeover
    {
        get { return fullscreenTakeover; }
    }

    public bool KeepPersistentHudVisible
    {
        get { return keepPersistentHudVisible; }
    }

    private void Reset()
    {
        EnsureCanvasGroup();
    }

    private void Awake()
    {
        EnsureCanvasGroup();
    }

    public void Configure(
        HearthFirstPersonHudPageId newPageId,
        bool newFullscreenTakeover,
        bool newKeepPersistentHudVisible)
    {
        pageId = newPageId;
        fullscreenTakeover = newFullscreenTakeover;
        keepPersistentHudVisible = newKeepPersistentHudVisible;
        EnsureCanvasGroup();
    }

    public void Show()
    {
        SetVisible(true);
    }

    public void Hide()
    {
        SetVisible(false);
    }

    public void SetVisible(bool visible)
    {
        EnsureCanvasGroup();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }

        gameObject.SetActive(visible);
    }

    private void EnsureCanvasGroup()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }
}
