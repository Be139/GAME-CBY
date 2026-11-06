using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(CityDuskLightingPresetController))]
public class CityDuskLightingPresetControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CityDuskLightingPresetController controller = (CityDuskLightingPresetController)target;
        EditorGUILayout.Space(10f);

        bool isActiveScene = controller.gameObject.scene == SceneManager.GetActiveScene();
        if (!isActiveScene)
        {
            EditorGUILayout.HelpBox("Make this object's scene active before changing global lighting.", MessageType.Info);
        }

        using (new EditorGUI.DisabledScope(!isActiveScene))
        {
            if (GUILayout.Button("Apply Fixed Dusk"))
            {
                ApplyLighting(controller, true);
            }

            if (GUILayout.Button("Restore Original Lighting"))
            {
                ApplyLighting(controller, false);
            }
        }
    }

    private void ApplyLighting(CityDuskLightingPresetController controller, bool apply)
    {
        string action = apply ? "Apply fixed dusk" : "Restore original lighting";
        var affected = new List<Object> { controller, Unsupported.GetRenderSettings() };
        serializedObject.Update();
        foreach (string field in new[] { "originalDirectionalLight", "originalGlobalVolume", "duskContentRoot", "roomSampleRoot" })
        {
            Object value = serializedObject.FindProperty(field).objectReferenceValue;
            if (value != null && !affected.Contains(value)) affected.Add(value);
        }

        Undo.RecordObjects(affected.ToArray(), action);
        if (apply) controller.ApplyDusk();
        else controller.RestoreOriginal();

        foreach (Object value in affected)
        {
            EditorUtility.SetDirty(value);
            if (PrefabUtility.IsPartOfPrefabInstance(value))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(value);
            }
        }
        EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
    }
}
