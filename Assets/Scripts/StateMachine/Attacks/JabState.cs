using UnityEngine;

public class JabState : AttackState
{
    protected override float Startup => character.stats.jabAttack.startup;
    protected override float Active => character.stats.jabAttack.active;
    protected override float Recovery => character.stats.jabAttack.recovery;
    protected override float Cooldown => character.stats.jabAttack.cooldown;

    protected override void OpenHitbox() => character.Combat.OpenJabHitbox();
    protected override void CloseHitbox() => character.Combat.CloseJabHitbox();

    public JabState(PlayerController character, StateMachine sm) : base(character, sm) { }
}
