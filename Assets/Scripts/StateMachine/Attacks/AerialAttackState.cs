public abstract class AerialAttackState : AttackState
{
    protected AerialAttackState(PlayerController character, StateMachine sm, AttackStats attackStats): base(character, sm, attackStats) { }

    public override void Enter()
    {
        base.Enter();
        character.Anim.Play("Jab");
    }

    protected override bool StopsHorizontalMovement => false;
    protected override bool AllowsAirDrift => true;
}

