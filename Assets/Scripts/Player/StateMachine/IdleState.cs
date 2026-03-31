using UnityEngine;

public class IdleState : PlayerState
{
    public IdleState(PlayerController player, StateMachine sm) : base(player, sm) { }

    public override void Enter() { }

    public override void Update()
    {
        if (player.IsDead)
        {
            stateMachine.ChangeState(player.DieState);
            return;
        }

        if (player.AttackInput)
        {
            stateMachine.ChangeState(player.ResolveAttackState());
            return;
        }

        if (player.JumpPressed && player.IsGrounded)
        {
            stateMachine.ChangeState(player.JumpState);
            return;
        }

        if (Mathf.Abs(player.MoveInput.x) > 0.01f)
            stateMachine.ChangeState(player.MoveState);
    }

    public override void FixedUpdate()
    {
        player.ApplyHorizontalMovement();
    }
}
