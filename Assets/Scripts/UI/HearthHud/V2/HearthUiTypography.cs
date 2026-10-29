using System;
using TMPro;
using UnityEngine;

public enum HearthUiTextRole { Heading, Section, Body, Button, Label, Caption, Subtitle, Speaker, Display }

/// <summary>Explicit text roles on an authored UI root. No per-frame hierarchy searches or layout writes.</summary>
[DisallowMultipleComponent]
public sealed class HearthUiTypography : MonoBehaviour
{
    [Serializable]
    public struct TextBinding
    {
        public TMP_Text text;
        public HearthUiTextRole role;
    }

    [SerializeField] private HearthUiThemeProfile theme;
    [SerializeField] private TextBinding[] texts = new TextBinding[0];
    public HearthUiThemeProfile Theme { get { return theme; } }
    public TextBinding[] Texts { get { return texts; } }

    private void OnEnable() { Apply(); }

    public void Configure(HearthUiThemeProfile profile, TextBinding[] bindings)
    {
        theme = profile;
        texts = bindings;
        Apply();
    }

    [ContextMenu("Apply Typography")]
    public void Apply()
    {
        if (theme == null) return;
        foreach (var binding in texts)
        {
            if (binding.text == null) continue;
            TMP_Text text = binding.text;
            text.font = binding.role == HearthUiTextRole.Subtitle ? theme.DialogueFontAsset : theme.UiFontAsset;
            text.enableAutoSizing = false;
            text.fontSize = theme.GetFontSize(binding.role);
            text.enableWordWrapping = true;
            text.fontStyle &= ~FontStyles.Bold;
        }
    }

    // Used by legacy visual writers so showing new data cannot undo an authored role.
    public static float ResolveSize(TMP_Text text, float fallback)
    {
        var typography = text.GetComponentInParent<HearthUiTypography>(true);
        if (typography != null && typography.theme != null)
            foreach (var binding in typography.texts)
                if (binding.text == text) return typography.theme.GetFontSize(binding.role);
        return fallback;
    }
}
