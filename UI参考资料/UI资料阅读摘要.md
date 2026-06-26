# UI 资料阅读摘要

## 已归档文件

- `HEARTH-HUD.pptx`：40 页 HUD 视觉稿。Slides 1-24 是剧情时序页面，Slides 25-40 是拆出来的组件库。
- `已为您沟通_UI说明书_for_Unity_v1.md`：逐页说明每个 UI 的触发条件、交互区域和跳转关系。

## 当前理解

- 所有 UI 都是 Mia 眼镜中的工作 HUD，不是传统游戏菜单。关键词是半透明浮屏、玻璃质感、几何切角、克制的设备界面。
- UI 分三类：常驻 HUD、全屏接管页面、局部浮屏页面。
- 左上工号区是全局入口和状态灯：ACTIVE 绿色、DORMANT 灰色、ALERT 红色或橙红色。
- 中下字幕区是 NPC、Lily、陪伴单元说话的唯一位置，不把对白塞进面板内部。
- 17F-01 和 17F-02 可复用同一套门口终端、ACQUISITION、处置态、陪伴单元第一人称模板；17F-03 是 Alert 红色变体，需要单独状态。
- 现有项目已经有 `MinLoopTerminalPresenter`、`MinLoopRobotHudPresenter`、`MinLoopObjectivePresenter`、`MinLoopTrustPresenter`、`MinLoopSubtitlePlayer` 等占位 UI/正式 UI 绑定入口，正式界面可以优先挂到这些 Presenter 上。

## 正式制作前建议确认

1. 先确认首版范围：只做 17F-01 最小闭环，还是直接做说明书里的完整 1-24 页面状态。
2. 确认 Canvas 方案：PC 第一版建议先用 Screen Space Overlay 或 Screen Space Camera；VR 版以后再转 World Space HUD。
3. 确认目标分辨率：建议先按 1920x1080 还原，所有导出资源按 1x 或 2x 命名。
4. 提供或确认字体：PPT 使用了多种系统字体；Unity 里最好统一成一到两套 TextMeshPro 字体资产。
5. 补齐 PPT 未做的内容文案：17F-02 具体数据、部分 tab 内容、黑屏尾声字幕、设置/历史页如果首版要做也需要文案。
6. Unity 正式挂接前，最好打开当前目标场景，并让 Unity MCP 可连接；如果 MCP 仍不可用，就需要手动告诉我目标 Canvas、主相机、玩家对象、终端对象的名称。

## 透明图导出建议

需要导出，但不要把所有文字页面都烘成整张图。

- 适合导出透明 PNG/SVG：几何边框、玻璃底板、状态灯、切角装饰、机器视角整屏边框、数据流装饰、相框浮卡底板、警告框底板、E HOLD TO ACT 按钮底板。
- 不建议烘成图片的部分：动态文字、任务进度、时间、信任度、tab 文案、A/B 按钮文字、字幕。这些应该在 Unity 里用 TextMeshPro 和 Button 做，后续才能随剧情改内容。
- 如果从 PPT 导出，优先导出 Slides 25-40 的单独组件透明图；Slides 1-24 保留为视觉对照即可。
