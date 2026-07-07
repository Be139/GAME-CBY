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
