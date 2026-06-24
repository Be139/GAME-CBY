using TMPro;
using UnityEngine;

public class MinLoopWorldGuideMarker : MonoBehaviour
{
    [Header("Guide Content")]
    [SerializeField] private string markerLabel = "目标";
    [SerializeField] private Color markerColor = new Color(0.22f, 0.86f, 1f, 1f);
    [SerializeField] private Color labelColor = Color.white;

    [Header("References")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private Renderer markerRenderer;
    [SerializeField] private Light markerLight;
    [SerializeField] private Camera targetCamera;

    [Header("Fallback Visual")]
    [SerializeField] private bool createFallbackVisual = true;
    [SerializeField] private Vector3 visualLocalOffset = new Vector3(0f, 1.25f, 0f);
    [SerializeField] private Vector3 markerLocalScale = new Vector3(0.18f, 0.18f, 0.18f);
    [SerializeField] private float labelFontSize = 2.4f;
    [SerializeField] private Vector2 labelRectSize = new Vector2(4.8f, 1.2f);

    [Header("Motion")]
    [SerializeField] private bool faceCamera = true;
    [SerializeField] private bool findCameraIfMissing = true;
    [SerializeField] private bool bob = true;
    [SerializeField] private float bobAmplitude = 0.1f;
    [SerializeField] private float bobFrequency = 1.2f;
    [SerializeField] private bool pulse = true;
    [SerializeField] private float pulseScale = 0.08f;
    [SerializeField] private float pulseFrequency = 1.6f;

    [Header("Distance")]
    [SerializeField] private bool showDistance = true;
    [SerializeField] private float distanceRefreshInterval = 0.12f;

    private MaterialPropertyBlock propertyBlock;
    private Vector3 baseLocalPosition;
    private Vector3 baseLocalScale = Vector3.one;
    private float nextDistanceRefreshTime;
    private int cachedDistanceStep = int.MinValue;
    private bool hasDistance;

    public string MarkerLabel
    {
        get { return markerLabel; }
    }

    private void Awake()
    {
        EnsureVisuals();
        CacheBaseTransform();
        RefreshVisuals();
    }

    private void OnEnable()
    {
        EnsureVisuals();
        CacheBaseTransform();
        RefreshVisuals();
    }

    private void Update()
    {
        if (visualRoot == null)
        {
            return;
        }

        FaceCameraIfNeeded();
        ApplyMotion();
        RefreshDistanceIfNeeded();
    }

    private void OnValidate()
    {
        bobAmplitude = Mathf.Max(0f, bobAmplitude);
        bobFrequency = Mathf.Max(0f, bobFrequency);
        pulseScale = Mathf.Max(0f, pulseScale);
        pulseFrequency = Mathf.Max(0f, pulseFrequency);
        labelFontSize = Mathf.Max(0.1f, labelFontSize);
        distanceRefreshInterval = Mathf.Max(0.02f, distanceRefreshInterval);
    }

    public void SetLabel(string value)
    {
        markerLabel = value;
        RefreshVisuals();
    }

    public void SetMarkerColor(Color value)
    {
        markerColor = value;
        RefreshVisuals();
    }

    public void SetTargetCamera(Camera camera)
    {
        targetCamera = camera;
        cachedDistanceStep = int.MinValue;
        RefreshVisuals();
    }

    public void SetShowDistance(bool value)
    {
        showDistance = value;
        hasDistance = false;
        cachedDistanceStep = int.MinValue;
        RefreshVisuals();
    }

    [ContextMenu("Refresh Guide Visuals")]
    public void RefreshVisuals()
    {
        EnsureVisuals();

        if (labelText != null)
        {
            labelText.color = labelColor;
            labelText.fontSize = labelFontSize;
        }

        RefreshLabelText();
        ApplyRendererColor();

        if (markerLight != null)
        {
            markerLight.color = markerColor;
        }
    }

    private void EnsureVisuals()
    {
        if (visualRoot == null && createFallbackVisual)
        {
            CreateFallbackVisual();
        }

        if (labelText == null && visualRoot != null)
        {
            labelText = visualRoot.GetComponentInChildren<TMP_Text>(true);
        }

        if (markerRenderer == null && visualRoot != null)
        {
            markerRenderer = visualRoot.GetComponentInChildren<Renderer>(true);
        }

        if (markerLight == null && visualRoot != null)
        {
            markerLight = visualRoot.GetComponentInChildren<Light>(true);
        }
    }

    private void CreateFallbackVisual()
    {
        GameObject rootObject = new GameObject("Generated Guide Marker");
        rootObject.transform.SetParent(transform, false);
        rootObject.transform.localPosition = visualLocalOffset;
        visualRoot = rootObject.transform;

        GameObject dotObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        dotObject.name = "Guide Dot";
        dotObject.transform.SetParent(visualRoot, false);
        dotObject.transform.localScale = markerLocalScale;
        markerRenderer = dotObject.GetComponent<Renderer>();

        Collider dotCollider = dotObject.GetComponent<Collider>();
        if (dotCollider != null)
        {
            dotCollider.enabled = false;
        }

        GameObject textObject = new GameObject("Guide Label", typeof(RectTransform));
        textObject.transform.SetParent(visualRoot, false);
        textObject.transform.localPosition = new Vector3(0f, 0.38f, 0f);
        textObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.sizeDelta = labelRectSize;

        TextMeshPro textMesh = textObject.AddComponent<TextMeshPro>();
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.enableWordWrapping = true;
        labelText = textMesh;

        GameObject lightObject = new GameObject("Guide Light", typeof(Light));
        lightObject.transform.SetParent(visualRoot, false);
        lightObject.transform.localPosition = new Vector3(0f, 0.18f, 0f);
        markerLight = lightObject.GetComponent<Light>();
        markerLight.type = LightType.Point;
        markerLight.range = 1.6f;
        markerLight.intensity = 0.75f;
    }

    private void CacheBaseTransform()
    {
        if (visualRoot == null)
        {
            return;
        }

        baseLocalPosition = visualRoot.localPosition;
        baseLocalScale = visualRoot.localScale;
    }

    private void FaceCameraIfNeeded()
    {
        if (!faceCamera)
        {
            return;
        }

        Camera camera = ResolveCamera();
        if (camera == null)
        {
            return;
        }

        Vector3 direction = visualRoot.position - camera.transform.position;
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        visualRoot.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
    }

    private Camera ResolveCamera()
    {
        if (targetCamera != null || !findCameraIfMissing)
        {
            return targetCamera;
        }

        targetCamera = Camera.main;
        if (targetCamera == null)
        {
            targetCamera = FindObjectOfType<Camera>();
        }

        return targetCamera;
    }

    private void ApplyMotion()
    {
        float time = Time.time;
        Vector3 nextPosition = baseLocalPosition;
        Vector3 nextScale = baseLocalScale;

        if (bob && bobFrequency > 0f && bobAmplitude > 0f)
        {
            nextPosition.y += Mathf.Sin(time * bobFrequency * Mathf.PI * 2f) * bobAmplitude;
        }

        if (pulse && pulseFrequency > 0f && pulseScale > 0f)
        {
            float scale = 1f + Mathf.Sin(time * pulseFrequency * Mathf.PI * 2f) * pulseScale;
            nextScale = baseLocalScale * scale;
        }

        visualRoot.localPosition = nextPosition;
        visualRoot.localScale = nextScale;
    }

    private void RefreshDistanceIfNeeded()
    {
        if (!showDistance || labelText == null)
        {
            ClearDistanceIfNeeded();
            return;
        }

        if (Time.time < nextDistanceRefreshTime)
        {
            return;
        }

        nextDistanceRefreshTime = Time.time + distanceRefreshInterval;

        Camera camera = ResolveCamera();
        if (camera == null)
        {
            ClearDistanceIfNeeded();
            return;
        }

        float distance = Vector3.Distance(camera.transform.position, transform.position);
        int nextDistanceStep = Mathf.Max(0, Mathf.RoundToInt(distance * 10f));
        if (hasDistance && nextDistanceStep == cachedDistanceStep)
        {
            return;
        }

        hasDistance = true;
        cachedDistanceStep = nextDistanceStep;
        RefreshLabelText();
    }

    private void ClearDistanceIfNeeded()
    {
        if (!hasDistance && cachedDistanceStep == int.MinValue)
        {
            return;
        }

        hasDistance = false;
        cachedDistanceStep = int.MinValue;
        RefreshLabelText();
    }

    private void RefreshLabelText()
    {
        if (labelText == null)
        {
            return;
        }

        if (showDistance && hasDistance && cachedDistanceStep >= 0)
        {
            float meters = cachedDistanceStep * 0.1f;
            labelText.text = markerLabel + "\n" + meters.ToString("0.0") + " m";
            return;
        }

        labelText.text = markerLabel;
    }

    private void ApplyRendererColor()
    {
        if (markerRenderer == null)
        {
            return;
        }

        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        markerRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_Color", markerColor);
        propertyBlock.SetColor("_BaseColor", markerColor);
        propertyBlock.SetColor("_EmissionColor", markerColor * 1.5f);
        markerRenderer.SetPropertyBlock(propertyBlock);
    }
}
