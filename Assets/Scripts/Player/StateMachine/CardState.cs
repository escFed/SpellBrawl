using UnityEngine;

public class CardState : PlayerState
{
    private float _timer;
    private float _recoveryTime = 0.4f;
    private ICardable _cardToUse;

    public CardState(PlayerController player, StateMachine stateMachine) : base(player, stateMachine) { }

    public void SetCard(ICardable card, float recovery)
    {
        _cardToUse = card;
        _recoveryTime = recovery;
    }

    public override void Enter()
    {
        _timer = 0f;

        player.StopHorizontalMovement();

        if (_cardToUse != null)
        {
            _cardToUse.ExecuteCard(player);
        }
    }

    public override void Update()
    {
        if (player.IsDead)
        {
            stateMachine.ChangeState(player.DieState);
            return;
        }

        _timer += Time.deltaTime;

        if (_timer >= _recoveryTime)
        {
            if (Mathf.Abs(player.MoveInput.x) > 0.01f)
            {
                stateMachine.ChangeState(player.MoveState);
            }
            else
            {
                stateMachine.ChangeState(player.IdleState);
            }
        }
    }
}
