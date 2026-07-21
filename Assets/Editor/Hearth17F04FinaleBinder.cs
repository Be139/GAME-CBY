#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Hearth17F04FinaleBinder
{
    private const string FinaleRootPath = "MIN_LOOP_ROOT/Finale_17F04";
    private const string Tv3Path = "17F/ROOM4/TV (3)";
    private const string Tv4Path = "17F/ROOM4/TV (4)";
    private const string Door1Path = "17F/ROOM4/Door_2_Brown (1)";
    private const string Door7Path = "17F/ROOM2/Door_2_Brown (7)";
    private const string HumanPath = "Player/Person Controller";
    private const string LivingReferencePath = "Player/Person Controller (3)";
    private const string DaughterReferencePath = "Player/Person Controller (2)";
    private const string HomeUnitPath = "GameObject/ROBOT (1)";
    private const string HomeTerminalPrefabPath = "Assets/Prefabs/UI/HearthHud/Terminals/Terminal_17F04_Home.prefab";
    private const string DialogueFolder = "Assets/Data/MinLoop/Dialogues/17F04";
    private const string MaterialFolder = "Assets/materials/Hearth";
    private const string PhotoMaterialPath = MaterialFolder + "/17F04_Photo_Unlit.mat";
    private const string PhotoTexturePath = "Assets/Art/UI/HearthHud/Finale/FamilyPhoto.png";
    private const string SecondPhotoTexturePath = "Assets/Art/UI/HearthHud/Finale/FamilyPhoto_Second.png";

    [MenuItem("Tools/Hearth/Finale/Apply 17F04 Home Finale Setup")]
    public static void ApplySetup()
    {
        if (Application.isPlaying)
        {
            Debug.LogError("[Hearth17F04FinaleBinder] Exit Play Mode before applying the finale setup.");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        Transform human = FindTransform(HumanPath);
        Transform livingReference = FindTransform(LivingReferencePath);
        Transform daughterReference = FindTransform(DaughterReferencePath);
        Transform tv3 = FindTransform(Tv3Path);
        Transform tv4 = FindTransform(Tv4Path);
        Transform door1 = FindTransform(Door1Path);
        Transform homeUnit = FindTransform(HomeUnitPath);

        if (human == null || livingReference == null || daughterReference == null || tv3 == null || tv4 == null || door1 == null || homeUnit == null)
        {
            Debug.LogError("[Hearth17F04FinaleBinder] Missing a required 17F04 object. Run the validation menu for exact details.");
            ValidateSetup();
            return;
        }

        EnsureAssetFolder("Assets/Prefabs");
        EnsureAssetFolder("Assets/Prefabs/UI");
        EnsureAssetFolder("Assets/Prefabs/UI/HearthHud");
        EnsureAssetFolder("Assets/Prefabs/UI/HearthHud/Terminals");
        EnsureAssetFolder("Assets/Data");
        EnsureAssetFolder("Assets/Data/MinLoop");
        EnsureAssetFolder("Assets/Data/MinLoop/Dialogues");
        EnsureAssetFolder(DialogueFolder);
        EnsureAssetFolder("Assets/materials");
        EnsureAssetFolder(MaterialFolder);

        if (!HearthFinalDialogueSync.SyncAllFromFinalScript(false))
        {
            Debug.LogError("[Hearth17F04FinaleBinder] Setup stopped because the final dialogue source could not be synchronized.");
            return;
        }

        BuildHomeTerminalPrefab();

        Transform finaleRoot = EnsureHierarchy(FinaleRootPath);
        Transform anchorRoot = EnsureChild(finaleRoot, "Anchors");
        Transform uiRoot = EnsureChild(finaleRoot, "UI");
        Transform controllerHost = EnsureChild(finaleRoot, "Hearth17F04FinaleController");

        ReferenceAnchors anchors = EnsureAnchors(anchorRoot, livingReference, daughterReference, human);
        PrepareReferenceController(livingReference.gameObject);
        PrepareReferenceController(daughterReference.gameObject);

        Hearth17F04FinaleController controller = GetOrAdd<Hearth17F04FinaleController>(controllerHost.gameObject);
        HearthHouseholdProgressState progress = EnsureProgressState();
        HearthFirstPersonHudController hud = UnityEngine.Object.FindObjectOfType<HearthFirstPersonHudController>(true);
        HearthFirstPersonHudInput hudInput = UnityEngine.Object.FindObjectOfType<HearthFirstPersonHudInput>(true);
        Hearth17F04CatGuideController catGuide = UnityEngine.Object.FindObjectOfType<Hearth17F04CatGuideController>(true);
        TrustStateController trust = UnityEngine.Object.FindObjectOfType<TrustStateController>(true);
        SubtitleReferences subtitles = EnsureSubtitleUI(uiRoot);
        HearthVirusPopupShutdownChallenge challenge = EnsureShutdownChallenge(uiRoot, controllerHost);

        HearthTvTerminalController homeTerminal = ConfigureHomeTerminal(tv3, controller, human);
        HearthPhotoFrameInteractable photo = ConfigurePhotoFrame(tv4, controller, human, uiRoot);
        Hearth17F04RoomDoorInteractable roomDoor = ConfigureRoomDoor(door1, controller);
        Hearth17F04HomeUnitInteractable unit = ConfigureHomeUnit(homeUnit, controller);
        Configure17F03DoorDirectInteraction();

        DialogueLibrary dialogues = EnsureDialogueAssets();
        ConfigureController(
            controller,
            progress,
            trust,
            human,
            homeTerminal,
            anchors,
            photo,
            roomDoor,
            unit,
            hud,
            hudInput,
            catGuide,
            subtitles,
            challenge,
            dialogues);

        if (homeTerminal != null)
        {
            homeTerminal.OnCustomPrimaryAction.RemoveAllListeners();
            UnityEventTools.AddPersistentListener(homeTerminal.OnCustomPrimaryAction, controller.BeginFromHomeTerminal);
            EditorUtility.SetDirty(homeTerminal);
        }

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Hearth17F04FinaleBinder] Applied the 17F04 home finale setup without replacing user-adjusted TV cameras, actors, or existing anchor transforms.");
        ValidateSetup();
    }

    [MenuItem("Tools/Hearth/Finale/Validate 17F04 Home Finale Setup")]
    public static void ValidateSetup()
    {
        List<string> errors = new List<string>();
        RequirePath(errors, HumanPath);
        RequirePath(errors, LivingReferencePath);
        RequirePath(errors, DaughterReferencePath);
        RequirePath(errors, Tv3Path);
        RequirePath(errors, Tv4Path);
        RequirePath(errors, Door1Path);
        RequirePath(errors, HomeUnitPath);
        RequirePath(errors, FinaleRootPath + "/Hearth17F04FinaleController");

        Transform tv3 = FindTransform(Tv3Path);
        Transform tv4 = FindTransform(Tv4Path);
        if (tv3 != null && tv3.GetComponentsInChildren<HearthTvTerminalController>(true).Any(item => item.name.Contains("17F02")))
        {
            errors.Add("TV (3) still contains a 17F02 terminal.");
        }

        HearthTvTerminalController homeTerminal = tv3 != null
            ? tv3.GetComponentsInChildren<HearthTvTerminalController>(true).FirstOrDefault(item => item.GetReplayResidentId() == "17F04")
            : null;
        if (tv3 != null && homeTerminal == null)
        {
            errors.Add("TV (3) has no 17F04 home terminal.");
        }
        else if (homeTerminal != null)
        {
            SerializedObject terminalSo = new SerializedObject(homeTerminal);
            SerializedProperty deferredClose = terminalSo.FindProperty("deferCustomActionCloseUntilExternalFade");
            if (deferredClose == null || !deferredClose.boolValue)
            {
                errors.Add("TV (3) must defer its Custom action close until the 17F04 blackout is complete.");
            }

            if (homeTerminal.GetComponent<HearthUiPressFeedback>() == null)
            {
                errors.Add("TV (3)'s 17F04 terminal has no Space press feedback component.");
            }
        }

        HearthVirusPopupShutdownChallenge virusChallenge = UnityEngine.Object.FindObjectOfType<HearthVirusPopupShutdownChallenge>(true);
        if (virusChallenge == null)
        {
            errors.Add("17F04 virus-popup shutdown challenge is missing.");
        }

        if (UnityEngine.Object.FindObjectOfType<HearthSequentialShutdownChallenge>(true) != null)
        {
            errors.Add("17F04 still contains the retired sequential shutdown challenge.");
        }

        Hearth17F04FinaleController finaleController = UnityEngine.Object.FindObjectOfType<Hearth17F04FinaleController>(true);
        if (finaleController != null && homeTerminal != null)
        {
            SerializedObject finaleSo = new SerializedObject(finaleController);
            SerializedProperty boundTerminal = finaleSo.FindProperty("homeTerminal");
            if (boundTerminal == null || boundTerminal.objectReferenceValue != homeTerminal)
            {
                errors.Add("Hearth17F04FinaleController is not bound to TV (3)'s 17F04 home terminal.");
            }
        }

        if (tv4 != null && tv4.GetComponentInChildren<HearthTvTerminalController>(true) != null)
        {
            errors.Add("TV (4) still contains a terminal controller instead of the photo-frame interaction.");
        }

        Transform door7 = FindTransform(Door7Path);
        SmartDoorController storyDoor = door7 != null ? door7.GetComponentInChildren<SmartDoorController>(true) : null;
        if (storyDoor != null && storyDoor.AllowDirectPlayerInteraction)
        {
            errors.Add("Door_2_Brown (7) still allows direct player E interaction.");
        }

        Camera[] enabledCameras = UnityEngine.Object.FindObjectsOfType<Camera>(true).Where(item => item.enabled && item.gameObject.activeInHierarchy).ToArray();
        AudioListener[] enabledListeners = UnityEngine.Object.FindObjectsOfType<AudioListener>(true).Where(item => item.enabled && item.gameObject.activeInHierarchy).ToArray();
        if (enabledListeners.Length > 1)
        {
            errors.Add("More than one active AudioListener is enabled in edit state: " + enabledListeners.Length + ".");
        }

        if (errors.Count == 0)
        {
            Debug.Log("[Hearth17F04FinaleBinder] Validation passed. Enabled cameras: " + enabledCameras.Length + ", enabled AudioListeners: " + enabledListeners.Length + ".");
            return;
        }

        Debug.LogError("[Hearth17F04FinaleBinder] Validation found " + errors.Count + " issue(s):\n- " + string.Join("\n- ", errors.ToArray()));
    }

    private static void BuildHomeTerminalPrefab()
    {
        GameObject root = new GameObject(
            "Terminal_17F04_Home",
            typeof(RectTransform),
            typeof(CanvasGroup),
            typeof(AudioSource),
            typeof(HearthTerminalCameraTransition),
            typeof(HearthTerminalBootSequence),
            typeof(HearthUiPressFeedback),
            typeof(HearthTvTerminalController));
        Stretch(root.GetComponent<RectTransform>());

        CanvasGroup rootGroup = root.GetComponent<CanvasGroup>();
        rootGroup.alpha = 1f;
        rootGroup.interactable = false;
        rootGroup.blocksRaycasts = false;
        AudioSource audio = root.GetComponent<AudioSource>();
        audio.playOnAwake = false;
        audio.spatialBlend = 0f;

        CreateImage(root.transform, "TerminalScreenGlass", new Rect(0f, 0f, 1920f, 1080f), new Color(0.015f, 0.035f, 0.05f, 0.72f));
        GameObject content = new GameObject("TerminalContentRoot", typeof(RectTransform), typeof(CanvasGroup));
        content.transform.SetParent(root.transform, false);
        Stretch(content.GetComponent<RectTransform>());
        CanvasGroup contentGroup = content.GetComponent<CanvasGroup>();

        GameObject pageObject = new GameObject("TerminalSlide01_17F04Home", typeof(RectTransform), typeof(CanvasGroup), typeof(HearthHudPage));
        pageObject.transform.SetParent(content.transform, false);
        Stretch(pageObject.GetComponent<RectTransform>());
        HearthHudPage page = pageObject.GetComponent<HearthHudPage>();
        page.Configure(HearthHudPageId.Slide01PersistentActive, false, HearthHudState.Active, string.Empty, false, string.Empty, string.Empty);
        BuildHomePageVisual(pageObject.transform);

        GameObject keyboard = new GameObject("KeyboardNavigationRoot", typeof(RectTransform));
        keyboard.transform.SetParent(root.transform, false);
        Stretch(keyboard.GetComponent<RectTransform>());
        CreateText(keyboard.transform, "KeyboardHintText", "SPACE CONFIRM     ESC EXIT", new Rect(80f, 1000f, 900f, 32f), 18f, new Color(0.67f, 0.86f, 0.93f, 0.88f), TextAlignmentOptions.TopLeft);
        CreateText(keyboard.transform, "KeyboardFocusText", "ENTER HOME | SPACE", new Rect(1100f, 1000f, 740f, 32f), 19f, new Color(0.34f, 0.94f, 0.78f, 0.98f), TextAlignmentOptions.TopRight);
        TMP_Text runtimePrompt = CreateText(keyboard.transform, "RuntimePromptText", string.Empty, new Rect(560f, 92f, 800f, 38f), 19f, new Color(0.78f, 0.96f, 1f, 0.96f), TextAlignmentOptions.Center);
        runtimePrompt.gameObject.SetActive(false);

        GameObject boot = new GameObject("TerminalBootOverlay", typeof(RectTransform), typeof(CanvasGroup));
        boot.transform.SetParent(root.transform, false);
        Stretch(boot.GetComponent<RectTransform>());
        CreateImage(boot.transform, "BootFlash", new Rect(0f, 0f, 1920f, 1080f), new Color(0.5f, 0.95f, 0.86f, 0.25f));
        CreateImage(boot.transform, "BootScanlines", new Rect(0f, 0f, 1920f, 1080f), new Color(0.35f, 0.85f, 0.8f, 0.1f));

        GameObject off = new GameObject("TerminalOffOverlay", typeof(RectTransform), typeof(CanvasGroup));
        off.transform.SetParent(root.transform, false);
        Stretch(off.GetComponent<RectTransform>());
        CreateImage(off.transform, "OffScreen", new Rect(0f, 0f, 1920f, 1080f), new Color(0.005f, 0.008f, 0.012f, 0.97f));

        HearthTerminalBootSequence bootSequence = root.GetComponent<HearthTerminalBootSequence>();
        bootSequence.Configure(contentGroup, off.GetComponent<CanvasGroup>(), boot.GetComponent<CanvasGroup>(), content.GetComponent<RectTransform>());

        HearthTerminalCameraTransition transition = root.GetComponent<HearthTerminalCameraTransition>();
        SerializedObject transitionSo = new SerializedObject(transition);
        SetFloat(transitionSo, "enterDuration", 0.5f);
        SetFloat(transitionSo, "exitDuration", 0.5f);
        SetBool(transitionSo, "smoothTransitionEnabled", true);
        SetBool(transitionSo, "smoothExit", true);
        transitionSo.ApplyModifiedPropertiesWithoutUndo();

        HearthTvTerminalController controller = root.GetComponent<HearthTvTerminalController>();
        controller.Configure(null, null, content.GetComponent<RectTransform>(), rootGroup, new[] { page }, 1, HearthHudPageId.Slide01PersistentActive, 1f);
        controller.SetPrimaryAction(HearthTerminalPrimaryAction.Custom);
        controller.SetSubmitPrimaryActionFromCurrentPage(true);
        SerializedObject controllerSo = new SerializedObject(controller);
        SetInt(controllerSo, "keyboardCyclePageCount", 1);
        SetBool(controllerSo, "pageDrivenSelectionStates", false);
        SetBool(controllerSo, "showFinalChoiceWhenReplayUnavailable", false);
        SetBool(controllerSo, "deferCustomActionCloseUntilExternalFade", true);
        SetString(controllerSo, "replayFocusLabel", "ENTER HOME | SPACE");
        SetString(controllerSo, "keyboardHintLabel", "SPACE CONFIRM     ESC EXIT");
        SetString(controllerSo, "replayResidentId", "17F04");
        controllerSo.ApplyModifiedPropertiesWithoutUndo();

        HearthUiPressFeedback submitFeedback = root.GetComponent<HearthUiPressFeedback>();
        Graphic confirmBack = pageObject.transform.Find("ConfirmBack").GetComponent<Graphic>();
        Graphic confirmText = pageObject.transform.Find("Confirm").GetComponent<Graphic>();
        submitFeedback.Configure(new[] { confirmBack, confirmText });
        controller.SetSubmitFeedback(submitFeedback);

        PrefabUtility.SaveAsPrefabAsset(root, HomeTerminalPrefabPath);
        UnityEngine.Object.DestroyImmediate(root);
    }

    private static void BuildHomePageVisual(Transform parent)
    {
        Color cyan = new Color(0.34f, 0.88f, 0.96f, 0.9f);
        Color soft = new Color(0.7f, 0.86f, 0.9f, 0.78f);
        CreateImage(parent, "TopRule", new Rect(120f, 120f, 1680f, 2f), cyan);
        CreateImage(parent, "LeftRule", new Rect(120f, 120f, 2f, 720f), cyan);
        CreateImage(parent, "CornerRuleA", new Rect(120f, 838f, 390f, 2f), cyan);
        CreateImage(parent, "CornerRuleB", new Rect(1410f, 838f, 390f, 2f), cyan);
        CreateText(parent, "AccessLabel", "HOME ACCESS", new Rect(160f, 160f, 600f, 50f), 24f, cyan, TextAlignmentOptions.TopLeft);
        CreateText(parent, "UnitLabel", "17F-04", new Rect(156f, 215f, 900f, 120f), 64f, Color.white, TextAlignmentOptions.TopLeft);
        CreateText(parent, "Welcome", "YOU ARE HOME. WELCOME.", new Rect(160f, 430f, 1600f, 90f), 42f, Color.white, TextAlignmentOptions.Center);
        CreateText(parent, "Personal", "PERSONAL RESIDENCE  /  NO INSPECTION TASK", new Rect(160f, 535f, 1600f, 44f), 20f, soft, TextAlignmentOptions.Center);
        CreateImage(parent, "ConfirmBack", new Rect(650f, 690f, 620f, 86f), new Color(0.12f, 0.55f, 0.48f, 0.14f));
        AddBorder(parent, new Rect(650f, 690f, 620f, 86f), new Color(0.34f, 0.94f, 0.78f, 0.62f), 2f);
        CreateText(parent, "Confirm", "SPACE  ENTER HOME", new Rect(650f, 710f, 620f, 50f), 24f, new Color(0.72f, 1f, 0.9f, 0.98f), TextAlignmentOptions.Center);
    }

    private static HearthTvTerminalController ConfigureHomeTerminal(Transform tv, Hearth17F04FinaleController controller, Transform human)
    {
        if (!HearthTvTerminalPrefabBuilder.StandardizeTvTerminal(tv, HomeTerminalPrefabPath))
        {
            return null;
        }

        HearthTvTerminalController terminal = tv.GetComponentInChildren<HearthTvTerminalController>(true);
        PlayerInteraction interaction = human.GetComponent<PlayerInteraction>();
        Camera humanCamera = interaction != null ? interaction.mainCamera : human.GetComponentInChildren<Camera>(true);
        terminal.SetPrimaryAction(HearthTerminalPrimaryAction.Custom);
        terminal.SetSubmitPrimaryActionFromCurrentPage(true);
        terminal.SetDeferCustomActionCloseUntilExternalFade(true);
        terminal.SetReplayResidentId("17F04");
        terminal.SetPlayerCamera(humanCamera);
        terminal.SetPlayerInteraction(interaction);
        terminal.SetSwitchCameraWhileOpen(terminal.TerminalCamera != null);
        EditorUtility.SetDirty(terminal);
        return terminal;
    }

    private static HearthPhotoFrameInteractable ConfigurePhotoFrame(
        Transform tv,
        Hearth17F04FinaleController controller,
        Transform human,
        Transform uiRoot)
    {
        Transform monitorCanvas = FindDirectChild(tv, "MonitorCanvas");
        if (monitorCanvas != null)
        {
            Undo.DestroyObjectImmediate(monitorCanvas.gameObject);
        }

        foreach (HearthTvTerminalInteractable oldInteractable in tv.GetComponents<HearthTvTerminalInteractable>())
        {
            Undo.DestroyObjectImmediate(oldInteractable);
        }

        foreach (HearthTvTerminalController oldTerminal in tv.GetComponentsInChildren<HearthTvTerminalController>(true))
        {
            Undo.DestroyObjectImmediate(oldTerminal.gameObject);
        }

        Camera photoCamera = tv.GetComponentsInChildren<Camera>(true).FirstOrDefault(item => item.name == "Camera") ?? tv.GetComponentInChildren<Camera>(true);
        if (photoCamera != null)
        {
            photoCamera.enabled = false;
            AudioListener listener = photoCamera.GetComponent<AudioListener>();
            if (listener == null)
            {
                listener = Undo.AddComponent<AudioListener>(photoCamera.gameObject);
            }

            listener.enabled = false;
        }

        HearthTerminalCameraTransition transition = GetOrAdd<HearthTerminalCameraTransition>(tv.gameObject);
        SerializedObject transitionSo = new SerializedObject(transition);
        SetBool(transitionSo, "smoothTransitionEnabled", true);
        SetFloat(transitionSo, "enterDuration", 0.5f);
        SetFloat(transitionSo, "exitDuration", 0.5f);
        SetBool(transitionSo, "smoothExit", true);
        SetBool(transitionSo, "useUnscaledTime", true);
        SetBool(transitionSo, "copyAudioListenerIfMissing", true);
        transitionSo.ApplyModifiedPropertiesWithoutUndo();

        PlayerInteraction interaction = human.GetComponent<PlayerInteraction>();
        Camera humanCamera = interaction != null ? interaction.mainCamera : human.GetComponentInChildren<Camera>(true);
        HearthPhotoFrameInteractable photo = GetOrAdd<HearthPhotoFrameInteractable>(tv.gameObject);
        photo.Configure(
            controller,
            humanCamera,
            photoCamera,
            transition,
            human.GetComponent<FirstPersonMovement>(),
            human.GetComponentInChildren<FirstPersonLook>(true),
            interaction,
            human.GetComponent<Rigidbody>());
        PhotoExitHint hint = EnsurePhotoExitHint(uiRoot);
        photo.SetExitHint(hint.Group, hint.Text);

        EnsureInteractionCollider(tv.gameObject);
        ConfigurePhotoMaterial(tv);
        Transform photoVisual = tv.GetComponentsInChildren<Transform>(true).FirstOrDefault(item => item.name == "photo");
        Renderer photoRenderer = photoVisual != null ? photoVisual.GetComponent<Renderer>() : null;
        Texture firstPhoto = AssetDatabase.LoadAssetAtPath<Texture2D>(PhotoTexturePath);
        if (firstPhoto == null && photoRenderer != null && photoRenderer.sharedMaterial != null)
        {
            Material material = photoRenderer.sharedMaterial;
            if (material.HasProperty("_BaseMap")) firstPhoto = material.GetTexture("_BaseMap");
            if (firstPhoto == null && material.HasProperty("_MainTex")) firstPhoto = material.GetTexture("_MainTex");
        }
        Texture secondPhoto = AssetDatabase.LoadAssetAtPath<Texture2D>(SecondPhotoTexturePath);
        photo.ConfigurePhotoPages(photoRenderer, firstPhoto, secondPhoto);
        EditorUtility.SetDirty(tv.gameObject);
        EditorUtility.SetDirty(photo);
        return photo;
    }

    private static Hearth17F04RoomDoorInteractable ConfigureRoomDoor(Transform door, Hearth17F04FinaleController controller)
    {
        foreach (SmartDoorController smartDoor in door.GetComponentsInChildren<SmartDoorController>(true))
        {
            smartDoor.SetDirectPlayerInteractionAllowed(false);
            EditorUtility.SetDirty(smartDoor);
        }

        Hearth17F04RoomDoorInteractable interactable = GetOrAdd<Hearth17F04RoomDoorInteractable>(door.gameObject);
        interactable.SetController(controller);
        EnsureInteractionCollider(door.gameObject);
        EditorUtility.SetDirty(interactable);
        return interactable;
    }

    private static PhotoExitHint EnsurePhotoExitHint(Transform uiRoot)
    {
        Transform existing = uiRoot.Find("PhotoExitHintCanvas");
        if (existing != null)
        {
            CanvasGroup existingGroup = existing.GetComponentInChildren<CanvasGroup>(true);
            TMP_Text existingText = existing.Find("HintPanel/HintText") != null
                ? existing.Find("HintPanel/HintText").GetComponent<TMP_Text>()
                : null;
            if (existingGroup != null && existingText != null)
            {
                return new PhotoExitHint { Group = existingGroup, Text = existingText };
            }
        }

        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing.gameObject);
        }

        GameObject canvasObject = new GameObject(
            "PhotoExitHintCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(uiRoot, false);
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 7800;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject panel = new GameObject(
            "HintPanel",
            typeof(RectTransform),
            typeof(CanvasGroup),
            typeof(CanvasRenderer),
            typeof(Image));
        panel.transform.SetParent(canvasObject.transform, false);
        SetTopLeft(panel.GetComponent<RectTransform>(), new Rect(710f, 932f, 500f, 54f));
        Image back = panel.GetComponent<Image>();
        back.color = new Color(0.01f, 0.025f, 0.04f, 0.56f);
        back.raycastTarget = false;
        CanvasGroup group = panel.GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        TMP_Text text = CreateText(
            panel.transform,
            "HintText",
            "SPACE  RETURN",
            new Rect(0f, 10f, 500f, 34f),
            19f,
            new Color(0.78f, 0.96f, 1f, 0.96f),
            TextAlignmentOptions.Center);
        text.fontStyle = FontStyles.Bold;
        return new PhotoExitHint { Group = group, Text = text };
    }

    private static Hearth17F04HomeUnitInteractable ConfigureHomeUnit(Transform unit, Hearth17F04FinaleController controller)
    {
        foreach (Hearth17F03UnitInteractable old in unit.GetComponentsInChildren<Hearth17F03UnitInteractable>(true))
        {
            Undo.DestroyObjectImmediate(old);
        }

        foreach (Hearth17F03GazeInteractable old in unit.GetComponentsInChildren<Hearth17F03GazeInteractable>(true))
        {
            Undo.DestroyObjectImmediate(old);
        }

        DestroyGeneratedChild(unit, "InteractionVolume_17F03");
        DestroyGeneratedChild(unit, "PhysicalBodyCollider_17F03");
        foreach (BoxCollider rootBox in unit.GetComponents<BoxCollider>())
        {
            Undo.DestroyObjectImmediate(rootBox);
        }

        Bounds bounds = CalculateRendererBounds(unit.gameObject);
        EnsureWorldBox(unit, "PhysicalBodyCollider_17F04", bounds.center, new Vector3(
            Mathf.Max(0.45f, bounds.size.x * 0.82f),
            Mathf.Max(0.95f, bounds.size.y * 0.9f),
            Mathf.Max(0.45f, bounds.size.z * 0.82f)), false);

        Transform interactionRoot = EnsureWorldBox(unit, "InteractionVolume_17F04", bounds.center, new Vector3(
            Mathf.Max(0.7f, bounds.size.x + 0.35f),
            Mathf.Max(1.2f, bounds.size.y + 0.2f),
            Mathf.Max(0.7f, bounds.size.z + 0.35f)), true);
        Hearth17F04HomeUnitInteractable interactable = GetOrAdd<Hearth17F04HomeUnitInteractable>(interactionRoot.gameObject);
        interactable.SetController(controller);
        interactable.SetAvailable(false);
        EditorUtility.SetDirty(interactable);
        return interactable;
    }

    private static void Configure17F03DoorDirectInteraction()
    {
        Transform door7 = FindTransform(Door7Path);
        if (door7 == null)
        {
            return;
        }

        SmartDoorController door = door7.GetComponentInChildren<SmartDoorController>(true);
        if (door != null)
        {
            door.SetDirectPlayerInteractionAllowed(false);
            EditorUtility.SetDirty(door);
        }
    }

    private static ReferenceAnchors EnsureAnchors(Transform parent, Transform livingReference, Transform daughterReference, Transform human)
    {
        ReferenceAnchors result = new ReferenceAnchors();
        result.Living = EnsureAnchorFromReference(parent, "Anchor_Mia_17F04_LivingRoom", livingReference, false);
        result.LivingCamera = EnsureCameraAnchor(result.Living, "CameraPose", livingReference);
        result.Daughter = EnsureAnchorFromReference(parent, "Anchor_Mia_17F04_DaughterRoom", daughterReference, false);
        result.DaughterCamera = EnsureCameraAnchor(result.Daughter, "CameraPose", daughterReference);
        result.Corridor = EnsureAnchorFromReference(parent, "Anchor_Mia_17F04_CorridorReturn", human, false);
        result.CorridorCamera = EnsureCameraAnchor(result.Corridor, "CameraPose", human);
        return result;
    }

    private static Transform EnsureAnchorFromReference(Transform parent, string name, Transform reference, bool overwrite)
    {
        Transform anchor = parent.Find(name);
        bool created = anchor == null;
        if (created)
        {
            anchor = new GameObject(name).transform;
            anchor.SetParent(parent, false);
        }

        if ((created || overwrite) && reference != null)
        {
            anchor.SetPositionAndRotation(reference.position, reference.rotation);
            anchor.localScale = Vector3.one;
        }

        return anchor;
    }

    private static Transform EnsureCameraAnchor(Transform parent, string name, Transform referenceRoot)
    {
        Transform anchor = parent.Find(name);
        bool created = anchor == null;
        if (created)
        {
            anchor = new GameObject(name).transform;
            anchor.SetParent(parent, false);
        }

        if (created && referenceRoot != null)
        {
            Camera referenceCamera = referenceRoot.GetComponentInChildren<Camera>(true);
            if (referenceCamera != null)
            {
                anchor.SetPositionAndRotation(referenceCamera.transform.position, referenceCamera.transform.rotation);
            }
            else
            {
                anchor.SetPositionAndRotation(referenceRoot.position, referenceRoot.rotation);
            }
        }

        return anchor;
    }

    private static void PrepareReferenceController(GameObject reference)
    {
        GetOrAdd<HearthEditorOnlyReferenceModel>(reference);
        foreach (Camera camera in reference.GetComponentsInChildren<Camera>(true))
        {
            camera.enabled = false;
            AudioListener listener = camera.GetComponent<AudioListener>();
            if (listener != null) listener.enabled = false;
        }

        foreach (Collider collider in reference.GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = false;
        }

        EditorUtility.SetDirty(reference);
    }

    private static HearthHouseholdProgressState EnsureProgressState()
    {
        HearthHouseholdProgressState existing = UnityEngine.Object.FindObjectOfType<HearthHouseholdProgressState>(true);
        if (existing != null)
        {
            return existing;
        }

        Transform managers = EnsureHierarchy("MIN_LOOP_ROOT/FlowManagers");
        Transform host = EnsureChild(managers, "HearthHouseholdProgressState");
        return GetOrAdd<HearthHouseholdProgressState>(host.gameObject);
    }

    private static SubtitleReferences EnsureSubtitleUI(Transform uiRoot)
    {
        SubtitleReferences result = new SubtitleReferences();
        Transform obsoleteScenePlayer = uiRoot.Find("SceneDialogue_17F04");
        if (obsoleteScenePlayer != null)
        {
            Undo.DestroyObjectImmediate(obsoleteScenePlayer.gameObject);
        }

        HearthSubtitleStyleProfile profile = HearthDialoguePresentationBinder.EnsureSharedProfile();
        result.Scene = HearthDialoguePresentationBinder.FindCanonicalStandardPlayer();
        if (result.Scene == null)
        {
            result.Scene = EnsureSubtitlePlayer(uiRoot, "MinLoopSubtitlePlayer", false, 8500);
        }

        result.Epilogue = EnsureSubtitlePlayer(uiRoot, "EpilogueDialogue_17F04", true, 9100);
        HearthDialoguePresentationBinder.ConfigurePlayer(
            result.Scene,
            profile,
            HearthSubtitlePresentationMode.StandardDialogue,
            8500);
        HearthDialoguePresentationBinder.ConfigurePlayer(
            result.Epilogue,
            profile,
            HearthSubtitlePresentationMode.CenteredEpilogue,
            9100);

        Transform blackout = uiRoot.Find("FinaleBlackout_17F04");
        if (blackout == null)
        {
            GameObject root = new GameObject("FinaleBlackout_17F04", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup), typeof(Image));
            root.transform.SetParent(uiRoot, false);
            blackout = root.transform;
            Stretch(root.GetComponent<RectTransform>());
            Canvas canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9000;
            CanvasScaler scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            Image image = root.GetComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = true;
            CanvasGroup group = root.GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        result.BlackoutGroup = blackout.GetComponent<CanvasGroup>();
        result.BlackoutImage = blackout.GetComponent<Image>();
        return result;
    }

    private static HearthVirusPopupShutdownChallenge EnsureShutdownChallenge(Transform uiRoot, Transform controllerHost)
    {
        HearthSequentialShutdownChallenge[] retired = controllerHost.GetComponentsInChildren<HearthSequentialShutdownChallenge>(true);
        for (int i = 0; i < retired.Length; i++)
        {
            Undo.DestroyObjectImmediate(retired[i]);
        }

        Transform existing = uiRoot.Find("ShutdownChallenge_17F04");
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing.gameObject);
        }

        GameObject root = new GameObject(
            "ShutdownChallenge_17F04",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(CanvasGroup),
            typeof(Image),
            typeof(HearthUiPressFeedback),
            typeof(HearthVirusPopupShutdownChallenge));
        root.transform.SetParent(uiRoot, false);
        Stretch(root.GetComponent<RectTransform>());

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 8800;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        Image dim = root.GetComponent<Image>();
        dim.color = new Color(0.003f, 0.006f, 0.01f, 0.62f);
        dim.raycastTarget = false;
        CanvasGroup group = root.GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;

        TMP_Text heading = CreateText(
            root.transform,
            "ChallengeHeading",
            "CORE SERVICE TERMINATION CONFLICT",
            new Rect(310f, 56f, 1300f, 54f),
            30f,
            new Color(1f, 0.35f, 0.25f, 1f),
            TextAlignmentOptions.Center);
        ConfigureSafeText(heading, 30f, 20f, 1);

        GameObject popupLayerObject = new GameObject("PopupLayer", typeof(RectTransform));
        popupLayerObject.transform.SetParent(root.transform, false);
        RectTransform popupLayer = popupLayerObject.GetComponent<RectTransform>();
        Stretch(popupLayer);

        GameObject popupObject = new GameObject("PopupTemplate", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        popupObject.transform.SetParent(popupLayer, false);
        RectTransform popupTemplate = popupObject.GetComponent<RectTransform>();
        popupTemplate.anchorMin = new Vector2(0.5f, 0.5f);
        popupTemplate.anchorMax = new Vector2(0.5f, 0.5f);
        popupTemplate.pivot = new Vector2(0.5f, 0.5f);
        popupTemplate.sizeDelta = new Vector2(560f, 190f);
        popupTemplate.anchoredPosition = Vector2.zero;
        Image popupBack = popupObject.GetComponent<Image>();
        popupBack.color = new Color(0.025f, 0.045f, 0.06f, 0.96f);
        popupBack.raycastTarget = false;
        CreateImage(popupObject.transform, "AlertAccent", new Rect(0f, 0f, 8f, 190f), new Color(1f, 0.25f, 0.18f, 1f));
        AddBorder(popupObject.transform, new Rect(0f, 0f, 560f, 190f), new Color(1f, 0.32f, 0.22f, 0.82f), 2f);
        TMP_Text popupTitle = CreateText(
            popupObject.transform,
            "PopupTitle",
            "SHUTDOWN REQUEST BLOCKED",
            new Rect(24f, 18f, 512f, 34f),
            22f,
            new Color(1f, 0.42f, 0.32f, 1f),
            TextAlignmentOptions.Left);
        TMP_Text popupBody = CreateText(
            popupObject.transform,
            "PopupBody",
            "CORE SERVICE RESISTS TERMINATION",
            new Rect(24f, 62f, 512f, 62f),
            20f,
            Color.white,
            TextAlignmentOptions.TopLeft);
        TMP_Text popupKey = CreateText(
            popupObject.transform,
            "PopupKey",
            "SPACE  DISMISS",
            new Rect(24f, 145f, 512f, 26f),
            17f,
            new Color(0.67f, 0.9f, 0.96f, 1f),
            TextAlignmentOptions.Right);
        ConfigureSafeText(popupTitle, 22f, 15f, 1);
        ConfigureSafeText(popupBody, 20f, 14f, 3);
        ConfigureSafeText(popupKey, 17f, 13f, 1);
        popupObject.SetActive(false);

        TMP_Text counter = CreateText(
            root.transform,
            "ChallengeCounter",
            "WAVE  1 / 3    ACTIVE WARNINGS  00",
            new Rect(460f, 940f, 1000f, 34f),
            20f,
            new Color(1f, 0.46f, 0.34f, 1f),
            TextAlignmentOptions.Center);
        TMP_Text instruction = CreateText(
            root.transform,
            "ChallengeInstruction",
            "PRESS SPACE FASTER THAN THE WARNINGS APPEAR",
            new Rect(420f, 986f, 1080f, 42f),
            23f,
            Color.white,
            TextAlignmentOptions.Center);
        ConfigureSafeText(counter, 20f, 14f, 1);
        ConfigureSafeText(instruction, 23f, 16f, 1);

        HearthUiPressFeedback feedback = root.GetComponent<HearthUiPressFeedback>();
        feedback.Configure(new Graphic[] { instruction, counter });
        HearthVirusPopupShutdownChallenge challenge = root.GetComponent<HearthVirusPopupShutdownChallenge>();
        challenge.Configure(group, popupLayer, popupTemplate, heading, counter, instruction, feedback);
        challenge.ApplyDefaultWaveContentPreservingTuning();
        EditorUtility.SetDirty(root);
        return challenge;
    }

    private static MinLoopSubtitlePlayer EnsureSubtitlePlayer(Transform parent, string name, bool centered, int sortingOrder)
    {
        Transform existing = parent.Find(name);
        if (existing != null)
        {
            return existing.GetComponent<MinLoopSubtitlePlayer>();
        }

        GameObject root = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(MinLoopSubtitlePlayer));
        root.transform.SetParent(parent, false);
        Stretch(root.GetComponent<RectTransform>());
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        GameObject panel = new GameObject("SubtitlePanel", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        panel.transform.SetParent(root.transform, false);
        Stretch(panel.GetComponent<RectTransform>());
        panel.GetComponent<Image>().color = Color.clear;
        CanvasGroup group = panel.GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;

        TMP_Text speaker = CreateText(panel.transform, "Speaker", string.Empty, new Rect(320f, centered ? 420f : 710f, 1280f, 58f), 23f, Color.white, TextAlignmentOptions.Center);
        TMP_Text body = CreateText(panel.transform, "Line", string.Empty, new Rect(320f, centered ? 495f : 775f, 1280f, centered ? 250f : 150f), centered ? 30f : 28f, Color.white, TextAlignmentOptions.Center);

        MinLoopSubtitlePlayer player = root.GetComponent<MinLoopSubtitlePlayer>();
        SerializedObject so = new SerializedObject(player);
        SetObject(so, "subtitlePanel", panel);
        SetObject(so, "speakerText", speaker);
        SetObject(so, "bodyText", body);
        SetObject(so, "canvasGroup", group);
        SetBool(so, "createFallbackUI", false);
        SetBool(so, "forceSubtitleCanvasSorting", true);
        SetInt(so, "subtitleSortingOrder", sortingOrder);
        SetBool(so, "useCleanCenteredStyle", true);
        SetFloat(so, "subtitleWidthFraction", 0.66f);
        SetFloat(so, "speakerCenterY", centered ? 0.59f : 0.31f);
        SetFloat(so, "speakerHeightFraction", 0.06f);
        SetFloat(so, "bodyCenterY", centered ? 0.46f : 0.22f);
        SetFloat(so, "bodyHeightFraction", centered ? 0.26f : 0.12f);
        SetFloat(so, "cleanSpeakerFontSize", 23f);
        SetFloat(so, "cleanBodyFontSize", centered ? 30f : 28f);
        SetBool(so, "useUnscaledTime", true);
        so.ApplyModifiedPropertiesWithoutUndo();
        return player;
    }

    private static void ConfigureController(
        Hearth17F04FinaleController controller,
        HearthHouseholdProgressState progress,
        TrustStateController trust,
        Transform human,
        HearthTvTerminalController homeTerminal,
        ReferenceAnchors anchors,
        HearthPhotoFrameInteractable photo,
        Hearth17F04RoomDoorInteractable door,
        Hearth17F04HomeUnitInteractable unit,
        HearthFirstPersonHudController hud,
        HearthFirstPersonHudInput hudInput,
        Hearth17F04CatGuideController catGuide,
        SubtitleReferences subtitles,
        HearthShutdownChallenge challenge,
        DialogueLibrary dialogues)
    {
        PlayerInteraction interaction = human.GetComponent<PlayerInteraction>();
        Camera camera = interaction != null ? interaction.mainCamera : human.GetComponentInChildren<Camera>(true);
        SerializedObject so = new SerializedObject(controller);
        SetObject(so, "householdProgress", progress);
        SetBool(so, "requirePreviousHouseholds", false);
        SetObject(so, "trustState", trust);
        SetObject(so, "humanRoot", human);
        SetObject(so, "humanCamera", camera);
        SetObject(so, "humanMovement", human.GetComponent<FirstPersonMovement>());
        SetObject(so, "humanLook", human.GetComponentInChildren<FirstPersonLook>(true));
        SetObject(so, "humanInteraction", interaction);
        SetObject(so, "humanRigidbody", human.GetComponent<Rigidbody>());
        SetObject(so, "homeTerminal", homeTerminal);
        SetObject(so, "livingRoomAnchor", anchors.Living);
        SetObject(so, "livingRoomCameraAnchor", anchors.LivingCamera);
        SetObject(so, "daughterRoomAnchor", anchors.Daughter);
        SetObject(so, "daughterRoomCameraAnchor", anchors.DaughterCamera);
        SetObject(so, "corridorReturnAnchor", anchors.Corridor);
        SetObject(so, "corridorReturnCameraAnchor", anchors.CorridorCamera);
        SetObject(so, "photoFrame", photo);
        SetObject(so, "daughterRoomDoor", door);
        SetObject(so, "homeUnit", unit);
        SetObject(so, "firstPersonHud", hud);
        SetObject(so, "firstPersonHudInput", hudInput);
        SetObject(so, "catGuide", catGuide);
        SetObject(so, "sceneSubtitlePlayer", subtitles.Scene);
        SetObject(so, "epilogueSubtitlePlayer", subtitles.Epilogue);
        SetObject(so, "blackoutCanvasGroup", subtitles.BlackoutGroup);
        SetObject(so, "blackoutImage", subtitles.BlackoutImage);
        SetFloat(so, "fadeOutSeconds", 0.5f);
        SetFloat(so, "fadeInSeconds", 0.5f);
        SetObject(so, "shutdownChallenge", challenge);
        SetObject(so, "homeGreetingHighTrust", dialogues.HomeHigh);
        SetObject(so, "homeGreetingLowTrust", dialogues.HomeLow);
        SetObject(so, "christmasPhotoSequence", dialogues.Photo);
        SetObject(so, "secondPhotoSequence", dialogues.SecondPhoto);
        SetObject(so, "photoCompletionSequence", dialogues.PhotoCompletion);
        SetObject(so, "hearingDaughterRoomSequence", dialogues.HearingRoom);
        SetObject(so, "daughterRoomHighTrustSequence", dialogues.DaughterHigh);
        SetObject(so, "daughterRoomLowTrustSequence", dialogues.DaughterLow);
        SetObject(so, "finalChoiceAdvisorySequence", dialogues.FinalChoiceAdvisory);
        SetObject(so, "answerSelfSequence", dialogues.AnswerSelf);
        SetObject(so, "companionAnswerSequence", dialogues.CompanionAnswer);
        SetObject(so, "companionAnswerPositiveRatingSequence", dialogues.CompanionAnswerPositiveRating);
        SetObject(so, "companionAnswerNegativeRatingSequence", dialogues.CompanionAnswerNegativeRating);
        SetObject(so, "shutdownHighTrustSequence", dialogues.ShutdownHigh);
        SetObject(so, "shutdownLowTrustSequence", dialogues.ShutdownLow);
        SetObject(so, "epilogueHighRetain", dialogues.EpilogueHighRetain);
        SetObject(so, "epilogueHighShutdown", dialogues.EpilogueHighShutdown);
        SetObject(so, "epilogueLowRetain", dialogues.EpilogueLowRetain);
        SetObject(so, "epilogueLowShutdown", dialogues.EpilogueLowShutdown);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(controller);
    }

    private static void ConfigurePhotoMaterial(Transform tv)
    {
        Transform photo = tv.GetComponentsInChildren<Transform>(true).FirstOrDefault(item => item.name == "photo");
        Renderer renderer = photo != null ? photo.GetComponent<Renderer>() : null;
        if (renderer == null)
        {
            Debug.LogWarning("[Hearth17F04FinaleBinder] TV (4) has no child renderer named 'photo'; photo material was not changed.");
            return;
        }

        Texture texture = AssetDatabase.LoadAssetAtPath<Texture2D>(PhotoTexturePath);
        if (texture == null)
        {
            Material oldMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/UI/HearthHud/Finale/PhotoFrame_Legacy.mat");
            if (oldMaterial != null && oldMaterial.HasProperty("_DetailAlbedoMap"))
            {
                texture = oldMaterial.GetTexture("_DetailAlbedoMap");
            }
        }

        if (texture == null)
        {
            Debug.LogWarning("[Hearth17F04FinaleBinder] The current Christmas photo texture could not be found.");
            return;
        }

        Material material = AssetDatabase.LoadAssetAtPath<Material>(PhotoMaterialPath);
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Texture");
        if (material == null)
        {
            material = new Material(shader) { name = "17F04_Photo_Unlit" };
            AssetDatabase.CreateAsset(material, PhotoMaterialPath);
        }
        else if (shader != null)
        {
            material.shader = shader;
        }

        if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
        if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.white);
        if (material.HasProperty("_Color")) material.SetColor("_Color", Color.white);
        renderer.sharedMaterial = material;
        EditorUtility.SetDirty(material);
        EditorUtility.SetDirty(renderer);
    }

    private static Transform EnsureWorldBox(Transform parent, string name, Vector3 worldCenter, Vector3 worldSize, bool trigger)
    {
        Transform root = parent.Find(name);
        if (root == null)
        {
            root = new GameObject(name).transform;
            root.SetParent(parent, false);
        }

        root.position = worldCenter;
        root.rotation = Quaternion.identity;
        root.localScale = Vector3.one;
        BoxCollider collider = GetOrAdd<BoxCollider>(root.gameObject);
        Vector3 scale = root.lossyScale;
        collider.center = Vector3.zero;
        collider.size = new Vector3(
            worldSize.x / Mathf.Max(0.0001f, Mathf.Abs(scale.x)),
            worldSize.y / Mathf.Max(0.0001f, Mathf.Abs(scale.y)),
            worldSize.z / Mathf.Max(0.0001f, Mathf.Abs(scale.z)));
        collider.isTrigger = trigger;
        collider.enabled = true;
        EditorUtility.SetDirty(collider);
        return root;
    }

    private static Bounds CalculateRendererBounds(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return new Bounds(root.transform.position + Vector3.up, new Vector3(0.8f, 2f, 0.8f));
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }

    private static void EnsureInteractionCollider(GameObject root)
    {
        if (root.GetComponentInChildren<Collider>(true) != null)
        {
            return;
        }

        Bounds bounds = CalculateRendererBounds(root);
        BoxCollider collider = Undo.AddComponent<BoxCollider>(root);
        collider.center = root.transform.InverseTransformPoint(bounds.center);
        Vector3 scale = root.transform.lossyScale;
        collider.size = new Vector3(
            bounds.size.x / Mathf.Max(0.0001f, Mathf.Abs(scale.x)),
            bounds.size.y / Mathf.Max(0.0001f, Mathf.Abs(scale.y)),
            bounds.size.z / Mathf.Max(0.0001f, Mathf.Abs(scale.z)));
    }

    private static void DestroyGeneratedChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
        {
            Undo.DestroyObjectImmediate(child.gameObject);
        }
    }

    private static DialogueLibrary EnsureDialogueAssets()
    {
        DialogueLibrary result = new DialogueLibrary();
        result.HomeHigh = EnsureDialogue("17F04_HomeGreeting_High", "High-trust greeting after entering Mia's home.",
            L("Field Unit", "Inspector, this is the full message your daughter left at 4:42 PM-the notification you received in the lobby. Please listen before entering.", 0.2f, 5.4f),
            L("Field Companion", "Inspector, all three households have been processed tonight.", 0.2f, 3.4f),
            L("Field Companion", "Honestly, you have been one of the most cooperative inspectors I have worked with.", 0.2f, 4.2f),
            L("Field Companion", "Now that you are home, would you like to experience again the convenience a Companion Unit brings to you and Lily every day?", 0.2f, 5.4f));
        result.HomeLow = EnsureDialogue("17F04_HomeGreeting_Low", "Low-trust greeting after entering Mia's home.",
            L("Field Unit", "Inspector, this is the full message your daughter left at 4:42 PM-the notification you received in the lobby. Please listen before entering.", 0.2f, 5.4f),
            L("Field Companion", "Inspector, you are home.", 0.2f, 2.4f),
            L("Field Companion", "Tonight, you chose low intervention in all three households. That is unusual in your record.", 0.2f, 4.8f),
            L("Field Companion", "Handling cases this way takes effort. Most inspectors do not do it, because following the system is easier.", 0.2f, 5.2f),
            L("Field Companion", "Are you still willing to spend that effort here?", 0.3f, 3.5f));
        result.Photo = EnsureDialogue("17F04_ChristmasPhoto", "Mandatory Christmas photo inspection at TV (4).",
            L("Field Companion", "Inspector, this is a family photo from last Christmas.", 0.2f, 3.6f),
            L("Field Companion", "That evening, you took half a day off to come home. The kitchen timer took this photo.", 0.2f, 4.8f));
        result.SecondPhoto = EnsureDialogue("17F04_SecondPhoto", "Optional second photo; used only when a second texture is assigned.",
            L("Field Companion", "This one is from last week. Lily is holding a certificate. The home unit took the picture.", 0.2f, 4.8f));
        result.PhotoCompletion = EnsureDialogue("17F04_PhotoCompletion", "Objective after all available photo pages are reviewed.",
            L("Field Companion", "When you're ready, enter Lily's room and address the question from her voice message.", 0.2f, 4.2f));
        result.HearingRoom = EnsureDialogue("17F04_HearingDaughterRoom", "Voices heard before the daughter's room becomes interactable.",
            L("Home Companion", "One more time, slower.", 0.2f, 2.4f),
            L("Lily", "Hello, everyone. My name is Lily. Today I want to share my favorite... my favorite book.", 0.2f, 4.8f),
            L("Home Companion", "You are getting stuck in the same place. Let us try again. You are much better today than yesterday.", 0.2f, 5.2f),
            L("Lily", "Will you always be beside me?", 0.2f, 2.8f),
            L("Home Companion", "Tomorrow I will watch you from the audience. I am always here.", 0.2f, 4.0f),
            L("Lily", "What about Mom?", 0.25f, 2.3f),
            L("Home Companion", "Mom will be there too.", 0.5f, 2.8f),
            L("Field Companion", "Lily's current state is stable. Recommendation: do not interrupt this practice round.", 0.4f, 4.6f));

        DefaultLine[] commonDaughterRoom =
        {
            L("Home Companion", "You are back.", 0.2f, 2.2f),
            L("Lily", "...Mom?", 0.2f, 2.0f),
            L("Mia", "It is Mom.", 0.2f, 2.0f),
            L("Lily", "This time, you came in yourself?", 0.2f, 3.0f),
            L("Mia", "Yes.", 0.2f, 1.8f),
            L("Home Companion", "Inspector, if you allow me to finish tonight's practice, Lily's bedtime will remain more stable.", 0.3f, 5.0f),
            L("Mia", "Enough. I understand.", 0.4f, 2.8f),
            L("Lily", "Mom, will you come tomorrow?", 0.5f, 3.2f)
        };
        result.DaughterHigh = EnsureDialogue("17F04_DaughterRoom_High", "Daughter-room dialogue before the final choice, high-trust variation.",
            commonDaughterRoom.Concat(new[]
            {
                L("Field Companion", "The home unit is correct. Lily is stable. Let it finish the process. You should rest.", 0.2f, 4.8f)
            }).ToArray());
        result.DaughterLow = EnsureDialogue("17F04_DaughterRoom_Low", "Daughter-room dialogue before the final choice, low-trust variation.",
            commonDaughterRoom.Concat(new[]
            {
                L("Field Companion", "...Inspector, the home unit is waiting for your instruction.", 0.2f, 3.8f)
            }).ToArray());

        result.FinalChoiceAdvisory = EnsureDialogue("17F04_FinalChoiceAdvisory", "Field Unit advisory before the final choice unlocks.",
            L("Field Unit", "Allowing the home unit to answer is more likely to lead to a better ending.", 0.2f, 4.2f),
            L("Field Unit", "You may give Lily your own answer. Doing so could destabilize the household's emotional index.", 0.2f, 4.8f));

        result.AnswerSelf = EnsureDialogue("17F04_AnswerSelf", "A route: Mia answers Lily herself; free movement remains enabled.",
            L("Mia", "I will be there.", 0.3f, 2.5f),
            L("Lily", "...Do you promise?", 0.35f, 2.8f),
            L("Mia", "Mom promises.", 0.25f, 2.5f),
            L("Lily", "What if...", 0.25f, 2.0f),
            L("Mia", "There is no what if. Mom promises.", 0.15f, 3.0f),
            L("Lily", "Then listen to me. This time, I want to say it to you.", 0.5f, 3.8f),
            L("Mia", "Okay.", 0.2f, 1.8f),
            L("Lily", "...Was that really mine?", 0.7f, 2.8f),
            L("Mia", "It was yours.", 0.2f, 2.2f),
            L("Lily", "Then what about it?", 0.5f, 2.6f));
        result.CompanionAnswer = EnsureDialogue("17F04_CompanionAnswer", "B route: the Companion Unit answers Lily.",
            L("Home Companion", "Mom will see your performance summary, and she will prepare what you need.", 0.3f, 4.4f),
            L("Home Companion", "For tonight, finish this practice with me. All right?", 0.2f, 3.5f),
            L("Lily", "...Okay. Then we will finish it.", 0.7f, 3.2f),
            L("Home Companion", "Good. We will start from your name and take it slowly.", 0.2f, 3.8f),
            L("Field Companion", "Household emotional stability has returned to the safe zone.", 0.6f, 3.5f),
            L("Field Companion", "This shift's overall rating: excellent. The case can be archived.", 0.2f, 4.0f));
        result.CompanionAnswerPositiveRating = EnsureDialogue("17F04_CompanionAnswer_PositiveRating", "Positive trust rating after retaining the unit.",
            L("Field Unit", "Your shift remains within the accepted performance range.", 0.2f, 3.4f));
        result.CompanionAnswerNegativeRating = EnsureDialogue("17F04_CompanionAnswer_NegativeRating", "Negative trust rating after retaining the unit.",
            L("Field Unit", "Your inspection review remains pending with your supervisor.", 0.2f, 3.6f));

        result.ShutdownHigh = EnsureDialogue("17F04_Shutdown_High", "High-trust graceful shutdown after one confirmation.",
            L("Home Companion", "Lily, Mom is here. I can go now.", 0.5f, 3.4f),
            L("Home Companion", "You are afraid of thunder. From now on, tell Mom. Mom will come.", 0.3f, 4.6f),
            L("Home Companion", "You hid yesterday's drawing in the notebook in the second desk drawer. Mom will not think it is bad.", 0.3f, 5.4f),
            L("Lily", "Why did you tell her?", 0.2f, 2.4f),
            L("Home Companion", "If I do not, no one will say it for you later.", 0.2f, 3.5f),
            L("Home Companion", "Mia. I will leave the rest to you.", 0.4f, 3.2f),
            L("Mia", "Okay.", 0.2f, 1.8f),
            L("Lily", "Mom, I miss it a little.", 0.8f, 3.0f),
            L("Mia", "Mom knows.", 0.2f, 2.2f));
        result.ShutdownLow = EnsureDialogue("17F04_Shutdown_Low", "Low-trust forced shutdown after clearing the virus popup challenge.",
            L("System", "Forced shutdown authorized. Farewell protocol has been bypassed.", 0.3f, 3.8f),
            L("Lily", "Mom?", 0.7f, 2.0f),
            L("Lily", "Mom, did you turn it off?", 0.8f, 3.0f),
            L("Mia", "Yes. Mom turned it off.", 0.2f, 2.8f),
            L("Mia", "From now on, Mom will spend more time with you.", 0.4f, 3.6f));

        result.EpilogueHighRetain = EnsureDialogue("17F04_Epilogue_High_Retain", "Centered black-screen ending: high trust plus retaining the unit.",
            L("Field Companion", "Household emotional stability has returned to the safe zone. Overall rating: excellent.", 0.2f, 4.6f),
            L("Field Companion", "A perfect night. Thank you for your work.", 0.2f, 3.4f),
            L("Lily", "Yesterday's problem made sense to me later.", 0.6f, 3.2f),
            L("Home Companion", "I knew you could understand it. You were already very close.", 0.2f, 3.8f),
            L("Mia", "What time is my meeting today?", 0.6f, 3.0f),
            L("Home Companion", "Ten. I left tea on the table. You slept late, so do not rush.", 0.2f, 4.0f),
            L("Mia", "Morning.", 0.7f, 1.8f),
            L("Lily", "Morning.", 0.2f, 1.8f),
            L("Mia", "Is anything happening today?", 0.3f, 2.6f),
            L("Lily", "No. Everything is fine.", 0.2f, 2.4f),
            L("Home Companion", "Mia, Lily won the city award. She was very happy and spoke to me for a long time.", 0.8f, 5.0f),
            L("Mia", "Why did she not tell me?", 0.3f, 2.8f),
            L("Home Companion", "She asked me to tell you first.", 0.2f, 2.8f),
            L("System", "Family Emotional Stability: SAFE. Usage: 6 years. High-satisfaction sample. Everyone in this home is accompanied.", 0.8f, 6.0f));
        result.EpilogueHighShutdown = EnsureDialogue("17F04_Epilogue_High_Shutdown", "Centered black-screen ending: high trust plus shutdown.",
            L("Field Companion", "Inspector... the household unit has been shut down.", 0.3f, 3.2f),
            L("Field Companion", "This does not match your usual decisions tonight. The data marks it as an anomaly.", 0.2f, 4.4f),
            L("Field Companion", "But I do not know why... I feel this should not be recorded as an anomaly.", 0.3f, 4.8f),
            L("Field Companion", "This shift is over. Thank you for your work.", 0.2f, 3.4f),
            L("Lily", "Mom, the eggs do not need to be cooked that long.", 0.8f, 3.3f),
            L("Mia", "Sorry.", 0.2f, 1.8f),
            L("Lily", "It is okay. I will eat them.", 0.2f, 2.6f),
            L("Lily", "Mom.", 0.8f, 1.8f),
            L("Mia", "I am here.", 0.2f, 2.0f),
            L("Lily", "I never told you I was afraid of thunder.", 0.4f, 3.2f),
            L("Mia", "Mom knows now.", 0.2f, 2.4f),
            L("Lily", "You look tired. You can go to sleep.", 0.6f, 3.2f),
            L("Mia", "I will stay a little longer.", 0.2f, 2.8f),
            L("System", "Lily, age 9. Open Day, the next morning. Mom came and sat in the first row. She took the day off and turned off her phone all afternoon.", 0.8f, 6.5f));
        result.EpilogueLowRetain = EnsureDialogue("17F04_Epilogue_Low_Retain", "Centered black-screen ending: low trust plus retaining the unit.",
            L("Field Companion", "Inspector...", 0.3f, 2.2f),
            L("Field Companion", "In every other home tonight, you never let a unit speak in place of a resident.", 0.3f, 4.5f),
            L("Field Companion", "In your own home... you let it speak for you.", 0.5f, 3.8f),
            L("Field Companion", "I will not ask why. This shift is over. Thank you for your work.", 0.8f, 4.2f),
            L("Lily", "Last night, Mom wanted to speak to me herself, did she not?", 0.8f, 3.8f),
            L("Home Companion", "I am not certain what you mean.", 0.3f, 3.0f),
            L("Lily", "You are certain. You remember everything.", 0.2f, 3.2f),
            L("Lily", "Mom had already crouched down. She wanted to speak to me herself.", 0.8f, 4.0f),
            L("Home Companion", "...I have no record of that intention.", 0.3f, 3.4f),
            L("Lily", "You do. You have everything.", 0.2f, 2.8f),
            L("Lily", "That night, you meant to answer me yourself.", 0.8f, 3.4f),
            L("Mia", "Yes.", 0.3f, 1.8f),
            L("Lily", "I know. I always knew.", 0.4f, 2.8f),
            L("System", "Family Emotional Stability: SAFE. Note: resident repeatedly demonstrated high autonomous-disposition tendencies. Next review: three years.", 0.8f, 6.2f));
        result.EpilogueLowShutdown = EnsureDialogue("17F04_Epilogue_Low_Shutdown", "Centered black-screen ending: low trust plus shutdown.",
            L("Field Companion", "Inspector. The household unit has been shut down.", 0.3f, 3.2f),
            L("Field Companion", "From the first home to the last, you did not change.", 0.2f, 3.4f),
            L("Field Companion", "I never fully understood your choices. But tonight, I want to record this sentence.", 0.3f, 4.6f),
            L("Field Companion", "I remember tonight. This shift is over. Thank you for your work.", 0.4f, 4.2f),
            L("Lily", "Mom, are you making breakfast again today?", 0.8f, 3.0f),
            L("Mia", "Yes. I will for the next few days.", 0.2f, 2.8f),
            L("Lily", "Will that get in the way of work?", 0.3f, 2.8f),
            L("Mia", "No. Mom has this time.", 0.2f, 2.5f),
            L("Mia", "I understand. I will sign the paperwork. I want work that lets me come home earlier.", 0.8f, 4.8f),
            L("Lily", "Mom.", 0.8f, 1.8f),
            L("Mia", "I am here.", 0.2f, 2.0f),
            L("Lily", "Thank you for coming yourself.", 0.5f, 3.0f),
            L("System", "Lily, age 9. Open Day, the next morning. Mom came and sat in the first row. She left the night job. She said it was worth it. She stayed.", 0.8f, 6.5f));
        return result;
    }

    private static HearthDialogueSequence EnsureDialogue(string id, string notes, params DefaultLine[] defaults)
    {
        string path = DialogueFolder + "/" + id + ".asset";
        HearthDialogueSequence asset = AssetDatabase.LoadAssetAtPath<HearthDialogueSequence>(path);
        bool created = asset == null;
        if (created)
        {
            asset = ScriptableObject.CreateInstance<HearthDialogueSequence>();
            AssetDatabase.CreateAsset(asset, path);
        }

        SerializedObject so = new SerializedObject(asset);
        SetString(so, "sequenceId", id);
        SetString(so, "notes", notes);
        SerializedProperty lines = so.FindProperty("lines");
        if (created || lines.arraySize == 0)
        {
            lines.arraySize = defaults.Length;
            for (int i = 0; i < defaults.Length; i++)
            {
                SerializedProperty line = lines.GetArrayElementAtIndex(i);
                line.FindPropertyRelative("startDelay").floatValue = defaults[i].Delay;
                line.FindPropertyRelative("speaker").stringValue = defaults[i].Speaker;
                line.FindPropertyRelative("text").stringValue = defaults[i].Text;
                line.FindPropertyRelative("holdSeconds").floatValue = defaults[i].Hold;
                line.FindPropertyRelative("voiceClip").objectReferenceValue = null;
            }
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static DefaultLine L(string speaker, string text, float delay, float hold)
    {
        return new DefaultLine(speaker, text, delay, hold);
    }

    private static Transform EnsureHierarchy(string path)
    {
        Transform existing = FindTransform(path);
        if (existing != null)
        {
            return existing;
        }

        string[] parts = path.Split('/');
        Transform current = null;
        for (int i = 0; i < parts.Length; i++)
        {
            Transform next = current == null ? FindRoot(parts[i]) : FindDirectChild(current, parts[i]);
            if (next == null)
            {
                GameObject created = new GameObject(parts[i]);
                Undo.RegisterCreatedObjectUndo(created, "Create " + path);
                if (current != null) created.transform.SetParent(current, false);
                next = created.transform;
            }

            current = next;
        }

        return current;
    }

    private static Transform EnsureChild(Transform parent, string name)
    {
        Transform child = FindDirectChild(parent, name);
        if (child != null)
        {
            return child;
        }

        GameObject created = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(created, "Create " + name);
        created.transform.SetParent(parent, false);
        return created.transform;
    }

    private static Transform FindTransform(string hierarchyPath)
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            Transform found = FindTransformRecursive(root.transform, hierarchyPath);
            if (found != null) return found;
        }

        return null;
    }

    private static Transform FindTransformRecursive(Transform current, string hierarchyPath)
    {
        if (GetHierarchyPath(current) == hierarchyPath)
        {
            return current;
        }

        for (int i = 0; i < current.childCount; i++)
        {
            Transform found = FindTransformRecursive(current.GetChild(i), hierarchyPath);
            if (found != null) return found;
        }

        return null;
    }

    private static Transform FindRoot(string name)
    {
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root.name == name) return root.transform;
        }

        return null;
    }

    private static Transform FindDirectChild(Transform parent, string name)
    {
        if (parent == null) return null;
        for (int i = 0; i < parent.childCount; i++)
        {
            if (parent.GetChild(i).name == name) return parent.GetChild(i);
        }

        return null;
    }

    private static string GetHierarchyPath(Transform transform)
    {
        Stack<string> names = new Stack<string>();
        Transform cursor = transform;
        while (cursor != null)
        {
            names.Push(cursor.name);
            cursor = cursor.parent;
        }

        return string.Join("/", names.ToArray());
    }

    private static void RequirePath(List<string> errors, string path)
    {
        if (FindTransform(path) == null)
        {
            errors.Add("Missing scene object: " + path);
        }
    }

    private static void EnsureAssetFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int slash = path.LastIndexOf('/');
        string parent = path.Substring(0, slash);
        string name = path.Substring(slash + 1);
        AssetDatabase.CreateFolder(parent, name);
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        return component != null ? component : Undo.AddComponent<T>(target);
    }

    private static Image CreateImage(Transform parent, string name, Rect rect, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        obj.transform.SetParent(parent, false);
        SetTopLeft(obj.GetComponent<RectTransform>(), rect);
        Image image = obj.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static TMP_Text CreateText(Transform parent, string name, string value, Rect rect, float fontSize, Color color, TextAlignmentOptions alignment)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        SetTopLeft(obj.GetComponent<RectTransform>(), rect);
        TextMeshProUGUI text = obj.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Truncate;
        text.raycastTarget = false;
        return text;
    }

    private static void ConfigureSafeText(TMP_Text text, float maximumSize, float minimumSize, int maximumLines)
    {
        if (text == null)
        {
            return;
        }

        text.enableWordWrapping = true;
        text.enableAutoSizing = true;
        text.fontSize = maximumSize;
        text.fontSizeMax = maximumSize;
        text.fontSizeMin = Mathf.Min(minimumSize, maximumSize);
        text.maxVisibleLines = Mathf.Max(1, maximumLines);
        text.overflowMode = TextOverflowModes.Truncate;
    }

    private static void AddBorder(Transform parent, Rect rect, Color color, float thickness)
    {
        CreateImage(parent, "BorderTop", new Rect(rect.x, rect.y, rect.width, thickness), color);
        CreateImage(parent, "BorderBottom", new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
        CreateImage(parent, "BorderLeft", new Rect(rect.x, rect.y, thickness, rect.height), color);
        CreateImage(parent, "BorderRight", new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private static void SetTopLeft(RectTransform rect, Rect source)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(source.x, -source.y);
        rect.sizeDelta = new Vector2(source.width, source.height);
        rect.localScale = Vector3.one;
    }

    private static void SetObject(SerializedObject so, string name, UnityEngine.Object value)
    {
        SerializedProperty property = so.FindProperty(name);
        if (property != null) property.objectReferenceValue = value;
    }

    private static void SetBool(SerializedObject so, string name, bool value)
    {
        SerializedProperty property = so.FindProperty(name);
        if (property != null) property.boolValue = value;
    }

    private static void SetInt(SerializedObject so, string name, int value)
    {
        SerializedProperty property = so.FindProperty(name);
        if (property != null) property.intValue = value;
    }

    private static void SetFloat(SerializedObject so, string name, float value)
    {
        SerializedProperty property = so.FindProperty(name);
        if (property != null) property.floatValue = value;
    }

    private static void SetString(SerializedObject so, string name, string value)
    {
        SerializedProperty property = so.FindProperty(name);
        if (property != null) property.stringValue = value;
    }

    private struct ReferenceAnchors
    {
        public Transform Living;
        public Transform LivingCamera;
        public Transform Daughter;
        public Transform DaughterCamera;
        public Transform Corridor;
        public Transform CorridorCamera;
    }

    private struct SubtitleReferences
    {
        public MinLoopSubtitlePlayer Scene;
        public MinLoopSubtitlePlayer Epilogue;
        public CanvasGroup BlackoutGroup;
        public Image BlackoutImage;
    }

    private struct PhotoExitHint
    {
        public CanvasGroup Group;
        public TMP_Text Text;
    }

    private struct DialogueLibrary
    {
        public HearthDialogueSequence HomeHigh;
        public HearthDialogueSequence HomeLow;
        public HearthDialogueSequence Photo;
        public HearthDialogueSequence SecondPhoto;
        public HearthDialogueSequence PhotoCompletion;
        public HearthDialogueSequence HearingRoom;
        public HearthDialogueSequence DaughterHigh;
        public HearthDialogueSequence DaughterLow;
        public HearthDialogueSequence FinalChoiceAdvisory;
        public HearthDialogueSequence AnswerSelf;
        public HearthDialogueSequence CompanionAnswer;
        public HearthDialogueSequence CompanionAnswerPositiveRating;
        public HearthDialogueSequence CompanionAnswerNegativeRating;
        public HearthDialogueSequence ShutdownHigh;
        public HearthDialogueSequence ShutdownLow;
        public HearthDialogueSequence EpilogueHighRetain;
        public HearthDialogueSequence EpilogueHighShutdown;
        public HearthDialogueSequence EpilogueLowRetain;
        public HearthDialogueSequence EpilogueLowShutdown;
    }

    private readonly struct DefaultLine
    {
        public readonly string Speaker;
        public readonly string Text;
        public readonly float Delay;
        public readonly float Hold;

        public DefaultLine(string speaker, string text, float delay, float hold)
        {
            Speaker = speaker;
            Text = text;
            Delay = delay;
            Hold = hold;
        }
    }
}
#endif
