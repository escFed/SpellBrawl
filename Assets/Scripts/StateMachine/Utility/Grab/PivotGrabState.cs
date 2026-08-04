public class PivotGrabState : GrabState
{
    public PivotGrabState(PlayerController character, StateMachine sm, GrabStats grabStats): base(character, sm, grabStats) { }

    public override void Enter()
    {
        base.Enter();
        character.Anim.Play("Grab");
    }

    protected override void ReadyGrabbox() => character.Grab.SetupPivotGrabbox();
    protected override void OpenGrabbox() => character.Grab.OpenPivotGrabbox();
    protected override void CloseGrabbox() => character.Grab.ClosePivotGrabbox();
}