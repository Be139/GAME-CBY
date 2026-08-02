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
