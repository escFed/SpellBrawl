using UnityEngine;

public class HeavyAttackState : AttackState
{
    private const float JabFinisherStartup = 0.12f;
    private const float JabFinisherRecovery = 0.30f;
    private static int HeavyAttackTypeParameter = Animator.StringToHash("HeavyAttackType");
    private static int HeavyChargeRatioParameter = Animator.StringToHash("HeavyChargeRatio");
    private static int HeavyChargeMaxParameter = Animator.StringToHash("HeavyChargeMax");
    private static int HeavyAttackPhaseParameter = Animator.StringToHash("HeavyAttackPhase");

    private HeavyAttackType attackType;
    private float chargeRatio;
    private bool isJabFinisher;
    private HeavyAttackStats HeavyStats => (HeavyAttackStats)stats;

    public HeavyAttackState(CharacterCoordinator character, CharacterStateMachine stateMachine) : base(character, stateMachine, null) { }

    public void Prepare(HeavyAttackType selectedAttackType, HeavyAttackStats selectedStats, float selectedChargeRatio, bool fromJabCombo = false)
    {
        attackType = selectedAttackType;
        stats = selectedStats;
        chargeRatio = Mathf.Clamp01(selectedChargeRatio);
        isJabFinisher = fromJabCombo;
    }

    public override void Enter()
    {
        base.Enter();
        character.Animation.TryPlay(HeavyStats.executionAnimationState, GetFallbackAnimation());
        if (isJabFinisher)
            character.Animation.TrySetPlaybackSpeed(Mathf.Max(1f, stats.startup / Startup));
        character.Animation.TrySetInt(HeavyAttackTypeParameter, (int)attackType, optional: true);
        character.Animation.TrySetFloat(HeavyChargeRatioParameter, chargeRatio, optional: true);
        character.Animation.TrySetBool(HeavyChargeMaxParameter, chargeRatio >= 1f, optional: true);
        character.Animation.TrySetInt(HeavyAttackPhaseParameter, 2, optional: true);
    }

    public override void Update()
    {
        base.Update();
        if (stateMachine.CurrentState != this)
            return;

        int phase = ElapsedTime < Startup
            ? 2
            : ElapsedTime < Startup + Active ? 3 : 4;
        character.Animation.TrySetInt(HeavyAttackPhaseParameter, phase, optional: true);
    }

    public override void Exit()
    {
        base.Exit();
        if (isJabFinisher)
            character.Animation.TrySetPlaybackSpeed(1f);
        character.Combat.CloseAllHeavyHitboxes();
        character.Animation.TrySetFloat(HeavyChargeRatioParameter, 0f, optional: true);
        character.Animation.TrySetBool(HeavyChargeMaxParameter, false, optional: true);
        character.Animation.TrySetInt(HeavyAttackPhaseParameter, 0, optional: true);
    }

    protected override void ReadyHitbox() => character.Combat.SetupHeavyAttack(attackType, HeavyStats, chargeRatio, isJabFinisher);
    protected override void OpenHitbox() => character.Combat.OpenHeavyHitbox(attackType);
    protected override void CloseHitbox() => character.Combat.CloseHeavyHitbox(attackType);
    protected override float Startup => isJabFinisher ? JabFinisherStartup : base.Startup;
    protected override float Recovery => isJabFinisher ? JabFinisherRecovery : base.Recovery;

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
