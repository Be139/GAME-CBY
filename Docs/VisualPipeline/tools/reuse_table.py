"""Build the model reuse table from the room specs and write it into the handbook."""
import os
import re

base = r"D:/UGit/GAME-CBY/Docs/VisualPipeline/tools/"
doc = r"D:/UGit/GAME-CBY/Docs/VisualPipeline/室内视觉改造操作手册.md"
spaces = [("s_noah", "Noah"), ("r_f01.cs", "17F01"), ("r_f02.cs", "17F02"), ("r_f03.cs", "17F03"), ("r_f04.cs", "17F04"), ("r_corridor.cs", "走廊"), ("s_lobby.cs", "大堂"), ("r_street.cs", "街道")]
noah = ["bed", "bedside_table", "table_lamp", "ceiling_lamp", "dresser", "toybox", "bookshelf", "desk", "chair", "backpack", "plush_dino", "toy_rocket", "toy_blocks", "globe", "desk_lamp", "book_stack", "pencil_cup", "slippers", "plant", "curtain", "alarm_clock", "photo_frame", "floor_cushion", "laundry_basket", "coat_hooks"]
use = {}
for f, label in spaces:
    ids = set(noah) if f == "s_noah" else set()
    if f != "s_noah":
        s = open(base + f, encoding="utf-8").read()
        ids |= set(re.findall(r'(?:Put|Out|Back|Stand|OnWall)\("([a-z_]+)"', s))
        ids |= set(re.findall(r'Swap\("[^"]+", "([a-z_]+)"', s))
        ids |= set(re.findall(r'On\([^,]+(?:\("[^"]+"\))?, "([a-z_]+)"', s))
        ids |= set(["pendant_lamp"] if "Hang(" in s else [])
        ids |= set(["wall_sconce"] if 'Lit(Put("wall_sconce"' in s else [])
    for i in ids:
        use.setdefault(i, set()).add(label)
labels = [l for _, l in spaces]
rows = sorted(use.items(), key=lambda kv: (-len(kv[1]), kv[0]))
out = ["| 模型 | " + " | ".join(labels) + " | 次数 |", "|---" * (len(labels) + 2) + "|"]
for i, u in rows:
    out.append(f"| `{i}` | " + " | ".join("✓" if l in u else "" for l in labels) + f" | {len(u)} |")
reused = sum(1 for _, u in rows if len(u) > 1)
table = "\n".join(out) + f"\n\n共 {len(rows)} 种模型，其中 {reused} 种用在两个以上空间。"
s = open(doc, encoding="utf-8").read()
if "MODEL_REUSE_TABLE" in s:
    s = s.replace("MODEL_REUSE_TABLE", "<!-- reuse -->\n" + table + "\n<!-- /reuse -->")
else:
    s = re.sub(r"<!-- reuse -->.*<!-- /reuse -->", "<!-- reuse -->\n" + table + "\n<!-- /reuse -->", s, flags=re.S)
open(doc, "w", encoding="utf-8").write(s)
print(len(rows), reused)
for i, u in rows:
    print(i, len(u), end="; ")
