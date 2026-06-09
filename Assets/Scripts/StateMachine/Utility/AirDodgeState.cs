using UnityEngine;

public class AirDodgeState : PlayerState
{
    private enum Phase { Active, Recovery }
    private Phase phase;
    private float timer;

    private const float Active   = 0.22f; 
    private const float Recovery = 0.18f;
    private const float DashSpeed = 11f;

    private Vector2 dashDirection;

    public AirDodgeState(PlayerController character, StateMachine sm) : base(character, sm) { }

    public override void Enter()
    {
        phase = Phase.Active;
        timer = 0f;

        character.IsIntangible = true;
        character.UseAirDodge();

        
        Vector2 input = character.MoveInput;
        dashDirection = input.magnitude > 0.15f ? input.normalized : Vector2.zero;

        
        character.Movement.StopAllMovement();

        if (character.Sprite != null)
            character.Sprite.color = new Color(1f, 1f, 1f, 0.35f);
    }

    public override void Exit()
    {
        character.IsIntangible = false;
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

        timer += Time.deltaTime;

        switch (phase)
        {
            case Phase.Active:
                if (timer >= Active)
                {
                    phase = Phase.Recovery;
                    timer = 0f;
                    character.IsIntangible = false;
                    if (character.Sprite != null)
                        character.Sprite.color = Color.white;
                }
                break;

            case Phase.Recovery:
                if (timer >= Recovery)
                {
                    if (character.IsGrounded)
                    {
                        stateMachine.ChangeState(StateCharacter.Idle);
                    }
                    else
                    {
                        // Re-enter aerial state without re-applying jump force
                        stateMachine.Jump.PrepareReentry();
                        stateMachine.ChangeState(StateCharacter.Jump);
                    }
                }
                break;
        }
    }

    public override void FixedUpdate()
    {
        if (phase == Phase.Active && dashDirection != Vector2.zero)
            character.Movement.ApplyDirectionalDash(dashDirection, DashSpeed);
        else
            character.Movement.ClampFallSpeed(); // let gravity work normally
    }
}
