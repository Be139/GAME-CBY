"""Generate evidence-based object roles, original paths and script relationships."""
import collections
import csv
import json
from pathlib import Path

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Docs/SceneInventory'
data=json.loads((ROOT/'Logs/HearthUiReview/scene-program-inventory.json').read_text(encoding='utf-8'))
baseline=json.loads((ROOT/'Logs/HearthUiReview/scene-before-naming.json').read_text(encoding='utf-8'))
old={o['instanceId']:o for o in baseline['objects']}
objects=data['objects']; links=data['links']
by_id={o['instanceId']:o for o in objects}
component_owner={c['instanceId']:o for o in objects for c in o['components'] if 'instanceId' in c}
incoming=collections.defaultdict(list); outgoing=collections.defaultdict(list)
for link in links:
 if link['field']=='m_GameObject':continue
 owner=by_id.get(link['targetInstanceId']) or component_owner.get(link['targetInstanceId'])
 if owner:incoming[owner['instanceId']].append(link)
 source=component_owner.get(link['sourceInstanceId'])
 if source:outgoing[source['instanceId']].append(link)
roles={
 'HumanCanvas':'旧人类视角基础画布；与正式 HearthHudRoot 分开记录',
 'HearthHudRoot':'正式人类 HUD、菜单、设置、处置与全屏页面',
 'HearthCompanionHudRoot':'Companion 任务、交互提示、投影与回放界面',
 'Player':'人类/Companion 视角 Rig 与各位置参考；不能将全部 Controller 当作同时运行的玩家',
 'Main Camera':'独立相机；是否承担运行画面需核对 Camera.enabled 与实际视角切换',
 'HEARTH_LOCATION_SYSTEM':'位置探测与生成的位置碰撞体',
 'EventSystem':'UI 输入事件分发',
 'MIN_LOOP_ROOT':'主流程、住户回放、终局、大堂开场、演员、锚点、音频与相关 UI',
 '17F':'17 层四户室内、终端、交互与建筑模型',
 '1F (1)':'实际大堂布景与开场交互区域（近 x=-2318）',
 'GameObject':'17F03/17F04 实体机器人与角色走位参考；现名 HouseholdActors，按正式控制器引用维护',
 'Directional Light':'原有方向光，受黄昏预设控制器引用',
 'Global Volume':'原有全局后处理 Volume，受黄昏预设控制器引用',
 'dikuai':'城市地块输入源，CityLotAssetPlacer.lotRoot 与名称查找依赖',
 'dikuai (1)':'第二份城市地块模型；与 dikuai 根位置相同，需核对渲染/子节点再判断重复',
 'CityLot_Secondary':'第二份城市地块模型；与 dikuai 根位置相同，需核对渲染/子节点再判断重复',
 'BUILDING':'人工布置建筑集合，广告牌布置与夜窗控制器共同引用',
 'CityBillboardPlacer':'立面广告牌生成与内容分发工具',
 'CityBillboards_BUILDING':'建筑立面广告牌实例、槽位标记与内容控制',
 'CityBuildings_ByHeight':'按高度分组的生成建筑，包含 462 个布置槽位',
 'CityAssetPlacer':'依据地块生成城市资产的配置与工具入口',
 'outdoor':'室外椅子、树木与景观道具集合',
 'OutdoorProps':'室外椅子、树木与景观道具集合',
 'unity5':'导入环境源模型 unity5；具体网格语义未逐一人工确认',
 'EnvironmentSource_Unity5':'导入环境源模型 unity5；具体网格语义未逐一人工确认',
 'DIKUAIunity':'大型导入环境模型；含大堂/位置检测按路径绑定的几何',
 '地块':'位于另一世界位置的地块模型（近 x=-1041,z=-1674）',
 'CityLot_Offset':'位于另一世界位置的地块模型（近 x=-1041,z=-1674）',
 'Lighting_Dusk_Fixed':'黄昏灯光预设、内容灯光及补光',
 'CityWindowNightSystem':'建筑窗户夜间外观控制',
 'Sitting_Idle':'当前未激活的坐姿模型与姿态预设，未发现外部根引用',
 'Laying_Sleeping':'17F01 躺卧男孩，boyPosePreset/boyAnimation 指向此实例',
 'Girl_A_Rigged':'女孩导入模型；未发现根级序列化剧情引用，不能据此判定废弃',
 'little_boy_B':'未激活男孩模型及自身姿态/动画配置，需与正式躺卧男孩区分',
 'casual_Female_G@Sitting_Idle':'17F01 母亲，回放控制器 motherPosePreset/motherAnimation 引用',
 'casual_Male_K (1)':'17F02 终端处丈夫，与餐桌丈夫为不同演出实例',
 'casual_Male_K':'17F02 餐桌丈夫，diningHusbandActor 引用',
 'police_Female_A':'原点附近警察模型，未发现根级剧情引用，保留待确认',
 'casual_Female_K':'17F02 餐桌妻子，回放与音效跟随引用',
 'casual_Male_G@Sitting':'17F01 父亲，fatherPosePreset/fatherAnimation 引用',
 'casual_Male_G':'邻近正式父亲位置的源模型，未发现根级剧情引用，保留待确认',
 'Plane':'原点附近平面，实际用途未由脚本引用确认',
 '1F':'原点附近旧大堂布景，与实际大堂 1F (1) 分开；未判定可删除',
}

def write_csv(name,rows):
 with (OUT/name).open('w',encoding='utf-8',newline='') as f:
  w=csv.DictWriter(f,fieldnames=list(rows[0]));w.writeheader();w.writerows(rows)

rows=[]
for o in objects:
 types=[c['type'].split('.')[-1] for c in o['components']]
 scripts=[Path(c['script']).stem for c in o['components'] if c['script']]
 role=roles.get(old[o['instanceId']]['root'],'层级分割标记' if '====' in o['root'] else '待确认')
 if o['parentInstanceId']==0:detail='根对象职责见所属用途'
 elif scripts:detail='脚本挂载节点：'+'; '.join(scripts)
 elif 'SkinnedMeshRenderer' in types:detail='蒙皮可视网格；保留模型/骨骼绑定名称'
 elif 'MeshRenderer' in types:detail='静态可视网格；具体物品按来源模型、材质和上级语境识别'
 elif 'RectTransform' in types:detail='UI 布局/显示节点；具体页面以路径及 UI 库为准'
 elif any('Collider' in t for t in types):detail='碰撞/触发几何；触发条件需查看组件字段及控制脚本'
 elif 'Animator' in types:detail='动画控制节点；保留 AnimationClip 相对路径契约'
 elif 'Light' in types:detail='灯光节点'
 elif 'Camera' in types:detail='相机节点'
 else:detail='变换、分组、锚点或骨骼节点；未单凭名称推断具体语义'
 rows.append(dict(instanceId=o['instanceId'],originalPath=old[o['instanceId']]['path'],currentPath=o['path'],rootPurpose=role,objectRole=detail,worldPosition=json.dumps(o.get('worldPosition')),activeHierarchy=o['activeHierarchy'],prefab=o['prefab'],mesh=o.get('mesh',''),materials='; '.join(o.get('materials',[])),scripts='; '.join(scripts),referencedBy='; '.join(f"{Path(l['script']).stem}.{l['field']} <- {l['source']}" for l in incoming[o['instanceId']]),controlsOrReferences='; '.join(f"{Path(l['script']).stem}.{l['field']} -> {l['target']}" for l in outgoing[o['instanceId']]),evidence='编辑器组件/空间/序列化引用；未逐网格视觉验收'))
write_csv('object-roles.csv',rows)
renames=[dict(instanceId=o['instanceId'],originalPath=old[o['instanceId']]['path'],currentPath=o['path']) for o in objects if old[o['instanceId']]['path']!=o['path']]
if renames:write_csv('name-migration.csv',renames)
lines=['# 场景根对象用途与原名','', '2026-10-29；位置为世界坐标。全部后代逐项记录在 object-roles.csv；功能分类来自组件与引用，不代表每个网格均已视觉确认。','', '|原名|当前名称|世界位置|用途|','|---|---|---|---|']
for o in objects:
 if o['parentInstanceId']==0 and '====' not in o['path']:
  pos=', '.join(f'{v:.2f}' for v in o.get('worldPosition',[]))
  lines.append(f"|{old[o['instanceId']]['path']}|{o['path']}|{pos}|{roles.get(old[o['instanceId']]['root'],'待确认')}|")
(OUT/'场景根对象用途与原名.md').write_text('\n'.join(lines)+'\n',encoding='utf-8')
print(json.dumps({'objects':len(rows),'changedPaths':len(renames),'roots':sum(o['parentInstanceId']==0 for o in objects)},ensure_ascii=False))
