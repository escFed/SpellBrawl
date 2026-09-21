using UnityEngine;

public class CrouchState : CharacterState
{
    public CrouchState(CharacterCoordinator character, CharacterStateMachine sm) : base(character, sm) { }

    public override void Enter()
    {
        character.Movement.SetCrouching(true);
        character.Animation.TryPlay("Crouch", "Idle");
    }

    public override void Exit()
    {
        character.Movement.SetCrouching(false);
    }

    public override void Update()
    {
        if (character.IsDead)
        {
            stateMachine.ChangeState(character.States.Die);
            return;
        }

        // Jump from crouch
        if (character.JumpPressed && character.CanJump)
        {
            stateMachine.ChangeState(character.States.Jump);
            return;
        }

        // Exit crouch when S released or no longer grounded
        bool stillPressingDown = character.MoveInput.y < -character.stats.tiltThreshold;
        if (!stillPressingDown || !character.IsGrounded)
        {
            stateMachine.ChangeState(Mathf.Abs(character.MoveInput.x) > 0.01f
                ? character.States.Move
                : character.States.Idle);
        }
    }

    public override void FixedUpdate()
    {
        character.Movement.StopHorizontalMovement();
    }
}
