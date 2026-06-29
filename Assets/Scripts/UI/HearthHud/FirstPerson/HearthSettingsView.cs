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
        Refresh();
        RefreshFocus();
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
        Refresh();
        volumeChanged.Invoke("Master", masterVolume);
    }

    public void SetDialogueVolume(int value)
    {
        dialogueVolume = ClampVolume(value);
        Refresh();
        volumeChanged.Invoke("Dialogue", dialogueVolume);
    }

    public void SetAmbientVolume(int value)
    {
        ambientVolume = ClampVolume(value);
        Refresh();
        volumeChanged.Invoke("Ambient", ambientVolume);
    }

    public void SetSfxVolume(int value)
    {
        sfxVolume = ClampVolume(value);
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
        SetOptionalText(masterVolumeText, masterVolume);
        SetOptionalText(dialogueVolumeText, dialogueVolume);
        SetOptionalText(ambientVolumeText, ambientVolume);
        SetOptionalText(sfxVolumeText, sfxVolume);
        SetOptionalSlider(masterVolumeSlider, masterVolume);
        SetOptionalSlider(dialogueVolumeSlider, dialogueVolume);
        SetOptionalSlider(ambientVolumeSlider, ambientVolume);
        SetOptionalSlider(sfxVolumeSlider, sfxVolume);
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
