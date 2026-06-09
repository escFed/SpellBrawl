using UnityEngine;

public class DodgeState : PlayerState
{
    private enum Phase { Startup, Active, Recovery }
    private Phase phase;
    private float timer;

    
    private const float Startup  = 0.04f;
    private const float Active   = 0.18f; 
    private const float Recovery = 0.14f;

    private float dashDirection;

    public DodgeState(PlayerController character, StateMachine sm) : base(character, sm) { }

    public override void Enter()
    {
        phase = Phase.Startup;
        timer = 0f;

        
        dashDirection = Mathf.Sign(character.MoveInput.x);
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
            case Phase.Startup:
                if (timer >= Startup)
                {
                    phase = Phase.Active;
                    timer = 0f;
                    character.IsIntangible = true;
                    if (character.Sprite != null)
                        character.Sprite.color = new Color(1f, 1f, 1f, 0.35f);
                }
                break;

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
                    stateMachine.ChangeState(StateCharacter.Idle);
                break;
        }
    }

    public override void FixedUpdate()
    {
        if (phase == Phase.Active)
            character.Movement.ApplyDodge(dashDirection, character.stats.dodgeSpeed);
        else
            character.Movement.StopHorizontalMovement();
    }
}
