#!/usr/bin/env bash
# Use the already provisioned, project-pinned CoplayDev CLI environment.
set -e
export PYTHONUTF8=1
export PYTHONIOENCODING=utf-8
exec '/c/Users/彩笔/AppData/Local/Microsoft/WinGet/Links/uvx.exe' \
  --offline --python D:/Tools/Python/python.exe \
  --from mcpforunityserver==10.3.0 unity-mcp "$@"
