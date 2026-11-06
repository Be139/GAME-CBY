"""Floor textures for the room passes: pale neutral maps that each room tints through its material colour."""
import os

import numpy as np
from PIL import Image, ImageDraw, ImageFilter

out = r"D:/UGit/GAME-CBY/Assets/Art/Generated/Rooms/shared"
os.makedirs(out, exist_ok=True)
rng = np.random.default_rng(7)
S = 1024

# planks: 2 m square tile, 8 boards wide, staggered joints, very gentle per-board shade
im = Image.new("RGB", (S, S), (235, 235, 235))
d = ImageDraw.Draw(im)
n = 8
bw = S / n
for i in range(n):
    x0, x1 = i * bw, (i + 1) * bw
    off = rng.integers(0, S)
    y = -off
    while y < S:
        ln = rng.integers(S // 2, S)
        shade = int(rng.integers(214, 250))
        d.rectangle([x0, y, x1, y + ln], fill=(shade, shade, shade))
        d.line([x0, y, x1, y], fill=(150, 150, 150), width=2)
        y += ln
    d.line([x0, 0, x0, S], fill=(140, 140, 140), width=2)
im.filter(ImageFilter.GaussianBlur(0.6)).save(os.path.join(out, "T_Planks.png"))

# tile: 1.2 m tile set, 2 x 2 tiles with thin joints
im = Image.new("RGB", (S, S), (236, 236, 236))
d = ImageDraw.Draw(im)
for i in range(2):
    for j in range(2):
        shade = int(rng.integers(228, 244))
        d.rectangle([i * S / 2, j * S / 2, (i + 1) * S / 2, (j + 1) * S / 2], fill=(shade, shade, shade))
for k in range(3):
    p = k * S / 2
    d.line([p, 0, p, S], fill=(160, 160, 160), width=3)
    d.line([0, p, S, p], fill=(160, 160, 160), width=3)
im.filter(ImageFilter.GaussianBlur(0.5)).save(os.path.join(out, "T_Tile.png"))

# terrazzo: soft speckles on a pale ground
a = np.full((S, S, 3), 225.0)
im = Image.fromarray(a.astype(np.uint8))
d = ImageDraw.Draw(im)
for count, r0, r1, lo, hi in [(5000, 2, 5, 205, 224), (1200, 3, 7, 190, 212), (2500, 2, 4, 234, 246)]:
    for _ in range(count):
        x, y, r = rng.integers(0, S), rng.integers(0, S), rng.integers(r0, r1)
        c = int(rng.integers(lo, hi))
        d.ellipse([x - r, y - r, x + r, y + r * rng.uniform(0.6, 1.2)], fill=(c, c, c))
im.filter(ImageFilter.GaussianBlur(0.7)).save(os.path.join(out, "T_Terrazzo.png"))
print("textures written")
