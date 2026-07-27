# HEARTH Generated UI Parts

This folder contains transparent PNG components extracted from the approved
human HUD, companion HUD, and doorway terminal visual direction.

The library currently contains 25 production parts. Identity labels, resident
names, task copy, dialogue copy, location text, key labels, and translucent
panel fills are intentionally not separate bitmap files. They must remain TMP
text or tintable Unity `Image` components so their content, scale, color, and
alpha stay editable.

## Rendering rule

- Sprites contain only cyan technical borders and decorative lines.
- Text must use TextMeshPro in Unity.
- Blue-gray panel backing must use a separate Unity `Image`.
- Selected and unselected colors must be applied through Unity tint/alpha.
- Do not bake button labels, resident names, task text, or trust values into
  these sprites.

Recommended backing colors:

- Selected: `#8FDDF0` at approximately 22% alpha.
- Unselected: `#708899` at approximately 12% alpha.
- Disabled: `#60707C` at approximately 7% alpha.

## Common

- `Common/HUD_Common_ButtonFrame_9Slice.png`
  - Tab menu rows, terminal navigation, and primary action buttons.
- `Common/HUD_Common_PanelFrame_9Slice.png`
  - Task, Field Unit, and general information panels.
- `Common/HUD_Common_DialogueFrame.png`
  - Shared human/companion dialogue panel.
- `Common/HUD_Common_SpeakerTabFrame_9Slice.png`
  - Reusable speaker-name tab placed above the dialogue panel.
- `Common/HUD_Common_KeycapFrame_9Slice.png`
  - Individual keyboard keycaps such as `E`, `TAB`, and `SPACE`.
- `Common/HUD_Common_HeaderUnderline.png`
  - Identity and section-title underline decoration.

## Companion

- `Companion/HUD_Companion_FullscreenFrame.png`
  - Fixed 16:9 companion playback frame. Use `Image.Type = Simple`.

## Terminal

- `Terminal/HUD_Terminal_FullscreenFrame.png`
  - Fixed 16:9 doorway terminal boundary. Use `Image.Type = Simple`.
- `Terminal/HUD_Terminal_PortraitFrame_9Slice.png`
  - Resident photo slot for SON, DAD, or MOM.
- `Terminal/HUD_Terminal_InfoPanelFrame_9Slice.png`
  - Household introduction and Field Unit information panels.

## Interaction

- `Interaction/HUD_Interaction_TapFrame_9Slice.png`
  - Single-press `E` interaction prompt.
- `Interaction/HUD_Interaction_HoldFrame.png`
  - Hold interaction prompt with an empty visual progress track.
- `Interaction/HUD_Interaction_HoldProgressFill.png`
  - Separate progress fill. Resize its `RectTransform` from 0% to 100%.
- `Interaction/HUD_Interaction_GazePromptFrame_9Slice.png`
  - Gaze-target instruction such as facing a resident.
- `Interaction/HUD_Interaction_ChoiceHintFrame_9Slice.png`
  - Horizontal or vertical choice-control reminder.

## Feedback

- `Feedback/HUD_Feedback_FieldUnitToastFrame_9Slice.png`
  - Short Field Unit system notice that is not spoken dialogue.
- `Feedback/HUD_Feedback_TrustToastFrame_9Slice.png`
  - Shared frame for positive or negative trust feedback.
- `Feedback/HUD_Feedback_PleaseWaitFrame_9Slice.png`
  - Disabled input and queued-action state.
- `Feedback/HUD_Feedback_WarningModalFrame_9Slice.png`
  - General warning or confirmation modal.

## Inspection

- `Inspection/HUD_Inspection_DiagnosticViewportFrame_9Slice.png`
  - Physical companion-unit diagnostic camera viewport.

## Finale

- `Finale/HUD_Finale_PhotoFrame_9Slice.png`
  - Electronic family-photo inspection frame.
- `Finale/HUD_Finale_ShutdownModalFrame_9Slice.png`
  - Standard high-trust shutdown confirmation.
- `Finale/HUD_Finale_VirusPopup_Phase01_9Slice.png`
  - Low-trust shutdown popup, cyan phase.
- `Finale/HUD_Finale_VirusPopup_Phase02_9Slice.png`
  - Low-trust shutdown popup, amber phase.
- `Finale/HUD_Finale_VirusPopup_Phase03_9Slice.png`
  - Low-trust shutdown popup, red phase.

## Full-screen reference mockups

The full 1920x1080 white-background review images are stored outside `Assets`:

`UI参考资料/HEARTH_UI_Fullscreen_Mockups/`

They cover:

1. Lobby task terminal.
2. Human Tab menu.
3. Physical companion-unit inspection.
4. Home-unit terminal.
5. Photo archive.
6. Final A/B choice.
7. Standard shutdown confirmation.
8. Low-trust popup challenge.

## Unity import

For reusable parts:

1. Set `Texture Type` to `Sprite (2D and UI)`.
2. Set `Sprite Mode` to `Single`.
3. Enable `Alpha Is Transparency`.
4. Use `Mesh Type = Full Rect`.
5. Set the sprite border in Sprite Editor, then use `Image.Type = Sliced`.

The two fullscreen frames should keep their original aspect ratio and should
not be 9-sliced.

The chroma-key source renders are stored outside `Assets` in:

`UI参考资料/HEARTH_UI_Component_Sources/`

These sprites are not yet assigned to existing HUD or terminal prefabs.
