using UnityEngine;

public class JumpState : PlayerState
{
    private float airTimer;
    private bool skipForceOnEnter;

    public JumpState(PlayerController character, StateMachine sm) : base(character, sm) { }

    public void PrepareReentry() => skipForceOnEnter = true;

    public bool TryPerformBufferedJump()
    {
        if (!character.TryPerformJump())
            return false;

        airTimer = 0f;
        return true;
    }

    public override void Enter()
    {
        base.Enter();
        character.Anim.Play("Jump");

        airTimer = 0f;

        if (skipForceOnEnter)
        {
            skipForceOnEnter = false;
        }
        else
        {
            character.TryPerformJump();
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
        character.HandleAirborneMovementInput();

        if (character.AttackInput)
        {
            stateMachine.ChangeState(character.Combat.ResolveAttackState());
            return;
        }

        if (character.JumpPressed && character.CanJump)
        {
            TryPerformBufferedJump();
            return;
        }

        if (airTimer > 0.1f && character.IsGrounded)
        {
            stateMachine.ChangeState(Mathf.Abs(character.MoveInput.x) > 0.01f ? StateCharacter.Move : StateCharacter.Idle);
            return;
        }
    }

    public override void FixedUpdate()
    {
        character.Movement.ApplyHorizontalMovement();
        character.Movement.ClampFallSpeed();
    }
}
