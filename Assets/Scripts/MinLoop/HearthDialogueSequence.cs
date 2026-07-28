using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HearthDialogueSequence", menuName = "Hearth/Dialogue Sequence")]
public class HearthDialogueSequence : ScriptableObject
{
    [SerializeField] private string sequenceId;

    [TextArea(2, 4)]
    [SerializeField] private string notes;

    [Header("Dialogue Runtime")]
    [SerializeField] private DialogueChannel dialogueChannel = DialogueChannel.Formal;
    [SerializeField] private SpeakerSide defaultSpeakerSide = SpeakerSide.Auto;
    [SerializeField] private AdvancePolicy advancePolicy = AdvancePolicy.ManualSpace;

    [SerializeField] private List<MinLoopSubtitleLine> lines = new List<MinLoopSubtitleLine>();

    [Min(0f)]
    [SerializeField] private float postSequenceDelay;

    public string SequenceId
    {
        get { return sequenceId; }
    }

    public string Notes
    {
        get { return notes; }
    }

    public IReadOnlyList<MinLoopSubtitleLine> Lines
    {
        get { return lines; }
    }

    public DialogueChannel DialogueChannel
    {
        get { return dialogueChannel; }
    }

    public SpeakerSide DefaultSpeakerSide
    {
        get { return defaultSpeakerSide; }
    }

    public AdvancePolicy AdvancePolicy
    {
        get { return advancePolicy; }
    }

    public float PostSequenceDelay
    {
        get { return postSequenceDelay; }
    }

    public bool HasLines
    {
        get { return lines != null && lines.Count > 0; }
    }
}
