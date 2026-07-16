using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
[ExecuteAlways]
public class HearthEditorOnlyReferenceModel : MonoBehaviour
{
    [SerializeField] private bool disableCollidersAlways = true;
    [SerializeField] private bool makeRigidbodiesKinematicAlways = true;
    [SerializeField] private bool hideRenderersInPlay = true;
    [SerializeField] private bool disableAnimatorsInPlay = true;
    [SerializeField] private bool disableLegacyAnimationsInPlay = true;
    [SerializeField] private bool disableAudioSourcesInPlay = true;
    [SerializeField] private bool disableCamerasInPlay = true;
    [SerializeField] private bool disableAudioListenersInPlay = true;
    [SerializeField] private bool disableNavigationInPlay = true;
    [SerializeField] private bool disableMonoBehavioursInPlay = true;

    private void Awake()
    {
        ApplyReferenceState();
    }

    private void OnEnable()
    {
        ApplyReferenceState();
    }

    private void Start()
    {
        ApplyReferenceState();
    }

    private void LateUpdate()
    {
        if (Application.isPlaying)
        {
            ApplyReferenceState();
        }
    }

    private void OnValidate()
    {
        ApplyReferenceState();
    }

    public void ApplyPlayModeState()
    {
        ApplyReferenceState();
    }

    public void ApplyReferenceState()
    {
        if (disableCollidersAlways)
        {
            foreach (Collider collider in GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = false;
            }
        }

        if (makeRigidbodiesKinematicAlways)
        {
            foreach (Rigidbody body in GetComponentsInChildren<Rigidbody>(true))
            {
                if (Application.isPlaying && !body.isKinematic)
                {
                    body.velocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }

                body.isKinematic = true;
                body.detectCollisions = false;
            }
        }

        if (!Application.isPlaying)
        {
            return;
        }

        if (hideRenderersInPlay)
        {
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>(true))
            {
                renderer.enabled = false;
            }
        }

        if (disableAnimatorsInPlay)
        {
            foreach (Animator animator in GetComponentsInChildren<Animator>(true))
            {
                animator.enabled = false;
            }
        }

        if (disableLegacyAnimationsInPlay)
        {
            foreach (Animation animation in GetComponentsInChildren<Animation>(true))
            {
                animation.Stop();
                animation.enabled = false;
            }
        }

        if (disableAudioSourcesInPlay)
        {
            foreach (AudioSource audioSource in GetComponentsInChildren<AudioSource>(true))
            {
                audioSource.enabled = false;
            }
        }

        if (disableCamerasInPlay)
        {
            foreach (Camera camera in GetComponentsInChildren<Camera>(true))
            {
                camera.enabled = false;
            }
        }

        if (disableAudioListenersInPlay)
        {
            foreach (AudioListener listener in GetComponentsInChildren<AudioListener>(true))
            {
                listener.enabled = false;
            }
        }

        if (disableNavigationInPlay)
        {
            foreach (NavMeshAgent agent in GetComponentsInChildren<NavMeshAgent>(true))
            {
                agent.enabled = false;
            }

            foreach (NavMeshObstacle obstacle in GetComponentsInChildren<NavMeshObstacle>(true))
            {
                obstacle.enabled = false;
            }
        }

        if (disableMonoBehavioursInPlay)
        {
            foreach (MonoBehaviour behaviour in GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour != null && behaviour != this)
                {
                    behaviour.enabled = false;
                }
            }
        }
    }
}
