# HEARTH 下一阶段：当前系统盘点

> 本文记录 2026-07-23 的项目文件事实。Unity MCP 在本轮 Codex 会话中没有暴露可调用工具，且本地 HTTP MCP 端点未连通，因此本轮没有对编辑器 Play Mode 状态作实时确认；场景和程序结论来自当前项目文件、Scene YAML、Prefab、资源与代码审计。

## 一、当前项目规模

| 项目 | 当前结果 |
|---|---:|
| Build Settings 正式场景 | `Assets/Scenes/SampleScene.unity` |
| `Assets/Scripts` C# | 109 |
| `Assets/Editor` C# | 47 |
| `HearthDialogueSequence` 资产 | 71 |
| 对白行 | 398 |
| 已绑定对白语音 | 0 |
| 项目音频文件 | 15 |
| 17F 终端整页 PNG | 24 |
| Audio Mixer 资产 | 0 |
| 使用旧 `Input.GetKey/GetAxis` 的运行时脚本 | 13 |

旧文档中的 `34 个 Dialogue Asset / 215 行对白` 已经过时，后续应使用本表的新统计。

## 二、场景与关卡

当前正式流程集中在 `SampleScene`，包含：

- 一楼大堂出生、三组公共事件、任务终端、电梯。
- 17F01 小男孩家庭。
- 17F02 夫妻家庭。
- 17F03 父母与女儿家庭。
- 17F04 Mia 自宅与最终选择。
- 玩家与陪伴单元双视角。
- 人类 HUD、陪伴单元 HUD、World Space TV 终端。
- 信任度、处置记录、设置、地点 HUD 和相框流程。

当前优点是所有主流程都在一个场景中，调试路径短。当前风险是同一 Scene 内存在大量运行时演员、参考模型、锚点、流程根和 UI 根，若缺少统一状态机，很容易发生重复显示、碰撞残留、输入抢占和相机引用串线。

## 三、当前对白系统

### 已有能力

- `Assets/Scripts/MinLoop/MinLoopSubtitlePlayer.cs`
  - 播放 `HearthDialogueSequence`。
  - 支持标准对白、黑幕对白和时间卡。
  - 有语音时可按 `AudioClip.length` 控制时长。
  - 无语音时使用 `holdSeconds`。
- `Assets/Editor/HearthFinalDialogueSync.cs`
  - 从正式稿同步 Dialogue Asset。
  - 支持正式稿覆盖率检查。
  - 支持两行字幕验证。
- `Assets/Editor/HearthDialoguePresentationBinder.cs`
  - 把四关对白绑定到共享字幕播放器和共享样式。

### 当前问题

1. 对白默认自动计时推进，玩家不能逐句确认下一句。
2. 正式人物对白、Mia 自言自语、Field Unit 通讯和决策反馈共用过多视觉规则。
3. 四个关卡中的控制锁定规则不一致：
   - 有些对白锁移动但可转头。
   - 有些对白允许自由移动。
   - 有些流程由 Trigger 自动开始。
4. 398 行对白目前没有任何语音绑定，无法验证真实语速、停顿和字幕时长。
5. 当前共享样式有利于统一修改，但不足以表达不同叙事层级。

## 四、当前交互系统

### 已有能力

- `Assets/Scripts/Interactions/PlayerInteraction.cs`
  - 从屏幕中心发射射线。
  - 统一用 E 触发。
  - 支持交互距离、提示文本和英文字符归一化。
- 长按交互由陪伴单元 HUD 的 Hold Prompt 和各关流程共同控制。
- TV、门、相框、实体机器人和剧情目标已经有不同交互组件。

### 当前问题

1. 一楼三组公共事件由 `HearthLobbyConversationZone` 的 Trigger 自动触发，不符合老师提出的“玩家主动按 E”。
2. 一次按 E、长按 E、Space 确认、方向键选择分别散落在不同控制器中。
3. 没有统一的“交互状态说明”：
   - 当前可交互。
   - 条件未满足。
   - 正在长按。
   - 已完成。
   - 本轮不可重复。
4. 项目仍使用旧 Input Manager 轮询，后续键位重绑、VR 和输入提示自动适配会比较困难。

## 五、当前终端系统

### 已有能力

- `Assets/Scripts/UI/HearthHud/HearthTvTerminalController.cs`
  - 终端开关。
  - 0.5 秒镜头过渡。
  - 开机闪烁。
  - 多页切换。
  - 回放/入户主操作。
  - A/B 选择。
  - 音效接口。
- 一楼任务终端和 17F04 自宅终端使用 Unity TMP/UI 组件。
- 17F01、17F02、17F03 使用 24 张整页 PNG。

### OCR 核对结果

本轮调用 PaddleOCR MCP OCR 检查了：

`Assets/Resources/UI/HearthTerminalSlideImages/TerminalImageSlide01.png` 至 `TerminalImageSlide24.png`

当前结构是：

| 终端 | 页面 | 当前内容 |
|---|---|---|
| 17F01 | 1-8 | 住户摘要、获取背景、家庭日志、信任趋势、检查历史、A/B 处置 |
| 17F02 | 9-16 | 住户摘要、获取背景、家庭日志、信任趋势、检查历史、A/B 处置 |
| 17F03 | 17-24 | Alert 信息、背景、事件、信任、检查历史、入户、A/B 处置 |

### 当前问题

1. 信息量过大，玩家需要翻阅 5 个以上栏目才能理解核心情境。
2. 终端同时承担“查资料、进入回放、选择处置、显示结果”四种职责。
3. 终端 PNG 文本不可动态排版、不能响应字号设置，也不利于后续本地化。
4. A/B 已经烘焙在整页图中，不符合老师提出的“分析完成后再出现独立决策”。
5. 控制器支持键盘导航，却仍有解锁鼠标光标的逻辑，可能产生“显示了鼠标但不能有效点击”的体验。
6. 一楼任务终端是一次性流程，而新分析任务要求信息可重复查询，需要重新确认哪些信息可重复、哪些剧情只播一次。

## 六、当前人类 HUD

### 已有能力

- `Assets/Scripts/UI/HearthHud/FirstPerson/HearthFirstPersonHudController.cs`
  - Tab 菜单。
  - 历史记录。
  - 设置。
  - 信任度反馈。
  - 警告与最终选择。
- 地点、当前任务、Lily 消息和一次性提示已经有独立显示逻辑。

### 当前问题

1. 左下任务主要仍是固定的 `NIGHT ROUNDS - BLOCK A - 17F`，没有完整的阶段驱动目标。
2. 系统设置已有 Master/Dialogue/Ambient/SFX，但缺少：
   - 鼠标灵敏度。
   - 字幕字号。
   - 字幕背景强度。
   - 键位重绑。
3. Tab 菜单、HUD 任务、剧情对白、终端和决策之间没有统一的输入焦点管理。
4. 一些装饰性文本占据视觉层级，但没有提供操作或剧情信息。

## 七、当前陪伴单元 HUD

### 已有能力

- `Assets/Scripts/UI/HearthHud/Companion/HearthCompanionHudController.cs`
  - 14 个数据驱动页面。
  - 右上决策区。
  - 左下数据流。
  - 临时监控卡。
  - 长按交互。
  - 方向引导。
  - 故障、黑场和深眠特效。
- 共享布局资产可统一调节区域位置、文字和缩放。
- 机器人底框使用固定图，动态内容使用 Unity UI/TMP。

### 当前问题

1. “正式回放”“机器人实时判断”“玩家操作提示”同时出现时，信息层级仍然偏满。
2. 右上、左下和中央提示缺少严格的互斥规则。
3. 回放模式缺少持续且明确的 `ARCHIVED PLAYBACK / TIMESTAMP / ROLE` 标识。
4. 回放中的目标顺序主要由关卡控制器决定，玩家不容易理解为什么当前只能和某一个 NPC 交互。

## 八、当前关卡流程代码

主要控制器：

- `MinLoopFlowController`
- `HearthCompanion17F01ReplayController`
- `HearthCompanion17F02ReplayController`
- `HearthCompanion17F03ReplayController`
- `Hearth17F04FinaleController`
- `HearthLobbyFlowController`

这些控制器已经实现大量具体流程，但每户都逐渐形成自己的输入、字幕、演员显隐、相机、门和 UI 管理逻辑。继续在每个控制器里单独补规则，会增加以下风险：

- 同一种交互在四关表现不同。
- 某关修复输入锁后影响另一关。
- 对白样式修改需要重复检查。
- 演员和参考模型状态残留。
- 终端、回放和选择互相抢控制权。

## 九、当前音频系统

### 已有能力

- `HearthAudioSettingsController`
- `HearthAudioChannelSource`
- `HearthSfxCuePlayer`
- 人类与机器人不同 Footstep Profile。
- 多个剧情节点已经预留 AudioClip 字段。

### 当前问题

1. 没有 Audio Mixer 资产。
2. 当前 Master/Dialogue/Ambient/SFX 主要靠脚本乘音量，而不是 Mixer Group。
3. 没有统一的对白 ducking、室内外快照、终端视角滤波和回放滤波。
4. 398 行对白语音绑定为 0，音频驱动字幕尚未得到真实验证。
5. 项目音频文件只有 15 个，环境音、终端循环、门、电梯、UI、机器人机械声仍不完整。

## 十、可以直接复用的部分

- 当前正式稿与稳定标记同步机制。
- `HearthDialogueSequence` 数据资产。
- 共享字幕样式的基础能力。
- 中心射线 E 交互。
- 0.5 秒镜头过渡。
- 终端开机闪烁与音效接口。
- 信任度与处置记录。
- 14 个陪伴单元 HUD 数据页。
- 现有关卡演员、锚点、路径和显隐绑定。
- 人类/机器人独立脚步接口。

## 十一、不建议继续沿用的部分

- 用整页 PNG 承载大量终端文字。
- 在终端内部完成最终处置。
- 所有对白只靠自动计时推进。
- Trigger 自动强制公共事件。
- 关卡各自维护一套输入锁和提示规则。
- 固定左下任务文案。
- 继续使用旧 Input Manager 扩展复杂重绑和 VR。
- 只靠脚本音量而不建立 Audio Mixer 总线。

## 十二、当前 UI 与 Prefab 对照

| 当前 UI | 主要 Prefab/资产 | 主要控制脚本 | 当前判断 |
|---|---|---|---|
| 人类 HUD | `Assets/Prefabs/UI/HearthHud/HearthHudRoot.prefab` | `HearthFirstPersonHudController` | 保留并瘦身 |
| Tab/历史/设置 | `Assets/Prefabs/UI/HearthHud/FirstPersonPages/Slide03-24_*.prefab` | First Person HUD 系列脚本 | 保留功能，重新统一样式和输入 |
| 陪伴单元 HUD | `Assets/Prefabs/UI/HearthHud/Companion/HearthCompanionHudRoot.prefab` | `HearthCompanionHudController` | 保留数据结构，减少同时显示信息 |
| 共享字幕 | `Assets/Data/MinLoop/UI/Hearth_SubtitleStyle.asset` | `MinLoopSubtitlePlayer` | 保留资产，扩展为多种对白模式 |
| 17F01 终端 | `Assets/Prefabs/UI/HearthHud/Terminals/Terminal_17F01.prefab` | `HearthTvTerminalController` | 保留空间结构，替换 8 张页面 |
| 17F02 终端 | `Assets/Prefabs/UI/HearthHud/Terminals/Terminal_17F02.prefab` | `HearthTvTerminalController` | 保留空间结构，替换 8 张页面 |
| 17F03 终端 | `Assets/Prefabs/UI/HearthHud/Terminals/Terminal_17F03_Alert.prefab` | `HearthTvTerminalController` | 保留 Alert 变体，替换 8 张页面 |
| 一楼任务终端 | `Assets/Prefabs/UI/HearthHud/Terminals/Terminal_Lobby_Assignment.prefab` | TV + Lobby Flow | 保留并改为剧情一次、信息可重看 |
| 17F04 终端 | `Assets/Prefabs/UI/HearthHud/Terminals/Terminal_17F04_Home.prefab` | TV + Finale | 保留入口能力 |
| 24 张终端图页 | `Assets/Prefabs/UI/HearthHud/TerminalImagePages/*.prefab` | Terminal Controller | 退为视觉/文字参考 |
| 旧 Unity 重绘终端页 | `Assets/Prefabs/UI/HearthHud/TerminalPages/*.prefab` | Builder/Terminal | 审计后归档，避免与 ImagePages 双轨 |

## 十三、当前交互与状态对照

| 交互/状态 | 当前触发 | 当前控制 | 主要脚本 | 下一阶段判断 |
|---|---|---|---|---|
| 普通 E | 中心射线 | 由对象自行处理 | `PlayerInteraction` | 保留，统一可用状态和提示 |
| Hold E | 关卡条件 + HUD | 各户控制器 | Companion Hold Prompt | 保留，接入统一状态 |
| 大厅公共事件 | Trigger Enter/Stay | 锁移动、可转头 | `HearthLobbyConversationZone` | 改为范围内主动 E |
| 正式对白 | 自动时长 | 各关决定 | `MinLoopSubtitlePlayer` | 增加玩家推进与统一锁定 |
| Mia/Field Unit | 自动时长 | 各关决定 | Subtitle Player/Lobby HUD | 分流为轻量通讯 |
| TV 打开 | E | 锁玩家，切固定相机 | `HearthTvTerminalController` | 保留 |
| TV 翻页 | Tab/方向键 | 终端内 | TV Controller | 压缩到 1-2 页 |
| TV 主操作 | Space | 终端内 | TV Controller | 保留主操作，移除最终选择 |
| 回放 | 终端主操作 | 切机器人控制器 | 三户 Replay Controller | 保留演出，统一回放层 |
| A/B | 终端或人类 HUD | 各关不同 | TV/HUD/Finale | 移到独立 Decision |
| 相框 | E + Space/Esc | 固定相机 | `HearthPhotoFrameInteractable` | 保留，补统一提示 |
| 低信任关闭 | Space 弹窗 | Finale | Shutdown Challenge | 保留玩法，统一遮罩和状态 |

## 十四、旧剧情信息风险

### 已确认仍存在旧信息的位置

24 张终端 PNG 是从早期 PPT 固化而来，包含固定住户年龄、服务时间、使用率、检查记录、信任指数和 A/B 文案。它们不会随正式稿或剧情资产自动更新。

OCR 示例：

- 17F01：`Father 35 / Mother 33 / Son 8`、`Approve Upgrade / Enable Observation`。
- 17F02：`Husband 34 / Wife 32`、`Maintain Current Configuration / Recommend Family Counseling`。
- 17F03：`Father 38 / Mother 36 / Daughter 12`、`Restart and Restore Service`。

这些内容必须逐项与当前正式稿核对，不能因为画面能显示就视为最新设定。

### 已有同步保护的位置

- `HearthDialogueSequence` 使用正式稿稳定标记同步。
- `HearthFinalDialogueSync` 可以检查覆盖率。
- 正式稿变更后应通过工具同步，不应手工改 71 个资产。

### 需要建立的保护

- 终端住户信息也应转为数据资产。
- 决策文案应来自同一住户处置数据。
- HUD、终端、回放和结果页只能引用数据，不再各自保存一份文字。

## 十五、结论

项目现在不是“缺功能”，而是“功能之间缺少统一的体验语法”。下一阶段的重点应从继续增加单点脚本，转为：

1. 先定义 UI 系统边界。
2. 再定义全局输入和对白状态。
3. 用 17F01 做一次完整迁移验证。
4. 通过后批量迁移其余关卡。
