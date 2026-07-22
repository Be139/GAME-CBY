using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class HearthCompanionHudExclusiveMode : MonoBehaviour
{
    [Header("Mode")]
    [SerializeField] private ViewSwitchController viewSwitchController;
    [SerializeField] private bool autoFindViewSwitchController = true;
    [SerializeField] private bool suppressInCompanionView = true;
    [SerializeField] private bool suppressWhenThisHudVisible = true;
    [SerializeField] private CanvasGroup ownCanvasGroup;

    [Header("Human HUD Targets")]
    [SerializeField] private string[] autoSuppressRootNames = { "HearthHudRoot" };
    [SerializeField] private CanvasGroup[] canvasGroupsToSuppress;

    private readonly List<SuppressedGroupState> runtimeGroups = new List<SuppressedGroupState>();
    private bool suppressing;

    private void Awake()
    {
        ResolveReferences();
        CaptureGroups();
    }

    private void OnEnable()
    {
        ResolveReferences();
        CaptureGroups();
        RefreshSuppression();
    }

    private void LateUpdate()
    {
        RefreshSuppression();
    }

    private void OnDisable()
    {
        RestoreSuppressedGroups();
    }

    public void SetViewSwitchController(ViewSwitchController controller)
    {
        viewSwitchController = controller;
        RefreshSuppression();
    }

    public void RefreshTargets()
    {
        CaptureGroups();
        RefreshSuppression();
    }

    private void ResolveReferences()
    {
        if (autoFindViewSwitchController &&
            (viewSwitchController == null ||
             !viewSwitchController.enabled ||
             !viewSwitchController.gameObject.activeInHierarchy))
        {
            viewSwitchController = ViewSwitchController.FindPreferredController();
        }

        if (ownCanvasGroup == null)
        {
            ownCanvasGroup = GetComponent<CanvasGroup>();
        }
    }

    private void CaptureGroups()
    {
        runtimeGroups.Clear();

        if (canvasGroupsToSuppress != null)
        {
            for (int i = 0; i < canvasGroupsToSuppress.Length; i++)
            {
                AddGroup(canvasGroupsToSuppress[i]);
            }
        }

        if (autoSuppressRootNames == null)
        {
            return;
        }

        for (int i = 0; i < autoSuppressRootNames.Length; i++)
        {
            string rootName = autoSuppressRootNames[i];
            if (string.IsNullOrEmpty(rootName))
            {
                continue;
            }

            GameObject root = GameObject.Find(rootName);
            if (root == null || root == gameObject || root.transform.IsChildOf(transform))
            {
                continue;
            }

            CanvasGroup group = root.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = root.AddComponent<CanvasGroup>();
            }

            AddGroup(group);
        }
    }

    private void AddGroup(CanvasGroup group)
    {
        if (group == null)
        {
            return;
        }

        for (int i = 0; i < runtimeGroups.Count; i++)
        {
            if (runtimeGroups[i].group == group)
            {
                return;
            }
        }

        runtimeGroups.Add(new SuppressedGroupState(group));
    }

    private void RefreshSuppression()
    {
        ResolveReferences();

        bool shouldSuppressByMode = suppressInCompanionView &&
            viewSwitchController != null &&
            viewSwitchController.CurrentMode == ViewSwitchController.ViewMode.Companion;

        bool shouldSuppressByHud = suppressWhenThisHudVisible &&
            ownCanvasGroup != null &&
            ownCanvasGroup.alpha > 0.01f &&
            gameObject.activeInHierarchy;

        bool shouldSuppress = shouldSuppressByMode || shouldSuppressByHud;

        if (shouldSuppress == suppressing)
        {
            return;
        }

        suppressing = shouldSuppress;
        if (suppressing)
        {
            CaptureGroups();
            ApplySuppression();
        }
        else
        {
            RestoreSuppressedGroups();
        }
    }

    private void ApplySuppression()
    {
        for (int i = 0; i < runtimeGroups.Count; i++)
        {
            CanvasGroup group = runtimeGroups[i].group;
            if (group == null)
            {
                continue;
            }

            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }
    }

    private void RestoreSuppressedGroups()
    {
        for (int i = 0; i < runtimeGroups.Count; i++)
        {
            runtimeGroups[i].Restore();
        }

        suppressing = false;
    }

    private struct SuppressedGroupState
    {
        public readonly CanvasGroup group;
        private readonly float alpha;
        private readonly bool interactable;
        private readonly bool blocksRaycasts;

        public SuppressedGroupState(CanvasGroup group)
        {
            this.group = group;
            alpha = group != null ? group.alpha : 1f;
            interactable = group != null && group.interactable;
            blocksRaycasts = group != null && group.blocksRaycasts;
        }

        public void Restore()
        {
            if (group == null)
            {
                return;
            }

            group.alpha = alpha;
            group.interactable = interactable;
            group.blocksRaycasts = blocksRaycasts;
        }
    }
}
