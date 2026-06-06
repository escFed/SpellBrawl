using UnityEngine;

public class JumpState : PlayerState
{
    private float airTimer;
    private bool isFastFalling;

    public JumpState(PlayerController character, StateMachine sm) : base(character, sm) { }

    public override void Enter()
    {
        airTimer = 0f;
        isFastFalling = false;
        character.ConsumeJump();
        character.Movement.ApplyJumpForce();
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
            stateMachine.ChangeState(Mathf.Abs(character.MoveInput.x) > 0.01f ? StateCharacter.Move : StateCharacter.Idle);
            return;
        }

        // Double jump (and beyond, up to maxJumps)
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
