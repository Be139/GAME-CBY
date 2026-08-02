using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public class HearthAudioChannelSource : MonoBehaviour
{
    private static readonly List<HearthAudioChannelSource> ActiveSources = new List<HearthAudioChannelSource>();

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private HearthAudioChannel channel = HearthAudioChannel.SFX;
    [Range(0f, 1f)] [SerializeField] private float baseVolume = 1f;

    [Header("Dialogue Ducking")]
    [SerializeField] private bool duckWhileDialogue;
    [Tooltip("0.56 is approximately -5 dB, suitable for the quiet lobby walla under speech.")]
    [Range(0.1f, 1f)] [SerializeField] private float dialogueDuckScale = 0.56f;
    [Min(0.01f)] [SerializeField] private float duckAttackSeconds = 0.12f;
    [Min(0.01f)] [SerializeField] private float duckReleaseSeconds = 0.35f;

    private float currentDuckScale = 1f;

    public HearthAudioChannel Channel
    {
        get { return channel; }
    }

    public float BaseVolume
    {
        get { return baseVolume; }
    }

    private void Awake()
    {
        ResolveSource();
        currentDuckScale = ResolveTargetDuckScale();
        ApplyVolume();
    }

    private void OnEnable()
    {
        if (!ActiveSources.Contains(this))
        {
            ActiveSources.Add(this);
        }

        currentDuckScale = ResolveTargetDuckScale();
        ApplyVolume();
    }

    private void OnDisable()
    {
        ActiveSources.Remove(this);
    }

    private void OnValidate()
    {
        baseVolume = Mathf.Clamp01(baseVolume);
        dialogueDuckScale = Mathf.Clamp(dialogueDuckScale, 0.1f, 1f);
        duckAttackSeconds = Mathf.Max(0.01f, duckAttackSeconds);
        duckReleaseSeconds = Mathf.Max(0.01f, duckReleaseSeconds);
        ResolveSource();

        if (!Application.isPlaying)
        {
            ApplyVolume();
        }
    }

    private void Update()
    {
        float target = ResolveTargetDuckScale();
        float seconds = target < currentDuckScale
            ? duckAttackSeconds
            : duckReleaseSeconds;
        float next = Mathf.MoveTowards(
            currentDuckScale,
            target,
            Time.unscaledDeltaTime / Mathf.Max(0.01f, seconds));
        if (!Mathf.Approximately(next, currentDuckScale))
        {
            currentDuckScale = next;
            ApplyVolume();
        }
    }

    public void Configure(AudioSource source, HearthAudioChannel newChannel, float newBaseVolume)
    {
        audioSource = source != null ? source : GetComponent<AudioSource>();
        channel = newChannel == HearthAudioChannel.Master ? HearthAudioChannel.SFX : newChannel;
        baseVolume = Mathf.Clamp01(newBaseVolume);
        ApplyVolume();
    }

    public void SetChannel(HearthAudioChannel value)
    {
        channel = value == HearthAudioChannel.Master ? HearthAudioChannel.SFX : value;
        ApplyVolume();
    }

    public void SetBaseVolume(float value)
    {
        baseVolume = Mathf.Clamp01(value);
        ApplyVolume();
    }

    public void ConfigureDialogueDucking(
        bool shouldDuck,
        float duckScale = 0.56f,
        float attackSeconds = 0.12f,
        float releaseSeconds = 0.35f)
    {
        duckWhileDialogue = shouldDuck;
        dialogueDuckScale = Mathf.Clamp(duckScale, 0.1f, 1f);
        duckAttackSeconds = Mathf.Max(0.01f, attackSeconds);
        duckReleaseSeconds = Mathf.Max(0.01f, releaseSeconds);
        currentDuckScale = ResolveTargetDuckScale();
        ApplyVolume();
    }

    public void ApplyVolume()
    {
        ResolveSource();
        if (audioSource == null)
        {
            return;
        }

        HearthAudioSettingsController settings = HearthAudioSettingsController.Resolve();
        float channelScale = settings != null ? settings.GetLinearVolume(channel) : 1f;
        audioSource.volume = baseVolume * channelScale * currentDuckScale;
    }

    public static void RefreshAll()
    {
        for (int i = ActiveSources.Count - 1; i >= 0; i--)
        {
            HearthAudioChannelSource source = ActiveSources[i];
            if (source == null)
            {
                ActiveSources.RemoveAt(i);
                continue;
            }

            source.ApplyVolume();
        }
    }

    private void ResolveSource()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private float ResolveTargetDuckScale()
    {
        return duckWhileDialogue && MinLoopSubtitlePlayer.AnyDialoguePlaying
            ? dialogueDuckScale
            : 1f;
    }
}
