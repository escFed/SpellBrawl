using UnityEngine;

public class GrabHoldState : CharacterState
{
    private GrabStats stats;
    private float holdEndTime;
    private bool isNewHold;
    private bool directionArmed;
    private bool preserveGrabOnExit;

    public bool HasHoldExpired => Time.time >= holdEndTime;

    public GrabHoldState(CharacterCoordinator character, CharacterStateMachine sm) : base(character, sm) { }

    public void BeginHold(GrabStats grabStats)
    {
        stats = grabStats;
        float maxHoldDuration = stats != null ? stats.maxHoldDuration : 2f;
        holdEndTime = Time.time + Mathf.Max(0.1f, maxHoldDuration);
        isNewHold = true;
    }

    public override void Enter()
    {
        base.Enter();

        preserveGrabOnExit = false;
        character.Movement.StopAllMovement();
        character.Animation.TryPlay("Idle");

        if (isNewHold)
        {
            directionArmed = IsDirectionNeutral();
            character.ActiveInput?.ConsumeAttack();
            isNewHold = false;
        }
    }

    public override void Update()
    {
        if (!character.Grab.HasGrabbedTarget)
        {
            stateMachine.ChangeState(character.States.Idle);
            return;
        }

        if (HasHoldExpired)
        {
            ChangeStatePreservingGrab(character.States.Throw);
            return;
        }

        if (character.AttackInput)
        {
            character.ActiveInput?.ConsumeAttack();
            character.States.Pummel.SetGrabStats(stats);
            ChangeStatePreservingGrab(character.States.Pummel);
            return;
        }

        if (!directionArmed && IsDirectionNeutral())
            directionArmed = true;

        if (character.GrabInput)
        {
            character.ActiveInput?.ConsumeGrab();
            ChangeStatePreservingGrab(character.States.Throw);
            return;
        }

        if (directionArmed && !IsDirectionNeutral())
            ChangeStatePreservingGrab(character.States.Throw);
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
    }

    private void ChangeStatePreservingGrab(ICharacterState nextState)
    {
        preserveGrabOnExit = true;
        stateMachine.ChangeState(nextState);
    }

    private bool IsDirectionNeutral()
    {
        Vector2 directionInput = character.ActiveInput != null ? character.ActiveInput.CurrentDirection : Vector2.zero;

        return Mathf.Abs(directionInput.x) < character.stats.tiltThreshold &&
               Mathf.Abs(directionInput.y) < character.stats.tiltThreshold;
    }
}
