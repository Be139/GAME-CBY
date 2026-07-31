#!/usr/bin/env python3
"""Build and maintain the unique HEARTH dialogue audio collection.

The initial assembly selects exactly one MP3 for every line in the authoritative
JSONL.  Later, ``replace`` can atomically replace a single line while retaining
an audit record and never keeping the superseded audio in the collection.
"""

from __future__ import annotations

import argparse
import datetime as dt
import hashlib
import json
import os
import shutil
import subprocess
import sys
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
DEFAULT_JSONL = Path(
    r"C:\Users\彩笔\Downloads\HEARTH_ElevenLabs_v3_API_Voice_Lines_Manual_Revision.jsonl"
)
DEFAULT_FULL = ROOT / "GeneratedAudio/ElevenLabs/FullDialogue_2026-07-30"
DEFAULT_REVIEW = ROOT / "GeneratedAudio/BenAndUnit_Balanced_Review_2026-07-31"
DEFAULT_OUTPUT = ROOT / "GeneratedAudio/HEARTH_FinalVoiceCollection_2026-07-31"
DEFAULT_FFPROBE = Path(
    r"E:\VoxCPM\ffmpeg\ffmpeg-8.1.2-full_build-shared\bin\ffprobe.exe"
)
WIFE_EXIT = "17F02_WifeExit_Ben_001"


def utc_now() -> str:
    return dt.datetime.now(dt.timezone.utc).isoformat()


def sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def read_json(path: Path) -> dict:
    return json.loads(path.read_text(encoding="utf-8"))


def read_jsonl(path: Path) -> list[dict]:
    return [
        json.loads(line)
        for line in path.read_text(encoding="utf-8-sig").splitlines()
        if line.strip()
    ]


def is_unit(speaker: str) -> bool:
    return speaker in {"Field Unit", "Public Unit", "Work Unit", "Care Unit"} or (
        "Home Unit" in speaker or "Synth Voice" in speaker
    )


def resolve_recorded_path(value: str, fallback_dir: Path, file_name: str | None = None) -> Path:
    recorded = Path(value) if value else Path()
    if recorded.is_file():
        return recorded.resolve()
    if file_name:
        fallback = fallback_dir / file_name
        if fallback.is_file():
            return fallback.resolve()
    raise FileNotFoundError(f"Source audio is missing: {value or file_name}")


def probe_mp3(path: Path, ffprobe: Path) -> dict:
    if not ffprobe.is_file():
        raise FileNotFoundError(f"FFprobe is missing: {ffprobe}")
    command = [
        str(ffprobe),
        "-v",
        "error",
        "-select_streams",
        "a:0",
        "-show_entries",
        "stream=codec_name,sample_rate,channels,bit_rate,duration",
        "-of",
        "json",
        str(path),
    ]
    result = subprocess.run(command, check=True, capture_output=True, text=True, encoding="utf-8")
    streams = json.loads(result.stdout).get("streams", [])
    if len(streams) != 1 or streams[0].get("codec_name") != "mp3":
        raise ValueError(f"Not a single-stream MP3: {path}")
    stream = streams[0]
    duration = float(stream.get("duration") or 0)
    if duration <= 0:
        raise ValueError(f"Invalid MP3 duration: {path}")
    return {
        "codec": "mp3",
        "sample_rate_hz": int(stream.get("sample_rate") or 0),
        "channels": int(stream.get("channels") or 0),
        "bit_rate_bps": int(stream.get("bit_rate") or 0),
        "duration_seconds": round(duration, 6),
    }


def source_maps(full_dir: Path, review_dir: Path) -> tuple[dict, dict]:
    full_manifest = read_json(full_dir / "full_dialogue_manifest.json")
    review_manifest = read_json(review_dir / "batch_manifest.json")
    full = {item["line_id"]: item for item in full_manifest["items"]}
    review = {item["line_id"]: item for item in review_manifest["items"]}
    if len(full) != len(full_manifest["items"]):
        raise ValueError("FullDialogue contains duplicate line_id values")
    if len(review) != len(review_manifest["items"]):
        raise ValueError("BenAndUnit review contains duplicate line_id values")
    return full, review


def select_source(row: dict, full: dict, review: dict, full_dir: Path) -> tuple[Path, str, dict]:
    line_id = row["line_id"]
    speaker = row["speaker"]
    if line_id == WIFE_EXIT:
        item = full[line_id]
        path = resolve_recorded_path(item.get("file_path", ""), full_dir, item.get("file_name"))
        return path, "ElevenLabs original (Ben WifeExit override)", item
    if speaker == "Ben" or is_unit(speaker):
        item = review[line_id]
        path = resolve_recorded_path(item.get("path", ""), Path())
        label = "VoxCPM Ben (Zane reference)" if speaker == "Ben" else "Unit Balanced human-machine"
        return path, label, item
    item = full[line_id]
    path = resolve_recorded_path(item.get("file_path", ""), full_dir, item.get("file_name"))
    return path, "ElevenLabs FullDialogue", item


def assemble(args: argparse.Namespace) -> None:
    jsonl = args.jsonl.resolve()
    full_dir = args.full_dir.resolve()
    review_dir = args.review_dir.resolve()
    output = args.output.resolve()
    building = output.with_name(output.name + "__building")
    if output.exists() or building.exists():
        raise FileExistsError(f"Output or staging directory already exists: {output}")

    rows = read_jsonl(jsonl)
    line_ids = [row["line_id"] for row in rows]
    if len(rows) != 338 or len(set(line_ids)) != 338:
        raise ValueError(f"Expected 338 unique JSONL lines, got {len(rows)} / {len(set(line_ids))}")
    full, review = source_maps(full_dir, review_dir)

    entries: list[dict] = []
    building.mkdir(parents=True)
    try:
        for index, row in enumerate(rows, 1):
            source, source_group, source_item = select_source(row, full, review, full_dir)
            destination = building / f"{row['line_id']}.mp3"
            expected_hash = source_item.get("audio_sha256") or source_item.get("sha256")
            actual_source_hash = sha256(source)
            if expected_hash and expected_hash != actual_source_hash:
                raise ValueError(f"Source hash mismatch: {source}")
            shutil.copy2(source, destination)
            output_hash = sha256(destination)
            if output_hash != actual_source_hash:
                raise ValueError(f"Copy hash mismatch: {destination}")
            stream = probe_mp3(destination, args.ffprobe)
            entries.append(
                {
                    "source_index": index,
                    "line_id": row["line_id"],
                    "speaker": row["speaker"],
                    "text": row["text"],
                    "source_group": source_group,
                    "source_path_at_assembly": str(source),
                    "source_sha256": actual_source_hash,
                    "file_name": destination.name,
                    "sha256": output_hash,
                    "stream": stream,
                }
            )

        groups: dict[str, int] = {}
        for entry in entries:
            groups[entry["source_group"]] = groups.get(entry["source_group"], 0) + 1
        manifest = {
            "collection": output.name,
            "status": "success",
            "assembled_at": utc_now(),
            "authoritative_jsonl": str(jsonl),
            "expected_count": 338,
            "success_count": len(entries),
            "naming": "<line_id>.mp3",
            "source_counts": groups,
            "special_override": {
                "line_id": WIFE_EXIT,
                "selection": "ElevenLabs original Zane recording",
                "excluded": "VoxCPM Zane-reference version from BenAndUnit Balanced Review",
            },
            "items": entries,
        }
        (building / "collection_manifest.json").write_text(
            json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8"
        )
        mp3_count = sum(1 for path in building.iterdir() if path.suffix.lower() == ".mp3")
        if mp3_count != 338 or len(entries) != 338:
            raise ValueError(f"Final count mismatch: {mp3_count} MP3 / {len(entries)} manifest items")
        building.rename(output)
    except Exception:
        if building.exists():
            shutil.rmtree(building)
        raise
    print(json.dumps({"status": "success", "output": str(output), "count": 338, "groups": groups}, ensure_ascii=False))


def replace(args: argparse.Namespace) -> None:
    collection = args.collection.resolve()
    source = args.source.resolve()
    manifest_path = collection / "collection_manifest.json"
    manifest = read_json(manifest_path)
    items = {item["line_id"]: item for item in manifest["items"]}
    if args.line_id not in items:
        raise KeyError(f"Unknown line_id: {args.line_id}")
    probe = probe_mp3(source, args.ffprobe)
    source_hash = sha256(source)
    destination = collection / f"{args.line_id}.mp3"
    old_hash = sha256(destination)
    temporary = collection / f".{args.line_id}.replacement.part"
    shutil.copy2(source, temporary)
    if sha256(temporary) != source_hash:
        temporary.unlink(missing_ok=True)
        raise ValueError("Replacement copy hash mismatch")
    os.replace(temporary, destination)

    item = items[args.line_id]
    item.update(
        {
            "source_group": args.source_label,
            "source_path_at_assembly": str(source),
            "source_sha256": source_hash,
            "sha256": source_hash,
            "stream": probe,
            "replaced_at": utc_now(),
        }
    )
    history_path = collection / "replacement_history.jsonl"
    with history_path.open("a", encoding="utf-8", newline="\n") as stream:
        stream.write(
            json.dumps(
                {
                    "replaced_at": item["replaced_at"],
                    "line_id": args.line_id,
                    "old_sha256": old_hash,
                    "new_sha256": source_hash,
                    "source": str(source),
                    "source_label": args.source_label,
                },
                ensure_ascii=False,
            )
            + "\n"
        )
    manifest["updated_at"] = utc_now()
    manifest_path.write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    if args.move_source and source != destination:
        source.unlink()
    print(json.dumps({"status": "success", "line_id": args.line_id, "sha256": source_hash}, ensure_ascii=False))


def parser() -> argparse.ArgumentParser:
    result = argparse.ArgumentParser(description=__doc__)
    subparsers = result.add_subparsers(dest="command", required=True)
    build = subparsers.add_parser("assemble", help="Build the initial 338-file unique collection")
    build.add_argument("--jsonl", type=Path, default=DEFAULT_JSONL)
    build.add_argument("--full-dir", type=Path, default=DEFAULT_FULL)
    build.add_argument("--review-dir", type=Path, default=DEFAULT_REVIEW)
    build.add_argument("--output", type=Path, default=DEFAULT_OUTPUT)
    build.add_argument("--ffprobe", type=Path, default=DEFAULT_FFPROBE)
    build.set_defaults(run=assemble)

    update = subparsers.add_parser("replace", help="Atomically replace one line in an existing collection")
    update.add_argument("--collection", type=Path, default=DEFAULT_OUTPUT)
    update.add_argument("--line-id", required=True)
    update.add_argument("--source", type=Path, required=True)
    update.add_argument("--source-label", required=True)
    update.add_argument("--move-source", action="store_true")
    update.add_argument("--ffprobe", type=Path, default=DEFAULT_FFPROBE)
    update.set_defaults(run=replace)
    return result


def main() -> int:
    args = parser().parse_args()
    args.run(args)
    return 0


if __name__ == "__main__":
    sys.exit(main())
