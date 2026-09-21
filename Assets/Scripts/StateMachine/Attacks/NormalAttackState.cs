public sealed class NormalAttackState : AttackState
{
    private readonly NormalAttackType attackType;
    private readonly string animationState;
    private readonly bool isAerial;

    public NormalAttackState(CharacterCoordinator character, CharacterStateMachine stateMachine,
        NormalAttackType attackType, NormalAttackStats attackStats, string animationState, bool isAerial)
        : base(character, stateMachine, attackStats)
    {
        this.attackType = attackType;
        this.animationState = animationState;
        this.isAerial = isAerial;
    }

    public override void Enter()
    {
        base.Enter();
        character.Animation.TryPlay(animationState);
    }

    protected override void ReadyHitbox() => character.Combat.SetupNormalAttack(attackType, (NormalAttackStats)stats);
    protected override void OpenHitbox() => character.Combat.SetNormalAttackHitbox(attackType, true);
    protected override void CloseHitbox() => character.Combat.SetNormalAttackHitbox(attackType, false);
    protected override bool StopsHorizontalMovement => !isAerial;
    protected override bool AllowsAirDrift => isAerial;
}
