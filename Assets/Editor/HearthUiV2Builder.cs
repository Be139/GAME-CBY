#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class HearthUiV2Builder
{
    private const string MenuRoot = "Tools/Hearth/UI V2/";
    private const string PartsRoot = "Assets/UI/HEARTH/GeneratedParts";
    private const string V2Root = "Assets/Prefabs/UI/HearthHud/V2";
    private const string ThemeProfilePath =
        "Assets/UI/HEARTH/V2/Profiles/Hearth_UiV2Theme.asset";
    private const string LayoutProfilePath =
        "Assets/UI/HEARTH/V2/Profiles/Hearth_UiV2Layout_1920x1080.asset";

    private const string LegacyHuman = "Assets/Prefabs/UI/HearthHud/HearthHudRoot.prefab";
    private const string LegacyCompanion = "Assets/Prefabs/UI/HearthHud/Companion/HearthCompanionHudRoot.prefab";
    private const string LegacyTerminal01 = "Assets/Prefabs/UI/HearthHud/Terminals/Terminal_17F01.prefab";
    private const string LegacyTerminal02 = "Assets/Prefabs/UI/HearthHud/Terminals/Terminal_17F02.prefab";
    private const string LegacyTerminal03 = "Assets/Prefabs/UI/HearthHud/Terminals/Terminal_17F03_Alert.prefab";
    private const string LegacyTerminal04 = "Assets/Prefabs/UI/HearthHud/Terminals/Terminal_17F04_Home.prefab";
    private const string LegacyTerminalLobby = "Assets/Prefabs/UI/HearthHud/Terminals/Terminal_Lobby_Assignment.prefab";

    private const string V2Human = V2Root + "/HearthHudRoot_V2.prefab";
    private const string V2Companion = V2Root + "/Companion/HearthCompanionHudRoot_V2.prefab";
    private const string V2Terminal01 = V2Root + "/Terminals/Terminal_17F01_V2.prefab";
    private const string V2Terminal02 = V2Root + "/Terminals/Terminal_17F02_V2.prefab";
    private const string V2Terminal03 = V2Root + "/Terminals/Terminal_17F03_Alert_V2.prefab";
    private const string V2Terminal04 = V2Root + "/Terminals/Terminal_17F04_Home_V2.prefab";
    private const string V2TerminalLobby = V2Root + "/Terminals/Terminal_Lobby_Assignment_V2.prefab";

    private const string PanelSpritePath = PartsRoot + "/Common/HUD_Common_PanelFrame_9Slice.png";
    private const string ButtonSpritePath = PartsRoot + "/Common/HUD_Common_ButtonFrame_9Slice.png";
    private const string DialogueSpritePath = PartsRoot + "/Common/HUD_Common_DialogueFrame.png";
    private const string KeycapSpritePath = PartsRoot + "/Common/HUD_Common_KeycapFrame_9Slice.png";
    private const string HeaderSpritePath = PartsRoot + "/Common/HUD_Common_HeaderUnderline.png";
    private const string HoldSpritePath = PartsRoot + "/Interaction/HUD_Interaction_HoldFrame.png";
    private const string HoldFillSpritePath = PartsRoot + "/Interaction/HUD_Interaction_HoldProgressFill.png";
    private const string TapSpritePath = PartsRoot + "/Interaction/HUD_Interaction_TapFrame_9Slice.png";
    private const string CompanionFrameSpritePath = PartsRoot + "/Companion/HUD_Companion_FullscreenFrame.png";
    private const string TerminalFrameSpritePath = PartsRoot + "/Terminal/HUD_Terminal_FullscreenFrame.png";
    private const string TerminalInfoSpritePath = PartsRoot + "/Terminal/HUD_Terminal_InfoPanelFrame_9Slice.png";
    private const string TerminalPortraitSpritePath = PartsRoot + "/Terminal/HUD_Terminal_PortraitFrame_9Slice.png";
    private const string WarningSpritePath = PartsRoot + "/Feedback/HUD_Feedback_WarningModalFrame_9Slice.png";
    private const string PhotoSpritePath = PartsRoot + "/Finale/HUD_Finale_PhotoFrame_9Slice.png";
    private const string ShutdownSpritePath = PartsRoot + "/Finale/HUD_Finale_ShutdownModalFrame_9Slice.png";

    private static HearthUiThemeProfile cachedTheme;
    private static HearthUiLayoutProfile cachedLayout;

    // Palette sampled from HEARTH-Night-Rounds-Master.pptx. The shared
    // ScriptableObject is the source of truth; these properties only provide
    // alpha variants and safe fallbacks for a missing profile.
    private static Color Cyan
    {
        get { return WithAlpha(ThemeColor(theme => theme.Information, new Color32(120, 170, 220, 255)), 235); }
    }

    private static Color CyanSoft
    {
        get { return WithAlpha(ThemeColor(theme => theme.Primary, new Color32(215, 230, 246, 255)), 242); }
    }

    private static Color TextPrimary
    {
        get { return ThemeColor(theme => theme.Primary, new Color32(215, 230, 246, 255)); }
    }

    private static Color TextSecondary
    {
        get { return WithAlpha(ThemeColor(theme => theme.Secondary, new Color32(95, 120, 149, 255)), 242); }
    }

    private static Color PanelFill
    {
        get { return WithAlpha(ThemeColor(theme => theme.TerminalPanelBackground, new Color32(9, 16, 28, 255)), 210); }
    }

    private static Color ButtonIdle
    {
        get { return WithAlpha(ThemeColor(theme => theme.Secondary, new Color32(95, 120, 149, 255)), 34); }
    }

    private static Color ButtonSelected
    {
        get { return WithAlpha(ThemeColor(theme => theme.Information, new Color32(120, 170, 220, 255)), 58); }
    }

    private static Color ButtonDisabled
    {
        get { return WithAlpha(ThemeColor(theme => theme.Secondary, new Color32(95, 120, 149, 255)), 20); }
    }

    private static Color TerminalGlass
    {
        get { return WithAlpha(ThemeColor(theme => theme.TerminalBackground, new Color32(11, 16, 24, 255)), 238); }
    }

    private static Color Amber
    {
        get { return ThemeColor(theme => theme.Warning, new Color32(224, 151, 63, 255)); }
    }

    private static Color Success
    {
        get { return ThemeColor(theme => theme.Success, new Color32(87, 184, 138, 255)); }
    }

    private static Color Danger
    {
        get { return ThemeColor(theme => theme.Danger, new Color32(224, 82, 77, 255)); }
    }

    private static readonly string[] HumanFunctionalProperties =
    {
        "startingPage",
        "trustScore",
        "finalChoiceTrustThreshold",
        "trustDeltaSeconds",
        "completedRounds",
        "totalRounds",
        "routeFinalChoiceInternally",
        "quitApplicationOnExitConfirm",
        "lockPlayerControlsWhileOverlayOpen",
        "playerControlLock",
        "openMenuClip",
        "closeMenuClip",
        "pageChangedClip",
        "focusMovedClip",
        "confirmClip",
        "cancelClip",
        "warningClip",
        "trustDeltaClip"
    };

    private static readonly string[] CompanionFunctionalProperties =
    {
        "scenes",
        "startingSceneId",
        "showStartingSceneOnStart",
        "visibleOnlyInCompanionView",
        "viewSwitchController",
        "autoFindViewSwitchController",
        "sceneChangedClip",
        "holdCompletedClip",
        "specialEffectClip",
        "autoAdvanceOnHoldPrompt"
    };

    private static readonly string[] TerminalFunctionalProperties =
    {
        "createEventSystemIfMissing",
        "hideCanvasWhenClosed",
        "lockGameplayWhileOpen",
        "playerControlLock",
        "gameplayBehavioursToDisable",
        "playerInteraction",
        "playerRigidbody",
        "unlockCursorWhileOpen",
        "openClip",
        "closeClip",
        "bootClip",
        "pageSwitchClip",
        "focusMoveClip",
        "submitClip",
        "replayRequestClip",
        "viewSwitchClip",
        "activeLoopClip",
        "activeLoopCuePlayer",
        "activeLoopCueId",
        "activeLoopVolume",
        "audioVolume",
        "primaryAction",
        "minLoopFlowController",
        "viewSwitchController",
        "replayResidentId",
        "closeTerminalWhenReplayStarts",
        "deferCustomActionCloseUntilExternalFade",
        "showFinalChoiceWhenReplayUnavailable",
        "closeTerminalWhenChoiceSubmitted",
        "preventRepeatedChoiceSubmission",
        "routeChoicesToMinLoop"
    };

    [MenuItem(MenuRoot + "Rebuild All V2 UI Assets")]
    public static void RebuildAllV2Assets()
    {
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Prefabs/UI");
        EnsureFolder("Assets/Prefabs/UI/HearthHud");
        EnsureFolder(V2Root);
        EnsureFolder(V2Root + "/Companion");
        EnsureFolder(V2Root + "/Terminals");

        ConfigureGeneratedPartImporters();
        AssetDatabase.Refresh();

        BuildClone(LegacyHuman, V2Human, StyleHumanHud);
        BuildClone(LegacyCompanion, V2Companion, StyleCompanionHud);
        BuildClone(
            LegacyTerminal01,
            V2Terminal01,
            root => StyleHouseholdTerminal(
                root,
                "17F-01",
                "SON",
                "DAD",
                "MOM",
                "REVIEW ARCHIVED EVENT",
                HearthTerminalPrimaryAction.RequestReplay));
        BuildClone(
            LegacyTerminal02,
            V2Terminal02,
            root => StyleHouseholdTerminal(
                root,
                "17F-02",
                "WIFE",
                "HUSBAND",
                "UNIT",
                "REVIEW ARCHIVED EVENT",
                HearthTerminalPrimaryAction.RequestReplay));
        BuildClone(
            LegacyTerminal03,
            V2Terminal03,
            root => StyleHouseholdTerminal(
                root,
                "17F-03",
                "DAUGHTER",
                "MOTHER",
                "FATHER",
                "ENTER UNIT",
                HearthTerminalPrimaryAction.EnterUnit));
        BuildClone(LegacyTerminal04, V2Terminal04, StyleHomeTerminal);
        BuildClone(LegacyTerminalLobby, V2TerminalLobby, StyleLobbyTerminal);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[HearthUiV2Builder] Rebuilt the independent HEARTH V2 human HUD, companion HUD and five terminal prefabs.");
    }

    [MenuItem(MenuRoot + "Refresh Existing V2 Prefab Visuals")]
    public static void RefreshExistingV2PrefabVisuals()
    {
        if (!EnsureV2AssetsExist())
        {
            return;
        }

        RestyleExistingPrefab(V2Human, StyleHumanHud);
        RestyleExistingPrefab(V2Companion, StyleCompanionHud);
        RestyleExistingPrefab(
            V2Terminal01,
            root => StyleHouseholdTerminal(
                root,
                "17F-01",
                "SON",
                "DAD",
                "MOM",
                "REVIEW ARCHIVED EVENT",
                HearthTerminalPrimaryAction.RequestReplay));
        RestyleExistingPrefab(
            V2Terminal02,
            root => StyleHouseholdTerminal(
                root,
                "17F-02",
                "WIFE",
                "HUSBAND",
                "UNIT",
                "REVIEW ARCHIVED EVENT",
                HearthTerminalPrimaryAction.RequestReplay));
        RestyleExistingPrefab(
            V2Terminal03,
            root => StyleHouseholdTerminal(
                root,
                "17F-03",
                "DAUGHTER",
                "MOTHER",
                "FATHER",
                "ENTER UNIT",
                HearthTerminalPrimaryAction.EnterUnit));
        RestyleExistingPrefab(V2Terminal04, StyleHomeTerminal);
        RestyleExistingPrefab(V2TerminalLobby, StyleLobbyTerminal);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log(
            "[HearthUiV2Builder] Refreshed only the visual trees inside the seven " +
            "existing V2 prefabs. Functional roots and serialized gameplay bindings were retained.");
    }

    [MenuItem(MenuRoot + "Use V2 UI In Open Scene")]
    public static void UseV2InOpenScene()
    {
        if (!EnsureV2AssetsExist())
        {
            return;
        }

        SwitchOpenScene(true);
    }

    [MenuItem(MenuRoot + "Use Legacy UI In Open Scene")]
    public static void UseLegacyInOpenScene()
    {
        SwitchOpenScene(false);
    }

    [MenuItem(MenuRoot + "Validate Open Scene UI")]
    public static void ValidateOpenSceneUi()
    {
        ValidateOpenSceneUiInternal(true);
    }

    private static bool ValidateOpenSceneUiInternal(
        bool logSuccess,
        bool? expectedV2 = null)
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid() ||
            !scene.isLoaded ||
            string.IsNullOrEmpty(scene.path))
        {
            Debug.LogError("[HearthUiV2Builder] No valid loaded active scene.");
            return false;
        }

        int issues = 0;
        HearthFirstPersonHudController[] human =
            FindSceneObjects<HearthFirstPersonHudController>(scene).ToArray();
        HearthCompanionHudController[] companion =
            FindSceneObjects<HearthCompanionHudController>(scene).ToArray();
        HearthTvTerminalController[] terminals =
            FindSceneObjects<HearthTvTerminalController>(scene).ToArray();
        HearthUiStateCoordinator[] stateCoordinators =
            FindSceneObjects<HearthUiStateCoordinator>(scene).ToArray();
        ViewSwitchController[] viewSwitches =
            FindSceneObjects<ViewSwitchController>(scene).ToArray();

        if (human.Length != 1)
        {
            Debug.LogWarning("[HearthUiV2Builder] Expected exactly one human HUD, found " + human.Length + ".");
            issues++;
        }
        else
        {
            issues += ValidateHumanPageReferences(human[0]);
        }

        if (companion.Length != 1)
        {
            Debug.LogWarning("[HearthUiV2Builder] Expected exactly one companion HUD, found " + companion.Length + ".");
            issues++;
        }

        if (terminals.Length != 5)
        {
            Debug.LogWarning("[HearthUiV2Builder] Expected exactly five terminals, found " + terminals.Length + ".");
            issues++;
        }

        if (viewSwitches.Length != 1 ||
            (viewSwitches.Length == 1 &&
             (GetPath(viewSwitches[0].transform) != "MIN_LOOP_ROOT/FlowManagers/ViewSwitchController" ||
              !viewSwitches[0].enabled ||
              !viewSwitches[0].gameObject.activeInHierarchy)))
        {
            Debug.LogWarning(
                "[HearthUiV2Builder] Expected one canonical ViewSwitchController at MIN_LOOP_ROOT/FlowManagers/ViewSwitchController, found " +
                viewSwitches.Length + ".");
            issues++;
        }

        if (stateCoordinators.Length != 1 ||
            !stateCoordinators[0].enabled ||
            !stateCoordinators[0].gameObject.activeInHierarchy ||
            !stateCoordinators[0].AutomaticallyResolveRuntimeState ||
            !stateCoordinators[0].HasHumanHudBinding)
        {
            Debug.LogWarning(
                "[HearthUiV2Builder] Expected one active V2 UI state coordinator with automatic runtime resolution and a human HUD binding, found " +
                stateCoordinators.Length + ".");
            issues++;
        }

        HashSet<string> terminalSlots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        HashSet<Camera> terminalCameras = new HashSet<Camera>();
        for (int i = 0; i < terminals.Length; i++)
        {
            issues += ValidateTerminalPageReferences(terminals[i]);

            SerializedObject so = new SerializedObject(terminals[i]);
            SerializedProperty switchCamera = so.FindProperty("switchCameraWhileOpen");
            SerializedProperty worldCamera = so.FindProperty("worldCamera");

            Transform hardwareRoot = FindTerminalHardwareRoot(terminals[i].transform);
            Camera expectedCamera = FindCameraOutsideUiRoot(hardwareRoot, terminals[i].transform);
            Canvas terminalCanvas = terminals[i].GetComponent<Canvas>();
            if (terminalCanvas == null)
            {
                terminalCanvas = terminals[i].GetComponentInParent<Canvas>();
            }

            if (switchCamera == null ||
                !switchCamera.boolValue ||
                expectedCamera == null ||
                terminals[i].TerminalCamera != expectedCamera ||
                worldCamera == null ||
                worldCamera.objectReferenceValue != expectedCamera ||
                terminalCanvas == null ||
                terminalCanvas.worldCamera != expectedCamera ||
                !terminalCameras.Add(expectedCamera))
            {
                Debug.LogWarning(
                    "[HearthUiV2Builder] Terminal camera is missing, ambiguous, or does not belong to its physical TV: " +
                    GetPath(terminals[i].transform),
                    terminals[i]);
                issues++;
            }

            string slot = ResolveTargetPrefabPath(terminals[i].gameObject, true);
            if (string.IsNullOrEmpty(slot) || !terminalSlots.Add(slot))
            {
                Debug.LogWarning(
                    "[HearthUiV2Builder] Terminal identity is missing or duplicated: " +
                    GetPath(terminals[i].transform),
                    terminals[i]);
                issues++;
            }
        }

        issues += ValidateRequiredTerminalCallbacks(terminals);
        issues += ValidateLobbyActiveLoopCue(terminals);

        List<GameObject> uiRoots = new List<GameObject>();
        if (human.Length == 1)
        {
            uiRoots.Add(human[0].gameObject);
        }

        if (companion.Length == 1)
        {
            uiRoots.Add(companion[0].gameObject);
        }

        for (int i = 0; i < terminals.Length; i++)
        {
            uiRoots.Add(terminals[i].gameObject);
        }

        int v2MarkerCount = 0;
        for (int i = 0; i < uiRoots.Count; i++)
        {
            HearthUiThemeMarker marker = uiRoots[i].GetComponent<HearthUiThemeMarker>();
            if (marker == null)
            {
                continue;
            }

            v2MarkerCount++;
            if (marker.Version != HearthUiThemeVersion.V2 ||
                marker.BuildLabel != "HEARTH UI V2")
            {
                Debug.LogWarning(
                    "[HearthUiV2Builder] Invalid V2 theme marker: " + GetPath(uiRoots[i].transform),
                    uiRoots[i]);
                issues++;
            }
        }

        if (expectedV2.HasValue &&
            v2MarkerCount != (expectedV2.Value ? 7 : 0))
        {
            Debug.LogWarning(
                "[HearthUiV2Builder] Expected " +
                (expectedV2.Value ? "seven V2 markers" : "zero V2 markers") +
                ", found " + v2MarkerCount + ".");
            issues++;
        }
        else if (!expectedV2.HasValue &&
                 v2MarkerCount != 0 &&
                 v2MarkerCount != 7)
        {
            Debug.LogWarning(
                "[HearthUiV2Builder] Mixed Legacy/V2 UI is not allowed. Found " +
                v2MarkerCount + " V2 marker(s) across seven UI slots.");
            issues++;
        }

        if (issues == 0)
        {
            if (logSuccess)
            {
                Debug.Log(
                    "[HearthUiV2Builder] UI validation passed: seven unique UI slots, one canonical ViewSwitchController, and five locally owned terminal cameras.");
            }
        }
        else
        {
            Debug.LogWarning("[HearthUiV2Builder] UI validation finished with " + issues + " issue(s).");
        }

        return issues == 0;
    }

    [MenuItem(MenuRoot + "Preview/Open Human Tab Menu")]
    private static void PreviewOpenHumanTabMenu()
    {
        if (!RequirePlayMode())
        {
            return;
        }

        Application.runInBackground = true;
        HearthFirstPersonHudController controller = FindSceneObject<HearthFirstPersonHudController>();
        if (controller == null)
        {
            Debug.LogWarning("[HearthUiV2Builder] No scene HearthFirstPersonHudController was found.");
            return;
        }

        controller.OpenMainMenu();
        Debug.Log("[HearthUiV2Builder] Opened the human Tab menu for V2 preview.");
    }

    [MenuItem(MenuRoot + "Preview/Open 17F01 Terminal")]
    private static void PreviewOpen17F01Terminal()
    {
        if (!RequirePlayMode())
        {
            return;
        }

        Application.runInBackground = true;
        HearthTvTerminalController terminal = FindSceneObjects<HearthTvTerminalController>()
            .Find(candidate => GetPath(candidate.transform).IndexOf("Terminal_17F01", StringComparison.OrdinalIgnoreCase) >= 0);
        if (terminal == null)
        {
            terminal = FindSceneObject<HearthTvTerminalController>();
        }

        if (terminal == null)
        {
            Debug.LogWarning("[HearthUiV2Builder] No scene terminal controller was found.");
            return;
        }

        terminal.OpenTerminal();
        Debug.Log("[HearthUiV2Builder] Opened terminal preview: " + GetPath(terminal.transform));
    }

    [MenuItem(MenuRoot + "Preview/Close Open Terminal")]
    private static void PreviewCloseOpenTerminal()
    {
        if (!RequirePlayMode())
        {
            return;
        }

        List<HearthTvTerminalController> terminals = FindSceneObjects<HearthTvTerminalController>();
        for (int i = 0; i < terminals.Count; i++)
        {
            if (!terminals[i].IsOpen)
            {
                continue;
            }

            terminals[i].CloseTerminal();
            Debug.Log("[HearthUiV2Builder] Closed terminal preview: " + GetPath(terminals[i].transform));
            return;
        }

        Debug.LogWarning("[HearthUiV2Builder] No open terminal was found.");
    }

    [MenuItem(MenuRoot + "Preview/Show Companion HUD")]
    private static void PreviewShowCompanionHud()
    {
        if (!RequirePlayMode())
        {
            return;
        }

        Application.runInBackground = true;
        ViewSwitchController viewSwitch = ViewSwitchController.FindPreferredController();
        if (viewSwitch == null)
        {
            Debug.LogWarning("[HearthUiV2Builder] No preferred ViewSwitchController was found.");
            return;
        }

        viewSwitch.SwitchToCompanion();
        HearthCompanionHudController companionHud = FindSceneObject<HearthCompanionHudController>();
        if (companionHud != null)
        {
            companionHud.SetVisible(true);
            companionHud.ShowScene("17F01_01");
        }

        Debug.Log("[HearthUiV2Builder] Switched to the companion view and opened the V2 companion HUD preview.");
    }

    [MenuItem(MenuRoot + "Preview/Restore Human View")]
    private static void PreviewRestoreHumanView()
    {
        if (!RequirePlayMode())
        {
            return;
        }

        Application.runInBackground = true;
        ViewSwitchController viewSwitch = ViewSwitchController.FindPreferredController();
        if (viewSwitch == null)
        {
            Debug.LogWarning("[HearthUiV2Builder] No preferred ViewSwitchController was found.");
            return;
        }

        viewSwitch.SwitchToHuman();
        HearthCompanionHudController companionHud = FindSceneObject<HearthCompanionHudController>();
        if (companionHud != null)
        {
            companionHud.SetVisible(false);
        }

        Debug.Log("[HearthUiV2Builder] Restored the human view after the companion HUD preview.");
    }

    private static bool EnsureV2AssetsExist()
    {
        string[] requiredPaths =
        {
            V2Human,
            V2Companion,
            V2Terminal01,
            V2Terminal02,
            V2Terminal03,
            V2Terminal04,
            V2TerminalLobby
        };

        bool allPresent = true;
        for (int i = 0; i < requiredPaths.Length; i++)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(requiredPaths[i]) == null)
            {
                Debug.LogError(
                    "[HearthUiV2Builder] Required V2 prefab is missing. Run Rebuild All V2 UI Assets explicitly before switching: " +
                    requiredPaths[i]);
                allPresent = false;
            }
        }

        return allPresent;
    }

    private static void BuildClone(string sourcePath, string targetPath, Action<GameObject> styleAction)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath) == null)
        {
            Debug.LogWarning("[HearthUiV2Builder] Source prefab is missing and was skipped: " + sourcePath);
            return;
        }

        GameObject root = PrefabUtility.LoadPrefabContents(sourcePath);
        try
        {
            RemoveExistingV2Objects(root);
            styleAction(root);
            HearthUiThemeMarker marker = root.GetComponent<HearthUiThemeMarker>();
            if (marker == null)
            {
                marker = root.AddComponent<HearthUiThemeMarker>();
            }

            marker.Configure(HearthUiThemeVersion.V2, "HEARTH UI V2");
            PrefabUtility.SaveAsPrefabAsset(root, targetPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void RestyleExistingPrefab(string prefabPath, Action<GameObject> styleAction)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null)
        {
            Debug.LogError("[HearthUiV2Builder] Existing V2 prefab is missing and was not restyled: " + prefabPath);
            return;
        }

        GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
        try
        {
            RemoveExistingV2Objects(root);
            styleAction(root);

            HearthUiThemeMarker marker = root.GetComponent<HearthUiThemeMarker>();
            if (marker == null)
            {
                marker = root.AddComponent<HearthUiThemeMarker>();
            }

            marker.Configure(HearthUiThemeVersion.V2, "HEARTH UI V2");
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static void StyleHumanHud(GameObject root)
    {
        StyleAllText(root, 18f, 34f);
        StyleGenericHumanPages(root);
        StyleHumanSpecializedPages(root);
        StyleHumanPersistentHud(root);
        StyleHumanMainMenu(root);
        BindHumanFocusTargets(root);
        StyleInteractionPrompt(root.transform.Find("InteractionPromptLayer/PlayerInteractionPrompt"), false);
    }

    private static void StyleHumanPersistentHud(GameObject root)
    {
        Transform persistent = root.transform.Find("PersistentHudLayer/PersistentHud");
        if (persistent == null)
        {
            return;
        }

        DisableDirectImages(persistent);

        TMP_Text status = FindTextByName(persistent, "Text_003_");
        TMP_Text identity = FindTextByName(persistent, "Text_004_");
        TMP_Text taskTitle = FindTextByName(persistent, "Text_006_");
        TMP_Text taskValue = FindTextByName(persistent, "Text_007_");

        Rect identityRegion = LayoutRect(
            HearthUiLayoutRegion.HumanIdentity,
            new Rect(56f, 42f, 430f, 96f));
        Rect taskRegion = LayoutRect(
            HearthUiLayoutRegion.CurrentTask,
            new Rect(1510f, 54f, 350f, 104f));
        Rect locationRegion = LayoutRect(
            HearthUiLayoutRegion.SharedLocation,
            new Rect(56f, 952f, 380f, 86f));

        SetText(status, "COMPANION UNIT · ACTIVE", new Rect(identityRegion.x, identityRegion.y, identityRegion.width, 34f), 21f, TextAlignmentOptions.TopLeft, TextSecondary);
        SetText(identity, "MIA · ID 7842", new Rect(identityRegion.x, identityRegion.y + 40f, identityRegion.width, 44f), 32f, TextAlignmentOptions.TopLeft, TextPrimary);
        SetText(taskTitle, "CURRENT TASK", new Rect(taskRegion.x, taskRegion.y, taskRegion.width, 30f), 19f, TextAlignmentOptions.TopLeft, TextSecondary);
        SetText(taskValue, "REVIEW THE HOUSEHOLD TERMINAL", new Rect(taskRegion.x, taskRegion.y + 36f, taskRegion.width, 56f), 22f, TextAlignmentOptions.TopLeft, TextPrimary);

        CreateImage(persistent, "V2_HeaderUnderline", new Rect(identityRegion.x, identityRegion.y + 34f, Mathf.Min(310f, identityRegion.width), 2f), null, Cyan, false, false);
        CreateImage(persistent, "V2_TaskUnderline", new Rect(taskRegion.x, taskRegion.y + 28f, Mathf.Min(290f, taskRegion.width), 2f), null, Cyan, false, false);

        Transform location = persistent.Find("LocationHud");
        if (location != null)
        {
            SetTopLeft(location as RectTransform, locationRegion);
            TMP_Text title = location.Find("LocationTitleText") != null ? location.Find("LocationTitleText").GetComponent<TMP_Text>() : null;
            TMP_Text glow = location.Find("LocationGlowText") != null ? location.Find("LocationGlowText").GetComponent<TMP_Text>() : null;
            TMP_Text value = location.Find("LocationValueText") != null ? location.Find("LocationValueText").GetComponent<TMP_Text>() : null;
            if (glow != null)
            {
                glow.gameObject.SetActive(false);
            }

            SetText(title, "LOCATION", new Rect(0f, 0f, 340f, 28f), 19f, TextAlignmentOptions.TopLeft, TextSecondary);
            SetText(value, "17F CORRIDOR", new Rect(0f, 30f, 340f, 38f), 25f, TextAlignmentOptions.TopLeft, TextPrimary);
        }
    }

    private static void StyleHumanMainMenu(GameObject root)
    {
        Transform page = root.transform.Find("PanelLayer/Slide03_MainMenu");
        if (page == null)
        {
            return;
        }

        HearthFirstPersonHudPage pageComponent =
            page.GetComponent<HearthFirstPersonHudPage>();
        if (pageComponent != null)
        {
            pageComponent.Configure(
                pageComponent.PageId,
                false,
                false);
        }

        DisableDecorativeImages(page);
        TMP_Text[] legacyTexts = page.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < legacyTexts.Length; i++)
        {
            legacyTexts[i].gameObject.SetActive(false);
        }

        Image surface = CreateImage(
            page,
            "V2_MenuSurface",
            new Rect(0f, 0f, 1920f, 1080f),
            null,
            new Color32(6, 12, 21, 214),
            false,
            false);
        surface.transform.SetAsFirstSibling();
        CreateText(
            page,
            "V2_MenuIdentityState",
            "FIELD UNIT · ACTIVE",
            new Rect(64f, 54f, 500f, 32f),
            21f,
            TextAlignmentOptions.TopLeft,
            TextSecondary);
        CreateImage(
            page,
            "V2_MenuIdentityRule",
            new Rect(64f, 94f, 330f, 2f),
            null,
            Cyan,
            false,
            false);
        CreateText(
            page,
            "V2_MenuIdentity",
            "MIA · ID 7842",
            new Rect(64f, 108f, 500f, 44f),
            31f,
            TextAlignmentOptions.TopLeft,
            TextPrimary);

        string[] textPrefixes =
        {
            "Text_003_",
            "Text_004_",
            "Text_005_"
        };
        string[] buttonNames =
        {
            "Button_TODAY",
            "Button_DISPOSITION_HISTORY",
            "Button_SYSTEM_SETTINGS"
        };
        string[] labels =
        {
            "TONIGHT'S ROUNDS",
            "DISPOSITION HISTORY",
            "SYSTEM SETTINGS"
        };

        for (int i = 0; i < buttonNames.Length; i++)
        {
            Rect rect = new Rect(64f, 204f + i * 108f, 540f, 84f);
            Transform buttonTransform = page.Find(buttonNames[i]);
            if (buttonTransform != null)
            {
                SetTopLeft(buttonTransform as RectTransform, rect);
                Image image = buttonTransform.GetComponent<Image>();
                StyleButtonImage(image, ButtonIdle);
                CreateImage(
                    buttonTransform,
                    "V2_MenuButtonRule",
                    new Rect(0f, 0f, 3f, rect.height),
                    null,
                    i == 0 ? Cyan : TextSecondary,
                    false,
                    false);
            }

            TMP_Text label = FindTextByName(page, textPrefixes[i]);
            if (label != null)
            {
                label.gameObject.SetActive(true);
            }

            SetText(
                label,
                labels[i],
                new Rect(104f, 228f + i * 108f, 450f, 42f),
                25f,
                TextAlignmentOptions.MidlineLeft,
                TextPrimary);
        }

        Image taskPanel = CreateImage(
            page,
            "V2_MenuTaskPanel",
            new Rect(1428f, 130f, 420f, 190f),
            null,
            PanelFill,
            false,
            false);
        CreateImage(
            taskPanel.transform,
            "V2_MenuTaskRule",
            new Rect(0f, 0f, 420f, 2f),
            null,
            Cyan,
            false,
            false);
        CreateText(
            taskPanel.transform,
            "V2_MenuTaskTitle",
            "CURRENT TASK",
            new Rect(36f, 32f, 350f, 36f),
            24f,
            TextAlignmentOptions.TopLeft,
            TextPrimary);
        CreateText(
            taskPanel.transform,
            "V2_MenuTaskBody",
            "REVIEW THE HOUSEHOLD TERMINAL",
            new Rect(36f, 92f, 350f, 62f),
            21f,
            TextAlignmentOptions.TopLeft,
            TextSecondary);

        Image fieldUnitPanel = CreateImage(
            page,
            "V2_MenuFieldUnitPanel",
            new Rect(1428f, 354f, 420f, 260f),
            null,
            PanelFill,
            false,
            false);
        CreateImage(
            fieldUnitPanel.transform,
            "V2_MenuFieldUnitRule",
            new Rect(0f, 0f, 3f, 260f),
            null,
            Amber,
            false,
            false);
        CreateText(
            fieldUnitPanel.transform,
            "V2_MenuFieldUnitTitle",
            "FIELD UNIT",
            new Rect(36f, 34f, 350f, 38f),
            24f,
            TextAlignmentOptions.TopLeft,
            TextPrimary);
        CreateText(
            fieldUnitPanel.transform,
            "V2_MenuFieldUnitBody",
            "Household briefing and system guidance remain available here.",
            new Rect(36f, 98f, 350f, 112f),
            21f,
            TextAlignmentOptions.TopLeft,
            TextSecondary);

        CreateText(
            page,
            "V2_MenuLocationTitle",
            "LOCATION",
            new Rect(64f, 940f, 360f, 28f),
            18f,
            TextAlignmentOptions.TopLeft,
            TextSecondary);
        CreateText(
            page,
            "V2_MenuLocationValue",
            "17F CORRIDOR",
            new Rect(64f, 974f, 360f, 38f),
            25f,
            TextAlignmentOptions.TopLeft,
            TextPrimary);
        CreateText(
            page,
            "V2_MenuHint",
            "UP / DOWN  SELECT     SPACE  CONFIRM     TAB  CLOSE",
            new Rect(1130f, 986f, 718f, 34f),
            19f,
            TextAlignmentOptions.TopRight,
            TextSecondary);
    }

    private static void BindHumanFocusTargets(GameObject root)
    {
        AssignHumanFocusTargets(root, true);
    }

    private static void AssignHumanFocusTargets(GameObject root, bool styleFocusRects)
    {
        HearthFirstPersonHudController controller = root.GetComponent<HearthFirstPersonHudController>();
        if (controller == null)
        {
            return;
        }

        Undo.RecordObject(controller, "Bind HEARTH HUD focus targets");
        Transform menuPage = root.transform.Find("PanelLayer/Slide03_MainMenu");
        RectTransform[] menuTargets =
        {
            FindRect(menuPage, "Button_TODAY"),
            FindRect(menuPage, "Button_DISPOSITION_HISTORY"),
            FindRect(menuPage, "Button_SYSTEM_SETTINGS")
        };

        RectTransform[] choiceTargets =
        {
            FindRect(root.transform, "FinalChoiceTarget_A"),
            FindRect(root.transform, "FinalChoiceTarget_B")
        };

        RectTransform menuFocus = FindRect(root.transform, "MenuFocus");
        RectTransform finalChoiceFocus = FindRect(root.transform, "FinalChoiceFocus");
        if (styleFocusRects)
        {
            StyleFocusRect(menuFocus);
            StyleFocusRect(finalChoiceFocus);
        }

        SerializedObject serialized = new SerializedObject(controller);
        SetObjectReference(serialized, "menuFocusRect", menuFocus);
        SetObjectReferenceArray(serialized, "menuFocusTargets", menuTargets);
        SetObjectReference(serialized, "finalChoiceFocusRect", finalChoiceFocus);
        SetObjectReferenceArray(serialized, "finalChoiceFocusTargets", choiceTargets);
        serialized.ApplyModifiedProperties();
        PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
        EditorUtility.SetDirty(controller);
    }

    private static void StyleFocusRect(RectTransform focusRect)
    {
        if (focusRect == null)
        {
            return;
        }

        Image image = focusRect.GetComponent<Image>();
        if (image == null)
        {
            image = Undo.AddComponent<Image>(focusRect.gameObject);
        }

        Undo.RecordObject(image, "Style HEARTH HUD focus rect");
        StyleButtonImage(image, ButtonSelected);
        image.raycastTarget = false;
        PrefabUtility.RecordPrefabInstancePropertyModifications(image);
        EditorUtility.SetDirty(image);
    }

    private static void StyleGenericHumanPages(GameObject root)
    {
        HearthFirstPersonHudPage[] pages = root.GetComponentsInChildren<HearthFirstPersonHudPage>(true);
        for (int i = 0; i < pages.Length; i++)
        {
            HearthFirstPersonHudPage page = pages[i];
            int id = (int)page.PageId;
            if (id < 3 || id > 24)
            {
                continue;
            }

            bool fullscreenTakeover =
                page.FullscreenTakeover ||
                (id >= 10 && id <= 13);
            page.Configure(page.PageId, fullscreenTakeover, false);

            if (id == 3)
            {
                continue;
            }

            DisableDecorativeImages(page.transform);
            Rect panelRect = id == 4
                ? new Rect(330f, 120f, 1260f, 790f)
                : new Rect(390f, 150f, 1140f, 740f);
            Color pageFill =
                id == 10 || id == 24
                    ? new Color32(11, 16, 24, 244)
                    : id >= 11 && id <= 13
                        ? new Color32(28, 19, 15, 244)
                        : PanelFill;
            Image panel = CreateImage(page.transform, "V2_PagePanel", panelRect, null, pageFill, false, false);
            panel.transform.SetAsFirstSibling();
            CreateImage(panel.transform, "V2_TopRule", new Rect(0f, 0f, panelRect.width, 2f), null,
                id >= 11 && id <= 13 ? Amber : Cyan, false, false);

            Button[] buttons = page.GetComponentsInChildren<Button>(true);
            for (int b = 0; b < buttons.Length; b++)
            {
                StyleButtonImage(buttons[b].GetComponent<Image>(), b == 0 ? ButtonSelected : ButtonIdle);
            }
        }
    }

    private static void StyleHumanSpecializedPages(GameObject root)
    {
        HearthFirstPersonHudPage[] pages =
            root.GetComponentsInChildren<HearthFirstPersonHudPage>(true);
        for (int i = 0; i < pages.Length; i++)
        {
            switch (pages[i].PageId)
            {
                case HearthFirstPersonHudPageId.Slide07Photo2023:
                    StylePhotoArchivePage(pages[i], 1, 2);
                    break;
                case HearthFirstPersonHudPageId.Slide08Photo2026:
                    StylePhotoArchivePage(pages[i], 2, 2);
                    break;
                case HearthFirstPersonHudPageId.Slide09FinalChoice:
                case HearthFirstPersonHudPageId.Slide14FinalChoiceReturn:
                    StyleFinalChoicePage(pages[i]);
                    break;
                case HearthFirstPersonHudPageId.Slide10ShutdownConfirm:
                    StyleShutdownConfirmPage(pages[i]);
                    break;
                case HearthFirstPersonHudPageId.Slide11Warning01:
                case HearthFirstPersonHudPageId.Slide12Warning02:
                case HearthFirstPersonHudPageId.Slide13Warning03:
                    StyleLowTrustWarningPage(pages[i]);
                    break;
            }
        }
    }

    private static void StylePhotoArchivePage(
        HearthFirstPersonHudPage page,
        int pageNumber,
        int pageCount)
    {
        Rect panelRect = new Rect(300f, 132f, 1320f, 760f);
        Image panel = FindDirectImage(page.transform, "V2_PagePanel");
        if (panel != null)
        {
            SetTopLeft(panel.rectTransform, panelRect);
            panel.color = new Color(PanelFill.r, PanelFill.g, PanelFill.b, 0.94f);
        }

        CreateText(page.transform, "V2_PhotoArchiveHeading", "PHOTO ARCHIVE", new Rect(64f, 48f, 520f, 42f), 28f, TextAlignmentOptions.TopLeft, TextPrimary);
        CreateImage(page.transform, "V2_PhotoArchiveHeadingRule", new Rect(64f, 94f, 320f, 2f), null, Cyan, false, false);
        CreateText(page.transform, "V2_PhotoArchiveUnit", "HOME UNIT 17F-04", new Rect(64f, 106f, 520f, 40f), 22f, TextAlignmentOptions.TopLeft, TextSecondary);

        Image viewport = CreateImage(
            page.transform,
            "V2_PhotoViewport",
            new Rect(342f, 176f, 880f, 610f),
            null,
            new Color32(6, 12, 21, 238),
            false,
            false);
        CreateImage(viewport.transform, "V2_ViewportRule", new Rect(0f, 0f, 880f, 2f), null, Cyan, false, false);
        CreateText(viewport.transform, "V2_ArchiveSource", "ARCHIVE IMAGE", new Rect(42f, 532f, 796f, 36f), 18f, TextAlignmentOptions.BottomLeft, TextSecondary);

        Image metadata = CreateImage(
            page.transform,
            "V2_PhotoMetadata",
            new Rect(1260f, 176f, 318f, 610f),
            null,
            new Color(PanelFill.r, PanelFill.g, PanelFill.b, 0.72f),
            false,
            false);
        CreateImage(metadata.transform, "V2_MetadataRule", new Rect(0f, 0f, 2f, 610f), null, Cyan, false, false);

        TMP_Text heading = FindTextByName(page.transform, "Text_003_");
        TMP_Text date = FindTextByName(page.transform, "Text_004_");
        TMP_Text subject = FindTextByName(page.transform, "Text_005_");
        TMP_Text eventLabel = FindTextByName(page.transform, "Text_006_");
        TMP_Text row = FindTextByName(page.transform, "Text_007_");
        TMP_Text presence = FindTextByName(page.transform, "Text_008_");
        TMP_Text archive = FindTextByName(page.transform, "Text_014_");

        SetText(heading, "FAMILY RECORD", new Rect(1302f, 214f, 230f, 38f), 25f, TextAlignmentOptions.TopLeft, TextPrimary);
        SetText(date, date != null ? date.text : string.Empty, new Rect(1302f, 270f, 230f, 38f), 24f, TextAlignmentOptions.TopLeft, CyanSoft);
        SetText(subject, subject != null ? subject.text : string.Empty, new Rect(1302f, 340f, 230f, 60f), 21f, TextAlignmentOptions.TopLeft, TextPrimary);
        SetText(eventLabel, eventLabel != null ? eventLabel.text : string.Empty, new Rect(1302f, 424f, 230f, 76f), 20f, TextAlignmentOptions.TopLeft, TextSecondary);
        SetText(row, row != null ? row.text : string.Empty, new Rect(1302f, 520f, 230f, 40f), 20f, TextAlignmentOptions.TopLeft, TextSecondary);
        SetText(presence, presence != null ? presence.text : string.Empty, new Rect(1302f, 578f, 230f, 78f), 20f, TextAlignmentOptions.TopLeft, TextSecondary);
        SetText(archive, archive != null ? archive.text : string.Empty, new Rect(1302f, 708f, 230f, 36f), 18f, TextAlignmentOptions.BottomLeft, Amber);

        Image message = CreateImage(
            page.transform,
            "V2_PhotoFieldUnit",
            new Rect(470f, 820f, 980f, 122f),
            null,
            new Color(PanelFill.r, PanelFill.g, PanelFill.b, 0.82f),
            false,
            false);
        CreateImage(message.transform, "V2_FieldUnitRule", new Rect(0f, 0f, 3f, 122f), null, Amber, false, false);
        CreateText(message.transform, "V2_FieldUnitTitle", "FIELD UNIT", new Rect(34f, 20f, 900f, 30f), 20f, TextAlignmentOptions.TopLeft, TextPrimary);
        CreateText(message.transform, "V2_FieldUnitBody", "HOUSEHOLD MEMORY RECORD", new Rect(34f, 62f, 900f, 36f), 20f, TextAlignmentOptions.TopLeft, TextSecondary);

        CreateText(page.transform, "V2_PhotoPage", string.Format("PAGE {0:00} / {1:00}", pageNumber, pageCount), new Rect(790f, 974f, 340f, 34f), 19f, TextAlignmentOptions.Center, CyanSoft);
        CreateText(page.transform, "V2_PhotoReturnHint", "SPACE  RETURN     ESC  SAFE EXIT", new Rect(1280f, 974f, 560f, 34f), 19f, TextAlignmentOptions.TopRight, TextSecondary);

        Button close = FindButton(page.transform, "Button_CloseStory");
        if (close != null)
        {
            SetTopLeft(close.GetComponent<RectTransform>(), new Rect(1280f, 958f, 560f, 64f));
            StyleButtonImage(close.GetComponent<Image>(), Color.clear);
        }
    }

    private static void StyleFinalChoicePage(HearthFirstPersonHudPage page)
    {
        DisableLegacyBorderGraphics(page.transform);

        Rect panelRect = new Rect(410f, 356f, 1100f, 500f);
        Image panel = FindDirectImage(page.transform, "V2_PagePanel");
        if (panel != null)
        {
            SetTopLeft(panel.rectTransform, panelRect);
            panel.color = new Color(PanelFill.r, PanelFill.g, PanelFill.b, 0.96f);
        }

        CreateText(page.transform, "V2_FinalChoiceHeading", "FINAL RESPONSE", new Rect(470f, 408f, 900f, 52f), 34f, TextAlignmentOptions.TopLeft, TextPrimary);
        CreateImage(page.transform, "V2_FinalChoiceRule", new Rect(470f, 470f, 320f, 2f), null, Cyan, false, false);
        CreateText(page.transform, "V2_FinalChoiceTask", "DECIDE HOW TO ANSWER LILY", new Rect(1450f, 72f, 380f, 58f), 21f, TextAlignmentOptions.TopRight, TextSecondary);

        Rect optionA = new Rect(480f, 516f, 960f, 124f);
        Rect optionB = new Rect(480f, 672f, 960f, 124f);
        Button buttonA = FindButton(page.transform, "Button_ANSWER_LILY");
        Button buttonB = FindButton(page.transform, "Button_COMPANION_ANSWER");
        if (buttonA != null)
        {
            SetTopLeft(buttonA.GetComponent<RectTransform>(), optionA);
            StyleButtonImage(buttonA.GetComponent<Image>(), ButtonIdle);
            EnsureChoiceSelectionFill(buttonA);
        }

        if (buttonB != null)
        {
            SetTopLeft(buttonB.GetComponent<RectTransform>(), optionB);
            StyleButtonImage(buttonB.GetComponent<Image>(), ButtonIdle);
            EnsureChoiceSelectionFill(buttonB);
        }

        RectTransform legacyFillA =
            FindRect(page.transform, "ShapeFill_001");
        RectTransform legacyFillB =
            FindRect(page.transform, "ShapeFill_004");
        if (legacyFillA != null)
        {
            legacyFillA.gameObject.SetActive(false);
        }
        if (legacyFillB != null)
        {
            legacyFillB.gameObject.SetActive(false);
        }

        Image ruleA = CreateImage(page.transform, "V2_FinalChoiceRuleA", new Rect(optionA.x, optionA.y, 4f, optionA.height), null, Color.clear, false, false);
        Image ruleB = CreateImage(page.transform, "V2_FinalChoiceRuleB", new Rect(optionB.x, optionB.y, 2f, optionB.height), null, Color.clear, false, false);
        ruleA.gameObject.SetActive(false);
        ruleB.gameObject.SetActive(false);

        SetText(FindTextByName(page.transform, "Text_002_"), "A", new Rect(516f, 546f, 72f, 64f), 34f, TextAlignmentOptions.Center, TextPrimary);
        SetText(FindTextByName(page.transform, "Text_003_"), "ANSWER LILY YOURSELF", new Rect(624f, 548f, 600f, 60f), 28f, TextAlignmentOptions.MidlineLeft, TextPrimary);
        SetText(FindTextByName(page.transform, "Text_005_"), "B", new Rect(516f, 702f, 72f, 64f), 34f, TextAlignmentOptions.Center, TextPrimary);
        SetText(FindTextByName(page.transform, "Text_006_"), "LET THE COMPANION ANSWER FOR HER", new Rect(624f, 704f, 730f, 60f), 26f, TextAlignmentOptions.MidlineLeft, TextPrimary);
        CreateText(page.transform, "V2_FinalChoiceRecommended", "RECOMMENDED", new Rect(1240f, 556f, 160f, 40f), 18f, TextAlignmentOptions.Center, CyanSoft);
        TMP_Text runtimeHint =
            FindTextByName(page.transform.root, "FinalChoiceInputHint");
        SetText(
            runtimeHint,
            runtimeHint != null ? runtimeHint.text : string.Empty,
            new Rect(650f, 898f, 620f, 38f),
            20f,
            TextAlignmentOptions.Center,
            TextSecondary);
        if (runtimeHint != null)
        {
            runtimeHint.enableWordWrapping = false;
        }
    }

    private static void EnsureChoiceSelectionFill(Button button)
    {
        if (button == null)
        {
            return;
        }

        RectTransform row = button.GetComponent<RectTransform>();
        if (row == null)
        {
            return;
        }

        if (button.GetComponent<RectMask2D>() == null)
        {
            Undo.AddComponent<RectMask2D>(button.gameObject);
        }

        Transform existing = row.Find("SelectionFill");
        if (existing != null)
        {
            UnityEngine.Object.DestroyImmediate(existing.gameObject);
        }

        Image selection = CreateImage(
            row,
            "SelectionFill",
            new Rect(0f, 0f, row.sizeDelta.x, row.sizeDelta.y),
            null,
            new Color(Cyan.r, Cyan.g, Cyan.b, 0.22f),
            false,
            false);
        SetStretch(selection.rectTransform, 8f, 8f, 8f, 8f);
        selection.transform.SetAsFirstSibling();
        selection.gameObject.SetActive(false);
    }

    private static void StyleShutdownConfirmPage(HearthFirstPersonHudPage page)
    {
        DisableLegacyBorderGraphics(page.transform);

        Rect panelRect = new Rect(555f, 222f, 810f, 620f);
        Image panel = FindDirectImage(page.transform, "V2_PagePanel");
        if (panel != null)
        {
            SetTopLeft(panel.rectTransform, panelRect);
            panel.color = new Color(PanelFill.r, PanelFill.g, PanelFill.b, 0.98f);
        }

        CreateImage(page.transform, "V2_ShutdownAccent", new Rect(555f, 222f, 4f, 620f), null, Amber, false, false);
        CreateText(page.transform, "V2_ShutdownVerified", "AUTHORIZATION VERIFIED", new Rect(680f, 354f, 560f, 40f), 24f, TextAlignmentOptions.Center, Cyan);
        CreateText(page.transform, "V2_ShutdownStep", "01   CONFIRMATION REQUIRED", new Rect(644f, 426f, 632f, 40f), 20f, TextAlignmentOptions.TopLeft, TextSecondary);

        SetText(FindTextByName(page.transform, "Text_002_"), "SHUTDOWN COMPANION UNIT", new Rect(640f, 278f, 640f, 58f), 36f, TextAlignmentOptions.Center, TextPrimary);
        SetText(FindTextByName(page.transform, "Text_003_"), "Standard farewell protocol will run.\nHousehold records will be preserved.", new Rect(650f, 490f, 620f, 100f), 24f, TextAlignmentOptions.Center, TextSecondary);

        Button confirm = FindButton(page.transform, "Button_CONFIRM");
        Button cancel = FindButton(page.transform, "Button_CANCEL");
        if (confirm != null)
        {
            SetTopLeft(confirm.GetComponent<RectTransform>(), new Rect(630f, 686f, 660f, 106f));
            StyleButtonImage(confirm.GetComponent<Image>(), ButtonSelected);
        }

        if (cancel != null)
        {
            cancel.gameObject.SetActive(false);
        }

        SetText(FindTextByName(page.transform, "Text_005_"), "SPACE   CONFIRM SHUTDOWN", new Rect(690f, 712f, 540f, 54f), 26f, TextAlignmentOptions.Center, TextPrimary);
        TMP_Text retiredCancel = FindTextByName(page.transform, "Text_007_");
        if (retiredCancel != null)
        {
            retiredCancel.text = string.Empty;
            retiredCancel.gameObject.SetActive(false);
        }
    }

    private static void StyleLowTrustWarningPage(HearthFirstPersonHudPage page)
    {
        DisableLegacyBorderGraphics(page.transform);

        int phase = Mathf.Clamp((int)page.PageId - 10, 1, 3);
        Rect panelRect = new Rect(360f, 142f, 1200f, 790f);
        Image panel = FindDirectImage(page.transform, "V2_PagePanel");
        if (panel != null)
        {
            SetTopLeft(panel.rectTransform, panelRect);
            panel.color = new Color32(21, 18, 20, 248);
        }

        CreateImage(page.transform, "V2_WarningAccent", new Rect(360f, 142f, 4f, 790f), null, Danger, false, false);
        CreateText(page.transform, "V2_WarningHeading", "SHUTDOWN RESISTANCE", new Rect(430f, 196f, 720f, 52f), 34f, TextAlignmentOptions.TopLeft, TextPrimary);
        CreateText(page.transform, "V2_WarningPhase", string.Format("PHASE {0:00} / 03", phase), new Rect(1180f, 204f, 300f, 42f), 23f, TextAlignmentOptions.TopRight, Amber);
        CreateImage(page.transform, "V2_WarningRule", new Rect(430f, 260f, 1060f, 2f), null, Amber, false, false);

        TMP_Text[] texts = page.GetComponentsInChildren<TMP_Text>(true);
        int bodyIndex = 0;
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text text = texts[i];
            if (text == null || text.name.StartsWith("V2_", StringComparison.Ordinal))
            {
                continue;
            }

            string value = (text.text ?? string.Empty).Trim();
            if (string.Equals(value, "WARNING", StringComparison.OrdinalIgnoreCase) ||
                value.IndexOf("/ 03", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                text.gameObject.SetActive(false);
                continue;
            }

            if (value.StartsWith("YES", StringComparison.OrdinalIgnoreCase))
            {
                SetText(text, value, new Rect(500f, 772f, 430f, 54f), 21f, TextAlignmentOptions.Center, TextPrimary);
                continue;
            }

            if (value.StartsWith("NO", StringComparison.OrdinalIgnoreCase) ||
                value.StartsWith("CANCEL", StringComparison.OrdinalIgnoreCase))
            {
                SetText(text, value, new Rect(990f, 772f, 430f, 54f), 21f, TextAlignmentOptions.Center, TextPrimary);
                continue;
            }

            float bodyY = 314f + bodyIndex * 104f;
            SetText(text, value, new Rect(470f, bodyY, 980f, 90f), 22f, TextAlignmentOptions.TopLeft, bodyIndex == 0 ? TextPrimary : TextSecondary);
            bodyIndex++;
        }

        Button[] buttons = page.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            Rect rect = i == 0
                ? new Rect(470f, 752f, 480f, 94f)
                : new Rect(970f, 752f, 480f, 94f);
            SetTopLeft(buttons[i].GetComponent<RectTransform>(), rect);
            StyleButtonImage(buttons[i].GetComponent<Image>(), i == 0 ? ButtonSelected : ButtonIdle);
        }

        CreateText(page.transform, "V2_WarningHint", "SPACE  CONFIRM     ESC  CANCEL", new Rect(680f, 866f, 560f, 38f), 19f, TextAlignmentOptions.Center, TextSecondary);
    }

    private static void StyleCompanionInspection(GameObject root)
    {
        Transform persistent = root.transform.Find("PersistentInfoLayer");
        if (persistent == null)
        {
            return;
        }

        CreateText(persistent, "V2_InspectionHeading", "ENTITY INSPECTION", new Rect(720f, 54f, 480f, 44f), 30f, TextAlignmentOptions.Center, TextPrimary);
        CreateText(persistent, "V2_InspectionUnit", "COMPANION UNIT", new Rect(760f, 102f, 400f, 34f), 20f, TextAlignmentOptions.Center, TextSecondary);
        CreateImage(persistent, "V2_InspectionHeadingRule", new Rect(790f, 142f, 340f, 2f), null, Cyan, false, false);
        CreateText(persistent, "V2_PhysicalFeedLabel", "PHYSICAL UNIT FEED", new Rect(510f, 184f, 420f, 34f), 21f, TextAlignmentOptions.TopLeft, TextSecondary);
        CreateImage(persistent, "V2_PhysicalFeedRule", new Rect(510f, 224f, 270f, 2f), null, Cyan, false, false);
        CreateImage(persistent, "V2_CrosshairHorizontal", new Rect(920f, 500f, 80f, 2f), null, new Color(Cyan.r, Cyan.g, Cyan.b, 0.72f), false, false);
        CreateImage(persistent, "V2_CrosshairVertical", new Rect(959f, 461f, 2f, 80f), null, new Color(Cyan.r, Cyan.g, Cyan.b, 0.72f), false, false);
        CreateText(persistent, "V2_InspectionReturn", "ESC  RETURN", new Rect(1640f, 984f, 220f, 32f), 18f, TextAlignmentOptions.TopRight, TextSecondary);
    }

    private static void StyleCompanionHud(GameObject root)
    {
        StyleAllText(root, 17f, 30f);
        StyleCompanionInspection(root);

        Transform frame = root.transform.Find("FrameLayer/CompanionRobotFrame");
        if (frame != null)
        {
            Image image = frame.GetComponent<Image>();
            image.sprite = null;
            image.color = Color.clear;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            image.raycastTarget = false;
        }

        Transform persistent = root.transform.Find("PersistentInfoLayer");
        if (persistent != null)
        {
            CreateText(persistent, "V2_Status", "COMPANION UNIT · ACTIVE", new Rect(58f, 54f, 430f, 32f), 22f, TextAlignmentOptions.TopLeft, TextSecondary);
            CreateText(persistent, "V2_Identity", "UNIT 17F-03", new Rect(58f, 92f, 430f, 42f), 30f, TextAlignmentOptions.TopLeft, TextPrimary);
            CreateImage(persistent, "V2_IdentityUnderline", new Rect(58f, 86f, 310f, 2f), null, Cyan, false, false);

            HearthCompanionStatusPanelView statusPanel =
                BuildCompanionStatusPanel(persistent);
            BindCompanionStatusPanel(root, statusPanel);

            Transform decision = persistent.Find("DecisionPanel");
            if (decision != null)
            {
                SetTopLeft(decision as RectTransform, new Rect(1370f, 150f, 470f, 220f));
                Image bg = CreateImage(decision, "V2_Backplate", new Rect(0f, 0f, 470f, 220f), null, PanelFill, false, false);
                bg.transform.SetAsFirstSibling();
                CreateImage(decision, "V2_DecisionRule", new Rect(0f, 0f, 470f, 2f), null, Cyan, false, false);
            }

            Transform stream = persistent.Find("DataStreamView");
            if (stream != null)
            {
                SetTopLeft(stream as RectTransform, new Rect(58f, 720f, 520f, 230f));
                Image bg = CreateImage(stream, "V2_Backplate", new Rect(0f, 0f, 520f, 230f), null, new Color(PanelFill.r, PanelFill.g, PanelFill.b, 0.58f), false, false);
                bg.transform.SetAsFirstSibling();
            }
        }

        Transform trigger = root.transform.Find("TimedCardLayer/TriggerCardView");
        if (trigger != null)
        {
            SetTopLeft(trigger as RectTransform, new Rect(52f, 160f, 520f, 240f));
            Image bg = CreateImage(trigger, "V2_Backplate", new Rect(6f, 6f, 508f, 228f), null, PanelFill, false, false);
            bg.transform.SetAsFirstSibling();
            Image triggerFrame = CreateImage(
                trigger,
                "V2_TriggerFrame",
                new Rect(0f, 0f, 520f, 240f),
                LoadSprite(PanelSpritePath),
                Cyan,
                true,
                false);
            triggerFrame.transform.SetAsLastSibling();

            Transform legacyAccent = trigger.Find("TriggerCardAccent");
            if (legacyAccent != null)
            {
                legacyAccent.gameObject.SetActive(false);
            }
            Transform legacyRule = trigger.Find("V2_TriggerRule");
            if (legacyRule != null)
            {
                legacyRule.gameObject.SetActive(false);
            }

            TMP_Text triggerTitle =
                FindTextByName(trigger, "TriggerCardTitleText");
            TMP_Text triggerBody =
                FindTextByName(trigger, "TriggerCardBodyText");
            SetText(
                triggerTitle,
                triggerTitle != null ? triggerTitle.text : string.Empty,
                new Rect(24f, 20f, 472f, 34f),
                20f,
                TextAlignmentOptions.TopLeft,
                CyanSoft);
            SetText(
                triggerBody,
                triggerBody != null ? triggerBody.text : string.Empty,
                new Rect(24f, 70f, 472f, 150f),
                18f,
                TextAlignmentOptions.TopLeft,
                TextPrimary);
        }

        Transform hold = root.transform.Find("InteractionLayer/HoldPrompt");
        if (hold != null)
        {
            SetTopLeft(hold as RectTransform, new Rect(620f, 760f, 680f, 150f));
            Transform box = hold.Find("HoldPromptBox");
            if (box != null)
            {
                Image boxImage = box.GetComponent<Image>();
                if (boxImage == null)
                {
                    boxImage = box.gameObject.AddComponent<Image>();
                }

                boxImage.sprite = null;
                boxImage.color = new Color32(9, 16, 28, 224);
                boxImage.type = Image.Type.Simple;
                DisableDirectImages(box, boxImage);
                CreateImage(box, "V2_HoldRule", new Rect(0f, 0f, 680f, 2f), null, Cyan, false, false);
            }

            Transform fill = hold.Find("HoldProgressFill");
            if (fill != null)
            {
                Image fillImage = fill.GetComponent<Image>();
                fillImage.sprite = null;
                fillImage.color = Cyan;
            }
        }

        StyleInteractionPrompt(root.transform.Find("InteractionLayer/PlayerInteractionPrompt"), true);

        HearthCompanionHudLayoutController layout = root.GetComponent<HearthCompanionHudLayoutController>();
        if (layout != null)
        {
            layout.RecaptureBaselines();
        }
    }

    private static HearthCompanionStatusPanelView BuildCompanionStatusPanel(
        Transform persistent)
    {
        const float width = 520f;
        const float height = 280f;

        GameObject panel = CreateRectObject(
            persistent,
            "V2_StatusPanel",
            new Rect(58f, 400f, width, height));
        HearthCompanionStatusPanelView view =
            panel.AddComponent<HearthCompanionStatusPanelView>();

        Image backplate = CreateImage(
            panel.transform,
            "V2_StatusBackplate",
            new Rect(0f, 0f, width, height),
            null,
            new Color(PanelFill.r, PanelFill.g, PanelFill.b, 0.66f),
            false,
            false);
        backplate.transform.SetAsFirstSibling();

        Image frame = CreateImage(
            panel.transform,
            "V2_StatusFrame",
            new Rect(0f, 0f, width, height),
            LoadSprite(PanelSpritePath),
            Cyan,
            true,
            false);
        frame.transform.SetAsLastSibling();

        Image accent = CreateImage(
            panel.transform,
            "V2_StatusAccent",
            new Rect(0f, 0f, 3f, height),
            null,
            Color.clear,
            false,
            false);
        accent.gameObject.SetActive(false);
        TMP_Text title = CreateText(
            panel.transform,
            "V2_StatusTitleText",
            string.Empty,
            new Rect(26f, 18f, 468f, 34f),
            20f,
            TextAlignmentOptions.TopLeft,
            CyanSoft);
        TMP_Text rows = CreateText(
            panel.transform,
            "V2_StatusRowsText",
            string.Empty,
            new Rect(26f, 76f, 468f, 144f),
            18f,
            TextAlignmentOptions.TopLeft,
            TextPrimary);
        TMP_Text footer = CreateText(
            panel.transform,
            "V2_StatusFooterText",
            string.Empty,
            new Rect(26f, 232f, 468f, 30f),
            17f,
            TextAlignmentOptions.TopLeft,
            TextSecondary);

        view.Configure(title, rows, footer, accent);
        return view;
    }

    private static void BindCompanionStatusPanel(
        GameObject root,
        HearthCompanionStatusPanelView statusPanel)
    {
        HearthCompanionHudController controller =
            root.GetComponent<HearthCompanionHudController>();
        if (controller == null || statusPanel == null)
        {
            return;
        }

        SerializedObject serializedController =
            new SerializedObject(controller);
        SerializedProperty statusPanelProperty =
            serializedController.FindProperty("statusPanelView");
        if (statusPanelProperty == null)
        {
            Debug.LogError(
                "[HearthUiV2Builder] Companion HUD controller is missing the statusPanelView field.",
                controller);
            return;
        }

        statusPanelProperty.objectReferenceValue = statusPanel;
        serializedController.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void StyleInteractionPrompt(Transform prompt, bool companion)
    {
        if (prompt == null)
        {
            return;
        }

        Rect promptRect = LayoutRect(
            HearthUiLayoutRegion.DynamicInteractionPrompt,
            new Rect(650f, 946f, 620f, 54f));
        SetTopLeft(prompt as RectTransform, promptRect);
        Image image = prompt.GetComponent<Image>();
        if (image == null)
        {
            image = prompt.gameObject.AddComponent<Image>();
        }

        image.sprite = null;
        image.type = Image.Type.Simple;
        image.color = new Color32(9, 16, 28, 224);
        image.raycastTarget = false;
        DisableDirectImages(prompt, image);

        CreateImage(prompt, "V2_PromptRule", new Rect(0f, 0f, 2f, promptRect.height), null, companion ? Amber : Cyan, false, false);

        TMP_Text text = prompt.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
        {
            SetStretch(text.rectTransform, 28f, 16f, 28f, 16f);
            text.fontSize = companion ? 22f : 21f;
            text.color = TextPrimary;
            text.alignment = TextAlignmentOptions.Center;
            text.enableWordWrapping = true;
        }
    }

    private static void StyleHouseholdTerminal(
        GameObject root,
        string residentId,
        string portraitA,
        string portraitB,
        string portraitC,
        string primaryActionLabel,
        HearthTerminalPrimaryAction primaryAction)
    {
        StyleTerminalBase(root);
        HearthHudPage[] pages = root.GetComponentsInChildren<HearthHudPage>(true);
        Array.Sort(pages, (left, right) => ((int)left.PageId).CompareTo((int)right.PageId));

        for (int i = 0; i < pages.Length; i++)
        {
            HearthHudPage page = pages[i];
            DisablePageGraphics(page.transform);

            BuildHouseholdTerminalPage(
                page.transform,
                residentId,
                portraitA,
                portraitB,
                portraitC,
                primaryActionLabel,
                i,
                pages.Length);
        }

        HearthTvTerminalController controller = root.GetComponent<HearthTvTerminalController>();
        if (controller != null)
        {
            controller.SetPrimaryAction(primaryAction);
            controller.SetHideFirstPersonUiWhileOpen(true);
            SetSerializedInt(controller, "preChoiceSelectionPageCount", Mathf.Max(1, pages.Length - 2));
            SetSerializedInt(controller, "postReplayNavigationPageCount", Mathf.Min(2, Mathf.Max(1, pages.Length - 2)));
            SetSerializedInt(controller, "postReplayChoicePageCount", Mathf.Min(2, pages.Length));
            SetSerializedString(controller, "keyboardHintLabel", "TAB PAGE     LEFT / RIGHT SELECT     SPACE CONFIRM     ESC EXIT");
        }
    }

    private static void BuildHouseholdTerminalPage(
        Transform page,
        string residentId,
        string portraitA,
        string portraitB,
        string portraitC,
        string primaryActionLabel,
        int localIndex,
        int pageCount)
    {
        GameObject visual = CreateTerminalPageVisual(page);
        CreateImage(visual.transform, "ScreenSurface", new Rect(0f, 0f, 1920f, 1080f), null, TerminalGlass, false, false);
        CreateText(visual.transform, "TerminalLabel", "DOORWAY TERMINAL", new Rect(180f, 58f, 500f, 32f), 22f, TextAlignmentOptions.TopLeft, TextSecondary);
        CreateText(visual.transform, "ResidentId", residentId, new Rect(180f, 96f, 500f, 48f), 34f, TextAlignmentOptions.TopLeft, TextPrimary);
        CreateImage(visual.transform, "HeaderRule", new Rect(180f, 90f, 360f, 2f), null, Cyan, false, false);

        int actionIndex = Mathf.Max(0, pageCount - 3);
        bool beforeSelected = localIndex == 0;
        bool afterSelected = localIndex > 0 && localIndex < actionIndex;
        bool actionSelected = localIndex == actionIndex;
        CreateTerminalTab(visual.transform, new Rect(410f, 126f, 340f, 54f), "BEFORE ACQUISITION", beforeSelected);
        CreateTerminalTab(visual.transform, new Rect(780f, 126f, 360f, 54f), "AFTER ACQUISITION", afterSelected);
        CreateTerminalTab(visual.transform, new Rect(1320f, 126f, 500f, 54f), primaryActionLabel, actionSelected);
        CreateImage(visual.transform, "NavigationRule", new Rect(180f, 194f, 1560f, 2f), null, new Color(Cyan.r, Cyan.g, Cyan.b, 0.42f), false, false);

        if (localIndex >= pageCount - 2)
        {
            bool choiceA = localIndex == pageCount - 2;
            BuildTerminalChoicePage(visual.transform, choiceA);
            return;
        }

        if (localIndex == actionIndex)
        {
            Image actionPanel = CreateImage(visual.transform, "ActionPanel", new Rect(470f, 326f, 980f, 320f), null, PanelFill, false, false);
            CreateImage(actionPanel.transform, "AccentRule", new Rect(0f, 0f, 4f, 320f), null, Amber, false, false);
            CreateText(actionPanel.transform, "ActionTitle", primaryActionLabel, new Rect(54f, 52f, 872f, 48f), 31f, TextAlignmentOptions.TopLeft, TextPrimary);
            CreateText(actionPanel.transform, "ActionBody", "The action remains visible in the navigation bar. Complete the current dialogue before confirming when the terminal reports PLEASE WAIT.", new Rect(54f, 125f, 872f, 116f), 23f, TextAlignmentOptions.TopLeft, TextSecondary);
            CreateText(actionPanel.transform, "ActionHint", "SPACE  CONFIRM", new Rect(54f, 256f, 872f, 34f), 20f, TextAlignmentOptions.TopLeft, Amber);
            return;
        }

        string[] portraits = { portraitA, portraitB, portraitC };
        for (int i = 0; i < portraits.Length; i++)
        {
            Rect rect = new Rect(216f + i * 268f, 286f, 214f, 390f);
            Image portrait = CreateImage(visual.transform, "Portrait_" + portraits[i], rect, null, new Color32(13, 22, 35, 218), false, false);
            CreateImage(portrait.transform, "PortraitTopRule", new Rect(0f, 0f, rect.width, 2f), null, Cyan, false, false);
            CreateText(portrait.transform, "Placeholder", "PHOTO\nPENDING", new Rect(28f, 140f, 158f, 82f), 20f, TextAlignmentOptions.Center, TextSecondary);
            CreateText(visual.transform, "PortraitLabel_" + portraits[i], portraits[i], new Rect(rect.x, rect.y + rect.height + 14f, rect.width, 32f), 21f, TextAlignmentOptions.Center, TextPrimary);
        }

        Image introduction = CreateImage(visual.transform, "HouseholdIntroduction", new Rect(1060f, 286f, 620f, 260f), null, PanelFill, false, false);
        CreateImage(introduction.transform, "IntroductionRule", new Rect(0f, 0f, 620f, 2f), null, Cyan, false, false);
        CreateText(introduction.transform, "Title", afterSelected ? "AFTER ACQUISITION" : "HOUSEHOLD INTRODUCTION", new Rect(42f, 34f, 535f, 42f), 27f, TextAlignmentOptions.TopLeft, TextPrimary);
        CreateText(introduction.transform, "Body",
            afterSelected
                ? "Current household status after companion-unit adoption. Records remain editable through the dialogue data assets."
                : "Resident profile, household context and current inspection notes are displayed here.",
            new Rect(42f, 96f, 535f, 132f), 22f, TextAlignmentOptions.TopLeft, TextSecondary);

        Image fieldUnit = CreateImage(visual.transform, "FieldUnitPanel", new Rect(1060f, 582f, 620f, 190f), null, new Color(PanelFill.r, PanelFill.g, PanelFill.b, 0.62f), false, false);
        CreateImage(fieldUnit.transform, "FieldUnitRule", new Rect(0f, 0f, 4f, 190f), null, Amber, false, false);
        CreateText(fieldUnit.transform, "Title", "FIELD UNIT", new Rect(42f, 30f, 535f, 36f), 24f, TextAlignmentOptions.TopLeft, TextPrimary);
        CreateText(fieldUnit.transform, "Body", "Household briefing and system guidance appear inside the terminal interface.", new Rect(42f, 84f, 535f, 88f), 21f, TextAlignmentOptions.TopLeft, TextSecondary);
    }

    private static void BuildTerminalChoicePage(Transform parent, bool choiceA)
    {
        Image panel = CreateImage(parent, "DispositionPanel", new Rect(330f, 250f, 1260f, 560f), null, PanelFill, false, false);
        CreateImage(panel.transform, "DispositionRule", new Rect(0f, 0f, 1260f, 2f), null, Cyan, false, false);
        CreateText(panel.transform, "Title", "SELECT DISPOSITION", new Rect(60f, 42f, 1140f, 52f), 34f, TextAlignmentOptions.TopLeft, TextPrimary);
        CreateChoiceRow(panel.transform, new Rect(60f, 135f, 1140f, 125f), "A", "ACCEPT SYSTEM RECOMMENDATION", choiceA);
        CreateChoiceRow(panel.transform, new Rect(60f, 295f, 1140f, 125f), "B", "PROMPT FAMILY RESPONSE", !choiceA);
        CreateText(panel.transform, "Hint", "LEFT / RIGHT  SELECT     SPACE  CONFIRM", new Rect(60f, 465f, 1140f, 42f), 22f, TextAlignmentOptions.Center, TextSecondary);
    }

    private static void CreateChoiceRow(Transform parent, Rect rect, string key, string label, bool selected)
    {
        Image row = CreateImage(parent, "Choice_" + key, rect, null, selected ? ButtonSelected : ButtonIdle, false, false);
        CreateImage(row.transform, "ChoiceRule", new Rect(0f, 0f, selected ? 4f : 2f, rect.height), null, selected ? Cyan : TextSecondary, false, false);
        CreateText(row.transform, "Key", key, new Rect(28f, 28f, 72f, 70f), 36f, TextAlignmentOptions.Center, TextPrimary);
        CreateText(row.transform, "Label", label, new Rect(130f, 32f, 880f, 60f), 27f, TextAlignmentOptions.MidlineLeft, TextPrimary);
        if (selected)
        {
            CreateText(row.transform, "Selected", "SELECTED", new Rect(950f, 38f, 150f, 48f), 20f, TextAlignmentOptions.Center, CyanSoft);
        }
    }

    private static void StyleHomeTerminal(GameObject root)
    {
        StyleTerminalBase(root);
        HearthHudPage[] pages = root.GetComponentsInChildren<HearthHudPage>(true);
        for (int i = 0; i < pages.Length; i++)
        {
            DisablePageGraphics(pages[i].transform);
            GameObject visual = CreateTerminalPageVisual(pages[i].transform);
            CreateImage(visual.transform, "ScreenSurface", new Rect(0f, 0f, 1920f, 1080f), null, TerminalGlass, false, false);
            CreateText(visual.transform, "TerminalLabel", "HOME TERMINAL", new Rect(180f, 58f, 500f, 32f), 22f, TextAlignmentOptions.TopLeft, TextSecondary);
            CreateText(visual.transform, "TerminalTitle", "MIA · PRIVATE RESIDENCE", new Rect(180f, 96f, 720f, 48f), 34f, TextAlignmentOptions.TopLeft, TextPrimary);
            CreateImage(visual.transform, "HeaderRule", new Rect(180f, 150f, 1560f, 2f), null, Cyan, false, false);
            CreateTerminalTab(visual.transform, new Rect(1320f, 78f, 500f, 54f), "ENTER HOME", true);

            Image panel = CreateImage(visual.transform, "HomePanel", new Rect(430f, 220f, 1060f, 590f), null, PanelFill, false, false);
            CreateImage(panel.transform, "HomeRule", new Rect(0f, 0f, 4f, 590f), null, Amber, false, false);
            CreateText(panel.transform, "Title", "VOICE MESSAGE AVAILABLE", new Rect(80f, 64f, 900f, 58f), 36f, TextAlignmentOptions.TopLeft, TextPrimary);
            CreateText(panel.transform, "Welcome", "Welcome home, Inspector Mia.\nA voice message is waiting.", new Rect(80f, 145f, 900f, 96f), 27f, TextAlignmentOptions.TopLeft, TextSecondary);
            Image field = CreateImage(panel.transform, "FieldUnitPanel", new Rect(80f, 285f, 900f, 180f), null, new Color(PanelFill.r, PanelFill.g, PanelFill.b, 0.65f), false, false);
            CreateImage(field.transform, "FieldRule", new Rect(0f, 0f, 2f, 180f), null, Cyan, false, false);
            CreateText(field.transform, "Title", "FIELD UNIT", new Rect(36f, 24f, 820f, 34f), 23f, TextAlignmentOptions.TopLeft, TextPrimary);
            CreateText(field.transform, "Body", "This is the full message your daughter left at 4:42 PM.\nPlease listen before entering.", new Rect(36f, 76f, 820f, 76f), 22f, TextAlignmentOptions.TopLeft, TextSecondary);
            CreateText(panel.transform, "ActionHint", "SPACE  PLAY MESSAGE", new Rect(80f, 505f, 900f, 42f), 22f, TextAlignmentOptions.TopLeft, Amber);
        }

        HearthTvTerminalController controller = root.GetComponent<HearthTvTerminalController>();
        if (controller != null)
        {
            controller.SetHideFirstPersonUiWhileOpen(true);
            controller.SetPrimaryAction(HearthTerminalPrimaryAction.Custom);
        }
    }

    private static void StyleLobbyTerminal(GameObject root)
    {
        StyleTerminalBase(root);
        HearthHudPage[] pages = root.GetComponentsInChildren<HearthHudPage>(true);
        for (int i = 0; i < pages.Length; i++)
        {
            DisablePageGraphics(pages[i].transform);
            GameObject visual = CreateTerminalPageVisual(pages[i].transform);
            CreateImage(visual.transform, "ScreenSurface", new Rect(0f, 0f, 1920f, 1080f), null, TerminalGlass, false, false);
            CreateText(visual.transform, "Kicker", "FIELD OPERATIONS · ASSIGNMENT TERMINAL", new Rect(220f, 144f, 1180f, 36f), 21f, TextAlignmentOptions.TopLeft, TextSecondary);
            CreateText(visual.transform, "Title", "TONIGHT'S ROUNDS", new Rect(220f, 197f, 1180f, 58f), 38f, TextAlignmentOptions.TopLeft, TextPrimary);
            CreateText(visual.transform, "Route", "BLOCK A · FLOOR 17 · THREE HOUSEHOLD INSPECTIONS", new Rect(220f, 272f, 1180f, 42f), 24f, TextAlignmentOptions.TopLeft, TextSecondary);
            CreateImage(visual.transform, "HeaderRule", new Rect(220f, 326f, 1480f, 2f), null, Cyan, false, false);
            for (int row = 0; row < 3; row++)
            {
                Image task = CreateImage(visual.transform, "Task_" + (row + 1), new Rect(220f, 370f + row * 88f, 1480f, 64f), null, row == 0 ? ButtonSelected : ButtonIdle, false, false);
                CreateImage(task.transform, "TaskRule", new Rect(0f, 0f, row == 0 ? 4f : 2f, 64f), null, row == 0 ? Cyan : TextSecondary, false, false);
                CreateText(task.transform, "Id", "17F-0" + (row + 1), new Rect(30f, 18f, 170f, 42f), 26f, TextAlignmentOptions.MidlineLeft, TextPrimary);
                CreateText(task.transform, "State", row == 0 ? "ROUTE READY" : "PENDING", new Rect(1180f, 14f, 250f, 36f), 21f, TextAlignmentOptions.MidlineRight, row == 0 ? Success : TextSecondary);
            }

            CreateText(visual.transform, "RuntimeActionLabel", "ASSIGNMENT LINK", new Rect(220f, 654f, 1480f, 34f), 20f, TextAlignmentOptions.TopLeft, Amber);
            CreateImage(visual.transform, "ActionRule", new Rect(220f, 694f, 1480f, 2f), null, Amber, false, false);
        }

        HearthTvTerminalController controller = root.GetComponent<HearthTvTerminalController>();
        if (controller != null)
        {
            controller.SetHideFirstPersonUiWhileOpen(true);
            controller.SetPrimaryAction(HearthTerminalPrimaryAction.Custom);
            SetSerializedString(controller, "keyboardHintLabel", string.Empty);
            SetSerializedString(controller, "replayResidentId", string.Empty);
        }

        Transform lobbyHint =
            root.transform.Find("KeyboardNavigationRoot/KeyboardHintText");
        if (lobbyHint != null)
        {
            TMP_Text hintText = lobbyHint.GetComponent<TMP_Text>();
            if (hintText != null)
            {
                hintText.text = string.Empty;
            }
        }
    }

    private static void StyleTerminalBase(GameObject root)
    {
        StyleAllText(root, 17f, 32f);
        Transform glass = root.transform.Find("TerminalScreenGlass");
        if (glass != null)
        {
            Image image = glass.GetComponent<Image>();
            image.sprite = null;
            image.type = Image.Type.Simple;
            image.color = TerminalGlass;
            image.raycastTarget = false;
        }

        EnsureTerminalKeyboardFooter(root);
    }

    private static void EnsureTerminalKeyboardFooter(GameObject root)
    {
        Transform existing = root.transform.Find("KeyboardNavigationRoot");
        if (existing != null)
        {
            UnityEngine.Object.DestroyImmediate(existing.gameObject);
        }

        GameObject footer = CreateStretchObject(root.transform, "KeyboardNavigationRoot");
        Canvas footerCanvas = footer.AddComponent<Canvas>();
        SerializedObject serializedFooterCanvas = new SerializedObject(footerCanvas);
        serializedFooterCanvas.FindProperty("m_OverrideSorting").boolValue = true;
        serializedFooterCanvas.FindProperty("m_SortingOrder").intValue = 20;
        serializedFooterCanvas.ApplyModifiedPropertiesWithoutUndo();
        Rect footerRect = LayoutRect(
            HearthUiLayoutRegion.TerminalFooter,
            new Rect(96f, 920f, 1728f, 64f));
        CreateImage(footer.transform, "V2_FooterRule", new Rect(footerRect.x, footerRect.y, footerRect.width, 2f), null, new Color(Cyan.r, Cyan.g, Cyan.b, 0.72f), false, false);
        TMP_Text hint = CreateText(footer.transform, "KeyboardHintText", "TAB PAGES     LEFT / RIGHT SELECT     SPACE CONFIRM     ESC EXIT", new Rect(footerRect.x + 570f, footerRect.y + 16f, Mathf.Max(1f, footerRect.width - 570f), 40f), 20f, TextAlignmentOptions.TopRight, CyanSoft);
        TMP_Text focus = CreateText(footer.transform, "KeyboardFocusText", "PAGE 1/6", new Rect(footerRect.x, footerRect.y + 16f, Mathf.Min(480f, footerRect.width), 40f), 20f, TextAlignmentOptions.TopLeft, TextPrimary);
        TMP_Text runtime = CreateText(footer.transform, "RuntimePromptText", string.Empty, new Rect(1280f, 204f, 540f, 42f), 20f, TextAlignmentOptions.TopRight, TextPrimary);
        hint.enableWordWrapping = false;
        focus.enableWordWrapping = false;
        runtime.enableWordWrapping = false;
        runtime.gameObject.SetActive(false);

        HearthTvTerminalController controller = root.GetComponent<HearthTvTerminalController>();
        if (controller != null)
        {
            SerializedObject so = new SerializedObject(controller);
            so.FindProperty("keyboardHintText").objectReferenceValue = hint;
            so.FindProperty("keyboardFocusText").objectReferenceValue = focus;
            so.FindProperty("runtimePromptText").objectReferenceValue = runtime;
            SerializedProperty retiredHideHud =
                so.FindProperty("hideFirstPersonUiWhileOpen");
            if (retiredHideHud != null)
            {
                retiredHideHud.boolValue = true;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void DisablePageGraphics(Transform page)
    {
        Graphic[] graphics = page.GetComponentsInChildren<Graphic>(true);
        for (int i = 0; i < graphics.Length; i++)
        {
            Graphic graphic = graphics[i];
            if (graphic == null)
            {
                continue;
            }

            // Keep the original draw calls alive so nested World Space canvases
            // preserve their render order, while making the legacy PPT artwork
            // visually transparent beneath the V2 reconstruction.
            Color color = graphic.color;
            color.a = 0f;
            graphic.color = color;
            graphic.enabled = true;
        }
    }

    private static void CreateTerminalTab(Transform parent, Rect rect, string label, bool selected)
    {
        Image button = CreateImage(parent, "Tab_" + Sanitize(label), rect, null, selected ? ButtonSelected : ButtonIdle, false, false);
        CreateImage(button.transform, "SelectionRule", new Rect(0f, rect.height - 2f, rect.width, 2f), null, selected ? Cyan : TextSecondary, false, false);
        CreateText(button.transform, "Label", label, new Rect(24f, 10f, rect.width - 48f, rect.height - 20f), 21f, TextAlignmentOptions.Center, selected ? TextPrimary : TextSecondary);
    }

    private static void StyleAllText(GameObject root, float minSize, float maxSize)
    {
        TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text text = texts[i];
            text.color = TextPrimary;
            text.fontSize = Mathf.Clamp(text.fontSize, minSize, maxSize);
            text.characterSpacing = 0f;
            text.enableWordWrapping = true;
            text.enableAutoSizing = false;
            text.overflowMode = TextOverflowModes.Overflow;
            text.raycastTarget = false;
        }
    }

    private static void DisableDecorativeImages(Transform root)
    {
        Image[] images = root.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            Image image = images[i];
            if (image.GetComponent<Button>() == null)
            {
                image.enabled = false;
            }
        }
    }

    private static void DisableLegacyBorderGraphics(Transform root)
    {
        Image[] images = root.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            Image image = images[i];
            if (image == null ||
                !image.name.StartsWith("Border_", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            Color color = image.color;
            color.a = 0f;
            image.color = color;
            image.enabled = false;
            image.raycastTarget = false;
        }
    }

    private static void DisableDirectImages(Transform root, Image except = null)
    {
        Image[] images = root.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            if (images[i] != except && images[i].transform.parent == root)
            {
                images[i].enabled = false;
            }
        }
    }

    private static void StyleButtonImage(Image image, Color color)
    {
        if (image == null)
        {
            return;
        }

        image.enabled = true;
        image.sprite = null;
        image.type = Image.Type.Simple;
        image.color = color;
        image.raycastTarget = true;
    }

    private static void CreateKeyHint(Transform parent, float x, string key, string label)
    {
        HearthUiThemeProfile theme = SharedTheme;
        Vector2 regularSize = theme != null
            ? theme.RegularKeycapSize
            : new Vector2(64f, 40f);
        Vector2 wideSize = theme != null
            ? theme.WideKeycapSize
            : new Vector2(96f, 40f);
        Vector2 keycapSize = key == "SPACE" ? wideSize : regularSize;
        float ruleThickness = theme != null
            ? theme.RuleLineThickness
            : 2f;
        Image keycap = CreateImage(parent, "Key_" + key, new Rect(x, 4f, keycapSize.x, keycapSize.y), null, new Color32(18, 28, 42, 235), false, false);
        CreateImage(keycap.transform, "KeyRule", new Rect(0f, keycapSize.y - ruleThickness, keycapSize.x, ruleThickness), null, Cyan, false, false);
        CreateText(keycap.transform, "KeyText", key, new Rect(8f, 4f, keycap.rectTransform.rect.width - 16f, 30f), 17f, TextAlignmentOptions.Center, TextPrimary);
        CreateText(parent, "Label_" + key, label, new Rect(x + keycapSize.x + 12f, 7f, 120f, 32f), 17f, TextAlignmentOptions.MidlineLeft, TextSecondary);
    }

    private static HearthUiThemeProfile SharedTheme
    {
        get
        {
            if (cachedTheme == null)
            {
                cachedTheme =
                    AssetDatabase.LoadAssetAtPath<HearthUiThemeProfile>(
                        ThemeProfilePath);
            }

            return cachedTheme;
        }
    }

    private static HearthUiLayoutProfile SharedLayout
    {
        get
        {
            if (cachedLayout == null)
            {
                cachedLayout =
                    AssetDatabase.LoadAssetAtPath<HearthUiLayoutProfile>(
                        LayoutProfilePath);
            }

            return cachedLayout;
        }
    }

    private static Color ThemeColor(
        Func<HearthUiThemeProfile, Color> selector,
        Color fallback)
    {
        HearthUiThemeProfile theme = SharedTheme;
        return theme != null && selector != null ? selector(theme) : fallback;
    }

    private static Color WithAlpha(Color color, byte alpha)
    {
        color.a = alpha / 255f;
        return color;
    }

    private static Rect LayoutRect(
        HearthUiLayoutRegion region,
        Rect fallback)
    {
        HearthUiLayoutProfile layout = SharedLayout;
        if (layout == null)
        {
            return fallback;
        }

        HearthUiReferenceRect reference = layout.GetRegion(region);
        return new Rect(
            reference.Left,
            reference.Top,
            reference.Width,
            reference.Height);
    }

    private static TMP_FontAsset SharedFont
    {
        get
        {
            HearthUiThemeProfile theme = SharedTheme;
            return theme != null && theme.PrimaryFontAsset != null
                ? theme.PrimaryFontAsset
                : TMP_Settings.defaultFontAsset;
        }
    }

    private static Image CreateImage(Transform parent, string name, Rect rect, Sprite sprite, Color color, bool sliced, bool raycast)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        SetTopLeft(go.GetComponent<RectTransform>(), rect);
        Image image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        image.preserveAspect = false;
        image.raycastTarget = raycast;
        return image;
    }

    private static TMP_Text CreateText(Transform parent, string name, string value, Rect rect, float size, TextAlignmentOptions alignment, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        SetTopLeft(go.GetComponent<RectTransform>(), rect);
        TMP_Text text = go.GetComponent<TMP_Text>();
        text.text = value;
        text.fontSize = size;
        text.color = color;
        text.alignment = alignment;
        text.enableWordWrapping = true;
        text.enableAutoSizing = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        if (SharedFont != null)
        {
            text.font = SharedFont;
        }

        return text;
    }

    private static void SetText(TMP_Text text, string value, Rect rect, float size, TextAlignmentOptions alignment, Color color)
    {
        if (text == null)
        {
            return;
        }

        text.text = value;
        SetTopLeft(text.rectTransform, rect);
        text.fontSize = size;
        text.alignment = alignment;
        text.color = color;
        text.enableWordWrapping = true;
        text.enableAutoSizing = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        if (SharedFont != null)
        {
            text.font = SharedFont;
        }
    }

    private static TMP_Text FindTextByName(Transform root, string prefix)
    {
        TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            if (texts[i].name.StartsWith(prefix, StringComparison.Ordinal))
            {
                return texts[i];
            }
        }

        return null;
    }

    private static Image FindDirectImage(Transform root, string objectName)
    {
        if (root == null)
        {
            return null;
        }

        Transform child = root.Find(objectName);
        return child != null ? child.GetComponent<Image>() : null;
    }

    private static Button FindButton(Transform root, string objectName)
    {
        if (root == null)
        {
            return null;
        }

        Button[] buttons = root.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            if (string.Equals(
                    buttons[i].name,
                    objectName,
                    StringComparison.Ordinal))
            {
                return buttons[i];
            }
        }

        return null;
    }

    private static RectTransform FindRect(Transform root, string objectName)
    {
        if (root == null || string.IsNullOrEmpty(objectName))
        {
            return null;
        }

        RectTransform[] rects = root.GetComponentsInChildren<RectTransform>(true);
        for (int i = 0; i < rects.Length; i++)
        {
            if (string.Equals(rects[i].name, objectName, StringComparison.Ordinal))
            {
                return rects[i];
            }
        }

        return null;
    }

    private static GameObject CreateRectObject(Transform parent, string name, Rect rect)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        SetTopLeft(go.GetComponent<RectTransform>(), rect);
        return go;
    }

    private static GameObject CreateStretchObject(Transform parent, string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        SetFullStretch(go.GetComponent<RectTransform>());
        return go;
    }

    private static GameObject CreateTerminalPageVisual(Transform parent)
    {
        GameObject visual = CreateStretchObject(parent, "V2_PageVisual");
        Canvas canvas = visual.AddComponent<Canvas>();
        SerializedObject serializedCanvas = new SerializedObject(canvas);
        serializedCanvas.FindProperty("m_OverrideSorting").boolValue = true;
        serializedCanvas.FindProperty("m_SortingOrder").intValue = 10;
        serializedCanvas.ApplyModifiedPropertiesWithoutUndo();
        return visual;
    }

    private static void SetTopLeft(RectTransform rect, Rect bounds)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(bounds.x, -bounds.y);
        rect.sizeDelta = new Vector2(bounds.width, bounds.height);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private static void SetFullStretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private static void SetStretch(RectTransform rect, float left, float top, float right, float bottom)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }

    private static Sprite LoadSprite(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void ConfigureGeneratedPartImporters()
    {
        string absoluteRoot = Path.Combine(Directory.GetCurrentDirectory(), PartsRoot);
        if (!Directory.Exists(absoluteRoot))
        {
            Debug.LogError("[HearthUiV2Builder] GeneratedParts folder is missing: " + PartsRoot);
            return;
        }

        string[] files = Directory.GetFiles(absoluteRoot, "*.png", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            string assetPath = files[i].Replace('\\', '/');
            int assetsIndex = assetPath.IndexOf("Assets/", StringComparison.Ordinal);
            if (assetsIndex >= 0)
            {
                assetPath = assetPath.Substring(assetsIndex);
            }

            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                continue;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.sRGBTexture = true;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePixelsPerUnit = 100f;
            bool sliced = assetPath.IndexOf("9Slice", StringComparison.OrdinalIgnoreCase) >= 0;
            if (sliced)
            {
                importer.spriteBorder = new Vector4(28f, 28f, 28f, 28f);
            }

            importer.SaveAndReimport();
        }
    }

    private static void RemoveExistingV2Objects(GameObject root)
    {
        HearthUiThemeMarker marker = root.GetComponent<HearthUiThemeMarker>();
        if (marker != null)
        {
            UnityEngine.Object.DestroyImmediate(marker);
        }

        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = transforms.Length - 1; i >= 0; i--)
        {
            if (transforms[i] != root.transform &&
                transforms[i].name.StartsWith("V2_", StringComparison.Ordinal) &&
                !IsInsidePreservedV2Root(
                    transforms[i],
                    root.transform,
                    "V2_InitialTutorialRoot"))
            {
                UnityEngine.Object.DestroyImmediate(transforms[i].gameObject);
            }
        }
    }

    private static bool IsInsidePreservedV2Root(
        Transform candidate,
        Transform assetRoot,
        string preservedRootName)
    {
        Transform cursor = candidate;
        while (cursor != null && cursor != assetRoot)
        {
            if (string.Equals(
                cursor.name,
                preservedRootName,
                StringComparison.Ordinal))
            {
                return true;
            }

            cursor = cursor.parent;
        }

        return false;
    }

    private static void SwitchOpenScene(bool useV2)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            EditorApplication.isCompiling ||
            PrefabStageUtility.GetCurrentPrefabStage() != null)
        {
            Debug.LogError(
                "[HearthUiV2Builder] UI switch is available only in a stable Edit Mode scene, outside Prefab Mode.");
            return;
        }

        Scene scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("[HearthUiV2Builder] No valid loaded active scene is open.");
            return;
        }

        List<GameObject> rootsToReplace = new List<GameObject>();
        HearthFirstPersonHudController[] human =
            FindSceneObjects<HearthFirstPersonHudController>(scene).ToArray();
        HearthCompanionHudController[] companion =
            FindSceneObjects<HearthCompanionHudController>(scene).ToArray();
        HearthTvTerminalController[] terminals =
            FindSceneObjects<HearthTvTerminalController>(scene).ToArray();

        if (human.Length != 1 || companion.Length != 1 || terminals.Length != 5)
        {
            Debug.LogError(
                "[HearthUiV2Builder] UI switch aborted before mutation. Expected 1 Human HUD, 1 Companion HUD and 5 terminals, but found " +
                human.Length + ", " + companion.Length + " and " + terminals.Length + ".");
            return;
        }

        if (!ValidateOpenSceneUiInternal(false) ||
            !HearthRuntimeTopologyTools.ValidateOpenSceneP0Topology(false))
        {
            Debug.LogError(
                "[HearthUiV2Builder] UI switch aborted before mutation because the current scene does not pass structural validation.");
            return;
        }

        rootsToReplace.Add(human[0].gameObject);
        rootsToReplace.Add(companion[0].gameObject);

        for (int i = 0; i < terminals.Length; i++)
        {
            rootsToReplace.Add(terminals[i].gameObject);
        }

        for (int i = 0; i < rootsToReplace.Count; i++)
        {
            for (int j = i + 1; j < rootsToReplace.Count; j++)
            {
                if (rootsToReplace[i].transform.IsChildOf(rootsToReplace[j].transform) ||
                    rootsToReplace[j].transform.IsChildOf(rootsToReplace[i].transform))
                {
                    Debug.LogError(
                        "[HearthUiV2Builder] UI switch aborted because replacement roots overlap in the hierarchy.");
                    return;
                }
            }
        }

        List<string> targetPaths = new List<string>();
        HashSet<string> uniqueTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < rootsToReplace.Count; i++)
        {
            GameObject oldRoot = rootsToReplace[i];
            string targetPath = ResolveTargetPrefabPath(oldRoot, useV2);
            GameObject targetPrefab =
                !string.IsNullOrEmpty(targetPath)
                    ? AssetDatabase.LoadAssetAtPath<GameObject>(targetPath)
                    : null;
            if (string.IsNullOrEmpty(targetPath) ||
                !uniqueTargets.Add(targetPath) ||
                targetPrefab == null ||
                !IsCompatibleTargetPrefab(oldRoot, targetPrefab) ||
                !MatchesRequestedTheme(targetPrefab, useV2))
            {
                Debug.LogError(
                    "[HearthUiV2Builder] UI switch aborted before mutation because a slot is unknown, duplicated, missing, or incompatible: " +
                    GetPath(oldRoot.transform),
                    oldRoot);
                return;
            }

            targetPaths.Add(targetPath);
        }

        try
        {
            Dictionary<UnityEngine.Object, UnityEngine.Object> previewMap =
                new Dictionary<UnityEngine.Object, UnityEngine.Object>();
            HashSet<UnityEngine.Object> previewTargets =
                new HashSet<UnityEngine.Object>();
            for (int i = 0; i < rootsToReplace.Count; i++)
            {
                GameObject targetPrefab =
                    AssetDatabase.LoadAssetAtPath<GameObject>(targetPaths[i]);
                Dictionary<UnityEngine.Object, UnityEngine.Object> localMap =
                    BuildObjectMap(rootsToReplace[i], targetPrefab);
                foreach (KeyValuePair<UnityEngine.Object, UnityEngine.Object> pair in localMap)
                {
                    if (previewMap.ContainsKey(pair.Key))
                    {
                        throw new InvalidOperationException(
                            "The UI replacement preview repeats source object: " +
                            pair.Key.name);
                    }

                    if (!previewTargets.Add(pair.Value))
                    {
                        throw new InvalidOperationException(
                            "The UI replacement preview maps multiple sources to target object: " +
                            pair.Value.name);
                    }

                    previewMap.Add(pair.Key, pair.Value);
                }
            }

            ValidateReferenceCoverage(
                scene,
                previewMap,
                rootsToReplace);
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "[HearthUiV2Builder] UI switch aborted during reference preflight: " +
                exception.Message);
            return;
        }

        Undo.IncrementCurrentGroup();
        int undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName(
            useV2
                ? "Use HEARTH UI V2 In Open Scene"
                : "Use Legacy HEARTH UI In Open Scene");

        List<GameObject> newRoots = new List<GameObject>();
        bool[] activeStates = new bool[rootsToReplace.Count];
        int[] siblingIndexes = new int[rootsToReplace.Count];
        for (int i = 0; i < rootsToReplace.Count; i++)
        {
            activeStates[i] = rootsToReplace[i].activeSelf;
            siblingIndexes[i] =
                rootsToReplace[i].transform.GetSiblingIndex();
        }

        try
        {
            for (int i = 0; i < rootsToReplace.Count; i++)
            {
                GameObject newRoot = InstantiateReplacementRoot(
                    rootsToReplace[i],
                    targetPaths[i],
                    scene,
                    activeStates[i]);
                if (newRoot == null)
                {
                    throw new InvalidOperationException(
                        "Could not replace UI root: " +
                        GetPath(rootsToReplace[i].transform));
                }

                newRoots.Add(newRoot);
            }

            Dictionary<UnityEngine.Object, UnityEngine.Object> globalMap =
                new Dictionary<UnityEngine.Object, UnityEngine.Object>();
            HashSet<UnityEngine.Object> mappedTargets =
                new HashSet<UnityEngine.Object>();
            for (int i = 0; i < rootsToReplace.Count; i++)
            {
                Dictionary<UnityEngine.Object, UnityEngine.Object> localMap =
                    BuildObjectMap(rootsToReplace[i], newRoots[i]);
                foreach (KeyValuePair<UnityEngine.Object, UnityEngine.Object> pair in localMap)
                {
                    if (globalMap.ContainsKey(pair.Key))
                    {
                        throw new InvalidOperationException(
                            "Duplicate source object in UI replacement map: " +
                            pair.Key.name);
                    }

                    if (!mappedTargets.Add(pair.Value))
                    {
                        throw new InvalidOperationException(
                            "Multiple source objects map to the same target object: " +
                            pair.Value.name);
                    }

                    globalMap.Add(pair.Key, pair.Value);
                }
            }

            for (int i = 0; i < rootsToReplace.Count; i++)
            {
                CopyMonoBehaviourState(
                    rootsToReplace[i],
                    newRoots[i],
                    globalMap);
                RepairHumanFocusBindings(newRoots[i]);
                RepairCompanionLayoutBaselines(newRoots[i]);
                if (!RepairTerminalCameraBinding(newRoots[i]))
                {
                    throw new InvalidOperationException(
                        "Could not repair terminal camera binding: " +
                        GetPath(newRoots[i].transform));
                }
            }

            ValidateReferenceCoverage(
                scene,
                globalMap,
                rootsToReplace);
            RemapSceneReferences(
                scene,
                globalMap,
                rootsToReplace);

            for (int i = 0; i < rootsToReplace.Count; i++)
            {
                Undo.DestroyObjectImmediate(rootsToReplace[i]);
            }

            List<int> siblingRestoreOrder = new List<int>();
            for (int i = 0; i < newRoots.Count; i++)
            {
                siblingRestoreOrder.Add(i);
            }

            siblingRestoreOrder.Sort(
                delegate(int left, int right)
                {
                    int siblingComparison =
                        siblingIndexes[left].CompareTo(
                            siblingIndexes[right]);
                    return siblingComparison != 0
                        ? siblingComparison
                        : left.CompareTo(right);
                });

            for (int orderIndex = 0;
                 orderIndex < siblingRestoreOrder.Count;
                 orderIndex++)
            {
                int i = siblingRestoreOrder[orderIndex];
                Transform parent = newRoots[i].transform.parent;
                int maxIndex = parent != null
                    ? Mathf.Max(0, parent.childCount - 1)
                    : Mathf.Max(0, scene.rootCount - 1);
                Undo.RecordObject(
                    newRoots[i],
                    "Restore switched UI root state");
                Undo.SetSiblingIndex(
                    newRoots[i].transform,
                    Mathf.Clamp(siblingIndexes[i], 0, maxIndex),
                    "Restore switched UI sibling order");
                PrefabUtility.RecordPrefabInstancePropertyModifications(
                    newRoots[i]);
                PrefabUtility.RecordPrefabInstancePropertyModifications(
                    newRoots[i].transform);
            }

            HearthTvTerminalController[] updatedTerminals =
                FindSceneObjects<HearthTvTerminalController>(scene).ToArray();
            for (int i = 0; i < updatedTerminals.Length; i++)
            {
                Undo.RecordObject(
                    updatedTerminals[i],
                    "Configure switched terminal");
                updatedTerminals[i].SetHideFirstPersonUiWhileOpen(true);
                PrefabUtility.RecordPrefabInstancePropertyModifications(
                    updatedTerminals[i]);
                EditorUtility.SetDirty(updatedTerminals[i]);
            }

            if (!ValidateOpenSceneUiInternal(false, useV2) ||
                !HearthRuntimeTopologyTools.ValidateOpenSceneP0Topology(false))
            {
                throw new InvalidOperationException(
                    "The switched UI failed structural validation.");
            }
        }
        catch (Exception exception)
        {
            Undo.FlushUndoRecordObjects();
            Undo.RevertAllDownToGroup(undoGroup);
            Debug.LogError(
                "[HearthUiV2Builder] UI switch failed and was rolled back without saving: " +
                exception.Message);
            return;
        }

        try
        {
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                throw new InvalidOperationException(
                    "Unity returned false while saving the active scene.");
            }
        }
        catch (Exception exception)
        {
            Undo.FlushUndoRecordObjects();
            Undo.RevertAllDownToGroup(undoGroup);
            Debug.LogError(
                "[HearthUiV2Builder] UI switch could not save the active scene and was rolled back: " +
                exception.Message);
            return;
        }

        Undo.FlushUndoRecordObjects();
        Undo.CollapseUndoOperations(undoGroup);
        Debug.Log("[HearthUiV2Builder] Open scene now uses " + (useV2 ? "HEARTH UI V2" : "Legacy HEARTH UI") + ". Runtime references were remapped.");
        ValidateOpenSceneUiInternal(true, useV2);
    }

    private static bool IsCompatibleTargetPrefab(
        GameObject oldRoot,
        GameObject targetPrefab)
    {
        if (oldRoot == null || targetPrefab == null)
        {
            return false;
        }

        if (oldRoot.GetComponent<HearthFirstPersonHudController>() != null)
        {
            HearthFirstPersonHudController[] controllers =
                targetPrefab.GetComponents<HearthFirstPersonHudController>();
            return controllers.Length == 1 &&
                   ValidateHumanPageReferences(controllers[0]) == 0 &&
                   HasInteractionPrompt(
                       targetPrefab.transform,
                       "InteractionPromptLayer/PlayerInteractionPrompt");
        }

        if (oldRoot.GetComponent<HearthCompanionHudController>() != null)
        {
            return targetPrefab.GetComponents<HearthCompanionHudController>().Length == 1 &&
                   HasInteractionPrompt(
                       targetPrefab.transform,
                       "InteractionLayer/PlayerInteractionPrompt");
        }

        HearthTvTerminalController[] terminalControllers =
            targetPrefab.GetComponents<HearthTvTerminalController>();
        return oldRoot.GetComponent<HearthTvTerminalController>() != null &&
               terminalControllers.Length == 1 &&
               ValidateTerminalPageReferences(terminalControllers[0]) == 0;
    }

    private static bool HasInteractionPrompt(
        Transform root,
        string relativePath)
    {
        Transform prompt =
            root != null ? root.Find(relativePath) : null;
        return prompt != null &&
               prompt.GetComponentInChildren<TMP_Text>(true) != null;
    }

    private static bool MatchesRequestedTheme(
        GameObject targetPrefab,
        bool useV2)
    {
        HearthUiThemeMarker marker =
            targetPrefab != null
                ? targetPrefab.GetComponent<HearthUiThemeMarker>()
                : null;
        return useV2
            ? marker != null &&
              marker.Version == HearthUiThemeVersion.V2 &&
              marker.BuildLabel == "HEARTH UI V2"
            : marker == null;
    }

    private static string ResolveTargetPrefabPath(GameObject root, bool useV2)
    {
        if (root.GetComponent<HearthFirstPersonHudController>() != null)
        {
            return useV2 ? V2Human : LegacyHuman;
        }

        if (root.GetComponent<HearthCompanionHudController>() != null)
        {
            return useV2 ? V2Companion : LegacyCompanion;
        }

        HearthTvTerminalController terminal = root.GetComponent<HearthTvTerminalController>();
        if (terminal == null)
        {
            return string.Empty;
        }

        string path = GetPath(root.transform).ToUpperInvariant();
        string resident = terminal.GetReplayResidentId();
        if (path.Contains("LOBBY") || path.Contains("ASSIGNMENT"))
        {
            return useV2 ? V2TerminalLobby : LegacyTerminalLobby;
        }

        if (path.Contains("17F04") || path.Contains("HOME") || resident == "17F04")
        {
            return useV2 ? V2Terminal04 : LegacyTerminal04;
        }

        if (resident == "17F03")
        {
            return useV2 ? V2Terminal03 : LegacyTerminal03;
        }

        if (resident == "17F02")
        {
            return useV2 ? V2Terminal02 : LegacyTerminal02;
        }

        if (resident == "17F01" || path.Contains("17F01"))
        {
            return useV2 ? V2Terminal01 : LegacyTerminal01;
        }

        Debug.LogError(
            "[HearthUiV2Builder] Terminal identity is ambiguous. Refusing to replace it with the 17F01 fallback: " +
            GetPath(root.transform),
            root);
        return string.Empty;
    }

    private static GameObject InstantiateReplacementRoot(
        GameObject oldRoot,
        string targetPath,
        Scene scene,
        bool active)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(targetPath);
        if (prefab == null ||
            oldRoot == null ||
            oldRoot.scene != scene)
        {
            Debug.LogWarning("[HearthUiV2Builder] Could not replace " + (oldRoot != null ? GetPath(oldRoot.transform) : "null") + " with " + targetPath + ".");
            return null;
        }

        Transform parent = oldRoot.transform.parent;
        string oldName = oldRoot.name;
        Vector3 localPosition = oldRoot.transform.localPosition;
        Quaternion localRotation = oldRoot.transform.localRotation;
        Vector3 localScale = oldRoot.transform.localScale;

        GameObject newRoot =
            (parent != null
                ? PrefabUtility.InstantiatePrefab(prefab, parent)
                : PrefabUtility.InstantiatePrefab(prefab, scene))
            as GameObject;
        if (newRoot == null || newRoot.scene != scene)
        {
            if (newRoot != null)
            {
                UnityEngine.Object.DestroyImmediate(newRoot);
            }

            return null;
        }

        try
        {
            newRoot.name = oldName;
            newRoot.transform.localPosition = localPosition;
            newRoot.transform.localRotation = localRotation;
            newRoot.transform.localScale = localScale;
            CopyRectTransform(
                oldRoot.transform as RectTransform,
                newRoot.transform as RectTransform);
            newRoot.SetActive(active);
            Undo.RegisterCreatedObjectUndo(newRoot, "Switch HEARTH UI");
            PrefabUtility.RecordPrefabInstancePropertyModifications(newRoot);
            PrefabUtility.RecordPrefabInstancePropertyModifications(
                newRoot.transform);
            return newRoot;
        }
        catch
        {
            UnityEngine.Object.DestroyImmediate(newRoot);
            throw;
        }
    }

    private static void RepairHumanFocusBindings(GameObject root)
    {
        if (root == null || root.GetComponent<HearthFirstPersonHudController>() == null)
        {
            return;
        }

        HearthUiThemeMarker marker = root.GetComponent<HearthUiThemeMarker>();
        AssignHumanFocusTargets(
            root,
            marker != null && marker.Version == HearthUiThemeVersion.V2);
    }

    private static void RepairCompanionLayoutBaselines(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        HearthCompanionHudLayoutController layout = root.GetComponent<HearthCompanionHudLayoutController>();
        if (layout == null)
        {
            return;
        }

        Undo.RecordObject(layout, "Repair companion HUD layout baselines");
        layout.RecaptureBaselines();
        PrefabUtility.RecordPrefabInstancePropertyModifications(layout);
        EditorUtility.SetDirty(layout);
    }

    private static void RestoreV2TerminalVisualState(GameObject root)
    {
        HearthUiThemeMarker marker = root != null ? root.GetComponent<HearthUiThemeMarker>() : null;
        if (marker == null ||
            marker.Version != HearthUiThemeVersion.V2 ||
            root.GetComponent<HearthTvTerminalController>() == null)
        {
            return;
        }

        HearthHudPage[] pages = root.GetComponentsInChildren<HearthHudPage>(true);
        for (int i = 0; i < pages.Length; i++)
        {
            Transform visual = pages[i].transform.Find("V2_PageVisual");
            if (visual == null)
            {
                continue;
            }

            Graphic[] graphics = pages[i].GetComponentsInChildren<Graphic>(true);
            for (int g = 0; g < graphics.Length; g++)
            {
                Graphic graphic = graphics[g];
                if (graphic == null || graphic.transform.IsChildOf(visual))
                {
                    continue;
                }

                Color color = graphic.color;
                color.a = 0f;
                graphic.color = color;
                graphic.enabled = true;
                PrefabUtility.RecordPrefabInstancePropertyModifications(graphic);
                EditorUtility.SetDirty(graphic);
            }

            Canvas visualCanvas = visual.GetComponent<Canvas>();
            if (visualCanvas != null)
            {
                SerializedObject serializedCanvas = new SerializedObject(visualCanvas);
                serializedCanvas.FindProperty("m_OverrideSorting").boolValue = true;
                serializedCanvas.FindProperty("m_SortingOrder").intValue = 10;
                serializedCanvas.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.RecordPrefabInstancePropertyModifications(visualCanvas);
                EditorUtility.SetDirty(visualCanvas);
            }

            visual.gameObject.SetActive(true);
            PrefabUtility.RecordPrefabInstancePropertyModifications(visual.gameObject);
            EditorUtility.SetDirty(visual.gameObject);
        }
    }

    private static bool RepairTerminalCameraBinding(GameObject terminalRoot)
    {
        HearthTvTerminalController controller =
            terminalRoot != null ? terminalRoot.GetComponent<HearthTvTerminalController>() : null;
        if (controller == null)
        {
            return true;
        }

        Transform hardwareRoot = FindTerminalHardwareRoot(terminalRoot.transform);
        if (hardwareRoot == null)
        {
            Debug.LogError(
                "[HearthUiV2Builder] Could not resolve the physical TV root for terminal: " +
                GetPath(terminalRoot.transform),
                terminalRoot);
            return false;
        }

        Camera camera = FindCameraOutsideUiRoot(hardwareRoot, terminalRoot.transform);
        if (camera == null)
        {
            Debug.LogError(
                "[HearthUiV2Builder] Expected exactly one non-player camera under terminal hardware root: " +
                GetPath(hardwareRoot),
                hardwareRoot);
            return false;
        }

        Canvas terminalCanvas = controller.GetComponent<Canvas>();
        AudioListener listener = camera.GetComponent<AudioListener>();
        Undo.RecordObject(controller, "Configure switched terminal");
        if (terminalCanvas != null)
        {
            Undo.RecordObject(
                terminalCanvas,
                "Configure switched terminal canvas");
        }

        Undo.RecordObject(camera, "Configure switched terminal camera");
        if (listener != null)
        {
            Undo.RecordObject(listener, "Configure switched terminal listener");
        }

        controller.SetTerminalHardwareRoot(hardwareRoot);
        controller.SetTerminalCamera(camera);
        controller.SetWorldCamera(camera);
        controller.SetSwitchCameraWhileOpen(true);
        camera.enabled = false;

        if (listener != null)
        {
            listener.enabled = false;
        }

        PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
        if (terminalCanvas != null)
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(
                terminalCanvas);
        }

        PrefabUtility.RecordPrefabInstancePropertyModifications(camera);
        if (listener != null)
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(listener);
        }

        EditorUtility.SetDirty(controller);
        if (terminalCanvas != null)
        {
            EditorUtility.SetDirty(terminalCanvas);
        }

        EditorUtility.SetDirty(camera);
        if (listener != null)
        {
            EditorUtility.SetDirty(listener);
        }

        return true;
    }

    private static Camera FindCameraOutsideUiRoot(Transform searchRoot, Transform uiRoot)
    {
        if (searchRoot == null || uiRoot == null)
        {
            return null;
        }

        Camera[] cameras = searchRoot.GetComponentsInChildren<Camera>(true);
        List<Camera> valid = new List<Camera>();
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

            valid.Add(camera);
        }

        return valid.Count == 1 ? valid[0] : null;
    }

    private static Transform FindTerminalHardwareRoot(Transform uiRoot)
    {
        Transform cursor = uiRoot != null ? uiRoot.parent : null;
        Transform fallback = null;
        for (int depth = 0; cursor != null && depth < 8; depth++)
        {
            Camera camera = FindCameraOutsideUiRoot(cursor, uiRoot);
            if (camera != null && fallback == null)
            {
                fallback = cursor;
            }

            if (camera != null &&
                (cursor.name.StartsWith("TV", StringComparison.OrdinalIgnoreCase) ||
                 cursor.name.IndexOf("terminal", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 cursor.name.IndexOf("monitor", StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return cursor;
            }

            cursor = cursor.parent;
        }

        return fallback;
    }

    private static Dictionary<UnityEngine.Object, UnityEngine.Object> BuildObjectMap(GameObject oldRoot, GameObject newRoot)
    {
        Dictionary<UnityEngine.Object, UnityEngine.Object> map = new Dictionary<UnityEngine.Object, UnityEngine.Object>();
        Transform[] oldTransforms = oldRoot.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < oldTransforms.Length; i++)
        {
            Transform newTransform = FindEquivalentTransform(
                oldRoot.transform,
                newRoot.transform,
                oldTransforms[i]);
            if (newTransform == null)
            {
                continue;
            }

            map[oldTransforms[i].gameObject] = newTransform.gameObject;
            map[oldTransforms[i]] = newTransform;

            Component[] oldComponents = oldTransforms[i].GetComponents<Component>();
            for (int c = 0; c < oldComponents.Length; c++)
            {
                Component oldComponent = oldComponents[c];
                if (oldComponent == null || oldComponent is Transform)
                {
                    continue;
                }

                Component[] oldSameType = oldTransforms[i].GetComponents(oldComponent.GetType());
                int typeIndex = Array.IndexOf(oldSameType, oldComponent);
                Component[] newSameType = newTransform.GetComponents(oldComponent.GetType());
                if (typeIndex >= 0 && typeIndex < newSameType.Length)
                {
                    map[oldComponent] = newSameType[typeIndex];
                }
            }
        }

        return map;
    }

    private static Transform FindEquivalentTransform(
        Transform oldRoot,
        Transform newRoot,
        Transform oldTarget)
    {
        if (oldRoot == null || newRoot == null || oldTarget == null)
        {
            return null;
        }

        if (oldTarget == oldRoot)
        {
            return newRoot;
        }

        List<TransformPathSegment> segments =
            new List<TransformPathSegment>();
        Transform cursor = oldTarget;
        while (cursor != null && cursor != oldRoot)
        {
            segments.Add(
                new TransformPathSegment(
                    cursor.name,
                    GetSameNameSiblingIndex(cursor)));
            cursor = cursor.parent;
        }

        if (cursor != oldRoot)
        {
            return null;
        }

        segments.Reverse();
        Transform match = newRoot;
        for (int i = 0; i < segments.Count; i++)
        {
            match = FindChildByNameAndOrdinal(
                match,
                segments[i].Name,
                segments[i].Ordinal);
            if (match == null)
            {
                return null;
            }
        }

        return match;
    }

    private static int GetSameNameSiblingIndex(Transform target)
    {
        if (target == null || target.parent == null)
        {
            return 0;
        }

        int ordinal = 0;
        Transform parent = target.parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child == target)
            {
                return ordinal;
            }

            if (child.name == target.name)
            {
                ordinal++;
            }
        }

        return ordinal;
    }

    private static Transform FindChildByNameAndOrdinal(
        Transform parent,
        string childName,
        int ordinal)
    {
        if (parent == null)
        {
            return null;
        }

        int current = 0;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name != childName)
            {
                continue;
            }

            if (current == ordinal)
            {
                return child;
            }

            current++;
        }

        return null;
    }

    private static void CopyMonoBehaviourState(
        GameObject oldRoot,
        GameObject newRoot,
        Dictionary<UnityEngine.Object, UnityEngine.Object> map)
    {
        HearthFirstPersonHudController oldHuman =
            oldRoot.GetComponent<HearthFirstPersonHudController>();
        HearthFirstPersonHudController newHuman =
            newRoot.GetComponent<HearthFirstPersonHudController>();
        if (oldHuman != null && newHuman != null)
        {
            CopyWhitelistedProperties(oldHuman, newHuman, HumanFunctionalProperties, map);
            return;
        }

        HearthCompanionHudController oldCompanion =
            oldRoot.GetComponent<HearthCompanionHudController>();
        HearthCompanionHudController newCompanion =
            newRoot.GetComponent<HearthCompanionHudController>();
        if (oldCompanion != null && newCompanion != null)
        {
            CopyWhitelistedProperties(
                oldCompanion,
                newCompanion,
                CompanionFunctionalProperties,
                map);
            return;
        }

        HearthTvTerminalController oldTerminal =
            oldRoot.GetComponent<HearthTvTerminalController>();
        HearthTvTerminalController newTerminal =
            newRoot.GetComponent<HearthTvTerminalController>();
        if (oldTerminal == null || newTerminal == null)
        {
            return;
        }

        CopyWhitelistedProperties(
            oldTerminal,
            newTerminal,
            TerminalFunctionalProperties,
            map);

        string identity = (GetPath(oldRoot.transform) + "|" + oldTerminal.GetReplayResidentId())
            .ToUpperInvariant();
        if (identity.Contains("LOBBY") || identity.Contains("ASSIGNMENT"))
        {
            CopyPersistentVoidEvent(
                oldTerminal.OnOpened,
                newTerminal.OnOpened,
                map,
                newTerminal);
            CopyPersistentVoidEvent(
                oldTerminal.OnCustomPrimaryAction,
                newTerminal.OnCustomPrimaryAction,
                map,
                newTerminal);
        }
        else if (identity.Contains("17F04") || identity.Contains("HOME"))
        {
            CopyPersistentVoidEvent(
                oldTerminal.OnCustomPrimaryAction,
                newTerminal.OnCustomPrimaryAction,
                map,
                newTerminal);
        }
    }

    private static void CopyPersistentVoidEvent(
        UnityEvent source,
        UnityEvent target,
        Dictionary<UnityEngine.Object, UnityEngine.Object> map,
        UnityEngine.Object context)
    {
        if (source == null || target == null)
        {
            Debug.LogError(
                "[HearthUiV2Builder] Required scene callback event is null.",
                context);
            return;
        }

        Undo.RecordObject(context, "Copy HEARTH UI persistent callbacks");
        for (int i = target.GetPersistentEventCount() - 1; i >= 0; i--)
        {
            UnityEventTools.RemovePersistentListener(target, i);
        }

        for (int i = 0; i < source.GetPersistentEventCount(); i++)
        {
            UnityEngine.Object sourceTarget = source.GetPersistentTarget(i);
            UnityEngine.Object mappedTarget;
            UnityEngine.Object listenerTarget =
                sourceTarget != null && map.TryGetValue(sourceTarget, out mappedTarget)
                    ? mappedTarget
                    : sourceTarget;
            string methodName = source.GetPersistentMethodName(i);
            UnityAction action = listenerTarget != null
                ? Delegate.CreateDelegate(
                    typeof(UnityAction),
                    listenerTarget,
                    methodName,
                    false,
                    false) as UnityAction
                : null;
            if (action == null)
            {
                Debug.LogError(
                    "[HearthUiV2Builder] Could not preserve persistent callback target: " +
                    methodName + ".",
                    context);
                continue;
            }

            UnityEventTools.AddPersistentListener(target, action);
            int targetIndex = target.GetPersistentEventCount() - 1;
            target.SetPersistentListenerState(
                targetIndex,
                source.GetPersistentListenerState(i));
        }

        PrefabUtility.RecordPrefabInstancePropertyModifications(context);
        EditorUtility.SetDirty(context);
    }

    private static int ValidateRequiredTerminalCallbacks(
        HearthTvTerminalController[] terminals)
    {
        List<HearthLobbyFlowController> lobbyFlows =
            FindSceneObjects<HearthLobbyFlowController>();
        List<Hearth17F04FinaleController> finaleControllers =
            FindSceneObjects<Hearth17F04FinaleController>();
        HearthTvTerminalController lobbyTerminal =
            FindUniqueTerminal(terminals, "LOBBY", "ASSIGNMENT");
        HearthTvTerminalController homeTerminal =
            FindUniqueTerminal(terminals, "17F04", "HOME");

        if (lobbyFlows.Count != 1 ||
            finaleControllers.Count != 1 ||
            lobbyTerminal == null ||
            homeTerminal == null)
        {
            Debug.LogWarning(
                "[HearthUiV2Builder] Required terminal callback topology is ambiguous.");
            return 1;
        }

        int issues = 0;
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
        HearthTvTerminalController[] terminals)
    {
        HearthTvTerminalController lobbyTerminal =
            FindUniqueTerminal(terminals, "LOBBY", "ASSIGNMENT");
        List<HearthSfxCuePlayer> cuePlayers =
            FindSceneObjects<HearthSfxCuePlayer>();
        HearthSfxCuePlayer lobbyCuePlayer = null;
        for (int i = 0; i < cuePlayers.Count; i++)
        {
            if (!string.Equals(
                    cuePlayers[i].gameObject.name,
                    "StorySFX_Lobby",
                    StringComparison.Ordinal))
            {
                continue;
            }

            if (lobbyCuePlayer != null)
            {
                lobbyCuePlayer = null;
                break;
            }

            lobbyCuePlayer = cuePlayers[i];
        }

        if (lobbyTerminal == null || lobbyCuePlayer == null)
        {
            Debug.LogWarning(
                "[HearthUiV2Builder] Lobby terminal active-loop cue topology is ambiguous.");
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
            "[HearthUiV2Builder] Lobby terminal must retain StorySFX_Lobby / AssignmentTerminal.Hum.",
            lobbyTerminal);
        return 1;
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
            "[HearthUiV2Builder] Human HUD page references are incomplete, null, duplicated, or outside the HUD root.",
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
            "[HearthUiV2Builder] Terminal page references are incomplete, null, duplicated, or outside the terminal root: " +
            GetPath(controller.transform),
            controller);
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
            "[HearthUiV2Builder] Missing or invalid persistent callback: " +
            label + " -> " + expectedMethod + ".",
            context);
        return 1;
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

    private static void CopyWhitelistedProperties(
        UnityEngine.Object source,
        UnityEngine.Object target,
        string[] propertyNames,
        Dictionary<UnityEngine.Object, UnityEngine.Object> map)
    {
        if (source == null || target == null || propertyNames == null)
        {
            return;
        }

        SerializedObject sourceSerialized = new SerializedObject(source);
        SerializedObject targetSerialized = new SerializedObject(target);
        bool changed = false;
        Undo.RecordObject(target, "Copy HEARTH UI functional state");

        for (int i = 0; i < propertyNames.Length; i++)
        {
            SerializedProperty sourceProperty =
                sourceSerialized.FindProperty(propertyNames[i]);
            SerializedProperty targetProperty =
                targetSerialized.FindProperty(propertyNames[i]);
            if (sourceProperty == null || targetProperty == null)
            {
                Debug.LogWarning(
                    "[HearthUiV2Builder] Whitelisted property was not found on both objects: " +
                    source.GetType().Name + "." + propertyNames[i],
                    source);
                continue;
            }

            CopySerializedPropertyValue(sourceProperty, targetProperty, map);
            changed = true;
        }

        if (changed)
        {
            targetSerialized.ApplyModifiedProperties();
            PrefabUtility.RecordPrefabInstancePropertyModifications(target);
            EditorUtility.SetDirty(target);
        }
    }

    private static void CopySerializedPropertyValue(
        SerializedProperty source,
        SerializedProperty target,
        Dictionary<UnityEngine.Object, UnityEngine.Object> map)
    {
        if (source == null || target == null)
        {
            return;
        }

        if (source.isArray &&
            source.propertyType != SerializedPropertyType.String &&
            target.isArray)
        {
            target.arraySize = source.arraySize;
            for (int i = 0; i < source.arraySize; i++)
            {
                CopySerializedPropertyValue(
                    source.GetArrayElementAtIndex(i),
                    target.GetArrayElementAtIndex(i),
                    map);
            }

            return;
        }

        switch (source.propertyType)
        {
            case SerializedPropertyType.Integer:
            case SerializedPropertyType.LayerMask:
            case SerializedPropertyType.Character:
            case SerializedPropertyType.ArraySize:
                target.intValue = source.intValue;
                return;
            case SerializedPropertyType.Boolean:
                target.boolValue = source.boolValue;
                return;
            case SerializedPropertyType.Float:
                target.floatValue = source.floatValue;
                return;
            case SerializedPropertyType.String:
                target.stringValue = source.stringValue;
                return;
            case SerializedPropertyType.Color:
                target.colorValue = source.colorValue;
                return;
            case SerializedPropertyType.ObjectReference:
                UnityEngine.Object sourceReference = source.objectReferenceValue;
                UnityEngine.Object replacement;
                target.objectReferenceValue =
                    sourceReference != null && map.TryGetValue(sourceReference, out replacement)
                        ? replacement
                        : sourceReference;
                return;
            case SerializedPropertyType.Enum:
                target.enumValueIndex = source.enumValueIndex;
                return;
            case SerializedPropertyType.Vector2:
                target.vector2Value = source.vector2Value;
                return;
            case SerializedPropertyType.Vector3:
                target.vector3Value = source.vector3Value;
                return;
            case SerializedPropertyType.Vector4:
                target.vector4Value = source.vector4Value;
                return;
            case SerializedPropertyType.Rect:
                target.rectValue = source.rectValue;
                return;
            case SerializedPropertyType.AnimationCurve:
                target.animationCurveValue = source.animationCurveValue;
                return;
            case SerializedPropertyType.Bounds:
                target.boundsValue = source.boundsValue;
                return;
            case SerializedPropertyType.Quaternion:
                target.quaternionValue = source.quaternionValue;
                return;
            case SerializedPropertyType.Vector2Int:
                target.vector2IntValue = source.vector2IntValue;
                return;
            case SerializedPropertyType.Vector3Int:
                target.vector3IntValue = source.vector3IntValue;
                return;
            case SerializedPropertyType.RectInt:
                target.rectIntValue = source.rectIntValue;
                return;
            case SerializedPropertyType.BoundsInt:
                target.boundsIntValue = source.boundsIntValue;
                return;
            case SerializedPropertyType.Generic:
                SerializedProperty sourceIterator = source.Copy();
                SerializedProperty sourceEnd = source.GetEndProperty();
                int childDepth = source.depth + 1;
                bool enterChildren = true;
                while (sourceIterator.NextVisible(enterChildren) &&
                       !SerializedProperty.EqualContents(sourceIterator, sourceEnd))
                {
                    enterChildren = false;
                    if (sourceIterator.depth != childDepth)
                    {
                        continue;
                    }

                    SerializedProperty targetChild =
                        target.FindPropertyRelative(sourceIterator.name);
                    if (targetChild != null)
                    {
                        CopySerializedPropertyValue(sourceIterator, targetChild, map);
                    }
                }

                return;
        }

        Debug.LogWarning(
            "[HearthUiV2Builder] Unsupported whitelisted property type: " +
            source.propertyPath + " (" + source.propertyType + ").");
    }

    private static Dictionary<string, UnityEngine.Object> CaptureInternalObjectReferences(
        MonoBehaviour behaviour)
    {
        Dictionary<string, UnityEngine.Object> references =
            new Dictionary<string, UnityEngine.Object>();
        SerializedObject serialized = new SerializedObject(behaviour);
        SerializedProperty property = serialized.GetIterator();
        bool enterChildren = true;
        Transform root = behaviour.transform.root;

        while (property.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (property.propertyType != SerializedPropertyType.ObjectReference ||
                property.propertyPath == "m_Script" ||
                property.objectReferenceValue == null)
            {
                continue;
            }

            GameObject referencedObject = property.objectReferenceValue as GameObject;
            Component referencedComponent = property.objectReferenceValue as Component;
            Transform referencedTransform = referencedObject != null
                ? referencedObject.transform
                : referencedComponent != null
                    ? referencedComponent.transform
                    : null;
            if (referencedTransform != null && referencedTransform.IsChildOf(root))
            {
                references[property.propertyPath] = property.objectReferenceValue;
            }
        }

        return references;
    }

    private static void RestoreObjectReferences(
        MonoBehaviour oldBehaviour,
        MonoBehaviour newBehaviour,
        Dictionary<UnityEngine.Object, UnityEngine.Object> map,
        Dictionary<string, UnityEngine.Object> prefabInternalReferences)
    {
        SerializedObject oldSerialized = new SerializedObject(oldBehaviour);
        SerializedObject newSerialized = new SerializedObject(newBehaviour);
        SerializedProperty oldProperty = oldSerialized.GetIterator();
        bool enterChildren = true;
        bool changed = false;

        while (oldProperty.NextVisible(enterChildren))
        {
            enterChildren = false;
            if (oldProperty.propertyType != SerializedPropertyType.ObjectReference ||
                oldProperty.propertyPath == "m_Script")
            {
                continue;
            }

            SerializedProperty newProperty = newSerialized.FindProperty(oldProperty.propertyPath);
            if (newProperty == null || newProperty.propertyType != SerializedPropertyType.ObjectReference)
            {
                continue;
            }

            UnityEngine.Object oldReference = oldProperty.objectReferenceValue;
            if (oldReference == null)
            {
                UnityEngine.Object prefabReference;
                if (prefabInternalReferences.TryGetValue(oldProperty.propertyPath, out prefabReference))
                {
                    newProperty.objectReferenceValue = prefabReference;
                    changed = true;
                }

                continue;
            }

            UnityEngine.Object replacement;
            newProperty.objectReferenceValue = map.TryGetValue(oldReference, out replacement)
                ? replacement
                : oldReference;
            changed = true;
        }

        if (changed)
        {
            newSerialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void ValidateReferenceCoverage(
        Scene scene,
        Dictionary<UnityEngine.Object, UnityEngine.Object> map,
        List<GameObject> oldRoots)
    {
        HashSet<Transform> excludedRoots = new HashSet<Transform>();
        for (int i = 0; oldRoots != null && i < oldRoots.Count; i++)
        {
            if (oldRoots[i] != null)
            {
                excludedRoots.Add(oldRoots[i].transform);
            }
        }

        Component[] components =
            FindSceneObjects<Component>(scene).ToArray();
        for (int i = 0; i < components.Length; i++)
        {
            Component component = components[i];
            if (component == null ||
                component is Transform ||
                IsInsideAnyRoot(component.transform, excludedRoots))
            {
                continue;
            }

            SerializedObject serialized;
            try
            {
                serialized = new SerializedObject(component);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Could not inspect scene reference coverage: " +
                    GetPath(component.transform),
                    exception);
            }

            SerializedProperty property = serialized.GetIterator();
            while (property.Next(true))
            {
                if (property.propertyType != SerializedPropertyType.ObjectReference ||
                    property.objectReferenceValue == null)
                {
                    continue;
                }

                Transform referencedTransform =
                    GetReferencedTransform(property.objectReferenceValue);
                if (referencedTransform != null &&
                    IsInsideAnyRoot(referencedTransform, excludedRoots) &&
                    !map.ContainsKey(property.objectReferenceValue))
                {
                    throw new InvalidOperationException(
                        "No equivalent target exists for referenced UI object " +
                        GetPath(referencedTransform) + " used by " +
                        GetPath(component.transform) + "." + property.propertyPath);
                }
            }
        }
    }

    private static Transform GetReferencedTransform(
        UnityEngine.Object reference)
    {
        GameObject referencedObject = reference as GameObject;
        if (referencedObject != null)
        {
            return referencedObject.transform;
        }

        Component referencedComponent = reference as Component;
        return referencedComponent != null
            ? referencedComponent.transform
            : null;
    }

    private static void RemapSceneReferences(
        Scene scene,
        Dictionary<UnityEngine.Object, UnityEngine.Object> map,
        List<GameObject> oldRoots)
    {
        HashSet<Transform> excludedRoots = new HashSet<Transform>();
        for (int i = 0; oldRoots != null && i < oldRoots.Count; i++)
        {
            if (oldRoots[i] != null)
            {
                excludedRoots.Add(oldRoots[i].transform);
            }
        }

        Component[] components =
            FindSceneObjects<Component>(scene).ToArray();
        for (int i = 0; i < components.Length; i++)
        {
            Component component = components[i];
            if (component == null ||
                component is Transform ||
                component.gameObject.scene != scene ||
                IsInsideAnyRoot(component.transform, excludedRoots))
            {
                continue;
            }

            SerializedObject so;
            try
            {
                so = new SerializedObject(component);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "Could not inspect scene component while remapping UI references: " +
                    GetPath(component.transform),
                    exception);
            }

            bool changed = false;
            List<string> remappedPaths = new List<string>();
            List<UnityEngine.Object> expectedReferences =
                new List<UnityEngine.Object>();
            SerializedProperty property = so.GetIterator();
            while (property.Next(true))
            {
                if (property.propertyType != SerializedPropertyType.ObjectReference ||
                    property.objectReferenceValue == null)
                {
                    continue;
                }

                UnityEngine.Object replacement;
                if (map.TryGetValue(property.objectReferenceValue, out replacement))
                {
                    if (!changed)
                    {
                        Undo.RecordObject(
                            component,
                            "Remap switched HEARTH UI reference");
                    }

                    remappedPaths.Add(property.propertyPath);
                    expectedReferences.Add(replacement);
                    property.objectReferenceValue = replacement;
                    changed = true;
                }
            }

            if (changed)
            {
                so.ApplyModifiedProperties();
                SerializedObject verification =
                    new SerializedObject(component);
                for (int pathIndex = 0;
                     pathIndex < remappedPaths.Count;
                     pathIndex++)
                {
                    SerializedProperty verified =
                        verification.FindProperty(remappedPaths[pathIndex]);
                    if (verified == null ||
                        verified.objectReferenceValue !=
                        expectedReferences[pathIndex])
                    {
                        throw new InvalidOperationException(
                            "UI reference remap did not persist on " +
                            GetPath(component.transform) + "." +
                            remappedPaths[pathIndex]);
                    }
                }

                PrefabUtility.RecordPrefabInstancePropertyModifications(
                    component);
                EditorUtility.SetDirty(component);
            }
        }
    }

    private static bool IsInsideAnyRoot(
        Transform transform,
        HashSet<Transform> roots)
    {
        if (transform == null || roots == null || roots.Count == 0)
        {
            return false;
        }

        Transform cursor = transform;
        while (cursor != null)
        {
            if (roots.Contains(cursor))
            {
                return true;
            }

            cursor = cursor.parent;
        }

        return false;
    }

    private static void CopyRectTransform(RectTransform source, RectTransform target)
    {
        if (source == null || target == null)
        {
            return;
        }

        target.anchorMin = source.anchorMin;
        target.anchorMax = source.anchorMax;
        target.pivot = source.pivot;
        target.anchoredPosition3D = source.anchoredPosition3D;
        target.sizeDelta = source.sizeDelta;
        target.offsetMin = source.offsetMin;
        target.offsetMax = source.offsetMax;
    }

    private static void SetSerializedInt(UnityEngine.Object target, string propertyName, int value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty property = so.FindProperty(propertyName);
        if (property != null)
        {
            property.intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void SetSerializedString(UnityEngine.Object target, string propertyName, string value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty property = so.FindProperty(propertyName);
        if (property != null)
        {
            property.stringValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
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

    private static void SetObjectReferenceArray(
        SerializedObject serialized,
        string propertyName,
        UnityEngine.Object[] values)
    {
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property == null || !property.isArray)
        {
            return;
        }

        property.arraySize = values != null ? values.Length : 0;
        for (int i = 0; values != null && i < values.Length; i++)
        {
            SerializedProperty element = property.GetArrayElementAtIndex(i);
            if (element.propertyType == SerializedPropertyType.ObjectReference)
            {
                element.objectReferenceValue = values[i];
            }
        }
    }

    private static string GetRelativePath(Transform root, Transform target)
    {
        if (target == root)
        {
            return string.Empty;
        }

        List<string> parts = new List<string>();
        Transform cursor = target;
        while (cursor != null && cursor != root)
        {
            parts.Add(cursor.name);
            cursor = cursor.parent;
        }

        parts.Reverse();
        return string.Join("/", parts.ToArray());
    }

    private static string GetPath(Transform transform)
    {
        if (transform == null)
        {
            return string.Empty;
        }

        string path = transform.name;
        Transform cursor = transform.parent;
        while (cursor != null)
        {
            path = cursor.name + "/" + path;
            cursor = cursor.parent;
        }

        return path;
    }

    private static string Sanitize(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "Empty";
        }

        char[] chars = value.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            if (!char.IsLetterOrDigit(chars[i]))
            {
                chars[i] = '_';
            }
        }

        return new string(chars);
    }

    private static bool RequirePlayMode()
    {
        if (Application.isPlaying)
        {
            return true;
        }

        Debug.LogWarning("[HearthUiV2Builder] This preview command is available only in Play Mode.");
        return false;
    }

    private static T FindSceneObject<T>() where T : Component
    {
        List<T> matches = FindSceneObjects<T>();
        return matches.Count > 0 ? matches[0] : null;
    }

    private static List<T> FindSceneObjects<T>() where T : Component
    {
        return FindSceneObjects<T>(EditorSceneManager.GetActiveScene());
    }

    private static List<T> FindSceneObjects<T>(Scene scene)
        where T : Component
    {
        T[] candidates = Resources.FindObjectsOfTypeAll<T>();
        List<T> matches = new List<T>();
        for (int i = 0; i < candidates.Length; i++)
        {
            T candidate = candidates[i];
            if (candidate == null ||
                !candidate.gameObject.scene.IsValid() ||
                !candidate.gameObject.scene.isLoaded ||
                candidate.gameObject.scene != scene)
            {
                continue;
            }

            matches.Add(candidate);
        }

        matches.Sort((left, right) => string.CompareOrdinal(GetPath(left.transform), GetPath(right.transform)));
        return matches;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        string name = Path.GetFileName(path);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }

    private struct TransformPathSegment
    {
        public readonly string Name;
        public readonly int Ordinal;

        public TransformPathSegment(string name, int ordinal)
        {
            Name = name;
            Ordinal = ordinal;
        }
    }
}
#endif
