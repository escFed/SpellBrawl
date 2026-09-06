using UnityEngine;

public class IdleState : PlayerState
{
    private static readonly int IdleAnimation = Animator.StringToHash("Base Layer.Idle");

    public IdleState(PlayerController character, StateMachine sm) : base(character, sm) { }

    public override void Enter()
    {
        base.Enter();
        character.TryPlayAnimation(IdleAnimation);
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

        if (character.WalkDirection != 0f)
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
