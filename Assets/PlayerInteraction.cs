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
    [SerializeField] private string fallbackDescription = "E 交互";
    [SerializeField] private bool refreshPromptEveryFrame;
    [SerializeField] private float descriptionRefreshInterval = 0.1f;

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

            IInteractable interactable = FindInteractable(hit.collider);
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

    private IInteractable FindInteractable(Collider hitCollider)
    {
        IInteractable interactable = hitCollider.GetComponent<IInteractable>();
        if (interactable != null || !searchParentForInteractable)
        {
            return interactable;
        }

        return hitCollider.GetComponentInParent<IInteractable>();
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
        if (CurrentInteractable == null || !Input.GetKeyDown(interactKey))
        {
            return;
        }

        CurrentInteractable.Interact();
        RefreshPrompt(true);
    }

    private void RefreshPrompt(bool forceTextUpdate)
    {
        if (CurrentInteractable == null)
        {
            SetInteractionPrompt(false);
            return;
        }

        string description = CurrentInteractable.GetDescription();
        if (string.IsNullOrEmpty(description))
        {
            description = fallbackDescription;
        }

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
}
