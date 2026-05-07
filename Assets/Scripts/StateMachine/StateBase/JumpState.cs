using UnityEngine;

public class JumpState : PlayerState
{
    private float airTimer;

    public JumpState(PlayerController character, StateMachine sm) : base(character, sm) { }

    public override void Enter()
    {
        airTimer = 0f;
        character.ConsumeJump();
        character.Movement.ApplyJumpForce();
    }

    public override void Update()
    {
        if (character.IsDead)
        {
            stateMachine.ChangeState(StateCharacter.Die);
            return;
        }

        airTimer += Time.deltaTime;

        if (airTimer > 0.1f && character.IsGrounded)
        {
            stateMachine.ChangeState(Mathf.Abs(character.MoveInput.x) > 0.01f ? StateCharacter.Move : StateCharacter.Idle);
        }
    }

    public override void FixedUpdate()
    {
        character.Movement.ApplyHorizontalMovement();
    }
}
