# HEARTH 剧情变更记录

> 本文档记录用户口述的剧情、流程、演出和走位修改。它是制作过程记录，方便后续整理进正式策划案；除非后续条目标注“已同步实现”，否则这里只记录意图，不代表脚本或场景已经同步修改。

## 2026-07-02 17F02 剧情与游戏进程调整

### 本次记录范围

- 本次只记录剧情和走位意图。
- 暂不修改 C# 脚本。
- 暂不修改 Unity 场景。
- 暂不运行 `Tools / Hearth / Replay / Apply 17F02 Minimal Loop Setup`。
- 后续真正实现时，再把这些记录同步到字幕资产、音效事件、Timeline/Animator 或 `HearthCompanion17F02ReplayController` 的流程字段。

### 最新剧情顺序

1. 玩家从 17F02 终端进入陪伴单元机器人视角后，先进入一段黑屏。
2. 黑屏期间，玩家能听到女主刚进家门后，在客厅/房外和男主聊天的声音；字幕正常播放。
3. 男主按原剧本表达自己没空，随后女主进入卧室。
4. 女主进入卧室时，应该有门声、关门声、坐到床上的声音等演出。
5. 女主坐下后，用一段语音主动唤醒卧室里的陪伴单元。
6. 陪伴单元被唤醒后，进入女主对陪伴单元倾诉的环节。
7. 倾诉结束后，才开放玩家按 `E` 的安慰交互。
8. 玩家按 `E` 后，陪伴单元说出安慰内容。
9. 女主听到陪伴单元的安慰后，表现为得到了一些安慰。
10. 随后男主从餐厅/客厅喊女主吃饭，字幕正常显示。
11. 女主只回应男主，不再对陪伴单元说“等一下”或类似关照陪伴单元感受的话。
12. 女主直接起身离开卧室，走向餐桌。
13. 女主离开房间期间，陪伴单元不能移动、不能干预，只能看着女主走出去。
14. 女主最终停留在餐桌处，位置和朝向以 `casual_Female_K` 为准。

### 女主人离开卧室的走位记录

正式记录的路径顺序：

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

走位意图：

- `Path01 (1)` 到 `Path01 (5)` 都是用户新复制出来的细调锚点。
- 这些锚点用于控制女主人离开卧室时的位置和朝向。
- 更多路径点的目的，是减少穿模、控制转身方向，并让女主人从房间里走出来的路径看起来更丝滑。
- `DoorPause` 仍作为开门前的停顿点。
- `ExitOutside` 仍作为离开卧室后的终点。
- 后续如果继续使用脚本位移，应把以上路径点按顺序写入 `HearthCompanion17F02ReplayController / Wife Exit Path Points`，再单独绑定 `Wife Door Pause Anchor` 和 `Wife Exit Outside Anchor`。

### 后续实现提醒

- 黑屏期间的房外/客厅对话应优先写入 17F02 开场字幕资产，并预留 `Voice Clip`。
- 女主进入卧室的门声、关门声、坐下声应作为音效事件或 Timeline 事件接入。
- 女主离开卧室的动作后续由用户自己制作；Codex 后续实现时应尊重用户摆好的锚点和动画，不主动重写整段位移逻辑。
- 如果后续剧情文档、接口说明或脚本注释与本记录冲突，实现前先以本记录为最新意图，再询问是否同步到正式策划案或脚本。

## 2026-07-02 17F02 剧情与走位同步实现

### 已同步内容

- `17F02_BedroomWake.asset` 已改为黑屏开场：先播放男女主在房外/客厅的对话，再播放卧室门、关门、坐到床上的占位音效字幕，最后由女主唤醒陪伴单元。
- `17F02_BedroomComfort.asset` 已改为玩家按 `E` 后由陪伴单元安慰女主，随后女主表示得到一些帮助。
- `17F02_WifeExit.asset` 已去掉女主对陪伴单元说“等一下”的旧文本，改为男主喊吃饭、女主只回应男主。
- 当前打开场景中的 `HearthCompanion17F02ReplayController / Wife Exit Path Points` 已写入 6 个细调点：`Anchor_Wife_17F02_Path01` 到 `Anchor_Wife_17F02_Path01 (5)`。
- 当前打开场景中的 `Robot Controller (2)` 和 `Robot Controller (3)` 已作为参考控制器关闭，运行时仍只使用正式 `Player/Robot Controller`。

### 仍保留接口

- 门声、关门声、坐下声目前先用字幕占位，并保留每句字幕的 `Voice Clip` 字段；之后录音或音效素材准备好后，直接拖到对应 subtitle line。
- 女主离开卧室仍走脚本路径点位移；如果后续你自己做 Animator/Timeline，可以把 `Move Bedroom Wife To Door` 关闭，改由 Timeline 控制模型移动和开门。

## 2026-07-02 17F02 门时机与角色显隐修正

### 已同步内容

- `HearthCompanion17F02ReplayController` 新增 `Open Door After Path Point Count`，用于控制女主走到第几个路径点后先去门口开门，再继续走后续路径。
- 当前场景该值已设为 `5`：女主先走到 `Path01 (4)`，再去 `DoorPause` 开门，然后继续走 `Path01 (5)` 和 `ExitOutside`。
- 当前场景已开启 `Manage Actor Visibility`。
- 第二幕餐桌段只显示 `casual_Male_K` 和 `casual_Female_K`。
- 第三幕终端段只显示 `casual_Male_K (1)`，餐桌男女会隐藏。

### 后续调试提醒

- 如果女主仍然先穿门再开门，把 `Open Door After Path Point Count` 调小。
- 如果门开得太早，把 `Open Door After Path Point Count` 调大。
- 如果以后完全改用 Timeline/Animator 演出女主离开，可以关闭 `Move Bedroom Wife To Door`，让脚本只负责字幕、锁定和阶段切换。

## 2026-07-04 17F02 女主离开卧室路线简化

### 本次问题判断

- 用户在机器人视角观察到：女主有时先走出门外，再开门，然后又回到房间再走出去。
- 原因不是单个锚点错，而是旧逻辑把所有点放在一个数组里，再用 `Open Door After Path Point Count` 判断何时开门；当用户继续复制细调点时，很难直观看出哪些点属于门前、哪些点属于门后。
- 用户摆锚点时没有实体模型预览，也难以判断角色朝向，所以需要在 Scene 视图给每个路线点显示一个“线框小人”和朝向箭头。

### 新实现口径

- 女主离开卧室改为三段式：
  1. `Wife Before Door Path Points`：只放房间内、开门前的移动点。
  2. `Wife Door Pause Anchor`：女主停在门口，等待一小段时间，然后开门。
  3. `Wife After Door Path Points`：只放开门之后、穿过门和走出门外的移动点，最后再到 `Wife Exit Outside Anchor`。
- `Use Simple Wife Exit Route` 默认开启；旧的 `Open Door After Path Point Count` 只作为兼容模式保留。
- 如果当前场景还没有填写新的 Before/After 两个数组，脚本会临时把旧的 `Wife Exit Path Points` 自动拆分：路径点数量大于等于 6 时，前 4 个当作门前段，后面的当作门后段。
- 新增菜单 `Tools / Hearth / Replay / Migrate 17F02 Wife Exit Route To Simple Segments`，用于把当前场景已有锚点迁移到新三段式字段，并自动给锚点添加可视化朝向。

### 推荐调试方式

- `Path01` 到 `Path01 (3)` 建议先放入门前段。
- `DoorPause` 只负责开门前停顿，不要放在普通路径数组中反复经过。
- `Path01 (4)`、`Path01 (5)` 建议放入门后段。
- `ExitOutside` 只作为最后离开卧室后的最终位置。
- 如果女主又出现“走出去再回头”的感觉，优先检查是否有某个门外点误放进了门前段，或者某个门内点误放进了门后段。

## 2026-07-04 17F02 女主参考模型走位细调

### 本次制作口径

- 用户希望继续用复制出来的女主模型细调走位，因为实体模型比空锚点更容易判断身体位置和面朝方向。
- `casual_Female_K (2)` 是唯一正式卧室女主，运行时必须一直存在。
- `casual_Female_K (3)` 到 `casual_Female_K (9)` 都只是参考模型，不参与运行时显示、碰撞或剧情显隐。

### 新路线映射

```text
casual_Female_K (2) -> Actor_Wife_17F02_Bedroom
casual_Female_K (3) -> REF_Wife_17F02_BeforeDoor_01
casual_Female_K (4) -> REF_Wife_17F02_BeforeDoor_02
casual_Female_K (5) -> REF_Wife_17F02_BeforeDoor_03
casual_Female_K (6) -> REF_Wife_17F02_BeforeDoor_04
casual_Female_K (7) -> REF_Wife_17F02_BeforeDoor_05
casual_Female_K (8) -> REF_Wife_17F02_DoorPause
casual_Female_K (9) -> REF_Wife_17F02_ExitOutside
```

### 实现规则

- 以后细调 17F02 女主离开卧室时，优先移动和旋转 `REF_Wife_17F02_*` 参考模型。
- 调整完成后运行 `Tools / Hearth / Replay / Build 17F02 Wife Route From Female References`，由工具把参考模型同步到程序锚点。
- `REF_Wife_17F02_DoorPause` 只作为“停下来并开门”的位置，不放进普通路径数组，避免女主反复经过同一个门口点。
- 当前门后没有额外中间点，`REF_Wife_17F02_ExitOutside` 就是开门后走出房间的终点。
- 参考模型只用于编辑器观察位置和朝向，Collider / Rigidbody 碰撞必须关闭，避免挡住玩家操作的机器人。
- 旧的 `Anchor_Wife_17F02_Path01`、`Path01 (1)-(5)`、`Path02` 路线锚点已经淘汰，同步参考模型路线时应清理掉，后续不要再恢复这套旧流程。

## 2026-07-04 17F02 第三幕固定视角修正

### 本次问题判断

- 用户在第二幕餐桌观察结束前，如果最后把机器人视角看向地面，第三幕“男主调用记录”会沿用这个低头视角。
- 问题原因是流程只复制了第三幕 `Robot Controller (3)` 的身体位置和朝向，没有单独复制它子 Camera 的俯仰角。

### 新实现口径

- 第三幕固定视角由两个锚点共同决定：
  - `Anchor_Robot_17F02_LivingRoomTerminal`：正式机器人身体位置和水平朝向。
  - `Anchor_Robot_17F02_LivingRoomTerminalCamera`：正式机器人相机位置、俯仰角和最终画面朝向。
- 从第二幕进入第三幕时，流程会同时套用身体锚点和相机锚点，并关闭机器人移动/转头，确保第三幕视角保持为用户摆好的第三幕占位相机视角。
- 后续如果用户想调整第三幕视角，应先调整 `Robot Controller (3)` 和它子物体 Camera 的位置/角度，再运行 `Tools / Hearth / Replay / Apply 17F02 Minimal Loop Setup` 同步锚点。

## 2026-07-06 17F01/17F02 人物动作接入

### 本次制作口径

- 17F01 的人物模型、位置和朝向以用户当前场景为准，不再由脚本重新摆放。
- 17F01 只在进入机器人回放流程时播放用户已经设置好的循环动作：
  - `little_boy_B` 播放 `LayingSleeping`。
  - `casual_Female_G@Sitting_Idle` 播放 `SittingIdle`。
  - `casual_Male_G@Sitting` 播放 `Sitting`。
- 17F02 从旧的静态 Pose / 简单脚本走位，升级为“剧情流程控制 + 可替换动画 Clip + 锚点走位”。
- `SITTING TALKING ON CASUAL_FEMALE_K` 的实际导入文件按用户确认使用 `Assets/Animations/Hearth/17F02/Clips/Sitting_Talking.fbx`。

### 17F02 新动作流程

1. 从 17F02 终端进入机器人回放后，先进入黑屏阶段，播放房外/客厅男女主对话和女主进房相关字幕。
2. 黑屏恢复、陪伴单元被唤醒后，卧室女主 `Actor_Wife_17F02_Bedroom` 循环播放 `SittingDisbelief`。
3. 女主倾诉结束并等待约 `1.5s` 后，玩家才可以按 `E` 触发陪伴单元安慰。
4. 玩家完成安慰交互后，卧室女主切换为循环播放 `SittingTalking`，直到男主喊吃饭、女主准备离开。
5. 女主离开卧室时，依次播放 `SitToStand`、`StartWalking`，随后循环 `WalkLoop` 并沿用户参考模型同步出来的路线锚点移动。
6. 女主到 `Wife Door Pause Anchor` 后停下，播放 `OpenDoorOutwards`。该动作当前允许使用 root motion。
7. `OpenDoorOutwards` 开始约 `1s` 后调用 `Door_2_Brown (4)` 的开门逻辑；如果时机不准，后续调 `Door Open Delay After Animation Start Seconds`。
8. 开门动作结束后，流程关闭 root motion，并把女主移动/校正到 `Wife Exit Outside Anchor`，避免动画自带位移让后续站位漂移。
9. 第二幕餐桌阶段只显示 `casual_Male_K` 和 `casual_Female_K`，并播放基础坐姿循环。
10. 第三幕终端阶段隐藏餐桌男女，只显示 `casual_Male_K (1)`，并播放 `ButtonPushing` 基础循环。

### 后续实现提醒

- 如果用户之后替换 Mixamo 动作，应优先在对应角色根对象的 `HearthActorAnimationPlayer / Clips` 里替换 `Clip`，尽量不要改 `Clip Id`。
- 如果某段动作衔接不自然，优先调对应 Slot 的 `Fade Seconds`、路径参考模型的位置/朝向，以及 `Door Open Delay After Animation Start Seconds`。
- 如果 `OpenDoorOutwards` 的 root motion 偏移过大，可以先关闭该 Slot 的 `Apply Root Motion`，让角色完全按锚点移动。

## 2026-07-06 17F01/17F02 模型与动画绑定修正

### 本次问题判断

- 17F01 的 E 交互判定曾经跟随 `little_boy_B` 可见模型作为子物体存在；用户删除或替换该模型后，`Capsule Mesh (1)` 也会一起消失，导致机器人看向床边时无法触发 E。
- 17F02 同时存在正式卧室女主 `Actor_Wife_17F02_Bedroom` 和源动作模型 `casual_Female_K@Sitting_Disbelief`，两者位置接近，造成开场女主重叠。
- 17F02 流程仍在部分阶段先调用旧 `HearthActorPosePreset`，再播放新动画；旧 PosePreset 会暂停 Animator，容易让 Mixamo 动作不播放或看起来混乱。

### 新实现口径

- 17F01 的 `Capsule Mesh (1)` 改为独立交互判定物，挂在 `MIN_LOOP_ROOT/ReplayRoom_17F01/RuntimeInteractables` 下，不再作为小男孩可见模型的子物体。
- 17F01 绑定工具优先使用 `Laying_Sleeping` / `little_boy_B-Laying_Sleeping` 作为睡姿模型；旧 `little_boy_B` 根对象如果还在，会被关闭，避免叠模和误播动作。
- 17F02 有 `HearthActorAnimationPlayer` 且 Clip 存在时，不再执行旧 PosePreset；PosePreset 只作为缺少 Clip 时的兜底。
- 17F02 的真实演员显示规则固定为：卧室女主、餐桌男女、第三幕男主分阶段显示；源动作模型和参考模型不参与运行时画面。

## 2026-07-07 17F02 女主真实模型与动作绑定修正

### 本次补充修复口径

- 为解决玩家完成 E 安慰交互后女主消失、后续起身/走路/开门流程中断的问题，17F02 卧室女主改为“可见模型 + RuntimeRoot”分离结构。
- `Actor_Wife_17F02_BedroomRuntimeRoot` 是唯一运行时卧室女主演员根、显隐根和脚本移动根。
- `casual_Female_K@Sitting_Disbelief` 继续作为卧室可见女主模型，但必须作为 RuntimeRoot 的子物体。
- 女主所有卧室动作都在同一个可见模型上播放，不再切换到旧 `Actor_Wife_17F02_Bedroom`，也不再把 `casual_Female_K@Sitting_Disbelief` 当源动作对象自动隐藏。
- 女主的 `SittingDisbelief / SittingTalking / SitToStand / WalkLoop / OpenDoorOutwards` 都关闭 `Apply Root Motion`，并开启 `Stabilize Animator Transform`；世界位移完全由 RuntimeRoot 和 `REF_Wife_17F02_*` 路线锚点控制。
- `Wife Before Door Path Points` 固定为 `REF_Wife_17F02_BeforeDoor_01` 到 `05`；`Wife After Door Path Points` 当前为空；开门后直接到 `REF_Wife_17F02_ExitOutside`。
- 17F02 回放等待、女主移动、门开启动画延迟改为不受普通 TimeScale 影响；`Door_2_Brown (4)` 的 `SmartDoorController` 也开启 `Use Unscaled Time`。
- 女主移动流程新增防卡死保护：如果 RuntimeRoot 没有向目标锚点前进，会输出 Warning 并吸附到目标点继续剧情，避免整段回放卡住。

### 本次制作口径

- 本次只修改 17F02；17F01 已由用户自行解决，不再调整第一户逻辑。
- 卧室阶段的唯一真实女主固定为 `casual_Female_K@Sitting_Disbelief`。旧 `Actor_Wife_17F02_Bedroom` 不再参与流程，应保持 inactive。
- `casual_Female_K@Sitting_Disbelief` 不是源动作对象，也不能被自动关闭；它同时承担卧室真实角色、卧室位移根对象和卧室动作播放对象。
- `REF_Wife_17F02_*` 仍然只作为编辑器走位和朝向参考；Play Mode 下不显示、不碰撞、不参与剧情显隐。

### 17F02 新动作流程修正

1. 黑屏恢复、陪伴单元被唤醒后，`casual_Female_K@Sitting_Disbelief` 循环播放 `SittingDisbelief`，Clip 使用 `Assets/Animations/Hearth/17F02/Clips/casual_Female_K@Sitting_Disbelief.fbx / mixamo.com`。
2. 玩家完成 E 安慰交互后，卧室女主播放一次 `SittingTalking`，Clip 使用 `Assets/Animations/Hearth/17F02/Clips/Sitting_Talking.fbx / mixamo.com`。`Bedroom Talking Max Seconds` 默认 `10`，设为 `0` 或负数时等待完整动作/字幕。
3. 男主喊吃饭、女主回应后，女主播放 `SitToStand` 一次。
4. 离开卧室时不再使用 `Female_Start_Walking` 或旧 `locom_f_basicWalk_30f`，走路循环改为 `Assets/Animations/Hearth/17F02/Clips/casual_Female_K@Walking.fbx / Walking`。
5. 女主仍沿 `REF_Wife_17F02_BeforeDoor_*` 同步出的锚点移动到 `REF_Wife_17F02_DoorPause`。
6. 到门口后播放 `OpenDoorOutwards`，并按现有延迟调用 `Door_2_Brown (4)` 开门。
7. 开门结束后不再额外推算复杂门后路径，直接校正/移动到 `REF_Wife_17F02_ExitOutside`。
8. 第二幕餐桌阶段只显示 `casual_Female_K` 和 `casual_Male_K`，动作分别为 `Sitting` 与 `SittingIdle`。
9. 第三幕只显示 `casual_Male_K (1)`，动作是 `ButtonPushing`。

### 后续实现提醒

- 如果后续重跑 `Tools / Hearth / Replay / Apply 17F02 Minimal Loop Setup`，应确认卧室女主仍绑定到 `casual_Female_K@Sitting_Disbelief`，旧 `Actor_Wife_17F02_Bedroom` 仍为 inactive。
- 如果用户替换动作，只替换对应 `HearthActorAnimationPlayer / Clips` 里的 Clip，尽量不要改 `Clip Id`。
- 如果女主路线穿模，先移动 `REF_Wife_17F02_*` 参考模型，再运行 `Tools / Hearth / Replay / Build 17F02 Wife Route From Female References` 同步锚点。
## 2026-07-07 17F02 动作播放规范修正记录

### 问题现象

- 玩家在 17F02 卧室完成 E 安慰交互后，女主没有播放 `SittingTalking`。
- 后续也没有正常播放 `SitToStand`、`WalkLoop`、`OpenDoorOutwards`，看起来像是一直保持坐姿移动出门。
- 第二幕餐桌男主/女主和第三幕男主也没有稳定播放指定动作。

### 本次原因判断

- 之前 17F02 使用 Playables 直接播放 FBX Clip，但这些 Mixamo FBX 原本是 Generic 导入，不能稳定作为 Humanoid 动作重定向到真实角色。
- 卧室女主运行时绑定到的 Animator 曾经是 `avatar=null / controller=null`，不符合 Unity 角色动画规范。
- 用户复制模型来调位置和朝向的方式可以保留，但这些复制模型只能作为参考锚点，不能作为运行时演员或动画目标。

### 最新制作规则

- 17F02 角色动画改为 Unity 标准 Humanoid / Animator Controller 流程。
- 真实演员只使用：
  - 卧室女主：`Actor_Wife_17F02_BedroomRuntimeRoot` 下的 `casual_Female_K@Sitting_Disbelief`
  - 餐桌女主：`casual_Female_K`
  - 餐桌男主：`casual_Male_K`
  - 第三幕男主：`casual_Male_K (1)`
- `REF_Wife_17F02_*` 只作为用户调走位和朝向的参考模型；Play Mode 下不显示、不碰撞、不参与剧情显隐。
- 17F02 的动作状态现在由 `HearthActorAnimatorDriver` 播放，绑定工具自动生成 `Assets/Animation/Hearth/17F02/*.controller`。

### 最新动作顺序

1. 卧室女主循环 `SittingDisbelief`。
2. E 安慰后播放一次 `SittingTalking`。
3. 离开前播放 `SitToStand`。
4. 循环 `WalkLoop`，沿参考模型生成的锚点移动。
5. 门口播放 `OpenDoorOutwards`，约 1 秒后开 `Door_2_Brown (4)`。
6. 开门动作结束后直接校正到 `ExitOutside`。
7. 第二幕餐桌男女播放 `Sitting / SittingIdle`。
8. 第三幕男主播放 `ButtonPushing`。

### 后续实现提醒

- 以后新增或替换 Mixamo 动作时，优先检查 FBX 是否为 Humanoid，Clip 是否 `human=True`。
- 如果动作不播放，先检查演员 Animator 是否有有效 Avatar 和 Controller，而不是先改剧情流程。
- 如果女主走位不准，先移动 `REF_Wife_17F02_*` 参考模型并重跑路线同步菜单，不要把参考模型改成运行时演员。

## 2026-07-09 17F02 / 17F03 门口 TV 终端入口对调

### 本次制作口径

- 本次只交换两台门口 TV 的终端内容和回放住户 ID。
- 不修改地面 `LOCATION` 判定、不修改房间 Mesh 归属、不修改玩家 HUD 的历史列表或住户排序。
- `17F/ROOM3/TV (2)` 现在作为 17F02 第二关卡入口，挂载 `Terminal_17F02`。
- `17F/ROOM2/TV (4)` 现在作为 17F03 Alert 终端入口，挂载 `Terminal_17F03_Alert`。

### 已同步到当前场景

- `17F/ROOM3/TV (2)/MonitorCanvas` 下只保留 `Terminal_17F02`。
- `17F/ROOM3/TV (2)/MonitorCanvas/Terminal_17F02` 的 `replayResidentId` 显式设置为 `17F02`。
- `17F/ROOM2/TV (4)/MonitorCanvas` 下只保留 `Terminal_17F03_Alert`。
- `17F/ROOM2/TV (4)/MonitorCanvas/Terminal_17F03_Alert` 的 `replayResidentId` 显式设置为 `17F03`。
- `17F/ROOM1/TV (3)` 的 17F01 终端入口保持不变。

### 后续实现提醒

- 因为终端控制器会从父级 `ROOM2 / ROOM3` 名称推断住户 ID，所以这两个对调后的终端必须保留显式 `replayResidentId`。
- 后续如果重新运行 TV 标准化工具或重新拖入终端 Prefab，需要再次确认 `ROOM3/TV (2)` 是 `17F02`，`ROOM2/TV (4)` 是 `17F03`。

## 2026-07-10 17F03 剧情定稿 v2 与动作前期准备

### 本次制作范围

- 剧情来源：`C:/Users/彩笔/Downloads/剧情对话稿_已为您沟通_玩法整合定稿v2.md`。
- 17F01 与 17F02 的具体场景和已制作流程冻结，本次不再根据新稿回改前两户。
- 当前进入 17F03 的动作素材选择与场景演出准备阶段；本条仅记录制作意图，暂不创建 17F03 的正式流程脚本或场景绑定。

### 17F03 当前剧情流程

1. Mia 在门口收到家庭陪伴单元离线预警，入户检查。
2. 母亲从客厅主动迎上来说明机器故障；父亲从沙发起身补充情况；女儿房门关闭。
3. 机器人回放为中午客厅冲突：母亲在沙发，女儿在地毯，陪伴单元站在两人中间替双方传话，冲突被表面降级。
4. 当晚女儿从房间走到陪伴单元面前，倾诉后打开陪伴单元屏幕，进入维护菜单并关闭核心服务，机器人视角进入深度休眠。
5. Mia 回到屋内终端作出处置：A 重启单元；B 暂停修复并进入七天人工观察。

### 动作制作原则

- 机器人第一人称难以传达细微表情，优先使用玩家能直接读到的大动作：起身、走近、转向、指向、停顿、操作屏幕、离开。
- 母亲、父亲与女儿的台词仍是情绪信息的主体；动作只加强人物关系和节奏，不能要求玩家从微小表情读剧情。
- 待选择的 Mixamo 动作重点：母亲沙发坐姿与愤怒手势、父亲坐姿起身、女儿地板坐姿/站起/走向陪伴单元/操作屏幕/转身离开。
- 具体下载、导入和 Animator 接入将在动作素材确认后单独实施。

## 2026-07-11 三户交互开放条件实现校正

### 记录范围

- 本条不改变三户剧情文本，只校正已经确定的“什么时候允许玩家交互”。
- 17F01、17F02 与 17F03 继续使用各自现有剧情和动作流程。

### 当前统一口径

- 17F01：卧室前奏字幕和额外等待结束后，才启用小男孩身上的独立胶囊判定；完成交互后立即关闭，不能提前或重复触发。
- 17F02：女主人倾诉字幕结束并再等待约 `1.5s` 后，才显示陪伴单元的长按 E 安慰；必须完成 100% 才继续剧情。
- 17F03：米娅进入屋内后，必须先听完父亲和母亲第一幕对话；对话结束后，实体陪伴单元才显示 E 检查提示。
- 17F03 第二幕面向女儿/母亲的传话仍使用中下方长按提示；只有当前剧情指定目标可以触发。

### 技术说明

- 17F01 射线长度保留用户已调好的 `0.71`，此次问题不是通过加长射线解决。
- 交互体在不可用阶段会同步关闭 Collider，在开放阶段重新启用，避免不可见 Collider 状态残留导致提示消失。
- 本条已经同步到当前脚本和 `SampleScene`；不是仅记录、未实现的剧情意图。

## 2026-07-11 17F03 第三幕女儿动作与关机时机校正

### 最新演出顺序

- 第三幕开门完成后，女儿播放 `Walk` 并沿夜间路径走到陪伴单元前的最终锚点。
- 抵达最终锚点后立即结束走路状态，循环播放 `Talking`；此阶段不能随机切回坐姿或继续走路。
- 女儿完成夜间倾诉后，陪伴单元先说出回应，女儿说 `Enough.`。这一小段仍保持 Talking。
- 只有进入“打开显示屏并操作维护菜单”的剧情段时，女儿才播放一次 `EnteringCode`，随后进入深眠流程。

### 显隐规则

- 第三幕只显示正式女儿演员；第一幕父亲、第二幕父亲和母亲均隐藏。
- 正式米娅控制器的占位胶囊、正式机器人控制器的占位胶囊，以及 `Person Controller (1)`、`Robot Controller (4)/(5)` 参考控制器胶囊在 Play Mode 中都不可见。
- 参考控制器只保存位置、朝向和相机参考，不作为运行时演员或可见模型。

### 实现记录

- 已关闭 17F03 正式演员上的第三方 `CityPeople` 随机自动动画，避免它覆盖剧情指定动作。
- 关机段拆为 `17F03_NightShutdownLeadIn` 与 `17F03_NightShutdownAction` 两个可编辑字幕/语音资产，后续加入语音后仍按真实播放完成时机推进。
- 本条已同步到脚本、Dialogue Sequence 和当前 `SampleScene`。

## 2026-07-12 17F03 母亲起身位移保留

### 演出调整

- 第一幕中，母亲播放 `SitToStand` 时，动作自带的水平位移应被保留；她完成起身后进入 Talking，不能瞬间回到起始坐姿位置。
- 该位移只服务第一幕的连续演出。进入第二幕时，母亲仍按既定流程切换到 `Anchor_Mother_17F03_Midday`，作为新的冲突场景站位。

### 实现记录

- `HearthActorRootMotionRelay` 将动画模型的实际水平位移转存到 `Actor_Mother_17F03_RuntimeRoot`，并保持模型子节点本地位置稳定。
- `casual_Female_G@Sit_To_Stand.fbx` 已解除 XZ Root Motion 的 Bake Into Pose；母亲 `SitToStand` State 启用 Root Motion，Talking 与 StandingArguing 不启用。
- 这是当前 17F03 的正式演出规则；后续替换起身动作时应重新运行 17F03 Setup 与 Validate 菜单。

## 2026-07-14 17F03 门开启方向与实体检查镜头表现

### 门开启

- 第三幕中女儿打开 `17F/ROOM2/Door_2_Brown (7)/DoorHinge_17F03` 时，门扇应绕现有右侧铰链旋转，朝远离墙体的一侧打开。
- 场景检查确认旋转轴本身无误；此前问题来自旋转门完成时被脚本额外套用了滑动偏移，视觉上像门扇卡进墙里。
- 当前正式设定为 `Motion Mode = Rotate`、`Open Local Euler Offset = (0, -90, 0)`，门只旋转，不产生额外位移。

### 实体陪伴单元检查

- 第一幕父母对话结束后，米娅面向实体陪伴单元按 E 进入检查时，改为和门口终端一致的短距离平移镜头。
- 默认进入与退出各约 `0.5s`；检查界面本身不再叠黑场。其他剧情幕次的渐黑/渐亮切换保持原样。
- 本条是当前第三户正式演出规则，后续只需在 `UnitInspectionCameraTransition_17F03` 调节节奏，不需改台词或交互顺序。

## 2026-07-14 17F03 交互限制与 17F04 自宅终局

### 17F03 最新规则

- `Door_2_Brown (7)` 不再允许玩家直接按 E 开关；第三幕剧情仍能正常调用开门与关门。
- 门板的真实侧边轴命名为 `DoorHinge_17F03`，向外旋转 `-90` 度，门只旋转、不额外平移进墙。
- 第一幕米娅检查实体陪伴单元时，以约 `0.5s` 从当前视角平移到 `ROBOT/Camera (1)`；Esc 反向返回。引用缺失才使用黑场兜底。

### 17F04 正式游戏顺序

1. 玩家在 `TV (3)` 打开自宅入口终端，Space 确认后渐黑进入客厅。
2. 客厅阶段必须检查 `TV (4)` 上的圣诞合照；镜头平移到相框正面，照片对白完成后 Space/Esc 返回。
3. 相框与后续客厅对白完成后，`Door_2_Brown (1)` 才显示 E；门板不播放动画，流程渐黑传送到女儿房间。
4. 女儿房间正式对白播放期间，玩家可以自由移动、转动视角和观察房间；对白本身不会锁死人物。
5. 对白结束后显示最终选择：A `ANSWER LILY YOURSELF`，B `LET THE COMPANION ANSWER FOR HER`。此时才锁定移动和视角。
6. A 路线由米娅亲自回答，随后恢复自由移动并开放 `ROBOT (1)`；玩家走近、准星命中后按 E 进入关闭流程。
7. B 路线由陪伴单元代为回答，不开放机器人关闭交互，直接进入对应结局。
8. 关闭流程读取既有信任：正信任一次 Space；负信任进入病毒警告压力流程，玩家需持续按 Space 清除从屏幕四周生成的弹窗。
9. 四种组合进入独立居中黑幕文本，结束后返回走廊并触发终局完成事件。

### 信任与结局规则

- 最终 A/B 不再增加或减少信任，只读取前三户累计结果。
- `> 0` 为高信任，`< 0` 为低信任。
- 分数恰好为 `0` 只作为测试边界：默认使用高信任预览并输出警告，也可在 Inspector 强制 High/Low。
- A/B 同一轮只允许提交一次，不能通过重复 Space 重复触发对白或结局。

### 资源与后续演出说明

- 17F04 当前使用 `Girl_A_Rigged` 的既有位置和待机表现，本轮不新增角色动画。
- 当前只制作 TV4 的圣诞合照；第二张照片仅保留数据与交互扩展入口。
- 四种结局和各阶段对白已拆成 `Assets/Data/MinLoop/Dialogues/17F04/*.asset`，可继续修改文本、停留时长和音频，不需要改状态机。
- 低信任关闭已改为病毒警告弹窗压力流程；旧三次确认和滑块/中央判定方案不再使用。
- 本条已经同步到脚本、场景、Prefab 和 Dialogue Sequence，不是只记录未实现的剧情意图。

## 2026-07-15 17F04 自宅入口镜头衔接修正

### 正式表现

- 玩家看向第四户门口 `TV (3)` 按 E 后，仍以约 `0.5s` 平移进入终端固定视角。
- 在终端页面按 Space 进入自宅时，渐黑必须直接发生在终端固定视角上；不能先退回按 E 前的米娅视角再渐黑。
- 黑幕完全覆盖后才在幕后恢复正式人类相机、移动到客厅 Anchor，随后从黑幕渐亮。
- 进入客厅与女儿房间后，第一人称相机必须绕玩家头部/控制器中心原地转动，不能以较大半径环绕胶囊体。

### 已实现记录

- TV3 Custom Action 改为“等待外部黑幕完成后再关闭终端”，并阻止等待期间重复 Space。
- 17F04 传送统一保留正式玩家相机的本地枢轴偏移，目标 `CameraPose` 只决定到达视角和相机世界位置。
- 本条只修正镜头衔接与第一人称旋转中心，不改变 17F04 的对白、相框、房门、A/B、信任分支和结局顺序。

## 2026-07-15 17F04 猫咪空间引导与最终选择操作

### 最新剧情与操作口径

- 玩家进入客厅渐亮后，猫咪立即从 `CatMoveRoot` 出发，沿 `CatMoveRoot (1)-(7)` 移动到相框附近沙发，用空间演出引导玩家注意 TV4 相框。
- 猫咪只负责引导，不是剧情门槛。玩家进入客厅后可立即检查相框，不必等待猫咪到达沙发。
- 猫咪前六段走路，最后一段以带弧线的跑跳到达沙发；到达后从 `Lie_to` 过渡到循环 `Lie_idle`。
- 欢迎对白期间玩家也可以立刻进入相框视角。欢迎对白继续播放完毕，相框对白随后自动接续；两套字幕不得重叠或丢失。
- 相框对白完成并退出后，才继续女儿房间的声音、房门解锁和后续流程。
- 第四关最终 A/B 只使用 `↑/↓` 改变高亮，按 `Space` 确认。`←/→`、字母 `A/B`、`Enter` 在该页面不生效。
- 默认高亮 A，同一轮仍只允许提交一次；离开第四关流程后恢复前三户原有输入规则。

### 猫咪路线时长

1. 起点到 1：`3s / Walk_F`
2. 1 到 2：`3s / Walk_F`
3. 2 到 3：`3s / Walk_F`
4. 3 到 4：`3s / Walk_F`
5. 4 到 5：`15s / Walk_F`
6. 5 到 6：`1s / Walk_F`
7. 6 到 7：`1s / Run_F + 抛物线落点校正`

### 制作规则

- `CatMoveRoot` 是唯一运行时猫；`CatMoveRoot (1)-(7)` 只保存位置和朝向，Play Mode 中隐藏且无碰撞。
- 调整参考猫 Transform 后无需同步菜单，下一次运行直接读取新值。
- 本条已实现到脚本与当前 `SampleScene`，不是只记录未实现的演出意图。

## 2026-07-16 全局 UI、交互文字与对白音频接口整理

- 本次不修改 17F01-04 的剧情顺序，只完善跨关卡系统。
- 米娅 Tab 菜单的处置历史改为显示已完成住户、真实当前信任与本轮记录增减总和；设置页四个音量值正式生效并保存。
- 所有单按 E 的运行时提示统一为 ASCII 英文，清理场景中旧中文序列化提示，避免 TMP 显示方框；长按 E 的 0-100% 交互保持原样。
- 人类与机器人脚步声拆成两个独立 Profile，后续可以分别替换素材、速度和音量。
- 所有 `HearthDialogueSequence` 每句新增语音时长模式与语音尾停顿，允许继续自由修改文字、句数、顺序、时长和 AudioClip。
- 当前只完成语音接入口，尚未加入真实录音；现有 215 个逐句 Voice Clip 槽位均为空。

## 2026-07-16 17F04 猫咪移动节奏修正

- 不改变猫咪引导的剧情功能、路线点、跳上沙发和最终趴卧顺序。
- 前六段 `Walk_F` 改为原地动作，取消动画自身的 XZ 根位移，猫咪世界移动只由路线父级控制。
- 最后一段 `Run_F` 保持原动作设置，继续配合 6 到 7 的抛物线落点校正。
- 路线整体加快 2 倍：时长调整为 `1.5 / 1.5 / 1.5 / 1.5 / 7.5 / 0.5 / 0.5` 秒。
- 本条已经同步到动画导入设置、绑定工具和当前 `SampleScene`，不是只记录未实现的节奏意图。

## 2026-07-16 17F04 猫咪移动连续性修正

- 不改变猫咪路线点、剧情触发、到达沙发后的趴卧顺序，也不覆盖用户调整过的参考猫 Transform。
- `Walk_F` 循环改为 Unity 原生无缝循环，取消每轮结束时的时间硬归零。
- 猫咪经过路线点时改为连续速度曲线和平滑朝向过渡，减少急转与相邻段速度变化造成的停顿感。
- `Path Smoothing` 作为 Inspector 可维护参数保留；当前正式值为 `0.75`。

## 2026-07-16 分关卡音效空槽与猫咪步频修正

- 本次只建立音效系统和发声位置，没有选择、下载或导入实际 AudioClip，也不处理对白配音。
- 明确排除床单/床铺摩擦、小男孩呼吸/翻身、全部猫咪声音。
- 17F02 自动接入女主起身、跟随移动、餐桌 Foley、数据调取、故障和关机；女主坐床只保留 `TBD` 空槽，等待精确时刻确认。
- 17F03 自动接入母亲起身、女儿起身、女儿跟随移动、输入代码、故障和深眠关机。
- 17F04 自动接入相框记忆提示和陪伴单元最终关机；猫咪不发声。
- `Walk_F` 动作频率改为 `2.0x`，只加快走路动作；`Run_F` 和趴卧动作保持原速。
- 猫咪路线仍实时读取 `CatMoveRoot (1)-(7)`；用户调整过的第 4、5、6 点位置和朝向不被绑定菜单覆盖。
- 旧 Legacy `Animation` 已从正式猫和参考猫移除，避免与 Playables 动作冲突和 `Drop_L_sit` 报错。

## 2026-07-17 四户对白表现、第一户交互与第四户关闭流程修订

- 17F01 小男孩段落只保留一次长按 E；完成到 100% 后直接继续安抚和后续剧情，不再要求第二次重复长按。
- 17F01-04 所有普通对白统一使用同一个中下部字幕播放器和共享样式资产；第三户不再误用第四户的中央黑幕字幕。
- 普通对白的说话人位于正文上方，正文限制在屏幕约三分之二宽度内居中换行；最终黑幕仍保留正中央模式。
- 17F03 女儿关闭陪伴单元时，蓝色/红色故障信息的标题、正文、状态和强调线统一水平居中。
- 17F04 TV3 的 Space 入户确认、陪伴单元关闭过程的 Space 操作均增加短暂灰显反馈。
- 17F04 低信任关闭不再使用三段固定确认，也不制作旧滑块小游戏。新流程会从屏幕四面连续生成警告浮窗；Space 每次关闭一个，生成结束且全部清空后才完成关机。
- 17F04 低信任浮窗所有文字必须留在各自窗口内，允许窗口互相覆盖制造失控感，但不允许文字跑出窗口边界。
- 本条已同步到脚本、Prefab、当前场景和 UI 资产，不是只记录未实现意图。

## 2026-07-17 17F02 演出修正与 17F04 关闭/猫咪节奏二次调整

- 17F02 女主在 Sitting、Talking、起身、走路和开门动作之间改用较缓的混合过渡，避免切换时像抽动一样突然跳姿势；同一规则作为共享演员默认值供其他关卡使用。
- 17F02 女主的开门动作开始 `0.5s` 后即打开实体门，比旧触发提前半秒。
- 女主出门完成后保持在 `ExitOutside`，不再短暂闪回门后半个身位。
- 17F02 第三幕只保留家庭记录投影面板，删除与其重叠的中央蓝色大字。
- 17F04 低信任关闭改为三轮不同警告：第一轮蓝色、第二轮橙色且更快、第三轮红色且最快。每轮先关闭一个主浮窗，再持续清理该类型不断生成的浮窗；浮窗标题不显示编号。
- 猫咪前六个 Walk 路段在当前速度基础上再加快 3 倍，RunJump、落点校正、Lie_to 和 Lie_idle 保持原速度。
- 猫咪路线继续实时读取用户调整后的 `CatMoveRoot (1)-(7)`，本次没有覆盖第 4、5、6、7 点的位置或朝向。
- 本条已同步到脚本、当前场景、HUD 数据和生成工具，不是只记录演出意图。

## 2026-07-17 17F04 猫咪速度更正与 1F 大堂流程意图

### 17F04 猫咪

- 上一次“再加快 3 倍”为口述错误，正式口径改为在当时基础上加快 `1.5` 倍。
- 只缩短 Walk 路段；RunJump、落点、Lie_to 和 Lie_idle 保持原样。
- Walk 腿部动作步频恢复为调整前的独立值，不再额外乘路线速度倍率。
- 本条猫咪修正已实现到脚本、绑定工具和当前场景。

### 1F 大堂（制作意图，暂未接入脚本和场景）

- 玩家出生在封闭的一楼大堂内，玻璃门暂时阻止玩家直接离开。
- 大堂依次分布三组世界观对白：左侧小女孩与公共陪伴单元、前方年轻/中年男人与工作辅助单元、更深处老奶奶与护理单元。
- 对白文本以 `HEARTH_Full_Game_Script_Native_English_Polished.md / Scene 1.1` 为当前来源。
- 期望玩家完整听完三组对白；每组进入指定范围后开始，正式播放期间玩家不能移动，但可以自由转动视角。
- 对白结束后该组进入完成状态并恢复移动；三组均完成后再开放远端 Console、玻璃门或下一段流程。
- 触发范围和玩家提示方式尚未正式实现，后续优先采用“外层提示范围 + 内层正式对白范围”的两级 Trigger 方案。

## 2026-07-18 1F 大堂开场、任务终端与电梯流程正式接入

本条覆盖上一条中“尚未接入”和“三组都完成后才继续”的旧意图。

### 正式剧情顺序

- 游戏正式出生点改为一楼大堂 `Player/Person Controller (4)` 的位置与相机朝向，不再从 17 楼开始。
- 开场对白期间米娅不能移动，也不能转动视角；对白结束后才恢复完整控制。
- 大堂三组世界观对白为自由探索内容，可按任意顺序触发，也不要求全部完成：
  - `Girl_A_Rigged (2)/space`：小女孩与公共陪伴单元。
  - `casual_Male_G@Sitting (1)/space1`：年轻男人与工作辅助单元。
  - `Sitting_Idle (2)/space2`：老奶奶与护理单元。
- 玩家进入任一触发范围后，该段对白只播放一次；播放期间锁定移动，但允许自由转动视角。对白完成后恢复移动。
- 三组 NPC、机器人和触发范围均使用用户当前摆放的位置，不由运行时脚本移动。
- 一楼同步终端为 `1F (1)/TvUnitSet5`。玩家看向终端并按 E 后，镜头用约 `0.5s` 平滑移动到终端 Camera，并播放开机效果；按 Space 正式领取 17 楼巡查任务。
- 电梯按钮为 `DIKUAIunity/Group1/Group144/Rectangle2106772232/Mesh2643`。领取任务前不可用；领取后看向并按 E，渐黑传送到 `Player/Person Controller (5)`，播放电梯对白，再渐黑抵达用户已调整的 17 楼出生位置。
- 所有跨空间切换继续使用约 `0.5s` 渐黑/渐亮；终端本身仍使用平滑镜头进入，不用硬切。

### 对白与后续语音

- 新的正式稿 `HEARTH_Full_Game_Script_Expanded_Native_English_Lobby_Mia_Commentary.md` 已复制到项目根目录，并成为目前全部关卡对白的唯一正式来源。
- 一楼新增对白与 17F01、17F02、17F03、17F04 已有对白均已同步为 `HearthDialogueSequence` 数据。
- 当前没有正式语音文件；每句对白均保留独立 `AudioClip` 槽。以后拖入语音后，字幕会按该音频实际长度显示；无音频时使用可编辑的回退时长。
- 终端分支提示和签退类对白也已生成补充 Dialogue Asset；尚未有正式运行节点的内容保留为数据接口，不会自行抢占当前剧情。

### 既有流程校正

- 17F03 实体陪伴单元检查页在允许 Space 调取记录前，先播放正式稿中的本地记录提示。
- 17F04 从自宅门口终端确认进入后，问候对白保持在终端固定摄像机画面中播放；对白完成后才从该画面渐黑并进入客厅，不再先闪回米娅原视角。

## 2026-07-18 任务终端、全局字幕与三户处置流程修订

### 一楼任务终端

- `1F (1)/TvUnitSet5` 的 World Space Canvas 在关闭状态完全隐藏；只有玩家按 E 后才启用并播放镜头平移、开机闪烁和正式页面。
- Canvas 使用可编辑的 `TaskTerminalScreenAnchor` 对齐实体屏幕。用户已调整的终端 Camera 位置、旋转和 FOV 不由绑定工具覆盖。
- 这项修订只改变关闭时的可见性，不改变领取任务、Space 确认或电梯解锁顺序。

### 正式对白与字幕

- 根目录最终稿中的长对白已按自然标点拆为独立字幕段，不删词、不改写语气与顺序。
- 普通对白和黑幕对白都限制为最多两行；每个拆分段保留独立语音槽，有语音时按实际音频长度播放。
- 最终稿与 Dialogue Asset 改为隐藏稳定标记映射，不再依赖段落数字下标。当前正式稿共有 `343` 个字幕段，映射到 `59` 个 Dialogue Asset。

### 一楼留言与可选剧情

- Lily 留言在开场消息后保持缩小显示，直到 Mia 说完 `Okay.` 才永久隐藏本轮；终端、实体检查和机器人视角不会再次显示。
- 三组大厅剧情改为“区域内播放 NPC 对话 -> 对话结束恢复移动 -> 玩家离开 Trigger 后播放 Mia 感想”。
- Mia 感想期间玩家可以继续移动和转头；若立刻进入下一组，下一段 NPC 对话等待当前感想结束，避免字幕或未来语音重叠。

### 17F01 / 17F02 处置闭环

1. 回放结束后仍返回现有门口终端。
2. A/B 页面先显示但暂时锁住输入。
3. Field Unit 播放正式稿中的推荐后才开放选择。
4. Space 提交后立即锁定，信任度和历史只结算一次。
5. 播放该选择的评价与下一户指引，全部结束后才关闭终端并恢复控制。
6. 完成后分别登记 `17F01` 或 `17F02` 到住户进度状态，重复调用不重复记录。

### 17F03 回放与房内处置

- 实体机器人检查说明尚未结束时按 Space，会立刻灰显并排队；说明结束后自动进入记录回放，重复 Space 不会重复启动。
- 回放结束后返回米娅所在住宅内，先播放技术说明；说明结束后才重新开放实体机器人 E。
- 米娅再次检查实体机器人时平滑进入固定摄像机，在房内使用 `↑/↓ + Space` 选择 A/B。A 默认绿色高亮并标记 `RECOMMENDED`。
- 提交后平滑返回米娅房内视角，再播放对应父母与 Field Unit 评价。B 且结算后信任小于 0 时，追加主管复核警告。
- 分支结束后渐黑到 `Anchor_Mia_17F03_DoorReturn`，恢复普通人类控制；不自动打开任何门口终端。
- 第三户信任结算、历史事件和完成登记都只允许一次。

## 2026-07-19 任务终端显示、低信任遮罩、音频接口与项目整理

- 本条主要是制作系统维护，不改写正式剧情台词或关卡顺序。
- 一楼任务终端在 Edit Mode 保持可见，便于调整 `MonitorCanvas`；Play Mode 未按 E 时完全隐藏，按 E 后才显示并开机。
- 当前 `MonitorCanvas` 与 `TaskTerminalScreenAnchor` 仍有小幅位置/旋转差异；没有修改用户手调的终端 Camera，也没有自动覆盖 Canvas。后续由用户选择“对齐到 Anchor”或“把当前 Canvas 捕获为新 Anchor”。
- Lily 留言关闭条件确认不变：Mia 说完 `Okay.` 后调用 `DismissVoiceMessage()`，不会延迟到终端或机器人阶段。
- 17F04 低信任弹窗增加半透明黑色全屏遮罩，默认 Alpha `0.62`；弹窗出现、关闭、波次升级和清空均新增独立音效接口。
- 全游戏剧情音效槽扩展为 45 个，并补齐一楼大厅、电梯、任务终端、17F01、17F04 弹窗与环境保留槽；本次不导入或擅自选择最终 AudioClip。
- SampleScene 根层级已按 UI、玩家、系统、正式场景、环境、松散演员、待审对象分区。仅调整顺序，不改 Transform 或父级。
- `Assets` 根目录散落的动作、角色、脚本和 UI 图片已按功能移动并保留 GUID；第三方资源包和场景家具不移动、不删除。
- `little_boy_B`、`GameObject`、`Plane`、旧 `1F` 只移入 Review 分区，等待后续实机确认，没有删除。

## 2026-07-19 一楼任务终端一次性领取与正式稿流程校准

### 一楼任务终端

- 玩家在任务终端按 Space 成功领取任务后，终端关闭并在本轮永久失效；再次看向它不会出现 E，也不能重新打开。
- 为避免误按 Esc 直接造成整局软锁，只有“成功领取”才消费终端；未确认前取消仍可重新进入。
- 终端关闭后的任务说明对白期间，米娅可以移动和转动视角，但所有 E 交互暂时关闭。
- 电梯不会因为 `AssignmentLoaded` 已经写入就提前开放；它还会等待任务说明全部结束、Lobby Flow 离开 Busy 状态后才显示 E。

### 正式剧本文档

- 根目录正式稿已校准 Lily 留言：播放时展开，之后缩成右上角已读卡片，保持到 Mia 说完 `Okay.`，随后关闭；终端、检查摄像机和机器人回放视角均不显示。
- 正式稿已补充大厅三组可选对白的“进入区域锁移动、保留视角、离区后播放 Mia 感想”规则。
- 正式稿已补充一楼任务终端的 E/Space、一次性消费、对白期间可移动以及电梯延迟解锁规则。
- 正式稿已把 17F01/17F02 改为当前门口终端回放与处置流程，把 17F03 房内检查/处置和 17F04 固定终端画面内完成问候后再渐黑入户写为当前版本。
- 本次只校准流程说明和演出描述，不擅自改写正式英文对白原句，也不改变隐藏的 `HEARTH:SEQUENCES` 映射。

## 2026-07-19 全局 HUD、终端引导与结局时间提示

### 一楼大厅与任务终端

- 大厅开场 `FIELD COMPANION UNIT / ACTIVATED` 卡片下移，避免与米娅左上常驻身份 HUD 重叠。
- 一楼任务终端开机完成、任务页面真正可见后立即开始 Field Unit 简报。
- 前 5 秒显示 `PLEASE WAIT`，Space 和 Esc 都不能关闭；5 秒后显示 `SPACE  CLOSE TERMINAL`。
- 关闭终端后简报继续，米娅可移动和转头，但其他 E 交互与电梯继续锁定；最后一句结束才全部开放。
- 正式稿补充路线指引：`Route loaded. Proceed to the elevator and call it when you're ready. Destination: Floor Seventeen.`
- 成功领取后任务终端本轮不可再次打开；用户已调终端 Camera、ScreenAnchor 与 Canvas 不由绑定工具覆盖。

### 住户终端与处置提示

- 17F01、17F02、17F03 在每轮第一次完整打开终端时播放住户/任务简介。
- 简介期间允许 Tab 浏览资料，但回放、入户或提交操作锁定；提前 Esc 会取消本次简介，重新打开从头播放。
- 简介完整结束后本轮不再重复，并自动开放主操作。
- A/B 页面统一增加明确键位提示：17F01/02 使用左右键，17F03/04 使用上下键，Space 确认；等待时显示 `PLEASE WAIT`。
- 17F02 对 Claire 的长按文案固定为 `HOLD E  OFFER REASSURANCE TO CLAIRE`，不再显示方框或无意义符号。

### 字幕、留言与结局时间

- 大厅 Lily 卡片、第四关终端记录与正式稿统一为 `4:42 PM`。
- 第四关进入终端后先由 Field Unit 说明这是大厅收到的完整留言；第一次确认后按钮灰显为 `PLEASE WAIT`，重复 Space 不跳过留言。
- 相框对白完成后明确显示 `SPACE  RETURN`；Esc 仅作为隐藏的安全退出键。
- 第四关关闭路线依次显示 `MORNING - KITCHEN`、`DAYTIME - HOME`、`NIGHT - LILY'S ROOM`。
- 保留路线依次显示 `THE NEXT MORNING - KITCHEN`、`AFTERNOON - SCHOOL OPEN HOUSE`、`THREE YEARS LATER - FRONT HALL`。
- 时间提示使用独立 Time Card 表现，不占说话人栏；普通对白与黑幕对白继续保持最多两行。

### 制作维护口径

- 四关普通字幕继续共用 `Hearth_SubtitleStyle.asset`；结局黑幕和 Time Card 使用同一资产中的独立布局区。
- 三户机器人 HUD 的右上决策区和左下数据流改为共用 `Hearth_CompanionHudLayout.asset`，统一缩放、字号与位置。
- 本次内容已经写入正式稿并同步到 Dialogue Asset；未来修改文字仍以根目录正式稿为唯一来源。

## 2026-07-21 新定稿替换与四户流程确认

### 正式资料优先级

- 当前正式游戏对白唯一来源改为 `HEARTH_Full_Game_Script_No_Audio_Tags_Native_English.md`。
- `HEARTH_Full_Game_Script_ElevenLabs_v3_Native_English.md` 只作为未来 AI 配音的情绪、停顿与表演提示来源，不直接写入游戏字幕。
- `HEARTH_Codex_Program_Todo_Confirmed_Revision.md` 用作流程审计参考；与用户本轮明确决定冲突时，以本条记录为准。
- 当前游戏不制作、播放或预留开场宣传片流程，直接从一楼大厅正式开场。

### 本轮用户确认的覆盖规则

- 17F01 与 17F02 已完成的空间演出、机器人回放、人物走位和终端结构全部保留；本轮只调整正式对白顺序、推荐播放时机、选择前后门控和下一户指引。
- 17F04 保留现有三阶段病毒弹窗关闭玩法，不采用确认版文档中的三次静态系统警告替代方案。
- 时间统一为：Lily 留言 `4:42 PM`；17F03 当前检查约 `18:57`、深眠发生于 `18:47`；Mia 到自宅门口约 `19:08`。
- 其他已确认修订照常接入：大厅可转头锁移动、公共事件一次性且可跳过、17F03 回放后房内询问与走廊评价顺序、最终选择前第四面墙诱导、电子照片屏第二页接口。

### 17F03 新处置顺序

1. 门口终端说明并取得入户权限。
2. 回放结束返回米娅房内视角，Laura 先询问，Mia 只回应自己已找到关闭点并需要作出处置。
3. 随后开放实体陪伴单元 E；进入固定检查视角后，A/B 页面先锁定，Field Unit 在此说明两种措施并推荐重启，建议播完再开放确认。
4. 提交只生效一次；米娅先在房内说出具体措施，父母回应结束后再渐黑到走廊。
5. Field Unit 只在走廊评价刚才的具体决定，然后按累计信任播放 accepted range 或 supervisor review，最后宣布 `All three inspections are complete.`

### 17F04 新内容

- 最终 A/B 前播放 Field Unit 的 `better ending` 诱导，明确表现其公司立场与轻微打破第四面墙的效果。
- 让陪伴单元代答后，按累计信任正负播放不同绩效评价。
- 电子照片屏支持同一屏幕左右切换两张照片；目前第二张图片未提供，因此运行时仍只显示圣诞合照，不出现空页或翻页提示。
- 第二张照片预留文件名为 `Assets/Art/UI/HearthHud/Finale/FamilyPhoto_Second.png`。素材到位后重新运行 17F04 Apply 菜单即可启用。
- 低信任关闭继续使用三类无限生成式弹窗波次和半透明黑色遮罩；本轮只更新三类警告含义，不覆盖生成速度与数量调参。

### 同步与验收结果

- 新正式稿已移除宣传片段，完成自然两行拆分并写入稳定 Sequence 标记。
- Unity 已同步 `70` 个 Dialogue Asset、`394` 个映射条目；正式稿共有 `330` 个稳定字幕段。
- Coverage 与真实 TMP 两行布局验证均通过。
- 大厅、17F03、17F04 场景 Binder 已应用并通过 Camera/AudioListener/锚点验证；17F01/17F02 玩法未重建。

## 2026-07-21 三户长按交互恢复

- 本次只修 UI 绑定和显示生命周期，不改变 17F01、17F02、17F03 的剧情顺序。
- 17F01：仍然只有准星命中小男孩交互胶囊、距离与阶段同时满足时显示长按 E。
- 17F02：仍然在 Claire 的对白结束并等待约 1.5 秒后，显示 `HOLD E  OFFER REASSURANCE TO CLAIRE`。
- 17F03：仍按剧情顺序先面对女儿、再面对母亲；准星命中当前目标时显示对应长按 E。
- 正式剧情关闭机器人 HUD 页面浏览输入；后续提示内容由对应 `HearthCompanionHudSceneData` 维护。
- 单按 E 的终端、门、相框与实体机器人交互不受本次修改影响。

## 2026-07-25 HEARTH UI V2 与终端视图规则

- 新增独立 V2 人类 HUD、陪伴单元 HUD、17F01/02/03 门口终端、17F04 自宅终端和一楼任务终端。
- Legacy UI 全部保留；Unity 菜单可在当前场景一键切换 Legacy/V2，本次最终场景保存为 V2。
- 人类视角只显示人类第一人称 HUD；陪伴单元视角只显示陪伴单元第一人称 HUD。
- 进入任意终端固定视角后，全部第一人称 UI 必须隐藏，包括人类身份、任务、地点、Tab 菜单、按键教学、大厅叙事卡和陪伴单元全屏框。
- 终端自身的页面、开机动画、终端内部对白和键盘提示继续显示。
- 关闭终端后，恢复进入前使用的真实玩家相机、交互器和每个 HUD 原有显隐状态，不能错误跳到旧机器人相机。
- V2 Tab 菜单和最终 A/B 继续保持可操作；高亮范围跟随目标文字/按钮，不因 Prefab 切换丢失。
- 陪伴单元右上决策区和左下数据流使用 V2 坐标作为共享布局基准，Play 后不再回跳到旧版越界坐标。
- 本轮只更新 UI 视觉、显示边界和切换维护方式，不改变四户剧情顺序、信任结算或长按 E 条件。

## 2026-07-27 第二套 UI 接入三条真实流程

- 本条只记录 UI 显示端和真实流程的接线，不改正式英文对白，不改 17F03/17F04 剧情顺序、
  信任判断、处置结果或相机触发条件。
- 17F03 Entity Inspection 不再只是 Companion Prefab 中的静态示意元素：
  真实 `Hearth17F03InspectionCanvas` 原地采用第二套深蓝黑、冷白、灰蓝视觉和 1920×1080
  信息层级；Recall、回放后 A/B、Space/Esc 和检查固定相机仍使用原控制器。
- 进入 17F03 检查时隐藏 Human 身份、任务、地点、叙事卡和字幕视觉；已经开始的正式对白、
  语音、计时与完成事件不停止。检查面板作为 Modal 独占画面，退出后按当时真实状态恢复
  Human HUD；如果面板中途被停用，也必须释放 Modal。
- 17F04 Photo Archive 的 Human `Slide07/08` 接入真实相框流程：实体照片 Renderer 仍是照片
  数据源，固定 Photo Camera 的实时画面显示在 V2 档案视口；只有第二张正式贴图存在时才开放
  第二页和左右切换；当前只有一张贴图时页码显示 `01 / 01`。
- Photo Archive 和高信任 Shutdown Confirm 都隐藏 Human 常驻身份/任务；照片页把静态
  Field Unit 占位通道让给正式字幕，不与真实对白叠成两层。
- 相框对白未结束时 V2 档案页显示 `PLEASE WAIT`，不允许 Space 提前退出；可翻页和可退出状态
  分别显示对应按键，不再同时叠加旧 ExitHint。
- 17F04 高信任关机使用 Human V2 `Slide10ShutdownConfirm`，Space 确认、Esc 取消；
  `Hearth17F04FinaleController` 继续接收 Challenge 完成/取消事件并推进或退回 ApproachUnit。
- 低信任关机仍使用已确认的三波动态病毒弹窗玩法，本轮不以 Human Slide11–13 替代。

## 2026-07-27 第二套 UI 最终显示互斥与大厅终端节奏

### 一楼任务终端与正式简报

- 本条覆盖 2026-07-19 “任务终端开机后立即在终端内播放简报”的旧表现。
- 玩家确认领取任务后，任务终端先独占画面至少 5 秒；这段时间右上动作保留但锁定，
  底栏显示 `PLEASE WAIT`，Space 和 Esc 都不能提前关闭。
- 5 秒后提示改为 `SPACE CLOSE TERMINAL`。此时 Space 只关闭终端，
  不在终端画面内播放或跳过正式路线简报。
- 只有终端已经真实关闭并恢复 Human 视角后，`assignmentLoadedDialogue`
  才开始播放。这样终端 UI、世界字幕和大厅叙事卡不会成为三层叠加。
- 路线简报期间米娅可以移动和转动视角，但所有 E 交互和电梯继续锁定；
  最后一句自然播放完成后，才恢复交互并开放电梯。
- 本次没有修改正式英文对白原句、语音时长规则或一次性领取条件。

### 全局 UI 互斥

- 第二套 UI 运行时按状态只保留一个主接管层：
  `Takeover > Modal > Terminal > 普通 Human HUD`。
- 进入任意终端时，只允许显式标记为 `Terminal` Context 的终端所属对白继续显示；
  Human/Field Unit 等世界字幕和大厅 HUD 卡片必须隐藏。Human Modal
  （Tab、照片档案、实体检查、最终选择等）或 Shutdown/结局 Takeover
  会抑制全部字幕视觉与大厅 HUD，不再与主界面重叠。
- 字幕抑制只隐藏视觉；已经开始的对白、语音、计时和剧情完成事件继续运行，
  不会被停止、重播或因为切 UI 而改稿。退出接管界面后，显示按当时的真实播放状态恢复。
- 17F03 实体检查以自身为 Modal Owner 申请互斥，关闭或停用时必须释放；
  Recall、回放、A/B 处置和检查相机仍由原 Replay Controller 控制。

### Tab 门控与教程优先级

- Human Tab 菜单只允许在普通、可交互的 Human Gameplay 中打开。
  正式对白、控制锁、终端、Modal 或 Takeover 期间按 Tab 不会穿出第二层菜单；
  已经打开的菜单仍可用原 Tab/Esc 关闭。
- 按键教程的显示优先级固定为：
  安全/关机 Takeover → 全屏终端 → 选择或 Hold E → Tab 菜单 →
  动态 E 交互 → 初始 10 秒教程 → 无提示。
- 初始教程只显示 `WASD MOVE / MOUSE LOOK / E INTERACT / TAB MENU`。
  它只累计 10 秒有效 Human Gameplay 时间；正式对白、动态 E、终端、Modal、
  Takeover、控制锁、暂停或非 Human 视角期间暂停计时并隐藏，返回普通玩法后继续，
  最后用 0.35 秒淡出。
- 正式对白继续自动推进；任何对白状态都不显示错误的 `SPACE CONTINUE`，
  教程与操作提示不得借用字幕播放器。

## 2026-07-27 第二套 UI 最终排布、文字容量与分辨率收口

- 本条只记录 UI 显示与验证增量。正式游戏对白唯一来源
  `HEARTH_Full_Game_Script_No_Audio_Tags_Native_English.md` 本次未修改；
  17F01/17F02 终端内处置对白只新增显式 `Terminal` 播放语境，不改任何原句、顺序、
  音频时长、选择结果或信任结算。
- 一楼任务终端是独立 Lobby/Assignment 终端，不属于 17F01。
  它不再被页码兜底推断成 `17F01`，但原 Custom 动作和节奏保持：
  前 5 秒 `PLEASE WAIT`，随后 `SPACE CLOSE TERMINAL`，关闭后才播放世界简报。
- 17F04 自宅终端继续位于 `17F/ROOM4/TV (3)`。当 V2 Home Terminal Prefab
  存在时，重跑 `Apply 17F04 Home Finale Setup` 必须继续使用
  `Terminal_17F04_Home_V2.prefab`，不能把整套场景中的第七个 V2 槽位降级为 Legacy。
  Apply 后 Human、Companion、Lobby、17F01–17F04 必须仍保持 `7/7 V2`。
- 17F04 Home Terminal 的相机与控制关系必须在 Apply 后完整保留：
  统一 `ViewSwitchController`、共享 `HearthPlayerControlLock`、`TV (3)` hardware root、
  TV 自有 World Camera 与 Canvas `worldCamera`。这些修复只保证 UI/相机拓扑，
  不改变 Space 进入自宅、渐黑、相机切换或正式剧情顺序。
- 终端共享 Footer 的 1920×1080 坐标确定为
  `X=96 / Y=920 / W=1728 / H=64`，并使用高于页面层的 Canvas 排序；
  目的是让按键说明完整留在实体电视内，不被下边框裁切，也不再增加第二个全屏外框。
- Human Modal、Terminal、Shutdown/Low Trust Takeover 会通过 Coordinator
  外部抑制 Human Persistent HUD；身份、任务和地点不应穿透全屏界面。
  Coordinator 停用时必须恢复 Human、字幕与 Lobby Overlay 的外部显示状态。
- Final Choice 只保留 `FocusLayer/FinalChoiceInputHint` 这一套真实动态提示。
  它根据当前水平/垂直选择和输入开放状态更新；不再叠加静态
  `V2_FinalChoiceHint`。
- Companion V2 的 `PersistentInfoLayer/V2_StatusPanel` 已补建并绑定到
  `HearthCompanionHudController.statusPanelView`。Title、最多五行 Rows、Footer
  与 Accent 仍由不同 `HearthCompanionHudSceneData` 动态提供，不能烘焙成固定图片或文字。
- Companion 临时 Trigger Card 的信息层级高于常驻 Status Panel：
  Card 开始淡入时隐藏 Status Panel，淡入、停留和淡出期间只显示临时 Card；
  淡出完成后按当前 SceneData 是否有 Status 内容自动恢复常驻面板。
  TriggerCardView 被停用时必须立即清除可见状态；空 TimedCues 或缺失 CanvasGroup
  也按安全隐藏处理，避免切视角后 Status Panel 一直无法恢复。
  Status Panel 只有在 SceneData 确有非空标题、Footer 或状态行时才显示，不生成空卡。
  该规则只消除重复内容，不改变 Timed Cue 延迟/时长、三户剧情触发或 Companion 输入。
- Final Choice、Shutdown Confirm 与 Low Trust Warning 页面在 V2 安全刷新时
  必须清除旧 `Border_*` Image 的可见性和 Raycast，不能让 Legacy/Scene Override
  遗留细框穿过新面板。真实按钮、焦点提示、V2 规则线和原 Space/Esc/选择逻辑保持不变。
- 大厅 Overlay 与任务终端全部 TMP、17F04 照片退出提示、Human/Companion 动态 E
  交互提示都采用固定字号与 `Overflow`，不再使用 Auto Size、Ellipsis 或 Truncate。
  动态 E 的 Repair 还会禁止换行；对应 Lobby、17F04、Runtime Interface Validator
  必须覆盖固定字号与 Overflow 安全条件。
- 初始教程中的 `WASD/MOUSE/E/TAB` 键位与 `MOVE/LOOK/INTERACT/MENU`
  动作文字全部禁止换行；`INTERACT` 不得在 1920×1080 或缩放分辨率下拆成两行。
- 已真实把 Game View 切换到 `1280×720` 和 `2560×1440` 检查共享锚点与缩放，
  两个分辨率均未出现代表性 HUD/终端漂移，并保存对应基线截图。
  该结果不等于 11 类界面在两个分辨率下都已保存完整逐界面截图矩阵。

## 2026-07-31 一楼大厅旁人声与履带机器人声音方向

- 一楼大厅除原有建筑空间底噪 `Lobby.RoomTone` 外，新增一层可独立控制的
  `Lobby.Walla`：表现任务终端、等候区或路过 NPC 附近不可辨识的低声谈话与轻微活动声。
- `Lobby.Walla` 不能包含能听清内容的人物对白，也不能与正式路线简报、字幕或终端语音争抢；
  实现时应低音量循环，并可根据玩家位置或剧情状态淡入淡出。
- 玩家/陪伴单元的机器人移动方式确认按小型轮式或履带式处理，不再沿用双足机器人脚步方向。
- 小型履带声音采用分层意图：RC 小电机或小型履带循环作为主体，塑料/橡胶轮接触作为细节，
  起步、停止和原地转向时再增加短伺服；排除大型坦克、柴油发动机、重链条和液压机甲声音。
- 本条目前只记录音效选择与演出意图，并同步到素材清单；暂未修改 Unity 场景、音频槽位、
  脚本、正式英文对白、剧情顺序或信任结算。

## 2026-08-01 一楼大厅旁人声进一步收弱

- 本条细化并覆盖 2026-07-31 对 `Lobby.Walla` 中“轻微活动声”的描述：大厅旁人声现在只保留
  2–4 名成年人低声、含混、不可辨识的谈话，不再包含脚步、衣物、餐具、门、广播、笑声、
  咳嗽或其他明显空间事件。
- 不使用大型商场、大厅或拥挤人群录音再强行压低音量；优先选择原本就安静的 Couple / Small
  Group Walla，并作为 `Lobby.RoomTone` 上方很薄的一层。
- 混音目标是玩家只感到“附近似乎有人”，而不会主动注意到人声；正式对白期间还要进一步
  压低或 Duck，不能影响路线简报、终端语音与字幕阅读。
- 本条仍为音效选择和演出意图记录，暂未修改 Unity 场景、音频槽位、脚本、正式英文对白、
  剧情顺序或信任结算。

## 2026-07-31 V2 UI 视觉与结构收口

- 本条只记录 V2 UI 呈现、层级和预览验收结果；没有修改正式英文对白、语音内容、剧情顺序、
  回放结果、处置逻辑或信任结算。
- Human、Companion 与 Terminal 的基础视觉根统一交给 `HearthUiStateCoordinator` 互斥显示；
  Companion 控制器只更新本界面的住户数据、Status、Decision、REC 和特殊效果，不再隐藏或恢复
  Human HUD。
- Human 正式对白固定在下方中央，左右姓名签互斥；Field Unit 辅助通讯固定在右侧，并与正式对白
  互斥。Current Task 当前只保留标题，逐阶段任务文案待用户提供或确认，制作端不得自行补写。
- Companion 回放统一保留顶部 REC；身份、Subject Monitoring、Synth Voice、Current Task 和正式对白
  使用固定 1920 坐标。旧 Physical Unit Feed、Monitor Bus、遗留竖线和重复横线不再进入 V2 成品。
- Human Tab 选中衬底收进按钮边框内部；Photo Archive 使用独立连续模态底板；Live Audio 数据区上移，
  与下方正式对白分离。
- 大厅、17F01–17F04 终端的标题、导航、正文和 Footer 合并进唯一 `TerminalVisualRoot`，共用连续
  深蓝黑内屏。实体电视仍是唯一外框；17F04 旧 HOME ACCESS、WELCOME、UnitLabel 等重复文字已移除。
- 本次只用 Runtime Preview 与 Unity MCP 截图检查 Human 9 类、Companion 7 类、Terminal 5 类，
  并抽查真实 1280×720 与 2560×1440 Game View。完整剧情、真实输入和流程恢复由用户之后亲自测试。

## 2026-08-01 正式音效素材导入、复用范围与剧情位置确认

- 已把用户在 `E:\桌面\音效` 中确认的 29 个素材复制并统一整理为 MP3，正式副本位于
  `Assets/Audio/HEARTH/Imported/`，按 `Ambience / UI / System / Foley` 分类；桌面原件没有移动、
  改名、覆盖或转码。
- 所有室内场景的基础 Room Tone 改为共用
  `AMB01_GLOBAL_AllInteriors_RoomTone_Main_01.mp3`：包括一楼大厅、17 楼走廊和
  17F01–17F04 家庭室内。该决定覆盖此前“家庭、走廊、大厅分别使用独立底噪”的素材选择规则；
  场景差异后续通过音量、EQ 和混响处理。
- 一楼大厅的人声仍保持独立层：
  `AMB09_LOBBY_WaitingArea_Walla_SmallGroup_01.mp3` 只用于等候区和任务终端附近，
  不能扩展到住户房间或走廊，也不能与正式对白竞争。
- 通用 UI 素材已明确拆分：普通导航/选项焦点、A/B 决策焦点、单次 E 确认、A/B 正式提交、
  长按过程、长按完成和高危警告分别使用独立文件。当前部分 UI 脚本仍共用 Focus Clip 槽；
  本次只记录并导入素材，不修改脚本或强行绑定。
- 一楼大厅 Lily 耳机通知是特定剧情 UI；17F04 `Error Interface` 只用于关机挑战中的错误弹窗出现，
  后续需要限制密集弹窗的叠音。`Alien Warning Signal` 则是跨关卡高危警告，不作为普通错误音。
- 17F04 Path A 黑屏结局的三段声音位置固定为：厨房煎炒底层 → 放学回家钥匙落桌 → Lily 房间远雷。
  其中厨房煎炒素材明确不属于 17F02。
- 17F04 Path B“三年后—前厅”使用行李箱滚动和门槛撞击候选，随后衔接通用住宅门关闭声。
- 住宅门、17F02/17F03 坐下起身、17F01–17F03 履带机器人移动、17F01/17F02 餐桌餐具按跨关卡
  复用素材管理；完整母素材后续再分割成实际运行片段。
- 本次没有切音、修循环点、调音量、裁时长或处理叠音，也没有把 AudioClip 绑定到场景、Prefab、
  AudioSource 或剧情事件；正式英文对白、剧情顺序、处置逻辑和信任结算均未修改。
- 原始文件名、正式名称、具体关卡、对象、触发动作、Unity 槽位状态和后续处理要求统一记录在
  `HEARTH_音效导入映射.md`。

## 2026-08-01 一楼大厅对白 UI、Lily 消息与 Space 后流程恢复

- 关卡/场景：一楼大厅开场，`Assets/Scenes/SampleScene.unity`。
- 本条已实现到脚本、Dialogue Asset、V2 Prefab 和当前场景，不是仅记录演出意图；
  正式英文对白 Markdown、原句顺序、语音内容和信任结算均未修改。
- Field Unit 路线简报继续属于 Auxiliary 通讯，位置固定在右上角；专用框由旧的
  `640×180` 扩大为 `640×400`，为当前长句和后续改字保留余量。人名与正文使用同一左边界，
  不再出现正文越过 `Field Unit` 名称左侧的情况。
- 正式人物对白仍位于下方中央；对话视觉层级统一为人名不小于正文：Standard 为
  人名 28 / 正文 26，Centered Epilogue 为人名 30 / 正文 28。姓名签继续包住人名，
  左右说话人位置可在正式字幕播放器 Inspector 中按 1920 坐标手动调整。
- Lily 的语音留言不再同时生成下方普通字幕框。它使用独立右上角 `540×300`
  `INCOMING VOICE MESSAGE` 卡片，正文由大厅 Overlay 显示；对应 Dialogue 行为
  Auxiliary + AudioOnly + Manual Space，因此玩家仍可按 Space 跳过留言。
- Mia 在留言后的三句收尾对白和最后一句 `Okay.` 保持原流程。最后一句完成或按 Space 推进后，
  大厅流程必须进入 `FreeExploration`，并恢复移动、视角、普通交互和 Tab 菜单；不得出现全部按键
  仍被锁住的状态。
- 控制恢复以共享 Owner/Mask 控制锁为唯一真值。ViewSwitch 在对白释放、Owner 变化和视角切换后
  重新应用真实 Mask，关卡脚本不得另存一份 Movement/Look/Interaction 的旧启停快照。
- Human Tab 的 Today、Disposition History、System Settings 三类页面使用专用 V2 页面框和内容框；
  History 与 Settings 另有独立底部指标框。装饰线不得穿过标题或正文，三页仍沿用原 Tab/Esc/
  方向键输入和 Modal 互斥规则，不改变剧情或数值数据。
- 最小验收：
  1. Field Unit 最长简报完整处于 640×400 框内，姓名/正文左边界一致。
  2. Lily 留言只显示右上专用卡，Space 可跳过，不出现下方普通对白框。
  3. 播放至 Mia `Okay.` 后状态为 `FreeExploration`，控制 Mask 为 `None`，移动、视角、交互和菜单恢复。
  4. Today、History、Settings 均有专用 UI，标题、内框和底部指标框互不压线。

## 2026-08-01 最终配音与字幕定稿接入

- 来源优先级：本轮以配音任务 `019fa9b8-61ee-7a22-9535-60362e5c9558` 确认的
  `HEARTH_ElevenLabs_v3_API_Voice_Lines_Manual_Revision.jsonl` 为台词与逐句语音定稿，
  已把去除表演标签后的正文同步回正式游戏对白 Markdown；后续运行时仍由正式 Markdown 生成资产。
- 开场不制作、不接回宣传片，游戏继续直接从 `SampleScene` 当前一楼大厅视角开始。
  `Prologue_HEARTHCommercial` 的 7 条语音全部不导入；大厅中依赖“刚看完宣传片”语境的
  `Lobby_OpeningBriefing_FieldUnit_002` 也暂不进入当前开场，避免对白与画面冲突。
- 当前实际接入为 `330` 个独立正文语音，覆盖大厅、17F01、17F02、17F03、17F04 和全部正式分支；
  高低信任共用句按稳定 Line ID 复用后，共形成 `394` 个 Dialogue Asset 运行条目。
- 字幕内容使用 JSONL 最终措辞，但不显示 `[calm]`、`[quieter]` 等配音表演标签；说话人、标点和
  口语化措辞均跟随该定稿。每条有语音的字幕时长跟随对应 `AudioClip.length`，尾部保留 `0.12s`。
- 17F04 的 6 条照片语音在配音稿中属于同一组，游戏内继续按现有流程拆为第一张照片 2 条、
  第二张照片 3 条、完成指引 1 条；只改变文本与语音绑定，不改变翻页、退出或最终选择门控。
- 本条已实现到正式 Markdown、330 个 Unity MP3、70 个 `HearthDialogueSequence` 资产和验证工具；
  没有改变三户检查顺序、A/B 结果、信任计算、玩家控制或大厅 Space 推进规则。
- 为保证“直接从一楼大厅开始”不被旧 17F 流程的同帧初始化打断，大厅自动开场改为场景初始化后的
  下一帧取得共享字幕播放器；字幕 Stop/替换也会取消旧剧情协程，避免画面已隐藏但 Opening 仍卡住。

## 2026-08-01 自然对白、任务引导与旧 UI 残留恢复（最新覆盖）

- 本条覆盖同日较早记录中“Mia 使用正式框/可用 Space 推进”“Current Task 只保留标题”以及
  “任务终端后允许移动和转头”的旧描述；本条已实现到运行脚本、V2 Prefab 与当前场景。
- 一楼大厅 Field Unit 仍使用放大的通讯框；同一段中的 Mia 不再进入该框，也不显示姓名签或
  `SPACE CONTINUE`。Mia 使用屏幕中下方居中的黑色半透明底、白色字幕，并在 Voice Clip 播完
  （无语音时使用回退时长）后自动进入下一句。
- 大厅 Field Unit → Mia → Field Unit 的顺序完全由 Dialogue Sequence 和真实音频长度驱动；
  玩家不需要按 Space。大厅三处可选对话改为进入范围后短按 `1`，提示恢复为 `1 TALK`；
  17F01–17F03 已有固定剧情操作继续使用长按 `1`，不改变长按进度与完成条件。
- 大厅右上 Current Task 正式启用：依次提示听取 Field Unit、前往任务终端、查看任务、前往电梯、
  搭乘至 17 楼和前往 `17F-01` 终端。任务终端退出后的 Field Unit/Mia 说明期间锁定移动、视角、
  E 与 Tab；全部播放完才恢复控制并开放电梯。
- 17F01–17F03 人类终端任务按状态更新为前往终端、查看住户、等待陪伴单元连接、查看记录、
  返回终端、选择处置和前往下一终端；Companion HUD 的 Current Task 按当前回放 SceneData
  显示对应交互目标。
- 任务终端、门口终端、17F04 Home/照片相关 Field Unit 说明统一进入终端下方的大型说明栏，
  不再与右侧语音框、终端正文或照片内容叠在一起；终端/照片 Takeover 会隐藏背后的旧 HUD，
  但保留当前说明栏。
- 17F01 从小男孩房间切到客厅时不再继承低头俯角；脚本保留用户手调站位和锚点，只按目标锚点
  的水平朝向把相机俯仰重置为 `0`，使画面正对客厅人物。
- Companion 左侧 Status Change 卡片使用 V2 蓝青色、统一字号和短装饰线，旧整高竖条停用。
  Human/Terminal 选择页的选中填充收进 A/B 选项框，旧左侧竖线停用；全屏信息使用连续深色遮罩，
  不再用越出选项范围的大色块。
- 17F04 关机确认页只保留 `SPACE CONFIRM SHUTDOWN`，按钮下移；`ESC CANCEL` 隐藏且 Esc 不再
  取消该固定剧情。黑幕后自然对白恢复为黑底白字居中字幕，按语音长度自动连续播放，不显示
  Space 提示。
- 相册页保持玩家主 Camera 为 Display 1 的后备渲染，修复 `Display 1 / No cameras rendering`；
  Photo Archive 作为全屏 Takeover 隐藏背后旧层级。用户已手调的终端、相册、镜头锚点和站位
  均保留，本轮没有运行会重建这些对象的完整 Binder。
- 最小验收：从一楼大厅开始不按 Space 听完 Field Unit/Mia；短按 `1` 触发大厅对话；任务终端
  关闭后直到对白完毕不能转头或移动；随后 Current Task 指向电梯；进入 17F01 客厅视线水平；
  终端/相册/选择/关机/黑幕页面无旧 UI 穿透或缺失字幕。

## 2026-08-01 补充音效素材导入与普通锁定反馈取消

- 用户确认不制作“普通无效操作／门锁定反馈”。现有 `SmartDoorController.lockedClip` 字段保持原状，
  本轮不为其导入素材、不绑定高危警告，也不修改门脚本；高危警告只继续用于严重风险与 17F04
  关机警告升级。
- 17F02 Scene 2.4 的浴室段落使用新导入的门后淋浴母素材；后续需低通、降音量、制作遮挡感并
  检查循环，不能按近距离淋浴声直接播放。
- 17F02 Scene 2.5 黑屏争吵使用新导入的厨房冰箱底层与城市交通母素材。冰箱素材已经包含厨房
  RoomTone，不应与全局 `AMB-01` 全量重复叠加；交通素材需处理成隔窗／隔墙后的远声。
- 17F02 Scene 2.2 `BedroomComfort` 使用新导入的柔和爵士，位置以正式对白稿为准，不属于 17F01；
  后续需确认循环、授权记录，并在离开卧室或进入晚餐段落时淡出。
- 17F04 Path B 学校开放日黑屏段落使用新导入的体育馆 Hum，保持远、空、低存在感，不盖住 Lily
  与陪伴单元对白。
- 所有同类终端共用新导入的设备开机声；17F02 强制关机、17F03 Deep Sleep 与 17F04 最终关机
  共用新导入的陪伴单元关机声。当前只记录计划用途，尚未绑定 Unity 槽位。
- 本批从 `E:\桌面\音效\新音效` 只读导入 7 个项目副本；桌面原件未移动、未改名、未转码。
  本轮不分割、不修循环点、不调音量、不修改脚本或场景。

## 2026-08-01 HEARTH V2 对白、交互提示与残留 UI 最终修正（覆盖同日旧口述记录）

### 制作前提

- 正式字幕唯一来源继续为 `HEARTH_Full_Game_Script_No_Audio_Tags_Native_English.md`；不恢复宣传片/序章。
- 保留已绑定语音以及用户手调的 Camera、TV、终端、照片 Renderer 和锚点 Transform。
- 本记录所述为本轮正式实现意图；若与上方同日“短按 1/长按 1”“终端自动对白”“全屏相册”记录冲突，以本节为准。

### 对白与 Space 规则

- 默认：Field Unit、Home Unit、Lily、居民、回放角色及参与真实交流的 Mia 都使用 V2 特殊对白框，按一次 Space 停止当前语音并进入下一句。
- 自动：只保留策略表登记的大厅 Mia 短答/评论、电梯和任务终端短答、`17F04_HomeGreeting...Mia_001`；使用屏幕中下方黑底白字并按语音自动结束。
- 17F03 入户/处置后交流、17F04 女儿房、最终回答、Home Unit 处置及黑幕前交流中的 Mia 都恢复特殊框 + Space。
- 17F04 画面完全变黑后才进入自动尾声；所有角色黑底白字居中、按语音连续播放，无 Space 提示。
- 大厅 Lily 留言保留专用消息卡且可 Space；17F04 自宅终端 Lily 留言使用独立消息卡，随后 Home/Field Unit 切到终端下方框，不与普通框叠加。
- 删除按 Speaker=Mia 自动化、按序列含 Field Unit 统一通道的推断。分类只由稳定 lineId/sequenceId 策略和逐行元数据决定。
- Space 必须松开后才能推进下一行；同一按键不能同时结束对白并关闭终端/相册或启动下一终端动作。

### 一楼大厅与任务终端

- 三个大厅交流区只登记给 `PlayerInteraction`，短按 E 触发；不再自行监听数字 1 或重复监听 E。
- 开场顺序保持 Field Unit → Mia → Field Unit：Field Unit 等 Space，Mia 自动，之后恢复 E Prompt。
- 任务终端 E 打开并完成开机后，页面保持开启并立刻播放终端内简报；Field Unit 使用下方世界空间框 + Space，Mia 自动短句回全局中下方。
- 全部对白结束、最短阅读时间满足且 Space 已松开后，才显示 `SPACE CLOSE TERMINAL`；下一次 Space 关闭并恢复移动。
- Current Task 顺序：听 Field Unit → 前往任务终端 → 查看任务 → 前往电梯 → 前往 17 楼 → 前往 17F-01。

### 住户、陪伴视角与镜头

- 所有短按交互统一 E；固定剧情长按统一 Hold E。长按层出现时短按层暂停，完成/隐藏后按原状态恢复。
- Companion Current Task 只写目标，E/Hold E 始终只出现在中下方提示层。
- 17F01→17F02、17F02→17F03、17F03→RETURN HOME；住户内部依次指向终端、资料、陪伴同步、分析和处置。
- `17F01_02 STATUS CHANGE` 使用纯 V2 TriggerCard；移除旧整高竖线、重复规则线及旧色覆盖。
- 男孩房到客厅切换使用独立 Living Room Camera Anchor 的完整位置与俯仰；Robot Root Anchor 只决定落点。自动修复只补空引用，不覆盖用户已有锚点。

### 17F04 终端、TV4、选择和结尾

- 自宅终端对白留在 TV(3) 的世界空间 UI：Lily 专用消息卡，Home/Field Unit 下方框，Mia 自动短句回全局字幕。
- TV4 只保留实体照片、用户 Photo Camera 和过渡；在 TV4 下建立 `PhotoArchiveCanvas_V2` 及下方 Field Unit 框。停止使用 Slide07/08、RenderTexture、`PhotoCameraFeed_V2` 和独立 Exit Canvas。
- 选择页面保留一个全屏中性暗幕遮挡世界；内容面板只包标题和两行选项。选中色块严格使用真实按钮 Rect，不越界；删除 A/B 左侧旧线和重复焦点几何。
- 高信任关机页只保留下移的 `SPACE CONFIRM SHUTDOWN`；删除 `ESC CANCEL` 文字、按钮、隐藏点击区和 Escape 事件。低信任警告流程不合并。
- 17F04 Current Task：使用自宅终端 → 查看 Lily 留言 → 查看相册 → 前往 Lily 房间 → 交谈 → 最终回答 → 接近 Home Unit → 确认关闭；黑幕尾声隐藏任务正文。

### 暂不改变

- 不改定稿台词，不覆盖 AudioClip，不改住户/信任结算，不移动用户 Camera、TV、终端或锚点。
- 编辑器数字页预览只允许在 Editor QA 中手动开启，构建和正式流程不响应数字 1。

## 2026-08-02 V2 实施结果补记

- 上述 2026-08-01 “最终修正”已经落实到对白播放器、终端/TV4 Surface、E/Hold E、Current Task、STATUS CHANGE、17F01 客厅镜头、A/B 选择和高信任关机确认流程。
- 自动对白分类已作为逐行资产元数据落盘：仅策略表登记的 Mia 短句与完全黑幕后尾声自动；其余角色和真实交流中的 Mia 默认特殊框 + Space。Lily 留言继续使用专用消息框 + Space。
- 一楼任务终端对白在终端保持开启时播放，最后一句与关闭终端之间加入 Space 松开门；TV4 只使用世界空间相册和下方对白区，旧独立退出 Canvas 已从场景删除。
- 本轮未改定稿台词、AudioClip、用户 Camera/TV/终端/锚点坐标，也未恢复宣传片。
- C# 与静态资源审计已通过；由于 Unity 编辑器的 MCP 心跳未响应，Play Mode 全流程与菜单内 Coverage/Policy 日志仍列为编辑器重启后的最终人工验收项，不作为已完成结果记录。

## 2026-08-02 正式音效全流程接入（对话 019fb158-4372-7a72-90a0-4976329a4537）

### 制作原则

- 宣传片/序章继续不接入，声音从一楼大厅正式视角开始。
- 36 个正式 MP3 原件不裁切、不覆盖；开关门和短 Foley 只在运行时按起点/时长播放。
- 终端、HUD、E、Hold E、选择、视角切换、关机和脚步按类型复用，不为每户复制素材。
- 普通无效操作和门锁拒绝不新增反馈音；猫叫和猫脚步也不接入。
- 人类使用轻脚步；陪伴单元使用轻型履带/玩具电机感，不得呈现坦克、柴油机或重工业质感。

### 一楼大厅与电梯

- 开场同时播放主室内 RoomTone 和等候区局部 3D Walla。Walla 仅保留模糊成人声，约比对白低 18–24 dB，对白时再 Duck 约 5 dB。
- Lily 留言出现前播放耳机通知音；留言专用 UI、语音和 Space 规则不变。
- 领取任务、终端页面和有效 E 操作使用对应 UI/系统声；进电梯时同时停止大厅 RoomTone 与 Walla。
- 电梯按钮、关门、运行、到达铃、开门按原流程顺序播放；抵达后切换走廊 RoomTone。

### 17F01–17F03

- 17F01：卧室/客厅 RoomTone、转场、安抚确认、客厅餐桌 Foley 与轻型履带素材接入；餐桌只取受控短段。
- 17F02：落座、安慰段轻爵士、妻子起身/走位、住宅门、餐桌、终端、门后淋浴、故障/关机和黑屏冰箱+远交通接入。爵士、淋浴、冰箱、交通均在对应段结束停止。
- 17F03：母女起身、女儿走路、键盘、陪伴单元故障和关机接入；门复用住宅门源。
- 本次不改变 E/Hold E、Space、演员路线、相机锚点、处置或信任结算。

### 17F04 与黑幕结尾

- 自宅客厅和女儿房使用独立可维护 RoomTone；切房时切换，进入完全黑幕后停止。
- 相册、病毒弹窗、危险波次、确认与关机接入正式系统/UI 音效。
- Path A：Lily 首句启动煎锅；Mia 回家行停止煎锅并落钥匙；后段 Lily 呼唤行启动远雷雨；序列结束自动停止。
- Path B：Home Unit 开放日叙述行启动远处体育馆；Lily 宣布搬宿舍时停止；Lily “I know.” 行播放行李箱落槛并启动滚动；Home Unit 报告 Lily 已离开时停止。
- 黑幕后仍按第一套自然字幕自动连续播放、无 Space；本次只加音效，不恢复旧特殊对白框。

### 维护结论

- 已建立中央 Sound ID、关卡 Cue ID、对白 `sequenceId + lineId` 精确触发，以及统一 Audio Channel/Duck。
- 全局换素材改 `HearthSfxCatalog.asset`；单关特例用对应 Cue 的 `Primary Clip` 覆盖。
- 以上为已实现的脚本/场景变更，不是仅记录意图；仍需 Play Mode 主观试听最终音量、EQ、循环点和门源分段边界。

## 2026-08-03 V2 UI、E/Hold E 与 17F04 黑幕场景卡落盘

- “Few/Fuel Unit”统一为 `Field Unit`。终端内名称/正文/Space 字号统一为 52/26/26；同步终端只放大文字，不改变现有外框尺寸。
- Companion 顶部身份和任务拆成四个独立 TMP；只调整这四项，其他 Companion 页面保留给用户手调。
- 短按 E 与 Hold E 恢复 V1 功能结构并应用 V2 蓝色视觉。普通剧情木门不再允许玩家 E 开关；终端、电梯、检查点、相册等合法目标继续短按 E，固定持续剧情继续 Hold E。
- 门口终端、17F04 Home Terminal、TV4 和 Final Response 按 1920×1080 Profile 重排。处置和 Final Response 遮罩覆盖全屏，操作提示位于遮罩之上。
- 17F02 正式 Synth Voice/Field Unit 出现时临时隐藏旧 Persistent DecisionPanel，结束后才恢复，避免双层内容叠加。
- 17F04 Home Terminal 标题改为 `WELCOME HOME`；Lily 留言留在中央内容区，Field Unit 使用底部共享区，二者互斥。
- 黑幕后 TimeCard 先居中独立显示约 1.5 秒，再缩为对白上方持续场景标题。对白白字居中、按语音自动衔接、无姓名框、无 Space。
- 正式稿新增稳定 `HEARTH:TIME_CARD` 元数据，英文正文及已有对白 Line ID 不变；逐句 SFX 触发键不变。
- 本轮不重新导入、不改名现有 36 个正式音效素材，不改变中央 SFX Catalog；仅做 UI 后的 Cue 回归。

## 2026-08-03 全项目保守式结构重构（剧情与流程无变化）

- 本轮只调整代码职责、正式 UI 来源、显式引用和 Editor 工具安全性；不修改任何剧情台词、演出顺序、角色走位意图、按键规则、任务顺序、信任/处置条件或结局分支。
- Lobby、17F01、17F02、17F03、17F04 各自的 Controller 和 Coroutine 继续保留，不合并成通用状态机。
- F02/F03 的黑幕仍在原剧情节点、使用原持续时间；只把重复淡入淡出机械逻辑交给 `HearthScreenTransitionService`，并复用场景已有黑幕 CanvasGroup。
- Human 菜单/Final 高亮、TV4 相册、17F03 Inspection、五终端 Surface 和字幕改为正式 Prefab/Bindings 接管；运行时只改内容、状态和显隐。
- 正式缺失绑定时改为明确报错；旧运行时生成逻辑保留为 Legacy 隔离期回退，完整回放通过前不删除。
- 36 个正式 SFX、330 个正式语音、Catalog、Cue ID、Dialogue Line ID 和逐句 SFX Track 保持不变。
- 当前 Unity HTTP MCP 未发现活动实例，因此本记录只确认代码与离线编译完成；Prefab 安装、场景绑定和 Play Mode 结果必须在 Unity Reload 后另行补记，不能提前视为已验收。

## 2026-08-09 UI 显示与 F03 终端流程修复（流程顺序不变）

### 终端与第三户

- F01、F02、F03、F04 终端打开后的黑屏，确认为正式 Prefab 的 `TerminalVisualRoot` 被保存为关闭；现已恢复，并增加运行时打开前自检。
- F03 “ENTER UNIT”无响应，确认为该场景终端丢失 `MinLoopFlowController` 引用；已重新绑定唯一正式 Flow。Play Mode 中请求已推进到 `Entering Resident Unit`。
- 本轮不改变 F01→F02→F03→F04 顺序，不改变按钮开放条件、住户演出、Camera Anchor、处置或信任结算。
- F01–F04 选择页的旧 `KeyboardNavigationRoot` 不再参与显示，避免旧焦点字与新选择页形成两套 A/B、两套操作提示；Lobby 的独立关闭提示保留。

### 字幕与大厅信息

- Mia 的 `NaturalCaption` 取消黑色横向衬底，只保留白色居中文字；自动播放与音频/回退时长不变。
- 大厅 Lily 信息仍按原流程先完整弹出；收起后不再保留右上角 `READ / 4:42 PM / ASSIGNMENT NOT LOADED` 小卡。
- Companion 视角的 Noah、住户角色、Field Unit、Synth Voice 等正式框式对白出现时，旧 DecisionPanel 临时隐藏；对白结束后按原状态恢复。

### V2 外框

- 对有 `PanelBackdrop`、但既无 `ScalableFrame` 也无 `PanelFrame` 的正式终端面板补可缩放 V2 外框。
- 只补视觉边框，不改变面板 RectTransform、文本、按钮、Surface、Camera、音效或剧情事件。

## 2026-08-09 住户终端六页统一及 01/03/04 表现修正（已实现）

### 门口终端

- 用户手调后的 `Terminal_17F01_V2` Before Acquisition 作为六页视觉基线。
- F01 After、F02 Before/After、F03 Before/After 只同步相同元素的布局、字号、边距、颜色和框线；每户自己的文字、照片 Sprite、对白、音频、UnityEvent 和 Page 引用保持不变。
- 01–03 的处置选择面板统一居中并稍微上移；选择结束或 Field Unit 对白接管后不再残留 `UP / DOWN SELECT  SPACE CONFIRM`。
- F03 处置页使用与 01/02 相同的全屏 0.82 半透明黑幕，并在面板下方提供固定 Field Unit 特殊框和字幕，不再只有声音。

### 17F01

- 小男孩入睡段完成后，到客厅观察段之间新增短暂 Fade Out → 场景切换/传送 → Fade In。
- 连续 Space 推进正式框式对白时，旧 Synth Voice Decision 不再闪现一帧；正式对白结束后仍按原状态恢复 HUD。
- 移除各户 `STANDBY OBSERVATION` / `STANDBY - OBSERVE` 旧信息卡，但不删除对应剧情阶段或任务推进。

### 大厅与 17F04

- 大厅 `INCOMING VOICE MESSAGE` 展开卡的黑色衬底内缩到特殊框内部；信息内容和出现时机不变。
- 17F04 `WELCOME HOME` 只显示初始一次；Lily 留言开始后关闭 Welcome 内容，二者不再半透明重叠。
- TV4 相册的 Field Unit 区增加可缩放 V2 特殊框，不改变照片、Photo Camera 和相册世界空间挂点。
- F04 Prefab 编辑预览隐藏不属于本户的 Before/After；这只修编辑视图，正式游戏的 Enter Home 与剧情流程不变。

### 明确未改变

- 不改 Lobby→F01→F02→F03→F04 顺序，不改 E/Hold E/Space 规则，不改住户选择结果、信任、结局条件、Camera Anchor、模型站位、正式台词、Line ID 或音频素材。

## 2026-08-10 17F03 检查界面与回放防卡死修复

### 17F03

- Entity Inspection 删除没有信息的 `PHYSICAL UNIT FEED`、旧右下角 Field Unit、旧准星与页脚，只保留有效状态数据。
- `POWER STATE / MEMORY ARCHIVE / MOTOR RESPONSE / LAST EVENT` 改成 2×2 数据区；Recall 模式只保留 Space 调取回忆入口。
- 处置选择继续使用全屏 0.82 黑色半透明遮罩；A/B 与操作提示只在可选状态显示，提交后立即隐藏。
- 正式 Field Unit 字幕使用终端同一 Theme 字号和 ScalableFrame，位于检查面板底部固定区域，不再覆盖右下角数据；第一次打开 Entity Inspection 时，首句语音、Field Unit 框和 Space 提示必须同时出现，不能等玩家再按一次 Space 才补出框。
- Recall 蓝色按钮内部固定显示 `RECALL TODAY'S EVENT [SPACE]`；不再把提示文字放到总面板外侧。
- 选择遮罩属于 `17F-03 DISPOSITION` 总面板内部层级：基础数据、标题和已结束的 Field Unit 解释层在遮罩下，A/B 选项在遮罩上；进入选择时 Field Unit 框已经隐藏。
- 从回忆开始到安全返回人类视角并恢复实体检查前，手动 R 视角切换被剧情 Owner 临时锁定；正常完成、取消或对象停用都会释放。该修复只防止回放中途切走造成 Space/E 无效，不改变自动切换、对白、锚点或关卡顺序。

### F01 / F02 门口终端与 F04 Home Terminal

- A/B 选择一经提交，旧的 `UP / DOWN SELECT`、`SPACE CONFIRM`、焦点文字和高亮一并关闭。
- F04 保持 `WELCOME HOME` 只出现一次；Lily 正式留言开始时确保留言 Surface 所在页面处于可见层级，修复“能听到女儿声音但没有文字/对话框”。
- 本次不改正式英文文本、Line ID、AudioClip、任务顺序、信任结算或两条结局条件。

### Lobby 与相册

- Lobby 左侧 `FIELD COMPANION UNIT / ACTIVATED` 信息卡停止显示；这只删除重复说明，不影响 Field Unit 正式开场对白、任务或 Lily 消息。
- TV4 相册底部 Field Unit 区只保留切角 `ScalableFrame`，删除重叠的普通矩形 Outline；对白内容、Space 规则和照片页顺序不变。

## 2026-08-11 终端导航、17F03 说明/选择时序与 Hold E 视觉修复（已实现）

### F01–F03 门口终端

- 用户当前手调完成的 `Terminal_17F01_V2` 顶部导航作为唯一视觉基准；F02/F03 的 Before、After、主操作按钮和分隔线只同步位置、尺寸、间距和字号。
- F02/F03 的户号、人物名称、Review/Enter Unit 文字、UnityEvent、页面状态和进入关卡条件保持不变。

### 17F03 Entity Inspection

- Field Unit 说明框使用 F01 终端完整规格：1460×248，Speaker 52、正文 26、Space 26，并保留相同切角框与内边距。
- 流程明确分为：Recall → Field Unit Explanation → 等待 Space 完全松开并额外跨一帧 → A/B Choice → Submitted。
- Field Unit 讲话时没有选择黑幕和 A/B；最后一句结束后先隐藏对白框，再用全屏 0.82 黑幕覆盖检查内容，最后把 A/B 和 `UP / DOWN SELECT · SPACE CONFIRM` 放在黑幕之上。
- 上一句对白的 Space 不再自动提交默认 A；A/B 提交后遮罩、选项和提示一起关闭。
- 本轮不改变英文对白、Line ID、AudioClip、信任结算、处置结果、任务顺序、Camera Anchor 或住户流程。

### Hold E

- 只替换为蓝青切角框和琥珀进度反馈；E 键、1.5 秒持续、松开取消、完成条件与现有音效逻辑不变。
- 装饰框由可编辑 SVG 和 2× 透明 PNG 提供，文字、百分比和进度条继续由运行时组件显示。

## 2026-08-11 简化 UI 框、17F03 宽版衬底与提示位置确认（已实现）

- 用户否决上一版偏花哨的框体；正式视觉改为单层细线、少量切角、无左侧半圆/多棱装饰、无额外内框。SVG 不包含文字、按键或进度条。
- 17F03 `Entity Inspection` 外衬底扩展为 1600×932，仍以 1920×1080 居中；2×2 状态数据、Recall 按钮和标题保持居中，1460×248 的 Field Unit 区完整落在衬底内。
- 17F03 Field Unit、Lobby Field Unit 与 E/Hold E 分别使用可编辑简化 SVG 和 2× PNG；旧直接矩形/折线边框在正式层级中停用，避免双层框。
- 17F04 Lily 的 `SPACE CONTINUE` 固定在留言框右下角，字号 26、琥珀色；不改变 Lily 留言文字、音频、Surface 显隐或 Space 推进。
- Human Final Response 的两个正式页面都使用相同居中内容区，A/B 与 `UP / DOWN SELECT  SPACE CONFIRM` 位于选择层内部；只改视觉坐标，不改默认焦点、选择结果或结局条件。
- 本次没有改变 Lobby→F01→F02→F03→F04 流程、任何 Line ID、AudioClip、任务条件、Camera Anchor、E/Hold E/Space 规则或 36 个正式 SFX Cue。

## 2026-08-13 全项目双字体视觉规则（已实现）

- 所有 Human/Companion HUD、终端、检查、相册、按钮、任务、操作提示、字幕说话人名称和 `SPACE CONTINUE/CONFIRM` 统一使用 Oxanium。
- 所有真正表达的对白句子统一使用 Chakra Petch，包括框式对白正文、自然字幕、黑幕对白、Field Unit 与 Lily 留言正文。
- 正式字体由 `Hearth_UiV2Theme.asset` 的 UI/Dialogue 两个独立字段控制；运行时动态 Surface 也遵循同一规则。
- 本次只调整字体角色，不改变任何英文台词、Line ID、AudioClip、对白顺序、Space/自动推进策略、任务、信任、处置、Camera、E/Hold E、关卡顺序或结局条件。
