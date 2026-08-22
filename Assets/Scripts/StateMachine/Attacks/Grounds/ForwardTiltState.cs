public class ForwardTiltState : GroundAttackState
{
    public ForwardTiltState(PlayerController character, StateMachine sm, GroundAttackStats attackStats) : base(character, sm, attackStats) { }

    public override void Enter()
    {
        base.Enter();
        character.Anim.Play("FTilt");
    }

    protected override void ReadyHitbox()
    {
        character.Combat.SetupFTilt(GroundStats);
    }

    protected override void OpenHitbox() => character.Combat.OpenFTiltHitbox();

    protected override void CloseHitbox() => character.Combat.CloseFTiltHitbox();
}
