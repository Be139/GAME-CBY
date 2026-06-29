using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CityFacadeBillboardPlacer))]
public class CityFacadeBillboardPlacerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CityFacadeBillboardPlacer placer = (CityFacadeBillboardPlacer)target;

        EditorGUILayout.Space(12f);
        EditorGUILayout.HelpBox("Creates blank facade billboard placeholders under one shared root by default. Re-running can skip buildings that already have billboards, so hand-edited signs are not duplicated. Replace each billboard's ContentRoot later with image, video, or custom sign prefabs.", MessageType.Info);

        if (GUILayout.Button("Find Generated City Roots"))
        {
            Undo.RecordObject(placer, "Find generated city roots");
            placer.FindGeneratedCityRoots();
            EditorUtility.SetDirty(placer);
        }

        if (GUILayout.Button("Find Manual BUILDING Roots"))
        {
            Undo.RecordObject(placer, "Find manual building roots");
            placer.FindManualBuildingRoots();
            EditorUtility.SetDirty(placer);
        }

        if (GUILayout.Button("Generate Blank Facade Billboards"))
        {
            placer.GenerateBillboards();
        }

        if (GUILayout.Button("Clear Orphan Facade Billboards"))
        {
            placer.ClearOrphanBillboards();
        }

        if (GUILayout.Button("Clear Generated Facade Billboards"))
        {
            placer.ClearBillboards();
        }
    }
}
