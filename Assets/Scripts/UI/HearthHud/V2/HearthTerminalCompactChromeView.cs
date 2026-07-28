using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class HearthTerminalCompactChromeView : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private HearthTvTerminalController terminal;

    [Header("Chrome")]
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private TMP_Text terminalLabel;
    [SerializeField] private TMP_Text residentLabel;
    [SerializeField] private TMP_Text beforeLabel;
    [SerializeField] private TMP_Text afterLabel;
    [SerializeField] private TMP_Text primaryActionLabel;
    [SerializeField] private TMP_Text statusLabel;
    [SerializeField] private TMP_Text footerLabel;
    [SerializeField] private Image beforeBackground;
    [SerializeField] private Image afterBackground;
    [SerializeField] private Image primaryActionBackground;

    private static readonly Color IdleBackground =
        new Color(0.09f, 0.14f, 0.21f, 0.9f);
    private static readonly Color FocusBackground =
        new Color(0.29f, 0.41f, 0.54f, 0.96f);
    private static readonly Color LockedBackground =
        new Color(0.16f, 0.18f, 0.21f, 0.88f);
    private static readonly Color Text =
        new Color(0.84f, 0.9f, 0.96f, 1f);
    private static readonly Color Muted =
        new Color(0.37f, 0.47f, 0.58f, 0.95f);

    private void Awake()
    {
        ResolveTerminal();
        Subscribe();
        Refresh();
    }

    private void OnEnable()
    {
        ResolveTerminal();
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void Configure(
        HearthTvTerminalController newTerminal,
        GameObject newVisualRoot,
        TMP_Text newTerminalLabel,
        TMP_Text newResidentLabel,
        TMP_Text newBeforeLabel,
        TMP_Text newAfterLabel,
        TMP_Text newPrimaryActionLabel,
        TMP_Text newStatusLabel,
        TMP_Text newFooterLabel,
        Image newBeforeBackground,
        Image newAfterBackground,
        Image newPrimaryActionBackground)
    {
        Unsubscribe();
        terminal = newTerminal;
        visualRoot = newVisualRoot;
        terminalLabel = newTerminalLabel;
        residentLabel = newResidentLabel;
        beforeLabel = newBeforeLabel;
        afterLabel = newAfterLabel;
        primaryActionLabel = newPrimaryActionLabel;
        statusLabel = newStatusLabel;
        footerLabel = newFooterLabel;
        beforeBackground = newBeforeBackground;
        afterBackground = newAfterBackground;
        primaryActionBackground = newPrimaryActionBackground;
        Subscribe();
        Refresh();
    }

    public void Refresh()
    {
        if (terminal == null)
        {
            return;
        }

        Apply(terminal.TerminalViewState);
    }

    private void Apply(HearthTerminalViewState state)
    {
        if (state == null)
        {
            return;
        }

        if (visualRoot != null)
        {
            visualRoot.SetActive(true);
        }

        bool home = terminal.TerminalMode == HearthTerminalMode.Home;
        SetVisible(beforeBackground, !home);
        SetVisible(afterBackground, !home);
        SetText(terminalLabel, home ? "HOME TERMINAL" : "DOORWAY TERMINAL");
        SetText(residentLabel, state.TerminalId);
        SetText(beforeLabel, "BEFORE ACQUISITION");
        SetText(afterLabel, "AFTER ACQUISITION");
        SetText(primaryActionLabel, state.PrimaryActionLabel);
        SetText(statusLabel, state.StatusMessage);
        SetText(
            footerLabel,
            home && state.CanExit
                ? "SPACE  CONFIRM     ESC  EXIT"
                : home
                    ? "SPACE  CONFIRM"
                    : state.CanExit
                ? "LEFT / RIGHT  SELECT     SPACE  CONFIRM     ESC  EXIT"
                : "LEFT / RIGHT  SELECT     SPACE  CONFIRM");

        SetTabState(
            beforeBackground,
            beforeLabel,
            state.FocusTarget ==
                HearthTerminalFocusTarget.BeforeAcquisitionTab,
            false);
        SetTabState(
            afterBackground,
            afterLabel,
            state.FocusTarget ==
                HearthTerminalFocusTarget.AfterAcquisitionTab,
            false);
        SetTabState(
            primaryActionBackground,
            primaryActionLabel,
            state.FocusTarget == HearthTerminalFocusTarget.PrimaryAction,
            state.PrimaryActionLocked);
    }

    private static void SetTabState(
        Image background,
        TMP_Text label,
        bool focused,
        bool locked)
    {
        if (background != null)
        {
            background.color = locked
                ? LockedBackground
                : focused
                    ? FocusBackground
                    : IdleBackground;
        }

        if (label != null)
        {
            label.color = focused && !locked ? Text : Muted;
        }
    }

    private static void SetText(TMP_Text target, string value)
    {
        if (target == null)
        {
            return;
        }

        target.text = value ?? string.Empty;
        target.gameObject.SetActive(!string.IsNullOrEmpty(target.text));
    }

    private static void SetVisible(Image target, bool visible)
    {
        if (target != null)
        {
            target.gameObject.SetActive(visible);
        }
    }

    private void ResolveTerminal()
    {
        if (terminal == null)
        {
            terminal = GetComponent<HearthTvTerminalController>();
        }
    }

    private void Subscribe()
    {
        if (terminal == null)
        {
            return;
        }

        terminal.TerminalViewStateChanged -= Apply;
        terminal.TerminalViewStateChanged += Apply;
    }

    private void Unsubscribe()
    {
        if (terminal != null)
        {
            terminal.TerminalViewStateChanged -= Apply;
        }
    }
}
