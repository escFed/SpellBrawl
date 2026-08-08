public class UpTiltState : GroundAttackState
{
    public UpTiltState(PlayerController character, StateMachine sm, GroundAttackStats attackStats) : base(character, sm, attackStats) { }

    public override void Enter()
    {
        base.Enter();
        character.Anim.Play("UpTilt");
    }

    protected override void ReadyHitbox()
    {
        character.Combat.SetupUTilt(GroundStats);
    }

    protected override void OpenHitbox() => character.Combat.OpenUTiltHitbox();

    protected override void CloseHitbox() => character.Combat.CloseUTiltHitbox();
}
