using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class MinLoopSceneAutoBinder
{
    private const string RootName = "MIN_LOOP_ROOT";

    [MenuItem("Tools/Min Loop/Auto Bind 17F-01 Scene References")]
    public static void AutoBindSceneReferences()
    {
        AutoBindReport report = new AutoBindReport();

        GameObject root = FindSceneGameObject(RootName);
        if (root == null)
        {
            bool shouldCreateSkeleton = EditorUtility.DisplayDialog(
                "缺少最小循环骨架",
                "当前场景没有 MIN_LOOP_ROOT。是否先创建 17F-01 标准骨架，然后继续自动绑定？",
                "创建并绑定",
                "只绑定现有对象");

            if (shouldCreateSkeleton)
            {
                MinLoopSceneSkeletonCreator.CreateSceneSkeleton();
                root = FindSceneGameObject(RootName);
                report.Added("创建/更新 MIN_LOOP_ROOT 标准骨架。");
            }
        }

        MinLoopFlowController flowController = FindSceneComponent<MinLoopFlowController>("MinLoopFlowController");
        TrustStateController trustStateController = FindSceneComponent<TrustStateController>("TrustStateController");
        ViewSwitchController viewSwitchController = FindSceneComponent<ViewSwitchController>("ViewSwitchController");
        MinLoopStageObjectActivator stageObjectActivator = FindSceneComponent<MinLoopStageObjectActivator>("MinLoopStageObjectActivator");
        MinLoopStageAnchorController stageAnchorController = FindSceneComponent<MinLoopStageAnchorController>("MinLoopStageAnchorController");
        MinLoopStageCueController stageCueController = FindSceneComponent<MinLoopStageCueController>("MinLoopStageCueController");
        MinLoopLightingStateController lightingStateController = FindSceneComponent<MinLoopLightingStateController>("MinLoopLightingStateController");
        MinLoopAudioStateController audioStateController = FindSceneComponent<MinLoopAudioStateController>("MinLoopAudioStateController");
        MinLoopDebugHotkeys debugHotkeys = FindSceneComponent<MinLoopDebugHotkeys>("MinLoopDebugHotkeys");
        MinLoopTerminalPresenter terminalPresenter = FindSceneComponent<MinLoopTerminalPresenter>("MinLoopTerminalPresenter");
        MinLoopSubtitlePlayer subtitlePlayer = FindSceneComponent<MinLoopSubtitlePlayer>("MinLoopSubtitlePlayer");
        MinLoopRobotHudPresenter robotHudPresenter = FindSceneComponent<MinLoopRobotHudPresenter>("MinLoopRobotHudPresenter");
        MinLoopObjectivePresenter objectivePresenter = FindSceneComponent<MinLoopObjectivePresenter>("MinLoopObjectivePresenter");
        MinLoopTrustPresenter trustPresenter = FindSceneComponent<MinLoopTrustPresenter>("MinLoopTrustPresenter");
        ReplaySequenceController replaySequenceController = FindSceneComponent<ReplaySequenceController>("ReplaySequenceController");
        ComfortActionInteractable comfortAction = FindSceneComponent<ComfortActionInteractable>("ComfortAction_Bedside");
        ResidentTerminalFlow terminalFlow = FindSceneComponent<ResidentTerminalFlow>("Terminal_17F01_Interactable", "Terminal_17F01");
        MinLoopSceneValidator validator = FindSceneComponent<MinLoopSceneValidator>("MinLoopSceneValidator");
        TerminalUIController terminalUI = FindSceneComponent<TerminalUIController>("TerminalUIController", "Terminal UI Controller", "Terminal_UI");

        GameObject humanRoot = FindSceneGameObject("Player_Mia_Controller", "Mia_Controller", "Mia_FirstPersonController", "Player_Mia", "Mia");
        GameObject companionRoot = FindSceneGameObject("Companion_Controller", "CompanionUnit_Controller", "Robot_Controller", "Companion_FirstPersonController", "Robot");
        Transform doorLookTarget = FindSceneTransform("DoorLook_Target", "DoorLookTarget", "Child_DoorLook_Target");
        Transform miaCorridorAnchor = FindSceneTransform("Anchor_Mia_CorridorStart");
        Transform companionReplayAnchor = FindSceneTransform("Anchor_Companion_ReplayStart");
        Transform miaTerminalReturnAnchor = FindSceneTransform("Anchor_Mia_TerminalReturn");
        Transform nextResidentAnchor = FindSceneTransform("Anchor_NextResident_17F02");

        FirstPersonMovement humanMovement = FindComponentInChildren<FirstPersonMovement>(humanRoot);
        FirstPersonLook humanLook = FindComponentInChildren<FirstPersonLook>(humanRoot);
        PlayerInteraction humanInteraction = FindComponentInChildren<PlayerInteraction>(humanRoot);
        Rigidbody humanRigidbody = FindComponentInChildren<Rigidbody>(humanRoot);
        Camera humanCamera = FindComponentInChildren<Camera>(humanRoot);

        PlayerInteraction companionInteraction = FindComponentInChildren<PlayerInteraction>(companionRoot);
        Camera companionCamera = FindComponentInChildren<Camera>(companionRoot);
        Canvas robotCanvas = FindComponentInChildren<Canvas>(FindSceneGameObject("RobotCanvas", "Robot_Canvas", "CompanionCanvas", "Companion_Canvas"));
        if (robotCanvas == null)
        {
            robotCanvas = FindComponentInChildren<Canvas>(companionRoot);
        }

        InteractionFeedbackController terminalOpenFeedback = FindSceneComponent<InteractionFeedbackController>("Feedback_TerminalOpen");
        InteractionFeedbackController accessCardFeedback = FindSceneComponent<InteractionFeedbackController>("Feedback_AccessCard");
        InteractionFeedbackController replayRequestFeedback = FindSceneComponent<InteractionFeedbackController>("Feedback_ReplayRequest");
        InteractionFeedbackController dispositionSubmitFeedback = FindSceneComponent<InteractionFeedbackController>("Feedback_DispositionSubmit");
        InteractionFeedbackController comfortReadyFeedback = FindSceneComponent<InteractionFeedbackController>("Feedback_ComfortReady");
        InteractionFeedbackController morningReviewFeedback = FindSceneComponent<InteractionFeedbackController>("Feedback_MorningReview");
        InteractionFeedbackController nextResidentGuideFeedback = FindSceneComponent<InteractionFeedbackController>("Feedback_NextResidentGuide");

        Light corridorLight = FindSceneComponent<Light>("Light_Corridor_Warm");
        Light replayNightLight = FindSceneComponent<Light>("Light_Replay_Night");
        Light morningLight = FindSceneComponent<Light>("Light_Morning_Soft");

        AudioSource corridorAmbience = FindSceneComponent<AudioSource>("Audio_Corridor_Ambience");
        AudioSource replayNightAmbience = FindSceneComponent<AudioSource>("Audio_Replay_Night_Ambience");
        AudioSource morningAmbience = FindSceneComponent<AudioSource>("Audio_Morning_Ambience");

        SimpleActorCueController childActor = FindOrAddActorCue("Child_Actor", report);
        SimpleActorCueController motherActor = FindOrAddActorCue("Mother_Actor", report);
        SimpleActorCueController fatherActor = FindOrAddActorCue("Father_Actor", report);

        AssignCoreReferences(flowController, terminalPresenter, viewSwitchController, replaySequenceController, trustStateController, terminalOpenFeedback, accessCardFeedback, replayRequestFeedback, dispositionSubmitFeedback, report);
        AssignTerminalReferences(terminalFlow, terminalPresenter, terminalUI, flowController, humanMovement, humanLook, humanInteraction, humanRigidbody, report);
        AssignReplayReferences(replaySequenceController, flowController, subtitlePlayer, comfortAction, childActor, motherActor, fatherActor, doorLookTarget, report);
        AssignComfortReferences(comfortAction, replaySequenceController, report);
        AssignViewSwitchReferences(viewSwitchController, humanRoot, companionRoot, report);
        AssignPresenterReferences(objectivePresenter, robotHudPresenter, trustPresenter, flowController, trustStateController, robotCanvas, report);
        AssignBoundUIReferences(terminalUI, terminalPresenter, objectivePresenter, robotHudPresenter, trustPresenter, report);
        AssignDebugReferences(debugHotkeys, flowController, replaySequenceController, report);
        AssignStageActivatorReferences(stageObjectActivator, flowController, report);
        AssignStageAnchorReferences(stageAnchorController, flowController, humanRoot, companionRoot, miaCorridorAnchor, companionReplayAnchor, miaTerminalReturnAnchor, nextResidentAnchor, report);
        AssignStageCueReferences(stageCueController, flowController, comfortReadyFeedback, morningReviewFeedback, nextResidentGuideFeedback, report);
        AssignLightingReferences(lightingStateController, flowController, corridorLight, replayNightLight, morningLight, report);
        AssignAudioReferences(audioStateController, flowController, corridorAmbience, replayNightAmbience, morningAmbience, report);
        AssignValidatorReferences(validator, flowController, terminalFlow, terminalPresenter, viewSwitchController, stageObjectActivator, stageAnchorController, stageCueController, lightingStateController, audioStateController, replaySequenceController, comfortAction, subtitlePlayer, robotHudPresenter, objectivePresenter, trustPresenter, trustStateController, debugHotkeys, report);
        AssignInteractionCamera(humanInteraction, humanCamera, "Mia 交互相机", report);
        AssignInteractionCamera(companionInteraction, companionCamera, "陪伴单元交互相机", report);

        EnsureCollider(terminalFlow != null ? terminalFlow.gameObject : null, new Vector3(1.2f, 1.6f, 0.25f), "17F-01 终端交互 Collider", report);
        EnsureCollider(comfortAction != null ? comfortAction.gameObject : null, new Vector3(1.2f, 1.2f, 1.2f), "床边安抚交互 Collider", report);

        if (validator != null)
        {
            Undo.RecordObject(validator, "Resolve Min Loop Validator References");
            validator.ResolveReferences();
            EditorUtility.SetDirty(validator);
            report.Bound("刷新 MinLoopSceneValidator 引用缓存。");
        }

        if (report.HasChanges)
        {
            MarkActiveSceneDirty();
        }

        Selection.activeGameObject = validator != null ? validator.gameObject : (root != null ? root : Selection.activeGameObject);
        Debug.Log(report.BuildMessage(), validator != null ? validator : null);
    }

    private static void AssignCoreReferences(
        MinLoopFlowController flowController,
        MinLoopTerminalPresenter terminalPresenter,
        ViewSwitchController viewSwitchController,
        ReplaySequenceController replaySequenceController,
        TrustStateController trustStateController,
        InteractionFeedbackController terminalOpenFeedback,
        InteractionFeedbackController accessCardFeedback,
        InteractionFeedbackController replayRequestFeedback,
        InteractionFeedbackController dispositionSubmitFeedback,
        AutoBindReport report)
    {
        if (flowController == null)
        {
            report.Missing("缺少 MinLoopFlowController，总流程无法自动接线。");
            return;
        }

        AssignObject(flowController, "terminalPresenter", terminalPresenter, "总流程 -> 终端展示器", report);
        AssignObject(flowController, "viewSwitchController", viewSwitchController, "总流程 -> 视角切换器", report);
        AssignObject(flowController, "replaySequenceController", replaySequenceController, "总流程 -> 复盘控制器", report);
        AssignObject(flowController, "trustStateController", trustStateController, "总流程 -> 信任度控制器", report);
        AssignObject(flowController, "terminalOpenFeedback", terminalOpenFeedback, "总流程 -> 终端打开反馈", report, false);
        AssignObject(flowController, "accessCardFeedback", accessCardFeedback, "总流程 -> 刷工牌反馈", report, false);
        AssignObject(flowController, "replayRequestFeedback", replayRequestFeedback, "总流程 -> 调出昨夜事件反馈", report, false);
        AssignObject(flowController, "dispositionSubmitFeedback", dispositionSubmitFeedback, "总流程 -> 处置提交反馈", report, false);
    }

    private static void AssignTerminalReferences(
        ResidentTerminalFlow terminalFlow,
        MinLoopTerminalPresenter terminalPresenter,
        TerminalUIController terminalUI,
        MinLoopFlowController flowController,
        FirstPersonMovement humanMovement,
        FirstPersonLook humanLook,
        PlayerInteraction humanInteraction,
        Rigidbody humanRigidbody,
        AutoBindReport report)
    {
        AssignObject(terminalFlow, "flowController", flowController, "终端入口 -> 总流程", report);
        AssignObject(terminalPresenter, "terminalUI", terminalUI, "终端展示器 -> TerminalUIController", report, false);

        List<Behaviour> behaviours = new List<Behaviour>();
        AddIfNotNull(behaviours, humanMovement);
        AddIfNotNull(behaviours, humanLook);
        AddIfNotNull(behaviours, humanInteraction);
        AssignObjectArray(terminalPresenter, "gameplayBehavioursToDisable", behaviours.ToArray(), "终端展示器 -> 打开终端时临时禁用 Mia 控制", report, false);

        AssignObject(terminalUI, "playerMovement", humanMovement, "TerminalUIController -> Mia 移动控制", report, false);
        AssignObject(terminalUI, "playerLook", humanLook, "TerminalUIController -> Mia 视角控制", report, false);
        AssignObject(terminalUI, "playerInteraction", humanInteraction, "TerminalUIController -> Mia 交互控制", report, false);
        AssignObject(terminalUI, "playerRigidbody", humanRigidbody, "TerminalUIController -> Mia Rigidbody", report, false);
    }

    private static void AssignReplayReferences(
        ReplaySequenceController replaySequenceController,
        MinLoopFlowController flowController,
        MinLoopSubtitlePlayer subtitlePlayer,
        ComfortActionInteractable comfortAction,
        SimpleActorCueController childActor,
        SimpleActorCueController motherActor,
        SimpleActorCueController fatherActor,
        Transform doorLookTarget,
        AutoBindReport report)
    {
        AssignObject(replaySequenceController, "flowController", flowController, "复盘控制器 -> 总流程", report);
        AssignObject(replaySequenceController, "subtitlePlayer", subtitlePlayer, "复盘控制器 -> 字幕播放器", report);
        AssignObject(replaySequenceController, "comfortAction", comfortAction, "复盘控制器 -> 唯一安抚点", report, false);
        AssignObject(replaySequenceController, "childActor", childActor, "复盘控制器 -> 孩子演员", report, false);
        AssignObject(replaySequenceController, "motherActor", motherActor, "复盘控制器 -> 母亲演员", report, false);
        AssignObject(replaySequenceController, "fatherActor", fatherActor, "复盘控制器 -> 父亲演员", report, false);
        AssignObject(replaySequenceController, "doorLookTarget", doorLookTarget, "复盘控制器 -> 孩子看向门口目标点", report, false);
    }

    private static void AssignComfortReferences(ComfortActionInteractable comfortAction, ReplaySequenceController replaySequenceController, AutoBindReport report)
    {
        AssignObject(comfortAction, "sequenceController", replaySequenceController, "安抚点 -> 复盘控制器", report);

        GameObject visualRoot = FindSceneGameObject("ComfortAction_Visual", "ComfortActionVisual", "Guide_ComfortAction");
        AssignObject(comfortAction, "visualRoot", visualRoot, "安抚点 -> 可见提示物", report, false);
    }

    private static void AssignViewSwitchReferences(ViewSwitchController viewSwitchController, GameObject humanRoot, GameObject companionRoot, AutoBindReport report)
    {
        if (viewSwitchController == null)
        {
            report.Missing("缺少 ViewSwitchController，无法自动绑定 Mia/陪伴单元视角。");
            return;
        }

        AssignViewRig(viewSwitchController, "human", humanRoot, "Mia/Human", report);
        AssignViewRig(viewSwitchController, "companion", companionRoot, "Companion/Robot", report);
    }

    private static void AssignPresenterReferences(
        MinLoopObjectivePresenter objectivePresenter,
        MinLoopRobotHudPresenter robotHudPresenter,
        MinLoopTrustPresenter trustPresenter,
        MinLoopFlowController flowController,
        TrustStateController trustStateController,
        Canvas robotCanvas,
        AutoBindReport report)
    {
        AssignObject(objectivePresenter, "flowController", flowController, "目标提示 -> 总流程", report, false);
        AssignObject(robotHudPresenter, "flowController", flowController, "机器 HUD -> 总流程", report, false);
        AssignObject(robotHudPresenter, "fallbackParentCanvas", robotCanvas, "机器 HUD -> RobotCanvas", report, false);
        AssignObject(trustPresenter, "trustStateController", trustStateController, "信任度显示 -> 信任度控制器", report, false);
        AssignObject(trustPresenter, "flowController", flowController, "信任度显示 -> 总流程", report, false);
    }

    private static void AssignBoundUIReferences(
        TerminalUIController terminalUI,
        MinLoopTerminalPresenter terminalPresenter,
        MinLoopObjectivePresenter objectivePresenter,
        MinLoopRobotHudPresenter robotHudPresenter,
        MinLoopTrustPresenter trustPresenter,
        AutoBindReport report)
    {
        AssignTerminalBoundUI(terminalUI, terminalPresenter, report);
        AssignObjectiveBoundUI(objectivePresenter, report);
        AssignRobotHudBoundUI(robotHudPresenter, report);
        AssignTrustBoundUI(trustPresenter, report);
    }

    private static void AssignTerminalBoundUI(TerminalUIController terminalUI, MinLoopTerminalPresenter terminalPresenter, AutoBindReport report)
    {
        GameObject root = FindSceneGameObject(
            "Terminal_BoundUI",
            "Terminal_UI_Root",
            "Terminal_UI",
            "TerminalPanel",
            "Terminal_Panel");

        GameObject panel = FindSceneGameObject("Terminal_Panel", "TerminalPanel", "Terminal_UI_Panel");
        if (panel == null)
        {
            panel = root;
        }

        Button primaryButton = FindNamedComponent<Button>(root, "Terminal_PrimaryButton", "PrimaryButton", "Button_Primary", "Button_A", "OptionA_Button");
        Button secondaryButton = FindNamedComponent<Button>(root, "Terminal_SecondaryButton", "SecondaryButton", "Button_Secondary", "Button_B", "OptionB_Button");
        Button closeButton = FindNamedComponent<Button>(root, "Terminal_CloseButton", "CloseButton", "Button_Close");

        AssignOptionalObjectIfFound(terminalUI, "terminalPanel", panel, "TerminalUIController -> 正式终端面板", report);
        AssignOptionalObjectIfFound(terminalUI, "contentRoot", FindNamedTransform(root, "Terminal_ContentRoot", "ContentRoot", "Terminal_Content"), "TerminalUIController -> 正式内容根物体", report);
        AssignOptionalObjectIfFound(terminalUI, "closeButton", closeButton, "TerminalUIController -> 正式关闭按钮", report);

        AssignOptionalObjectIfFound(terminalPresenter, "boundUIRoot", root, "终端展示器 -> 正式 UI 根物体", report);
        AssignOptionalObjectIfFound(terminalPresenter, "boundTitleText", FindNamedComponent<TMP_Text>(root, "Terminal_TitleText", "Terminal_Title", "TitleText", "Title"), "终端展示器 -> 正式标题文本", report);
        AssignOptionalObjectIfFound(terminalPresenter, "boundBodyText", FindNamedComponent<TMP_Text>(root, "Terminal_BodyText", "Terminal_Body", "BodyText", "Body"), "终端展示器 -> 正式正文文本", report);
        AssignOptionalObjectIfFound(terminalPresenter, "boundPrimaryButton", primaryButton, "终端展示器 -> 正式主按钮", report);
        AssignOptionalObjectIfFound(terminalPresenter, "boundPrimaryButtonText", FindButtonLabel(primaryButton, root, "Terminal_PrimaryButtonText", "PrimaryButtonText"), "终端展示器 -> 正式主按钮文字", report);
        AssignOptionalObjectIfFound(terminalPresenter, "boundSecondaryButton", secondaryButton, "终端展示器 -> 正式第二按钮", report);
        AssignOptionalObjectIfFound(terminalPresenter, "boundSecondaryButtonText", FindButtonLabel(secondaryButton, root, "Terminal_SecondaryButtonText", "SecondaryButtonText"), "终端展示器 -> 正式第二按钮文字", report);
        AssignOptionalObjectIfFound(terminalPresenter, "boundCloseButton", closeButton, "终端展示器 -> 正式关闭按钮", report);
        AssignOptionalObjectIfFound(terminalPresenter, "boundCloseButtonText", FindButtonLabel(closeButton, root, "Terminal_CloseButtonText", "CloseButtonText"), "终端展示器 -> 正式关闭按钮文字", report);
    }

    private static void AssignObjectiveBoundUI(MinLoopObjectivePresenter objectivePresenter, AutoBindReport report)
    {
        GameObject root = FindSceneGameObject("Objective_Root", "ObjectiveRoot", "Objective_Panel", "MinLoop_Objective_UI");
        AssignOptionalObjectIfFound(objectivePresenter, "objectiveRoot", root, "目标提示 -> 正式 UI 根物体", report);
        AssignOptionalObjectIfFound(objectivePresenter, "titleText", FindNamedComponent<TMP_Text>(root, "Objective_TitleText", "Objective_Title", "ObjectiveTitle", "Objective_Title_Label"), "目标提示 -> 正式标题文本", report);
        AssignOptionalObjectIfFound(objectivePresenter, "bodyText", FindNamedComponent<TMP_Text>(root, "Objective_BodyText", "Objective_Body", "ObjectiveBody", "Objective_Body_Label"), "目标提示 -> 正式正文文本", report);
        AssignOptionalObjectIfFound(objectivePresenter, "canvasGroup", FindNamedComponent<CanvasGroup>(root, "Objective_Root", "ObjectiveRoot", "Objective_Panel", "MinLoop_Objective_UI"), "目标提示 -> 正式 CanvasGroup", report);
    }

    private static void AssignRobotHudBoundUI(MinLoopRobotHudPresenter robotHudPresenter, AutoBindReport report)
    {
        GameObject root = FindSceneGameObject("RobotHUD_Root", "Robot_HUD_Root", "RobotHUD_Panel", "CompanionHUD_Root", "Companion_HUD_Root");
        AssignOptionalObjectIfFound(robotHudPresenter, "hudRoot", root, "机器 HUD -> 正式 UI 根物体", report);
        AssignOptionalObjectIfFound(robotHudPresenter, "timeText", FindNamedComponent<TMP_Text>(root, "RobotHUD_TimeText", "RobotHUD_Time", "HUD_TimeText", "TimeText"), "机器 HUD -> 时间文本", report);
        AssignOptionalObjectIfFound(robotHudPresenter, "heartRateText", FindNamedComponent<TMP_Text>(root, "RobotHUD_HeartRateText", "RobotHUD_HeartRate", "HUD_HeartRateText", "HeartRateText"), "机器 HUD -> 心率文本", report);
        AssignOptionalObjectIfFound(robotHudPresenter, "statusText", FindNamedComponent<TMP_Text>(root, "RobotHUD_StatusText", "RobotHUD_Status", "HUD_StatusText", "StatusText"), "机器 HUD -> 状态文本", report);
        AssignOptionalObjectIfFound(robotHudPresenter, "instructionText", FindNamedComponent<TMP_Text>(root, "RobotHUD_InstructionText", "RobotHUD_Instruction", "HUD_InstructionText", "InstructionText"), "机器 HUD -> 指令文本", report);
        AssignOptionalObjectIfFound(robotHudPresenter, "accentImage", FindNamedComponent<Image>(root, "RobotHUD_Accent", "HUD_Accent", "AccentImage", "Accent_Image"), "机器 HUD -> 状态色条", report);
        AssignOptionalObjectIfFound(robotHudPresenter, "canvasGroup", FindNamedComponent<CanvasGroup>(root, "RobotHUD_Root", "Robot_HUD_Root", "RobotHUD_Panel", "CompanionHUD_Root", "Companion_HUD_Root"), "机器 HUD -> CanvasGroup", report);
    }

    private static void AssignTrustBoundUI(MinLoopTrustPresenter trustPresenter, AutoBindReport report)
    {
        GameObject root = FindSceneGameObject("Trust_Root", "TrustRoot", "Trust_Panel", "MinLoop_Trust_UI");
        AssignOptionalObjectIfFound(trustPresenter, "trustRoot", root, "信任度显示 -> 正式 UI 根物体", report);
        AssignOptionalObjectIfFound(trustPresenter, "valueText", FindNamedComponent<TMP_Text>(root, "Trust_ValueText", "Trust_Value", "TrustValueText", "ValueText"), "信任度显示 -> 数值文本", report);
        AssignOptionalObjectIfFound(trustPresenter, "deltaText", FindNamedComponent<TMP_Text>(root, "Trust_DeltaText", "Trust_Delta", "TrustDeltaText", "DeltaText"), "信任度显示 -> 变化量文本", report);
        AssignOptionalObjectIfFound(trustPresenter, "labelText", FindNamedComponent<TMP_Text>(root, "Trust_LabelText", "Trust_Label", "TrustLabelText", "LabelText"), "信任度显示 -> 标题文本", report);
        AssignOptionalObjectIfFound(trustPresenter, "trustSlider", FindNamedComponent<Slider>(root, "Trust_Slider", "TrustSlider", "Slider_Trust"), "信任度显示 -> Slider", report);
        AssignOptionalObjectIfFound(trustPresenter, "canvasGroup", FindNamedComponent<CanvasGroup>(root, "Trust_Root", "TrustRoot", "Trust_Panel", "MinLoop_Trust_UI"), "信任度显示 -> CanvasGroup", report);
    }

    private static void AssignDebugReferences(
        MinLoopDebugHotkeys debugHotkeys,
        MinLoopFlowController flowController,
        ReplaySequenceController replaySequenceController,
        AutoBindReport report)
    {
        AssignObject(debugHotkeys, "flowController", flowController, "调试热键 -> 总流程", report, false);
        AssignObject(debugHotkeys, "replaySequenceController", replaySequenceController, "调试热键 -> 复盘控制器", report, false);
    }

    private static void AssignStageActivatorReferences(MinLoopStageObjectActivator stageObjectActivator, MinLoopFlowController flowController, AutoBindReport report)
    {
        AssignObject(stageObjectActivator, "flowController", flowController, "阶段显隐器 -> 总流程", report, false);

        GameObject terminalGuide = FindSceneGameObject("Guide_Terminal_17F01");
        GameObject comfortGuide = FindSceneGameObject("Guide_ComfortAction");
        GameObject nextResidentGuide = FindSceneGameObject("Guide_NextResident_17F02");
        EnsureGuideMarker(terminalGuide, "前往 17F-01 终端", report);
        EnsureGuideMarker(comfortGuide, "床边安抚操作", report);
        EnsureGuideMarker(nextResidentGuide, "下一户 17F-02", report);

        AddOrUpdateStageRule(stageObjectActivator, "终端入口指引", terminalGuide, new MinLoopStage[] { MinLoopStage.Corridor }, report);
        AddOrUpdateStageRule(stageObjectActivator, "安抚操作指引", comfortGuide, new MinLoopStage[] { MinLoopStage.WaitingForComfort }, report);
        AddOrUpdateStageRule(stageObjectActivator, "下一户指引", nextResidentGuide, new MinLoopStage[] { MinLoopStage.Complete }, report);
    }

    private static void AssignStageAnchorReferences(
        MinLoopStageAnchorController stageAnchorController,
        MinLoopFlowController flowController,
        GameObject humanRoot,
        GameObject companionRoot,
        Transform miaCorridorAnchor,
        Transform companionReplayAnchor,
        Transform miaTerminalReturnAnchor,
        Transform nextResidentAnchor,
        AutoBindReport report)
    {
        AssignObject(stageAnchorController, "flowController", flowController, "阶段锚点 -> 总流程", report, false);

        if (stageAnchorController == null)
        {
            report.OptionalMissing("跳过阶段锚点规则：缺少 MinLoopStageAnchorController。可重新运行 Create 17F-01 Scene Skeleton 补齐。");
            return;
        }

        AddOrUpdateAnchorRule(
            stageAnchorController,
            "Mia 初始走廊站位",
            humanRoot,
            miaCorridorAnchor,
            new MinLoopStage[] { MinLoopStage.Corridor },
            report);

        AddOrUpdateAnchorRule(
            stageAnchorController,
            "陪伴单元复盘起点",
            companionRoot,
            companionReplayAnchor,
            new MinLoopStage[] { MinLoopStage.SwitchingToCompanion },
            report);

        AddOrUpdateAnchorRule(
            stageAnchorController,
            "Mia 回到终端站位",
            humanRoot,
            miaTerminalReturnAnchor,
            new MinLoopStage[] { MinLoopStage.ReturningToTerminal },
            report);

        if (nextResidentAnchor != null)
        {
            report.AlreadyBound("下一户 Anchor 已存在：" + nextResidentAnchor.name + "。默认不传送玩家，只供指引/后续扩展使用。");
        }
    }

    private static void AssignStageCueReferences(
        MinLoopStageCueController stageCueController,
        MinLoopFlowController flowController,
        InteractionFeedbackController comfortReadyFeedback,
        InteractionFeedbackController morningReviewFeedback,
        InteractionFeedbackController nextResidentGuideFeedback,
        AutoBindReport report)
    {
        AssignObject(stageCueController, "flowController", flowController, "阶段 Cue -> 总流程", report, false);

        if (stageCueController == null)
        {
            report.OptionalMissing("跳过阶段 Cue 规则：缺少 MinLoopStageCueController。可重新运行 Create 17F-01 Scene Skeleton 补齐。");
            return;
        }

        AddOrUpdateCueRule(
            stageCueController,
            "安抚点就绪反馈",
            comfortReadyFeedback,
            new MinLoopStage[] { MinLoopStage.WaitingForComfort },
            report);

        AddOrUpdateCueRule(
            stageCueController,
            "早晨回顾反馈",
            morningReviewFeedback,
            new MinLoopStage[] { MinLoopStage.MorningReview },
            report);

        AddOrUpdateCueRule(
            stageCueController,
            "下一户指引反馈",
            nextResidentGuideFeedback,
            new MinLoopStage[] { MinLoopStage.Complete },
            report);
    }

    private static void AssignLightingReferences(
        MinLoopLightingStateController lightingStateController,
        MinLoopFlowController flowController,
        Light corridorLight,
        Light replayNightLight,
        Light morningLight,
        AutoBindReport report)
    {
        AssignObject(lightingStateController, "flowController", flowController, "阶段灯光 -> 总流程", report, false);

        if (lightingStateController == null)
        {
            report.OptionalMissing("跳过阶段灯光规则：缺少 MinLoopLightingStateController。可重新运行 Create 17F-01 Scene Skeleton 补齐。");
            return;
        }

        AddOrUpdateLightingRule(
            lightingStateController,
            "走廊/终端暖光",
            corridorLight,
            new MinLoopStage[]
            {
                MinLoopStage.Corridor,
                MinLoopStage.AccessCard,
                MinLoopStage.ResidentSummary,
                MinLoopStage.ReturningToTerminal,
                MinLoopStage.DispositionChoice,
                MinLoopStage.Complete
            },
            new Color(1f, 0.74f, 0.46f, 1f),
            1.35f,
            8f,
            new Color(0.18f, 0.14f, 0.1f, 1f),
            false,
            Color.black,
            0f,
            report);

        AddOrUpdateLightingRule(
            lightingStateController,
            "昨夜复盘冷光",
            replayNightLight,
            new MinLoopStage[]
            {
                MinLoopStage.SwitchingToCompanion,
                MinLoopStage.CompanionReplay,
                MinLoopStage.WaitingForComfort,
                MinLoopStage.Comforting
            },
            new Color(0.32f, 0.54f, 1f, 1f),
            0.9f,
            7.5f,
            new Color(0.04f, 0.06f, 0.1f, 1f),
            true,
            new Color(0.02f, 0.03f, 0.06f, 1f),
            0.012f,
            report);

        AddOrUpdateLightingRule(
            lightingStateController,
            "早晨回顾柔光",
            morningLight,
            new MinLoopStage[] { MinLoopStage.MorningReview },
            new Color(1f, 0.9f, 0.7f, 1f),
            1.15f,
            9f,
            new Color(0.24f, 0.22f, 0.18f, 1f),
            false,
            Color.black,
            0f,
            report);
    }

    private static void AssignAudioReferences(
        MinLoopAudioStateController audioStateController,
        MinLoopFlowController flowController,
        AudioSource corridorAmbience,
        AudioSource replayNightAmbience,
        AudioSource morningAmbience,
        AutoBindReport report)
    {
        AssignObject(audioStateController, "flowController", flowController, "阶段音频 -> 总流程", report, false);

        if (audioStateController == null)
        {
            report.OptionalMissing("跳过阶段音频规则：缺少 MinLoopAudioStateController。可重新运行 Create 17F-01 Scene Skeleton 补齐。");
            return;
        }

        AddOrUpdateAudioRule(
            audioStateController,
            "走廊/终端环境声",
            corridorAmbience,
            new MinLoopStage[]
            {
                MinLoopStage.Corridor,
                MinLoopStage.AccessCard,
                MinLoopStage.ResidentSummary,
                MinLoopStage.ReturningToTerminal,
                MinLoopStage.DispositionChoice,
                MinLoopStage.Complete
            },
            0.18f,
            0.55f,
            0.35f,
            report);

        AddOrUpdateAudioRule(
            audioStateController,
            "昨夜复盘环境声",
            replayNightAmbience,
            new MinLoopStage[]
            {
                MinLoopStage.SwitchingToCompanion,
                MinLoopStage.CompanionReplay,
                MinLoopStage.WaitingForComfort,
                MinLoopStage.Comforting
            },
            0.16f,
            0.6f,
            0.45f,
            report);

        AddOrUpdateAudioRule(
            audioStateController,
            "早晨回顾环境声",
            morningAmbience,
            new MinLoopStage[] { MinLoopStage.MorningReview },
            0.12f,
            0.5f,
            0.4f,
            report);
    }

    private static void AssignValidatorReferences(
        MinLoopSceneValidator validator,
        MinLoopFlowController flowController,
        ResidentTerminalFlow terminalFlow,
        MinLoopTerminalPresenter terminalPresenter,
        ViewSwitchController viewSwitchController,
        MinLoopStageObjectActivator stageObjectActivator,
        MinLoopStageAnchorController stageAnchorController,
        MinLoopStageCueController stageCueController,
        MinLoopLightingStateController lightingStateController,
        MinLoopAudioStateController audioStateController,
        ReplaySequenceController replaySequenceController,
        ComfortActionInteractable comfortAction,
        MinLoopSubtitlePlayer subtitlePlayer,
        MinLoopRobotHudPresenter robotHudPresenter,
        MinLoopObjectivePresenter objectivePresenter,
        MinLoopTrustPresenter trustPresenter,
        TrustStateController trustStateController,
        MinLoopDebugHotkeys debugHotkeys,
        AutoBindReport report)
    {
        if (validator == null)
        {
            report.OptionalMissing("缺少 MinLoopSceneValidator，无法自动刷新检查器引用。");
            return;
        }

        AssignObject(validator, "flowController", flowController, "检查器 -> 总流程", report);
        AssignObject(validator, "terminalFlow", terminalFlow, "检查器 -> 终端入口", report);
        AssignObject(validator, "terminalPresenter", terminalPresenter, "检查器 -> 终端展示器", report);
        AssignObject(validator, "viewSwitchController", viewSwitchController, "检查器 -> 视角切换器", report);
        AssignObjectArray(validator, "stageObjectActivators", new MinLoopStageObjectActivator[] { stageObjectActivator }, "检查器 -> 阶段显隐器", report, false);
        AssignObject(validator, "stageAnchorController", stageAnchorController, "检查器 -> 阶段锚点控制器", report, false);
        AssignObject(validator, "stageCueController", stageCueController, "检查器 -> 阶段 Cue 控制器", report, false);
        AssignObject(validator, "lightingStateController", lightingStateController, "检查器 -> 阶段灯光控制器", report, false);
        AssignObject(validator, "audioStateController", audioStateController, "检查器 -> 阶段音频控制器", report, false);
        AssignObject(validator, "replaySequenceController", replaySequenceController, "检查器 -> 复盘控制器", report);
        AssignObject(validator, "comfortAction", comfortAction, "检查器 -> 唯一安抚点", report, false);
        AssignObject(validator, "subtitlePlayer", subtitlePlayer, "检查器 -> 字幕播放器", report, false);
        AssignObject(validator, "robotHudPresenter", robotHudPresenter, "检查器 -> 机器 HUD", report, false);
        AssignObject(validator, "objectivePresenter", objectivePresenter, "检查器 -> 目标提示", report, false);
        AssignObject(validator, "trustPresenter", trustPresenter, "检查器 -> 信任度显示", report, false);
        AssignObject(validator, "trustStateController", trustStateController, "检查器 -> 信任度控制器", report);
        AssignObject(validator, "debugHotkeys", debugHotkeys, "检查器 -> 调试热键", report, false);

        InteractionFeedbackController[] feedbackObjects = FindSceneComponents<InteractionFeedbackController>();
        SmartDoorController[] doors = FindSceneComponents<SmartDoorController>();
        SmartDoorTriggerZone[] doorTriggerZones = FindSceneComponents<SmartDoorTriggerZone>();
        MinLoopWorldGuideMarker[] guideMarkers = FindSceneComponents<MinLoopWorldGuideMarker>();
        AssignDoorTriggerReferences(doorTriggerZones, doors, report);
        AssignObjectArray(validator, "feedbackObjects", feedbackObjects, "检查器 -> 反馈对象列表", report, false);
        AssignObjectArray(validator, "doors", doors, "检查器 -> 门对象列表", report, false);
        AssignObjectArray(validator, "doorTriggerZones", doorTriggerZones, "检查器 -> 自动门触发区列表", report, false);
        AssignObjectArray(validator, "guideMarkers", guideMarkers, "检查器 -> 世界指引标记列表", report, false);
    }

    private static void AssignDoorTriggerReferences(SmartDoorTriggerZone[] triggerZones, SmartDoorController[] doors, AutoBindReport report)
    {
        if (triggerZones == null)
        {
            return;
        }

        for (int i = 0; i < triggerZones.Length; i++)
        {
            SmartDoorTriggerZone triggerZone = triggerZones[i];
            if (triggerZone == null)
            {
                continue;
            }

            SmartDoorController door = triggerZone.TargetDoor;
            if (door == null)
            {
                door = triggerZone.GetComponentInParent<SmartDoorController>();
            }

            if (door == null && doors != null && doors.Length == 1)
            {
                door = doors[0];
            }

            AssignObject(triggerZone, "targetDoor", door, "自动门触发区 " + triggerZone.name + " -> 目标门", report, false);
        }
    }

    private static void AssignViewRig(ViewSwitchController viewSwitchController, string rigPropertyName, GameObject rootObject, string label, AutoBindReport report)
    {
        if (rootObject == null)
        {
            report.Missing("找不到 " + label + " 根物体。请按约定命名为 Player_Mia_Controller 或 Companion_Controller。");
            return;
        }

        SerializedObject serializedObject = new SerializedObject(viewSwitchController);
        SerializedProperty rigProperty = serializedObject.FindProperty(rigPropertyName);
        if (rigProperty == null)
        {
            report.Missing("ViewSwitchController 缺少字段 " + rigPropertyName + "。");
            return;
        }

        Undo.RecordObject(viewSwitchController, "Auto Bind View Switch Rig");
        bool changed = false;
        changed |= AssignRelativeObject(rigProperty, "rootObject", rootObject, label + " 根物体", report);
        changed |= AssignRelativeObject(rigProperty, "viewCamera", FindComponentInChildren<Camera>(rootObject), label + " Camera", report);
        changed |= AssignRelativeObject(rigProperty, "movement", FindComponentInChildren<FirstPersonMovement>(rootObject), label + " FirstPersonMovement", report);
        changed |= AssignRelativeObject(rigProperty, "look", FindComponentInChildren<FirstPersonLook>(rootObject), label + " FirstPersonLook", report);
        changed |= AssignRelativeObject(rigProperty, "interaction", FindComponentInChildren<PlayerInteraction>(rootObject), label + " PlayerInteraction", report, false);
        changed |= AssignRelativeObject(rigProperty, "rigidbody", FindComponentInChildren<Rigidbody>(rootObject), label + " Rigidbody", report, false);
        changed |= AssignRelativeObject(rigProperty, "canvas", FindComponentInChildren<Canvas>(rootObject), label + " Canvas", report, false);

        if (changed)
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(viewSwitchController);
        }
    }

    private static void AssignOptionalObjectIfFound(UnityEngine.Object target, string propertyName, UnityEngine.Object value, string label, AutoBindReport report)
    {
        if (target == null || value == null)
        {
            return;
        }

        AssignObject(target, propertyName, value, label, report, false);
    }

    private static bool AssignObject(UnityEngine.Object target, string propertyName, UnityEngine.Object value, string label, AutoBindReport report, bool required = true)
    {
        if (target == null)
        {
            if (required)
            {
                report.Missing("无法绑定 " + label + "：目标脚本不存在。");
            }
            else
            {
                report.OptionalMissing("跳过 " + label + "：目标脚本不存在。");
            }

            return false;
        }

        if (value == null)
        {
            if (required)
            {
                report.Missing("无法绑定 " + label + "：引用对象未找到。");
            }
            else
            {
                report.OptionalMissing("跳过 " + label + "：引用对象未找到。");
            }

            return false;
        }

        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            report.Missing("无法绑定 " + label + "：字段 " + propertyName + " 不存在。");
            return false;
        }

        if (property.objectReferenceValue == value)
        {
            report.AlreadyBound(label);
            return false;
        }

        Undo.RecordObject(target, "Auto Bind Min Loop Reference");
        property.objectReferenceValue = value;
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
        report.Bound(label);
        return true;
    }

    private static bool AssignRelativeObject(SerializedProperty parentProperty, string propertyName, UnityEngine.Object value, string label, AutoBindReport report, bool required = true)
    {
        SerializedProperty property = parentProperty.FindPropertyRelative(propertyName);
        if (property == null)
        {
            report.Missing("无法绑定 " + label + "：字段 " + propertyName + " 不存在。");
            return false;
        }

        if (value == null)
        {
            if (required)
            {
                report.Missing("无法绑定 " + label + "：引用对象未找到。");
            }
            else
            {
                report.OptionalMissing("跳过 " + label + "：引用对象未找到。");
            }

            return false;
        }

        if (property.objectReferenceValue == value)
        {
            report.AlreadyBound(label);
            return false;
        }

        property.objectReferenceValue = value;
        report.Bound(label);
        return true;
    }

    private static void AssignObjectArray(UnityEngine.Object target, string propertyName, UnityEngine.Object[] values, string label, AutoBindReport report, bool required = true)
    {
        if (target == null)
        {
            if (required)
            {
                report.Missing("无法绑定 " + label + "：目标脚本不存在。");
            }
            else
            {
                report.OptionalMissing("跳过 " + label + "：目标脚本不存在。");
            }

            return;
        }

        UnityEngine.Object[] filteredValues = FilterNull(values);
        if (filteredValues.Length == 0)
        {
            if (required)
            {
                report.Missing("无法绑定 " + label + "：没有可用对象。");
            }
            else
            {
                report.OptionalMissing("跳过 " + label + "：没有可用对象。");
            }

            return;
        }

        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null || !property.isArray)
        {
            report.Missing("无法绑定 " + label + "：字段 " + propertyName + " 不存在或不是数组。");
            return;
        }

        if (ArrayMatches(property, filteredValues))
        {
            report.AlreadyBound(label);
            return;
        }

        Undo.RecordObject(target, "Auto Bind Min Loop Reference Array");
        property.arraySize = filteredValues.Length;
        for (int i = 0; i < filteredValues.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = filteredValues[i];
        }

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
        report.Bound(label + "（" + filteredValues.Length + " 个）");
    }

    private static void AddOrUpdateStageRule(MinLoopStageObjectActivator stageObjectActivator, string label, GameObject targetObject, MinLoopStage[] activeStages, AutoBindReport report)
    {
        if (stageObjectActivator == null)
        {
            report.OptionalMissing("跳过阶段规则 " + label + "：缺少 MinLoopStageObjectActivator。");
            return;
        }

        if (targetObject == null)
        {
            report.OptionalMissing("跳过阶段规则 " + label + "：目标物体未找到。");
            return;
        }

        SerializedObject serializedObject = new SerializedObject(stageObjectActivator);
        SerializedProperty rulesProperty = serializedObject.FindProperty("rules");
        if (rulesProperty == null || !rulesProperty.isArray)
        {
            report.Missing("无法绑定阶段规则 " + label + "：rules 字段不存在。");
            return;
        }

        int ruleIndex = FindStageRuleIndex(rulesProperty, label);
        if (ruleIndex < 0)
        {
            Undo.RecordObject(stageObjectActivator, "Add Min Loop Stage Rule");
            ruleIndex = rulesProperty.arraySize;
            rulesProperty.arraySize++;
        }
        else
        {
            Undo.RecordObject(stageObjectActivator, "Update Min Loop Stage Rule");
        }

        SerializedProperty ruleProperty = rulesProperty.GetArrayElementAtIndex(ruleIndex);
        SetString(ruleProperty.FindPropertyRelative("label"), label);
        SetObject(ruleProperty.FindPropertyRelative("targetObject"), targetObject);
        SetBool(ruleProperty.FindPropertyRelative("invertMatch"), false);
        SetStageArray(ruleProperty.FindPropertyRelative("activeStages"), activeStages);

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(stageObjectActivator);
        report.Bound("阶段显隐规则 -> " + label);
    }

    private static void AddOrUpdateAnchorRule(
        MinLoopStageAnchorController stageAnchorController,
        string label,
        GameObject targetRoot,
        Transform anchor,
        MinLoopStage[] activeStages,
        AutoBindReport report)
    {
        if (stageAnchorController == null)
        {
            report.OptionalMissing("跳过锚点规则 " + label + "：缺少 MinLoopStageAnchorController。");
            return;
        }

        if (targetRoot == null)
        {
            report.OptionalMissing("跳过锚点规则 " + label + "：目标角色根物体未找到。");
            return;
        }

        if (anchor == null)
        {
            report.OptionalMissing("跳过锚点规则 " + label + "：Anchor 未找到。可重新运行 Create 17F-01 Scene Skeleton。");
            return;
        }

        SerializedObject serializedObject = new SerializedObject(stageAnchorController);
        SerializedProperty rulesProperty = serializedObject.FindProperty("rules");
        if (rulesProperty == null || !rulesProperty.isArray)
        {
            report.Missing("无法绑定锚点规则 " + label + "：rules 字段不存在。");
            return;
        }

        int ruleIndex = FindStageRuleIndex(rulesProperty, label);
        if (ruleIndex < 0)
        {
            Undo.RecordObject(stageAnchorController, "Add Min Loop Anchor Rule");
            ruleIndex = rulesProperty.arraySize;
            rulesProperty.arraySize++;
        }
        else
        {
            Undo.RecordObject(stageAnchorController, "Update Min Loop Anchor Rule");
        }

        SerializedProperty ruleProperty = rulesProperty.GetArrayElementAtIndex(ruleIndex);
        Camera camera = FindComponentInChildren<Camera>(targetRoot);

        SetString(ruleProperty.FindPropertyRelative("label"), label);
        SetStageArray(ruleProperty.FindPropertyRelative("activeStages"), activeStages);
        SetObject(ruleProperty.FindPropertyRelative("targetRoot"), targetRoot.transform);
        SetObject(ruleProperty.FindPropertyRelative("anchor"), anchor);
        SetObject(ruleProperty.FindPropertyRelative("targetRigidbody"), FindComponentInChildren<Rigidbody>(targetRoot));
        SetObject(ruleProperty.FindPropertyRelative("targetCharacterController"), FindComponentInChildren<CharacterController>(targetRoot));
        SetObject(ruleProperty.FindPropertyRelative("firstPersonLook"), FindComponentInChildren<FirstPersonLook>(targetRoot));
        SetObject(ruleProperty.FindPropertyRelative("lookTransform"), camera != null ? camera.transform : null);
        SetBool(ruleProperty.FindPropertyRelative("applyPosition"), true);
        SetBool(ruleProperty.FindPropertyRelative("applyRotation"), true);
        SetBool(ruleProperty.FindPropertyRelative("yawOnly"), true);
        SetBool(ruleProperty.FindPropertyRelative("resetLookLocalRotation"), true);
        SetBool(ruleProperty.FindPropertyRelative("syncFirstPersonLook"), true);
        SetBool(ruleProperty.FindPropertyRelative("clearRigidbodyVelocity"), true);

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(stageAnchorController);
        report.Bound("阶段锚点规则 -> " + label);
    }

    private static void AddOrUpdateCueRule(
        MinLoopStageCueController stageCueController,
        string label,
        InteractionFeedbackController feedback,
        MinLoopStage[] activeStages,
        AutoBindReport report)
    {
        if (stageCueController == null)
        {
            report.OptionalMissing("跳过阶段 Cue 规则 " + label + "：缺少 MinLoopStageCueController。");
            return;
        }

        if (feedback == null)
        {
            report.OptionalMissing("跳过阶段 Cue 规则 " + label + "：默认反馈对象未找到。可重新运行 Create 17F-01 Scene Skeleton。");
            return;
        }

        SerializedObject serializedObject = new SerializedObject(stageCueController);
        SerializedProperty rulesProperty = serializedObject.FindProperty("rules");
        if (rulesProperty == null || !rulesProperty.isArray)
        {
            report.Missing("无法绑定阶段 Cue 规则 " + label + "：rules 字段不存在。");
            return;
        }

        int ruleIndex = FindStageRuleIndex(rulesProperty, label);
        if (ruleIndex < 0)
        {
            Undo.RecordObject(stageCueController, "Add Min Loop Cue Rule");
            ruleIndex = rulesProperty.arraySize;
            rulesProperty.arraySize++;
        }
        else
        {
            Undo.RecordObject(stageCueController, "Update Min Loop Cue Rule");
        }

        SerializedProperty ruleProperty = rulesProperty.GetArrayElementAtIndex(ruleIndex);
        SetString(ruleProperty.FindPropertyRelative("label"), label);
        SetStageArray(ruleProperty.FindPropertyRelative("activeStages"), activeStages);
        SetBool(ruleProperty.FindPropertyRelative("triggerOnce"), true);
        SetFloat(ruleProperty.FindPropertyRelative("delaySeconds"), 0f);
        SetObjectArray(ruleProperty.FindPropertyRelative("feedbackObjects"), new UnityEngine.Object[] { feedback });

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(stageCueController);
        report.Bound("阶段 Cue 规则 -> " + label);
    }

    private static void AddOrUpdateLightingRule(
        MinLoopLightingStateController lightingStateController,
        string label,
        Light targetLight,
        MinLoopStage[] activeStages,
        Color lightColor,
        float lightIntensity,
        float lightRange,
        Color ambientColor,
        bool applyFog,
        Color fogColor,
        float fogDensity,
        AutoBindReport report)
    {
        if (lightingStateController == null)
        {
            report.OptionalMissing("跳过灯光规则 " + label + "：缺少 MinLoopLightingStateController。");
            return;
        }

        if (targetLight == null)
        {
            report.OptionalMissing("跳过灯光规则 " + label + "：默认灯光对象未找到。可重新运行 Create 17F-01 Scene Skeleton。");
            return;
        }

        SerializedObject serializedObject = new SerializedObject(lightingStateController);
        SerializedProperty rulesProperty = serializedObject.FindProperty("rules");
        if (rulesProperty == null || !rulesProperty.isArray)
        {
            report.Missing("无法绑定灯光规则 " + label + "：rules 字段不存在。");
            return;
        }

        int ruleIndex = FindStageRuleIndex(rulesProperty, label);
        if (ruleIndex < 0)
        {
            Undo.RecordObject(lightingStateController, "Add Min Loop Lighting Rule");
            ruleIndex = rulesProperty.arraySize;
            rulesProperty.arraySize++;
        }
        else
        {
            Undo.RecordObject(lightingStateController, "Update Min Loop Lighting Rule");
        }

        SerializedProperty ruleProperty = rulesProperty.GetArrayElementAtIndex(ruleIndex);
        SetString(ruleProperty.FindPropertyRelative("label"), label);
        SetStageArray(ruleProperty.FindPropertyRelative("activeStages"), activeStages);
        SetObjectArray(ruleProperty.FindPropertyRelative("lights"), new UnityEngine.Object[] { targetLight });
        SetBool(ruleProperty.FindPropertyRelative("setLightEnabled"), true);
        SetBool(ruleProperty.FindPropertyRelative("lightEnabled"), true);
        SetBool(ruleProperty.FindPropertyRelative("setLightColor"), true);
        SetColor(ruleProperty.FindPropertyRelative("lightColor"), lightColor);
        SetBool(ruleProperty.FindPropertyRelative("setLightIntensity"), true);
        SetFloat(ruleProperty.FindPropertyRelative("lightIntensity"), lightIntensity);
        SetBool(ruleProperty.FindPropertyRelative("setLightRange"), true);
        SetFloat(ruleProperty.FindPropertyRelative("lightRange"), lightRange);
        SetBool(ruleProperty.FindPropertyRelative("applyAmbientColor"), true);
        SetColor(ruleProperty.FindPropertyRelative("ambientColor"), ambientColor);
        SetBool(ruleProperty.FindPropertyRelative("applyFog"), true);
        SetBool(ruleProperty.FindPropertyRelative("fogEnabled"), applyFog);
        SetColor(ruleProperty.FindPropertyRelative("fogColor"), fogColor);
        SetFloat(ruleProperty.FindPropertyRelative("fogDensity"), fogDensity);
        SetFloat(ruleProperty.FindPropertyRelative("transitionSeconds"), 0.45f);

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(lightingStateController);
        report.Bound("阶段灯光规则 -> " + label);
    }

    private static void AddOrUpdateAudioRule(
        MinLoopAudioStateController audioStateController,
        string label,
        AudioSource targetSource,
        MinLoopStage[] activeStages,
        float targetVolume,
        float fadeInSeconds,
        float fadeOutSeconds,
        AutoBindReport report)
    {
        if (audioStateController == null)
        {
            report.OptionalMissing("跳过音频规则 " + label + "：缺少 MinLoopAudioStateController。");
            return;
        }

        if (targetSource == null)
        {
            report.OptionalMissing("跳过音频规则 " + label + "：默认 AudioSource 未找到。可重新运行 Create 17F-01 Scene Skeleton。");
            return;
        }

        SerializedObject serializedObject = new SerializedObject(audioStateController);
        SerializedProperty rulesProperty = serializedObject.FindProperty("rules");
        if (rulesProperty == null || !rulesProperty.isArray)
        {
            report.Missing("无法绑定音频规则 " + label + "：rules 字段不存在。");
            return;
        }

        int ruleIndex = FindStageRuleIndex(rulesProperty, label);
        if (ruleIndex < 0)
        {
            Undo.RecordObject(audioStateController, "Add Min Loop Audio Rule");
            ruleIndex = rulesProperty.arraySize;
            rulesProperty.arraySize++;
        }
        else
        {
            Undo.RecordObject(audioStateController, "Update Min Loop Audio Rule");
        }

        SerializedProperty ruleProperty = rulesProperty.GetArrayElementAtIndex(ruleIndex);
        SetString(ruleProperty.FindPropertyRelative("label"), label);
        SetStageArray(ruleProperty.FindPropertyRelative("activeStages"), activeStages);
        SetObjectArray(ruleProperty.FindPropertyRelative("audioSources"), new UnityEngine.Object[] { targetSource });
        SetBool(ruleProperty.FindPropertyRelative("assignFallbackClipIfMissing"), true);
        SetBool(ruleProperty.FindPropertyRelative("loop"), true);
        SetFloat(ruleProperty.FindPropertyRelative("targetVolume"), targetVolume);
        SetBool(ruleProperty.FindPropertyRelative("setSpatialBlend"), true);
        SetFloat(ruleProperty.FindPropertyRelative("spatialBlend"), 0f);
        SetBool(ruleProperty.FindPropertyRelative("setPitch"), false);
        SetFloat(ruleProperty.FindPropertyRelative("pitch"), 1f);
        SetBool(ruleProperty.FindPropertyRelative("restartOnEnter"), false);
        SetBool(ruleProperty.FindPropertyRelative("stopWhenUnmatched"), true);
        SetFloat(ruleProperty.FindPropertyRelative("fadeInSeconds"), fadeInSeconds);
        SetFloat(ruleProperty.FindPropertyRelative("fadeOutSeconds"), fadeOutSeconds);

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(audioStateController);
        report.Bound("阶段音频规则 -> " + label);
    }

    private static void AssignInteractionCamera(PlayerInteraction interaction, Camera camera, string label, AutoBindReport report)
    {
        if (interaction == null || camera == null)
        {
            report.OptionalMissing("跳过 " + label + "：缺少 PlayerInteraction 或 Camera。");
            return;
        }

        if (interaction.mainCamera == camera)
        {
            report.AlreadyBound(label);
            return;
        }

        Undo.RecordObject(interaction, "Auto Bind Interaction Camera");
        interaction.SetInteractionCamera(camera);
        EditorUtility.SetDirty(interaction);
        report.Bound(label);
    }

    private static void EnsureCollider(GameObject target, Vector3 size, string label, AutoBindReport report)
    {
        if (target == null)
        {
            report.OptionalMissing("跳过 " + label + "：目标物体不存在。");
            return;
        }

        if (target.GetComponentInChildren<Collider>(true) != null)
        {
            report.AlreadyBound(label);
            return;
        }

        BoxCollider collider = Undo.AddComponent<BoxCollider>(target);
        collider.size = size;
        EditorUtility.SetDirty(collider);
        report.Added(label);
    }

    private static void EnsureGuideMarker(GameObject target, string label, AutoBindReport report)
    {
        if (target == null)
        {
            report.OptionalMissing("跳过世界指引标记：" + label + " 的目标物体不存在。");
            return;
        }

        MinLoopWorldGuideMarker marker = target.GetComponent<MinLoopWorldGuideMarker>();
        if (marker == null)
        {
            marker = Undo.AddComponent<MinLoopWorldGuideMarker>(target);
            report.Added("给 " + target.name + " 添加 MinLoopWorldGuideMarker。");
        }
        else
        {
            report.AlreadyBound(target.name + " 世界指引标记");
        }

        AssignString(marker, "markerLabel", label, target.name + " 世界指引文字", report, false);
    }

    private static SimpleActorCueController FindOrAddActorCue(string objectName, AutoBindReport report)
    {
        GameObject actorObject = FindSceneGameObject(objectName);
        if (actorObject == null)
        {
            report.OptionalMissing("未找到演员对象 " + objectName + "。");
            return null;
        }

        SimpleActorCueController existing = actorObject.GetComponentInChildren<SimpleActorCueController>(true);
        if (existing != null)
        {
            report.AlreadyBound(objectName + " 演员控制器");
            return existing;
        }

        SimpleActorCueController created = Undo.AddComponent<SimpleActorCueController>(actorObject);
        EditorUtility.SetDirty(created);
        report.Added("给 " + objectName + " 添加 SimpleActorCueController。");
        return created;
    }

    private static T FindSceneComponent<T>(params string[] preferredObjectNames) where T : Component
    {
        for (int i = 0; i < preferredObjectNames.Length; i++)
        {
            GameObject target = FindSceneGameObject(preferredObjectNames[i]);
            T component = FindComponentInChildren<T>(target);
            if (component != null)
            {
                return component;
            }
        }

        T[] components = FindSceneComponents<T>();
        return components.Length > 0 ? components[0] : null;
    }

    private static T FindNamedComponent<T>(GameObject root, params string[] names) where T : Component
    {
        for (int i = 0; i < names.Length; i++)
        {
            GameObject target = root != null ? FindChildGameObject(root.transform, names[i]) : FindSceneGameObject(names[i]);
            T component = FindComponentInChildren<T>(target);
            if (component != null)
            {
                return component;
            }
        }

        return null;
    }

    private static Transform FindNamedTransform(GameObject root, params string[] names)
    {
        for (int i = 0; i < names.Length; i++)
        {
            GameObject target = root != null ? FindChildGameObject(root.transform, names[i]) : FindSceneGameObject(names[i]);
            if (target != null)
            {
                return target.transform;
            }
        }

        return null;
    }

    private static TMP_Text FindButtonLabel(Button button, GameObject fallbackRoot, params string[] names)
    {
        if (button != null)
        {
            TMP_Text buttonLabel = FindNamedComponent<TMP_Text>(button.gameObject, "Label", "Text", "ButtonText");
            if (buttonLabel != null)
            {
                return buttonLabel;
            }

            buttonLabel = button.GetComponentInChildren<TMP_Text>(true);
            if (buttonLabel != null)
            {
                return buttonLabel;
            }
        }

        return FindNamedComponent<TMP_Text>(fallbackRoot, names);
    }

    private static T[] FindSceneComponents<T>() where T : Component
    {
        T[] components = Resources.FindObjectsOfTypeAll<T>();
        List<T> sceneComponents = new List<T>();

        for (int i = 0; i < components.Length; i++)
        {
            T component = components[i];
            if (component == null || EditorUtility.IsPersistent(component) || !component.gameObject.scene.IsValid())
            {
                continue;
            }

            sceneComponents.Add(component);
        }

        return sceneComponents.ToArray();
    }

    private static GameObject FindSceneGameObject(params string[] names)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        for (int nameIndex = 0; nameIndex < names.Length; nameIndex++)
        {
            string expectedName = names[nameIndex];
            if (string.IsNullOrEmpty(expectedName))
            {
                continue;
            }

            for (int objectIndex = 0; objectIndex < allObjects.Length; objectIndex++)
            {
                GameObject candidate = allObjects[objectIndex];
                if (!IsSceneObject(candidate))
                {
                    continue;
                }

                if (string.Equals(candidate.name, expectedName, System.StringComparison.OrdinalIgnoreCase))
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private static GameObject FindChildGameObject(Transform root, string expectedName)
    {
        if (root == null || string.IsNullOrEmpty(expectedName))
        {
            return null;
        }

        if (string.Equals(root.name, expectedName, System.StringComparison.OrdinalIgnoreCase))
        {
            return root.gameObject;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            GameObject found = FindChildGameObject(root.GetChild(i), expectedName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static Transform FindSceneTransform(params string[] names)
    {
        GameObject target = FindSceneGameObject(names);
        return target != null ? target.transform : null;
    }

    private static bool IsSceneObject(GameObject candidate)
    {
        return candidate != null &&
               !EditorUtility.IsPersistent(candidate) &&
               candidate.scene.IsValid() &&
               candidate.hideFlags == HideFlags.None;
    }

    private static T FindComponentInChildren<T>(GameObject root) where T : Component
    {
        if (root == null)
        {
            return null;
        }

        return root.GetComponentInChildren<T>(true);
    }

    private static void AddIfNotNull<T>(List<T> list, T item) where T : UnityEngine.Object
    {
        if (item != null && !list.Contains(item))
        {
            list.Add(item);
        }
    }

    private static UnityEngine.Object[] FilterNull(UnityEngine.Object[] values)
    {
        if (values == null)
        {
            return new UnityEngine.Object[0];
        }

        List<UnityEngine.Object> filtered = new List<UnityEngine.Object>();
        for (int i = 0; i < values.Length; i++)
        {
            if (values[i] != null && !filtered.Contains(values[i]))
            {
                filtered.Add(values[i]);
            }
        }

        return filtered.ToArray();
    }

    private static bool ArrayMatches(SerializedProperty property, UnityEngine.Object[] values)
    {
        if (property.arraySize != values.Length)
        {
            return false;
        }

        for (int i = 0; i < values.Length; i++)
        {
            if (property.GetArrayElementAtIndex(i).objectReferenceValue != values[i])
            {
                return false;
            }
        }

        return true;
    }

    private static int FindStageRuleIndex(SerializedProperty rulesProperty, string label)
    {
        for (int i = 0; i < rulesProperty.arraySize; i++)
        {
            SerializedProperty ruleProperty = rulesProperty.GetArrayElementAtIndex(i);
            SerializedProperty labelProperty = ruleProperty.FindPropertyRelative("label");
            if (labelProperty != null && labelProperty.stringValue == label)
            {
                return i;
            }
        }

        return -1;
    }

    private static void SetString(SerializedProperty property, string value)
    {
        if (property != null)
        {
            property.stringValue = value;
        }
    }

    private static void AssignString(UnityEngine.Object target, string propertyName, string value, string label, AutoBindReport report, bool required = true)
    {
        if (target == null)
        {
            if (required)
            {
                report.Missing("无法写入 " + label + "：目标脚本不存在。");
            }
            else
            {
                report.OptionalMissing("跳过 " + label + "：目标脚本不存在。");
            }

            return;
        }

        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            report.Missing("无法写入 " + label + "：字段 " + propertyName + " 不存在。");
            return;
        }

        if (property.stringValue == value)
        {
            report.AlreadyBound(label);
            return;
        }

        Undo.RecordObject(target, "Auto Bind Min Loop String");
        property.stringValue = value;
        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
        report.Bound(label);
    }

    private static void SetObject(SerializedProperty property, UnityEngine.Object value)
    {
        if (property != null)
        {
            property.objectReferenceValue = value;
        }
    }

    private static void SetBool(SerializedProperty property, bool value)
    {
        if (property != null)
        {
            property.boolValue = value;
        }
    }

    private static void SetFloat(SerializedProperty property, float value)
    {
        if (property != null)
        {
            property.floatValue = value;
        }
    }

    private static void SetColor(SerializedProperty property, Color value)
    {
        if (property != null)
        {
            property.colorValue = value;
        }
    }

    private static void SetStageArray(SerializedProperty property, MinLoopStage[] stages)
    {
        if (property == null || !property.isArray || stages == null)
        {
            return;
        }

        property.arraySize = stages.Length;
        for (int i = 0; i < stages.Length; i++)
        {
            property.GetArrayElementAtIndex(i).enumValueIndex = (int)stages[i];
        }
    }

    private static void SetObjectArray(SerializedProperty property, UnityEngine.Object[] values)
    {
        if (property == null || !property.isArray || values == null)
        {
            return;
        }

        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }

    private static void MarkActiveSceneDirty()
    {
        Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (activeScene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(activeScene);
        }
    }

    private class AutoBindReport
    {
        private readonly StringBuilder builder = new StringBuilder(4096);
        private int changedCount;
        private int alreadyCount;
        private int missingCount;
        private int optionalMissingCount;

        public bool HasChanges
        {
            get { return changedCount > 0; }
        }

        public AutoBindReport()
        {
            builder.AppendLine("[MinLoopSceneAutoBinder] 17F-01 场景自动绑定报告");
        }

        public void Bound(string message)
        {
            changedCount++;
            builder.AppendLine("[绑定] " + message);
        }

        public void Added(string message)
        {
            changedCount++;
            builder.AppendLine("[添加] " + message);
        }

        public void AlreadyBound(string message)
        {
            alreadyCount++;
            builder.AppendLine("[已存在] " + message);
        }

        public void Missing(string message)
        {
            missingCount++;
            builder.AppendLine("[需要处理] " + message);
        }

        public void OptionalMissing(string message)
        {
            optionalMissingCount++;
            builder.AppendLine("[可稍后] " + message);
        }

        public string BuildMessage()
        {
            builder.AppendLine("结果：新增/更新 " + changedCount + "，已正确 " + alreadyCount + "，必须处理 " + missingCount + "，可稍后 " + optionalMissingCount + "。");
            return builder.ToString();
        }
    }
}
