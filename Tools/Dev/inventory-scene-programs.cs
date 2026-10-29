var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
var nodes=new System.Collections.Generic.List<object>();
var links=new System.Collections.Generic.List<object>();
var identityCache=new System.Collections.Generic.Dictionary<int,string>();
System.Func<UnityEngine.Object,string> getId=value=>{int key=value.GetInstanceID();string result;if(!identityCache.TryGetValue(key,out result)){result=GlobalObjectId.GetGlobalObjectIdSlow(value).ToString();identityCache.Add(key,result);}return result;};
string section="UNSECTIONED";
foreach(var root in scene.GetRootGameObjects()){
 if(root.name.Contains("====")) section=root.name;
 foreach(var t in root.GetComponentsInChildren<Transform>(true)){
  var path=HearthUiLibraryWindow.HierarchyPath(t);
  var components=new System.Collections.Generic.List<object>();
  foreach(var c in t.GetComponents<Component>()){
   if(c==null){components.Add(new{type="MISSING_SCRIPT",script=""});continue;}
   var mono=c as MonoBehaviour;
   string script=mono!=null?AssetDatabase.GetAssetPath(MonoScript.FromMonoBehaviour(mono)):"";
   components.Add(new{type=c.GetType().FullName,script=script,instanceId=c.GetInstanceID(),enabled=c is Behaviour?((Behaviour)c).enabled:(bool?)null});
   if(mono==null||string.IsNullOrEmpty(script))continue;
   var so=new SerializedObject(c);var p=so.GetIterator();
   while(p.Next(true)){
    if(p.propertyType!=SerializedPropertyType.ObjectReference||p.propertyPath=="m_Script")continue;
    var value=p.objectReferenceValue;if(value==null)continue;
    var target=value as GameObject;var component=value as Component;if(component!=null)target=component.gameObject;
    links.Add(new{source=path,sourceId=getId(c),sourceInstanceId=c.GetInstanceID(),script=script,field=p.propertyPath,target=target!=null&&target.scene.IsValid()?HearthUiLibraryWindow.HierarchyPath(target.transform):AssetDatabase.GetAssetPath(value),targetId=getId(value),targetInstanceId=value.GetInstanceID(),targetType=value.GetType().FullName});
   }
  }
  var renderer=t.GetComponent<Renderer>();
  var mesh=t.GetComponent<MeshFilter>();
  nodes.Add(new{path=path,id=GlobalObjectId.GetGlobalObjectIdSlow(t.gameObject).ToString(),instanceId=t.gameObject.GetInstanceID(),parentInstanceId=t.parent!=null?t.parent.gameObject.GetInstanceID():0,worldPosition=new[]{t.position.x,t.position.y,t.position.z},mesh=mesh!=null&&mesh.sharedMesh!=null?AssetDatabase.GetAssetPath(mesh.sharedMesh):"",materials=renderer!=null?renderer.sharedMaterials.Select(m=>m!=null?AssetDatabase.GetAssetPath(m):"").ToArray():new string[0],localPosition=new[]{t.localPosition.x,t.localPosition.y,t.localPosition.z},localRotation=new[]{t.localRotation.x,t.localRotation.y,t.localRotation.z,t.localRotation.w},localScale=new[]{t.localScale.x,t.localScale.y,t.localScale.z},root=root.name,section=section,sibling=t.GetSiblingIndex(),activeSelf=t.gameObject.activeSelf,activeHierarchy=t.gameObject.activeInHierarchy,prefab=PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(t.gameObject),components=components});
 }
}
var payload=new{scene=scene.path,dirty=scene.isDirty,playing=EditorApplication.isPlaying,objects=nodes,links=links};
var json=Newtonsoft.Json.JsonConvert.SerializeObject(payload,Newtonsoft.Json.Formatting.Indented);
System.IO.File.WriteAllText("Logs/HearthUiReview/scene-program-inventory.json",json,new System.Text.UTF8Encoding(false));
return new{scene=scene.path,dirty=scene.isDirty,playing=EditorApplication.isPlaying,objects=nodes.Count,links=links.Count,bytes=json.Length};
