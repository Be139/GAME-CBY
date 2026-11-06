#!/usr/bin/env bash
# Usage: vp_views.sh <sheet name> <columns> "<view list>". Renders the views and tiles them into one image.
cd /d/UGit/GAME-CBY
SP="D:/UGit/GAME-CBY/Docs/VisualPipeline/tools"
mkdir -p "$SP/_work/views" "$SP/_work/plans"
/d/Tools/Python/python.exe -I - "$SP/vp_views.cs" "$SP/_work/vp_views_run.cs" "$3" <<'PYEOF'
import sys
s=open(sys.argv[1],encoding='utf-8').read().replace('VIEW_LIST',sys.argv[3])
open(sys.argv[2],'w',encoding='utf-8').write(s)
PYEOF
./Tools/Dev/unity-mcp.sh -t 120 -f json code execute -f "$SP/_work/vp_views_run.cs" 2>&1 | grep -E '"result": "|"error"' | cut -c1-200
/d/Tools/Python/python.exe -I - "$SP/_work/views" "$1" "$2" "$3" <<'PYEOF'
import sys
from PIL import Image,ImageDraw
d,sheet,cols,lst=sys.argv[1],sys.argv[2],int(sys.argv[3]),sys.argv[4]
names=[e.split('|')[0] for e in lst.split(';') if e]
W,H=640,360; rows=(len(names)+cols-1)//cols
S=Image.new('RGB',(W*cols,H*rows))
for i,n in enumerate(names):
    im=Image.open(f'{d}/{n}.jpg').convert('RGB').resize((W,H)); dr=ImageDraw.Draw(im); dr.rectangle((0,0,130,14),fill=(0,0,0)); dr.text((3,1),n,fill=(255,255,0))
    S.paste(im,((i%cols)*W,(i//cols)*H))
S.save(f'{d}/{sheet}.jpg',quality=84); print('sheet',sheet)
PYEOF
