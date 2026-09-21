using UnityEngine;

public class ParryState : CharacterState
{
    private static int ParryAnimation = Animator.StringToHash("Base Layer.Parry");

    private float startup = 0.05f;
    private float activeWindow = 0.2f;
    private float recovery = 0.3f;

    private enum Phase { Startup, Active, Recovery }
    private Phase _phase;
    private float _timer;
    private Color originalSpriteColor;

    public ParryState(CharacterCoordinator character, CharacterStateMachine sm) : base(character, sm) { }

    public override void Enter()
    {
        base.Enter();
        character.Movement.StopHorizontalMovement();
        character.Animation.TryPlay(ParryAnimation);

        if (character.Sprite != null)
            originalSpriteColor = character.Sprite.color;

        _timer = 0f;
        _phase = Phase.Startup;
        character.Parry.SetParryWindowActive(false);
    }

    public override void Update()
    {
        if (character.IsDead) return;

        _timer += Time.deltaTime;

        switch (_phase)
        {
            case Phase.Startup:
                if (_timer >= startup)
                {
                    _phase = Phase.Active;
                    _timer = 0f;
                    character.Parry.SetParryWindowActive(true);

                    if (character.Sprite != null)
                        character.Sprite.color = new Color(Color.cyan.r, Color.cyan.g, Color.cyan.b, originalSpriteColor.a);
                }
                break;

            case Phase.Active:
                if (_timer >= activeWindow)
                {
                    _phase = Phase.Recovery;
                    _timer = 0f;
                    character.Parry.SetParryWindowActive(false);

                    RestoreSpriteColor();
                }
                break;

            case Phase.Recovery:
                if (_timer >= recovery)
                {
                    stateMachine.ChangeState(character.States.Idle);
                }
                break;
        }
    }

    public override void Exit()
    {
        character.Parry.SetParryWindowActive(false);
        RestoreSpriteColor();
    }

    private void RestoreSpriteColor()
    {
        if (character.Sprite != null)
            character.Sprite.color = originalSpriteColor;
    }
}
