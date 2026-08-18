#!/usr/bin/env python3
"""Restore every Ben line in the final collection to direct ElevenLabs audio."""

from __future__ import annotations

import argparse
import hashlib
import json
import os
import shutil
import sys
import urllib.parse
import urllib.request
from pathlib import Path
from typing import Any

from generate_casting_auditions import (
    MODEL_ID,
    OUTPUT_FORMAT,
    VOICE_SETTINGS,
    atomic_write_json,
    parse_mp3,
    request_audio,
    safe_name,
    stable_seed,
    utc_now,
)


EXPECTED_BEN_COUNT = 26
WIFE_EXIT_LINE_ID = "17F02_WifeExit_Ben_001"
TARGET_SOURCE_GROUP = "ElevenLabs original Ben (Zane)"
STAGING_FOLDER = ".BenElevenLabsRestore_2026-08-13"
COLLECTION_FOLDER = "HEARTH_FinalVoiceCollection_2026-07-31"


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def load_rows(path: Path) -> list[dict[str, Any]]:
    rows: list[dict[str, Any]] = []
    with path.open("r", encoding="utf-8-sig") as stream:
        for line_number, line in enumerate(stream, 1):
            if not line.strip():
                continue
            try:
                rows.append(json.loads(line))
            except json.JSONDecodeError as exc:
                raise RuntimeError(f"Invalid JSONL line {line_number}: {exc}") from exc
    return rows


def params_hash(job: dict[str, Any]) -> str:
    keys = (
        "line_id",
        "speaker",
        "text",
        "voice_alias",
        "voice_id",
        "voice_name",
        "model_id",
        "output_format",
        "voice_settings",
        "seed",
    )
    payload = {key: job[key] for key in keys}
    return hashlib.sha256(
        json.dumps(payload, sort_keys=True, ensure_ascii=False).encode("utf-8")
    ).hexdigest()


def verify_voice(api_key: str, voice_id: str) -> str:
    url = f"https://api.elevenlabs.io/v1/voices/{urllib.parse.quote(voice_id)}"
    request = urllib.request.Request(
        url,
        method="GET",
        headers={"xi-api-key": api_key, "Accept": "application/json"},
    )
    with urllib.request.urlopen(request, timeout=60) as response:
        payload = json.loads(response.read().decode("utf-8"))
    returned_id = payload.get("voice_id")
    if returned_id != voice_id:
        raise RuntimeError(f"ElevenLabs returned an unexpected voice ID: {returned_id}")
    return str(payload.get("name") or "Zane")


def valid_staged(item: dict[str, Any], job: dict[str, Any], path: Path) -> bool:
    if (
        item.get("status") != "success"
        or item.get("params_sha256") != job["params_sha256"]
        or not path.is_file()
        or path.stat().st_size != item.get("file_size_bytes")
    ):
        return False
    return sha256(path) == item.get("audio_sha256")


def write_text_atomic(path: Path, content: str) -> None:
    temporary = path.with_suffix(path.suffix + ".tmp")
    temporary.write_text(content, encoding="utf-8", newline="\n")
    os.replace(temporary, path)


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
    source_jsonl = repo_root / "HEARTH_ElevenLabs_v3_API_Voice_Lines_Manual_Revision.jsonl"
    voice_map_path = repo_root / "HEARTH_ElevenLabs_Voice_Map.json"
    collection = repo_root / "GeneratedAudio" / COLLECTION_FOLDER
    collection_manifest_path = collection / "collection_manifest.json"
    staging = repo_root / "GeneratedAudio" / STAGING_FOLDER
    staging_manifest_path = staging / "generation_manifest.json"

    rows = load_rows(source_jsonl)
    ben_rows = [row for row in rows if row.get("speaker") == "Ben"]
    if len(ben_rows) != EXPECTED_BEN_COUNT:
        raise RuntimeError(f"Expected {EXPECTED_BEN_COUNT} Ben rows, found {len(ben_rows)}")
    if len({row["line_id"] for row in ben_rows}) != EXPECTED_BEN_COUNT:
        raise RuntimeError("Ben rows contain duplicate line_id values")

    voice_map = json.loads(voice_map_path.read_text(encoding="utf-8"))
    ben_voice = voice_map["speakers"]["Ben"]
    voice_id = ben_voice["voice_id"]
    voice_name = ben_voice["voice_name"]
    if voice_id != "7DkaWvcqvBstUe3167oW":
        raise RuntimeError(f"Unexpected Ben voice ID: {voice_id}")
    if voice_map.get("model_id") != MODEL_ID or voice_map.get("output_format") != OUTPUT_FORMAT:
        raise RuntimeError("Voice map model or output format does not match the locked settings")

    collection_manifest = json.loads(collection_manifest_path.read_text(encoding="utf-8"))
    collection_items = {item["line_id"]: item for item in collection_manifest["items"]}
    for row in ben_rows:
        item = collection_items.get(row["line_id"])
        path = collection / f"{row['line_id']}.mp3"
        if item is None or not path.is_file() or sha256(path) != item.get("sha256"):
            raise RuntimeError(f"Final collection is not valid for {row['line_id']}")
        if item.get("text") != row.get("text"):
            raise RuntimeError(f"Text mismatch for {row['line_id']}")

    already_restored = all(
        collection_items[row["line_id"]].get("source_group") == TARGET_SOURCE_GROUP
        for row in ben_rows
    )
    if already_restored:
        print("Ben is already fully restored to direct ElevenLabs audio")
        return 0

    verified_voice_name = verify_voice(api_key, voice_id)
    print(f"Verified ElevenLabs voice: {verified_voice_name} ({voice_id})", flush=True)

    request_rows = [row for row in ben_rows if row["line_id"] != WIFE_EXIT_LINE_ID]
    staging.mkdir(parents=True, exist_ok=True)
    previous_items: dict[str, dict[str, Any]] = {}
    if staging_manifest_path.is_file():
        previous = json.loads(staging_manifest_path.read_text(encoding="utf-8"))
        previous_items = {item["line_id"]: item for item in previous.get("items", [])}

    jobs: list[dict[str, Any]] = []
    for row in request_rows:
        file_name = f"{safe_name('Zane')}__{safe_name(row['line_id'])}.mp3"
        job: dict[str, Any] = {
            "line_id": row["line_id"],
            "speaker": "Ben",
            "text": row["text"],
            "voice_alias": "Zane",
            "voice_id": voice_id,
            "voice_name": voice_name,
            "model_id": MODEL_ID,
            "output_format": OUTPUT_FORMAT,
            "voice_settings": VOICE_SETTINGS,
            "seed": stable_seed(row["line_id"]),
            "file_name": file_name,
            "file_path": str((staging / file_name).resolve()),
        }
        job["params_sha256"] = params_hash(job)
        jobs.append(job)

    generated_items: list[dict[str, Any]] = []
    generation_manifest: dict[str, Any] = {
        "batch": "Restore Ben to direct ElevenLabs Zane originals",
        "source_jsonl": str(source_jsonl),
        "voice_id": voice_id,
        "voice_name": voice_name,
        "model_id": MODEL_ID,
        "output_format": OUTPUT_FORMAT,
        "voice_settings": VOICE_SETTINGS,
        "expected_count": len(jobs),
        "updated_at": utc_now(),
        "items": generated_items,
    }

    for index, job in enumerate(jobs, 1):
        output_path = Path(job["file_path"])
        previous = previous_items.get(job["line_id"], {})
        if valid_staged(previous, job, output_path):
            generated_items.append(previous)
            print(f"[{index:02d}/{len(jobs)}] SKIP {job['line_id']}", flush=True)
        else:
            print(f"[{index:02d}/{len(jobs)}] GENERATE {job['line_id']}", flush=True)
            audio, response_meta = request_audio(
                api_key=api_key,
                voice_id=voice_id,
                text=job["text"],
                seed=job["seed"],
                max_attempts=args.max_attempts,
            )
            mp3 = parse_mp3(audio)
            temporary = output_path.with_suffix(".mp3.part")
            temporary.write_bytes(audio)
            os.replace(temporary, output_path)
            item = {
                **job,
                "status": "success",
                "generated_at": utc_now(),
                "provenance": "elevenlabs_api_direct",
                "file_size_bytes": len(audio),
                "audio_sha256": hashlib.sha256(audio).hexdigest(),
                "mp3": mp3,
                **response_meta,
            }
            generated_items.append(item)
            print(f"  OK {mp3['duration_seconds']:.2f}s", flush=True)

        generation_manifest["updated_at"] = utc_now()
        generation_manifest["success_count"] = len(generated_items)
        atomic_write_json(staging_manifest_path, generation_manifest)

    if len(generated_items) != len(jobs) or any(
        not valid_staged(item, next(job for job in jobs if job["line_id"] == item["line_id"]), Path(item["file_path"]))
        for item in generated_items
    ):
        raise RuntimeError("The staged ElevenLabs Ben batch did not pass validation")

    by_line_id = {item["line_id"]: item for item in generated_items}
    rollback = staging / "_rollback"
    rollback.mkdir()
    original_manifest_text = collection_manifest_path.read_text(encoding="utf-8")
    history_path = collection / "replacement_history.jsonl"
    original_history_text = history_path.read_text(encoding="utf-8") if history_path.exists() else ""
    history_records: list[dict[str, Any]] = []
    replaced_at = utc_now()

    try:
        for row in request_rows:
            line_id = row["line_id"]
            destination = collection / f"{line_id}.mp3"
            shutil.copy2(destination, rollback / destination.name)
            staged_path = Path(by_line_id[line_id]["file_path"])
            temporary = destination.with_suffix(".mp3.part")
            shutil.copy2(staged_path, temporary)
            if sha256(temporary) != by_line_id[line_id]["audio_sha256"]:
                raise RuntimeError(f"Replacement copy hash mismatch for {line_id}")
            os.replace(temporary, destination)

        for row in ben_rows:
            line_id = row["line_id"]
            destination = collection / f"{line_id}.mp3"
            old_item = collection_items[line_id]
            old_hash = old_item["sha256"]
            new_hash = sha256(destination)
            new_item = by_line_id.get(line_id)
            old_item["source_group"] = TARGET_SOURCE_GROUP
            old_item["source_path_at_assembly"] = (
                "ElevenLabs API direct; temporary staging removed after validation"
                if new_item
                else "ElevenLabs original retained from the previous FullDialogue batch"
            )
            old_item["source_sha256"] = new_hash
            old_item["sha256"] = new_hash
            old_item["restored_at"] = replaced_at
            if new_item:
                old_item["stream"] = {
                    "codec": "mp3",
                    "sample_rate_hz": new_item["mp3"]["sample_rate_hz"],
                    "channels": 1,
                    "bit_rate_bps": new_item["mp3"]["bitrate_kbps"] * 1000,
                    "duration_seconds": new_item["mp3"]["duration_seconds"],
                }
            old_item["generation"] = (
                {
                    "provenance": new_item["provenance"],
                    "voice_alias": new_item["voice_alias"],
                    "voice_id": new_item["voice_id"],
                    "voice_name": new_item["voice_name"],
                    "model_id": new_item["model_id"],
                    "output_format": new_item["output_format"],
                    "voice_settings": new_item["voice_settings"],
                    "seed": new_item["seed"],
                    "request_id": new_item.get("request_id"),
                    "trace_id": new_item.get("trace_id"),
                    "character_cost": new_item.get("character_cost"),
                    "generated_at": new_item["generated_at"],
                    "mp3": new_item["mp3"],
                }
                if new_item
                else {
                    "provenance": "elevenlabs_full_dialogue_retained",
                    "voice_alias": "Zane",
                    "voice_id": voice_id,
                    "voice_name": voice_name,
                    "model_id": MODEL_ID,
                    "output_format": OUTPUT_FORMAT,
                    "voice_settings": VOICE_SETTINGS,
                    "seed": stable_seed(line_id),
                }
            )
            if line_id != WIFE_EXIT_LINE_ID:
                history_records.append(
                    {
                        "replaced_at": replaced_at,
                        "line_id": line_id,
                        "old_sha256": old_hash,
                        "new_sha256": new_hash,
                        "old_source_group": "VoxCPM Ben (Zane reference)",
                        "new_source_group": TARGET_SOURCE_GROUP,
                        "reason": "User restored Ben to the unprocessed ElevenLabs Zane version.",
                    }
                )

        groups: dict[str, int] = {}
        for item in collection_manifest["items"]:
            group = item["source_group"]
            groups[group] = groups.get(group, 0) + 1
        collection_manifest["source_counts"] = groups
        collection_manifest["updated_at"] = replaced_at
        collection_manifest.pop("special_override", None)
        collection_manifest["ben_voice_selection"] = {
            "speaker": "Ben",
            "line_count": EXPECTED_BEN_COUNT,
            "source": TARGET_SOURCE_GROUP,
            "voice_id": voice_id,
            "voice_name": voice_name,
            "processing": "No VoxCPM voice cloning or optimization",
            "restored_at": replaced_at,
        }

        history_text = original_history_text + "".join(
            json.dumps(record, ensure_ascii=False) + "\n" for record in history_records
        )
        write_text_atomic(history_path, history_text)
        atomic_write_json(collection_manifest_path, collection_manifest)

        final_mp3 = list(collection.glob("*.mp3"))
        if len(final_mp3) != 338 or len({path.name for path in final_mp3}) != 338:
            raise RuntimeError("Final collection count changed during Ben restoration")
        for row in ben_rows:
            line_id = row["line_id"]
            if sha256(collection / f"{line_id}.mp3") != collection_items[line_id]["sha256"]:
                raise RuntimeError(f"Final hash validation failed for {line_id}")
    except Exception:
        for backup in rollback.glob("*.mp3"):
            os.replace(backup, collection / backup.name)
        write_text_atomic(collection_manifest_path, original_manifest_text)
        if original_history_text:
            write_text_atomic(history_path, original_history_text)
        elif history_path.exists():
            history_path.unlink()
        raise

    generation_summary = {
        "restored_at": replaced_at,
        "speaker": "Ben",
        "line_count": EXPECTED_BEN_COUNT,
        "regenerated_count": len(request_rows),
        "retained_original_count": 1,
        "voice_id": voice_id,
        "voice_name": voice_name,
        "model_id": MODEL_ID,
        "output_format": OUTPUT_FORMAT,
        "voice_settings": VOICE_SETTINGS,
        "source_jsonl": str(source_jsonl),
        "collection": str(collection),
        "status": "success",
    }
    (collection / "ben_elevenlabs_restore_record.json").write_text(
        json.dumps(generation_summary, ensure_ascii=False, indent=2) + "\n",
        encoding="utf-8",
    )
    shutil.rmtree(staging)
    print(
        json.dumps(
            {
                "status": "success",
                "restored": EXPECTED_BEN_COUNT,
                "regenerated": len(request_rows),
                "retained_original": 1,
                "collection": str(collection),
            },
            ensure_ascii=False,
        )
    )
    return 0


if __name__ == "__main__":
    sys.exit(main())
