using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class InteractionFeedbackController : MonoBehaviour, IInteractable, IInteractionAvailability
{
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    [Header("Interaction")]
    [SerializeField] private bool canInteract = true;
    [SerializeField] private string interactionDescription = "E  INTERACT";
    [SerializeField] private bool playFeedbackOnInteract = true;
    [SerializeField] private bool oneShot;
    [SerializeField] private float cooldown = 0.25f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip feedbackClip;
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;
    [SerializeField] private Vector2 pitchRange = Vector2.one;

    [Header("Light Pulse")]
    [SerializeField] private Light[] targetLights;
    [SerializeField] private Color lightColor = new Color(0.35f, 0.85f, 1f, 1f);
    [SerializeField] private float lightIntensity = 2.5f;

    [Header("Renderer Pulse")]
    [SerializeField] private Renderer[] targetRenderers;
    [SerializeField] private Color pulseColor = new Color(0.2f, 0.85f, 1f, 1f);
    [SerializeField] private float emissionIntensity = 1.8f;
    [SerializeField] private bool setBaseColor;

    [Header("Timing")]
    [SerializeField] private float pulseDuration = 0.4f;
    [SerializeField] private bool stayLitAfterFeedback;

    [Header("Events")]
    [SerializeField] private UnityEvent feedbackStarted = new UnityEvent();
    [SerializeField] private UnityEvent feedbackCompleted = new UnityEvent();

    private MaterialPropertyBlock propertyBlock;
    private Coroutine feedbackRoutine;
    private float nextAllowedTime;
    private bool hasPlayed;
    private LightState[] lightStates;
    private MaterialPropertyBlock[] rendererStates;

    public bool CanInteract
    {
        get { return canInteract; }
    }

    public bool IsInteractionAvailable
    {
        get { return canInteract; }
    }

    private void Awake()
    {
        EnsurePropertyBlock();
        ResolveReferences();
        CacheLightStates();
    }

    private void OnValidate()
    {
        cooldown = Mathf.Max(0f, cooldown);
        pulseDuration = Mathf.Max(0f, pulseDuration);

        if (pitchRange.x <= 0f)
        {
            pitchRange.x = 0.01f;
        }

        if (pitchRange.y <= 0f)
        {
            pitchRange.y = 0.01f;
        }

        if (pitchRange.y < pitchRange.x)
        {
            pitchRange.y = pitchRange.x;
        }
    }

    public void Interact()
    {
        if (!canInteract || !playFeedbackOnInteract)
        {
            return;
        }

        PlayFeedback();
    }

    public string GetDescription()
    {
        return interactionDescription;
    }

    public void PlayFeedback()
    {
        if (!canInteract)
        {
            return;
        }

        if (oneShot && hasPlayed)
        {
            return;
        }

        if (Time.time < nextAllowedTime)
        {
            return;
        }

        ResolveReferences();
        CacheLightStates();
        CacheRendererStates();

        nextAllowedTime = Time.time + cooldown;
        hasPlayed = true;

        if (feedbackRoutine != null)
        {
            StopCoroutine(feedbackRoutine);
        }

        feedbackRoutine = StartCoroutine(FeedbackRoutine());
    }

    public void SetInteractable(bool value)
    {
        canInteract = value;
    }

    public void ResetOneShot()
    {
        hasPlayed = false;
    }

    public void SetStayLit(bool value)
    {
        stayLitAfterFeedback = value;

        if (!stayLitAfterFeedback)
        {
            RestoreVisuals();
        }
    }

    private IEnumerator FeedbackRoutine()
    {
        feedbackStarted.Invoke();
        PlayAudio();
        ApplyVisuals(1f);

        if (pulseDuration > 0f)
        {
            yield return new WaitForSeconds(pulseDuration);
        }

        if (!stayLitAfterFeedback)
        {
            RestoreVisuals();
        }

        feedbackRoutine = null;
        feedbackCompleted.Invoke();
    }

    private void PlayAudio()
    {
        if (audioSource == null || feedbackClip == null)
        {
            return;
        }

        audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        audioSource.PlayOneShot(feedbackClip, volume);
    }

    private void ApplyVisuals(float multiplier)
    {
        EnsurePropertyBlock();

        Color finalLightColor = lightColor;
        Color finalEmission = pulseColor * emissionIntensity * Mathf.Max(0f, multiplier);

        for (int i = 0; i < targetLights.Length; i++)
        {
            Light targetLight = targetLights[i];
            if (targetLight == null)
            {
                continue;
            }

            targetLight.enabled = true;
            targetLight.color = finalLightColor;
            targetLight.intensity = lightIntensity * Mathf.Max(0f, multiplier);
        }

        for (int i = 0; i < targetRenderers.Length; i++)
        {
            Renderer targetRenderer = targetRenderers[i];
            if (targetRenderer == null)
            {
                continue;
            }

            propertyBlock.Clear();
            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(EmissionColorId, finalEmission);

            if (setBaseColor)
            {
                propertyBlock.SetColor(BaseColorId, pulseColor);
                propertyBlock.SetColor(ColorId, pulseColor);
            }

            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    private void RestoreVisuals()
    {
        if (targetLights == null)
        {
            targetLights = new Light[0];
        }

        if (targetRenderers == null)
        {
            targetRenderers = new Renderer[0];
        }

        if (lightStates != null)
        {
            for (int i = 0; i < targetLights.Length && i < lightStates.Length; i++)
            {
                Light targetLight = targetLights[i];
                if (targetLight == null)
                {
                    continue;
                }

                targetLight.enabled = lightStates[i].enabled;
                targetLight.color = lightStates[i].color;
                targetLight.intensity = lightStates[i].intensity;
            }
        }

        for (int i = 0; i < targetRenderers.Length; i++)
        {
            Renderer targetRenderer = targetRenderers[i];
            if (targetRenderer == null)
            {
                continue;
            }

            if (rendererStates != null && i < rendererStates.Length && rendererStates[i] != null)
            {
                targetRenderer.SetPropertyBlock(rendererStates[i]);
            }
            else
            {
                targetRenderer.SetPropertyBlock(null);
            }
        }
    }

    private void ResolveReferences()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (targetLights == null || targetLights.Length == 0)
        {
            targetLights = GetComponentsInChildren<Light>(true);
        }

        if (targetRenderers == null || targetRenderers.Length == 0)
        {
            targetRenderers = GetComponentsInChildren<Renderer>(true);
        }
    }

    private void CacheLightStates()
    {
        if (targetLights == null)
        {
            targetLights = new Light[0];
        }

        lightStates = new LightState[targetLights.Length];
        for (int i = 0; i < targetLights.Length; i++)
        {
            Light targetLight = targetLights[i];
            if (targetLight == null)
            {
                continue;
            }

            lightStates[i] = new LightState(targetLight.enabled, targetLight.color, targetLight.intensity);
        }
    }

    private void CacheRendererStates()
    {
        if (targetRenderers == null)
        {
            targetRenderers = new Renderer[0];
        }

        rendererStates = new MaterialPropertyBlock[targetRenderers.Length];
        for (int i = 0; i < targetRenderers.Length; i++)
        {
            Renderer targetRenderer = targetRenderers[i];
            if (targetRenderer == null)
            {
                continue;
            }

            MaterialPropertyBlock block = new MaterialPropertyBlock();
            targetRenderer.GetPropertyBlock(block);
            rendererStates[i] = block;
        }
    }

    private void EnsurePropertyBlock()
    {
        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
    }

    private struct LightState
    {
        public readonly bool enabled;
        public readonly Color color;
        public readonly float intensity;

        public LightState(bool enabled, Color color, float intensity)
        {
            this.enabled = enabled;
            this.color = color;
            this.intensity = intensity;
        }
    }
}
