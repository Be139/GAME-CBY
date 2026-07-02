# HEARTH 剧情接口与可接入点

> 后续做剧情、字幕、语音、处置记录、信任度或关卡流程前，先读 `HEARTH_剧情变更记录.md`，再读这份文档，最后读 `脚本使用说明总表.md` 中对应脚本条目。
> 如果某个住户的最新剧情变更记录和旧接口说明冲突，先以 `HEARTH_剧情变更记录.md` 的最新记录作为当前制作意图，再决定是否同步修改脚本、字幕资产或正式策划案。

## 最新剧情变更入口

- `HEARTH_剧情变更记录.md`：记录用户口述的剧情、流程、演出、角色走位、UI 触发条件等调整。
- 当前 17F02 最新剧情以 `2026-07-02 17F02 剧情与游戏进程调整` 为准；其中字幕资产和当前场景路径已在 `2026-07-02 17F02 剧情与走位同步实现` 中同步。
- 17F02 中女主离开卧室的路径点已写入当前场景：`Anchor_Wife_17F02_Path01 -> Anchor_Wife_17F02_Path01 (1) -> Anchor_Wife_17F02_Path01 (2) -> Anchor_Wife_17F02_Path01 (3) -> Anchor_Wife_17F02_Path01 (4) -> Anchor_Wife_17F02_Path01 (5) -> Anchor_Wife_17F02_DoorPause -> Anchor_Wife_17F02_ExitOutside`。

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
