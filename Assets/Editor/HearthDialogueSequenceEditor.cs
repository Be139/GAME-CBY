using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(HearthDialogueSequence))]
public class HearthDialogueSequenceEditor : Editor
{
    private SerializedProperty sequenceId;
    private SerializedProperty notes;
    private SerializedProperty lines;
    private SerializedProperty postSequenceDelay;
    private ReorderableList lineList;

    private void OnEnable()
    {
        sequenceId = serializedObject.FindProperty("sequenceId");
        notes = serializedObject.FindProperty("notes");
        lines = serializedObject.FindProperty("lines");
        postSequenceDelay = serializedObject.FindProperty("postSequenceDelay");

        lineList = new ReorderableList(serializedObject, lines, true, true, true, true);
        lineList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Dialogue Lines (drag to reorder)");
        lineList.elementHeightCallback = GetLineHeight;
        lineList.drawElementCallback = DrawLine;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(sequenceId);
        EditorGUILayout.PropertyField(notes);
        EditorGUILayout.Space(4f);
        lineList.DoLayoutList();
        EditorGUILayout.PropertyField(postSequenceDelay);

        EditorGUILayout.Space(6f);
        EditorGUILayout.HelpBox(
            "Each line can use manual timing, follow its AudioClip length, or use whichever is longer. " +
            "Add, remove and reorder lines here; no script change is required.",
            MessageType.Info);

        serializedObject.ApplyModifiedProperties();
    }

    private float GetLineHeight(int index)
    {
        if (index < 0 || index >= lines.arraySize)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        SerializedProperty line = lines.GetArrayElementAtIndex(index);
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        return EditorGUI.GetPropertyHeight(line.FindPropertyRelative("speaker")) + spacing +
               EditorGUI.GetPropertyHeight(line.FindPropertyRelative("text")) + spacing +
               EditorGUI.GetPropertyHeight(line.FindPropertyRelative("startDelay")) + spacing +
               EditorGUI.GetPropertyHeight(line.FindPropertyRelative("durationMode")) + spacing +
               EditorGUI.GetPropertyHeight(line.FindPropertyRelative("holdSeconds")) + spacing +
               EditorGUI.GetPropertyHeight(line.FindPropertyRelative("voiceClip")) + spacing +
               EditorGUI.GetPropertyHeight(line.FindPropertyRelative("voiceTailSeconds")) + 10f;
    }

    private void DrawLine(Rect rect, int index, bool isActive, bool isFocused)
    {
        SerializedProperty line = lines.GetArrayElementAtIndex(index);
        float y = rect.y + 2f;
        DrawProperty(ref y, rect, line.FindPropertyRelative("speaker"), "Speaker");
        DrawProperty(ref y, rect, line.FindPropertyRelative("text"), "Subtitle Text");
        DrawProperty(ref y, rect, line.FindPropertyRelative("startDelay"), "Delay Before Line");
        DrawProperty(ref y, rect, line.FindPropertyRelative("durationMode"), "Duration Mode");
        DrawProperty(ref y, rect, line.FindPropertyRelative("holdSeconds"), "Manual Hold Seconds");
        DrawProperty(ref y, rect, line.FindPropertyRelative("voiceClip"), "Voice AudioClip");
        DrawProperty(ref y, rect, line.FindPropertyRelative("voiceTailSeconds"), "Voice Tail Seconds");
    }

    private static void DrawProperty(ref float y, Rect elementRect, SerializedProperty property, string label)
    {
        float height = EditorGUI.GetPropertyHeight(property);
        Rect fieldRect = new Rect(elementRect.x, y, elementRect.width, height);
        EditorGUI.PropertyField(fieldRect, property, new GUIContent(label), true);
        y += height + EditorGUIUtility.standardVerticalSpacing;
    }
}
