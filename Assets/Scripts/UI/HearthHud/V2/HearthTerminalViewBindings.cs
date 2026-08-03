using UnityEngine;

/// <summary>
/// Explicit terminal presentation bindings. Page-owned surfaces are selected
/// by hierarchy ownership, never by display text or generated object names.
/// </summary>
[DisallowMultipleComponent]
public sealed class HearthTerminalViewBindings : MonoBehaviour
{
    [SerializeField] private HearthDialogueSurface terminalDialogueSurface;
    [SerializeField] private HearthDialogueSurface terminalMessageSurface;
    [SerializeField] private HearthDialogueSurface[] pageDialogueSurfaces =
        new HearthDialogueSurface[0];

    public HearthDialogueSurface TerminalDialogueSurface
    {
        get { return terminalDialogueSurface; }
    }

    public HearthDialogueSurface TerminalMessageSurface
    {
        get { return terminalMessageSurface; }
    }

    public HearthDialogueSurface[] PageDialogueSurfaces
    {
        get { return pageDialogueSurfaces; }
    }

    public bool HasDialogueSurface
    {
        get
        {
            if (terminalDialogueSurface != null) return true;
            if (pageDialogueSurfaces == null) return false;
            for (int i = 0; i < pageDialogueSurfaces.Length; i++)
            {
                if (pageDialogueSurfaces[i] != null) return true;
            }
            return false;
        }
    }

    public bool HasMessageSurface
    {
        get { return terminalMessageSurface != null; }
    }

    public void Configure(
        HearthDialogueSurface newTerminalDialogueSurface,
        HearthDialogueSurface newTerminalMessageSurface,
        HearthDialogueSurface[] newPageDialogueSurfaces)
    {
        terminalDialogueSurface = newTerminalDialogueSurface;
        terminalMessageSurface = newTerminalMessageSurface;
        pageDialogueSurfaces = newPageDialogueSurfaces ??
            new HearthDialogueSurface[0];
    }

    public HearthDialogueSurface ResolveDialogueSurface(HearthHudPage page)
    {
        if (page != null && pageDialogueSurfaces != null)
        {
            for (int i = 0; i < pageDialogueSurfaces.Length; i++)
            {
                HearthDialogueSurface surface = pageDialogueSurfaces[i];
                if (surface != null && surface.transform.IsChildOf(page.transform))
                {
                    return surface;
                }
            }
        }

        return terminalDialogueSurface;
    }
}
