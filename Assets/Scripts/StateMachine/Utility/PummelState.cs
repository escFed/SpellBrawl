using UnityEngine;

public class PummelState : PlayerState
{
    private GrabStats stats;
    private float timer;
    private bool preserveGrabOnExit;

    public PummelState(PlayerController character, StateMachine sm) : base(character, sm) { }

    public void SetGrabStats(GrabStats grabStats) => stats = grabStats;

    public override void Enter()
    {
        base.Enter();

        timer = 0f;
        preserveGrabOnExit = false;
        character.Movement.StopAllMovement();

        if (stats == null || !character.Grab.HasGrabbedTarget)
        {
            stateMachine.ChangeState(StateCharacter.Idle);
            return;
        }

        character.Anim.Play("Pummel", 0, 0f);
        character.Grab.ApplyPummel(stats);
    }

    public override void Update()
    {
        if (!character.Grab.HasGrabbedTarget)
        {
            stateMachine.ChangeState(StateCharacter.Idle);
            return;
        }

        timer += Time.deltaTime;

        if (stateMachine.GrabHold.HasHoldExpired)
        {
            ChangeStatePreservingGrab(StateCharacter.Throw);
            return;
        }

        if (timer >= Mathf.Max(0f, stats.pummelCooldown))
            ChangeStatePreservingGrab(StateCharacter.GrabHold);
    }

    public override void FixedUpdate()
    {
        character.Movement.StopAllMovement();
        character.Grab.UpdateGrabbedTargetPosition();
    }

    public override void Exit()
    {
        if (!preserveGrabOnExit)
            character.Grab.ReleaseGrabbedTarget();

        stats = null;
    }

    private void ChangeStatePreservingGrab(StateCharacter nextState)
    {
        preserveGrabOnExit = true;
        stateMachine.ChangeState(nextState);
    }
}
