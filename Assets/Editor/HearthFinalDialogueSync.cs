#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;

public static class HearthFinalDialogueSync
{
    private const string FinalScriptFileName = "HEARTH_Full_Game_Script_Expanded_Native_English_Lobby_Mia_Commentary.md";
    private const string SupplementalFolder = "Assets/Data/MinLoop/Dialogues/FinalScriptSupplemental";
    private const string SubtitleStylePath = "Assets/Data/MinLoop/UI/Hearth_SubtitleStyle.asset";
    private const string MarkerPrefix = "<!-- HEARTH:SEQUENCES ";

    private static readonly Regex SceneRegex = new Regex(
        @"^### Scene\s+([0-9]+\.[0-9]+)\b",
        RegexOptions.Compiled);

    private static readonly Regex DialogueRegex = new Regex(
        "^(?<prefix>\\*\\*(?<speaker>.+?):\\*\\*\\s*(?:\\([^)]*\\)\\s*)?)\"(?<text>.*)\"\\s*$",
        RegexOptions.Compiled);

    private static readonly Regex MarkerRegex = new Regex(
        @"^<!--\s*HEARTH:SEQUENCES\s+(.+?)\s*-->$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private sealed class DialogueLine
    {
        public string SceneId;
        public int SceneIndex;
        public string Speaker;
        public string Prefix;
        public string Text;
        public int SourceLine;
        public HearthSubtitleLinePresentationKind PresentationKind;
        public readonly List<string> SequenceIds = new List<string>();
    }

    private sealed class ParsedScript
    {
        public readonly Dictionary<string, List<DialogueLine>> Scenes =
            new Dictionary<string, List<DialogueLine>>(StringComparer.OrdinalIgnoreCase);

        public bool HasStableMarkers;

        public IEnumerable<DialogueLine> AllLines
        {
            get { return Scenes.Values.SelectMany(lines => lines).OrderBy(line => line.SourceLine); }
        }
    }

    private sealed class DialogueMapping
    {
        public string AssetPath;
        public string SceneId;
        public int[] LegacyIndices;
        public string Purpose;

        public string SequenceId
        {
            get { return Path.GetFileNameWithoutExtension(AssetPath); }
        }

        public DialogueMapping(string assetPath, string sceneId, int[] legacyIndices, string purpose)
        {
            AssetPath = assetPath;
            SceneId = sceneId;
            LegacyIndices = legacyIndices;
            Purpose = purpose;
        }
    }

    private sealed class SubtitleLineMeasurer : IDisposable
    {
        private readonly GameObject root;
        private readonly TextMeshProUGUI text;
        private readonly float widthPixels;
        private readonly float heightPixels;

        public SubtitleLineMeasurer()
        {
            HearthSubtitleStyleProfile profile = AssetDatabase.LoadAssetAtPath<HearthSubtitleStyleProfile>(SubtitleStylePath);
            HearthSubtitleLayoutSettings layout = profile != null
                ? profile.GetLayout(HearthSubtitlePresentationMode.StandardDialogue)
                : new HearthSubtitleLayoutSettings();

            widthPixels = 1920f * Mathf.Clamp(layout.widthFraction, 0.35f, 0.95f);
            heightPixels = 1080f * Mathf.Clamp(layout.bodyHeightFraction, 0.06f, 0.4f);

            root = new GameObject("Hearth Subtitle Measurement", typeof(RectTransform), typeof(Canvas), typeof(TextMeshProUGUI));
            root.hideFlags = HideFlags.HideAndDontSave;
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(widthPixels, heightPixels);

            text = root.GetComponent<TextMeshProUGUI>();
            text.fontSize = Mathf.Max(1f, layout.bodyFontSize);
            text.fontSizeMax = text.fontSize;
            text.fontSizeMin = Mathf.Min(layout.bodyMinimumFontSize, text.fontSize);
            text.enableAutoSizing = true;
            text.enableWordWrapping = true;
            text.overflowMode = TextOverflowModes.Overflow;
            text.alignment = TextAlignmentOptions.Center;
            text.lineSpacing = layout.lineSpacing;
            text.maxVisibleLines = 0;
        }

        public bool FitsTwoLines(string value)
        {
            text.text = value ?? string.Empty;
            text.ForceMeshUpdate(true, true);
            return text.textInfo.lineCount <= 2 && text.preferredHeight <= heightPixels + 0.5f;
        }

        public int GetLineCount(string value)
        {
            text.text = value ?? string.Empty;
            text.ForceMeshUpdate(true, true);
            return Mathf.Max(1, text.textInfo.lineCount);
        }

        public void Dispose()
        {
            if (root != null)
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }

    [MenuItem("Tools/Hearth/Dialogue/Normalize Final Script To Two-Line Segments")]
    public static void NormalizeFinalScriptToTwoLineSegmentsMenu()
    {
        if (NormalizeAndTagFinalScript())
        {
            SyncAllFromFinalScript(true);
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
        ParsedScript script;
        string error;
        if (!TryParseFinalScript(out script, out error))
        {
            Debug.LogError("[HearthFinalDialogueSync] " + error);
            return;
        }

        List<string> issues = ValidateMappings(script, BuildMappings());
        if (issues.Count == 0)
        {
            Debug.Log(
                "[HearthFinalDialogueSync] Coverage valid: all " + script.AllLines.Count() +
                " final-script dialogue segments use stable sequence markers and are represented by dialogue assets.");
            return;
        }

        Debug.LogError("[HearthFinalDialogueSync] Coverage validation failed:\n- " + string.Join("\n- ", issues));
    }

    [MenuItem("Tools/Hearth/Dialogue/Validate Two-Line Subtitle Layout")]
    public static void ValidateTwoLineSubtitleLayoutMenu()
    {
        ParsedScript script;
        string error;
        if (!TryParseFinalScript(out script, out error))
        {
            Debug.LogError("[HearthFinalDialogueSync] " + error);
            return;
        }

        List<string> issues = new List<string>();
        using (SubtitleLineMeasurer measurer = new SubtitleLineMeasurer())
        {
            foreach (DialogueLine line in script.AllLines)
            {
                if (!measurer.FitsTwoLines(line.Text))
                {
                    issues.Add(
                        "Final script source line " + line.SourceLine + " (" + line.Speaker + ") uses " +
                        measurer.GetLineCount(line.Text) + " rendered lines.");
                }
            }

            string[] assetGuids = AssetDatabase.FindAssets("t:HearthDialogueSequence");
            for (int i = 0; i < assetGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetGuids[i]);
                HearthDialogueSequence sequence = AssetDatabase.LoadAssetAtPath<HearthDialogueSequence>(path);
                if (sequence == null || sequence.Lines == null)
                {
                    continue;
                }

                for (int lineIndex = 0; lineIndex < sequence.Lines.Count; lineIndex++)
                {
                    MinLoopSubtitleLine line = sequence.Lines[lineIndex];
                    if (line != null && !measurer.FitsTwoLines(line.text))
                    {
                        issues.Add(
                            path + " line " + lineIndex + " uses " + measurer.GetLineCount(line.text) +
                            " rendered lines: " + line.text);
                    }
                }
            }
        }

        if (issues.Count == 0)
        {
            Debug.Log("[HearthFinalDialogueSync] Two-line validation passed for the final script and every HearthDialogueSequence asset.");
        }
        else
        {
            Debug.LogError("[HearthFinalDialogueSync] Two-line validation failed:\n- " + string.Join("\n- ", issues));
        }
    }

    public static bool SyncAllFromFinalScript(bool logSummary)
    {
        ParsedScript script;
        string error;
        if (!TryParseFinalScript(out script, out error))
        {
            Debug.LogError("[HearthFinalDialogueSync] " + error);
            return false;
        }

        List<DialogueMapping> mappings = BuildMappings();
        List<string> issues = ValidateMappings(script, mappings);
        if (issues.Count > 0)
        {
            Debug.LogError(
                "[HearthFinalDialogueSync] Sync stopped because the final script mapping is invalid:\n- " +
                string.Join("\n- ", issues));
            return false;
        }

        EnsureFolder(SupplementalFolder);
        int sequenceCount = 0;
        int lineCount = 0;
        foreach (DialogueMapping mapping in mappings)
        {
            List<DialogueLine> selected = SelectLines(script, mapping);
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
            Debug.Log(
                "[HearthFinalDialogueSync] Synced " + sequenceCount + " dialogue assets (" + lineCount +
                " mapped entries) from " + FinalScriptFileName + ". Existing voice clips were preserved only when " +
                "speaker and text still matched exactly.");
        }

        return true;
    }

    private static bool NormalizeAndTagFinalScript()
    {
        ParsedScript script;
        string error;
        if (!TryParseFinalScript(out script, out error))
        {
            Debug.LogError("[HearthFinalDialogueSync] " + error);
            return false;
        }

        List<DialogueMapping> mappings = BuildMappings();
        Dictionary<int, DialogueLine> dialogueBySourceLine = script.AllLines.ToDictionary(line => line.SourceLine);
        Dictionary<int, List<string>> markerIdsBySourceLine = new Dictionary<int, List<string>>();
        foreach (DialogueLine line in script.AllLines)
        {
            List<string> ids = line.SequenceIds.Count > 0
                ? line.SequenceIds.Distinct(StringComparer.OrdinalIgnoreCase).ToList()
                : FindLegacySequenceIds(line, mappings);
            if (ids.Count == 0)
            {
                Debug.LogError(
                    "[HearthFinalDialogueSync] Cannot tag source line " + line.SourceLine +
                    " because it is not represented by a dialogue mapping.");
                return false;
            }

            markerIdsBySourceLine[line.SourceLine] = ids;
        }

        string sourcePath = GetFinalScriptPath();
        string[] sourceLines = File.ReadAllLines(sourcePath, new UTF8Encoding(false));
        List<string> output = new List<string>(sourceLines.Length + 128);
        int splitCount = 0;
        using (SubtitleLineMeasurer measurer = new SubtitleLineMeasurer())
        {
            for (int i = 0; i < sourceLines.Length; i++)
            {
                string trimmed = sourceLines[i].Trim();
                if (MarkerRegex.IsMatch(trimmed))
                {
                    continue;
                }

                DialogueLine dialogue;
                if (!dialogueBySourceLine.TryGetValue(i + 1, out dialogue))
                {
                    output.Add(sourceLines[i]);
                    continue;
                }

                List<string> segments = SplitNaturallyForTwoLines(dialogue.Text, measurer);
                splitCount += Mathf.Max(0, segments.Count - 1);
                List<string> sequenceIds = markerIdsBySourceLine[dialogue.SourceLine];
                for (int segmentIndex = 0; segmentIndex < segments.Count; segmentIndex++)
                {
                    output.Add(MarkerPrefix + string.Join(",", sequenceIds) + " -->");
                    output.Add(dialogue.Prefix + "\"" + segments[segmentIndex] + "\"");
                    if (segmentIndex < segments.Count - 1)
                    {
                        output.Add(string.Empty);
                    }
                }
            }
        }

        string normalized = string.Join("\n", output).TrimEnd() + "\n";
        File.WriteAllText(sourcePath, normalized, new UTF8Encoding(false));
        AssetDatabase.Refresh();
        Debug.Log(
            "[HearthFinalDialogueSync] Added stable sequence markers and split " + splitCount +
            " long subtitle segments in " + FinalScriptFileName + ". Dialogue wording and order were preserved.");
        return true;
    }

    private static List<string> SplitNaturallyForTwoLines(string text, SubtitleLineMeasurer measurer)
    {
        string normalized = Regex.Replace(text ?? string.Empty, @"\s+", " ").Trim();
        if (normalized.Length == 0 || measurer.FitsTwoLines(normalized))
        {
            return new List<string> { normalized };
        }

        List<string> units = Regex.Split(normalized, @"(?<=[.!?;:])\s+")
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToList();

        if (units.Count <= 1)
        {
            units = Regex.Split(normalized, @"(?<=,)\s+")
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .ToList();
        }

        List<string> expandedUnits = new List<string>();
        foreach (string unit in units)
        {
            if (measurer.FitsTwoLines(unit))
            {
                expandedUnits.Add(unit);
            }
            else
            {
                expandedUnits.AddRange(SplitByWords(unit, measurer));
            }
        }

        List<string> result = new List<string>();
        string current = string.Empty;
        foreach (string unit in expandedUnits)
        {
            string candidate = string.IsNullOrEmpty(current) ? unit : current + " " + unit;
            if (string.IsNullOrEmpty(current) || measurer.FitsTwoLines(candidate))
            {
                current = candidate;
            }
            else
            {
                result.Add(current);
                current = unit;
            }
        }

        if (!string.IsNullOrWhiteSpace(current))
        {
            result.Add(current);
        }

        return result.Count > 0 ? result : new List<string> { normalized };
    }

    private static IEnumerable<string> SplitByWords(string text, SubtitleLineMeasurer measurer)
    {
        string[] words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        List<string> result = new List<string>();
        string current = string.Empty;
        for (int i = 0; i < words.Length; i++)
        {
            string candidate = string.IsNullOrEmpty(current) ? words[i] : current + " " + words[i];
            if (string.IsNullOrEmpty(current) || measurer.FitsTwoLines(candidate))
            {
                current = candidate;
            }
            else
            {
                result.Add(current);
                current = words[i];
            }
        }

        if (!string.IsNullOrWhiteSpace(current))
        {
            result.Add(current);
        }

        return result;
    }

    private static bool TryParseFinalScript(out ParsedScript script, out string error)
    {
        script = new ParsedScript();
        error = null;

        string sourcePath = GetFinalScriptPath();
        if (!File.Exists(sourcePath))
        {
            error = "Final dialogue source is missing: " + sourcePath;
            return false;
        }

        string currentScene = null;
        List<string> pendingMarkers = new List<string>();
        string[] sourceLines = File.ReadAllLines(sourcePath, new UTF8Encoding(false));
        for (int i = 0; i < sourceLines.Length; i++)
        {
            string raw = sourceLines[i].Trim();
            Match markerMatch = MarkerRegex.Match(raw);
            if (markerMatch.Success)
            {
                pendingMarkers = markerMatch.Groups[1].Value
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(value => value.Trim())
                    .Where(value => value.Length > 0)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
                script.HasStableMarkers = true;
                continue;
            }

            Match sceneMatch = SceneRegex.Match(raw);
            if (sceneMatch.Success)
            {
                currentScene = sceneMatch.Groups[1].Value;
                if (!script.Scenes.ContainsKey(currentScene))
                {
                    script.Scenes.Add(currentScene, new List<DialogueLine>());
                }

                pendingMarkers.Clear();
                continue;
            }

            Match dialogueMatch = DialogueRegex.Match(raw);
            if (!dialogueMatch.Success)
            {
                if (raw.Length > 0 && !raw.StartsWith("<!--", StringComparison.Ordinal))
                {
                    pendingMarkers.Clear();
                }
                continue;
            }

            if (string.IsNullOrEmpty(currentScene))
            {
                error = "Found dialogue before the first Scene heading at source line " + (i + 1) + ".";
                return false;
            }

            DialogueLine line = new DialogueLine
            {
                SceneId = currentScene,
                SceneIndex = script.Scenes[currentScene].Count,
                Speaker = dialogueMatch.Groups["speaker"].Value.Trim(),
                Prefix = dialogueMatch.Groups["prefix"].Value,
                Text = dialogueMatch.Groups["text"].Value.Trim(),
                SourceLine = i + 1,
                PresentationKind = string.Equals(
                    dialogueMatch.Groups["speaker"].Value.Trim(),
                    "TIME CARD",
                    StringComparison.OrdinalIgnoreCase)
                    ? HearthSubtitleLinePresentationKind.TimeCard
                    : HearthSubtitleLinePresentationKind.Dialogue
            };
            line.SequenceIds.AddRange(pendingMarkers);
            script.Scenes[currentScene].Add(line);
            pendingMarkers.Clear();
        }

        if (script.Scenes.Count == 0 || !script.AllLines.Any())
        {
            error = "No dialogue could be parsed from " + FinalScriptFileName + ".";
            return false;
        }

        return true;
    }

    private static List<DialogueLine> SelectLines(ParsedScript script, DialogueMapping mapping)
    {
        if (script.HasStableMarkers)
        {
            return script.AllLines
                .Where(line => line.SequenceIds.Any(id => string.Equals(id, mapping.SequenceId, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        List<DialogueLine> sceneLines = script.Scenes[mapping.SceneId];
        return mapping.LegacyIndices.Select(index => sceneLines[index]).ToList();
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

        serialized.FindProperty("sequenceId").stringValue = mapping.SequenceId;
        serialized.FindProperty("notes").stringValue =
            mapping.Purpose + " Source: " + FinalScriptFileName + ", stable marker " + mapping.SequenceId +
            ". Text is synchronized by Tools/Hearth/Dialogue/Sync All Dialogue From Final Script.";

        SerializedProperty targetLines = serialized.FindProperty("lines");
        targetLines.arraySize = lines.Count;
        for (int i = 0; i < lines.Count; i++)
        {
            DialogueLine source = lines[i];
            SerializedProperty target = targetLines.GetArrayElementAtIndex(i);
            target.FindPropertyRelative("startDelay").floatValue = i == 0 ? 0.15f : 0.18f;
            target.FindPropertyRelative("speaker").stringValue = source.PresentationKind == HearthSubtitleLinePresentationKind.TimeCard
                ? string.Empty
                : source.Speaker;
            target.FindPropertyRelative("text").stringValue = source.Text;
            SerializedProperty presentationKind = target.FindPropertyRelative("presentationKind");
            if (presentationKind != null)
            {
                presentationKind.enumValueIndex = (int)source.PresentationKind;
            }
            target.FindPropertyRelative("holdSeconds").floatValue = EstimateHoldSeconds(source.Text);
            AudioClip preserved;
            preservedClips.TryGetValue(BuildVoiceKey(
                source.PresentationKind == HearthSubtitleLinePresentationKind.TimeCard ? string.Empty : source.Speaker,
                source.Text), out preserved);
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

    private static List<string> ValidateMappings(ParsedScript script, List<DialogueMapping> mappings)
    {
        return script.HasStableMarkers
            ? ValidateStableMappings(script, mappings)
            : ValidateLegacyMappings(script, mappings);
    }

    private static List<string> ValidateStableMappings(ParsedScript script, List<DialogueMapping> mappings)
    {
        List<string> issues = new List<string>();
        HashSet<string> knownIds = new HashSet<string>(mappings.Select(mapping => mapping.SequenceId), StringComparer.OrdinalIgnoreCase);
        HashSet<string> usedIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (DialogueLine line in script.AllLines)
        {
            if (line.SequenceIds.Count == 0)
            {
                issues.Add(
                    "Scene " + line.SceneId + " source line " + line.SourceLine + " (" + line.Speaker +
                    ") has no HEARTH:SEQUENCES marker.");
                continue;
            }

            foreach (string id in line.SequenceIds)
            {
                usedIds.Add(id);
                if (!knownIds.Contains(id))
                {
                    issues.Add("Source line " + line.SourceLine + " references unknown sequence marker " + id + ".");
                }
            }
        }

        foreach (DialogueMapping mapping in mappings)
        {
            if (!usedIds.Contains(mapping.SequenceId))
            {
                issues.Add(mapping.AssetPath + " has no marked dialogue in the final script.");
            }
        }

        return issues;
    }

    private static List<string> ValidateLegacyMappings(ParsedScript script, List<DialogueMapping> mappings)
    {
        List<string> issues = new List<string>();
        Dictionary<string, HashSet<int>> covered = new Dictionary<string, HashSet<int>>(StringComparer.OrdinalIgnoreCase);

        foreach (DialogueMapping mapping in mappings)
        {
            List<DialogueLine> sceneLines;
            if (!script.Scenes.TryGetValue(mapping.SceneId, out sceneLines))
            {
                issues.Add(mapping.AssetPath + " references missing Scene " + mapping.SceneId + ".");
                continue;
            }

            if (!covered.ContainsKey(mapping.SceneId))
            {
                covered.Add(mapping.SceneId, new HashSet<int>());
            }

            foreach (int index in mapping.LegacyIndices)
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

        foreach (KeyValuePair<string, List<DialogueLine>> scene in script.Scenes)
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

    private static List<string> FindLegacySequenceIds(DialogueLine line, List<DialogueMapping> mappings)
    {
        return mappings
            .Where(mapping =>
                string.Equals(mapping.SceneId, line.SceneId, StringComparison.OrdinalIgnoreCase) &&
                mapping.LegacyIndices.Contains(line.SceneIndex))
            .Select(mapping => mapping.SequenceId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
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
            M(lobby + "Lobby_Group01_Girl.asset", "1.1", R(11, 13), "Optional lobby girl and public-unit conversation."),
            M(lobby + "Lobby_Group01_MiaExit.asset", "1.1", I(14), "Mia's commentary after leaving the lobby girl conversation zone."),
            M(lobby + "Lobby_Group02_YoungMan.asset", "1.1", R(15, 20), "Optional young-man and work-unit conversation."),
            M(lobby + "Lobby_Group02_MiaExit.asset", "1.1", I(21), "Mia's commentary after leaving the young-man conversation zone."),
            M(lobby + "Lobby_Group03_Grandmother.asset", "1.1", R(22, 28), "Optional Mrs. Ellis and care-unit conversation."),
            M(lobby + "Lobby_Group03_MiaExit.asset", "1.1", I(29), "Mia's commentary after leaving the grandmother conversation zone."),
            M(lobby + "Lobby_AssignmentLoaded.asset", "1.1", R(30, 33), "Plays after the assignment is loaded and unlocks the elevator."),
            M(lobby + "Lobby_ElevatorRide.asset", "1.2", R(0, 14), "Elevator briefing before arrival on floor 17."),

            M(extra + "17F01_ApartmentGreeting.asset", "1.3", R(0, 2), "Final-script 17F01 apartment and terminal introduction."),
            M(root + "17F01_BedroomPrelude.asset", "1.4", R(0, 3), "17F01 bedroom playback before the soothe interaction."),
            M(root + "17F01_BedsideSoothing.asset", "1.4", R(4, 11), "17F01 soothe interaction and event archive."),
            M(root + "17F01_LivingRoomObservation.asset", "1.5", R(0, 13), "17F01 morning parent observation."),
            M(extra + "17F01_TerminalSignoffIntro.asset", "1.6", I(0), "17F01 sign-off recommendation."),
            M(extra + "17F01_TerminalSignoff_A.asset", "1.6", I(1, 2, 8), "17F01 option A sign-off and next-household direction."),
            M(extra + "17F01_TerminalSignoff_B.asset", "1.6", R(3, 8), "17F01 option B sign-off and next-household direction."),

            M(extra + "17F02_TerminalIntro.asset", "2.0", R(0, 1), "17F02 terminal introduction."),
            M(root + "17F02_BedroomWake.asset", "2.1", R(0, 6), "17F02 dormant-unit bedroom opening."),
            M(root + "17F02_BedroomConfide.asset", "2.2", R(0, 2), "17F02 companion-mode decision and Claire's account."),
            M(root + "17F02_BedroomComfort.asset", "2.2", R(3, 5), "17F02 comfort response and session archive."),
            M(root + "17F02_WifeExit.asset", "2.3", R(0, 1), "17F02 dinner call and Claire's response."),
            M(root + "17F02_DiningObservation.asset", "2.3", R(2, 10), "17F02 dining-room observation."),
            M(root + "17F02_LogAccess.asset", "2.4", R(0, 12), "17F02 household-log access and soft-guidance decision."),
            M(root + "17F02_ForcedShutdown.asset", "2.4", R(13, 15), "17F02 forced shutdown exchange."),
            M(root + "17F02_BlackAudioArgument.asset", "2.5", R(0, 28), "17F02 black-screen argument recording."),
            M(extra + "17F02_TerminalSignoffIntro.asset", "2.6", R(0, 4), "17F02 terminal recommendation."),
            M(extra + "17F02_TerminalSignoff_A.asset", "2.6", I(5, 6, 13, 14), "17F02 option A sign-off and 17F03 alert."),
            M(extra + "17F02_TerminalSignoff_B.asset", "2.6", R(7, 14), "17F02 option B sign-off and 17F03 alert."),

            M(extra + "17F03_TerminalEntry.asset", "3.1", I(0), "17F03 enter-unit instruction."),
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
            M(extra + "17F03_PostReplay_A.asset", "3.5", R(3, 7), "17F03 option A result."),
            M(extra + "17F03_PostReplay_B.asset", "3.5", I(8, 10, 11, 12, 13, 14, 15), "17F03 option B result excluding the conditional warning."),
            M(extra + "17F03_NegativeTrustSupervisorWarning.asset", "3.5", I(9), "17F03 option B warning used only when resulting trust is negative."),
            M(extra + "17F03_AllInspectionsComplete.asset", "3.5", I(16), "17F03 common completion cue after either disposition."),

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

    private static string GetFinalScriptPath()
    {
        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        return Path.Combine(projectRoot, FinalScriptFileName);
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
