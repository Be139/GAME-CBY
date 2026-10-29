# HEARTH UI 层级与字号管理

更新：2026-10-29。用户选择 Reach 1.3.11，要求全 UI 层级管理、字号统一和视觉检查。

最新修正及实际画面证据见 `HEARTH_UI真实场景整改与验收.md`。本文件下方“收尾验证状态”为较早阶段记录；最新照片构图为左侧实体照片、右侧眼镜对白，标题/页码靠近照片相机，不再贴在照片几何表面。

## 编辑入口与职责

- 正式外观：`Assets/Prefabs/UI/HearthHud/V2` 下的 Human、Companion、5 台 Terminal、Inspection、PhotoArchive；字幕为 `Assets/Prefabs/UI/HearthSubtitle/V2/HearthSubtitleVisualCanvas_V2.prefab`。
- 字体和字号：`Assets/UI/HEARTH/V2/Profiles/Hearth_UiV2Theme.asset`；每个根物体的 `HearthUiTypography` 显式指定文本角色。不要在单个 TMP 上改字号，运行时会应用角色值。
- 显隐及互斥：继续由 `HearthUiStateCoordinator` 与现有页面、剧情控制器负责。新增组件仅管理呈现属性。
- Canvas 绘制顺序：`HearthUiCanvasOrder` 的 Band。世界空间页面 10、导航 20、选择 30，确保选择遮罩覆盖导航；各屏幕层保留原剧情顺序，以命名枚举集中记录。
- 场景独立 UI：LobbyNarrativeCanvas、ShutdownChallenge、Epilogue、转场黑幕在 SampleScene，已绑定字体/层级组件；实际内容仍由原控制器驱动。
- Legacy Unsafe 菜单继续隔离旧生成器。ResidentTerminalFlow、ReplaySequenceController 是流程组件，不再误报为 UI 生成器。MinLoopTerminalPresenter 仅允许生成 fallback 时才作为遗留风险报告。

## 字号表（1920×1080 参考坐标）

|角色|字号|用途|
|---|---:|---|
|Heading|36|页面标题|
|Section|28|分区标题、身份值|
|Body / Button|24|正文、按钮、选项|
|Label|20|状态与字段标签|
|Caption|18|辅助说明、操作提示|
|Subtitle|28|对白正文，保留 Chakra Petch|
|Speaker|34|说话人，大于 28 号对白正文|
|Display|44|剧情大字|

UI 字体采用 Reach 的 Rajdhani Medium；正文对白保留项目独立字体。世界空间终端必须同时检验摄像机距离与 Canvas 缩放，不能仅凭 Inspector 数值判断阅读大小。

## 本轮修整

- 将 10 个正式 Prefab 里的 TMP 纳入显式角色表；状态卡、提示卡、Location、字幕、DialogueSurface 原来的字号写入读取角色值。
- Reach 字体与 Cross Frame 替换对应视觉部件；保留原输入与剧情绑定。没有导入 Reach Demo、整套菜单控制器或切换 Input System。
- 终端选择面板扩至 1260×550；选择 Canvas 排序升至 30，覆盖导航层 20。
- 关机警告启用换行；住户资料去除旧排版软换行并增大正文区域；结尾长标题扩宽；大厅留言扩展文字及底板区域。
- 陪伴 HUD 五条场景位置覆盖归并至权威 Prefab，标题和分隔线重新对齐。
- 照片档案正式 Prefab 的对白面板顶部从 890 上移至 790，页码/返回提示移至 730，分隔线移至 778，避免对白框贴住 1080 底边。正式绑定不走旧 fallback 布局；外观继续以 Prefab 为准。

## 2026-10-29 收尾验证状态

### 后续定点验收结果（覆盖下方此前阻塞状态）

- Unity MCP 已连接 GAME-CBY；重新导出 52 个正式页面及 3 个场景覆盖层。照片档案新图已确认对白框底部保留 100 参考像素，页码、返回提示与对白框分离。
- 正式 Prefab/显式绑定校验通过。Play 中 15 个 Typography 组件的字号偏离角色数为 0。
- 通过实际视角切换进入陪伴模式，显示 17F02_02 测试阶段，复核身份/任务/长按提示；再显示测试字幕，未见重叠。此为指定 UI 状态注入，未执行 17F02 完整剧情。
- 重复 SetVisible(true) 前后 Alpha 均为 1，不重播透明淡入。
- 修复长按进度填充丢失 Sprite：采用内置 UISprite 和 Horizontal Filled。0/50/100% 网格宽度为 0/276.87/553.74；0% 新截图只显示底条。
- 最后 Console 错误查询为 0；存在 BoxCollider 负缩放警告和 1 个 Legacy 组件提示，未将其计为已解决。已退出 Play，恢复 runInBackground=false。
- 证据：`Temp/HearthUiReview/Pages/HearthPhotoArchiveWorldView_V2__HearthPhotoArchiveWorldView_V2.png`、`acceptance-companion-dialogue.png`、`acceptance-hold-zero.png`。
- 验收结论：上述 UI 定点项目通过。教程、全部陪伴阶段、检查选择与真实相册镜头的完整动态组合、全剧情分支通关尚未验收；静态预览不替代这些项目。

### 此前检查记录

- 已复核上一轮实际运行截图：陪伴身份栏、任务栏与字幕没有重叠；大厅留言预览的正文与底部提示分离。
- 52 个正式页面和 3 个场景覆盖层的既有文字尺寸表共 55 份，未发现 preferredHeight 超出矩形高度超过 3 的条目。此检查不代表所有动态内容、遮挡和分辨率均通过。
- 本次照片档案坐标已修改，最新渲染尚未导出；现有照片档案 PNG 仍是修改前版本，不可作为修复后的验收图。
- `git diff --check` 已通过；测试用 runInBackground 设置已在项目文件恢复为 0。用户确认已退出 Play。
- 本轮 MCP 服务反复停止监听/重置连接，原生工具和本地 HTTP 均未能稳定调用；因此最新照片档案画面、最终编辑器状态和动态组合复查仍待完成。上一轮编译及字号运行时检查通过，不冒充本轮新检查。
- 下一步：MCP 稳定后导出 PhotoArchive 页，确认实际相册镜头与字幕/翻页提示，再完成各陪伴阶段、教程及检查选择的运行时组合验证。

## 检查与后续使用

`Tools > Hearth > Production UI`：

1. 打开 `Assets/Scenes/UI/HearthUiWorkbench.unity`，进入对应正式 Prefab Mode 编辑；旧换肤/重建菜单已撤下，不用于正式制作。
2. 修改 Theme 或 Typography 的显式角色；Canvas Band 在对应 Canvas 上编辑。场景独立 UI 在 SampleScene 编辑。
3. `Audit > Export All Page Images`：导出 52 张正式页面预览与文字尺寸 TSV 到 `Temp/HearthUiReview/Pages`。
4. `Audit > Export Scene Overlay Images`：导出大厅留言、关机弹窗和旧 HumanCanvas 供检查。
5. `Validate Production UI`：检查正式绑定、fallback、场景视觉覆盖。

预览图用于静态排版，不能替代运行时互斥、动态内容和世界空间摄像机测试。空照片、空提示或未激活状态不代表实际流程缺内容。已检查文字高度溢出、终端选择遮罩及设置页运行时显示；完整剧情分支通关仍需单独回归。

Reach 来源：用户提供的 `E:/桌面/Reach - Complete Sci-Fi UI v1.3.11.unitypackage`；选用资产及原 GUID 保存在 `Assets/UI/HEARTH/V2/ThirdParty/Reach`，附原 Read Me。未发布第三方资源下载。
