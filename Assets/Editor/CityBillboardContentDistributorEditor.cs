using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CityBillboardContentDistributor))]
public class CityBillboardContentDistributorEditor : Editor
{
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

        if (GUILayout.Button("Clear Media Assignments"))
        {
            distributor.ClearAllContent();
        }
    }
}
