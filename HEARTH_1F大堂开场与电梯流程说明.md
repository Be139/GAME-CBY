# HEARTH 1F 大堂开场与电梯流程说明

## 一、正式流程

```text
Person Controller (4) 大堂出生
  -> 开场对白（移动和视角均锁定）
  -> 大堂自由探索
     -> 三组 NPC 对白可选、任意顺序（只锁移动，保留视角）
  -> TvUnitSet5 同步终端按 E
  -> 0.5 秒镜头平移 + 开机
  -> Space 领取 17F 巡查任务
  -> 电梯按钮 Mesh2643 按 E
  -> 渐黑到 Person Controller (5)
  -> 电梯对白
  -> 渐黑到已保存的 17F 到达点
```

三组 NPC 对话是自由探索内容，不要求全部完成，也不会阻止领取任务。

## 二、场景对象表

| 用途 | 场景对象 |
|---|---|
| 正式可控制玩家 | `Player/Person Controller` |
| 大堂出生参考 | `Player/Person Controller (4)` |
| 电梯内部参考 | `Player/Person Controller (5)` |
| 小女孩对白范围 | `1F/Girl_A_Rigged (2)/space` |
| 年轻男人对白范围 | `1F/casual_Male_G@Sitting (1)/space1` |
| 老奶奶对白范围 | `.../Sitting_Idle (2)/space2` |
| 任务终端 | `1F (1)/TvUnitSet5` |
| 电梯按钮模型 | `DIKUAIunity/Group1/Group144/Rectangle2106772232/Mesh2643` |
| 流程总控 | `MIN_LOOP_ROOT/LobbyOpening/HearthLobbyFlowController` |

`Person Controller (4)/(5)` 只是位置和相机朝向参考，不是第二个可控制玩家。Play Mode 中它们的 Camera、AudioListener、Collider 和移动组件不参与运行。

## 三、如何调整位置和范围

### 调整大堂出生与电梯视角

1. 退出 Play Mode。
2. 移动 `Person Controller (4)` 或 `(5)` 的根物体到目标位置。
3. 调整它们子相机的朝向，作为玩家到达后的视角。
4. 重新 Play 即生效，不需要运行同步菜单。

### 调整 17 楼到达点

1. 退出 Play Mode。
2. 把正式 `Player/Person Controller` 放到希望抵达 17 楼的位置，并调好正式相机朝向。
3. 执行 `Tools > Hearth > Lobby > Capture Current Player Pose As 17F Arrival`。
4. 再执行 `Validate Ground Floor Opening Setup`。

### 调整 NPC 对白范围

1. 选中 `space`、`space1` 或 `space2`。
2. 修改 BoxCollider 的 `Center / Size`，或直接移动/缩放该范围物体。
3. 保持 Collider 的 `Is Trigger` 开启。
4. NPC 和机器人模型不需要跟着范围移动；脚本不会改动它们的位置。

## 四、如何调整终端

- 终端内容 Prefab：`Assets/Prefabs/UI/HearthHud/Terminals/Terminal_Lobby_Assignment.prefab`。
- 世界终端位置、大小：调整 `TvUnitSet5/MonitorCanvas` 的 Transform。
- 固定观察视角：调整终端绑定的 Camera Transform。
- 进入时长：`HearthTerminalCameraTransition / Duration`，当前约 `0.5s`。
- 开机闪烁：`HearthTerminalBootSequence` 的时长、颜色、扫描线和闪烁参数。
- UI 文字与几何：打开 Prefab Mode 修改 Canvas 子对象；修改 Prefab 后场景实例会跟随。
- 领取任务使用 `HearthTvTerminalController / Custom Primary Action`，不要改成前三户的 Replay 操作。

## 五、如何修改对白与接入语音

正式对白源：项目根目录 `HEARTH_Full_Game_Script_Expanded_Native_English_Lobby_Mia_Commentary.md`。

修改步骤：

1. 先编辑最终 Markdown 中对应 Scene 的 Speaker 和对白正文。
2. 在 Unity 执行 `Tools > Hearth > Dialogue > Sync All Dialogue From Final Script`。
3. 执行 `Validate Final Script Coverage`，确认没有漏行。
4. 打开 `Assets/Data/MinLoop/Dialogues/` 中对应的 Dialogue Asset。
5. 把每句录好的语音拖入 `Voice Clip`。
6. 保持 `Duration Mode = Voice Clip When Assigned`。字幕会按真实音频长度播放；没有音频时使用 `Hold Seconds`。

注意：同步只保留说话人与正文完全相同的旧语音。如果改了文字，需要重新检查该句的语音绑定，这是为了避免错配对白。

## 六、常用工具

- 建立/修复流程：`Tools > Hearth > Lobby > Apply Ground Floor Opening Setup`
- 保存当前 17F 落点：`Capture Current Player Pose As 17F Arrival`
- 验证场景引用：`Validate Ground Floor Opening Setup`
- 同步正式对白：`Tools > Hearth > Dialogue > Sync All Dialogue From Final Script`
- 验证对白覆盖：`Validate Final Script Coverage`

Apply 菜单可以安全重跑，但日常调整 NPC、触发范围、Person Controller 参考点或终端 Camera 后通常不需要重跑。只有组件丢失、引用断开或重建流程时才运行 Apply。

## 七、最小验收清单

- 开始游戏位于 1F 大堂，开场结束前不能移动和转头。
- 开场结束后可自由探索。
- 三组对白任意顺序触发；播放时不能移动但可以转头；每组一轮只播一次。
- 不听三组对白也能使用同步终端。
- 终端按 E 后镜头平滑进入，没有人类 HUD 或实体 TV 黑块挡住页面。
- Space 领取任务后电梯按钮才显示 E。
- 电梯对白结束后到达当前保存的 17F 落点，移动和视角恢复。
- 全程只有一个有效 Camera 和一个 AudioListener，Console 没有新增脚本 Error。
