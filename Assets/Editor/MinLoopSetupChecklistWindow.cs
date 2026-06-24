using System.Text;
using UnityEditor;
using UnityEngine;

public class MinLoopSetupChecklistWindow : EditorWindow
{
    private Vector2 scrollPosition;
    private string cachedChecklist;

    [MenuItem("Tools/Min Loop/Open Setup Checklist")]
    public static void OpenWindow()
    {
        MinLoopSetupChecklistWindow window = GetWindow<MinLoopSetupChecklistWindow>("Min Loop Checklist");
        window.minSize = new Vector2(560f, 520f);
        window.RefreshChecklist();
        window.Show();
    }

    private void OnEnable()
    {
        RefreshChecklist();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("17F-01 最小循环搭建清单", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("先创建骨架，再把 Mia、Companion、终端、儿童房、孩子、父母、门、锚点、灯光、音频和反馈对象摆到场景里。按清单命名后可用 Auto Bind References 自动拖一批脚本引用。", MessageType.Info);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create / Update Skeleton", GUILayout.Height(30f)))
        {
            MinLoopSceneSkeletonCreator.CreateSceneSkeleton();
            RefreshChecklist();
        }

        if (GUILayout.Button("Auto Bind References", GUILayout.Height(30f)))
        {
            MinLoopSceneAutoBinder.AutoBindSceneReferences();
            RefreshChecklist();
        }

        if (GUILayout.Button("Validate Scene", GUILayout.Height(30f)))
        {
            ValidateScene();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Refresh Checklist"))
        {
            RefreshChecklist();
        }

        if (GUILayout.Button("Copy Checklist"))
        {
            EditorGUIUtility.systemCopyBuffer = cachedChecklist;
            Debug.Log("[MinLoopSetupChecklistWindow] 已复制 17F-01 最小循环搭建清单。");
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8f);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        EditorGUILayout.TextArea(cachedChecklist, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    private void ValidateScene()
    {
        MinLoopSceneValidator validator = FindObjectOfType<MinLoopSceneValidator>(true);
        if (validator == null)
        {
            if (EditorUtility.DisplayDialog("缺少检查器", "当前场景没有 MinLoopSceneValidator。是否先创建标准骨架？", "创建骨架", "取消"))
            {
                MinLoopSceneSkeletonCreator.CreateSceneSkeleton();
                validator = FindObjectOfType<MinLoopSceneValidator>(true);
            }
        }

        if (validator != null)
        {
            validator.ResolveReferences();
            validator.ValidateSceneSetup();
            Selection.activeGameObject = validator.gameObject;
        }
    }

    private void RefreshChecklist()
    {
        cachedChecklist = BuildChecklist();
    }

    private string BuildChecklist()
    {
        StringBuilder builder = new StringBuilder(4096);

        builder.AppendLine("【目标】");
        builder.AppendLine("跑通 17F-01 最小循环：走廊 -> 终端刷卡 -> 摘要 -> 陪伴单元复盘 -> 安抚孩子 -> 父母对话 -> A/B 处置 -> 下一户。");
        builder.AppendLine();

        builder.AppendLine("【第一步：创建管理骨架】");
        builder.AppendLine("1. 点击 Tools/Min Loop/Create 17F-01 Scene Skeleton。");
        builder.AppendLine("2. 确认 Hierarchy 出现 MIN_LOOP_ROOT。");
        builder.AppendLine("3. 按第二步命名并摆好角色/演员/目标点后，点击 Tools/Min Loop/Auto Bind 17F-01 Scene References。");
        builder.AppendLine("4. 选中 MIN_LOOP_ROOT/MinLoopSceneValidator，点击 Validate Scene Setup。");
        builder.AppendLine();

        builder.AppendLine("【第二步：必须摆到场景里的对象】");
        builder.AppendLine("1. Player_Mia_Controller：Mia / 玩家第一人称控制器。");
        builder.AppendLine("2. Companion_Controller：陪伴单元第一人称控制器，放在儿童房复盘起点。");
        builder.AppendLine("3. Terminal_17F01：17F-01 门口终端模型或占位物。");
        builder.AppendLine("4. ReplayRoom_17F01：儿童房复盘空间，至少有床、门口方向、孩子位置。");
        builder.AppendLine("5. ComfortAction_Bedside：床边安抚交互点，可使用骨架自动创建的对象。");
        builder.AppendLine("6. Child_Actor：孩子模型或占位物。");
        builder.AppendLine("7. DoorLook_Target：孩子看向门外的空物体，放在儿童房门口方向。");
        builder.AppendLine("8. Mother_Actor / Father_Actor：父母模型或占位物，可后补。");
        builder.AppendLine("9. Guide_Terminal_17F01 / Guide_ComfortAction / Guide_NextResident_17F02：阶段指引父物体，骨架会自动创建。");
        builder.AppendLine("10. Light_Corridor_Warm / Light_Replay_Night / Light_Morning_Soft：阶段灯光，占位灯骨架会自动创建。");
        builder.AppendLine("11. Audio_Corridor_Ambience / Audio_Replay_Night_Ambience / Audio_Morning_Ambience：阶段环境声，占位 AudioSource 骨架会自动创建。");
        builder.AppendLine("12. Anchor_Mia_CorridorStart / Anchor_Companion_ReplayStart / Anchor_Mia_TerminalReturn / Anchor_NextResident_17F02：阶段站位锚点，骨架会自动创建。");
        builder.AppendLine("13. Feedback_ComfortReady / Feedback_MorningReview / Feedback_NextResidentGuide：阶段 Cue 默认反馈对象，骨架会自动创建。");
        builder.AppendLine("14. 摆好后运行 Auto Bind References；它会自动绑定 ViewSwitchController、ReplaySequenceController、TerminalUIController、Validator、阶段锚点、阶段 Cue、阶段灯光、阶段音频等常用字段。");
        builder.AppendLine("15. 如果 Child_Actor / Mother_Actor / Father_Actor 没挂 SimpleActorCueController，自动绑定工具会给这些命名对象补上该组件。");
        builder.AppendLine();

        builder.AppendLine("【第三步：核心字段拖拽】");
        builder.AppendLine("ViewSwitchController：");
        builder.AppendLine("- Human Root / Camera / FirstPersonMovement / FirstPersonLook / PlayerInteraction / Rigidbody / HumanCanvas。");
        builder.AppendLine("- Companion Root / Camera / FirstPersonMovement / FirstPersonLook / PlayerInteraction / Rigidbody / RobotCanvas。");
        builder.AppendLine();
        builder.AppendLine("ResidentTerminalFlow：");
        builder.AppendLine("- 挂在 Terminal_17F01 或其交互 Collider 对象上。");
        builder.AppendLine("- Flow Controller 指向 MinLoopFlowController。");
        builder.AppendLine("- Terminal_17F01 必须有 Collider。");
        builder.AppendLine();
        builder.AppendLine("ReplaySequenceController：");
        builder.AppendLine("- Flow Controller 指向 MinLoopFlowController。");
        builder.AppendLine("- Subtitle Player 指向 MinLoopSubtitlePlayer。");
        builder.AppendLine("- Comfort Action 指向 ComfortAction_Bedside。");
        builder.AppendLine("- 如果第一版暂时没有 ComfortAction_Bedside，保持 Create Fallback Comfort Action If Missing 开启，Play 后会生成 Generated_ComfortAction_Bedside 防止复盘卡死。");
        builder.AppendLine("- Child Actor 指向孩子身上的 SimpleActorCueController。");
        builder.AppendLine("- Door Look Target 指向儿童房门口方向空物体。");
        builder.AppendLine("- Wait Before Door Look After Wake 建议保持 0.35 左右，让无 Animator 的惊醒占位姿势先播放一点，再看向门口。");
        builder.AppendLine();
        builder.AppendLine("SimpleActorCueController：");
        builder.AppendLine("- Child_Actor / Mother_Actor / Father_Actor 可以由 Auto Bind References 自动补这个组件。");
        builder.AppendLine("- 有 Animator 时拖 Animator，并确认 Sleep / Nightmare / Comforted / Morning Trigger 名称。");
        builder.AppendLine("- 没有 Animator 时保持 Use Fallback Poses When No Animator 开启，设置 Pose Root 和 Sleep / Nightmare Wake / Comforted / Morning Pose。");
        builder.AppendLine("- 可以手动摆好姿势后，用组件右上角 Context Menu 的 Capture ... Pose From Current 记录姿势。");
        builder.AppendLine();
        builder.AppendLine("ComfortActionInteractable：");
        builder.AppendLine("- Sequence Controller 指向 ReplaySequenceController。");
        builder.AppendLine("- 物体或子物体必须有 Collider。");
        builder.AppendLine("- Visual Root 可以拖床边提示图标/夜灯高亮；不拖时保持 Create Fallback Visual If Missing 开启，会自动生成小发光球。");
        builder.AppendLine();

        builder.AppendLine("MinLoopStageObjectActivator：");
        builder.AppendLine("- 骨架会自动绑定 Flow Controller 和 3 条规则。");
        builder.AppendLine("- 把箭头、光圈、提示模型分别放到 Guide_Terminal_17F01、Guide_ComfortAction、Guide_NextResident_17F02 子物体下。");
        builder.AppendLine("- 如果还没有正式箭头/光圈，骨架会给 3 个 Guide 自动挂 MinLoopWorldGuideMarker，Play 后显示发光点、文字和距离。");
        builder.AppendLine("- Show Distance 建议保持开启，Distance Refresh Interval 默认 0.12 左右即可。");
        builder.AppendLine("- 流程阶段变化后，这些父物体会自动显示/隐藏。");
        builder.AppendLine();
        builder.AppendLine("MinLoopStageAnchorController：");
        builder.AppendLine("- 骨架会自动绑定 Flow Controller，并在 MIN_LOOP_ROOT/Anchors 下创建 4 个 Anchor 空物体。");
        builder.AppendLine("- 把 Anchor_Mia_CorridorStart 放到玩家开场位置，把 Anchor_Companion_ReplayStart 放到儿童房复盘起点。");
        builder.AppendLine("- 把 Anchor_Mia_TerminalReturn 放到 17F-01 终端前方，用于复盘结束回到终端。");
        builder.AppendLine("- Anchor_NextResident_17F02 默认只做下一户方向参考，不会自动传送玩家。");
        builder.AppendLine("- 运行 Auto Bind References 后，会自动把 Mia/Companion 根物体填到锚点规则。");
        builder.AppendLine();
        builder.AppendLine("MinLoopStageCueController：");
        builder.AppendLine("- 骨架会自动绑定 Flow Controller，并创建 3 条默认 Cue 规则。");
        builder.AppendLine("- 默认规则：等待安抚时播放 Feedback_ComfortReady，早晨回顾时播放 Feedback_MorningReview，完成时播放 Feedback_NextResidentGuide。");
        builder.AppendLine("- 后续可以在对应规则里追加 Door Cues、Animator Cues、Audio Cues，或用 Cue Event 调你自己的脚本。");
        builder.AppendLine("- 如果只想做第一版占位，把反馈对象的 AudioSource、Renderer 或 Light 拖好即可。");
        builder.AppendLine();
        builder.AppendLine("MinLoopLightingStateController：");
        builder.AppendLine("- 骨架会自动绑定 Flow Controller，并在 MIN_LOOP_ROOT/Lighting 下创建 3 盏占位灯。");
        builder.AppendLine("- 默认规则：走廊/终端用暖光，昨夜复盘用偏冷夜光，早晨回顾用柔光。");
        builder.AppendLine("- 后续放正式灯光时，把正式 Light 拖进对应规则的 Lights，或直接替换默认 Light 对象。");
        builder.AppendLine("- 如果不想改 RenderSettings，关闭对应规则的 Apply Ambient Color / Apply Fog。");
        builder.AppendLine();
        builder.AppendLine("MinLoopAudioStateController：");
        builder.AppendLine("- 骨架会自动绑定 Flow Controller，并在 MIN_LOOP_ROOT/Audio 下创建 3 个占位 AudioSource。");
        builder.AppendLine("- 默认规则：走廊/终端环境声、昨夜复盘环境声、早晨回顾环境声。");
        builder.AppendLine("- 第一版没有 Clip 也不会报错；后续把正式环境声拖到对应 AudioSource 的 AudioClip，或拖到规则的 Fallback Clip。");
        builder.AppendLine("- 刷卡哔声、按钮声、终端确认声仍然用 InteractionFeedbackController。");
        builder.AppendLine();
        builder.AppendLine("MinLoopObjectivePresenter：");
        builder.AppendLine("- 骨架会自动绑定 Flow Controller。");
        builder.AppendLine("- 第一版不拖 UI 也会自动生成左上角当前目标提示。");
        builder.AppendLine("- 后续正式 UI 可拖 Objective Root、Title Text、Body Text、Canvas Group。");
        builder.AppendLine();
        builder.AppendLine("MinLoopRobotHudPresenter：");
        builder.AppendLine("- 骨架会自动绑定 Flow Controller。");
        builder.AppendLine("- 第一版不拖 UI 也会自动生成机器视角 HUD，显示 02:47、心率、噩梦判定和安抚指令。");
        builder.AppendLine("- 如果场景里有 RobotCanvas 或 CompanionCanvas，Auto Bind References 会优先把它作为占位 HUD 父 Canvas。");
        builder.AppendLine("- 后续正式 UI 可拖 HUD Root、Time Text、Heart Rate Text、Status Text、Instruction Text、Accent Image、Canvas Group。");
        builder.AppendLine();
        builder.AppendLine("MinLoopTrustPresenter：");
        builder.AppendLine("- 骨架会自动绑定 Trust State Controller 和 Flow Controller。");
        builder.AppendLine("- 第一版不拖 UI 也会自动生成右上角信任度提示。");
        builder.AppendLine("- 后续正式 UI 可拖 Trust Root、Value Text、Delta Text、Label Text、Trust Slider、Canvas Group。");
        builder.AppendLine();
        builder.AppendLine("MinLoopDebugHotkeys：");
        builder.AppendLine("- 骨架会自动绑定 Flow Controller 和 Replay Sequence Controller。");
        builder.AppendLine("- Play Mode 测试热键：F1 重置，F2 打开终端，F3 刷工牌，F4 调复盘，F5 模拟安抚，F6 选 A，F7 选 B，F8 下一户。");
        builder.AppendLine("- 默认只在 Editor 或 Development Build 生效。正式演示不需要时可关闭 Enable Hotkeys。");
        builder.AppendLine();

        builder.AppendLine("【第四步：可选但建议补的反馈对象】");
        builder.AppendLine("1. Feedback_TerminalOpen：终端打开提示音/发光。");
        builder.AppendLine("2. Feedback_AccessCard：刷工牌哔声。");
        builder.AppendLine("3. Feedback_ReplayRequest：调出昨夜事件的确认声。");
        builder.AppendLine("4. Feedback_DispositionSubmit：A/B 提交反馈。");
        builder.AppendLine("5. Feedback_ComfortReady：等待安抚时的床边提示音/发光。");
        builder.AppendLine("6. Feedback_MorningReview：早晨父母对话开始时的提示反馈。");
        builder.AppendLine("7. Feedback_NextResidentGuide：出现下一户指引时的提示反馈。");
        builder.AppendLine("8. Elevator_Button_17F：电梯按钮可挂 InteractionFeedbackController，拖叮咚音和按钮 Renderer/Light。");
        builder.AppendLine();

        builder.AppendLine("【第五步：门】");
        builder.AppendLine("1. 大厅玻璃门：挂 SmartDoorController，Motion Mode = Slide，Moving Root 拖玻璃门板。");
        builder.AppendLine("2. 如果玻璃门要靠近自动开，在门下新建 Lobby_GlassDoor_TriggerZone，挂 SmartDoorTriggerZone，保持 Create Box Collider If Missing 和 Auto Mark Collider As Trigger 开启。");
        builder.AppendLine("3. SmartDoorTriggerZone 的 Target Door 拖大厅玻璃门；如果触发区是门的子物体，也可以交给 Auto Bind 自动找父级门。");
        builder.AppendLine("4. 17F-01 房门：挂 SmartDoorController，Motion Mode = Rotate，Open Local Euler Offset 通常 Y = 90。");
        builder.AppendLine("5. 门或门交互父物体必须有 Collider；自动门触发区必须是 Trigger Collider。");
        builder.AppendLine();

        builder.AppendLine("【第六步：正式 UI 接入】");
        builder.AppendLine("现在不用正式 UI 也能跑。后续 UI 上传后，在 MinLoopTerminalPresenter 的 Bound UI Optional 中拖：");
        builder.AppendLine("- Bound UI Root");
        builder.AppendLine("- Bound Title Text");
        builder.AppendLine("- Bound Body Text");
        builder.AppendLine("- Bound Primary Button / Text");
        builder.AppendLine("- Bound Secondary Button / Text");
        builder.AppendLine("- Bound Close Button / Text");
        builder.AppendLine("- 如果暂时不用 TerminalUIController，保持 Create Event System If Missing 和 Disable Gameplay Behaviours When Open 开启。");
        builder.AppendLine("- Gameplay Behaviours To Disable 拖 Mia 的 FirstPersonMovement、FirstPersonLook、PlayerInteraction。");
        builder.AppendLine("当前目标 UI 上传后，在 MinLoopObjectivePresenter 中拖 Objective Root、Title Text、Body Text。");
        builder.AppendLine("机器 HUD UI 上传后，在 MinLoopRobotHudPresenter 中拖 HUD Root、Time Text、Heart Rate Text、Status Text、Instruction Text、Accent Image。");
        builder.AppendLine("信任度 UI 上传后，在 MinLoopTrustPresenter 中拖 Trust Root、Value Text、Delta Text、Trust Slider。");
        builder.AppendLine("想让 Auto Bind 自动接正式 UI，推荐命名：");
        builder.AppendLine("- 终端：Terminal_BoundUI、Terminal_TitleText、Terminal_BodyText、Terminal_PrimaryButton、Terminal_SecondaryButton、Terminal_CloseButton。");
        builder.AppendLine("- 目标：Objective_Root、Objective_TitleText、Objective_BodyText。");
        builder.AppendLine("- 机器 HUD：RobotHUD_Root、RobotHUD_TimeText、RobotHUD_HeartRateText、RobotHUD_StatusText、RobotHUD_InstructionText、RobotHUD_Accent。");
        builder.AppendLine("- 信任度：Trust_Root、Trust_ValueText、Trust_DeltaText、Trust_LabelText、Trust_Slider。");
        builder.AppendLine();

        builder.AppendLine("【最小验收】");
        builder.AppendLine("1. Validate Scene Setup 没有 [必须] 错误。");
        builder.AppendLine("2. Play 后看向终端按 E，出现刷工牌页。");
        builder.AppendLine("3. 点击刷工牌，出现住户摘要。");
        builder.AppendLine("4. 点击调出昨夜事件，黑场切到 Companion。");
        builder.AppendLine("5. Companion 视角里能看到机器 HUD：02:47、心率、噩梦判定、安抚指令。");
        builder.AppendLine("6. 复盘中孩子说“妈妈”，安抚点出现。");
        builder.AppendLine("7. 按 E 安抚后进入父母早晨对话。");
        builder.AppendLine("8. 回终端后能选 A/B，显示信任度变化和下一户。");
        builder.AppendLine("9. 下一户阶段时 Guide_NextResident_17F02 显示；其他阶段不会常亮。");
        builder.AppendLine("10. 调出昨夜事件前，Companion 会先被放到 Anchor_Companion_ReplayStart。");
        builder.AppendLine("11. 复盘结束回到终端前，Mia 会被放到 Anchor_Mia_TerminalReturn。");
        builder.AppendLine("12. 走廊/终端、昨夜复盘、早晨回顾三个阶段的灯光颜色能随流程切换。");
        builder.AppendLine("13. 配好环境声 Clip 后，走廊/终端、昨夜复盘、早晨回顾三个阶段的环境声能随流程淡入淡出。");
        builder.AppendLine("14. 等待安抚、早晨回顾、下一户完成阶段会分别触发对应 Feedback 对象。");
        builder.AppendLine("15. 左上角目标提示会随流程显示当前该做什么。");
        builder.AppendLine("16. 选择 A/B 后，右上角信任度提示会显示当前值和变化量。");
        builder.AppendLine("17. 调试时可以用 F1-F8 快速跳测关键节点。");

        return builder.ToString();
    }
}
