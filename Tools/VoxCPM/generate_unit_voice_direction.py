#!/usr/bin/env python3
"""Generate one locked Robert-based strong human-machine VoxCPM direction sample."""

from __future__ import annotations

import argparse
import datetime as dt
import hashlib
import json
import math
import os
import subprocess
from pathlib import Path
from typing import Any

import numpy as np
import soundfile as sf
import torch
from voxcpm import VoxCPM


MODEL_DIR = Path("E:/VoxCPM/models/VoxCPM2")
FFMPEG = Path("E:/VoxCPM/ffmpeg/ffmpeg-8.1.2-full_build-shared/bin/ffmpeg.exe")
REFERENCE_SOURCE_NAME = "Robert__17F01_BedsideSoothing_17F01HomeUnit_003.mp3"
REFERENCE_WAV = Path(
    "E:/VoxCPM/reference_audio/"
    "Robert_17F01_BedsideSoothing_HomeUnit_003_16k_mono.wav"
)
TARGET_ID = "FieldCompanionOpening_StrongHumanMachineDirection"
TARGET_TEXT = (
    "Good evening, Inspector. Field Companion Unit online. "
    "I'll be your partner tonight."
)
CONTROL = (
    "a warm companion artificial intelligence voice, unmistakably synthetic "
    "yet reassuring, professional and controlled, precise machine-timed "
    "phrasing, stable pitch, moderate metallic resonance, narrow-band speaker "
    "coloration, audible light vocoder texture, minimal human breathiness, "
    "crisp intelligible consonants, no harsh distortion or buzzing"
)
DEFAULT_CFG_VALUE = 2.5
INFERENCE_TIMESTEPS = 10


def utc_now() -> str:
    return dt.datetime.now(dt.timezone.utc).isoformat()


def stable_seed(value: str) -> int:
    return int.from_bytes(hashlib.sha256(value.encode("utf-8")).digest()[:4], "big")


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
    rms_before = float(math.sqrt(float(np.mean(values * values))))
    if rms_before < 0.0001:
        raise RuntimeError(f"Generated audio is effectively silent: rms={rms_before}")
    safety_gain = 1.0
    if peak_before > 0.98:
        safety_gain = 0.98 / peak_before
        values = values * safety_gain
    return values, {
        "sample_rate_hz": sample_rate,
        "channels": 1,
        "sample_count": int(values.size),
        "duration_seconds": round(values.size / sample_rate, 3),
        "peak_before_safety_gain": round(peak_before, 6),
        "safety_gain": round(safety_gain, 6),
        "peak": round(float(np.max(np.abs(values))), 6),
        "rms": round(rms_before * safety_gain, 6),
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo-root", type=Path, required=True)
    parser.add_argument("--cfg-value", type=float, default=DEFAULT_CFG_VALUE)
    args = parser.parse_args()
    if not 0.1 <= args.cfg_value <= 10.0:
        raise RuntimeError("--cfg-value must be between 0.1 and 10.0")

    repo_root = args.repo_root.resolve()
    source_audio_dir = (
        repo_root / "GeneratedAudio" / "ElevenLabs" / "FullDialogue_2026-07-30"
    )
    reference_source = source_audio_dir / REFERENCE_SOURCE_NAME
    output_dir = (
        repo_root
        / "GeneratedAudio"
        / "VoxCPM"
        / "UnitVoiceDirection_Review_2026-07-31"
    )
    output_dir.mkdir(parents=True, exist_ok=True)
    output_name = (
        "FieldCompanionOpening__RobertReference__"
        f"StrongHumanMachineDirection_cfg{args.cfg_value:.1f}.wav"
    )
    output_path = output_dir / output_name
    manifest_path = (
        output_dir
        / f"unit_voice_direction_cfg{args.cfg_value:.1f}_manifest.json"
    )

    if not reference_source.is_file():
        raise RuntimeError(f"Reference source is missing: {reference_source}")
    if not MODEL_DIR.is_dir():
        raise RuntimeError(f"Model directory is missing: {MODEL_DIR}")
    if not FFMPEG.is_file():
        raise RuntimeError(f"FFmpeg is missing: {FFMPEG}")
    prepare_reference(reference_source)

    seed = stable_seed(TARGET_ID)
    controlled_text = f"({CONTROL}){TARGET_TEXT}"
    params = {
        "target_id": TARGET_ID,
        "target_text": TARGET_TEXT,
        "control": CONTROL,
        "controlled_text": controlled_text,
        "cfg_value": args.cfg_value,
        "inference_timesteps": INFERENCE_TIMESTEPS,
        "normalize": False,
        "denoise": False,
        "retry_badcase": True,
        "seed": seed,
        "device": "cuda",
        "model_path": str(MODEL_DIR),
        "load_denoiser": False,
        "optimize": False,
        "reference_source": str(reference_source.resolve()),
        "reference_source_sha256": sha256_file(reference_source),
        "reference_wav": str(REFERENCE_WAV),
        "reference_wav_sha256": sha256_file(REFERENCE_WAV),
    }
    params_hash = hashlib.sha256(
        json.dumps(params, ensure_ascii=False, sort_keys=True).encode("utf-8")
    ).hexdigest()
    if manifest_path.exists() and output_path.exists():
        previous = json.loads(manifest_path.read_text(encoding="utf-8"))
        if (
            previous.get("status") == "success"
            and previous.get("params_sha256") == params_hash
            and previous.get("output_sha256") == sha256_file(output_path)
        ):
            print(f"SKIP existing verified output: {output_path}", flush=True)
            return 0

    manifest: dict[str, Any] = {
        "batch": "HEARTH Unit Strong Human-Machine Direction",
        **params,
        "params_sha256": params_hash,
        "status": "loading_model",
        "started_at": utc_now(),
    }
    atomic_json(manifest_path, manifest)

    print("Loading VoxCPM2 on cuda...", flush=True)
    load_started = dt.datetime.now(dt.timezone.utc)
    model = VoxCPM.from_pretrained(
        str(MODEL_DIR),
        load_denoiser=False,
        local_files_only=True,
        optimize=False,
        device="cuda",
    )
    sample_rate = int(model.tts_model.sample_rate)
    manifest["model_load_seconds"] = round(
        (dt.datetime.now(dt.timezone.utc) - load_started).total_seconds(),
        3,
    )
    manifest["model_sample_rate_hz"] = sample_rate
    manifest["status"] = "generating"
    atomic_json(manifest_path, manifest)

    torch.manual_seed(seed)
    torch.cuda.manual_seed_all(seed)
    np.random.seed(seed)
    generation_started = dt.datetime.now(dt.timezone.utc)
    print("Generating strong human-machine direction sample...", flush=True)
    wav = model.generate(
        text=controlled_text,
        reference_wav_path=str(REFERENCE_WAV),
        cfg_value=args.cfg_value,
        inference_timesteps=INFERENCE_TIMESTEPS,
        normalize=False,
        denoise=False,
        retry_badcase=True,
    )
    safe_wav, wav_info = validate_audio(wav, sample_rate)
    temporary = output_path.with_suffix(".wav.part")
    sf.write(str(temporary), safe_wav, sample_rate, subtype="PCM_24", format="WAV")
    os.replace(temporary, output_path)

    manifest.update(
        {
            "status": "success",
            "generation_seconds": round(
                (dt.datetime.now(dt.timezone.utc) - generation_started).total_seconds(),
                3,
            ),
            "output_name": output_name,
            "output_path": str(output_path.resolve()),
            "output_size_bytes": output_path.stat().st_size,
            "output_sha256": sha256_file(output_path),
            "wav": wav_info,
            "completed_at": utc_now(),
        }
    )
    atomic_json(manifest_path, manifest)
    print(
        f"COMPLETE {output_path} {wav_info['duration_seconds']:.2f}s "
        f"peak={wav_info['peak']:.3f}",
        flush=True,
    )
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
