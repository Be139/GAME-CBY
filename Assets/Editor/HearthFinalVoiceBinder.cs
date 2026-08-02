#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public static class HearthFinalVoiceBinder
{
    private const string SourceJsonlFileName =
        "HEARTH_ElevenLabs_v3_API_Voice_Lines_Manual_Revision.jsonl";
    private const string SourceCollectionFolder =
        "GeneratedAudio/HEARTH_FinalVoiceCollection_2026-07-31";
    private const string ImportedVoiceFolder =
        "Assets/Audio/HEARTH/Dialogue/FinalVoiceCollection_2026-07-31";
    private const string ExcludedSequence = "Prologue_HEARTHCommercial";
    private const string ExcludedPromoReferenceLine = "Lobby_OpeningBriefing_FieldUnit_002";
    private const int ExpectedManifestRows = 338;
    private const int ExpectedRuntimeRows = 330;

    private static readonly Regex PerformanceTagRegex = new Regex(
        @"\[[^\]\r\n]+\]",
        RegexOptions.Compiled);

    private static readonly Regex WhitespaceRegex = new Regex(
        @"\s+",
        RegexOptions.Compiled);

    [Serializable]
    private sealed class VoiceManifestLine
    {
        public string line_id;
        public string speaker;
        public string sequence;
        public string text;
        public string model;
    }

    [MenuItem("Tools/Hearth/Dialogue/Import Final Voice Collection And Bind Subtitles")]
    public static void ImportFinalVoiceCollectionAndBindSubtitlesMenu()
    {
        List<VoiceManifestLine> allRows;
        List<VoiceManifestLine> runtimeRows;
        string error;
        if (!TryLoadRows(out allRows, out runtimeRows, out error))
        {
            Debug.LogError("[HearthFinalVoiceBinder] " + error);
            return;
        }

        if (!HearthFinalDialogueSync.SyncAllFromFinalScript(false))
        {
            Debug.LogError(
                "[HearthFinalVoiceBinder] Final-script synchronization failed; voice import was stopped before binding.");
            return;
        }

        List<string> copyIssues = CopyRuntimeAudio(runtimeRows);
        if (copyIssues.Count > 0)
        {
            Debug.LogError(
                "[HearthFinalVoiceBinder] Voice import failed:\n- " + string.Join("\n- ", copyIssues));
            return;
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        List<string> bindIssues = BindClips(runtimeRows);
        if (bindIssues.Count > 0)
        {
            Debug.LogError(
                "[HearthFinalVoiceBinder] Voice binding failed:\n- " + string.Join("\n- ", bindIssues));
            return;
        }

        AssetDatabase.SaveAssets();
        List<string> validationIssues = ValidateBindings(runtimeRows);
        if (validationIssues.Count > 0)
        {
            Debug.LogError(
                "[HearthFinalVoiceBinder] Import completed, but validation failed:\n- " +
                string.Join("\n- ", validationIssues));
            return;
        }

        int usageCount = CountBoundAssetLineUsages(runtimeRows);
        Debug.Log(
            "[HearthFinalVoiceBinder] Final voice binding passed: " + runtimeRows.Count +
            " distinct gameplay clips imported and bound across " + usageCount +
            " runtime dialogue entries. The 7 commercial clips and the lobby line that refers to the omitted " +
            "commercial were intentionally excluded. Subtitles use the final JSONL wording without performance tags.");
    }

    [MenuItem("Tools/Hearth/Dialogue/Validate Final Voice Bindings")]
    public static void ValidateFinalVoiceBindingsMenu()
    {
        List<VoiceManifestLine> allRows;
        List<VoiceManifestLine> runtimeRows;
        string error;
        if (!TryLoadRows(out allRows, out runtimeRows, out error))
        {
            Debug.LogError("[HearthFinalVoiceBinder] " + error);
            return;
        }

        List<string> issues = ValidateBindings(runtimeRows);
        if (issues.Count > 0)
        {
            Debug.LogError(
                "[HearthFinalVoiceBinder] Final voice validation failed:\n- " + string.Join("\n- ", issues));
            return;
        }

        Debug.Log(
            "[HearthFinalVoiceBinder] Final voice validation passed: " + runtimeRows.Count +
            " distinct gameplay clips, " + CountBoundAssetLineUsages(runtimeRows) +
            " bound runtime entries, no commercial audio imported.");
    }

    private static bool TryLoadRows(
        out List<VoiceManifestLine> allRows,
        out List<VoiceManifestLine> runtimeRows,
        out string error)
    {
        allRows = new List<VoiceManifestLine>();
        runtimeRows = new List<VoiceManifestLine>();
        error = null;
        string sourcePath = GetProjectPath(SourceJsonlFileName);
        if (!File.Exists(sourcePath))
        {
            error = "Final voice JSONL snapshot is missing: " + sourcePath;
            return false;
        }

        string[] lines = File.ReadAllLines(sourcePath, new UTF8Encoding(false));
        for (int i = 0; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
            {
                continue;
            }

            VoiceManifestLine row;
            try
            {
                row = JsonUtility.FromJson<VoiceManifestLine>(lines[i]);
            }
            catch (Exception exception)
            {
                error = "Could not parse JSONL source line " + (i + 1) + ": " + exception.Message;
                return false;
            }

            if (row == null || string.IsNullOrWhiteSpace(row.line_id))
            {
                error = "JSONL source line " + (i + 1) + " has no line_id.";
                return false;
            }

            allRows.Add(row);
            if (IsRuntimeRow(row))
            {
                runtimeRows.Add(row);
            }
        }

        if (allRows.Count != ExpectedManifestRows)
        {
            error = "Expected " + ExpectedManifestRows + " final voice rows, but found " + allRows.Count + ".";
            return false;
        }

        if (runtimeRows.Count != ExpectedRuntimeRows)
        {
            error = "Expected " + ExpectedRuntimeRows + " runtime voice rows, but found " + runtimeRows.Count + ".";
            return false;
        }

        string duplicate = runtimeRows
            .GroupBy(row => row.line_id, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .FirstOrDefault();
        if (!string.IsNullOrEmpty(duplicate))
        {
            error = "Duplicate runtime voice line_id: " + duplicate;
            return false;
        }

        return true;
    }

    private static List<string> CopyRuntimeAudio(List<VoiceManifestLine> runtimeRows)
    {
        List<string> issues = new List<string>();
        string sourceFolder = GetProjectPath(SourceCollectionFolder);
        string destinationFolder = GetProjectPath(ImportedVoiceFolder);
        Directory.CreateDirectory(destinationFolder);

        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (VoiceManifestLine row in runtimeRows)
            {
                string sourcePath = Path.Combine(sourceFolder, row.line_id + ".mp3");
                string destinationPath = Path.Combine(destinationFolder, row.line_id + ".mp3");
                if (!File.Exists(sourcePath))
                {
                    issues.Add("Missing final collection file " + sourcePath);
                    continue;
                }

                File.Copy(sourcePath, destinationPath, true);
            }
        }
        catch (Exception exception)
        {
            issues.Add(exception.Message);
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        return issues;
    }

    private static List<string> BindClips(List<VoiceManifestLine> runtimeRows)
    {
        Dictionary<string, VoiceManifestLine> rowsById = runtimeRows.ToDictionary(
            row => row.line_id,
            row => row,
            StringComparer.Ordinal);
        HashSet<string> runtimeSequenceIds = BuildRuntimeSequenceIds(runtimeRows);
        List<string> issues = new List<string>();
        string[] assetGuids = AssetDatabase.FindAssets(
            "t:HearthDialogueSequence",
            new[] { "Assets/Data/MinLoop/Dialogues" });

        foreach (string guid in assetGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            HearthDialogueSequence sequence = AssetDatabase.LoadAssetAtPath<HearthDialogueSequence>(path);
            if (sequence == null || !runtimeSequenceIds.Contains(sequence.SequenceId))
            {
                continue;
            }

            SerializedObject serialized = new SerializedObject(sequence);
            SerializedProperty lines = serialized.FindProperty("lines");
            for (int i = 0; i < lines.arraySize; i++)
            {
                SerializedProperty line = lines.GetArrayElementAtIndex(i);
                string lineId = line.FindPropertyRelative("lineId").stringValue;
                VoiceManifestLine row;
                if (string.IsNullOrWhiteSpace(lineId) || !rowsById.TryGetValue(lineId, out row))
                {
                    issues.Add(path + " line " + i + " has no valid final voice line ID.");
                    continue;
                }

                string clipPath = ImportedVoiceFolder + "/" + lineId + ".mp3";
                AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(clipPath);
                if (clip == null)
                {
                    issues.Add("Unity did not import AudioClip " + clipPath);
                    continue;
                }

                line.FindPropertyRelative("voiceClip").objectReferenceValue = clip;
                line.FindPropertyRelative("durationMode").enumValueIndex =
                    (int)HearthSubtitleDurationMode.VoiceClipWhenAssigned;
                line.FindPropertyRelative("voiceTailSeconds").floatValue = 0.12f;
            }

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(sequence);
        }

        return issues;
    }

    private static List<string> ValidateBindings(List<VoiceManifestLine> runtimeRows)
    {
        Dictionary<string, VoiceManifestLine> rowsById = runtimeRows.ToDictionary(
            row => row.line_id,
            row => row,
            StringComparer.Ordinal);
        HashSet<string> runtimeSequenceIds = BuildRuntimeSequenceIds(runtimeRows);
        HashSet<string> usedLineIds = new HashSet<string>(StringComparer.Ordinal);
        List<string> issues = new List<string>();

        string destinationFolder = GetProjectPath(ImportedVoiceFolder);
        if (!Directory.Exists(destinationFolder))
        {
            issues.Add("Imported voice folder is missing: " + destinationFolder);
            return issues;
        }

        HashSet<string> importedIds = new HashSet<string>(
            Directory.GetFiles(destinationFolder, "*.mp3", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileNameWithoutExtension),
            StringComparer.Ordinal);
        foreach (VoiceManifestLine row in runtimeRows)
        {
            if (!importedIds.Contains(row.line_id))
            {
                issues.Add("Imported MP3 is missing for " + row.line_id + ".");
            }
        }

        foreach (string importedId in importedIds)
        {
            if (!rowsById.ContainsKey(importedId))
            {
                issues.Add("Unexpected or excluded MP3 exists in the runtime import folder: " + importedId + ".mp3");
            }
        }

        string[] assetGuids = AssetDatabase.FindAssets(
            "t:HearthDialogueSequence",
            new[] { "Assets/Data/MinLoop/Dialogues" });
        foreach (string guid in assetGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            HearthDialogueSequence sequence = AssetDatabase.LoadAssetAtPath<HearthDialogueSequence>(path);
            if (sequence == null || !runtimeSequenceIds.Contains(sequence.SequenceId))
            {
                continue;
            }

            if (sequence.Lines == null || sequence.Lines.Count == 0)
            {
                issues.Add(path + " has no dialogue lines.");
                continue;
            }

            for (int i = 0; i < sequence.Lines.Count; i++)
            {
                MinLoopSubtitleLine line = sequence.Lines[i];
                VoiceManifestLine row;
                if (line == null || string.IsNullOrWhiteSpace(line.lineId) ||
                    !rowsById.TryGetValue(line.lineId, out row))
                {
                    issues.Add(path + " line " + i + " has no valid final voice line ID.");
                    continue;
                }

                usedLineIds.Add(line.lineId);
                string expectedSpeaker = row.speaker ?? string.Empty;
                string expectedSubtitle = CleanSubtitle(row.text);
                if (!string.Equals(line.speaker ?? string.Empty, expectedSpeaker, StringComparison.Ordinal))
                {
                    issues.Add(path + " line " + i + " speaker differs from " + line.lineId + ".");
                }

                if (!string.Equals(line.text ?? string.Empty, expectedSubtitle, StringComparison.Ordinal))
                {
                    issues.Add(path + " line " + i + " subtitle differs from " + line.lineId + ".");
                }

                if (line.voiceClip == null)
                {
                    issues.Add(path + " line " + i + " has no AudioClip for " + line.lineId + ".");
                }
                else
                {
                    string actualClipPath = AssetDatabase.GetAssetPath(line.voiceClip);
                    string expectedClipPath = ImportedVoiceFolder + "/" + line.lineId + ".mp3";
                    if (!string.Equals(actualClipPath, expectedClipPath, StringComparison.Ordinal))
                    {
                        issues.Add(path + " line " + i + " uses the wrong AudioClip for " + line.lineId + ".");
                    }
                }

                if (line.durationMode != HearthSubtitleDurationMode.VoiceClipWhenAssigned)
                {
                    issues.Add(path + " line " + i + " does not follow AudioClip duration.");
                }
            }
        }

        foreach (VoiceManifestLine row in runtimeRows)
        {
            if (!usedLineIds.Contains(row.line_id))
            {
                issues.Add("Final voice row is not used by any runtime dialogue asset: " + row.line_id);
            }
        }

        return issues;
    }

    private static int CountBoundAssetLineUsages(List<VoiceManifestLine> runtimeRows)
    {
        HashSet<string> runtimeIds = new HashSet<string>(
            runtimeRows.Select(row => row.line_id),
            StringComparer.Ordinal);
        int count = 0;
        string[] assetGuids = AssetDatabase.FindAssets(
            "t:HearthDialogueSequence",
            new[] { "Assets/Data/MinLoop/Dialogues" });
        foreach (string guid in assetGuids)
        {
            HearthDialogueSequence sequence = AssetDatabase.LoadAssetAtPath<HearthDialogueSequence>(
                AssetDatabase.GUIDToAssetPath(guid));
            if (sequence == null || sequence.Lines == null)
            {
                continue;
            }

            count += sequence.Lines.Count(line => line != null && runtimeIds.Contains(line.lineId));
        }

        return count;
    }

    private static HashSet<string> BuildRuntimeSequenceIds(List<VoiceManifestLine> runtimeRows)
    {
        HashSet<string> result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int photoIndex = 0;
        foreach (VoiceManifestLine row in runtimeRows)
        {
            if (string.Equals(row.sequence, "17F04_ChristmasPhoto", StringComparison.Ordinal))
            {
                result.Add(photoIndex < 2
                    ? "17F04_ChristmasPhoto"
                    : photoIndex < 5
                        ? "17F04_SecondPhoto"
                        : "17F04_PhotoCompletion");
                photoIndex++;
                continue;
            }

            foreach (string sequence in SplitSequences(row.sequence))
            {
                result.Add(sequence);
            }
        }

        return result;
    }

    private static bool IsRuntimeRow(VoiceManifestLine row)
    {
        return !string.Equals(row.line_id, ExcludedPromoReferenceLine, StringComparison.Ordinal) &&
               !SplitSequences(row.sequence).Any(sequence =>
                   string.Equals(sequence, ExcludedSequence, StringComparison.OrdinalIgnoreCase));
    }

    private static IEnumerable<string> SplitSequences(string value)
    {
        return (value ?? string.Empty)
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Trim())
            .Where(part => part.Length > 0);
    }

    private static string CleanSubtitle(string value)
    {
        string withoutTags = PerformanceTagRegex.Replace(value ?? string.Empty, string.Empty);
        return WhitespaceRegex.Replace(withoutTags, " ").Trim();
    }

    private static string GetProjectPath(string relativePath)
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        return Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }
}
#endif
