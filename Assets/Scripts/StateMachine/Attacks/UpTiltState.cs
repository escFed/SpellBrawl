public class UpTiltState : AttackState
{
    public UpTiltState(PlayerController character, StateMachine sm, AttackStats attackStats) : base(character, sm, attackStats) { }

    public override void Enter()
    {
        base.Enter();
        character.Anim.Play("Jab");
    }

    protected override void ReadyHitbox()
    {
        character.Combat.SetupUTilt(stats);
    }

    protected override void OpenHitbox() => character.Combat.OpenUTiltHitbox();

    protected override void CloseHitbox() => character.Combat.CloseUTiltHitbox();
}