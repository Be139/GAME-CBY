using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "HearthSfxCatalog",
    menuName = "Hearth/Audio/SFX Catalog")]
public sealed class HearthSfxCatalog : ScriptableObject
{
    [Serializable]
    public sealed class SoundEntry
    {
        [Tooltip("Stable sound asset ID. Gameplay cue IDs may reuse the same sound ID.")]
        public string soundId;

        [TextArea(2, 4)]
        public string maintenanceNote;

        public AudioClip primaryClip;
        public AudioClip[] alternateClips = Array.Empty<AudioClip>();
    }

    [SerializeField] private SoundEntry[] entries = Array.Empty<SoundEntry>();

    private readonly Dictionary<string, SoundEntry> entryMap =
        new Dictionary<string, SoundEntry>(StringComparer.OrdinalIgnoreCase);

    public int EntryCount
    {
        get { return entries != null ? entries.Length : 0; }
    }

    public int AssignedClipCount
    {
        get
        {
            int count = 0;
            if (entries == null)
            {
                return count;
            }

            for (int i = 0; i < entries.Length; i++)
            {
                if (ResolveClip(entries[i], false) != null)
                {
                    count++;
                }
            }

            return count;
        }
    }

    private void OnEnable()
    {
        RebuildMap();
    }

    private void OnValidate()
    {
        RebuildMap();
    }

    public bool HasSound(string soundId)
    {
        return FindEntry(soundId) != null;
    }

    public AudioClip GetPrimaryClip(string soundId)
    {
        return ResolveClip(FindEntry(soundId), false);
    }

    public AudioClip ResolveClip(string soundId, bool randomize)
    {
        return ResolveClip(FindEntry(soundId), randomize);
    }

    private SoundEntry FindEntry(string soundId)
    {
        if (string.IsNullOrWhiteSpace(soundId))
        {
            return null;
        }

        if (entryMap.Count != EntryCount)
        {
            RebuildMap();
        }

        SoundEntry entry;
        return entryMap.TryGetValue(soundId, out entry) ? entry : null;
    }

    private void RebuildMap()
    {
        entryMap.Clear();
        if (entries == null)
        {
            entries = Array.Empty<SoundEntry>();
            return;
        }

        for (int i = 0; i < entries.Length; i++)
        {
            SoundEntry entry = entries[i];
            if (entry == null || string.IsNullOrWhiteSpace(entry.soundId))
            {
                continue;
            }

            entryMap[entry.soundId.Trim()] = entry;
        }
    }

    private static AudioClip ResolveClip(SoundEntry entry, bool randomize)
    {
        if (entry == null)
        {
            return null;
        }

        if (entry.primaryClip != null)
        {
            return entry.primaryClip;
        }

        AudioClip[] alternates = entry.alternateClips;
        if (alternates == null || alternates.Length == 0)
        {
            return null;
        }

        int start = randomize
            ? UnityEngine.Random.Range(0, alternates.Length)
            : 0;
        for (int i = 0; i < alternates.Length; i++)
        {
            AudioClip candidate = alternates[(start + i) % alternates.Length];
            if (candidate != null)
            {
                return candidate;
            }
        }

        return null;
    }
}
