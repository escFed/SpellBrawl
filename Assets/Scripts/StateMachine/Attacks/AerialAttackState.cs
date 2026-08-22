public abstract class AerialAttackState : AttackState
{
    protected AerialAttackState(PlayerController character, StateMachine sm, AerialAttackStats attackStats) : base(character, sm, attackStats) { }

    protected AerialAttackStats AerialStats => (AerialAttackStats)stats;

    public override void Enter()
    {
        base.Enter();
        character.Anim.Play("Jab");
    }

    protected override bool StopsHorizontalMovement => false;
    protected override bool AllowsAirDrift => true;
}

