"""Copy generated models and pictures into Assets and derive the lamp glow masks and hue variants."""
import colorsys
import glob
import os
import shutil

import numpy as np
import trimesh
from PIL import Image, ImageDraw, ImageFilter

G = r"E:/GAME-CBY_VisualBackup/generated"
A = r"D:/UGit/GAME-CBY/Assets/Art/Generated/Rooms"
N = r"D:/UGit/GAME-CBY/Assets/Art/Generated/NoahRoom"
os.makedirs(A + "/shared", exist_ok=True)

n = 0
for p in glob.glob(G + "/rooms/*/*.fbx"):
    i = os.path.splitext(os.path.basename(p))[0]
    d = os.path.join(A, i)
    os.makedirs(d, exist_ok=True)
    t = os.path.join(d, i + ".fbx")
    if not os.path.exists(t):
        shutil.copy2(p, t)
        n += 1
print("copied models", n)

# Tripo exports: FBX plus a separate colour map
m = 0
for p in glob.glob(G + "/tripo/*/*.fbx"):
    i = os.path.splitext(os.path.basename(p))[0]
    d = os.path.join(A, i)
    os.makedirs(d, exist_ok=True)
    t = os.path.join(d, i + ".fbx")
    if not os.path.exists(t):
        shutil.copy2(p, t)
        for tex in glob.glob(os.path.dirname(p) + "/texture_0.*"):
            Image.open(tex).convert("RGB").save(os.path.join(d, "texture_0.png"))
        m += 1
print("copied tripo models", m)

for p in glob.glob(G + "/art/*/ref/image_0.png"):
    i = os.path.basename(os.path.dirname(os.path.dirname(p)))
    im = Image.open(p).convert("RGB")
    im.thumbnail((1024, 1024))
    im.save(os.path.join(A, "shared", i + ".png"))
print("pictures done")


def glow_mask(i, cut):
    """Paint the faces above a height fraction into an emission map."""
    src = os.path.join(G, "rooms", i, i + ".glb")
    if not os.path.exists(src):
        print("no glb", i)
        return
    s = trimesh.load(src, force="scene")
    m = next(iter(s.geometry.values()))
    v, f, uv = np.asarray(m.vertices), np.asarray(m.faces), np.asarray(m.visual.uv)
    tex = m.visual.material.baseColorTexture.convert("RGB")
    W, H = tex.size
    y0, y1 = v[:, 1].min(), v[:, 1].max()
    shade = v[f].mean(1)[:, 1] > y0 + (y1 - y0) * cut
    mask = Image.new("L", (W, H), 0)
    d = ImageDraw.Draw(mask)
    for t in f[shade]:
        d.polygon([(float(uv[k, 0]) * W, (1 - float(uv[k, 1])) * H) for k in t], fill=255)
    mask = mask.filter(ImageFilter.MaxFilter(21)).filter(ImageFilter.GaussianBlur(3))
    a = np.asarray(tex).astype(np.float32) / 255
    k = np.asarray(mask).astype(np.float32)[..., None] / 255
    Image.fromarray((a * k * 255).astype(np.uint8)).save(os.path.join(A, i, i + "_emit.png"))
    print("mask", i, int(shade.sum()), "of", len(f))


glow_mask("floor_lamp", 0.72)


def hue_variant(src, out, target_hue=None, sat=1.0, val=1.0):
    """Recolour the saturated parts of a texture; greys and wood stay as they are unless target_hue is None."""
    a = np.asarray(Image.open(src).convert("RGB")).astype(np.float32) / 255
    mx, mn = a.max(2), a.min(2)
    c = mx - mn
    s = np.where(mx > 0, c / (mx + 1e-6), 0)
    v = mx
    h = np.zeros_like(mx)
    r, g, b = a[..., 0], a[..., 1], a[..., 2]
    m1 = (mx == r) & (c > 0)
    m2 = (mx == g) & (c > 0)
    m3 = (mx == b) & (c > 0)
    h[m1] = ((g - b)[m1] / c[m1]) % 6
    h[m2] = (b - r)[m2] / c[m2] + 2
    h[m3] = (r - g)[m3] / c[m3] + 4
    h /= 6
    if target_hue is not None:
        h = np.full_like(h, target_hue)
    s = np.clip(s * sat, 0, 1)
    v = np.clip(v * val, 0, 1)
    i = np.floor(h * 6).astype(int) % 6
    f = h * 6 - np.floor(h * 6)
    p, q, t = v * (1 - s), v * (1 - s * f), v * (1 - s * (1 - f))
    rgb = np.stack([
        np.choose(i, [v, q, p, p, t, v]),
        np.choose(i, [t, v, v, q, p, p]),
        np.choose(i, [p, p, t, v, v, q]),
    ], 2)
    Image.fromarray((rgb * 255).astype(np.uint8)).save(out)


os.makedirs(A + "/variants", exist_ok=True)
for name, src_id, hue, sat, val in [
    ("bed_lily", "bed", 0.95, 0.75, 1.05),
    ("bed_ava", "bed", 0.60, 0.55, 0.95),
    ("curtain_warm", "curtain", 0.08, 0.8, 1.1),
    ("curtain_slate", "curtain", 0.60, 0.5, 0.95),
]:
    hue_variant(os.path.join(N, src_id, "texture_0.png"), os.path.join(A, "variants", name + ".png"), hue, sat, val)
print("variants done")
