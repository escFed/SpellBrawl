using UnityEngine;

public sealed class HeavyChargeState : PlayerState
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

    public HeavyChargeState(PlayerController character, StateMachine stateMachine) : base(character, stateMachine) { }

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
        character.TryPlayAnimation(stats.chargeAnimationState, "Idle");
        character.TrySetAnimatorInt(HeavyAttackTypeParameter, (int)attackType);
        character.TrySetAnimatorFloat(HeavyChargeRatioParameter, 0f);
        character.TrySetAnimatorBool(HeavyChargeMaxParameter, false);
        character.TrySetAnimatorInt(HeavyAttackPhaseParameter, 1);
    }

    public override void Update()
    {
        if (!character.IsGrounded)
        {
            stateMachine.Jump.PrepareReentry();
            stateMachine.ChangeState(StateCharacter.Jump);
            return;
        }

        if (character.JumpPressed && character.CanJump)
        {
            stateMachine.ChangeState(StateCharacter.Jump);
            return;
        }

        charge.Tick(Time.deltaTime);
        character.TrySetAnimatorFloat(HeavyChargeRatioParameter, charge.ChargeRatio);

        if (charge.IsFullyCharged)
        {
            character.TrySetAnimatorBool(HeavyChargeMaxParameter, true);
            character.TrySetAnimatorTrigger(HeavyChargeReachedMaxTrigger);
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
            character.TrySetAnimatorBool(HeavyChargeMaxParameter, false);
            character.TrySetAnimatorInt(HeavyAttackPhaseParameter, 5);
            character.TrySetAnimatorTrigger(HeavyChargeCancelledTrigger);
        }
    }

    private void ExecuteAttack(float chargeRatio)
    {
        stateMachine.HeavyAttack.Prepare(attackType, stats, chargeRatio);
        isExecuting = true;
        character.ActiveInput?.ConsumeHeavyAttackRelease();
        stateMachine.ChangeState(StateCharacter.HeavyAttack);
    }
}
