using UnityEngine;

public class DashState : CharacterState
{
    private float timer;

    public DashState(CharacterCoordinator character, CharacterStateMachine sm) : base(character, sm) { }

    public override void Enter()
    {
        timer = 0f;
        character.Animation.TryPlay("Move");
    }

    public override void Update()
    {
        if (character.IsDead)
        {
            stateMachine.ChangeState(character.States.Die);
            return;
        }

        timer += Time.deltaTime;

        if (character.AttackInput)
        {
            character.ActiveInput?.ConsumeAttack();
            stateMachine.ChangeState(character.States.DashAttack);
            return;
        }

        if (character.GrabInput)
        {
            character.ActiveInput?.ConsumeGrab();
            stateMachine.ChangeState(character.States.DashGrab);
            return;
        }

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
            character.States.Jump.PrepareReentry();
            stateMachine.ChangeState(character.States.Jump);
            return;
        }

        stateMachine.ChangeState(Mathf.Abs(character.MoveInput.x) > 0.01f
            ? character.States.Move
            : character.States.Idle);
    }
}
