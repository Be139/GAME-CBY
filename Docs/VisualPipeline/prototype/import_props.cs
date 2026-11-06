// Import every generated prop copied under Assets/Art/Generated/NoahRoom: real-size mesh, lightmap UVs, one matte URP material.
var baseDir = "Assets/Art/Generated/NoahRoom";
var sb = new System.Text.StringBuilder();
UnityEditor.AssetDatabase.Refresh();
foreach (var dir in UnityEditor.AssetDatabase.GetSubFolders(baseDir))
{
    var id = System.IO.Path.GetFileName(dir); if (id == "shared") continue;
    var p = dir + "/" + id + ".fbx";
    var mi = UnityEditor.AssetImporter.GetAtPath(p) as UnityEditor.ModelImporter; if (mi == null) { sb.AppendLine("no fbx " + id); continue; }
    if (mi.generateSecondaryUV && mi.secondaryUVPackMargin == 48f) continue;
    var texPath = dir + "/texture_0.png";
    if (UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(texPath) == null) { mi.ExtractTextures(dir); UnityEditor.AssetDatabase.Refresh(); }
    var ti = UnityEditor.AssetImporter.GetAtPath(texPath) as UnityEditor.TextureImporter;
    if (ti != null) { ti.maxTextureSize = (id == "bed" || id == "bookshelf" || id == "desk" || id == "dresser") ? 2048 : 1024; ti.textureCompression = UnityEditor.TextureImporterCompression.Compressed; ti.SaveAndReimport(); }
    var matPath = dir + "/M_" + id + ".mat";
    var m = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(matPath);
    if (m == null) { m = new UnityEngine.Material(UnityEngine.Shader.Find("Universal Render Pipeline/Lit")); UnityEditor.AssetDatabase.CreateAsset(m, matPath); }
    m.SetTexture("_BaseMap", UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>(texPath)); m.SetColor("_BaseColor", UnityEngine.Color.white); m.SetFloat("_Smoothness", 0.14f); m.SetFloat("_Metallic", 0f);
    UnityEditor.EditorUtility.SetDirty(m);
    mi.useFileScale = false; mi.globalScale = 1f; mi.generateSecondaryUV = true; mi.secondaryUVHardAngle = 80f; mi.secondaryUVPackMargin = 48f; mi.importNormals = UnityEditor.ModelImporterNormals.Import; mi.importAnimation = false; mi.animationType = UnityEditor.ModelImporterAnimationType.None;
    mi.materialImportMode = UnityEditor.ModelImporterMaterialImportMode.ImportViaMaterialDescription;
    foreach (var o in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(p)) { var im = o as UnityEngine.Material; if (im != null) mi.AddRemap(new UnityEditor.AssetImporter.SourceAssetIdentifier(typeof(UnityEngine.Material), im.name), m); }
    mi.SaveAndReimport();
    var go = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(p);
    var mf = go.GetComponentInChildren<UnityEngine.MeshFilter>();
    sb.AppendLine(id + " tris=" + mf.sharedMesh.triangles.Length / 3 + " size=" + mf.sharedMesh.bounds.size.ToString("0.000") + " center=" + mf.sharedMesh.bounds.center.ToString("0.000") + " mat=" + go.GetComponentInChildren<UnityEngine.MeshRenderer>().sharedMaterial.name);
}
UnityEditor.AssetDatabase.SaveAssets();
return sb.ToString();
