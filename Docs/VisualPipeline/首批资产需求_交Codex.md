# 首批资产需求

2026-10-31，Claude Code 根据两段录像、语音解说、用户反馈和实时场景整理。范围是试点区：17 层走廊和 17F01 客厅，加上全程出镜的家用单元。

**这份清单已过期，只保留作尺寸和命令参考。** 用户选定的参考是 `references/` 7 张图加 Virginia、Last Stop，模型由 Claude Code 直接通过 Meshy 生成。现行做法见 [房间视觉制作流程](房间视觉制作流程.md)，Noah 房间实际用的资产见 [Noah 房间资产记录](Noah房间资产记录.md)。

## 分工

- 风格帧、单体参考图：Codex 生成，不扣 Meshy 积分。备选是 Meshy CLI 的图生图，每次 3–12 积分。
- 模型：默认由 Claude Code 直接运行 Meshy。Codex 也可以运行；Tripo 插件只有 Codex 能用。
- 摆放、灯光、材质、回归：Claude Code。
- 用户：选对标、看成片、购买工具和补充额度、完成浏览器登录，可随时追加参考图。

## 规则

- 模型生成后直接用，不做减面、调色板或风格归一，也不逐件检验。摆进场景明显不合适才重新生成。
- 面数在生成时设上限：大件 15,000，中件 5,000，小件 1,500。
- 所有参考图都带上同一组已确认的风格帧，减少风格漂移。
- 文件放到 `Docs/VisualPipeline/generated/<资产ID>/`。

## 第 1 步：风格帧

每个空间出 2 张给用户选。输入图用 `lookdev/now_lobby.jpg`（大堂）、`lookdev/base_corridor.jpg`（走廊）、`lookdev/base_living.jpg`（客厅），对标游戏的官方截图从参考板取，作为风格参考。

固定要求：保持房间结构、家具位置和人物姿势，只提升表面、光影和建筑细节；不要添加文字和品牌标识；不要改变人物数量。

| 空间 | 图生图提示词 |
|---|---|
| 大堂 | `Repaint as a stylized realistic game interior. A calm corporate residential lobby with warm cove lighting in a sculpted ceiling, a large curved reception desk, tidy seating groups, planters, clear wayfinding signs and a two-tone floor. Clean painted surfaces, soft baked light, quiet and slightly too perfect, allegorical mood.` |
| 走廊 | `Repaint as a stylized realistic game interior. A residential hotel-like corridor with painted wainscoting, baseboards, framed doors with number plates, warm wall sconces, slim ceiling lights, framed art and a carpet runner. Clean colour-blocked walls, soft baked light, welcoming but slightly uncanny.` |
| 客厅 | `Repaint as a stylized realistic game interior. A tidy lived-in near-future family apartment at dusk. Handcrafted furniture with true proportions, soft textiles, framed photos, warm pendant lamp over the dining table, cool city light through the window. Soft baked light, intimate and quiet.` |

## 第 2 步：单体参考图

单个物体，完整入镜，无遮挡，纯中性灰背景，均匀布光，无投影，无文字，无标识。硬表面件额外出正、侧、背三视图。

**风格尾缀**：`stylized realistic game asset, clean painted PBR surfaces, soft bevelled edges, simple readable shapes, warm muted palette`

**排除项**：`text, logo, watermark, baked lighting, hard cast shadow, heavy grime, scratches, cluttered background, people, hands, cropped object`

如果用户换了对标，只替换风格尾缀：

| 对标 | 尾缀 |
|---|---|
| Life is Strange | `hand-painted textures, impressionistic brushwork, warm nostalgic colours` |
| Before Your Eyes | `stylized toon game asset, rounded simple forms, soft illustrated colours` |
| American Arcadia | `1970s retrofuturist design, bold rounded shapes, orange and teal palette` |
| Virginia | `low poly, flat shading, solid matte colour blocks` |

## 资产清单

尺寸单位为米，写作 宽 × 高 × 深。原点默认在底部中心，正面朝 +Z。档位对应上面的面数上限。

### 家用单元

**HU-01 家用陪伴单元**（大件，最高优先）

- 目标：`HouseholdActors/HomeUnit_17F04`、`HouseholdActors/CompanionUnit_17F03`、`Lobby/ROBOT (2)` 到 `(7)` 的可视子物体。
- 尺寸：约 0.80 × 1.43 × 0.95。分机身、前面板、双耳、挡泥板、轮子。
- 保留：`Camera (1)`、`PhysicalBodyCollider_*`、`InteractionVolume_*`。场景索引未发现脚本字段引用 6 个可视网格，关机演出替换后要回归。
- 不绑定骨骼。用正、侧、背三视图走多图建模。
- 提示词：`A friendly domestic companion robot appliance for a near-future home. Tall rounded capsule body like a premium kitchen appliance, smooth off-white ceramic shell, one large soft-cornered oval panel on the front with a thin seam line, two short angled antenna ears on top with warm coral rings, two side wheels with matte charcoal tyres and simple curved fender covers. Calm and trustworthy. No face, no eyes.`
- 备注：现有机身用了墙面石膏材质 `RM Plaster smooth`，基础修复时先换干净材质过渡。

### 走廊建筑细节件

| ID | 名称 | 尺寸 | 档位 | 提示词 |
|---|---|---|---|---|
| CO-01 | 护墙板模块 | 1.20 × 1.00 × 0.05 | 中 | `painted wooden wainscoting wall panel module with simple raised moulding and a top rail` |
| CO-02 | 踢脚线和墙角收边 | 1.20 × 0.15 × 0.03 | 小 | `simple painted skirting board segment` |
| CO-03 | 入户门带门套 | 1.00 × 2.25 × 0.12 | 中 | `modern apartment entrance door with painted frame, simple vertical handle and a small recessed number plate` |
| CO-04 | 门牌和状态灯 | 0.22 × 0.12 × 0.02 | 小 | `slim apartment number plate with a tiny status light, blank surface` |
| CO-05 | 线型顶灯 | 1.20 × 0.05 × 0.12 | 小 | `slim linear ceiling light fixture with a soft diffuser strip` |
| CO-06 | 壁灯 | 0.12 × 0.32 × 0.10 | 小 | `small residential corridor wall sconce with warm opal glass` |
| CO-07 | 挂画 | 0.60 × 0.80 × 0.03 | 小 | `simple framed abstract landscape print` |
| CO-08 | 消防箱 | 0.60 × 0.80 × 0.15 | 小 | `wall-mounted fire hose cabinet, blank door` |
| CO-09 | 走廊地毯 | 1.40 × 0.01 × 3.00 | 小 | `plain carpet runner with a thin border pattern` |
| CO-10 | 门口终端外壳 | 0.96 × 0.61 × 0.06 | 中 | `wall-mounted residential service terminal, slim bezel, warm grey housing, small speaker grille, blank flat screen` |
| CO-11 | 端部防火门 | 1.60 × 2.30 × 0.12 | 中 | `double fire door for a residential corridor with narrow vision panels and push bars` |

- CO-03 替换 `Door_2_Brown` 等门的可视部分，保留转轴、开门方向、Collider 和脚本引用，原点放在铰链侧底部。
- CO-10 保留实体屏幕、Collider、交互组件和固定相机，屏幕尺寸以 `InspectionTerminal_17F01` 现状为准。
- CO-11 位置在试点时现场确认（录像 B 05:06）。

### 17F01 客厅

| ID | 名称 | 尺寸 | 档位 | 提示词 |
|---|---|---|---|---|
| RM-01 | 餐桌 | 1.60 × 0.75 × 0.90 | 中 | `simple solid wood family dining table` |
| RM-02 | 餐椅 | 0.45 × 0.85 × 0.50 | 中 | `simple upholstered dining chair` |
| RM-03 | 吊灯 | 直径 0.55，高 0.30，含 1.0 吊线 | 小 | `simple dome pendant lamp with fabric cord` |
| RM-04 | 沙发 | 2.10 × 0.85 × 0.90 | 大 | `comfortable three-seat fabric sofa, slightly worn` |
| RM-05 | 毯子和抱枕 | 1.30 × 0.15 × 0.80 | 中 | `folded soft throw blanket with two cushions` |
| RM-06 | 电视柜 | 1.80 × 0.55 × 0.45 | 中 | `low media cabinet, warm wood with simple doors` |
| RM-07 | 全家福相框组 | 0.60 × 0.50 × 0.03 | 小 | `small group of framed family photographs, blank pictures` |
| RM-08 | 餐具组 | 约 0.26 | 小 | `plain dinner plates, mugs, child's lunch box, opened envelope`，分件交付 |
| RM-09 | 落地灯 | 0.40 × 1.50 × 0.40 | 小 | `simple fabric floor lamp` |

- 现有餐桌被拉伸到 3.09 倍，这批按真实比例生成，再由 Claude Code 对齐演员座位和路线。
- `Actor_Daniel_17F01`、`Actor_Emily_17F01` 的坐姿高度依赖椅子和桌子，替换后要回归。

### 2D 素材

- **DC-01 贴花组**（1024 × 1024，透明 PNG）：地面磨痕、墙面污渍、门框上的儿童身高铅笔刻度、蜡笔画、便利贴。
- **DC-02 HEARTH 待机画面**（1920 × 1080）：先出图给用户看，确认后再接入。

### 不走 AI 建模

**EL-01 电梯轿厢和电梯门**：空心结构由 Claude Code 在 Unity 里搭建。只需控制面板和楼层显示的平面素材。尺寸按轿厢 2.0 × 2.4 × 2.2、门洞 1.1 × 2.2。

## 建模命令

本机没有全局 `meshy`，用固定版本运行：

```bash
npm.cmd exec --yes --package=meshy-cli@0.4.0 -- meshy image-to-3d create --image-url "参考图路径" --model-type smart-topology --target-polycount 5000 --should-texture true --enable-pbr true --target-formats glb --async --output-schema v1 --format json --no-update-check
```

- `smart-topology` 就是 `meshy-t2`，100–15,000 三角形，分件清晰。它和 PBR 贴图的组合还没实测，先提交一件确认，再批量。
- 价目表（2026-10-31 读取）：`meshy-t2` 带 2K 贴图 15 积分，仅网格 5；完整模型 `meshy-7.1` 带 2K 贴图 30 积分；绑定 5 积分。实际扣费以任务返回的 `consumed_credits` 为准。
- 非企业 API 的输出只保留 3 天，生成后立刻下载。

## 账号

- **Meshy**：`default` 余额 1,354，`production-02` 余额 1,322，当前活动账号是 `production-02`。用 `meshy auth list` 查看，`meshy auth use <名字>` 切换。切换影响本机所有任务，并行时不要切。
- **Tripo**：用户协议第 2.2 条规定每个平台不得持有多于一个账号，所以只用一个账号。免费用户的输出权利归 Tripo，商业使用要用付费账号。
- 凭据只留在工具的授权存储，文档只写非秘密别名。

## 回传格式

每件资产写一个 `generated/<资产ID>/record.md`：最终提示词和参考图路径、平台、账号别名、任务 ID、实际扣费、模型和贴图路径、实际格式、尺寸和面数。
