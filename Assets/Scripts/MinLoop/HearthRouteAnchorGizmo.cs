using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class HearthRouteAnchorGizmo : MonoBehaviour
{
    [SerializeField] private string labelOverride;
    [SerializeField] private bool drawAlways = true;
    [SerializeField] private Color bodyColor = new Color(0.2f, 0.85f, 1f, 0.8f);
    [SerializeField] private Color forwardColor = new Color(1f, 0.85f, 0.2f, 0.9f);
    [SerializeField] private float bodyHeight = 1.6f;
    [SerializeField] private float bodyRadius = 0.22f;
    [SerializeField] private float arrowLength = 0.7f;
    [SerializeField] private float labelHeight = 1.9f;

    public void Configure(string newLabel, Color newBodyColor, Color newForwardColor)
    {
        labelOverride = newLabel;
        bodyColor = newBodyColor;
        forwardColor = newForwardColor;
    }

    private void OnValidate()
    {
        bodyHeight = Mathf.Max(0.1f, bodyHeight);
        bodyRadius = Mathf.Max(0.01f, bodyRadius);
        arrowLength = Mathf.Max(0.05f, arrowLength);
        labelHeight = Mathf.Max(0.1f, labelHeight);
    }

    private void OnDrawGizmos()
    {
        if (drawAlways)
        {
            DrawPreview();
        }
    }

    private void OnDrawGizmosSelected()
    {
        DrawPreview();
    }

    private void DrawPreview()
    {
        Vector3 root = transform.position;
        Vector3 top = root + Vector3.up * bodyHeight;
        Vector3 mid = root + Vector3.up * (bodyHeight * 0.5f);

        Gizmos.color = bodyColor;
        Gizmos.DrawWireSphere(root + Vector3.up * (bodyRadius * 0.5f), bodyRadius);
        Gizmos.DrawWireSphere(top - Vector3.up * bodyRadius, bodyRadius);
        Gizmos.DrawLine(root + transform.right * bodyRadius, top + transform.right * bodyRadius - Vector3.up * bodyRadius);
        Gizmos.DrawLine(root - transform.right * bodyRadius, top - transform.right * bodyRadius - Vector3.up * bodyRadius);
        Gizmos.DrawLine(root + transform.forward * bodyRadius, top + transform.forward * bodyRadius - Vector3.up * bodyRadius);
        Gizmos.DrawLine(root - transform.forward * bodyRadius, top - transform.forward * bodyRadius - Vector3.up * bodyRadius);

        Vector3 arrowStart = mid;
        Vector3 arrowEnd = arrowStart + transform.forward * arrowLength;
        Gizmos.color = forwardColor;
        Gizmos.DrawLine(arrowStart, arrowEnd);
        Vector3 left = Quaternion.Euler(0f, 150f, 0f) * transform.forward;
        Vector3 right = Quaternion.Euler(0f, -150f, 0f) * transform.forward;
        Gizmos.DrawLine(arrowEnd, arrowEnd + left * (arrowLength * 0.28f));
        Gizmos.DrawLine(arrowEnd, arrowEnd + right * (arrowLength * 0.28f));

#if UNITY_EDITOR
        Handles.color = forwardColor;
        Handles.Label(root + Vector3.up * labelHeight, string.IsNullOrEmpty(labelOverride) ? name : labelOverride);
#endif
    }
}
