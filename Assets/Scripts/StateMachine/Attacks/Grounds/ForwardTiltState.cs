public class ForwardTiltState : AttackState
{
    public ForwardTiltState(PlayerController character, StateMachine sm, AttackStats attackStats) : base(character, sm, attackStats) { }

    public override void Enter()
    {
        base.Enter();
        character.Anim.Play("FTilt");
    }

    protected override void ReadyHitbox()
    {
        character.Combat.SetupFTilt(stats);
    }

    protected override void OpenHitbox() => character.Combat.OpenFTiltHitbox();

    protected override void CloseHitbox() => character.Combat.CloseFTiltHitbox();
}
