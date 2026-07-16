using System;
using UnityEngine;
using UnityEngine.Events;

public enum HearthAudioChannel
{
    Master,
    Dialogue,
    Ambient,
    SFX
}

[Serializable]
public class HearthAudioVolumeChangedEvent : UnityEvent<HearthAudioChannel, int>
{
}

[DisallowMultipleComponent]
public class HearthAudioSettingsController : MonoBehaviour
{
    private const string PlayerPrefsPrefix = "HEARTH.Audio.";

    [Header("Default Volumes")]
    [Range(0, 100)] [SerializeField] private int defaultMasterVolume = 80;
    [Range(0, 100)] [SerializeField] private int defaultDialogueVolume = 100;
    [Range(0, 100)] [SerializeField] private int defaultAmbientVolume = 70;
    [Range(0, 100)] [SerializeField] private int defaultSfxVolume = 80;

    [Header("Persistence")]
    [SerializeField] private bool loadSavedValuesOnAwake = true;
    [SerializeField] private bool saveChangesImmediately = true;

    [Header("Runtime Values")]
    [Range(0, 100)] [SerializeField] private int masterVolume = 80;
    [Range(0, 100)] [SerializeField] private int dialogueVolume = 100;
    [Range(0, 100)] [SerializeField] private int ambientVolume = 70;
    [Range(0, 100)] [SerializeField] private int sfxVolume = 80;

    [Header("Events")]
    [SerializeField] private HearthAudioVolumeChangedEvent volumeChanged = new HearthAudioVolumeChangedEvent();

    public static HearthAudioSettingsController Instance { get; private set; }

    public event Action<HearthAudioChannel, int> VolumeChanged;

    public HearthAudioVolumeChangedEvent OnVolumeChanged
    {
        get { return volumeChanged; }
    }

    private void Awake()
    {
        if (Instance == null || Instance == this)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("[HearthAudioSettingsController] Multiple audio settings controllers found. The first active controller remains authoritative.", this);
        }

        if (loadSavedValuesOnAwake)
        {
            LoadSavedValues();
        }
        else
        {
            ClampValues();
        }

        ApplyAll();
    }

    private void OnEnable()
    {
        ApplyAll();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void OnValidate()
    {
        defaultMasterVolume = Clamp(defaultMasterVolume);
        defaultDialogueVolume = Clamp(defaultDialogueVolume);
        defaultAmbientVolume = Clamp(defaultAmbientVolume);
        defaultSfxVolume = Clamp(defaultSfxVolume);
        ClampValues();
    }

    public int GetVolume(HearthAudioChannel channel)
    {
        switch (channel)
        {
            case HearthAudioChannel.Dialogue:
                return dialogueVolume;
            case HearthAudioChannel.Ambient:
                return ambientVolume;
            case HearthAudioChannel.SFX:
                return sfxVolume;
            default:
                return masterVolume;
        }
    }

    public float GetLinearVolume(HearthAudioChannel channel)
    {
        return GetVolume(channel) / 100f;
    }

    public void SetVolume(HearthAudioChannel channel, int value)
    {
        value = Clamp(value);
        bool changed = GetVolume(channel) != value;

        switch (channel)
        {
            case HearthAudioChannel.Dialogue:
                dialogueVolume = value;
                break;
            case HearthAudioChannel.Ambient:
                ambientVolume = value;
                break;
            case HearthAudioChannel.SFX:
                sfxVolume = value;
                break;
            default:
                masterVolume = value;
                break;
        }

        ApplyAll();

        if (saveChangesImmediately)
        {
            SaveValues();
        }

        if (changed)
        {
            volumeChanged.Invoke(channel, value);
            if (VolumeChanged != null)
            {
                VolumeChanged(channel, value);
            }
        }
    }

    public void SetVolume(string channel, int value)
    {
        HearthAudioChannel parsed;
        if (!Enum.TryParse(channel, true, out parsed))
        {
            parsed = HearthAudioChannel.Master;
        }

        SetVolume(parsed, value);
    }

    public void ResetToDefaults()
    {
        masterVolume = defaultMasterVolume;
        dialogueVolume = defaultDialogueVolume;
        ambientVolume = defaultAmbientVolume;
        sfxVolume = defaultSfxVolume;
        ApplyAll();
        SaveValues();

        volumeChanged.Invoke(HearthAudioChannel.Master, masterVolume);
        volumeChanged.Invoke(HearthAudioChannel.Dialogue, dialogueVolume);
        volumeChanged.Invoke(HearthAudioChannel.Ambient, ambientVolume);
        volumeChanged.Invoke(HearthAudioChannel.SFX, sfxVolume);
    }

    public void SaveValues()
    {
        PlayerPrefs.SetInt(PlayerPrefsPrefix + HearthAudioChannel.Master, masterVolume);
        PlayerPrefs.SetInt(PlayerPrefsPrefix + HearthAudioChannel.Dialogue, dialogueVolume);
        PlayerPrefs.SetInt(PlayerPrefsPrefix + HearthAudioChannel.Ambient, ambientVolume);
        PlayerPrefs.SetInt(PlayerPrefsPrefix + HearthAudioChannel.SFX, sfxVolume);
        PlayerPrefs.Save();
    }

    public void LoadSavedValues()
    {
        masterVolume = PlayerPrefs.GetInt(PlayerPrefsPrefix + HearthAudioChannel.Master, defaultMasterVolume);
        dialogueVolume = PlayerPrefs.GetInt(PlayerPrefsPrefix + HearthAudioChannel.Dialogue, defaultDialogueVolume);
        ambientVolume = PlayerPrefs.GetInt(PlayerPrefsPrefix + HearthAudioChannel.Ambient, defaultAmbientVolume);
        sfxVolume = PlayerPrefs.GetInt(PlayerPrefsPrefix + HearthAudioChannel.SFX, defaultSfxVolume);
        ClampValues();
        ApplyAll();
    }

    public void ApplyAll()
    {
        AudioListener.volume = masterVolume / 100f;
        HearthAudioChannelSource.RefreshAll();
    }

    public static HearthAudioSettingsController Resolve()
    {
        if (Instance == null)
        {
            Instance = FindObjectOfType<HearthAudioSettingsController>();
        }

        return Instance;
    }

    private void ClampValues()
    {
        masterVolume = Clamp(masterVolume);
        dialogueVolume = Clamp(dialogueVolume);
        ambientVolume = Clamp(ambientVolume);
        sfxVolume = Clamp(sfxVolume);
    }

    private static int Clamp(int value)
    {
        return Mathf.Clamp(value, 0, 100);
    }
}
