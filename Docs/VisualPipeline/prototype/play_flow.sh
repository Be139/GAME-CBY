#!/usr/bin/env bash
# Regression: real replay entry, real interaction check, confirm, then watch the flow move on.
cd /d/UGit/GAME-CBY
U=./Tools/Dev/unity-mcp.sh
SP="D:/UGit/GAME-CBY/Docs/VisualPipeline/prototype/_work"
$U -t 30 editor play 2>&1 | tail -1 | cut -c1-100
powershell.exe -NoProfile -Command "\$p = Get-Process Unity | Select-Object -First 1; (New-Object -ComObject WScript.Shell).AppActivate(\$p.Id) | Out-Null"
for i in 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18; do
  timeout 5 tail -f /dev/null 2>/dev/null
  R=$($U -t 10 -f json code execute -f "$SP/run_bg.cs" 2>&1 | grep -E '"result": "' | cut -d'"' -f4)
  case "$R" in *playing=True*) echo "in play"; break;; esac
  [ $((i % 4)) -eq 0 ] && powershell.exe -NoProfile -Command "\$p = Get-Process Unity | Select-Object -First 1; (New-Object -ComObject WScript.Shell).AppActivate(\$p.Id) | Out-Null"
done
timeout 6 tail -f /dev/null 2>/dev/null
$U -t 20 -f json code execute -f "$SP/play_begin.cs" 2>&1 | grep -E '"result": "|"error"' | cut -c1-120
timeout 12 tail -f /dev/null 2>/dev/null
$U -t 20 -f json code execute -f "$SP/play_step.cs" 2>&1 | grep -E '"result": "' | cut -c1-160
$U -t 20 -f json code execute -f "$SP/play_interact.cs" > "$SP/play_interact.out" 2>&1
PYTHONIOENCODING=utf-8 /d/Tools/Python/python.exe -I "$SP/show.py" "$SP/play_interact.out" | cut -c1-230
for i in 1 2 3 4 5 6; do
  timeout 10 tail -f /dev/null 2>/dev/null
  $U -t 20 -f json code execute -f "$SP/play_step.cs" 2>&1 | grep -E '"result": "' | cut -c1-140
done
echo "== errors other than the known cat material"
$U -t 15 editor console --type error --count 60 2>&1 | grep -v "M_Cat_L_Red_D" | tail -6 | cut -c1-150
$U -t 30 editor stop 2>&1 | tail -1 | cut -c1-100
for i in 1 2 3 4 5 6 7 8; do timeout 5 tail -f /dev/null 2>/dev/null; R=$($U -t 10 -f json code execute -f "$SP/is_playing.cs" 2>&1 | grep -E '"result": "' | cut -d'"' -f4); case "$R" in playing=False*) echo "back in edit mode"; break;; esac; done
