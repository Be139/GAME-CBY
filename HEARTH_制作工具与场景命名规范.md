# HEARTH 制作工具与场景命名规范

2026-10-29。当前制作入口如下；旧文档中的生成、重建、修补和自动摆位菜单已退役。

|工作|正式入口|修改方式|
|---|---|---|
|UI|Tools > Hearth > UI Library > Open Library|打开目录中的正式 Prefab；通过 Inspector 编辑结构、绑定和视觉。沿用 StateCoordinator / CanvasOrder|
|场景与演员|SampleScene 的 Gameplay、Floor17、Lobby、HouseholdActors|直接编辑正式对象及控制器引用的 Anchor；保存场景后测试受影响流程|
|17F03 摆位|Gameplay/ReplayRoom_17F03/Anchors|修改每幕正式 Anchor 的 Transform；RuntimeActors 由回放控制器按阶段定位|
|对白|Tools > Hearth > Dialogue|从正式英文稿同步 Sequence，再检查覆盖与语音绑定|
|只读检查|Tools > Hearth > Validation；UI Library 的目录检查|查看报告，再修改具体配置；检查不会重建游戏流程|
|层级顺序|Tools > Hearth > Project|仅按职责整理根对象同级顺序；不迁移资产、不重建内容|
|环境制作|现有 City 控制器的 Inspector|保留基于显式配置的灯光、夜窗、地块及广告牌工具；执行有意的制作操作后检查结果|

名称使用可说明用途的英文名，住户采用 Apartment_17F01 等实际编号，演员采用 Actor_姓名_住户_用途。HumanRig / CompanionRig、ViewCamera、GroundProbe 表达组件用途。Anchor 名说明站位或阶段。重复实例用明确用途或序号区分。

改名同时检查序列化字段、字符串查找、UI 目录和动画相对路径。本次保留 FBX 骨骼与导入模型内部路径；不为统一名称破坏 Avatar 或动画绑定。分割线不等于父容器，Inactive 不等于废弃。

已退役 33 个 Editor 文件及独立摆位代理；保留工具也移除了旧生成/修补实现。旧源码归档位于 Docs/Archive/EditorTools_Retired_2026-10-29.zip，不参与编译或菜单注册。正式功能修改从现有控制器、Prefab 和 Inspector 接入，不恢复一次性生成器，也不新增自动覆盖已制作场景的启动回调。

改名逐项记录见 Docs/SceneInventory/用途命名变更.csv；移除记录见 已移除制作残留.csv。脚本挂载、依赖与接口见根目录脚本使用说明总表，最新章节优先于历史记录。
