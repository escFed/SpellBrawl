using UnityEngine;

public class UpTiltState : AttackState
{
    protected override float Startup => 0.08f;
    protected override float Active => 0.10f;
    protected override float Recovery => 0.18f;
    protected override float Cooldown => 0f;

    protected override void OpenHitbox() => player.OpenUTiltHitbox();
    protected override void CloseHitbox() => player.CloseUTiltHitbox();

    public UpTiltState(PlayerController player, StateMachine sm) : base(player, sm) { }
}