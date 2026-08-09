# HEARTH 第二套 UI 手工调整与接管手册

> 更新日期：2026-08-08
> 适用项目：`D:\UGit\GAME-CBY`
> 适用场景：`Assets/Scenes/SampleScene.unity`
> 设计参考分辨率：1920 × 1080
> 本文目的：让用户可以先接管固定 UI 的位置与图片摆放，并把结果安全地交还给 Codex 继续完成动态绑定、颜色统一和全流程收口。

## 1. 先说结论

这套 UI 可以由用户先手工调整，而且 Human HUD 最适合先这样做。

但当前 UI 不是“画面上看见什么就只拖什么”这么简单。实际分成三层：

1. **固定视觉层**
   - 身份、Current Task、Location、装饰线、菜单底板、固定边框。
   - 这部分最适合用户直接在 Prefab Mode 中调整。

2. **动态显示层**
   - 正式对白、Field Unit 辅助通讯、说话人姓名、任务文字、地点文字、住户编号、照片、终端状态。
   - 文字和显隐由脚本实时写入。
   - 可以调整承载它们的区域，但不能删除、改名或把动态 TMP 换成烘焙文字图片。

3. **输入与焦点层**
   - Button 点击区域、键盘焦点框、不可见的选择目标、E 射线交互、Hold E 进度。
   - 有些对象看不见，却决定按钮是否能点、键盘高亮出现在哪里。
   - 只移动可见文字或底图，可能造成“画面在这里，点击区域还在旧位置”的问题。

因此，本轮推荐顺序是：

1. 用户先完成 Human 固定 HUD、Human 菜单和固定装饰图的位置。
2. Codex 再把正式对白与 Field Unit 拆成两套可独立编辑的视觉根。
3. 用户或 Codex继续摆放动态框体。
4. Codex统一颜色、运行时绑定、输入焦点和终端/Companion 系统。
5. 最后只使用 Runtime Preview 菜单逐页预览，由用户亲自跑完整游戏流程。

## 2. 术语统一

用户口述的 “A Few Unit” 在项目中对应：

`Field Unit`

它是 Human 视角中的辅助通讯通道，不是正式人物对白，也不是 Companion 第一人称 HUD。

两类对白必须分清：

| 类型 | 用途 | 输入 | 玩家控制 | 视觉位置 |
|---|---|---|---|---|
| Formal 正式对白 | Mia 与 NPC/住户的正式对话 | Space 推进 | 锁定移动、视角、交互和菜单 | 屏幕下方中央 |
| Auxiliary / Field Unit | Field Unit 给 Mia 的辅助通讯 | Field Unit 可见句由 Space 推进；Mia 无字幕语音自动衔接 | 可移动、可转视角；临时阻断冲突交互 | Current Task 下方、中右区域 |

同一时刻只允许显示其中一类，正式对白与 Field Unit 不应叠在一起。

## 3. 当前场景中的真实 UI 根

以下名称已按当前打开的 `SampleScene` 实际读取，不是根据旧文档猜测。

| 系统 | Scene 中的根名称或路径 | Prefab 资产 |
|---|---|---|
| Human HUD | `HearthHudRoot` | `Assets/Prefabs/UI/HearthHud/V2/HearthHudRoot_V2.prefab` |
| Companion HUD | `HearthCompanionHudRoot` | `Assets/Prefabs/UI/HearthHud/V2/Companion/HearthCompanionHudRoot_V2.prefab` |
| 主对白播放器 | `MIN_LOOP_ROOT/UI/MinLoopSubtitlePlayer` | 使用下方 Subtitle Prefab |
| 主对白视觉 | `MIN_LOOP_ROOT/UI/MinLoopSubtitlePlayer/HearthSubtitleVisualCanvas_V2` | `Assets/Prefabs/UI/HearthSubtitle/V2/HearthSubtitleVisualCanvas_V2.prefab` |
| 17F04 尾声对白 | `MIN_LOOP_ROOT/Finale_17F04/UI/EpilogueDialogue_17F04` | 独立实例；不要当作全局对白来调整 |
| 一楼同步/任务终端 | `1F (1)/TvUnitSet5/MonitorCanvas/Terminal_Lobby_Assignment` | `Assets/Prefabs/UI/HearthHud/V2/Terminals/Terminal_Lobby_Assignment_V2.prefab` |
| 17F01 门口终端 | `17F/ROOM1/TV (3)/MonitorCanvas/Terminal_17F01` | `Assets/Prefabs/UI/HearthHud/V2/Terminals/Terminal_17F01_V2.prefab` |
| 17F02 门口终端 | `17F/ROOM3/TV (2)/MonitorCanvas/Terminal_17F02` | `Assets/Prefabs/UI/HearthHud/V2/Terminals/Terminal_17F02_V2.prefab` |
| 17F03 门口终端 | `17F/ROOM2/TV (4)/MonitorCanvas/Terminal_17F03_Alert` | `Assets/Prefabs/UI/HearthHud/V2/Terminals/Terminal_17F03_Alert_V2.prefab` |
| 17F04 家庭终端 | `17F/ROOM4/TV (3)/MonitorCanvas/Terminal_17F04_Home_V2` | `Assets/Prefabs/UI/HearthHud/V2/Terminals/Terminal_17F04_Home_V2.prefab` |

注意：

- Scene 根名称是 `HearthHudRoot`，Prefab 文件名是 `HearthHudRoot_V2`，两者不完全相同。
- 不要把 Scene 根强行改名成带 `_V2` 的名称。
- 不要修改以上终端的场景父级、Camera、MonitorCanvas 或世界空间缩放。用户只调整终端 Prefab 内部的 UI。

## 4. Prefab Mode 与 Scene 调整的区别

### 推荐：在 Prefab Mode 中修改

1. 在 Project 窗口找到目标 Prefab。
2. 双击 Prefab，进入 Prefab Mode。
3. 只调整该 Prefab 内的 RectTransform、Image、TMP 和子装饰。
4. 保存 Prefab。
5. 回到 `SampleScene`，进入 Play Mode，用 Runtime Preview 查看。

优点：

- 修改会成为该 UI 的正式默认值。
- 不依赖某一个 Scene Override。
- 后续 Codex 接手时更容易知道用户到底改了什么。

### 仅用于临时试摆：在 Scene 实例中修改

如果直接在 Hierarchy 的 `HearthHudRoot` 下拖位置：

- 修改只形成当前 Scene 的 Prefab Override。
- 以后替换 Prefab、运行 Builder 或 Apply 其他修改时更容易丢失。
- 不要使用 `Overrides > Apply All`，因为它可能把场景中的功能引用一起写回 Prefab。

如确实在 Scene 中试摆：

1. 退出 Play Mode。
2. 只修改一个明确的 RectTransform 或 Image 属性。
3. 在 Overrides 面板逐条查看。
4. 只 Apply 明确的视觉属性；不 Apply 控制器、Camera、剧情引用和 UnityEvent。
5. 把最终 RectTransform 数值记录到本文最后的交接表。

## 5. 1920 × 1080 的位置基线

下面使用左上角为原点的设计坐标，`X` 向右，`Y` 向下。

| 区域 | X | Y | W | H |
|---|---:|---:|---:|---:|
| Human 身份 | 64 | 48 | 432 | 96 |
| Current Task | 1408 | 48 | 448 | 104 |
| Location | 64 | 944 | 360 | 80 |
| Field Unit 辅助通讯 | 1408 | 318 | 448 | 260 |
| Formal 正式对白正文框 | 432 | 670 | 960 | 256 |
| Formal 正文文字区 | 472 | 714 | 880 | 164 |
| Formal 左/右姓名签 | 左 X=432；右 X=1052 | 622 | 340 | 48 |
| 动态 E 提示 | 660 | 688 | 600 | 64 |
| 初始教程 | 1136 | 928 | 720 | 96 |
| 终端标题和导航安全区 | 120 | 72 | 1680 | 140 |
| 终端内容区 | 120 | 232 | 1680 | 528 |
| 终端消息预留区 | 320 | 790 | 1280 | 120 |
| 终端 Footer | 96 | 920 | 1728 | 64 |

Canvas 必须保持：

- `Canvas Scaler = Scale With Screen Size`
- `Reference Resolution = 1920 × 1080`
- `Match = 0.5`

不要通过缩放整个 `HearthHudRoot` 来修正某一个 UI 的位置。

不要随意重设已有对象的 Anchor 和 Pivot。对已有对象，优先使用 Rect Tool 拖动并保留原 Anchor；对新建装饰子物体，才使用 Stretch 到父物体。

## 6. Human 固定 HUD：可以现在直接调整

Prefab：

`Assets/Prefabs/UI/HearthHud/V2/HearthHudRoot_V2.prefab`

Scene 根：

`HearthHudRoot`

### 6.1 左上身份

对象路径：

- `PersistentHudLayer/PersistentHud/Text_003_COMPANION_UNIT___ACTIVE`
- `PersistentHudLayer/PersistentHud/Text_004_MIA___7842`
- `PersistentHudLayer/PersistentHud/V2_HeaderUnderline`

这三个对象应多选后一起移动。

不要只移动其中一行，否则标题、身份和横线会错位。

目标：

- 左边缘对齐 `X=64`。
- 文本左对齐。
- 身份区整体放在屏幕左上安全区。

这部分没有点击功能，只是显示。

### 6.2 右上 Current Task

对象路径：

- `PersistentHudLayer/PersistentHud/Text_006_CURRENT_TASK`
- `PersistentHudLayer/PersistentHud/V2_CurrentTaskBody`
- `PersistentHudLayer/PersistentHud/V2_TaskUnderline`

这三个对象应一起调整。

目标：

- 区域右边缘对齐到 `X=1856`。
- TMP Alignment 使用右对齐。
- 文本从右向左延伸。
- Field Unit 区域从它下方开始，不能与 Lily 消息或 Field Unit 框重叠。

这部分没有点击功能，文字由运行时更新。

旧对象 `Text_007_NIGHT_ROUNDS___BLOCK_A___17F` 不再是正式正文入口；不要用它判断
Game View 中 Current Task 的字号或位置。

### 6.3 左下 Location

对象路径：

`PersistentHudLayer/PersistentHud/LocationHud`

子物体：

- `LocationTitleText`
- `LocationValueText`
- `LocationGlowText`

只移动 `LocationHud` 父物体即可，三个文字会一起移动。

当前 Prefab 约为：

- 左侧 `64`
- 距底部 `48`
- 大小约 `340 × 92`

设计基线为：

- `X=64`
- `Y=944`
- `W=360`
- `H=80`

最重要的是让它与左上身份的左边缘对齐。

这部分没有点击功能，Location 内容由运行时更新。

### 6.4 初始 10 秒教程

对象路径：

`PersistentHudLayer/V2_InitialTutorialRoot`

子结构：

- `Slot_WASD`
- `Slot_MOUSE`
- `Slot_E`
- `Slot_TAB`
- 每个 Slot 下的 `Keycap / Key / KeyRule / Action`

移动规则：

- 可以安全移动 `V2_InitialTutorialRoot` 父物体。
- 不要逐个拆散 Slot。
- 不要改名 `Slot_WASD / Slot_MOUSE / Slot_E / Slot_TAB`。
- 新键帽图片放到每个 Slot 的 `Keycap` Image。

这部分不是按钮，不能点击。

### 6.5 动态 E 交互提示

对象路径：

`InteractionPromptLayer/PlayerInteractionPrompt`

文字：

`InteractionPromptLayer/PlayerInteractionPrompt/InteractionText`

移动规则：

- 只移动整个 `PlayerInteractionPrompt`。
- 不要只移动 `InteractionText`。
- 不要改名或改变这条层级。

这不是 UI Button。按 E 的逻辑来自屏幕中心射线和世界交互脚本。

`PlayerInteraction` 会按 `HearthHudRoot` 和上述固定路径查找它，因此改名后可能再次出现“提示消失，但 E 仍然有效”的问题。

## 7. Human Tab 菜单：可调整，但必须连同点击区域

页面根：

`PanelLayer/Slide03_MainMenu`

### 7.1 三个真正的按钮

| 用途 | Button 根名称 | 当前参考位置和大小 |
|---|---|---|
| Tonight’s Rounds | `Button_TODAY` | 左上约 `X=64 / Y=204 / W=540 / H=84` |
| Disposition History | `Button_DISPOSITION_HISTORY` | 左上约 `X=64 / Y=312 / W=540 / H=84` |
| System Settings | `Button_SYSTEM_SETTINGS` | 左上约 `X=64 / Y=420 / W=540 / H=84` |

每个 Button 根上同时存在：

- Unity `Button`
- `HearthFirstPersonHudButtonAction`

脚本会在 Awake 时动态挂接点击事件，因此 Inspector 中 `On Click()` 看起来为空也可能是正常的。

调整规则：

- 必须移动整个 `Button_*` 根。
- Button 的 RectTransform 就是鼠标点击命中区。
- 不要只移动底图或 TMP。
- 不要删除 `HearthFirstPersonHudButtonAction`。

菜单动态焦点：

`FocusLayer/MenuFocus`

它会在运行时从三个 Button 的 RectTransform 复制位置和大小，因此不要手调 `MenuFocus`。只要移动 Button 根，焦点框会自动跟随。

### 7.2 菜单右侧信息面板

对象：

- `PanelLayer/Slide03_MainMenu/V2_MenuTaskPanel`
- `PanelLayer/Slide03_MainMenu/V2_MenuFieldUnitPanel`

当前参考：

- Task Panel：右上约 `X=1428 / Y=130 / W=420 / H=190`
- Field Unit Panel：右侧约 `X=1428 / Y=354 / W=420 / H=260`

它们只是菜单信息面板，不是世界中的 Field Unit 辅助通讯框。

## 8. Final Choice、Shutdown 和警告按钮

### 8.1 Final Choice

页面：

`PanelLayer/Slide09_FinalChoice`

实际按钮：

- `Button_ANSWER_LILY`
- `Button_COMPANION_ANSWER`

键盘目标：

- `FocusLayer/FinalChoiceTarget_A`
- `FocusLayer/FinalChoiceTarget_B`

动态描边：

`FocusLayer/FinalChoiceFocus`

参考区域：

- A：`X=432 / Y=350 / W=1056 / H=112`
- B：`X=432 / Y=486 / W=1056 / H=112`

移动一个选项时，必须同步移动：

1. 实际 `Button_*` 根。
2. 该选项的文字和底图。
3. 对应的 `FinalChoiceTarget_A` 或 `FinalChoiceTarget_B`。

不要手调 `FinalChoiceFocus`，它是运行时描边。

`Slide14_Return` 中还有一套同名最终选择按钮，修改最终页面时也要检查那一页。

### 8.2 Shutdown、Warning 和 Exit

常见真实 Button 根：

- `Button_CONFIRM`
- `Button_CANCEL`
- `Button_YES`
- `Button_NO`
- `Button_FORCE_EXECUTE`
- `Button_EXIT_GAME`

这些都可能带 `Button + HearthFirstPersonHudButtonAction`。

只能移动整个 Button 根，不要只移动其文字或底图。

### 8.3 Settings 隐形键盘目标

设置页面使用：

- `FocusLayer/SettingsTarget_Master`
- `FocusLayer/SettingsTarget_Dialogue`
- `FocusLayer/SettingsTarget_Ambient`
- `FocusLayer/SettingsTarget_SFX`
- `FocusLayer/SettingsTarget_Exit`
- `FocusLayer/SettingsFocus`

如果移动设置行，必须同步移动对应的 `SettingsTarget_*`。不要手调动态 `SettingsFocus`。

## 9. 正式对白与 Field Unit：当前不能只靠 Scene 拖动

### 9.1 当前真实结构

场景主路径：

`MIN_LOOP_ROOT/UI/MinLoopSubtitlePlayer/HearthSubtitleVisualCanvas_V2/VisualRoot`

子物体：

- `Backdrop`
- `AccentRule`
- `SpeakerTab`
- `Speaker`
- `Body`

这些对象全部是纯显示：

- `raycastTarget = false`
- 不能点击
- Space 由输入脚本处理，不是由 UI Button 处理

### 9.2 为什么用户找不到独立的 Field Unit 框

目前 Formal 与 Field Unit 共用同一套：

`Backdrop / AccentRule / SpeakerTab / Speaker / Body`

脚本在每次显示时，根据 Dialogue Channel 把它们移动到不同位置。

因此：

- 在 Prefab 或 Scene 中手动拖 `Backdrop`，运行时会被脚本重新写回。
- 给 `Backdrop` 塞一张 Formal 对话框图片，会同时影响 Field Unit。
- 给 `SpeakerTab` 塞一张左姓名签图片，Mia 或右侧说话人也会继续使用同一张图。

当前代码写死的区域就是第 5 节中的 Formal 和 Field Unit 坐标。

### 9.3 正确的目标结构

后续应由 Codex先把动态视觉拆成：

```text
VisualRoot
├── FormalVisualRoot
│   ├── BackdropFill
│   ├── DialogueFrame
│   ├── AccentRule
│   ├── SpeakerTabLeft
│   ├── SpeakerTabRight
│   ├── Speaker
│   └── Body
└── AuxiliaryVisualRoot
    ├── BackdropFill
    ├── FieldUnitFrame
    ├── AccentRule
    ├── Speaker
    └── Body
```

这样用户才能分别拖：

- Formal 正文框。
- Formal 左姓名签。
- Formal 右姓名签。
- Field Unit 辅助框。

在这个拆分完成前，用户可以确定想要的位置、大小和图片，但不要删除或改名当前五个绑定对象。

### 9.4 Field Unit 必须有半透明衬底

是的，正确的 Field Unit 结构必须是两层：

1. **半透明底色**
   - Unity `Image`
   - Source Image 可以为空
   - 颜色使用 `#09101C`
   - 建议 Alpha 约 `0.76`

2. **透明装饰边框**
   - 使用新的透明 PNG
   - 建议素材：`HUD_Feedback_FieldUnitToastFrame_640x180.png`
   - Image Tint 使用 `#78AADC`
   - `Raycast Target` 关闭
   - 使用 `Sliced`

再把 `Speaker` 和 `Body` TMP 放在这两层上面。

不能只把透明线框 PNG 放进去，因为线框的中心是透明的；如果没有单独的 Unity Image 底色，游戏场景会直接透出来，文字可读性会变差。

Formal 对话框也遵循同样原则：

- `BackdropFill` 提供半透明深蓝黑。
- `HUD_Common_DialogueFrame_960x256.png` 只提供装饰边框。
- 左右 `SpeakerTab` 分别使用独立图片。

## 10. 新 SVG/PNG 美术资产在哪里

已经完成去斜线、统一白色 Mask、透明背景的 27 个组件。

总目录：

`D:\image-to-svg\outputs\hearth-ui-library-no-slashes`

可直接给 Unity 使用的 PNG：

`D:\image-to-svg\outputs\hearth-ui-library-no-slashes\png`

可继续编辑的 SVG：

`D:\image-to-svg\outputs\hearth-ui-library-no-slashes\svg`

总览图：

`D:\image-to-svg\outputs\hearth-ui-library-no-slashes\review\HEARTH_UI_Vector_Library_NoSlashes_ContactSheet.png`

清单和 9-slice 边界：

`D:\image-to-svg\outputs\hearth-ui-library-no-slashes\manifest.json`

当前这些新素材还没有正式导入并绑定到 Unity V2 Prefab。

不要重新使用：

`Assets/UI/HEARTH/GeneratedParts/`

该目录是旧素材和历史来源，不是本轮新矢量库。

## 11. 如何把图片放进 Unity

### 11.1 建议建立的 Unity 目录

在 Project 窗口中建立：

`Assets/UI/HEARTH/V2/VectorParts/`

按原分类复制 PNG：

- `Common`
- `Companion`
- `Feedback`
- `Finale`
- `Inspection`
- `Interaction`
- `Terminal`

SVG 继续保留在 `D:\image-to-svg` 作为可编辑源文件；Unity 运行时先使用 PNG。

### 11.2 PNG 导入设置

选中导入的 PNG，在 Inspector 设置：

- Texture Type：`Sprite (2D and UI)`
- Sprite Mode：`Single`
- Alpha Is Transparency：开启
- Mesh Type：`Full Rect`
- Generate Mip Maps：关闭
- Wrap Mode：`Clamp`
- Filter Mode：`Bilinear`
- Compression：调试阶段先用 `None`

点击 Apply。

### 11.3 2 倍导出尺寸

新 PNG 是以 2 倍分辨率导出的。

例如：

- 文件名设计尺寸：`HUD_Common_DialogueFrame_960x256`
- 实际 PNG 像素：`1920 × 512`
- Unity 中 RectTransform 仍使用：`960 × 256`

不要把 RectTransform 也设成 `1920 × 512`，否则 UI 会放大两倍。

### 11.4 9-slice

需要拉伸的素材使用 Sprite Editor 设置 Border。

常用边界以实际 PNG 像素为准：

| 素材 | Border |
|---|---|
| ButtonFrame | 32 / 32 / 32 / 32 |
| PanelFrame | 40 / 40 / 40 / 40 |
| FieldUnitToastFrame | 40 / 40 / 40 / 40 |
| PleaseWaitFrame | 32 / 32 / 32 / 32 |

设置完成后：

- UI Image 的 `Image Type = Sliced`
- 不要使用 Preserve Aspect
- `Raycast Target` 关闭，除非该 Image 本身就是 Button 的 Target Graphic

不拉伸的固定素材使用 `Simple`：

- DialogueFrame
- SpeakerTab Left/Right
- Keycap 64×40
- Space Keycap 96×40
- Companion FullscreenFrame

## 12. 图片、底色、文字的正确叠放顺序

一个普通面板推荐这样建立：

```text
PanelRoot
├── BackdropFill
├── FrameDecoration
├── AccentRule
├── TitleText
└── BodyText
```

Hierarchy 越靠后通常越晚绘制，所以文字应在底板和边框后面。

### BackdropFill

- Unity Image
- Source Image：None
- Color：`#09101C`
- Alpha：根据用途约 0.62–0.86
- Raycast Target：关闭
- Stretch 到父物体

### FrameDecoration

- Unity Image
- Source Image：新的透明 PNG
- Color：`#78AADC`
- Raycast Target：关闭
- 固定框用 Simple；可拉伸框用 Sliced
- Stretch 到父物体

### TMP 文字

- 继续使用 TextMeshPro。
- 不能把姓名、正文、任务、地点烘焙到 PNG。
- 正文不使用 Auto Size。
- 容量不够时扩大文字 Rect，不缩小文字。

## 13. 推荐色彩分工

| 用途 | 颜色 |
|---|---|
| 深色底板 | `#09101C` |
| 更深终端背景 | `#0B1018` |
| 次级灰蓝 | `#5F7895` |
| 主文字冷白 | `#D7E6F6` |
| 普通装饰和信息蓝 | `#78AADC` |
| 警告/等待强调 | `#E0973F` 附近 |
| 成功 | 绿色，只用于成功状态 |
| 危险 | 红色，只用于危险和关机接管 |

新 SVG/PNG 是白色 Mask，Unity Image Tint 决定最终颜色。

不要在 Photoshop 中为每张 PNG 分别改色，否则后续很难统一主题。

## 14. Companion HUD 的可调整位置

Prefab：

`Assets/Prefabs/UI/HearthHud/V2/Companion/HearthCompanionHudRoot_V2.prefab`

Scene 根：

`HearthCompanionHudRoot`

常用对象：

| 区域 | 路径或名称 | 手工调整规则 |
|---|---|---|
| 全屏科技框 | `FrameLayer/CompanionRobotFrame` | 可替换透明全屏框；不要增加第二个全屏框 |
| 左上身份 | `PersistentInfoLayer/V2_Identity` | 可移动；不要改名 |
| 身份下划线 | `V2_IdentityUnderline` | 与身份一起移动 |
| 顶部 REC | `PersistentInfoLayer/V2_REC` | 可移动；不要改名 |
| 右上任务 | `PersistentInfoLayer/V2_CurrentTask` | 可移动；不要改名 |
| Subject Monitoring | `PersistentInfoLayer/V2_StatusPanel` | 可整体移动；文字由 SceneData 写入 |
| 临时 Decision | `PersistentInfoLayer/DecisionPanel` | 受布局控制器管理；直接拖动可能弹回 |
| 旧数据流 | `DataStreamView` | 受布局控制器管理；当前先保留，不作为 Human 阶段重点 |
| E 提示 | `InteractionLayer/PlayerInteractionPrompt` | 只移动父物体；不可改名 |
| Hold E | `InteractionLayer/HoldPrompt` | 移动整个 HoldPrompt；不要只移进度条 |
| 检查投影 | `ProjectionLayer/ProjectionPanel` | 动态内容区域 |

Companion 的以下对象由名称回退查找，不能改名：

- `V2_Identity`
- `V2_CurrentTask`
- `V2_REC`

`DecisionPanel` 和 `DataStreamView` 受 `HearthCompanionHudLayoutController` 的 ExecuteAlways 布局控制，手工拖动可能被重新写回。它们应改：

`Assets/Data/HearthHud/Companion/Hearth_CompanionHudLayout.asset`

或等 Codex 接回后改成明确的可编辑布局入口。

## 15. 终端的固定流程和对象名称

### 15.1 一楼同步终端

Prefab：

`Assets/Prefabs/UI/HearthHud/V2/Terminals/Terminal_Lobby_Assignment_V2.prefab`

核心对象：

- `TerminalContentRoot`
- `V2_PageVisual`
- `AssignmentTitle`
- `KeyboardNavigationRoot`
- `KeyboardHintText`
- `KeyboardFocusText`
- `RuntimePromptText`
- `RuntimeActionLabel`
- `V2_FooterRule`

设计意图：

- 聚焦实体电视。
- 不增加第二个全屏终端外框。
- 中间下方给正式对白预留空间。
- Current Task 下方、中右区域给 Field Unit 预留空间。
- 前 5 秒为 `PLEASE WAIT`，之后 Space 关闭。
- 没有终端内部 Tab 分页。

### 15.2 17F01/02 门口终端

共同核心对象：

- `TerminalContentRoot`
- `V2_PageVisual`
- `V2_ClosureTerminalChrome`
- `Tab_BEFORE_ACQUISITION`
- `Tab_AFTER_ACQUISITION`
- `Tab_REVIEW_ARCHIVED_EVENT`
- `PrimaryActionTab`
- `HouseholdIntroduction`
- `FieldUnitPanel`
- `ActionPanel`
- `KeyboardNavigationRoot`
- `Footer`

17F03 把右侧动作换为：

`Tab_ENTER_UNIT`

门口终端目标输入：

- Left/Right 切换顶部三个焦点。
- Space 执行。
- Esc 在允许退出时关闭。
- 不把旧的 6/8 页继续留在导航中。
- A/B 选择不放在终端内部。

这些顶部 Tab 当前主要由 `HearthTerminalCompactChromeView` 驱动，是状态视觉，不一定是 Unity Button。移动时移动整个 Tab 父物体，不要只移动 Label。

### 15.3 17F04 家庭终端

核心对象：

- `TerminalContentRoot`
- `V2_PageVisual`
- `HomePanel`
- `FieldUnitPanel`
- `Tab_ENTER_HOME`
- `PrimaryActionTab`
- `KeyboardNavigationRoot`
- `Footer`

右上动作必须一直保留 `ENTER HOME`；锁定时显示 `PLEASE WAIT`，不能把动作移动到隐藏页。

### 15.4 终端不可改名对象

所有终端中不要改名或删除：

- `TerminalContentRoot`
- `KeyboardNavigationRoot`
- `KeyboardHintText`
- `KeyboardFocusText`
- `RuntimePromptText`
- 可选的 `TerminalActiveLoop`
- `V2_ClosureTerminalChrome`
- `PrimaryActionTab`

终端控制器会按这些字段或固定路径恢复运行时引用。

不要移动 Scene 中终端根、MonitorCanvas、Terminal Camera 或实体电视模型来修 UI。只调整对应终端 Prefab 内部 RectTransform。

## 16. 哪些看起来像 UI，但不是可以点击的按钮

| 对象 | 是否可点击 | 实际输入来源 |
|---|---:|---|
| 身份、Current Task、Location | 否 | 纯显示 |
| Formal 对话框 | 否 | Space 由对白播放器读取 |
| Field Unit 辅助框 | 否 | Space 由对白播放器读取 |
| 动态 E 提示 | 否 | 世界中心射线 + E |
| Companion HoldPrompt | 否 | Hold E 逻辑 |
| 初始教程键帽 | 否 | 纯教程显示 |
| Human `Button_*` | 是 | Unity Button + HUD Action |
| Final Choice `Button_*` | 是 | Unity Button + HUD Action；另有键盘 Target |
| 终端 Tab | 主要是状态视觉 | 终端控制器读取 Left/Right/Space |
| Photo Archive 图片槽 | 否 | Left/Right/Space；世界 E 打开 |

判断一个对象是否真的可点：

1. 选中 GameObject。
2. 看 Inspector 是否有 `Button` 组件。
3. 如有 Button，移动这个 GameObject 的 RectTransform，点击命中区会一起移动。
4. 如只有 Image/TMP，它通常只是视觉。
5. 如另有 `FocusTarget`，键盘焦点还需要同步对应 Target。

## 17. 用户手工调整 Human UI 的推荐实操

### 阶段 A：只摆固定位置

1. 退出 Play Mode。
2. 双击打开 `HearthHudRoot_V2.prefab`。
3. Game View 设为 1920 × 1080。
4. 先调整左上身份三件套。
5. 调整右上 Current Task 三件套，并把文字设为右对齐。
6. 移动整个 `LocationHud`，让其左边缘与身份区一致。
7. 移动 `V2_InitialTutorialRoot`。
8. 移动整个 `PlayerInteractionPrompt`。
9. 保存 Prefab。

### 阶段 B：放图片

1. 把新 PNG 复制到 `Assets/UI/HEARTH/V2/VectorParts/`。
2. 完成 Sprite 导入设置。
3. 固定装饰 Image 的 `Raycast Target` 关闭。
4. 身份/任务下划线可使用 HeaderUnderline，或继续使用 2px Unity Image 规则线。
5. 教程 Keycap 使用 64×40 和 96×40 两类图片。
6. 菜单按钮边框作为 Button 根下的子 Image，不能单独放在 Button 外面。
7. 暂时不要给共享 Subtitle `Backdrop` 和 `SpeakerTab` 同时塞 Formal/Field Unit 两套图片。

### 阶段 C：调整菜单与点击区域

1. 移动 `Button_TODAY` 整个根。
2. 移动 `Button_DISPOSITION_HISTORY` 整个根。
3. 移动 `Button_SYSTEM_SETTINGS` 整个根。
4. 不移动 `MenuFocus`。
5. 进入 Play Mode 预览，确认鼠标和键盘高亮仍然落在按钮上。

### 阶段 D：记录结果

每完成一个区域，记录：

- Prefab 名。
- 对象完整路径。
- Anchor Min/Max。
- Pivot。
- Anchored Position。
- Width/Height。
- 使用的 Sprite。
- Image Type。
- Tint 和 Alpha。
- 是否有 Button。
- 是否同步移动了 Focus Target。

## 18. 只使用 Runtime Preview 验证

这些菜单只有在 Play Mode 中可用：

`Tools > Hearth > UI V2 > Runtime Preview`

Human：

- `Human/01 Persistent HUD`
- `Human/02 Field Unit Auxiliary`
- `Human/03 Formal Dialogue Left`
- `Human/04 Formal Dialogue Right`
- `Human/05 Tab Menu`
- `Human/06 Photo Archive`
- `Human/07 Disposition Choice`
- `Human/08 Shutdown Confirm`

Companion：

- `Companion/01 17F01`
- `Companion/02 17F02`
- `Companion/03 17F03`

终端：

- `Terminal/01 Lobby Synchronization`
- `Terminal/02 Doorway 17F01`
- `Terminal/03 Doorway 17F02`
- `Terminal/04 Doorway 17F03`
- `Terminal/05 Home 17F04`

停止：

- `Stop Preview`

正确循环：

1. 退出 Play Mode。
2. 修改 Prefab。
3. 保存。
4. 回到 SampleScene。
5. 进入 Play Mode。
6. 调用一个 Runtime Preview。
7. 观察 1920 × 1080 Game View。
8. 调用 Stop Preview。
9. 退出 Play Mode后再继续修改。

不要在 Play Mode 中调整并期待 Unity 自动保存。

## 19. 当前禁止操作

- 不运行 `Rebuild All V2 UI Assets`。
- 不运行 `Use V2 UI In Open Scene` 来刷新当前画面。
- 不运行旧 Human/Companion/Terminal Builder 覆盖手工结果。
- 不使用 `Overrides > Apply All`。
- 不改名 `HearthHudRoot` 或 `HearthCompanionHudRoot`。
- 不改名两个 `PlayerInteractionPrompt` 固定路径。
- 不删除动态 TMP。
- 不把正文、姓名、任务、地点烘焙进 PNG。
- 不给终端再套一个 `HUD_Terminal_FullscreenFrame`，实体电视已经是唯一外框。
- 不在 Human 阶段先调整 Companion 的 `DecisionPanel` 和 `DataStreamView`。
- 不直接拖共享 Subtitle 五个对象并认为位置会永久生效。

## 20. 用户与 Codex 的分工建议

### 用户现在最适合做

- Human 左上身份位置。
- Human 右上 Current Task 位置与右对齐。
- Location 对齐。
- 教程键帽、位置和大小。
- Human Tab 菜单按钮布局。
- 固定装饰图和半透明底板的视觉判断。
- 记录满意后的 RectTransform 数值。

### Codex 接回后负责

- 把 Formal 和 Auxiliary 拆成两套可编辑视觉根。
- 根据 Channel 和 SpeakerSide 自动切换边框、左右姓名签和显隐。
- 把新 PNG 正式导入并批量统一 Import Settings。
- 统一 Image Tint、Alpha、9-slice 和主题色。
- 保证动态 TMP、数据绑定和最长文本容量。
- 修正 Final Choice、Settings、终端键盘焦点和实际按钮命中区。
- 继续完成 Companion、五台终端、照片、Final Choice、Shutdown 和 Low Trust。
- 只调用 Runtime Preview 做逐页验证，不替用户跑完整流程。
- 保留用户的 Prefab 修改和 Scene Override，不重跑 Builder 覆盖。

## 21. 交还给 Codex 时需要提供什么

用户完成手调后，只需告诉 Codex：

1. 修改的是哪个 Prefab。
2. 哪些对象已经满意，不允许重排。
3. 哪些对象只是临时试摆。
4. 新 PNG 被复制到了什么 Unity 路径。
5. 哪些颜色暂时不满意，留给 Codex统一。
6. 是否有 Scene Override 尚未 Apply。
7. 每个改动前后的一张 1920 × 1080 截图。

Codex 接手时的固定顺序：

1. 读取当前 Prefab 和 Scene Override。
2. 把用户已确认的 RectTransform 视为最新布局真值。
3. 不重跑 Builder。
4. 先完成 Formal/Auxiliary 动态视觉拆分。
5. 再做图片绑定、颜色和输入焦点。
6. 使用 Runtime Preview 逐状态检查。
7. 更新 UI 结构记录；如修改脚本，同步更新 `脚本使用说明总表.md`。

## 22. 手工修改记录表

每次可以复制下面一行填写：

| 日期 | Prefab | 对象完整路径 | Anchor | Pivot | Pos X/Y | W/H | Sprite | Tint/Alpha | Button/Focus 联动 | 状态 |
|---|---|---|---|---|---|---|---|---|---|---|
|  |  |  |  |  |  |  |  |  |  |  |

建议状态只使用：

- `试摆`
- `用户确认`
- `待 Codex 动态绑定`
- `已进入 Runtime Preview`

## 23. Human 第一阶段完成标准

用户完成 Human 固定 UI 后，应达到：

- 左上身份、左下 Location 左边缘一致。
- Current Task 右对齐，不与 Lily/Field Unit 区域重叠。
- 初始教程在右下，不覆盖 Location 或对白。
- 动态 E 提示位于中下，不覆盖 Formal 对话正文。
- 三个 Tab 菜单按钮的可见区域、鼠标命中区和键盘焦点一致。
- 所有新图片透明、无白底、无烘焙文字。
- 固定装饰 Image 不阻挡 Raycast。
- 没有修改或删除任何动态绑定名称。
- Formal/Field Unit 暂时只确定位置，不强行把两套图片塞进共享 VisualRoot。

达到这些条件后，最适合把项目交还给 Codex 完成动态 UI 和其余系统。

## 24. 当前已完成后的手工微调入口（2026-07-30）

Codex 已继续完成动态层、终端和矢量组件接入。用户现在若只想做美术微调，应从下面对象开始，
不要再寻找旧 GeneratedParts 或旧 Builder 生成层。

### Human

- Prefab：`Assets/Prefabs/UI/HearthHud/V2/HearthHudRoot_V2.prefab`
- 固定 HUD：`PersistentHud`
- Field Unit：`V2_FieldUnitRoot`
- Tab：`V2_TabMenuRoot`
- Photo：`V2_PhotoArchiveRoot`
- Final/Shutdown/Low Trust：各页面自己的 `V2_*` 页面根
- 正式对白不在 Human Prefab 内，见 Subtitle Prefab。

### 正式对白

- Prefab：`Assets/Prefabs/UI/HearthSubtitle/V2/HearthSubtitleVisualCanvas_V2.prefab`
- 可移动根：`VisualRoot/FormalFrame`
- 左姓名签：`VisualRoot/SpeakerTabLeft`
- 右姓名签：`VisualRoot/SpeakerTabRight`
- Space 提示：`VisualRoot/AdvanceHint`
- Field Unit 独立框：`VisualRoot/AuxiliaryFrame`
- 移动 Frame 时要把对应姓名签和提示一起移动；不要只移动 TMP。

### Companion

- Prefab：
  `Assets/Prefabs/UI/HearthHud/V2/Companion/HearthCompanionHudRoot_V2.prefab`
- 全屏装饰：`CompanionRobotFrame`
- 左上身份：`V2_Identity`
- 顶部 REC：`V2_REC`
- 右上任务：`V2_CurrentTask`
- 左中监测：`V2_StatusPanel`
- 右上临时决策：`DecisionPanel`
- 中央边界提示：`CenterMessageText`
- 这些文字由 SceneData 动态写入；可改 RectTransform 和底图，不要把示例文字烘焙进 PNG。

### 五台终端

- 目录：`Assets/Prefabs/UI/HearthHud/V2/Terminals/`
- 顶部导航统一在 `V2_ClosureTerminalChrome`：
  `BeforeTab`、`AfterTab`、`PrimaryActionTab`、`Footer`。
- 内容区域继续位于各终端自己的 `V2_PageVisual`。
- 顶部三个 Tab 是状态焦点，并非必须是 Unity Button；移动父级 Tab，
  不要只移动 Label。
- 不要再添加全屏外框；屏幕外的实体电视模型就是唯一外框。

### 图片与半透明衬底

- 当前矢量 PNG：`Assets/UI/HEARTH/V2/VectorParts/`。
- PNG 只承担边框和固定装饰；深蓝黑/灰蓝半透明衬底仍由父级 Unity `Image` 提供。
- 最安全的层级顺序：
  1. 父级容器。
  2. 半透明底板 Image。
  3. 动态 TMP/照片。
  4. 最上层 Vector Frame Image。
- 底板与边框 Image 的 `Raycast Target` 关闭；真正的 Button/交互根保留 Raycast。

### 修改后只做预览检查

## 25. 历史 Profile 接管流程（2026-08-03，已停用）

> 本节仅保留用于解释旧项目结构。它已经被第 26 节“正式 Prefab 唯一入口”取代。
> 日常调整不要运行 `Apply Current Profiles`，也不要再把 Layout Profile 当成静态 UI 的主要编辑入口。

历史版本曾让以下两份资产同时驱动 Builder、Final Visual Repair 和运行时动态 UI：

- 字体、颜色、线宽与遮罩：`Assets/UI/HEARTH/V2/Profiles/Hearth_UiV2Theme.asset`
- 1920×1080 位置与尺寸：`Assets/UI/HEARTH/V2/Profiles/Hearth_UiV2Layout_1920x1080.asset`

该旧流程会重建视觉层，容易覆盖正式 Prefab 中的手动调整，因此入口已迁移到 `Tools > Hearth > Legacy / Unsafe`。只有排查旧版兼容问题时才允许使用，并且使用前必须备份当前 Prefab 外观。

### 25.1 字号字段对照

| Inspector 分组/字段 | 控制内容 | 当前值 |
|---|---|---:|
| Terminal Dialogue Typography / Speaker | 终端 `Field Unit` 名称 | 52 |
| Terminal Dialogue Typography / Body | 终端正式正文 | 26 |
| Terminal Dialogue Typography / Advance | `SPACE CONTINUE` | 26 |
| Companion Header Typography / Identity Heading | `COMPANION UNIT · ACTIVE` | 21 |
| Companion Header Typography / Identity Value | `UNIT 17F-01` | 32 |
| Companion Header Typography / Task Heading | `CURRENT TASK` | 19 |
| Companion Header Typography / Task Body | 任务正文 | 22 |
| Interaction Typography / Prompt | 短按 E 与 Hold E 主提示 | 22 |
| Finale Typography / Scene Card | 黑幕首次出现的场景卡 | 44 |
| Finale Typography / Epilogue Caption | 黑幕居中对白 | 36 |
| Finale Typography / Persistent Scene Header | 对白上方持续场景标题 | 24 |
| Overlay / Fullscreen Decision Dimmer Alpha | A/B 与 Final Response 全屏黑幕 | 0.82 |

普通全局人物名称、普通字幕和旧结尾字幕继续分别使用 `Speaker Font Size`、`Subtitle Font Size`、`Ending Subtitle Font Size`。修改字号时不要打开 TMP 的 Auto Size；本项目需要固定字号和可预测换行。

### 25.2 位置字段对照

Layout Profile 全部采用“左上角为原点”的 `Left / Top / Width / Height`。`Left` 增大向右，`Top` 增大向下。

| Region | 对应对象 |
|---|---|
| Companion Identity Heading / Value | Companion 左上两行，运行时对象名为 `V2_IdentityHeading`、`V2_IdentityValue` |
| Companion Task Heading / Body | Companion 右上两行，运行时对象名为 `V2_TaskHeading`、`V2_TaskBody` |
| Dynamic Interaction Prompt | Human/Companion 短按 E 提示基线 |
| Doorway Portrait One/Two/Three | 17F01–03 三张照片 |
| Doorway Portrait Labels | SON/DAD/MOM 等标签基线 |
| Doorway Introduction | `HOUSEHOLD INTRODUCTION` |
| Doorway Navigation | Before/After/Review 一行 |
| Doorway Field Unit | 门口终端共享底部消息区 |
| Terminal Message Lane | 同步终端与 17F04 终端共享底部消息区 |
| Photo Archive Field Unit / Page | TV4 下方 Field Unit 与 `PAGE 01 / 01` |
| Fullscreen Selection Dimmer | 终端处置、Final Response 全屏遮罩 |
| Epilogue Scene Card / Header | 黑幕首次场景卡与持续小标题 |

同步终端的现有框体大小不由本轮放大；只读取 Theme 中的三项终端字号。门口终端当前基线为照片 `180/440/700, 264, 240×400`，简介 `1016,246,828×468`，Field Unit `230,745,1460×248`。TV4 Field Unit 为 `320,790,1280×190`，页码为 `240,730`。

### 25.3 直接微调 TMP 的方法

1. 以下步骤属于历史参考；正式静态 UI 应按第 26 节直接在对应 Prefab Mode 中修改。
2. TMP 的 `Alignment` 决定文字锚向；Field Unit 名称与正文都使用左上，操作提示使用右下。
3. `Line Spacing` 只调正文的行距；建议每次改 2–4，避免一次跨太大。
4. 内边距由 TMP Rect 相对外框 Rect 决定。名称、正文左边缘必须共线；不要拉伸外框图片来制造边距。
5. 禁止打开 Auto Size；如果文字仍溢出，先增加对应 Region 高度或减少正文长度，再调整字号。
6. 应用 Profile 后依次运行 `Validate Open Scene UI`、`Validate Repaired Prefabs` 和 Runtime Preview 的 `QA/Audit Active Regions`。

### 25.4 哪些可以直接改，哪些不能改名

- 正式规则见第 26 节：静态 UI 的 Rect、单项字号、边距和对齐直接改权威 Prefab。
- Theme Profile 只管全局字体、颜色和公共视觉参数；Layout Profile 只保留动态安全区、TimeCard 等少量运行时区域。
- 运行时生成：缺失时补建的 `TerminalDialogueSurface_V2`、`TerminalMessageSurface_V2`、`PhotoArchiveCanvas_V2`、黑幕持续标题。不要只改 Scene 中临时实例。
- 绝对不能改名：`PlayerInteractionPrompt`、`HoldPrompt`、`HoldPromptText`、`HoldProgressFill`、`FieldUnitPanel`、`LilyMessagePanel`、上方四个 Companion 分离 TMP 名，以及终端 Before/After/Review 功能节点。
- 不允许在普通木门上恢复玩家直接 E 开关门；剧情仍可调用 `SmartDoorController.Open()`、`Close()`。

### 25.5 E／Hold E 与预览

短按和长按都采用 V1 功能结构；V2 只替换配色、边框和字体。Hold E 的根及内部六个 Rect 必须一起移动，不能只移动 `HoldPrompt` 根。

- 生成对照图：`Tools > Hearth > UI V2 > Reference > Capture E-Hold E Comparison`
- 输出目录：`Documentation/HEARTH_UI_V2_Reference`
- Play Mode 预览：`Tools > Hearth > UI V2 > Runtime Preview`
- 1920×1080：Runtime Preview 下 `QA/Resolution/1920x1080`

不要为了日常手调重新运行旧 Builder 或 `Apply Current Profiles`。需要长期保留的静态视觉必须写入第 26 节列出的权威 Prefab。
1. 退出 Play Mode 修改 Prefab。
2. 保存后进入 Play Mode。
3. 使用 `Tools > Hearth > UI V2 > Runtime Preview` 选择状态。
4. 使用 `QA > Audit Active Regions` 检查主要区域遮挡。
5. 满意后保存 1920×1080 截图。
6. 不运行旧 Builder，不执行 `Overrides > Apply All`。

最新总览图：

- `HEARTH_UI_V2_Baselines/MCP_Final/HEARTH_UI_V2_MCP_Final_ContactSheet.png`
- `HEARTH_UI_V2_Baselines/MCP_Responsive/HEARTH_UI_V2_MCP_Responsive_ContactSheet.png`

## 26. 正式 Prefab 唯一入口（2026-08-03，覆盖第 24–25 节旧流程）

### 26.1 为什么以前改了 Prefab，游戏里却没变化

旧结构同时存在五层控制：Prefab、SampleScene 覆盖、Layout/Theme Profile、Builder/Repair、运行时生成代码。场景中的 Human HUD 曾有大量视觉覆盖，所以 Scene 值会压过 Prefab；之后 Builder 或运行时代码又会再次压过两者。

现在的规则是：**位置、尺寸、单项字号和边距直接改正式 Prefab；运行时只写文字、显隐、焦点和进度。** `Apply Current Profiles` 因为会重建视觉，已移入 `Tools > Hearth > Legacy / Unsafe`，日常不要运行。

### 26.2 人类视角从哪里改

- 正式 Prefab：`Assets/Prefabs/UI/HearthHud/V2/HearthHudRoot_V2.prefab`
- 左上身份、右上任务、左下 Location：`PersistentHud`
- Current Task：标题 `Text_006_CURRENT_TASK`，正文 `V2_CurrentTaskBody`。改字体大小必须分别改两个 TMP。
- Tab 页面：各 `HearthFirstPersonHudPage` 子物体。按钮位置、可见图片、TMP 和 Button/焦点目标必须一起移动。
- 菜单/Final 选中底色：每个目标下的 `SelectionFill`。运行时只开关这个子物体，不再创建。
- Shutdown：正式 Prefab 中 Cancel 对象必须保持关闭；不要恢复 Esc 取消。
- 动态 E：`InteractionPromptLayer/PlayerInteractionPrompt`；由 `HearthInteractionPromptPresenter` 绑定。
- 正式/自然/黑幕字幕不在 Human Prefab，见 26.4。

### 26.3 陪伴单元视角从哪里改

- 正式 Prefab：`Assets/Prefabs/UI/HearthHud/V2/Companion/HearthCompanionHudRoot_V2.prefab`
- 左上：`V2_IdentityHeading`、`V2_IdentityValue`。
- 右上：`V2_TaskHeading`、`V2_TaskBody`。
- 短按 E：`PlayerInteractionPrompt`；Hold E：`HoldPrompt`、`HoldPromptText`、`HoldProgressFill`。
- 状态、Decision、TriggerCard 仍在同一个 Companion Prefab 内。可以调 Rect/TMP/Image，但不能另造第二个 HUD。
- 正式 Synth/Field Unit 出现时旧 DecisionPanel 会临时隐藏；不要用复制一套面板解决重叠。

### 26.4 字幕从哪里改

- 正式 Prefab：`Assets/Prefabs/UI/HearthSubtitle/V2/HearthSubtitleVisualCanvas_V2.prefab`
- `VisualRoot`：总布局；`Speaker` / `Body` / `AdvanceHint`：人物名、正文、Space。
- `FormalFrame`、`AuxiliaryFrame`、左右 Speaker Tab：正式框。
- `Backdrop`：自然字幕黑底。
- `PersistentSceneHeader`：黑幕对白上方持续场景标题。
- 播放器会根据正式/自然/终端/黑幕模式切换显隐和响应式高度，但禁止创建第二个 Subtitle Canvas。对白内容和 Line ID 不在 Prefab 中修改。

### 26.5 五个终端从哪里改

- 目录：`Assets/Prefabs/UI/HearthHud/V2/Terminals/`。
- 每台终端只改自己的正式 Prefab；不要在 SampleScene 临时改完就结束。
- Field Unit：`FieldUnitPanel`；17F04 Lily：`LilyMessagePanel` 或 `TerminalMessageSurface_V2`。
- 17F04 必须同时有 Field Surface 和 Lily Surface，二者互斥。
- 正式绑定后 Controller 不再用 Layout Profile 改 Rect/字号，也不再创建缺失面板。
- 如果你先在 Game View 对应 Scene 实例调出了目标外观，使用 Production UI 的 Adopt/Clear 流程反写，不要使用 `Apply All Overrides`。

### 26.6 TV4、17F03 和黑幕

- TV4 相册正式 Prefab：`Assets/Prefabs/UI/HearthHud/V2/PhotoArchive/HearthPhotoArchiveWorldView_V2.prefab`；只在首次安装时创建。改页码、Field 框和字体都在此 Prefab；Photo Camera/Renderer 仍留在场景。
- 17F03 检查正式 Prefab：`Assets/Prefabs/UI/HearthHud/V2/Inspection/Hearth17F03InspectionPanel_V2.prefab`；首次 Bind 会采用当前场景认可外观，此后不再由运行时布局代码覆盖。
- F02/F03 黑幕继续使用场景中已有 CanvasGroup，Production Scene Bind 会加统一 `HearthScreenTransitionService`。不要在 Controller 下再建黑幕 Canvas。

### 26.7 Theme、Layout 和任务文案各管什么

- `Hearth_UiV2Theme.asset`：全局字体、颜色和公共视觉参数；不要把单个面板位置放进去。
- `Hearth_UiV2Layout_1920x1080.asset`：只保留动态安全区、结局场景卡等确实必须在运行时计算的少量区域；不再作为 Human/Terminal/TV4 静态排版的覆盖源。
- `HearthTaskTextCatalog.asset`：Current Task 和 Companion Scene 任务文案。改任务文字在这里，不改 Controller switch；旧 switch 只是 Catalog 缺失时的回退。

### 26.8 推荐的手调与应用流程

1. 退出 Play Mode。
2. 首次迁移运行 `Tools > Hearth > Production UI > Install or Refresh Explicit Bindings`。
3. 打开 SampleScene，运行 `Bind Open Scene To Canonical Views`；检查变化后手动保存。
4. 直接进入上方指定的正式 Prefab 调 RectTransform、TMP、Image。
5. 若要保留当前 Scene 外观：先 `Compare Scene vs Prefab`，选中正式 Prefab 实例，执行 `Adopt Approved Appearance`，再 `Clear Visual Overrides`。
6. 运行 `Validate Production UI`。目标是所有正式绑定完整、Runtime Fallback 关闭、静态视觉覆盖为 0。
7. 进入 Play Mode，在 1920×1080 回放。Prefab 修改只有在退出 Play Mode 保存后才会永久生效。

### 26.9 绝对不要做

- 不运行 `Legacy / Unsafe` 下的 Builder/Repair，除非明确要做兼容恢复并已备份。
- 不对 Human/Companion/Terminal 使用 `Overrides > Apply All`。
- 不复制第二个 Human HUD、Companion HUD、Subtitle Canvas 或终端 Surface。

## 27. Prefab 打开后空白、F 无法定位的修复与新入口（2026-08-08）

### 27.1 这次空白的真实原因

这不是对象被删掉，也不是用户不会操作。旧 Builder 在创建顶层 UI 时，曾把部分正式
Prefab 的根 `RectTransform` 保存为 `Scale = 0,0,0`；部分场景实例恰好用 Override 补成
`1,1,1`，主字幕实例却还保留了旧的零缩放 Override。结果就是：

- 游戏中的场景实例还能显示；
- 双击正式 Prefab 后，内容全部压缩到零尺寸；
- Scene View 没有可计算的包围盒，所以选中后按 `F` 也没有反应；
- Companion、字幕、17F03 检查面板和终端还有 CanvasGroup/页面默认隐藏，进一步造成空白。

目前正式 Human、Companion、Subtitle、17F03 Inspection 和五台 Terminal Prefab 的根
缩放已经统一为 `1,1,1`，主场景字幕实例的旧零缩放 Override 也已撤销。字幕根补回了
明确的 `1920 × 1080` 编辑参考尺寸；终端在
Prefab Mode 中会显示一张起始页。进入 Play Mode 后，原控制器仍会在 `Awake/Start`
重新执行正式显隐，因此这项修复不改变剧情流程或对话时机。

### 27.2 最稳定的打开方式

退出 Play Mode 后，使用：

`Tools > Hearth > Production UI > Preview`

该菜单下可以直接打开：

- `Human HUD`
- `Companion HUD`
- `Subtitle`
- `17F03 Inspection`
- `Photo Archive`
- `Terminal - Lobby / 17F01 / 17F02 / 17F03 / 17F04`

菜单会进入真正的正式 Prefab，并自动选中主要编辑根、切换到 2D、执行 Frame Selected。
例如 Human 会选中 `PersistentHud`，Companion 会选中 `PersistentInfoLayer`，终端会选中
`TerminalContentRoot`。因此不需要先在巨大层级中猜哪个对象生效。

如果旧 Prefab 曾被其他 Builder 再次写成空白，运行：

`Tools > Hearth > Production UI > Repair Prefab Authoring Visibility`

这个命令只修正式 Prefab 的编辑缩放、编辑显隐和终端起始预览页，不保存场景、不修改
对白、音效、Camera、剧情引用、UnityEvent 或玩家控制。

### 27.3 Project 窗口双击仍然可以使用

也可以直接双击正式 Prefab。现在选中任意有 RectTransform 的可见子对象后按 `F` 应能
定位。若要调 Human Current Task，实际对象是：

- 标题：`PersistentHudLayer/PersistentHud/Text_006_CURRENT_TASK`
- 正文：`PersistentHudLayer/PersistentHud/V2_CurrentTaskBody`
- 下划线：`PersistentHudLayer/PersistentHud/V2_TaskUnderline`

`Text_007_NIGHT_ROUNDS___BLOCK_A___17F` 是迁移期旧文字，不再作为正式 Current Task
正文入口，不要继续修改它。

### 27.4 Runtime Preview 与 Prefab Preview 不同

`Tools > Hearth > UI V2 > Runtime Preview` 只在 Play Mode 中启用，用来验证真实控制器、
页面切换和输入逻辑；退出 Play Mode 时菜单呈灰色是正常的。

`Tools > Hearth > Production UI > Preview` 可在 Edit Mode 使用，用来调整正式 Prefab 的
RectTransform、TMP、Image 和层级。日常美术修改先使用 Production UI Preview，完成后
再进入 Play Mode 使用 Runtime Preview 或完整流程验收。

### 27.5 修改后如何让游戏看到结果

1. 退出 Play Mode。
2. 从 `Production UI > Preview` 打开目标正式 Prefab。
3. 修改并保存 Prefab；不要只改 `Canvas (Environment)`，它只是 Prefab Mode 的临时环境。
4. 返回 SampleScene。若该场景实例仍有旧视觉 Override，运行
   `Compare Scene vs Prefab`，确认后只清理视觉覆盖。若是旧字幕实例的 Scale 0，运行
   `Bind Open Scene To Canonical Views` 会只撤销这条零缩放覆盖。
5. 运行 `Validate Production UI`。
6. 进入 Play Mode，在 1920×1080 下检查对应流程。

不要使用 `Overrides > Apply All`，也不要通过缩放整个 Scene 根来修正一个文字的位置。
- 不改正式绑定对象名，不删除 Binding/Presenter，不把示例文字烘焙进图片。
- 不为了改 UI 保存整个 SampleScene 的无关 Camera、模型、锚点或剧情引用变化。

完整结构、Legacy 引用数和删除门槛见 `HEARTH_全项目保守重构基线与Legacy清理表.md`。
