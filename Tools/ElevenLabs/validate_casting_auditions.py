#!/usr/bin/env python3
"""Validate the ElevenLabs casting-audition batch without making API calls."""

from __future__ import annotations

import argparse
import collections
import hashlib
import importlib.util
import json
from pathlib import Path


EXPECTED_VOICES = {
    "Amanda",
    "Bex",
    "Blondie",
    "Brian",
    "Clarice",
    "Gregory",
    "Holly",
    "Jane",
    "Jodi",
    "John Shaw",
    "Julian",
    "Katherine",
    "Robert",
    "Sky",
    "Tarquin",
    "Zane",
}


def load_generator(generator_path: Path):
    spec = importlib.util.spec_from_file_location("audition_generator", generator_path)
    if spec is None or spec.loader is None:
        raise RuntimeError(f"Cannot load generator: {generator_path}")
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
        / "CastingAuditions_2026-07-30"
    )
    manifest_path = output_dir / "casting_auditions_manifest.json"
    generator = load_generator(
        repo_root / "Tools" / "ElevenLabs" / "generate_casting_auditions.py"
    )
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    items = manifest["items"]
    source_rows = generator.load_jsonl(Path(manifest["source_manifest"]))
    voice_map = json.loads(
        Path(manifest["voice_map"]).read_text(encoding="utf-8")
    )
    expected_jobs = generator.build_jobs(
        rows=source_rows,
        voice_map=voice_map,
        output_dir=output_dir,
    )
    expected_by_line_id = {job["line_id"]: job for job in expected_jobs}

    errors: list[str] = []
    voice_counts: collections.Counter[str] = collections.Counter()
    line_ids: set[str] = set()
    filenames: set[str] = set()
    total_bytes = 0
    total_duration = 0.0
    character_costs: list[int] = []
    request_ids = 0

    for item in items:
        line_id = item["line_id"]
        filename = item["file_name"]
        voice_alias = item["voice_alias"]
        voice_counts[voice_alias] += 1
        line_ids.add(line_id)
        filenames.add(filename)

        expected = expected_by_line_id.get(line_id)
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

        audio_path = output_dir / filename
        if not audio_path.is_file() or audio_path.stat().st_size <= 0:
            errors.append(f"missing:{filename}")
            continue

        audio = audio_path.read_bytes()
        if hashlib.sha256(audio).hexdigest() != item["audio_sha256"]:
            errors.append(f"sha256:{filename}")

        info = generator.parse_mp3(audio)
        if info["sample_rate_hz"] != 44100:
            errors.append(f"sample-rate:{filename}")
        if info["bitrate_kbps"] != 128:
            errors.append(f"bitrate:{filename}")
        if info["duration_seconds"] <= 0.1:
            errors.append(f"duration:{filename}")

        total_bytes += len(audio)
        total_duration += info["duration_seconds"]
        if item.get("character_cost") is not None:
            character_costs.append(int(item["character_cost"]))
        if item.get("request_id"):
            request_ids += 1

    if len(items) != 32:
        errors.append(f"item-count:{len(items)}")
    if len(line_ids) != 32:
        errors.append(f"line-id-count:{len(line_ids)}")
    if len(filenames) != 32:
        errors.append(f"filename-count:{len(filenames)}")
    if set(voice_counts) != EXPECTED_VOICES:
        errors.append("voice-set")
    if any(count != 2 for count in voice_counts.values()):
        errors.append("voice-count")
    if manifest.get("success_count") != 32:
        errors.append(f"manifest-success:{manifest.get('success_count')}")
    if manifest.get("failed_count") != 0:
        errors.append(f"manifest-failed:{manifest.get('failed_count')}")

    result = {
        "manifest_success_count": manifest.get("success_count"),
        "manifest_failed_count": manifest.get("failed_count"),
        "validated_files": sum(
            1 for item in items if (output_dir / item["file_name"]).is_file()
        ),
        "unique_line_ids": len(line_ids),
        "unique_filenames": len(filenames),
        "source_jobs_matched": len(expected_by_line_id),
        "voice_counts": dict(sorted(voice_counts.items())),
        "request_ids_recorded": request_ids,
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
