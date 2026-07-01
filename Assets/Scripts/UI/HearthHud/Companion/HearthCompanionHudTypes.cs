using System;
using UnityEngine;
using UnityEngine.Events;

public enum HearthCompanionHudTemplate
{
    Standard,
    Projection,
    BlackAudio,
    ShutdownGlitch,
    DeepSleep
}

public enum HearthCompanionSpecialEffect
{
    None,
    ShutdownGlitch,
    BlackAudio,
    DeepSleep
}

[Serializable]
public class HearthCompanionMetricLine
{
    public string label;
    public string value;

    public HearthCompanionMetricLine()
    {
    }

    public HearthCompanionMetricLine(string label, string value)
    {
        this.label = label;
        this.value = value;
    }
}

[Serializable]
public class HearthCompanionTimedCue
{
    public float delaySeconds = 0.25f;
    public float visibleSeconds = 3.5f;
    public string title;

    [TextArea(2, 8)]
    public string body;

    public HearthCompanionTimedCue()
    {
    }

    public HearthCompanionTimedCue(float delaySeconds, float visibleSeconds, string title, string body)
    {
        this.delaySeconds = Mathf.Max(0f, delaySeconds);
        this.visibleSeconds = Mathf.Max(0f, visibleSeconds);
        this.title = title;
        this.body = body;
    }
}

[Serializable]
public class HearthCompanionHudSceneEvent : UnityEvent<string>
{
}
