using UnityEngine;

/// <summary>
/// Shared mechanical player/companion pose service. Household controllers
/// still decide when and where a move happens; this service only applies it.
/// </summary>
[DisallowMultipleComponent]
public sealed class HearthPlayerRigService : MonoBehaviour
{
    [System.Serializable]
    public sealed class RigBinding
    {
        public Transform root;
        public Camera viewCamera;
        public Rigidbody body;
        public FirstPersonLook look;
    }

    [SerializeField] private RigBinding human = new RigBinding();
    [SerializeField] private RigBinding companion = new RigBinding();

    public RigBinding Human { get { return human; } }
    public RigBinding Companion { get { return companion; } }

    public bool MoveHuman(Transform rootAnchor, Transform cameraAnchor = null)
    {
        return ApplyPose(human, rootAnchor, cameraAnchor);
    }

    public bool MoveCompanion(Transform rootAnchor, Transform cameraAnchor = null)
    {
        return ApplyPose(companion, rootAnchor, cameraAnchor);
    }

    public static bool ApplyPose(
        RigBinding rig,
        Transform rootAnchor,
        Transform cameraAnchor = null)
    {
        if (rig == null || rig.root == null || rootAnchor == null)
        {
            return false;
        }

        rig.root.SetPositionAndRotation(rootAnchor.position, rootAnchor.rotation);
        if (rig.viewCamera != null && cameraAnchor != null)
        {
            rig.viewCamera.transform.SetPositionAndRotation(
                cameraAnchor.position,
                cameraAnchor.rotation);
        }

        ClearVelocity(rig.body);
        if (rig.look != null)
        {
            rig.look.ForceLookFromCurrentTransforms();
        }

        return true;
    }

    public static bool ApplyPose(
        Transform root,
        Camera viewCamera,
        Rigidbody body,
        FirstPersonLook look,
        Transform rootAnchor,
        Transform cameraAnchor = null)
    {
        if (root == null || rootAnchor == null)
        {
            return false;
        }

        root.SetPositionAndRotation(rootAnchor.position, rootAnchor.rotation);
        if (viewCamera != null && cameraAnchor != null)
        {
            viewCamera.transform.SetPositionAndRotation(
                cameraAnchor.position,
                cameraAnchor.rotation);
        }

        ClearVelocity(body);
        if (look != null)
        {
            look.ForceLookFromCurrentTransforms();
        }

        return true;
    }

    public static void ClearVelocity(Rigidbody body)
    {
        if (body == null || body.isKinematic)
        {
            return;
        }

        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
    }
}
