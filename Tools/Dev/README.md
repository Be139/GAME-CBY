# Unity MCP CLI

当前项目使用 CoplayDev MCP for Unity 10.3.0。通过 Git for Windows Bash 执行：

```sh
./Tools/Dev/unity-mcp.sh status
./Tools/Dev/unity-mcp.sh instance list
./Tools/Dev/unity-mcp.sh editor console --type error --count 10
./Tools/Dev/unity-mcp.sh editor play
./Tools/Dev/unity-mcp.sh editor stop
./Tools/Dev/unity-mcp.sh -f json code execute -f path/to/command.cs
```

入口复用现有 uvx 缓存、固定 Python 与服务器版本，离线启动 CLI，不安装依赖。UTF-8 环境变量解决 Windows 控制台输出符号时的 GBK 编码异常。此脚本路径按当前工作站配置；迁移工作站时调整 uvx 路径。

Unity MCP 的 `Auto-Start on Editor Load` 已开启，HTTP 模式使用现有 localhost 服务。CLI 绕开 Codex 持久工具连接，但仍依赖 HTTP 服务与 Unity 会话。

脚本编译和 Play Mode 造成域重载时，可短暂返回 503/no Unity session；等待编辑器完成重载，再读 status 和无副作用的状态。不要自动重复执行刚才的修改命令：断线不代表修改没有执行。服务器健康不等于 Unity 会话健康，应确认 status 列出 GAME-CBY，并成功读取编辑器状态。

不要通过关闭 Domain Reload 来掩盖连接问题，不修改第三方 PackageCache。正式 UI 在 Prefab 和专用制作场景维护，CLI 是编辑器操作入口，不是生成外观的依赖。
