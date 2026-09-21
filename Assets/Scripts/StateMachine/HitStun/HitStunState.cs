using UnityEngine;

public class HitStunState : CharacterState
{
    private static int HitAnimation = Animator.StringToHash("Base Layer.Hit");
    private static int StrongHitAnimation = Animator.StringToHash("Base Layer.StrongHit");
    private static int StunnedAnimation = Animator.StringToHash("Base Layer.Stunned");
    private static readonly int ReactionSpeed = Animator.StringToHash("HitReactionSpeed");

    private float pendingDuration;
    private HitReaction pendingReaction;

    public float TimeRemaining { get; private set; }

    public HitStunState(CharacterCoordinator character, CharacterStateMachine stateMachine) : base(character, stateMachine) { }

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

        stateMachine.ChangeState(character.States.HitStun);
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

        ICharacterState recoveryState = character.IsGrounded
            ? character.States.Idle
            : character.States.Jump;

        if (recoveryState == character.States.Jump)
            character.States.Jump.PrepareReentry();

        stateMachine.ChangeState(recoveryState);
    }

    public override void Exit()
    {
        TimeRemaining = 0f;
        character.Animation.TrySetFloat(ReactionSpeed, 1f);
    }

    private void PlayReaction(HitReaction reaction)
    {
        float threshold = character.stats != null ? character.stats.strongHitStunThreshold : 0.5f;
        if (reaction == HitReaction.Hit && TimeRemaining >= Mathf.Max(0.01f, threshold))
            reaction = HitReaction.StrongHit;

        int animation = reaction switch
        {
            HitReaction.StrongHit => StrongHitAnimation,
            HitReaction.Stunned => StunnedAnimation,
            _ => HitAnimation
        };

        // The speed parameter belongs only to reaction states, never to locomotion or attacks.
        bool canAdjustSpeed = character.Animation.TrySetFloat(ReactionSpeed, 1f);
        if (!character.Animation.TryPlay(animation) || !canAdjustSpeed)
            return;

        // Resolve the newly selected state's actual motion length before fitting short stuns.
        // Reaction clips are presentation-only and must not contain gameplay animation events.
        character.Animation.TryFitCurrentStateToDuration(animation, ReactionSpeed, TimeRemaining);
    }
}
