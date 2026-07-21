using UnityEngine;

[DisallowMultipleComponent]
public class HearthCompanionHudPreviewInput : MonoBehaviour
{
    [Header("Preview")]
    [SerializeField] private bool previewInputEnabled;
    [SerializeField] private HearthCompanionHudController controller;

    [Header("Keys")]
    [SerializeField] private KeyCode nextSceneKey = KeyCode.RightBracket;
    [SerializeField] private KeyCode previousSceneKey = KeyCode.LeftBracket;
    [SerializeField] private KeyCode nextSceneAltKey = KeyCode.PageDown;
    [SerializeField] private KeyCode previousSceneAltKey = KeyCode.PageUp;
    [SerializeField] private KeyCode completePromptKey = KeyCode.Return;
    [SerializeField] private KeyCode toggleVisibleKey = KeyCode.BackQuote;

    private bool visible = true;

    private void Awake()
    {
        ResolveController();
    }

    private void Update()
    {
        if (!previewInputEnabled)
        {
            return;
        }

        ResolveController();
        if (controller == null)
        {
            return;
        }

        for (int i = 1; i <= 9; i++)
        {
            KeyCode numberKey = (KeyCode)((int)KeyCode.Alpha0 + i);
            if (Input.GetKeyDown(numberKey))
            {
                controller.ShowScene(i);
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.F10))
        {
            controller.ShowScene(10);
        }
        else if (Input.GetKeyDown(KeyCode.F11))
        {
            controller.ShowScene(11);
        }
        else if (Input.GetKeyDown(KeyCode.F12))
        {
            controller.ShowScene(12);
        }
        else if (Input.GetKeyDown(nextSceneKey) || Input.GetKeyDown(nextSceneAltKey))
        {
            controller.AdvanceScene();
        }
        else if (Input.GetKeyDown(previousSceneKey) || Input.GetKeyDown(previousSceneAltKey))
        {
            controller.ShowPreviousScene();
        }
        else if (Input.GetKeyDown(completePromptKey))
        {
            controller.ConfirmCurrentPrompt();
        }
        else if (Input.GetKeyDown(toggleVisibleKey))
        {
            visible = !visible;
            controller.SetVisible(visible);
        }
    }

    public void SetPreviewInputEnabled(bool enabled)
    {
        previewInputEnabled = enabled;
    }

    private void ResolveController()
    {
        if (controller == null)
        {
            controller = GetComponent<HearthCompanionHudController>();
        }

        if (controller == null)
        {
            controller = GetComponentInParent<HearthCompanionHudController>();
        }
    }
}
