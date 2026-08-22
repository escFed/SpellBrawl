using UnityEngine;

public class CardState : PlayerState
{
    private float _timer;
    private float _recoveryTime = 0.4f;
    private ICardable _cardToUse;

    public CardState(PlayerController character, StateMachine stateMachine) : base(character, stateMachine) { }

    public void SetCard(ICardable card, float recovery)
    {
        _cardToUse = card;
        _recoveryTime = recovery;
    }

    public override void Enter()
    {
        _timer = 0f;

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
            stateMachine.ChangeState(StateCharacter.Die);
            return;
        }

        _timer += Time.deltaTime;

        if (_timer >= _recoveryTime)
        {
            if (Mathf.Abs(character.MoveInput.x) > 0.01f)
            {
                stateMachine.ChangeState(StateCharacter.Move);
            }
            else
            {
                stateMachine.ChangeState(StateCharacter.Idle);
            }
        }
    }
}
