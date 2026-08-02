#!/usr/bin/env python3
"""Sync HEARTH's final voice-manifest wording and stable line IDs into the marked script."""

from __future__ import annotations

import argparse
import pathlib
import shutil
import sys

from audit_final_voice_alignment import (
    DIALOGUE_RE,
    MARKER_RE,
    assign_runtime_sequences,
    is_runtime_voice_row,
    read_marked_dialogue,
    read_voice_rows,
)


VOICE_MARKER_PREFIX = "<!-- HEARTH:VOICE "
VOICE_MARKER_RE = __import__("re").compile(
    r"^<!--\s*HEARTH:VOICE\s+(.+?)\s*-->$", __import__("re").IGNORECASE
)


def build_dialogue_bindings(script_rows: list[dict], voice_rows: list[dict]) -> dict[int, dict]:
    by_sequence: dict[str, list[dict]] = {}
    for row in voice_rows:
        for sequence in row["runtime_sequences"]:
            by_sequence.setdefault(sequence, []).append(row)

    cursors = {sequence: 0 for sequence in by_sequence}
    bindings: dict[int, dict] = {}
    for script_row in script_rows:
        candidates: list[dict] = []
        for sequence in script_row["sequences"]:
            if sequence not in by_sequence:
                raise RuntimeError(
                    f"Script line {script_row['source_line']} references voice-less sequence {sequence}."
                )
            index = cursors[sequence]
            if index >= len(by_sequence[sequence]):
                raise RuntimeError(
                    f"Script has more rows than the voice manifest for {sequence}."
                )
            candidates.append(by_sequence[sequence][index])
            cursors[sequence] += 1

        if not candidates:
            raise RuntimeError(
                f"Script line {script_row['source_line']} has no stable HEARTH:SEQUENCES marker."
            )
        line_ids = {row["line_id"] for row in candidates}
        if len(line_ids) != 1:
            raise RuntimeError(
                f"Script line {script_row['source_line']} maps to conflicting voice IDs: "
                + ", ".join(sorted(line_ids))
            )
        bindings[script_row["source_line"]] = candidates[0]

    leftovers = []
    for sequence, rows in by_sequence.items():
        if cursors[sequence] != len(rows):
            leftovers.append(
                f"{sequence}: consumed {cursors[sequence]} of {len(rows)} voice rows"
            )
    if leftovers:
        raise RuntimeError("Unmapped voice rows remain:\n- " + "\n- ".join(leftovers))
    return bindings


def rewrite_script(script_path: pathlib.Path, bindings: dict[int, dict]) -> str:
    source_lines = script_path.read_text(encoding="utf-8-sig").splitlines()
    output: list[str] = []
    for source_line, raw in enumerate(source_lines, 1):
        if VOICE_MARKER_RE.match(raw.strip()):
            continue
        voice = bindings.get(source_line)
        if voice is None:
            output.append(raw)
            continue
        dialogue = DIALOGUE_RE.match(raw.strip())
        if dialogue is None:
            raise RuntimeError(f"Expected dialogue at source line {source_line}.")
        leading = raw[: len(raw) - len(raw.lstrip())]
        output.append(f"{leading}{VOICE_MARKER_PREFIX}{voice['line_id']} -->")
        output.append(f'{leading}**{voice["speaker"]}:** "{voice["subtitle"]}"')
    return "\n".join(output).rstrip() + "\n"


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--jsonl", type=pathlib.Path, required=True)
    parser.add_argument("--script", type=pathlib.Path, required=True)
    parser.add_argument("--snapshot", type=pathlib.Path, required=True)
    parser.add_argument("--apply", action="store_true")
    args = parser.parse_args()

    voice_rows_all = read_voice_rows(args.jsonl)
    voice_rows = [row for row in voice_rows_all if is_runtime_voice_row(row)]
    assign_runtime_sequences(voice_rows)
    script_rows = read_marked_dialogue(args.script)
    bindings = build_dialogue_bindings(script_rows, voice_rows)
    updated = rewrite_script(args.script, bindings)

    changed = updated != args.script.read_text(encoding="utf-8-sig")
    print(f"Runtime voice rows: {len(voice_rows)}")
    print(f"Mapped subtitle rows: {len(bindings)}")
    print(f"Script would change: {changed}")
    if not args.apply:
        print("Dry run only; pass --apply to update the script and source snapshot.")
        return 0

    args.script.write_text(updated, encoding="utf-8", newline="\n")
    args.snapshot.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(args.jsonl, args.snapshot)
    print(f"Updated: {args.script}")
    print(f"Snapshot: {args.snapshot}")
    return 0


if __name__ == "__main__":
    sys.exit(main())
