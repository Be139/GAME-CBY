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
        ApplyVolume();
    }

    private void OnEnable()
    {
        if (!ActiveSources.Contains(this))
        {
            ActiveSources.Add(this);
        }

        ApplyVolume();
    }

    private void OnDisable()
    {
        ActiveSources.Remove(this);
    }

    private void OnValidate()
    {
        baseVolume = Mathf.Clamp01(baseVolume);
        ResolveSource();

        if (!Application.isPlaying)
        {
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

    public void ApplyVolume()
    {
        ResolveSource();
        if (audioSource == null)
        {
            return;
        }

        HearthAudioSettingsController settings = HearthAudioSettingsController.Resolve();
        float channelScale = settings != null ? settings.GetLinearVolume(channel) : 1f;
        audioSource.volume = baseVolume * channelScale;
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
}
