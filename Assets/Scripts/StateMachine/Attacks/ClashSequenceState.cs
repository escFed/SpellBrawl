using UnityEngine;

public class ClashSequenceState : CharacterState
{
    private static string[] AnimationStates =
    {
        "FTilt",
        "UpTilt",
        "DownTilt",
        "Jab"
    };

    private CharacterCoordinator opponent;
    private Vector2 launchVelocity;
    private float stepDuration;
    private float launchStun;
    private float elapsed;
    private int animationIndex;

    public ClashSequenceState(CharacterCoordinator character, CharacterStateMachine stateMachine): base(character, stateMachine) { }

    public void Prepare(CharacterCoordinator clashOpponent, Vector2 finalLaunchVelocity, float animationStepDuration, float finalLaunchStun)
    {
        opponent = clashOpponent;
        launchVelocity = finalLaunchVelocity;
        stepDuration = Mathf.Max(0.01f, animationStepDuration);
        launchStun = Mathf.Max(0f, finalLaunchStun);
    }

    public override void Enter()
    {
        elapsed = 0f;
        animationIndex = 0;
        character.Combat.PrepareForClash();

        if (opponent == null || opponent.IsDead)
        {
            RecoverWithoutLaunch();
            return;
        }

        character.Combat.FaceDirection(opponent.transform.position.x - character.transform.position.x);
        character.Animation.TryPlay(AnimationStates[animationIndex]);
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
            character.Animation.TryPlay(AnimationStates[animationIndex]);
        }

        if (elapsed >= stepDuration * AnimationStates.Length)
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
        opponent = null;
        launchVelocity = Vector2.zero;
    }

    private void RecoverWithoutLaunch()
    {
        ICharacterState recoveryState = character.IsGrounded? character.States.Idle: character.States.Jump;

        if (recoveryState == character.States.Jump)
            character.States.Jump.PrepareReentry();

        stateMachine.ChangeState(recoveryState);
    }
}
