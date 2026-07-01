using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HearthFirstPersonHudController : MonoBehaviour
{
    [Header("Pages")]
    [SerializeField] private HearthFirstPersonHudPage[] pages;
    [SerializeField] private HearthFirstPersonHudPageId startingPage = HearthFirstPersonHudPageId.Slide01PersistentHud;

    [Header("Persistent HUD")]
    [SerializeField] private GameObject persistentHudRoot;
    [SerializeField] private CanvasGroup persistentHudCanvasGroup;
    [SerializeField] private Image statusDotImage;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text taskText;
    [SerializeField] private TMP_Text clockText;

    [Header("Trust")]
    [SerializeField] private int trustScore;
    [SerializeField] private int finalChoiceTrustThreshold = 3;
    [SerializeField] private CanvasGroup trustDeltaCanvasGroup;
    [SerializeField] private TMP_Text trustDeltaText;
    [SerializeField] private float trustDeltaSeconds = 3f;

    [Header("Rounds")]
    [SerializeField] private int completedRounds;
    [SerializeField] private int totalRounds = 3;

    [Header("Menu Focus")]
    [SerializeField] private RectTransform menuFocusRect;
    [SerializeField] private RectTransform[] menuFocusTargets;
    [SerializeField] private Vector2 menuFocusPadding = new Vector2(8f, 4f);

    [Header("Final Choice Focus")]
    [SerializeField] private RectTransform finalChoiceFocusRect;
    [SerializeField] private RectTransform[] finalChoiceFocusTargets;
    [SerializeField] private Vector2 finalChoiceFocusPadding = new Vector2(10f, 6f);

    [Header("Sub Views")]
    [SerializeField] private HearthDispositionHistoryView dispositionHistoryView;
    [SerializeField] private HearthSettingsView settingsView;

    [Header("Player Control Lock")]
    [SerializeField] private bool lockPlayerControlsWhileOverlayOpen = true;
    [SerializeField] private HearthPlayerControlLock playerControlLock;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip openMenuClip;
    [SerializeField] private AudioClip closeMenuClip;
    [SerializeField] private AudioClip pageChangedClip;
    [SerializeField] private AudioClip focusMovedClip;
    [SerializeField] private AudioClip confirmClip;
    [SerializeField] private AudioClip cancelClip;
    [SerializeField] private AudioClip warningClip;
    [SerializeField] private AudioClip trustDeltaClip;

    [Header("Events")]
    [SerializeField] private HearthFirstPersonHudPageEvent pageShown = new HearthFirstPersonHudPageEvent();
    [SerializeField] private UnityEvent onSyncConfirmed = new UnityEvent();
    [SerializeField] private UnityEvent onFinalChoiceA = new UnityEvent();
    [SerializeField] private UnityEvent onFinalChoiceB = new UnityEvent();
    [SerializeField] private UnityEvent onGracefulShutdownConfirmed = new UnityEvent();
    [SerializeField] private UnityEvent onForcedShutdownConfirmed = new UnityEvent();
    [SerializeField] private UnityEvent onExitConfirmed = new UnityEvent();
    [SerializeField] private UnityEvent onExitCancelled = new UnityEvent();
    [SerializeField] private HearthFirstPersonEndingEvent endingShown = new HearthFirstPersonEndingEvent();
    [SerializeField] private HearthTrustDeltaEvent trustDeltaShown = new HearthTrustDeltaEvent();

    private readonly Dictionary<HearthFirstPersonHudPageId, HearthFirstPersonHudPage> pageMap =
        new Dictionary<HearthFirstPersonHudPageId, HearthFirstPersonHudPage>();

    private HearthFirstPersonHudPageId currentPageId = HearthFirstPersonHudPageId.None;
    private int menuSelectionIndex;
    private int finalChoiceSelectionIndex;
    private Coroutine trustDeltaRoutine;

    public HearthFirstPersonHudPageId CurrentPageId
    {
        get { return currentPageId; }
    }

    public int TrustScore
    {
        get { return trustScore; }
    }

    public int FinalChoiceTrustThreshold
    {
        get { return finalChoiceTrustThreshold; }
    }

    public HearthFirstPersonHudPageEvent PageShown
    {
        get { return pageShown; }
    }

    public UnityEvent OnSyncConfirmed
    {
        get { return onSyncConfirmed; }
    }

    public UnityEvent OnExitConfirmed
    {
        get { return onExitConfirmed; }
    }

    private void Awake()
    {
        BuildPageMap();
        ResolvePlayerControlLock();
        HideAllPages();
        HideFocusRects();
        HideTrustDeltaImmediate();
    }

    private void Start()
    {
        ShowPage(startingPage);
    }

    public void ConfigurePages(HearthFirstPersonHudPage[] newPages)
    {
        pages = newPages;
        BuildPageMap();
    }

    public void ShowPage(HearthFirstPersonHudPageId pageId)
    {
        BuildPageMap();

        if (pageId == HearthFirstPersonHudPageId.Slide02TrustDelta)
        {
            ShowTrustDelta(1);
            return;
        }

        HideAllPages();

        if (pageId == HearthFirstPersonHudPageId.None || pageId == HearthFirstPersonHudPageId.Slide01PersistentHud)
        {
            currentPageId = HearthFirstPersonHudPageId.Slide01PersistentHud;
            SetPersistentVisible(true);
            HideFocusRects();
            SetPlayerControlsLocked(false);
            pageShown.Invoke(currentPageId);
            return;
        }

        HearthFirstPersonHudPage page;
        if (!pageMap.TryGetValue(pageId, out page) || page == null)
        {
            Debug.LogWarning("[HearthFirstPersonHudController] Page not found: " + pageId, this);
            SetPersistentVisible(true);
            SetPlayerControlsLocked(false);
            return;
        }

        page.Show();
        currentPageId = pageId;
        SetPersistentVisible(page.KeepPersistentHudVisible);
        SetPlayerControlsLocked(ShouldLockPlayerControls(pageId));
        RefreshPageHelpers();
        PlayPageAudio(pageId);
        pageShown.Invoke(currentPageId);
    }

    private void OnDisable()
    {
        SetPlayerControlsLocked(false);
    }

    public void HideOverlay()
    {
        ShowPage(HearthFirstPersonHudPageId.Slide01PersistentHud);
        PlayOneShot(closeMenuClip);
    }

    public void OpenMainMenu()
    {
        menuSelectionIndex = Mathf.Clamp(menuSelectionIndex, 0, 2);
        ShowPage(HearthFirstPersonHudPageId.Slide03MainMenu);
        RefreshMenuFocus();
        PlayOneShot(openMenuClip);
    }

    public void MoveMenuFocus(int direction)
    {
        if (currentPageId != HearthFirstPersonHudPageId.Slide03MainMenu)
        {
            return;
        }

        menuSelectionIndex = Wrap(menuSelectionIndex + direction, 3);
        RefreshMenuFocus();
        PlayOneShot(focusMovedClip);
    }

    public void ConfirmMenuSelection()
    {
        if (currentPageId != HearthFirstPersonHudPageId.Slide03MainMenu)
        {
            return;
        }

        PlayOneShot(confirmClip);
        if (menuSelectionIndex == 0)
        {
            OpenTodayRounds();
        }
        else if (menuSelectionIndex == 1)
        {
            OpenDispositionHistory();
        }
        else
        {
            OpenSettings();
        }
    }

    public void ConfirmSyncTerminal()
    {
        SetRoundsProgress(0, totalRounds);
        PlayOneShot(confirmClip);
        onSyncConfirmed.Invoke();
        HideOverlay();
    }

    public void OpenTodayRounds()
    {
        ShowPage(HearthFirstPersonHudPageId.Slide05TodayRounds);
    }

    public void OpenDispositionHistory()
    {
        int count = dispositionHistoryView != null ? dispositionHistoryView.RecordCount : 0;
        count = Mathf.Clamp(count, 0, 3);
        ShowPage((HearthFirstPersonHudPageId)((int)HearthFirstPersonHudPageId.Slide18HistoryEmpty + count));
    }

    public void OpenSettings()
    {
        if (settingsView != null)
        {
            settingsView.ResetFocus();
        }

        ShowPage(HearthFirstPersonHudPageId.Slide22Settings);
    }

    public void OpenExitConfirm()
    {
        ShowPage(HearthFirstPersonHudPageId.Slide24ExitConfirm);
    }

    public void ShowFinalChoice(bool returnState)
    {
        finalChoiceSelectionIndex = 0;
        ShowPage(returnState ? HearthFirstPersonHudPageId.Slide14FinalChoiceReturn : HearthFirstPersonHudPageId.Slide09FinalChoice);
        RefreshFinalChoiceFocus();
    }

    public void MoveFinalChoiceFocus(int direction)
    {
        if (!IsFinalChoicePage(currentPageId))
        {
            return;
        }

        finalChoiceSelectionIndex = Wrap(finalChoiceSelectionIndex + direction, 2);
        RefreshFinalChoiceFocus();
        PlayOneShot(focusMovedClip);
    }

    public void ConfirmFocusedFinalChoice()
    {
        if (!IsFinalChoicePage(currentPageId))
        {
            return;
        }

        if (finalChoiceSelectionIndex == 0)
        {
            ChooseFinalA();
        }
        else
        {
            ChooseFinalB();
        }
    }

    public void ChooseFinalA()
    {
        PlayOneShot(confirmClip);
        onFinalChoiceA.Invoke();

        if (trustScore >= finalChoiceTrustThreshold)
        {
            ShowPage(HearthFirstPersonHudPageId.Slide10ShutdownConfirm);
        }
        else
        {
            SetHudState(HearthFirstPersonHudState.AlertPendingReview);
            ShowPage(HearthFirstPersonHudPageId.Slide11Warning01);
        }
    }

    public void ChooseFinalB()
    {
        PlayOneShot(confirmClip);
        onFinalChoiceB.Invoke();
        ShowEnding(HearthFirstPersonEndingPath.CompanionB);
    }

    public void ConfirmGracefulShutdown()
    {
        PlayOneShot(confirmClip);
        onGracefulShutdownConfirmed.Invoke();
        ShowEnding(HearthFirstPersonEndingPath.GracefulA);
    }

    public void CancelShutdownDecision()
    {
        PlayOneShot(cancelClip);
        ShowFinalChoice(true);
    }

    public void ContinueWarning()
    {
        PlayOneShot(warningClip);

        if (currentPageId == HearthFirstPersonHudPageId.Slide11Warning01)
        {
            ShowPage(HearthFirstPersonHudPageId.Slide12Warning02);
        }
        else if (currentPageId == HearthFirstPersonHudPageId.Slide12Warning02)
        {
            SetHudState(HearthFirstPersonHudState.AlertHighRisk);
            ShowPage(HearthFirstPersonHudPageId.Slide13Warning03);
        }
        else if (currentPageId == HearthFirstPersonHudPageId.Slide13Warning03)
        {
            onForcedShutdownConfirmed.Invoke();
            ShowEnding(HearthFirstPersonEndingPath.ForcedA);
        }
    }

    public void CancelWarning()
    {
        PlayOneShot(cancelClip);
        ShowFinalChoice(true);
    }

    public void ShowEnding(HearthFirstPersonEndingPath path)
    {
        switch (path)
        {
            case HearthFirstPersonEndingPath.GracefulA:
                ShowPage(HearthFirstPersonHudPageId.Slide15EndingGraceful);
                break;
            case HearthFirstPersonEndingPath.ForcedA:
                ShowPage(HearthFirstPersonHudPageId.Slide16EndingForced);
                break;
            case HearthFirstPersonEndingPath.CompanionB:
                ShowPage(HearthFirstPersonHudPageId.Slide17EndingCompanion);
                break;
        }

        endingShown.Invoke(path);
    }

    public void ConfirmExitGame()
    {
        PlayOneShot(confirmClip);
        onExitConfirmed.Invoke();
    }

    public void CancelExitGame()
    {
        PlayOneShot(cancelClip);
        onExitCancelled.Invoke();
        OpenSettings();
    }

    public void HandleSubmit()
    {
        switch (currentPageId)
        {
            case HearthFirstPersonHudPageId.Slide03MainMenu:
                ConfirmMenuSelection();
                break;
            case HearthFirstPersonHudPageId.Slide04SyncTerminal:
                ConfirmSyncTerminal();
                break;
            case HearthFirstPersonHudPageId.Slide09FinalChoice:
            case HearthFirstPersonHudPageId.Slide14FinalChoiceReturn:
                ConfirmFocusedFinalChoice();
                break;
            case HearthFirstPersonHudPageId.Slide10ShutdownConfirm:
                ConfirmGracefulShutdown();
                break;
            case HearthFirstPersonHudPageId.Slide11Warning01:
            case HearthFirstPersonHudPageId.Slide12Warning02:
            case HearthFirstPersonHudPageId.Slide13Warning03:
                ContinueWarning();
                break;
            case HearthFirstPersonHudPageId.Slide22Settings:
            case HearthFirstPersonHudPageId.Slide23SettingsFocus:
                if (settingsView != null)
                {
                    settingsView.ConfirmFocusedItem();
                }
                break;
            case HearthFirstPersonHudPageId.Slide24ExitConfirm:
                ConfirmExitGame();
                break;
        }
    }

    public void HandleCancel()
    {
        switch (currentPageId)
        {
            case HearthFirstPersonHudPageId.Slide10ShutdownConfirm:
                CancelShutdownDecision();
                break;
            case HearthFirstPersonHudPageId.Slide11Warning01:
            case HearthFirstPersonHudPageId.Slide12Warning02:
            case HearthFirstPersonHudPageId.Slide13Warning03:
                CancelWarning();
                break;
            case HearthFirstPersonHudPageId.Slide24ExitConfirm:
                CancelExitGame();
                break;
            case HearthFirstPersonHudPageId.Slide15EndingGraceful:
            case HearthFirstPersonHudPageId.Slide16EndingForced:
            case HearthFirstPersonHudPageId.Slide17EndingCompanion:
                break;
            default:
                HideOverlay();
                break;
        }
    }

    public void CloseStoryPopup()
    {
        if (currentPageId == HearthFirstPersonHudPageId.Slide06HomeWelcome ||
            currentPageId == HearthFirstPersonHudPageId.Slide07Photo2023 ||
            currentPageId == HearthFirstPersonHudPageId.Slide08Photo2026)
        {
            HideOverlay();
        }
    }

    public void SetHudState(HearthFirstPersonHudState state)
    {
        if (statusText != null)
        {
            switch (state)
            {
                case HearthFirstPersonHudState.Active:
                    statusText.text = "ACTIVE";
                    break;
                case HearthFirstPersonHudState.Dormant:
                    statusText.text = "DORMANT";
                    break;
                case HearthFirstPersonHudState.AlertPendingReview:
                    statusText.text = "ALERT · PENDING REVIEW";
                    break;
                case HearthFirstPersonHudState.AlertHighRisk:
                    statusText.text = "ALERT · HIGH-RISK";
                    break;
            }
        }

        if (statusDotImage != null)
        {
            switch (state)
            {
                case HearthFirstPersonHudState.Active:
                    statusDotImage.color = new Color(0.1f, 0.95f, 0.65f, 0.95f);
                    break;
                case HearthFirstPersonHudState.Dormant:
                    statusDotImage.color = new Color(0.45f, 0.5f, 0.55f, 0.85f);
                    break;
                default:
                    statusDotImage.color = new Color(1f, 0.25f, 0.18f, 0.95f);
                    break;
            }
        }
    }

    public void SetClock(string label)
    {
        if (clockText != null)
        {
            clockText.text = label;
        }
    }

    public void SetRoundsProgress(int completed, int total)
    {
        completedRounds = Mathf.Max(0, completed);
        totalRounds = Mathf.Max(1, total);

        if (taskText != null)
        {
            taskText.text = "CURRENT TASK\nNIGHT ROUNDS · BLOCK A · 17F\n" + completedRounds + "/" + totalRounds;
        }
    }

    public void SetTrustScore(int value)
    {
        trustScore = value;
        if (dispositionHistoryView != null)
        {
            dispositionHistoryView.SetCurrentTrustScore(trustScore);
        }
    }

    public void RecordDisposition(MinLoopDispositionChoice choice)
    {
        int delta = choice == MinLoopDispositionChoice.SystemRecommendedA ? 1 : -1;
        RecordDisposition(choice, trustScore + delta, delta);
    }

    public void RecordDisposition(MinLoopDispositionChoice choice, int currentTrustAfter, int trustDelta)
    {
        string unitId = "17F-" + Mathf.Clamp((dispositionHistoryView != null ? dispositionHistoryView.RecordCount : completedRounds) + 1, 1, 3).ToString("00");
        string actionLabel = choice == MinLoopDispositionChoice.SystemRecommendedA
            ? "Approve Upgrade · Deep Night Companion Pro"
            : "Recommend Family Counseling · Pause unit";
        string statusLabel = choice == MinLoopDispositionChoice.SystemRecommendedA ? "RECOMMENDED" : string.Empty;
        RecordDisposition(unitId, actionLabel, statusLabel, trustDelta, currentTrustAfter);
    }

    public void RecordDisposition(string unitId, string actionLabel, string statusLabel, int trustDelta)
    {
        RecordDisposition(unitId, actionLabel, statusLabel, trustDelta, trustScore + trustDelta);
    }

    public void RecordDisposition(string unitId, string actionLabel, string statusLabel, int trustDelta, int currentTrustAfter)
    {
        trustScore = currentTrustAfter;
        completedRounds = Mathf.Clamp(completedRounds + 1, 0, totalRounds);
        SetRoundsProgress(completedRounds, totalRounds);

        if (dispositionHistoryView != null)
        {
            HearthDispositionRecord record = new HearthDispositionRecord();
            record.unitId = unitId;
            record.actionLabel = actionLabel;
            record.statusLabel = statusLabel;
            record.trustDelta = trustDelta;
            dispositionHistoryView.AddRecord(record, currentTrustAfter);
        }

        ShowTrustDelta(trustDelta);
    }

    public void ShowTrustDelta(int delta)
    {
        if (trustDeltaText != null)
        {
            trustDeltaText.text = (delta >= 0 ? "+" : string.Empty) + delta + " TRUST";
        }

        trustDeltaShown.Invoke(delta);
        PlayOneShot(trustDeltaClip);

        if (trustDeltaRoutine != null)
        {
            StopCoroutine(trustDeltaRoutine);
        }

        trustDeltaRoutine = StartCoroutine(TrustDeltaRoutine());
    }

    public void PlayConfirmSound()
    {
        PlayOneShot(confirmClip);
    }

    public void PlayCancelSound()
    {
        PlayOneShot(cancelClip);
    }

    private IEnumerator TrustDeltaRoutine()
    {
        if (trustDeltaCanvasGroup == null)
        {
            yield break;
        }

        float fadeIn = 0.25f;
        float fadeOut = 0.45f;
        float hold = Mathf.Max(0f, trustDeltaSeconds - fadeIn - fadeOut);

        trustDeltaCanvasGroup.gameObject.SetActive(true);

        float t = 0f;
        while (t < fadeIn)
        {
            t += Time.unscaledDeltaTime;
            trustDeltaCanvasGroup.alpha = Mathf.Clamp01(t / fadeIn);
            yield return null;
        }

        trustDeltaCanvasGroup.alpha = 1f;
        yield return new WaitForSecondsRealtime(hold);

        t = 0f;
        while (t < fadeOut)
        {
            t += Time.unscaledDeltaTime;
            trustDeltaCanvasGroup.alpha = 1f - Mathf.Clamp01(t / fadeOut);
            yield return null;
        }

        HideTrustDeltaImmediate();
        trustDeltaRoutine = null;
    }

    private void BuildPageMap()
    {
        pageMap.Clear();
        if (pages == null)
        {
            return;
        }

        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null && pages[i].PageId != HearthFirstPersonHudPageId.None)
            {
                pageMap[pages[i].PageId] = pages[i];
            }
        }
    }

    private void HideAllPages()
    {
        if (pages == null)
        {
            return;
        }

        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
            {
                pages[i].Hide();
            }
        }
    }

    private void SetPersistentVisible(bool visible)
    {
        if (persistentHudRoot != null)
        {
            persistentHudRoot.SetActive(visible);
        }

        if (persistentHudCanvasGroup != null)
        {
            persistentHudCanvasGroup.alpha = visible ? 1f : 0f;
            persistentHudCanvasGroup.interactable = false;
            persistentHudCanvasGroup.blocksRaycasts = false;
        }
    }

    private void RefreshPageHelpers()
    {
        HideFocusRects();

        if (currentPageId == HearthFirstPersonHudPageId.Slide03MainMenu)
        {
            RefreshMenuFocus();
        }
        else if (IsFinalChoicePage(currentPageId))
        {
            RefreshFinalChoiceFocus();
        }
        else if (currentPageId == HearthFirstPersonHudPageId.Slide22Settings ||
                 currentPageId == HearthFirstPersonHudPageId.Slide23SettingsFocus)
        {
            if (settingsView != null)
            {
                settingsView.SetFocusVisible(true);
            }
        }
    }

    private void RefreshMenuFocus()
    {
        SetFocusRect(menuFocusRect, menuFocusTargets, menuSelectionIndex, menuFocusPadding);
    }

    private void RefreshFinalChoiceFocus()
    {
        SetFocusRect(finalChoiceFocusRect, finalChoiceFocusTargets, finalChoiceSelectionIndex, finalChoiceFocusPadding);
    }

    private void SetFocusRect(RectTransform focusRect, RectTransform[] targets, int index, Vector2 padding)
    {
        if (focusRect == null || targets == null || index < 0 || index >= targets.Length || targets[index] == null)
        {
            if (focusRect != null)
            {
                focusRect.gameObject.SetActive(false);
            }

            return;
        }

        RectTransform target = targets[index];
        focusRect.gameObject.SetActive(true);
        focusRect.anchorMin = target.anchorMin;
        focusRect.anchorMax = target.anchorMax;
        focusRect.pivot = target.pivot;
        focusRect.anchoredPosition = target.anchoredPosition;
        focusRect.sizeDelta = target.sizeDelta + padding;
    }

    private void HideFocusRects()
    {
        if (menuFocusRect != null)
        {
            menuFocusRect.gameObject.SetActive(false);
        }

        if (finalChoiceFocusRect != null)
        {
            finalChoiceFocusRect.gameObject.SetActive(false);
        }

        if (settingsView != null)
        {
            settingsView.SetFocusVisible(false);
        }
    }

    private void HideTrustDeltaImmediate()
    {
        if (trustDeltaCanvasGroup != null)
        {
            trustDeltaCanvasGroup.alpha = 0f;
            trustDeltaCanvasGroup.gameObject.SetActive(false);
        }
    }

    private void ResolvePlayerControlLock()
    {
        if (playerControlLock == null)
        {
            playerControlLock = GetComponent<HearthPlayerControlLock>();
        }

        if (playerControlLock == null)
        {
            playerControlLock = FindObjectOfType<HearthPlayerControlLock>();
        }
    }

    private bool ShouldLockPlayerControls(HearthFirstPersonHudPageId pageId)
    {
        return lockPlayerControlsWhileOverlayOpen &&
               pageId != HearthFirstPersonHudPageId.None &&
               pageId != HearthFirstPersonHudPageId.Slide01PersistentHud &&
               pageId != HearthFirstPersonHudPageId.Slide02TrustDelta;
    }

    private void SetPlayerControlsLocked(bool locked)
    {
        if (!lockPlayerControlsWhileOverlayOpen && locked)
        {
            return;
        }

        ResolvePlayerControlLock();
        if (playerControlLock != null)
        {
            playerControlLock.SetControlsLocked(locked);
        }
    }

    private void PlayPageAudio(HearthFirstPersonHudPageId pageId)
    {
        if (pageId == HearthFirstPersonHudPageId.Slide11Warning01 ||
            pageId == HearthFirstPersonHudPageId.Slide12Warning02 ||
            pageId == HearthFirstPersonHudPageId.Slide13Warning03)
        {
            PlayOneShot(warningClip);
            return;
        }

        PlayOneShot(pageChangedClip);
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private static int Wrap(int value, int count)
    {
        if (count <= 0)
        {
            return 0;
        }

        while (value < 0)
        {
            value += count;
        }

        return value % count;
    }

    private static bool IsFinalChoicePage(HearthFirstPersonHudPageId pageId)
    {
        return pageId == HearthFirstPersonHudPageId.Slide09FinalChoice ||
               pageId == HearthFirstPersonHudPageId.Slide14FinalChoiceReturn;
    }
}
