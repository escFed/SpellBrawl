using UnityEngine;

public class ThrowState : CharacterState
{
    private float timer;
    private bool released;
    private ThrowDirection direction;
    private ThrowStats stats;

    public ThrowState(CharacterCoordinator character, CharacterStateMachine sm) : base(character, sm) { }

    public override void Enter()
    {
        base.Enter();

        timer = 0f;
        released = false;
        direction = character.Grab.ResolveThrowDirection();
        stats = character.Grab.GetThrowStats(direction);
        character.Movement.StopAllMovement();
        PlayThrowAnimation();
    }

    private string GetAnimationName(ThrowDirection direction)
    {
        return direction switch
        {
            ThrowDirection.Back => "BackThrow",
            ThrowDirection.Down => "DownThrow",
            ThrowDirection.Up => "UpThrow",
            _ => "ForwardThrow"
        };
    }

    private void PlayThrowAnimation()
    {
        string animationName = GetAnimationName(direction);
        bool animationPlayed = character.Animation.TryPlay(animationName);
        Debug.Log(
            $"[Throw] Character='{character.name}' Direction={direction} Animation='{animationName}' " +
            $"StateFound={animationPlayed}",
            character);
    }

    public override void Update()
    {
        if (stats == null)
        {
            character.Grab.ReleaseGrabbedTarget();
            stateMachine.ChangeState(character.States.Idle);
            return;
        }

        if (!character.Grab.HasGrabbedTarget && !released)
        {
            stateMachine.ChangeState(character.States.Idle);
            return;
        }

        timer += Time.deltaTime;

        if (!released && timer >= stats.releaseDelay)
        {
            character.Grab.ApplyThrow(direction);
            released = true;
        }

        if (timer >= stats.releaseDelay + stats.recovery)
        {
            stateMachine.ChangeState(Mathf.Abs(character.MoveInput.x) > 0.01f ? character.States.Move : character.States.Idle);
        }
    }

    public override void FixedUpdate()
    {
        character.Movement.StopAllMovement();

        if (!released)
            character.Grab.UpdateGrabbedTargetPosition();
    }

    public override void Exit()
    {
        if (!released)
            character.Grab.ReleaseGrabbedTarget();
    }
}
