# 2026-07-02 回放切入闪到 17F01 的修复记录

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
   当前门时机由 `Open Door After Path Point Count` 控制，当前值是 `5`，用于避免女主先穿门、门后打开。

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

当前场景已同步的女主离开卧室路线是：

```text
Anchor_Wife_17F02_Path01
-> Anchor_Wife_17F02_Path01 (1)
-> Anchor_Wife_17F02_Path01 (2)
-> Anchor_Wife_17F02_Path01 (3)
-> Anchor_Wife_17F02_Path01 (4)
-> Anchor_Wife_17F02_Path01 (5)
-> Anchor_Wife_17F02_DoorPause
-> Anchor_Wife_17F02_ExitOutside
```

选中：

`MIN_LOOP_ROOT/ReplayRoom_17F02/HearthCompanion17F02ReplayController`

看 `Wife Exit Blocking`：

- `Move Bedroom Wife To Door`：是否让女主人自动走向门。关闭后只播放字幕和锁定，不移动角色。
- `Bedroom Wife Move Root`：卧室女主人模型，一般是 `casual_Female_K (2)` 或你指定的卧室女主人根物体。
- `Wife Exit Path Points`：中途路线点数组。最新走位意图记录为 `Anchor_Wife_17F02_Path01 -> Anchor_Wife_17F02_Path01 (1) -> Anchor_Wife_17F02_Path01 (2) -> Anchor_Wife_17F02_Path01 (3) -> Anchor_Wife_17F02_Path01 (4) -> Anchor_Wife_17F02_Path01 (5)`，这些路径点用于细调女主人离开卧室时的位置和朝向，让路径更丝滑。
- `Open Door After Path Point Count`：女主走完几个 `Wife Exit Path Points` 后，先移动到 `Wife Door Pause Anchor` 开门，再继续剩余路径。当前推荐值是 `5`。如果女主还是先穿门再开门，把它调小；如果门开太早，把它调大；如果填 `-1`，会恢复旧逻辑，也就是走完所有路径点后才开门。
- `Wife Door Pause Anchor`：女主人开门前停一下的位置。
- `Wife Exit Outside Anchor`：女主人走出房间后的终点。
- `Wife Walk Speed / Wife Rotate Speed`：女主人脚本位移和转身速度。现在只是滑动位移，后续接 Animator 后可以保留这些锚点作为路线。
- `Wife Door Pause Seconds`：到门口停顿多久再开门。
- `Wait After Door Open Seconds`：门打开后等多久再走出去。
- `Wife Exit Door`：拖 `Door_2_Brown (4)` 上的 `SmartDoorController`。
- `Open Door During Wife Exit`：女主人到门口后是否自动开门。
- `Keep Door Open After Wife Exit`：女主人离开后门是否保持打开。
- `Hide Bedroom Wife After Exit`：女主人走出后是否隐藏卧室那份模型。现在默认关闭，避免误删视野中的模型；如果你用餐桌女主人替代后续状态，可以开启。

路线由锚点控制，不建议写死在脚本里。`Path01 (1)` 到 `Path01 (5)` 是你额外复制出来的细调点，用于减少穿模、控制转身方向，并让女主从房间里走出来的运动更自然。后续真正接脚本时，应把这些点按顺序拖进 `Wife Exit Path Points`，再单独绑定 `Wife Door Pause Anchor` 和 `Wife Exit Outside Anchor`。

门如果打开方向反了，选中 `Door_2_Brown (4)`，把 `SmartDoorController / Open Local Euler Offset / Y` 从 `90` 改成 `-90`。如果没有开门声音，把音效拖到 `Open Clip` 和 `Close Clip`；当前门资产可能只有 AudioSource，但没有实际 AudioClip。

### 你自己调路线的具体方法

1. 在 Hierarchy 里展开 `MIN_LOOP_ROOT / Anchors`。
2. 选中 `Anchor_Wife_17F02_Path01`，用移动工具把它放到女主人离床后的第一个安全位置。
3. 依次选中 `Anchor_Wife_17F02_Path01 (1)`、`(2)`、`(3)`、`(4)`、`(5)`，把它们放到女主人从卧室走向门口时需要细调的位置。
4. 选中 `Anchor_Wife_17F02_DoorPause`，把它放在门内侧，让女主人到这里先停顿并准备开门。
5. 选中 `Anchor_Wife_17F02_ExitOutside`，把它放在门外侧，作为走出房间后的终点。
6. 用旋转工具调整每个锚点的 Y 轴方向。女主人走向这个点时会逐渐转向这个锚点的朝向，到达时会贴合这个锚点的朝向。
7. 女主最终到餐桌后的静态位置和朝向，以 `casual_Female_K` 为准。

如果还是穿模，可以继续复制新的空物体放到 `MIN_LOOP_ROOT / Anchors` 下，并把它按顺序拖进 `HearthCompanion17F02ReplayController / Wife Exit Path Points` 数组里。路径点越多，越方便控制女主转身和绕开床、墙、门框。

如果你暂时不想让脚本移动女主人，可以关闭 `Move Bedroom Wife To Door`。这样流程仍然播放字幕和锁定机器人，但女主人不会自动走；后续可以用 Animator、Timeline 或事件来接真正动作。

## 第二幕/第三幕模型显隐

选中：

`MIN_LOOP_ROOT/ReplayRoom_17F02/HearthCompanion17F02ReplayController`

看 `Actor Visibility`：

- `Manage Actor Visibility`：当前应开启。
- `Bedroom Wife Actor`：卧室里的女主，当前是 `casual_Female_K (2)`。
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

现在动作还没有强制接 Animator。你可以先把模型放好，后面逐步把 Animator Clip 或姿势预设接到这些事件上。

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

- 女主人起身和走路的真实 Animator 动画。
- 更精细的门动画和门音效素材。
- 餐桌两人坐姿动画。
- 男主俯身操作陪伴单元 UI 的动画。
- 男主生气动作。
- 黑屏争吵的真实语音。
- 餐厅/客厅的空间触发范围限制。
- 第二户专属 A/B 文案细化。

这些都已经有接口，不需要重写第二户流程。
