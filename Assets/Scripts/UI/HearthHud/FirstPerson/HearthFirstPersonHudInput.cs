using System;
using UnityEngine;

public enum HearthFinalChoiceNavigationAxis
{
    Horizontal,
    Vertical
}

[Serializable]
public struct HearthFinalChoiceInputProfile
{
    public HearthFinalChoiceNavigationAxis navigationAxis;
    public bool allowDirectLetterKeys;
    public bool allowReturnSubmit;

    public HearthFinalChoiceInputProfile(
        HearthFinalChoiceNavigationAxis navigationAxis,
        bool allowDirectLetterKeys,
        bool allowReturnSubmit)
    {
        this.navigationAxis = navigationAxis;
        this.allowDirectLetterKeys = allowDirectLetterKeys;
        this.allowReturnSubmit = allowReturnSubmit;
    }
}

[DisallowMultipleComponent]
public class HearthFirstPersonHudInput : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HearthFirstPersonHudController controller;
    [SerializeField] private HearthSettingsView settingsView;

    [Header("Input")]
    [SerializeField] private bool enableKeyboardInput = true;
    [SerializeField] private KeyCode menuKey = KeyCode.Tab;
    [SerializeField] private KeyCode cancelKey = KeyCode.Escape;
    [SerializeField] private KeyCode submitKey = KeyCode.Space;
    [SerializeField] private KeyCode upKey = KeyCode.UpArrow;
    [SerializeField] private KeyCode downKey = KeyCode.DownArrow;
    [SerializeField] private KeyCode leftKey = KeyCode.LeftArrow;
    [SerializeField] private KeyCode rightKey = KeyCode.RightArrow;
    [SerializeField] private KeyCode chooseAKey = KeyCode.A;
    [SerializeField] private KeyCode chooseBKey = KeyCode.B;

    [Header("Final Choice")]
    [SerializeField] private HearthFinalChoiceInputProfile finalChoiceInputProfile =
        new HearthFinalChoiceInputProfile(HearthFinalChoiceNavigationAxis.Horizontal, true, true);

    [Header("Settings")]
    [SerializeField] private int settingsVolumeStep = 5;

    private void Awake()
    {
        ResolveReferences();
    }

    private void Update()
    {
        if (!enableKeyboardInput)
        {
            return;
        }

        ResolveReferences();
        if (controller == null)
        {
            return;
        }

        HearthFirstPersonHudPageId page = controller.CurrentPageId;

        if (Input.GetKeyDown(menuKey))
        {
            HandleMenuKey(page);
            return;
        }

        if (Input.GetKeyDown(cancelKey))
        {
            controller.HandleCancel();
            return;
        }

        bool isFinalChoice = IsFinalChoicePage(page);
        bool submitPressed = Input.GetKeyDown(submitKey) ||
                             (Input.GetKeyDown(KeyCode.Return) &&
                              (!isFinalChoice || finalChoiceInputProfile.allowReturnSubmit));
        if (submitPressed)
        {
            controller.HandleSubmit();
            return;
        }

        if (finalChoiceInputProfile.allowDirectLetterKeys && Input.GetKeyDown(chooseAKey) && isFinalChoice)
        {
            controller.ChooseFinalA();
            return;
        }

        if (finalChoiceInputProfile.allowDirectLetterKeys && Input.GetKeyDown(chooseBKey) && isFinalChoice)
        {
            controller.ChooseFinalB();
            return;
        }

        if (Input.GetKeyDown(upKey))
        {
            HandleVertical(page, -1);
            return;
        }

        if (Input.GetKeyDown(downKey))
        {
            HandleVertical(page, 1);
            return;
        }

        if (Input.GetKeyDown(leftKey))
        {
            HandleHorizontal(page, -1);
            return;
        }

        if (Input.GetKeyDown(rightKey))
        {
            HandleHorizontal(page, 1);
            return;
        }

        if (Input.anyKeyDown && IsDismissableStoryPage(page))
        {
            controller.CloseStoryPopup();
        }
    }

    public void SetKeyboardInputEnabled(bool value)
    {
        enableKeyboardInput = value;
    }

    public HearthFinalChoiceInputProfile GetFinalChoiceInputProfile()
    {
        return finalChoiceInputProfile;
    }

    public void SetFinalChoiceInputProfile(HearthFinalChoiceInputProfile profile)
    {
        finalChoiceInputProfile = profile;
    }

    private void HandleMenuKey(HearthFirstPersonHudPageId page)
    {
        if (page == HearthFirstPersonHudPageId.Slide01PersistentHud ||
            page == HearthFirstPersonHudPageId.None)
        {
            controller.OpenMainMenu();
            return;
        }

        if (page == HearthFirstPersonHudPageId.Slide03MainMenu ||
            page == HearthFirstPersonHudPageId.Slide05TodayRounds ||
            IsHistoryPage(page) ||
            IsSettingsPage(page))
        {
            controller.HideOverlay();
        }
    }

    private void HandleVertical(HearthFirstPersonHudPageId page, int direction)
    {
        if (IsFinalChoicePage(page) &&
            finalChoiceInputProfile.navigationAxis == HearthFinalChoiceNavigationAxis.Vertical)
        {
            controller.MoveFinalChoiceFocus(direction);
            return;
        }

        if (page == HearthFirstPersonHudPageId.Slide03MainMenu)
        {
            controller.MoveMenuFocus(direction);
            return;
        }

        if (IsSettingsPage(page) && settingsView != null)
        {
            settingsView.MoveFocus(direction);
        }
    }

    private void HandleHorizontal(HearthFirstPersonHudPageId page, int direction)
    {
        if (IsFinalChoicePage(page) &&
            finalChoiceInputProfile.navigationAxis == HearthFinalChoiceNavigationAxis.Horizontal)
        {
            controller.MoveFinalChoiceFocus(direction);
            return;
        }

        if (IsSettingsPage(page) && settingsView != null)
        {
            settingsView.AdjustFocusedVolume(direction * settingsVolumeStep);
        }
    }

    private void ResolveReferences()
    {
        if (controller == null)
        {
            controller = GetComponent<HearthFirstPersonHudController>();
        }

        if (controller == null)
        {
            controller = FindObjectOfType<HearthFirstPersonHudController>();
        }

        if (settingsView == null && controller != null)
        {
            settingsView = GetComponentInChildren<HearthSettingsView>(true);
        }
    }

    private static bool IsFinalChoicePage(HearthFirstPersonHudPageId page)
    {
        return page == HearthFirstPersonHudPageId.Slide09FinalChoice ||
               page == HearthFirstPersonHudPageId.Slide14FinalChoiceReturn;
    }

    private static bool IsHistoryPage(HearthFirstPersonHudPageId page)
    {
        return page == HearthFirstPersonHudPageId.Slide18HistoryEmpty ||
               page == HearthFirstPersonHudPageId.Slide19HistoryOne ||
               page == HearthFirstPersonHudPageId.Slide20HistoryTwo ||
               page == HearthFirstPersonHudPageId.Slide21HistoryThree;
    }

    private static bool IsSettingsPage(HearthFirstPersonHudPageId page)
    {
        return page == HearthFirstPersonHudPageId.Slide22Settings ||
               page == HearthFirstPersonHudPageId.Slide23SettingsFocus ||
               page == HearthFirstPersonHudPageId.Slide24ExitConfirm;
    }

    private static bool IsDismissableStoryPage(HearthFirstPersonHudPageId page)
    {
        return page == HearthFirstPersonHudPageId.Slide06HomeWelcome ||
               page == HearthFirstPersonHudPageId.Slide07Photo2023 ||
               page == HearthFirstPersonHudPageId.Slide08Photo2026;
    }
}
