using UnityEngine;

public class ComfortActionInteractable : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [SerializeField] private string interactionDescription = "执行安抚操作";
    [SerializeField] private bool availableOnStart;

    [Header("References")]
    [SerializeField] private ReplaySequenceController sequenceController;
    [SerializeField] private GameObject visualRoot;

    [Header("Fallback Visual")]
    [SerializeField] private bool createFallbackVisualIfMissing = true;
    [SerializeField] private Color fallbackVisualColor = new Color(0.28f, 0.86f, 1f, 1f);
    [SerializeField] private Vector3 fallbackVisualLocalPosition = new Vector3(0f, 0.75f, 0f);
    [SerializeField] private Vector3 fallbackVisualScale = new Vector3(0.22f, 0.22f, 0.22f);
    [SerializeField] private float fallbackLightIntensity = 1.8f;
    [SerializeField] private float fallbackLightRange = 2.2f;

    private Collider[] cachedColliders;
    private Renderer fallbackRenderer;
    private MaterialPropertyBlock fallbackPropertyBlock;
    private bool isAvailable;

    public bool IsAvailable
    {
        get { return isAvailable; }
    }

    private void Awake()
    {
        EnsureFallbackVisual();
        CacheColliders();
        ResolveSequenceController();
    }

    private void Start()
    {
        SetAvailable(availableOnStart);
    }

    private void OnValidate()
    {
        fallbackVisualScale.x = Mathf.Max(0.01f, fallbackVisualScale.x);
        fallbackVisualScale.y = Mathf.Max(0.01f, fallbackVisualScale.y);
        fallbackVisualScale.z = Mathf.Max(0.01f, fallbackVisualScale.z);
        fallbackLightIntensity = Mathf.Max(0f, fallbackLightIntensity);
        fallbackLightRange = Mathf.Max(0f, fallbackLightRange);
        ApplyFallbackVisualColor();
    }

    public void Interact()
    {
        if (!isAvailable)
        {
            return;
        }

        ResolveSequenceController();

        if (sequenceController == null)
        {
            Debug.LogWarning("ComfortActionInteractable has no ReplaySequenceController assigned.", this);
            return;
        }

        sequenceController.PerformComfortAction();
    }

    public string GetDescription()
    {
        return interactionDescription;
    }

    public void SetAvailable(bool value)
    {
        isAvailable = value;
        EnsureFallbackVisual();
        CacheColliders();

        if (visualRoot != null)
        {
            visualRoot.SetActive(value);
        }

        for (int i = 0; i < cachedColliders.Length; i++)
        {
            if (cachedColliders[i] != null)
            {
                cachedColliders[i].enabled = value;
            }
        }
    }

    public void SetSequenceController(ReplaySequenceController controller)
    {
        sequenceController = controller;
    }

    private void CacheColliders()
    {
        if (cachedColliders == null)
        {
            cachedColliders = GetComponentsInChildren<Collider>(true);
        }
    }

    private void EnsureFallbackVisual()
    {
        if (visualRoot != null || !createFallbackVisualIfMissing)
        {
            return;
        }

        GameObject visualObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visualObject.name = "Generated Comfort Action Visual";
        visualObject.transform.SetParent(transform, false);
        visualObject.transform.localPosition = fallbackVisualLocalPosition;
        visualObject.transform.localRotation = Quaternion.identity;
        visualObject.transform.localScale = fallbackVisualScale;

        Collider visualCollider = visualObject.GetComponent<Collider>();
        if (visualCollider != null)
        {
            if (Application.isPlaying)
            {
                Destroy(visualCollider);
            }
            else
            {
                DestroyImmediate(visualCollider);
            }
        }

        fallbackRenderer = visualObject.GetComponent<Renderer>();
        ApplyFallbackVisualColor();

        GameObject lightObject = new GameObject("Generated Comfort Action Light", typeof(Light));
        lightObject.transform.SetParent(visualObject.transform, false);
        lightObject.transform.localPosition = Vector3.zero;

        Light pointLight = lightObject.GetComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.color = fallbackVisualColor;
        pointLight.intensity = fallbackLightIntensity;
        pointLight.range = fallbackLightRange;
        pointLight.shadows = LightShadows.None;

        visualRoot = visualObject;
        visualRoot.SetActive(isAvailable);
    }

    private void ApplyFallbackVisualColor()
    {
        if (fallbackRenderer == null)
        {
            return;
        }

        if (fallbackPropertyBlock == null)
        {
            fallbackPropertyBlock = new MaterialPropertyBlock();
        }

        fallbackRenderer.GetPropertyBlock(fallbackPropertyBlock);
        fallbackPropertyBlock.SetColor("_Color", fallbackVisualColor);
        fallbackPropertyBlock.SetColor("_BaseColor", fallbackVisualColor);
        fallbackRenderer.SetPropertyBlock(fallbackPropertyBlock);
    }

    private void ResolveSequenceController()
    {
        if (sequenceController == null)
        {
            sequenceController = FindObjectOfType<ReplaySequenceController>();
        }
    }
}
