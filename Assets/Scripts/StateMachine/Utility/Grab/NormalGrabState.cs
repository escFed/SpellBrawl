public class NormalGrabState : GrabState
{
    public NormalGrabState(PlayerController character, StateMachine sm, GrabStats grabStats): base(character, sm, grabStats) { }

    public override void Enter()
    {
        base.Enter();
        character.Anim.Play("Grab");
    }

    protected override void ReadyGrabbox() => character.Grab.SetupGrabbox();
    protected override void OpenGrabbox() => character.Grab.OpenGrabbox();
    protected override void CloseGrabbox() => character.Grab.CloseGrabbox();
}