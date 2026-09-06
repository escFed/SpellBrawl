using UnityEngine;

public class MoveState : PlayerState
{
    private static readonly int MoveAnimation = Animator.StringToHash("Base Layer.Move");

    public MoveState(PlayerController character, StateMachine sm) : base(character, sm) { }

    public override void Enter()
    {
        base.Enter();
        character.TryPlayAnimation(MoveAnimation);
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

        if (character.JumpPressed && character.IsGrounded)
        {
            stateMachine.ChangeState(StateCharacter.Jump);
            return;
        }

        if (character.WalkDirection == 0f)
        {
            // Crouch: stopped horizontally and pressing down
            if (character.MoveInput.y < -character.stats.tiltThreshold && character.IsGrounded)
                stateMachine.ChangeState(StateCharacter.Crouch);
            else
                stateMachine.ChangeState(StateCharacter.Idle);
        }
    }

    public override void FixedUpdate()
    {
        character.Movement.ApplyHorizontalMovement();
    }
}
