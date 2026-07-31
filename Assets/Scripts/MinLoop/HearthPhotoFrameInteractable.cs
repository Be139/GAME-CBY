using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Second UI Photo Archive")]
    [SerializeField] private bool useSecondUiPhotoArchive = true;
    [SerializeField] private HearthFirstPersonHudController photoArchiveHud;
    [SerializeField] private HearthFirstPersonHudInput photoArchiveHudInput;
    [SerializeField] private Vector2Int archiveRenderTextureSize =
        new Vector2Int(1280, 720);

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
    private bool pageNavigationEnabled;
    private int currentPageIndex;
    private MaterialPropertyBlock photoPropertyBlock;
    private RenderTexture archiveRenderTexture;
    private RenderTexture previousPhotoTargetTexture;
    private HearthFirstPersonHudPage activeArchivePage;
    private TMP_Text activeArchiveHintText;
    private HearthFirstPersonHudPageId previousHudPage = HearthFirstPersonHudPageId.Slide01PersistentHud;
    private bool archiveOverlayOpen;
    private bool archiveHudInputWasEnabled;

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
        if (photoCamera != null)
        {
            SetCameraState(photoCamera, false, false);
        }
        SetHint(string.Empty, false);
    }

    private void OnDestroy()
    {
        RestorePhotoCameraTarget();
        ReleaseArchiveRenderTexture();
    }

    private void OnDisable()
    {
        if (Application.isPlaying && archiveOverlayOpen)
        {
            CloseSecondUiPhotoArchive();
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
        pageNavigationEnabled = HasSecondPhoto;
        dialogueComplete = true;
        SetHint(pageNavigationEnabled ? browseAndExitHintLabel : exitHintLabel, true);
    }

    public void NotifyPageReadyForNavigation()
    {
        dialogueComplete = false;
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
        exitHintGroup = group;
        exitHintText = text;
        SetHint(string.Empty, false);
    }

    public void ConfigureSecondUiPhotoArchive(
        HearthFirstPersonHudController hud,
        HearthFirstPersonHudInput hudInput,
        bool enabled)
    {
        photoArchiveHud = hud;
        photoArchiveHudInput = hudInput;
        useSecondUiPhotoArchive = enabled;
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
        OpenSecondUiPhotoArchive();
        finaleController.BeginPhotoInspection();
    }

    private IEnumerator CloseViewRoutine()
    {
        transitioning = true;
        PrepareSecondUiPhotoArchiveExit();

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

        CloseSecondUiPhotoArchive();
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

        if (photoArchiveHud == null)
        {
            photoArchiveHud = FindObjectOfType<HearthFirstPersonHudController>(true);
        }

        if (photoArchiveHudInput == null)
        {
            photoArchiveHudInput = FindObjectOfType<HearthFirstPersonHudInput>(true);
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
        ShowSecondUiPhotoPage();

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
        if (archiveOverlayOpen)
        {
            if (activeArchiveHintText != null)
            {
                activeArchiveHintText.text = visible ? label : "PLEASE WAIT";
                activeArchiveHintText.gameObject.SetActive(true);
            }

            if (exitHintGroup != null)
            {
                exitHintGroup.alpha = 0f;
                exitHintGroup.interactable = false;
                exitHintGroup.blocksRaycasts = false;
                exitHintGroup.gameObject.SetActive(true);
            }

            return;
        }

        if (exitHintText != null)
        {
            exitHintText.text = label;
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

    private void OpenSecondUiPhotoArchive()
    {
        ResolveReferences();
        if (!useSecondUiPhotoArchive || photoArchiveHud == null || photoCamera == null)
        {
            return;
        }

        HearthFirstPersonHudPage firstPage = FindPhotoArchivePage(0);
        if (firstPage == null ||
            FindNamedChild(firstPage.transform, "V2_PhotoViewport") == null)
        {
            Debug.LogWarning(
                "[HearthPhotoFrameInteractable] V2 photo archive page or camera viewport is missing; keeping the physical photo camera view.",
                this);
            return;
        }

        previousHudPage = photoArchiveHud.CurrentPageId;
        if (previousHudPage == HearthFirstPersonHudPageId.None ||
            previousHudPage == HearthFirstPersonHudPageId.Slide07Photo2023 ||
            previousHudPage == HearthFirstPersonHudPageId.Slide08Photo2026)
        {
            previousHudPage = HearthFirstPersonHudPageId.Slide01PersistentHud;
        }

        if (photoArchiveHudInput != null)
        {
            archiveHudInputWasEnabled = photoArchiveHudInput.enabled;
            photoArchiveHudInput.enabled = false;
        }

        EnsureArchiveRenderTexture();
        if (archiveRenderTexture == null)
        {
            if (photoArchiveHudInput != null)
            {
                photoArchiveHudInput.enabled = archiveHudInputWasEnabled;
            }
            return;
        }

        previousPhotoTargetTexture = photoCamera.targetTexture;
        photoCamera.targetTexture = archiveRenderTexture;
        archiveOverlayOpen = true;
        ShowSecondUiPhotoPage();
        SetHint(string.Empty, false);
    }

    private void ShowSecondUiPhotoPage()
    {
        if (!archiveOverlayOpen || photoArchiveHud == null)
        {
            return;
        }

        HearthFirstPersonHudPage page = FindPhotoArchivePage(currentPageIndex);
        if (page == null)
        {
            return;
        }

        page.Configure(page.PageId, page.FullscreenTakeover, false);
        photoArchiveHud.ShowPage(page.PageId);
        activeArchivePage = page;
        SetArchiveNarrativeLaneVisible(page, false);
        BindArchiveCameraFeed(page);
        activeArchiveHintText = FindNamedText(page.transform, "V2_PhotoReturnHint");
        TMP_Text pageCounter = FindNamedText(page.transform, "V2_PhotoPage");
        if (pageCounter != null)
        {
            pageCounter.text = string.Format(
                "PAGE {0:00} / {1:00}",
                currentPageIndex + 1,
                HasSecondPhoto ? 2 : 1);
        }

        string label = dialogueComplete
            ? (pageNavigationEnabled ? browseAndExitHintLabel : exitHintLabel)
            : pageNavigationEnabled
                ? navigationHintLabel
                : string.Empty;
        SetHint(label, dialogueComplete || pageNavigationEnabled);
    }

    private void PrepareSecondUiPhotoArchiveExit()
    {
        if (!archiveOverlayOpen)
        {
            return;
        }

        RestorePhotoCameraTarget();
        if (activeArchivePage != null)
        {
            activeArchivePage.Hide();
        }
    }

    private void CloseSecondUiPhotoArchive()
    {
        if (!archiveOverlayOpen)
        {
            return;
        }

        RestorePhotoCameraTarget();
        archiveOverlayOpen = false;
        activeArchivePage = null;
        activeArchiveHintText = null;
        SetAllArchiveNarrativeLanesVisible(true);

        if (photoArchiveHud != null)
        {
            photoArchiveHud.ShowPage(previousHudPage);
        }

        if (photoArchiveHudInput != null)
        {
            photoArchiveHudInput.enabled = archiveHudInputWasEnabled;
        }

        SetHint(string.Empty, false);
    }

    private HearthFirstPersonHudPage FindPhotoArchivePage(int pageIndex)
    {
        if (photoArchiveHud == null)
        {
            return null;
        }

        HearthFirstPersonHudPageId requestedId = pageIndex == 1
            ? HearthFirstPersonHudPageId.Slide08Photo2026
            : HearthFirstPersonHudPageId.Slide07Photo2023;
        HearthFirstPersonHudPage[] pages =
            photoArchiveHud.GetComponentsInChildren<HearthFirstPersonHudPage>(true);
        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null && pages[i].PageId == requestedId)
            {
                return pages[i];
            }
        }

        return null;
    }

    private void BindArchiveCameraFeed(HearthFirstPersonHudPage page)
    {
        if (page == null || archiveRenderTexture == null)
        {
            return;
        }

        Transform viewport = FindNamedChild(page.transform, "V2_PhotoViewport");
        if (viewport == null)
        {
            return;
        }

        Transform existing = viewport.Find("PhotoCameraFeed_V2");
        RawImage feed;
        if (existing == null)
        {
            GameObject target = new GameObject(
                "PhotoCameraFeed_V2",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(RawImage));
            target.transform.SetParent(viewport, false);
            RectTransform rect = target.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(8f, 8f);
            rect.offsetMax = new Vector2(-8f, -8f);
            target.transform.SetAsFirstSibling();
            feed = target.GetComponent<RawImage>();
        }
        else
        {
            feed = existing.GetComponent<RawImage>();
        }

        if (feed != null)
        {
            feed.texture = archiveRenderTexture;
            feed.color = Color.white;
            feed.raycastTarget = false;
            feed.uvRect = new Rect(0f, 0f, 1f, 1f);
        }
    }

    private void SetAllArchiveNarrativeLanesVisible(bool visible)
    {
        SetArchiveNarrativeLaneVisible(FindPhotoArchivePage(0), visible);
        SetArchiveNarrativeLaneVisible(FindPhotoArchivePage(1), visible);
    }

    private static void SetArchiveNarrativeLaneVisible(
        HearthFirstPersonHudPage page,
        bool visible)
    {
        if (page == null)
        {
            return;
        }

        Transform lane = FindNamedChild(page.transform, "V2_PhotoFieldUnit");
        if (lane != null)
        {
            lane.gameObject.SetActive(visible);
        }
    }

    private void EnsureArchiveRenderTexture()
    {
        int width = Mathf.Max(320, archiveRenderTextureSize.x);
        int height = Mathf.Max(180, archiveRenderTextureSize.y);
        if (archiveRenderTexture != null &&
            (archiveRenderTexture.width != width || archiveRenderTexture.height != height))
        {
            ReleaseArchiveRenderTexture();
        }

        if (archiveRenderTexture != null)
        {
            if (!archiveRenderTexture.IsCreated())
            {
                archiveRenderTexture.Create();
            }
            return;
        }

        archiveRenderTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
        {
            name = "HEARTH_PhotoArchiveFeed_V2",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            useMipMap = false,
            autoGenerateMips = false
        };
        archiveRenderTexture.Create();
    }

    private void RestorePhotoCameraTarget()
    {
        if (photoCamera != null && photoCamera.targetTexture == archiveRenderTexture)
        {
            photoCamera.targetTexture = previousPhotoTargetTexture;
        }

        previousPhotoTargetTexture = null;
    }

    private void ReleaseArchiveRenderTexture()
    {
        if (archiveRenderTexture == null)
        {
            return;
        }

        archiveRenderTexture.Release();
        Destroy(archiveRenderTexture);
        archiveRenderTexture = null;
    }

    private static Transform FindNamedChild(Transform root, string childName)
    {
        if (root == null)
        {
            return null;
        }

        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            if (children[i] != null && children[i].name == childName)
            {
                return children[i];
            }
        }

        return null;
    }

    private static TMP_Text FindNamedText(Transform root, string childName)
    {
        Transform child = FindNamedChild(root, childName);
        return child != null ? child.GetComponent<TMP_Text>() : null;
    }
}
