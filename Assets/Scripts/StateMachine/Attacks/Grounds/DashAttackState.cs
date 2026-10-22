using UnityEngine;

public class DashAttackState : GroundAttackState
{
    private float dashTimer;
    private float followThroughEndTime;
    private bool hitConfirmed;

    public DashAttackState(CharacterCoordinator character, CharacterStateMachine sm, GroundAttackStats attackStats) : base(character, sm, attackStats) { }

    public override void Enter()
    {
        dashTimer = 0f;
        followThroughEndTime = 0f;
        hitConfirmed = false;
        character.Combat.LockFacing();
        base.Enter();
        character.Animation.TryPlay("DashAttack", "FTilt");
    }

    public override void Update()
    {
        dashTimer += Time.deltaTime;
        base.Update();
    }

    public override void FixedUpdate()
    {
        if (dashTimer < character.stats.dashDuration)
            character.Movement.ApplyHorizontalDash(character.Dash.Direction, character.stats.dashAttackSpeed);
        else if (hitConfirmed && dashTimer < followThroughEndTime)
            character.Movement.ApplyHorizontalDash(character.Dash.Direction, character.stats.dashAttackHitFollowThroughSpeed);
        else
            character.Movement.StopHorizontalMovement();
    }

    public void ConfirmHit(AttackStats attackStats)
    {
        if (hitConfirmed || attackStats != stats)
            return;

        hitConfirmed = true;
        followThroughEndTime = Mathf.Max(dashTimer, character.stats.dashDuration) +
            Mathf.Max(0f, character.stats.dashAttackHitFollowThroughDuration);
    }

    public override void Exit()
    {
        base.Exit();
        character.Combat.UnlockFacing();
        character.Dash.CompleteDash();
        character.Movement.StopHorizontalMovement();
    }

    protected override void ReadyHitbox() => character.Combat.SetupFTilt(GroundStats);
    protected override void OpenHitbox() => character.Combat.OpenFTiltHitbox();
    protected override void CloseHitbox() => character.Combat.CloseFTiltHitbox();
    protected override bool StopsHorizontalMovement => false;
}
