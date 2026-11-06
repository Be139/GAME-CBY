#!/usr/bin/env bash
# Usage: bake_run.sh <resolution> <indirect samples>. Starts the bake, waits for it, restores the scene state.
cd /d/UGit/GAME-CBY
U=./Tools/Dev/unity-mcp.sh
SP="D:/UGit/GAME-CBY/Docs/VisualPipeline/prototype/_work"
sed -e "s/LM_RES/${1}f/; s/LM_SAMPLES/${2}/" "$SP/bake_start.cs" > "$SP/bake_start_run.cs"
$U -t 120 -f json code execute -f "$SP/bake_start_run.cs" 2>&1 | grep -E '"result": "|"error"' | cut -c1-200
START=$(date +%s)
echo 'return (UnityEditor.Lightmapping.isRunning ? "RUN " : "IDLE ") + UnityEditor.Lightmapping.buildProgress.ToString("0.00");' > "$SP/bake_poll.cs"
LAST=""
while true; do
  sleep 15
  R=$($U -t 40 -f json code execute -f "$SP/bake_poll.cs" 2>&1 | grep -E '"result": "' | cut -d'"' -f4)
  E=$(( $(date +%s) - START ))
  case "$R" in
    IDLE*) echo "bake ended after ${E}s"; break;;
    RUN*) P="${R:0:7}"; [ "$P" != "$LAST" ] && echo "t=${E}s $R"; LAST="$P";;
    *) echo "t=${E}s no answer";;
  esac
  [ "$E" -gt 1500 ] && { echo "TIMEOUT"; break; }
done
sleep 3
$U -t 280 -f json code execute -f "$SP/bake_finish.cs" 2>&1 | tail -12 | cut -c1-420
ls -la Assets/Scenes/SampleScene 2>/dev/null | cut -c25-
