using UnityEngine;

public enum HearthUiThemeVersion
{
    Legacy,
    V2
}

[DisallowMultipleComponent]
public class HearthUiThemeMarker : MonoBehaviour
{
    [SerializeField] private HearthUiThemeVersion version = HearthUiThemeVersion.V2;
    [SerializeField] private string buildLabel = "HEARTH V2";

    public HearthUiThemeVersion Version
    {
        get { return version; }
    }

    public string BuildLabel
    {
        get { return buildLabel; }
    }

    public void Configure(HearthUiThemeVersion newVersion, string newBuildLabel)
    {
        version = newVersion;
        buildLabel = string.IsNullOrWhiteSpace(newBuildLabel)
            ? newVersion.ToString()
            : newBuildLabel;
    }
}
