using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MinLoopSceneValidator))]
public class MinLoopSceneValidatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(12f);

        MinLoopSceneValidator validator = (MinLoopSceneValidator)target;

        if (GUILayout.Button("Resolve References"))
        {
            Undo.RecordObject(validator, "Resolve Min Loop Validator References");
            validator.ResolveReferences();
            EditorUtility.SetDirty(validator);
        }

        if (GUILayout.Button("Auto Bind Scene References"))
        {
            MinLoopSceneAutoBinder.AutoBindSceneReferences();
        }

        if (GUILayout.Button("Validate Scene Setup"))
        {
            validator.ValidateSceneSetup();
        }

        if (validator.LastErrorCount > 0)
        {
            EditorGUILayout.HelpBox("上次检查发现必须修复的问题：" + validator.LastErrorCount, MessageType.Error);
        }
        else if (validator.LastWarningCount > 0)
        {
            EditorGUILayout.HelpBox("上次检查没有致命错误，但有建议项：" + validator.LastWarningCount, MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox("点击 Validate Scene Setup 检查 17F-01 最小循环场景挂载。", MessageType.Info);
        }
    }
}
