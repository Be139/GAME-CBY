# 《已为您沟通》UI 说明书 · for Unity Agent

> **本文档目的**：解释这份 40 页 PPT 中每个 UI 是什么、什么时机出现、有哪些可交互区域、点击后跳转到哪个页面。Unity 开发时按本说明书构建 UI 系统。

---

## 目录

1. [项目背景](#一项目背景)
2. [PPT 文件结构](#二ppt-文件结构)
3. [UI 系统总览](#三ui-系统总览)
4. [组件库（slides 25–40）](#四组件库slides-2540)
5. [页面清单（slides 1–24）](#五页面清单slides-124)
6. [完整交互流程](#六完整交互流程)
7. [复用规则：17F-02 怎么办](#七复用规则17f-02-怎么办)
8. [Unity 需要实现的内容（PPT 没展示的）](#八unity-需要实现的内容ppt-没展示的)

---

## 一、项目背景

《已为您沟通》是一款近未来题材的第一人称剧情游戏（PC + VR 双端）。主角 **Mia · 工号 7842**，AI 陪伴单元巡检员，整局发生在一个夜班巡检过程中——戴公司发的眼镜（AVP 风格 HUD），到 A 栋 17F 依次巡检三户家庭的陪伴单元，最后回到自己家面对 9 岁的女儿 Lily，作出二选一的终局决定。

所有 UI 都是 Mia 的眼镜 HUD 系统呈现的（半透明浮屏 / 玻璃质感 / 几何切角）。视觉上"这是一台工作设备，不是游戏 UI"。

---

## 二、PPT 文件结构

PPT 共 40 页，分两部分：

| 范围 | 内容 | 用途 |
|---|---|---|
| **Slides 1–24** | 成品 UI 画面（黑色背景 / 玻璃质感浮屏完整呈现） | 按剧情时序排列的最终视觉效果，作为 **Unity UI 还原的视觉参考** |
| **Slides 25–40** | 单独的 UI 组件（白色背景 / 透明导出用） | 拆出的独立组件，作为 **Unity Prefab 的设计参考**，可分别导出 PNG 透明背景图后让 Unity 识别元素 |

**Unity Agent 的工作流建议**：
1. 先看 slides 25–40 理解组件库
2. 再看 slides 1–24 理解组件如何组合成完整页面
3. 按本说明书中"页面清单"实现状态机
4. 按"交互流程"接 Unity 的状态切换逻辑

---

## 三、UI 系统总览

### 3.1 三类 HUD

| 类型 | 行为 | 包含哪些页面 |
|---|---|---|
| **A 常驻 HUD** | 始终显示在屏幕四角，不集中，半透明 | Slides 1（裸态）/ 7 / 8 / 19 / 20 / 21 / 22 / 23 / 24 |
| **B 全屏接管类** | 出现时遮挡常驻 HUD，背景轻微暗化 | Slides 3 / 4 / 5 / 12 / 13–18 |
| **C 局部浮屏类** | 占据画面中部，四周常驻 HUD 保留 | Slides 2 / 6 / 9 / 11 |

### 3.2 常驻 HUD 的元素分布

| 位置 | 元素 | 对应组件 |
|---|---|---|
| 左上 | 工号信息条 + 状态点 | Slide 28（绿色 / ACTIVE）/ Slide 36（红色 / ALERT） |
| 顶部居中 | 日期 + 时间 `2026.09.15·MON·18:47` | （直接文字，无独立组件） |
| 右上 | 信任度变化浮字 `+1 TRUST` / `-1 TRUST` | （瞬时浮现 3 秒消失） |
| 左下 | 当前任务 + 进度 `1/3` | （仅巡检中显示，自宅段消失） |
| 中下 | HUD 旁白字幕（NPC 说话的唯一位置） | （黑底白字，淡入淡出） |

### 3.3 状态点颜色规则

状态点是左上工号区一个小圆点，颜色全游戏统一含义：

| 颜色 | 含义 | 场景 |
|---|---|---|
| 绿色 + 微脉冲 | 巡检中（COMPANION UNIT · ACTIVE） | 全部巡检场景 |
| 灰色 | 静默（COMPANION UNIT · DORMANT） | 自宅段 7 / 8 / 20 / 21 / 22 |
| 红色脉冲 | 警告 / 等待（ALERT · PENDING REVIEW） | Alert 段 13–18 / 警告 23 / 24 / 9 |

### 3.4 字幕区规则

中下方字幕区是**陪伴单元、Lily、所有 NPC 说话的唯一呈现位置**——绝不要把 NPC 的话放到 panel UI 内。

- 字幕居中，宽度适中
- 字幕持续时间 = 语音长度 + 短暂淡出
- 切换字幕时淡入淡出，**不闪烁**

---

## 四、组件库（slides 25–40）

这些是从主页面拆出的独立 UI 组件。Unity 实现时建议每个做成独立 Prefab。

| Slide | 组件名 | 用在哪些主页面 | 用途说明 |
|---|---|---|---|
| **25** | 监测面板（SUBJECT MONITORING） | Slide 6 | 陪伴单元视角左上的服务对象状态监测条 |
| **26** | 二选一选项组（A / B） | Slide 8 | 终局二选一的极简选项 |
| **27** | 工作面板（WORKSPACE）| Slide 12 | 注视入口展开的工作面板，含三户清单 |
| **28** | 工号信息条 · ACTIVE | 所有巡检场景 (1, 2, 3, 4, 5, 7) | 左上工号区，绿色状态点 |
| **29** | 门口终端 · 初始态结构 | Slide 3 | 门口终端的完整 UI 框架（header + tabs + 内容区） |
| **30** | 门口终端 · 处置态结构 | Slide 5 | 门口终端处置态（VIEWED ✓ + A/B 选项栏） |
| **31** | 门口终端 · ACQUISITION tab | Slides 4 / 14 | tab 切换后的内容布局（左叙述 + 右图表） |
| **32** | 机器视角整屏边框 | Slide 6 | 陪伴单元第一人称的整屏几何边框 |
| **33** | E HOLD TO ACT 按钮 | Slide 6 | 陪伴单元视角中央长按按钮 |
| **34** | （Alert 状态相关元素 - 见 PPT 原页） | Slides 13–18 / 23 / 24 / 9 | Alert 红色调状态指示 |
| **35** | 快捷菜单（TODAY'S ROUNDS / DISPOSITION HISTORY / SYSTEM SETTINGS） | Slide 11 | 注视入口聚焦后浮起的小菜单 |
| **36** | 工号信息条 · ALERT | Slides 13–18 / 23 / 24 / 9 | 左上工号区，红色状态点 |
| **37** | 屋内陪伴单元侧面板 | Slide 19 | 17F-03 入户后贴在单元侧面的窄长终端 |
| **38** | 相框拾取浮卡 #1（2023 / 母亲在场） | Slide 20 | 自宅客厅相框 |
| **39** | 相框拾取浮卡 #2（2026 / 母亲不在场） | Slide 21 | 自宅客厅相框 |
| **40** | 体面关闭确认对话框 | Slide 22 | 简单 Confirm / Cancel |

---

## 五、页面清单（slides 1–24）

按剧情时序排列。每页都写明：触发条件 → 包含组件 → 可点击区域 → 点击跳转。

---

### Slide 1 · 常驻态（巡检中）

**类型**: A 常驻 HUD（裸态）  
**触发**: 戴上眼镜后激活；之后整局巡检过程中任何"非接管"时刻

**包含组件**:
- Slide 28（工号信息条 · ACTIVE）
- 顶部时间、左下任务栏、中下字幕区、右上信任度浮字

**可点击区域**:
| 区域 | 点击行为 | 跳转 |
|---|---|---|
| 左上工号区 | 注视 0.5 秒 | → Slide 10 → 11 → 12（入口逻辑三步） |
| 走到门口（物理） | 自动触发 | → Slide 3（17F-01）/ Slide 13（17F-03 Alert） |

**状态变体**:
- 序幕戴眼镜时：任务进度 0/3，无信任度
- 巡检中：任务进度 1/3、2/3，可能有信任度浮字
- 收到预警时：状态点变红，文字改为 `ALERT · PENDING REVIEW`

---

### Slide 2 · 大厅同步终端

**类型**: C 局部浮屏（四周常驻 HUD 保留）  
**触发**: 走到大厅同步终端 + 刷工牌

**包含组件**:
- Slide 28（工号区）+ 四周常驻
- 中央浮屏：Tonight's Rounds 三户清单 + CONFIRM 按钮

**可点击区域**:
| 区域 | 点击行为 | 跳转 |
|---|---|---|
| CONFIRM 按钮 | 确认任务 | → 进电梯简报 → Slide 1（任务激活 0/3） |

---

### Slide 3 · 17F-01 门口终端 · 初始态（RESIDENT SUMMARY tab）

**类型**: B 全屏接管  
**触发**: 走到 17F-01 门口 + 刷工牌

**包含组件**:
- Slide 29（门口终端框架）
- 顶部 header：`DOORWAY TERMINAL` / `17F-01` / 读取时间
- Tab 栏（6 个）：RESIDENT SUMMARY / ACQUISITION / FAMILY LOG / TRUST TREND / INSPECTION HISTORY / **RECALL EVENT ★**
- 内容区：住户摘要（HOUSEHOLD / COMPANION / SERVICE TIME / MONTHLY USAGE / LAST CHECK + 三个标签）

**可点击区域**:
| 区域 | 点击行为 | 跳转 |
|---|---|---|
| RESIDENT SUMMARY tab | 已激活（无变化） | — |
| ACQUISITION tab | 切换内容 | → **Slide 4** |
| FAMILY LOG tab | 切换内容 | → 新页面（PPT 未做，需 Unity 按 Slide 4 布局新建） |
| TRUST TREND tab | 切换内容 | → 新页面（同上） |
| INSPECTION HISTORY tab | 切换内容 | → 新页面（同上） |
| **RECALL EVENT ★**（暖橙 CTA） | 调出回放 | → **Slide 6**（陪伴单元第一人称） |

---

### Slide 4 · 17F-01 门口终端 · ACQUISITION tab

**类型**: B 全屏接管  
**触发**: 在 Slide 3 点击 ACQUISITION tab

**包含组件**:
- Slide 31（ACQUISITION tab 内容布局：左叙述 + 引语 / 右改善趋势图表）

**可点击区域**: 同 Slide 3 的 tab 栏 + RECALL EVENT；切换其他 tab 直接换内容区

> **注**：FAMILY LOG / TRUST TREND / INSPECTION HISTORY 这三个 tab 在 PPT 中没单独画页面，但都共享 Slide 4 / 31 的左右分栏布局结构，只是内容字段不同。Unity 实现时建议做一个通用的左右分栏 Prefab，按 tab 切换数据源。

---

### Slide 5 · 17F-01 门口终端 · 处置态

**类型**: B 全屏接管  
**触发**: 从 Slide 6（陪伴单元第一人称）长按 E 完成回放后回到本终端

**包含组件**:
- Slide 30（处置态结构：VIEWED ✓ + A/B 选项栏）
- Tab 栏的 `RECALL EVENT ★` 被替换为 `VIEWED ✓`（克制的灰青色 + 对勾）
- 底部新增 `SELECT DISPOSITION` + A/B 选项

**可点击区域**:
| 区域 | 点击行为 | 跳转 |
|---|---|---|
| Tab 栏（5 个常规 tab） | 切换内容（处置态后仍可查阅） | → 对应 tab |
| **A · Approve Upgrade**（RECOMMENDED） | 选择处置 | → Slide 1（信任度 -1 + 任务进度 1/3） |
| **B · Enable Observation** | 选择处置 | → Slide 1（信任度 +1 + 任务进度 1/3） |

> **信任度方向**：A 是公司推荐 = 顺从系统视角 → -1；B 是顶住公司压力做更深入处置 → +1。规则在三户处置中保持一致：**B 推动改变 = 玩家成长**。

---

### Slide 6 · 陪伴单元第一人称（17F-01 噩梦回放）

**类型**: C 特殊（整屏机器边框 + Mia 的常驻 HUD 全部消失）  
**触发**: 在 Slide 3 / 4 / 19 点击 RECALL EVENT ★

**包含组件**:
- Slide 32（整屏机器边框）
- Slide 25（SUBJECT MONITORING 监测面板）
- 右上 SYNTH VOICE · DECISION
- 左下数据流装饰
- Slide 33（中央 [Approach Bedside · Watch Over Subject] / E HOLD TO ACT 按钮）
- 屏幕底栏：`COMPANION UNIT · FIRST PERSON · MONITORING MODE`

**可点击区域**:
| 区域 | 点击行为 | 跳转 |
|---|---|---|
| **中央按钮（长按 E）** | 执行机器决策 | → Slide 5（17F-01 处置态） |

> **17F-02 / 17F-03 复用**：Slide 6 同一布局复用三次，每次替换 SUBJECT / Time / State / Heart / Pupil / ASSESSMENT / DECISION / 数据流 / 按钮文案。三次内容差异见第七章"复用规则"。

---

### Slide 7 · 自宅终端

**类型**: A 常驻 HUD（部分隐藏） + 中央极简浮屏  
**触发**: 三户巡检完毕 + 进自宅门

**包含组件**:
- Slide 28（但状态点变灰、文字变 `COMPANION UNIT · DORMANT`）
- 顶部时间、中下字幕区保留
- **左下任务栏消失**
- 中央：`You're home. Welcome.` 一行字

**可点击区域**: 无（自动进度，玩家走入客厅后自动切换到客厅相框场景）

---

### Slide 8 · 终局二选一

**类型**: A 常驻 HUD（部分隐藏） + 中央选项  
**触发**: 进儿童房 + Lily 问完话

**包含组件**:
- Slide 28（DORMANT 状态）+ 顶部时间 + 中下字幕区
- Slide 26（A / B 选项组）

**可点击区域**:
| 区域 | 点击行为 | 跳转条件 |
|---|---|---|
| **A · ANSWER LILY YOURSELF** | 亲口回答 | 信任度 ≥ 阈值 → **Slide 22**（体面关闭）<br>信任度 < 阈值 → **Slide 23**（警告 01/03） |
| **B · LET THE COMPANION ANSWER FOR HER** | 让单元代答 | → 字幕过场 → 黑屏尾声 B 路径 |

> **设计意图**：这里**无推荐标签 / 无信任度提示 / 无任何引导**——整个游戏唯一一次"UI 不帮你"的选择。Unity 实现时不要加任何 hover 高亮或 tooltip。

---

### Slide 9 · 强行关闭警告 · 03 / 03（最后一次）

**类型**: C 局部浮屏 + Alert 常驻  
**触发**: Slide 24 点击 YES · CONTINUE

**包含组件**:
- Slide 36（工号区 ALERT）+ 顶部时间 + 字幕区
- 中央警告浮屏（红色调）

**可点击区域**:
| 区域 | 点击行为 | 跳转 |
|---|---|---|
| **YES · FORCE EXECUTE** | 确认强制关闭 | → 黑屏尾声 A 强行路径 |
| **CANCEL** | 取消 | → Slide 8（返回二选一） |

---

### Slide 10 · 入口逻辑 · ① 默认态

**类型**: 演示页（不在游戏剧情中作为状态，但描述了 Slide 1 的初始时刻）  
**触发**: 玩家开始注视左上工号区但未到 0.5 秒

**说明**：旁注 `① Resident — glance held under 0.5 s, nothing expands` 说明此时玩家正在尝试触发但还未成功。

**Unity 实现要点**：注视计时器 0–0.5s 内显示此状态，工号区无变化。

---

### Slide 11 · 入口逻辑 · ② 入口聚焦

**类型**: 演示页 → Slide 1 的中间过渡态  
**触发**: 注视工号区满 0.5 秒

**包含组件**:
- Slide 28 + 左上工号区高亮 + 圆环闭合反馈
- Slide 35（快捷菜单浮起：TODAY'S ROUNDS / DISPOSITION HISTORY / SYSTEM SETTINGS）

**可点击区域**:
| 区域 | 点击行为 | 跳转 |
|---|---|---|
| TODAY'S ROUNDS | 点击 / 凝视确认 | → **Slide 12**（工作面板完全展开） |
| DISPOSITION HISTORY | 同上 | → 处置历史页（PPT 未做） |
| SYSTEM SETTINGS | 同上 | → 设置页（PPT 未做） |
| 视线移开 / 点击外区域 | 取消 | → 回到 Slide 1 |

---

### Slide 12 · 入口逻辑 · ③ 工作面板展开

**类型**: B 全屏接管  
**触发**: 在 Slide 11 点击 TODAY'S ROUNDS

**包含组件**:
- Slide 27（WORKSPACE 工作面板：三户状态清单）
- 背景轻微暗化

**可点击区域**:
| 区域 | 点击行为 | 跳转 |
|---|---|---|
| 三户清单的某一户 | 查看历史处置（可选） | → 子页面（PPT 未做） |
| ESC / 点外区域 | 关闭面板 | → 回到 Slide 1 |

**状态变体**（同一布局，根据剧情进度显示不同状态）：
- 17F-01 进入前：PENDING / PENDING / PENDING / SELF LOCKED
- 17F-01 完成后：DONE / IN CHECK / PENDING / SELF LOCKED
- 全部完成后：DONE / DONE / DONE / SELF UNLOCKED

---

### Slide 13 · 17F-03 门口终端 · Alert 态 · RESIDENT SUMMARY

**类型**: B 全屏接管（红色 Alert 变体）  
**触发**: 22:14 收到预警 + 走到 17F-03 门口（自动触发，不需刷卡）

**包含组件**:
- Slide 36（工号区 ALERT）+ Alert 常驻 HUD
- 红色调门口终端框架
- Tab 栏 6 个：RESIDENT SUMMARY / ACQUISITION / FAMILY LOG / TRUST TREND / INSPECTION HISTORY / **ENTER UNIT ★**（红色 CTA，注意是"入户"不是"调出事件"）
- 内容区：住户摘要（含 `MONTHLY USAGE × Unreadable` 等无法读取字段）

**可点击区域**:
| 区域 | 点击行为 | 跳转 |
|---|---|---|
| ACQUISITION tab | 切换 | → **Slide 14** |
| FAMILY LOG tab | 切换 | → **Slide 15** |
| TRUST TREND tab | 切换 | → **Slide 16** |
| INSPECTION HISTORY tab | 切换 | → **Slide 17** |
| **ENTER UNIT ★**（红色） | 入户检查 | → **Slide 19**（屋内侧面板） |

---

### Slide 14 · 17F-03 ACQUISITION tab

**类型**: B 全屏接管（红色 Alert 变体）  
**触发**: 在 Slide 13 点击 ACQUISITION

**说明**：左侧叙述 + 引语，右侧 FESI 演变图（含 `Current × unreadable`）。点击其他 tab 切换；点击 ENTER UNIT → Slide 19。

---

### Slide 15 · 17F-03 FAMILY LOG tab

**触发**: 在 Slide 13/14 点击 FAMILY LOG

**说明**：左侧 EVENT TIMELINE（含 `2026.09.14 · 22:00–22:14 × DATA UNREADABLE`），右侧详情面板显示最后可读片段：`> child user inputting maintenance commands > ... > OFFLINE`。

点击其他 tab 切换；点击 ENTER UNIT → Slide 19。

---

### Slide 16 · 17F-03 TRUST TREND tab

**触发**: 在 Slide 13/14/15 点击 TRUST TREND

**说明**：显示三个家庭成员各自的信任度（女儿降幅最大），叙述部分包含女儿 8 岁到 12 岁的语言记录变化。

点击其他 tab 切换；点击 ENTER UNIT → Slide 19。

---

### Slide 17 · 17F-03 INSPECTION HISTORY tab

**触发**: 在前面任一 tab 切到此

**说明**：过往巡检员处置历史，关键行 `2026.06.02 · #7138 · Cooldown patch · daughter curve flagged · no escalation` 暗示问题早就被发现但未升级。

点击其他 tab 切换；点击 ENTER UNIT → Slide 19。

---

### Slide 18 · 17F-03 处置态

**类型**: B 全屏接管（红色 Alert 变体）  
**触发**: 从 17F-03 维护菜单回放（借用 Slide 6 模板）完成后

**包含组件**:
- 同 Slide 5 结构，但红色调
- Tab 栏的 `ENTER UNIT ★` 被替换为 `VIEWED ✓`
- A/B 选项：
  - A · Restart and Restore Service（RECOMMENDED · 公司推荐恢复）
  - B · Honor User Shutdown（尊重女儿的关闭）

**可点击区域**:
| 区域 | 点击行为 | 跳转 |
|---|---|---|
| **A · Restart** | 选择处置 | → Slide 1（信任度 -1 + 任务进度 3/3 + 状态点恢复绿色） |
| **B · Honor Shutdown** | 选择处置 | → Slide 1（信任度 +1 + 任务进度 3/3） |

---

### Slide 19 · 屋内陪伴单元侧面板（17F-03）

**类型**: C 局部浮屏（四周常驻 HUD 保留 / Alert 状态）  
**触发**: 在 Slide 13 点击 ENTER UNIT ★ → 进门动画 → 走到单元前

**包含组件**:
- Slide 37（侧面板）+ Slide 36（工号区 ALERT）+ 字幕区
- 字幕区显示环境音：`(faint · parents arguing in the bedroom, muffled)`

**可点击区域**:
| 区域 | 点击行为 | 跳转 |
|---|---|---|
| **RECALL EVENT ★**（红色） | 调出维护菜单回放 | → Slide 6 模板（内容替换为 17F-03 女儿维护菜单事件） |

---

### Slide 20 · 自宅客厅 · 相框 #1（2023）

**类型**: C 局部浮屏（常驻 DORMANT）  
**触发**: 在客厅拾起沙发旁桌上的旧相框

**包含组件**:
- Slide 38（相框浮卡）+ Slide 28（DORMANT 状态）

**内容**: `2023 / Lily · age 6 / Classroom open day / Front row / Mother in frame`

**可点击区域**:
| 区域 | 点击行为 | 跳转 |
|---|---|---|
| 关闭按钮 / 点外区域 | 放下相框 | → 回到客厅自由探索 |

---

### Slide 21 · 自宅客厅 · 相框 #2（2026 · 今天）

**触发**: 拾起柜子上的新相框

**内容**: `2026.09.15 / Lily · age 9 / Classroom open day · today / Front row / No parent in frame`

**关键对比**：同一场景（教室前排开放日），三年间从"母亲在场"变成"母亲不在场"——这是 Lily 问 Mia "明天你会来吗"的背景。

可点击区域同 Slide 20。

---

### Slide 22 · 体面关闭确认

**类型**: C 局部浮屏 + DORMANT 常驻  
**触发**: 在 Slide 8 点击 A · ANSWER LILY YOURSELF（且信任度 ≥ 阈值）

**包含组件**:
- Slide 40（体面关闭确认对话框）+ Slide 28（DORMANT）+ 顶部时间 + 字幕区

**可点击区域**:
| 区域 | 点击行为 | 跳转 |
|---|---|---|
| **CONFIRM** | 标准关闭 | → 黑屏尾声 A 体面路径 |
| **CANCEL** | 取消 | → Slide 8（返回二选一） |

---

### Slide 23 · 强行关闭警告 · 01 / 03（第一次）

**类型**: C 局部浮屏 + Alert 常驻（橙色）  
**触发**: 在 Slide 8 点击 A（且信任度 < 阈值，强行关闭路径开始）

**说明**：警告语气最轻，内容是劝玩家走标准协议。状态点变为橙色脉冲。

**可点击区域**:
| 区域 | 点击行为 | 跳转 |
|---|---|---|
| **YES · CONTINUE** | 继续 | → **Slide 24**（警告 02/03） |
| **NO · KEEP UNIT ACTIVE** | 放弃强关 | → Slide 8（返回二选一） |

---

### Slide 24 · 强行关闭警告 · 02 / 03（第二次）

**触发**: 在 Slide 23 点击 YES

**说明**：警告升级，提到 Lily 的 6 年成长记录会被破坏。状态点深橙。

**可点击区域**:
| 区域 | 点击行为 | 跳转 |
|---|---|---|
| **YES · CONTINUE** | 继续 | → **Slide 9**（警告 03/03 · 最高级红色脉冲） |
| **NO · KEEP UNIT ACTIVE** | 放弃强关 | → Slide 8（返回二选一） |

---

## 六、完整交互流程

```
┌─────────────────────────────────────────────────────────┐
│                    PHASE 1 · 序幕                        │
└─────────────────────────────────────────────────────────┘
[戴眼镜] ──→ Slide 1 (任务进度 0/3 · 工号 ACTIVE)
              │
              ↓ [走到大厅 · 刷工牌]
            Slide 2 (同步终端)
              │
              ↓ [CONFIRM]
            [电梯简报字幕（基于 Slide 1）]
              │
              ↓ [出电梯进 17F 走廊]
            Slide 1 (走廊 · 任务激活 · 0/3)
              │
              ↓ [走到 17F-01 门口 · 刷工牌]
              ↓
┌─────────────────────────────────────────────────────────┐
│              PHASE 2 · 17F-01 巡检循环                    │
└─────────────────────────────────────────────────────────┘
            Slide 3 (RESIDENT SUMMARY)
              │
              ├─→ ACQUISITION tab ──→ Slide 4
              ├─→ FAMILY LOG tab ──→ [PPT 未做 · Unity 按 Slide 4 布局新建]
              ├─→ TRUST TREND tab ──→ [PPT 未做 · 同上]
              ├─→ INSPECTION HISTORY tab ──→ [PPT 未做 · 同上]
              │
              ↓ [RECALL EVENT ★]
            Slide 6 (陪伴单元第一人称 · 噩梦安抚)
              │
              ↓ [长按 E · 完成回放]
            Slide 5 (处置态 · VIEWED ✓ + A/B)
              │
              ├─→ A · Approve Upgrade ──→ Slide 1 (-1 TRUST · 1/3)
              └─→ B · Enable Observation ──→ Slide 1 (+1 TRUST · 1/3)
              ↓
              ↓ [走到 17F-02 门口 · 刷工牌]
              ↓
┌─────────────────────────────────────────────────────────┐
│         PHASE 3 · 17F-02 巡检循环 (复用 17F-01 模板)       │
└─────────────────────────────────────────────────────────┘
            [借用 Slide 3 模板 · 替换 17F-02 内容]
              │
              ↓
            [借用 Slide 4 模板 · 替换 17F-02 内容]
              │
              ↓ [RECALL EVENT ★]
            [借用 Slide 6 模板 · 17F-02 倾诉+丈夫按开关事件]
              │
              ↓
            [借用 Slide 5 模板 · 17F-02 处置]
              │
              ├─→ A · Maintain Configuration ──→ Slide 1 (-1 · 2/3)
              └─→ B · Recommend Counseling ──→ Slide 1 (+1 · 2/3)
              ↓
              ↓ [22:14 收到预警 · 走到 17F-03]
              ↓
┌─────────────────────────────────────────────────────────┐
│              PHASE 4 · 17F-03 Alert 巡检                  │
└─────────────────────────────────────────────────────────┘
            [状态点变红 · 工号区切换为 ALERT · PENDING REVIEW]
              │
              ↓ [门口 · 自动触发]
            Slide 13 (Alert · RESIDENT SUMMARY)
              │
              ├─→ ACQUISITION tab ──→ Slide 14
              ├─→ FAMILY LOG tab ──→ Slide 15
              ├─→ TRUST TREND tab ──→ Slide 16
              ├─→ INSPECTION HISTORY tab ──→ Slide 17
              │
              ↓ [ENTER UNIT ★ · 入户]
            Slide 19 (屋内侧面板)
              │
              ↓ [RECALL EVENT ★]
            [借用 Slide 6 模板 · 17F-03 维护菜单回放]
              │
              ↓ [长按 E · 完成回放]
            Slide 18 (Alert 处置态 · VIEWED ✓ + A/B)
              │
              ├─→ A · Restart ──→ Slide 1 (-1 · 3/3 · 状态恢复绿色)
              └─→ B · Honor Shutdown ──→ Slide 1 (+1 · 3/3)
              ↓
              ↓ [走回家]
              ↓
┌─────────────────────────────────────────────────────────┐
│                    PHASE 5 · 自宅段                       │
└─────────────────────────────────────────────────────────┘
            Slide 7 (自宅终端 · 状态切换为 DORMANT · 任务栏消失)
              │
              ↓ [进客厅]
            [客厅自由探索]
              │
              ├─→ 拾起旧相框 ──→ Slide 20 ──→ 关闭返回客厅
              ├─→ 拾起新相框 ──→ Slide 21 ──→ 关闭返回客厅
              │
              ↓ [进儿童房 · Lily 问话]
            Slide 8 (终局二选一)
              │
              ├─→ A · ANSWER LILY YOURSELF
              │     │
              │     ├─→ 信任度 ≥ 阈值 ──→ Slide 22 (体面关闭确认)
              │     │                          │
              │     │                          ├─→ CONFIRM ──→ 黑屏尾声 A 体面
              │     │                          └─→ CANCEL ──→ 回 Slide 8
              │     │
              │     └─→ 信任度 < 阈值 ──→ Slide 23 (警告 01/03 · 橙)
              │                                │
              │                                ├─→ YES ──→ Slide 24 (警告 02/03 · 深橙)
              │                                │             │
              │                                │             ├─→ YES ──→ Slide 9 (警告 03/03 · 红)
              │                                │             │             │
              │                                │             │             ├─→ YES · FORCE EXECUTE
              │                                │             │             │     ──→ 黑屏尾声 A 强行
              │                                │             │             └─→ CANCEL ──→ 回 Slide 8
              │                                │             └─→ NO ──→ 回 Slide 8
              │                                └─→ NO ──→ 回 Slide 8
              │
              └─→ B · LET THE COMPANION ANSWER FOR HER
                    │
                    ↓ [字幕过场 · 单元温暖语气 · 工号变 ACTIVE]
                    ↓
                    黑屏尾声 B
              

任意时刻 · 注视左上工号区 0.5s :
    Slide 1 → Slide 10 → Slide 11 → Slide 12
                                       │
                                       ↓ ESC / 点外
                                    回到之前状态
```

---

## 七、复用规则：17F-02 怎么办？

**问题**：17F-02 在 PPT 中**没有专门的页面**。

**原因**：17F-02 与 17F-01 共享同一套页面模板，只是文案内容不同。

**Unity 实现方案**：把 Slides 3 / 4 / 5 / 6 做成**带数据源切换的 Prefab**。运行时根据当前关卡（17F-01 / 17F-02）注入不同的内容。

| 模板 | 17F-01 用法 | 17F-02 用法 |
|---|---|---|
| Slide 3 (门口终端初始态) | 显示 17F-01 的住户摘要 + 标签 | 显示 17F-02 的住户摘要 + 标签（夫妻 / 单方使用 / 切换覆盖等） |
| Slide 4 (ACQUISITION tab) | 显示 17F-01 的购置背景（孩子噩梦） | 显示 17F-02 的购置背景（妻子主动接入） |
| Slide 5 (处置态) | A · Approve Upgrade / B · Enable Observation | A · Maintain Configuration / B · Recommend Counseling |
| Slide 6 (陪伴单元第一人称) | SUBJECT 是孩子 · 噩梦安抚事件 | SUBJECT 是妻子 · 倾诉+丈夫按开关事件 |

**关键差异**：
- 17F-02 中央按钮文案：`[ Comply With Override · Cease Output ]`（玩家被迫"配合关闭自己"）
- 17F-02 屏幕底栏：`COMPANION UNIT · FIRST PERSON · COMPLIANCE MODE`

**17F-03 不复用 17F-01 模板**：因为 17F-03 是 Alert 红色变体（Slides 13–18），需要单独的 Prefab 集。

---

## 八、Unity 需要实现的内容（PPT 没展示的）

PPT 是静态视觉稿，下列内容需要 Unity 实现：

### 8.1 状态点脉冲动画
- 绿色 ACTIVE：缓慢脉冲（约 2 秒一周期）
- 红色 ALERT：急促脉冲（约 0.8 秒一周期）
- 灰色 DORMANT：静止

### 8.2 注视触发圆环
- 玩家注视左上工号区时，工号区周围出现圆环
- 圆环用 0.5 秒逐渐闭合
- 闭合完成 = 触发，跳转到 Slide 11

### 8.3 Tab 切换动画
- 内容区淡出（0.15s）→ 切换数据 → 淡入（0.15s）
- Tab 下划线滑动到新位置（0.3s）

### 8.4 A/B 选项按下后的转场
- 按下后 A/B 选项框短暂高亮（0.2s）
- 右上浮起信任度变化字（`+1 TRUST` / `-1 TRUST`，3 秒后淡出）
- HUD 旁白字幕区切换为新内容（陪伴单元说"下一户"）
- 背景从全屏接管态淡出，回到常驻态（约 0.4s）

### 8.5 入户动画（Slide 13 → Slide 19）
- 门口终端淡出
- 第一人称视角推门进入（约 1.5s）
- 客厅环境音渐起（隔壁卧室父母吵架的低声）
- 走到单元前，Slide 19 侧面板从单元侧面浮起

### 8.6 调出事件回放转场（任何门口终端 → Slide 6）
- 终端 UI 淡出
- 整屏机器边框淡入（约 0.5s）
- 监测面板、决策面板、数据流依次淡入（每个间隔 0.2s）
- 中央按钮最后出现

### 8.7 长按 E 反馈
- 在 Slide 6 中央按钮上长按 E
- 按钮边框逐渐充满（约 1.5 秒充满）
- 充满后回放结束，视角缓慢回到现实

### 8.8 字幕区淡入淡出
- 任何字幕切换都用 0.3 秒淡入 / 0.3 秒淡出
- **不要闪烁**

### 8.9 强行关闭警告的渐进升级
- Slide 23 (01/03) 状态点橙色
- Slide 24 (02/03) 状态点深橙
- Slide 9 (03/03) 状态点红色脉冲
- 每次警告之间字号、警告色都略微加重

### 8.10 黑屏尾声字幕
PPT 中未制作。三种结局（A 体面 / A 强行 / B 数据收尾）需要按以下方式实现：
- 全黑背景
- 文字逐行渐入（每行间隔约 1.5 秒）
- 全部出现后停留 5 秒，然后淡出到 STAFF 字幕

---

## 附录 · PPT 中未制作的页面清单

下列页面需要 Unity 实现时新建，但本说明书未单独编号。布局可参考已有 slide 模板：

| 未做页面 | 参考模板 | 备注 |
|---|---|---|
| 17F-01 / 02 / 03 的 FAMILY LOG / TRUST TREND / INSPECTION HISTORY tab | Slide 4 / 14 | 左右分栏布局，左叙述 / 右数据 |
| 17F-02 整套页面 | Slides 3 / 4 / 5 / 6 | 数据源切换 |
| 17F-02 / 17F-03 的 RECALL EVENT 回放具体内容 | Slide 6 | 替换 SUBJECT / 数据流 / 按钮文案 |
| 工作面板的"DISPOSITION HISTORY"子页 | Slide 12 | 时间线列表 |
| 工作面板的"SYSTEM SETTINGS"子页 | Slide 12 | 偏好设置 |
| 黑屏尾声字幕（A 体面 / A 强行 / B 数据） | — | 全黑 + 渐入文字 |

---

**END OF SPEC**
