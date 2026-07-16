using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HearthSettingsView : MonoBehaviour
{
    private const int ExitGameIndex = 4;

    [Header("Values")]
    [Range(0, 100)]
    [SerializeField] private int masterVolume = 80;
    [Range(0, 100)]
    [SerializeField] private int dialogueVolume = 100;
    [Range(0, 100)]
    [SerializeField] private int ambientVolume = 70;
    [Range(0, 100)]
    [SerializeField] private int sfxVolume = 80;

    [Header("Focus")]
    [SerializeField] private int focusIndex;
    [SerializeField] private RectTransform focusRect;
    [SerializeField] private RectTransform[] focusTargets;
    [SerializeField] private Vector2 focusPadding = new Vector2(8f, 4f);

    [Header("Optional UI Bindings")]
    [SerializeField] private TMP_Text masterVolumeText;
    [SerializeField] private TMP_Text dialogueVolumeText;
    [SerializeField] private TMP_Text ambientVolumeText;
    [SerializeField] private TMP_Text sfxVolumeText;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider dialogueVolumeSlider;
    [SerializeField] private Slider ambientVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Additional Value Texts")]
    [SerializeField] private TMP_Text[] masterVolumeTexts;
    [SerializeField] private TMP_Text[] dialogueVolumeTexts;
    [SerializeField] private TMP_Text[] ambientVolumeTexts;
    [SerializeField] private TMP_Text[] sfxVolumeTexts;

    [Header("Audio Settings")]
    [SerializeField] private HearthAudioSettingsController audioSettings;
    [SerializeField] private bool autoFindAudioSettings = true;
    [SerializeField] private bool autoResolveValueTexts = true;

    [Header("Events")]
    [SerializeField] private HearthSettingsVolumeEvent volumeChanged = new HearthSettingsVolumeEvent();
    [SerializeField] private UnityEvent onExitRequested = new UnityEvent();

    public int FocusIndex
    {
        get { return focusIndex; }
    }

    public UnityEvent OnExitRequested
    {
        get { return onExitRequested; }
    }

    public HearthSettingsVolumeEvent VolumeChanged
    {
        get { return volumeChanged; }
    }

    private void Awake()
    {
        ResolveAudioSettings();
        ResolveValueTexts();
        SyncFromAudioSettings();
        Refresh();
        RefreshFocus();
    }

    private void Start()
    {
        SyncFromAudioSettings();
        Refresh();
    }

    public void ResetFocus()
    {
        focusIndex = 0;
        RefreshFocus();
    }

    public void MoveFocus(int direction)
    {
        int count = Mathf.Max(1, focusTargets != null ? focusTargets.Length : 5);
        focusIndex = Wrap(focusIndex + direction, count);
        RefreshFocus();
    }

    public void AdjustFocusedVolume(int delta)
    {
        if (focusIndex == 0)
        {
            SetMasterVolume(masterVolume + delta);
        }
        else if (focusIndex == 1)
        {
            SetDialogueVolume(dialogueVolume + delta);
        }
        else if (focusIndex == 2)
        {
            SetAmbientVolume(ambientVolume + delta);
        }
        else if (focusIndex == 3)
        {
            SetSfxVolume(sfxVolume + delta);
        }
    }

    public void ConfirmFocusedItem()
    {
        if (focusIndex == ExitGameIndex)
        {
            onExitRequested.Invoke();
        }
    }

    public void SetFocusVisible(bool visible)
    {
        if (focusRect != null)
        {
            focusRect.gameObject.SetActive(visible);
            if (visible)
            {
                RefreshFocus();
            }
        }
    }

    public void SetMasterVolume(int value)
    {
        masterVolume = ClampVolume(value);
        ApplyVolume(HearthAudioChannel.Master, masterVolume);
        Refresh();
        volumeChanged.Invoke("Master", masterVolume);
    }

    public void SetDialogueVolume(int value)
    {
        dialogueVolume = ClampVolume(value);
        ApplyVolume(HearthAudioChannel.Dialogue, dialogueVolume);
        Refresh();
        volumeChanged.Invoke("Dialogue", dialogueVolume);
    }

    public void SetAmbientVolume(int value)
    {
        ambientVolume = ClampVolume(value);
        ApplyVolume(HearthAudioChannel.Ambient, ambientVolume);
        Refresh();
        volumeChanged.Invoke("Ambient", ambientVolume);
    }

    public void SetSfxVolume(int value)
    {
        sfxVolume = ClampVolume(value);
        ApplyVolume(HearthAudioChannel.SFX, sfxVolume);
        Refresh();
        volumeChanged.Invoke("SFX", sfxVolume);
    }

    public int GetVolume(string channel)
    {
        if (channel == "Dialogue")
        {
            return dialogueVolume;
        }

        if (channel == "Ambient")
        {
            return ambientVolume;
        }

        if (channel == "SFX")
        {
            return sfxVolume;
        }

        return masterVolume;
    }

    public void Refresh()
    {
        ResolveValueTexts();
        SetOptionalText(masterVolumeText, masterVolume);
        SetOptionalText(dialogueVolumeText, dialogueVolume);
        SetOptionalText(ambientVolumeText, ambientVolume);
        SetOptionalText(sfxVolumeText, sfxVolume);
        SetAllTexts(masterVolumeTexts, masterVolume);
        SetAllTexts(dialogueVolumeTexts, dialogueVolume);
        SetAllTexts(ambientVolumeTexts, ambientVolume);
        SetAllTexts(sfxVolumeTexts, sfxVolume);
        SetOptionalSlider(masterVolumeSlider, masterVolume);
        SetOptionalSlider(dialogueVolumeSlider, dialogueVolume);
        SetOptionalSlider(ambientVolumeSlider, ambientVolume);
        SetOptionalSlider(sfxVolumeSlider, sfxVolume);
    }

    public void RefreshFromAudioSettings()
    {
        ResolveAudioSettings();
        SyncFromAudioSettings();
        Refresh();
    }

    public void SetAudioSettingsController(HearthAudioSettingsController controller)
    {
        audioSettings = controller;
        RefreshFromAudioSettings();
    }

    private void RefreshFocus()
    {
        if (focusRect == null || focusTargets == null || focusIndex < 0 || focusIndex >= focusTargets.Length || focusTargets[focusIndex] == null)
        {
            return;
        }

        RectTransform target = focusTargets[focusIndex];
        focusRect.anchorMin = target.anchorMin;
        focusRect.anchorMax = target.anchorMax;
        focusRect.pivot = target.pivot;
        focusRect.anchoredPosition = target.anchoredPosition;
        focusRect.sizeDelta = target.sizeDelta + focusPadding;
    }

    private static int ClampVolume(int value)
    {
        return Mathf.Clamp(value, 0, 100);
    }

    private static void SetOptionalText(TMP_Text text, int value)
    {
        if (text != null)
        {
            text.text = value.ToString();
        }
    }

    private static void SetOptionalSlider(Slider slider, int value)
    {
        if (slider != null)
        {
            slider.minValue = 0;
            slider.maxValue = 100;
            slider.value = value;
        }
    }

    private void ResolveAudioSettings()
    {
        if (audioSettings != null || !autoFindAudioSettings)
        {
            return;
        }

        audioSettings = GetComponent<HearthAudioSettingsController>();
        if (audioSettings == null)
        {
            audioSettings = GetComponentInParent<HearthAudioSettingsController>();
        }

        if (audioSettings == null)
        {
            audioSettings = FindObjectOfType<HearthAudioSettingsController>();
        }
    }

    private void SyncFromAudioSettings()
    {
        if (audioSettings == null)
        {
            return;
        }

        masterVolume = audioSettings.GetVolume(HearthAudioChannel.Master);
        dialogueVolume = audioSettings.GetVolume(HearthAudioChannel.Dialogue);
        ambientVolume = audioSettings.GetVolume(HearthAudioChannel.Ambient);
        sfxVolume = audioSettings.GetVolume(HearthAudioChannel.SFX);
    }

    private void ApplyVolume(HearthAudioChannel channel, int value)
    {
        ResolveAudioSettings();
        if (audioSettings != null)
        {
            audioSettings.SetVolume(channel, value);
        }
        else if (channel == HearthAudioChannel.Master)
        {
            AudioListener.volume = value / 100f;
        }
    }

    private void ResolveValueTexts()
    {
        if (!autoResolveValueTexts)
        {
            return;
        }

        TMP_Text[] allTexts = GetComponentsInChildren<TMP_Text>(true);
        if (masterVolumeTexts == null || masterVolumeTexts.Length == 0)
        {
            masterVolumeTexts = FindValueTexts(allTexts, "Master Volume");
        }

        if (dialogueVolumeTexts == null || dialogueVolumeTexts.Length == 0)
        {
            dialogueVolumeTexts = FindValueTexts(allTexts, "Dialogue Volume");
        }

        if (ambientVolumeTexts == null || ambientVolumeTexts.Length == 0)
        {
            ambientVolumeTexts = FindValueTexts(allTexts, "Ambient Volume");
        }

        if (sfxVolumeTexts == null || sfxVolumeTexts.Length == 0)
        {
            sfxVolumeTexts = FindValueTexts(allTexts, "SFX Volume");
        }
    }

    private static TMP_Text[] FindValueTexts(TMP_Text[] allTexts, string label)
    {
        List<TMP_Text> values = new List<TMP_Text>();
        if (allTexts == null)
        {
            return values.ToArray();
        }

        for (int i = 0; i < allTexts.Length; i++)
        {
            TMP_Text labelText = allTexts[i];
            if (labelText == null || !string.Equals(labelText.text, label, System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            HearthFirstPersonHudPage page = labelText.GetComponentInParent<HearthFirstPersonHudPage>(true);
            if (page == null)
            {
                continue;
            }

            TMP_Text best = null;
            float bestDistance = float.MaxValue;
            Vector2 labelPosition = labelText.rectTransform.anchoredPosition;

            for (int j = 0; j < allTexts.Length; j++)
            {
                TMP_Text candidate = allTexts[j];
                int parsedValue;
                if (candidate == null || candidate == labelText ||
                    candidate.GetComponentInParent<HearthFirstPersonHudPage>(true) != page ||
                    !int.TryParse(candidate.text, out parsedValue))
                {
                    continue;
                }

                Vector2 candidatePosition = candidate.rectTransform.anchoredPosition;
                if (candidatePosition.x <= labelPosition.x || Mathf.Abs(candidatePosition.y - labelPosition.y) > 12f)
                {
                    continue;
                }

                float distance = Mathf.Abs(candidatePosition.x - labelPosition.x);
                if (distance < bestDistance)
                {
                    best = candidate;
                    bestDistance = distance;
                }
            }

            if (best != null && !values.Contains(best))
            {
                values.Add(best);
            }
        }

        return values.ToArray();
    }

    private static void SetAllTexts(TMP_Text[] texts, int value)
    {
        if (texts == null)
        {
            return;
        }

        for (int i = 0; i < texts.Length; i++)
        {
            SetOptionalText(texts[i], value);
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
}
