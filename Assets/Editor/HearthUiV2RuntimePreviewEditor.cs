#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class HearthUiV2RuntimePreviewEditor
{
    private const string MenuRoot = "Tools/Hearth/UI V2/Runtime Preview/";
    private const string PhotoPreviewTexturePath =
        "Assets/Art/UI/HearthHud/Finale/FamilyPhoto.png";

    private const string FormalCapacitySample =
        "The household record shows a repeated conflict between scheduled care, " +
        "work demands, and the child's need for reassurance. I need the complete " +
        "context before I make a disposition.";

    [MenuItem(MenuRoot + "Human/01 Persistent HUD")]
    public static void ShowHumanPersistent()
    {
        PrepareHuman();
    }

    [MenuItem(MenuRoot + "Human/02 Field Unit Auxiliary")]
    public static void ShowFieldUnitAuxiliary()
    {
        PrepareHuman();
        MinLoopSubtitlePlayer player = FindSubtitlePlayer();
        if (player == null)
        {
            PreviewError("No MinLoopSubtitlePlayer is available.");
            return;
        }

        player.PlaySequence(
            new List<MinLoopSubtitleLine>
            {
                new MinLoopSubtitleLine
                {
                    speaker = "FIELD UNIT",
                    text =
                        "Good evening, Inspector. Field Companion Unit online. " +
                        "I'll be your partner tonight.",
                    dialogueMode = HearthDialogueLineMode.Auxiliary,
                    speakerSide = SpeakerSide.Right,
                    advancePolicy =
                        HearthDialogueLineAdvancePolicy.ManualSpace,
                    holdSeconds = 999f
                }
            },
            HearthSubtitleContext.FieldUnit);
        QueuePreviewRepaint();
    }

    [MenuItem(MenuRoot + "Human/03 Formal Dialogue Left")]
    public static void ShowFormalLeft()
    {
        ShowFormal(
            "CLAIRE · RESIDENT",
            FormalCapacitySample,
            SpeakerSide.Left);
    }

    [MenuItem(MenuRoot + "Human/04 Formal Dialogue Right")]
    public static void ShowFormalRight()
    {
        ShowFormal(
            "MIA · INSPECTOR",
            FormalCapacitySample,
            SpeakerSide.Right);
    }

    [MenuItem(MenuRoot + "Human/05 Tab Menu")]
    public static void ShowHumanMenu()
    {
        PrepareHuman();
        ShowHumanPage(HearthFirstPersonHudPageId.Slide03MainMenu);
    }

    [MenuItem(MenuRoot + "Human/06 Photo Archive")]
    public static void ShowPhotoArchive()
    {
        PrepareHuman();
        ShowHumanPage(HearthFirstPersonHudPageId.Slide07Photo2023);
    }

    [MenuItem(MenuRoot + "Human/07 Disposition Choice")]
    public static void ShowDispositionChoice()
    {
        PrepareHuman();
        ShowHumanPage(HearthFirstPersonHudPageId.Slide09FinalChoice);
    }

    [MenuItem(MenuRoot + "Human/08 Shutdown Confirm")]
    public static void ShowShutdownConfirm()
    {
        PrepareHuman();
        ShowHumanPage(HearthFirstPersonHudPageId.Slide10ShutdownConfirm);
    }

    [MenuItem(MenuRoot + "Human/09 Low Trust Takeover")]
    public static void ShowLowTrustTakeover()
    {
        PrepareHuman();
        ShowHumanPage(HearthFirstPersonHudPageId.Slide11Warning01);
    }

    [MenuItem(MenuRoot + "Companion/01 17F01")]
    public static void ShowCompanion17F01()
    {
        ShowCompanionScene(1);
    }

    [MenuItem(MenuRoot + "Companion/02 17F02")]
    public static void ShowCompanion17F02()
    {
        ShowCompanionScene(4);
    }

    [MenuItem(MenuRoot + "Companion/03 17F03")]
    public static void ShowCompanion17F03()
    {
        ShowCompanionScene(10);
    }

    [MenuItem(MenuRoot + "Companion/04 Decision Card")]
    public static void ShowCompanionDecisionCard()
    {
        HearthCompanionHudController controller = ShowCompanionScene(1);
        if (controller == null || controller.CurrentScene == null)
        {
            return;
        }

        controller.StopAllCoroutines();
        HearthCompanionDecisionPanelView panel =
            controller.GetComponentInChildren<HearthCompanionDecisionPanelView>(
                true);
        if (panel != null)
        {
            panel.Apply(controller.CurrentScene);
            CanvasGroup group = panel.GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.alpha = 1f;
                group.interactable = false;
                group.blocksRaycasts = false;
            }
        }
        QueuePreviewRepaint();
    }

    [MenuItem(MenuRoot + "Companion/05 Formal Dialogue")]
    public static void ShowCompanionFormalDialogue()
    {
        HearthCompanionHudController controller = ShowCompanionScene(1);
        if (controller == null)
        {
            return;
        }

        controller.StopAllCoroutines();
        controller.enabled = false;
        HearthCompanionDecisionPanelView panel =
            controller.GetComponentInChildren<HearthCompanionDecisionPanelView>(
                true);
        if (panel != null)
        {
            panel.gameObject.SetActive(false);
            CanvasGroup group = panel.GetComponent<CanvasGroup>();
            if (group != null)
            {
                group.alpha = 0f;
                group.interactable = false;
                group.blocksRaycasts = false;
            }
        }
        Transform triggerCard =
            FindNamed(controller.transform, "TriggerCardView");
        if (triggerCard != null)
        {
            triggerCard.gameObject.SetActive(false);
        }
        Transform holdPrompt = FindNamed(controller.transform, "HoldPrompt");
        if (holdPrompt != null)
        {
            holdPrompt.gameObject.SetActive(false);
        }

        MinLoopSubtitlePlayer player = FindSubtitlePlayer();
        if (player == null)
        {
            PreviewError("No MinLoopSubtitlePlayer is available.");
            return;
        }

        player.PlaySequence(
            new List<MinLoopSubtitleLine>
            {
                new MinLoopSubtitleLine
                {
                    speaker = "DANIEL · RESIDENT",
                    text = FormalCapacitySample,
                    dialogueMode = HearthDialogueLineMode.Formal,
                    speakerSide = SpeakerSide.Left,
                    advancePolicy =
                        HearthDialogueLineAdvancePolicy.ManualSpace,
                    holdSeconds = 999f
                }
            },
            HearthSubtitleContext.Human);
        QueuePreviewRepaint();
    }

    [MenuItem(MenuRoot + "Companion/06 Permission Boundary")]
    public static void ShowCompanionPermissionBoundary()
    {
        HearthCompanionHudController controller = ShowCompanionScene(3);
        if (controller == null)
        {
            return;
        }

        controller.StopAllCoroutines();
        Transform center = FindNamed(controller.transform, "CenterMessageText");
        TMP_Text centerText =
            center != null ? center.GetComponent<TMP_Text>() : null;
        if (centerText != null)
        {
            centerText.text = "—  ACTIVITY PERMISSION BOUNDARY  —";
            centerText.gameObject.SetActive(true);
        }
        QueuePreviewRepaint();
    }

    [MenuItem(MenuRoot + "Terminal/01 Lobby Synchronization")]
    public static void ShowLobbyTerminal()
    {
        ShowTerminal(HearthTerminalMode.LobbySync, string.Empty);
    }

    [MenuItem(MenuRoot + "Terminal/02 Doorway 17F01")]
    public static void ShowDoorway17F01()
    {
        ShowTerminal(HearthTerminalMode.Doorway, "17F01");
    }

    [MenuItem(MenuRoot + "Terminal/03 Doorway 17F02")]
    public static void ShowDoorway17F02()
    {
        ShowTerminal(HearthTerminalMode.Doorway, "17F02");
    }

    [MenuItem(MenuRoot + "Terminal/04 Doorway 17F03")]
    public static void ShowDoorway17F03()
    {
        ShowTerminal(HearthTerminalMode.Doorway, "17F03");
    }

    [MenuItem(MenuRoot + "Terminal/05 Home 17F04")]
    public static void ShowHomeTerminal()
    {
        ShowTerminal(HearthTerminalMode.Home, "17F04");
    }

    [MenuItem(MenuRoot + "Stop Preview")]
    public static void StopPreview()
    {
        StopSubtitles();
        CloseTerminals();

        HearthCompanionHudController[] companionHuds =
            UnityEngine.Object.FindObjectsOfType<HearthCompanionHudController>(
                true);
        for (int i = 0; i < companionHuds.Length; i++)
        {
            companionHuds[i].SetVisible(false);
        }

        PrepareHuman();
    }

    [MenuItem(MenuRoot + "QA/Audit Active Regions")]
    public static void AuditActiveRegions()
    {
        HashSet<string> auditedNames = new HashSet<string>
        {
            "LocationHud",
            "PlayerInteractionPrompt",
            "V2_InitialTutorialRoot",
            "FormalFrame",
            "AuxiliaryFrame",
            "Button_TODAY_ROUNDS",
            "Button_DISPOSITION_HISTORY",
            "Button_SYSTEM_SETTINGS",
            "Button_ANSWER_LILY",
            "Button_COMPANION_ANSWER",
            "Button_CloseStory",
            "V2_PhotoViewport",
            "V2_PhotoFieldUnitFrame",
            "V2_ShutdownModalFrame",
            "V2_WarningModalFrame",
            "V2_Identity",
            "V2_CurrentTask",
            "V2_StatusPanel",
            "DecisionPanel",
            "CenterMessageText",
            "BeforeTab",
            "AfterTab",
            "PrimaryActionTab",
            "HouseholdIntroduction",
            "FieldUnitPanel",
            "Portrait_SON",
            "Portrait_DAD",
            "Portrait_MOM",
            "Portrait_WIFE",
            "Portrait_HUSBAND",
            "Portrait_UNIT",
            "Portrait_DAUGHTER",
            "Portrait_MOTHER",
            "Portrait_FATHER"
        };

        RectTransform[] all =
            UnityEngine.Object.FindObjectsOfType<RectTransform>(true);
        List<AuditedRegion> regions = new List<AuditedRegion>();
        for (int i = 0; i < all.Length; i++)
        {
            RectTransform rect = all[i];
            if (rect == null ||
                !auditedNames.Contains(rect.name) ||
                !IsVisuallyActive(rect))
            {
                continue;
            }

            Rect screenRect = GetScreenRect(rect);
            if (screenRect.width > 1f && screenRect.height > 1f)
            {
                regions.Add(new AuditedRegion(rect, screenRect));
            }
        }

        List<string> issues = new List<string>();
        for (int i = 0; i < regions.Count; i++)
        {
            for (int j = i + 1; j < regions.Count; j++)
            {
                AuditedRegion a = regions[i];
                AuditedRegion b = regions[j];
                if (a.Transform.IsChildOf(b.Transform) ||
                    b.Transform.IsChildOf(a.Transform))
                {
                    continue;
                }

                Rect intersection = Intersect(a.ScreenRect, b.ScreenRect);
                float area = intersection.width * intersection.height;
                float smaller =
                    Mathf.Min(
                        a.ScreenRect.width * a.ScreenRect.height,
                        b.ScreenRect.width * b.ScreenRect.height);
                if (area > 64f && smaller > 0f && area / smaller > 0.08f)
                {
                    issues.Add(
                        a.Transform.name +
                        " overlaps " +
                        b.Transform.name +
                        " at " +
                        intersection);
                }
            }
        }

        if (issues.Count == 0)
        {
            Debug.Log(
                "[HearthUiV2RuntimePreviewEditor] Active layout overlap audit " +
                "passed (" +
                regions.Count +
                " visible regions).");
        }
        else
        {
            Debug.LogWarning(
                "[HearthUiV2RuntimePreviewEditor] Active layout overlap audit " +
                "found " +
                issues.Count +
                " issue(s): " +
                string.Join(" ; ", issues));
        }
    }

    [MenuItem(MenuRoot + "Human/01 Persistent HUD", true)]
    [MenuItem(MenuRoot + "Human/02 Field Unit Auxiliary", true)]
    [MenuItem(MenuRoot + "Human/03 Formal Dialogue Left", true)]
    [MenuItem(MenuRoot + "Human/04 Formal Dialogue Right", true)]
    [MenuItem(MenuRoot + "Human/05 Tab Menu", true)]
    [MenuItem(MenuRoot + "Human/06 Photo Archive", true)]
    [MenuItem(MenuRoot + "Human/07 Disposition Choice", true)]
    [MenuItem(MenuRoot + "Human/08 Shutdown Confirm", true)]
    [MenuItem(MenuRoot + "Human/09 Low Trust Takeover", true)]
    [MenuItem(MenuRoot + "Companion/01 17F01", true)]
    [MenuItem(MenuRoot + "Companion/02 17F02", true)]
    [MenuItem(MenuRoot + "Companion/03 17F03", true)]
    [MenuItem(MenuRoot + "Companion/04 Decision Card", true)]
    [MenuItem(MenuRoot + "Companion/05 Formal Dialogue", true)]
    [MenuItem(MenuRoot + "Companion/06 Permission Boundary", true)]
    [MenuItem(MenuRoot + "Terminal/01 Lobby Synchronization", true)]
    [MenuItem(MenuRoot + "Terminal/02 Doorway 17F01", true)]
    [MenuItem(MenuRoot + "Terminal/03 Doorway 17F02", true)]
    [MenuItem(MenuRoot + "Terminal/04 Doorway 17F03", true)]
    [MenuItem(MenuRoot + "Terminal/05 Home 17F04", true)]
    [MenuItem(MenuRoot + "QA/Audit Active Regions", true)]
    [MenuItem(MenuRoot + "Stop Preview", true)]
    private static bool ValidateRuntimePreview()
    {
        return EditorApplication.isPlaying;
    }

    private static void ShowFormal(
        string speaker,
        string text,
        SpeakerSide side)
    {
        PrepareHuman();
        MinLoopSubtitlePlayer player = FindSubtitlePlayer();
        if (player == null)
        {
            PreviewError("No MinLoopSubtitlePlayer is available.");
            return;
        }

        player.PlaySequence(
            new List<MinLoopSubtitleLine>
            {
                new MinLoopSubtitleLine
                {
                    speaker = speaker,
                    text = text,
                    dialogueMode = HearthDialogueLineMode.Formal,
                    speakerSide = side,
                    advancePolicy =
                        HearthDialogueLineAdvancePolicy.ManualSpace,
                    holdSeconds = 999f
                }
            },
            HearthSubtitleContext.Human);
        QueuePreviewRepaint();
    }

    private static void ShowHumanPage(HearthFirstPersonHudPageId page)
    {
        HearthFirstPersonHudController controller =
            UnityEngine.Object.FindObjectOfType<HearthFirstPersonHudController>(
                true);
        if (controller == null)
        {
            PreviewError("No HearthFirstPersonHudController is available.");
            return;
        }
        controller.ShowPage(page);
        if (page == HearthFirstPersonHudPageId.Slide07Photo2023 ||
            page == HearthFirstPersonHudPageId.Slide08Photo2026)
        {
            InjectPhotoPreview(controller.transform);
        }
        QueuePreviewRepaint();
    }

    private static HearthCompanionHudController ShowCompanionScene(
        int slideNumber)
    {
        BeginPreviewRendering();
        SuspendAutomaticPreviewDrivers();
        StopSubtitles();
        CloseTerminals();

        ViewSwitchController switcher =
            ViewSwitchController.FindPreferredController();
        if (switcher != null)
        {
            switcher.SwitchTo(ViewSwitchController.ViewMode.Companion);
        }

        HearthCompanionHudController controller =
            UnityEngine.Object.FindObjectOfType<HearthCompanionHudController>(
                true);
        if (controller == null)
        {
            PreviewError("No HearthCompanionHudController is available.");
            return null;
        }

        controller.enabled = true;
        Transform decisionPanel =
            FindNamed(controller.transform, "DecisionPanel");
        if (decisionPanel != null)
        {
            decisionPanel.gameObject.SetActive(true);
        }
        Transform triggerCard =
            FindNamed(controller.transform, "TriggerCardView");
        if (triggerCard != null)
        {
            triggerCard.gameObject.SetActive(true);
        }
        Transform holdPrompt = FindNamed(controller.transform, "HoldPrompt");
        if (holdPrompt != null)
        {
            holdPrompt.gameObject.SetActive(true);
        }

        controller.SetVisible(true);
        controller.ShowScene(slideNumber);
        QueuePreviewRepaint();
        return controller;
    }

    private static void InjectPhotoPreview(Transform root)
    {
        Transform viewport = FindNamed(root, "V2_PhotoViewport");
        if (viewport == null)
        {
            PreviewError("Photo archive viewport is missing.");
            return;
        }

        Transform existing = viewport.Find("PhotoPreviewSample_V2");
        RawImage image;
        if (existing != null)
        {
            image = existing.GetComponent<RawImage>();
        }
        else
        {
            GameObject imageObject =
                new GameObject(
                    "PhotoPreviewSample_V2",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(RawImage));
            imageObject.transform.SetParent(viewport, false);
            image = imageObject.GetComponent<RawImage>();
        }

        RectTransform imageRect = image.rectTransform;
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.pivot = new Vector2(0.5f, 0.5f);
        imageRect.offsetMin = new Vector2(8f, 8f);
        imageRect.offsetMax = new Vector2(-8f, -8f);
        image.texture =
            AssetDatabase.LoadAssetAtPath<Texture>(PhotoPreviewTexturePath);
        image.color = Color.white;
        image.raycastTarget = false;
        image.uvRect = new Rect(0f, 0f, 1f, 1f);
        image.transform.SetAsFirstSibling();

        TMPro.TMP_Text page =
            FindNamed(root, "V2_PhotoPage")?.GetComponent<TMPro.TMP_Text>();
        if (page != null)
        {
            page.text = "PAGE 01 / 02";
        }

        TMPro.TMP_Text hint =
            FindNamed(root, "V2_PhotoReturnHint")
                ?.GetComponent<TMPro.TMP_Text>();
        if (hint != null)
        {
            hint.text =
                "LEFT / RIGHT  SWITCH PHOTO     SPACE  RETURN";
        }
    }

    private static void ShowTerminal(
        HearthTerminalMode mode,
        string nameToken)
    {
        PrepareHuman();
        HearthTvTerminalController[] terminals =
            UnityEngine.Object.FindObjectsOfType<HearthTvTerminalController>(
                true);
        HearthTvTerminalController target = null;
        for (int i = 0; i < terminals.Length; i++)
        {
            HearthTvTerminalController terminal = terminals[i];
            if (terminal == null || terminal.TerminalMode != mode)
            {
                continue;
            }

            string normalizedName =
                Normalize(terminal.gameObject.name + " " +
                          terminal.GetReplayResidentId());
            if (nameToken.Length == 0 ||
                normalizedName.Contains(Normalize(nameToken)))
            {
                target = terminal;
                break;
            }
        }

        if (target == null)
        {
            PreviewError(
                "No terminal matched " +
                mode +
                " / " +
                nameToken +
                ".");
            return;
        }

        target.OpenTerminal();
        QueuePreviewRepaint();
    }

    private static void PrepareHuman()
    {
        BeginPreviewRendering();
        SuspendAutomaticPreviewDrivers();
        StopSubtitles();
        CloseTerminals();

        ViewSwitchController switcher =
            ViewSwitchController.FindPreferredController();
        if (switcher != null)
        {
            switcher.SwitchTo(ViewSwitchController.ViewMode.Human);
        }

        HearthCompanionHudController[] companionHuds =
            UnityEngine.Object.FindObjectsOfType<HearthCompanionHudController>(
                true);
        for (int i = 0; i < companionHuds.Length; i++)
        {
            companionHuds[i].SetVisible(false);
        }

        HearthFirstPersonHudController controller =
            UnityEngine.Object.FindObjectOfType<HearthFirstPersonHudController>(
                true);
        if (controller != null)
        {
            controller.ShowPage(
                HearthFirstPersonHudPageId.Slide01PersistentHud);
        }
        QueuePreviewRepaint();
    }

    private static void StopSubtitles()
    {
        MinLoopSubtitlePlayer[] players =
            UnityEngine.Object.FindObjectsOfType<MinLoopSubtitlePlayer>(true);
        for (int i = 0; i < players.Length; i++)
        {
            players[i].Stop();
        }
    }

    private static void SuspendAutomaticPreviewDrivers()
    {
        AudioListener[] listeners =
            UnityEngine.Object.FindObjectsOfType<AudioListener>(true);
        bool keptListener = false;
        for (int i = 0; i < listeners.Length; i++)
        {
            AudioListener listener = listeners[i];
            if (listener == null ||
                !listener.enabled ||
                !listener.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (!keptListener)
            {
                keptListener = true;
                continue;
            }

            listener.enabled = false;
        }

        HearthLobbyFlowController[] lobbyFlows =
            UnityEngine.Object.FindObjectsOfType<HearthLobbyFlowController>(
                true);
        for (int i = 0; i < lobbyFlows.Length; i++)
        {
            HearthLobbyFlowController flow = lobbyFlows[i];
            if (flow != null && flow.gameObject.activeSelf)
            {
                flow.gameObject.SetActive(false);
            }
        }

        string[] blackoutNames =
        {
            "LobbyBlackoutCanvas",
            "Hearth17F02ReplayBlackout",
            "Hearth17F03Blackout",
            "FinaleBlackout_17F04"
        };
        Canvas[] canvases =
            UnityEngine.Object.FindObjectsOfType<Canvas>(true);
        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];
            for (int j = 0; j < blackoutNames.Length; j++)
            {
                if (canvas != null && canvas.name == blackoutNames[j])
                {
                    canvas.gameObject.SetActive(false);
                    break;
                }
            }
        }
    }

    private static void CloseTerminals()
    {
        HearthTvTerminalController[] terminals =
            UnityEngine.Object.FindObjectsOfType<HearthTvTerminalController>(
                true);
        for (int i = 0; i < terminals.Length; i++)
        {
            if (terminals[i] != null && terminals[i].IsOpen)
            {
                terminals[i].CloseTerminalImmediateForPreview();
            }
        }
    }

    private static MinLoopSubtitlePlayer FindSubtitlePlayer()
    {
        MinLoopSubtitlePlayer[] players =
            UnityEngine.Object.FindObjectsOfType<MinLoopSubtitlePlayer>(true);
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null &&
                players[i].gameObject.activeInHierarchy)
            {
                return players[i];
            }
        }
        return players.Length > 0 ? players[0] : null;
    }

    private static string Normalize(string value)
    {
        return (value ?? string.Empty)
            .Replace("-", string.Empty)
            .Replace("_", string.Empty)
            .Replace(" ", string.Empty)
            .ToUpperInvariant();
    }

    private static Transform FindNamed(Transform root, string objectName)
    {
        if (root == null)
        {
            return null;
        }
        if (root.name == objectName)
        {
            return root;
        }
        for (int i = 0; i < root.childCount; i++)
        {
            Transform match = FindNamed(root.GetChild(i), objectName);
            if (match != null)
            {
                return match;
            }
        }
        return null;
    }

    private static void PreviewError(string message)
    {
        Debug.LogError(
            "[HearthUiV2RuntimePreviewEditor] " +
            message);
    }

    private static void BeginPreviewRendering()
    {
        Application.runInBackground = true;
        if (EditorApplication.isPaused)
        {
            EditorApplication.isPaused = false;
        }
    }

    private static void QueuePreviewRepaint()
    {
        Canvas.ForceUpdateCanvases();
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        EditorApplication.delayCall += ForcePreviewRepaint;
    }

    private static bool IsVisuallyActive(RectTransform rect)
    {
        if (!rect.gameObject.activeInHierarchy)
        {
            return false;
        }

        Transform current = rect;
        while (current != null)
        {
            CanvasGroup group = current.GetComponent<CanvasGroup>();
            if (group != null && group.alpha <= 0.01f)
            {
                return false;
            }
            current = current.parent;
        }

        HearthTvTerminalController terminal =
            rect.GetComponentInParent<HearthTvTerminalController>();
        if (terminal != null)
        {
            Transform content = FindNamed(terminal.transform, "TerminalContentRoot");
            CanvasGroup contentGroup =
                content != null ? content.GetComponent<CanvasGroup>() : null;
            if (contentGroup != null && contentGroup.alpha <= 0.01f)
            {
                return false;
            }
        }

        return true;
    }

    private static Rect GetScreenRect(RectTransform rect)
    {
        Canvas canvas = rect.GetComponentInParent<Canvas>();
        Camera camera =
            canvas != null &&
            canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera != null
                    ? canvas.worldCamera
                    : Camera.main
                : null;
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);
        for (int i = 0; i < corners.Length; i++)
        {
            Vector2 point =
                RectTransformUtility.WorldToScreenPoint(camera, corners[i]);
            min = Vector2.Min(min, point);
            max = Vector2.Max(max, point);
        }
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private static Rect Intersect(Rect a, Rect b)
    {
        float xMin = Mathf.Max(a.xMin, b.xMin);
        float yMin = Mathf.Max(a.yMin, b.yMin);
        float xMax = Mathf.Min(a.xMax, b.xMax);
        float yMax = Mathf.Min(a.yMax, b.yMax);
        return xMax <= xMin || yMax <= yMin
            ? Rect.zero
            : Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    private readonly struct AuditedRegion
    {
        public readonly RectTransform Transform;
        public readonly Rect ScreenRect;

        public AuditedRegion(
            RectTransform transform,
            Rect screenRect)
        {
            Transform = transform;
            ScreenRect = screenRect;
        }
    }

    private static void ForcePreviewRepaint()
    {
        Canvas.ForceUpdateCanvases();
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        SceneView.RepaintAll();
    }
}
#endif
