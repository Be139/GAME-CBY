using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CityLotAssetPlacer))]
public class CityLotAssetPlacerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CityLotAssetPlacer placer = (CityLotAssetPlacer)target;

        EditorGUILayout.Space(12f);
        EditorGUILayout.HelpBox("Height-controlled high-rise placement is enabled by default. Use Auto Fill to collect imported building prefabs, then Generate to build the selected dikuai lots.", MessageType.Info);

        if (GUILayout.Button("Find dikuai Lot Root"))
        {
            Undo.RecordObject(placer, "Find dikuai Lot Root");
            placer.FindLotRoot();
            EditorUtility.SetDirty(placer);
        }

        if (GUILayout.Button("Auto Fill Imported Building Prefabs"))
        {
            Undo.RecordObject(placer, "Auto Fill Imported Building Prefabs");
            placer.AutoFillDefaultBuildingPrefabs();
            EditorUtility.SetDirty(placer);
        }

        if (GUILayout.Button("Generate Asset Placement Rules"))
        {
            placer.GeneratePlacement();
        }

        if (GUILayout.Button("Clear Generated Asset Placement"))
        {
            placer.ClearPlacement();
        }
    }
}
