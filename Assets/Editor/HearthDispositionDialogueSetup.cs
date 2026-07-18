#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class HearthDispositionDialogueSetup
{
    private const string SupplementalRoot = "Assets/Data/MinLoop/Dialogues/FinalScriptSupplemental/";

    public static void ConfigureEarlyHouseholds(MinLoopFlowController flow, MinLoopSubtitlePlayer subtitlePlayer)
    {
        if (flow == null)
        {
            return;
        }

        SerializedObject serialized = new SerializedObject(flow);
        SerializedProperty subtitle = serialized.FindProperty("subtitlePlayer");
        if (subtitle != null)
        {
            subtitle.objectReferenceValue = subtitlePlayer;
        }

        SerializedProperty progress = serialized.FindProperty("householdProgress");
        if (progress != null)
        {
            progress.objectReferenceValue = Object.FindObjectOfType<HearthHouseholdProgressState>(true);
        }

        SerializedProperty sets = serialized.FindProperty("dispositionDialogueSets");
        if (sets == null)
        {
            return;
        }

        sets.arraySize = 2;
        ConfigureSet(
            sets.GetArrayElementAtIndex(0),
            "17F01",
            "17F01_TerminalSignoffIntro",
            "17F01_TerminalSignoff_A",
            "17F01_TerminalSignoff_B");
        ConfigureSet(
            sets.GetArrayElementAtIndex(1),
            "17F02",
            "17F02_TerminalSignoffIntro",
            "17F02_TerminalSignoff_A",
            "17F02_TerminalSignoff_B");
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(flow);
    }

    private static void ConfigureSet(
        SerializedProperty set,
        string residentId,
        string briefingId,
        string optionAId,
        string optionBId)
    {
        set.FindPropertyRelative("residentId").stringValue = residentId;
        set.FindPropertyRelative("preChoiceBriefing").objectReferenceValue = Load(briefingId);
        set.FindPropertyRelative("optionAResult").objectReferenceValue = Load(optionAId);
        set.FindPropertyRelative("optionBResult").objectReferenceValue = Load(optionBId);
        set.FindPropertyRelative("postChoiceCommon").objectReferenceValue = null;
    }

    private static HearthDialogueSequence Load(string id)
    {
        return AssetDatabase.LoadAssetAtPath<HearthDialogueSequence>(SupplementalRoot + id + ".asset");
    }
}
#endif
