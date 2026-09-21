using UnityEngine;

public class PummelState : CharacterState
{
    private GrabStats stats;
    private float timer;
    private bool preserveGrabOnExit;

    public PummelState(CharacterCoordinator character, CharacterStateMachine sm) : base(character, sm) { }

    public void SetGrabStats(GrabStats grabStats) => stats = grabStats;

    public override void Enter()
    {
        base.Enter();

        timer = 0f;
        preserveGrabOnExit = false;
        character.Movement.StopAllMovement();

        if (stats == null || !character.Grab.HasGrabbedTarget)
        {
            stateMachine.ChangeState(character.States.Idle);
            return;
        }

        character.Animation.TryPlay("Pummel");
        character.Grab.ApplyPummel(stats);
    }

    public override void Update()
    {
        if (!character.Grab.HasGrabbedTarget)
        {
            stateMachine.ChangeState(character.States.Idle);
            return;
        }

        timer += Time.deltaTime;

        if (character.States.GrabHold.HasHoldExpired)
        {
            ChangeStatePreservingGrab(character.States.Throw);
            return;
        }

        if (timer >= Mathf.Max(0f, stats.pummelCooldown))
            ChangeStatePreservingGrab(character.States.GrabHold);
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

    private void ChangeStatePreservingGrab(ICharacterState nextState)
    {
        preserveGrabOnExit = true;
        stateMachine.ChangeState(nextState);
    }
}
