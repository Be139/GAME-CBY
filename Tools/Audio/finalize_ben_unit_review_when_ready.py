#!/usr/bin/env python3
"""Wait for Ben generation, then validate the combined MP3 review batch."""

from __future__ import annotations

import argparse
import json
import subprocess
import sys
import time
from pathlib import Path


def process_is_running(pid: int) -> bool:
    result = subprocess.run(
        ["tasklist.exe", "/FI", f"PID eq {pid}", "/FO", "CSV", "/NH"],
        check=False,
        capture_output=True,
        text=True,
        encoding="utf-8",
        errors="replace",
    )
    return f'"{pid}"' in result.stdout


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo-root", type=Path, required=True)
    parser.add_argument("--output-root", type=Path, required=True)
    parser.add_argument("--ben-pid", type=int, required=True)
    args = parser.parse_args()

    repo_root = args.repo_root.resolve()
    output_root = args.output_root.resolve()
    manifest_path = output_root / "ben_full_manifest.json"
    pid_path = output_root / "Logs" / "ben_full_cuda.pid"

    print(f"Waiting for Ben generation PID {args.ben_pid}", flush=True)
    while process_is_running(args.ben_pid):
        time.sleep(30)

    if not manifest_path.is_file():
        raise RuntimeError("Ben process ended without creating its manifest")
    manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    if manifest.get("success_count") != 26:
        raise RuntimeError(
            "Ben process ended before completing 26 files: "
            f"{manifest.get('success_count', 0)}/26"
        )

    validator = repo_root / "Tools" / "Audio" / "validate_ben_unit_mp3_review.py"
    result = subprocess.run(
        [
            sys.executable,
            str(validator),
            "--repo-root",
            str(repo_root),
            "--output-root",
            str(output_root),
        ],
        check=False,
    )
    if result.returncode != 0:
        raise RuntimeError(f"Combined validator failed with code {result.returncode}")
    if pid_path.exists():
        pid_path.unlink()
    print("FINALIZED 173/173 MP3 review files", flush=True)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
