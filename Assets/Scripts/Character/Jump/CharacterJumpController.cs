using UnityEngine;

public sealed class CharacterJumpController
{
    private readonly CharacterCoordinator character;
    private readonly CharacterMovement movement;
    private readonly CharacterStateMachine stateMachine;

    private int airJumpsRemaining;
    private bool groundJumpAvailable;
    private bool wasStablyGrounded;
    private float coyoteTimeRemaining;
    private int currentWallSurfaceId;
    private int lastWallJumpSurfaceId;
    private int wallJumpSide;
    private float wallCoyoteTimeRemaining;

    public int JumpsRemaining => airJumpsRemaining + (CanGroundJump ? 1 : 0);
    public bool CanGroundJump => groundJumpAvailable &&
        (movement.HasStableGroundContact || coyoteTimeRemaining > 0f);
    public bool CanWallJump => !character.IsGrounded && wallCoyoteTimeRemaining > 0f &&
        currentWallSurfaceId != 0 && currentWallSurfaceId != lastWallJumpSurfaceId;
    public bool CanJump => CanGroundJump || CanWallJump ||
        (!character.IsGrounded && airJumpsRemaining > 0);
    public float CoyoteTimeRemaining => coyoteTimeRemaining;
    public float WallCoyoteTimeRemaining => wallCoyoteTimeRemaining;

    public CharacterJumpController(CharacterCoordinator character)
    {
        this.character = character;
        movement = character.Movement;
        stateMachine = character.stateMachine;
    }

    public void Initialize()
    {
        ResetJumps();
        wasStablyGrounded = movement.HasStableGroundContact;
    }

    public bool Tick()
    {
        bool landedThisFrame = UpdateJumpAvailability();
        UpdateWallJumpAvailability();
        EnterAirborneLocomotionIfNeeded();
        return TryHandleBufferedLandingJump(landedThisFrame);
    }

    public bool TryPerformJump()
    {
        IInputProvider input = character.ActiveInput;
        if (input == null || !CanJump)
            return false;

        bool isWallJump = CanWallJump && !CanGroundJump;

        if (CanGroundJump)
        {
            groundJumpAvailable = false;
            coyoteTimeRemaining = 0f;
        }
        else if (isWallJump)
        {
            lastWallJumpSurfaceId = currentWallSurfaceId;
            wallCoyoteTimeRemaining = 0f;
        }
        else
        {
            airJumpsRemaining = Mathf.Max(0, airJumpsRemaining - 1);
        }

        input.ConsumeJump();
        if (isWallJump)
            movement.ApplyWallJumpForce(wallJumpSide);
        else
            movement.ApplyJumpForce();

        return true;
    }

    public void HandleAirborneMovementInput()
    {
        IInputProvider input = character.ActiveInput;
        if (input == null || character.IsGrounded)
            return;

        if (input.WasJumpReleased)
        {
            movement.TryApplyShortHop();
            input.ConsumeJumpRelease();
        }

        movement.TryStartFastFall(character.MoveInput.y);
    }

    public void ResetJumps()
    {
        int maxJumps = character.stats != null ? Mathf.Max(0, character.stats.maxJumps) : 1;
        airJumpsRemaining = Mathf.Max(0, maxJumps - 1);
        groundJumpAvailable = maxJumps > 0;
        coyoteTimeRemaining = 0f;
        currentWallSurfaceId = 0;
        lastWallJumpSurfaceId = 0;
        wallJumpSide = 0;
        wallCoyoteTimeRemaining = 0f;
    }

    public void CancelGroundJumpAvailability()
    {
        ForfeitGroundJump();
        wasStablyGrounded = false;
    }

    private bool UpdateJumpAvailability()
    {
        bool isStablyGrounded = movement.HasStableGroundContact;
        bool landedThisFrame = isStablyGrounded && !wasStablyGrounded;

        if (landedThisFrame)
        {
            ResetJumps();
        }
        else if (!isStablyGrounded && wasStablyGrounded && groundJumpAvailable)
        {
            if (CanArmCoyoteTime())
                coyoteTimeRemaining = character.stats != null ? Mathf.Max(0f, character.stats.coyoteTime) : 0.1f;
            else
                ForfeitGroundJump();
        }
        else if (!isStablyGrounded && coyoteTimeRemaining > 0f)
        {
            coyoteTimeRemaining = Mathf.Max(0f, coyoteTimeRemaining - Time.deltaTime);
            if (coyoteTimeRemaining <= 0f)
                groundJumpAvailable = false;
        }

        wasStablyGrounded = isStablyGrounded;
        return landedThisFrame;
    }

    private bool TryHandleBufferedLandingJump(bool landedThisFrame)
    {
        IInputProvider input = character.ActiveInput;
        if (!landedThisFrame || input == null || !input.HasBufferedJump || !CanGroundJump ||
            !IsLocomotionState())
            return false;

        JumpState jumpState = character.States.Jump;
        if (stateMachine.Is(character.States.Jump))
            return jumpState.TryPerformBufferedJump();

        stateMachine.ChangeState(character.States.Jump);
        return stateMachine.Is(character.States.Jump) && !input.HasBufferedJump;
    }

    private void UpdateWallJumpAvailability()
    {
        if (character.IsGrounded)
        {
            currentWallSurfaceId = 0;
            wallJumpSide = 0;
            wallCoyoteTimeRemaining = 0f;
            return;
        }

        if (movement.IsTouchingWall)
        {
            currentWallSurfaceId = movement.WallCollider.GetInstanceID();
            wallJumpSide = movement.WallSide;
            wallCoyoteTimeRemaining = character.stats != null
                ? Mathf.Max(0f, character.stats.wallCoyoteTime)
                : 0.08f;
            return;
        }

        wallCoyoteTimeRemaining = Mathf.Max(0f, wallCoyoteTimeRemaining - Time.deltaTime);
        if (wallCoyoteTimeRemaining <= 0f)
        {
            currentWallSurfaceId = 0;
            wallJumpSide = 0;
        }
    }

    private bool CanArmCoyoteTime()
    {
        ICharacterState current = stateMachine.CurrentState;
        return current == character.States.Idle || current == character.States.Move ||
            current == character.States.Crouch;
    }

    private bool IsLocomotionState()
    {
        ICharacterState current = stateMachine.CurrentState;
        return current == character.States.Idle || current == character.States.Move ||
            current == character.States.Crouch || current == character.States.Jump;
    }

    private void ForfeitGroundJump()
    {
        groundJumpAvailable = false;
        coyoteTimeRemaining = 0f;
    }

    private void EnterAirborneLocomotionIfNeeded()
    {
        if (character.IsGrounded || !CanArmCoyoteTime())
            return;

        character.States.Jump.PrepareReentry();
        stateMachine.ChangeState(character.States.Jump);
    }
}
