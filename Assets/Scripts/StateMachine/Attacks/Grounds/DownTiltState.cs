public class DownTiltState : AttackState
{
    public DownTiltState(PlayerController character, StateMachine sm, AttackStats attackStats) : base(character, sm, attackStats) { }

    public override void Enter()
    {
        base.Enter();
        character.Anim.Play("DownTilt");
    }

    protected override void ReadyHitbox()
    {
        character.Combat.SetupDTilt(stats);
    }

    protected override void OpenHitbox() => character.Combat.OpenDTiltHitbox();

    protected override void CloseHitbox() => character.Combat.CloseDTiltHitbox();
}
