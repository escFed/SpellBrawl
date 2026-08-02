using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    public CharacterStats stats;
    public bool IsDead { get; private set; }
    public int PlayerIndex { get; set; }
    public bool IsParrying { get; set; }
    public bool IsShielding { get; set; }
    public bool IsIntangible { get; set; }
    public bool HasUsedAirDodge { get; private set; }
    public float stunTimer;

    public int JumpsRemaining { get; private set; }
    private bool wasGrounded;
    public bool controlsEnabled = true;

    private IInputProvider input;
    private CharacterDeck deck;
    private CharacterParry parry;

    public Animator Anim { get; private set; }
    public CharacterCombat Combat { get; private set; }
    public CharacterMovement Movement { get; private set; }
    public CharacterHealth Health { get; private set; }
    public StateMachine stateMachine { get; private set; }
    public SpriteRenderer Sprite { get; private set; }

    public IInputProvider ActiveInput => input;
    public Vector2 MoveInput => input.CurrentDirection;
    public bool JumpPressed =>  input.HasBufferedJump;
    public bool IsGrounded => Movement.IsGrounded;
    public bool AttackInput => input.HasBufferedAttack;
    private CardAnimationsUI cardUI;
    private void Awake()
    {
        input = GetComponent<IInputProvider>();
        deck = GetComponent<CharacterDeck>();
        parry = GetComponent<CharacterParry>();
        Combat = GetComponent<CharacterCombat>();
        Movement = GetComponent<CharacterMovement>();
        Health = GetComponent<CharacterHealth>();
        Sprite = GetComponentInChildren<SpriteRenderer>();
        Anim = GetComponentInChildren<Animator>();
        
        stateMachine = new StateMachine();
        stateMachine.Idle = new IdleState(this, stateMachine);
        stateMachine.Move = new MoveState(this, stateMachine);
        stateMachine.Jump = new JumpState(this, stateMachine);
        stateMachine.Crouch = new CrouchState(this, stateMachine);
        //stateMachine.Shield = new ShieldState(this, stateMachine);
        stateMachine.Dodge = new DodgeState(this, stateMachine);
        stateMachine.AirDodge = new AirDodgeState(this, stateMachine);
        stateMachine.Jab = new JabState(this, stateMachine, stats.jabAttack);
        stateMachine.ForwardTilt = new ForwardTiltState(this, stateMachine, stats.fTiltAttack);
        stateMachine.UpTilt = new UpTiltState(this, stateMachine, stats.upTiltAttack);
        stateMachine.DownTilt = new DownTiltState(this, stateMachine, stats.dTiltAttack);
        stateMachine.Card = new CardState(this, stateMachine);
        stateMachine.Die = new DieState(this, stateMachine);
        stateMachine.Parry = new ParryState(this, stateMachine);
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

        cardUI = FindFirstObjectByType<CardAnimationsUI>();
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

        if (MoveInput.x > 0.01f)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (MoveInput.x < -0.01f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        if (input.HasBufferedParry)
        {
            // Tap J in the air → air dodge (one use per flight)
            if (!IsGrounded && !HasUsedAirDodge && stateMachine.CurrentState == stateMachine.Jump)
                stateMachine.ChangeState(StateCharacter.AirDodge);
            else
                parry.TryParry();

            input.ConsumeParry();
        }

        bool canUseCards = (stateMachine.CurrentState == stateMachine.Idle || stateMachine.CurrentState == stateMachine.Move);

        if (input.HasBufferedDrawCards)
        {
            if (canUseCards) deck.TryDrawNewHand();
            input.ConsumeDrawCards();
        }

        if (canUseCards)
        {
            if (input.HasBufferedHand1) { deck.TryUseCardFromHand(0); cardUI.OnCardInteraction(0); input.ConsumeHand1(); }
            else if (input.HasBufferedHand2) { deck.TryUseCardFromHand(1); cardUI.OnCardInteraction(1); input.ConsumeHand2(); }
            else if (input.HasBufferedHand3) { deck.TryUseCardFromHand(2); cardUI.OnCardInteraction(2); input.ConsumeHand3(); }
            else if (input.HasBufferedHand4) { deck.TryUseCardFromHand(3); cardUI.OnCardInteraction(3);  input.ConsumeHand4(); }
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
        HasUsedAirDodge = false;
    }

    public void UseAirDodge() => HasUsedAirDodge = true;

    public void EnterDieState() => stateMachine.ChangeState(StateCharacter.Die);
}