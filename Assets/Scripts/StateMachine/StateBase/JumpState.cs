using UnityEngine;

public class JumpState : CharacterState
{
    private float airTimer;
    private bool skipForceOnEnter;

    public JumpState(CharacterCoordinator character, CharacterStateMachine sm) : base(character, sm) { }

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
        character.Animation.TryPlay("Jump");

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
            stateMachine.ChangeState(character.States.Die);
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
            stateMachine.ChangeState(Mathf.Abs(character.MoveInput.x) > 0.01f ? character.States.Move : character.States.Idle);
            return;
        }
    }

    public override void FixedUpdate()
    {
        character.Movement.ApplyHorizontalMovement();
        character.Movement.ClampFallSpeed();
    }
}
