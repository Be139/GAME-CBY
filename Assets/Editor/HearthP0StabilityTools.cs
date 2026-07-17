#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class HearthP0StabilityTools
{
    private const string MenuRoot = "Tools/Hearth/Stability/";
    private const string CityPeopleTypeName = "CityPeople.CityPeople";

    private static readonly string[] DoorPrefabPaths =
    {
        "Assets/Free Wood Door Pack/Prefab/Wood/Door_2/Door_2_Brown.prefab",
        "Assets/Free Wood Door Pack/Prefab/Wood/Door_2/Door_2_Milk.prefab",
        "Assets/Free Wood Door Pack/Prefab/Wood/Door_2/Door_2_White.prefab",
    };

    [MenuItem(MenuRoot + "Apply P0 01-03 Repairs")]
    public static void ApplyP0Repairs()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("[HearthP0StabilityTools] Exit Play Mode before applying repairs.");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("[HearthP0StabilityTools] No loaded scene is available.");
            return;
        }

        int removedDoorScripts = RemoveMissingScriptsFromDoorPrefabs();
        int disabledCityPeople = DisableControlledCityPeopleAutoplay(scene);
        int removedLegacyCatAnimations = RemoveLegacyCatAnimations(scene);

        if (disabledCityPeople > 0 || removedLegacyCatAnimations > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log(
            "[HearthP0StabilityTools] Repairs applied. Removed missing door scripts: " + removedDoorScripts +
            ", disabled controlled CityPeople behaviours: " + disabledCityPeople +
            ", removed legacy cat Animation components: " + removedLegacyCatAnimations + ".");
        ValidateP0Repairs();
    }

    [MenuItem(MenuRoot + "Validate P0 01-03")]
    public static void ValidateP0Repairs()
    {
        List<string> issues = new List<string>();
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("[HearthP0StabilityTools] No loaded scene is available.");
            return;
        }

        ValidateSceneMissingScripts(scene, issues);
        ValidateDoorPrefabs(issues);
        ValidateControlledCityPeople(scene, issues);
        ValidateCatLegacyAnimations(scene, issues);

        if (issues.Count == 0)
        {
            Debug.Log(
                "[HearthP0StabilityTools] Validation passed: Door prefabs have no missing scripts, " +
                "controlled actors have no enabled random CityPeople behaviour, and the cat uses no Legacy Animation component.");
            return;
        }

        Debug.LogError(
            "[HearthP0StabilityTools] Validation found " + issues.Count + " issue(s):\n- " +
            string.Join("\n- ", issues.ToArray()));
    }

    public static int DisableCityPeopleAutoplayInHierarchy(GameObject root, bool recordUndo)
    {
        if (root == null)
        {
            return 0;
        }

        int changed = 0;
        MonoBehaviour[] behaviours = root.GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour == null || !behaviour.enabled || behaviour.GetType().FullName != CityPeopleTypeName)
            {
                continue;
            }

            if (recordUndo)
            {
                Undo.RecordObject(behaviour, "Disable random CityPeople animation");
            }

            behaviour.enabled = false;
            EditorUtility.SetDirty(behaviour);
            changed++;
        }

        return changed;
    }

    private static int RemoveMissingScriptsFromDoorPrefabs()
    {
        int removed = 0;
        for (int i = 0; i < DoorPrefabPaths.Length; i++)
        {
            string path = DoorPrefabPaths[i];
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
            {
                Debug.LogWarning("[HearthP0StabilityTools] Door prefab was not found: " + path);
                continue;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            bool changed = false;
            try
            {
                foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
                {
                    int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(item.gameObject);
                    if (count <= 0)
                    {
                        continue;
                    }

                    GameObjectUtility.RemoveMonoBehavioursWithMissingScript(item.gameObject);
                    removed += count;
                    changed = true;
                }

                if (changed)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, path);
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        return removed;
    }

    private static int DisableControlledCityPeopleAutoplay(Scene scene)
    {
        int changed = 0;
        MonoBehaviour[] behaviours = Object.FindObjectsOfType<MonoBehaviour>(true);
        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour == null || behaviour.gameObject.scene != scene || !IsControlledActorBehaviour(behaviour))
            {
                continue;
            }

            changed += DisableCityPeopleAutoplayInHierarchy(behaviour.gameObject, true);
        }

        return changed;
    }

    private static bool IsControlledActorBehaviour(MonoBehaviour behaviour)
    {
        if (behaviour.GetType().FullName != CityPeopleTypeName)
        {
            return false;
        }

        Transform current = behaviour.transform;
        while (current != null)
        {
            if (current.GetComponent<HearthActorAnimatorDriver>() != null ||
                current.GetComponent<HearthActorAnimationPlayer>() != null ||
                current.GetComponent<Hearth17F03StagingPoseProxy>() != null ||
                current.GetComponent<HearthEditorOnlyReferenceModel>() != null)
            {
                return true;
            }

            current = current.parent;
        }

        string path = GetHierarchyPath(behaviour.transform);
        return path.StartsWith("MIN_LOOP_ROOT/ReplayRoom_17F", StringComparison.Ordinal) ||
               path.StartsWith("MIN_LOOP_ROOT/Finale_17F04", StringComparison.Ordinal);
    }

    private static int RemoveLegacyCatAnimations(Scene scene)
    {
        int removed = 0;
        foreach (GameObject gameObject in EnumerateSceneGameObjects(scene))
        {
            if (!IsCatRouteObject(gameObject.name))
            {
                continue;
            }

            Animation[] legacy = gameObject.GetComponentsInChildren<Animation>(true);
            for (int i = 0; i < legacy.Length; i++)
            {
                if (legacy[i] == null)
                {
                    continue;
                }

                Undo.DestroyObjectImmediate(legacy[i]);
                removed++;
            }
        }

        return removed;
    }

    private static void ValidateSceneMissingScripts(Scene scene, List<string> issues)
    {
        foreach (GameObject gameObject in EnumerateSceneGameObjects(scene))
        {
            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);
            if (count > 0)
            {
                issues.Add(GetHierarchyPath(gameObject.transform) + " has " + count + " missing script component(s).");
            }
        }
    }

    private static void ValidateDoorPrefabs(List<string> issues)
    {
        for (int i = 0; i < DoorPrefabPaths.Length; i++)
        {
            string path = DoorPrefabPaths[i];
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null)
            {
                issues.Add("Door prefab is missing: " + path + ".");
                continue;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
                {
                    int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(item.gameObject);
                    if (count > 0)
                    {
                        issues.Add(path + " / " + GetHierarchyPath(item) + " has " + count + " missing script component(s).");
                    }
                }
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }
    }

    private static void ValidateControlledCityPeople(Scene scene, List<string> issues)
    {
        MonoBehaviour[] behaviours = Object.FindObjectsOfType<MonoBehaviour>(true);
        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour == null || behaviour.gameObject.scene != scene || !behaviour.enabled)
            {
                continue;
            }

            if (IsControlledActorBehaviour(behaviour))
            {
                issues.Add(GetHierarchyPath(behaviour.transform) + " still has enabled CityPeople random autoplay.");
            }
        }
    }

    private static void ValidateCatLegacyAnimations(Scene scene, List<string> issues)
    {
        foreach (GameObject gameObject in EnumerateSceneGameObjects(scene))
        {
            if (!IsCatRouteObject(gameObject.name))
            {
                continue;
            }

            Animation[] legacy = gameObject.GetComponentsInChildren<Animation>(true);
            if (legacy.Any(item => item != null))
            {
                issues.Add(GetHierarchyPath(gameObject.transform) + " still contains a Legacy Animation component.");
            }
        }
    }

    private static bool IsCatRouteObject(string objectName)
    {
        return objectName == "CatMoveRoot" || objectName.StartsWith("CatMoveRoot (", StringComparison.Ordinal);
    }

    private static IEnumerable<GameObject> EnumerateSceneGameObjects(Scene scene)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(true);
            for (int j = 0; j < transforms.Length; j++)
            {
                yield return transforms[j].gameObject;
            }
        }
    }

    private static string GetHierarchyPath(Transform transform)
    {
        if (transform == null)
        {
            return "<missing>";
        }

        string path = transform.name;
        Transform parent = transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }

        return path;
    }
}
#endif
