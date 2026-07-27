using System;
using UnityEngine;

public enum HearthUiLayoutRegion
{
    GlobalSafeArea,
    TerminalSafeArea,
    HumanIdentity,
    CurrentTask,
    SharedLocation,
    SharedSubtitleSpeaker,
    SharedSubtitleBody,
    InitialHumanTutorial,
    DynamicInteractionPrompt,
    TerminalHeaderAndNavigation,
    TerminalPrimaryAction,
    TerminalContent,
    TerminalMessageLane,
    TerminalFooter
}

[Serializable]
public struct HearthUiReferenceRect
{
    [SerializeField] private float left;
    [SerializeField] private float top;
    [SerializeField] private float width;
    [SerializeField] private float height;

    public HearthUiReferenceRect(float left, float top, float width, float height)
    {
        this.left = left;
        this.top = top;
        this.width = width;
        this.height = height;
    }

    public float Left { get { return left; } }
    public float Top { get { return top; } }
    public float Width { get { return width; } }
    public float Height { get { return height; } }
    public float Right { get { return left + width; } }
    public float Bottom { get { return top + height; } }
    public Vector2 Size { get { return new Vector2(width, height); } }

    public Rect ToBottomLeftRect(Vector2 referenceResolution)
    {
        return new Rect(
            left,
            referenceResolution.y - top - height,
            width,
            height);
    }

    public void ApplyTopLeftAnchors(RectTransform target)
    {
        if (target == null)
        {
            return;
        }

        target.anchorMin = new Vector2(0f, 1f);
        target.anchorMax = new Vector2(0f, 1f);
        target.pivot = new Vector2(0f, 1f);
        target.anchoredPosition = new Vector2(left, -top);
        target.sizeDelta = new Vector2(width, height);
    }
}

[CreateAssetMenu(
    menuName = "Hearth/UI/V2/Layout Profile",
    fileName = "Hearth_UiV2Layout_1920x1080")]
public sealed class HearthUiLayoutProfile : ScriptableObject
{
    [Header("Reference Space")]
    [Tooltip("All regions below use top-left 1920 x 1080 design coordinates.")]
    [SerializeField] private Vector2 referenceResolution = new Vector2(1920f, 1080f);
    [SerializeField] private HearthUiReferenceRect globalSafeArea =
        new HearthUiReferenceRect(48f, 40f, 1824f, 1000f);
    [SerializeField] private HearthUiReferenceRect terminalSafeArea =
        new HearthUiReferenceRect(96f, 64f, 1728f, 968f);

    [Header("Human / Shared HUD")]
    [SerializeField] private HearthUiReferenceRect humanIdentity =
        new HearthUiReferenceRect(64f, 48f, 432f, 96f);
    [SerializeField] private HearthUiReferenceRect currentTask =
        new HearthUiReferenceRect(1408f, 48f, 448f, 104f);
    [SerializeField] private HearthUiReferenceRect sharedLocation =
        new HearthUiReferenceRect(64f, 944f, 360f, 80f);
    [SerializeField] private HearthUiReferenceRect sharedSubtitleSpeaker =
        new HearthUiReferenceRect(480f, 748f, 960f, 32f);
    [SerializeField] private HearthUiReferenceRect sharedSubtitleBody =
        new HearthUiReferenceRect(320f, 792f, 1280f, 96f);
    [SerializeField] private HearthUiReferenceRect initialHumanTutorial =
        new HearthUiReferenceRect(1136f, 928f, 720f, 96f);
    [SerializeField] private HearthUiReferenceRect dynamicInteractionPrompt =
        new HearthUiReferenceRect(660f, 688f, 600f, 64f);

    [Header("Terminal")]
    [SerializeField] private HearthUiReferenceRect terminalHeaderAndNavigation =
        new HearthUiReferenceRect(120f, 72f, 1680f, 140f);
    [SerializeField] private HearthUiReferenceRect terminalPrimaryAction =
        new HearthUiReferenceRect(1480f, 148f, 320f, 56f);
    [SerializeField] private HearthUiReferenceRect terminalContent =
        new HearthUiReferenceRect(120f, 232f, 1680f, 528f);
    [SerializeField] private HearthUiReferenceRect terminalMessageLane =
        new HearthUiReferenceRect(320f, 790f, 1280f, 120f);
    [SerializeField] private HearthUiReferenceRect terminalFooter =
        new HearthUiReferenceRect(96f, 920f, 1728f, 64f);

    public Vector2 ReferenceResolution { get { return referenceResolution; } }

    public HearthUiReferenceRect GetRegion(HearthUiLayoutRegion region)
    {
        switch (region)
        {
            case HearthUiLayoutRegion.GlobalSafeArea:
                return globalSafeArea;
            case HearthUiLayoutRegion.TerminalSafeArea:
                return terminalSafeArea;
            case HearthUiLayoutRegion.HumanIdentity:
                return humanIdentity;
            case HearthUiLayoutRegion.CurrentTask:
                return currentTask;
            case HearthUiLayoutRegion.SharedLocation:
                return sharedLocation;
            case HearthUiLayoutRegion.SharedSubtitleSpeaker:
                return sharedSubtitleSpeaker;
            case HearthUiLayoutRegion.SharedSubtitleBody:
                return sharedSubtitleBody;
            case HearthUiLayoutRegion.InitialHumanTutorial:
                return initialHumanTutorial;
            case HearthUiLayoutRegion.DynamicInteractionPrompt:
                return dynamicInteractionPrompt;
            case HearthUiLayoutRegion.TerminalHeaderAndNavigation:
                return terminalHeaderAndNavigation;
            case HearthUiLayoutRegion.TerminalPrimaryAction:
                return terminalPrimaryAction;
            case HearthUiLayoutRegion.TerminalContent:
                return terminalContent;
            case HearthUiLayoutRegion.TerminalMessageLane:
                return terminalMessageLane;
            case HearthUiLayoutRegion.TerminalFooter:
                return terminalFooter;
            default:
                throw new ArgumentOutOfRangeException("region", region, "Unknown HEARTH UI layout region.");
        }
    }

    private void OnValidate()
    {
        referenceResolution.x = Mathf.Max(1f, referenceResolution.x);
        referenceResolution.y = Mathf.Max(1f, referenceResolution.y);
    }
}
