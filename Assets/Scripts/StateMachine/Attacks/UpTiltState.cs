using UnityEngine;

public class UpTiltState : AttackState
{
    protected override float Startup => character.stats.upTiltAttack.startup;
    protected override float Active => character.stats.upTiltAttack.active;
    protected override float Recovery => character.stats.upTiltAttack.recovery;
    protected override float Cooldown => character.stats.upTiltAttack.cooldown;

    protected override void OpenHitbox() => character.Combat.OpenUTiltHitbox();
    protected override void CloseHitbox() => character.Combat.CloseUTiltHitbox();

    public UpTiltState(PlayerController character, StateMachine sm) : base(character, sm) { }
}