using UnityEngine;

public class DieState : CharacterState
{
    public DieState(CharacterCoordinator character, CharacterStateMachine sm) : base(character, sm) { }

    public override void Enter()
    {
        character.Grab?.ReleaseGrabbedTarget();
        character.Health.OnDeath();
    }
}
