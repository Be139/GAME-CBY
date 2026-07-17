#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class Hearth17F03StagingPreviewTool
{
    private const string ReplayRootPath = "MIN_LOOP_ROOT/ReplayRoom_17F03";
    private const string RuntimeActorsPath = ReplayRootPath + "/RuntimeActors";
    private const string AnchorsPath = ReplayRootPath + "/Anchors";
    private const string StagingRootName = "StagingPreview_17F03";

    static Hearth17F03StagingPreviewTool()
    {
        EditorApplication.playModeStateChanged -= HandlePlayModeStateChanged;
        EditorApplication.playModeStateChanged += HandlePlayModeStateChanged;
    }

    [MenuItem("Tools/Hearth/Replay/17F03 Staging/Create Or Update Preview")]
    public static void CreateOrUpdatePreview()
    {
        if (!CanEditScene())
        {
            return;
        }

        Transform runtimeActors = FindSceneTransform(RuntimeActorsPath);
        Transform anchors = FindSceneTransform(AnchorsPath);
        if (runtimeActors == null || anchors == null)
        {
            Debug.LogError("[Hearth17F03StagingPreviewTool] Missing 17F03 RuntimeActors or Anchors. Run Apply 17F03 Minimal Loop Setup first.");
            return;
        }

        Transform replayRoot = FindSceneTransform(ReplayRootPath);
        Transform stagingRoot = EnsureChild(replayRoot, StagingRootName);
        HearthEditorOnlyReferenceModel referenceModel = stagingRoot.GetComponent<HearthEditorOnlyReferenceModel>();
        if (referenceModel == null)
        {
            referenceModel = Undo.AddComponent<HearthEditorOnlyReferenceModel>(stagingRoot.gameObject);
        }

        PreviewSpec[] specs = BuildPreviewSpecs();
        int created = 0;
        for (int i = 0; i < specs.Length; i++)
        {
            PreviewSpec spec = specs[i];
            Transform source = FindDirectChild(runtimeActors, spec.SourceActorName);
            Transform anchor = FindDirectChild(anchors, spec.AnchorName);
            if (source == null || anchor == null)
            {
                Debug.LogWarning("[Hearth17F03StagingPreviewTool] Skipped " + spec.PreviewName + ". Missing source actor or anchor.");
                continue;
            }

            Transform stageRoot = EnsureChild(stagingRoot, spec.StageGroupName);
            GameObject preview = EnsurePreviewActor(stageRoot, source, anchor, spec, out bool wasCreated);
            if (preview != null && wasCreated)
            {
                created++;
            }
        }

        referenceModel.ApplyReferenceState();
        HideRuntimeActorsInSceneView(runtimeActors, true);
        MarkAndSaveScene();
        Selection.activeGameObject = stagingRoot.gameObject;
        Debug.Log("[Hearth17F03StagingPreviewTool] Staging preview is ready. Created " + created + " new preview actor(s). Move or rotate Preview_* objects, then use Apply Preview Poses To Anchors.");
    }

    [MenuItem("Tools/Hearth/Replay/17F03 Staging/Apply Preview Poses To Anchors")]
    public static void ApplyPreviewPosesToAnchors()
    {
        if (!CanEditScene())
        {
            return;
        }

        Transform stagingRoot = FindSceneTransform(ReplayRootPath + "/" + StagingRootName);
        if (stagingRoot == null)
        {
            Debug.LogWarning("[Hearth17F03StagingPreviewTool] No 17F03 staging preview exists. Create it first.");
            return;
        }

        Hearth17F03StagingPoseProxy[] proxies = stagingRoot.GetComponentsInChildren<Hearth17F03StagingPoseProxy>(true);
        int applied = ApplyPreviewPosesToAnchorsInternal(proxies, true);

        Physics.SyncTransforms();
        MarkAndSaveScene();
        Debug.Log("[Hearth17F03StagingPreviewTool] Applied " + applied + " preview pose(s) to the formal 17F03 anchors. Existing gameplay references use these anchors automatically.");
    }

    [MenuItem("Tools/Hearth/Replay/17F03 Staging/Reapply Animation Poses")]
    public static void ReapplyAnimationPoses()
    {
        if (!CanEditScene())
        {
            return;
        }

        Transform stagingRoot = FindSceneTransform(ReplayRootPath + "/" + StagingRootName);
        if (stagingRoot == null)
        {
            Debug.LogWarning("[Hearth17F03StagingPreviewTool] No 17F03 staging preview exists. Create it first.");
            return;
        }

        Hearth17F03StagingPoseProxy[] proxies = stagingRoot.GetComponentsInChildren<Hearth17F03StagingPoseProxy>(true);
        int applied = 0;
        for (int i = 0; i < proxies.Length; i++)
        {
            Hearth17F03StagingPoseProxy proxy = proxies[i];
            if (proxy != null && proxy.ApplyConfiguredPreviewPose())
            {
                EditorUtility.SetDirty(proxy);
                applied++;
            }
        }

        SceneView.RepaintAll();
        MarkAndSaveScene();
        Debug.Log("[Hearth17F03StagingPreviewTool] Reapplied " + applied + " staged animation pose(s).");
    }

    [MenuItem("Tools/Hearth/Replay/17F03 Staging/Reset Preview Poses From Anchors")]
    public static void ResetPreviewPosesFromAnchors()
    {
        if (!CanEditScene())
        {
            return;
        }

        Transform stagingRoot = FindSceneTransform(ReplayRootPath + "/" + StagingRootName);
        if (stagingRoot == null)
        {
            Debug.LogWarning("[Hearth17F03StagingPreviewTool] No 17F03 staging preview exists. Create it first.");
            return;
        }

        Hearth17F03StagingPoseProxy[] proxies = stagingRoot.GetComponentsInChildren<Hearth17F03StagingPoseProxy>(true);
        int reset = 0;
        for (int i = 0; i < proxies.Length; i++)
        {
            Hearth17F03StagingPoseProxy proxy = proxies[i];
            if (proxy == null || proxy.TargetAnchor == null)
            {
                continue;
            }

            Undo.RecordObject(proxy.transform, "Reset 17F03 staging pose from anchor");
            if (proxy.ResetPreviewFromAnchor())
            {
                EditorUtility.SetDirty(proxy.transform);
                reset++;
            }
        }

        MarkAndSaveScene();
        Debug.Log("[Hearth17F03StagingPreviewTool] Reset " + reset + " preview pose(s) from the formal 17F03 anchors.");
    }

    [MenuItem("Tools/Hearth/Replay/17F03 Staging/Remove Preview")]
    public static void RemovePreview()
    {
        if (!CanEditScene())
        {
            return;
        }

        Transform stagingRoot = FindSceneTransform(ReplayRootPath + "/" + StagingRootName);
        Transform runtimeActors = FindSceneTransform(RuntimeActorsPath);
        if (runtimeActors != null)
        {
            HideRuntimeActorsInSceneView(runtimeActors, false);
        }

        if (stagingRoot != null)
        {
            Undo.DestroyObjectImmediate(stagingRoot.gameObject);
            MarkAndSaveScene();
            Debug.Log("[Hearth17F03StagingPreviewTool] Removed the 17F03 staging preview and restored the formal actors in Scene view.");
        }
    }

    private static GameObject EnsurePreviewActor(
        Transform stageRoot,
        Transform sourceActor,
        Transform anchor,
        PreviewSpec spec,
        out bool wasCreated)
    {
        wasCreated = false;
        Transform existing = FindDirectChild(stageRoot, spec.PreviewName);
        GameObject preview;
        if (existing == null)
        {
            preview = UnityEngine.Object.Instantiate(sourceActor.gameObject);
            preview.name = spec.PreviewName;
            Undo.RegisterCreatedObjectUndo(preview, "Create 17F03 staging preview actor");
            preview.transform.SetParent(stageRoot, true);
            preview.transform.SetPositionAndRotation(anchor.position, anchor.rotation);
            preview.SetActive(true);
            DisablePreviewRuntimeComponents(preview);
            wasCreated = true;
        }
        else
        {
            preview = existing.gameObject;
            preview.SetActive(true);

            if (RestoreMissingPreviewVisual(preview, sourceActor))
            {
                wasCreated = true;
            }
        }

        HearthP0StabilityTools.DisableCityPeopleAutoplayInHierarchy(preview, true);

        Hearth17F03StagingPoseProxy proxy = preview.GetComponent<Hearth17F03StagingPoseProxy>();
        if (proxy == null)
        {
            proxy = Undo.AddComponent<Hearth17F03StagingPoseProxy>(preview);
        }
        proxy.Configure(anchor, spec.StageLabel, spec.AnimatorStateName, spec.NormalizedTime);
        EditorUtility.SetDirty(proxy);

        proxy.ApplyConfiguredPreviewPose();
        return preview;
    }

    private static bool RestoreMissingPreviewVisual(GameObject preview, Transform sourceActor)
    {
        Transform sourceVisual = FindPrimaryVisualChild(sourceActor);
        if (sourceVisual == null)
        {
            return false;
        }

        Transform existingVisual = FindDirectChild(preview.transform, sourceVisual.name);
        if (existingVisual != null && existingVisual.GetComponentsInChildren<Renderer>(true).Length > 0)
        {
            return false;
        }

        if (existingVisual != null)
        {
            Undo.DestroyObjectImmediate(existingVisual.gameObject);
        }

        GameObject restoredVisual = UnityEngine.Object.Instantiate(sourceVisual.gameObject);
        restoredVisual.name = sourceVisual.name;
        Undo.RegisterCreatedObjectUndo(restoredVisual, "Restore missing 17F03 staging preview visual");
        restoredVisual.transform.SetParent(preview.transform, false);
        restoredVisual.transform.localPosition = sourceVisual.localPosition;
        restoredVisual.transform.localRotation = sourceVisual.localRotation;
        restoredVisual.transform.localScale = sourceVisual.localScale;
        DisablePreviewRuntimeComponents(restoredVisual);
        return true;
    }

    private static void DisablePreviewRuntimeComponents(GameObject preview)
    {
        Collider[] colliders = preview.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        CharacterController[] controllers = preview.GetComponentsInChildren<CharacterController>(true);
        for (int i = 0; i < controllers.Length; i++)
        {
            controllers[i].enabled = false;
        }

        Rigidbody[] rigidbodies = preview.GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            rigidbodies[i].isKinematic = true;
            rigidbodies[i].detectCollisions = false;
        }

        NavMeshAgent[] agents = preview.GetComponentsInChildren<NavMeshAgent>(true);
        for (int i = 0; i < agents.Length; i++)
        {
            agents[i].enabled = false;
        }

        AudioSource[] audioSources = preview.GetComponentsInChildren<AudioSource>(true);
        for (int i = 0; i < audioSources.Length; i++)
        {
            audioSources[i].enabled = false;
        }

        Camera[] cameras = preview.GetComponentsInChildren<Camera>(true);
        for (int i = 0; i < cameras.Length; i++)
        {
            cameras[i].enabled = false;
        }

        AudioListener[] listeners = preview.GetComponentsInChildren<AudioListener>(true);
        for (int i = 0; i < listeners.Length; i++)
        {
            listeners[i].enabled = false;
        }

        MonoBehaviour[] behaviours = preview.GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < behaviours.Length; i++)
        {
            if (behaviours[i] != null)
            {
                behaviours[i].enabled = false;
            }
        }
    }

    private static int ApplyPreviewPosesToAnchorsInternal(Hearth17F03StagingPoseProxy[] proxies, bool recordUndo)
    {
        int applied = 0;
        for (int i = 0; i < proxies.Length; i++)
        {
            Hearth17F03StagingPoseProxy proxy = proxies[i];
            if (proxy == null || proxy.TargetAnchor == null)
            {
                continue;
            }

            if (recordUndo)
            {
                Undo.RecordObject(proxy.TargetAnchor, "Apply 17F03 staging pose to anchor");
            }

            if (proxy.ApplyPreviewPoseToAnchor())
            {
                EditorUtility.SetDirty(proxy.TargetAnchor);
                applied++;
            }
        }

        return applied;
    }

    private static void HandlePlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingEditMode)
        {
            return;
        }

        Transform stagingRoot = FindSceneTransform(ReplayRootPath + "/" + StagingRootName);
        if (stagingRoot == null)
        {
            return;
        }

        Hearth17F03StagingPoseProxy[] proxies = stagingRoot.GetComponentsInChildren<Hearth17F03StagingPoseProxy>(true);
        int applied = ApplyPreviewPosesToAnchorsInternal(proxies, false);
        if (applied > 0)
        {
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log("[Hearth17F03StagingPreviewTool] Auto-applied " + applied + " preview pose(s) before Play Mode.");
        }
    }

    private static void HideRuntimeActorsInSceneView(Transform runtimeActors, bool hidden)
    {
        if (runtimeActors == null)
        {
            return;
        }

        for (int i = 0; i < runtimeActors.childCount; i++)
        {
            GameObject actor = runtimeActors.GetChild(i).gameObject;
            if (hidden)
            {
                SceneVisibilityManager.instance.Hide(actor, true);
            }
            else
            {
                SceneVisibilityManager.instance.Show(actor, true);
            }
        }
    }

    private static PreviewSpec[] BuildPreviewSpecs()
    {
        return new[]
        {
            new PreviewSpec("Act01_HumanEntry", "Preview_Mother_Human", "Actor_Mother_17F03_RuntimeRoot", "Anchor_Mother_17F03_Human", "Act 01: mother", "SitToStand", 0f),
            new PreviewSpec("Act01_HumanEntry", "Preview_Father_Human", "Actor_Father_17F03_RuntimeRoot", "Anchor_Father_17F03_Human", "Act 01: father", "Sitting", 0.2f),
            new PreviewSpec("Act02_MiddayMediation", "Preview_Mother_Midday", "Actor_Mother_17F03_RuntimeRoot", "Anchor_Mother_17F03_Midday", "Act 02: mother", "StandingArguing", 0.3f),
            new PreviewSpec("Act02_MiddayMediation", "Preview_Father_Midday", "Actor_Father_17F03_MiddayRuntimeRoot", "Anchor_Father_17F03_Midday", "Act 02: father", "Sitting", 0.2f),
            new PreviewSpec("Act02_MiddayMediation", "Preview_Daughter_Midday", "Actor_Daughter_17F03_RuntimeRoot", "Anchor_Daughter_17F03_Midday", "Act 02: daughter", "SittingPose", 0.2f),
            new PreviewSpec("Act03_NightShutdown", "Preview_Daughter_NightStart", "Actor_Daughter_17F03_RuntimeRoot", "Anchor_Daughter_17F03_NightDoorStart", "Act 03: daughter start", "Walk", 0.15f),
            new PreviewSpec("Act03_NightShutdown", "Preview_Daughter_NightPath_01", "Actor_Daughter_17F03_RuntimeRoot", "Anchor_Daughter_17F03_NightPath_01", "Act 03: daughter path", "Walk", 0.55f),
            new PreviewSpec("Act03_NightShutdown", "Preview_Daughter_NightApproach", "Actor_Daughter_17F03_RuntimeRoot", "Anchor_Daughter_17F03_NightApproach", "Act 03: daughter approach", "Talking", 0.3f)
        };
    }

    private static Transform EnsureChild(Transform parent, string childName)
    {
        Transform child = FindDirectChild(parent, childName);
        if (child != null)
        {
            return child;
        }

        GameObject childObject = new GameObject(childName);
        Undo.RegisterCreatedObjectUndo(childObject, "Create 17F03 staging preview group");
        childObject.transform.SetParent(parent, false);
        return childObject.transform;
    }

    private static Transform FindSceneTransform(string hierarchyPath)
    {
        if (string.IsNullOrEmpty(hierarchyPath))
        {
            return null;
        }

        string[] parts = hierarchyPath.Split('/');
        Scene scene = SceneManager.GetActiveScene();
        GameObject[] roots = scene.GetRootGameObjects();
        Transform current = null;
        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i].name == parts[0])
            {
                current = roots[i].transform;
                break;
            }
        }

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

        return current;
    }

    private static Transform FindDirectChild(Transform parent, string childName)
    {
        if (parent == null)
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

    private static Transform FindPrimaryVisualChild(Transform actorRoot)
    {
        if (actorRoot == null)
        {
            return null;
        }

        for (int i = 0; i < actorRoot.childCount; i++)
        {
            Transform child = actorRoot.GetChild(i);
            if (child.GetComponentsInChildren<Renderer>(true).Length > 0)
            {
                return child;
            }
        }

        return null;
    }

    private static bool CanEditScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("[Hearth17F03StagingPreviewTool] Exit Play Mode before changing the staging preview.");
            return false;
        }

        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid() || !scene.isLoaded)
        {
            Debug.LogError("[Hearth17F03StagingPreviewTool] No open scene is available.");
            return false;
        }

        return true;
    }

    private static void MarkAndSaveScene()
    {
        Scene scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private struct PreviewSpec
    {
        public readonly string StageGroupName;
        public readonly string PreviewName;
        public readonly string SourceActorName;
        public readonly string AnchorName;
        public readonly string StageLabel;
        public readonly string AnimatorStateName;
        public readonly float NormalizedTime;

        public PreviewSpec(
            string stageGroupName,
            string previewName,
            string sourceActorName,
            string anchorName,
            string stageLabel,
            string animatorStateName,
            float normalizedTime)
        {
            StageGroupName = stageGroupName;
            PreviewName = previewName;
            SourceActorName = sourceActorName;
            AnchorName = anchorName;
            StageLabel = stageLabel;
            AnimatorStateName = animatorStateName;
            NormalizedTime = Mathf.Clamp01(normalizedTime);
        }
    }
}
#endif
