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
    private const string MenuRoot = "Tools/Hearth/Production UI/Typography/";
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

    [MenuItem(MenuRoot + "Install And Apply Production Fonts")]
    public static void InstallAndApplyProductionFonts()
    {
        AssetDatabase.ImportAsset(UiFontSourcePath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.ImportAsset(DialogueFontSourcePath, ImportAssetOptions.ForceUpdate);

        TMP_FontAsset uiFont = EnsureTmpFontAsset(
            UiFontSourcePath,
            UiFontAssetPath,
            "Oxanium_UI SDF");
        TMP_FontAsset dialogueFont = EnsureTmpFontAsset(
            DialogueFontSourcePath,
            DialogueFontAssetPath,
            "ChakraPetch_Dialogue SDF");

        if (uiFont == null || dialogueFont == null)
        {
            Debug.LogError(
                "[HEARTH Typography] Font installation stopped because one or more source fonts could not be loaded.");
            return;
        }

        HearthUiThemeProfile theme = AssignThemeFonts(uiFont, dialogueFont);
        if (theme == null)
        {
            return;
        }

        ApplyProductionTypography(includeOpenScenes: true, logResult: true);
        ValidateProductionFonts();
    }

    [MenuItem(MenuRoot + "Apply Production Fonts")]
    public static void ApplyProductionFontsMenu()
    {
        ApplyProductionTypography(includeOpenScenes: true, logResult: true);
    }

    /// <summary>
    /// Reapplies semantic font roles without changing layout, text or story data.
    /// Open scenes are marked dirty but are deliberately not auto-saved.
    /// </summary>
    public static bool ApplyProductionTypography(
        bool includeOpenScenes = true,
        bool logResult = true)
    {
        HearthUiThemeProfile theme = AssetDatabase.LoadAssetAtPath<HearthUiThemeProfile>(ThemePath);
        if (!HasBothFonts(theme))
        {
            Debug.LogError(
                "[HEARTH Typography] Theme fonts are missing. Run Install And Apply Production Fonts first.");
            return false;
        }

        int prefabCount = 0;
        int textCount = 0;
        foreach (string prefabPath in ProductionPrefabPaths)
        {
            if (!File.Exists(prefabPath))
            {
                Debug.LogWarning("[HEARTH Typography] Production prefab is missing: " + prefabPath);
                continue;
            }

            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                int changed = ApplyHierarchy(root, theme, assignRuntimeProfiles: true);
                if (changed > 0)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                    textCount += changed;
                }
                prefabCount++;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        int sceneCount = 0;
        if (includeOpenScenes)
        {
            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.IsValid() || !scene.isLoaded)
                {
                    continue;
                }

                int changedInScene = 0;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    changedInScene += ApplyHierarchy(root, theme, assignRuntimeProfiles: true);
                }

                if (changedInScene > 0)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    textCount += changedInScene;
                }
                sceneCount++;
            }
        }

        AssetDatabase.SaveAssets();
        if (logResult)
        {
            Debug.Log(
                "[HEARTH Typography] Applied Oxanium UI and Chakra Petch dialogue roles to " +
                prefabCount + " production prefabs and " + sceneCount +
                " open scenes (" + textCount + " text/profile assignments). " +
                "Open scenes were not auto-saved.");
        }

        return true;
    }

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

    private static TMP_FontAsset EnsureTmpFontAsset(
        string sourcePath,
        string targetPath,
        string assetName)
    {
        TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(targetPath);
        if (existing != null)
        {
            return existing;
        }

        Font sourceFont = AssetDatabase.LoadAssetAtPath<Font>(sourcePath);
        if (sourceFont == null)
        {
            Debug.LogError("[HEARTH Typography] Source font was not imported: " + sourcePath);
            return null;
        }

        TMP_FontAsset created = TMP_FontAsset.CreateFontAsset(sourceFont);
        if (created == null)
        {
            Debug.LogError("[HEARTH Typography] TMP could not create a font asset from: " + sourcePath);
            return null;
        }

        created.name = assetName;
        created.atlasPopulationMode = AtlasPopulationMode.Dynamic;
        AssetDatabase.CreateAsset(created, targetPath);

        Texture2D atlas = created.atlasTexture;
        if (atlas != null && !AssetDatabase.Contains(atlas))
        {
            atlas.name = assetName + " Atlas";
            AssetDatabase.AddObjectToAsset(atlas, created);
        }

        Material material = created.material;
        if (material != null && !AssetDatabase.Contains(material))
        {
            material.name = assetName + " Material";
            AssetDatabase.AddObjectToAsset(material, created);
        }

        EditorUtility.SetDirty(created);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(targetPath, ImportAssetOptions.ForceUpdate);
        return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(targetPath);
    }

    private static HearthUiThemeProfile AssignThemeFonts(
        TMP_FontAsset uiFont,
        TMP_FontAsset dialogueFont)
    {
        HearthUiThemeProfile theme = AssetDatabase.LoadAssetAtPath<HearthUiThemeProfile>(ThemePath);
        if (theme == null)
        {
            Debug.LogError("[HEARTH Typography] Theme profile is missing: " + ThemePath);
            return null;
        }

        SerializedObject serializedTheme = new SerializedObject(theme);
        serializedTheme.FindProperty("primaryFontAsset").objectReferenceValue = uiFont;
        serializedTheme.FindProperty("dialogueFontAsset").objectReferenceValue = dialogueFont;
        serializedTheme.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(theme);
        AssetDatabase.SaveAssets();
        return theme;
    }

    private static int ApplyHierarchy(
        GameObject root,
        HearthUiThemeProfile theme,
        bool assignRuntimeProfiles)
    {
        if (root == null)
        {
            return 0;
        }

        int changed = 0;
        foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
        {
            if (text.font != theme.UiFontAsset)
            {
                text.font = theme.UiFontAsset;
                EditorUtility.SetDirty(text);
                changed++;
            }
        }

        foreach (HearthDialogueSurface surface in root.GetComponentsInChildren<HearthDialogueSurface>(true))
        {
            changed += ApplyFont(surface.SpeakerText, theme.UiFontAsset);
            changed += ApplyFont(surface.AdvanceHintText, theme.UiFontAsset);
            changed += ApplyFont(surface.BodyText, theme.DialogueFontAsset);
        }

        foreach (HearthSubtitleViewBindings bindings in root.GetComponentsInChildren<HearthSubtitleViewBindings>(true))
        {
            changed += ApplyFont(bindings.SpeakerText, theme.UiFontAsset);
            changed += ApplyFont(bindings.AdvanceHintText, theme.UiFontAsset);
            changed += ApplyFont(bindings.PersistentSceneHeaderText, theme.UiFontAsset);
            changed += ApplyFont(bindings.BodyText, theme.DialogueFontAsset);
        }

        if (assignRuntimeProfiles)
        {
            foreach (MinLoopSubtitlePlayer player in root.GetComponentsInChildren<MinLoopSubtitlePlayer>(true))
            {
                changed += AssignObjectReference(player, "uiThemeProfile", theme);
            }

            foreach (HearthTvTerminalController controller in root.GetComponentsInChildren<HearthTvTerminalController>(true))
            {
                changed += AssignObjectReference(controller, "uiThemeProfile", theme);
            }

            foreach (HearthPhotoArchiveWorldView archive in root.GetComponentsInChildren<HearthPhotoArchiveWorldView>(true))
            {
                changed += AssignObjectReference(archive, "uiThemeProfile", theme);
            }

            foreach (Hearth17F03InspectionPanel panel in root.GetComponentsInChildren<Hearth17F03InspectionPanel>(true))
            {
                changed += AssignObjectReference(panel, "secondUiTheme", theme);
            }
        }

        return changed;
    }

    private static int ApplyFont(TMP_Text text, TMP_FontAsset font)
    {
        if (text == null || font == null || text.font == font)
        {
            return 0;
        }

        text.font = font;
        EditorUtility.SetDirty(text);
        return 1;
    }

    private static int AssignObjectReference(
        UnityEngine.Object target,
        string propertyName,
        UnityEngine.Object value)
    {
        if (target == null)
        {
            return 0;
        }

        SerializedObject serializedTarget = new SerializedObject(target);
        SerializedProperty property = serializedTarget.FindProperty(propertyName);
        if (property == null || property.objectReferenceValue == value)
        {
            return 0;
        }

        property.objectReferenceValue = value;
        serializedTarget.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(target);
        return 1;
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
