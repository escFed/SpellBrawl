using UnityEngine;

public class DodgeState : CharacterState
{
    private static int AirDodgeAnimation = Animator.StringToHash("Base Layer.AirDodge");

    private enum Phase { Startup, Active, Recovery }

    private float Startup = 0.04f;
    private float Active = 0.22f;
    private float Recovery = 0.18f;
    private float DirectionDeadzone = 0.2f;
    private float DashSpeed = 11f;
    private Phase phase;
    private float timer;
    private Vector2 dodgeDirection;
    private Color originalSpriteColor;
    private readonly DirectionalDodgeTrail directionalTrail;

    public DodgeState(CharacterCoordinator character, CharacterStateMachine sm) : base(character, sm)
    {
        directionalTrail = character.GetComponent<DirectionalDodgeTrail>();
    }

    public override void Enter()
    {
        if (character.Sprite != null)
            originalSpriteColor = character.Sprite.color;

        if (!BeginDodge())
            ReturnToLocomotion();
    }

    public override void Exit()
    {
        directionalTrail?.Stop();
        SetIntangible(false);
        character.Dodge.CompleteDodge();
    }

    public override void Update()
    {
        if (character.IsDead)
        {
            stateMachine.ChangeState(character.States.Die);
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
                    directionalTrail?.Play(Active);
                }
                break;

            case Phase.Active:
                if (timer >= Active)
                {
                    phase = Phase.Recovery;
                    timer = 0f;
                    SetIntangible(false);
                    directionalTrail?.Stop();
                    character.Movement.StopAllMovement();
                }
                break;

            case Phase.Recovery:
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
        directionalTrail?.SetDirection(dodgeDirection);
        directionalTrail?.Stop();
        SetIntangible(false);
        character.Movement.StopAllMovement();
        character.Animation.TryPlay(AirDodgeAnimation);
        return true;
    }

    private Vector2 ResolveCardinalDirection(Vector2 input)
    {
        if (input.magnitude < DirectionDeadzone)
        {
            return new Vector2(character.Combat.FacingSign, 0f);
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
                ? character.States.Move
                : character.States.Idle);
            return;
        }

        character.States.Jump.PrepareReentry();
        stateMachine.ChangeState(character.States.Jump);
    }

    private void SetIntangible(bool intangible)
    {
        character.Health.SetIntangible(intangible);

        if (character.Sprite == null)
            return;

        character.Sprite.color = intangible
            ? new Color(originalSpriteColor.r, originalSpriteColor.g, originalSpriteColor.b, originalSpriteColor.a * 0.35f)
            : originalSpriteColor;
    }
}
