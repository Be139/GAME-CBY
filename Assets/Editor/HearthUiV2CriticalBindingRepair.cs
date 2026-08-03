#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class HearthUiV2CriticalBindingRepair
{
    private const string MenuPath =
        HearthLegacyToolGuard.MenuRoot + "Repair 17F01-02 Critical Bindings";

    [MenuItem(MenuPath)]
    public static void RepairOpenScene()
    {
        if (!HearthLegacyToolGuard.Confirm(
                "Repair 17F01-02 Critical Bindings",
                "the active scene terminal references"))
        {
            return;
        }

        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning(
                "[HearthUiV2CriticalBindingRepair] Exit Play Mode before repairing bindings.");
            return;
        }

        Scene scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError(
                "[HearthUiV2CriticalBindingRepair] No loaded active scene was found.");
            return;
        }

        MinLoopFlowController flow = FindUniqueSceneComponent<MinLoopFlowController>(
            scene,
            "MinLoopFlowController");
        PlayerInteraction interaction = FindPreferredPlayerInteraction(scene);
        Camera playerCamera = interaction != null
            ? interaction.mainCamera
            : null;

        if (playerCamera == null && interaction != null)
        {
            playerCamera = interaction.GetComponentInChildren<Camera>(true);
        }

        HearthTvTerminalController terminal17F01 =
            FindUniqueTerminal(scene, "17F01");
        HearthTvTerminalController terminal17F02 =
            FindUniqueTerminal(scene, "17F02");

        if (flow == null ||
            interaction == null ||
            playerCamera == null ||
            terminal17F01 == null ||
            terminal17F02 == null)
        {
            Debug.LogError(
                "[HearthUiV2CriticalBindingRepair] Repair aborted before mutation. " +
                "Required: one MinLoopFlowController, the Human PlayerInteraction and camera, " +
                "and one terminal each for 17F01 and 17F02.");
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Repair HEARTH UI V2 17F01-02 bindings");

        RepairTerminal(terminal17F01, flow, interaction, playerCamera);
        RepairTerminal(terminal17F02, flow, interaction, playerCamera);

        EditorSceneManager.MarkSceneDirty(scene);
        Undo.FlushUndoRecordObjects();
        Undo.CollapseUndoOperations(undoGroup);
        Debug.Log(
            "[HearthUiV2CriticalBindingRepair] Repaired only the six critical 17F01/02 " +
            "terminal references: flow controller, Human camera and PlayerInteraction. " +
            "Review the diff and save the scene manually.");
    }

    private static void RepairTerminal(
        HearthTvTerminalController terminal,
        MinLoopFlowController flow,
        PlayerInteraction interaction,
        Camera playerCamera)
    {
        Undo.RecordObject(terminal, "Repair terminal critical references");
        terminal.SetMinLoopFlowController(flow);
        terminal.SetPlayerInteraction(interaction);
        terminal.SetPlayerCamera(playerCamera);
        PrefabUtility.RecordPrefabInstancePropertyModifications(terminal);
        EditorUtility.SetDirty(terminal);
    }

    private static PlayerInteraction FindPreferredPlayerInteraction(Scene scene)
    {
        PlayerInteraction[] interactions =
            FindSceneComponents<PlayerInteraction>(scene);
        PlayerInteraction fallback = null;

        for (int i = 0; i < interactions.Length; i++)
        {
            PlayerInteraction candidate = interactions[i];
            string path = GetPath(candidate.transform);
            if (path.IndexOf(
                    "Player/Person Controller",
                    StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return candidate;
            }

            if (fallback == null &&
                candidate.enabled &&
                candidate.gameObject.activeInHierarchy)
            {
                fallback = candidate;
            }
        }

        return fallback;
    }

    private static HearthTvTerminalController FindUniqueTerminal(
        Scene scene,
        string residentId)
    {
        HearthTvTerminalController[] terminals =
            FindSceneComponents<HearthTvTerminalController>(scene);
        HearthTvTerminalController match = null;

        for (int i = 0; i < terminals.Length; i++)
        {
            HearthTvTerminalController candidate = terminals[i];
            string normalizedName =
                candidate.name.Replace("-", string.Empty)
                    .Replace("_", string.Empty)
                    .Replace(" ", string.Empty)
                    .ToUpperInvariant();
            if (!string.Equals(
                    candidate.GetReplayResidentId(),
                    residentId,
                    StringComparison.OrdinalIgnoreCase) ||
                normalizedName.IndexOf(
                    residentId,
                    StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }

            if (match != null)
            {
                Debug.LogError(
                    "[HearthUiV2CriticalBindingRepair] More than one terminal resolves to " +
                    residentId + ".");
                return null;
            }

            match = candidate;
        }

        return match;
    }

    private static T FindUniqueSceneComponent<T>(
        Scene scene,
        string label)
        where T : Component
    {
        T[] components = FindSceneComponents<T>(scene);
        if (components.Length == 1)
        {
            return components[0];
        }

        Debug.LogError(
            "[HearthUiV2CriticalBindingRepair] Expected exactly one " +
            label + "; found " + components.Length + ".");
        return null;
    }

    private static T[] FindSceneComponents<T>(Scene scene)
        where T : Component
    {
        List<T> results = new List<T>();
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            results.AddRange(roots[i].GetComponentsInChildren<T>(true));
        }

        return results.ToArray();
    }

    private static string GetPath(Transform target)
    {
        if (target == null)
        {
            return string.Empty;
        }

        string path = target.name;
        Transform current = target.parent;
        while (current != null)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return path;
    }
}
#endif
