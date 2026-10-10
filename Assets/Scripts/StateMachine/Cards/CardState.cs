using UnityEngine;

public class CardState : CharacterState
{
    private float _timer;
    private float _recoveryTime = 0.4f;
    private ICardable _cardToUse;

    public CardState(CharacterCoordinator character, CharacterStateMachine stateMachine) : base(character, stateMachine) { }

    public void SetCard(ICardable card, float recovery)
    {
        _cardToUse = card;
        _recoveryTime = recovery;
    }

    public override void Enter()
    {
        character.Health.CancelRespawnProtection();
        _timer = 0f;

        if (character.IsGrounded)
            character.Movement.StopHorizontalMovement();

        if (_cardToUse != null)
        {
            _cardToUse.ExecuteCard(character);
        }
    }

    public override void Update()
    {
        if (character.IsDead)
        {
            stateMachine.ChangeState(character.States.Die);
            return;
        }

        character.HandleAirborneMovementInput();
        _timer += Time.deltaTime;

        if (_timer >= _recoveryTime)
        {
            if (!character.IsGrounded)
            {
                character.States.Jump.PrepareReentry();
                stateMachine.ChangeState(character.States.Jump);
                return;
            }

            if (Mathf.Abs(character.MoveInput.x) > 0.01f)
            {
                stateMachine.ChangeState(character.States.Move);
            }
            else
            {
                stateMachine.ChangeState(character.States.Idle);
            }
        }
    }

    public override void FixedUpdate()
    {
        if (character.IsGrounded)
            character.Movement.StopHorizontalMovement();
        else
        {
            character.Movement.ApplyHorizontalMovement();
            character.Movement.ClampFallSpeed();
        }
    }
}
