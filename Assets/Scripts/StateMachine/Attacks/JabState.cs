public class JabState : AttackState
{
    public JabState(PlayerController character, StateMachine sm, AttackStats attackStats): base(character, sm, attackStats) { }

    protected override void ReadyHitbox()
    {
        character.Combat.SetupJab(stats);
    }

    protected override void OpenHitbox() => character.Combat.OpenJabHitbox();

    protected override void CloseHitbox() => character.Combat.CloseJabHitbox();
}
