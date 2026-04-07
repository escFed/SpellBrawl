public class DownTiltState : AttackState
{
    protected override float Startup => 0.08f;
    protected override float Active => 0.10f;
    protected override float Recovery => 0.18f;
    protected override float Cooldown => 0f;

    protected override void OpenHitbox() => player.OpenDTiltHitbox();
    protected override void CloseHitbox() => player.CloseDTiltHitbox();

    public DownTiltState(PlayerController player, StateMachine sm) : base(player, sm) { }
}
