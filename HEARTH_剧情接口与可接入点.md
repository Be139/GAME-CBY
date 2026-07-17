# HEARTH 剧情接口与可接入点

> 后续做剧情、字幕、语音、处置记录、信任度或关卡流程前，先读 `HEARTH_剧情变更记录.md`，再读这份文档，最后读 `脚本使用说明总表.md` 中对应脚本条目。
> 如果某个住户的最新剧情变更记录和旧接口说明冲突，先以 `HEARTH_剧情变更记录.md` 的最新记录作为当前制作意图，再决定是否同步修改脚本、字幕资产或正式策划案。

## 最新剧情变更入口

- `HEARTH_剧情变更记录.md`：记录用户口述的剧情、流程、演出、角色走位、UI 触发条件等调整。
- 当前 17F02 最新剧情以 `2026-07-02 17F02 剧情与游戏进程调整` 为准；其中字幕资产和当前场景路径已在 `2026-07-02 17F02 剧情与走位同步实现` 中同步。
- 17F02 中女主离开卧室的路径点已写入当前场景：`Anchor_Wife_17F02_Path01 -> Anchor_Wife_17F02_Path01 (1) -> Anchor_Wife_17F02_Path01 (2) -> Anchor_Wife_17F02_Path01 (3) -> Anchor_Wife_17F02_Path01 (4) -> Anchor_Wife_17F02_Path01 (5) -> Anchor_Wife_17F02_DoorPause -> Anchor_Wife_17F02_ExitOutside`。

## 三户“字幕结束后开放交互”统一入口

### 17F01 小男孩

- 判定对象：`MIN_LOOP_ROOT/ReplayRoom_17F01/RuntimeInteractables/Capsule Mesh (1)`。
- 开放顺序：卧室字幕完成 -> 等待 `Prompt Delay After Bedroom Prelude` -> `HearthCompanionReplayInteractable.SetAvailable(true)`。
- 检测规则：准星中心射线必须命中胶囊体，并通过距离和床边允许侧判断。
- 当前射线长度：`Max Distance = 0.71`；后续不要由自动绑定工具擅自覆盖为其他数值。

### 17F02 女主人

- 开放顺序：倾诉字幕完成 -> 等待 `Bedroom Prompt Delay After Confide Seconds` -> `HearthCompanionHudController.ShowCurrentHoldPrompt()`。
- 确认入口：`HearthCompanionHoldPrompt` 完成 100% 后触发当前 Scene Id；`17F02_02` 会把 `bedroomAcknowledged` 设为 true。
- 文案、按键、长按时长统一在 `17F02_02` 的 `HearthCompanionHudSceneData` 修改。

### 17F03 实体陪伴单元

- 开放顺序：米娅入户 -> 父母第一幕对话全部完成 -> `Hearth17F03UnitInteractable.SetAvailable(true)`。
- 判定对象：实体 `ROBOT/InteractionVolume_17F03`。
- 玩家入口：`PlayerInteraction` 命中后显示 `E INSPECT COMPANION UNIT`，按 E 调用 `HearthCompanion17F03ReplayController.OpenUnitInspection()`。
- 故障检查面板中按 Space 调用 `BeginRecordedReplay()`；Esc 关闭检查并回到米娅。

统一制作规则：任何家庭都应先 `yield return` 等对话序列结束，再开放交互 Collider 或调用 `ShowCurrentHoldPrompt()`；不要依赖固定总秒数猜测字幕结束时间。

共享机器人长按提示只有当前正在运行的关卡可以管理。17F03 只允许在 `AwaitingDaughter / AwaitingMother` 阶段逐帧刷新目标提示；关卡处于 Inactive 或其他阶段时，不能主动隐藏 17F01/17F02 正在使用的共享提示。

## 17F03 第三幕动作与字幕入口

- 女儿动作状态由 `HearthCompanion17F03ReplayController` 驱动：`Walk -> Talking -> EnteringCode`。
- `Assets/Data/MinLoop/Dialogues/17F03_NightDaughter.asset`：女儿走到陪伴单元前后的主要倾诉。
- `Assets/Data/MinLoop/Dialogues/17F03_NightShutdownLeadIn.asset`：陪伴单元回应与女儿的 `Enough.`；这段仍保持 Talking。
- `Assets/Data/MinLoop/Dialogues/17F03_NightShutdownAction.asset`：屏幕打开、维护菜单、核心服务关闭；该段开始时才播放 `EnteringCode`。
- 每句仍可独立修改 `Speaker / Text / Start Delay / Hold Seconds / Voice Clip`。有 Voice Clip 时字幕至少等待音频长度，动作切换不依赖手写总秒数。
- 正式演员 Animator 只由 `HearthActorAnimatorDriver` 控制；不要重新启用模型上的 `CityPeople` 自动随机动画。
- 第一幕母亲 `SitToStand` 的水平位移会保留到 Talking。动作位移进入 `Actor_Mother_17F03_RuntimeRoot`，不会写回第一幕 Anchor；进入第二幕时再由 `Anchor_Mother_17F03_Midday` 控制新站位。

## 17F03 人物站位摆演入口

- 统一编辑入口：`MIN_LOOP_ROOT/ReplayRoom_17F03/StagingPreview_17F03`。这里的 `Preview_*` 是专门给编辑摆位用的副本，不是运行时演员。
- 调整流程：退出 Play Mode -> `Tools / Hearth / Replay / 17F03 Staging / Create Or Update Preview` -> 移动或旋转 `Preview_*` -> `Apply Preview Poses To Anchors` -> Play 测试。
- 当前也支持自动同步：移动或旋转 `Preview_*` 根节点后可以直接按 Play，工具会在进入 Play Mode 前把位置和朝向写入对应 Anchor。`Apply Preview Poses To Anchors` 仍可用于立即保存；若人物显示回默认坐姿或展开手姿势，执行 `Reapply Animation Poses`。
- 每幕对应关系：
  - 第一幕：`Preview_Mother_Human`、`Preview_Father_Human`。
  - 第二幕：`Preview_Mother_Midday`、`Preview_Father_Midday`、`Preview_Daughter_Midday`。
  - 第三幕：`Preview_Daughter_NightStart`、`Preview_Daughter_NightPath_01`、`Preview_Daughter_NightApproach`。
- 运行时显隐仍由 `HearthCompanion17F03ReplayController` 管理。预览层在 Play Mode 自动隐藏，不能替代 RuntimeActors。
- 不要直接改 `RuntimeActors` 的位置；正式流程会重新吸附到 `Anchors`。正常运行 `Apply 17F03 Minimal Loop Setup` 会保留已同步的 Anchor；`Rebuild 17F03 Anchors From References` 会覆盖它们，只在确实要从旧参考模型重建时使用。
- 预览安全规则：即使已经创建 `StagingPreview_17F03`，也可以正常重跑 `Apply 17F03 Minimal Loop Setup`。绑定工具会忽略预览副本并检查正式演员是否重复；若预览根节点少了可视子模型，重跑 `Create Or Update Preview` 会补回模型但不重置你手调过的预览根节点位置和朝向。

## 17F01 字幕与语音入口

位置：

- `Assets/Data/MinLoop/Dialogues/17F01_BedroomPrelude.asset`
- `Assets/Data/MinLoop/Dialogues/17F01_BedsideSoothing.asset`
- `Assets/Data/MinLoop/Dialogues/17F01_LivingRoomObservation.asset`

使用方式：

- 每个资产里有一个 `Lines` 列表。
- 每一句可以改 `Speaker`、`Text`、`Start Delay`、`Hold Seconds`、`Voice Clip`。
- `Hold Seconds` 是字幕最短停留时间。
- 如果拖入 `Voice Clip`，字幕会至少停留到该音频播放完。
- `Post Sequence Delay` 是整段字幕播完后的额外等待时间。

当前流程：

- 卧室前奏播完后，再等 `HearthCompanion17F01ReplayController / Prompt Delay After Bedroom Prelude`，才开放看向小男孩的 `E` 交互。
- 床边安抚播完后进入客厅观察。
- 客厅观察字幕播完后返回人类终端，显示 A/B 处置页。

## 后续家庭复用方式

建议每个家庭、每个场景都建立独立的 `HearthDialogueSequence`：

- `17F02_BedroomPrelude`
- `17F02_KitchenArgument`
- `17F03_AlertEntry`
- `17F03_DaughterRoom`

新的剧情流程脚本只需要：

1. 拖入对应 `HearthDialogueSequence`。
2. 调用 `MinLoopSubtitlePlayer.PlaySequenceAsset(sequence)`。
3. `yield return` 等它播完。
4. 再开放下一步交互、按钮或镜头切换。

## A/B 处置与信任度

当前规则：

- A：`+1`
- B：`-1`
- 信任度范围：`-3` 到 `3`
- 最终 A 结局阈值：`3`

入口：

- `MinLoopFlowController.ChooseDispositionA()`
- `MinLoopFlowController.ChooseDispositionB()`
- `TrustStateController.ConfigureRules(...)`
- `HearthFirstPersonHudController.RecordDisposition(...)`

现在 A/B 已经做成一次性提交：

- 同一次处置页里重复按空格不会重复改变信任度。
- 选择后终端会自动关闭。
- 历史记录只在处置成功后增加一条。

## 今日历史记录

入口：

- `HearthFirstPersonHudController.RecordDisposition(choice, currentTrustAfter, trustDelta)`
- `HearthDispositionHistoryView.AddRecord(record, currentTrustAfter)`
- `HearthDispositionHistoryView.SetRecords(records)`
- `HearthDispositionHistoryView.ClearRecords()`

规则：

- 完成一户，只显示一户记录。
- 未完成的户不提前显示。
- `Current Trust` 显示当前分数，不再显示百分制占位。
- `view archive` 已移除，后续如要做档案系统再另开页面。

## 预留但暂未真正接入

- 语音系统：先把录好的音频拖到每句字幕的 `Voice Clip`，之后如果要接 AudioMixer，可以从 `MinLoopSubtitlePlayer / Audio Source` 接入。
- 存档系统：历史记录和信任分数已有公开 setter/getter，后续存档读取后调用 `SetRecords` 和 `TrustStateController.SetTrust`。
- 17F02/17F03：建议复用 `HearthDialogueSequence + MinLoopSubtitlePlayer + 完成后开放交互` 的结构。
- VR 输入：所有关键动作已有公开方法，后续 VR 射线或按钮直接调用 A/B、字幕后交互、终端提交等方法。
- 交互 UI 主题：当前 PC 版由 `HearthCompanionHoldPrompt` 和 `PlayerInteractionPrompt` 提供；后续 VR 只需调用同一确认接口，可替换表现层而不改剧情流程。

## 17F03 实体陪伴单元检查镜头

- 入口：第一幕父母对话完成后，玩家面向 `ROBOT/InteractionVolume_17F03` 按 E，由 `HearthCompanion17F03ReplayController.OpenUnitInspection()` 执行。
- 当前表现：人类视角会在约 `0.5s` 平滑移动至 `ROBOT/Camera (1)`，不再以黑场作为正常检查入口；检查 UI 打开后，Space 继续调取记录，Esc 调用 `CancelFlow()` 平滑返回人类视角。
- 可调组件：`MIN_LOOP_ROOT/ReplayRoom_17F03/UnitInspectionCameraTransition_17F03`。可在 Inspector 修改 `Enter Duration`、`Exit Duration`、缓动曲线和是否平滑退出。
- 后备规则：若平移组件、人类 Camera 或 `ROBOT/Camera (1)` 的任一引用缺失，流程自动使用旧黑场切换，避免剧情卡死。
- 与终端的关系：该组件复用 `HearthTerminalCameraTransition` 的镜头平移能力，但只服务第三户“实体陪伴单元检查”；门口 TV 终端转场逻辑不受影响。

## 17F04 自宅终局接口

### 正式流程入口

- TV3 入口：`Hearth17F04FinaleController.BeginFromHomeTerminal()`。
- 当前 TV3 使用 `HearthTvTerminalController.PrimaryAction = Custom`，`On Custom Primary Action` 已绑定上述入口。
- TV3 使用 `Defer Custom Action Close Until External Fade = true`：Space 后保持终端固定相机，17F04 黑幕完全遮住时再调用 `CompleteCustomActionHandoff()`。若入口条件不满足，则调用 `CancelCustomActionHandoff()` 恢复终端输入。
- 状态顺序：`HomeTerminal -> LivingRoom -> Photo -> LivingRoom -> DaughterRoom -> Dialogue -> FinalChoice -> ApproachUnit/直接结局 -> Shutdown -> Epilogue -> Complete`。
- `Require Previous Households` 当前为 `false`；以后开启后由 `HearthHouseholdProgressState.AreFirstThreeCompleted` 决定是否允许进入。
- 住户完成写入：`HearthHouseholdProgressState.MarkHouseholdCompleted(residentId)`；存档读取后可重复调用，脚本会去重。

### 相框与女儿房间

- 相框入口：`HearthPhotoFrameInteractable.OpenView()`；正常由玩家看向 `TV (4)` 后按 E 调用。
- 相框完成：照片对白结束后流程调用 `NotifyDialogueComplete()`；玩家再按 Space/Esc，组件平滑返回并调用 `Hearth17F04FinaleController.CompletePhotoInspection()`。
- 房门入口：`Hearth17F04FinaleController.EnterDaughterRoom()`；正常由 `Hearth17F04RoomDoorInteractable` 在照片和客厅对白完成后开放。
- 女儿房间对白期间，正式人类控制器保持移动、视角和普通交互开启；进入 A/B 页面后才锁定。
- 第二张照片：新建一个 `HearthDialogueSequence`，复制相框交互组件并接入新的完成事件；不要把照片做回 TV 终端分页。

### 最终 A/B 与信任分支

- A：`Hearth17F04FinaleController.ChooseAnswerSelf()`。
- B：`Hearth17F04FinaleController.ChooseCompanionAnswer()`。
- 第一人称 HUD 在本流程中调用 `SetRouteFinalChoiceInternally(false)`，因此按钮只发事件，不会进入旧结局页。
- 同一轮只接受第一次选择；重复 Space 不会重复提交，也不会重复结算。
- 本次选择不改变信任度。分支只读取前三户结果：`trust > 0` 为 High，`trust < 0` 为 Low；`0` 仅用于预览，默认 High 并输出警告。
- A 路线完成对白后开放 `ROBOT (1)/InteractionVolume_17F04`；B 路线不会开放关闭交互。

### 陪伴单元关闭挑战

- 开始：`Hearth17F04FinaleController.BeginUnitShutdown()`。
- 当前挑战：`HearthVirusPopupShutdownChallenge`，场景对象为 `MIN_LOOP_ROOT/Finale_17F04/UI/ShutdownChallenge_17F04`。
- High：一个授权窗口，按一次 Space。
- Low：依次经历蓝、橙、红三轮。每轮先按 Space 关闭居中的主警告，再从四个屏幕边缘持续生成该轮浮窗；本轮生成完且清空后才进入下一轮。
- 主要节奏接口：`Low Trust Waves` 数组中每轮的 `Popup Count / Initial Burst Count / Spawn Interval / Messages / Background Color / Accent Color`，以及公共 `Wave Transition Seconds / Popup Enter Seconds / Popup Dismiss Seconds`。
- 可替换抽象接口：`HearthShutdownChallenge.BeginChallenge(bool)`、`Submit()`、`Cancel()`、`Completed`、`Cancelled`。
- 旧 `HearthSequentialShutdownChallenge` 已从第四户场景解绑；后续如替换玩法，新组件继续继承 `HearthShutdownChallenge`，终局状态机、A/B 和结局文本不需要重写。

### 对白、语音与黑幕

- 全部 17F04 文本资产位于 `Assets/Data/MinLoop/Dialogues/17F04/`。
- 每句可直接修改 `Speaker`、`Text`、`Start Delay`、`Hold Seconds` 和 `Voice Clip`。
- 场景对白统一使用全局 `MinLoopSubtitlePlayer`；黑幕结局使用 `EpilogueDialogue_17F04`。
- 四种黑幕资产：`17F04_Epilogue_High_Retain`、`High_Shutdown`、`Low_Retain`、`Low_Shutdown`。
- 语音接入方法：把录音导入 Unity 后，拖到对应 Dialogue Sequence 每句的 `Voice Clip`；字幕播放器会按音频/句子时长推进。需要混音时，在两个 `MinLoopSubtitlePlayer` 的 `Audio Source` 接入 AudioMixer Group。
- 黑幕文字位于 16:9 正中央、宽约屏幕三分之二；不要改用旧的偏下普通对白层。

### 结束与后续系统

- 正常结束：`Hearth17F04FinaleController.CompleteFinale()`，返回 `Anchor_Mia_17F04_CorridorReturn`；Anchor 缺失时恢复进入 TV3 前保存的位置。
- 完成事件：`Hearth17F04FinaleController.OnFinaleCompleted`。

### 17F04 猫咪引导与并发对白

- 猫咪控制器：`Hearth17F04CatGuideController`，场景对象为 `MIN_LOOP_ROOT/Finale_17F04/CatGuide/CatMoveRoot`。
- 开始/复位/停止：`BeginSequence()`、`ResetSequence()`、`StopSequence()`。
- 状态：`IsRunning`、`HasReachedPhoto`；事件：`OnReachedPhoto`、`OnSequenceCompleted`。
- 猫咪只是视觉引导，禁止用 `HasReachedPhoto` 作为相框或 Door1 的开放条件。
- `Route Steps` 当前编辑时长为 `1.5 / 1.5 / 1.5 / 1.5 / 7.5 / 0.5 / 0.5` 秒；`Walk Route Speed Multiplier = 3` 会在运行时只把前六个 Walk 段除以 3，6→7 的 RunJump 仍为 `0.5s`。
- `Walk_F` 使用 XZ 原地烘焙，世界位移由路线控制；`Run_F` 保持原设置并只用于最后的跳跃段。
- 路线转弯平滑度由同一组件的 `Path Smoothing` 控制，默认 `0.75`；正常只调整 `0.5-1.0`，设为 `0` 会恢复逐段直线移动。
- `Walk_F` 和 `Lie_idle` 的动画 Slot 使用 `Seamless Loop`；替换循环动画时应同时在 FBX Import Settings 开启 `Loop Time / Loop Pose`。
- 相框可用条件由 `Hearth17F04FinaleController.CanInspectPhoto` 提供；客厅渐亮后立即为真。
- 若相框在欢迎对白期间打开，控制器先完成欢迎对白，再播放 `17F04_ChristmasPhoto`。相框组件只有收到 `NotifyDialogueComplete()` 后才允许 Space/Esc 返回。

### 17F04 最终选择输入配置

- 输入配置类型：`HearthFinalChoiceInputProfile`。
- 设置/读取：`HearthFirstPersonHudInput.SetFinalChoiceInputProfile(...)`、`GetFinalChoiceInputProfile()`。
- 17F04 临时值：`Navigation Axis = Vertical`、`Allow Direct Letter Keys = false`、`Allow Return Submit = false`。
- 关卡控制器负责保存和恢复旧配置；其他关卡不要永久修改全局 HUD Input。
- 后续可在该事件上接主菜单、结局动画、存档、成就或新页面；当前只返回走廊。
- 测试重置：`ResetForPreview()`，只用于 Play Mode 快速测试，不作为正式新游戏/读档接口。
- 机位接口：`CaptureCurrentHumanCameraPivot()` 用于运行时重新读取正式玩家相机的本地枢轴；通常在 Edit Mode 调好 `First Person Camera` 后重新进入 Play 即可，不必手动调用。

## 全局对白、语音与音量接口（2026-07-16）

- 正式对白数据统一使用 `HearthDialogueSequence`；当前 34 个资产位于 `Assets/Data/MinLoop/Dialogues/`。
- 每句可自由增删和排序，字段为 `Speaker / Text / Start Delay / Hold Seconds / Voice Clip / Duration Mode / Voice Tail Seconds`。
- 推荐 `Duration Mode = VoiceClipWhenAssigned`：有录音时自动跟随真实录音长度，无录音时继续使用手动 Hold，不需要在流程控制器硬编码秒数。
- 当前 215 个 Voice Clip 槽位均未绑定真实语音。后续录好每句声音后，直接拖到对应行，不需要改脚本或关卡状态机。
- 普通对白共用一个正式 `MinLoopSubtitlePlayer`，17F04 黑幕使用一个 `EpilogueDialogue_17F04`；两者的 AudioSource 均接 Dialogue 通道。
- `MIN_LOOP_ROOT/Audio` 下 Corridor、Replay Night、Morning 三个现有 Ambience 音源已接 Ambient；其他环境声和新 SFX 在目标 AudioSource 同物体添加 `HearthAudioChannelSource`，Channel 分别选 Ambient 或 SFX。
- 人类和机器人脚步入口已分离，具体层级和字段见 `HEARTH_UI音频与对白调整入口.md`。
- 对白行数量变化后，后续 E/切幕仍以整段 Sequence 实际播放完成为准；不要额外补固定等待时间。

## 四户共享字幕样式接口（2026-07-17）

- 唯一样式资产：`Assets/Data/MinLoop/UI/Hearth_SubtitleStyle.asset`。
- `Standard Dialogue` 同时控制 17F01、17F02、17F03、17F04 普通对白的位置、宽度、字号、最小字号、最大行数和行距。
- `Centered Epilogue` 只控制第四户最终黑幕，仍位于屏幕正中央。
- 只改样式数值时无需运行菜单，重新 Play 即生效；只有新建播放器或引用丢失时运行 `Tools / Hearth / Dialogue / Apply Shared Subtitle Presentation`。
- 运行 `Validate Shared Subtitle Presentation` 可检查四户是否仍绑定到统一普通播放器，避免第三户再次误接黑幕播放器。
- 每句文字、句数、时长和语音仍在各自 `HearthDialogueSequence` 修改；共享样式只控制表现，不改剧情内容。

## 17F03 门与检查补充接口

- `Door_2_Brown (7)` 的 `SmartDoorController.Allow Direct Player Interaction = false`；玩家不可 E 开门。
- 剧情仍调用 `Open()` / `Close()`，不受直接交互开关影响。
- 门轴对象是 `DoorHinge_17F03`；只调整 `Open Local Euler Offset`、`Move Duration`，不要移动门根或给门叠加第二个旋转父级。
- 米娅检查实体机器人继续使用 `OpenUnitInspection()` / `CancelFlow()`；`UnitInspectionCameraTransition_17F03` 控制进入和退出的 0.5 秒平移。

## 分关卡剧情音效接口（2026-07-16）

- 通用播放器：`HearthSfxCuePlayer.PlayCue()`、`PlayCueOneShot()`、`StartCueLoop()`、`StopCue()`、`StopAllCues()`。
- 运行时替换素材：`AssignPrimaryClip(cueId, clip)`；Inspector 中优先直接修改 `Primary Clip / Alternate Clips`。
- 17F02 控制器自动触发：`Wife.StandUp / Wife.Walk / Dining.TableFoley / System.DataScan / System.Glitch / System.PowerOff`。
- 17F03 控制器自动触发：`Mother.StandUp / Daughter.StandUp / Daughter.Walk / Daughter.Keypad / System.Glitch / System.PowerOff`。
- 17F04 控制器自动触发：`Photo.Memory / Unit.PowerOff`。
- `Wife.SitOnBed` 是保留接口，当前不自动触发。确认女主实际坐下时刻后，接到对应 Dialogue/Animation Event 即可。
- 两个移动 Cue 的 AudioSource 位于 `MIN_LOOP_ROOT/Audio`，但通过 `Follow Target` 实时跟随人物 RuntimeRoot；不需要把音效对象手动放进人物模型。
- 门、HUD、TV、主角脚步和机器人脚步继续使用原本的专用接口，禁止再用同一个 Clip 在 StorySFX 中重复触发。
- 所有 Cue 在 Clip 为空时静默跳过，剧情不会停住，也不会输出错误。

## 17F02 动作与门同步接口（2026-07-17）

- 共享动作柔化入口：每个演员 `HearthActorAnimatorDriver / Minimum Transition Seconds`，当前为 `0.32s`；单个状态是否锁定 Animator 子模型偏移由 `State Slot / Stabilize Animator Transform` 控制。
- 17F02 女主的世界位置必须继续由 `Actor_Wife_17F02_BedroomRuntimeRoot` 和路线 Anchor 控制；Mixamo 动作只控制骨骼。
- 门提前量：`HearthCompanion17F02ReplayController / Door Open Delay After Animation Start Seconds = 0.5`。减小会更早开门，增大会更晚。
- 女主出门完成后自动调用动画基准恢复并精确对齐 `Wife Exit Outside Anchor`；若以后替换动作后再次出现闪回，先检查对应 State 的稳定选项，再检查可见模型是否仍是 RuntimeRoot 子物体。
- 第三幕中央蓝字来源为 `CompanionScene_07_17F02_04 / Center Message`，当前必须保持为空；家庭记录内容继续在 Projection Panel 数据中维护。
