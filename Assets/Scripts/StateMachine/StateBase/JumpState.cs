using UnityEngine;

public class JumpState : PlayerState
{
    private float airTimer;
    private bool isFastFalling;
    private bool skipForceOnEnter;

    public JumpState(PlayerController character, StateMachine sm) : base(character, sm) { }

    public void PrepareReentry() => skipForceOnEnter = true;

    public override void Enter()
    {
        base.Enter();
        character.Anim.Play("Jump");

        airTimer = 0f;
        isFastFalling = false;

        if (skipForceOnEnter)
        {
            skipForceOnEnter = false;
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

        if (character.AttackInput)
        {
            stateMachine.ChangeState(character.Combat.ResolveAttackState());
            return;
        }

        if (airTimer > 0.1f && character.IsGrounded)
        {
            stateMachine.ChangeState(Mathf.Abs(character.MoveInput.x) > 0.01f ? StateCharacter.Move : StateCharacter.Idle);
            return;
        }

        if (character.JumpPressed && character.JumpsRemaining > 0 && !character.IsGrounded)
        {
            airTimer = 0f;
            isFastFalling = false;
            character.ConsumeJump();
            character.Movement.ApplyJumpForce();
            return;
        }

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
