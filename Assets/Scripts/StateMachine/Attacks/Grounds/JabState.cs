public class JabState : GroundAttackState
{
    public JabState(PlayerController character, StateMachine sm, GroundAttackStats attackStats) : base(character, sm, attackStats) { }

    public override void Enter()
    {
        base.Enter();
        character.Anim.Play("Jab");
    }
    protected override void ReadyHitbox()
    {
        character.Combat.SetupJab(GroundStats);
    }

    protected override void OpenHitbox() => character.Combat.OpenJabHitbox();

    protected override void CloseHitbox() => character.Combat.CloseJabHitbox();
}
