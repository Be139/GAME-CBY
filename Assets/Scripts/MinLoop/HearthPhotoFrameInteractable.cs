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

    [Header("Photo Pages")]
    [SerializeField] private Renderer photoRenderer;
    [SerializeField] private Texture firstPhotoTexture;
    [SerializeField] private Texture secondPhotoTexture;
    [SerializeField] private KeyCode previousPageKey = KeyCode.LeftArrow;
    [SerializeField] private KeyCode nextPageKey = KeyCode.RightArrow;
    [SerializeField] private string navigationHintLabel = "LEFT / RIGHT  SWITCH PHOTO";
    [SerializeField] private string browseAndExitHintLabel = "LEFT / RIGHT  SWITCH PHOTO     SPACE  RETURN";

    [Header("World-Space Photo Archive V2")]
    [SerializeField] private HearthPhotoArchiveWorldView photoArchiveWorldView;

    [Header("Exit")]
    [SerializeField] private KeyCode confirmExitKey = KeyCode.Space;
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
    private bool pageNavigationEnabled;
    private bool exitInputArmed;
    private int currentPageIndex;
    private MaterialPropertyBlock photoPropertyBlock;
    private bool archiveOverlayOpen;

    public bool IsOpen
    {
        get { return viewOpen; }
    }

    public bool IsTransitioning
    {
        get { return transitioning; }
    }

    public bool HasSecondPhoto
    {
        get { return secondPhotoTexture != null; }
    }

    public int CurrentPageIndex
    {
        get { return currentPageIndex; }
    }

    public bool IsInteractionAvailable
    {
        get { return !viewOpen && !transitioning && finaleController != null && finaleController.CanInspectPhoto; }
    }

    private void Awake()
    {
        ResolveReferences();
        ResolvePhotoArchiveWorldView();
        if (photoCamera != null)
        {
            SetCameraState(photoCamera, false, false);
        }
        if (photoArchiveWorldView != null)
        {
            photoArchiveWorldView.Hide();
        }
        SetHint(string.Empty, false);
    }

    private void OnDisable()
    {
        if (photoArchiveWorldView != null)
        {
            photoArchiveWorldView.Hide();
        }
    }

    private void Update()
    {
        if (!viewOpen || transitioning)
        {
            return;
        }

        if (pageNavigationEnabled && HasSecondPhoto)
        {
            if (Input.GetKeyDown(previousPageKey))
            {
                TrySelectPage(currentPageIndex - 1);
                return;
            }

            if (Input.GetKeyDown(nextPageKey))
            {
                TrySelectPage(currentPageIndex + 1);
                return;
            }
        }

        if (!dialogueComplete)
        {
            return;
        }

        if (!exitInputArmed)
        {
            exitInputArmed = !Input.GetKey(confirmExitKey);
            return;
        }

        if (Input.GetKeyDown(confirmExitKey))
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
        pageNavigationEnabled = HasSecondPhoto;
        dialogueComplete = true;
        exitInputArmed = false;
        SetHint(pageNavigationEnabled ? browseAndExitHintLabel : exitHintLabel, true);
    }

    public void NotifyPageReadyForNavigation()
    {
        dialogueComplete = false;
        exitInputArmed = false;
        pageNavigationEnabled = HasSecondPhoto;
        SetHint(pageNavigationEnabled ? navigationHintLabel : exitHintLabel, true);
    }

    public void SetPageNavigationEnabled(bool value)
    {
        pageNavigationEnabled = value && HasSecondPhoto;
        if (viewOpen)
        {
            if (dialogueComplete)
            {
                SetHint(pageNavigationEnabled ? browseAndExitHintLabel : exitHintLabel, true);
            }
            else
            {
                SetHint(pageNavigationEnabled ? navigationHintLabel : string.Empty, pageNavigationEnabled);
            }
        }
    }

    public void ConfigurePhotoPages(Renderer targetRenderer, Texture firstTexture, Texture secondTexture)
    {
        photoRenderer = targetRenderer;
        firstPhotoTexture = firstTexture;
        secondPhotoTexture = secondTexture;
        currentPageIndex = 0;
        ApplyCurrentPhoto();
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
        // Backward-compatible editor hook: TV4 owns the only visible hint.
        if (text != null)
        {
            text.text = string.Empty;
        }
        if (group != null)
        {
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
            group.gameObject.SetActive(false);
        }
    }

    public void ConfigureSecondUiPhotoArchive(
        HearthFirstPersonHudController hud,
        HearthFirstPersonHudInput hudInput,
        bool enabled)
    {
        // Kept only so old scene/prefab bindings deserialize safely. TV4 no
        // longer opens the global Slide07/08 archive or a RenderTexture feed.
    }

    public HearthDialogueSurface ResolveDialogueSurface()
    {
        ResolvePhotoArchiveWorldView();
        return photoArchiveWorldView != null
            ? photoArchiveWorldView.ResolveDialogueSurface()
            : null;
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
        pageNavigationEnabled = false;
        exitInputArmed = false;
        currentPageIndex = 0;
        ApplyCurrentPhoto();
        SetHint(string.Empty, false);
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
        OpenWorldSpacePhotoArchive();
        finaleController.BeginPhotoInspection();
    }

    private IEnumerator CloseViewRoutine()
    {
        transitioning = true;
        PrepareWorldSpacePhotoArchiveExit();

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

        CloseWorldSpacePhotoArchive();
        viewOpen = false;
        dialogueComplete = false;
        pageNavigationEnabled = false;
        SetHint(string.Empty, false);
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

    private void ResolvePhotoArchiveWorldView()
    {
        if (photoArchiveWorldView == null)
        {
            photoArchiveWorldView = GetComponent<HearthPhotoArchiveWorldView>();
        }

        if (photoArchiveWorldView == null && Application.isPlaying)
        {
            photoArchiveWorldView = gameObject.AddComponent<HearthPhotoArchiveWorldView>();
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

    private void TrySelectPage(int requestedIndex)
    {
        int pageCount = HasSecondPhoto ? 2 : 1;
        int nextIndex = Mathf.Clamp(requestedIndex, 0, pageCount - 1);
        if (nextIndex == currentPageIndex || finaleController == null)
        {
            return;
        }

        currentPageIndex = nextIndex;
        ApplyCurrentPhoto();
        ShowWorldSpacePhotoPage();

        if (dialogueComplete)
        {
            pageNavigationEnabled = HasSecondPhoto;
            SetHint(pageNavigationEnabled ? browseAndExitHintLabel : exitHintLabel, true);
            return;
        }

        pageNavigationEnabled = false;
        SetHint(string.Empty, false);
        finaleController.RequestPhotoPage(currentPageIndex);
    }

    private void ApplyCurrentPhoto()
    {
        if (photoRenderer == null)
        {
            return;
        }

        Texture texture = currentPageIndex == 1 && secondPhotoTexture != null
            ? secondPhotoTexture
            : firstPhotoTexture;
        if (texture == null)
        {
            return;
        }

        if (photoPropertyBlock == null)
        {
            photoPropertyBlock = new MaterialPropertyBlock();
        }

        photoRenderer.GetPropertyBlock(photoPropertyBlock);
        photoPropertyBlock.SetTexture("_BaseMap", texture);
        photoPropertyBlock.SetTexture("_MainTex", texture);
        photoRenderer.SetPropertyBlock(photoPropertyBlock);
    }

    private void SetHint(string label, bool visible)
    {
        if (photoArchiveWorldView != null && archiveOverlayOpen)
        {
            photoArchiveWorldView.SetHint(label, visible);
        }
    }

    private void OpenWorldSpacePhotoArchive()
    {
        ResolveReferences();
        ResolvePhotoArchiveWorldView();
        if (photoArchiveWorldView == null || photoCamera == null)
        {
            Debug.LogWarning(
                "[HearthPhotoFrameInteractable] TV4 world-space photo archive view is unavailable.",
                this);
            return;
        }

        archiveOverlayOpen = true;
        photoArchiveWorldView.Show(
            photoCamera,
            photoRenderer,
            currentPageIndex,
            HasSecondPhoto ? 2 : 1);
        SetHint(string.Empty, false);
    }

    private void ShowWorldSpacePhotoPage()
    {
        if (!archiveOverlayOpen || photoArchiveWorldView == null)
        {
            return;
        }

        photoArchiveWorldView.SetPage(
            currentPageIndex,
            HasSecondPhoto ? 2 : 1);

        string label = dialogueComplete
            ? (pageNavigationEnabled ? browseAndExitHintLabel : exitHintLabel)
            : pageNavigationEnabled
                ? navigationHintLabel
                : string.Empty;
        SetHint(label, dialogueComplete || pageNavigationEnabled);
    }

    private void PrepareWorldSpacePhotoArchiveExit()
    {
        if (!archiveOverlayOpen || photoArchiveWorldView == null)
        {
            return;
        }

        photoArchiveWorldView.Hide();
    }

    private void CloseWorldSpacePhotoArchive()
    {
        if (!archiveOverlayOpen)
        {
            return;
        }

        if (photoArchiveWorldView != null)
        {
            photoArchiveWorldView.Hide();
        }
        archiveOverlayOpen = false;

        SetHint(string.Empty, false);
    }

}
