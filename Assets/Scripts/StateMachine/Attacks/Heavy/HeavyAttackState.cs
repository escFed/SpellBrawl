using UnityEngine;

public class HeavyAttackState : AttackState
{
    private static int HeavyAttackTypeParameter = Animator.StringToHash("HeavyAttackType");
    private static int HeavyChargeRatioParameter = Animator.StringToHash("HeavyChargeRatio");
    private static int HeavyChargeMaxParameter = Animator.StringToHash("HeavyChargeMax");
    private static int HeavyAttackPhaseParameter = Animator.StringToHash("HeavyAttackPhase");

    private HeavyAttackType attackType;
    private float chargeRatio;
    private HeavyAttackStats HeavyStats => (HeavyAttackStats)stats;

    public HeavyAttackState(PlayerController character, StateMachine stateMachine) : base(character, stateMachine, null) { }

    public void Prepare(HeavyAttackType selectedAttackType, HeavyAttackStats selectedStats, float selectedChargeRatio)
    {
        attackType = selectedAttackType;
        stats = selectedStats;
        chargeRatio = Mathf.Clamp01(selectedChargeRatio);
    }

    public override void Enter()
    {
        base.Enter();
        character.TryPlayAnimation(HeavyStats.executionAnimationState, GetFallbackAnimation());
        character.TrySetAnimatorInt(HeavyAttackTypeParameter, (int)attackType);
        character.TrySetAnimatorFloat(HeavyChargeRatioParameter, chargeRatio);
        character.TrySetAnimatorBool(HeavyChargeMaxParameter, chargeRatio >= 1f);
        character.TrySetAnimatorInt(HeavyAttackPhaseParameter, 2);
    }

    public override void Update()
    {
        base.Update();
        if (stateMachine.CurrentState != this)
            return;

        int phase = ElapsedTime < stats.startup
            ? 2
            : ElapsedTime < stats.startup + stats.active ? 3 : 4;
        character.TrySetAnimatorInt(HeavyAttackPhaseParameter, phase);
    }

    public override void Exit()
    {
        base.Exit();
        character.Combat.CloseAllHeavyHitboxes();
        character.TrySetAnimatorFloat(HeavyChargeRatioParameter, 0f);
        character.TrySetAnimatorBool(HeavyChargeMaxParameter, false);
        character.TrySetAnimatorInt(HeavyAttackPhaseParameter, 0);
    }

    protected override void ReadyHitbox() => character.Combat.SetupHeavyAttack(attackType, HeavyStats, chargeRatio);
    protected override void OpenHitbox() => character.Combat.OpenHeavyHitbox(attackType);
    protected override void CloseHitbox() => character.Combat.CloseHeavyHitbox(attackType);

    private string GetFallbackAnimation()
    {
        return attackType switch
        {
            HeavyAttackType.Up => "UpTilt",
            HeavyAttackType.Down => "DownTilt",
            _ => "FTilt"
        };
    }
}
