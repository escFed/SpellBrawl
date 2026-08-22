using UnityEngine;

public class ThrowState : PlayerState
{
    private float timer;
    private bool released;
    private ThrowDirection direction;
    private ThrowStats stats;

    public ThrowState(PlayerController character, StateMachine sm) : base(character, sm) { }

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
        Animator animator = character.Anim;
        string controllerName = animator != null && animator.runtimeAnimatorController != null
            ? animator.runtimeAnimatorController.name
            : "None";
        string layerName = animator != null && animator.layerCount > 0 ? animator.GetLayerName(0) : "None";
        bool hasAnimationState = animator != null && animator.layerCount > 0 &&
                                 animator.HasState(0, Animator.StringToHash($"{layerName}.{animationName}"));

        string message =
            $"[Throw] Character='{character.name}' Direction={direction} Animation='{animationName}' " +
            $"Controller='{controllerName}' Layer='{layerName}' StateFound={hasAnimationState}";

        if (hasAnimationState)
            Debug.Log(message, character);
        else
            Debug.LogWarning(message, character);

        if (animator != null)
            animator.Play(animationName, 0, 0f);
    }

    public override void Update()
    {
        if (stats == null)
        {
            character.Grab.ReleaseGrabbedTarget();
            stateMachine.ChangeState(StateCharacter.Idle);
            return;
        }

        if (!character.Grab.HasGrabbedTarget && !released)
        {
            stateMachine.ChangeState(StateCharacter.Idle);
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
            stateMachine.ChangeState(Mathf.Abs(character.MoveInput.x) > 0.01f ? StateCharacter.Move : StateCharacter.Idle);
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
