#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Deterministic final-pass repair for the V2 visual roots. This tool edits
/// only visual children and leaves gameplay wrappers, page components and
/// scene bindings intact. It intentionally does not invoke the old Closure
/// Builder.
/// </summary>
public static class HearthUiV2FinalVisualRepairEditor
{
    private const string MenuRoot = "Tools/Hearth/UI V2/Final Repair/";
    private const string HumanPrefab =
        "Assets/Prefabs/UI/HearthHud/V2/HearthHudRoot_V2.prefab";
    private const string CompanionPrefab =
        "Assets/Prefabs/UI/HearthHud/V2/Companion/HearthCompanionHudRoot_V2.prefab";
    private const string SubtitlePrefab =
        "Assets/Prefabs/UI/HearthSubtitle/V2/HearthSubtitleVisualCanvas_V2.prefab";
    private const string TerminalFolder =
        "Assets/Prefabs/UI/HearthHud/V2/Terminals/";
    private const string VectorRoot =
        "Assets/UI/HEARTH/V2/VectorParts/";

    private const string FullscreenFrame = VectorRoot +
        "Companion/HUD_Companion_FullscreenFrame_1920x1080.png";
    private const string StatusFrame = VectorRoot +
        "Common/HUD_Common_StatusFrame_520x240.png";
    private const string DecisionFrame = VectorRoot +
        "Common/HUD_Common_DecisionFrame_520x216.png";
    private const string DialogueFrame = VectorRoot +
        "Common/HUD_Common_DialogueFrame_960x256.png";
    private const string SpeakerLeft = VectorRoot +
        "Common/HUD_Common_SpeakerTab_Left_340x48.png";
    private const string SpeakerRight = VectorRoot +
        "Common/HUD_Common_SpeakerTab_Right_340x48.png";
    private const string AuxiliaryFrame = VectorRoot +
        "Feedback/HUD_Feedback_FieldUnitToastFrame_640x400.png";
    private const string MenuButtonFrame = VectorRoot +
        "Common/HUD_Common_ButtonFrame_540x84.png";
    private const string HumanTabPageFrame = VectorRoot +
        "Human/HUD_Human_TabPageFrame_1120x760.png";
    private const string HumanContentFrame = VectorRoot +
        "Human/HUD_Human_ContentFrame_860x420.png";
    private const string HumanMetricFrame = VectorRoot +
        "Human/HUD_Human_MetricFrame_860x132.png";
    private const string TerminalInfoFrame = VectorRoot +
        "Terminal/HUD_Terminal_InfoPanelFrame_620x260.png";
    private const string TerminalFieldFrame = VectorRoot +
        "Terminal/HUD_Terminal_FieldUnitFrame_620x190.png";
    private const string TerminalPortraitFrame = VectorRoot +
        "Terminal/HUD_Terminal_PortraitFrame_240x400.png";
    private const string TerminalTabFrame = VectorRoot +
        "Terminal/HUD_Terminal_TabFrame_310x52.png";
    private const string TerminalPrimaryTabFrame = VectorRoot +
        "Terminal/HUD_Terminal_PrimaryTabFrame_570x52.png";

    private static readonly Color DeepBlueBlack =
        new Color32(11, 16, 24, 244);
    private static readonly Color PanelBlueBlack =
        new Color32(9, 16, 28, 210);
    private static readonly Color GreyBlue =
        new Color32(95, 120, 149, 255);
    private static readonly Color ColdWhite =
        new Color32(215, 230, 246, 255);
    private static readonly Color AccentBlue =
        new Color32(120, 170, 220, 230);
    private static readonly Color Amber =
        new Color32(224, 151, 44, 255);
    private static readonly Color RecRed =
        new Color32(235, 58, 48, 255);

    [MenuItem(MenuRoot + "Apply Visual and Structure Repair")]
    public static void ApplyAll()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError(
                "[HearthUiV2FinalVisualRepairEditor] Exit Play Mode before " +
                "repairing prefab visuals.");
            return;
        }

        HearthUiV2VectorAssetEditor.PrepareImportedSprites();
        EditPrefab(HumanPrefab, RepairHuman);
        EditPrefab(CompanionPrefab, RepairCompanion);
        EditPrefab(SubtitlePrefab, RepairSubtitle);
        RepairTerminalPrefab(
            TerminalFolder + "Terminal_Lobby_Assignment_V2.prefab",
            HearthTerminalMode.LobbySync);
        RepairTerminalPrefab(
            TerminalFolder + "Terminal_17F01_V2.prefab",
            HearthTerminalMode.Doorway);
        RepairTerminalPrefab(
            TerminalFolder + "Terminal_17F02_V2.prefab",
            HearthTerminalMode.Doorway);
        RepairTerminalPrefab(
            TerminalFolder + "Terminal_17F03_Alert_V2.prefab",
            HearthTerminalMode.Doorway);
        RepairTerminalPrefab(
            TerminalFolder + "Terminal_17F04_Home_V2.prefab",
            HearthTerminalMode.Home);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        Debug.Log(
            "[HearthUiV2FinalVisualRepairEditor] V2 visual and structure " +
            "repair applied without rebuilding gameplay wrappers.");
    }

    [MenuItem(MenuRoot + "Validate Repaired Prefabs")]
    public static void ValidateRepairedPrefabs()
    {
        List<string> issues = new List<string>();
        ValidateCompanion(issues);
        ValidateSubtitle(issues);
        ValidateHuman(issues);
        ValidateTerminal(
            TerminalFolder + "Terminal_Lobby_Assignment_V2.prefab",
            issues);
        ValidateTerminal(
            TerminalFolder + "Terminal_17F01_V2.prefab",
            issues);
        ValidateTerminal(
            TerminalFolder + "Terminal_17F02_V2.prefab",
            issues);
        ValidateTerminal(
            TerminalFolder + "Terminal_17F03_Alert_V2.prefab",
            issues);
        ValidateTerminal(
            TerminalFolder + "Terminal_17F04_Home_V2.prefab",
            issues);

        string[] sceneDataGuids = AssetDatabase.FindAssets(
            "t:HearthCompanionHudSceneData",
            new[] { "Assets/Data/HearthHud/Companion" });
        int missingTaskMappings = 0;
        for (int i = 0; i < sceneDataGuids.Length; i++)
        {
            HearthCompanionHudSceneData scene =
                AssetDatabase.LoadAssetAtPath<HearthCompanionHudSceneData>(
                    AssetDatabase.GUIDToAssetPath(sceneDataGuids[i]));
            if (scene != null && string.IsNullOrWhiteSpace(scene.CurrentTask))
            {
                missingTaskMappings++;
            }
        }

        if (issues.Count == 0)
        {
            Debug.Log(
                "[HearthUiV2FinalVisualRepairEditor] Prefab validation " +
                "passed. Companion task-copy TODO entries: " +
                missingTaskMappings + ".");
        }
        else
        {
            Debug.LogError(
                "[HearthUiV2FinalVisualRepairEditor] Validation found " +
                issues.Count + " issue(s):\n- " +
                string.Join("\n- ", issues));
        }
    }

    private static void RepairHuman(GameObject root)
    {
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode =
                CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        HearthUiStateCoordinator coordinator =
            root.GetComponentInChildren<HearthUiStateCoordinator>(true);
        if (coordinator == null)
        {
            coordinator = root.AddComponent<HearthUiStateCoordinator>();
        }
        coordinator.SetRuntimeIntegration(true, false);

        SetBottomLeft(FindRect(root.transform, "LocationHud"), 64f, 48f, 340f, 92f);

        TMP_Text currentTaskTitle =
            FindText(root.transform, "Text_006_CURRENT_TASK");
        if (currentTaskTitle != null)
        {
            currentTaskTitle.text = "CURRENT TASK";
        }

        TMP_Text unapprovedTaskBody =
            FindText(
                root.transform,
                "Text_007_NIGHT_ROUNDS___BLOCK_A___17F");
        if (unapprovedTaskBody != null)
        {
            unapprovedTaskBody.text = string.Empty;
            unapprovedTaskBody.gameObject.SetActive(false);
        }

        Transform menu = FindNamed(root.transform, "Slide03_MainMenu");
        if (menu != null)
        {
            SetNamedActiveAll(menu, "MenuFocus", false);
            SetNamedActiveAll(menu, "V2_MenuButtonRule", false);

            // The stage-specific task mapping has not been approved yet.
            // Keep only the heading in the Human Tab summary as well as in
            // the persistent HUD; preview copy must never invent gameplay
            // objectives.
            TMP_Text menuTaskBody = FindText(menu, "V2_MenuTaskBody");
            if (menuTaskBody != null)
            {
                menuTaskBody.text = string.Empty;
                menuTaskBody.gameObject.SetActive(false);
            }
        }
        SetNamedActiveAll(root.transform, "MenuFocus", false);

        RepairMenuButton(root.transform, "Button_TODAY");
        RepairMenuButton(root.transform, "Button_DISPOSITION_HISTORY");
        RepairMenuButton(root.transform, "Button_SYSTEM_SETTINGS");

        RepairHumanTabPage(
            root.transform,
            "Slide05_TodayRounds",
            "V2_TodayRoundsContentFrame",
            new Rect(130f, 238f, 860f, 420f),
            null,
            default(Rect));
        string[] historyPages =
        {
            "Slide18_HistoryEmpty", "Slide19_HistoryOne",
            "Slide20_HistoryTwo", "Slide21_HistoryThree"
        };
        for (int i = 0; i < historyPages.Length; i++)
        {
            RepairHumanTabPage(
                root.transform,
                historyPages[i],
                "V2_HistoryContentFrame",
                new Rect(130f, 230f, 860f, 320f),
                "V2_HistoryMetricsFrame",
                new Rect(130f, 560f, 860f, 150f));
        }
        RepairHumanTabPage(
            root.transform,
            "Slide22_Settings",
            "V2_SettingsContentFrame",
            new Rect(130f, 220f, 860f, 340f),
            "V2_SettingsFooterFrame",
            new Rect(130f, 570f, 860f, 150f));
        RepairHumanTabPage(
            root.transform,
            "Slide23_SettingsFocus",
            "V2_SettingsContentFrame",
            new Rect(130f, 220f, 860f, 340f),
            "V2_SettingsFooterFrame",
            new Rect(130f, 570f, 860f, 150f));

        RepairPhotoPage(root.transform, "Slide07_Photo2023");
        RepairPhotoPage(root.transform, "Slide08_Photo2026");
        RepairFinalChoicePage(root.transform, "Slide09_FinalChoice");
        RepairFinalChoicePage(
            root.transform,
            "Slide14_FinalChoiceReturn");
        RepairShutdownPage(root.transform);
    }

    private static void RepairFinalChoicePage(
        Transform root,
        string pageName)
    {
        Transform page = FindNamed(root, pageName);
        if (page == null)
        {
            return;
        }

        SetNamedActiveAll(page, "ShapeFill_001", false);
        SetNamedActiveAll(page, "ShapeFill_004", false);
        SetNamedActiveAll(page, "V2_FinalChoiceRuleA", false);
        SetNamedActiveAll(page, "V2_FinalChoiceRuleB", false);
        SetNamedActiveAll(page, "Border_Left", false);
        SetNamedActiveAll(page, "Border_Right", false);
        SetNamedActiveAll(page, "Border_Top", false);
        SetNamedActiveAll(page, "Border_Bottom", false);

        RepairChoiceButton(page, "Button_ANSWER_LILY");
        RepairChoiceButton(page, "Button_COMPANION_ANSWER");
        SetNamedActiveAll(root, "FinalChoiceFocus", false);
    }

    private static void RepairChoiceButton(Transform page, string name)
    {
        RectTransform button = FindRect(page, name);
        if (button == null)
        {
            return;
        }

        if (button.GetComponent<RectMask2D>() == null)
        {
            button.gameObject.AddComponent<RectMask2D>();
        }

        DestroyDirectChildren(button, "SelectionFill");
        Image selection = CreateImage(
            button,
            "SelectionFill",
            new Color(AccentBlue.r, AccentBlue.g, AccentBlue.b, 0.22f));
        SetStretch(selection.rectTransform, 8f, 8f, 8f, 8f);
        selection.transform.SetAsFirstSibling();
        selection.gameObject.SetActive(false);

        Image rootImage = button.GetComponent<Image>();
        if (rootImage != null)
        {
            rootImage.color =
                new Color(GreyBlue.r, GreyBlue.g, GreyBlue.b, 0.13f);
        }
    }

    private static void RepairShutdownPage(Transform root)
    {
        Transform page = FindNamed(root, "Slide10_ShutdownConfirm");
        if (page == null)
        {
            return;
        }

        SetTopLeft(
            FindRect(page, "Button_CONFIRM"),
            630f,
            686f,
            660f,
            106f);
        SetTopLeft(
            FindRect(page, "Text_005_CONFIRM"),
            690f,
            712f,
            540f,
            54f);

        Transform[] items = page.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < items.Length; i++)
        {
            Transform item = items[i];
            if (item == null ||
                item.name.IndexOf(
                    "CANCEL",
                    StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }

            Button cancel = item.GetComponent<Button>();
            if (cancel != null)
            {
                cancel.interactable = false;
            }
            Graphic[] graphics = item.GetComponentsInChildren<Graphic>(true);
            for (int g = 0; g < graphics.Length; g++)
            {
                graphics[g].raycastTarget = false;
            }
            CanvasGroup group = item.GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.interactable = false;
                group.blocksRaycasts = false;
            }
            item.gameObject.SetActive(false);
        }
    }

    private static void RepairHumanTabPage(
        Transform root,
        string pageName,
        string contentFrameName,
        Rect contentRect,
        string metricFrameName,
        Rect metricRect)
    {
        Transform page = FindNamed(root, pageName);
        if (page == null)
        {
            return;
        }

        RectTransform shell = FindRect(page, "V2_PagePanel");
        if (shell == null)
        {
            return;
        }

        SetTopLeft(shell, 400f, 160f, 1120f, 760f);
        EnsurePanelLayers(shell, HumanTabPageFrame);
        SetNamedActiveAll(shell, "V2_TopRule", false);

        DestroyDirectChildren(shell, contentFrameName);
        Image content = CreateSpriteImage(
            shell,
            contentFrameName,
            HumanContentFrame,
            AccentBlue);
        SetTopLeft(
            content.rectTransform,
            contentRect.x,
            contentRect.y,
            contentRect.width,
            contentRect.height);

        if (!string.IsNullOrWhiteSpace(metricFrameName))
        {
            DestroyDirectChildren(shell, metricFrameName);
            Image metric = CreateSpriteImage(
                shell,
                metricFrameName,
                HumanMetricFrame,
                AccentBlue);
            SetTopLeft(
                metric.rectTransform,
                metricRect.x,
                metricRect.y,
                metricRect.width,
                metricRect.height);
        }

        string[] legacyBorders =
        {
            "Border_Top", "Border_Bottom", "Border_Left", "Border_Right"
        };
        for (int i = 0; i < legacyBorders.Length; i++)
        {
            SetNamedActiveAll(page, legacyBorders[i], false);
        }
    }

    private static void RepairPhotoPage(Transform root, string pageName)
    {
        Transform page = FindNamed(root, pageName);
        if (page == null)
        {
            return;
        }

        // One continuous modal surface contains the image, metadata,
        // Field Unit note, page number and return action.
        SetTopLeft(FindRect(page, "V2_PagePanel"), 260f, 132f, 1420f, 908f);
        SetTopLeft(FindRect(page, "V2_PhotoMetadata"), 398f, 680f, 520f, 124f);
        SetTopLeft(FindRect(page, "V2_PhotoFieldUnit"), 432f, 830f, 1056f, 132f);
        SetTopLeft(FindRect(page, "V2_PhotoFieldUnitFrame"), 432f, 830f, 1056f, 132f);
        SetTopLeft(FindRect(page, "V2_PhotoPage"), 312f, 990f, 220f, 32f);
        SetTopLeft(FindRect(page, "Button_CloseStory"), 1342f, 978f, 336f, 52f);

        // These four imported rules belonged to the original slide shell.
        // Leaving one of them visible created the long cyan line below the
        // photo in the rebuilt modal.
        SetNamedActiveAll(page, "Border_Top", false);
        SetNamedActiveAll(page, "Border_Bottom", false);
        SetNamedActiveAll(page, "Border_Left", false);
        SetNamedActiveAll(page, "Border_Right", false);
        SetNamedActiveAll(page, "V2_MetadataRule", false);
    }

    private static void RepairMenuButton(Transform root, string name)
    {
        RectTransform button = FindRect(root, name);
        if (button == null)
        {
            return;
        }

        DestroyDirectChildren(button, "SelectionFill");
        DestroyDirectChildren(button, "V2_ButtonFill");
        DestroyDirectChildren(button, "V2_VectorButtonFrame");
        DestroyDirectChildren(button, "ButtonFrame");

        Image rootImage = button.GetComponent<Image>();
        if (rootImage != null)
        {
            rootImage.color = Color.clear;
        }

        Image selection = CreateImage(
            button,
            "SelectionFill",
            new Color(AccentBlue.r, AccentBlue.g, AccentBlue.b, 0.22f));
        SetStretch(selection.rectTransform, 8f, 8f, 8f, 8f);
        selection.transform.SetAsFirstSibling();
        selection.gameObject.SetActive(false);

        Image frame = CreateSpriteImage(
            button,
            "ButtonFrame",
            MenuButtonFrame,
            AccentBlue);
        SetStretch(frame.rectTransform, 0f, 0f, 0f, 0f);
        frame.transform.SetAsLastSibling();
    }

    private static void RepairCompanion(GameObject root)
    {
        HearthCompanionHudExclusiveMode exclusive =
            root.GetComponentInChildren<HearthCompanionHudExclusiveMode>(true);
        if (exclusive != null)
        {
            UnityEngine.Object.DestroyImmediate(exclusive);
        }

        HearthCompanionHudLayoutController legacyLayout =
            root.GetComponentInChildren<HearthCompanionHudLayoutController>(true);
        if (legacyLayout != null)
        {
            UnityEngine.Object.DestroyImmediate(legacyLayout);
        }

        CanvasGroup group = root.GetComponent<CanvasGroup>();
        if (group != null)
        {
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }

        RectTransform fullscreen =
            FindRect(root.transform, "CompanionRobotFrame");
        if (fullscreen != null)
        {
            SetStretch(fullscreen, 20f, 20f, 20f, 20f);
            AssignSprite(fullscreen.GetComponent<Image>(), FullscreenFrame, AccentBlue);
            fullscreen.SetAsFirstSibling();
        }

        TMP_Text identity = FindText(root.transform, "V2_Identity");
        SetTopLeft(identity != null ? identity.rectTransform : null, 60f, 42f, 430f, 88f);
        ConfigureText(identity, 20f, TextAlignmentOptions.TopLeft, ColdWhite, FontStyles.Normal);

        RectTransform identityRule =
            FindRect(root.transform, "V2_IdentityUnderline");
        SetTopLeft(identityRule, 60f, 132f, 430f, 2f);
        ConfigureRule(identityRule, AccentBlue);

        TMP_Text rec = FindText(root.transform, "V2_REC");
        SetTopCenter(rec != null ? rec.rectTransform : null, 0f, 28f, 220f, 30f);
        ConfigureText(rec, 20f, TextAlignmentOptions.Center, RecRed, FontStyles.Bold);
        if (rec != null)
        {
            rec.text = "●  REC";
            rec.gameObject.SetActive(true);
        }

        TMP_Text task = FindText(root.transform, "V2_CurrentTask");
        SetTopRight(task != null ? task.rectTransform : null, 60f, 42f, 520f, 88f);
        ConfigureText(task, 20f, TextAlignmentOptions.TopRight, ColdWhite, FontStyles.Normal);
        if (task != null)
        {
            task.text = "CURRENT TASK";
        }

        RectTransform status = FindRect(root.transform, "V2_StatusPanel");
        SetTopLeft(status, 52f, 160f, 520f, 240f);
        EnsurePanelLayers(status, StatusFrame);
        SetLocalTopLeft(FindText(status, "V2_StatusTitleText"), 24f, 20f, 472f, 34f);
        SetLocalTopLeft(FindText(status, "V2_StatusRowsText"), 24f, 70f, 472f, 110f);
        SetLocalTopLeft(FindText(status, "V2_StatusFooterText"), 24f, 198f, 472f, 24f);

        RectTransform trigger = FindRect(root.transform, "TriggerCardView");
        SetTopLeft(trigger, 52f, 160f, 520f, 240f);
        EnsurePanelLayers(trigger, StatusFrame);
        SetNamedActiveAll(trigger, "V2_Backplate", false);
        SetNamedActiveAll(trigger, "TriggerCardAccent", false);
        SetNamedActiveAll(trigger, "V2_TriggerRule", false);
        SetNamedActiveAll(trigger, "V2_StatusRule", false);
        TMP_Text triggerTitle = FindText(trigger, "TriggerCardTitleText");
        TMP_Text triggerBody = FindText(trigger, "TriggerCardBodyText");
        SetLocalTopLeft(triggerTitle, 24f, 20f, 472f, 34f);
        SetLocalTopLeft(triggerBody, 24f, 70f, 472f, 150f);
        ConfigureText(
            triggerTitle,
            20f,
            TextAlignmentOptions.TopLeft,
            AccentBlue,
            FontStyles.Bold);
        ConfigureText(
            triggerBody,
            18f,
            TextAlignmentOptions.TopLeft,
            ColdWhite,
            FontStyles.Normal);

        RectTransform decision = FindRect(root.transform, "DecisionPanel");
        SetTopLeft(decision, 1348f, 160f, 520f, 216f);
        EnsurePanelLayers(decision, DecisionFrame);
        SetLocalTopLeft(FindText(decision, "DecisionKickerText"), 24f, 20f, 472f, 26f);
        SetLocalTopLeft(FindText(decision, "DecisionTitleText"), 24f, 54f, 472f, 36f);
        SetLocalTopLeft(FindText(decision, "DecisionBodyText"), 24f, 102f, 472f, 84f);

        SetTopCenter(
            FindRect(root.transform, "CenterMessageText"),
            0f,
            432f,
            760f,
            88f);
        SetBottomCenter(
            FindRect(root.transform, "ModeLabelText"),
            0f,
            42f,
            760f,
            28f);

        string[] hiddenNames =
        {
            "V2_Status", "V2_StatusBackplate", "V2_PhysicalFeedLabel",
            "V2_PhysicalFeedRule", "V2_InspectionHeading",
            "V2_InspectionHeadingRule", "V2_InspectionUnit",
            "V2_InspectionReturn", "DataStreamView", "DecisionAccent",
            "V2_StatusAccent", "V2_DecisionRule", "V2_StatusRule",
            "Border_Left", "Border_Right", "Border_Top", "Border_Bottom",
            "V2_VectorPanelFrame"
        };
        for (int i = 0; i < hiddenNames.Length; i++)
        {
            SetNamedActiveAll(root.transform, hiddenNames[i], false);
        }
    }

    private static void RepairSubtitle(GameObject root)
    {
        Transform visual = FindNamed(root.transform, "VisualRoot");
        if (visual == null)
        {
            return;
        }

        Image backdrop = FindImage(visual, "Backdrop");
        if (backdrop != null)
        {
            backdrop.sprite = null;
            backdrop.color = new Color(
                PanelBlueBlack.r,
                PanelBlueBlack.g,
                PanelBlueBlack.b,
                0.82f);
            SetTopLeft(backdrop.rectTransform, 486f, 676f, 948f, 244f);
            backdrop.transform.SetAsFirstSibling();
        }

        Image formal = EnsureSpriteImage(visual, "FormalFrame", DialogueFrame, AccentBlue);
        Image auxiliary = EnsureSpriteImage(visual, "AuxiliaryFrame", AuxiliaryFrame, AccentBlue);
        Image left = EnsureSpriteImage(visual, "SpeakerTabLeft", SpeakerLeft, AccentBlue);
        Image right = EnsureSpriteImage(visual, "SpeakerTabRight", SpeakerRight, AccentBlue);
        SetTopLeft(formal.rectTransform, 480f, 670f, 960f, 256f);
        SetTopLeft(auxiliary.rectTransform, 1216f, 214f, 640f, 400f);
        SetTopLeft(left.rectTransform, 480f, 622f, 340f, 48f);
        SetTopLeft(right.rectTransform, 1100f, 622f, 340f, 48f);
        formal.gameObject.SetActive(false);
        auxiliary.gameObject.SetActive(false);
        left.gameObject.SetActive(false);
        right.gameObject.SetActive(false);
        SetNamedActiveAll(visual, "AccentRule", false);
        SetNamedActiveAll(visual, "SpeakerTab", false);
    }

    private static void RepairTerminalPrefab(string path, HearthTerminalMode mode)
    {
        EditPrefab(path, root => RepairTerminal(root, mode));
    }

    private static void RepairTerminal(GameObject root, HearthTerminalMode mode)
    {
        HearthTvTerminalController terminal =
            root.GetComponent<HearthTvTerminalController>() ??
            root.GetComponentInChildren<HearthTvTerminalController>(true);
        if (terminal == null)
        {
            return;
        }
        terminal.SetTerminalMode(mode);

        Transform content = FindNamed(root.transform, "TerminalContentRoot");
        Transform keyboard = FindNamed(root.transform, "KeyboardNavigationRoot");
        Transform oldVisual = FindNamed(root.transform, "TerminalVisualRoot");
        if (oldVisual != null)
        {
            if (content != null && content.IsChildOf(oldVisual))
            {
                content.SetParent(root.transform, false);
            }
            if (keyboard != null && keyboard.IsChildOf(oldVisual))
            {
                keyboard.SetParent(root.transform, false);
            }
            UnityEngine.Object.DestroyImmediate(oldVisual.gameObject);
        }
        DestroyAllNamed(root.transform, "V2_ClosureTerminalChrome");

        GameObject visualObject = new GameObject(
            "TerminalVisualRoot",
            typeof(RectTransform));
        visualObject.layer = root.layer;
        visualObject.transform.SetParent(root.transform, false);
        RectTransform visual = visualObject.GetComponent<RectTransform>();
        SetStretch(visual, 0f, 0f, 0f, 0f);

        Image backdrop = CreateImage(visual, "InteriorBackdrop", DeepBlueBlack);
        SetStretch(backdrop.rectTransform, 0f, 0f, 0f, 0f);
        backdrop.transform.SetAsFirstSibling();

        if (content != null)
        {
            content.SetParent(visual, false);
            SetStretch(content as RectTransform, 0f, 0f, 0f, 0f);
            content.SetSiblingIndex(1);
            ClearLegacyTerminalVisuals(content);
        }
        if (keyboard != null)
        {
            keyboard.SetParent(visual, false);
        }

        if (mode == HearthTerminalMode.LobbySync)
        {
            RepairLobbyTerminal(content, keyboard);
            return;
        }

        if (keyboard != null)
        {
            keyboard.gameObject.SetActive(false);
        }
        BuildTerminalChrome(root, visual, terminal, mode);

        if (mode == HearthTerminalMode.Home)
        {
            RepairHomeContent(content);
        }
        else
        {
            RepairDoorwayContent(content);
        }
    }

    private static void ClearLegacyTerminalVisuals(Transform content)
    {
        Transform[] items = content.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < items.Length; i++)
        {
            Transform item = items[i];
            string name = item.name;
            Canvas canvas = item.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.overrideSorting = false;
            }

            Image image = item.GetComponent<Image>();
            if (image != null &&
                (name == "ScreenSurface" || name == "V2_PageVisual" ||
                 name == "OffScreen" || name == "ConfirmBack"))
            {
                image.color = Color.clear;
                image.sprite = null;
            }

            bool hide =
                name == "TerminalLabel" || name == "ResidentId" ||
                name == "HeaderRule" || name == "AccentRule" ||
                name == "NavigationRule" || name == "SelectionRule" ||
                name == "ChoiceRule" || name == "V2_FooterRule" ||
                name == "TopRule" || name == "LeftRule" ||
                name == "CornerRuleA" || name == "CornerRuleB" ||
                name == "BorderLeft" || name == "BorderRight" ||
                name == "BorderTop" || name == "BorderBottom" ||
                name == "Selected" || name == "BeforeTab" ||
                name == "AfterTab" || name == "PrimaryActionTab" ||
                name.StartsWith("Tab_", StringComparison.Ordinal) ||
                name.StartsWith("Scanline_", StringComparison.Ordinal);
            if (hide)
            {
                item.gameObject.SetActive(false);
            }
        }
    }

    private static void RepairLobbyTerminal(
        Transform content,
        Transform keyboard)
    {
        if (keyboard != null)
        {
            keyboard.gameObject.SetActive(true);
            SetBottomLeft(keyboard as RectTransform, 76f, 48f, 1768f, 58f);
            TMP_Text hint = FindText(keyboard, "KeyboardHintText");
            if (hint != null)
            {
                hint.text = "SPACE  CLOSE TERMINAL";
                ConfigureText(
                    hint,
                    18f,
                    TextAlignmentOptions.Center,
                    GreyBlue,
                    FontStyles.Bold);
            }
            TMP_Text focus = FindText(keyboard, "KeyboardFocusText");
            if (focus != null)
            {
                focus.text = string.Empty;
            }
        }

        RectTransform slide = FindRect(content, "TerminalSlide01_LobbyAssignment");
        SetStretch(slide, 0f, 0f, 0f, 0f);
    }

    private static void BuildTerminalChrome(
        GameObject prefabRoot,
        RectTransform visual,
        HearthTvTerminalController terminal,
        HearthTerminalMode mode)
    {
        GameObject chromeObject = new GameObject("ChromeRoot", typeof(RectTransform));
        chromeObject.layer = prefabRoot.layer;
        chromeObject.transform.SetParent(visual, false);
        RectTransform chrome = chromeObject.GetComponent<RectTransform>();
        SetStretch(chrome, 0f, 0f, 0f, 0f);
        chrome.SetAsLastSibling();

        TMP_Text terminalLabel = CreateText(chrome, "TerminalLabel", "DOORWAY TERMINAL");
        SetTopLeft(terminalLabel.rectTransform, 76f, 56f, 420f, 30f);
        ConfigureText(terminalLabel, 22f, TextAlignmentOptions.TopLeft, ColdWhite, FontStyles.Bold);

        TMP_Text resident = CreateText(chrome, "ResidentId", "17F-01");
        SetTopLeft(resident.rectTransform, 76f, 90f, 300f, 42f);
        ConfigureText(resident, 30f, TextAlignmentOptions.TopLeft, ColdWhite, FontStyles.Normal);

        TMP_Text status = CreateText(chrome, "Status", string.Empty);
        SetTopRight(status.rectTransform, 76f, 56f, 520f, 34f);
        ConfigureText(status, 18f, TextAlignmentOptions.TopRight, GreyBlue, FontStyles.Bold);

        TabVisual before = CreateTerminalTab(
            chrome, "BeforeTab", TerminalTabFrame, 318f, 142f, 310f, 52f,
            "BEFORE ACQUISITION");
        TabVisual after = CreateTerminalTab(
            chrome, "AfterTab", TerminalTabFrame, 652f, 142f, 310f, 52f,
            "AFTER ACQUISITION");
        TabVisual primary = CreateTerminalTab(
            chrome, "PrimaryActionTab", TerminalPrimaryTabFrame,
            1274f, 142f, 570f, 52f,
            mode == HearthTerminalMode.Home ? "ENTER HOME" : "PRIMARY ACTION");

        Image rule = CreateImage(chrome, "ChromeHeaderRule", AccentBlue);
        SetTopLeft(rule.rectTransform, 76f, 214f, 1768f, 2f);

        TMP_Text footer = CreateText(
            chrome,
            "Footer",
            "LEFT / RIGHT  SELECT     SPACE  CONFIRM     ESC  EXIT");
        SetBottomRight(footer.rectTransform, 76f, 42f, 900f, 34f);
        ConfigureText(footer, 18f, TextAlignmentOptions.BottomRight, GreyBlue, FontStyles.Bold);

        HearthTerminalCompactChromeView view =
            prefabRoot.GetComponent<HearthTerminalCompactChromeView>();
        if (view == null)
        {
            view = prefabRoot.AddComponent<HearthTerminalCompactChromeView>();
        }
        view.Configure(
            terminal,
            visual.gameObject,
            terminalLabel,
            resident,
            before.Label,
            after.Label,
            primary.Label,
            status,
            footer,
            before.Fill,
            after.Fill,
            primary.Fill);
    }

    private static TabVisual CreateTerminalTab(
        Transform parent,
        string name,
        string spritePath,
        float x,
        float y,
        float width,
        float height,
        string label)
    {
        GameObject tabObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        tabObject.layer = parent.gameObject.layer;
        tabObject.transform.SetParent(parent, false);
        RectTransform rect = tabObject.GetComponent<RectTransform>();
        SetTopLeft(rect, x, y, width, height);
        Image frame = tabObject.GetComponent<Image>();
        AssignSprite(frame, spritePath, AccentBlue);

        Image fill = CreateImage(
            rect,
            "SelectionFill",
            new Color(AccentBlue.r, AccentBlue.g, AccentBlue.b, 0.16f));
        SetStretch(fill.rectTransform, 8f, 8f, 8f, 8f);
        fill.transform.SetAsFirstSibling();

        TMP_Text text = CreateText(rect, "Label", label);
        SetStretch(text.rectTransform, 18f, 8f, 18f, 8f);
        ConfigureText(text, 18f, TextAlignmentOptions.Center, ColdWhite, FontStyles.Bold);
        text.transform.SetAsLastSibling();
        return new TabVisual(fill, text);
    }

    private static void RepairDoorwayContent(Transform content)
    {
        if (content == null)
        {
            return;
        }

        List<Transform> introductions = FindNamedAll(content, "HouseholdIntroduction");
        for (int i = 0; i < introductions.Count; i++)
        {
            RectTransform panel = introductions[i] as RectTransform;
            SetTopLeft(panel, 1016f, 246f, 620f, 260f);
            EnsurePanelLayers(panel, TerminalInfoFrame);
            SetLocalTopLeft(FindText(panel, "Title"), 24f, 24f, 572f, 40f);
            SetLocalTopLeft(FindText(panel, "Body"), 24f, 78f, 572f, 150f);
        }

        List<Transform> fieldPanels = FindNamedAll(content, "FieldUnitPanel");
        for (int i = 0; i < fieldPanels.Count; i++)
        {
            RectTransform panel = fieldPanels[i] as RectTransform;
            SetTopLeft(panel, 1016f, 548f, 620f, 190f);
            EnsurePanelLayers(panel, TerminalFieldFrame);
            SetLocalTopLeft(FindText(panel, "Title"), 24f, 22f, 572f, 36f);
            SetLocalTopLeft(FindText(panel, "Body"), 24f, 70f, 572f, 92f);
        }

        ApplyPortraitSlot(content, "SON", 180f);
        ApplyPortraitSlot(content, "DAUGHTER", 180f);
        ApplyPortraitSlot(content, "WIFE", 180f);
        ApplyPortraitSlot(content, "DAD", 440f);
        ApplyPortraitSlot(content, "MOTHER", 440f);
        ApplyPortraitSlot(content, "HUSBAND", 440f);
        ApplyPortraitSlot(content, "MOM", 700f);
        ApplyPortraitSlot(content, "FATHER", 700f);
        ApplyPortraitSlot(content, "UNIT", 700f);
    }

    private static void ApplyPortraitSlot(Transform root, string token, float x)
    {
        List<Transform> portraits = FindNamedAll(root, "Portrait_" + token);
        for (int i = 0; i < portraits.Count; i++)
        {
            RectTransform portrait = portraits[i] as RectTransform;
            SetTopLeft(portrait, x, 264f, 240f, 400f);
            EnsurePanelLayers(portrait, TerminalPortraitFrame);
        }

        List<Transform> labels = FindNamedAll(root, "PortraitLabel_" + token);
        for (int i = 0; i < labels.Count; i++)
        {
            RectTransform label = labels[i] as RectTransform;
            SetTopLeft(label, x, 680f, 240f, 34f);
            TMP_Text text = labels[i].GetComponent<TMP_Text>();
            ConfigureText(text, 18f, TextAlignmentOptions.Center, ColdWhite, FontStyles.Bold);
        }
    }

    private static void RepairHomeContent(Transform content)
    {
        if (content == null)
        {
            return;
        }

        string[] duplicates =
        {
            "AccessLabel", "UnitLabel", "Welcome", "Personal", "Confirm",
            "TerminalTitle", "HomeRule", "FieldRule", "ActionHint"
        };
        for (int i = 0; i < duplicates.Length; i++)
        {
            SetNamedActiveAll(content, duplicates[i], false);
        }

        RectTransform homePanel = FindRect(content, "HomePanel");
        SetTopLeft(homePanel, 516f, 266f, 888f, 220f);
        if (homePanel != null)
        {
            EnsurePlainPanel(homePanel, Amber);
            SetLocalTopLeft(FindText(homePanel, "Title"), 64f, 32f, 780f, 48f);
            SetLocalTopLeft(FindText(homePanel, "Body"), 64f, 94f, 780f, 92f);
        }

        RectTransform field = FindRect(content, "FieldUnitPanel");
        if (field != null && homePanel != null && field.parent == homePanel)
        {
            // The legacy layout nested the Field Unit card inside HomePanel,
            // so applying the 1920-space X/Y directly doubled the offset.
            // Keep it under the same page visual, but give it absolute terminal
            // coordinates like the other V2 panels.
            field.SetParent(homePanel.parent, false);
        }
        SetTopLeft(field, 650f, 520f, 620f, 190f);
        EnsurePanelLayers(field, TerminalFieldFrame);
        SetLocalTopLeft(FindText(field, "Title"), 24f, 22f, 572f, 36f);
        SetLocalTopLeft(FindText(field, "Body"), 24f, 70f, 572f, 92f);
    }

    private static void EnsurePlainPanel(RectTransform panel, Color accent)
    {
        if (panel == null)
        {
            return;
        }
        DestroyDirectChildren(panel, "PanelBackdrop");
        DestroyDirectChildren(panel, "PanelAccent");
        Image backdrop = CreateImage(panel, "PanelBackdrop", PanelBlueBlack);
        SetStretch(backdrop.rectTransform, 6f, 6f, 6f, 6f);
        backdrop.transform.SetAsFirstSibling();
        Image rule = CreateImage(panel, "PanelAccent", accent);
        SetTopLeft(rule.rectTransform, 6f, 6f, 3f, panel.rect.height - 12f);
        rule.transform.SetAsLastSibling();
    }

    private static void EnsurePanelLayers(RectTransform panel, string framePath)
    {
        if (panel == null)
        {
            return;
        }
        Image rootImage = panel.GetComponent<Image>();
        if (rootImage != null)
        {
            rootImage.color = Color.clear;
            rootImage.sprite = null;
        }
        DestroyDirectChildren(panel, "PanelBackdrop");
        DestroyDirectChildren(panel, "PanelFrame");
        DestroyDirectChildren(panel, "V2_VectorInfoFrame");
        DestroyDirectChildren(panel, "V2_VectorPanelFrame");
        DestroyDirectChildren(panel, "V2_VectorPortraitFrame");
        DestroyDirectChildren(panel, "V2_VectorDecisionFrame");
        DestroyDirectChildren(panel, "V2_Backplate");
        DestroyDirectChildren(panel, "V2_TriggerFrame");
        DestroyDirectChildren(panel, "V2_StatusFrame");
        Image backdrop = CreateImage(panel, "PanelBackdrop", PanelBlueBlack);
        SetStretch(backdrop.rectTransform, 6f, 6f, 6f, 6f);
        backdrop.transform.SetAsFirstSibling();
        Image frame = CreateSpriteImage(panel, "PanelFrame", framePath, AccentBlue);
        SetStretch(frame.rectTransform, 0f, 0f, 0f, 0f);
        frame.transform.SetAsLastSibling();
    }

    private static void ValidateCompanion(List<string> issues)
    {
        ValidatePrefab(CompanionPrefab, root =>
        {
            RequireRect(root.transform, "V2_StatusPanel", new Rect(52f, 160f, 520f, 240f), issues);
            RequireRect(root.transform, "TriggerCardView", new Rect(52f, 160f, 520f, 240f), issues);
            RequireRect(root.transform, "DecisionPanel", new Rect(1348f, 160f, 520f, 216f), issues);
            RectTransform trigger = FindRect(root.transform, "TriggerCardView");
            if (trigger == null ||
                trigger.Find("PanelBackdrop") == null ||
                trigger.Find("PanelFrame") == null)
            {
                issues.Add(
                    "TriggerCardView must use the V2 backdrop and vector frame.");
            }
            string[] retiredTriggerParts =
            {
                "TriggerCardAccent", "V2_TriggerRule", "V2_Backplate"
            };
            for (int i = 0; i < retiredTriggerParts.Length; i++)
            {
                Transform retired =
                    trigger != null
                        ? FindNamed(trigger, retiredTriggerParts[i])
                        : null;
                if (retired != null && retired.gameObject.activeSelf)
                {
                    issues.Add(
                        retiredTriggerParts[i] +
                        " must stay disabled in TriggerCardView.");
                }
            }
            if (root.GetComponentInChildren<HearthCompanionHudExclusiveMode>(true) != null)
            {
                issues.Add("Companion prefab still contains legacy exclusive-mode ownership.");
            }
            TMP_Text task = FindText(root.transform, "V2_CurrentTask");
            if (task == null || task.text.Trim() != "CURRENT TASK")
            {
                issues.Add("Companion Current Task must contain the heading only.");
            }
        });
    }

    private static void ValidateSubtitle(List<string> issues)
    {
        ValidatePrefab(SubtitlePrefab, root =>
        {
            RectTransform frame = FindRect(root.transform, "FormalFrame");
            if (frame == null || Mathf.Abs(FrameCenterX(frame) - 960f) > 0.01f)
            {
                issues.Add("Formal dialogue frame is not centered at X=960.");
            }
        });
    }

    private static void ValidateHuman(List<string> issues)
    {
        ValidatePrefab(HumanPrefab, root =>
        {
            TMP_Text menuTaskBody =
                FindText(root.transform, "V2_MenuTaskBody");
            if (menuTaskBody != null &&
                (menuTaskBody.gameObject.activeSelf ||
                 !string.IsNullOrWhiteSpace(menuTaskBody.text)))
            {
                issues.Add(
                    "Human Tab task body must stay hidden until approved " +
                    "stage copy exists.");
            }

            string[] buttons =
            {
                "Button_TODAY", "Button_DISPOSITION_HISTORY",
                "Button_SYSTEM_SETTINGS"
            };
            for (int i = 0; i < buttons.Length; i++)
            {
                RectTransform button = FindRect(root.transform, buttons[i]);
                if (button == null || button.Find("SelectionFill") == null)
                {
                    issues.Add(buttons[i] + " is missing its inset SelectionFill.");
                }
            }

            string[] choicePages =
            {
                "Slide09_FinalChoice", "Slide14_FinalChoiceReturn"
            };
            for (int i = 0; i < choicePages.Length; i++)
            {
                Transform page = FindNamed(root.transform, choicePages[i]);
                string[] choiceButtons =
                {
                    "Button_ANSWER_LILY", "Button_COMPANION_ANSWER"
                };
                for (int b = 0; b < choiceButtons.Length; b++)
                {
                    RectTransform button = FindRect(page, choiceButtons[b]);
                    if (button == null ||
                        button.Find("SelectionFill") == null ||
                        button.GetComponent<RectMask2D>() == null)
                    {
                        issues.Add(
                            choicePages[i] + "/" + choiceButtons[b] +
                            " needs a clipped inset SelectionFill.");
                    }
                }

                Transform oldFillA = FindNamed(page, "ShapeFill_001");
                Transform oldFillB = FindNamed(page, "ShapeFill_004");
                if ((oldFillA != null && oldFillA.gameObject.activeSelf) ||
                    (oldFillB != null && oldFillB.gameObject.activeSelf))
                {
                    issues.Add(
                        choicePages[i] +
                        " still has an active legacy choice fill.");
                }
            }

            Transform finalFocus = FindNamed(root.transform, "FinalChoiceFocus");
            if (finalFocus != null && finalFocus.gameObject.activeSelf)
            {
                issues.Add("FinalChoiceFocus must stay retired.");
            }

            Transform shutdown =
                FindNamed(root.transform, "Slide10_ShutdownConfirm");
            Transform[] shutdownItems = shutdown != null
                ? shutdown.GetComponentsInChildren<Transform>(true)
                : Array.Empty<Transform>();
            for (int i = 0; i < shutdownItems.Length; i++)
            {
                if (shutdownItems[i].name.IndexOf(
                        "CANCEL",
                        StringComparison.OrdinalIgnoreCase) >= 0 &&
                    shutdownItems[i].gameObject.activeSelf)
                {
                    issues.Add(
                        "Slide10_ShutdownConfirm still exposes a cancel object.");
                    break;
                }
            }

            RequireHumanTabPage(
                root.transform,
                "Slide05_TodayRounds",
                "V2_TodayRoundsContentFrame",
                null,
                issues);
            string[] historyPages =
            {
                "Slide18_HistoryEmpty", "Slide19_HistoryOne",
                "Slide20_HistoryTwo", "Slide21_HistoryThree"
            };
            for (int i = 0; i < historyPages.Length; i++)
            {
                RequireHumanTabPage(
                    root.transform,
                    historyPages[i],
                    "V2_HistoryContentFrame",
                    "V2_HistoryMetricsFrame",
                    issues);
            }
            RequireHumanTabPage(
                root.transform,
                "Slide22_Settings",
                "V2_SettingsContentFrame",
                "V2_SettingsFooterFrame",
                issues);
            RequireHumanTabPage(
                root.transform,
                "Slide23_SettingsFocus",
                "V2_SettingsContentFrame",
                "V2_SettingsFooterFrame",
                issues);
        });
    }

    private static void RequireHumanTabPage(
        Transform root,
        string pageName,
        string contentFrameName,
        string metricFrameName,
        List<string> issues)
    {
        Transform page = FindNamed(root, pageName);
        if (page == null)
        {
            issues.Add(pageName + " is missing.");
            return;
        }

        RequireRect(
            page,
            "V2_PagePanel",
            new Rect(400f, 160f, 1120f, 760f),
            issues);
        RectTransform shell = FindRect(page, "V2_PagePanel");
        if (shell == null || shell.Find(contentFrameName) == null)
        {
            issues.Add(pageName + " is missing " + contentFrameName + ".");
        }
        if (!string.IsNullOrWhiteSpace(metricFrameName) &&
            (shell == null || shell.Find(metricFrameName) == null))
        {
            issues.Add(pageName + " is missing " + metricFrameName + ".");
        }
    }

    private static void ValidateTerminal(string path, List<string> issues)
    {
        ValidatePrefab(path, root =>
        {
            List<Transform> visualRoots = FindNamedAll(root.transform, "TerminalVisualRoot");
            if (visualRoots.Count != 1)
            {
                issues.Add(path + " must contain exactly one TerminalVisualRoot.");
            }
            if (FindNamed(root.transform, "V2_ClosureTerminalChrome") != null)
            {
                issues.Add(path + " still contains V2_ClosureTerminalChrome.");
            }
        });
    }

    private static void RequireRect(
        Transform root,
        string name,
        Rect expected,
        List<string> issues)
    {
        RectTransform rect = FindRect(root, name);
        if (rect == null)
        {
            issues.Add(name + " is missing.");
            return;
        }
        Vector2 topLeft = ToReferenceTopLeft(rect);
        if (Vector2.Distance(topLeft, expected.position) > 0.1f ||
            Vector2.Distance(rect.sizeDelta, expected.size) > 0.1f)
        {
            issues.Add(name + " does not match " + expected + ".");
        }
    }

    private static float FrameCenterX(RectTransform rect)
    {
        Vector2 topLeft = ToReferenceTopLeft(rect);
        return topLeft.x + rect.sizeDelta.x * 0.5f;
    }

    private static Vector2 ToReferenceTopLeft(RectTransform rect)
    {
        if (rect.anchorMin == new Vector2(1f, 1f))
        {
            return new Vector2(1920f + rect.anchoredPosition.x - rect.sizeDelta.x, -rect.anchoredPosition.y);
        }
        if (rect.anchorMin == new Vector2(0.5f, 1f))
        {
            return new Vector2(960f + rect.anchoredPosition.x - rect.sizeDelta.x * 0.5f, -rect.anchoredPosition.y);
        }
        return new Vector2(rect.anchoredPosition.x, -rect.anchoredPosition.y);
    }

    private static void EditPrefab(string path, Action<GameObject> edit)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
        {
            Debug.LogError("[HearthUiV2FinalVisualRepairEditor] Missing prefab: " + path);
            return;
        }
        GameObject contents = PrefabUtility.LoadPrefabContents(path);
        try
        {
            edit(contents);
            PrefabUtility.SaveAsPrefabAsset(contents, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    private static void ValidatePrefab(string path, Action<GameObject> validate)
    {
        GameObject contents = PrefabUtility.LoadPrefabContents(path);
        try
        {
            validate(contents);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    private static Transform FindNamed(Transform root, string name)
    {
        if (root == null)
        {
            return null;
        }
        Transform[] items = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].name == name)
            {
                return items[i];
            }
        }
        return null;
    }

    private static List<Transform> FindNamedAll(Transform root, string name)
    {
        List<Transform> matches = new List<Transform>();
        if (root == null)
        {
            return matches;
        }
        Transform[] items = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].name == name)
            {
                matches.Add(items[i]);
            }
        }
        return matches;
    }

    private static RectTransform FindRect(Transform root, string name)
    {
        return FindNamed(root, name) as RectTransform;
    }

    private static TMP_Text FindText(Transform root, string name)
    {
        Transform item = FindNamed(root, name);
        return item != null ? item.GetComponent<TMP_Text>() : null;
    }

    private static Image FindImage(Transform root, string name)
    {
        Transform item = FindNamed(root, name);
        return item != null ? item.GetComponent<Image>() : null;
    }

    private static void SetNamedActiveAll(Transform root, string name, bool active)
    {
        List<Transform> items = FindNamedAll(root, name);
        for (int i = 0; i < items.Count; i++)
        {
            items[i].gameObject.SetActive(active);
        }
    }

    private static void DestroyAllNamed(Transform root, string name)
    {
        List<Transform> items = FindNamedAll(root, name);
        for (int i = items.Count - 1; i >= 0; i--)
        {
            UnityEngine.Object.DestroyImmediate(items[i].gameObject);
        }
    }

    private static void DestroyDirectChildren(Transform parent, string name)
    {
        if (parent == null)
        {
            return;
        }
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (child.name == name)
            {
                UnityEngine.Object.DestroyImmediate(child.gameObject);
            }
        }
    }

    private static Image CreateImage(Transform parent, string name, Color color)
    {
        GameObject item = new GameObject(
            name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        item.layer = parent.gameObject.layer;
        item.transform.SetParent(parent, false);
        Image image = item.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static Image CreateSpriteImage(
        Transform parent,
        string name,
        string path,
        Color color)
    {
        Image image = CreateImage(parent, name, color);
        AssignSprite(image, path, color);
        return image;
    }

    private static Image EnsureSpriteImage(
        Transform parent,
        string name,
        string path,
        Color color)
    {
        Transform item = parent.Find(name);
        Image image = item != null ? item.GetComponent<Image>() : null;
        if (image == null)
        {
            image = CreateImage(parent, name, color);
        }
        AssignSprite(image, path, color);
        return image;
    }

    private static void AssignSprite(Image image, string path, Color color)
    {
        if (image == null)
        {
            return;
        }
        image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
        image.color = color;
        image.raycastTarget = false;
    }

    private static TMP_Text CreateText(Transform parent, string name, string value)
    {
        GameObject item = new GameObject(name, typeof(RectTransform));
        item.layer = parent.gameObject.layer;
        item.transform.SetParent(parent, false);
        TextMeshProUGUI text = item.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null)
        {
            text.font = TMP_Settings.defaultFontAsset;
        }
        text.text = value;
        return text;
    }

    private static void ConfigureRule(RectTransform rect, Color color)
    {
        if (rect == null)
        {
            return;
        }
        Image image = rect.GetComponent<Image>();
        if (image != null)
        {
            image.sprite = null;
            image.color = color;
            image.raycastTarget = false;
        }
        rect.gameObject.SetActive(true);
    }

    private static void ConfigureText(
        TMP_Text text,
        float size,
        TextAlignmentOptions alignment,
        Color color,
        FontStyles style)
    {
        if (text == null)
        {
            return;
        }
        text.fontSize = size;
        text.fontSizeMin = size;
        text.fontSizeMax = size;
        text.enableAutoSizing = false;
        text.enableWordWrapping = true;
        text.maxVisibleLines = 99;
        text.overflowMode = TextOverflowModes.Overflow;
        text.alignment = alignment;
        text.color = color;
        text.fontStyle = style;
        text.raycastTarget = false;
    }

    private static void SetLocalTopLeft(
        TMP_Text text,
        float x,
        float y,
        float width,
        float height)
    {
        if (text == null)
        {
            return;
        }
        SetTopLeft(text.rectTransform, x, y, width, height);
    }

    private static void SetTopLeft(
        RectTransform rect,
        float x,
        float y,
        float width,
        float height)
    {
        if (rect == null)
        {
            return;
        }
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
        rect.localScale = Vector3.one;
    }

    private static void SetTopRight(
        RectTransform rect,
        float right,
        float y,
        float width,
        float height)
    {
        if (rect == null)
        {
            return;
        }
        rect.anchorMin = Vector2.one;
        rect.anchorMax = Vector2.one;
        rect.pivot = Vector2.one;
        rect.anchoredPosition = new Vector2(-right, -y);
        rect.sizeDelta = new Vector2(width, height);
        rect.localScale = Vector3.one;
    }

    private static void SetTopCenter(
        RectTransform rect,
        float x,
        float y,
        float width,
        float height)
    {
        if (rect == null)
        {
            return;
        }
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
        rect.localScale = Vector3.one;
    }

    private static void SetBottomLeft(
        RectTransform rect,
        float x,
        float bottom,
        float width,
        float height)
    {
        if (rect == null)
        {
            return;
        }
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = Vector2.zero;
        rect.anchoredPosition = new Vector2(x, bottom);
        rect.sizeDelta = new Vector2(width, height);
        rect.localScale = Vector3.one;
    }

    private static void SetBottomRight(
        RectTransform rect,
        float right,
        float bottom,
        float width,
        float height)
    {
        if (rect == null)
        {
            return;
        }
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(-right, bottom);
        rect.sizeDelta = new Vector2(width, height);
        rect.localScale = Vector3.one;
    }

    private static void SetBottomCenter(
        RectTransform rect,
        float x,
        float bottom,
        float width,
        float height)
    {
        if (rect == null)
        {
            return;
        }
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(x, bottom);
        rect.sizeDelta = new Vector2(width, height);
        rect.localScale = Vector3.one;
    }

    private static void SetStretch(
        RectTransform rect,
        float left,
        float top,
        float right,
        float bottom)
    {
        if (rect == null)
        {
            return;
        }
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
        rect.localScale = Vector3.one;
    }

    private readonly struct TabVisual
    {
        public readonly Image Fill;
        public readonly TMP_Text Label;

        public TabVisual(Image fill, TMP_Text label)
        {
            Fill = fill;
            Label = label;
        }
    }
}
#endif
