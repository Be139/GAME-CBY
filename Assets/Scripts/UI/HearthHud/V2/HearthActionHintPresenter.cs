using System;
using TMPro;
using UnityEngine;

[Serializable]
public sealed class HearthActionHintSlot
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private TMP_Text actionText;
    [SerializeField] private RectTransform keycapRect;

    public void Configure(
        GameObject newRoot,
        TMP_Text newKeyText,
        TMP_Text newActionText,
        RectTransform newKeycapRect)
    {
        root = newRoot;
        keyText = newKeyText;
        actionText = newActionText;
        keycapRect = newKeycapRect;
    }

    public void Render(
        HearthActionHintItem item,
        Vector2 regularKeycapSize,
        Vector2 wideKeycapSize)
    {
        if (root != null)
        {
            root.SetActive(true);
        }

        if (keyText != null)
        {
            keyText.text = item.KeyLabel;
            keyText.color = SetAlpha(keyText.color, item.Available ? 1f : 0.45f);
        }

        if (actionText != null)
        {
            actionText.text = item.ActionLabel;
            actionText.color = SetAlpha(actionText.color, item.Available ? 1f : 0.45f);
        }

        if (keycapRect != null)
        {
            bool useWideKeycap = item.WideKeycap ||
                string.Equals(item.KeyLabel, "SPACE", StringComparison.OrdinalIgnoreCase);
            keycapRect.sizeDelta = useWideKeycap ? wideKeycapSize : regularKeycapSize;
        }
    }

    public void Hide()
    {
        if (root != null)
        {
            root.SetActive(false);
        }
    }

    private static Color SetAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }
}

[DisallowMultipleComponent]
public sealed class HearthActionHintPresenter : MonoBehaviour
{
    private static readonly Vector2 DefaultRegularKeycapSize = new Vector2(64f, 40f);
    private static readonly Vector2 DefaultWideKeycapSize = new Vector2(96f, 40f);

    [Header("Bindings")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private HearthActionHintSlot[] slots = new HearthActionHintSlot[0];
    [SerializeField] private HearthUiThemeProfile themeProfile;

    public HearthActionHintState CurrentState { get; private set; }

    public void Configure(
        CanvasGroup newCanvasGroup,
        TMP_Text newStatusText,
        HearthActionHintSlot[] newSlots,
        HearthUiThemeProfile newThemeProfile)
    {
        canvasGroup = newCanvasGroup;
        statusText = newStatusText;
        slots = newSlots ?? new HearthActionHintSlot[0];
        themeProfile = newThemeProfile;
        Apply(HearthActionHintState.Hidden);
    }

    public void Apply(HearthActionHintState state)
    {
        CurrentState = state ?? HearthActionHintState.Hidden;

        bool hasStatus = !string.IsNullOrEmpty(CurrentState.StatusMessage);
        bool visible = CurrentState.Visible &&
            (hasStatus || CurrentState.ItemCount > 0);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (statusText != null)
        {
            statusText.text = CurrentState.StatusMessage;
            statusText.gameObject.SetActive(visible && hasStatus);
        }

        Vector2 regularSize = themeProfile != null
            ? themeProfile.RegularKeycapSize
            : DefaultRegularKeycapSize;
        Vector2 wideSize = themeProfile != null
            ? themeProfile.WideKeycapSize
            : DefaultWideKeycapSize;

        int slotCount = slots != null ? slots.Length : 0;
        for (int i = 0; i < slotCount; i++)
        {
            HearthActionHintSlot slot = slots[i];
            if (slot == null)
            {
                continue;
            }

            if (visible && i < CurrentState.ItemCount)
            {
                slot.Render(CurrentState.GetItem(i), regularSize, wideSize);
            }
            else
            {
                slot.Hide();
            }
        }
    }
}
