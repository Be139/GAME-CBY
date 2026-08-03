#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class HearthUiV2ClosureEditor
{
    private static bool legacyBatchAuthorized;
    private const string HumanPrefab =
        "Assets/Prefabs/UI/HearthHud/V2/HearthHudRoot_V2.prefab";
    private const string CompanionPrefab =
        "Assets/Prefabs/UI/HearthHud/V2/Companion/HearthCompanionHudRoot_V2.prefab";
    private const string TerminalFolder =
        "Assets/Prefabs/UI/HearthHud/V2/Terminals/";
    private const string SubtitlePrefab =
        "Assets/Prefabs/UI/HearthSubtitle/V2/HearthSubtitleVisualCanvas_V2.prefab";
    private const string VectorRoot =
        "Assets/UI/HEARTH/V2/VectorParts/";
    private const string CompanionFramePath =
        VectorRoot +
        "Companion/HUD_Companion_FullscreenFrame_1920x1080.png";
    private const string ButtonFramePath =
        VectorRoot + "Common/HUD_Common_ButtonFrame_320x72.png";
    private const string PanelFramePath =
        VectorRoot + "Common/HUD_Common_PanelFrame_520x320.png";
    private const string StatusFramePath =
        VectorRoot + "Common/HUD_Common_StatusFrame_520x240.png";
    private const string PromptFramePath =
        VectorRoot +
        "Interaction/HUD_Interaction_GazePromptFrame_520x128.png";
    private const string HeaderUnderlinePath =
        VectorRoot + "Common/HUD_Common_HeaderUnderline_310x8.png";
    private const string DialogueFramePath =
        VectorRoot + "Common/HUD_Common_DialogueFrame_960x256.png";
    private const string SpeakerTabLeftPath =
        VectorRoot + "Common/HUD_Common_SpeakerTab_Left_340x48.png";
    private const string SpeakerTabRightPath =
        VectorRoot + "Common/HUD_Common_SpeakerTab_Right_340x48.png";
    private const string FieldUnitFramePath =
        VectorRoot +
        "Feedback/HUD_Feedback_FieldUnitToastFrame_640x180.png";
    private const string PhotoFramePath =
        VectorRoot + "Finale/HUD_Finale_PhotoFrame_1280x720.png";
    private const string ShutdownFramePath =
        VectorRoot + "Finale/HUD_Finale_ShutdownModalFrame_720x420.png";
    private const string WarningFramePath =
        VectorRoot + "Feedback/HUD_Feedback_WarningModalFrame_720x360.png";
    private const string TerminalInfoFramePath =
        VectorRoot + "Terminal/HUD_Terminal_InfoPanelFrame_520x320.png";
    private const string TerminalPortraitFramePath =
        VectorRoot + "Terminal/HUD_Terminal_PortraitFrame_240x400.png";

    private static readonly Color DeepBlueBlack =
        new Color32(11, 16, 24, 235);
    private static readonly Color GreyBlue =
        new Color32(95, 120, 149, 242);
    private static readonly Color ColdWhite =
        new Color32(215, 230, 246, 255);
    private static readonly Color LowSaturationBlue =
        new Color32(120, 170, 220, 220);
    private static readonly Color Red =
        new Color32(228, 62, 54, 255);

    [MenuItem("Tools/Hearth/Legacy Unsafe/UI V2 Closure/Apply Approved Closure")]
    public static void ApplyAll()
    {
        if (!HearthLegacyToolGuard.Confirm(
                "Apply Approved V2 Closure",
                "all canonical V2 Prefabs and selected active-scene UI bindings"))
        {
            return;
        }

        legacyBatchAuthorized = true;
        try
        {
        HearthUiV2VectorAssetEditor.PrepareImportedSprites();
        ApplySubtitle();
        ApplyHuman();
        ApplyCompanion();
        ApplyOpenSceneCompanionClosure();
        ApplyOpenScenePhotoClosure();
        ApplyTerminal(
            TerminalFolder + "Terminal_Lobby_Assignment_V2.prefab",
            HearthTerminalMode.LobbySync);
        ApplyTerminal(
            TerminalFolder + "Terminal_17F01_V2.prefab",
            HearthTerminalMode.Doorway);
        ApplyTerminal(
            TerminalFolder + "Terminal_17F02_V2.prefab",
            HearthTerminalMode.Doorway);
        ApplyTerminal(
            TerminalFolder + "Terminal_17F03_Alert_V2.prefab",
            HearthTerminalMode.Doorway);
        ApplyTerminal(
            TerminalFolder + "Terminal_17F04_Home_V2.prefab",
            HearthTerminalMode.Home);
        // The approved closure predates the final V2 visual repair. Normalize
        // its output through the current authoritative layout so rerunning
        // this compatibility menu cannot restore retired terminal chrome.
        HearthUiV2FinalVisualRepairEditor.ApplyAll();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log(
            "[HearthUiV2ClosureEditor] Applied the approved second-UI closure " +
            "without rebuilding scene bindings or legacy prefabs.");
        }
        finally
        {
            legacyBatchAuthorized = false;
        }
    }

    [MenuItem("Tools/Hearth/Legacy Unsafe/UI V2 Closure/Apply Subtitle Visual Closure")]
    public static void ApplySubtitle()
    {
        if (!legacyBatchAuthorized &&
            !HearthLegacyToolGuard.Confirm(
                "Apply Subtitle Visual Closure",
                "the canonical V2 subtitle Prefab"))
        {
            return;
        }

        EditPrefab(
            SubtitlePrefab,
            root =>
            {
                Transform visual = FindNamed(root.transform, "VisualRoot");
                if (visual == null)
                {
                    Debug.LogWarning(
                        "[HearthUiV2ClosureEditor] Subtitle VisualRoot missing.");
                    return;
                }

                Image backdrop =
                    FindImage(visual, "Backdrop");
                if (backdrop != null)
                {
                    backdrop.sprite = null;
                    backdrop.type = Image.Type.Simple;
                    backdrop.color =
                        new Color(
                            DeepBlueBlack.r,
                            DeepBlueBlack.g,
                            DeepBlueBlack.b,
                            0.82f);
                    backdrop.raycastTarget = false;
                    backdrop.transform.SetAsFirstSibling();
                }

                Image formalFrame = CreateOrGetImage(
                    visual,
                    "FormalFrame",
                    DialogueFramePath,
                    LowSaturationBlue,
                    false);
                Image auxiliaryFrame = CreateOrGetImage(
                    visual,
                    "AuxiliaryFrame",
                    FieldUnitFramePath,
                    LowSaturationBlue,
                    true);
                Image leftTab = CreateOrGetImage(
                    visual,
                    "SpeakerTabLeft",
                    SpeakerTabLeftPath,
                    LowSaturationBlue,
                    false);
                Image rightTab = CreateOrGetImage(
                    visual,
                    "SpeakerTabRight",
                    SpeakerTabRightPath,
                    LowSaturationBlue,
                    false);

                SetTopLeft(
                    formalFrame.rectTransform,
                    432f,
                    670f,
                    960f,
                    256f);
                SetTopLeft(
                    auxiliaryFrame.rectTransform,
                    1216f,
                    214f,
                    640f,
                    180f);
                SetTopLeft(
                    leftTab.rectTransform,
                    432f,
                    622f,
                    340f,
                    48f);
                SetTopLeft(
                    rightTab.rectTransform,
                    1052f,
                    622f,
                    340f,
                    48f);

                Transform legacyTab = FindNamed(visual, "SpeakerTab");
                if (legacyTab != null)
                {
                    legacyTab.gameObject.SetActive(false);
                }

                Transform legacyAccent = FindNamed(visual, "AccentRule");
                if (legacyAccent != null)
                {
                    legacyAccent.gameObject.SetActive(false);
                }

                TMP_Text hint = CreateOrGetText(
                    visual,
                    "AdvanceHint",
                    "SPACE  CONTINUE");
                SetTopLeft(
                    hint.rectTransform,
                    1128f,
                    884f,
                    224f,
                    24f);
                ConfigureText(
                    hint,
                    15f,
                    TextAlignmentOptions.Right,
                    LowSaturationBlue,
                    FontStyles.Bold);

                formalFrame.gameObject.SetActive(false);
                auxiliaryFrame.gameObject.SetActive(false);
                leftTab.gameObject.SetActive(false);
                rightTab.gameObject.SetActive(false);
                hint.gameObject.SetActive(false);

                if (backdrop != null)
                {
                    backdrop.transform.SetSiblingIndex(0);
                }
                formalFrame.transform.SetSiblingIndex(1);
                auxiliaryFrame.transform.SetSiblingIndex(2);
                leftTab.transform.SetSiblingIndex(3);
                rightTab.transform.SetSiblingIndex(4);
                hint.transform.SetAsLastSibling();
            });
    }

    [MenuItem("Tools/Hearth/Legacy Unsafe/UI V2 Closure/Validate Approved Closure")]
    public static void ValidateApprovedClosure()
    {
        List<string> issues = new List<string>();
        ValidateHumanPrefab(issues);
        ValidateCompanionPrefab(issues);
        ValidateOpenSceneCompanionInstances(issues);
        ValidateTerminalPrefab(
            TerminalFolder + "Terminal_Lobby_Assignment_V2.prefab",
            HearthTerminalMode.LobbySync,
            false,
            issues);
        ValidateTerminalPrefab(
            TerminalFolder + "Terminal_17F01_V2.prefab",
            HearthTerminalMode.Doorway,
            false,
            issues);
        ValidateTerminalPrefab(
            TerminalFolder + "Terminal_17F02_V2.prefab",
            HearthTerminalMode.Doorway,
            false,
            issues);
        ValidateTerminalPrefab(
            TerminalFolder + "Terminal_17F03_Alert_V2.prefab",
            HearthTerminalMode.Doorway,
            false,
            issues);
        ValidateTerminalPrefab(
            TerminalFolder + "Terminal_17F04_Home_V2.prefab",
            HearthTerminalMode.Home,
            false,
            issues);
        ValidateSubtitlePrefab(issues);

        if (issues.Count == 0)
        {
            Debug.Log(
                "[HearthUiV2ClosureEditor] Validation passed: Human, Companion, " +
                "subtitle, lobby synchronization terminal, doorway terminals, " +
                "and home terminal all satisfy the approved V2 closure structure.");
            return;
        }

        Debug.LogError(
            "[HearthUiV2ClosureEditor] Validation found " +
            issues.Count +
            " issue(s):\n- " +
            string.Join("\n- ", issues));
    }

    [MenuItem("Tools/Hearth/Legacy Unsafe/UI V2 Closure/Apply Human Prefab Closure")]
    public static void ApplyHuman()
    {
        if (!legacyBatchAuthorized &&
            !HearthLegacyToolGuard.Confirm(
                "Apply Human Prefab Closure",
                "the canonical V2 Human HUD Prefab"))
        {
            return;
        }

        EditPrefab(
            HumanPrefab,
            root =>
            {
                CanvasScaler scaler = root.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode =
                        CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920f, 1080f);
                    scaler.screenMatchMode =
                        CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    scaler.matchWidthOrHeight = 0.5f;
                }

                Transform persistent = FindNamed(root.transform, "PersistentHud");
                if (persistent != null)
                {
                    ApplyPersistentHumanLayout(persistent);
                }

                ApplyFrame(
                    FindRect(root.transform, "V2_HeaderUnderline"),
                    HeaderUnderlinePath,
                    LowSaturationBlue,
                    false);
                ApplyFrame(
                    FindRect(root.transform, "V2_TaskUnderline"),
                    HeaderUnderlinePath,
                    LowSaturationBlue,
                    false);

                SetBottomLeft(
                    FindRect(root.transform, "LocationHud"),
                    64f,
                    48f,
                    340f,
                    92f);
                AlignNamedText(
                    root.transform,
                    "LocationTitleText",
                    TextAlignmentOptions.BottomLeft,
                    16f,
                    GreyBlue);
                AlignNamedText(
                    root.transform,
                    "LocationValueText",
                    TextAlignmentOptions.BottomLeft,
                    24f,
                    ColdWhite);
                AlignNamedText(
                    root.transform,
                    "LocationGlowText",
                    TextAlignmentOptions.BottomLeft,
                    24f,
                    LowSaturationBlue);

                RectTransform interaction =
                    FindRect(root.transform, "PlayerInteractionPrompt");
                SetBottomCenter(interaction, 0f, 118f, 600f, 64f);
                ApplyFrame(interaction, PromptFramePath, GreyBlue, true);
                AlignNamedText(
                    root.transform,
                    "InteractionText",
                    TextAlignmentOptions.Center,
                    22f,
                    ColdWhite);

                SetBottomRight(
                    FindRect(root.transform, "V2_InitialTutorialRoot"),
                    64f,
                    48f,
                    720f,
                    96f);

                ApplyPhotoLayout(root.transform);
                ApplyDecisionLayout(root.transform);
                ApplyShutdownAndTakeoverFrames(root.transform);
                ApplyInteractiveButtonFrames(root.transform);
                ApplyKeycapSizing(root.transform);
            });
    }

    [MenuItem("Tools/Hearth/Legacy Unsafe/UI V2 Closure/Apply Companion Prefab Closure")]
    public static void ApplyCompanion()
    {
        if (!legacyBatchAuthorized &&
            !HearthLegacyToolGuard.Confirm(
                "Apply Companion Prefab Closure",
                "the canonical V2 Companion HUD Prefab"))
        {
            return;
        }

        EditPrefab(
            CompanionPrefab,
            root =>
            {
                Transform identity = FindNamed(root.transform, "V2_Identity");
                Transform visualParent =
                    identity != null ? identity.parent : root.transform;

                RectTransform frame =
                    FindRect(root.transform, "CompanionRobotFrame");
                if (frame != null)
                {
                    SetStretch(frame, 20f, 20f, 20f, 20f);
                    Image image = frame.GetComponent<Image>();
                    if (image != null)
                    {
                        image.sprite =
                            AssetDatabase.LoadAssetAtPath<Sprite>(
                                CompanionFramePath);
                        image.type = Image.Type.Simple;
                        image.preserveAspect = false;
                        image.color =
                            new Color(
                                LowSaturationBlue.r,
                                LowSaturationBlue.g,
                                LowSaturationBlue.b,
                                0.78f);
                        image.raycastTarget = false;
                    }
                    frame.SetAsFirstSibling();
                }

                TMP_Text identityText =
                    FindText(root.transform, "V2_Identity");
                if (identityText != null)
                {
                    SetTopLeft(identityText.rectTransform, 64f, 54f, 460f, 78f);
                    ConfigureText(
                        identityText,
                        22f,
                        TextAlignmentOptions.TopLeft,
                        ColdWhite,
                        FontStyles.Normal);
                }

                TMP_Text rec = CreateOrGetText(
                    visualParent,
                    "V2_REC",
                    "●  REC");
                SetTopCenter(rec.rectTransform, 0f, 48f, 220f, 40f);
                ConfigureText(
                    rec,
                    22f,
                    TextAlignmentOptions.Top,
                    Red,
                    FontStyles.Bold);

                TMP_Text task = CreateOrGetText(
                    visualParent,
                    "V2_CurrentTask",
                    "CURRENT TASK\nREVIEW RECORDED HOUSEHOLD EVENT");
                SetTopRight(task.rectTransform, 64f, 54f, 520f, 92f);
                ConfigureText(
                    task,
                    20f,
                    TextAlignmentOptions.TopRight,
                    ColdWhite,
                    FontStyles.Normal);

                SetTopLeft(
                    FindRect(root.transform, "V2_StatusPanel"),
                    52f,
                    160f,
                    520f,
                    240f);
                SetTopRight(
                    FindRect(root.transform, "DecisionPanel"),
                    64f,
                    206f,
                    520f,
                    220f);
                ApplyOverlayFrame(
                    FindRect(root.transform, "V2_StatusPanel"),
                    "V2_VectorPanelFrame",
                    StatusFramePath,
                    LowSaturationBlue);
                ApplyOverlayFrame(
                    FindRect(root.transform, "DecisionPanel"),
                    "V2_VectorPanelFrame",
                    PanelFramePath,
                    LowSaturationBlue);
                RectTransform trigger =
                    FindRect(root.transform, "TriggerCardView");
                SetTopLeft(trigger, 52f, 160f, 520f, 240f);
                ApplyOverlayFrame(
                    trigger,
                    "V2_VectorTriggerFrame",
                    StatusFramePath,
                    LowSaturationBlue);
                SetNamedActive(trigger, "TriggerCardAccent", false);
                SetNamedActive(trigger, "V2_TriggerRule", false);
                SetNamedActive(trigger, "V2_Backplate", false);
                TMP_Text triggerTitle =
                    FindText(trigger, "TriggerCardTitleText");
                TMP_Text triggerBody =
                    FindText(trigger, "TriggerCardBodyText");
                SetTopLeft(
                    triggerTitle != null ? triggerTitle.rectTransform : null,
                    24f,
                    20f,
                    472f,
                    34f);
                SetTopLeft(
                    triggerBody != null ? triggerBody.rectTransform : null,
                    24f,
                    70f,
                    472f,
                    150f);
                ConfigureText(
                    triggerTitle,
                    20f,
                    TextAlignmentOptions.TopLeft,
                    LowSaturationBlue,
                    FontStyles.Bold);
                ConfigureText(
                    triggerBody,
                    18f,
                    TextAlignmentOptions.TopLeft,
                    ColdWhite,
                    FontStyles.Normal);
                ApplyCompanionPanelSafeAreas(root.transform);
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

                HearthCompanionHudLayoutController layoutController =
                    root.GetComponentInChildren<
                        HearthCompanionHudLayoutController>(true);
                if (layoutController != null)
                {
                    layoutController.RecaptureBaselines();
                }

                SetNamedActive(root.transform, "DataStreamView", false);
                SetNamedActive(root.transform, "V2_Status", false);
                SetNamedActive(root.transform, "V2_PhysicalFeedLabel", false);
                SetNamedActive(root.transform, "V2_PhysicalFeedRule", false);
                SetNamedActive(root.transform, "V2_InspectionHeading", false);
                SetNamedActive(root.transform, "V2_InspectionHeadingRule", false);
                SetNamedActive(root.transform, "V2_InspectionUnit", false);
                SetNamedActive(root.transform, "V2_InspectionReturn", false);

                HearthCompanionDataStreamView stream =
                    root.GetComponentInChildren<HearthCompanionDataStreamView>(
                        true);
                if (stream != null)
                {
                    stream.gameObject.SetActive(false);
                }
            });
    }

    private static void ApplyOpenSceneCompanionClosure()
    {
        HearthCompanionHudController[] controllers =
            Resources.FindObjectsOfTypeAll<HearthCompanionHudController>();
        bool changed = false;
        for (int i = 0; i < controllers.Length; i++)
        {
            HearthCompanionHudController controller = controllers[i];
            if (controller == null ||
                !controller.gameObject.scene.IsValid() ||
                !controller.gameObject.scene.isLoaded)
            {
                continue;
            }

            RectTransform frame =
                FindRect(controller.transform, "CompanionRobotFrame");
            if (frame != null)
            {
                SetStretch(frame, 20f, 20f, 20f, 20f);
                Image image = frame.GetComponent<Image>();
                if (image != null)
                {
                    image.sprite =
                        AssetDatabase.LoadAssetAtPath<Sprite>(
                            CompanionFramePath);
                    image.type = Image.Type.Simple;
                    image.preserveAspect = false;
                    image.color =
                        new Color(
                            LowSaturationBlue.r,
                            LowSaturationBlue.g,
                            LowSaturationBlue.b,
                            0.78f);
                    image.raycastTarget = false;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(
                        image);
                }
                PrefabUtility.RecordPrefabInstancePropertyModifications(frame);
                frame.SetAsFirstSibling();
                changed = true;
            }

            Transform legacyStatus =
                FindNamed(controller.transform, "V2_Status");
            if (legacyStatus != null)
            {
                legacyStatus.gameObject.SetActive(false);
                PrefabUtility.RecordPrefabInstancePropertyModifications(
                    legacyStatus.gameObject);
                changed = true;
            }

            RectTransform decision =
                FindRect(controller.transform, "DecisionPanel");
            if (decision != null)
            {
                SetTopRight(decision, 64f, 206f, 520f, 220f);
                PrefabUtility.RecordPrefabInstancePropertyModifications(
                    decision);
                HearthCompanionHudLayoutController layout =
                    controller.GetComponentInChildren<
                        HearthCompanionHudLayoutController>(true);
                if (layout != null)
                {
                    layout.RecaptureBaselines();
                    EditorUtility.SetDirty(layout);
                }
                changed = true;
            }

            RectTransform status =
                FindRect(controller.transform, "V2_StatusPanel");
            if (status != null)
            {
                SetTopLeft(status, 52f, 160f, 520f, 240f);
                PrefabUtility.RecordPrefabInstancePropertyModifications(
                    status);
                changed = true;
            }

            RectTransform trigger =
                FindRect(controller.transform, "TriggerCardView");
            if (trigger != null)
            {
                SetTopLeft(trigger, 52f, 160f, 520f, 240f);
                SetNamedActive(trigger, "TriggerCardAccent", false);
                SetNamedActive(trigger, "V2_TriggerRule", false);
                SetNamedActive(trigger, "V2_Backplate", false);
                PrefabUtility.RecordPrefabInstancePropertyModifications(
                    trigger);
                changed = true;
            }

            ApplyCompanionPanelSafeAreas(controller.transform);

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(
                    controller.gameObject.scene);
            }
        }

        if (changed)
        {
            Debug.Log(
                "[HearthUiV2ClosureEditor] Companion scene visuals changed. " +
                "Review the diff and save manually.");
        }
    }

    private static void ApplyCompanionPanelSafeAreas(Transform root)
    {
        TMP_Text statusTitle =
            FindText(root, "V2_StatusTitleText");
        if (statusTitle != null)
        {
            SetTopLeft(statusTitle.rectTransform, 26f, 18f, 468f, 34f);
        }

        TMP_Text statusRows =
            FindText(root, "V2_StatusRowsText");
        if (statusRows != null)
        {
            SetTopLeft(statusRows.rectTransform, 26f, 76f, 468f, 112f);
        }

        TMP_Text statusFooter =
            FindText(root, "V2_StatusFooterText");
        if (statusFooter != null)
        {
            SetTopLeft(statusFooter.rectTransform, 26f, 204f, 468f, 28f);
        }

        TMP_Text decisionKicker =
            FindText(root, "DecisionKickerText");
        if (decisionKicker != null)
        {
            SetTopLeft(decisionKicker.rectTransform, 26f, 18f, 468f, 28f);
        }

        TMP_Text decisionTitle =
            FindText(root, "DecisionTitleText");
        if (decisionTitle != null)
        {
            SetTopLeft(decisionTitle.rectTransform, 26f, 56f, 468f, 40f);
        }

        TMP_Text decisionBody =
            FindText(root, "DecisionBodyText");
        if (decisionBody != null)
        {
            SetTopLeft(decisionBody.rectTransform, 26f, 106f, 468f, 80f);
        }
    }

    private static void ApplyOpenScenePhotoClosure()
    {
        HearthPhotoFrameInteractable[] interactables =
            Resources.FindObjectsOfTypeAll<HearthPhotoFrameInteractable>();
        bool changed = false;
        for (int i = 0; i < interactables.Length; i++)
        {
            HearthPhotoFrameInteractable interactable = interactables[i];
            if (interactable == null ||
                !interactable.gameObject.scene.IsValid() ||
                !interactable.gameObject.scene.isLoaded)
            {
                continue;
            }

            SerializedObject serialized = new SerializedObject(interactable);
            SerializedProperty useSecondUi =
                serialized.FindProperty("useSecondUiPhotoArchive");
            if (useSecondUi != null)
            {
                useSecondUi.boolValue = false;
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.RecordPrefabInstancePropertyModifications(
                interactable);
            EditorSceneManager.MarkSceneDirty(interactable.gameObject.scene);
            changed = true;
        }

        if (changed)
        {
            Debug.Log(
                "[HearthUiV2ClosureEditor] Photo scene bindings changed. " +
                "Review the diff and save manually.");
        }
    }

    private static void ApplyTerminal(
        string prefabPath,
        HearthTerminalMode mode)
    {
        EditPrefab(
            prefabPath,
            root =>
            {
                HearthTvTerminalController terminal =
                    root.GetComponent<HearthTvTerminalController>() ??
                    root.GetComponentInChildren<HearthTvTerminalController>(
                        true);
                if (terminal == null)
                {
                    Debug.LogWarning(
                        "[HearthUiV2ClosureEditor] Terminal controller missing: " +
                        prefabPath);
                    return;
                }

                terminal.SetTerminalMode(mode);

                RectTransform keyboardRoot =
                    FindRect(root.transform, "KeyboardNavigationRoot");
                if (mode == HearthTerminalMode.LobbySync)
                {
                    if (keyboardRoot != null)
                    {
                        SetBottomLeft(
                            keyboardRoot,
                            76f,
                            48f,
                            1768f,
                            58f);
                    }

                    TMP_Text lobbyHint =
                        FindText(root.transform, "KeyboardHintText");
                    if (lobbyHint != null)
                    {
                        lobbyHint.text = "SPACE  CLOSE TERMINAL";
                        ConfigureText(
                            lobbyHint,
                            20f,
                            TextAlignmentOptions.Center,
                            GreyBlue,
                            FontStyles.Normal);
                    }

                    TMP_Text lobbyFocus =
                        FindText(root.transform, "KeyboardFocusText");
                    if (lobbyFocus != null)
                    {
                        lobbyFocus.text = string.Empty;
                    }
                    return;
                }

                DisableLegacyTerminalChrome(root.transform);
                if (mode == HearthTerminalMode.Home)
                {
                    DisableHomeLegacyLabels(root.transform);
                }
                ApplyTerminalContentFrames(root.transform);
                ApplyTerminalTextContrast(root.transform);
                BuildCompactTerminalChrome(root, terminal, mode);
            });
    }

    private static void BuildCompactTerminalChrome(
        GameObject prefabRoot,
        HearthTvTerminalController terminal,
        HearthTerminalMode mode)
    {
        Transform previous =
            FindNamed(prefabRoot.transform, "V2_ClosureTerminalChrome");
        if (previous != null)
        {
            UnityEngine.Object.DestroyImmediate(previous.gameObject);
        }

        GameObject chrome = new GameObject(
            "V2_ClosureTerminalChrome",
            typeof(RectTransform));
        chrome.layer = prefabRoot.layer;
        chrome.transform.SetParent(prefabRoot.transform, false);
        RectTransform chromeRect = chrome.GetComponent<RectTransform>();
        SetStretch(chromeRect, 0f, 0f, 0f, 0f);
        chrome.transform.SetAsLastSibling();

        TMP_Text terminalLabel =
            CreateOrGetText(chrome.transform, "TerminalLabel", "DOORWAY TERMINAL");
        SetTopLeft(terminalLabel.rectTransform, 76f, 62f, 420f, 32f);
        ConfigureText(
            terminalLabel,
            24f,
            TextAlignmentOptions.TopLeft,
            Color.white,
            FontStyles.Bold);

        TMP_Text residentLabel =
            CreateOrGetText(chrome.transform, "ResidentId", "17F-01");
        SetTopLeft(residentLabel.rectTransform, 76f, 96f, 300f, 44f);
        ConfigureText(
            residentLabel,
            32f,
            TextAlignmentOptions.TopLeft,
            Color.white,
            FontStyles.Bold);

        TMP_Text status =
            CreateOrGetText(chrome.transform, "Status", string.Empty);
        SetTopRight(status.rectTransform, 76f, 64f, 400f, 40f);
        ConfigureText(
            status,
            19f,
            TextAlignmentOptions.TopRight,
            ColdWhite,
            FontStyles.Bold);

        Image before = CreateTab(
            chrome.transform,
            "BeforeTab",
            310f,
            150f,
            310f,
            52f);
        TMP_Text beforeText = CreateTabText(before.transform, "Label");
        beforeText.text = "BEFORE ACQUISITION";

        Image after = CreateTab(
            chrome.transform,
            "AfterTab",
            640f,
            150f,
            310f,
            52f);
        TMP_Text afterText = CreateTabText(after.transform, "Label");
        afterText.text = "AFTER ACQUISITION";

        Image primary = CreateTab(
            chrome.transform,
            "PrimaryActionTab",
            1274f,
            150f,
            570f,
            52f);
        TMP_Text primaryText = CreateTabText(primary.transform, "Label");
        primaryText.text =
            mode == HearthTerminalMode.Home ? "ENTER HOME" : "PRIMARY ACTION";

        Image rule = CreateImage(chrome.transform, "HeaderRule", LowSaturationBlue);
        SetTopLeft(rule.rectTransform, 76f, 218f, 1768f, 2f);

        TMP_Text footer =
            CreateOrGetText(
                chrome.transform,
                "Footer",
                "LEFT / RIGHT  SELECT     SPACE  CONFIRM     ESC  EXIT");
        SetBottomRight(footer.rectTransform, 76f, 42f, 900f, 36f);
        ConfigureText(
            footer,
            20f,
            TextAlignmentOptions.BottomRight,
            Color.white,
            FontStyles.Bold);

        HearthTerminalCompactChromeView view =
            prefabRoot.GetComponent<HearthTerminalCompactChromeView>();
        if (view == null)
        {
            view =
                prefabRoot.AddComponent<HearthTerminalCompactChromeView>();
        }

        view.Configure(
            terminal,
            chrome,
            terminalLabel,
            residentLabel,
            beforeText,
            afterText,
            primaryText,
            status,
            footer,
            before,
            after,
            primary);
    }

    private static Image CreateTab(
        Transform parent,
        string name,
        float x,
        float y,
        float width,
        float height)
    {
        Image image = CreateImage(
            parent,
            name,
            new Color(0.09f, 0.14f, 0.21f, 0.9f));
        Sprite sprite =
            AssetDatabase.LoadAssetAtPath<Sprite>(ButtonFramePath);
        if (sprite != null)
        {
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
        }
        SetTopLeft(image.rectTransform, x, y, width, height);
        return image;
    }

    private static TMP_Text CreateTabText(Transform parent, string name)
    {
        TMP_Text text = CreateOrGetText(parent, name, string.Empty);
        SetStretch(text.rectTransform, 18f, 10f, 18f, 10f);
        ConfigureText(
            text,
            22f,
            TextAlignmentOptions.Center,
            Color.white,
            FontStyles.Bold);
        return text;
    }

    private static void DisableLegacyTerminalChrome(Transform root)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform current = transforms[i];
            string name = current.name;
            if (name == "KeyboardNavigationRoot" ||
                name == "V2_FooterRule" ||
                name == "TerminalLabel" ||
                name == "ResidentId" ||
                name == "HeaderRule" ||
                name == "AccentRule" ||
                name.StartsWith("Tab_", StringComparison.Ordinal) ||
                name == "NavigationRule")
            {
                current.gameObject.SetActive(false);
            }
        }
    }

    private static void DisableHomeLegacyLabels(Transform root)
    {
        Transform slide = FindNamed(root, "TerminalSlide01_17F04Home");
        if (slide == null)
        {
            return;
        }

        string[] legacyNames =
        {
            "AccessLabel",
            "UnitLabel",
            "Welcome",
            "Personal",
            "Confirm"
        };
        for (int i = 0; i < legacyNames.Length; i++)
        {
            Transform legacy = slide.Find(legacyNames[i]);
            if (legacy != null)
            {
                legacy.gameObject.SetActive(false);
            }
        }
    }

    private static void ApplyPersistentHumanLayout(Transform persistent)
    {
        TMP_Text[] texts = persistent.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text text = texts[i];
            string value = (text.text ?? string.Empty).Trim();
            if (value == "COMPANION UNIT · ACTIVE")
            {
                SetTopLeft(text.rectTransform, 64f, 48f, 448f, 28f);
                ConfigureText(
                    text,
                    17f,
                    TextAlignmentOptions.TopLeft,
                    GreyBlue,
                    FontStyles.Normal);
            }
            else if (value.StartsWith("MIA ·", StringComparison.Ordinal))
            {
                SetTopLeft(text.rectTransform, 64f, 78f, 448f, 38f);
                ConfigureText(
                    text,
                    27f,
                    TextAlignmentOptions.TopLeft,
                    ColdWhite,
                    FontStyles.Normal);
            }
            else if (value == "CURRENT TASK")
            {
                SetTopRight(text.rectTransform, 64f, 48f, 448f, 28f);
                ConfigureText(
                    text,
                    17f,
                    TextAlignmentOptions.TopRight,
                    GreyBlue,
                    FontStyles.Normal);
            }
            else if (value.Contains("NIGHT ROUNDS") ||
                     value.Contains("TONIGHT'S ROUNDS"))
            {
                SetTopRight(text.rectTransform, 64f, 80f, 560f, 54f);
                ConfigureText(
                    text,
                    22f,
                    TextAlignmentOptions.TopRight,
                    ColdWhite,
                    FontStyles.Normal);
            }
        }
    }

    private static void ApplyPhotoLayout(Transform root)
    {
        SetNamedRectsTopLeft(root, "V2_PhotoArchiveHeading", 373f, 68f, 800f, 44f);
        SetNamedRectsTopLeft(root, "V2_PhotoArchiveUnit", 373f, 116f, 520f, 28f);
        SetNamedRectsTopLeft(root, "V2_PhotoViewport", 373f, 152f, 1174f, 660f);
        SetNamedRectsTopLeft(root, "V2_PhotoMetadata", 397f, 746f, 520f, 56f);
        SetNamedRectsTopLeft(root, "V2_PhotoFieldUnit", 432f, 830f, 1056f, 132f);
        SetNamedRectsBottomLeft(root, "V2_PhotoPage", 240f, 52f, 260f, 30f);
        SetNamedActive(root, "V2_PhotoReturnHint", false);

        ConfigureNamedTexts(
            root,
            "V2_PhotoArchiveHeading",
            30f,
            TextAlignmentOptions.TopLeft,
            ColdWhite,
            FontStyles.Normal);
        ConfigureNamedTexts(
            root,
            "V2_PhotoArchiveUnit",
            18f,
            TextAlignmentOptions.TopLeft,
            GreyBlue,
            FontStyles.Normal);

        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].name == "V2_PhotoViewport")
            {
                ApplyFrame(
                    transforms[i] as RectTransform,
                    PhotoFramePath,
                    GreyBlue,
                    true);
            }
        }

        ApplyPhotoPageControls(root, "Slide07_Photo2023");
        ApplyPhotoPageControls(root, "Slide08_Photo2026");
    }

    private static void ApplyInteractiveButtonFrames(Transform root)
    {
        Button[] buttons = root.GetComponentsInChildren<Button>(true);
        Sprite frame = AssetDatabase.LoadAssetAtPath<Sprite>(ButtonFramePath);
        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];
            Image image = button.targetGraphic as Image;
            if (image == null)
            {
                image = button.GetComponent<Image>();
            }

            if (frame == null)
            {
                continue;
            }

            if (image != null && image.sprite == frame)
            {
                image.sprite = null;
                image.type = Image.Type.Simple;
                image.color = new Color(1f, 1f, 1f, 0f);
                image.raycastTarget = true;
                button.targetGraphic = image;
            }

            Image fill = CreateOrGetImage(
                button.transform,
                "V2_ButtonFill",
                string.Empty,
                new Color(
                    DeepBlueBlack.r,
                    DeepBlueBlack.g,
                    DeepBlueBlack.b,
                    0.42f),
                false);
            fill.sprite = null;
            fill.type = Image.Type.Simple;
            fill.raycastTarget = false;
            SetStretch(fill.rectTransform, 2f, 2f, 2f, 2f);
            fill.transform.SetAsFirstSibling();

            Image outline = CreateOrGetImage(
                button.transform,
                "V2_VectorButtonFrame",
                ButtonFramePath,
                new Color(
                    LowSaturationBlue.r,
                    LowSaturationBlue.g,
                    LowSaturationBlue.b,
                    0.88f),
                true);
            SetStretch(outline.rectTransform, 0f, 0f, 0f, 0f);
            outline.raycastTarget = false;
            outline.transform.SetAsLastSibling();
        }
    }

    private static void ApplyPhotoPageControls(
        Transform root,
        string pageName)
    {
        Transform page = FindNamed(root, pageName);
        if (page == null)
        {
            return;
        }

        Transform closeButton = FindNamed(page, "Button_CloseStory");
        if (closeButton is RectTransform closeRect)
        {
            SetBottomRight(closeRect, 240f, 48f, 340f, 56f);
            TMP_Text label = CreateOrGetText(
                closeButton,
                "V2_ReturnLabel",
                "SPACE  RETURN");
            SetStretch(label.rectTransform, 20f, 10f, 20f, 10f);
            ConfigureText(
                label,
                18f,
                TextAlignmentOptions.Center,
                ColdWhite,
                FontStyles.Bold);
            label.transform.SetAsLastSibling();
        }

        Transform fieldUnitText = FindNamed(page, "V2_PhotoFieldUnit");
        if (fieldUnitText != null)
        {
            Image frame = CreateOrGetImage(
                page,
                "V2_PhotoFieldUnitFrame",
                FieldUnitFramePath,
                new Color(
                    LowSaturationBlue.r,
                    LowSaturationBlue.g,
                    LowSaturationBlue.b,
                    0.82f),
                true);
            SetTopLeft(frame.rectTransform, 432f, 830f, 1056f, 132f);
            frame.transform.SetSiblingIndex(
                Mathf.Max(0, fieldUnitText.GetSiblingIndex()));
        }
    }

    private static void ApplyShutdownAndTakeoverFrames(Transform root)
    {
        ApplyPageFrame(
            root,
            "Slide10_ShutdownConfirm",
            "V2_ShutdownModalFrame",
            ShutdownFramePath,
            555f,
            222f,
            810f,
            620f,
            LowSaturationBlue);

        Transform shutdownPage =
            FindNamed(root, "Slide10_ShutdownConfirm");
        if (shutdownPage != null)
        {
            SetNamedActive(shutdownPage, "Button_CANCEL", false);
            SetNamedActive(shutdownPage, "Text_007_CANCEL", false);
            SetNamedRectsTopLeft(
                shutdownPage,
                "Button_CONFIRM",
                630f,
                686f,
                660f,
                106f);
            SetNamedRectsTopLeft(
                shutdownPage,
                "Text_005_CONFIRM",
                690f,
                712f,
                540f,
                54f);
        }

        ApplyPageFrame(
            root,
            "Slide11_Warning01",
            "V2_WarningModalFrame",
            WarningFramePath,
            600f,
            280f,
            720f,
            360f,
            Red);
        ApplyPageFrame(
            root,
            "Slide12_Warning02",
            "V2_WarningModalFrame",
            WarningFramePath,
            600f,
            280f,
            720f,
            360f,
            Red);
        ApplyPageFrame(
            root,
            "Slide13_Warning03",
            "V2_WarningModalFrame",
            WarningFramePath,
            600f,
            280f,
            720f,
            360f,
            Red);
    }

    private static void ApplyPageFrame(
        Transform root,
        string pageName,
        string frameName,
        string spritePath,
        float x,
        float y,
        float width,
        float height,
        Color color)
    {
        Transform page = FindNamed(root, pageName);
        if (page == null)
        {
            return;
        }

        Image frame = CreateOrGetImage(
            page,
            frameName,
            spritePath,
            new Color(color.r, color.g, color.b, 0.9f),
            true);
        SetTopLeft(frame.rectTransform, x, y, width, height);
        frame.transform.SetAsFirstSibling();
    }

    private static void ApplyTerminalContentFrames(Transform root)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform current = transforms[i];
            string spritePath = null;
            string frameName = null;
            if (current.name == "HouseholdIntroduction" ||
                current.name == "FieldUnitPanel")
            {
                spritePath = TerminalInfoFramePath;
                frameName = "V2_VectorInfoFrame";
                Transform legacyRule =
                    current.Find(
                        current.name == "HouseholdIntroduction"
                            ? "IntroductionRule"
                            : "FieldUnitRule");
                if (legacyRule != null)
                {
                    legacyRule.gameObject.SetActive(false);
                }
            }
            else if (current.name.StartsWith(
                         "Portrait_",
                         StringComparison.Ordinal))
            {
                spritePath = TerminalPortraitFramePath;
                frameName = "V2_VectorPortraitFrame";
            }

            if (spritePath == null)
            {
                continue;
            }

            Image frame = CreateOrGetImage(
                current,
                frameName,
                spritePath,
                new Color(
                    LowSaturationBlue.r,
                    LowSaturationBlue.g,
                    LowSaturationBlue.b,
                    0.9f),
                true);
            SetStretch(frame.rectTransform, 0f, 0f, 0f, 0f);
            frame.transform.SetAsLastSibling();
        }
    }

    private static void ApplyOverlayFrame(
        RectTransform parent,
        string frameName,
        string spritePath,
        Color color)
    {
        if (parent == null)
        {
            return;
        }

        Image frame = CreateOrGetImage(
            parent,
            frameName,
            spritePath,
            new Color(color.r, color.g, color.b, 0.84f),
            true);
        SetStretch(frame.rectTransform, 0f, 0f, 0f, 0f);
        frame.transform.SetAsLastSibling();
    }

    private static void ApplyTerminalTextContrast(Transform root)
    {
        Transform terminalChrome =
            FindNamed(root, "V2_ClosureTerminalChrome");
        TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text text = texts[i];
            if (text == null ||
                (terminalChrome != null &&
                 text.transform.IsChildOf(terminalChrome)))
            {
                continue;
            }

            string value = (text.text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(value))
            {
                continue;
            }

            bool heading =
                value.IndexOf(
                    "HOUSEHOLD INTRODUCTION",
                    StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.Equals(
                    "FIELD UNIT",
                    StringComparison.OrdinalIgnoreCase) ||
                value.IndexOf(
                    "PHOTO PENDING",
                    StringComparison.OrdinalIgnoreCase) >= 0;
            text.color = heading ? ColdWhite : LowSaturationBlue;
            text.enableAutoSizing = false;
            text.overflowMode = TextOverflowModes.Overflow;
        }
    }

    private static void ApplyDecisionLayout(Transform root)
    {
        SetNamedRectsTopLeft(root, "V2_FinalChoiceHeading", 432f, 232f, 1056f, 52f);
        SetNamedRectsTopLeft(root, "FinalChoiceTarget_A", 432f, 350f, 1056f, 112f);
        SetNamedRectsTopLeft(root, "FinalChoiceTarget_B", 432f, 486f, 1056f, 112f);
        SetNamedRectsBottomCenter(
            root,
            "FinalChoiceInputHint",
            0f,
            78f,
            900f,
            42f);
        SetNamedActive(root, "V2_FinalChoiceHint", false);

        Transform[] transforms =
            root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform page = transforms[i];
            if (page.name != "Slide09_FinalChoice" &&
                page.name != "Slide14_FinalChoiceReturn")
            {
                continue;
            }

            ApplyDecisionPageLayout(page);
        }

        RectTransform focus = FindRect(root, "FinalChoiceFocus");
        Image focusImage =
            focus != null ? focus.GetComponent<Image>() : null;
        if (focusImage != null)
        {
            focusImage.color =
                new Color(
                    LowSaturationBlue.r,
                    LowSaturationBlue.g,
                    LowSaturationBlue.b,
                    0.28f);
            focusImage.raycastTarget = false;
        }
        if (focus != null)
        {
            focus.gameObject.SetActive(false);
        }

        HearthFirstPersonHudInput input =
            root.GetComponentInChildren<HearthFirstPersonHudInput>(true);
        if (input != null)
        {
            input.SetFinalChoiceInputProfile(
                new HearthFinalChoiceInputProfile(
                    HearthFinalChoiceNavigationAxis.Vertical,
                    false,
                    false));
        }
    }

    private static void ApplyDecisionPageLayout(Transform page)
    {
        SetNamedRectsTopLeft(page, "ShapeFill_001", 432f, 350f, 1056f, 112f);
        SetNamedRectsTopLeft(page, "Button_ANSWER_LILY", 432f, 350f, 1056f, 112f);
        SetNamedRectsTopLeft(page, "Text_002_A", 480f, 382f, 64f, 48f);
        SetNamedRectsTopLeft(
            page,
            "Text_003_ANSWER_LILY_YOURSELF",
            624f,
            382f,
            560f,
            48f);
        SetNamedRectsTopLeft(page, "V2_FinalChoiceRuleA", 480f, 350f, 4f, 112f);
        SetNamedActive(page, "V2_FinalChoiceRuleA", false);
        SetNamedRectsTopLeft(
            page,
            "V2_FinalChoiceRecommended",
            1256f,
            382f,
            184f,
            48f);

        SetNamedRectsTopLeft(page, "ShapeFill_004", 432f, 486f, 1056f, 112f);
        SetNamedRectsTopLeft(
            page,
            "Button_COMPANION_ANSWER",
            432f,
            486f,
            1056f,
            112f);
        SetNamedRectsTopLeft(page, "Text_005_B", 480f, 518f, 64f, 48f);
        SetNamedRectsTopLeft(
            page,
            "Text_006_LET_THE_COMPANION_ANSWER_FOR_HER",
            624f,
            518f,
            720f,
            48f);
        SetNamedRectsTopLeft(page, "V2_FinalChoiceRuleB", 480f, 486f, 4f, 112f);
        SetNamedActive(page, "V2_FinalChoiceRuleB", false);
        SetNamedRectsTopLeft(page, "V2_FinalChoiceRule", 432f, 310f, 1056f, 2f);

        ConfigureText(
            FindText(page, "Text_002_A"),
            28f,
            TextAlignmentOptions.MidlineLeft,
            ColdWhite,
            FontStyles.Bold);
        ConfigureText(
            FindText(page, "Text_003_ANSWER_LILY_YOURSELF"),
            26f,
            TextAlignmentOptions.MidlineLeft,
            ColdWhite,
            FontStyles.Normal);
        ConfigureText(
            FindText(page, "Text_005_B"),
            28f,
            TextAlignmentOptions.MidlineLeft,
            ColdWhite,
            FontStyles.Bold);
        ConfigureText(
            FindText(
                page,
                "Text_006_LET_THE_COMPANION_ANSWER_FOR_HER"),
            26f,
            TextAlignmentOptions.MidlineLeft,
            ColdWhite,
            FontStyles.Normal);

        SetNamedActive(page, "ShapeFill_001", false);
        SetNamedActive(page, "ShapeFill_004", false);
        EnsureChoiceSelectionFill(page, "Button_ANSWER_LILY");
        EnsureChoiceSelectionFill(page, "Button_COMPANION_ANSWER");
    }

    private static void EnsureChoiceSelectionFill(
        Transform page,
        string buttonName)
    {
        RectTransform button = FindRect(page, buttonName);
        if (button == null)
        {
            return;
        }

        if (button.GetComponent<RectMask2D>() == null)
        {
            button.gameObject.AddComponent<RectMask2D>();
        }

        Transform existing = button.Find("SelectionFill");
        Image selection =
            existing != null ? existing.GetComponent<Image>() : null;
        if (selection == null)
        {
            selection = CreateImage(
                button,
                "SelectionFill",
                new Color(
                    LowSaturationBlue.r,
                    LowSaturationBlue.g,
                    LowSaturationBlue.b,
                    0.22f));
        }

        SetStretch(selection.rectTransform, 8f, 8f, 8f, 8f);
        selection.raycastTarget = false;
        selection.transform.SetAsFirstSibling();
        selection.gameObject.SetActive(false);
    }

    private static void ApplyKeycapSizing(Transform root)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            RectTransform rect = transforms[i] as RectTransform;
            if (rect == null ||
                (rect.name != "Keycap" && rect.name != "Key"))
            {
                continue;
            }

            TMP_Text label = rect.GetComponentInChildren<TMP_Text>(true);
            bool space =
                label != null &&
                (label.text ?? string.Empty).IndexOf(
                    "SPACE",
                    StringComparison.OrdinalIgnoreCase) >= 0;
            rect.sizeDelta = new Vector2(space ? 96f : 64f, 40f);
        }
    }

    private static void EditPrefab(
        string path,
        Action<GameObject> edit)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
        {
            Debug.LogWarning(
                "[HearthUiV2ClosureEditor] Prefab missing: " + path);
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

    private static void ValidateHumanPrefab(List<string> issues)
    {
        GameObject root = LoadPrefabForValidation(HumanPrefab, issues);
        if (root == null)
        {
            return;
        }

        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        if (scaler == null ||
            scaler.uiScaleMode !=
                CanvasScaler.ScaleMode.ScaleWithScreenSize ||
            scaler.referenceResolution != new Vector2(1920f, 1080f))
        {
            issues.Add(
                "Human HUD is not locked to the 1920×1080 scaling baseline.");
        }

        RectTransform location = FindRect(root.transform, "LocationHud");
        if (location == null ||
            Mathf.Abs(location.anchoredPosition.x - 64f) > 0.5f)
        {
            issues.Add(
                "Human Location HUD is not aligned to the X=64 identity edge.");
        }

        if (FindNamed(root.transform, "PlayerInteractionPrompt") == null)
        {
            issues.Add(
                "Human HUD has no independent world-interaction prompt layer.");
        }
    }

    private static void ValidateCompanionPrefab(List<string> issues)
    {
        GameObject root = LoadPrefabForValidation(CompanionPrefab, issues);
        if (root == null)
        {
            return;
        }

        Transform frame = FindNamed(root.transform, "CompanionRobotFrame");
        Image frameImage = frame != null ? frame.GetComponent<Image>() : null;
        string framePath =
            frameImage != null && frameImage.sprite != null
                ? AssetDatabase.GetAssetPath(frameImage.sprite)
                : string.Empty;
        if (frameImage == null || framePath != CompanionFramePath)
        {
            issues.Add(
                "Companion HUD is not using the single approved transparent " +
                "fullscreen technology frame.");
        }

        RequireActive(root.transform, "V2_REC", "Companion REC marker", issues);
        RequireActive(
            root.transform,
            "V2_CurrentTask",
            "Companion Current Task",
            issues);
        RequireInactive(
            root.transform,
            "DataStreamView",
            "obsolete Monitor Bus",
            issues);
        RequireInactive(
            root.transform,
            "V2_PhysicalFeedLabel",
            "obsolete Physical Unit Feed label",
            issues);
        RequireInactive(
            root.transform,
            "V2_PhysicalFeedRule",
            "obsolete Physical Unit Feed rule",
            issues);
        RequireInactive(
            root.transform,
            "V2_InspectionReturn",
            "replay Esc/Return hint",
            issues);
    }

    private static void ValidateOpenSceneCompanionInstances(
        List<string> issues)
    {
        HearthCompanionHudController[] controllers =
            Resources.FindObjectsOfTypeAll<HearthCompanionHudController>();
        for (int i = 0; i < controllers.Length; i++)
        {
            HearthCompanionHudController controller = controllers[i];
            if (controller == null ||
                !controller.gameObject.scene.IsValid() ||
                !controller.gameObject.scene.isLoaded)
            {
                continue;
            }

            Transform frame =
                FindNamed(controller.transform, "CompanionRobotFrame");
            Image image = frame != null ? frame.GetComponent<Image>() : null;
            string path =
                image != null && image.sprite != null
                    ? AssetDatabase.GetAssetPath(image.sprite)
                    : string.Empty;
            if (image == null ||
                path != CompanionFramePath ||
                image.color.a < 0.75f)
            {
                issues.Add(
                    "Open-scene Companion HUD must use one visible " +
                    "CompanionRobotFrame without a transparent scene override.");
            }

            Transform legacyStatus =
                FindNamed(controller.transform, "V2_Status");
            if (legacyStatus != null && legacyStatus.gameObject.activeSelf)
            {
                issues.Add(
                    "Open-scene Companion HUD still enables legacy V2_Status.");
            }
        }
    }

    private static void ValidateTerminalPrefab(
        string path,
        HearthTerminalMode expectedMode,
        bool requiresCompactChrome,
        List<string> issues)
    {
        GameObject root = LoadPrefabForValidation(path, issues);
        if (root == null)
        {
            return;
        }

        HearthTvTerminalController terminal =
            root.GetComponent<HearthTvTerminalController>() ??
            root.GetComponentInChildren<HearthTvTerminalController>(true);
        if (terminal == null)
        {
            issues.Add(path + " has no HearthTvTerminalController.");
            return;
        }

        if (terminal.TerminalMode != expectedMode)
        {
            issues.Add(
                path +
                " has the wrong terminal strategy: " +
                terminal.TerminalMode +
                ".");
        }

        Transform chrome =
            FindNamed(root.transform, "V2_ClosureTerminalChrome");
        if (requiresCompactChrome)
        {
            if (chrome == null ||
                root.GetComponent<HearthTerminalCompactChromeView>() == null)
            {
                issues.Add(path + " is missing the compact three-focus chrome.");
                return;
            }

            RequireActive(
                chrome,
                "PrimaryActionTab",
                path + " primary action",
                issues);
            ValidateNoActiveLegacyTerminalChrome(root.transform, chrome, path, issues);
        }
        else if (chrome != null && chrome.gameObject.activeSelf)
        {
            issues.Add(
                path +
                " incorrectly contains doorway/home fullscreen navigation chrome.");
        }
    }

    private static void ValidateSubtitlePrefab(List<string> issues)
    {
        GameObject root = LoadPrefabForValidation(
            HearthSubtitleV2VisualBuilder.PrefabPath,
            issues);
        if (root == null)
        {
            return;
        }

        RequireActive(root.transform, "VisualRoot", "subtitle VisualRoot", issues);
        RequireInactive(
            root.transform,
            "SpeakerTab",
            "legacy shared subtitle speaker tab",
            issues);
        RequireInactive(
            root.transform,
            "AccentRule",
            "legacy shared subtitle accent rule",
            issues);
        RequirePresent(
            root.transform,
            "FormalFrame",
            "formal dialogue vector frame",
            issues);
        RequirePresent(
            root.transform,
            "AuxiliaryFrame",
            "Field Unit auxiliary vector frame",
            issues);
        RequirePresent(
            root.transform,
            "SpeakerTabLeft",
            "left formal speaker tab",
            issues);
        RequirePresent(
            root.transform,
            "SpeakerTabRight",
            "right formal speaker tab",
            issues);
        RequirePresent(
            root.transform,
            "AdvanceHint",
            "manual Space advance hint",
            issues);

        TMP_Text body = FindText(root.transform, "Body");
        if (body == null ||
            body.enableAutoSizing ||
            body.overflowMode != TextOverflowModes.Overflow)
        {
            issues.Add(
                "Subtitle body is not fixed-size and overflow-safe.");
        }
    }

    private static GameObject LoadPrefabForValidation(
        string path,
        List<string> issues)
    {
        GameObject root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (root == null)
        {
            issues.Add("Missing prefab: " + path);
        }
        return root;
    }

    private static void ValidateNoActiveLegacyTerminalChrome(
        Transform root,
        Transform approvedChrome,
        string path,
        List<string> issues)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform current = transforms[i];
            if (current == approvedChrome ||
                current.IsChildOf(approvedChrome) ||
                !current.gameObject.activeSelf)
            {
                continue;
            }

            string name = current.name;
            if (name == "KeyboardNavigationRoot" ||
                name == "V2_FooterRule" ||
                name == "NavigationRule" ||
                name.StartsWith("Tab_", StringComparison.Ordinal))
            {
                issues.Add(
                    path +
                    " still has active legacy terminal chrome: " +
                    name +
                    ".");
            }
        }
    }

    private static void RequireActive(
        Transform root,
        string name,
        string label,
        List<string> issues)
    {
        Transform found = FindNamed(root, name);
        if (found == null || !found.gameObject.activeSelf)
        {
            issues.Add(label + " is missing or inactive.");
        }
    }

    private static void RequireInactive(
        Transform root,
        string name,
        string label,
        List<string> issues)
    {
        Transform found = FindNamed(root, name);
        if (found != null && found.gameObject.activeSelf)
        {
            issues.Add(label + " must be inactive in the V2 prefab.");
        }
    }

    private static void RequirePresent(
        Transform root,
        string name,
        string label,
        List<string> issues)
    {
        if (FindNamed(root, name) == null)
        {
            issues.Add(label + " is missing.");
        }
    }

    private static Transform FindNamed(Transform root, string name)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].name == name)
            {
                return transforms[i];
            }
        }
        return null;
    }

    private static RectTransform FindRect(Transform root, string name)
    {
        return FindNamed(root, name) as RectTransform;
    }

    private static Image FindImage(Transform root, string name)
    {
        Transform found = FindNamed(root, name);
        return found != null ? found.GetComponent<Image>() : null;
    }

    private static TMP_Text FindText(Transform root, string name)
    {
        Transform found = FindNamed(root, name);
        return found != null ? found.GetComponent<TMP_Text>() : null;
    }

    private static void ConfigureNamedTexts(
        Transform root,
        string name,
        float fontSize,
        TextAlignmentOptions alignment,
        Color color,
        FontStyles style)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].name != name)
            {
                continue;
            }

            TMP_Text text = transforms[i].GetComponent<TMP_Text>();
            if (text != null)
            {
                ConfigureText(text, fontSize, alignment, color, style);
            }
        }
    }

    private static TMP_Text CreateOrGetText(
        Transform parent,
        string name,
        string value)
    {
        Transform existing = parent.Find(name);
        TMP_Text text =
            existing != null ? existing.GetComponent<TMP_Text>() : null;
        if (text == null)
        {
            GameObject textObject =
                new GameObject(name, typeof(RectTransform));
            textObject.layer = parent.gameObject.layer;
            textObject.transform.SetParent(parent, false);
            text = textObject.AddComponent<TextMeshProUGUI>();
            if (TMP_Settings.defaultFontAsset != null)
            {
                text.font = TMP_Settings.defaultFontAsset;
            }
        }

        text.text = value;
        return text;
    }

    private static Image CreateImage(
        Transform parent,
        string name,
        Color color)
    {
        GameObject imageObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        imageObject.layer = parent.gameObject.layer;
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static Image CreateOrGetImage(
        Transform parent,
        string name,
        string spritePath,
        Color color,
        bool sliced)
    {
        Transform existing = parent.Find(name);
        Image image =
            existing != null ? existing.GetComponent<Image>() : null;
        if (image == null)
        {
            image = CreateImage(parent, name, color);
        }

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        image.sprite = sprite;
        image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        image.preserveAspect = false;
        image.color = color;
        image.raycastTarget = false;
        return image;
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
        text.overflowMode = TextOverflowModes.Overflow;
        text.alignment = alignment;
        text.color = color;
        text.fontStyle = style;
        text.raycastTarget = false;
    }

    private static void AlignNamedText(
        Transform root,
        string name,
        TextAlignmentOptions alignment,
        float size,
        Color color)
    {
        TMP_Text text = FindText(root, name);
        ConfigureText(
            text,
            size,
            alignment,
            color,
            text != null ? text.fontStyle : FontStyles.Normal);
    }

    private static void ApplyFrame(
        RectTransform rect,
        string spritePath,
        Color color,
        bool sliced)
    {
        if (rect == null)
        {
            return;
        }

        Image image = rect.GetComponent<Image>();
        if (image == null)
        {
            return;
        }

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite != null)
        {
            image.sprite = sprite;
            image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        }
        image.color = color;
        image.raycastTarget = false;
    }

    private static void SetNamedActive(
        Transform root,
        string name,
        bool active)
    {
        if (root == null)
        {
            return;
        }

        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].name == name)
            {
                transforms[i].gameObject.SetActive(active);
            }
        }
    }

    private static void TintNamedImage(
        Transform root,
        string name,
        Color color)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].name != name)
            {
                continue;
            }

            Image image = transforms[i].GetComponent<Image>();
            if (image != null)
            {
                image.color = color;
            }
        }
    }

    private static void SetNamedRectsTopLeft(
        Transform root,
        string name,
        float x,
        float y,
        float width,
        float height)
    {
        SetNamedRects(
            root,
            name,
            rect => SetTopLeft(rect, x, y, width, height));
    }

    private static void SetNamedRectsBottomLeft(
        Transform root,
        string name,
        float x,
        float y,
        float width,
        float height)
    {
        SetNamedRects(
            root,
            name,
            rect => SetBottomLeft(rect, x, y, width, height));
    }

    private static void SetNamedRectsBottomRight(
        Transform root,
        string name,
        float right,
        float bottom,
        float width,
        float height)
    {
        SetNamedRects(
            root,
            name,
            rect => SetBottomRight(rect, right, bottom, width, height));
    }

    private static void SetNamedRectsBottomCenter(
        Transform root,
        string name,
        float x,
        float bottom,
        float width,
        float height)
    {
        SetNamedRects(
            root,
            name,
            rect => SetBottomCenter(rect, x, bottom, width, height));
    }

    private static void SetNamedRects(
        Transform root,
        string name,
        Action<RectTransform> set)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            RectTransform rect = transforms[i] as RectTransform;
            if (rect != null && rect.name == name)
            {
                set(rect);
            }
        }
    }

    private static void SetTopLeft(
        RectTransform rect,
        float x,
        float y,
        float width,
        float height)
    {
        if (rect == null) return;
        rect.anchorMin = Vector2.up;
        rect.anchorMax = Vector2.up;
        rect.pivot = Vector2.up;
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void SetTopRight(
        RectTransform rect,
        float right,
        float y,
        float width,
        float height)
    {
        if (rect == null) return;
        rect.anchorMin = Vector2.one;
        rect.anchorMax = Vector2.one;
        rect.pivot = Vector2.one;
        rect.anchoredPosition = new Vector2(-right, -y);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void SetTopCenter(
        RectTransform rect,
        float x,
        float y,
        float width,
        float height)
    {
        if (rect == null) return;
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void SetBottomLeft(
        RectTransform rect,
        float x,
        float y,
        float width,
        float height)
    {
        if (rect == null) return;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = Vector2.zero;
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void SetBottomRight(
        RectTransform rect,
        float right,
        float bottom,
        float width,
        float height)
    {
        if (rect == null) return;
        rect.anchorMin = Vector2.right;
        rect.anchorMax = Vector2.right;
        rect.pivot = Vector2.right;
        rect.anchoredPosition = new Vector2(-right, bottom);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void SetBottomCenter(
        RectTransform rect,
        float x,
        float bottom,
        float width,
        float height)
    {
        if (rect == null) return;
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(x, bottom);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void SetStretch(
        RectTransform rect,
        float left,
        float top,
        float right,
        float bottom)
    {
        if (rect == null) return;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }
}
#endif
