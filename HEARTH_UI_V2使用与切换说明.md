# HEARTH UI V2 使用与切换说明

> 2026-07-27 最新口径：第二套 UI 已按完整运行系统维护，不再通过旧界面叠加 GeneratedParts。
> 日常只允许使用 `Refresh Existing V2 Prefab Visuals` 定向刷新现有七套 V2 Prefab；
> `Rebuild All V2 UI Assets` 与 `Use V2 UI In Open Scene` 不得作为日常修复按钮。
> 若后文历史说明与本段冲突，以本段和“日常安全维护流程”为准。
>
> 2026-07-30 手工接管入口：
> 用户自行摆放固定 UI、导入新矢量 PNG、区分动态文字与点击命中区时，
> 优先阅读根目录 `HEARTH_UI_V2_手工调整与接管手册.md`。

## 当前状态

- `SampleScene` 当前保存为 V2 UI。
- Legacy UI 未删除，仍位于原 `Assets/Prefabs/UI/HearthHud/` 路径。
- V2 使用 Human、Companion、17F01、17F02、17F03、17F04、Lobby 七套独立 Prefab，
  保留各自功能 Wrapper、场景引用和剧情绑定。
- 共享视觉配置：
  - `Assets/UI/HEARTH/V2/Profiles/Hearth_UiV2Theme.asset`：
    终端深蓝黑、灰蓝、冷白、信息蓝及琥珀/成功/危险状态色、Liberation Sans、
    2 px 规则线和键帽尺寸。
  - `Assets/UI/HEARTH/V2/Profiles/Hearth_UiV2Layout_1920x1080.asset`：
    1920×1080 参考分辨率、安全区和 Human/终端共享矩形。
- 动态底板、按钮、键帽、规则线和文字由 Unity Image/TMP 构成；
  当前 V2 Prefab/场景的运行序列化引用审计中，
  `Assets/UI/HEARTH/GeneratedParts/` 引用数为 `0`。
- 2026-07-26 的 P0 修复后，场景中只保留
  `MIN_LOOP_ROOT/FlowManagers/ViewSwitchController` 这一套正式视角控制器。
- 五台终端都已绑定到各自 TV 层级内的 Camera，不再把 Human 第一人称相机当作终端相机。
- `Apply 17F04 Home Finale Setup` 在 V2 Home Prefab 存在时会继续使用
  `Terminal_17F04_Home_V2.prefab` 标准化 `TV (3)`；重跑后七个正式 UI 槽位仍保持
  `7/7 V2`，不会把 Home Terminal 降级成 Legacy。
- 终端、Human HUD 与视角切换现在共用带 Owner 的 `HearthPlayerControlLock`；
  终端打开期间会阻断移动、视角、交互、HUD 预览输入和手动 `R` 切换。
- 进入任意终端固定视角时，全部第一人称 UI 会隐藏；本轮已通过五台终端的
  Play Mode 打开/关闭检查，并对 17F01 做了组件中途停用后的恢复检查。
- `HearthUiStateCoordinator` 统一解析 Persistent、Dialogue、Interaction、Terminal、
  Modal、Takeover；终端只允许显式 `Terminal` Context 的所属对白显示，
  Modal/Takeover 抑制全部字幕视觉。三类接管状态都会抑制 Human Persistent HUD
  与大厅 HUD，但不会停止正式对白、语音或剧情计时。
- 初始 Human 教程只累计 10 秒有效 Gameplay，0.35 秒淡出；正式对白、动态 E、
  终端、Modal、Takeover、控制锁、暂停和非 Human 视角期间隐藏并暂停计时；
  Key 与 Action TMP 均禁止换行，`INTERACT` 不会被拆成两行。
- Lobby 任务终端现在作为独立非住户终端处理，`GetReplayResidentId()` 为空，
  不再被兜底推断成 `17F01`。
- 本轮终端语境、布局和文字安全修正没有修改正式对白源
  `HEARTH_Full_Game_Script_No_Audio_Tags_Native_English.md`。
- 当前七套实例仍保留既有 Scene Prefab Override，因此继续禁止用全量 Rebuild 覆盖现场。

## 日常安全维护流程

1. 退出 Play Mode，保存场景，并确认当前 Scene/Prefab Override 有可回退基线。
2. 只运行：
   `Tools > Hearth > UI V2 > Refresh Existing V2 Prefab Visuals`。
3. Refresh 现在会保留
   `HearthHudRoot_V2/PersistentHudLayer/V2_InitialTutorialRoot`，
   因此正常视觉刷新后不需要重复安装教程。
4. 只有首次缺少 Theme/Layout Profile、`HearthUiStateCoordinator` 或教程根，
   或 System Validator 明确报缺失时，才运行：
   `Tools > Hearth > UI V2 > System > Install Profiles And Human Tutorial`。
5. 依次运行：
   - `Tools > Hearth > UI V2 > Validate Open Scene UI`
   - `Tools > Hearth > UI V2 > System > Validate Profiles And Human Tutorial`
   - `Tools > Hearth > UI V2 > Subtitles > Validate Open Scene`
   - `Tools > Hearth > Validation > Validate Runtime Topology`
   - Lobby、17F01、17F02、17F03、Companion Hold、17F04 Finale 对应 Validator
6. 进入 Play Mode，按真实流程检查输入、互斥、相机、控制恢复和原生 1920×1080 截图。

安全边界：

- 不运行 `Rebuild All V2 UI Assets` 来修颜色、字号、位置或单个页面。
- 不运行 `Use V2 UI In Open Scene` 来刷新已经处于 V2 的正式场景。
- 不为 UI 微调重跑住户整套 Binder；只使用对应定向视觉/引用修复入口。
- 如果剧情/引用维护确实需要重跑 17F04 Finale Apply，完成后必须立即运行
  `Validate 17F04 Home Finale Setup` 与 `Validate Open Scene UI`，
  确认 Home Terminal Marker 有效且场景仍为 `7/7 V2`。
- 不手工把 GeneratedParts、白底、全屏装饰框或烘焙文字重新塞回运行 Prefab。

## Legacy / V2 整体切换（只作受控回退）

在 Unity 顶部菜单打开：

`Tools > Hearth > UI V2`

- `Use V2 UI In Open Scene`：当前场景切到新版。
- `Use Legacy UI In Open Scene`：当前场景切回旧版。
- `Validate Open Scene UI`：检查七个唯一 UI 槽位、V2/Legacy 标记、唯一正式
  `ViewSwitchController`、五台终端的本地相机归属、页面引用和必要剧情回调。

切入 V2 前会确认 Human、Companion 和五类终端共七个 V2 Prefab 都存在，并在修改场景前要求
当前场景恰好有一个 Human、一个 Companion、五台可唯一识别的终端。切换会保存当前场景，
并重映射剧情和流程脚本引用。不要在 Play Mode 中执行正式切换。

当前迁移不再遍历并复制所有 MonoBehaviour 状态，只复制 Human、Companion 和 Terminal 根控制器中
明确列入白名单的功能字段。Lobby 的两个必要回调和 17F04 Home 的一个必要回调单独迁移；
TMP、Image、Canvas、CanvasGroup 和通用 UnityEvent 不再作为全量迁移对象。

这些菜单用于完整版本迁移或回退，不是日常视觉制作流程。执行前必须保存、记录 Override，
执行后必须重新跑全部静态验证和五台终端 Play Mode 矩阵。

### 事务与回退验证

2026-07-26 已在 `SampleScene` 的隔离副本中完成以下验证，不直接拿正式场景做破坏性试验：

- V2 → Legacy 与 Legacy → V2 双向切换都通过。
- 单次切换作为完整 Undo 组，可撤销回原来的七个 UI 根，也可 Redo 回目标版本。
- 切换完成并保存后重新载入隔离场景，七个槽位、主题标记、页面引用、剧情回调和运行拓扑仍通过。
- 额外把 Human 与 Companion 放在同一父节点并交错插入占位兄弟；双向切换、Undo/Redo 和重载后，
  两者仍保持各自原兄弟索引。
- 失败预检和失败后置校验都不会保存半套 UI；测试中出现的映射歧义和 Listener 缺失均成功回滚。

这证明的是“七个 UI 根整体迁移的事务、回退和结构持久化”已经成立，不等于 Scene Override 已清理，
也不等于 Legacy/V2 的最终视觉差异已经完成逐像素验收。

## 全量重建（当前禁用为日常流程）

`Tools > Hearth > UI V2 > Rebuild All V2 UI Assets`
会从 Legacy 克隆并大范围重建七套 V2 Prefab。它可能覆盖既有 V2 VisualRoot、
手工视觉调整和现场 Override，也不是 GeneratedParts 更新入口。

当前项目的固定规则：

- 调颜色、字号、坐标、底板、键帽、规则线或单页布局：只运行安全 Refresh。
- Theme/Layout 首次缺失：运行 System Installer，不运行 Rebuild。
- 功能引用丢失：运行对应定向 Repair/Binder/Validator，不运行 Rebuild。
- 只有用户明确决定重新从 Legacy 建立整套 V2、已经备份并准备执行完整迁移回归时，
  才能单独规划全量 Rebuild；完成后不得自动继续覆盖正式场景。

## Theme 与 Layout 的调整入口

- 全局配色、字体、2 px 规则线、普通/宽键帽、字幕字号：
  `Assets/UI/HEARTH/V2/Profiles/Hearth_UiV2Theme.asset`。
- 1920×1080 安全区、Human 身份/任务/地点、字幕、教程、动态 E、
  终端顶栏/动作/内容/消息/底栏：
  `Assets/UI/HEARTH/V2/Profiles/Hearth_UiV2Layout_1920x1080.asset`。
- 终端 Footer 当前正式矩形为
  `X=96 / Y=920 / W=1728 / H=64`；其 Canvas 使用
  `Override Sorting=true / Sorting Order=20`，不要把 Y 改回会被实体电视裁切的 968。
- 所有相关 CanvasScaler 保持
  `Scale With Screen Size / Reference Resolution 1920×1080`。
- 修改 Profile 后运行安全 Refresh 和对应 Validator；不要通过缩放场景根或扩大 PNG
  来修位置。

终端正式色值基线为 `#0B1018/#09101C`、`#5F7895`、`#D7E6F6`、
`#78AADC`，状态使用琥珀、成功绿和危险红。当前字体统一使用项目已有
Liberation Sans，通过字号、字距、大小写和颜色建立层级。

## 11 类界面与真实入口

| 类别 | 用途 | 真实运行入口 |
|---|---|---|
| Base Human HUD | 身份、任务、地点、字幕安全区、短时教程 | `HearthFirstPersonHudController` |
| Base Companion HUD | Companion 状态、数据流、Hold E | `HearthCompanionHudController` |
| Base Doorway Terminal | 17F01/02/03 资料、回放/入户 | `HearthTvTerminalController` |
| Lobby Task Terminal | 领取任务、前 5 秒等待、关闭后简报 | `HearthLobbyFlowController` |
| Human Tab Menu | Tonight、History、Settings | Human HUD `Slide03/05/18-24` |
| Entity Robot Inspection | 实体诊断、Recall、A/B | `Hearth17F03InspectionPanel` |
| Home Terminal | Lily 留言、`ENTER HOME` | 17F04 Terminal Controller |
| Photo Archive | 实体照片相机、页码与退出门控 | `HearthPhotoFrameInteractable` → Human `Slide07/08` |
| Final Choice | 最终 A/B | Human `Slide09/14` |
| Shutdown Confirm | 高信任关机确认 | `HearthVirusPopupShutdownChallenge` → Human `Slide10` |
| Low Trust Challenge | 三波动态病毒弹窗 Takeover | `HearthVirusPopupShutdownChallenge` |

这些 Presenter/VisualRoot 只显示真实状态，不自行读取额外按键或推进剧情。
17F01/02 顶栏动作固定为 `REVIEW ARCHIVED EVENT`，17F03 为 `ENTER UNIT`，
17F04 为 `ENTER HOME`；锁定时动作保留并显示 `PLEASE WAIT`。

## 可以直接调整的内容

### 人类 HUD

- Prefab：`Assets/Prefabs/UI/HearthHud/V2/HearthHudRoot_V2.prefab`
- 可直接调整 TMP 字号、文字区域、菜单按钮大小和位置。
- 最终选择只使用真实
  `FocusLayer/FinalChoiceInputHint`；内容由 `HearthFirstPersonHudInput`
  根据水平/垂直选择与输入开放状态动态更新。不要重新创建静态
  `V2_FinalChoiceHint`，否则会出现上下两套重复按键提示。
- V2 安全 Refresh 会在 Final Choice（Slide09/14）、Shutdown Confirm（Slide10）和
  Low Trust Warning（Slide11–13）中禁用全部旧 `Border_*` Image；
  不要通过 Scene Override 重新开启这些遗留细框。
- Tab 高亮依赖稳定对象名，不要随意改名：
  - `MenuFocus`
  - `Button_TODAY`
  - `Button_DISPOSITION_HISTORY`
  - `Button_SYSTEM_SETTINGS`

### 陪伴单元 HUD

- Prefab：`Assets/Prefabs/UI/HearthHud/V2/Companion/HearthCompanionHudRoot_V2.prefab`
- 推荐统一调整：
  - `Assets/Data/HearthHud/Companion/Hearth_CompanionHudLayout.asset`
- 该资产可以统一缩放右上决策区、左下数据流和两区文字。
- 右上、左下仍可分别追加偏移。
- V2 安全刷新会在 `PersistentInfoLayer/V2_StatusPanel` 定向生成状态面板，并自动绑定
  `HearthCompanionHudController.statusPanelView`。面板只使用平面 Image、TMP 和 2 px
  规则线，Title/Rows/Footer/Accent 均由现有 Companion SceneData 动态写入。
- `TimedCardLayer/TriggerCardView` 会报告自身真实可见状态。
  临时 Trigger Card 开始淡入后，Controller 自动隐藏常驻 `V2_StatusPanel`；
  Card 淡出完成后，仅在当前 SceneData 含 Status 内容时恢复面板。
  TriggerCardView 停用时会立即清理可见状态；空 TimedCues 或缺失 CanvasGroup
  会安全隐藏，切换视角后不会卡住 Status Panel。
  Controller 只在 SceneData 确有非空标题、Footer 或状态行时显示状态卡。
  无需新增 Inspector 绑定，不要让剧情脚本手动切换 Status Panel。

### 终端

- Prefab：`Assets/Prefabs/UI/HearthHud/V2/Terminals/`
- 17F01/02/03、17F04 自宅、一楼任务终端各自独立。
- 终端内部文字和布局可以在对应 V2 Prefab 中调整。
- 不要把人类 HUD、陪伴单元 HUD 或大厅 HUD 放进终端 Prefab。
- `Terminal Camera` 必须来自对应 TV/Terminal 硬件根内；`Player Camera`、
  `Terminal Camera` 与 Canvas 的 `World Camera` 是三种不同语义，不要把 Human 第一人称相机拖入
  `Terminal Camera`。
- `Terminal Hardware Root` 应指向拥有该 Camera 的 TV/Terminal 根；
  `Player Control Lock` 应指向场景内共享的 `HearthPlayerControlLock`。
- 17F04 `TV (3)` 由 Finale Binder 标准化时还必须绑定唯一
  `ViewSwitchController`、共享 Control Lock、TV hardware root、TV 自有 World Camera，
  并让终端 Canvas 的 `worldCamera` 指向同一 `TerminalCamera`。
  V2 Home Prefab 可用时，Validator 要求其
  `HearthUiThemeMarker.Version=V2 / Build Label=HEARTH UI V2`。
- Lobby 任务终端的 `Active Loop Cue Player` 已持久化指向
  `MIN_LOOP_ROOT/Audio/StorySFX_Lobby`，但 `AssignmentTerminal.Hum` 当前还没有正式
  `AudioClip`。现阶段只代表播放路由正确，不能声称循环底噪已经可听。
- Lobby/Assignment 终端的住户 ID必须为空；17F01/02/03/04 才使用住户 ID。
- 终端内必须继续显示的正式对白应在调用处显式传
  `HearthSubtitleContext.Terminal`。Terminal 状态不会放行普通 Human/Field Unit
  世界字幕，Modal/Takeover 则不放行任何字幕视觉。

### 场景文字安全

- 大厅 Overlay 七处正式 TMP 和 Lobby 终端全部 TMP：固定字号 + Overflow，
  由 `Validate Ground Floor Opening Setup` 覆盖。
- 17F04 `PhotoExitHintCanvas/HintPanel/HintText`：固定字号 + Overflow，
  由 `Validate 17F04 Home Finale Setup` 覆盖。
- Human/Companion 动态 E 交互提示：定向 Repair 写入固定字号、禁止换行和 Overflow；
  HUD/Audio/English Prompt Validator 覆盖 Auto Size 与 Overflow 安全条件。
- 若文字容量不足，扩大 RectTransform 或内容区；不要恢复 Auto Size、Ellipsis、
  Truncate，也不要把文字烘焙进 PNG。

## GeneratedParts 历史素材

目录：`Assets/UI/HEARTH/GeneratedParts/`

- 该目录保留作历史素材、比对和回退，不是第二套 UI 的设计真值。
- 当前七套 V2 Prefab 和正式场景的序列化运行引用审计为 `0`；
  不要因为文件仍存在就把它们重新拖入 VisualRoot。
- 白底、定位细框、角落示意框、固定对白框、通用超大按钮框、超大键帽框和重复全屏外框
  均不得进入成品。
- 动态容器统一使用无 Sprite 的 Unity Image、2 px 规则线和 TMP；
  实体电视继续作为终端唯一外框。
- 未来确需生成新的固定装饰素材时，必须先锁定目标像素、透明区、文字安全区和 9-slice 边界；
  素材保持透明、无文字、单一用途，并在接入前单独做引用审计。

## Play Mode 预览

可使用：

- `Preview > Open Human Tab Menu`
- `Preview > Show Companion HUD`
- `Preview > Restore Human View`
- `Preview > Open 17F01 Terminal`
- `Preview > Close Open Terminal`

这些菜单只用于检查画面，不会替代正式剧情测试。

## 最小验收

1. 人类视角只显示人类 HUD。
2. 陪伴单元视角只显示陪伴单元 HUD。
3. 进入终端时，上述两套第一人称 HUD 都完全消失。
4. 终端只保留实体电视外框；页面、顶栏动作、页码和 Footer 清晰，
   不叠第二个全屏框；只显示显式 Terminal Context 的所属对白，
   不显示世界字幕或大厅卡片。
5. Esc 退出后恢复进入前相机与 HUD。
6. 普通玩法 Tab 可开菜单；对白、锁控、终端、Modal、Takeover 中 Tab 不穿透。
7. 初始教程只累计 10 秒有效 Gameplay；动态 E、对白或更高优先级提示出现时让位。
8. 最长人物与 Field Unit 对白保持固定字号、完整显示、向上增高，
   无自动缩字、截断、装饰线穿字或 `SPACE CONTINUE`。
9. 17F01/02 为 `REVIEW ARCHIVED EVENT`，17F03 为 `ENTER UNIT`，
   17F04 为 `ENTER HOME`；锁定时动作仍可见并显示 `PLEASE WAIT`。
10. 17F03 Entity Inspection、17F04 Photo Archive、高信任 Shutdown Confirm、
    低信任动态三波弹窗均从真实流程进入，输入和剧情事件不由 Presenter 接管。
11. V2 Prefab/场景不引用 GeneratedParts Sprite，不含白底或烘焙文字。
12. `Validate Open Scene UI`、System、Subtitle、Runtime Topology 和各住户 Validator
    输出通过。
13. 五台终端逐台打开时，任意时刻只保留一台 Camera 和一个 AudioListener；
    关闭后回到 Human Camera，控制锁 Owner 数回到 0，`R` 恢复。
14. 1920×1080 保存原生 Game View 基线；1280×720、2560×1440
    已完成真实 Game View 代表性缩放检查且无共享锚点漂移。
    11 类界面的双分辨率完整截图矩阵仍属于扩展验收。
15. 重跑 `Apply 17F04 Home Finale Setup` 后，Home Terminal 仍带有效 V2 Marker，
    唯一 ViewSwitch、共享 Control Lock、TV hardware root 与 World Camera 引用完整，
    `Validate Open Scene UI` 仍报告七个正式槽位为 `7/7 V2`。
16. Companion Trigger Card 可见时常驻 Status Panel 隐藏，淡出后按当前 SceneData
    自动恢复；Final Choice、Shutdown Confirm、Low Trust Warning 不存在启用的
    `Border_*` 遗留 Image。

本轮已实际执行并通过的范围：

- Unity 脚本刷新与编译，没有出现本轮脚本造成的新编译错误。
- `Validate Open Scene UI`、`Validate Runtime Topology`、
  Profiles/Tutorial、Subtitle、17F01、17F02、17F03、Companion Hold、
  Lobby 和 17F04 Finale 静态验证。
- Lobby、17F01、17F02、17F03、17F04 五台终端的打开、稳定、关闭。
- 17F01 打开后中途停用 `HearthTvTerminalController` 的异常退出清理。
- 两个 Owner 嵌套申请同一个控制锁时，先释放一个 Owner 不会提前恢复控制。
- Human 教程、Human Tab、17F01 Terminal 的全局互斥状态；
  17F03 Entity Inspection、17F04 Photo Archive 与高信任 Shutdown Confirm
  已从实际控制器入口完成运行接线检查。
- V2 Prefab/场景对 `Assets/UI/HEARTH/GeneratedParts/` 的序列化运行引用为 `0`。
- 相关 CanvasScaler 均为 `Scale With Screen Size / 1920×1080`；
  已在真实 Game View 切换到 1280×720 与 2560×1440 验证代表性 HUD/终端，
  共享锚点、文字和 Footer 无漂移，并保存两张分辨率基线。
- Lobby 不再返回 `17F01` 住户 ID；Lobby/17F04/动态 E 文字安全条件已进入对应 Validator。
- Final Choice 只保留一套真实动态输入提示；Companion V2 状态面板已自动绑定并由 SceneData 驱动。
- 17F04 Finale Apply 已按 V2 优先标准化 Home Terminal；Apply/Validate 后场景保持
  Human、Companion、Lobby、17F01–17F04 共 `7/7 V2`。
- Companion Trigger Card/Status Panel 使用可见状态通知互斥；
  终局相关页面的旧 `Border_*` 由安全 Refresh 显式禁用。
- 在隔离场景中完成 V2/Legacy 双向切换、Undo、Redo、保存后重新载入，并再次通过两套验证器。
- Lobby `activeLoopCuePlayer` 在切换和重载后仍保留；对应 Cue 的正式 `AudioClip` 仍待绑定。

尚未建立正式的 EditMode/PlayMode 测试程序集，上述 Play Mode 结果仍是通过 Unity MCP 执行的手工运行验证，
不是可在 CI 中重复运行的自动化测试。

## 1920×1080 基准截图

目录：

`Assets/Screenshots/HEARTH_UI_V2_2026-07-26/`

当前收口检查优先使用：

- `HEARTH_UI_V2_Human_Tutorial_Final_Acceptance.png`
- `HEARTH_UI_V2_Human_Tab_Final_Acceptance.png`
- `HEARTH_UI_V2_Terminal_17F01_Final_Acceptance.png`
- `HEARTH_UI_V2_Lobby_PleaseWait_Runtime_Acceptance.png`
- `HEARTH_UI_V2_Lobby_CloseReady_Runtime_Acceptance.png`
- `HEARTH_UI_V2_17F03_EntityInspection_ActualFlow_Final.png`
- `HEARTH_UI_V2_17F04_PhotoArchive_ActualFlow_Final.png`
- `HEARTH_UI_V2_17F04_ShutdownHighTrust_ActualFlow_Final.png`
- `HEARTH_UI_V2_Resolution_HD_1280x720.png`
- `HEARTH_UI_V2_Resolution_QHD_2560x1440.png`

截图必须来自原生 1920×1080 Game View，不用 Scene View 或编辑器缩放图替代。
修改 Coordinator、终端排序、字幕抑制或现场 Override 后，应重新捕获受影响的 Actual Flow，
并确认截图中没有旧字幕、Lobby 卡片、教程或双重大框残留。

## 当前剩余问题

Play Mode 仍会报告 ROOM2 书桌和 ROOM3 电视柜的负缩放 BoxCollider 错误。它们属于旧场景模型碰撞问题，不是 V2 UI 新增错误。

V2 仍有以下未完成项：

- Human V2 实例约有 1435 个 Scene Prefab Override，Companion V2 约有 200 个；
  七个 V2 实例合计约 2023 个 Override。该数字来自本轮结构审计，尚未做批量清理。
- 运行视觉真值已收敛到 Theme/Layout、V2 Prefab 和安全 Refresh 规则；
  GeneratedParts 只作历史素材且运行引用为 0。Scene Override 仍需在不破坏剧情引用的前提下
  分批审计，不能用全量 Rebuild 粗暴清理。
- 还没有独立的 EditMode/PlayMode 测试 asmdef，也没有覆盖五台终端全部入口、剧情交接和场景切换的参数化测试。
- 1280×720 与 2560×1440 已通过真实 Game View 代表性缩放验证并保存基线，
  但还没有保存 11 类界面的完整逐界面双分辨率截图矩阵。
- 已验证“保留现有 Override 的事务切换在保存重载后结构一致”，
  仍未验证“清空视觉 Override 后重开 Unity 仍完全一致”。

## 2026-07-30 V2 视觉与预览收口

本节为当前最新状态，优先于上方较早的“未完成项”描述。

- 20 个 Runtime Preview 状态已经形成一套最终 1920×1080 MCP 基线：
  Human 9、Companion 6、Terminal 5。
- Human 的固定 HUD、Field Unit、左右正式对白、Tab、照片、处置、关机和低信任
  均已接入透明矢量组件与统一灰蓝主题。
- Companion 已完成全屏科技框、REC、动态住户编号、Current Task、
  `SUBJECT - MONITORING`、Decision、Formal Dialogue 和 Permission Boundary。
- Doorway 终端为 Before、After、主动作三个焦点；Home 终端旧淡色标题和确认层已清理；
  五台终端只保留实体电视外框。
- `Apply Approved Closure` 可重复执行，`Validate Approved Closure` 当前通过。
- `Runtime Preview > QA > Audit Active Regions` 已对 1920×1080 全部代表状态检查；
  Human、Companion 与 Terminal 没有达到阈值的区域遮挡。
- 1280×720 与 2560×1440 已分别检查 Human Formal、Photo、
  Companion Decision 与 Doorway Terminal，区域审计均通过。

当前基线：

- `HEARTH_UI_V2_Baselines/MCP_Final/HEARTH_UI_V2_MCP_Final_ContactSheet.png`
- `HEARTH_UI_V2_Baselines/MCP_Responsive/HEARTH_UI_V2_MCP_Responsive_ContactSheet.png`

本轮遵循用户要求只做 Runtime Preview 和 MCP 验证，没有替用户跑大厅至结局的完整玩法。
正式剧情输入、控制恢复、音频衔接和所有关卡触发仍由用户最终实机通关验收。
