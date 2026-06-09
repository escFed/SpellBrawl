using UnityEngine;

public class ShieldState : PlayerState
{
    private static readonly Color ShieldColor = new Color(0.3f, 0.6f, 1f, 1f);

    public ShieldState(PlayerController character, StateMachine sm) : base(character, sm) { }

    public override void Enter()
    {
        character.IsShielding = true;
        if (character.Sprite != null)
            character.Sprite.color = ShieldColor;
    }

    public override void Exit()
    {
        character.IsShielding = false;
        if (character.Sprite != null)
            character.Sprite.color = Color.white;
    }

    public override void Update()
    {
        if (character.IsDead)
        {
            stateMachine.ChangeState(StateCharacter.Die);
            return;
        }

        // Jump out of shield
        if (character.JumpPressed)
        {
            stateMachine.ChangeState(StateCharacter.Jump);
            return;
        }

        // Exit shield when J is released
        if (!character.IsShieldHeld)
        {
            stateMachine.ChangeState(Mathf.Abs(character.MoveInput.x) > 0.01f
                ? StateCharacter.Move
                : StateCharacter.Idle);
            return;
        }

        // Dodge: A or D while shielding
        if (Mathf.Abs(character.MoveInput.x) > 0.01f)
        {
            stateMachine.ChangeState(StateCharacter.Dodge);
            return;
        }
    }

    public override void FixedUpdate()
    {
        character.Movement.StopHorizontalMovement();
    }
}
