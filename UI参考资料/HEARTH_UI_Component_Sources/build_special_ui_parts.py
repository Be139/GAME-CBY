from pathlib import Path

from PIL import Image, ImageDraw, ImageFont, ImageOps


REPO_ROOT = Path(__file__).resolve().parents[2]
FULLSCREEN_ROOT = REPO_ROOT / "UI参考资料" / "HEARTH_UI_Fullscreen_Mockups"
PARTS_ROOT = REPO_ROOT / "Assets" / "UI" / "HEARTH" / "GeneratedParts"

CYAN = (18, 167, 238, 255)
LIGHT_CYAN = (137, 222, 243, 255)
AMBER = (232, 158, 24, 255)
RED = (218, 62, 71, 255)
FONT_PATH = Path("C:/Windows/Fonts/bahnschrift.ttf")


def cut_corner_points(width, height, inset, cut):
    return [
        (inset + cut, inset),
        (width - inset - cut, inset),
        (width - inset, inset + cut),
        (width - inset, height - inset - cut),
        (width - inset - cut, height - inset),
        (inset + cut, height - inset),
        (inset, height - inset - cut),
        (inset, inset + cut),
    ]


def draw_frame(
    size,
    path,
    color=CYAN,
    cut=34,
    inset=14,
    inner_gap=10,
    accent=True,
):
    width, height = size
    image = Image.new("RGBA", size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)
    outer = cut_corner_points(width, height, inset, cut)
    inner = cut_corner_points(
        width,
        height,
        inset + inner_gap,
        max(12, cut - inner_gap),
    )
    draw.line(outer + [outer[0]], fill=color, width=4, joint="curve")
    draw.line(inner + [inner[0]], fill=LIGHT_CYAN, width=2, joint="curve")

    if accent:
        top_y = inset
        bottom_y = height - inset
        draw.line(
            [(width * 0.62, top_y), (width * 0.78, top_y)],
            fill=LIGHT_CYAN,
            width=8,
        )
        for index in range(3):
            x = int(width * 0.46) + index * 18
            draw.line(
                [(x, bottom_y - 2), (x + 12, bottom_y - 2)],
                fill=color,
                width=6,
            )

    path.parent.mkdir(parents=True, exist_ok=True)
    image.save(path)


def draw_hold_frame(size, path):
    draw_frame(size, path, cut=30)
    image = Image.open(path).convert("RGBA")
    draw = ImageDraw.Draw(image)
    width, height = size
    bar_y = height - 54
    draw.line([(62, bar_y), (width - 62, bar_y)], fill=(74, 103, 122, 170), width=8)
    image.save(path)


def draw_progress_fill(size, path):
    image = Image.new("RGBA", size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)
    draw.rounded_rectangle(
        (2, 2, size[0] - 2, size[1] - 2),
        radius=max(2, size[1] // 3),
        fill=CYAN,
    )
    path.parent.mkdir(parents=True, exist_ok=True)
    image.save(path)


def draw_photo_frame(size, path):
    draw_frame(size, path, cut=42, inset=18, inner_gap=12)
    image = Image.open(path).convert("RGBA")
    draw = ImageDraw.Draw(image)
    width, height = size
    draw.line([(72, 88), (width * 0.28, 88)], fill=CYAN, width=4)
    draw.line([(width * 0.73, height - 76), (width - 72, height - 76)], fill=CYAN, width=4)
    image.save(path)


def resize_fullscreen_mockups():
    if not FULLSCREEN_ROOT.exists():
        return
    for path in FULLSCREEN_ROOT.glob("*.png"):
        image = Image.open(path).convert("RGB")
        if image.size != (1920, 1080):
            image = image.resize((1920, 1080), Image.Resampling.LANCZOS)
            image.save(path)


def build_contact_sheet(paths, output_path, columns, thumb_size):
    paths = sorted(paths)
    if not paths:
        return

    font = ImageFont.truetype(str(FONT_PATH), 22)
    cell_width = thumb_size[0] + 40
    cell_height = thumb_size[1] + 72
    rows = (len(paths) + columns - 1) // columns
    sheet = Image.new(
        "RGB",
        (cell_width * columns, cell_height * rows),
        (245, 248, 250),
    )
    draw = ImageDraw.Draw(sheet)

    for index, path in enumerate(paths):
        image = Image.open(path).convert("RGBA")
        preview = ImageOps.contain(image, thumb_size, Image.Resampling.LANCZOS)
        canvas = Image.new("RGBA", thumb_size, (19, 29, 39, 255))
        offset = (
            (thumb_size[0] - preview.width) // 2,
            (thumb_size[1] - preview.height) // 2,
        )
        canvas.alpha_composite(preview, offset)
        x = (index % columns) * cell_width + 20
        y = (index // columns) * cell_height + 16
        sheet.paste(canvas.convert("RGB"), (x, y))
        draw.text(
            (x, y + thumb_size[1] + 12),
            path.stem,
            font=font,
            fill=(8, 31, 61),
        )

    output_path.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(output_path)


def build_parts():
    draw_frame(
        (760, 210),
        PARTS_ROOT / "Interaction" / "HUD_Interaction_TapFrame_9Slice.png",
        cut=26,
    )
    draw_hold_frame(
        (980, 310),
        PARTS_ROOT / "Interaction" / "HUD_Interaction_HoldFrame.png",
    )
    draw_progress_fill(
        (860, 18),
        PARTS_ROOT / "Interaction" / "HUD_Interaction_HoldProgressFill.png",
    )
    draw_frame(
        (980, 250),
        PARTS_ROOT / "Interaction" / "HUD_Interaction_GazePromptFrame_9Slice.png",
        cut=28,
    )
    draw_frame(
        (1000, 170),
        PARTS_ROOT / "Interaction" / "HUD_Interaction_ChoiceHintFrame_9Slice.png",
        cut=24,
    )

    draw_frame(
        (900, 380),
        PARTS_ROOT / "Feedback" / "HUD_Feedback_FieldUnitToastFrame_9Slice.png",
        cut=34,
    )
    draw_frame(
        (520, 180),
        PARTS_ROOT / "Feedback" / "HUD_Feedback_TrustToastFrame_9Slice.png",
        cut=24,
    )
    draw_frame(
        (700, 170),
        PARTS_ROOT / "Feedback" / "HUD_Feedback_PleaseWaitFrame_9Slice.png",
        color=(112, 140, 157, 210),
        cut=24,
    )
    draw_frame(
        (1120, 560),
        PARTS_ROOT / "Feedback" / "HUD_Feedback_WarningModalFrame_9Slice.png",
        color=AMBER,
        cut=42,
    )

    draw_frame(
        (1200, 760),
        PARTS_ROOT / "Inspection" / "HUD_Inspection_DiagnosticViewportFrame_9Slice.png",
        cut=42,
    )

    draw_photo_frame(
        (1440, 820),
        PARTS_ROOT / "Finale" / "HUD_Finale_PhotoFrame_9Slice.png",
    )
    draw_frame(
        (1160, 620),
        PARTS_ROOT / "Finale" / "HUD_Finale_ShutdownModalFrame_9Slice.png",
        cut=42,
    )
    draw_frame(
        (820, 390),
        PARTS_ROOT / "Finale" / "HUD_Finale_VirusPopup_Phase01_9Slice.png",
        color=CYAN,
        cut=34,
    )
    draw_frame(
        (820, 390),
        PARTS_ROOT / "Finale" / "HUD_Finale_VirusPopup_Phase02_9Slice.png",
        color=AMBER,
        cut=34,
    )
    draw_frame(
        (820, 390),
        PARTS_ROOT / "Finale" / "HUD_Finale_VirusPopup_Phase03_9Slice.png",
        color=RED,
        cut=34,
    )


if __name__ == "__main__":
    resize_fullscreen_mockups()
    build_parts()
    build_contact_sheet(
        FULLSCREEN_ROOT.glob("*.png"),
        FULLSCREEN_ROOT / "HEARTH_UI_Fullscreen_ContactSheet.jpg",
        columns=3,
        thumb_size=(480, 270),
    )
    build_contact_sheet(
        (
            path
            for path in PARTS_ROOT.rglob("*.png")
            if "ContactSheet" not in path.name
        ),
        Path(__file__).parent / "HEARTH_UI_Component_ContactSheet.jpg",
        columns=3,
        thumb_size=(480, 240),
    )
