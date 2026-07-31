#!/usr/bin/env python3
"""Generate the locked three-line HEARTH Ben VoxCPM review batch."""

from __future__ import annotations

import argparse
import datetime as dt
import hashlib
import json
import math
import os
import re
import subprocess
import sys
import traceback
from pathlib import Path
from typing import Any

import numpy as np
import soundfile as sf
import torch
from voxcpm import VoxCPM


SOURCE_JSONL = Path(
    "C:/Users/彩笔/Downloads/HEARTH_ElevenLabs_v3_API_Voice_Lines_Manual_Revision.jsonl"
)
MODEL_DIR = Path("E:/VoxCPM/models/VoxCPM2")
FFMPEG = Path("E:/VoxCPM/ffmpeg/ffmpeg-8.1.2-full_build-shared/bin/ffmpeg.exe")
REFERENCE_SOURCE_NAME = "Zane__17F02_BlackAudioArgument_Ben_001.mp3"
REFERENCE_WAV = Path(
    "E:/VoxCPM/reference_audio/Ben_Zane_BlackAudioArgument_001_16k_mono.wav"
)
PREVIEW_LINE_IDS = [
    "17F02_DiningObservation_Ben_002",
    "17F02_ForcedShutdown_Ben_001",
    "17F02_BlackAudioArgument_Ben_004",
]
SPECIAL_TEXT = {
    "17F02_BlackAudioArgument_Ben_004": (
        "(restrained anger, becoming hurt and quieter after the first sentence) "
        "For ten minutes. Two weeks, Claire. You talked to that thing nine times. "
        "You actually talked to me... once."
    ),
}
GENERATION_PARAMETERS = {
    "cfg_value": 2.0,
    "inference_timesteps": 10,
    "normalize": False,
    "denoise": False,
    "retry_badcase": True,
}


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


def stable_seed(line_id: str) -> int:
    return int.from_bytes(hashlib.sha256(line_id.encode("utf-8")).digest()[:4], "big")


def controlled_text(line_id: str, source_text: str) -> str:
    if line_id in SPECIAL_TEXT:
        return SPECIAL_TEXT[line_id]
    return re.sub(r"^\[([^\]]+)\]\s*", r"(\1) ", source_text).strip()


def prepare_reference(source_path: Path) -> None:
    REFERENCE_WAV.parent.mkdir(parents=True, exist_ok=True)
    if REFERENCE_WAV.is_file() and REFERENCE_WAV.stat().st_size > 1024:
        return
    temporary = REFERENCE_WAV.with_suffix(".wav.part")
    subprocess.run(
        [
            str(FFMPEG),
            "-hide_banner",
            "-loglevel",
            "error",
            "-y",
            "-i",
            str(source_path),
            "-ar",
            "16000",
            "-ac",
            "1",
            "-c:a",
            "pcm_s16le",
            "-f",
            "wav",
            str(temporary),
        ],
        check=True,
    )
    os.replace(temporary, REFERENCE_WAV)


def validate_audio(wav: np.ndarray, sample_rate: int) -> tuple[np.ndarray, dict[str, Any]]:
    values = np.asarray(wav, dtype=np.float32).reshape(-1)
    if sample_rate != 48000 or values.size < sample_rate // 10:
        raise RuntimeError(f"Invalid output shape/rate: {values.shape}, {sample_rate}")
    if not np.isfinite(values).all():
        raise RuntimeError("Generated audio contains NaN or infinity")
    peak_before = float(np.max(np.abs(values)))
    rms = float(math.sqrt(float(np.mean(values * values))))
    if rms < 0.0001:
        raise RuntimeError(f"Generated audio is effectively silent: rms={rms}")
    gain = 1.0
    if peak_before > 0.98:
        gain = 0.98 / peak_before
        values = values * gain
    return values, {
        "sample_rate_hz": sample_rate,
        "channels": 1,
        "sample_count": int(values.size),
        "duration_seconds": round(values.size / sample_rate, 3),
        "peak_before_safety_gain": round(peak_before, 6),
        "safety_gain": round(gain, 6),
        "peak": round(float(np.max(np.abs(values))), 6),
        "rms": round(rms * gain, 6),
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo-root", type=Path, required=True)
    parser.add_argument("--device", choices=("cuda", "cpu"), default="cuda")
    args = parser.parse_args()

    repo_root = args.repo_root.resolve()
    source_audio_dir = (
        repo_root / "GeneratedAudio" / "ElevenLabs" / "FullDialogue_2026-07-30"
    )
    reference_source = source_audio_dir / REFERENCE_SOURCE_NAME
    output_dir = (
        repo_root
        / "GeneratedAudio"
        / "VoxCPM"
        / "BenConsistency_Review_2026-07-31"
    )
    output_dir.mkdir(parents=True, exist_ok=True)
    log_path = output_dir / f"ben_preview_{args.device}.log"
    manifest_path = output_dir / "ben_preview_manifest.json"

    if not reference_source.is_file():
        raise RuntimeError(f"Reference MP3 is missing: {reference_source}")
    if not MODEL_DIR.is_dir():
        raise RuntimeError(f"Model directory is missing: {MODEL_DIR}")
    prepare_reference(reference_source)

    rows = [
        json.loads(line)
        for line in SOURCE_JSONL.read_text(encoding="utf-8-sig").splitlines()
        if line.strip()
    ]
    row_by_line = {row["line_id"]: row for row in rows}
    jobs = [row_by_line[line_id] for line_id in PREVIEW_LINE_IDS]
    if any(job["speaker"] != "Ben" for job in jobs):
        raise RuntimeError("A locked preview line is not spoken by Ben")

    existing_by_line: dict[str, dict[str, Any]] = {}
    if manifest_path.exists():
        existing = json.loads(manifest_path.read_text(encoding="utf-8"))
        existing_by_line = {
            item["line_id"]: item for item in existing.get("items", [])
        }

    reference_hash = sha256_file(REFERENCE_WAV)
    manifest: dict[str, Any] = {
        "batch": "HEARTH Ben VoxCPM Consistency Preview",
        "device": args.device,
        "model_path": str(MODEL_DIR),
        "reference_source": str(reference_source.resolve()),
        "reference_wav": str(REFERENCE_WAV),
        "reference_sha256": reference_hash,
        "generation_parameters": GENERATION_PARAMETERS,
        "expected_count": 3,
        "updated_at": utc_now(),
        "items": [],
    }

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

    for index, row in enumerate(jobs, 1):
        line_id = row["line_id"]
        text = controlled_text(line_id, row["text"])
        seed = stable_seed(line_id)
        output_name = f"{line_id}__VoxCPM_ZaneReference.wav"
        output_path = output_dir / output_name
        params = {
            "line_id": line_id,
            "speaker": "Ben",
            "source_text": row["text"],
            "controlled_text": text,
            "seed": seed,
            "reference_sha256": reference_hash,
            "device": args.device,
            **GENERATION_PARAMETERS,
        }
        params_hash = hashlib.sha256(
            json.dumps(params, ensure_ascii=False, sort_keys=True).encode("utf-8")
        ).hexdigest()
        previous = existing_by_line.get(line_id, {})
        if (
            previous.get("status") == "success"
            and previous.get("params_sha256") == params_hash
            and output_path.is_file()
            and previous.get("output_sha256") == sha256_file(output_path)
        ):
            manifest["items"].append(previous)
            print(f"[{index}/3] SKIP {line_id}", flush=True)
            continue

        torch.manual_seed(seed)
        if torch.cuda.is_available():
            torch.cuda.manual_seed_all(seed)
        np.random.seed(seed)
        started = dt.datetime.now(dt.timezone.utc)
        print(f"[{index}/3] GENERATE {line_id}", flush=True)
        wav = model.generate(
            text=text,
            reference_wav_path=str(REFERENCE_WAV),
            **GENERATION_PARAMETERS,
        )
        safe_wav, wav_info = validate_audio(wav, sample_rate)
        temporary = output_path.with_suffix(".wav.part")
        sf.write(str(temporary), safe_wav, sample_rate, subtype="PCM_24", format="WAV")
        os.replace(temporary, output_path)
        item = {
            **params,
            "params_sha256": params_hash,
            "status": "success",
            "output_name": output_name,
            "output_path": str(output_path.resolve()),
            "output_size_bytes": output_path.stat().st_size,
            "output_sha256": sha256_file(output_path),
            "wav": wav_info,
            "elapsed_seconds": round(
                (dt.datetime.now(dt.timezone.utc) - started).total_seconds(),
                3,
            ),
            "completed_at": utc_now(),
        }
        manifest["items"].append(item)
        manifest["updated_at"] = utc_now()
        atomic_json(manifest_path, manifest)
        print(
            f"  OK {wav_info['duration_seconds']:.2f}s "
            f"peak={wav_info['peak']:.3f}",
            flush=True,
        )

    manifest["success_count"] = sum(
        item.get("status") == "success" for item in manifest["items"]
    )
    manifest["completed_at"] = utc_now()
    atomic_json(manifest_path, manifest)
    print(f"COMPLETE {manifest['success_count']}/3", flush=True)
    return 0 if manifest["success_count"] == 3 else 1


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except RuntimeError as exc:
        message = str(exc)
        print(traceback.format_exc(), file=sys.stderr, flush=True)
        if "out of memory" in message.lower():
            if torch.cuda.is_available():
                torch.cuda.empty_cache()
            raise SystemExit(42)
        raise
