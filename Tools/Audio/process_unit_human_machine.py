#!/usr/bin/env python3
"""Create review WAVs with deterministic human-machine Unit effects."""

from __future__ import annotations

import argparse
import array
import datetime as dt
import hashlib
import json
import math
import os
import re
import subprocess
import wave
from pathlib import Path
from typing import Any


SOURCE_JSONL = Path(
    "C:/Users/彩笔/Downloads/HEARTH_ElevenLabs_v3_API_Voice_Lines_Manual_Revision.jsonl"
)
FFMPEG = Path("E:/VoxCPM/ffmpeg/ffmpeg-8.1.2-full_build-shared/bin/ffmpeg.exe")
FFPROBE = FFMPEG.with_name("ffprobe.exe")
EXPECTED_FULL_COUNT = 147
PREVIEW_LINE_IDS = [
    "Lobby_OpeningBriefing_FieldUnit_001",
    "Lobby_Group01_Girl_PublicUnit_001",
    "Lobby_Group02_YoungMan_WorkUnit_003",
    "Lobby_Group03_Grandmother_CareUnit_004",
    "17F01_BedsideSoothing_17F01HomeUnit_003",
    "17F04_HomeGreeting_High_17F04_HomeGreeting_Low_MiasHomeUnit_001",
    "17F03_NightShutdown_17F03_NightShutdownAction_17F03SynthVoice_001",
]
FLANGER_PREVIEW_LINE_IDS = [
    "Lobby_OpeningBriefing_FieldUnit_001",
    "17F01_BedsideSoothing_17F01HomeUnit_003",
]
BILIBILI_REFERENCE = {
    "url": "https://www.bilibili.com/video/BV1be411675C/",
    "bvid": "BV1be411675C",
    "title": "思源梦机器人声音模仿",
    "original_time_range_seconds": [20.0, 25.0],
    "target_effect_time_range_seconds": [39.0, 43.0],
}
AUDITION_FLANGER_BASE = {
    "initial_delay_ms": 0.0,
    "final_delay_ms": 6.22,
    "feedback_percent": 0.0,
    "modulation_rate_hz": 7.0,
    "wave_shape": "sinusoidal",
    "stereo_phase_degrees": 180.0,
    "ffmpeg_phase_percent": 50.0,
    "interpolation": "quadratic",
    "special_effects_mode": True,
    "loudness_normalization_passes": 2,
}
AUDITION_FLANGER_MIXES = {
    "Light": {"dry": 0.70, "special": 0.30},
    "Balanced": {"dry": 0.60, "special": 0.40},
    "Reference": {"dry": 0.50, "special": 0.50},
    "Strong": {"dry": 0.30, "special": 0.70},
}

FILTERS = {
    "WarmUnit": (
        "[0:a]aresample=48000,asplit=2[dry][fx];"
        "[fx]highpass=f=95,lowpass=f=8800,"
        "equalizer=f=2100:t=q:w=1.4:g=2.5,"
        "tremolo=f=26:d=0.18,"
        "aphaser=in_gain=0.4:out_gain=0.7:delay=3:"
        "decay=0.35:speed=0.45:type=t[robot];"
        "[dry][robot]amix=inputs=2:weights='0.70 0.30':normalize=0,"
        "loudnorm=I=-18:LRA=7:TP=-1.5[out]"
    ),
    "SynthVoice": (
        "[0:a]aresample=48000,asplit=2[dry][fx];"
        "[fx]highpass=f=120,lowpass=f=7000,"
        "equalizer=f=1850:t=q:w=1.1:g=4.0,"
        "tremolo=f=35:d=0.36,"
        "aphaser=in_gain=0.45:out_gain=0.75:delay=3:"
        "decay=0.45:speed=0.70:type=t,"
        "chorus=0.5:0.65:8:0.22:0.45:0.8[robot];"
        "[dry][robot]amix=inputs=2:weights='0.55 0.45':normalize=0,"
        "loudnorm=I=-19:LRA=5:TP=-1.5[out]"
    ),
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


def load_jsonl(path: Path) -> list[dict[str, Any]]:
    return [
        json.loads(line)
        for line in path.read_text(encoding="utf-8-sig").splitlines()
        if line.strip()
    ]


def is_unit_speaker(speaker: str) -> bool:
    return speaker == "Field Unit" or "Unit" in speaker or "Synth Voice" in speaker


def profile_for(speaker: str) -> str:
    return "SynthVoice" if "Synth Voice" in speaker else "WarmUnit"


def build_audition_flanger_filter(dry_mix: float, special_mix: float) -> str:
    return (
        "[0:a]aresample=48000,aformat=channel_layouts=mono,"
        "asplit=3[dry][lead][fxin];"
        "[fxin]pan=stereo|c0=c0|c1=c0,"
        "flanger=delay=0:depth=6.22:regen=0:width=100:"
        "speed=7:shape=sinusoidal:phase=50:interp=quadratic,"
        "pan=mono|c0=0.5*c0+0.5*c1[flanged];"
        "[lead]volume=-1[inverted];"
        "[flanged][inverted]amix=inputs=2:weights='1 1':normalize=0[special];"
        f"[dry][special]amix=inputs=2:weights='{dry_mix:.2f} {special_mix:.2f}':"
        "normalize=0,loudnorm=I=-18:LRA=7:TP=-1.5,"
        "loudnorm=I=-18:LRA=7:TP=-1.5[out]"
    )


AUDITION_FLANGER_FILTERS = {
    strength: build_audition_flanger_filter(mix["dry"], mix["special"])
    for strength, mix in AUDITION_FLANGER_MIXES.items()
}


def validate_wav(path: Path) -> dict[str, Any]:
    with wave.open(str(path), "rb") as audio:
        channels = audio.getnchannels()
        sample_width = audio.getsampwidth()
        sample_rate = audio.getframerate()
        frames = audio.getnframes()
        raw = audio.readframes(frames)
    if channels != 1 or sample_width != 2 or sample_rate != 48000 or frames <= 0:
        raise RuntimeError(
            f"Invalid WAV: channels={channels}, width={sample_width}, "
            f"rate={sample_rate}, frames={frames}"
        )
    samples = array.array("h")
    samples.frombytes(raw)
    peak = max(abs(sample) for sample in samples) / 32768.0
    rms = math.sqrt(sum(sample * sample for sample in samples) / len(samples)) / 32768.0
    if not math.isfinite(peak) or not math.isfinite(rms) or rms < 0.0001:
        raise RuntimeError(f"Invalid audio levels: peak={peak}, rms={rms}")
    return {
        "sample_rate_hz": sample_rate,
        "channels": channels,
        "sample_width_bytes": sample_width,
        "frame_count": frames,
        "duration_seconds": round(frames / sample_rate, 3),
        "peak": round(peak, 6),
        "rms": round(rms, 6),
    }


def probe_duration(path: Path) -> float:
    result = subprocess.run(
        [
            str(FFPROBE),
            "-v",
            "error",
            "-show_entries",
            "format=duration",
            "-of",
            "default=noprint_wrappers=1:nokey=1",
            str(path),
        ],
        check=True,
        capture_output=True,
        text=True,
        encoding="utf-8",
        errors="replace",
    )
    return float(result.stdout.strip())


def measure_loudness(path: Path) -> dict[str, float]:
    result = subprocess.run(
        [
            str(FFMPEG),
            "-hide_banner",
            "-nostats",
            "-i",
            str(path),
            "-af",
            "loudnorm=I=-18:LRA=7:TP=-1.5:print_format=json",
            "-f",
            "null",
            os.devnull,
        ],
        check=True,
        capture_output=True,
        text=True,
        encoding="utf-8",
        errors="replace",
    )
    matches = re.findall(r'\{\s*"input_i".*?\}', result.stderr, flags=re.DOTALL)
    if not matches:
        raise RuntimeError(f"Unable to read loudness measurements for {path}")
    values = json.loads(matches[-1])
    integrated = float(values["input_i"])
    true_peak = float(values["input_tp"])
    if not -60.0 <= integrated <= 0.0:
        raise RuntimeError(f"Integrated loudness is invalid: {integrated} LUFS")
    if true_peak > -1.4:
        raise RuntimeError(f"True peak exceeds target: {true_peak} dBTP")
    return {
        "integrated_lufs": integrated,
        "true_peak_dbtp": true_peak,
        "loudness_range_lu": float(values["input_lra"]),
        "threshold_lufs": float(values["input_thresh"]),
    }


def process_audition_flanger_preview(
    repo_root: Path,
    source_audio_dir: Path,
    source_manifest_path: Path,
    source_items: dict[str, dict[str, Any]],
    unit_rows: list[dict[str, Any]],
) -> int:
    output_root = (
        repo_root
        / "GeneratedAudio"
        / "UnitFlangerReference_Review_2026-07-31"
    )
    output_root.mkdir(parents=True, exist_ok=True)
    manifest_path = output_root / "unit_flanger_reference_preview_manifest.json"
    rows_by_id = {row["line_id"]: row for row in unit_rows}
    rows = []
    for line_id in FLANGER_PREVIEW_LINE_IDS:
        row = rows_by_id.get(line_id)
        if row is None:
            raise RuntimeError(f"Locked flanger preview line is missing: {line_id}")
        rows.append(row)

    prior_by_key: dict[str, dict[str, Any]] = {}
    if manifest_path.exists():
        prior = json.loads(manifest_path.read_text(encoding="utf-8"))
        prior_by_key = {
            f"{item['line_id']}|{item['strength']}": item
            for item in prior.get("items", [])
        }

    manifest: dict[str, Any] = {
        "batch": "HEARTH Unit Audition Flanger Reference Preview",
        "effect_group": "AuditionFlangerReference",
        "source_jsonl": str(SOURCE_JSONL),
        "source_audio_manifest": str(source_manifest_path),
        "reference": BILIBILI_REFERENCE,
        "audition_flanger_base": AUDITION_FLANGER_BASE,
        "mix_profiles": AUDITION_FLANGER_MIXES,
        "filters": AUDITION_FLANGER_FILTERS,
        "output_format": {
            "sample_rate_hz": 48000,
            "channels": 1,
            "sample_width_bits": 16,
            "codec": "pcm_s16le",
            "target_integrated_lufs": -18.0,
            "maximum_true_peak_dbtp": -1.5,
        },
        "corporate_voice_included": False,
        "expected_count": len(rows) * len(AUDITION_FLANGER_MIXES),
        "updated_at": utc_now(),
        "items": [],
    }

    jobs = [
        (row, strength, mix)
        for strength, mix in AUDITION_FLANGER_MIXES.items()
        for row in rows
    ]
    for index, (row, strength, mix) in enumerate(jobs, 1):
        line_id = row["line_id"]
        source_item = source_items.get(line_id)
        if source_item is None:
            raise RuntimeError(f"Source audio is missing for {line_id}")
        source_path = source_audio_dir / source_item["file_name"]
        if not source_path.is_file():
            raise RuntimeError(f"Source audio file is missing: {source_path}")
        source_hash = sha256_file(source_path)
        source_duration = probe_duration(source_path)
        output_name = f"{line_id}__AuditionFlangerReference_{strength}.wav"
        output_path = output_root / output_name
        filter_graph = AUDITION_FLANGER_FILTERS[strength]
        params = {
            "line_id": line_id,
            "speaker": row["speaker"],
            "text": row["text"],
            "source_path": str(source_path.resolve()),
            "source_sha256": source_hash,
            "source_duration_seconds": round(source_duration, 3),
            "effect_group": "AuditionFlangerReference",
            "strength": strength,
            "mix": mix,
            "filter": filter_graph,
            "sample_rate_hz": 48000,
            "channels": 1,
            "codec": "pcm_s16le",
        }
        params_hash = hashlib.sha256(
            json.dumps(params, ensure_ascii=False, sort_keys=True).encode("utf-8")
        ).hexdigest()
        key = f"{line_id}|{strength}"
        previous = prior_by_key.get(key, {})
        if (
            previous.get("status") == "success"
            and previous.get("params_sha256") == params_hash
            and output_path.is_file()
            and previous.get("output_sha256") == sha256_file(output_path)
        ):
            manifest["items"].append(previous)
            print(
                f"[{index:02d}/{len(jobs):02d}] SKIP {strength} {line_id}",
                flush=True,
            )
            continue

        temporary = output_path.with_suffix(".wav.part")
        command = [
            str(FFMPEG),
            "-hide_banner",
            "-loglevel",
            "error",
            "-y",
            "-i",
            str(source_path),
            "-filter_complex",
            filter_graph,
            "-map",
            "[out]",
            "-ar",
            "48000",
            "-ac",
            "1",
            "-c:a",
            "pcm_s16le",
            "-f",
            "wav",
            str(temporary),
        ]
        started = dt.datetime.now(dt.timezone.utc)
        subprocess.run(command, check=True)
        os.replace(temporary, output_path)
        wav_info = validate_wav(output_path)
        if abs(wav_info["duration_seconds"] - source_duration) > 0.1:
            raise RuntimeError(
                f"Unexpected duration change for {line_id}: "
                f"{source_duration:.3f}s -> {wav_info['duration_seconds']:.3f}s"
            )
        loudness = measure_loudness(output_path)
        elapsed = (dt.datetime.now(dt.timezone.utc) - started).total_seconds()
        item = {
            **params,
            "params_sha256": params_hash,
            "status": "success",
            "output_name": output_name,
            "output_path": str(output_path.resolve()),
            "output_size_bytes": output_path.stat().st_size,
            "output_sha256": sha256_file(output_path),
            "wav": wav_info,
            "loudness": loudness,
            "elapsed_seconds": round(elapsed, 3),
            "completed_at": utc_now(),
        }
        manifest["items"].append(item)
        manifest["updated_at"] = utc_now()
        atomic_json(manifest_path, manifest)
        print(
            f"[{index:02d}/{len(jobs):02d}] OK {strength} {line_id} "
            f"{wav_info['duration_seconds']:.2f}s "
            f"{loudness['integrated_lufs']:.2f} LUFS",
            flush=True,
        )

    manifest["success_count"] = sum(
        item.get("status") == "success" for item in manifest["items"]
    )
    manifest["completed_at"] = utc_now()
    atomic_json(manifest_path, manifest)
    print(
        f"COMPLETE {manifest['success_count']}/{manifest['expected_count']}",
        flush=True,
    )
    return 0 if manifest["success_count"] == manifest["expected_count"] else 1


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo-root", type=Path, required=True)
    parser.add_argument("--mode", choices=("preview", "full"), default="preview")
    parser.add_argument(
        "--effect-set",
        choices=("legacy", "audition-flanger-reference"),
        default="legacy",
    )
    args = parser.parse_args()

    repo_root = args.repo_root.resolve()
    source_audio_dir = (
        repo_root / "GeneratedAudio" / "ElevenLabs" / "FullDialogue_2026-07-30"
    )
    source_manifest_path = source_audio_dir / "full_dialogue_manifest.json"
    output_root = (
        repo_root
        / "GeneratedAudio"
        / "UnitHumanMachine_Review_2026-07-31"
    )
    output_root.mkdir(parents=True, exist_ok=True)
    manifest_path = output_root / f"unit_human_machine_{args.mode}_manifest.json"

    if not FFMPEG.is_file():
        raise RuntimeError(f"FFmpeg is missing: {FFMPEG}")
    if not FFPROBE.is_file():
        raise RuntimeError(f"FFprobe is missing: {FFPROBE}")
    source_manifest = json.loads(source_manifest_path.read_text(encoding="utf-8"))
    source_items = {
        item["line_id"]: item
        for item in source_manifest["items"]
        if item.get("status") == "success"
    }
    rows = [row for row in load_jsonl(SOURCE_JSONL) if is_unit_speaker(row["speaker"])]
    if len(rows) != EXPECTED_FULL_COUNT:
        raise RuntimeError(f"Expected 147 Unit lines, got {len(rows)}")
    if args.effect_set == "audition-flanger-reference":
        if args.mode != "preview":
            raise RuntimeError(
                "Audition flanger reference is locked to preview mode until approval"
            )
        return process_audition_flanger_preview(
            repo_root,
            source_audio_dir,
            source_manifest_path,
            source_items,
            rows,
        )
    if args.mode == "preview":
        wanted = set(PREVIEW_LINE_IDS)
        rows = [row for row in rows if row["line_id"] in wanted]
        if len(rows) != len(PREVIEW_LINE_IDS):
            raise RuntimeError("One or more locked preview lines are missing")

    prior_by_line: dict[str, dict[str, Any]] = {}
    if manifest_path.exists():
        prior = json.loads(manifest_path.read_text(encoding="utf-8"))
        prior_by_line = {item["line_id"]: item for item in prior.get("items", [])}

    manifest: dict[str, Any] = {
        "batch": f"HEARTH Unit Human-Machine {args.mode.title()}",
        "mode": args.mode,
        "source_jsonl": str(SOURCE_JSONL),
        "source_audio_manifest": str(source_manifest_path),
        "corporate_voice_included": False,
        "expected_count": len(rows),
        "profiles": FILTERS,
        "updated_at": utc_now(),
        "items": [],
    }

    for index, row in enumerate(rows, 1):
        line_id = row["line_id"]
        source_item = source_items.get(line_id)
        if source_item is None:
            raise RuntimeError(f"Source audio is missing for {line_id}")
        source_path = source_audio_dir / source_item["file_name"]
        profile = profile_for(row["speaker"])
        output_dir = output_root / profile
        output_dir.mkdir(parents=True, exist_ok=True)
        output_name = f"{line_id}__human_machine_{profile}.wav"
        output_path = output_dir / output_name
        source_hash = sha256_file(source_path)
        params = {
            "line_id": line_id,
            "speaker": row["speaker"],
            "text": row["text"],
            "source_path": str(source_path.resolve()),
            "source_sha256": source_hash,
            "profile": profile,
            "filter": FILTERS[profile],
            "sample_rate_hz": 48000,
            "channels": 1,
            "codec": "pcm_s16le",
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
            print(f"[{index:03d}/{len(rows):03d}] SKIP {line_id}", flush=True)
            continue

        temporary = output_path.with_suffix(".wav.part")
        command = [
            str(FFMPEG),
            "-hide_banner",
            "-loglevel",
            "error",
            "-y",
            "-i",
            str(source_path),
            "-filter_complex",
            FILTERS[profile],
            "-map",
            "[out]",
            "-ar",
            "48000",
            "-ac",
            "1",
            "-c:a",
            "pcm_s16le",
            "-f",
            "wav",
            str(temporary),
        ]
        started = dt.datetime.now(dt.timezone.utc)
        subprocess.run(command, check=True)
        os.replace(temporary, output_path)
        wav_info = validate_wav(output_path)
        elapsed = (dt.datetime.now(dt.timezone.utc) - started).total_seconds()
        item = {
            **params,
            "params_sha256": params_hash,
            "status": "success",
            "output_name": output_name,
            "output_path": str(output_path.resolve()),
            "output_size_bytes": output_path.stat().st_size,
            "output_sha256": sha256_file(output_path),
            "wav": wav_info,
            "elapsed_seconds": round(elapsed, 3),
            "completed_at": utc_now(),
        }
        manifest["items"].append(item)
        manifest["updated_at"] = utc_now()
        atomic_json(manifest_path, manifest)
        print(
            f"[{index:03d}/{len(rows):03d}] OK {profile} {line_id} "
            f"{wav_info['duration_seconds']:.2f}s",
            flush=True,
        )

    manifest["success_count"] = sum(
        item.get("status") == "success" for item in manifest["items"]
    )
    manifest["completed_at"] = utc_now()
    atomic_json(manifest_path, manifest)
    print(
        f"COMPLETE {manifest['success_count']}/{manifest['expected_count']}",
        flush=True,
    )
    return 0 if manifest["success_count"] == manifest["expected_count"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
