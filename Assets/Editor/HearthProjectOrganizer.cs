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

    private sealed class MoveSpec
    {
        public readonly string source;
        public readonly string destination;

        public MoveSpec(string source, string destination)
        {
            this.source = source;
            this.destination = destination;
        }
    }

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
            "HumanCanvas", "HearthCompanionHudRoot", "HearthHudRoot"),
        new SceneSection(
            "========= 02 PLAYER & CAMERAS =========",
            "Player", "Main Camera"),
        new SceneSection(
            "========= 03 GAMEPLAY SYSTEMS =========",
            "HEARTH_LOCATION_SYSTEM", "EventSystem", "MIN_LOOP_ROOT"),
        new SceneSection(
            "========= 04 WORLD SCENES =========",
            "1F (1)", "17F"),
        new SceneSection(
            "========= 05 ENVIRONMENT =========",
            "Directional Light", "Global Volume", "BUILDING", "CityBillboardPlacer",
            "CityBillboards_BUILDING", "CityBuildings_ByHeight", "CityAssetPlacer", "outdoor",
            "unity5", "DIKUAIunity", "dikuai", "dikuai (1)"),
        new SceneSection(
            "========= 06 LOOSE ACTORS & SOURCE REFERENCES ========="),
        new SceneSection(
            "========= 07 DEPRECATED / REVIEW =========",
            "Plane", "1F", "GameObject", "little_boy_B"),
    };

    private static readonly MoveSpec[] AssetMoves =
    {
        new MoveSpec("Assets/1地块.fbx", "Assets/Art/Environment/SourceModels/1地块.fbx"),
        new MoveSpec("Assets/地块.fbx", "Assets/Art/Environment/SourceModels/地块.fbx"),
        new MoveSpec("Assets/6unity.fbx", "Assets/Art/Environment/SourceModels/6unity.fbx"),
        new MoveSpec("Assets/DIKUAIunity.fbx", "Assets/Art/Environment/SourceModels/DIKUAIunity.fbx"),
        new MoveSpec("Assets/unity3.fbx", "Assets/Art/Environment/SourceModels/unity3.fbx"),
        new MoveSpec("Assets/unity4.fbx", "Assets/Art/Environment/SourceModels/unity4.fbx"),
        new MoveSpec("Assets/unity5.fbx", "Assets/Art/Environment/SourceModels/unity5.fbx"),
        new MoveSpec("Assets/unity3d.txt", "Assets/Art/Environment/SourceModels/unity3d.txt"),

        new MoveSpec("Assets/Laying_Sleeping.fbx", "Assets/Animations/Hearth/17F01/Clips/Laying_Sleeping.fbx"),
        new MoveSpec("Assets/casual_Female_G@Sitting_Idle.fbx", "Assets/Animations/Hearth/17F01/Clips/casual_Female_G@Sitting_Idle.fbx"),
        new MoveSpec("Assets/casual_Male_G@Sitting.fbx", "Assets/Animations/Hearth/17F01/Clips/casual_Male_G@Sitting.fbx"),

        new MoveSpec("Assets/Button_Pushing.fbx", "Assets/Animations/Hearth/17F02/Clips/Button_Pushing.fbx"),
        new MoveSpec("Assets/Open_Door_Outwards.fbx", "Assets/Animations/Hearth/17F02/Clips/Open_Door_Outwards.fbx"),
        new MoveSpec("Assets/Sitting.fbx", "Assets/Animations/Hearth/17F02/Clips/Sitting.fbx"),
        new MoveSpec("Assets/Sitting_Idle.fbx", "Assets/Animations/Hearth/17F02/Clips/Sitting_Idle.fbx"),
        new MoveSpec("Assets/Sitting_Talking.fbx", "Assets/Animations/Hearth/17F02/Clips/Sitting_Talking.fbx"),
        new MoveSpec("Assets/X_Bot@Sit_To_Stand.fbx", "Assets/Animations/Hearth/17F02/Clips/X_Bot@Sit_To_Stand.fbx"),
        new MoveSpec("Assets/casual_Female_K@Sitting_Disbelief.fbx", "Assets/Animations/Hearth/17F02/Clips/casual_Female_K@Sitting_Disbelief.fbx"),
        new MoveSpec("Assets/casual_Female_K@Walking.fbx", "Assets/Animations/Hearth/17F02/Clips/casual_Female_K@Walking.fbx"),
        new MoveSpec("Assets/Female_Start_Walking.fbx", "Assets/Animations/Hearth/_Reference/Female_Start_Walking.fbx"),
        new MoveSpec("Assets/Sitting_Idle (1).fbx", "Assets/Animations/Hearth/_Reference/Sitting_Idle (1).fbx"),
        new MoveSpec("Assets/Sitting_Idle (2).fbx", "Assets/Animations/Hearth/_Reference/Sitting_Idle (2).fbx"),

        new MoveSpec("Assets/Animation/Hearth/17F02/BedroomWife17F02.controller", "Assets/Animations/Hearth/17F02/Controllers/BedroomWife17F02.controller"),
        new MoveSpec("Assets/Animation/Hearth/17F02/DiningHusband17F02.controller", "Assets/Animations/Hearth/17F02/Controllers/DiningHusband17F02.controller"),
        new MoveSpec("Assets/Animation/Hearth/17F02/DiningWife17F02.controller", "Assets/Animations/Hearth/17F02/Controllers/DiningWife17F02.controller"),
        new MoveSpec("Assets/Animation/Hearth/17F02/TerminalHusband17F02.controller", "Assets/Animations/Hearth/17F02/Controllers/TerminalHusband17F02.controller"),

        new MoveSpec("Assets/action/Doctor_Male_B@Male_Sitting_Pose.fbx", "Assets/Animations/Hearth/17F03/Clips/Doctor_Male_B@Male_Sitting_Pose.fbx"),
        new MoveSpec("Assets/action/casual_Female_G@Sit_To_Stand.fbx", "Assets/Animations/Hearth/17F03/Clips/casual_Female_G@Sit_To_Stand.fbx"),
        new MoveSpec("Assets/action/casual_Female_G@Standing_Arguing.fbx", "Assets/Animations/Hearth/17F03/Clips/casual_Female_G@Standing_Arguing.fbx"),
        new MoveSpec("Assets/action/casual_Female_G@Talking.fbx", "Assets/Animations/Hearth/17F03/Clips/casual_Female_G@Talking.fbx"),
        new MoveSpec("Assets/action/casual_Female_K@Entering_Code.fbx", "Assets/Animations/Hearth/17F03/Clips/casual_Female_K@Entering_Code.fbx"),
        new MoveSpec("Assets/action/casual_Female_K@Female_Walk.fbx", "Assets/Animations/Hearth/17F03/Clips/casual_Female_K@Female_Walk.fbx"),
        new MoveSpec("Assets/action/casual_Female_K@Male_Sitting_Pose.fbx", "Assets/Animations/Hearth/17F03/Clips/casual_Female_K@Male_Sitting_Pose.fbx"),
        new MoveSpec("Assets/action/casual_Female_K@Situp_To_Idle.fbx", "Assets/Animations/Hearth/17F03/Clips/casual_Female_K@Situp_To_Idle.fbx"),
        new MoveSpec("Assets/action/casual_Female_K@Talking.fbx", "Assets/Animations/Hearth/17F03/Clips/casual_Female_K@Talking.fbx"),
        new MoveSpec("Assets/Animations/Hearth17F03/Hearth17F03_Daughter.controller", "Assets/Animations/Hearth/17F03/Controllers/Hearth17F03_Daughter.controller"),
        new MoveSpec("Assets/Animations/Hearth17F03/Hearth17F03_Father.controller", "Assets/Animations/Hearth/17F03/Controllers/Hearth17F03_Father.controller"),
        new MoveSpec("Assets/Animations/Hearth17F03/Hearth17F03_Mother.controller", "Assets/Animations/Hearth/17F03/Controllers/Hearth17F03_Mother.controller"),

        new MoveSpec("Assets/Girl_A_Rigged.fbx", "Assets/Art/Characters/Girl/Girl_A_Rigged.fbx"),
        new MoveSpec("Assets/Girl_A_Rigged.json", "Assets/Art/Characters/Girl/Girl_A_Rigged.json"),
        new MoveSpec("Assets/Girl_A_SittingIdle.controller", "Assets/Animations/Hearth/17F03/Controllers/Girl_A_SittingIdle.controller"),

        new MoveSpec("Assets/CityBillboardContentController.cs", "Assets/Scripts/Environment/City/CityBillboardContentController.cs"),
        new MoveSpec("Assets/CityBillboardContentDistributor.cs", "Assets/Scripts/Environment/City/CityBillboardContentDistributor.cs"),
        new MoveSpec("Assets/CityBillboardPlacementSlot.cs", "Assets/Scripts/Environment/City/CityBillboardPlacementSlot.cs"),
        new MoveSpec("Assets/CityBuildingPlacementSlot.cs", "Assets/Scripts/Environment/City/CityBuildingPlacementSlot.cs"),
        new MoveSpec("Assets/CityFacadeBillboardPlacer.cs", "Assets/Scripts/Environment/City/CityFacadeBillboardPlacer.cs"),
        new MoveSpec("Assets/CityLotAssetPlacer.cs", "Assets/Scripts/Environment/City/CityLotAssetPlacer.cs"),
        new MoveSpec("Assets/IInteractable1.cs", "Assets/Scripts/Interactions/IInteractable.cs"),
        new MoveSpec("Assets/PlayerInteraction.cs", "Assets/Scripts/Interactions/PlayerInteraction.cs"),
        new MoveSpec("Assets/TerminalUIController.cs", "Assets/Scripts/UI/TerminalUIController.cs"),
        new MoveSpec("Assets/ViewSwitchController.cs", "Assets/Scripts/MinLoop/ViewSwitchController.cs"),

        new MoveSpec("Assets/ChatGPT Image Jul 14, 2026, 10_10_35 AM.png", "Assets/Art/UI/HearthHud/Finale/FamilyPhoto.png"),
        new MoveSpec("Assets/image.mat", "Assets/Art/UI/HearthHud/Finale/PhotoFrame_Legacy.mat"),
        new MoveSpec("Assets/GameObject.prefab", "Assets/Prefabs/_Legacy/GameObject_Legacy.prefab"),
    };

    [MenuItem(MenuRoot + "Organize SampleScene Hierarchy (Safe)")]
    public static void OrganizeSceneHierarchy()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("[HearthProjectOrganizer] No loaded scene is available.");
            return;
        }

        List<GameObject> roots = scene.GetRootGameObjects().ToList();
        for (int i = roots.Count - 1; i >= 0; i--)
        {
            GameObject root = roots[i];
            if (IsEmptySeparator(root))
            {
                Undo.DestroyObjectImmediate(root);
                roots.RemoveAt(i);
            }
        }

        Dictionary<string, GameObject> separators = new Dictionary<string, GameObject>(StringComparer.Ordinal);
        for (int i = 0; i < SceneSections.Length; i++)
        {
            GameObject separator = new GameObject(SceneSections[i].separator);
            Undo.RegisterCreatedObjectUndo(separator, "Create HEARTH hierarchy separator");
            separator.tag = "EditorOnly";
            separators.Add(SceneSections[i].separator, separator);
        }

        HashSet<GameObject> assigned = new HashSet<GameObject>();
        List<GameObject> deprecated = roots.Where(root => SceneSections[6].names.Contains(root.name)).ToList();
        List<GameObject> loose = roots.Where(root =>
            !SceneSections.Take(5).Any(section => section.names.Contains(root.name)) &&
            !SceneSections[6].names.Contains(root.name)).ToList();

        int siblingIndex = 0;
        for (int sectionIndex = 0; sectionIndex < SceneSections.Length; sectionIndex++)
        {
            SceneSection section = SceneSections[sectionIndex];
            separators[section.separator].transform.SetSiblingIndex(siblingIndex++);
            IEnumerable<GameObject> members;
            if (sectionIndex == 5)
            {
                members = loose;
            }
            else if (sectionIndex == 6)
            {
                members = deprecated;
            }
            else
            {
                members = roots.Where(root => section.names.Contains(root.name));
            }

            foreach (GameObject member in members)
            {
                if (member == null || !assigned.Add(member))
                {
                    continue;
                }

                Undo.RecordObject(member.transform, "Organize HEARTH root hierarchy");
                member.transform.SetSiblingIndex(siblingIndex++);
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[HearthProjectOrganizer] SampleScene root hierarchy organized without changing gameplay parent paths or object transforms.");
    }

    [MenuItem(MenuRoot + "Organize Assets Root (Preserve GUIDs)")]
    public static void OrganizeAssets()
    {
        int moved = 0;
        int skipped = 0;
        List<string> errors = new List<string>();
        AssetDatabase.StartAssetEditing();
        try
        {
            for (int i = 0; i < AssetMoves.Length; i++)
            {
                MoveSpec spec = AssetMoves[i];
                if (AssetDatabase.LoadMainAssetAtPath(spec.source) == null)
                {
                    if (AssetDatabase.LoadMainAssetAtPath(spec.destination) != null)
                    {
                        skipped++;
                    }
                    continue;
                }

                EnsureFolder(Path.GetDirectoryName(spec.destination).Replace('\\', '/'));
                if (AssetDatabase.LoadMainAssetAtPath(spec.destination) != null)
                {
                    errors.Add("Destination already exists: " + spec.destination);
                    continue;
                }

                string originalGuid = AssetDatabase.AssetPathToGUID(spec.source);
                string error = AssetDatabase.MoveAsset(spec.source, spec.destination);
                if (!string.IsNullOrEmpty(error))
                {
                    errors.Add(spec.source + " -> " + spec.destination + ": " + error);
                    continue;
                }

                string movedGuid = AssetDatabase.AssetPathToGUID(spec.destination);
                if (!string.Equals(originalGuid, movedGuid, StringComparison.Ordinal))
                {
                    errors.Add("GUID changed unexpectedly for " + spec.destination + ".");
                }
                moved++;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        DeleteKnownEmptyFolder("Assets/action");
        DeleteKnownEmptyFolder("Assets/Animations/Hearth17F03");
        DeleteKnownEmptyFolder("Assets/Animation/Hearth/17F02");
        DeleteKnownEmptyFolder("Assets/Animation/Hearth");
        DeleteKnownEmptyFolder("Assets/Animation");
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (errors.Count > 0)
        {
            Debug.LogError("[HearthProjectOrganizer] Asset organization moved " + moved + " item(s), skipped " + skipped +
                           ", with " + errors.Count + " issue(s):\n- " + string.Join("\n- ", errors.ToArray()));
        }
        else
        {
            Debug.Log("[HearthProjectOrganizer] Asset organization complete: " + moved +
                      " item(s) moved with GUIDs preserved; " + skipped + " already organized.");
        }
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
        }

        for (int i = 0; i < AssetMoves.Length; i++)
        {
            MoveSpec spec = AssetMoves[i];
            if (AssetDatabase.LoadMainAssetAtPath(spec.source) != null)
            {
                issues.Add("Root asset still needs organization: " + spec.source);
            }
            else if (AssetDatabase.LoadMainAssetAtPath(spec.destination) == null)
            {
                issues.Add("Organized asset is missing: " + spec.destination);
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

    private static void EnsureFolder(string folderPath)
    {
        if (string.IsNullOrEmpty(folderPath) || AssetDatabase.IsValidFolder(folderPath))
        {
            return;
        }

        string parent = Path.GetDirectoryName(folderPath).Replace('\\', '/');
        string name = Path.GetFileName(folderPath);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }

    private static void DeleteKnownEmptyFolder(string assetFolder)
    {
        if (!AssetDatabase.IsValidFolder(assetFolder))
        {
            return;
        }

        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string absolute = Path.Combine(projectRoot, assetFolder.Replace('/', Path.DirectorySeparatorChar));
        bool hasContent = Directory.EnumerateFileSystemEntries(absolute)
            .Any(path => !path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase));
        if (!hasContent)
        {
            AssetDatabase.DeleteAsset(assetFolder);
        }
    }
}
#endif
