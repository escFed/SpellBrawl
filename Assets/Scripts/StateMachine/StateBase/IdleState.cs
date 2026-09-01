using UnityEngine;

public class IdleState : PlayerState
{
    public IdleState(PlayerController character, StateMachine sm) : base(character, sm) { }

    public override void Enter()
    {
        base.Enter();
        character.Anim.Play("Idle");
    }

    public override void Update()
    {
        if (character.IsDead)
        {
            stateMachine.ChangeState(StateCharacter.Die);
            return;
        }

        if (character.GrabInput)
        {
            stateMachine.ChangeState(character.Grab.ResolveGrabState());
            return;
        }

        if (character.AttackInput)
        {
            stateMachine.ChangeState(character.Combat.ResolveAttackState());
            return;
        }

        if (character.JumpPressed && character.CanJump)
        {
            stateMachine.ChangeState(StateCharacter.Jump);
            return;
        }

        if (Mathf.Abs(character.MoveInput.x) > 0.01f)
        {
            stateMachine.ChangeState(StateCharacter.Move);
            return;
        }

        // Crouch: pure downward input on the ground (no horizontal component)
        if (character.MoveInput.y < -character.stats.tiltThreshold &&
            Mathf.Abs(character.MoveInput.x) < character.stats.tiltThreshold &&
            character.IsGrounded)
        {
            stateMachine.ChangeState(StateCharacter.Crouch);
        }
    }

    public override void FixedUpdate()
    {
        character.Movement.StopHorizontalMovement();
    }
}
