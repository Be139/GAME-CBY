using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class HearthCompanionHoldInteractionRepair
{
    private const string MenuRoot = "Tools/Hearth/HUD/";
    private const string CompanionHudPrefabPath = "Assets/Prefabs/UI/HearthHud/Companion/HearthCompanionHudRoot.prefab";

    [MenuItem(MenuRoot + "Repair Companion Hold Interactions")]
    public static void RepairOpenScene()
    {
        HearthCompanionHudController hud = FindSceneObject<HearthCompanionHudController>();
        if (hud == null)
        {
            Debug.LogError("[HearthCompanionHoldInteractionRepair] HearthCompanionHudRoot was not found in the open scene.");
            return;
        }

        HearthCompanionHoldPrompt prompt = hud.GetComponentInChildren<HearthCompanionHoldPrompt>(true);
        if (prompt == null)
        {
            Debug.LogError("[HearthCompanionHoldInteractionRepair] HoldPrompt was not found below HearthCompanionHudRoot.", hud);
            return;
        }

        BindObject(hud, "holdPrompt", prompt);
        BindObject(prompt, "controller", hud);

        foreach (HearthCompanion17F01ReplayController controller in FindSceneObjects<HearthCompanion17F01ReplayController>())
        {
            BindObject(controller, "companionHud", hud);
        }

        foreach (HearthCompanion17F02ReplayController controller in FindSceneObjects<HearthCompanion17F02ReplayController>())
        {
            BindObject(controller, "companionHud", hud);
            SetBool(controller, "waitForBedroomAcknowledgement", true);
        }

        foreach (HearthCompanion17F03ReplayController controller in FindSceneObjects<HearthCompanion17F03ReplayController>())
        {
            BindObject(controller, "companionHud", hud);
        }

        HearthCompanionHudPreviewInput preview = hud.GetComponent<HearthCompanionHudPreviewInput>();
        if (preview != null)
        {
            SetBool(preview, "previewInputEnabled", false);
            BindObject(preview, "controller", hud);
        }

        RepairPrefabDefaults();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        Debug.Log("[HearthCompanionHoldInteractionRepair] Rebound the shared HoldPrompt to 17F01/02/03 and disabled formal-build preview input.");
        ValidateOpenScene();
    }

    [MenuItem(MenuRoot + "Validate Companion Hold Interactions")]
    public static void ValidateOpenScene()
    {
        int warnings = 0;
        HearthCompanionHudController hud = FindSceneObject<HearthCompanionHudController>();
        HearthCompanionHoldPrompt prompt = hud != null
            ? hud.GetComponentInChildren<HearthCompanionHoldPrompt>(true)
            : null;

        if (hud == null || prompt == null)
        {
            Debug.LogWarning("[HearthCompanionHoldInteractionRepair] The formal companion HUD or HoldPrompt is missing.");
            warnings++;
        }

        warnings += ValidateHudReferences(FindSceneObjects<HearthCompanion17F01ReplayController>(), hud, "17F01");
        warnings += ValidateHudReferences(FindSceneObjects<HearthCompanion17F02ReplayController>(), hud, "17F02");
        warnings += ValidateHudReferences(FindSceneObjects<HearthCompanion17F03ReplayController>(), hud, "17F03");

        HearthCompanionHudPreviewInput preview = hud != null
            ? hud.GetComponent<HearthCompanionHudPreviewInput>()
            : null;
        if (preview != null && ReadBool(preview, "previewInputEnabled"))
        {
            Debug.LogWarning("[HearthCompanionHoldInteractionRepair] Companion HUD preview input is still enabled in the formal scene.", preview);
            warnings++;
        }

        if (warnings == 0)
        {
            Debug.Log("[HearthCompanionHoldInteractionRepair] Validation passed: the three replay controllers share one valid HoldPrompt binding.");
        }
        else
        {
            Debug.LogWarning("[HearthCompanionHoldInteractionRepair] Validation finished with " + warnings + " warning(s).");
        }
    }

    private static int ValidateHudReferences<T>(T[] controllers, HearthCompanionHudController expectedHud, string label)
        where T : MonoBehaviour
    {
        int warnings = 0;
        for (int i = 0; i < controllers.Length; i++)
        {
            SerializedObject serialized = new SerializedObject(controllers[i]);
            SerializedProperty property = serialized.FindProperty("companionHud");
            if (property == null || property.objectReferenceValue != expectedHud)
            {
                Debug.LogWarning("[HearthCompanionHoldInteractionRepair] " + label + " is not bound to the formal companion HUD.", controllers[i]);
                warnings++;
            }
        }

        return warnings;
    }

    private static T FindSceneObject<T>() where T : Component
    {
        T[] objects = FindSceneObjects<T>();
        return objects.Length > 0 ? objects[0] : null;
    }

    private static T[] FindSceneObjects<T>() where T : Component
    {
        T[] all = Object.FindObjectsOfType<T>(true);
        System.Collections.Generic.List<T> sceneObjects = new System.Collections.Generic.List<T>();
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null && all[i].gameObject.scene == EditorSceneManager.GetActiveScene())
            {
                sceneObjects.Add(all[i]);
            }
        }

        return sceneObjects.ToArray();
    }

    private static void BindObject(Object target, string propertyName, Object value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null)
        {
            Debug.LogWarning("[HearthCompanionHoldInteractionRepair] Missing serialized property " + propertyName + " on " + target.name + ".", target);
            return;
        }

        property.objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void SetBool(Object target, string propertyName, bool value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null)
        {
            return;
        }

        property.boolValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static bool ReadBool(Object target, string propertyName)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        return property != null && property.boolValue;
    }

    private static void RepairPrefabDefaults()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(CompanionHudPrefabPath) == null)
        {
            return;
        }

        GameObject contents = PrefabUtility.LoadPrefabContents(CompanionHudPrefabPath);
        try
        {
            HearthCompanionHudController hud = contents.GetComponent<HearthCompanionHudController>();
            HearthCompanionHoldPrompt prompt = contents.GetComponentInChildren<HearthCompanionHoldPrompt>(true);
            HearthCompanionHudPreviewInput preview = contents.GetComponent<HearthCompanionHudPreviewInput>();
            if (hud != null && prompt != null)
            {
                BindObject(hud, "holdPrompt", prompt);
                BindObject(prompt, "controller", hud);
            }

            if (preview != null)
            {
                SetBool(preview, "previewInputEnabled", false);
                BindObject(preview, "controller", hud);
            }

            PrefabUtility.SaveAsPrefabAsset(contents, CompanionHudPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }
}
