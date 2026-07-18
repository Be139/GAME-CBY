#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class HearthFinalDialogueSync
{
    private const string FinalScriptFileName = "HEARTH_Full_Game_Script_Expanded_Native_English_Lobby_Mia_Commentary.md";
    private const string SupplementalFolder = "Assets/Data/MinLoop/Dialogues/FinalScriptSupplemental";

    private static readonly Regex SceneRegex = new Regex(
        @"^### Scene\s+([0-9]+\.[0-9]+)\b",
        RegexOptions.Compiled);

    private static readonly Regex DialogueRegex = new Regex(
        "^\\*\\*(.+?):\\*\\*\\s*(?:\\([^)]*\\)\\s*)?\"(.*)\"\\s*$",
        RegexOptions.Compiled);

    private sealed class DialogueLine
    {
        public string Speaker;
        public string Text;
        public int SourceLine;
    }

    private sealed class DialogueMapping
    {
        public string AssetPath;
        public string SceneId;
        public int[] Indices;
        public string Purpose;

        public DialogueMapping(string assetPath, string sceneId, int[] indices, string purpose)
        {
            AssetPath = assetPath;
            SceneId = sceneId;
            Indices = indices;
            Purpose = purpose;
        }
    }

    [MenuItem("Tools/Hearth/Dialogue/Sync All Dialogue From Final Script")]
    public static void SyncAllFromFinalScriptMenu()
    {
        SyncAllFromFinalScript(true);
    }

    [MenuItem("Tools/Hearth/Dialogue/Validate Final Script Coverage")]
    public static void ValidateFinalScriptCoverageMenu()
    {
        Dictionary<string, List<DialogueLine>> scenes;
        string error;
        if (!TryParseFinalScript(out scenes, out error))
        {
            Debug.LogError("[HearthFinalDialogueSync] " + error);
            return;
        }

        List<string> issues = ValidateMappings(scenes, BuildMappings());
        if (issues.Count == 0)
        {
            int lineCount = scenes.Values.Sum(lines => lines.Count);
            Debug.Log("[HearthFinalDialogueSync] Coverage valid: all " + lineCount + " final-script dialogue lines are represented by dialogue assets.");
            return;
        }

        Debug.LogError("[HearthFinalDialogueSync] Coverage validation failed:\n- " + string.Join("\n- ", issues));
    }

    public static bool SyncAllFromFinalScript(bool logSummary)
    {
        Dictionary<string, List<DialogueLine>> scenes;
        string error;
        if (!TryParseFinalScript(out scenes, out error))
        {
            Debug.LogError("[HearthFinalDialogueSync] " + error);
            return false;
        }

        List<DialogueMapping> mappings = BuildMappings();
        List<string> issues = ValidateMappings(scenes, mappings);
        if (issues.Count > 0)
        {
            Debug.LogError("[HearthFinalDialogueSync] Sync stopped because the final script no longer matches the approved scene mapping:\n- " + string.Join("\n- ", issues));
            return false;
        }

        EnsureFolder(SupplementalFolder);
        int sequenceCount = 0;
        int lineCount = 0;
        foreach (DialogueMapping mapping in mappings)
        {
            List<DialogueLine> source = scenes[mapping.SceneId];
            List<DialogueLine> selected = mapping.Indices.Select(index => source[index]).ToList();
            if (!WriteSequence(mapping, selected))
            {
                return false;
            }

            sequenceCount++;
            lineCount += selected.Count;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        if (logSummary)
        {
            int uniqueSourceLines = scenes.Values.Sum(lines => lines.Count);
            Debug.Log(
                "[HearthFinalDialogueSync] Synced " + sequenceCount + " dialogue assets (" + lineCount +
                " mapped entries). Coverage includes all " + uniqueSourceLines + " dialogue lines in " + FinalScriptFileName +
                ". Existing voice clips were preserved only when speaker and text still matched exactly.");
        }

        return true;
    }

    private static bool TryParseFinalScript(out Dictionary<string, List<DialogueLine>> scenes, out string error)
    {
        scenes = new Dictionary<string, List<DialogueLine>>(StringComparer.OrdinalIgnoreCase);
        error = null;

        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string sourcePath = Path.Combine(projectRoot, FinalScriptFileName);
        if (!File.Exists(sourcePath))
        {
            error = "Final dialogue source is missing: " + sourcePath;
            return false;
        }

        string currentScene = null;
        string[] sourceLines = File.ReadAllLines(sourcePath, new UTF8Encoding(false));
        for (int i = 0; i < sourceLines.Length; i++)
        {
            string raw = sourceLines[i].Trim();
            Match sceneMatch = SceneRegex.Match(raw);
            if (sceneMatch.Success)
            {
                currentScene = sceneMatch.Groups[1].Value;
                if (!scenes.ContainsKey(currentScene))
                {
                    scenes.Add(currentScene, new List<DialogueLine>());
                }

                continue;
            }

            Match dialogueMatch = DialogueRegex.Match(raw);
            if (!dialogueMatch.Success)
            {
                continue;
            }

            if (string.IsNullOrEmpty(currentScene))
            {
                error = "Found dialogue before the first Scene heading at source line " + (i + 1) + ".";
                return false;
            }

            scenes[currentScene].Add(new DialogueLine
            {
                Speaker = dialogueMatch.Groups[1].Value.Trim(),
                Text = dialogueMatch.Groups[2].Value.Trim(),
                SourceLine = i + 1
            });
        }

        if (scenes.Count == 0 || scenes.Values.Sum(lines => lines.Count) == 0)
        {
            error = "No dialogue could be parsed from " + FinalScriptFileName + ".";
            return false;
        }

        return true;
    }

    private static bool WriteSequence(DialogueMapping mapping, List<DialogueLine> lines)
    {
        EnsureFolder(Path.GetDirectoryName(mapping.AssetPath).Replace('\\', '/'));
        HearthDialogueSequence asset = AssetDatabase.LoadAssetAtPath<HearthDialogueSequence>(mapping.AssetPath);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<HearthDialogueSequence>();
            AssetDatabase.CreateAsset(asset, mapping.AssetPath);
        }

        SerializedObject serialized = new SerializedObject(asset);
        SerializedProperty oldLines = serialized.FindProperty("lines");
        Dictionary<string, AudioClip> preservedClips = new Dictionary<string, AudioClip>(StringComparer.Ordinal);
        for (int i = 0; i < oldLines.arraySize; i++)
        {
            SerializedProperty oldLine = oldLines.GetArrayElementAtIndex(i);
            string speaker = oldLine.FindPropertyRelative("speaker").stringValue;
            string text = oldLine.FindPropertyRelative("text").stringValue;
            AudioClip clip = oldLine.FindPropertyRelative("voiceClip").objectReferenceValue as AudioClip;
            if (clip != null)
            {
                preservedClips[BuildVoiceKey(speaker, text)] = clip;
            }
        }

        string id = Path.GetFileNameWithoutExtension(mapping.AssetPath);
        serialized.FindProperty("sequenceId").stringValue = id;
        serialized.FindProperty("notes").stringValue =
            mapping.Purpose + " Source: " + FinalScriptFileName + ", Scene " + mapping.SceneId +
            ". Text is synchronized by Tools/Hearth/Dialogue/Sync All Dialogue From Final Script.";

        SerializedProperty targetLines = serialized.FindProperty("lines");
        targetLines.arraySize = lines.Count;
        for (int i = 0; i < lines.Count; i++)
        {
            DialogueLine source = lines[i];
            SerializedProperty target = targetLines.GetArrayElementAtIndex(i);
            target.FindPropertyRelative("startDelay").floatValue = i == 0 ? 0.15f : 0.18f;
            target.FindPropertyRelative("speaker").stringValue = source.Speaker;
            target.FindPropertyRelative("text").stringValue = source.Text;
            target.FindPropertyRelative("holdSeconds").floatValue = EstimateHoldSeconds(source.Text);
            AudioClip preserved;
            preservedClips.TryGetValue(BuildVoiceKey(source.Speaker, source.Text), out preserved);
            target.FindPropertyRelative("voiceClip").objectReferenceValue = preserved;
            target.FindPropertyRelative("durationMode").enumValueIndex = (int)HearthSubtitleDurationMode.VoiceClipWhenAssigned;
            target.FindPropertyRelative("voiceTailSeconds").floatValue = 0.12f;
        }

        serialized.FindProperty("postSequenceDelay").floatValue = 0.18f;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
        return true;
    }

    private static string BuildVoiceKey(string speaker, string text)
    {
        return (speaker ?? string.Empty).Trim() + "\n" + (text ?? string.Empty).Trim();
    }

    private static float EstimateHoldSeconds(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 2.4f;
        }

        int words = text.Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
        return Mathf.Clamp(1.15f + words / 2.65f, 2.2f, 12f);
    }

    private static List<string> ValidateMappings(
        Dictionary<string, List<DialogueLine>> scenes,
        List<DialogueMapping> mappings)
    {
        List<string> issues = new List<string>();
        Dictionary<string, HashSet<int>> covered = new Dictionary<string, HashSet<int>>(StringComparer.OrdinalIgnoreCase);

        foreach (DialogueMapping mapping in mappings)
        {
            List<DialogueLine> sceneLines;
            if (!scenes.TryGetValue(mapping.SceneId, out sceneLines))
            {
                issues.Add(mapping.AssetPath + " references missing Scene " + mapping.SceneId + ".");
                continue;
            }

            if (!covered.ContainsKey(mapping.SceneId))
            {
                covered.Add(mapping.SceneId, new HashSet<int>());
            }

            foreach (int index in mapping.Indices)
            {
                if (index < 0 || index >= sceneLines.Count)
                {
                    issues.Add(
                        mapping.AssetPath + " references Scene " + mapping.SceneId + " dialogue index " + index +
                        ", but that scene has " + sceneLines.Count + " dialogue lines.");
                    continue;
                }

                covered[mapping.SceneId].Add(index);
            }
        }

        foreach (KeyValuePair<string, List<DialogueLine>> scene in scenes)
        {
            HashSet<int> sceneCoverage;
            covered.TryGetValue(scene.Key, out sceneCoverage);
            for (int i = 0; i < scene.Value.Count; i++)
            {
                if (sceneCoverage == null || !sceneCoverage.Contains(i))
                {
                    DialogueLine line = scene.Value[i];
                    issues.Add(
                        "Scene " + scene.Key + " dialogue index " + i + " (source line " + line.SourceLine +
                        ", " + line.Speaker + ") is not represented by any dialogue asset.");
                }
            }
        }

        return issues;
    }

    private static List<DialogueMapping> BuildMappings()
    {
        const string root = "Assets/Data/MinLoop/Dialogues/";
        const string finale = root + "17F04/";
        const string lobby = root + "Lobby/";
        const string extra = SupplementalFolder + "/";

        return new List<DialogueMapping>
        {
            M(lobby + "Lobby_OpeningBriefing.asset", "1.1", R(0, 6), "Lobby opening briefing; movement and view are locked."),
            M(lobby + "Lobby_LilyVoiceMessage.asset", "1.1", I(7), "Lily's recorded lobby voice message."),
            M(lobby + "Lobby_OpeningCloseout.asset", "1.1", R(8, 10), "Mia and the Field Unit close the lobby introduction."),
            M(lobby + "Lobby_Group01_Girl.asset", "1.1", R(11, 14), "Optional lobby girl and public-unit conversation."),
            M(lobby + "Lobby_Group02_YoungMan.asset", "1.1", R(15, 21), "Optional young-man and work-unit conversation."),
            M(lobby + "Lobby_Group03_Grandmother.asset", "1.1", R(22, 29), "Optional Mrs. Ellis and care-unit conversation."),
            M(lobby + "Lobby_AssignmentLoaded.asset", "1.1", R(30, 33), "Plays after the assignment is loaded and unlocks the elevator."),
            M(lobby + "Lobby_ElevatorRide.asset", "1.2", R(0, 14), "Elevator briefing before arrival on floor 17."),

            M(extra + "17F01_ApartmentGreeting.asset", "1.3", R(0, 2), "Final-script 17F01 apartment and terminal introduction; ready for a terminal/doorway cue binding."),
            M(root + "17F01_BedroomPrelude.asset", "1.4", R(0, 3), "17F01 bedroom playback before the soothe interaction."),
            M(root + "17F01_BedsideSoothing.asset", "1.4", R(4, 11), "17F01 soothe interaction and event archive."),
            M(root + "17F01_LivingRoomObservation.asset", "1.5", R(0, 13), "17F01 morning parent observation."),
            M(extra + "17F01_TerminalSignoffIntro.asset", "1.6", I(0), "17F01 sign-off recommendation; ready for terminal choice binding."),
            M(extra + "17F01_TerminalSignoff_A.asset", "1.6", I(1, 2, 8), "17F01 option A sign-off and next-household direction; ready for disposition binding."),
            M(extra + "17F01_TerminalSignoff_B.asset", "1.6", R(3, 8), "17F01 option B sign-off and next-household direction; ready for disposition binding."),

            M(extra + "17F02_TerminalIntro.asset", "2.0", R(0, 1), "17F02 terminal introduction; ready for terminal-open and replay cues."),
            M(root + "17F02_BedroomWake.asset", "2.1", R(0, 6), "17F02 dormant-unit bedroom opening."),
            M(root + "17F02_BedroomConfide.asset", "2.2", R(0, 2), "17F02 companion-mode decision and Claire's account."),
            M(root + "17F02_BedroomComfort.asset", "2.2", R(3, 5), "17F02 comfort response and session archive."),
            M(root + "17F02_WifeExit.asset", "2.3", R(0, 1), "17F02 dinner call and Claire's response."),
            M(root + "17F02_DiningObservation.asset", "2.3", R(2, 10), "17F02 dining-room observation."),
            M(root + "17F02_LogAccess.asset", "2.4", R(0, 12), "17F02 household-log access and soft-guidance decision."),
            M(root + "17F02_ForcedShutdown.asset", "2.4", R(13, 15), "17F02 forced shutdown exchange."),
            M(root + "17F02_BlackAudioArgument.asset", "2.5", R(0, 28), "17F02 black-screen argument recording."),
            M(extra + "17F02_TerminalSignoffIntro.asset", "2.6", R(0, 4), "17F02 terminal recommendation; ready for terminal choice binding."),
            M(extra + "17F02_TerminalSignoff_A.asset", "2.6", I(5, 6, 13, 14), "17F02 option A sign-off and 17F03 alert; ready for disposition binding."),
            M(extra + "17F02_TerminalSignoff_B.asset", "2.6", R(7, 14), "17F02 option B sign-off and 17F03 alert; ready for disposition binding."),

            M(extra + "17F03_TerminalEntry.asset", "3.1", I(0), "17F03 enter-unit instruction; ready for terminal primary-action binding."),
            M(root + "17F03_HumanEntryParents.asset", "3.2", R(0, 6), "17F03 parent explanation after Mia enters the unit."),
            M(extra + "17F03_InspectionRecallPrompt.asset", "3.2", I(7), "Field Unit instruction shown at the physical-unit inspection camera."),
            M(root + "17F03_MiddayConflict.asset", "3.3", R(0, 1), "17F03 midday conflict opening."),
            M(root + "17F03_MediateToDaughter.asset", "3.3", I(2), "17F03 mediation line addressed to Ava."),
            M(root + "17F03_MediateToMother.asset", "3.3", I(3, 4), "17F03 mediation line addressed to Laura and completion cue."),
            M(root + "17F03_NightDaughter.asset", "3.4", R(0, 6), "17F03 night conversation with Ava."),
            M(root + "17F03_NightShutdownLeadIn.asset", "3.4", R(7, 9), "17F03 failed standard response and Ava's interruption."),
            M(root + "17F03_NightShutdown.asset", "3.4", I(10), "17F03 deep-sleep system line."),
            M(root + "17F03_NightShutdownAction.asset", "3.4", I(10), "Compatibility copy of the 17F03 deep-sleep system line."),
            M(root + "17F03_PostReplayExplanation.asset", "3.5", R(0, 2), "17F03 post-replay technical explanation and Laura's question."),
            M(extra + "17F03_PostReplay_A.asset", "3.5", I(3, 4, 5, 6, 7, 16), "17F03 option A result and route-complete cue; ready for disposition binding."),
            M(extra + "17F03_PostReplay_B.asset", "3.5", R(8, 16), "17F03 option B result and route-complete cue; ready for disposition binding."),

            M(finale + "17F04_HomeGreeting_High.asset", "4.1", R(0, 3), "17F04 terminal-side guardian-confirmation exchange (positive trust variant)."),
            M(finale + "17F04_HomeGreeting_Low.asset", "4.1", R(0, 3), "17F04 terminal-side guardian-confirmation exchange (negative trust variant)."),
            M(finale + "17F04_ChristmasPhoto.asset", "4.2", R(0, 3), "17F04 photo-frame narration."),
            M(finale + "17F04_HearingDaughterRoom.asset", "4.3", R(0, 8), "17F04 dialogue heard outside Lily's door."),
            M(finale + "17F04_DaughterRoom_High.asset", "4.4", R(0, 9), "17F04 daughter-room conversation (positive trust variant)."),
            M(finale + "17F04_DaughterRoom_Low.asset", "4.4", R(0, 9), "17F04 daughter-room conversation (negative trust variant)."),
            M(finale + "17F04_AnswerSelf.asset", "4.5", R(0, 10), "17F04 path A: Mia answers Lily herself."),
            M(finale + "17F04_CompanionAnswer.asset", "4.7", R(18, 25), "17F04 path B: the home unit answers and the Field Unit closes the shift."),
            M(finale + "17F04_Shutdown_High.asset", "4.6", R(0, 12), "17F04 positive-trust shutdown and proper goodbye."),
            M(finale + "17F04_Shutdown_Low.asset", "4.6", I(0, 1, 2, 13, 14, 15, 16), "17F04 negative-trust forced shutdown."),
            M(finale + "17F04_Epilogue_High_Shutdown.asset", "4.7", R(0, 17), "17F04 path A black-screen epilogue (positive trust compatibility variant)."),
            M(finale + "17F04_Epilogue_Low_Shutdown.asset", "4.7", R(0, 17), "17F04 path A black-screen epilogue (negative trust compatibility variant)."),
            M(finale + "17F04_Epilogue_High_Retain.asset", "4.8", R(0, 24), "17F04 path B black-screen epilogue (positive trust compatibility variant)."),
            M(finale + "17F04_Epilogue_Low_Retain.asset", "4.8", R(0, 24), "17F04 path B black-screen epilogue (negative trust compatibility variant).")
        };
    }

    private static DialogueMapping M(string path, string scene, int[] indices, string purpose)
    {
        return new DialogueMapping(path, scene, indices, purpose);
    }

    private static int[] I(params int[] indices)
    {
        return indices;
    }

    private static int[] R(int first, int last)
    {
        return Enumerable.Range(first, last - first + 1).ToArray();
    }

    private static void EnsureFolder(string folder)
    {
        folder = folder.Replace('\\', '/').TrimEnd('/');
        if (AssetDatabase.IsValidFolder(folder))
        {
            return;
        }

        string parent = Path.GetDirectoryName(folder).Replace('\\', '/');
        string name = Path.GetFileName(folder);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }
}
#endif
