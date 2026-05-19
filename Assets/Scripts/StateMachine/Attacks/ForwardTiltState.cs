public class ForwardTiltState : AttackState
{
    public ForwardTiltState(PlayerController character, StateMachine sm, AttackStats attackStats) : base(character, sm, attackStats) { }

    protected override void ReadyHitbox()
    {
        character.Combat.SetupFTilt(stats);
    }

    protected override void OpenHitbox() => character.Combat.OpenFTiltHitbox();

    protected override void CloseHitbox() => character.Combat.CloseFTiltHitbox();
}
