using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(HearthDialogueSequence))]
public class HearthDialogueSequenceEditor : Editor
{
    private SerializedProperty sequenceId;
    private SerializedProperty notes;
    private SerializedProperty dialogueChannel;
    private SerializedProperty defaultSpeakerSide;
    private SerializedProperty advancePolicy;
    private SerializedProperty lines;
    private SerializedProperty postSequenceDelay;
    private ReorderableList lineList;

    private void OnEnable()
    {
        sequenceId = serializedObject.FindProperty("sequenceId");
        notes = serializedObject.FindProperty("notes");
        dialogueChannel = serializedObject.FindProperty("dialogueChannel");
        defaultSpeakerSide = serializedObject.FindProperty("defaultSpeakerSide");
        advancePolicy = serializedObject.FindProperty("advancePolicy");
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
        EditorGUILayout.LabelField("Sequence Defaults", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(dialogueChannel);
        EditorGUILayout.PropertyField(defaultSpeakerSide);
        EditorGUILayout.PropertyField(advancePolicy);
        EditorGUILayout.Space(4f);
        lineList.DoLayoutList();
        EditorGUILayout.PropertyField(postSequenceDelay);

        EditorGUILayout.Space(6f);
        EditorGUILayout.HelpBox(
            "Each line independently selects its visual presentation, dialogue channel, speaker side and " +
            "advance policy. New ordinary dialogue defaults to framed Space-to-continue playback; the " +
            "versioned final-script policy owns the small automatic-caption allowlist.",
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
        return EditorGUI.GetPropertyHeight(line.FindPropertyRelative("lineId")) + spacing +
               EditorGUI.GetPropertyHeight(line.FindPropertyRelative("presentationKind")) + spacing +
               EditorGUI.GetPropertyHeight(line.FindPropertyRelative("dialogueMode")) + spacing +
               EditorGUI.GetPropertyHeight(line.FindPropertyRelative("speakerSide")) + spacing +
               EditorGUI.GetPropertyHeight(line.FindPropertyRelative("advancePolicy")) + spacing +
               EditorGUI.GetPropertyHeight(line.FindPropertyRelative("speaker")) + spacing +
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
        DrawProperty(ref y, rect, line.FindPropertyRelative("lineId"), "Voice Line ID");
        DrawProperty(ref y, rect, line.FindPropertyRelative("presentationKind"), "Presentation");
        DrawProperty(ref y, rect, line.FindPropertyRelative("dialogueMode"), "Dialogue Channel");
        DrawProperty(ref y, rect, line.FindPropertyRelative("speakerSide"), "Speaker Side");
        DrawProperty(ref y, rect, line.FindPropertyRelative("advancePolicy"), "Advance Policy");
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
