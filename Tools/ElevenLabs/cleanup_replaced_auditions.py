#!/usr/bin/env python3
"""Delete the six superseded audition files after their replacements exist."""

from __future__ import annotations

import argparse
import hashlib
import json
import os
import tempfile
from datetime import datetime, timezone
from pathlib import Path


REPLACEMENTS = {
    "Eric__17F03_HumanEntryParents_Mark_001.mp3":
        "Brian__17F03_HumanEntryParents_Mark_001.mp3",
    "Eric__17F03_PostReplay_B_Mark_001.mp3":
        "Brian__17F03_PostReplay_B_Mark_001.mp3",
    "Valf__17F01_BedroomPrelude_Noah_002.mp3":
        "Gregory__17F01_BedroomPrelude_Noah_002.mp3",
    "Valf__17F01_BedsideSoothing_Noah_003.mp3":
        "Gregory__17F01_BedsideSoothing_Noah_003.mp3",
    "Astro__17F02_DiningObservation_Ben_002.mp3":
        "Ollie__17F02_DiningObservation_Ben_002.mp3",
    "Astro__17F02_BlackAudioArgument_Ben_004.mp3":
        "Ollie__17F02_BlackAudioArgument_Ben_004.mp3",
}


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def atomic_write_json(path: Path, value: object) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with tempfile.NamedTemporaryFile(
        mode="w",
        encoding="utf-8",
        newline="\n",
        dir=path.parent,
        prefix=f".{path.name}.",
        suffix=".tmp",
        delete=False,
    ) as handle:
        json.dump(value, handle, ensure_ascii=False, indent=2)
        handle.write("\n")
        temporary_path = Path(handle.name)
    os.replace(temporary_path, path)


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo-root", type=Path, required=True)
    args = parser.parse_args()

    output_dir = (
        args.repo_root.resolve()
        / "GeneratedAudio"
        / "ElevenLabs"
        / "CastingAuditions_2026-07-30"
    ).resolve()
    if not output_dir.is_dir():
        raise RuntimeError(f"Missing output directory: {output_dir}")

    records = []
    for old_name, new_name in REPLACEMENTS.items():
        old_path = (output_dir / old_name).resolve()
        new_path = (output_dir / new_name).resolve()
        if old_path.parent != output_dir or new_path.parent != output_dir:
            raise RuntimeError("Resolved cleanup target escaped the audition directory")
        if not old_path.is_file():
            raise RuntimeError(f"Expected old file is missing: {old_path}")
        if not new_path.is_file() or new_path.stat().st_size == 0:
            raise RuntimeError(f"Validated replacement is missing: {new_path}")
        records.append(
            {
                "old_file_name": old_name,
                "old_size_bytes": old_path.stat().st_size,
                "old_sha256": sha256(old_path),
                "replacement_file_name": new_name,
                "replacement_size_bytes": new_path.stat().st_size,
                "replacement_sha256": sha256(new_path),
            }
        )

    for record in records:
        (output_dir / record["old_file_name"]).unlink()

    report = {
        "deleted_at": datetime.now(timezone.utc).isoformat(),
        "deletion_scope": "Six exact superseded casting-audition MP3 files",
        "recoverable": False,
        "deleted_count": len(records),
        "records": records,
    }
    atomic_write_json(output_dir / "replaced_audio_cleanup.json", report)
    print(json.dumps(report, ensure_ascii=False, indent=2))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
