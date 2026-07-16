#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Hearth17F04CatGuideController))]
public class Hearth17F04CatGuideControllerEditor : Editor
{
    private int routePointIndex;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Hearth17F04CatGuideController guide = (Hearth17F04CatGuideController)target;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Route Tools", EditorStyles.boldLabel);

        if (GUILayout.Button("Capture Current Pose As Start"))
        {
            Undo.RecordObject(guide, "Capture cat start pose");
            guide.CaptureCurrentAsStartPose();
            EditorUtility.SetDirty(guide);
        }

        if (GUILayout.Button("Reset Cat To Start"))
        {
            Undo.RecordObject(guide.transform, "Reset cat to start");
            guide.ResetSequence();
            EditorUtility.SetDirty(guide.transform);
        }

        int pointCount = guide.RoutePointCount;
        using (new EditorGUI.DisabledScope(pointCount == 0))
        {
            routePointIndex = Mathf.Clamp(
                EditorGUILayout.IntSlider("Route Point", routePointIndex + 1, 1, Mathf.Max(1, pointCount)) - 1,
                0,
                Mathf.Max(0, pointCount - 1));
            if (GUILayout.Button("Snap Cat To Selected Point"))
            {
                Undo.RecordObject(guide.transform, "Snap cat to route point");
                guide.SnapToRoutePoint(routePointIndex);
                EditorUtility.SetDirty(guide.transform);
            }
        }

        using (new EditorGUI.DisabledScope(!Application.isPlaying))
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Begin Play Mode Preview"))
            {
                guide.BeginSequence();
            }

            if (GUILayout.Button("Stop Preview"))
            {
                guide.StopSequence();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif
