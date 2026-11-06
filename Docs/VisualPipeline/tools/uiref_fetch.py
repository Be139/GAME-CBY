"""Downloads the official Steam store screenshots (thumbnails) of the UI reference games and tiles them into labelled contact sheets for picking."""
import json
import os
import sys
import urllib.request
from concurrent.futures import ThreadPoolExecutor
from PIL import Image, ImageDraw

out = sys.argv[1]
games = [(343860, "Tacoma"), (906100, "Observation"), (480490, "Prey"), (1693980, "Dead Space"), (1222140, "Detroit Become Human"),
         (282140, "SOMA"), (214490, "Alien Isolation"), (1578650, "Citizen Sleeper"), (632470, "Disco Elysium"), (870780, "Control"),
         (1091500, "Cyberpunk 2077"), (337000, "Deus Ex Mankind Divided"), (1850570, "Death Stranding DC"), (1386900, "Observer System Redux"),
         (1708010, "The Expanse Telltale"), (794540, "Neo Cab"), (1122690, "Last Stop"), (1262350, "SIGNALIS"), (1238000, "Mass Effect Andromeda")]
os.makedirs(out + "/thumb", exist_ok=True)
UA = {"User-Agent": "Mozilla/5.0 (reference board)"}


def get(url):
    with urllib.request.urlopen(urllib.request.Request(url, headers=UA), timeout=30) as r:
        return r.read()


index = {}
for gi, (app, name) in enumerate(games):
    try:
        d = json.loads(get(f"https://store.steampowered.com/api/appdetails?appids={app}&filters=screenshots&cc=us&l=en"))
        shots = d[str(app)]["data"].get("screenshots", [])[:10]
    except Exception as e:
        print("fail", name, e)
        shots = []
    index[gi] = {"app": app, "name": name, "shots": [{"thumb": s["path_thumbnail"], "full": s["path_full"]} for s in shots]}
json.dump(index, open(out + "/index.json", "w", encoding="utf-8"), indent=1)

jobs = [(gi, si, s["thumb"]) for gi, g in index.items() for si, s in enumerate(g["shots"])]


def dl(job):
    gi, si, url = job
    p = f"{out}/thumb/{gi:02d}_{si:02d}.jpg"
    if not os.path.exists(p):
        try:
            open(p, "wb").write(get(url))
        except Exception as e:
            return f"fail {gi} {si} {e}"
    return None


with ThreadPoolExecutor(8) as ex:
    for r in ex.map(dl, jobs):
        if r:
            print(r)

TW, TH = 320, 180
per_sheet = 4
gids = sorted(index)
for sheet in range(0, len(gids), per_sheet):
    part = gids[sheet:sheet + per_sheet]
    S = Image.new("RGB", (TW * 5, TH * 2 * len(part) + 22 * len(part)), (20, 20, 20))
    d = ImageDraw.Draw(S)
    y = 0
    for gi in part:
        d.text((4, y + 4), f"G{gi:02d} {index[gi]['name']}", fill=(255, 220, 120))
        y += 22
        for si in range(len(index[gi]["shots"])):
            p = f"{out}/thumb/{gi:02d}_{si:02d}.jpg"
            if not os.path.exists(p):
                continue
            im = Image.open(p).convert("RGB").resize((TW, TH))
            x0, y0 = (si % 5) * TW, y + (si // 5) * TH
            S.paste(im, (x0, y0))
            d.rectangle((x0, y0, x0 + 44, y0 + 14), fill=(0, 0, 0))
            d.text((x0 + 3, y0 + 1), f"{gi:02d}-{si:02d}", fill=(255, 255, 0))
        y += TH * 2
    S.save(f"{out}/sheet_{sheet // per_sheet}.jpg", quality=82)
print("games", len(index), "shots", len(jobs))
