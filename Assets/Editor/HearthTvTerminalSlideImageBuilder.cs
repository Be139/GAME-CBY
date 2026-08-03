#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class HearthTvTerminalSlideImageBuilder
{
    private static bool legacyBatchAuthorized;
    private const int ExpectedSlideCount = 24;
    private const int SlidesPerTerminal = 8;
    private const int PreChoiceSelectionPages = 6;
    private const int ChoicePages = 2;
    private const float ReferenceWidth = 1920f;
    private const float ReferenceHeight = 1080f;
    private const float DefaultTerminalZoom = 1.08f;
    private const string SourcePptxName = "HEARTH-Night-Rounds-Master.pptx";
    private const string SlideImageFolder = "Assets/Resources/UI/HearthTerminalSlideImages";
    private const string PagePrefabFolder = "Assets/Prefabs/UI/HearthHud/TerminalImagePages";
    private const string TerminalPrefabFolder = "Assets/Prefabs/UI/HearthHud/Terminals";
    private const string Terminal17F01PrefabPath = TerminalPrefabFolder + "/Terminal_17F01.prefab";
    private const string Terminal17F02PrefabPath = TerminalPrefabFolder + "/Terminal_17F02.prefab";
    private const string Terminal17F03PrefabPath = TerminalPrefabFolder + "/Terminal_17F03_Alert.prefab";

    [MenuItem("Tools/Hearth/Legacy Unsafe/Terminals/Rebuild TV Terminal UI From PPT Images")]
    public static void RebuildTvTerminalImagePrefabs()
    {
        if (!legacyBatchAuthorized &&
            !HearthLegacyToolGuard.Confirm(
                "Rebuild TV Terminal UI From PPT Images",
                "all legacy image-driven terminal Prefabs"))
        {
            return;
        }

        string pptxPath = FindPptxPath();
        if (string.IsNullOrEmpty(pptxPath))
        {
            Debug.LogError("[HearthTvTerminalSlideImageBuilder] Could not find " + SourcePptxName + ".");
            return;
        }

        EnsureDirectory("Assets/Resources");
        EnsureDirectory("Assets/Resources/UI");
        EnsureDirectory(SlideImageFolder);
        EnsureDirectory("Assets/Prefabs");
        EnsureDirectory("Assets/Prefabs/UI");
        EnsureDirectory("Assets/Prefabs/UI/HearthHud");
        EnsureDirectory(PagePrefabFolder);
        EnsureDirectory(TerminalPrefabFolder);

        string tempRoot = Path.Combine(Path.GetTempPath(), "HearthTvTerminalSlides_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
        string tempPptxPath = Path.Combine(tempRoot, SourcePptxName);
        string exportedPngFolder = Path.Combine(tempRoot, "exported");
        Directory.CreateDirectory(exportedPngFolder);
        CopySharedReadFile(pptxPath, tempPptxPath);

        int exportedCount = ExportSlidesWithPowerPoint(tempPptxPath, exportedPngFolder, ExpectedSlideCount);
        if (exportedCount <= 0)
        {
            Debug.LogError("[HearthTvTerminalSlideImageBuilder] No slides were exported from " + pptxPath + ".");
            return;
        }

        if (exportedCount != ExpectedSlideCount)
        {
            Debug.LogWarning("[HearthTvTerminalSlideImageBuilder] Expected " + ExpectedSlideCount + " slides, exported " + exportedCount + ".");
        }

        Dictionary<int, GameObject> pagePrefabs = new Dictionary<int, GameObject>();
        int slideCount = Mathf.Min(exportedCount, ExpectedSlideCount);
        for (int slideNumber = 1; slideNumber <= slideCount; slideNumber++)
        {
            string exportedPngPath = Path.Combine(exportedPngFolder, "slide_" + slideNumber.ToString("00") + ".png");
            string assetPath = SlideImageFolder + "/TerminalImageSlide" + slideNumber.ToString("00") + ".png";
            ProcessSlideImageToTransparentPng(exportedPngPath, assetPath);
            ConfigureSpriteImporter(assetPath);

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            GameObject pageObject = BuildImagePage(slideNumber, sprite);
            string pagePath = PagePrefabFolder + "/TerminalImageSlide" + slideNumber.ToString("00") + "_" + GetTerminalSlideShortName(slideNumber) + ".prefab";
            GameObject savedPagePrefab = PrefabUtility.SaveAsPrefabAsset(pageObject, pagePath);
            pagePrefabs[slideNumber] = savedPagePrefab;
            UnityEngine.Object.DestroyImmediate(pageObject);
        }

        GameObject terminal17F01 = BuildTerminalGroup("Terminal_17F01", 1, pagePrefabs);
        GameObject terminal17F02 = BuildTerminalGroup("Terminal_17F02", 9, pagePrefabs);
        GameObject terminal17F03 = BuildTerminalGroup("Terminal_17F03_Alert", 17, pagePrefabs);

        GameObject saved17F01 = PrefabUtility.SaveAsPrefabAsset(terminal17F01, Terminal17F01PrefabPath);
        PrefabUtility.SaveAsPrefabAsset(terminal17F02, Terminal17F02PrefabPath);
        PrefabUtility.SaveAsPrefabAsset(terminal17F03, Terminal17F03PrefabPath);

        UnityEngine.Object.DestroyImmediate(terminal17F01);
        UnityEngine.Object.DestroyImmediate(terminal17F02);
        UnityEngine.Object.DestroyImmediate(terminal17F03);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = saved17F01;

        Debug.Log("[HearthTvTerminalSlideImageBuilder] Rebuilt PPT image-driven TV terminal prefabs from " + pptxPath + ".");
    }

    [MenuItem("Tools/Hearth/Legacy Unsafe/Terminals/Rebuild PPT Image Terminals And Apply Current Scene")]
    public static void RebuildAndApplyCurrentScene()
    {
        if (!HearthLegacyToolGuard.Confirm(
                "Rebuild And Apply PPT Image Terminals",
                "legacy terminal Prefabs and the active scene"))
        {
            return;
        }

        legacyBatchAuthorized = true;
        try
        {
            RebuildTvTerminalImagePrefabs();
        }
        finally
        {
            legacyBatchAuthorized = false;
        }

        HearthTvTerminalPrefabBuilder.StandardizeTvByHierarchyPath("17F/ROOM1/TV (3)", Terminal17F01PrefabPath);
        HearthTvTerminalPrefabBuilder.StandardizeTvByHierarchyPath("17F/ROOM3/TV (2)", Terminal17F02PrefabPath);
        HearthTvTerminalPrefabBuilder.StandardizeTvByHierarchyPath("17F/ROOM2/TV (4)", Terminal17F03PrefabPath);

        SetSceneTerminalResidentId("17F/ROOM1/TV (3)/MonitorCanvas/Terminal_17F01", "17F01");
        SetSceneTerminalResidentId("17F/ROOM3/TV (2)/MonitorCanvas/Terminal_17F02", "17F02");
        SetSceneTerminalResidentId("17F/ROOM2/TV (4)/MonitorCanvas/Terminal_17F03_Alert", "17F03");

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("[HearthTvTerminalSlideImageBuilder] Applied PPT image terminals to current 17F scene TVs. Review the diff and save manually.");
    }

    private static void SetSceneTerminalResidentId(string hierarchyPath, string residentId)
    {
        GameObject terminalObject = GameObject.Find(hierarchyPath);
        HearthTvTerminalController controller = terminalObject != null
            ? terminalObject.GetComponent<HearthTvTerminalController>()
            : null;
        if (controller == null)
        {
            Debug.LogWarning("[HearthTvTerminalSlideImageBuilder] Could not set resident id because terminal was not found: " + hierarchyPath);
            return;
        }

        controller.SetReplayResidentId(residentId);
        EditorUtility.SetDirty(controller);
    }

    private static GameObject BuildImagePage(int slideNumber, Sprite sprite)
    {
        GameObject pageRoot = new GameObject(
            "TerminalImageSlide" + slideNumber.ToString("00") + "_" + GetTerminalSlideShortName(slideNumber),
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(CanvasGroup),
            typeof(Image),
            typeof(HearthHudPage));
        StretchToParent(pageRoot.GetComponent<RectTransform>());

        Image image = pageRoot.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.raycastTarget = false;
        image.preserveAspect = false;

        HearthHudPage page = pageRoot.GetComponent<HearthHudPage>();
        page.Configure(
            (HearthHudPageId)slideNumber,
            false,
            slideNumber >= 17 ? HearthHudState.Alert : HearthHudState.Active,
            string.Empty,
            false,
            string.Empty,
            string.Empty);
        page.SetVisible(false);
        return pageRoot;
    }

    private static GameObject BuildTerminalGroup(string prefabName, int firstSlideNumber, Dictionary<int, GameObject> pagePrefabs)
    {
        GameObject root = new GameObject(
            prefabName,
            typeof(RectTransform),
            typeof(CanvasGroup),
            typeof(AudioSource),
            typeof(HearthTerminalCameraTransition),
            typeof(HearthTerminalBootSequence),
            typeof(HearthTvTerminalController));
        StretchToParent(root.GetComponent<RectTransform>());

        CanvasGroup canvasGroup = root.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        AudioSource audioSource = root.GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        Image screenGlass = CreateImage(root.transform, "TerminalScreenGlass", new Rect(0f, 0f, ReferenceWidth, ReferenceHeight), new Color(0.043f, 0.063f, 0.094f, 0.58f));
        screenGlass.raycastTarget = false;

        GameObject contentObject = new GameObject("TerminalContentRoot", typeof(RectTransform), typeof(CanvasGroup));
        contentObject.transform.SetParent(root.transform, false);
        RectTransform contentRect = contentObject.GetComponent<RectTransform>();
        StretchToParent(contentRect);
        contentRect.localScale = Vector3.one * DefaultTerminalZoom;

        CanvasGroup contentGroup = contentObject.GetComponent<CanvasGroup>();
        contentGroup.alpha = 0f;
        contentGroup.interactable = false;
        contentGroup.blocksRaycasts = false;

        List<HearthHudPage> pages = new List<HearthHudPage>();
        for (int slideNumber = firstSlideNumber; slideNumber < firstSlideNumber + SlidesPerTerminal; slideNumber++)
        {
            GameObject pagePrefab;
            if (!pagePrefabs.TryGetValue(slideNumber, out pagePrefab) || pagePrefab == null)
            {
                continue;
            }

            GameObject pageInstance = (GameObject)PrefabUtility.InstantiatePrefab(pagePrefab, contentObject.transform);
            pageInstance.name = pagePrefab.name;
            StretchToParent(pageInstance.GetComponent<RectTransform>());

            HearthHudPage page = pageInstance.GetComponent<HearthHudPage>();
            if (page != null)
            {
                pages.Add(page);
                page.SetVisible(slideNumber == firstSlideNumber);
            }
        }

        BuildBootOverlays(root.transform, contentGroup, contentRect);

        HearthTvTerminalController controller = root.GetComponent<HearthTvTerminalController>();
        controller.Configure(
            null,
            null,
            contentRect,
            canvasGroup,
            pages.ToArray(),
            firstSlideNumber,
            (HearthHudPageId)firstSlideNumber,
            DefaultTerminalZoom);
        controller.SetPageDrivenSelectionStates(true, PreChoiceSelectionPages, ChoicePages);

        return root;
    }

    private static void BuildBootOverlays(Transform parent, CanvasGroup contentGroup, RectTransform contentRect)
    {
        GameObject bootOverlay = new GameObject("TerminalBootOverlay", typeof(RectTransform), typeof(CanvasGroup));
        bootOverlay.transform.SetParent(parent, false);
        StretchToParent(bootOverlay.GetComponent<RectTransform>());
        CanvasGroup bootGroup = bootOverlay.GetComponent<CanvasGroup>();
        bootGroup.alpha = 0f;
        bootGroup.interactable = false;
        bootGroup.blocksRaycasts = false;

        CreateImage(bootOverlay.transform, "BootFlash", new Rect(0f, 0f, ReferenceWidth, ReferenceHeight), new Color(0.55f, 0.95f, 0.85f, 0.18f));

        GameObject scanlineRoot = new GameObject("BootScanlines", typeof(RectTransform));
        scanlineRoot.transform.SetParent(bootOverlay.transform, false);
        StretchToParent(scanlineRoot.GetComponent<RectTransform>());
        for (int y = 0; y < ReferenceHeight; y += 28)
        {
            CreateImage(scanlineRoot.transform, "Scanline_" + y.ToString("0000"), new Rect(0f, y, ReferenceWidth, 2f), new Color(0.68f, 1f, 0.9f, 0.12f));
        }

        GameObject offOverlay = new GameObject("TerminalOffOverlay", typeof(RectTransform), typeof(CanvasGroup));
        offOverlay.transform.SetParent(parent, false);
        StretchToParent(offOverlay.GetComponent<RectTransform>());
        CanvasGroup offGroup = offOverlay.GetComponent<CanvasGroup>();
        offGroup.alpha = 1f;
        offGroup.interactable = false;
        offGroup.blocksRaycasts = false;
        CreateImage(offOverlay.transform, "OffDarkScreen", new Rect(0f, 0f, ReferenceWidth, ReferenceHeight), new Color(0.005f, 0.008f, 0.012f, 0.96f));

        HearthTerminalBootSequence bootSequence = parent.GetComponent<HearthTerminalBootSequence>();
        if (bootSequence != null)
        {
            bootSequence.Configure(contentGroup, offGroup, bootGroup, contentRect);
        }
    }

    private static int ExportSlidesWithPowerPoint(string pptxPath, string outputFolder, int maxSlides)
    {
        string scriptPath = Path.Combine(Path.GetDirectoryName(pptxPath), "export_hearth_terminal_slides.ps1");
        File.WriteAllText(scriptPath, GetPowerPointExportScript());

        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
        startInfo.FileName = "powershell.exe";
        startInfo.Arguments =
            "-NoProfile -ExecutionPolicy Bypass -File " + QuoteProcessArgument(scriptPath) +
            " -PptxPath " + QuoteProcessArgument(pptxPath) +
            " -OutputFolder " + QuoteProcessArgument(outputFolder) +
            " -MaxSlides " + maxSlides.ToString();
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;

        try
        {
            using (System.Diagnostics.Process process = System.Diagnostics.Process.Start(startInfo))
            {
                if (process == null)
                {
                    Debug.LogError("[HearthTvTerminalSlideImageBuilder] Failed to start PowerShell for PPT export.");
                    return 0;
                }

                string stdout = process.StandardOutput.ReadToEnd();
                string stderr = process.StandardError.ReadToEnd();
                bool exited = process.WaitForExit(120000);
                if (!exited)
                {
                    process.Kill();
                    Debug.LogError("[HearthTvTerminalSlideImageBuilder] PowerPoint export timed out.");
                    return 0;
                }

                if (process.ExitCode != 0)
                {
                    Debug.LogError("[HearthTvTerminalSlideImageBuilder] PowerPoint export failed: " + stderr + "\n" + stdout);
                    return 0;
                }
            }
        }
        catch (Exception exception)
        {
            Debug.LogError("[HearthTvTerminalSlideImageBuilder] Failed to export PPT slides: " + exception.Message);
            return 0;
        }

        return Directory.GetFiles(outputFolder, "slide_*.png").Length;
    }

    private static string GetPowerPointExportScript()
    {
        return @"
param(
    [Parameter(Mandatory=$true)][string]$PptxPath,
    [Parameter(Mandatory=$true)][string]$OutputFolder,
    [Parameter(Mandatory=$true)][int]$MaxSlides
)
$ErrorActionPreference = 'Stop'
New-Item -ItemType Directory -Force -Path $OutputFolder | Out-Null
$app = New-Object -ComObject PowerPoint.Application
$presentation = $null
try {
    $presentation = $app.Presentations.Open($PptxPath, $true, $false, $false)
    $count = [Math]::Min($presentation.Slides.Count, $MaxSlides)
    for ($i = 1; $i -le $count; $i++) {
        $path = Join-Path $OutputFolder ('slide_{0:D2}.png' -f $i)
        $presentation.Slides.Item($i).Export($path, 'PNG', 1920, 1080)
    }
    Write-Output ('Exported {0} slides' -f $count)
}
finally {
    if ($presentation -ne $null) {
        $presentation.Close()
        [System.Runtime.InteropServices.Marshal]::ReleaseComObject($presentation) | Out-Null
    }
    if ($app -ne $null) {
        $app.Quit()
        [System.Runtime.InteropServices.Marshal]::ReleaseComObject($app) | Out-Null
    }
    [GC]::Collect()
    [GC]::WaitForPendingFinalizers()
}
";
    }

    private static string QuoteProcessArgument(string value)
    {
        return "\"" + value.Replace("\"", "\\\"") + "\"";
    }

    private static void ProcessSlideImageToTransparentPng(string sourcePngPath, string assetPath)
    {
        byte[] bytes = File.ReadAllBytes(sourcePngPath);
        Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.LoadImage(bytes);
        Color32[] pixels = texture.GetPixels32();

        for (int i = 0; i < pixels.Length; i++)
        {
            Color32 pixel = pixels[i];
            int max = Mathf.Max(pixel.r, Mathf.Max(pixel.g, pixel.b));
            int min = Mathf.Min(pixel.r, Mathf.Min(pixel.g, pixel.b));
            int saturation = max - min;

            if (max <= 28 && saturation <= 24)
            {
                pixel.a = 0;
            }
            else if (max <= 52 && saturation <= 18)
            {
                float alpha = Mathf.InverseLerp(28f, 52f, max);
                pixel.a = (byte)Mathf.RoundToInt(pixel.a * alpha);
            }

            pixels[i] = pixel;
        }

        texture.SetPixels32(pixels);
        texture.Apply(false, false);
        File.WriteAllBytes(assetPath, texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);
    }

    private static void ConfigureSpriteImporter(string assetPath)
    {
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.alphaIsTransparency = true;
        importer.mipmapEnabled = false;
        importer.sRGBTexture = true;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
    }

    private static Image CreateImage(Transform parent, string name, Rect rect, Color color)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        SetTopLeft(imageObject.GetComponent<RectTransform>(), rect);
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
        rect.anchoredPosition = Vector2.zero;
    }

    private static void SetTopLeft(RectTransform rect, Rect pptRect)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(pptRect.x, -pptRect.y);
        rect.sizeDelta = new Vector2(pptRect.width, pptRect.height);
        rect.localScale = Vector3.one;
    }

    private static string GetTerminalSlideShortName(int slideNumber)
    {
        int group = slideNumber <= 8 ? 1 : slideNumber <= 16 ? 9 : 17;
        int local = slideNumber - group + 1;
        string room = group == 17 ? "17F03Alert" : group == 9 ? "17F02" : "17F01";
        switch (local)
        {
            case 1: return room + "_ResidentSummary";
            case 2: return room + "_Acquisition";
            case 3: return room + "_FamilyLog";
            case 4: return room + "_TrustTrend";
            case 5: return room + "_InspectionHistory";
            case 6: return room + "_Action";
            case 7: return room + "_ChoiceA";
            case 8: return room + "_ChoiceB";
            default: return room + "_Page";
        }
    }

    private static void CopySharedReadFile(string sourcePath, string targetPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
        using (FileStream source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
        using (FileStream target = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            source.CopyTo(target);
        }
    }

    private static string FindPptxPath()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        List<string> candidates = new List<string>();

        string desktop = Path.Combine("E:\\桌面", SourcePptxName);
        if (File.Exists(desktop))
        {
            candidates.Add(desktop);
        }

        string downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", SourcePptxName);
        if (File.Exists(downloads))
        {
            candidates.Add(downloads);
        }

        string direct = Path.Combine(projectRoot, SourcePptxName);
        if (File.Exists(direct))
        {
            candidates.Add(direct);
        }

        string[] directProjectMatches = Directory.GetFiles(projectRoot, SourcePptxName, SearchOption.AllDirectories);
        for (int i = 0; i < directProjectMatches.Length; i++)
        {
            if (!candidates.Contains(directProjectMatches[i]))
            {
                candidates.Add(directProjectMatches[i]);
            }
        }

        if (candidates.Count == 0)
        {
            return string.Empty;
        }

        candidates.Sort((left, right) => File.GetLastWriteTimeUtc(right).CompareTo(File.GetLastWriteTimeUtc(left)));
        return candidates[0];
    }

    private static void EnsureDirectory(string assetDirectory)
    {
        if (AssetDatabase.IsValidFolder(assetDirectory))
        {
            return;
        }

        string parent = Path.GetDirectoryName(assetDirectory).Replace("\\", "/");
        string name = Path.GetFileName(assetDirectory);
        if (!AssetDatabase.IsValidFolder(parent))
        {
            EnsureDirectory(parent);
        }

        AssetDatabase.CreateFolder(parent, name);
    }
}
#endif
