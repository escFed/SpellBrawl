using UnityEngine;

public class CardState : CharacterState
{
    private float _timer;
    private float _recoveryTime = 0.4f;
    private ICardable _cardToUse;
    private bool _allowMovement;
    private int _mobilePose;

    public CardState(CharacterCoordinator character, CharacterStateMachine stateMachine) : base(character, stateMachine) { }

    public void SetCard(ICardable card, float recovery)
    {
        _cardToUse = card;
        _recoveryTime = recovery;
        _allowMovement = card is IMobileCard;
    }

    public override void Enter()
    {
        character.Health.CancelRespawnProtection();
        _timer = 0f;
        _mobilePose = 0;

        if (character.IsGrounded && !_allowMovement)
            character.Movement.StopHorizontalMovement();

        if (_cardToUse != null)
        {
            _cardToUse.ExecuteCard(character);
        }

        if (_allowMovement)
            UpdateMobilePose();
    }

    public override void Update()
    {
        if (character.IsDead)
        {
            stateMachine.ChangeState(character.States.Die);
            return;
        }

        character.HandleAirborneMovementInput();
        if (_allowMovement)
        {
            if (character.JumpPressed && character.CanJump && character.TryPerformJump())
            {
                _mobilePose = 3;
                character.Animation.TryPlay("Jump");
            }
            else
            {
                UpdateMobilePose();
            }
        }
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
        {
            if (_allowMovement)
                character.Movement.ApplyHorizontalMovement();
            else
                character.Movement.StopHorizontalMovement();
        }
        else
        {
            character.Movement.ApplyHorizontalMovement();
            character.Movement.ClampFallSpeed();
        }
    }

    private void UpdateMobilePose()
    {
        int pose = !character.IsGrounded ? 3 : Mathf.Abs(character.MoveInput.x) > 0.01f ? 2 : 1;
        if (pose == _mobilePose)
            return;

        _mobilePose = pose;
        character.Animation.TryPlay(pose == 3 ? "Jump" : pose == 2 ? "Move" : "Idle");
    }
}
