using UnityEngine;

public class DashGrabState : GrabState
{
    private float slideTimer;

    public DashGrabState(PlayerController character, StateMachine sm, GrabStats grabStats): base(character, sm, grabStats) { }

    public override void Enter()
    {
        slideTimer = 0f;
        base.Enter();
        character.Anim.Play("Grab");
    }

    public override void Update()
    {
        slideTimer += Time.deltaTime;
        base.Update();
    }

    public override void FixedUpdate()
    {
        if (slideTimer < character.stats.dashGrabSlideDuration)
            character.Movement.ApplyRoll(character.Dash.Direction, character.stats.dashGrabSpeed);
        else
            character.Movement.StopHorizontalMovement();
    }

    public override void Exit()
    {
        base.Exit();
        character.Movement.StopHorizontalMovement();
    }

    protected override void ReadyGrabbox() => character.Grab.SetupGrabbox();
    protected override void OpenGrabbox() => character.Grab.OpenGrabbox();
    protected override void CloseGrabbox() => character.Grab.CloseGrabbox();
}
