"""Overlay the floor coordinate grid (u right, v up) on the aligned plan."""
import sys
from PIL import Image, ImageDraw, ImageFont

base = sys.argv[1]
font = ImageFont.load_default()
im = Image.open(f"{base}/uv.jpg").convert("RGB")
w, h = im.size
half = 24.0
cu, cv = -6.0, -18.0
px = w / (2 * half)
d = ImageDraw.Draw(im)
def sx(u):
    return (u - (cu - half)) * px
def sy(v):
    return h - (v - (cv - half)) * px
for i in range(int(cu - half), int(cu + half) + 1):
    major = i % 5 == 0
    d.line([(sx(i), 0), (sx(i), h)], fill=(230, 230, 80) if major else (75, 75, 45), width=1)
    if major:
        d.text((sx(i) + 2, 2), str(i), fill=(255, 255, 90), font=font)
for j in range(int(cv - half), int(cv + half) + 1):
    major = j % 5 == 0
    d.line([(0, sy(j)), (w, sy(j))], fill=(80, 230, 230) if major else (45, 75, 75), width=1)
    if major:
        d.text((2, sy(j) + 2), str(j), fill=(90, 255, 255), font=font)
im.save(f"{base}/uv_grid.jpg", quality=84)
print("ok")
