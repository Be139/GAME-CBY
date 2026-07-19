# HEARTH 场景与 Assets 整理清单

更新时间：2026-07-19

## 1. 本次整理原则

- 本文中的资源目录统一指 Unity 项目的 `Assets` 文件夹。
- 所有资源移动均通过 `AssetDatabase.MoveAsset` 完成，原 `.meta` 与 GUID 保留，Prefab、Scene、Animator 和脚本引用不会因文件夹变化而断开。
- 场景只调整根对象的显示顺序并添加 `EditorOnly` 分隔线，不修改 Transform、不修改 Active 状态、不改房间父子关系。
- `17F/ROOM1-4`、`1F (1)`、`MIN_LOOP_ROOT` 等已有运行路径保持不变，避免破坏脚本中的层级查找。
- 家具、建筑、装饰、灯光和用户手调的人物/相机位置没有删除。

## 2. SampleScene 根层级

| 分区 | 当前对象 | 用途 |
| --- | --- | --- |
| `01 UI` | `HumanCanvas`、`HearthCompanionHudRoot`、`HearthHudRoot` | 人类 HUD、机器人 HUD、通用 UI |
| `02 PLAYER & CAMERAS` | `Player`、`Main Camera` | 正式人类/机器人控制器与全局相机 |
| `03 GAMEPLAY SYSTEMS` | `HEARTH_LOCATION_SYSTEM`、`EventSystem`、`MIN_LOOP_ROOT` | 地点判定、输入事件、所有关卡状态机和音频接口 |
| `04 WORLD SCENES` | `17F`、`1F (1)` | 正式 17 楼与一楼场景内容 |
| `05 ENVIRONMENT` | 建筑、地块、城市生成器、广告牌、室外环境、灯光和 Volume | 共享环境与城市外景 |
| `06 LOOSE ACTORS & SOURCE REFERENCES` | 根层级演员、动作参考模型、角色源对象 | 当前仍被流程引用或需要继续核对归属的演员/参考对象 |
| `07 DEPRECATED / REVIEW` | `little_boy_B`、`GameObject`、`Plane`、旧 `1F` | 可能已被替代，但暂不删除，等待逐项 Play Mode 回归确认 |

分隔线均使用 `EditorOnly` 标签，只用于 Hierarchy 阅读，不进入正式构建。

## 3. 暂未删除对象

| 对象 | 当前判断 | 处理 |
| --- | --- | --- |
| `little_boy_B` | 当前 inactive，第一户正式睡姿对象已改用 `Laying_Sleeping`，但历史绑定较多 | 放入 Review，暂不删 |
| `GameObject` | 名称不明确，含 5 个子物体，可能仍承载旧视角或引用 | 放入 Review，先查引用再决定 |
| `Plane` | 可能是测试地面，也可能参与碰撞/防坠 | 放入 Review，不删 |
| 旧根对象 `1F` | 与正式 `1F (1)` 并存，仍有 31 个子物体 | 放入 Review，必须实机确认后再删 |

`Assets/GameObject.prefab` 没有直接引用证据，但没有删除，已归档为 `Assets/Prefabs/_Legacy/GameObject_Legacy.prefab`。

## 4. Assets 资源去向

| 原位置/类型 | 新位置 | 内容 |
| --- | --- | --- |
| `Assets/*.fbx` 地块与楼层源模型 | `Assets/Art/Environment/SourceModels/` | `1地块`、`地块`、`6unity`、`DIKUAIunity`、`unity3/4/5` 等 |
| 第一户动作 | `Assets/Animations/Hearth/17F01/Clips/` | 睡姿、父母坐姿动作 |
| 第二户正式动作 | `Assets/Animations/Hearth/17F02/Clips/` | Talking、Walk、SitToStand、开门、餐桌与按钮动作 |
| 第二户 Animator Controller | `Assets/Animations/Hearth/17F02/Controllers/` | 卧室女主、餐桌男女、终端男主 Controller |
| 第三户正式动作 | `Assets/Animations/Hearth/17F03/Clips/` | 父亲坐姿、母亲起身/争执/说话、女儿走路/起身/输入代码等 |
| 第三户 Animator Controller | `Assets/Animations/Hearth/17F03/Controllers/` | Mother、Father、Daughter 与 Girl Idle Controller |
| 未进入正式流程的动作素材 | `Assets/Animations/Hearth/_Reference/` | `Female_Start_Walking`、两个额外 `Sitting_Idle` |
| `Girl_A_Rigged` 模型与 JSON | `Assets/Art/Characters/Girl/` | 女儿角色源文件 |
| 城市广告牌/建筑布置脚本 | `Assets/Scripts/Environment/City/` | `CityBillboard*`、`CityBuilding*`、`CityLot*` |
| 根目录交互脚本 | `Assets/Scripts/Interactions/` | `IInteractable.cs`、`PlayerInteraction.cs` |
| 根目录视角/终端脚本 | `Assets/Scripts/MinLoop/ViewSwitchController.cs`、`Assets/Scripts/UI/TerminalUIController.cs` | 正式运行脚本 |
| 自宅相框照片与旧材质 | `Assets/Art/UI/HearthHud/Finale/` | `FamilyPhoto.png`、`PhotoFrame_Legacy.mat` |
| 无引用旧 Prefab | `Assets/Prefabs/_Legacy/` | `GameObject_Legacy.prefab` |

第三方资源包保持原目录，例如 `Free Wood Door Pack`、`Furniture Mega Pack`、`Mini First Person Controller`、`TextMesh Pro`、`polyperfect`。这些包可能含内部相对引用，不做批量迁移。

## 5. 自动工具

### 整理场景

菜单：`Tools > Hearth > Project > Organize SampleScene Hierarchy (Safe)`

可安全重跑。它只更新根层级顺序和分隔线，不移动对象父级，也不改 Transform。

### 整理 Assets

菜单：`Tools > Hearth > Project > Organize Assets Root (Preserve GUIDs)`

可安全重跑。已在目标目录的资源会跳过；源和目标同时存在时会报错，不会覆盖。

### 验证

菜单：`Tools > Hearth > Project > Validate Project Organization`

检查七个场景分区、旧分隔线和资源新路径。整理脚本位于 `Assets/Editor/HearthProjectOrganizer.cs`。

## 6. 后续新增资源规范

- 角色模型：`Assets/Art/Characters/<角色或家庭>/`
- 场景模型：`Assets/Art/Environment/<场景>/`
- 动作：`Assets/Animations/Hearth/<关卡>/Clips/`
- Animator Controller：`Assets/Animations/Hearth/<关卡>/Controllers/`
- 正式脚本：`Assets/Scripts/<系统>/`
- Editor 工具：`Assets/Editor/`
- UI 图片：`Assets/Art/UI/HearthHud/<系统>/`
- Prefab：`Assets/Prefabs/<类型>/`
- 对白数据：`Assets/Data/MinLoop/Dialogues/<关卡>/`
- 正式音频：建议建立 `Assets/Audio/HEARTH/Ambient`、`SFX`、`Footsteps`、`Dialogue`。

移动已有 Unity 资源时不要用 Windows 文件管理器剪切；优先在 Project 窗口移动，或使用本项目整理菜单，以保留 `.meta` 和 GUID。
