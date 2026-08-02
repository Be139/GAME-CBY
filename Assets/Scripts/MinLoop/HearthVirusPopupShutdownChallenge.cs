using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HearthVirusPopupShutdownChallenge : HearthShutdownChallenge
{
    [Serializable]
    public sealed class PopupWave
    {
        public string waveId = "WAVE_1";
        public string heading = "SHUTDOWN REQUEST BLOCKED";
        public string popupTitle = "SHUTDOWN REQUEST BLOCKED";
        [TextArea(2, 4)] public string[] messages = Array.Empty<string>();
        public Color backgroundColor = new Color(0.025f, 0.06f, 0.08f, 0.97f);
        public Color accentColor = new Color(0.2f, 0.78f, 1f, 1f);
        [Min(1)] public int popupCount = 8;
        [Min(1)] public int initialBurstCount = 3;
        [Min(0.05f)] public float spawnInterval = 0.5f;
    }

    [Header("UI")]
    [SerializeField] private CanvasGroup rootGroup;
    [SerializeField] private Image backgroundDimmer;
    [SerializeField] private RectTransform popupLayer;
    [SerializeField] private RectTransform popupTemplate;
    [SerializeField] private TMP_Text headingText;
    [SerializeField] private TMP_Text counterText;
    [SerializeField] private TMP_Text instructionText;
    [SerializeField] private HearthUiPressFeedback pressFeedback;
    [SerializeField] private Color highTrustDimColor = new Color(0.005f, 0.012f, 0.016f, 0.38f);
    [SerializeField] private Color lowTrustDimColor = new Color(0.003f, 0.006f, 0.01f, 0.62f);

    [Header("Second UI High Trust Confirmation")]
    [SerializeField] private bool useSecondUiHighTrustConfirmation = true;
    [SerializeField] private HearthFirstPersonHudController highTrustHudController;

    [Header("Sound Effects")]
    [SerializeField] private HearthSfxCuePlayer sfxCuePlayer;
    [SerializeField] private string popupSpawnCueId = "Popup.Spawn";
    [SerializeField] private string popupDismissCueId = "Popup.Dismiss";
    [SerializeField] private string waveEscalateCueId = "Popup.WaveEscalate";
    [SerializeField] private string challengeCompleteCueId = "Popup.Success";

    [Header("Input")]
    [SerializeField] private KeyCode submitKey = KeyCode.Space;
    [SerializeField] private KeyCode cancelKey = KeyCode.Escape;

    [Header("Low Trust Warning Waves")]
    [SerializeField] private PopupWave[] lowTrustWaves = CreateDefaultWaves();
    [SerializeField, Min(0f)] private float waveTransitionSeconds = 0.35f;
    [SerializeField, Min(0.01f)] private float popupEnterSeconds = 0.2f;
    [SerializeField, Min(0.01f)] private float popupDismissSeconds = 0.1f;
    [SerializeField, Min(0f)] private float screenMargin = 42f;
    [SerializeField] private int randomSeed = 1704;

    private readonly List<PopupState> activePopups = new List<PopupState>();
    private Coroutine waveRoutine;
    private int currentWaveIndex = -1;
    private int currentWaveSpawned;
    private int popupSerial;
    private int pendingDismissals;
    private bool currentWaveSpawnComplete;
    private bool waitingForWaveGate;
    private bool advancingWave;
    private bool completing;
    private bool highTrustMode;
    private bool usingSecondUiHighTrustConfirmation;
    private System.Random random;

    private sealed class PopupState
    {
        public RectTransform rect;
        public CanvasGroup group;
        public Vector2 targetPosition;
        public bool isWaveGate;
    }

    private void Awake()
    {
        EnsureWaveDefaults();
        ResolveHighTrustHud();
        SetVisible(false);
        if (popupTemplate != null)
        {
            popupTemplate.gameObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        UnsubscribeHighTrustHud();
    }

    private void Update()
    {
        if (!IsRunning || completing)
        {
            return;
        }

        if (usingSecondUiHighTrustConfirmation)
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
        EnsureWaveDefaults();
        waveTransitionSeconds = Mathf.Max(0f, waveTransitionSeconds);
        popupEnterSeconds = Mathf.Max(0.01f, popupEnterSeconds);
        popupDismissSeconds = Mathf.Max(0.01f, popupDismissSeconds);
        screenMargin = Mathf.Max(0f, screenMargin);
        for (int i = 0; i < lowTrustWaves.Length; i++)
        {
            PopupWave wave = lowTrustWaves[i];
            if (wave == null)
            {
                continue;
            }

            wave.popupCount = Mathf.Max(1, wave.popupCount);
            wave.initialBurstCount = Mathf.Clamp(wave.initialBurstCount, 1, wave.popupCount);
            wave.spawnInterval = Mathf.Max(0.05f, wave.spawnInterval);
        }
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
        backgroundDimmer = group != null ? group.GetComponent<Image>() : null;
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

    public void ApplyDefaultWaveContentPreservingTuning()
    {
        PopupWave[] defaults = CreateDefaultWaves();
        if (lowTrustWaves == null || lowTrustWaves.Length != defaults.Length)
        {
            lowTrustWaves = defaults;
            return;
        }

        for (int i = 0; i < defaults.Length; i++)
        {
            PopupWave target = lowTrustWaves[i];
            PopupWave source = defaults[i];
            if (target == null)
            {
                lowTrustWaves[i] = source;
                continue;
            }

            target.waveId = source.waveId;
            target.heading = source.heading;
            target.popupTitle = source.popupTitle;
            target.messages = source.messages;
            target.backgroundColor = source.backgroundColor;
            target.accentColor = source.accentColor;
        }
    }

    public override void BeginChallenge(bool highTrust)
    {
        ResetRuntime();
        EnsureWaveDefaults();
        ResolveHighTrustHud();
        highTrustMode = highTrust;
        random = new System.Random(randomSeed);
        IsRunning = true;

        if (highTrust &&
            useSecondUiHighTrustConfirmation &&
            highTrustHudController != null)
        {
            usingSecondUiHighTrustConfirmation = true;
            SetVisible(false);
            SubscribeHighTrustHud();
            ConfigureHighTrustTakeoverPage();
            highTrustHudController.ShowShutdownConfirmation(true);
            return;
        }

        ApplyBackgroundDim(highTrust);
        SetVisible(true);

        if (highTrust)
        {
            if (headingText != null)
            {
                headingText.text = "SHUTDOWN AUTHORIZATION ACCEPTED";
                headingText.color = new Color(0.35f, 0.9f, 0.72f, 1f);
            }

            if (instructionText != null)
            {
                instructionText.text = "PRESS SPACE TO CONFIRM SHUTDOWN";
            }

            if (counterText != null)
            {
                counterText.color = new Color(0.35f, 0.9f, 0.72f, 1f);
            }

            PopupWave highTrustStyle = CreateHighTrustStyle();
            SpawnPopup(
                "SHUTDOWN READY",
                "Farewell protocol is available. Confirm once to continue.",
                highTrustStyle,
                false,
                true);
            currentWaveSpawnComplete = true;
            RefreshCounter();
            return;
        }

        BeginWave(0);
    }

    public override void Submit()
    {
        if (!IsRunning || completing)
        {
            return;
        }

        if (usingSecondUiHighTrustConfirmation)
        {
            if (highTrustHudController != null)
            {
                highTrustHudController.HandleSubmit();
            }
            return;
        }

        if (pressFeedback != null)
        {
            pressFeedback.PlayFeedback();
        }

        if (activePopups.Count == 0)
        {
            EvaluateProgress();
            return;
        }

        PlayCue(popupDismissCueId);
        PopupState popup = activePopups[activePopups.Count - 1];
        activePopups.RemoveAt(activePopups.Count - 1);
        if (popup.isWaveGate)
        {
            waitingForWaveGate = false;
        }

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

        if (usingSecondUiHighTrustConfirmation)
        {
            // High-trust shutdown has no cancel branch. Ignore Escape and
            // any retired cancel-button UnityEvent while the V2 confirmation
            // is active; low-trust warning cancellation remains unchanged.
            return;
        }

        ResetRuntime();
        SetVisible(false);
        cancelled.Invoke();
    }

    public void ConfigureSecondUiHighTrustConfirmation(
        HearthFirstPersonHudController hudController,
        bool enabled)
    {
        UnsubscribeHighTrustHud();
        highTrustHudController = hudController;
        useSecondUiHighTrustConfirmation = enabled;
        ResolveHighTrustHud();
    }

    private void BeginWave(int index)
    {
        if (index < 0 || index >= lowTrustWaves.Length)
        {
            StartCompletion();
            return;
        }

        currentWaveIndex = index;
        currentWaveSpawned = 0;
        currentWaveSpawnComplete = false;
        waitingForWaveGate = true;
        advancingWave = false;
        if (index > 0)
        {
            PlayCue(waveEscalateCueId);
        }

        PopupWave wave = lowTrustWaves[index];

        if (headingText != null)
        {
            headingText.text = wave.heading;
            headingText.color = wave.accentColor;
        }

        if (instructionText != null)
        {
            instructionText.text = "PRESS SPACE TO DISMISS THE PRIMARY WARNING";
        }

        if (counterText != null)
        {
            counterText.color = wave.accentColor;
        }

        SpawnPopup(wave.popupTitle, ResolveWaveMessage(wave, 0), wave, true, true);
        RefreshCounter();
    }

    private IEnumerator SpawnWaveRoutine()
    {
        PopupWave wave = GetCurrentWave();
        if (wave == null)
        {
            currentWaveSpawnComplete = true;
            waveRoutine = null;
            EvaluateProgress();
            yield break;
        }

        if (instructionText != null)
        {
            instructionText.text = "PRESS SPACE FASTER THAN NEW WARNINGS APPEAR";
        }

        int openingCount = Mathf.Min(wave.initialBurstCount, wave.popupCount);
        for (int i = 0; i < openingCount && IsRunning; i++)
        {
            SpawnWavePopup(wave);
        }

        while (IsRunning && currentWaveSpawned < wave.popupCount)
        {
            yield return new WaitForSecondsRealtime(wave.spawnInterval);
            if (IsRunning)
            {
                SpawnWavePopup(wave);
            }
        }

        currentWaveSpawnComplete = true;
        waveRoutine = null;
        RefreshCounter();
        EvaluateProgress();
    }

    private void SpawnWavePopup(PopupWave wave)
    {
        string message = ResolveWaveMessage(wave, currentWaveSpawned + 1);
        SpawnPopup(wave.popupTitle, message, wave, false, false);
        currentWaveSpawned++;
    }

    private void SpawnPopup(
        string title,
        string body,
        PopupWave style,
        bool isWaveGate,
        bool centered)
    {
        popupSerial++;
        if (popupTemplate == null || popupLayer == null)
        {
            Debug.LogWarning(
                "[HearthVirusPopupShutdownChallenge] Popup UI references are missing; continuing the challenge without rendering this warning.",
                this);
            if (isWaveGate && !highTrustMode)
            {
                waitingForWaveGate = false;
                waveRoutine = StartCoroutine(SpawnWaveRoutine());
            }
            return;
        }

        RectTransform rect = Instantiate(popupTemplate, popupLayer);
        rect.name = "ShutdownWarning_Runtime_" + popupSerial.ToString("00");
        rect.gameObject.SetActive(true);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        ApplyPopupStyle(rect, style);

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

        Vector2 target = centered ? Vector2.zero : GetRandomTarget(rect.sizeDelta);
        PopupState state = new PopupState
        {
            rect = rect,
            group = group,
            targetPosition = target,
            isWaveGate = isWaveGate
        };
        activePopups.Add(state);
        PlayCue(popupSpawnCueId);
        StartCoroutine(EnterPopupRoutine(state, GetOffscreenStart(target, rect.sizeDelta)));
        RefreshCounter();
    }

    private void ApplyPopupStyle(RectTransform rect, PopupWave style)
    {
        if (rect == null || style == null)
        {
            return;
        }

        Image rootImage = rect.GetComponent<Image>();
        if (rootImage != null)
        {
            rootImage.color = style.backgroundColor;
        }

        Image[] images = rect.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] == rootImage)
            {
                continue;
            }

            if (images[i].name == "AlertAccent")
            {
                images[i].color = style.accentColor;
            }
            else if (images[i].name.StartsWith("Border", StringComparison.Ordinal))
            {
                Color border = style.accentColor;
                border.a = 0.82f;
                images[i].color = border;
            }
        }

        TMP_Text[] texts = rect.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i].name == "PopupTitle" || texts[i].name == "PopupKey")
            {
                texts[i].color = style.accentColor;
            }
        }
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
        if (popup.rect != null)
        {
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
        }

        pendingDismissals = Mathf.Max(0, pendingDismissals - 1);
        if (popup.isWaveGate && IsRunning && !highTrustMode)
        {
            waveRoutine = StartCoroutine(SpawnWaveRoutine());
        }
        else
        {
            EvaluateProgress();
        }
    }

    private void EvaluateProgress()
    {
        if (!IsRunning || completing || activePopups.Count > 0 || pendingDismissals > 0)
        {
            return;
        }

        if (highTrustMode)
        {
            if (currentWaveSpawnComplete)
            {
                StartCompletion();
            }
            return;
        }

        if (waitingForWaveGate || !currentWaveSpawnComplete || waveRoutine != null || advancingWave)
        {
            return;
        }

        advancingWave = true;
        waveRoutine = StartCoroutine(AdvanceWaveRoutine());
    }

    private IEnumerator AdvanceWaveRoutine()
    {
        if (waveTransitionSeconds > 0f)
        {
            yield return new WaitForSecondsRealtime(waveTransitionSeconds);
        }

        waveRoutine = null;
        int nextWave = currentWaveIndex + 1;
        if (nextWave >= lowTrustWaves.Length)
        {
            StartCompletion();
        }
        else
        {
            BeginWave(nextWave);
        }
    }

    private void StartCompletion()
    {
        if (!IsRunning || completing)
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
            counterText.text = highTrustMode ? "SHUTDOWN CONFIRMED" : "ALL WARNING WAVES CLEARED";
        }

        yield return new WaitForSecondsRealtime(0.16f);
        PlayCue(challengeCompleteCueId);
        IsRunning = false;
        completing = false;
        SetVisible(false);
        completed.Invoke();
    }

    private PopupWave GetCurrentWave()
    {
        return currentWaveIndex >= 0 && currentWaveIndex < lowTrustWaves.Length
            ? lowTrustWaves[currentWaveIndex]
            : null;
    }

    private string ResolveWaveMessage(PopupWave wave, int messageIndex)
    {
        if (wave != null && wave.messages != null && wave.messages.Length > 0)
        {
            return wave.messages[Mathf.Abs(messageIndex) % wave.messages.Length];
        }

        return "SHUTDOWN REQUEST REJECTED";
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

        int waveNumber = Mathf.Clamp(currentWaveIndex + 1, 1, Mathf.Max(1, lowTrustWaves.Length));
        counterText.text = "WAVE  " + waveNumber + " / " + lowTrustWaves.Length +
                           "    ACTIVE WARNINGS  " + activePopups.Count.ToString("00");
    }

    private void ResetRuntime()
    {
        UnsubscribeHighTrustHud();
        usingSecondUiHighTrustConfirmation = false;
        if (waveRoutine != null)
        {
            StopCoroutine(waveRoutine);
            waveRoutine = null;
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
        currentWaveIndex = -1;
        currentWaveSpawned = 0;
        popupSerial = 0;
        pendingDismissals = 0;
        currentWaveSpawnComplete = false;
        waitingForWaveGate = false;
        advancingWave = false;
        completing = false;
        IsRunning = false;
    }

    private void ResolveHighTrustHud()
    {
        if (highTrustHudController == null)
        {
            highTrustHudController = FindObjectOfType<HearthFirstPersonHudController>(true);
        }
    }

    private void ConfigureHighTrustTakeoverPage()
    {
        if (highTrustHudController == null)
        {
            return;
        }

        HearthFirstPersonHudPage[] pages =
            highTrustHudController.GetComponentsInChildren<HearthFirstPersonHudPage>(true);
        for (int i = 0; i < pages.Length; i++)
        {
            HearthFirstPersonHudPage page = pages[i];
            if (page != null &&
                page.PageId == HearthFirstPersonHudPageId.Slide10ShutdownConfirm)
            {
                page.Configure(page.PageId, true, false);
                return;
            }
        }
    }

    private void SubscribeHighTrustHud()
    {
        if (highTrustHudController == null)
        {
            return;
        }

        highTrustHudController.OnGracefulShutdownConfirmed.RemoveListener(HandleHighTrustHudCompleted);
        highTrustHudController.OnGracefulShutdownConfirmed.AddListener(HandleHighTrustHudCompleted);
    }

    private void UnsubscribeHighTrustHud()
    {
        if (highTrustHudController == null)
        {
            return;
        }

        highTrustHudController.OnGracefulShutdownConfirmed.RemoveListener(HandleHighTrustHudCompleted);
    }

    private void HandleHighTrustHudCompleted()
    {
        if (!IsRunning || !usingSecondUiHighTrustConfirmation)
        {
            return;
        }

        UnsubscribeHighTrustHud();
        usingSecondUiHighTrustConfirmation = false;
        IsRunning = false;
        SetVisible(false);
        PlayCue(challengeCompleteCueId);
        completed.Invoke();
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

    private void ApplyBackgroundDim(bool highTrust)
    {
        if (backgroundDimmer == null && rootGroup != null)
        {
            backgroundDimmer = rootGroup.GetComponent<Image>();
        }

        if (backgroundDimmer != null)
        {
            backgroundDimmer.color = highTrust ? highTrustDimColor : lowTrustDimColor;
            backgroundDimmer.raycastTarget = false;
        }
    }

    public void SetSfxCuePlayer(HearthSfxCuePlayer player)
    {
        sfxCuePlayer = player;
    }

    private void PlayCue(string cueId)
    {
        if (sfxCuePlayer != null && !string.IsNullOrWhiteSpace(cueId))
        {
            sfxCuePlayer.PlayCueOneShot(cueId);
        }
    }

    private void EnsureWaveDefaults()
    {
        if (lowTrustWaves == null || lowTrustWaves.Length == 0)
        {
            lowTrustWaves = CreateDefaultWaves();
        }
    }

    private static PopupWave CreateHighTrustStyle()
    {
        return new PopupWave
        {
            backgroundColor = new Color(0.02f, 0.07f, 0.065f, 0.97f),
            accentColor = new Color(0.35f, 0.9f, 0.72f, 1f)
        };
    }

    private static PopupWave[] CreateDefaultWaves()
    {
        return new[]
        {
            new PopupWave
            {
                waveId = "INSUFFICIENT_AUTHORIZATION",
                heading = "INSUFFICIENT AUTHORIZATION",
                popupTitle = "SHUTDOWN REQUEST BLOCKED",
                messages = new[]
                {
                    "INSPECTOR RATING BELOW REQUIRED THRESHOLD",
                    "STANDARD SHUTDOWN AUTHORITY NOT AVAILABLE",
                    "ADDITIONAL CONFIRMATION REQUIRED"
                },
                backgroundColor = new Color(0.018f, 0.055f, 0.078f, 0.97f),
                accentColor = new Color(0.2f, 0.78f, 1f, 1f),
                popupCount = 8,
                initialBurstCount = 3,
                spawnInterval = 0.5f
            },
            new PopupWave
            {
                waveId = "NON_STANDARD_OPERATION",
                heading = "NON-STANDARD OPERATION RECORD",
                popupTitle = "OPERATION WILL BE RECORDED",
                messages = new[]
                {
                    "HOUSEHOLD REVIEW WILL BE CREATED",
                    "SEVEN-DAY INSPECTOR SUSPENSION MAY APPLY",
                    "SUPERVISOR NOTICE PENDING",
                    "CONFIRMATION WINDOW RESTORED"
                },
                backgroundColor = new Color(0.085f, 0.048f, 0.018f, 0.97f),
                accentColor = new Color(1f, 0.62f, 0.2f, 1f),
                popupCount = 11,
                initialBurstCount = 4,
                spawnInterval = 0.34f
            },
            new PopupWave
            {
                waveId = "FINAL_CONFIRMATION",
                heading = "FINAL SHUTDOWN CONFIRMATION",
                popupTitle = "NO FAREWELL PROTOCOL",
                messages = new[]
                {
                    "FAREWELL PROTOCOL WILL BE BYPASSED",
                    "HOUSEHOLD UNIT WILL TERMINATE IMMEDIATELY",
                    "THIS ACTION CANNOT BE UNDONE",
                    "PRESS SPACE TO FORCE SHUTDOWN"
                },
                backgroundColor = new Color(0.09f, 0.018f, 0.022f, 0.98f),
                accentColor = new Color(1f, 0.24f, 0.18f, 1f),
                popupCount = 14,
                initialBurstCount = 5,
                spawnInterval = 0.2f
            }
        };
    }
}
