#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class HearthEarlyHouseholdValidationTools
{
    private const string MenuRoot = "Tools/Hearth/Replay/";

    [MenuItem(MenuRoot + "Validate 17F01 Minimal Loop Setup")]
    public static void Validate17F01()
    {
        List<string> issues = new List<string>();
        List<string> warnings = new List<string>();
        SharedReferences shared = ValidateShared("17F01", issues, warnings);
        HearthCompanion17F01ReplayController controller = FindSceneComponent<HearthCompanion17F01ReplayController>();
        if (controller == null)
        {
            issues.Add("HearthCompanion17F01ReplayController is missing.");
            Report("17F01", issues, warnings);
            return;
        }

        ValidateControllerSharedBindings(controller, shared, issues);
        RequireReferences(
            controller,
            issues,
            "robotRoot", "robotCamera", "robotMovement", "robotLook", "robotInteraction", "robotRigidbody",
            "childRoomStartAnchor", "livingRoomStartAnchor",
            "livingRoomCameraAnchor",
            "approachBoyInteractable", "boyAnimation", "motherAnimation", "fatherAnimation");
        RequireDialogues(controller, issues, "bedroomPreludeSequence", "soothingSequence", "livingRoomSequence");
        RequireAnimationClip(controller, "boyAnimation", "boySleepAnimationId", issues);
        RequireAnimationClip(controller, "motherAnimation", "motherSittingAnimationId", issues);
        RequireAnimationClip(controller, "fatherAnimation", "fatherSittingAnimationId", issues);
        Validate17F01Interaction(controller, issues);
        ValidateTerminal("17F01", shared, issues);
        Report("17F01", issues, warnings);
    }

    [MenuItem(MenuRoot + "Validate 17F02 Minimal Loop Setup")]
    public static void Validate17F02()
    {
        List<string> issues = new List<string>();
        List<string> warnings = new List<string>();
        SharedReferences shared = ValidateShared("17F02", issues, warnings);
        HearthCompanion17F02ReplayController controller = FindSceneComponent<HearthCompanion17F02ReplayController>();
        if (controller == null)
        {
            issues.Add("HearthCompanion17F02ReplayController is missing.");
            Report("17F02", issues, warnings);
            return;
        }

        ValidateControllerSharedBindings(controller, shared, issues);
        RequireReferences(
            controller,
            issues,
            "robotRoot", "robotCamera", "robotMovement", "robotLook", "robotInteraction", "robotRigidbody",
            "bedroomStartAnchor", "bedroomStartCameraAnchor", "livingRoomTerminalAnchor", "livingRoomTerminalCameraAnchor",
            "bedroomWifeActor", "diningWifeActor", "diningHusbandActor", "terminalHusbandActor",
            "bedroomWifeMoveRoot", "wifeDoorPauseAnchor", "wifeExitOutsideAnchor", "wifeExitDoor",
            "bedroomWifeAnimation", "diningWifeAnimation", "diningHusbandAnimation", "terminalHusbandAnimation");
        RequireReferenceArray(controller, "wifeBeforeDoorPathPoints", 1, issues);
        RequireDialogues(
            controller,
            issues,
            "bedroomWakeSequence", "bedroomConfideSequence", "bedroomComfortSequence", "wifeExitSequence",
            "diningObservationSequence", "logAccessSequence", "forcedShutdownSequence", "blackAudioSequence");
        RequireAnimatorState(controller, "bedroomWifeAnimation", "bedroomWifeIdleAnimationId", issues);
        RequireAnimatorState(controller, "bedroomWifeAnimation", "bedroomWifeTalkingAnimationId", issues);
        RequireAnimatorState(controller, "bedroomWifeAnimation", "bedroomWifeSitToStandAnimationId", issues);
        RequireAnimatorState(controller, "bedroomWifeAnimation", "bedroomWifeWalkLoopAnimationId", issues);
        RequireAnimatorState(controller, "bedroomWifeAnimation", "bedroomWifeOpenDoorAnimationId", issues);
        RequireAnimatorState(controller, "diningWifeAnimation", "diningWifeAnimationId", issues);
        RequireAnimatorState(controller, "diningHusbandAnimation", "diningHusbandAnimationId", issues);
        RequireAnimatorState(controller, "terminalHusbandAnimation", "terminalHusbandAnimationId", issues);
        Validate17F02InteractionGate(controller, issues);
        ValidateTerminal("17F02", shared, issues);
        Report("17F02", issues, warnings);
    }

    private sealed class SharedReferences
    {
        public MinLoopFlowController Flow;
        public ViewSwitchController ViewSwitch;
        public HearthCompanionHudController Hud;
        public MinLoopSubtitlePlayer Subtitle;
        public GameObject Human;
        public GameObject Robot;
        public Camera HumanCamera;
        public Camera RobotCamera;
    }

    private static SharedReferences ValidateShared(string residentId, List<string> issues, List<string> warnings)
    {
        SharedReferences shared = new SharedReferences
        {
            Flow = FindSceneComponent<MinLoopFlowController>(),
            Hud = FindSceneComponent<HearthCompanionHudController>(),
            Subtitle = FindSceneComponent<MinLoopSubtitlePlayer>(),
            Human = FindSceneObjectByPath("Player/Person Controller"),
            Robot = FindSceneObjectByPath("Player/Robot Controller"),
        };

        ViewSwitchController[] viewSwitches = FindSceneComponents<ViewSwitchController>()
            .Where(item => item.enabled && item.gameObject.activeInHierarchy)
            .ToArray();
        if (viewSwitches.Length != 1)
        {
            issues.Add("Expected exactly one enabled ViewSwitchController, found " + viewSwitches.Length + ".");
        }
        else
        {
            shared.ViewSwitch = viewSwitches[0];
        }

        if (shared.Flow == null) issues.Add("MinLoopFlowController is missing.");
        if (FindSceneComponent<TrustStateController>() == null) issues.Add("TrustStateController is missing.");
        if (shared.Hud == null) issues.Add("HearthCompanionHudController is missing.");
        if (shared.Subtitle == null) issues.Add("MinLoopSubtitlePlayer is missing.");
        if (shared.Human == null) issues.Add("Formal Player/Person Controller is missing.");
        if (shared.Robot == null) issues.Add("Formal Player/Robot Controller is missing.");

        if (shared.Human != null)
        {
            shared.HumanCamera = shared.Human.GetComponentInChildren<Camera>(true);
            RequireRigComponents(shared.Human, "human", issues);
        }

        if (shared.Robot != null)
        {
            shared.RobotCamera = shared.Robot.GetComponentInChildren<Camera>(true);
            RequireRigComponents(shared.Robot, "robot", issues);
        }

        Camera[] enabledCameras = FindSceneComponents<Camera>()
            .Where(item => item.enabled && item.gameObject.activeInHierarchy)
            .ToArray();
        AudioListener[] enabledListeners = FindSceneComponents<AudioListener>()
            .Where(item => item.enabled && item.gameObject.activeInHierarchy)
            .ToArray();
        if (enabledCameras.Length != 1)
        {
            issues.Add("Expected exactly one enabled Camera in Edit Mode, found " + enabledCameras.Length + ".");
        }
        if (enabledListeners.Length != 1)
        {
            issues.Add("Expected exactly one enabled AudioListener, found " + enabledListeners.Length + ".");
        }

        if (enabledCameras.Length == 1 && shared.HumanCamera != null && enabledCameras[0] != shared.HumanCamera)
        {
            warnings.Add("The single enabled Camera is not the formal human camera: " + GetHierarchyPath(enabledCameras[0].transform) + ".");
        }

        return shared;
    }

    private static void RequireRigComponents(GameObject rig, string label, List<string> issues)
    {
        if (rig.GetComponentInChildren<Camera>(true) == null) issues.Add("Formal " + label + " rig has no Camera.");
        if (rig.GetComponent<FirstPersonMovement>() == null) issues.Add("Formal " + label + " rig has no FirstPersonMovement.");
        if (rig.GetComponentInChildren<FirstPersonLook>(true) == null) issues.Add("Formal " + label + " rig has no FirstPersonLook.");
        if (rig.GetComponent<PlayerInteraction>() == null) issues.Add("Formal " + label + " rig has no PlayerInteraction.");
        if (rig.GetComponent<Rigidbody>() == null) issues.Add("Formal " + label + " rig has no Rigidbody.");
    }

    private static void ValidateControllerSharedBindings(Object controller, SharedReferences shared, List<string> issues)
    {
        RequireSameReference(controller, "flowController", shared.Flow, issues);
        RequireSameReference(controller, "viewSwitchController", shared.ViewSwitch, issues);
        RequireSameReference(controller, "companionHud", shared.Hud, issues);
        RequireSameReference(controller, "subtitlePlayer", shared.Subtitle, issues);
        RequireSameReference(controller, "robotRoot", shared.Robot != null ? shared.Robot.transform : null, issues);
        RequireSameReference(controller, "robotCamera", shared.RobotCamera, issues);
    }

    private static void Validate17F01Interaction(HearthCompanion17F01ReplayController controller, List<string> issues)
    {
        HearthCompanionReplayInteractable interactable = GetReference<HearthCompanionReplayInteractable>(controller, "approachBoyInteractable");
        if (interactable == null)
        {
            return;
        }

        SerializedObject serialized = new SerializedObject(interactable);
        Collider collider = GetReference<Collider>(serialized, "interactionCollider");
        if (collider == null)
        {
            collider = interactable.GetComponent<Collider>();
        }
        if (collider == null) issues.Add("17F01 boy interaction target has no Collider.");
        if (GetReference<Transform>(serialized, "focusTarget") == null) issues.Add("17F01 interaction Focus Target is missing.");
        if (GetReference<Transform>(serialized, "raycastTargetRoot") == null) issues.Add("17F01 interaction Raycast Target Root is missing.");
        if (GetReference<HearthCompanion17F01ReplayController>(serialized, "replayController") != controller)
        {
            issues.Add("17F01 interaction is not bound back to its replay controller.");
        }

        SerializedProperty centerRay = serialized.FindProperty("requireCenterRayHit");
        if (centerRay == null || !centerRay.boolValue) issues.Add("17F01 interaction must require a center-ray hit.");
        SerializedProperty distance = serialized.FindProperty("maxDistance");
        if (distance == null || distance.floatValue <= 0.1f) issues.Add("17F01 interaction ray distance is invalid.");
    }

    private static void Validate17F02InteractionGate(HearthCompanion17F02ReplayController controller, List<string> issues)
    {
        SerializedObject serialized = new SerializedObject(controller);
        SerializedProperty wait = serialized.FindProperty("waitForBedroomAcknowledgement");
        if (wait == null || !wait.boolValue)
        {
            issues.Add("17F02 must wait for the bedroom acknowledgement hold interaction.");
        }

        SerializedProperty delay = serialized.FindProperty("bedroomPromptDelayAfterConfideSeconds");
        if (delay == null || delay.floatValue < 0f)
        {
            issues.Add("17F02 bedroom prompt delay is invalid.");
        }
    }

    private static void ValidateTerminal(string residentId, SharedReferences shared, List<string> issues)
    {
        HearthTvTerminalController terminal = FindSceneComponents<HearthTvTerminalController>()
            .FirstOrDefault(item => string.Equals(item.GetReplayResidentId(), residentId, StringComparison.OrdinalIgnoreCase));
        if (terminal == null)
        {
            issues.Add("No TV terminal is explicitly bound to " + residentId + ".");
            return;
        }

        SerializedObject serialized = new SerializedObject(terminal);
        RequireSameReference(serialized, "minLoopFlowController", shared.Flow, terminal.name, issues);
        RequireSameReference(serialized, "viewSwitchController", shared.ViewSwitch, terminal.name, issues);
        RequireSameReference(serialized, "playerCamera", shared.HumanCamera, terminal.name, issues);
        RequireReference(serialized, "terminalCamera", terminal.name, issues);
        RequireReference(serialized, "playerInteraction", terminal.name, issues);
        RequireReference(serialized, "cameraTransition", terminal.name, issues);
        RequireReference(serialized, "bootSequence", terminal.name, issues);

        SerializedProperty pages = serialized.FindProperty("pages");
        if (pages == null || pages.arraySize == 0)
        {
            issues.Add(terminal.name + " has no terminal pages.");
        }

        if (terminal.PrimaryAction != HearthTerminalPrimaryAction.RequestReplay)
        {
            issues.Add(terminal.name + " Primary Action must be RequestReplay for " + residentId + ".");
        }
    }

    private static void RequireDialogues(Object owner, List<string> issues, params string[] fields)
    {
        SerializedObject serialized = new SerializedObject(owner);
        for (int i = 0; i < fields.Length; i++)
        {
            HearthDialogueSequence sequence = GetReference<HearthDialogueSequence>(serialized, fields[i]);
            if (sequence == null)
            {
                issues.Add(owner.name + " / " + fields[i] + " is missing.");
            }
            else if (!sequence.HasLines)
            {
                issues.Add(owner.name + " / " + fields[i] + " has no dialogue lines.");
            }
        }
    }

    private static void RequireAnimationClip(Object owner, string playerField, string idField, List<string> issues)
    {
        SerializedObject serialized = new SerializedObject(owner);
        HearthActorAnimationPlayer player = GetReference<HearthActorAnimationPlayer>(serialized, playerField);
        string clipId = GetString(serialized, idField);
        if (player != null && (string.IsNullOrEmpty(clipId) || player.GetClipLength(clipId) <= 0f))
        {
            issues.Add(owner.name + " / " + playerField + " cannot play clip ID '" + clipId + "'.");
        }
    }

    private static void RequireAnimatorState(Object owner, string driverField, string idField, List<string> issues)
    {
        SerializedObject serialized = new SerializedObject(owner);
        HearthActorAnimatorDriver driver = GetReference<HearthActorAnimatorDriver>(serialized, driverField);
        string stateId = GetString(serialized, idField);
        if (driver != null && (string.IsNullOrEmpty(stateId) || !driver.HasState(stateId)))
        {
            issues.Add(owner.name + " / " + driverField + " cannot play state ID '" + stateId + "'.");
        }
    }

    private static void RequireReferences(Object owner, List<string> issues, params string[] fields)
    {
        SerializedObject serialized = new SerializedObject(owner);
        for (int i = 0; i < fields.Length; i++)
        {
            RequireReference(serialized, fields[i], owner.name, issues);
        }
    }

    private static void RequireReference(SerializedObject serialized, string field, string ownerName, List<string> issues)
    {
        SerializedProperty property = serialized.FindProperty(field);
        if (property == null || property.propertyType != SerializedPropertyType.ObjectReference || property.objectReferenceValue == null)
        {
            issues.Add(ownerName + " / " + field + " is missing.");
        }
    }

    private static void RequireReferenceArray(Object owner, string field, int minimum, List<string> issues)
    {
        SerializedObject serialized = new SerializedObject(owner);
        SerializedProperty property = serialized.FindProperty(field);
        if (property == null || !property.isArray || property.arraySize < minimum)
        {
            issues.Add(owner.name + " / " + field + " needs at least " + minimum + " item(s).");
            return;
        }

        for (int i = 0; i < property.arraySize; i++)
        {
            if (property.GetArrayElementAtIndex(i).objectReferenceValue == null)
            {
                issues.Add(owner.name + " / " + field + " has a missing item at index " + i + ".");
            }
        }
    }

    private static void RequireSameReference(Object owner, string field, Object expected, List<string> issues)
    {
        SerializedObject serialized = new SerializedObject(owner);
        RequireSameReference(serialized, field, expected, owner.name, issues);
    }

    private static void RequireSameReference(SerializedObject serialized, string field, Object expected, string ownerName, List<string> issues)
    {
        SerializedProperty property = serialized.FindProperty(field);
        if (property == null || property.objectReferenceValue != expected)
        {
            issues.Add(ownerName + " / " + field + " is not bound to " + (expected != null ? expected.name : "<missing expected object>") + ".");
        }
    }

    private static T GetReference<T>(Object owner, string field) where T : Object
    {
        return owner != null ? GetReference<T>(new SerializedObject(owner), field) : null;
    }

    private static T GetReference<T>(SerializedObject serialized, string field) where T : Object
    {
        SerializedProperty property = serialized.FindProperty(field);
        return property != null ? property.objectReferenceValue as T : null;
    }

    private static string GetString(SerializedObject serialized, string field)
    {
        SerializedProperty property = serialized.FindProperty(field);
        return property != null ? property.stringValue : string.Empty;
    }

    private static T FindSceneComponent<T>() where T : Component
    {
        return FindSceneComponents<T>().FirstOrDefault();
    }

    private static T[] FindSceneComponents<T>() where T : Component
    {
        Scene scene = SceneManager.GetActiveScene();
        return Object.FindObjectsOfType<T>(true)
            .Where(item => item != null && item.gameObject.scene == scene)
            .ToArray();
    }

    private static GameObject FindSceneObjectByPath(string path)
    {
        string[] parts = path.Split('/');
        GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
        Transform current = roots.Select(item => item.transform).FirstOrDefault(item => item.name == parts[0]);
        for (int i = 1; i < parts.Length && current != null; i++)
        {
            current = current.Find(parts[i]);
        }

        return current != null ? current.gameObject : null;
    }

    private static string GetHierarchyPath(Transform transform)
    {
        string path = transform.name;
        while (transform.parent != null)
        {
            transform = transform.parent;
            path = transform.name + "/" + path;
        }

        return path;
    }

    private static void Report(string residentId, List<string> issues, List<string> warnings)
    {
        if (issues.Count == 0 && warnings.Count == 0)
        {
            Debug.Log(
                "[HearthEarlyHouseholdValidationTools] " + residentId +
                " validation passed: interaction, cameras, listener, formal controllers, dialogue assets, flow and terminal bindings are ready.");
            return;
        }

        string report = "[HearthEarlyHouseholdValidationTools] " + residentId + " validation";
        if (issues.Count > 0)
        {
            report += " found " + issues.Count + " issue(s):\n- " + string.Join("\n- ", issues.ToArray());
        }
        if (warnings.Count > 0)
        {
            report += "\nWarnings:\n- " + string.Join("\n- ", warnings.ToArray());
        }

        if (issues.Count > 0) Debug.LogError(report);
        else Debug.LogWarning(report);
    }
}
#endif
