# MCP 通道与恢复验收

日期：2026-10-29。Unity 2022.3.61f1c1，CoplayDev MCP for Unity 10.3.0。

## 已完成

- 使用包自带 `unity-mcp` CLI，复用现有 uvx 离线缓存及固定 Python；入口 `Tools/Dev/unity-mcp.sh`。
- 修正 CLI 默认 GBK 编码导致输出勾号时抛出 UnicodeEncodeError：在入口设置 PYTHONUTF8 和 PYTHONIOENCODING。
- 启用现有插件的 Auto-Start on Editor Load；HTTP 模式保持开启。没有修改第三方 PackageCache，没有额外安装常驻服务。
- 保存开放场景后执行恢复测试，全程无需 Computer Use。

|场景|实际结果|
|---|---|
|脚本编译/域重载|编译后 CLI 成功执行 C# 状态查询；后续 refresh --compile 的请求在重载时断开，等待后 Console 查询和 Play 命令均恢复成功。|
|进入/退出 Play Mode|进入后曾返回 503/no Unity session，随后自动恢复；读取 playing=true，退出后读取 playing=false。未手动点击 Connect。|
|关闭并重启编辑器|旧进程 36928 退出，新进程 102348 启动；初始资产加载期间服务不可达，随后自动启动 HTTP 服务并注册 GAME-CBY；CLI 读到新编辑器 uptime 132.94 秒、SampleScene、playing=false。|

结论：三项恢复测试通过，但不是“重载期间零断线”。CLI 消除了对聊天客户端长连接的依赖，编辑器插件仍需要重载恢复时间。短暂不可用时先等待，再查询状态；不可自动重放已发送的修改命令。若未来服务在线但无实例，分别检查 Editor.log 中自动启动、WebSocket 和重载日志。若服务不可达，再检查服务进程。

本次不替换 KitWright/AI Game Developer：现有组合已具备自动恢复能力，暂未出现必须更换的持续故障。
