"""Render the approved minimal HEARTH UI SVG geometry to transparent 2x PNGs.

The SVG files remain the editable source of truth.  This renderer mirrors their
simple polygon and line geometry so Unity receives predictable anti-aliased PNGs
without depending on a browser or native Cairo installation.
"""

from __future__ import annotations

from pathlib import Path

from PIL import Image, ImageDraw


ROOT = Path(__file__).resolve().parents[2]
CYAN = (115, 217, 255, 255)
CYAN_DIVIDER = (115, 217, 255, 199)
AMBER = (242, 163, 66, 255)
FILL_INSPECTION = (8, 13, 27, 219)
FILL_PANEL = (8, 13, 27, 235)


def clipped_polygon(width: int, height: int, cut: int) -> list[tuple[int, int]]:
    return [
        (cut, 2),
        (width - cut, 2),
        (width - 2, cut),
        (width - 2, height - cut),
        (width - cut, height - 2),
        (cut, height - 2),
        (2, height - cut),
        (2, cut),
    ]


def render(
    relative_path: str,
    logical_size: tuple[int, int],
    cut: int,
    fill: tuple[int, int, int, int],
    divider_y: int | None = None,
    accent: bool = False,
) -> None:
    scale = 2
    width, height = logical_size
    image = Image.new("RGBA", (width * scale, height * scale), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)
    polygon = [(x * scale, y * scale) for x, y in clipped_polygon(width, height, cut)]
    draw.polygon(polygon, fill=fill)
    draw.line(polygon + [polygon[0]], fill=CYAN, width=3 * scale, joint="curve")

    if divider_y is not None:
        draw.line(
            [(72 * scale, divider_y * scale), ((width - 72) * scale, divider_y * scale)],
            fill=CYAN_DIVIDER,
            width=2 * scale,
        )
    if accent:
        draw.line(
            [(22 * scale, 28 * scale), (22 * scale, 92 * scale)],
            fill=AMBER,
            width=4 * scale,
        )

    destination = ROOT / relative_path
    destination.parent.mkdir(parents=True, exist_ok=True)
    image.save(destination, optimize=True)
    print(f"wrote {destination} ({image.width}x{image.height})")


def main() -> None:
    render(
        "Assets/UI/HEARTH/V2/VectorParts/Inspection/"
        "HUD_Inspection_EntityPanelFrame_1600x932.png",
        (1600, 932),
        18,
        FILL_INSPECTION,
        divider_y=154,
    )
    render(
        "Assets/UI/HEARTH/V2/VectorParts/Terminal/"
        "HUD_Terminal_LobbyDialogueFrame_1460x248.png",
        (1460, 248),
        14,
        FILL_PANEL,
        accent=True,
    )
    render(
        "Assets/UI/HEARTH/V2/VectorParts/Interaction/"
        "HUD_Interaction_PromptFrame_680x150.png",
        (680, 150),
        14,
        FILL_PANEL,
    )


if __name__ == "__main__":
    main()
