#if UNITY_EDITOR
using System;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static partial class HearthProductionUiTools
{
    private const string ReachFolder = "Assets/UI/HEARTH/V2/ThirdParty/Reach/";

    // Historical migration entry: production appearance is edited in Prefab Mode.
    public static void ApplyReachStyleAndTypeScale()
    {
        var theme = AssetDatabase.LoadAssetAtPath<HearthUiThemeProfile>(ThemeProfilePath);
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(ReachFolder + "Rajdhani-Medium SDF.asset");
        var frame = AssetDatabase.LoadAssetAtPath<Sprite>(ReachFolder + "Cross Frame.png");
        if (theme == null || font == null || frame == null) throw new InvalidOperationException("Reach assets or theme missing.");
        var so = new SerializedObject(theme);
        so.FindProperty("primaryFontAsset").objectReferenceValue = font;
        so.FindProperty("companionIdentityHeadingFontSize").floatValue = 20;
        so.FindProperty("companionIdentityValueFontSize").floatValue = 28;
        so.FindProperty("companionTaskHeadingFontSize").floatValue = 20;
        so.FindProperty("companionTaskBodyFontSize").floatValue = 24;
        so.FindProperty("terminalDialogueSpeakerFontSize").floatValue = 22;
        so.FindProperty("terminalDialogueBodyFontSize").floatValue = 28;
        so.FindProperty("terminalDialogueAdvanceFontSize").floatValue = 18;
        so.FindProperty("interactionPromptFontSize").floatValue = 24;
        so.ApplyModifiedPropertiesWithoutUndo();
        foreach (string path in CanonicalPrefabs)
        {
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            try
            {
                var typography = root.GetComponent<HearthUiTypography>();
                if (typography == null) typography = root.AddComponent<HearthUiTypography>();
                var existing = typography.Texts.Where(b => b.text != null).ToDictionary(b => b.text, b => b.role);
                typography.Configure(theme, root.GetComponentsInChildren<TMP_Text>(true).Select(t => new HearthUiTypography.TextBinding
                {
                    text = t, role = existing.ContainsKey(t) ? existing[t] : SeedTextRole(t)
                }).ToArray());
                // These labels had ambiguous legacy names. Keep their semantic role explicit.
                var bindings = typography.Texts;
                for (int i = 0; i < bindings.Length; i++)
                {
                    TMP_Text t = bindings[i].text;
                    string n = t.name;
                    if (n == "LocationTitleText" || n == "V2_MenuLocationTitle") bindings[i].role = HearthUiTextRole.Label;
                    if (n == "LocationValueText" || n == "LocationGlowText") bindings[i].role = HearthUiTextRole.Body;
                    if (n.Contains("YES___") || n.Contains("NO___") || n.Contains("EXIT_GAME") && !n.Contains("035")) bindings[i].role = HearthUiTextRole.Button;
                    if (n.Contains("FieldUnitTitle")) bindings[i].role = HearthUiTextRole.Speaker;
                }
                typography.Apply();
                foreach (var t in root.GetComponentsInChildren<TMP_Text>(true).Where(t => t.name == "Body" && t.transform.parent.name == "HouseholdIntroduction"))
                {
                    // Imported hard line breaks were laid out for a different font and made records overflow.
                    t.text = System.Text.RegularExpressions.Regex.Replace(t.text, @"(?<!\n)\n[ ]{4,}(?=\S)", " ");
                    t.lineSpacing = -2;
                    t.paragraphSpacing = 0;
                    t.rectTransform.anchoredPosition = new Vector2(24, -66);
                    t.rectTransform.sizeDelta = new Vector2(((RectTransform)t.transform.parent).sizeDelta.x - 48, 400);
                    var panelRect = (RectTransform)t.transform.parent;
                    panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, -246);
                    panelRect.sizeDelta = new Vector2(panelRect.sizeDelta.x, 476);
                }
                foreach (TMP_Text t in root.GetComponentsInChildren<TMP_Text>(true))
                {
                    RectTransform rect = t.rectTransform;
                    float minimumLine = Mathf.Ceil(t.fontSize * 1.35f);
                    if (rect.anchorMin.y == rect.anchorMax.y && rect.sizeDelta.y > 0 && rect.sizeDelta.y < minimumLine)
                        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minimumLine);
                    if (t.name.Contains("CLASSROOM_OPEN_DAY___THE_DAY_AFTER"))
                        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1200);
                    if (t.color.r <= t.color.g * 1.15f)
                        t.color = new Color(.84f, .92f, .97f, Mathf.Max(.9f, t.color.a));
                }

                foreach (Image img in root.GetComponentsInChildren<Image>(true))
                {
                    string sprite = img.sprite == null ? "" : img.sprite.name;
                    if (sprite == "Cross Frame" || sprite.StartsWith("HUD_") && (sprite.Contains("Frame") || sprite.Contains("frame")))
                    {
                        img.sprite = frame;
                        img.type = Image.Type.Sliced;
                        img.pixelsPerUnitMultiplier = 16;
                        img.color = new Color(.12f, .66f, .82f, .55f);
                    }
                    if (img.sprite != null && AssetDatabase.GetAssetPath(img.sprite).Contains("Kenney"))
                    {
                        string asset = AssetDatabase.GetAssetPath(img.sprite);
                        if (asset.Contains("Kenney")) { img.sprite = null; img.type = Image.Type.Simple; }
                    }
                    // A Filled Image with no sprite renders a full quad regardless of fillAmount.
                    if (img.name == "HoldProgressFill")
                    {
                        img.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
                        img.type = Image.Type.Filled;
                        img.fillMethod = Image.FillMethod.Horizontal;
                        img.fillOrigin = 0;
                    }
                    if (img.name == "PanelBackdrop" || img.name == "ScreenSurface")
                        img.color = new Color(.025f, .045f, .065f, .95f);
                }
                foreach (var panel in root.GetComponentsInChildren<RectTransform>(true).Where(t => t.name == "DispositionPanel"))
                {
                    var page = panel.GetComponentInParent<HearthHudPage>(true);
                    if (page == null) continue;
                    panel.anchoredPosition = new Vector2(330, -230);
                    panel.sizeDelta = new Vector2(1260, 550);
                    var pageCanvas = page.GetComponentsInChildren<Canvas>(true).FirstOrDefault();
                    if (pageCanvas != null) { pageCanvas.overrideSorting = true; pageCanvas.sortingOrder = 30; }
                }
                foreach (Canvas canvas in root.GetComponentsInChildren<Canvas>(true))
                    AssignCanvasBand(canvas);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        AssetDatabase.SaveAssets();
        Debug.Log("[Production UI] Reach visual primitives and explicit typography roles applied to all 10 canonical prefabs.");
    }

    private static void AssignCanvasBand(Canvas canvas)
    {
        int order = canvas.sortingOrder;
        bool independent = canvas.overrideSorting;
        if (!Enum.IsDefined(typeof(HearthUiCanvasBand), order)) return;
        var policy = canvas.GetComponent<HearthUiCanvasOrder>();
        if (policy == null) policy = canvas.gameObject.AddComponent<HearthUiCanvasOrder>();
        policy.Configure((HearthUiCanvasBand)order, independent);
    }

    // Historical migration entry: production appearance is edited in Prefab Mode.
    public static void BindSceneCanvasBands()
    {
        foreach (Canvas canvas in UnityEngine.Object.FindObjectsOfType<Canvas>(true).Where(c => c.gameObject.scene.IsValid()))
        {
            // Canonical instances inherit their policy. Scene-only overlays get explicit ownership here.
            if (!Enum.IsDefined(typeof(HearthUiCanvasBand), canvas.sortingOrder)) continue;
            int order = canvas.sortingOrder;
            bool independent = canvas.overrideSorting;
            var policy = canvas.GetComponent<HearthUiCanvasOrder>();
            if (policy != null && (int)policy.Band == order) continue;
            Undo.RecordObject(canvas, "Bind UI Canvas Band");
            if (policy == null) policy = Undo.AddComponent<HearthUiCanvasOrder>(canvas.gameObject);
            else Undo.RecordObject(policy, "Bind UI Canvas Band");
            policy.Configure((HearthUiCanvasBand)order, independent);
            PrefabUtility.RecordPrefabInstancePropertyModifications(policy);
            EditorUtility.SetDirty(policy);
        }
        var theme = AssetDatabase.LoadAssetAtPath<HearthUiThemeProfile>(ThemeProfilePath);
        foreach (Canvas canvas in UnityEngine.Object.FindObjectsOfType<Canvas>(true).Where(c => c.gameObject.scene.IsValid() && c.isRootCanvas))
        {
            var unbound = canvas.GetComponentsInChildren<TMP_Text>(true).Where(t => t.GetComponentInParent<HearthUiTypography>(true) == null).ToArray();
            if (unbound.Length == 0) continue;
            var typography = Undo.AddComponent<HearthUiTypography>(canvas.gameObject);
            foreach (var text in unbound) Undo.RecordObject(text, "Bind Scene Typography");
            typography.Configure(theme, unbound.Select(t => new HearthUiTypography.TextBinding {text = t,
                role = t.name == "MessageTranscript" || t.name == "Line" ? HearthUiTextRole.Subtitle : SeedTextRole(t)}).ToArray());
            EditorUtility.SetDirty(typography);
        }
    }

    // Initial migration only; subsequent applications preserve the Inspector's explicit role assignments.
    private static HearthUiTextRole SeedTextRole(TMP_Text text)
    {
        string n = text.name.ToLowerInvariant();
        string content = text.text.ToUpperInvariant();
        string parent = text.transform.parent == null ? "" : text.transform.parent.name.ToLowerInvariant();
        bool surface = text.GetComponentInParent<HearthDialogueSurface>(true) != null;
        if ((n == "body" || n == "bodytext") && (surface || text.GetComponentInParent<HearthSubtitleViewBindings>(true) != null)) return HearthUiTextRole.Subtitle;
        if (n.Contains("speaker") || (surface && (n == "title" || n.Contains("name")))) return HearthUiTextRole.Speaker;
        if (n.Contains("hint") || n.Contains("footer") || n.Contains("kicker") || n.Contains("recommended") || n.Contains("timestamp") || n.Contains("detail") || n.Contains("protocol") || n.Contains("last_sync")) return HearthUiTextRole.Caption;
        if (n == "key" || n == "text_002_a" || n == "text_005_b" || n.Contains("identityvalue") || n == "residentid" || n == "floor") return HearthUiTextRole.Section;
        if (n.Contains("heading") && !n.Contains("identity") && !n.Contains("task")) return HearthUiTextRole.Heading;
        if (n == "assignmenttitle" || n == "archivetitle" || n.Contains("warningheading")) return HearthUiTextRole.Heading;
        if (n.Contains("body") || n.Contains("rows")) return HearthUiTextRole.Body;
        if (n.Contains("title")) return HearthUiTextRole.Section;
        if (parent.Contains("choice") || parent.Contains("button") || parent.Contains("tab") || n.Contains("confirm") || n.Contains("continue") || n.Contains("answer") || n.Contains("recallaction")) return HearthUiTextRole.Button;
        if (n.Contains("identityheading") || n.Contains("taskheading") || n.Contains("locationtitle") || n.Contains("label") || n == "status" || n == "value") return HearthUiTextRole.Label;
        if (text.fontSize >= 38) return HearthUiTextRole.Display;
        if (text.fontSize >= 30) return HearthUiTextRole.Heading;
        if (text.fontSize >= 25) return HearthUiTextRole.Section;
        if (content.Length > 65) return HearthUiTextRole.Body;
        return text.fontSize <= 17 ? HearthUiTextRole.Caption : HearthUiTextRole.Label;
    }
}
#endif
