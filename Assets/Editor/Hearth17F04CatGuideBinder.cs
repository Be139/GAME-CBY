#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Hearth17F04CatGuideBinder
{
    private const string GuideRootPath = "MIN_LOOP_ROOT/Finale_17F04/CatGuide";
    private const string RuntimeCatName = "CatMoveRoot";
    private const string MoveClipPath = "Assets/IndieCat Animals/Cats/Animations/A_Cat_Move.fbx";
    private const string RestClipPath = "Assets/IndieCat Animals/Cats/Animations/A_Cat_Rest.fbx";

    private static readonly float[] DefaultDurations = { 1.5f, 1.5f, 1.5f, 1.5f, 7.5f, 0.5f, 0.5f };
    private static readonly float[] LegacyDefaultDurations = { 3f, 3f, 3f, 3f, 15f, 1f, 1f };
    private const float DefaultPathSmoothing = 0.75f;
    private const float DefaultWalkPlaybackSpeed = 2f;

    [MenuItem("Tools/Hearth/Finale/Apply 17F04 Cat Guide Setup")]
    public static void ApplySetup()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("[Hearth17F04CatGuideBinder] No loaded scene is available.");
            return;
        }

        Transform runtimeCat = FindSceneTransform(RuntimeCatName);
        Transform[] references = new Transform[7];
        for (int i = 0; i < references.Length; i++)
        {
            references[i] = FindSceneTransform(RuntimeCatName + " (" + (i + 1) + ")");
        }

        List<string> missing = new List<string>();
        if (runtimeCat == null) missing.Add(RuntimeCatName);
        for (int i = 0; i < references.Length; i++)
        {
            if (references[i] == null) missing.Add(RuntimeCatName + " (" + (i + 1) + ")");
        }

        if (missing.Count > 0)
        {
            Debug.LogError("[Hearth17F04CatGuideBinder] Missing cat route object(s): " + string.Join(", ", missing.ToArray()));
            return;
        }

        Transform guideRoot = EnsureHierarchy(GuideRootPath);
        ReparentPreservingWorld(runtimeCat, guideRoot);
        for (int i = 0; i < references.Length; i++)
        {
            ReparentPreservingWorld(references[i], guideRoot);
            ConfigureReferenceModel(references[i].gameObject);
        }

        Animator catAnimator = ConfigureRuntimeAnimator(runtimeCat);
        HearthActorAnimationPlayer animationPlayer = ConfigureAnimationPlayer(runtimeCat.gameObject, catAnimator);
        Hearth17F04CatGuideController guide = ConfigureGuide(runtimeCat.gameObject, animationPlayer, references);
        BindFinaleController(guide);

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[Hearth17F04CatGuideBinder] Cat guide applied. Existing CatMoveRoot positions and rotations were preserved.");
        ValidateSetup();
    }

    [MenuItem("Tools/Hearth/Finale/Validate 17F04 Cat Guide Setup")]
    public static void ValidateSetup()
    {
        List<string> errors = new List<string>();
        Transform runtimeCat = FindSceneTransform(RuntimeCatName);
        if (runtimeCat == null)
        {
            errors.Add("CatMoveRoot is missing.");
        }
        else
        {
            Hearth17F04CatGuideController guide = runtimeCat.GetComponent<Hearth17F04CatGuideController>();
            HearthActorAnimationPlayer player = runtimeCat.GetComponent<HearthActorAnimationPlayer>();
            if (guide == null) errors.Add("CatMoveRoot has no Hearth17F04CatGuideController.");
            if (player == null) errors.Add("CatMoveRoot has no HearthActorAnimationPlayer.");
            if (player != null && !player.HasAnimator) errors.Add("The runtime cat has no playable child Animator.");
            if (player != null)
            {
                string[] requiredClips = { "Walk_F", "Run_F", "Lie_to", "Lie_idle" };
                for (int i = 0; i < requiredClips.Length; i++)
                {
                    if (player.GetClipLength(requiredClips[i]) <= 0f)
                    {
                        errors.Add("The runtime cat is missing clip " + requiredClips[i] + ".");
                    }
                }
            }

            ValidateWalkClipImport(errors);

            if (guide != null)
            {
                SerializedObject guideSo = new SerializedObject(guide);
                SerializedProperty route = guideSo.FindProperty("routeSteps");
                if (!MatchesDurationProfile(route, DefaultDurations))
                {
                    errors.Add("The cat route does not use the current 2x-speed duration profile (1.5/1.5/1.5/1.5/7.5/0.5/0.5 seconds).");
                }

                SerializedProperty smoothing = guideSo.FindProperty("pathSmoothing");
                if (smoothing == null || smoothing.floatValue <= 0f)
                {
                    errors.Add("Cat path smoothing is disabled; sharp route turns can visibly hitch.");
                }

                SerializedProperty walkSpeed = guideSo.FindProperty("walkPlaybackSpeed");
                if (walkSpeed == null || !Mathf.Approximately(walkSpeed.floatValue, DefaultWalkPlaybackSpeed))
                {
                    errors.Add("Walk_F playback speed should be 2.0 while Run_F and lie clips remain at 1.0.");
                }
            }

            Animation[] legacyAnimations = runtimeCat.GetComponentsInChildren<Animation>(true);
            if (legacyAnimations.Any(item => item != null))
            {
                errors.Add("The runtime cat still has a legacy Animation component that can conflict with Playables.");
            }
        }

        for (int i = 1; i <= 7; i++)
        {
            Transform reference = FindSceneTransform(RuntimeCatName + " (" + i + ")");
            if (reference == null)
            {
                errors.Add(RuntimeCatName + " (" + i + ") is missing.");
                continue;
            }

            if (reference.GetComponent<HearthEditorOnlyReferenceModel>() == null)
            {
                errors.Add(reference.name + " is not marked as an editor-only reference model.");
            }
        }

        Hearth17F04FinaleController finale = Object.FindObjectOfType<Hearth17F04FinaleController>(true);
        if (finale != null && runtimeCat != null)
        {
            SerializedObject finaleSo = new SerializedObject(finale);
            SerializedProperty catProperty = finaleSo.FindProperty("catGuide");
            if (catProperty == null || catProperty.objectReferenceValue != runtimeCat.GetComponent<Hearth17F04CatGuideController>())
            {
                errors.Add("Hearth17F04FinaleController is not bound to the cat guide.");
            }
        }

        if (errors.Count == 0)
        {
            Debug.Log("[Hearth17F04CatGuideBinder] Validation passed: one runtime cat, seven hidden runtime references, and four animation clips are configured.");
        }
        else
        {
            Debug.LogError("[Hearth17F04CatGuideBinder] Validation found " + errors.Count + " issue(s):\n- " + string.Join("\n- ", errors.ToArray()));
        }
    }

    private static Hearth17F04CatGuideController ConfigureGuide(
        GameObject runtimeCat,
        HearthActorAnimationPlayer animationPlayer,
        Transform[] references)
    {
        Hearth17F04CatGuideController guide = GetOrAdd<Hearth17F04CatGuideController>(runtimeCat);
        SerializedObject so = new SerializedObject(guide);
        SetObject(so, "actorRoot", runtimeCat.transform);
        SetObject(so, "animationPlayer", animationPlayer);
        SetFloat(so, "jumpArcHeight", Mathf.Max(0.25f, GetFloat(so, "jumpArcHeight", 0.25f)));
        SerializedProperty smoothing = so.FindProperty("pathSmoothing");
        if (smoothing != null && smoothing.floatValue <= 0f)
        {
            smoothing.floatValue = DefaultPathSmoothing;
        }

        SerializedProperty walkSpeed = so.FindProperty("walkPlaybackSpeed");
        if (walkSpeed != null && (walkSpeed.floatValue <= 0f || Mathf.Approximately(walkSpeed.floatValue, 1f)))
        {
            walkSpeed.floatValue = DefaultWalkPlaybackSpeed;
        }

        SerializedProperty route = so.FindProperty("routeSteps");
        bool initializeDurations = route == null || route.arraySize != references.Length;
        bool migrateLegacyDurations = MatchesDurationProfile(route, LegacyDefaultDurations);
        if (route != null)
        {
            route.arraySize = references.Length;
            for (int i = 0; i < references.Length; i++)
            {
                SerializedProperty step = route.GetArrayElementAtIndex(i);
                step.FindPropertyRelative("target").objectReferenceValue = references[i];
                SerializedProperty duration = step.FindPropertyRelative("duration");
                if (initializeDurations || migrateLegacyDurations || duration.floatValue <= 0f)
                {
                    duration.floatValue = DefaultDurations[i];
                }

                step.FindPropertyRelative("motion").enumValueIndex =
                    i == references.Length - 1
                        ? (int)Hearth17F04CatGuideController.CatMotion.RunJump
                        : (int)Hearth17F04CatGuideController.CatMotion.Walk;
            }
        }

        SerializedProperty hasStartPose = so.FindProperty("hasStartPose");
        if (hasStartPose != null && !hasStartPose.boolValue)
        {
            SetVector3(so, "startWorldPosition", runtimeCat.transform.position);
            SetQuaternion(so, "startWorldRotation", runtimeCat.transform.rotation);
            hasStartPose.boolValue = true;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(guide);
        return guide;
    }

    private static HearthActorAnimationPlayer ConfigureAnimationPlayer(GameObject runtimeCat, Animator animator)
    {
        ConfigureWalkClipAsInPlace();

        AnimationClip walk = LoadClip(MoveClipPath, "Walk_F");
        AnimationClip run = LoadClip(MoveClipPath, "Run_F");
        AnimationClip lieTo = LoadClip(RestClipPath, "Lie_to");
        AnimationClip lieIdle = LoadClip(RestClipPath, "Lie_idle");

        HearthActorAnimationPlayer player = GetOrAdd<HearthActorAnimationPlayer>(runtimeCat);
        SerializedObject so = new SerializedObject(player);
        SetObject(so, "animator", animator);
        SetBool(so, "playOnEnable", false);
        SetBool(so, "useUnscaledTime", false);

        SerializedProperty clips = so.FindProperty("clips");
        clips.arraySize = 4;
        ConfigureClipSlot(clips.GetArrayElementAtIndex(0), "Walk_F", walk, true, true, 0.12f);
        ConfigureClipSlot(clips.GetArrayElementAtIndex(1), "Run_F", run, false, false, 0.08f);
        ConfigureClipSlot(clips.GetArrayElementAtIndex(2), "Lie_to", lieTo, false, false, 0.15f);
        ConfigureClipSlot(clips.GetArrayElementAtIndex(3), "Lie_idle", lieIdle, true, true, 0.16f);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(player);
        return player;
    }

    private static void ConfigureWalkClipAsInPlace()
    {
        ModelImporter importer = AssetImporter.GetAtPath(MoveClipPath) as ModelImporter;
        if (importer == null)
        {
            Debug.LogWarning("[Hearth17F04CatGuideBinder] Could not configure Walk_F as in-place because its ModelImporter is missing.");
            return;
        }

        ModelImporterClipAnimation[] clips = importer.clipAnimations;
        if (clips == null || clips.Length == 0)
        {
            clips = importer.defaultClipAnimations;
        }

        bool found = false;
        bool changed = false;
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i].name != "Walk_F")
            {
                continue;
            }

            found = true;
            if (!clips[i].lockRootPositionXZ)
            {
                clips[i].lockRootPositionXZ = true;
                changed = true;
            }

            if (!clips[i].loopPose)
            {
                clips[i].loopPose = true;
                changed = true;
            }
        }

        if (!found)
        {
            Debug.LogWarning("[Hearth17F04CatGuideBinder] Walk_F was not found in the movement FBX import settings.");
            return;
        }

        if (changed)
        {
            importer.clipAnimations = clips;
            importer.SaveAndReimport();
        }
    }

    private static void ValidateWalkClipImport(List<string> errors)
    {
        ModelImporter importer = AssetImporter.GetAtPath(MoveClipPath) as ModelImporter;
        if (importer == null)
        {
            errors.Add("The movement FBX ModelImporter is missing.");
            return;
        }

        ModelImporterClipAnimation walk = importer.clipAnimations
            .FirstOrDefault(item => item != null && item.name == "Walk_F");
        if (walk == null)
        {
            errors.Add("Walk_F import settings are missing.");
        }
        else if (!walk.lockRootPositionXZ)
        {
            errors.Add("Walk_F is not imported in-place. Enable Bake Into Pose for Root Transform Position (XZ).");
        }
        else if (!walk.loopPose)
        {
            errors.Add("Walk_F Loop Pose is disabled; the end pose can snap back to the first frame.");
        }
    }

    private static bool MatchesDurationProfile(SerializedProperty route, float[] profile)
    {
        if (route == null || profile == null || route.arraySize != profile.Length)
        {
            return false;
        }

        for (int i = 0; i < profile.Length; i++)
        {
            SerializedProperty step = route.GetArrayElementAtIndex(i);
            SerializedProperty duration = step != null ? step.FindPropertyRelative("duration") : null;
            if (duration == null || !Mathf.Approximately(duration.floatValue, profile[i]))
            {
                return false;
            }
        }

        return true;
    }

    private static Animator ConfigureRuntimeAnimator(Transform runtimeCat)
    {
        Animator[] rootAnimators = runtimeCat.GetComponents<Animator>();
        for (int i = 0; i < rootAnimators.Length; i++)
        {
            rootAnimators[i].enabled = false;
            EditorUtility.SetDirty(rootAnimators[i]);
        }

        foreach (Animation legacy in runtimeCat.GetComponentsInChildren<Animation>(true))
        {
            if (legacy != null)
            {
                Undo.DestroyObjectImmediate(legacy);
            }
        }

        Transform animatorHost = runtimeCat.GetComponentsInChildren<Transform>(true)
            .FirstOrDefault(item => item != runtimeCat && item.name == "Cat_L_Red-D");
        if (animatorHost == null)
        {
            Renderer renderer = runtimeCat.GetComponentInChildren<SkinnedMeshRenderer>(true);
            animatorHost = renderer != null ? renderer.transform.parent : runtimeCat;
        }

        Animator animator = GetOrAdd<Animator>(animatorHost.gameObject);
        animator.runtimeAnimatorController = null;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        animator.enabled = true;
        EditorUtility.SetDirty(animator);
        return animator;
    }

    private static void ConfigureReferenceModel(GameObject reference)
    {
        GetOrAdd<HearthEditorOnlyReferenceModel>(reference);
        foreach (Animation legacy in reference.GetComponentsInChildren<Animation>(true))
        {
            if (legacy != null)
            {
                Undo.DestroyObjectImmediate(legacy);
            }
        }

        EditorUtility.SetDirty(reference);
    }

    private static void BindFinaleController(Hearth17F04CatGuideController guide)
    {
        Hearth17F04FinaleController finale = Object.FindObjectOfType<Hearth17F04FinaleController>(true);
        if (finale == null)
        {
            Debug.LogWarning("[Hearth17F04CatGuideBinder] The cat is configured, but no Hearth17F04FinaleController was found to start it.");
            return;
        }

        SerializedObject so = new SerializedObject(finale);
        SetObject(so, "catGuide", guide);
        HearthFirstPersonHudInput hudInput = Object.FindObjectOfType<HearthFirstPersonHudInput>(true);
        SetObject(so, "firstPersonHudInput", hudInput);
        so.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(finale);
    }

    private static void ConfigureClipSlot(
        SerializedProperty slot,
        string id,
        AnimationClip clip,
        bool loop,
        bool seamlessLoop,
        float fade)
    {
        slot.FindPropertyRelative("clipId").stringValue = id;
        slot.FindPropertyRelative("clip").objectReferenceValue = clip;
        slot.FindPropertyRelative("loop").boolValue = loop;
        SerializedProperty seamless = slot.FindPropertyRelative("seamlessLoop");
        if (seamless != null) seamless.boolValue = seamlessLoop;
        slot.FindPropertyRelative("applyRootMotion").boolValue = false;
        slot.FindPropertyRelative("applyFootIk").boolValue = false;
        slot.FindPropertyRelative("stabilizeAnimatorTransform").boolValue = true;
        slot.FindPropertyRelative("fadeSeconds").floatValue = fade;
        slot.FindPropertyRelative("playbackSpeed").floatValue = 1f;
    }

    private static AnimationClip LoadClip(string path, string clipName)
    {
        AnimationClip clip = AssetDatabase.LoadAllAssetsAtPath(path)
            .OfType<AnimationClip>()
            .FirstOrDefault(item => !item.name.StartsWith("__preview__") && item.name == clipName);
        if (clip == null)
        {
            Debug.LogWarning("[Hearth17F04CatGuideBinder] Animation clip '" + clipName + "' was not found at " + path + ". Route movement will still continue.");
        }

        return clip;
    }

    private static Transform FindSceneTransform(string exactName)
    {
        Scene scene = SceneManager.GetActiveScene();
        return Resources.FindObjectsOfTypeAll<Transform>()
            .FirstOrDefault(item => item != null && item.gameObject.scene == scene && item.name == exactName);
    }

    private static Transform EnsureHierarchy(string path)
    {
        string[] parts = path.Split('/');
        Transform current = null;
        for (int i = 0; i < parts.Length; i++)
        {
            Transform next = current == null
                ? SceneManager.GetActiveScene().GetRootGameObjects()
                    .Select(item => item.transform)
                    .FirstOrDefault(item => item.name == parts[i])
                : current.Find(parts[i]);
            if (next == null)
            {
                GameObject created = new GameObject(parts[i]);
                Undo.RegisterCreatedObjectUndo(created, "Create 17F04 cat guide hierarchy");
                next = created.transform;
                if (current != null) next.SetParent(current, false);
            }

            current = next;
        }

        return current;
    }

    private static void ReparentPreservingWorld(Transform child, Transform parent)
    {
        if (child == null || parent == null || child.parent == parent)
        {
            return;
        }

        Undo.SetTransformParent(child, parent, "Group 17F04 cat guide objects");
    }

    private static T GetOrAdd<T>(GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        return component != null ? component : Undo.AddComponent<T>(gameObject);
    }

    private static void SetObject(SerializedObject so, string name, Object value)
    {
        SerializedProperty property = so.FindProperty(name);
        if (property != null) property.objectReferenceValue = value;
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

    private static float GetFloat(SerializedObject so, string name, float fallback)
    {
        SerializedProperty property = so.FindProperty(name);
        return property != null ? property.floatValue : fallback;
    }

    private static void SetVector3(SerializedObject so, string name, Vector3 value)
    {
        SerializedProperty property = so.FindProperty(name);
        if (property != null) property.vector3Value = value;
    }

    private static void SetQuaternion(SerializedObject so, string name, Quaternion value)
    {
        SerializedProperty property = so.FindProperty(name);
        if (property != null) property.quaternionValue = value;
    }
}
#endif
