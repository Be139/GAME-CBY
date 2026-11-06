#!/usr/bin/env bash
# Usage: gen_img2.sh <id> <aspect> <prompt>. Flat picture (rug, wall art) from the same style references.
ID="$1"; AR="$2"; P="$3"
WS="E:/GAME-CBY_VisualBackup/generated"; D="$WS/art/$ID"; R="D:/UGit/GAME-CBY/Docs/VisualPipeline/references/style"
M="npm.cmd exec --yes --package=meshy-cli@0.4.0 -- meshy"; PY=/d/Tools/Python/python.exe
mkdir -p "$D"; [ -s "$D/ref/image_0.png" ] && { echo "$ID exists"; exit 0; }
cd "D:/UGit/GAME-CBY/Docs/VisualPipeline/tools" || exit 1
$M image-to-image create --ai-model gpt-image-2 --prompt "$P" --reference-image-urls "$R/clay_room.jpg,$R/virginia_home.jpg,$R/laststop_study.jpg" --aspect-ratio "$AR" --async --output-schema v1 --format json --no-update-check > "$D/i.out" 2> "$D/i.err"
IMG=$($PY -I -c "
import json,sys
s=open(sys.argv[1],encoding='utf-8',errors='replace').read()
j=json.loads(s[s.find('{'):]); print(j['result']['submission']['task_id'] if j.get('ok') else 'ERR')
" "$D/i.out" 2>/dev/null)
case "$IMG" in ERR*|"") echo "$ID IMAGE FAILED"; exit 1;; esac
$M image-to-image wait "$IMG" --timeout 300 --output-schema v1 --format json --no-update-check > "$D/iw.out" 2> "$D/iw.err"
$M download --resource image-to-image --task-id "$IMG" --kind image --output-dir "$D/ref" --workspace "$WS" --output-schema v1 --format json --no-update-check > "$D/id.out" 2>&1
echo "$ID done"
