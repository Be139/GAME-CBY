> 历史来源，2026-10-31 已整合。不得按本文件的旧菜单、路径、计数或未验收状态覆盖当前实现。现行入口：[程序手册](../../../脚本使用说明总表.md)、[交接指南](../../../HEARTH_ClaudeCode接手指南.md)。

# HEARTH 程序全量审查与整改

2026-10-29 建立；2026-10-31 状态：按用户要求暂停，未完成。范围：Assets 中全部 C#（含导入资产），Tools 中 C#/Python/Shell；不修改 Unity PackageCache、Library 编译产物。初始清单 507 文件、103,449 行，见 Docs/ProgramAudit/source-scan.json。逐文件完成状态将随审读更新，自动特征命中不等于缺陷。

## 审查方法

### 2026-10-30 持续目标与验收标准

继续全量审查，目标是使项目程序具备可维护性、可扩展性，并遵循团队游戏制作的工程惯例。以本项目实际生产需求落实下列标准；不存在适用于所有游戏公司的单一代码规范，不以引入更多框架或重写第三方库作为达标依据。

1. **职责与所有权**：剧情控制器管理顺序，UI 管显示与输入，数据资产承载文案和配置；共享相机、输入锁、音频、显隐状态有明确所有者。
2. **制作数据可编辑**：静态 UI 在正式 Prefab、站位在 Anchor、对白在正式稿；运行时按配置执行，不隐式覆盖制作数据。动态对象使用有用途的模板和生命周期。
3. **扩展路径明确**：新增住户、替换表现/模型/声音时有既有接口与说明；公共接口在禁用、取消、重入和销毁时遵守相同约定。
4. **引用与资源完整**：调用方、序列化字段、事件订阅、协程、生成对象和音视频资源成对管理，错误可定位，不吞错报成功。
5. **可验证与可交接**：逐文件标注审读范围和实际发现；变更附受影响行为验证及脚本总表记录。编译、定点测试、连续流程、视觉和性能分别报告。

全量范围维持初始 507 个文件，并跟踪后续增删；第三方源码审读与正式接入测试分别记录。尚未逐文件完成前，不声称所有程序已经达标。

核对每个文件的用途、使用位置、调用/绑定、编辑器与运行时职责、表现数据来源、资源/订阅生命周期及错误路径。场景引用索引与实际状态用于区分正式使用、兼容路径和第三方示例；没有挂载不自动判定可删除。第三方代码的问题优先在正式集成配置处处理，只有实际需要时维护补丁。

## 已定位并修复

### R01：Reach 空本地化组件启动越界

`LocalizedObject.InitializeItem()` 在没有 UIManager 时访问空搜索结果 `[0]`。正式游戏只使用 Reach 视觉层，却在五个正式 Prefab 里带入了 37 个无 key、无事件、无文本/音频/图片目标的 Custom LocalizedObject；这些组件没有业务用途。

已删除上述正式资产中的空组件，关闭对应 ButtonManager.useLocalization；没有额外生成本地化管理器或覆盖正式英文文本。五个资产的 TMP 文本逐项比对一致，场景剩余组件为 0。现有 HearthProductionUiTools.ValidateProductionUi 增加对应集成检查。

验证：实际 Play 进入 Lobby Opening、Human 视角和正确相机，Console error=0；退出 Play，SampleScene 已保存。第三方套件本身的通用本地化实现仍保留；未来确需多语言应另行配置完整文案来源与资源，不能重新复制空组件。

## 正在追踪

R21 终端 Boot 子协程所有权（待验证）：完整读取 HearthTerminalBootSequence，并继续终端 565–1460、3735–3925。开/关动画由终端独立 StartCoroutine，但原句柄为主协程局部变量；切换流程或立即关闭后旧 Boot 不会随主协程停止。现改为一个终端字段，主流程切换和立即关闭先 StopBootRoutine，正常结束也清空。需真实 Play 覆盖途中关闭/停用和再次打开；尚未编译/回归，Unity instances=0。未修改 BootSequence 的视觉参数。

R20 终端 Surface fallback 边界（待编译/回归）：终端两种 Resolve 方法在 fallback=false 时仍先按旧名修补组件并重排，开关只拦截完全新建。已把既有检查前移至旧名查找之前，保留正式 Bindings 和有效已配置 Surface 入口。照片 WorldView 已全文审读，正式绑定会关闭 fallback 并跳过内部布局覆盖；UpdatePose 的相机相对摆位是现有功能。终端当前只读至相关调用分支，未全文审查。CLI 服务在线但 instances=0，测试待重连；总表已同步。

对白支撑代码覆盖更新：8 个文件逐项记录于 `Docs/ProgramAudit/dialogue-source-review.md`。核对当前 71 个对白资产：12 个非默认时长模式均为无语音时间卡；16 个播放策略 ID 均有资产文本匹配。此次无新增生产代码变更。Surface 的终端/照片布局调用须随调用方继续审读，未将数据扫描当成线路通关。

R19 字幕生命周期（定点及真实 Play 取消回归通过）：MinLoopSubtitlePlayer 的 OnDisable 原先只释放部分显示状态，未失效外部 IEnumerator 或停止自身播放/语音；现复用 Stop。LineStarted 后加已有播放代次检查，时间卡淡入/标题迁移迭代器在取消后退出。两类序列内部入口拒绝停用播放。已核对 Finale 外部 yield 调用、Stop/隐藏/绑定/渐变代码；文件 2313 行尚未全文审读。重连后 `test-subtitle-cancel.cs` 四项通过：回调取消、停用迭代器拒绝、外部淡入和标题过渡取消；Console error=0。随后真实 Play 通过：外部 MonoBehaviour 持有时间卡序列、语音实际启动后停用字幕，跨帧且超过淡入时限不复活、不发完成事件；重新启用播放正常。临时对象、Clip、Sequence 已清理，恢复后台设置并退出 Play，Console error=0。该结果不代替连续剧情通关。旧 ObjectivePresenter/TrustPresenter 路径已不存在，不能据旧脚本表推断现行源码覆盖。

R18 阶段控制器整组审读：完整读取 LightingStateController、StageObjectActivator、StageAnchorController、StageCueController，搜索全部 C# 重新绑定/环境恢复调用。四者均补停用重绑保护；Lighting 增加已捕获标记，避免未捕获时恢复环境默认值。Unity 编译并运行 `test-stage-controller-lifecycle.cs`：四个控制器各六项可观察行为断言、环境两项断言通过，共 26 项。测试临时对象位于 PreviewScene，环境参数在 finally 还原；未保存正式场景。总表四项已同步。仍需继续检查 Cue 延迟记录/重绑后待执行任务，以及共享 object/behaviour 规则顺序；本轮不据推测修改这些语义。

R17 阶段音频控制器：全文审读并搜索调用方，`SetFlowController` 停用时仍订阅/应用规则。现只在 `isActiveAndEnabled` 时订阅并刷新，停用绑定留到 OnEnable。Unity 编译后 `test-stage-audio-rebind.cs` 通过：隐藏绑定、启用应用、停用重绑、旧流程解绑、新流程响应；测试为 PreviewScene 显式生命周期，不替代真实音频时序验收。脚本使用说明已更新。

R16 最新验证：真实 Play 运行姿态后禁用，下一帧不再覆盖外部设置的坐标；重新启用后初始姿态恢复正常。`test-cue-disable.cs` / `-finish.cs` 通过，恢复后台设置、销毁临时对象并退出 Play。用户补充的“优先精简实现且保证功能完整”已同步需求文档，后续继续以现有接口解决实际问题。

R16 SimpleActorCueController：未捕获初始姿态时禁止使用默认零值重置，停用时停止姿态协程。Unity `test-cue-baseline.cs` 两分支通过：未捕获保持原位置旋转缩放，已捕获恢复位置缩放。脚本说明已同步；停用后的跨帧写入测试未完成。当前场景索引无正式挂载，不据此删除兼容控制器。

R15 最新验收：无目标测试通过（Animator 不被暂停）；真实 Play 平滑姿态运行中禁用，activeRoutine 清空，后续帧未覆盖手动设置的 Transform，重新启用即时应用正常。测试恢复后台运行设置并销毁临时对象，Console error=0，已退出 Play。证据 `test-pose-missing-root.cs`、`test-pose-disable.cs` / `-finish.cs`。

R15 姿态预设：查阅场景索引有 33 个 PosePreset 挂载（含参考对象），SimpleActorCueController 为 0，后者未据此删除。PosePreset 增加停用协程清理，目标验证先于 Animator 暂停；未改变即时/平滑接口行为。无目标测试已准备，Unity 域重载中 503 未执行，平滑停用跨帧仍待回归。SimpleActorCue 初始捕获/重置问题继续追踪。

新增逐文件审读：HearthActorPosePreset、SimpleActorCueController 全文。PosePreset 在 ApplyPose 找不到 targetRoot 之前就暂停 Animator，且未在 OnDisable 停止 activeRoutine；SimpleActorCueController 同样无停用清理，captureInitialPoseOnAwake=false 时 ResetToInitialPose 仍使用默认初始向量（可能移至零坐标）。17F01 使用 PosePreset 的睡眠/醒来/安抚接口，17F02 有 fallback 接入。须结合正式挂载/隐藏顺序及关闭捕获的配置确认修复，不擅自把姿态暂停改为启用全部 Animator。此次只审读，未修改这两个脚本。

R14 最新验收：重载后确认 GAME-CBY 会话，再执行双 PlayableGraph 测试；timeScale=0 下 unscaled 淡入完成、scaled 淡入仍暂停，符合各自时钟。恢复原 timeScale/runInBackground、销毁临时对象、Console error=0，已退出 Play。本结果覆盖此前连接失败的待验状态。

R14 验证准备：已编写真实 Play 的双时钟测试 `test-animation-clock.cs` / `-finish.cs`。首次启动请求因 MCP 连接失败未得到执行结果；随后服务/编辑器恢复，finish 明确返回 Missing test root，已退出 Play。没有获得淡入完成证据，仍需重跑。端口已监听，未重复启动服务。

演员循环实时证据：场景 18 个 StateSlot 的 declaredLoop 与 clip.isLooping 均一致；17F03 PlayActorLoop 也用于非循环 Sitting/SittingPose。不能直接让这些调用强制循环改变现有坐姿行为。待统一该接口与 Clip 的唯一控制来源。

R14 全文审读 HearthActorAnimationPlayer：PlayableGraph 遵循 useUnscaledTime，FadeInputs 却固定 Time.deltaTime，暂停时会造成图继续播放、混合权重停住。已统一 FadeInputs 时间基准，待编译与 timeScale=0 的实际跨帧回归。该播放器与 AnimatorDriver 是两种既有执行方式，不能把前者 PlayLoop 的真实覆盖语义直接套到后者。

R13 演员无效状态副作用：PlayInternal 原先在验证状态路径前启用 Animator、改速度/根运动并恢复 Transform；现把实际状态验证前置。Unity 已加载修改，使用真实母亲 Controller 的隔离测试通过：无效状态返回 0，enabled/speed/rootMotion/Transform/ActiveStateId 保持不变。全文读完 HearthActorRootMotionRelay，调用方 17F03 的 PlayActorOnce/Loop 只确认 StateSlot 存在，不能替代 Animator.HasState；循环语义与 Controller 配置仍待核对。

R12 终端交接补验完成：正式 Play 注入 pending handoff，分别 choiceInputEnabled=true/false 调用 Finale.ResetForPreview，pending 清空且 terminalInputReady 恢复到对应许可值，两项通过，Console error=0，已退出 Play。验证状态恢复接口，不等同于从终端淡出连续中断演出。

新增全文审读：HearthActorAnimatorDriver。待追踪：PlayLoop/PlayOnce 只写 StateSlot.loop，但 PlayInternal 不读取该字段，实际循环依赖 Animator/Clip；缺失 state 时 PlayInternal 已先改 speed/root motion/transform，与 HoldStateAtStart 先验证再修改的行为不一致。需查真实调用及 Controller/RootMotionRelay，再决定接口与最小修改；尚未改演员脚本。

R12 验证更新：正式 SampleScene Play 11 项通过（Inactive HUD 保留、禁用入口、注入 Photo/Shutdown/FinalChoice 的停用、外部锁保留、句柄清空）；Shutdown 实际启动高信任挑战，Photo/FinalChoice 仅状态注入，不证明执行中协程中断。Console error=0，已退出 Play。追踪终端发现 Reset 未取消 customActionHandoffPending，新增调用现有 CancelCustomActionHandoff 恢复终端输入，待编译与此路径回归。

R12 终局停用清理（实现待验证）：事件订阅迁至 OnEnable/OnDisable；停用运行中终局复用 ResetForPreview，调用系统 Abort 接口并释放本 owner 锁。Inactive 停用不批量隐藏共享 HUD。禁用入口拒绝启动。HUD routing 改为实际接管时捕获/一次恢复，避免 Awake 快照在不相关重置中覆盖当前设置。需要 Photo/Shutdown/FinalChoice 停用与重新启用回归，尚未验收。

R11 顺序挑战/HUD 路由 Play 回归：`test-shutdown-routing.cs` 14 项通过，覆盖原路由 true/false 的高信任确认、低信任三波确认、取消、中止，确认不重复内部结局/重开选择；系统中止不派发结果事件、禁用拒绝 Begin、重新启用仅订阅一次。测试使用临时 HUD/页面调用真实方法，Console error=0，已退出 Play。正式 Virus 弹窗系统 Abort 组合与 Finale 自身停用仍待验证/完成。

R11 路由关联整改：Sequential 原先将 routeFinalChoiceInternally=false 后不恢复；现保存/恢复于结束、取消、中止和换 HUD。HUD 四个关机事件分支在派发前固定本次路由，避免同步回调恢复开关后改变正在提交的分支；中止只隐藏自身关机页。上一轮 Abort 接口编译后 Console error=0；本次新增路由改动仍待编译及事件回归。

R11 系统中止契约（实现待验证）：HearthShutdownChallenge 增加 AbortChallenge，两派生实现及 Finale.ResetForPreview 已接入；不调用完成/玩家取消事件，不改变高信任 V2 忽略 Cancel 的规则。Virus 停用清理弹窗与确认页；Sequential 停用中止/退订、启用订阅。两实现禁用时拒绝 Begin。全文检索仅两派生类，均已实现；请求 Unity 编译。仍需系统中止事件与重开回归，Sequential 的 HUD routing 恢复及 Finale 自身停用清理继续追踪。

R10 正式配置落地：实时 Edit Mode 确认 `Gameplay/Finale_17F04/Hearth17F04FinaleController.playerControlLock` 为空，唯一共享锁为 `HearthHudRoot`。已通过 SerializedObject 显式绑定并保存 SampleScene，最终场景差异仅该引用 1 行。保存时发现玩家 Movement/Look 启用状态附带变化，已按保存前 Git 基线恢复，最终差异检查通过。此前隔离锁测试虽在 PreviewScene，内部 RefreshPreferredViewControls 会调用全场景 ViewSwitch，后续此类测试须在 Play 中进行或明确恢复真实组件状态，不能假定 PreviewScene 自动隔离静态/全局查找副作用。

R10 控制锁接入：Photo 使用 Finale 既有 PlayerControlLock，以自身申请移动/视角/交互/菜单锁，仅释放自身所有权。新增只读属性，没有第二套锁系统。隔离 PreviewScene 叠加测试通过、Console error=0。SampleScene 文本中现有 playerControlLock 字段存在 4 处空引用，需要核对正式 Finale 的实时解析与显式绑定，后续仍须配置落地；无锁旧配置暂沿用直接组件路径。

R10 Play 补验：`test-photo-transition-abort.cs` / `-finish.cs` 在真实 Play 启动 0.2 秒进入转换，立即禁用 Photo，原玩家相机立即恢复；超过原转换截止时间后，照片/临时转换相机仍关闭、IsTransitioning=false，旧协程未重新开启目标相机。后台运行设置恢复，临时对象销毁，Console error=0，退出 Play。该项覆盖进入中断；退出中断及控制锁叠加仍待测试。

R10 更新：补齐 Photo.AbortView → Finale.AbortPhotoInspection，仅中止绑定照片当前 Photo 阶段，停止照片协程/字幕，回到 LivingRoom 并重新开放照片，不触发照片完成或女儿房解锁。Unity 已加载更新代码；隔离 PreviewScene `test-photo-abort.cs` 5 项通过，覆盖相机/监听器恢复、重试、状态清理、重复中止、禁用交互。仍待真实转换跨帧中断与控制锁集成，未宣称终局全流程通过。

R10 照片系统中止（实现中）：PhotoFrame 新增 AbortView，停用/Finale.ResetForPreview 走同一视图恢复入口，停止自有协程并取消转换相机，恢复捕获的相机/监听器/控制快照。没有调用 CompletePhotoInspection，避免系统重置推进剧情。尚待编译与进入/退出转换测试；单独停用后的 Finale.Photo 状态恢复、控制锁统一仍未完成，不能视为整条终局清理已验收。

### R09：关机弹窗取消时遗漏正在消失的对象（已修改，待 Play 验证）

最新验证覆盖下述失败状态：失败诊断为 Play frame=2，后台没有推进游戏帧。测试临时启用 Application.runInBackground 并记录起始帧，确认跨帧后检查；Cancel/重新 Begin 的旧运行时弹窗已全部销毁，仅保留模板，连同 3 项状态断言均通过。测试恢复原后台设置，Console error=0，已退出 Play。此结果验证取消/重开清理；正常逐波完成及高信任组合尚需纳入终局回归。

Play 验证进展：编译后 Console error=0，实际 Play 中“消失中持有对象、Cancel 清空且事件一次、重新 Begin 清空旧列表”3 项通过；后续检查根子物体只剩 Template 的断言失败，尚不能确认是残留弹窗还是其他运行时子物体。已补失败输出的子物体名称、暂停状态与帧号，需重跑定位，不将此项标为通过。已退出 Play。

全文审读 HearthVirusPopupShutdownChallenge 后确认：Submit 从 activePopups 移除弹窗后启动消失协程；ResetRuntime 停止全部协程，只销毁 activePopups，因此会遗漏消失中的对象。新增 dismissingPopups 持有该阶段对象，正常消失时移除，重置时统一销毁。PopupState 标记消失状态，入场动画不再竞争写入同一对象。未修改高信任 V2 Cancel 规则。脚本说明已补齐；终局整体停用/系统中止契约仍待处理。

### 2026-10-30 会话恢复后的验证更新（优先于下文待验证旧状态）

R08 定点验证：Unity 加载正式 Inspection Prefab 的隔离内容，执行 `test-inspection-phases.cs`。绑定、检查/解释/处置互斥、解释拒绝提交、等待释放阶段暂不开放选择、单次提交、拒绝后恢复、关闭隐藏及禁止输入全部通过；全体 RectTransform 的数量、anchoredPosition、sizeDelta 不变。Console error=0，卸载测试内容且没有保存 Prefab。测试直接调用阶段激活函数，尚未证明真实 Play 的 Space 松开跨帧逻辑及对白组合显示；后续保留这两项验证。

R06 补验完成：重新确认 GAME-CBY 编辑器连接后，在隔离 PreviewScene 执行 `test-player-target-lifetime.cs`，9 项全部通过；新增实际调用 UpdateCurrentTarget 验证有效 owner 接管、owner 销毁后释放当前目标并清空 proximity 引用。Console error=0。该结果补齐 owner 生命周期的定点验收，不代替连续游戏流程验证。

用户已恢复 Unity 会话，CLI 确认 GAME-CBY@ce06e3f26f537e83；Console error=0。`test-player-target-lifetime.cs` 已通过 6 项（正常、禁用组件、隐藏对象、业务不可用、销毁接口、null），R06 的接口判定已验证；owner 销毁后的近距离接管仍需补实际 UpdateCurrentTarget 测试。

已全文审读 Hearth17F04FinaleController、Hearth17F04HomeUnitInteractable、Hearth17F04RoomDoorInteractable、HearthPhotoFrameInteractable、HearthShutdownChallenge、HearthSequentialShutdownChallenge、TrustStateController。信任度 0 默认高信任预览与剧情变更记录 456–457 行一致，保留。待整改：Finale 缺 OnDisable 清理；Photo.OnDisable 仅隐藏视图，没有恢复相机/控制与转换状态；高信任 Cancel 按正式规则无效，终局重置不能用 Cancel 代替系统中止。需要完整读取 VirusPopupShutdownChallenge/TerminalCameraTransition 后统一取消契约，尚未改这些脚本。

- R08（实现待编辑器验证）：Hearth17F03InspectionPanel 留有已禁用的迁移排版系统，可动态创建控件、硬编码位置并覆盖标题。全 Assets 的 C# 及序列化事件检索无三个迁移方法消费者；正式 Prefab 开关为 0。已移除 332 行旧迁移实现、废弃开关/属性与 Editor 中对应检查，清理 Prefab 两字段；保留阶段/输入/主题颜色/内容绑定，不更换视觉。脚本总表已同步。需编译及检查→解释→处置回归。
- project 索引 19 已读取，但合并输出中检查页局部曾被截断；已补读 RefreshModeVisuals 至 ConfigureText，以及余下生成/协调器方法。Hearth17F03UnitInteractable、Hearth17F04CatGuideController 全文读到。删除代码会改变批次分组，后续不能直接照旧索引跳过；建议按文件继续从 Hearth17F04FinaleController 起读取，并以 source-scan 路径对照覆盖。

- R07（实现待验证）：CityLotAssetPlacer.BuildAxisLayout 在 roadWidth 为足够大的负值时 cursor 不前进。Inspector 增加 Min(0)，函数入口也按非负值计算；待负值/0/正常宽度测试，未重生成正式场景。
- 继续审读 project 索引 18：SmartDoorTriggerZone、ComfortActionInteractable、Hearth17F03GazeInteractable 全文，以及 Hearth17F03InspectionPanel 前 500 非空行；下次索引 19。触发区 occupants 在 collider 销毁/禁用且没有 Exit 时残留需验证；当前未修改。进入 UI 后需先复读 UI 库规范/目录再处理动态外观路径。
- R06 测试片段已准备 `Logs/HearthUiReview/test-player-target-lifetime.cs`，尚未运行。当前 Git diff --check 通过，Editor.log 未找到 error CS，但这不能替代 Unity 实时回归。会话恢复尝试：CLI 起初 server 正常但 instances=0；cua 识别 GAME-CBY 窗口但 AX 不可用且无法截图；随后 HTTP 8080 无连接，按记录恢复后台服务。保持目标 active，继续独立源码工作；不将暂时连接失败视为全量任务阻塞完成。

- R06（实现待验证）：PlayerInteraction 的接口引用绕过 Unity 销毁对象 null 语义，且不检查 Behaviour 启用状态，近距离目标不检查 owner 生存期。已补统一有效性判定和 owner 检查；需编译后覆盖正常/禁用/隐藏/销毁接口/owner 销毁测试。刷新时遇到 MCP 503，尚未验证此项，勿重放任何场景修改。
- 最新审读游标：project 索引 17 已读，PlayerInteraction、SmartDoorController 全文；下次索引 18。SmartDoorController 的禁用后 motion/audio 协程清理、片段音频时长使用缩放时间的问题需结合调用方继续检查。R05 已完成真实 Play 回归并补总表；R06 尚待验证。

- R05（已修复，Play Mode 6 项断言通过）：InteractionFeedbackController 连续触发缓存高亮状态且禁用不恢复视觉。现先恢复旧快照再播放，禁用停止协程并恢复，拒绝禁用播放，开始回调停用时不再施加高亮。测试见 `Logs/HearthUiReview/test-interaction-feedback.cs`，Console error=0，已退出 Play，脚本总表已更新。

- R04（已修复，4 项回归通过）：CityFacadeBillboardPlacer.ClearOrphanBillboards 复用生成时的活动状态/Renderer 启用筛选，将隐藏建筑误判为孤立源。已改为检查源引用和包含 inactive 的 Renderer 存在性；隐藏建筑/禁用 Renderer/隐藏 Slot 保留，真实缺失源清理。测试见 `Logs/HearthUiReview/test-billboard-orphan-cleanup.cs`，临时加法场景验证，脚本总表已更新。

- R02（已修复，定点验证通过）：HearthSfxCuePlayer 的循环维护误依赖 followTarget；分段时长使用最大随机音高而非本次实际音高；禁用播放器仍可启动外部 AudioSource。已解耦循环/位置跟随，按 source.pitch 计算时长，禁用时拒绝播放。
- R03（已修复，定点验证通过）：HearthDialogueSfxTrack.Configure 在禁用状态下仍订阅字幕事件，替换 cuePlayer 前没有停止旧循环。已先停止旧循环再重配，并限制订阅到启用状态。
- R02/R03 证据：`Logs/HearthUiReview/test-audio-lifecycle.cs`，2026-10-30 Unity 编译后执行成功，7 项断言通过。隔离 PreviewScene、Edit Mode 显式生命周期调用；未替代真实播放与连续剧情验收。脚本总表已更新。
- 前次中断已修改 FinalDialogueSync/FinalVoiceBinder 的 TimeCard 处理，但尚需针对现行文件复核、验证与补说明，不能当作已验收。

### 审读游标（用于继续工作）

`Logs/HearthUiReview/read-programs.py project` 的 0–5 批在前次会话中读取；本次继续读取 6–9 批（共 69 批，185 个项目归属文件）。6：RuntimeTopologyTools；7：StorySfxValidator、TypographyTools、UiLibraryWindow、UiV2VectorAssetEditor、MinLoopSceneValidatorEditor；8：AudioChannelSource、AudioSettingsController、DialogueSfxTrack、FootstepAudioProfile、SfxCatalog；9：SfxCuePlayer、CityBillboardContentController。后续从索引 10 继续。读取不代表全部结论/验证完成；当前发现见 R02/R03，其他模块仍需合并调用方结论。

后续游标更新：10–12 已读，覆盖 CityBillboardContentDistributor、CityBillboardPlacementSlot、CityBuildingPlacementSlot、CityDuskLightingPresetController、CityFacadeBillboardPlacer 全文；**下次从 project 索引 13 继续**。广告牌批量制作是仍保留的显式 CustomEditor 入口，不因出现 CreatePrimitive 就认定应删除。待核对：FindGeneratedCityRoots 仍限定顶层根且默认名称提示是旧名，可能与现行分组冲突；ClearOrphanBillboards 将 inactive/no active renderer 当成不存在，需与正式使用配置核对后修复；Dusk 的 ExecuteAlways 自动应用/恢复需核对当前配置与制作预期。尚未修改这些环境脚本。

本次音频编译后 Console error=0（2026-10-30），正式场景仍为 SampleScene。测试未保存正式场景；未进入 Play。新测试片段位于被 Git 忽略的 Logs/HearthUiReview，仅作当次验收工具，脚本使用说明已登记。

2026-10-30 下一轮审读更新：project 索引 13–16 已读，CityLotAssetPlacer 全文、CityWindowNightController、IInteractable、InteractionFeedbackController 全文；**下次从 project 索引 17 继续**。明确待整改：InteractionFeedbackController 重入前缓存正在发光的状态，可能无法恢复原视觉；OnDisable 缺少协程/视觉清理。先追踪了 7 个正式 Feedback_* 挂载点及 MinLoopFlowController/StageCue/SmartDoorTriggerZone 调用。CityLotAssetPlacer 的内部道路宽度未约束，BuildAxisLayout 在负值配置下可能不前进，需补针对性修复。环境工具并非自动启动生成，保留显式配置用途。

实时环境证据：CityBuildings_Manual、CityBuildings_Generated 仍为顶层根；当前 CityBillboardAuthoring.useGeneratedCitySlots=false、generatedCityRoots=[]、manualBuildingRoots 有一个空引用，manualBuildingRootNameHints 已为 CityBuildings_Manual。需要补绑定而非重生成建筑/广告牌；目前未改场景。FindGeneratedCityRoots 的旧名称提示仍需同步现行根名。

- 正式 UI 绑定存在时仍保留的动态创建、硬编码布局与 fallback：区分真实动态效果和过时生成路径后处理。
- 自有与导入程序的调用、场景挂载、编辑器自动写入与资源生命周期。
- 旧校验器中指向已退役菜单的诊断提示，以及仅对历史外观成立的检查口径。

## 验证边界

本页当前只把 R01 记为已修复；全量审读尚未完成。编译、启动、定点接口测试、连续通关与视觉验收分别记录，不互相替代。
