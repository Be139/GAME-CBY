#!/usr/bin/env bash
# Usage: view.sh <tag>. Renders all fixed views and tiles them into one sheet.
cd /d/UGit/GAME-CBY
SP="D:/UGit/GAME-CBY/Docs/VisualPipeline/prototype/_work"
sed "s/TAG_PLACEHOLDER/$1/" "$SP/view_noah.cs" > "$SP/view_noah_run.cs"
./Tools/Dev/unity-mcp.sh -t 120 -f json code execute -f "$SP/view_noah_run.cs" 2>&1 | grep -E '"result": "|"error"' | cut -c1-200
/d/Tools/Python/python.exe -I - "$SP/noah_views" "$1" <<'PYEOF'
import sys,os
from PIL import Image,ImageDraw
import numpy as np
d,tag=sys.argv[1],sys.argv[2]
names=['game_start','to_window','to_head','to_south','to_desk','top']
W,H=640,360; S=Image.new('RGB',(W*3,H*2))
for i,n in enumerate(names):
    p=os.path.join(d,f'{tag}_{n}.jpg')
    if not os.path.exists(p): continue
    im=Image.open(p).convert('RGB')
    a=np.asarray(im.resize((160,90))).reshape(-1,3)/255.; l=a@np.array([.2126,.7152,.0722])
    print(n,'mean %.2f p50 %.2f p95 %.2f'%(l.mean(),np.median(l),np.percentile(l,95)))
    im=im.resize((W,H)); dr=ImageDraw.Draw(im); dr.rectangle((0,0,120,16),fill=(0,0,0)); dr.text((4,2),n,fill=(255,255,0))
    S.paste(im,((i%3)*W,(i//3)*H))
S.save(os.path.join(d,f'{tag}_sheet.jpg'),quality=88)
PYEOF
