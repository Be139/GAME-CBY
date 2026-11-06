# 对白支撑代码审读记录

日期：2026-10-30。本记录表示所列文件已全文阅读，并注明关联证据；不表示全项目审查完成或连续剧情通关。

|文件（项目根相对路径）|职责与审读结论|关联核对/剩余事项|
|---|---|---|
|Assets/Scripts/MinLoop/MinLoopTypes.cs|阶段枚举、UnityEvent 和字幕行数据；保留序列化枚举顺序，未修改|71 个对白资产中 12 个非默认 durationMode 全为无语音时间卡，未发现现有资产使用手动时长截断语音|
|Assets/Scripts/MinLoop/HearthDialogueSequence.cs|正式对白序列及只读访问接口；无运行时自动写入，未修改|播放器从 Lines 读取，正式同步入口维护内容；R19 用临时序列验证取消，不改正式资产|
|Assets/Scripts/MinLoop/HearthDialoguePlaybackContext.cs|单次播放的全局/嵌入式 Surface 与上下文参数；无生命周期资源，未修改|播放器消费 FramedSurface/MessageSurface，时间卡仍走全局画面|
|Assets/Scripts/MinLoop/HearthDialoguePlaybackPolicy.cs|按稳定行 ID/序列 ID 应用自动字幕与留言策略；未修改|16 个策略 ID 均出现在当前 71 个对白资产文本内；仅证明 ID 存在，不证明全部线路可达|
|Assets/Scripts/MinLoop/HearthResidentDispositionDialogueSet.cs|住户 ID 归一化与处置对白引用；未修改|忽略连字符、下划线、空格和大小写是现有匹配契约；空 ID 的正式配置仍需随流程引用审查|
|Assets/Scripts/MinLoop/HearthDialogueSurface.cs|纯显示目标，音频/时序/输入留给播放器；未修改|Show 支持初次激活触发 Awake 后再次显现，不能简单增加 inactive 拒绝。终端与照片存在内部布局调用，需在对应控制器审读时核对正式资产与 fallback 分支|
|Assets/Scripts/UI/HearthHud/V2/HearthSubtitleViewBindings.cs|显式视觉绑定，无自动排版；未修改|IsComplete 只要求核心文字与画布引用，标题等附属引用需由正式资产检查覆盖|
|Assets/Scripts/MinLoop/HearthSubtitleStyleProfile.cs|可编辑样式数据与显式默认值接口；未修改|OnValidate 做数据归一化，不写场景；播放器对自然字幕/终端字号仍有覆盖值，视觉替换暂停期间不擅自改样式|

MinLoopSubtitlePlayer 的取消修复与测试见主审查记录 R19。该大文件初次输出截断，后续已补读多段；在完成剩余覆盖核对前，不把它标为全文审查完成。

后续调用审读：`HearthPhotoArchiveWorldView.cs` 与 `HearthPhotoArchiveViewBindings.cs` 已全文阅读。正式完整绑定会将 allowRuntimeFallback=false，Resolve/SetUiProfiles 不重排内部文字；Show 的相机相对摆位仍属现有行为。终端 `HearthTvTerminalController.cs` 的 Resolve/SetUiProfiles/Surface 构建分支发现 R20（见主记录）；尚未完成全文审读，不能将局部分支检查计作全部覆盖。
