#!/usr/bin/env bash
# Tower design images. The neon street picture the user chose sets the city colours; the room references keep the same simplified forms.
SP="D:/UGit/GAME-CBY/Docs/VisualPipeline/tools"
mkdir -p "$SP/_work/views" "$SP/_work/plans"; cd "$SP"
R="D:/UGit/GAME-CBY/Docs/VisualPipeline/references"
REF="$SP/views/ref5.jpg,$R/style/clay_room.jpg,$R/style/virginia_office.jpg"
HEAD="Create one single building for a stylized 3D game city in the exact visual language of the reference images: simplified low-poly illustrated forms, large clean planes, flat colours with soft gradients, blue-violet night palette with neon accents. Not photorealistic, not gritty, no fine detail. Building:"
TAIL="Complete building fully visible from street level to roof, three-quarter view from slightly above, plain dark blue-grey background, no ground plane, no cast shadow, no text, no letters, no logo, no people."
active=0
while IFS='|' read -r id desc; do
  [ -z "$id" ] && continue
  printf '%s %s. %s\n' "$HEAD" "$desc" "$TAIL" > "prompts/$id.txt"
  bash gen_ref.sh "$id" "prompts/$id.txt" "$REF" 2>&1 | grep -E "done|FAILED" &
  active=$((active+1)); if [ "$active" -ge 5 ]; then wait; active=0; fi
done < items_towers.txt
wait
echo "towers finished"
