using UnityEngine;

public class FirstPersonLook : MonoBehaviour
{
    [SerializeField]
    Transform character;
    public float sensitivity = 2;
    public float smoothing = 1.5f;

    Vector2 velocity;
    Vector2 frameVelocity;


    void Reset()
    {
        // Get the character from the FirstPersonMovement in parents.
        character = GetComponentInParent<FirstPersonMovement>().transform;
    }

    void Start()
    {
        // Lock the mouse cursor to the game screen.
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Get smooth velocity.
        Vector2 mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
        Vector2 rawFrameVelocity = Vector2.Scale(mouseDelta, Vector2.one * sensitivity);
        frameVelocity = Vector2.Lerp(frameVelocity, rawFrameVelocity, 1 / smoothing);
        velocity += frameVelocity;
        velocity.y = Mathf.Clamp(velocity.y, -90, 90);

        // Rotate camera up-down and controller left-right from velocity.
        transform.localRotation = Quaternion.AngleAxis(-velocity.y, Vector3.right);
        character.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);
    }

    public void ForceLookAngles(float yaw, float pitch)
    {
        velocity = new Vector2(yaw, Mathf.Clamp(pitch, -90, 90));
        frameVelocity = Vector2.zero;

        transform.localRotation = Quaternion.AngleAxis(-velocity.y, Vector3.right);

        if (character != null)
        {
            character.localRotation = Quaternion.AngleAxis(velocity.x, Vector3.up);
        }
    }

    public void ForceLookFromCurrentTransforms()
    {
        float yaw = character != null ? NormalizeAngle(character.localEulerAngles.y) : 0f;
        float pitch = NormalizeAngle(-transform.localEulerAngles.x);
        ForceLookAngles(yaw, pitch);
    }

    static float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle > 180f)
        {
            angle -= 360f;
        }

        if (angle < -180f)
        {
            angle += 360f;
        }

        return angle;
    }
}
