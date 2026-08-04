public class ForwardAirState : AerialAttackState
{
    public ForwardAirState(PlayerController character, StateMachine sm, AttackStats attackStats): base(character, sm, attackStats) { }

    protected override void ReadyHitbox()
    {
        character.Combat.SetupForwardAir(stats);
    }

    protected override void OpenHitbox() => character.Combat.OpenForwardAirHitbox();

    protected override void CloseHitbox() => character.Combat.CloseForwardAirHitbox();
}
