"""Before/after sheet: one row per view, the frame before this pass on the left and after it on the right."""
import sys
from PIL import Image, ImageDraw

d, out, names = sys.argv[1], sys.argv[2], sys.argv[3].split(",")
W, H = 800, 450
S = Image.new("RGB", (W * 2 + 12, len(names) * (H + 8)), (14, 18, 24))
dr = ImageDraw.Draw(S)
for i, n in enumerate(names):
    y = i * (H + 8)
    for k, sub in enumerate(("views_before", "views")):
        try:
            im = Image.open(f"{d}/{sub}/{n}.jpg").convert("RGB").resize((W, H))
        except OSError:
            continue
        S.paste(im, (k * (W + 12), y))
        tag = f"{n}  {'before' if k == 0 else 'after'}"
        dr.rectangle((k * (W + 12), y, k * (W + 12) + 150, y + 18), fill=(0, 0, 0))
        dr.text((k * (W + 12) + 5, y + 3), tag, fill=(255, 220, 120))
S.save(out, quality=86)
print("sheet", out)
