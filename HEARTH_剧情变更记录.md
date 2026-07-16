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
- `SITTING TALKING ON CASUAL_FEMALE_K` 的实际导入文件按用户确认使用 `Assets/Sitting_Talking.fbx`。

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

1. 黑屏恢复、陪伴单元被唤醒后，`casual_Female_K@Sitting_Disbelief` 循环播放 `SittingDisbelief`，Clip 使用 `Assets/casual_Female_K@Sitting_Disbelief.fbx / mixamo.com`。
2. 玩家完成 E 安慰交互后，卧室女主播放一次 `SittingTalking`，Clip 使用 `Assets/Sitting_Talking.fbx / mixamo.com`。`Bedroom Talking Max Seconds` 默认 `10`，设为 `0` 或负数时等待完整动作/字幕。
3. 男主喊吃饭、女主回应后，女主播放 `SitToStand` 一次。
4. 离开卧室时不再使用 `Female_Start_Walking` 或旧 `locom_f_basicWalk_30f`，走路循环改为 `Assets/casual_Female_K@Walking.fbx / Walking`。
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
8. 关闭流程读取既有信任：正信任一次 Space，负信任三段警告各一次 Space。
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
- 低信任关闭暂用三次 Space；以后可替换为滑块/中央判定小游戏。
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
