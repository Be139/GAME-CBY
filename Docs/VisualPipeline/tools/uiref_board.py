"""Writes the UI reference board page from picks.json and the notes below."""
import html
import json
import sys

base = sys.argv[1]
picks = {p["code"]: p for p in json.load(open(base + "/board/picks.json", encoding="utf-8")) if "error" not in p}

sections = [
    ("A", "眼镜 HUD 与交互提示", "对应：巡检员眼镜 HUD、V2 短按交互提示、开始教程", [
        ("A1", "提示贴着目标物：一个图标、一行动词，不挡视线。", "交互提示"),
        ("A3", "AR 视觉只给目标描边上色，其余信息退到屏幕边角。", "识别高亮"),
        ("A4", "摄像机取景：四角框线、底部一行操作说明。", "HUD 边框"),
        ("A5", "设备本身就是界面，屏幕内容很少。", "实体感"),
    ]),
    ("B", "对白、字幕与选择", "对应：正式对白字幕、Lily 留言、结尾字幕、处置选择", [
        ("B1", "字幕底部居中，半透明底，说话人名在前。", "字幕"),
        ("B2", "通话类对白：人物框和字幕分两层。", "留言 / 语音"),
        ("B3", "字幕下方直接接选项，边框清楚，当前项高亮。", "选择"),
        ("B4", "左下角情绪面板和对白同屏，各占一角。", "情绪 + 对白"),
        ("B5", "右侧整列对话记录，说话人名大写着色。", "完整记录"),
        ("B6", "叙事文字放右栏，人物在左，装饰线少而准。", "记录排版"),
        ("B7", "来电卡片贴在边缘，字幕仍在底部。", "通话"),
        ("B8", "对话时左右切换选项，字幕在下。", "选择"),
    ]),
    ("C", "陪伴单元视角：识别、回放、情绪数据", "对应：陪伴单元 HUD、回放、记录、17F03 检查面板", [
        ("C1", "AR 回放：人物是彩色全息，头顶身份标签，底部时间轴。和 17F02 回放最接近。", "回放"),
        ("C2", "多人同时出现时，标签各自贴在人物上方，不互相遮挡。", "身份标签"),
        ("C3", "AI 摄像头：取景角标、机位编号、状态文字。", "机器在看"),
        ("C4", "场景重建：白色线框人物叠在实景上。", "推演镜头"),
        ("C5", "分析结论：大号结论文字配线框人形。", "检查结论"),
        ("C6", "回放编辑器：时间轴、图层切换、标记点。", "回放控制"),
        ("C7", "识别信息做成贴在脸旁的小卡片，不是大方框。", "识别卡片"),
        ("C8", "状态面板：小字很多，但分区清楚。", "自检 / 状态"),
    ]),
    ("D", "终端与档案", "对应：大堂任务终端、三户门口终端、17F03 告警终端、自宅终端、实体照片档案", [
        ("D1", "实体终端：屏幕内容简单，氛围靠机器外形。", "实体终端"),
        ("D2", "墙面屏幕也在讲环境故事。", "环境屏幕"),
        ("D3", "左列表、右详情、底部状态的终端页。", "终端布局"),
        ("D4", "文件阅读：左目录，右侧大段正文，带机构印章。", "家庭档案"),
        ("D5", "单份文件阅读，正文区很宽，四周留白。", "阅读页"),
        ("D6", "日志：列表加正文，强调色只用在标题线。", "日志"),
        ("D7", "人物资料库：人像加资料卡。", "住户档案"),
        ("D8", "数据库：左侧分类图标，右侧大标题和数字。", "数据库"),
        ("D9", "证件做成真实证件的样子。", "照片档案"),
        ("D10", "物品检查：配色强烈，说明文字很小但可读。", "检查说明"),
    ]),
    ("E", "菜单、设置、警告与结算", "对应：菜单 / 设置 / 关机确认、关机警告、处置结果", [
        ("E1", "设置页：分组、开关、滑块，很干净。", "设置"),
        ("E2", "暂停菜单：标志加竖排选项。", "菜单"),
        ("E3", "章节流程图：把选择和后果画出来。", "处置记录"),
        ("E4", "系统操作：环形步骤加倒计时。", "关机流程"),
        ("E5", "行动卡片：标题、说明、按钮位置每张一致。", "卡片规范"),
        ("E6", "结算页：评分条和数据分层。", "处置结果"),
    ]),
]

cards_html = []
for key, title, maps_to, items in sections:
    cards = []
    for code, note, tag in items:
        p = picks.get(code)
        if not p:
            continue
        cards.append(f'''<button class="card" type="button" id="card-{code}" data-code="{code}" aria-pressed="false">
  <span class="shot"><img src="img/{code}.jpg" alt="{html.escape(p["game"])} 界面截图" loading="lazy" width="{p["w"]}" height="{p["h"]}"></span>
  <span class="meta"><span class="code">{code}</span><span class="game">{html.escape(p["game"])}</span><span class="tag">{tag}</span></span>
  <span class="note">{html.escape(note)}</span>
</button>''')
    cards_html.append(f'''<section class="group" id="sec-{key}">
  <header class="ghead"><span class="gkey">{key}</span><div><h2>{title}</h2><p class="maps">{maps_to}</p></div></header>
  <div class="grid">{"".join(cards)}</div>
</section>''')

sources = {}
for p in picks.values():
    sources.setdefault(p["page"], p["game"] + (" · Steam 商店截图" if p["src"] == "steam" else " · Interface In Game"))
src_html = "".join(f'<li><a href="{u}" target="_blank" rel="noopener">{html.escape(t)}</a></li>' for u, t in sorted(sources.items(), key=lambda x: x[1]))

page = open(base + "/board_template.html", encoding="utf-8").read()
page = page.replace("<!--GROUPS-->", "\n".join(cards_html)).replace("<!--SOURCES-->", src_html).replace("<!--COUNT-->", str(len(picks)))
open(base + "/board/ui_board.html", "w", encoding="utf-8").write(page)
print("cards", len(picks))
