public class ForwardTiltState : AttackState
{
    protected override float Startup => character.stats.fTiltAttack.startup;
    protected override float Active => character.stats.fTiltAttack.active;
    protected override float Recovery => character.stats.fTiltAttack.recovery;
    protected override float Cooldown => character.stats.fTiltAttack.cooldown;

    protected override void OpenHitbox() => character.Combat.OpenFTiltHitbox();
    protected override void CloseHitbox() => character.Combat.CloseFTiltHitbox();

    public ForwardTiltState(PlayerController character, StateMachine sm) : base(character, sm) { }
}
