#!/usr/bin/env python3
"""Audit the final voice JSONL against the marked HEARTH subtitle script and MP3 collection."""

from __future__ import annotations

import argparse
import collections
import json
import pathlib
import re
import sys


MARKER_RE = re.compile(r"^<!--\s*HEARTH:SEQUENCES\s+(.+?)\s*-->$", re.IGNORECASE)
DIALOGUE_RE = re.compile(
    r'^\*\*(?P<speaker>.+?):\*\*\s*(?:\([^)]*\)\s*)?"(?P<text>.*)"\s*$'
)
PERFORMANCE_TAG_RE = re.compile(r"\[[^\]\r\n]+\]")
EXCLUDED_SEQUENCE = "Prologue_HEARTHCommercial"
EXCLUDED_LINE_IDS = {"Lobby_OpeningBriefing_FieldUnit_002"}


def split_sequences(value: str) -> tuple[str, ...]:
    return tuple(part.strip() for part in value.split(",") if part.strip())


def clean_subtitle(text: str) -> str:
    without_tags = PERFORMANCE_TAG_RE.sub("", text or "")
    return re.sub(r"\s+", " ", without_tags).strip()


def read_voice_rows(path: pathlib.Path) -> list[dict]:
    rows: list[dict] = []
    for source_index, raw in enumerate(path.read_text(encoding="utf-8-sig").splitlines(), 1):
        if not raw.strip():
            continue
        row = json.loads(raw)
        row["source_index"] = source_index
        row["sequences"] = split_sequences(row["sequence"])
        row["subtitle"] = clean_subtitle(row["text"])
        rows.append(row)
    return rows


def is_runtime_voice_row(row: dict) -> bool:
    return (
        EXCLUDED_SEQUENCE not in row["sequences"]
        and row["line_id"] not in EXCLUDED_LINE_IDS
    )


def assign_runtime_sequences(rows: list[dict]) -> None:
    photo_index = 0
    for row in rows:
        runtime_sequences = row["sequences"]
        if row["sequence"] == "17F04_ChristmasPhoto":
            if photo_index < 2:
                runtime_sequences = ("17F04_ChristmasPhoto",)
            elif photo_index < 5:
                runtime_sequences = ("17F04_SecondPhoto",)
            else:
                runtime_sequences = ("17F04_PhotoCompletion",)
            photo_index += 1
        row["runtime_sequences"] = runtime_sequences


def read_marked_dialogue(path: pathlib.Path) -> list[dict]:
    rows: list[dict] = []
    pending: tuple[str, ...] = ()
    for source_line, raw in enumerate(path.read_text(encoding="utf-8-sig").splitlines(), 1):
        stripped = raw.strip()
        marker = MARKER_RE.match(stripped)
        if marker:
            pending = split_sequences(marker.group(1))
            continue
        dialogue = DIALOGUE_RE.match(stripped)
        if dialogue:
            rows.append(
                {
                    "source_line": source_line,
                    "sequences": pending,
                    "speaker": dialogue.group("speaker").strip(),
                    "subtitle": dialogue.group("text").strip(),
                }
            )
            pending = ()
            continue
        if stripped and not stripped.startswith("<!--"):
            pending = ()
    return rows


def index_by_sequence(rows: list[dict]) -> dict[str, list[dict]]:
    indexed: dict[str, list[dict]] = collections.defaultdict(list)
    for row in rows:
        for sequence in row.get("runtime_sequences", row["sequences"]):
            indexed[sequence].append(row)
    return dict(indexed)


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--jsonl", type=pathlib.Path, required=True)
    parser.add_argument("--script", type=pathlib.Path, required=True)
    parser.add_argument("--collection", type=pathlib.Path, required=True)
    args = parser.parse_args()

    voice_rows_all = read_voice_rows(args.jsonl)
    voice_rows = [row for row in voice_rows_all if is_runtime_voice_row(row)]
    assign_runtime_sequences(voice_rows)
    script_rows = read_marked_dialogue(args.script)
    voice_by_sequence = index_by_sequence(voice_rows)
    script_by_sequence = index_by_sequence(script_rows)

    issues: list[str] = []
    all_sequences = sorted(set(voice_by_sequence) | set(script_by_sequence))
    for sequence in all_sequences:
        expected = voice_by_sequence.get(sequence, [])
        actual = script_by_sequence.get(sequence, [])
        if len(expected) != len(actual):
            issues.append(
                f"{sequence}: voice rows={len(expected)}, marked subtitle rows={len(actual)}"
            )
        for index, (voice, subtitle) in enumerate(zip(expected, actual), 1):
            if voice["speaker"] != subtitle["speaker"] or voice["subtitle"] != subtitle["subtitle"]:
                issues.append(
                    f"{sequence} #{index}: {voice['line_id']} differs at script line "
                    f"{subtitle['source_line']}\n"
                    f"  voice: {voice['speaker']}: {voice['subtitle']}\n"
                    f"  script: {subtitle['speaker']}: {subtitle['subtitle']}"
                )

    for row in voice_rows:
        audio_path = args.collection / f"{row['line_id']}.mp3"
        if not audio_path.is_file():
            issues.append(f"Missing MP3: {audio_path}")

    promotional_audio = sorted(args.collection.glob("Prologue_HEARTHCommercial_*.mp3"))
    print(f"JSONL total rows: {len(voice_rows_all)}")
    print(f"Excluded promotional/dependent rows: {len(voice_rows_all) - len(voice_rows)}")
    print(f"Gameplay voice rows: {len(voice_rows)}")
    print(f"Marked subtitle rows: {len(script_rows)}")
    print(f"Gameplay sequence ids: {len(voice_by_sequence)}")
    print(f"Collection promotional MP3s (kept outside Unity import): {len(promotional_audio)}")
    print(f"Issues: {len(issues)}")
    for issue in issues[:120]:
        print(f"- {issue}")
    if len(issues) > 120:
        print(f"- ... {len(issues) - 120} more")
    return 1 if issues else 0


if __name__ == "__main__":
    sys.exit(main())
