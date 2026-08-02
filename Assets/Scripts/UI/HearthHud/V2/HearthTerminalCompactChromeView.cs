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
        new Color(0.37f, 0.47f, 0.58f, 0.16f);
    private static readonly Color FocusBackground =
        new Color(0.47f, 0.67f, 0.86f, 0.34f);
    private static readonly Color LockedBackground =
        new Color(0.34f, 0.40f, 0.48f, 0.12f);
    private static readonly Color Text =
        new Color(0.96f, 0.98f, 1f, 1f);
    private static readonly Color Muted =
        new Color(0.82f, 0.90f, 0.98f, 1f);

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
            visualRoot.SetActive(state.Visible);
        }

        if (!state.Visible)
        {
            return;
        }

        bool home = terminal.TerminalMode == HearthTerminalMode.Home;
        SetVisible(beforeBackground, !home);
        SetVisible(afterBackground, !home);
        SetText(terminalLabel, home ? "HOME TERMINAL" : "DOORWAY TERMINAL");
        SetText(
            residentLabel,
            home ? string.Empty : FormatTerminalId(state.TerminalId));
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

    private static string FormatTerminalId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string compact =
            value.Trim()
                .Replace("-", string.Empty)
                .Replace(" ", string.Empty)
                .ToUpperInvariant();
        if (compact.Length == 5 &&
            compact.StartsWith("17F", System.StringComparison.Ordinal))
        {
            return compact.Substring(0, 3) + "-" + compact.Substring(3);
        }

        return value.Trim();
    }

    private static void SetVisible(Image target, bool visible)
    {
        if (target != null)
        {
            Transform tabRoot =
                target.name == "SelectionFill" && target.transform.parent != null
                    ? target.transform.parent
                    : target.transform;
            tabRoot.gameObject.SetActive(visible);
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
