using System.Collections;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class HearthPhotoFrameInteractable : MonoBehaviour, IInteractable, IInteractionAvailability
{
    [Header("Flow")]
    [SerializeField] private Hearth17F04FinaleController finaleController;
    [SerializeField] private string description = "E  VIEW PHOTO";

    [Header("Cameras")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera photoCamera;
    [SerializeField] private HearthTerminalCameraTransition cameraTransition;

    [Header("Player Controls")]
    [SerializeField] private FirstPersonMovement playerMovement;
    [SerializeField] private FirstPersonLook playerLook;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private Rigidbody playerRigidbody;

    [Header("Exit")]
    [SerializeField] private KeyCode confirmExitKey = KeyCode.Space;
    [SerializeField] private KeyCode cancelExitKey = KeyCode.Escape;
    [SerializeField] private CanvasGroup exitHintGroup;
    [SerializeField] private TMP_Text exitHintText;
    [SerializeField] private string exitHintLabel = "SPACE  RETURN";

    private bool viewOpen;
    private bool dialogueComplete;
    private bool transitioning;
    private bool movementWasEnabled;
    private bool lookWasEnabled;
    private bool interactionWasEnabled;
    private bool playerCameraWasEnabled;
    private bool photoCameraWasEnabled;
    private bool playerAudioWasEnabled;
    private bool photoAudioWasEnabled;

    public bool IsOpen
    {
        get { return viewOpen; }
    }

    public bool IsTransitioning
    {
        get { return transitioning; }
    }

    public bool IsInteractionAvailable
    {
        get { return !viewOpen && !transitioning && finaleController != null && finaleController.CanInspectPhoto; }
    }

    private void Awake()
    {
        ResolveReferences();
        if (photoCamera != null)
        {
            SetCameraState(photoCamera, false, false);
        }
        SetExitHintVisible(false);
    }

    private void Update()
    {
        if (!viewOpen || transitioning || !dialogueComplete)
        {
            return;
        }

        if (Input.GetKeyDown(confirmExitKey) || Input.GetKeyDown(cancelExitKey))
        {
            CloseView();
        }
    }

    public void Interact()
    {
        if (IsInteractionAvailable)
        {
            OpenView();
        }
    }

    public string GetDescription()
    {
        return description;
    }

    public void OpenView()
    {
        if (!IsInteractionAvailable)
        {
            return;
        }

        StartCoroutine(OpenViewRoutine());
    }

    public void CloseView()
    {
        if (!viewOpen || transitioning)
        {
            return;
        }

        StartCoroutine(CloseViewRoutine());
    }

    public void NotifyDialogueComplete()
    {
        dialogueComplete = true;
        SetExitHintVisible(true);
    }

    public void Configure(
        Hearth17F04FinaleController controller,
        Camera humanCamera,
        Camera fixedPhotoCamera,
        HearthTerminalCameraTransition transition,
        FirstPersonMovement movement,
        FirstPersonLook look,
        PlayerInteraction interaction,
        Rigidbody body)
    {
        finaleController = controller;
        playerCamera = humanCamera;
        photoCamera = fixedPhotoCamera;
        cameraTransition = transition;
        playerMovement = movement;
        playerLook = look;
        playerInteraction = interaction;
        playerRigidbody = body;
    }

    public void SetExitHint(CanvasGroup group, TMP_Text text)
    {
        exitHintGroup = group;
        exitHintText = text;
        SetExitHintVisible(false);
    }

    private IEnumerator OpenViewRoutine()
    {
        ResolveReferences();
        if (playerCamera == null || photoCamera == null)
        {
            Debug.LogWarning("[HearthPhotoFrameInteractable] Player or photo camera is missing.", this);
            yield break;
        }

        transitioning = true;
        dialogueComplete = false;
        SetExitHintVisible(false);
        CaptureStates();
        SetControlsEnabled(false);

        if (cameraTransition != null && cameraTransition.CanRunEnterTransition(playerCamera, photoCamera))
        {
            yield return cameraTransition.TransitionToTerminal(playerCamera, photoCamera, null);
        }
        else
        {
            Debug.LogWarning("[HearthPhotoFrameInteractable] Smooth photo transition unavailable; using an immediate camera switch.", this);
            SetCameraState(playerCamera, false, false);
            SetCameraState(photoCamera, true, true);
        }

        viewOpen = true;
        transitioning = false;
        finaleController.BeginPhotoInspection();
    }

    private IEnumerator CloseViewRoutine()
    {
        transitioning = true;

        if (cameraTransition != null && cameraTransition.CanRunExitTransition(playerCamera, photoCamera))
        {
            yield return cameraTransition.TransitionToPlayer(
                playerCamera,
                photoCamera,
                null,
                playerCameraWasEnabled,
                photoCameraWasEnabled,
                playerAudioWasEnabled,
                photoAudioWasEnabled);
        }
        else
        {
            SetCameraState(photoCamera, photoCameraWasEnabled, photoAudioWasEnabled);
            SetCameraState(playerCamera, playerCameraWasEnabled, playerAudioWasEnabled);
        }

        viewOpen = false;
        dialogueComplete = false;
        SetExitHintVisible(false);
        transitioning = false;
        RestoreControls();
        finaleController.CompletePhotoInspection();
    }

    private void ResolveReferences()
    {
        if (playerInteraction == null)
        {
            PlayerInteraction[] interactions = FindObjectsOfType<PlayerInteraction>(true);
            for (int i = 0; i < interactions.Length; i++)
            {
                if (interactions[i] != null && interactions[i].gameObject.name == "Person Controller")
                {
                    playerInteraction = interactions[i];
                    break;
                }
            }
        }

        if (playerInteraction != null)
        {
            if (playerCamera == null) playerCamera = playerInteraction.mainCamera;
            if (playerMovement == null) playerMovement = playerInteraction.GetComponent<FirstPersonMovement>();
            if (playerLook == null) playerLook = playerInteraction.GetComponentInChildren<FirstPersonLook>(true);
            if (playerRigidbody == null) playerRigidbody = playerInteraction.GetComponent<Rigidbody>();
        }

        if (photoCamera == null)
        {
            photoCamera = GetComponentInChildren<Camera>(true);
        }

        if (cameraTransition == null)
        {
            cameraTransition = GetComponent<HearthTerminalCameraTransition>();
        }
    }

    private void CaptureStates()
    {
        movementWasEnabled = playerMovement != null && playerMovement.enabled;
        lookWasEnabled = playerLook != null && playerLook.enabled;
        interactionWasEnabled = playerInteraction != null && playerInteraction.InteractionEnabled;
        playerCameraWasEnabled = playerCamera != null && playerCamera.enabled;
        photoCameraWasEnabled = photoCamera != null && photoCamera.enabled;
        playerAudioWasEnabled = GetAudioState(playerCamera);
        photoAudioWasEnabled = GetAudioState(photoCamera);
    }

    private void SetControlsEnabled(bool enabled)
    {
        if (playerMovement != null) playerMovement.enabled = enabled;
        if (playerLook != null) playerLook.enabled = enabled;
        if (playerInteraction != null) playerInteraction.SetInteractionEnabled(enabled);
        if (!enabled && playerRigidbody != null && !playerRigidbody.isKinematic)
        {
            playerRigidbody.velocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }
    }

    private void RestoreControls()
    {
        if (playerMovement != null) playerMovement.enabled = movementWasEnabled;
        if (playerLook != null) playerLook.enabled = lookWasEnabled;
        if (playerInteraction != null) playerInteraction.SetInteractionEnabled(interactionWasEnabled);
    }

    private static bool GetAudioState(Camera camera)
    {
        AudioListener listener = camera != null ? camera.GetComponent<AudioListener>() : null;
        return listener != null && listener.enabled;
    }

    private static void SetCameraState(Camera camera, bool cameraEnabled, bool audioEnabled)
    {
        if (camera == null)
        {
            return;
        }

        camera.enabled = cameraEnabled;
        AudioListener listener = camera.GetComponent<AudioListener>();
        if (listener != null)
        {
            listener.enabled = audioEnabled;
        }
    }

    private void SetExitHintVisible(bool visible)
    {
        if (exitHintText != null)
        {
            exitHintText.text = exitHintLabel;
        }

        if (exitHintGroup == null)
        {
            return;
        }

        exitHintGroup.alpha = visible ? 1f : 0f;
        exitHintGroup.interactable = false;
        exitHintGroup.blocksRaycasts = false;
        exitHintGroup.gameObject.SetActive(true);
    }
}
