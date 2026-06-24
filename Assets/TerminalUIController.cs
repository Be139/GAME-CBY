using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TerminalUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject terminalPanel;
    [SerializeField] private Button closeButton;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private GameObject initialContentPrefab;
    [SerializeField] private bool createFallbackUI = true;
    [SerializeField] private bool createEventSystemIfMissing = true;

    [Header("Player Lock")]
    [SerializeField] private bool lockPlayerWhileOpen = true;
    [SerializeField] private FirstPersonMovement playerMovement;
    [SerializeField] private FirstPersonLook playerLook;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private Rigidbody playerRigidbody;

    private CursorLockMode previousCursorLockState;
    private bool previousCursorVisible;
    private bool movementWasEnabled;
    private bool lookWasEnabled;
    private bool interactionWasEnabled;
    private GameObject activeContent;

    public bool IsOpen { get; private set; }

    public Transform ContentRoot
    {
        get { return contentRoot; }
    }

    private void Awake()
    {
        if (createFallbackUI && (terminalPanel == null || closeButton == null || contentRoot == null))
        {
            CreateFallbackUI();
        }

        if (terminalPanel != null)
        {
            terminalPanel.SetActive(false);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
        }

        if (initialContentPrefab != null)
        {
            SetContent(initialContentPrefab);
        }
    }

    private void CreateFallbackUI()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Terminal Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler canvasScaler = canvasObject.GetComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920f, 1080f);
        }

        if (terminalPanel == null)
        {
            GameObject panelObject = new GameObject("Terminal Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelObject.transform.SetParent(canvas.transform, false);

            RectTransform panelRect = panelObject.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(900f, 560f);
            panelRect.anchoredPosition = Vector2.zero;

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = new Color(0.06f, 0.06f, 0.07f, 0.96f);

            terminalPanel = panelObject;
        }

        RectTransform terminalPanelRect = terminalPanel.GetComponent<RectTransform>();
        if (terminalPanelRect == null)
        {
            return;
        }

        if (contentRoot == null)
        {
            GameObject contentObject = new GameObject("Content Root", typeof(RectTransform));
            contentObject.transform.SetParent(terminalPanel.transform, false);

            RectTransform contentRect = contentObject.GetComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = new Vector2(32f, 32f);
            contentRect.offsetMax = new Vector2(-32f, -32f);

            contentRoot = contentObject.transform;
        }

        if (closeButton == null)
        {
            GameObject buttonObject = new GameObject("Close Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(terminalPanel.transform, false);

            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(1f, 1f);
            buttonRect.anchorMax = new Vector2(1f, 1f);
            buttonRect.pivot = new Vector2(1f, 1f);
            buttonRect.sizeDelta = new Vector2(44f, 44f);
            buttonRect.anchoredPosition = new Vector2(-16f, -16f);

            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = new Color(0f, 0f, 0f, 0.35f);

            closeButton = buttonObject.GetComponent<Button>();
            CreateCloseButtonLine(buttonRect, 45f);
            CreateCloseButtonLine(buttonRect, -45f);
        }
    }

    private void CreateCloseButtonLine(RectTransform parent, float angle)
    {
        GameObject lineObject = new GameObject("Close Line", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        lineObject.transform.SetParent(parent, false);

        RectTransform lineRect = lineObject.GetComponent<RectTransform>();
        lineRect.anchorMin = new Vector2(0.5f, 0.5f);
        lineRect.anchorMax = new Vector2(0.5f, 0.5f);
        lineRect.pivot = new Vector2(0.5f, 0.5f);
        lineRect.sizeDelta = new Vector2(22f, 2f);
        lineRect.anchoredPosition = Vector2.zero;
        lineRect.localRotation = Quaternion.Euler(0f, 0f, angle);

        Image lineImage = lineObject.GetComponent<Image>();
        lineImage.color = Color.white;
    }

    private void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
        }
    }

    private void Update()
    {
        if (IsOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
    }

    public void Open()
    {
        if (IsOpen)
        {
            return;
        }

        EnsureEventSystem();
        IsOpen = true;
        previousCursorLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        if (terminalPanel != null)
        {
            terminalPanel.SetActive(true);
        }

        SetPlayerLocked(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Close()
    {
        if (!IsOpen)
        {
            return;
        }

        IsOpen = false;

        if (terminalPanel != null)
        {
            terminalPanel.SetActive(false);
        }

        SetPlayerLocked(false);
        Cursor.lockState = previousCursorLockState;
        Cursor.visible = previousCursorVisible;
    }

    public void SetContent(GameObject contentPrefab)
    {
        ClearContent();

        if (contentPrefab == null || contentRoot == null)
        {
            return;
        }

        activeContent = Instantiate(contentPrefab, contentRoot);
        activeContent.SetActive(true);
    }

    public void ClearContent()
    {
        if (activeContent == null)
        {
            return;
        }

        Destroy(activeContent);
        activeContent = null;
    }

    private void EnsureEventSystem()
    {
        if (!createEventSystemIfMissing || EventSystem.current != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        eventSystemObject.transform.SetParent(transform, false);
    }

    private void SetPlayerLocked(bool locked)
    {
        if (!lockPlayerWhileOpen)
        {
            return;
        }

        if (locked)
        {
            movementWasEnabled = playerMovement != null && playerMovement.enabled;
            lookWasEnabled = playerLook != null && playerLook.enabled;
            interactionWasEnabled = playerInteraction != null && playerInteraction.InteractionEnabled;

            if (playerMovement != null)
            {
                playerMovement.enabled = false;
            }

            if (playerLook != null)
            {
                playerLook.enabled = false;
            }

            if (playerInteraction != null)
            {
                playerInteraction.SetInteractionEnabled(false);
            }

            if (playerRigidbody != null)
            {
                playerRigidbody.velocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
            }
        }
        else
        {
            if (playerMovement != null)
            {
                playerMovement.enabled = movementWasEnabled;
            }

            if (playerLook != null)
            {
                playerLook.enabled = lookWasEnabled;
            }

            if (playerInteraction != null)
            {
                playerInteraction.SetInteractionEnabled(interactionWasEnabled);
            }
        }
    }
}
