# 门类 Unity 资产候选清单

整理日期：2026-06-16

项目版本参考：Unity `2022.3.61f1c1`，URP `14.0.12`。

## 结论

当前项目里已经有一批门、门框、窗、钥匙、滑门和门开关音效，第一阶段建议优先用本地资源，不急着购买新资产。

最小循环和近期大厅/电梯厅需求可以这样分：

| 游戏位置 | 推荐资源 | 是否需要外部购买 | 说明 |
|---|---|---|---|
| A 栋大厅玻璃自动门 | `Assets/家具筛选/通用_门窗门禁/Door_slide.prefab` + 玻璃/金属材质 | 暂时不需要 | 用脚本控制左右滑动即可。 |
| 17F 住户入户门 | `Door_main.prefab`、`Door_WideWithFootstep.prefab`、`Entry_door.prefab` | 不需要 | 更适合住宅入户门、门口终端旁的门。 |
| 普通室内门 | `Door.prefab`、`BackDoor.prefab`、`DoorRoom_narrow/wide` | 不需要 | 用于儿童房、客厅、卧室之间。 |
| 门框/门洞白盒 | `DoorFrameA/B/C.prefab`、`Wall_1-5m_door.prefab` | 不需要 | 适合快速搭白盒走廊和住户门口。 |
| 衣柜门/柜门 | `Closets_door.prefab`、`Closets_door_narrow.prefab` | 不需要 | 儿童房/卧室背景物件可用。 |
| 电梯门 | 先用两片金属板 + `17F公共区_电梯金属面板` 材质自制 | 暂时不需要 | 当前只要按钮亮、叮咚、进入 17F，不必买完整电梯系统。 |
| 钥匙/门禁占位 | `Key.prefab`、现有终端交互脚本 | 不需要 | 如果后面做门禁刷卡，可扩展交互脚本。 |

## 本地已存在资源

### 通用门窗门禁目录

路径：`Assets/家具筛选/通用_门窗门禁`

重点资源：

- `Door_slide.prefab`：滑动门，适合大厅玻璃门、自动门、科技感通道门。
- `Door_main.prefab`：主门/入户门，适合 17F-01、17F-02、17F-03 住户门。
- `Door_WideWithFootstep.prefab`：较宽门，带门槛/脚步区域，适合公共区入口。
- `Door.prefab`：普通室内门。
- `Entry_door.prefab`：住宅入口门。
- `BackDoor.prefab`：后门/普通门，可做室内或服务门。
- `DoorFrameA.prefab`、`DoorFrameB.prefab`、`DoorFrameC.prefab`：门框。
- `Wall_1-5m_door.prefab`：带门洞的墙体模块，适合白盒和快速搭建。
- `Closets_door.prefab`、`Closets_door_narrow.prefab`：柜门。
- `Garage_door.prefab`：卷帘/车库类门，不是当前主线必需。
- `Key.prefab`：钥匙占位物。

配套材质在：

- `Assets/家具筛选/通用_门窗门禁/_配套材质`
- `Assets/家具筛选/通用_门窗门禁/_模型FBX/Materials`

### HQ Residential House 原始门资源

路径：`Assets/HQ_ResidentialHouse/Prefabs`

可用资源：

- `Door_slide.prefab`
- `Door_main.prefab`
- `DoorRoom_narrow.prefab`
- `DoorRoom_wide.prefab`
- `Door_WideWithFootstep.prefab`
- `DoorFrameA/B/C.prefab`
- `DoorMat.prefab`
- `Garage_door.prefab`
- `Closets_door*.prefab`

相关动画：

- `Assets/HQ_ResidentialHouse/Animations/DoorMain_open.anim`
- `Assets/HQ_ResidentialHouse/Animations/DoorRooms_open.anim`
- `Assets/HQ_ResidentialHouse/Animations/DoorRoomsWide_open.anim`
- `Assets/HQ_ResidentialHouse/Animations/Door_slide_anim/DoorSlide01_open.anim`
- `Assets/HQ_ResidentialHouse/Animations/Door_slide_anim/DoorSlide02_open.anim`

相关音效：

- `Assets/HQ_ResidentialHouse/Sounds/door_open.mp3`
- `Assets/HQ_ResidentialHouse/Sounds/door_close.wav`
- `Assets/HQ_ResidentialHouse/Sounds/GarageDoor.wav`

已有旧门脚本：

- `Assets/HQ_ResidentialHouse/Scripts/DoorScript.cs`

注意：这个脚本功能比较完整，但风格偏旧，依赖 `Animation`、Tag、Trigger 和内置 UI Text。建议可以参考，不建议直接作为主项目统一门系统。后续如果要和现有 `PlayerInteraction`、`IInteractable` 接起来，最好重新写一个更轻的门脚本。

## 外部可选资源

以下资源是补充候选，不是当前必须购买项。

| 资源 | 类型 | 价格/状态 | 当前建议 |
|---|---|---|---|
| [Classic Interior Door Pack 1](https://assetstore.unity.com/packages/3d/props/interior/classic-interior-door-pack-1-118744) | 室内门模型包 | 免费 | 如果本地住宅门不够好看，可以下载补充。 |
| [Key Door System](https://assetstore.unity.com/packages/templates/systems/key-door-system-fps-microgame-approved-add-ons-175210) | 钥匙开门系统 | 免费 | 可参考门禁/钥匙逻辑，但项目已有交互接口，谨慎整包导入。 |
| [Door Interaction Animaions](https://assetstore.unity.com/packages/3d/animations/door-interaction-animaions-108412) | 开门/推拉门交互动画 | 付费 | 如果后面要做第一人称手部开门动作，可以考虑。 |
| [Elevator](https://assetstore.unity.com/packages/3d/props/electronics/elevator-160162) | 电梯模型 | 付费 | 只缺电梯外观时可考虑，完整系统暂时没必要。 |
| [Moving Elevator System](https://marketplace.unity.com/packages/3d/props/electronics/moving-elevator-system-fully-functional-99115) | 可运行电梯系统 | 付费 | 当前过重，只有做完整大厅/多楼层时再考虑。 |
| [Sci-Fi One-Sided Sliding Door](https://assetstore.unity.com/packages/3d/props/sci-fi-one-sided-sliding-door-invisible-outside-frame-341716) | 科幻滑门 | 付费，Unity 6/URP | 风格偏科幻，且原始版本是 Unity 6；当前项目是 2022.3，暂不推荐。 |
| [DOTween](https://dotween.demigiant.com/) | 位移动画/缓动工具 | 免费开源 | 如果以后有很多滑门、UI 动效、按钮发光，可用；单个门不必为它引入依赖。 |

## 推荐制作路线

1. 大厅玻璃门：使用 `Door_slide.prefab` 或两片自制玻璃门扇，写轻量脚本控制位置移动。
2. 电梯门：先用两片金属板 + 电梯金属材质，按钮触发开合和叮咚音，不做真实电梯系统。
3. 17F 入户门：使用 `Door_main.prefab` 或 `Entry_door.prefab`，门本身可以先不打开，只作为终端旁的空间标识。
4. 儿童房/室内门：使用 `Door.prefab` 或 `BackDoor.prefab`，后续剧情需要孩子看向门外时，门的位置比开关功能更重要。
5. 如果要做统一脚本，建议分三类：
   - `SlidingDoorController`：左右/上下滑动门。
   - `HingedDoorController`：旋转开合门。
   - `DoorInteractable`：接入 `IInteractable`，负责玩家按 `E` 触发。

## 当前不建议

- 不建议为最小循环购买完整电梯系统。
- 不建议直接把大型门系统包导入主项目，容易和现有交互逻辑、输入、UI 混在一起。
- 不建议为了大厅玻璃门使用复杂 Animator Controller；第一版用脚本控制 Transform 更快。
- 不建议让所有门都可打开。17F-01 最小循环里，很多门只需要作为空间和叙事标识。
