using UnityEngine;

public class ShieldState : CharacterState
{
    private static readonly int ShieldAnimation = Animator.StringToHash("Base Layer.Shield");

    public ShieldState(CharacterCoordinator character, CharacterStateMachine sm) : base(character, sm) { }

    public override void Enter()
    {
        if (!character.Shield.TryActivate())
        {
            stateMachine.ChangeState(character.States.Idle);
            return;
        }

        character.Animation.TryPlay(ShieldAnimation);
        character.Movement.StopAllMovement();
    }

    public override void Update()
    {
        if (character.IsDead)
        {
            stateMachine.ChangeState(character.States.Die);
            return;
        }

        if (!character.Shield.IsActive || !character.ActiveInput.IsShieldHeld || !character.IsGrounded)
            stateMachine.ChangeState(character.States.Idle);
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
