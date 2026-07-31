#!/usr/bin/env python3
"""Generate every HEARTH JSONL line as a separate ElevenLabs v3 MP3."""

from __future__ import annotations

import argparse
import hashlib
import json
import os
import shutil
import sys
from pathlib import Path
from typing import Any

from generate_casting_auditions import (
    MODEL_ID,
    OUTPUT_FORMAT,
    SOURCE_MANIFEST,
    VOICE_SETTINGS,
    atomic_write_json,
    load_jsonl,
    parse_mp3,
    request_audio,
    safe_name,
    stable_seed,
    utc_now,
)


EXPECTED_COUNT = 338
BATCH_FOLDER = "FullDialogue_2026-07-30"


def voice_alias(voice_name: str) -> str:
    return voice_name.split(" - ", 1)[0].strip()


def build_jobs(
    *,
    rows: list[dict[str, Any]],
    voice_map: dict[str, Any],
    output_dir: Path,
) -> list[dict[str, Any]]:
    speakers = voice_map.get("speakers", {})
    jobs: list[dict[str, Any]] = []
    seen_line_ids: set[str] = set()

    for source_index, row in enumerate(rows, 1):
        line_id = row.get("line_id")
        speaker = row.get("speaker")
        text = row.get("text")
        if not isinstance(line_id, str) or not line_id:
            raise RuntimeError(f"Missing line_id in source row {source_index}")
        if line_id in seen_line_ids:
            raise RuntimeError(f"Duplicate line_id in source JSONL: {line_id}")
        seen_line_ids.add(line_id)
        if not isinstance(speaker, str) or not speaker:
            raise RuntimeError(f"Missing speaker for {line_id}")
        if not isinstance(text, str) or not text.strip():
            raise RuntimeError(f"Missing text for {line_id}")

        voice = speakers.get(speaker)
        if not voice or not voice.get("voice_id") or not voice.get("voice_name"):
            raise RuntimeError(f"Missing voice mapping for speaker: {speaker}")

        alias = voice_alias(voice["voice_name"])
        seed = stable_seed(line_id)
        params = {
            "line_id": line_id,
            "speaker": speaker,
            "text": text,
            "voice_alias": alias,
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
        file_name = f"{safe_name(alias)}__{safe_name(line_id)}.mp3"
        jobs.append(
            {
                **params,
                "source_index": source_index,
                "params_sha256": params_hash,
                "file_name": file_name,
                "file_path": str((output_dir / file_name).resolve()),
            }
        )

    if len(jobs) != EXPECTED_COUNT:
        raise RuntimeError(
            f"Full dialogue batch must contain {EXPECTED_COUNT} rows, got {len(jobs)}"
        )
    if len({job["file_name"] for job in jobs}) != EXPECTED_COUNT:
        raise RuntimeError("Full dialogue batch contains duplicate output file names")
    return jobs


def valid_existing(item: dict[str, Any], job: dict[str, Any], path: Path) -> bool:
    if (
        item.get("status") != "success"
        or item.get("params_sha256") != job["params_sha256"]
        or not path.is_file()
        or path.stat().st_size != item.get("file_size_bytes")
    ):
        return False
    audio = path.read_bytes()
    return hashlib.sha256(audio).hexdigest() == item.get("audio_sha256")


def append_event(path: Path, event: dict[str, Any]) -> None:
    with path.open("a", encoding="utf-8") as handle:
        handle.write(json.dumps(event, ensure_ascii=False) + "\n")


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
    output_dir = repo_root / "GeneratedAudio" / "ElevenLabs" / BATCH_FOLDER
    output_dir.mkdir(parents=True, exist_ok=True)
    manifest_path = output_dir / "full_dialogue_manifest.json"
    event_log_path = output_dir / "generation_events.jsonl"
    audition_dir = (
        repo_root
        / "GeneratedAudio"
        / "ElevenLabs"
        / "CastingAuditions_2026-07-30"
    )
    audition_manifest_path = audition_dir / "casting_auditions_manifest.json"

    rows = load_jsonl(SOURCE_MANIFEST)
    voice_map = json.loads(voice_map_path.read_text(encoding="utf-8"))
    if voice_map.get("model_id") != MODEL_ID:
        raise RuntimeError("Voice map model_id does not match the locked model")
    if voice_map.get("output_format") != OUTPUT_FORMAT:
        raise RuntimeError("Voice map output_format does not match the locked format")

    jobs = build_jobs(rows=rows, voice_map=voice_map, output_dir=output_dir)
    previous_by_line_id: dict[str, dict[str, Any]] = {}
    if manifest_path.exists():
        previous_manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
        previous_by_line_id = {
            item["line_id"]: item for item in previous_manifest.get("items", [])
        }

    audition_by_line_id: dict[str, dict[str, Any]] = {}
    if audition_manifest_path.exists():
        audition_manifest = json.loads(
            audition_manifest_path.read_text(encoding="utf-8")
        )
        audition_by_line_id = {
            item["line_id"]: item for item in audition_manifest.get("items", [])
        }

    manifest: dict[str, Any] = {
        "batch_name": "HEARTH Eleven v3 Full Dialogue",
        "source_manifest": str(SOURCE_MANIFEST),
        "voice_map": str(voice_map_path),
        "model_id": MODEL_ID,
        "output_format": OUTPUT_FORMAT,
        "voice_settings": VOICE_SETTINGS,
        "expected_count": EXPECTED_COUNT,
        "updated_at": utc_now(),
        "items": [],
    }

    for index, job in enumerate(jobs, 1):
        output_path = Path(job["file_path"])
        previous = previous_by_line_id.get(job["line_id"], {})
        if valid_existing(previous, job, output_path):
            manifest["items"].append(previous)
            manifest["updated_at"] = utc_now()
            atomic_write_json(manifest_path, manifest)
            print(f"[{index:03d}/{EXPECTED_COUNT}] SKIP {job['file_name']}", flush=True)
            continue

        audition = audition_by_line_id.get(job["line_id"], {})
        audition_path = audition_dir / audition.get("file_name", "")
        if valid_existing(audition, job, audition_path):
            shutil.copy2(audition_path, output_path)
            audio = output_path.read_bytes()
            item = {
                **job,
                "status": "success",
                "generated_at": audition.get("generated_at"),
                "reused_at": utc_now(),
                "provenance": "casting_audition_reuse",
                "source_audio_path": str(audition_path.resolve()),
                "file_size_bytes": len(audio),
                "audio_sha256": hashlib.sha256(audio).hexdigest(),
                "mp3": parse_mp3(audio),
                "http_status": audition.get("http_status"),
                "request_id": audition.get("request_id"),
                "trace_id": audition.get("trace_id"),
                "character_cost": audition.get("character_cost"),
                "content_type": audition.get("content_type"),
                "attempt": audition.get("attempt"),
            }
            manifest["items"].append(item)
            append_event(
                event_log_path,
                {
                    "timestamp": utc_now(),
                    "event": "audition_reused",
                    "line_id": job["line_id"],
                    "file_name": job["file_name"],
                    "audio_sha256": item["audio_sha256"],
                },
            )
            manifest["updated_at"] = utc_now()
            atomic_write_json(manifest_path, manifest)
            print(f"[{index:03d}/{EXPECTED_COUNT}] REUSE {job['file_name']}", flush=True)
            continue

        print(
            f"[{index:03d}/{EXPECTED_COUNT}] GENERATE "
            f"{job['voice_alias']} :: {job['line_id']}",
            flush=True,
        )
        append_event(
            event_log_path,
            {
                "timestamp": utc_now(),
                "event": "generation_started",
                "line_id": job["line_id"],
                "voice_alias": job["voice_alias"],
                "params_sha256": job["params_sha256"],
            },
        )
        try:
            audio, response_meta = request_audio(
                api_key=api_key,
                voice_id=job["voice_id"],
                text=job["text"],
                seed=job["seed"],
                max_attempts=args.max_attempts,
            )
            mp3_info = parse_mp3(audio)
            temp_path = output_path.with_suffix(output_path.suffix + ".part")
            temp_path.write_bytes(audio)
            os.replace(temp_path, output_path)
            item = {
                **job,
                "status": "success",
                "generated_at": utc_now(),
                "provenance": "elevenlabs_api",
                "file_size_bytes": len(audio),
                "audio_sha256": hashlib.sha256(audio).hexdigest(),
                "mp3": mp3_info,
                **response_meta,
            }
            manifest["items"].append(item)
            append_event(
                event_log_path,
                {
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
                },
            )
            print(
                f"  OK {len(audio)} bytes, {mp3_info['duration_seconds']:.2f}s",
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
            append_event(
                event_log_path,
                {
                    "timestamp": utc_now(),
                    "event": "generation_failed",
                    "line_id": job["line_id"],
                    "voice_alias": job["voice_alias"],
                    "error": str(exc),
                },
            )
            manifest["updated_at"] = utc_now()
            manifest["success_count"] = sum(
                value.get("status") == "success" for value in manifest["items"]
            )
            manifest["failed_count"] = 1
            atomic_write_json(manifest_path, manifest)
            print(f"  FAILED {exc}", file=sys.stderr, flush=True)
            return 1

        manifest["updated_at"] = utc_now()
        atomic_write_json(manifest_path, manifest)

    success_count = sum(
        item.get("status") == "success" for item in manifest["items"]
    )
    reused_count = sum(
        item.get("provenance") == "casting_audition_reuse"
        for item in manifest["items"]
    )
    generated_count = sum(
        item.get("provenance") == "elevenlabs_api" for item in manifest["items"]
    )
    manifest.update(
        {
            "completed_at": utc_now(),
            "success_count": success_count,
            "failed_count": EXPECTED_COUNT - success_count,
            "reused_count": reused_count,
            "generated_count": generated_count,
        }
    )
    atomic_write_json(manifest_path, manifest)
    print(
        f"COMPLETE success={success_count}/{EXPECTED_COUNT} "
        f"reused={reused_count} generated={generated_count}",
        flush=True,
    )
    return 0 if success_count == EXPECTED_COUNT else 1


if __name__ == "__main__":
    raise SystemExit(main())
