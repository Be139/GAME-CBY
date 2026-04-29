using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public Camera mainCamera;
    public float interactionRange = 3.0f;

    public GameObject uiInteraction;
    public TMP_Text uiInteractionText;

    void Start()
    {
        uiInteraction.SetActive(false);
    }

    void Update()
    {
        InteractionRay();
    }

    private void InteractionRay()
    {
        Ray ray = mainCamera.ViewportPointToRay(Vector3.one / 2f);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, interactionRange))
        {
            IInteractable interactable = hitInfo.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                uiInteractionText.text = interactable.GetDescription();
                uiInteraction.SetActive(true);

                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactable.Interact();
                }
            }
            else
            {
                uiInteraction.SetActive(false);
            }
        }
        else
        {
            uiInteraction.SetActive(false);
        }
    }
}