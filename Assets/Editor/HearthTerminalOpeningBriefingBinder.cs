#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class HearthTerminalOpeningBriefingBinder
{
    private const string DialogueRoot = "Assets/Data/MinLoop/Dialogues/FinalScriptSupplemental/";

    [MenuItem("Tools/Hearth/Terminals/Apply Household Opening Briefings")]
    public static void ApplyHouseholdOpeningBriefings()
    {
        if (Application.isPlaying)
        {
            Debug.LogError("[HearthTerminalOpeningBriefingBinder] Exit Play Mode before applying terminal briefings.");
            return;
        }

        MinLoopSubtitlePlayer subtitlePlayer = Object.FindObjectOfType<MinLoopSubtitlePlayer>(true);
        if (subtitlePlayer == null)
        {
            Debug.LogError("[HearthTerminalOpeningBriefingBinder] No shared MinLoopSubtitlePlayer exists in the open scene.");
            return;
        }

        Dictionary<string, string> dialogueByResident = new Dictionary<string, string>
        {
            { "17F01", DialogueRoot + "17F01_TerminalIntro.asset" },
            { "17F02", DialogueRoot + "17F02_TerminalIntro.asset" },
            { "17F03", DialogueRoot + "17F03_CorridorTerminal.asset" }
        };

        int configured = 0;
        HearthTvTerminalController[] terminals = Object.FindObjectsOfType<HearthTvTerminalController>(true);
        for (int i = 0; i < terminals.Length; i++)
        {
            HearthTvTerminalController terminal = terminals[i];
            string residentId = ReadResidentId(terminal);
            if (!dialogueByResident.TryGetValue(residentId, out string dialoguePath))
            {
                continue;
            }

            HearthDialogueSequence sequence = AssetDatabase.LoadAssetAtPath<HearthDialogueSequence>(dialoguePath);
            if (sequence == null)
            {
                Debug.LogWarning("[HearthTerminalOpeningBriefingBinder] Missing dialogue asset: " + dialoguePath, terminal);
                continue;
            }

            HearthTerminalOpeningBriefing briefing = terminal.GetComponent<HearthTerminalOpeningBriefing>();
            if (briefing == null)
            {
                briefing = Undo.AddComponent<HearthTerminalOpeningBriefing>(terminal.gameObject);
            }

            EnsureRuntimePrompt(terminal);
            briefing.Configure(terminal, subtitlePlayer, sequence);
            EditorUtility.SetDirty(briefing);
            configured++;
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        Debug.Log("[HearthTerminalOpeningBriefingBinder] Configured household opening briefings: " + configured + ".");
    }

    private static string ReadResidentId(HearthTvTerminalController terminal)
    {
        SerializedObject serialized = new SerializedObject(terminal);
        SerializedProperty resident = serialized.FindProperty("replayResidentId");
        string explicitId = resident != null ? resident.stringValue.Trim().ToUpperInvariant() : string.Empty;
        if (!string.IsNullOrEmpty(explicitId))
        {
            return explicitId;
        }

        string hierarchyName = terminal.name.ToUpperInvariant();
        Transform cursor = terminal.transform.parent;
        while (cursor != null)
        {
            hierarchyName += "/" + cursor.name.ToUpperInvariant();
            cursor = cursor.parent;
        }

        if (hierarchyName.Contains("17F01")) return "17F01";
        if (hierarchyName.Contains("17F02")) return "17F02";
        if (hierarchyName.Contains("17F03")) return "17F03";
        return string.Empty;
    }

    private static void EnsureRuntimePrompt(HearthTvTerminalController terminal)
    {
        Transform keyboard = terminal.transform.Find("KeyboardNavigationRoot");
        if (keyboard == null)
        {
            GameObject keyboardObject = new GameObject("KeyboardNavigationRoot", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(keyboardObject, "Create terminal keyboard navigation root");
            keyboardObject.transform.SetParent(terminal.transform, false);
            RectTransform keyboardRect = keyboardObject.GetComponent<RectTransform>();
            keyboardRect.anchorMin = Vector2.zero;
            keyboardRect.anchorMax = Vector2.one;
            keyboardRect.pivot = new Vector2(0.5f, 0.5f);
            keyboardRect.offsetMin = Vector2.zero;
            keyboardRect.offsetMax = Vector2.zero;
            keyboard = keyboardObject.transform;
        }

        Transform existing = keyboard.Find("RuntimePromptText");
        TMP_Text prompt = existing != null ? existing.GetComponent<TMP_Text>() : null;
        if (prompt == null)
        {
            GameObject promptObject = new GameObject("RuntimePromptText", typeof(RectTransform), typeof(TextMeshProUGUI));
            Undo.RegisterCreatedObjectUndo(promptObject, "Create terminal runtime prompt");
            promptObject.transform.SetParent(keyboard, false);
            RectTransform rect = promptObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(560f, -92f);
            rect.sizeDelta = new Vector2(800f, 38f);
            prompt = promptObject.GetComponent<TMP_Text>();
            prompt.text = string.Empty;
            prompt.fontSize = 19f;
            prompt.fontStyle = FontStyles.Bold;
            prompt.alignment = TextAlignmentOptions.Center;
            prompt.color = new Color(0.78f, 0.96f, 1f, 0.96f);
            prompt.raycastTarget = false;
            promptObject.SetActive(false);
        }

        SerializedObject serialized = new SerializedObject(terminal);
        SerializedProperty promptProperty = serialized.FindProperty("runtimePromptText");
        if (promptProperty != null)
        {
            promptProperty.objectReferenceValue = prompt;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
#endif
