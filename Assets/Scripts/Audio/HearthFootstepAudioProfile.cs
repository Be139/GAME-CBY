using UnityEngine;

public enum HearthFootstepRole
{
    Human,
    Companion
}

[DisallowMultipleComponent]
public class HearthFootstepAudioProfile : MonoBehaviour
{
    [Header("Role")]
    [SerializeField] private HearthFootstepRole role = HearthFootstepRole.Human;

    [Header("Existing Controller Audio")]
    [SerializeField] private FirstPersonAudio firstPersonAudio;
    [SerializeField] private AudioSource walkSource;
    [SerializeField] private AudioSource runSource;

    [Header("Replaceable Clips")]
    [SerializeField] private AudioClip walkClip;
    [SerializeField] private AudioClip runClip;

    [Header("Playback Speed / Cadence")]
    [Tooltip("Changes playback speed and therefore the cadence of the current multi-step recording. 1 is the source speed; lower values are slower.")]
    [Range(0.5f, 1.5f)] [SerializeField] private float walkPlaybackSpeed = 1f;
    [Tooltip("Changes playback speed and therefore the cadence of the current multi-step recording. 1 is the source speed; lower values are slower.")]
    [Range(0.5f, 1.75f)] [SerializeField] private float runPlaybackSpeed = 1.3f;

    [Header("Volume Before SFX Bus")]
    [Range(0f, 1f)] [SerializeField] private float walkVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float runVolume = 1f;

    public HearthFootstepRole Role
    {
        get { return role; }
    }

    private void Awake()
    {
        ResolveReferences();
        Apply();
    }

    private void OnValidate()
    {
        walkPlaybackSpeed = Mathf.Clamp(walkPlaybackSpeed, 0.5f, 1.5f);
        runPlaybackSpeed = Mathf.Clamp(runPlaybackSpeed, 0.5f, 1.75f);
        walkVolume = Mathf.Clamp01(walkVolume);
        runVolume = Mathf.Clamp01(runVolume);
        ResolveReferences();

        if (!Application.isPlaying)
        {
            Apply();
        }
    }

    [ContextMenu("Apply Footstep Profile")]
    public void Apply()
    {
        ResolveReferences();
        ApplySource(walkSource, walkClip, walkPlaybackSpeed, walkVolume);
        ApplySource(runSource, runClip, runPlaybackSpeed, runVolume);
    }

    public void Configure(
        HearthFootstepRole newRole,
        FirstPersonAudio audio,
        AudioClip newWalkClip,
        AudioClip newRunClip,
        float newWalkPlaybackSpeed,
        float newRunPlaybackSpeed)
    {
        role = newRole;
        firstPersonAudio = audio;
        walkSource = audio != null ? audio.stepAudio : null;
        runSource = audio != null ? audio.runningAudio : null;
        walkClip = newWalkClip;
        runClip = newRunClip;
        walkPlaybackSpeed = newWalkPlaybackSpeed;
        runPlaybackSpeed = newRunPlaybackSpeed;
        Apply();
    }

    public void SetPlaybackSpeeds(float walkSpeed, float runSpeed)
    {
        walkPlaybackSpeed = walkSpeed;
        runPlaybackSpeed = runSpeed;
        Apply();
    }

    public void SetFootstepClips(AudioClip newWalkClip, AudioClip newRunClip)
    {
        walkClip = newWalkClip;
        runClip = newRunClip;
        Apply();
    }

    private void ResolveReferences()
    {
        if (firstPersonAudio == null)
        {
            firstPersonAudio = GetComponent<FirstPersonAudio>();
        }

        if (firstPersonAudio == null)
        {
            firstPersonAudio = GetComponentInParent<FirstPersonAudio>();
        }

        if (firstPersonAudio != null)
        {
            if (walkSource == null)
            {
                walkSource = firstPersonAudio.stepAudio;
            }

            if (runSource == null)
            {
                runSource = firstPersonAudio.runningAudio;
            }
        }
    }

    private static void ApplySource(AudioSource source, AudioClip clip, float playbackSpeed, float volume)
    {
        if (source == null)
        {
            return;
        }

        if (clip != null)
        {
            source.clip = clip;
        }

        source.pitch = playbackSpeed;

        HearthAudioChannelSource channelSource = source.GetComponent<HearthAudioChannelSource>();
        if (channelSource != null)
        {
            channelSource.SetBaseVolume(volume);
        }
        else
        {
            source.volume = volume;
        }
    }
}
