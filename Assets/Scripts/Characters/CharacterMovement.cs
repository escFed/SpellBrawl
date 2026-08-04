using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    private const float CrouchHeightRatio = 0.55f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Movement Modifiers")]
    public float moveSpeedMultiplier = 1f;

    public bool IsGrounded { get; private set; }

    private PlayerController controller;
    private Rigidbody2D rb;
    private CapsuleCollider2D bodyCollider;
    private Vector2 standingColliderSize;
    private Vector2 standingColliderOffset;

    public bool IsCrouching { get; private set; }

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<CapsuleCollider2D>();

        if (bodyCollider != null)
        {
            standingColliderSize = bodyCollider.size;
            standingColliderOffset = bodyCollider.offset;
        }
    }

    private void OnDisable() => SetCrouching(false);

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

    public void SetCrouching(bool crouching)
    {
        if (bodyCollider == null || bodyCollider.direction != CapsuleDirection2D.Vertical || crouching == IsCrouching)
            return;

        if (crouching)
        {
            float crouchedHeight = Mathf.Max(standingColliderSize.x, standingColliderSize.y * CrouchHeightRatio);
            float removedHeight = standingColliderSize.y - crouchedHeight;

            bodyCollider.size = new Vector2(standingColliderSize.x, crouchedHeight);
            bodyCollider.offset = standingColliderOffset + Vector2.down * (removedHeight * 0.5f);
            IsCrouching = true;
            return;
        }

        bodyCollider.size = standingColliderSize;
        bodyCollider.offset = standingColliderOffset;
        IsCrouching = false;
    }


    public void ApplyRoll(float directionSign, float speed)
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