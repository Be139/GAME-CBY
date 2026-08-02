using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class HearthSfxCuePlayer : MonoBehaviour
{
    [Serializable]
    public class CueSlot
    {
        [Tooltip("Stable cue ID used by gameplay scripts. Keep this value unchanged after wiring a level.")]
        public string cueId;

        [Tooltip("Stable sound ID resolved through the shared catalog. Several gameplay cues may reuse one sound.")]
        public string soundId;

        [TextArea(2, 4)]
        public string placementNote;

        [Tooltip("True when the current level flow already triggers this cue. False means the source is reserved until its exact timing is confirmed.")]
        public bool automaticallyTriggered = true;

        [Header("Source And Clips")]
        public AudioSource source;
        public AudioClip primaryClip;
        public AudioClip[] alternateClips = Array.Empty<AudioClip>();
        public bool loop;
        public bool restartIfPlaying = true;

        [Header("Non-Destructive Source Segment")]
        [Min(0f)] public float playFromSeconds;
        [Tooltip("0 plays to the end. For loops, a value above 0 loops only this segment without editing the source file.")]
        [Min(0f)] public float playDurationSeconds;

        [Header("Mix")]
        public HearthAudioChannel channel = HearthAudioChannel.SFX;
        [Range(0f, 1f)] public float baseVolume = 1f;
        [Min(0.01f)] public float pitch = 1f;
        [Range(0f, 0.25f)] public float randomPitchRange;

        [Header("Placement")]
        public Transform followTarget;
        public Vector3 localOffset;
        public bool followWhilePlaying = true;
        [Range(0f, 1f)] public float spatialBlend = 1f;
        [Min(0.01f)] public float minDistance = 1f;
        [Min(0.01f)] public float maxDistance = 12f;

        [Header("Dialogue Mix")]
        public bool duckWhileDialogue;
        [Range(0.1f, 1f)] public float dialogueDuckScale = 0.56f;
    }

    [SerializeField] private HearthSfxCatalog catalog;
    [SerializeField] private CueSlot[] cues = Array.Empty<CueSlot>();
    [SerializeField] private bool logMissingClips;

    private readonly HashSet<CueSlot> activeLoopCues =
        new HashSet<CueSlot>();
    private readonly Dictionary<AudioSource, Coroutine> scheduledStops =
        new Dictionary<AudioSource, Coroutine>();

    public HearthSfxCatalog Catalog
    {
        get { return catalog; }
    }

    public int CueCount
    {
        get { return cues != null ? cues.Length : 0; }
    }

    public int AssignedClipCount
    {
        get
        {
            int count = 0;
            if (cues == null)
            {
                return count;
            }

            for (int i = 0; i < cues.Length; i++)
            {
                if (ResolveClip(cues[i], false) != null)
                {
                    count++;
                }
            }

            return count;
        }
    }

    private void Awake()
    {
        ApplyAllSourceSettings();
        SnapSourcesToTargets();
    }

    private void LateUpdate()
    {
        if (cues == null)
        {
            return;
        }

        for (int i = 0; i < cues.Length; i++)
        {
            CueSlot cue = cues[i];
            if (cue == null || cue.source == null || cue.followTarget == null)
            {
                continue;
            }

            if (cue.followWhilePlaying && cue.source.isPlaying)
            {
                PositionSource(cue);
            }

            if (activeLoopCues.Contains(cue))
            {
                MaintainLoopSegment(cue);
            }
        }
    }

    private void OnDisable()
    {
        StopAllCues();
    }

    private void OnValidate()
    {
        if (cues == null)
        {
            cues = Array.Empty<CueSlot>();
            return;
        }

        ApplyAllSourceSettings();
        if (!Application.isPlaying)
        {
            SnapSourcesToTargets();
        }
    }

    public bool PlayCue(string cueId)
    {
        CueSlot cue = FindCue(cueId);
        if (cue == null)
        {
            return false;
        }

        return cue.loop ? StartCueLoop(cueId) : PlayCueOneShot(cueId);
    }

    public bool PlayCueOneShot(string cueId)
    {
        CueSlot cue = FindCue(cueId);
        if (!CanPlay(cue))
        {
            return false;
        }

        AudioClip clip = ResolveClip(cue, true);
        if (clip == null)
        {
            LogMissingClip(cue);
            return false;
        }

        PrepareSource(cue, false);
        activeLoopCues.Remove(cue);
        CancelScheduledStop(cue.source);
        if (cue.restartIfPlaying && cue.source.isPlaying)
        {
            cue.source.Stop();
        }

        cue.source.loop = false;
        cue.source.clip = clip;
        cue.source.time = ResolveStartTime(cue, clip);
        cue.source.Play();
        float duration = ResolvePlaybackDuration(cue, clip);
        if (duration > 0f)
        {
            scheduledStops[cue.source] = StartCoroutine(
                StopSourceAfter(cue.source, duration));
        }
        return true;
    }

    public bool StartCueLoop(string cueId)
    {
        CueSlot cue = FindCue(cueId);
        if (!CanPlay(cue))
        {
            return false;
        }

        AudioClip clip = ResolveClip(cue, true);
        if (clip == null)
        {
            LogMissingClip(cue);
            return false;
        }

        if (cue.source.isPlaying &&
            cue.source.clip == clip &&
            activeLoopCues.Contains(cue) &&
            !cue.restartIfPlaying)
        {
            return true;
        }

        PrepareSource(cue, true);
        CancelScheduledStop(cue.source);
        cue.source.Stop();
        cue.source.clip = clip;
        cue.source.loop = !UsesCustomLoopSegment(cue, clip);
        cue.source.time = ResolveStartTime(cue, clip);
        activeLoopCues.Add(cue);
        cue.source.Play();
        return true;
    }

    public void StopCue(string cueId)
    {
        CueSlot cue = FindCue(cueId);
        if (cue == null || cue.source == null)
        {
            return;
        }

        cue.source.Stop();
        cue.source.loop = false;
        activeLoopCues.Remove(cue);
        CancelScheduledStop(cue.source);
    }

    public void StopAllCues()
    {
        if (cues == null)
        {
            return;
        }

        for (int i = 0; i < cues.Length; i++)
        {
            CueSlot cue = cues[i];
            if (cue == null || cue.source == null)
            {
                continue;
            }

            cue.source.Stop();
            cue.source.loop = false;
            CancelScheduledStop(cue.source);
        }

        activeLoopCues.Clear();
    }

    public bool HasCue(string cueId)
    {
        return FindCue(cueId) != null;
    }

    public bool HasAssignedClip(string cueId)
    {
        return ResolveClip(FindCue(cueId), false) != null;
    }

    public void AssignPrimaryClip(string cueId, AudioClip clip)
    {
        CueSlot cue = FindCue(cueId);
        if (cue != null)
        {
            cue.primaryClip = clip;
        }
    }

    public void SetCatalog(HearthSfxCatalog value)
    {
        catalog = value;
    }

    [ContextMenu("Snap Sources To Follow Targets")]
    public void SnapSourcesToTargets()
    {
        if (cues == null)
        {
            return;
        }

        for (int i = 0; i < cues.Length; i++)
        {
            PositionSource(cues[i]);
        }
    }

    private CueSlot FindCue(string cueId)
    {
        if (string.IsNullOrEmpty(cueId) || cues == null)
        {
            return null;
        }

        for (int i = 0; i < cues.Length; i++)
        {
            CueSlot cue = cues[i];
            if (cue != null && string.Equals(cue.cueId, cueId, StringComparison.OrdinalIgnoreCase))
            {
                return cue;
            }
        }

        if (logMissingClips && Application.isPlaying)
        {
            Debug.LogWarning("[HearthSfxCuePlayer] Cue ID not found: " + cueId, this);
        }

        return null;
    }

    private static bool CanPlay(CueSlot cue)
    {
        return cue != null && cue.source != null && cue.source.isActiveAndEnabled;
    }

    private void PrepareSource(CueSlot cue, bool loop)
    {
        ApplySourceSettings(cue);
        PositionSource(cue);
        cue.source.loop = loop;
        cue.source.pitch = Mathf.Max(0.01f, cue.pitch + UnityEngine.Random.Range(-cue.randomPitchRange, cue.randomPitchRange));
    }

    private void ApplyAllSourceSettings()
    {
        if (cues == null)
        {
            return;
        }

        for (int i = 0; i < cues.Length; i++)
        {
            ApplySourceSettings(cues[i]);
        }
    }

    private static void ApplySourceSettings(CueSlot cue)
    {
        if (cue == null || cue.source == null)
        {
            return;
        }

        cue.baseVolume = Mathf.Clamp01(cue.baseVolume);
        cue.pitch = Mathf.Max(0.01f, cue.pitch);
        cue.spatialBlend = Mathf.Clamp01(cue.spatialBlend);
        cue.minDistance = Mathf.Max(0.01f, cue.minDistance);
        cue.maxDistance = Mathf.Max(cue.minDistance, cue.maxDistance);
        cue.playFromSeconds = Mathf.Max(0f, cue.playFromSeconds);
        cue.playDurationSeconds = Mathf.Max(0f, cue.playDurationSeconds);
        cue.dialogueDuckScale = Mathf.Clamp(cue.dialogueDuckScale, 0.1f, 1f);

        AudioSource source = cue.source;
        source.playOnAwake = false;
        source.spatialBlend = cue.spatialBlend;
        source.minDistance = cue.minDistance;
        source.maxDistance = cue.maxDistance;
        source.dopplerLevel = 0f;
        source.rolloffMode = AudioRolloffMode.Linear;

        HearthAudioChannelSource channelSource = source.GetComponent<HearthAudioChannelSource>();
        if (channelSource != null)
        {
            channelSource.Configure(source, cue.channel, cue.baseVolume);
            channelSource.ConfigureDialogueDucking(
                cue.duckWhileDialogue,
                cue.dialogueDuckScale);
        }
        else
        {
            source.volume = cue.baseVolume;
        }
    }

    private static void PositionSource(CueSlot cue)
    {
        if (cue == null || cue.source == null || cue.followTarget == null)
        {
            return;
        }

        cue.source.transform.position = cue.followTarget.TransformPoint(cue.localOffset);
    }

    private AudioClip ResolveClip(CueSlot cue, bool randomize)
    {
        if (cue == null)
        {
            return null;
        }

        if (cue.primaryClip != null)
        {
            return cue.primaryClip;
        }

        if (cue.alternateClips != null && cue.alternateClips.Length > 0)
        {
            if (!randomize)
            {
                for (int i = 0; i < cue.alternateClips.Length; i++)
                {
                    if (cue.alternateClips[i] != null)
                    {
                        return cue.alternateClips[i];
                    }
                }
            }
            else
            {
                int start = UnityEngine.Random.Range(0, cue.alternateClips.Length);
                for (int i = 0; i < cue.alternateClips.Length; i++)
                {
                    AudioClip candidate = cue.alternateClips[(start + i) % cue.alternateClips.Length];
                    if (candidate != null)
                    {
                        return candidate;
                    }
                }
            }
        }

        if (catalog != null)
        {
            AudioClip catalogClip = catalog.ResolveClip(
                string.IsNullOrWhiteSpace(cue.soundId)
                    ? cue.cueId
                    : cue.soundId,
                randomize);
            if (catalogClip != null)
            {
                return catalogClip;
            }
        }

        return cue.source != null ? cue.source.clip : null;
    }

    private IEnumerator StopSourceAfter(AudioSource source, float seconds)
    {
        float elapsed = 0f;
        while (source != null && source.isPlaying && elapsed < seconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (source != null && source.isPlaying)
        {
            source.Stop();
        }

        if (source != null)
        {
            scheduledStops.Remove(source);
        }
    }

    private void CancelScheduledStop(AudioSource source)
    {
        if (source == null)
        {
            return;
        }

        Coroutine routine;
        if (scheduledStops.TryGetValue(source, out routine))
        {
            if (routine != null)
            {
                StopCoroutine(routine);
            }

            scheduledStops.Remove(source);
        }
    }

    private static float ResolveStartTime(CueSlot cue, AudioClip clip)
    {
        if (cue == null || clip == null || clip.length <= 0.02f)
        {
            return 0f;
        }

        return Mathf.Clamp(cue.playFromSeconds, 0f, clip.length - 0.01f);
    }

    private static float ResolvePlaybackDuration(CueSlot cue, AudioClip clip)
    {
        if (cue == null || clip == null || cue.playDurationSeconds <= 0f)
        {
            return 0f;
        }

        float remaining = Mathf.Max(0f, clip.length - ResolveStartTime(cue, clip));
        return Mathf.Min(cue.playDurationSeconds, remaining) /
            Mathf.Max(0.01f, cue.pitch + cue.randomPitchRange);
    }

    private static bool UsesCustomLoopSegment(CueSlot cue, AudioClip clip)
    {
        return cue != null &&
            clip != null &&
            (cue.playFromSeconds > 0f ||
             (cue.playDurationSeconds > 0f &&
              cue.playDurationSeconds < clip.length - 0.01f));
    }

    private void MaintainLoopSegment(CueSlot cue)
    {
        if (cue == null || cue.source == null || cue.source.clip == null)
        {
            activeLoopCues.Remove(cue);
            return;
        }

        AudioClip clip = cue.source.clip;
        if (!UsesCustomLoopSegment(cue, clip))
        {
            cue.source.loop = true;
            return;
        }

        float start = ResolveStartTime(cue, clip);
        float duration = cue.playDurationSeconds > 0f
            ? Mathf.Min(cue.playDurationSeconds, clip.length - start)
            : clip.length - start;
        float end = start + Mathf.Max(0.02f, duration);
        if (!cue.source.isPlaying || cue.source.time >= end - 0.015f)
        {
            cue.source.Stop();
            cue.source.time = start;
            cue.source.Play();
        }
    }

    private void LogMissingClip(CueSlot cue)
    {
        if (!logMissingClips || cue == null)
        {
            return;
        }

        Debug.LogWarning(
            "[HearthSfxCuePlayer] Cue '" + cue.cueId + "' has no AudioClip yet. The gameplay event continued silently.",
            this);
    }
}
