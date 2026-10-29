#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static partial class HearthProductionUiTools
{
    [MenuItem(MenuRoot + "Audit/Export Scene Overlay Images")]
    public static void ExportSceneOverlayImages()
    {
        string folder = "Temp/HearthUiReview/Pages";
        Directory.CreateDirectory(folder);
        var sources = UnityEngine.Object.FindObjectsOfType<Canvas>(true).Where(c => c.gameObject.scene.IsValid() &&
            (c.name == "LobbyNarrativeCanvas" || c.name == "ShutdownChallenge_17F04" || c.name == "HumanCanvas")).ToArray();
        foreach (Canvas source in sources)
        {
            var scene = EditorSceneManager.NewPreviewScene();
            GameObject copy = UnityEngine.Object.Instantiate(source.gameObject);
            try
            {
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(copy, scene);
                foreach (var behaviour in copy.GetComponentsInChildren<MonoBehaviour>(true))
                    if (!(behaviour is TMP_Text) && !(behaviour is UnityEngine.UI.Graphic)) behaviour.enabled = false;
                foreach (var group in copy.GetComponentsInChildren<CanvasGroup>(true)) { group.gameObject.SetActive(true); group.alpha = 1; }
                if (source.name == "LobbyNarrativeCanvas")
                    foreach (Transform t in copy.GetComponentsInChildren<Transform>(true))
                        if (t.name == "PinnedLilyMessage" || t.name == "ActivationPanel") t.gameObject.SetActive(false);
                copy.SetActive(true);
                foreach (var typography in copy.GetComponentsInChildren<HearthUiTypography>(true)) typography.Apply();
                RenderAuditPage(copy, folder + "/Scene__" + source.name);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(copy);
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }
    }

    // Isolated prefab previews: never save preview state into an asset or the open scene.
    [MenuItem(MenuRoot + "Audit/Export All Page Images")]
    public static void ExportAllPageImages() { ExportPageImages(""); }

    public static string ExportPageImages(string prefabName)
    {
        string folder = "Temp/HearthUiReview/Pages";
        Directory.CreateDirectory(folder);
        int count = 0;
        foreach (string path in CanonicalPrefabs.Where(p => p.Contains(prefabName)))
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                root.SetActive(true);
                var pages = root.GetComponentsInChildren<HearthHudPage>(true).Select(p => p.gameObject)
                    .Concat(root.GetComponentsInChildren<HearthFirstPersonHudPage>(true).Select(p => p.gameObject)).ToArray();
                var active = root.GetComponentsInChildren<Transform>(true).ToDictionary(t => t.gameObject, t => t.gameObject.activeSelf);
                var variants = pages.Length == 0 ? new[] {root} : pages;
                foreach (GameObject page in variants)
                {
                    foreach (var pair in active) pair.Key.SetActive(pair.Value);
                    foreach (var p in pages) p.SetActive(p == page);
                    for (Transform t = page.transform; t != null; t = t.parent) t.gameObject.SetActive(true);
                    foreach (CanvasGroup group in root.GetComponentsInChildren<CanvasGroup>(true)) group.alpha = 1;
                    string name = Path.GetFileNameWithoutExtension(path) + "__" + page.name;
                    RenderAuditPage(root, folder + "/" + name);
                    count++;
                }
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        return count + " page images exported to " + folder;
    }

    private static void RenderAuditPage(GameObject root, string output)
    {
        Canvas canvas = root.GetComponent<Canvas>();
        if (canvas == null) canvas = root.AddComponent<Canvas>();
        Transform originalParent = canvas.transform.parent;
        canvas.transform.SetParent(null, false);
        canvas.gameObject.SetActive(true);
        var rect = (RectTransform)canvas.transform;
        rect.position = Vector3.zero;
        rect.rotation = Quaternion.identity;
        rect.localScale = Vector3.one;
        rect.pivot = new Vector2(.5f, .5f);
        rect.sizeDelta = new Vector2(1920, 1080);
        foreach (var scaler in canvas.GetComponentsInChildren<CanvasScaler>(true)) scaler.enabled = false;
        canvas.renderMode = RenderMode.WorldSpace;
        ulong mask = EditorSceneManager.CalculateAvailableSceneCullingMask();
        EditorSceneManager.SetSceneCullingMask(root.scene, mask);
        var go = new GameObject("AuditCamera");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(go, root.scene);
        var camera = go.AddComponent<Camera>();
        camera.scene = root.scene;
        camera.overrideSceneCullingMask = mask;
        camera.transform.position = new Vector3(0, 0, -100);
        camera.orthographic = true;
        camera.orthographicSize = 540;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.045f, .055f, .065f);
        canvas.worldCamera = camera;
        var rt = new RenderTexture(1920, 1080, 24);
        var tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
        RenderTexture previous = RenderTexture.active;
        try
        {
            Canvas.ForceUpdateCanvases();
            var report = new List<string> {"path\tfontSize\twidth\theight\tpreferredWidth\tpreferredHeight\ttext"};
            foreach (TMP_Text text in canvas.GetComponentsInChildren<TMP_Text>())
            {
                text.ForceMeshUpdate();
                report.Add(AnimationUtility.CalculateTransformPath(text.transform, canvas.transform) + "\t" + text.fontSize + "\t" +
                    text.rectTransform.rect.width + "\t" + text.rectTransform.rect.height + "\t" + text.preferredWidth + "\t" +
                    text.preferredHeight + "\t" + text.text.Replace("\n", " | ").Replace("\t", " "));
            }
            camera.targetTexture = rt;
            camera.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            tex.Apply();
            File.WriteAllBytes(output + ".png", tex.EncodeToPNG());
            File.WriteAllLines(output + ".tsv", report, new System.Text.UTF8Encoding(false));
        }
        finally
        {
            RenderTexture.active = previous;
            UnityEngine.Object.DestroyImmediate(tex);
            camera.targetTexture = null;
            rt.Release();
            UnityEngine.Object.DestroyImmediate(rt);
            UnityEngine.Object.DestroyImmediate(go);
            canvas.transform.SetParent(originalParent, false);
        }
    }
}
#endif
