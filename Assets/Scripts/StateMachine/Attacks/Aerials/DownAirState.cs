public class DownAirState : AerialAttackState
{
    public DownAirState(PlayerController character, StateMachine sm, AttackStats attackStats): base(character, sm, attackStats) { }

    protected override void ReadyHitbox()
    {
        character.Combat.SetupDownAir(stats);
    }

    protected override void OpenHitbox() => character.Combat.OpenDownAirHitbox();

    protected override void CloseHitbox() => character.Combat.CloseDownAirHitbox();
}
