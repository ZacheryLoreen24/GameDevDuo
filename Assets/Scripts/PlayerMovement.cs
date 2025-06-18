
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Speeds")]
    public float walkSpeed = 6f;
    public float sprintMultiplier = 1.5f;

    [Header("Jump & Gravity")]
    public float jumpHeight = 3f;
    public float extraGravity = 2f;        // multiplies Physics.gravity.y

    [Header("Air Control")]
    public float airBrake = 8f;            // slows momentum when holding S
    public float airControl = 2f;          // small steering while airborne

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.3f;
    public LayerMask groundMask;

    Rigidbody rb;
    Vector2 moveInput;
    bool jumpPressed;
    bool isGrounded;
    Vector3 momentum;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;          // keep capsule upright
    }

    void Update()
    {
        /* -------- Gather input each frame -------- */
        moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),   // A / D
            Input.GetAxisRaw("Vertical"));    // W / S

        jumpPressed = Input.GetButtonDown("Jump");
    }

    void FixedUpdate()
    {
        /* -------- Ground check -------- */
        isGrounded = Physics.CheckSphere(groundCheck.position, groundRadius, groundMask);

        /* -------- Horizontal movement -------- */
        Vector3 camForward = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 camRight   = new Vector3(transform.right.x,   0f, transform.right.z).normalized;

        Vector3 wishDir = (camRight * moveInput.x + camForward * moveInput.y).normalized;

        float speed = Input.GetKey(KeyCode.LeftShift) && isGrounded
                      ? walkSpeed * sprintMultiplier
                      : walkSpeed;

        if (isGrounded)
        {
            /* snap horizontal velocity to desired value */
            Vector3 desiredVel = wishDir * speed;
            rb.velocity = new Vector3(desiredVel.x, rb.velocity.y, desiredVel.z);
            momentum = desiredVel;          // store for airborne use
        }
        else
        {
            /* keep momentum, allow a little air steering */
            momentum += wishDir * airControl * Time.fixedDeltaTime;
            momentum.y = 0f;

            /* air-brake when holding S */
            if (moveInput.y < 0f)
                momentum = Vector3.MoveTowards(momentum, Vector3.zero, airBrake * Time.fixedDeltaTime);

            rb.velocity = new Vector3(momentum.x, rb.velocity.y, momentum.z);
        }

        /* -------- Jump -------- */
        if (jumpPressed && isGrounded)
        {
            float jumpVel = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            rb.velocity = new Vector3(rb.velocity.x, jumpVel, rb.velocity.z);
        }

        /* -------- Extra gravity for snappier falls -------- */
        rb.AddForce(Physics.gravity * (extraGravity - 1f), ForceMode.Acceleration);
    }
}