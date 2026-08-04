using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    public CharacterStats stats;

    public bool IsDead { get; private set; }
    public int PlayerIndex { get; set; }
    public bool IsParrying { get; set; }
    public bool IsIntangible { get; set; }
    public float stunTimer;

    public int JumpsRemaining { get; private set; }
    private bool wasGrounded;
    public bool controlsEnabled = true;
    public bool cardsEnabled = true;

    private IInputProvider input;
    private CharacterDeck deck;
    private CharacterParry parry;

    public Animator Anim { get; private set; }
    public CharacterCombat Combat { get; private set; }
    public CharacterGrab Grab { get; private set; }
    public CharacterMovement Movement { get; private set; }
    public CharacterHealth Health { get; private set; }
    public CharacterShield Shield { get; private set; }
    public CharacterRoll Roll { get; private set; }
    public CharacterDodge Dodge { get; private set; }
    public CharacterDash Dash { get; private set; }
    public StateMachine stateMachine { get; private set; }
    public SpriteRenderer Sprite { get; private set; }

    public IInputProvider ActiveInput => input;
    public Vector2 MoveInput => input.CurrentDirection;
    public bool JumpPressed => input.HasBufferedJump;
    public bool IsGrounded => Movement.IsGrounded;
    public bool AttackInput => input.HasBufferedAttack;
    public bool GrabInput => input.HasBufferedGrab;
    public bool EvadePressed => input.HasBufferedEvade;
    public bool DashPressed => input.HasBufferedDash;

    private void Awake()
    {
        input = GetComponent<IInputProvider>();
        deck = GetComponent<CharacterDeck>();
        parry = GetComponent<CharacterParry>();
        Combat = GetComponent<CharacterCombat>();
        Grab = GetComponent<CharacterGrab>();
        Movement = GetComponent<CharacterMovement>();
        Health = GetComponent<CharacterHealth>();
        Sprite = GetComponentInChildren<SpriteRenderer>();
        Anim = GetComponentInChildren<Animator>();
        Shield = GetComponent<CharacterShield>();
        Roll = GetComponent<CharacterRoll>();
        Dodge = GetComponent<CharacterDodge>();
        Dash = GetComponent<CharacterDash>();

        if (Shield == null)
            Shield = gameObject.AddComponent<CharacterShield>();

        if (Roll == null)
            Roll = gameObject.AddComponent<CharacterRoll>();

        if (Dodge == null)
            Dodge = gameObject.AddComponent<CharacterDodge>();

        if (Dash == null)
            Dash = gameObject.AddComponent<CharacterDash>();

        Shield.Initialize(Sprite);

        stateMachine = new StateMachine();
        stateMachine.Idle = new IdleState(this, stateMachine);
        stateMachine.Move = new MoveState(this, stateMachine);
        stateMachine.Jump = new JumpState(this, stateMachine);
        stateMachine.Crouch = new CrouchState(this, stateMachine);
        stateMachine.Shield = new ShieldState(this, stateMachine);
        stateMachine.Roll = new RollState(this, stateMachine);
        stateMachine.Dodge = new DodgeState(this, stateMachine);
        stateMachine.Dash = new DashState(this, stateMachine);
        stateMachine.Jab = new JabState(this, stateMachine, stats.jabAttack);
        stateMachine.ForwardTilt = new ForwardTiltState(this, stateMachine, stats.fTiltAttack);
        stateMachine.UpTilt = new UpTiltState(this, stateMachine, stats.upTiltAttack);
        stateMachine.DownTilt = new DownTiltState(this, stateMachine, stats.dTiltAttack);
        stateMachine.NeutralAir = new NeutralAirState(this, stateMachine, stats.neutralAirAttack);
        stateMachine.ForwardAir = new ForwardAirState(this, stateMachine, stats.forwardAirAttack);
        stateMachine.UpAir = new UpAirState(this, stateMachine, stats.upAirAttack);
        stateMachine.DownAir = new DownAirState(this, stateMachine, stats.downAirAttack);
        stateMachine.DashAttack = new DashAttackState(this, stateMachine, stats.dashAttack != null ? stats.dashAttack : stats.fTiltAttack);
        stateMachine.Grab = new NormalGrabState(this, stateMachine, stats.grabStats);
        stateMachine.PivotGrab = new PivotGrabState(this, stateMachine, stats.pivotGrabStats);
        stateMachine.DashGrab = new DashGrabState(this, stateMachine, stats.dashGrabStats != null ? stats.dashGrabStats : stats.grabStats);
        stateMachine.GrabHold = new GrabHoldState(this, stateMachine);
        stateMachine.Pummel = new PummelState(this, stateMachine);
        stateMachine.Throw = new ThrowState(this, stateMachine);
        stateMachine.Card = new CardState(this, stateMachine);
        stateMachine.Die = new DieState(this, stateMachine);
        stateMachine.Parry = new ParryState(this, stateMachine);

        if (GetComponent<IGrabbable>() == null)
            gameObject.AddComponent<CharacterGrabbable>();
    }

    private void Start()
    {
        stateMachine.ChangeState(StateCharacter.Idle);
        ResetJumps();
        wasGrounded = IsGrounded;

        bool isAI = (PlayerIndex == 1);

        if (isAI)
        {
            input = GetComponent<CharacterAI>();
        }
        else
        {
            input = GetComponent<CharacterBrain>();
        }

        if (stats != null && stats.characterIcon != null)
        {
            UIEvents.OnIconSet?.Invoke(PlayerIndex, stats.characterIcon);
        }

        Health.UpdateUI();
        GetComponent<EnergyManager>().UpdateUI();
        deck.UpdateHandUI();
        deck.UpdateDeckCountUI();
    }

    public IState GetCurrentState() => stateMachine.CurrentState;
    public void ChangeState(StateCharacter newState) => stateMachine.ChangeState(newState);

    private void Update()
    {
        if (IsDead) return;

        if (!controlsEnabled) return;

        bool isNowGrounded = IsGrounded;
        if (isNowGrounded && !wasGrounded)
            ResetJumps();
        wasGrounded = isNowGrounded;

        if (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
            return;
        }

        if (input.HasBufferedShield)
            input.ConsumeShield();

        if (input.IsShieldHeld)
        {
            bool canStartShield = Shield.CanActivate && IsGrounded &&
                (stateMachine.CurrentState == stateMachine.Idle ||
                 stateMachine.CurrentState == stateMachine.Move ||
                 stateMachine.CurrentState == stateMachine.Crouch);

            if (canStartShield)
            {
                stateMachine.ChangeState(StateCharacter.Shield);
                return;
            }
        }

        if (MoveInput.x > 0.01f)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (MoveInput.x < -0.01f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        if (TryHandleDashInput())
            return;

        bool isEvading = stateMachine.CurrentState == stateMachine.Roll ||
            stateMachine.CurrentState == stateMachine.Dodge;

        if (!isEvading && input.HasBufferedEvade)
        {
            bool canStartRoll = Roll.CanRoll && IsGrounded &&
                (stateMachine.CurrentState == stateMachine.Idle ||
                 stateMachine.CurrentState == stateMachine.Move ||
                 stateMachine.CurrentState == stateMachine.Crouch ||
                 stateMachine.CurrentState == stateMachine.Jump);

            bool canStartDodge = Dodge.CanDodge && !IsGrounded &&
                stateMachine.CurrentState == stateMachine.Jump;

            if (canStartRoll)
            {
                input.ConsumeEvade();
                stateMachine.ChangeState(StateCharacter.Roll);
                return;
            }

            if (canStartDodge)
            {
                input.ConsumeEvade();
                stateMachine.ChangeState(StateCharacter.Dodge);
                return;
            }
        }

        if (input.HasBufferedParry)
        {
            parry.TryParry();
            input.ConsumeParry();
        }

        bool canUseCards = cardsEnabled && (stateMachine.CurrentState == stateMachine.Idle || stateMachine.CurrentState == stateMachine.Move);

        if (input.HasBufferedDrawCards)
        {
            if (canUseCards) deck.TryDrawNewHand();
            input.ConsumeDrawCards();
        }

        if (canUseCards)
        {
            if (input.HasBufferedHand1) { deck.TryUseCardFromHand(0); input.ConsumeHand1(); }
            else if (input.HasBufferedHand2) { deck.TryUseCardFromHand(1); input.ConsumeHand2(); }
            else if (input.HasBufferedHand3) { deck.TryUseCardFromHand(2); input.ConsumeHand3(); }
            else if (input.HasBufferedHand4) { deck.TryUseCardFromHand(3); input.ConsumeHand4(); }
        }

        stateMachine.Update();
    }

    private void FixedUpdate()
    {
        if (IsDead || stunTimer > 0) return;
        if (!controlsEnabled) return;
        stateMachine.FixedUpdate();
    }
    public void ExecuteCardState(ICardable cardToUse)
    {
        stateMachine.Card.SetCard(cardToUse, 0.5f);
        stateMachine.ChangeState(StateCharacter.Card);
    }

    public void ConsumeJump()
    {
        input.ConsumeJump();
        JumpsRemaining = Mathf.Max(0, JumpsRemaining - 1);
    }

    public void ResetJumps()
    {
        JumpsRemaining = stats != null ? stats.maxJumps : 1;
    }

    public void ConsumeEvadeInput() => input.ConsumeEvade();

    public bool TryPlayAnimation(int stateHash)
    {
        const int baseLayer = 0;

        if (Anim == null || Anim.layerCount <= baseLayer || !Anim.HasState(baseLayer, stateHash))
            return false;

        Anim.Play(stateHash, baseLayer, 0f);
        return true;
    }

    private bool TryHandleDashInput()
    {
        if (!input.HasBufferedDash)
            return false;

        bool canStartFromCurrentState = stateMachine.CurrentState == stateMachine.Idle ||
            stateMachine.CurrentState == stateMachine.Move ||
            stateMachine.CurrentState == stateMachine.Crouch;
        bool hasHorizontalDirection = Mathf.Abs(MoveInput.x) >= stats.tiltThreshold;

        input.ConsumeDash();

        if (!canStartFromCurrentState || !IsGrounded || !hasHorizontalDirection ||
            !Dash.TryStartDash(MoveInput.x))
            return false;

        if (input.HasBufferedAttack)
        {
            input.ConsumeAttack();
            stateMachine.ChangeState(StateCharacter.DashAttack);
            return true;
        }

        if (input.HasBufferedGrab)
        {
            input.ConsumeGrab();
            stateMachine.ChangeState(StateCharacter.DashGrab);
            return true;
        }

        stateMachine.ChangeState(StateCharacter.Dash);
        return true;
    }

    public void EnterDieState() => stateMachine.ChangeState(StateCharacter.Die);
}
