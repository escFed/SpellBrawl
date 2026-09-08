using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    private float CrouchHeightRatio = 0.55f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Movement Modifiers")]
    public float moveSpeedMultiplier = 1f;

    public bool IsGrounded { get; private set; }
    public bool HasStableGroundContact => IsGrounded && rb != null && rb.linearVelocity.y <= 0.01f;
    public bool IsFastFalling { get; private set; }
    public JumpType CurrentJumpType { get; private set; }

    private PlayerController controller;
    private Rigidbody2D rb;
    private readonly KnockbackMotion knockbackMotion = new KnockbackMotion();
    public Vector2 KnockbackVelocity => knockbackMotion.Velocity;
    public Vector2 OrdinaryVelocity => rb.linearVelocity - KnockbackVelocity;

    public void ApplyKnockback(Vector2 velocity)
    {
        ResetAirMovementState();
        controller.CancelGroundJumpAvailability();
        knockbackMotion.Launch(rb, velocity);
    }

    public void ResetKnockback() => knockbackMotion.Clear(rb);

    private void SetOrdinaryVelocity(Vector2 velocity) => knockbackMotion.SetOrdinaryVelocity(rb, velocity);

    public void StopVerticalMovement() => SetOrdinaryVelocity(new Vector2(OrdinaryVelocity.x, 0f));
    private CapsuleCollider2D bodyCollider;
    private Vector2 standingColliderSize;
    private Vector2 standingColliderOffset;
    private float activeJumpGravityMultiplier = 1f;
    private bool fastFallInputArmed = true;

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

    private void OnDisable()
    {
        ResetKnockback();
        SetCrouching(false);
        ResetAirMovementState();
    }

    private void Update()
    {
        RefreshGroundedState();
    }

    public void RefreshGroundedState()
    {
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (HasStableGroundContact)
            ResetAirMovementState();
    }

    // Called explicitly before the current state's physics update, after collision resolution.
    public void StepPhysics()
    {
        knockbackMotion.Step(rb, Time.fixedDeltaTime, controller.stats.knockbackAirDeceleration,
            controller.stats.knockbackGroundDeceleration);
        if (rb.bodyType != RigidbodyType2D.Dynamic)
        {
            activeJumpGravityMultiplier = 1f;
            return;
        }

        if (activeJumpGravityMultiplier == 1f)
            return;

        if (IsGrounded && rb.linearVelocity.y <= 0.01f)
        {
            activeJumpGravityMultiplier = 1f;
            return;
        }

        Vector2 extraGravity = Physics2D.gravity * rb.gravityScale * (activeJumpGravityMultiplier - 1f);
        rb.linearVelocity += extraGravity * Time.fixedDeltaTime;
    }

    public void ApplyHorizontalMovement()
    {
        float currentSpeed = controller.stats.moveSpeed * moveSpeedMultiplier;
        SetOrdinaryVelocity(new Vector2(controller.MoveInput.x * currentSpeed, OrdinaryVelocity.y));

        controller.Combat.CheckAndFlip(controller.MoveInput.x);
    }

    public void StopHorizontalMovement() => SetOrdinaryVelocity(new Vector2(0f, OrdinaryVelocity.y));
    public void ApplyJumpForce()
    {
        float speedMultiplier = Mathf.Max(0.01f, controller.stats.jumpSpeedMultiplier);
        activeJumpGravityMultiplier = speedMultiplier * speedMultiplier;
        IsFastFalling = false;
        CurrentJumpType = JumpType.Full;
        fastFallInputArmed = controller.MoveInput.y >= -controller.stats.tiltThreshold;
        SetOrdinaryVelocity(new Vector2(OrdinaryVelocity.x, controller.stats.jumpForce * speedMultiplier));
    }

    public bool TryApplyShortHop()
    {
        if (CurrentJumpType != JumpType.Full || rb.linearVelocity.y <= 0f)
            return false;

        float velocityMultiplier = Mathf.Clamp01(controller.stats.shortHopVelocityMultiplier);
        SetOrdinaryVelocity(new Vector2(OrdinaryVelocity.x, OrdinaryVelocity.y * velocityMultiplier));
        CurrentJumpType = JumpType.Short;
        return true;
    }

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
        SetOrdinaryVelocity(new Vector2(directionSign * speed, OrdinaryVelocity.y));
    }


    public void ApplyDirectionalDash(Vector2 direction, float speed)
    {
        SetOrdinaryVelocity(direction * speed);
    }


    public void StopAllMovement()
    {
        SetOrdinaryVelocity(Vector2.zero);
    }


    public bool TryStartFastFall(float verticalInput)
    {
        float inputThreshold = Mathf.Clamp01(controller.stats.tiltThreshold);
        bool isPressingDown = verticalInput < -inputThreshold;

        if (!isPressingDown)
        {
            fastFallInputArmed = true;
            return false;
        }

        if (IsGrounded || IsFastFalling || !fastFallInputArmed)
            return false;

        if (rb.linearVelocity.y >= 0f)
        {
            fastFallInputArmed = false;
            return false;
        }

        fastFallInputArmed = false;
        ApplyFastFall();
        return true;
    }

    private void ApplyFastFall()
    {
        IsFastFalling = true;
        float fallSpeedLimit = GetFallSpeedLimit();

        if (OrdinaryVelocity.y > fallSpeedLimit)
            SetOrdinaryVelocity(new Vector2(OrdinaryVelocity.x, fallSpeedLimit));
    }


    public void ClampFallSpeed()
    {
        float fallSpeedLimit = GetFallSpeedLimit();

        if (OrdinaryVelocity.y < fallSpeedLimit)
            SetOrdinaryVelocity(new Vector2(OrdinaryVelocity.x, fallSpeedLimit));
    }

    private float GetFallSpeedLimit()
    {
        return IsFastFalling ? Mathf.Min(controller.stats.fastFallSpeed, controller.stats.maxFallSpeed): controller.stats.maxFallSpeed;
    }

    private void ResetAirMovementState()
    {
        activeJumpGravityMultiplier = 1f;
        IsFastFalling = false;
        CurrentJumpType = JumpType.None;
        fastFallInputArmed = controller == null || controller.stats == null || controller.ActiveInput == null || controller.MoveInput.y >= -controller.stats.tiltThreshold;
    }
}
