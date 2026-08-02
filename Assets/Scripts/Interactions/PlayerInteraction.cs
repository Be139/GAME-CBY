using TMPro;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Raycast")]
    public Camera mainCamera;
    public float interactionRange = 3.0f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private LayerMask interactionLayers = ~0;
    [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Collide;
    [SerializeField] private bool searchParentForInteractable = true;
    [SerializeField] private bool findCameraIfMissing = true;

    [Header("Prompt UI")]
    public GameObject uiInteraction;
    public TMP_Text uiInteractionText;
    [SerializeField] private string fallbackDescription = "E  INTERACT";
    [SerializeField] private bool refreshPromptEveryFrame;
    [SerializeField] private float descriptionRefreshInterval = 0.1f;
    [SerializeField] private bool autoResolvePromptIfMissing = true;
    [SerializeField] private float promptResolveRetryInterval = 0.5f;

    [Header("Single-Press Prompt Format")]
    [SerializeField] private bool englishPromptsOnly = true;
    [SerializeField] private bool normalizeSinglePressPrompts = true;
    [SerializeField] private string interactionKeyLabel = "E";

    private readonly RaycastHit[] hitBuffer = new RaycastHit[8];
    private Collider currentCollider;
    private string currentDescription;
    private bool promptVisible;
    private float nextDescriptionRefreshTime;
    private float nextPromptResolveTime;
    private UnityEngine.Object proximityInteractionOwner;
    private IInteractable proximityInteractable;

    public bool InteractionEnabled { get; private set; } = true;

    public IInteractable CurrentInteractable { get; private set; }

    public bool HasCurrentTarget
    {
        get { return CurrentInteractable != null; }
    }

    public KeyCode InteractKey
    {
        get { return interactKey; }
    }

    private void Awake()
    {
        ResolveCamera();
        ResolvePromptUiNow();
        SetInteractionPrompt(false, null, true);
    }

    private void Start()
    {
        ResolveCamera();
        ResolvePromptUiNow();
        SetInteractionPrompt(false, null, true);
    }

    private void OnDisable()
    {
        ClearCurrentTarget();
    }

    private void OnValidate()
    {
        interactionRange = Mathf.Max(0f, interactionRange);
        descriptionRefreshInterval = Mathf.Max(0f, descriptionRefreshInterval);
        promptResolveRetryInterval = Mathf.Max(0.1f, promptResolveRetryInterval);

        if (englishPromptsOnly && ContainsNonAscii(fallbackDescription))
        {
            fallbackDescription = "E  INTERACT";
        }

        if (string.IsNullOrWhiteSpace(interactionKeyLabel))
        {
            interactionKeyLabel = "E";
        }
    }

    private void Update()
    {
        TryResolveMissingPromptUi();

        if (!InteractionEnabled)
        {
            ClearCurrentTarget();
            return;
        }

        if (mainCamera == null)
        {
            ResolveCamera();
        }

        if (mainCamera == null)
        {
            ClearCurrentTarget();
            return;
        }

        UpdateCurrentTarget();
        TryInteract();
    }

    public void SetInteractionEnabled(bool enabled)
    {
        InteractionEnabled = enabled;

        if (!enabled)
        {
            ClearCurrentTarget();
            return;
        }

        ResolveCamera();
    }

    public void SetInteractionCamera(Camera camera)
    {
        mainCamera = camera;
        ClearCurrentTarget();
    }

    public void ForceRefreshPrompt()
    {
        RefreshPrompt(true);
    }

    public void SetProximityInteraction(
        UnityEngine.Object owner,
        IInteractable interactable)
    {
        if (owner == null || interactable == null)
        {
            return;
        }

        proximityInteractionOwner = owner;
        proximityInteractable = interactable;
        SetCurrentTarget(interactable, null);
    }

    public void ClearProximityInteraction(UnityEngine.Object owner)
    {
        if (owner == null || proximityInteractionOwner != owner)
        {
            return;
        }

        proximityInteractionOwner = null;
        proximityInteractable = null;
        ClearCurrentTarget();
    }

    public void BindPromptUi(GameObject prompt, TMP_Text label)
    {
        if (uiInteraction != null && uiInteraction != prompt)
        {
            uiInteraction.SetActive(false);
        }

        uiInteraction = prompt;
        uiInteractionText = label != null
            ? label
            : prompt != null ? prompt.GetComponentInChildren<TMP_Text>(true) : null;
        promptVisible = false;
        currentDescription = null;

        if (uiInteraction != null)
        {
            uiInteraction.SetActive(false);
        }

        if (CurrentInteractable != null)
        {
            RefreshPrompt(true);
        }
    }

    public bool ResolvePromptUiNow()
    {
        if (uiInteraction != null && uiInteractionText != null)
        {
            return true;
        }

        if (!autoResolvePromptIfMissing || !IsFormalPlayerController())
        {
            return false;
        }

        bool companion = name == "Robot Controller";
        string hudRootName = companion ? "HearthCompanionHudRoot" : "HearthHudRoot";
        string promptPath = companion
            ? "InteractionLayer/PlayerInteractionPrompt"
            : "InteractionPromptLayer/PlayerInteractionPrompt";

        Canvas[] canvases = FindObjectsOfType<Canvas>(true);
        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas canvas = canvases[i];
            if (canvas == null || canvas.name != hudRootName || !canvas.gameObject.scene.IsValid())
            {
                continue;
            }

            Transform promptTransform = canvas.transform.Find(promptPath);
            if (promptTransform == null)
            {
                promptTransform = FindDescendantByName(canvas.transform, "PlayerInteractionPrompt");
            }

            if (promptTransform == null)
            {
                continue;
            }

            TMP_Text label = promptTransform.GetComponentInChildren<TMP_Text>(true);
            if (label == null)
            {
                continue;
            }

            BindPromptUi(promptTransform.gameObject, label);
            return true;
        }

        return false;
    }

    private void UpdateCurrentTarget()
    {
        if (proximityInteractable != null &&
            IsInteractionAvailable(proximityInteractable))
        {
            SetCurrentTarget(proximityInteractable, null);
            return;
        }

        if (proximityInteractable != null)
        {
            proximityInteractionOwner = null;
            proximityInteractable = null;
        }

        Ray ray = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        int hitCount = Physics.RaycastNonAlloc(ray, hitBuffer, interactionRange, interactionLayers, triggerInteraction);

        if (hitCount <= 0)
        {
            ClearCurrentTarget();
            return;
        }

        IInteractable bestInteractable = null;
        Collider bestCollider = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = hitBuffer[i];
            if (hit.collider == null || hit.distance >= bestDistance)
            {
                continue;
            }

            IInteractable interactable = FindAvailableInteractable(hit.collider);
            if (interactable == null)
            {
                continue;
            }

            bestInteractable = interactable;
            bestCollider = hit.collider;
            bestDistance = hit.distance;
        }

        if (bestInteractable == null)
        {
            ClearCurrentTarget();
            return;
        }

        SetCurrentTarget(bestInteractable, bestCollider);
    }

    private IInteractable FindAvailableInteractable(Collider hitCollider)
    {
        Transform cursor = hitCollider != null ? hitCollider.transform : null;
        while (cursor != null)
        {
            MonoBehaviour[] behaviours = cursor.GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                IInteractable interactable = behaviours[i] as IInteractable;
                if (interactable != null && IsInteractionAvailable(interactable))
                {
                    return interactable;
                }
            }

            if (!searchParentForInteractable)
            {
                break;
            }

            cursor = cursor.parent;
        }

        return null;
    }

    private void SetCurrentTarget(IInteractable interactable, Collider sourceCollider)
    {
        bool targetChanged = !ReferenceEquals(CurrentInteractable, interactable) || currentCollider != sourceCollider;

        CurrentInteractable = interactable;
        currentCollider = sourceCollider;

        if (targetChanged)
        {
            nextDescriptionRefreshTime = 0f;
            RefreshPrompt(true);
            return;
        }

        if (refreshPromptEveryFrame || Time.time >= nextDescriptionRefreshTime)
        {
            RefreshPrompt(false);
        }
    }

    private void TryInteract()
    {
        if (CurrentInteractable == null || !IsInteractionAvailable(CurrentInteractable))
        {
            ClearCurrentTarget();
            return;
        }

        if (!Input.GetKeyDown(GetCurrentInteractionKey()))
        {
            return;
        }

        CurrentInteractable.Interact();
        RefreshPrompt(true);
    }

    private static bool IsInteractionAvailable(IInteractable interactable)
    {
        IInteractionAvailability availability = interactable as IInteractionAvailability;
        return availability == null || availability.IsInteractionAvailable;
    }

    private void RefreshPrompt(bool forceTextUpdate)
    {
        if (CurrentInteractable == null || !IsInteractionAvailable(CurrentInteractable))
        {
            ClearCurrentTarget();
            SetInteractionPrompt(false);
            return;
        }

        string description = CurrentInteractable.GetDescription();
        if (string.IsNullOrEmpty(description))
        {
            description = fallbackDescription;
        }

        description = FormatPrompt(
            description,
            GetCurrentInteractionKeyLabel());

        SetInteractionPrompt(true, description, forceTextUpdate || description != currentDescription);
        nextDescriptionRefreshTime = Time.time + descriptionRefreshInterval;
    }

    private void ClearCurrentTarget()
    {
        CurrentInteractable = null;
        currentCollider = null;
        currentDescription = null;
        nextDescriptionRefreshTime = 0f;
        SetInteractionPrompt(false);
    }

    private void SetInteractionPrompt(bool isActive, string message = null, bool force = false)
    {
        if (uiInteractionText != null && message != null && (force || message != currentDescription))
        {
            uiInteractionText.text = message;
            currentDescription = message;
        }

        if (uiInteraction != null && (force || promptVisible != isActive))
        {
            uiInteraction.SetActive(isActive);
            promptVisible = isActive;
        }
        else if (uiInteraction == null)
        {
            promptVisible = isActive;
        }
    }

    private void ResolveCamera()
    {
        if (!findCameraIfMissing || mainCamera != null)
        {
            return;
        }

        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            mainCamera = GetComponentInChildren<Camera>(true);
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void TryResolveMissingPromptUi()
    {
        if (!autoResolvePromptIfMissing || (uiInteraction != null && uiInteractionText != null))
        {
            return;
        }

        if (Time.unscaledTime < nextPromptResolveTime)
        {
            return;
        }

        nextPromptResolveTime = Time.unscaledTime + promptResolveRetryInterval;
        ResolvePromptUiNow();
    }

    private bool IsFormalPlayerController()
    {
        return transform.parent != null
            && transform.parent.name == "Player"
            && (name == "Person Controller" || name == "Robot Controller");
    }

    private static Transform FindDescendantByName(Transform root, string targetName)
    {
        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.name == targetName)
            {
                return child;
            }

            Transform nested = FindDescendantByName(child, targetName);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    public void SetSinglePressPromptPolicy(bool englishOnly, bool normalize, string keyLabel)
    {
        englishPromptsOnly = englishOnly;
        normalizeSinglePressPrompts = normalize;
        interactionKeyLabel = string.IsNullOrWhiteSpace(keyLabel) ? "E" : keyLabel.Trim();
        ForceRefreshPrompt();
    }

    private KeyCode GetCurrentInteractionKey()
    {
        IInteractionKeyProvider provider =
            CurrentInteractable as IInteractionKeyProvider;
        return provider != null ? provider.InteractionKey : interactKey;
    }

    private string GetCurrentInteractionKeyLabel()
    {
        IInteractionKeyProvider provider =
            CurrentInteractable as IInteractionKeyProvider;
        string targetLabel = provider != null
            ? provider.InteractionKeyLabel
            : null;
        return string.IsNullOrWhiteSpace(targetLabel)
            ? interactionKeyLabel
            : targetLabel.Trim();
    }

    private string FormatPrompt(string description, string keyLabel)
    {
        string value = string.IsNullOrWhiteSpace(description) ? fallbackDescription : description.Trim();
        if (englishPromptsOnly && ContainsNonAscii(value))
        {
            value = fallbackDescription;
        }

        if (!normalizeSinglePressPrompts)
        {
            return value;
        }

        string action = StripExistingKeyPrefix(value);
        if (string.IsNullOrWhiteSpace(action))
        {
            action = "INTERACT";
        }

        return keyLabel.Trim().ToUpperInvariant() + "  " + action.Trim().ToUpperInvariant();
    }

    private static string StripExistingKeyPrefix(string value)
    {
        string trimmed = value.Trim();
        string upper = trimmed.ToUpperInvariant();

        if (upper.StartsWith("PRESS E", System.StringComparison.Ordinal))
        {
            return trimmed.Substring(7).TrimStart(' ', ':', '-', '_');
        }

        if (upper.StartsWith("[E]", System.StringComparison.Ordinal))
        {
            return trimmed.Substring(3).TrimStart(' ', ':', '-', '_');
        }

        if (upper.Length > 1 && upper[0] == 'E' && char.IsWhiteSpace(upper[1]))
        {
            return trimmed.Substring(1).TrimStart(' ', ':', '-', '_');
        }

        return trimmed;
    }

    private static bool ContainsNonAscii(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        for (int i = 0; i < value.Length; i++)
        {
            if (value[i] > 127)
            {
                return true;
            }
        }

        return false;
    }
}
