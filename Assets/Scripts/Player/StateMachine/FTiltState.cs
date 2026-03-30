using UnityEngine;

public class ForwardTiltState : AttackState
{
    protected override float Startup => 0.10f;
    protected override float Active => 0.08f;
    protected override float Recovery => 0.20f;
    protected override float Cooldown => 0f;

    protected override void OpenHitbox() => player.OpenFTiltHitbox();
    protected override void CloseHitbox() => player.CloseFTiltHitbox();

    public ForwardTiltState(PlayerController player, StateMachine sm) : base(player, sm) { }
}
