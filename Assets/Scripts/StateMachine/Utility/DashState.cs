using UnityEngine;

public class DashState : CharacterState
{
    private enum Phase { Startup, Active, Recovery }

    private const float StartupDuration = 0.04f;
    private const float ChainWindow = 0.08f;

    private Phase phase;
    private float timer;
    private Color originalSpriteColor;

    public DashState(CharacterCoordinator character, CharacterStateMachine sm) : base(character, sm) { }

    public override void Enter()
    {
        if (character.Sprite != null)
            originalSpriteColor = character.Sprite.color;

        BeginDash();
    }

    public override void Update()
    {
        if (character.IsDead)
        {
            stateMachine.ChangeState(character.States.Die);
            return;
        }

        timer += Time.deltaTime;

        if (character.AttackInput)
        {
            character.ActiveInput?.ConsumeAttack();
            stateMachine.ChangeState(character.States.DashAttack);
            return;
        }

        switch (phase)
        {
            case Phase.Startup:
                if (timer >= StartupDuration)
                {
                    phase = Phase.Active;
                    timer = 0f;
                    character.Dash.BeginCharacterCollisionPassThrough();
                    SetIntangible(true);
                }
                break;

            case Phase.Active:
                if (timer >= character.stats.dashDuration)
                {
                    phase = Phase.Recovery;
                    timer = 0f;
                    character.Dash.EndCharacterCollisionPassThrough();
                    SetIntangible(false);
                }
                break;

            case Phase.Recovery:
                if (timer >= character.stats.dashRecovery - ChainWindow && character.IsGrounded && character.DashPressed &&
                    character.Dash.TryStartDash(character.MoveInput.x))
                {
                    character.ActiveInput?.ConsumeDash();
                    BeginDash();
                    return;
                }

                if (timer >= character.stats.dashRecovery)
                    ReturnToLocomotion();
                break;
        }
    }

    public override void FixedUpdate()
    {
        if (phase == Phase.Active)
        {
            character.Dash.TrackSafeCollisionPosition();
            character.Movement.ApplyHorizontalDash(character.Dash.Direction, character.stats.dashSpeed);
        }
        else
            character.Movement.StopHorizontalMovement();
    }

    public override void Exit()
    {
        character.Dash.EndCharacterCollisionPassThrough();
        SetIntangible(false);
        character.Dash.CompleteDash();
        character.Movement.StopHorizontalMovement();
    }

    private void BeginDash()
    {
        phase = Phase.Startup;
        timer = 0f;
        SetIntangible(false);
        character.Animation.TryPlay("Move");
    }

    private void ReturnToLocomotion()
    {
        if (!character.IsGrounded)
        {
            character.States.Jump.PrepareReentry();
            stateMachine.ChangeState(character.States.Jump);
            return;
        }

        stateMachine.ChangeState(Mathf.Abs(character.MoveInput.x) > 0.01f
            ? character.States.Move
            : character.States.Idle);
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
