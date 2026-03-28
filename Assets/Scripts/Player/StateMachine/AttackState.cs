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

    protected AttackState(PlayerController player, StateMachine sm) : base(player, sm) { }

    public override void Enter()
    {
        _phase = Phase.Startup;
        _timer = 0f;

        player.StopHorizontalMovement();
    }

    public override void Update()
    {
        if (player.IsDead)
        {
            CloseHitbox();
            stateMachine.ChangeState(player.DieState);
            return;
        }

        _timer += Time.deltaTime;

        switch (_phase)
        {
            case Phase.Startup:
                if (_timer >= Startup) EnterActive();
                break;

            case Phase.Active:
                if (_timer >= Active) EnterRecovery();
                break;

            case Phase.Recovery:
                if (_timer >= Recovery) EnterCooldown();
                break;

            case Phase.Cooldown:
                if (_timer >= Cooldown)
                {
                    if (player.AttackInput)
                    {
                        stateMachine.ChangeState(player.ResolveAttackState());
                    }
                    else if (Mathf.Abs(player.MoveInput.x) > 0.01f)
                    {
                        stateMachine.ChangeState(player.MoveState);
                    }
                    else
                    {
                        stateMachine.ChangeState(player.IdleState);
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
