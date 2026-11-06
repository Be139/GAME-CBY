#!/usr/bin/env bash
# Usage: play_run.sh <tag>. Enter Play, keep it running while Unity is unfocused, start the real 17F01 replay, render views, leave Play.
cd /d/UGit/GAME-CBY
U=./Tools/Dev/unity-mcp.sh
SP="D:/UGit/GAME-CBY/Docs/VisualPipeline/prototype/_work"
T="$1"
$U -t 30 editor play 2>&1 | tail -1 | cut -c1-100
powershell.exe -NoProfile -Command "\$p = Get-Process Unity | Select-Object -First 1; (New-Object -ComObject WScript.Shell).AppActivate(\$p.Id) | Out-Null"
for i in 1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18; do
  timeout 5 tail -f /dev/null 2>/dev/null
  R=$($U -t 10 -f json code execute -f "$SP/run_bg.cs" 2>&1 | grep -E '"result": "' | cut -d'"' -f4)
  case "$R" in *playing=True*) echo "in play: $R"; break;; *) echo "wait $i";; esac
  [ $((i % 4)) -eq 0 ] && powershell.exe -NoProfile -Command "\$p = Get-Process Unity | Select-Object -First 1; (New-Object -ComObject WScript.Shell).AppActivate(\$p.Id) | Out-Null"
done
timeout 6 tail -f /dev/null 2>/dev/null
$U -t 20 -f json code execute -f "$SP/play_begin.cs" 2>&1 | grep -E '"result": "|"error"' | cut -c1-160
timeout 11 tail -f /dev/null 2>/dev/null
sed "s/TAG_PLACEHOLDER/${T}_cam/" "$SP/play_capture.cs" > "$SP/play_capture_run.cs"
$U -t 30 -f json code execute -f "$SP/play_capture_run.cs" > "$SP/${T}_cap.out" 2>&1
PYTHONIOENCODING=utf-8 /d/Tools/Python/python.exe -I "$SP/show.py" "$SP/${T}_cap.out" | head -3 | cut -c1-150
sed "s/TAG_PLACEHOLDER/${T}/" "$SP/play_views.cs" > "$SP/play_views_run.cs"
$U -t 60 -f json code execute -f "$SP/play_views_run.cs" > "$SP/${T}_views.out" 2>&1
PYTHONIOENCODING=utf-8 /d/Tools/Python/python.exe -I "$SP/show.py" "$SP/${T}_views.out" | cut -c1-170
$U -t 15 editor console --type error --count 40 2>&1 | grep -v "M_Cat_L_Red_D" | tail -5 | cut -c1-140
$U -t 30 editor stop 2>&1 | tail -1 | cut -c1-100
for i in 1 2 3 4 5 6 7 8; do timeout 5 tail -f /dev/null 2>/dev/null; R=$($U -t 10 -f json code execute -f "$SP/is_playing.cs" 2>&1 | grep -E '"result": "' | cut -d'"' -f4); case "$R" in playing=False*) echo "back in edit mode"; break;; esac; done
/d/Tools/Python/python.exe -I - "$SP/noah_views" "$T" <<'PYEOF'
import sys,os
from PIL import Image,ImageDraw
d,t=sys.argv[1],sys.argv[2]
names=['game_start','to_window','to_head','to_south','to_desk','door_side']
W,H=640,360; S=Image.new('RGB',(W*3,H*2))
for i,n in enumerate(names):
    p=os.path.join(d,f'{t}_{n}.jpg')
    if not os.path.exists(p): continue
    im=Image.open(p).convert('RGB').resize((W,H)); dr=ImageDraw.Draw(im); dr.rectangle((0,0,120,16),fill=(0,0,0)); dr.text((4,2),n,fill=(255,255,0)); S.paste(im,((i%3)*W,(i//3)*H))
S.save(os.path.join(d,f'{t}_sheet.jpg'),quality=88); print('sheet ok')
PYEOF
