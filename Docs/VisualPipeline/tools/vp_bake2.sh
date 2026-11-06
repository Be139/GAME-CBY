#!/usr/bin/env bash
# Usage: vp_bake2.sh (2026-11-06 quality settings). <texels per unit> <indirect samples>. Starts the bake, waits for it, restores the scene state.
cd /d/UGit/GAME-CBY
U=./Tools/Dev/unity-mcp.sh
SP="D:/UGit/GAME-CBY/Docs/VisualPipeline/tools"
mkdir -p "$SP/_work/views" "$SP/_work/plans"
sed -e "s/LM_RES/${1}f/; s/LM_SAMPLES/${2}/" "$SP/vp_bake2_start.cs" > "$SP/_work/vp_bake_start_run.cs"
$U -t 180 -f json code execute -f "$SP/_work/vp_bake_start_run.cs" 2>&1 | grep -E '"result": "|"error"|rror' | cut -c1-220
START=$(date +%s)
echo 'return (UnityEditor.Lightmapping.isRunning ? "RUN " : "IDLE ") + UnityEditor.Lightmapping.buildProgress.ToString("0.00");' > "$SP/_work/bake_poll.cs"
LAST=""
while true; do
  sleep 30
  R=$($U -t 40 -f json code execute -f "$SP/_work/bake_poll.cs" 2>&1 | grep -E '"result": "' | cut -d'"' -f4)
  E=$(( $(date +%s) - START ))
  case "$R" in
    IDLE*) echo "bake ended after ${E}s"; break;;
    RUN*) P="${R:0:6}"; [ "$P" != "$LAST" ] && echo "t=${E}s $R"; LAST="$P";;
    *) echo "t=${E}s no answer";;
  esac
  [ "$E" -gt 9000 ] && { echo "TIMEOUT"; break; }
done
sleep 3
$U -t 280 -f json code execute -f "$SP/vp_bake_finish.cs" 2>&1 | grep -E '"result": "|rror' | cut -c1-300
