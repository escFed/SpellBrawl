using UnityEngine;

public abstract class AttackState : CharacterState
{
    protected AttackStats stats;
    private float timer;
    private bool hitboxOpen;
    private bool hitboxClose;
    public AttackState(CharacterCoordinator character, CharacterStateMachine sm, AttackStats attackStats) : base(character, sm)
    {
        stats = attackStats;
    }

    public override void Enter()
    {
        base.Enter();
        character.Health.CancelRespawnProtection();

        if (StopsHorizontalMovement)
            character.Movement.StopHorizontalMovement();

        timer = 0;
        hitboxOpen = false;
        hitboxClose = false;
        ReadyHitbox();
    }

    public override void Update()
    {
        if (AllowsAirDrift)
            character.HandleAirborneMovementInput();

        timer += Time.deltaTime;

        if (timer >= stats.startup && !hitboxOpen)
        {
            OpenHitbox();
            hitboxOpen = true;
        }

        if (timer >= stats.startup + stats.active && !hitboxClose)
        {
            CloseHitbox();
            hitboxClose = true;
        }

        if (timer >= stats.startup + stats.active + stats.recovery)
        {
            ICharacterState nextState = GetRecoveryState();
            if (nextState == character.States.Jump)
                character.States.Jump.PrepareReentry();

            stateMachine.ChangeState(nextState);
        }
    }

    public override void FixedUpdate()
    {
        if (AllowsAirDrift)
        {
            character.Movement.ApplyHorizontalMovement();
            character.Movement.ClampFallSpeed();
        }
    }

    public override void Exit()
    {
        if (hitboxOpen && !hitboxClose)
            CloseHitbox();
    }

    protected abstract void ReadyHitbox();
    protected abstract void OpenHitbox();
    protected abstract void CloseHitbox();

    protected virtual bool StopsHorizontalMovement => true;
    protected virtual bool AllowsAirDrift => false;
    protected float ElapsedTime => timer;

    protected virtual ICharacterState GetRecoveryState()
    {
        if (!character.IsGrounded)
            return character.States.Jump;

        return Mathf.Abs(character.MoveInput.x) > 0.01f
            ? character.States.Move
            : character.States.Idle;
    }
}
