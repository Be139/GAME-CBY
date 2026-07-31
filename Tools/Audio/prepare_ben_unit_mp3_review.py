#!/usr/bin/env python3
"""Prepare the combined Ben and Unit MP3 review batch before full generation."""

from __future__ import annotations

import argparse
import datetime as dt
import hashlib
import json
import os
import subprocess
from pathlib import Path
from typing import Any


FFMPEG = Path("E:/VoxCPM/ffmpeg/ffmpeg-8.1.2-full_build-shared/bin/ffmpeg.exe")
FFPROBE = FFMPEG.with_name("ffprobe.exe")
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


def probe_mp3(path: Path) -> dict[str, Any]:
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
        raise RuntimeError(f"Expected one audio stream in {path}")
    stream = streams[0]
    audio_format = data.get("format", {})
    codec = stream.get("codec_name")
    sample_rate = int(stream.get("sample_rate", 0))
    channels = int(stream.get("channels", 0))
    bit_rate = int(stream.get("bit_rate") or audio_format.get("bit_rate") or 0)
    duration = float(audio_format.get("duration", 0.0))
    if codec != "mp3" or sample_rate != 48000 or channels != 1:
        raise RuntimeError(
            f"Invalid MP3 stream for {path}: {codec}, {sample_rate}, {channels}"
        )
    if not 180000 <= bit_rate <= 200000 or duration <= 0.1:
        raise RuntimeError(
            f"Invalid MP3 bitrate/duration for {path}: {bit_rate}, {duration}"
        )
    return {
        "codec": codec,
        "sample_rate_hz": sample_rate,
        "channels": channels,
        "bit_rate_bps": bit_rate,
        "duration_seconds": round(duration, 3),
        "format_name": audio_format.get("format_name"),
    }


def encode_mp3(source: Path, destination: Path, title: str) -> None:
    temporary = destination.with_suffix(".mp3.part")
    subprocess.run(
        [
            str(FFMPEG),
            "-hide_banner",
            "-loglevel",
            "error",
            "-y",
            "-i",
            str(source),
            "-map",
            "0:a:0",
            "-ar",
            "48000",
            "-ac",
            "1",
            "-c:a",
            "libmp3lame",
            "-b:a",
            "192k",
            "-write_id3v2",
            "1",
            "-id3v2_version",
            "3",
            "-metadata",
            f"title={title}",
            "-metadata",
            "artist=Ben",
            "-f",
            "mp3",
            str(temporary),
        ],
        check=True,
    )
    os.replace(temporary, destination)


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo-root", type=Path, required=True)
    parser.add_argument("--output-root", type=Path, required=True)
    args = parser.parse_args()

    repo_root = args.repo_root.resolve()
    output_root = args.output_root.resolve()
    generated_audio_root = (repo_root / "GeneratedAudio").resolve()
    try:
        output_root.relative_to(generated_audio_root)
    except ValueError as exc:
        raise RuntimeError("Output root must be inside GeneratedAudio") from exc

    if not FFMPEG.is_file() or not FFPROBE.is_file():
        raise RuntimeError("FFmpeg or FFprobe is missing")
    output_root.mkdir(parents=True, exist_ok=True)
    ben_output = output_root / "Ben"
    unit_output = output_root / "Unit"
    logs_output = output_root / "Logs"
    ben_output.mkdir(exist_ok=True)
    unit_output.mkdir(exist_ok=True)
    logs_output.mkdir(exist_ok=True)

    resolved_old_dirs = [(repo_root / relative).resolve() for relative in OLD_REVIEW_DIRS]
    for path in resolved_old_dirs:
        try:
            path.relative_to(generated_audio_root)
        except ValueError as exc:
            raise RuntimeError(f"Unsafe old review path: {path}") from exc
        if not path.is_dir():
            raise RuntimeError(f"Expected old review directory is missing: {path}")

    cleanup_items = []
    for directory in resolved_old_dirs:
        for path in sorted(item for item in directory.rglob("*") if item.is_file()):
            cleanup_items.append(
                {
                    "path": str(path),
                    "relative_to_repo": str(path.relative_to(repo_root)),
                    "size_bytes": path.stat().st_size,
                    "sha256": sha256_file(path),
                }
            )
    cleanup_record = {
        "batch": "HEARTH Ben and Unit review cleanup record",
        "created_at": utc_now(),
        "directories_authorized_for_deletion": [str(path) for path in resolved_old_dirs],
        "file_count": len(cleanup_items),
        "files": cleanup_items,
    }
    atomic_json(output_root / "cleanup_record.json", cleanup_record)

    ben_preview_dir = resolved_old_dirs[0]
    preview_manifest_path = ben_preview_dir / "ben_preview_manifest.json"
    preview_manifest = json.loads(preview_manifest_path.read_text(encoding="utf-8"))
    preview_items = [
        item for item in preview_manifest.get("items", []) if item.get("status") == "success"
    ]
    if len(preview_items) != 3:
        raise RuntimeError(f"Expected three verified Ben previews, got {len(preview_items)}")

    migration_items = []
    for item in preview_items:
        line_id = item["line_id"]
        source = ben_preview_dir / item["output_name"]
        if not source.is_file():
            raise RuntimeError(f"Ben preview is missing: {source}")
        source_hash = sha256_file(source)
        if source_hash != item["output_sha256"]:
            raise RuntimeError(f"Ben preview hash mismatch: {source}")
        destination = ben_output / f"{line_id}__VoxCPM_ZaneReference.mp3"
        encode_mp3(source, destination, line_id)
        mp3_info = probe_mp3(destination)
        migration_items.append(
            {
                "line_id": line_id,
                "source_path": str(source),
                "source_sha256": source_hash,
                "source_params_sha256": item.get("params_sha256"),
                "output_path": str(destination),
                "output_name": destination.name,
                "output_size_bytes": destination.stat().st_size,
                "output_sha256": sha256_file(destination),
                "mp3": mp3_info,
                "status": "success",
            }
        )

    migration_manifest = {
        "batch": "HEARTH verified Ben preview migration",
        "created_at": utc_now(),
        "expected_count": 3,
        "success_count": len(migration_items),
        "encoding": {
            "codec": "libmp3lame",
            "sample_rate_hz": 48000,
            "channels": 1,
            "bit_rate_bps": 192000,
            "id3_version": "2.3",
        },
        "items": migration_items,
    }
    atomic_json(output_root / "ben_preview_migration_manifest.json", migration_manifest)
    print(
        f"PREPARED {len(migration_items)}/3 Ben previews; "
        f"recorded {len(cleanup_items)} old review files",
        flush=True,
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
