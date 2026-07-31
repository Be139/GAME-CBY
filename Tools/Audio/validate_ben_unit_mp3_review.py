#!/usr/bin/env python3
"""Validate and finalize the combined 173-file HEARTH MP3 review batch."""

from __future__ import annotations

import argparse
import datetime as dt
import hashlib
import json
import os
import subprocess
from pathlib import Path
from typing import Any


FFPROBE = Path("E:/VoxCPM/ffmpeg/ffmpeg-8.1.2-full_build-shared/bin/ffprobe.exe")
EXPECTED_BEN = 26
EXPECTED_UNIT = 147
OLD_REVIEW_DIRS = [
    Path("GeneratedAudio/VoxCPM/BenConsistency_Review_2026-07-31"),
    Path("GeneratedAudio/UnitHumanMachine_Review_2026-07-31"),
    Path("GeneratedAudio/UnitFlangerReference_Review_2026-07-31"),
    Path("GeneratedAudio/VoxCPM/UnitVoiceDirection_Review_2026-07-31"),
]


def utc_now() -> str:
    return dt.datetime.now(dt.timezone.utc).isoformat()


def sha256_file(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as handle:
        for chunk in iter(lambda: handle.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def atomic_json(path: Path, value: Any) -> None:
    temporary = path.with_suffix(path.suffix + ".tmp")
    temporary.write_text(
        json.dumps(value, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )
    os.replace(temporary, path)


def validate_stream(path: Path) -> dict[str, Any]:
    result = subprocess.run(
        [
            str(FFPROBE),
            "-v",
            "error",
            "-show_entries",
            "format=format_name,bit_rate,duration:"
            "stream=codec_name,sample_rate,channels,bit_rate",
            "-of",
            "json",
            str(path),
        ],
        check=True,
        capture_output=True,
        text=True,
        encoding="utf-8",
        errors="replace",
    )
    data = json.loads(result.stdout)
    streams = data.get("streams", [])
    if len(streams) != 1:
        raise RuntimeError(f"Expected one stream: {path}")
    stream = streams[0]
    audio_format = data.get("format", {})
    codec = stream.get("codec_name")
    sample_rate = int(stream.get("sample_rate", 0))
    channels = int(stream.get("channels", 0))
    bit_rate = int(stream.get("bit_rate") or audio_format.get("bit_rate") or 0)
    duration = float(audio_format.get("duration", 0.0))
    if (
        codec != "mp3"
        or sample_rate != 48000
        or channels != 1
        or not 180000 <= bit_rate <= 200000
        or duration <= 0.1
    ):
        raise RuntimeError(
            f"Invalid MP3: {path} ({codec}, {sample_rate}, {channels}, "
            f"{bit_rate}, {duration})"
        )
    return {
        "codec": codec,
        "sample_rate_hz": sample_rate,
        "channels": channels,
        "bit_rate_bps": bit_rate,
        "duration_seconds": round(duration, 3),
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo-root", type=Path, required=True)
    parser.add_argument("--output-root", type=Path, required=True)
    args = parser.parse_args()

    repo_root = args.repo_root.resolve()
    output_root = args.output_root.resolve()
    ben_manifest_path = output_root / "ben_full_manifest.json"
    unit_manifest_path = output_root / "unit_balanced_full_manifest.json"
    cleanup_path = output_root / "cleanup_record.json"
    ben_manifest = json.loads(ben_manifest_path.read_text(encoding="utf-8"))
    unit_manifest = json.loads(unit_manifest_path.read_text(encoding="utf-8"))
    cleanup = json.loads(cleanup_path.read_text(encoding="utf-8"))

    if (
        ben_manifest.get("success_count") != EXPECTED_BEN
        or len(ben_manifest.get("items", [])) != EXPECTED_BEN
    ):
        raise RuntimeError("Ben manifest is incomplete")
    if (
        unit_manifest.get("success_count") != EXPECTED_UNIT
        or len(unit_manifest.get("items", [])) != EXPECTED_UNIT
    ):
        raise RuntimeError("Unit manifest is incomplete")

    ben_ids = {item["line_id"] for item in ben_manifest["items"]}
    unit_ids = {item["line_id"] for item in unit_manifest["items"]}
    if len(ben_ids) != EXPECTED_BEN or len(unit_ids) != EXPECTED_UNIT:
        raise RuntimeError("Duplicate line_id detected")

    all_items = [
        ("Ben", item) for item in ben_manifest["items"]
    ] + [
        ("Unit", item) for item in unit_manifest["items"]
    ]
    validated = []
    for group, item in all_items:
        path = Path(item["output_path"])
        if not path.is_file() or path.suffix.lower() != ".mp3":
            raise RuntimeError(f"Missing final MP3: {path}")
        output_hash = sha256_file(path)
        if output_hash != item["output_sha256"]:
            raise RuntimeError(f"Hash mismatch: {path}")
        validated.append(
            {
                "group": group,
                "line_id": item["line_id"],
                "path": str(path),
                "sha256": output_hash,
                "stream": validate_stream(path),
            }
        )

    final_mp3s = sorted(output_root.rglob("*.mp3"))
    if len(final_mp3s) != EXPECTED_BEN + EXPECTED_UNIT:
        raise RuntimeError(f"Expected 173 MP3 files, got {len(final_mp3s)}")
    unwanted = sorted(
        path
        for path in output_root.rglob("*")
        if path.is_file() and (
            path.suffix.lower() == ".wav"
            or path.name.endswith(".part")
            or "AuditionFlangerReference_Light" in path.name
            or "AuditionFlangerReference_Reference" in path.name
            or "AuditionFlangerReference_Strong" in path.name
        )
    )
    if unwanted:
        raise RuntimeError(f"Unwanted review/intermediate files remain: {unwanted}")

    old_remaining = [
        str((repo_root / relative).resolve())
        for relative in OLD_REVIEW_DIRS
        if (repo_root / relative).exists()
    ]
    if old_remaining:
        raise RuntimeError(f"Old review directories still exist: {old_remaining}")

    batch_manifest = {
        "batch": "HEARTH Ben and Unit Balanced Full MP3 Review",
        "completed_at": utc_now(),
        "status": "success",
        "expected_count": EXPECTED_BEN + EXPECTED_UNIT,
        "success_count": len(validated),
        "counts": {"Ben": EXPECTED_BEN, "Unit": EXPECTED_UNIT},
        "source_jsonl": ben_manifest.get("source_jsonl"),
        "encoding": {
            "codec": "mp3",
            "sample_rate_hz": 48000,
            "channels": 1,
            "bit_rate_bps": 192000,
            "id3_version": "2.3",
        },
        "cleanup_record": {
            "path": str(cleanup_path),
            "recorded_file_count": cleanup.get("file_count"),
            "sha256": sha256_file(cleanup_path),
        },
        "ben_manifest": {
            "path": str(ben_manifest_path),
            "sha256": sha256_file(ben_manifest_path),
        },
        "unit_manifest": {
            "path": str(unit_manifest_path),
            "sha256": sha256_file(unit_manifest_path),
        },
        "items": validated,
    }
    batch_manifest_path = output_root / "batch_manifest.json"
    atomic_json(batch_manifest_path, batch_manifest)
    print(f"VALIDATED {len(validated)}/173 MP3 files", flush=True)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
