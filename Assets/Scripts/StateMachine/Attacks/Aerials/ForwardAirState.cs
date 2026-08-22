public class ForwardAirState : AerialAttackState
{
    public ForwardAirState(PlayerController character, StateMachine sm, AerialAttackStats attackStats) : base(character, sm, attackStats) { }

    protected override void ReadyHitbox()
    {
        character.Combat.SetupForwardAir(AerialStats);
    }

    protected override void OpenHitbox() => character.Combat.OpenForwardAirHitbox();

    protected override void CloseHitbox() => character.Combat.CloseForwardAirHitbox();
}
