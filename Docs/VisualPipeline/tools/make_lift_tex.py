"""Elevator textures: the button panel with its lit map, and the arrow display above the door."""
import os

from PIL import Image, ImageDraw, ImageFont

out = r"D:/UGit/GAME-CBY/Assets/Art/Generated/Rooms/shared"
os.makedirs(out, exist_ok=True)
try:
    font = ImageFont.truetype("arial.ttf", 34)
    small = ImageFont.truetype("arial.ttf", 22)
except OSError:
    font = small = ImageFont.load_default()
W, H = 420, 1200
base = Image.new("RGB", (W, H), (150, 148, 142))
glow = Image.new("RGB", (W, H), (0, 0, 0))
d, g = ImageDraw.Draw(base), ImageDraw.Draw(glow)
for y in range(0, H, 3):
    d.line([0, y, W, y], fill=(146 + (y * 7) % 9, 144 + (y * 7) % 9, 138 + (y * 7) % 9))
d.rectangle([8, 8, W - 9, H - 9], outline=(110, 108, 102), width=3)
# speaker slots
for i in range(7):
    d.rounded_rectangle([120, 40 + i * 14, 300, 46 + i * 14], 3, fill=(60, 60, 60))
# floor buttons, two columns, bottom to top
floors = list(range(1, 19))
for i, f in enumerate(floors):
    col, row = i % 2, i // 2
    cx, cy = 130 + col * 160, 1000 - row * 84
    lit = f in (1, 17)
    d.ellipse([cx - 34, cy - 34, cx + 34, cy + 34], fill=(70, 70, 72), outline=(190, 188, 180), width=3)
    d.text((cx, cy), str(f), fill=(235, 232, 225), font=font, anchor="mm")
    if lit:
        g.ellipse([cx - 34, cy - 34, cx + 34, cy + 34], outline=(255, 190, 120), width=7)
        g.text((cx, cy), str(f), fill=(255, 210, 150), font=font, anchor="mm")
# open, close and alarm
for cx, sym in [(110, "<>"), (210, "><"), (310, "!")]:
    d.ellipse([cx - 30, 1100 - 30, cx + 30, 1100 + 30], fill=(70, 70, 72), outline=(190, 188, 180), width=3)
    d.text((cx, 1100), sym, fill=(235, 232, 225), font=small, anchor="mm")
base.save(os.path.join(out, "T_LiftPanel.png"))
glow.save(os.path.join(out, "T_LiftPanelGlow.png"))

# call panel beside the doors in the lobby and on 17F
base = Image.new("RGB", (240, 420), (150, 148, 142))
glow = Image.new("RGB", (240, 420), (0, 0, 0))
d, g = ImageDraw.Draw(base), ImageDraw.Draw(glow)
d.rectangle([6, 6, 233, 413], outline=(110, 108, 102), width=3)
for cy, up in [(150, True), (280, False)]:
    d.ellipse([70, cy - 50, 170, cy + 50], fill=(70, 70, 72), outline=(190, 188, 180), width=3)
    tri = [(120, cy - 24), (92, cy + 18), (148, cy + 18)] if up else [(120, cy + 24), (92, cy - 18), (148, cy - 18)]
    d.polygon(tri, fill=(235, 232, 225))
    g.ellipse([70, cy - 50, 170, cy + 50], outline=(120, 190, 255), width=6)
    g.polygon(tri, fill=(150, 210, 255))
base.save(os.path.join(out, "T_LiftCall.png"))
glow.save(os.path.join(out, "T_LiftCallGlow.png"))

# arrow display above the doors
img = Image.new("RGB", (512, 160), (10, 12, 16))
d = ImageDraw.Draw(img)
for i in range(3):
    x = 170 + i * 70
    d.polygon([(x, 120), (x + 26, 40), (x + 52, 120)], fill=(255, 176, 96))
img.save(os.path.join(out, "T_LiftDisplay.png"))
print("lift textures written")
