#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class HearthStorySfxBinder
{
    private const string MenuRoot = "Tools/Hearth/Audio/";
    private const string AudioRootPath = "MIN_LOOP_ROOT/Audio";

    private sealed class CueSpec
    {
        public string id;
        public string objectName;
        public string note;
        public bool automatic;
        public bool loop;
        public Transform followTarget;
        public float spatialBlend;
        public float volume;
        public float minDistance;
        public float maxDistance;
        public HearthAudioChannel channel;
    }

    [MenuItem(MenuRoot + "Apply Story SFX Placeholder Setup")]
    public static void ApplySetup()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("[HearthStorySfxBinder] No loaded scene is available.");
            return;
        }

        Transform audioRoot = EnsureHierarchy(AudioRootPath);
        ConfigureGlobal(audioRoot);
        ConfigureLobby(audioRoot);
        Configure17F01(audioRoot);
        Configure17F02(audioRoot);
        Configure17F03(audioRoot);
        Configure17F04(audioRoot);
        EnsureKnownDoorSources();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[HearthStorySfxBinder] Story SFX placeholders applied. No AudioClip was imported or assigned by this tool.");
        ValidateSetup();
    }

    [MenuItem(MenuRoot + "Validate Story SFX Placeholder Setup")]
    public static void ValidateSetup()
    {
        List<string> issues = new List<string>();
        int cueCount = 0;
        int assignedClipCount = 0;
        int reservedCount = 0;

        ValidatePlayer("MIN_LOOP_ROOT/Audio/StorySFX_Global", 9, issues, ref cueCount, ref assignedClipCount, ref reservedCount);
        ValidatePlayer("MIN_LOOP_ROOT/Audio/StorySFX_Lobby", 9, issues, ref cueCount, ref assignedClipCount, ref reservedCount);
        ValidatePlayer("MIN_LOOP_ROOT/Audio/StorySFX_17F01", 6, issues, ref cueCount, ref assignedClipCount, ref reservedCount);
        ValidatePlayer("MIN_LOOP_ROOT/Audio/StorySFX_17F02", 7, issues, ref cueCount, ref assignedClipCount, ref reservedCount);
        ValidatePlayer("MIN_LOOP_ROOT/Audio/StorySFX_17F03", 6, issues, ref cueCount, ref assignedClipCount, ref reservedCount);
        ValidatePlayer("MIN_LOOP_ROOT/Audio/StorySFX_17F04", 8, issues, ref cueCount, ref assignedClipCount, ref reservedCount);

        HearthLobbyFlowController lobby = UnityEngine.Object.FindObjectOfType<HearthLobbyFlowController>(true);
        HearthCompanion17F01ReplayController replay17F01 = UnityEngine.Object.FindObjectOfType<HearthCompanion17F01ReplayController>(true);
        HearthCompanion17F02ReplayController replay17F02 = UnityEngine.Object.FindObjectOfType<HearthCompanion17F02ReplayController>(true);
        HearthCompanion17F03ReplayController replay17F03 = UnityEngine.Object.FindObjectOfType<HearthCompanion17F03ReplayController>(true);
        Hearth17F04FinaleController finale17F04 = UnityEngine.Object.FindObjectOfType<Hearth17F04FinaleController>(true);
        ValidateControllerBinding(lobby, "sfxCuePlayer", "StorySFX_Lobby", issues);
        ValidateControllerBinding(replay17F01, "sfxCuePlayer", "StorySFX_17F01", issues);
        ValidateControllerBinding(replay17F02, "sfxCuePlayer", "StorySFX_17F02", issues);
        ValidateControllerBinding(replay17F03, "sfxCuePlayer", "StorySFX_17F03", issues);
        ValidateControllerBinding(finale17F04, "sfxCuePlayer", "StorySFX_17F04", issues);

        if (replay17F02 != null)
        {
            ValidateDoor(GetObject<SmartDoorController>(replay17F02, "wifeExitDoor"), "17F02 wife exit door", issues);
        }

        if (replay17F03 != null)
        {
            ValidateDoor(GetObject<SmartDoorController>(replay17F03, "daughterDoor"), "17F03 daughter door", issues);
        }

        Transform catGuide = FindTransform("MIN_LOOP_ROOT/Finale_17F04/CatGuide");
        if (catGuide != null && catGuide.GetComponentInChildren<HearthSfxCuePlayer>(true) != null)
        {
            issues.Add("CatGuide contains a HearthSfxCuePlayer even though cat sounds are intentionally excluded.");
        }

        if (issues.Count == 0)
        {
            Debug.Log(
                "[HearthStorySfxBinder] Validation passed: " + cueCount +
                " story SFX slots, " + assignedClipCount +
                " clips currently assigned, " + reservedCount +
                " reserved timing slot(s). Empty clips are expected until final sounds are selected.");
        }
        else
        {
            Debug.LogWarning(
                "[HearthStorySfxBinder] Validation found " + issues.Count + " issue(s):\n- " +
                string.Join("\n- ", issues.ToArray()));
        }
    }

    private static void ConfigureGlobal(Transform audioRoot)
    {
        CueSpec[] specs =
        {
            Spec("UI.InteractSingle", "TBD_Global_UI_InteractSingle", "Reserved common single-press E feedback. Assign one restrained UI confirmation clip, then route special interactions here only when they do not already own a dedicated clip.", false, false, null, 0f, 0.8f, 1f, 1f),
            Spec("UI.HoldProgress", "TBD_Global_UI_HoldProgress", "Reserved loop/tick bed for long-hold interactions. The hold widgets may keep their dedicated clip fields; use this as the shared fallback.", false, true, null, 0f, 0.55f, 1f, 1f),
            Spec("UI.HoldComplete", "TBD_Global_UI_HoldComplete", "Reserved completion cue when a long-hold interaction reaches 100 percent.", false, false, null, 0f, 0.8f, 1f, 1f),
            Spec("UI.Confirm", "TBD_Global_UI_Confirm", "Reserved shared confirm/submit cue for non-terminal UI.", false, false, null, 0f, 0.8f, 1f, 1f),
            Spec("Transition.Blackout", "TBD_Global_Transition_Blackout", "Reserved short tonal transition for fade-to-black scene changes.", false, false, null, 0f, 0.65f, 1f, 1f),
            Spec("Transition.CameraMove", "TBD_Global_Transition_CameraMove", "Reserved subtle camera glide cue for smooth fixed-view transitions.", false, false, null, 0f, 0.55f, 1f, 1f),
            Spec("Terminal.PageSwitch", "TBD_Global_Terminal_PageSwitch", "Shared fallback for terminal page switching. Prefer each HearthTvTerminalController Page Switch Clip when a terminal needs a unique sound.", false, false, null, 0f, 0.7f, 1f, 1f),
            Spec("Terminal.FocusMove", "TBD_Global_Terminal_FocusMove", "Shared fallback for keyboard focus movement inside terminal menus.", false, false, null, 0f, 0.62f, 1f, 1f),
            Spec("Terminal.Submit", "TBD_Global_Terminal_Submit", "Shared fallback for terminal confirmation. Prefer the terminal Submit Clip field for per-terminal variation.", false, false, null, 0f, 0.78f, 1f, 1f),
        };

        ConfigurePlayer(audioRoot, "StorySFX_Global", specs);
    }

    private static void ConfigureLobby(Transform audioRoot)
    {
        HearthLobbyFlowController controller = UnityEngine.Object.FindObjectOfType<HearthLobbyFlowController>(true);
        if (controller == null)
        {
            Debug.LogWarning("[HearthStorySfxBinder] Lobby flow controller was not found; its SFX placeholders were not created.");
            return;
        }

        HearthTvTerminalController terminal = GetObject<HearthTvTerminalController>(controller, "assignmentTerminal");
        Transform elevatorButton = FindTransform("DIKUAIunity/Group1/Group144/Rectangle2106772232");
        CueSpec[] specs =
        {
            Spec("Lobby.RoomTone", "AUTO_Lobby_RoomTone", "Loops from the opening briefing through free lobby exploration, then stops when the elevator transition begins.", true, true, null, 0f, 0.42f, 1f, 1f, HearthAudioChannel.Ambient),
            Spec("AssignmentTerminal.Hum", "TBD_Lobby_AssignmentTerminal_Hum", "Reserved spatial terminal electrical hum. Assign the same clip to the terminal Active Loop Clip when it must start and stop exactly with the terminal screen.", false, true, terminal != null ? terminal.transform : null, 1f, 0.35f, 0.6f, 7f),
            Spec("AssignmentTerminal.Confirm", "AUTO_Lobby_AssignmentTerminal_Confirm", "Plays once when the assignment is successfully loaded.", true, false, terminal != null ? terminal.transform : null, 1f, 0.75f, 0.6f, 7f),
            Spec("Elevator.Button", "AUTO_Lobby_Elevator_Button", "Plays when the unlocked elevator button is pressed.", true, false, elevatorButton, 1f, 0.75f, 0.5f, 6f),
            Spec("Elevator.DoorsClose", "AUTO_Lobby_Elevator_DoorsClose", "Plays as the fade into the elevator begins. Replace with a door-close clip after the elevator prop timing is finalized.", true, false, elevatorButton, 1f, 0.75f, 1f, 12f),
            Spec("Elevator.Motor", "AUTO_Lobby_Elevator_Motor", "Loops while the elevator briefing is playing and stops before arrival.", true, true, null, 0f, 0.52f, 1f, 1f, HearthAudioChannel.Ambient),
            Spec("Elevator.Arrival", "AUTO_Lobby_Elevator_Arrival", "Plays after the elevator dialogue and before the floor-17 transition.", true, false, elevatorButton, 1f, 0.72f, 1f, 12f),
            Spec("Elevator.DoorsOpen", "AUTO_Lobby_Elevator_DoorsOpen", "Plays with the arrival transition before the player appears on floor 17.", true, false, elevatorButton, 1f, 0.75f, 1f, 12f),
            Spec("Corridor.RoomTone", "AUTO_17F_Corridor_RoomTone", "Starts when the player arrives on floor 17. Stop or replace it from later scene ambience controllers when entering a household replay.", true, true, null, 0f, 0.38f, 1f, 1f, HearthAudioChannel.Ambient),
        };

        HearthSfxCuePlayer player = ConfigurePlayer(audioRoot, "StorySFX_Lobby", specs);
        if (terminal != null)
        {
            Undo.RecordObject(terminal, "Bind assignment terminal active loop cue");
            terminal.SetActiveLoopCue(player, "AssignmentTerminal.Hum");
            EditorUtility.SetDirty(terminal);
        }

        Undo.RecordObject(controller, "Bind lobby story SFX");
        controller.SetSfxCuePlayer(player);
        EditorUtility.SetDirty(controller);
    }

    private static void Configure17F01(Transform audioRoot)
    {
        HearthCompanion17F01ReplayController controller = UnityEngine.Object.FindObjectOfType<HearthCompanion17F01ReplayController>(true);
        if (controller == null)
        {
            Debug.LogWarning("[HearthStorySfxBinder] 17F01 replay controller was not found; its SFX placeholders were not created.");
            return;
        }

        Transform robot = GetObject<Transform>(controller, "robotRoot");
        Transform livingAnchor = GetObject<Transform>(controller, "livingRoomStartAnchor");
        CueSpec[] specs =
        {
            Spec("Bedroom.RoomTone", "AUTO_17F01_Bedroom_RoomTone", "Loops during the boy bedroom replay. Intentionally excludes boy breathing, sheets and sleep movement sounds.", true, true, null, 0f, 0.34f, 1f, 1f, HearthAudioChannel.Ambient),
            Spec("LivingRoom.RoomTone", "AUTO_17F01_LivingRoom_RoomTone", "Loops during the living-room observation and stops when returning to the terminal.", true, true, null, 0f, 0.34f, 1f, 1f, HearthAudioChannel.Ambient),
            Spec("Replay.SceneTransition", "AUTO_17F01_Replay_SceneTransition", "Plays when the replay moves from the bedside scene to the living-room observation.", true, false, null, 0f, 0.6f, 1f, 1f),
            Spec("Interaction.ComfortConfirmed", "AUTO_17F01_Comfort_Confirmed", "Plays after the boy interaction hold completes and the soothing scene starts.", true, false, robot, 0.35f, 0.72f, 1f, 7f),
            Spec("Parent.TableFoley", "TBD_17F01_Parent_TableFoley", "Reserved restrained table or chair foley during the living-room parent conversation. Trigger it only after exact subtitle timing is chosen.", false, false, livingAnchor, 1f, 0.45f, 1f, 8f),
            Spec("Robot.ServoLoop", "TBD_17F01_Robot_ServoLoop", "Reserved companion-unit movement servo loop. Do not use human footsteps for the robot.", false, true, robot, 1f, 0.42f, 0.8f, 8f),
        };

        HearthSfxCuePlayer player = ConfigurePlayer(audioRoot, "StorySFX_17F01", specs);
        Undo.RecordObject(controller, "Bind 17F01 story SFX");
        controller.SetSfxCuePlayer(player);
        EditorUtility.SetDirty(controller);
    }

    private static void Configure17F02(Transform audioRoot)
    {
        HearthCompanion17F02ReplayController controller = UnityEngine.Object.FindObjectOfType<HearthCompanion17F02ReplayController>(true);
        if (controller == null)
        {
            Debug.LogWarning("[HearthStorySfxBinder] 17F02 replay controller was not found; its SFX placeholders were not created.");
            return;
        }

        Transform wife = GetObject<Transform>(controller, "bedroomWifeMoveRoot");
        GameObject diningWife = GetObject<GameObject>(controller, "diningWifeActor");
        Transform terminal = GetObject<Transform>(controller, "livingRoomTerminalAnchor");
        Transform robot = GetObject<Transform>(controller, "robotRoot");

        CueSpec[] specs =
        {
            Spec("Wife.SitOnBed", "TBD_17F02_Wife_SitOnBed", "Reserved: the exact subtitle/animation moment is not yet identifiable. Assign a clip now if desired, then wire PlayCue after the timing is confirmed.", false, false, wife, 1f, 0.8f, 1f, 9f),
            Spec("Wife.StandUp", "AUTO_17F02_Wife_StandUp", "Automatically plays when the SitToStand sequence starts.", true, false, wife, 1f, 0.85f, 1f, 9f),
            Spec("Wife.Walk", "AUTO_17F02_Wife_Walk", "Loop follows the bedroom wife while the scripted route is moving, pauses at the door, then resumes outside.", true, true, wife, 1f, 0.72f, 1f, 10f),
            Spec("Dining.TableFoley", "AUTO_17F02_Dining_TableFoley", "One restrained table/dish cue when the dining observation begins. Add more discrete cues later through dialogue events if needed.", true, false, diningWife != null ? diningWife.transform : null, 1f, 0.5f, 1f, 8f),
            Spec("System.DataScan", "AUTO_17F02_System_DataScan", "Plays when the living-room log access scene begins.", true, false, terminal, 0.65f, 0.65f, 1f, 8f),
            Spec("System.Glitch", "AUTO_17F02_System_Glitch", "Plays immediately before the forced-shutdown HUD glitch.", true, false, robot, 0.35f, 0.8f, 1f, 8f),
            Spec("System.PowerOff", "AUTO_17F02_System_PowerOff", "Plays after the configured shutdown glitch duration.", true, false, robot, 0.55f, 0.9f, 1f, 8f),
        };

        HearthSfxCuePlayer player = ConfigurePlayer(audioRoot, "StorySFX_17F02", specs);
        Undo.RecordObject(controller, "Bind 17F02 story SFX");
        controller.SetSfxCuePlayer(player);
        EditorUtility.SetDirty(controller);
    }

    private static void Configure17F03(Transform audioRoot)
    {
        HearthCompanion17F03ReplayController controller = UnityEngine.Object.FindObjectOfType<HearthCompanion17F03ReplayController>(true);
        if (controller == null)
        {
            Debug.LogWarning("[HearthStorySfxBinder] 17F03 replay controller was not found; its SFX placeholders were not created.");
            return;
        }

        Transform mother = GetObject<Transform>(controller, "motherMoveRoot");
        Transform daughter = GetObject<Transform>(controller, "daughterMoveRoot");
        Transform robot = GetObject<Transform>(controller, "robotRoot");

        CueSpec[] specs =
        {
            Spec("Mother.StandUp", "AUTO_17F03_Mother_StandUp", "Plays when the first-act mother SitToStand animation starts.", true, false, mother, 1f, 0.85f, 1f, 9f),
            Spec("Daughter.StandUp", "AUTO_17F03_Daughter_StandUp", "Plays after the daughter gaze interaction, when SitupToIdle begins.", true, false, daughter, 1f, 0.8f, 1f, 9f),
            Spec("Daughter.Walk", "AUTO_17F03_Daughter_Walk", "Loop follows the daughter from the opened door to the companion unit and stops at the final route point.", true, true, daughter, 1f, 0.72f, 1f, 10f),
            Spec("Daughter.Keypad", "AUTO_17F03_Daughter_Keypad", "Plays when EnteringCode begins. Use a short keypad sequence or add Animation Events later for per-key sounds.", true, false, daughter, 1f, 0.7f, 1f, 7f),
            Spec("System.Glitch", "AUTO_17F03_System_Glitch", "Plays when the deep-sleep degradation starts.", true, false, robot, 0.35f, 0.8f, 1f, 8f),
            Spec("System.PowerOff", "AUTO_17F03_System_PowerOff", "Plays after Deep Sleep Power Off Delay Seconds, while the screen is degrading.", true, false, robot, 0.55f, 0.9f, 1f, 8f),
        };

        HearthSfxCuePlayer player = ConfigurePlayer(audioRoot, "StorySFX_17F03", specs);
        Undo.RecordObject(controller, "Bind 17F03 story SFX");
        controller.SetSfxCuePlayer(player);
        EditorUtility.SetDirty(controller);
    }

    private static void Configure17F04(Transform audioRoot)
    {
        Hearth17F04FinaleController controller = UnityEngine.Object.FindObjectOfType<Hearth17F04FinaleController>(true);
        if (controller == null)
        {
            Debug.LogWarning("[HearthStorySfxBinder] 17F04 finale controller was not found; its SFX placeholders were not created.");
            return;
        }

        HearthPhotoFrameInteractable photo = GetObject<HearthPhotoFrameInteractable>(controller, "photoFrame");
        Hearth17F04HomeUnitInteractable unit = GetObject<Hearth17F04HomeUnitInteractable>(controller, "homeUnit");

        CueSpec[] specs =
        {
            Spec("Home.RoomTone", "TBD_17F04_Home_RoomTone", "Reserved living-room ambience for Mia's home. Start and stop it from the finale flow after the final environment mix is approved.", false, true, null, 0f, 0.34f, 1f, 1f, HearthAudioChannel.Ambient),
            Spec("DaughterRoom.RoomTone", "TBD_17F04_DaughterRoom_RoomTone", "Reserved daughter-room ambience. Use a separate loop only if the room needs a distinct acoustic bed from the living room.", false, true, null, 0f, 0.32f, 1f, 1f, HearthAudioChannel.Ambient),
            Spec("Photo.Memory", "AUTO_17F04_Photo_Memory", "Plays when the player starts the TV4 photo inspection. The camera transition remains controlled by the photo-frame script.", true, false, photo != null ? photo.transform : null, 1f, 0.65f, 1f, 8f),
            Spec("Popup.Spawn", "AUTO_17F04_Popup_Spawn", "Plays for each low-trust shutdown warning that enters the screen. Keep it short so dense waves remain readable.", true, false, null, 0f, 0.42f, 1f, 1f),
            Spec("Popup.Dismiss", "AUTO_17F04_Popup_Dismiss", "Plays each time Space dismisses one shutdown warning.", true, false, null, 0f, 0.5f, 1f, 1f),
            Spec("Popup.WaveEscalate", "AUTO_17F04_Popup_WaveEscalate", "Plays once as warning wave two or three begins.", true, false, null, 0f, 0.72f, 1f, 1f),
            Spec("Popup.Success", "AUTO_17F04_Popup_Success", "Plays once after all warning waves are cleared or high-trust shutdown is confirmed.", true, false, null, 0f, 0.78f, 1f, 1f),
            Spec("Unit.PowerOff", "AUTO_17F04_Unit_PowerOff", "Plays once after the high/low-trust shutdown challenge succeeds.", true, false, unit != null ? unit.transform : null, 1f, 0.9f, 1f, 9f),
        };

        HearthSfxCuePlayer player = ConfigurePlayer(audioRoot, "StorySFX_17F04", specs);
        HearthVirusPopupShutdownChallenge popupChallenge = UnityEngine.Object.FindObjectOfType<HearthVirusPopupShutdownChallenge>(true);
        if (popupChallenge != null)
        {
            Undo.RecordObject(popupChallenge, "Bind 17F04 popup SFX");
            popupChallenge.SetSfxCuePlayer(player);
            EditorUtility.SetDirty(popupChallenge);
        }

        Undo.RecordObject(controller, "Bind 17F04 story SFX");
        controller.SetSfxCuePlayer(player);
        EditorUtility.SetDirty(controller);
    }

    private static HearthSfxCuePlayer ConfigurePlayer(Transform audioRoot, string name, CueSpec[] specs)
    {
        Transform owner = audioRoot.Find(name);
        if (owner == null)
        {
            GameObject created = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(created, "Create HEARTH story SFX group");
            owner = created.transform;
            owner.SetParent(audioRoot, false);
        }

        HearthSfxCuePlayer player = owner.GetComponent<HearthSfxCuePlayer>();
        if (player == null)
        {
            player = Undo.AddComponent<HearthSfxCuePlayer>(owner.gameObject);
        }

        SerializedObject serialized = new SerializedObject(player);
        SerializedProperty cues = serialized.FindProperty("cues");
        for (int i = 0; i < specs.Length; i++)
        {
            CueSpec spec = specs[i];
            SerializedProperty cue = FindOrAddCue(cues, spec.id);
            bool wasNew = string.IsNullOrEmpty(cue.FindPropertyRelative("cueId").stringValue);
            cue.FindPropertyRelative("cueId").stringValue = spec.id;
            cue.FindPropertyRelative("placementNote").stringValue = spec.note;
            cue.FindPropertyRelative("automaticallyTriggered").boolValue = spec.automatic;
            cue.FindPropertyRelative("loop").boolValue = spec.loop;
            cue.FindPropertyRelative("channel").enumValueIndex = (int)spec.channel;
            cue.FindPropertyRelative("followTarget").objectReferenceValue = spec.followTarget;
            cue.FindPropertyRelative("followWhilePlaying").boolValue = true;
            cue.FindPropertyRelative("spatialBlend").floatValue = spec.spatialBlend;

            AudioSource source = EnsureCueSource(owner, spec);
            cue.FindPropertyRelative("source").objectReferenceValue = source;
            if (wasNew)
            {
                cue.FindPropertyRelative("restartIfPlaying").boolValue = true;
                cue.FindPropertyRelative("baseVolume").floatValue = spec.volume;
                cue.FindPropertyRelative("pitch").floatValue = 1f;
                cue.FindPropertyRelative("randomPitchRange").floatValue = spec.loop ? 0f : 0.025f;
                cue.FindPropertyRelative("minDistance").floatValue = spec.minDistance;
                cue.FindPropertyRelative("maxDistance").floatValue = spec.maxDistance;
            }
        }

        SerializedProperty logMissing = serialized.FindProperty("logMissingClips");
        if (logMissing != null)
        {
            logMissing.boolValue = false;
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
        player.SnapSourcesToTargets();
        EditorUtility.SetDirty(player);
        return player;
    }

    private static SerializedProperty FindOrAddCue(SerializedProperty cues, string cueId)
    {
        for (int i = 0; i < cues.arraySize; i++)
        {
            SerializedProperty candidate = cues.GetArrayElementAtIndex(i);
            SerializedProperty id = candidate.FindPropertyRelative("cueId");
            if (id != null && string.Equals(id.stringValue, cueId, StringComparison.OrdinalIgnoreCase))
            {
                return candidate;
            }
        }

        int index = cues.arraySize;
        cues.InsertArrayElementAtIndex(index);
        SerializedProperty added = cues.GetArrayElementAtIndex(index);
        added.FindPropertyRelative("cueId").stringValue = string.Empty;
        added.FindPropertyRelative("primaryClip").objectReferenceValue = null;
        SerializedProperty alternates = added.FindPropertyRelative("alternateClips");
        if (alternates != null)
        {
            alternates.arraySize = 0;
        }

        return added;
    }

    private static AudioSource EnsureCueSource(Transform owner, CueSpec spec)
    {
        Transform sourceTransform = owner.Find(spec.objectName);
        if (sourceTransform == null)
        {
            GameObject created = new GameObject(spec.objectName);
            Undo.RegisterCreatedObjectUndo(created, "Create HEARTH SFX source");
            sourceTransform = created.transform;
            sourceTransform.SetParent(owner, false);
        }

        if (spec.followTarget != null)
        {
            sourceTransform.position = spec.followTarget.position;
        }

        AudioSource source = sourceTransform.GetComponent<AudioSource>();
        if (source == null)
        {
            source = Undo.AddComponent<AudioSource>(sourceTransform.gameObject);
        }

        source.playOnAwake = false;
        source.loop = spec.loop;
        source.spatialBlend = spec.spatialBlend;
        source.minDistance = spec.minDistance;
        source.maxDistance = spec.maxDistance;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.dopplerLevel = 0f;

        HearthAudioChannelSource channel = sourceTransform.GetComponent<HearthAudioChannelSource>();
        if (channel == null)
        {
            channel = Undo.AddComponent<HearthAudioChannelSource>(sourceTransform.gameObject);
        }

        channel.Configure(source, spec.channel, spec.volume);
        EditorUtility.SetDirty(source);
        EditorUtility.SetDirty(channel);
        return source;
    }

    private static void EnsureKnownDoorSources()
    {
        HearthCompanion17F02ReplayController replay17F02 = UnityEngine.Object.FindObjectOfType<HearthCompanion17F02ReplayController>(true);
        HearthCompanion17F03ReplayController replay17F03 = UnityEngine.Object.FindObjectOfType<HearthCompanion17F03ReplayController>(true);
        EnsureDoorSource(replay17F02 != null ? GetObject<SmartDoorController>(replay17F02, "wifeExitDoor") : null);
        EnsureDoorSource(replay17F03 != null ? GetObject<SmartDoorController>(replay17F03, "daughterDoor") : null);
    }

    private static void EnsureDoorSource(SmartDoorController door)
    {
        if (door == null)
        {
            return;
        }

        AudioSource source = door.GetComponent<AudioSource>();
        if (source == null)
        {
            source = Undo.AddComponent<AudioSource>(door.gameObject);
        }

        source.playOnAwake = false;
        source.spatialBlend = 1f;
        source.minDistance = 1f;
        source.maxDistance = 12f;
        source.dopplerLevel = 0f;

        HearthAudioChannelSource channel = source.GetComponent<HearthAudioChannelSource>();
        if (channel == null)
        {
            channel = Undo.AddComponent<HearthAudioChannelSource>(door.gameObject);
        }

        channel.Configure(source, HearthAudioChannel.SFX, 0.8f);
        SetObject(door, "audioSource", source);
        EditorUtility.SetDirty(source);
        EditorUtility.SetDirty(channel);
        EditorUtility.SetDirty(door);
    }

    private static CueSpec Spec(
        string id,
        string objectName,
        string note,
        bool automatic,
        bool loop,
        Transform followTarget,
        float spatialBlend,
        float volume,
        float minDistance,
        float maxDistance,
        HearthAudioChannel channel = HearthAudioChannel.SFX)
    {
        return new CueSpec
        {
            id = id,
            objectName = objectName,
            note = note,
            automatic = automatic,
            loop = loop,
            followTarget = followTarget,
            spatialBlend = spatialBlend,
            volume = volume,
            minDistance = minDistance,
            maxDistance = maxDistance,
            channel = channel,
        };
    }

    private static Transform EnsureHierarchy(string path)
    {
        string[] parts = path.Split('/');
        Transform current = null;
        for (int i = 0; i < parts.Length; i++)
        {
            Transform next = current == null
                ? FindRoot(parts[i])
                : current.Find(parts[i]);
            if (next == null)
            {
                GameObject created = new GameObject(parts[i]);
                Undo.RegisterCreatedObjectUndo(created, "Create HEARTH audio hierarchy");
                next = created.transform;
                if (current != null)
                {
                    next.SetParent(current, false);
                }
            }

            current = next;
        }

        return current;
    }

    private static Transform FindRoot(string name)
    {
        GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i].name == name)
            {
                return roots[i].transform;
            }
        }

        return null;
    }

    private static Transform FindTransform(string path)
    {
        string[] parts = path.Split('/');
        Transform current = FindRoot(parts[0]);
        for (int i = 1; i < parts.Length && current != null; i++)
        {
            current = current.Find(parts[i]);
        }

        return current;
    }

    private static T GetObject<T>(UnityEngine.Object owner, string propertyName) where T : UnityEngine.Object
    {
        if (owner == null)
        {
            return null;
        }

        SerializedObject serialized = new SerializedObject(owner);
        SerializedProperty property = serialized.FindProperty(propertyName);
        return property != null ? property.objectReferenceValue as T : null;
    }

    private static void SetObject(UnityEngine.Object owner, string propertyName, UnityEngine.Object value)
    {
        if (owner == null)
        {
            return;
        }

        SerializedObject serialized = new SerializedObject(owner);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null)
        {
            return;
        }

        property.objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void ValidatePlayer(
        string path,
        int expectedMinimumCues,
        List<string> issues,
        ref int cueCount,
        ref int assignedClipCount,
        ref int reservedCount)
    {
        Transform transform = FindTransform(path);
        HearthSfxCuePlayer player = transform != null ? transform.GetComponent<HearthSfxCuePlayer>() : null;
        if (player == null)
        {
            issues.Add("Missing HearthSfxCuePlayer at " + path + ".");
            return;
        }

        cueCount += player.CueCount;
        assignedClipCount += player.AssignedClipCount;
        if (player.CueCount < expectedMinimumCues)
        {
            issues.Add(path + " has " + player.CueCount + " cues; expected at least " + expectedMinimumCues + ".");
        }

        SerializedObject serialized = new SerializedObject(player);
        SerializedProperty cues = serialized.FindProperty("cues");
        for (int i = 0; i < cues.arraySize; i++)
        {
            SerializedProperty cue = cues.GetArrayElementAtIndex(i);
            string cueId = cue.FindPropertyRelative("cueId").stringValue;
            AudioSource source = cue.FindPropertyRelative("source").objectReferenceValue as AudioSource;
            bool automatic = cue.FindPropertyRelative("automaticallyTriggered").boolValue;
            Transform target = cue.FindPropertyRelative("followTarget").objectReferenceValue as Transform;
            float spatialBlend = cue.FindPropertyRelative("spatialBlend").floatValue;
            if (source == null)
            {
                issues.Add(path + " / " + cueId + " has no AudioSource.");
            }

            if (automatic && spatialBlend > 0f && target == null)
            {
                issues.Add(path + " / " + cueId + " is spatial and automatic but has no follow target.");
            }

            if (!automatic)
            {
                reservedCount++;
            }
        }
    }

    private static void ValidateControllerBinding(UnityEngine.Object owner, string propertyName, string expectedName, List<string> issues)
    {
        HearthSfxCuePlayer player = GetObject<HearthSfxCuePlayer>(owner, propertyName);
        if (player == null || player.name != expectedName)
        {
            issues.Add((owner != null ? owner.name : "<missing controller>") + " is not bound to " + expectedName + ".");
        }
    }

    private static void ValidateDoor(SmartDoorController door, string label, List<string> issues)
    {
        if (door == null)
        {
            issues.Add(label + " reference is missing.");
            return;
        }

        AudioSource source = door.GetComponent<AudioSource>();
        HearthAudioChannelSource channel = door.GetComponent<HearthAudioChannelSource>();
        if (source == null || channel == null || channel.Channel != HearthAudioChannel.SFX)
        {
            issues.Add(label + " does not have an SFX AudioSource and HearthAudioChannelSource.");
        }
    }
}
#endif
