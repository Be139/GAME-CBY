#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class HearthStorySfxValidator
{
    private const string MenuRoot = "Tools/Hearth/Validation/";
    private const string AudioRootPath = "Gameplay/Audio";
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

    [MenuItem(MenuRoot + "Validate Production Story SFX Setup")]
    public static void ValidateProductionSetup()
    {
        List<string> issues = new List<string>();
        int cueCount = 0;
        int assignedClipCount = 0;
        int reservedCount = 0;

        ValidatePlayer("Gameplay/Audio/StorySFX_Global", 9, issues, ref cueCount, ref assignedClipCount, ref reservedCount);
        HearthSfxCatalog catalog = AssetDatabase.LoadAssetAtPath<HearthSfxCatalog>(CatalogPath);
        if (catalog == null || catalog.EntryCount < GetCatalogSpecs().Length || catalog.AssignedClipCount < GetCatalogSpecs().Length)
        {
            issues.Add("Central SFX catalog is missing entries or clips at " + CatalogPath + ".");
        }

        ValidatePlayer("Gameplay/Audio/StorySFX_Lobby", 11, issues, ref cueCount, ref assignedClipCount, ref reservedCount);
        ValidatePlayer("Gameplay/Audio/StorySFX_17F01", 6, issues, ref cueCount, ref assignedClipCount, ref reservedCount);
        ValidatePlayer("Gameplay/Audio/StorySFX_17F02", 11, issues, ref cueCount, ref assignedClipCount, ref reservedCount);
        ValidatePlayer("Gameplay/Audio/StorySFX_17F03", 6, issues, ref cueCount, ref assignedClipCount, ref reservedCount);
        ValidatePlayer("Gameplay/Audio/StorySFX_17F04", 14, issues, ref cueCount, ref assignedClipCount, ref reservedCount);

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

        Transform catGuide = FindTransform("Gameplay/Finale_17F04/CatGuide");
        if (catGuide != null && catGuide.GetComponentInChildren<HearthSfxCuePlayer>(true) != null)
        {
            issues.Add("CatGuide contains a HearthSfxCuePlayer even though cat sounds are intentionally excluded.");
        }

        if (issues.Count == 0)
        {
            Debug.Log(
                "[HearthStorySfxValidator] Validation passed: " + cueCount +
                " story SFX slots, " + assignedClipCount +
                " resolved clips, " + reservedCount +
                " reserved timing slot(s), and a complete shared catalog.");
        }
        else
        {
            Debug.LogWarning(
                "[HearthStorySfxValidator] Validation found " + issues.Count + " issue(s): " +
                string.Join(" | ", issues.ToArray()));
        }
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
