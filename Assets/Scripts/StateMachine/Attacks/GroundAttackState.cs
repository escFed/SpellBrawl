public abstract class GroundAttackState : AttackState
{
    protected GroundAttackState(PlayerController character, StateMachine sm, GroundAttackStats attackStats): base(character, sm, attackStats) { }

    protected GroundAttackStats GroundStats => (GroundAttackStats)stats;
}
