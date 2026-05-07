using UnityEngine;

public class MoveState : PlayerState
{
    public MoveState(PlayerController character, StateMachine sm) : base(character, sm) { }

    public override void Enter() { }

    public override void Update()
    {
        if (character.IsDead)
        {
            stateMachine.ChangeState(StateCharacter.Die);
            return;
        }

        if (character.AttackInput)
        {
            stateMachine.ChangeState(character.Combat.ResolveAttackState());
            return;
        }

        if (character.JumpPressed && character.IsGrounded)
        {
            stateMachine.ChangeState(StateCharacter.Jump);
            return;
        }

        if (Mathf.Abs(character.MoveInput.x) < 0.01f)
            stateMachine.ChangeState(StateCharacter.Idle);
    }

    public override void FixedUpdate()
    {
        character.Movement.ApplyHorizontalMovement();
    }
}
