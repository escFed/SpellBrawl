public class NeutralAirState : AerialAttackState
{
    public NeutralAirState(PlayerController character, StateMachine sm, AerialAttackStats attackStats) : base(character, sm, attackStats) { }

    protected override void ReadyHitbox()
    {
        character.Combat.SetupNeutralAir(AerialStats);
    }

    protected override void OpenHitbox() => character.Combat.OpenNeutralAirHitbox();

    protected override void CloseHitbox() => character.Combat.CloseNeutralAirHitbox();
}
