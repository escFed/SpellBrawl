using UnityEngine;

public class IdleState : CharacterState
{
    public IdleState(CharacterCoordinator character, CharacterStateMachine sm) : base(character, sm) { }

    public override void Enter()
    {
        base.Enter();
        character.Animation.TryPlay("Idle");
    }

    public override void Update()
    {
        if (character.IsDead)
        {
            stateMachine.ChangeState(character.States.Die);
            return;
        }

        if (character.AttackInput)
        {
            stateMachine.ChangeState(character.Combat.ResolveAttackState());
            return;
        }

        if (character.JumpPressed && character.CanJump)
        {
            stateMachine.ChangeState(character.States.Jump);
            return;
        }

        if (Mathf.Abs(character.MoveInput.x) > 0.01f)
        {
            stateMachine.ChangeState(character.States.Move);
            return;
        }

    }

    public override void FixedUpdate()
    {
        character.Movement.StopHorizontalMovement();
    }
}
