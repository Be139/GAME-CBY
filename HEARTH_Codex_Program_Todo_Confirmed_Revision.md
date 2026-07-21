# HEARTH — Codex 程序待做与流程修改说明（本轮确认版）

> **2026-07-21 项目执行覆盖说明（优先级最高）**
>
> - 当前正式游戏不制作、不播放也不预留第一幕宣传片，直接从一楼大厅开始；下文第 1 节和第 2.1 节中的宣传片状态仅保留为历史设想。
> - 17F01、17F02 的现有空间玩法、机器人回放、人物走位和门口终端结构全部保留；只采用本文对对白顺序、选择门控、评价和下一户指引的修订。
> - 17F04 低信任关闭继续采用项目现有的三阶段病毒弹窗玩法；第 15.2 节的三条警告用于三类弹窗的语义和对白，不改成三次静态 Space 确认。
> - 时间以当前正式稿为准：Lily 留言 `4:42 PM`，17F03 当前检查约 `18:57`（深眠发生于 `18:47`），Mia 到自宅门口约 `19:08`。
> - 除以上四条覆盖项外，本文件其余修订继续执行。

## 0. 文档用途

这份文档专门说明**游戏程序、触发顺序、交互状态和分支判断**。
Codex 不能只读取对白并把文字放进游戏，还必须按照本文件检查整个流程。

配套中文审阅稿：

- `HEARTH_Complete_Chinese_Script_Confirmed_Revision.md`

重要说明：

1. 中文稿用于确认内容，不是最终英文配音文件。
2. 最终英文对白稍后另行生成。
3. 最终英文版必须以现有未删减英文母语化剧本为底稿修改，不能把中文稿直接翻译成英文。
4. 新增英文对白需要沿用原英文稿的美式口语、人物称呼、情绪标签和短语音单元结构。
5. 现有任务终端、家庭资料面板和 HUD 文本原则上保持不变。除本文明确列出的新增或替换项，不要擅自重写 UI。

---

# 1. 用户本人制作的 HUD 宣传片

## 1.1 责任边界

开场宣传片由**用户本人亲手制作**，包括：

- 游戏场景选择
- 运镜
- 剪辑
- 音乐
- 宣传片旁白
- 最终视频文件

Codex **不要生成宣传片、不要调用 AI 制作视频、不要替换或修改用户提供的视频资产**。

Codex 只负责：

1. 预留开场视频播放接口。
2. 播放用户之后提供的视频文件。
3. 在视频结束时执行渐黑。
4. 短暂停顿后渐亮。
5. 切换到 A 栋一楼大堂的 Mia 第一人称视角。
6. 启动大堂开场对白序列。

用户尚未提供最终视频路径时，可以使用清晰命名的占位引用，例如：

```text
Assets/Video/USER_PROVIDED_HEARTH_PROMO.mp4
```

不要自行创建替代宣传片。

## 1.2 世界观设定

- 当前年份：2045 年。
- 陪伴单元从 2035 年左右开始规模化生产。
- 陪伴单元具备高级语言理解、情绪识别和自主决策能力。
- 剧本不确认陪伴单元具有人的自我意识。
- “全球近半家庭已经使用”“2050 年进入几乎所有家庭”属于企业宣传口径。

---

# 2. 开场播放与玩家控制

## 2.1 状态顺序

```text
Boot
→ PlayUserPromoVideo
→ FadeToBlack
→ LoadOrRevealLobby
→ FadeFromBlack
→ LobbyOpeningDialogue
→ FreeLobbyExploration
```

## 2.2 大堂开场对白期间的控制规则

宣传片结束、画面渐亮后：

```text
Camera Look Input: ENABLED
Player Translation Movement: DISABLED
Interaction: DISABLED
```

玩家可以自由转动第一人称视角，但不能走动。

直到以下条件完成：

```text
Mia 说完 Lily 消息后的最后一句“好”
```

随后：

```text
Camera Look Input: ENABLED
Player Translation Movement: ENABLED
Interaction: ENABLED
```

不要继续使用“视角和人物都锁死”的旧逻辑。

## 2.3 大堂三个公共事件

三个事件全部为可选：

- 小女孩
- 年轻男人
- 老奶奶

规则：

- 不要求完成三个事件。
- 不影响任务终端解锁。
- 每个事件只播放一次。
- 任务终端在开场控制锁解除后立即可用。
- 玩家可以看完零个、一个、两个或三个事件后前往电梯。

如现有单个事件播放时会暂时锁定移动，可保留该机制，但必须允许玩家转动视角。事件和 Mia 的离场评价结束后恢复移动。

---

# 3. 对白与字幕技术规则

## 3.1 长度

用户已经手动把较长台词拆成多个独立语音单元。不得重新合并。

最终英文对白的每个语音单元：

```text
word_count <= 当前未删减英文稿中最长语音单元
```

当前使用的安全上限为约 42 个英文单词。新增内容也需要拆分。

## 3.2 情绪标签

不得因为正文删减而缩短情绪标签。

每条配音仍需包含：

- 核心情绪
- 声音力度
- 语速或节奏
- 犹豫、打断、压抑、录音、失真等必要状态

动作和镜头继续写在场景说明中，不写进语音标签。

## 3.3 角色不能念 A/B

UI 可以继续显示：

```text
A.
B.
```

但角色对白中不能说：

```text
选择 A
选择 B
A 方案
B 方案
```

Field Unit 和 Mia 必须说具体处置名称，例如：

- 批准升级至“夜间陪伴 Pro”
- 启动两周低介入观察期
- 远程重启并启动伴侣关系修复模块
- 保持关闭并进入观察期
- 立即重启单元
- 保持离线并进入七天人工观察期

---

# 4. 一楼大堂

## 4.1 宣传片后的对白

使用最终英文剧本中的短语音单元播放，内容包括：

- Field Unit 上线。
- 说明刚才是公司宣传内容。
- 简短说明三户巡检任务。
- 说明十七层也是 Mia 自家楼层。
- 提示大堂中可以观察陪伴单元。
- 引导前往任务终端。

## 4.2 Lily 消息

播放顺序：

```text
Opening Briefing
→ Lily HUD Message Opens
→ Lily Recorded Voice
→ Mia Asks
→ Field Unit Replies
→ Mia Says “Okay”
→ Lily HUD Message Closes
→ Player Movement Unlocks
```

Lily 消息关闭后：

- 不固定在 HUD。
- 不出现在终端画面。
- 不出现在陪伴单元回放视角。
- 不在后续切回 Mia 视角时重新出现。

## 4.3 年轻男人事件

删除旧的久坐提醒逻辑和对应对白：

```text
“您从三点开始就一直坐着……”
“等一下。”
```

保留：

- 文档措辞辅助
- 图表调用
- 帮他整理发给母亲的消息
- 发送消息
- 读取母亲回复
- Mia 的生活琐事评价

## 4.4 老奶奶事件

在原对话之前新增明确触发：

```text
Care Unit 主动在胸前屏幕展示孙女昨天发送的画。
```

然后再进入年龄、画面内容和“第三次询问”的对话。

## 4.5 任务终端

任务终端现有 UI 内容保持不变。

终端可用条件：

```text
LobbyOpeningDialogueComplete == true
```

不能依赖：

```text
GirlEventComplete
YoungManEventComplete
GrandmotherEventComplete
```

---

# 5. 电梯与十七层转场

## 5.1 电梯说明

只保留三步巡检流程：

1. 在门外终端读取资料。
2. 进入家用陪伴单元视角回放事件。
3. 选择具体处置。

不在电梯中重复：

- 覆盖率长数据
- 雇主、保险、学校影响
- 三户详细家庭资料
- 第二户完整风险数据

## 5.2 电梯到达

```text
DialogueComplete
→ FadeToBlack
→ ElevatorChime
→ Move/Load Player At 17F Elevator Exit
→ FadeFromBlack
→ Field Unit Points To 17F-01 Exterior Terminal
```

---

# 6. 第一户流程

## 6.1 终端位置

17F-01 使用**门外巡检终端**。

```text
Player does not enter apartment.
No badge.
Inspector authorization is automatic.
```

## 6.2 回放按钮

家庭资料和背景说明播放完毕后，Field Unit 提示：

```text
请选择终端中的最后一个按钮，开始回放。
```

Space 键或具体按键由 UI 提示显示，不由角色念出来。

## 6.3 回放内容

以下内容保持未删减版本：

- Noah 噩梦
- 家用单元安抚
- 第二天早晨父母对话
- Emily 判断“应该是单元处理好了”
- 父母逐渐意识到 Noah 已经很久没有向他们讲过噩梦

## 6.4 回放结束

```text
PlaybackEnd
→ ReturnToMiaHumanView
→ LoadDispositionPanel
→ FieldUnitExplainsAndRecommends
→ UnlockChoice
```

Field Unit 不再使用“全年安全区”作为主要推荐理由。

推荐依据：

- 刚才的噩梦处理成功。
- 父母主动申请升级。
- 升级措施经过更多家庭案例验证。

## 6.5 选择后的评价

保留未删减英文版中的完整分支评价。不要使用早期减半稿中的短评价替换。

---

# 7. 第二户流程

## 7.1 时间轴

统一使用：

```text
17:55 Claire 到家
17:57 开始与单元交谈
18:05 倾诉结束
18:12 晚餐开始
18:14 Ben 询问工作
18:32 Ben 强制关闭单元
18:35 Mia 到达第二户终端
```

不得残留：

- 18:34 作为 Claire 唤醒单元时间
- 18:47 作为第二户强制关机时间

## 7.2 终端位置

17F-02 使用门外巡检终端。

```text
Player does not enter apartment.
No badge.
Inspector authorization is automatic.
```

## 7.3 Scene 2.1、2.2、2.3

保留未删减版本。

Claire 接受音乐后的新台词需要表达：

```text
谢谢你。有你在，我感觉好多了。
```

最终英文版重新母语化，不能机械翻译。

## 7.4 Scene 2.4

家庭日志查看保持未删减。

必须保留：

- 今日完整时间日志
- 十四天对比
- 9 次对单元、1 次对 Ben
- Ben 逐步查看完整会话
- Ben 强制关机

## 7.5 Scene 2.5

黑屏争吵是第二户主要删减区域。

保留的核心信息：

- Claire 曾经先找 Ben。
- Ben 让她等十分钟。
- 等 Ben 问时，她已经处理完情绪。
- 再讲一遍令她疲惫。
- 两周内对单元倾诉 9 次，对 Ben 只有 1 次。
- Ben 以为两个人无话不谈。
- Claire 认为单元是在替 Ben 分担。
- Ben 认为自己被替代。
- 两个人都不知道关系从什么时候变成这样。

程序上使用新的短台词顺序，不要继续播放被删除的旧音频。

## 7.6 第二户处置说明

Field Unit 必须说完整名称：

- 家庭情感稳定指数跌破警戒线。
- 无外部支持时，两周内很可能分开。
- 推荐远程重启并启动伴侣关系修复模块。
- 观察期让两个人自行处理，但历史稳定效果较低。

选择后的完整评价沿用未删减版本。

---

# 8. 第三户门外终端与入户权限

## 8.1 时间轴

```text
18:47 17F-03 核心服务进入深度休眠
18:50 Mia 到达第三户门外终端
约 19:00 完成第三户
约 19:02 到达自宅门口
```

## 8.2 读取规则

第三户门外终端**读取成功**。

可以读取：

- 家庭成员
- 单元类型
- 购买背景
- 使用功能
- 当前异常
- 远程重启失败

不能在走廊完成：

- 核心服务恢复
- 本地维护菜单检查
- 实体维修

因此玩家需要通过终端取得：

```text
EmergencyEntryAuthorization
```

确认后门锁开放。

不要继续使用旧逻辑：

```text
Household data unreadable from outside
```

## 8.3 入户

第三户是今晚唯一需要真正进入室内维修的住户。

进入室内后：

- Laura 和 Mark 对话。
- 删除 Laura 的血压与医生台词。
- 其余父母开场保留未删减版本。
- 玩家在实体单元上读取本地回放。

---

# 9. 第三户回放与处置状态机

## 9.1 回放

Scene 3.3 保持未删减。

Scene 3.4 只删除 Ava 的重复句：

```text
“他一句话都没有跟我说。”
```

其余 Ava 对“机器越来越了解父母，父母越来越不了解她”的表达必须保留。

## 9.2 回放后的正确程序顺序

必须严格实现：

```text
ThirdPlaybackEnds
→ ReturnToMiaHumanView
→ LauraAsksWhatHappened
→ MiaSaysSheFoundShutdownPoint
→ EnablePhysicalUnitInteraction
→ PlayerPressesInteract
→ MoveCameraToInspectionPosition
→ OpenDispositionPanelLocked
→ FieldUnitExplainsBothMeasures
→ FieldUnitRecommendsRestart
→ UnlockDispositionInput
→ PlayerChooses
→ MiaStatesConcreteDecisionToParents
→ ParentsRespond
→ FadeRoomToBlack
→ TeleportPlayerTo17F03CorridorAnchor
→ FadeFromBlack
→ FieldUnitEvaluatesConcreteDecision
→ EvaluateCumulativeTrust
→ AnnounceAllInspectionsComplete
```

## 9.3 选择前锁定

在 Field Unit 说完建议前：

```text
ChoiceNavigation may be visible
Confirm input must be disabled
```

建议播放完毕后：

```text
Confirm input enabled
```

## 9.4 推荐内容

Field Unit 推荐重启的依据：

- Mark 和 Laura 的工作压缩直接陪伴时间。
- Ava 处于需要持续家庭支持的青春期。
- 重启可以继续承担陪伴、调解和状态同步。
- 七天人工观察会增加父母沟通与照护负担。

这里是 Field Unit 的公司立场，不代表游戏客观认定 Ava 没有判断能力。

## 9.5 家庭回应与评价位置

选择后：

1. Mia 先向父母说明具体措施。
2. 父母回应。
3. 回应全部结束。
4. 玩家被传送到走廊。
5. Field Unit 才评价刚才的具体处置。

Field Unit 的评价不能在房间内抢在父母反应之前播放。

---

# 10. 信任值

前三户每户：

```text
推荐措施 = +1
非推荐措施 = -1
```

最终可能值：

```text
+3
+1
-1
-3
```

只有符号影响关闭方式：

```text
trust > 0 → 正常关闭权限／完整告别
trust < 0 → 强制关闭流程／跳过告别
```

第三户单独选择重启，不代表三户都回到安全区。禁止播放：

```text
Three of three reviews are now within the stable range.
```

改用：

```text
All three inspections are complete.
```

Path B 中也不能无条件播放：

```text
top performance band
```

根据累计信任分别播放：

```text
trust > 0 → shift within accepted performance range
trust < 0 → supervisor review pending
```

---

# 11. 自宅门口

门口终端按钮使用：

```text
ACKNOWLEDGE
```

含义仅为：

```text
Mia 已经查看待回应事项
```

不代表：

- Mia 已经答应出席。
- Mia 已经替 Lily 作出回答。
- 最终选择已经完成。

Field Unit 提示：

```text
请确认您已查看这条待回应事项。
接下来，请回到家中处理。
```

---

# 12. 电子照片屏

## 12.1 资产结构

删除“两张独立相框”的交互逻辑。

改为：

```text
One Electronic Photo Display
PhotoIndex = 0 or 1
Left/Right input switches photos
```

照片：

```text
Photo 0: Christmas 2044
Photo 1: Last week's certificate photo
```

## 12.2 播放规则

- 默认显示第一张。
- 每张照片第一次显示时播放对应 Field Unit 说明。
- 重复切换不重复播放。
- 两张照片说明全部完成后，播放明确目标提示：

```text
进入 Lily 的房间，处理她在语音消息中提出的问题。
```

- 目标提示必须完整说完，不能被房间内声音打断。
- 目标提示结束后，才开始播放 Lily 房间里的练习声。

---

# 13. Lily 房门外与房间

## 13.1 房门外

先明确 Lily 正在练习第二天开放日的演讲。

新增家用单元台词：

```text
现在开始今晚的倒数第二次练习。
```

其余 Scene 4.3 使用未删减版本。

## 13.2 Lily 房间

Scene 4.4 使用未删减版本。

---

# 14. 最终选择与第四面墙诱导

## 14.1 播放顺序

```text
Lily asks if Mia will come
→ Field Unit recommends machine answer
→ Field Unit says “better ending”
→ Field Unit explains short-term stability
→ Field Unit warns personal answer may disturb stability score
→ Show final choice
```

## 14.2 具体 UI 文本

可以保留 A/B 操作标记，但文字改得更具体：

```text
A. 亲自回答 Lily，并由 Mia 承担这个承诺
B. 采用随行单元推荐，让家用陪伴单元替 Mia 回答
```

## 14.3 “更好的结局”

此处选择明显打破第四面墙。

Field Unit 需要使用相当于以下含义的台词：

```text
让家用单元替您回答，更有可能得到一个更好的结局。
```

这句话同时表示：

- 短期数据结果更稳定。
- 它仿佛知道玩家正在选择游戏结局。

玩家相信建议后进入 Path B 坏结局。

---

# 15. 关闭家用单元

## 15.1 高信任

删除“书桌抽屉里的画”整段。

保留：

- 雷雨恐惧
- Lily 过去只告诉单元
- 单元让她以后告诉妈妈
- 单元把照护责任交还 Mia
- Lily 对单元产生离别感

## 15.2 低信任

在三次系统警告前，家用单元先说：

```text
Mia，你确定要关闭我吗？
```

然后家用单元不再说话。

警告顺序：

1. 当前累计评价不足以执行常规关闭。
2. 强制关闭将覆盖家庭稳定建议。
3. 强制关闭将跳过告别协议。

最后一次确认后关机，再进入 Lily 与 Mia 的原有对话。

---

# 16. Path A 黑屏后日谈

每段声音前先显示文字卡：

```text
某天早晨 · 厨房
当天放学后 · 家中
某个雷雨夜 · Lily 的房间
```

使用未删减版对白。

结尾保留：

```text
Lily，九岁。
开放日的第二天。
妈妈去了，坐在第一排。
```

---

# 17. Path B 黑屏后日谈

每段声音前显示文字卡：

```text
第二天早晨 · 厨房
当天下午 · 学校开放日
三年后 · 家中玄关
```

使用未删减版对白。

结尾保留：

```text
家庭情感稳定指数：安全区
单元服务时长：六年
家庭满意度：高
下一次复核：三年后
```

---

# 18. 年份一致性

当前年份：2045。

需要同步检查所有时间文字：

```text
旧：Christmas 2024
新：Christmas 2044
```

Lily 在圣诞照片中七岁。当前故事中约八岁。Path A 结尾文字卡显示 Lily 九岁，可以理解为后日谈蒙太奇已经推进到她九岁时。

全剧搜索并清除明显错误年份：

```text
2024
20124
```

除非它们存在于开发日志，不应出现在正式剧情或 UI 中。

---

# 19. Sequence 与音频映射

保留现有 `HEARTH:SEQUENCES` 结构。

规则：

- 每条配音正上方必须有正确 Sequence 标签。
- 被删除对白对应音频和触发也要删除。
- 新增对白需要创建清晰的新 Sequence 或加入正确的现有 Sequence。
- 不得把一个标签误绑到下一句。
- 选择分支标签只能在对应分支播放。

建议新增或检查的逻辑序列：

```text
Prologue_HUDPromo
Lobby_PostPromoBriefing
Lobby_LilyVoiceMessage
Lobby_Group02_YoungMan
Lobby_Group03_Grandmother
17F01_ExteriorTerminalIntro
17F01_PostPlaybackRecommendation
17F02_BlackAudioArgument_Short
17F03_ExteriorTerminalAuthorization
17F03_PostPlaybackLockedRecommendation
17F04_ElectronicPhotoDisplay
17F04_SpeechPracticePrelude
17F04_FinalChoiceFourthWallRecommendation
17F04_ShutdownLow_Warning
17F04_Epilogue_PathA_Cards
17F04_Epilogue_PathB_Cards
```

具体名称可适配现有工程命名，但必须保持唯一且可追踪。

---

# 20. 验收清单

## 开场

- [ ] Codex 没有生成宣传片。
- [ ] 播放的是用户提供的视频资产或占位引用。
- [ ] 视频结束后先渐黑，再渐亮到大堂。
- [ ] 大堂开场对白期间可转动视角。
- [ ] 大堂开场对白期间不能移动。
- [ ] Mia 说完“好”后恢复移动。
- [ ] 三个公共事件全部可跳过。
- [ ] 任务终端不依赖公共事件。

## 巡检通用

- [ ] 无刷工牌逻辑。
- [ ] 第一户和第二户在门外终端完成。
- [ ] 第三户终端读取成功。
- [ ] 第三户通过终端取得入户权限。
- [ ] 角色对白不念 A/B。
- [ ] 长对白保持拆分。
- [ ] 情绪标签没有被缩短或丢失。

## 第一户

- [ ] 噩梦回放未删减。
- [ ] 早晨父母对话未删减。
- [ ] 回放后推荐依据改为事件效果、父母申请和案例验证。
- [ ] 两个结果评价保留完整版本。

## 第二户

- [ ] 时间轴不存在倒置。
- [ ] 日志查看未删减。
- [ ] 只精简黑屏争吵。
- [ ] 9 次和 1 次仍存在。
- [ ] Claire 的感谢台词已更新。
- [ ] 处置建议讲清完整措施与风险。
- [ ] 分支评价保留完整版本。

## 第三户

- [ ] 18:47 离线，18:50 到达。
- [ ] 删除 Laura 血压台词。
- [ ] 删除 Ava 重复句。
- [ ] 回放结束后 Laura 先问。
- [ ] 选择页面先锁定。
- [ ] 建议播完后才可确认。
- [ ] Mia 先向父母宣布措施。
- [ ] 父母回应后传送走廊。
- [ ] Field Unit 在走廊评价。
- [ ] 不错误宣称三户全部进入安全区。

## 自宅

- [ ] ACKNOWLEDGE 不等于回答 Lily。
- [ ] 一个电子照片屏，左右切换两张照片。
- [ ] 圣诞照片年份为 2044。
- [ ] 两张照片语音只播放一次。
- [ ] 目标提示完整说完后才播放房间声音。
- [ ] 房门外明确是演讲练习。
- [ ] 最终选择前出现“更好的结局”诱导。
- [ ] 最终按钮使用具体措辞。
- [ ] 高信任删除画作段落。
- [ ] 低信任先播放家用单元确认句。
- [ ] Path A 和 Path B 都有黑屏文字卡。
- [ ] 两个结尾统计文字都保留。

---

# 21. 可逆修改提示词：删除第四面墙诱导

以下提示词独立保存，不写进正式游戏剧情。以后决定取消“更好的结局”诱导时，可以直接交给 Codex：

```text
请修改 HEARTH 最终选择前的 Field Unit 建议，移除所有明显打破第四面墙的表达。

具体要求：
1. 删除或替换 “better ending / 更好的结局” 等让 Field Unit 像是知道玩家正在选择游戏结局的台词。
2. 保留 Field Unit 推荐让家用陪伴单元代答的公司立场。
3. 推荐理由只允许涉及 Lily 当前的情绪稳定、演讲练习连续性和家庭情感稳定指数。
4. 不改变最终两个选择、不改变 Path A 与 Path B 的剧情结果，也不改变信任值。
5. 将建议改写为中性的系统措辞，例如：
   “Allowing the household unit to respond is more likely to preserve Lily’s current emotional stability and keep tonight’s rehearsal on track.”
6. 保留“玩家也可以让 Mia 亲自回答，但稳定指数可能波动”的风险提示。
7. 更新相关 Sequence、字幕和音频引用，删除不再使用的第四面墙语音资源。
```
