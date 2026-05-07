using UnityEngine;

public abstract class AttackState : PlayerState
{
    protected abstract float Startup { get; }
    protected abstract float Active { get; }
    protected abstract float Recovery { get; }
    protected abstract float Cooldown { get; }

    protected abstract void OpenHitbox();
    protected abstract void CloseHitbox();

    private enum Phase { Startup, Active, Recovery, Cooldown }
    private Phase _phase;
    private float _timer;

    protected AttackState(PlayerController character, StateMachine sm) : base(character, sm) { }

    public override void Enter()
    {
        _phase = Phase.Startup;
        _timer = 0f;

        character.Movement.StopHorizontalMovement();
    }

    public override void Update()
    {
        if (character.IsDead)
        {
            CloseHitbox();
            stateMachine.ChangeState(StateCharacter.Die);
            return;
        }

        _timer += Time.deltaTime;

        switch (_phase)
        {
            case Phase.Startup:
                if (_timer >= (Startup / character.Combat.attackSpeedMultiplier)) EnterActive();
                break;

            case Phase.Active:
                if (_timer >= (Active / character.Combat.attackSpeedMultiplier)) EnterRecovery();
                break;

            case Phase.Recovery:
                if (_timer >= (Recovery / character.Combat.attackSpeedMultiplier)) EnterCooldown();
                break;

            case Phase.Cooldown:
                if (_timer >= (Cooldown / character.Combat.attackSpeedMultiplier))
                {
                    if (character.AttackInput)
                    {
                        stateMachine.ChangeState(character.Combat.ResolveAttackState());
                    }
                    else if (Mathf.Abs(character.MoveInput.x) > 0.01f)
                    {
                        stateMachine.ChangeState(StateCharacter.Move);
                    }
                    else
                    {
                        stateMachine.ChangeState(StateCharacter.Idle);
                    }
                }
                break;
        }
    }

    public override void Exit() => CloseHitbox();

    private void EnterActive() { _phase = Phase.Active; _timer = 0f; OpenHitbox(); }
    private void EnterRecovery() { _phase = Phase.Recovery; _timer = 0f; CloseHitbox(); }
    private void EnterCooldown() { _phase = Phase.Cooldown; _timer = 0f; }
}
