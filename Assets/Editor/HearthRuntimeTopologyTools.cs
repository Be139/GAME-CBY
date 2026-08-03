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
        "MIN_LOOP_ROOT/FlowManagers/ViewSwitchController";

    [MenuItem("Tools/Hearth/Validation/Validate Runtime Topology")]
    public static void ValidateRuntimeTopologyMenu()
    {
        ValidateOpenSceneP0Topology(true);
    }

    [MenuItem("Tools/Hearth/Legacy Unsafe/Repair P0 Runtime Topology")]
    public static void RepairRuntimeTopologyMenu()
    {
        if (!HearthLegacyToolGuard.Confirm(
                "Repair P0 Runtime Topology",
                "ViewSwitch, Camera, Control Lock, terminal and story-flow references in the active scene"))
        {
            return;
        }

        RepairOpenSceneP0Topology();
    }

    public static bool RepairOpenSceneP0Topology()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("[HearthRuntimeTopologyTools] No loaded active scene.");
            return false;
        }

        ViewSwitchController canonical = FindCanonicalViewSwitch(scene);
        HearthFirstPersonHudController[] humanHuds =
            FindSceneComponents<HearthFirstPersonHudController>(scene);
        HearthTvTerminalController[] terminals = FindSceneComponents<HearthTvTerminalController>(scene);
        HearthPlayerControlLock[] controlLocks =
            FindSceneComponents<HearthPlayerControlLock>(scene);
        HearthPlayerControlLock controlLock = FindPreferredControlLock(scene);
        HearthSfxCuePlayer lobbyCuePlayer =
            FindUniqueNamedComponent<HearthSfxCuePlayer>(
                scene,
                "StorySFX_Lobby");
        HearthLobbyFlowController[] lobbyFlows =
            FindSceneComponents<HearthLobbyFlowController>(scene);
        Hearth17F04FinaleController[] finaleControllers =
            FindSceneComponents<Hearth17F04FinaleController>(scene);
        HearthTvTerminalController lobbyTerminal =
            FindUniqueTerminal(terminals, "LOBBY", "ASSIGNMENT");
        HearthTvTerminalController homeTerminal =
            FindUniqueTerminal(terminals, "17F04", "HOME");
        if (canonical == null ||
            humanHuds.Length != 1 ||
            terminals.Length != 5 ||
            controlLocks.Length != 1 ||
            controlLock == null ||
            !controlLock.enabled ||
            !controlLock.gameObject.activeInHierarchy ||
            lobbyCuePlayer == null ||
            !lobbyCuePlayer.HasCue("AssignmentTerminal.Hum") ||
            lobbyFlows.Length != 1 ||
            finaleControllers.Length != 1 ||
            lobbyTerminal == null ||
            homeTerminal == null)
        {
            Debug.LogError(
                "[HearthRuntimeTopologyTools] Repair aborted before mutation. Required: canonical ViewSwitch, one Human HUD, five unique terminals, one active player control lock, Lobby SFX cue player, one Lobby flow, and one 17F04 finale controller.");
            return false;
        }

        Dictionary<HearthTvTerminalController, TerminalCameraBinding> terminalBindings =
            new Dictionary<HearthTvTerminalController, TerminalCameraBinding>();
        for (int i = 0; i < terminals.Length; i++)
        {
            TerminalCameraBinding binding;
            if (!TryResolveTerminalCamera(terminals[i], out binding))
            {
                Debug.LogError(
                    "[HearthRuntimeTopologyTools] Repair aborted before mutation. Terminal camera ownership is ambiguous: " +
                    GetPath(terminals[i].transform),
                    terminals[i]);
                return false;
            }

            terminalBindings.Add(terminals[i], binding);
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Repair HEARTH P0 Runtime Topology");

        ViewSwitchController[] switches = FindSceneComponents<ViewSwitchController>(scene);
        HashSet<UnityEngine.Object> duplicates = new HashSet<UnityEngine.Object>();
        for (int i = 0; i < switches.Length; i++)
        {
            if (switches[i] != canonical)
            {
                duplicates.Add(switches[i]);
            }
        }

        RemapSceneObjectReferences(scene, duplicates, canonical);

        foreach (UnityEngine.Object duplicate in duplicates)
        {
            if (duplicate != null)
            {
                Undo.DestroyObjectImmediate(duplicate);
            }
        }

        foreach (KeyValuePair<HearthTvTerminalController, TerminalCameraBinding> pair in terminalBindings)
        {
            RepairTerminal(
                pair.Key,
                pair.Value,
                canonical,
                controlLock);
        }

        RevertPageListToPrefabDefault(humanHuds[0]);
        Undo.RecordObject(humanHuds[0], "Repair Human HUD control lock");
        SerializedObject humanSerialized =
            new SerializedObject(humanHuds[0]);
        SetObjectReference(
            humanSerialized,
            "playerControlLock",
            controlLock);
        humanSerialized.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.RecordPrefabInstancePropertyModifications(
            humanHuds[0]);
        EditorUtility.SetDirty(humanHuds[0]);
        for (int i = 0; i < terminals.Length; i++)
        {
            RevertPageListToPrefabDefault(terminals[i]);
        }

        RepairTransitionAudioContinuity(scene);
        RepairTerminalFlowCallbacks(
            lobbyTerminal,
            homeTerminal,
            lobbyFlows[0],
            finaleControllers[0]);
        RepairLobbyActiveLoopCue(lobbyTerminal, lobbyCuePlayer);
        EditorSceneManager.MarkSceneDirty(scene);

        if (!ValidateOpenSceneP0Topology(true))
        {
            Undo.FlushUndoRecordObjects();
            Undo.RevertAllDownToGroup(undoGroup);
            Debug.LogError(
                "[HearthRuntimeTopologyTools] Repair failed validation and was rolled back.");
            return false;
        }

        Undo.FlushUndoRecordObjects();
        Undo.CollapseUndoOperations(undoGroup);
        Debug.Log(
            "[HearthRuntimeTopologyTools] P0 topology repaired: one canonical ViewSwitch, " +
            "five owned terminal cameras, shared owner-based control lock. The scene was " +
            "not saved automatically; review it and save manually.");
        return true;
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

    private static void RevertPageListToPrefabDefault(MonoBehaviour controller)
    {
        if (controller == null)
        {
            return;
        }

        SerializedObject serialized = new SerializedObject(controller);
        SerializedProperty pages = serialized.FindProperty("pages");
        Component prefabSource =
            PrefabUtility.GetCorrespondingObjectFromSource(controller);
        if (pages != null && prefabSource != null)
        {
            SerializedObject sourceSerialized = new SerializedObject(prefabSource);
            SerializedProperty sourcePages = sourceSerialized.FindProperty("pages");
            if (sourcePages != null && sourcePages.isArray && sourcePages.arraySize > 0)
            {
                Undo.RecordObject(controller, "Restore prefab page references");
                PrefabUtility.RevertPropertyOverride(
                    pages,
                    InteractionMode.UserAction);
                EditorUtility.SetDirty(controller);
                return;
            }
        }

        Undo.RecordObject(controller, "Repair page references");
        if (controller is HearthFirstPersonHudController)
        {
            HearthFirstPersonHudPage[] discovered =
                controller.GetComponentsInChildren<HearthFirstPersonHudPage>(true);
            Array.Sort(
                discovered,
                (left, right) => ((int)left.PageId).CompareTo((int)right.PageId));
            SetObjectReferenceArray(serialized, pages, discovered);
        }
        else if (controller is HearthTvTerminalController)
        {
            HearthHudPage[] discovered =
                controller.GetComponentsInChildren<HearthHudPage>(true);
            Array.Sort(
                discovered,
                (left, right) => ((int)left.PageId).CompareTo((int)right.PageId));
            SetObjectReferenceArray(serialized, pages, discovered);
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(controller);
    }

    private static void SetObjectReferenceArray(
        SerializedObject serialized,
        SerializedProperty property,
        UnityEngine.Object[] values)
    {
        if (serialized == null || property == null || !property.isArray)
        {
            return;
        }

        property.arraySize = values != null ? values.Length : 0;
        for (int i = 0; values != null && i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
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

    private static void RepairTerminal(
        HearthTvTerminalController terminal,
        TerminalCameraBinding binding,
        ViewSwitchController canonical,
        HearthPlayerControlLock controlLock)
    {
        Undo.RecordObject(terminal, "Repair terminal topology");
        SerializedObject serialized = new SerializedObject(terminal);
        SetObjectReference(serialized, "terminalCamera", binding.Camera);
        SetObjectReference(serialized, "terminalHardwareRoot", binding.HardwareRoot);
        SetObjectReference(serialized, "worldCamera", binding.Camera);
        SetObjectReference(serialized, "viewSwitchController", canonical);
        SetObjectReference(serialized, "playerControlLock", controlLock);
        SetBool(serialized, "switchCameraWhileOpen", true);
        SetBool(serialized, "hideFirstPersonUiWhileOpen", true);
        CompactObjectReferenceArray(serialized.FindProperty("gameplayBehavioursToDisable"));
        serialized.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.RecordPrefabInstancePropertyModifications(terminal);
        EditorUtility.SetDirty(terminal);

        Canvas canvas = terminal.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = terminal.GetComponentInParent<Canvas>();
        }

        if (canvas != null)
        {
            Undo.RecordObject(canvas, "Repair terminal world camera");
            canvas.worldCamera = binding.Camera;
            PrefabUtility.RecordPrefabInstancePropertyModifications(canvas);
            EditorUtility.SetDirty(canvas);
        }

        Undo.RecordObject(binding.Camera, "Disable terminal camera at rest");
        binding.Camera.enabled = false;
        PrefabUtility.RecordPrefabInstancePropertyModifications(binding.Camera);
        EditorUtility.SetDirty(binding.Camera);

        AudioListener listener = binding.Camera.GetComponent<AudioListener>();
        if (listener != null)
        {
            Undo.RecordObject(listener, "Disable terminal listener at rest");
            listener.enabled = false;
            PrefabUtility.RecordPrefabInstancePropertyModifications(listener);
            EditorUtility.SetDirty(listener);
        }
    }

    private static void RepairTerminalFlowCallbacks(
        HearthTvTerminalController lobbyTerminal,
        HearthTvTerminalController homeTerminal,
        HearthLobbyFlowController lobbyFlow,
        Hearth17F04FinaleController finaleController)
    {
        Undo.RecordObject(lobbyTerminal, "Repair Lobby terminal callbacks");
        ReplacePersistentListener(
            lobbyTerminal.OnOpened,
            lobbyFlow.BeginAssignmentBriefingFromTerminal);
        ReplacePersistentListener(
            lobbyTerminal.OnCustomPrimaryAction,
            lobbyFlow.ConfirmAssignmentTerminalClose);
        PrefabUtility.RecordPrefabInstancePropertyModifications(
            lobbyTerminal);
        EditorUtility.SetDirty(lobbyTerminal);

        Undo.RecordObject(homeTerminal, "Repair 17F04 terminal callback");
        ReplacePersistentListener(
            homeTerminal.OnCustomPrimaryAction,
            finaleController.BeginFromHomeTerminal);
        PrefabUtility.RecordPrefabInstancePropertyModifications(
            homeTerminal);
        EditorUtility.SetDirty(homeTerminal);
    }

    private static void RepairLobbyActiveLoopCue(
        HearthTvTerminalController lobbyTerminal,
        HearthSfxCuePlayer lobbyCuePlayer)
    {
        Undo.RecordObject(lobbyTerminal, "Repair Lobby terminal active loop cue");
        SerializedObject serialized = new SerializedObject(lobbyTerminal);
        SetObjectReference(
            serialized,
            "activeLoopCuePlayer",
            lobbyCuePlayer);
        SerializedProperty cueId = serialized.FindProperty("activeLoopCueId");
        if (cueId != null)
        {
            cueId.stringValue = "AssignmentTerminal.Hum";
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.RecordPrefabInstancePropertyModifications(
            lobbyTerminal);
        EditorUtility.SetDirty(lobbyTerminal);
    }

    private static void ReplacePersistentListener(
        UnityEvent targetEvent,
        UnityAction listener)
    {
        if (targetEvent == null || listener == null)
        {
            return;
        }

        for (int i = targetEvent.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            UnityEventTools.RemovePersistentListener(targetEvent, i);
        }

        UnityEventTools.AddPersistentListener(targetEvent, listener);
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

    private static void RepairTransitionAudioContinuity(Scene scene)
    {
        HearthTerminalCameraTransition[] transitions =
            FindSceneComponents<HearthTerminalCameraTransition>(scene);
        for (int i = 0; i < transitions.Length; i++)
        {
            Undo.RecordObject(transitions[i], "Enable transition audio continuity");
            SerializedObject serialized = new SerializedObject(transitions[i]);
            SetBool(serialized, "copyAudioListenerIfMissing", true);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.RecordPrefabInstancePropertyModifications(
                transitions[i]);
            EditorUtility.SetDirty(transitions[i]);
        }
    }

    private static void RemapSceneObjectReferences(
        Scene scene,
        HashSet<UnityEngine.Object> oldReferences,
        UnityEngine.Object replacement)
    {
        if (oldReferences == null || oldReferences.Count == 0)
        {
            return;
        }

        MonoBehaviour[] behaviours = FindSceneComponents<MonoBehaviour>(scene);
        for (int i = 0; i < behaviours.Length; i++)
        {
            MonoBehaviour behaviour = behaviours[i];
            if (behaviour == null)
            {
                continue;
            }

            SerializedObject serialized;
            try
            {
                serialized = new SerializedObject(behaviour);
            }
            catch
            {
                continue;
            }

            SerializedProperty property = serialized.GetIterator();
            bool enterChildren = true;
            bool changed = false;
            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (property.propertyType == SerializedPropertyType.ObjectReference &&
                    oldReferences.Contains(property.objectReferenceValue))
                {
                    if (!changed)
                    {
                        Undo.RecordObject(behaviour, "Remap canonical ViewSwitch");
                    }

                    property.objectReferenceValue = replacement;
                    changed = true;
                }
            }

            if (changed)
            {
                serialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(behaviour);
            }
        }
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

    private static void SetObjectReference(
        SerializedObject serialized,
        string propertyName,
        UnityEngine.Object value)
    {
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null &&
            property.propertyType == SerializedPropertyType.ObjectReference)
        {
            property.objectReferenceValue = value;
        }
    }

    private static void SetBool(
        SerializedObject serialized,
        string propertyName,
        bool value)
    {
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null &&
            property.propertyType == SerializedPropertyType.Boolean)
        {
            property.boolValue = value;
        }
    }

    private static void CompactObjectReferenceArray(SerializedProperty property)
    {
        if (property == null || !property.isArray)
        {
            return;
        }

        List<UnityEngine.Object> values = new List<UnityEngine.Object>();
        for (int i = 0; i < property.arraySize; i++)
        {
            SerializedProperty element = property.GetArrayElementAtIndex(i);
            if (element.propertyType == SerializedPropertyType.ObjectReference &&
                element.objectReferenceValue != null)
            {
                values.Add(element.objectReferenceValue);
            }
        }

        property.arraySize = values.Count;
        for (int i = 0; i < values.Count; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
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
