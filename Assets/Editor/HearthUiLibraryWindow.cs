#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>Searchable authoring index; never rebuilds UI or changes gameplay state.</summary>
public sealed class HearthUiLibraryWindow : EditorWindow
{
    public const string CatalogPath = "Assets/UI/HEARTH/V2/HearthUiLibrary.json";
    [Serializable] public sealed class Catalog { public Entry[] entries; }
    [Serializable] public sealed class Entry
    {
        public string id, title, asset, sceneObject, owner, appears, coexist, excludes, layer, status;
    }
    private Catalog catalog;
    private Vector2 scroll;
    private string search = "";
    [MenuItem("Tools/Hearth/UI Library/Open Library")]
    public static void Open() { GetWindow<HearthUiLibraryWindow>("HEARTH UI Library"); }
    private void OnEnable() { Reload(); }
    private void Reload() { catalog = JsonUtility.FromJson<Catalog>(File.ReadAllText(CatalogPath)); }
    private void OnGUI()
    {
        EditorGUILayout.HelpBox("正式资产索引；打开资产只进入编辑，不执行迁移或运行预览。场景对象仅在已加载场景中定位。", MessageType.Info);
        EditorGUILayout.BeginHorizontal();
        search=EditorGUILayout.TextField("搜索 ID / 用途 / 调用方",search);
        if(GUILayout.Button("刷新",GUILayout.Width(55))) Reload();
        if(GUILayout.Button("制作规范",GUILayout.Width(85))) Application.OpenURL(Path.GetFullPath("HEARTH_UI库制作与协作规范.md"));
        EditorGUILayout.EndHorizontal();
        scroll=EditorGUILayout.BeginScrollView(scroll);
        if(catalog!=null && catalog.entries!=null) foreach(var e in catalog.entries)
        {
            if(!string.IsNullOrEmpty(search) && JsonUtility.ToJson(e).IndexOf(search,StringComparison.OrdinalIgnoreCase)<0) continue;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(e.id+" · "+e.title,EditorStyles.boldLabel);
            foreach(var line in new[]{"状态："+e.status,"出现："+e.appears,"共存："+e.coexist,"互斥："+e.excludes,"层级："+e.layer,"调用方："+e.owner,"资产："+e.asset,"场景路径："+e.sceneObject})
                EditorGUILayout.LabelField(line,EditorStyles.wordWrappedLabel);
            if(string.IsNullOrEmpty(e.sceneObject))
            {
                var asset=AssetDatabase.LoadAssetAtPath<GameObject>(e.asset);
                if(asset!=null && GUILayout.Button("打开正式 Prefab")) AssetDatabase.OpenAsset(asset);
                if(asset!=null)
                {
                    var pages=asset.GetComponentsInChildren<HearthFirstPersonHudPage>(true).Select(p=>p.PageId.ToString())
                        .Concat(asset.GetComponentsInChildren<HearthHudPage>(true).Select(p=>p.PageId.ToString()));
                    EditorGUILayout.LabelField("资产内页面（存在不代表剧情可达）："+string.Join(" / ",pages),EditorStyles.wordWrappedLabel);
                }
            }
            else if(GUILayout.Button("定位已加载场景对象"))
            {
                var target=Resources.FindObjectsOfTypeAll<Transform>().FirstOrDefault(t=>t.gameObject.scene.IsValid() && HierarchyPath(t)==e.sceneObject);
                if(target!=null) {Selection.activeGameObject=target.gameObject;EditorGUIUtility.PingObject(target.gameObject);}
                else ShowNotification(new GUIContent("请先打开 SampleScene；不会自动切换或保存场景"));
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndScrollView();
    }
    public static string HierarchyPath(Transform t)
    {
        string path=t.name;while(t.parent!=null){t=t.parent;path=t.name+"/"+path;}return path;
    }
    [MenuItem("Tools/Hearth/UI Library/Validate Catalog And Loaded Layers")]
    public static void Validate()
    {
        var c=JsonUtility.FromJson<Catalog>(File.ReadAllText(CatalogPath));int errors=0;
        foreach(var g in c.entries.GroupBy(e=>e.id)) if(g.Count()>1){Debug.LogError("Duplicate UI ID: "+g.Key);errors++;}
        foreach(var e in c.entries)
            if(AssetDatabase.LoadMainAssetAtPath(e.asset)==null){Debug.LogError("Missing UI asset: "+e.id+" "+e.asset);errors++;}
        foreach(var order in Resources.FindObjectsOfTypeAll<HearthUiCanvasOrder>().Where(x=>x.gameObject.scene.IsValid()))
        {
            var canvas=order.GetComponent<Canvas>();
            if(canvas.sortingOrder!=(int)order.Band){Debug.LogError("Canvas order drift: "+HierarchyPath(order.transform),order);errors++;}
        }
        Debug.Log("[UI Library] entries="+c.entries.Length+"; catalog/order errors="+errors+". Coexistence requires runtime scenario checks.");
    }
}
#endif
