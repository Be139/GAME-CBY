#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class HearthProjectOrganizer
{
    private const string MenuRoot = "Tools/Hearth/Project/";

    private sealed class SceneSection
    {
        public readonly string separator;
        public readonly HashSet<string> names;

        public SceneSection(string separator, params string[] names)
        {
            this.separator = separator;
            this.names = new HashSet<string>(names, StringComparer.Ordinal);
        }
    }

    private static readonly SceneSection[] SceneSections =
    {
        new SceneSection(
            "========= 01 UI =========",
            "LegacyHumanOverlay", "HearthCompanionHudRoot", "HearthHudRoot"),
        new SceneSection(
            "========= 02 PLAYER & CAMERAS =========",
            "Player", "FallbackCamera"),
        new SceneSection(
            "========= 03 GAMEPLAY SYSTEMS =========",
            "LocationSystem", "EventSystem", "Gameplay"),
        new SceneSection(
            "========= 04 WORLD SCENES =========",
            "Lobby", "Floor17", "HouseholdActors"),
        new SceneSection(
            "========= 05 ENVIRONMENT =========",
            "OriginalSunLight", "OriginalPostProcessing", "CityBuildings_Manual", "CityBillboardAuthoring",
            "CityBillboards_ManualBuildings", "CityBuildings_Generated", "CityLotAuthoring", "outdoor",
            "unity5", "BuildingSiteGeometry", "CityLots_Primary", "dikuai (1)",
            "OutdoorProps", "EnvironmentGeometry_Secondary", "CityLots_Secondary", "CityLots_Offset",
            "DuskLighting", "CityWindowLighting"),
        new SceneSection(
            "========= 06 LOOSE ACTORS & SOURCE REFERENCES =========",
            "PoseReference_Ben_Seated", "Actor_Noah_17F01", "CharacterReference_Girl", "CharacterReference_Noah",
            "Actor_Emily_17F01", "Actor_Ben_17F02_Terminal", "Actor_Ben_17F02_Dining",
            "CharacterReference_PoliceOfficer", "Actor_Claire_17F02_Dining", "Actor_Daniel_17F01", "CharacterReference_Daniel"),
        new SceneSection(
            "========= 07 REVIEW =========",
            "GroundPlane_Reference", "LobbyLayoutReference"),
    };

    [MenuItem(MenuRoot + "Organize SampleScene Hierarchy (Safe)")]
    public static void OrganizeSceneHierarchy()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded ||
            scene.path != "Assets/Scenes/SampleScene.unity" || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError("[HearthProjectOrganizer] Open SampleScene in Edit Mode before organizing its hierarchy.");
            return;
        }

        List<GameObject> roots = scene.GetRootGameObjects().ToList();
        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Organize HEARTH root hierarchy");
        Dictionary<string, GameObject> separators = new Dictionary<string, GameObject>(StringComparer.Ordinal);
        for (int i = 0; i < SceneSections.Length; i++)
        {
            string name = SceneSections[i].separator;
            GameObject separator = roots.FirstOrDefault(root => IsEmptySeparator(root) && root.name == name);
            if (separator == null && i == 6)
                separator = roots.FirstOrDefault(root => IsEmptySeparator(root) && root.name == "========= 07 DEPRECATED / REVIEW =========");
            if (separator == null)
            {
                separator = new GameObject(name);
                Undo.RegisterCreatedObjectUndo(separator, "Create HEARTH hierarchy separator");
            }
            else
            {
                Undo.RecordObject(separator, "Update HEARTH hierarchy separator");
                roots.Remove(separator);
                separator.name = name;
            }
            separator.tag = "EditorOnly";
            separators.Add(name, separator);
        }

        HashSet<GameObject> assigned = new HashSet<GameObject>();
        int siblingIndex = 0;
        for (int sectionIndex = 0; sectionIndex < SceneSections.Length; sectionIndex++)
        {
            SceneSection section = SceneSections[sectionIndex];
            Undo.SetSiblingIndex(separators[section.separator].transform, siblingIndex++, "Organize HEARTH separator");
            IEnumerable<GameObject> members = roots.Where(root => GetSceneSectionIndex(root) == sectionIndex);

            foreach (GameObject member in members)
            {
                if (member == null || !assigned.Add(member))
                {
                    continue;
                }

                Undo.SetSiblingIndex(member.transform, siblingIndex++, "Organize HEARTH root hierarchy");
            }
        }

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[HearthProjectOrganizer] SampleScene root hierarchy organized. Parent paths, transforms and references are unchanged; scene remains unsaved for review.");
    }

    private static int GetSceneSectionIndex(GameObject root)
    {
        for (int i = 0; i < SceneSections.Length; i++)
            if (SceneSections[i].names.Contains(root.name)) return i;
        string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(root);
        if (prefabPath.StartsWith("Assets/Art/Environment/SourceModels/", StringComparison.Ordinal)) return 4;
        return 6;
    }

    [MenuItem(MenuRoot + "Validate Project Organization")]
    public static void ValidateOrganization()
    {
        List<string> issues = new List<string>();
        Scene scene = SceneManager.GetActiveScene();
        if (scene.IsValid() && scene.isLoaded)
        {
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < SceneSections.Length; i++)
            {
                if (!roots.Any(root => root.name == SceneSections[i].separator && root.CompareTag("EditorOnly")))
                {
                    issues.Add("Missing EditorOnly hierarchy separator: " + SceneSections[i].separator);
                }
            }

            if (roots.Any(root => IsEmptySeparator(root) && !IsStandardSeparatorName(root.name)))
            {
                issues.Add("An old empty hierarchy separator is still present.");
            }

            int currentSection = -1;
            foreach (GameObject root in roots.OrderBy(root => root.transform.GetSiblingIndex()))
            {
                int separatorIndex = Array.FindIndex(SceneSections, section => section.separator == root.name);
                if (separatorIndex >= 0 && IsEmptySeparator(root))
                {
                    currentSection = separatorIndex;
                    continue;
                }
                int expected = GetSceneSectionIndex(root);
                if (currentSection != expected)
                    issues.Add("Root is outside its assigned section: " + root.name + " -> " + SceneSections[expected].separator);
            }
        }

        if (issues.Count == 0)
        {
            Debug.Log("[HearthProjectOrganizer] Project organization validation passed.");
        }
        else
        {
            Debug.LogWarning("[HearthProjectOrganizer] Validation found " + issues.Count + " issue(s):\n- " + string.Join("\n- ", issues.ToArray()));
        }
    }

    private static bool IsEmptySeparator(GameObject gameObject)
    {
        if (gameObject == null || gameObject.transform.parent != null || gameObject.transform.childCount != 0)
        {
            return false;
        }

        if (!gameObject.name.StartsWith("=====", StringComparison.Ordinal))
        {
            return false;
        }

        return gameObject.GetComponents<Component>().All(component => component is Transform);
    }

    private static bool IsStandardSeparatorName(string name)
    {
        return SceneSections.Any(section => section.separator == name);
    }

}
#endif
