# Noah 房间资产记录

2026-10-31。全部由 Claude Code 通过 Meshy 生成，账号别名 `production-02`。场景位置 `Floor17/Apartment_17F01/VisualPass_NoahRoom`，模型在 `Assets/Art/Generated/NoahRoom/<ID>/`，原始文件和任务记录在 `Docs/VisualPipeline/generated/noah/<ID>/`。

每件的 `task_model.json` 记录模型任务，`ref/image_0.png` 是生成用的单件图，同目录 `image_task.txt`、`model_task.txt` 是任务 ID。

## 风格参考

生成单件图时喂入 `references/style/` 里的三张图：`clay_room.jpg`（你的第 2 张图）、`virginia_home.jpg`、`laststop_study.jpg`。

## 模型

| ID | 物件 | 目标面数 | 实际尺寸（米） | 用途 |
|---|---|---|---|---|
| bed | 单人床 | 1,200 | 1.07 × 0.83 × 2.05 | 替换 `Bed1_02` |
| bedside_table | 床头柜 | 500 | 0.50 × 0.52 × 0.51 | 替换 `Nightstand_01` |
| table_lamp | 台灯 | 400 | 0.29 × 0.42 × 0.29 | 替换 `HomeLamp_04`，灯罩自发光 |
| desk | 书桌 | 900 | 1.25 × 0.73 × 0.71 | 替换 `Desk` |
| chair | 书桌椅 | 600 | 0.47 × 0.80 × 0.49 | 替换 `KidsChair` |
| bookshelf | 矮书架 | 1,500 | 1.55 × 0.96 × 0.46 | 替换 `RoomFurniture_01` |
| dresser | 抽屉柜 | 900 | 0.95 × 0.87 × 0.50 | 新增，窗下 |
| toybox | 玩具箱 | 700 | 0.47 × 0.47 × 0.62 | 新增 |
| backpack | 书包 | 700 | 0.27 × 0.42 × 0.32 | 新增 |
| plush_dino | 恐龙玩偶 | 500 | 0.17 × 0.27 × 0.30 | 新增 |
| toy_rocket | 木火箭 | 400 | 0.16 × 0.30 × 0.16 | 新增，书架上 |
| globe | 地球仪 | 300 | 0.22 × 0.28 × 0.19 | 替换 `EarthMapBall` |
| book_stack | 绘本堆 | 200 | 0.27 × 0.11 × 0.24 | 替换 `Book`，用了两处 |
| pencil_cup | 笔筒 | 200 | 0.08 × 0.15 × 0.08 | 新增 |
| desk_lamp | 书桌灯 | 400 | 0.30 × 0.40 × 0.17 | 替换 `DeskLight`，夜间不亮 |
| slippers | 拖鞋 | 200 | 0.27 × 0.07 × 0.24 | 新增 |
| ceiling_lamp | 吸顶灯 | 200 | 0.42 × 0.07 × 0.42 | 新增，夜间不亮 |
| plant | 落地绿植 | 900 | 高 1.05 | 新增 |
| curtain | 窗帘 | 500 | 高 2.40，用了三处 | 替换 `CurtainSet_19` |
| alarm_clock | 闹钟 | 200 | 0.14 | 新增，床头柜上 |
| photo_frame | 全家福 | 150 | 0.20 | 新增，抽屉柜上 |
| toy_blocks | 积木 | 300 | 0.34 | 新增，地毯上 |
| floor_cushion | 地垫 | 300 | 直径 0.62 | 新增 |
| laundry_basket | 洗衣篮 | 500 | 0.48 | 新增 |
| coat_hooks | 衣钩和小外套 | 500 | 宽 0.80 | 新增，南墙 |

## 2D 素材（`generated/noah_art/`）

| ID | 内容 | 位置 |
|---|---|---|
| art_rocket | 蜡笔画：火箭 | 床头墙 |
| art_robot2 | 蜡笔画：Noah、胶囊形 HEARTH 单元，父母远远站在角落 | 床头墙 |
| art_house | 蜡笔画：公寓楼 | 床头墙 |
| poster_space | 太阳系海报 | 南墙 |
| rug | 地毯图案 | 地面 |

`generated/noah_art/art_robot/` 里的第一版已弃用：画出来的机器人很像《超能陆战队》的大白，不能用。后续生成涉及机器人时，提示词要写明胶囊外形、无脸、无手脚。

## 手工制作

| 物件 | 说明 |
|---|---|
| 内壳、踢脚线、窗台、百叶、窗帘杆 | Unity 方块搭建 |
| 夜色玻璃 | 半透明深蓝面片，只压暗这个房间看到的窗外 |
| 被子 `Prop_Blanket` | 程序生成网格，盖在床的模型上。人物重做后可以直接删除 |
| 地板贴图 `T_FloorPlanks.png` | 程序生成的简单木板 |

## 隐藏的旧物件

`Bed1_02`、`KidsChair`、`Desk`、`Book`、`PuzzleCube`、`DeskLight`、`EarthMapBall`、`RoomFurniture_01`、`Nightstand_01`、`Ball_04`、`HomeLamp_04`、`AcousticGuitar`、`Carpet`、`CurtainSet_19`。这些物件只是隐藏，没有删除。

## 费用

生成单件图每张 12 积分，模型每件 15 积分，2D 素材每张 12 积分。
