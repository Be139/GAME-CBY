using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[DisallowMultipleComponent]
public class CityDuskLightingPresetController : MonoBehaviour
{
    [Header("Original Scene")]
    [SerializeField] private Light originalDirectionalLight;
    [SerializeField] private Volume originalGlobalVolume;

    [Header("Dusk Objects")]
    [SerializeField] private GameObject duskContentRoot;
    [SerializeField] private GameObject roomSampleRoot;
    [SerializeField] private Material duskSkybox;

    [Header("Dusk Ambient")]
    [SerializeField] private Color ambientSkyColor = new Color(0.12f, 0.16f, 0.24f, 1f);
    [SerializeField] private Color ambientEquatorColor = new Color(0.16f, 0.12f, 0.18f, 1f);
    [SerializeField] private Color ambientGroundColor = new Color(0.055f, 0.04f, 0.06f, 1f);
    [SerializeField, Min(0f)] private float ambientIntensity = 0.7f;

    [Header("Dusk Fog")]
    [SerializeField] private bool fogEnabled = true;
    [SerializeField] private FogMode fogMode = FogMode.ExponentialSquared;
    [SerializeField] private Color fogColor = new Color(0.11f, 0.13f, 0.2f, 1f);
    [SerializeField, Min(0f)] private float fogDensity = 0.0018f;
    [SerializeField, Min(0f)] private float fogStartDistance = 80f;
    [SerializeField, Min(0f)] private float fogEndDistance = 650f;

    [Header("Activation")]
    [SerializeField] private bool applyOnEnable = true;
    [SerializeField] private bool restoreOnDisable = true;
    [SerializeField, HideInInspector] private bool isDuskApplied;

    [Header("Captured Original State")]
    [SerializeField, HideInInspector] private bool originalStateCaptured;
    [SerializeField, HideInInspector] private bool originalDirectionalEnabled;
    [SerializeField, HideInInspector] private bool originalVolumeEnabled;
    [SerializeField, HideInInspector] private Material originalSkybox;
    [SerializeField, HideInInspector] private AmbientMode originalAmbientMode;
    [SerializeField, HideInInspector] private Color originalAmbientSkyColor;
    [SerializeField, HideInInspector] private Color originalAmbientEquatorColor;
    [SerializeField, HideInInspector] private Color originalAmbientGroundColor;
    [SerializeField, HideInInspector] private float originalAmbientIntensity;
    [SerializeField, HideInInspector] private bool originalFogEnabled;
    [SerializeField, HideInInspector] private FogMode originalFogMode;
    [SerializeField, HideInInspector] private Color originalFogColor;
    [SerializeField, HideInInspector] private float originalFogDensity;
    [SerializeField, HideInInspector] private float originalFogStartDistance;
    [SerializeField, HideInInspector] private float originalFogEndDistance;

    public bool IsDuskApplied
    {
        get { return isDuskApplied; }
    }

    public Light OriginalDirectionalLight
    {
        get { return originalDirectionalLight; }
    }

    public Volume OriginalGlobalVolume
    {
        get { return originalGlobalVolume; }
    }

    private void OnEnable()
    {
        if (applyOnEnable)
        {
            ApplyDusk();
        }
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && (EditorApplication.isCompiling || EditorApplication.isUpdating))
        {
            return;
        }
#endif

        if (restoreOnDisable)
        {
            RestoreOriginal();
        }
    }

    private void OnValidate()
    {
        ambientIntensity = Mathf.Max(0f, ambientIntensity);
        fogDensity = Mathf.Max(0f, fogDensity);
        fogStartDistance = Mathf.Max(0f, fogStartDistance);
        fogEndDistance = Mathf.Max(fogStartDistance, fogEndDistance);
    }

    public void Configure(
        Light originalLight,
        Volume originalVolume,
        GameObject contentRoot,
        GameObject sampleRoot,
        Material skybox)
    {
        originalDirectionalLight = originalLight;
        originalGlobalVolume = originalVolume;
        duskContentRoot = contentRoot;
        roomSampleRoot = sampleRoot;
        duskSkybox = skybox;
        CaptureOriginalState(true);
    }

    public void ApplyDusk()
    {
        CaptureOriginalState(false);

        if (originalDirectionalLight != null)
        {
            originalDirectionalLight.enabled = false;
        }

        if (originalGlobalVolume != null)
        {
            originalGlobalVolume.enabled = false;
        }

        if (duskContentRoot != null)
        {
            duskContentRoot.SetActive(true);
        }

        if (roomSampleRoot != null)
        {
            roomSampleRoot.SetActive(true);
        }

        if (duskSkybox != null)
        {
            RenderSettings.skybox = duskSkybox;
        }

        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = ambientSkyColor;
        RenderSettings.ambientEquatorColor = ambientEquatorColor;
        RenderSettings.ambientGroundColor = ambientGroundColor;
        RenderSettings.ambientIntensity = ambientIntensity;
        RenderSettings.fog = fogEnabled;
        RenderSettings.fogMode = fogMode;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.fogStartDistance = fogStartDistance;
        RenderSettings.fogEndDistance = fogEndDistance;
        DynamicGI.UpdateEnvironment();
        isDuskApplied = true;
    }

    public void RestoreOriginal()
    {
        if (!originalStateCaptured)
        {
            return;
        }

        if (originalDirectionalLight != null)
        {
            originalDirectionalLight.enabled = originalDirectionalEnabled;
        }

        if (originalGlobalVolume != null)
        {
            originalGlobalVolume.enabled = originalVolumeEnabled;
        }

        if (duskContentRoot != null)
        {
            duskContentRoot.SetActive(false);
        }

        if (roomSampleRoot != null)
        {
            roomSampleRoot.SetActive(false);
        }

        RenderSettings.skybox = originalSkybox;
        RenderSettings.ambientMode = originalAmbientMode;
        RenderSettings.ambientSkyColor = originalAmbientSkyColor;
        RenderSettings.ambientEquatorColor = originalAmbientEquatorColor;
        RenderSettings.ambientGroundColor = originalAmbientGroundColor;
        RenderSettings.ambientIntensity = originalAmbientIntensity;
        RenderSettings.fog = originalFogEnabled;
        RenderSettings.fogMode = originalFogMode;
        RenderSettings.fogColor = originalFogColor;
        RenderSettings.fogDensity = originalFogDensity;
        RenderSettings.fogStartDistance = originalFogStartDistance;
        RenderSettings.fogEndDistance = originalFogEndDistance;
        DynamicGI.UpdateEnvironment();
        isDuskApplied = false;
    }

    private void CaptureOriginalState(bool force)
    {
        if (originalStateCaptured && !force)
        {
            return;
        }

        originalDirectionalEnabled = originalDirectionalLight == null || originalDirectionalLight.enabled;
        originalVolumeEnabled = originalGlobalVolume == null || originalGlobalVolume.enabled;
        originalSkybox = RenderSettings.skybox;
        originalAmbientMode = RenderSettings.ambientMode;
        originalAmbientSkyColor = RenderSettings.ambientSkyColor;
        originalAmbientEquatorColor = RenderSettings.ambientEquatorColor;
        originalAmbientGroundColor = RenderSettings.ambientGroundColor;
        originalAmbientIntensity = RenderSettings.ambientIntensity;
        originalFogEnabled = RenderSettings.fog;
        originalFogMode = RenderSettings.fogMode;
        originalFogColor = RenderSettings.fogColor;
        originalFogDensity = RenderSettings.fogDensity;
        originalFogStartDistance = RenderSettings.fogStartDistance;
        originalFogEndDistance = RenderSettings.fogEndDistance;
        originalStateCaptured = true;
    }
}
