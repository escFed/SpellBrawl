using UnityEngine;

public class ShieldState : PlayerState
{
    private static readonly int ShieldAnimation = Animator.StringToHash("Base Layer.Shield");

    public ShieldState(PlayerController character, StateMachine sm) : base(character, sm) { }

    public override void Enter()
    {
        if (!character.Shield.TryActivate())
        {
            stateMachine.ChangeState(StateCharacter.Idle);
            return;
        }

        character.TryPlayAnimation(ShieldAnimation);
        character.Movement.StopAllMovement();
    }

    public override void Update()
    {
        if (character.IsDead)
        {
            stateMachine.ChangeState(StateCharacter.Die);
            return;
        }

        if (!character.Shield.IsActive || !character.ActiveInput.IsShieldHeld || !character.IsGrounded)
            stateMachine.ChangeState(StateCharacter.Idle);
    }

    public override void FixedUpdate()
    {
        character.Movement.StopHorizontalMovement();
    }

    public override void Exit()
    {
        character.Shield.Deactivate();
    }
}
