using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HearthHoldToActButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Input")]
    [SerializeField] private KeyCode holdKey = KeyCode.E;
    [SerializeField] private float holdSeconds = 1.35f;
    [SerializeField] private bool allowKeyboardHold = true;
    [SerializeField] private bool allowPointerHold = true;
    [SerializeField] private bool resetWhenDisabled = true;

    [Header("Bindings")]
    [SerializeField] private Image progressFillImage;
    [SerializeField] private TMP_Text progressLabelText;
    [SerializeField] private HearthHudController controller;
    [SerializeField] private bool notifyControllerOnCompleted = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onHoldCompleted;

    private bool pointerHolding;
    private bool completed;
    private float progress;

    public float Progress
    {
        get { return progress; }
    }

    private void OnEnable()
    {
        FindControllerIfMissing();

        if (resetWhenDisabled)
        {
            ResetHold();
        }
    }

    private void OnDisable()
    {
        if (resetWhenDisabled)
        {
            ResetHold();
        }
    }

    private void Update()
    {
        bool keyboardHolding = allowKeyboardHold && Input.GetKey(holdKey);
        bool isHolding = keyboardHolding || (allowPointerHold && pointerHolding);

        if (completed)
        {
            return;
        }

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
            progress = Mathf.MoveTowards(progress, 0f, Time.unscaledDeltaTime * 1.2f);
        }

        RefreshProgress();
    }

    public void Configure(HearthHudController newController, Image newProgressFillImage, TMP_Text newProgressLabelText)
    {
        controller = newController;
        progressFillImage = newProgressFillImage;
        progressLabelText = newProgressLabelText;
        RefreshProgress();
    }

    public void ResetHold()
    {
        pointerHolding = false;
        completed = false;
        progress = 0f;
        RefreshProgress();
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

        if (notifyControllerOnCompleted && controller != null)
        {
            controller.CompleteRobotReplay();
        }

        if (onHoldCompleted != null)
        {
            onHoldCompleted.Invoke();
        }
    }

    private void FindControllerIfMissing()
    {
        if (controller != null)
        {
            return;
        }

        controller = GetComponentInParent<HearthHudController>();
        if (controller == null)
        {
            controller = FindObjectOfType<HearthHudController>();
        }
    }

    private void RefreshProgress()
    {
        if (progressFillImage != null)
        {
            progressFillImage.fillAmount = Mathf.Clamp01(progress);
        }

        if (progressLabelText != null)
        {
            int percent = Mathf.RoundToInt(Mathf.Clamp01(progress) * 100f);
            progressLabelText.text = completed ? "COMPLETE" : "HOLD E  " + percent.ToString("00") + "%";
        }
    }
}
