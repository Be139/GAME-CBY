#!/usr/bin/env python3
"""Create the full 147-line HEARTH Unit Balanced MP3 review batch."""

from __future__ import annotations

import argparse
import collections
import hashlib
import json
import os
import subprocess
from pathlib import Path
from typing import Any

from process_unit_human_machine import (
    AUDITION_FLANGER_FILTERS,
    AUDITION_FLANGER_MIXES,
    FFMPEG,
    FFPROBE,
    SOURCE_JSONL,
    atomic_json,
    is_unit_speaker,
    load_jsonl,
    measure_loudness,
    probe_duration,
    sha256_file,
    utc_now,
)


EXPECTED_COUNT = 147
ENCODING = {
    "codec": "libmp3lame",
    "sample_rate_hz": 48000,
    "channels": 1,
    "bit_rate_bps": 192000,
    "id3_version": "2.3",
}
FULL_FILTER = AUDITION_FLANGER_FILTERS["Balanced"].replace("TP=-1.5", "TP=-2.0")


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


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo-root", type=Path, required=True)
    parser.add_argument("--output-root", type=Path, required=True)
    args = parser.parse_args()

    repo_root = args.repo_root.resolve()
    output_root = args.output_root.resolve()
    output_dir = output_root / "Unit"
    output_dir.mkdir(parents=True, exist_ok=True)
    manifest_path = output_root / "unit_balanced_full_manifest.json"
    source_audio_dir = (
        repo_root / "GeneratedAudio" / "ElevenLabs" / "FullDialogue_2026-07-30"
    )
    source_manifest_path = source_audio_dir / "full_dialogue_manifest.json"
    source_manifest = json.loads(source_manifest_path.read_text(encoding="utf-8"))
    source_items = {
        item["line_id"]: item
        for item in source_manifest["items"]
        if item.get("status") == "success"
    }
    rows = [row for row in load_jsonl(SOURCE_JSONL) if is_unit_speaker(row["speaker"])]
    if len(rows) != EXPECTED_COUNT or len({row["line_id"] for row in rows}) != EXPECTED_COUNT:
        raise RuntimeError(f"Expected {EXPECTED_COUNT} unique Unit lines")

    prior_by_line: dict[str, dict[str, Any]] = {}
    if manifest_path.exists():
        prior = json.loads(manifest_path.read_text(encoding="utf-8"))
        prior_by_line = {item["line_id"]: item for item in prior.get("items", [])}

    manifest: dict[str, Any] = {
        "batch": "HEARTH Unit Balanced Full MP3 Review",
        "source_jsonl": str(SOURCE_JSONL),
        "source_audio_manifest": str(source_manifest_path),
        "effect_group": "AuditionFlangerBalanced",
        "mix": AUDITION_FLANGER_MIXES["Balanced"],
        "filter": FULL_FILTER,
        "encoding": ENCODING,
        "corporate_voice_included": False,
        "speaker_counts": dict(collections.Counter(row["speaker"] for row in rows)),
        "expected_count": EXPECTED_COUNT,
        "updated_at": utc_now(),
        "items": [],
    }

    for index, row in enumerate(rows, 1):
        line_id = row["line_id"]
        source_item = source_items.get(line_id)
        if source_item is None:
            raise RuntimeError(f"Source audio is missing from manifest: {line_id}")
        source_path = source_audio_dir / source_item["file_name"]
        if not source_path.is_file():
            raise RuntimeError(f"Source audio file is missing: {source_path}")
        source_hash = sha256_file(source_path)
        source_duration = probe_duration(source_path)
        output_name = f"{line_id}__AuditionFlangerBalanced.mp3"
        output_path = output_dir / output_name
        params = {
            "line_id": line_id,
            "speaker": row["speaker"],
            "text": row["text"],
            "source_path": str(source_path.resolve()),
            "source_sha256": source_hash,
            "source_duration_seconds": round(source_duration, 3),
            "effect_group": "AuditionFlangerBalanced",
            "mix": AUDITION_FLANGER_MIXES["Balanced"],
            "filter": FULL_FILTER,
            "encoding": ENCODING,
        }
        params_hash = hashlib.sha256(
            json.dumps(params, ensure_ascii=False, sort_keys=True).encode("utf-8")
        ).hexdigest()
        previous = prior_by_line.get(line_id, {})
        if (
            previous.get("status") == "success"
            and previous.get("params_sha256") == params_hash
            and output_path.is_file()
            and previous.get("output_sha256") == sha256_file(output_path)
        ):
            manifest["items"].append(previous)
            print(f"[{index:03d}/{EXPECTED_COUNT:03d}] SKIP {line_id}", flush=True)
            continue

        temporary = output_path.with_suffix(".mp3.part")
        command = [
            str(FFMPEG),
            "-hide_banner",
            "-loglevel",
            "error",
            "-y",
            "-i",
            str(source_path),
            "-filter_complex",
            FULL_FILTER,
            "-map",
            "[out]",
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
            f"title={line_id}",
            "-metadata",
            f"artist={row['speaker']}",
            "-f",
            "mp3",
            str(temporary),
        ]
        subprocess.run(command, check=True)
        os.replace(temporary, output_path)
        mp3_info = probe_mp3(output_path)
        if abs(mp3_info["duration_seconds"] - source_duration) > 0.15:
            raise RuntimeError(
                f"Unexpected duration change for {line_id}: "
                f"{source_duration:.3f}s -> {mp3_info['duration_seconds']:.3f}s"
            )
        loudness = measure_loudness(output_path)
        item = {
            **params,
            "params_sha256": params_hash,
            "status": "success",
            "output_name": output_name,
            "output_path": str(output_path.resolve()),
            "output_size_bytes": output_path.stat().st_size,
            "output_sha256": sha256_file(output_path),
            "mp3": mp3_info,
            "loudness": loudness,
            "completed_at": utc_now(),
        }
        manifest["items"].append(item)
        manifest["updated_at"] = utc_now()
        atomic_json(manifest_path, manifest)
        print(
            f"[{index:03d}/{EXPECTED_COUNT:03d}] OK {line_id} "
            f"{mp3_info['duration_seconds']:.2f}s "
            f"{loudness['integrated_lufs']:.2f} LUFS",
            flush=True,
        )

    manifest["success_count"] = sum(
        item.get("status") == "success" for item in manifest["items"]
    )
    manifest["completed_at"] = utc_now()
    atomic_json(manifest_path, manifest)
    print(f"COMPLETE {manifest['success_count']}/{EXPECTED_COUNT}", flush=True)
    return 0 if manifest["success_count"] == EXPECTED_COUNT else 1


if __name__ == "__main__":
    raise SystemExit(main())
