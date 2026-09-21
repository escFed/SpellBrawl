public abstract class GroundAttackState : AttackState
{
    protected GroundAttackState(CharacterCoordinator character, CharacterStateMachine sm, GroundAttackStats attackStats): base(character, sm, attackStats) { }

    protected GroundAttackStats GroundStats => (GroundAttackStats)stats;
}
