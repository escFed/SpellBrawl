public class StateMachine
{
    public IState CurrentState { get; private set; }

    public IdleState Idle { get; set; }
    public MoveState Move { get; set; }
    public JumpState Jump { get; set; }
    public JabState Jab { get; set; }
    public ForwardTiltState ForwardTilt { get; set; }
    public UpTiltState UpTilt { get; set; }
    public DownTiltState DownTilt { get; set; }
    public CardState Card { get; set; }
    public DieState Die { get; set; }
    public ParryState Parry { get; set; }

    public void ChangeState(StateCharacter character)
    {
        IState nextState = character switch
        {
            StateCharacter.Idle => Idle,
            StateCharacter.Move => Move,
            StateCharacter.Jump => Jump,
            StateCharacter.Jab => Jab,
            StateCharacter.ForwardTilt => ForwardTilt,
            StateCharacter.UpTilt => UpTilt,
            StateCharacter.DownTilt => DownTilt,
            StateCharacter.Card => Card,
            StateCharacter.Die => Die,
            StateCharacter.Parry => Parry,
            _ => null
        };

        if (nextState != null && nextState != CurrentState)
        {
            CurrentState?.Exit();
            CurrentState = nextState;
            CurrentState?.Enter();
        }
    }

    public void Update() => CurrentState?.Update();
    public void FixedUpdate() => CurrentState?.FixedUpdate();
}
