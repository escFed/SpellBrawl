using UnityEngine;

public class ClashSequenceState : CharacterState
{
    private const int HeavyStepIndex = 3;
    private const float HeavyStepDurationMultiplier = 2f;
    private static readonly string[] AnimationStates =
    {
        "Jab",
        "Jab2",
        "Jab3",
        "ForwardHeavy"
    };

    private CharacterCoordinator opponent;
    private Vector2 launchVelocity;
    private float stepDuration;
    private float launchStun;
    private float elapsed;
    private int animationIndex;
    private int lastSoundIndex;
    private bool playsSequenceAudio;

    public ClashSequenceState(CharacterCoordinator character, CharacterStateMachine stateMachine): base(character, stateMachine) { }

    public void Prepare(CharacterCoordinator clashOpponent, Vector2 finalLaunchVelocity, float animationStepDuration, float finalLaunchStun, bool playSequenceAudio)
    {
        opponent = clashOpponent;
        launchVelocity = finalLaunchVelocity;
        stepDuration = Mathf.Max(0.01f, animationStepDuration);
        launchStun = Mathf.Max(0f, finalLaunchStun);
        playsSequenceAudio = playSequenceAudio;
    }

    public override void Enter()
    {
        elapsed = 0f;
        animationIndex = 0;
        lastSoundIndex = -1;
        character.Combat.PrepareForClash();

        if (opponent == null || opponent.IsDead)
        {
            RecoverWithoutLaunch();
            return;
        }

        character.Combat.FaceDirection(opponent.transform.position.x - character.transform.position.x);
        PlayStepAnimation();
    }

    public override void Update()
    {
        if (opponent == null || opponent.IsDead || character.IsDead)
        {
            RecoverWithoutLaunch();
            return;
        }

        elapsed += Time.deltaTime;
        int nextAnimationIndex = Mathf.Min(Mathf.FloorToInt(elapsed / stepDuration), AnimationStates.Length - 1);

        if (nextAnimationIndex > animationIndex)
        {
            animationIndex = nextAnimationIndex;
            PlayStepAnimation();
        }

        if (playsSequenceAudio)
        {
            while (lastSoundIndex < HeavyStepIndex && elapsed >= GetSoundTime(lastSoundIndex + 1))
            {
                lastSoundIndex++;
                AudioClip clip = lastSoundIndex == HeavyStepIndex
                    ? character.stats?.forwardHeavyAttack?.hitSound
                    : character.stats?.jabAttack?.hitSound;
                CombatFeedback.PlayHitSound(clip);
                if (lastSoundIndex == HeavyStepIndex)
                {
                    Vector2 impactPoint = (character.transform.position + opponent.transform.position) * 0.5f;
                    CombatFeedback.PlayImpact(impactPoint, Vector2.up * launchVelocity.y, HitReaction.Stunned, Color.white);
                }
            }
        }

        if (elapsed >= stepDuration * (HeavyStepIndex + HeavyStepDurationMultiplier))
            character.Combat.LaunchFromClash(launchVelocity, launchStun);
    }

    public override void FixedUpdate()
    {
        character.Movement.StopAllMovement();
    }

    public override void Exit()
    {
        elapsed = 0f;
        animationIndex = 0;
        lastSoundIndex = -1;
        playsSequenceAudio = false;
        opponent = null;
        launchVelocity = Vector2.zero;
    }

    private void PlayStepAnimation()
    {
        string fallback = animationIndex == HeavyStepIndex ? "FTilt" : "Jab";
        character.Animation.TryPlay(AnimationStates[animationIndex], fallback);
    }

    private float GetSoundTime(int stepIndex)
    {
        float stepStart = stepIndex * stepDuration;
        return stepStart + (stepIndex == HeavyStepIndex ? stepDuration * 0.75f : stepDuration * 0.25f);
    }

    private void RecoverWithoutLaunch()
    {
        ICharacterState recoveryState = character.IsGrounded? character.States.Idle: character.States.Jump;

        if (recoveryState == character.States.Jump)
            character.States.Jump.PrepareReentry();

        stateMachine.ChangeState(recoveryState);
    }
}
