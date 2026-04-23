using UnityEngine;

public class ParryState : PlayerState
{
    private float startup = 0.05f;
    private float activeWindow = 0.2f;
    private float recovery = 0.3f;

    private enum Phase { Startup, Active, Recovery }
    private Phase _phase;
    private float _timer;

    public ParryState(PlayerController player, StateMachine sm) : base(player, sm) { }

    public override void Enter()
    {
        _timer = 0f;
        _phase = Phase.Startup;
        player.StopHorizontalMovement();
        player.IsParrying = false;
    }

    public override void Update()
    {
        if (player.IsDead) return;

        _timer += Time.deltaTime;

        switch (_phase)
        {
            case Phase.Startup:
                if (_timer >= startup)
                {
                    _phase = Phase.Active;
                    _timer = 0f;
                    player.IsParrying = true;

                    player.GetComponent<SpriteRenderer>().color = Color.cyan;
                }
                break;

            case Phase.Active:
                if (_timer >= activeWindow)
                {
                    _phase = Phase.Recovery;
                    _timer = 0f;
                    player.IsParrying = false;

                    player.GetComponent<SpriteRenderer>().color = Color.white;
                }
                break;

            case Phase.Recovery:
                if (_timer >= recovery)
                {
                    stateMachine.ChangeState(player.IdleState);
                }
                break;
        }
    }

    public override void Exit()
    {
        player.IsParrying = false;
        player.GetComponent<SpriteRenderer>().color = Color.white;
    }
}
