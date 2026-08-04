using UnityEngine;

public class DashAttackState : AttackState
{
    private float dashTimer;

    public DashAttackState(PlayerController character, StateMachine sm, AttackStats attackStats): base(character, sm, attackStats) { }

    public override void Enter()
    {
        dashTimer = 0f;
        base.Enter();
        character.Anim.Play("DashAttack");
    }

    public override void Update()
    {
        dashTimer += Time.deltaTime;
        base.Update();
    }

    public override void FixedUpdate()
    {
        if (dashTimer < character.stats.dashDuration)
            character.Movement.ApplyRoll(character.Dash.Direction, character.stats.dashAttackSpeed);
        else
            character.Movement.StopHorizontalMovement();
    }

    public override void Exit()
    {
        base.Exit();
        character.Movement.StopHorizontalMovement();
    }

    protected override void ReadyHitbox() => character.Combat.SetupFTilt(stats);
    protected override void OpenHitbox() => character.Combat.OpenFTiltHitbox();
    protected override void CloseHitbox() => character.Combat.CloseFTiltHitbox();
    protected override bool StopsHorizontalMovement => false;
}
