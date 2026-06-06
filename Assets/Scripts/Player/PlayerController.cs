using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    public CharacterStats stats;
    public bool IsDead { get; private set; }
    public int PlayerIndex { get; private set; }
    public bool IsParrying { get; set; }
    public float stunTimer;

    // Jump tracking
    public int JumpsRemaining { get; private set; }
    private bool wasGrounded;

    private IInputProvider input;
    private CharacterDeck deck;
    private CharacterParry parry;

    public CharacterCombat Combat { get; private set; }
    public CharacterMovement Movement { get; private set; }
    public CharacterHealth Health { get; private set; }
    public StateMachine stateMachine { get; private set; }
    public SpriteRenderer Sprite { get; private set; }

    public Vector2 MoveInput => input.CurrentDirection;
    public bool JumpPressed =>  input.HasBufferedJump;
    public bool IsGrounded => Movement.IsGrounded;
    public bool AttackInput => input.HasBufferedAttack;

    private void Awake()
    {
        input = GetComponent<IInputProvider>();
        deck = GetComponent<CharacterDeck>();
        parry = GetComponent<CharacterParry>();
        Combat = GetComponent<CharacterCombat>();
        Movement = GetComponent<CharacterMovement>();
        Health = GetComponent<CharacterHealth>();
        Sprite = GetComponentInChildren<SpriteRenderer>();

        stateMachine = new StateMachine();
        stateMachine.Idle = new IdleState(this, stateMachine);
        stateMachine.Move = new MoveState(this, stateMachine);
        stateMachine.Jump = new JumpState(this, stateMachine);
        stateMachine.Crouch = new CrouchState(this, stateMachine);
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

        bool isAI = GetComponent<CharacterAI>() != null && GetComponent<CharacterAI>().enabled;
        PlayerIndex = isAI ? 1 : 0;

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

        // Reset jumps on landing
        bool isNowGrounded = IsGrounded;
        if (isNowGrounded && !wasGrounded)
            ResetJumps();
        wasGrounded = isNowGrounded;

        if (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
            return;
        }

        if (input.HasBufferedParry)
        {
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
            if (input.HasBufferedHand1) { deck.TryUseCardFromHand(0); input.ConsumeHand1(); }
            else if (input.HasBufferedHand2) { deck.TryUseCardFromHand(1); input.ConsumeHand2(); }
            else if (input.HasBufferedHand3) { deck.TryUseCardFromHand(2); input.ConsumeHand3(); }
            else if (input.HasBufferedHand4) { deck.TryUseCardFromHand(3); input.ConsumeHand4(); }
            else if (input.HasBufferedHand5) { deck.TryUseCardFromHand(4); input.ConsumeHand5(); }
        }

        stateMachine.Update();
    }

    private void FixedUpdate()
    {
        if (IsDead || stunTimer > 0) return;
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

    public void ResetJumps() => JumpsRemaining = stats != null ? stats.maxJumps : 1;

    public void EnterDieState() => stateMachine.ChangeState(StateCharacter.Die);
}