using UnityEngine;

public class NormalAttackState : AttackState
{
    private int jabFinisherStage = 2;
    private float jabFollowupStartup = 0.08f;
    private float jabQueuedRecovery = 0.06f;
    private float jabInputWindowAfterActive = 0.14f;

    private NormalAttackType attackType;
    private string animationState;
    private bool isAerial;
    private int jabStage;
    private bool jabFollowupQueued;
    private bool jabHeavyQueued;
    private float attackFacingSign;

    public NormalAttackState(CharacterCoordinator character, CharacterStateMachine stateMachine, NormalAttackType attackType, NormalAttackStats attackStats, string animationState, bool isAerial) : base(character, stateMachine, attackStats)
    {
        this.attackType = attackType;
        this.animationState = animationState;
        this.isAerial = isAerial;
    }

    public override void Enter()
    {
        jabStage = 0;
        jabFollowupQueued = false;
        jabHeavyQueued = false;
        if (attackType == NormalAttackType.DownAir)
            character.Movement.DisarmFastFallUntilDownReleased();
        if (attackType == NormalAttackType.DiagonalDownAir)
        {
            character.Combat.FaceDirection(character.MoveInput.x);
            attackFacingSign = character.Combat.FacingSign;
            character.Combat.LockFacing();
        }
        base.Enter();
        if (attackType == NormalAttackType.DiagonalDownAir)
            character.Animation.TryPlay(animationState, "Jab");
        else
            character.Animation.TryPlay(animationState);
    }

    public override void Exit()
    {
        base.Exit();
        if (attackType == NormalAttackType.DiagonalDownAir)
            character.Combat.UnlockFacing();
    }

    public override void Update()
    {
        if (isAerial && character.Movement.HasStableGroundContact)
        {
            stateMachine.ChangeState(GetRecoveryState());
            return;
        }

        if (attackType == NormalAttackType.Jab && jabStage == 1 && !jabHeavyQueued &&
            character.IsGrounded && character.HeavyAttackPressed &&
            ElapsedTime <= Startup + Active + jabInputWindowAfterActive)
        {
            character.ActiveInput.ConsumeHeavyAttack();
            character.ActiveInput.ConsumeAttack();
            jabFollowupQueued = false;
            jabHeavyQueued = true;
        }

        if (attackType == NormalAttackType.Jab && jabStage < jabFinisherStage &&
            !jabHeavyQueued &&
            !jabFollowupQueued && character.AttackInput &&
            ElapsedTime >= Startup && ElapsedTime <= Startup + Active + jabInputWindowAfterActive)
        {
            character.ActiveInput.ConsumeAttack();
            jabFollowupQueued = true;
        }

        base.Update();

        if (attackType != NormalAttackType.Jab || !jabHeavyQueued ||
            stateMachine.CurrentState != this || ElapsedTime < Startup + Active || !character.IsGrounded)
            return;

        HeavyAttackType heavyType = character.Combat.ResolveHeavyAttackType();
        HeavyAttackStats heavyStats = character.Combat.GetHeavyAttackStats(heavyType);
        jabHeavyQueued = false;
        if (heavyStats == null)
        {
            Debug.LogError($"[NormalAttackState] Missing {heavyType} Heavy Attack stats on '{character.name}'.", character);
            return;
        }

        if (character.ActiveInput.IsHeavyAttackHeld)
        {
            character.States.HeavyCharge.Prepare(heavyType, heavyStats, true);
            stateMachine.ChangeState(character.States.HeavyCharge);
        }
        else
        {
            character.ActiveInput.ConsumeHeavyAttackRelease();
            character.States.HeavyAttack.Prepare(heavyType, heavyStats, 0f, true);
            stateMachine.ChangeState(character.States.HeavyAttack);
        }
    }

    public override void FixedUpdate()
    {
        if (attackType == NormalAttackType.DiagonalDownAir &&
            !character.Movement.HasStableGroundContact &&
            character.Movement.AerialSuspensionRemaining <= 0f &&
            ElapsedTime >= Startup && ElapsedTime < Startup + Active)
        {
            Vector2 velocity = ((AerialAttackStats)stats).activeMovementVelocity;
            velocity.x *= attackFacingSign;
            character.Movement.ApplyAerialAttackMovement(velocity);
            return;
        }

        base.FixedUpdate();
    }

    protected override void ReadyHitbox()
    {
        if (attackType == NormalAttackType.Jab)
            character.Combat.SetupJabStage((GroundAttackStats)stats, jabStage);
        else
            character.Combat.SetupNormalAttack(attackType, (NormalAttackStats)stats);
    }
    protected override void OpenHitbox() => character.Combat.SetNormalAttackHitbox(attackType, true);
    protected override void CloseHitbox() => character.Combat.SetNormalAttackHitbox(attackType, false);
    protected override bool StopsHorizontalMovement => !isAerial;
    protected override bool AllowsAirDrift => isAerial;
    protected override float Startup => attackType == NormalAttackType.Jab && jabStage > 0 ? jabFollowupStartup : base.Startup;
    protected override float Recovery => attackType == NormalAttackType.Jab && jabFollowupQueued ? jabQueuedRecovery : base.Recovery;

    protected override bool TryContinueAttack()
    {
        if (attackType != NormalAttackType.Jab || !jabFollowupQueued || jabStage >= jabFinisherStage || !character.IsGrounded)
            return false;

        jabStage++;
        jabFollowupQueued = false;
        RestartAttack();
        character.Animation.TryPlay($"{animationState}{jabStage + 1}", animationState);
        return true;
    }
}
