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
