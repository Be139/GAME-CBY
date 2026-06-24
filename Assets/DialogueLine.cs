using System;
using UnityEngine;

public enum DialogueSpeaker
{
    SpeakerOne,
    SpeakerTwo
}

[Serializable]
public class DialogueLine
{
    public DialogueSpeaker speaker = DialogueSpeaker.SpeakerTwo;

    [TextArea(2, 5)]
    public string text;

    public string GetSpeakerName(string speakerOneName, string speakerTwoName)
    {
        return speaker == DialogueSpeaker.SpeakerOne ? speakerOneName : speakerTwoName;
    }
}
