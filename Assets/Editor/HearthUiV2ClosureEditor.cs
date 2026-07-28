#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class HearthUiV2ClosureEditor
{
    private const string HumanPrefab =
        "Assets/Prefabs/UI/HearthHud/V2/HearthHudRoot_V2.prefab";
    private const string CompanionPrefab =
        "Assets/Prefabs/UI/HearthHud/V2/Companion/HearthCompanionHudRoot_V2.prefab";
    private const string TerminalFolder =
        "Assets/Prefabs/UI/HearthHud/V2/Terminals/";
    private const string CompanionFramePath =
        "Assets/UI/HEARTH/GeneratedParts/Companion/HUD_Companion_FullscreenFrame.png";
    private const string ButtonFramePath =
        "Assets/UI/HEARTH/GeneratedParts/Common/HUD_Common_ButtonFrame_9Slice.png";
    private const string PromptFramePath =
        "Assets/UI/HEARTH/GeneratedParts/Interaction/HUD_Interaction_GazePromptFrame_9Slice.png";

    private static readonly Color DeepBlueBlack =
        new Color32(11, 16, 24, 235);
    private static readonly Color GreyBlue =
        new Color32(95, 120, 149, 242);
    private static readonly Color ColdWhite =
        new Color32(215, 230, 246, 255);
    private static readonly Color LowSaturationBlue =
        new Color32(120, 170, 220, 220);
    private static readonly Color Red =
        new Color32(228, 62, 54, 255);

    [MenuItem("Tools/Hearth/UI V2/Apply Approved Closure")]
    public static void ApplyAll()
    {
        ApplyHuman();
        ApplyCompanion();
        ApplyTerminal(
            TerminalFolder + "Terminal_Lobby_Assignment_V2.prefab",
            HearthTerminalMode.LobbySync);
        ApplyTerminal(
            TerminalFolder + "Terminal_17F01_V2.prefab",
            HearthTerminalMode.Doorway);
        ApplyTerminal(
            TerminalFolder + "Terminal_17F02_V2.prefab",
            HearthTerminalMode.Doorway);
        ApplyTerminal(
            TerminalFolder + "Terminal_17F03_Alert_V2.prefab",
            HearthTerminalMode.Doorway);
        ApplyTerminal(
            TerminalFolder + "Terminal_17F04_Home_V2.prefab",
            HearthTerminalMode.Home);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log(
            "[HearthUiV2ClosureEditor] Applied the approved second-UI closure " +
            "without rebuilding scene bindings or legacy prefabs.");
    }

    [MenuItem("Tools/Hearth/UI V2/Closure/Apply Human Prefab Closure")]
    public static void ApplyHuman()
    {
        EditPrefab(
            HumanPrefab,
            root =>
            {
                CanvasScaler scaler = root.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode =
                        CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920f, 1080f);
                    scaler.screenMatchMode =
                        CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    scaler.matchWidthOrHeight = 0.5f;
                }

                Transform persistent = FindNamed(root.transform, "PersistentHud");
                if (persistent != null)
                {
                    ApplyPersistentHumanLayout(persistent);
                }

                SetBottomLeft(
                    FindRect(root.transform, "LocationHud"),
                    64f,
                    48f,
                    340f,
                    92f);
                AlignNamedText(
                    root.transform,
                    "LocationTitleText",
                    TextAlignmentOptions.BottomLeft,
                    16f,
                    GreyBlue);
                AlignNamedText(
                    root.transform,
                    "LocationValueText",
                    TextAlignmentOptions.BottomLeft,
                    24f,
                    ColdWhite);
                AlignNamedText(
                    root.transform,
                    "LocationGlowText",
                    TextAlignmentOptions.BottomLeft,
                    24f,
                    LowSaturationBlue);

                RectTransform interaction =
                    FindRect(root.transform, "PlayerInteractionPrompt");
                SetBottomCenter(interaction, 0f, 118f, 600f, 64f);
                ApplyFrame(interaction, PromptFramePath, GreyBlue, true);
                AlignNamedText(
                    root.transform,
                    "InteractionText",
                    TextAlignmentOptions.Center,
                    22f,
                    ColdWhite);

                SetBottomRight(
                    FindRect(root.transform, "V2_InitialTutorialRoot"),
                    64f,
                    48f,
                    720f,
                    96f);

                ApplyPhotoLayout(root.transform);
                ApplyDecisionLayout(root.transform);
                ApplyKeycapSizing(root.transform);
            });
    }

    [MenuItem("Tools/Hearth/UI V2/Closure/Apply Companion Prefab Closure")]
    public static void ApplyCompanion()
    {
        EditPrefab(
            CompanionPrefab,
            root =>
            {
                Transform identity = FindNamed(root.transform, "V2_Identity");
                Transform visualParent =
                    identity != null ? identity.parent : root.transform;

                RectTransform frame =
                    FindRect(root.transform, "CompanionRobotFrame");
                if (frame != null)
                {
                    SetStretch(frame, 20f, 20f, 20f, 20f);
                    Image image = frame.GetComponent<Image>();
                    if (image != null)
                    {
                        image.sprite =
                            AssetDatabase.LoadAssetAtPath<Sprite>(
                                CompanionFramePath);
                        image.type = Image.Type.Simple;
                        image.preserveAspect = false;
                        image.color =
                            new Color(
                                LowSaturationBlue.r,
                                LowSaturationBlue.g,
                                LowSaturationBlue.b,
                                0.52f);
                        image.raycastTarget = false;
                    }
                    frame.SetAsFirstSibling();
                }

                TMP_Text identityText =
                    FindText(root.transform, "V2_Identity");
                if (identityText != null)
                {
                    SetTopLeft(identityText.rectTransform, 64f, 54f, 460f, 78f);
                    ConfigureText(
                        identityText,
                        22f,
                        TextAlignmentOptions.TopLeft,
                        ColdWhite,
                        FontStyles.Normal);
                }

                TMP_Text rec = CreateOrGetText(
                    visualParent,
                    "V2_REC",
                    "●  REC");
                SetTopCenter(rec.rectTransform, 0f, 48f, 220f, 40f);
                ConfigureText(
                    rec,
                    22f,
                    TextAlignmentOptions.Top,
                    Red,
                    FontStyles.Bold);

                TMP_Text task = CreateOrGetText(
                    visualParent,
                    "V2_CurrentTask",
                    "CURRENT TASK\nREVIEW RECORDED HOUSEHOLD EVENT");
                SetTopRight(task.rectTransform, 64f, 54f, 520f, 92f);
                ConfigureText(
                    task,
                    20f,
                    TextAlignmentOptions.TopRight,
                    ColdWhite,
                    FontStyles.Normal);

                SetTopLeft(
                    FindRect(root.transform, "V2_StatusPanel"),
                    64f,
                    294f,
                    520f,
                    250f);
                SetTopRight(
                    FindRect(root.transform, "DecisionPanel"),
                    64f,
                    206f,
                    520f,
                    220f);
                SetTopCenter(
                    FindRect(root.transform, "CenterMessageText"),
                    0f,
                    432f,
                    760f,
                    88f);
                SetBottomCenter(
                    FindRect(root.transform, "ModeLabelText"),
                    0f,
                    42f,
                    760f,
                    28f);

                SetNamedActive(root.transform, "DataStreamView", false);
                SetNamedActive(root.transform, "V2_PhysicalFeedLabel", false);
                SetNamedActive(root.transform, "V2_PhysicalFeedRule", false);
                SetNamedActive(root.transform, "V2_InspectionHeading", false);
                SetNamedActive(root.transform, "V2_InspectionHeadingRule", false);
                SetNamedActive(root.transform, "V2_InspectionUnit", false);
                SetNamedActive(root.transform, "V2_InspectionReturn", false);

                HearthCompanionDataStreamView stream =
                    root.GetComponentInChildren<HearthCompanionDataStreamView>(
                        true);
                if (stream != null)
                {
                    stream.gameObject.SetActive(false);
                }
            });
    }

    private static void ApplyTerminal(
        string prefabPath,
        HearthTerminalMode mode)
    {
        EditPrefab(
            prefabPath,
            root =>
            {
                HearthTvTerminalController terminal =
                    root.GetComponent<HearthTvTerminalController>() ??
                    root.GetComponentInChildren<HearthTvTerminalController>(
                        true);
                if (terminal == null)
                {
                    Debug.LogWarning(
                        "[HearthUiV2ClosureEditor] Terminal controller missing: " +
                        prefabPath);
                    return;
                }

                terminal.SetTerminalMode(mode);

                RectTransform keyboardRoot =
                    FindRect(root.transform, "KeyboardNavigationRoot");
                if (mode == HearthTerminalMode.LobbySync)
                {
                    if (keyboardRoot != null)
                    {
                        SetBottomLeft(
                            keyboardRoot,
                            76f,
                            48f,
                            1768f,
                            58f);
                    }

                    TMP_Text lobbyHint =
                        FindText(root.transform, "KeyboardHintText");
                    if (lobbyHint != null)
                    {
                        lobbyHint.text = "SPACE  CLOSE TERMINAL";
                        ConfigureText(
                            lobbyHint,
                            20f,
                            TextAlignmentOptions.Center,
                            GreyBlue,
                            FontStyles.Normal);
                    }

                    TMP_Text lobbyFocus =
                        FindText(root.transform, "KeyboardFocusText");
                    if (lobbyFocus != null)
                    {
                        lobbyFocus.text = string.Empty;
                    }
                    return;
                }

                DisableLegacyTerminalChrome(root.transform);
                BuildCompactTerminalChrome(root, terminal, mode);
            });
    }

    private static void BuildCompactTerminalChrome(
        GameObject prefabRoot,
        HearthTvTerminalController terminal,
        HearthTerminalMode mode)
    {
        Transform previous =
            FindNamed(prefabRoot.transform, "V2_ClosureTerminalChrome");
        if (previous != null)
        {
            UnityEngine.Object.DestroyImmediate(previous.gameObject);
        }

        GameObject chrome = new GameObject(
            "V2_ClosureTerminalChrome",
            typeof(RectTransform));
        chrome.layer = prefabRoot.layer;
        chrome.transform.SetParent(prefabRoot.transform, false);
        RectTransform chromeRect = chrome.GetComponent<RectTransform>();
        SetStretch(chromeRect, 0f, 0f, 0f, 0f);
        chrome.transform.SetAsLastSibling();

        TMP_Text terminalLabel =
            CreateOrGetText(chrome.transform, "TerminalLabel", "DOORWAY TERMINAL");
        SetTopLeft(terminalLabel.rectTransform, 76f, 48f, 420f, 32f);
        ConfigureText(
            terminalLabel,
            19f,
            TextAlignmentOptions.TopLeft,
            GreyBlue,
            FontStyles.Bold);

        TMP_Text residentLabel =
            CreateOrGetText(chrome.transform, "ResidentId", "17F-01");
        SetTopLeft(residentLabel.rectTransform, 76f, 80f, 300f, 44f);
        ConfigureText(
            residentLabel,
            28f,
            TextAlignmentOptions.TopLeft,
            ColdWhite,
            FontStyles.Normal);

        TMP_Text status =
            CreateOrGetText(chrome.transform, "Status", string.Empty);
        SetTopRight(status.rectTransform, 76f, 52f, 400f, 40f);
        ConfigureText(
            status,
            18f,
            TextAlignmentOptions.TopRight,
            GreyBlue,
            FontStyles.Bold);

        Image before = CreateTab(
            chrome.transform,
            "BeforeTab",
            310f,
            124f,
            310f,
            52f);
        TMP_Text beforeText = CreateTabText(before.transform, "Label");
        beforeText.text = "BEFORE ACQUISITION";

        Image after = CreateTab(
            chrome.transform,
            "AfterTab",
            640f,
            124f,
            310f,
            52f);
        TMP_Text afterText = CreateTabText(after.transform, "Label");
        afterText.text = "AFTER ACQUISITION";

        Image primary = CreateTab(
            chrome.transform,
            "PrimaryActionTab",
            1274f,
            124f,
            570f,
            52f);
        TMP_Text primaryText = CreateTabText(primary.transform, "Label");
        primaryText.text =
            mode == HearthTerminalMode.Home ? "ENTER HOME" : "PRIMARY ACTION";

        Image rule = CreateImage(chrome.transform, "HeaderRule", LowSaturationBlue);
        SetTopLeft(rule.rectTransform, 76f, 194f, 1768f, 2f);

        TMP_Text footer =
            CreateOrGetText(
                chrome.transform,
                "Footer",
                "LEFT / RIGHT  SELECT     SPACE  CONFIRM     ESC  EXIT");
        SetBottomRight(footer.rectTransform, 76f, 42f, 900f, 36f);
        ConfigureText(
            footer,
            18f,
            TextAlignmentOptions.BottomRight,
            GreyBlue,
            FontStyles.Normal);

        HearthTerminalCompactChromeView view =
            prefabRoot.GetComponent<HearthTerminalCompactChromeView>();
        if (view == null)
        {
            view =
                prefabRoot.AddComponent<HearthTerminalCompactChromeView>();
        }

        view.Configure(
            terminal,
            chrome,
            terminalLabel,
            residentLabel,
            beforeText,
            afterText,
            primaryText,
            status,
            footer,
            before,
            after,
            primary);
    }

    private static Image CreateTab(
        Transform parent,
        string name,
        float x,
        float y,
        float width,
        float height)
    {
        Image image = CreateImage(
            parent,
            name,
            new Color(0.09f, 0.14f, 0.21f, 0.9f));
        Sprite sprite =
            AssetDatabase.LoadAssetAtPath<Sprite>(ButtonFramePath);
        if (sprite != null)
        {
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
        }
        SetTopLeft(image.rectTransform, x, y, width, height);
        return image;
    }

    private static TMP_Text CreateTabText(Transform parent, string name)
    {
        TMP_Text text = CreateOrGetText(parent, name, string.Empty);
        SetStretch(text.rectTransform, 18f, 10f, 18f, 10f);
        ConfigureText(
            text,
            19f,
            TextAlignmentOptions.Center,
            GreyBlue,
            FontStyles.Normal);
        return text;
    }

    private static void DisableLegacyTerminalChrome(Transform root)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform current = transforms[i];
            string name = current.name;
            if (name == "KeyboardNavigationRoot" ||
                name == "V2_FooterRule" ||
                name == "TerminalLabel" ||
                name == "ResidentId" ||
                name.StartsWith("Tab_", StringComparison.Ordinal) ||
                name == "NavigationRule")
            {
                current.gameObject.SetActive(false);
            }
        }
    }

    private static void ApplyPersistentHumanLayout(Transform persistent)
    {
        TMP_Text[] texts = persistent.GetComponentsInChildren<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text text = texts[i];
            string value = (text.text ?? string.Empty).Trim();
            if (value == "COMPANION UNIT · ACTIVE")
            {
                SetTopLeft(text.rectTransform, 64f, 48f, 448f, 28f);
                ConfigureText(
                    text,
                    17f,
                    TextAlignmentOptions.TopLeft,
                    GreyBlue,
                    FontStyles.Normal);
            }
            else if (value.StartsWith("MIA ·", StringComparison.Ordinal))
            {
                SetTopLeft(text.rectTransform, 64f, 78f, 448f, 38f);
                ConfigureText(
                    text,
                    27f,
                    TextAlignmentOptions.TopLeft,
                    ColdWhite,
                    FontStyles.Normal);
            }
            else if (value == "CURRENT TASK")
            {
                SetTopRight(text.rectTransform, 64f, 48f, 448f, 28f);
                ConfigureText(
                    text,
                    17f,
                    TextAlignmentOptions.TopRight,
                    GreyBlue,
                    FontStyles.Normal);
            }
            else if (value.Contains("NIGHT ROUNDS") ||
                     value.Contains("TONIGHT'S ROUNDS"))
            {
                SetTopRight(text.rectTransform, 64f, 80f, 560f, 54f);
                ConfigureText(
                    text,
                    22f,
                    TextAlignmentOptions.TopRight,
                    ColdWhite,
                    FontStyles.Normal);
            }
        }
    }

    private static void ApplyPhotoLayout(Transform root)
    {
        SetNamedRectsTopLeft(root, "V2_PhotoArchiveHeading", 240f, 82f, 800f, 48f);
        SetNamedRectsTopLeft(root, "V2_PhotoViewport", 240f, 156f, 1440f, 640f);
        SetNamedRectsTopLeft(root, "V2_PhotoMetadata", 240f, 806f, 520f, 84f);
        SetNamedRectsTopLeft(root, "V2_PhotoFieldUnit", 432f, 834f, 1056f, 132f);
        SetNamedRectsBottomLeft(root, "V2_PhotoPage", 240f, 52f, 260f, 30f);
        SetNamedRectsBottomRight(root, "V2_PhotoReturnHint", 240f, 52f, 520f, 30f);

        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].name == "V2_PhotoViewport")
            {
                ApplyFrame(
                    transforms[i] as RectTransform,
                    "Assets/UI/HEARTH/GeneratedParts/Finale/HUD_Finale_PhotoFrame_9Slice.png",
                    GreyBlue,
                    true);
            }
        }
    }

    private static void ApplyDecisionLayout(Transform root)
    {
        SetNamedRectsTopLeft(root, "V2_FinalChoiceHeading", 432f, 232f, 1056f, 52f);
        SetNamedRectsTopLeft(root, "FinalChoiceTarget_A", 432f, 350f, 1056f, 112f);
        SetNamedRectsTopLeft(root, "FinalChoiceTarget_B", 432f, 486f, 1056f, 112f);
        SetNamedRectsBottomCenter(root, "V2_FinalChoiceHint", 0f, 78f, 900f, 42f);
    }

    private static void ApplyKeycapSizing(Transform root)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            RectTransform rect = transforms[i] as RectTransform;
            if (rect == null ||
                (rect.name != "Keycap" && rect.name != "Key"))
            {
                continue;
            }

            TMP_Text label = rect.GetComponentInChildren<TMP_Text>(true);
            bool space =
                label != null &&
                (label.text ?? string.Empty).IndexOf(
                    "SPACE",
                    StringComparison.OrdinalIgnoreCase) >= 0;
            rect.sizeDelta = new Vector2(space ? 96f : 64f, 40f);
        }
    }

    private static void EditPrefab(
        string path,
        Action<GameObject> edit)
    {
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null)
        {
            Debug.LogWarning(
                "[HearthUiV2ClosureEditor] Prefab missing: " + path);
            return;
        }

        GameObject contents = PrefabUtility.LoadPrefabContents(path);
        try
        {
            edit(contents);
            PrefabUtility.SaveAsPrefabAsset(contents, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(contents);
        }
    }

    private static Transform FindNamed(Transform root, string name)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].name == name)
            {
                return transforms[i];
            }
        }
        return null;
    }

    private static RectTransform FindRect(Transform root, string name)
    {
        return FindNamed(root, name) as RectTransform;
    }

    private static TMP_Text FindText(Transform root, string name)
    {
        Transform found = FindNamed(root, name);
        return found != null ? found.GetComponent<TMP_Text>() : null;
    }

    private static TMP_Text CreateOrGetText(
        Transform parent,
        string name,
        string value)
    {
        Transform existing = parent.Find(name);
        TMP_Text text =
            existing != null ? existing.GetComponent<TMP_Text>() : null;
        if (text == null)
        {
            GameObject textObject =
                new GameObject(name, typeof(RectTransform));
            textObject.layer = parent.gameObject.layer;
            textObject.transform.SetParent(parent, false);
            text = textObject.AddComponent<TextMeshProUGUI>();
            if (TMP_Settings.defaultFontAsset != null)
            {
                text.font = TMP_Settings.defaultFontAsset;
            }
        }

        text.text = value;
        return text;
    }

    private static Image CreateImage(
        Transform parent,
        string name,
        Color color)
    {
        GameObject imageObject = new GameObject(
            name,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        imageObject.layer = parent.gameObject.layer;
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static void ConfigureText(
        TMP_Text text,
        float size,
        TextAlignmentOptions alignment,
        Color color,
        FontStyles style)
    {
        if (text == null)
        {
            return;
        }

        text.fontSize = size;
        text.fontSizeMin = size;
        text.fontSizeMax = size;
        text.enableAutoSizing = false;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Overflow;
        text.alignment = alignment;
        text.color = color;
        text.fontStyle = style;
        text.raycastTarget = false;
    }

    private static void AlignNamedText(
        Transform root,
        string name,
        TextAlignmentOptions alignment,
        float size,
        Color color)
    {
        TMP_Text text = FindText(root, name);
        ConfigureText(
            text,
            size,
            alignment,
            color,
            text != null ? text.fontStyle : FontStyles.Normal);
    }

    private static void ApplyFrame(
        RectTransform rect,
        string spritePath,
        Color color,
        bool sliced)
    {
        if (rect == null)
        {
            return;
        }

        Image image = rect.GetComponent<Image>();
        if (image == null)
        {
            return;
        }

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (sprite != null)
        {
            image.sprite = sprite;
            image.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        }
        image.color = color;
        image.raycastTarget = false;
    }

    private static void SetNamedActive(
        Transform root,
        string name,
        bool active)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].name == name)
            {
                transforms[i].gameObject.SetActive(active);
            }
        }
    }

    private static void SetNamedRectsTopLeft(
        Transform root,
        string name,
        float x,
        float y,
        float width,
        float height)
    {
        SetNamedRects(
            root,
            name,
            rect => SetTopLeft(rect, x, y, width, height));
    }

    private static void SetNamedRectsBottomLeft(
        Transform root,
        string name,
        float x,
        float y,
        float width,
        float height)
    {
        SetNamedRects(
            root,
            name,
            rect => SetBottomLeft(rect, x, y, width, height));
    }

    private static void SetNamedRectsBottomRight(
        Transform root,
        string name,
        float right,
        float bottom,
        float width,
        float height)
    {
        SetNamedRects(
            root,
            name,
            rect => SetBottomRight(rect, right, bottom, width, height));
    }

    private static void SetNamedRectsBottomCenter(
        Transform root,
        string name,
        float x,
        float bottom,
        float width,
        float height)
    {
        SetNamedRects(
            root,
            name,
            rect => SetBottomCenter(rect, x, bottom, width, height));
    }

    private static void SetNamedRects(
        Transform root,
        string name,
        Action<RectTransform> set)
    {
        Transform[] transforms = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            RectTransform rect = transforms[i] as RectTransform;
            if (rect != null && rect.name == name)
            {
                set(rect);
            }
        }
    }

    private static void SetTopLeft(
        RectTransform rect,
        float x,
        float y,
        float width,
        float height)
    {
        if (rect == null) return;
        rect.anchorMin = Vector2.up;
        rect.anchorMax = Vector2.up;
        rect.pivot = Vector2.up;
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void SetTopRight(
        RectTransform rect,
        float right,
        float y,
        float width,
        float height)
    {
        if (rect == null) return;
        rect.anchorMin = Vector2.one;
        rect.anchorMax = Vector2.one;
        rect.pivot = Vector2.one;
        rect.anchoredPosition = new Vector2(-right, -y);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void SetTopCenter(
        RectTransform rect,
        float x,
        float y,
        float width,
        float height)
    {
        if (rect == null) return;
        rect.anchorMin = new Vector2(0.5f, 1f);
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 1f);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void SetBottomLeft(
        RectTransform rect,
        float x,
        float y,
        float width,
        float height)
    {
        if (rect == null) return;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = Vector2.zero;
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void SetBottomRight(
        RectTransform rect,
        float right,
        float bottom,
        float width,
        float height)
    {
        if (rect == null) return;
        rect.anchorMin = Vector2.right;
        rect.anchorMax = Vector2.right;
        rect.pivot = Vector2.right;
        rect.anchoredPosition = new Vector2(-right, bottom);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void SetBottomCenter(
        RectTransform rect,
        float x,
        float bottom,
        float width,
        float height)
    {
        if (rect == null) return;
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(x, bottom);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void SetStretch(
        RectTransform rect,
        float left,
        float top,
        float right,
        float bottom)
    {
        if (rect == null) return;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = new Vector2(left, bottom);
        rect.offsetMax = new Vector2(-right, -top);
    }
}
#endif
