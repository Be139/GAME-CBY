#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Applies deterministic Unity import settings to the approved transparent V2
/// vector-PNG library. Fixed-purpose frames are exported at their exact design
/// size, while older shared assets retain their existing sliced borders.
/// </summary>
public static class HearthUiV2VectorAssetEditor
{
    public const string Root =
        "Assets/UI/HEARTH/V2/VectorParts/";

    private static readonly Dictionary<string, Vector4> SlicedBorders =
        new Dictionary<string, Vector4>
        {
            { "Common/HUD_Common_ButtonFrame_320x72.png", Border(32f) },
            { "Common/HUD_Common_PanelFrame_520x320.png", Border(40f) },
            {
                "Feedback/HUD_Feedback_FieldUnitToastFrame_640x400.png",
                Border(40f)
            },
            {
                "Feedback/HUD_Feedback_PleaseWaitFrame_420x96.png",
                Border(32f)
            },
            {
                "Feedback/HUD_Feedback_TrustToastFrame_420x120.png",
                Border(32f)
            },
            {
                "Feedback/HUD_Feedback_WarningModalFrame_720x360.png",
                Border(44f)
            },
            { "Finale/HUD_Finale_PhotoFrame_1280x720.png", Border(48f) },
            {
                "Finale/HUD_Finale_ShutdownModalFrame_720x420.png",
                Border(44f)
            },
            {
                "Finale/HUD_Finale_VirusPopup_Phase01_560x260.png",
                Border(40f)
            },
            {
                "Finale/HUD_Finale_VirusPopup_Phase02_560x260.png",
                Border(40f)
            },
            {
                "Finale/HUD_Finale_VirusPopup_Phase03_560x260.png",
                Border(40f)
            },
            {
                "Inspection/HUD_Inspection_DiagnosticViewportFrame_840x520.png",
                Border(44f)
            },
            {
                "Interaction/HUD_Interaction_ChoiceHintFrame_620x96.png",
                Border(32f)
            },
            {
                "Interaction/HUD_Interaction_GazePromptFrame_520x128.png",
                Border(32f)
            },
            {
                "Interaction/HUD_Interaction_TapFrame_420x112.png",
                Border(32f)
            },
            {
                "Terminal/HUD_Terminal_InfoPanelFrame_520x320.png",
                Border(44f)
            },
            {
                "Terminal/HUD_Terminal_PortraitFrame_240x400.png",
                Border(40f)
            }
        };

    [MenuItem("Tools/Hearth/UI V2/Vector Art/Prepare Imported Sprites")]
    public static void PrepareImportedSprites()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { Root });
        int configured = 0;
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            TextureImporter importer =
                AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
            {
                continue;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression =
                TextureImporterCompression.Uncompressed;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 4096;
            importer.spritePixelsPerUnit = 100f;

            string relative = path.Substring(Root.Length);
            Vector4 border;
            importer.spriteBorder =
                SlicedBorders.TryGetValue(relative, out border)
                    ? border
                    : Vector4.zero;
            importer.SaveAndReimport();
            configured++;
        }

        Debug.Log(
            "[HearthUiV2VectorAssetEditor] Prepared " +
            configured +
            " transparent V2 vector PNG sprites.");
    }

    [MenuItem("Tools/Hearth/UI V2/Vector Art/Validate Imported Sprites")]
    public static void ValidateImportedSprites()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { Root });
        List<string> issues = new List<string>();

        if (guids.Length != 39)
        {
            issues.Add(
                "Expected 39 vector PNGs but Unity currently sees " +
                guids.Length +
                ".");
        }

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            TextureImporter importer =
                AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null ||
                importer.textureType != TextureImporterType.Sprite ||
                importer.spriteImportMode != SpriteImportMode.Single ||
                importer.mipmapEnabled ||
                !importer.alphaIsTransparency)
            {
                issues.Add("Invalid sprite import settings: " + path);
            }
        }

        if (issues.Count == 0)
        {
            Debug.Log(
                "[HearthUiV2VectorAssetEditor] Validation passed for all " +
                "39 imported vector PNG sprites.");
        }
        else
        {
            Debug.LogError(
                "[HearthUiV2VectorAssetEditor] Validation found " +
                issues.Count +
                " issue(s):\n- " +
                string.Join("\n- ", issues));
        }
    }

    private static Vector4 Border(float value)
    {
        return new Vector4(value, value, value, value);
    }
}
#endif
