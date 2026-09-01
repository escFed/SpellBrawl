using UnityEngine;

public class CrouchState : PlayerState
{
    private static readonly int CrouchAnimation = Animator.StringToHash("Base Layer.Crouch");

    public CrouchState(PlayerController character, StateMachine sm) : base(character, sm) { }

    public override void Enter()
    {
        character.Movement.SetCrouching(true);

        if (character.Anim == null)
            return;

        if (character.Anim.HasState(0, CrouchAnimation))
        {
            character.Anim.Play(CrouchAnimation, 0, 0f);
            return;
        }

        character.Anim.Play("Idle", 0, 0f);
    }

    public override void Exit()
    {
        character.Movement.SetCrouching(false);
    }

    public override void Update()
    {
        if (character.IsDead)
        {
            stateMachine.ChangeState(StateCharacter.Die);
            return;
        }

        // Jump from crouch
        if (character.JumpPressed && character.CanJump)
        {
            stateMachine.ChangeState(StateCharacter.Jump);
            return;
        }

        // Exit crouch when S released or no longer grounded
        bool stillPressingDown = character.MoveInput.y < -character.stats.tiltThreshold;
        if (!stillPressingDown || !character.IsGrounded)
        {
            stateMachine.ChangeState(Mathf.Abs(character.MoveInput.x) > 0.01f
                ? StateCharacter.Move
                : StateCharacter.Idle);
        }
    }

    public override void FixedUpdate()
    {
        character.Movement.StopHorizontalMovement();
    }
}
