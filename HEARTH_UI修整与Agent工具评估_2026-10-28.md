# HEARTH UI 修整与 Agent 工具评估

日期：2026-10-28。基于当前 Unity 2022.3.61f1c1、URP、uGUI/TMP 与 MCP for Unity 10.3.0。

## 本轮视觉与交互方向

视觉方向：安静的企业终端，深蓝灰底、冷色重点、清晰正文，让玩家优先看人物和当前操作。
信息顺序：住户/当前目标常驻；诊断和决策短时出现；对白和输入提示按当前状态显示。
动效：陪伴 HUD 进入时 0.22 秒淡入；视角交接立即隐藏；保留原信息卡淡入淡出与剧情关机效果。

## 已落地

- 陪伴 HUD：有 TimedCues 或 TriggerCard 的阶段由限时卡承担状态呈现。卡片出现前和结束后不会再弹出旧状态面板；无卡片阶段保留状态面板。数据资产与正式对白未删改。
- HUD 显示：协调器逐帧重复设置可见状态不会重置淡入进度；退出立即隐藏，避免两种视角 HUD 混显。
- 视觉：去掉陪伴视角全屏装饰边框、诊断面板的重复边框和底部技术模式说明；用统一的九宫格面板，保留身份、目标与操作提示。
- 终端：正式绑定分支现在也读取共享主题字号；说话人 24、正文 26、继续提示 18。旧回退面板把更多高度分配给正文并开启换行。
- 提示：终端对白期间隐藏导航、焦点和 RuntimePrompt；提交后隐藏，正常退出对白且未提交时恢复。
- 字幕：正式与辅助对白共用面板素材，装饰边框透明度减至 0.32；说话人颜色/左右位置和正文时序保留。
- 终局：系统推荐标签移到 B（让陪伴单元代答），选项文字绘制在高亮层之上，缩紧面板范围，移除重复的右上角目标文字。

## 成套 UI 选择

| 套件 | 可复用部分 | 当前项目接入结论 |
| --- | --- | --- |
| [Kenney UI Pack Sci-Fi](https://kenney.nl/assets/ui-pack-sci-fi) | 成套面板、按钮、进度条等 PNG/SVG，CC0 | 已选取灰色面板和进度条两件，导入 `Assets/UI/HEARTH/V2/ThirdParty/KenneySciFi`，通过 uGUI 着色和九宫格适配。它是视觉素材，不自带完整动效/游戏逻辑。 |
| [Michsky Reach](https://michsky.com/portfolio/reach-complete-sci-fi-ui/) | 完整科幻 UI 组件与统一 UI Manager | 官方列出 Unity 2022 与 URP 支持，输入要求为 Input System。付费资产，本轮未购买/导入。适合整体菜单替换，但必须把其输入与现有 Space/E/Hold E 流程对接。 |
| [Michsky Heat](https://michsky.com/portfolio/heat-complete-modern-ui/) | 较简洁的现代菜单方向 | 作为风格候选，未导入或运行验证。下一轮若希望整体换肤，可先比较 Reach 的科幻语言与 Heat 的简洁语言。 |

Kenney 文件保留原始 License.txt。下载来源为作者在 OpenGameArt 发布的资源包：<https://opengameart.org/sites/default/files/kenney_ui-pack-scifi.zip>。不需要将整个素材包、示例工程或额外输入框架纳入项目。

## Codex + Unity 的新方法与工具

| 环节 | 推荐工作方式 | 当前状态/条件 |
| --- | --- | --- |
| 编辑与 UI | Codex 检查脚本与显式绑定 → MCP 修改权威 Prefab → 编译 → Play 中调用现有接口 → Game View 截图 | 本轮已经实际使用；继续以此为当前主线。 |
| 测试与性能 | MCP 测试、Console、Profiler，按具体流程定位错误或性能热点 | 本轮验证编译与定点 UI 状态；Profiler 与完整游戏回归尚未执行，不能据此声称整局已验收。 |
| 3D 资产 | Blender Bridge 建模/导入，保留 Collider、动画与剧情锚点 | 10.3 已提供接口；上一轮检查时没有可连接 Blender 插件。生成服务还需要账户/额度，不等于已可自动产出合格游戏资产。 |
| 官方新路线 | Unity Agent Plugin + Unity CLI / Pipeline | 官方插件与 Pipeline 均要求 Unity 6.0+；当前 2022.3 不直接采用。是否升引擎应单独评估包、URP、脚本与场景兼容。 |
| 多 Agent | 研究、代码审查、资产规格可拆分；Unity 场景、Prefab、Play Mode 由一个执行者统一操作 | 这是针对本项目的协作建议。本轮未启动多 Agent；不能让多个 Agent 同时修改同一编辑器状态。独立代码任务可按文件范围分工，再集中编译和回归。 |

官方与一手来源：

- [Unity 插件说明](https://docs.unity.com/en-us/ai/unity-plugin/about-unity-plugin)：支持 Codex 等 Agent，提供 Unity 技能；明确既有项目应说明使用 uGUI。
- [Unity Pipeline 前提与接入](https://docs.unity.com/en-us/unity-cli/unity-pipeline/unity-pipeline-package)：Unity 6.0+，本地 HTTP API，构建和测试自动化。
- [CoplayDev unity-mcp](https://github.com/CoplayDev/unity-mcp)：当前项目继续锁定 v10.3.0，不追随 main。
- [OpenAI Subagents](https://learn.chatgpt.com/docs/agent-configuration/subagents)：子 Agent 工作方式参考；场景单一写入者是本项目实施建议。

## 验证与边界

- Unity 编译后 Console 错误查询为 0 条。随后导航隔离测试直接对尚未打开的终端子面板调用 Show，触发了现有的“说话人不在激活层级”诊断一条；这条测试前置条件错误保留在 Console，未清空日志，不计为编译错误，也不作为正式流程已通过的证据。
- Play Mode 定点检查：限时卡片结束后状态面板为隐藏；连续 20 次设置可见时 Alpha 保持 1；设置隐藏立即为 0。
- 五台正式终端 ResolveDialogueSurface 后均返回说话人 24 / 正文 26。
- 使用项目内 410 条已加载对白资产做文本测量：此前测得正文 26、宽 1376 的最大首选高度为 101.41，小于正式终端正文区域高度 110。这是排版测量，不等同于各镜头下的可读性全覆盖。
- RuntimePrompt 实测：对白期间隐藏、对白结束恢复、choiceSubmitted 后隐藏，三项均通过。
- 截图位于 `Temp/HearthUiReview`：`companion-verified.png`、`final-isolated.png`、`final-selection-delivery.png`。这些是当前大厅背景上的定点 UI 预览，不是对应关卡的完整实玩录像。最初 before-companion 截图实际仍为人类 HUD，不能拿它冒充同状态前后对照。
- 测试用临时状态未保存到场景；结束后退出 Play Mode。保留原有场景、剧情资产、语音和用户已有字体修改。
- 相册镜头裁切与 AudioListener 尚未在本轮复现/修复；所有住户完整流程、不同分辨率与实际长按输入仍需后续实玩回归。本轮修复不代表所有闪烁来源已被排除。
