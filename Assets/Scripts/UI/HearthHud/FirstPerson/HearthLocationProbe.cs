using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class HearthLocationProbe : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ViewSwitchController viewSwitchController;
    [SerializeField] private Transform humanProbeRoot;
    [SerializeField] private Transform companionProbeRoot;
    [SerializeField] private HearthLocationHudView hudView;
    [SerializeField] private HearthFirstPersonHudController hudController;

    [Header("Probe")]
    [SerializeField] private bool locationEnabled = true;
    [SerializeField] private LayerMask raycastMask = ~0;
    [SerializeField] private float probeHeight = 2f;
    [SerializeField] private float probeDistance = 8f;
    [SerializeField] private float refreshInterval = 0.1f;
    [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;

    [Header("Home Welcome")]
    [SerializeField] private bool showHomeWelcomeOnce = true;
    [SerializeField] private string homeWelcomeLocationId = "17F-04";
    [SerializeField] private float homeWelcomeAutoCloseSeconds = 2.5f;
    [SerializeField] private bool waitForPersistentHudBeforeWelcome = true;

    private float refreshTimer;
    private HearthLocationSurface currentSurface;
    private bool homeWelcomeShown;
    private bool pendingHomeWelcome;
    private Coroutine autoCloseRoutine;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();

        if (hudController != null)
        {
            hudController.PageShown.AddListener(OnHudPageShown);
        }
    }

    private void OnDisable()
    {
        if (hudController != null)
        {
            hudController.PageShown.RemoveListener(OnHudPageShown);
        }
    }

    private void Update()
    {
        if (!locationEnabled)
        {
            return;
        }

        refreshTimer -= Time.deltaTime;
        if (refreshTimer > 0f)
        {
            return;
        }

        refreshTimer = Mathf.Max(0.02f, refreshInterval);
        RefreshCurrentLocation();
    }

    private void OnValidate()
    {
        probeHeight = Mathf.Max(0f, probeHeight);
        probeDistance = Mathf.Max(0.1f, probeDistance);
        refreshInterval = Mathf.Max(0.02f, refreshInterval);
        homeWelcomeAutoCloseSeconds = Mathf.Max(0f, homeWelcomeAutoCloseSeconds);
    }

    public void SetLocationEnabled(bool enabled)
    {
        locationEnabled = enabled;

        if (!locationEnabled)
        {
            if (hudView != null)
            {
                hudView.HideLocation();
            }

            currentSurface = null;
        }
    }

    public void RefreshCurrentLocation()
    {
        ResolveReferences();

        if (!locationEnabled)
        {
            return;
        }

        Transform probeRoot = GetActiveProbeRoot();
        HearthLocationSurface nextSurface = probeRoot != null ? ProbeSurface(probeRoot.position) : null;

        if (nextSurface == currentSurface)
        {
            if (currentSurface != null && hudView != null)
            {
                hudView.ShowLocation(currentSurface.DisplayLabel);
            }

            return;
        }

        currentSurface = nextSurface;

        if (currentSurface == null)
        {
            if (hudView != null)
            {
                hudView.HideLocation();
            }

            return;
        }

        if (hudView != null)
        {
            hudView.ShowLocation(currentSurface.DisplayLabel);
        }

        TryTriggerHomeWelcome(currentSurface);
    }

    public void ResetHomeWelcomeOnce()
    {
        homeWelcomeShown = false;
        pendingHomeWelcome = false;
    }

    private void ResolveReferences()
    {
        ViewSwitchController preferredViewSwitch =
            ViewSwitchController.FindPreferredController(gameObject.scene);
        if (preferredViewSwitch != null &&
            (viewSwitchController == null ||
             viewSwitchController != preferredViewSwitch ||
             !viewSwitchController.enabled ||
             !viewSwitchController.gameObject.activeInHierarchy))
        {
            viewSwitchController = preferredViewSwitch;
        }

        if (hudController == null)
        {
            hudController = FindObjectOfType<HearthFirstPersonHudController>();
        }

        if (hudView == null)
        {
            hudView = FindObjectOfType<HearthLocationHudView>(true);
        }

        if (humanProbeRoot == null || companionProbeRoot == null)
        {
            FirstPersonMovement[] movements = FindObjectsOfType<FirstPersonMovement>(true);
            for (int i = 0; i < movements.Length; i++)
            {
                if (movements[i] == null)
                {
                    continue;
                }

                string path = GetPath(movements[i].transform);
                if (humanProbeRoot == null && path.Contains("Person Controller"))
                {
                    humanProbeRoot = movements[i].transform;
                }
                else if (companionProbeRoot == null && path.Contains("Robot Controller"))
                {
                    companionProbeRoot = movements[i].transform;
                }
            }
        }
    }

    private Transform GetActiveProbeRoot()
    {
        if (viewSwitchController != null && viewSwitchController.CurrentMode == ViewSwitchController.ViewMode.Companion)
        {
            return companionProbeRoot != null ? companionProbeRoot : humanProbeRoot;
        }

        return humanProbeRoot != null ? humanProbeRoot : companionProbeRoot;
    }

    private HearthLocationSurface ProbeSurface(Vector3 position)
    {
        Vector3 origin = position + Vector3.up * probeHeight;
        RaycastHit[] hits = Physics.RaycastAll(origin, Vector3.down, probeHeight + probeDistance, raycastMask, triggerInteraction);
        HearthLocationSurface bestSurface = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            HearthLocationSurface surface = hits[i].collider != null
                ? hits[i].collider.GetComponentInParent<HearthLocationSurface>()
                : null;

            if (surface == null)
            {
                continue;
            }

            if (bestSurface == null ||
                surface.Priority > bestSurface.Priority ||
                (surface.Priority == bestSurface.Priority && hits[i].distance < bestDistance))
            {
                bestSurface = surface;
                bestDistance = hits[i].distance;
            }
        }

        return bestSurface;
    }

    private void TryTriggerHomeWelcome(HearthLocationSurface surface)
    {
        if (surface == null || !surface.CanTriggerHomeWelcome)
        {
            return;
        }

        if (showHomeWelcomeOnce && homeWelcomeShown)
        {
            return;
        }

        if (!string.Equals(surface.LocationId, homeWelcomeLocationId, System.StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (waitForPersistentHudBeforeWelcome && hudController != null &&
            hudController.CurrentPageId != HearthFirstPersonHudPageId.Slide01PersistentHud &&
            hudController.CurrentPageId != HearthFirstPersonHudPageId.None)
        {
            pendingHomeWelcome = true;
            return;
        }

        ShowHomeWelcome();
    }

    private void OnHudPageShown(HearthFirstPersonHudPageId pageId)
    {
        if (!pendingHomeWelcome)
        {
            return;
        }

        if (pageId == HearthFirstPersonHudPageId.Slide01PersistentHud || pageId == HearthFirstPersonHudPageId.None)
        {
            ShowHomeWelcome();
        }
    }

    private void ShowHomeWelcome()
    {
        if (hudController == null)
        {
            return;
        }

        pendingHomeWelcome = false;
        homeWelcomeShown = true;
        hudController.ShowPage(HearthFirstPersonHudPageId.Slide06HomeWelcome);

        if (autoCloseRoutine != null)
        {
            StopCoroutine(autoCloseRoutine);
        }

        if (homeWelcomeAutoCloseSeconds > 0f)
        {
            autoCloseRoutine = StartCoroutine(AutoCloseHomeWelcomeRoutine());
        }
    }

    private IEnumerator AutoCloseHomeWelcomeRoutine()
    {
        yield return new WaitForSecondsRealtime(homeWelcomeAutoCloseSeconds);
        autoCloseRoutine = null;

        if (hudController != null && hudController.CurrentPageId == HearthFirstPersonHudPageId.Slide06HomeWelcome)
        {
            hudController.HideOverlay();
        }
    }

    private static string GetPath(Transform transform)
    {
        if (transform == null)
        {
            return string.Empty;
        }

        System.Collections.Generic.Stack<string> stack = new System.Collections.Generic.Stack<string>();
        while (transform != null)
        {
            stack.Push(transform.name);
            transform = transform.parent;
        }

        return string.Join("/", stack.ToArray());
    }
}
