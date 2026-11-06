#!/usr/bin/env bash
# Usage: gen_item.sh <asset id> <polycount> <object description>
# Reference image from the user's style references, then a textured low-poly model from that image.
ID="$1"; POLY="$2"; DESC="$3"
WS="D:/UGit/GAME-CBY/Docs/VisualPipeline/generated"
D="$WS/noah/$ID"; R="D:/UGit/GAME-CBY/Docs/VisualPipeline/references/style"
M="npm.cmd exec --yes --package=meshy-cli@0.4.0 -- meshy"
PY=/d/Tools/Python/python.exe
mkdir -p "$D"
cd "D:/UGit/GAME-CBY/Docs/VisualPipeline/prototype/_work" || exit 1
tid() { $PY -I -c "
import json,sys
s=open(sys.argv[1],encoding='utf-8',errors='replace').read()
j=json.loads(s[s.find('{'):]); print(j['result']['submission']['task_id'] if j.get('ok') else 'ERR:'+json.dumps(j.get('error'))[:240])
" "$1" 2>/dev/null; }
PROMPT="Create one single object for a 3D game in the exact visual language of the reference images: very simple low-poly forms, large flat planes, soft small bevels, clean flat matte colours with only gentle gradients, muted warm-and-cool palette, no fabric texture, no wrinkle detail, no wood grain, no small clutter. Object: $DESC. Complete object fully visible, three-quarter view, plain neutral grey background, soft even light, no cast shadow, no text, no logo, no people."
if [ ! -s "$D/ref/image_0.png" ]; then
  $M image-to-image create --ai-model gpt-image-2 --prompt "$PROMPT" --reference-image-urls "$R/clay_room.jpg,$R/virginia_home.jpg,$R/laststop_study.jpg" --aspect-ratio 1:1 --async --output-schema v1 --format json --no-update-check > "$D/i.out" 2> "$D/i.err"
  IMG=$(tid "$D/i.out"); echo "$ID image=$IMG"
  case "$IMG" in ERR*|"") echo "$ID IMAGE FAILED"; tail -3 "$D/i.err" | cut -c1-200; exit 1;; esac
  echo "$IMG" > "$D/image_task.txt"
  $M image-to-image wait "$IMG" --timeout 300 --save-json "$D/task_ref.json" --output-schema v1 --format json --no-update-check > "$D/iw.out" 2> "$D/iw.err"
  $M download --resource image-to-image --task-id "$IMG" --kind image --output-dir "$D/ref" --workspace "$WS" --output-schema v1 --format json --no-update-check > "$D/id.out" 2>&1
else
  IMG=$(cat "$D/image_task.txt")
fi
$M image-to-3d create --input-task-id "$IMG" --model-type smart-topology --target-polycount "$POLY" --should-texture true --enable-pbr false --texture-resolution 2k --target-formats glb,fbx --async --output-schema v1 --format json --no-update-check > "$D/m.out" 2> "$D/m.err"
MID=$(tid "$D/m.out"); echo "$ID model=$MID"
case "$MID" in ERR*|"") echo "$ID MODEL FAILED"; tail -3 "$D/m.err" | cut -c1-200; tail -8 "$D/m.out" | cut -c1-200; exit 1;; esac
echo "$MID" > "$D/model_task.txt"
$M image-to-3d wait "$MID" --timeout 500 --save-json "$D/task_model.json" --output-schema v1 --format json --no-update-check > "$D/mw.out" 2> "$D/mw.err"
$M download --resource image-to-3d --task-id "$MID" --model-format glb --output "$D/$ID.glb" --workspace "$WS" --output-schema v1 --format json --no-update-check > "$D/dg.out" 2>&1
$M download --resource image-to-3d --task-id "$MID" --model-format fbx --output "$D/$ID.fbx" --workspace "$WS" --output-schema v1 --format json --no-update-check > "$D/df.out" 2>&1
$M download --resource image-to-3d --task-id "$MID" --asset thumbnail.primary --output "$D/thumb.png" --workspace "$WS" --output-schema v1 --format json --no-update-check > "$D/dt.out" 2>&1
echo "$ID done"; ls -la "$D" | grep -E "\.glb|\.fbx|thumb" | cut -c25-
