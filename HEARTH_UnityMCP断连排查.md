# Unity MCP 断连排查

2026-10-29 后续观察：本轮 Play 测试中 10.2.0 仍出现 503 / 会话断开及等待命令结果超时；编辑器窗口激活、域重载完成后 CLI 恢复并读到 Opening 状态，随后正常停止 Play。回退不能视为断连问题已根治；本次没有改 PackageCache 或关闭 Domain Reload。

日期：2026-10-29；Unity 2022.3.61f1c1，本地 HTTP 127.0.0.1:8080。当前按用户要求回退为 MCP for Unity **10.2.0**；下方保留 10.3.0 排查证据。

## 10.2.0 回退与验证

用户在反复断连后明确要求回退到上一个稳定版本进行检查。选择官方上一个非 beta 正式发布 [v10.2.0](https://github.com/CoplayDev/unity-mcp/releases/tag/v10.2.0)，并不预先断言其在本机长期稳定。项目更早的 Git 版本使用过 v10.0.0，此次先按相邻正式版本做对照。

- Unity 包锁定 `v10.2.0`，`git ls-remote --tags` 核实实际提交为 `30d22075093d1d35dfb0091c1c7550e9ad948577`；manifest、packages-lock 同步。官方 package.json 依赖与既有锁定依赖一致，保留项目其他包改动。
- 服务端与 CLI 均固定 `mcpforunityserver==10.2.0`，复用固定 Python 和 uvx。已精确停止原 10.3.0 服务并以隐藏窗口启动 10.2.0，服务实际安装元数据 Version=10.2.0；未修改代理、证书、Domain Reload 或 PackageCache 源码。
- Unity Package Manager Resolve 成功，编辑器内 `PackageInfo.FindForAssembly` 返回版本 10.2.0、目录 `com.coplaydev.unity-mcp@30d2207509`。包切换重载后自动重新连接，无需手动 Connect。
- 随后打开 MCP 面板，发起域重载及真实脚本编译请求。服务日志记录 21:41:30 会话断开，21:41:43 自动重新注册 GAME-CBY，21:41:45 完成 35 个插件工具注册（Asia/Taipei）。重载后 status 与代码执行成功，本次自动恢复耗时约 13 秒。测试开始时面板打开；最终复查面板已关闭，不能称为全程持续开窗耐久测试。
- 最终 Console error 为 0 条，编辑器实际加载 10.2.0，CLI 可读取场景。场景保持 SampleScene、Edit Mode、27,888 个对象及原有未保存状态；回退不撤销场景整理。
- 本次结果：启动、读取与一次实际脚本重载自动恢复通过。未做长时间耐久测试；不能据此认定 10.3.0 是唯一原因或断连已永久解决。保留 10.2.0，不自动升级。

本轮 10.3.0 又出现 orphaned-session 日志；当时界面实际可见 MCP 面板，因此不能声称是“面板始终关闭仍断连”的复现。关闭面板仍只是缓解措施，不是已验证根治。

## 结论

10.3.0 的证据更支持 Unity 插件端的会话检测/恢复问题，而非外网断网。不能证明这是 10.3.0 新引入的回归；旧版本也有同类报告。随后已按用户授权执行上述 10.2.0 回退对照。

本机 Editor.log 出现 `Server no longer running; ending orphaned session.`，同阶段 CLI 可连 HTTP 服务但没有 Unity 实例。包内 `McpConnectionSection.UpdateConnectionStatus` 会据本地服务探测结果调用 `EndOrphanedSessionAsync`；后者取消重连标志并调用 `Bridge.StopAsync`。因此这条日志对应的是编辑器主动结束会话，不能直接解释为服务进程已崩溃。

官方 [issue #1207](https://github.com/CoplayDev/unity-mcp/issues/1207) 记录过服务健康、编辑器却误判服务消失并断开连接的同类路径，也讨论域重载期间的连接恢复问题。它报告的是旧版和不同平台，支持问题机制相似，不构成本机每次断连原因的证明。

Unity 的 Curl error 35 是证书校验错误；当前 MCP 使用 localhost 明文 HTTP，没有证据把该外部请求错误与本地会话断连建立因果关系。没有关闭证书验证、改代理或防火墙。

## 已采取措施与结果

1. 保持现有服务与版本，连接后通过 CLI 关闭 MCP for Unity 面板。窗口 OnDisable 解绑界面更新，不调用 Bridge.Stop；避免其界面轮询触发上述主动断连路径。
2. 面板关闭后，CLI 返回关闭成功；后续重新查询 status 仍列出 GAME-CBY 实例。当前缓解有效，但未做长时或多次域重载稳定性验收，不能称为彻底修复。
3. AGENTS 规定 CLI 优先、区分 HTTP 服务和 Unity 会话；仅自动恢复失败且没有可用接口时，才用界面恢复 Connect/Start Session。修改请求遇到断连先核对实际结果，避免重复执行。
4. `uvx --offline` 输出 Installed 72 packages 表示可以从本地缓存构建隔离环境，不证明网络下载，也没有证据证明它造成此次服务断连。已纠正文档的“不安装依赖”表述。

若再次出现，优先对齐 Editor.log 中 WebSocket/HTTP Reload/orphaned 时间与服务器健康、进程和注册日志；面板已关闭仍断连时，应继续查域重载和 WebSocket 恢复路径。当前没有修改 PackageCache、关闭 Domain Reload 或引入自动重启守护程序。
