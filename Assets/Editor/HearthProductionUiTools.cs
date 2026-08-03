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
public static class HearthProductionUiTools
{
    private const string MenuRoot = "Tools/Hearth/Production UI/";
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
    private const string TaskCatalogPath =
        "Assets/Resources/HEARTH/HearthTaskTextCatalog.asset";
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

    [MenuItem(MenuRoot + "Preview Canonical Prefab")]
    public static void PreviewCanonicalPrefab()
    {
        GameObject selected = Selection.activeGameObject;
        string path = selected != null
            ? GetSourcePrefabPath(PrefabUtility.GetNearestPrefabInstanceRoot(selected))
            : string.Empty;
        if (!CanonicalPrefabs.Contains(path))
        {
            path = HumanPrefab;
        }

        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        Selection.activeObject = asset;
        EditorGUIUtility.PingObject(asset);
        AssetDatabase.OpenAsset(asset);
    }

    [MenuItem(MenuRoot + "Adopt Approved Appearance")]
    public static void AdoptApprovedAppearance()
    {
        GameObject instanceRoot = GetSelectedCanonicalInstance();
        if (instanceRoot == null)
        {
            return;
        }

        string path = GetSourcePrefabPath(instanceRoot);
        if (!EditorUtility.DisplayDialog(
                "Adopt approved UI appearance",
                "Only RectTransform, TMP visual settings, Image visuals and Canvas sorting will be copied to:\n\n" +
                path +
                "\n\nText, active state, story, audio, camera and events will not be copied.",
                "Adopt visual properties",
                "Cancel"))
        {
            return;
        }

        ApplyVisualOverridesToPrefab(instanceRoot, path);
        AssetDatabase.SaveAssets();
        Debug.Log(
            "[Production UI] Adopted approved visual properties into " + path +
            ". Unity recorded the operation for Undo.");
    }

    [MenuItem(MenuRoot + "Clear Visual Overrides")]
    public static void ClearVisualOverrides()
    {
        GameObject instanceRoot = GetSelectedCanonicalInstance();
        if (instanceRoot == null)
        {
            return;
        }

        if (!EditorUtility.DisplayDialog(
                "Clear visual overrides",
                "Only visual properties will be reverted to the canonical prefab. " +
                "Gameplay references and text are preserved. The scene will be marked dirty but not saved.",
                "Clear visual overrides",
                "Cancel"))
        {
            return;
        }

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Clear HEARTH visual overrides");
        RevertVisualTree(instanceRoot);
        EditorSceneManager.MarkSceneDirty(instanceRoot.scene);
        Debug.Log("[Production UI] Visual overrides cleared. Review and save the scene manually.");
    }

    [MenuItem(MenuRoot + "Install or Refresh Explicit Bindings")]
    public static void InstallExplicitBindings()
    {
        if (!EditorUtility.DisplayDialog(
                "Install production UI bindings",
                "This updates only the canonical V2 prefab assets and shared task catalog. " +
                "Open scenes are not saved or structurally rebuilt.",
                "Install",
                "Cancel"))
        {
            return;
        }

        InstallProductionAuthoringFoundationBatch();
    }

    public static void InstallProductionAuthoringFoundationBatch()
    {
        InstallHumanBindings();
        InstallCompanionBindings();
        InstallSubtitleBindings();
        InstallPhotoArchiveBindings();
        for (int i = FirstTerminalIndex; i < CanonicalPrefabs.Length; i++)
        {
            InstallTerminalBindings(CanonicalPrefabs[i]);
        }
        CreateOrRefreshTaskCatalog();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log(
            "[Production UI] Explicit bindings and task catalog installed. " +
            "No open scene was saved.");
    }

    [MenuItem(MenuRoot + "Bind Open Scene To Canonical Views")]
    public static void BindOpenSceneToCanonicalViews()
    {
        Scene scene = EditorSceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("[Production UI] No loaded active scene.");
            return;
        }

        if (!EditorUtility.DisplayDialog(
                "Bind open scene to canonical views",
                "This assigns explicit binding components to the existing Human HUD, " +
                "Companion HUD, subtitle players, terminals and formal PlayerInteraction " +
                "components. Story, audio, camera and active-state values are not changed. " +
                "If the canonical F03 inspection Prefab does not exist yet, the current " +
                "approved scene panel is adopted once without changing its child layout. " +
                "The scene will not be saved automatically.",
                "Bind",
                "Cancel"))
        {
            return;
        }

        int bound = 0;
        HearthFirstPersonHudController[] humanHuds =
            FindSceneComponents<HearthFirstPersonHudController>(scene);
        for (int i = 0; i < humanHuds.Length; i++)
        {
            HearthHumanHudBindings bindings =
                humanHuds[i].GetComponent<HearthHumanHudBindings>();
            if (bindings == null) continue;
            RecordAndBind(humanHuds[i], () => humanHuds[i].SetAuthoredBindings(bindings));
            bound++;
        }

        HearthCompanionHudController[] companionHuds =
            FindSceneComponents<HearthCompanionHudController>(scene);
        for (int i = 0; i < companionHuds.Length; i++)
        {
            HearthCompanionHudBindings bindings =
                companionHuds[i].GetComponent<HearthCompanionHudBindings>();
            if (bindings == null) continue;
            RecordAndBind(companionHuds[i], () => companionHuds[i].SetAuthoredBindings(bindings));
            bound++;
        }

        HearthTvTerminalController[] terminals =
            FindSceneComponents<HearthTvTerminalController>(scene);
        for (int i = 0; i < terminals.Length; i++)
        {
            HearthTerminalViewBindings bindings =
                terminals[i].GetComponent<HearthTerminalViewBindings>();
            if (bindings == null) continue;
            RecordAndBind(terminals[i], () => terminals[i].SetViewBindings(bindings));
            bound++;
        }

        MinLoopSubtitlePlayer[] subtitlePlayers =
            FindSceneComponents<MinLoopSubtitlePlayer>(scene);
        for (int i = 0; i < subtitlePlayers.Length; i++)
        {
            GameObject visualRoot = subtitlePlayers[i].VisualRoot;
            HearthSubtitleViewBindings bindings = visualRoot != null
                ? visualRoot.GetComponentInParent<HearthSubtitleViewBindings>(true)
                : null;
            if (bindings == null) continue;
            RecordAndBind(
                subtitlePlayers[i],
                () => subtitlePlayers[i].SetAuthoredBindings(bindings));
            bound++;
        }

        HearthPhotoArchiveWorldView[] archiveViews =
            FindSceneComponents<HearthPhotoArchiveWorldView>(scene);
        GameObject archivePrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>(PhotoArchivePrefab);
        for (int i = 0; i < archiveViews.Length; i++)
        {
            HearthPhotoArchiveViewBindings bindings =
                archiveViews[i].GetComponentInChildren<HearthPhotoArchiveViewBindings>(true);
            if (bindings == null && archivePrefab != null)
            {
                GameObject instance = PrefabUtility.InstantiatePrefab(
                    archivePrefab,
                    archiveViews[i].transform) as GameObject;
                if (instance != null)
                {
                    Undo.RegisterCreatedObjectUndo(
                        instance,
                        "Create canonical HEARTH photo archive view");
                    instance.transform.localPosition = Vector3.zero;
                    instance.transform.localRotation = Quaternion.identity;
                    instance.transform.localScale = Vector3.one;
                    bindings = instance.GetComponent<HearthPhotoArchiveViewBindings>();
                }
            }
            if (bindings == null) continue;
            RecordAndBind(
                archiveViews[i],
                () => archiveViews[i].SetAuthoredBindings(bindings));
            bound++;
        }

        Hearth17F03InspectionPanel[] inspectionPanels =
            FindSceneComponents<Hearth17F03InspectionPanel>(scene);
        if (inspectionPanels.Length == 1)
        {
            Hearth17F03InspectionPanel panel = inspectionPanels[0];
            if (AssetDatabase.LoadAssetAtPath<GameObject>(InspectionPrefab) == null)
            {
                EnsureAssetFolder("Assets/Prefabs/UI/HearthHud/V2/Inspection");
                PrefabUtility.SaveAsPrefabAssetAndConnect(
                    panel.gameObject,
                    InspectionPrefab,
                    InteractionMode.UserAction);
            }

            string sourcePath = GetSourcePrefabPath(
                PrefabUtility.GetNearestPrefabInstanceRoot(panel.gameObject));
            if (sourcePath == InspectionPrefab)
            {
                RecordAndBind(panel, () => panel.UseAuthoredVisualLayout(true));
                bound++;
            }
            else
            {
                Debug.LogError(
                    "[Production UI] F03 inspection panel was not changed because it is " +
                    "already connected to another Prefab: " + sourcePath,
                    panel);
            }
        }

        HearthCompanion17F02ReplayController[] f02Controllers =
            FindSceneComponents<HearthCompanion17F02ReplayController>(scene);
        for (int i = 0; i < f02Controllers.Length; i++)
        {
            if (BindTransitionService(
                    f02Controllers[i],
                    "blackoutCanvasGroup",
                    true,
                    f02Controllers[i].SetTransitionService))
            {
                bound++;
            }
        }

        HearthCompanion17F03ReplayController[] f03Controllers =
            FindSceneComponents<HearthCompanion17F03ReplayController>(scene);
        for (int i = 0; i < f03Controllers.Length; i++)
        {
            if (BindTransitionService(
                    f03Controllers[i],
                    "blackoutCanvasGroup",
                    true,
                    f03Controllers[i].SetTransitionService))
            {
                bound++;
            }
        }

        HearthHumanHudBindings humanBinding = humanHuds.Length == 1
            ? humanHuds[0].GetComponent<HearthHumanHudBindings>()
            : null;
        HearthCompanionHudBindings companionBinding = companionHuds.Length == 1
            ? companionHuds[0].GetComponent<HearthCompanionHudBindings>()
            : null;
        PlayerInteraction[] interactions = FindSceneComponents<PlayerInteraction>(scene);
        for (int i = 0; i < interactions.Length; i++)
        {
            HearthInteractionPromptPresenter presenter =
                IsHumanInteraction(interactions[i])
                    ? humanBinding != null ? humanBinding.InteractionPrompt : null
                    : IsCompanionInteraction(interactions[i])
                        ? companionBinding != null ? companionBinding.InteractionPrompt : null
                        : null;
            if (presenter == null) continue;
            RecordAndBind(
                interactions[i],
                () => interactions[i].BindPromptPresenter(presenter));
            bound++;
        }

        if (bound > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
        }
        Debug.Log(
            "[Production UI] Bound " + bound +
            " scene component(s) to canonical views. Review the diff and save manually.");
    }

    [MenuItem(MenuRoot + "Validate Production UI")]
    public static void ValidateProductionUi()
    {
        int issues = 0;
        for (int i = 0; i < CanonicalPrefabs.Length; i++)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(CanonicalPrefabs[i]) == null)
            {
                Debug.LogError("[Production UI] Missing canonical prefab: " + CanonicalPrefabs[i]);
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

    private static void InstallHumanBindings()
    {
        EditPrefab(HumanPrefab, root =>
        {
            HearthHumanHudBindings bindings =
                GetOrAdd<HearthHumanHudBindings>(root);
            Transform persistent = FindNamed(root.transform, "PersistentHud");
            TMP_Text identity = FindText(root.transform, "Text_004_MIA___7842");
            TMP_Text heading = FindText(root.transform, "Text_006_CURRENT_TASK");
            TMP_Text body = FindText(root.transform, "V2_CurrentTaskBody");
            if (heading != null && body == null)
            {
                body = UnityEngine.Object.Instantiate(heading, heading.transform.parent);
                body.name = "V2_CurrentTaskBody";
                body.text = string.Empty;
                SplitCurrentTaskRects(heading, body);
            }

            HearthInteractionPromptPresenter prompt =
                EnsurePromptPresenter(root.transform);
            List<RectTransform> selectionTargets =
                FindHumanSelectionTargets(root.transform);
            RectTransform[] selectionFills = new RectTransform[selectionTargets.Count];
            for (int i = 0; i < selectionTargets.Count; i++)
            {
                selectionFills[i] = EnsureAuthoredSelectionFill(selectionTargets[i]);
            }
            RetireLegacyHumanPageVisuals(root.transform);
            bindings.Configure(
                persistent != null ? persistent.gameObject : null,
                persistent != null ? persistent.GetComponent<CanvasGroup>() : null,
                identity,
                heading,
                body,
                prompt,
                selectionTargets.ToArray(),
                selectionFills);
            HearthFirstPersonHudController controller =
                root.GetComponent<HearthFirstPersonHudController>();
            if (controller != null)
            {
                controller.SetAuthoredBindings(bindings);
            }
        });
    }

    private static void InstallCompanionBindings()
    {
        EditPrefab(CompanionPrefab, root =>
        {
            HearthCompanionHudBindings bindings =
                GetOrAdd<HearthCompanionHudBindings>(root);
            bindings.Configure(
                FindText(root.transform, "V2_IdentityHeading"),
                FindText(root.transform, "V2_IdentityValue"),
                FindText(root.transform, "V2_TaskHeading"),
                FindText(root.transform, "V2_TaskBody"),
                EnsurePromptPresenter(root.transform),
                root.GetComponentInChildren<HearthCompanionHoldPrompt>(true));
            HearthCompanionHudController controller =
                root.GetComponent<HearthCompanionHudController>();
            if (controller != null)
            {
                controller.SetAuthoredBindings(bindings);
            }
        });
    }

    private static void InstallSubtitleBindings()
    {
        EditPrefab(SubtitlePrefab, root =>
        {
            Transform visual = FindNamed(root.transform, "VisualRoot");
            HearthSubtitleViewBindings bindings =
                GetOrAdd<HearthSubtitleViewBindings>(root);
            bindings.Configure(
                visual != null ? visual.gameObject : null,
                visual as RectTransform,
                visual != null ? visual.GetComponent<CanvasGroup>() : null,
                FindImage(visual, "Backdrop"),
                FindImage(visual, "AccentRule"),
                FindImage(visual, "SpeakerTab"),
                FindImage(visual, "FormalFrame"),
                FindImage(visual, "AuxiliaryFrame"),
                FindImage(visual, "SpeakerTabLeft"),
                FindImage(visual, "SpeakerTabRight"),
                FindText(visual, "Speaker"),
                FindText(visual, "Body"),
                FindText(visual, "AdvanceHint"),
                FindText(root.transform, "PersistentSceneHeader"),
                FindNamed(root.transform, "PersistentSceneHeader") != null
                    ? FindNamed(root.transform, "PersistentSceneHeader").GetComponent<CanvasGroup>()
                    : null);
        });
    }

    private static void InstallPhotoArchiveBindings()
    {
        CreatePhotoArchivePrefabIfMissing();
        EditPrefab(PhotoArchivePrefab, root =>
        {
            HearthPhotoArchiveViewBindings bindings =
                GetOrAdd<HearthPhotoArchiveViewBindings>(root);
            Transform panel = FindNamed(root.transform, "FieldUnitPanel");
            HearthDialogueSurface surface = panel != null
                ? EnsureDialogueSurface(panel)
                : null;
            bindings.Configure(
                root.GetComponent<Canvas>(),
                root.GetComponent<CanvasGroup>(),
                FindText(root.transform, "ArchivePage"),
                FindText(root.transform, "ArchiveReturnHint"),
                surface);
        });
    }

    private static void InstallTerminalBindings(string path)
    {
        EditPrefab(path, root =>
        {
            HearthTerminalViewBindings bindings =
                GetOrAdd<HearthTerminalViewBindings>(root);
            List<HearthDialogueSurface> pageSurfaces =
                new List<HearthDialogueSurface>();
            Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
            HearthDialogueSurface terminalSurface = null;
            HearthDialogueSurface messageSurface = null;
            for (int i = 0; i < transforms.Length; i++)
            {
                Transform item = transforms[i];
                if (item.name == "FieldUnitPanel")
                {
                    HearthDialogueSurface surface = EnsureDialogueSurface(item);
                    if (item.GetComponentInParent<HearthHudPage>(true) != null)
                    {
                        pageSurfaces.Add(surface);
                    }
                    else if (terminalSurface == null)
                    {
                        terminalSurface = surface;
                    }
                }
                else if (item.name == "LilyMessagePanel" ||
                         item.name == "TerminalMessageSurface_V2")
                {
                    messageSurface = EnsureDialogueSurface(item);
                }
            }

            bindings.Configure(terminalSurface, messageSurface, pageSurfaces.ToArray());
            HearthTvTerminalController controller =
                root.GetComponent<HearthTvTerminalController>();
            if (controller != null)
            {
                controller.SetViewBindings(bindings);
            }
        });
    }

    private static HearthDialogueSurface EnsureDialogueSurface(Transform panel)
    {
        HearthDialogueSurface surface = GetOrAdd<HearthDialogueSurface>(panel.gameObject);
        CanvasGroup group = GetOrAdd<CanvasGroup>(panel.gameObject);
        TMP_Text speaker = FindFirstText(panel, "Title", "Speaker", "SpeakerName");
        TMP_Text body = FindFirstText(panel, "Body", "DialogueText", "MessageText");
        TMP_Text hint = FindFirstText(panel, "DialogueAdvanceHint", "AdvanceHint");
        surface.Configure(group, speaker, body, hint);
        return surface;
    }

    private static HearthInteractionPromptPresenter EnsurePromptPresenter(Transform root)
    {
        Transform promptRoot = FindNamed(root, "PlayerInteractionPrompt");
        if (promptRoot == null) return null;
        HearthInteractionPromptPresenter presenter =
            GetOrAdd<HearthInteractionPromptPresenter>(promptRoot.gameObject);
        presenter.Configure(
            promptRoot.gameObject,
            FindText(promptRoot, "InteractionText"),
            GetOrAdd<CanvasGroup>(promptRoot.gameObject));
        return presenter;
    }

    private static List<RectTransform> FindHumanSelectionTargets(Transform root)
    {
        HashSet<string> names = new HashSet<string>
        {
            "Button_TODAY",
            "Button_DISPOSITION_HISTORY",
            "Button_SYSTEM_SETTINGS",
            "Button_ANSWER_LILY",
            "Button_COMPANION_ANSWER"
        };
        List<RectTransform> results = new List<RectTransform>();
        RectTransform[] rects = root.GetComponentsInChildren<RectTransform>(true);
        for (int i = 0; i < rects.Length; i++)
        {
            if (rects[i] != null && names.Contains(rects[i].name))
            {
                results.Add(rects[i]);
            }
        }
        return results;
    }

    private static RectTransform EnsureAuthoredSelectionFill(RectTransform target)
    {
        if (target == null) return null;
        GetOrAdd<RectMask2D>(target.gameObject);
        Transform existing = target.Find("SelectionFill");
        GameObject fillObject = existing != null
            ? existing.gameObject
            : new GameObject(
                "SelectionFill",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
        if (existing == null)
        {
            fillObject.layer = target.gameObject.layer;
            fillObject.transform.SetParent(target, false);
        }

        RectTransform fill = fillObject.GetComponent<RectTransform>();
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = Vector2.one;
        fill.pivot = new Vector2(0.5f, 0.5f);
        fill.offsetMin = new Vector2(8f, 8f);
        fill.offsetMax = new Vector2(-8f, -8f);
        Image image = GetOrAdd<Image>(fillObject);
        image.sprite = null;
        image.type = Image.Type.Simple;
        image.color = new Color32(120, 170, 220, 56);
        image.raycastTarget = false;
        fillObject.SetActive(false);
        fill.SetAsFirstSibling();
        return fill;
    }

    private static void RetireLegacyHumanPageVisuals(Transform root)
    {
        HashSet<string> retired = new HashSet<string>
        {
            "ShapeFill_001", "ShapeFill_004",
            "V2_FinalChoiceRuleA", "V2_FinalChoiceRuleB",
            "FinalChoiceFocus"
        };
        Transform[] items = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < items.Length; i++)
        {
            Transform item = items[i];
            if (retired.Contains(item.name) ||
                item.name.IndexOf("CANCEL", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                item.gameObject.SetActive(false);
            }
        }
    }

    private static void CreateOrRefreshTaskCatalog()
    {
        EnsureAssetFolder("Assets/Resources");
        EnsureAssetFolder("Assets/Resources/HEARTH");
        HearthTaskTextCatalog catalog =
            AssetDatabase.LoadAssetAtPath<HearthTaskTextCatalog>(TaskCatalogPath);
        if (catalog == null)
        {
            catalog = ScriptableObject.CreateInstance<HearthTaskTextCatalog>();
            AssetDatabase.CreateAsset(catalog, TaskCatalogPath);
        }

        HearthTaskTextCatalog.TaskEntry[] tasks =
        {
            Task(HearthCurrentTaskId.ListenToFieldUnit, "LISTEN TO FIELD UNIT"),
            Task(HearthCurrentTaskId.GoToAssignmentTerminal, "GO TO THE ASSIGNMENT TERMINAL"),
            Task(HearthCurrentTaskId.ReviewAssignments, "REVIEW TONIGHT'S ASSIGNMENTS"),
            Task(HearthCurrentTaskId.GoToElevator, "GO TO THE ELEVATOR"),
            Task(HearthCurrentTaskId.RideToFloor17, "RIDE TO FLOOR 17"),
            Task(HearthCurrentTaskId.GoToResidentTerminal, "GO TO TERMINAL {RESIDENT}"),
            Task(HearthCurrentTaskId.ReviewResidentProfile, "REVIEW HOUSEHOLD PROFILE"),
            Task(HearthCurrentTaskId.WaitForCompanionLink, "WAIT FOR COMPANION LINK"),
            Task(HearthCurrentTaskId.ReviewHouseholdEvent, "REVIEW RECORDED HOUSEHOLD EVENT"),
            Task(HearthCurrentTaskId.ReturnToResidentTerminal, "RETURN TO TERMINAL {RESIDENT}"),
            Task(HearthCurrentTaskId.ReviewHouseholdAnalysis, "REVIEW HOUSEHOLD ANALYSIS"),
            Task(HearthCurrentTaskId.SelectDisposition, "SELECT A DISPOSITION"),
            Task(HearthCurrentTaskId.ReturnHome, "RETURN HOME"),
            Task(HearthCurrentTaskId.UseHomeTerminal, "USE THE HOME TERMINAL"),
            Task(HearthCurrentTaskId.ReviewLilyMessage, "REVIEW LILY'S MESSAGE"),
            Task(HearthCurrentTaskId.InspectPhotoArchive, "INSPECT THE PHOTO ARCHIVE"),
            Task(HearthCurrentTaskId.GoToLilyRoom, "GO TO LILY'S ROOM"),
            Task(HearthCurrentTaskId.TalkToLily, "TALK TO LILY"),
            Task(HearthCurrentTaskId.MakeFinalResponse, "MAKE YOUR FINAL RESPONSE"),
            Task(HearthCurrentTaskId.ApproachHomeUnit, "APPROACH THE HOME UNIT"),
            Task(HearthCurrentTaskId.ConfirmShutdown, "CONFIRM COMPANION SHUTDOWN")
        };
        HearthTaskTextCatalog.CompanionSceneEntry[] scenes =
        {
            SceneTask("17F01_01", "APPROACH THE BEDSIDE AND COMFORT NOAH"),
            SceneTask("17F01_02", "OBSERVE THE HALLWAY AND FOLLOW THE PARENTS"),
            SceneTask("17F01_03", "LISTEN TO THE PARENTS IN THE LIVING ROOM"),
            SceneTask("17F02_01", "LISTEN TO CLAIRE"),
            SceneTask("17F02_02", "OFFER REASSURANCE"),
            SceneTask("17F02_03", "FOLLOW CLAIRE TO THE DINING ROOM"),
            SceneTask("17F02_04", "OBSERVE THE HOUSEHOLD QUERY"),
            SceneTask("17F02_05", "INITIATE SOFT GUIDANCE"),
            SceneTask("17F02_06", "LISTEN TO THE RECORDED ARGUMENT"),
            SceneTask("17F03_01", "OBSERVE THE FAMILY CONFLICT"),
            SceneTask("17F03_02", "FACE THE DAUGHTER AND RELAY THE MESSAGE"),
            SceneTask("17F03_03", "FACE THE MOTHER AND RELAY THE MESSAGE"),
            SceneTask("17F03_04", "OBSERVE THE SERVICE SUBJECT"),
            SceneTask("17F03_05", "CONFIRM MAINTENANCE SHUTDOWN")
        };
        catalog.Configure(tasks, scenes);
        EditorUtility.SetDirty(catalog);
    }

    private static void ApplyVisualOverridesToPrefab(
        GameObject instanceRoot,
        string prefabPath)
    {
        Transform[] transforms = instanceRoot.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            ApplyPropertiesToPrefab(
                transforms[i] as RectTransform,
                RectProperties,
                prefabPath);
            ApplyPropertiesToPrefab(
                transforms[i].GetComponent<TMP_Text>(),
                TmpProperties,
                prefabPath);
            ApplyPropertiesToPrefab(
                transforms[i].GetComponent<Image>(),
                ImageProperties,
                prefabPath);
            ApplyPropertiesToPrefab(
                transforms[i].GetComponent<Canvas>(),
                CanvasProperties,
                prefabPath);
        }
    }

    private static void ApplyPropertiesToPrefab(
        UnityEngine.Object target,
        string[] paths,
        string prefabPath)
    {
        if (target == null || !PrefabUtility.IsPartOfPrefabInstance(target)) return;
        SerializedObject serialized = new SerializedObject(target);
        for (int i = 0; i < paths.Length; i++)
        {
            SerializedProperty property = serialized.FindProperty(paths[i]);
            if (property != null && property.prefabOverride)
            {
                PrefabUtility.ApplyPropertyOverride(
                    property,
                    prefabPath,
                    InteractionMode.UserAction);
            }
        }
    }

    private static void RevertVisualTree(GameObject instanceRoot)
    {
        Transform[] transforms = instanceRoot.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            RevertProperties(transforms[i] as RectTransform, RectProperties);
            RevertProperties(transforms[i].GetComponent<TMP_Text>(), TmpProperties);
            RevertProperties(transforms[i].GetComponent<Image>(), ImageProperties);
            RevertProperties(transforms[i].GetComponent<Canvas>(), CanvasProperties);
        }
    }

    private static void RevertProperties(UnityEngine.Object target, string[] paths)
    {
        if (target == null || !PrefabUtility.IsPartOfPrefabInstance(target)) return;
        SerializedObject serialized = new SerializedObject(target);
        for (int i = 0; i < paths.Length; i++)
        {
            SerializedProperty property = serialized.FindProperty(paths[i]);
            if (property != null && property.prefabOverride)
            {
                PrefabUtility.RevertPropertyOverride(property, InteractionMode.UserAction);
            }
        }
    }

    private static bool IsVisualModification(PropertyModification modification)
    {
        if (modification == null || modification.target == null) return false;
        if (modification.target is RectTransform)
            return RectProperties.Contains(modification.propertyPath);
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
            "MinLoopTerminalPresenter", "TerminalUIController",
            "ResidentTerminalFlow", "ReplaySequenceController"
        };
        int count = 0;
        MonoBehaviour[] behaviours = FindSceneComponents<MonoBehaviour>(scene);
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] != null && behaviours[i].enabled &&
                names.Contains(behaviours[i].GetType().Name)) count++;
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

    private static GameObject GetSelectedCanonicalInstance()
    {
        GameObject selected = Selection.activeGameObject;
        GameObject root = selected != null
            ? PrefabUtility.GetNearestPrefabInstanceRoot(selected)
            : null;
        string path = GetSourcePrefabPath(root);
        if (root == null || !CanonicalPrefabs.Contains(path))
        {
            Debug.LogError(
                "[Production UI] Select a scene instance of one canonical V2 prefab first.");
            return null;
        }
        return root;
    }

    private static string GetSourcePrefabPath(GameObject instanceRoot)
    {
        if (instanceRoot == null) return string.Empty;
        return PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(instanceRoot);
    }

    private static void CreatePhotoArchivePrefabIfMissing()
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(PhotoArchivePrefab) != null)
        {
            return;
        }

        EnsureAssetFolder("Assets/Prefabs/UI/HearthHud/V2/PhotoArchive");
        GameObject root = new GameObject(
            "PhotoArchiveCanvas_V2",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(CanvasGroup),
            typeof(HearthPhotoArchiveViewBindings));
        try
        {
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.sizeDelta = new Vector2(1920f, 1080f);
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 80;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.dynamicPixelsPerUnit = 12f;
            root.GetComponent<GraphicRaycaster>().enabled = false;

            CreateArchiveText(
                rootRect,
                "ArchiveTitle",
                new Rect(240f, 70f, 620f, 64f),
                "PHOTO ARCHIVE",
                42f,
                new Color32(223, 235, 242, 255),
                TextAlignmentOptions.MidlineLeft,
                FontStyles.Bold);
            TMP_Text page = CreateArchiveText(
                rootRect,
                "ArchivePage",
                new Rect(240f, 730f, 320f, 48f),
                "PAGE 01 / 01",
                22f,
                new Color32(223, 235, 242, 255),
                TextAlignmentOptions.MidlineLeft,
                FontStyles.Bold);
            TMP_Text hint = CreateArchiveText(
                rootRect,
                "ArchiveReturnHint",
                new Rect(1360f, 730f, 320f, 48f),
                "SPACE  RETURN",
                22f,
                new Color32(223, 235, 242, 255),
                TextAlignmentOptions.MidlineRight,
                FontStyles.Bold);
            CreateArchiveRule(
                rootRect,
                "ArchiveBottomRule",
                new Rect(240f, 778f, 1440f, 2f));

            GameObject panel = CreateArchivePanel(
                rootRect,
                "FieldUnitPanel",
                new Rect(320f, 790f, 1280f, 190f));
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            TMP_Text speaker = CreateArchiveText(
                panelRect,
                "Speaker",
                new Rect(36f, 10f, 620f, 58f),
                "Field Unit",
                52f,
                new Color32(235, 176, 79, 255),
                TextAlignmentOptions.MidlineLeft,
                FontStyles.Bold);
            TMP_Text body = CreateArchiveText(
                panelRect,
                "Body",
                new Rect(36f, 72f, 1208f, 82f),
                string.Empty,
                26f,
                new Color32(223, 235, 242, 255),
                TextAlignmentOptions.TopLeft,
                FontStyles.Normal);
            body.enableWordWrapping = true;
            body.overflowMode = TextOverflowModes.Truncate;
            TMP_Text advance = CreateArchiveText(
                panelRect,
                "AdvanceHint",
                new Rect(910f, 150f, 334f, 32f),
                "SPACE  CONTINUE",
                26f,
                new Color32(116, 190, 232, 255),
                TextAlignmentOptions.MidlineRight,
                FontStyles.Bold);

            HearthDialogueSurface surface =
                panel.AddComponent<HearthDialogueSurface>();
            surface.Configure(
                panel.GetComponent<CanvasGroup>(),
                speaker,
                body,
                advance);
            root.GetComponent<HearthPhotoArchiveViewBindings>().Configure(
                canvas,
                root.GetComponent<CanvasGroup>(),
                page,
                hint,
                surface);
            PrefabUtility.SaveAsPrefabAsset(root, PhotoArchivePrefab);
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    private static GameObject CreateArchivePanel(
        RectTransform parent,
        string objectName,
        Rect rect)
    {
        GameObject panel = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Outline),
            typeof(CanvasGroup));
        panel.transform.SetParent(parent, false);
        ApplyTopLeft(panel.GetComponent<RectTransform>(), rect);
        Image image = panel.GetComponent<Image>();
        image.color = new Color32(8, 12, 24, 226);
        image.raycastTarget = false;
        Outline outline = panel.GetComponent<Outline>();
        outline.effectColor = new Color32(116, 190, 232, 255);
        outline.effectDistance = new Vector2(2f, -2f);
        return panel;
    }

    private static TMP_Text CreateArchiveText(
        RectTransform parent,
        string objectName,
        Rect rect,
        string value,
        float fontSize,
        Color color,
        TextAlignmentOptions alignment,
        FontStyles style)
    {
        GameObject target = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        target.transform.SetParent(parent, false);
        ApplyTopLeft(target.GetComponent<RectTransform>(), rect);
        TMP_Text text = target.GetComponent<TMP_Text>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.fontStyle = style;
        text.raycastTarget = false;
        return text;
    }

    private static void CreateArchiveRule(
        RectTransform parent,
        string objectName,
        Rect rect)
    {
        GameObject rule = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        rule.transform.SetParent(parent, false);
        ApplyTopLeft(rule.GetComponent<RectTransform>(), rect);
        Image image = rule.GetComponent<Image>();
        image.color = new Color32(116, 190, 232, 255);
        image.raycastTarget = false;
    }

    private static void ApplyTopLeft(RectTransform target, Rect rect)
    {
        target.anchorMin = new Vector2(0f, 1f);
        target.anchorMax = new Vector2(0f, 1f);
        target.pivot = new Vector2(0f, 1f);
        target.anchoredPosition = new Vector2(rect.x, -rect.y);
        target.sizeDelta = new Vector2(rect.width, rect.height);
    }

    private static void EditPrefab(string path, Action<GameObject> edit)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
        {
            Debug.LogError("[Production UI] Cannot edit missing prefab: " + path);
            return;
        }
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            edit(root);
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        return component != null ? component : target.AddComponent<T>();
    }

    private static void SplitCurrentTaskRects(TMP_Text headingText, TMP_Text bodyText)
    {
        RectTransform heading = headingText != null ? headingText.rectTransform : null;
        RectTransform body = bodyText != null ? bodyText.rectTransform : null;
        if (heading == null || body == null) return;
        float totalHeight = Mathf.Max(56f, heading.sizeDelta.y);
        Vector2 originalPosition = heading.anchoredPosition;
        float originalTop = originalPosition.y + totalHeight * (1f - heading.pivot.y);
        heading.sizeDelta = new Vector2(heading.sizeDelta.x, 28f);
        heading.anchoredPosition = new Vector2(
            originalPosition.x,
            originalTop - 28f * (1f - heading.pivot.y));
        body.sizeDelta = new Vector2(body.sizeDelta.x, Mathf.Max(24f, totalHeight - 34f));
        body.anchoredPosition = new Vector2(
            originalPosition.x,
            originalTop - 34f - body.sizeDelta.y * (1f - body.pivot.y));
        headingText.alignment = TextAlignmentOptions.TopRight;
        bodyText.alignment = TextAlignmentOptions.TopRight;
    }

    private static HearthTaskTextCatalog.TaskEntry Task(
        HearthCurrentTaskId id,
        string text)
    {
        return new HearthTaskTextCatalog.TaskEntry { taskId = id, text = text };
    }

    private static HearthTaskTextCatalog.CompanionSceneEntry SceneTask(
        string id,
        string text)
    {
        return new HearthTaskTextCatalog.CompanionSceneEntry
        {
            sceneId = id,
            text = text
        };
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

    private static void RecordAndBind(UnityEngine.Object target, Action bind)
    {
        Undo.RecordObject(target, "Bind HEARTH canonical UI view");
        bind();
        PrefabUtility.RecordPrefabInstancePropertyModifications(target);
        EditorUtility.SetDirty(target);
    }

    private static bool BindTransitionService(
        MonoBehaviour owner,
        string canvasGroupProperty,
        bool useUnscaledTime,
        Action<HearthScreenTransitionService> bind)
    {
        SerializedObject serialized = new SerializedObject(owner);
        SerializedProperty groupProperty =
            serialized.FindProperty(canvasGroupProperty);
        CanvasGroup group = groupProperty != null
            ? groupProperty.objectReferenceValue as CanvasGroup
            : null;
        if (group == null)
        {
            Debug.LogError(
                "[Production UI] Cannot bind transition service because the authored " +
                "blackout CanvasGroup is missing: " + GetHierarchyPath(owner.transform),
                owner);
            return false;
        }

        HearthScreenTransitionService service =
            group.GetComponent<HearthScreenTransitionService>();
        if (service == null)
        {
            service = Undo.AddComponent<HearthScreenTransitionService>(group.gameObject);
        }
        Undo.RecordObject(service, "Configure HEARTH screen transition service");
        service.Configure(
            group.GetComponent<HearthScreenFader>(),
            group,
            useUnscaledTime);
        EditorUtility.SetDirty(service);
        RecordAndBind(owner, () => bind(service));
        return true;
    }

    private static bool IsHumanInteraction(PlayerInteraction interaction)
    {
        return interaction != null &&
            (interaction.name == "Person Controller" ||
             GetHierarchyPath(interaction.transform).Contains("Player/Person Controller"));
    }

    private static bool IsCompanionInteraction(PlayerInteraction interaction)
    {
        return interaction != null &&
            (interaction.name == "Robot Controller" ||
             GetHierarchyPath(interaction.transform).Contains("Player/Robot Controller"));
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

    private static void EnsureAssetFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int split = path.LastIndexOf('/');
        string parent = path.Substring(0, split);
        string name = path.Substring(split + 1);
        EnsureAssetFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }
}
#endif
