# HEARTH 音效资源需求与素材来源清单

更新日期：2026-07-16  
适用范围：17F01-17F04、米娅 HUD、TV 终端、陪伴单元 HUD、环境底噪、脚步与剧情动作音效。

## 1. 本次范围

本清单只负责游戏音效，不负责对白录音、角色配音或机器语音。

明确不纳入本次制作：

- 床单、被褥和床上布料摩擦声。
- 小男孩呼吸、梦中翻身和床单轻响。
- 所有猫咪叫声、呼噜、脚步、跳跃与落地声。
- 任何 stock 人声、争吵录音或对白替代音频。

需要保留：

- 女主坐到床上的动作音效，但只取身体落座、床架或软垫受力，不要床单摩擦。
- 女主从床上起身的动作音效。
- 门、人物脚步、机器人脚步、餐桌 Foley、UI、终端、数据扫描、故障、警告与关机声音。

用户提供的是 Pixabay 视频入口。音效应从 [Pixabay Sound Effects](https://pixabay.com/sound-effects/) 获取，不从视频中截取声音。

## 2. 选材原则

- 同一套 HUD/终端操作尽量使用同一个作者或同一声音包，避免每次点击都像来自不同游戏。
- UI 短音控制在约 `0.05-0.35s`；终端开机、数据扫描和故障可以更长。
- 环境底噪必须能自然循环，且不能包含清晰对白、宠物声或突出的单次事件。
- Foley 优先选干净、近距离、没有背景音乐的素材。
- 机器人脚步应轻于大型机甲。候选素材如果太重，只截取机械/伺服层并降低低频。
- Pixabay 候选只代表值得试听，不代表已最终选定。下载前仍需完整试听开头、结尾和背景噪声。
- 每个最终采用的素材都要保留来源、作者、下载日期和授权记录。

## 3. 当前 Unity 接口

### 米娅 HUD

`HearthFirstPersonHudController`：

- `Open Menu Clip`
- `Close Menu Clip`
- `Page Changed Clip`
- `Focus Moved Clip`
- `Confirm Clip`
- `Cancel Clip`
- `Warning Clip`
- `Trust Delta Clip`

### TV 终端

`HearthTvTerminalController`：

- `Open Clip`
- `Close Clip`
- `Boot Clip`
- `Page Switch Clip`
- `Focus Move Clip`
- `Submit Clip`
- `Replay Request Clip`
- `View Switch Clip`

### 陪伴单元 HUD

`HearthCompanionHudController`：

- `Scene Changed Clip`
- `Hold Completed Clip`
- `Special Effect Clip`

### 脚步、门与剧情节点

- 米娅脚步：`Player/Person Controller/First Person Audio/HearthFootstepAudioProfile`
- 机器人脚步：`Player/Robot Controller/Robot First Person Audio/HearthFootstepAudioProfile`
- 普通 E 交互：`InteractionFeedbackController / Feedback Clip`
- 门：`SmartDoorController / Open Clip / Close Clip / Locked Clip`
- 剧情动作音：`MinLoopStageCueController / Cue Clip`
- 阶段环境声：`MinLoopAudioStateController / Fallback Clip`

## 4. 优先级

| 优先级 | 内容 | 原因 |
| --- | --- | --- |
| P0 | UI 确认、长按完成、终端开机、门、脚步、故障、警告、关机 | 玩家操作和剧情反馈依赖这些声音 |
| P1 | 卧室/客厅 Room Tone、数据扫描、回放转场、键盘输入 | 建立空间感与科技系统质感 |
| P2 | 坐下、起身、餐具、相框记忆提示 | 丰富关键演出，但不应盖过对白字幕 |

## 5. 下载与剪辑说明

- 一个来源文件可以剪成多个游戏内短音，但每个导出文件都要保留同一来源记录。
- `Progress` 类素材通常比游戏长按时间长，应剪出约 `1.0-2.0s` 的稳定片段，再单独叠加完成提示。
- 门声应按动画拆成门把/锁舌、门轴移动、门关闭三段，不能只把一条长录音从头播放到底。
- 坐下与起身候选可能包含椅子或衣物声，只截取身体重心和家具受力部分；不要保留床单或持续布料摩擦。
- 长环境声进入 Unity 前先检查是否能无缝循环；必要时做交叉淡化。
- 不直接把网页 MP3 当最终母带。若来源只有 MP3，可用于预览；正式版本优先寻找 WAV 或自行重录。

## 6. 分关卡音效清单

### 6.1 通用候选音效库

以下每一种保留音效都提供至少 3 个直达试听链接。编号用于后面的关卡调用表。

#### SFX-01 夜间卧室 Room Tone

- [Bedroom Room Tone](https://pixabay.com/sound-effects/household-bedroom-room-tone-446021/)
- [Roomtone Bedroom Small Quiet](https://pixabay.com/sound-effects/household-roomtone-bedroom-small-quiet-6754/)
- [Room Tone Habitacion](https://pixabay.com/sound-effects/city-room-tone-habitaci%C3%B3n-51122/)

用途：17F01 儿童房、17F02 卧室、17F04 女儿房间。必须确认素材中没有呼吸、翻身、宠物或清晰人声。

#### SFX-02 公寓/客厅 Room Tone

- [Interior City Apartment](https://pixabay.com/sound-effects/city-001013-interior-city-apartment-53658/)
- [Apartment Living Room Tone](https://pixabay.com/sound-effects/household-room-tone-in-an-apartment-living-room-2-6176/)
- [Room Ambience](https://pixabay.com/sound-effects/household-room-ambiencewav-14849/)

用途：17F01 客厅、17F02 黑屏房外环境与餐桌、17F03/17F04 室内空间。优先选择没有可辨识电视和谈话内容的版本。

#### SFX-03 长按进度与 100% 完成

- [UI Processing Data Progress 01](https://pixabay.com/sound-effects/film-special-effects-ui-processing-data-sequence-modern-interface-progress-01-230496/)
- [UI Processing Data Progress 02](https://pixabay.com/sound-effects/film-special-effects-ui-processing-data-sequence-modern-interface-progress-02-230495/)
- [UI Loading End Success](https://pixabay.com/sound-effects/film-special-effects-ui-loading-end-success-522861/)

用途：第一至第三户长按 E、安抚完成、系统操作完成。前两条剪成进度层，第三条作为 100% 完成点。

#### SFX-04 UI 确认、提交与普通 E 交互

- [UI Beep Confirmation](https://pixabay.com/sound-effects/film-special-effects-ui-beep-confirmation-228332/)
- [Modern Success Confirmation Notification](https://pixabay.com/sound-effects/technology-modern-success-confirmation-notification-566072/)
- [Interface](https://pixabay.com/sound-effects/film-special-effects-interface-124464/)

用途：A/B 提交、Space 确认、普通 E 交互成功、终端提交。不要使用带真人说话的确认声。

#### SFX-05 回放、摄像机与数字转场

- [Futuristic Transition 3s](https://pixabay.com/sound-effects/technology-futuristic-transition-499653/)
- [Futuristic Transition 6s](https://pixabay.com/sound-effects/film-special-effects-futuristic-transition-390304/)
- [Cinematic Flashback Transition](https://pixabay.com/sound-effects/film-special-effects-cinematic-flashback-transition-463199/)

用途：进入/退出机器人回放、终端镜头移动、黑屏切幕。应跟 `0.5s` 视觉转场重新剪辑，不播放完整长尾。

#### SFX-06 数据扫描、记录调取与系统处理

- [Computer Processing](https://pixabay.com/sound-effects/technology-computer-processing-sound-effect-01-122131/)
- [Sensor Scan Small](https://pixabay.com/sound-effects/film-special-effects-ui-processing-data-continuous-sequence-sensor-scan-small-230491/)
- [Digital Data Beeps](https://pixabay.com/sound-effects/technology-digital-data-beeps-77587/)

用途：调取记录、机器人 HUD 数据刷新、扫描住户、读取当天回放。

#### SFX-07 木门开启、关闭、门把与锁舌

- [Door Opening and Closing](https://pixabay.com/sound-effects/household-door-opening-and-closing-18398/)
- [Opening Door](https://pixabay.com/sound-effects/film-special-effects-opening-door-450444/)
- [Old Wooden Door with Latch](https://pixabay.com/sound-effects/film-special-effects-old-wooden-door-with-a-latch-opening-and-closing-544539/)

用途：17F02 女主离开、17F03 第三幕、其他住宅门。建议从候选中分别剪出 `Handle/Latch`、`Open`、`Close`。

#### SFX-08 坐到床上/坐下

- [Sitting on Chair](https://pixabay.com/sound-effects/household-sitting-on-chair-43280/)
- [Sitting in Computer Chair](https://pixabay.com/sound-effects/household-sitting-in-computer-chair-gain-02-01-94009/)
- [Sitting in a Dress](https://pixabay.com/sound-effects/film-special-effects-044245-sitting-in-a-dresswav-64473/)

用途：17F02 女主坐到床上。这里只借用身体落座、家具受力或短促衣物动作，必须剪掉床单、被褥和持续布料声。

#### SFX-09 从床/座位起身

- [Standing Up from the Floor](https://pixabay.com/sound-effects/standing-up-from-the-floor-81281/)
- [Sit Down / Stand Up Foley](https://pixabay.com/sound-effects/sit-downstand-upx2-338300/)
- [Chair Sitting and Standing](https://pixabay.com/sound-effects/household-rolling-office-chair-sitting-standing-up-rolling-again-64831/)

用途：17F02 女主起身、17F03 人物起身。只剪取起身片段，并和动画脚落地时刻对齐。

#### SFX-10 人类室内脚步

- [Footsteps Walking Boots Parquet 1](https://pixabay.com/sound-effects/film-special-effects-footsteps-walking-boots-parquet-1-420135/)
- [Footsteps on Wood](https://pixabay.com/sound-effects/film-special-effects-footsteps-on-wood-397989/)
- [Footsteps Walking Boots Parquet 3](https://pixabay.com/sound-effects/film-special-effects-footsteps-walking-boots-parquet-3-420136/)

用途：女主/女儿剧情走位，也可作为米娅脚步的候选。实际落地时应拆成多个单步样本并随机播放。

#### SFX-11 陪伴单元机械脚步

- [Robot Walk](https://pixabay.com/sound-effects/film-special-effects-robot-walk-82499/)
- [Robot Step](https://pixabay.com/sound-effects/film-special-effects-robot-step-39326/)
- [Robot Footsteps Loop](https://pixabay.com/sound-effects/film-special-effects-gentoix27s-bon-footsteps-loop-walk-424387/)

用途：机器人自由移动。优先保留轻机械/伺服部分，不要做成大型机甲重踏。

#### SFX-12 餐桌餐具与桌面 Foley

- [Dishes Clink](https://pixabay.com/sound-effects/household-dishes-clink-189725/)
- [Plate and Mug on Table](https://pixabay.com/sound-effects/film-special-effects-plate-and-mug-on-table-64014/)
- [Fork on Ceramic Plate](https://pixabay.com/sound-effects/household-fork-striking-and-rubbing-a-ceramic-plate-375551/)

用途：17F02 餐桌段落。只放少量离散事件，不把餐具声连续铺满整个对白阶段。

#### SFX-13 维护键盘、密码与按钮

- [Single Keypad Beep](https://pixabay.com/sound-effects/film-special-effects-single-keypad-beep-433456/)
- [ATM Keypad Beep](https://pixabay.com/sound-effects/technology-atm-keypad-beep-481754/)
- [Numeric Keypad](https://pixabay.com/sound-effects/film-special-effects-the-numeric-keypad-393053/)

用途：17F03 `Entering_Code`、维护输入、机器人屏幕按钮。按键音和最终确认音应分开。

#### SFX-14 屏幕故障与数据损坏

- [TV Glitch](https://pixabay.com/sound-effects/technology-tv-glitch-6245/)
- [Computer Glitch 1](https://pixabay.com/sound-effects/technology-computer-glitch-1-34620/)
- [Computer Glitch Corrupted File](https://pixabay.com/sound-effects/film-special-effects-computer-glitch-corrupted-file-96176/)

用途：17F02 强制关闭前后、17F03 故障/深眠、终端闪烁。长素材只剪关键故障片段。

#### SFX-15 警告与低信任升级

- [Beep Warning](https://pixabay.com/sound-effects/film-special-effects-beep-warning-6387/)
- [UI Warning Alert Beep](https://pixabay.com/sound-effects/film-special-effects-ui-warning-alert-beep-534607/)
- [Error Beep Sound](https://pixabay.com/sound-effects/film-special-effects-error-beep-sound-361851/)

用途：17F03 警告链、17F04 低信任三段关闭。建议按低/中/高音高或密度排成三段，而不是把同一个声音简单放大三次。

#### SFX-16 机器人关机、断电与深眠

- [Robot Power Off](https://pixabay.com/sound-effects/film-special-effects-robot-power-off-97246/)
- [Power Off](https://pixabay.com/sound-effects/film-special-effects-power-off-386180/)
- [High-Tech Mechanical Power Off](https://pixabay.com/sound-effects/film-special-effects-high-tech-mechanical-power-off-194038/)

用途：17F02 强制关闭、17F03 深眠、17F04 最终关闭。可用一条电子关机加一条机械停止分层，但不要三条同时满音量叠加。

#### SFX-17 终端/陪伴单元开机

- [Computer Startup Sound Effect](https://pixabay.com/sound-effects/film-special-effects-computer-startup-sound-effect-312870/)
- [Robotic Creature Powering On](https://pixabay.com/sound-effects/film-special-effects-robotic-creature-powering-on-194040/)
- [VHS Startup](https://pixabay.com/sound-effects/film-special-effects-vhs-startup-6088/)

用途：TV 开机闪烁、陪伴单元唤醒、自宅终端。不要使用 Windows、Mac 或主机的真实品牌启动音。

#### SFX-18 相框与记忆提示

- [Cinematic Flashback Transition](https://pixabay.com/sound-effects/film-special-effects-cinematic-flashback-transition-463199/)
- [Flashback Sound](https://pixabay.com/sound-effects/film-special-effects-flashback-sound-6848/)
- [Remembrance Harp](https://pixabay.com/sound-effects/musical-remembrance-harp-72958/)

用途：17F04 进入/退出相框和记忆提示。优先选克制的短音，不要让它变成煽情背景音乐。

### 6.2 17F01 音效调用

| 剧情节点 | 使用类别 | 备注 |
| --- | --- | --- |
| 儿童房开场 | `SFX-01` | 只播放安静 Room Tone；不加小男孩呼吸、翻身或床单声 |
| 机器人在房间移动 | `SFX-11` | 跟随脚步系统，不做持续机械噪声 |
| 看向小男孩并长按安抚 | `SFX-03` | 进度层 + 100% 完成点 |
| 回放/数据页切换 | `SFX-05`、`SFX-06` | 转场一次，数据声保持低音量 |
| 客厅观察父母 | `SFX-02` | 只用空间底噪，不用 stock 人声 |
| 返回终端并提交 A/B | `SFX-05`、`SFX-04` | 提交只能响一次，避免重复结算造成连响 |

### 6.3 17F02 音效调用

| 剧情节点 | 使用类别 | 备注 |
| --- | --- | --- |
| 黑屏听见房外剧情 | `SFX-02` | 这里只配置环境底噪；对白声音不属于本清单 |
| 女主进卧室与关门 | `SFX-07` | 按门把、开门、关门时间点拆开 |
| 女主坐到床上 | `SFX-08` | 保留落座/床架受力；不使用床单或被褥声音 |
| 女主起身 | `SFX-09` | 与 `Sit_To_Stand` 动画重心上移和脚落地对齐 |
| 女主走向房门 | `SFX-10` | 按动画脚步事件播放，不播放整条循环录音 |
| 餐桌阶段 | `SFX-02`、`SFX-12` | Room Tone 常驻，餐具只在少数节点出现 |
| 调取记录/按钮操作 | `SFX-06`、`SFX-04` | 调取用数据声，确认用短 UI 声 |
| 强制关闭与黑屏 | `SFX-14`、`SFX-16` | 故障在前，关机在后，避免同时开始 |

### 6.4 17F03 音效调用

| 剧情节点 | 使用类别 | 备注 |
| --- | --- | --- |
| 米娅入户/检查实体机器人 | `SFX-02`、`SFX-05` | 环境底噪 + 0.5 秒镜头过渡 |
| 调取机器人记录 | `SFX-06` | 数据读取开始、完成可分别触发 |
| 机器人场景内移动或人物走位 | `SFX-11`、`SFX-10` | 机器人和人物脚步必须使用不同 Profile |
| 女儿起身 | `SFX-09` | 只在动作实际开始时播放 |
| 第三幕开门 | `SFX-07` | 先门把/锁舌，再门板旋转，避免声音领先画面 |
| 输入维护代码 | `SFX-13`、`SFX-04` | 每次输入短按键声，最终单独确认 |
| 故障与警告升级 | `SFX-14`、`SFX-15` | 故障纹理和警告节奏分层 |
| 核心服务关闭/深眠 | `SFX-16` | 关机后停止持续电子底噪 |
| 返回米娅并提交终端处置 | `SFX-05`、`SFX-04` | 返回转场后再播放提交声 |

### 6.5 17F04 音效调用

| 剧情节点 | 使用类别 | 备注 |
| --- | --- | --- |
| TV3 自宅终端开机 | `SFX-17` | 跟开机闪烁同步，素材剪到约 0.6-1.2 秒 |
| 进入客厅 | `SFX-05`、`SFX-02` | 转场结束后淡入客厅 Room Tone |
| 猫咪引导 | 无 | 本任务不配置任何猫咪声音 |
| TV4 相框进入/退出 | `SFX-18` | 进入和退出可用同音色的正放/反放变体 |
| 女儿房间 | `SFX-01` 或 `SFX-02` | 根据实景试听选择更自然的一条，不叠两条底噪 |
| A/B 选择提交 | `SFX-04` | 只在 Space 最终提交时播放，不在上下移动时重复用确认声 |
| 高信任关闭 | `SFX-04`、`SFX-16` | 一次确认后使用干净、平静的关机声 |
| 低信任关闭 | `SFX-15`、`SFX-16` | 病毒弹窗持续生成时使用短促警告；每次 Space 关闭窗口可播放轻量反馈，全部清空后只播放一次最终关机声 |
| 结局黑幕 | `SFX-02` 或静音 | 保持极低环境层，最后只留一个收束声；不使用对白替代素材 |

### 6.6 当前 Unity 空槽落地状态

- 已在 `MIN_LOOP_ROOT/Audio` 建立 `StorySFX_17F02 / StorySFX_17F03 / StorySFX_17F04`。
- 共 15 个剧情音效 Cue，每个 Cue 都有独立空 AudioSource、SFX 通道、空间范围和跟随目标；当前没有导入或绑定本清单候选素材。
- `AUTO_*` 发声点已经接入关卡流程，后续只需把选定素材拖入对应 `HearthSfxCuePlayer / Cues / Primary Clip`。
- `TBD_17F02_Wife_SitOnBed` 只完成发声位置和空槽。现有流程无法可靠识别黑屏对白中女主实际落座的精确时刻，所以暂不自动触发。
- 17F02/17F03 的人物走路音源会边移动边跟随 RuntimeRoot；门口停顿和路径结束时停止。
- 17F02、17F03 的剧情门继续使用各自 `SmartDoorController` 的 `Open Clip / Close Clip`，避免和 StorySFX 重复播放。
- 17F01 继续复用现有环境声、机器人脚步、长按完成声和 TV/HUD 音效字段；没有创建小男孩或床铺相关发声点。
- 猫咪没有任何音效槽。

应用/修复菜单：`Tools / Hearth / Audio / Apply Story SFX Placeholder Setup`。

验证菜单：`Tools / Hearth / Audio / Validate Story SFX Placeholder Setup`。

## 7. 其他可用音效网站

| 网站 | 适合内容 | 授权注意 |
| --- | --- | --- |
| [Pixabay Sound Effects](https://pixabay.com/sound-effects/) | 单条 Foley、门、脚步、故障、环境 | 适用 Pixabay Content License；可在作品中使用和修改，但不能把原素材独立再分发 |
| [Kenney Interface Sounds](https://kenney.nl/assets/interface-sounds) | 一整套统一 UI 声 | CC0，可商用且不要求署名 |
| [Mixkit Sound Effects](https://mixkit.co/free-sound-effects/) | 科技、UI、转场与生活音效 | 下载时保存对应 License 页面 |
| [Sonniss GDC Bundle](https://gdc.sonniss.com/gdc-game-audio-bundle/) | 高质量 WAV Foley、环境、机械与家居 | 可用于游戏和修改；不能把原声音作为素材包重新出售 |
| [Freesound](https://freesound.org/) | 冷门 Foley 和现场录音 | 每条授权不同；优先 CC0/CC BY，避开 CC BY-NC；CC BY 必须署名 |
| [ZapSplat](https://www.zapsplat.com/) | 大量 Foley 与家居音效 | 免费方案通常要求署名；下载前核对账户对应授权 |
| [OpenGameArt Audio](https://opengameart.org/art-search-advanced?field_art_type_tid%5B%5D=12) | 游戏 UI、环境与声音包 | 每个条目独立授权；优先 CC0 |

Pixabay 候选受 [Pixabay Content License](https://pixabay.com/service/license-summary/) 约束。下载时仍应保存素材页和授权页的日期证据。

## 8. 授权记录规范

每下载一个最终使用的素材，同时记录：

- 原始文件名。
- 游戏内重命名。
- 作者。
- 来源页面 URL。
- 下载日期。
- License 名称与授权页 URL。
- 是否要求署名。
- 使用关卡、剧情节点和 Inspector 字段。
- 是否经过剪辑、降噪、变调或分层。

建议建立 `Assets/Audio/HEARTH/Audio_Source_Register.csv`。即使素材不要求署名，也保留来源证据。

## 9. Unity 导入建议

- 短 UI、按键和 Foley：优先 WAV；`Load Type = Decompress On Load`。
- 长环境循环：使用可无缝循环的 WAV/OGG；长文件可用 `Streaming`。
- 世界中的门、脚步和人物动作声：Mono，使用 3D `AudioSource`。
- HUD、终端菜单与确认声：2D，`Spatial Blend = 0`。
- 环境音不要直接循环普通 MP3，编码首尾可能产生缝隙。
- 正式混音至少保留 `Master / Dialogue / Ambient / SFX / Human Footstep / Robot Footstep`。虽然本任务不制作对白，仍应保留 Dialogue 分组供后续配音接入。

## 10. 推荐执行顺序

1. 从每一类的 3 个候选中试听并标记首选/备选。
2. 先落地 `SFX-03/04/05/06/14/15/16/17`，完成 UI、终端和机器人系统反馈。
3. 替换 `SFX-10/11` 的人类与机器人脚步并校准步频。
4. 接入 `SFX-01/02` 三条最终 Room Tone，并做循环测试。
5. 按 17F02、17F03 动画时间补 `SFX-07/08/09/12/13`。
6. 最后接入 17F04 相框 `SFX-18`，做全流程响度回归。

## 11. 2026-07-19 全游戏音频接口落地表

当前场景已经建立 `45` 个剧情音效槽，全部位于 `MIN_LOOP_ROOT/Audio`。目前 `AudioClip` 仍为空，这是有意保留给最终试听素材的状态。

| 层级/组件 | 槽数量 | 负责内容 | 放音频的位置 |
| --- | ---: | --- | --- |
| `StorySFX_Global` | 9 | 单按 E、长按进度/完成、普通确认、黑屏、镜头平移、终端翻页/焦点/提交 | `HearthSfxCuePlayer > Cues > Primary Clip` |
| `StorySFX_Lobby` | 9 | 一楼大厅底噪、任务终端电流、领取确认、电梯按钮、关门、运行、到达、开门、17 楼走廊底噪 | 同上 |
| `StorySFX_17F01` | 6 | 儿童房/客厅底噪、回放转场、安抚完成、餐桌 Foley、机器人伺服 | 同上 |
| `StorySFX_17F02` | 7 | 坐床、起身、女主走路、餐桌、数据读取、故障、关机 | 同上 |
| `StorySFX_17F03` | 6 | 母亲起身、女儿起身/走路/键盘、故障、深眠关机 | 同上 |
| `StorySFX_17F04` | 8 | 客厅/女儿房底噪、相框、弹窗出现/关闭/升级/清空、最终关机 | 同上 |

### 11.1 人类与机器人脚步

- 人类：`Player/Person Controller/First Person Audio/HearthFootstepAudioProfile`。
- 机器人：`Player/Robot Controller/Robot First Person Audio/HearthFootstepAudioProfile`。
- 替换声音：拖到 `Walk Clip / Run Clip`。
- 调整步频：改 `Walk Playback Speed / Run Playback Speed`。数值越低越慢；该字段通过 AudioSource Pitch 改变当前多步录音的播放速度。
- 两套 Profile 独立，机器人不能复用人类脚步。若使用连续伺服声，放到相应关卡的 `Robot.ServoLoop`，不要塞进人类脚步槽。

### 11.2 环境循环

- 一楼、17 楼走廊及电梯：`StorySFX_Lobby`。
- 第一户房间：`StorySFX_17F01`。
- 通用最小循环还有 `MIN_LOOP_ROOT/.../MinLoopAudioStateController`，包含 `Audio_Corridor_Ambience`、`Audio_Replay_Night_Ambience`、`Audio_Morning_Ambience` 三个阶段循环源。
- 17F04 的 `Home.RoomTone` 与 `DaughterRoom.RoomTone` 先作为保留槽，等最终环境混音确认后再决定是否由流程自动启动，避免同一空间叠两条底噪。

### 11.3 终端与 HUD

- 每台 `HearthTvTerminalController` 可单独替换 `Open / Close / Boot / Page Switch / Focus Move / Submit / Replay Request / View Switch Clip`。
- 持续电流声使用 `Active Loop Cue Player / Active Loop Cue Id`。一楼任务终端已绑定 `StorySFX_Lobby / AssignmentTerminal.Hum`，终端打开时循环，退出时停止。
- 人类 HUD：`HearthHudRoot/HearthFirstPersonHudController` 的 Menu、Page、Focus、Confirm、Cancel、Warning、Trust Delta 音效字段。
- 机器人 HUD：`HearthCompanionHudRoot/HearthCompanionHudController` 的 Scene Changed、Hold Completed、Special Effect 字段。

### 11.4 门、人物动作与低信任弹窗

- 剧情门：对应 `SmartDoorController / Open Clip / Close Clip`，音源使用 3D SFX 通道。
- 人物动作 Foley：优先放到该关卡 `StorySFX_*` 的具体 Cue；精确脚落地以后可改成 Animation Event 调用同一 Cue。
- 低信任弹窗：`Popup.Spawn / Popup.Dismiss / Popup.WaveEscalate / Popup.Success` 已自动接入；全清空后再播放 `Unit.PowerOff`。
- 猫咪、小男孩呼吸/翻身、床单声音按已确认需求不创建、不播放。

### 11.5 对白语音

- 对白不放入 Story SFX。每句语音仍放在 `Assets/Data/MinLoop/Dialogues/` 对应 `HearthDialogueSequence > Lines > Voice Clip`。
- `Duration Mode` 保持 `Voice Clip When Assigned`：有语音时按 `AudioClip.length` 显示字幕，无语音时使用该句 `Hold Seconds`。
- 修改台词先改项目根目录正式稿，再运行 Dialogue Sync；只有 Speaker 与 Text 未变化的旧语音会自动保留。

场景空槽创建/修复：`Tools > Hearth > Audio > Apply Story SFX Placeholder Setup`。

完整验证：`Tools > Hearth > Audio > Validate Story SFX Placeholder Setup`。当前验收值应为 `45 slots / 0 clips`，直到你开始选择正式音效。
