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
- `Route Steps` 当前编辑时长为 `1.5 / 1.5 / 1.5 / 1.5 / 7.5 / 0.5 / 0.5` 秒；`Walk Route Speed Multiplier = 1.5` 会在运行时只把前六个 Walk 段除以 `1.5`，6→7 的 RunJump 仍为 `0.5s`。腿部动作仍只使用 `Walk Playback Speed = 2.0`，不会再乘路线倍率。
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

- 正式对白数据统一使用 `HearthDialogueSequence`；当前最终稿 `330` 个稳定字幕段同步到 `Assets/Data/MinLoop/Dialogues/` 下 `70` 个对白资产。
- 每句可自由增删和排序，字段为 `Speaker / Text / Start Delay / Hold Seconds / Voice Clip / Duration Mode / Voice Tail Seconds`。
- 推荐 `Duration Mode = VoiceClipWhenAssigned`：有录音时自动跟随真实录音长度，无录音时继续使用手动 Hold，不需要在流程控制器硬编码秒数。
- 每个拆分段都保留独立 `Voice Clip`。后续录好每句声音后，直接拖到对应行，不需要改脚本或关卡状态机。
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

## 1F 大堂对白触发旧方案（已废弃）

- 2026-07-17 的“双层 Trigger、三组全部完成才开放后续”只是早期预留方案，已被下方 2026-07-18 正式实现覆盖，后续不得再按旧方案绑定。

## 17F02 动作与门同步接口（2026-07-17）

- 共享动作柔化入口：每个演员 `HearthActorAnimatorDriver / Minimum Transition Seconds`，当前为 `0.32s`；单个状态是否锁定 Animator 子模型偏移由 `State Slot / Stabilize Animator Transform` 控制。
- 17F02 女主的世界位置必须继续由 `Actor_Wife_17F02_BedroomRuntimeRoot` 和路线 Anchor 控制；Mixamo 动作只控制骨骼。
- 门提前量：`HearthCompanion17F02ReplayController / Door Open Delay After Animation Start Seconds = 0.5`。减小会更早开门，增大会更晚。
- 女主出门完成后自动调用动画基准恢复并精确对齐 `Wife Exit Outside Anchor`；若以后替换动作后再次出现闪回，先检查对应 State 的稳定选项，再检查可见模型是否仍是 RuntimeRoot 子物体。
- 第三幕中央蓝字来源为 `CompanionScene_07_17F02_04 / Center Message`，当前必须保持为空；家庭记录内容继续在 Projection Panel 数据中维护。

## 1F 大堂、同步终端与电梯接口（2026-07-18，已实现）

### 正式流程入口

- 总控制器：`MIN_LOOP_ROOT/LobbyOpening/HearthLobbyFlowController`。
- `BeginOpening()`：把正式米娅放到 `Person Controller (4)` 锚点，锁定移动和普通交互、保留鼠标观察，并自动播放开场。
- `TryPlayOptionalConversation(...)`：玩家进入任一大堂可选对白范围后，使用短按 `1` 开始；播放时锁移动、保留鼠标视角，Field Unit/NPC 与 Mia 按语音长度自动接续，不需要 Space。
- `TryPlayExitCommentary(...)`：NPC 对话完成并离开 Trigger 后自动播放 Mia 感想；播放时不锁移动和视角，字幕按语音长度自动结束。
- `AcquireAssignmentFromTerminal()`：在同步终端按 Space 后领取任务。成功领取后终端本轮永久不可再次打开；终端关闭后的任务说明属于固定剧情，移动、视角与 E 全部保持锁定，直到 Field Unit/Mia 全部讲完才开放电梯。
- `BeginElevatorRide()`：渐黑进入 `Person Controller (5)` 电梯锚点，播放对白，再抵达 17 楼。
- `ResetLobbyFlowForPreview()`：清空本轮状态，供 Play Mode 反复测试。
- 事件接口：`onOpeningCompleted`、`onAssignmentLoaded`、`onElevatorEntered`、`onFloor17Arrived`。

### 三个可选对白范围

- 触发脚本为 `HearthLobbyConversationZone`，挂在 `space / space1 / space2` 的 Trigger Collider 上。
- 每个范围包含 `Exchange Sequence` 与 `Exit Commentary Sequence`：前者在区域内播放 NPC 对话，后者在对话完成且玩家离开区域后播放 Mia 感想。
- 范围内提示由 `PlayerInteraction.SetProximityInteraction(...)` 显示为 `1  TALK`；只响应 `KeyDown Alpha1`，不是长按。`HearthCompanionHoldPrompt` 中的固定剧情操作仍保留长按 `1`，两类输入不能混用。
- 不要求三组全部完成，也不决定终端是否可用。若上一段感想仍在播放，下一组 NPC 对话会等待共享字幕播放器空闲。
- 调整范围只需移动或缩放对应 Collider。NPC 位置、朝向和动作不由该脚本修改。
- `Play Once` 默认开启；需要重新测试时调用 `ResetConversation()` 或重置整个 Lobby Flow。

### 同步终端与电梯

- 终端交互脚本：`HearthLobbyTaskTerminalInteractable`，复用 `HearthTvTerminalController` 的蓝色 E 提示、0.5 秒镜头平移、开机闪烁和 Space 主操作。
- 任务终端启用 `Hide Canvas When Closed`：关闭时整个 World Space Canvas 不渲染；E 打开时才启用。`TaskTerminalScreenAnchor` 控制 Canvas 与实体屏幕的相对位置、朝向和大小，Lobby Apply 不覆盖终端 Camera。
- 一次性规则：成功按 Space 后 `AssignmentLoaded = true`，`CanOpenAssignmentTerminal` 永久返回 `false`，终端不再显示 E。若玩家在尚未确认任务时误按 Esc，仍允许重新打开，避免本轮流程软锁。
- 任务说明对白期间：米娅 Movement/Look、`PlayerInteraction`、Tab 与其他辅助输入全部锁定；对白结束后一次性恢复并把 Current Task 更新为 `GO TO THE ELEVATOR`。
- 电梯交互脚本：`HearthLobbyElevatorInteractable`，实现 `IInteractionAvailability`；只有 `AssignmentLoaded = true`、任务说明播放完毕且流程不忙时才显示 E。
- 一楼终端 Prefab：`Assets/Prefabs/UI/HearthHud/Terminals/Terminal_Lobby_Assignment.prefab`。
- 调整 `Person Controller (4)/(5)` 后，运行时会直接读取其位置与摄像机朝向；不需要重新执行绑定菜单。
- 修改正式 17 楼到达点时，先在 Edit Mode 把正式玩家摆到目标位置，再执行 `Tools > Hearth > Lobby > Capture Current Player Pose As 17F Arrival`。

## 最终对白稿与语音接口（2026-07-18）

### 唯一文本来源

- 项目根目录 `HEARTH_Full_Game_Script_No_Audio_Tags_Native_English.md` 是当前正式游戏对白唯一来源；`HEARTH_Full_Game_Script_ElevenLabs_v3_Native_English.md` 只用于未来 ElevenLabs 配音时查询情绪提示。
- 同步菜单：`Tools > Hearth > Dialogue > Sync All Dialogue From Final Script`。
- 自然拆分菜单：`Tools > Hearth > Dialogue > Normalize Final Script To Two-Line Segments`。
- 覆盖检查：`Tools > Hearth > Dialogue > Validate Final Script Coverage`。
- 布局检查：`Tools > Hearth > Dialogue > Validate Two-Line Subtitle Layout`。
- 当前检查覆盖正式稿中的 `330` 个字幕段；同步范围包含一楼开场和四户流程，并验证普通/黑幕字幕均不超过两行。
- 最终稿使用 `<!-- HEARTH:SEQUENCES ... -->` 隐藏稳定标记绑定资产；不要删除这些标记，否则同步工具会报告覆盖缺口。

### 每句对白的可调字段

- `Speaker`：说话人名称。
- `Text`：字幕正文。
- `Start Delay Seconds`：本句开始前等待。
- `Hold Seconds`：没有语音时的回退显示时长。
- `Voice Clip`：本句真实语音。
- `Duration Mode = Voice Clip When Assigned`：有语音时自动使用 `AudioClip.length`，无语音时使用 `Hold Seconds`。
- `Voice Tail Seconds`：语音结束后字幕额外保留时间，当前同步默认 `0.12s`。

### 语音接入步骤

1. 在 `Assets/Data/MinLoop/Dialogues/` 找到对应关卡和场景的 `HearthDialogueSequence`。
2. 展开 `Lines`，把每句对应的 `AudioClip` 拖入 `Voice Clip`。
3. 保持 `Duration Mode` 为 `Voice Clip When Assigned`，按需要调整 `Start Delay` 和 `Voice Tail`。
4. 文字有修改时先修改最终 Markdown，再运行同步菜单；同步只自动保留说话人与正文完全相同的语音绑定。
5. 运行覆盖检查并试听完整场景，确认字幕、语音和下一句的间隔。

### 补充数据接口

- `Assets/Data/MinLoop/Dialogues/FinalScriptSupplemental/` 保存最终稿中暂时没有独立运行节点的终端说明、分支签退和检查提示。
- 这些对白已经进入正式数据层，但不会自动插入不匹配的旧剧情节点。后续新增对应终端按钮或分支节点时，直接把相应 Asset 拖入控制器，无需再次抄写文本。

## 三户处置对白与完成状态接口（2026-07-18）

### 17F01 / 17F02 门口终端

- `MinLoopFlowController.BeginDispositionBriefing(residentId)`：回放结束后进入处置推荐；页面先显示但锁住输入。
- `HearthResidentDispositionDialogueSet`：每户配置推荐、A 评价、B 评价和可选公共指引。
- `HearthTvTerminalController.SetChoiceInputEnabled(bool)`：推荐结束前为 `false`，结束后为 `true`。
- `MinLoopFlowController.SubmitDisposition(choice)`：只接受第一次提交，返回是否成功；信任度、HUD 历史和完成事件不会因重复 Space 再次触发。
- 评价与下一户指引播完后，流程才关闭终端并调用 `HearthHouseholdProgressState.MarkHouseholdCompleted(residentId)`。

### 17F03 房内处置

- 检查提示期间提前确认：`Hearth17F03InspectionPanel.QueueRecallRequest()`；状态通过 `RecallQueued` 查询。
- 回放后再次 E：`HearthCompanion17F03ReplayController.OpenUnitInspection()`，平滑进入实体机器人固定摄像机并打开房内 A/B。
- A/B：`OpenDispositionChoice()`、`MoveChoice(int)`、`SubmitChoice()`；正式输入为 `↑/↓ + Space`。
- 共用信任结算：`MinLoopFlowController.BeginExternalDispositionChoice("17F03")`、`SubmitDisposition(choice)`、`CompleteExternalDisposition()`。
- 17F03 的 `onHouseholdCompleted` 仍可接任务、存档或成就；正式住户进度同时写入 `HearthHouseholdProgressState`，重复写入会去重。

## 任务终端与全游戏音频接口（2026-07-19）

### 一楼任务终端维护

- 编辑定位：运行 `Tools > Hearth > Lobby > Show And Align Task Terminal Canvas For Editing`，Hierarchy 会选中 `1F (1)/TvUnitSet5/MonitorCanvas`。
- 可以直接调整 `MonitorCanvas` 的 Position、Rotation、Scale；调整满意后运行 `Capture Task Terminal Canvas Placement` 保存到 `TaskTerminalScreenAnchor`。
- 正式运行时终端关闭状态会禁用 Canvas，只有 E 打开时显示；终端 Camera 的 Transform 和 FOV 不被上述两个菜单修改。
- 电流/沙沙声接口：`MIN_LOOP_ROOT/Audio/StorySFX_Lobby/HearthSfxCuePlayer` 的 `AssignmentTerminal.Hum / Primary Clip`。

### 全局音效与语音

- 六组 Story SFX 共 45 个槽：Global、Lobby、17F01、17F02、17F03、17F04。
- 人类脚步：`Player/Person Controller/First Person Audio/HearthFootstepAudioProfile`。
- 机器人脚步：`Player/Robot Controller/Robot First Person Audio/HearthFootstepAudioProfile`。
- 每台终端仍可在自己的 `HearthTvTerminalController` 中替换开关机、翻页、移动焦点、提交和视角切换声音。
- 每句对白语音继续绑定到 `HearthDialogueSequence / Lines / Voice Clip`，显示时长跟随该 Clip；无 Clip 才使用 Hold Seconds。
- 完整清单和 Inspector 入口见 `HEARTH_音频资源需求与素材来源清单.md / 第 11 节`。

### 本次未改变的剧情逻辑

- Lily 消息仍在 Mia 的 `Okay.` 完成后关闭。
- 本次场景/Assets 整理没有更改关卡顺序、人物位置、相机位置、触发区或 A/B 结算。

## 一楼任务终端一次性状态与正式稿校准（2026-07-19）

- `HearthLobbyFlowController.CanOpenAssignmentTerminal` 现在同时检查 `AssignmentLoaded = false`。成功领取任务并关闭终端后，本轮不再出现终端 E 提示，也不能重新打开。
- `AssignmentLoadedRoutine()` 在终端关闭后以“可移动 + 可转头 + 不可交互”状态播放任务说明。说明期间 `busy = true`，所以电梯仍保持锁定；最后一句结束后恢复普通交互并开放电梯。
- 正式稿新增 `Current playable-flow authority`，并同步校准 Lily 留言生命周期、大厅可选对白、任务终端、电梯、17F01/02 门口终端、17F03 房内处置和 17F04 固定终端入户等已经落地的流程。

## 全局终端提示、住户简介与 Time Card 接口（2026-07-19）

### 通用终端门控

- `HearthTvTerminalController.SetPrimaryActionInputEnabled(bool)`：只锁定 Space 主操作，Tab 浏览仍可使用。
- `SetCloseInputEnabled(bool)`：单独锁定 Esc，供一楼任务终端前 5 秒使用。
- `SetRuntimePrompt(string)` / `ClearRuntimePrompt()`：显示或清除终端上方安全区提示；锁定提示统一使用 ASCII `PLEASE WAIT`。
- `OnOpened` / `OnClosed`：供简介、音效和外部剧情订阅，不需要轮询终端状态。
- `SetChoiceInputEnabled(bool)` 仍负责整组选择输入；与 Primary Action 门控用途不同，不要混用。

### 一楼任务终端

- `HearthLobbyFlowController.BeginAssignmentBriefingFromTerminal()`：终端开机完成后开始 Field Unit 简报并启动 5 秒门槛。
- `ConfirmAssignmentTerminalClose()`：5 秒后接受 Space，关闭页面但不停止简报。
- `Assignment Terminal Minimum View Seconds`：Inspector 可调最短阅读时间，默认 `5`。
- `CanOpenAssignmentTerminal`：成功领取后永久返回 false；`CanUseElevator` 还会等待整段简报完成。
- 终端 Camera、`TaskTerminalScreenAnchor` 和 `MonitorCanvas` 为用户维护数据，Binder 只验证差异，不自动覆盖。

### 三户终端简介

- `HearthTerminalOpeningBriefing.BeginBriefing()`：打开当前终端后播放本户简介并锁主操作。
- `CancelBriefing()`：Esc 提前退出时停止本次简介；下一次打开会从头开始。
- `ResetBriefing()`：预览或重开测试时清除“本轮已完成”状态。
- 三户默认资产：`17F01_TerminalIntro`、`17F02_TerminalIntro`、`17F03_CorridorTerminal`。
- 自动绑定菜单：`Tools > Hearth > Terminals > Apply Household Opening Briefings`。

### 统一机器人 HUD 布局

- 配置资产：`Assets/Data/HearthHud/Companion/Hearth_CompanionHudLayout.asset`。
- `HearthCompanionHudLayoutController.ApplySharedLayout()`：立即把共享缩放、文字缩放和偏移应用到右上决策区与左下数据流。
- 资产保存后编辑模式实时预览；剧情脚本只更新内容，不改布局参数。

### 字幕与结局时间卡

- `MinLoopSubtitleLine.Presentation Kind`：`Dialogue` 或 `TimeCard`。
- 正式稿使用 `**TIME CARD:** "..."`，`HearthFinalDialogueSync` 会自动同步为 TimeCard 并清空说话人栏。
- 共享配置入口：`Hearth_SubtitleStyle.asset / Time Card`；独立控制宽度、位置、字号和淡入淡出。
- `HearthPhotoFrameInteractable.SetExitHint(...)`：绑定相框对白完成后的 `SPACE  RETURN` 提示；Esc 保留为隐藏的安全退出。

## 2026-07-21 新定稿与新增流程接口

### 正式稿和配音稿

- 游戏字幕同步源：`HEARTH_Full_Game_Script_No_Audio_Tags_Native_English.md`。
- ElevenLabs 表演参考：`HEARTH_Full_Game_Script_ElevenLabs_v3_Native_English.md`；不能直接替代无标签字幕稿。
- 当前同步基线：`330` 个字幕段、`70` 个 Dialogue Asset、`394` 个映射条目；Coverage 与 Two-Line 验证均通过。
- 当前游戏从一楼大厅开始，不调用宣传片或 `Prologue_HUDPromo`。

### 大厅和 17 楼抵达

- `HearthLobbyFlowController / Floor17 Arrival Dialogue` 绑定 `17F01_CorridorArrival.asset`。
- 开场期间调用控制锁时使用“移动 false、视角 true、交互 false”；`Okay.` 完成后恢复。
- 三个 `HearthLobbyConversationZone` 使用 `playOnce = true`，不参与任务终端解锁条件。
- 任务终端当前 Canvas 的手调结果已通过 `Capture Task Terminal Canvas Placement` 保存到 `TaskTerminalScreenAnchor`。

### 17F01 / 17F02

- 玩法、回放状态机和人物走位保持原实现。
- 开场简介资产改为 `17F01_TerminalIntro`、`17F02_TerminalIntro`；处置推荐与签退仍由 `MinLoopFlowController.BeginDispositionBriefing/SubmitDisposition` 门控。
- 以后只换对白时运行 Dialogue Sync，不运行 17F01/17F02 场景重建工具。

### 17F03 新序列槽

- `Terminal Entry Sequence = 17F03_TerminalEntry`
- `Post Replay Question Sequence = 17F03_PostReplayQuestion`
- `Post Replay Explanation Sequence = 17F03_PostReplayExplanation`
- `Corridor Evaluation A/B Sequence = 17F03_CorridorEvaluation_A/B`
- `Post Replay Positive Trust Result Sequence = 17F03_PositiveTrustShiftResult`
- 负信任与完成通知继续使用 `17F03_NegativeTrustSupervisorWarning`、`17F03_AllInspectionsComplete`。
- 回放返回房内时只播放 `Post Replay Question Sequence`；玩家再次按 E 进入固定检查视角后，调用 `Hearth17F03InspectionPanel.OpenDispositionChoice(false)`，播放 `Post Replay Explanation Sequence`，最后调用 `SetChoiceInputEnabled(true)`。
- 这些槽都由 `Apply 17F03 Minimal Loop Setup` 自动绑定；公开运行入口保持 `BeginHumanEntry/OpenUnitInspection/BeginRecordedReplay/CancelFlow`。

### 17F04 两张照片与最终选择

- `HearthPhotoFrameInteractable.ConfigurePhotoPages(renderer, first, second)`：配置同一电子屏的两张图片。
- `Hearth17F04FinaleController.RequestPhotoPage(index)`：首次显示第二页时播放 `17F04_SecondPhoto`；两页完成后播放 `17F04_PhotoCompletion`。
- 两页说明完成后仍允许左右浏览；提示合并为 `LEFT / RIGHT  SWITCH PHOTO     SPACE  RETURN`。重复翻页不会重播说明。
- 第二张图片固定预留路径 `Assets/Art/UI/HearthHud/Finale/FamilyPhoto_Second.png`。缺失时 `HasSecondPhoto = false`，左右翻页与第二页对白都不会开放。
- `Final Choice Advisory Sequence = 17F04_FinalChoiceAdvisory`：播放完成后才打开最终选择。
- 让陪伴单元代答后按信任正负分别播放 `17F04_CompanionAnswer_PositiveRating/NegativeRating`。
- `HearthVirusPopupShutdownChallenge.ApplyDefaultWaveContentPreservingTuning()` 仅刷新三波默认文案/颜色；保留三阶段弹窗玩法和 Inspector 中的速度、数量、遮罩、音效参数。

### 验收与后续素材

- 新增第二张照片后，只需放入固定路径并运行 17F04 Apply；不要手动复制第二套相框或另建终端。
- 新增语音后，把 Clip 拖到对应 Dialogue Asset 的独立行；无需改状态机。正文变更仍先改无标签正式稿，再同步并重新检查变化行的语音引用。
- Play Mode 当前已通过启动冒烟测试；已知旧问题仅为 ROOM2 书桌与 ROOM3 电视柜的负缩放 BoxCollider，不属于本轮流程接口。

## 三户共享长按提示维护入口

- 正式提示对象：`HearthCompanionHudRoot/InteractionLayer/HoldPrompt`。
- 固定阶段显示：调用 `HearthCompanionHudController.ShowCurrentHoldPrompt()`；适用于剧情已经允许操作、无需持续瞄准目标的阶段。
- 准星条件显示：每帧把目标判定结果传给 `HearthCompanionHudController.SetHoldPromptVisible(canInteract)`；适用于 17F01 小男孩和 17F03 母亲/女儿。
- 提示文案、时长和是否显示继续由 `HearthCompanionHudSceneData` 的当前页面数据控制。
- 17F01 目标条件继续由 `HearthCompanionReplayInteractable.CanInteract(actor, camera)` 提供；17F03 继续由当前剧情目标和中心射线共同判定。
- 引用丢失或重建 HUD 后运行 `Tools > Hearth > HUD > Repair Companion Hold Interactions`，再运行同目录下的 Validate。
- 正式 Prefab 必须保持 `HearthCompanionHudPreviewInput.previewInputEnabled = false`，避免预览输入改变当前剧情页。
- 机器人 HUD、`HearthCompanionHudFlowBinder`、`HearthCompanionHudExclusiveMode` 与三户 Replay Controller 必须统一引用 `MIN_LOOP_ROOT/FlowManagers/ViewSwitchController`。若绑定到 1F 参考物体上的停用控制器，长按逻辑仍可能执行，但整个蓝色提示层会被错误隐藏。
- 运行时可通过 `ViewSwitchController.FindPreferredController()` 取得正式控制器；Editor Repair 菜单会同步修复全部上述引用。

## HEARTH UI V2 与终端独占显示接口

- 主题生成与切换统一由 `Tools > Hearth > UI V2` 管理；Legacy 和 V2 Prefab 不互相覆盖。
- 终端默认调用 `HearthTvTerminalController.SetHideFirstPersonUiWhileOpen(true)`。打开时隐藏人类 HUD、陪伴单元 HUD 和大厅叙事 HUD，关闭或取消时恢复进入前状态。
- 新增固定视角、相框或特殊终端时，应通过 `ViewSwitchController.CurrentViewCamera`、`CurrentInteraction` 和 `CurrentViewRoot` 获取当前真实玩家 Rig，不能从场景中按名字寻找 Camera。
- 剧情需要临时强制保留某个额外 Screen Space UI 时，可把该对象从终端自动发现命名中移除；需要额外隐藏时，把根对象加入终端的 `First Person Ui Roots To Hide`。
- 陪伴单元共享布局仍由 `Hearth_CompanionHudLayout.asset` 控制；V2 Prefab 切换后由 `RecaptureBaselines()` 自动登记新版位置。
- UI 主题切换不改变 Dialogue Asset、信任度、回放阶段或交互条件；剧情脚本引用由生成器自动重映射。

## 最终配音逐句绑定接口（2026-08-01）

- 游戏运行字幕仍以 `HEARTH_Full_Game_Script_No_Audio_Tags_Native_English.md` 为唯一同步入口；
  项目内 `HEARTH_ElevenLabs_v3_API_Voice_Lines_Manual_Revision.jsonl` 是本轮配音定稿快照，
  先通过 `Tools/Audio/sync_final_voice_subtitles.py` 更新正式 Markdown。
- 每条正式对白使用 `HEARTH:VOICE <line_id>` 和 `MinLoopSubtitleLine.lineId` 贯通
  Markdown、Dialogue Asset、`<line_id>.mp3`；后续禁止再用数组下标猜测语音归属。
- 批量入口：`Tools > Hearth > Dialogue > Import Final Voice Collection And Bind Subtitles`；
  校验入口：同目录 `Validate Final Voice Bindings`。当前基线为 330 个正文 Clip、394 个分支条目。
- `HearthFinalDialogueSync` 重跑时优先按 Line ID 保留 Clip；带 Line ID 的句子不得自动拆成多个字幕条，
  因为一个 MP3 只对应一个完整句子。
- `Prologue_HEARTHCommercial` 与 `Lobby_OpeningBriefing_FieldUnit_002` 当前排除；启动流程仍直接进入
  一楼大厅。若以后恢复宣传片，必须同时恢复这 8 条内容并重新检查大厅开场语境，不能只接回视频。
- 17F04 照片语音的源序列虽统一为 `17F04_ChristmasPhoto`，运行时仍分别绑定到
  `17F04_ChristmasPhoto / 17F04_SecondPhoto / 17F04_PhotoCompletion`，不要合并状态机。
- 大厅自动开场会在其他场景组件完成 `Start()` 重置后的下一帧取得全局字幕播放器；其他系统若要
  中断正式对白必须调用播放器 `Stop/Hide`，播放器会用内部代次令旧外部协程退出，不能直接停
  AudioSource 或只隐藏 VisualRoot。

## V2 逐行对白与世界空间 Surface 接口（2026-08-01 最新）

### 逐行播放权威

- `MinLoopSubtitleLine.lineId`：正式稿、Dialogue Asset、AudioClip 和播放策略的稳定主键。
- `HearthDialoguePlaybackPolicy`：只登记少量自动 Mia、17F04 黑幕尾声和专用 Lily 留言；新增对白默认 ManualSpace。
- `MinLoopSubtitlePlayer.PlaySequenceAsset(sequence)`：普通全局 V2 对白。
- `PlaySequenceAsset(sequence, HearthDialoguePlaybackContext.Embedded(surface, context))`：普通终端/TV 下方框。
- `Embedded(framedSurface, messageSurface, context)`：同一序列逐行在专用留言卡、普通终端框和全局 NaturalCaption 之间切换。
- `HearthDialogueSurface` 只接收 `Show/HideImmediate`；音频、Space、时长和控制锁仍由唯一播放器负责，禁止 Surface 自行读键。

### 正式同步与校验

- `HearthFinalDialogueSync` 按 lineId 保留 `VoiceClip`、PresentationKind、DialogueMode、SpeakerSide、AdvancePolicy、StartDelay、HoldSeconds、DurationMode 和 VoiceTail。
- 菜单顺序：`Sync All Dialogue From Final Script` → `Validate Final Script Coverage` → `Validate V2 Playback Policy`。
- 运行时策略会即时纠正已登记行；资产同步后同一分类也会写回 Dialogue Asset，后续 Sync 不再覆盖。

### 终端与 TV4

- `HearthTvTerminalController.ResolveDialogueSurface()`：复用/补建 `FieldUnitPanel`。
- `ResolveMessageSurface()`：复用/补建 `LilyMessagePanel`；只供 `DedicatedMessage` 行。
- `SetPrimaryActionInputEnabled(false)` 与 `SetCloseInputEnabled(false)`：对白期间让渡 Space/关闭输入。
- 一楼任务终端：`BeginAssignmentBriefingFromTerminal()` 在页面开启时播放；`ConfirmAssignmentTerminalClose()` 只在对白结束、Space 松开和门槛满足后生效。
- `HearthPhotoFrameInteractable.ResolveDialogueSurface()`：取得 TV4 下 `HearthPhotoArchiveWorldView` 的下方框。
- `HearthPhotoArchiveWorldView.Show/Hide/SetPage/SetHint`：只控制 TV4 世界空间 Chrome；实体 Renderer 继续负责照片，不需要 RenderTexture。

### 交互与 Current Task

- `PlayerInteraction.SetProximityInteraction(owner, interactable)` / `ClearProximityInteraction(owner)`：大厅触发区把短按 E 的唯一所有权交给玩家交互器。
- `HearthCompanionHudController.SetHoldPromptVisible(true)`：显示 Hold E 时暂停 Robot 短按交互；隐藏、完成或 Disable 后恢复进入前状态。
- `HearthCurrentTaskRouter.ApplyHuman/ApplyCompanion`：只写目标正文，不写按键。
- `ResolveMinLoopTask(stage, residentId)`：住户主流程任务；`ResolveCompanionSceneTask(sceneId, fallback)`：陪伴视角任务；`ResolveHouseholdCompletionTask`：跨住户目标。

### 17F04 任务节点

- `BeginFromHomeTerminal`：ReviewLilyMessage，并用 Home Terminal 的 framed/message 两个 Surface 播放。
- `BeginPhotoInspection/RequestPhotoPage/CompletePhotoInspection`：InspectPhotoArchive → GoToLilyRoom；照片对白使用 TV4 Surface。
- `EnterDaughterRoom`：TalkToLily；对话结束进入 MakeFinalResponse。
- `ChooseAnswerSelf`：结束后 ApproachHomeUnit；`BeginUnitShutdown`：ConfirmShutdown。
- `EpilogueRoutine`：先清空 Current Task，再完全淡黑，随后才用 NaturalCaption + AudioComplete 播放结尾。

### UI 修复工具约束

- V2 Builder、Closure、Final Visual Repair 必须同步维护选择色块、关机确认、STATUS CHANGE、Field Unit 9-slice 和 TV4 世界空间入口。
- 修复工具只可补缺失组件/引用；已有 Camera Anchor、Photo Camera、Renderer、TV 和终端 Transform 一律视为用户数据，不得写回默认坐标。
- `PhotoExitHintCanvas`、`PhotoCameraFeed_V2`、Slide07/08 相册运行路径、旧 A/B 左线和高信任 Esc Cancel 均为退役接口，不得重新生成或绑定。

## V2 最终落盘约束与校验状态（2026-08-02）

- 终端 Surface 的有效作用域是“当前打开页面”，禁止从整个终端根跨页抓取隐藏的 `HearthDialogueSurface`；生成名固定为 `TerminalDialogueSurface_V2` / `TerminalMessageSurface_V2`。
- TV4 世界空间 UI 必须对父层 `lossyScale` 做反向补偿；外观尺寸变化只改 `HearthPhotoArchiveWorldView` 的画布参数，不能重置用户 TV4、照片 Renderer、Photo Camera 或过渡锚点。
- `HearthDialoguePlaybackPolicy` 与 Dialogue Asset 当前一致：98 份自动记录、两份专用 Lily 留言记录；Sync 后必须继续得到同一结果。394 份正式分支行均有非空 `lineId` 与 AudioClip。
- 场景生产输入残留扫描只允许命中带构建保护的 `HearthHudPreviewInput` 和未引用第三方示例；正式脚本、Prefab、场景不得重新出现 Alpha1、Keypad1、`interactionKey: 49` 或 `useNumberOneForStoryHolds`。
- 当前离线验证通过两套程序集编译、配音 330/330 对齐审计及目标差异检查。Unity MCP 编辑器心跳尚未恢复，菜单内 Sync/Coverage/Policy 与 Play Mode 画面验收必须在编辑器重启后补跑。

## 正式音效维护与剧情接入接口（2026-08-02）

### 三层稳定主键

- `HearthSfxCatalog.SoundEntry.soundId`：素材层稳定键，例如 `AMB.Lobby.Walla`、`UI.HoldProgress`、`SYS.PowerOff`。
- `HearthSfxCuePlayer.CueSlot.cueId`：剧情层稳定键，例如 `Lobby.Walla`、`BlackAudio.Fridge`、`Epilogue.PathA.Keys`；多个 Cue 可复用一个 Sound ID。
- `HearthDialogueSfxTrack.CueAction`：精确时间层，使用 `sequenceId + lineId + action + cueId`，禁止用对白数组下标或文本模糊匹配。

### 运行接口

- 一次性：`HearthSfxCuePlayer.PlayCue/PlayCueOneShot(cueId)`。
- 循环：`StartCueLoop(cueId)`；阶段结束 `StopCue(cueId)`；关卡取消或 Disable 调 `StopAllCues()`。
- 局部覆盖：Cue 的 `Primary Clip / Alternate Clips` 优先于 Catalog；`SetCatalog()` 可替换整套目录。
- 非破坏性片段：Cue 的 `Play From Seconds / Play Duration Seconds`；门使用 `SmartDoorController` 的 Open/Close Start/Duration。
- 对白 Duck：`HearthAudioChannelSource.ConfigureDialogueDucking()`；全局状态来自 `MinLoopSubtitlePlayer.AnyDialoguePlaying`。
- 逐行事件：`MinLoopSubtitlePlayer.LineStarted`、`SequenceCompleted`；新逐句 Foley 优先挂 `HearthDialogueSfxTrack`，不要改字幕协程。

### 编辑器入口与落盘对象

- 应用：`Tools > Hearth > Audio > Apply Production Story SFX Setup`。
- 校验：`Tools > Hearth > Audio > Validate Production Story SFX Setup`。
- 中央资产：`Assets/Audio/HEARTH/HearthSfxCatalog.asset`。
- 场景播放器：`MIN_LOOP_ROOT/Audio/StorySFX_Global / Lobby / 17F01 / 17F02 / 17F03 / 17F04`。
- 工具会绑定终端、Human HUD、Companion HUD、Hold E、FirstPersonAudio、SmartDoor 和 17F04 Epilogue Track，但不会移动 UI、相机、TV、演员或锚点。

### 混音与扩展约束

- Dialogue/Ambient/SFX 继续由 `HearthAudioSettingsController` 控制；环境层允许 Duck，UI 和关键 Foley 默认不 Duck。
- 大厅 Walla 必须局部 3D、极低音量且不可听清具体句子；不得加入脚步、餐具、门、笑声或广播。
- 陪伴脚步只用轻型履带/伺服质感；人类脚步和机器人移动声不得互换。
- 无效操作与门锁拒绝保持静默；新增失败反馈前必须有新的体验要求。
- 新场景顺序：Catalog 加 Sound ID → StorySFX 加 Cue → 状态机调用；只有需要逐句时才加 Dialogue Track。

## V2 Profile 与黑幕 TimeCard 接口（2026-08-03）

### 唯一视觉配置入口

- `HearthUiThemeProfile`：终端对白、Companion 顶部、E/Hold E、场景卡、黑幕对白和全屏遮罩。
- `HearthUiLayoutProfile.GetRegion()`：门口终端、相册、Companion 顶部、全屏遮罩和结局场景卡 Rect。
- `HearthTvTerminalController.SetUiProfiles()`、`HearthPhotoArchiveWorldView.SetUiProfiles()`、`MinLoopSubtitlePlayer.SetUiProfiles()`：运行时注入接口。
- Editor 统一入口：`Tools > Hearth > UI V2 > Apply Current Profiles`。禁止另一个修复工具重新写一套常量。

### Companion 临时对白独占

- `HearthCompanionHudController` 保留兼容 `SetCurrentTask()`，并分别绑定 Identity Heading/Value 与 Task Heading/Body。
- 正式 Synth Voice、Field Unit、Home Unit 进入时打开临时独占；`PersistentInfoLayer/DecisionPanel` 在该行期间隐藏。
- 行结束后只恢复进入前确实可见的面板状态；预览或状态机不得无条件强制打开旧 DecisionPanel。

### 终端与 TV4 互斥 Surface

- `HearthDialogueSurface.SetExclusivePeer()` 保证 Lily Message 与 Field Unit Surface 互斥。
- Home Terminal 的 Lily 位于中央内容区，Field Unit 位于 `TerminalMessageLane`；门口终端 Field Unit 位于 `DoorwayFieldUnit`。
- TV4 使用 `PhotoArchiveFieldUnit` 和 `PhotoArchivePage`；不创建屏幕空间相册副本，也不移动用户 Photo Camera。

### TimeCard 数据与播放

- 正式稿标记：`<!-- HEARTH:TIME_CARD stable_id | ENGLISH TEXT -->`。
- Sync 写入现有 `TimeCard` Presentation，`VoiceClip=null`、`AudioComplete`、回退 1.5 秒；普通对白 Line ID 不变。
- 播放器先使用 `EpilogueSceneCard` 居中显示，再切换到 `EpilogueSceneHeader`；标题保留到下一张卡或序列结束。
- 黑幕对白使用 `CenteredEpilogue`，有 Clip 取 `AudioClip.length`，无 Clip 取资产回退时长，不接收 Space。
- 排序固定为 Blackout Canvas 9000、结局字幕/场景卡 9100 以上。

### E/Hold E 稳定路径

- Human 与 Companion 都保留 `PlayerInteractionPrompt`、`HoldPrompt`、`HoldPromptText`、`HoldProgressFill`。
- `PlayerInteraction` 独占短按 E；`HearthCompanionHoldPrompt` 独占持续操作并发出开始/取消/完成及 SFX。
- `SmartDoorController.allowDirectPlayerInteraction=false` 只关闭门的玩家切换入口；剧情 `Open()`、`Close()` 和门事件仍是正式接口。
