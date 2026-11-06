#!/usr/bin/env bash
# Usage: gen_ref.sh <id> <prompt file> <comma separated reference images>. One design image from Meshy, kept for a later 3D step.
ID="$1"; PF="$2"; REFS="$3"
WS="E:/GAME-CBY_VisualBackup/generated"; D="$WS/refs/$ID"
M="npm.cmd exec --yes --package=meshy-cli@0.4.0 -- meshy"; PY=/d/Tools/Python/python.exe
mkdir -p "$D"; [ -s "$D/ref/image_0.png" ] && { echo "$ID exists"; exit 0; }
cd "D:/UGit/GAME-CBY/Docs/VisualPipeline/tools" || exit 1
P="$(cat "$PF")"
$M image-to-image create --ai-model gpt-image-2 --prompt "$P" --reference-image-urls "$REFS" --aspect-ratio 1:1 --async --output-schema v1 --format json --no-update-check > "$D/i.out" 2> "$D/i.err"
IMG=$($PY -I -c "
import json,sys
s=open(sys.argv[1],encoding='utf-8',errors='replace').read()
j=json.loads(s[s.find('{'):]); print(j['result']['submission']['task_id'] if j.get('ok') else 'ERR')
" "$D/i.out" 2>/dev/null)
case "$IMG" in ERR*|"") echo "$ID IMAGE FAILED"; tail -3 "$D/i.err" | cut -c1-200; exit 1;; esac
$M image-to-image wait "$IMG" --timeout 300 --output-schema v1 --format json --no-update-check > "$D/iw.out" 2> "$D/iw.err"
$M download --resource image-to-image --task-id "$IMG" --kind image --output-dir "$D/ref" --workspace "$WS" --output-schema v1 --format json --no-update-check > "$D/id.out" 2>&1
echo "$ID done"
