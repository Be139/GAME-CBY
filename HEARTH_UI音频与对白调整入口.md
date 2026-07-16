# HEARTH UI、音频与对白调整入口

> 本文记录当前正式场景 `SampleScene` 的可维护入口。调整普通数值、文字、AudioClip 后通常不需要运行生成器；只有新增控制器、丢失引用或重建 HUD 后，才需要重新运行绑定菜单。

## 1. 米娅 Tab 菜单

- 运行时按 `Tab` 打开左上菜单，使用 `↑/↓` 移动高亮，`Space` 确认，`Esc` 或再次按 `Tab` 关闭。
- 菜单三项：今日巡查、处置历史/信任度、系统设置。
- 打开任一菜单页面时，人类移动、视角和普通交互都会锁定；关闭后恢复。
- 处置历史只显示已完成住户：0/1/2/3 户分别对应 Slides 18/19/20/21。
- `SHIFT TRUST DELTA` 是当前记录的增减总和；`CURRENT TRUST` 是 `TrustStateController` 的真实当前分数，不是百分制。

## 2. 系统音量

场景入口：

- `HearthHudRoot / HearthSettingsView`：设置页焦点、四个显示值和退出入口。
- `HearthHudRoot / HearthAudioSettingsController`：Master、Dialogue、Ambient、SFX 的默认值、运行值和 PlayerPrefs 持久化。

运行时：在设置页用 `↑/↓` 选项目，`←/→` 每次调整 5；四组数值会同步到 Slides 22/23/24。

音频归类：

- Master：直接控制 `AudioListener.volume`。
- Dialogue：已绑定三个正式 `MinLoopSubtitlePlayer` 的语音 AudioSource。
- SFX：已绑定正式人类/机器人脚步声，以及 HUD/TV 上已有的短音效 AudioSource。
- Ambient：已绑定 `MIN_LOOP_ROOT/Audio` 下 Corridor、Replay Night、Morning 三个 Ambience 占位音源；以后给其他环境 AudioSource 添加 `HearthAudioChannelSource`，把 `Channel` 设为 `Ambient` 即可接入。

## 3. 人类与机器人脚步声

### 人类 Mia

层级：`Player/Person Controller/First Person Audio`

组件：`HearthFootstepAudioProfile`

- `Role = Human`
- `Walk Clip` / `Run Clip`：替换人类脚步素材。
- `Walk Playback Speed`：当前 `0.82`，数值越小，当前录音中的步频越慢。
- `Run Playback Speed`：当前 `1.05`。
- `Walk Volume` / `Run Volume`：进入 SFX 总线前的基础音量。

实际播放源位于子物体 `Step Audio` 与 `Running Audio`。

### 陪伴单元 Robot

层级：`Player/Robot Controller/Robot First Person Audio`

组件：`HearthFootstepAudioProfile`

- `Role = Companion`
- 当前 `Walk Playback Speed = 1.00`、`Run Playback Speed = 1.30`。
- 后续把机器人专用金属/机械脚步分别拖到 `Walk Clip`、`Run Clip`，不会覆盖人类脚步。

### 当前素材限制

现有 `Steps` 是一段包含多个脚步的循环录音，所以 `Playback Speed` 会同时改变步频和音高。若以后要求“只改步频、不改音高”，应换成单个脚步 One Shot 素材，再把 `FirstPersonAudio` 升级成按距离/时间触发；当前接口已把人类和机器人素材分开，不需要再改剧情脚本。

## 4. 对白文字、句数、时间与语音

正式对白资产：

- 17F01-03：`Assets/Data/MinLoop/Dialogues/17F01_*.asset`、`17F02_*.asset`、`17F03_*.asset`
- 17F04：`Assets/Data/MinLoop/Dialogues/17F04/*.asset`

当前共有 34 个 Dialogue Sequence、215 个逐句语音槽位；目前尚未拖入真实语音。

选中任一 `HearthDialogueSequence` 后，可在专用 Inspector 中：

- 用 `+ / -` 增删句子。
- 拖动句子改变顺序。
- 修改 `Speaker`、`Subtitle Text`、`Delay Before Line`。
- 把录音拖到 `Voice AudioClip`。
- 设置 `Duration Mode`：
  - `VoiceClipWhenAssigned`：有语音时跟随语音长度；无语音时使用手动时长。推荐。
  - `ManualHold`：始终使用 `Manual Hold Seconds`。
  - `LongerOfVoiceAndManual`：取手动时长与语音时长中较长者。
- `Voice Tail Seconds`：语音结束后额外停顿多久再进入下一句。

增删句子、改文字、改时长或拖语音后直接保存资产即可，关卡会在下一次 Play 时读取；不需要重跑场景绑定菜单。流程会等待整段 Dialogue Sequence 播放完，再开放后续 E、切幕或选项。

## 5. 单按 E 与长按 E

- 单按 E 统一由正式人类/机器人上的 `PlayerInteraction` 显示，运行时强制使用英文 ASCII 提示，例如 `E  INTERACT`、`E  ACCESS TERMINAL`、`E  OPEN DOOR`。
- 场景中旧的中文序列化文字已经迁移为英文，因此不会再以方框显示。
- 长按 E 的 0-100% 进度条属于 `HearthCompanionHoldPrompt` / 关卡专用长按逻辑，本次没有改成单按。
- 新增单按交互时，让脚本实现 `IInteractable`，`GetDescription()` 返回英文；若阶段受限，同时实现 `IInteractionAvailability`。

## 6. 什么时候运行绑定菜单

菜单：`Tools / Hearth / Systems / Apply HUD Audio And English Prompt Fixes`

需要运行：

- 新建了正式人类/机器人控制器。
- 重建了 `HearthHudRoot`。
- 手动删除了音频配置、字幕 AudioSource 或设置页引用。
- 新场景要采用同一套系统。

不需要运行：

- 调整音量、脚步速度、音量或替换 AudioClip。
- 修改 Dialogue Sequence 的文字、句数、顺序、时长或语音。
- 调整已有 UI 的 RectTransform、字体大小、颜色。

验证菜单：`Tools / Hearth / Systems / Validate HUD Audio And English Prompts`。它会检查正式音频入口以及所有运行时 `IInteractable` 提示是否仍为英文。

## 7. 最小检查

1. Play 后按 Tab，确认人物停止移动。
2. 打开处置历史，确认只显示已完成住户和真实信任值。
3. 打开设置，把 Master 调低，确认数值与整体声音同时变化。
4. 分别操控 Mia 和机器人，确认两套脚步配置互不覆盖。
5. 给任意 Dialogue Sequence 一句拖入测试 AudioClip，确认字幕按所选 Duration Mode 推进。
6. 走近单按 E 交互物，确认提示是英文且无方框。

## 8. 分关卡剧情音效空槽

场景总入口：`MIN_LOOP_ROOT / Audio`。

现有三条 `Audio_Corridor_Ambience / Audio_Replay_Night_Ambience / Audio_Morning_Ambience` 继续负责持续环境声。新增剧情动作音效集中在：

- `StorySFX_17F02`：女主起身、女主移动、餐桌 Foley、调取记录、故障、关机。
- `StorySFX_17F03`：母亲起身、女儿起身、女儿移动、键盘输入、故障、深眠关机。
- `StorySFX_17F04`：相框记忆提示、最终关闭陪伴单元。

每个分组都挂 `HearthSfxCuePlayer`。展开 `Cues` 后，把选好的素材拖到对应 `Primary Clip`；需要多个随机版本时拖到 `Alternate Clips`。通常不需要修改子物体 AudioSource，也不需要重新运行菜单。

命名规则：

- `AUTO_*`：剧情脚本已经在准确阶段自动调用，拖入 Clip 后直接生效。
- `TBD_*`：发声位置已建立，但触发时刻尚未确认，不会自动播放。
- 当前唯一的 `TBD` 是 `TBD_17F02_Wife_SitOnBed`。需要先确认黑屏对白中的精确落座时刻，再调用 `StorySFX_17F02.PlayCue("Wife.SitOnBed")`。

移动音源：

- `AUTO_17F02_Wife_Walk` 会跟随 `Actor_Wife_17F02_BedroomRuntimeRoot`，门口停下时停止，继续出门时恢复。
- `AUTO_17F03_Daughter_Walk` 会跟随 `Actor_Daughter_17F03_RuntimeRoot`，到达最后路径点后停止。
- `HearthSfxCuePlayer / Follow Target` 是实际跟随对象；`Spatial Blend / Min Distance / Max Distance` 控制空间听感。
- 猫咪音效明确排除，`CatGuide` 下没有创建任何音效槽。

门音效仍使用门自身的 `SmartDoorController / Open Clip / Close Clip / Locked Clip`，不放进 StorySFX 重复播放。17F02 女主出门和 17F03 女儿开门已经补好 SFX AudioSource；拖入 Clip 即可。

应用菜单：`Tools / Hearth / Audio / Apply Story SFX Placeholder Setup`。只有新增/删除发声点、引用丢失或重建场景时才运行。普通替换 Clip 不运行。

验证菜单：`Tools / Hearth / Audio / Validate Story SFX Placeholder Setup`。空 Clip 是允许状态；验证重点是 Cue、AudioSource、空间跟随目标和剧情控制器绑定。

17F01 没有新增剧情 Foley 分组：排除小男孩呼吸、翻身和床单声后，该户由现有 Room Tone、机器人脚步、Companion HUD 长按完成声和 TV 终端音效槽覆盖，避免重复播放。

## 9. 17F04 猫咪动作频率

- 正式对象：`MIN_LOOP_ROOT / Finale_17F04 / CatGuide / CatMoveRoot`。
- 组件：`Hearth17F04CatGuideController / Walk Playback Speed`，当前为 `2.0`。
- 该值只加快 `Walk_F` 的腿部动作频率；`Run_F / Lie_to / Lie_idle` 保持原速。
- 路线时间仍为 `1.5 / 1.5 / 1.5 / 1.5 / 7.5 / 0.5 / 0.5` 秒。
- 第 4、5、6 个参考点在运行开始时直接读取当前 Transform。移动这些参考点后重新 Play 即可，不需要同步菜单。
