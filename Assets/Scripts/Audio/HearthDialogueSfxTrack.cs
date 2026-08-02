using System;
using System.Collections.Generic;
using UnityEngine;

public enum HearthDialogueSfxActionType
{
    PlayOneShot,
    StartLoop,
    Stop
}

[DisallowMultipleComponent]
public sealed class HearthDialogueSfxTrack : MonoBehaviour
{
    [Serializable]
    public sealed class CueAction
    {
        [Tooltip("Exact HearthDialogueSequence.SequenceId.")]
        public string sequenceId;

        [Tooltip("Exact MinLoopSubtitleLine.lineId. Leave empty to run at sequence completion.")]
        public string lineId;

        public HearthDialogueSfxActionType action =
            HearthDialogueSfxActionType.PlayOneShot;
        public string cueId;
    }

    [SerializeField] private MinLoopSubtitlePlayer subtitlePlayer;
    [SerializeField] private HearthSfxCuePlayer cuePlayer;
    [SerializeField] private CueAction[] actions = Array.Empty<CueAction>();

    private readonly HashSet<string> activeLoopCueIds =
        new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();
        Subscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
        StopTrackedLoops();
    }

    public void Configure(
        MinLoopSubtitlePlayer newSubtitlePlayer,
        HearthSfxCuePlayer newCuePlayer,
        CueAction[] newActions)
    {
        Unsubscribe();
        subtitlePlayer = newSubtitlePlayer;
        cuePlayer = newCuePlayer;
        actions = newActions ?? Array.Empty<CueAction>();
        Subscribe();
    }

    private void Subscribe()
    {
        if (subtitlePlayer == null)
        {
            return;
        }

        subtitlePlayer.LineStarted -= HandleLineStarted;
        subtitlePlayer.SequenceCompleted -= HandleSequenceCompleted;
        subtitlePlayer.LineStarted += HandleLineStarted;
        subtitlePlayer.SequenceCompleted += HandleSequenceCompleted;
    }

    private void Unsubscribe()
    {
        if (subtitlePlayer == null)
        {
            return;
        }

        subtitlePlayer.LineStarted -= HandleLineStarted;
        subtitlePlayer.SequenceCompleted -= HandleSequenceCompleted;
    }

    private void HandleLineStarted(
        string sequenceId,
        MinLoopSubtitleLine line,
        int lineIndex)
    {
        if (line == null || actions == null)
        {
            return;
        }

        for (int i = 0; i < actions.Length; i++)
        {
            CueAction cueAction = actions[i];
            if (cueAction == null ||
                string.IsNullOrWhiteSpace(cueAction.lineId) ||
                !Matches(cueAction.sequenceId, sequenceId) ||
                !Matches(cueAction.lineId, line.lineId))
            {
                continue;
            }

            Run(cueAction);
        }
    }

    private void HandleSequenceCompleted(string sequenceId)
    {
        if (actions != null)
        {
            for (int i = 0; i < actions.Length; i++)
            {
                CueAction cueAction = actions[i];
                if (cueAction != null &&
                    string.IsNullOrWhiteSpace(cueAction.lineId) &&
                    Matches(cueAction.sequenceId, sequenceId))
                {
                    Run(cueAction);
                }
            }
        }

        StopTrackedLoops();
    }

    private void Run(CueAction cueAction)
    {
        if (cuePlayer == null || string.IsNullOrWhiteSpace(cueAction.cueId))
        {
            return;
        }

        switch (cueAction.action)
        {
            case HearthDialogueSfxActionType.StartLoop:
                if (cuePlayer.StartCueLoop(cueAction.cueId))
                {
                    activeLoopCueIds.Add(cueAction.cueId);
                }
                break;
            case HearthDialogueSfxActionType.Stop:
                cuePlayer.StopCue(cueAction.cueId);
                activeLoopCueIds.Remove(cueAction.cueId);
                break;
            default:
                cuePlayer.PlayCueOneShot(cueAction.cueId);
                break;
        }
    }

    private void StopTrackedLoops()
    {
        if (cuePlayer != null)
        {
            foreach (string cueId in activeLoopCueIds)
            {
                cuePlayer.StopCue(cueId);
            }
        }

        activeLoopCueIds.Clear();
    }

    private void ResolveReferences()
    {
        if (subtitlePlayer == null)
        {
            subtitlePlayer = GetComponent<MinLoopSubtitlePlayer>();
        }
    }

    private static bool Matches(string expected, string actual)
    {
        return string.Equals(
            expected != null ? expected.Trim() : string.Empty,
            actual != null ? actual.Trim() : string.Empty,
            StringComparison.OrdinalIgnoreCase);
    }
}
