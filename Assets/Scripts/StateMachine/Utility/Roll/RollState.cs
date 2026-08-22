using UnityEngine;

public class RollState : PlayerState
{
    private static readonly int RollAnimation = Animator.StringToHash("Base Layer.Roll");

    private enum Phase { Startup, Active, Recovery }

    private float Startup = 0.04f;
    private float Active = 0.18f;
    private float Recovery = 0.14f;
    private float ChainWindow = 0.08f;
    private float DirectionDeadzone = 0.2f;

    private Phase phase;
    private float timer;
    private float rollDirection;
    private Color originalSpriteColor;

    public RollState(PlayerController character, StateMachine sm) : base(character, sm) { }

    public override void Enter()
    {
        if (character.Sprite != null)
            originalSpriteColor = character.Sprite.color;

        if (!BeginRoll())
            ReturnToLocomotion();
    }

    public override void Exit()
    {
        character.Roll.EndCharacterCollisionPassThrough();
        SetIntangible(false);
        character.Roll.CompleteRoll();
    }

    public override void Update()
    {
        if (character.IsDead)
        {
            stateMachine.ChangeState(StateCharacter.Die);
            return;
        }

        timer += Time.deltaTime;

        switch (phase)
        {
            case Phase.Startup:
                if (timer >= Startup)
                {
                    phase = Phase.Active;
                    timer = 0f;
                    character.Roll.BeginCharacterCollisionPassThrough();
                    SetIntangible(true);
                }
                break;

            case Phase.Active:
                if (timer >= Active)
                {
                    phase = Phase.Recovery;
                    timer = 0f;
                    character.Roll.EndCharacterCollisionPassThrough();
                    SetIntangible(false);
                }
                break;

            case Phase.Recovery:
                if (timer >= Recovery - ChainWindow && character.EvadePressed)
                {
                    character.ConsumeEvadeInput();

                    if (character.IsGrounded && BeginRoll())
                        return;
                }

                if (timer >= Recovery)
                    ReturnToLocomotion();
                break;
        }
    }

    public override void FixedUpdate()
    {
        if (phase == Phase.Active)
        {
            character.Roll.TrackSafeCollisionPosition();
            character.Movement.ApplyRoll(rollDirection, character.stats.dodgeSpeed);
        }
        else
            character.Movement.StopHorizontalMovement();
    }

    private bool BeginRoll()
    {
        if (!character.Roll.TryStartRoll())
            return false;

        phase = Phase.Startup;
        timer = 0f;
        SetIntangible(false);

        float inputX = character.MoveInput.x;
        rollDirection = Mathf.Abs(inputX) >= DirectionDeadzone
            ? Mathf.Sign(inputX)
            : Mathf.Sign(character.transform.localScale.x);

        if (Mathf.Approximately(rollDirection, 0f))
            rollDirection = 1f;

        character.TryPlayAnimation(RollAnimation);
        return true;
    }

    private void ReturnToLocomotion()
    {
        if (character.IsGrounded)
        {
            stateMachine.ChangeState(Mathf.Abs(character.MoveInput.x) > 0.01f
                ? StateCharacter.Move
                : StateCharacter.Idle);
            return;
        }

        stateMachine.Jump.PrepareReentry();
        stateMachine.ChangeState(StateCharacter.Jump);
    }

    private void SetIntangible(bool intangible)
    {
        character.IsIntangible = intangible;

        if (character.Sprite == null)
            return;

        character.Sprite.color = intangible
            ? new Color(originalSpriteColor.r, originalSpriteColor.g, originalSpriteColor.b, originalSpriteColor.a * 0.35f)
            : originalSpriteColor;
    }
}
