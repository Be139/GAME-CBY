"""Textures for the 2026-11-06 pass: the 17F kiosk screen and the city's vertical signs and roof crowns.
Signs follow the research: buildings keep their own colours, the colour comes from small light sources."""
import os
import sys
from PIL import Image, ImageDraw, ImageFilter, ImageFont

out = sys.argv[1]
os.makedirs(out, exist_ok=True)
F = "C:/Windows/Fonts/"


def font(name, size):
    for n in (name, "segoeui.ttf", "arial.ttf"):
        try:
            return ImageFont.truetype(F + n, size)
        except OSError:
            continue
    return ImageFont.load_default()


# kiosk: friendly building service screen, system blue with one warm human accent
W, H = 512, 768
im = Image.new("RGB", (W, H), (12, 24, 38))
d = ImageDraw.Draw(im)
d.rectangle((0, 0, W, 86), fill=(18, 40, 58))
d.text((28, 22), "HEARTH", font=font("bahnschrift.ttf", 40), fill=(130, 201, 221))
d.text((28, 64), "RESIDENCE  ·  FLOOR 17", font=font("bahnschrift.ttf", 16), fill=(162, 185, 197))
d.text((28, 120), "18:42", font=font("bahnschrift.ttf", 112), fill=(232, 240, 244))
d.text((32, 248), "Welcome home.", font=font("segoeui.ttf", 34), fill=(184, 231, 240))
rows = [("Elevators", "A · B   ready"), ("Parcels", "2 waiting"), ("Rooftop garden", "open until 21:00"), ("Quiet hours", "from 22:00")]
y = 330
for k, v in rows:
    d.ellipse((30, y + 10, 46, y + 26), outline=(130, 201, 221), width=3)
    d.text((62, y), k, font=font("segoeui.ttf", 26), fill=(226, 234, 238))
    d.text((62, y + 34), v, font=font("segoeui.ttf", 20), fill=(162, 185, 197))
    d.line((30, y + 72, W - 30, y + 72), fill=(30, 58, 78), width=2)
    y += 88
d.rounded_rectangle((28, H - 92, W - 28, H - 32), radius=14, fill=(46, 36, 30), outline=(228, 184, 139), width=2)
d.text((52, H - 78), "Need anything?  Ask HEARTH", font=font("segoeui.ttf", 24), fill=(228, 184, 139))
im.save(out + "/T_KioskScreen.png")

# vertical signs: stacked letters, glowing tube colour on a dark panel
signs = [("HOTEL", (255, 226, 184)), ("CAFE", (255, 180, 90)), ("NOODLES", (255, 214, 150)), ("CLINIC", (120, 228, 255)),
         ("BOOKS", (255, 236, 210)), ("24H", (255, 110, 90)), ("RAMEN", (255, 196, 120)), ("PHARMA", (130, 236, 210))]
for name, col in signs:
    w, h = 256, 1024
    base = Image.new("RGB", (w, h), (16, 18, 24))
    glow = Image.new("RGB", (w, h), (0, 0, 0))
    gd, bd = ImageDraw.Draw(glow), ImageDraw.Draw(base)
    n = len(name)
    size = min(150, int((h - 120) / n * 0.92))
    f = font("bahnschrift.ttf", size)
    step = (h - 100) / n
    for i, ch in enumerate(name):
        bb = gd.textbbox((0, 0), ch, font=f)
        x = (w - (bb[2] - bb[0])) / 2 - bb[0]
        y = 50 + i * step + (step - (bb[3] - bb[1])) / 2 - bb[1]
        gd.text((x, y), ch, font=f, fill=col)
    halo = glow.filter(ImageFilter.GaussianBlur(14))
    img = Image.blend(base, halo, 0.55)
    ImageDraw.Draw(img).rectangle((6, 6, w - 7, h - 7), outline=tuple(int(c * 0.55) for c in col), width=5)
    img.paste(glow, (0, 0), glow.convert("L").point(lambda p: 255 if p > 40 else 0))
    img.save(out + f"/T_Sign_{name}.png")
print("ok", len(signs) + 1)
