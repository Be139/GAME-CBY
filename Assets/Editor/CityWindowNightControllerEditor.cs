using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(CityWindowNightController))]
public class CityWindowNightControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CityWindowNightController controller =
            (CityWindowNightController)target;

        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("Window Night Actions", EditorStyles.boldLabel);

        if (GUILayout.Button("Apply Night Windows"))
        {
            Undo.RecordObject(controller, "Edit city window appearance");
            controller.ApplyNightWindows();
            EditorUtility.SetDirty(controller);
        }

        if (GUILayout.Button("Restore Original Windows"))
        {
            Undo.RecordObject(controller, "Edit city window appearance");
            controller.RestoreOriginalWindows();
            EditorUtility.SetDirty(controller);
        }

        if (GUILayout.Button("Redistribute Night Windows"))
        {
            Undo.RecordObject(controller, "Edit city window appearance");
            controller.RedistributeNightWindows();
            EditorUtility.SetDirty(controller);
        }

        if (GUILayout.Button("Validate Setup"))
        {
            controller.ValidateSetup();
        }
    }
}
