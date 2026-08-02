#!/usr/bin/env python
"""Generate the HEARTH V2 thin-frame SVG/PNG asset family.

The SVG files are the editable source of truth. PNG files are exact-size,
transparent Unity render targets produced from those SVGs with CairoSVG.
"""

from __future__ import annotations

from dataclasses import dataclass
from pathlib import Path
from typing import Callable

import resvg_py
from PIL import Image, ImageDraw


PROJECT_ROOT = Path(__file__).resolve().parents[2]
SOURCE_ROOT = PROJECT_ROOT / "Assets/UI/HEARTH/V2/VectorSource/Frames"
PNG_ROOT = PROJECT_ROOT / "Assets/UI/HEARTH/V2/VectorParts"
REVIEW_ROOT = Path("D:/image-to-svg/outputs/hearth-ui-v2-thin-frames")

ACCENT = "#78AADC"
OUTER_OPACITY = 0.85
INNER_OPACITY = 0.45


@dataclass(frozen=True)
class AssetSpec:
    category: str
    name: str
    width: int
    height: int
    shape: str = "panel"


ASSETS = (
    AssetSpec("Common", "HUD_Common_StatusFrame_520x240", 520, 240),
    AssetSpec("Common", "HUD_Common_DecisionFrame_520x216", 520, 216),
    AssetSpec("Common", "HUD_Common_DialogueFrame_960x256", 960, 256, "dialogue"),
    AssetSpec("Common", "HUD_Common_ButtonFrame_540x84", 540, 84),
    AssetSpec("Common", "HUD_Common_SpeakerTab_Left_340x48", 340, 48, "speaker_left"),
    AssetSpec("Common", "HUD_Common_SpeakerTab_Right_340x48", 340, 48, "speaker_right"),
    AssetSpec("Feedback", "HUD_Feedback_FieldUnitToastFrame_640x400", 640, 400),
    AssetSpec("Feedback", "HUD_Feedback_LilyVoiceMessageFrame_540x300", 540, 300),
    AssetSpec("Human", "HUD_Human_TabPageFrame_1120x760", 1120, 760, "dialogue"),
    AssetSpec("Human", "HUD_Human_ContentFrame_860x420", 860, 420),
    AssetSpec("Human", "HUD_Human_MetricFrame_860x132", 860, 132),
    AssetSpec("Companion", "HUD_Companion_FullscreenFrame_1920x1080", 1920, 1080, "fullscreen"),
    AssetSpec("Terminal", "HUD_Terminal_InfoPanelFrame_620x260", 620, 260),
    AssetSpec("Terminal", "HUD_Terminal_FieldUnitFrame_620x190", 620, 190),
    AssetSpec("Terminal", "HUD_Terminal_PortraitFrame_240x400", 240, 400),
    AssetSpec("Terminal", "HUD_Terminal_TabFrame_310x52", 310, 52),
    AssetSpec("Terminal", "HUD_Terminal_PrimaryTabFrame_570x52", 570, 52),
)


def panel_path(width: int, height: int, inset: float, chamfer: float) -> str:
    left = inset
    top = inset
    right = width - inset
    bottom = height - inset
    return (
        f"M {left + chamfer:g} {top:g} H {right - chamfer:g} "
        f"L {right:g} {top + chamfer:g} V {bottom - chamfer:g} "
        f"L {right - chamfer:g} {bottom:g} H {left + chamfer:g} "
        f"L {left:g} {bottom - chamfer:g} V {top + chamfer:g} Z"
    )


def dialogue_path(width: int, height: int, inset: float, chamfer: float) -> str:
    left = inset
    top = inset
    right = width - inset
    bottom = height - inset
    notch_left = width * 0.28
    notch_right = width * 0.72
    notch_depth = 10
    return (
        f"M {left + chamfer:g} {top:g} H {notch_left:g} "
        f"L {notch_left + 14:g} {top + notch_depth:g} "
        f"H {notch_right - 14:g} L {notch_right:g} {top:g} "
        f"H {right - chamfer:g} L {right:g} {top + chamfer:g} "
        f"V {bottom - chamfer:g} L {right - chamfer:g} {bottom:g} "
        f"H {notch_right:g} L {notch_right - 14:g} {bottom - notch_depth:g} "
        f"H {notch_left + 14:g} L {notch_left:g} {bottom:g} "
        f"H {left + chamfer:g} L {left:g} {bottom - chamfer:g} "
        f"V {top + chamfer:g} Z"
    )


def speaker_path(width: int, height: int, inset: float, right_side: bool) -> str:
    left = inset
    top = inset
    right = width - inset
    bottom = height - inset
    c = 8
    if right_side:
        return (
            f"M {left:g} {top:g} H {right - c:g} L {right:g} {top + c:g} "
            f"V {bottom - c:g} L {right - c:g} {bottom:g} H {left + 16:g} "
            f"L {left:g} {bottom - 10:g} Z"
        )
    return (
        f"M {left + c:g} {top:g} H {right:g} L {right:g} {bottom - 10:g} "
        f"L {right - 16:g} {bottom:g} H {left + c:g} L {left:g} {bottom - c:g} "
        f"V {top + c:g} Z"
    )


def fullscreen_path(width: int, height: int, inset: float, inner: bool) -> str:
    left = inset
    top = inset
    right = width - inset
    bottom = height - inset
    c = 18 if not inner else 14
    top_notch_left = width * 0.41
    top_notch_right = width * 0.59
    top_notch_depth = 42 if not inner else 34
    bottom_notch_left = width * 0.475
    bottom_notch_right = width * 0.525
    bottom_notch_depth = 20 if not inner else 15
    return (
        f"M {left + c:g} {top:g} H {top_notch_left:g} "
        f"L {top_notch_left + 42:g} {top + top_notch_depth:g} "
        f"H {top_notch_right - 42:g} L {top_notch_right:g} {top:g} "
        f"H {right - c:g} L {right:g} {top + c:g} V {bottom - c:g} "
        f"L {right - c:g} {bottom:g} H {bottom_notch_right + 22:g} "
        f"L {bottom_notch_right:g} {bottom - bottom_notch_depth:g} "
        f"H {bottom_notch_left:g} L {bottom_notch_left - 22:g} {bottom:g} "
        f"H {left + c:g} L {left:g} {bottom - c:g} V {top + c:g} Z"
    )


def shape_paths(spec: AssetSpec) -> tuple[str, str]:
    if spec.shape == "dialogue":
        return (
            dialogue_path(spec.width, spec.height, 1.5, 14),
            dialogue_path(spec.width, spec.height, 8.5, 10),
        )
    if spec.shape == "speaker_left":
        return (
            speaker_path(spec.width, spec.height, 1.5, False),
            speaker_path(spec.width, spec.height, 8.5, False),
        )
    if spec.shape == "speaker_right":
        return (
            speaker_path(spec.width, spec.height, 1.5, True),
            speaker_path(spec.width, spec.height, 8.5, True),
        )
    if spec.shape == "fullscreen":
        return (
            fullscreen_path(spec.width, spec.height, 2, False),
            fullscreen_path(spec.width, spec.height, 9, True),
        )
    chamfer = max(7, min(16, round(min(spec.width, spec.height) * 0.08)))
    return (
        panel_path(spec.width, spec.height, 1.5, chamfer),
        panel_path(spec.width, spec.height, 8.5, max(5, chamfer - 5)),
    )


def build_svg(spec: AssetSpec) -> str:
    outer, inner = shape_paths(spec)
    return f'''<?xml version="1.0" encoding="UTF-8"?>
<svg xmlns="http://www.w3.org/2000/svg" width="{spec.width}" height="{spec.height}" viewBox="0 0 {spec.width} {spec.height}">
  <title>{spec.name}</title>
  <g id="thin-frame" fill="none" stroke="{ACCENT}" stroke-linecap="square" stroke-linejoin="miter" shape-rendering="geometricPrecision">
    <path id="outer-line" d="{outer}" stroke-width="2" stroke-opacity="{OUTER_OPACITY}"/>
    <path id="inner-line" d="{inner}" stroke-width="1" stroke-opacity="{INNER_OPACITY}"/>
  </g>
</svg>
'''


def render_asset(spec: AssetSpec) -> Path:
    category_source = SOURCE_ROOT / spec.category
    category_png = PNG_ROOT / spec.category
    category_review_svg = REVIEW_ROOT / "svg" / spec.category
    category_review_png = REVIEW_ROOT / "png" / spec.category
    for directory in (
        category_source,
        category_png,
        category_review_svg,
        category_review_png,
    ):
        directory.mkdir(parents=True, exist_ok=True)

    svg_text = build_svg(spec)
    project_svg = category_source / f"{spec.name}.svg"
    review_svg = category_review_svg / f"{spec.name}.svg"
    project_png = category_png / f"{spec.name}.png"
    review_png = category_review_png / f"{spec.name}.png"
    project_svg.write_text(svg_text, encoding="utf-8", newline="\n")
    review_svg.write_text(svg_text, encoding="utf-8", newline="\n")
    project_png.write_bytes(resvg_py.svg_to_bytes(svg_string=svg_text))
    review_png.write_bytes(project_png.read_bytes())
    return review_png


def build_contact_sheet(rendered: list[tuple[AssetSpec, Path]]) -> None:
    thumb_width = 640
    margin = 28
    label_height = 32
    rows: list[tuple[AssetSpec, Image.Image]] = []
    for spec, path in rendered:
        image = Image.open(path).convert("RGBA")
        scale = min(1.0, thumb_width / image.width, 260 / image.height)
        if scale < 1.0:
            image = image.resize(
                (max(1, round(image.width * scale)), max(1, round(image.height * scale))),
                Image.Resampling.LANCZOS,
            )
        rows.append((spec, image))

    row_height = max(image.height for _, image in rows) + label_height + margin
    sheet = Image.new(
        "RGBA",
        (thumb_width + margin * 2, row_height * len(rows) + margin),
        (9, 16, 28, 255),
    )
    draw = ImageDraw.Draw(sheet)
    y = margin
    for spec, image in rows:
        x = (sheet.width - image.width) // 2
        sheet.alpha_composite(image, (x, y + label_height))
        draw.text((margin, y), f"{spec.name}  {spec.width}x{spec.height}", fill=(215, 230, 246, 255))
        y += row_height
    review_dir = REVIEW_ROOT / "review"
    review_dir.mkdir(parents=True, exist_ok=True)
    sheet.convert("RGB").save(review_dir / "HEARTH_UI_V2_ThinFrames_ContactSheet.png")


def main() -> None:
    rendered: list[tuple[AssetSpec, Path]] = []
    for spec in ASSETS:
        rendered.append((spec, render_asset(spec)))
    build_contact_sheet(rendered)
    print(f"Generated {len(rendered)} SVG/PNG pairs.")
    print(REVIEW_ROOT / "review/HEARTH_UI_V2_ThinFrames_ContactSheet.png")


if __name__ == "__main__":
    main()
