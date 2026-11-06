#!/usr/bin/env bash
# Usage: gen_refs_batch.sh <items file: id|poly|description> [art file: id|aspect|prompt]. Design images from Meshy, six at a time. Existing images are skipped.
SP="D:/UGit/GAME-CBY/Docs/VisualPipeline/tools"
mkdir -p "$SP/_work/views" "$SP/_work/plans"; cd "$SP"
R="D:/UGit/GAME-CBY/Docs/VisualPipeline/references/style"
TAIL="Complete object fully visible, three-quarter view, plain neutral grey background, soft even light, no cast shadow, no text, no logo, no people."
HEAD="Create one single object for a 3D game in the exact visual language of the reference images: very simple low-poly forms, large flat planes, soft small bevels, clean flat matte colours with only gentle gradients, muted warm-and-cool palette, no fabric texture, no wrinkle detail, no wood grain, no small clutter. Object:"
mkdir -p prompts
active=0
while IFS='|' read -r id poly desc; do
  [ -z "$id" ] && continue
  printf '%s %s. %s\n' "$HEAD" "$desc" "$TAIL" > "prompts/$id.txt"
  bash gen_ref.sh "$id" "prompts/$id.txt" "$R/clay_room.jpg,$R/virginia_home.jpg,$R/laststop_study.jpg" 2>&1 | grep -E "done|FAILED" &
  active=$((active+1)); if [ "$active" -ge 6 ]; then wait; active=0; fi
done < "$1"
wait
if [ -n "$2" ]; then
  active=0
  while IFS='|' read -r id ar p; do
    [ -z "$id" ] && continue
    bash gen_img2.sh "$id" "$ar" "$p" 2>&1 | grep -E "done|FAILED" &
    active=$((active+1)); if [ "$active" -ge 6 ]; then wait; active=0; fi
  done < "$2"
  wait
fi
echo "refs finished"
