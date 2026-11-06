#!/usr/bin/env bash
# Runs the prop pool and the flat pictures, six at a time. Finished items are skipped, so it can be re-run after a failure.
SP="D:/UGit/GAME-CBY/Docs/VisualPipeline/tools"
mkdir -p "$SP/_work/views" "$SP/_work/plans"; cd "$SP"
active=0
while IFS='|' read -r id ar p; do
  [ -z "$id" ] && continue
  bash gen_img2.sh "$id" "$ar" "$p" 2>&1 | grep -E "done|FAILED" &
  active=$((active+1)); if [ "$active" -ge 6 ]; then wait; active=0; fi
done < items_art.txt
wait; active=0
while IFS='|' read -r id poly desc; do
  [ -z "$id" ] && continue
  [ -s "E:/GAME-CBY_VisualBackup/generated/rooms/$id/$id.fbx" ] && continue
  bash gen_item2.sh "$id" "$poly" "$desc" 2>&1 | grep -E "done|FAILED" &
  active=$((active+1)); if [ "$active" -ge 6 ]; then wait; active=0; fi
done < "${1:-items_pool.txt}"
wait
echo "pool finished"
