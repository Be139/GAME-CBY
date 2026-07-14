# HEARTH 17F03 关卡制作日志

## 文档用途

本文件记录第三户 17F03 的场景实现、对象绑定、阶段验收和待完善项。继续制作 17F03 前，先读本文件，再读 `HEARTH_剧情变更记录.md`、`HEARTH_剧情接口与可接入点.md` 和 `脚本使用说明总表.md`。

## 当前流程图

```text
17F03 门口终端 ENTER UNIT
-> 渐黑进入米娅室内视角
-> 第一幕父母对话
-> 对话结束后开放实体 ROBOT 的 E 检查
-> 故障检查 UI，Space 调取当天记录
-> 第二幕机器人中午回放：女儿 -> 母亲顺序传话
-> 第三幕夜间回放：开门 -> 女儿走近 -> 倾诉 -> Entering Code
-> 深眠与黑场
-> 返回米娅，播放系统说明
-> 渐黑回到 17F03 门口终端
-> A/B 处置，只允许提交一次
```

## 核心对象表

| 用途 | 当前对象 |
|---|---|
| 正式人类控制器 | `Player/Person Controller` |
| 正式机器人控制器 | `Player/Robot Controller` |
| 人类入户参考 | `Person Controller (1)` 生成的位置/相机锚点 |
| 第二幕机器人参考 | `Robot Controller (4)` 生成的位置/相机锚点 |
| 第三幕机器人参考 | `Robot Controller (5)` 生成的位置/相机锚点 |
| 第一幕父亲 | `Doctor_Male_B@Male_Sitting_Pose` |
| 第二幕父亲 | `Doctor_Male_B@Male_Sitting_Pose (1)` |
| 母亲 | RuntimeRoot 下的正式母亲模型；参考模型只提供位置和朝向 |
| 女儿 | RuntimeRoot 下的正式女儿模型；第二幕/第三幕复用同一演员 |
| 实体陪伴单元 | `ROBOT`，子物体 `InteractionVolume_17F03` 负责 E 判定 |
| 第三幕房门 | `Door_2_Brown (7)` |
| 第三幕女儿终点 | `casual_Female_K (3)` 生成的锚点 |
| 门口终端 | `17F/ROOM2/TV (4)/MonitorCanvas/Terminal_17F03_Alert` |

参考模型只在编辑器里帮助调整位置和朝向；Play Mode 必须隐藏 Renderer、关闭 Collider，不能作为第二位运行时演员。

## 2026-07-11 当前完成状态

### 已完成并验证

- 17F03 终端显式绑定 `Resident Id = 17F03`、`Primary Action = EnterUnit`。
- 正式流程只使用一个人类控制器、一个机器人控制器和正式 `ViewSwitchController`；参考控制器只提供锚点。
- 第一幕、第二幕、第三幕演员分幕显隐逻辑已建立。
- 第二幕女儿 -> 母亲顺序目标与中下方长按 E 提示已建立；只有中心射线命中当前目标才显示。
- 第三幕门先打开，女儿再沿路径走到夜间终点；第三幕视角使用独立机器人相机锚点。
- 第三幕结束后返回 17F03 终端，不再错误打开 17F02 终端。
- `Time.timeScale` 已恢复并验证为 `1`。
- 实体 ROBOT 的交互 Collider 已按世界尺寸正确换算；父母台词结束后能被人类中心射线识别。
- 人类交互提示已绑定到 `HearthHudRoot/InteractionPromptLayer/PlayerInteractionPrompt`。
- 人类交互提示层固定为 `HearthHudRoot` 最后一个 sibling，避免被全屏页面遮挡。
- `Tools / Hearth / Replay / Validate 17F03 Minimal Loop Setup` 当前通过。
- 第三幕女儿到终点后已从 `Walk` 直接切换为循环 `Talking`；关机前对白结束后才播放 `EnteringCode`。
- 17F03 正式演员上的 `CityPeople` 随机自动动画已关闭，不再抢占剧情 Animator 状态。
- 正式米娅、正式机器人和三个参考控制器的占位胶囊 Renderer 已关闭；碰撞与正式控制逻辑保留。

### 本轮实测数据

- 实体 ROBOT 交互体世界尺寸约 `0.77 x 1.43 x 1.01`。
- 人类中心射线命中 `InteractionVolume_17F03` 后，`PlayerInteraction.CurrentInteractable` 为 `Hearth17F03UnitInteractable`。
- 显示文本为 `E INSPECT COMPANION UNIT`。
- 当前运行时间倍率为 `1`。
- 运行时状态检查：女儿能够分别进入 `Walk / Talking / EnteringCode`；第一幕父亲、第二幕父亲、母亲在第三幕均为 inactive。
- 视觉检查：第三幕终点机位中女儿为 Talking 姿态，画面中没有米娅或参考控制器胶囊体。

## 可维护入口

- 流程控制器：`MIN_LOOP_ROOT/ReplayRoom_17F03/HearthCompanion17F03ReplayController`。
- 字幕与语音：`Assets/Data/MinLoop/Dialogues/17F03_*.asset`；每句可改 Speaker、Text、等待、显示时长和 Voice Clip。
- 第三幕关机时机：`17F03_NightShutdownLeadIn.asset` 播完后才切 `EnteringCode`；输入代码期间字幕在 `17F03_NightShutdownAction.asset` 修改。
- 机器人 HUD 页面：`Assets/Data/HearthHud/CompanionScenes/17F03_*.asset`。
- 人类实体 ROBOT 提示：`HearthHudRoot/InteractionPromptLayer/PlayerInteractionPrompt`。
- 第二幕/第三幕长按框：`HearthCompanionHudRoot` 下的 `HearthCompanionHoldPrompt`；页面文案和时长由对应 Scene Data 控制。
- 锚点：移动参考控制器/参考演员后，只有显式运行“从参考重建锚点”菜单才覆盖正式锚点。
- 自动绑定：`Tools / Hearth / Replay / Apply 17F03 Minimal Loop Setup`。
- 场景校验：`Tools / Hearth / Replay / Validate 17F03 Minimal Loop Setup`。

## 待完善与已预留接口

- 语音素材尚未完整接入；在对应 `HearthDialogueSequence / Lines / Voice Clip` 拖入 AudioClip 即可。
- 第三幕女儿专用开门动作暂未接入，当前由门脚本先开门；演员动画接口保留。
- 系统说明段、环境音、按键音和故障音可继续通过 Dialogue Clip、AudioSource 与现有 UnityEvent 接入。
- 存档系统尚未正式接入；A/B、住户完成、信任度和历史记录已有公开接口。
- VR 输入尚未接入；可复用 `OpenUnitInspection()`、`BeginRecordedReplay()`、`ConfirmCurrentGazeTarget()` 和 A/B 接口。

## 当前非本关卡新增的场景问题

- 场景里仍有若干旧 `Door` Missing Script 报错。
- 17F03 正式剧情演员上的 `CityPeople` 已禁用；场景其他旧人物资产仍可能产生第三方 Animator 状态报错。
- ROOM2/ROOM3 部分家具的负缩放 BoxCollider 会报几何警告。

这些问题不会改变本轮三户交互修复结论，但正式演示前建议单独做一次场景清理，不应和 17F03 流程脚本混在同一次修改中。

## 2026-07-14 门与检查镜头验证

### 已完成并验证

- `Door_2_Brown (7)/Door` 的铰链轴位于门扇侧边，轴位置正确；开启方向已统一为 local Y `-90`。
- 修复了 `SmartDoorController` 在 `Rotate` 模式结束帧仍写入滑动位移的通用问题。运行时实测门扇打开后为 `IsOpen = true`、local rotation `(0, 270, 0)`，local position 保持关闭时位置，不再滑进墙体。
- 新建并绑定 `UnitInspectionCameraTransition_17F03`。实测从人类视角进入检查后为 `UnitInspection`、人类 Camera 关闭、实体 ROBOT Camera 开启、黑场 alpha `0`；退出后人类 Camera 恢复、实体 ROBOT Camera 关闭。

### 可调入口

- 门的速度与方向：`17F/ROOM2/Door_2_Brown (7)/Door / SmartDoorController`。
- 检查镜头速度与缓动：`MIN_LOOP_ROOT/ReplayRoom_17F03/UnitInspectionCameraTransition_17F03`。
- 若要让女儿在门口等待更久，再调整 `HearthCompanion17F03ReplayController / After Door Open Seconds`；不要用门的旋转角度代替等待时间。
