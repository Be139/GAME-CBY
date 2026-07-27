# HEARTH 下一阶段：UI 风格参考方案

> 这些参考用于提炼信息层级、动效和交互语法，不用于直接临摹某个游戏的界面。

## 一、HEARTH 的视觉关键词

- 近未来家居。
- 企业服务系统。
- 温和但持续监控。
- 人类情感与机器判断并存。
- 不是军事 HUD。
- 不是赛博朋克霓虹酒吧。
- 不是纯恐怖故障界面。
- 不是充满装饰数据的“假科技屏”。

因此，UI 应表现为：

`克制、可信、清晰、带一点令人不安的制度感`

## 二、可参考作品

推荐搜索关键词：

| 方向 | 搜索关键词 |
|---|---|
| 人物对白 | `Citizen Sleeper dialogue UI`, `narrative game bottom dialogue panel` |
| 移动中通讯 | `Firewatch radio dialogue UI`, `Oxenfree walk and talk UI` |
| 终端 | `Observation game interface`, `diegetic sci-fi terminal UI` |
| 回放 | `Tacoma AR recording UI`, `archived playback game interface` |
| 调查与选择 | `Detroit Become Human investigation UI`, `ethical choice game UI` |
| 家居科技 | `soft corporate futurism interface`, `near future domestic AI UI` |

### Citizen Sleeper

[Citizen Sleeper - Steam](https://store.steampowered.com/app/1578650/Citizen_Sleeper/)

适合参考：

- 对白、角色与选择的清楚层级。
- 叙事文本与科幻背景的结合。
- 大块信息不依赖复杂装饰也能形成风格。

不应照搬：

- 桌游骰子结构。
- 强插画角色占屏。

### Observation

[Observation - Official Site](https://observationgame.com/)

适合参考：

- 玩家作为 AI/系统时的界面身份。
- 机器视角、节点连接、诊断与故障感。
- 画面中的系统信息具有功能，而不是纯装饰。

不应照搬：

- 太强的太空站恐怖感。
- 过暗、过多噪点导致家庭空间难以观察。

### Tacoma

[Fullbright: Tacoma 的 AR 设计说明](https://blog.fullbrig.ht/)

适合参考：

- 过去记录叠加在现实空间上的“存档播放”语法。
- 时间、角色与环境信息在 AR 中统一。
- 数字信息像真实世界系统的一部分。

不应照搬：

- 彩色人物轮廓。
- 过多空间 AR 标签。

### Detroit: Become Human

[Detroit: Become Human - Quantic Dream](https://www.quanticdream.com/en/detroit-become-human)

适合参考：

- 调查、证据与选择之间的因果关系。
- 选择不是突然出现，而是建立在观察之后。
- 不同角色/机器身份使用不同信息视觉。

不应照搬：

- 电影级大制作复杂度。
- 大量分支图和 QTE。

### OXENFREE / OXENFREE II

[OXENFREE - Night School Studio](https://nightschoolstudio.com/oxenfree)  
[OXENFREE II - Press Kit](https://nightschoolstudio.com/press-kit/oxenfree-ii-press-kit/)

适合参考：

- 移动中对话。
- 对话与探索不必总是互相打断。
- 通讯可以从任何位置自然发生。

不应照搬：

- 头顶气泡式选项。
- 青少年手绘风格。

### Firewatch

[Firewatch - About](https://www.firewatchgame.com/about/)

适合参考：

- 远程通讯成为叙事伙伴。
- 玩家探索时仍能和一个看不见的角色建立关系。
- Field Unit 可以借鉴“无线电伙伴”的节奏，而不是每次都进入正式对话。

## 三、方案 A：Soft Corporate Domestic Futurism

### 核心感觉

温和、可信、家居化的企业科技。第一眼不危险，但随着信息积累产生不安。

### 色彩

- 深炭黑：背景与遮罩。
- 冷白：正文。
- 柔和青蓝：系统可用状态。
- 低饱和绿色：推荐或安全。
- 克制橙红：警告。

### 形状

- 细线。
- 小切角。
- 低圆角。
- 大留白。
- 少量玻璃层，不使用大片黑色盒子。

### 字体

- 人物对白：易读的人文无衬线。
- 系统：中性窄体无衬线。
- 数字：等宽字体只用于时间戳和编号。

### 动效

- 0.15-0.3 秒柔和淡入。
- 轻微扫描或焦点锁定。
- 不频繁闪烁。
- 警告时才破坏稳定布局。

### 优点

- 最符合“家庭陪伴产品”的设定。
- 人类场景和机器系统可以共存。
- 长时间阅读不疲劳。

### 风险

- 如果对比度和层级不足，会显得平淡。

### 适用

推荐作为 HEARTH 的主方向。

### 搜索关键词

`soft corporate futurism UI`, `domestic AI interface`, `calm sci-fi HUD`, `near future home operating system`

## 四、方案 B：Clinical Archive System

### 核心感觉

冷静、精密、记录一切的企业档案系统。

### 色彩

- 黑蓝。
- 冰蓝。
- 灰白。
- 琥珀色回放。
- 红色故障。

### 形状

- 模块化网格。
- 明确时间轴。
- 档案标签。
- 状态条。
- 数据块之间严格对齐。

### 动效

- 页面像系统模块装载。
- 回放使用扫描线、时间码和片段跳转。
- 故障时出现短暂数据错位。

### 优点

- 终端和回放身份非常清楚。
- 适合 17F03 警告与机器人视角。

### 风险

- 容易回到当前“信息太多、像 PPT”的问题。
- 如果全局采用，会削弱家庭情感。

### 适用

建议作为终端、回放和故障状态的次级语言。

### 搜索关键词

`clinical archive UI`, `AI diagnostic interface`, `archived playback HUD`, `sci-fi evidence terminal`

## 五、方案 C：Warm Human / Cold Machine Contrast

### 核心感觉

人类对白温暖、柔软；机器界面冷静、锋利。通过反差表达主题。

### 人类层

- 更宽的对白区域。
- 柔和半透明背景。
- 更大的文字。
- 温和暖白。
- 减少线框。

### 机器层

- 小字号系统标签。
- 青蓝细线。
- 等宽数字。
- 明确状态与时间戳。
- 更快、更精确的动效。

### 决策层

在两者中间：

- 机器提供推荐。
- 玩家承担最终选择。
- 推荐色与玩家高亮必须不同。

### 优点

- 主题表达最强。
- 玩家很容易区分“人说话”与“系统判断”。

### 风险

- 如果使用两套完全不同的组件，会增加维护成本。

### 适用

建议作为方案 A 的叙事增强，而不是完全独立的第三套系统。

### 搜索关键词

`human machine UI contrast`, `warm dialogue cold system interface`, `narrative sci-fi choice UI`

## 六、推荐组合

推荐：

`方案 A 作为全局基础 + 方案 B 用于终端/回放 + 方案 C 用于人机层级差异`

具体分配：

| 系统 | 视觉方向 |
|---|---|
| 人类正式对白 | A + C 的人类层 |
| Mia 自言自语 | A 的轻量版 |
| Field Unit | A + C 的机器层 |
| 左侧任务 | A |
| 终端 | A 的框架 + B 的档案结构 |
| 回放 | B |
| 陪伴单元 HUD | B，但减少常驻信息 |
| A/B 决策 | A + C |
| 低信任故障 | B 的失控版本 |

## 七、统一视觉规则

### 文字

- 正式对白正文优先大于装饰标签。
- 每句最多两行。
- 说话人不与正文同字号。
- 系统标签统一大写，人物名字不强制大写。
- 不使用无功能的随机代码。

### 面板

- 正文需要可读衬底。
- 衬底只覆盖文字安全区，不覆盖整个屏幕。
- 不在卡片中再嵌套卡片。
- 相同层级的面板边框粗细一致。

### 高亮

- 当前选择：亮度/底色。
- Field Unit 推荐：小标签。
- 警告：颜色 + 图标 +声音，不只依赖红色。
- 已提交：变灰并禁用。

### 动效

- 普通 UI：150-300ms。
- 终端镜头：约 500ms。
- 回放进入：500-800ms。
- 警告和故障：允许不稳定，但必须可关闭或继续。

## 八、可访问性约束

[Xbox Accessibility Guidelines](https://learn.microsoft.com/en-us/xbox/accessibility/guidelines) 将文字显示、对比度、字幕、输入、UI 导航、焦点和时间限制分开检查。HEARTH 的视觉方案应从一开始支持：

- 字幕字号调整。
- 背景透明度调整。
- 键位提示不写死。
- 不只用颜色传达状态。
- 大字幕时不遮挡交互提示。
- 时间限制玩法提供合理容错。

## 九、视觉确认交付物

正式进入 Unity 前，至少确认以下五张：

1. 人物正式对白。
2. Field Unit + 任务 HUD。
3. 17F 终端 Summary。
4. Archived Playback。
5. 独立 A/B 决策。

这五张确定后，其余状态才能可靠沿用同一套视觉语法。
