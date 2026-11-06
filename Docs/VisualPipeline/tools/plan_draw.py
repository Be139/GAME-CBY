"""Draws each apartment plan from _work/plan_data.txt: walls, doors, new props, kept props, units, actors and anchors, on a 1 m grid."""
import sys

from PIL import Image, ImageDraw, ImageFont

base = sys.argv[1]
rows = [l.split() for l in open(base + "/_work/plan_data.txt", encoding="utf-8") if l.strip()]
font = ImageFont.load_default()
S = 72
scale = {"cor": 26}
areas = {
    "f01": (-7.2, 3.2, -2.6, 7.0),
    "f02": (-7.2, 3.2, -17.0, -5.0),
    "f03": (-19.2, -9.0, -13.6, -1.5),
    "f04": (-10.0, 2.8, -36.5, -24.4),
    "cor": (-21.0, 4.0, -26.0, 13.0),
}
colors = {"new": (70, 150, 255), "old": (150, 150, 150), "door": (255, 70, 70), "unit": (255, 160, 40)}
for name, (u0, u1, v0, v1) in areas.items():
    S = scale.get(name, 72)
    W, H = int((u1 - u0) * S) + 40, int((v1 - v0) * S) + 40
    im = Image.new("RGB", (W, H), (22, 22, 26))
    d = ImageDraw.Draw(im)
    X = lambda u: 30 + (u - u0) * S
    Y = lambda v: H - 30 - (v - v0) * S
    u = int(u0)
    while u <= u1:
        d.line([X(u), 0, X(u), H], fill=(55, 55, 62))
        d.text((X(u) + 2, 2), str(u), fill=(230, 230, 110), font=font)
        u += 1
    v = int(v0)
    while v <= v1:
        d.line([0, Y(v), W, Y(v)], fill=(55, 55, 62))
        d.text((2, Y(v) + 1), str(v), fill=(110, 230, 230), font=font)
        v += 1
    for r in rows:
        if r[0] == "W":
            a, b, c, e = [float(x) for x in r[2:6]]
            col = (235, 235, 235) if r[1] not in ("Mesh21", "Mesh60", "Mesh19", "Mesh53") else (120, 130, 150)
            d.line([X(a), Y(b), X(c), Y(e)], fill=col, width=2)
    for r in rows:
        if r[0] == "P":
            kind, n = r[1], r[2]
            a, b, c, e, y0, y1, yaw = [float(x) for x in r[3:10]]
            if b < u0 or a > u1 or e < v0 or c > v1:
                continue
            if b - a > 8 or e - c > 8:
                continue
            col = colors.get(kind, (200, 200, 200))
            d.rectangle([X(a), Y(e), X(b), Y(c)], outline=col, width=2 if y0 < 0.3 else 1)
            label = n.replace("Prop_", "").replace("Door_2_", "D_")[:17]
            d.text((X(a) + 2, Y(e) + 1), label, fill=col, font=font)
            d.text((X(a) + 2, Y(e) + 11), f"h{y1:.1f}" + (f" b{y0:.1f}" if y0 > 0.1 else ""), fill=(col[0] // 2 + 60, col[1] // 2 + 60, col[2] // 2 + 60), font=font)
        if r[0] == "A":
            n, a, b = r[1], float(r[2]), float(r[3])
            if u0 <= a <= u1 and v0 <= b <= v1:
                d.ellipse([X(a) - 5, Y(b) - 5, X(a) + 5, Y(b) + 5], fill=(255, 80, 220))
                d.text((X(a) + 7, Y(b) - 5), n.replace("Actor_", ""), fill=(255, 120, 230), font=font)
        if r[0] == "N":
            n, a, b = r[1], float(r[2]), float(r[3])
            if u0 <= a <= u1 and v0 <= b <= v1:
                d.ellipse([X(a) - 3, Y(b) - 3, X(a) + 3, Y(b) + 3], fill=(90, 255, 120))
                d.text((X(a) + 5, Y(b) - 5), n[:18], fill=(120, 255, 150), font=font)
    im.save(f"{base}/plans/p_{name}.png")
print("drawn")
