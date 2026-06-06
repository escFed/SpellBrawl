using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Movement Modifiers")]
    public float moveSpeedMultiplier = 1f;

    public bool IsGrounded { get; private set; }

    private PlayerController controller;
    private Rigidbody2D rb;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    public void ApplyHorizontalMovement()
    {
        float currentSpeed = controller.stats.moveSpeed * moveSpeedMultiplier;
        rb.linearVelocity = new Vector2(controller.MoveInput.x * currentSpeed, rb.linearVelocity.y);

        controller.Combat.CheckAndFlip(controller.MoveInput.x);
    }

    public void StopHorizontalMovement() => rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    public void ApplyJumpForce() => rb.linearVelocity = new Vector2(rb.linearVelocity.x, controller.stats.jumpForce);

    // Instantly reach fast-fall terminal velocity
    public void ApplyFastFall()
    {
        if (rb.linearVelocity.y > controller.stats.fastFallSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, controller.stats.fastFallSpeed);
    }

    // Cap normal fall speed every physics frame
    public void ClampFallSpeed()
    {
        if (rb.linearVelocity.y < controller.stats.maxFallSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, controller.stats.maxFallSpeed);
    }
}