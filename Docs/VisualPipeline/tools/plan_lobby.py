"""Draws the lobby and site structure from _work/plan_lobby.txt."""
import sys

from PIL import Image, ImageDraw, ImageFont

base = sys.argv[1]
rows = [l.split() for l in open(base + "/_work/plan_lobby.txt", encoding="utf-8") if l.strip()]
font = ImageFont.load_default()
S = 16
u0, u1, v0, v1 = -42.0, 42.0, -14.0, 66.0
W, H = int((u1 - u0) * S) + 30, int((v1 - v0) * S) + 30
im = Image.new("RGB", (W, H), (20, 20, 24))
d = ImageDraw.Draw(im)
X = lambda u: 20 + (u - u0) * S
Y = lambda v: H - 20 - (v - v0) * S
for u in range(int(u0), int(u1) + 1, 5):
    d.line([X(u), 0, X(u), H], fill=(70, 70, 45) if u % 10 == 0 else (42, 42, 48))
    if u % 10 == 0:
        d.text((X(u) + 2, 2), str(u), fill=(230, 230, 110), font=font)
for v in range(int(v0), int(v1) + 1, 5):
    d.line([0, Y(v), W, Y(v)], fill=(45, 70, 70) if v % 10 == 0 else (42, 42, 48))
    if v % 10 == 0:
        d.text((2, Y(v) + 1), str(v), fill=(110, 230, 230), font=font)
for r in rows:
    if r[0] == "W":
        a, b, c, e = [float(x) for x in r[2:6]]
        d.line([X(a), Y(b), X(c), Y(e)], fill=(240, 240, 240) if r[1] == "S" else (130, 150, 190), width=1)
    if r[0] == "O":
        n, c, e, su, sv = r[1], float(r[2]), float(r[3]), float(r[4]), float(r[5])
        if su < 12 and sv < 12:
            d.rectangle([X(c - su / 2), Y(e + sv / 2), X(c + su / 2), Y(e - sv / 2)], outline=(255, 150, 60))
d.ellipse([X(14.3) - 5, Y(25.7) - 5, X(14.3) + 5, Y(25.7) + 5], fill=(90, 255, 120))
d.text((X(14.3) + 7, Y(25.7) - 5), "spawn", fill=(90, 255, 120), font=font)
d.ellipse([X(-7.2) - 5, Y(11.2) - 5, X(-7.2) + 5, Y(11.2) + 5], fill=(255, 90, 200))
d.text((X(-7.2) + 7, Y(11.2) - 5), "lift", fill=(255, 90, 200), font=font)
im.save(base + "/plans/p_lobby.png")
print("drawn", W, H)
