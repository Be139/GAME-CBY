# 2026-07-02 回放切入闪到 17F01 的修复记录

## 2026-07-07 机器人参考控制器怎么调

- 正式运行时只控制 `Player/Robot Controller`。
- `Player/Robot Controller (2)` 和 `Player/Robot Controller (3)` 不是第二、第三个正式机器人，而是 17F02 的可视化参考体：
  - `(2)` 代表卧室唤醒阶段的机器人出生点和朝向。
  - `(2)` 子物体里的 `Robot First Person Camera` 会同步到 `Anchor_Robot_17F02_BedroomStartCamera`，用于卧室开场的实际视角。
  - `(3)` 代表第三幕男主调用记录时的机器人站位；它子物体里的 `Robot First Person Camera` 位置/朝向会同步到 `Anchor_Robot_17F02_LivingRoomTerminalCamera`。
- 现在绑定工具不会再把 `(2)/(3)` 设为 inactive。它们在编辑器里可见、可拖动、可旋转，方便你调整胶囊体和相机。
- 进入 Play Mode 时，`HearthEditorOnlyReferenceModel` 会让这两个参考体隐藏、无碰撞、无控制脚本，所以玩家不会看到额外机器人，也不会被它们挡住。
- 调整方法：
  1. 在 Hierarchy 里选中 `Player/Robot Controller (2)` 或 `Player/Robot Controller (3)`。
  2. 直接移动/旋转它们，必要时展开子物体调 `Robot First Person Camera`。
  3. 运行 `Tools / Hearth / Replay / Apply 17F02 Minimal Loop Setup`。
  4. 程序会把位置和 Camera 视角同步到 `MIN_LOOP_ROOT/Anchors/Anchor_Robot_17F02_*`。
- 不建议直接拖 `Player/Robot Controller` 来调第二户点位，因为它是正式运行时角色，会在 17F01/17F02/17F03 之间被剧情流程复用。

- 问题原因：17F02 终端传入的住户编号是正确的，但总流程原来是先执行 `ViewSwitchController.SwitchToCompanion()`，等相机切到正式机器人后，才启动 `HearthCompanion17F02ReplayController.BeginReplay()`。如果正式 `Player/Robot Controller` 上一次停在 17F01 或默认位置，玩家就会先看到一小段第一户视角。
- 当前修复：`MinLoopFlowController` 现在会在切相机前调用当前住户回放控制器的 `PrepareReplayStart()`。17F02 会先把正式机器人放到 `Anchor_Robot_17F02_BedroomStart`，并把 `Hearth17F02ReplayBlackout` 盖到 1，再开始视角切换。
- 你不需要额外操作：继续从 `17F/ROOM2/TV (4)` 的终端进入回放即可。
- 后续如果做 17F03：建议也让 17F03 专用回放控制器提供 `PrepareReplayStart()`，在里面完成“传送到本户起点 + 黑屏/遮罩 + 禁用控制”，再由 `MinLoopFlowController` 切相机。

# HEARTH 17F02 最小循环接口说明

> 做第二户剧情、字幕、动作、门动画、音效前，先读这份。  
> 它只记录第二户当前最小循环的可维护入口。

## 当前 17F02 流程

当前第二户被拆成 7 个节拍：

1. `BedroomWake`  
   玩家进入陪伴单元视角后先黑屏。黑屏期间，玩家能听到女主刚进家门后在客厅/房外和男主聊天的声音，字幕正常播放。随后女主进入卧室，应有门声、关门声、坐到床上的声音；女主坐下后，用语音主动唤醒陪伴单元。

2. `BedroomConfide`  
   陪伴单元被唤醒后，机器人 HUD 显示 `17F02_02`。女主对陪伴单元倾诉，玩家可以移动和转视角。倾诉字幕播完后，等待约 `1.5s`，才出现 E 安慰交互。

3. `WifeExitLocked`  
   玩家完成 E 交互后，先播放 `17F02_BedroomComfort`，也就是陪伴单元对女主人的回应/安慰字幕。女主得到一些安慰后，听到男主从餐厅/客厅喊她吃饭；女主只回应男主，不再对陪伴单元说“等一下”，也不需要照顾陪伴单元的感受。随后女主直接起身离开卧室。离开期间机器人不能移动、不能干预，只能看着她走出去。
   当前出门路线改为三段式：先走 `Wife Before Door Path Points`，再到 `Wife Door Pause Anchor` 开门，再走 `Wife After Door Path Points` 和 `Wife Exit Outside Anchor`。这样不会再需要计算“第几个点后插入开门”。

4. `DiningObservation`  
   女主最终移动到餐桌处，位置和朝向以 `casual_Female_K` 为准。几秒后默认认为男主人和女主人已经坐到餐桌前。机器人恢复移动，玩家可以走出房间并听到餐桌对话。
   第二幕只显示 `casual_Male_K` 和 `casual_Female_K`。

5. `LivingRoomTerminal`  
   餐桌沉默后黑屏切到客厅固定视角。机器人被传到 `Robot Controller3` 对应锚点，不能移动，也不能转视角。男主面对陪伴单元调用记录。
   第三幕只显示 `casual_Male_K (1)`，餐桌男女会隐藏。

6. `ForcedShutdown`  
   男主查看记录后情绪升级，触发强制关闭 UI 和故障效果。

7. `BlackAudio`  
   黑屏音频阶段，只播放争吵字幕/后续可接语音。结束后返回人类终端 A/B 处置。

## 场景中你需要准备的对象

运行绑定菜单前，建议先准备：

- `Player/Robot Controller`  
  正式运行时机器人，只保留这一份用于玩家控制。

- `Robot Controller2`  
  放在第二户卧室里，作为机器人开场位置和朝向参考。运行绑定工具后，它会被复制成锚点，自己会被隐藏并禁用成“参考物”，游戏里不会再看到它。

- `Robot Controller3`  
  放在客厅男主面对机器人时的位置和朝向参考。运行绑定工具后，它会被复制成锚点，自己会被隐藏并禁用成“参考物”，游戏里不会再看到它。

- `casual_Female_K2`  
  默认作为女主人卧室模型引用。

- `casual_Female_K` 或 `casual_Female_K2`  
  默认作为女主人餐桌模型引用。若你之后使用独立餐桌女主人模型，把它拖到 `Dining Wife Actor`。

- `casual_Male_K`  
  默认作为男主人餐桌模型引用。

- `casual_Male_K2`  
  默认作为客厅中面对机器人、操作陪伴单元 UI 的男主人模型引用。

- `Door_2_Brown (4)`  
  第二户卧室门。绑定菜单会自动给它添加/配置 `SmartDoorController`，默认让子物体 `Door` 旋转打开。

## 自动绑定菜单

菜单位置：

`Tools / Hearth / Replay / Apply 17F02 Minimal Loop Setup`

它会自动：

- 创建 `MIN_LOOP_ROOT/ReplayRoom_17F02/HearthCompanion17F02ReplayController`。
- 创建 `MIN_LOOP_ROOT/Anchors/Anchor_Robot_17F02_BedroomStart`。
- 创建 `MIN_LOOP_ROOT/Anchors/Anchor_Robot_17F02_LivingRoomTerminal`。
- 创建 `MIN_LOOP_ROOT/Anchors/Anchor_Wife_17F02_Path01`。
- 创建 `MIN_LOOP_ROOT/Anchors/Anchor_Wife_17F02_Path02`。
- 如果你已经手动复制了 `Anchor_Wife_17F02_Path01 (1)` 到 `Anchor_Wife_17F02_Path01 (5)`，绑定工具会优先把这些细调点按顺序写入 `Wife Exit Path Points`；旧 `Path02` 只在没有细调点时兜底。
- 创建 `MIN_LOOP_ROOT/Anchors/Anchor_Wife_17F02_DoorPause`。
- 创建 `MIN_LOOP_ROOT/Anchors/Anchor_Wife_17F02_ExitOutside`。
- 给 `Door_2_Brown (4)` 添加/配置 `SmartDoorController`。
- 把第二户终端的 `Replay Resident Id` 设置为 `17F02`。
- 把 `MinLoopFlowController` 绑定到 `HearthCompanion17F02ReplayController`。
- 创建默认字幕资产。

## 字幕和语音在哪里改

第二户字幕资产在：

`Assets/Data/MinLoop/Dialogues/`

当前会生成这些文件：

- `17F02_BedroomWake.asset`
- `17F02_BedroomConfide.asset`
- `17F02_BedroomComfort.asset`
- `17F02_WifeExit.asset`
- `17F02_DiningObservation.asset`
- `17F02_LogAccess.asset`
- `17F02_ForcedShutdown.asset`
- `17F02_BlackAudioArgument.asset`

每个资产里重点字段：

- `Lines`：字幕列表。
- `Speaker`：说话人。
- `Text`：字幕正文。
- `Start Delay`：这一句出现前等待多久。
- `Hold Seconds`：这一句至少显示多久。
- `Voice Clip`：后续录音后拖进这里。拖入后字幕会至少显示到音频播放结束。
- `Post Sequence Delay`：整段字幕结束后的额外等待时间。

黑屏阶段字幕显示层级：

- `HearthCompanion17F02ReplayController / Blackout Sorting Order` 默认是 `7000`。
- `MIN_LOOP_ROOT/UI/MinLoopSubtitlePlayer / Force Subtitle Canvas Sorting` 应开启。
- `MIN_LOOP_ROOT/UI/MinLoopSubtitlePlayer / Subtitle Sorting Order` 推荐保持 `7600`。
- 如果黑屏中听得到流程但看不到字幕，优先检查这两个排序值：字幕排序必须高于黑屏排序。

## 机器人 HUD 页面在哪里改

第二户机器人 HUD 数据在：

`Assets/Data/HearthHud/Companion/`

对应页面：

- `CompanionScene_04_17F02_01.asset`：唤醒/卧室开场。
- `CompanionScene_05_17F02_02.asset`：卧室倾诉。
- `CompanionScene_06_17F02_03.asset`：餐桌观察。
- `CompanionScene_07_17F02_04.asset`：男主调用日志。
- `CompanionScene_08_17F02_05.asset`：强制关闭。
- `CompanionScene_09_17F02_06.asset`：黑屏音频。

常改字段：

- `Decision Title / Decision Body`：右上角长期信息。
- `Data Stream Lines`：左下角数据流。
- `Timed Cues`：短时间出现的左上角监控卡。
- `Show Hold Prompt / Hold Prompt Text / Hold Seconds`：长按 E 提示。
- `Special Title / Special Body / Special Status Label`：黑屏、故障、深眠等特效文字。

UI 的位置和大小仍然看：

`HEARTH_陪伴单元机器人HUD调参入口.md`

## 关卡02 UI 统一调整入口

如果你想整体调整第二户的 UI，不要只看一个脚本，按下面几类入口调：

- 17F02 TV 终端页面：选中 `17F/ROOM2/TV (4)/MonitorCanvas/Terminal_17F02`。页面整体大小优先调 `MonitorCanvas` 或终端内容父级的缩放；按钮/页面切换由 `HearthTvTerminalController` 控制。
- 17F02 机器人 HUD 文本和短时卡：改 `Assets/Data/HearthHud/Companion/CompanionScene_04_17F02_01.asset` 到 `CompanionScene_09_17F02_06.asset`。
- 机器人 HUD 的通用位置、大小、文字字号、边框和面板位置：改场景里的 `HearthCompanionHudRoot`，具体入口见 `HEARTH_陪伴单元机器人HUD调参入口.md`。这些属于通用机器人 HUD，改一次会影响 17F01/17F02/17F03；如果只想第二户独立变体，后续需要复制一个 17F02 专用 Root 或增加 per-scene override。
- 字幕位置、字号、宽度：选中 `MIN_LOOP_ROOT/UI/MinLoopSubtitlePlayer`，调整 `Speaker Center Y`、`Body Center Y`、字体大小和正文宽度。这里目前是全局字幕样式，改一次会影响所有关卡字幕。
- 17F02 字幕内容、每句停留时长、语音：改 `Assets/Data/MinLoop/Dialogues/17F02_*.asset`。后续录音后，把对应 `AudioClip` 拖到每句 `Voice Clip`。
- 17F02 黑屏、淡入淡出、等待时长、E 交互开放时间：选中 `MIN_LOOP_ROOT/ReplayRoom_17F02/HearthCompanion17F02ReplayController`，看 `Timing` 和 `Interaction Gates`。

## 第三幕固定视角怎么调

第三幕“男主调用记录”的视角现在由两个锚点一起决定：

- `Anchor_Robot_17F02_LivingRoomTerminal`：控制正式机器人身体位置和水平朝向。
- `Anchor_Robot_17F02_LivingRoomTerminalCamera`：控制正式机器人相机位置和俯仰角。

推荐调法：

1. 在 Scene 里把 `Robot Controller (3)` 放到第三幕你想要的位置。
2. 调它子物体里的 Camera 视角，特别是俯仰角。
3. 运行 `Tools / Hearth / Replay / Apply 17F02 Minimal Loop Setup`，工具会重新复制身体锚点和相机锚点。
4. 如果你同时调整了女主离开卧室参考模型，再运行一次 `Tools / Hearth / Replay / Build 17F02 Wife Route From Female References`。

修复后的逻辑：从第二幕餐桌切到第三幕后，正式 `Player/Robot Controller` 会同时套用第三幕身体锚点和相机锚点，不再继承第二幕最后玩家看地面或看天花板的视角。

## 黑屏和节奏在哪里调

选中：

`MIN_LOOP_ROOT/ReplayRoom_17F02/HearthCompanion17F02ReplayController`

在 Inspector 里看 `Timing`：

- `Initial Black Seconds`：开场黑屏停留多久。
- `Wake Fade Seconds`：被唤醒后从黑屏淡入多久。
- `Wait After Confide Seconds`：卧室倾诉结束后等多久进入女主人离开段。
- `Wife Exit Locked Seconds`：女主人离开后，机器人锁定多久。
- `Wait Before Dining Dialogue Seconds`：餐桌观察开始后多久播放对话。
- `Post Dining Silence Seconds`：餐桌对话结束后的沉默时间。
- `Living Room Fade Out / Hold / Fade In`：餐桌到客厅固定视角的黑屏转场。
- `Wait Before Forced Shutdown Seconds`：日志调用后多久进入强制关闭。
- `Shutdown Effect Seconds`：强制关闭故障效果持续多久。
- `Wait Before Return Seconds`：黑屏音频结束后多久返回人类终端。

## 女主人出门路线和门怎么调

当前推荐的女主离开卧室路线由“参考女主模型”驱动，而不是直接调空锚点。先在 Scene 视图里移动和旋转这些可见模型，再运行菜单同步到程序锚点：

```text
casual_Female_K@Sitting_Disbelief
-> REF_Wife_17F02_BeforeDoor_01
-> REF_Wife_17F02_BeforeDoor_02
-> REF_Wife_17F02_BeforeDoor_03
-> REF_Wife_17F02_BeforeDoor_04
-> REF_Wife_17F02_BeforeDoor_05
-> REF_Wife_17F02_DoorPause
-> REF_Wife_17F02_ExitOutside
```

选中：

`MIN_LOOP_ROOT/ReplayRoom_17F02/HearthCompanion17F02ReplayController`

看 `Wife Exit Blocking`：

- `Move Bedroom Wife To Door`：是否让女主人自动走向门。关闭后只播放字幕和锁定，不移动角色。
- `Bedroom Wife Move Root`：卧室女主人模型，现在固定推荐是 `casual_Female_K@Sitting_Disbelief`。
- `Use Simple Wife Exit Route`：当前推荐开启。开启后使用更直观的三段式路线，不再使用旧的“第几个路径点后开门”算法。
- `Wife Before Door Path Points`：门前路线点，只放女主还在房间内、准备走到门口之前的点。
- `Move To Door Pause Before Opening`：当前推荐开启。女主走完门前路线后，会先贴到 `Wife Door Pause Anchor`，停顿，然后开门。
- `Wife After Door Path Points`：门后路线点，只放门已经打开后、女主穿过门框和走到门外的点。
- `Wife Exit Path Points`：旧路线数组，保留作为兜底。如果新的门前/门后数组为空，脚本会自动把旧数组前 4 个点当门前、后 2 个点当门后，避免旧场景直接失效。
- `Wife Door Pause Anchor`：女主人开门前停一下的位置。
- `Wife Exit Outside Anchor`：女主人走出房间后的终点。
- `Wife Walk Speed / Wife Rotate Speed`：女主人脚本位移和转身速度。当前走路动作由 `HearthActorAnimationPlayer / WalkLoop` 播放，身体位置仍按这些锚点移动。
- `Wife Door Pause Seconds`：到门口停顿多久再开门。
- `Wait After Door Open Seconds`：门打开后等多久再走出去。
- `Wife Exit Door`：拖 `Door_2_Brown (4)` 上的 `SmartDoorController`。
- `Open Door During Wife Exit`：女主人到门口后是否自动开门。
- `Keep Door Open After Wife Exit`：女主人离开后门是否保持打开。
- `Hide Bedroom Wife After Exit`：女主人走出后是否隐藏卧室那份模型。现在默认关闭，避免误删视野中的模型；如果你用餐桌女主人替代后续状态，可以开启。

路线由菜单自动从参考模型复制到锚点，不建议手工维护数组。执行：

`Tools / Hearth / Replay / Build 17F02 Wife Route From Female References`

菜单会自动处理：

- `casual_Female_K@Sitting_Disbelief` 保持原名并作为唯一运行时卧室女主，不再被当作源动作对象关闭。
- 旧 `Actor_Wife_17F02_Bedroom` 如果仍在场景里，会被设为 inactive，避免与真实卧室女主重叠。
- `casual_Female_K (3)` 到 `(7)` 会改名为 `REF_Wife_17F02_BeforeDoor_01` 到 `05`，作为开门前路线参考。
- `casual_Female_K (8)` 会改名为 `REF_Wife_17F02_DoorPause`，只作为停下并开门的位置，不再放进普通路径。
- `casual_Female_K (9)` 会改名为 `REF_Wife_17F02_ExitOutside`，作为走出房间后的终点。
- 参考模型会统一放到 `MIN_LOOP_ROOT / ReplayRoom_17F02 / WifeRouteReferenceModels`，并挂 `HearthEditorOnlyReferenceModel`。编辑时仍然可见，但 Collider / Rigidbody 碰撞会保持关闭；Play 时 Renderer、Collider、Animator、AudioSource 和导航组件都会关闭，所以不会挡住机器人走路。
- 程序锚点会自动生成到 `MIN_LOOP_ROOT / Anchors`，并写入 `HearthCompanion17F02ReplayController`。
- 已淘汰的旧路线锚点 `Anchor_Wife_17F02_Path01`、`Path01 (1)-(5)`、`Path02` 会被清理，避免以后误用。

门如果打开方向反了，选中 `Door_2_Brown (4)`，把 `SmartDoorController / Open Local Euler Offset / Y` 从 `90` 改成 `-90`。如果没有开门声音，把音效拖到 `Open Clip` 和 `Close Clip`；当前门资产可能只有 AudioSource，但没有实际 AudioClip。

### 你自己调路线的具体方法

1. 在 Hierarchy 里展开 `MIN_LOOP_ROOT / ReplayRoom_17F02 / WifeRouteReferenceModels`。
2. 直接移动和旋转 `REF_Wife_17F02_BeforeDoor_01` 到 `05`，它们代表女主开门前的走位和朝向。
3. 移动和旋转 `REF_Wife_17F02_DoorPause`，它代表女主停下来、等待、开门的位置。
4. 移动和旋转 `REF_Wife_17F02_ExitOutside`，它代表女主走出房间后的最终位置。
5. 运行 `Tools / Hearth / Replay / Build 17F02 Wife Route From Female References`，把参考模型同步到程序锚点。
6. 进入 Play Mode 测试；如果还穿模，退出 Play Mode 后继续移动参考模型，再重跑菜单。
7. 女主最终到餐桌后的静态位置和朝向，以 `casual_Female_K` 为准。

如果你更喜欢看空锚点，也可以查看 `MIN_LOOP_ROOT / Anchors` 下自动生成的 `Anchor_Wife_17F02_BeforeDoor_01` 到 `05`、`Anchor_Wife_17F02_DoorPause`、`Anchor_Wife_17F02_ExitOutside`。它们会挂 `HearthRouteAnchorGizmo`，只用于预览和脚本移动。旧 `Path01 / Path02` 系列已经淘汰，不要再手动恢复。

如果你暂时不想让脚本移动女主人，可以关闭 `Move Bedroom Wife To Door`。这样流程仍然播放字幕和锁定机器人，但女主人不会自动走；后续可以用 Animator、Timeline 或事件来接真正动作。

## 2026-07-07 最新女主移动根与开门修正规则

这一节优先于上面的旧文字。

17F02 卧室阶段现在分成两个层级：

```text
Actor_Wife_17F02_BedroomRuntimeRoot
└── casual_Female_K@Sitting_Disbelief
```

- `Actor_Wife_17F02_BedroomRuntimeRoot` 是真正的剧情演员根、移动根和显隐根。
- `casual_Female_K@Sitting_Disbelief` 是可见女主模型，只作为 RuntimeRoot 的子物体。
- `HearthCompanion17F02ReplayController / Bedroom Wife Actor` 应绑定 RuntimeRoot。
- `HearthCompanion17F02ReplayController / Bedroom Wife Move Root` 应绑定 RuntimeRoot。
- `HearthCompanion17F02ReplayController / Bedroom Wife Animation` 应是 RuntimeRoot 上的 `HearthActorAnimationPlayer`，但它的 `Animator` 字段指向子模型的 Animator。

女主动作 Slot 当前规则：

```text
SittingDisbelief   Apply Root Motion = off   Stabilize Animator Transform = on
SittingTalking     Apply Root Motion = off   Stabilize Animator Transform = on
SitToStand         Apply Root Motion = off   Stabilize Animator Transform = on
WalkLoop           Apply Root Motion = off   Stabilize Animator Transform = on
OpenDoorOutwards   Apply Root Motion = off   Stabilize Animator Transform = on
```

也就是说，女主世界位置全部由 RuntimeRoot 和路线锚点控制，不再让 Mixamo 动画自己移动世界坐标。

当前出门路线固定为：

```text
REF_Wife_17F02_BeforeDoor_01
-> REF_Wife_17F02_BeforeDoor_02
-> REF_Wife_17F02_BeforeDoor_03
-> REF_Wife_17F02_BeforeDoor_04
-> REF_Wife_17F02_BeforeDoor_05
-> REF_Wife_17F02_DoorPause
-> REF_Wife_17F02_ExitOutside
```

对应 Inspector：

- `Wife Before Door Path Points`：5 个点，`BeforeDoor_01` 到 `BeforeDoor_05`。
- `Wife After Door Path Points`：当前为空。
- `Wife Door Pause Anchor`：`REF_Wife_17F02_DoorPause`。
- `Wife Exit Outside Anchor`：`REF_Wife_17F02_ExitOutside`。

门控制：

- `Door_2_Brown (4)` 上的 `SmartDoorController / Use Unscaled Time` 应开启。
- `Door Open Delay After Animation Start Seconds` 默认 `1`，控制 `OpenDoorOutwards` 开始多久后开门。
- 如果门不开，先检查 `Wife Exit Door` 是否拖的是 `Door_2_Brown (4)` 上的 `SmartDoorController`。

防卡死：

- 如果 RuntimeRoot 一段时间没有靠近目标点，脚本会输出 Warning 并吸附到目标锚点继续流程。
- 如果某个动作 Clip 缺失，会跳过动作但继续路线和开门。
- 如果门引用缺失，会跳过开门但继续后续阶段，不会让整段 17F02 回放停止。

重跑绑定：

1. 移动或旋转 `REF_Wife_17F02_*` 参考模型。
2. 运行 `Tools / Hearth / Replay / Build 17F02 Wife Route From Female References`。
3. 运行 `Tools / Hearth / Replay / Apply 17F02 Minimal Loop Setup`。
4. 确认 RuntimeRoot、BeforeDoor 数组、门和动作 Slot 仍然按本节规则绑定。

## 第二幕/第三幕模型显隐

选中：

`MIN_LOOP_ROOT/ReplayRoom_17F02/HearthCompanion17F02ReplayController`

看 `Actor Visibility`：

- `Manage Actor Visibility`：当前应开启。
- `Bedroom Wife Actor`：卧室里的女主，当前是 `casual_Female_K@Sitting_Disbelief`。
- `Dining Wife Actor`：第二幕餐桌女主，当前是 `casual_Female_K`。
- `Dining Husband Actor`：第二幕餐桌男主，当前是 `casual_Male_K`。
- `Terminal Husband Actor`：第三幕操作陪伴单元的男主，当前是 `casual_Male_K (1)`。

显示规则：

- 卧室段：只显示卧室女主。
- 第二幕餐桌段：只显示 `casual_Male_K` 和 `casual_Female_K`。
- 第三幕终端段：只显示 `casual_Male_K (1)`。

## 卧室 E 交互怎么调

选中：

`MIN_LOOP_ROOT/ReplayRoom_17F02/HearthCompanion17F02ReplayController`

看 `Interaction Gates`：

- `Show Bedroom Hold Prompt During Confide`：默认关闭，所以倾诉期间不会一直显示 E。
- `Wait For Bedroom Acknowledgement`：默认开启，所以倾诉结束后会等待玩家按 E。
- `Bedroom Prompt Delay After Confide Seconds`：默认 `1.5`，控制倾诉结束后过多久才出现 E。

按 E 后播放：

`Assets/Data/MinLoop/Dialogues/17F02_BedroomComfort.asset`

你可以直接改这个资产里的 `Lines`，把陪伴单元的安慰话术和女主人的回应改成你想要的内容。

## 门动画和人物动作接口

同样选中：

`HearthCompanion17F02ReplayController`

看 `Events`：

- `On Bedroom Wake Started`：开场黑屏唤醒开始。
- `On Bedroom Confide Started`：卧室倾诉开始。
- `On Wife Exit Started`：女主人准备离开。当前基础位移和开门已由 `Wife Exit Blocking` 执行，后续适合接起身动画、脚步声、镜头提示。
- `On Wife Exit Finished`：女主人离开完成。适合把门保持打开、切换餐桌角色状态。
- `On Dining Observation Started`：餐桌观察开始。
- `On Living Room Terminal Started`：黑屏切到客厅固定视角后触发。适合切男主站位/俯身动作。
- `On Forced Shutdown Started`：男主强制关闭开始。适合接愤怒动作、手部操作、关闭音效。
- `On Replay Finished`：整个机器人回放结束。

当前人物动作已经由 `HearthActorAnimationPlayer` 接入，基础 Clip 映射见本文末尾“2026-07-06 动作接入后的调整入口”。这些 UnityEvent 仍然保留，用来追加脚步声、门声、特写、额外手部动作或后续 Timeline。

## 机器人能不能动在哪里控制

当前默认：

- 开场黑屏：不能动，不能转视角。
- 卧室倾诉：可以动，可以转视角。
- 女主人离开：不能动，可以转视角。
- 餐桌观察：可以动，可以转视角。
- 客厅固定视角：不能动，不能转视角。
- 强制关闭/黑屏音频：不能动，不能转视角。

这些逻辑在：

`Assets/Scripts/MinLoop/HearthCompanion17F02ReplayController.cs`

如果之后要把某一段改成“不能移动但能转头”，告诉 Codex 改对应阶段的 `SetRobotControl(move, look, interaction)`。

## A/B 处置接口

第二户结束后仍然回到当前 TV 终端的 A/B 页面：

- A：`MinLoopFlowController.ChooseDispositionA()`，信任度 `+1`。
- B：`MinLoopFlowController.ChooseDispositionB()`，信任度 `-1`。

历史记录会由玩家 HUD 的 `HearthFirstPersonHudFlowBinder` 监听处置事件后记录。  
如果第一户已经完成，第二户完成后记录会显示为 `17F-02`。

## 当前预留但还没真正制作的部分

- 更精细的门音效素材。
- 餐桌两人目前已有基础坐姿循环，后续可替换为更自然的吃饭/交谈动画。
- 第三幕男主目前已有 `ButtonPushing` 基础循环，后续可替换为更精细的俯身操作陪伴单元 UI 动画。
- 男主生气动作。
- 黑屏争吵的真实语音。
- 餐厅/客厅的空间触发范围限制。
- 第二户专属 A/B 文案细化。

这些都已经有接口，不需要重写第二户流程。

## 2026-07-06 动作接入后的调整入口

现在 17F02 已经接入 `HearthActorAnimationPlayer`，选中下面这些角色根对象即可替换动作：

- `casual_Female_K@Sitting_Disbelief`：卧室女主。
- `casual_Female_K`：第二幕餐桌女主。
- `casual_Male_K`：第二幕餐桌男主。
- `casual_Male_K (1)`：第三幕男主。

每个对象上都有 `HearthActorAnimationPlayer`：

- `Clip Id` 是剧情脚本调用的名字，不建议随便改。
- `Clip` 是实际动画，可以直接拖新的 Mixamo / FBX 动作 Clip。
- `Loop` 控制是否循环。
- `Apply Root Motion` 控制动画是否自己带位移。当前只有 `OpenDoorOutwards` 开启。
- `Fade Seconds` 控制切动作时的淡入时间。

运行时真实演员规则：

- 卧室阶段只显示并驱动 `casual_Female_K@Sitting_Disbelief`。
- 餐桌阶段只显示并驱动 `casual_Female_K` 与 `casual_Male_K`。
- 第三幕只显示并驱动 `casual_Male_K (1)`。
- `REF_Wife_17F02_*` 是编辑器参考模型，只用于你在 Scene 里看位置和朝向；Play 时会隐藏并且没有碰撞。
- 旧 `Actor_Wife_17F02_Bedroom`、`Sitting_Idle`、`Sitting`、`Button_Pushing`、`Female_Start_Walking`、`Open_Door_Outwards`、`X_Bot@Sit_To_Stand` 这类旧演员或源动作对象不应该参与游戏显示。重跑 `Apply 17F02 Minimal Loop Setup` 会自动关闭它们。

动作与旧 Pose 的关系：

- 当前优先播放 `HearthActorAnimationPlayer` 里的 Clip。
- 只有某个 Clip 没有绑定时，才回退到旧 `HearthActorPosePreset`。
- 这样可以避免 PosePreset 暂停 Animator，导致女主不起身、动作混乱或餐桌坐姿不播放。

当前 17F02 默认动作映射：

```text
casual_Female_K@Sitting_Disbelief
- SittingDisbelief -> Assets/casual_Female_K@Sitting_Disbelief.fbx / mixamo.com
- SittingTalking -> Assets/Sitting_Talking.fbx / mixamo.com
- SitToStand -> Assets/X_Bot@Sit_To_Stand.fbx / Sit_To_Stand
- WalkLoop -> Assets/casual_Female_K@Walking.fbx / Walking
- OpenDoorOutwards -> Assets/Open_Door_Outwards.fbx / mixamo.com

casual_Female_K
- Sitting -> Assets/Sitting.fbx / mixamo.com

casual_Male_K
- SittingIdle -> Assets/Sitting_Idle.fbx / mixamo.com

casual_Male_K (1)
- ButtonPushing -> Assets/Button_Pushing.fbx / mixamo.com
```

女主离开卧室的新执行顺序：

1. 黑屏恢复后，女主循环 `SittingDisbelief`。
2. 玩家完成 E 安慰后，女主播放一次 `SittingTalking`。`Bedroom Talking Max Seconds` 默认 `10`，设为 `0` 或负数时等待完整动作/字幕。
3. 男主喊吃饭、女主回应后，女主播放 `SitToStand`。
4. 循环 `WalkLoop`，使用 `Assets/casual_Female_K@Walking.fbx / Walking`，同时按参考点路线移动到门口。
5. 到 `Wife Door Pause Anchor` 后停止走路，播放 `OpenDoorOutwards`。
6. `OpenDoorOutwards` 开始约 `1s` 后调用 `Door_2_Brown (4)` 开门。
7. 开门动作结束后关闭 root motion，再直接移动/校正到 `Wife Exit Outside Anchor`。

如果开门与人物动作不同步：

- 选中 `MIN_LOOP_ROOT/ReplayRoom_17F02/HearthCompanion17F02ReplayController`。
- 调 `Actor Animations / Door Open Delay After Animation Start Seconds`。
- 数值变小：门更早打开。
- 数值变大：门更晚打开。

如果 `OpenDoorOutwards` 自带位移太大或方向不对：

- 首先微调 `REF_Wife_17F02_DoorPause` 和 `REF_Wife_17F02_ExitOutside`，再运行 `Build 17F02 Wife Route From Female References`。
- 如果仍然偏移明显，可以临时关闭 `OpenDoorOutwards / Apply Root Motion`，让脚本完全按锚点移动。

17F01 动作调整入口：

- `little_boy_B`：`LayingSleeping`。
- `casual_Female_G@Sitting_Idle`：`SittingIdle`。
- `casual_Male_G@Sitting`：`Sitting`。

17F01 的位置和模型不由脚本重摆；进入 17F01 回放后只播放这些循环动作。
## 2026-07-07 最新动画接入规则：Humanoid + Animator Controller

本节优先级高于旧的 `HearthActorAnimationPlayer / Playables` 说明。17F02 从现在开始使用 Unity 标准 Humanoid / Animator Controller 流程。

### 为什么改

- 之前的 Mixamo FBX 是 Generic 导入，`AnimationClip.isHumanMotion = false`，无法稳定重定向到真实女主/男主模型。
- 卧室女主曾经绑定到 `avatar=null / controller=null` 的 Animator，导致脚本推进了，但模型仍保持坐姿移动。
- 现在已经改为：FBX 统一 Humanoid 导入，真实演员有 Avatar、有 Animator Controller，流程只调用 Animator 状态。

### 当前真实演员

- 卧室女主：`Actor_Wife_17F02_BedroomRuntimeRoot`
  - 可见子模型：`casual_Female_K@Sitting_Disbelief`
  - Animator：子模型上的 Animator
  - Avatar：`casual_Female_KAvatar`
  - Controller：`BedroomWife17F02.controller`
- 第二幕餐桌女主：`casual_Female_K`
  - Controller：`DiningWife17F02.controller`
- 第二幕餐桌男主：`casual_Male_K`
  - Controller：`DiningHusband17F02.controller`
- 第三幕男主：`casual_Male_K (1)`
  - Controller：`TerminalHusband17F02.controller`

### 当前动作顺序

1. 黑屏和唤醒结束后，卧室女主循环 `SittingDisbelief`。
2. 玩家完成 E 安慰后，卧室女主播放一次 `SittingTalking`。
3. 男主喊吃饭后，卧室女主播放 `SitToStand`。
4. 女主循环 `WalkLoop`，同时脚本移动 RuntimeRoot 到 `BeforeDoor_01` 到 `BeforeDoor_05`。
5. 到 `DoorPause` 后播放 `OpenDoorOutwards`。
6. `OpenDoorOutwards` 开始约 1 秒后调用 `Door_2_Brown (4)` 的开门逻辑。
7. 开门动作结束后，女主直接校正到 `ExitOutside`。
8. 第二幕只显示餐桌男女并播放 `Sitting / SittingIdle`。
9. 第三幕只显示 `casual_Male_K (1)` 并播放 `ButtonPushing`。

### 以后怎么换动作

1. 把新的 Mixamo FBX 放进 `Assets`。
2. 如果只是替换同一动作，优先保持原 `State Id` 不变，只在绑定工具代码或 Inspector 里换 Clip。
3. 运行 `Tools / Hearth / Replay / Apply 17F02 Minimal Loop Setup`。
4. 运行 `Tools / Hearth / Replay / Validate 17F02 Animation Setup`，确认 Clip 是 `human=True`，Driver 有 Avatar 和 Controller。

### 路线参考模型规则

- `REF_Wife_17F02_*` 仍然只用于你在 Scene 里细调位置和朝向。
- 参考模型不是运行时演员，不播放剧情动作。
- Play Mode 下参考模型应隐藏 Renderer、关闭 Collider，不能挡机器人移动。
- 调整参考模型后，先运行 `Build 17F02 Wife Route From Female References`，再运行 `Apply 17F02 Minimal Loop Setup`。

## 2026-07-17 最新动作衔接、开门与出门校正规则

本节优先于上方旧的 `1s` 开门延迟和 Root Motion 建议。

- 卧室女主 `HearthActorAnimatorDriver / Minimum Transition Seconds` 当前为 `0.32s`。SittingDisbelief、SittingTalking、SitToStand、WalkLoop、OpenDoorOutwards 五个状态都启用 `Stabilize Animator Transform`。
- 若动作仍显得太急，可以先把最小过渡调到 `0.4-0.5s`；不要通过给剧情协程添加无关固定等待来掩盖动作切换。
- 门的正式触发值为 `Door Open Delay After Animation Start Seconds = 0.5s`。这是“开门动作开始后等待多久打开实体门”，不是整段动作播放时长。
- 女主出门完成后，控制器会停止动作、恢复可见模型的本地基准，并把 RuntimeRoot 再次精确放到 `Wife Exit Outside Anchor`，防止动画尾帧把她拉回门后。
- 当前女主所有动作的世界 Root Motion 都不负责剧情位移；路线和最终站位只认 RuntimeRoot 与 Anchor。
- 17F02 第三幕 `CompanionScene_07_17F02_04 / Center Message` 必须为空；中央只显示 `FAMILY LOG - TODAY` 投影面板。
- MCP 直接运行女主离开协程的验收结果：最终 RuntimeRoot 与 ExitOutside 距离 `0.00000m`，门处于打开状态，额外等待后子模型没有闪回。
