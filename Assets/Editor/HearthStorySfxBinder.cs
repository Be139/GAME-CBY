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
    private const string CatalogPath = "Assets/Audio/HEARTH/HearthSfxCatalog.asset";

    private sealed class CatalogSpec
    {
        public string id;
        public string path;
        public string note;

        public CatalogSpec(string id, string path, string note)
        {
            this.id = id;
            this.path = path;
            this.note = note;
        }
    }

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

    [MenuItem(MenuRoot + "Apply Production Story SFX Setup")]
    public static void ApplyProductionSetup()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("[HearthStorySfxBinder] No loaded scene is available.");
            return;
        }

        HearthSfxCatalog catalog = EnsureCatalog();
        Transform audioRoot = EnsureHierarchy(AudioRootPath);
        ConfigureGlobal(audioRoot);
        ConfigureLobby(audioRoot);
        Configure17F01(audioRoot);
        Configure17F02(audioRoot);
        Configure17F03(audioRoot);
        Configure17F04(audioRoot);
        EnsureKnownDoorSources();
        BindDirectAudioHooks(catalog);
        BindFootstepProfiles(catalog);
        BindEpilogueDialogueTrack();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[HearthStorySfxBinder] Production story SFX setup applied from the central catalog. Source files remain untouched; reusable clips and source segments are referenced non-destructively.");
        ValidateProductionSetup();
    }

    [MenuItem(MenuRoot + "Apply Story SFX Placeholder Setup")]
    public static void ApplySetup()
    {
        ApplyProductionSetup();
    }

    [MenuItem(MenuRoot + "Validate Production Story SFX Setup")]
    public static void ValidateProductionSetup()
    {
        List<string> issues = new List<string>();
        int cueCount = 0;
        int assignedClipCount = 0;
        int reservedCount = 0;

        ValidatePlayer("MIN_LOOP_ROOT/Audio/StorySFX_Global", 9, issues, ref cueCount, ref assignedClipCount, ref reservedCount);
        HearthSfxCatalog catalog = AssetDatabase.LoadAssetAtPath<HearthSfxCatalog>(CatalogPath);
        if (catalog == null || catalog.EntryCount < GetCatalogSpecs().Length || catalog.AssignedClipCount < GetCatalogSpecs().Length)
        {
            issues.Add("Central SFX catalog is missing entries or clips at " + CatalogPath + ".");
        }

        ValidatePlayer("MIN_LOOP_ROOT/Audio/StorySFX_Lobby", 11, issues, ref cueCount, ref assignedClipCount, ref reservedCount);
        ValidatePlayer("MIN_LOOP_ROOT/Audio/StorySFX_17F01", 6, issues, ref cueCount, ref assignedClipCount, ref reservedCount);
        ValidatePlayer("MIN_LOOP_ROOT/Audio/StorySFX_17F02", 11, issues, ref cueCount, ref assignedClipCount, ref reservedCount);
        ValidatePlayer("MIN_LOOP_ROOT/Audio/StorySFX_17F03", 6, issues, ref cueCount, ref assignedClipCount, ref reservedCount);
        ValidatePlayer("MIN_LOOP_ROOT/Audio/StorySFX_17F04", 14, issues, ref cueCount, ref assignedClipCount, ref reservedCount);

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
                " resolved clips, " + reservedCount +
                " reserved timing slot(s), and a complete shared catalog.");
        }
        else
        {
            Debug.LogWarning(
                "[HearthStorySfxBinder] Validation found " + issues.Count + " issue(s): " +
                string.Join(" | ", issues.ToArray()));
        }
    }

    [MenuItem(MenuRoot + "Validate Story SFX Placeholder Setup")]
    public static void ValidateSetup()
    {
        ValidateProductionSetup();
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
        Transform lobbyWallaAnchor = FindTransform("1F (1)/casual_Male_G@Sitting (1)/space1");
        if (lobbyWallaAnchor == null)
        {
            lobbyWallaAnchor = GetObject<Transform>(controller, "lobbyStartAnchor");
        }
        CueSpec[] specs =
        {
            Spec("Lobby.RoomTone", "AUTO_Lobby_RoomTone", "Loops from the opening briefing through free lobby exploration, then stops when the elevator transition begins.", true, true, null, 0f, 0.28f, 1f, 1f, HearthAudioChannel.Ambient),
            Spec("Lobby.Walla", "AUTO_Lobby_WaitingArea_Walla", "Extremely quiet local 3D adult walla near the lobby waiting area. It ducks by about 5 dB whenever dialogue is playing.", true, true, lobbyWallaAnchor, 1f, 0.06f, 0.8f, 9f, HearthAudioChannel.Ambient),
            Spec("Lily.MessageNotification", "AUTO_Lobby_Lily_Message_Notification", "Plays once immediately before Lily's incoming voice message begins.", true, false, null, 0f, 0.65f, 1f, 1f),
            Spec("AssignmentTerminal.Hum", "AUTO_Lobby_AssignmentTerminal_Hum", "A restrained spatial device bed while the assignment terminal is open.", true, true, terminal != null ? terminal.transform : null, 1f, 0.08f, 0.6f, 7f, HearthAudioChannel.Ambient),
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
            Spec("Wife.SitOnBed", "AUTO_17F02_Wife_SitOnBed", "Plays at the start of the bedroom wake sequence.", true, false, wife, 1f, 0.75f, 1f, 9f),
            Spec("Wife.StandUp", "AUTO_17F02_Wife_StandUp", "Automatically plays when the SitToStand sequence starts.", true, false, wife, 1f, 0.85f, 1f, 9f),
            Spec("Wife.Walk", "AUTO_17F02_Wife_Walk", "Loop follows the bedroom wife while the scripted route is moving, pauses at the door, then resumes outside.", true, true, wife, 1f, 0.72f, 1f, 10f),
            Spec("Bedroom.Jazz", "AUTO_17F02_Bedroom_SoftJazz", "Quiet source music during the bedroom comfort exchange; automatically ducks under dialogue.", true, true, wife, 0.65f, 0.12f, 1f, 9f, HearthAudioChannel.Ambient),
            Spec("Dining.TableFoley", "AUTO_17F02_Dining_TableFoley", "One restrained table/dish cue when the dining observation begins. Add more discrete cues later through dialogue events if needed.", true, false, diningWife != null ? diningWife.transform : null, 1f, 0.5f, 1f, 8f),
            Spec("System.DataScan", "AUTO_17F02_System_DataScan", "Plays when the living-room log access scene begins.", true, false, terminal, 0.65f, 0.65f, 1f, 8f),
            Spec("Bathroom.Shower", "AUTO_17F02_Bathroom_Shower", "Muffled shower water during the terminal/log access scene; stops before forced shutdown.", true, true, terminal, 0.75f, 0.13f, 1f, 10f, HearthAudioChannel.Ambient),
            Spec("System.Glitch", "AUTO_17F02_System_Glitch", "Plays immediately before the forced-shutdown HUD glitch.", true, false, robot, 0.35f, 0.8f, 1f, 8f),
            Spec("System.PowerOff", "AUTO_17F02_System_PowerOff", "Plays after the configured shutdown glitch duration.", true, false, robot, 0.55f, 0.9f, 1f, 8f),
            Spec("BlackAudio.Fridge", "AUTO_17F02_BlackAudio_Fridge", "Kitchen refrigerator/room tone during the black-screen argument.", true, true, null, 0f, 0.16f, 1f, 1f, HearthAudioChannel.Ambient),
            Spec("BlackAudio.Traffic", "AUTO_17F02_BlackAudio_Traffic", "Very distant city traffic during the black-screen argument.", true, true, null, 0f, 0.07f, 1f, 1f, HearthAudioChannel.Ambient),
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
            Spec("Epilogue.PathA.Frying", "AUTO_17F04_Epilogue_PathA_Frying", "Path A frying bed, started and stopped from exact epilogue subtitle line IDs.", true, true, null, 0f, 0.22f, 1f, 1f, HearthAudioChannel.Ambient),
            Spec("Epilogue.PathA.Keys", "AUTO_17F04_Epilogue_PathA_Keys", "Path A keys drop on Mia's home arrival line.", true, false, null, 0f, 0.75f, 1f, 1f),
            Spec("Epilogue.PathA.Thunder", "AUTO_17F04_Epilogue_PathA_Thunder", "Path A distant thunder/rain bed for Lily's room exchange.", true, true, null, 0f, 0.18f, 1f, 1f, HearthAudioChannel.Ambient),
            Spec("Epilogue.PathB.Gym", "AUTO_17F04_Epilogue_PathB_Gym", "Path B distant open-house gym ambience.", true, true, null, 0f, 0.16f, 1f, 1f, HearthAudioChannel.Ambient),
            Spec("Epilogue.PathB.SuitcaseImpact", "AUTO_17F04_Epilogue_PathB_SuitcaseImpact", "Path B suitcase threshold impact.", true, false, null, 0f, 0.75f, 1f, 1f),
            Spec("Epilogue.PathB.SuitcaseRolling", "AUTO_17F04_Epilogue_PathB_SuitcaseRolling", "Path B suitcase rolling loop until Lily has left.", true, true, null, 0f, 0.34f, 1f, 1f),
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
        SerializedProperty catalogProperty = serialized.FindProperty("catalog");
        if (catalogProperty != null)
        {
            catalogProperty.objectReferenceValue = AssetDatabase.LoadAssetAtPath<HearthSfxCatalog>(CatalogPath);
        }

        SerializedProperty cues = serialized.FindProperty("cues");
        for (int i = 0; i < specs.Length; i++)
        {
            CueSpec spec = specs[i];
            SerializedProperty cue = FindOrAddCue(cues, spec.id);
            cue.FindPropertyRelative("cueId").stringValue = spec.id;
            cue.FindPropertyRelative("soundId").stringValue = ResolveSoundId(spec.id);
            cue.FindPropertyRelative("placementNote").stringValue = spec.note;
            cue.FindPropertyRelative("automaticallyTriggered").boolValue = spec.automatic;
            cue.FindPropertyRelative("loop").boolValue = spec.loop;
            cue.FindPropertyRelative("channel").enumValueIndex = (int)spec.channel;
            cue.FindPropertyRelative("followTarget").objectReferenceValue = spec.followTarget;
            cue.FindPropertyRelative("followWhilePlaying").boolValue = true;
            cue.FindPropertyRelative("spatialBlend").floatValue = spec.spatialBlend;
            cue.FindPropertyRelative("playFromSeconds").floatValue = ResolveSegmentStart(spec.id);
            cue.FindPropertyRelative("playDurationSeconds").floatValue = ResolveSegmentDuration(spec.id);
            cue.FindPropertyRelative("duckWhileDialogue").boolValue = ShouldDuckWhileDialogue(spec.id);
            cue.FindPropertyRelative("dialogueDuckScale").floatValue = 0.56f;

            AudioSource source = EnsureCueSource(owner, spec);
            cue.FindPropertyRelative("source").objectReferenceValue = source;
            cue.FindPropertyRelative("restartIfPlaying").boolValue = true;
            cue.FindPropertyRelative("baseVolume").floatValue = spec.volume;
            cue.FindPropertyRelative("pitch").floatValue = 1f;
            cue.FindPropertyRelative("randomPitchRange").floatValue = spec.loop ? 0f : 0.025f;
            cue.FindPropertyRelative("minDistance").floatValue = spec.minDistance;
            cue.FindPropertyRelative("maxDistance").floatValue = spec.maxDistance;
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
        channel.ConfigureDialogueDucking(ShouldDuckWhileDialogue(spec.id), 0.56f);
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

    private static HearthSfxCatalog EnsureCatalog()
    {
        HearthSfxCatalog catalog = AssetDatabase.LoadAssetAtPath<HearthSfxCatalog>(CatalogPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<HearthSfxCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
        }

        CatalogSpec[] specs = GetCatalogSpecs();
        SerializedObject serialized = new SerializedObject(catalog);
        SerializedProperty entries = serialized.FindProperty("entries");
        entries.arraySize = specs.Length;
        for (int i = 0; i < specs.Length; i++)
        {
            CatalogSpec spec = specs[i];
            SerializedProperty entry = entries.GetArrayElementAtIndex(i);
            entry.FindPropertyRelative("soundId").stringValue = spec.id;
            entry.FindPropertyRelative("maintenanceNote").stringValue = spec.note;
            entry.FindPropertyRelative("primaryClip").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<AudioClip>(spec.path);
            SerializedProperty alternates = entry.FindPropertyRelative("alternateClips");
            if (alternates != null)
            {
                alternates.arraySize = 0;
            }
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(catalog);
        AssetDatabase.SaveAssets();
        return catalog;
    }

    private static CatalogSpec[] GetCatalogSpecs()
    {
        const string imported = "Assets/Audio/HEARTH/Imported/";
        return new[]
        {
            new CatalogSpec("AMB.Interior.Main", imported + "Ambience/AMB01_GLOBAL_AllInteriors_RoomTone_Main_01.mp3", "Primary reusable interior room tone."),
            new CatalogSpec("AMB.Interior.Alt", imported + "Ambience/AMB01_GLOBAL_AllInteriors_RoomTone_Alt_CityApartment_01.mp3", "Alternate city-apartment interior room tone."),
            new CatalogSpec("AMB.Elevator.DoorsOpen", imported + "Ambience/AMB04_LOBBY_ElevatorRide_DoorsOpen_01.mp3", "Elevator door-open transition."),
            new CatalogSpec("AMB.Elevator.Motor", imported + "Ambience/AMB04_LOBBY_ElevatorRide_MotorLoop_01.mp3", "Elevator ride motor loop."),
            new CatalogSpec("AMB.Elevator.Arrival", imported + "Ambience/AMB04_LOBBY_ElevatorRide_ArrivalChime_01.mp3", "Elevator arrival chime."),
            new CatalogSpec("AMB.17F02.Fridge", imported + "Ambience/AMB05_17F02_BlackAudioArgument_KitchenFridgeRoomTone_SourceFull_01.mp3", "17F02 black-audio kitchen refrigerator room tone source."),
            new CatalogSpec("AMB.17F02.Traffic", imported + "Ambience/AMB06_17F02_BlackAudioArgument_DistantCityTraffic_SourceFull_01.mp3", "17F02 distant city traffic source."),
            new CatalogSpec("AMB.17F02.Shower", imported + "Ambience/AMB07_17F02_BathroomBehindDoor_ShowerWater_SourceFull_01.mp3", "17F02 muffled shower-water source."),
            new CatalogSpec("AMB.17F04.Gym", imported + "Ambience/AMB08_17F04_PATHB_SchoolOpenHouse_DistantGymHum_SourceFull_01.mp3", "17F04 Path B distant gym/open-house ambience."),
            new CatalogSpec("AMB.Lobby.Walla", imported + "Ambience/AMB09_LOBBY_WaitingArea_Walla_SmallGroup_01.mp3", "Very quiet, local 3D lobby waiting-area walla; keep 18-24 dB below dialogue."),
            new CatalogSpec("UI.Navigation", imported + "UI/UI01_GLOBAL_NavigationAndOption_FocusMove_01.mp3", "Shared navigation/page/focus movement."),
            new CatalogSpec("UI.DecisionFocus", imported + "UI/UI01_GLOBAL_DecisionAB_FocusMove_01.mp3", "Decision A/B focus movement."),
            new CatalogSpec("UI.Close", imported + "UI/UI01_GLOBAL_HumanHUDAndTerminal_Close_01.mp3", "Human HUD and terminal close/cancel."),
            new CatalogSpec("UI.Interact", imported + "UI/UI02_GLOBAL_SinglePressE_ActionConfirm_01.mp3", "Valid short-press E interaction confirmation; never used for invalid feedback."),
            new CatalogSpec("UI.Submit", imported + "UI/UI02_GLOBAL_DecisionAB_SubmitConfirm_01.mp3", "Decision/terminal submit confirmation."),
            new CatalogSpec("UI.HoldProgress", imported + "UI/UI03_GLOBAL_HoldE_ActionProgress_Loop_01.mp3", "Long-hold E progress loop."),
            new CatalogSpec("UI.HoldComplete", imported + "UI/UI04_GLOBAL_HoldE_ActionComplete_01.mp3", "Long-hold E completion."),
            new CatalogSpec("UI.LilyNotification", imported + "UI/UI05_LOBBY_LilyMessage_EarpieceNotification_01.mp3", "Lily voice-message earpiece notification."),
            new CatalogSpec("UI.Warning", imported + "UI/UI06_GLOBAL_HighRiskWarning_01.mp3", "High-risk warning."),
            new CatalogSpec("UI.Popup", imported + "UI/UI06_17F04_ShutdownPopup_SpawnError_01.mp3", "17F04 shutdown popup spawn/error."),
            new CatalogSpec("SYS.ViewSwitch", imported + "System/SYS01_SHARED_17F01_17F03_HumanCompanion_ViewSwitch_01.mp3", "Human/companion view switch."),
            new CatalogSpec("SYS.TerminalCamera", imported + "System/SYS01_GLOBAL_HumanTerminal_CameraTransition_01.mp3", "Terminal camera/fixed-view transition."),
            new CatalogSpec("SYS.Glitch", imported + "System/SYS02_SHARED_17F02_17F03_Companion_ShutdownGlitch_01.mp3", "Companion shutdown/degradation glitch."),
            new CatalogSpec("SYS.TerminalPower", imported + "System/SYS03_GLOBAL_AllTerminals_DevicePowerOn_01.mp3", "Terminal device power-on."),
            new CatalogSpec("SYS.PowerOff", imported + "System/SYS03_SHARED_17F02_17F03_17F04_Companion_PowerOff_01.mp3", "Companion power-off."),
            new CatalogSpec("MUSIC.17F02.Jazz", imported + "Music/MUS01_17F02_BedroomComfort_SoftJazz_SourceFull_01.mp3", "17F02 bedroom comfort soft-jazz source."),
            new CatalogSpec("FOLEY.Door", imported + "Foley/FOL01_GLOBAL_ResidentialDoor_OpenClose_SourceFull_01.mp3", "Residential open/close source; runtime uses non-destructive halves."),
            new CatalogSpec("FOLEY.SitBed", imported + "Foley/FOL03_17F02_BedroomWake_ClaireSitOnBed_01.mp3", "Claire sits on bed."),
            new CatalogSpec("FOLEY.SitStand", imported + "Foley/FOL03_SHARED_17F02_17F03_CharacterSitStand_SourceFull_01.mp3", "Reusable character sit/stand source."),
            new CatalogSpec("FOLEY.RobotTracked", imported + "Foley/FOL04_SHARED_17F01_17F03_CompanionTrackedMovement_SourceFull_01.mp3", "Light tracked/toy-like companion movement; never tank or diesel."),
            new CatalogSpec("FOLEY.Table", imported + "Foley/FOL05_SHARED_17F01_17F02_ParentDining_TableFoley_SourceFull_01.mp3", "Restrained table/dining foley source."),
            new CatalogSpec("FOLEY.Frying", imported + "Foley/FOL06_17F04_PATHA_EpilogueKitchen_FryingBed_01.mp3", "17F04 Path A frying bed."),
            new CatalogSpec("FOLEY.Keys", imported + "Foley/FOL07_17F04_PATHA_EpilogueHomeFromSchool_KeysDrop_01.mp3", "17F04 Path A keys drop."),
            new CatalogSpec("FOLEY.Thunder", imported + "Foley/FOL08_17F04_PATHA_EpilogueLilyRoom_ThunderRainstorm_SourceFull_01.mp3", "17F04 Path A distant thunder/rain source."),
            new CatalogSpec("FOLEY.SuitcaseImpact", imported + "Foley/FOL09_17F04_PATHB_EpilogueFrontHall_SuitcaseThresholdImpact_01.mp3", "17F04 Path B suitcase threshold impact."),
            new CatalogSpec("FOLEY.SuitcaseRolling", imported + "Foley/FOL09_17F04_PATHB_EpilogueFrontHall_SuitcaseRolling_SourceFull_01.mp3", "17F04 Path B suitcase rolling source."),
            new CatalogSpec("FOLEY.HumanSteps", "Assets/Mini First Person Controller/Audio/Steps.wav", "Existing reusable human footstep recording."),
        };
    }

    private static void BindDirectAudioHooks(HearthSfxCatalog catalog)
    {
        HearthSfxCuePlayer globalPlayer = null;
        Transform globalTransform = FindTransform(AudioRootPath + "/StorySFX_Global");
        if (globalTransform != null)
        {
            globalPlayer = globalTransform.GetComponent<HearthSfxCuePlayer>();
        }

        HearthTvTerminalController[] terminals = UnityEngine.Object.FindObjectsOfType<HearthTvTerminalController>(true);
        for (int i = 0; i < terminals.Length; i++)
        {
            HearthTvTerminalController terminal = terminals[i];
            AudioSource source = EnsureAudioSource(terminal.gameObject, false);
            BindChannel(source, HearthAudioChannel.SFX, 0.68f);
            SetObject(terminal, "audioSource", source);
            SetObject(terminal, "openClip", Clip(catalog, "SYS.TerminalCamera"));
            SetObject(terminal, "closeClip", Clip(catalog, "UI.Close"));
            SetObject(terminal, "bootClip", Clip(catalog, "SYS.TerminalPower"));
            SetObject(terminal, "pageSwitchClip", Clip(catalog, "UI.Navigation"));
            SetObject(terminal, "focusMoveClip", Clip(catalog, "UI.DecisionFocus"));
            SetObject(terminal, "submitClip", Clip(catalog, "UI.Submit"));
            SetObject(terminal, "replayRequestClip", Clip(catalog, "UI.Interact"));
            SetObject(terminal, "viewSwitchClip", Clip(catalog, "SYS.ViewSwitch"));
            EditorUtility.SetDirty(terminal);
        }

        HearthFirstPersonHudController[] humanHuds = UnityEngine.Object.FindObjectsOfType<HearthFirstPersonHudController>(true);
        for (int i = 0; i < humanHuds.Length; i++)
        {
            HearthFirstPersonHudController hud = humanHuds[i];
            AudioSource source = EnsureAudioSource(hud.gameObject, false);
            BindChannel(source, HearthAudioChannel.SFX, 0.65f);
            SetObject(hud, "audioSource", source);
            SetObject(hud, "openMenuClip", Clip(catalog, "UI.Interact"));
            SetObject(hud, "closeMenuClip", Clip(catalog, "UI.Close"));
            SetObject(hud, "pageChangedClip", Clip(catalog, "UI.Navigation"));
            SetObject(hud, "focusMovedClip", Clip(catalog, "UI.DecisionFocus"));
            SetObject(hud, "confirmClip", Clip(catalog, "UI.Submit"));
            SetObject(hud, "cancelClip", Clip(catalog, "UI.Close"));
            SetObject(hud, "warningClip", Clip(catalog, "UI.Warning"));
            SetObject(hud, "trustDeltaClip", Clip(catalog, "UI.Submit"));
            EditorUtility.SetDirty(hud);
        }

        HearthCompanionHudController[] companionHuds = UnityEngine.Object.FindObjectsOfType<HearthCompanionHudController>(true);
        for (int i = 0; i < companionHuds.Length; i++)
        {
            HearthCompanionHudController hud = companionHuds[i];
            AudioSource source = EnsureAudioSource(hud.gameObject, false);
            BindChannel(source, HearthAudioChannel.SFX, 0.62f);
            SetObject(hud, "audioSource", source);
            SetObject(hud, "sceneChangedClip", Clip(catalog, "UI.Navigation"));
            SetObject(hud, "holdCompletedClip", null);
            SetObject(hud, "specialEffectClip", Clip(catalog, "SYS.Glitch"));
            EditorUtility.SetDirty(hud);
        }

        HearthCompanionHoldPrompt[] holdPrompts = UnityEngine.Object.FindObjectsOfType<HearthCompanionHoldPrompt>(true);
        for (int i = 0; i < holdPrompts.Length; i++)
        {
            Undo.RecordObject(holdPrompts[i], "Bind long-hold E audio");
            holdPrompts[i].SetSfxCuePlayer(globalPlayer);
            EditorUtility.SetDirty(holdPrompts[i]);
        }

        SmartDoorController[] doors = UnityEngine.Object.FindObjectsOfType<SmartDoorController>(true);
        AudioClip doorClip = Clip(catalog, "FOLEY.Door");
        float half = doorClip != null ? doorClip.length * 0.5f : 0f;
        for (int i = 0; i < doors.Length; i++)
        {
            SmartDoorController door = doors[i];
            AudioSource source = EnsureAudioSource(door.gameObject, true);
            BindChannel(source, HearthAudioChannel.SFX, 0.72f);
            SetObject(door, "audioSource", source);
            SetObject(door, "openClip", doorClip);
            SetObject(door, "closeClip", doorClip);
            SetObject(door, "lockedClip", null);
            SetFloat(door, "openClipStartSeconds", 0f);
            SetFloat(door, "openClipDurationSeconds", half);
            SetFloat(door, "closeClipStartSeconds", half);
            SetFloat(door, "closeClipDurationSeconds", half);
            EditorUtility.SetDirty(door);
        }
    }

    private static void BindFootstepProfiles(HearthSfxCatalog catalog)
    {
        FirstPersonAudio[] audioOwners = UnityEngine.Object.FindObjectsOfType<FirstPersonAudio>(true);
        for (int i = 0; i < audioOwners.Length; i++)
        {
            FirstPersonAudio firstPersonAudio = audioOwners[i];
            bool companion = firstPersonAudio.transform.root.name.IndexOf("Robot", StringComparison.OrdinalIgnoreCase) >= 0 ||
                firstPersonAudio.transform.name.IndexOf("Robot", StringComparison.OrdinalIgnoreCase) >= 0 ||
                firstPersonAudio.transform.parent != null && firstPersonAudio.transform.parent.name.IndexOf("Robot", StringComparison.OrdinalIgnoreCase) >= 0;
            HearthFootstepRole role = companion ? HearthFootstepRole.Companion : HearthFootstepRole.Human;
            HearthFootstepAudioProfile profile = firstPersonAudio.GetComponent<HearthFootstepAudioProfile>();
            if (profile == null)
            {
                profile = Undo.AddComponent<HearthFootstepAudioProfile>(firstPersonAudio.gameObject);
            }

            AudioClip clip = Clip(catalog, companion ? "FOLEY.RobotTracked" : "FOLEY.HumanSteps");
            Undo.RecordObject(profile, "Bind HEARTH footstep profile");
            profile.Configure(role, firstPersonAudio, clip, clip, companion ? 0.9f : 1f, companion ? 1.08f : 1.25f);
            BindChannel(firstPersonAudio.stepAudio, HearthAudioChannel.SFX, companion ? 0.32f : 0.58f);
            BindChannel(firstPersonAudio.runningAudio, HearthAudioChannel.SFX, companion ? 0.38f : 0.65f);
            EditorUtility.SetDirty(profile);
        }
    }

    private static void BindEpilogueDialogueTrack()
    {
        Hearth17F04FinaleController finale = UnityEngine.Object.FindObjectOfType<Hearth17F04FinaleController>(true);
        if (finale == null)
        {
            return;
        }

        MinLoopSubtitlePlayer subtitlePlayer = GetObject<MinLoopSubtitlePlayer>(finale, "epilogueSubtitlePlayer");
        HearthSfxCuePlayer cuePlayer = GetObject<HearthSfxCuePlayer>(finale, "sfxCuePlayer");
        if (subtitlePlayer == null || cuePlayer == null)
        {
            return;
        }

        HearthDialogueSfxTrack track = subtitlePlayer.GetComponent<HearthDialogueSfxTrack>();
        if (track == null)
        {
            track = Undo.AddComponent<HearthDialogueSfxTrack>(subtitlePlayer.gameObject);
        }

        List<HearthDialogueSfxTrack.CueAction> actions = new List<HearthDialogueSfxTrack.CueAction>();
        string[] shutdownSequences = { "17F04_Epilogue_High_Shutdown", "17F04_Epilogue_Low_Shutdown" };
        for (int i = 0; i < shutdownSequences.Length; i++)
        {
            AddDialogueAction(actions, shutdownSequences[i], "17F04_Epilogue_High_Shutdown_17F04_Epilogue_Low_Shutdown_Lily_001", HearthDialogueSfxActionType.StartLoop, "Epilogue.PathA.Frying");
            AddDialogueAction(actions, shutdownSequences[i], "17F04_Epilogue_High_Shutdown_17F04_Epilogue_Low_Shutdown_Mia_003", HearthDialogueSfxActionType.Stop, "Epilogue.PathA.Frying");
            AddDialogueAction(actions, shutdownSequences[i], "17F04_Epilogue_High_Shutdown_17F04_Epilogue_Low_Shutdown_Mia_003", HearthDialogueSfxActionType.PlayOneShot, "Epilogue.PathA.Keys");
            AddDialogueAction(actions, shutdownSequences[i], "17F04_Epilogue_High_Shutdown_17F04_Epilogue_Low_Shutdown_Lily_007", HearthDialogueSfxActionType.StartLoop, "Epilogue.PathA.Thunder");
        }

        string[] retainSequences = { "17F04_Epilogue_High_Retain", "17F04_Epilogue_Low_Retain" };
        for (int i = 0; i < retainSequences.Length; i++)
        {
            AddDialogueAction(actions, retainSequences[i], "17F04_Epilogue_High_Retain_17F04_Epilogue_Low_Retain_MiasHomeUnit_004", HearthDialogueSfxActionType.StartLoop, "Epilogue.PathB.Gym");
            AddDialogueAction(actions, retainSequences[i], "17F04_Epilogue_High_Retain_17F04_Epilogue_Low_Retain_Lily_008", HearthDialogueSfxActionType.Stop, "Epilogue.PathB.Gym");
            AddDialogueAction(actions, retainSequences[i], "17F04_Epilogue_High_Retain_17F04_Epilogue_Low_Retain_Lily_011", HearthDialogueSfxActionType.PlayOneShot, "Epilogue.PathB.SuitcaseImpact");
            AddDialogueAction(actions, retainSequences[i], "17F04_Epilogue_High_Retain_17F04_Epilogue_Low_Retain_Lily_011", HearthDialogueSfxActionType.StartLoop, "Epilogue.PathB.SuitcaseRolling");
            AddDialogueAction(actions, retainSequences[i], "17F04_Epilogue_High_Retain_17F04_Epilogue_Low_Retain_MiasHomeUnit_008", HearthDialogueSfxActionType.Stop, "Epilogue.PathB.SuitcaseRolling");
        }

        Undo.RecordObject(track, "Bind epilogue line-level SFX");
        track.Configure(subtitlePlayer, cuePlayer, actions.ToArray());
        EditorUtility.SetDirty(track);
    }

    private static void AddDialogueAction(
        List<HearthDialogueSfxTrack.CueAction> actions,
        string sequenceId,
        string lineId,
        HearthDialogueSfxActionType action,
        string cueId)
    {
        actions.Add(new HearthDialogueSfxTrack.CueAction
        {
            sequenceId = sequenceId,
            lineId = lineId,
            action = action,
            cueId = cueId,
        });
    }

    private static AudioSource EnsureAudioSource(GameObject owner, bool spatial)
    {
        AudioSource source = owner.GetComponent<AudioSource>();
        if (source == null)
        {
            source = Undo.AddComponent<AudioSource>(owner);
        }

        source.playOnAwake = false;
        source.spatialBlend = spatial ? 1f : 0f;
        source.minDistance = 1f;
        source.maxDistance = spatial ? 12f : 500f;
        source.dopplerLevel = 0f;
        source.rolloffMode = AudioRolloffMode.Linear;
        EditorUtility.SetDirty(source);
        return source;
    }

    private static HearthAudioChannelSource BindChannel(AudioSource source, HearthAudioChannel channel, float baseVolume)
    {
        if (source == null)
        {
            return null;
        }

        HearthAudioChannelSource binding = source.GetComponent<HearthAudioChannelSource>();
        if (binding == null)
        {
            binding = Undo.AddComponent<HearthAudioChannelSource>(source.gameObject);
        }

        binding.Configure(source, channel, baseVolume);
        EditorUtility.SetDirty(binding);
        return binding;
    }

    private static AudioClip Clip(HearthSfxCatalog catalog, string soundId)
    {
        return catalog != null ? catalog.GetPrimaryClip(soundId) : null;
    }

    private static void SetFloat(UnityEngine.Object owner, string propertyName, float value)
    {
        SerializedObject serialized = new SerializedObject(owner);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null)
        {
            property.floatValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static string ResolveSoundId(string cueId)
    {
        switch (cueId)
        {
            case "UI.InteractSingle": return "UI.Interact";
            case "UI.HoldProgress": return "UI.HoldProgress";
            case "UI.HoldComplete": return "UI.HoldComplete";
            case "UI.Confirm": return "UI.Submit";
            case "Transition.Blackout":
            case "Transition.CameraMove": return "SYS.TerminalCamera";
            case "Terminal.PageSwitch":
            case "Terminal.FocusMove": return "UI.Navigation";
            case "Terminal.Submit": return "UI.Submit";
            case "Lobby.RoomTone":
            case "LivingRoom.RoomTone":
            case "Home.RoomTone": return "AMB.Interior.Main";
            case "Bedroom.RoomTone":
            case "Corridor.RoomTone":
            case "DaughterRoom.RoomTone":
            case "AssignmentTerminal.Hum": return "AMB.Interior.Alt";
            case "Lobby.Walla": return "AMB.Lobby.Walla";
            case "Lily.MessageNotification": return "UI.LilyNotification";
            case "AssignmentTerminal.Confirm": return "UI.Submit";
            case "Elevator.Button": return "UI.Interact";
            case "Elevator.DoorsClose": return "FOLEY.Door";
            case "Elevator.Motor": return "AMB.Elevator.Motor";
            case "Elevator.Arrival": return "AMB.Elevator.Arrival";
            case "Elevator.DoorsOpen": return "AMB.Elevator.DoorsOpen";
            case "Replay.SceneTransition":
            case "Photo.Memory": return "SYS.TerminalCamera";
            case "Interaction.ComfortConfirmed": return "UI.HoldComplete";
            case "Parent.TableFoley":
            case "Dining.TableFoley": return "FOLEY.Table";
            case "Robot.ServoLoop": return "FOLEY.RobotTracked";
            case "Wife.SitOnBed": return "FOLEY.SitBed";
            case "Wife.StandUp":
            case "Mother.StandUp":
            case "Daughter.StandUp": return "FOLEY.SitStand";
            case "Wife.Walk":
            case "Daughter.Walk": return "FOLEY.HumanSteps";
            case "Bedroom.Jazz": return "MUSIC.17F02.Jazz";
            case "Bathroom.Shower": return "AMB.17F02.Shower";
            case "BlackAudio.Fridge": return "AMB.17F02.Fridge";
            case "BlackAudio.Traffic": return "AMB.17F02.Traffic";
            case "System.DataScan": return "SYS.TerminalPower";
            case "Daughter.Keypad": return "UI.Navigation";
            case "System.Glitch": return "SYS.Glitch";
            case "System.PowerOff":
            case "Unit.PowerOff": return "SYS.PowerOff";
            case "Popup.Spawn": return "UI.Popup";
            case "Popup.Dismiss": return "UI.Close";
            case "Popup.WaveEscalate": return "UI.Warning";
            case "Popup.Success": return "UI.Submit";
            case "Epilogue.PathA.Frying": return "FOLEY.Frying";
            case "Epilogue.PathA.Keys": return "FOLEY.Keys";
            case "Epilogue.PathA.Thunder": return "FOLEY.Thunder";
            case "Epilogue.PathB.Gym": return "AMB.17F04.Gym";
            case "Epilogue.PathB.SuitcaseImpact": return "FOLEY.SuitcaseImpact";
            case "Epilogue.PathB.SuitcaseRolling": return "FOLEY.SuitcaseRolling";
            default: return cueId;
        }
    }

    private static float ResolveSegmentStart(string cueId)
    {
        return string.Equals(cueId, "Elevator.DoorsClose", StringComparison.OrdinalIgnoreCase) ? 1.5f : 0f;
    }

    private static float ResolveSegmentDuration(string cueId)
    {
        switch (cueId)
        {
            case "Elevator.DoorsClose": return 1.5f;
            case "Parent.TableFoley":
            case "Dining.TableFoley": return 2.2f;
            case "Wife.SitOnBed": return 2f;
            case "Wife.StandUp":
            case "Mother.StandUp":
            case "Daughter.StandUp": return 2.5f;
            default: return 0f;
        }
    }

    private static bool ShouldDuckWhileDialogue(string cueId)
    {
        return cueId.IndexOf("RoomTone", StringComparison.OrdinalIgnoreCase) >= 0 ||
            cueId.IndexOf("Walla", StringComparison.OrdinalIgnoreCase) >= 0 ||
            cueId.IndexOf("Motor", StringComparison.OrdinalIgnoreCase) >= 0 ||
            cueId.IndexOf("Jazz", StringComparison.OrdinalIgnoreCase) >= 0 ||
            cueId.IndexOf("Shower", StringComparison.OrdinalIgnoreCase) >= 0 ||
            cueId.IndexOf("BlackAudio", StringComparison.OrdinalIgnoreCase) >= 0 ||
            cueId.IndexOf("AssignmentTerminal.Hum", StringComparison.OrdinalIgnoreCase) >= 0 ||
            cueId.IndexOf("Epilogue.PathA.Frying", StringComparison.OrdinalIgnoreCase) >= 0 ||
            cueId.IndexOf("Epilogue.PathA.Thunder", StringComparison.OrdinalIgnoreCase) >= 0 ||
            cueId.IndexOf("Epilogue.PathB.Gym", StringComparison.OrdinalIgnoreCase) >= 0;
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
        if (player.Catalog == null)
        {
            issues.Add(path + " is not bound to the central SFX catalog.");
        }
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
            else if (!player.HasAssignedClip(cueId))
            {
                issues.Add(path + " / " + cueId + " does not resolve an AudioClip through its override or the central catalog.");
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
