using UnityEngine;

public class DieState : PlayerState
{
    public DieState(PlayerController character, StateMachine sm) : base(character, sm) { }

    public override void Enter()
    {
        character.Grab?.ReleaseGrabbedTarget();
        character.Health.OnDeath();
    }
}
