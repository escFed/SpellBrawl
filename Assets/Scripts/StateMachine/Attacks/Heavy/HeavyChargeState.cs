using UnityEngine;

public sealed class HeavyChargeState : CharacterState
{
    private static readonly int HeavyAttackTypeParameter = Animator.StringToHash("HeavyAttackType");
    private static readonly int HeavyChargeRatioParameter = Animator.StringToHash("HeavyChargeRatio");
    private static readonly int HeavyChargeMaxParameter = Animator.StringToHash("HeavyChargeMax");
    private static readonly int HeavyChargeReachedMaxTrigger = Animator.StringToHash("HeavyChargeReachedMax");
    private static readonly int HeavyChargeCancelledTrigger = Animator.StringToHash("HeavyChargeCancelled");
    private static readonly int HeavyAttackPhaseParameter = Animator.StringToHash("HeavyAttackPhase");

    private readonly HeavyAttackCharge charge = new HeavyAttackCharge();
    private HeavyAttackType attackType;
    private HeavyAttackStats stats;
    private bool isExecuting;

    public HeavyChargeState(CharacterCoordinator character, CharacterStateMachine stateMachine) : base(character, stateMachine) { }

    public void Prepare(HeavyAttackType selectedAttackType, HeavyAttackStats selectedStats)
    {
        attackType = selectedAttackType;
        stats = selectedStats;
    }

    public override void Enter()
    {
        character.Health.CancelRespawnProtection();
        isExecuting = false;
        charge.Begin(stats.maxChargeTime);
        character.Movement.StopHorizontalMovement();
        character.Animation.TryPlay(stats.chargeAnimationState, "Idle");
        character.Animation.TrySetInt(HeavyAttackTypeParameter, (int)attackType, optional: true);
        character.Animation.TrySetFloat(HeavyChargeRatioParameter, 0f, optional: true);
        character.Animation.TrySetBool(HeavyChargeMaxParameter, false, optional: true);
        character.Animation.TrySetInt(HeavyAttackPhaseParameter, 1, optional: true);
    }

    public override void Update()
    {
        if (!character.IsGrounded)
        {
            character.States.Jump.PrepareReentry();
            stateMachine.ChangeState(character.States.Jump);
            return;
        }

        if (character.JumpPressed && character.CanJump)
        {
            stateMachine.ChangeState(character.States.Jump);
            return;
        }

        charge.Tick(Time.deltaTime);
        character.Animation.TrySetFloat(HeavyChargeRatioParameter, charge.ChargeRatio, optional: true);

        if (charge.IsFullyCharged)
        {
            character.Animation.TrySetBool(HeavyChargeMaxParameter, true, optional: true);
            character.Animation.TrySetTrigger(HeavyChargeReachedMaxTrigger, optional: true);
            ExecuteAttack(1f);
            return;
        }

        IInputProvider input = character.ActiveInput;
        if (input != null && (input.WasHeavyAttackReleased || !input.IsHeavyAttackHeld))
            ExecuteAttack(charge.ChargeRatio);
    }

    public override void FixedUpdate()
    {
        character.Movement.StopHorizontalMovement();
    }

    public override void Exit()
    {
        charge.Reset();
        character.ActiveInput?.ConsumeHeavyAttackRelease();

        if (!isExecuting)
        {
            character.Animation.TrySetBool(HeavyChargeMaxParameter, false, optional: true);
            character.Animation.TrySetInt(HeavyAttackPhaseParameter, 5, optional: true);
            character.Animation.TrySetTrigger(HeavyChargeCancelledTrigger, optional: true);
        }
    }

    private void ExecuteAttack(float chargeRatio)
    {
        character.States.HeavyAttack.Prepare(attackType, stats, chargeRatio);
        isExecuting = true;
        character.ActiveInput?.ConsumeHeavyAttackRelease();
        stateMachine.ChangeState(character.States.HeavyAttack);
    }
}
