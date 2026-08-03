using UnityEngine;

/// <summary>
/// Stable story-facing facade over the production SFX catalog player.
/// Existing cue IDs and audio asset GUIDs remain unchanged.
/// </summary>
[DisallowMultipleComponent]
public sealed class HearthStoryCueService : MonoBehaviour
{
    [SerializeField] private HearthSfxCuePlayer cuePlayer;

    public HearthSfxCuePlayer CuePlayer { get { return cuePlayer; } }

    public bool Play(string cueId)
    {
        return cuePlayer != null && cuePlayer.PlayCue(cueId);
    }

    public bool PlayOneShot(string cueId)
    {
        return cuePlayer != null && cuePlayer.PlayCueOneShot(cueId);
    }

    public bool StartLoop(string cueId)
    {
        return cuePlayer != null && cuePlayer.StartCueLoop(cueId);
    }

    public void Stop(string cueId)
    {
        if (cuePlayer != null)
        {
            cuePlayer.StopCue(cueId);
        }
    }

    public void StopAll()
    {
        if (cuePlayer != null)
        {
            cuePlayer.StopAllCues();
        }
    }

    public void Configure(HearthSfxCuePlayer player)
    {
        cuePlayer = player;
    }

    public static bool Play(HearthSfxCuePlayer player, string cueId)
    {
        return player != null && player.PlayCue(cueId);
    }

    public static bool StartLoop(HearthSfxCuePlayer player, string cueId)
    {
        return player != null && player.StartCueLoop(cueId);
    }

    public static void Stop(HearthSfxCuePlayer player, string cueId)
    {
        if (player != null)
        {
            player.StopCue(cueId);
        }
    }

    public static void StopAll(HearthSfxCuePlayer player)
    {
        if (player != null)
        {
            player.StopAllCues();
        }
    }
}
