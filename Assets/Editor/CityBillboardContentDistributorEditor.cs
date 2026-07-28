using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(CityBillboardContentDistributor))]
public class CityBillboardContentDistributorEditor : Editor
{
    private const string DefaultImageFolder = "Assets/Art/Environment/Billboards/AIImages";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CityBillboardContentDistributor distributor = (CityBillboardContentDistributor)target;

        EditorGUILayout.Space(12f);
        EditorGUILayout.HelpBox(
            "Image Weight 7 and Animation Weight 3 means about 70% images and 30% looping VideoClips. Add imported textures and videos to the two pools, then redistribute without moving or recreating the billboards.",
            MessageType.Info);

        if (GUILayout.Button("Find Billboard Root"))
        {
            Undo.RecordObject(distributor, "Find billboard root");
            distributor.FindBillboardRoot();
            EditorUtility.SetDirty(distributor);
        }

        if (GUILayout.Button("Prepare Existing Billboard Screens"))
        {
            distributor.PrepareExistingBillboards();
        }

        if (GUILayout.Button("Apply HDR Material To All Screens"))
        {
            distributor.ApplySurfaceMaterialToAll();
        }

        if (GUILayout.Button("Redistribute Images And Animations"))
        {
            distributor.RedistributeAll();
        }

        if (GUILayout.Button("Import AI Images And Redistribute"))
        {
            ImportAiImagesAndRedistribute();
        }

        if (GUILayout.Button("Clear Media Assignments"))
        {
            distributor.ClearAllContent();
        }
    }

    [MenuItem("Tools/City/Billboards/Import AI Images And Redistribute")]
    public static void ImportAiImagesAndRedistribute()
    {
        CityBillboardContentDistributor distributor =
            UnityEngine.Object.FindObjectOfType<CityBillboardContentDistributor>(true);
        if (distributor == null)
        {
            Debug.LogError("City billboard images: CityBillboardContentDistributor was not found in the loaded scene.");
            return;
        }

        Texture[] images = LoadImagesFromDefaultFolder();
        if (images.Length == 0)
        {
            Debug.LogError("City billboard images: no Texture assets were found in " + DefaultImageFolder + ".");
            return;
        }

        Undo.RecordObject(distributor, "Import AI billboard images");
        distributor.SetImageContents(images, false);
        distributor.FindBillboardRoot();
        distributor.PrepareExistingBillboards();
        distributor.ApplySurfaceMaterialToAll();
        distributor.RedistributeAll();
        EditorUtility.SetDirty(distributor);

        Scene scene = distributor.gameObject.scene;
        if (scene.IsValid() && scene.isLoaded)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        AssetDatabase.SaveAssets();
        Debug.Log(
            "City billboard images: imported " + images.Length +
            " AI images and redistributed them by billboard aspect ratio.");
    }

    private static Texture[] LoadImagesFromDefaultFolder()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { DefaultImageFolder });
        List<Texture> images = new List<Texture>();

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            ConfigureBillboardTextureImporter(path);
            Texture texture = AssetDatabase.LoadAssetAtPath<Texture>(path);
            if (texture != null)
            {
                images.Add(texture);
            }
        }

        images.Sort((left, right) => string.Compare(left.name, right.name, StringComparison.Ordinal));
        return images.ToArray();
    }

    private static void ConfigureBillboardTextureImporter(string assetPath)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        bool changed =
            importer.textureType != TextureImporterType.Default ||
            importer.npotScale != TextureImporterNPOTScale.None ||
            !importer.sRGBTexture ||
            !importer.mipmapEnabled ||
            importer.wrapMode != TextureWrapMode.Clamp ||
            importer.maxTextureSize != 2048;

        if (!changed)
        {
            return;
        }

        importer.textureType = TextureImporterType.Default;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.sRGBTexture = true;
        importer.mipmapEnabled = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.maxTextureSize = 2048;
        importer.SaveAndReimport();
    }
}
