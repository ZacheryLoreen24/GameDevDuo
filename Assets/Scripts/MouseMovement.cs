
using UnityEngine;

public class MouseMovement : MonoBehaviour
{
    public Transform playerBody;
    public float sensitivity = 100f;

    float pitch = 0f;

    void Start() => Cursor.lockState = CursorLockMode.Locked;

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        /* vertical look (pitch) */
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        /* horizontal look (yaw) */
        playerBody.Rotate(Vector3.up * mouseX);
    }
}