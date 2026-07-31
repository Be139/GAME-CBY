#!/usr/bin/env python3
"""Generate the locked ElevenLabs v3 casting-audition batch for HEARTH."""

from __future__ import annotations

import argparse
import datetime as dt
import hashlib
import json
import os
import random
import re
import sys
import time
import urllib.error
import urllib.parse
import urllib.request
from pathlib import Path
from typing import Any


SOURCE_MANIFEST = Path(
    "C:/Users/彩笔/Downloads/HEARTH_ElevenLabs_v3_API_Voice_Lines_Manual_Revision.jsonl"
)
MODEL_ID = "eleven_v3"
OUTPUT_FORMAT = "mp3_44100_128"
VOICE_SETTINGS = {
    "stability": 0.5,
    "similarity_boost": 0.75,
    "style": 0.0,
    "use_speaker_boost": True,
    "speed": 1.0,
}

SELECTIONS = [
    ("Katherine", "Corporate Voice", "Prologue_HEARTHCommercial_CorporateVoice_001"),
    ("Katherine", "Corporate Voice", "Prologue_HEARTHCommercial_CorporateVoice_006"),
    ("Tarquin", "Field Unit", "Lobby_OpeningBriefing_FieldUnit_001"),
    ("Tarquin", "Field Unit", "17F03_AllInspectionsComplete_FieldUnit_001"),
    ("Holly", "Mia", "Lobby_ElevatorRide_Mia_003"),
    ("Holly", "Mia", "17F04_AnswerSelf_Mia_003"),
    (
        "Sky",
        "Lily",
        "17F04_HomeGreeting_High_17F04_HomeGreeting_Low_Lily_001",
    ),
    (
        "Sky",
        "Lily",
        "17F04_Epilogue_High_Retain_17F04_Epilogue_Low_Retain_Lily_006",
    ),
    ("Amanda", "Lobby Girl", "Lobby_Group01_Girl_LobbyGirl_001"),
    ("Amanda", "Lobby Girl", "Lobby_Group01_Girl_LobbyGirl_002"),
    ("John Shaw", "Young Man", "Lobby_Group02_YoungMan_YoungMan_003"),
    ("John Shaw", "Young Man", "Lobby_Group02_YoungMan_YoungMan_005"),
    ("Jane", "Mrs. Ellis", "Lobby_Group03_Grandmother_MrsEllis_001"),
    ("Jane", "Mrs. Ellis", "Lobby_Group03_Grandmother_MrsEllis_003"),
    ("Julian", "Daniel", "17F01_LivingRoomObservation_Daniel_006"),
    ("Julian", "Daniel", "17F01_LivingRoomObservation_Daniel_007"),
    ("Jodi", "Emily", "17F01_LivingRoomObservation_Emily_004"),
    ("Jodi", "Emily", "17F01_LivingRoomObservation_Emily_005"),
    ("Gregory", "Noah", "17F01_BedroomPrelude_Noah_002"),
    ("Gregory", "Noah", "17F01_BedsideSoothing_Noah_003"),
    ("Zane", "Ben", "17F02_DiningObservation_Ben_002"),
    ("Zane", "Ben", "17F02_BlackAudioArgument_Ben_004"),
    ("Clarice", "Claire", "17F02_BedroomComfort_Claire_001"),
    ("Clarice", "Claire", "17F02_BlackAudioArgument_Claire_003"),
    ("Brian", "Mark", "17F03_HumanEntryParents_Mark_001"),
    ("Brian", "Mark", "17F03_PostReplay_B_Mark_001"),
    ("Bex", "Laura", "17F03_MiddayConflict_Laura_001"),
    ("Bex", "Laura", "17F03_PostReplay_A_Laura_002"),
    ("Blondie", "Ava", "17F03_NightDaughter_Ava_001"),
    ("Blondie", "Ava", "17F03_NightDaughter_Ava_003"),
    ("Robert", "17F-01 Home Unit", "17F01_BedsideSoothing_17F01HomeUnit_003"),
    (
        "Robert",
        "17F-03 Synth Voice",
        "17F03_NightShutdown_17F03_NightShutdownAction_17F03SynthVoice_001",
    ),
]


def utc_now() -> str:
    return dt.datetime.now(dt.timezone.utc).isoformat()


def safe_name(value: str) -> str:
    sanitized = re.sub(r"[^A-Za-z0-9._-]+", "_", value).strip("._")
    return sanitized or "voice"


def stable_seed(line_id: str) -> int:
    return int.from_bytes(hashlib.sha256(line_id.encode("utf-8")).digest()[:4], "big")


def atomic_write_json(path: Path, value: Any) -> None:
    temp_path = path.with_suffix(path.suffix + ".tmp")
    temp_path.write_text(
        json.dumps(value, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )
    os.replace(temp_path, path)


def load_jsonl(path: Path) -> list[dict[str, Any]]:
    rows = []
    with path.open("r", encoding="utf-8-sig") as handle:
        for line_number, line in enumerate(handle, 1):
            if not line.strip():
                continue
            try:
                rows.append(json.loads(line))
            except json.JSONDecodeError as exc:
                raise RuntimeError(f"Invalid JSONL at line {line_number}: {exc}") from exc
    return rows


def parse_mp3(data: bytes) -> dict[str, Any]:
    if len(data) < 1024:
        raise RuntimeError(f"MP3 response is unexpectedly small: {len(data)} bytes")

    position = 0
    if data.startswith(b"ID3") and len(data) >= 10:
        tag_size = (
            ((data[6] & 0x7F) << 21)
            | ((data[7] & 0x7F) << 14)
            | ((data[8] & 0x7F) << 7)
            | (data[9] & 0x7F)
        )
        position = 10 + tag_size

    bitrate_v1_l3 = [0, 32, 40, 48, 56, 64, 80, 96, 112, 128, 160, 192, 224, 256, 320, 0]
    bitrate_v2_l3 = [0, 8, 16, 24, 32, 40, 48, 56, 64, 80, 96, 112, 128, 144, 160, 0]
    sample_rates = {
        3: [44100, 48000, 32000],
        2: [22050, 24000, 16000],
        0: [11025, 12000, 8000],
    }

    frames = 0
    total_seconds = 0.0
    first_sample_rate = None
    first_bitrate = None

    while position + 4 <= len(data):
        b0, b1, b2, _b3 = data[position : position + 4]
        if b0 != 0xFF or (b1 & 0xE0) != 0xE0:
            position += 1
            continue

        version_id = (b1 >> 3) & 0x03
        layer_id = (b1 >> 1) & 0x03
        bitrate_index = (b2 >> 4) & 0x0F
        sample_index = (b2 >> 2) & 0x03
        padding = (b2 >> 1) & 0x01

        if version_id == 1 or layer_id != 1 or sample_index == 3:
            position += 1
            continue

        bitrate_table = bitrate_v1_l3 if version_id == 3 else bitrate_v2_l3
        bitrate_kbps = bitrate_table[bitrate_index]
        if bitrate_kbps == 0:
            position += 1
            continue

        sample_rate = sample_rates[version_id][sample_index]
        if version_id == 3:
            frame_length = int((144 * bitrate_kbps * 1000) / sample_rate) + padding
            samples_per_frame = 1152
        else:
            frame_length = int((72 * bitrate_kbps * 1000) / sample_rate) + padding
            samples_per_frame = 576

        if frame_length <= 4 or position + frame_length > len(data):
            break

        frames += 1
        total_seconds += samples_per_frame / sample_rate
        first_sample_rate = first_sample_rate or sample_rate
        first_bitrate = first_bitrate or bitrate_kbps
        position += frame_length

    if frames < 3 or total_seconds <= 0.1:
        raise RuntimeError(
            f"Could not validate MP3 frames: frames={frames}, seconds={total_seconds:.3f}"
        )

    return {
        "frame_count": frames,
        "duration_seconds": round(total_seconds, 3),
        "sample_rate_hz": first_sample_rate,
        "bitrate_kbps": first_bitrate,
    }


def read_error_body(exc: urllib.error.HTTPError) -> dict[str, Any]:
    try:
        payload = json.loads(exc.read().decode("utf-8"))
    except Exception:
        return {"http_status": exc.code}
    detail = payload.get("detail", {}) if isinstance(payload, dict) else {}
    if not isinstance(detail, dict):
        detail = {}
    return {
        "http_status": exc.code,
        "error_type": detail.get("type"),
        "error_code": detail.get("code"),
        "error_message": detail.get("message"),
    }


def request_audio(
    *,
    api_key: str,
    voice_id: str,
    text: str,
    seed: int,
    max_attempts: int,
) -> tuple[bytes, dict[str, Any]]:
    url = (
        "https://api.elevenlabs.io/v1/text-to-speech/"
        f"{urllib.parse.quote(voice_id)}?output_format={OUTPUT_FORMAT}"
    )
    payload = {
        "text": text,
        "model_id": MODEL_ID,
        "voice_settings": VOICE_SETTINGS,
        "seed": seed,
        "apply_text_normalization": "auto",
    }
    request_data = json.dumps(payload, ensure_ascii=False).encode("utf-8")

    for attempt in range(1, max_attempts + 1):
        request = urllib.request.Request(
            url,
            data=request_data,
            method="POST",
            headers={
                "xi-api-key": api_key,
                "Content-Type": "application/json",
                "Accept": "audio/mpeg",
            },
        )
        try:
            with urllib.request.urlopen(request, timeout=120) as response:
                audio = response.read()
                metadata = {
                    "http_status": response.status,
                    "request_id": response.headers.get("request-id"),
                    "trace_id": response.headers.get("x-trace-id"),
                    "character_cost": response.headers.get("character-cost"),
                    "content_type": response.headers.get("content-type"),
                    "attempt": attempt,
                }
                return audio, metadata
        except urllib.error.HTTPError as exc:
            error = read_error_body(exc)
            retryable = exc.code == 429 or exc.code >= 500
            if not retryable or attempt >= max_attempts:
                raise RuntimeError(json.dumps(error, ensure_ascii=False)) from exc
        except urllib.error.URLError as exc:
            if attempt >= max_attempts:
                raise RuntimeError(f"Network error: {type(exc.reason).__name__}") from exc

        delay = min(30.0, (2 ** (attempt - 1)) + random.random())
        print(f"  retrying in {delay:.1f}s (attempt {attempt + 1}/{max_attempts})", flush=True)
        time.sleep(delay)

    raise RuntimeError("Unreachable request retry state")


def build_jobs(
    *,
    rows: list[dict[str, Any]],
    voice_map: dict[str, Any],
    output_dir: Path,
) -> list[dict[str, Any]]:
    by_line_id: dict[str, dict[str, Any]] = {}
    for row in rows:
        line_id = row.get("line_id")
        if line_id in by_line_id:
            raise RuntimeError(f"Duplicate line_id in source JSONL: {line_id}")
        by_line_id[line_id] = row

    speakers = voice_map.get("speakers", {})
    jobs = []
    for voice_alias, expected_speaker, line_id in SELECTIONS:
        row = by_line_id.get(line_id)
        if row is None:
            raise RuntimeError(f"Selected line_id is missing: {line_id}")
        if row.get("speaker") != expected_speaker:
            raise RuntimeError(
                f"Speaker mismatch for {line_id}: "
                f"expected {expected_speaker!r}, got {row.get('speaker')!r}"
            )
        voice = speakers.get(expected_speaker)
        if not voice or not voice.get("voice_id"):
            raise RuntimeError(f"Missing voice mapping for speaker: {expected_speaker}")

        seed = stable_seed(line_id)
        params = {
            "line_id": line_id,
            "speaker": expected_speaker,
            "text": row["text"],
            "voice_alias": voice_alias,
            "voice_id": voice["voice_id"],
            "voice_name": voice["voice_name"],
            "model_id": MODEL_ID,
            "output_format": OUTPUT_FORMAT,
            "voice_settings": VOICE_SETTINGS,
            "seed": seed,
        }
        params_hash = hashlib.sha256(
            json.dumps(params, sort_keys=True, ensure_ascii=False).encode("utf-8")
        ).hexdigest()
        file_name = f"{safe_name(voice_alias)}__{safe_name(line_id)}.mp3"
        jobs.append(
            {
                **params,
                "params_sha256": params_hash,
                "file_name": file_name,
                "file_path": str((output_dir / file_name).resolve()),
            }
        )

    if len(jobs) != 32 or len({job["file_name"] for job in jobs}) != 32:
        raise RuntimeError("Casting batch must contain exactly 32 unique output files")
    return jobs


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo-root", type=Path, required=True)
    parser.add_argument("--max-attempts", type=int, default=5)
    args = parser.parse_args()

    api_key = os.environ.get("ELEVENLABS_API_KEY", "").strip()
    if not api_key:
        print("ELEVENLABS_API_KEY is not available", file=sys.stderr)
        return 2

    repo_root = args.repo_root.resolve()
    voice_map_path = repo_root / "HEARTH_ElevenLabs_Voice_Map.json"
    output_dir = (
        repo_root
        / "GeneratedAudio"
        / "ElevenLabs"
        / "CastingAuditions_2026-07-30"
    )
    output_dir.mkdir(parents=True, exist_ok=True)
    manifest_path = output_dir / "casting_auditions_manifest.json"
    event_log_path = output_dir / "generation_events.jsonl"

    rows = load_jsonl(SOURCE_MANIFEST)
    voice_map = json.loads(voice_map_path.read_text(encoding="utf-8"))
    if voice_map.get("model_id") != MODEL_ID:
        raise RuntimeError("Voice map model_id does not match the locked audition model")
    if voice_map.get("output_format") != OUTPUT_FORMAT:
        raise RuntimeError("Voice map output_format does not match the locked audition format")

    jobs = build_jobs(rows=rows, voice_map=voice_map, output_dir=output_dir)
    existing_by_line_id: dict[str, dict[str, Any]] = {}
    if manifest_path.exists():
        existing_manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
        existing_by_line_id = {
            item["line_id"]: item for item in existing_manifest.get("items", [])
        }

    manifest = {
        "batch_name": "HEARTH Eleven v3 Casting Auditions",
        "source_manifest": str(SOURCE_MANIFEST),
        "voice_map": str(voice_map_path),
        "model_id": MODEL_ID,
        "output_format": OUTPUT_FORMAT,
        "voice_settings": VOICE_SETTINGS,
        "expected_count": 32,
        "updated_at": utc_now(),
        "items": [],
    }

    total = len(jobs)
    for index, job in enumerate(jobs, 1):
        output_path = Path(job["file_path"])
        previous = existing_by_line_id.get(job["line_id"], {})
        if (
            previous.get("status") == "success"
            and previous.get("params_sha256") == job["params_sha256"]
            and output_path.exists()
            and output_path.stat().st_size == previous.get("file_size_bytes")
        ):
            manifest["items"].append(previous)
            manifest["updated_at"] = utc_now()
            atomic_write_json(manifest_path, manifest)
            print(f"[{index:02d}/{total}] SKIP {job['file_name']}", flush=True)
            continue

        print(
            f"[{index:02d}/{total}] GENERATE "
            f"{job['voice_alias']} :: {job['line_id']}",
            flush=True,
        )
        event = {
            "timestamp": utc_now(),
            "event": "generation_started",
            "line_id": job["line_id"],
            "voice_alias": job["voice_alias"],
            "params_sha256": job["params_sha256"],
        }
        with event_log_path.open("a", encoding="utf-8") as event_log:
            event_log.write(json.dumps(event, ensure_ascii=False) + "\n")

        try:
            audio, response_meta = request_audio(
                api_key=api_key,
                voice_id=job["voice_id"],
                text=job["text"],
                seed=job["seed"],
                max_attempts=args.max_attempts,
            )
            mp3_info = parse_mp3(audio)
            temp_audio_path = output_path.with_suffix(output_path.suffix + ".part")
            temp_audio_path.write_bytes(audio)
            os.replace(temp_audio_path, output_path)
            item = {
                **job,
                "status": "success",
                "generated_at": utc_now(),
                "file_size_bytes": len(audio),
                "audio_sha256": hashlib.sha256(audio).hexdigest(),
                "mp3": mp3_info,
                **response_meta,
            }
            manifest["items"].append(item)
            event = {
                "timestamp": utc_now(),
                "event": "generation_succeeded",
                "line_id": job["line_id"],
                "voice_alias": job["voice_alias"],
                "request_id": response_meta.get("request_id"),
                "character_cost": response_meta.get("character_cost"),
                "file_name": job["file_name"],
                "file_size_bytes": len(audio),
                "audio_sha256": item["audio_sha256"],
                "duration_seconds": mp3_info["duration_seconds"],
            }
            with event_log_path.open("a", encoding="utf-8") as event_log:
                event_log.write(json.dumps(event, ensure_ascii=False) + "\n")
            print(
                f"  OK {len(audio)} bytes, "
                f"{mp3_info['duration_seconds']:.2f}s, "
                f"{mp3_info['sample_rate_hz']} Hz",
                flush=True,
            )
        except Exception as exc:
            item = {
                **job,
                "status": "failed",
                "failed_at": utc_now(),
                "error": str(exc),
            }
            manifest["items"].append(item)
            event = {
                "timestamp": utc_now(),
                "event": "generation_failed",
                "line_id": job["line_id"],
                "voice_alias": job["voice_alias"],
                "error": str(exc),
            }
            with event_log_path.open("a", encoding="utf-8") as event_log:
                event_log.write(json.dumps(event, ensure_ascii=False) + "\n")
            manifest["updated_at"] = utc_now()
            atomic_write_json(manifest_path, manifest)
            print(f"  FAILED {exc}", file=sys.stderr, flush=True)
            return 1

        manifest["updated_at"] = utc_now()
        atomic_write_json(manifest_path, manifest)

    succeeded = sum(item.get("status") == "success" for item in manifest["items"])
    manifest["completed_at"] = utc_now()
    manifest["success_count"] = succeeded
    manifest["failed_count"] = total - succeeded
    atomic_write_json(manifest_path, manifest)
    print(f"COMPLETE success={succeeded}/{total}", flush=True)
    return 0 if succeeded == total else 1


if __name__ == "__main__":
    raise SystemExit(main())
