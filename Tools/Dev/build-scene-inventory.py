"""Build read-only searchable indexes from the Unity scene snapshot and C# sources."""
import collections
import csv
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'Logs/HearthUiReview/scene-program-inventory.json'
OUT = ROOT / 'Docs/SceneInventory'
OUT.mkdir(parents=True, exist_ok=True)
data = json.loads(SOURCE.read_text(encoding='utf-8'))
objects, links = data['objects'], data['links']
by_root = collections.defaultdict(list)
attached = collections.defaultdict(list)
for obj in objects:
    by_root[obj['root']].append(obj)
    for component in obj['components']:
        if component['script']:
            attached[component['script']].append(obj['path'])

def category(root, section):
    if '====' in root:
        return '分割线（非容器）'
    if root in ('HumanCanvas', 'HearthHudRoot', 'HearthCompanionHudRoot'):
        return '01 UI'
    if root in ('Lighting_Dusk_Fixed', 'CityWindowNightSystem'):
        return '05 Environment'
    for number, name in [('01', 'UI'), ('02', 'PlayerCameras'), ('03', 'GameplaySystems'), ('04', 'World'), ('05', 'Environment'), ('06', 'ActorsReferences')]:
        if f' {number} ' in section:
            return f'{number} {name}'
    return '07 Review（尚未判定废弃）'

def write_csv(name, fields, rows):
    with (OUT / name).open('w', encoding='utf-8', newline='') as stream:
        writer = csv.DictWriter(stream, fieldnames=fields)
        writer.writeheader()
        writer.writerows(rows)

object_rows = []
for obj in objects:
    object_rows.append(dict(id=obj['id'], instanceId=obj['instanceId'], parentInstanceId=obj['parentInstanceId'], path=obj['path'], root=obj['root'], visualSection=obj['section'], category=category(obj['root'], obj['section']), sibling=obj['sibling'], activeSelf=obj['activeSelf'], activeHierarchy=obj['activeHierarchy'], prefab=obj['prefab'], components='; '.join(c['type'] for c in obj['components']), scripts='; '.join(c['script'] for c in obj['components'] if c['script'])))
write_csv('objects.csv', list(object_rows[0]), object_rows)
write_csv('references.csv', ['source', 'sourceId', 'sourceInstanceId', 'script', 'field', 'target', 'targetId', 'targetInstanceId', 'targetType'], links)

source_paths = sorted(p for p in (ROOT / 'Assets').rglob('*.cs') if '__MACOSX' not in p.parts and not p.name.startswith('._'))
script_rows, lookup_rows = [], []
script_sources = {}
for path in source_paths:
    relative = path.relative_to(ROOT).as_posix()
    source = path.read_text(encoding='utf-8-sig', errors='replace')
    script_sources[relative] = source
    own = relative.startswith(('Assets/Scripts/', 'Assets/Editor/'))
    module = '/'.join(relative.split('/')[2:-1]) if own else relative.split('/')[1]
    summary = re.search(r'<summary>(.*?)</summary>', source, re.S)
    description = re.sub(r'\s+', ' ', summary[1].replace('///', '')).strip() if summary else ''
    declarations = re.findall(r'^\s*(?:public\s+)?(?:(?:sealed|abstract|static|partial)\s+)*(?:class|interface|struct|enum)\s+(\w+)', source, re.M)
    api = re.findall(r'^\s*public\s+(?:(?:static|virtual|override|sealed|async|new)\s+)*(?:[\w<>\[\],.?]+)\s+(\w+)\s*\(', source, re.M)
    script_rows.append(dict(path=relative, owner='项目目录' if own else '外部资产目录（来源按目录）', module=module or '根模块', editor='/Editor/' in relative, types='; '.join(declarations), sourceSummary=description, publicMethods='; '.join(sorted(set(api))), loadedSceneInstances=len(attached[relative]), loadedScenePaths='; '.join(attached[relative])))
    if own:
        for i, line in enumerate(source.splitlines(), 1):
            if re.search(r'\b(?:Find|FindObjectOfType|FindObjectsOfType|FindFirstObjectByType|FindAnyObjectByType|GetComponentInParent|GetComponentsInChildren|Resources\.Load)\s*[<(]', line):
                lookup_rows.append(dict(script=relative, line=i, code=line.strip()))
write_csv('scripts.csv', list(script_rows[0]), script_rows)
write_csv('dynamic-lookups.csv', ['script', 'line', 'code'], lookup_rows)

root_rows = []
for root, group in by_root.items():
    counter = collections.Counter(c['script'] for obj in group for c in obj['components'] if c['script'].startswith('Assets/Scripts/'))
    root_rows.append(dict(root=root, visualSection=group[0]['section'], category=category(root, group[0]['section']), objects=len(group), activeObjects=sum(o['activeHierarchy'] for o in group), projectScriptInstances=sum(counter.values()), projectScripts='; '.join(f'{Path(s).stem} ({n})' for s, n in counter.most_common())))
write_csv('roots.csv', list(root_rows[0]), root_rows)
own = [r for r in script_rows if r['owner']=='项目目录']
paths = collections.Counter(o['path'] for o in objects)
summary = dict(scene=data['scene'], dirty=data['dirty'], playing=data['playing'], objects=len(objects), roots=len(root_rows), serializedReferences=len(links), missingScripts=sum(c['type']=='MISSING_SCRIPT' for o in objects for c in o['components']), duplicatePaths=sum(v>1 for v in paths.values()), scripts=len(script_rows), projectScripts=len(own), loadedProjectScriptTypes=sum(bool(r['loadedSceneInstances']) for r in own), dynamicLookupLines=len(lookup_rows))
(OUT / 'summary.json').write_text(json.dumps(summary, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
lines = ['# 当前场景根物件与程序挂载', '', '生成时间口径：2026-10-29 编辑器快照。完整子物件见 objects.csv，字段级关联见 references.csv。', '', '|根物件|当前视觉区段|职责分类|含子物件数|自有脚本实例数|主要脚本（最多 8 项）|', '|---|---|---|---:|---:|---|']
for row in root_rows:
    lines.append('|'+ '|'.join(str(row[k]).replace('|','\\|') for k in ['root','visualSection','category','objects','projectScriptInstances'])+'|'+'; '.join(row['projectScripts'].split('; ')[:8])+'|')
(OUT / '根物件分类.md').write_text('\n'.join(lines)+'\n', encoding='utf-8')
print(json.dumps(summary, ensure_ascii=False, indent=2))
