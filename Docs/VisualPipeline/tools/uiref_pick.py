"""Downloads the chosen UI reference screenshots at full size, resizes them for a web page and writes picks.json for the board."""
import json
import os
import sys
import urllib.request
from concurrent.futures import ThreadPoolExecutor
from io import BytesIO
from PIL import Image

base = sys.argv[1]
out = base + "/board"
os.makedirs(out + "/img", exist_ok=True)
idx = json.load(open(base + "/index.json", encoding="utf-8"))
UA = {"User-Agent": "Mozilla/5.0 (reference board)"}
IIG = "https://interfaceingame.com/wp-content/uploads/{g}/{g}-{n}.{ext}"
# (code, source, game, key) ; source steam: key = "gi-si" ; iig: key = slug:name:ext
picks = [
    ("A1", "iig", "Death Stranding", "death-stranding:interact-2:jpg"),
    ("A2", "iig", "Detroit: Become Human", "detroit-become-human:objectives:jpg"),
    ("A3", "iig", "Deus Ex: Mankind Divided", "deus-ex-mankind-divided:smart-vision-enemies:jpg"),
    ("A4", "steam", "Observation", "01-08"),
    ("A5", "steam", "Alien: Isolation", "06-03"),
    ("B1", "iig", "Death Stranding", "death-stranding:subtitles-3:jpg"),
    ("B2", "iig", "Death Stranding", "death-stranding:codec-2:jpg"),
    ("B3", "steam", "The Expanse: A Telltale Series", "14-06"),
    ("B4", "steam", "Neo Cab", "15-03"),
    ("B5", "steam", "Disco Elysium", "08-05"),
    ("B6", "steam", "Citizen Sleeper", "07-00"),
    ("B7", "iig", "Cyberpunk 2077", "cyberpunk-2077:call:jpg"),
    ("B8", "iig", "Deus Ex: Mankind Divided", "deus-ex-mankind-divided:dialogue:jpg"),
    ("C1", "steam", "Tacoma", "00-02"),
    ("C2", "steam", "Tacoma", "00-04"),
    ("C3", "steam", "Observation", "01-03"),
    ("C4", "steam", "Detroit: Become Human", "04-02"),
    ("C5", "iig", "Detroit: Become Human", "detroit-become-human:analysis-conclusion:jpg"),
    ("C6", "iig", "Cyberpunk 2077", "cyberpunk-2077:braindance-editor:jpg"),
    ("C7", "steam", "Neo Cab", "15-05"),
    ("C8", "steam", "SIGNALIS", "17-01"),
    ("D1", "steam", "Alien: Isolation", "06-01"),
    ("D2", "steam", "Observer: System Redux", "13-04"),
    ("D3", "iig", "Death Stranding", "death-stranding:delivery-terminal-2:jpg"),
    ("D4", "iig", "Control", "control:correspondence:jpg"),
    ("D5", "iig", "Control", "control:read-collectible:jpg"),
    ("D6", "iig", "Cyberpunk 2077", "cyberpunk-2077:journal:jpg"),
    ("D7", "iig", "Cyberpunk 2077", "cyberpunk-2077:character-database:jpg"),
    ("D8", "iig", "Deus Ex: Mankind Divided", "deus-ex-mankind-divided:database:jpg"),
    ("D9", "steam", "Tacoma", "00-01"),
    ("D10", "steam", "SIGNALIS", "17-02"),
    ("E1", "iig", "Control", "control:display:jpg"),
    ("E2", "iig", "Cyberpunk 2077", "cyberpunk-2077:pause:jpg"),
    ("E3", "iig", "Detroit: Become Human", "detroit-become-human:chapters:jpg"),
    ("E4", "steam", "Observation", "01-02"),
    ("E5", "steam", "Citizen Sleeper", "07-02"),
    ("E6", "iig", "Death Stranding", "death-stranding:results-2:jpg"),
]


def get(url):
    with urllib.request.urlopen(urllib.request.Request(url, headers=UA), timeout=40) as r:
        return r.read()


def fetch(p):
    code, src, game, key = p
    try:
        if src == "steam":
            gi, si = key.split("-")
            url = idx[str(int(gi))]["shots"][int(si)]["full"]
            page = f"https://store.steampowered.com/app/{idx[str(int(gi))]['app']}/"
        else:
            g, n, ext = key.split(":")
            url = IIG.format(g=g, n=n, ext=ext)
            try:
                data = get(url)
            except Exception:
                url = url.rsplit(".", 1)[0] + (".png" if ext == "jpg" else ".jpg")
                data = None
            page = f"https://interfaceingame.com/games/{g}/"
        data = get(url) if src == "steam" or data is None else data
        im = Image.open(BytesIO(data)).convert("RGB")
        im.thumbnail((1280, 720))
        im.save(f"{out}/img/{code}.jpg", quality=82)
        return {"code": code, "game": game, "src": src, "url": url, "page": page, "w": im.width, "h": im.height}
    except Exception as e:
        return {"code": code, "game": game, "error": str(e)}


with ThreadPoolExecutor(6) as ex:
    res = list(ex.map(fetch, picks))
json.dump(res, open(out + "/picks.json", "w", encoding="utf-8"), ensure_ascii=False, indent=1)
bad = [r for r in res if "error" in r]
print("ok", len(res) - len(bad), "failed", [(b["code"], b["error"][:60]) for b in bad])
