using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Hearth17F01MinimalLoopBinder
{
    private const string MenuPath = "Tools/Hearth/Replay/Apply 17F01 Minimal Loop Setup";
    private const float BoyInteractionDistance = 0.71f;
    private const float RobotAutoMoveSpeed = 0.45f;
    private const float RobotWalkSpeed = 1.625f;
    private const float RobotRunSpeed = 4.095f;
    private const float SubtitleSpeakerCenterY = 0.31f;
    private const float SubtitleBodyCenterY = 0.22f;
    private const float ProxyRepositionDistance = 2f;
    private const string DialogueFolder = "Assets/Data/MinLoop/Dialogues";
    private const string BedroomPreludeDialoguePath = DialogueFolder + "/17F01_BedroomPrelude.asset";
    private const string SoothingDialoguePath = DialogueFolder + "/17F01_BedsideSoothing.asset";
    private const string LivingRoomDialoguePath = DialogueFolder + "/17F01_LivingRoomObservation.asset";

    [MenuItem(MenuPath)]
    public static void Apply()
    {
        GameObject minLoopRoot = FindOrCreate("MIN_LOOP_ROOT");
        Transform anchorsRoot = FindOrCreateChild(minLoopRoot.transform, "Anchors");
        Transform replayRoot = FindOrCreateChild(minLoopRoot.transform, "ReplayRoom_17F01");

        GameObject person = Find("Player/Person Controller", "Person Controller", "Player_Mia_Controller");
        GameObject robot = Find("Player/Robot Controller", "Robot Controller", "Companion_Controller");
        GameObject robotLiving = Find("Player/Robot Controller (1)", "Robot Controller (1)");
        GameObject robotBedside = Find("Player/Robot Controller (2)", "Robot Controller (2)");

        Transform childStartAnchor = CreateAnchor(anchorsRoot, "Anchor_Robot_17F01_ChildRoomStart", robot != null ? robot.transform : null);
        Transform bedsideAnchor = robotBedside != null ? CreateAnchor(anchorsRoot, "Anchor_Robot_17F01_BedsideInteract", robotBedside.transform) : null;
        Transform livingAnchor = CreateAnchor(anchorsRoot, "Anchor_Robot_17F01_LivingRoomStart", robotLiving != null ? robotLiving.transform : null);
        Transform pathAnchor = robotBedside != null ? CreatePathAnchor(anchorsRoot, childStartAnchor, bedsideAnchor) : null;

        DisableReferenceController(robotLiving);
        DisableReferenceController(robotBedside);

        ViewSwitchController viewSwitch = GetOrAdd<ViewSwitchController>(FindOrCreate("MIN_LOOP_ROOT/FlowManagers/ViewSwitchController"));
        ConfigureViewSwitch(viewSwitch, person, robot);

        MinLoopFlowController flow = GetOrAdd<MinLoopFlowController>(FindOrCreate("MIN_LOOP_ROOT/FlowManagers/MinLoopFlowController"));
        TrustStateController trust = GetOrAdd<TrustStateController>(FindOrCreate("MIN_LOOP_ROOT/FlowManagers/TrustStateController"));
        MinLoopSubtitlePlayer subtitlePlayer = Object.FindObjectOfType<MinLoopSubtitlePlayer>(true);

        HearthTvTerminalController terminal = FindTerminal17F01();
        HearthCompanionHudController hud = Object.FindObjectOfType<HearthCompanionHudController>(true);
        HearthCompanionHudFlowBinder hudFlowBinder = hud != null ? hud.GetComponent<HearthCompanionHudFlowBinder>() : null;
        HearthCompanionHudPreviewInput previewInput = hud != null ? hud.GetComponent<HearthCompanionHudPreviewInput>() : null;
        HearthCompanionHudExclusiveMode exclusiveMode = hud != null ? hud.GetComponent<HearthCompanionHudExclusiveMode>() : null;
        HearthDialogueSequence bedroomPreludeDialogue = Ensure17F01BedroomPreludeDialogue();
        HearthDialogueSequence soothingDialogue = Ensure17F01SoothingDialogue();
        HearthDialogueSequence livingRoomDialogue = Ensure17F01LivingRoomDialogue();

        GameObject replayControllerObject = FindOrCreateChild(replayRoot, "HearthCompanion17F01ReplayController").gameObject;
        HearthCompanion17F01ReplayController replayController = GetOrAdd<HearthCompanion17F01ReplayController>(replayControllerObject);

        GameObject boy = FindActor("Laying_Sleeping", "little_boy_B-Laying_Sleeping", "little_boy_B");
        GameObject boyInteractionProxy = Find("Capsule Mesh (1)");
        Transform interactablesRoot = FindOrCreateChild(replayRoot, "RuntimeInteractables");
        Transform boyInteractionTarget = ConfigureBoyInteractionProxy(boyInteractionProxy, boy, interactablesRoot);
        DisableOldBoyRootIfReplacementExists(boy);
        HearthActorPosePreset boyPose = boy != null ? GetOrAdd<HearthActorPosePreset>(boy) : null;
        ConfigurePosePreset(boyPose, new[] { "Sleep", "Awake", "Comforted" });

        GameObject mother = FindActor("casual_Female_G@Sitting_Idle", "casual_Female_G", "casual_Female_K");
        GameObject father = FindActor("casual_Male_G@Sitting", "casual_Male_G", "casual_Male_K");
        HearthActorPosePreset motherPose = mother != null ? GetOrAdd<HearthActorPosePreset>(mother) : null;
        HearthActorPosePreset fatherPose = father != null ? GetOrAdd<HearthActorPosePreset>(father) : null;
        ConfigurePosePreset(motherPose, new[] { "Sitting" });
        ConfigurePosePreset(fatherPose, new[] { "Sitting" });
        DisableActorAnimators(boy);
        DisableActorAnimators(mother);
        DisableActorAnimators(father);
        DisableActorAnimatorsByNames("little_boy_B", "casual_Male_K", "casual_Female_K", "casual_Male_G", "casual_Female_G");
        HearthActorAnimationPlayer boyAnimation = ConfigureActorAnimation(
            boy,
            new ActorClipBinding("LayingSleeping", "Assets/Laying_Sleeping.fbx", "mixamo.com", true, false, 0.18f));
        HearthActorAnimationPlayer motherAnimation = ConfigureActorAnimation(
            mother,
            new ActorClipBinding("SittingIdle", "Assets/casual_Female_G@Sitting_Idle.fbx", "mixamo.com", true, false, 0.18f));
        HearthActorAnimationPlayer fatherAnimation = ConfigureActorAnimation(
            father,
            new ActorClipBinding("Sitting", "Assets/casual_Male_G@Sitting.fbx", "mixamo.com", true, false, 0.18f));

        if (boyInteractionTarget == null && boy != null)
        {
            boyInteractionTarget = boy.transform;
        }

        RemoveDuplicateBoyInteractable(boy, boyInteractionTarget);
        HearthCompanionReplayInteractable approachInteractable = boyInteractionTarget != null ? GetOrAdd<HearthCompanionReplayInteractable>(boyInteractionTarget.gameObject) : null;
        ConfigureApproachInteractable(
            approachInteractable,
            boyInteractionTarget,
            boy != null ? boy.transform : boyInteractionTarget,
            childStartAnchor,
            replayController);

        ConfigureReplayController(
            replayController,
            flow,
            viewSwitch,
            hud,
            subtitlePlayer,
            robot,
            childStartAnchor,
            bedsideAnchor,
            livingAnchor,
            pathAnchor,
            approachInteractable,
            boyPose,
            motherPose,
            fatherPose,
            boyAnimation,
            motherAnimation,
            fatherAnimation,
            bedroomPreludeDialogue,
            soothingDialogue,
            livingRoomDialogue);
        ConfigureRobotMovement(robot);
        ConfigureRobotMovement(robotLiving);
        ConfigureRobotMovement(robotBedside);

        ConfigureFlow(flow, terminal, viewSwitch, replayController, trust);
        ConfigureTrust(trust);
        ConfigureTerminal(terminal, flow, viewSwitch, person);
        ConfigureHud(hud, hudFlowBinder, previewInput, exclusiveMode, flow, viewSwitch);
        ConfigureSubtitlePlayers();
        DisableLegacyMinLoopOverlayPresenters();

        EditorUtility.SetDirty(minLoopRoot);
        if (terminal != null)
        {
            EditorUtility.SetDirty(terminal);
        }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("[Hearth17F01MinimalLoopBinder] 17F01 minimal loop setup applied.");
    }

    private static void ConfigureViewSwitch(ViewSwitchController viewSwitch, GameObject person, GameObject robot)
    {
        if (viewSwitch == null)
        {
            return;
        }

        SerializedObject so = new SerializedObject(viewSwitch);
        SetObject(so, "human.rootObject", person);
        SetObject(so, "human.viewCamera", person != null ? person.GetComponentInChildren<Camera>(true) : null);
        SetObject(so, "human.movement", person != null ? person.GetComponent<FirstPersonMovement>() : null);
        SetObject(so, "human.look", person != null ? person.GetComponentInChildren<FirstPersonLook>(true) : null);
        SetObject(so, "human.interaction", person != null ? person.GetComponent<PlayerInteraction>() : null);
        SetObject(so, "human.rigidbody", person != null ? person.GetComponent<Rigidbody>() : null);
        SetObject(so, "companion.rootObject", robot);
        SetObject(so, "companion.viewCamera", robot != null ? robot.GetComponentInChildren<Camera>(true) : null);
        SetObject(so, "companion.movement", robot != null ? robot.GetComponent<FirstPersonMovement>() : null);
        SetObject(so, "companion.look", robot != null ? robot.GetComponentInChildren<FirstPersonLook>(true) : null);
        SetObject(so, "companion.interaction", robot != null ? robot.GetComponent<PlayerInteraction>() : null);
        SetObject(so, "companion.rigidbody", robot != null ? robot.GetComponent<Rigidbody>() : null);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(viewSwitch);
    }

    private static void ConfigureFlow(
        MinLoopFlowController flow,
        HearthTvTerminalController terminal,
        ViewSwitchController viewSwitch,
        HearthCompanion17F01ReplayController replayController,
        TrustStateController trust)
    {
        if (flow == null)
        {
            return;
        }

        SerializedObject so = new SerializedObject(flow);
        SetObject(so, "tvTerminalController", terminal);
        SetObject(so, "viewSwitchController", viewSwitch);
        SetObject(so, "companion17F01ReplayController", replayController);
        SetObject(so, "trustStateController", trust);
        SetBool(so, "useCompanion17F01ReplayController", true);
        so.ApplyModifiedPropertiesWithoutUndo();
        HearthDispositionDialogueSetup.ConfigureEarlyHouseholds(flow, Object.FindObjectOfType<MinLoopSubtitlePlayer>(true));
        EditorUtility.SetDirty(flow);
    }

    private static void ConfigureTrust(TrustStateController trust)
    {
        if (trust == null)
        {
            return;
        }

        trust.ConfigureRules(0, -3, 3, 1, -1, true);
        EditorUtility.SetDirty(trust);
    }

    private static void ConfigureTerminal(
        HearthTvTerminalController terminal,
        MinLoopFlowController flow,
        ViewSwitchController viewSwitch,
        GameObject person)
    {
        if (terminal == null)
        {
            return;
        }

        SerializedObject so = new SerializedObject(terminal);
        SetObject(so, "minLoopFlowController", flow);
        SetObject(so, "viewSwitchController", viewSwitch);
        SetObject(so, "playerCamera", person != null ? person.GetComponentInChildren<Camera>(true) : null);
        SetBool(so, "routeChoicesToMinLoop", true);
        SetBool(so, "closeTerminalWhenChoiceSubmitted", false);
        SetBool(so, "preventRepeatedChoiceSubmission", true);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(terminal);
    }

    private static void ConfigureHud(
        HearthCompanionHudController hud,
        HearthCompanionHudFlowBinder flowBinder,
        HearthCompanionHudPreviewInput previewInput,
        HearthCompanionHudExclusiveMode exclusiveMode,
        MinLoopFlowController flow,
        ViewSwitchController viewSwitch)
    {
        if (hud != null)
        {
            SerializedObject so = new SerializedObject(hud);
            SetBool(so, "showStartingSceneOnStart", false);
            SetBool(so, "autoAdvanceOnHoldPrompt", false);
            SetObject(so, "viewSwitchController", viewSwitch);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(hud);
        }

        if (flowBinder != null)
        {
            SerializedObject so = new SerializedObject(flowBinder);
            SetObject(so, "companionHud", hud);
            SetObject(so, "minLoopFlowController", flow);
            SetObject(so, "viewSwitchController", viewSwitch);
            SetString(so, "firstReplaySceneId", "17F01_01");
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(flowBinder);
        }

        if (previewInput != null)
        {
            SerializedObject so = new SerializedObject(previewInput);
            SetBool(so, "previewInputEnabled", false);
            SetObject(so, "controller", hud);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(previewInput);
        }

        if (exclusiveMode != null)
        {
            SerializedObject so = new SerializedObject(exclusiveMode);
            SetObject(so, "viewSwitchController", viewSwitch);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(exclusiveMode);
        }
    }

    private static void ConfigureReplayController(
        HearthCompanion17F01ReplayController replayController,
        MinLoopFlowController flow,
        ViewSwitchController viewSwitch,
        HearthCompanionHudController hud,
        MinLoopSubtitlePlayer subtitlePlayer,
        GameObject robot,
        Transform childStartAnchor,
        Transform bedsideAnchor,
        Transform livingAnchor,
        Transform pathAnchor,
        HearthCompanionReplayInteractable approachInteractable,
        HearthActorPosePreset boyPose,
        HearthActorPosePreset motherPose,
        HearthActorPosePreset fatherPose,
        HearthActorAnimationPlayer boyAnimation,
        HearthActorAnimationPlayer motherAnimation,
        HearthActorAnimationPlayer fatherAnimation,
        HearthDialogueSequence bedroomPreludeDialogue,
        HearthDialogueSequence soothingDialogue,
        HearthDialogueSequence livingRoomDialogue)
    {
        if (replayController == null)
        {
            return;
        }

        SerializedObject so = new SerializedObject(replayController);
        SetObject(so, "flowController", flow);
        SetObject(so, "viewSwitchController", viewSwitch);
        SetObject(so, "companionHud", hud);
        SetObject(so, "subtitlePlayer", subtitlePlayer);
        SetObject(so, "robotRoot", robot != null ? robot.transform : null);
        SetObject(so, "robotCamera", robot != null ? robot.GetComponentInChildren<Camera>(true) : null);
        SetObject(so, "robotMovement", robot != null ? robot.GetComponent<FirstPersonMovement>() : null);
        SetObject(so, "robotLook", robot != null ? robot.GetComponentInChildren<FirstPersonLook>(true) : null);
        SetObject(so, "robotInteraction", robot != null ? robot.GetComponent<PlayerInteraction>() : null);
        SetObject(so, "robotRigidbody", robot != null ? robot.GetComponent<Rigidbody>() : null);
        SetObject(so, "childRoomStartAnchor", childStartAnchor);
        SetObject(so, "bedsideInteractAnchor", bedsideAnchor);
        SetObject(so, "livingRoomStartAnchor", livingAnchor);
        SetArray(so, "bedsidePathPoints", pathAnchor != null ? new Object[] { pathAnchor } : new Object[0]);
        SetObject(so, "approachBoyInteractable", approachInteractable);
        SetFloat(so, "promptDelayAfterBedroomPrelude", 1.5f);
        SetFloat(so, "autoMoveSpeed", RobotAutoMoveSpeed);
        SetObject(so, "boyPosePreset", boyPose);
        SetObject(so, "motherPosePreset", motherPose);
        SetObject(so, "fatherPosePreset", fatherPose);
        SetObject(so, "boyAnimation", boyAnimation);
        SetObject(so, "motherAnimation", motherAnimation);
        SetObject(so, "fatherAnimation", fatherAnimation);
        SetBool(so, "preferDialogueSequenceAssets", true);
        SetObject(so, "bedroomPreludeSequence", bedroomPreludeDialogue);
        SetObject(so, "soothingSequence", soothingDialogue);
        SetObject(so, "livingRoomSequence", livingRoomDialogue);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(replayController);
    }

    private static void ConfigureApproachInteractable(
        HearthCompanionReplayInteractable interactable,
        Transform interactionTarget,
        Transform sideReference,
        Transform childStartAnchor,
        HearthCompanion17F01ReplayController replayController)
    {
        if (interactable == null)
        {
            return;
        }

        SerializedObject so = new SerializedObject(interactable);
        SetObject(so, "focusTarget", interactionTarget);
        SetObject(so, "interactionCollider", interactable.GetComponent<Collider>());
        SetObject(so, "raycastTargetRoot", interactionTarget);
        SetObject(so, "replayController", replayController);
        SetString(so, "interactionLabel", "[ Approach bedside - Guard service subject ]");
        SetBool(so, "availableOnStart", false);
        SetFloat(so, "maxDistance", BoyInteractionDistance);
        SetFloat(so, "maxViewAngle", 8f);
        SetBool(so, "requireLineOfSight", false);
        SetBool(so, "requireCenterRayHit", true);
        SetBool(so, "useAllowedSideGate", childStartAnchor != null && sideReference != null);
        SetObject(so, "allowedSideReference", sideReference);
        if (childStartAnchor != null && sideReference != null)
        {
            Vector3 worldNormal = (childStartAnchor.position - sideReference.position).normalized;
            SetVector3(so, "allowedSideLocalNormal", sideReference.InverseTransformDirection(worldNormal));
            SetFloat(so, "minAllowedSideDot", -0.15f);
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(interactable);
    }

    private static Transform ConfigureBoyInteractionProxy(GameObject proxy, GameObject boy, Transform parent)
    {
        if (proxy == null && boy != null)
        {
            proxy = CreateBoyInteractionProxy(boy, parent);
        }

        if (proxy == null)
        {
            return null;
        }

        if (parent != null && proxy.transform.parent != parent)
        {
            Vector3 worldPosition = proxy.transform.position;
            Quaternion worldRotation = proxy.transform.rotation;
            Undo.SetTransformParent(proxy.transform, parent, "Parent boy interaction proxy");
            proxy.transform.SetPositionAndRotation(worldPosition, worldRotation);
        }

        if (boy != null)
        {
            if (Vector3.Distance(proxy.transform.position, boy.transform.position) > ProxyRepositionDistance)
            {
                PositionProxyOnBoy(proxy, boy);
            }
        }

        foreach (Renderer renderer in proxy.GetComponentsInChildren<Renderer>(true))
        {
            renderer.enabled = false;
            EditorUtility.SetDirty(renderer);
        }

        Collider[] colliders = proxy.GetComponentsInChildren<Collider>(true);
        if (colliders.Length == 0)
        {
            CapsuleCollider capsule = proxy.AddComponent<CapsuleCollider>();
            capsule.isTrigger = true;
            capsule.radius = 0.35f;
            capsule.height = 1.4f;
            capsule.direction = 1;
            EditorUtility.SetDirty(capsule);
        }
        else
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                Collider collider = colliders[i];
                if (collider == null)
                {
                    continue;
                }

                MeshCollider meshCollider = collider as MeshCollider;
                if (meshCollider != null)
                {
                    meshCollider.convex = true;
                }

                collider.enabled = true;
                collider.isTrigger = true;
                EditorUtility.SetDirty(collider);
            }
        }

        EditorUtility.SetDirty(proxy);
        return proxy.transform;
    }

    private static GameObject CreateBoyInteractionProxy(GameObject boy, Transform parent)
    {
        GameObject proxy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        Undo.RegisterCreatedObjectUndo(proxy, "Create boy interaction proxy");
        proxy.name = "Capsule Mesh (1)";
        foreach (Renderer renderer in proxy.GetComponentsInChildren<Renderer>(true))
        {
            renderer.enabled = false;
        }

        if (parent != null)
        {
            proxy.transform.SetParent(parent, false);
        }

        PositionProxyOnBoy(proxy, boy);
        return proxy;
    }

    private static void PositionProxyOnBoy(GameObject proxy, GameObject boy)
    {
        if (proxy == null || boy == null)
        {
            return;
        }

        Bounds bounds;
        bool hasBounds = TryGetRendererBounds(boy, proxy, out bounds);
        proxy.transform.position = hasBounds ? bounds.center : boy.transform.position;
        proxy.transform.rotation = boy.transform.rotation;
        proxy.transform.localScale = Vector3.one;

        CapsuleCollider capsule = proxy.GetComponent<CapsuleCollider>();
        if (capsule == null)
        {
            capsule = proxy.AddComponent<CapsuleCollider>();
        }

        capsule.isTrigger = true;
        capsule.direction = 1;
        capsule.center = Vector3.zero;
        if (hasBounds)
        {
            capsule.height = Mathf.Max(1.2f, bounds.size.y);
            capsule.radius = Mathf.Max(0.35f, Mathf.Min(0.7f, Mathf.Max(bounds.extents.x, bounds.extents.z)));
        }
        else
        {
            capsule.height = 1.4f;
            capsule.radius = 0.35f;
        }

        EditorUtility.SetDirty(proxy.transform);
        EditorUtility.SetDirty(capsule);
    }

    private static bool TryGetRendererBounds(GameObject target, GameObject excludedChild, out Bounds bounds)
    {
        bounds = new Bounds();
        if (target == null)
        {
            return false;
        }

        Renderer[] renderers = target.GetComponentsInChildren<Renderer>(true);
        bool initialized = false;
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null)
            {
                continue;
            }

            if (excludedChild != null && renderer.transform.IsChildOf(excludedChild.transform))
            {
                continue;
            }

            if (!initialized)
            {
                bounds = renderer.bounds;
                initialized = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return initialized;
    }

    private static void ConfigureRobotMovement(GameObject robot)
    {
        if (robot == null)
        {
            return;
        }

        FirstPersonMovement movement = robot.GetComponent<FirstPersonMovement>();
        if (movement == null)
        {
            return;
        }

        SerializedObject so = new SerializedObject(movement);
        SetFloat(so, "speed", RobotWalkSpeed);
        SetFloat(so, "runSpeed", RobotRunSpeed);
        SetBool(so, "canRun", false);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(movement);
    }

    private static void ConfigureSubtitlePlayers()
    {
        foreach (MinLoopSubtitlePlayer subtitlePlayer in Object.FindObjectsOfType<MinLoopSubtitlePlayer>(true))
        {
            SerializedObject so = new SerializedObject(subtitlePlayer);
            SetBool(so, "useCleanCenteredStyle", true);
            SetFloat(so, "subtitleWidthFraction", 0.66f);
            SetFloat(so, "speakerCenterY", SubtitleSpeakerCenterY);
            SetFloat(so, "speakerHeightFraction", 0.06f);
            SetFloat(so, "bodyCenterY", SubtitleBodyCenterY);
            SetFloat(so, "bodyHeightFraction", 0.12f);
            SetFloat(so, "cleanSpeakerFontSize", 22f);
            SetFloat(so, "cleanBodyFontSize", 28f);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(subtitlePlayer);
        }
    }

    private static HearthDialogueSequence Ensure17F01BedroomPreludeDialogue()
    {
        return EnsureDialogueSequence(
            BedroomPreludeDialoguePath,
            "17F01_BedroomPrelude",
            "Plays after the companion replay begins in the boy's room. The E prompt is gated until this sequence finishes, then the replay controller waits its prompt delay.",
            new DefaultSubtitleLine("Son", "... No...", 1.8f, 1.2f),
            new DefaultSubtitleLine("Son", "... Mom...", 2.2f, 1.5f),
            new DefaultSubtitleLine("Synth Voice", "Decision: initiate soothing protocol. Reason: service subject showing signs of nightmare.", 0.4f, 3f));
    }

    private static HearthDialogueSequence Ensure17F01SoothingDialogue()
    {
        return EnsureDialogueSequence(
            SoothingDialoguePath,
            "17F01_BedsideSoothing",
            "Plays after the player confirms the bedside interaction. Replace these lines with the final soothing script and voice clips.",
            new DefaultSubtitleLine("Companion Unit", "Was it a nightmare? Come on, with me, slowly. Deep breath. One, two...", 0f, 3.5f),
            new DefaultSubtitleLine("Companion Unit", "Mom and Dad should be asleep. If you go knock at this hour, she'll be very tired tomorrow.", 0.2f, 4f),
            new DefaultSubtitleLine("Companion Unit", "Let's calm down like this first. When you feel a little better, if you still want to go, you can go then. Okay?", 0.2f, 4.4f),
            new DefaultSubtitleLine("Companion Unit", "Two more deep breaths. That's it. Good. Let's lie back down. I'll stay with you until you fall asleep.", 0.2f, 4.2f),
            new DefaultSubtitleLine("Synth Voice", "Event archived.", 0.6f, 1.8f));
    }

    private static HearthDialogueSequence Ensure17F01LivingRoomDialogue()
    {
        return EnsureDialogueSequence(
            LivingRoomDialoguePath,
            "17F01_LivingRoomObservation",
            "Plays during the living-room observation. The replay returns to the terminal after this sequence finishes.",
            new DefaultSubtitleLine("Father", "He had a nightmare last night?", 0.2f, 2.2f),
            new DefaultSubtitleLine("Mother", "... He didn't come out.", 0.4f, 2.2f),
            new DefaultSubtitleLine("Father", "Mm.", 0.2f, 1.2f),
            new DefaultSubtitleLine("Mother", "Then it handled it well.", 1.2f, 2.2f),
            new DefaultSubtitleLine("Father", "Mm.", 0.2f, 1.2f),
            new DefaultSubtitleLine("Mother", "... But it feels strange.", 0.6f, 2.4f),
            new DefaultSubtitleLine("Mother", "Recently, I haven't heard him knock at all. I'm actually a little unused to it.", 0.2f, 4f),
            new DefaultSubtitleLine("Father", "Isn't that a good thing? He's grown up. And it saves us from getting up in the middle of the night.", 0.4f, 4.2f),
            new DefaultSubtitleLine("Mother", "But the child hasn't told us about his nightmares for a long time.", 0.3f, 3.6f),
            new DefaultSubtitleLine("Father", "At least we don't have to worry about him at night anymore, right?", 0.6f, 3.2f),
            new DefaultSubtitleLine("Mother", "... Mm.", 0.8f, 1.6f));
    }

    private static HearthDialogueSequence EnsureDialogueSequence(string path, string sequenceId, string notes, params DefaultSubtitleLine[] defaults)
    {
        EnsureAssetFolder(DialogueFolder);

        HearthDialogueSequence sequence = AssetDatabase.LoadAssetAtPath<HearthDialogueSequence>(path);
        if (sequence == null)
        {
            sequence = ScriptableObject.CreateInstance<HearthDialogueSequence>();
            AssetDatabase.CreateAsset(sequence, path);
        }

        SerializedObject so = new SerializedObject(sequence);
        SetString(so, "sequenceId", sequenceId);
        SetString(so, "notes", notes);

        SerializedProperty lines = so.FindProperty("lines");
        if (lines != null && lines.arraySize == 0 && defaults != null)
        {
            lines.arraySize = defaults.Length;
            for (int i = 0; i < defaults.Length; i++)
            {
                SerializedProperty line = lines.GetArrayElementAtIndex(i);
                line.FindPropertyRelative("speaker").stringValue = defaults[i].speaker;
                line.FindPropertyRelative("text").stringValue = defaults[i].text;
                line.FindPropertyRelative("startDelay").floatValue = defaults[i].startDelay;
                line.FindPropertyRelative("holdSeconds").floatValue = defaults[i].holdSeconds;
                line.FindPropertyRelative("voiceClip").objectReferenceValue = null;
            }
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(sequence);
        return sequence;
    }

    private static void EnsureAssetFolder(string folder)
    {
        if (AssetDatabase.IsValidFolder(folder))
        {
            return;
        }

        string parent = System.IO.Path.GetDirectoryName(folder).Replace("\\", "/");
        if (!AssetDatabase.IsValidFolder(parent))
        {
            EnsureAssetFolder(parent);
        }

        AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(folder));
    }

    private readonly struct DefaultSubtitleLine
    {
        public readonly string speaker;
        public readonly string text;
        public readonly float startDelay;
        public readonly float holdSeconds;

        public DefaultSubtitleLine(string newSpeaker, string newText, float newStartDelay, float newHoldSeconds)
        {
            speaker = newSpeaker;
            text = newText;
            startDelay = newStartDelay;
            holdSeconds = newHoldSeconds;
        }
    }

    private static void DisableLegacyMinLoopOverlayPresenters()
    {
        GameObject minLoopRoot = Find("MIN_LOOP_ROOT");
        if (minLoopRoot != null)
        {
            HideGeneratedChildren(minLoopRoot.transform);
        }

        DestroySceneObject("MinLoopObjectivePresenter");
        DestroySceneObject("MinLoopTrustPresenter");
        DestroySceneObject("MinLoopRobotHudPresenter");
    }

    private static void HideGeneratedChildren(Transform root)
    {
        if (root == null)
        {
            return;
        }

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child == root)
            {
                continue;
            }

            if (child.name == "Min Loop Objective Canvas" ||
                child.name == "Min Loop Trust Canvas" ||
                child.name == "Min Loop Robot HUD Canvas" ||
                child.name == "Min Loop Objective Panel" ||
                child.name == "Min Loop Trust Panel" ||
                child.name == "Min Loop Robot HUD Panel")
            {
                child.gameObject.SetActive(false);
                EditorUtility.SetDirty(child.gameObject);
            }
        }
    }

    private static void RemoveDuplicateBoyInteractable(GameObject boy, Transform interactionTarget)
    {
        if (boy == null || interactionTarget == null || boy.transform == interactionTarget)
        {
            return;
        }

        HearthCompanionReplayInteractable duplicate = boy.GetComponent<HearthCompanionReplayInteractable>();
        if (duplicate != null)
        {
            Undo.DestroyObjectImmediate(duplicate);
            EditorUtility.SetDirty(boy);
        }
    }

    private static void DisableOldBoyRootIfReplacementExists(GameObject selectedBoy)
    {
        if (selectedBoy == null || selectedBoy.name == "little_boy_B")
        {
            return;
        }

        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < allObjects.Length; i++)
        {
            GameObject candidate = allObjects[i];
            if (candidate == null || !candidate.scene.IsValid() || candidate.name != "little_boy_B")
            {
                continue;
            }

            if (candidate == selectedBoy ||
                candidate.transform.IsChildOf(selectedBoy.transform) ||
                selectedBoy.transform.IsChildOf(candidate.transform))
            {
                continue;
            }

            candidate.SetActive(false);
            foreach (Renderer renderer in candidate.GetComponentsInChildren<Renderer>(true))
            {
                renderer.enabled = false;
                EditorUtility.SetDirty(renderer);
            }

            foreach (Collider collider in candidate.GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = false;
                EditorUtility.SetDirty(collider);
            }

            foreach (Animator animator in candidate.GetComponentsInChildren<Animator>(true))
            {
                animator.enabled = false;
                EditorUtility.SetDirty(animator);
            }

            EditorUtility.SetDirty(candidate);
        }
    }

    private static void DestroySceneObject(string objectName)
    {
        GameObject target = Find(objectName);
        if (target == null)
        {
            return;
        }

        Undo.DestroyObjectImmediate(target);
    }

    private static void ConfigurePosePreset(HearthActorPosePreset preset, string[] ids)
    {
        if (preset == null || ids == null)
        {
            return;
        }

        SerializedObject so = new SerializedObject(preset);
        SetObject(so, "defaultPoseRoot", preset.transform);
        SetBool(so, "pauseAnimatorsOnApply", true);
        Animator[] animators = preset.GetComponentsInChildren<Animator>(true);
        SetArray(so, "animatorsToPause", animators);
        SerializedProperty poses = so.FindProperty("poses");
        if (poses != null)
        {
            poses.arraySize = ids.Length;
            for (int i = 0; i < ids.Length; i++)
            {
                SerializedProperty pose = poses.GetArrayElementAtIndex(i);
                pose.FindPropertyRelative("id").stringValue = ids[i];
                pose.FindPropertyRelative("root").objectReferenceValue = preset.transform;
                pose.FindPropertyRelative("localPosition").vector3Value = preset.transform.localPosition;
                pose.FindPropertyRelative("localEulerAngles").vector3Value = preset.transform.localEulerAngles;
                pose.FindPropertyRelative("localScale").vector3Value = preset.transform.localScale;
            }
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(preset);
    }

    private static void DisableActorAnimators(GameObject actor)
    {
        if (actor == null)
        {
            return;
        }

        foreach (Animator animator in actor.GetComponentsInChildren<Animator>(true))
        {
            animator.enabled = false;
            EditorUtility.SetDirty(animator);
        }
    }

    private static void DisableActorAnimatorsByNames(params string[] actorNames)
    {
        if (actorNames == null)
        {
            return;
        }

        Animator[] animators = Object.FindObjectsOfType<Animator>(true);
        for (int i = 0; i < animators.Length; i++)
        {
            Animator animator = animators[i];
            if (animator == null)
            {
                continue;
            }

            for (int j = 0; j < actorNames.Length; j++)
            {
                if (string.Equals(animator.gameObject.name, actorNames[j], System.StringComparison.OrdinalIgnoreCase))
                {
                    animator.enabled = false;
                    EditorUtility.SetDirty(animator);
                    break;
                }
            }
        }
    }

    private static Transform CreateAnchor(Transform parent, string name, Transform source)
    {
        Transform anchor = FindOrCreateChild(parent, name);
        if (source != null)
        {
            anchor.SetPositionAndRotation(source.position, source.rotation);
        }

        EditorUtility.SetDirty(anchor.gameObject);
        return anchor;
    }

    private static Transform CreatePathAnchor(Transform parent, Transform start, Transform end)
    {
        Transform anchor = FindOrCreateChild(parent, "Anchor_Robot_17F01_BedsidePath_01");
        if (start != null && end != null)
        {
            anchor.position = Vector3.Lerp(start.position, end.position, 0.5f);
            anchor.rotation = Quaternion.Slerp(start.rotation, end.rotation, 0.5f);
        }

        EditorUtility.SetDirty(anchor.gameObject);
        return anchor;
    }

    private static void DisableReferenceController(GameObject controller)
    {
        if (controller == null)
        {
            return;
        }

        foreach (Camera camera in controller.GetComponentsInChildren<Camera>(true))
        {
            camera.enabled = false;
            EditorUtility.SetDirty(camera);
        }

        foreach (AudioListener listener in controller.GetComponentsInChildren<AudioListener>(true))
        {
            listener.enabled = false;
            EditorUtility.SetDirty(listener);
        }

        foreach (Behaviour behaviour in controller.GetComponentsInChildren<Behaviour>(true))
        {
            if (behaviour is FirstPersonMovement ||
                behaviour is FirstPersonLook ||
                behaviour is PlayerInteraction ||
                behaviour.GetType().Name == "Jump" ||
                behaviour.GetType().Name == "Crouch" ||
                behaviour.GetType().Name == "Zoom" ||
                behaviour.GetType().Name == "FirstPersonAudio")
            {
                behaviour.enabled = false;
                EditorUtility.SetDirty(behaviour);
            }
        }

        foreach (Collider collider in controller.GetComponentsInChildren<Collider>(true))
        {
            collider.enabled = false;
            EditorUtility.SetDirty(collider);
        }

        HearthEditorOnlyReferenceModel marker = GetOrAdd<HearthEditorOnlyReferenceModel>(controller);
        marker.ApplyReferenceState();
        EditorUtility.SetDirty(marker);
    }

    private static HearthTvTerminalController FindTerminal17F01()
    {
        GameObject terminalObject = Find("17F/ROOM1/TV (3)/MonitorCanvas/Terminal_17F01", "Terminal_17F01");
        if (terminalObject == null)
        {
            return Object.FindObjectOfType<HearthTvTerminalController>(true);
        }

        return terminalObject.GetComponent<HearthTvTerminalController>();
    }

    private static GameObject FindOrCreate(string path)
    {
        GameObject found = GameObject.Find(path);
        if (found != null)
        {
            return found;
        }

        string[] parts = path.Split('/');
        Transform parent = null;
        GameObject current = null;
        for (int i = 0; i < parts.Length; i++)
        {
            string partial = parts[i];
            Transform child = parent != null ? parent.Find(partial) : null;
            if (child != null)
            {
                current = child.gameObject;
            }
            else
            {
                current = new GameObject(partial);
                if (parent != null)
                {
                    current.transform.SetParent(parent, false);
                }
            }

            parent = current.transform;
        }

        return current;
    }

    private static Transform FindOrCreateChild(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
        {
            return child;
        }

        GameObject childObject = new GameObject(name);
        childObject.transform.SetParent(parent, false);
        return childObject.transform;
    }

    private readonly struct ActorClipBinding
    {
        public readonly string clipId;
        public readonly string assetPath;
        public readonly string preferredClipName;
        public readonly bool loop;
        public readonly bool applyRootMotion;
        public readonly float fadeSeconds;

        public ActorClipBinding(string newClipId, string newAssetPath, string newPreferredClipName, bool newLoop, bool newApplyRootMotion, float newFadeSeconds)
        {
            clipId = newClipId;
            assetPath = newAssetPath;
            preferredClipName = newPreferredClipName;
            loop = newLoop;
            applyRootMotion = newApplyRootMotion;
            fadeSeconds = newFadeSeconds;
        }
    }

    private static HearthActorAnimationPlayer ConfigureActorAnimation(GameObject actor, params ActorClipBinding[] bindings)
    {
        if (actor == null)
        {
            return null;
        }

        HearthActorAnimationPlayer player = GetOrAdd<HearthActorAnimationPlayer>(actor);
        SerializedObject so = new SerializedObject(player);
        Animator animator = actor.GetComponentInChildren<Animator>(true);
        if (animator == null)
        {
            animator = actor.GetComponent<Animator>();
        }

        if (animator == null)
        {
            animator = actor.AddComponent<Animator>();
            EditorUtility.SetDirty(animator);
        }

        SetObject(so, "animator", animator);

        SerializedProperty clips = so.FindProperty("clips");
        if (clips != null && clips.isArray)
        {
            clips.arraySize = bindings != null ? bindings.Length : 0;
            for (int i = 0; i < clips.arraySize; i++)
            {
                SerializedProperty slot = clips.GetArrayElementAtIndex(i);
                AnimationClip clip = FindAnimationClip(bindings[i].assetPath, bindings[i].preferredClipName);
                slot.FindPropertyRelative("clipId").stringValue = bindings[i].clipId;
                slot.FindPropertyRelative("clip").objectReferenceValue = clip;
                slot.FindPropertyRelative("loop").boolValue = bindings[i].loop;
                slot.FindPropertyRelative("applyRootMotion").boolValue = bindings[i].applyRootMotion;
                slot.FindPropertyRelative("applyFootIk").boolValue = true;
                slot.FindPropertyRelative("fadeSeconds").floatValue = bindings[i].fadeSeconds;
                slot.FindPropertyRelative("playbackSpeed").floatValue = 1f;
            }
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(player);
        return player;
    }

    private static AnimationClip FindAnimationClip(string assetPath, string preferredClipName)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
        AnimationClip fallback = null;
        for (int i = 0; i < assets.Length; i++)
        {
            AnimationClip clip = assets[i] as AnimationClip;
            if (clip == null)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(preferredClipName) && clip.name == preferredClipName)
            {
                return clip;
            }

            if (fallback == null && !clip.name.StartsWith("__preview__") && clip.length > 0.05f)
            {
                fallback = clip;
            }
        }

        return fallback;
    }

    private static GameObject Find(params string[] namesOrPaths)
    {
        for (int i = 0; i < namesOrPaths.Length; i++)
        {
            GameObject found = GameObject.Find(namesOrPaths[i]);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private static GameObject FindActor(params string[] namesOrPaths)
    {
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        for (int i = 0; i < namesOrPaths.Length; i++)
        {
            string wanted = namesOrPaths[i];
            if (string.IsNullOrEmpty(wanted))
            {
                continue;
            }

            GameObject best = null;
            for (int j = 0; j < allObjects.Length; j++)
            {
                GameObject candidate = allObjects[j];
                if (candidate == null || !candidate.scene.IsValid() || candidate.name != wanted)
                {
                    continue;
                }

                if (candidate.GetComponent<Animator>() != null)
                {
                    return candidate;
                }

                if (candidate.GetComponentInChildren<Animator>(true) != null)
                {
                    best = candidate;
                    continue;
                }

                if (best == null)
                {
                    best = candidate;
                }
            }

            if (best != null)
            {
                return best;
            }
        }

        return Find(namesOrPaths);
    }

    private static T GetOrAdd<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        if (component == null)
        {
            component = target.AddComponent<T>();
        }

        return component;
    }

    private static void SetObject(SerializedObject so, string path, Object value)
    {
        SerializedProperty property = so.FindProperty(path);
        if (property != null)
        {
            property.objectReferenceValue = value;
        }
    }

    private static void SetBool(SerializedObject so, string path, bool value)
    {
        SerializedProperty property = so.FindProperty(path);
        if (property != null)
        {
            property.boolValue = value;
        }
    }

    private static void SetFloat(SerializedObject so, string path, float value)
    {
        SerializedProperty property = so.FindProperty(path);
        if (property != null)
        {
            property.floatValue = value;
        }
    }

    private static void SetString(SerializedObject so, string path, string value)
    {
        SerializedProperty property = so.FindProperty(path);
        if (property != null)
        {
            property.stringValue = value;
        }
    }

    private static void SetVector3(SerializedObject so, string path, Vector3 value)
    {
        SerializedProperty property = so.FindProperty(path);
        if (property != null)
        {
            property.vector3Value = value;
        }
    }

    private static void SetArray(SerializedObject so, string path, Object[] values)
    {
        SerializedProperty property = so.FindProperty(path);
        if (property == null || !property.isArray)
        {
            return;
        }

        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }
}
