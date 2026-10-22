using UnityEngine;

public class MoveState : CharacterState
{
    public MoveState(CharacterCoordinator character, CharacterStateMachine sm) : base(character, sm) { }

    public override void Enter()
    {
        base.Enter();
        character.Animation.TryPlay("Move");
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

        if (Mathf.Abs(character.MoveInput.x) < 0.01f)
            stateMachine.ChangeState(character.States.Idle);
    }

    public override void FixedUpdate()
    {
        character.Movement.ApplyHorizontalMovement();
    }
}
