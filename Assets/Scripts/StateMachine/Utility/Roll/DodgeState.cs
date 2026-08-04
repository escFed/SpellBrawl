using UnityEngine;

public class DodgeState : PlayerState
{
    private static readonly int AirDodgeAnimation = Animator.StringToHash("Base Layer.AirDodge");

    private enum Phase { Startup, Active, Recovery }

    private const float Startup = 0.04f;
    private const float Active = 0.22f;
    private const float Recovery = 0.18f;
    private const float ChainWindow = 0.08f;
    private const float DirectionDeadzone = 0.2f;
    private const float DashSpeed = 11f;

    private Phase phase;
    private float timer;
    private Vector2 dodgeDirection;
    private Color originalSpriteColor;

    public DodgeState(PlayerController character, StateMachine sm) : base(character, sm) { }

    public override void Enter()
    {
        if (character.Sprite != null)
            originalSpriteColor = character.Sprite.color;

        if (!BeginDodge())
            ReturnToLocomotion();
    }

    public override void Exit()
    {
        SetIntangible(false);
        character.Dodge.CompleteDodge();
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
                    SetIntangible(true);
                }
                break;

            case Phase.Active:
                if (timer >= Active)
                {
                    phase = Phase.Recovery;
                    timer = 0f;
                    SetIntangible(false);
                    character.Movement.StopAllMovement();
                }
                break;

            case Phase.Recovery:
                if (timer >= Recovery - ChainWindow && character.EvadePressed)
                {
                    character.ConsumeEvadeInput();

                    if (!character.IsGrounded && BeginDodge())
                        return;
                }

                if (timer >= Recovery)
                    ReturnToLocomotion();
                break;
        }
    }

    public override void FixedUpdate()
    {
        switch (phase)
        {
            case Phase.Startup:
                character.Movement.StopAllMovement();
                break;

            case Phase.Active:
                character.Movement.ApplyDirectionalDash(dodgeDirection, DashSpeed);
                break;

            case Phase.Recovery:
                character.Movement.ClampFallSpeed();
                break;
        }
    }

    private bool BeginDodge()
    {
        if (!character.Dodge.TryStartDodge())
            return false;

        phase = Phase.Startup;
        timer = 0f;
        dodgeDirection = ResolveCardinalDirection(character.MoveInput);
        SetIntangible(false);
        character.Movement.StopAllMovement();
        character.TryPlayAnimation(AirDodgeAnimation);
        return true;
    }

    private Vector2 ResolveCardinalDirection(Vector2 input)
    {
        if (input.magnitude < DirectionDeadzone)
        {
            float facingDirection = Mathf.Sign(character.transform.localScale.x);
            return new Vector2(Mathf.Approximately(facingDirection, 0f) ? 1f : facingDirection, 0f);
        }

        if (Mathf.Abs(input.x) >= Mathf.Abs(input.y))
            return new Vector2(Mathf.Sign(input.x), 0f);

        return new Vector2(0f, Mathf.Sign(input.y));
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
