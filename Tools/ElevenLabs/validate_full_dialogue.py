#!/usr/bin/env python3
"""Validate the complete HEARTH ElevenLabs batch without API calls."""

from __future__ import annotations

import argparse
import collections
import hashlib
import importlib.util
import json
import sys
from pathlib import Path


def load_generator(path: Path):
    sys.path.insert(0, str(path.parent))
    spec = importlib.util.spec_from_file_location("full_dialogue_generator", path)
    if spec is None or spec.loader is None:
        raise RuntimeError(f"Cannot load generator: {path}")
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo-root", type=Path, required=True)
    args = parser.parse_args()

    repo_root = args.repo_root.resolve()
    output_dir = (
        repo_root
        / "GeneratedAudio"
        / "ElevenLabs"
        / "FullDialogue_2026-07-30"
    )
    manifest_path = output_dir / "full_dialogue_manifest.json"
    generator = load_generator(
        repo_root / "Tools" / "ElevenLabs" / "generate_full_dialogue.py"
    )
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    source_rows = generator.load_jsonl(Path(manifest["source_manifest"]))
    voice_map = json.loads(Path(manifest["voice_map"]).read_text(encoding="utf-8"))
    expected_jobs = generator.build_jobs(
        rows=source_rows,
        voice_map=voice_map,
        output_dir=output_dir,
    )
    expected_by_line = {job["line_id"]: job for job in expected_jobs}
    items = manifest.get("items", [])

    errors: list[str] = []
    line_ids: set[str] = set()
    file_names: set[str] = set()
    speaker_counts: collections.Counter[str] = collections.Counter()
    voice_counts: collections.Counter[str] = collections.Counter()
    total_bytes = 0
    total_duration = 0.0
    character_costs: list[int] = []

    for item in items:
        line_id = item.get("line_id")
        file_name = item.get("file_name")
        if line_id in line_ids:
            errors.append(f"duplicate-line:{line_id}")
        if file_name in file_names:
            errors.append(f"duplicate-file:{file_name}")
        line_ids.add(line_id)
        file_names.add(file_name)
        speaker_counts[item.get("speaker")] += 1
        voice_counts[item.get("voice_alias")] += 1

        expected = expected_by_line.get(line_id)
        if expected is None:
            errors.append(f"unexpected-line:{line_id}")
        else:
            for field in (
                "speaker",
                "text",
                "voice_alias",
                "voice_id",
                "voice_name",
                "model_id",
                "output_format",
                "voice_settings",
                "seed",
                "params_sha256",
                "file_name",
            ):
                if item.get(field) != expected.get(field):
                    errors.append(f"source-mismatch:{field}:{line_id}")

        if item.get("status") != "success":
            errors.append(f"not-success:{line_id}")
        audio_path = output_dir / str(file_name)
        if not audio_path.is_file() or audio_path.stat().st_size <= 0:
            errors.append(f"missing:{file_name}")
            continue
        audio = audio_path.read_bytes()
        if len(audio) != item.get("file_size_bytes"):
            errors.append(f"size:{file_name}")
        if hashlib.sha256(audio).hexdigest() != item.get("audio_sha256"):
            errors.append(f"sha256:{file_name}")
        try:
            info = generator.parse_mp3(audio)
        except Exception as exc:
            errors.append(f"mp3:{file_name}:{exc}")
            continue
        if info["sample_rate_hz"] != 44100:
            errors.append(f"sample-rate:{file_name}")
        if info["bitrate_kbps"] != 128:
            errors.append(f"bitrate:{file_name}")
        if info["duration_seconds"] <= 0.1:
            errors.append(f"duration:{file_name}")
        total_bytes += len(audio)
        total_duration += info["duration_seconds"]
        if item.get("character_cost") is not None:
            character_costs.append(int(item["character_cost"]))

    expected_count = generator.EXPECTED_COUNT
    source_speaker_counts = collections.Counter(row["speaker"] for row in source_rows)
    if len(items) != expected_count:
        errors.append(f"item-count:{len(items)}")
    if len(line_ids) != expected_count:
        errors.append(f"line-id-count:{len(line_ids)}")
    if len(file_names) != expected_count:
        errors.append(f"file-count:{len(file_names)}")
    if line_ids != set(expected_by_line):
        errors.append("source-coverage")
    if speaker_counts != source_speaker_counts:
        errors.append("speaker-counts")
    if manifest.get("success_count") != expected_count:
        errors.append(f"manifest-success:{manifest.get('success_count')}")
    if manifest.get("failed_count") != 0:
        errors.append(f"manifest-failed:{manifest.get('failed_count')}")

    result = {
        "expected_count": expected_count,
        "validated_files": sum(
            (output_dir / item.get("file_name", "")).is_file() for item in items
        ),
        "unique_line_ids": len(line_ids),
        "unique_file_names": len(file_names),
        "manifest_success_count": manifest.get("success_count"),
        "manifest_failed_count": manifest.get("failed_count"),
        "reused_count": manifest.get("reused_count"),
        "generated_count": manifest.get("generated_count"),
        "speaker_counts": dict(sorted(speaker_counts.items())),
        "voice_counts": dict(sorted(voice_counts.items())),
        "character_cost_sum": sum(character_costs) if character_costs else None,
        "character_cost_headers": len(character_costs),
        "total_bytes": total_bytes,
        "total_duration_seconds": round(total_duration, 3),
        "errors": errors,
    }
    print(json.dumps(result, ensure_ascii=False, indent=2))
    return 1 if errors else 0


if __name__ == "__main__":
    raise SystemExit(main())
