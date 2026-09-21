// Owns one set of state instances per character. CharacterStateMachine runs them.
public sealed class CharacterStates
{
    public IdleState Idle { get; }
    public MoveState Move { get; }
    public JumpState Jump { get; }
    public CrouchState Crouch { get; }
    public ShieldState Shield { get; }
    public RollState Roll { get; }
    public DodgeState Dodge { get; }
    public DashState Dash { get; }

    public NormalAttackState Jab { get; }
    public NormalAttackState ForwardTilt { get; }
    public NormalAttackState UpTilt { get; }
    public NormalAttackState DownTilt { get; }
    public NormalAttackState NeutralAir { get; }
    public NormalAttackState ForwardAir { get; }
    public NormalAttackState UpAir { get; }
    public NormalAttackState DownAir { get; }

    public HeavyChargeState HeavyCharge { get; }
    public HeavyAttackState HeavyAttack { get; }
    public DashAttackState DashAttack { get; }

    public NormalGrabState Grab { get; }
    public PivotGrabState PivotGrab { get; }
    public DashGrabState DashGrab { get; }
    public GrabHoldState GrabHold { get; }
    public PummelState Pummel { get; }
    public ThrowState Throw { get; }
    public CardState Card { get; }
    public DieState Die { get; }
    public ParryState Parry { get; }
    public HitStunState HitStun { get; }

    public CharacterStates(CharacterCoordinator character, CharacterStateMachine machine)
    {
        CharacterStats stats = character.Stats;
        Idle = new IdleState(character, machine);
        Move = new MoveState(character, machine);
        Jump = new JumpState(character, machine);
        Crouch = new CrouchState(character, machine);
        Shield = new ShieldState(character, machine);
        Roll = new RollState(character, machine);
        Dodge = new DodgeState(character, machine);
        Dash = new DashState(character, machine);

        Jab = new NormalAttackState(character, machine, NormalAttackType.Jab, stats.jabAttack, "Jab", false);
        ForwardTilt = new NormalAttackState(character, machine, NormalAttackType.ForwardTilt, stats.fTiltAttack, "FTilt", false);
        UpTilt = new NormalAttackState(character, machine, NormalAttackType.UpTilt, stats.upTiltAttack, "UpTilt", false);
        DownTilt = new NormalAttackState(character, machine, NormalAttackType.DownTilt, stats.dTiltAttack, "DownTilt", false);
        NeutralAir = new NormalAttackState(character, machine, NormalAttackType.NeutralAir, stats.neutralAirAttack, "Jab", true);
        ForwardAir = new NormalAttackState(character, machine, NormalAttackType.ForwardAir, stats.forwardAirAttack, "Jab", true);
        UpAir = new NormalAttackState(character, machine, NormalAttackType.UpAir, stats.upAirAttack, "Jab", true);
        DownAir = new NormalAttackState(character, machine, NormalAttackType.DownAir, stats.downAirAttack, "Jab", true);

        HeavyCharge = new HeavyChargeState(character, machine);
        HeavyAttack = new HeavyAttackState(character, machine);
        DashAttack = new DashAttackState(character, machine, stats.dashAttack != null ? stats.dashAttack : stats.fTiltAttack);

        Grab = new NormalGrabState(character, machine, stats.grabStats);
        PivotGrab = new PivotGrabState(character, machine, stats.pivotGrabStats);
        DashGrab = new DashGrabState(character, machine, stats.dashGrabStats != null ? stats.dashGrabStats : stats.grabStats);
        GrabHold = new GrabHoldState(character, machine);
        Pummel = new PummelState(character, machine);
        Throw = new ThrowState(character, machine);
        Card = new CardState(character, machine);
        Die = new DieState(character, machine);
        Parry = new ParryState(character, machine);
        HitStun = new HitStunState(character, machine);
    }
}
