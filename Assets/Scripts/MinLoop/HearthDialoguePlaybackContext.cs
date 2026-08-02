/// <summary>
/// Selects where framed dialogue is rendered for one sequence. Natural
/// captions, time cards and epilogues deliberately remain on the global
/// subtitle canvas even when an embedded terminal surface is supplied.
/// </summary>
public sealed class HearthDialoguePlaybackContext
{
    private HearthDialoguePlaybackContext(
        HearthDialogueSurface framedSurface,
        HearthDialogueSurface messageSurface,
        bool hasSubtitleContextOverride,
        HearthSubtitleContext subtitleContext)
    {
        FramedSurface = framedSurface;
        MessageSurface = messageSurface;
        HasSubtitleContextOverride = hasSubtitleContextOverride;
        SubtitleContext = subtitleContext;
    }

    public HearthDialogueSurface FramedSurface { get; private set; }

    public HearthDialogueSurface MessageSurface { get; private set; }

    public bool HasSubtitleContextOverride { get; private set; }

    public HearthSubtitleContext SubtitleContext { get; private set; }

    public static HearthDialoguePlaybackContext Global()
    {
        return new HearthDialoguePlaybackContext(
            null,
            null,
            false,
            HearthSubtitleContext.Human);
    }

    public static HearthDialoguePlaybackContext Global(
        HearthSubtitleContext subtitleContext)
    {
        return new HearthDialoguePlaybackContext(
            null,
            null,
            true,
            subtitleContext);
    }

    public static HearthDialoguePlaybackContext Embedded(
        HearthDialogueSurface framedSurface,
        HearthSubtitleContext subtitleContext = HearthSubtitleContext.Terminal)
    {
        return new HearthDialoguePlaybackContext(
            framedSurface,
            null,
            true,
            subtitleContext);
    }

    public static HearthDialoguePlaybackContext Embedded(
        HearthDialogueSurface framedSurface,
        HearthDialogueSurface messageSurface,
        HearthSubtitleContext subtitleContext = HearthSubtitleContext.Terminal)
    {
        return new HearthDialoguePlaybackContext(
            framedSurface,
            messageSurface,
            true,
            subtitleContext);
    }
}
