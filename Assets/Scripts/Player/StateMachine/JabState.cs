using UnityEngine;

public class JabState : AttackState
{
    protected override float Startup => 0.05f;
    protected override float Active => 0.05f;
    protected override float Recovery => 0.15f;
    protected override float Cooldown => 0f;

    protected override void OpenHitbox() => player.OpenJabHitbox();
    protected override void CloseHitbox() => player.CloseJabHitbox();

    public JabState(PlayerController player, StateMachine sm) : base(player, sm) { }
}
