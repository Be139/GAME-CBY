# HEARTH UI 库制作与协作规范

更新：2026-10-29。当前授权范围：UI 层级功能、规范化资产管理、制作流程与协作记录。暂停继续换肤、排版和动效替换；不改剧情、正式文案、语音时序或关卡规则。

## 规范依据与适用范围

本文是 HEARTH 项目约定，不能称为“大多数游戏公司的统一规范”。Unity 官方建议记录并统一命名、目录与代码风格，分离自有与第三方资产、优先复用 Prefab，同时明确没有唯一的目录或 GameObject 命名标准。参考：[Unity 项目组织建议](https://unity.com/how-to/organizing-your-project)。

本项目的 21 项目录、PageId、Canvas 排序数值、共存矩阵和 `=======` 分割线均为本地设计。排序值只能结合 Render Mode、相机、Sorting Layer 和 overrideSorting 判断，不能当成跨项目通用层级。分割线对象只标识同级列表中的视觉区段，不建立父子归属；实际归属以 Transform 与组件引用为准。Inactive 或位于 Deprecated 标记之后，都不构成可以删除的依据。

后续整理采用“现状清单 → 职责分类 → 引用核对 → 最小变更 → 对应流程验收”。场景展示分组、代码模块、运行时显隐各有职责，不要求为了名称一致强行移动对象或代码；大规模拆场景、框架迁移按实际协作和性能需求另行评估。

## 从哪里开始

2026-10-29 工具清理：旧 UI 生成、重建、自动修补及重复预览菜单已移除，当前制作操作遵循 `HEARTH_制作工具与场景命名规范.md`。只读 UI 检查集中到 `Tools > Hearth > Validation > UI`，正式资产定位统一使用 UI Library。

1. Unity 菜单 `Tools > Hearth > UI Library > Open Library`：搜索用途、ID、调用方；打开正式 Prefab，或定位已加载 SampleScene 的对象。窗口列出资产中的实际 PageId；资产存在不代表正式剧情可达。
2. 机器可读唯一目录：`Assets/UI/HEARTH/V2/HearthUiLibrary.json`。每项记录稳定 ID、正式资产、场景路径、调用方、出现条件、共存、互斥、层级及状态。AI 先读此文件再改对应对象，不全项目猜名称。
3. 制作场景：`Assets/Scenes/UI/HearthUiWorkbench.unity`；运行验收场景：`Assets/Scenes/SampleScene.unity`。Workbench 用于找到并编辑正式 Prefab，不作为游戏流程入口。
4. 本文记录制作约定；`脚本使用说明总表.md` 记录挂载/Inspector/接口；`HEARTH_剧情接口与可接入点.md` 记录流程接入；`HEARTH_剧情变更记录.md` 记录最新制作意图。

目录是索引，不是第二套显隐系统。资产仍在原路径，以保留 GUID、场景实例和脚本引用；不为整理目录搬动物理场景中的电视或照片。

## 权威来源与职责

|内容|唯一负责方|不能用来替代它的东西|
|正式画面结构|10 个正式 V2 Prefab；场景独有 UI 见目录|旧生成器、预览截图、Reach Demo|
|剧情何时进入/退出|各 Lobby / Replay / Resident / Finale 流程控制器|按钮视觉组件、编辑器窗口|
|跨 UI 显隐/共存|HearthUiStateCoordinator|各组件各自扫描所有 Canvas 并改 alpha|
|绘制顺序|HearthUiCanvasOrder + HearthUiCanvasBand|临时把 sortingOrder 改成更大数|
|页内切换|HearthFirstPersonHudController / HearthTvTerminalController / Inspection phase|同时 SetActive 多个互斥页|
|字体字号|Hearth_UiV2Theme.asset + HearthUiTypography 的明确角色|单改 TMP 数值而忽略运行时写入|
|正式对白|HEARTH_Full_Game_Script_No_Audio_Tags_Native_English.md → HearthDialogueSequence|旧下载稿、配音情绪稿、旧资产内容|
|输入、锁和提交|原游戏控制器与 HearthPlayerControlLock|Reach 控件再注册一次相同提交事件|

Reach 是视觉组件供应方。原流程接口和内容绑定保留；不把第三方整套 MenuManager / ControllerManager 接管游戏。现已导入完整组件资源和 Input System 1.7.0，输入配置为 Both。此前文档中的“只导入字体/未切换 Input System”是历史状态。

## UI 分类与用途

|库分组|内容|制作位置|
|Human|常驻身份、任务、位置；菜单、今日巡检、处置历史、设置；家庭与终局选项；关机确认/结局页|HearthHudRoot_V2.prefab；窗口展开实际 PageId|
|Companion|陪伴身份、阶段任务、状态、长按 E 与进度|Companion/HearthCompanionHudRoot_V2.prefab；内容来自 HearthCompanionHudSceneData|
|Dialogue|正式左右对白、辅助通信、结尾字幕实例|HearthSubtitleVisualCanvas_V2.prefab；说话人与正文绑定保留|
|Terminal|大厅任务、17F01/02 家庭、17F03 告警、17F04 家门与留言|Terminals 下五份正式 Prefab；世界空间相机与实体位置留在场景|
|Inspection|检查数据、Field Unit 解释、处置选项三阶段|Inspection/Hearth17F03InspectionPanel_V2.prefab|
|Photo|真实照片、标题、页码、返回提示|PhotoArchive Prefab + 场景照片 Renderer / 相机；旧 Human 照片页不是正式真实入口|
|Scene overlays|大厅留言、低信任度警告模板、结尾字幕、各转场黑幕|SampleScene 中目录指定对象|
|Interaction / Tutorial|短按 E 提示、初始教程|Human 根内 Presenter / Tutorial；长按属于 Companion，不另造输入系统|
|Legacy|HumanCanvas、旧页、fallback 构建器|保留明确兼容用途；不作为新功能模板|

## 层级与共存契约

逻辑优先级为 `Takeover > Modal > Terminal > 普通 HUD/Interaction`。Dialogue 是剧情请求的独立通道；不是看到 Modal 就无条件关闭对白。

|当前状态|允许同时出现|必须隐藏/不接收输入|退出时|
|人类探索|人类常驻 + 可用短按提示；对白出现后可保留常驻|对白期间普通交互提示；陪伴 HUD|由实际视角与锁状态重新计算|
|陪伴视角|陪伴身份/任务 + 当前对白；剧情开放后可长按|人类常驻；终端/模态/接管期间陪伴 HUD|恢复当前陪伴阶段；不提前开放长按|
|终端资料|当前终端 + 需要的 Field Unit 眼镜对白|人类常驻和普通交互|关闭终端后按协调器恢复，不硬写全 UI alpha=1|
|家庭/终局处置|模态标题、选项、提示；当前剧情明确需要的对白|终端资料与导航、普通交互、常驻与陪伴 HUD|提交仅一次；移除旧选择提示，进入下一剧情状态|
|17F03 检查|检查数据 + 召回提示|解释/处置页不同时显示|检查入口退出释放自身模态请求|
|17F03 解释|Field Unit 对白|检查数据和选择输入|对白结束才开放处置选项|
|照片档案|左侧真实照片 + 右侧眼镜对白 + 页码/返回提示|普通交互、人类菜单；不重复显示照片内旧对白 Surface|返回原相机并释放自身锁/模态请求|
|关机/低信任度接管|当前确认页或警告波；剧情明确需要的对白|普通 HUD、终端和其他模态|挑战结束/取消恢复流程，不由 UI 猜测结果|
|转场/结尾|按黑幕与字幕排序决定可见内容|黑幕下的内容不可当可交互界面|流程释放对应遮罩，不批量关闭其他遮罩|

“允许共存”不代表“必须显示”。显隐请求来自真实流程。CanvasGroup alpha=0 的对象还可能 active，审查必须同时看父 CanvasGroup、Canvas enabled、排序、相机、射线和输入锁。

### 绘制顺序

世界空间：WorldSurface 0、TerminalPage 10、TerminalNavigation 20、TerminalChoice 30、PhotoArchive 80。只在对应相机/Canvas 空间内比较，不能与 Overlay 数字直接推断遮挡。

屏幕层：HumanHud 100、CompanionHud 6200、Replay02Blackout 7000、LobbyNarrative 7200、Inspection 7700、Replay03Blackout 8000、Tutorial 8200、Dialogue 8500、Shutdown 8800、FinaleBlackout 9000、Epilogue 9100、LobbyBlackout 20000、ViewTransition 32767。

- 同一终端多个页面使用相同 Band 是正常的，依赖页面互斥；禁止将“相同排序号”一律当错误。
- 嵌套 Canvas 的 overrideSorting 决定是否脱离父级。新增前确认父 Canvas、RenderMode 和相机；尤其核对结尾字幕与 FinaleBlackout 的组合。
- 选择遮罩、内容和按钮应在同一个模态容器内，按遮罩→底板→内容→焦点顺序组织；不要把按钮拆到低层父 Canvas。
- 展示、输入、剧情三者分开：视觉子物体不重复监听 Space/E，不因为隐藏底板而让输入穿透。

## 全部制作流程

1. **需求定位**：在目录检索现有用途和调用方，读取最新剧情记录。确认只调层级、改内容、改交互还是改视觉；本轮只允许层级与管理。
2. **写明契约**：新增/修改前确定出现条件、结束条件、共存与互斥、输入所有者、Canvas Band、真实相机。条件未知时记录待定，不默认所有 HUD 都共存。
3. **选择正式资产**：打开目录中的 Prefab；场景独有对象通过完整路径定位。优先扩展已有页，不能用旧生成器覆盖现有资产。
4. **组织结构**：根保留 Controller/Bindings；视觉放现有 VisualRoot；常驻、对白、提示、终端、模态、接管使用原协调器分组。保留引用字段与 GUID。
5. **绑定**：序列化绑定指向明确对象；外部模态使用 `SetExternalModalRequest(owner, true/false)` 成对申请/释放，自身关闭/禁用也要释放。不要重设其他所有者的请求。
6. **接入调用方**：流程通过已有公开方法打开/关闭，页内只管理自己的状态。正式对白用 Sequence，语音存在时跟随 AudioClip.length，无语音才回退时长。
7. **同步目录与文档**：新 UI 必须登记稳定 ID、路径、状态和共存规则；脚本说明补齐挂载、组件、Inspector、公开接口、引用、操作步骤、测试和扩展点。内部改动也写“使用方式无变化”。
8. **验证**：先编译、目录/层级检查，再在 SampleScene 运行相关组合；打开、切换、提交、取消、关闭后各检查一次，确保遮挡与输入锁释放。静态 Workbench 预览不能替代运行验证。
9. **交付**：记录改了哪些资产/脚本、通过哪些检查、未覆盖什么。退出 Play 并恢复测试设置；不把状态注入截图称为连续通关。

## 最小验收清单

- `Tools > Hearth > UI Library > Validate Catalog And Loaded Layers`：目录 ID 唯一、资产存在、已加载 Canvas 排序与 Band 一致。该检查不自动改场景。
- `Tools > Hearth > Validation > UI > Validate Production UI`：原绑定/fallback 校验。警告需按实际用途判断，不凭名称删流程组件。
- 人类↔陪伴：仅当前视角的常驻 HUD 显示；模态打开后两种常驻均正确抑制，关闭后恢复。
- 终端＋对白、终端＋选择：对白位置正确；选择时资料与导航不透出；提交一次且退出后提示不残留。
- 检查→解释→处置：三阶段互斥，解释期间对白可见，未开放选择时不能提交。
- 真实照片＋对白：实体照片不被换成 UI 占位；相机返回、页码和关闭提示正确。
- 关机警告/结尾＋黑幕：顺序符合剧情，黑幕下的按钮不能响应；结尾字幕是否高于黑幕需真实组合验证。
- 任何新分辨率支持都单独验证，不能从 1920×1080 截图推定。

## AI 协作注意事项与当前未完成项

- 每轮先读 AGENTS、本规范、目录、脚本总表；只读取所需资产与调用方。目录是定位入口，运行实现是行为证据，两者冲突先核实并同步。
- 不扩展第二套 UI 管理器；不移动世界空间物体来整理库；不改掉已有用户工作；不通过提高 sortingOrder 掩盖显隐错误。
- 不恢复宣传片/序章，不以 Reach 示例文本覆盖正式游戏内容。配音参考稿不直接同步游戏字幕。
- 前一轮 Reach 视觉替换已保存但未完成视觉验收。已知选中填充偏亮、召回/关机文字对比度不足；当前按用户要求暂停视觉修正，不能将其标记为完成。
- 本轮仅增加可检索库入口和层级约定，并修正陪伴 HUD 在 Modal 下的残留条件。全流程键鼠通关、全部结局/分辨率不是已通过项目。
- `Logs/HearthUiReview/` 是本机临时诊断证据，可能被 Git 忽略；长期规则与目录不得只放在此处。

## 本轮验证记录

- Unity 中打开库窗口成功；目录 21 项，正式资产存在，场景目录项全部定位成功；目录/已加载 Canvas 排序错误 0。
- 编译后 Console 错误查询 0；本轮脚本与文档的 `git diff --check` 通过。
- Play 中使用协调器实际绑定的 ViewSwitchController 与 Companion HUD，注入陪伴状态后调用实际显隐逻辑：显示 → Modal 隐藏 → 解除后恢复，通过；Modal 保留请求中的 Dialogue，同时屏蔽 Interaction，通过。测试最后恢复原视角字段并重新计算状态。
- 该测试验证协调器逻辑与真实绑定，不等同于从检查入口连续键鼠走完剧情，也没有验收此前未完成的 Reach 外观。
