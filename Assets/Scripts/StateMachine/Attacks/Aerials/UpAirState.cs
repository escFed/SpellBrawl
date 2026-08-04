public class UpAirState : AerialAttackState
{
    public UpAirState(PlayerController character, StateMachine sm, AttackStats attackStats): base(character, sm, attackStats) { }

    protected override void ReadyHitbox()
    {
        character.Combat.SetupUpAir(stats);
    }

    protected override void OpenHitbox() => character.Combat.OpenUpAirHitbox();

    protected override void CloseHitbox() => character.Combat.CloseUpAirHitbox();
}
