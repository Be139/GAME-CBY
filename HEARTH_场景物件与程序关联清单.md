# HEARTH 场景物件与程序关联清单

2026-10-29 当前状态：用途命名与旧制作工具清理已完成，SampleScene 已保存。清单取自保存前的最终 Edit Mode 快照（所以 summary.json 的 dirty=true 表示采样时状态），数量与保留引用已验证。

## 当前快照与验证

- 46 个根对象、27,472 个 GameObject、18,433 条非空序列化对象引用；Missing Script 为 0。
- 480 个 C# 源文件，Assets/Scripts 与 Assets/Editor 共 176 个，当前场景挂载 104 种项目脚本。
- 316 个对象按实际用途改名，416 个已核实制作残留移除；保留引用端点和世界位置一致。大堂两套参考 Rig 已变为保留身份的四个 Anchor Transform。
- 最新名称、用途和保留项见 [场景用途与对象规范化说明](HEARTH_场景用途与对象规范化说明.md)；制作入口见 [制作工具与场景命名规范](HEARTH_制作工具与场景命名规范.md)。旧生成/重建工具说明不再有效。
- 旧迁移验证.json 是仅整理根层级时的历史记录；本次依据为 Docs/SceneInventory/用途与工具清理验证.json、用途命名变更.csv、已移除制作残留.csv。
- UI 库 21 项检查、场景组织、运行拓扑、17F01/17F02、演员与门 Prefab 检查通过。引用清单不替代连续通关。

## 覆盖范围与入口

Build Settings 仍仅启用 SampleScene；HearthUiWorkbench 是制作入口，第三方 Demo 不属于正式场景。

|需要查询什么|清单|
|---|---|
|根物件如何归类、挂了什么程序|[根物件分类](Docs/SceneInventory/根物件分类.md)、[roots.csv](Docs/SceneInventory/roots.csv)|
|全部子物件、Prefab 来源、组件、激活状态、身份 ID|[objects.csv](Docs/SceneInventory/objects.csv)|
|哪个脚本的哪个 Inspector 字段指向哪个对象或资产|[references.csv](Docs/SceneInventory/references.csv)|
|所有源文件、模块、类型、公开方法名、当前场景挂载位置|[scripts.csv](Docs/SceneInventory/scripts.csv)|
|修改名称或父子结构前，要检查哪些代码查找|[dynamic-lookups.csv](Docs/SceneInventory/dynamic-lookups.csv)|
|机器可读数量与范围|[summary.json](Docs/SceneInventory/summary.json)|

CSV 为 UTF-8 无 BOM，可按路径、脚本、字段或分组检索。`visualSection` 是最近一个同级分割线标记，`category` 是职责分类。86 个路径字符串存在重复（额外同名对象共 156 个）；未保存新物件的 GlobalObjectId 也可能为空值形式，因此当前快照使用 instanceId 区分，不能跨编辑器重启复用。引用中的 sourceId/targetId 是具体组件或资产的 ID，不一定等于 GameObject ID；sourceInstanceId/targetInstanceId 记录同一会话内的确切端点。

引用清单覆盖序列化的非空 ObjectReference（含 UnityEvent 的目标对象），不等于完整运行时调用图：不覆盖运行时新建对象、自动查找结果、C# 事件订阅与字符串调用。公开方法和动态查找列表是源码检索索引，修改前需读方法正文；未挂载不等于未使用，数据类、静态服务、编辑器工具和 Prefab 内脚本可能不出现在当前场景。

## 分类使用规则

分割线仅为同级视觉标记，不是容器或删除依据。当前各区对象见 roots.csv 与根物件分类.md，精确程序关联以 references.csv 为准。Gameplay、Floor17、Lobby、HouseholdActors 等是现行用途名；旧名只在迁移对照中保留。环境和参考模型仍需结合组件、空间位置与实际画面判断。

## 程序职责与关联路径

|模块|已有程序及职责|调整时需要连带检查|
|---|---|---|
|交互入口|PlayerInteraction 对相机射线命中的 IInteractable 发起交互；SmartDoorController/TriggerZone 管门，ResidentTerminalFlow 调用 BeginTerminalInspection|Collider、LayerMask、相机、父级交互查找、提示 Presenter 与输入锁|
|全局流程|MinLoopFlowController 持有阶段、住户、处置、信任度、字幕、终端及 F01–F03 Replay 引用|StageChanged/DispositionApplied 事件、终端与 HUD 绑定；保留已有各住户流程职责|
|玩家与相机|ViewSwitchController 管 Human/Companion Rig；Rig 包含 root、Camera、移动、视角、交互、Rigidbody、Canvas|切换相机、旧 Rig 停用、父子变换、音频监听与控制锁|
|各住户剧情|HearthLobbyFlowController、F01/F02/F03 Replay、F04 Finale 等维护具体顺序与演出|演员、锚点、门、终端、字幕序列、关机挑战及相关触发物件|
|共用服务|HearthHouseholdFlowContext 汇集 HUD/字幕/锁/视角/Rig/转场/SFX 引用；PlayerRigService、ScreenTransitionService、StoryCueService 提供对应操作|沿用现有服务；Context 不拥有剧情顺序或协程，不再新建重复总管理器|
|UI|FirstPerson/Companion Controller 与 Binder 更新内容；UiStateCoordinator 决定共存显隐；UiCanvasOrder 决定排序；Bindings 对接 Prefab|先查 UI Library 的资产、PageId、owner、出现条件；保持输入提交与视觉反馈分工|
|世界终端与照片|HearthTvTerminalController、CameraTransition、BootSequence、PhotoArchiveWorldView 与实体屏幕/相机/照片协作|保留世界空间位置与本地相机；不能为了把 UI 放在同一目录而把实体终端重新挂到屏幕 HUD|
|对白与音频|MinLoopSubtitlePlayer、DialogueSurface/Sequence、SfxCuePlayer、AudioChannelSource 等|正式稿唯一来源、AudioClip 时长、字幕 Surface、音频通道及触发方|
|环境|CityLotAssetPlacer、FacadeBillboardPlacer、PlacementSlot、BillboardContent、DuskLighting、WindowNight|生成工具、配置标记和运行控制器分别看待，不能看到 City 前缀就统一删除|
|编辑器及外部资产|Assets/Editor 工具；Reach、人物/建筑资产附带脚本|Editor 工具不挂场景；第三方源码不按自有命名规范批量重命名|

具体挂载实例与字段指向以 CSV 为准，挂载方法、必需组件和使用步骤沿用 `脚本使用说明总表.md`。本轮同步了正式脚本中的用途名称依赖，挂载、字段引用和公开接口使用方式无变化；已退役工具以脚本总表最新章节为准。

## 后续规范化的执行顺序

1. 使用根清单确定实际职责，先处理已确认错分的两个 HUD 和两个环境控制器。若只改善 Hierarchy 展示，优先调整同级顺序；不要把分割线改成父对象。
2. 对待改对象按 ID 查看入向/出向字段引用，再查 dynamic-lookups 中的按名查找、父级查找、子级扫描和 Resources 路径。序列化引用通常能跟随对象移动，字符串路径及扫描结果可能变化。
3. 对 MIN_LOOP_ROOT、17F、1F (1) 按子系统逐项调整：世界 UI 留在实体位置，演员与锚点保留演出关系，流程控制器继续引用它们。避免一次性搬全部层级。
4. 需要重命名或迁移脚本资产时，通过 Unity 资产操作保留 .meta/GUID，检查类型名、场景/Prefab、UnityEvent 方法以及 Editor 调用；不要靠目录重排替代职责重构。
5. 每个实际变更验证其影响路径：交互→终端→回放→处置→HUD 恢复；涉及某一住户则补该住户的门、镜头、演员、字幕。更新脚本总表、UI 目录和对应流程记录。

当前已完成根物件物理重排、分类与关联清单，以及整理工具修正。深层级职责重构和全流程回归未实施；没有为了目录一致搬迁已有 UI 程序、实体终端或剧情锚点。

## 更新清单

在项目根目录、目标场景处于 Edit Mode 时：

```sh
./Tools/Dev/unity-mcp.sh status
./Tools/Dev/unity-mcp.sh -f json code execute -f Tools/Dev/inventory-scene-programs.cs
/d/Tools/Python/python.exe Tools/Dev/build-scene-inventory.py
```

第一步确认 GAME-CBY 实例，第二步检查内部 success 为 true 后再执行第三步。只读快照写入 Logs/HearthUiReview，索引写入 Docs/SceneInventory；不会保存场景、进入 Play 或改组件。清单随场景变更重新生成，不能把旧快照当成实时状态。
