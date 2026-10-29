# HEARTH 程序全量审查与整改

2026-10-29；状态：进行中。范围：Assets 中全部 C#（含导入资产），Tools 中 C#/Python/Shell；不修改 Unity PackageCache、Library 编译产物。初始清单 507 文件、103,449 行，见 Docs/ProgramAudit/source-scan.json。逐文件完成状态将随审读更新，自动特征命中不等于缺陷。

## 审查方法

核对每个文件的用途、使用位置、调用/绑定、编辑器与运行时职责、表现数据来源、资源/订阅生命周期及错误路径。场景引用索引与实际状态用于区分正式使用、兼容路径和第三方示例；没有挂载不自动判定可删除。第三方代码的问题优先在正式集成配置处处理，只有实际需要时维护补丁。

## 已定位并修复

### R01：Reach 空本地化组件启动越界

`LocalizedObject.InitializeItem()` 在没有 UIManager 时访问空搜索结果 `[0]`。正式游戏只使用 Reach 视觉层，却在五个正式 Prefab 里带入了 37 个无 key、无事件、无文本/音频/图片目标的 Custom LocalizedObject；这些组件没有业务用途。

已删除上述正式资产中的空组件，关闭对应 ButtonManager.useLocalization；没有额外生成本地化管理器或覆盖正式英文文本。五个资产的 TMP 文本逐项比对一致，场景剩余组件为 0。现有 HearthProductionUiTools.ValidateProductionUi 增加对应集成检查。

验证：实际 Play 进入 Lobby Opening、Human 视角和正确相机，Console error=0；退出 Play，SampleScene 已保存。第三方套件本身的通用本地化实现仍保留；未来确需多语言应另行配置完整文案来源与资源，不能重新复制空组件。

## 正在追踪

- 正式 UI 绑定存在时仍保留的动态创建、硬编码布局与 fallback：区分真实动态效果和过时生成路径后处理。
- 自有与导入程序的调用、场景挂载、编辑器自动写入与资源生命周期。
- 旧校验器中指向已退役菜单的诊断提示，以及仅对历史外观成立的检查口径。

## 验证边界

本页当前只把 R01 记为已修复；全量审读尚未完成。编译、启动、定点接口测试、连续通关与视觉验收分别记录，不互相替代。
