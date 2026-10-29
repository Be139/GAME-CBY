# HEARTH 场景用途与对象规范化说明

2026-10-29。SampleScene 已完成本轮用途命名与制作残留清理并保存。正式流程、对白和视觉替换不在本次变更范围。

## 实际变更

- 316 个对象改为用途名称，27,888 个对象变为 27,472 个；保留 18,433 条序列化对象引用。
- MIN_LOOP_ROOT → Gameplay；17F → Floor17；实际大堂 → Lobby；GameObject 演员集合 → HouseholdActors。人类/陪伴玩家改为 HumanRig / CompanionRig。
- 原 ROOM2 实际绑定 17F03，改为 Apartment_17F03；原 ROOM3 实际绑定 17F02，改为 Apartment_17F02。按门、终端及回放引用确认归属。
- 玩家子节点、演员、终端、触发器、照片、环境根及可识别家具实例按用途命名。完整逐项映射在用途命名变更.csv，不以此声称所有导入网格均已人工识别。
- 删除 8 套无正式流程引用的参考 Rig、17F03 过时摆位副本、空占位及多余子节点，共 416 个 GameObject。
- 大堂仍使用的两套参考 Rig 保留原有根和视角 Transform，改为 LobbyStartAnchor / ElevatorArrivalAnchor 及各自 ViewAnchor；移除多余控制、物理和相机组件。四个 Transform 的引用与世界位置保留。
- 删除 33 个旧 Editor 实现文件，保留工具移除 145 个旧生成/修补方法并进一步合并重复预览入口；独立 StagingPoseProxy 同步退役。具体文件与当前入口见脚本使用说明总表及制作工具规范。

## 验证证据

用途与工具清理验证.json：保留对象世界位置、激活状态、Mesh、材质和 Prefab 来源不变；除四个明确转换的 Anchor 外，父节点、本地变换与组件不变。所有保留脚本的序列化引用端点一致，Missing Script 为 0。重复路径的额外对象数由 188 降至 156，剩余主要按具体对象身份区分，不据此删除模型。

UI 库 21 项目录与排序检查为 0 错误；场景组织、运行拓扑、17F01/17F02 绑定、演员/门 Prefab 检查通过。编译 Console 无错误。启动检查结果另见本页末尾；结构验证不等于连续通关。

## 保留项与后续接入

两份地块有 131 个网格几何重合但一处材质不同，本轮保留。未确认用途的参考模型继续明确标记 Reference/Review；骨骼和导入内部路径保持原状。对象数量本身不能证明性能瓶颈，性能工作需实际测量。

正式制作遵循 HEARTH_制作工具与场景命名规范.md；UI 沿用 UI Library。17F03 直接编辑正式 Anchor，取消预览副本写回与进入 Play 自动改场景。剧情控制器仍按原阶段使用 Anchor。

更新清单先执行 Tools/Dev/inventory-scene-programs.cs，再运行 build-scene-inventory.py 与 build-scene-roles.py。原名对照依赖本次同一编辑器会话的 instanceId；重启后不可用旧 ID 执行改名。

## 本轮启动检查

实际进入 Play：HearthLobbyFlowController.CurrentStage=Opening、ViewSwitchController.CurrentMode=Human；人类相机为 Player/HumanRig/ViewCamera，大堂/电梯四个 Anchor 引用均有效，画面显示正式 Field Unit 开场对白。测试后退出 Play。证据：Logs/HearthUiReview/startup-smoke.json。

编译无错误，但运行 Console 有一项 Reach LocalizedObject.InitializeItem() 第 67 行的 IndexOutOfRangeException（Awake 调用），尚未修复；另有原模型 BoxCollider 负缩放警告。本轮未修改该第三方本地化脚本或模型缩放，不能将启动验证表述为运行零错误。连续住户通关、Reach 视觉/本地化验收未完成。
