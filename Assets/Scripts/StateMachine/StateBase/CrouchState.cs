using UnityEngine;

public class CrouchState : PlayerState
{
    // How much to squish the sprite vertically (0.55 = 55% of original height)
    private const float CrouchScaleY = 0.55f;

    private Vector3 originalSpriteScale;

    public CrouchState(PlayerController character, StateMachine sm) : base(character, sm) { }

    public override void Enter()
    {
        if (character.Sprite != null)
        {
            originalSpriteScale = character.Sprite.transform.localScale;
            character.Sprite.transform.localScale = new Vector3(
                originalSpriteScale.x,
                originalSpriteScale.y * CrouchScaleY,
                originalSpriteScale.z);
        }
    }

    public override void Exit()
    {
        if (character.Sprite != null)
            character.Sprite.transform.localScale = originalSpriteScale;
    }

    public override void Update()
    {
        if (character.IsDead)
        {
            stateMachine.ChangeState(StateCharacter.Die);
            return;
        }

        // Jump from crouch
        if (character.JumpPressed && character.IsGrounded)
        {
            stateMachine.ChangeState(StateCharacter.Jump);
            return;
        }

        // Exit crouch when S released or no longer grounded
        bool stillPressingDown = character.MoveInput.y < -character.stats.tiltThreshold;
        if (!stillPressingDown || !character.IsGrounded)
        {
            stateMachine.ChangeState(Mathf.Abs(character.MoveInput.x) > 0.01f
                ? StateCharacter.Move
                : StateCharacter.Idle);
        }
    }

    public override void FixedUpdate()
    {
        character.Movement.StopHorizontalMovement();
    }
}
