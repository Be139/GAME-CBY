using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(CityDuskLightingPresetController))]
public class CityDuskLightingPresetControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CityDuskLightingPresetController controller = (CityDuskLightingPresetController)target;
        EditorGUILayout.Space(10f);

        if (GUILayout.Button("Apply Fixed Dusk"))
        {
            Undo.RecordObject(controller, "Apply fixed dusk");
            controller.ApplyDusk();
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
        }

        if (GUILayout.Button("Restore Original Lighting"))
        {
            Undo.RecordObject(controller, "Restore original lighting");
            controller.RestoreOriginal();
            EditorUtility.SetDirty(controller);
            EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
        }
    }
}
