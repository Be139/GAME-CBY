using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Resolution-independent HEARTH V2 frame. Unlike a stretched bitmap border,
/// the chamfer and stroke thickness stay stable when a terminal dialogue lane
/// is resized through HearthUiLayoutProfile.
/// </summary>
[DisallowMultipleComponent]
public sealed class HearthV2FrameGraphic : MaskableGraphic
{
    [SerializeField, Min(1f)] private float strokeThickness = 2f;
    [SerializeField, Min(0f)] private float cornerCut = 14f;

    public void Configure(Color frameColor, float thickness, float cut)
    {
        color = frameColor;
        strokeThickness = Mathf.Max(1f, thickness);
        cornerCut = Mathf.Max(0f, cut);
        raycastTarget = false;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        Rect rect = GetPixelAdjustedRect();
        float maxCut = Mathf.Max(0f, Mathf.Min(rect.width, rect.height) * 0.5f - 1f);
        float cut = Mathf.Min(cornerCut, maxCut);
        float stroke = Mathf.Clamp(
            strokeThickness,
            1f,
            Mathf.Max(1f, Mathf.Min(rect.width, rect.height) * 0.25f));

        Vector2[] outer = BuildRing(rect, cut, 0f);
        Vector2[] inner = BuildRing(rect, cut, stroke);
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        for (int i = 0; i < 8; i++)
        {
            vertex.position = outer[i];
            vh.AddVert(vertex);
        }
        for (int i = 0; i < 8; i++)
        {
            vertex.position = inner[i];
            vh.AddVert(vertex);
        }

        for (int i = 0; i < 8; i++)
        {
            int next = (i + 1) % 8;
            vh.AddTriangle(i, next, 8 + next);
            vh.AddTriangle(i, 8 + next, 8 + i);
        }
    }

    private static Vector2[] BuildRing(Rect rect, float cut, float inset)
    {
        float left = rect.xMin + inset;
        float right = rect.xMax - inset;
        float bottom = rect.yMin + inset;
        float top = rect.yMax - inset;
        float adjustedCut = Mathf.Max(0f, cut - inset * 0.35f);
        return new[]
        {
            new Vector2(left + adjustedCut, top),
            new Vector2(right - adjustedCut, top),
            new Vector2(right, top - adjustedCut),
            new Vector2(right, bottom + adjustedCut),
            new Vector2(right - adjustedCut, bottom),
            new Vector2(left + adjustedCut, bottom),
            new Vector2(left, bottom + adjustedCut),
            new Vector2(left, top - adjustedCut)
        };
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        strokeThickness = Mathf.Max(1f, strokeThickness);
        cornerCut = Mathf.Max(0f, cornerCut);
        SetVerticesDirty();
    }
#endif
}
