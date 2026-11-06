# HEARTH 音效需求与素材直达清单

更新日期：2026-08-01

适用范围：17F01-17F04、一楼大厅、17 楼走廊、电梯、米娅 HUD、TV 终端、陪伴单元 HUD、人物与剧情动作。

快速打开：在项目文件夹中双击 `OPEN_HEARTH_SFX_LINKS.cmd`，即可把本清单转换为可点击导航页并交给 Windows 默认浏览器打开。

## 1. 使用规则

- 本清单按“实际可复用素材”整理，不按 Unity 播放槽逐个找。一个素材可以放进多个位置，但听感明显不同的声音不强行共用。
- 每类列出 5 个可直接试听或进入精准结果的候选。最终通常只选 `1` 个主素材，最多再留 `1` 个备选。
- 当前正式选择改为所有室内场景共用一条 `AMB-01` 主底噪：一楼大厅、17 楼走廊和 17F01–17F04 家庭房间先使用同一素材，后续只通过音量、EQ 和混响区分空间。`AMB-02`、`AMB-03` 仍保留为放置位置与未来替换搜索入口，不再代表必须单独导入两条素材。
- 一楼大厅采用两层环境声：通用 `AMB-01` 负责建筑空间底噪，`AMB-09` 负责旁边人群的不可辨识谈话；两层必须能独立调音量，不能把清晰对白烘焙进大厅底噪。
- UI 不再笼统只用一条点击声：普通导航/选项移动、A/B 决策焦点移动、单次 E 确认、A/B 提交、长按过程、长按完成、高危警告分别保留独立素材；其中跨关卡复用与特定剧情 UI 必须在文件名和映射表中标明。
- 仍然排除：床单/被褥摩擦、小男孩呼吸与翻身、猫咪声音、素材库人物对白或争吵录音。
- `直达单条` 表示打开后可直接试听该候选；`精准结果` 表示已经带好搜索方向，进入后仍要从结果中选一条。

## 1.1 本轮已导入结论

- 36 个已选素材已经以 MP3 形式复制到 `Assets/Audio/HEARTH/Imported/`；桌面原件保持不变。
- 正式文件名、原下载名、关卡位置、对象、触发动作、Unity 槽位状态和后续处理要求统一记录在 `HEARTH_音效导入映射.md`。
- `AMB01_GLOBAL_AllInteriors_RoomTone_Main_01.mp3` 是当前所有室内场景共用的主底噪；`AMB01_GLOBAL_AllInteriors_RoomTone_Alt_CityApartment_01.mp3` 只作为备选。
- `AMB09_LOBBY_WaitingArea_Walla_SmallGroup_01.mp3` 只用于一楼大厅人声层；不能扩展到住户房间或走廊。
- `FOL06_17F04_PATHA_EpilogueKitchen_FryingBed_01.mp3` 明确属于第 4 关 Path A 黑屏厨房结局，不属于第 2 关。
- 补充批次已经加入 17F02 的冰箱厨房底层、远处交通、门后淋浴与柔和爵士，17F04 Path B 的体育馆底噪，以及全局终端开机和 17F02–17F04 陪伴单元关机素材。
- 普通无效操作／门锁定反馈不再制作或寻找；现有高危警告和第 4 关错误弹窗仍保留原用途，不能挪作普通锁门提示。
- 导入不等于绑定：本轮没有把任何 AudioClip 写入场景、Prefab 或脚本字段。

## 2. 底噪与环境

| 编号 / 音效分类 | 音效放哪儿 | 要找什么 | 5 个网站直达 |
| --- | --- | --- | --- |
| `AMB-01` 全场景通用室内底噪 | 共用给 `Lobby.RoomTone`、`Corridor.RoomTone`、`Bedroom.RoomTone`、`LivingRoom.RoomTone`、`Home.RoomTone`、`DaughterRoom.RoomTone`，并作为 17F02/17F03 室内基础底层 | 当前主素材为 `AMB01_GLOBAL_AllInteriors_RoomTone_Main_01.mp3`，城市公寓长录音为备选；所有空间先复用同一素材，后续只改音量、EQ 和混响 | 1. [Pixabay：Apartment Living Room Room Tone（直达单条）](https://pixabay.com/sound-effects/household-room-tone-in-an-apartment-living-room-2-6176/)<br>2. [Freesound：Room Tone 2024 Pack](https://freesound.org/people/klankbeeld/packs/40249/)<br>3. [ZapSplat：Room Tone 精准标签](https://www.zapsplat.com/tags/room-tone/)<br>4. [Mixkit：Bedroom / quiet interior 候选](https://mixkit.co/free-sound-effects/bedroom/)<br>5. [99Sounds：World Sounds 室内录音包](https://99sounds.org/world-sounds/) |
| `AMB-02` 17 楼走廊放置位（共用 `AMB-01`） | `Corridor.RoomTone`；到达 17 楼后播放，进入家庭回放时停止或替换 | 本轮不再单独导入走廊底噪，直接使用 `AMB-01` 主素材；本行网站只作为未来确需独立走廊声时的替换候选 | 1. [Freesound：Quiet Corridor Room Tone（直达单条）](https://freesound.org/s/530750/)<br>2. [Pixabay：Empty Room Tone 4（直达单条）](https://pixabay.com/sound-effects/technology-005487-empty-room-tone-4-51110/)<br>3. [ZapSplat：Room Tone 标签，筛 hallway/corridor](https://www.zapsplat.com/tags/room-tone/)<br>4. [SoundJay：Indoor Ambient Sounds](https://www.soundjay.com/ambient-sounds.html)<br>5. [Sonniss：GDC Game Audio Bundle，检索 corridor/hallway](https://gdc.sonniss.com/gdc-game-audio-bundle/) |
| `AMB-03` 一楼大厅放置位（共用 `AMB-01`） | `Lobby.RoomTone`；开场简报、任务终端和大厅自由探索 | 本轮不再单独导入大厅建筑底噪，直接使用 `AMB-01` 主素材；本行网站只作为未来确需独立大厅声时的替换候选 | 1. [Pixabay：Ambient Hotel Lobby（直达单条）](https://pixabay.com/sound-effects/household-ambient-hotel-lobby-49668/)<br>2. [Freesound：Lobby ambience 精准结果](https://freesound.org/search/?q=quiet+modern+lobby+ambience)<br>3. [ZapSplat：Hotel Lobby Ambience 精准结果](https://www.zapsplat.com/?s=hotel+lobby+ambience&post_type=music)<br>4. [Mixkit：Ambience，试听 public interior 候选](https://mixkit.co/free-sound-effects/ambience/)<br>5. [Sonniss：GDC Bundle，检索 lobby/foyer/reception](https://gdc.sonniss.com/gdc-game-audio-bundle/) |
| `AMB-09` 一楼大厅极轻纯人声 / Walla | `Lobby.Walla`；任务终端周围或等候区，作为通用 `AMB-01` 上方很薄的一层 | 当前素材为 `AMB09_LOBBY_WaitingArea_Walla_SmallGroup_01.mp3`；只保留 2–4 名成年人低声、含混、听不清词句的纯人声，不扩展到走廊或住户房间 | 1. [Freesound：Indoor Adult Murmur, Couple，两人低声，0:19，CC0，无缝循环](https://freesound.org/people/SpliceSound/sounds/260124/)<br>2. [Freesound：Indoor Adult Murmur, Small Group，少量成年人，0:26，CC0，无缝循环](https://freesound.org/people/SpliceSound/sounds/260121/)<br>3. [Freesound：Murmur，只有低语与含混人声，0:49，CC0](https://freesound.org/people/Ben_0815/sounds/515798/)<br>4. [Bensound：People Group Talking Indoor Generic Walla Casual，1:46（付费/订阅候选）](https://www.bensound.com/sound-effects/people-group-talking-indoor-generic-walla-casual-01-crowd)<br>5. [Pond5：Quiet Murmuring，精准选择 Group Murmur / Quiet Walla / Soft Conversation / Light Whispering](https://www.pond5.com/search?kw=quiet-murmuring&media=sfx) |
| `AMB-04` 完整电梯录音 | `Elevator.DoorsClose`、`Elevator.Motor`、`Elevator.Arrival`、`Elevator.DoorsOpen`；按钮按下共用 `UI-01` | 当前已经导入开门、运行循环和到达提示；关门成品仍需优先从现有开门素材反向重剪，效果不自然时才另找，不扩大其他素材数量 | 1. [ZapSplat：Doors close → short ride → doors open（直达单条）](https://www.zapsplat.com/music/elevator-lift-doors-open-and-close-very-short-ride-then-doors-open/)<br>2. [Pixabay：Arrival Ding and Door Opening（直达单条）](https://pixabay.com/sound-effects/household-elevator-arrival-ding-and-door-opening-339161/)<br>3. [Freesound：Elevator Doors 1（直达单条）](https://freesound.org/s/456889/)<br>4. [Sonniss：Elevators 96kHz/24bit 专业包](https://sonniss.com/sound-effects/elevators-96khz-24-bit-stereo/)<br>5. [Pond5：Elevator 完整过程精准结果](https://www.pond5.com/search?kw=elevator&media=sfx) |
| `AMB-05` 冰箱嗡鸣 | 正式对白稿的“room tone, refrigerator hum”；17F02 Scene 2.5 黑屏争吵 | 当前素材为 `AMB05_17F02_BlackAudioArgument_KitchenFridgeRoomTone_SourceFull_01.mp3`；它已经混有厨房 RoomTone，后续应从中选稳定段，不能与 `AMB-01` 全量重复叠加 | 1. [Freesound：Residential Kitchen Refrigerator Hum，CC0（直达单条）](https://freesound.org/people/SpliceSound/sounds/338115/)<br>2. [Pixabay：Refrigerator Hum（直达单条）](https://pixabay.com/sound-effects/refrigerator-hum-58719/)<br>3. [ZapSplat：Refrigerator Hum（直达单条）](https://www.zapsplat.com/music/refrigerator-hum/)<br>4. [SoundJay：Appliance Sounds，试听 refrigerator](https://www.soundjay.com/appliance-sounds-2.html)<br>5. [Adobe Audition 免费素材：Household 包](https://www.adobe.com/products/audition/offers/audition-dlc.html) |
| `AMB-06` 室内听到的远处交通 | 正式对白稿的“distant traffic”；17F02 Scene 2.5 黑屏争吵 | 当前素材为 `AMB06_17F02_BlackAudioArgument_DistantCityTraffic_SourceFull_01.mp3`；原录音偏吵，后续必须低通、降音量并删除突发声，处理成隔窗／隔墙远声 | 1. [Pixabay：Traffic Heard from Apartment Interior（直达单条）](https://pixabay.com/sound-effects/005354-traffic-heard-from-apartment-interior-51212/)<br>2. [ZapSplat：Apartment Interior + Distant Traffic（直达单条）](https://www.zapsplat.com/music/apartment-interior-ambience-room-tone-hallway-open-door-distant-traffic/)<br>3. [Mixkit：Traffic，试听 City Traffic Background Ambience](https://mixkit.co/free-sound-effects/traffic/)<br>4. [Freesound：Traffic Through Closed Window 精准结果](https://freesound.org/search/?q=traffic+through+closed+window+interior)<br>5. [99Sounds：World Sounds，检索 urban/apartment](https://99sounds.org/world-sounds/) |
| `AMB-07` 浴室门后持续水声 | 正式对白稿“a door closes. Water, faint and steady”；17F02 Scene 2.4 | 当前素材为 `AMB07_17F02_BathroomBehindDoor_ShowerWater_SourceFull_01.mp3`；后续制作循环并通过低通、降音量和空间遮挡处理成门后淋浴声 | 1. [Pixabay：Water Running in Sink（直达单条）](https://pixabay.com/sound-effects/water-running-in-sink-391062/)<br>2. [Mixkit：Bathroom，试听 Shower Water / Sink Water Running](https://mixkit.co/free-sound-effects/bathroom/)<br>3. [ZapSplat：Metal Sink，Tap On → Steady Flow → Off（直达单条）](https://www.zapsplat.com/music/metal-sink-faucet-tap-on-run-off-steady-fast-flow/)<br>4. [Freesound：Shower in Next Room 精准结果](https://freesound.org/search/?q=shower+running+next+room+behind+door)<br>5. [SoundJay：Household Sounds，筛 faucet/shower](https://www.soundjay.com/household-sounds.html) |
| `AMB-08` 学校体育馆远处底噪 | 正式对白稿“a school gymnasium hum, distant”；17F04 Path B 学校开放日 | 当前素材为 `AMB08_17F04_PATHB_SchoolOpenHouse_DistantGymHum_SourceFull_01.mp3`；后续选稳定段并制作远、空、低存在感的循环 | 1. [Freesound：Empty School Gym Room Tone（直达单条）](https://freesound.org/s/192576/)<br>2. [ZapSplat：Indoor Sports Hall Warm-up Ambience（直达单条）](https://www.zapsplat.com/music/sports-hall-indoor-gymnasium-ambience-basketball-and-futsal-warmup-voices-balls-bounce-australia/)<br>3. [Pixabay：Empty Hockey Rink Ambience（直达单条）](https://pixabay.com/sound-effects/city-ambiencehockeyrinkempty-34361/)<br>4. [Mixkit：Sports，试听低人声的 Stadium/Indoor 候选](https://mixkit.co/free-sound-effects/sports/)<br>5. [Sonniss：GDC Bundle，检索 gym/sports hall](https://gdc.sonniss.com/gdc-game-audio-bundle/) |

## 3. UI 与交互

| 编号 / 音效分类 | 音效放哪儿 | 要找什么 | 5 个网站直达 |
| --- | --- | --- | --- |
| `UI-01` UI 轻点击 / 角色触屏 | 共用给 `UI.InteractSingle`、普通 UI 点击、终端翻页/焦点移动、角色点击屏幕、电梯按钮、女儿键盘、轻弹窗出现/消失 | 一次短、轻、干净的触控反馈，约 `0.08-0.25s`；不要鼠标实体机械声、硬币声或明显游戏得分感 | 1. [UI SFX：Press / Select / Focus，CC0](https://uisfx.com/)<br>2. [Kenney：UI Audio，50 个 CC0 点击](https://www.kenney.nl/assets/ui-audio)<br>3. [Mixkit：Click，优先 Modern Technology Select / Interface Device Click](https://mixkit.co/free-sound-effects/click/)<br>4. [ObsydianX：Interface SFX Pack 1，CC0 Cursor Tones](https://obsydianx.itch.io/interface-sfx-pack-1)<br>5. [Hove Audio：Free SCI-FI UI Pack，29 Clicks](https://hoveaudio.itch.io/free-sci-fi-ui-sound-effects-pack) |
| `UI-02` 确认 / 提交 | `UI.Confirm`、`Terminal.Submit`、`AssignmentTerminal.Confirm`；关键选择已被系统接受 | 比轻点击稍有落点的短确认，约 `0.15-0.45s`；可用同一素材的小变体，但不要胜利旋律、奖励金币或夸张升级声 | 1. [UI SFX：Check / Send / Success，CC0](https://uisfx.com/)<br>2. [Kenney：Interface Sounds，100 个 CC0 确认/按钮](https://kenney.nl/assets/interface-sounds)<br>3. [Mixkit：Interface，试听 Option Select / Confirmation](https://mixkit.co/free-sound-effects/interface/)<br>4. [ObsydianX：CC0 Confirm Tones，多套变体](https://obsydianx.itch.io/interface-sfx-pack-1)<br>5. [SoundJay：Button Sounds 2，筛干净短确认](https://www.soundjay.com/button-sounds-2.html) |
| `UI-03` 长按进行中 | `UI.HoldProgress`、HUD 长按环和持续操作过程 | 旧候选全部作废。新目标是 `1.0-1.6s` 的安静循环脉冲/处理中底层，能持续但不催促；不能用单次 Whoosh、倒计时或完成旋律 | 1. [UI SFX：Loading / Processing / Progress Step，CC0，可直接逐条试听](https://uisfx.com/)<br>2. [Eterion RELIC：Charge / System Processing 候选](https://eterionaudio.com/)<br>3. [Resonex UI Sounds Digital：Data Processing / Loop 候选](https://resonex-audio.itch.io/ui-sounds-digital)<br>4. [BOOM Modern UI：Computer Processing Noises / Reverse Counters](https://www.boomlibrary.com/sound-effects/modern-ui/)<br>5. [Hove Audio：Terminal UI Lite，筛持续数据处理声](https://hoveaudio.itch.io/terminal-user-interface-sound-effects-pack-lite-edition) |
| `UI-04` 长按完成 / 操作完成 | `UI.HoldComplete`、`Interaction.ComfortConfirmed`、`Popup.Success`、陪伴单元 `Hold Completed Clip` | 旧“Loading End / Level Complete”候选作废。新目标是 `0.3-0.7s` 的克制完成点：短上行或两段式落定；不要关卡胜利、金币、掌声和长尾旋律 | 1. [UI SFX：Complete / Success，CC0，可直接试听](https://uisfx.com/)<br>2. [Freesound：Powerup/Success，0.9s，CC0（直达单条）](https://freesound.org/people/GabrielAraujo/sounds/242501/)<br>3. [Pixabay：UI Complete（直达单条）](https://pixabay.com/sound-effects/film-special-effects-ui-complete-243461/)<br>4. [Kenney：Interface Sounds，CC0，筛 confirmation/complete](https://kenney.nl/assets/interface-sounds)<br>5. [Mixkit：Correct，试听短促 Correct Positive Notification](https://mixkit.co/free-sound-effects/correct/) |
| `UI-05` 柔和通知 | 正式对白稿“soft notification tone in Mia's earpiece”；普通非紧急信息到达 | 柔和、单次、无压迫感，约 `0.25-0.6s`；比确认声更像“新信息到达”，但不能像手机短信或门铃 | 1. [UI SFX：Notification / Receive / Info，CC0](https://uisfx.com/)<br>2. [Mixkit：Notification，筛 Interface Hint / Soft Alert](https://mixkit.co/free-sound-effects/notification/)<br>3. [Kenney：Interface Sounds，CC0，筛 notification](https://kenney.nl/assets/interface-sounds)<br>4. [Hove Audio：Free SCI-FI UI Pack，12 Rings](https://hoveaudio.itch.io/free-sci-fi-ui-sound-effects-pack)<br>5. [Eterion RELIC：Notification Soft / UI Message](https://eterionaudio.com/) |
| `UI-06` 高危警告 / 关机弹窗错误 | `Popup.Spawn`、`Popup.WaveEscalate`、HUD `Warning Clip` 与低信任关机挑战 | 保留当前 `UI06_GLOBAL_HighRiskWarning_01.mp3` 和 `UI06_17F04_ShutdownPopup_SpawnError_01.mp3`。普通无效操作和门锁定反馈不制作，也不把高危警告挪作普通锁门提示 | 1. [UI SFX：Warning / Error / Blocked，CC0](https://uisfx.com/)<br>2. [ObsydianX：CC0 Error Tones](https://obsydianx.itch.io/interface-sfx-pack-1)<br>3. [Mixkit：Alerts，筛 Game Quick Warning / Negative Tone](https://mixkit.co/free-sound-effects/alerts/)<br>4. [Hove Audio：Free SCI-FI UI Pack，Glitches / Impacts](https://hoveaudio.itch.io/free-sci-fi-ui-sound-effects-pack)<br>5. [SoundJay：Beep Sounds 2，筛短错误提示](https://www.soundjay.com/beep-sounds-2.html) |

## 4. 数字系统

| 编号 / 音效分类 | 音效放哪儿 | 要找什么 | 5 个网站直达 |
| --- | --- | --- | --- |
| `SYS-01` 数字转场 / 扫描 / 记忆提示 | `Transition.Blackout`、`Transition.CameraMove`、`Replay.SceneTransition`、`System.DataScan`、`Photo.Memory` | 找一条有“数字掠过/扫描纹理”的母素材，剪成短转场、持续扫描和记忆提示；允许反放、截取和轻微变调，但不要电影预告片重击 | 1. [Pixabay：Futuristic Transition（直达单条）](https://pixabay.com/sound-effects/technology-futuristic-transition-499653/)<br>2. [Mixkit：Transition，筛 Technology Slide / Futuristic Sweep](https://mixkit.co/free-sound-effects/transition/)<br>3. [99Sounds：Sci-Fi Sounds 免费包](https://99sounds.org/sci-fi-sounds/)<br>4. [Hove Audio：Free SCI-FI UI Pack，bleeps/impacts](https://hoveaudio.itch.io/free-sci-fi-ui-sound-effects-pack)<br>5. [BOOM Modern UI：Holograms / Noise Sweeps / Digital Hits](https://www.boomlibrary.com/sound-effects/modern-ui/) |
| `SYS-02` 屏幕故障 / 数据损坏 | 所有关卡的 `System.Glitch`、画面干扰、数据异常 | 一条短数字破碎/电噪即可跨关卡复用；不同严重程度用长度、重复和失真量区分。不要持续白噪盖住对白 | 1. [Pixabay：TV Glitch（直达单条）](https://pixabay.com/sound-effects/technology-tv-glitch-6245/)<br>2. [Mixkit：Glitch，筛 Small Electric / Digital Signal Interference](https://mixkit.co/free-sound-effects/glitch/)<br>3. [Hove Audio：Free SCI-FI UI Pack，31 Glitches](https://hoveaudio.itch.io/free-sci-fi-ui-sound-effects-pack)<br>4. [ZapSplat：Glitch Sounds 分类](https://www.zapsplat.com/sound-effect-category/glitches/)<br>5. [99Sounds：Sci-Fi Sounds，筛 glitch/static](https://99sounds.org/sci-fi-sounds/) |
| `SYS-03` 设备开机 / 关机 | 终端 `Boot Clip`、17F02/03 `System.PowerOff`、17F04 `Unit.PowerOff` | 已分别导入 `SYS03_GLOBAL_AllTerminals_DevicePowerOn_01.mp3` 与 `SYS03_SHARED_17F02_17F03_17F04_Companion_PowerOff_01.mp3`；终端共用开机声，三关陪伴单元共用关机声 | 1. [Pixabay：Robot Power Off（直达单条）](https://pixabay.com/sound-effects/film-special-effects-robot-power-off-97246/)<br>2. [Mixkit：Technology，试听 Electronics Power Up / Machine Activation](https://mixkit.co/free-sound-effects/technology/)<br>3. [Resonex UI Sounds Digital：Power On / Off Transitions](https://resonex-audio.itch.io/ui-sounds-digital)<br>4. [Hove Audio：Terminal UI Lite，筛 boot/shutdown](https://hoveaudio.itch.io/terminal-user-interface-sound-effects-pack-lite-edition)<br>5. [BOOM Mechanicals：motors / mechanisms / power movement](https://www.boomlibrary.com/sound-effects/mechanicals/) |

## 5. 人物、物件与剧情动作

| 编号 / 音效分类 | 音效放哪儿 | 要找什么 | 5 个网站直达 |
| --- | --- | --- | --- |
| `FOL-01` 住宅门 | `SmartDoorController`、卧室门、浴室门、住户门、最终前门；所有普通住宅门共用 | 一条完整室内木门录音，切出开门与关门；最好包含门把/锁舌，但不要古堡吱呀、金属舱门或猛烈摔门 | 1. [Pixabay：Door Opening and Closing（直达单条）](https://pixabay.com/sound-effects/household-door-opening-and-closing-18398/)<br>2. [Mixkit：Doors，筛普通木门 open/close](https://mixkit.co/free-sound-effects/doors/)<br>3. [SoundJay：Door Sounds 1](https://www.soundjay.com/door-sounds-1.html)<br>4. [FilmCow：Free SFX，筛 residential door](https://filmcow.itch.io/filmcow-sfx)<br>5. [Sonniss：GDC Bundle，检索 interior wood door](https://gdc.sonniss.com/gdc-game-audio-bundle/) |
| `FOL-02` 人类室内脚步 | 米娅、人类角色 `Wife.Walk`、`Daughter.Walk`；同类硬地板脚步共用 | 项目已有 `Assets/Mini First Person Controller/Audio/Steps.wav`，当前先由米娅、Claire 和 Ava 共用；不同人物只改音量和轻微音高，不再额外下载一套 | 1. [Nox Essentials：免费 Foley / Footsteps 包](https://nox-sound-design.itch.io/essentials-series-sfx-nox-sound)<br>2. [Mixkit：Footsteps，筛 indoor walk](https://mixkit.co/free-sound-effects/footsteps/)<br>3. [Pixabay：Boots on Parquet（直达单条，试听后减重）](https://pixabay.com/sound-effects/film-special-effects-footsteps-walking-boots-parquet-1-420135/)<br>4. [FilmCow：Free SFX，筛 footsteps](https://filmcow.itch.io/filmcow-sfx)<br>5. [Sonniss：GDC Bundle，检索 indoor footsteps wood/tile](https://gdc.sonniss.com/gdc-game-audio-bundle/) |
| `FOL-03` 坐下 / 起身 / 床架受力 | `Wife.SitOnBed`、`Wife.StandUp`、`Mother.StandUp`、`Daughter.StandUp`；17F01 床边动作 | 同一段人物重心与家具受力录音可切成坐下、起身；只保留身体落座、床架/软垫受力和轻衣物瞬态，删除床单、被褥和持续布料摩擦 | 1. [Pixabay 精准结果：选择 Sit Down / Stand Up x2](https://pixabay.com/sound-effects/search/sit-down/)<br>2. [ZapSplat：Person Sitting on Bed Mattress Creak（直达单条）](https://www.zapsplat.com/music/person-sitting-on-bed-mattress-creak-4/)<br>3. [Freesound：Sit Down + Stand Up Foley 精准结果，筛 CC0/CC BY](https://freesound.org/search/?q=sit+down+stand+up+foley)<br>4. [Adobe Audition 免费素材：Foley / Human Elements 包](https://www.adobe.com/products/audition/offers/audition-dlc.html)<br>5. [Sonniss：GDC Bundle，检索 sit/stand/bed creak](https://gdc.sonniss.com/gdc-game-audio-bundle/) |
| `FOL-04` 小型履带机器人移动 | 机器人第一人称移动、`Robot.ServoLoop`、陪伴单元起步/停止/原地转向；不再使用双足机器人脚步 | 用三层合成：①短而稳定的 RC 小电机/履带循环作为主体；②塑料或橡胶轮接触地面的细小颗粒；③只在起步、停下和转向时加入短伺服。删掉柴油发动机、坦克低频、重金属链条和液压冲击；同一母素材做慢行、正常、转向三种速度即可 | 1. [ZapSplat：Small Robot Tracks，30 条小型履带/轮组素材包](https://www.zapsplat.com/sound-effect-packs/small-robot-tracks/)<br>2. [Pro Sound Effects：Servo Robot Toy Mini Tank Tredder，2:06 WAV（付费直达）](https://www.prosoundeffects.com/sound-effects/PSE_SMPH-RL/VrgfE/Servo-Robot-Toy-Mini-Tank-Tredder)<br>3. [Freesound：Designer’s Choice TOYS，选择 RC Car Motor，CC0](https://freesound.org/people/designerschoice/packs/43695/)<br>4. [Pixabay：Remote Controlled Toy Car，0:45（直达单条）](https://pixabay.com/sound-effects/film-special-effects-driving-sounds-of-remoted-controlled-toy-car-29656/)<br>5. [Mixkit：Car，选择 Toy Car Plastic Wheels 作为地面接触层](https://mixkit.co/free-sound-effects/car/) |
| `FOL-05` 餐桌餐具 | `Parent.TableFoley`、`Dining.TableFoley`；家庭谈话中的克制桌面动作 | 盘子、碗、杯或餐具的一两次轻碰，不要洗碗、餐厅人群、连续摆桌或陶瓷摔碎 | 1. [Pixabay 精准结果：选择 Dishes Clink](https://pixabay.com/sound-effects/search/tableware/)<br>2. [Mixkit：Kitchen，试听 Glasses Clinking / Bowl Placed on Table](https://mixkit.co/free-sound-effects/kitchen/)<br>3. [SoundJay：Kitchen Sounds 2](https://www.soundjay.com/kitchen-sounds-2.html)<br>4. [ZapSplat：Metal Fork Movement in Bowl（直达单条）](https://www.zapsplat.com/music/metal-fork-movement-in-bowl-3/)<br>5. [FilmCow：Free SFX，筛 dishes/cutlery/table](https://filmcow.itch.io/filmcow-sfx) |
| `FOL-06` 锅铲碰锅 | 正式对白稿“a spatula against a pan”；17F04 Path A 早晨厨房记忆 | 当前已有 `FOL06_17F04_PATHA_EpilogueKitchen_FryingBed_01.mp3` 煎炒底声；先检查能否从中切出一两次锅铲触锅，切不出来时才补单次素材 | 1. [Pixabay：Plastic Spatula on Non-Stick Pan（直达单条）](https://pixabay.com/sound-effects/household-050588-plastic-spatula-on-non-stick-panwav-38063/)<br>2. [Mixkit：Cooking，筛 pan close / pot hit / frying](https://mixkit.co/free-sound-effects/cooking/)<br>3. [ZapSplat：Spatula + Frying Pan 精准结果](https://www.zapsplat.com/?s=spatula+frying+pan&post_type=music)<br>4. [Freesound：Spatula on Pan 精准结果，筛 CC0/CC BY](https://freesound.org/search/?q=spatula+on+pan+foley)<br>5. [Adobe Audition 免费素材：Foley / Household 包](https://www.adobe.com/products/audition/offers/audition-dlc.html) |
| `FOL-07` 钥匙落桌 | 正式对白稿“Keys landing on a table”；17F04 日间记忆 | 一串金属钥匙轻落在木桌，需有金属短响和木面落点；不要掉在地砖、钥匙插锁或长时间翻找 | 1. [ZapSplat：Alum Key Drop on Table（直达单条）](https://www.zapsplat.com/music/alum-key-drop-on-table/)<br>2. [Mixkit：Drop，试听 Dropping Keys](https://mixkit.co/free-sound-effects/drop/)<br>3. [Pixabay：Keys on Wooden Table 精准结果](https://pixabay.com/sound-effects/search/keys%20wooden%20table/)<br>4. [FilmCow：Free SFX，筛 keys/drop/table](https://filmcow.itch.io/filmcow-sfx)<br>5. [Sonniss：GDC Bundle，检索 keys wood table](https://gdc.sonniss.com/gdc-game-audio-bundle/) |
| `FOL-08` 远雷 | 正式对白稿“Thunder, far away”；女儿房夜间记忆 | 单次远雷或低沉滚雷，无雨或只有极弱雨底；不要近雷爆点、恐怖冲击和连续暴风雨 | 1. [Pixabay：Dry Thunder / No Rain（直达单条）](https://pixabay.com/sound-effects/nature-dry-thunder-364468/)<br>2. [Mixkit：Thunder，试听 Distant Thunder Explosion / Rumble](https://mixkit.co/free-sound-effects/thunder/)<br>3. [ZapSplat：Isolated Thunder Rumble, No Rain（直达单条）](https://www.zapsplat.com/music/a-rumble-of-thunder-isolated-no-rain-1/)<br>4. [99Sounds：Rain and Thunder 免费包](https://99sounds.org/rain-and-thunder/)<br>5. [Helton Yan：Weather Elements 游戏音效包](https://heltonyan.itch.io/weatherelements) |
| `FOL-09` 行李箱滚轮 / 门槛 | 正式对白稿“suitcase wheels over the doorstep”；17F04 最终前厅记忆 | 小型拉杆箱在硬地板短距离滚动，并有一次过门槛的轻撞；优先从一条完整录音切滚动和门槛两段，不再另找撞击声 | 1. [Pixabay：Roll-Aboard Suitcase on Hard Surface（直达单条）](https://pixabay.com/sound-effects/roll-aboard-suitcase-rolling-on-hard-surface-63927/)<br>2. [ZapSplat：Small Carry-On Suitcase Roll（直达单条）](https://www.zapsplat.com/music/small-carry-on-sized-suitcase-on-wheels-roll-a-short-distance-on-ground-4/)<br>3. [Freesound：Rolling Suitcase + Threshold 精准结果，筛 CC0/CC BY](https://freesound.org/search/?q=rolling+suitcase+threshold)<br>4. [FilmCow：Free SFX，筛 luggage/wheels](https://filmcow.itch.io/filmcow-sfx)<br>5. [Sonniss：GDC Bundle，检索 suitcase/luggage wheels](https://gdc.sonniss.com/gdc-game-audio-bundle/) |

## 6. 音乐

| 编号 / 音效分类 | 音效放哪儿 | 要找什么 | 5 个网站直达 |
| --- | --- | --- | --- |
| `MUS-01` 柔和爵士 | 正式对白稿“soft jazz, low”；17F02 Scene 2.2 `BedroomComfort` 的低音量室内音乐 | 当前素材为 `MUS01_17F02_BedroomComfort_SoftJazz_SourceFull_01.mp3`；后续核验授权、检查循环并在离开卧室或转入晚餐段落时淡出 | 1. [Pixabay：No Copyright Jazz Lounge（直达单曲）](https://pixabay.com/music/modern-jazz-no-copyright-jazz-lounge-541629/)<br>2. [Incompetech：Backbay Lounge，CC BY 4.0（直达单曲）](https://incompetech.com/music/royalty-free/index.html?Search=Search&isrc=USUAN1700068)<br>3. [OpenGameArt：Jazz，CC0（直达单曲）](https://opengameart.org/content/jazz-1)<br>4. [Free Music Archive：Jazz 精准结果，最终只选 Public Domain / CC BY / CC0](https://freemusicarchive.org/search/?search-genre=Jazz)<br>5. [itch.io：Jazz Groove Game Starter Pack，游戏用循环版本](https://serenofm.itch.io/jazz-groove-starter-music-pack) |

## 7. 下载与落地检查

1. 先试听每类 5 个候选，只确定一个主素材；不要把 5 个都下载进 Unity。
2. 环境声检查首尾循环、清晰人声、宠物声和突发事件；UI 检查是否过响、过长或像奖励音。
3. 下载时记录：素材名、作者、页面链接、下载日期、许可、是否需要署名、是否允许商业游戏和修改。
4. Freesound 精准结果只选 `CC0` 或允许商业使用的 `CC BY`；排除 `CC BY-NC`。ZapSplat 免费账户素材通常需要署名，下载时保存当日授权页。
5. Pixabay 音乐额外检查 Content ID；Free Music Archive 必须逐曲确认许可。Mixkit 音效可继续使用，但本清单没有采用 Mixkit 音乐作为游戏候选。
6. 正式母带优先 `WAV 48kHz / 24bit`；网页只有 MP3 时先用于试听，再寻找 WAV、购买正式版本或自行重录。
7. Unity 导入后：环境循环开启 Loop；短 UI 保持 2D；门、脚步、餐具、机器人履带和行李箱使用 3D；所有剧情 Foley 必须低于对白与字幕提示。大厅 `Lobby.Walla` 应与 `Lobby.RoomTone` 分轨控制；它只用于补充“有人在附近”的感觉，正常游玩时不应主动吸引注意。


## 导入映射与制作记录（合并原音效导入映射）

以下保留原映射的素材路径和日期状态，实际绑定需在修改对应音源时核对；不把历史缺项自动列为当前任务。程序接入方式统一见脚本使用说明总表.md 第 5 节。

# HEARTH 音效导入映射

更新日期：2026-08-02

正式目录：`Assets/Audio/HEARTH/Imported/`

原件目录：`E:\桌面\音效`、`E:\桌面\音效\新音效`

## 1. 状态说明

- 当前共导入 `36` 个 MP3，分为 `Ambience 10 / UI 10 / System 5 / Foley 10 / Music 1`。
- 桌面原件只读使用；没有移动、重命名、覆盖或转码原件。
- 原本就是 MP3 的 5 个文件采用二进制原样复制；M4A、OGG、WAV 使用 FFmpeg `libmp3lame -q:a 2` 转为 MP3。
- 本表逐行“Unity 状态”保留 2026-08-01 导入时快照；2026-08-02 当前正式状态以第 9 节为准。
- 2026-08-02 已完成中央 Catalog、剧情 Cue、终端/HUD/脚步/门、环境循环和结局逐行绑定；没有切音、覆盖、重编码或移动原件。
- 文件名中的 `GLOBAL` 表示全游戏同类操作复用；`SHARED_...` 表示只在列出的关卡间复用；直接写 `LOBBY / 17F02 / 17F04_PATHA` 表示特定剧情位置。
- 当前文件名只能追踪到下载文件名；素材许可、商业使用范围和署名要求仍需依据各下载页面或购买记录单独保存与核验。

## 2. 环境音

| 原始文件 | 原格式 / 时长 | 正式文件 | 使用范围与具体位置 | Unity 状态 | 后续处理 |
| --- | --- | --- | --- | --- | --- |
| `xomxomski-ambient-empty-room-noise-sound-effect-429845.mp3` | MP3 / 59.690s | `Ambience/AMB01_GLOBAL_AllInteriors_RoomTone_Main_01.mp3` | 所有室内场景共用：一楼大厅、17 楼走廊、17F01–17F04 家庭室内 | `Lobby.RoomTone`、`Corridor.RoomTone`、17F01/17F04 部分 RoomTone 槽已存在；尚未绑定 Clip，17F02/17F03 仍需后续接入 | 检查循环点；整体降音量；按场景调整 EQ/混响 |
| `freesound_community-001013_interior-city-apartment-53658.mp3` | MP3 / 817.896s | `Ambience/AMB01_GLOBAL_AllInteriors_RoomTone_Alt_CityApartment_01.mp3` | 通用室内底噪备选，不默认与主底噪同时播放 | 可复用上述 RoomTone 槽；尚未绑定 | 从长录音中选稳定片段、制作循环并检查突发声 |
| `Freesound - Indoor adult murmur, small group.wav by SpliceSound.mp3` | MP3 / 26.482s | `Ambience/AMB09_LOBBY_WaitingArea_Walla_SmallGroup_01.mp3` | 仅一楼大厅等候区和任务终端附近；作为室内底噪上方的极薄纯人声层 | 当前没有正式 `Lobby.Walla` 播放槽 | 循环检查；降低音量；对白时 Duck；保持与 RoomTone 分轨 |
| `Hotel Elevator Door Open ~ Sound Clip Royalty Free #54110277.m4a` | M4A/AAC / 3.692s | `Ambience/AMB04_LOBBY_ElevatorRide_DoorsOpen_01.mp3` | Scene 1.2，抵达 17 楼后电梯开门 | `Elevator.DoorsOpen` 槽已存在；尚未绑定 | 按模型动画裁切起点和尾部 |
| `电梯运行 06 可循环 ~ 库存音效 #79504362 --- Elevator Movement 06 Loopable ~ Stock Sound Effect #79504362.m4a` | M4A/AAC / 28.148s | `Ambience/AMB04_LOBBY_ElevatorRide_MotorLoop_01.mp3` | Scene 1.2，从一楼到 17 楼的电梯运行阶段 | `Elevator.Motor` 循环槽已存在；尚未绑定 | 检查无缝循环、低频和对白遮蔽 |
| `电梯铃声 ~ 库存音效 #169522078 - Pond5 --- Elevator Bell ~ Stock Sound Effect #169522078.m4a` | M4A/AAC / 5.000s | `Ambience/AMB04_LOBBY_ElevatorRide_ArrivalChime_01.mp3` | Scene 1.2，抵达 17 楼提示音 | `Elevator.Arrival` 槽已存在；尚未绑定 | 检查是否包含多个铃声或过长尾音 |
| `Indoors ambience, kitchen room tone with refrigerator humming in the background ~ Clip #308942389.m4a` | M4A/AAC / 76.600s | `Ambience/AMB05_17F02_BlackAudioArgument_KitchenFridgeRoomTone_SourceFull_01.mp3` | 17F02 Scene 2.5 黑屏争吵；提供厨房空间与冰箱压缩机嗡鸣 | 正式对白稿有环境声指示，当前没有独立运行时槽 | 素材已包含厨房 RoomTone，使用时不要再全量叠加 `AMB01`；选稳定段、检查循环并压低至对白下方 |
| `Noisy City Traffic ~ Sound Effect Royalty Free #162301640.m4a` | M4A/AAC / 21.077s | `Ambience/AMB06_17F02_BlackAudioArgument_DistantCityTraffic_SourceFull_01.mp3` | 17F02 Scene 2.5 黑屏争吵；模拟住宅室内听到的远处城市交通 | 正式对白稿有环境声指示，当前没有独立运行时槽 | 原素材名称标明 Noisy；后续低通、降音量并删除喇叭等突发声，处理成隔窗远声 |
| `Shower ~ Stock Sound Effect #276091262.m4a` | M4A/AAC / 22.895s | `Ambience/AMB07_17F02_BathroomBehindDoor_ShowerWater_SourceFull_01.mp3` | 17F02 Scene 2.4，Claire 进入浴室并关门后持续播放的门后水声 | 正式对白稿有环境声指示，当前没有独立运行时槽 | 制作循环；低通、降音量并加遮挡感，不能像近距离淋浴录音 |
| `Roomtone,Athletic Centre,Gymnasium,Hum ~ Sound Effect #4842392.m4a` | M4A/AAC / 39.093s | `Ambience/AMB08_17F04_PATHB_SchoolOpenHouse_DistantGymHum_SourceFull_01.mp3` | 17F04 Path B 学校开放日黑屏段落的远处体育馆底噪 | 正式对白稿有环境声指示，当前没有独立运行时槽 | 选稳定段、检查循环；保持远、空、低存在感，避免盖住 Lily 与陪伴单元对白 |

## 3. UI 音效

| 原始文件 | 原格式 / 时长 | 正式文件 | 使用范围与具体位置 | Unity 状态 | 后续处理 |
| --- | --- | --- | --- | --- | --- |
| `UI Hover.m4a` | M4A/AAC / 1.042s | `UI/UI01_GLOBAL_NavigationAndOption_FocusMove_01.mp3` | 全游戏导航栏、普通菜单选项和终端页面的焦点移动 | Human HUD `focusMovedClip` 与终端 `focusMoveClip` 已有；尚未绑定 | 试听尾音；必要时裁短以适应快速连续导航 |
| `切换要确认点击的目标时候的切换声.m4a` | M4A/AAC / 2.189s | `UI/UI01_GLOBAL_DecisionAB_FocusMove_01.mp3` | 17F01–17F04 的 A/B 决策目标之间切换；不用于普通导航 | 当前脚本与普通导航共用 Focus Clip，没有独立的决策焦点槽 | 后续增加或拆分决策焦点槽；检查快速切换时的叠音 |
| `高科技用户界面菜单关闭 (172) ~ 音效 #258077080 --- High Tech UI Menu Close (172) ~ Sound Effect #258077080.m4a` | M4A/AAC / 1.005s | `UI/UI01_GLOBAL_HumanHUDAndTerminal_Close_01.mp3` | 米娅 HUD、任务终端和住户终端关闭 | Human HUD `closeMenuClip`、终端 `closeClip` 已存在；尚未绑定 | 检查是否需要裁短或与打开音配对 |
| `UI 按E完成按钮后反馈音.m4a` | M4A/AAC / 0.907s | `UI/UI02_GLOBAL_SinglePressE_ActionConfirm_01.mp3` | 普通单次 E 操作被接受：常规交互、终端动作、机器人普通按钮 | `UI.InteractSingle` 占位槽与多个 `InteractionFeedbackController` 已存在；尚未绑定 | 与 A/B 提交声保持区分；检查高频操作节奏 |
| `点击按钮 确认选择AB选项的按下反馈.m4a` | M4A/AAC / 0.484s | `UI/UI02_GLOBAL_DecisionAB_SubmitConfirm_01.mp3` | 17F01–17F04 所有 A/B 处置或最终选择正式提交 | `UI.Confirm`、Human HUD `confirmClip`、终端 `submitClip` 已存在；尚未绑定 | 后续避免普通提交和剧情 A/B 提交误用同一槽 |
| `长按EProgressing ~ 片段 #236282073 --- Futuristic Digital Hud Screen Text Typing ~ Clip #236282073.m4a` | M4A/AAC / 3.062s | `UI/UI03_GLOBAL_HoldE_ActionProgress_Loop_01.mp3` | 17F01、17F02、17F03 陪伴单元 HUD 长按过程 | `UI.HoldProgress` 占位槽存在，但 Hold 控件当前没有完整的开始/停止播放接线 | 后续裁成适合约 1.5s Hold 的循环并接入取消/完成停止逻辑 |
| `UI 长按E完成按钮后反馈音.m4a` | M4A/AAC / 0.626s | `UI/UI04_GLOBAL_HoldE_ActionComplete_01.mp3` | 上述长按达到 100% 时播放 | `UI.HoldComplete` 与 Companion HUD `holdCompletedClip` 已存在；尚未绑定 | 检查是否需要与 17F01 安抚完成、其他成功音分开 |
| `Alien Warning Signal 01 ~ Stock Sound Effect #153428050.m4a` | M4A/AAC / 3.935s | `UI/UI06_GLOBAL_HighRiskWarning_01.mp3` | 全游戏严重风险、高危警告；包括第 4 关关机警告升级，不用于普通错误 | Human HUD `warningClip`、17F04 `Popup.WaveEscalate` 可接入；尚未绑定 | 可能过长；后续裁出短版并限制重复播放 |
| `通知铃声.m4a` | M4A/AAC / 1.071s | `UI/UI05_LOBBY_LilyMessage_EarpieceNotification_01.mp3` | 一楼大厅 Scene 1.1，Mia 耳机收到 Lily 留言时播放一次 | 正式对白稿有 SFX 指示，当前没有独立剧情音效槽 | 后续按 Lily 消息出现时刻接入 |
| `Sound Effect- Error Interface 14 (Reverb) ~ #219024179.m4a` | M4A/AAC / 1.333s | `UI/UI06_17F04_ShutdownPopup_SpawnError_01.mp3` | 第 4 关关机挑战，每个错误弹窗进入屏幕时播放 | `Popup.Spawn` 槽已存在；尚未绑定 | 密集弹窗可能叠音；后续限制并发、音量或最短播放间隔 |

## 4. 数字系统音效

| 原始文件 | 原格式 / 时长 | 正式文件 | 使用范围与具体位置 | Unity 状态 | 后续处理 |
| --- | --- | --- | --- | --- | --- |
| `人类和机器视角切换过场音效.m4a` | M4A/AAC / 3.603s | `System/SYS01_SHARED_17F01_17F03_HumanCompanion_ViewSwitch_01.mp3` | 17F01–17F03，从 Mia/终端进入陪伴单元第一人称回放 | 终端 `viewSwitchClip` 已存在；尚未绑定 | 与实际相机转场时长对齐；评估进入/退出是否共用 |
| `人类摄像机切换到终端摄像机视角 mixkit-fast-sci-fi-transition-sweep-3114.wav` | WAV/PCM / 1.208s | `System/SYS01_GLOBAL_HumanTerminal_CameraTransition_01.mp3` | 一楼任务终端、走廊终端、17F04 家庭终端进入/退出固定相机 | 终端有 Open/Close Clip，但相机过渡组件没有独立专用音效槽 | 后续决定接在终端开关还是相机过渡事件，并与动画时长对齐 |
| `机器人故障Sound Effect- Video Glitch 01 ~ Download #200781471.m4a` | M4A/AAC / 1.491s | `System/SYS02_SHARED_17F02_17F03_Companion_ShutdownGlitch_01.mp3` | 17F02 强制关机画面故障与 17F03 Deep Sleep 故障阶段复用 | 两关均已有 `System.Glitch` 槽；尚未绑定 | 检查是否需要为两关做长度或失真强度变体 |
| `设备开机 Sound Clip #339011585.m4a` | M4A/AAC / 4.000s | `System/SYS03_GLOBAL_AllTerminals_DevicePowerOn_01.mp3` | 一楼任务终端、17 楼住户终端及 17F04 家庭终端启动；所有同类终端共用 | `HearthTvTerminalController.bootClip` 已存在；尚未绑定 | 与终端画面启动时长对齐；必要时从 4 秒母素材裁出短版 |
| `power off.mp3` | MP3 / 2.351s | `System/SYS03_SHARED_17F02_17F03_17F04_Companion_PowerOff_01.mp3` | 17F02 强制关机、17F03 Deep Sleep、17F04 最终关机共用 | 17F02/17F03 `System.PowerOff` 与 17F04 `Unit.PowerOff` 自动槽已存在；尚未绑定 | 与画面断电点对齐；检查三关是否需要不同尾音或音量 |

## 5. 音乐

| 原始文件 | 原格式 / 时长 | 正式文件 | 使用范围与具体位置 | Unity 状态 | 后续处理 |
| --- | --- | --- | --- | --- | --- |
| `softjazz.mp3` | MP3 / 10.423s | `Music/MUS01_17F02_BedroomComfort_SoftJazz_SourceFull_01.mp3` | 17F02 Scene 2.2 `BedroomComfort`，Claire 同意播放平时使用的爵士歌单后低音量播放 | 正式对白稿有 SFX 指示，当前没有独立音乐播放槽 | 检查授权记录、循环点和首尾；低音量播放，并在离开卧室或进入下一段时淡出 |

## 6. 通用与跨关卡 Foley

| 原始文件 | 原格式 / 时长 | 正式文件 | 使用范围与具体位置 | Unity 状态 | 后续处理 |
| --- | --- | --- | --- | --- | --- |
| `开门和关门 ~ 音效 #236930832 --- Opening and Closing Door ~ Sound Effect #236930832.m4a` | M4A/AAC / 5.944s | `Foley/FOL01_GLOBAL_ResidentialDoor_OpenClose_SourceFull_01.mp3` | 所有普通住宅门、卧室门、浴室门和最终前门 | `SmartDoorController.openClip/closeClip` 已存在；尚未绑定 | 分割 Open/Close；按门动画裁切；保留 Path B 最终关门用途 |
| `Sitting Down and Standing Up from Sofa-Couch ~ Sound Effect #41498656.m4a` | M4A/AAC / 18.375s | `Foley/FOL03_SHARED_17F02_17F03_CharacterSitStand_SourceFull_01.mp3` | 17F02 Claire 起身；17F03 母亲、女儿起身 | `Wife.StandUp`、`Mother.StandUp`、`Daughter.StandUp` 槽已存在；尚未绑定 | 分割坐下/起身及多个 Take，删除无关持续摩擦 |
| `Servo Robot Toy Mini Tank Tredder - Pro Sound Effects.ogg` | OGG/Vorbis / 126.629s | `Foley/FOL04_SHARED_17F01_17F03_CompanionTrackedMovement_SourceFull_01.mp3` | 17F01–17F03 玩家控制的小型履带陪伴单元：移动、起步、停止、原地转向 | 目前只有 17F01 `Robot.ServoLoop` 占位槽；跨三关移动接入尚未完成 | 分割不同段落，制作正常/慢速/转向循环，删除大型机械感片段 |
| `Sound Effect- Fork Movement, Eating Utensil on Plate Movement, Constant Movements ~ #116200967.m4a` | M4A/AAC / 22.005s | `Foley/FOL05_SHARED_17F01_17F02_ParentDining_TableFoley_SourceFull_01.mp3` | 17F01 父母餐桌背景动作；17F02 Scene 2.3 Dinner 餐具动作 | 17F01 `Parent.TableFoley` 为占位槽；17F02 `Dining.TableFoley` 已有自动触发槽；尚未绑定 | 分割一两次克制餐具碰触，避免连续动作盖住对白 |

## 7. 单关卡剧情 Foley

| 原始文件 | 原格式 / 时长 | 正式文件 | 使用范围与具体位置 | Unity 状态 | 后续处理 |
| --- | --- | --- | --- | --- | --- |
| `坐在床上Sit Down On Couch ~ Sound Clip Royalty Free #199896109.m4a` | M4A/AAC / 1.220s | `Foley/FOL03_17F02_BedroomWake_ClaireSitOnBed_01.mp3` | 17F02 Scene 2.1 `BedroomWake`，Claire 开门后坐到床边 | `Wife.SitOnBed` 为保留槽，尚未自动触发或绑定 | 与床边动画和字幕时点对齐；检查 Couch 质感是否适合床垫 |
| `Sound Effect- Kitchen Gas Stove Cooking Fry Food 2 ~ #73557335.m4a` | M4A/AAC / 31.402s | `Foley/FOL06_17F04_PATHA_EpilogueKitchen_FryingBed_01.mp3` | 第 4 关 Path A，Scene 4.7 黑屏结局“稍后的早晨—厨房”；不用于第 2 关 | 正式对白稿有厨房 SFX 指示，当前没有对应运行时槽 | 挑选稳定煎炒段，降低音量；仍需判断是否补单次锅铲碰锅 |
| `Sound Effect- Cluster of house keys falling ~ #277239943.m4a` | M4A/AAC / 0.645s | `Foley/FOL07_17F04_PATHA_EpilogueHomeFromSchool_KeysDrop_01.mp3` | 第 4 关 Path A，“那天下午—放学回家”，钥匙落桌 | 正式对白稿有 SFX 指示，当前没有对应运行时槽 | 检查落点材质；与黑屏字幕和对白时点对齐 |
| `音效：雷声轰鸣，暴雨来临 ~ #250296715 --- Sound Effect- Powerful Thunder Starts Rainstorm ~ #250296715.m4a` | M4A/AAC / 59.183s | `Foley/FOL08_17F04_PATHA_EpilogueLilyRoom_ThunderRainstorm_SourceFull_01.mp3` | 第 4 关 Path A，“暴风雨之夜—Lily 房间” | 正式对白稿有远雷指示，当前没有对应运行时槽 | 从整段提取远雷，弱化或删除明显雨声与近雷爆点 |
| `Luggage drop, heavy suitcase impact ~ Stock Sound #243751058.m4a` | M4A/AAC / 1.067s | `Foley/FOL09_17F04_PATHB_EpilogueFrontHall_SuitcaseThresholdImpact_01.mp3` | 第 4 关 Path B，“三年后—前厅”，行李箱过门槛的撞击候选 | 正式对白稿有行李箱过门槛指示，当前没有对应运行时槽 | 检查是否过重；必要时降音量、削低频或放弃该候选 |
| `音效：旅行者必备拉杆箱音效 ~ #287497055 --- Sound Effect- Traveler's Essential Rolling Suitcase Sound Effect ~ #287497055.m4a` | M4A/AAC / 10.057s | `Foley/FOL09_17F04_PATHB_EpilogueFrontHall_SuitcaseRolling_SourceFull_01.mp3` | 同一段落，Lily 拉行李箱越过门口，随后接通用住宅门关闭声 | 正式对白稿有 SFX 指示，当前没有对应运行时槽 | 选择短滚动段，与门槛撞击和门关闭组成连续事件 |

## 8. 本轮验收

- 正式目录共有 36 个 MP3，且没有复制 M4A、WAV 或 OGG。
- 所有正式文件均通过 FFprobe 解码、声道数和时长容差检查。
- 5 个原始 MP3 与项目副本二进制一致。
- 桌面原件处理前后 SHA-256 一致。
- 补充批次从 `E:\桌面\音效\新音效` 导入 7 个文件；普通无效操作／门锁定反馈已按用户决定取消，没有导入对应素材。
- 2026-08-01 导入批次当时未修改 Unity/C#；2026-08-02 已完成运行时和 Editor 绑定，并同步更新 `脚本使用说明总表.md`。

## 9. 2026-08-02 Unity 正式绑定状态

- 中央资产：`Assets/Audio/HEARTH/HearthSfxCatalog.asset`，登记 `36` 个导入 MP3 和 `Assets/Mini First Person Controller/Audio/Steps.wav`，共 `37` 个 Sound ID。
- 场景入口：`MIN_LOOP_ROOT/Audio/StorySFX_Global / Lobby / 17F01 / 17F02 / 17F03 / 17F04`；所有自动 Cue 通过 Catalog 解析 Clip。
- UI/System：全部终端的打开、关闭、开机、翻页、焦点、提交、回放和视角切换已绑定；Human/Companion HUD、Hold E、危险弹窗与关机已绑定。
- 环境：大厅 RoomTone + 局部 3D Walla、电梯完整流程、17F01 房间层、17F02 爵士/淋浴/冰箱/交通、17F04 房间与两条结局环境层已绑定，并在对白时按需 Duck。
- Foley：人类脚步、轻型陪伴履带、角色起身/落座、餐桌、住宅门、钥匙、煎锅、雷雨、行李箱已绑定；片段通过运行时起点/时长使用，不产生派生副本。
- 明确未绑定：普通无效操作、门锁拒绝、猫叫或猫脚步；保持静默是当前制作决定。
- 自动维护：`Tools > Hearth > Audio > Apply Production Story SFX Setup`；校验：`Validate Production Story SFX Setup`。
- 待 Play Mode 主观试听：长环境源循环点、Walla 与对白相对响度、门 Open/Close 分段、17F02 交通低通和 17F04 体育馆远近感。
