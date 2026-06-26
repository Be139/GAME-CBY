using UnityEngine;

[DisallowMultipleComponent]
public class HearthHudPreviewInput : MonoBehaviour
{
    [SerializeField] private HearthHudController controller;
    [SerializeField] private bool enablePreviewInput = true;
    [SerializeField] private KeyCode previousPageKey = KeyCode.LeftBracket;
    [SerializeField] private KeyCode nextPageKey = KeyCode.RightBracket;
    [SerializeField] private KeyCode homeKey = KeyCode.Escape;
    [SerializeField] private KeyCode tabKey = KeyCode.Tab;

    private HearthDoorwayTab previewTab = HearthDoorwayTab.ResidentSummary;

    private void Awake()
    {
        if (controller == null)
        {
            controller = GetComponent<HearthHudController>();
        }

        if (controller == null)
        {
            controller = FindObjectOfType<HearthHudController>();
        }
    }

    private void Update()
    {
        if (!enablePreviewInput || controller == null)
        {
            return;
        }

        if (Input.GetKeyDown(previousPageKey))
        {
            controller.ShowPreviousPage();
        }

        if (Input.GetKeyDown(nextPageKey))
        {
            controller.ShowNextPage();
        }

        if (Input.GetKeyDown(homeKey))
        {
            controller.ShowPage(HearthHudPageId.Slide01PersistentActive);
        }

        if (Input.GetKeyDown(tabKey))
        {
            CycleTab();
        }

        HandleNumberShortcuts();
    }

    public void SetPreviewInputEnabled(bool value)
    {
        enablePreviewInput = value;
    }

    private void HandleNumberShortcuts()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            controller.ShowPage(HearthHudPageId.Slide01PersistentActive);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            controller.ShowPage(HearthHudPageId.Slide03DoorwaySummary);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            controller.ShowPage(HearthHudPageId.Slide05DoorwayDisposition);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            controller.ShowRobotReplay(HearthHudPageId.Slide05DoorwayDisposition);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            controller.ShowPage(HearthHudPageId.Slide11WorkspaceQuickMenu);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            controller.ShowPage(HearthHudPageId.Slide13AlertDoorwaySummary);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            controller.ShowPage(HearthHudPageId.Slide19IndoorSidePanel);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            controller.ShowPage(HearthHudPageId.Slide08FinalChoice);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            controller.ShowPage(HearthHudPageId.Slide23Warning00);
        }
    }

    private void CycleTab()
    {
        int next = ((int)previewTab + 1) % 5;
        previewTab = (HearthDoorwayTab)next;
        controller.SelectDoorwayTab(previewTab);
    }
}
