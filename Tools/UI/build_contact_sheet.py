from __future__ import annotations

import argparse
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Build a labelled contact sheet from Unity UI screenshots."
    )
    parser.add_argument("input_dir", type=Path)
    parser.add_argument("output", type=Path)
    parser.add_argument("--columns", type=int, default=4)
    parser.add_argument("--thumb-width", type=int, default=480)
    parser.add_argument(
        "--include",
        nargs="+",
        help="Optional exact PNG file names, in the desired sheet order.",
    )
    args = parser.parse_args()

    if args.include:
        files = [args.input_dir / name for name in args.include]
        missing = [path for path in files if not path.is_file()]
        if missing:
            raise SystemExit(
                "Missing requested screenshots: "
                + ", ".join(str(path) for path in missing)
            )
    else:
        files = sorted(args.input_dir.glob("*.png"))
    files = [path for path in files if path.resolve() != args.output.resolve()]
    if not files:
        raise SystemExit("No PNG screenshots found.")

    columns = max(1, args.columns)
    thumb_width = max(160, args.thumb_width)
    with Image.open(files[0]) as first:
        ratio = first.height / first.width
    thumb_height = round(thumb_width * ratio)
    label_height = 28
    rows = (len(files) + columns - 1) // columns

    sheet = Image.new(
        "RGB",
        (columns * thumb_width, rows * (thumb_height + label_height)),
        (10, 14, 21),
    )
    draw = ImageDraw.Draw(sheet)
    font = ImageFont.load_default()

    for index, path in enumerate(files):
        column = index % columns
        row = index // columns
        x = column * thumb_width
        y = row * (thumb_height + label_height)
        with Image.open(path) as source:
            preview = source.convert("RGB")
            preview.thumbnail((thumb_width, thumb_height), Image.Resampling.LANCZOS)
            sheet.paste(preview, (x, y + label_height))
        draw.text((x + 8, y + 8), path.stem, fill=(215, 230, 246), font=font)

    args.output.parent.mkdir(parents=True, exist_ok=True)
    sheet.save(args.output, optimize=True)


if __name__ == "__main__":
    main()
