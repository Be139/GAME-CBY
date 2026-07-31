#!/usr/bin/env python3
"""Generate the complete 26-line HEARTH Ben VoxCPM MP3 review batch."""

from __future__ import annotations

import argparse
import datetime as dt
import hashlib
import json
import math
import os
import subprocess
import sys
import traceback
from pathlib import Path
from typing import Any

import numpy as np
import soundfile as sf
import torch
from voxcpm import VoxCPM

from generate_ben_consistency import (
    FFMPEG,
    GENERATION_PARAMETERS,
    MODEL_DIR,
    PREVIEW_LINE_IDS,
    REFERENCE_SOURCE_NAME,
    REFERENCE_WAV,
    SOURCE_JSONL,
    atomic_json,
    controlled_text,
    prepare_reference,
    sha256_file,
    stable_seed,
    utc_now,
    validate_audio,
)


FFPROBE = FFMPEG.with_name("ffprobe.exe")
REFERENCE_LINE_ID = "17F02_BlackAudioArgument_Ben_001"
EXPECTED_COUNT = 26
ENCODING = {
    "codec": "libmp3lame",
    "sample_rate_hz": 48000,
    "channels": 1,
    "bit_rate_bps": 192000,
    "id3_version": "2.3",
}


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


def decoded_audio_metrics(path: Path) -> dict[str, float]:
    result = subprocess.run(
        [
            str(FFMPEG),
            "-hide_banner",
            "-loglevel",
            "error",
            "-i",
            str(path),
            "-ar",
            "48000",
            "-ac",
            "1",
            "-f",
            "f32le",
            "-c:a",
            "pcm_f32le",
            "pipe:1",
        ],
        check=True,
        capture_output=True,
    )
    values = np.frombuffer(result.stdout, dtype="<f4")
    if values.size < 4800 or not np.isfinite(values).all():
        raise RuntimeError(f"Invalid decoded MP3 samples: {path}")
    peak = float(np.max(np.abs(values)))
    rms = float(math.sqrt(float(np.mean(values * values))))
    if rms < 0.0001 or peak <= 0.0:
        raise RuntimeError(f"MP3 is effectively silent: {path}")
    return {
        "decoded_sample_count": int(values.size),
        "decoded_duration_seconds": round(values.size / 48000, 3),
        "decoded_peak": round(peak, 6),
        "decoded_rms": round(rms, 6),
    }


def encode_mp3(source: Path, destination: Path, line_id: str) -> None:
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
            f"title={line_id}",
            "-metadata",
            "artist=Ben",
            "-f",
            "mp3",
            str(temporary),
        ],
        check=True,
    )
    os.replace(temporary, destination)


def completed_item(
    params: dict[str, Any],
    params_hash: str,
    output_path: Path,
    elapsed_seconds: float,
) -> dict[str, Any]:
    return {
        **params,
        "params_sha256": params_hash,
        "status": "success",
        "output_name": output_path.name,
        "output_path": str(output_path.resolve()),
        "output_size_bytes": output_path.stat().st_size,
        "output_sha256": sha256_file(output_path),
        "mp3": probe_mp3(output_path),
        "audio": decoded_audio_metrics(output_path),
        "elapsed_seconds": round(elapsed_seconds, 3),
        "completed_at": utc_now(),
    }


def params_digest(params: dict[str, Any]) -> str:
    return hashlib.sha256(
        json.dumps(params, ensure_ascii=False, sort_keys=True).encode("utf-8")
    ).hexdigest()


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo-root", type=Path, required=True)
    parser.add_argument("--output-root", type=Path, required=True)
    parser.add_argument("--device", choices=("cuda", "cpu"), default="cuda")
    args = parser.parse_args()

    repo_root = args.repo_root.resolve()
    output_root = args.output_root.resolve()
    output_dir = output_root / "Ben"
    output_dir.mkdir(parents=True, exist_ok=True)
    manifest_path = output_root / "ben_full_manifest.json"
    migration_manifest_path = output_root / "ben_preview_migration_manifest.json"
    source_audio_dir = (
        repo_root / "GeneratedAudio" / "ElevenLabs" / "FullDialogue_2026-07-30"
    )
    reference_source = source_audio_dir / REFERENCE_SOURCE_NAME
    if not reference_source.is_file():
        raise RuntimeError(f"Reference MP3 is missing: {reference_source}")
    if not MODEL_DIR.is_dir() or not FFMPEG.is_file() or not FFPROBE.is_file():
        raise RuntimeError("VoxCPM model or FFmpeg runtime is missing")
    prepare_reference(reference_source)

    rows = [
        json.loads(line)
        for line in SOURCE_JSONL.read_text(encoding="utf-8-sig").splitlines()
        if line.strip()
    ]
    jobs = [row for row in rows if row.get("speaker") == "Ben"]
    if len(jobs) != EXPECTED_COUNT or len({row["line_id"] for row in jobs}) != EXPECTED_COUNT:
        raise RuntimeError(f"Expected {EXPECTED_COUNT} unique Ben lines")

    migration = json.loads(migration_manifest_path.read_text(encoding="utf-8"))
    migration_by_line = {
        item["line_id"]: item
        for item in migration.get("items", [])
        if item.get("status") == "success"
    }
    if set(migration_by_line) != set(PREVIEW_LINE_IDS):
        raise RuntimeError("Migrated Ben previews do not match the locked preview set")

    prior_by_line: dict[str, dict[str, Any]] = {}
    if manifest_path.exists():
        prior = json.loads(manifest_path.read_text(encoding="utf-8"))
        prior_by_line = {item["line_id"]: item for item in prior.get("items", [])}

    reference_hash = sha256_file(REFERENCE_WAV)
    manifest: dict[str, Any] = {
        "batch": "HEARTH Ben VoxCPM Full MP3 Review",
        "device": args.device,
        "source_jsonl": str(SOURCE_JSONL),
        "model_path": str(MODEL_DIR),
        "reference_source": str(reference_source.resolve()),
        "reference_source_sha256": sha256_file(reference_source),
        "reference_wav": str(REFERENCE_WAV),
        "reference_sha256": reference_hash,
        "generation_parameters": GENERATION_PARAMETERS,
        "encoding": ENCODING,
        "expected_count": EXPECTED_COUNT,
        "updated_at": utc_now(),
        "items": [],
    }

    pending_generation: list[tuple[dict[str, Any], dict[str, Any], str, Path]] = []
    for index, row in enumerate(jobs, 1):
        line_id = row["line_id"]
        text = controlled_text(line_id, row["text"])
        seed = stable_seed(line_id)
        output_path = output_dir / f"{line_id}__VoxCPM_ZaneReference.mp3"

        if line_id in migration_by_line:
            migrated = migration_by_line[line_id]
            params = {
                "line_id": line_id,
                "speaker": "Ben",
                "source_text": row["text"],
                "controlled_text": text,
                "seed": seed,
                "origin": "reused_verified_preview",
                "migration_source_sha256": migrated["source_sha256"],
                "migration_output_sha256": migrated["output_sha256"],
                "reference_sha256": reference_hash,
                "device": args.device,
                **GENERATION_PARAMETERS,
                "encoding": ENCODING,
            }
            params_hash = params_digest(params)
            if (
                not output_path.is_file()
                or sha256_file(output_path) != migrated["output_sha256"]
            ):
                raise RuntimeError(f"Migrated preview is invalid: {output_path}")
            previous = prior_by_line.get(line_id, {})
            if (
                previous.get("params_sha256") == params_hash
                and previous.get("output_sha256") == migrated["output_sha256"]
            ):
                item = previous
            else:
                item = completed_item(params, params_hash, output_path, 0.0)
            manifest["items"].append(item)
            print(f"[{index:02d}/{EXPECTED_COUNT:02d}] REUSE {line_id}", flush=True)
            continue

        if line_id == REFERENCE_LINE_ID:
            params = {
                "line_id": line_id,
                "speaker": "Ben",
                "source_text": row["text"],
                "controlled_text": text,
                "seed": seed,
                "origin": "direct_zane_reference_transcode",
                "source_path": str(reference_source.resolve()),
                "source_sha256": sha256_file(reference_source),
                "encoding": ENCODING,
            }
            params_hash = params_digest(params)
            previous = prior_by_line.get(line_id, {})
            if (
                previous.get("status") == "success"
                and previous.get("params_sha256") == params_hash
                and output_path.is_file()
                and previous.get("output_sha256") == sha256_file(output_path)
            ):
                item = previous
                print(f"[{index:02d}/{EXPECTED_COUNT:02d}] SKIP {line_id}", flush=True)
            else:
                started = dt.datetime.now(dt.timezone.utc)
                encode_mp3(reference_source, output_path, line_id)
                item = completed_item(
                    params,
                    params_hash,
                    output_path,
                    (dt.datetime.now(dt.timezone.utc) - started).total_seconds(),
                )
                print(f"[{index:02d}/{EXPECTED_COUNT:02d}] COPY {line_id}", flush=True)
            manifest["items"].append(item)
            manifest["updated_at"] = utc_now()
            atomic_json(manifest_path, manifest)
            continue

        params = {
            "line_id": line_id,
            "speaker": "Ben",
            "source_text": row["text"],
            "controlled_text": text,
            "seed": seed,
            "origin": "voxcpm_generated",
            "reference_sha256": reference_hash,
            "device": args.device,
            **GENERATION_PARAMETERS,
            "encoding": ENCODING,
        }
        params_hash = params_digest(params)
        previous = prior_by_line.get(line_id, {})
        if (
            previous.get("status") == "success"
            and previous.get("params_sha256") == params_hash
            and output_path.is_file()
            and previous.get("output_sha256") == sha256_file(output_path)
        ):
            manifest["items"].append(previous)
            print(f"[{index:02d}/{EXPECTED_COUNT:02d}] SKIP {line_id}", flush=True)
        else:
            pending_generation.append((row, params, params_hash, output_path))

    if pending_generation:
        print(f"Loading VoxCPM2 on {args.device}...", flush=True)
        started_load = dt.datetime.now(dt.timezone.utc)
        model = VoxCPM.from_pretrained(
            str(MODEL_DIR),
            load_denoiser=False,
            local_files_only=True,
            optimize=False,
            device=args.device,
        )
        sample_rate = int(model.tts_model.sample_rate)
        manifest["model_load_seconds"] = round(
            (dt.datetime.now(dt.timezone.utc) - started_load).total_seconds(),
            3,
        )
        manifest["model_sample_rate_hz"] = sample_rate
        atomic_json(manifest_path, manifest)

        for row, params, params_hash, output_path in pending_generation:
            line_id = row["line_id"]
            seed = params["seed"]
            torch.manual_seed(seed)
            if torch.cuda.is_available():
                torch.cuda.manual_seed_all(seed)
            np.random.seed(seed)
            started = dt.datetime.now(dt.timezone.utc)
            print(f"GENERATE {line_id}", flush=True)
            wav = model.generate(
                text=params["controlled_text"],
                reference_wav_path=str(REFERENCE_WAV),
                **GENERATION_PARAMETERS,
            )
            safe_wav, wav_info = validate_audio(wav, sample_rate)
            temporary_wav = output_path.with_suffix(".wav.part")
            try:
                sf.write(
                    str(temporary_wav),
                    safe_wav,
                    sample_rate,
                    subtype="PCM_24",
                    format="WAV",
                )
                encode_mp3(temporary_wav, output_path, line_id)
            finally:
                if temporary_wav.exists():
                    temporary_wav.unlink()
            item = completed_item(
                {**params, "generated_wav": wav_info},
                params_hash,
                output_path,
                (dt.datetime.now(dt.timezone.utc) - started).total_seconds(),
            )
            manifest["items"].append(item)
            manifest["updated_at"] = utc_now()
            atomic_json(manifest_path, manifest)
            print(
                f"  OK {item['mp3']['duration_seconds']:.2f}s "
                f"peak={item['audio']['decoded_peak']:.3f}",
                flush=True,
            )

    order = {row["line_id"]: index for index, row in enumerate(jobs)}
    manifest["items"].sort(key=lambda item: order[item["line_id"]])
    manifest["success_count"] = sum(
        item.get("status") == "success" for item in manifest["items"]
    )
    manifest["completed_at"] = utc_now()
    atomic_json(manifest_path, manifest)
    print(f"COMPLETE {manifest['success_count']}/{EXPECTED_COUNT}", flush=True)
    return 0 if manifest["success_count"] == EXPECTED_COUNT else 1


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except RuntimeError as exc:
        print(traceback.format_exc(), file=sys.stderr, flush=True)
        if "out of memory" in str(exc).lower() and torch.cuda.is_available():
            torch.cuda.empty_cache()
            raise SystemExit(42)
        raise
