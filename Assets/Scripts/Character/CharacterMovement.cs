using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    private float CrouchHeightRatio = 0.55f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("Wall Check")]
    [SerializeField] private LayerMask wallJumpLayer;
    [SerializeField, Min(0f)] private float wallCheckDistance = 0.08f;
    [SerializeField, Range(0f, 1f)] private float minimumWallNormalX = 0.7f;

    [Header("Movement Modifiers")]
    public float moveSpeedMultiplier = 1f;

    public bool IsGrounded { get; private set; }
    public bool HasStableGroundContact => IsGrounded && rb != null && rb.linearVelocity.y <= 0.01f;
    public bool IsFastFalling { get; private set; }
    public JumpType CurrentJumpType { get; private set; }
    public bool IsTouchingWall => WallSide != 0 && WallCollider != null;
    public int WallSide { get; private set; }
    public Collider2D WallCollider { get; private set; }
    public bool HasWallJumpControlLock => wallJumpControlLockRemaining > 0f;

    private CharacterCoordinator controller;
    private Rigidbody2D rb;
    private readonly KnockbackMotion knockbackMotion = new KnockbackMotion();
    private readonly Dictionary<Component, float> externalHorizontalPushes = new Dictionary<Component, float>();
    public Vector2 KnockbackVelocity => knockbackMotion.Velocity;
    public Vector2 OrdinaryVelocity => rb.linearVelocity - KnockbackVelocity;

    public void ApplyKnockback(Vector2 velocity, float airDecelerationMultiplier = 1f)
    {
        ResetAirMovementState();
        controller.CancelGroundJumpAvailability();
        knockbackMotion.Launch(rb, velocity, airDecelerationMultiplier);
    }

    public void SetExternalHorizontalPush(Component source, float speed)
    {
        if (source != null)
            externalHorizontalPushes[source] = speed;
    }

    public void ClearExternalHorizontalPush(Component source)
    {
        if (source != null)
            externalHorizontalPushes.Remove(source);
    }

    // Runs after the current state writes its movement so the current cannot be erased by input.
    public void ApplyExternalHorizontalPushes()
    {
        if (rb == null || rb.bodyType != RigidbodyType2D.Dynamic || !rb.simulated ||
            externalHorizontalPushes.Count == 0 || controller.Health == null ||
            controller.Health.Phase != RespawnPhase.Active || controller.IsIntangible ||
            controller.IsParrying || (controller.Shield != null && controller.Shield.IsActive))
            return;

        float totalPush = 0f;
        float maximumPush = 0f;
        foreach (KeyValuePair<Component, float> entry in externalHorizontalPushes)
        {
            if (entry.Key == null)
                continue;
            totalPush += entry.Value;
            maximumPush = Mathf.Max(maximumPush, Mathf.Abs(entry.Value));
        }

        totalPush = Mathf.Clamp(totalPush, -maximumPush, maximumPush);
        if (Mathf.Abs(totalPush) < 0.001f)
            return;

        Vector2 velocity = rb.linearVelocity;
        float speedLimit = Mathf.Max(Mathf.Abs(velocity.x), maximumPush);
        velocity.x = Mathf.Clamp(velocity.x + totalPush, -speedLimit, speedLimit);
        rb.linearVelocity = velocity;
    }

    public void ResetKnockback() => knockbackMotion.Clear(rb);

    private void SetOrdinaryVelocity(Vector2 velocity) => knockbackMotion.SetOrdinaryVelocity(rb, velocity);

    public void StopVerticalMovement() => SetOrdinaryVelocity(new Vector2(OrdinaryVelocity.x, 0f));
    private CapsuleCollider2D bodyCollider;
    private Vector2 standingColliderSize;
    private Vector2 standingColliderOffset;
    private float activeJumpGravityMultiplier = 1f;
    private float levitationGravityMultiplier = 1f;
    private float aerialSuspensionRemaining;
    private float aerialSuspensionGravityMultiplier = 1f;
    private bool fastFallInputArmed = true;
    private float wallJumpControlLockRemaining;
    private ContactFilter2D wallContactFilter;
    private readonly RaycastHit2D[] wallCastResults = new RaycastHit2D[8];

    public bool IsCrouching { get; private set; }
    public float AerialSuspensionRemaining => aerialSuspensionRemaining;

    private void Awake()
    {
        controller = GetComponent<CharacterCoordinator>();
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<CapsuleCollider2D>();
        ConfigureWallContactFilter();

        if (bodyCollider != null)
        {
            standingColliderSize = bodyCollider.size;
            standingColliderOffset = bodyCollider.offset;
        }
    }

    private void OnDisable()
    {
        externalHorizontalPushes.Clear();
        ResetKnockback();
        SetCrouching(false);
        ResetAirMovementState();
        ClearWallContact();
    }

    private void Update()
    {
        RefreshGroundedState();
    }

    public void RefreshGroundedState()
    {
        bool wasGrounded = IsGrounded;
        IsGrounded = groundCheck != null &&
            Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        RefreshWallContactState();

        if (HasStableGroundContact)
        {
            if (!wasGrounded)
                controller.Combat.ResetAerialCombo();
            ResetAirMovementState();
        }
    }

    // Called explicitly before the current state's physics update, after collision resolution.
    public void StepPhysics()
    {
        knockbackMotion.Step(rb, Time.fixedDeltaTime, controller.stats.knockbackAirDeceleration,
            controller.stats.knockbackGroundDeceleration);
        wallJumpControlLockRemaining = Mathf.Max(0f,
            wallJumpControlLockRemaining - Time.fixedDeltaTime);

        if (rb.bodyType != RigidbodyType2D.Dynamic)
        {
            ResetAirMovementState();
            return;
        }

        if (IsGrounded && rb.linearVelocity.y <= 0.01f)
        {
            ResetAirMovementState();
            return;
        }

        float gravityMultiplier = activeJumpGravityMultiplier * levitationGravityMultiplier;
        if (aerialSuspensionRemaining > 0f)
        {
            gravityMultiplier *= aerialSuspensionGravityMultiplier;
            aerialSuspensionRemaining = Mathf.Max(0f, aerialSuspensionRemaining - Time.fixedDeltaTime);
            if (aerialSuspensionRemaining <= 0f)
                aerialSuspensionGravityMultiplier = 1f;
        }

        if (!Mathf.Approximately(gravityMultiplier, 1f))
        {
            Vector2 extraGravity = Physics2D.gravity * rb.gravityScale * (gravityMultiplier - 1f);
            rb.linearVelocity += extraGravity * Time.fixedDeltaTime;
        }
    }

    public void BeginAerialSuspension(AerialSuspension suspension)
    {
        if (!suspension.IsActive || rb == null || rb.bodyType != RigidbodyType2D.Dynamic)
            return;

        aerialSuspensionRemaining = Mathf.Max(aerialSuspensionRemaining, suspension.Duration);
        aerialSuspensionGravityMultiplier = Mathf.Min(aerialSuspensionGravityMultiplier, suspension.GravityMultiplier);

        if (suspension.MaximumDownwardSpeed > 0f && OrdinaryVelocity.y < -suspension.MaximumDownwardSpeed)
            SetOrdinaryVelocity(new Vector2(OrdinaryVelocity.x, -suspension.MaximumDownwardSpeed));
    }

    public void SetLevitationGravityMultiplier(float multiplier)
    {
        levitationGravityMultiplier = Mathf.Clamp01(multiplier);
    }

    public void ApplyLevitationPulse(float upwardSpeed)
    {
        if (rb == null || rb.bodyType != RigidbodyType2D.Dynamic)
            return;

        SetOrdinaryVelocity(OrdinaryVelocity + Vector2.up * Mathf.Max(0f, upwardSpeed));
    }

    public void ApplyHorizontalMovement()
    {
        if (HasWallJumpControlLock)
        {
            controller.Combat.CheckAndFlip(OrdinaryVelocity.x);
            return;
        }

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
        wallJumpControlLockRemaining = 0f;
        fastFallInputArmed = controller.MoveInput.y >= -controller.stats.tiltThreshold;
        SetOrdinaryVelocity(new Vector2(OrdinaryVelocity.x, controller.stats.jumpForce * speedMultiplier));
    }

    public void ApplyWallJumpForce(int wallSide)
    {
        if (wallSide == 0)
            return;

        float awayFromWall = -Mathf.Sign(wallSide);
        float speedMultiplier = Mathf.Max(0.01f, controller.stats.jumpSpeedMultiplier);
        activeJumpGravityMultiplier = speedMultiplier * speedMultiplier;
        aerialSuspensionRemaining = 0f;
        aerialSuspensionGravityMultiplier = 1f;
        IsFastFalling = false;
        CurrentJumpType = JumpType.Wall;
        fastFallInputArmed = controller.MoveInput.y >= -controller.stats.tiltThreshold;
        wallJumpControlLockRemaining = Mathf.Max(0f, controller.stats.wallJumpControlLockTime);

        SetOrdinaryVelocity(new Vector2(
            awayFromWall * Mathf.Max(0f, controller.stats.wallJumpHorizontalSpeed),
            Mathf.Max(0f, controller.stats.wallJumpVerticalSpeed)));
        controller.Combat.CheckAndFlip(awayFromWall);
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
        aerialSuspensionRemaining = 0f;
        aerialSuspensionGravityMultiplier = 1f;
        IsFastFalling = false;
        CurrentJumpType = JumpType.None;
        wallJumpControlLockRemaining = 0f;
        fastFallInputArmed = controller == null || controller.stats == null || controller.ActiveInput == null || controller.MoveInput.y >= -controller.stats.tiltThreshold;
    }

    private void ConfigureWallContactFilter()
    {
        wallContactFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = wallJumpLayer,
            useTriggers = true
        };
    }

    private void RefreshWallContactState()
    {
        ClearWallContact();

        if (bodyCollider == null || wallJumpLayer.value == 0 || IsGrounded)
            return;

        ConfigureWallContactFilter();
        int preferredSide = controller != null && Mathf.Abs(controller.MoveInput.x) > 0.01f
            ? (int)Mathf.Sign(controller.MoveInput.x)
            : 0;

        if (preferredSide != 0 && TrySetWallContact(preferredSide))
            return;

        if (preferredSide != -1 && TrySetWallContact(-1))
            return;

        if (preferredSide != 1)
            TrySetWallContact(1);
    }

    private bool TrySetWallContact(int side)
    {
        Vector2 direction = side < 0 ? Vector2.left : Vector2.right;
        int hitCount = bodyCollider.Cast(
            direction,
            wallContactFilter,
            wallCastResults,
            Mathf.Max(0f, wallCheckDistance));

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D hit = wallCastResults[i];
            if (hit.collider == null || hit.normal.x * side > -minimumWallNormalX)
                continue;

            WallSide = side;
            WallCollider = hit.collider;
            return true;
        }

        return false;
    }

    private void ClearWallContact()
    {
        WallSide = 0;
        WallCollider = null;
    }
}
