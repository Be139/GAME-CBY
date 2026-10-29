using Michsky.UI.Reach;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Forwards presentation focus to Reach; gameplay remains the sole owner of submission.</summary>
[DisallowMultipleComponent]
public sealed class HearthReachControl : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private ButtonManager visual;
    private bool focused;
    public void Configure(ButtonManager value) { visual = value; }
    public void SetFocused(bool value)
    {
        focused = value;
        if (visual == null || !visual.isActiveAndEnabled || !Application.isPlaying) return;
        if (value) visual.OnSelect(null);
        else visual.OnDeselect(null);
    }
    private void Start() { SetFocused(focused); }
    public void OnPointerEnter(PointerEventData data) { if (visual != null) visual.OnPointerEnter(data); }
    public void OnPointerExit(PointerEventData data) { SetFocused(focused); }
    public static void Focus(Transform target, bool value)
    {
        if (target == null) return;
        var control = target.GetComponent<HearthReachControl>();
        if (control != null) control.SetFocused(value);
    }
}
