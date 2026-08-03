#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class HearthDialoguePresentationBinder
{
    public const string SharedProfilePath = "Assets/Data/MinLoop/UI/Hearth_SubtitleStyle.asset";

    [MenuItem(HearthLegacyToolGuard.MenuRoot + "Dialogue/Apply Shared Subtitle Presentation")]
    public static void ApplySharedPresentation()
    {
        if (!HearthLegacyToolGuard.Confirm(
                "Apply Shared Subtitle Presentation",
                "all subtitle player bindings in the active scene"))
        {
            return;
        }

        if (Application.isPlaying)
        {
            Debug.LogError("[HearthDialoguePresentationBinder] Exit Play Mode before applying subtitle presentation.");
            return;
        }

        HearthSubtitleStyleProfile profile = EnsureSharedProfile();
        MinLoopSubtitlePlayer standardPlayer = FindCanonicalStandardPlayer();
        if (standardPlayer == null)
        {
            Debug.LogError("[HearthDialoguePresentationBinder] No standard MinLoopSubtitlePlayer exists in the open scene.");
            return;
        }

        MinLoopSubtitlePlayer[] players = Object.FindObjectsOfType<MinLoopSubtitlePlayer>(true);
        for (int i = 0; i < players.Length; i++)
        {
            MinLoopSubtitlePlayer player = players[i];
            bool epilogue = IsEpiloguePlayer(player);
            ConfigurePlayer(
                player,
                profile,
                epilogue ? HearthSubtitlePresentationMode.CenteredEpilogue : HearthSubtitlePresentationMode.StandardDialogue,
                epilogue ? 9100 : 8500);
        }

        BindField(Object.FindObjectOfType<HearthCompanion17F01ReplayController>(true), "subtitlePlayer", standardPlayer);
        BindField(Object.FindObjectOfType<HearthCompanion17F02ReplayController>(true), "subtitlePlayer", standardPlayer);
        BindField(Object.FindObjectOfType<HearthCompanion17F03ReplayController>(true), "subtitlePlayer", standardPlayer);
        BindField(Object.FindObjectOfType<ReplaySequenceController>(true), "subtitlePlayer", standardPlayer);

        Hearth17F04FinaleController finale = Object.FindObjectOfType<Hearth17F04FinaleController>(true);
        if (finale != null)
        {
            BindField(finale, "sceneSubtitlePlayer", standardPlayer);
            MinLoopSubtitlePlayer epiloguePlayer = players.FirstOrDefault(IsEpiloguePlayer);
            if (epiloguePlayer != null)
            {
                BindField(finale, "epilogueSubtitlePlayer", epiloguePlayer);
            }
        }

        RemoveObsolete17F04ScenePlayer(standardPlayer);
        AssetDatabase.SaveAssets();
        Scene scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[HearthDialoguePresentationBinder] All normal dialogue now uses one standard subtitle player and the shared subtitle style profile. Epilogue dialogue uses the centered mode of the same profile. Review and save the scene manually.");
        ValidateSharedPresentation();
    }

    [MenuItem("Tools/Hearth/Dialogue/Validate Shared Subtitle Presentation")]
    public static void ValidateSharedPresentation()
    {
        List<string> issues = new List<string>();
        HearthSubtitleStyleProfile profile = AssetDatabase.LoadAssetAtPath<HearthSubtitleStyleProfile>(SharedProfilePath);
        MinLoopSubtitlePlayer standardPlayer = FindCanonicalStandardPlayer();
        if (profile == null) issues.Add("Shared subtitle style profile is missing.");
        if (standardPlayer == null) issues.Add("Canonical standard subtitle player is missing.");

        ValidateBinding(Object.FindObjectOfType<HearthCompanion17F01ReplayController>(true), "subtitlePlayer", standardPlayer, "17F01", issues);
        ValidateBinding(Object.FindObjectOfType<HearthCompanion17F02ReplayController>(true), "subtitlePlayer", standardPlayer, "17F02", issues);
        ValidateBinding(Object.FindObjectOfType<HearthCompanion17F03ReplayController>(true), "subtitlePlayer", standardPlayer, "17F03", issues);
        ValidateBinding(Object.FindObjectOfType<Hearth17F04FinaleController>(true), "sceneSubtitlePlayer", standardPlayer, "17F04 scene dialogue", issues);

        MinLoopSubtitlePlayer[] players = Object.FindObjectsOfType<MinLoopSubtitlePlayer>(true);
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].StyleProfile != profile)
            {
                issues.Add(players[i].name + " is not bound to the shared subtitle style profile.");
            }
        }

        if (issues.Count == 0)
        {
            Debug.Log("[HearthDialoguePresentationBinder] Validation passed: all four households share one normal subtitle player and one editable style profile.");
            return;
        }

        Debug.LogError("[HearthDialoguePresentationBinder] Validation found " + issues.Count + " issue(s):\n- " + string.Join("\n- ", issues));
    }

    public static HearthSubtitleStyleProfile EnsureSharedProfile()
    {
        EnsureFolder("Assets/Data");
        EnsureFolder("Assets/Data/MinLoop");
        EnsureFolder("Assets/Data/MinLoop/UI");
        HearthSubtitleStyleProfile profile = AssetDatabase.LoadAssetAtPath<HearthSubtitleStyleProfile>(SharedProfilePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<HearthSubtitleStyleProfile>();
            AssetDatabase.CreateAsset(profile, SharedProfilePath);
        }

        return profile;
    }

    public static MinLoopSubtitlePlayer FindCanonicalStandardPlayer()
    {
        MinLoopSubtitlePlayer[] players = Object.FindObjectsOfType<MinLoopSubtitlePlayer>(true);
        return players.FirstOrDefault(item => item != null && item.name == "MinLoopSubtitlePlayer") ??
               players.FirstOrDefault(item => item != null && !IsEpiloguePlayer(item));
    }

    public static void ConfigurePlayer(
        MinLoopSubtitlePlayer player,
        HearthSubtitleStyleProfile profile,
        HearthSubtitlePresentationMode mode,
        int sortingOrder)
    {
        if (player == null)
        {
            return;
        }

        SerializedObject so = new SerializedObject(player);
        so.FindProperty("styleProfile").objectReferenceValue = profile;
        so.FindProperty("presentationMode").enumValueIndex = (int)mode;
        so.FindProperty("useCleanCenteredStyle").boolValue = true;
        so.FindProperty("forceSubtitleCanvasSorting").boolValue = true;
        so.FindProperty("subtitleSortingOrder").intValue = sortingOrder;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(player);
    }

    private static bool IsEpiloguePlayer(MinLoopSubtitlePlayer player)
    {
        return player != null && player.name.IndexOf("Epilogue", System.StringComparison.OrdinalIgnoreCase) >= 0;
    }

    private static void BindField(Object target, string fieldName, Object value)
    {
        if (target == null)
        {
            return;
        }

        SerializedObject so = new SerializedObject(target);
        SerializedProperty property = so.FindProperty(fieldName);
        if (property == null)
        {
            Debug.LogWarning("[HearthDialoguePresentationBinder] Missing field " + fieldName + " on " + target.name + ".");
            return;
        }

        property.objectReferenceValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
    }

    private static void ValidateBinding(Object target, string fieldName, Object expected, string label, List<string> issues)
    {
        if (target == null)
        {
            issues.Add(label + " controller is missing.");
            return;
        }

        SerializedObject so = new SerializedObject(target);
        SerializedProperty property = so.FindProperty(fieldName);
        if (property == null || property.objectReferenceValue != expected)
        {
            issues.Add(label + " is not bound to the canonical standard subtitle player.");
        }
    }

    private static void RemoveObsolete17F04ScenePlayer(MinLoopSubtitlePlayer standardPlayer)
    {
        MinLoopSubtitlePlayer[] players = Object.FindObjectsOfType<MinLoopSubtitlePlayer>(true);
        for (int i = 0; i < players.Length; i++)
        {
            MinLoopSubtitlePlayer player = players[i];
            if (player != null && player != standardPlayer && player.name == "SceneDialogue_17F04")
            {
                Undo.DestroyObjectImmediate(player.gameObject);
            }
        }
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        int split = path.LastIndexOf('/');
        string parent = path.Substring(0, split);
        string name = path.Substring(split + 1);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }
}
#endif
