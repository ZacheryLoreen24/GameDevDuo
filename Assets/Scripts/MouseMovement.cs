using UnityEngine;

// REQUIREMENTS:
// • This component lives on the same GameObject that has the Rigidbody.
// • A child object named "CameraPivot" holds the Camera (or is the Camera itself).
// • Rigidbody: freeze Rotation X & Z, leave Y free.

[RequireComponent(typeof(Rigidbody))]
public class MouseLook : MonoBehaviour
{
    public Transform pitchPivot;    // drag the Camera or empty pivot here
    public float sensitivity = 200f;

    Rigidbody rb;
    float pitch;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()              // physics step = best place for MoveRotation
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime;

        // --- YAW (left / right) ---
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, mouseX, 0f));

        // --- PITCH (up / down) ---
        pitch = Mathf.Clamp(pitch - mouseY, -90f, 90f);
        pitchPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
