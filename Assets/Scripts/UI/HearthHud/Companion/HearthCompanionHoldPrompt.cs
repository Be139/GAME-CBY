using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HearthCompanionHoldPrompt : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Input")]
    [SerializeField] private KeyCode holdKey = KeyCode.E;
    [SerializeField] private float holdSeconds = 1.5f;
    [SerializeField] private bool allowKeyboardHold = true;
    [SerializeField] private bool allowPointerHold = false;
    [SerializeField] private bool resetWhenShown = true;

    [Header("Bindings")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private TMP_Text keyText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Image progressFillImage;
    [SerializeField] private HearthCompanionHudController controller;

    [Header("Events")]
    [SerializeField] private UnityEvent onHoldCompleted = new UnityEvent();

    private bool pointerHolding;
    private bool completed;
    private float progress;

    public UnityEvent OnHoldCompleted { get { return onHoldCompleted; } }

    public void Configure(
        HearthCompanionHudController newController,
        CanvasGroup newCanvasGroup,
        TMP_Text newPromptText,
        TMP_Text newKeyText,
        TMP_Text newProgressText,
        Image newProgressFillImage)
    {
        controller = newController;
        canvasGroup = newCanvasGroup;
        promptText = newPromptText;
        keyText = newKeyText;
        progressText = newProgressText;
        progressFillImage = newProgressFillImage;
        SetVisible(false);
    }

    private void Update()
    {
        if (canvasGroup == null || canvasGroup.alpha <= 0.01f || completed)
        {
            return;
        }

        bool keyboardHolding = allowKeyboardHold && Input.GetKey(holdKey);
        bool isHolding = keyboardHolding || (allowPointerHold && pointerHolding);

        if (isHolding)
        {
            progress += Time.unscaledDeltaTime / Mathf.Max(0.1f, holdSeconds);
            if (progress >= 1f)
            {
                progress = 1f;
                CompleteHold();
            }
        }
        else
        {
            progress = Mathf.MoveTowards(progress, 0f, Time.unscaledDeltaTime * 1.25f);
        }

        RefreshProgress();
    }

    public void Apply(HearthCompanionHudSceneData scene)
    {
        if (scene == null || !scene.ShowHoldPrompt)
        {
            SetVisible(false);
            return;
        }

        holdKey = scene.HoldKey;
        holdSeconds = Mathf.Max(0.1f, scene.HoldSeconds);
        if (promptText != null)
        {
            promptText.text = scene.HoldPromptText;
        }

        if (keyText != null)
        {
            keyText.text = holdKey.ToString().ToUpperInvariant();
        }

        if (resetWhenShown)
        {
            ResetHold();
        }

        SetVisible(true);
    }

    public void SetController(HearthCompanionHudController newController)
    {
        controller = newController;
    }

    public void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible && allowPointerHold;
        }

        gameObject.SetActive(visible);
    }

    public void ResetHold()
    {
        pointerHolding = false;
        completed = false;
        progress = 0f;
        RefreshProgress();
    }

    public void ForceComplete()
    {
        if (!completed)
        {
            progress = 1f;
            CompleteHold();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (allowPointerHold)
        {
            pointerHolding = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pointerHolding = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerHolding = false;
    }

    private void CompleteHold()
    {
        completed = true;
        RefreshProgress();

        if (controller != null)
        {
            controller.ConfirmCurrentPrompt();
        }

        if (onHoldCompleted != null)
        {
            onHoldCompleted.Invoke();
        }
    }

    private void RefreshProgress()
    {
        float clamped = Mathf.Clamp01(progress);
        if (progressFillImage != null)
        {
            progressFillImage.fillAmount = clamped;
        }

        if (progressText != null)
        {
            progressText.text = completed ? "COMPLETE" : "HOLD TO ACT  " + Mathf.RoundToInt(clamped * 100f).ToString("00") + "%";
        }
    }
}
