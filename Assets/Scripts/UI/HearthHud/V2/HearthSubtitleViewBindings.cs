using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Explicit authored bindings for the shared subtitle canvas. This component
/// contains no playback rules and never modifies layout.
/// </summary>
[DisallowMultipleComponent]
public sealed class HearthSubtitleViewBindings : MonoBehaviour
{
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private RectTransform layoutRoot;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image backdropImage;
    [SerializeField] private Image accentRuleImage;
    [SerializeField] private Image speakerTabImage;
    [SerializeField] private Image formalFrameImage;
    [SerializeField] private Image auxiliaryFrameImage;
    [SerializeField] private Image leftSpeakerTabImage;
    [SerializeField] private Image rightSpeakerTabImage;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text advanceHintText;
    [SerializeField] private TMP_Text persistentSceneHeaderText;
    [SerializeField] private CanvasGroup persistentSceneHeaderGroup;

    public GameObject VisualRoot { get { return visualRoot; } }
    public RectTransform LayoutRoot { get { return layoutRoot; } }
    public CanvasGroup CanvasGroup { get { return canvasGroup; } }
    public Image BackdropImage { get { return backdropImage; } }
    public Image AccentRuleImage { get { return accentRuleImage; } }
    public Image SpeakerTabImage { get { return speakerTabImage; } }
    public Image FormalFrameImage { get { return formalFrameImage; } }
    public Image AuxiliaryFrameImage { get { return auxiliaryFrameImage; } }
    public Image LeftSpeakerTabImage { get { return leftSpeakerTabImage; } }
    public Image RightSpeakerTabImage { get { return rightSpeakerTabImage; } }
    public TMP_Text SpeakerText { get { return speakerText; } }
    public TMP_Text BodyText { get { return bodyText; } }
    public TMP_Text AdvanceHintText { get { return advanceHintText; } }
    public TMP_Text PersistentSceneHeaderText { get { return persistentSceneHeaderText; } }
    public CanvasGroup PersistentSceneHeaderGroup { get { return persistentSceneHeaderGroup; } }

    public bool IsComplete
    {
        get
        {
            return visualRoot != null && layoutRoot != null &&
                canvasGroup != null && speakerText != null && bodyText != null;
        }
    }

    public void Configure(
        GameObject newVisualRoot,
        RectTransform newLayoutRoot,
        CanvasGroup newCanvasGroup,
        Image newBackdropImage,
        Image newAccentRuleImage,
        Image newSpeakerTabImage,
        Image newFormalFrameImage,
        Image newAuxiliaryFrameImage,
        Image newLeftSpeakerTabImage,
        Image newRightSpeakerTabImage,
        TMP_Text newSpeakerText,
        TMP_Text newBodyText,
        TMP_Text newAdvanceHintText,
        TMP_Text newPersistentSceneHeaderText,
        CanvasGroup newPersistentSceneHeaderGroup)
    {
        visualRoot = newVisualRoot;
        layoutRoot = newLayoutRoot;
        canvasGroup = newCanvasGroup;
        backdropImage = newBackdropImage;
        accentRuleImage = newAccentRuleImage;
        speakerTabImage = newSpeakerTabImage;
        formalFrameImage = newFormalFrameImage;
        auxiliaryFrameImage = newAuxiliaryFrameImage;
        leftSpeakerTabImage = newLeftSpeakerTabImage;
        rightSpeakerTabImage = newRightSpeakerTabImage;
        speakerText = newSpeakerText;
        bodyText = newBodyText;
        advanceHintText = newAdvanceHintText;
        persistentSceneHeaderText = newPersistentSceneHeaderText;
        persistentSceneHeaderGroup = newPersistentSceneHeaderGroup;
    }
}
