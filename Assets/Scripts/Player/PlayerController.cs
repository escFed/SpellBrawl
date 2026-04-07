using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private PlayerStats stats;

    [Header("Cards")]
    public GameObject[] cardSlots = new GameObject[4];

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
    public DownTiltState DownTiltState { get; private set; }
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
        DownTiltState = new DownTiltState(this, stateMachine);
        DieState = new DieState(this, stateMachine);
        CardState = new CardState(this, stateMachine);
    }

    private void Start()
    {
        stateMachine.Initialize(IdleState);

        PlayerInput playerInput = GetComponent<PlayerInput>();
        PlayerIndex = playerInput != null ? playerInput.playerIndex : 1;
        PlayerHealth health = GetComponent<PlayerHealth>();

        if (UIManager.Instance != null)
        {
            if (PlayerIndex == 0 && health != null) health.SetDamageText(UIManager.Instance.p1_damageText);
            else if (PlayerIndex == 1 && health != null) health.SetDamageText(UIManager.Instance.p2_damageText);

            Image[] UISlots = (PlayerIndex == 0) ? UIManager.Instance.p1_cards : UIManager.Instance.p2_cards;

            for (int i = 0; i < cardSlots.Length; i++)
            {
                if (cardSlots[i] != null && i < UISlots.Length)
                {
                    ICardable genericCard = cardSlots[i].GetComponent<ICardable>();
                    if (genericCard != null)
                    {
                        genericCard.SetUI(UISlots[i]);
                    }
                }
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

        if (input.HasBufferedSpecial)
        {
            int slotToUse = ResolveCardSlot();
            TryUseCard(slotToUse);
            input.ConsumeSpecial();
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

    private int ResolveCardSlot()
    {
        Vector2 dir = input.CurrentDirection;
        float deadzone = 0.3f;

        if (dir.magnitude < deadzone) return 0;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            return 1;
        }
        else
        {
            if (dir.y > 0) return 2;
            else return 3;
        }
    }

    public void TryUseCard(int slotIndex)
    {
        if (stateMachine.CurrentState != IdleState && stateMachine.CurrentState != MoveState) return;

        if (slotIndex >= 0 && slotIndex < cardSlots.Length && cardSlots[slotIndex] != null)
        {
            ICardable cardToUse = cardSlots[slotIndex].GetComponent<ICardable>();

            if (cardToUse != null)
            {
                CardState.SetCard(cardToUse, 0.5f);
                stateMachine.ChangeState(CardState);
            }
        }
    }

    public IState ResolveAttackState()
    {
        Vector2 dir = input.CurrentDirection;
        bool hasHorizontal = Mathf.Abs(dir.x) >= stats.tiltThreshold;
        bool hasUp = dir.y >= stats.tiltThreshold;

        bool hasDown = dir.y <= -stats.tiltThreshold;

        input.ConsumeAttack();

        if (hasUp && (!hasHorizontal || dir.y >= Mathf.Abs(dir.x))) return UpTiltState;

        if (hasDown && (!hasHorizontal || Mathf.Abs(dir.y) >= Mathf.Abs(dir.x))) return DownTiltState;

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
    public void OpenDTiltHitbox() => HitBox.SetDTiltHitbox(true);
    public void CloseDTiltHitbox() => HitBox.SetDTiltHitbox(false);

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