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

    [Header("Audio")]
    [SerializeField] private HearthSfxCuePlayer sfxCuePlayer;
    [SerializeField] private string holdProgressCueId = "UI.HoldProgress";
    [SerializeField] private string holdCompleteCueId = "UI.HoldComplete";

    [Header("Events")]
    [SerializeField] private UnityEvent onHoldCompleted = new UnityEvent();

    private bool pointerHolding;
    private bool completed;
    private bool wasHolding;
    private float progress;

    public UnityEvent OnHoldCompleted { get { return onHoldCompleted; } }
    public bool IsVisible
    {
        get
        {
            return gameObject.activeInHierarchy &&
                   (canvasGroup == null || canvasGroup.alpha > 0.01f);
        }
    }

    private void Awake()
    {
        ResolveReferences();
        holdKey = KeyCode.E;
    }

    private void OnValidate()
    {
        // Story hold actions always use E. Keeping this invariant here also
        // repairs stale serialized data without requiring a scene rebuild.
        holdKey = KeyCode.E;
        holdSeconds = Mathf.Max(0.1f, holdSeconds);
    }

    private void OnDisable()
    {
        StopHoldProgressAudio();
    }

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

        if (isHolding && !wasHolding)
        {
            StartHoldProgressAudio();
        }
        else if (!isHolding && wasHolding)
        {
            StopHoldProgressAudio();
        }

        wasHolding = isHolding;

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

        holdKey = KeyCode.E;
        holdSeconds = Mathf.Max(0.1f, scene.HoldSeconds);
        if (promptText != null)
        {
            promptText.text = scene.HoldPromptText;
        }

        if (keyText != null)
        {
            keyText.text = "E";
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

    public void SetSfxCuePlayer(HearthSfxCuePlayer player)
    {
        sfxCuePlayer = player;
    }

    public void SetVisible(bool visible)
    {
        ResolveReferences();

        if (!visible)
        {
            StopHoldProgressAudio();
        }

        if (visible && !gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible && allowPointerHold;
        }

        if (!visible && gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    public void ResetHold()
    {
        StopHoldProgressAudio();
        pointerHolding = false;
        completed = false;
        wasHolding = false;
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
        StopHoldProgressAudio();
        completed = true;
        wasHolding = false;
        RefreshProgress();

        if (sfxCuePlayer != null)
        {
            sfxCuePlayer.PlayCueOneShot(holdCompleteCueId);
        }

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

    private void ResolveReferences()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (controller == null)
        {
            controller = GetComponentInParent<HearthCompanionHudController>(true);
        }
    }

    private void StartHoldProgressAudio()
    {
        if (sfxCuePlayer != null)
        {
            sfxCuePlayer.StartCueLoop(holdProgressCueId);
        }
    }

    private void StopHoldProgressAudio()
    {
        if (sfxCuePlayer != null)
        {
            sfxCuePlayer.StopCue(holdProgressCueId);
        }

        wasHolding = false;
    }
}
