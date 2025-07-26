using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 6f;
    public float sprintMultiplier = 1.5f;

    [Header("Jump & Gravity")]
    public float jumpHeight = 3f;
    public float extraGravity = 2f;

    [Header("Air Control")]
    public float airBrake = 8f;
    public float airControl = 2f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.3f;
    public LayerMask groundMask;

    Rigidbody rb;
    Vector2 input;
    bool jumpQueued;
    bool isGrounded;
    Vector3 momentum;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ;   // keep upright
    }

    void Update()
    {
        input = new Vector2(Input.GetAxisRaw("Horizontal"),
                            Input.GetAxisRaw("Vertical"));
        jumpQueued |= Input.GetButtonDown("Jump");
    }

    void FixedUpdate()
    {
        // --- ground check ---
        isGrounded = Physics.CheckSphere(groundCheck.position,
                                         groundRadius, groundMask);

        // --- movement direction uses *body* forward/right (already yawed) ---
        Vector3 dir = (transform.right * input.x + transform.forward * input.y).normalized;

        float maxSpeed = Input.GetKey(KeyCode.LeftShift) && isGrounded
                         ? walkSpeed * sprintMultiplier
                         : walkSpeed;

        if (isGrounded)
        {
            Vector3 desired = dir * maxSpeed;
            rb.linearVelocity = new Vector3(desired.x, rb.linearVelocity.y, desired.z);
            momentum   = desired;
        }
        else
        {
            momentum += dir * airControl * Time.fixedDeltaTime;
            momentum  = Vector3.ClampMagnitude(momentum, maxSpeed);

            if (input.y < 0)
                momentum = Vector3.MoveTowards(momentum, Vector3.zero,
                                               airBrake * Time.fixedDeltaTime);

            rb.linearVelocity = new Vector3(momentum.x, rb.linearVelocity.y, momentum.z);
        }

        // --- jump ---
        if (jumpQueued && isGrounded)
        {
            float jumpV = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpV, rb.linearVelocity.z);
        }
        jumpQueued = false;

        // --- extra gravity ---
        rb.AddForce(Physics.gravity * (extraGravity - 1f), ForceMode.Acceleration);
    }
}
