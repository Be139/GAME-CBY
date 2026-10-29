# AGENTS.md

## Codex 内置浏览器限制

- 本项目不要使用 Codex 内置浏览器访问网页；用户已确认该操作会导致 Codex 崩溃。
- 查询外部资料时优先使用官方公开网页搜索、官方 API 或其他不会启动 Codex 内置浏览器的只读方式。
- 除非用户之后明确撤销本限制，不要再次尝试 Codex 内置浏览器。

## UI 库协作入口

- 场景命名、正式 Anchor 摆位及 Editor 工具入口遵循 `HEARTH_制作工具与场景命名规范.md`；已退役的一次性生成/重建/预览写回菜单不得按旧文档恢复。

- UI 任务先读取 `HEARTH_UI库制作与协作规范.md` 与 `Assets/UI/HEARTH/V2/HearthUiLibrary.json`，通过 `Tools > Hearth > UI Library > Open Library` 定位正式资产及场景对象。
- 新增或改变 UI 用途/出现条件/共存关系时，同步目录与制作规范；沿用 `HearthUiStateCoordinator` 和 `HearthUiCanvasOrder`，不另造显隐系统。
- 当前阶段只修正层级与规范化库管理；后续视觉替换须依据新的明确任务授权。此前 Reach 视觉迁移未完成验收，不得当作已验收基线。

## Unity 脚本记录要求

本项目中，凡是由 Codex 新增、修改或重构的 Unity / C# 脚本，都必须同步更新根目录的 `脚本使用说明总表.md`。

这条规则不仅适用于终端 UI，也适用于后续所有代码相关任务，包括交互、玩家控制、UI、关卡触发、道具、系统管理器、剧情逻辑和工具脚本。

## 每次脚本任务完成前必须记录

每次生成或修改脚本后，必须检查并记录：

- 脚本名称和所在路径。
- 脚本具体负责什么功能。
- 脚本应该挂载到哪个 GameObject 上。
- 目标 GameObject 还需要哪些组件，例如 Collider、Canvas、Button、Rigidbody。
- Inspector 中重要字段分别应该拖入什么对象。
- 脚本暴露了哪些公开方法、预留接口或可被其他脚本调用的能力。
- 这个脚本和其他脚本之间的引用关系。
- 在 Unity 中的具体使用步骤。
- 最小测试方法和验收标准。
- 后续如果要替换 UI、替换模型或扩展功能，应从哪里接入。

如果某次脚本修改只改变内部逻辑，没有改变挂载方式或 Inspector 引用，也必须在 `脚本使用说明总表.md` 中标注“使用方式无变化”，避免后续协作时重复确认。

## 文档维护方式

- `AGENTS.md` 只记录长期规则和协作提醒，不写大量脚本细节。
- `脚本使用说明总表.md` 作为实际脚本说明总表，所有脚本使用说明都写在那里。
- `HEARTH_剧情变更记录.md` 记录用户口述的剧情、流程、演出、角色走位、UI 触发条件等变更；这些内容先作为制作记录保存，不直接等同于正式策划案。
- `HEARTH_剧情接口与可接入点.md` 记录剧情、字幕、语音、处置记录、信任度和后续关卡预留接口；后续开始剧情或 UI 流程相关任务前，应优先读取它，再判断怎么接入。
- 每次完成代码任务前，把“是否更新脚本使用说明总表”作为固定收尾检查项。
- 后续继续协作时，Codex 应优先读取 `AGENTS.md` 和 `脚本使用说明总表.md`，再判断已有脚本如何使用或扩展。

## 剧情与流程变更记录规则

当用户口述修改剧情、游戏流程、场景演出、角色走位、音效触发、字幕内容、UI 触发条件或关卡节奏时，Codex 必须把这些变更整理记录到 `HEARTH_剧情变更记录.md`。

记录时应写清楚：

- 日期、关卡或住户编号，例如 `17F02`。
- 修改后的剧情顺序和游戏进程。
- 涉及的角色、模型、锚点、门、终端或 UI 名称。
- 哪些内容只是剧情/演出意图记录，暂时不实现到脚本或场景。
- 如果这些记录和旧说明冲突，后续实现前应优先读最新剧情变更记录，再决定是否同步修改正式脚本、字幕资产或策划案。

`AGENTS.md` 只记录这条长期规则；具体每次剧情内容写入 `HEARTH_剧情变更记录.md`，不要把长剧情直接塞进 `AGENTS.md`。

## Unity MCP 使用规则

场景分类与程序关联入口：先读 `HEARTH_场景物件与程序关联清单.md`，再按需查询 `Docs/SceneInventory/` 的物件、挂载脚本及字段引用。清单是注明日期的编辑器快照，变更前应检查实时状态；`=======` 分割线和 Deprecated 名称不能替代实际归属或删除依据。

本项目已选择 `MCP for Unity` 作为 Unity 2022 项目的第三方 MCP 连接方案。

这里的 `MCP for Unity` 不是 Codex 内置下载的官方工具，而是 Unity 项目内安装的第三方包：

- Unity 包名：`com.coplaydev.unity-mcp`
- 当前项目锁定来源：`https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#v10.2.0`（2026-10-29 从 10.3.0 回退做断连对照，未经用户授权不自动升级）
- Codex 本机 MCP 配置名：`unityMCP`
- 默认 HTTP 地址：`http://127.0.0.1:8080/mcp`

当前项目的 Unity MCP 包入口文件：

- Unity 包声明：`Packages/manifest.json` 中的 `com.coplaydev.unity-mcp`
- Unity 包锁定：`Packages/packages-lock.json` 中的 `com.coplaydev.unity-mcp.hash`
- Codex MCP 客户端有效配置：`C:\CodexHome\config.toml`（当前 `CODEX_HOME`）；历史配置：`C:\Users\彩笔\.codex\config.toml`。两处目前均指向同一个本地 HTTP 地址。

### CLI 优先与启动顺序

- v10.3.0 本机已观察到 MCP 面板的本地服务探测触发 `ending orphaned session`，而 HTTP 服务仍正常。连接后日常关闭 MCP for Unity 面板，使用 CLI/原生工具；关闭面板不会停止 Bridge。此为已验证可保持连接的缓解措施，不等同于插件问题根治。详见 `HEARTH_UnityMCP断连排查.md`。

- 日常连接检查、Hierarchy、组件、Console、Play Mode 和脚本执行优先使用现有 `Tools/Dev/unity-mcp.sh`；原生 Unity MCP 工具可用时也可直接调用。不要为了启动或检查 MCP 默认使用 Computer Use。
- 在项目根目录经 Git for Windows Bash 执行 `./Tools/Dev/unity-mcp.sh status`，再执行 `./Tools/Dev/unity-mcp.sh instance list`。确认目标为 GAME-CBY；多实例用 `-i <实际实例哈希>` 指定。CLI 使用已缓存的 10.2.0 与固定 Python，详细用法见 `Tools/Dev/README.md`。
- HTTP 服务未运行时，先使用下述已记录的 `uvx.exe ... mcp-for-unity --transport http ...` 命令启动服务。后台启动通过 PowerShell 7 的 `Start-Process -WindowStyle Hidden`，遵守全局编码与 fallback 注释规则；不要重复启动已运行的服务。
- 服务进程和 Unity 编辑器会话是两层：CLI 是控制客户端，没有 `start-server` 或离线强制连接编辑器的命令。启动 HTTP 服务后，应等待已开启的 `Auto-Start on Editor Load` 自动连接，再用 `status` 和 `editor console --type error --count 10` 验证；只有 server connected 不能视作编辑器连接成功。
- 域重载期间的 503 / no Unity session 先等待编译结束，再做只读检查；不能把断线视为修改未执行而重放修改命令。若服务正常但自动连接未恢复，且当前没有可用 API/CLI 入口，才使用 Computer Use 点击 Unity 面板的 Start Session，并说明是恢复编辑器会话所必需。不得通过关闭 Domain Reload 或修改 PackageCache 掩盖问题。
- 当前 Codex 工具面板没有 Unity MCP 工具时，明确说明这一限制后优先走 CLI；CLI 也不可用时再读项目文件，并标注未验证编辑器内状态。不使用 Codex 内置浏览器访问本地 MCP 地址。

Unity 编辑器内使用入口（手动配置或必要时恢复会话）：

- 打开面板：Unity 菜单 `Window > MCP for Unity`
- 快捷键：Windows/Linux 可用 `Ctrl + Shift + M`
- 常用按钮：`Configure All Detected Clients` 用于重新写入/刷新 Codex 等客户端配置；`Start/Stop Server` 用于启动或停止本地 HTTP MCP 服务。
- 连接检查：面板里的 HTTP URL 应与 Codex 配置一致，默认是 `http://127.0.0.1:8080/mcp` 或 `http://localhost:8080/mcp`。
- 手动启动命令入口在面板 `Connect > Manual Server Launch`。本项目 v10.2 当前命令格式为：`C:\Users\彩笔\AppData\Local\Microsoft\WinGet\Links\uvx.exe --python D:/Tools/Python/python.exe --from "mcpforunityserver==10.2.0" mcp-for-unity --transport http --http-url http://127.0.0.1:8080 --project-scoped-tools`。注意：手动命令里的 `--http-url` 不带 `/mcp`，Codex 客户端配置里的 URL 带 `/mcp`。
- v10 工具组：v10 将工具分为 `core`、`animation`、`asset_gen`、`docs`、`probuilder`、`profiling`、`scripting_ext`、`testing`、`ui`、`vfx`。非 `core` 工具可能需要在 Unity 的 `Tools` 标签页启用，或通过 MCP 工具 `manage_tools action=activate group=组名` 启用。
- 多 Unity 实例：如果之后同时打开多个 Unity 项目，应优先读取 MCP 资源 `unity_instances`，再用 `set_active_instance` 选择目标实例。

v10 连接验证方法：

- 普通浏览器或 `GET http://127.0.0.1:8080/mcp` 返回 `406 Not Acceptable` 不代表失败；这是因为 `/mcp` 需要 MCP/JSON-RPC 请求头。
- 最小连通性验证：向 `http://127.0.0.1:8080/mcp` 发送 JSON-RPC `initialize`，Header 使用 `Content-Type: application/json` 和 `Accept: application/json, text/event-stream`。成功时会返回 `200 OK`、`mcp-session-id`，并显示 serverInfo，例如 `mcp-for-unity-server`。
- 初始化后应发送 `notifications/initialized`，后续请求都带上 `mcp-session-id`。
- 读取工具列表：调用 JSON-RPC 方法 `tools/list`。v10 正常应能看到约 47 个工具，包括 `manage_scene`、`manage_gameobject`、`manage_components`、`read_console`、`refresh_unity`、`manage_tools`、`set_active_instance` 等。
- 读取 Unity Console：调用 `tools/call`，参数 `name="read_console"`，`arguments={"action":"get","count":"10"}`。如果能读到 `Server ready on http://127.0.0.1:8080` 或 `Session connected`，说明 Unity 侧会话已经通。
- 当前项目实例验证：读取资源 `mcpforunity://instances`，本项目通常显示类似 `GAME-CBY@...`；如果有多个 Unity 实例，先调用 `set_active_instance` 绑定目标实例。

更新 Unity MCP 包时的固定做法：

1. 先用 `git ls-remote --tags https://github.com/CoplayDev/unity-mcp.git` 确认目标版本标签和实际提交。
2. 把 `Packages/manifest.json` 的 `com.coplaydev.unity-mcp` 改成明确版本标签，例如 `#v10.0.0`，避免一直跟随 `main` 造成版本漂移。
3. 同步更新 `Packages/packages-lock.json` 里的 `version` 和 `hash`；如果不确定依赖变化，优先让 Unity Package Manager 重新解析。
4. 回到 Unity 后等待 Package Manager 编译完成，再在 `Window > MCP for Unity` 点击 `Configure All Detected Clients`。
5. 如果 Codex 里仍然没有 Unity 工具，先重启 MCP Server/重新配置客户端，再重新打开或刷新 Codex 会话。

每次开始涉及 Unity 场景、Prefab、层级、Console、Play Mode、物体绑定或编辑器状态的大任务前，Codex 必须先做一次 MCP 可用性检查：

1. 先通过工具发现能力搜索 Unity / MCP 相关工具，确认当前 Codex 会话里是否真的暴露了可调用的 Unity MCP 工具。
2. 如果 Unity MCP 工具可调用，优先用它读取 Hierarchy、Console、当前场景、选中物体、组件和 Play Mode 状态，再决定怎么改。
3. 如果 Unity 面板显示已连接，但 Codex 当前工具列表没有暴露 Unity MCP 调用能力，应明确说明“Unity MCP 已配置/可能已连接，但当前 Codex 工具面板不可直接调用”，优先通过上述 CLI 继续读取编辑器；CLI 也不可用时回退到项目文件、场景 YAML、Prefab、脚本和 Editor 菜单工具。
4. 如果需要确认 MCP 配置，应优先检查有效的 `C:\CodexHome\config.toml` 和 `Packages/manifest.json`；`C:\Users\彩笔\.codex\config.toml` 仅作历史配置参考，不要误认为这是 Codex 自带插件。

后续涉及 Unity 场景、层级、Prefab、材质、Console、测试运行、物体选择和编辑器状态的任务时，Codex 应优先尝试通过 Unity MCP 读取当前编辑器状态，再决定是否需要直接编辑项目文件。

如果 Unity MCP 未连接、Unity 未打开或 MCP 工具不可用，Codex 可以回退到读取项目文件、脚本和场景文本的方式，但需要在回复中说明当前没有通过 MCP 验证编辑器内状态。

涉及新增、修改或重构 Unity / C# 脚本时，仍然必须同步更新 `脚本使用说明总表.md`。

## 最终对白稿同步规则

- 当前正式游戏对白唯一来源是项目根目录 `HEARTH_Full_Game_Script_No_Audio_Tags_Native_English.md`。
- `HEARTH_Full_Game_Script_ElevenLabs_v3_Native_English.md` 只作为后续 AI 配音的情绪与表演参考，不直接同步为游戏字幕文本。
- 当前游戏不包含宣传片/序章。除非用户之后明确要求重新接入，不得在启动流程、正式对白或同步映射中恢复 `Prologue_HEARTHCommercial`。
- 除非用户明确提供并指定新的替代定稿，后续修改剧情、字幕、语音或关卡流程前，必须先读取正式游戏对白文件，不得以旧下载稿、旧 Dialogue Asset 或聊天记忆覆盖它。
- 正式稿变更后，优先运行 Unity 菜单 `Tools > Hearth > Dialogue > Sync All Dialogue From Final Script`，再运行 `Validate Final Script Coverage`；不要逐个手工复制全文到资产。
- 同步会保留“说话人与文本均未变化”的已有 `AudioClip`；文本发生变化的行需要重新检查并绑定语音。
- 所有正式对白继续使用 `HearthDialogueSequence`。语音存在时字幕显示时长必须跟随 `AudioClip.length`，无语音时才使用可编辑的回退时长。
