using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float deceleration = 80f;
    [SerializeField] private float airControlMultiplier = 0.65f;
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private float turnAccelerationMultiplier = 1.6f;

    [Header("Jump Assistance")]
    [SerializeField] private float coyoteTime = 0.12f;
    [SerializeField] private float jumpBufferTime = 0.12f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;

    [Header("Gravity")]
    [SerializeField] private float fallGravityMultiplier = 1.5f;
    [SerializeField] private float maxFallSpeed = 25f;
    [SerializeField] private float apexThreshold = 1.25f;
    [SerializeField] private float apexGravityMultiplier = 0.65f;
    private float baseGravityScale;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        baseGravityScale = rb.gravityScale;
    }

    private void Update()
    {
        bool grounded = IsGrounded();

        if (grounded && rb.linearVelocity.y <= 0.01f)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter = Mathf.Max(
                coyoteTimeCounter - Time.deltaTime,
                0f
            );
        }

        jumpBufferCounter = Mathf.Max(
            jumpBufferCounter - Time.deltaTime,
            0f
        );

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );

            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
        }
    }

    private void FixedUpdate()
    {
        bool grounded = IsGrounded();

        // Apex hang + falling gravity
        bool nearApex =
            !grounded &&
            Mathf.Abs(rb.linearVelocity.y) < apexThreshold;

        if (nearApex)
        {
            rb.gravityScale =
                baseGravityScale * apexGravityMultiplier;
        }
        else if (rb.linearVelocity.y < 0f)
        {
            rb.gravityScale =
                baseGravityScale * fallGravityMultiplier;
        }
        else
        {
            rb.gravityScale = baseGravityScale;
        }

        Vector2 velocity = rb.linearVelocity;

        // Horizontal movement
        float targetSpeed = moveInput.x * moveSpeed;

        bool hasInput = Mathf.Abs(moveInput.x) > 0.01f;

        bool changingDirection =
            hasInput &&
            Mathf.Abs(velocity.x) > 0.01f &&
            Mathf.Sign(targetSpeed) != Mathf.Sign(velocity.x);

        float rate;

        if (!hasInput)
        {
            rate = deceleration;
        }
        else if (changingDirection)
        {
            rate = acceleration * turnAccelerationMultiplier;
        }
        else
        {
            rate = acceleration;
        }

        if (!grounded)
        {
            rate *= airControlMultiplier;
        }

        velocity.x = Mathf.MoveTowards(
            velocity.x,
            targetSpeed,
            rate * Time.fixedDeltaTime
        );

        // Terminal fall speed
        velocity.y = Mathf.Max(
            velocity.y,
            -maxFallSpeed
        );

        rb.linearVelocity = velocity;
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else if (rb.linearVelocity.y > 0f)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                rb.linearVelocity.y * jumpCutMultiplier
            );
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        ) != null;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.DrawWireSphere(
                groundCheck.position,
                groundCheckRadius
            );
        }
    }
}
