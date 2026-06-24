using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

public class MinLoopSceneValidator : MonoBehaviour
{
    [Header("Validation")]
    [SerializeField] private bool validateOnStart = true;
    [SerializeField] private bool includeInactiveObjects = true;
    [SerializeField] private bool logSuccessDetails = true;

    [Header("Expected Scene Objects")]
    [SerializeField] private MinLoopFlowController flowController;
    [SerializeField] private ResidentTerminalFlow terminalFlow;
    [SerializeField] private MinLoopTerminalPresenter terminalPresenter;
    [SerializeField] private ViewSwitchController viewSwitchController;
    [SerializeField] private MinLoopStageObjectActivator[] stageObjectActivators;
    [SerializeField] private ReplaySequenceController replaySequenceController;
    [SerializeField] private ComfortActionInteractable comfortAction;
    [SerializeField] private MinLoopSubtitlePlayer subtitlePlayer;
    [SerializeField] private MinLoopRobotHudPresenter robotHudPresenter;
    [SerializeField] private MinLoopObjectivePresenter objectivePresenter;
    [SerializeField] private MinLoopTrustPresenter trustPresenter;
    [SerializeField] private TrustStateController trustStateController;
    [SerializeField] private MinLoopDebugHotkeys debugHotkeys;

    [Header("Optional Scene Props")]
    [SerializeField] private MinLoopStageAnchorController stageAnchorController;
    [SerializeField] private MinLoopStageCueController stageCueController;
    [SerializeField] private MinLoopLightingStateController lightingStateController;
    [SerializeField] private MinLoopAudioStateController audioStateController;
    [SerializeField] private MinLoopWorldGuideMarker[] guideMarkers;
    [SerializeField] private SmartDoorController[] doors;
    [SerializeField] private SmartDoorTriggerZone[] doorTriggerZones;
    [SerializeField] private InteractionFeedbackController[] feedbackObjects;

    private int errorCount;
    private int warningCount;
    private int successCount;
    private StringBuilder reportBuilder;

    public int LastErrorCount
    {
        get { return errorCount; }
    }

    public int LastWarningCount
    {
        get { return warningCount; }
    }

    private void Awake()
    {
        ResolveReferences();
    }

    private void Start()
    {
        if (validateOnStart)
        {
            ValidateSceneSetup();
        }
    }

    [ContextMenu("Validate Min Loop Scene Setup")]
    public void ValidateSceneSetup()
    {
        ResolveReferences();

        errorCount = 0;
        warningCount = 0;
        successCount = 0;
        reportBuilder = new StringBuilder(2048);
        reportBuilder.AppendLine("[MinLoopSceneValidator] 17F-01 最小循环场景检查");

        ValidateCoreFlow();
        ValidatePlayerAndViewSwitching();
        ValidateTerminal();
        ValidateReplay();
        ValidateOptionalProps();

        reportBuilder.AppendLine("结果：错误 " + errorCount + "，警告 " + warningCount + "，通过 " + successCount + "。");

        if (errorCount > 0)
        {
            Debug.LogError(reportBuilder.ToString(), this);
        }
        else if (warningCount > 0)
        {
            Debug.LogWarning(reportBuilder.ToString(), this);
        }
        else
        {
            Debug.Log(reportBuilder.ToString(), this);
        }
    }

    public void ResolveReferences()
    {
        if (flowController == null)
        {
            flowController = FindSceneObject<MinLoopFlowController>();
        }

        if (terminalFlow == null)
        {
            terminalFlow = FindSceneObject<ResidentTerminalFlow>();
        }

        if (terminalPresenter == null)
        {
            terminalPresenter = FindSceneObject<MinLoopTerminalPresenter>();
        }

        if (viewSwitchController == null)
        {
            viewSwitchController = FindSceneObject<ViewSwitchController>();
        }

        if (stageObjectActivators == null || stageObjectActivators.Length == 0)
        {
            stageObjectActivators = FindSceneObjects<MinLoopStageObjectActivator>();
        }

        if (replaySequenceController == null)
        {
            replaySequenceController = FindSceneObject<ReplaySequenceController>();
        }

        if (comfortAction == null)
        {
            comfortAction = FindSceneObject<ComfortActionInteractable>();
        }

        if (subtitlePlayer == null)
        {
            subtitlePlayer = FindSceneObject<MinLoopSubtitlePlayer>();
        }

        if (robotHudPresenter == null)
        {
            robotHudPresenter = FindSceneObject<MinLoopRobotHudPresenter>();
        }

        if (objectivePresenter == null)
        {
            objectivePresenter = FindSceneObject<MinLoopObjectivePresenter>();
        }

        if (trustPresenter == null)
        {
            trustPresenter = FindSceneObject<MinLoopTrustPresenter>();
        }

        if (trustStateController == null)
        {
            trustStateController = FindSceneObject<TrustStateController>();
        }

        if (debugHotkeys == null)
        {
            debugHotkeys = FindSceneObject<MinLoopDebugHotkeys>();
        }

        if (stageAnchorController == null)
        {
            stageAnchorController = FindSceneObject<MinLoopStageAnchorController>();
        }

        if (stageCueController == null)
        {
            stageCueController = FindSceneObject<MinLoopStageCueController>();
        }

        if (lightingStateController == null)
        {
            lightingStateController = FindSceneObject<MinLoopLightingStateController>();
        }

        if (audioStateController == null)
        {
            audioStateController = FindSceneObject<MinLoopAudioStateController>();
        }

        if (doors == null || doors.Length == 0)
        {
            doors = FindSceneObjects<SmartDoorController>();
        }

        if (doorTriggerZones == null || doorTriggerZones.Length == 0)
        {
            doorTriggerZones = FindSceneObjects<SmartDoorTriggerZone>();
        }

        if (guideMarkers == null || guideMarkers.Length == 0)
        {
            guideMarkers = FindSceneObjects<MinLoopWorldGuideMarker>();
        }

        if (feedbackObjects == null || feedbackObjects.Length == 0)
        {
            feedbackObjects = FindSceneObjects<InteractionFeedbackController>();
        }
    }

    private void ValidateCoreFlow()
    {
        AddRequired(flowController != null, "存在 MinLoopFlowController 总流程控制器。", "缺少 MinLoopFlowController。请在 MIN_LOOP_ROOT/FlowManagers 下新建对象并挂载。");
        AddRequired(terminalPresenter != null, "存在 MinLoopTerminalPresenter 终端展示器。", "缺少 MinLoopTerminalPresenter。终端刷卡、摘要和 A/B 页面无法显示。");
        AddRequired(replaySequenceController != null, "存在 ReplaySequenceController 儿童房复盘控制器。", "缺少 ReplaySequenceController。无法进入昨夜事件复盘。");
        AddRequired(trustStateController != null, "存在 TrustStateController 信任度控制器。", "缺少 TrustStateController。A/B 处置后无法记录信任度。");
    }

    private void ValidatePlayerAndViewSwitching()
    {
        FirstPersonMovement[] movements = FindSceneObjects<FirstPersonMovement>();
        FirstPersonLook[] looks = FindSceneObjects<FirstPersonLook>();
        PlayerInteraction[] interactions = FindSceneObjects<PlayerInteraction>();
        Camera[] cameras = FindSceneObjects<Camera>();

        AddRequired(viewSwitchController != null, "存在 ViewSwitchController 视角切换器。", "缺少 ViewSwitchController。无法从 Mia 切到陪伴单元视角。");
        AddRequired(movements.Length >= 2, "场景中检测到至少 2 个 FirstPersonMovement。", "建议放置 Mia 和 Companion 两套第一人称控制器，目前移动控制器少于 2 个。");
        AddRequired(looks.Length >= 2, "场景中检测到至少 2 个 FirstPersonLook。", "建议放置 Mia 和 Companion 两套视角控制，目前视角控制器少于 2 个。");
        AddRequired(cameras.Length >= 2, "场景中检测到至少 2 个 Camera。", "建议至少有 Mia Camera 和 Companion Camera。");
        AddRequired(interactions.Length >= 1, "场景中检测到 PlayerInteraction。", "缺少 PlayerInteraction。玩家无法按 E 与终端、门或安抚点交互。");
    }

    private void ValidateTerminal()
    {
        AddRequired(terminalFlow != null, "存在 ResidentTerminalFlow 17F-01 终端入口。", "缺少 ResidentTerminalFlow。玩家看向 17F-01 终端时不会进入最小循环。");

        if (terminalFlow != null)
        {
            AddRequired(HasCollider(terminalFlow.gameObject), "17F-01 终端入口带有 Collider。", "ResidentTerminalFlow 所在物体或子物体缺少 Collider，PlayerInteraction 射线无法命中。");
        }

        TerminalUIController terminalUI = FindSceneObject<TerminalUIController>();
        EventSystem eventSystem = FindSceneObject<EventSystem>();
        AddRecommended(terminalUI != null || terminalPresenter != null, "终端 UI 有控制器或占位展示器。", "没有检测到 TerminalUIController。MinLoopTerminalPresenter 可生成占位 UI，但正式 UI 冻结玩家需要配置 TerminalUIController。");
        AddRecommended(eventSystem != null || terminalUI != null || (terminalPresenter != null && terminalPresenter.CanCreateEventSystemIfMissing), "终端按钮有 EventSystem 或自动创建兜底。", "场景中没有 EventSystem，且终端脚本没有开启自动创建；UI 按钮可能无法点击。");

        if (terminalPresenter != null)
        {
            AddRequired(terminalPresenter.CanCreateFallbackUI || terminalPresenter.HasSingleButtonBoundUI, "终端单按钮页面有占位 UI 或正式绑定 UI。", "MinLoopTerminalPresenter 既没有开启占位 UI，也没有完整绑定正式 UI 的标题、正文和主按钮。刷卡/摘要页面无法显示。");
            AddRequired(terminalPresenter.CanCreateFallbackUI || terminalPresenter.HasChoiceBoundUI, "A/B 处置页面有占位 UI 或正式绑定 UI。", "MinLoopTerminalPresenter 既没有开启占位 UI，也没有完整绑定正式 UI 的第二按钮。A/B 页面无法显示。");
            AddRecommended(terminalPresenter.HasTerminalUIController || terminalPresenter.HasGameplayBehavioursToDisable, "终端打开时可以冻结或临时关闭玩家控制。", "MinLoopTerminalPresenter 没有绑定 TerminalUIController，也没有填写 Gameplay Behaviours To Disable；占位 UI 可点击，但玩家仍可能移动或转头。");
            AddRecommended(terminalPresenter.HasTerminalUIController, "终端 UI 已接入 TerminalUIController，可统一冻结玩家并管理鼠标。", "MinLoopTerminalPresenter 可以用禁用列表兜底，但正式演示仍建议配置 TerminalUIController。");

            if (terminalPresenter.HasAnyBoundUIAssigned)
            {
                AddRecommended(terminalPresenter.HasSingleButtonBoundUI, "正式终端 UI 的单按钮页面绑定完整。", "检测到部分正式终端 UI 字段，但标题、正文或主按钮未绑定完整；未完整时会回退占位 UI。");
                AddRecommended(terminalPresenter.HasChoiceBoundUI, "正式终端 UI 的 A/B 页面绑定完整。", "检测到部分正式终端 UI 字段，但第二按钮未绑定；A/B 页面会回退占位 UI。");
            }
        }
    }

    private void ValidateReplay()
    {
        bool canFallbackComfortAction = replaySequenceController != null && replaySequenceController.CanCreateFallbackComfortAction;
        AddRequired(comfortAction != null || canFallbackComfortAction, "存在 ComfortActionInteractable 唯一安抚操作点，或复盘控制器可自动生成兜底安抚点。", "缺少 ComfortActionInteractable，且 ReplaySequenceController 没有开启自动生成兜底安抚点。复盘会停在等待安抚阶段。");

        if (comfortAction != null)
        {
            AddRequired(HasCollider(comfortAction.gameObject), "唯一安抚操作点带有 Collider。", "ComfortActionInteractable 所在物体或子物体缺少 Collider，陪伴单元无法按 E 触发安抚。");
        }
        else if (canFallbackComfortAction)
        {
            AddRecommended(false, string.Empty, "未检测到手动摆放的 ComfortActionInteractable；ReplaySequenceController 会在运行时生成兜底安抚点。正式演示建议仍摆放 ComfortAction_Bedside。");
        }

        AddRecommended(subtitlePlayer != null, "存在 MinLoopSubtitlePlayer 字幕播放器。", "缺少 MinLoopSubtitlePlayer。复盘仍可按时间推进，但玩家看不到关键字幕。");
        AddRecommended(robotHudPresenter != null, "存在 MinLoopRobotHudPresenter 机器视角 HUD。", "缺少 MinLoopRobotHudPresenter。复盘仍能跑，但陪伴单元视角不会显示 02:47、心率、噩梦判定等机器信息。");
        AddRecommended(objectivePresenter != null, "存在 MinLoopObjectivePresenter 当前目标提示。", "缺少 MinLoopObjectivePresenter。最小循环仍能跑，但玩家可能不知道下一步该做什么。");
        AddRecommended(trustPresenter != null, "存在 MinLoopTrustPresenter 信任度显示。", "缺少 MinLoopTrustPresenter。终端仍会显示信任度结果，但没有独立信任度 UI。");

        SimpleActorCueController[] actorCues = FindSceneObjects<SimpleActorCueController>();
        AddRecommended(actorCues.Length >= 1, "检测到至少 1 个 SimpleActorCueController。", "建议给孩子模型或占位物挂 SimpleActorCueController，用于看向门口和切换状态。");
    }

    private void ValidateOptionalProps()
    {
        AddRecommended(doors != null && doors.Length > 0, "检测到 SmartDoorController 门控制脚本。", "没有检测到 SmartDoorController。若当前场景还没放门，可以稍后再挂。");

        if (doors != null)
        {
            for (int i = 0; i < doors.Length; i++)
            {
                if (doors[i] == null)
                {
                    continue;
                }

                AddRecommended(HasCollider(doors[i].gameObject), "门 " + doors[i].name + " 带有 Collider。", "门 " + doors[i].name + " 缺少 Collider，玩家可能无法射线交互。");
            }
        }

        AddRecommended(doorTriggerZones != null && doorTriggerZones.Length > 0, "检测到 SmartDoorTriggerZone 自动门触发区。", "如果大厅玻璃门需要靠近自动开门，请在门旁触发区挂 SmartDoorTriggerZone。");

        if (doorTriggerZones != null)
        {
            for (int i = 0; i < doorTriggerZones.Length; i++)
            {
                if (doorTriggerZones[i] == null)
                {
                    continue;
                }

                AddRecommended(doorTriggerZones[i].HasConfiguredDoor, "自动门触发区 " + doorTriggerZones[i].name + " 已能找到目标门。", "自动门触发区 " + doorTriggerZones[i].name + " 没有 Target Door，也没有父级 SmartDoorController。");
                AddRecommended(doorTriggerZones[i].HasUsableTriggerCollider || doorTriggerZones[i].CanCreateBoxColliderIfMissing, "自动门触发区 " + doorTriggerZones[i].name + " 有 Trigger Collider 或可自动创建。", "自动门触发区 " + doorTriggerZones[i].name + " 缺少 Trigger Collider，且未开启自动创建。");
            }
        }

        AddRecommended(feedbackObjects != null && feedbackObjects.Length > 0, "检测到 InteractionFeedbackController 反馈对象。", "没有检测到 InteractionFeedbackController。电梯按钮、刷卡、终端提交可以稍后再补反馈。");
        AddRecommended(stageObjectActivators != null && stageObjectActivators.Length > 0, "检测到 MinLoopStageObjectActivator 阶段对象显隐控制。", "没有检测到 MinLoopStageObjectActivator。下一户指引、终端高亮或安抚提示需要手动显隐。");
        AddRecommended(stageAnchorController != null, "检测到 MinLoopStageAnchorController 阶段锚点控制。", "没有检测到 MinLoopStageAnchorController。流程仍能跑，但 Mia/陪伴单元切换前后的站位需要手动摆好。");

        if (stageAnchorController != null)
        {
            AddRecommended(stageAnchorController.HasRules, "阶段锚点控制器已有站位规则。", "MinLoopStageAnchorController 存在但没有规则。请运行 Create 17F-01 Scene Skeleton 或手动添加 Mia/陪伴单元锚点规则。");
        }

        AddRecommended(stageCueController != null, "检测到 MinLoopStageCueController 阶段 Cue 控制。", "没有检测到 MinLoopStageCueController。流程仍能跑，但安抚点就绪、早晨回顾、下一户指引等阶段反馈需要手动触发。");

        if (stageCueController != null)
        {
            AddRecommended(stageCueController.HasRules, "阶段 Cue 控制器已有表现规则。", "MinLoopStageCueController 存在但没有规则。请运行 Create 17F-01 Scene Skeleton 或手动添加阶段反馈、开门、动画、音效规则。");
        }

        AddRecommended(guideMarkers != null && guideMarkers.Length > 0, "检测到 MinLoopWorldGuideMarker 世界指引标记。", "没有检测到 MinLoopWorldGuideMarker。如果 Guide 父物体下面还没放正式箭头/光圈，玩家可能看不到阶段指引。");
        AddRecommended(lightingStateController != null, "检测到 MinLoopLightingStateController 阶段灯光控制。", "没有检测到 MinLoopLightingStateController。流程仍能跑，但走廊、夜间复盘和早晨回顾的氛围需要手动切灯。");

        if (lightingStateController != null)
        {
            AddRecommended(lightingStateController.HasRules, "阶段灯光控制器已有灯光规则。", "MinLoopLightingStateController 存在但没有规则。请运行 Create 17F-01 Scene Skeleton 或手动添加走廊/夜间/早晨灯光规则。");
        }

        AddRecommended(audioStateController != null, "检测到 MinLoopAudioStateController 阶段音频控制。", "没有检测到 MinLoopAudioStateController。流程仍能跑，但走廊、夜间复盘和早晨回顾的环境声需要手动开关。");

        if (audioStateController != null)
        {
            AddRecommended(audioStateController.HasRules, "阶段音频控制器已有音频规则。", "MinLoopAudioStateController 存在但没有规则。请运行 Create 17F-01 Scene Skeleton 或手动添加走廊/夜间/早晨音频规则。");
        }

        AddRecommended(debugHotkeys != null, "检测到 MinLoopDebugHotkeys 测试热键。", "没有检测到 MinLoopDebugHotkeys。正式流程不受影响，但 Play Mode 调试会慢一些。");
    }

    private bool HasCollider(GameObject target)
    {
        return target != null && target.GetComponentInChildren<Collider>(true) != null;
    }

    private void AddRequired(bool passed, string successMessage, string failureMessage)
    {
        if (passed)
        {
            AddSuccess(successMessage);
            return;
        }

        errorCount++;
        reportBuilder.AppendLine("[必须] " + failureMessage);
    }

    private void AddRecommended(bool passed, string successMessage, string failureMessage)
    {
        if (passed)
        {
            AddSuccess(successMessage);
            return;
        }

        warningCount++;
        reportBuilder.AppendLine("[建议] " + failureMessage);
    }

    private void AddSuccess(string message)
    {
        successCount++;
        if (logSuccessDetails)
        {
            reportBuilder.AppendLine("[通过] " + message);
        }
    }

    private T FindSceneObject<T>() where T : Object
    {
        T[] objects = FindSceneObjects<T>();
        return objects.Length > 0 ? objects[0] : null;
    }

    private T[] FindSceneObjects<T>() where T : Object
    {
        return Object.FindObjectsOfType<T>(includeInactiveObjects);
    }
}
