using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class HearthTerminalOpeningBriefing : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HearthTvTerminalController terminal;
    [SerializeField] private MinLoopSubtitlePlayer subtitlePlayer;
    [SerializeField] private HearthDialogueSequence briefingSequence;

    [Header("Behaviour")]
    [SerializeField] private bool playOncePerRun = true;
    [SerializeField] private bool restartAfterEarlyExit = true;

    [Header("Runtime State")]
    [SerializeField] private bool briefingCompleted;
    [SerializeField] private bool briefingPlaying;

    private Coroutine briefingRoutine;

    public bool BriefingCompleted
    {
        get { return briefingCompleted; }
    }

    public bool BriefingPlaying
    {
        get { return briefingPlaying; }
    }

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
        CancelBriefing();
    }

    public void Configure(
        HearthTvTerminalController terminalController,
        MinLoopSubtitlePlayer player,
        HearthDialogueSequence sequence)
    {
        Unsubscribe();
        terminal = terminalController;
        subtitlePlayer = player;
        briefingSequence = sequence;
        Subscribe();
    }

    public void BeginBriefing()
    {
        ResolveReferences();
        if (terminal == null || !terminal.IsOpen)
        {
            return;
        }

        if (playOncePerRun && briefingCompleted)
        {
            terminal.SetPrimaryActionInputEnabled(true);
            terminal.ClearRuntimePrompt();
            return;
        }

        StopRoutine(false);
        briefingRoutine = StartCoroutine(BriefingRoutine());
    }

    public void CancelBriefing()
    {
        if (!briefingPlaying)
        {
            return;
        }

        StopRoutine(true);
        if (restartAfterEarlyExit)
        {
            briefingCompleted = false;
        }
    }

    public void ResetBriefing()
    {
        StopRoutine(true);
        briefingCompleted = false;
        if (terminal != null)
        {
            terminal.SetPrimaryActionInputEnabled(true);
            terminal.ClearRuntimePrompt();
        }
    }

    private IEnumerator BriefingRoutine()
    {
        briefingPlaying = true;
        terminal.SetPrimaryActionInputEnabled(false);
        terminal.SetRuntimePrompt("PLEASE WAIT");

        if (subtitlePlayer != null && briefingSequence != null && briefingSequence.HasLines)
        {
            HearthDialogueSurface surface = terminal.ResolveDialogueSurface();
            if (surface != null)
            {
                yield return subtitlePlayer.PlaySequenceAsset(
                    briefingSequence,
                    HearthDialoguePlaybackContext.Embedded(
                        surface,
                        HearthSubtitleContext.Terminal));
            }
            else
            {
                yield return subtitlePlayer.PlaySequenceAsset(briefingSequence);
            }
        }

        while (Input.GetKey(KeyCode.Space))
        {
            yield return null;
        }

        briefingPlaying = false;
        briefingCompleted = true;
        briefingRoutine = null;

        if (terminal != null && terminal.IsOpen)
        {
            terminal.SetPrimaryActionInputEnabled(true);
            terminal.ClearRuntimePrompt();
        }
    }

    private void StopRoutine(bool stopSubtitle)
    {
        if (briefingRoutine != null)
        {
            StopCoroutine(briefingRoutine);
            briefingRoutine = null;
        }

        if (stopSubtitle && subtitlePlayer != null && subtitlePlayer.IsPlaying)
        {
            subtitlePlayer.Stop();
            subtitlePlayer.Hide();
        }

        briefingPlaying = false;
        if (terminal != null)
        {
            terminal.SetPrimaryActionInputEnabled(true);
            terminal.ClearRuntimePrompt();
        }
    }

    private void Subscribe()
    {
        if (terminal == null)
        {
            return;
        }

        terminal.OnOpened.RemoveListener(BeginBriefing);
        terminal.OnOpened.AddListener(BeginBriefing);
        terminal.OnClosed.RemoveListener(CancelBriefing);
        terminal.OnClosed.AddListener(CancelBriefing);
    }

    private void Unsubscribe()
    {
        if (terminal == null)
        {
            return;
        }

        terminal.OnOpened.RemoveListener(BeginBriefing);
        terminal.OnClosed.RemoveListener(CancelBriefing);
    }

    private void ResolveReferences()
    {
        if (terminal == null)
        {
            terminal = GetComponent<HearthTvTerminalController>();
        }

        if (subtitlePlayer == null)
        {
            subtitlePlayer = FindObjectOfType<MinLoopSubtitlePlayer>(true);
        }
    }
}
