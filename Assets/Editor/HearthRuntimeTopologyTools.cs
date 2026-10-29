#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public static class HearthRuntimeTopologyTools
{
    private const string CanonicalViewSwitchPath =
        "Gameplay/FlowManagers/ViewSwitchController";

    [MenuItem("Tools/Hearth/Validation/Validate Runtime Topology")]
    public static void ValidateRuntimeTopologyMenu()
    {
        ValidateOpenSceneP0Topology(true);
    }

    public static bool ValidateOpenSceneP0Topology(bool logSuccess)
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("[HearthRuntimeTopologyTools] No loaded active scene.");
            return false;
        }

        int issues = 0;
        ViewSwitchController[] switches = FindSceneComponents<ViewSwitchController>(scene);
        ViewSwitchController canonical = FindCanonicalViewSwitch(scene);
        if (switches.Length != 1 ||
            canonical == null ||
            switches[0] != canonical ||
            !canonical.enabled ||
            !canonical.gameObject.activeInHierarchy)
        {
            Debug.LogWarning(
                "[HearthRuntimeTopologyTools] Expected exactly one canonical ViewSwitchController; found " +
                switches.Length + ".");
            issues++;
        }

        HearthLocationProbe[] probes = FindSceneComponents<HearthLocationProbe>(scene);
        for (int i = 0; i < probes.Length; i++)
        {
            SerializedObject serialized = new SerializedObject(probes[i]);
            SerializedProperty viewSwitch =
                serialized.FindProperty("viewSwitchController");
            if (viewSwitch == null || viewSwitch.objectReferenceValue != canonical)
            {
                Debug.LogWarning(
                    "[HearthRuntimeTopologyTools] Location probe does not reference the canonical ViewSwitch: " +
                    GetPath(probes[i].transform),
                    probes[i]);
                issues++;
            }
        }

        HearthPlayerControlLock[] controlLocks =
            FindSceneComponents<HearthPlayerControlLock>(scene);
        HearthPlayerControlLock controlLock = FindPreferredControlLock(scene);
        if (controlLocks.Length != 1 ||
            controlLock == null ||
            !controlLock.enabled ||
            !controlLock.gameObject.activeInHierarchy)
        {
            Debug.LogWarning(
                "[HearthRuntimeTopologyTools] Expected exactly one active scene HearthPlayerControlLock; found " +
                controlLocks.Length + ".");
            issues++;
        }

        HearthFirstPersonHudController[] humanHuds =
            FindSceneComponents<HearthFirstPersonHudController>(scene);
        if (humanHuds.Length != 1)
        {
            Debug.LogWarning(
                "[HearthRuntimeTopologyTools] Expected exactly one Human HUD; found " +
                humanHuds.Length + ".");
            issues++;
        }
        else
        {
            issues += ValidateHumanPageReferences(humanHuds[0]);
            SerializedObject humanSerialized =
                new SerializedObject(humanHuds[0]);
            SerializedProperty humanControlLock =
                humanSerialized.FindProperty("playerControlLock");
            if (humanControlLock == null ||
                humanControlLock.objectReferenceValue != controlLock)
            {
                Debug.LogWarning(
                    "[HearthRuntimeTopologyTools] Human HUD does not reference the unique player control lock.",
                    humanHuds[0]);
                issues++;
            }
        }

        HearthTvTerminalController[] terminals = FindSceneComponents<HearthTvTerminalController>(scene);
        if (terminals.Length != 5)
        {
            Debug.LogWarning(
                "[HearthRuntimeTopologyTools] Expected exactly five terminals; found " +
                terminals.Length + ".");
            issues++;
        }

        HashSet<Camera> ownedTerminalCameras = new HashSet<Camera>();
        for (int i = 0; i < terminals.Length; i++)
        {
            issues += ValidateTerminalPageReferences(terminals[i]);

            TerminalCameraBinding expected;
            SerializedObject serialized = new SerializedObject(terminals[i]);
            SerializedProperty terminalCamera = serialized.FindProperty("terminalCamera");
            SerializedProperty worldCamera = serialized.FindProperty("worldCamera");
            SerializedProperty terminalHardwareRoot =
                serialized.FindProperty("terminalHardwareRoot");
            SerializedProperty terminalViewSwitch =
                serialized.FindProperty("viewSwitchController");
            SerializedProperty terminalControlLock =
                serialized.FindProperty("playerControlLock");
            SerializedProperty switchCamera =
                serialized.FindProperty("switchCameraWhileOpen");
            Canvas terminalCanvas = terminals[i].GetComponent<Canvas>();
            if (terminalCanvas == null)
            {
                terminalCanvas = terminals[i].GetComponentInParent<Canvas>();
            }

            bool bindingResolved = TryResolveTerminalCamera(terminals[i], out expected);
            if (!bindingResolved ||
                terminalCamera == null ||
                terminalCamera.objectReferenceValue != expected.Camera ||
                worldCamera == null ||
                worldCamera.objectReferenceValue != expected.Camera ||
                terminalCanvas == null ||
                terminalCanvas.worldCamera != expected.Camera ||
                !ownedTerminalCameras.Add(expected.Camera) ||
                terminalHardwareRoot == null ||
                terminalHardwareRoot.objectReferenceValue != expected.HardwareRoot ||
                terminalViewSwitch == null ||
                terminalViewSwitch.objectReferenceValue != canonical ||
                terminalControlLock == null ||
                terminalControlLock.objectReferenceValue != controlLock ||
                switchCamera == null ||
                !switchCamera.boolValue)
            {
                Debug.LogWarning(
                    "[HearthRuntimeTopologyTools] Invalid terminal topology: " +
                    GetPath(terminals[i].transform),
                    terminals[i]);
                issues++;
            }
        }

        issues += ValidateTerminalFlowCallbacks(scene, terminals);
        issues += ValidateLobbyActiveLoopCue(scene, terminals);

        HearthTerminalCameraTransition[] transitions =
            FindSceneComponents<HearthTerminalCameraTransition>(scene);
        for (int i = 0; i < transitions.Length; i++)
        {
            SerializedObject serialized = new SerializedObject(transitions[i]);
            SerializedProperty copyListener =
                serialized.FindProperty("copyAudioListenerIfMissing");
            if (copyListener == null || !copyListener.boolValue)
            {
                Debug.LogWarning(
                    "[HearthRuntimeTopologyTools] Transition camera audio continuity is disabled: " +
                    GetPath(transitions[i].transform),
                    transitions[i]);
                issues++;
            }
        }

        int enabledCameraCount = CountEnabledComponents(
            FindSceneComponents<Camera>(scene));
        int enabledListenerCount = CountEnabledComponents(
            FindSceneComponents<AudioListener>(scene));
        if (enabledCameraCount != 1 || enabledListenerCount != 1)
        {
            Debug.LogWarning(
                "[HearthRuntimeTopologyTools] Expected exactly one enabled Camera and AudioListener; found " +
                enabledCameraCount + " and " + enabledListenerCount + ".");
            issues++;
        }

        if (logSuccess && issues == 0)
        {
            Debug.Log(
                "[HearthRuntimeTopologyTools] Runtime topology validation passed.");
        }
        else if (issues > 0)
        {
            Debug.LogWarning(
                "[HearthRuntimeTopologyTools] Runtime topology validation found " +
                issues + " issue(s).");
        }

        return issues == 0;
    }

    private static int ValidateHumanPageReferences(
        HearthFirstPersonHudController controller)
    {
        HearthFirstPersonHudPage[] discovered =
            controller.GetComponentsInChildren<HearthFirstPersonHudPage>(true);
        SerializedObject serialized = new SerializedObject(controller);
        SerializedProperty pages = serialized.FindProperty("pages");
        HashSet<UnityEngine.Object> references = new HashSet<UnityEngine.Object>();
        HashSet<int> pageIds = new HashSet<int>();
        bool valid =
            pages != null &&
            pages.isArray &&
            discovered.Length > 0 &&
            pages.arraySize == discovered.Length;

        for (int i = 0; valid && i < pages.arraySize; i++)
        {
            HearthFirstPersonHudPage page =
                pages.GetArrayElementAtIndex(i).objectReferenceValue
                as HearthFirstPersonHudPage;
            int pageId = page != null ? (int)page.PageId : 0;
            valid =
                page != null &&
                page.transform.IsChildOf(controller.transform) &&
                references.Add(page) &&
                pageId != 0 &&
                pageIds.Add(pageId);
        }

        if (valid)
        {
            return 0;
        }

        Debug.LogWarning(
            "[HearthRuntimeTopologyTools] Human HUD page references are incomplete, null, duplicated, or outside the HUD root.",
            controller);
        return 1;
    }

    private static int ValidateTerminalPageReferences(
        HearthTvTerminalController controller)
    {
        HearthHudPage[] discovered =
            controller.GetComponentsInChildren<HearthHudPage>(true);
        SerializedObject serialized = new SerializedObject(controller);
        SerializedProperty pages = serialized.FindProperty("pages");
        HashSet<UnityEngine.Object> references = new HashSet<UnityEngine.Object>();
        HashSet<int> pageIds = new HashSet<int>();
        bool valid =
            pages != null &&
            pages.isArray &&
            discovered.Length > 0 &&
            pages.arraySize == discovered.Length;

        for (int i = 0; valid && i < pages.arraySize; i++)
        {
            HearthHudPage page =
                pages.GetArrayElementAtIndex(i).objectReferenceValue
                as HearthHudPage;
            int pageId = page != null ? (int)page.PageId : 0;
            valid =
                page != null &&
                page.transform.IsChildOf(controller.transform) &&
                references.Add(page) &&
                pageId != 0 &&
                pageIds.Add(pageId);
        }

        if (valid)
        {
            return 0;
        }

        Debug.LogWarning(
            "[HearthRuntimeTopologyTools] Terminal page references are incomplete, null, duplicated, or outside the terminal root: " +
            GetPath(controller.transform),
            controller);
        return 1;
    }

    private static int ValidateTerminalFlowCallbacks(
        Scene scene,
        HearthTvTerminalController[] terminals)
    {
        int issues = 0;
        HearthLobbyFlowController[] lobbyFlows =
            FindSceneComponents<HearthLobbyFlowController>(scene);
        Hearth17F04FinaleController[] finaleControllers =
            FindSceneComponents<Hearth17F04FinaleController>(scene);
        HearthTvTerminalController lobbyTerminal =
            FindUniqueTerminal(terminals, "LOBBY", "ASSIGNMENT");
        HearthTvTerminalController homeTerminal =
            FindUniqueTerminal(terminals, "17F04", "HOME");

        if (lobbyFlows.Length != 1 ||
            finaleControllers.Length != 1 ||
            lobbyTerminal == null ||
            homeTerminal == null)
        {
            Debug.LogWarning(
                "[HearthRuntimeTopologyTools] Terminal story callback topology is ambiguous.");
            return 1;
        }

        issues += ValidatePersistentListener(
            lobbyTerminal.OnOpened,
            lobbyFlows[0],
            "BeginAssignmentBriefingFromTerminal",
            lobbyTerminal,
            "Lobby onOpened");
        issues += ValidatePersistentListener(
            lobbyTerminal.OnCustomPrimaryAction,
            lobbyFlows[0],
            "ConfirmAssignmentTerminalClose",
            lobbyTerminal,
            "Lobby primary action");
        issues += ValidatePersistentListener(
            homeTerminal.OnCustomPrimaryAction,
            finaleControllers[0],
            "BeginFromHomeTerminal",
            homeTerminal,
            "17F04 primary action");
        return issues;
    }

    private static int ValidateLobbyActiveLoopCue(
        Scene scene,
        HearthTvTerminalController[] terminals)
    {
        HearthTvTerminalController lobbyTerminal =
            FindUniqueTerminal(terminals, "LOBBY", "ASSIGNMENT");
        HearthSfxCuePlayer lobbyCuePlayer =
            FindUniqueNamedComponent<HearthSfxCuePlayer>(
                scene,
                "StorySFX_Lobby");
        if (lobbyTerminal == null || lobbyCuePlayer == null)
        {
            Debug.LogWarning(
                "[HearthRuntimeTopologyTools] Lobby terminal active-loop cue topology is ambiguous.");
            return 1;
        }

        SerializedObject serialized = new SerializedObject(lobbyTerminal);
        SerializedProperty cuePlayer =
            serialized.FindProperty("activeLoopCuePlayer");
        SerializedProperty cueId =
            serialized.FindProperty("activeLoopCueId");
        bool valid =
            cuePlayer != null &&
            cuePlayer.objectReferenceValue == lobbyCuePlayer &&
            cueId != null &&
            cueId.stringValue == "AssignmentTerminal.Hum" &&
            lobbyCuePlayer.HasCue("AssignmentTerminal.Hum");
        if (valid)
        {
            return 0;
        }

        Debug.LogWarning(
            "[HearthRuntimeTopologyTools] Lobby terminal must use StorySFX_Lobby / AssignmentTerminal.Hum for its active loop.",
            lobbyTerminal);
        return 1;
    }

    private static int ValidatePersistentListener(
        UnityEvent targetEvent,
        UnityEngine.Object expectedTarget,
        string expectedMethod,
        UnityEngine.Object context,
        string label)
    {
        bool valid =
            targetEvent != null &&
            targetEvent.GetPersistentEventCount() == 1 &&
            targetEvent.GetPersistentTarget(0) == expectedTarget &&
            targetEvent.GetPersistentMethodName(0) == expectedMethod &&
            targetEvent.GetPersistentListenerState(0) != UnityEventCallState.Off;
        if (valid)
        {
            return 0;
        }

        Debug.LogWarning(
            "[HearthRuntimeTopologyTools] Missing or invalid persistent callback: " +
            label + " -> " + expectedMethod + ".",
            context);
        return 1;
    }

    private static ViewSwitchController FindCanonicalViewSwitch(Scene scene)
    {
        ViewSwitchController[] switches = FindSceneComponents<ViewSwitchController>(scene);
        for (int i = 0; i < switches.Length; i++)
        {
            if (GetPath(switches[i].transform) == CanonicalViewSwitchPath)
            {
                return switches[i];
            }
        }

        return null;
    }

    private static HearthPlayerControlLock FindPreferredControlLock(Scene scene)
    {
        HearthPlayerControlLock[] locks = FindSceneComponents<HearthPlayerControlLock>(scene);
        HearthPlayerControlLock fallback = null;
        for (int i = 0; i < locks.Length; i++)
        {
            if (fallback == null)
            {
                fallback = locks[i];
            }

            if (locks[i].enabled && locks[i].gameObject.activeInHierarchy)
            {
                return locks[i];
            }
        }

        return fallback;
    }

    private static T FindUniqueNamedComponent<T>(
        Scene scene,
        string objectName)
        where T : Component
    {
        T match = null;
        T[] candidates = FindSceneComponents<T>(scene);
        for (int i = 0; i < candidates.Length; i++)
        {
            if (!string.Equals(
                    candidates[i].gameObject.name,
                    objectName,
                    StringComparison.Ordinal))
            {
                continue;
            }

            if (match != null)
            {
                return null;
            }

            match = candidates[i];
        }

        return match;
    }

    private static HearthTvTerminalController FindUniqueTerminal(
        HearthTvTerminalController[] terminals,
        params string[] identityTokens)
    {
        HearthTvTerminalController match = null;
        for (int i = 0; terminals != null && i < terminals.Length; i++)
        {
            string identity =
                (GetPath(terminals[i].transform) + "|" + terminals[i].GetReplayResidentId())
                .ToUpperInvariant();
            bool matches = false;
            for (int tokenIndex = 0;
                 identityTokens != null && tokenIndex < identityTokens.Length;
                 tokenIndex++)
            {
                if (identity.Contains(identityTokens[tokenIndex].ToUpperInvariant()))
                {
                    matches = true;
                    break;
                }
            }

            if (!matches)
            {
                continue;
            }

            if (match != null)
            {
                return null;
            }

            match = terminals[i];
        }

        return match;
    }

    private static bool TryResolveTerminalCamera(
        HearthTvTerminalController terminal,
        out TerminalCameraBinding binding)
    {
        binding = default(TerminalCameraBinding);
        if (terminal == null)
        {
            return false;
        }

        Transform uiRoot = terminal.transform;
        Transform cursor = uiRoot.parent;
        TerminalCameraBinding fallback = default(TerminalCameraBinding);
        bool hasFallback = false;

        for (int depth = 0; cursor != null && depth < 8; depth++)
        {
            List<Camera> cameras = FindOwnedCameras(cursor, uiRoot);
            if (cameras.Count == 1 && !hasFallback)
            {
                fallback = new TerminalCameraBinding(cursor, cameras[0]);
                hasFallback = true;
            }

            if (cameras.Count == 1 &&
                (cursor.name.StartsWith("TV", StringComparison.OrdinalIgnoreCase) ||
                 cursor.name.IndexOf("terminal", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 cursor.name.IndexOf("monitor", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                binding = new TerminalCameraBinding(cursor, cameras[0]);
                return true;
            }

            cursor = cursor.parent;
        }

        if (hasFallback)
        {
            binding = fallback;
            return true;
        }

        return false;
    }

    private static List<Camera> FindOwnedCameras(Transform hardwareRoot, Transform uiRoot)
    {
        List<Camera> result = new List<Camera>();
        Camera[] cameras = hardwareRoot.GetComponentsInChildren<Camera>(true);
        for (int i = 0; i < cameras.Length; i++)
        {
            Camera camera = cameras[i];
            if (camera == null ||
                camera.transform.IsChildOf(uiRoot) ||
                camera.name.IndexOf("Transition", StringComparison.OrdinalIgnoreCase) >= 0 ||
                camera.GetComponentInParent<FirstPersonMovement>() != null ||
                camera.GetComponentInParent<PlayerInteraction>() != null)
            {
                continue;
            }

            string path = GetPath(camera.transform);
            if (path.IndexOf("First Person Camera", StringComparison.OrdinalIgnoreCase) >= 0 ||
                path.IndexOf("Robot First Person Camera", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                continue;
            }

            result.Add(camera);
        }

        return result;
    }

    private static T[] FindSceneComponents<T>(Scene scene) where T : Component
    {
        List<T> result = new List<T>();
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            result.AddRange(roots[i].GetComponentsInChildren<T>(true));
        }

        return result.ToArray();
    }

    private static int CountEnabledComponents<T>(T[] components)
        where T : Behaviour
    {
        int count = 0;
        for (int i = 0; components != null && i < components.Length; i++)
        {
            if (components[i] != null &&
                components[i].enabled &&
                components[i].gameObject.activeInHierarchy)
            {
                count++;
            }
        }

        return count;
    }

    private static string GetPath(Transform target)
    {
        if (target == null)
        {
            return string.Empty;
        }

        string path = target.name;
        Transform cursor = target.parent;
        while (cursor != null)
        {
            path = cursor.name + "/" + path;
            cursor = cursor.parent;
        }

        return path;
    }

    private struct TerminalCameraBinding
    {
        public readonly Transform HardwareRoot;
        public readonly Camera Camera;

        public TerminalCameraBinding(Transform hardwareRoot, Camera camera)
        {
            HardwareRoot = hardwareRoot;
            Camera = camera;
        }
    }
}
#endif
