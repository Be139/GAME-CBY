using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Creates and applies the two production typography roles used by HEARTH V2.
/// Oxanium is the UI language; Chakra Petch is reserved for spoken body copy.
/// Layout, text content, story references and active state are never changed.
/// </summary>
public static class HearthTypographyTools
{
    private const string MenuRoot = "Tools/Hearth/Validation/UI/";
    private const string UiFontSourcePath =
        "Assets/UI/HEARTH/V2/Fonts/Oxanium/Oxanium-VariableFont_wght.ttf";
    private const string UiFontAssetPath =
        "Assets/UI/HEARTH/V2/Fonts/Oxanium/Oxanium_UI SDF.asset";
    private const string DialogueFontSourcePath =
        "Assets/UI/HEARTH/V2/Fonts/ChakraPetch/ChakraPetch-Regular.ttf";
    private const string DialogueFontAssetPath =
        "Assets/UI/HEARTH/V2/Fonts/ChakraPetch/ChakraPetch_Dialogue SDF.asset";
    private const string ThemePath =
        "Assets/UI/HEARTH/V2/Profiles/Hearth_UiV2Theme.asset";

    private static readonly string[] ProductionPrefabPaths =
    {
        "Assets/Prefabs/UI/HearthHud/V2/HearthHudRoot_V2.prefab",
        "Assets/Prefabs/UI/HearthHud/V2/Companion/HearthCompanionHudRoot_V2.prefab",
        "Assets/Prefabs/UI/HearthHud/V2/Inspection/Hearth17F03InspectionPanel_V2.prefab",
        "Assets/Prefabs/UI/HearthHud/V2/PhotoArchive/HearthPhotoArchiveWorldView_V2.prefab",
        "Assets/Prefabs/UI/HearthHud/V2/Terminals/Terminal_17F01_V2.prefab",
        "Assets/Prefabs/UI/HearthHud/V2/Terminals/Terminal_17F02_V2.prefab",
        "Assets/Prefabs/UI/HearthHud/V2/Terminals/Terminal_17F03_Alert_V2.prefab",
        "Assets/Prefabs/UI/HearthHud/V2/Terminals/Terminal_17F04_Home_V2.prefab",
        "Assets/Prefabs/UI/HearthHud/V2/Terminals/Terminal_Lobby_Assignment_V2.prefab",
        "Assets/Prefabs/UI/HearthSubtitle/V2/HearthSubtitleVisualCanvas_V2.prefab"
    };

    /// <summary>
    /// Reapplies semantic font roles without changing layout, text or story data.
    /// Open scenes are marked dirty but are deliberately not auto-saved.
    /// </summary>

    [MenuItem(MenuRoot + "Validate Production Fonts")]
    public static void ValidateProductionFonts()
    {
        HearthUiThemeProfile theme = AssetDatabase.LoadAssetAtPath<HearthUiThemeProfile>(ThemePath);
        if (!HasBothFonts(theme))
        {
            Debug.LogError("[HEARTH Typography] Validation failed: the theme does not contain both font roles.");
            return;
        }

        List<string> errors = new List<string>();
        int checkedTexts = 0;
        foreach (string prefabPath in ProductionPrefabPaths)
        {
            if (!File.Exists(prefabPath))
            {
                errors.Add("Missing production prefab: " + prefabPath);
                continue;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                ValidateHierarchy(root, prefabPath, theme, errors, ref checkedTexts);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
        {
            Scene scene = SceneManager.GetSceneAt(sceneIndex);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                continue;
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                ValidateHierarchy(root, "Scene: " + scene.name, theme, errors, ref checkedTexts);
            }
        }

        if (errors.Count == 0)
        {
            Debug.Log(
                "[HEARTH Typography] Validation passed. " + checkedTexts +
                " TMP text components use the expected Oxanium/Chakra Petch role.");
            return;
        }

        Debug.LogError(
            "[HEARTH Typography] Validation found " + errors.Count + " issue(s):\n- " +
            string.Join("\n- ", errors));
    }

    private static void ValidateHierarchy(
        GameObject root,
        string sourceLabel,
        HearthUiThemeProfile theme,
        List<string> errors,
        ref int checkedTexts)
    {
        HashSet<TMP_Text> dialogueBodies = new HashSet<TMP_Text>();
        foreach (HearthDialogueSurface surface in root.GetComponentsInChildren<HearthDialogueSurface>(true))
        {
            if (surface.BodyText != null)
            {
                dialogueBodies.Add(surface.BodyText);
            }
        }

        foreach (HearthSubtitleViewBindings bindings in root.GetComponentsInChildren<HearthSubtitleViewBindings>(true))
        {
            if (bindings.BodyText != null)
            {
                dialogueBodies.Add(bindings.BodyText);
            }
        }

        foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
        {
            TMP_FontAsset expected = dialogueBodies.Contains(text)
                ? theme.DialogueFontAsset
                : theme.UiFontAsset;
            checkedTexts++;
            if (text.font != expected)
            {
                errors.Add(
                    sourceLabel + " / " + GetHierarchyPath(text.transform) +
                    " expected " + expected.name + " but uses " +
                    (text.font != null ? text.font.name : "<null>"));
            }
        }
    }

    private static string GetHierarchyPath(Transform target)
    {
        if (target == null)
        {
            return "<missing>";
        }

        string path = target.name;
        Transform parent = target.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }

    private static bool HasBothFonts(HearthUiThemeProfile theme)
    {
        return theme != null &&
            theme.UiFontAsset != null &&
            theme.DialogueFontAsset != null;
    }
}
