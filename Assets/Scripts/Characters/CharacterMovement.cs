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

    
    public void ApplyDodge(float directionSign, float speed)
    {
        rb.linearVelocity = new Vector2(directionSign * speed, rb.linearVelocity.y);
    }

    
    public void ApplyDirectionalDash(Vector2 direction, float speed)
    {
        rb.linearVelocity = direction * speed;
    }

    
    public void StopAllMovement()
    {
        rb.linearVelocity = Vector2.zero;
    }

    
    public void ApplyFastFall()
    {
        if (rb.linearVelocity.y > controller.stats.fastFallSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, controller.stats.fastFallSpeed);
    }

    
    public void ClampFallSpeed()
    {
        if (rb.linearVelocity.y < controller.stats.maxFallSpeed)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, controller.stats.maxFallSpeed);
    }
}