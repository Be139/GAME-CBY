"""Post-process previews from Unity renders. Not a game pipeline; only used for the style comparison sheet."""
import os
import sys

import numpy as np
from PIL import Image, ImageFilter

d = os.path.join(sys.argv[1], "lookdev")
views = ["V1corridor", "V2living", "V3noah"]
LUM = np.array([0.299, 0.587, 0.114], dtype=np.float32)


def load(name):
    return np.asarray(Image.open(os.path.join(d, name)).convert("RGB")).astype(np.float32) / 255.0


def to_img(a):
    return Image.fromarray((np.clip(a, 0, 1) * 255).astype(np.uint8))


def save(arr, name):
    to_img(arr).save(os.path.join(d, name))


def box(a, r):
    a2 = a if a.ndim == 3 else a[..., None]
    pad = np.pad(a2, ((r + 1, r), (r + 1, r), (0, 0)), mode="edge")
    ii = pad.cumsum(0).cumsum(1)
    k = 2 * r + 1
    out = (ii[k:, k:] - ii[:-k, k:] - ii[k:, :-k] + ii[:-k, :-k]) / (k * k)
    return out if a.ndim == 3 else out[..., 0]


def kuwahara(img, r):
    """Standard 4-quadrant Kuwahara filter, the same maths a post-process shader would run."""
    lum = img @ LUM
    means, vars_ = [], []
    for sx, sy in ((-1, -1), (1, -1), (-1, 1), (1, 1)):
        shift = (sy * (r // 2 + 1), sx * (r // 2 + 1))
        ml = box(lum, r)
        means.append(np.roll(box(img, r), shift, (0, 1)))
        vars_.append(np.roll(box(lum * lum, r) - ml * ml, shift, (0, 1)))
    means = np.stack(means, 0)
    vars_ = np.stack(vars_, 0)
    idx = vars_.argmin(0)
    h, w = idx.shape
    yy, xx = np.mgrid[0:h, 0:w]
    return means[idx, yy, xx]


def edge_mask(img, thresh):
    col = np.abs(np.roll(img, -1, 1) - np.roll(img, 1, 1)).sum(2) + np.abs(np.roll(img, -1, 0) - np.roll(img, 1, 0)).sum(2)
    m = (col > thresh).astype(np.float32)
    return np.asarray(to_img(m).filter(ImageFilter.GaussianBlur(0.55))).astype(np.float32) / 255.0


BAYER = np.array([[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]], dtype=np.float32) / 16.0 - 0.5

for v in views:
    # S4 graphic cel: flat albedo x banded lighting, contour from albedo edges (how a cel shader composes)
    alb = load(f"CEL_albedo_{v}.png")
    alb = np.asarray(to_img(alb).filter(ImageFilter.MedianFilter(3))).astype(np.float32) / 255.0
    lit = load(f"CEL_light_{v}.png")
    l = np.asarray(to_img(lit @ LUM).filter(ImageFilter.GaussianBlur(1.0))).astype(np.float32) / 255.0
    l = l / max(float(np.percentile(l, 97)), 0.05)
    shade = np.where(l < 0.2, 0.3, np.where(l < 0.55, 0.62, 1.0))
    tint = lit / np.maximum((lit @ LUM)[..., None], 0.04)
    tint = np.asarray(to_img(np.clip(tint / 2.0, 0, 1)).filter(ImageFilter.GaussianBlur(6))).astype(np.float32) / 255.0 * 2.0
    cel = alb * shade[..., None] * (0.5 + 0.5 * tint)
    cool = np.array([0.72, 0.8, 1.06], dtype=np.float32)
    cel = np.where((l < 0.2)[..., None], cel * cool, cel)
    cel = np.clip(cel * 1.02, 0, 1)
    e = edge_mask(alb, 0.16)
    ink = cel * (1 - e[..., None]) + np.array([0.07, 0.06, 0.1], dtype=np.float32) * e[..., None]
    save(ink, f"S4_graphicCel_{v}.png")

    # S3 diorama: tilt-shift blur bands plus a gentle lift, previewing the miniature read
    d3 = load(f"S3_softDiorama_{v}.png")
    blur = np.asarray(to_img(d3).filter(ImageFilter.GaussianBlur(7))).astype(np.float32) / 255.0
    h = d3.shape[0]
    mask = np.clip((np.abs(np.linspace(-1, 1, h)) - 0.42) / 0.5, 0, 1)[:, None, None]
    d3b = d3 * (1 - mask) + blur * mask
    g = d3b.mean(2, keepdims=True)
    save(np.clip(g + (d3b - g) * 1.22, 0, 1) * 0.97 + 0.03, f"S3_softDiorama_{v}.png")

    # S7 lo-fi: 320x180 point sampled with ordered dither and a limited palette
    src = Image.open(os.path.join(d, f"S1_stylizedReal_{v}.png")).convert("RGB").resize((320, 180), Image.BILINEAR)
    a = np.asarray(src).astype(np.float32) / 255.0
    th = np.tile(BAYER, (46, 81))[:180, :320]
    steps = 12.0
    q = np.floor(a * steps + 0.5 + th[..., None] * 0.95) / steps
    save(np.repeat(np.repeat(q, 6, 0), 6, 1), f"S7_lofiRetro_{v}.png")

    # S8 painterly: Kuwahara over the stylized-real render, saturation lifted slightly
    pa = kuwahara(load(f"S1_stylizedReal_{v}.png"), 8)
    g = pa.mean(2, keepdims=True)
    save(g + (pa - g) * 1.2, f"S8_painterly_{v}.png")

print("ok")
