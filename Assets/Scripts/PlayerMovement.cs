using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    public float speed = 12f;
    public float sprintMultiplier = 1.5f;
    public float gravity = -9.81f * 2;
    public float jumpHeight = 3f;
    public float airBrake = 8f; // rate at which backward input slows momentum
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    Vector3 velocity;
    bool isGrounded;
    private bool isSprinting;
    private Vector3 horizontalMomentum;
    

    // Update is called once per frame
    private float groundCheckOffset;

    void Start()
    {
        groundCheckOffset = Mathf.Abs(groundCheck.localPosition.y);
        isSprinting = false;
        horizontalMomentum = Vector3.zero;
    }

    void Update()
    {
        Vector3 checkPos = transform.position + Vector3.down * groundCheckOffset;
        isGrounded = Physics.CheckSphere(checkPos, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // use only the player's yaw rotation for movement so looking straight up or down keeps forward movement
        float yaw = transform.eulerAngles.y;
        Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);
        Vector3 right = yawRotation * Vector3.right;
        Vector3 forward = yawRotation * Vector3.forward;

        Vector3 move = right * x + forward * z;

        if (isGrounded)
        {
            isSprinting = Input.GetKey(KeyCode.LeftShift);
        }

        if (isGrounded)
        {
            float groundSpeed = isSprinting ? speed * sprintMultiplier : speed;
            Vector3 desiredVelocity = move * groundSpeed;
            controller.Move(desiredVelocity * Time.deltaTime);
            horizontalMomentum = desiredVelocity;
        }
        else
        {
            if (z < 0f)
            {
                // gradually reduce horizontal momentum when pressing back in the air
                float reduce = airBrake * Time.deltaTime;
                float m = horizontalMomentum.magnitude;
                m = Mathf.Max(0f, m - reduce);
                horizontalMomentum = horizontalMomentum.normalized * m;
            }

            controller.Move(horizontalMomentum * Time.deltaTime);
        }

        //check if the player is on the ground so he can jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            //the equation for jumping
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}