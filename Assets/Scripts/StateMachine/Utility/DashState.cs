using UnityEngine;

public class DashState : PlayerState
{
    private float timer;

    public DashState(PlayerController character, StateMachine sm) : base(character, sm) { }

    public override void Enter()
    {
        timer = 0f;
        character.Anim.Play("Move");
    }

    public override void Update()
    {
        if (character.IsDead)
        {
            stateMachine.ChangeState(StateCharacter.Die);
            return;
        }

        timer += Time.deltaTime;

        if (timer >= character.stats.dashDuration + character.stats.dashRecovery)
            ReturnToLocomotion();
    }

    public override void FixedUpdate()
    {
        if (timer < character.stats.dashDuration)
            character.Movement.ApplyRoll(character.Dash.Direction, character.stats.dashSpeed);
        else
            character.Movement.StopHorizontalMovement();
    }

    public override void Exit()
    {
        character.Movement.StopHorizontalMovement();
    }

    private void ReturnToLocomotion()
    {
        if (!character.IsGrounded)
        {
            stateMachine.Jump.PrepareReentry();
            stateMachine.ChangeState(StateCharacter.Jump);
            return;
        }

        stateMachine.ChangeState(Mathf.Abs(character.MoveInput.x) > 0.01f
            ? StateCharacter.Move
            : StateCharacter.Idle);
    }
}
