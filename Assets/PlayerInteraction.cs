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

    [Header("Single-Press Prompt Format")]
    [SerializeField] private bool englishPromptsOnly = true;
    [SerializeField] private bool normalizeSinglePressPrompts = true;
    [SerializeField] private string interactionKeyLabel = "E";

    private readonly RaycastHit[] hitBuffer = new RaycastHit[8];
    private Collider currentCollider;
    private string currentDescription;
    private bool promptVisible;
    private float nextDescriptionRefreshTime;

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
        SetInteractionPrompt(false, null, true);
    }

    private void Start()
    {
        ResolveCamera();
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

    private void UpdateCurrentTarget()
    {
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

        if (!Input.GetKeyDown(interactKey))
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

        description = FormatPrompt(description);

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

    public void SetSinglePressPromptPolicy(bool englishOnly, bool normalize, string keyLabel)
    {
        englishPromptsOnly = englishOnly;
        normalizeSinglePressPrompts = normalize;
        interactionKeyLabel = string.IsNullOrWhiteSpace(keyLabel) ? "E" : keyLabel.Trim();
        ForceRefreshPrompt();
    }

    private string FormatPrompt(string description)
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

        return interactionKeyLabel.Trim().ToUpperInvariant() + "  " + action.Trim().ToUpperInvariant();
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
