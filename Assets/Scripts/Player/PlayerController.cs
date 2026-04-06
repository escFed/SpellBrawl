using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private PlayerStats stats;

    [Header("Cards")]
    [SerializeField] private GameObject slotCard;
    [SerializeField] private GameObject slotCard1;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    private IInputProvider input;
    private Rigidbody2D rb;
    private StateMachine stateMachine;
    private PlayerHitBox HitBox;

    public IdleState IdleState { get; private set; }
    public MoveState MoveState { get; private set; }
    public JumpState JumpState { get; private set; }
    public JabState JabState { get; private set; }
    public ForwardTiltState ForwardTiltState { get; private set; }
    public UpTiltState UpTiltState { get; private set; }
    public CardState CardState { get; private set; }
    public DieState DieState { get; private set; }

    public Vector2 MoveInput => input.CurrentDirection;
    public bool JumpPressed =>  input.HasBufferedJump;
    public bool AttackInput => input.HasBufferedAttack;
    public bool IsGrounded { get; private set; }
    public bool IsDead { get; private set; }
    public int PlayerIndex { get; private set; }

    public float stunTimer;
    public Transform throwPoint;

    private void Awake()
    {
        input = GetComponent<IInputProvider>();
        HitBox = GetComponent<PlayerHitBox>();
        rb = GetComponent<Rigidbody2D>();

        stateMachine = new StateMachine();
        IdleState = new IdleState(this, stateMachine);
        MoveState = new MoveState(this, stateMachine);
        JumpState = new JumpState(this, stateMachine);
        JabState = new JabState(this, stateMachine);
        ForwardTiltState = new ForwardTiltState(this, stateMachine);
        UpTiltState = new UpTiltState(this, stateMachine);
        DieState = new DieState(this, stateMachine);
        CardState = new CardState(this, stateMachine);
    }

    private void Start()
    {
        stateMachine.Initialize(IdleState);

        PlayerInput playerInput = GetComponent<PlayerInput>();
        PlayerIndex = playerInput != null ? playerInput.playerIndex : 1;
        PlayerHealth health = GetComponent<PlayerHealth>();

        FireBallCard fireCard = slotCard != null ? slotCard.GetComponent<FireBallCard>() : null;
        ThunderStrikeCard thunderCard = slotCard1 != null ? slotCard1.GetComponent<ThunderStrikeCard>() : null;

        if (UIManager.Instance != null)
        {
            if (PlayerIndex == 0)
            {
                if (health != null) health.SetDamageText(UIManager.Instance.p1_damageText);
                if (fireCard != null) fireCard.SetUI(UIManager.Instance.p1_fireCard);
                if (thunderCard != null) thunderCard.SetUI(UIManager.Instance.p1_thunderCard);
            }
            else if (PlayerIndex == 1)
            {
                if (health != null) health.SetDamageText(UIManager.Instance.p2_damageText);
                if (fireCard != null) fireCard.SetUI(UIManager.Instance.p2_fireCard);
                if (thunderCard != null) thunderCard.SetUI(UIManager.Instance.p2_thunderCard);
            }
        }
    }

    private void Update()
    {
        if (IsDead) return;
        IsGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
            return;
        }

        if (input.HasBufferedFire)
        {
            TryUseCard(1);
            input.ConsumeFire();
        }

        if (input.HasBufferedThunder)
        {
            TryUseCard(2);
            input.ConsumeThunder();
        }   

        stateMachine.Update();
    }

    private void FixedUpdate()
    {
        if (IsDead) return;
        if (stunTimer > 0) return;

        stateMachine.FixedUpdate();
    }

    public void TakeHit(float stunDuration)
    {
        stunTimer = stunDuration;
        input.ClearAllInputs();
        stateMachine.ChangeState(IdleState);
    }

    public void TryUseCard(int slotIndex)
    {

        if (stateMachine.CurrentState != IdleState && stateMachine.CurrentState != MoveState)
            return;

        ICardable cardToUse = null;

        if (slotIndex == 1 && slotCard != null)
            cardToUse = slotCard.GetComponent<ICardable>();
        else if (slotIndex == 2 && slotCard1 != null)
            cardToUse = slotCard1.GetComponent<ICardable>();

        if (cardToUse != null)
        {
            CardState.SetCard(cardToUse, 0.5f);
            stateMachine.ChangeState(CardState);
        }
    }

    public IState ResolveAttackState()
    {
        Vector2 dir = input.CurrentDirection;
        bool hasHorizontal = Mathf.Abs(dir.x) >= stats.tiltThreshold;
        bool hasUp = dir.y >= stats.tiltThreshold;

        input.ConsumeAttack();

        if (hasUp && (!hasHorizontal || dir.y >= Mathf.Abs(dir.x))) return UpTiltState;
        if (hasHorizontal) return ForwardTiltState;
        return JabState;
    }

    public void ConsumeJump() => input.ConsumeJump();

    public void ApplyHorizontalMovement()
    {
        rb.linearVelocity = new Vector2(MoveInput.x * stats.moveSpeed, rb.linearVelocity.y);

        HitBox.CheckAndFlip(MoveInput.x);
    }

    public void StopHorizontalMovement() => rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

    public void ApplyJumpForce() => rb.linearVelocity = new Vector2(rb.linearVelocity.x, stats.jumpForce);

    public void OpenJabHitbox() => HitBox.SetJabHitbox(true);
    public void CloseJabHitbox() => HitBox.SetJabHitbox(false);
    public void OpenFTiltHitbox() => HitBox.SetFTiltHitbox(true);
    public void CloseFTiltHitbox() => HitBox.SetFTiltHitbox(false);
    public void OpenUTiltHitbox() => HitBox.SetUTiltHitbox(true);
    public void CloseUTiltHitbox() => HitBox.SetUTiltHitbox(false);

    public void OnDeath()
    {
        IsDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
    }

    public void Respawn(Vector3 position)
    {
        IsDead = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        transform.position = position;
        input.ClearAllInputs();
        stateMachine.ChangeState(IdleState);
    }

}