#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Conservative production UI authoring tools. They never save an open scene
/// and never migrate text, active state, story, audio or UnityEvent data.
/// </summary>
public static partial class HearthProductionUiTools
{
    private const string MenuRoot = "Tools/Hearth/Validation/UI/";
    private const string HumanPrefab =
        "Assets/Prefabs/UI/HearthHud/V2/HearthHudRoot_V2.prefab";
    private const string CompanionPrefab =
        "Assets/Prefabs/UI/HearthHud/V2/Companion/HearthCompanionHudRoot_V2.prefab";
    private const string SubtitlePrefab =
        "Assets/Prefabs/UI/HearthSubtitle/V2/HearthSubtitleVisualCanvas_V2.prefab";
    private const string PhotoArchivePrefab =
        "Assets/Prefabs/UI/HearthHud/V2/PhotoArchive/HearthPhotoArchiveWorldView_V2.prefab";
    private const string InspectionPrefab =
        "Assets/Prefabs/UI/HearthHud/V2/Inspection/Hearth17F03InspectionPanel_V2.prefab";
    private const string TerminalFolder =
        "Assets/Prefabs/UI/HearthHud/V2/Terminals/";
    private const string Doorway01Prefab =
        "Assets/Prefabs/UI/HearthHud/V2/Terminals/Terminal_17F01_V2.prefab";
    private const string Doorway02Prefab =
        "Assets/Prefabs/UI/HearthHud/V2/Terminals/Terminal_17F02_V2.prefab";
    private const string Doorway03Prefab =
        "Assets/Prefabs/UI/HearthHud/V2/Terminals/Terminal_17F03_Alert_V2.prefab";
    private const string Home04Prefab =
        "Assets/Prefabs/UI/HearthHud/V2/Terminals/Terminal_17F04_Home_V2.prefab";
    private const string TaskCatalogPath =
        "Assets/Resources/HEARTH/HearthTaskTextCatalog.asset";
    private const string ThemeProfilePath =
        "Assets/UI/HEARTH/V2/Profiles/Hearth_UiV2Theme.asset";
    private const string InspectionFrameSvg =
        "Assets/UI/HEARTH/V2/VectorSource/Frames/Inspection/HUD_Inspection_EntityPanelFrame_1600x932.svg";
    private const string InspectionFramePng =
        "Assets/UI/HEARTH/V2/VectorParts/Inspection/HUD_Inspection_EntityPanelFrame_1600x932.png";
    private const string LobbyDialogueFrameSvg =
        "Assets/UI/HEARTH/V2/VectorSource/Frames/Terminal/HUD_Terminal_LobbyDialogueFrame_1460x248.svg";
    private const string LobbyDialogueFramePng =
        "Assets/UI/HEARTH/V2/VectorParts/Terminal/HUD_Terminal_LobbyDialogueFrame_1460x248.png";
    private const string InteractionPromptFrameSvg =
        "Assets/UI/HEARTH/V2/VectorSource/Frames/Interaction/HUD_Interaction_PromptFrame_680x150.svg";
    private const string InteractionPromptFramePng =
        "Assets/UI/HEARTH/V2/VectorParts/Interaction/HUD_Interaction_PromptFrame_680x150.png";
    private const int FirstTerminalIndex = 5;

    private static readonly string[] CanonicalPrefabs =
    {
        HumanPrefab,
        CompanionPrefab,
        SubtitlePrefab,
        PhotoArchivePrefab,
        InspectionPrefab,
        TerminalFolder + "Terminal_Lobby_Assignment_V2.prefab",
        TerminalFolder + "Terminal_17F01_V2.prefab",
        TerminalFolder + "Terminal_17F02_V2.prefab",
        TerminalFolder + "Terminal_17F03_Alert_V2.prefab",
        TerminalFolder + "Terminal_17F04_Home_V2.prefab"
    };

    private static readonly string[] RectProperties =
    {
        "m_AnchorMin.x", "m_AnchorMin.y", "m_AnchorMax.x", "m_AnchorMax.y",
        "m_AnchoredPosition.x", "m_AnchoredPosition.y", "m_SizeDelta.x",
        "m_SizeDelta.y", "m_Pivot.x", "m_Pivot.y", "m_LocalRotation.x",
        "m_LocalRotation.y", "m_LocalRotation.z", "m_LocalRotation.w",
        "m_LocalScale.x", "m_LocalScale.y", "m_LocalScale.z"
    };

    private static readonly string[] TmpProperties =
    {
        "m_fontAsset", "m_sharedMaterial", "m_fontColor", "m_fontColor32",
        "m_fontSize", "m_fontSizeBase", "m_fontWeight", "m_enableAutoSizing",
        "m_fontSizeMin", "m_fontSizeMax", "m_fontStyle", "m_HorizontalAlignment",
        "m_VerticalAlignment", "m_textAlignment", "m_lineSpacing",
        "m_lineSpacingMax", "m_paragraphSpacing", "m_characterSpacing",
        "m_wordSpacing", "m_margin", "m_enableWordWrapping", "m_overflowMode"
    };

    private static readonly string[] ImageProperties =
    {
        "m_Color", "m_Sprite", "m_Type", "m_PreserveAspect", "m_FillCenter",
        "m_FillMethod", "m_FillAmount", "m_FillClockwise", "m_FillOrigin"
    };

    private static readonly string[] CanvasProperties =
    {
        "m_OverrideSorting", "m_SortingOrder", "m_SortingLayerID"
    };

    [MenuItem(MenuRoot + "Compare Scene vs Prefab")]
    public static void CompareSceneVsPrefab()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("[Production UI] No loaded active scene.");
            return;
        }

        Dictionary<string, int> totals = CanonicalPrefabs.ToDictionary(
            path => path,
            path => 0);
        Dictionary<string, int> visual = CanonicalPrefabs.ToDictionary(
            path => path,
            path => 0);

        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Transform[] transforms = roots[i].GetComponentsInChildren<Transform>(true);
            for (int j = 0; j < transforms.Length; j++)
            {
                GameObject instanceRoot = PrefabUtility.GetNearestPrefabInstanceRoot(
                    transforms[j].gameObject);
                if (instanceRoot != transforms[j].gameObject)
                {
                    continue;
                }

                string path = GetSourcePrefabPath(instanceRoot);
                if (!totals.ContainsKey(path))
                {
                    continue;
                }

                PropertyModification[] modifications =
                    PrefabUtility.GetPropertyModifications(instanceRoot);
                if (modifications == null)
                {
                    continue;
                }

                totals[path] += modifications.Length;
                for (int k = 0; k < modifications.Length; k++)
                {
                    if (IsVisualModification(modifications[k]))
                    {
                        visual[path]++;
                    }
                }
            }
        }

        string report = string.Join(
            "\n",
            CanonicalPrefabs.Select(path =>
                string.Format(
                    "{0}: total={1}, visual={2}",
                    path,
                    totals[path],
                    visual[path])));
        Debug.Log(
            "[Production UI] Scene/Prefab comparison (read-only):\n" + report +
            "\nUse Adopt Approved Appearance only on one reviewed prefab instance at a time.");
    }

    // Historical migration entry: production appearance is edited in Prefab Mode.

    // Historical migration entry: production appearance is edited in Prefab Mode.

    // Historical migration entry: production appearance is edited in Prefab Mode.

    // Historical migration entry: production appearance is edited in Prefab Mode.

    // Historical migration entry: production appearance is edited in Prefab Mode.

    // Historical migration entry: production appearance is edited in Prefab Mode.

    // Historical migration entry: production appearance is edited in Prefab Mode.

    [MenuItem(MenuRoot + "Validate Doorway Navigation Alignment")]
    public static void ValidateDoorwayNavigationAlignment()
    {
        int issues = 0;
        GameObject sourceRoot = PrefabUtility.LoadPrefabContents(Doorway01Prefab);
        try
        {
            Transform sourceChrome = ResolveDoorwayChromeRoot(sourceRoot.transform);
            if (sourceChrome == null)
            {
                Debug.LogError(
                    "[Production UI] Canonical 17F01 TerminalVisualRoot/ChromeRoot is missing.");
                return;
            }

            string[] targets = { Doorway02Prefab, Doorway03Prefab };
            for (int i = 0; i < targets.Length; i++)
            {
                GameObject targetRoot = PrefabUtility.LoadPrefabContents(targets[i]);
                try
                {
                    Transform targetChrome = ResolveDoorwayChromeRoot(targetRoot.transform);
                    issues += CountDoorwayChromeDifferences(
                        sourceChrome,
                        targetChrome,
                        targets[i]);
                }
                finally
                {
                    PrefabUtility.UnloadPrefabContents(targetRoot);
                }
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(sourceRoot);
        }

        if (issues == 0)
        {
            Debug.Log(
                "[Production UI] 17F01, 17F02 and 17F03 active doorway navigation " +
                "RectTransforms and typography are aligned.");
        }
        else
        {
            Debug.LogError(
                "[Production UI] Doorway navigation validation found " + issues +
                " visual difference(s). Run Apply Approved Terminal And F03 Repairs.");
        }
    }

    [MenuItem(MenuRoot + "Validate Approved F03 And Hold E Presentation")]
    public static void ValidateApprovedF03AndHoldPresentation()
    {
        int issues = ValidateF03InspectionPresentation();
        issues += ValidateHoldPromptPresentation();
        issues += ValidateApprovedSimplifiedTerminalPresentation();
        if (issues == 0)
        {
            Debug.Log(
                "[Production UI] 17F03 Field Unit, full-screen selection hierarchy, " +
                "shared E/Hold E frame, Lobby terminal frame, Lily advance hint and " +
                "Final Response layout passed the approved presentation validation.");
        }
        else
        {
            Debug.LogError(
                "[Production UI] Approved F03/Hold E validation found " + issues +
                " issue(s). Run Apply Approved Terminal And F03 Repairs.");
        }
    }

    // Historical migration entry: production appearance is edited in Prefab Mode.

    // Historical migration entry: production appearance is edited in Prefab Mode.

    // Historical migration entry: production appearance is edited in Prefab Mode.

    [MenuItem(MenuRoot + "Validate Production UI")]
    public static void ValidateProductionUi()
    {
        int issues = 0;
        for (int i = 0; i < CanonicalPrefabs.Length; i++)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(CanonicalPrefabs[i]);
            if (prefab == null)
            {
                Debug.LogError("[Production UI] Missing canonical prefab: " + CanonicalPrefabs[i]);
                issues++;
                continue;
            }

            // HEARTH owns production text; Reach is used only for presentation.
            if (prefab.GetComponentsInChildren<Michsky.UI.Reach.LocalizedObject>(true).Length > 0 ||
                prefab.GetComponentsInChildren<Michsky.UI.Reach.ButtonManager>(true)
                    .Any(button => button.useLocalization))
            {
                Debug.LogError("[Production UI] Unused Reach localization is enabled in " +
                    CanonicalPrefabs[i] + ". Remove empty LocalizedObject components and disable " +
                    "Use Localization on presentation buttons; retain HEARTH text bindings.");
                issues++;
            }
        }

        if (AssetDatabase.LoadAssetAtPath<HearthTaskTextCatalog>(TaskCatalogPath) == null)
        {
            Debug.LogError("[Production UI] Missing task text catalog: " + TaskCatalogPath);
            issues++;
        }

        issues += ValidatePrefabBinding<HearthHumanHudBindings>(HumanPrefab);
        issues += ValidatePrefabBinding<HearthCompanionHudBindings>(CompanionPrefab);
        issues += ValidatePrefabBinding<HearthSubtitleViewBindings>(SubtitlePrefab);
        issues += ValidatePrefabBinding<HearthPhotoArchiveViewBindings>(PhotoArchivePrefab);
        for (int i = FirstTerminalIndex; i < CanonicalPrefabs.Length; i++)
        {
            issues += ValidatePrefabBinding<HearthTerminalViewBindings>(CanonicalPrefabs[i]);
            issues += ValidateSingleTerminalNavigationRoot(CanonicalPrefabs[i]);
        }
        GameObject homeTerminal = AssetDatabase.LoadAssetAtPath<GameObject>(
            TerminalFolder + "Terminal_17F04_Home_V2.prefab");
        HearthTerminalViewBindings homeTerminalBindings = homeTerminal != null
            ? homeTerminal.GetComponent<HearthTerminalViewBindings>()
            : null;
        if (homeTerminalBindings == null || !homeTerminalBindings.HasMessageSurface)
        {
            Debug.LogError(
                "[Production UI] 17F04 Home Terminal lacks its authored Lily message surface.");
            issues++;
        }

        Scene scene = EditorSceneManager.GetActiveScene();
        if (scene.IsValid() && scene.isLoaded)
        {
            HearthFirstPersonHudController[] human =
                FindSceneComponents<HearthFirstPersonHudController>(scene);
            HearthCompanionHudController[] companion =
                FindSceneComponents<HearthCompanionHudController>(scene);
            HearthTvTerminalController[] terminals =
                FindSceneComponents<HearthTvTerminalController>(scene);
            HearthPhotoArchiveWorldView[] archiveViews =
                FindSceneComponents<HearthPhotoArchiveWorldView>(scene);
            Hearth17F03InspectionPanel[] inspectionPanels =
                FindSceneComponents<Hearth17F03InspectionPanel>(scene);
            HearthCompanion17F02ReplayController[] f02Controllers =
                FindSceneComponents<HearthCompanion17F02ReplayController>(scene);
            HearthCompanion17F03ReplayController[] f03Controllers =
                FindSceneComponents<HearthCompanion17F03ReplayController>(scene);
            if (human.Length != 1) issues += LogCount("Human HUD", human.Length, 1);
            if (companion.Length != 1) issues += LogCount("Companion HUD", companion.Length, 1);
            if (terminals.Length != 5) issues += LogCount("terminals", terminals.Length, 5);
            if (archiveViews.Length != 1)
                issues += LogCount("TV4 photo archive controller", archiveViews.Length, 1);
            if (inspectionPanels.Length != 1)
                issues += LogCount("17F03 inspection panel", inspectionPanels.Length, 1);
            if (f02Controllers.Length != 1)
                issues += LogCount("17F02 replay controller", f02Controllers.Length, 1);
            if (f03Controllers.Length != 1)
                issues += LogCount("17F03 replay controller", f03Controllers.Length, 1);

            issues += ValidateUniqueActive<Camera>(scene, "active gameplay Camera");
            issues += ValidateUniqueActive<AudioListener>(scene, "active AudioListener");
            HearthPlayerControlLock[] locks =
                FindSceneComponents<HearthPlayerControlLock>(scene);
            ViewSwitchController[] switches =
                FindSceneComponents<ViewSwitchController>(scene);
            if (locks.Length != 1)
                issues += LogCount("HearthPlayerControlLock", locks.Length, 1);
            if (switches.Length != 1)
                issues += LogCount("ViewSwitchController", switches.Length, 1);

            MinLoopSubtitlePlayer[] subtitlePlayers =
                FindSceneComponents<MinLoopSubtitlePlayer>(scene);
            for (int i = 0; i < subtitlePlayers.Length; i++)
            {
                if (!HasCompleteSubtitleBinding(subtitlePlayers[i]))
                {
                    Debug.LogError(
                        "[Production UI] Subtitle player lacks a complete authored view: " +
                        GetHierarchyPath(subtitlePlayers[i].transform),
                        subtitlePlayers[i]);
                    issues++;
                }
                else if (!subtitlePlayers[i].UsesCanonicalAuthoredView ||
                         subtitlePlayers[i].AllowsRuntimeFallback)
                {
                    Debug.LogError(
                        "[Production UI] Subtitle player still permits runtime fallback UI: " +
                        GetHierarchyPath(subtitlePlayers[i].transform),
                        subtitlePlayers[i]);
                    issues++;
                }
            }

            for (int i = 0; i < terminals.Length; i++)
            {
                HearthTerminalViewBindings binding =
                    terminals[i].GetComponent<HearthTerminalViewBindings>();
                if (binding == null || !binding.HasDialogueSurface)
                {
                    Debug.LogError(
                        "[Production UI] Terminal lacks an authored Field Unit surface: " +
                        GetHierarchyPath(terminals[i].transform),
                        terminals[i]);
                    issues++;
                }
                else if (!terminals[i].UsesCanonicalAuthoredView ||
                         terminals[i].AllowsRuntimeSurfaceFallback)
                {
                    Debug.LogError(
                        "[Production UI] Terminal still permits runtime dialogue surface " +
                        "creation: " + GetHierarchyPath(terminals[i].transform),
                        terminals[i]);
                    issues++;
                }

                Transform terminalVisual =
                    FindNamed(terminals[i].transform, "TerminalVisualRoot");
                if (terminalVisual == null || !terminalVisual.gameObject.activeSelf ||
                    terminals[i].ContentRoot == null ||
                    !terminals[i].ContentRoot.gameObject.activeSelf)
                {
                    Debug.LogError(
                        "[Production UI] Terminal visual/content root is inactive and will " +
                        "open as a blank screen: " +
                        GetHierarchyPath(terminals[i].transform),
                        terminals[i]);
                    issues++;
                }

                if (terminals[i].TerminalMode == HearthTerminalMode.Doorway &&
                    !terminals[i].HasMinLoopFlowController)
                {
                    Debug.LogError(
                        "[Production UI] Doorway terminal lacks MinLoopFlowController and " +
                        "its replay/enter action cannot continue the household flow: " +
                        GetHierarchyPath(terminals[i].transform),
                        terminals[i]);
                    issues++;
                }
            }

            for (int i = 0; i < archiveViews.Length; i++)
            {
                HearthPhotoArchiveViewBindings binding =
                    archiveViews[i].GetComponentInChildren<HearthPhotoArchiveViewBindings>(true);
                if (binding == null || !binding.IsComplete)
                {
                    Debug.LogError(
                        "[Production UI] TV4 photo archive lacks its canonical authored view: " +
                        GetHierarchyPath(archiveViews[i].transform),
                        archiveViews[i]);
                    issues++;
                }
                else if (!archiveViews[i].UsesCanonicalAuthoredView ||
                         archiveViews[i].AllowsRuntimeFallback)
                {
                    Debug.LogError(
                        "[Production UI] TV4 photo archive still permits runtime UI creation: " +
                        GetHierarchyPath(archiveViews[i].transform),
                        archiveViews[i]);
                    issues++;
                }
            }

            for (int i = 0; i < inspectionPanels.Length; i++)
            {
                GameObject prefabRoot = PrefabUtility.GetNearestPrefabInstanceRoot(
                    inspectionPanels[i].gameObject);
                string sourcePath = GetSourcePrefabPath(prefabRoot);
                if (sourcePath != InspectionPrefab)
                {
                    Debug.LogError(
                        "[Production UI] F03 inspection panel is not connected to its " +
                        "canonical Prefab: " + GetHierarchyPath(inspectionPanels[i].transform),
                        inspectionPanels[i]);
                    issues++;
                }
                if (!inspectionPanels[i].UsesAuthoredVisualLayout)
                {
                    Debug.LogError(
                        "[Production UI] F03 inspection panel still permits runtime visual " +
                        "layout writes. Run Bind Open Scene To Canonical Views.",
                        inspectionPanels[i]);
                    issues++;
                }
            }

            for (int i = 0; i < f02Controllers.Length; i++)
            {
                if (!f02Controllers[i].UsesAuthoredTransitionService)
                {
                    Debug.LogError(
                        "[Production UI] 17F02 replay still permits its duplicated runtime " +
                        "blackout builder. Bind the authored blackout to the shared service.",
                        f02Controllers[i]);
                    issues++;
                }
            }

            for (int i = 0; i < f03Controllers.Length; i++)
            {
                if (!f03Controllers[i].UsesAuthoredTransitionService)
                {
                    Debug.LogError(
                        "[Production UI] 17F03 replay still permits its duplicated runtime " +
                        "blackout builder. Bind the authored blackout to the shared service.",
                        f03Controllers[i]);
                    issues++;
                }
            }

            int visualOverrides = CountCanonicalVisualOverrides(scene);
            if (visualOverrides > 0)
            {
                Debug.LogError(
                    "[Production UI] " + visualOverrides +
                    " canonical visual override(s) remain in the scene. Adopt the approved " +
                    "appearance into its Prefab, then clear only those visual overrides.");
                issues++;
            }

            int enabledLegacy = CountEnabledLegacyComponents(scene);
            if (enabledLegacy > 0)
            {
                Debug.LogWarning(
                    "[Production UI] " + enabledLegacy +
                    " enabled Legacy components remain. They are retained during quarantine; " +
                    "turn them off only after full flow validation.");
            }
        }

        if (issues == 0)
        {
            Debug.Log(
                "[Production UI] Canonical prefab and explicit binding validation passed. " +
                "Compatibility fallbacks may remain until Play Mode regression is complete.");
        }
        else
        {
            Debug.LogError("[Production UI] Validation found " + issues + " issue(s).");
        }
    }

    // Historical migration entry: production appearance is edited in Prefab Mode.

    private static int ValidateSingleTerminalNavigationRoot(string prefabPath)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            return 0;
        }

        int count = 0;
        RectTransform[] rects = prefab.GetComponentsInChildren<RectTransform>(true);
        for (int i = 0; i < rects.Length; i++)
        {
            if (rects[i] != null && rects[i].name == "KeyboardNavigationRoot")
            {
                count++;
            }
        }

        if (count == 1)
        {
            return 0;
        }

        Debug.LogError(
            "[Production UI] " + prefabPath + " has " + count +
            " KeyboardNavigationRoot objects; exactly one canonical prompt root is required.");
        return 1;
    }

    private static Transform ResolveDoorwayChromeRoot(Transform prefabRoot)
    {
        Transform terminalVisual = FindNamed(prefabRoot, "TerminalVisualRoot");
        return terminalVisual != null
            ? FindNamed(terminalVisual, "ChromeRoot")
            : null;
    }

    private static int CountDoorwayChromeDifferences(
        Transform sourceChrome,
        Transform targetChrome,
        string targetPath)
    {
        if (targetChrome == null)
        {
            Debug.LogError(
                "[Production UI] Missing active TerminalVisualRoot/ChromeRoot in " +
                targetPath + ".");
            return 1;
        }

        string[] chromeNames =
        {
            "TerminalLabel",
            "ResidentId",
            "BeforeTab",
            "AfterTab",
            "PrimaryActionTab",
            "ChromeHeaderRule"
        };
        int issues = 0;
        for (int i = 0; i < chromeNames.Length; i++)
        {
            Transform source = FindNamed(sourceChrome, chromeNames[i]);
            Transform target = FindNamed(targetChrome, chromeNames[i]);
            if (source == null || target == null)
            {
                Debug.LogError(
                    "[Production UI] Missing navigation element '" +
                    chromeNames[i] + "' in " + targetPath + ".");
                issues++;
                continue;
            }

            issues += CountVisualSubtreeDifferences(
                source,
                target,
                chromeNames[i],
                targetPath);
        }

        return issues;
    }

    private static int ValidateApprovedSimplifiedTerminalPresentation()
    {
        int issues = 0;
        string lobbyPath = TerminalFolder + "Terminal_Lobby_Assignment_V2.prefab";
        Sprite expectedLobbyFrame = AssetDatabase.LoadAssetAtPath<Sprite>(
            LobbyDialogueFramePng);
        GameObject lobbyRoot = PrefabUtility.LoadPrefabContents(lobbyPath);
        try
        {
            Transform panel = FindNamed(lobbyRoot.transform, "FieldUnitPanel");
            Image background = panel != null ? panel.GetComponent<Image>() : null;
            if (panel == null || background == null ||
                background.sprite != expectedLobbyFrame ||
                !TopLeftRectMatches(
                    panel as RectTransform,
                    new Rect(230f, 745f, 1460f, 248f)))
            {
                Debug.LogError(
                    "[Production UI] Lobby Field Unit surface is not using the " +
                    "approved simplified 1460x248 frame.");
                issues++;
            }

            if (panel != null)
            {
                HearthV2FrameGraphic[] legacyFrames =
                    panel.GetComponentsInChildren<HearthV2FrameGraphic>(true);
                if (legacyFrames.Any(frame => frame != null && frame.enabled))
                {
                    Debug.LogError(
                        "[Production UI] Lobby Field Unit surface still has an " +
                        "enabled legacy frame layered over the simplified Sprite.");
                    issues++;
                }
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(lobbyRoot);
        }

        GameObject homeRoot = PrefabUtility.LoadPrefabContents(Home04Prefab);
        try
        {
            Transform lilyPanel = FindNamed(homeRoot.transform, "LilyMessagePanel") ??
                                  FindNamed(homeRoot.transform, "TerminalMessageSurface_V2");
            TMP_Text hint = lilyPanel != null
                ? FindFirstText(
                    lilyPanel,
                    "AdvanceHint",
                    "DialogueAdvanceHint",
                    "ActionHint")
                : null;
            Color32 expectedOrange = new Color32(242, 163, 66, 255);
            if (hint == null ||
                !TopLeftRectMatches(
                    hint.rectTransform,
                    new Rect(1110f, 334f, 300f, 36f)) ||
                !TextHasFixedSize(hint, 26f) ||
                hint.alignment != TextAlignmentOptions.TopRight ||
                ColorDistance(hint.color, expectedOrange) > 0.01f)
            {
                Debug.LogError(
                    "[Production UI] Lily SPACE CONTINUE must remain orange at " +
                    "the bottom-right of the authored message surface.");
                issues++;
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(homeRoot);
        }

        GameObject humanRoot = PrefabUtility.LoadPrefabContents(HumanPrefab);
        try
        {
            string[] pageNames =
            {
                "Slide09_FinalChoice",
                "Slide14_FinalChoiceReturn"
            };
            for (int i = 0; i < pageNames.Length; i++)
            {
                Transform page = FindNamed(humanRoot.transform, pageNames[i]);
                Transform panel = page != null
                    ? FindNamed(page, "V2_PagePanel")
                    : null;
                if (!TopLeftRectMatches(
                        panel as RectTransform,
                        new Rect(360f, 180f, 1200f, 620f)))
                {
                    Debug.LogError(
                        "[Production UI] " + pageNames[i] +
                        " is not using the centred 1200x620 Final Response panel.");
                    issues++;
                }
            }

            TMP_Text finalHint = FindText(humanRoot.transform, "FinalChoiceInputHint");
            if (finalHint == null ||
                !TopLeftRectMatches(
                    finalHint.rectTransform,
                    new Rect(650f, 830f, 620f, 38f)) ||
                finalHint.alignment != TextAlignmentOptions.Center)
            {
                Debug.LogError(
                    "[Production UI] Final Response input hint is not centred " +
                    "beneath the approved choice panel.");
                issues++;
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(humanRoot);
        }

        return issues;
    }

    private static int CountVisualSubtreeDifferences(
        Transform source,
        Transform target,
        string role,
        string targetPath)
    {
        int issues = 0;
        Dictionary<string, Queue<Transform>> targetsByName =
            BuildTransformQueues(target);
        Transform[] sources = source.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < sources.Length; i++)
        {
            Queue<Transform> candidates;
            if (!targetsByName.TryGetValue(sources[i].name, out candidates) ||
                candidates.Count == 0)
            {
                Debug.LogError(
                    "[Production UI] Navigation subtree '" + role +
                    "' is missing visual child '" + sources[i].name +
                    "' in " + targetPath + ".");
                issues++;
                continue;
            }

            Transform matched = candidates.Dequeue();
            if (!RectTransformsMatch(
                    sources[i] as RectTransform,
                    matched as RectTransform))
            {
                Debug.LogError(
                    "[Production UI] Navigation RectTransform differs for '" +
                    role + "/" + sources[i].name + "' in " + targetPath + ".");
                issues++;
            }

            TMP_Text sourceText = sources[i].GetComponent<TMP_Text>();
            TMP_Text targetText = matched.GetComponent<TMP_Text>();
            if (!TmpTypographyMatches(sourceText, targetText))
            {
                Debug.LogError(
                    "[Production UI] Navigation typography differs for '" +
                    role + "/" + sources[i].name + "' in " + targetPath + ".");
                issues++;
            }
        }

        return issues;
    }

    private static bool TmpTypographyMatches(TMP_Text source, TMP_Text target)
    {
        if (source == null || target == null)
        {
            return source == target;
        }

        const float epsilon = 0.01f;
        return Mathf.Abs(source.fontSize - target.fontSize) <= epsilon &&
               source.enableAutoSizing == target.enableAutoSizing &&
               Mathf.Abs(source.fontSizeMin - target.fontSizeMin) <= epsilon &&
               Mathf.Abs(source.fontSizeMax - target.fontSizeMax) <= epsilon &&
               source.alignment == target.alignment &&
               source.fontStyle == target.fontStyle &&
               Vector4.Distance(source.margin, target.margin) <= epsilon;
    }

    private static int ValidateF03InspectionPresentation()
    {
        int issues = 0;
        GameObject root = PrefabUtility.LoadPrefabContents(InspectionPrefab);
        try
        {
            Hearth17F03InspectionPanel controller =
                root.GetComponent<Hearth17F03InspectionPanel>();
            Transform surfaceTransform =
                FindNamed(root.transform, "FieldUnitDialogueSurface");
            Transform dimmerTransform =
                FindNamed(root.transform, "FullscreenSelectionDimmer");
            Transform choiceTransform =
                FindNamed(root.transform, "DispositionChoiceRoot");
            Transform backdropTransform =
                FindNamed(root.transform, "InspectionBackdropFrameV2");

            if (controller == null || surfaceTransform == null ||
                dimmerTransform == null || choiceTransform == null ||
                backdropTransform == null)
            {
                Debug.LogError(
                    "[Production UI] F03 inspection presentation is missing its " +
                    "controller, wide frame, dialogue, dimmer or choice binding.");
                return 1;
            }

            Image backdrop = backdropTransform.GetComponent<Image>();
            Sprite expectedBackdrop = AssetDatabase.LoadAssetAtPath<Sprite>(
                InspectionFramePng);
            if (backdrop == null || backdrop.sprite != expectedBackdrop ||
                !TopLeftRectMatches(
                    backdropTransform as RectTransform,
                    new Rect(160f, 74f, 1600f, 932f)))
            {
                Debug.LogError(
                    "[Production UI] F03 wide inspection frame is not the approved " +
                    "1600x932 centred substrate.");
                issues++;
            }

            if (surfaceTransform.parent != root.transform ||
                dimmerTransform.parent != root.transform ||
                choiceTransform.parent != root.transform)
            {
                Debug.LogError(
                    "[Production UI] F03 dialogue, dimmer and choices must be direct " +
                    "children of the authored inspection root.");
                issues++;
            }

            if (!TopLeftRectMatches(
                    surfaceTransform as RectTransform,
                    new Rect(230f, 745f, 1460f, 248f)))
            {
                Debug.LogError(
                    "[Production UI] F03 Field Unit surface is not the approved " +
                    "1460x248 terminal message region.");
                issues++;
            }

            HearthDialogueSurface dialogueSurface =
                surfaceTransform.GetComponent<HearthDialogueSurface>();
            if (dialogueSurface == null ||
                controller.ResolveDialogueSurface() != dialogueSurface)
            {
                Debug.LogError(
                    "[Production UI] F03 Field Unit dialogue surface is not explicitly bound.");
                issues++;
            }

            TMP_Text speaker = FindText(surfaceTransform, "Speaker");
            TMP_Text body = FindText(surfaceTransform, "Body");
            TMP_Text hint = FindText(surfaceTransform, "AdvanceHint");
            if (!TextHasFixedSize(speaker, 52f) ||
                !TextHasFixedSize(body, 26f) ||
                !TextHasFixedSize(hint, 26f))
            {
                Debug.LogError(
                    "[Production UI] F03 Field Unit typography must be 52/26/26.");
                issues++;
            }
            if (speaker == null || speaker.text != "Field Unit" ||
                hint == null || hint.text != "SPACE  CONTINUE")
            {
                Debug.LogError(
                    "[Production UI] F03 Field Unit speaker or advance label is missing.");
                issues++;
            }

            Image dimmer = dimmerTransform.GetComponent<Image>();
            RectTransform dimmerRect = dimmerTransform as RectTransform;
            if (dimmer == null || dimmerRect == null ||
                Vector2.Distance(dimmerRect.anchorMin, Vector2.zero) > 0.01f ||
                Vector2.Distance(dimmerRect.anchorMax, Vector2.one) > 0.01f ||
                dimmerRect.offsetMin.sqrMagnitude > 0.01f ||
                dimmerRect.offsetMax.sqrMagnitude > 0.01f ||
                Mathf.Abs(dimmer.color.a - 0.82f) > 0.01f)
            {
                Debug.LogError(
                    "[Production UI] F03 selection dimmer is not full-screen at alpha 0.82.");
                issues++;
            }

            if (!(surfaceTransform.GetSiblingIndex() < dimmerTransform.GetSiblingIndex() &&
                  dimmerTransform.GetSiblingIndex() < choiceTransform.GetSiblingIndex()))
            {
                Debug.LogError(
                    "[Production UI] F03 authored order must be dialogue < dimmer < choices.");
                issues++;
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        return issues;
    }

    private static int ValidateHoldPromptPresentation()
    {
        int issues = 0;
        Sprite expectedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(
            InteractionPromptFramePng);
        GameObject root = PrefabUtility.LoadPrefabContents(CompanionPrefab);
        try
        {
            Transform holdPrompt = FindNamed(root.transform, "HoldPrompt");
            Transform frameTransform = holdPrompt != null
                ? holdPrompt.Find("HoldPromptFrameV2")
                : null;
            HearthCompanionHoldPrompt holdController = holdPrompt != null
                ? holdPrompt.GetComponent<HearthCompanionHoldPrompt>()
                : null;
            Image frame = frameTransform != null
                ? frameTransform.GetComponent<Image>()
                : null;
            RectTransform frameRect = frameTransform as RectTransform;
            if (holdPrompt == null || holdController == null || frame == null ||
                frameRect == null)
            {
                Debug.LogError(
                    "[Production UI] Canonical Companion HoldPrompt lacks its " +
                    "runtime controller or HoldPromptFrameV2 image.");
                return 1;
            }

            if (frame.sprite != expectedSprite || frame.type != Image.Type.Sliced ||
                frameTransform.GetSiblingIndex() != 0 ||
                Vector2.Distance(frameRect.anchorMin, Vector2.zero) > 0.01f ||
                Vector2.Distance(frameRect.anchorMax, Vector2.one) > 0.01f ||
                frameRect.offsetMin.sqrMagnitude > 0.01f ||
                frameRect.offsetMax.sqrMagnitude > 0.01f)
            {
                Debug.LogError(
                    "[Production UI] HoldPromptFrameV2 is not the full-stretch " +
                    "9-sliced approved Sprite.");
                issues++;
            }

            if (expectedSprite == null ||
                Vector4.Distance(
                    expectedSprite.border,
                    new Vector4(48f, 48f, 48f, 48f)) > 0.01f)
            {
                Debug.LogError(
                    "[Production UI] Shared E/Hold E Sprite border must remain " +
                    "48 px on the 2x PNG.");
                issues++;
            }

            Transform legacyBox = holdPrompt.Find("HoldPromptBox");
            if (legacyBox != null && legacyBox.gameObject.activeSelf)
            {
                Debug.LogError(
                    "[Production UI] Legacy HoldPromptBox must stay retired to " +
                    "avoid a doubled backing.");
                issues++;
            }

            Image progress = FindImage(holdPrompt, "HoldProgressFill");
            Color32 expectedAmber = new Color32(242, 163, 66, 242);
            if (progress == null ||
                ColorDistance(progress.color, expectedAmber) > 0.01f)
            {
                Debug.LogError(
                    "[Production UI] Hold E progress fill is missing or not amber.");
                issues++;
            }
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        return issues;
    }

    private static bool TopLeftRectMatches(RectTransform target, Rect expected)
    {
        if (target == null)
        {
            return false;
        }

        const float epsilon = 0.01f;
        return Vector2.Distance(target.anchorMin, new Vector2(0f, 1f)) <= epsilon &&
               Vector2.Distance(target.anchorMax, new Vector2(0f, 1f)) <= epsilon &&
               Vector2.Distance(target.pivot, new Vector2(0f, 1f)) <= epsilon &&
               Vector2.Distance(
                   target.anchoredPosition,
                   new Vector2(expected.x, -expected.y)) <= epsilon &&
               Vector2.Distance(
                   target.sizeDelta,
                   new Vector2(expected.width, expected.height)) <= epsilon;
    }

    private static bool TextHasFixedSize(TMP_Text text, float expected)
    {
        return text != null &&
               !text.enableAutoSizing &&
               Mathf.Abs(text.fontSize - expected) <= 0.01f;
    }

    private static float ColorDistance(Color first, Color second)
    {
        Vector4 delta = (Vector4)first - (Vector4)second;
        return delta.magnitude;
    }

    private static bool RectTransformsMatch(
        RectTransform source,
        RectTransform target)
    {
        if (source == null || target == null)
        {
            return source == target;
        }

        const float epsilon = 0.01f;
        return Vector2.Distance(source.anchorMin, target.anchorMin) <= epsilon &&
               Vector2.Distance(source.anchorMax, target.anchorMax) <= epsilon &&
               Vector2.Distance(source.pivot, target.pivot) <= epsilon &&
               Vector3.Distance(source.anchoredPosition3D, target.anchoredPosition3D) <= epsilon &&
               Vector2.Distance(source.sizeDelta, target.sizeDelta) <= epsilon &&
               Quaternion.Angle(source.localRotation, target.localRotation) <= epsilon &&
               Vector3.Distance(source.localScale, target.localScale) <= epsilon;
    }

    private static Dictionary<string, Queue<Transform>> BuildTransformQueues(
        Transform root)
    {
        Dictionary<string, Queue<Transform>> result =
            new Dictionary<string, Queue<Transform>>(StringComparer.Ordinal);
        Transform[] items = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < items.Length; i++)
        {
            Queue<Transform> queue;
            if (!result.TryGetValue(items[i].name, out queue))
            {
                queue = new Queue<Transform>();
                result.Add(items[i].name, queue);
            }
            queue.Enqueue(items[i]);
        }
        return result;
    }

    private static bool IsVisualModification(PropertyModification modification)
    {
        if (modification == null || modification.target == null) return false;
        if (modification.target is RectTransform rectTransform)
        {
            // Unity always serializes a Prefab instance root's placement as a
            // scene override, even when every value is zero and the property
            // cannot be applied/reverted like an authored child UI override.
            // Treating that engine-owned placement as a production UI drift
            // produces permanent false positives for every canonical canvas.
            // PropertyModification.target points at the corresponding object
            // in the Prefab asset (not the scene instance), so the canonical
            // asset root is identified by its persistent, parentless RectTransform.
            if (PrefabUtility.IsPartOfPrefabAsset(rectTransform) &&
                rectTransform.parent == null)
            {
                return false;
            }

            return RectProperties.Contains(modification.propertyPath);
        }
        if (modification.target is TMP_Text)
            return TmpProperties.Contains(modification.propertyPath);
        if (modification.target is Image)
            return ImageProperties.Contains(modification.propertyPath);
        if (modification.target is Canvas)
            return CanvasProperties.Contains(modification.propertyPath);
        return false;
    }

    private static int ValidatePrefabBinding<T>(string path) where T : Component
    {
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        T binding = asset != null ? asset.GetComponent<T>() : null;
        bool complete = binding != null;
        HearthHumanHudBindings human = binding as HearthHumanHudBindings;
        HearthCompanionHudBindings companion = binding as HearthCompanionHudBindings;
        HearthSubtitleViewBindings subtitle = binding as HearthSubtitleViewBindings;
        HearthPhotoArchiveViewBindings archive = binding as HearthPhotoArchiveViewBindings;
        HearthTerminalViewBindings terminal = binding as HearthTerminalViewBindings;
        if (human != null) complete = human.IsComplete;
        if (companion != null) complete = companion.IsComplete;
        if (subtitle != null) complete = subtitle.IsComplete;
        if (archive != null) complete = archive.IsComplete;
        if (terminal != null) complete = terminal.HasDialogueSurface;
        if (complete) return 0;
        Debug.LogError(
            "[Production UI] Missing or incomplete " + typeof(T).Name +
            " on " + path);
        return 1;
    }

    private static int ValidateUniqueActive<T>(Scene scene, string label)
        where T : Behaviour
    {
        T[] all = FindSceneComponents<T>(scene);
        int active = 0;
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].enabled && all[i].gameObject.activeInHierarchy) active++;
        }
        return active == 1 ? 0 : LogCount(label, active, 1);
    }

    private static bool HasCompleteSubtitleBinding(MinLoopSubtitlePlayer player)
    {
        if (player == null || player.VisualRoot == null) return false;
        HearthSubtitleViewBindings binding =
            player.VisualRoot.GetComponentInParent<HearthSubtitleViewBindings>(true);
        return binding != null && binding.IsComplete;
    }

    private static int CountCanonicalVisualOverrides(Scene scene)
    {
        int count = 0;
        HashSet<GameObject> visited = new HashSet<GameObject>();
        Transform[] transforms = FindSceneComponents<Transform>(scene);
        for (int i = 0; i < transforms.Length; i++)
        {
            GameObject root = PrefabUtility.GetNearestPrefabInstanceRoot(
                transforms[i].gameObject);
            if (root == null || !visited.Add(root)) continue;
            if (!CanonicalPrefabs.Contains(GetSourcePrefabPath(root))) continue;
            PropertyModification[] modifications =
                PrefabUtility.GetPropertyModifications(root);
            if (modifications == null) continue;
            for (int j = 0; j < modifications.Length; j++)
            {
                if (IsVisualModification(modifications[j])) count++;
            }
        }
        return count;
    }

    private static int CountEnabledLegacyComponents(Scene scene)
    {
        HashSet<string> names = new HashSet<string>
        {
            "TerminalUIController"
        };
        int count = 0;
        MonoBehaviour[] behaviours = FindSceneComponents<MonoBehaviour>(scene);
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] == null || !behaviours[i].enabled) continue;
            // Interaction and replay flow components are not UI builders.
            var presenter = behaviours[i] as MinLoopTerminalPresenter;
            if (presenter != null && presenter.CanCreateFallbackUI) count++;
            else if (names.Contains(behaviours[i].GetType().Name)) count++;
        }
        return count;
    }

    private static int LogCount(string label, int actual, int expected)
    {
        Debug.LogError(
            "[Production UI] Expected " + expected + " " + label +
            " instance(s), found " + actual + ".");
        return 1;
    }

    private static string GetSourcePrefabPath(GameObject instanceRoot)
    {
        if (instanceRoot == null) return string.Empty;
        return PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instanceRoot);
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        return component != null ? component : target.AddComponent<T>();
    }

    private static Transform FindNamed(Transform root, string name)
    {
        if (root == null) return null;
        Transform[] all = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; i++)
            if (all[i].name == name) return all[i];
        return null;
    }

    private static TMP_Text FindText(Transform root, string name)
    {
        Transform found = FindNamed(root, name);
        return found != null ? found.GetComponent<TMP_Text>() : null;
    }

    private static Image FindImage(Transform root, string name)
    {
        Transform found = FindNamed(root, name);
        return found != null ? found.GetComponent<Image>() : null;
    }

    private static TMP_Text FindFirstText(Transform root, params string[] names)
    {
        for (int i = 0; i < names.Length; i++)
        {
            TMP_Text found = FindText(root, names[i]);
            if (found != null) return found;
        }
        return null;
    }

    private static T[] FindSceneComponents<T>(Scene scene) where T : Component
    {
        List<T> results = new List<T>();
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
            results.AddRange(roots[i].GetComponentsInChildren<T>(true));
        return results.ToArray();
    }

    private static string GetHierarchyPath(Transform target)
    {
        if (target == null) return "<null>";
        string path = target.name;
        while (target.parent != null)
        {
            target = target.parent;
            path = target.name + "/" + path;
        }
        return path;
    }

}
#endif
