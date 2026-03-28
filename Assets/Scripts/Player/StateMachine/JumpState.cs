using UnityEngine;

public class JumpState : PlayerState
{
    public JumpState(PlayerController player, StateMachine sm) : base(player, sm) { }

    public override void Enter()
    {
        player.ConsumeJump();
        player.ApplyJumpForce();
    }

    public override void Update()
    {
        if (player.IsDead)
        {
            stateMachine.ChangeState(player.DieState);
            return;
        }

        if (player.IsGrounded)
        {
            stateMachine.ChangeState(Mathf.Abs(player.MoveInput.x) > 0.01f ? (IState) player.MoveState: player.IdleState);
        }
    }

    public override void FixedUpdate()
    {
        player.ApplyHorizontalMovement();
    }
}
