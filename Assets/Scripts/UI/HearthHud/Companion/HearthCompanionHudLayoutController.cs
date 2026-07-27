using TMPro;
using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
public class HearthCompanionHudLayoutController : MonoBehaviour
{
    [SerializeField] private HearthCompanionHudLayoutProfile layoutProfile;
    [SerializeField] private RectTransform decisionRegion;
    [SerializeField] private RectTransform dataStreamRegion;
    [SerializeField] private TMP_Text[] sharedRegionTexts = new TMP_Text[0];

    [SerializeField, HideInInspector] private Vector2 decisionBasePosition;
    [SerializeField, HideInInspector] private Vector2 dataStreamBasePosition;
    [SerializeField, HideInInspector] private float[] baseFontSizes = new float[0];
    [SerializeField, HideInInspector] private bool baselinesCaptured;

    public HearthCompanionHudLayoutProfile LayoutProfile
    {
        get { return layoutProfile; }
    }

    private void OnEnable()
    {
        CaptureBaselinesIfNeeded();
        ApplySharedLayout();
    }

    private void OnValidate()
    {
        CaptureBaselinesIfNeeded();
        ApplySharedLayout();
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            ApplySharedLayout();
        }
    }

    public void Configure(
        HearthCompanionHudLayoutProfile profile,
        RectTransform decision,
        RectTransform dataStream,
        TMP_Text[] texts)
    {
        layoutProfile = profile;
        decisionRegion = decision;
        dataStreamRegion = dataStream;
        sharedRegionTexts = texts ?? new TMP_Text[0];
        baselinesCaptured = false;
        CaptureBaselinesIfNeeded();
        ApplySharedLayout();
    }

    public void ApplySharedLayout()
    {
        if (layoutProfile == null)
        {
            return;
        }

        CaptureBaselinesIfNeeded();
        float scale = layoutProfile.GlobalRegionScale;
        if (decisionRegion != null)
        {
            decisionRegion.localScale = new Vector3(scale, scale, 1f);
            Vector2 offset = layoutProfile.DecisionOffset;
            decisionRegion.anchoredPosition = decisionBasePosition + new Vector2(
                -layoutProfile.SharedHorizontalInset + offset.x,
                -layoutProfile.SharedVerticalOffset - offset.y);
        }

        if (dataStreamRegion != null)
        {
            dataStreamRegion.localScale = new Vector3(scale, scale, 1f);
            Vector2 offset = layoutProfile.DataStreamOffset;
            dataStreamRegion.anchoredPosition = dataStreamBasePosition + new Vector2(
                layoutProfile.SharedHorizontalInset + offset.x,
                -layoutProfile.SharedVerticalOffset - offset.y);
        }

        int count = Mathf.Min(sharedRegionTexts != null ? sharedRegionTexts.Length : 0, baseFontSizes.Length);
        for (int i = 0; i < count; i++)
        {
            TMP_Text text = sharedRegionTexts[i];
            if (text == null)
            {
                continue;
            }

            float size = Mathf.Max(1f, baseFontSizes[i] * layoutProfile.GlobalTextScale);
            text.fontSize = size;
            text.fontSizeMax = size;
        }
    }

    public void RecaptureBaselines()
    {
        baselinesCaptured = false;
        CaptureBaselinesIfNeeded();
        ApplySharedLayout();
    }

    private void CaptureBaselinesIfNeeded()
    {
        if (baselinesCaptured)
        {
            return;
        }

        if (decisionRegion != null) decisionBasePosition = decisionRegion.anchoredPosition;
        if (dataStreamRegion != null) dataStreamBasePosition = dataStreamRegion.anchoredPosition;
        int count = sharedRegionTexts != null ? sharedRegionTexts.Length : 0;
        baseFontSizes = new float[count];
        for (int i = 0; i < count; i++)
        {
            baseFontSizes[i] = sharedRegionTexts[i] != null ? sharedRegionTexts[i].fontSize : 1f;
        }

        baselinesCaptured = true;
    }
}
