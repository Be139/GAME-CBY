using UnityEngine;

[DisallowMultipleComponent]
[ExecuteAlways]
public class HearthEditorOnlyReferenceModel : MonoBehaviour
{
    [SerializeField] private bool hideRenderersInPlay = true;
    [SerializeField] private bool disableCollidersInPlay = true;
    [SerializeField] private bool disableAnimatorsInPlay = true;
    [SerializeField] private bool disableAudioSourcesInPlay = true;
    [SerializeField] private bool makeRigidbodiesKinematicInPlay = true;

    private void Awake()
    {
        ApplyPlayModeState();
    }

    private void OnEnable()
    {
        ApplyPlayModeState();
    }

    private void Start()
    {
        ApplyPlayModeState();
    }

    public void ApplyPlayModeState()
    {
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

        if (disableCollidersInPlay)
        {
            foreach (Collider collider in GetComponentsInChildren<Collider>(true))
            {
                collider.enabled = false;
            }
        }

        if (disableAnimatorsInPlay)
        {
            foreach (Animator animator in GetComponentsInChildren<Animator>(true))
            {
                animator.enabled = false;
            }
        }

        if (disableAudioSourcesInPlay)
        {
            foreach (AudioSource audioSource in GetComponentsInChildren<AudioSource>(true))
            {
                audioSource.enabled = false;
            }
        }

        if (makeRigidbodiesKinematicInPlay)
        {
            foreach (Rigidbody body in GetComponentsInChildren<Rigidbody>(true))
            {
                if (!body.isKinematic)
                {
                    body.velocity = Vector3.zero;
                    body.angularVelocity = Vector3.zero;
                }

                body.isKinematic = true;
                body.detectCollisions = false;
            }
        }
    }
}
