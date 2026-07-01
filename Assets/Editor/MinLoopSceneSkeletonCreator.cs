using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MinLoopSceneSkeletonCreator
{
    private const string RootName = "MIN_LOOP_ROOT";

    [MenuItem("Tools/Min Loop/Create 17F-01 Scene Skeleton")]
    public static void CreateSceneSkeleton()
    {
        GameObject root = FindOrCreateRoot(RootName);
        GameObject flowRoot = FindOrCreateChild(root.transform, "FlowManagers");
        GameObject uiRoot = FindOrCreateChild(root.transform, "UI");
        GameObject replayRoot = FindOrCreateChild(root.transform, "ReplayRoom_17F01");
        GameObject interactionRoot = FindOrCreateChild(root.transform, "Interactions");
        GameObject guideRoot = FindOrCreateChild(root.transform, "Guides");
        GameObject lightingRoot = FindOrCreateChild(root.transform, "Lighting");
        GameObject audioRoot = FindOrCreateChild(root.transform, "Audio");
        GameObject anchorRoot = FindOrCreateChild(root.transform, "Anchors");

        GameObject flowObject = FindOrCreateChild(flowRoot.transform, "MinLoopFlowController");
        MinLoopFlowController flowController = AddOrGetComponent<MinLoopFlowController>(flowObject);

        GameObject trustObject = FindOrCreateChild(flowRoot.transform, "TrustStateController");
        TrustStateController trustStateController = AddOrGetComponent<TrustStateController>(trustObject);

        GameObject viewSwitchObject = FindOrCreateChild(flowRoot.transform, "ViewSwitchController");
        ViewSwitchController viewSwitchController = AddOrGetComponent<ViewSwitchController>(viewSwitchObject);

        GameObject stageActivatorObject = FindOrCreateChild(flowRoot.transform, "MinLoopStageObjectActivator");
        MinLoopStageObjectActivator stageObjectActivator = AddOrGetComponent<MinLoopStageObjectActivator>(stageActivatorObject);

        GameObject stageAnchorObject = FindOrCreateChild(flowRoot.transform, "MinLoopStageAnchorController");
        MinLoopStageAnchorController stageAnchorController = AddOrGetComponent<MinLoopStageAnchorController>(stageAnchorObject);

        GameObject stageCueObject = FindOrCreateChild(flowRoot.transform, "MinLoopStageCueController");
        MinLoopStageCueController stageCueController = AddOrGetComponent<MinLoopStageCueController>(stageCueObject);

        GameObject debugHotkeysObject = FindOrCreateChild(flowRoot.transform, "MinLoopDebugHotkeys");
        MinLoopDebugHotkeys debugHotkeys = AddOrGetComponent<MinLoopDebugHotkeys>(debugHotkeysObject);

        GameObject lightingStateObject = FindOrCreateChild(flowRoot.transform, "MinLoopLightingStateController");
        MinLoopLightingStateController lightingStateController = AddOrGetComponent<MinLoopLightingStateController>(lightingStateObject);

        GameObject audioStateObject = FindOrCreateChild(flowRoot.transform, "MinLoopAudioStateController");
        MinLoopAudioStateController audioStateController = AddOrGetComponent<MinLoopAudioStateController>(audioStateObject);

        GameObject terminalPresenterObject = FindOrCreateChild(uiRoot.transform, "MinLoopTerminalPresenter");
        MinLoopTerminalPresenter terminalPresenter = AddOrGetComponent<MinLoopTerminalPresenter>(terminalPresenterObject);

        GameObject subtitleObject = FindOrCreateChild(uiRoot.transform, "MinLoopSubtitlePlayer");
        MinLoopSubtitlePlayer subtitlePlayer = AddOrGetComponent<MinLoopSubtitlePlayer>(subtitleObject);

        GameObject replayObject = FindOrCreateChild(replayRoot.transform, "ReplaySequenceController");
        ReplaySequenceController replaySequenceController = AddOrGetComponent<ReplaySequenceController>(replayObject);

        GameObject comfortObject = FindOrCreateChild(replayRoot.transform, "ComfortAction_Bedside");
        ComfortActionInteractable comfortAction = AddOrGetComponent<ComfortActionInteractable>(comfortObject);
        BoxCollider comfortCollider = AddOrGetComponent<BoxCollider>(comfortObject);
        comfortCollider.size = new Vector3(1.2f, 1.2f, 1.2f);

        GameObject terminalObject = FindOrCreateChild(interactionRoot.transform, "Terminal_17F01_Interactable");
        ResidentTerminalFlow terminalFlow = AddOrGetComponent<ResidentTerminalFlow>(terminalObject);
        BoxCollider terminalCollider = AddOrGetComponent<BoxCollider>(terminalObject);
        terminalCollider.size = new Vector3(1.2f, 1.6f, 0.25f);

        GameObject terminalGuideObject = FindOrCreateChild(guideRoot.transform, "Guide_Terminal_17F01");
        GameObject comfortGuideObject = FindOrCreateChild(guideRoot.transform, "Guide_ComfortAction");
        GameObject nextResidentGuideObject = FindOrCreateChild(guideRoot.transform, "Guide_NextResident_17F02");
        MinLoopWorldGuideMarker terminalGuideMarker = CreateGuideMarker(terminalGuideObject, "前往 17F-01 终端");
        MinLoopWorldGuideMarker comfortGuideMarker = CreateGuideMarker(comfortGuideObject, "床边安抚操作");
        MinLoopWorldGuideMarker nextResidentGuideMarker = CreateGuideMarker(nextResidentGuideObject, "下一户 17F-02");

        Light corridorLight = CreateDefaultLight(lightingRoot.transform, "Light_Corridor_Warm", new Vector3(0f, 2.6f, 0f), new Color(1f, 0.74f, 0.46f, 1f), 1.35f, 8f);
        Light replayNightLight = CreateDefaultLight(lightingRoot.transform, "Light_Replay_Night", new Vector3(0f, 2.2f, -1.2f), new Color(0.32f, 0.54f, 1f, 1f), 0.9f, 7.5f);
        Light morningLight = CreateDefaultLight(lightingRoot.transform, "Light_Morning_Soft", new Vector3(0f, 2.5f, 1.2f), new Color(1f, 0.9f, 0.7f, 1f), 1.15f, 9f);

        AudioSource corridorAmbience = CreateDefaultAudioSource(audioRoot.transform, "Audio_Corridor_Ambience", 0.18f);
        AudioSource replayNightAmbience = CreateDefaultAudioSource(audioRoot.transform, "Audio_Replay_Night_Ambience", 0.16f);
        AudioSource morningAmbience = CreateDefaultAudioSource(audioRoot.transform, "Audio_Morning_Ambience", 0.12f);

        Transform miaCorridorAnchor = CreateDefaultAnchor(anchorRoot.transform, "Anchor_Mia_CorridorStart", new Vector3(-2f, 0f, 0f), 90f);
        Transform companionReplayAnchor = CreateDefaultAnchor(anchorRoot.transform, "Anchor_Companion_ReplayStart", new Vector3(0f, 0f, 0f), 0f);
        Transform miaTerminalReturnAnchor = CreateDefaultAnchor(anchorRoot.transform, "Anchor_Mia_TerminalReturn", new Vector3(-1.2f, 0f, -1.6f), 0f);
        Transform nextResidentAnchor = CreateDefaultAnchor(anchorRoot.transform, "Anchor_NextResident_17F02", new Vector3(2f, 0f, 0f), 90f);

        InteractionFeedbackController terminalOpenFeedback = CreateFeedback(interactionRoot.transform, "Feedback_TerminalOpen", "E 打开终端");
        InteractionFeedbackController accessCardFeedback = CreateFeedback(interactionRoot.transform, "Feedback_AccessCard", "E 刷工牌");
        InteractionFeedbackController replayRequestFeedback = CreateFeedback(interactionRoot.transform, "Feedback_ReplayRequest", "E 调出昨夜事件");
        InteractionFeedbackController dispositionSubmitFeedback = CreateFeedback(interactionRoot.transform, "Feedback_DispositionSubmit", "E 提交处置");
        InteractionFeedbackController comfortReadyFeedback = CreateFeedback(interactionRoot.transform, "Feedback_ComfortReady", "安抚点就绪");
        InteractionFeedbackController morningReviewFeedback = CreateFeedback(interactionRoot.transform, "Feedback_MorningReview", "早晨回顾");
        InteractionFeedbackController nextResidentGuideFeedback = CreateFeedback(interactionRoot.transform, "Feedback_NextResidentGuide", "下一户指引");

        GameObject validatorObject = FindOrCreateChild(root.transform, "MinLoopSceneValidator");
        MinLoopSceneValidator validator = AddOrGetComponent<MinLoopSceneValidator>(validatorObject);

        AssignObject(flowController, "terminalPresenter", terminalPresenter);
        AssignObject(flowController, "viewSwitchController", viewSwitchController);
        AssignObject(flowController, "replaySequenceController", replaySequenceController);
        AssignObject(flowController, "trustStateController", trustStateController);
        AssignObject(flowController, "terminalOpenFeedback", terminalOpenFeedback);
        AssignObject(flowController, "accessCardFeedback", accessCardFeedback);
        AssignObject(flowController, "replayRequestFeedback", replayRequestFeedback);
        AssignObject(flowController, "dispositionSubmitFeedback", dispositionSubmitFeedback);

        AssignObject(stageObjectActivator, "flowController", flowController);
        AssignStageRules(stageObjectActivator, new StageRuleTemplate[]
        {
            new StageRuleTemplate("终端入口指引", terminalGuideObject, new MinLoopStage[] { MinLoopStage.Corridor }),
            new StageRuleTemplate("安抚操作指引", comfortGuideObject, new MinLoopStage[] { MinLoopStage.WaitingForComfort }),
            new StageRuleTemplate("下一户指引", nextResidentGuideObject, new MinLoopStage[] { MinLoopStage.Complete })
        });

        AssignObject(stageAnchorController, "flowController", flowController);
        AssignAnchorRules(stageAnchorController, new AnchorRuleTemplate[]
        {
            new AnchorRuleTemplate("Mia 初始走廊站位", null, miaCorridorAnchor, new MinLoopStage[] { MinLoopStage.Corridor }),
            new AnchorRuleTemplate("陪伴单元复盘起点", null, companionReplayAnchor, new MinLoopStage[] { MinLoopStage.SwitchingToCompanion }),
            new AnchorRuleTemplate("Mia 回到终端站位", null, miaTerminalReturnAnchor, new MinLoopStage[] { MinLoopStage.ReturningToTerminal })
        });

        AssignObject(stageCueController, "flowController", flowController);
        AssignCueRules(stageCueController, new CueRuleTemplate[]
        {
            new CueRuleTemplate("安抚点就绪反馈", comfortReadyFeedback, new MinLoopStage[] { MinLoopStage.WaitingForComfort }),
            new CueRuleTemplate("早晨回顾反馈", morningReviewFeedback, new MinLoopStage[] { MinLoopStage.MorningReview }),
            new CueRuleTemplate("下一户指引反馈", nextResidentGuideFeedback, new MinLoopStage[] { MinLoopStage.Complete })
        });

        AssignObject(debugHotkeys, "flowController", flowController);
        AssignObject(debugHotkeys, "replaySequenceController", replaySequenceController);

        AssignObject(lightingStateController, "flowController", flowController);
        AssignLightingRules(lightingStateController, new LightingRuleTemplate[]
        {
            new LightingRuleTemplate(
                "走廊/终端暖光",
                corridorLight,
                new MinLoopStage[]
                {
                    MinLoopStage.Corridor,
                    MinLoopStage.AccessCard,
                    MinLoopStage.ResidentSummary,
                    MinLoopStage.ReturningToTerminal,
                    MinLoopStage.DispositionChoice,
                    MinLoopStage.Complete
                },
                new Color(1f, 0.74f, 0.46f, 1f),
                1.35f,
                8f,
                new Color(0.18f, 0.14f, 0.1f, 1f),
                false,
                Color.black,
                0f),
            new LightingRuleTemplate(
                "昨夜复盘冷光",
                replayNightLight,
                new MinLoopStage[]
                {
                    MinLoopStage.SwitchingToCompanion,
                    MinLoopStage.CompanionReplay,
                    MinLoopStage.WaitingForComfort,
                    MinLoopStage.Comforting
                },
                new Color(0.32f, 0.54f, 1f, 1f),
                0.9f,
                7.5f,
                new Color(0.04f, 0.06f, 0.1f, 1f),
                true,
                new Color(0.02f, 0.03f, 0.06f, 1f),
                0.012f),
            new LightingRuleTemplate(
                "早晨回顾柔光",
                morningLight,
                new MinLoopStage[] { MinLoopStage.MorningReview },
                new Color(1f, 0.9f, 0.7f, 1f),
                1.15f,
                9f,
                new Color(0.24f, 0.22f, 0.18f, 1f),
                false,
                Color.black,
                0f)
        });

        AssignObject(audioStateController, "flowController", flowController);
        AssignAudioRules(audioStateController, new AudioRuleTemplate[]
        {
            new AudioRuleTemplate(
                "走廊/终端环境声",
                corridorAmbience,
                new MinLoopStage[]
                {
                    MinLoopStage.Corridor,
                    MinLoopStage.AccessCard,
                    MinLoopStage.ResidentSummary,
                    MinLoopStage.ReturningToTerminal,
                    MinLoopStage.DispositionChoice,
                    MinLoopStage.Complete
                },
                0.18f,
                0.55f,
                0.35f),
            new AudioRuleTemplate(
                "昨夜复盘环境声",
                replayNightAmbience,
                new MinLoopStage[]
                {
                    MinLoopStage.SwitchingToCompanion,
                    MinLoopStage.CompanionReplay,
                    MinLoopStage.WaitingForComfort,
                    MinLoopStage.Comforting
                },
                0.16f,
                0.6f,
                0.45f),
            new AudioRuleTemplate(
                "早晨回顾环境声",
                morningAmbience,
                new MinLoopStage[] { MinLoopStage.MorningReview },
                0.12f,
                0.5f,
                0.4f)
        });

        AssignObject(terminalFlow, "flowController", flowController);

        AssignObject(replaySequenceController, "flowController", flowController);
        AssignObject(replaySequenceController, "subtitlePlayer", subtitlePlayer);
        AssignObject(replaySequenceController, "comfortAction", comfortAction);

        AssignObject(comfortAction, "sequenceController", replaySequenceController);

        AssignObject(validator, "flowController", flowController);
        AssignObject(validator, "terminalFlow", terminalFlow);
        AssignObject(validator, "terminalPresenter", terminalPresenter);
        AssignObject(validator, "viewSwitchController", viewSwitchController);
        AssignObjectArray(validator, "stageObjectActivators", new Object[] { stageObjectActivator });
        AssignObject(validator, "stageAnchorController", stageAnchorController);
        AssignObject(validator, "stageCueController", stageCueController);
        AssignObject(validator, "replaySequenceController", replaySequenceController);
        AssignObject(validator, "comfortAction", comfortAction);
        AssignObject(validator, "subtitlePlayer", subtitlePlayer);
        AssignObject(validator, "trustStateController", trustStateController);
        AssignObject(validator, "debugHotkeys", debugHotkeys);
        AssignObject(validator, "lightingStateController", lightingStateController);
        AssignObject(validator, "audioStateController", audioStateController);
        AssignObjectArray(validator, "guideMarkers", new Object[]
        {
            terminalGuideMarker,
            comfortGuideMarker,
            nextResidentGuideMarker
        });
        AssignObjectArray(validator, "feedbackObjects", new Object[]
        {
            terminalOpenFeedback,
            accessCardFeedback,
            replayRequestFeedback,
            dispositionSubmitFeedback,
            comfortReadyFeedback,
            morningReviewFeedback,
            nextResidentGuideFeedback
        });

        Selection.activeGameObject = root;
        EditorSceneManager.MarkSceneDirty(root.scene);
        Debug.Log("[MinLoopSceneSkeletonCreator] 已创建/更新 17F-01 最小循环场景骨架。下一步：摆放 Mia、Companion、17F-01 终端、儿童房资源，再运行 MinLoopSceneValidator。", root);
    }

    private static InteractionFeedbackController CreateFeedback(Transform parent, string objectName, string description)
    {
        GameObject feedbackObject = FindOrCreateChild(parent, objectName);
        InteractionFeedbackController feedback = AddOrGetComponent<InteractionFeedbackController>(feedbackObject);
        AudioSource audioSource = AddOrGetComponent<AudioSource>(feedbackObject);
        audioSource.playOnAwake = false;
        AssignString(feedback, "interactionDescription", description);
        AssignBool(feedback, "playFeedbackOnInteract", false);
        return feedback;
    }

    private static Light CreateDefaultLight(Transform parent, string objectName, Vector3 localPosition, Color color, float intensity, float range)
    {
        GameObject lightObject = FindOrCreateChild(parent, objectName);
        lightObject.transform.localPosition = localPosition;

        Light targetLight = AddOrGetComponent<Light>(lightObject);
        targetLight.type = LightType.Point;
        targetLight.color = color;
        targetLight.intensity = intensity;
        targetLight.range = range;
        targetLight.shadows = LightShadows.None;
        return targetLight;
    }

    private static AudioSource CreateDefaultAudioSource(Transform parent, string objectName, float volume)
    {
        GameObject audioObject = FindOrCreateChild(parent, objectName);
        AudioSource audioSource = AddOrGetComponent<AudioSource>(audioObject);
        audioSource.playOnAwake = false;
        audioSource.loop = true;
        audioSource.volume = Mathf.Clamp01(volume);
        audioSource.spatialBlend = 0f;
        return audioSource;
    }

    private static Transform CreateDefaultAnchor(Transform parent, string objectName, Vector3 localPosition, float yawDegrees)
    {
        GameObject anchorObject = FindOrCreateChild(parent, objectName);
        anchorObject.transform.localPosition = localPosition;
        anchorObject.transform.localRotation = Quaternion.Euler(0f, yawDegrees, 0f);
        return anchorObject.transform;
    }

    private static MinLoopWorldGuideMarker CreateGuideMarker(GameObject guideObject, string label)
    {
        MinLoopWorldGuideMarker marker = AddOrGetComponent<MinLoopWorldGuideMarker>(guideObject);
        AssignString(marker, "markerLabel", label);
        return marker;
    }

    private static GameObject FindOrCreateRoot(string objectName)
    {
        GameObject existing = GameObject.Find(objectName);
        if (existing != null)
        {
            return existing;
        }

        GameObject created = new GameObject(objectName);
        Undo.RegisterCreatedObjectUndo(created, "Create " + objectName);
        return created;
    }

    private static GameObject FindOrCreateChild(Transform parent, string objectName)
    {
        Transform existing = parent.Find(objectName);
        if (existing != null)
        {
            return existing.gameObject;
        }

        GameObject created = new GameObject(objectName);
        Undo.RegisterCreatedObjectUndo(created, "Create " + objectName);
        created.transform.SetParent(parent, false);
        return created;
    }

    private static T AddOrGetComponent<T>(GameObject target) where T : Component
    {
        T component = target.GetComponent<T>();
        if (component != null)
        {
            return component;
        }

        return Undo.AddComponent<T>(target);
    }

    private static void AssignObject(Object target, string propertyName, Object value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            Debug.LogWarning("Could not find property '" + propertyName + "' on " + target.name + ".", target);
            return;
        }

        property.objectReferenceValue = value;
        serializedObject.ApplyModifiedProperties();
    }

    private static void AssignObjectArray(Object target, string propertyName, Object[] values)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null || !property.isArray)
        {
            Debug.LogWarning("Could not find array property '" + propertyName + "' on " + target.name + ".", target);
            return;
        }

        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        serializedObject.ApplyModifiedProperties();
    }

    private static void AssignStageRules(Object target, StageRuleTemplate[] templates)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty rulesProperty = serializedObject.FindProperty("rules");
        if (rulesProperty == null || !rulesProperty.isArray)
        {
            Debug.LogWarning("Could not find array property 'rules' on " + target.name + ".", target);
            return;
        }

        rulesProperty.arraySize = templates.Length;
        for (int i = 0; i < templates.Length; i++)
        {
            SerializedProperty ruleProperty = rulesProperty.GetArrayElementAtIndex(i);
            SerializedProperty labelProperty = ruleProperty.FindPropertyRelative("label");
            SerializedProperty targetProperty = ruleProperty.FindPropertyRelative("targetObject");
            SerializedProperty activeStagesProperty = ruleProperty.FindPropertyRelative("activeStages");
            SerializedProperty invertProperty = ruleProperty.FindPropertyRelative("invertMatch");

            if (labelProperty != null)
            {
                labelProperty.stringValue = templates[i].label;
            }

            if (targetProperty != null)
            {
                targetProperty.objectReferenceValue = templates[i].targetObject;
            }

            if (invertProperty != null)
            {
                invertProperty.boolValue = false;
            }

            if (activeStagesProperty == null || !activeStagesProperty.isArray)
            {
                continue;
            }

            activeStagesProperty.arraySize = templates[i].activeStages.Length;
            for (int stageIndex = 0; stageIndex < templates[i].activeStages.Length; stageIndex++)
            {
                activeStagesProperty.GetArrayElementAtIndex(stageIndex).enumValueIndex = (int)templates[i].activeStages[stageIndex];
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private static void AssignLightingRules(Object target, LightingRuleTemplate[] templates)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty rulesProperty = serializedObject.FindProperty("rules");
        if (rulesProperty == null || !rulesProperty.isArray)
        {
            Debug.LogWarning("Could not find array property 'rules' on " + target.name + ".", target);
            return;
        }

        rulesProperty.arraySize = templates.Length;
        for (int i = 0; i < templates.Length; i++)
        {
            SerializedProperty ruleProperty = rulesProperty.GetArrayElementAtIndex(i);
            SetString(ruleProperty.FindPropertyRelative("label"), templates[i].label);
            SetStageArray(ruleProperty.FindPropertyRelative("activeStages"), templates[i].activeStages);
            SetObjectArray(ruleProperty.FindPropertyRelative("lights"), new Object[] { templates[i].targetLight });
            SetBool(ruleProperty.FindPropertyRelative("setLightEnabled"), true);
            SetBool(ruleProperty.FindPropertyRelative("lightEnabled"), true);
            SetBool(ruleProperty.FindPropertyRelative("setLightColor"), true);
            SetColor(ruleProperty.FindPropertyRelative("lightColor"), templates[i].lightColor);
            SetBool(ruleProperty.FindPropertyRelative("setLightIntensity"), true);
            SetFloat(ruleProperty.FindPropertyRelative("lightIntensity"), templates[i].lightIntensity);
            SetBool(ruleProperty.FindPropertyRelative("setLightRange"), true);
            SetFloat(ruleProperty.FindPropertyRelative("lightRange"), templates[i].lightRange);
            SetBool(ruleProperty.FindPropertyRelative("applyAmbientColor"), true);
            SetColor(ruleProperty.FindPropertyRelative("ambientColor"), templates[i].ambientColor);
            SetBool(ruleProperty.FindPropertyRelative("applyFog"), true);
            SetBool(ruleProperty.FindPropertyRelative("fogEnabled"), templates[i].applyFog);
            SetColor(ruleProperty.FindPropertyRelative("fogColor"), templates[i].fogColor);
            SetFloat(ruleProperty.FindPropertyRelative("fogDensity"), templates[i].fogDensity);
            SetFloat(ruleProperty.FindPropertyRelative("transitionSeconds"), 0.45f);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private static void AssignAudioRules(Object target, AudioRuleTemplate[] templates)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty rulesProperty = serializedObject.FindProperty("rules");
        if (rulesProperty == null || !rulesProperty.isArray)
        {
            Debug.LogWarning("Could not find array property 'rules' on " + target.name + ".", target);
            return;
        }

        rulesProperty.arraySize = templates.Length;
        for (int i = 0; i < templates.Length; i++)
        {
            SerializedProperty ruleProperty = rulesProperty.GetArrayElementAtIndex(i);
            SetString(ruleProperty.FindPropertyRelative("label"), templates[i].label);
            SetStageArray(ruleProperty.FindPropertyRelative("activeStages"), templates[i].activeStages);
            SetObjectArray(ruleProperty.FindPropertyRelative("audioSources"), new Object[] { templates[i].audioSource });
            SetBool(ruleProperty.FindPropertyRelative("assignFallbackClipIfMissing"), true);
            SetBool(ruleProperty.FindPropertyRelative("loop"), true);
            SetFloat(ruleProperty.FindPropertyRelative("targetVolume"), templates[i].targetVolume);
            SetBool(ruleProperty.FindPropertyRelative("setSpatialBlend"), true);
            SetFloat(ruleProperty.FindPropertyRelative("spatialBlend"), 0f);
            SetBool(ruleProperty.FindPropertyRelative("setPitch"), false);
            SetFloat(ruleProperty.FindPropertyRelative("pitch"), 1f);
            SetBool(ruleProperty.FindPropertyRelative("restartOnEnter"), false);
            SetBool(ruleProperty.FindPropertyRelative("stopWhenUnmatched"), true);
            SetFloat(ruleProperty.FindPropertyRelative("fadeInSeconds"), templates[i].fadeInSeconds);
            SetFloat(ruleProperty.FindPropertyRelative("fadeOutSeconds"), templates[i].fadeOutSeconds);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private static void AssignAnchorRules(Object target, AnchorRuleTemplate[] templates)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty rulesProperty = serializedObject.FindProperty("rules");
        if (rulesProperty == null || !rulesProperty.isArray)
        {
            Debug.LogWarning("Could not find array property 'rules' on " + target.name + ".", target);
            return;
        }

        rulesProperty.arraySize = templates.Length;
        for (int i = 0; i < templates.Length; i++)
        {
            SerializedProperty ruleProperty = rulesProperty.GetArrayElementAtIndex(i);
            SetString(ruleProperty.FindPropertyRelative("label"), templates[i].label);
            SetStageArray(ruleProperty.FindPropertyRelative("activeStages"), templates[i].activeStages);
            SetObject(ruleProperty.FindPropertyRelative("targetRoot"), templates[i].targetRoot);
            SetObject(ruleProperty.FindPropertyRelative("anchor"), templates[i].anchor);
            SetBool(ruleProperty.FindPropertyRelative("applyPosition"), true);
            SetBool(ruleProperty.FindPropertyRelative("applyRotation"), true);
            SetBool(ruleProperty.FindPropertyRelative("yawOnly"), true);
            SetBool(ruleProperty.FindPropertyRelative("resetLookLocalRotation"), true);
            SetBool(ruleProperty.FindPropertyRelative("syncFirstPersonLook"), true);
            SetBool(ruleProperty.FindPropertyRelative("clearRigidbodyVelocity"), true);
        }

        serializedObject.ApplyModifiedProperties();
    }

    private static void AssignCueRules(Object target, CueRuleTemplate[] templates)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty rulesProperty = serializedObject.FindProperty("rules");
        if (rulesProperty == null || !rulesProperty.isArray)
        {
            Debug.LogWarning("Could not find array property 'rules' on " + target.name + ".", target);
            return;
        }

        rulesProperty.arraySize = templates.Length;
        for (int i = 0; i < templates.Length; i++)
        {
            SerializedProperty ruleProperty = rulesProperty.GetArrayElementAtIndex(i);
            SetString(ruleProperty.FindPropertyRelative("label"), templates[i].label);
            SetStageArray(ruleProperty.FindPropertyRelative("activeStages"), templates[i].activeStages);
            SetBool(ruleProperty.FindPropertyRelative("triggerOnce"), true);
            SetFloat(ruleProperty.FindPropertyRelative("delaySeconds"), 0f);
            SetObjectArray(ruleProperty.FindPropertyRelative("feedbackObjects"), new Object[] { templates[i].feedback });
        }

        serializedObject.ApplyModifiedProperties();
    }

    private static void AssignString(Object target, string propertyName, string value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            Debug.LogWarning("Could not find property '" + propertyName + "' on " + target.name + ".", target);
            return;
        }

        property.stringValue = value;
        serializedObject.ApplyModifiedProperties();
    }

    private static void AssignBool(Object target, string propertyName, bool value)
    {
        SerializedObject serializedObject = new SerializedObject(target);
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            Debug.LogWarning("Could not find property '" + propertyName + "' on " + target.name + ".", target);
            return;
        }

        property.boolValue = value;
        serializedObject.ApplyModifiedProperties();
    }

    private static void SetString(SerializedProperty property, string value)
    {
        if (property != null)
        {
            property.stringValue = value;
        }
    }

    private static void SetBool(SerializedProperty property, bool value)
    {
        if (property != null)
        {
            property.boolValue = value;
        }
    }

    private static void SetFloat(SerializedProperty property, float value)
    {
        if (property != null)
        {
            property.floatValue = value;
        }
    }

    private static void SetColor(SerializedProperty property, Color value)
    {
        if (property != null)
        {
            property.colorValue = value;
        }
    }

    private static void SetObject(SerializedProperty property, Object value)
    {
        if (property != null)
        {
            property.objectReferenceValue = value;
        }
    }

    private static void SetStageArray(SerializedProperty property, MinLoopStage[] stages)
    {
        if (property == null || !property.isArray || stages == null)
        {
            return;
        }

        property.arraySize = stages.Length;
        for (int i = 0; i < stages.Length; i++)
        {
            property.GetArrayElementAtIndex(i).enumValueIndex = (int)stages[i];
        }
    }

    private static void SetObjectArray(SerializedProperty property, Object[] values)
    {
        if (property == null || !property.isArray || values == null)
        {
            return;
        }

        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }

    private struct StageRuleTemplate
    {
        public readonly string label;
        public readonly GameObject targetObject;
        public readonly MinLoopStage[] activeStages;

        public StageRuleTemplate(string label, GameObject targetObject, MinLoopStage[] activeStages)
        {
            this.label = label;
            this.targetObject = targetObject;
            this.activeStages = activeStages;
        }
    }

    private struct LightingRuleTemplate
    {
        public readonly string label;
        public readonly Light targetLight;
        public readonly MinLoopStage[] activeStages;
        public readonly Color lightColor;
        public readonly float lightIntensity;
        public readonly float lightRange;
        public readonly Color ambientColor;
        public readonly bool applyFog;
        public readonly Color fogColor;
        public readonly float fogDensity;

        public LightingRuleTemplate(
            string label,
            Light targetLight,
            MinLoopStage[] activeStages,
            Color lightColor,
            float lightIntensity,
            float lightRange,
            Color ambientColor,
            bool applyFog,
            Color fogColor,
            float fogDensity)
        {
            this.label = label;
            this.targetLight = targetLight;
            this.activeStages = activeStages;
            this.lightColor = lightColor;
            this.lightIntensity = lightIntensity;
            this.lightRange = lightRange;
            this.ambientColor = ambientColor;
            this.applyFog = applyFog;
            this.fogColor = fogColor;
            this.fogDensity = fogDensity;
        }
    }

    private struct AudioRuleTemplate
    {
        public readonly string label;
        public readonly AudioSource audioSource;
        public readonly MinLoopStage[] activeStages;
        public readonly float targetVolume;
        public readonly float fadeInSeconds;
        public readonly float fadeOutSeconds;

        public AudioRuleTemplate(
            string label,
            AudioSource audioSource,
            MinLoopStage[] activeStages,
            float targetVolume,
            float fadeInSeconds,
            float fadeOutSeconds)
        {
            this.label = label;
            this.audioSource = audioSource;
            this.activeStages = activeStages;
            this.targetVolume = targetVolume;
            this.fadeInSeconds = fadeInSeconds;
            this.fadeOutSeconds = fadeOutSeconds;
        }
    }

    private struct AnchorRuleTemplate
    {
        public readonly string label;
        public readonly Transform targetRoot;
        public readonly Transform anchor;
        public readonly MinLoopStage[] activeStages;

        public AnchorRuleTemplate(string label, Transform targetRoot, Transform anchor, MinLoopStage[] activeStages)
        {
            this.label = label;
            this.targetRoot = targetRoot;
            this.anchor = anchor;
            this.activeStages = activeStages;
        }
    }

    private struct CueRuleTemplate
    {
        public readonly string label;
        public readonly InteractionFeedbackController feedback;
        public readonly MinLoopStage[] activeStages;

        public CueRuleTemplate(string label, InteractionFeedbackController feedback, MinLoopStage[] activeStages)
        {
            this.label = label;
            this.feedback = feedback;
            this.activeStages = activeStages;
        }
    }
}
