using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ViewSwitchController : MonoBehaviour
{
    public enum ViewMode
    {
        Human,
        Companion
    }

    [System.Serializable]
    public class ViewRig
    {
        public GameObject rootObject;
        public Camera viewCamera;
        public FirstPersonMovement movement;
        public FirstPersonLook look;
        public PlayerInteraction interaction;
        public Rigidbody rigidbody;
        public Canvas canvas;

        public void ResolveMissingReferences()
        {
            if (rootObject == null)
            {
                return;
            }

            if (viewCamera == null)
            {
                viewCamera = rootObject.GetComponentInChildren<Camera>(true);
            }

            if (movement == null)
            {
                movement = rootObject.GetComponentInChildren<FirstPersonMovement>(true);
            }

            if (look == null)
            {
                look = rootObject.GetComponentInChildren<FirstPersonLook>(true);
            }

            if (interaction == null)
            {
                interaction = rootObject.GetComponentInChildren<PlayerInteraction>(true);
            }

            if (rigidbody == null)
            {
                rigidbody = rootObject.GetComponent<Rigidbody>();
                if (rigidbody == null)
                {
                    rigidbody = rootObject.GetComponentInChildren<Rigidbody>(true);
                }
            }

            AssignInteractionCamera();
        }

        public void SetVisualsActive(bool isActive)
        {
            if (viewCamera != null)
            {
                viewCamera.enabled = isActive;

                AudioListener audioListener = viewCamera.GetComponent<AudioListener>();
                if (audioListener != null)
                {
                    audioListener.enabled = isActive;
                }
            }

            if (canvas != null)
            {
                canvas.gameObject.SetActive(isActive);
            }
        }

        public void SetControlsActive(bool isActive)
        {
            if (movement != null)
            {
                movement.enabled = isActive;
            }

            if (look != null)
            {
                look.enabled = isActive;
            }

            if (interaction != null)
            {
                AssignInteractionCamera();
                interaction.SetInteractionEnabled(isActive);
            }

            if (!isActive)
            {
                ClearVelocity();
            }
        }

        public void SetControlsActive(
            bool isActive,
            HearthPlayerControlMask lockedMask)
        {
            if (movement != null)
            {
                movement.enabled =
                    isActive &&
                    (lockedMask & HearthPlayerControlMask.Movement) == 0;
            }

            if (look != null)
            {
                look.enabled =
                    isActive &&
                    (lockedMask & HearthPlayerControlMask.Look) == 0;
            }

            if (interaction != null)
            {
                AssignInteractionCamera();
                interaction.SetInteractionEnabled(
                    isActive &&
                    (lockedMask & HearthPlayerControlMask.Interaction) == 0);
            }

            if (!isActive ||
                (lockedMask & HearthPlayerControlMask.Movement) != 0)
            {
                ClearVelocity();
            }
        }

        public void ClearVelocity()
        {
            if (rigidbody == null || !rigidbody.gameObject.activeInHierarchy || rigidbody.isKinematic)
            {
                return;
            }

            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
        }

        private void AssignInteractionCamera()
        {
            if (interaction != null && viewCamera != null)
            {
                interaction.SetInteractionCamera(viewCamera);
            }
        }
    }

    [Header("Input")]
    [SerializeField] private KeyCode switchKey = KeyCode.R;

    [Header("View Rigs")]
    [SerializeField] private ViewMode startingMode = ViewMode.Human;
    [SerializeField] private ViewRig human = new ViewRig();
    [SerializeField] private ViewRig companion = new ViewRig();

    [Header("Fade")]
    [SerializeField] private bool createFadeOverlay = true;
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private Image fadeImage;
    [SerializeField] private Color fadeColor = Color.black;
    [SerializeField] private float fadeOutDuration = 0.35f;
    [SerializeField] private float blackHoldDuration = 0.1f;
    [SerializeField] private float fadeInDuration = 0.35f;
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Runtime State")]
    [SerializeField] private ViewMode currentMode;

    private readonly HashSet<Object> manualSwitchBlockers = new HashSet<Object>();
    private Coroutine switchRoutine;

    public bool IsSwitching { get; private set; }

    public ViewMode CurrentMode
    {
        get { return currentMode; }
    }

    public Camera CurrentViewCamera
    {
        get
        {
            ResolveMissingReferences();
            return currentMode == ViewMode.Human ? human.viewCamera : companion.viewCamera;
        }
    }

    public PlayerInteraction CurrentInteraction
    {
        get
        {
            ResolveMissingReferences();
            return currentMode == ViewMode.Human ? human.interaction : companion.interaction;
        }
    }

    public GameObject CurrentViewRoot
    {
        get
        {
            return currentMode == ViewMode.Human ? human.rootObject : companion.rootObject;
        }
    }

    public static ViewSwitchController FindPreferredController()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        ViewSwitchController preferred = FindPreferredController(activeScene);
        return preferred != null ? preferred : FindPreferredControllerInternal(default(Scene), false);
    }

    public static ViewSwitchController FindPreferredController(Scene scene)
    {
        return FindPreferredControllerInternal(scene, scene.IsValid());
    }

    public bool IsManualSwitchBlocked
    {
        get
        {
            RemoveDestroyedManualSwitchBlockers();
            return manualSwitchBlockers.Count > 0 || HearthPlayerControlLock.AnyControlsLocked;
        }
    }

    public void SetManualSwitchBlocked(Object owner, bool blocked)
    {
        Object effectiveOwner = owner != null ? owner : this;
        if (blocked)
        {
            manualSwitchBlockers.Add(effectiveOwner);
        }
        else
        {
            manualSwitchBlockers.Remove(effectiveOwner);
        }
    }

    /// <summary>
    /// Reconciles the active view rig with the central owner-based control
    /// masks. This is safe to call after any dialogue or flow lock changes and
    /// prevents script execution order from restoring a stale disabled state.
    /// </summary>
    public void RefreshControlsFromLockState()
    {
        ResolveMissingReferences();
        if (IsSwitching)
        {
            SetAllControlsActive(false);
            return;
        }

        HearthPlayerControlMask lockedMask =
            HearthPlayerControlLock.CombinedActiveMask;
        bool humanActive = currentMode == ViewMode.Human;
        human.SetControlsActive(humanActive, lockedMask);
        companion.SetControlsActive(!humanActive, lockedMask);
        UpdateCameraTags(humanActive);
    }

    private static ViewSwitchController FindPreferredControllerInternal(Scene scene, bool restrictToScene)
    {
        ViewSwitchController[] controllers = Object.FindObjectsOfType<ViewSwitchController>(true);
        ViewSwitchController best = null;
        int bestScore = int.MinValue;

        for (int i = 0; i < controllers.Length; i++)
        {
            ViewSwitchController candidate = controllers[i];
            if (candidate == null || !candidate.gameObject.scene.IsValid() || !candidate.gameObject.scene.isLoaded)
            {
                continue;
            }

            if (restrictToScene && candidate.gameObject.scene != scene)
            {
                continue;
            }

            int score = 0;
            if (candidate.enabled)
            {
                score += 1000;
            }

            if (candidate.gameObject.activeInHierarchy)
            {
                score += 200;
            }

            string path = GetHierarchyPath(candidate.transform);
            if (string.Equals(
                path,
                "MIN_LOOP_ROOT/FlowManagers/ViewSwitchController",
                System.StringComparison.Ordinal))
            {
                score += 10000;
            }
            else if (path.Contains("MIN_LOOP_ROOT/FlowManagers"))
            {
                score += 2000;
            }
            else if (path.Contains("MIN_LOOP_ROOT"))
            {
                score += 1000;
            }

            if (candidate.gameObject.name == "ViewSwitchController")
            {
                score += 100;
            }

            if (score > bestScore)
            {
                best = candidate;
                bestScore = score;
            }
        }

        return best;
    }

    private void Awake()
    {
        if (FindPreferredController(gameObject.scene) != this)
        {
            return;
        }

        ResolveMissingReferences();
        EnsureFadeOverlay();
        ApplyMode(startingMode, true);
        SetFadeAlpha(0f, false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(switchKey) && !IsManualSwitchBlocked)
        {
            ToggleView();
        }
    }

    private void OnValidate()
    {
        fadeOutDuration = Mathf.Max(0f, fadeOutDuration);
        blackHoldDuration = Mathf.Max(0f, blackHoldDuration);
        fadeInDuration = Mathf.Max(0f, fadeInDuration);
    }

    public void ToggleView()
    {
        if (currentMode == ViewMode.Human)
        {
            SwitchToCompanion();
        }
        else
        {
            SwitchToHuman();
        }
    }

    public void SwitchToHuman()
    {
        SwitchTo(ViewMode.Human);
    }

    public void SwitchToCompanion()
    {
        SwitchTo(ViewMode.Companion);
    }

    public void SwitchTo(ViewMode targetMode)
    {
        ViewSwitchController preferred = FindPreferredController(gameObject.scene);
        if (preferred != null && preferred != this)
        {
            preferred.SwitchTo(targetMode);
            return;
        }

        if (IsSwitching || targetMode == currentMode)
        {
            return;
        }

        if (switchRoutine != null)
        {
            StopCoroutine(switchRoutine);
        }

        switchRoutine = StartCoroutine(SwitchRoutine(targetMode));
    }

    private IEnumerator SwitchRoutine(ViewMode targetMode)
    {
        IsSwitching = true;

        SetAllControlsActive(false);
        yield return FadeTo(1f, fadeOutDuration);

        if (blackHoldDuration > 0f)
        {
            yield return WaitForDuration(blackHoldDuration);
        }

        ApplyMode(targetMode, false);
        yield return FadeTo(0f, fadeInDuration);

        RefreshControlsFromLockState();
        LockCursorForGameplay();

        IsSwitching = false;
        switchRoutine = null;
    }

    private void ResolveMissingReferences()
    {
        human.ResolveMissingReferences();
        companion.ResolveMissingReferences();
    }

    private void ApplyMode(ViewMode mode, bool controlsActive)
    {
        currentMode = mode;

        bool humanActive = mode == ViewMode.Human;
        human.SetVisualsActive(humanActive);
        companion.SetVisualsActive(!humanActive);
        UpdateCameraTags(humanActive);

        if (controlsActive)
        {
            RefreshControlsFromLockState();
        }
        else
        {
            human.SetControlsActive(false);
            companion.SetControlsActive(false);
        }

        LockCursorForGameplay();
    }

    private void SetAllControlsActive(bool isActive)
    {
        human.SetControlsActive(isActive && currentMode == ViewMode.Human);
        companion.SetControlsActive(isActive && currentMode == ViewMode.Companion);
    }

    private void UpdateCameraTags(bool humanActive)
    {
        SetMainCameraTag(human.viewCamera, humanActive);
        SetMainCameraTag(companion.viewCamera, !humanActive);
    }

    private static string GetHierarchyPath(Transform target)
    {
        if (target == null)
        {
            return string.Empty;
        }

        string path = target.name;
        Transform parent = target.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }

        return path;
    }

    private void RemoveDestroyedManualSwitchBlockers()
    {
        if (manualSwitchBlockers.Count == 0)
        {
            return;
        }

        List<Object> destroyed = null;
        foreach (Object owner in manualSwitchBlockers)
        {
            if (owner != null)
            {
                continue;
            }

            if (destroyed == null)
            {
                destroyed = new List<Object>();
            }

            destroyed.Add(owner);
        }

        if (destroyed == null)
        {
            return;
        }

        for (int i = 0; i < destroyed.Count; i++)
        {
            manualSwitchBlockers.Remove(destroyed[i]);
        }
    }

    private static void SetMainCameraTag(Camera camera, bool isMain)
    {
        if (camera == null)
        {
            return;
        }

        camera.tag = isMain ? "MainCamera" : "Untagged";
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (fadeCanvasGroup == null)
        {
            yield break;
        }

        SetFadeBlocking(true);

        if (duration <= 0f)
        {
            SetFadeAlpha(targetAlpha, targetAlpha > 0.001f);
            yield break;
        }

        float startAlpha = fadeCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetFadeAlpha(Mathf.Lerp(startAlpha, targetAlpha, t), true);
            yield return null;
        }

        SetFadeAlpha(targetAlpha, targetAlpha > 0.001f);
    }

    private IEnumerator WaitForDuration(float seconds)
    {
        if (useUnscaledTime)
        {
            yield return new WaitForSecondsRealtime(seconds);
        }
        else
        {
            yield return new WaitForSeconds(seconds);
        }
    }

    private void EnsureFadeOverlay()
    {
        if (fadeCanvasGroup == null && fadeImage != null)
        {
            fadeCanvasGroup = fadeImage.GetComponentInParent<CanvasGroup>();
            if (fadeCanvasGroup == null)
            {
                fadeCanvasGroup = fadeImage.gameObject.AddComponent<CanvasGroup>();
            }
        }

        if (fadeCanvasGroup == null && createFadeOverlay)
        {
            CreateFadeOverlay();
        }

        if (fadeImage != null)
        {
            fadeImage.color = fadeColor;
        }
    }

    private void CreateFadeOverlay()
    {
        GameObject canvasObject = new GameObject("View Switch Fade Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        CanvasScaler canvasScaler = canvasObject.GetComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920f, 1080f);

        fadeCanvasGroup = canvasObject.GetComponent<CanvasGroup>();

        GameObject imageObject = new GameObject("Fade Image", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(canvasObject.transform, false);

        RectTransform imageRect = imageObject.GetComponent<RectTransform>();
        imageRect.anchorMin = Vector2.zero;
        imageRect.anchorMax = Vector2.one;
        imageRect.offsetMin = Vector2.zero;
        imageRect.offsetMax = Vector2.zero;

        fadeImage = imageObject.GetComponent<Image>();
        fadeImage.color = fadeColor;
    }

    private void SetFadeAlpha(float alpha, bool blocksRaycasts)
    {
        if (fadeCanvasGroup == null)
        {
            return;
        }

        fadeCanvasGroup.alpha = alpha;
        SetFadeBlocking(blocksRaycasts);
    }

    private void SetFadeBlocking(bool blocksRaycasts)
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = blocksRaycasts;
            fadeCanvasGroup.interactable = blocksRaycasts;
        }

        if (fadeImage != null)
        {
            fadeImage.raycastTarget = blocksRaycasts;
        }
    }

    private void LockCursorForGameplay()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
