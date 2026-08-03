using System;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class HearthInteractionPromptReferenceCapture
{
    private const int PreviewLayer = 30;
    private const string LegacyHumanPrefab =
        "Assets/Prefabs/UI/HearthHud/HearthHudRoot.prefab";
    private const string LegacyCompanionPrefab =
        "Assets/Prefabs/UI/HearthHud/Companion/HearthCompanionHudRoot.prefab";
    private const string V2HumanPrefab =
        "Assets/Prefabs/UI/HearthHud/V2/HearthHudRoot_V2.prefab";
    private const string V2CompanionPrefab =
        "Assets/Prefabs/UI/HearthHud/V2/Companion/HearthCompanionHudRoot_V2.prefab";

    [MenuItem("Tools/Hearth/UI V2/Reference/Capture E-Hold E Comparison")]
    public static void CaptureComparison()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string outputFolder = Path.Combine(
            projectRoot,
            "Documentation",
            "HEARTH_UI_V2_Reference");
        Directory.CreateDirectory(outputFolder);

        string legacyPath = Path.Combine(outputFolder, "V1_E_HoldE.png");
        string v2Path = Path.Combine(outputFolder, "V2_E_HoldE.png");
        Capture(
            LegacyHumanPrefab,
            LegacyCompanionPrefab,
            "ORIGINAL V1 INTERACTION PROMPTS",
            legacyPath);
        Capture(
            V2HumanPrefab,
            V2CompanionPrefab,
            "V1 STRUCTURE / V2 COLOR SYSTEM",
            v2Path);

        AssetDatabase.Refresh();
        Debug.Log(
            "[HearthInteractionPromptReferenceCapture] Wrote reference screenshots:\n" +
            legacyPath + "\n" + v2Path);
    }

    private static void Capture(
        string humanPrefabPath,
        string companionPrefabPath,
        string heading,
        string outputPath)
    {
        GameObject humanPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>(humanPrefabPath);
        GameObject companionPrefab =
            AssetDatabase.LoadAssetAtPath<GameObject>(companionPrefabPath);
        if (humanPrefab == null || companionPrefab == null)
        {
            throw new InvalidOperationException(
                "Missing interaction reference prefab: " +
                humanPrefabPath + " or " + companionPrefabPath);
        }

        Transform shortSource = FindDescendant(
            humanPrefab.transform,
            "PlayerInteractionPrompt");
        Transform holdSource = FindDescendant(
            companionPrefab.transform,
            "HoldPrompt");
        if (shortSource == null || holdSource == null)
        {
            throw new InvalidOperationException(
                "The selected HUD prefabs do not expose PlayerInteractionPrompt and HoldPrompt.");
        }

        Scene previousActiveScene = SceneManager.GetActiveScene();
        Scene previewScene = EditorSceneManager.NewScene(
            NewSceneSetup.EmptyScene,
            NewSceneMode.Additive);
        SceneManager.SetActiveScene(previewScene);
        RenderTexture target = null;
        Texture2D capture = null;
        try
        {
            Camera camera = CreateCamera(previewScene);
            Canvas canvas = CreateCanvas(previewScene, camera);
            CreateBackdrop(canvas.transform);
            CreateHeading(canvas.transform, heading);

            GameObject shortPrompt = UnityEngine.Object.Instantiate(
                shortSource.gameObject);
            SceneManager.MoveGameObjectToScene(shortPrompt, previewScene);
            shortPrompt.transform.SetParent(canvas.transform, false);
            PreparePrompt(shortPrompt, "E  INTERACT", new Vector2(0f, 118f));

            GameObject holdPrompt = UnityEngine.Object.Instantiate(
                holdSource.gameObject);
            SceneManager.MoveGameObjectToScene(holdPrompt, previewScene);
            holdPrompt.transform.SetParent(canvas.transform, false);
            PreparePrompt(holdPrompt, "HOLD E  INTERACT", new Vector2(0f, -90f));
            Image progress = FindImage(holdPrompt.transform, "HoldProgressFill");
            if (progress != null)
            {
                progress.type = Image.Type.Filled;
                progress.fillMethod = Image.FillMethod.Horizontal;
                progress.fillOrigin = 0;
                progress.fillAmount = 0.62f;
            }

            target = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
            camera.targetTexture = target;
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                canvas.GetComponent<RectTransform>());
            TMP_Text[] allText = canvas.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < allText.Length; i++)
            {
                allText[i].ForceMeshUpdate(true, true);
            }
            Canvas.ForceUpdateCanvases();
            camera.Render();

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = target;
            capture = new Texture2D(1920, 1080, TextureFormat.RGBA32, false);
            capture.ReadPixels(new Rect(0f, 0f, 1920f, 1080f), 0, 0);
            capture.Apply();
            File.WriteAllBytes(outputPath, capture.EncodeToPNG());
            RenderTexture.active = previous;
        }
        finally
        {
            if (capture != null)
            {
                UnityEngine.Object.DestroyImmediate(capture);
            }
            if (target != null)
            {
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
            if (previousActiveScene.IsValid() && previousActiveScene.isLoaded)
            {
                SceneManager.SetActiveScene(previousActiveScene);
            }
            EditorSceneManager.CloseScene(previewScene, true);
        }
    }

    private static Camera CreateCamera(Scene scene)
    {
        GameObject cameraObject = new GameObject("ReferenceCamera", typeof(Camera));
        SceneManager.MoveGameObjectToScene(cameraObject, scene);
        Camera camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.012f, 0.018f, 0.032f, 1f);
        camera.orthographic = true;
        camera.orthographicSize = 5.4f;
        camera.cullingMask = 1 << PreviewLayer;
        camera.transform.position = new Vector3(0f, 0f, -10f);
        return camera;
    }

    private static Canvas CreateCanvas(Scene scene, Camera camera)
    {
        GameObject canvasObject = new GameObject(
            "ReferenceCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler));
        SceneManager.MoveGameObjectToScene(canvasObject, scene);
        canvasObject.layer = PreviewLayer;
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 1f;
        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(1920f, 1080f);
        canvasRect.localScale = Vector3.one;
        canvasRect.localPosition = Vector3.zero;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    private static void CreateBackdrop(Transform parent)
    {
        GameObject backdropObject = new GameObject(
            "ReferenceBackdrop",
            typeof(RectTransform),
            typeof(Image));
        backdropObject.transform.SetParent(parent, false);
        backdropObject.layer = PreviewLayer;
        RectTransform rect = backdropObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Image image = backdropObject.GetComponent<Image>();
        image.color = new Color(0.012f, 0.018f, 0.032f, 1f);
        image.raycastTarget = false;
    }

    private static void CreateHeading(Transform parent, string heading)
    {
        GameObject labelObject = new GameObject(
            "ReferenceHeading",
            typeof(RectTransform),
            typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(parent, false);
        labelObject.layer = PreviewLayer;
        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(0f, -124f);
        rect.sizeDelta = new Vector2(1400f, 72f);
        TextMeshProUGUI text = labelObject.GetComponent<TextMeshProUGUI>();
        text.text = heading;
        text.fontSize = 34f;
        text.fontStyle = FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(0.78f, 0.9f, 1f, 1f);
        text.raycastTarget = false;
    }

    private static void PreparePrompt(
        GameObject prompt,
        string textValue,
        Vector2 position)
    {
        SetHierarchyActive(prompt.transform);
        CanvasGroup[] groups = prompt.GetComponentsInChildren<CanvasGroup>(true);
        for (int i = 0; i < groups.Length; i++)
        {
            groups[i].alpha = 1f;
            groups[i].interactable = false;
            groups[i].blocksRaycasts = false;
        }

        bool isHoldPrompt =
            prompt.name.StartsWith("HoldPrompt", StringComparison.Ordinal);
        if (isHoldPrompt)
        {
            NormalizeHoldPrompt(prompt.transform);
            SetPromptText(prompt.transform, "HoldPromptText", "HOLD TO INTERACT");
            SetPromptText(prompt.transform, "HoldKeyText", "E");
            SetPromptText(prompt.transform, "HoldProgressText", "HOLD");
        }
        else
        {
            TMP_Text[] labels = prompt.GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < labels.Length; i++)
            {
                labels[i].text = textValue;
                labels[i].raycastTarget = false;
            }
        }

        RectTransform rect = prompt.GetComponent<RectTransform>();
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        if (isHoldPrompt)
        {
            rect.sizeDelta = new Vector2(680f, 150f);
        }
        else if (rect.sizeDelta.x < 320f || rect.sizeDelta.y < 48f)
        {
            rect.sizeDelta = new Vector2(600f, 76f);
        }
    }

    private static void NormalizeHoldPrompt(Transform hold)
    {
        SetTopLeft(hold.Find("HoldPromptBox"), 24f, 18f, 632f, 69.333f);
        SetTopLeft(hold.Find("HoldPromptText"), 62.267f, 34f, 555.6f, 34f);
        SetTopLeft(hold.Find("HoldKeyText"), 250.533f, 94f, 24f, 20f);
        SetTopLeft(hold.Find("HoldProgressText"), 278.667f, 93f, 226.667f, 22f);
        SetTopLeft(hold.Find("HoldProgressBack"), 62.667f, 119f, 554.667f, 5.333f);
        SetTopLeft(hold.Find("HoldProgressFill"), 62.667f, 119f, 554.667f, 5.333f);
    }

    private static void SetTopLeft(
        Transform target,
        float x,
        float y,
        float width,
        float height)
    {
        RectTransform rect = target as RectTransform;
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private static void SetPromptText(
        Transform root,
        string objectName,
        string value)
    {
        Transform target = FindDescendant(root, objectName);
        TMP_Text text = target != null ? target.GetComponent<TMP_Text>() : null;
        if (text == null)
        {
            return;
        }

        text.text = value;
        text.raycastTarget = false;
    }

    private static void SetHierarchyActive(Transform root)
    {
        root.gameObject.SetActive(true);
        root.gameObject.layer = PreviewLayer;
        for (int i = 0; i < root.childCount; i++)
        {
            SetHierarchyActive(root.GetChild(i));
        }
    }

    private static Transform FindDescendant(Transform root, string objectName)
    {
        if (root.name == objectName)
        {
            return root;
        }
        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindDescendant(root.GetChild(i), objectName);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    private static Image FindImage(Transform root, string objectName)
    {
        Transform found = FindDescendant(root, objectName);
        return found != null ? found.GetComponent<Image>() : null;
    }
}
