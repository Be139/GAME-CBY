using UnityEngine;

[CreateAssetMenu(
    menuName = "Hearth/UI/Companion HUD Layout Profile",
    fileName = "Hearth_CompanionHudLayout")]
public class HearthCompanionHudLayoutProfile : ScriptableObject
{
    [Header("Shared Scale")]
    [SerializeField, Range(0.5f, 2f)] private float globalRegionScale = 1f;
    [SerializeField, Range(0.5f, 2f)] private float globalTextScale = 1f;

    [Header("Shared Position")]
    [Tooltip("Positive values move both regions inward from the screen edges.")]
    [SerializeField] private float sharedHorizontalInset;
    [Tooltip("Positive values move both regions downward.")]
    [SerializeField] private float sharedVerticalOffset;

    [Header("Individual Fine Tuning")]
    [SerializeField] private Vector2 decisionOffset;
    [SerializeField] private Vector2 dataStreamOffset;

    public float GlobalRegionScale { get { return globalRegionScale; } }
    public float GlobalTextScale { get { return globalTextScale; } }
    public float SharedHorizontalInset { get { return sharedHorizontalInset; } }
    public float SharedVerticalOffset { get { return sharedVerticalOffset; } }
    public Vector2 DecisionOffset { get { return decisionOffset; } }
    public Vector2 DataStreamOffset { get { return dataStreamOffset; } }

    private void OnValidate()
    {
        globalRegionScale = Mathf.Clamp(globalRegionScale, 0.5f, 2f);
        globalTextScale = Mathf.Clamp(globalTextScale, 0.5f, 2f);
    }
}
