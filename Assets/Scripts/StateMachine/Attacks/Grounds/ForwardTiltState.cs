using UnityEngine;

public class ForwardTiltState : GroundAttackState
{
    private float stepDuration = 0.10f;
    private float stepSpeedMultiplier = 0.6f;
    private float followupStartup = 0.08f;
    private float followupRecovery = 0.20f;
    private float queuedRecovery = 0.06f;
    private float inputWindowAfterActive = 0.14f;
    private int stage;
    private bool openerConnected;
    private bool followupQueued;
    private float facingSign;

    public ForwardTiltState(CharacterCoordinator character, CharacterStateMachine sm, GroundAttackStats attackStats): base(character, sm, attackStats) { }

    public override void Enter()
    {
        stage = 0;
        openerConnected = false;
        followupQueued = false;
        if (Mathf.Abs(character.MoveInput.x) >= character.stats.tiltThreshold)
            character.Combat.FaceDirection(character.MoveInput.x);
        facingSign = character.Combat.FacingSign;
        base.Enter();
        character.Animation.TryPlay("FTilt", "Jab");
    }

    public override void Update()
    {
        if (stage == 0 && openerConnected && !followupQueued && character.AttackInput &&
            ElapsedTime >= Startup && ElapsedTime <= Startup + Active + inputWindowAfterActive)
        {
            character.ActiveInput.ConsumeAttack();
            followupQueued = true;
        }

        base.Update();
    }

    public override void FixedUpdate()
    {
        if (stage == 0 && ElapsedTime < stepDuration && character.IsGrounded)
            character.Movement.ApplyHorizontalDash(facingSign, character.stats.moveSpeed * stepSpeedMultiplier);
        else
            character.Movement.StopHorizontalMovement();
    }

    public override void Exit()
    {
        base.Exit();
        character.Movement.StopHorizontalMovement();
        openerConnected = false;
        followupQueued = false;
    }

    public void ConfirmHit()
    {
        if (stage == 0)
            openerConnected = true;
    }

    protected override void ReadyHitbox() => character.Combat.SetupForwardTiltStage(GroundStats, stage);
    protected override void OpenHitbox() => character.Combat.OpenFTiltHitbox();
    protected override void CloseHitbox() => character.Combat.CloseFTiltHitbox();
    protected override float Startup => stage == 0 ? base.Startup : followupStartup;
    protected override float Recovery => stage == 0 ? (followupQueued ? queuedRecovery : base.Recovery) : followupRecovery;

    protected override bool TryContinueAttack()
    {
        if (stage != 0 || !openerConnected || !followupQueued || !character.IsGrounded)
            return false;

        stage = 1;
        followupQueued = false;
        RestartAttack();
        character.Animation.TryPlay("FTiltFollowup", "Jab");
        return true;
    }
}
