using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HearthVirusPopupShutdownChallenge : HearthShutdownChallenge
{
    [Header("UI")]
    [SerializeField] private CanvasGroup rootGroup;
    [SerializeField] private RectTransform popupLayer;
    [SerializeField] private RectTransform popupTemplate;
    [SerializeField] private TMP_Text headingText;
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private HearthUiPressFeedback pressFeedback;

    [Header("Input")]
    [SerializeField] private KeyCode submitKey = KeyCode.Space;
    [SerializeField] private KeyCode cancelKey = KeyCode.Escape;

    [Header("Low Trust Virus Pressure")]
    [SerializeField] private int totalPopupCount = 18;
    [SerializeField] private int initialPopupCount = 4;
    [SerializeField] private float spawnInterval = 0.36f;
    [SerializeField] private float popupEnterSeconds = 0.2f;
    [SerializeField] private float popupDismissSeconds = 0.1f;
    [SerializeField] private float screenMargin = 42f;
    [SerializeField] private int randomSeed = 1704;
    [SerializeField] private string[] warningMessages =
    {
        "FAREWELL PROTOCOL BYPASSED",
        "HOUSEHOLD CONTINUITY DATA AT RISK",
        "REMOTE AUTHORITY REJECTED",
        "LILY PROFILE HANDOFF INCOMPLETE",
        "INSPECTOR CLEARANCE FLAGGED",
        "CORE SERVICE RESISTS TERMINATION"
    };

    private readonly List<PopupState> activePopups = new List<PopupState>();
    private Coroutine spawnRoutine;
    private int totalSpawned;
    private int pendingDismissals;
    private bool spawnComplete;
    private bool completing;
    private bool highTrustMode;
    private System.Random random;

    private sealed class PopupState
    {
        public RectTransform rect;
        public CanvasGroup group;
        public Vector2 targetPosition;
    }

    private void Awake()
    {
        SetVisible(false);
        if (popupTemplate != null)
        {
            popupTemplate.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!IsRunning || completing)
        {
            return;
        }

        if (Input.GetKeyDown(cancelKey))
        {
            Cancel();
            return;
        }

        if (Input.GetKeyDown(submitKey))
        {
            Submit();
        }
    }

    private void OnValidate()
    {
        totalPopupCount = Mathf.Max(1, totalPopupCount);
        initialPopupCount = Mathf.Clamp(initialPopupCount, 1, totalPopupCount);
        spawnInterval = Mathf.Max(0.05f, spawnInterval);
        popupEnterSeconds = Mathf.Max(0.01f, popupEnterSeconds);
        popupDismissSeconds = Mathf.Max(0.01f, popupDismissSeconds);
        screenMargin = Mathf.Max(0f, screenMargin);
    }

    public void Configure(
        CanvasGroup group,
        RectTransform layer,
        RectTransform template,
        TMP_Text heading,
        TMP_Text counter,
        TMP_Text instruction,
        HearthUiPressFeedback feedback)
    {
        rootGroup = group;
        popupLayer = layer;
        popupTemplate = template;
        headingText = heading;
        counterText = counter;
        instructionText = instruction;
        pressFeedback = feedback;
        SetVisible(false);
        if (popupTemplate != null)
        {
            popupTemplate.gameObject.SetActive(false);
        }
    }

    public override void BeginChallenge(bool highTrust)
    {
        ResetRuntime();
        highTrustMode = highTrust;
        IsRunning = true;
        SetVisible(true);

        if (headingText != null)
        {
            headingText.text = highTrust
                ? "SHUTDOWN AUTHORIZATION ACCEPTED"
                : "CORE SERVICE TERMINATION CONFLICT";
        }

        if (instructionText != null)
        {
            instructionText.text = highTrust
                ? "PRESS SPACE TO CONFIRM SHUTDOWN"
                : "PRESS SPACE FASTER THAN THE WARNINGS APPEAR";
        }

        if (highTrust)
        {
            SpawnPopup("SHUTDOWN READY", "Farewell protocol is available. Confirm once to continue.");
            spawnComplete = true;
            RefreshCounter();
            return;
        }

        spawnRoutine = StartCoroutine(SpawnPressureRoutine());
    }

    public override void Submit()
    {
        if (!IsRunning || completing)
        {
            return;
        }

        if (pressFeedback != null)
        {
            pressFeedback.PlayFeedback();
        }

        if (activePopups.Count == 0)
        {
            TryComplete();
            return;
        }

        PopupState popup = activePopups[activePopups.Count - 1];
        activePopups.RemoveAt(activePopups.Count - 1);
        pendingDismissals++;
        StartCoroutine(DismissPopupRoutine(popup));
        RefreshCounter();
    }

    public override void Cancel()
    {
        if (!IsRunning)
        {
            return;
        }

        ResetRuntime();
        SetVisible(false);
        cancelled.Invoke();
    }

    private IEnumerator SpawnPressureRoutine()
    {
        random = new System.Random(randomSeed);

        int openingCount = Mathf.Min(initialPopupCount, totalPopupCount);
        for (int i = 0; i < openingCount; i++)
        {
            SpawnWarningPopup();
        }

        while (IsRunning && totalSpawned < totalPopupCount)
        {
            yield return new WaitForSecondsRealtime(spawnInterval);
            if (IsRunning)
            {
                SpawnWarningPopup();
            }
        }

        spawnComplete = true;
        spawnRoutine = null;
        RefreshCounter();
        TryComplete();
    }

    private void SpawnWarningPopup()
    {
        string message = warningMessages != null && warningMessages.Length > 0
            ? warningMessages[totalSpawned % warningMessages.Length]
            : "SHUTDOWN REQUEST REJECTED";
        SpawnPopup("SYSTEM WARNING " + (totalSpawned + 1).ToString("00"), message);
    }

    private void SpawnPopup(string title, string body)
    {
        if (popupTemplate == null || popupLayer == null)
        {
            totalSpawned++;
            Debug.LogWarning("[HearthVirusPopupShutdownChallenge] Popup UI references are missing; counting the warning without rendering it.", this);
            return;
        }

        RectTransform rect = Instantiate(popupTemplate, popupLayer);
        rect.name = "ShutdownWarning_" + (totalSpawned + 1).ToString("00");
        rect.gameObject.SetActive(true);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);

        TMP_Text[] texts = rect.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i].name == "PopupTitle") texts[i].text = title;
            else if (texts[i].name == "PopupBody") texts[i].text = body;
            else if (texts[i].name == "PopupKey") texts[i].text = "SPACE  DISMISS";
        }

        CanvasGroup group = rect.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = rect.gameObject.AddComponent<CanvasGroup>();
        }

        Vector2 target = GetRandomTarget(rect.sizeDelta);
        PopupState state = new PopupState
        {
            rect = rect,
            group = group,
            targetPosition = target
        };
        activePopups.Add(state);
        totalSpawned++;
        StartCoroutine(EnterPopupRoutine(state, GetOffscreenStart(target, rect.sizeDelta)));
        RefreshCounter();
    }

    private IEnumerator EnterPopupRoutine(PopupState popup, Vector2 start)
    {
        if (popup.rect == null)
        {
            yield break;
        }

        popup.rect.anchoredPosition = start;
        popup.rect.localScale = Vector3.one * 0.86f;
        popup.group.alpha = 0.15f;
        float elapsed = 0f;
        while (elapsed < popupEnterSeconds && popup.rect != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / popupEnterSeconds);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            popup.rect.anchoredPosition = Vector2.LerpUnclamped(start, popup.targetPosition, eased);
            popup.rect.localScale = Vector3.Lerp(Vector3.one * 0.86f, Vector3.one, eased);
            popup.group.alpha = Mathf.Lerp(0.15f, 1f, eased);
            yield return null;
        }

        if (popup.rect != null)
        {
            popup.rect.anchoredPosition = popup.targetPosition;
            popup.rect.localScale = Vector3.one;
            popup.group.alpha = 1f;
        }
    }

    private IEnumerator DismissPopupRoutine(PopupState popup)
    {
        if (popup.rect == null)
        {
            pendingDismissals--;
            TryComplete();
            yield break;
        }

        Graphic[] graphics = popup.rect.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            Color color = graphics[i].color;
            float luminance = (color.r + color.g + color.b) / 3f;
            graphics[i].color = new Color(luminance, luminance, luminance, color.a);
        }

        float elapsed = 0f;
        Vector3 startScale = popup.rect.localScale;
        while (elapsed < popupDismissSeconds && popup.rect != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / popupDismissSeconds);
            popup.rect.localScale = Vector3.Lerp(startScale, Vector3.one * 0.72f, t);
            popup.group.alpha = 1f - t;
            yield return null;
        }

        if (popup.rect != null)
        {
            Destroy(popup.rect.gameObject);
        }

        pendingDismissals = Mathf.Max(0, pendingDismissals - 1);
        TryComplete();
    }

    private void TryComplete()
    {
        if (!IsRunning || completing || !spawnComplete || activePopups.Count > 0 || pendingDismissals > 0)
        {
            return;
        }

        completing = true;
        StartCoroutine(CompleteRoutine());
    }

    private IEnumerator CompleteRoutine()
    {
        if (counterText != null)
        {
            counterText.text = highTrustMode ? "SHUTDOWN CONFIRMED" : "ALL HOSTILE WARNINGS CLEARED";
        }

        yield return new WaitForSecondsRealtime(0.16f);
        IsRunning = false;
        completing = false;
        SetVisible(false);
        completed.Invoke();
    }

    private Vector2 GetRandomTarget(Vector2 popupSize)
    {
        float halfWidth = popupSize.x * 0.5f;
        float halfHeight = popupSize.y * 0.5f;
        float minX = -960f + screenMargin + halfWidth;
        float maxX = 960f - screenMargin - halfWidth;
        float minY = -540f + screenMargin + halfHeight + 70f;
        float maxY = 540f - screenMargin - halfHeight - 70f;
        return new Vector2(RandomRange(minX, maxX), RandomRange(minY, maxY));
    }

    private Vector2 GetOffscreenStart(Vector2 target, Vector2 popupSize)
    {
        int edge = random != null ? random.Next(0, 4) : 0;
        switch (edge)
        {
            case 0: return new Vector2(-960f - popupSize.x, target.y);
            case 1: return new Vector2(960f + popupSize.x, target.y);
            case 2: return new Vector2(target.x, 540f + popupSize.y);
            default: return new Vector2(target.x, -540f - popupSize.y);
        }
    }

    private float RandomRange(float minimum, float maximum)
    {
        double t = random != null ? random.NextDouble() : 0.5d;
        return Mathf.Lerp(minimum, maximum, (float)t);
    }

    private void RefreshCounter()
    {
        if (counterText == null)
        {
            return;
        }

        if (highTrustMode)
        {
            counterText.text = activePopups.Count > 0 ? "AUTHORIZATION READY" : "SHUTDOWN CONFIRMED";
            return;
        }

        counterText.text = "ACTIVE WARNINGS  " + activePopups.Count.ToString("00") +
                           "    GENERATED  " + totalSpawned.ToString("00") + " / " + totalPopupCount.ToString("00");
    }

    private void ResetRuntime()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        StopAllCoroutines();
        for (int i = 0; i < activePopups.Count; i++)
        {
            if (activePopups[i].rect != null)
            {
                Destroy(activePopups[i].rect.gameObject);
            }
        }

        activePopups.Clear();
        totalSpawned = 0;
        pendingDismissals = 0;
        spawnComplete = false;
        completing = false;
        IsRunning = false;
    }

    private void SetVisible(bool visible)
    {
        if (rootGroup == null)
        {
            return;
        }

        rootGroup.alpha = visible ? 1f : 0f;
        rootGroup.interactable = false;
        rootGroup.blocksRaycasts = visible;
    }
}
