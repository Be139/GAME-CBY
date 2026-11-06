using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CityPeople
{
    public class CityPeople : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Autoplay random animation clips")] 
        private bool AutoPlayAnimations = true;
        [SerializeField]
        [Tooltip("Overrides palette materials, skips other objects")]
        private Material PaletteOverride;
        public string CurrentPaletteName { get; private set; }

        private AnimationClip[] myClips;
        private Animator animator;
        public const string people_pal_prefix = "people_pal";
        private List<Renderer> _paletteMeshes;

        private void Awake()
        {
            var AllRenderers = gameObject.GetComponentsInChildren<Renderer>();
            _paletteMeshes = new List<Renderer>();
            foreach (Renderer r in AllRenderers)
            {
                var material = r.sharedMaterial;
                if (material != null && material.name.StartsWith(people_pal_prefix, StringComparison.Ordinal))
                {
                    _paletteMeshes.Add(r);
                }
            }
            if (_paletteMeshes.Count > 0)
            {
                CurrentPaletteName = _paletteMeshes[0].sharedMaterial.name;
            }

            if (PaletteOverride != null)
            {
                SetPalette(PaletteOverride);
            }
        }

        void Start()
        {
            animator = GetComponent<Animator>();
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                myClips = animator.runtimeAnimatorController.animationClips;
                if (AutoPlayAnimations)
                {
                    PlayAnyClip();
                    StartCoroutine(ShuffleClips());
                }
            }

            if (AutoPlayAnimations)
            {
                //collider for detect clicks near the character
                CapsuleCollider collider =  gameObject.AddComponent<CapsuleCollider>();
                //average character dimentions
                collider.center = new Vector3(0f, 0.8f, 0f);
                collider.radius = 0.3f;
                collider.height = 1.77f;
                collider.direction = 1;
            }

        }

        private void OnEnable()
        {
            // Start owns the initial playback; subsequent enables resume the timer.
            if (AutoPlayAnimations && animator != null && myClips != null)
            {
                StartCoroutine(ShuffleClips());
            }
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        public void SetPalette(Material mat)
        {
            if (mat != null)
            {
                if (mat.name.StartsWith(people_pal_prefix, StringComparison.Ordinal))
                {
                    CurrentPaletteName = mat.name;
                    foreach (Renderer r in _paletteMeshes)
                    {
                        r.material = mat;
                    }
                } else
                {
                    Debug.Log("Material name should start with 'people_pal...' by convention.");
                } 
            }
        }

        public void PlayAnyClip()
        {
            if (animator != null && myClips != null && myClips.Length > 0)
            {
                var cl = myClips[Random.Range(0, myClips.Length)];
                animator.CrossFadeInFixedTime(cl.name, 1.0f, -1, Random.value * cl.length);
            }
            else Debug.LogWarning("Missing animations clips.");
        }

        IEnumerator ShuffleClips()
        {
            while (true)
            {
                yield return new WaitForSeconds(15.0f + Random.value * 5.0f);
                PlayAnyClip();
            }
        }

    }
}
