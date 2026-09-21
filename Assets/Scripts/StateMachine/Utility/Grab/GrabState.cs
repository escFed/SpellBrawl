using UnityEngine;

public abstract class GrabState : CharacterState
{
    protected GrabStats stats;
    private float timer;
    private bool grabOpen;
    private bool grabClosed;

    protected GrabState(CharacterCoordinator character, CharacterStateMachine sm, GrabStats grabStats): base(character, sm)
    {
        stats = grabStats;
    }

    public override void Enter()
    {
        base.Enter();
        character.Health.CancelRespawnProtection();

        timer = 0f;
        grabOpen = false;
        grabClosed = false;

        character.Movement.StopHorizontalMovement();
        ReadyGrabbox();
    }

    public override void Update()
    {
        if (stats == null)
        {
            stateMachine.ChangeState(character.States.Idle);
            return;
        }

        timer += Time.deltaTime;

        if (timer >= stats.startup && !grabOpen)
        {
            OpenGrabbox();
            grabOpen = true;
        }

        if (character.Grab.HasGrabbedTarget)
        {
            CloseGrabbox();
            grabClosed = true;
            character.States.GrabHold.BeginHold(stats);
            stateMachine.ChangeState(character.States.GrabHold);
            return;
        }

        if (timer >= stats.startup + stats.active && !grabClosed)
        {
            CloseGrabbox();
            grabClosed = true;
        }

        if (timer >= stats.startup + stats.active + stats.recovery)
        {
            stateMachine.ChangeState(Mathf.Abs(character.MoveInput.x) > 0.01f ? character.States.Move: character.States.Idle);
        }
    }

    public override void Exit()
    {
        if (grabOpen && !grabClosed)
            CloseGrabbox();
    }

    protected abstract void ReadyGrabbox();
    protected abstract void OpenGrabbox();
    protected abstract void CloseGrabbox();
}
