# HEARTH 陪伴单元机器人 HUD 调参入口

> 后续调整陪伴单元机器人视角 UI、字幕、短时监控卡、长时决策区之前，优先读这份文档。  
> 这里记录的是“去哪里调”和“调了会影响谁”，不替代 `脚本使用说明总表.md`。

## 一句话原则

机器人 HUD 分成两层：

- **通用版式层**：位置、大小、字体大小、线条长度、面板区域。主要在 `HearthCompanionHudRoot` 里面调。第 1 户、第 2 户、第 3 户共用。
- **每页内容层**：每页显示什么文字、短时卡什么时候出现、显示多久、长按按钮文案。主要在 `Assets/Data/HearthHud/Companion/CompanionScene_*.asset` 里面调。只影响对应页面。

如果你现在在 17F01 最小循环里把通用版式调舒服了，后面 17F02 / 17F03 只要继续使用同一个 `HearthCompanionHudRoot`，位置和大小会跟着统一使用。

## 当前场景里调位置和大小

在 Hierarchy 里找到：

`HearthCompanionHudRoot`

然后展开下面这些层级。

| 想调的 UI | 层级路径 | 主要调什么 |
| --- | --- | --- |
| 机器人视角底图边框 | `FrameLayer / CompanionRobotFrame` | 整体底图大小、铺满方式 |
| 右上角长期决策区 | `PersistentInfoLayer / DecisionPanel` | 右上角标题、正文、竖线位置和大小 |
| 左下角长期数据流 | `PersistentInfoLayer / DataStreamView` | 左下角数据流位置、宽度、字体大小 |
| 底部中间模式文字 | `PersistentInfoLayer / ModeLabelText` | 底部模式文字位置、字体大小 |
| 中间提示文字 | `PersistentInfoLayer / CenterMessageText` | 中央系统提示的位置、字体大小 |
| 左上角短时监控卡 | `TimedCardLayer / TriggerCardView` | 短时出现的监控卡位置、大小、内部文字大小 |
| 长按 E 提示 | `InteractionLayer / HoldPrompt` | 长按框、进度条、提示文字的位置和大小 |
| 视线引导 | `InteractionLayer / DirectionGuide` | 方向提示文字和标记位置 |
| 投影面板 | `ProjectionLayer / ProjectionPanel` | 家庭日志/投影信息面板位置和大小 |
| 故障/黑屏/深眠效果 | `SpecialEffectsLayer / SpecialEffectsView` | 黑场、特效文字、状态文字位置 |

选中对象后，在 Inspector 里看 `RectTransform`：

- `Pos X / Pos Y`：调位置。
- `Width / Height`：调区域大小。
- `Scale`：整体缩放。可以临时试效果，但不建议长期靠它精修。
- 子物体里的 `TMP Text` 的 `Font Size`：调文字大小。
- 子物体里的 `TMP Text` 的 `Alignment`：调左对齐、居中、右对齐。

推荐优先改 `Width / Height / Font Size`，少用 `Scale`。`Scale` 会把文字、线条、进度条、交互区域一起缩放，后面更难精确维护。

## 短时间出现的 UI 怎么调

左上角短时监控卡由两个地方共同决定：

### 1. 位置和大小

调这里：

`HearthCompanionHudRoot / TimedCardLayer / TriggerCardView`

这里决定它出现在左上、左中、还是其他位置，以及整个卡片占多大。

### 2. 文字、延迟、显示多久

调这些资产：

`Assets/Data/HearthHud/Companion/CompanionScene_*.asset`

比如：

- `CompanionScene_01_17F01_01.asset`
- `CompanionScene_02_17F01_02.asset`
- `CompanionScene_03_17F01_03.asset`

Inspector 里重点看：

- `Timed Cues`
- `Delay Seconds`：这条短时 UI 几秒后出现。
- `Visible Seconds`：显示多久后淡出。
- `Title`：短时卡标题。
- `Body`：短时卡正文。

如果 `Timed Cues` 为空，才会使用旧字段：

- `Show Trigger Card`
- `Trigger Card Title`
- `Trigger Card Body`
- `Trigger Card Delay`
- `Trigger Card Seconds`

现在建议优先用 `Timed Cues`，因为它支持一个页面里连续出现多条短时监控信息。

## 长时间显示的 UI 怎么调

长期显示的 UI 也分两部分：

### 推荐的统一调节入口

共享布局资产：

`Assets/Data/HearthHud/Companion/Hearth_CompanionHudLayout.asset`

这是目前右上决策区和左下数据流的正式统一入口：

- `Global Region Scale`：同时放大或缩小右上、左下两块区域以及其中的线框图形。
- `Global Text Scale`：同时调整两块区域全部 TMP 文字的字号。
- `Shared Horizontal Inset`：同时让两块区域向屏幕内部收拢或向外展开。
- `Shared Vertical Offset`：同时让两块区域向下或向上移动。
- `Decision Offset`：只微调右上决策区。
- `Data Stream Offset`：只微调左下数据流。

保存该资产后，场景中的 `HearthCompanionHudRoot / HearthCompanionHudLayoutController` 会在编辑模式实时预览。下一次 Play 直接生效，不需要重跑 Builder。17F01、17F02、17F03 的所有机器人 HUD 页面共用同一份布局。

如果只是想统一放大两块 UI，优先调 `Global Region Scale` 与 `Global Text Scale`，不要分别拖动两个区域，否则以后很难保持两边一致。

### 位置和大小

在 `HearthCompanionHudRoot` 里调：

- 右上角：`PersistentInfoLayer / DecisionPanel`
- 左下角：`PersistentInfoLayer / DataStreamView`
- 底部中间：`PersistentInfoLayer / ModeLabelText`
- 中央提示：`PersistentInfoLayer / CenterMessageText`

这些是通用版式。调完后，第 2 户、第 3 户会沿用。

### 内容文字

在对应的 `CompanionScene_*.asset` 里调：

- `Decision Kicker`：右上角小标题，例如 `SYNTH VOICE - DECISION`
- `Decision Title`：右上角主标题
- `Decision Body`：右上角说明正文
- `Data Stream Title`：左下角数据流标题
- `Data Stream Lines`：左下角数据流每一行
- `Mode Label`：底部中间模式文字
- `Center Message`：中间系统提示

这些只影响该 asset 对应的机器人 HUD 页面。

## 长按 E 和交互提示怎么调

### UI 位置和大小

调这里：

`HearthCompanionHudRoot / InteractionLayer / HoldPrompt`

常见可调对象：

- `HoldPromptBox`：提示框底板。
- `HoldPromptText`：主提示文字。
- `HoldKeyText`：按键文字，例如 `E`。
- `HoldProgressText`：`HOLD TO ACT` 文字。
- `HoldProgressBack` / `HoldProgressFill`：进度条背景和填充。

### 文案和长按时长

调对应 `CompanionScene_*.asset`：

- `Show Hold Prompt`：这个页面是否默认显示长按提示。
- `Hold Prompt Text`：提示文案。
- `Hold Key`：默认是 `E`。
- `Hold Seconds`：需要按住多久完成。

注意：17F01 当前正式流程里，床边交互不是一开始就显示，它还受剧情阶段、对话是否播完、射线是否看向目标、距离是否满足影响。

## 字幕位置、大小和时长怎么调

字幕不是机器人 HUD 里的 `CompanionScene_*.asset` 控制，而是由最小循环字幕播放器控制。

当前正式统一入口是：

`Assets/Data/MinLoop/UI/Hearth_SubtitleStyle.asset`

优先修改该资产中的 `Standard Dialogue`。它会同时影响四个关卡的普通对白；`Centered Epilogue` 只影响第四关结局黑幕；`Time Card` 只影响结局时间提示。下面列出的 `MinLoopSubtitlePlayer` 字段主要用于检查引用或兼容旧场景，不应再作为四关分别调节的首选入口。

在 Hierarchy 里找：

`MIN_LOOP_ROOT / UI / MinLoopSubtitlePlayer`

Inspector 里重点看这些字段：

- `Use Clean Centered Style`：保持开启。开启后是无黑底、居中白字字幕。
- `Subtitle Width Fraction`：字幕最大宽度。当前建议约 `0.66`，意思是屏幕宽度的三分之二。
- `Speaker Center Y`：说话人名字的垂直位置。数值越小越往下。
- `Body Center Y`：字幕正文的垂直位置。数值越小越往下。
- `Speaker Height Fraction`：说话人名字区域高度。
- `Body Height Fraction`：字幕正文区域高度。
- `Clean Speaker Font Size`：说话人名字字号。
- `Clean Body Font Size`：字幕正文字号。
- `Clean Text Color`：字幕颜色。

如果你觉得名字和正文太高：

- 先把 `Speaker Center Y` 稍微调小。
- 再把 `Body Center Y` 稍微调小。

如果字幕太长、换行不好看：

- 想让一行更长：调大 `Subtitle Width Fraction`。
- 想更早换行：调小 `Subtitle Width Fraction`。
- 想文字更小：调小 `Clean Body Font Size`。

### 字幕内容和每句停留时间

调这些资产：

`Assets/Data/MinLoop/Dialogues/`

当前 17F01 已有：

- `17F01_BedroomPrelude.asset`
- `17F01_BedsideSoothing.asset`
- `17F01_LivingRoomObservation.asset`

每个资产里有 `Lines`：

- `Start Delay`：这句字幕出现前等待多久。
- `Speaker`：说话人名字。
- `Text`：字幕正文。
- `Hold Seconds`：这句字幕最少显示多久。
- `Voice Clip`：之后录好声音后拖到这里。

如果有 `Voice Clip`，字幕会至少显示到音频播放结束。  
如果没有 `Voice Clip`，就按 `Hold Seconds` 控制时长。

`Post Sequence Delay` 是整段字幕播完后的额外等待时间。  
17F01 里“字幕播完后再开放 E 交互”的感觉，主要靠这里和流程脚本里的交互延迟共同实现。

## 一次调整会不会影响第 2 户和第 3 户

会不会跟着变，取决于你调的是哪种内容。

会跟着第 2 户、第 3 户一起变：

- `Hearth_CompanionHudLayout.asset` 中的共享缩放、文字缩放和位置参数
- `HearthCompanionHudRoot` 下未被共享布局控制的通用 UI 物体 `RectTransform`
- 通用 Prefab 中 TMP Text 的对齐方式和颜色
- `MinLoopSubtitlePlayer` 的字幕位置、字号、最大宽度
- 通用 prefab `Assets/Prefabs/UI/HearthHud/Companion/HearthCompanionHudRoot.prefab`

只影响单个页面：

- 某个 `CompanionScene_*.asset` 里的文字
- 某个 `CompanionScene_*.asset` 里的 `Timed Cues`
- 某个 `CompanionScene_*.asset` 里的 `Hold Seconds`
- 某个 `HearthDialogueSequence` 资产里的某一句字幕

## 场景里调和永久默认值的区别

你仍可直接在场景里的 `HearthCompanionHudRoot` 上调未被共享布局控制的对象。右上决策区和左下数据流则优先修改 `Hearth_CompanionHudLayout.asset`。

但要注意：

如果之后运行菜单：

`Tools / Hearth / HUD / Rebuild And Apply Companion Unit HUD To Scene`

场景里的 `HearthCompanionHudRoot` 会被重新生成，手动拖过的位置可能会被覆盖。

对右上/左下两块区域，稳定做法是直接保存共享 Layout Profile。对其他 UI，稳定做法是：

1. 先在场景里手动调到满意。
2. 记下关键对象的 `RectTransform` 数值。
3. 让我把这些数值写回默认生成器。

默认生成器在：

`Assets/Editor/HearthCompanionHudBuilder.cs`

常用位置在这些方法里：

- `BuildDecisionPanel`
- `BuildDataStream`
- `BuildTriggerCard`
- `BuildHoldPrompt`
- `BuildProjectionPanel`
- `BuildDirectionGuide`
- `BuildSpecialEffects`

这些方法里的 `PptRect(...)` 就是默认位置和大小。  
你不需要自己改代码；你把满意后的数值告诉我，我可以帮你写回去。

## 调整时的建议流程

1. 进入 Play Mode 前先确认 Game 视图是 `Full HD (1920x1080)` 或接近 16:9。
2. 先调 `HearthCompanionHudRoot` 的通用位置和大小。
3. 再调 `MinLoopSubtitlePlayer` 的字幕位置和字体。
4. 再调 `CompanionScene_*.asset` 里的每页文字和短时卡时间。
5. 退出 Play Mode 后再保存调整。Play Mode 里改的很多数值不会保留。
6. 如果版式已经满意，让 Codex 把这些数值固化进 `HearthCompanionHudBuilder.cs`。

## 快速排查

如果 UI 位置突然回去了：

- 检查是否刚运行过 HUD 重建工具。
- 检查你改的是场景实例，还是 prefab。
- 检查当前场景里是否有多个 `HearthCompanionHudRoot`。

如果短时卡一直不消失：

- 检查对应 `CompanionScene_*.asset` 的 `Timed Cues / Visible Seconds` 是否太长。
- 检查 `HearthCompanionTriggerCardView / Fade Seconds` 是否异常。

如果字幕太高：

- 调小 `MIN_LOOP_ROOT / UI / MinLoopSubtitlePlayer / Speaker Center Y`。
- 调小 `Body Center Y`。

如果字幕有黑底：

- 确认 `Use Clean Centered Style` 开启。
- 确认 `Subtitle Panel` 上的 Image 颜色不是黑色不透明。

如果第 2 户、第 3 户没有沿用版式：

- 检查它们是否使用同一个 `HearthCompanionHudRoot`。
- 检查有没有单独复制出新的机器人 HUD 根节点。
- 检查是否重新运行过 Builder，但没有把第 1 户调好的数值写回默认生成器。
