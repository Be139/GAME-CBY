using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Hearth17F01MinimalLoopBinder
{
    private const string MenuPath = "Tools/Hearth/Replay/Apply 17F01 Minimal Loop Setup";
    private const float BoyInteractionDistance = 1.42f;
    private const float RobotAutoMoveSpeed = 0.69f;

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
        Transform bedsideAnchor = CreateAnchor(anchorsRoot, "Anchor_Robot_17F01_BedsideInteract", robotBedside != null ? robotBedside.transform : null);
        Transform livingAnchor = CreateAnchor(anchorsRoot, "Anchor_Robot_17F01_LivingRoomStart", robotLiving != null ? robotLiving.transform : null);
        Transform pathAnchor = CreatePathAnchor(anchorsRoot, childStartAnchor, bedsideAnchor);

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

        GameObject replayControllerObject = FindOrCreateChild(replayRoot, "HearthCompanion17F01ReplayController").gameObject;
        HearthCompanion17F01ReplayController replayController = GetOrAdd<HearthCompanion17F01ReplayController>(replayControllerObject);

        GameObject boy = Find("little_boy_B");
        GameObject boyInteractionProxy = Find("Capsule Mesh (1)");
        Transform boyInteractionTarget = ConfigureBoyInteractionProxy(boyInteractionProxy);
        HearthActorPosePreset boyPose = boy != null ? GetOrAdd<HearthActorPosePreset>(boy) : null;
        ConfigurePosePreset(boyPose, new[] { "Sleep", "Awake", "Comforted" });

        GameObject mother = Find("casual_Female_K", "casual_Female_G");
        GameObject father = Find("casual_Male_K", "casual_Male_G");
        HearthActorPosePreset motherPose = mother != null ? GetOrAdd<HearthActorPosePreset>(mother) : null;
        HearthActorPosePreset fatherPose = father != null ? GetOrAdd<HearthActorPosePreset>(father) : null;
        ConfigurePosePreset(motherPose, new[] { "Sitting" });
        ConfigurePosePreset(fatherPose, new[] { "Sitting" });
        DisableActorAnimators(boy);
        DisableActorAnimators(mother);
        DisableActorAnimators(father);
        DisableActorAnimatorsByNames("little_boy_B", "casual_Male_K", "casual_Female_K", "casual_Male_G", "casual_Female_G");

        if (boyInteractionTarget == null && boy != null)
        {
            boyInteractionTarget = boy.transform;
        }

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
            fatherPose);

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
        HearthActorPosePreset fatherPose)
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
        SetFloat(so, "autoMoveSpeed", RobotAutoMoveSpeed);
        SetObject(so, "boyPosePreset", boyPose);
        SetObject(so, "motherPosePreset", motherPose);
        SetObject(so, "fatherPosePreset", fatherPose);
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

    private static Transform ConfigureBoyInteractionProxy(GameObject proxy)
    {
        if (proxy == null)
        {
            return null;
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

    private static void ConfigureSubtitlePlayers()
    {
        foreach (MinLoopSubtitlePlayer subtitlePlayer in Object.FindObjectsOfType<MinLoopSubtitlePlayer>(true))
        {
            SerializedObject so = new SerializedObject(subtitlePlayer);
            SetBool(so, "useCleanCenteredStyle", true);
            SetFloat(so, "subtitleWidthFraction", 0.66f);
            SetFloat(so, "subtitleCenterY", 0.46f);
            SetFloat(so, "subtitleHeightFraction", 0.18f);
            SetFloat(so, "cleanSpeakerFontSize", 22f);
            SetFloat(so, "cleanBodyFontSize", 28f);
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(subtitlePlayer);
        }
    }

    private static void DisableLegacyMinLoopOverlayPresenters()
    {
        DisableLegacyPresenter<MinLoopObjectivePresenter>();
        DisableLegacyPresenter<MinLoopTrustPresenter>();
        DisableLegacyPresenter<MinLoopRobotHudPresenter>();
    }

    private static void DisableLegacyPresenter<T>() where T : Behaviour
    {
        foreach (T presenter in Object.FindObjectsOfType<T>(true))
        {
            presenter.enabled = false;
            HideGeneratedChildren(presenter.transform);
            EditorUtility.SetDirty(presenter);
        }
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
