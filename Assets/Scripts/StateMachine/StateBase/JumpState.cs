using UnityEngine;

public class JumpState : PlayerState
{
    private float airTimer;
    private bool isFastFalling;
    private bool skipForceOnEnter;

    public JumpState(PlayerController character, StateMachine sm) : base(character, sm) { }

    /// <summary>
    /// Call this before ChangeState(Jump) when re-entering from the air (e.g. after AirDodge)
    /// so that Enter() doesn't re-apply jump force.
    /// </summary>
    public void PrepareReentry() => skipForceOnEnter = true;

    public override void Enter()
    {
        base.Enter();
        character.Anim.Play("Jump");

        airTimer = 0f;
        isFastFalling = false;

        if (skipForceOnEnter)
        {
            skipForceOnEnter = false; // reset for next normal jump
        }
        else
        {
            character.ConsumeJump();
            character.Movement.ApplyJumpForce();
        }
    }

    public override void Update()
    {
        if (character.IsDead)
        {
            stateMachine.ChangeState(StateCharacter.Die);
            return;
        }

        airTimer += Time.deltaTime;

        // Landing
        if (airTimer > 0.1f && character.IsGrounded)
        {
            stateMachine.ChangeState(Mathf.Abs(character.MoveInput.x) > 0.01f
                ? StateCharacter.Move
                : StateCharacter.Idle);
            return;
        }

        // Double jump (and beyond, up to maxJumps)
        // Note: air dodge is handled in PlayerController.Update() via tap-J detection
        if (character.JumpPressed && character.JumpsRemaining > 0 && !character.IsGrounded)
        {
            airTimer = 0f;
            isFastFalling = false;
            character.ConsumeJump();
            character.Movement.ApplyJumpForce();
            return;
        }

        // Fast fall: S pressed in the air while not already fast falling
        if (!isFastFalling && character.MoveInput.y < -0.5f && !character.IsGrounded)
        {
            isFastFalling = true;
            character.Movement.ApplyFastFall();
        }
    }

    public override void FixedUpdate()
    {
        character.Movement.ApplyHorizontalMovement();
        character.Movement.ClampFallSpeed();
    }
}
