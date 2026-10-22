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
        if (attackType == NormalAttackType.DownAir)
            character.Movement.DisarmFastFallUntilDownReleased();
        if (attackType == NormalAttackType.DiagonalDownAir)
        {
            character.Combat.FaceDirection(character.MoveInput.x);
            attackFacingSign = character.Combat.FacingSign;
            character.Combat.LockFacing();
        }
        base.Enter();
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

        if (attackType == NormalAttackType.Jab && jabStage < jabFinisherStage &&
            !jabFollowupQueued && character.AttackInput &&
            ElapsedTime >= Startup && ElapsedTime <= Startup + Active + jabInputWindowAfterActive)
        {
            character.ActiveInput.ConsumeAttack();
            jabFollowupQueued = true;
        }

        base.Update();
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
        character.Animation.TryPlay(animationState);
        return true;
    }
}
