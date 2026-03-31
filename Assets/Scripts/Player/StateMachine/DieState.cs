using UnityEngine;

public class DieState : PlayerState
{
    public DieState(PlayerController player, StateMachine sm) : base(player, sm) { }

    public override void Enter()
    {
        player.OnDeath();
    }
}
