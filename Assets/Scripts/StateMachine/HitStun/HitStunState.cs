using UnityEngine;

public class HitStunState : PlayerState
{
    private static readonly int HitAnimation = Animator.StringToHash("Base Layer.Hit");
    private static readonly int StrongHitAnimation = Animator.StringToHash("Base Layer.StrongHit");
    private static readonly int StunnedAnimation = Animator.StringToHash("Base Layer.Stunned");

    private float pendingDuration;
    private HitReaction pendingReaction;

    public float TimeRemaining { get; private set; }

    public HitStunState(PlayerController character, StateMachine stateMachine) : base(character, stateMachine) { }

    public void Apply(float duration, HitReaction reaction)
    {
        pendingDuration = Mathf.Max(0f, duration);
        pendingReaction = reaction;

        if (stateMachine.CurrentState == this)
        {
            TimeRemaining = Mathf.Max(TimeRemaining, pendingDuration);
            PlayReaction(pendingReaction);
            return;
        }

        stateMachine.ChangeState(StateCharacter.HitStun);
    }

    public override void Enter()
    {
        TimeRemaining = pendingDuration;
        PlayReaction(pendingReaction);
    }

    public override void Update()
    {
        TimeRemaining = Mathf.Max(0f, TimeRemaining - Time.deltaTime);
        if (TimeRemaining > 0f)
            return;

        StateCharacter recoveryState = character.IsGrounded
            ? StateCharacter.Idle
            : StateCharacter.Jump;
        stateMachine.ChangeState(recoveryState);
    }

    public override void Exit()
    {
        TimeRemaining = 0f;
    }

    private void PlayReaction(HitReaction reaction)
    {
        int animation = reaction switch
        {
            HitReaction.StrongHit => StrongHitAnimation,
            HitReaction.Stunned => StunnedAnimation,
            _ => HitAnimation
        };

        character.TryPlayAnimation(animation);
    }
}
