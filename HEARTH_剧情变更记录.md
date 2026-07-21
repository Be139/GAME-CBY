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
