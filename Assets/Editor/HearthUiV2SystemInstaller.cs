#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class HearthUiV2SystemInstaller
{
    private const string MenuRoot = "Tools/Hearth/UI V2/System/";
    private const string V2HumanPrefabPath =
        "Assets/Prefabs/UI/HearthHud/V2/HearthHudRoot_V2.prefab";
    private const string ProfilesFolder = "Assets/UI/HEARTH/V2/Profiles";
    private const string ThemeProfilePath =
        ProfilesFolder + "/Hearth_UiV2Theme.asset";
    private const string LayoutProfilePath =
        ProfilesFolder + "/Hearth_UiV2Layout_1920x1080.asset";
    private const string LiberationFontPath =
        "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
    private const string TutorialRootName = "V2_InitialTutorialRoot";

    [MenuItem(MenuRoot + "Install Profiles And Human Tutorial")]
    public static void InstallProfilesAndHumanTutorial()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning(
                "[HearthUiV2SystemInstaller] Exit Play Mode before installing UI assets.");
            return;
        }

        HearthUiThemeProfile theme = EnsureThemeProfile();
        HearthUiLayoutProfile layout = EnsureLayoutProfile();
        GameObject prefab =
            AssetDatabase.LoadAssetAtPath<GameObject>(V2HumanPrefabPath);
        if (prefab == null)
        {
            Debug.LogError(
                "[HearthUiV2SystemInstaller] V2 Human HUD prefab is missing: " +
                V2HumanPrefabPath);
            return;
        }

        GameObject root = PrefabUtility.LoadPrefabContents(V2HumanPrefabPath);
        try
        {
            HearthFirstPersonHudController humanHud =
                root.GetComponent<HearthFirstPersonHudController>();
            HearthUiStateCoordinator coordinator =
                root.GetComponent<HearthUiStateCoordinator>();
            if (coordinator == null)
            {
                coordinator = root.AddComponent<HearthUiStateCoordinator>();
            }

            coordinator.enabled = true;
            coordinator.ConfigureRuntimeSources(
                humanHud,
                null,
                null,
                null);
            coordinator.SetRuntimeIntegration(true, false);

            Transform existing = FindDescendant(root.transform, TutorialRootName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            Transform parent =
                root.transform.Find("PersistentHudLayer") ?? root.transform;
            BuildTutorial(parent, root, theme, layout, coordinator);
            PrefabUtility.SaveAsPrefabAsset(root, V2HumanPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        ValidateProfilesAndHumanTutorial();
        Debug.Log(
            "[HearthUiV2SystemInstaller] Installed the shared UI profiles and the " +
            "10-second Human gameplay tutorial without replacing the functional HUD root.");
    }

    [MenuItem(MenuRoot + "Reset Initial Tutorial Completion")]
    public static void ResetInitialTutorialCompletion()
    {
        HearthInitialGameplayTutorialController.ResetCompletionForCurrentRun();
        HearthInitialGameplayTutorialController[] controllers =
            Object.FindObjectsOfType<HearthInitialGameplayTutorialController>(true);
        for (int i = 0; i < controllers.Length; i++)
        {
            controllers[i].RestartForPreview();
        }

        Debug.Log(
            "[HearthUiV2SystemInstaller] Cleared the initial Human tutorial completion " +
            "flag for the current run and PlayerPrefs save.");
    }

    [MenuItem(MenuRoot + "Validate Profiles And Human Tutorial")]
    public static void ValidateProfilesAndHumanTutorial()
    {
        List<string> issues = new List<string>();
        HearthUiThemeProfile theme =
            AssetDatabase.LoadAssetAtPath<HearthUiThemeProfile>(ThemeProfilePath);
        HearthUiLayoutProfile layout =
            AssetDatabase.LoadAssetAtPath<HearthUiLayoutProfile>(LayoutProfilePath);
        GameObject human =
            AssetDatabase.LoadAssetAtPath<GameObject>(V2HumanPrefabPath);

        if (theme == null)
        {
            issues.Add("Theme profile is missing.");
        }
        else if (theme.PrimaryFontAsset == null)
        {
            issues.Add("Theme profile has no Liberation Sans TMP font.");
        }

        if (layout == null)
        {
            issues.Add("1920 x 1080 layout profile is missing.");
        }
        else if (layout.ReferenceResolution != new Vector2(1920f, 1080f))
        {
            issues.Add("Layout profile reference resolution is not 1920 x 1080.");
        }

        if (human == null)
        {
            issues.Add("V2 Human HUD prefab is missing.");
        }
        else
        {
            HearthFirstPersonHudController humanHud =
                human.GetComponent<HearthFirstPersonHudController>();
            HearthUiStateCoordinator coordinator =
                human.GetComponent<HearthUiStateCoordinator>();
            if (coordinator == null)
            {
                issues.Add("V2 Human HUD has no runtime UI state coordinator.");
            }
            else
            {
                if (!coordinator.enabled)
                {
                    issues.Add("Runtime UI state coordinator is disabled.");
                }

                if (!coordinator.AutomaticallyResolveRuntimeState)
                {
                    issues.Add(
                        "Runtime UI state coordinator is not using automatic resolution.");
                }

                if (coordinator.AppliesResolvedStateToCanvasGroups)
                {
                    issues.Add(
                        "Runtime UI state coordinator must remain passive until " +
                        "dedicated layer groups are bound.");
                }

                if (!coordinator.HasHumanHudBinding ||
                    coordinator.HumanHud != humanHud)
                {
                    issues.Add(
                        "Runtime UI state coordinator is not bound to the Human HUD controller.");
                }

                if (coordinator.HasDuplicateLayerBindings)
                {
                    issues.Add(
                        "Runtime UI state coordinator contains duplicate CanvasGroup bindings.");
                }
            }

            Transform tutorial = FindDescendant(human.transform, TutorialRootName);
            if (tutorial == null)
            {
                issues.Add("V2 Human HUD has no initial tutorial VisualRoot.");
            }
            else
            {
                if (tutorial.GetComponent<HearthActionHintPresenter>() == null)
                {
                    issues.Add("Initial tutorial has no action-hint presenter.");
                }

                HearthInitialGameplayTutorialController tutorialController =
                    tutorial.GetComponent<HearthInitialGameplayTutorialController>();
                if (tutorialController == null)
                {
                    issues.Add("Initial tutorial has no effective-gameplay timer.");
                }
                else if (tutorialController.UiStateCoordinator != coordinator)
                {
                    issues.Add(
                        "Initial tutorial is not bound to the shared UI state coordinator.");
                }

                Image[] images = tutorial.GetComponentsInChildren<Image>(true);
                for (int i = 0; i < images.Length; i++)
                {
                    if (images[i].sprite != null)
                    {
                        issues.Add(
                            "Initial tutorial contains a baked sprite instead of flat UI: " +
                            images[i].name + ".");
                    }
                }
            }
        }

        if (issues.Count == 0)
        {
            Debug.Log(
                "[HearthUiV2SystemInstaller] Validation passed: profiles, Liberation Sans, " +
                "passive runtime state coordination, flat keycaps, 10-second effective " +
                "gameplay timer and 0.35-second fade are installed.");
            return;
        }

        Debug.LogError(
            "[HearthUiV2SystemInstaller] Validation found " +
            issues.Count +
            " issue(s):\n- " +
            string.Join("\n- ", issues));
    }

    private static HearthUiThemeProfile EnsureThemeProfile()
    {
        EnsureFolderTree(ProfilesFolder);
        HearthUiThemeProfile profile =
            AssetDatabase.LoadAssetAtPath<HearthUiThemeProfile>(ThemeProfilePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<HearthUiThemeProfile>();
            AssetDatabase.CreateAsset(profile, ThemeProfilePath);
        }

        TMP_FontAsset font =
            AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(LiberationFontPath);
        SerializedObject serialized = new SerializedObject(profile);
        serialized.FindProperty("primaryFontAsset").objectReferenceValue = font;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(profile);
        return profile;
    }

    private static HearthUiLayoutProfile EnsureLayoutProfile()
    {
        EnsureFolderTree(ProfilesFolder);
        HearthUiLayoutProfile profile =
            AssetDatabase.LoadAssetAtPath<HearthUiLayoutProfile>(LayoutProfilePath);
        if (profile == null)
        {
            profile = ScriptableObject.CreateInstance<HearthUiLayoutProfile>();
            AssetDatabase.CreateAsset(profile, LayoutProfilePath);
        }

        EditorUtility.SetDirty(profile);
        return profile;
    }

    private static void BuildTutorial(
        Transform parent,
        GameObject hudRoot,
        HearthUiThemeProfile theme,
        HearthUiLayoutProfile layout,
        HearthUiStateCoordinator coordinator)
    {
        GameObject tutorial = new GameObject(
            TutorialRootName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Canvas),
            typeof(CanvasGroup),
            typeof(HearthActionHintPresenter),
            typeof(HearthInitialGameplayTutorialController));
        tutorial.transform.SetParent(parent, false);

        RectTransform tutorialRect = tutorial.GetComponent<RectTransform>();
        HearthUiReferenceRect layoutRect =
            layout.GetRegion(HearthUiLayoutRegion.InitialHumanTutorial);
        layoutRect.ApplyTopLeftAnchors(tutorialRect);

        Image background = tutorial.GetComponent<Image>();
        background.sprite = null;
        background.type = Image.Type.Simple;
        background.color = new Color32(9, 16, 28, 212);
        background.raycastTarget = false;

        Canvas tutorialCanvas = tutorial.GetComponent<Canvas>();
        tutorialCanvas.overrideSorting = true;
        tutorialCanvas.sortingOrder = 8200;

        CanvasGroup group = tutorial.GetComponent<CanvasGroup>();
        group.alpha = 1f;
        group.interactable = false;
        group.blocksRaycasts = false;

        CreateImage(
            tutorial.transform,
            "V2_TutorialRule",
            new Rect(0f, 0f, layoutRect.Width, theme.RuleLineThickness),
            theme.Information);

        string[] keys = { "WASD", "MOUSE", "E", "TAB" };
        string[] actions = { "MOVE", "LOOK", "INTERACT", "MENU" };
        HearthActionHintSlot[] slots = new HearthActionHintSlot[keys.Length];
        float slotWidth = layoutRect.Width / keys.Length;
        for (int i = 0; i < keys.Length; i++)
        {
            GameObject slotRoot = CreateRectObject(
                tutorial.transform,
                "Slot_" + keys[i],
                new Rect(i * slotWidth + 10f, 22f, slotWidth - 16f, 58f));
            float keyWidth =
                keys[i] == "E"
                    ? theme.RegularKeycapSize.x
                    : theme.WideKeycapSize.x;
            Image keycap = CreateImage(
                slotRoot.transform,
                "Keycap",
                new Rect(0f, 4f, keyWidth, theme.RegularKeycapSize.y),
                new Color32(18, 28, 42, 238));
            CreateImage(
                keycap.transform,
                "KeyRule",
                new Rect(
                    0f,
                    theme.RegularKeycapSize.y - theme.RuleLineThickness,
                    keyWidth,
                    theme.RuleLineThickness),
                theme.Information);
            TMP_Text keyText = CreateText(
                keycap.transform,
                "Key",
                keys[i],
                new Rect(4f, 6f, keyWidth - 8f, 28f),
                15f,
                TextAlignmentOptions.Center,
                theme.Primary,
                theme.PrimaryFontAsset);
            TMP_Text actionText = CreateText(
                slotRoot.transform,
                "Action",
                actions[i],
                new Rect(
                    keyWidth + 10f,
                    9f,
                    Mathf.Max(42f, slotWidth - keyWidth - 30f),
                    28f),
                15f,
                TextAlignmentOptions.MidlineLeft,
                theme.Secondary,
                theme.PrimaryFontAsset);
            keyText.enableWordWrapping = false;
            actionText.enableWordWrapping = false;

            slots[i] = new HearthActionHintSlot();
            slots[i].Configure(
                slotRoot,
                keyText,
                actionText,
                keycap.rectTransform);
        }

        HearthActionHintPresenter presenter =
            tutorial.GetComponent<HearthActionHintPresenter>();
        presenter.Configure(group, null, slots, theme);

        HearthInitialGameplayTutorialController controller =
            tutorial.GetComponent<HearthInitialGameplayTutorialController>();
        controller.Configure(
            presenter,
            group,
            hudRoot.GetComponent<HearthFirstPersonHudController>());
        controller.SetUiStateCoordinator(coordinator);
    }

    private static GameObject CreateRectObject(
        Transform parent,
        string objectName,
        Rect rect)
    {
        GameObject result = new GameObject(objectName, typeof(RectTransform));
        result.transform.SetParent(parent, false);
        SetTopLeft(result.GetComponent<RectTransform>(), rect);
        return result;
    }

    private static Image CreateImage(
        Transform parent,
        string objectName,
        Rect rect,
        Color color)
    {
        GameObject result = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        result.transform.SetParent(parent, false);
        SetTopLeft(result.GetComponent<RectTransform>(), rect);
        Image image = result.GetComponent<Image>();
        image.sprite = null;
        image.type = Image.Type.Simple;
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static TMP_Text CreateText(
        Transform parent,
        string objectName,
        string value,
        Rect rect,
        float fontSize,
        TextAlignmentOptions alignment,
        Color color,
        TMP_FontAsset font)
    {
        GameObject result = new GameObject(
            objectName,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        result.transform.SetParent(parent, false);
        SetTopLeft(result.GetComponent<RectTransform>(), rect);
        TMP_Text text = result.GetComponent<TMP_Text>();
        text.text = value;
        text.font = font;
        text.fontSize = fontSize;
        text.enableAutoSizing = false;
        text.enableWordWrapping = false;
        text.overflowMode = TextOverflowModes.Overflow;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        return text;
    }

    private static void SetTopLeft(RectTransform rect, Rect bounds)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(bounds.x, -bounds.y);
        rect.sizeDelta = new Vector2(bounds.width, bounds.height);
        rect.localScale = Vector3.one;
        rect.localRotation = Quaternion.identity;
    }

    private static Transform FindDescendant(
        Transform root,
        string objectName)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].name == objectName)
            {
                return transforms[i];
            }
        }

        return null;
    }

    private static void EnsureFolderTree(string folderPath)
    {
        string[] segments = folderPath.Split('/');
        string current = segments[0];
        for (int i = 1; i < segments.Length; i++)
        {
            string next = current + "/" + segments[i];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, segments[i]);
            }

            current = next;
        }
    }
}
#endif
