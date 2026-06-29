using System;
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
public class HearthCompanionHudSceneEvent : UnityEvent<string>
{
}
