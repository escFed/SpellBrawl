public class DownTiltState : AttackState
{
    protected override float Startup => character.stats.dTiltAttack.startup;
    protected override float Active => character.stats.dTiltAttack.active;
    protected override float Recovery => character.stats.dTiltAttack.recovery;
    protected override float Cooldown => character.stats.dTiltAttack.cooldown;

    protected override void OpenHitbox() => character.Combat.OpenDTiltHitbox();
    protected override void CloseHitbox() => character.Combat.CloseDTiltHitbox();

    public DownTiltState(PlayerController character, StateMachine sm) : base(character, sm) { }
}
