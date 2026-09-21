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

    public HeavyAttackState(CharacterCoordinator character, CharacterStateMachine stateMachine) : base(character, stateMachine, null) { }

    public void Prepare(HeavyAttackType selectedAttackType, HeavyAttackStats selectedStats, float selectedChargeRatio)
    {
        attackType = selectedAttackType;
        stats = selectedStats;
        chargeRatio = Mathf.Clamp01(selectedChargeRatio);
    }

    public override void Enter()
    {
        base.Enter();
        character.Animation.TryPlay(HeavyStats.executionAnimationState, GetFallbackAnimation());
        character.Animation.TrySetInt(HeavyAttackTypeParameter, (int)attackType);
        character.Animation.TrySetFloat(HeavyChargeRatioParameter, chargeRatio);
        character.Animation.TrySetBool(HeavyChargeMaxParameter, chargeRatio >= 1f);
        character.Animation.TrySetInt(HeavyAttackPhaseParameter, 2);
    }

    public override void Update()
    {
        base.Update();
        if (stateMachine.CurrentState != this)
            return;

        int phase = ElapsedTime < stats.startup
            ? 2
            : ElapsedTime < stats.startup + stats.active ? 3 : 4;
        character.Animation.TrySetInt(HeavyAttackPhaseParameter, phase);
    }

    public override void Exit()
    {
        base.Exit();
        character.Combat.CloseAllHeavyHitboxes();
        character.Animation.TrySetFloat(HeavyChargeRatioParameter, 0f);
        character.Animation.TrySetBool(HeavyChargeMaxParameter, false);
        character.Animation.TrySetInt(HeavyAttackPhaseParameter, 0);
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
