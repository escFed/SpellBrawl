public class StateMachine
{
    public IState CurrentState { get; private set; }

    public IdleState Idle { get; set; }
    public MoveState Move { get; set; }
    public JumpState Jump { get; set; }
    public CrouchState Crouch { get; set; }
    public ShieldState Shield { get; set; }
    public RollState Roll { get; set; }
    public DodgeState Dodge { get; set; }
    public DashState Dash { get; set; }
    public JabState Jab { get; set; }
    public ForwardTiltState ForwardTilt { get; set; }
    public UpTiltState UpTilt { get; set; }
    public DownTiltState DownTilt { get; set; }
    public NeutralAirState NeutralAir { get; set; }
    public ForwardAirState ForwardAir { get; set; }
    public UpAirState UpAir { get; set; }
    public DownAirState DownAir { get; set; }
    public HeavyChargeState HeavyCharge { get; set; }
    public HeavyAttackState HeavyAttack { get; set; }
    public DashAttackState DashAttack { get; set; }
    public NormalGrabState Grab { get; set; }
    public PivotGrabState PivotGrab { get; set; }
    public DashGrabState DashGrab { get; set; }
    public GrabHoldState GrabHold { get; set; }
    public PummelState Pummel { get; set; }
    public ThrowState Throw { get; set; }
    public CardState Card { get; set; }
    public DieState Die { get; set; }
    public ParryState Parry { get; set; }
    public HitStunState HitStun { get; set; }

    public void ChangeState(StateCharacter character)
    {
        IState nextState = character switch
        {
            StateCharacter.Idle => Idle,
            StateCharacter.Move => Move,
            StateCharacter.Jump => Jump,
            StateCharacter.Crouch => Crouch,
            StateCharacter.Shield => Shield,
            StateCharacter.Roll => Roll,
            StateCharacter.Dodge => Dodge,
            StateCharacter.Dash => Dash,
            StateCharacter.Jab => Jab,
            StateCharacter.ForwardTilt => ForwardTilt,
            StateCharacter.UpTilt => UpTilt,
            StateCharacter.DownTilt => DownTilt,
            StateCharacter.NeutralAir => NeutralAir,
            StateCharacter.ForwardAir => ForwardAir,
            StateCharacter.UpAir => UpAir,
            StateCharacter.DownAir => DownAir,
            StateCharacter.HeavyCharge => HeavyCharge,
            StateCharacter.HeavyAttack => HeavyAttack,
            StateCharacter.DashAttack => DashAttack,
            StateCharacter.Grab => Grab,
            StateCharacter.PivotGrab => PivotGrab,
            StateCharacter.DashGrab => DashGrab,
            StateCharacter.GrabHold => GrabHold,
            StateCharacter.Pummel => Pummel,
            StateCharacter.Throw => Throw,
            StateCharacter.Card => Card,
            StateCharacter.Die => Die,
            StateCharacter.Parry => Parry,
            StateCharacter.HitStun => HitStun,
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
