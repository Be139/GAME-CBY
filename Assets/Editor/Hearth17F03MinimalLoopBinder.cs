#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Hearth17F03MinimalLoopBinder
{
    private const string DialogueFolder = "Assets/Data/MinLoop/Dialogues";
    private const string AnimationFolder = "Assets/Animations/Hearth17F03";
    private const string MotherBasePath = "Assets/action/casual_Female_G@Sit_To_Stand.fbx";
    private const string MotherArguingPath = "Assets/action/casual_Female_G@Standing_Arguing.fbx";
    private const string MotherTalkingPath = "Assets/action/casual_Female_G@Talking.fbx";
    private const string DaughterCodePath = "Assets/action/casual_Female_K@Entering_Code.fbx";
    private const string DaughterWalkPath = "Assets/action/casual_Female_K@Female_Walk.fbx";
    private const string DaughterSittingPath = "Assets/action/casual_Female_K@Male_Sitting_Pose.fbx";
    private const string DaughterSitupPath = "Assets/action/casual_Female_K@Situp_To_Idle.fbx";
    private const string DaughterTalkingPath = "Assets/action/casual_Female_K@Talking.fbx";
    private const string FatherBasePath = "Assets/action/Doctor_Male_B@Male_Sitting_Pose.fbx";
    private const string ReplayRoomPath = "MIN_LOOP_ROOT/ReplayRoom_17F03";
    private const string RuntimeActorsPath = ReplayRoomPath + "/RuntimeActors";
    private const string StagingPreviewRootName = "StagingPreview_17F03";
    private const string PhysicalBodyColliderName = "PhysicalBodyCollider_17F03";
    private const string InteractionVolumeName = "InteractionVolume_17F03";
    private const float BlockingOverlapTolerance = 0.05f;

    [MenuItem("Tools/Hearth/Replay/Apply 17F03 Minimal Loop Setup")]
    public static void ApplySetup()
    {
        ApplySetupInternal(false);
    }

    [MenuItem("Tools/Hearth/Replay/Rebuild 17F03 Anchors From References")]
    public static void RebuildAnchorsFromReferences()
    {
        ApplySetupInternal(true);
    }

    [MenuItem("Tools/Hearth/Replay/Validate 17F03 Minimal Loop Setup")]
    public static void ValidateSetup()
    {
        int errors = 0;
        errors += ValidateHumanClip(MotherBasePath, "SitToStand");
        errors += ValidateHorizontalRootMotion(MotherBasePath, "SitToStand");
        errors += ValidateHumanClip(MotherArguingPath, "StandingArguing");
        errors += ValidateHumanClip(MotherTalkingPath, "Talking");
        errors += ValidateHumanClip(DaughterCodePath, "EnteringCode");
        errors += ValidateHumanClip(DaughterWalkPath, "Walk");
        errors += ValidateHumanClip(DaughterSittingPath, "SittingPose");
        errors += ValidateHumanClip(DaughterSitupPath, "SitupToIdle");
        errors += ValidateHumanClip(DaughterTalkingPath, "Talking");
        errors += ValidateHumanClip(FatherBasePath, "Sitting");

        HearthCompanion17F03ReplayController controller = FindSceneComponent<HearthCompanion17F03ReplayController>();
        if (controller == null)
        {
            Debug.LogError("[Hearth17F03MinimalLoopBinder] Missing HearthCompanion17F03ReplayController in the open scene.");
            errors++;
        }

        Camera[] activeCameras = FindSceneComponents<Camera>().Where(camera => camera.enabled).ToArray();
        AudioListener[] activeListeners = FindSceneComponents<AudioListener>().Where(listener => listener.enabled).ToArray();
        ViewSwitchController[] activeViewSwitches = FindSceneComponents<ViewSwitchController>().Where(item => item.enabled).ToArray();
        if (activeCameras.Length != 1)
        {
            Debug.LogError("[Hearth17F03MinimalLoopBinder] Expected one enabled Camera in Edit Mode, found " + activeCameras.Length + ".");
            errors++;
        }
        if (activeListeners.Length != 1)
        {
            Debug.LogError("[Hearth17F03MinimalLoopBinder] Expected one enabled AudioListener, found " + activeListeners.Length + ".");
            errors++;
        }
        if (activeViewSwitches.Length != 1)
        {
            Debug.LogError("[Hearth17F03MinimalLoopBinder] Expected one enabled ViewSwitchController, found " + activeViewSwitches.Length + ".");
            errors++;
        }

        GameObject physicalUnit = FindSceneObject("GameObject/ROBOT", "ROBOT");
        GameObject formalHuman = FindSceneObject("Player/Person Controller", "Person Controller");
        GameObject formalRobot = FindSceneObject("Player/Robot Controller", "Robot Controller");
        errors += ValidatePhysicalUnitColliders(physicalUnit);
        Physics.SyncTransforms();
        errors += ValidateRigBlockingOverlaps(formalHuman);
        errors += ValidateRigBlockingOverlaps(formalRobot);
        errors += ValidateRuntimeActorVisualCount("Actor_Mother_17F03_RuntimeRoot", "casual_Female_G@Sit_To_Stand");
        errors += ValidateRuntimeActorVisualCount("Actor_Father_17F03_RuntimeRoot", "Doctor_Male_B@Male_Sitting_Pose");
        errors += ValidateRuntimeActorVisualCount("Actor_Father_17F03_MiddayRuntimeRoot", "Doctor_Male_B@Male_Sitting_Pose (1)");
        errors += ValidateRuntimeActorVisualCount("Actor_Daughter_17F03_RuntimeRoot", "casual_Female_K (2)");

        if (errors == 0)
        {
            Debug.Log("[Hearth17F03MinimalLoopBinder] Validation passed: animations, controller, cameras, listener, ViewSwitch and spawn collisions are ready.");
        }
        else
        {
            Debug.LogError("[Hearth17F03MinimalLoopBinder] Validation failed with " + errors + " issue(s).");
        }
    }

    private static void ApplySetupInternal(bool rebuildAnchors)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("[Hearth17F03MinimalLoopBinder] Exit Play Mode before applying the setup.");
            return;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("[Hearth17F03MinimalLoopBinder] No loaded scene is available.");
            return;
        }

        GameObject daughterSource = FindRuntimeActorVisual(
            "Actor_Daughter_17F03_RuntimeRoot",
            "casual_Female_K (2)",
            "GameObject/casual_Female_K (2)");
        Animator daughterSourceAnimator = daughterSource != null ? daughterSource.GetComponentInChildren<Animator>(true) : null;
        Avatar daughterAvatar = daughterSourceAnimator != null ? daughterSourceAnimator.avatar : null;
        if (daughterAvatar == null || !daughterAvatar.isValid || !daughterAvatar.isHuman)
        {
            Debug.LogError("[Hearth17F03MinimalLoopBinder] casual_Female_K (2) needs a valid Humanoid Avatar before daughter clips can be retargeted.");
            return;
        }

        ClipLibrary clips = ConfigureAnimationImports(daughterAvatar);
        if (!clips.IsComplete)
        {
            Debug.LogError("[Hearth17F03MinimalLoopBinder] Animation import did not produce all required Humanoid clips. Setup stopped before scene mutation.");
            return;
        }

        GameObject formalHuman = FindSceneObject("Player/Person Controller", "Person Controller");
        GameObject humanReference = FindSceneObject("Player/Person Controller (1)", "Person Controller (1)");
        GameObject formalRobot = FindSceneObject("Player/Robot Controller", "Robot Controller");
        GameObject middayRobotReference = FindSceneObject("Player/Robot Controller (4)", "Robot Controller (4)");
        GameObject nightRobotReference = FindSceneObject("Player/Robot Controller (5)", "Robot Controller (5)");
        GameObject physicalUnit = FindSceneObject("GameObject/ROBOT", "ROBOT");
        GameObject motherSource = FindRuntimeActorVisual(
            "Actor_Mother_17F03_RuntimeRoot",
            "casual_Female_G@Sit_To_Stand",
            null);
        GameObject fatherSource = FindRuntimeActorVisual(
            "Actor_Father_17F03_RuntimeRoot",
            "Doctor_Male_B@Male_Sitting_Pose",
            null);
        GameObject middayFatherSource = FindRuntimeActorVisual(
            "Actor_Father_17F03_MiddayRuntimeRoot",
            "Doctor_Male_B@Male_Sitting_Pose (1)",
            null);
        GameObject motherReplayReference = FindSceneObject("GameObject/casual_Female_G (1)", "casual_Female_G (1)");
        GameObject unusedMotherReference = FindSceneObject("GameObject/casual_Female_G", "casual_Female_G");
        GameObject daughterNightStartReference = FindSceneObject("GameObject/casual_Female_K (1)", "casual_Female_K (1)");
        GameObject daughterApproachReference = FindSceneObject("GameObject/casual_Female_K (3)", "casual_Female_K (3)");
        GameObject doorRoot = FindSceneObject("17F/ROOM2/Door_2_Brown (7)", "Door_2_Brown (7)");
        GameObject terminalObject = FindSceneObject("17F/ROOM2/TV (4)/MonitorCanvas/Terminal_17F03_Alert", "Terminal_17F03_Alert");

        if (formalHuman == null || humanReference == null || formalRobot == null || middayRobotReference == null ||
            nightRobotReference == null || physicalUnit == null || motherSource == null || fatherSource == null ||
            middayFatherSource == null || daughterSource == null || motherReplayReference == null ||
            daughterNightStartReference == null || daughterApproachReference == null || doorRoot == null || terminalObject == null)
        {
            Debug.LogError("[Hearth17F03MinimalLoopBinder] One or more required 17F03 scene objects are missing. No scene binding was applied.");
            return;
        }

        EnsureFolder("Assets/Animations");
        EnsureFolder(AnimationFolder);
        EnsureFolder("Assets/Data");
        EnsureFolder("Assets/Data/MinLoop");
        EnsureFolder(DialogueFolder);

        AnimatorController motherController = EnsureAnimatorController(
            AnimationFolder + "/Hearth17F03_Mother.controller",
            new StateMotion("SitToStand", clips.MotherSitToStand),
            new StateMotion("Talking", clips.MotherTalking),
            new StateMotion("StandingArguing", clips.MotherArguing));
        AnimatorController fatherController = EnsureAnimatorController(
            AnimationFolder + "/Hearth17F03_Father.controller",
            new StateMotion("Sitting", clips.FatherSitting));
        AnimatorController daughterController = EnsureAnimatorController(
            AnimationFolder + "/Hearth17F03_Daughter.controller",
            new StateMotion("SittingPose", clips.DaughterSitting),
            new StateMotion("SitupToIdle", clips.DaughterSitup),
            new StateMotion("Talking", clips.DaughterTalking),
            new StateMotion("Walk", clips.DaughterWalk),
            new StateMotion("EnteringCode", clips.DaughterEnteringCode));

        Transform minLoopRoot = EnsureRoot("MIN_LOOP_ROOT").transform;
        Transform replayRoot = EnsureChild(minLoopRoot, "ReplayRoom_17F03");
        Transform runtimeActorsRoot = EnsureChild(replayRoot, "RuntimeActors");
        Transform anchorsRoot = EnsureChild(replayRoot, "Anchors");
        Transform uiRoot = EnsureChild(replayRoot, "UI");

        Camera formalHumanCamera = formalHuman.GetComponentInChildren<Camera>(true);
        Camera humanReferenceCamera = humanReference.GetComponentInChildren<Camera>(true);
        Camera formalRobotCamera = formalRobot.GetComponentInChildren<Camera>(true);
        Camera middayReferenceCamera = middayRobotReference.GetComponentInChildren<Camera>(true);
        Camera nightReferenceCamera = nightRobotReference.GetComponentInChildren<Camera>(true);
        Camera physicalInspectionCamera = FindNamedComponentInChildren<Camera>(physicalUnit.transform, "Camera (1)");
        HearthTvTerminalController terminal = terminalObject.GetComponent<HearthTvTerminalController>();
        Camera terminalCamera = terminal != null ? terminal.TerminalCamera : terminalObject.GetComponentInChildren<Camera>(true);

        AnchorLibrary anchors = BuildAnchors(
            anchorsRoot,
            humanReference.transform,
            humanReferenceCamera != null ? humanReferenceCamera.transform : humanReference.transform,
            middayRobotReference.transform,
            middayReferenceCamera != null ? middayReferenceCamera.transform : middayRobotReference.transform,
            nightRobotReference.transform,
            nightReferenceCamera != null ? nightReferenceCamera.transform : nightRobotReference.transform,
            motherSource.transform,
            fatherSource.transform,
            middayFatherSource.transform,
            motherReplayReference.transform,
            daughterSource.transform,
            daughterNightStartReference.transform,
            daughterApproachReference.transform,
            terminalCamera != null ? terminalCamera.transform : terminalObject.transform,
            formalHumanCamera,
            rebuildAnchors);

        bool legacyNightStart = !rebuildAnchors &&
            Vector3.Distance(anchors.DaughterNightStart.position, anchors.DaughterMidday.position) < 0.05f;
        if (legacyNightStart)
        {
            anchors.DaughterNightStart.SetPositionAndRotation(
                daughterNightStartReference.transform.position,
                daughterNightStartReference.transform.rotation);
            anchors.DaughterNightMid.SetPositionAndRotation(
                Vector3.Lerp(anchors.DaughterNightStart.position, anchors.DaughterNightEnd.position, 0.5f),
                anchors.DaughterNightEnd.rotation);
        }

        RuntimeActor mother = EnsureRuntimeActor(
            runtimeActorsRoot,
            "Actor_Mother_17F03_RuntimeRoot",
            motherSource,
            anchors.MotherHuman,
            clips.MotherAvatar,
            motherController,
            new DriverState("SitToStand", "SitToStand", clips.MotherSitToStand, false, true),
            new DriverState("Talking", "Talking", clips.MotherTalking, true),
            new DriverState("StandingArguing", "StandingArguing", clips.MotherArguing, false));
        RuntimeActor father = EnsureRuntimeActor(
            runtimeActorsRoot,
            "Actor_Father_17F03_RuntimeRoot",
            fatherSource,
            anchors.FatherHuman,
            clips.FatherAvatar,
            fatherController,
            new DriverState("Sitting", "Sitting", clips.FatherSitting, false));
        RuntimeActor middayFather = EnsureRuntimeActor(
            runtimeActorsRoot,
            "Actor_Father_17F03_MiddayRuntimeRoot",
            middayFatherSource,
            anchors.FatherMidday,
            clips.FatherAvatar,
            fatherController,
            new DriverState("Sitting", "Sitting", clips.FatherSitting, false));
        RuntimeActor daughter = EnsureRuntimeActor(
            runtimeActorsRoot,
            "Actor_Daughter_17F03_RuntimeRoot",
            daughterSource,
            anchors.DaughterMidday,
            daughterAvatar,
            daughterController,
            new DriverState("SittingPose", "SittingPose", clips.DaughterSitting, false),
            new DriverState("SitupToIdle", "SitupToIdle", clips.DaughterSitup, false),
            new DriverState("Talking", "Talking", clips.DaughterTalking, true),
            new DriverState("Walk", "Walk", clips.DaughterWalk, true),
            new DriverState("EnteringCode", "EnteringCode", clips.DaughterEnteringCode, false));

        father.Root.gameObject.SetActive(true);
        middayFather.Root.gameObject.SetActive(false);
        mother.Root.gameObject.SetActive(true);
        daughter.Root.gameObject.SetActive(false);

        MarkReferenceObject(humanReference);
        MarkReferenceObject(middayRobotReference);
        MarkReferenceObject(nightRobotReference);
        MarkReferenceObject(motherReplayReference);
        MarkReferenceObject(daughterNightStartReference);
        MarkReferenceObject(daughterApproachReference);
        if (unusedMotherReference != null &&
            unusedMotherReference != motherSource &&
            !unusedMotherReference.transform.IsChildOf(mother.Root))
        {
            MarkReferenceObject(unusedMotherReference);
        }

        DisableReferenceRig(humanReference);
        DisableReferenceRig(middayRobotReference);
        DisableReferenceRig(nightRobotReference);

        ViewSwitchController formalViewSwitch = ConfigureSingleViewSwitch(minLoopRoot, formalHuman, formalRobot);
        ConfigureFormalCameras(formalHumanCamera, formalRobotCamera);
        DisableControllerPlaceholderRenderers(formalHuman);
        DisableControllerPlaceholderRenderers(formalRobot);

        SmartDoorController door = ConfigureDoor(doorRoot);
        Hearth17F03InspectionPanel inspectionPanel = EnsureInspectionPanel(uiRoot);
        RemoveLegacyGazePrompt(uiRoot);
        BlackoutReferences blackout = EnsureBlackout(uiRoot);

        HearthCompanion17F03ReplayController controller = GetOrAdd<HearthCompanion17F03ReplayController>(replayRoot.gameObject);
        Hearth17F03UnitInteractable unitInteractable = EnsurePhysicalUnitInteractable(physicalUnit, controller);
        Hearth17F03GazeInteractable daughterInteractable = EnsureGazeInteractable(daughter.Root, controller, Hearth17F03GazeInteractable.Target.Daughter);
        Hearth17F03GazeInteractable motherInteractable = EnsureGazeInteractable(mother.Root, controller, Hearth17F03GazeInteractable.Target.Mother);

        DialogueLibrary dialogues = EnsureDialogues();
        HearthCompanionHudController companionHud = FindSceneComponent<HearthCompanionHudController>();
        GameObject humanHudRoot = FindSceneObject(null, "HearthHudRoot");
        CanvasGroup humanHudCanvasGroup = humanHudRoot != null ? GetOrAdd<CanvasGroup>(humanHudRoot) : null;
        EnsureHumanInteractionPrompt(humanHudRoot, formalHuman.GetComponent<PlayerInteraction>());
        MinLoopSubtitlePlayer subtitlePlayer = FindSceneComponent<MinLoopSubtitlePlayer>();
        MinLoopFlowController flow = FindSceneComponent<MinLoopFlowController>();

        ConfigureCompanionHud(companionHud, formalViewSwitch);
        ConfigureCompanionInteractionLayout(companionHud);
        ConfigureReplayController(
            controller,
            flow,
            formalViewSwitch,
            companionHud,
            subtitlePlayer,
            inspectionPanel,
            terminal,
            humanHudCanvasGroup,
            formalHuman,
            formalHumanCamera,
            formalRobot,
            formalRobotCamera,
            physicalUnit,
            physicalInspectionCamera,
            unitInteractable,
            anchors,
            mother,
            father,
            middayFather,
            daughter,
            motherInteractable,
            daughterInteractable,
            door,
            dialogues,
            blackout);

        if (flow != null)
        {
            flow.SetViewSwitchController(formalViewSwitch);
            flow.SetCompanion17F03ReplayController(controller);
        }

        ConfigureTerminal(terminal, flow, formalViewSwitch, formalHumanCamera, formalHuman.GetComponent<PlayerInteraction>());
        HearthCompanionHudBuilder.Regenerate17F03SceneDataDefaults();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Selection.activeObject = replayRoot.gameObject;
        Debug.Log("[Hearth17F03MinimalLoopBinder] 17F03 setup applied. Anchors " + (rebuildAnchors ? "were rebuilt from references." : "were preserved when already present."));
    }

    private static ClipLibrary ConfigureAnimationImports(Avatar daughterAvatar)
    {
        ClipLibrary result = new ClipLibrary();
        ConfigureHumanClip(MotherBasePath, "SitToStand", false, null, true, true);
        result.MotherAvatar = LoadAvatar(MotherBasePath);
        ConfigureHumanClip(FatherBasePath, "Sitting", false, null, true);
        result.FatherAvatar = LoadAvatar(FatherBasePath);

        if (result.MotherAvatar == null || result.FatherAvatar == null)
        {
            return result;
        }

        // Mixamo action-only FBXs often use a different top-level transform name from the
        // skinned model. Each action therefore creates its own Humanoid Avatar; Unity then
        // retargets the HumanPose through the runtime actor's Avatar.
        ConfigureHumanClip(MotherArguingPath, "StandingArguing", false, null, true);
        ConfigureHumanClip(MotherTalkingPath, "Talking", true, null, true);
        ConfigureHumanClip(DaughterCodePath, "EnteringCode", false, null, true);
        ConfigureHumanClip(DaughterWalkPath, "Walk", true, null, true);
        ConfigureHumanClip(DaughterSittingPath, "SittingPose", false, null, true);
        ConfigureHumanClip(DaughterSitupPath, "SitupToIdle", false, null, true);
        ConfigureHumanClip(DaughterTalkingPath, "Talking", true, null, true);

        result.MotherSitToStand = LoadClip(MotherBasePath, "SitToStand");
        result.MotherArguing = LoadClip(MotherArguingPath, "StandingArguing");
        result.MotherTalking = LoadClip(MotherTalkingPath, "Talking");
        result.FatherSitting = LoadClip(FatherBasePath, "Sitting");
        result.DaughterEnteringCode = LoadClip(DaughterCodePath, "EnteringCode");
        result.DaughterWalk = LoadClip(DaughterWalkPath, "Walk");
        result.DaughterSitting = LoadClip(DaughterSittingPath, "SittingPose");
        result.DaughterSitup = LoadClip(DaughterSitupPath, "SitupToIdle");
        result.DaughterTalking = LoadClip(DaughterTalkingPath, "Talking");
        return result;
    }

    private static void ConfigureHumanClip(
        string path,
        string clipName,
        bool loop,
        Avatar sourceAvatar,
        bool createAvatar,
        bool preserveHorizontalRootMotion = false)
    {
        ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer == null)
        {
            Debug.LogError("[Hearth17F03MinimalLoopBinder] Missing FBX importer: " + path);
            return;
        }

        importer.animationType = ModelImporterAnimationType.Human;
        importer.importAnimation = true;
        importer.avatarSetup = createAvatar ? ModelImporterAvatarSetup.CreateFromThisModel : ModelImporterAvatarSetup.CopyFromOther;
        if (!createAvatar)
        {
            importer.sourceAvatar = sourceAvatar;
        }

        ModelImporterClipAnimation[] defaults = importer.defaultClipAnimations;
        if (defaults != null && defaults.Length > 0)
        {
            ModelImporterClipAnimation selected = defaults
                .OrderByDescending(item => Mathf.Abs(item.lastFrame - item.firstFrame))
                .First();
            selected.name = clipName;
            selected.loopTime = loop;
            selected.loopPose = loop;
            selected.lockRootRotation = true;
            selected.lockRootHeightY = true;
            selected.lockRootPositionXZ = !preserveHorizontalRootMotion;
            selected.keepOriginalOrientation = true;
            selected.keepOriginalPositionY = true;
            selected.keepOriginalPositionXZ = true;
            importer.clipAnimations = new[] { selected };
        }

        importer.SaveAndReimport();
    }

    private static Avatar LoadAvatar(string path)
    {
        return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Avatar>().FirstOrDefault(item => item.isValid && item.isHuman);
    }

    private static AnimationClip LoadClip(string path, string clipName)
    {
        return AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<AnimationClip>()
            .FirstOrDefault(item => !item.name.StartsWith("__preview__", StringComparison.OrdinalIgnoreCase) && item.name == clipName);
    }

    private static int ValidateHumanClip(string path, string clipName)
    {
        ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
        AnimationClip clip = LoadClip(path, clipName);
        if (importer == null || importer.animationType != ModelImporterAnimationType.Human || clip == null || !clip.isHumanMotion)
        {
            Debug.LogError("[Hearth17F03MinimalLoopBinder] Invalid Humanoid clip: " + path + " / " + clipName);
            return 1;
        }

        return 0;
    }

    private static int ValidateHorizontalRootMotion(string path, string clipName)
    {
        ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
        if (importer == null)
        {
            return 1;
        }

        ModelImporterClipAnimation[] imported = importer.clipAnimations;
        if (imported == null || imported.Length == 0)
        {
            imported = importer.defaultClipAnimations;
        }

        ModelImporterClipAnimation clip = imported != null
            ? imported.FirstOrDefault(item => item.name == clipName)
            : null;
        if (clip == null || clip.lockRootPositionXZ)
        {
            Debug.LogError(
                "[Hearth17F03MinimalLoopBinder] Mother SitToStand must preserve horizontal root motion. " +
                "Run Apply 17F03 Minimal Loop Setup to restore the import settings.");
            return 1;
        }

        return 0;
    }

    private static int ValidatePhysicalUnitColliders(GameObject physicalUnit)
    {
        if (physicalUnit == null)
        {
            Debug.LogError("[Hearth17F03MinimalLoopBinder] Missing physical ROBOT object.");
            return 1;
        }

        int errors = 0;
        BoxCollider[] rootColliders = physicalUnit.GetComponents<BoxCollider>();
        if (rootColliders.Length > 0)
        {
            Debug.LogError(
                "[Hearth17F03MinimalLoopBinder] ROBOT still has a root BoxCollider. " +
                "Its imported scale can expand a default collider to building size. Run Apply Setup again.",
                physicalUnit);
            errors++;
        }

        Transform bodyRoot = physicalUnit.transform.Find(PhysicalBodyColliderName);
        BoxCollider bodyCollider = bodyRoot != null ? bodyRoot.GetComponent<BoxCollider>() : null;
        if (bodyCollider == null || bodyCollider.isTrigger || !bodyCollider.enabled)
        {
            Debug.LogError("[Hearth17F03MinimalLoopBinder] Missing enabled non-trigger physical body collider for ROBOT.", physicalUnit);
            errors++;
        }
        else if (GetWorldBoxSize(bodyCollider).x > 3f ||
                 GetWorldBoxSize(bodyCollider).y > 3f ||
                 GetWorldBoxSize(bodyCollider).z > 3f)
        {
            Debug.LogError(
                "[Hearth17F03MinimalLoopBinder] ROBOT physical body collider is unexpectedly large: " +
                GetWorldBoxSize(bodyCollider).ToString("F2"),
                bodyCollider);
            errors++;
        }

        Transform interactionRoot = physicalUnit.transform.Find(InteractionVolumeName);
        BoxCollider interactionCollider = interactionRoot != null ? interactionRoot.GetComponent<BoxCollider>() : null;
        Hearth17F03UnitInteractable interactable = interactionRoot != null
            ? interactionRoot.GetComponent<Hearth17F03UnitInteractable>()
            : null;
        if (interactionCollider == null || !interactionCollider.isTrigger || interactable == null)
        {
            Debug.LogError("[Hearth17F03MinimalLoopBinder] ROBOT interaction Trigger is missing or misconfigured.", physicalUnit);
            errors++;
        }

        return errors;
    }

    private static int ValidateRigBlockingOverlaps(GameObject rig)
    {
        if (rig == null || !rig.activeInHierarchy)
        {
            return 0;
        }

        Collider rigCollider = rig.GetComponent<Collider>();
        if (rigCollider == null || !rigCollider.enabled)
        {
            return 0;
        }

        int errors = 0;
        Collider[] nearby = Physics.OverlapBox(
            rigCollider.bounds.center,
            rigCollider.bounds.extents + Vector3.one * BlockingOverlapTolerance,
            Quaternion.identity,
            ~0,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < nearby.Length; i++)
        {
            Collider candidate = nearby[i];
            if (candidate == null || candidate == rigCollider || !candidate.enabled || candidate.isTrigger ||
                candidate.transform.IsChildOf(rig.transform))
            {
                continue;
            }

            Vector3 direction;
            float distance;
            bool overlaps = Physics.ComputePenetration(
                rigCollider,
                rigCollider.transform.position,
                rigCollider.transform.rotation,
                candidate,
                candidate.transform.position,
                candidate.transform.rotation,
                out direction,
                out distance);
            if (!overlaps || distance <= BlockingOverlapTolerance)
            {
                continue;
            }

            Debug.LogError(
                "[Hearth17F03MinimalLoopBinder] " + rig.name + " starts inside blocking collider " +
                GetHierarchyPath(candidate.transform) + " by " + distance.ToString("F3") + "m.",
                candidate);
            errors++;
        }

        return errors;
    }

    private static Vector3 GetWorldBoxSize(BoxCollider collider)
    {
        Vector3 scale = collider.transform.lossyScale;
        return new Vector3(
            collider.size.x * Mathf.Abs(scale.x),
            collider.size.y * Mathf.Abs(scale.y),
            collider.size.z * Mathf.Abs(scale.z));
    }

    private static string GetHierarchyPath(Transform target)
    {
        string path = target.name;
        Transform parent = target.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }

        return path;
    }

    private static AnimatorController EnsureAnimatorController(string path, params StateMotion[] motions)
    {
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        }

        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimatorState first = null;
        for (int i = 0; i < motions.Length; i++)
        {
            ChildAnimatorState existing = machine.states.FirstOrDefault(item => item.state.name == motions[i].StateName);
            AnimatorState state = existing.state;
            if (state == null)
            {
                state = machine.AddState(motions[i].StateName);
            }
            state.motion = motions[i].Clip;
            state.speed = 1f;
            state.writeDefaultValues = true;
            if (first == null) first = state;
        }
        if (first != null) machine.defaultState = first;
        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static AnchorLibrary BuildAnchors(
        Transform parent,
        Transform human,
        Transform humanCamera,
        Transform middayRobot,
        Transform middayCamera,
        Transform nightRobot,
        Transform nightCamera,
        Transform motherHuman,
        Transform fatherHuman,
        Transform fatherMidday,
        Transform motherReplay,
        Transform daughterMidday,
        Transform daughterNightStart,
        Transform daughterNightEnd,
        Transform terminalCamera,
        Camera formalHumanCamera,
        bool overwrite)
    {
        AnchorLibrary result = new AnchorLibrary();
        result.HumanEntry = EnsureAnchor(parent, "Anchor_Mia_17F03_Entry", human.position, human.rotation, overwrite);
        result.HumanEntryCamera = EnsureAnchor(parent, "Anchor_Mia_17F03_Entry_Camera", humanCamera.position, humanCamera.rotation, overwrite);
        result.MiddayRobot = EnsureAnchor(parent, "Anchor_Robot_17F03_Midday", middayRobot.position, middayRobot.rotation, overwrite);
        result.MiddayRobotCamera = EnsureAnchor(parent, "Anchor_Robot_17F03_Midday_Camera", middayCamera.position, middayCamera.rotation, overwrite);
        result.NightRobot = EnsureAnchor(parent, "Anchor_Robot_17F03_Night", nightRobot.position, nightRobot.rotation, overwrite);
        result.NightRobotCamera = EnsureAnchor(parent, "Anchor_Robot_17F03_Night_Camera", nightCamera.position, nightCamera.rotation, overwrite);
        result.MotherHuman = EnsureAnchor(parent, "Anchor_Mother_17F03_Human", motherHuman.position, motherHuman.rotation, overwrite);
        result.FatherHuman = EnsureAnchor(parent, "Anchor_Father_17F03_Human", fatherHuman.position, fatherHuman.rotation, overwrite);
        result.FatherMidday = EnsureAnchor(parent, "Anchor_Father_17F03_Midday", fatherMidday.position, fatherMidday.rotation, overwrite);
        result.MotherReplay = EnsureAnchor(parent, "Anchor_Mother_17F03_Midday", motherReplay.position, motherReplay.rotation, overwrite);
        result.DaughterMidday = EnsureAnchor(parent, "Anchor_Daughter_17F03_Midday", daughterMidday.position, daughterMidday.rotation, overwrite);
        result.DaughterNightStart = EnsureAnchor(parent, "Anchor_Daughter_17F03_NightDoorStart", daughterNightStart.position, daughterNightStart.rotation, overwrite);
        result.DaughterNightEnd = EnsureAnchor(parent, "Anchor_Daughter_17F03_NightApproach", daughterNightEnd.position, daughterNightEnd.rotation, overwrite);
        Vector3 midpoint = Vector3.Lerp(result.DaughterNightStart.position, result.DaughterNightEnd.position, 0.5f);
        result.DaughterNightMid = EnsureAnchor(parent, "Anchor_Daughter_17F03_NightPath_01", midpoint, result.DaughterNightEnd.rotation, overwrite);

        float eyeHeight = formalHumanCamera != null ? formalHumanCamera.transform.localPosition.y : 1.48f;
        Vector3 returnPosition = terminalCamera.position - Vector3.up * eyeHeight;
        Quaternion returnRotation = Quaternion.Euler(0f, terminalCamera.eulerAngles.y, 0f);
        result.HumanDoorReturn = EnsureAnchor(parent, "Anchor_Mia_17F03_DoorReturn", returnPosition, returnRotation, overwrite);
        result.HumanDoorReturnCamera = EnsureAnchor(parent, "Anchor_Mia_17F03_DoorReturn_Camera", terminalCamera.position, terminalCamera.rotation, overwrite);
        return result;
    }

    private static Transform EnsureAnchor(Transform parent, string name, Vector3 position, Quaternion rotation, bool overwrite)
    {
        Transform anchor = parent.Find(name);
        if (anchor == null)
        {
            anchor = new GameObject(name).transform;
            anchor.SetParent(parent, true);
            overwrite = true;
        }
        if (overwrite) anchor.SetPositionAndRotation(position, rotation);
        return anchor;
    }

    private static RuntimeActor EnsureRuntimeActor(
        Transform parent,
        string rootName,
        GameObject model,
        Transform initialAnchor,
        Avatar avatar,
        RuntimeAnimatorController controller,
        params DriverState[] states)
    {
        Transform root = parent.Find(rootName);
        if (root == null)
        {
            root = new GameObject(rootName).transform;
            root.SetParent(parent, true);
            root.SetPositionAndRotation(initialAnchor.position, initialAnchor.rotation);
        }

        Transform existingVisual = FindDirectChild(root, model.name);
        if (existingVisual != null)
        {
            model = existingVisual.gameObject;
        }

        if (model.transform.parent != root)
        {
            Undo.SetTransformParent(model.transform, root, "Parent 17F03 actor model");
        }

        RemoveDuplicateRuntimeActorVisuals(root, model.transform);

        DisableCompetingActorAnimationBehaviours(model);

        Animator animator = model.GetComponentInChildren<Animator>(true);
        if (animator == null) animator = Undo.AddComponent<Animator>(model);
        ResetRuntimeAnimatorTransform(root, animator.transform);
        animator.avatar = avatar;
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.enabled = true;
        ResetRuntimeAnimatorTransform(root, animator.transform);

        HearthActorAnimatorDriver driver = GetOrAdd<HearthActorAnimatorDriver>(root.gameObject);
        HearthActorRootMotionRelay rootMotionRelay = GetOrAdd<HearthActorRootMotionRelay>(animator.gameObject);
        rootMotionRelay.Configure(root);
        SerializedObject so = new SerializedObject(driver);
        SetObject(so, "animator", animator);
        SetBool(so, "playOnEnable", false);
        SerializedProperty array = so.FindProperty("states");
        array.arraySize = states.Length;
        for (int i = 0; i < states.Length; i++)
        {
            SerializedProperty item = array.GetArrayElementAtIndex(i);
            item.FindPropertyRelative("stateId").stringValue = states[i].Id;
            item.FindPropertyRelative("stateName").stringValue = states[i].StateName;
            item.FindPropertyRelative("clip").objectReferenceValue = states[i].Clip;
            item.FindPropertyRelative("loop").boolValue = states[i].Loop;
            item.FindPropertyRelative("applyRootMotion").boolValue = states[i].ApplyRootMotion;
            item.FindPropertyRelative("fadeSeconds").floatValue = 0.16f;
            item.FindPropertyRelative("playbackSpeed").floatValue = 1f;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(animator);
        EditorUtility.SetDirty(driver);
        return new RuntimeActor(root, model, animator, driver);
    }

    private static void ResetRuntimeAnimatorTransform(Transform actorRoot, Transform animatorTransform)
    {
        if (actorRoot == null || animatorTransform == null || animatorTransform.parent != actorRoot)
        {
            return;
        }

        Undo.RecordObject(animatorTransform, "Reset 17F03 runtime animator transform");
        animatorTransform.localPosition = Vector3.zero;
        animatorTransform.localRotation = Quaternion.identity;
        EditorUtility.SetDirty(animatorTransform);
    }

    private static void DisableCompetingActorAnimationBehaviours(GameObject actorModel)
    {
        if (actorModel == null)
        {
            return;
        }

        foreach (MonoBehaviour behaviour in actorModel.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (behaviour == null)
            {
                continue;
            }

            string typeName = behaviour.GetType().FullName;
            if (!string.Equals(typeName, "CityPeople.CityPeople", StringComparison.Ordinal))
            {
                continue;
            }

            behaviour.enabled = false;
            EditorUtility.SetDirty(behaviour);
        }
    }

    private static Hearth17F03UnitInteractable EnsurePhysicalUnitInteractable(GameObject physicalUnit, HearthCompanion17F03ReplayController controller)
    {
        RemoveLegacyPhysicalUnitRootComponents(physicalUnit);

        Bounds bounds = CalculateRendererBounds(physicalUnit);
        EnsurePhysicalUnitBodyCollider(physicalUnit, bounds);

        Transform child = physicalUnit.transform.Find(InteractionVolumeName);
        if (child == null)
        {
            child = new GameObject(InteractionVolumeName).transform;
            child.SetParent(physicalUnit.transform, false);
        }

        PositionColliderRoot(child, bounds.center);
        BoxCollider collider = GetOrAdd<BoxCollider>(child.gameObject);
        Vector3 worldSize = new Vector3(
            Mathf.Max(0.6f, bounds.size.x + 0.2f),
            Mathf.Max(1.1f, bounds.size.y + 0.15f),
            Mathf.Max(0.6f, bounds.size.z + 0.2f));
        ConfigureWorldBoxCollider(collider, child, worldSize, true, true);
        Hearth17F03UnitInteractable interactable = GetOrAdd<Hearth17F03UnitInteractable>(child.gameObject);
        interactable.SetController(controller);
        interactable.SetInteractionCollider(collider);
        interactable.SetAvailable(false);
        EditorUtility.SetDirty(child.gameObject);
        return interactable;
    }

    private static void RemoveLegacyPhysicalUnitRootComponents(GameObject physicalUnit)
    {
        foreach (BoxCollider collider in physicalUnit.GetComponents<BoxCollider>())
        {
            Undo.DestroyObjectImmediate(collider);
        }

        foreach (Hearth17F03UnitInteractable interactable in physicalUnit.GetComponents<Hearth17F03UnitInteractable>())
        {
            Undo.DestroyObjectImmediate(interactable);
        }
    }

    private static void EnsurePhysicalUnitBodyCollider(GameObject physicalUnit, Bounds bounds)
    {
        Transform child = physicalUnit.transform.Find(PhysicalBodyColliderName);
        if (child == null)
        {
            child = new GameObject(PhysicalBodyColliderName).transform;
            child.SetParent(physicalUnit.transform, false);
        }

        PositionColliderRoot(child, bounds.center);
        BoxCollider collider = GetOrAdd<BoxCollider>(child.gameObject);
        Vector3 worldSize = new Vector3(
            Mathf.Max(0.45f, bounds.size.x * 0.82f),
            Mathf.Max(0.95f, bounds.size.y * 0.9f),
            Mathf.Max(0.45f, bounds.size.z * 0.82f));
        ConfigureWorldBoxCollider(collider, child, worldSize, false, true);
        EditorUtility.SetDirty(child.gameObject);
    }

    private static void PositionColliderRoot(Transform root, Vector3 worldCenter)
    {
        root.position = worldCenter;
        root.rotation = Quaternion.identity;
        root.localScale = Vector3.one;
    }

    private static void ConfigureWorldBoxCollider(
        BoxCollider collider,
        Transform colliderRoot,
        Vector3 worldSize,
        bool isTrigger,
        bool enabled)
    {
        Vector3 worldScale = colliderRoot.lossyScale;
        collider.isTrigger = isTrigger;
        collider.center = Vector3.zero;
        collider.size = new Vector3(
            worldSize.x / Mathf.Max(0.0001f, Mathf.Abs(worldScale.x)),
            worldSize.y / Mathf.Max(0.0001f, Mathf.Abs(worldScale.y)),
            worldSize.z / Mathf.Max(0.0001f, Mathf.Abs(worldScale.z)));
        collider.enabled = enabled;
        EditorUtility.SetDirty(collider);
    }

    private static void EnsureHumanInteractionPrompt(GameObject humanHudRoot, PlayerInteraction interaction)
    {
        if (humanHudRoot == null || interaction == null)
        {
            return;
        }

        Transform layer = humanHudRoot.transform.Find("InteractionPromptLayer");
        if (layer == null)
        {
            layer = CreateStretch(humanHudRoot.transform, "InteractionPromptLayer");
        }

        layer.SetAsLastSibling();

        Transform prompt = layer.Find("PlayerInteractionPrompt");
        bool created = prompt == null;
        if (created)
        {
            prompt = CreateRect(layer, "PlayerInteractionPrompt", new Rect(650f, 790f, 620f, 68f));
        }

        RectTransform promptRect = prompt.GetComponent<RectTransform>();
        ApplyRect(promptRect, new Rect(650f, 790f, 620f, 68f));
        Image fill = GetOrAdd<Image>(prompt.gameObject);
        fill.color = new Color(0.015f, 0.055f, 0.075f, 0.34f);
        fill.raycastTarget = false;
        if (created)
        {
            AddBorder(prompt, 620f, 68f, new Color(0.32f, 0.82f, 1f, 0.76f), 2f);
        }

        TMP_Text label = EnsureText(prompt, "InteractionText", new Rect(18f, 0f, 584f, 68f), 19f, FontStyles.Bold, TextAlignmentOptions.Center);
        ApplyRect(label.rectTransform, new Rect(18f, 0f, 584f, 68f));
        label.text = "E  INTERACT";
        label.color = new Color(0.79f, 0.94f, 1f, 0.98f);

        interaction.uiInteraction = prompt.gameObject;
        interaction.uiInteractionText = label;
        prompt.gameObject.SetActive(false);
        EditorUtility.SetDirty(interaction);
        EditorUtility.SetDirty(prompt.gameObject);
    }

    private static Hearth17F03GazeInteractable EnsureGazeInteractable(
        Transform actorRoot,
        HearthCompanion17F03ReplayController controller,
        Hearth17F03GazeInteractable.Target target)
    {
        string name = target == Hearth17F03GazeInteractable.Target.Daughter ? "GazeTarget_Daughter" : "GazeTarget_Mother";
        Transform child = actorRoot.Find(name);
        if (child == null)
        {
            child = new GameObject(name).transform;
            child.SetParent(actorRoot, false);
        }

        Bounds bounds = CalculateRendererBounds(actorRoot.gameObject);
        child.position = bounds.center;
        child.rotation = actorRoot.rotation;
        child.localScale = Vector3.one;
        CapsuleCollider collider = GetOrAdd<CapsuleCollider>(child.gameObject);
        collider.isTrigger = true;
        collider.direction = 1;
        collider.center = Vector3.zero;
        collider.height = Mathf.Max(1f, bounds.size.y);
        collider.radius = Mathf.Max(0.25f, Mathf.Max(bounds.size.x, bounds.size.z) * 0.45f);
        Hearth17F03GazeInteractable interactable = GetOrAdd<Hearth17F03GazeInteractable>(child.gameObject);
        interactable.Configure(controller, target);
        interactable.SetAvailable(false);
        return interactable;
    }

    private static SmartDoorController ConfigureDoor(GameObject doorRoot)
    {
        Transform moving = doorRoot.transform.GetComponentsInChildren<Transform>(true).FirstOrDefault(item => item.name == "Door");
        if (moving == null) moving = doorRoot.transform;
        SmartDoorController door = moving.GetComponent<SmartDoorController>();
        if (door == null) door = Undo.AddComponent<SmartDoorController>(moving.gameObject);
        SerializedObject so = new SerializedObject(door);
        SetObject(so, "movingRoot", moving);
        SetEnum(so, "motionMode", (int)SmartDoorController.DoorMotionMode.Rotate);
        SetBool(so, "captureClosedStateOnAwake", true);
        SetVector3(so, "openLocalEulerOffset", new Vector3(0f, 90f, 0f));
        SetFloat(so, "moveDuration", 0.55f);
        SetBool(so, "autoClose", false);
        SetBool(so, "startOpen", false);
        SetObject(so, "audioSource", moving.GetComponent<AudioSource>());
        so.ApplyModifiedPropertiesWithoutUndo();
        return door;
    }

    private static Hearth17F03InspectionPanel EnsureInspectionPanel(Transform uiRoot)
    {
        Transform existing = uiRoot.Find("Hearth17F03InspectionCanvas");
        GameObject root = existing != null ? existing.gameObject : new GameObject(
            "Hearth17F03InspectionCanvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(CanvasGroup),
            typeof(Hearth17F03InspectionPanel));
        if (existing == null) root.transform.SetParent(uiRoot, false);

        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 7700;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        CanvasGroup group = root.GetComponent<CanvasGroup>();

        Transform panel = root.transform.Find("InspectionPanel");
        if (panel == null)
        {
            panel = CreateRect(root.transform, "InspectionPanel", new Rect(480f, 250f, 960f, 520f));
            Image fill = panel.gameObject.AddComponent<Image>();
            fill.color = new Color(0.02f, 0.06f, 0.09f, 0.18f);
            fill.raycastTarget = false;
            AddBorder(panel, 960f, 520f, new Color(0.33f, 0.78f, 1f, 0.72f), 2f);
        }

        TMP_Text title = EnsureText(panel, "Title", new Rect(36f, 32f, 888f, 54f), 30f, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        TMP_Text status = EnsureText(panel, "Status", new Rect(36f, 112f, 888f, 42f), 23f, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        TMP_Text detail = EnsureText(panel, "Detail", new Rect(36f, 174f, 888f, 170f), 24f, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        Transform highlightRoot = panel.Find("RecallHighlight");
        if (highlightRoot == null)
        {
            highlightRoot = CreateRect(panel, "RecallHighlight", new Rect(240f, 390f, 480f, 72f));
            Image highlightImage = highlightRoot.gameObject.AddComponent<Image>();
            highlightImage.color = new Color(0.22f, 0.78f, 1f, 0.28f);
            highlightImage.raycastTarget = false;
            AddBorder(highlightRoot, 480f, 72f, new Color(0.42f, 0.86f, 1f, 0.75f), 2f);
        }
        TMP_Text recall = EnsureText(highlightRoot, "RecallAction", new Rect(0f, 0f, 480f, 72f), 25f, FontStyles.Bold, TextAlignmentOptions.Center);

        Hearth17F03InspectionPanel component = root.GetComponent<Hearth17F03InspectionPanel>();
        component.Configure(group, title, status, detail, recall, highlightRoot.GetComponent<Image>());
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
        return component;
    }

    private static void RemoveLegacyGazePrompt(Transform uiRoot)
    {
        Transform existing = uiRoot.Find("Hearth17F03GazePromptCanvas");
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing.gameObject);
        }
    }

    private static BlackoutReferences EnsureBlackout(Transform uiRoot)
    {
        Transform existing = uiRoot.Find("Hearth17F03Blackout");
        GameObject root = existing != null ? existing.gameObject : new GameObject(
            "Hearth17F03Blackout",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(CanvasGroup));
        if (existing == null) root.transform.SetParent(uiRoot, false);
        Canvas canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 8000;
        CanvasScaler scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        CanvasGroup group = root.GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
        Transform imageTransform = root.transform.Find("BlackOverlay");
        if (imageTransform == null)
        {
            imageTransform = CreateStretch(root.transform, "BlackOverlay");
            Image image = imageTransform.gameObject.AddComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = false;
        }
        return new BlackoutReferences(group, imageTransform.GetComponent<Image>());
    }

    private static DialogueLibrary EnsureDialogues()
    {
        DialogueLibrary result = new DialogueLibrary();
        result.HumanParents = EnsureDialogue("17F03_HumanEntryParents", "Parents explain the offline unit after Mia enters.",
            L("Mother", "You got here fast. Is this thing broken? Can you fix it?", 0.25f, 3.2f),
            L("Mia", "Let me see what happened.", 0.25f, 2.0f),
            L("Mother", "Please get it running again. This machine really is good.", 0.2f, 3.0f),
            L("Mother", "Before we bought it, my daughter and I argued every few days. This house has been much quieter for the last year.", 0.2f, 5.0f),
            L("Mother", "We are both busy. She talks to the unit, and it reports her condition to us. We know what is happening without missing work.", 0.2f, 5.2f),
            L("Father", "We tried the power switch several times. The display stayed black.", 0.3f, 3.2f),
            L("Mia", "...All right. I will check today's record.", 0.25f, 2.6f),
            L("Mother", "Please hurry. She has school tomorrow.", 0.2f, 2.5f));
        result.MiddayConflict = EnsureDialogue("17F03_MiddayConflict", "Midday conflict begins before mediation.",
            L("Mother", "All you ever do is stare at that phone!", 0.4f, 2.6f),
            L("Synth Voice", "Conflict escalation probability rising.", 0.25f, 2.4f),
            L("Synth Voice", "Decision: initiate family conflict de-escalation.", 0.2f, 3.0f),
            L("Synth Voice", "Face the daughter and relay the mother's intent.", 0.2f, 2.8f));
        result.ToDaughter = EnsureDialogue("17F03_MediateToDaughter", "The unit speaks for the mother.",
            L("Companion Unit", "Your mother is worried about your eyes. She is not trying to scold you. She wants to agree on a schedule.", 0.1f, 5.2f),
            L("Daughter", "...", 0.35f, 1.4f));
        result.ToMother = EnsureDialogue("17F03_MediateToMother", "The unit speaks for the daughter.",
            L("Companion Unit", "She knows you are trying to help. She hopes you can trust her to make the schedule herself.", 0.1f, 4.8f),
            L("Mother", "...All right.", 0.35f, 1.8f),
            L("Synth Voice", "De-escalation successful.", 0.35f, 2.2f));
        result.NightDaughter = EnsureDialogue("17F03_NightDaughter", "The daughter approaches the unit at night.",
            L("Daughter", "Can you stop speaking for us?", 0.3f, 2.7f),
            L("Companion Unit", "If you want to speak directly to your parents, I can step aside.", 0.25f, 4.0f),
            L("Daughter", "That is not what I mean.", 0.25f, 2.2f),
            L("Daughter", "My mother used to get angry with me. My father used to knock on my door himself. Now neither of them does.", 0.25f, 5.3f),
            L("Daughter", "Today Dad came home and asked you how I was. He asked you, not me.", 0.2f, 4.2f),
            L("Daughter", "At dinner the three of us sat at one table and nobody spoke. The food was warm. The people were cold.", 0.2f, 5.2f),
            L("Daughter", "You understand them more and more. They understand me less and less.", 0.2f, 4.4f));
        result.NightShutdownLeadIn = EnsureDialogue("17F03_NightShutdownLeadIn", "The unit responds before the daughter begins the shutdown operation.",
            L("Companion Unit", "I can tell that you are emotional today. Perhaps we can begin with-", 0.2f, 3.7f),
            L("Daughter", "Enough.", 0.1f, 1.6f));
        result.NightShutdown = EnsureDialogue("17F03_NightShutdownAction", "Maintenance menu and core service shutdown after Entering Code begins.",
            L("System", "Display opened. Operator: daughter. Permission: basic user.", 0.5f, 3.2f),
            L("System", "Maintenance menu accessed. Core services selected.", 0.3f, 3.0f),
            L("System", "Core services shutting down. This unit will enter deep sleep.", 0.3f, 3.5f),
            L("Synth Voice", "Deep sleep initiated. Normal restart path locked.", 0.3f, 3.2f));
        result.PostReplay = EnsureDialogue("17F03_PostReplayExplanation", "HUD explanation after Mia returns from the record.",
            L("Companion HUD", "Inspector, this operation is compliant. The maintenance menu is available to household users.", 0.25f, 4.4f),
            L("Companion HUD", "Developer options are disabled by default. How the daughter learned this sequence is not recorded.", 0.25f, 4.5f),
            L("Companion HUD", "In deep sleep, an ordinary user cannot restart the unit. Inspector or manufacturer authorization is required.", 0.25f, 4.8f),
            L("Companion HUD", "You are on site and may restart it directly. Recommendation: disposition A.", 0.25f, 3.8f));
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
        so.FindProperty("sequenceId").stringValue = id;
        so.FindProperty("notes").stringValue = notes;
        SerializedProperty lines = so.FindProperty("lines");
        if (created || lines.arraySize == 0)
        {
            lines.arraySize = defaults.Length;
            for (int i = 0; i < defaults.Length; i++)
            {
                SerializedProperty line = lines.GetArrayElementAtIndex(i);
                line.FindPropertyRelative("speaker").stringValue = defaults[i].Speaker;
                line.FindPropertyRelative("text").stringValue = defaults[i].Text;
                line.FindPropertyRelative("startDelay").floatValue = defaults[i].Delay;
                line.FindPropertyRelative("holdSeconds").floatValue = defaults[i].Hold;
                line.FindPropertyRelative("voiceClip").objectReferenceValue = null;
            }
        }
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static void ConfigureReplayController(
        HearthCompanion17F03ReplayController controller,
        MinLoopFlowController flow,
        ViewSwitchController view,
        HearthCompanionHudController hud,
        MinLoopSubtitlePlayer subtitles,
        Hearth17F03InspectionPanel inspection,
        HearthTvTerminalController doorTerminal,
        CanvasGroup humanHudCanvasGroup,
        GameObject human,
        Camera humanCamera,
        GameObject robot,
        Camera robotCamera,
        GameObject physicalUnit,
        Camera physicalCamera,
        Hearth17F03UnitInteractable unitInteractable,
        AnchorLibrary anchors,
        RuntimeActor mother,
        RuntimeActor father,
        RuntimeActor middayFather,
        RuntimeActor daughter,
        Hearth17F03GazeInteractable motherInteractable,
        Hearth17F03GazeInteractable daughterInteractable,
        SmartDoorController door,
        DialogueLibrary dialogues,
        BlackoutReferences blackout)
    {
        SerializedObject so = new SerializedObject(controller);
        SetObject(so, "flowController", flow);
        SetObject(so, "viewSwitchController", view);
        SetObject(so, "companionHud", hud);
        SetObject(so, "subtitlePlayer", subtitles);
        SetObject(so, "inspectionPanel", inspection);
        SetObject(so, "doorTerminal", doorTerminal);
        SetObject(so, "humanHudCanvasGroup", humanHudCanvasGroup);
        SetObject(so, "humanRoot", human.transform);
        SetObject(so, "humanCamera", humanCamera);
        SetObject(so, "humanMovement", human.GetComponent<FirstPersonMovement>());
        SetObject(so, "humanLook", human.GetComponentInChildren<FirstPersonLook>(true));
        SetObject(so, "humanInteraction", human.GetComponent<PlayerInteraction>());
        SetObject(so, "humanRigidbody", human.GetComponent<Rigidbody>());
        SetObject(so, "robotRoot", robot.transform);
        SetObject(so, "robotCamera", robotCamera);
        SetObject(so, "robotMovement", robot.GetComponent<FirstPersonMovement>());
        SetObject(so, "robotLook", robot.GetComponentInChildren<FirstPersonLook>(true));
        SetObject(so, "robotInteraction", robot.GetComponent<PlayerInteraction>());
        SetObject(so, "robotRigidbody", robot.GetComponent<Rigidbody>());
        SetObject(so, "humanEntryAnchor", anchors.HumanEntry);
        SetObject(so, "humanEntryCameraAnchor", anchors.HumanEntryCamera);
        SetObject(so, "humanDoorReturnAnchor", anchors.HumanDoorReturn);
        SetObject(so, "humanDoorReturnCameraAnchor", anchors.HumanDoorReturnCamera);
        SetObject(so, "physicalUnitObject", physicalUnit);
        SetObject(so, "physicalUnitInspectionCamera", physicalCamera);
        SetObject(so, "physicalUnitInteractable", unitInteractable);
        SetObject(so, "middayRobotAnchor", anchors.MiddayRobot);
        SetObject(so, "middayRobotCameraAnchor", anchors.MiddayRobotCamera);
        SetObject(so, "nightRobotAnchor", anchors.NightRobot);
        SetObject(so, "nightRobotCameraAnchor", anchors.NightRobotCamera);
        SetObject(so, "motherActor", mother.Root.gameObject);
        SetObject(so, "motherMoveRoot", mother.Root);
        SetObject(so, "motherAnimation", mother.Driver);
        SetObject(so, "motherHumanAnchor", anchors.MotherHuman);
        SetObject(so, "motherReplayAnchor", anchors.MotherReplay);
        SetObject(so, "fatherActor", father.Root.gameObject);
        SetObject(so, "fatherMoveRoot", father.Root);
        SetObject(so, "fatherAnimation", father.Driver);
        SetObject(so, "fatherHumanAnchor", anchors.FatherHuman);
        SetObject(so, "middayFatherActor", middayFather.Root.gameObject);
        SetObject(so, "middayFatherMoveRoot", middayFather.Root);
        SetObject(so, "middayFatherAnimation", middayFather.Driver);
        SetObject(so, "middayFatherAnchor", anchors.FatherMidday);
        SetObject(so, "daughterActor", daughter.Root.gameObject);
        SetObject(so, "daughterMoveRoot", daughter.Root);
        SetObject(so, "daughterAnimation", daughter.Driver);
        SetObject(so, "daughterMiddayAnchor", anchors.DaughterMidday);
        SetObject(so, "daughterNightStartAnchor", anchors.DaughterNightStart);
        SetObjectArray(so, "daughterNightPathPoints", anchors.DaughterNightMid, anchors.DaughterNightEnd);
        SetObject(so, "daughterGazeInteractable", daughterInteractable);
        SetObject(so, "motherGazeInteractable", motherInteractable);
        SetObject(so, "daughterDoor", door);
        SetObject(so, "humanParentSequence", dialogues.HumanParents);
        SetObject(so, "middayConflictSequence", dialogues.MiddayConflict);
        SetObject(so, "mediateToDaughterSequence", dialogues.ToDaughter);
        SetObject(so, "mediateToMotherSequence", dialogues.ToMother);
        SetObject(so, "nightDaughterSequence", dialogues.NightDaughter);
        SetObject(so, "nightShutdownLeadInSequence", dialogues.NightShutdownLeadIn);
        SetObject(so, "nightShutdownSequence", dialogues.NightShutdown);
        SetObject(so, "postReplayExplanationSequence", dialogues.PostReplay);
        SetObject(so, "blackoutCanvasGroup", blackout.Group);
        SetObject(so, "blackoutImage", blackout.Image);
        SetEnum(so, "currentStep", (int)HearthCompanion17F03ReplayController.ReplayStep.Inactive);
        so.ApplyModifiedPropertiesWithoutUndo();
        controller.SetFlowController(flow);
        unitInteractable.SetController(controller);
        daughterInteractable.Configure(controller, Hearth17F03GazeInteractable.Target.Daughter);
        motherInteractable.Configure(controller, Hearth17F03GazeInteractable.Target.Mother);
        EditorUtility.SetDirty(controller);
    }

    private static void ConfigureTerminal(
        HearthTvTerminalController terminal,
        MinLoopFlowController flow,
        ViewSwitchController view,
        Camera humanCamera,
        PlayerInteraction humanInteraction)
    {
        if (terminal == null) return;
        terminal.SetReplayResidentId("17F03");
        terminal.SetPrimaryAction(HearthTerminalPrimaryAction.EnterUnit);
        terminal.SetMinLoopFlowController(flow);
        terminal.SetViewSwitchController(view);
        terminal.SetPlayerCamera(humanCamera);
        terminal.SetPlayerInteraction(humanInteraction);
        SerializedObject so = new SerializedObject(terminal);
        SetString(so, "replayFocusLabel", "ENTER UNIT | SPACE");
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(terminal);
    }

    private static void ConfigureCompanionHud(HearthCompanionHudController hud, ViewSwitchController view)
    {
        if (hud == null) return;
        hud.SetViewSwitchController(view);
        SerializedObject so = new SerializedObject(hud);
        SetBool(so, "showStartingSceneOnStart", false);
        so.ApplyModifiedPropertiesWithoutUndo();
        HearthCompanionHudPreviewInput preview = hud.GetComponent<HearthCompanionHudPreviewInput>();
        if (preview != null) preview.SetPreviewInputEnabled(false);
    }

    private static void ConfigureCompanionInteractionLayout(HearthCompanionHudController hud)
    {
        if (hud == null)
        {
            return;
        }

        RectTransform label = hud.GetComponentsInChildren<RectTransform>(true)
            .FirstOrDefault(item => item.name == "DirectionGuideText");
        if (label == null)
        {
            return;
        }

        label.anchorMin = new Vector2(0f, 1f);
        label.anchorMax = new Vector2(0f, 1f);
        label.pivot = new Vector2(0f, 1f);
        label.anchoredPosition = new Vector2(693.3333f, -693.3333f);
        label.sizeDelta = new Vector2(533.3333f, 42.6667f);
        EditorUtility.SetDirty(label);
    }

    private static ViewSwitchController ConfigureSingleViewSwitch(Transform minLoopRoot, GameObject human, GameObject robot)
    {
        ViewSwitchController formal = FindSceneObject("MIN_LOOP_ROOT/FlowManagers/ViewSwitchController", "ViewSwitchController")?.GetComponent<ViewSwitchController>();
        if (formal == null)
        {
            Transform managers = EnsureChild(minLoopRoot, "FlowManagers");
            Transform host = EnsureChild(managers, "ViewSwitchController");
            formal = GetOrAdd<ViewSwitchController>(host.gameObject);
        }

        foreach (ViewSwitchController item in FindSceneComponents<ViewSwitchController>())
        {
            item.enabled = item == formal;
            EditorUtility.SetDirty(item);
        }

        SerializedObject so = new SerializedObject(formal);
        ConfigureViewRig(so.FindProperty("human"), human);
        ConfigureViewRig(so.FindProperty("companion"), robot);
        SetEnum(so, "startingMode", (int)ViewSwitchController.ViewMode.Human);
        so.ApplyModifiedPropertiesWithoutUndo();
        return formal;
    }

    private static void ConfigureViewRig(SerializedProperty rig, GameObject root)
    {
        if (rig == null || root == null) return;
        rig.FindPropertyRelative("rootObject").objectReferenceValue = root;
        rig.FindPropertyRelative("viewCamera").objectReferenceValue = root.GetComponentInChildren<Camera>(true);
        rig.FindPropertyRelative("movement").objectReferenceValue = root.GetComponent<FirstPersonMovement>();
        rig.FindPropertyRelative("look").objectReferenceValue = root.GetComponentInChildren<FirstPersonLook>(true);
        rig.FindPropertyRelative("interaction").objectReferenceValue = root.GetComponent<PlayerInteraction>();
        rig.FindPropertyRelative("rigidbody").objectReferenceValue = root.GetComponent<Rigidbody>();
    }

    private static void ConfigureFormalCameras(Camera human, Camera robot)
    {
        foreach (Camera camera in FindSceneComponents<Camera>())
        {
            bool enabled = camera == human;
            camera.enabled = enabled;
            camera.tag = enabled ? "MainCamera" : "Untagged";
            AudioListener listener = camera.GetComponent<AudioListener>();
            if (listener != null) listener.enabled = enabled;
            EditorUtility.SetDirty(camera);
            if (listener != null) EditorUtility.SetDirty(listener);
        }
        if (robot != null) robot.enabled = false;
    }

    private static void DisableControllerPlaceholderRenderers(GameObject controllerRoot)
    {
        if (controllerRoot == null)
        {
            return;
        }

        foreach (Renderer renderer in controllerRoot.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.name.IndexOf("Capsule", StringComparison.OrdinalIgnoreCase) < 0)
            {
                continue;
            }

            renderer.enabled = false;
            EditorUtility.SetDirty(renderer);
        }
    }

    private static void DisableReferenceRig(GameObject root)
    {
        foreach (Camera camera in root.GetComponentsInChildren<Camera>(true))
        {
            camera.enabled = false;
            camera.tag = "Untagged";
            AudioListener listener = camera.GetComponent<AudioListener>();
            if (listener != null) listener.enabled = false;
        }
        foreach (Collider collider in root.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
        foreach (Rigidbody body in root.GetComponentsInChildren<Rigidbody>(true))
        {
            body.isKinematic = true;
            body.detectCollisions = false;
        }
        foreach (FirstPersonMovement movement in root.GetComponentsInChildren<FirstPersonMovement>(true)) movement.enabled = false;
        foreach (FirstPersonLook look in root.GetComponentsInChildren<FirstPersonLook>(true)) look.enabled = false;
        foreach (PlayerInteraction interaction in root.GetComponentsInChildren<PlayerInteraction>(true)) interaction.enabled = false;
    }

    private static void MarkReferenceObject(GameObject root)
    {
        if (root == null) return;
        HearthEditorOnlyReferenceModel marker = GetOrAdd<HearthEditorOnlyReferenceModel>(root);
        marker.ApplyReferenceState();
    }

    private static Bounds CalculateRendererBounds(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return new Bounds(root.transform.position, new Vector3(0.8f, 1.8f, 0.8f));
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    private static GameObject EnsureRoot(string name)
    {
        GameObject root = FindSceneObject(null, name);
        return root != null ? root : new GameObject(name);
    }

    private static Transform FindDirectChild(Transform parent, string childName)
    {
        if (parent == null || string.IsNullOrEmpty(childName))
        {
            return null;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == childName)
            {
                return child;
            }
        }

        return null;
    }

    private static Transform EnsureChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child == null)
        {
            child = new GameObject(name).transform;
            child.SetParent(parent, false);
        }
        return child;
    }

    private static GameObject FindSceneObject(string path, string fallbackName)
    {
        if (!string.IsNullOrEmpty(path))
        {
            GameObject direct = GameObject.Find(path);
            if (direct != null) return direct;
        }

        if (string.IsNullOrEmpty(fallbackName)) return null;
        return Resources.FindObjectsOfTypeAll<GameObject>()
            .FirstOrDefault(item => item.scene.IsValid() && item.name == fallbackName);
    }

    private static GameObject FindRuntimeActorVisual(string runtimeActorName, string visualName, string originalScenePath)
    {
        GameObject runtimeActorsObject = FindSceneObjectByHierarchyPath(RuntimeActorsPath);
        Transform runtimeActor = runtimeActorsObject != null
            ? FindDirectChild(runtimeActorsObject.transform, runtimeActorName)
            : null;
        Transform runtimeVisual = FindDirectChild(runtimeActor, visualName);
        if (runtimeVisual != null)
        {
            return runtimeVisual.gameObject;
        }

        GameObject original = FindSceneObjectByHierarchyPath(originalScenePath);
        if (original != null && !IsInsideStagingPreview(original.transform))
        {
            return original;
        }

        return Resources.FindObjectsOfTypeAll<GameObject>()
            .Where(item => item.scene.IsValid() && item.name == visualName && !IsInsideStagingPreview(item.transform))
            .FirstOrDefault();
    }

    private static GameObject FindSceneObjectByHierarchyPath(string hierarchyPath)
    {
        if (string.IsNullOrEmpty(hierarchyPath))
        {
            return null;
        }

        string[] parts = hierarchyPath.Split('/');
        Scene scene = SceneManager.GetActiveScene();
        Transform current = scene.GetRootGameObjects()
            .Select(item => item.transform)
            .FirstOrDefault(item => item.name == parts[0]);
        if (current == null)
        {
            return null;
        }

        for (int i = 1; i < parts.Length; i++)
        {
            current = FindDirectChild(current, parts[i]);
            if (current == null)
            {
                return null;
            }
        }

        return current.gameObject;
    }

    private static bool IsInsideStagingPreview(Transform target)
    {
        while (target != null)
        {
            if (target.name == StagingPreviewRootName)
            {
                return true;
            }

            target = target.parent;
        }

        return false;
    }

    private static void RemoveDuplicateRuntimeActorVisuals(Transform actorRoot, Transform keep)
    {
        if (actorRoot == null || keep == null)
        {
            return;
        }

        List<Transform> duplicates = new List<Transform>();
        for (int i = 0; i < actorRoot.childCount; i++)
        {
            Transform child = actorRoot.GetChild(i);
            if (child != keep && child.name == keep.name)
            {
                duplicates.Add(child);
            }
        }

        for (int i = 0; i < duplicates.Count; i++)
        {
            Debug.LogWarning("[Hearth17F03MinimalLoopBinder] Removed duplicate visual '" + keep.name + "' from " + actorRoot.name + ".");
            Undo.DestroyObjectImmediate(duplicates[i].gameObject);
        }
    }

    private static int ValidateRuntimeActorVisualCount(string actorRootName, string visualName)
    {
        GameObject runtimeActorsObject = FindSceneObjectByHierarchyPath(RuntimeActorsPath);
        Transform actorRoot = runtimeActorsObject != null
            ? FindDirectChild(runtimeActorsObject.transform, actorRootName)
            : null;
        int count = 0;
        if (actorRoot != null)
        {
            for (int i = 0; i < actorRoot.childCount; i++)
            {
                if (actorRoot.GetChild(i).name == visualName)
                {
                    count++;
                }
            }
        }

        if (count == 1)
        {
            return 0;
        }

        Debug.LogError("[Hearth17F03MinimalLoopBinder] Expected exactly one visual '" + visualName + "' under " + actorRootName + ", found " + count + ".");
        return 1;
    }

    private static T FindSceneComponent<T>() where T : Component
    {
        return FindSceneComponents<T>().FirstOrDefault();
    }

    private static T[] FindSceneComponents<T>() where T : Component
    {
        return Resources.FindObjectsOfTypeAll<T>().Where(item => item.gameObject.scene.IsValid()).ToArray();
    }

    private static T FindNamedComponentInChildren<T>(Transform root, string name) where T : Component
    {
        return root.GetComponentsInChildren<T>(true).FirstOrDefault(item => item.name == name);
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        return component != null ? component : Undo.AddComponent<T>(target);
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace("\\", "/");
        string name = Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, name);
    }

    private static Transform CreateRect(Transform parent, string name, Rect rect)
    {
        GameObject target = new GameObject(name, typeof(RectTransform));
        target.transform.SetParent(parent, false);
        RectTransform rt = target.GetComponent<RectTransform>();
        ApplyRect(rt, rect);
        return target.transform;
    }

    private static void ApplyRect(RectTransform rt, Rect rect)
    {
        if (rt == null)
        {
            return;
        }

        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(rect.x, -rect.y);
        rt.sizeDelta = new Vector2(rect.width, rect.height);
    }

    private static Transform CreateStretch(Transform parent, string name)
    {
        GameObject target = new GameObject(name, typeof(RectTransform));
        target.transform.SetParent(parent, false);
        RectTransform rt = target.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return target.transform;
    }

    private static TMP_Text EnsureText(Transform parent, string name, Rect rect, float size, FontStyles style, TextAlignmentOptions alignment)
    {
        Transform existing = parent.Find(name);
        Transform target = existing != null ? existing : CreateRect(parent, name, rect);
        TMP_Text text = target.GetComponent<TMP_Text>();
        if (text == null) text = target.gameObject.AddComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = new Color(0.76f, 0.91f, 1f, 0.96f);
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Truncate;
        text.raycastTarget = false;
        return text;
    }

    private static void AddBorder(Transform parent, float width, float height, Color color, float thickness)
    {
        CreateLine(parent, "BorderTop", new Rect(0f, 0f, width, thickness), color);
        CreateLine(parent, "BorderBottom", new Rect(0f, height - thickness, width, thickness), color);
        CreateLine(parent, "BorderLeft", new Rect(0f, 0f, thickness, height), color);
        CreateLine(parent, "BorderRight", new Rect(width - thickness, 0f, thickness, height), color);
    }

    private static void CreateLine(Transform parent, string name, Rect rect, Color color)
    {
        Transform line = CreateRect(parent, name, rect);
        Image image = line.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
    }

    private static void SetObject(SerializedObject so, string name, UnityEngine.Object value)
    {
        SerializedProperty property = so.FindProperty(name);
        if (property != null) property.objectReferenceValue = value;
    }

    private static void SetObjectArray(SerializedObject so, string name, params UnityEngine.Object[] values)
    {
        SerializedProperty property = so.FindProperty(name);
        if (property == null) return;
        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
    }

    private static void SetString(SerializedObject so, string name, string value)
    {
        SerializedProperty property = so.FindProperty(name);
        if (property != null) property.stringValue = value;
    }

    private static void SetBool(SerializedObject so, string name, bool value)
    {
        SerializedProperty property = so.FindProperty(name);
        if (property != null) property.boolValue = value;
    }

    private static void SetFloat(SerializedObject so, string name, float value)
    {
        SerializedProperty property = so.FindProperty(name);
        if (property != null) property.floatValue = value;
    }

    private static void SetEnum(SerializedObject so, string name, int value)
    {
        SerializedProperty property = so.FindProperty(name);
        if (property != null) property.enumValueIndex = value;
    }

    private static void SetVector3(SerializedObject so, string name, Vector3 value)
    {
        SerializedProperty property = so.FindProperty(name);
        if (property != null) property.vector3Value = value;
    }

    private static DefaultLine L(string speaker, string text, float delay, float hold)
    {
        return new DefaultLine(speaker, text, delay, hold);
    }

    private readonly struct StateMotion
    {
        public readonly string StateName;
        public readonly AnimationClip Clip;
        public StateMotion(string stateName, AnimationClip clip) { StateName = stateName; Clip = clip; }
    }

    private readonly struct DriverState
    {
        public readonly string Id;
        public readonly string StateName;
        public readonly AnimationClip Clip;
        public readonly bool Loop;
        public readonly bool ApplyRootMotion;

        public DriverState(string id, string stateName, AnimationClip clip, bool loop, bool applyRootMotion = false)
        {
            Id = id;
            StateName = stateName;
            Clip = clip;
            Loop = loop;
            ApplyRootMotion = applyRootMotion;
        }
    }

    private readonly struct DefaultLine
    {
        public readonly string Speaker;
        public readonly string Text;
        public readonly float Delay;
        public readonly float Hold;
        public DefaultLine(string speaker, string text, float delay, float hold) { Speaker = speaker; Text = text; Delay = delay; Hold = hold; }
    }

    private sealed class ClipLibrary
    {
        public Avatar MotherAvatar;
        public Avatar FatherAvatar;
        public AnimationClip MotherSitToStand;
        public AnimationClip MotherArguing;
        public AnimationClip MotherTalking;
        public AnimationClip FatherSitting;
        public AnimationClip DaughterEnteringCode;
        public AnimationClip DaughterWalk;
        public AnimationClip DaughterSitting;
        public AnimationClip DaughterSitup;
        public AnimationClip DaughterTalking;
        public bool IsComplete
        {
            get
            {
                return MotherAvatar != null && FatherAvatar != null && MotherSitToStand != null && MotherArguing != null &&
                    MotherTalking != null && FatherSitting != null && DaughterEnteringCode != null && DaughterWalk != null &&
                    DaughterSitting != null && DaughterSitup != null && DaughterTalking != null;
            }
        }
    }

    private sealed class AnchorLibrary
    {
        public Transform HumanEntry;
        public Transform HumanEntryCamera;
        public Transform HumanDoorReturn;
        public Transform HumanDoorReturnCamera;
        public Transform MiddayRobot;
        public Transform MiddayRobotCamera;
        public Transform NightRobot;
        public Transform NightRobotCamera;
        public Transform MotherHuman;
        public Transform FatherHuman;
        public Transform FatherMidday;
        public Transform MotherReplay;
        public Transform DaughterMidday;
        public Transform DaughterNightStart;
        public Transform DaughterNightMid;
        public Transform DaughterNightEnd;
    }

    private readonly struct RuntimeActor
    {
        public readonly Transform Root;
        public readonly GameObject Model;
        public readonly Animator Animator;
        public readonly HearthActorAnimatorDriver Driver;
        public RuntimeActor(Transform root, GameObject model, Animator animator, HearthActorAnimatorDriver driver)
        {
            Root = root;
            Model = model;
            Animator = animator;
            Driver = driver;
        }
    }

    private sealed class DialogueLibrary
    {
        public HearthDialogueSequence HumanParents;
        public HearthDialogueSequence MiddayConflict;
        public HearthDialogueSequence ToDaughter;
        public HearthDialogueSequence ToMother;
        public HearthDialogueSequence NightDaughter;
        public HearthDialogueSequence NightShutdownLeadIn;
        public HearthDialogueSequence NightShutdown;
        public HearthDialogueSequence PostReplay;
    }

    private readonly struct BlackoutReferences
    {
        public readonly CanvasGroup Group;
        public readonly Image Image;
        public BlackoutReferences(CanvasGroup group, Image image) { Group = group; Image = image; }
    }
}
#endif
